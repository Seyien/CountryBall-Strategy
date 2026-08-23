# PointerGesture

> **Kaynak:** `Assets/Game/Core/PointerGesture.cs`
> **Ad alanı:** `GridStrategy.Core` · **Assembly:** `GridStrategy.Core` (`references: []`, `noEngineReferences: true`)
> **Rol:** Durum Makinesi (State Machine) — kimliği var, hafızası var, KARAR VERİR

Basılı tutulan bir işaretçinin TIKLAMA mı yoksa SÜRÜKLEME mi olduğuna karar veren
saf durum makinesi.

Bu dosyanın var olma sebebi `BoardAdapter.cs`'in başında yazılı EŞİK'tir:
"dördüncü kural geldiği gün Core'a tıklamayı niyete çeviren bir komut sahibi
çıkmalı." Yerleştirme kipi bütün bir GİRİŞ KİPİ ekledi — sürükle-bırak ve
tıkla-bırak aynı anda yaşayacak — ve eşik aşıldı. Cevap `BoardAdapter`'ı büyütmek
değil, kararı dışarı çıkarmaktı: iki giriş şeklini ayıran tek soru ("basılı
tutulurken işaretçi gerçekten hareket etti mi?") Unity'siz sorulabilen bir
sorudur, dolayısıyla Unity'siz sorulur.

Neyi BİLMEZ: hangi düğmeye basıldığını, kaç saniyedir basılı olduğunu, hücreyi,
tahtayı, kamerayı, yerleştirilecek yapıyı, hayaletin nerede durduğunu. Hepsi
çağıranın işidir.

> **BİRİM UYARISI** — çağıranın uyması gereken tek sessiz sözleşme: `x`, `y` ve
> `dragThreshold` AYNI birimde olmalıdır. Tip bunların piksel mi, dünya birimi
> mi, ekran oranı mı olduğunu bilmez ve bilemez; karıştırıldığı gün kod derlenir,
> testler yeşil kalır ve yalnızca eşik yanlış yerde durur. Sözleşmeyi tipe
> yazdıramadığımız için buraya yazıyoruz.

`GridDistance` ile farkı tam olarak şurada: o yalnız ÖLÇER ve hafızasızdır ("aynı
dört sayı her zaman aynı cevabı verir"); bu tip ölçüyü bir EŞİKLE karşılaştırıp
bir KİP'e çevirir ve o kipi HATIRLAR. Aynı ailenin iki ucu.

| Üye | Karar | Detay |
|---|---|---|
| `PointerPhase` | beş kip; sıfırıncı değer `Idle` — ve bu bir RET değeri DEĞİL | [↓](#pointerphase) |
| `PointerGesture` (tip) | girdi dışarıdan gelir, tip cihazı tanımaz; kip bir AŞAMADIR | [↓](#pointergesture-tip) |
| `dragThresholdSquared` | karekök alınmaz: tipin eline ihtiyacı olmayan bir sayı verilmez | [↓](#dragthresholdsquared) |
| `PointerGesture(float)` | NaN ve negatif fırlatır, sıfır GEÇERLİ; kırpma yok | [↓](#pointergesturefloat-dragthreshold) |
| `Phase` | kipin tek yazarı bu tiptir | [↓](#phase) |
| `IsActive` | türetilmiş bool; ikinci bir kaynak doğmaz | [↓](#isactive) |
| `Press(float x, float y)` | zaten basılıyken gelen Press jesti YENİDEN BAŞLATIR | [↓](#pressfloat-x-float-y) |
| `MoveTo(float x, float y)` | `Dragging`'den geri dönüş yok — bir if ile değil, YAPI ile | [↓](#movetofloat-x-float-y) |
| `Release(float x, float y)` | iki ayrı sonuç kipi; bırakma konumu da ölçülür | [↓](#releasefloat-x-float-y) |
| `Reset()` | basma noktasını da temizler | [↓](#reset) |
| `ExceedsDragThreshold(float, float)` | ölçüm hep basma noktasından, karşılaştırma KESİN büyüktür | [↓](#exceedsdragthresholdfloat-x-float-y) |

**İlgili anlatılar:** [07-tıklamadan eyleme](../../konular/07-tiklamadan-eyleme.md) ·
[02-assembly duvarı](../../konular/02-assembly-duvari.md)

---

## PointerPhase

Bir işaretçi jestinin içinde bulunduğu kip. Beş değerin ikisi GEÇİCİ (`Pressed`,
`Dragging`), ikisi SONUÇ (`ClickReleased`, `DragReleased`), biri de yokluk
(`Idle`).

Sonuç değerleri, çağıran bir sonraki `Press`'i yapana ya da `Reset` diyene kadar
okunabilir hâlde KALIR — böylece kararı üreten kare ile kararı tüketen kare aynı
kare olmak zorunda değildir.

### SIFIRINCI DEĞER BURADA BİR RET DEĞİL

`Idle = 0` bilinçli: `default` ile doğan bir alan "hiç basılmadı" der ve bu, yeni
kurulmuş bir `PointerGesture`'ın gerçek hâliyle birebir aynıdır.

`MoveOutcome` ve `AttackOutcome` sıfırıncı değeri bir RET'e ayırır; oradaki
gerekçe "unutulmuş bir atama sessizce KABUL'e dönüşmesin" idi ve o gerekçe burada
yok — burada unutulmuş atamanın doğal karşılığı zaten "jest yok"tur. Karşılıklı
gerekçe [`MoveOutcome.md`](MoveOutcome.md#rejectedinvaliddestination)'de.

### NEDEN SAHİBİNİN DOSYASINDA

Projenin geri kalanında bir tip bir dosya. Sebep kavramsal: `MoveOutcome` ve
`AttackOutcome`'ın BİRDEN ÇOK üreticisi ve assembly sınırını aşan tüketicileri
var; `PointerPhase`'in ise tek üreticisi `PointerGesture` ve tipin bütün imzaları
bu enum'u döndürüyor — ayrı dosyada bekleyen bir yarısı yok.

---

## PointerGesture (tip)

### GİRDİ DIŞARIDAN GELİR: TİP CİHAZI TANIMAZ

#### HARİTA: motorun üç sorusu, bu tipin üç metodu

Çeviri `BoardAdapter`'da yapılıyor ve oradaki "üçlü giriş okuma" notu bu tablonun
sol yarısını anlatıyor:

```
┌─ GridStrategy.Unity ─ BoardAdapter ─────────────────────────┐
│  Input.GetMouseButtonDown(0)  basıldığı TEK kare  ──┐       │
│  Input.GetMouseButton(0)      basılı HER kare     ──┼──┐    │
│  Input.GetMouseButtonUp(0)    bırakıldığı kare    ──┼──┼─┐  │
└──────────────────────────────────────────────────── │  │ │──┘
       ▼ duvarı yalnız DÖRT FLOAT geçer               │  │ │
═══════════════ ASMDEF DUVARI ═══════════════════════ │  │ │
noEngineReferences = true  ◄── DURUŞ NOKTASI: Input,   │  │ │
Camera, Time, Vector2 bu çizgiyi geçemez               │  │ │
┌─ GridStrategy.Core ─ bu dosya ──────────────────────▼──▼─▼─┐
│  Press(x, y)          MoveTo(x, y)         Release(x, y)   │
└────────────────────────────────────────────────────────────┘
```

Duvarın altında "sol fare düğmesi" diye bir kavram yok; dokunmatik, atanmış tuş ya
da kayıttan oynatma aynı üç metoda aynı şekilde girer.

#### REDDEDILEN

Tip girdiyi kendisi okur:

```csharp
using UnityEngine;
public PointerPhase Sample()   // Input.GetMouseButton... + mousePosition
```

**KIRILAN:** Core'un asmdef'indeki `noEngineReferences = true` kırılır.

```
EditMode'da sınanamaz -> eşiği sınamak için fare sürüklemek gerekir
jest "sol fare düğmesi" -> dokunmatik ve atanmış tuş dışarıda kalır
derleyici: asmdef ayarı değişmeden derlenmez  .  test: koşamaz
```

**KAZANIRDI:** proje tek bir giriş cihazına kilitlenseydi ve bu tipin tek çağıranı
olsaydı — o gün üçlü çağrıyı her çağırana yazdırmak tören olurdu.

#### KAPSAM: kural "dışarıdan bilgi alma" DEĞİL

Duvarı geçemeyen şey VERİ değil, CİHAZ BİLGİSİ:

```
dört float (x, y, eşik)   -> hangi cihazdan geldiği belli değil ► geçer
Input/Camera/Time çağrısı -> cihazı ve motoru tipe yazar        ► geçmez
```

KARŞI ÖRNEK aynı dosyada, kurucunun kendisi: `dragThreshold` da dışarıdan geliyor
ve içeride hiçbir sabit yok. Üstelik bu tip o sayının BİRİMİNİ bile bilmiyor —
yukarıdaki "BİRİM UYARISI" tam olarak bunu söylüyor. Yani dışarıdan sayı almak bu
tipin deseni; reddedilen şey sayıyı KENDİSİNİN toplaması.

#### İŞ BÖLÜMÜ: çevirmen ile karar verici

```
motorun kare düzenini okumak ► BoardAdapter (üç Input sorgusu)
kipi belirlemek              ► bu tip (üç metot, tek durum makinesi)
```

Silinirse ne kırılır: çeviri bu tipe girerse asmdef ayarı değişmeden dosya
DERLENMEZ ve EditMode testleri koşamaz — eşiği sınamak için gerçek bir fare
sürüklemek gerekir. Karar `BoardAdapter`'a geri dönerse "bu tıklama mıydı" sorusu
Unity'siz sorulamaz hâle gelir ve `PointerGestureTests`'in yirmi dört testi
taşınacak yer bulamaz. İkisi aynı işi iki kez yapmıyor: biri kareyi okuyor, öteki
kipi kuruyor.

**TEK CUMLE:** Karar bir CİHAZ'ı tanımaz, tıpkı ölçünün bir VARLIK'ı tanımaması
gibi; girdi dışarıdan gelir, kip içeride doğar.

---

### KİP BİR AŞAMADIR: BEŞ HÂL TEK BOOL'A SIĞMAZ

#### HARİTA: durum makinesinin geçiş tablosu

Satır = içinde bulunulan kip, sütun = gelen olay, hücre = yeni kip. Bu tablo tipin
TAMAMIDIR; üç metot bu tablodan başka bir şey yapmaz.

```
kip ↓ / olay →   Press      MoveTo(x,y)            Release(x,y)
──────────────   ────────   ────────────────────   ──────────────────────
Idle             Pressed    — yok sayılır          — yok sayılır
Pressed          Pressed*   aşıldı  ► Dragging     aşıldı  ► DragReleased
                            aşılmadı ► Pressed     aşılmadı ► ClickReleased
Dragging         Pressed*   HER ZAMAN Dragging ◄─① DragReleased
ClickReleased    Pressed*   — yok sayılır          — yok sayılır
DragReleased     Pressed*   — yok sayılır          — yok sayılır

* her kipten Press YENİ bir jest başlatır, fırlatmaz
Reset: hangi kip olursa olsun ► Idle
```

① YASAK GEÇİŞ, ve bir `if` ile değil YAPI ile: `Dragging` satırında "Pressed"
yazan hiçbir hücre yok. Eşiğin içine geri dönen bir işaretçi hâlâ sürüklüyordur; o
hücre yasaklanmadı, HİÇ YAZILMADI.

Tablonun sağ alt köşesindeki iki SONUÇ kipi (`ClickReleased`, `DragReleased`)
kendiliğinden değişmez: bir sonraki `Press` ya da `Reset` gelene kadar okunabilir
kalır. Kararı üreten kare ile tüketen kare bu yüzden aynı olmak zorunda değil.

Tek bir `bool` bu tablonun yalnız bir sütununu taşıyabilir: "sürüklüyor mu". `Idle`
ile `ClickReleased` ikisi de `false`'a düşer ve tablonun üç satırı tek satıra
çöker.

#### REDDEDILEN

Kip yerine tek bir bayrak:

```csharp
public bool IsDragging { get; private set; }
```

**KIRILAN:** beş kipin beşi tek `bool`'a sığmaz; `Idle` ile `ClickReleased` aynı
değere düşer ve bayrak bir jestin BİTTİĞİNİ söyleyemez.

```
çağıran ikinci bayrak tutar -> sıfırlanmadığı gün hayalet takılı kalır
derleyici: hiçbir şey der  .  test: _StartsANewGesture yazılamaz
```

**KAZANIRDI:** yalnızca sürükleme desteklenseydi — tıklama diye bir giriş şekli
olmasaydı enum tek bit taşıyan gereksiz bir tören olurdu.

#### KAPSAM: kural "bool kullanma" DEĞİL

Ölçüt: değer bir AŞAMA mı, yoksa aşamalardan TÜRETİLEN bir soru mu?

```
"hangi aşamadayım"   beş hâl, geçiş kuralları var  ► enum
"şu an basılı mı"    iki hâl, kipten türetilebilir ► bool
```

KARŞI ÖRNEK aynı dosyada, birkaç satır aşağıda: `IsActive` bir `bool`'dur ve
doğrudur — çünkü SAKLANMIYOR, `Phase`'ten türetiliyor (`Pressed` veya `Dragging`).
İkinci bir bool ALAN olsaydı iki kaynak doğardı; türetilmiş bir bool ise tablodan
bir sütun okumaktan başka bir şey değil. Yani reddedilen şey bool değil, kipin
YERİNE geçen bool.

#### İŞ BÖLÜMÜ: Phase ile IsActive ÖRTÜŞMEZ, BÖLÜŞÜR

```
hangi aşamadayız    ► Phase     (tablonun kendisi, tek yazar bu tip)
jest sürüyor mu     ► IsActive  (Press/Release'in muhatap alacağı
                                 kipleri tek soruda toplar)
```

Silinirse ne kırılır: `Phase` bool'a inerse "jest bitti mi" sorulamaz ve çağıran
ikinci bir bayrak tutmak zorunda kalır — sıfırlanmadığı gün hayalet fareye yapışıp
kalır; `Press_AfterClickReleased_StartsANewGesture` yazılamaz hâle gelir.
`IsActive` silinirse davranış değişmez ama `Release` ile `MoveTo`'nun giriş
kontrolü aynı iki kipi ayrı ayrı saymak zorunda kalır. Biri durumu tutuyor, öteki
onu tek soruya indiriyor.

**TEK CUMLE:** `bool` bir DEĞERİ taşır, kip ise bir AŞAMAYI; "sürüklüyor mu"
sorusunun cevabı "bitti mi" sorusunu cevaplamaz.

---

## dragThresholdSquared

Eşik KARE olarak saklanıyor; karşılaştırma da kare mesafeyle yapılıyor, karekök
ALINMIYOR. Asıl sebep hız değil, TİPİN SINIRI: sorulan soru "ne kadar uzağa gitti"
değil, "eşiği aştı mı" — bir evet/hayır. Karekök almak kimsenin istemediği bir
SAYI üretir; o sayı üretildiği an bir `Distance` property'si olmak için yalvarır ve
tip, `GridDistance`'ın işini ikinci kez yapmaya başlar. Ölçünün sahibi bu tip
değil.

### HARİTA: hangi şekil tipin eline NE veriyor

```
SEÇİLEN — kare ile karşılaştır
  (dx*dx + dy*dy) > dragThresholdSquared
  ╔══════════════════════════════════════════════╗
  ║ tipin eline geçen: bir EVET/HAYIR            ║
  ╚══════════════════════════════════════════════╝

REDDEDILEN — karekök al, ham eşikle karşılaştır
  Math.Sqrt(dx*dx + dy*dy) > dragThreshold
  ╔══════════════════════════════════════════════╗
  ║ tipin eline geçen: önce bir MESAFE, sonra bir║
  ║ evet/hayır                                   ║
  ║    └─► `public float Distance =>...` diye bir║
  ║        üye için yalvaran sayı BURADA doğar   ║
  ║        ◄── SINIRIN AŞILDIĞI NOKTA            ║
  ╚══════════════════════════════════════════════╝

Kare almak neden aynı cevabı veriyor (sıralama korunur):
  d   : 0 ──────────── t ──────────►
  d²  : 0 ──────────── t² ─────────►     d > t  ⇔  d² > t²
```

İkisi de AYNI cevabı verir ve bu tesadüf değil: kare alma işlemi negatif olmayan
sayılarda artan bir fonksiyondur, dolayısıyla sıralamayı korur. Her iki taraf da
negatif olamıyor — mesafenin karesi iki karenin toplamı, eşik ise kurucuda
doğrulanmış durumda.

Karekökten kaçınmanın bir de hesap tarafı var ama ÖLÇMEDİM ve bu yüzden gerekçe
olarak SAYMIYORUM: her karede çağrılması onu makul kılar, kanıtlamaz.

### GARANTİ NEREDE BİTER

Bilinen sınır, gizlenmiyor: aşırı büyük bir eşik (yaklaşık 1.8e19 üstü) karesi
alınınca `float`'ta sonsuza taşar ve o jest ASLA sürüklemeye dönmez. Karekökle
karşılaştıran sürüm de pratikte aynı cevabı verirdi — o kadar uzağa giden bir
işaretçi yok — dolayısıyla bu bir davranış farkı değil, yalnızca farkın nerede
doğduğudur.

### REDDEDILEN

Karekök alınır, eşik ham hâliyle saklanır:

```csharp
return Math.Sqrt((dx * dx) + (dy * dy)) > dragThreshold;
```

**KIRILAN:** kod yine derlenir; kırılan şey davranış değil tipin ne BİLDİĞİ —
elinde artık bir MESAFE var ve onu saklamamak zorlaşır.

```
Math.Sqrt double döner -> float eşik sessizce yükseltilir
tam eşikteki eşitlik   -> karekökün yuvarlamasına bağlanır
derleyici: hiçbir şey der  .  test: _ExactlyAtThreshold kırılganlaşır
```

**KAZANIRDI:** eşik BİRDEN ÇOK biçimde sorulsaydı — "yüzde kaçını aştı" diyen bir
ilerleme çubuğu ya da mesafeyle orantılı bir saydamlık; o gün mesafenin kendisi
bir çıktıdır.

### KAPSAM: kural "mesafe hesaplanmaz" DEĞİL

Ölçüt: sayının kendisi bir ÇIKTI mı, yoksa bir ara değer mi?

```
"kaç adım uzakta"  sayı çıktının kendisi   ► hesaplanır ve DÖNER
"eşiği aştı mı"    sayı yalnız ara değer   ► hiç üretilmez
```

KARŞI ÖRNEK aynı ad alanında, `GridDistance.Between`: mesafeyi hesaplar ve bir
SAYI döndürür — ve bu doğru, çünkü ölçünün sahibi o. Bu tipin rol notu farkı zaten
yazıyor: o ölçer ve hafızasızdır, bu tip ölçüyü bir EŞİKLE karşılaştırıp bir kipe
çevirir. Aynı aritmetik, iki ayrı sahip.

### İŞ BÖLÜMÜ: kurucudaki kare ile ölçümdeki kare

İki ayrı yerde kare alınıyor ve ikisi BİRLİKTE anlamlı:

```
kurucu: dragThreshold * dragThreshold ► eşiği bir kez kareler
ExceedsDragThreshold: dx*dx + dy*dy   ► mesafeyi her çağrıda
```

Silinirse ne kırılır: kurucudaki kare kalkarsa karşılaştırma mesafenin KARESİ ile
eşiğin KENDİSİNİ karşılaştırır — birimler ayrışır ve 1'den küçük eşiklerde cevap
sessizce tersine döner. Ölçümdeki kare kalkarsa bu kez ham mesafe kareli eşikle
karşılaşır. İkisi yedek değil: aynı dönüşümün denklemin iki tarafındaki karşılığı;
biri olmadan öteki yanlış.

**TEK CUMLE:** Karekök almamak bir hız numarası değil, tipin eline ihtiyacı OLMAYAN
bir sayıyı VERMEME kararıdır.

---

## PointerGesture(float dragThreshold)

Eşik DIŞARIDAN geliyor; içeride sabit yok. İçeriden bir sabit okusaydık, eşiği
sınamak için o sabiti değiştirmek gerekirdi — ve sabit bir piksel eşiği farklı
çözünürlükte farklı anlama gelir, hiçbir test bunu göremez.

### HARİTA: eşiğin girdi uzayı ve üç ayrı bölge

```
   NaN         negatif        0         pozitif
┌────────┬───────────────┬─────────┬───────────────►
│   ✗    │       ✗       │    ✓    │       ✓
└────────┴───────────────┴─────────┴───────────────►
Argument   ArgumentOut     ◄── SINIR DAHİL: sıfır eşik geçerli,
Exception  OfRange             "en ufak kıpırdama sürüklemedir"

REDDEDILEN (kırpma) aynı uzayı katlıyor:
  -5 ──┐
  -1 ──┼──► 0     negatif bölge sıfıra ÇÖKER  ◄── BİLGİ KAYBI:
   0 ──┘          yanlış hesap, geçerli bir eşiğe dönüşür
```

`NaN`'ın AYRI bir bölge olması bir incelik değil: `NaN` her karşılaştırmada `false`
verir, dolayısıyla `dragThreshold < 0f` testinden yara almadan geçer. Kontrol
sırası bu yüzden **NaN ► negatif** olmak zorunda.

`AttackProfile` ve `MoveProfile`'ın `int` alan kurucularında bu tuzak yoktu;
`float`'a geçmenin bedeli tam olarak o `IsNaN` satırıdır.

### İŞ BÖLÜMÜ: iki kontrol, iki ayrı bozuk sayı

```
float.IsNaN     ► KARŞILAŞTIRILAMAYAN sayıyı keser (0/0, ∞-∞)
dragThreshold<0 ► karşılaştırılabilir ama ANLAMSIZ sayıyı keser
```

Silinirse ne kırılır: `NaN` kontrolü kalkarsa negatif kontrolü onu GÖREMEZ; eşiğin
karesi de `NaN` olur, her karşılaştırma `false` döner ve jest ASLA sürüklemeye
dönmez — derlenen, fırlatmayan, testleri yeşil ve yarısı ölü bir giriş sistemi
(`Constructor_NaNThreshold_Throws` kırmızıya döner). Negatif kontrolü kalkarsa
`NaN` kontrolü de onu göremez. İkisi aynı kapıyı iki kez kilitlemiyor; iki ayrı
kapı.

### REDDEDILEN — kırpma

Negatif eşik sessizce düzeltilir:

```csharp
dragThresholdSquared = Math.Max(0f, dragThreshold) * Math.Max(0f, dragThreshold);
```

**KIRILAN:** kırılan şey HATANIN GÖRÜNÜRLÜĞÜ — negatif eşik yalnızca yanlış
hesaplanmış bir sayıdan gelebilir.

```
sıfıra kırpılır -> "her hareket sürüklemedir" diye çalışır
tıklama şekli   -> sessizce ölür
derleyici: hiçbir şey der  .  test: _NegativeThreshold_Throws kırmızı
```

**KAZANIRDI:** eşik oyuncunun ayarladığı bir kaydırıcıdan ve kaydırıcı bozuk bir
kayıt dosyasından okunsaydı — ama o kırpmanın yeri bu tip değil, kaydı okuyan
yerdir.

#### KAPSAM: kural "sessiz onarım yasak" DEĞİL

Ölçüt, bozuk değerin KAYNAĞI:

```
bozuk sayı bir HESAPTAN geliyor   -> yalnız kod hatası olabilir ► fırlat
bozuk SIRA güvenilmez bir akıştan -> girdi hıçkırığı olabilir   ► onar
```

KARŞI ÖRNEK aynı dosyada, `Press`: zaten basılıyken gelen ikinci bir `Press`
FIRLATMAZ, jesti yeniden başlatır — çünkü kaynağı bir `Update` döngüsü ve alt+tab
bir bırakma olayını yutabilir. Aynı tip, biri fırlatan biri sessizce onaran iki
karar; ayıran şey üslup değil, kaynağın güvenilirliği.

**TEK CUMLE:** Kırpmak yanlış hesabı DÜZELTMEZ, görünmez kılar; projenin kendi
deseni de tersidir — iki profil de fırlatır, kırpmaz.

### REDDEDILEN — sıfırı da yasaklamak

Yukarıdaki şeklin TEK BİR NOKTASI: "0" yazan ve "SINIR DAHİL" diye işaretlenmiş
sütun. Sıfır oradan çıkarılsaydı uzay ikiye değil üçe bölünürdü — ve ortada,
hiçbir oyun durumuna karşılık gelmeyen bir yasak bölge doğardı:

```
   NaN        negatif      0        pozitif
┌────────┬─────────────┬───────┬───────────────►
│   ✗    │      ✗      │  ✗ ◄──┼── REDDEDILEN: ifade
└────────┴─────────────┴───────┴───  edilebilir bir ayar
                                     yasaklanmış olurdu
```

`AttackProfile`'ın "range < 1" eşiği birebir kopyalanır:

```csharp
if (dragThreshold <= 0f) throw new ArgumentOutOfRangeException(...);
```

**KIRILAN:** ifade edilebilir oyunlardan biri dilden düşer — sıfır eşik "en ufak
kıpırdama bile sürüklemedir" demektir ve dokunmatik olmayan hassas bir giriş için
geçerli bir ayardır.

```
derleyici: hiçbir şey der  .  test: _ZeroThreshold_IsAllowed kırmızı
```

**KAZANIRDI:** giriş cihazı gürültülü olsaydı — parmağın durduğu yerde bile
titreyen bir dokunmatik ekranda sıfır eşik her tıklamayı sürüklemeye çevirirdi ve
"tolerans yok" bir hata olurdu.

#### KAPSAM: sıfır, tipe göre ifade edilebilir ya da değil

Ölçüt tek: sıfır, o kavramda gerçek bir ayar mı adlandırıyor?

```
sıfır sürükleme eşiği -> "toleranssız hassas giriş"  ► anlamlı ► kabul
sıfır hareket menzili -> "kök salmış birim"          ► anlamlı ► kabul
sıfır saldırı menzili -> hiçbir hücreye vuramaz      ► anlamsız ► ret
```

KARŞI ÖRNEK aynı projede: `AttackProfile`'ın kurucusu `range < 1` ile kesiyor.
Aynı biçim, aynı istisna tipi, farklı eşik — ve fark tipin kaprisinden değil,
sıfırın o kavramda bir oyun durumu adlandırıp adlandırmamasından geliyor. Hareket
tarafındaki ikizi [`MoveProfile.md`](MoveProfile.md#moveprofileint-range)'de.

#### İŞ BÖLÜMÜ: tip ile onu BESLEYEN yer bölüşür

```
ANLAMSIZ değeri reddetmek     ► bu kurucu (NaN, negatif)
BOZUK kaydı onarmak/kırpmak   ► ayarı okuyan yer (kaydırıcı,
                                kayıt dosyası okuyucusu)
```

Silinirse ne kırılır: kurucu kontrolü kalkarsa bozuk bir sayı sessizce yayılır ve
jest yarısı ölü çalışır. Kırpma buraya taşınırsa bu kez kaydın bozuk olduğu hiç
öğrenilemez — hata görünmez olur, kaybolmaz. İki sorumluluk aynı satıra sığmaz.

**TEK CUMLE:** Asimetri `MoveProfile` ile `AttackProfile` arasındakinin aynısı:
ulaşamayan bir SALDIRI anlamsız, toleransı olmayan bir EŞİK anlamlı.

---

## Phase

Jestin şu anki kipi. Bırakma kipleri (`ClickReleased`, `DragReleased`) bir sonraki
`Press` ya da `Reset` çağrısına kadar okunabilir kalır.

### KİPİN TEK YAZARI BU TİPTİR

**Alternatif:** kipi dışarıdan da yazılabilir yapmak (`public set`). Seçilmedi:
kipe karar vermek bu tipin TEK işi ve iptalin tek doğru yolu `Reset`'tir.

### HARİTA: kimin yazma hakkı var

```
SEÇİLEN — private set
  Press ──┐
  MoveTo ─┼──► ╔═══════╗   dışarıdan gelen ok YOK; her yazma
  Release ┼──► ║ Phase ║   geçiş tablosunun bir satırıdır
  Reset ──┘    ╚═══════╝

REDDEDILEN — public set
  çağıranın herhangi bir satırı ──► ╔═══════╗
                                    ║ Phase ║ ◄── KIRILMA
                                    ╚═══════╝     NOKTASI
  Tablo artık bir SÖZ değil, bir öneri: "Dragging'den Pressed'e
  dönüş yok" güvencesi tek bir atamayla aşılır ve yapı ile
  korunan yasak geçiş yeniden mümkün olur.
```

### KAPSAM: kural "kip dışarıdan değişmez" DEĞİL

KARŞI ÖRNEK bu dosyanın sonunda: `Reset()` public'tir ve kipi dışarıdan gelen bir
istekle değiştirir — yerleştirme kipinden çıkarken tam olarak bunun için çağrılır.
Fark şu: `Reset` bir kip ADI almaz, yalnız "iptal" der; hedefi tip seçer. Yani
dışarısı geçişi TETİKLEYEBİLİR, ADLANDIRAMAZ.

### İŞ BÖLÜMÜ: private set ile Reset bölüşür

```
keyfî yazmayı kesmek   ► private set
meşru iptali sağlamak  ► Reset (Idle'a döner, basma noktasını siler)
```

Silinirse ne kırılır: `private set` gevşerse tablo bağlayıcı olmaktan çıkar.
`Reset` silinirse kip kapalı bir kutuya döner — jest yarıda iptal edilemez ve aynı
örnek yeni bir jest için temizlenemez; `Reset_ReturnsToIdle` ile
`Reset_ForgetsThePressOrigin` kırmızıya döner. Biri kapıyı kilitliyor, öteki meşru
anahtarı veriyor.

---

## IsActive

İşaretçi şu an basılı mı: `Pressed` ya da `Dragging`.

Türetilmiş, saklanmıyor. İkinci bir `bool` alan olsaydı iki kaynak doğardı ve biri
güncellenmediği gün kod derlenirdi. Neden bir bool'un burada doğru, `Phase`'in
yerine geçen bir bool'un yanlış olduğu
[`PointerGesture` (tip)](#pointergesture-tip) bölümünde.

---

## Press(float x, float y)

İşaretçi basıldı: yeni bir jest başlar ve ölçüm noktası burasıdır. Her zaman
`Pressed` döner.

Basmanın BAŞLADIĞI nokta `pressX`/`pressY`'de saklanır. Ölçüm her zaman buradan
yapılır, bir önceki `MoveTo`'dan değil. Fark önemli ve sessiz: adım adım ölçseydik
yavaşça yüz piksel sürüklenen bir işaretçi hiçbir adımda eşiği aşmazdı ve jest
sonuna kadar "tıklama" kalırdı.

### ZATEN BASILIYKEN GELEN İKİNCİ PRESS: YENİDEN BAŞLATIR, FIRLATMAZ

Gerekçe çağıranın gerçekliği: bu metodu besleyen şey bir `Update` döngüsü ve
pencerenin odağını kaybetmek, alt+tab, ya da enjekte edilmiş bir girdi bırakma
olayını yutabilir. Yutulduğu gün doğru davranış oyunu düşürmek değil, bir sonraki
basmayı yeni bir jest saymaktır — kullanıcının niyeti de zaten budur.

### HARİTA: yutulmuş bir bırakma olayının izi

Kareler soldan sağa. 3. karede pencere odağı kaybediyor ve motor o karede `Up`
olayını HİÇ üretmiyor:

```
kare    1        2         3 (alt+tab)    4         5
olay    Down     Move      — Up YUTULDU   —         Down
kip     Pressed  Dragging  Dragging ◄──①  Dragging   ?

① Tipin bakış açısından jest hâlâ sürüyor; onu bitirecek olay
  hiç gelmedi ve bir daha GELMEYECEK. Bu hâl kendi kendine
  düzelmez.

5. karede iki cevap mümkün:
  REDDEDILEN  throw  ──► oyun düşer; oysa kullanıcı yalnızca
                         yeni bir tıklama yaptı
  SEÇİLEN     Press  ──► yeni jest, yeni ölçüm noktası;
                         kullanıcının niyeti de zaten buydu
```

### REDDEDILEN

```csharp
if (IsActive) throw new InvalidOperationException("Pointer is pressed.");
```

**KIRILAN:** yutulmuş TEK bir bırakma olayı, sonraki tıklamada oyunu düşürür.

```
alt+tab, odak kaybı -> bırakma olayı hiç gelmez
sonraki Press       -> jest yerine çökme üretir
derleyici: hiçbir şey der  .  test: _RestartsFromTheNewOrigin kırmızı
```

**KAZANIRDI:** çağıran bir test ya da kayıttan oynatma olsaydı — orada bozuk sıra
bir girdi hıçkırığı değil, kaydın bozulduğunun kanıtıdır ve sessizce düzeltilmesi
asıl hatayı gizlerdi.

### KAPSAM: bu hoşgörü SIRAYA özeldir, DEĞERE değil

Aynı tip, iki farklı bozukluğa iki zıt cevap veriyor:

```
bozuk SIRA (beklenmedik olay akışı) -> onar, jesti yeniden başlat
bozuk DEĞER (NaN/negatif eşik)      -> fırlat
```

KARŞI ÖRNEK aynı dosyada, kurucu: bozuk bir eşik için hiç tereddüt etmeden
fırlatıyor. Çünkü orada değer bir HESAPTAN geliyor ve bozuksa yalnızca kod hatası
olabilir; burada ise sıra bir `Update` döngüsünden geliyor ve olay yutulması
normal. Kırpma bloğundaki gerekçenin tam ikizi, ters yönde.

### İŞ BÖLÜMÜ: iki kurtarma yolu, iki ayrı senaryo

```
olay YUTULDU, kullanıcı devam ediyor ► Press yeniden başlatır
çağıran kipten ÇIKMAK istiyor        ► Reset iptal eder
```

Silinirse ne kırılır: `Press`'in yeniden başlatması fırlatmaya çevrilirse tek bir
yutulmuş olay bir sonraki tıklamada oyunu düşürür ve
`Press_WhileAlreadyPressed_RestartsFromTheNewOrigin` kırmızıya döner. `Reset`
silinirse yarım kalmış jest ancak yeni bir `Press` ile temizlenebilir — yani iptal
etmek için basmak gerekir. Biri kullanıcının devam etmesini, öteki çağıranın
vazgeçmesini karşılıyor.

**TEK CUMLE:** Güvenilmez bir kaynağa istisna ile cevap vermek hatayı raporlamak
değil, onu kullanıcıya TAŞIMAKTIR.

---

## MoveTo(float x, float y)

İşaretçi basılıyken yeni bir konuma taşındı. Eşik aşıldıysa `Dragging`, aşılmadıysa
`Pressed` döner; jest etkin değilse kip değişmeden döner.

### BURASI KARARIN KENDİSİ: DRAGGING'DEN GERİ DÖNÜŞ YOK

Kip yalnızca `Pressed` iken yeniden hesaplanıyor. `Dragging`'e bir kez
geçildiğinde bu metot hiçbir şey ölçmez ve GERİ DÖNMEZ. Sebep tek cümle: eşiği
aşıp geri gelen bir işaretçi hâlâ sürüklüyordur. İnsan eli bir hedefe götürüp geri
çeker; her karede yeniden karar veren bir kip, eşiğin tam üstünde titreyen bir
işaretçide saniyede onlarca kez `Pressed` ile `Dragging` arasında atlar ve hayalet,
oyuncunun hiç istemediği bir anda yerleşir.

### HARİTA: eşiğin üstünde titreyen el

Eşik 10 birim; mesafe her zaman BASMA noktasından ölçülüyor. Oyuncu hedefe götürüp
hafifçe geri çekiyor (P = `Pressed`, D = `Dragging`):

```
kare          1     2     3     4     5     6 (Release)
mesafe        4    11     9    12     8     9
eşik aşıldı   ✗     ✓     ✗     ✓     ✗     ✗
──────────────────────────────────────────────────────────
SEÇİLEN       P     D     D     D     D  ► DragReleased
                    └── bir kez geçildi, GERİ DÖNÜŞ YOK ◄── ①
REDDEDILEN    P     D     P     D     P  ► ClickReleased
                          └── aynı hareket, bırakma anına göre
                              farklı sonuç
```

① Tek yönlülük tabloda bir satır olarak görünüyor: SEÇİLEN satırında D'den P'ye
dönen tek bir hücre yok.

Bu tek yönlülük bir `if` ile değil, YAPI ile korunuyor: `Dragging`'den çıkan bir
yol yok. Bir davranışı korumanın en ucuz yolu onu ifade edilemez kılmaktır.

### REDDEDILEN

Kip her karede yeniden hesaplanır ve geri dönebilir:

```csharp
Phase = ExceedsDragThreshold(x, y) ? PointerPhase.Dragging
                                   : PointerPhase.Pressed;
```

**KIRILAN:** oyuncu sürüklediği yapıyı başladığı hücreye geri getirip bırakınca
jest `Pressed`'e düşer.

```
bırakma ClickReleased üretir -> yerleştirme hiç bitmez
eşikte titreyen el           -> aynı hareket farklı davranır
derleyici: hiçbir şey der  .  test: _StaysDragging ikisi de kırmızı
```

**KAZANIRDI:** jest iptal edilebilir olsaydı — başladığı yere dönmek "vazgeçtim"
demek olsaydı; ama dönülen yer `Pressed` değil ayrı bir `Cancelled` kipi olurdu.

### KAPSAM: geri dönülemeyen şey EŞİK KARARIdır

Nesne kilitlenmiyor; kilitlenen tek şey aynı jest içinde eşiğin yeniden
tartışılması:

```
aynı jest içinde Dragging ► Pressed      ► YOK (bu blok)
yeni bir jest başlatmak (Press)          ► her kipten serbest
jesti iptal etmek (Reset)                ► her kipten serbest
```

KARŞI ÖRNEK aynı dosyada, geçiş tablosunun `Press` sütunu: `Dragging` iken gelen
bir `Press` kipi `Pressed`'e ÇEKER ve bu doğrudur — orada eski jest bitmiş, yenisi
başlamıştır. Yani "geri dönüş yok" sözü nesnenin ömrü için değil, TEK BİR JESTİN
içi için geçerli.

### İŞ BÖLÜMÜ: eşiği iki metot birlikte gözlüyor

Jestin ömrü boyunca eşik iki ayrı yerde ölçülüyor ve ikisi iki farklı kareyi
kapatıyor:

```
basılı geçen kareler ► MoveTo (yalnız Pressed iken hesaplar)
BIRAKMA karesi       ► Release (Pressed iken son konumu ölçer)
```

Silinirse ne kırılır: `MoveTo`'nun tek yönlülüğü kalkarsa eşiğe geri dönen el
jesti `Pressed`'e düşürür ve `MoveTo_BackInsideThreshold_StaysDragging` ile
`MoveTo_ReturningToPressOrigin_StaysDragging` kırmızıya döner. `Release`'in ölçümü
kalkarsa hızlı bir savurmada eşik hiç görülmez; o gerekçe
[`Release`](#releasefloat-x-float-y) bölümünde. İki metot aynı ölçümü
tekrarlamıyor; jestin iki ayrı kare kümesini bölüşüyor.

**TEK CUMLE:** Bir davranışı korumanın en ucuz yolu onu İFADE EDİLEMEZ kılmaktır;
`Dragging`'den çıkan bir yol yok, o yüzden bir `if` de yok.

---

## Release(float x, float y)

İşaretçi bırakıldı. Jest, eşiğin aşılıp aşılmadığına göre `ClickReleased` ya da
`DragReleased` ile biter; jest etkin değilse kip değişmeden döner.

### İKİ SONUCUN AYRI OLMASI BİR KARARDIR, ÜSLUP DEĞİL

Çağıran ikisine farklı cevap verir: sürükleme bırakıldığında yerleştirme BİTER;
tıklama bırakıldığında hayalet fareyi TAKİP ETMEYE DEVAM EDER ve ikinci bir
tıklama yerleştirir. Tek değere indirmek iki giriş şeklini tek şekle indirger:
sürükle-bırak ile tıkla-bırak aynı anda yaşayamaz.

#### HARİTA: iki sonucun çağıranda karşılığı

```
giriş şekli      son kip          çağıranın yaptığı
──────────────   ──────────────   ──────────────────────────────
sürükle-bırak    DragReleased     yerleştirme BİTER, kipten çıkılır
tıkla-bırak      ClickReleased    hayalet fareyi TAKİP ETMEYE
                                  devam eder; ikinci tıklama
                                  yerleştirir      ◄── AYRIM BURADA
────────────────────────────────────────────────────────────────
REDDEDILEN       tek "Released"   iki satırdan BİRİ seçilmek
                                  zorunda; seçilmeyen giriş şekli
                                  tamamen ölür
```

İki değer, iki ayrı davranışın adı. Tek değere inince tablo iki satırdan bire
düşer ve kaybolan şey bir enum üyesi değil, bir GİRİŞ ŞEKLİ olur.

#### KAPSAM: kural "her fark kendi kipini alsın" DEĞİL

Ölçüt yine davranış: çağıran ikisine FARKLI mı davranıyor?

KARŞI ÖRNEK aynı metodun ilk satırında: jest etkin değilken gelen bir `Release`,
kipi DEĞİŞTİRMEDEN döner ve "neden yok sayıldı" (`Idle` miydi, zaten bırakılmış
mıydı) diye bir ayrım ÜRETİLMEZ — çünkü çağıran her iki durumda da aynı şeyi
yapar: hiçbir şey. Aynı tip, biri ayıran biri birleştiren iki karar, tek ölçüt.

#### İŞ BÖLÜMÜ: sonucu ÜRETMEK ile SAKLAMAK

```
hangi sonuç doğdu ► Release'in iki kipten birini seçmesi
ne zamana kadar   ► sonuç kiplerinin bir sonraki Press ya da
okunabilir           Reset'e kadar KALICI olması
```

Silinirse ne kırılır: ayrım silinirse çağıran iki giriş şeklinden birini seçmek
zorunda kalır ve `Release_ClickAndDrag_ProduceDifferentPhases` kırmızıya döner.
Kalıcılık silinip kip kendiliğinden `Idle`'a düşseydi, kararı üreten kare ile
tüketen karenin aynı olması gerekirdi — çağıran bir kare geç okuduğunda sonuç
kaybolurdu. Biri cevabı üretiyor, öteki cevabın okunabildiği pencereyi açıyor.

#### REDDEDILEN

İki sonuç tek değere indirilir:

```csharp
public enum PointerPhase { Idle, Pressed, Dragging, Released }
```

**KIRILAN:** çağıran "yerleştir ve kipten çık" ile "kipte kal, hayalet takip
etsin" arasında seçim yapamaz; hangisini seçerse seçsin giriş şekillerinden biri
tamamen ölür.

```
derleyici: hiçbir şey der  .  test: _ProduceDifferentPhases kırmızı
```

**KAZANIRDI:** yalnızca TEK giriş şekli desteklenseydi — o gün iki değer, çağıranın
her `switch`'inde aynı gövdeyi iki kez yazdırırdı.

**TEK CUMLE:** İki değer ancak çağıran onlara FARKLI davranıyorsa ayrıdır; burada
biri kipi bitirir, öteki kipte tutar.

---

### BIRAKMA KONUMU DA EŞİKTEN GEÇİRİLİYOR — PARAMETRELER SÜS DEĞİL

Sebep: Unity'de bırakma karesinde `GetMouseButton` `false`, yalnızca
`GetMouseButtonUp` `true` döner; yani o karenin konumu bu tipe SADECE buradan
ulaşır. Hızlı bir savurmada eşiğin aşıldığı ilk kare tam da o karedir. Konumu yok
saysaydık cevap, konumun HANGİ metottan geldiğine bağlı olurdu — saf bir tipin
içine motorun kare düzeninin sızması demek olurdu bu.

**BEDELİ ödendi ve gizlenmiyor:** "Release, `Pressed` iken -> `ClickReleased`"
artık tek başına doğru değil; `Pressed` iken bırakılan bir jest, son konum eşiğin
DIŞINDAYSA `DragReleased` döner. Kip, karar anındaki kiptir ve son konum o
karardan önce hesaba katılır.

#### HARİTA: mesafe hangi karede katediliyor

Eşik 10 birim. Oyuncu geniş bir yay çizip savurmanın sonunda bırakıyor:

```
kare          1 (Down)    2 (Move)    3 (Up)
konum         (0,0)       (3,0)       (40,0)
basmadan uzak  0           3           40
─────────────────────────────────────────────────────────
motorun cevabı            GetMouseButton 3. karede FALSE döner
                          ► MoveTo o karede ÇAĞRILMAZ
                          ► (40,0) tipe SADECE Release'ten ulaşır
─────────────────────────────────────────────────────────
REDDEDILEN    konum yok sayılır ► eşik hiç görülmez
                                  ► ClickReleased  ◄── yapı
                                    yerleşmez, oyuncu bıraktığını
                                    sanır
SEÇİLEN       Release ölçer      ► eşik son karede aşılır
                                  ► DragReleased
```

Bütün ayrım tek bir karede, motorun kare düzeninin bıraktığı boşlukta doğuyor.

#### REDDEDILEN

Bırakma konumu yok sayılır, parametreler yalnızca simetri için durur:

```csharp
Phase = Phase == PointerPhase.Dragging ? PointerPhase.DragReleased
                                       : PointerPhase.ClickReleased;
```

**KIRILAN:** aynı nokta akışı, hangi metottan girdiğine göre farklı cevap alır;
hızlı savurmada bütün mesafe bırakma karesinde katedilir.

```
MoveTo eşiği hiç görmez -> jest ClickReleased döner
oyuncu geniş yay çizer  -> yapı yerleşmez, bıraktığını sanır
derleyici: hiçbir şey der  .  test: _IsDragReleased kırmızıya döner
```

**KAZANIRDI:** çağıran bırakma karesinde de `MoveTo` çağırmayı garanti etseydi — o
gün son konum tipe zaten ulaşmış olurdu ve `Release`'in konum alması gereksiz
tekrar olurdu.

#### KAPSAM: her (x, y) çifti aynı işi yapmıyor

Üç metodun da iki `float` alması bir simetri değil; üçü farklı rol veriyor:

```
Press(x, y)    ► ölçümün BAŞLANGIÇ noktasını kurar
MoveTo(x, y)   ► basılı geçen bir karede eşiği sınar
Release(x, y)  ► son karede eşiği sınar ve jesti bitirir
```

KARŞI ÖRNEK aynı dosyada, `Press`: oradaki koordinatlar hiçbir eşik testinden
geçmez, yalnız saklanır. Yani "parametre varsa ölçülür" diye bir kural yok — bu
bloğun savunduğu şey `Release`'in kendi karesinde ölçmek ZORUNDA olması.

#### İŞ BÖLÜMÜ: iki metot, motorun iki ayrı kare kümesi

```
GetMouseButton'un true olduğu kareler ► MoveTo görür
GetMouseButtonUp'ın true olduğu kare  ► YALNIZ Release görür
```

İki küme KESİŞMEZ; bırakma karesinin konumu `MoveTo`'ya hiç uğramaz. Silinirse ne
kırılır: `Release`'in ölçümü kalkarsa o kare hiçbir metot tarafından görülmez ve
hızlı savurmalar sessizce tıklamaya döner
(`Release_CrossingThresholdOnTheReleaseFrame_IsDragReleased` kırmızıya döner).
`MoveTo`'nun ölçümü kalkarsa bu kez yavaş sürüklemeler bırakılana kadar tıklama
sayılır. İkisi aynı kareyi iki kez ölçmüyor; kareleri bölüşüyor.

#### GARANTİ NEREDE BİTER

Bu tip çağıranın `MoveTo`'yu HER karede çağırdığını zorlayamaz — böyle bir söz
derlenmez. `Release`'in kendi ölçümü tam olarak bu zorlanamazlığın karşılığıdır:
çağıran hiç `MoveTo` çağırmasa bile jest doğru biter.

**TEK CUMLE:** Zorlanamayan bir garantiye yaslanmak sözleşmeyi yorumun eline
bırakmaktır; bu yüzden `Release` son konumu kendisi ölçer.

---

## Reset()

Jesti sıfırlar: kip `Idle` olur ve basma noktası unutulur. Yerleştirme kipinden
çıkarken, jest yarıda iptal edilirken ya da aynı örnek yeni bir jest için
kullanılmadan önce çağrılır.

Basma noktasını da temizliyor. Kip `Idle` iken o iki sayı zaten okunmuyor,
dolayısıyla bu satır davranışı değiştirmez — ama iptal edilmiş bir jestin
koordinatlarını nesnede bırakmak, bir sonraki hatanın "ölçüm nereden yapıldı" diye
sorulduğunda yanlış cevap vermesi demektir.

`Reset`'in `private set` ile nasıl bölüştüğü [`Phase`](#phase) bölümünde.

---

## ExceedsDragThreshold(float x, float y)

Ölçüm HER ZAMAN basma noktasından. Karşılaştırma KESİN büyüktür: tam eşik kadar
gidilmiş bir jest hâlâ bir tıklamadır. Eşik "bu kadar oynayabilirsin ve hâlâ
tıklıyorsun" diye okunur; sınır dahildir.

Karekökün neden alınmadığı ve iki karenin denklemin iki tarafında nasıl bölüştüğü
[`dragThresholdSquared`](#dragthresholdsquared) bölümünde.
