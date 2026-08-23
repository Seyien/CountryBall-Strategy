# Bir tıklamanın yolculuğu — pikselden savaş kuralına

> **Nerede geçiyor:** `BoardAdapter.cs` → `PointerGesture.cs` → `BattleActions.cs` → `UnitView.cs`
> **Kodda nereden geldin:** `BoardAdapter.FeedGesture`, `BoardAdapter.HandleClick`,
> `BoardAdapter.CommitPlacement`, `PointerGesture.MoveTo`, `PointerGesture.Release`,
> `PointerGesture.Phase`
> **Ne zaman oku:** giriş akışına yeni bir jest eklemeden önce (uzun basma, çift
> tıklama, sağ tık), ya da "bu üç `Input` sorgusundan biri fazla değil mi" diye
> sorduğunda.

---

## Sahne

Oyuncu fareye basıyor, birkaç piksel sürüklüyor, bırakıyor. Tahtaya bir baraka
konuyor.

Başka bir sefer aynı şeyi yapıyor — basıyor, bırakıyor — ve bu kez hiçbir şey
konmuyor; hayalet fareye yapışıp kalıyor, ikinci bir tıklama bekliyor.

**İki hareket başlangıcında birbirinin aynısı.** İkisini ayıran şey ne düğme, ne
hücre, ne de süre: yalnızca *"basılı tutulurken imleç gerçekten hareket etti
mi"*. Bu dosya o tek sorunun nerede sorulduğunu, kimin sorduğunu, ve cevabın
tahtaya varana kadar kaç el değiştirdiğini anlatıyor.

Yol boyunca bir duvar var ve duvarı yalnızca **dört float** geçiyor.

---

## Karakterler

Beş oyuncu. Hikâyeyi ilginç kılan yine bilmedikleri.

```
╔═ BoardAdapter (MonoBehaviour) ════════════════════════════════╗
║  İşi     : çevirmenlik — motorun kare düzenini okumak         ║
║  Bilir   : Input, Camera, Grid, Time, sahne, seçili birim     ║
║  BİLMEZ  : ██ BİR JESTİN NE OLDUĞUNU ██ — "bu tıklama mıydı"  ║
║            sorusunu kendisi cevaplamaz, SORAR                 ║
╚═══════════════════════════════════════════════════════════════╝

╔═ PointerGesture (saf sınıf, Core) ════════════════════════════╗
║  İşi     : karar — nokta akışını bir KİP'e çevirmek           ║
║  Bilir   : basmanın başladığı yer, eşik, içinde bulunduğu kip ║
║  BİLMEZ  : ██ CİHAZI ██ hangi düğme, kaç saniye, hangi hücre, ║
║            hangi kamera, hatta eşiğin BİRİMİ bile             ║
╚═══════════════════════════════════════════════════════════════╝

╔═ PointerPhase (enum, aynı dosyada) ═══════════════════════════╗
║  İşi     : beş hâli adlandırmak                               ║
║  Bilir   : Idle / Pressed / Dragging / ClickReleased /         ║
║            DragReleased                                       ║
║  BİLMEZ  : hiçbir şey — o bir sözlük, bir aktör değil         ║
╚═══════════════════════════════════════════════════════════════╝

╔═ BattleActions (statik, Battle) ══════════════════════════════╗
║  İşi     : geçerlilik — "kural izin veriyor mu"               ║
║  Bilir   : tahta, savaşçılar, sıra, menzil, doluluk           ║
║  BİLMEZ  : ██ NİYETİ ██ fare diye bir şey duymadı; ona göre    ║
║            her çağrı zaten kasıtlıdır                         ║
╚═══════════════════════════════════════════════════════════════╝

╔═ UnitView (MonoBehaviour) ════════════════════════════════════╗
║  İşi     : uygulamak — çerçeveyi açmak, gövdeyi boyamak       ║
║  Bilir   : kendi SpriteRenderer'ları                          ║
║  BİLMEZ  : `Unit` tipini, seçili olup olmadığını, tahtayı;    ║
║            "seçili miyim" diye SORULACAK bir yeri yok         ║
╚═══════════════════════════════════════════════════════════════╝
```

En tuhafı ikincisi: **`PointerGesture` fareyi hiç duymadı.** `Input` yok, `Camera`
yok, `Vector2` yok — dört float ve bir eşik. Bu bir zarafet gösterisi değil,
`GridStrategy.Core` asmdef'indeki tek satırın sonucu:

```json
"noEngineReferences": true
```

Bu satır ölçülmüş bir gerçek, tahmin değil: `Core` derlenirken `UnityEngine`'e
hiç bakmaz. Dolayısıyla `PointerGesture` içine bir `Input.GetMouseButton`
yazıldığı gün dosya **derlenmez** — ve bütün hikâye o duvarın etrafında dönüyor.

---

## Birinci durak: aynı basış, iki ayrı yol

Bir kare başlıyor. `Update` ilk işi olarak savaşın saatini ilerletiyor, sonra
tek bir soru soruyor:

```csharp
if (isPlacingStructure)
{
    UpdatePlacement();
    return;                    // ◄── giriş akışı burada İKİYE ayrılıyor
}
```

Bu `if` bir optimizasyon değil, bir **anlam ayrımı**. Aynı sol tık:

```
   kip KAPALI                          kip AÇIK
   ─────────────                       ─────────
   Update ──► GetMouseButtonDown       Update ──► UpdatePlacement
          ──► HandleClick                     ──► FeedGesture
          ──► seç / saldır / hareket ettir    ──► hayaleti taşı / yerleştir
   ██ JEST HİÇ SORULMAZ ██             ██ JEST BURADA YAŞIYOR ██
```

