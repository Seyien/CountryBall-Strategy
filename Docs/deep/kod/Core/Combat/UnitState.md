# UnitState

> **Kaynak:** `Assets/Game/Core/Combat/UnitState.cs`
> **Ad alanı:** `GridStrategy.Combat` · **Assembly:** `GridStrategy.Combat` (`noEngineReferences: true`)
> **Rol:** Tanım (Profile) — kimliği yok, hafızası yok, karar vermez **adlandırır**

Bir birimin yaşam döngüsündeki üç durumu. Geçişlere `UnitLifecycle` karar verir; bu
tip yalnızca durumu adlandırır.

Neden bool değil: bu tip doğmadan önce durumu tutan şey bir bool'du ve üçüncü
durumu ifade edemiyordu. "Ölü ama 10 saniye içinde diriltilebilir" ne canlıdır ne
de kalıcı ölü — ve hasar almaya DEVAM etmesi gerekir. Bool'la yazılsaydı bu kural
sessizce kaybolurdu.

| Üye | Karar | Detay |
|---|---|---|
| `enum UnitState` | iki bool reddedildi — geçersiz hâl tipte var olmamalı | [↓](#enum-unitstate) |
| `Alive` | ayakta; hedeflenebilir, canı azalır | [↓](#alive) |
| `Downed` | düşmüş ama kurtarılabilir; hasar almaya DEVAM eder | [↓](#downed) |
| `Dead` | kalıcı ölü; geriye yalnız ceset temizliği kalır | [↓](#dead) |

**İlgili anlatılar:** [05-yaşam döngüsü](../../../konular/05-yasam-dongusu.md) ·
[03-tahta sahipliği](../../../konular/03-tahta-sahipligi.md)

> Kodda tipin üstündeki `DERİN ANLATIM: Docs/deep/05-yasam-dongusu.md`
> yönlendirmesi yerinde bırakıldı.

---

## enum UnitState

**GEÇERSİZ HÂL TİPTE VAR OLMAMALI.**

### HARİTA: iki bayrak DÖRT hücre açar, üçü anlamlı

İki bool İKİ ayrı depolama hücresidir ve C#, iki alan arasında "aynı anda doğru
olamazlar" diye bir değişmez tanımlamanın yolunu vermez. Çarpım tablosu tipin
KENDİSİNDE yaşamaya başlar:

```
                     isDowned = false      isDowned = true
                   ┌──────────────────┐  ┌──────────────────┐
  isAlive = true   │ Alive            │  │ ???              │
                   │                  │  │ ◄── ██ ANLAMSIZ ██
                   └──────────────────┘  └── ama YAZILABİLİR ┘
  isAlive = false  ┌──────────────────┐  ┌──────────────────┐
                   │ Dead             │  │ Downed           │
                   └──────────────────┘  └──────────────────┘

  enum UnitState :  Alive    Downed    Dead
                    └─ ÜÇ değer; dördüncü hücre TİPTE HİÇ YOK ─┘
```

Fark bir YASAK değil bir İMKÂNSIZLIK: anlamsız hâl engellenmiyor, hiç doğmuyor.
Aynı ayrım `Battle`'daki tahta sahipliği kararında da yazılı ve orada da kararı
veren cümle aynıdır — **engellenen bir şey unutulabilir, doğmayan şey
unutulamaz.** Uzun hâli
[03-tahta sahipliği](../../../konular/03-tahta-sahipligi.md)'nde.

### KAPSAM: kural "bool kullanma" DEĞİLDİR

Tetikleyici tek soru: **iki bayrak birbirini DIŞLIYOR mu?**

```
  isAlive × isDowned     dışlıyor              -> enum  (bu tip)
  IsReadyForCleanup      dışlayacağı ikinci
                         bayrak YOK            -> bool DOĞRU
```

Karşı örnek aynı ad alanında, `UnitLifecycle.cs` içinde:
`public bool IsReadyForCleanup { get; private set; }` bir bool'dur ve öyle
KALMALIDIR. Tek bir olguyu iki değerle söyler, ikinci bir bayrakla çarpılmaz, bir
kez açıldıktan sonra geri de dönmez — çarpım tablosu hiç doğmaz. Onu bu enum'a
dördüncü değer olarak eklemenin reddi ayrı bir yerde,
[StructureState.md](StructureState.md#rubble)'deki `Rubble` reddinde yazılı:
"istekler bayrakla, durumlar enum'la yazılır".

### İŞ BÖLÜMÜ: enum ile bayrak örtüşmez, bölüşür

Yaşam ekseni iki mekanizma taşıyor ve her biri farklı ŞEKİLDE bir olguyu
kapatıyor:

```
  UnitState (enum)     birbirini dışlayan, iki yönlü gidilebilen evre
                       Alive → Downed → Dead ve Downed → Alive (geri)
  IsReadyForCleanup    tek yönlü, bir kez açılan, geri dönmeyen İSTEK
```

Enum silinip iki bool'a dönülürse yukarıdaki anlamsız hücre yazılabilir olur ve
geçerlilik programcının hafızasına devrolur. Bayrak silinip enum'a dördüncü değer
olarak eklenirse her switch'te iki dal aynı gövdeyi taşır — `Dead` ile
"temizlenebilir" hiçbir KURALDA ayrışmıyor — ve biri güncellenip diğeri unutulur.

### EKSİK DALI DERLEYİCİ NE KADAR GÖSTERİR

Okuyucu korumayı derleyiciye yazabilir ("enum kullanırsam eksik dalı söyler");
yalnızca yarısı doğru:

```
  switch İFADESİ (expression)   eksik dal -> CS8509 uyarısı   ✓
  switch DEYİMİ  (statement)    eksik dal -> sessiz            ✗
  iç içe if                     eksik dal -> sessiz            ✗
```

Bu tipin asıl kazancı uyarı DEĞİL, geçersiz hâlin yazılamamasıdır; uyarı bir
ikramiyedir ve tek bir sözdiziminde gelir. Aşağıdaki KIRILAN satırı da tam bunu
söylüyor.

### REDDEDILEN

```csharp
public bool isAlive;
public bool isDowned;
```

**KIRILAN:** iki bayrak dört kombinasyon üretir ve dördüncüsü hiçbir şey demez.

```
isAlive && isDowned      -> derlenir, anlamı yok, kimse yakalamaz
switch yerine iç içe if  -> unutulan dalı derleyici gösteremez
derleyici: hiçbir şey der  ·  test: geçersiz hâl için test bile yazılamaz
```

**KAZANIRDI:** durum gerçekten iki değerliyse ve üçüncüsünün eklenmeyeceği
tasarımca kesinse — o gün enum bir tipi boşuna eklemiş olurdu.

**KARSILASTIRMA:**

| seçenek | hâl sayısı | sonucu |
|---|---|---|
| tek bool | iki değer | üçüncü durum HİÇ yazılamaz |
| iki bool | dört hâl | biri anlamsız; geçerlilik akılda tutulur |
| `enum UnitState` | üç değer | geçersiz hâl TİPTE yok, eksik dal görünür |

**TEK CUMLE:** Enum geçersiz durumu YAZILAMAZ kılar; bayraklar onu yazılabilir
bırakıp doğruluğu programcının hafızasına devreder.

---

## Alive

Ayakta. Hedeflenebilir, canı azalır.

---

## Downed

Düşmüş ama kurtarılabilir. Hedeflenmeye ve hasar almaya **devam eder** — düşman ya
geri sayımın dolmasını bekler ya da gidip bitirir.

Bu değerin var olma sebebi bir **penceredir**: `Downed → Alive` geri oku. O
pencereyi atlayan kısayolun neden reddedildiği
[UnitLifecycle.md](UnitLifecycle.md#onhealthdepleted)'de.

Aynı değer, `Combatant`'a bir `IsAlive` kısayolu eklenmesini de reddettiren
satırdır: bool'un kapasitesi iki, taşınan ayrım üç
([Combatant.md](Combatant.md#state)).

---

## Dead

Kalıcı ölü. Diriltilemez; geriye yalnızca ceset temizliği kalır.

"Temizlenebilir" bu enum'un dördüncü değeri **değildir** — o bir İSTEKtir ve
`UnitLifecycle.IsReadyForCleanup` bayrağında yaşar
([↑ enum UnitState](#enum-unitstate)).
