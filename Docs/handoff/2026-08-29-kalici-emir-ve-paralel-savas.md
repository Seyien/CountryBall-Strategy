# Devir — kalıcı emir, paralel savaş ve arayüz kenarları

> **Tarih:** 2026-08-29 · **Durum:** BU TURUN DÖRT İŞİ DE KAPANDI, henüz commit YOK
> (bir önceki tur kapalı ve commit'li: 9 commit, push YOK)
> Bu belge, `Docs/handoff/2026-08-28-uretim-paneli-devri.md`'nin devamıdır.
> Önce onu oku: kapalı gerçekler, ölçülmüş kısıtlar ve açık borçlar orada.

## Bir bakışta — bugünkü sahiplik ve nerede kırıldığı

```
GIRDI                      TAHTANIN KIPI              BIRIMIN NIYETI
oyuncu tiklamasi   ->   BoardModeMachine        ->    ??? YOK
                        (TEK, ayni anda tek kip)      pendingStrike* dortlusu
                        Idle / Placement /            BoardAdapter'da TEK KOPYA
                        PendingStrike                 => ayni anda TEK emir

                                                      ^^^^^^^^^^^^^^^^^^^^^^^^
                                                      operatorun "iki taraf
                                                      paralel olmuyor"
                                                      sikayetinin KOK SEBEBI
```

`BoardAdapter` bekleyen vuruşu dört tekil alanda tutuyor
(`pendingStrikeAttacker`, `pendingStrikeTarget`, `pendingStrikeX/Y`). İkinci bir
birime emir verildiği an birincisinin emri siliniyor. Paralel emir bir ayar
eksikliği değil, **sahiplik hatası**: emir tahtaya değil BİRİME ait olmalı.

## Bu turda yapılacaklar — DÖRDÜ DE KAPANDI

> Aşağıdaki dört bölüm YAZILDIĞI GİBİ duruyor; değişen tek şey, her birinin
> başına ne yapıldığının ve nerede sınandığının eklenmesi. Tasarım gerekçesini
> silip yerine "yapıldı" yazmak, kararı okunamaz hâle getirirdi.
>
> | iş | durum | ürün kodu | ölçen test |
> |---|---|---|---|
> | İŞ-1 kalıcı emir | KAPALI | `Assets/Game/Unity/Orders/` (6 dosya) | `UnitOrderTests` 21 test |
> | İŞ-2 seçim bırakma | KAPALI | `BoardAdapter.IssueOrder` / `DescribeOrder` | `IssueOrder_*` 2 + `HandleOccupiedCellClick_OnAUnitThatHoldsAnOrder_*` |
> | İŞ-3 durum şeridi | KAPALI | `BattleStatusView.WriteSide` | `TurnMode` dalı, yeni mekanizma yok |
> | İŞ-4 kenar boşluğu | KAPALI | `SceneSetupTool.ScreenMargin` + `TrashLabelHeight` | test yok — Editor aracı, ölçüsü operatörün ekranı |
> | İŞ-5 üretim geri sayımı | KAPALI | `ProductionTimerView` + `IPlacementBoard.ShowProductionCountdown` | `StructureProductionTests` 5 + `ShowProductionCountdown_*` 4 |
> | İŞ-6 yapı seçimi görünür | KAPALI | `BoardAdapter.SetStructureSelectionFrame` | `SelectUnit_OnAStructure_AlsoDrawsASelectionFrame` |
> | İŞ-7 odak devri | KAPALI | `CanStructureAttack` + `TransferFocusTo` | `MovesTheFocus*` 2 + `KeepsTheFocusAndAttacks` |
> | İŞ-8 kamera kaydırma | KAPALI | `BoardViewport.ClampCentre` + `BoardCameraRig` | `BoardViewportTests` kaydırma 6 |
> | İŞ-9 yakınlaştırma | KAPALI | `BoardViewport.ClampHalfHeight` | `ClampHalfHeight_*` 3 |
> | İŞ-10 dinamik çerçeve | KAPALI | `BoardViewport.FitHalfHeight` | `FitHalfHeight_*` 4 |
> | İŞ-11 kırmızı hayalet | KAPALI | `BoardAdapter.PreviewAt` + `PlacementPreview` | `PreviewAt_*` 3 + `SetPlacementGhost_*` 3 + ikiz üye 1 |
> | İŞ-12 5000 hücre | KAPALI | `BoardAdapter` → `Tilemap` ×2 + `TileFor` | `BoardTilemapTests` 6 |
> | İŞ-13 deniz mesh patlaması | KAPALI | `SceneSetupTool.TileScaleFor` | test yok — Editor aracı, kanıtı Console'un susması |
> | İŞ-14 tahta taraması | KAPALI | `UnitGrid.TryGetPosition` ters dizin | `UnitGridTests` +8 (senkron testleri) |
> | İŞ-15 kuralın kalıcılaştırılması | KAPALI | `Tools/check-scale-ceilings.py` + skill kural 20 | kapı iki kollu doğrulandı |
> | İŞ-16 menzil penceresi | KAPALI | `TryFindStructureTarget` | 738 mevcut test; eşdeğerlik Chebyshev'den ispatlı |
> | İŞ-17 imlece yakınlaştırma | KAPALI | `BoardViewport.ZoomTowards` | `ZoomTowards_*` 5 test |
> | İŞ-18 sol tuşla kaydırma | KAPALI | `BoardPointerArbiter` + rig komutları | `BoardPointerArbiterTests` 19 test |

### İŞ-5 (P1) — üretim geri sayımı, savaşçı üreten binanın tepesinde

Operatör: *"savaşçı üreten kışlaların canın üstünde ne kadar süre sonra
tekrardan üretebileceğini gösterelim... son 3 saniye yarış araba oyunları olur
ya o tarz."* Ve kelepçesi: *"saldırı yapan kulelerin illa ki bunu göstermesine
gerek yok."*

**Şekil: lamba dizisi, rakam DEĞİL.** Yazı reddedildi ve gerekçesi ölçüldü —
projede TextMeshPro yok (`Packages/manifest.json` yalnız `com.unity.ugui`
taşıyor), dünya uzayında rakam ya gömülü bir yazı tipine ya da bina başına bir
Canvas'a bağlanmayı isterdi. Lamba dizisi aynı bilgiyi taşıyor ve zaten istenen
benzetme de o: yarış ışığında okunan şey rakam değil, kaç lamba kaldığıdır.
Lamba sayısı = kalan tam saniye, yani "1 saniye 1 saniye düşürtme" ayrı bir
animasyon değil, gösterimin kendisi.

**Sahiplik zinciri — üç sahip, tek yön:**

```
StructureProduction.ProducesUnits     "bu bina uretir mi"  (CEKIRDEK, TUR)
StructureProduction.RemainingSeconds  "kac saniye kaldi"   (CEKIRDEK, SAYAC)
        │
        ▼  ProductionDirector.Update — zaten her kare bu tabloyu geziyor
IPlacementBoard.ShowProductionCountdown(kimlik, kalan, toplam)
        │
        ▼  BoardAdapter — gorselleri tutan taraf
ProductionTimerView.SetRemaining
```

`ProducesUnits` çekirdekte, ekranda değil: soru motor tarafında sorulsaydı
`Produces.Count > 0` yazan İKİNCİ bir satır doğar ve üretim listesi bir gün
başka bir kurala bağlandığı gün sessizce eskirdi. Tür ile sayaç ayrı iki üye,
çünkü ayrı iki eksen — biri ömür boyu sabit, öteki her kare değişiyor.

**Arayüze dokuzuncu üye eklendi ve okun yönü DEĞİŞMEDİ:** yeni üye bir soru
değil bir SİPARİŞ ve şekli `SetPlacementGhost` ile aynı. Tersi (tahtanın her
karede müdüre "kaç saniye kaldı" diye sorması) reddedildi — o gün bağ çift yönlü
olur ve arayüzün bütün kazancı biterdi.

### İŞ-6 (P1) — yapı seçimi ekranda görünsün

Operatör: *"bazen yapılara tıkladığımda ne yazık ki seçili oldukları
gözükmüyor."* Kök sebep `StructureSelectedTint`'in bir **çarpan** olması: sıcak
renkli bir binada çarpım neredeyse hiçbir şeyi değiştirmiyordu. Çözüm
savaşçıdan alındı — onun seçimi zaten ayrı bir ÇERÇEVE nesnesinde yaşıyor ve tam
bu yüzden gövde rengine bağlı değil. Çarpan silinmedi, çerçeve üstüne kondu:
seçili hâli iki ayrı kanal anlatıyor.

### İŞ-7 (P1) — silahsız binada tıklama ret değil ODAK

Operatör: *"bir yapıyı seçerken rakip yapıyı seçtiğimde saldıramıyor diyor...
daha çok seçili olayını karşıdaki yapıya veya karşı takımın savaşçısına geçilse
... ama bu tabii ki saldırı yapan yapılar için geçerli değil."*

Eski dal koşulsuz `BattleActions.Attack` çağırıyordu; kışla gibi silahsız bir
bina için cevap her seferinde bir RETTİ ve oyuncu hiç istemediği bir eylemin
reddini okuyordu. Yeni kural tek cümlede: **yapabileceğin bir şey yoksa tıklama
bir eylem değil bir bakıştır.** Ayrımı yapan şey bir tür listesi değil
`Structure.CanAttack` — yeni bir silahlı bina eklendiği gün dal kendiliğinden
doğru tarafta kalıyor.

### İŞ-12/13 (P0) — tahta 100x50 olunca açığa çıkan iki kusur

Operatör tahtayı `10x5`'ten `100x50`'ye çıkardı ve Console iki şey söyledi.
İkisi de **normal değildi**.

**KUSUR 1 — 5616 GameObject.** `[Board] built 100x50 = 5000 cells.` Zemin hücre
başına bir `GameObject` + `SpriteRenderer` kuruyordu; halkayla birlikte
(`borderThickness = 2`) 5616 nesne. Çözüm iki `Tilemap`: 5616 çizici yerine
**iki** çizici. Karo nesneleri görünüm başına paylaşılıyor (`TileFor` —
Flyweight'in bu projedeki üçüncü örneği).

**KUSUR 2 — deniz hiç çizilmiyordu.**
`Cannot generate 9 slice ... Requires 161872 vertices and 242808 indices`
161872 / 4 = **40468 karo**; tek mesh tavanı 65535 köşe = 16383 karo. Unity
mesh'i kurmayı reddediyordu. `TileScaleFor` karo sayısını bütçenin altına
indiriyor — kaplanan **dünya alanı aynı**, yalnız her karo büyük çiziliyor.

**PATTERN GEREKMEDİ VE BU TURUN ASIL DERSİ BU.** Dört aday ölçüldü ve dördü de
reddedildi:

| aday | neden değil |
|---|---|
| Object Pool | Havuz doğup ölen nesneler içindir; hücreler bir kez doğuyor, hiç ölmüyor |
| Flyweight tek başına | Zaten vardı — sprite'lar paylaşılıyordu; azaltılması gereken şey ÇİZİCİ sayısı |
| Elle culling | Unity'nin `TilemapRenderer`'ını daha kötü biçimde yeniden yazmak |
| Tahta boyutunu kısıtlamak | Tasarımcının oyun kararını koda kısıtlatmak |

Doğru cevap bir GoF deseni değil, **motorun kendi sahibiydi**. Bir baskıya
pattern aramadan önce sorulacak soru: *bu işi motor zaten yapıyor mu?*
→ `Docs/deep/konular/09-kararlarin-cevrilmesi.md` madde 10.

### İŞ-14 (P0) — iki doğru kararın ÇARPIMI

`Battle.TryGetPosition` tahtanın tamamını tarıyordu. Bu tek başına bir kusur
değildi ve kalıcı emir de tek başına bir kusur değildi. **Çarpımları kusurdu:**

| ölçüm | değer |
|---|---|
| çağrı başına hücre yoklaması | 5000 (100x50) |
| emir başına kare başına çağrı | 3 (saldıran · hedef · vuruş) |
| on emir, 60 kare/sn | ~9.000.000 yoklama/sn |

Onarım `UnitGrid`'e indi — hücreleri YAZAN tipe, cevabı İSTEYEN tipe değil.
Tek yazma noktası (`WriteCell`) iki gerçeğin (dizi ve ters dizin) ayrışmasını
yapısal olarak imkânsız kılıyor; sekiz yeni test hız değil **senkron** ölçüyor.

Bir önceki devir belgesi bu günü adıyla öngörmüştü: *"o gün önce
TryGetPosition'ın sözlüğe alınması denenir, Burst değil."* Tetikleyici ateşledi
ve yazılı karar uygulandı.

### İŞ-15 — kusur SINIFININ kalıcılaştırılması

Operatör haklı olarak şunu sordu: *"bunun gibi başka mantıksız ele alınmış
karar olabilir ve bunu biz fark etmemiş olabiliriz."* Üç kusur da aynı sınıftan:
**maliyeti tasarımcının değiştirebildiği bir sayıyla büyüyen ama o sayının
tavanı hiçbir yerde yazmayan karar.**

Hiçbiri yakalanmadı — ne derleyici, ne 700 yeşil test, ne dokuz yeşil kapı.
Yakalayan şey bir `Debug.Log` satırıydı.

Onuncu kapı yazıldı: `Tools/check-scale-ceilings.py`. Ölçekle büyüyen bir
döngüde tahsis varsa ve yanında yazılı bir `ÖLÇEK:` tavanı yoksa kırmızı verir.
**İki kolla doğrulandı** — eski kusurlu şekil geri konunca kırmızı
(`TAVANSIZ TAHSIS: 2`), tavan yazılınca yeşil. Tek kol, kuralı uygulayan bir
kapıyı tahsisi tümden yasaklayan bir kapıdan ayırt edemezdi.

Aynı kural `unity-expert-code-quality` skill'ine kural 20 olarak yazıldı ve
mentor kural 56'dan (yük ekseni sayımı) ayrımı açıkça belirtildi: **56 bir
KARARA ateşler, 20 bir KOD ŞEKLİNE.** Bu üç kusur hiçbir karar anı üretmediği
için 56 hiç ateşlemedi.

### İŞ-16 (P1) — kule menzil dışını taramasın

Denetçi bildirdi, kod okunarak doğrulandı: `TryFindStructureTarget` menzilindeki
en yakın düşmanı bulmak için tahtanın TAMAMINI geziyordu. 100x50'de çağrı başına
5000 hücre ve bunların 4919'u zaten `distance > range` diye eleniyordu.

Onarım menzil penceresi: `(2×range+1)²`, bugün en fazla **81 hücre**.

**KAYIP OLMADIĞI İSPATLI, TAHMİN DEĞİL:** `GridDistance` Chebyshev
(`max(|dx|,|dy|)`), yani `distance ≤ range` ile "|dx| ≤ range VE |dy| ≤ range"
AYNI kümedir — pencere, menzil kümesinin ta kendisi. İçerideki mesafe kontrolü
yine de duruyor: bugün gereksiz, metrik Öklit'e dönerse zorunlu.

**`PathFinder` ONARILMADI, BORCU YAZILDI.** Her çağrıda beş dizi tahta boyutunda
tahsis ediliyor (100x50'de 25.000 eleman) ve kodun kendi yorumu *"bu oyunun
tahtası birkaç yüz hücre"* diyordu — yazıldığı gün doğru, bugün yanlış. Onarım
`PathFinder`'ı static olmaktan çıkarmayı gerektiriyor; static tampon bu projenin
bilerek reddettiği global durum olurdu. Tavan (~1000 hücre) ve yeniden açma
eşiği koda yazıldı.

### İŞ-17/18 (P1) — imlece yakınlaştırma ve sol tuşla gezinme

Operatör: *"tek bir noktaya yakınlaşıyor, farenin bulunduğum noktaya doğru
olmuyor"* ve *"tıklı basıp haritada gezinebilmem lazımdı, Clash of Clans
gibimsi... selected olayıyla çakışmaması lazım."*

**YAKINLAŞTIRMA — kök sebep:** yalnız `orthographicSize` değişiyordu ve
ortografik bir kamerada ekranın MERKEZİ sabit kalır, yani her yakınlaşma
tahtanın ortasına gidiyordu. `BoardViewport.ZoomTowards` imleci sabitliyor:

```
yeni merkez = imlec + (eski merkez - imlec) * k        k = yeni boy / eski boy
```

Kelepçelenmiş boy kullanılıyor, istenen boy değil: sınırda `k = 1` ve kamera
hiç kaymıyor.

**SOL TUŞ — yeni pattern gerekmedi, VAR OLAN SARMALANDI.** `PointerGesture`
zaten tıklama ile sürüklemeyi bir eşikle ayırıyordu ama yalnız yerleştirmeye
hizmet ediyordu. `BoardPointerArbiter` onu sarıp bir eyleme çeviriyor:

```
BoardAdapter (OKUR)  ->  BoardPointerArbiter (KARAR)  ->  BoardCameraRig (UYGULAR)
Input, kip, UI           saf C#, sinanabilir              BeginPan/ContinuePan/EndPan
```

**ÇAKIŞMA YAPISAL OLARAK İMKÂNSIZ, bir söz değil:** rig sol tuşu okuyamıyor —
tek okuması `(panButton)` ve o alan `[Min(1)]` ile Inspector'da, `Awake` ile
diskteki eski değerde kelepçeli. İki kollu kilit. Sol tuşu tahtanın kendi
yolunda tek yer okuyor.

**BİLEREK YAPILAN DAVRANIŞ DEĞİŞİKLİĞİ:** sıradan tahta tıklaması artık BASMA
karesinde değil, BIRAKMA karesinde doğuyor — sol sürüklemenin mümkün olmasının
tek yolu bu. Mevcut testler bunu görmedi çünkü `HandleClick`'i yansımayla
doğrudan çağırıyorlar.

**BU KATMANIN İLK TESTLERİ YAZILDI.** Zemin bugüne kadar hiç test edilmiyordu;
kusur da tam oradan çıktı — 5000 nesne doğdu ve hiçbir şey kırmızıya dönmedi,
çünkü sayacak bir iddia yoktu. `BoardTilemapTests` tahtayı 100x50 kurup çizici
SAYIYOR.

### İŞ-8/9/10 (P1) — haritada gezinme: kaydırma, yakınlaştırma, dinamik çerçeve

Operatör üç şeyi birden istedi ve üçü de **aynı sahibe** düştü: *"haritanın
gridini %50 yaptığımda düzgün ortalanmıyor"*, *"tıklı basıp kaydırdığımız...
ama gezdiğimizin de bir sınırı olmalı, GTA gibi"*, *"yakınlaştırma ve
uzaklaştırmanın da limiti olmalı"*.

**KÖK SEBEP (ortalama):** çerçeveleme Editor aracında **bir kez**, o andaki en
boy oranıyla hesaplanıyor ve çalışma zamanında bir daha hiç sorulmuyordu. Doğru
hesap, yanlış anda. → `Docs/deep/konular/09-kararlarin-cevrilmesi.md` madde 9.

**SAHİPLİK — iki katman, motor çağrısı tek yerde:**

```
BoardCameraRig (MonoBehaviour)     BoardViewport (saf C#)
  fare, tekerlek, kare               FitHalfHeight   -> oran degisince cerceve
  Camera.transform yazar             ClampHalfHeight -> yakinlastirma tavani
       │                             ClampCentre     -> kaydirma siniri
       └────────── sorar ──────────────────▲
```

Sınırın **gözle** değil **testle** doğrulanabilmesinin tek sebebi bu ayrım:
`BoardViewport` içinde ne `Camera` var ne `Transform`, yalnız sayı girip sayı
çıkıyor.

**KAYDIRMA KURALI — klasik çözüm bu tahtada ÇALIŞMIYOR.** Alışılmış kural
"görüş dikdörtgeni tahtanın içinde kalsın"dır; burada tahta 10x5 ve kamera onu
tümüyle görüyor, yani yarım genişlik tahtanın yarısından BÜYÜK. Aralık ters
dönüyor ve kaydırma hiç olmuyor. Yerine konan kural **örtüşmeye** bakıyor:
görüş ile tahta her eksende en az `minVisibleCells` kadar kesişsin. Operatörün
cümlesi bu — *"en sol köşeye kaydık, en sağ üstte en azından tuğlaları görmemiz
lazım."*

**SOL TUŞ REDDEDİLDİ, KAPI AÇIK BIRAKILDI.** Operatör sol tuşu tarif etti; sol
tuşun tahtada zaten İKİ anlamı var ve ikisi de **basılma karesinde** karar
veriyor (`Input.GetMouseButtonDown(0)` → `HandleClick`, ve yerleştirme jesti).
Üçüncü bir anlam, seçimi bırakma karesine ertelemeyi gerektirir. Varsayılan sağ
tuş; `panButton` bir alan ve Inspector'dan `0` yazılırsa sol tuş çalışır.
**Yeniden açma:** seçim bırakma karesine taşındığı gün varsayılan `0` olabilir.

### İŞ-11 (P1) — hayalet tahta dışında ve dolu hücrede KIRMIZI

Operatör: *"sürükle-bırak yaparken unit grid'in dışındakileri de hayalet
kısmını görebilmeliyiz ama kırmızılı hâlinde... bıraktığımızda tabii ki de
kabul etmesin."*

**DÜRÜST BULGU — bırakma zaten reddediyordu.** `DropAt` tahta dışını bir
vazgeçme sayıyor, `DropUnit` `IsCellFree` soruyor, `DropStructure`
`PlaceStructure`'ın sonucunu okuyor. Eksik olan kural değil, **oyuncunun onu
parmağını kaldırmadan önce görmesiydi.**

`PlacementPreview` üç değer taşıyor (`Placeable` / `OutsideBoard` /
`CellOccupied`) ve `IsCellFree` artık kendi kuralını yazmıyor, ona delege
ediyor — iki kopya kalsaydı "boş hücre" tanımı değiştiği gün bırakma kabul
eder, hayalet kırmızı gösterirdi. → madde 8.

### İŞ-1 (P0) — kalıcı saldırı emri, Command pattern ile

Operatör: *"bir attacker'a target belirttiğimizde 1 kere saldırıyor; tekrardan
yönlendirmediğimiz sürece saldırmaya devam edebilmeli. Hedef kaçarak menzilden
çıkarsa saldırı kesilmeli, ve birden fazla saldıran varsa her biri kendi
menzilinden koptuğunda kesmeli."*

**Seçilen pattern: Command (emir nesnesi), State DEĞİL.** Ayrım tek cümlede:

> **State**, TAHTANIN şu an ne yaptığıdır ve **tektir**.
> **Order/Command**, HER BİRİME ne söylendiğidir ve **çoğuldur**.

Kalıcı saldırı çoğul olduğu için kip makinesine sığmaz; `BoardModeMachine`
girdinin anlamını sahiplenmeye devam eder, emirler ondan bağımsız yaşar.

Önerilen şekil (bağlayıcı değil, ölçü):

```
IUnitOrder                       Tick(deltaSeconds) -> Devam / Bitti / Iptal
  AttackOrder(target)            menzildeyse vur, cooldown kapisi Core'da
  MoveOrder(x, y)                yurume bitince Bitti
                                 (bugunku "yaklas sonra vur" ikisinin BILESIMI)

Dictionary<Unit, IUnitOrder>     emir tablosu — birim BASINA bir emir
  yeni emir      -> eskisini degistirir (oyuncu yeniden yonlendirdi)
  hedef menzil disi -> Iptal, ve SADECE o saldiranin emri
  hedef tahtadan kalkti / oldu -> Iptal
  saldiran kalkti  -> Iptal
```

**Zorunlu testler** (bu turun ölçütü):
- İki farklı birime aynı anda emir verilebiliyor ve ikisi de tutuluyor.
- **İki AYRI TAKIMDAN** birer birim aynı anda emir tutabiliyor (operatörün
  bildirdiği belirti tam olarak budur).
- Hedef menzilden çıkınca yalnız etkilenen emir iptal oluyor; aynı hedefe
  saldıran menzildeki öteki birimin emri DEVAM ediyor.
- Yeni emir eskisinin yerine geçiyor, ikisi birden koşmuyor.
- Bekleme süresi kuralı emrin içinde İKİNCİ kez yazılmıyor — `AttackAction`
  zaten `RejectedOnCooldown` döndürüyor, emir onu sessizce yutup bekliyor.
- Emir tablosu `DespawnView` / `RemoveSelected` / kaldırma yollarında sızmıyor.

### İŞ-2 (P1) — seçim emirden sonra bırakılsın, ama geri alınabilsin

Operatör: *"attacker'ın kime saldıracağı belirtildiğinde seçim kaldırılmalı ama
tekrardan seçim alınabilecek şekilde de ayarlanabilir."*

Bugün seçim yalnız **isabet eden** saldırıdan sonra bırakılıyor
(`ReleaseSelectionAfterStrike`). Kalıcı emirle birlikte doğru kural şu olur:
**emir YAZILDIĞI an seçim bırakılır** — çünkü emir artık seçime bağlı değil,
birime ait. Birime tekrar tıklamak onu yeniden seçer ve mevcut emrini gösterir.
DİKKAT: bugünkü bekleyen-vuruş zinciri seçime bağlı (`PendingStrikeIsAlive`
`selectedUnit` ile karşılaştırıyor); emir tablosuna geçince o bağ KOPMALI,
yoksa seçimi bırakmak emri iptal eder.

### İŞ-3 (P1) — durum şeridi "sıra sen" demeyi bıraksın

`FreeForAll` kipinde tur numarası ilerlemiyor, yani şerit ölü bir sayı
gösteriyor. **Yeni mekanizma EKLENMEYECEK** — ölçüldü ve gerekçesi aşağıda
(`Pattern kararları` bölümü). `BattleStatusView` zaten `SelectionChanged`'e
abone; gösterilecek cümle "sıra sen" değil, seçili şeyin TARAFI (senin takımın /
düşman takım, mavi / kırmızı) ve savaşın durumu olmalı.

### İŞ-4 (P2) — arayüz bileşenleri ekran köşelerine yapışık

Operatör: *"sağ alttaki sil düğmesi direkt köşeye yapışık, düzgün değil."*
`SceneSetupTool` panelleri kuruyor; kenar boşluğu (margin), güvenli alan ve
dokunma hedefi ölçüleri orada sabit olarak yaşıyor. Köşeye yapışmayı tek tek
düzeltmek yerine **tek bir kenar boşluğu sahibi** tanımla ve bütün paneller onu
okusun — aynı "tek sahip" kuralı, bu sefer arayüzde.

## Pattern kararları — YENİDEN AÇMA

Bir önceki turda on iki pattern ölçüldü. Kapalı olanlar:

| pattern | durum | gerekçe |
|---|---|---|
| Object Pooling | **VAR** | `UnitViewPool` |
| Observer | **VAR** | 8 `public event` (ölçüldü; belge önce 11 diyordu) |
| Flyweight | **VAR** | `UnitBlueprint` / `AttackProfile` paylaşılan değişmez tanımlar |
| Factory | **YOK** — önceki satır ölçüyü yanlış uyguluyordu | `CreateCombatant`, `CreateStructure` ve `ProjectileView.Fire` üçü de nesne üretiyor ama **hiçbiri tip seçmiyor**. Fabrikanın ölçüsü nesne üretmek değil, çağıranın dönüş TİPİNİ bilmemesidir. `StructureProduction.Produce` somut tipi `out Combatant produced` parametresiyle imzasında taşıyor; `Combatant` ve `Structure` ikisi de `sealed`; üretilen somut tip sayısı bir. Ölçü ve tetikleyici: `Docs/ogrenme/13-desen-secim-rehberi.md` |
| State | **VAR** | `Assets/Game/Unity/Modes/` — tahtanın kipi |
| **Command** | **BU TURDA YAPILACAK** | kalıcı emir; tetikleyici geldi — ama *undo* için değil, **çoğul emir** için |
| Singleton | **REDDEDİLDİ** | referans proje `Map.Instance` kullanıyor; 105 Unity testi tam da global durum olmadığı için koşuyor |
| Event Bus | **REDDEDİLDİ** | bugünkü olaylar tipli ve yönlü; bus onları isimsiz yapar |
| Service Locator | **REDDEDİLDİ** | assembly duvarını deler |
| MVC/MVP | **GEREKSİZ** | yerine daha sert bir ayrım var: `noEngineReferences: true` derleyiciyle zorlanıyor |
| Strategy | **ERTELENDİ** | hedef seçimi tek algoritma; ikinci bir kural doğmadan soyutlamak erken |
| Decorator | **GEREKSİZ** | katmanlı etki (zırh/buff) yok |

**Kural:** bir pattern, ancak mevcut mekanizmanın **ölçülmüş** bir eksiği varsa
eklenir. "Sıra sen" sorununda eksik olan mekanizma değil, gösterilen cümledir.

### Aboneliğin ölçüsü: Observer nerede biter, Mediator nerede başlar

Operatör şunu sordu: *"abonelik sayısı gereğinden fazla arttığında farklı bir
pattern kullanılıyordu, onun da ismini söylersen sevinirim."* Aranan ad
**Mediator**; olay biçimindeki hâlinin adı ise **Event Aggregator**, oyun
dünyasında yaygın söylenişiyle **Event Bus** (Zenject/VContainer'da *Signal
Bus*). Üçü ayrı ad değil, aynı fikrin üç ölçeği.

**Ölçü abonelik SAYISI değil, BAĞLANTI sayısıdır** — ve ikisini karıştırmak bu
sorunun asıl tuzağı:

```
OBSERVER                        MEDIATOR / EVENT BUS
K yayinci, N dinleyici          herkes TEK araciyi taniyor
birbirini DOGRUDAN taniyor
                                yayinci ─┐
yayinci ──► dinleyici                    ├─► ARACI ──► dinleyici
        ──► dinleyici           yayinci ─┘         ──► dinleyici
yayinci ──► dinleyici
        ──► dinleyici           bag sayisi: K + N
bag sayisi: K x N
```

Yani eşik "kaç abonelik var" değil, **kaç YAYINCI var**. Tek yayıncılı bir
sistemde K = 1'dir ve K × N ile K + N aynı sayıdır — araya bir aracı koymak
hiçbir bağlantıyı ortadan kaldırmaz, yalnızca bir dolaylılık ekler.

**Bu projede ölçüldü:** sekiz `public event` var ve **hiçbiri çapraz değil**.
`SelectionChanged` / `UnitRemoved` / `TurnChanged` tek bir yayıncıdan (tahta)
çıkıyor; `PaletteEntryView`'ın dört olayı bir düğmenin kendi hayatı;
`SelectedProductionChanged` üretim müdürünün. Yani K = 1 olan üç ayrı küme, bir
tane K × N kümesi değil. Mediator'ün çözdüğü problem burada henüz yok.

**Bedeli de ölçülebilir ve bu yüzden reddedildi:** bir bus, *"bu olayı kim
dinliyor"* sorusunu **derleme zamanından çalışma zamanına** taşır. Bugün
`board.SelectionChanged -= OnSelectionChanged;` satırını silersen derleyici
susar ama IDE sana çağıranı gösterir; bir bus'ta dinleyici bir mesaj TİPİNE
abonedir ve o tipi kimin dinlediğini ancak çalıştırarak öğrenirsin. Aynı sebeple
`Service Locator` da reddedildi — ikisi de bağı görünmez yapıyor.

**YENİDEN AÇMA TETİKLEYİCİSİ — sayıyla:** aynı olguyu **ikiden fazla YAYINCI**
duyurmaya başladığı ve dinleyici kümesi ikisinde de aynı olduğu gün. Somut
örneği bugünden görünüyor: "bir birim tahtadan kalktı" olgusunu bugün yalnız
tahta duyuruyor; bir gün savaş çekirdeği ve bir kayıt/replay katmanı da
duyurursa üç yayıncı aynı dinleyicilere bağlanmak zorunda kalır — Mediator'ün
kazandığı gün tam olarak odur, ondan önce değil.

**TEK CÜMLE:** Observer'ın maliyeti dinleyici sayısıyla değil YAYINCI sayısıyla
büyür; Mediator bir kalabalık çözümü değil, bir ÇAPRAZLIK çözümüdür.

## Burst / Job System — ÖLÇÜLDÜ VE REDDEDİLDİ

Operatör bunları sordu; cevap dürüst biçimde **hayır**, ve sebebi sayılarla:

- Tahta **10x5 = 50 hücre**. Birim sayısı onlu mertebede.
- `unity-expert-code-quality` kural 15-16 ECS/Burst/Jobs'u on bir soruluk bir
  ön uçuşun ve ölçülmüş bir yük ekseni sayımının arkasına kilitliyor: *"A
  project reaching for jobs has already entered this contract."*
- Jobs/Burst binlerce varlık için vardır. Elli hücrede bir iş kuyruğu kurmanın
  bedeli, kazandırdığından büyüktür.

**Gerçek performans borcu başka yerde ve o ölçüldü:**
- `Battle.TryGetPosition` her çağrıda tahtanın TAMAMINI tarıyor ve sık
  çağrılıyor. Kalıcı emirler her karede konum soracağı için bu, İŞ-1 ile
  birlikte **gerçekten** ısınacak yer burasıdır.
- İmleç çerçevesinin rengi için her karede tam A* çalışıyordu; önbelleğe alındı
  ama önbellek yolu kapayan ÜÇÜNCÜ bir birim kımıldarsa eskiyebilir.

**Yeniden açma tetikleyicisi:** eşzamanlı emir sayısı × birim sayısı, ölçülmüş
bir kare bütçesini aştığı gün — ve o gün önce `TryGetPosition`'ın sözlüğe
alınması denenir, Burst değil.

## Operatör adımları — bu tur ZORUNLU

Bu turun üç işi ekrana ancak bu adımlardan sonra iner. Adımlar tek satır;
gerekçeleri yukarıdaki bölümlerde.

1. Unity'de menüden `CountryBall > Sahneyi Kur (her şey)` komutunu çalıştır.
2. `Ctrl+S` ile sahneyi kaydet.

Doğrulama (ikisi için tek satır): Hierarchy'de `Main Camera` seçildiğinde
Inspector'da **Board Camera Rig** bileşeni görünmeli ve `Home Half Height`
alanı sıfırdan büyük olmalı.

**KAPI BU BORCU ZATEN GÖRÜYOR VE KIRMIZI:** `Tools/check-asset-inventory.py`
şu an `1 ihlal` veriyor ve cümlesi tam olarak bu —
*"BoardCameraRig: 1 referans alanı var ama HİÇBİR sahne/prefab bu script'i
taşımıyor."* Kapı susturulmadı; adım atıldığında kendiliğinden yeşile döner.
Bu, makinenin kapatamadığı bir kova (`unity-expert-code-quality` kural 18):
kamera gerçekten kayıyor mu, sınıra dayanınca duruyor mu, tekerlek limitte
duruyor mu — üçünün de kanıtı bir İNSAN GÖZLEMİ, bir test değil.

## Açık borçlar (bir önceki turdan devredenler)

| borç | ölçü |
|---|---|
| `check-doc-code-refs` | HEAD'de 258 → şimdi ~350; `Docs/` içindeki satır çapaları kaydı |
| `PlacementGhost.prefab` | guid'ine sıfır atıf; ikinci ölü varlık |
| `BoardCameraRig` sahnede yok | `check-asset-inventory` 1 ihlal; iki operatör adımı kapatır. **Rig olmadan sol sürükleme çalışmaz** ama sessiz kalmaz: `[Board] No BoardCameraRig found` |
| `SampleScene.unity` `structureScale: 1.6` | ölü YAML; araç sahneyi kaydettiğinde düşer |
| `IPlacementModeHost` 7 üye, `IPendingStrikeHost` 9 üye | god object'in kalan bağımlılığının fotoğrafı; bölündükçe daralmalı |
| operatör adımı | `CountryBall → Sahneyi Kur (her şey)` + Ctrl+S — **bu tur ZORUNLU**: `BoardCameraRig` kameraya ancak araç koşunca takılır |

## Ölçülmüş kısıtlar (yeniden keşfetme)

- Unity **6000.5.7f1**, C# 9. `record struct` YOK.
- Unity serileştiricisi alanları yansımayla yazar, **kurucuyu çağırmaz** —
  Inspector'dan doldurulan `readonly struct` değişmezliği hakkında yalan söyler.
- Editor açıkken `run-editmode-tests.ps1` `exit 2`. Çözüm: `Assets/`+`Packages/`
  +`ProjectSettings/`+`Tools/` bir scratch dizine kopyalanır **VE O KOPYA
  YENİDEN KULLANILIR** — her koşumda yeni kopya, testin 0,6 saniyesi için 2-4
  dakika içe aktarma ödetir; ısınmış `Library/` ile ~30 saniye.
- `total` tek başına yalan söyler; XML'den **assembly başına** okunmalı.
- Kopya-koşumun kör noktası: kaynak doğru ama operatörün `Library/` durumu
  bozuksa yeşil verir. Bu turda gerçekten oldu — `BoardSizing.cs` AssetDatabase'e
  sıkışmış bir kayıtla girdi, `csproj` onu derlemeye almadı, ve düzelten şey
  dosyayı `Assets/` ağacından çıkarıp geri koymak oldu.
- Testlerde `using System;` **YASAK** (`Object` adı `UnityEngine.Object` ile
  belirsizleşir, CS0104); `System.ArgumentException` tam nitelikli yazılır.

## Son ölçüm

`651 → 738 test, 738/738 yeşil, 0 başarısız`

| assembly | HEAD (0bc2590) | şimdi | fark |
|---|---|---|---|
| `GridStrategy.Battle.EditModeTests` | 179 | 179 | — |
| `GridStrategy.Combat.EditModeTests` | 247 | **252** | **+5** |
| `GridStrategy.Core.EditModeTests` | 100 | **108** | **+8** |
| `GridStrategy.Unity.EditModeTests` | 125 | **199** | **+74** |
| **toplam** | **651** | **738** | **+87** |

Döküm: `UnitOrderTests` +21 (yeni dosya), `BoardViewportTests` +13 (yeni
dosya, kaydırma ve yakınlaştırma kuralı), `BoardTilemapTests` +6 (yeni dosya,
zemin katmanının İLK testleri), `UnitGridTests` +8 (ters dizinin senkronu),
`BoardPointerArbiterTests` +19 (yeni dosya, tıklama/kaydırma hakemliği),
`BoardModeTests` −11
(`PendingStrikeMode`'un kendisiyle birlikte giden testler), `BoardAdapterTests`
+21 (emir/seçim dalları, çökme, geri sayım dikişi, odak devri, yerleştirme
önizlemesi),
`StructureProductionTests` +5 (yeni dosya, üretim TÜRÜ ile SAYACIN ayrımı).
Dokuz assembly'nin
dokuzu da derleniyor; operatörün Editor'ünde MCP ile teyit edildi (Console'da
sıfır hata, sıfır uyarı).

> ██ ÖNCEKİ HÂLİ BİR YALANDI VE NASIL ÖLÇÜLDÜĞÜ ÖNEMLİ ██
> Burada `Battle 164 · Combat 241 · Core 100 · Unity 146` yazıyordu ve yanına
> "assembly başına doğrulandı" notu düşülmüştü. Toplam (651) doğruydu, kırılım
> DEĞİLDİ: dördünün toplamı 651 ediyor ama hiçbiri o turun gerçek sayısı değil.
> Ölçüm HEAD'in kendisi ikinci bir ısınmış kopyaya kurulup koşularak yapıldı —
> `git archive HEAD` ile çıkarılan ağaç, ayrı bir `Library/` ile. Kırılımı
> çalışan ağaçtan çıkarıp "önceki" diye yazmak tam olarak bu yalanı üretir.
>
> **TEK CÜMLE:** Bir toplamın doğru olması, onu oluşturan sayıların
> ölçüldüğünü göstermez.

## Bu ölçümden SONRA bulunan P0 çökme

Yukarıdaki 665'lik yeşil geçerliydi ve yine de bir yolu hiç çalıştırmamıştı:
**uzaktaki düşmüş dosta yürüyerek gitmek her seferinde
`NullReferenceException` veriyordu.** `IssueOrder` seçimi bırakıyor
(`selectedUnit = null`) ve hemen ardından gelen Console satırı o alanı
okuyordu.

Yeşilin bunu görmemesinin sebebi ölçüldü ve **açık bir borçtur**: gerçek tahta
`FreeForAll` ile kuruluyor, `BoardAdapterTests` fixture'ı ise varsayılan
`Alternating` ile. O kipte yürüyüş sırayı devrediyor ve üye emir satırına hiç
varmıyor — yani **fixture, oyuncunun oynadığı kipi hiç ölçmüyor.**

Onarımın eski kodu ve gerekçesi:
`Docs/deep/konular/09-kararlarin-cevrilmesi.md` (madde 2a-i).
Ölçüsü, tahtayı bilerek `FreeForAll` ile kuran tek test:
`TryCloseInOnAlly_WhenTheWalkStarts_WritesTheOrderWithoutReadingTheReleasedSelection`.

> **TEK CÜMLE:** Bir fixture'ın varsayılanı ürünün varsayılanından ayrıysa,
> yeşil ekran oyuncunun oynadığı oyunu değil başka bir oyunu ölçüyordur.