Ve burada, bu dosyanın en kolay yanlış hatırlanan gerçeği duruyor:

**Tıklama akışı `PointerGesture`'dan hiç geçmiyor.**

`HandleClick`'e giden yol yalnızca tek bir sorgu okur:

```csharp
if (!Input.GetMouseButtonDown(0))
{
    return;
}

HandleClick();
```

Tek tıklama istendiği için tek soru yetiyor. Jest makinesinin bugünkü **tek
müşterisi** yerleştirme kipidir. Bu bir eksiklik değil, kapsamın dürüst hâli:
sürükleme diye bir kavramı ihtiyaç duyan tek giriş kipi o.

Dosyada `Input.GetMouseButton*` toplam **dört** yerde geçiyor. Biri burada, üçü
`FeedGesture`'da.

---

## İkinci durak: piksel hücreye dönüyor — tek çeviri, iki çağıran

İki yol da aynı kapıdan geçiyor:

```csharp
private bool TryReadPointerCell(out float worldX, out float worldY, out int x, out int y)
```

Üç adım, tek metot:

```
Input.mousePosition          EKRAN pikseli, sol alt (0,0)
        │
        │  Camera.main.ScreenToWorldPoint(...)   ◄── kameraya BAĞLI
        ▼
worldPoint (x, y)            DÜNYA birimi  ──┬──► jest eşiği bunu ölçer
        │                                    │
        │  unityGrid.WorldToCell(...)        └──► ██ duvarı geçen şey ██
        ▼
cell (x, y)                  HÜCRE indeksi  ─────► kural bunu konuşur
```

Dönüş **iki çift** veriyor ve ikisinin iki ayrı müşterisi var:

| çıktı | kim ister | neden |
|---|---|---|
| `worldX`, `worldY` | `FeedGesture` → `PointerGesture` | eşik dünya biriminde ölçülüyor |
| `x`, `y` | `HandleClick`, `CommitPlacement` | kural hücre indeksi konuşuyor |

`HandleClick` dünya koordinatlarını `out _` ile çöpe atıyor — ihtiyacı yok.
`UpdatePlacement` ikisini birden alıyor.

**Eşik neden dünya biriminde:** piksel seçilseydi aynı parmak hareketi 1920'lik
ekranda "tıklama", 2560'lık ekranda "sürükleme" sayılırdı. Giriş **şekli**
çözünürlüğe bağlı olurdu. Dünya birimi ayrıca ölçülebilir bir anlam taşıyor:
`0.25` = "çeyrek hücre".

Ve bu, `PointerGesture`'ın özetindeki **BİRİM UYARISI**'nın karşılığı: tip
`x`, `y` ve `dragThreshold`'un aynı birimde olmasını *isteyemez*, yalnızca
yazabilir. Karıştırıldığı gün kod derlenir, testler yeşil kalır, sadece eşik
yanlış yerde durur. Sözleşmeyi tutan tek şey bu iki satırın aynı metottan
çıkması.

---

## Üçüncü durak: motorun üç sorusu

`FeedGesture` on satır. Bütün dosyanın en yoğun on satırı.

```csharp
private PointerPhase FeedGesture(float worldX, float worldY)
{
    PointerPhase phase = gesture.Phase;

    if (Input.GetMouseButtonDown(0))      { phase = gesture.Press(worldX, worldY); }
    else if (Input.GetMouseButton(0))     { phase = gesture.MoveTo(worldX, worldY); }

    if (Input.GetMouseButtonUp(0))        { phase = gesture.Release(worldX, worldY); }

    return phase;
}
```

Üç sorgu, **üç ayrı soruya** cevap veriyor:

```
   GetMouseButtonDown(0)   yalnız BASILDIĞI karede true   ──► Press
   GetMouseButton(0)       basılı olduğu HER karede true   ──► MoveTo
   GetMouseButtonUp(0)     yalnız BIRAKILDIĞI karede true  ──► Release
```

### Bir jestin kare kare hâli

```
kare                    1        2        3        4        5        6
════════════════════════════════════════════════════════════════════════
motorun cevapları
  ...ButtonDown(0)      ✓        ✗        ✗        ✗        ✗        ✗
  ...Button(0)          ✓        ✓        ✓        ✓        ✗        ✗
  ...ButtonUp(0)        ✗        ✗        ✗        ✗        ✓        ✗
────────────────────────────────────────────────────────────────────────
FeedGesture ne çağırır
  if / else if zinciri  Press   MoveTo   MoveTo   MoveTo     —        —
  AYRI if               —        —        —        —      Release     —
────────────────────────────────────────────────────────────────────────
kip                    Pressed  Pressed  Dragging Dragging  Drag-     Drag-
                                                            Released  Released
                                    ▲                          ▲
                       eşik burada aşıldı          ██ SONUÇ KİPİ KALICI ██
                                                   bir sonraki Press'e kadar
```

**1. karede Down VE Button ikisi de true.** `if / else if` olması bu yüzden:
aynı karede hem `Press` hem `MoveTo` çağrılsaydı, basmanın hemen ardından
ölçüm yapılırdı — zararsız ama anlamsız. Zincir `Press`'i seçiyor.

**5. karede Button false, yalnız Up true.** O karenin konumu jest tipine
**sadece** `Release`'ten ulaşabilir. Bu, dördüncü durağın konusu.

### Biri eksik olsaydı hangi kavram YAZILAMAZDI

