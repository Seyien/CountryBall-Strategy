# Combatant

> **Kaynak:** `Assets/Game/Core/Combat/Combatant.cs`
> **Ad alanı:** `GridStrategy.Combat` · **Assembly:** `GridStrategy.Combat` (`noEngineReferences: true`)
> **Rol:** Bileşik (Aggregate) — kimliği var, hafızası var, parçalar **arasındaki** kuralı yürütür

Bir savaşçının bütünü: canı, yaşam döngüsü ve saldırı tanımı bir arada. Var olma
sebebi tek cümleyle: `Health` canın bittiğini bilir ama `UnitLifecycle`'ı
tanımaz; `UnitLifecycle` düşmeyi bilir ama canı tanımaz. İkisini birden tanıyan
tek yer burasıdır — ve aralarındaki kuralı yürütmek başka hiçbir tipin işi
değildir.

Parçaların kendi kurallarına **karışmaz**: hasar formülünü `DamageRules`, menzili
`AttackResolver`, geri sayımı `UnitLifecycle` sahiplenir. Buradaki tek kural "ne
zaman hangisine haber verilir".

**NEDEN ADAPTÖR DEĞİL:** adaptör iki farklı **dil** arasında çeviri yapar
(`BoardAdapter`: Unity'nin `Vector3`'ü ↔ Core'un `int x, y`'si). Buradaki üç
parça da Core'a ait, aynı dili konuşuyor. Çevrilecek bir şey yok — sahiplenilecek
bir bütün var. Ayırt edici soru: **"iki tarafın dili farklı mı?"** Hayırsa
bileşiktir, evetse adaptör.

`Team` bu satırı **değiştirmedi**: taraf taşınan bir DEĞERdir, yürütülen bir
kural değil. "Aynı takıma saldırılmaz" hâlâ `TargetingRules`'ın; bu dosyada tek
bir `if` bile yok.

