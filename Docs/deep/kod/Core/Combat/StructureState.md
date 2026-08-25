# StructureState

> **Kaynak:** `Assets/Game/Core/Combat/StructureState.cs`
> **Ad alanı:** `GridStrategy.Combat` · **Assembly:** `GridStrategy.Combat` (`noEngineReferences: true`)
> **Rol:** Tanım (Profile) — kimliği yok, hafızası yok, karar vermez **adlandırır**

Bir yapının iki durumu: ayakta ya da yıkılmış. Geçişe `StructureLifecycle` karar
verir; bu tip yalnızca durumu adlandırır.

| Üye | Karar | Detay |
|---|---|---|
| `enum StructureState` | `UnitState` paylaşılmadı — geçiş grafikleri farklı | [↓](#enum-structurestate) |
| `Destroyed` | sıfırıncı değer bilerek "yıkık" | [↓](#destroyed) |
| `Rubble` | **eklenmedi** — hiçbir kuralı değiştirmiyor | [↓](#rubble) |
| `Standing` | ayakta; hedeflenebilir, canı azalır, onarılabilir | [↓](#standing) |

**İlgili anlatılar:** [05-yaşam döngüsü](../../../konular/05-yasam-dongusu.md) ·
[06-sonuç enumları](../../../konular/06-sonuc-enumlari.md)

---

## enum StructureState

**AYNI DEĞERLER, AYNI GEÇİŞLER DEMEK DEĞİLDİR.**

Neden `UnitState` değil: bir baraka **düşmez**. Düşme durumu (`UnitState.Downed`)
"ölü ama kurtarılabilir" demektir ve tek var olma sebebi diriltme penceresidir.
Yapıda böyle bir pencere yok: yıkılan bina onarılmaz, yeniden İNŞA edilir — ve
yeniden inşa, bu tipin bir geçişi değil, yeni bir yapı nesnesidir.

Neden `bool isDestroyed` değil: bugün iki değer var, ama ayrım enum'da durduğu
sürece üçüncü bir durum ([↓ Rubble](#rubble)) tartışılabilir kalır; bool ile o
tartışma hiç yazılamaz, yalnızca iç içe if'lere dönüşür.

### HARİTA: iki yaşam döngüsünün GEÇİŞ grafiği

Karar değerlerin sayısında değil, aralarındaki okların ŞEKLİNDE:

```
  UnitLifecycle — üç durum ve GERİ DÖNEN bir ok
    Alive ──can bitti──► Downed ──10 sn dolar──► Dead ──► (temizlik)
      ▲                    │
      └──── TryRevive ─────┘        ◄── >> KURTARMA PENCERESİ <<
            Downed'ın var olma sebebi TAM OLARAK bu geri ok

  StructureLifecycle — iki durum, geri dönen ok YOK
    Standing ──can bitti──► Destroyed ──► (temizlik)
      ▲                        │
      └── böyle bir ok YOK ────┘   ◄── >> YOKLUĞU KURALDIR <<
            yıkık bina onarılmaz; yeniden İNŞA edilir ve o da
            bu tipin bir geçişi değil, YENİ bir Structure nesnesidir

  Ortak enum seçilseydi eşleme şöyle olurdu:
    Standing → Alive ✓   Destroyed → Dead ✓   Downed → >> KARŞILIĞI YOK <<
    ve o karşılıksız değer her switch'te bir dal açmaya devam ederdi.
```

### KAPSAM: paylaşım yasağı DEĞİL — geçiş testi

Ayırt edici soru: **iki tip aynı DEĞERLERİ mi paylaşıyor, aynı GEÇİŞLERİ mi?**
Yalnız ikincisi ortak tipi hak eder.

Karşı örnek aynı ad alanında ve tam da bu iki tip arasında: `Health` PAYLAŞILIYOR
ve bu doğrudur. Bir barakanın canı ile bir askerin canı aynı sınıfla tutuluyor —
çünkü `Health`'in bir geçiş grafiği YOK, yalnız bir sayısı var; sayının "düşmüş"
hâli olmaz. `Structure`'ın XML özeti bunu sınanmış bir iddia olarak yazıyor. Yani
bu karar "yapılar ve birimler hiçbir tipi paylaşmasın" demiyor; geçişi olan tipin
paylaşılamayacağını söylüyor.

### İŞ BÖLÜMÜ: iki enum örtüşmez, bölüşür

```
UnitState        ► kurtarma penceresi OLAN yaşam döngüsü (3 değer)
StructureState   ► penceresi OLMAYAN yaşam döngüsü      (2 değer)
ikisini birden okuyan tek yer: TargetingRules (aşırı yükleme çifti)
```

`StructureState` silinip `UnitState` paylaşılırsa yapı üzerindeki her switch asla
çalışmayan bir `Downed` dalı taşır ve `CanBeAttacked(state)` kurtarma penceresi
kuralını binalara uygular. `UnitState` silinip `StructureState` paylaşılırsa
`Downed` diye bir durum kalmaz ve kurtarma penceresi HİÇ ifade edilemez — birim
tarafındaki bütün tasarım çöker. İkisi birbirinin dar ya da geniş sürümü değil;
farklı grafikler.

### ÖDENEN BEDEL açıkça: aşırı yükleme çifti

Bu ayrımın faturası `TargetingRules`'ta duruyor: `CanBeAttacked(UnitState)` ile
`CanBeAttacked(StructureState)` iki ayrı metot. Bedel budur, ve bilerek ödendi —
aşırı yüklemeyi derleyici İSTER ve gözle görülür; ölü dalı kimse istemez ve
görünmez.

**Bedelin tamamı bu çift de değildir:** hedefleme kuralının yapı tarafı saldırıda
ikizlendi, diriltmede **ikizlenmedi.** Yapı dirilmez — `Structure.TryRepair` bir
onarımdır, durum geçişi değil — ve bu asimetri `TargetingRules` içinde bir
REDDEDILEN bloğu olarak yazılı. Tek enum seçilseydi bu ayrım hiç sorulamazdı:
yapılar birimlerin diriltme kuralını sessizce devralırdı.

### REDDEDILEN

Bu dosya hiç doğmaz, yapılar `UnitState`'i yeniden kullanır:

```csharp
public UnitState State { get; private set; }   // StructureLifecycle içinde
```

**KIRILAN:** yapı üzerindeki HER switch, asla çalışamayacak bir `Downed` dalı açar.

```
dal `throw` olur         -> hiçbir testin geçmediği ölü kod
dal "ayakta"ya düşer     -> yıkılmış baraka sağlam çizilir
CanBeAttacked(UnitState) yapıları da yönetir -> `state != Dead`
kurtarma penceresi kuralını binalara uygular
derleyici: hiçbir şey der  ·  test: ölü dal hiçbir testte geçilmez
```

**KAZANIRDI:** yapılar `Downed`'a denk bir ARA duruma kavuşsaydı — "yıkılan baraka
yanar, itfaiye yetişirse ayağa kalkar". O gün tek enum doğru olurdu.

**KARSILASTIRMA:**

| seçenek | değer sayısı | sonucu |
|---|---|---|
| `UnitState` paylaş | üç | her switch'te asla çalışmayan `Downed` dalı |
| `bool isDestroyed` | iki | üçüncü durum tartışılamaz, iç içe if doğar |
| `StructureState` | iki | `TargetingRules` iki dil konuşur (ödenen bedel) |

**TEK CUMLE:** İki tip aynı DEĞERLERİ taşıyabilir ama aynı GEÇİŞLERİ taşımıyorsa
enum ortak olamaz; baraka düşmez, yıkılır.

---

## Destroyed

Yıkılmış. Hedeflenemez, saldırmaz, onarılamaz; geriye yalnızca enkaz temizliği
kalır.

**ATANMAYI UNUTULAN ALAN, TAHTADA BİR ŞEY ÜRETMEMELİ.**

Sıfırıncı değer BİLEREK "yıkık". Kural şu: atanmayı unutulmuş bir alan, olabilecek
en ZARARSIZ şeye benzemeli — ve tahtada zararsız olan şey, hiç olmayan şeydir.
Enkaz, "burada bir şey yok"a en yakın değerdir.

### HARİTA: atanmamış değerin tahtaya çıkışı

```
  SEÇİLEN                     REDDEDILEN
  ad          sayı            ad          sayı
  ───────────────────         ───────────────────
  Destroyed    0 ◄── default  Standing     0 ◄── >> default <<
  Standing     1              Destroyed    1

  REDDEDILEN sıralamada zincir:
    atanmamış alan / `new StructureState[9]`  -> hepsi Standing
      -> tahtada dokuz SAĞLAM bina belirir
      -> TargetingRules.CanBeAttacked(Standing) EVET der
      -> saldırı emri doğar, hücre dolu sayılır, üretim varsayılır
      -> hiçbiri gerçek değil ve kimse tek satır yazmadı
    derleyici: hiçbir şey der  ◄── >> SESSİZ <<
    test: Default_StructureStateValue_IsDestroyedNotStanding kırmızı

  SEÇİLEN sıralamada aynı zincir hiç başlamaz: enkaz "burada bir şey
  yok"a en yakın değerdir; hedeflenmez, saldırmaz, onarılmaz.
```

### KAPSAM: kural KONUMA özeldir, cevabı ALANA göre değişir

Sıfırıncı değere "en zararsız" olanı koymak repo genelinde geçerli bir kural; ama
zararsızın NE olduğu her alanda ayrı cevaplanır:

```
  tip              sıfırıncı    "en zararsız" burada ne demek
  ─────────────────────────────────────────────────────────────
  Team             None         hiçbir tarafa ait olmamak
  StructureState   Destroyed    tahtada hiç olmamaya en yakın olmak
```

Karşı örnek aynı dosyada, birkaç satır aşağıda: `Standing` üyesinin üstünde böyle
bir blok YOKTUR ve olmamalıdır. Kural değerin ADINA değil sıfırıncı KONUMA bağlı;
`Standing`'i 1 yerine 5 yapmak hiçbir şeyi değiştirmez, çünkü hiçbir alan sessizce
5'e düşemez.

### İŞ BÖLÜMÜ: sıralama ile TEST örtüşmez, bölüşür

```
üyelerin sırası    ► bugünkü davranışı sağlar
Default_Structure  ► kararı GELECEKTE korur; biri değerleri
StateValue_...     alfabetik sıralar ya da araya yeni bir üye
testi                eklerse kırmızıya döner
```

Sıralama silinirse tahta sahte binalarla dolar. Test silinirse bugün hiçbir şey
olmaz — ve tam bu yüzden tehlikelidir: sıralamayı bozan bir sonraki düzenleme
hiçbir uyarı üretmez. Kararı taşıyan sıralama, kararı KORUYAN testtir.

### ADIN KENDİSİ KORUMAZ

Koruma "Destroyed" sözcüğünden değil, o üyenin sayısal olarak sıfır olmasından
geliyor. `Destroyed = 3, Standing = 0` yazılsaydı ad aynı kalır, koruma tamamen
kaybolurdu. Aynı sebeple `Destroyed = 0` diye açıkça yazmak da bir şey eklemez;
karar bu yüzden dosyanın başında değil, tam bu üyenin üstünde duruyor.

### REDDEDILEN

```csharp
Standing,
Destroyed
```

**KIRILAN:** `default(StructureState)` artık "ayakta" demek olur.

```
sıfırıncı değer gerekçesinin tamamı Team.md'de yazılı; buradaki FARK,
atanmamış değerin tahtada SAĞLAM bir bina üretmesi — hedeflenir,
saldırı emri doğurur, üretim yaptığı varsayılır.
derleyici: hiçbir şey der  ·  test:
Default_StructureStateValue_IsDestroyedNotStanding kırmızıya döner
```

**KAZANIRDI:** `Team` ile aynı — sıfır bir güvenlik değil SIKLIK kararı olsaydı;
tahtadaki yapıların çoğu her an ayakta.

**TEK CUMLE:** Enkaz, "burada bir şey yok"a en yakın değerdir; atanmayı unutulan
alan en zararsız şeye benzemelidir.

---

## Rubble

**BİR DEĞER, EN AZ BİR KURALI DEĞİŞTİREREK YERİNİ HAK EDER.**

Üçüncü bir `Rubble` değeri **eklenmedi**. `IsReadyForCleanup` bayrağı yerine bir
durum olması reddedildi.

### HARİTA: `Rubble` hangi satırda `Destroyed`'dan ayrılıyor

```
  KURAL                     Destroyed   Rubble   ayrışıyor mu
  ─────────────────────────────────────────────────────────────
  hedeflenebilir mi         hayır       hayır    ✗
  saldırabilir mi           hayır       hayır    ✗
  onarılabilir mi           hayır       hayır    ✗
  hücreyi kapatır mı        (UnitGrid bu değeri hiç sormuyor)  ✗
  ─────────────────────────────────────────────────────────────
                   ◄── >> HİÇBİR SATIRDA AYRIŞMIYOR <<
  ayrışan tek şey: "artık kaldırılabilirim" -> bu bir DURUM değil,
  bir İSTEK; ve isteği taşıyan yer zaten var: IsReadyForCleanup

  Üçüncü değer eklenseydi her switch şu şekli alırdı:
    case Destroyed: ... ┐
    case Rubble:    ... ┘ ◄── >> İKİ DAL, AYNI GÖVDE <<
  ve bir gün biri güncellenip diğeri unutulurdu.
```

### KAPSAM: "enum küçük kalsın" DEĞİL

Karşı örnek aynı dosyada, tam bu değerin iki komşusu: `Destroyed` ile `Standing`
yukarıdaki tablonun HER satırında ayrışıyor — biri hedeflenir, saldırır, onarılır;
diğeri hiçbirini yapmaz. İkisinin ayrı değer olması bu yüzden bedava değil, hak
edilmiş. Kural şu: **bir değer, en az bir KURALIN cevabını değiştiriyorsa enum'a
girer.**

Aynı dosyanın yukarısındaki `UnitState` reddiyle de karıştırılmasın: orada üçüncü
değer (`Downed`) ASLA ULAŞILAMADIĞI için reddedildi — ölü dal. Burada `Rubble`
ulaşılabilir ama `Destroyed`'dan AYIRT EDİLEMEZ — kopya dal. İki farklı kusur, aynı
ilaç.

### İŞ BÖLÜMÜ: enum ile bayrak örtüşmez, bölüşür

```
StructureState (enum)  ► birbirini dışlayan, kuralları FARKLI evre
IsReadyForCleanup      ► tek yönlü, geri dönmeyen, kural
                         değiştirmeyen İSTEK
```

Enum bir değer kaybederse ayakta ile yıkık aynı şey olur. Bayrak silinip enum'a
taşınırsa yukarıdaki kopya dal doğar. Aynı karar `UnitLifecycle`'da da verildi
(`Dead` + `IsReadyForCleanup`) ve bu, deseni bu dosyaya özel olmaktan çıkarıp yaşam
ekseninin ortak kuralı yapıyor.

### DÖNÜŞ YOLU AÇIK BIRAKILDI

Bu red kalıcı bir yasak değil, bugünkü ölçüme bağlı bir karar: yukarıdaki tablonun
herhangi bir satırında iki sütun ayrıştığı gün `Rubble` bir DURUM olmayı hak eder.
Tetikleyici de adıyla yazılı — yıkık bina hücreyi kapatırken temizlenmiş enkazın
geçişe açılması; o gün `UnitGrid` bu değeri sormaya başlar ve bayrak yetmez.

### REDDEDILEN

`IsReadyForCleanup` bayrağı yerine üçüncü bir değer:

```csharp
Destroyed,
Rubble,
Standing
```

**KIRILAN:** üçüncü değer hiçbir KURALI değiştirmez — yıkık da enkaz da saldırmaz,
hedeflenmez, onarılmaz.

```
her switch'te iki dal aynı gövde -> biri güncellenir, biri unutulur
aynı karar UnitLifecycle'da da verildi: Dead + IsReadyForCleanup
derleyici: hiçbir şey der  ·  test: hiçbiri kırmızıya dönmez
```

**KAZANIRDI:** enkaz gerçekten ayrı bir kural taşısaydı — yıkık bina hücreyi
kapatırken temizlenmiş enkaz geçişe açılsaydı. O gün fark bayrakta değil durumda
yaşar, `UnitGrid` bu değeri sorardı.

**TEK CUMLE:** "Artık kaldırılabilirim" bir İSTEKtir, bir durum değil; istekler
bayrakla, durumlar enum'la yazılır.

---

## Standing

Ayakta. Hedeflenebilir, canı azalır, onarılabilir.

Üstünde bir gerekçe bloğu yok ve olmamalı: kural değerin ADINA değil sıfırıncı
KONUMA bağlı, `Standing` de sıfırıncı değil ([↑ Destroyed](#destroyed)).