```
  ✗ GetMouseButtonDown yok
      ► ölçümün BAŞLANGIÇ noktası hiç kurulmaz
      ► `Press` çağrılmaz → jest hiç doğmaz → beş kipin beşi de Idle
      ██ ÖLEN KAVRAM: jestin kendisi ██

  ✗ GetMouseButton yok
      ► basılı geçen kareler görünmez, `MoveTo` hiç çağrılmaz
      ► eşik yalnızca bırakma karesinde sınanır
      ██ ÖLEN KAVRAM: SÜRÜKLEME ██ — "basılıyken hareket etti mi"
         sorusu sorulamaz; yalnız "nerede bıraktı" bilinir

  ✗ GetMouseButtonUp yok
      ► bırakma karesi hiç görülmez, `Release` çağrılmaz
      ► kip sonsuza kadar Pressed/Dragging'de asılı kalır
      ██ ÖLEN KAVRAM: BİTİŞ ██ — hiçbir jest sonuca varmaz,
         hiçbir yapı yerleşmez
```

Üçü bir arada olmak zorunda ve bu bir "tam olsun" kaygısı değil: **her biri
farklı bir kare kümesini kapatıyor** ve kümeler kesişmiyor.

### Neden `Up` ayrı bir `if`

Kare süresinden kısa bir tıklama mümkün. O karede motor **Down ve Up'ı birlikte**
true döndürür:

```
kare                    1
════════════════════════════════════
  ...ButtonDown(0)      ✓
  ...Button(0)          ✓
  ...ButtonUp(0)        ✓   ◄── ÜÇÜ BİRDEN, tek karede
────────────────────────────────────
SEÇİLEN
  if / else if          Press
  AYRI if               Release   ► ClickReleased
                        ██ tam bir jest, tek karede ██

REDDEDILEN — hepsi TEK zincirde
  if      Down    → Press      ◄── kazanır
  else if Button  → MoveTo
  else if Up      → Release    ◄── ██ BURAYA HİÇ GELİNMEZ ██
                        ► bırakış sessizce YUTULUR
                        ► kip Pressed'de asılı kalır
                        ► hayalet fareye yapışır ve bir daha bırakmaz
```

İki `if`'in ayrı durması bir üslup tercihi değil; hızlı tıklamanın var olma
şartı.

---

## Dördüncü durak: duvar

`FeedGesture`'ın üç çağrısı bir sınırı geçiyor.

```
   ┌─ GridStrategy.Unity ── BoardAdapter (MonoBehaviour) ──────────────┐
   │                                                                   │
   │   Camera.main.ScreenToWorldPoint(Input.mousePosition)             │
   │   unityGrid.WorldToCell(worldPoint)                               │
   │   Input.GetMouseButtonDown / GetMouseButton / GetMouseButtonUp    │
   │   Time.deltaTime                                                  │
   │                                                                   │
   └──────────────────────────┬────────────────────────────────────────┘
                              │
                    duvarı GEÇEN her şey:  worldX, worldY  (+ kurucuda eşik)
                              │
   ═══════════════════════════▼═══════════════════════════════════════
   ██  A S M D E F   D U V A R I  ██   GridStrategy.Core
   "noEngineReferences": true
   Input ✗   Camera ✗   Time ✗   Vector2 ✗   MonoBehaviour ✗
   ═══════════════════════════╤═══════════════════════════════════════
                              │
   ┌──────────────────────────▼────────────────────────────────────────┐
   │  GridStrategy.Core ── PointerGesture (saf sınıf)                  │
   │                                                                   │
   │    Press(x, y)     MoveTo(x, y)     Release(x, y)     Reset()     │
   │              ──────────► PointerPhase ──────────►                 │
   └───────────────────────────────────────────────────────────────────┘
```

Duvarın altında **"sol fare düğmesi" diye bir kavram yok.** Dokunmatik, atanmış
bir tuş, ya da kayıttan oynatılan bir girdi aynı üç metoda aynı şekilde girer.

### Duvarın yasakladığı şey VERİ değil, CİHAZ BİLGİSİ

Ayrım ince ve dosyanın kendisi karşı örneğini taşıyor: `dragThreshold` de
dışarıdan geliyor ve içeride tek bir sabit yok. Tip o sayının **birimini** bile
bilmiyor.

```
   dört float (x, y, eşik)      hangi cihazdan geldiği belirsiz   ► GEÇER
   Input/Camera/Time çağrısı     cihazı ve motoru tipe yazar       ► GEÇMEZ
```

Yani dışarıdan sayı almak bu tipin **deseni**; reddedilen şey sayıyı
**kendisinin toplaması**.

### Faturanın adı: EditMode'da sınanabilirlik

Bu duvar bir estetik tercih değil, ölçülebilir bir kazanç:

```
  ŞİMDİ    PointerGestureTests  ──►  24 test, EditMode'da, saniyeler içinde
                                     eşik, titreme, savurma, alt+tab —
                                     hepsi dört float ile kuruluyor

  DUVAR YIKILSAYDI
           Core'un asmdef'i değişmeden dosya DERLENMEZ
           ayar değiştirilse bile: eşiği sınamak için GERÇEK bir fare
           sürüklemek gerekir ► 24 testin taşınacak yeri yok
```

Doğrulanmış test adlarından birkaçı, hangi cümleyi koruduklarıyla:

