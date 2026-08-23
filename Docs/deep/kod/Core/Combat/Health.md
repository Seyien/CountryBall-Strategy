# Health

> **Kaynak:** `Assets/Game/Core/Combat/Health.cs`
> **Ad alanı:** `GridStrategy.Combat` · **Assembly:** `GridStrategy.Combat` (`noEngineReferences: true`)
> **Rol:** Varlık (Entity) — kimliği var, hafızası var, karar vermez uygular

Bir sahibin can **sayısı**. Sahibinin ne olduğunu bilmez ve bilmemelidir: aynı
sınıf bir askerin de bir barakanın da canını tutar. Can bittikten sonra ne
olacağını da bilmez — düşme, yıkılma, ödül, animasyon hep dışarıda karara
bağlanır. Bu bilgisizlik sayesinde can kuralı Unity olmadan sınanabiliyor.

| Üye | Karar | Detay |
|---|---|---|
| `Health(int max)` | maksimum canın pozitifliği burada doğrulanır | [↓](#healthint-max) |
| `Max` | kurucuda konur, bir daha yazılmaz | [↓](#max) |
| `Current` | okunur, yazılmaz — her yazı bir kuraldan geçer | [↓](#current) |
| `HasRemaining` | sayı hakkında bir soru; alan yargısı taşımaz | [↓](#hasremaining) |
| `TakeDamage(int)` | formül dışarıda, yazma burada | [↓](#takedamageint-amount) |
| `Heal(int)` | `TakeDamage`'in aynadaki eşi | [↓](#healint-amount) |

**İlgili anlatılar:** [05-yaşam döngüsü](../../../konular/05-yasam-dongusu.md) ·
[02-assembly duvarı](../../../konular/02-assembly-duvari.md)

---

## Health(int max)

Kurucudaki `max <= 0` kontrolü bir **kopya değildir** ve kalmalıdır. Bu, aşağıda
`TakeDamage` için reddedilen doğrulamayla karıştırılmamalı: "maksimum can pozitif
olmalı" kuralının başka bir sahibi yok — sahibi bu tip.

Kural şu: **bir ön koşul, doğruladığı şeyin sahibi neredeyse orada durur.** Sahip
başkasıysa ön koşulu buraya yazmak onu ikinci kez yazmaktır.

---

## Max

Get-only otomatik property. "Yazılması" diye bir olgu yok: kurucuda bir kez konur
ve biter. Bu yüzden ne bir `set`'i ne de bir `SetMax` metodu var — ona bir yazma
metodu eklemek `Current` için savunulan şeyin tersi olurdu.

---

## Current

### HARİTA: `current` alanına kim yazabiliyor

Alan `private`; dışarıdan ona giden hiçbir ok yok. İçeriden giden iki ok var ve
ikisi de kuralın sahibinden geçiyor:

```
SEÇİLEN
çağıran ─► TakeDamage(n) ─► DamageRules.ResolveRemaining ──┐
çağıran ─► Heal(n) ───────► HealingRules.ResolveRestored ──┤
                                                           ▼
                                                ╔═══════════════╗
çağıran ─► Current ──────── yalnız OKUR ───────►║   current     ║
                                                ╚═══════════════╝
                     ◄── ██ HER YAZI KURALDAN GEÇER ██

REDDEDILEN — `set` eklenirse ÜÇÜNCÜ bir ok doğar
çağıran ─► Current = 3 ────────────────────────────────────┐
                                                           ▼
                                                ╔═══════════════╗
                                                ║   current     ║
                                                ╚═══════════════╝
                     ◄── ██ KURAL DEVREDE DEĞİL ██
kelepçe atlanır, formül atlanır, negatif can yazılabilir hâle gelir
```

### KAPSAM: kural "her get'in set'i olmasın" DEĞİL

Tetikleyici tek soru: **bu üyeye yazmanın bir kuralı var mı?**

Karşı örnek aynı dosyada, iki satır yukarıda: `public int Max { get; }` de
yazılamaz — ama sebebi bu değil. `Max`'ın "yazılması" diye bir olgu yok. Bu blok
yalnız yazmanın bir ön koşulu, bir kelepçesi ya da bir formülü olan üyeleri
ilgilendirir.

### İŞ BÖLÜMÜ: gizlilik ile adlandırma örtüşmez, bölüşür

```
private int current     ► dışarıdan yazmayı İMKÂNSIZ kılar
TakeDamage / Heal       ► içeriden yazmanın MEŞRU yolunu açar ve adıyla bir
                          NİYET taşır ("3 hasar al", "canı 3 yap" değil)
```

Alan gizliliği silinirse adların taşıdığı niyet hiçbir şey ifade etmez; kimse o
yoldan geçmek zorunda kalmaz. Adlı metotlar silinirse geriye meşru bir yazma yolu
**kalmaz** ve çağıran haklı olarak setter'ı geri ister. İkisi birbirinin yedeği
değil: biri bütün yolları kapatıyor, diğeri tek bir yolu açıyor.

### `=>` (get-only) neyi dondurmaz

Burada dönen şey bir değer tipi (`int`) olduğu için fark görünmüyor; okuyucu aynı
krediyi referans tipli bir üyeye taşırsa yanılır. Get-only bir property yalnızca
**o property üzerinden** atamayı keser, dönen nesnenin içini dondurmaz. Canlı
örnek `Combatant`'taki `private readonly Health health` alanıdır: alan kilitli,
ama `health.TakeDamage(5)` tamamen serbest. Aynı yanılgının uzun gerekçesi
[03-tahta sahipliği](../../../konular/03-tahta-sahipligi.md)'nde.

### GARANTİ NEREDE BİTER

`private` **sınıf** duvarında biter, metot duvarında değil: bu dosyaya eklenecek
her yeni metot `current`a doğrudan yazabilir ve derleyici bir şey demez. Bugün iki
yazan var ve ikisi de kurala yönlendiriyor; üçüncüsünü yönlendirecek olan kod
değil, bu belgenin kendisidir.

### REDDEDILEN

```csharp
public int Current { get; set; }
```

**KIRILAN:** sıfırın altına inmeme kuralını uygulamak çağıranın işi olur.

```
"canı 3 yap" diyen çağıran  ─► DamageRules'un formülü atlanır
                            ─► negatif can yazılır
                            ─► HasRemaining ters cevap verir
derleyici: hiçbir şey der   ·   test: hiçbiri kırmızı olmaz
```

**KAZANIRDI:** ham durumu geri yazmak zorunda olan bir çağıran doğsaydı — kayıt,
yükleme ya da senaryo kurulumu — ve kelepçe o yolun sahibinde dursaydı.

**TEK CUMLE:** Okunan üye ile yazılan üye aynı olmak zorunda değil; yazmak bir
niyet taşır, okumak taşımaz.

---

## HasRemaining

Ad bilerek **sayıyı** anlatır, sahibi değil. "Alive" bir alan sözcüğüdür ve bu
tipin bildiği tek şey olan sayı onu taşıyamaz: bir baraka canlı değildir ama canı
**kalmıştır**.

### HARİTA: bu tipin bildiği yer nerede bitiyor

```
SORU                     CEVABIN TEK SAHİBİ        burası bilir mi
────────────────────────────────────────────────────────────────
"kaç can kaldı"          Health.Current            ✓ bilir
"canı kaldı mı"          Health.HasRemaining       ✓ bilir
═══════════════════ ██ BU TİPİN SINIRI ██ ══════════════════════
"ayakta mı"              Structure.IsStanding      ✗ bilmez
"canlı mı / düşmüş mü"   UnitState (Combatant)     ✗ bilmez
"hasarsız mı"            SAHİPSİZ — kimse ölçmüyor ✗ bilmez
```

`IsIntact` çizginin **altında** bir soru sorar ve cevabı üstteki tek veriden
uydurur. Yalan tam burada doğuyor: ad bir bütünlük iddiasıdır, elde ise yalnızca
"sıfırdan büyük mü" vardır.

### KAPSAM: yasak sözcük listesi değil, yargının cinsi

Karşı örnek aynı dosyada: `Current` ve `Max`. İkisi de sayıyı adlandırıyor,
hiçbir alan yargısı taşımıyor. `HasRemaining` de aynı tarafta duruyor: "kalan var
mı" sayının kendisi hakkında bir sorudur.

Ayırt edici soru: **adı yüksek sesle okuduğunda cümlenin öznesi sayı mı, sahibi
mi?** Sahibiyse o ad bu tipe ait değildir.

### İŞ BÖLÜMÜ: adlandırma ile bilgisizlik bölüşür

```
ad disiplini          ► yanlış SORUYU sormayı engeller
bağımlılık yokluğu    ► yanlış CEVABI üretmeyi İMKÂNSIZ kılar
                        (bu tip ne UnitState'i ne StructureState'i tutar;
                         elinde yalnız iki int var)
```

Ad disiplini silinirse — `IsIntact` yazılırsa — kod yine derlenir ve yalnızca
yalan söyler. Bilgisizlik silinirse, yani bu tipe bir durum referansı verilirse,
ad artık **hesaplanabilir** olur ve geriye tek koruma olarak disiplin kalır. Asıl
kilit ikincisidir; birincisi onu görünür tutan şeydir.

### GARANTİ NEREDE BİTER

Bu tip alan yargısını üretemez ama **çağıran** üretebilir: elinde hem
`HasRemaining` hem `State` olan bir kod ikisini kendi kafasına göre birleştirir. O
birleştirmenin doğru yeri canı ve yaşam döngüsünü aynı anda gören iki yerdir —
`Structure` ile `Combatant`.

### REDDEDILEN

```csharp
public bool IsIntact => current > 0;
```

**KIRILAN:** ad, bu tipin hiç ölçmediği bir bütünlük iddiasında bulunur.

```
100 candan 1'e düşen baraka ─► IsIntact hâlâ true der
çağıran "hasar görmemiş" okur ─► onarım sırasını buna göre kurar
derleyici: hiçbir şey der   ·   test: hiçbiri kırmızıya dönmez
```

**KAZANIRDI:** `Health` gerçekten bütünlük taşısaydı — tam can ile eksik can
arasında ayrı bir davranış, bir hasar eşiği ya da bir çatlak durumu olsaydı; o gün
ad `current == Max` diye okunurdu.

**TEK CUMLE:** Bir sayı alan yargısı taşıyamaz: "sağlam" binanın sözcüğüdür,
"canlı" askerin, ikisini de bilmeyen bir sayaç ikisini de diyemez.

**Alternatif:** `IsDepleted`. Seçilmedi: olumlu soran her çağırana bir `!`
borçlandırır, okuma çift olumsuza döner.

---

## TakeDamage(int amount)

Hasarın tek giriş noktası. Girdi doğrulaması bilerek burada **değil**: formül ile
onun geçerli girdi aralığı aynı sahibe aittir (`DamageRules`).

### HARİTA: bir kuralın iki parçası, tek ev

```
DamageRules ╔══════════════════════════════════════════╗
            ║ ön koşul : current >= 0 , amount >= 0    ║
            ║ formül   : Math.Max(0, current - amount) ║
            ╚═══════════════════╤══════════════════════╝
                                │ tek çağrı  ◄── ██ SAHİP ██
Health.TakeDamage ──────────────┘   (kendi ön koşulu YOK)

REDDEDILEN — kopya konursa ön koşul İKİ evde yaşar
DamageRules ┌ ön koşul ┐          Health ┌ ön koşul KOPYASI ┐
            └─────┬────┘                 └────────┬─────────┘
                  │   zırh gelir, "negatif hasar" │
                  │   anlam kazanır, BİRİ genişler│
                  └────────────► ◄───────────────-┘
                       ██ AYRIŞMA NOKTASI ██
            derleyici sessiz · DamageRulesTests yeşil kalır
```

### KAPSAM: doğrulama yasağı değil, sahiplik testi

Karşı örnek aynı dosyada, kurucunun içinde:

```csharp
if (max <= 0) { throw new ArgumentOutOfRangeException(...); }
```

Bu bir kopya **değildir** ve kalmalıdır — sahibi bu tip. Kural: bir ön koşul,
doğruladığı şeyin sahibi neredeyse orada durur.

### İŞ BÖLÜMÜ: iki doğrulama yeri örtüşmez, bölüşür

```
kurucudaki  `max <= 0`      ► nesneyle DOĞAN değişmez
DamageRules `amount < 0`    ► her ÇAĞRIDA gelen argüman
```

Kurucudaki kontrol silinirse `new Health(0)` doğar doğmaz ölü bir nesne üretir;
`HasRemaining` sonsuza dek `false` döner ve bunu hiçbir hasar açıklamaz.
`DamageRules`'taki kontrol silinirse negatif hasar `TakeDamage` üzerinden bir
**iyileştirmeye** dönüşür ve `HealingRules`'ın üst kelepçesini hiç görmeden
maksimumu aşar. İkisi farklı **zamanı** kapatıyor: biri doğumu, diğeri her
çağrıyı.

### `private` burada hiçbir ön koşul sağlamaz

`current` alanının gizli olması yalnızca **dışarıdan** yazmayı keser; bu metoda
gelen `amount` değerinin işaretini denetlemez. Ön koşulu taşıyan tek şey
`ResolveRemaining`'in ilk iki `if`idir.

### REDDEDILEN

```csharp
if (amount < 0)
{
    throw new ArgumentOutOfRangeException(nameof(amount), amount, "Damage amount cannot be negative.");
}

current = DamageRules.ResolveRemaining(current, amount);
```

**KIRILAN:** aynı ön koşul iki dosyada yaşamaya başlar.

```
zırh gelir, "negatif hasar" anlam kazanır ─► kural genişler
buradaki kopya sessizce eskir             ─► Health kuraldan farklı davranır
derleyici: hiçbir şey der   ·   test: DamageRulesTests yeşil kalır
```

**KAZANIRDI:** `Health`'in kendi giriş noktasında kuralınkinden daha **dar** bir
ön koşul gerekseydi — kural negatifi hoş görürken bu yol görmeseydi.

**TEK CUMLE:** Bir kuralın metni ile o kuralın geçerli girdi aralığı aynı
sahibindir; ayırırsan ikisi ayrı hızda eskir.

---

## Heal(int amount)

`TakeDamage`'in aynadaki eşi. Aynı desen: formül dışarıda (`HealingRules`), yazma
burada. Girdi doğrulaması yine burada değil.

Bu metot da sahibinin ne olduğunu bilmez: onarım ile diriltme arasındaki farkı
`Structure` ve `Combatant` ayırır, burası yalnızca sayıyı yukarı yazar.
