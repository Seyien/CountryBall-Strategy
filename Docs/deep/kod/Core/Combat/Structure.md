# Structure

> **Kaynak:** `Assets/Game/Core/Combat/Structure.cs`
> **Ad alanı:** `GridStrategy.Combat` · **Assembly:** `GridStrategy.Combat` (`noEngineReferences: true`)
> **Rol:** Bileşik (Aggregate) — kimliği var, hafızası var, parçalar **arasındaki** kuralı yürütür

Bir yapının bütünü: canı, yaşam döngüsü, tarafı ve (varsa) saldırı tanımı. Var
olma sebebi `Combatant` ile aynı cümledir: `Health` canın bittiğini bilir ama
`StructureLifecycle`'ı tanımaz; `StructureLifecycle` yıkımı bilir ama canı
tanımaz. İkisini birden tanıyan tek yer burasıdır.

`Combatant`'ın kopyası değildir ve ondan **türemez**. Ortak olan tek şey
`Health`'tir — ve bu bir tesadüf değil, bu tipin varlığıyla sınanan iddiadır: can
kuralı tipten bağımsızsa, bir barakanın canı bir askerin canıyla aynı sınıfla
tutulabilmelidir.

| Üye | Karar | Detay |
|---|---|---|
| `sealed class Structure` | `: Combatant` reddedildi — geçiş grafiği farklı | [↓](#sealed-class-structure) |
| `Structure(...)` | saldırı tanımı isteğe bağlı: kural "yapı saldırmaz" | [↓](#structure) |
| `Team` | `Team.None` bilerek geçerli; taraf sonradan değişmez | [↓](#team) |
| `AttackProfile` | saldırmayan yapılarda `null` | [↓](#attackprofile) |
| `CanAttack` | aynı null kontrolü üç çağıranda doğmasın | [↓](#canattack) |
| `State` | durum yaşam döngüsünden okunur | [↓](#state) |
| `IsStanding` | tek kaynağa iner ama DOĞRU kaynağa | [↓](#isstanding) |
| `TakeDamage(int)` | sayı ile alan yargısı bilerek ayrı | [↓](#takedamageint-amount) |
| `TryRepair(int)` | kelepçe burada, yaşam döngüsünde değil | [↓](#tryrepairint-amount) |
| `Tick(float)` | zaman dışarıdan gelir | [↓](#tickfloat-deltaseconds) |

**İlgili anlatılar:** [05-yaşam döngüsü](../../../konular/05-yasam-dongusu.md) ·
[02-assembly duvarı](../../../konular/02-assembly-duvari.md)

---

## sealed class Structure

**KALITIM AYNI PARÇALAR DEĞİL, AYNI YAŞAM DÖNGÜSÜ DEMEKTİR.**

### HARİTA: `: Combatant` yazan satır neyi getirirdi

Kalıtım SEÇMELİ değildir: iki nokta üst üste, tabanın bütün üyelerini birden alır.
Satır satır sayıldığında üçü uyuyor, dördü kırılıyor:

```
  Combatant'ın üyesi        baraka için anlamı           sonuç
  ─────────────────────────────────────────────────────────────────
  Health                    canı var                     ✓ uyar
  Team                      tarafı var                   ✓ uyar
  TakeDamage(int)           hasar alır                   ✓ uyar
  ─────────────────────────────────────────────────────────────────
  TryRevive()               bina "diriltilebilir" olur   ✗ >> KIRILIR <<
  State : UnitState         binaya Downed hâli gelir     ✗ >> KIRILIR <<
  AttackProfile (zorunlu)   saldırmayan depo sahte
                            profil uydurur               ✗ >> KIRILIR <<
  UnitLifecycle             10 saniyelik kurtarma
                            penceresi binaya taşınır     ✗ >> KIRILIR <<

  SEÇİLEN — ortak olan şey TABAN değil, PARÇANIN SINIFI
    ┌──── Combatant ────┐              ┌──── Structure ────┐
    │ health ───────────┼──┐        ┌──┼─ health           │
    │ lifecycle ─► UnitLifecycle  StructureLifecycle ◄─────┤
    └───────────────────┘  │        │  └───────────────────┘
                           ▼        ▼
                     ╔═════════╗ ╔═════════╗
                     ║ Health  ║ ║ Health  ║
                     ║ nesnesi ║ ║ nesnesi ║
                     ╚═════════╝ ╚═════════╝
          ◄── >> AYNI SINIF, AYRI NESNE — ortak REFERANS YOK <<
```

### KAPSAM: paylaşım yasağı DEĞİL, geçiş grafiği testi

Ayırt edici soru: **iki tip aynı PARÇALARA mı sahip, aynı GEÇİŞLERDEN mi geçiyor?**
Yalnız ikincisi ortak tabanı hak eder.

Karşı örnek aynı dosyada, iki alan aşağıda: `private readonly Health health` —
`Health` sınıfı bu iki tip arasında GERÇEKTEN paylaşılıyor ve doğru olan da bu.
`Health`'in bir geçiş grafiği yok, yalnız bir sayısı var ve sayının "düşmüş" hâli
olmaz. Yani baraka ile asker arasındaki sınır parçalarda değil, yaşam
döngüsündedir; aynı ayrım [StructureState.md](StructureState.md#enum-structurestate)'de
iki grafik çizilerek gösterilmiş durumda.

### İŞ BÖLÜMÜ: ayrılığı İKİ tip birden taşıyor

```
StructureLifecycle   ► geçişleri ayırır (kurtarma penceresi YOK)
StructureState       ► durum kümesini ayırır (Downed YOK)
```

`StructureLifecycle` silinip `UnitLifecycle` kullanılsaydı bina düşer, on saniye
bekler ve diriltilebilir olurdu. `StructureState` silinip `UnitState` kullanılsaydı
geçişler doğru kalır ama her switch asla çalışmayan bir `Downed` dalı taşırdı.
İkisi aynı ayrımın farklı yarısı: biri OKLARI, diğeri DÜĞÜMLERİ ayırıyor.

### `sealed` bu kırılmaya karşı sıfır koruma sağlar

`sealed` yalnızca BU tipten türemeyi keser; bu tipin BAŞKASINDAN türemesini
engellemez:

```csharp
public sealed class Structure : Combatant   // ✓ tamamen geçerli, derlenir
public sealed class Tower : Structure       // ✗ derleme hatası
```

Yani yukarıdaki kırılmayı önleyen şey `sealed` değil, o satırın hiç yazılmamış
olmasıdır — ve onu yazılmamış tutan tek şey bu karardır.

### KAZANIRDI'nın sınırı

Aşağıdaki KAZANIRDI koşulu gerçekleşirse ortak taban yine ilk cevap olmaz: üç
dosyanın üç kopya kural taşıması sorunu, ortak TABANLA da ortak PARÇAYLA da
çözülebilir. Kalıtımı hak ettiren şey kuralın tekrarlanması değil, geçiş
grafiğinin AYNI olmasıdır.

### REDDEDILEN

```csharp
public sealed class Structure : Combatant
```

**KIRILAN:** baraka, askerin yaşam döngüsünü DEVRALIR ve hiçbiri ona uymaz.

```
TryRevive() devralınır  -> bina "diriltilebilir" olur
AttackProfile zorunlu   -> saldırmayan depo sahte profil uydurur
State artık UnitState   -> binaya hiç olmayacak Downed hâli gelir
derleyici: hiçbir şey der  ·  test: Repair_AfterDestruction
_IsRejected'in koruduğu ayrım ORTADAN KALKAR
```

**KAZANIRDI:** her bina düşüp kurtarılma penceresi açsaydı ve her binanın savunma
ateşi olsaydı — o gün üç dosya üç kopya kural olurdu.

**TEK CUMLE:** Kalıtım "aynı parçalara sahip" demek değil, "aynı yaşam
döngüsünden geçer" demektir; baraka geçmiyor.

---

## Structure(...)

**İSTEĞE BAĞLI PARAMETRE KURALI YAZDIRIR, ZORUNLU OLAN İSTİSNAYI.**

Kurucunun `attackProfile` parametresi saldırmayan yapılar için `null`. Kural olan
davranış "saldırmaz"dır; isteğe bağlı parametre, kuralı değil İSTİSNAyı yazdırır.

### HARİTA: zorunlu imzada her çağıranın yazmak zorunda kalacağı

```
  yapı türü        saldırır mı   zorunlu imzada ne yazılırdı
  ──────────────────────────────────────────────────────────────
  depo             hayır         new AttackProfile(0, 1)  ✗ YALAN
  duvar            hayır         new AttackProfile(0, 1)  ✗ YALAN
  kapı             hayır         new AttackProfile(0, 1)  ✗ YALAN
  savunma kulesi   evet          gerçek profil            ✓
  ──────────────────────────────────────────────────────────────
  ÇOĞUNLUK saldırmıyor   ◄── >> KURAL BU SATIRDA <<
```

Menzilin neden `0` değil `1` yazıldığına dikkat: `AttackProfile` kurucusu
`range < 1` için patlıyor (gerekçesi orada yazılı), yani "saldıramaz"ı ifade eden
dürüst bir profil YAZILAMIYOR. Uydurulan her profil komşu hücreye ulaştığını
SÖYLER; `damage` 0 olduğu için ulaşmaz — ama bunu bilen tek şey çağıranın
hafızasıdır.

```
┌─ zorunlu ─┐  her çağıran istisnayı yazar, kural görünmez
└─ isteğe bağlı ─┐  yalnız kule yazar, kural imzada okunur ◄── SEÇİLEN
```

### KAPSAM: "her parametreyi isteğe bağlı yap" DEĞİL

Karşı örnek aynı kurucuda, iki satır yukarıda: `health` ve `lifecycle`
ZORUNLUdur ve öyle kalmalıdır — üstelik `null` gelirlerse `ArgumentNullException`
atılıyor. Sebebi tam olarak bu kararın tersi: canı olmayan ya da yaşam döngüsü
olmayan bir yapı diye bir şey YOK, o yüzden "yok" hâli bir istisna değil bir
hatadır.

Ayırt edici soru: **bu parçanın YOKLUĞU meşru bir yapı türü tarif ediyor mu?**

### İŞ BÖLÜMÜ: varsayılan ile `CanAttack` örtüşmez, bölüşür

```
`attackProfile = null` ► KURULUM tarafı: saldırmayan yapı hiçbir
                         şey uydurmadan doğar
`CanAttack => AttackProfile != null` ► OKUMA tarafı: aynı null
                         kontrolü üç çağıranda üç kez doğmaz
```

Varsayılan silinirse yukarıdaki üç yalan satırı geri gelir. `CanAttack` silinirse
yapı doğru kurulur ama her çağıran `!= null` yazar ve o kontrol dağılır. Biri
`null`'ın DOĞMASINI meşrulaştırıyor, diğeri `null`'ın OKUNMASINI tek yere
topluyor.

### KURUCU HER PARÇAYI DOĞRULAMAZ

Sözü edilen iki satır ve onlara katılmayan üçüncü parça, kurucunun içinde şöyle
duruyor:

```csharp
this.health = health ?? throw new ArgumentNullException(nameof(health));
this.lifecycle = lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));

Team = team;

AttackProfile = attackProfile;   // ◄── ÜÇÜNCÜ PARÇA: `?? throw` YOK
```

Aşağıdaki **REDDEDILEN** bloğunda da bir `?? throw` satırı görünür ve o satır tam
olarak `attackProfile` içindir: yani orada okunan kod, bu kurucuda bilerek
YAZILMAMIŞ olan satırdır — reddedilen dünyanın kodu, seçilen dünyanınki değil.

Okuyucu üstteki iki `?? throw` satırına bakıp "bu kurucu parçaları doğruluyor" diye
kredi verebilir; üç parametrenin yalnız ikisi doğrulanıyor. `AttackProfile` için
bilerek `throw` YOK — `null` burada bir hata değil, tipin kendisi. Doğrulamanın
yokluğu da bu kararın parçası.

### REDDEDILEN

Saldırı tanımı zorunlu olur, `Combatant`'taki gibi:

```csharp
public Structure(Health health, StructureLifecycle lifecycle, Team team, AttackProfile attackProfile)
{
    AttackProfile = attackProfile ?? throw new ArgumentNullException(nameof(attackProfile));
}
```

**KIRILAN:** saldırmayan her depo ve duvar kendine sahte bir profil uydurur.

```
damage: 0 yazılır      -> "saldıramaz"ın TİPSİZ işaretçisi doğar
"menzil en az 1" kuralı -> uydurulan profil bir YALANA döner
derleyici: hiçbir şey der  ·  test:
Structure_WithoutAttackProfile_CannotAttack derlenemez
```

**KAZANIRDI:** iki ayrı tip (`Tower` / `Building`) yazılsaydı ve kuleler saldırıya
özgü DURUM taşısaydı — bekleme süresi, cephane, hedef hafızası.

**TEK CUMLE:** İsteğe bağlı parametre KURALI yazdırır, zorunlu parametre
İSTİSNAyı; burada kural "yapı saldırmaz"dır.

---

## Team

Yapının tarafı. `Team.None` tarafsız yapıları anlatır.

**`Team.None` bilerek geçerli:** tarafsız yıkılabilir duvar, kapı ya da engel
gerçek bir yapıdır. Kurucuya bir doğrulama koysaydık `Team`'in sıfırıncı değerinin
var olma sebebini burada iptal etmiş olurduk — o değerin gerekçesi
[Team.md](Team.md#none)'de.

**Takım sonradan değişmez.** Ele geçirilebilir bina istenirse bu bir kural
değişikliğidir (kim, ne kadar sürede, hangi mesafeden) ve o kuralın sahibi bu tip
olmayabilir; bugün `readonly` kalması, o kararın bir setter'ın içinde sessizce
verilmesini engelliyor. Aynı kararın `Combatant` tarafındaki uzun gerekçesi
[Combatant.md](Combatant.md#team)'de.

---

## AttackProfile

Saldırı tanımı; saldırmayan yapılarda `null`. `null` olmasının neden bir hata
değil tipin kendisi olduğu [↑ Structure(...)](#structure) başlığında.

---

## CanAttack

`AttackProfile != null`. Çağıranın bu karşılaştırmayı kendi yazmasını engellemek
için var: aynı `null` kontrolü üç çağıranda üç kez doğmasın.

Bu bir türevdir ve doğrudur — türetildiği şey bir kural değil, o nesnenin kendi
VERİSİdir. Veri değişince türev değişmeli zaten; kural değişince türev
DEĞİŞMEMELİ. Ayrımın uzun hâli
[MovementRules.md](MovementRules.md#canmoveunitstate-state)'de.

---

## State

`lifecycle.State`. Düpedüz bir aktarım: okuma biter bitmez geriye hiçbir bağ
kalmaz, çağıran eline bir KOPYA alır. Aynı sınıftaki `CurrentHealth`,
`RemainingSeconds` ve `IsReadyForCleanup` de öyle.

---

## IsStanding

**TEK KAYNAĞA İNMEK DOĞRUDUR; DOĞRU KAYNAĞA İNMEK ŞARTTIR.**

Yapı ayakta mı. Bu, `Health`'in cevaplayamayacağı sorudur: can bir SAYIdır, ayakta
olmak bir ALAN yargısıdır. Alan yargısının sahibi, alanı bilen taraftır — burada
`StructureLifecycle`.

### HARİTA: "ayakta mı" sorusu hangi oku takip ediyor

```
  SEÇİLEN
    health.Current  ──► CurrentHealth        (SAYI)
    lifecycle.State ──► >> IsStanding <<     (ALAN YARGISI)
                        ◄── iki soru, İKİ kaynak, doğru eşleme

  REDDEDILEN — tek kaynak CAN olursa
    health.Current  ──┬─► CurrentHealth
                      └─► IsStanding   ◄── >> ALAN YARGISI SAYIYA
                                            BAĞLANDI <<
    lifecycle.State ──► (artık kimse sormuyor)

    zincir:
      TryRepair'in kelepçesi `!IsStanding` = `!HasRemaining` olur
        -> kelepçe KENDİ koruduğu şeyi soruyor
      yıkık binanın canı iyileştirilir -> bina kendiliğinden kalkar
      moloz sayacı hâlâ State'e bağlı -> sayı ile durum ayrışır
    ÖLÇÜLDÜ (yalıtılmış koşu): 41 testin 40'ı YEŞİL kalır
      ◄── >> TEK KIRMIZI: DestroyedStructure_HealthItselfStillHeals <<
```

Yani bu kırılmayı gösteren şey test takımı değil, tek bir test.

### KAPSAM: tek kaynak ilkesi DOĞRU, kırılan şey SEÇİM

Karşı örnek aynı dosyada, hemen aşağıda:

```csharp
public int CurrentHealth => health.Current;
```

O da tek kaynağa iniyor ve hiçbir blok gerektirmiyor — çünkü sorunun cinsi ile
kaynağın cinsi örtüşüyor: sayı sorusu, sayı kaynağı. Bu karar "türetme yapma"
demiyor; **"sorunun cinsini kaynağın cinsine uydur"** diyor.

### İŞ BÖLÜMÜ: sayı ile durum örtüşmez, bölüşür

```
Health.HasRemaining     ► "canı kaldı mı"  — SAYI hakkında
IsStanding (State)      ► "ayakta mı"      — ALAN yargısı
TryRepair'in kelepçesi  ► ikisini aynı anda gören tek yerde durur
```

`HasRemaining` silinirse `TakeDamage` yaşam döngüsüne ne zaman haber vereceğini
bilemez. `IsStanding` durumdan koparılırsa yukarıdaki zincir başlar. Kelepçe
silinirse yıkık binaya can basılır. Üçü aynı gerçeği üç kez söylemiyor: biri
sayıyı, biri alanı, biri de aralarındaki kuralı taşıyor. Aynı ayrımın uzun hâli
[Health.md](Health.md#hasremaining)'de, uygulaması da
[StructureLifecycle.md](StructureLifecycle.md#tryrepair)'de.

### `=>` (ifade gövdeli property) tek kaynağı garanti etmez

Okuyucu korumayı sözdizimine yazabilir: "alan yok, demek ki ikinci bir gerçek
yok". `=>` yalnızca burada SAKLANAN ikinci bir değer olmadığını söyler; ikinci
gerçek depolamada değil, KAYNAK SEÇİMİNDE doğar. Reddedilen satır da tam olarak
bir `=>` idi ve hiçbir alan eklemiyordu.

### REDDEDILEN

Tek kaynak canın kendisi olsun:

```csharp
public bool IsStanding => health.HasRemaining;
```

**KIRILAN:** ad tek kaynağa iner ama YANLIŞ kaynağa; 41 testin 40'ı yeşil kalır
(ÖLÇÜLDÜ, yalıtılmış koşu).

```
yıkık binanın canı iyileştirilir -> bina kendiliğinden ayağa kalkar
TryRepair'in kelepçesi artık kendi koruduğu şeyi sorar
moloz sayacı hâlâ duruma bağlı -> sayı ile durum sessizce ayrışır
derleyici: hiçbir şey der  ·  test: DestroyedStructure_HealthItselfStillHeals
```

**KAZANIRDI:** yıkım geri sayımı, moloz penceresi ve onarım yasağı hiç olmasaydı —
o gün `StructureLifecycle` fazlalık, tek kaynak can olurdu.

**TEK CUMLE:** Can bir SAYIdır, ayakta olmak bir ALAN yargısıdır; tek kaynağa
inmek doğrudur ama kaynağın DOĞRU olanı seçilmelidir.

---

## TakeDamage(int amount)

Hasar uygular ve gerekiyorsa yaşam döngüsüne haber verir. Yapı BU vuruşla
yıkıldıysa `true` döner — cevabın neden burada değil `StructureLifecycle`'da
üretildiği [StructureLifecycle.md](StructureLifecycle.md#onhealthdepleted)'de.

`IsStanding` neden ayrı yaşıyor: alan yargısı, alanı BİLEN sahibe ait. Burası canı
ve yaşam döngüsünü aynı anda gören tek yer — ve tam bu yüzden ikisini birleştirmek
en kolay göründüğü yer. `HasRemaining` sayıyı söyler, `IsStanding` durumu; ikisi
bilerek ayrı kalır — gerekçe [↑ IsStanding](#isstanding) başlığındaki REDDEDILEN
bölümünde.

---

## TryRepair(int amount)

Ayakta olan yapıyı onarır. **Onarım diriltme değildir:** yalnızca canı artırır,
durumu değiştirmez ve yıkılmış bir yapıda çalışmaz. Yıkık bina onarılmaz — yeniden
inşa edilir, ki o da yeni bir `Structure` nesnesidir.

**Kelepçe burada, `StructureLifecycle`'da değil:** yıkık bir yapıyı ayağa
kaldırmanın yasak olduğu tek yer can ile durumu AYNI ANDA gören yerdir. Yaşam
döngüsü tek başına izin verseydi sıfır canla ayakta duran bir bina üretirdi —
gerekçenin tamamı [StructureLifecycle.md](StructureLifecycle.md#tryrepair)'de.

**Miktar doğrulaması bilerek burada değil:** negatif onarımın ne olduğuna
`HealingRules` karar verir ve aynı ön koşulu burada kopyalasaydık kural iki yerde
yaşardı. Aynı desenin uzun gerekçesi
[Health.md](Health.md#takedamageint-amount)'de.

Onarımın üç parçaya bölünmüş hâli:

```
Structure.TryRepair'deki `if (!IsStanding)`  ► KELEPÇE
Health.Heal                                  ► SAYIYI yazar
StructureLifecycle                           ► DURUMU tutar, onarımı hiç GÖRMEZ
```

---

## Tick(float deltaSeconds)

Yaşam döngüsüne aktarılır. Zamanın neden dışarıdan geldiği
[UnitLifecycle.md](UnitLifecycle.md#tickfloat-deltaseconds)'de ölçülerek yazılı ve
burada tekrar edilmiyor, yalnızca uygulanıyor.