| test | koruduğu cümle |
|---|---|
| `MoveTo_ExactlyAtThreshold_StaysPressed` | eşik sınırı DAHİL: tam eşik hâlâ tıklama |
| `MoveTo_MeasuresFromThePressOrigin_NotFromTheLastMove` | ölçüm basma noktasından, adım adım değil |
| `MoveTo_BackInsideThreshold_StaysDragging` | `Dragging`'den geri dönüş yok |
| `Release_CrossingThresholdOnTheReleaseFrame_IsDragReleased` | bırakma karesi de ölçülür |
| `Press_WhileAlreadyPressed_RestartsFromTheNewOrigin` | yutulmuş bırakış oyunu düşürmez |
| `Constructor_ZeroThreshold_IsAllowed` | sıfır eşik geçerli bir ayar |

Hiçbiri Unity açmadan koşuyor. Duvarın satın aldığı şey tam olarak bu.

---

## Beşinci durak: karar — durum makinesi

Duvarın altında tek bir tablo var ve üç metot bu tablodan başka bir şey yapmıyor.

```
 kip ↓ / olay →     Press          MoveTo(x,y)              Release(x,y)
 ═══════════════   ═══════════   ═══════════════════════   ══════════════════════
 Idle              Pressed        — yok sayılır             — yok sayılır
 Pressed           Pressed*       aşıldı   ► Dragging       aşıldı   ► DragReleased
                                  aşılmadı ► Pressed        aşılmadı ► ClickReleased
 Dragging          Pressed*       ██ HER ZAMAN Dragging ██  DragReleased
                                       ◄── ① YASAK GEÇİŞ
 ClickReleased     Pressed*       — yok sayılır             — yok sayılır
 DragReleased      Pressed*       — yok sayılır             — yok sayılır
 ───────────────────────────────────────────────────────────────────────────────
 Reset()  ►  hangi kip olursa olsun ► Idle  (+ basma noktası SİLİNİR)

 * her kipten gelen Press YENİ bir jest başlatır; fırlatmaz
```

### ① Yasak geçiş: bir `if` ile değil, YAPI ile

`Dragging` satırında "Pressed" yazan **hiçbir hücre yok**. Eşiğin içine geri
dönen bir işaretçi hâlâ sürüklüyordur. O hücre yasaklanmadı — **hiç yazılmadı**.

Kodda karşılığı, `MoveTo`'nun ilk satırı:

```csharp
if (Phase != PointerPhase.Pressed)
{
    return Phase;              // Dragging buradan çıkamaz, çünkü giremez
}
```

Neden önemli — eşikte titreyen bir el:

```
 eşik 10 birim; mesafe HER ZAMAN basma noktasından ölçülüyor
 oyuncu hedefe götürüp hafifçe geri çekiyor

 kare              1     2     3     4     5     6 (Release)
 mesafe            4    11     9    12     8     9
 eşik aşıldı       ✗     ✓     ✗     ✓     ✗     ✗
 ──────────────────────────────────────────────────────────────
 SEÇİLEN           P     D     D     D     D  ►  DragReleased
                         └── bir kez geçildi, GERİ DÖNÜŞ YOK
 REDDEDILEN        P     D     P     D     P  ►  ClickReleased
                               └── aynı hareket, bırakma anına göre
                                   FARKLI sonuç  ◄── kırılma noktası
```

Her karede yeniden karar veren bir kip, eşiğin tam üstünde titreyen bir elde
saniyede onlarca kez `Pressed` ile `Dragging` arasında atlar ve hayalet
oyuncunun hiç istemediği bir anda yerleşir.

### Ölçüm neden basma noktasından

`pressX`/`pressY` bir önceki `MoveTo`'dan değil, `Press`'ten geliyor. Adım adım
ölçseydik yavaşça yüz piksel sürüklenen bir işaretçi **hiçbir adımda** eşiği
aşmazdı ve jest sonuna kadar "tıklama" kalırdı.

### Karekök neden alınmıyor — hız değil, SINIR

```
 SEÇİLEN — kare ile karşılaştır
   (dx*dx + dy*dy) > dragThresholdSquared
   ╔══════════════════════════════════════════════════╗
   ║ tipin eline geçen: bir EVET/HAYIR                ║
   ╚══════════════════════════════════════════════════╝

 REDDEDILEN — karekök al
   Math.Sqrt(dx*dx + dy*dy) > dragThreshold
   ╔══════════════════════════════════════════════════╗
   ║ tipin eline geçen: önce bir MESAFE, sonra bir    ║
   ║ evet/hayır                                       ║
   ║   └─► `public float Distance => ...` diye bir    ║
   ║       üye için YALVARAN sayı burada doğar        ║
   ║       ◄── SINIRIN AŞILDIĞI NOKTA                 ║
   ╚══════════════════════════════════════════════════╝

 Aynı cevabı vermesi tesadüf değil — kare alma negatif olmayan sayılarda
 artan bir fonksiyondur, sıralamayı korur:
   d  : 0 ──────── t ────►
   d² : 0 ──────── t² ───►        d > t  ⇔  d² > t²
```

Sorulan soru "ne kadar uzağa gitti" değil, "eşiği aştı mı". Karekök almak
kimsenin istemediği bir **sayı** üretir; o sayı üretildiği an bir `Distance`
property'si olmak için yalvarır ve tip, `GridDistance`'ın işini ikinci kez
yapmaya başlar. Ölçünün sahibi bu tip değil.

### Kipin tek yazarı: `private set`

```
 SEÇİLEN — private set
   Press ──┐
   MoveTo ─┼──► ╔═══════╗   dışarıdan gelen ok YOK
   Release ┼──► ║ Phase ║   her yazma, tablonun bir satırı
   Reset ──┘    ╚═══════╝

 REDDEDILEN — public set
   çağıranın herhangi bir satırı ──► ╔═══════╗
                                     ║ Phase ║ ◄── ██ KIRILMA ██
                                     ╚═══════╝
   Tablo artık bir SÖZ değil, bir ÖNERİ: "Dragging'den Pressed'e
   dönüş yok" güvencesi tek bir atamayla aşılır.
```