| Üye | Karar | Detay |
|---|---|---|
| `ReviveHealthDivisor` | pay oran olarak yazılır, sabit sayı olarak değil | [↓](#revivehealthdivisor) |
| `lastObservedState` | önceki durum hatırlanır, türetilmez | [↓](#lastobservedstate) |
| `Combatant(...)` | parçalar dışarıdan gelir; yalnız takımın varsayılanı var | [↓](#combatant) |
| `StateChanged` | geçişi taşır, kimliği taşımaz; proxy, aktarım değil | [↓](#statechanged) |
| `OnLifecycleStateChanged(UnitState)` | adlı metot; önce hatırla, sonra yay | [↓](#onlifecyclestatechangedunitstate-next) |
| `Team` | kurulurken belli olur ve değişmez | [↓](#team) |
| `State` | durumu soran tek üye; kısayol bool reddedildi | [↓](#state) |
| `TakeDamage(int)` | izin sormaz, uygular; erken çıkış reddedildi | [↓](#takedamageint-amount) |
| `TryRevive()` | kapı yaşam döngüsünün, pay burada | [↓](#tryrevive) |

**İlgili anlatılar:** [01-olay zinciri](../../../konular/01-olay-zinciri.md) ·
[05-yaşam döngüsü](../../../konular/05-yasam-dongusu.md) ·
[03-tahta sahipliği](../../../konular/03-tahta-sahipligi.md)

> Kodda `Combatant.StateChanged` üyesinin üstünde duran `DERİN ANLATIM:
> Docs/deep/01-olay-zinciri.md` yönlendirmesi yerinde bırakıldı; bu belge onun
> yerini almaz, kararın gerekçesini taşır.

---

## ReviveHealthDivisor

**SABİT SAYININ ANLAMI HER BİRİMDE DEĞİŞİR, ORANINKİ DEĞİŞMEZ.**

Dirilen birim TAM canla kalkmaz. Oran olarak yazılı, sabit sayı olarak değil:
sabit 50 can, maksimumu 40 olan bir birimde tam iyileşme, maksimumu 400 olanda
hiç anlamına gelirdi.

### HARİTA: aynı satır, üç birimde üç ayrı kural

Diriltme sıfır candan başladığı için sonuç doğrudan verilen paydır; ve
`HealingRules` üst kelepçesi maksimumu aşmayı zaten engelliyor.

```
  birim        Max    sabit 50 can           Max / 2
  ──────────────────────────────────────────────────────────────
  sıhhiyeci     40    40'a kelepçelenir      20  (yarısı)
                      = TAM İYİLEŞME ◄── ██ ANLAM KAYDI ██
  asker        100    50 = yarısı            50  (yarısı)
  kule         400    50 = %12,5 ≈ HİÇ       200 (yarısı)
                      ◄── ██ ANLAM KAYDI ██
  ──────────────────────────────────────────────────────────────
  sabit sayı : tek satır, ÜÇ farklı kural
  oran       : tek satır, TEK kural — "yarısıyla kalkar"
```

### KAPSAM: her sayı orana çevrilsin DEĞİL

Ayırt edici soru: **bu sayının anlamı, uygulandığı birimin BAŞKA bir sayısına
göre mi değişiyor?**

Karşı örnek aynı ad alanında, `UnitLifecycle.cs`:

```csharp
public const float DefaultDownedWindowSeconds = 10f;
```

Kurtarma penceresi MUTLAK bir sayıdır ve öyle kalmalıdır: on saniye, sıhhiyecide
de kulede de on saniyedir. Zamanın birime göre ölçeklenen bir karşılığı yok. Yani
ayrım "sabit mi oran mı" değil; sayının anlamını belirleyen ikinci bir sayı var
mı, yok mu.

### İŞ BÖLÜMÜ: bölen ile KAPI örtüşmez, bölüşür

"Diriltmek ölümü geri almak değil, riskli bir yatırımdır" cümlesi iki
mekanizmayla ayakta duruyor ve ikisi de `TryRevive`'da görünür:

```
ReviveHealthDivisor      ► NE KADAR canla kalkılacağını sınırlar
lifecycle.TryRevive()    ► KİMİN kalkabileceğini sınırlar
                           (yalnız Downed; kalıcı ölü kalkmaz)
```

Bölen silinip tam can verilseydi diriltme ölümü geri alırdı ve pencere bir tehdit
olmaktan çıkardı. Kapı silinseydi ceset de yarım canla kalkardı. Biri yatırımın
GETİRİSİNİ, diğeri UYGUNLUĞUNU sınırlıyor; ikisi aynı şeyi iki kez yapmıyor.

### `const` ne sağlar, ne sağlamaz

İki dil-seviyesi ayrıntısı, ikisi de bu biçime özgü:

- ✓ Bölenin sıfır olması burada **derleme** hatasıdır (sabit sıfıra bölme); aynı
  sayı bir alan olsaydı hata çalışma zamanında, diriltme anında patlardı.
- ✗ `public const` bir **derleme zamanı** sabitidir: değeri kullanan taraf onu
  kendi IL'ine gömer. Bugün tek assembly olduğu için görünmüyor, ama ayrı
  derlenen bir tüketici çıktığı gün sayıyı değiştirmek onu yeniden derlemeden
  yetmez. Değeri sonradan ayarlanabilir yapmak istendiği gün doğru şekil `const`
  değil `static readonly` ya da bir tanım nesnesidir.

### REDDEDILEN

```csharp
public const int ReviveHealthAmount = 50;
```

**KIRILAN:** aynı sabit iki birimde iki ayrı kural olur.

```
maksimumu 40 olan birim  -> 50 can "tam iyileşme" demek
maksimumu 400 olan birim -> aynı 50 can "hiç" demek
derleyici: hiçbir şey der  ·  test: Revive_ScalesWithMaxHealth kırılır
```

**KAZANIRDI:** diriltmenin gücü birimden BAĞIMSIZ olsun isteniyorsa — tankları
zayıflatıp ucuz birimleri güçlendiren bilinçli bir denge kararı.

**TEK CUMLE:** Sabit sayı birimden bağımsız görünür ama anlamı her birimde
değişir; oran her birimde aynı şeyi söyler.

---

## lastObservedState

**TÜRETME KURALI TABLONUN KOPYASIYSA, TABLO İKİ EVDE YAŞAR.**

Önceki durum burada hatırlanıyor, çünkü `UnitLifecycle.StateChanged` yalnızca
YENİ durumu taşıyor. "Nereden nereye" sorusunu cevaplamak için bir yerin geçmişi
tutması gerekiyor ve o yer, geçişi dışarı veren taraf olmalı — dinleyicilerin her
biri kendi kopyasını tutarsa aynı hatırlama işi üç yerde doğar (`UnitLifecycle`
içinde reddedilen "her kare State'i oku ve karşılaştır" seçeneğinin ta kendisi).

### HARİTA: gerçek tablo ve onun TERSİNDEN yazılmış kopyası

```
  GERÇEK tablo (sahibi UnitLifecycle)    TÜRETİLEN ters tablo (burası)
  geçiş             doğduğu yer          next  ->  "önceki"
  ───────────────────────────────────    ─────────────────────────
  Alive  → Downed   OnHealthDepleted     Downed -> Alive    ✓ bugün
  Downed → Dead     Tick                 Dead   -> Downed   ✓ bugün
  Downed → Alive    TryRevive            Alive  -> Downed   ✓ bugün
  ───────────────────────────────────    ─────────────────────────
         ◄── ██ ÜÇÜ DE DOĞRU: TEHLİKELİ OLAN TAM BU ██

  UnitLifecycle'a dördüncü bir durum girdiği an (örn. Stunned → Alive):
    Alive -> "önceki: Downed"   ◄── ██ YALAN ██  birim hiç düşmemişti
```

Tabloyu genişleten kişi `UnitLifecycle.cs`'i açar; buraya bakması için hiçbir
sebep yoktur, çünkü bu dosyada "geçiş" diye bir sözcük geçmez — yalnız `next`
adlı bir parametre vardır.

### KAPSAM: "türetme yapma" DEĞİL

Ayırt edici soru: **türetme kuralı, sahibindeki tablonun KOPYASI mı?**

Karşı örnek aynı dosyada, aşağıda:

```csharp
public UnitState State => lifecycle.State;
```

Bu da bir türevdir ve hiçbir blok gerektirmez — çünkü kopya değil, sahibinden
AYNI ANDA okunan tek gerçektir; sahibi değişince o da değişir. Reddedilen
`PreviousOf` ise sahibinden hiçbir şey okumaz, sahibin bilgisini kendi içinde
YENİDEN YAZAR. Okumak serbest, tabloyu ikinci kez yazmak değil.

### İŞ BÖLÜMÜ: alan ile YAYIN SIRASI örtüşmez, bölüşür

```
lastObservedState alanı  ► "nereden" sorusuna cevap VARLIĞINI verir
OnLifecycleStateChanged  ► önce hatırla, SONRA yay sırası cevabın
içindeki sıra              DOĞRULUĞUNU verir
```

Alan silinirse soru cevapsız kalır. Sıra ters çevrilirse (önce yay, sonra
hatırla) dinleyicinin olay içinde yaptığı bir çağrı — diriltme, hasar — ikinci
bir geçiş doğurur ve o geçiş "önceki durum" olarak hâlâ eskisini görür. İkisi
aynı işi yapmıyor: biri bilgiyi tutuyor, diğeri bilginin ne zaman güncellendiğini
tutuyor.

### `readonly` burada bilerek yok

Okuyucunun karşılaştıracağı dört üye — üçü tipin başında, `Team` biraz aşağıda:

```csharp
private readonly Health health;
private readonly UnitLifecycle lifecycle;

private UnitState lastObservedState;   // ◄── `readonly` YOK — bu bölümün konusu

public Team Team { get; }              // alan DEĞİL, get-only property
```

Bu tipin diğer üç alanı `readonly` (`health`, `lifecycle` ve `Team`'in get-only
property'si); okuyucu aynı krediyi buraya taşıyabilir. Taşınamaz: hatırlama işi
tanımı gereği YAZILABİLİR olmak zorunda. Bu alan tipin tek değişebilir üyesidir
ve bunun bir kaza olmadığını söyleyen tek yer bu satırdır.

### GARANTİ NEREDE BİTER

Alanın doğruluğu tek bir söze dayanıyor: "her geçiş bir event olarak gelir". O
sözü tutan şey burada değil, `UnitLifecycle`'daki `SetState`'in tek giriş noktası
olmasıdır. `State`'e event tetiklemeden yazan ikinci bir yol açıldığı gün bu alan
sessizce eskir ve hata "bazen yanlış geçiş bildiriliyor" diye çıkar.

### REDDEDILEN

Alan hiç doğmaz; önceki durum YENİ durumdan türetilir, çünkü her geçişin tek bir
kaynağı var:

```csharp
private static UnitState PreviousOf(UnitState next)
{
    // Downed'a yalnız Alive'dan, Dead'e yalnız Downed'dan,
    // Alive'a yalnız Downed'dan gelinir.
    return next == UnitState.Alive ? UnitState.Downed
        : next == UnitState.Downed ? UnitState.Alive
        : UnitState.Downed;
}
```

**KIRILAN:** geçiş tablosu, sahibinin dışında ve tersinden İKİNCİ kez yazılır.

```
bugün üç geçişte de doğru cevap verir -> tam bu yüzden tehlikeli
UnitLifecycle'a dördüncü durum girer  -> burası yalan söylemeye başlar
dinleyici "Alive'dan geldi" duyar, oysa birim düşmüştü
derleyici: hiçbir şey der  ·  test: hiçbiri kırılmadan yalan başlar
```

**KAZANIRDI:** geçiş tablosu gerçekten tek yönlü ve dallanmasız olsaydı — Alive →
Dead, başka hiçbir şey — o gün "önceki durum" diye bir soru olmazdı.

**TEK CUMLE:** Türetilebilen her bilgi türetilmeli değildir; türetme kuralı
tablonun KOPYASIysa tablo iki yerde yaşamaya başlar.

---

## Combatant(...)

Kurucu üç ayrı kararı bir arada taşıyor: **takımın varsayılanı**, **parçaların
dışarıdan gelmesi** ve **aboneliğin sırası**.

### Karar 1 — VARSAYILAN, SORUNUN BİRİMDEN BAĞIMSIZ CEVABI VARSA KONUR

Takımın VARSAYILANI var, diğer üç parçanın yok — ve bu tutarsızlık bilinçli. "Kaç
can", "kaç saniye", "kaç hasar" sorularının birimden bağımsız bir cevabı yoktur;
takımın cevabı ise `Team.cs`'te zaten verilmiş: sıfır BİLEREK tarafsız. Atlanan
takım, uydurulmuş bir taraf değil, açıkça tarafsızlık demektir.

#### HARİTA: dört parametre, tek ayırt edici soru

```
  parametre       "birimden bağımsız bir cevabı var mı"   varsayılan
  ─────────────────────────────────────────────────────────────────
  health          YOK — "kaç can" her birimde başka         yok  ✓
  lifecycle       YOK — pencereler birime göre ayarlanır    yok  ✓
  attackProfile   YOK — "kaç hasar, kaç menzil"             yok  ✓
  ─────────────────────────────────────────────────────────────────
  team            VAR — Team.None, ve anlamı uydurma değil  VAR  ✓
                  ◄── ██ TUTARSIZLIK DEĞİL, AYNI TESTİN SONUCU ██
```

`Team.None`'ın "sıfır BİLEREK tarafsız" olması `Team.cs`'te yazılı; bu varsayılan
o kararı KULLANIYOR, ikinci kez vermiyor.

#### KAPSAM: "her parametreye varsayılan koy" DEĞİL

Karşı örnek aynı kurucunun ilk üç parametresi — yukarıdaki tablonun üst yarısı.
Üçünün de üstünde böyle bir blok YOKTUR ve olmamalıdır; bir varsayılan konsaydı
çağıran "hangi canla doğduğunu" yazmayı unutabilir ve bunu hiçbir derleme hatası
göstermezdi. Üçü de zorunlu ve üçü de `null` gelirse patlıyor.

#### İŞ BÖLÜMÜ: varsayılan ile SIFIRINCI DEĞER bölüşür

"Takımı yazılmamış bir şey sessizce taraf seçmesin" iki mekanizmayla korunuyor ve
ikisi FARKLI olguyu kapatıyor:

```
bu varsayılan (`= Team.None`)  ► ARGÜMAN hiç yazılmadı
                                 `new Combatant(h, l, p)`
Team.cs'te None'ın sıfırıncı   ► ALAN hiç atanmadı
olması                           dizi elemanı, `default(Team)`
```

Varsayılan silinirse takım kurucunun dördüncü zorunlu sorusu olur ve
`Constructor_NullPart_Throws`'un üç çağrısı da takım yazmak zorunda kalır — o test
null korumasını değil imzayı sınıyormuş gibi okunur. Sıfırıncı değer silinirse bu
varsayılan hiçbir işe yaramaz, çünkü orada bir kurucu ÇAĞRISI yoktur. Biri çağrı
yerini, diğeri depolamayı kapatıyor.

#### VARSAYILAN BİR DOĞRULAMA DEĞİLDİR

Sözü edilen üç satır — ve onlara katılmayan dördüncüsü — kurucunun içinde şöyle
duruyor:

```csharp
this.health = health ?? throw new ArgumentNullException(nameof(health));
this.lifecycle = lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));
AttackProfile = attackProfile ?? throw new ArgumentNullException(nameof(attackProfile));

Team = team;   // ◄── DÖRDÜNCÜ: `?? throw` YOK, ve olmaması bir KARAR
```

Okuyucu üstteki üç `?? throw` satırına bakıp dördüncü parametrenin de
denetlendiğini sanabilir. `Team` için `throw` YOK ve olmamalı da — gerekçesi
kurucunun içinde, `Team = team;` satırının üstünde yazılı: her `Team` değeri
geçerli bir taraftır. Varsayılan yalnızca yazılmamış argümanı doldurur; yazılan
argümanı denetlemez.

#### GARANTİ NEREDE BİTER

`new Combatant(h, l, p, (Team)7)` derlenir ve buradan geçer. Enum C#'ta kapalı bir
küme olmadığı için varsayılanın kapattığı tek şey "hiç yazılmamış" hâlidir;
"yanlış yazılmış" hâli okuyan tarafın işi.

#### REDDEDILEN

```csharp
public Combatant(Health health, UnitLifecycle lifecycle,
                 AttackProfile attackProfile, Team team)
```

**KIRILAN:** takım, kurucunun DÖRDÜNCÜ zorunlu sorusu olur.

```
Constructor_NullPart_Throws'un üç çağrısı da takım yazar
o test null korumasını değil imzayı sınıyormuş gibi okunur
yeni her tip — yıkılabilir yapı, tuzak — doğduğu an taraf seçer
derleyici: hiçbir şey der  ·  test: yeşil kalır, yalnız gürültülenir
```

**KAZANIRDI:** oyunda tarafsız hiçbir şey olmayacaksa — o gün varsayılan, takımı
atanmayı unutulmuş birimi sessizce `Team.None` yapardı.

**TEK CUMLE:** Varsayılan ancak sorunun birimden bağımsız bir cevabı VARSA konur;
"kaç can" sorusunun yok, "hangi taraf" sorusunun var.

### Karar 2 — PARÇAYI DIŞARIDAN ALMAK, KİMLİK HAKKINI DIŞARIDA BIRAKIR

#### HARİTA: TANIM mı, VARLIK mı

```
  REDDEDILEN — parça kurucunun İÇİNDE doğarsa
    ┌─okçu#1──┐  ┌─okçu#2──┐        ┌─okçu#200┐
    │ profil ─┼─►A│ profil ─┼─►B ... │ profil ─┼─►Z
    └─────────┘  └─────────┘        └─────────┘
    ◄── ██ AYNI TANIM, 200 AYRI NESNE ██
    "okçu hasarını 1 artır" -> 200 nesnenin hepsi değişmeli
    ve test, gerçek parçayı değil kurucunun ürettiğini sınar

  SEÇİLEN — parça dışarıdan gelir
    ┌─okçu#1──┐  ┌─okçu#2──┐        ┌─okçu#200┐
    │ profil ─┼┐ │ profil ─┼┐    ┌──┼─ profil │
    └─────────┘│ └─────────┘│    │  └─────────┘
               └────────────┴────┘
                           ▼
                 ╔════════════════════╗
                 ║ TEK AttackProfile  ║ ◄── ██ TANIM, PAYLAŞILIR ██
                 ╚════════════════════╝
```

Paylaşımı GÜVENLİ kılan şey bu kurucu değil, `AttackProfile`'ın kendisi: `Damage`
ve `Range` yalnız `get`, kurucudan sonra yazılamıyor. Değişmez bir nesnenin ikinci
bir oku tehlike doğurmaz.

#### KAPSAM: "her parçayı paylaş" DEĞİL

Karşı örnek aynı kurucunun İLK parametresi: `Health` de dışarıdan alınıyor ama
PAYLAŞILAMAZ. İki savaşçıya aynı `Health` nesnesi verilirse tek bir can havuzunu
paylaşırlar; birine vurmak diğerini de düşürür ve hiçbir derleme hatası çıkmaz.
Yani "dışarıdan al" kararı paylaşımı MÜMKÜN kılar, ZORUNLU kılmaz — hangi parçanın
paylaşılabileceğine parçanın kendi değişmezliği karar verir. Bu tip o kararı
vermiyor; verme HAKKINI dışarıda bırakıyor — aşağıdaki TEK CUMLE'nin söylediği
tam olarak bu.

#### İŞ BÖLÜMÜ: dışarıdan alma ile `?? throw` bölüşür

```
parçayı dışarıdan almak  ► paylaşımı ve kimliği MÜMKÜN kılar
`?? throw` üçlüsü        ► "hiç vermedim" hâlini KAPATIR
```

Null kontrolleri silinirse hata çok sonra, ilk `health.TakeDamage` çağrısında ve
sebebinden uzakta patlar. Dışarıdan alma bırakılırsa 200 kopya doğar. Biri kimliği
açıyor, diğeri boşluğu kapatıyor.

#### REFERANS SEMANTİĞİ: burada tehlike değil, özellik

Parametre olarak verilen bir sınıf nesnesi KOPYALANMAZ; yalnızca ikinci bir ok
açılır. `Battle`'daki tahta sahipliği bloğunda o ikinci ok bir tehlikeydi (iki
yazar doğuyordu); burada ise kazancın kendisi — çünkü `AttackProfile` değişmez,
yani ikinci ok YAZAMAZ. Aynı dil mekanizması, zıt sonuç; farkı yaratan şey
nesnenin değişebilir olup olmaması. Uzun hâli
[03-tahta sahipliği](../../../konular/03-tahta-sahipligi.md)'nde.

#### GARANTİ NEREDE BİTER

Bu tip, aldığı parçaların başkasına da verilmediğini denetleyemez. Aynı
`UnitLifecycle` örneği ikinci bir `Combatant`'a verilebilir ve hiçbir şey
engellemez; sonucu aşağıdaki abonelik kararında yazılı. Sözleşme çağıranın
disiplininde biter.

#### REDDEDILEN

```csharp
this.health = new Health(maxHealth);
```

**KIRILAN:** parçayı içeride kurmak, TANIM rolündeki `AttackProfile`'ı VARLIK'a
çevirir ve paylaşımı imkânsızlaştırır.

```
200 okçu tek profili paylaşamaz -> 200 kopya doğar
test gerçek parçayı değil kurucunun ürettiğini sınar
derleyici: hiçbir şey der  ·  test: yeşil kalır, ölçüsü kayar
```

**KAZANIRDI:** parça sayısı hiç artmayacaksa ve paylaşım hiç gerekmiyorsa — kurucu
çağıranı üç nesne kurma külfetinden kurtarırdı.

**TEK CUMLE:** Parçalarını dışarıdan almak, o parçaların KİMLİĞİNE karar verme
hakkını da dışarıda bırakmaktır.

### Karar 3 — abonelik kurucunun en sonunda

Sıra bir karardır: yukarıdaki üç null kontrolünden biri patlarsa geriye abone
olunmuş bir `UnitLifecycle` kalmamalı. Çağıran aynı `lifecycle` örneğini ikinci
bir `Combatant`'a verebilir (hiçbir şey engellemiyor); yarım kalmış bir
kurulumdan artan abonelik o gün ölü bir nesneyi olayla birlikte hayatta tutar ve
dinleyici aynı geçişi iki kez duyar.

Aboneliğin ÇÖZÜLDÜĞÜ bir yer yok ve bu bir ihmal değil: bu tip kendi
`lifecycle`'ının SAHİBİ. İkisi birlikte doğar, birlikte çöpe gider — abonelik bir
sahiplik sınırını GEÇMİYOR. Sınırı geçen abonelik `Battle`'da (Combatant →
Battle) ve orada bırakma zorunlu; gerekçesi `Battle`'ın kendi dosyasında.

İki satır da `this.` ile yazılı: parametre alanı GÖLGELİYOR ve iki satırın aynı
nesneye baktığını okuyanın çıkarmak zorunda kalması gereksiz bir yük.

---

## StateChanged

### Zincirin orta halkası

```
UnitLifecycle.StateChanged  Action<UnitState>              hangi duruma
Combatant.StateChanged      Action<UnitState, UnitState>   nereden nereye
Battle.UnitStateChanged     Action<Unit, UnitState, ...>   KİM, nereden nereye
```

Bu halka KİMLİK taşımaz ve taşıyamaz: bu tip kendi `Unit`'ini BİLMEZ. Kimlik
parçalarda değil, sözlükte yaşıyor — aynı gerekçe `unitViews`'ta ve
`Battle.combatants`'ta zaten iki kez yazılı. Kimliği ekleyen halka `Battle`'dır,
çünkü eşleşmenin tek sahibi odur. Dört durağın tamamı
[01-olay zinciri](../../../konular/01-olay-zinciri.md)'nde hikâye olarak.

### Karar 1 — OLAY KENDİNİ TAŞIRSA, EŞLEŞMEYE İKİNCİ BİR SAHİP DOĞAR

#### HARİTA: imza ne taşırsa, dinleyicinin İLK işi o olur

`Battle` her şeyi `Unit` ile anahtarlar. Olayın taşıdığı tip, o dinleyicinin daha
ilk satırda ne yapmak zorunda kaldığını belirler.

```
  REDDEDILEN — Action<Combatant, UnitState, UnitState>
    Combatant ──olay(this, p, n)──► Battle'ın dinleyicisi
      elde: Combatant                aranılan: Unit
      └─► Dictionary<Unit, Combatant> DEĞERDEN taranır
          ◄── ██ TERS ARAMA ██ sözlük bu yön için kurulmadı

  SEÇİLEN — Action<UnitState, UnitState>
    Combatant ──olay(p, n)──► (p, n) => ...Invoke(unit, p, n)
      elde: yalnız geçiş       unit ZATEN kapanışın İÇİNDE
          ◄── ██ ARAMA YOK ██ kimlik aboneliğe gömülü
```

Kimliği ekleyen kapanışın nerede saklandığı ve neden saklanmak zorunda olduğu
`Battle`'ın yönlendirici sözlüğünün üstünde yazılı.

#### KAPSAM: "olay göndereni taşımasın" DEĞİL

Ayırt edici soru: **eklenecek bilginin SAHİBİ bu halka mı?**

Karşı örnek aynı dosyada: bu halka iç olaya bir şey EKLİYOR — "nereden" bilgisini
— ve doğrusu da budur, çünkü o bilginin sahibi burası: `lastObservedState` bu
tipin kendi alanıdır. `Unit` ise bu tipin alanı değildir, hiç olmadı. Yasak
zenginleştirmeye değil, sahibi olmadığın bilgiyi taşımaya konur.

#### İŞ BÖLÜMÜ: geçiş ile KİMLİK örtüşmez, bölüşür

```
bu olayın iki değerli imzası    ► "nereden nereye"  sahibi burası
Battle'ın birim başına kapanışı ► "KİM"             sahibi Battle
```

İmza tek değere inerse "nereden" sorusu her dinleyiciye ayrı ayrı düşer ve aynı
hatırlama işi üç yerde doğar. Kapanış silinirse geçiş sahipsiz kalır: dinleyici
bir değişim duyar, kimin değiştiğini öğrenemez. Biri geçişi, diğeri kimliği
kapatıyor.

#### `event` anahtar sözcüğü bu kırılmaya karşı sıfır koruma

O sözcük yalnız iki şeyi kapatır: dışarıdan `Invoke` ve dışarıdan `=` ile toptan
atama. İmzanın ne taşıdığı, dinleyicinin kimliği nereden bulacağı ve aboneliğin
sökülebilir olup olmadığı hakkında tek kelime söylemez.

#### GARANTİ NEREDE BİTER

Bu imza ters aramayı gereksiz kılar ama YASAKLAMAZ; kimliği aboneliğe gömmek
dinleyicinin tercihidir. O abonelik bir KAPANIŞ olduğu an dil seviyesinde ikinci
bir kural doğar: delege eşitliği metne değil NESNEYE bakar, yani abone olunan
örnek saklanmadan `-=` sessizce hiçbir şey yapmaz. Sözleşme dinleyicinin
disiplininde biter.

#### REDDEDILEN

Olay kendisini de taşır ve `Battle` tek bir dinleyiciyle kurtulur:

```csharp
public event Action<Combatant, UnitState, UnitState> StateChanged;
```

**KIRILAN:** `Battle`'ın elinde `Combatant` olur, `Unit` olmaz — kimliğe TERS arama
gerekir.

```
sözlüğü baştan tara -> her geçişte UnitCount kadar karşılaştırma
Dictionary<Combatant, Unit> tut -> eşleşme İKİ sahipli olur
aynı Combatant iki Unit'e kayıtlıysa hangisi olduğu hiç bilinmez
derleyici: hiçbir şey der  ·  test: hiçbiri kırmızıya dönmez
```

**KARSILASTIRMA:**

| imza | ne taşır | sonucu |
|---|---|---|
| `Action<UnitState>` | yalnız yeni durum | "nereden" dinleyiciye kalır |
| `Action<Combatant, ...>` | kendini taşır | Battle ters arama yapar |
| `Action<UnitState, UnitState>` | geçişi taşır | kimliği üst halka ekler |

**KAZANIRDI:** `Unit` ile `Combatant` TEK bir tipte birleşseydi — o gün "kendini
taşımak" kimliği taşımakla aynı şey olurdu.

**TEK CUMLE:** Kimlik parçada değil sözlükte yaşar; kendini taşıyan bir olay kimlik
taşıyor SANILIR ve eşleşmeye ikinci bir sahip doğurur.

### Karar 2 — ÖZELLİK DEĞER GEÇİRİR, OLAY BAĞ GEÇİRİR

#### HARİTA: abonelik hangi nesnenin listesine düşüyor

```
  REDDEDILEN — add/remove aktarımı
    dış dinleyici ──abone──► Combatant.StateChanged
                                 │ add { lifecycle.StateChanged += value; }
                                 ▼
                            UnitLifecycle'ın dinleyici listesi
                            ◄── ██ BAĞ İÇ PARÇAYA DÜŞTÜ ██
    Combatant aradan çekilse bile bu bağ ayakta kalır; onu kesecek bir
    kol artık hiçbir yerde yoktur.

  SEÇİLEN — proxy
    dış dinleyici ──abone──► Combatant.StateChanged  (kendi listesi)
    Combatant     ──abone──► UnitLifecycle
                            ◄── ██ İÇ LİSTEDE HEP TEK ABONE ██
    ve o tek abone bir METOT: adı olan, sökülebilir bir hedef.
```

#### KAPSAM: "aktarma yapma" DEĞİL

Ayırt edici soru: **aktarılan şey bir DEĞER mi, yoksa bir BAĞ mı?**

Karşı örnek aynı dosyada, dört satır arka arkaya: `State`, `CurrentHealth`,
`RemainingSeconds` ve `IsReadyForCleanup`. Dördü de iç parçaya yapılan düpedüz
aktarımdır ve dördü de doğrudur — çünkü okuma biter bitmez geriye hiçbir bağ
kalmaz; çağıran eline bir KOPYA alır, iç parçaya tutamak almaz. Olay aktarımı ise
çağrının ömrünü aşan bir bağ bırakır ve o bağı bu tip artık kesemez.

#### İŞ BÖLÜMÜ: `private` alan ile ADLI metot bölüşür

```
`private readonly UnitLifecycle lifecycle` ► parçaya DIŞARIDAN
                                             erişimi kapatır
ayrı `OnLifecycleStateChanged` metodu      ► içeriye giden TEK
                                             bağı sökülebilir tutar
```

Alan gizliliği düşerse dış dinleyici zaten doğrudan bağlanır ve proxy bir törene
dönüşür. Metot bir lambdaya çevrilirse bağ kesilemez hâle gelir: delege eşitliği
nesneye bakar, aynı metnin ikinci lambdası birinciyle eşit değildir. Biri
dışarıyı, diğeri içeriyi kapatıyor.

#### `private` bu kırılmaya karşı sıfır koruma sağlar

Reddedilen add/remove gövdeleri bu tipin İÇİNDE yazılıdır; `private` onlara sonuna
kadar açıktır. Gizlilik dışarının uzanmasını keser, tipin kendi eliyle dışarı
verdiği şeyi değil.

#### DİL SEVİYESİNDE NE OLUR

add/remove aksesuarları yazıldığı an derleyici bu tipe destek alanı ÜRETMEZ:
`StateChanged?.Invoke(...)` derlenmez, çünkü artık burada çağrılacak bir dinleyici
listesi yoktur. Yani reddedilen şekil yalnız kapsüllemeyi delmiyor;
`lastObservedState`'i ve onun yayın sırasını aynı anda ölü koda çeviriyor.

#### REDDEDILEN

Proxy hiç doğmaz; iç parçanın olayı olduğu gibi dışarı verilir:

```csharp
public event Action<UnitState> StateChanged
{
    add { lifecycle.StateChanged += value; }
    remove { lifecycle.StateChanged -= value; }
}
```

**KIRILAN:** dış dinleyici doğrudan İÇ parçaya bağlanır ve bağ kopmaz.

```
bu tip aradan çekilse bile lifecycle'a tutunan bağ kalır
kapsülleme tek satırda biter -> parça artık gizli değil
derleyici: hiçbir şey der  ·  test: hiçbiri kırmızıya dönmez
```

**KAZANIRDI:** dinleyicilerin hiçbiri "nereden" sorusunu sormasaydı — o gün
add/remove aktarımı hem daha kısa hem daha dürüst olurdu.

**TEK CUMLE:** Bir olayı olduğu gibi geçirmek onu SAHİPLENMEK değildir; proxy
kapsüllemeyi delerken hiçbir şey eklemez.

### "Nereden" sorusunun gerçek tüketicisi

Üç durumun iki görseli olduğu için arayüz `Alive → Downed` ile `Downed → Dead`
geçişlerine farklı cevap verir. Yani ikinci parametre bir ihtimal değil, yazılmış
bir ihtiyaç.

---

## OnLifecycleStateChanged(UnitState next)

İç parçanın tek değerli olayını iki değerli hâle çevirir. Ayrı bir metot olması
kasıtlı: kurucuda yazılmış bir lambda, aboneliği çözmenin mümkün olduğu tek yeri
de yok ederdi (delege eşitliği nesneye bakar; aynı gövdeyi taşıyan ikinci bir
lambda birinciyle eşit değildir).

**Önce hatırla, SONRA yay.** Tersi olsaydı dinleyicinin olay içinde yaptığı bir
çağrı — diriltme, hasar — ikinci bir geçiş doğurabilir ve o geçiş "önceki durum"
olarak hâlâ eskisini görürdü. Sıra, `lastObservedState` alanının doğruluğunu
taşıyan şeyin ta kendisidir.

---

## Team

**CEVABI İKİ ÇAĞRI ARASINDA DEĞİŞEBİLEN SORU, KURAL TAŞIYAMAZ.**

Taraf kurulurken belli olur ve DEĞİŞMEZ. Diğer üç parça da `readonly`;
dördüncüsünün yazılabilir olması tipin tek sözünü — "kurulduğun anda ne olduğun
bellidir" — tek satırda bozardı.

### Neden `Unit`'te değil de burada

Tarafı soran bütün sorular ("kime vurulur", "kim diriltilir") `GridStrategy.Combat`
ad alanında soruluyor. `Unit`'e konsaydı `GridStrategy.Core` ad alanı
`GridStrategy.Combat`'i tanımak zorunda kalır ve `TargetingRules` takımı öğrenmek
için bir `Combatant`'tan bir `Unit`'e ulaşmak zorunda kalırdı — bugün ikisi
arasında böyle bir bağ yok ve bu işi kurmak için bağ açmaya değmez.

### Kurucuda doğrulama neden yok

`Team`'in her değeri geçerli bir taraftır, `Team.None` dahil. Kurucuya bir aralık
kontrolü koymak, `Team.cs`'teki değer listesini C# dışı bir yerde ikinci kez
yazmak olurdu.

### HARİTA: onay ile uygulama arasındaki açıklık

Bir saldırı tek bir işlem değil, sıralı bir zincirdir: taraf zincirin BAŞINDA
sorulur, hasar SONUNDA uygulanır. Arada geçen her satır bir açıklıktır — ve o
açıklık bu dosyada gerçektir, çünkü hasar uygulanırken `StateChanged` yayılır ve
dinleyicinin kodu zincirin İÇİNDE koşar.

```
  REDDEDILEN — `{ get; set; }`
    t0  AttackAction ► CanBeAttacked(durum, saldıran, hedef)  ✓ onay
    t1  hedef.Team = saldıranın takımı  ◄── ██ ARAYA GİREN ATAMA ██
    t2  hedef.TakeDamage(...)           ► dost ateşi, onaysız
        ◄── ██ t0'ın onayı t2'de ARTIK BAŞKA BİR OLGUYA AİT ██

  SEÇİLEN — `{ get; }`
    t0  onay ✓
    t1  hedef.Team = ...   ✗ DERLEME HATASI — t1 doğamaz
    t2  hasar              ► t0'ın baktığı olgu hâlâ aynı olgu
```

### KAPSAM: "her üye salt okunur olsun" DEĞİL

Ayırt edici soru: **bu, kurulurken BELLİ OLAN bir şey mi, yoksa nesnenin
yaşadıkça değişen bir kaydı mı?**

Karşı örnek aynı dosyada, yukarıda: `lastObservedState`. Bu tipin tek yazılabilir
üyesidir ve yazılabilir OLMAK ZORUNDADIR — hatırlama işi tanımı gereği değişir.
Aynı ayrım komşu tipte de görünür: `UnitLifecycle.State` `private set` taşır,
çünkü orada değişmek işin kendisidir. Taraf ise doğumda belli olur; ikisi aynı
testin iki farklı cevabıdır.

### İŞ BÖLÜMÜ: setter'ın YOKLUĞU ile kurucudaki atama bölüşür

```
`{ get; }`        ► değerin SONRADAN değişmesini kapatır
kurucudaki atama  ► değerin BELİRLENDİĞİ tek anı sabitler
```

Setter eklenirse cevap iki çağrı arasında kayar. Kurucudaki atama silinirse her
savaşçı sessizce `Team.None` doğar ve parametre süse döner; derleyici atanmamış bir
otomatik özellik için hiçbir şey demez. Biri zamanı, diğeri doğumu kapatıyor.

### `private set` bu kırılmayı önlemez

Okuyucu "dışarı kapalı olması yeter" diyebilir. Yetmez: yukarıdaki `t1` satırını bu
tipin KENDİ içine yazılmış bir metot da üretebilir. `{ get; }` ise derleyicinin
ürettiği alanı `readonly` yapar — kurucudan sonra bu tipin kendi metotları da
yazamaz. Fark bir görgü kuralı değil, alanın modifikatörüdür.

### GARANTİ NEREDE BİTER

Söz bu NESNE için verilmiştir, kimlik için değil. `Battle`'ın `Unit` → `Combatant`
eşleşmesinde aynı `Unit`'e başka bir `Combatant` kaydedilirse "o birimin tarafı"
değişmiş olur ve buradaki hiçbir modifikatör onu görmez. Değişmezlik nesnenin
sınırında biter.

### REDDEDILEN

```csharp
public Team Team { get; set; }
```

**KIRILAN:** "aynı takım mı" sorusunun cevabı iki çağrı arasında DEĞİŞEBİLİR olur.

```
AttackAction hedefi onaylar -> araya giren tek atama tarafı çevirir
hasar uygulanana kadar dost ateşi açılır -> ancak oyunda görülür
derleyici: hiçbir şey der  ·  test: hiçbiri kırmızıya dönmez
```

**KAZANIRDI:** ele geçirme gerçek bir mekanik olsaydı — zihin kontrolü, bayrak
devri, taraf değiştiren paralı asker.

**TEK CUMLE:** Kurulurken belli olan şey readonly yazılır; yazılabilir bir taraf
"kurulduğun anda ne olduğun bellidir" sözünü tek satırda bozar.

---

## State

**ÜÇ DEĞERLİ GERÇEĞİ İKİ KUTUYA BÖLEN KISAYOL, BİRİNİ YUTAR.**

Durumu soran TEK üye bu. Yanına bir kısayol `bool` eklemek reddedildi.

### HARİTA: aynı birim, aynı an, zıt cevaplar

```
  durum    State     IsAlive   CanBeAttacked   CanBeRevived
  ───────────────────────────────────────────────────
  Alive    Alive     true      true            false
  Downed   Downed    false ◄── true            true
                     ██ AYNI BİRİM İÇİN ZIT CEVAP ██
  Dead     Dead      false     false           false
  ───────────────────────────────────────────────────
```

`Alive` ile `Dead` satırlarında bool doğruyu söyler; taşan tek satır `Downed`'dır
ve kısayolun yuttuğu şey tam olarak odur. Bool'un kapasitesi iki, taşınan ayrım üç
— eşleme KAYIPLI, ve kaybın yerini derleyici gösteremez.

### KAPSAM: "kısayol üye yazma" DEĞİL

Ayırt edici soru: **kısayol sahibinin cevabını TAŞIYOR mu, yoksa yerine kendisi mi
YARGILIYOR?**

Karşı örnek aynı dosyada, birkaç satır aşağıda: `IsReadyForCleanup`. O da bir
bool'dur, o da tek satırdır, o da yaşam döngüsünden gelir — ve hiçbir blok
gerektirmez, çünkü yargıyı burada üretmez; sahibinin verdiği cevabı olduğu gibi
taşır. `CurrentHealth` ile `RemainingSeconds` de aynı sınıfta. Reddedilen kısayol
ise sahibinden bir enum alıp burada `==` ile daraltır. Ayıran şey üyenin tipi
değil, yargının nerede üretildiği.

### İŞ BÖLÜMÜ: State ile CurrentHealth örtüşmez, bölüşür

```
State         ► ALAN yargısı — kurtarılabilir mi, kaldırılacak mı
CurrentHealth ► SAYI — ne kadar kaldı
```

`State` silinirse çağıran alan yargısını sayıdan çıkarmaya çalışır ve `Downed` ile
`Dead` ayırt edilemez olur; ikisinin de canı sıfırdır. `CurrentHealth` silinirse
can çubuğu enuma bakmak zorunda kalır ve üç kademeden fazlasını gösteremez. İkisi
aynı soruyu iki kez cevaplamıyor; iki ayrı soruyu bir kez cevaplıyor.

### Salt okunur olması bu kırılmayı önlemez

Okuyucu `=>` ile yazılmış bir türevin zararsız olduğunu düşünebilir. Türev olmak
yalnız BAYAT olmaktan kurtarır: cevap her okumada tazedir. Yanlış olmaktan
kurtarmaz — ve buradaki kusur tazelik değil, daraltmadır.

### `Health.HasRemaining` bu boşluğu dolduramaz

Ve bilerek doldurmuyor: o SAYIyı söyler, `State` ALAN yargısını. `Downed` bir birim
ile `Dead` bir birimin canı aynıdır — ikisi de sıfır — ama biri kurtarılabilir,
diğeri değildir. Farkı yalnızca yaşam döngüsü bilir.

### REDDEDILEN

`State`'in yanında, onun kısayolu:

```csharp
public bool IsAlive => lifecycle.State == UnitState.Alive;
```

**KIRILAN:** iki soru aynı birim hakkında ZIT cevap verir.

```
Downed birim -> IsAlive false, ama CanBeAttacked hâlâ true
aynı birim -> TryRevive hâlâ başarılı, çağıran hangisine baksın
kurtarma penceresi "ölü sayılan" birimlerde sessizce kullanılmaz olur
derleyici: hiçbir şey der  ·  test: Downed_StillAcceptsDamage örtbas eder
```

**KAZANIRDI:** yaşam döngüsü gerçekten iki değerli olsaydı — Alive/Dead, kurtarma
penceresi yok — o gün enum bir tipi boşuna eklemiş olurdu.

**TEK CUMLE:** `UnitState` tam olarak "iki bayrak dört kombinasyon üretir, üçü
anlamlıdır" diye doğdu — gerekçesi `UnitState.cs`'te — ve yanına konan tek bir bool
o iki kaynaklı gerçeği geri getirir.

---

## TakeDamage(int amount)

**SESSİZ BİR HAYIR, KURALI İKİNCİ BİR EVE TAŞIR.**

Kontrol her KAREde değil, her HASAR OLAYINDA yapılır — ve zaten bu metodun
içindeyiz, yani "canı bitti mi" sorusunu sormanın maliyeti bir bool okuması. Ayrı
bir dinleme mekanizması (event) bugün buna hiçbir şey katmazdı: haber verecek olan
da, duyacak olan da bu tip.

Metodun ikinci satırındaki soru SAYIya soruluyor, alana değil: "canı kaldı mı".
Alan cevabını (Alive / Downed / Dead) o satırdan sonra `UnitLifecycle` verir.

### HARİTA: "kime vurulur" sorusunun kaç evi var

```
  BUGÜN — tek ev
    "kime vurulur"   ► TargetingRules.CanBeAttacked ◄── ██ TEK EV ██
    "ne kadar hasar" ► DamageRules
    "ne uygulanır"   ► burası — soru sormaz, uygular

  REDDEDILEN — erken çıkış eklenirse
    "kime vurulur"   ► TargetingRules.CanBeAttacked
                     ► + bu metodun ilk satırı ◄── ██ İKİNCİ EV ██
    iki ev ayrıştığı gün: TargetingRules "vurulabilir" der, burası
    vurmaz — ve ikisinin çeliştiğini gösteren tek satır hiçbir yerde
    yoktur.
```

### KAPSAM: "erken çıkış yazma" DEĞİL

Ayırt edici soru: **erken çıkış kuralın SAHİBİNE mi soruyor, yoksa kuralı burada mı
kuruyor — ve cevabı çağırana söylüyor mu?**

Karşı örnek aynı dosyada, hemen aşağıdaki `TryRevive`: gövdesi düpedüz bir erken
çıkışla başlar ve doğrudur. Çünkü kararı kendi vermez, sahibine sorar ve aldığı
hayırı çağırana `false` olarak GERİ SÖYLER. Buradaki metot `void` döner; aynı erken
çıkış burada söylenemez bir hayır olurdu — çağıran hasarın uygulandığını mı,
reddedildiğini mi öğrendiğini ayırt edemez.

### İŞ BÖLÜMÜ: izin ile SONUÇ örtüşmez, bölüşür

```
TargetingRules.CanBeAttacked  ► İZİN — vurulabilir mi
buradaki HasRemaining kapısı  ► SONUÇ — vurulan ne oldu
```

İzin silinirse cesede de vurulur ve kural hiç kimsenin olmaz. Sonuç kapısı
silinirse can biter ama birim düşmez: geri sayım hiç başlamaz, kurtarma penceresi
hiç açılmaz. Erken çıkış ikisinin arasına üçüncü bir mekanizma sokar ve ilkinin
işini yarım yapar.

### GARANTİ NEREDE BİTER

Bu metot izin sormadığı için tek başına hiçbir şeyi güvence altına almaz: doğrudan
çağıran biri onaysız hasar uygulayabilir. Sözleşme çağrı yolunun disiplininde biter
— ve o yolun tek meşru girişi `AttackAction`'dır.

### REDDEDILEN

`health.TakeDamage(amount);` satırının üstüne eklenmesi reddedildi:

```csharp
if (!health.HasRemaining) return;
```

**KIRILAN:** üç ayrı sebep aynı yönü gösterir, en pahalısı ortadaki.

```
ölçüldü: gövdenin tamamı 0,92 ns -> kâr karede 1,1 milyon çağrı ister
Downed birim hasar almaya devam eder -> "bitirme" yolu kapanır
"kime vurulur" TargetingRules'ın -> kural iki yerde eskir
derleyici: hiçbir şey der  ·  test: Downed_StillAcceptsDamage kırmızı
```

**KAZANIRDI:** hasar almak bir DOSYA/AĞ işi tetikleseydi — ölüm kaydı, sunucuya
bildirim — o zaman erken çıkış nanosaniye değil milisaniye kazandırırdı.

**TEK CUMLE:** Erken çıkış bir performans kararı gibi görünür, oysa burada bir KURAL
kararıdır: yerdekine vurmayı kapatır.

---

## TryRevive()

Düşmüş savaşçıyı ayağa kaldırır. Tam canla değil, maksimumun bir kesriyle —
diriltmek ölümü geri almak değil, riskli bir yatırımdır.

Gövde iki kapıdan geçer ve ikisinin sahibi farklıdır:

```
lifecycle.TryRevive()            ► KİM kalkabilir  (sahibi UnitLifecycle)
health.Max / ReviveHealthDivisor ► NE KADAR canla  (sahibi bu tip)
```

Erken çıkış burada meşrudur ve `TakeDamage`'daki reddedilenle karıştırılmamalı:
kararı kendi vermiyor, sahibine soruyor ve aldığı hayırı çağırana `false` olarak
geri söylüyor. `void` dönen bir metotta aynı çıkış "söylenemez bir hayır" olurdu.

Can sıfırdayken iyileştirildiği için sonuç doğrudan payı verir; `HealingRules`'ın
üst kelepçesi maksimumu aşmayı zaten engelliyor. Oranın neden sabit sayı olmadığı
[↑ ReviveHealthDivisor](#revivehealthdivisor) başlığında.