Karşı örnek aynı dosyada: `Reset()` **public**'tir ve kipi dışarıdan gelen bir
istekle değiştirir. Fark şu — `Reset` bir kip **adı almaz**, yalnız "iptal" der;
hedefi tip seçer. Dışarısı geçişi **tetikleyebilir, adlandıramaz**.

### Alt+tab'da yutulan olay

Tipin en cömert kararı burada:

```
 kare    1        2         3 (alt+tab)      4          5
 olay    Down     Move      — Up YUTULDU     —          Down
 kip     Pressed  Dragging  Dragging ◄── ①   Dragging   ?

 ① Tipin bakış açısından jest hâlâ sürüyor; onu bitirecek olay
   hiç gelmedi ve bir daha GELMEYECEK. Bu hâl kendi kendine düzelmez.

 5. karede iki cevap mümkün:
   REDDEDILEN  throw  ──► oyun düşer; oysa kullanıcı yalnızca yeni
                          bir tıklama yaptı
   SEÇİLEN     Press  ──► yeni jest, yeni ölçüm noktası
```

Aynı tip, iki farklı bozukluğa **zıt** cevap veriyor ve ayıran şey üslup değil,
kaynağın güvenilirliği:

```
   bozuk SIRA  (beklenmedik olay akışı, Update döngüsünden)  ► onar
   bozuk DEĞER (NaN / negatif eşik, bir HESAPTAN)            ► fırlat
```

Kurucu bozuk bir eşik için tereddütsüz fırlatıyor (`Constructor_NaNThreshold_Throws`,
`Constructor_NegativeThreshold_Throws`). Çünkü orada değer bir hesaptan geliyor
ve bozuksa yalnızca kod hatası olabilir.

### Bırakma karesi: mesafenin katedildiği yer

`Release` de eşiği sınıyor ve parametreleri süs değil.

```
 eşik 10 birim. Oyuncu geniş bir yay çizip savurmanın sonunda bırakıyor.

 kare              1 (Down)     2 (Move)     3 (Up)
 konum             (0,0)        (3,0)        (40,0)
 basmadan uzak      0            3            40
 ────────────────────────────────────────────────────────────────
 motorun cevabı                 GetMouseButton 3. karede FALSE
                                ► MoveTo o karede ÇAĞRILMAZ
                                ► (40,0) tipe SADECE Release'ten ulaşır
 ────────────────────────────────────────────────────────────────
 REDDEDILEN   konum yok sayılır ► eşik HİÇ görülmez
                                  ► ClickReleased ◄── yapı yerleşmez,
                                    oyuncu bıraktığını sanır
 SEÇİLEN      Release ölçer     ► eşik son karede aşılır
                                  ► DragReleased
```

Bedeli ödendi ve gizlenmiyor: *"Release, Pressed iken → ClickReleased"* artık tek
başına doğru değil. `Pressed` iken bırakılan bir jest, son konum eşiğin
**dışındaysa** `DragReleased` döner.

Garantinin sınırı da yazılı: bu tip çağıranın `MoveTo`'yu her karede çağırdığını
**zorlayamaz** — böyle bir söz derlenmez. `Release`'in kendi ölçümü tam olarak bu
zorlanamazlığın karşılığı. Çağıran hiç `MoveTo` çağırmasa bile jest doğru biter.

### İki sonuç kipi, iki ayrı davranış

```
 giriş şekli      son kip         çağıranın yaptığı
 ──────────────   ─────────────   ────────────────────────────────────
 sürükle-bırak    DragReleased    CommitPlacement ► kipten ÇIKILIR
 tıkla-bırak      ClickReleased   ghostIsCarried false ise:
                                    hayalet fareye YAPIŞIR, kipte kalınır
                                  ghostIsCarried true ise:
                                    CommitPlacement ► ikinci tıklama yerleştirir
 ──────────────────────────────────────────────────────────────────────
 REDDEDILEN       tek "Released"  iki satırdan BİRİ seçilmek zorunda;
                                  seçilmeyen GİRİŞ ŞEKLİ tamamen ölür
```

Sonuç kipleri **kalıcı**: bir sonraki `Press` ya da `Reset`'e kadar okunabilir
kalır. Bu yüzden kararı üreten kare ile tüketen kare aynı olmak zorunda değil.

---

## Altıncı durak: niyet ile geçerlilik

Kip kapalıyken tıklama `HandleClick`'e gidiyor ve orada bambaşka bir ayrım var.

```
 tıklama hücreye çevrildi
   │
   ├── tahta DIŞI            ──► Debug.Log, çık
   │
   ├── hücre DOLU  ──┬── seçim YOK            ──► SEÇ
   │                 ├── tıklanan == seçili   ──► SEÇİMİ BIRAK    ◄── ██
   │                 │      ██ AKIŞ BURADA BİTER ██
   │                 └── tıklanan != seçili   ──► SALDIR
   │
   └── hücre BOŞ   ──┬── seçim YOK            ──► Debug.Log, çık
                     └── seçim VAR            ──► HAREKET
```

İşaretli dal tek satır:

```csharp
if (ReferenceEquals(clicked, selectedUnit))
{
    ClearSelection();
    return;
}
```

Bu satır silinirse ne olur? **Hiçbir şey patlamaz.** Kod derlenir, testler yeşil
kalır. Çünkü aşağıdaki çağrı — `BattleActions.Move(battle, selectedUnit, x, y,
moveRange)` — bu hareketi **kabul eder**. `MoveAction`'ın doluluk kontrolü
birimin kendisini bilerek dışarıda bırakıyor:

```csharp
// MoveAction.cs
if (board.TryGetUnit(toX, toY, out Unit occupant)
    && !ReferenceEquals(occupant, unit))          // ◄── kendisi ENGEL sayılmaz
```

Yani kural "evet" der, oyuncu "bırak" demişti.

```
 ── İŞ BÖLÜMÜ ────────────────────────────────────────────────────
   BoardAdapter'daki ReferenceEquals   ► NİYET   "oyuncu ne demek istedi"
   MoveAction'ın kontrolleri           ► GEÇERLİLİK "kural izin veriyor mu"

 Niyet dalı silinirse   ► geçerlilik "evet" der, tıklama sessizce bir
                          harekete dönüşür — belirti YOK, hata SESSİZ
 Geçerlilik silinirse   ► niyet doğru okunur ama TAHTA bozulur —
                          birim duvardan geçer, dolu hücreye biner
```

İkisi aynı soruyu iki kez sormuyor: **geçerliliğin "evet" dediği yerde niyetin
"hayır" diyebilmesi** tam olarak o dalın var olma sebebi.

Karşı örnek aynı dosyada, `HandleEmptyCellClick`: orada **tam olarak aynı çağrı**
hiçbir ön kontrol olmadan yapılıyor ve doğru. Çünkü seçim varken boş bir hücreye
tıklamanın başka okuması yok. Aynı dosya, aynı çağrı, zıt karar; farkı yaratan
tek şey hedef hücrede seçili birimin durması.

Yerleştirme tarafında da aynı sınır, aynı keskinlikte:

```csharp
private void CommitPlacement(int x, int y)
{
    Unit placer = selectedUnit;
    PlacementOutcome outcome =
        BattleActions.PlaceStructure(battle, placer, NewStructure(placer), x, y);
    ...
}
```

Bu metotta ne bir sınır kontrolü, ne "hücre dolu mu" sorusu, ne de bir sıra
sorusu var. Hepsi `PlaceStructure`'ın içinde. Çeviri ile karar arasındaki sınır
tam olarak burası.

---

## Yedinci durak: ekran

Yolun sonunda `UnitView` var ve ondan istenen şey tek cümle:

```csharp
view.SetSelected(true);       // BoardAdapter → UnitView
```

`UnitView` "kim seçildi", "neden seçildi", "seçilebilir miydi" diye sormuyor.
Bir `bool` alıyor ve `selectionOverlay.enabled` yazıyor. **Hiçbir şey
saklamıyor** — "seçili miyim" bilgisinin tek doğruluk kaynağı
`BoardAdapter.selectedUnit`.

```
 ┌─BoardAdapter──────┐          ┌─Combatant─────────┐
 │ selectedUnit      │          │ State             │
 └─────────┬─────────┘          └─────────┬─────────┘
    SetSelected(bool)            UnitStateChanged olayı
           │                       -> ApplyStateVisual
           ▼                               ▼
 ╔═════════════════════ UnitView ══════════════════════╗
 ║  selectionOverlay.enabled  │  Body.flipY / .color   ║
 ║      (yalnız YAZILIR)      │    (yalnız YAZILIR)    ║
 ╚════════════════════════════╪════════════════════════╝
                              ◄── İKİ EKSEN BURADA KESİŞMEZ
```

Tıklama yolculuğu burada bitiyor: bir piksel, dört float, bir kip, bir niyet, bir
kural, ve nihayet açılan bir `enabled` bayrağı.

---

## Bütün yol tek bakışta

```
 Input.mousePosition                          EKRAN pikseli
        │
        │  TryReadPointerCell  ── Camera.main.ScreenToWorldPoint
        ▼
 worldX, worldY  ──────────────┐              DÜNYA birimi
        │                      │
        │  WorldToCell         │
        ▼                      │
 x, y (hücre)                  │              HÜCRE indeksi
        │                      │
        │                      │  ██ ASMDEF DUVARI ██  yalnız float geçer
        │                      ▼
        │              ┌─ PointerGesture ──────────────────────┐
        │              │  Press / MoveTo / Release / Reset     │
        │              │  eşiği KARE ile sınar, kipi HATIRLAR  │
        │              └───────────────┬───────────────────────┘
        │                              │
        │                       PointerPhase
        │                              │
        │        ┌─────────────────────┴─────────────────────┐
        │        ▼                                           ▼
        │  ClickReleased                               DragReleased
        │        │                                           │
        │        │ ghostIsCarried ? ─── hayır ──► YAPIŞ       │
        │        │                 └── evet ──┐              │
        ▼        ▼                            ▼              ▼
 ┌──────────────────────┐            ┌──────────────────────────┐
 │  HandleClick         │            │  CommitPlacement         │
 │  ██ NİYET ██         │            │  ██ NİYET ██             │
 │  boş → hareket       │            │  bırakıldığı yer         │
 │  dolu → saldırı      │            │                          │
 │  kendine → bırak     │            │                          │
 └──────────┬───────────┘            └──────────┬───────────────┘
            │                                   │
            ▼                                   ▼
       ┌────────────────── BattleActions ──────────────────┐
       │  Attack / Move / PlaceStructure                   │
       │  ██ GEÇERLİLİK ██  sıra, menzil, doluluk, sınır   │
       └────────────────────────┬──────────────────────────┘
                                │
                    MoveOutcome / AttackOutcome / PlacementOutcome
                                │
                                ▼
                   ┌──────────────────────────┐
                   │  UnitView.SetSelected    │
                   │  CreateStructureVisual   │
                   │  ██ UYGULAMA ██          │
                   └──────────────────────────┘
```

**Dört ayrı iş, dört ayrı sahip:** çeviri (`BoardAdapter`), karar
(`PointerGesture`), niyet (`BoardAdapter` — hâlâ, ve bu bilinen bir borç),
geçerlilik (`BattleActions`).

Niyetin hâlâ `BoardAdapter`'da olması dosyanın kendi rol notunda **KOKU** olarak
adı konmuş durumda. Eşik de yazılı: bu üç dala dördüncüsü eklendiği gün
(sıra sorgusu, çoklu seçim, hedef önizlemesi) `PointerGesture`'ın ikizi doğmalı —
`(x, y)` + tahtanın durumu alıp bir **niyet** döndüren saf bir tip.

---

## Kural: yeni bir giriş jestini nereye yazarsın

Uzun basma, çift tıklama, sağ tık, iki parmakla kaydırma. Sırayla sor:

```
① Bu jestin cevabı YALNIZCA nokta akışından türetilebilir mi?
   (basma noktası + şu anki nokta + eşik — başka HİÇBİR ŞEY)

      EVET  → ██ PointerGesture ██
              geçiş tablosuna yeni bir satır/sütun, PointerPhase'e yeni
              bir kip. Testi EditMode'da yazılır, Unity açılmaz.
              örnek: "eşiği aştıktan sonra geri dönerse iptal olsun"
                     → Cancelled kipi, MoveTo'da yeni bir hücre

      HAYIR → ②

② Cevap için ZAMAN gerekiyor mu?  (uzun basma: "kaç saniyedir basılı")

      EVET  → yine PointerGesture, ama zaman GİRDİ olarak verilir:
              Press(x, y, elapsed)  ya da  Tick(deltaSeconds)
              ██ Time.deltaTime OKUNMAZ ██ — duvarı Time geçemez
              saat BoardAdapter'da kalır, ölçüm Core'da yapılır
              (aynı desen zaten var: Battle.Tick(Time.deltaTime))

      HAYIR → ③

③ Cevap için TAHTANIN DURUMU gerekiyor mu?
   (çift tıklama → "aynı türden hepsini seç"; sağ tık → "hedefi önizle")

      EVET  → ██ PointerGesture DEĞİL ██
              Bu tip tahtayı bilmez ve bilmemeli. İstenen şey dosyanın
              başında adı konmuş "PointerGesture'ın ikizi": (x, y) +
              tahta durumu alan, bir NİYET değeri döndüren saf tip.
              Bugün o niyet HandleClick'in içinde yaşıyor; ikinci
              müşterisi doğduğu gün dışarı çıkar.

      HAYIR → ④

④ Cevap yalnızca motorun KARE DÜZENİNDEN mi geliyor?
   (hangi düğme, kaçıncı tık, hangi tuş basılı)

      EVET  → ██ BoardAdapter'ın çeviri yarısı ██
              FeedGesture'a yeni bir sorgu; jest tipine yeni bir metot
              DEĞİL. Örnek: sağ tık için GetMouseButtonDown(1) —
              PointerGesture'a "hangi düğme" diye bir kavram GİRMEZ,
              ikinci bir PointerGesture ÖRNEĞİ açılır.
```

Ve tek satırlık özet: **eşik `PointerGesture`'a, düğme `BoardAdapter`'a,
tahta `BattleActions`'a aittir.** Bir jest bu üçünden hangisinin sözlüğünü
konuşuyorsa oraya yazılır.

---

## Yanlış hatırlanan üç şey

**"Her tıklama `PointerGesture`'dan geçer."** Geçmez. `Update`'in kip ayrımı
akışı ikiye bölüyor ve tıklama yolu (`HandleClick`) yalnız `GetMouseButtonDown(0)`
okuyor — jest tipine hiç uğramıyor. `FeedGesture`'ın bugünkü tek çağıranı
`UpdatePlacement`. Bu bir eksiklik değil: sürükleme kavramına ihtiyaç duyan tek
giriş kipi yerleştirme.

**"Üç `Input` sorgusu fazlalık; `Down` ile `Up` yeterdi."** Yarı doğru olduğu
için tehlikeli. `GetMouseButton` kalksaydı `Release` son konumu yine ölçerdi ve
*sonuç* çoğu jestte doğru çıkardı — bugünkü yerleştirme akışı sessizce çalışmaya
devam ederdi. Kaybolan şey sonuç değil, **jestin ortası**: kip bütün basılı
kareler boyunca `Pressed` okunur, yani "şu an sürüklüyor" diye bir olgu hiç
doğmaz. Sürüklerken değişen bir imleç, canlı bir geçerlilik boyaması ya da
yalnız sürüklemede beliren bir yardımcı çizgi **yazılamaz** hâle gelir; ve
`MoveTo_BeyondThreshold_BecomesDragging` ile `MoveTo_BackInsideThreshold_StaysDragging`
gibi testlerin sınayacağı bir davranış kalmaz. Üç sorgu üç ayrı kare kümesini
kapatıyor ve kümeler kesişmiyor.

**"Sonuç kipi okunduktan sonra kendiliğinden sıfırlanır."** Sıfırlanmaz.
`ClickReleased` ve `DragReleased` bir sonraki `Press` ya da `Reset`'e kadar
okunabilir kalır — ve bu bilinçli: kararı üreten kare ile tüketen kare aynı olmak
zorunda değil. `UpdatePlacement`'ın `ClickReleased` dalında `gesture.Reset()`
çağırmasının sebebi tam olarak bu; sıfırlanmasaydı sonraki kare aynı sonucu
ikinci kez okur ve hayalet bir kare sonra kendi kendine yerleşirdi.

**Bonus, sık karıştırılan:** `MoveTo` "tam eşik kadar" giden bir jesti
sürüklemeye çevirmez. Karşılaştırma **kesin büyüktür** ve eşik "bu kadar
oynayabilirsin ve hâlâ tıklıyorsun" diye okunur; sınır dahildir
(`MoveTo_ExactlyAtThreshold_StaysPressed`).

---

## Kaçış yolu: kararı `BoardAdapter`'da bırakmak

Bütün bu ayrım tek bir alternatife karşı verildi. O alternatif şu:

```csharp
// BoardAdapter.cs — jest tipi HİÇ DOĞMASAYDI
private Vector3 pressWorld;
private bool isDragging;

private void UpdatePlacement()
{
    if (Input.GetMouseButtonDown(0)) { pressWorld = world; isDragging = false; }
    else if (Input.GetMouseButton(0))
    {
        if ((world - pressWorld).sqrMagnitude > dragThreshold * dragThreshold)
        {
            isDragging = true;
        }
    }

    if (Input.GetMouseButtonUp(0))
    {
        if (isDragging) { CommitPlacement(x, y); }
        else { /* tıkla-bırak */ }
    }
}
```

Otuz satır yerine on beş. Yeni bir dosya yok, yeni bir tip yok, dolaylılık yok.

**NE KAZANDIRIRDI**

```
  ► Bir dosya az. Bir asmdef sınırı az. Bir kurucu az.
  ► Vector3 ve sqrMagnitude doğrudan kullanılabilirdi — dört float'ı
    elle taşımak yerine motorun kendi matematiği.
  ► "Bu tıklama mıydı" sorusunun cevabı, sorulduğu yerin iki satır
    aşağısında dururdu. Okuması kolay.
  ► Bugünkü tek müşteri yerleştirme kipi. TEK çağıranı olan bir tipi
    dışarı çıkarmak, çoğu zaman erken soyutlamadır.
```

**NE KAYBETTİRİRDİ — ve sırayla**

```
  ① EditMode'da 24 test YAZILAMAZ
     Eşiği sınamak için gerçek bir fare sürüklemek gerekir. Titreyen
     el, hızlı savurma, alt+tab'da yutulan olay, tam eşikteki eşitlik —
     dördü de elle denenen, kayda geçmeyen senaryolara döner.
     ██ EN AĞIR KAYIP BU ██

  ② İki BAYRAK, tek KİP yerine
     `isDragging` bir bool ve beş hâl bir bool'a sığmaz: Idle ile
     ClickReleased ikisi de false'a düşer. "Jest BİTTİ mi" sorusu
     sorulamaz hâle gelir ve çağıran ikinci bir bayrak tutmak zorunda
     kalır (`ghostIsCarried`'ın yanında bir de `hasReleased`).
     Sıfırlanmadığı gün hayalet fareye yapışıp kalır.

  ③ Yasak geçiş YAPI ile korunamaz
     `isDragging = true` bir atamadır; onu `false` yapan bir satır
     yarın herhangi bir yere yazılabilir ve derleyici susar.
     `Dragging`'den çıkan yolun OLMAMASI, bir bool'da ifade edilemez.

  ④ Giriş cihazına kilitlenme
     "Sol fare düğmesi" kavramı kararın içine girer. Dokunmatik,
     atanmış tuş ya da kayıttan oynatma aynı akışa giremez.

  ⑤ Aynı mantığın İKİNCİ kopyası
     Sağ tık jesti eklendiği gün pressWorld/isDragging çifti ikinci kez
     yazılır. PointerGesture'da karşılığı: ikinci bir ÖRNEK.
```

**Neden kaçılmadı — ve dürüst sebep:** tek başına "temiz olsun" yetmezdi.
Karar `BoardAdapter.cs`'in başında **yazılı bir eşiğe** dayanıyor:

> *"dördüncü kural geldiği gün Core tarafına bir 'komut' sahibi çıkmalı:
> tıklamayı niyete çeviren saf bir tip."*

Yerleştirme kipi bütün bir **giriş kipi** ekledi — sürükle-bırak ve tıkla-bırak
aynı anda yaşayacak — ve eşik aşıldı. Cevap `BoardAdapter`'ı büyütmek değil,
kararı dışarı çıkarmaktı.

Ama çıkan yarı **niyet** değil **jest** oldu. Niyet (boş → hareket, dolu →
saldırı, kendine → seçimi bırak) hâlâ `HandleClick`'in içinde ve Unity'siz
sınanamaz durumda. Bu yarım kalmışlık gizlenmiyor; rol notunda **KALAN YARI**
başlığıyla adı konmuş.

Yani kaçış yolu bugün de açık duruyor — yalnızca faturası ölçüldü ve ödenmedi.

---

## Bunu okuduktan sonra kodda ne göreceksin

`BoardAdapter.cs`'in başındaki **GİRİŞ OKUMA NOTU** üç sorguyu tablo olarak
veriyor; `PointerGesture.cs`'in başındaki blok geçiş tablosunu ve duvarı
çiziyor. İkisi de kararın kendisini ve reddedilen alternatifini söylüyor —
yolculuğu anlatmıyor. Yolculuk burada.

Kodda karar, burada hikâye. İkisi çelişirse **kod kazanır** — orası çalışan
metin, burası anlatı.
