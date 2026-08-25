# BoardAdapter

> **Kaynak:** `Assets/Game/Unity/BoardAdapter.cs`
> **Ad alanı:** `GridStrategy.Unity` · **Assembly:** `GridStrategy.Unity` (MonoBehaviour, motora bağımlı)
> **Rol:** Karma — Çevirmen (Adapter) + Varlık (Entity); **hafızası var** — aynı dolu hücreye iki kez tıkla, biri **seçer** ötekisi **bırakır**

Unity dünyası ile motordan bağımsız savaş kuralları arasındaki çevirmen — ama
**yalnız çevirmen değil**. Geçerlilik sorularının hiçbirini kendisi cevaplamaz:
sınır, doluluk, menzil ve dost ateşi `Battle` ile `BattleActions`'a sorulur.
Buna karşılık **niyet** kararı burada duruyor, ve ölçüsü şu: dolu hücreye
tıklamak SALDIRI, boş hücreye tıklamak HAREKET, seçili birimin kendi üstüne
tıklaması SEÇİMİ BIRAKIR — üçü de bu dosyada yazılı ve hiçbiri `Battle`'a
sorulmuyor. Aşağıdaki [ROL](#rol) bölümündeki KOKU satırının konusu tam olarak
budur.

Birim başına görsel durum artık burada değil, `UnitView` içinde yaşıyor — o baskı
gerçekten doğdu ve bölündü. Buna karşılık input okuma ve zemin kurulumu hâlâ
burada: ikisi de bağımsız değişme baskısı üretmedi, baskısız bir katman yalnızca
dolaylılık ekler.

**Tahta artık burada değil.** Bu tipte bir `UnitGrid` alanı vardı; o alan silindi
çünkü `Battle` tahtayı kendisi sahipleniyor. İki sahibin bedeli `Battle`'ın
kurucusundaki REDDEDILEN bloğunda yazılı ve o öngörü artık kapanmıştır.

| Üye / konu | Karar | Detay |
|---|---|---|
| `using Battle = global::...` | çıplak `Battle` bu dosyada CS0118'dir; alias'ın YERİ kuralın kendisidir | [↓](#cs0118-alias) |
| girdi okuma | DÖRT fare sorgusu, İKİ akış: üçlü yalnız yerleştirme kipinde şart | [↓](#girdi-okuma-notu) |
| rol künyesi | KOKU büyüdü, EŞİK AŞILDI ve yarısıyla karşılandı | [↓](#rol) |
| `unitPrefab` | alan tipi `UnitView`: eksik bileşen Play'e basmadan yakalanır | [↓](#unitprefab) |
| `maxHealth` · `damage` · `attackRange` | denge sayıları sahnede yazılı, asset'te değil | [↓](#maxhealth-damage-attackrange) |
| `moveRange` | sayının sahibi burası, kuralın sahibi `MoveProfile` — adı konmuş BORÇ | [↓](#moverange) |
| `placementGhost` | hayalet gerçek bir `Structure` DEĞİLDİR; tahtaya hiç yazmaz | [↓](#placementghost) |
| `dragThreshold` | eşik DÜNYA biriminde, pikselde değil | [↓](#dragthreshold) |
| `unityGrid` | Unity'nin `Grid` bileşeni yalnızca bir koordinat çevirmeni | [↓](#unitygrid) |
| `battle` | tahtanın ve savaşın durumu burada değil, `Battle`'ın içinde | [↓](#battle) |
| `unitViews` | anahtar `Unit`, çünkü KONUM yalnız tahtada yaşasın | [↓](#unitviews) |
| `cleanupBuffer` | durum değil, tahsisten kaçınmak için yeniden kullanılan KAP | [↓](#cleanupbuffer) |
| `gesture` | `cleanupBuffer`'ın tersi: gerçek bir hafıza | [↓](#gesture) |
| `ghostIsCarried` | sayaç değil bool: ayrım "kaçıncı tıklama" değil | [↓](#ghostiscarried) |
| `Awake()` | kurulum sırası; jest ancak burada kurulabilir | [↓](#awake) |
| `OnEnable()` · `OnDisable()` | abonelik ETKİNLİĞE aittir, doğuma değil | [↓](#onenable-ve-ondisable) |
| `OnUnitStateChanged(...)` | "nereden" bugün kullanılmıyor ve bu bir eksiklik değil | [↓](#onunitstatechangedunit-unit-unitstate-from-unitstate-to) |
| `Update()` | iki sıra kararı: zaman her kare ilerler, kip ayrımı en üstte | [↓](#update) |
| `TryEnterPlacementMode()` | seçili birim şartı TEKNİK, oyun kuralı değil | [↓](#tryenterplacementmode) |
| `UpdatePlacement()` | iptal her zaman önce; hayalet her kare koşulsuz taşınır | [↓](#updateplacement) |
| `FeedGesture(...)` | Down/MoveTo zincir, Up AYRI if | [↓](#feedgesturefloat-worldx-float-worldy) |
| `CommitPlacement(int, int)` | geçerliliğe bu dosya karar vermiyor; kipten çıkış sonuçtan bağımsız | [↓](#commitplacementint-x-int-y) |
| `CancelPlacement()` | tahtaya DOKUNMAZ | [↓](#cancelplacement) |
| `NewStructure(Unit)` | taraf, yapıyı koyan birimden okunur | [↓](#newstructureunit-placer) |
| `CreateStructureVisual(int, int)` | prefab değil kodla kurulan GameObject; tabloya kaydedilmiyor | [↓](#createstructurevisualint-x-int-y) |
| `TryReadPointerCell(...)` | bir Unity tipinin Core'un diline çevrildiği TEK yer | [↓](#tryreadpointercell) |
| `AdvanceBattleTime()` | zamanı buradan vermek zorunlu; temizlik yoklama değil TOPLU | [↓](#advancebattletime) |
| `BuildCellVisuals()` | sprite yoksa gürültüyle dur | [↓](#buildcellvisuals) |
| `CreateCellVisual(int, int)` | ebeveynlik konum için değil TOPLU YAŞAM DÖNGÜSÜ için | [↓](#createcellvisualint-x-int-y) |
| `PickTerrainSprite(int, int)` | deterministik; Random olsaydı hata tekrar üretilemezdi | [↓](#pickterrainspriteint-x-int-y) |
| `CellCentre(int, int)` | hücre → dünya çevirisinin TEK yeri | [↓](#cellcentreint-x-int-y) |
| `SpawnUnit(...)` | önce KURAL sonra görsel; tahtaya giden tek kapı `Battle` | [↓](#spawnunitstring-name-team-team-int-x-int-y) |
| `NewCombatant(Team)` | yaşam döngüsü pencereleri bilerek serileştirilMEDİ | [↓](#newcombatantteam-team) |
| `HandleClick()` | çeviri tek sahibe indi; dallanma satır satır aynı kaldı | [↓](#handleclick) |
| `HandleOccupiedCellClick(...)` | kendi üstüne tıklamak seçimi bırakır — NİYET ile GEÇERLİLİK ayrı | [↓](#handleoccupiedcellclickunit-clicked-int-x-int-y) |
| `HandleEmptyCellClick(int, int)` | seçim varsa hareket, yoksa yalnızca bildir | [↓](#handleemptycellclickint-x-int-y) |
| `ReactToAttack(...)` | görsel bu daldan tazelenmiyor; TEK yol olay zinciri | [↓](#reacttoattackattackoutcome-outcome-unit-target-int-x-int-y) |
| `ReactToMove(...)` | iki dal bugün ULAŞILAMAZ ve yine de yazılı | [↓](#reacttomovemoveoutcome-outcome-unit-unit-int-x-int-y) |
| `ApplyStateVisual(...)` | görsel SONUÇ enum'undan değil DURUMDAN okunur | [↓](#applystatevisualunit-unit-unitstate-state) |
| `DespawnView(Unit)` | seçim önce bırakılır ama `ClearSelection` ile DEĞİL | [↓](#despawnviewunit-unit) |
| `SelectUnit(Unit)` | önce eskiyi temizle: iki birim aynı anda seçili görünemez | [↓](#selectunitunit-unit) |
| `SetSelectionVisual(...)` | adaptör çerçeveyi görmüyor bile; yalnızca niyeti söylüyor | [↓](#setselectionvisualunit-unit-bool-isselected) |
| `TryGetView(...)` | tabloda olmamak bir OYUN olgusu değil, PROGRAMCI hatası | [↓](#trygetviewunit-unit-out-unitview-view) |

**İlgili anlatılar:** [03-tahta sahipliği](../../konular/03-tahta-sahipligi.md) ·
[06-sonuç enum'ları](../../konular/06-sonuc-enumlari.md) ·
[07-tıklamadan eyleme](../../konular/07-tiklamadan-eyleme.md) ·
[01-olay zinciri](../../konular/01-olay-zinciri.md)

---

## CS0118 alias

Çıplak `Battle` yazmak bu dosyada bir **derleme hatasıdır** (CS0118). Harita
(ad ağacı) ve arama sırası tablosu **kodda kaldı**, çünkü onlar olmadan
`using Battle = global::GridStrategy.Battle.Battle;` satırı okunamaz hâle gelir.
Buraya taşınan şey kapsam, iş bölümü ve alternatiflerdir.

### KAPSAM: bu SADECE `Battle` adına özeldir

Kural: bir tip adı yalnızca **aynı zamanda** kapsayan zincirde görünen bir **ad
alanının** adıysa tuzağa düşer. Kesişimi al:

```
GridStrategy'nin ad alanları : Battle   Combat  Core  Unity
projedeki 34 tip adı         : Battle   BattleActions  Unit  ...
kesişim                      : { Battle }        ← tek eleman
```

**KARŞI ÖRNEK** aynı dosyada, `CommitPlacement` metodunun içinde: `BattleActions`
ve `PlacementOutcome` tam olarak **aynı** ad alanında, **aynı** klasörde, **aynı**
assembly'de yaşar — ve alias olmadan çalışırlar, çünkü o adlarda bir ad alanı
yok. Onlara alias yazmak gereksiz gürültü olurdu.

Yeni tip eklerken sorulacak tek soru: **adı `Battle`, `Combat`, `Core` veya
`Unity` mi?** Değilse `using` yeter.

### İŞ BÖLÜMÜ: using ile alias ÖRTÜŞMEZ, BÖLÜŞÜR

Bu dosya `GridStrategy.Battle`'dan üç tip kullanıyor ve ikisi tamamen farklı
yoldan geliyor:

```
Battle             çakışıyor   ► ALIAS halleder  (using etkisiz)
BattleActions      çakışmıyor  ► using halleder  (alias gereksiz)
PlacementOutcome   çakışmıyor  ► using halleder  (alias gereksiz)
```

Bu yüzden ikisi de gerekli ve hibrit bir kaza değil: üstteki `using` silinirse
`BattleActions` ile `PlacementOutcome` kırılır, alias silinirse `Battle` kırılır.

### `global::` HATAYI ÇÖZMEZ, GELECEĞİ KİLİTLER

Alias'ın **sağ** tarafındaki `GridStrategy` adı da çözülmek zorunda ve o çözüm de
kodda yazılı aynı sıraya tabi. `global::` aramayı seviye 1-2'yi atlayıp **kökten**
başlatır: ileride buraya `GridStrategy` adlı bir tip ya da ad alanı eklense bile
alias'ın hedefi sessizce kaymaz. Yani aynı tuzağın alias'ın **kendi hedefinde**
kurulmasını engelleyen bir sigortadır — bugünkü hatanın çözümü değil.

Aynı desen ve gerekçe `BattleTests` ile `BattleActionsTests`'teki kardeş
alias'ların üstünde de yazılı.

### ALTERNATİF VE ASIL KÖK

**Alternatif:** alias yok, her kullanım `GridStrategy.Battle.Battle` diye tam
nitelenir. Derleme geçer ama tuzağı anlatan tek satır kaybolur — tip bu dosyada
**bir** kez geçseydi tercih tersine dönerdi.

**Asıl kök adlandırmadır:** sınıf `BattleState` ya da ad alanı
`GridStrategy.Battles` olsaydı tuzak hiç doğmazdı. Ad korundu, bedeli o blok
oldu.

---

## Girdi okuma notu

**ÖLÇÜ:** dosyada `Input.GetMouseButton` ara. **Dört** çağrı çıkar, üç değil — ve
ikiye ayrılırlar:

```
Update()                                       ◄── SIRADAN TIKLAMA AKIŞI
  GetMouseButtonDown(0)  -> HandleClick()          PointerGesture'a UĞRAMAZ

FeedGesture()   (yalnız yerleştirme kipi açıkken koşar)
  GetMouseButtonDown(0)  yalnız BASILDIĞI karede true    -> gesture.Press
  GetMouseButton(0)      basılı olduğu HER karede true    -> gesture.MoveTo
  GetMouseButtonUp(0)    yalnız BIRAKILDIĞI karede true   -> gesture.Release
```

**Tek sorgu ne zaman yeter:** sıradan tahta tıklamasında. Orada sürükleme diye
bir kavram **yok**; basış anı zaten tıklamadır ve `Update` içindeki yalnız
`Down` doğruca `HandleClick`'e gider.

**Üçlü ne zaman şart:** yerleştirme kipinde, çünkü orada bir tıklama ile bir
sürükleme **başlangıçlarında** birbirinin aynısıdır. İkisini ayıran tek şey
"basılı tutulurken imleç gerçekten hareket etti mi" sorusudur ve o soru ancak
basılı geçen karelerde (`GetMouseButton`) sorulabilir. Yalnız `Down` okunsaydı
sürükleme diye bir kavram o kipte **yazılamazdı**: bırakma anı hiç görülmediği
için "nerede bıraktı" da bilinemezdi.

**Kararın kendisi burada değil — ama yalnız o kipte:** hangi karenin tıklama,
hangisinin sürükleme olduğuna `GridStrategy.Core.PointerGesture` karar verir. Bu
dosyanın işi motorun üç sorusunu o tipin üç metoduna **çevirmekten** ibarettir —
çevirmen yarısının ders kitabı örneği. Sıradan tıklama akışında ise çevrilecek
bir jest bile yoktur.

**ÇERÇEVE SINIRI, ve bilerek yazıyorum:** bir kare içinde `Down` ve `Up` **aynı
anda** true olabilir (kare süresinden kısa bir tıklama). Bu yüzden
[`FeedGesture`](#feedgesturefloat-worldx-float-worldy)'da `Down`/`MoveTo` bir
if-else zinciri, `Up` ise **ayrı** bir `if`'tir; hepsi tek zincire konsaydı o
hızlı tıklamanın bırakılışı sessizce yutulur ve hayalet fareye yapışıp kalırdı.

---

## ROL

```
kimlik : var — ölçüsü şu: aynı sahneye İKİ BoardAdapter koy, İKİ AYRI
         savaş doğar; Awake her birinde kendi Battle'ını kurar ve
         birinde seçtiğin birim ötekinde seçili GÖRÜNMEZ. Tipte tek
         bir static alan yok — battle, unitViews ve selectedUnit'in
         üçü de örneğe aittir, yani durum burada ikamet ediyor
hafıza : var — ölçüsü şu: AYNI dolu hücreye arka arkaya İKİ kez tıkla,
         iki FARKLI şey olur. Birinci tıklama birimi SEÇER (o an
         seçim yoktu), ikincisi seçimi BIRAKIR (ReferenceEquals
         tutar). Farkı doğuran şey, tipin hangi birimin seçili
         olduğunu kareler arasında hatırlaması — yani bu bir OYUN
         durumu, çeviri durumu değil: saf bir çevirmenin
         taşımayacağı şey tam olarak budur
Unity  : zorunlu — ölçüsü şu: Assets/Tests/EditMode/Unity/ klasörüne
         bak; UnitViewTests VAR, BoardAdapterTests YOK. Sebebi
         sayılabilir: bu tip new ile kurulamaz, ancak bir sahne
         nesnesinin üstünde yaşar; Camera.main EditMode'da boştur ve
         Input hiçbir karede true dönmez. Motor yüzeyi: Input,
         Camera, Time, Instantiate, MonoBehaviour
karar  : ikisi birden — piksel→hücre çevirisi (çevirmen işi) ile
         "aynı anda tek birim seçili" ve "dolu hücreye tıklamak SALDIRI,
         boş hücreye tıklamak HAREKET demektir" kuralları (varlık işi)
         aynı tipte
```

### KOKU: evet ve BÜYÜDÜ

Önceki başlık *"tek satırlık kural için bugün ayrı katman yalnızca dolaylılık
olurdu"* diyordu; o cümle artık doğru değil, çünkü üç yeni oyun kararı buraya
girdi:

- tıklamanın **niyete** çevrilmesi (boş→hareket, dolu→saldırı, kendine→seçimi
  bırak),
- savaşın zamanını ilerletme ve ceset süpürme takvimi,
- birim sayılarının (can, hasar, menzil, taraf) yazılı olduğu yer.

Üçü de Unity'siz test **edilemez** hâlde ve üçü de bu tipin "çevirmen" yarısına
ait değil.

### EŞİK AŞILDI — ve notu SİLMİYORUM

Bir eşiğin aşıldığını söyleyen satır, eşiği koyan satır kadar öğreticidir. Yazılı
eşik şuydu: *"dördüncü kural geldiği gün Core tarafına bir 'komut' sahibi
çıkmalı: tıklamayı niyete çeviren saf bir tip."* Madde #10 bütün bir **giriş
kipi** ekledi (yerleştirme) ve eşiği aştı.

### NASIL KARŞILANDI — ve yarısıyla

Karar dışarı çıktı, ama çıkan yarı "niyet" değil **"jest"** oldu.
`GridStrategy.Core.PointerGesture` "bu bir tıklama mıydı yoksa sürükleme mi"
sorusunun tek sahibi; Unity'siz, `Time`'sız, `Vector2`'süz ve EditMode'da
sınanabilir. Bu MonoBehaviour ona dört float veriyor ve dönen fazı uyguluyor —
eşiğin istediği şeklin ta kendisi, yalnız dar bir soru için.

**KALAN YARI, ve dürüst adı:** tıklamanın **niyete** çevrilmesi (boş→hareket,
dolu→saldırı, kendine→seçimi bırak) hâlâ burada,
[`HandleClick`](#handleclick)'in içinde ve Unity'siz sınanamaz durumda.

**SIRADAKİ EŞİK:** bu üç dala **dördüncüsü** eklendiği gün — sıra kimde
(`TurnRules` yazılı ve burada hâlâ **sorulmuyor**), çoklu seçim, ya da hedef
önizlemesi. O gün `PointerGesture`'ın ikizi doğmalı: `(x, y)` + tahtanın durumu
alıp bir **niyet** değeri döndüren saf bir tip, ve bu dosyada geriye yalnız o
niyetin uygulanması kalmalı.

---

## unitPrefab

Alan tipi `GameObject` değil `UnitView`: Inspector artık `UnitView` **taşımayan**
bir prefab'ı kabul etmez. Yani "prefab'a bileşen eklemeyi unuttum" hatası Play'e
basmadan, sürükle-bırak anında yakalanır. `GameObject` tutsaydık aynı hata ancak
ilk tıklamada `NullReferenceException` olarak çıkardı.

---

## maxHealth, damage, attackRange

Birim sayıları buradan geliyor: düz `[SerializeField]` olarak.

`AttackProfile`'ın asset bloğundaki KAZANIRDI satırı "hasar ve menzil sayılarını
programcı değil tasarımcı ayarlayacaksa" diyor. O gün **gelmedi** — ama yarısı
geldi: sayıların artık gerçek bir okuyucusu var ve yeniden derlemeden
denenebilmeleri gerekiyor. Inspector alanı bunu verir; asset dosyası bundan
fazlasını (paylaşım, birim listesi, sürümleme) verir ve o fazlanın bugün alıcısı
yok.

### HARİTA: bir sayı üç dosyadan birinde yaşayabilir

Seçenekleri ayıran şey "kim okur" değil, **dosyayı kim üretir**:

```
.cs  ────────────► dosyayı KOD üretir, kod okur
  const int MaxHealth = 30;
  değiştirmek = bir derleme turu     ◄── denemeyi PAHALI yapar

.unity (sahne) ──► dosyayı EDİTÖR üretir, kod okur   >> BUGÜN <<
  BoardAdapter bileşeninin serileştirilmiş alanı
  tek sahne, tek kopya; Play'e basmadan değişir

.asset ──────────► dosyayı EDİTÖR üretir, kod okur
  ScriptableObject; paylaşılır, sürümlenir, birim listesi taşır
  >> KOD BU DOSYAYI ÜRETEMEZ <<  ◄── REDDEDILEN'in kırılma noktası
```

Alt iki satır aynı yeteneği taşır (Inspector'da düzenlenebilir) ama kurulum
bedelleri farklı: sahne alanı **zaten var olan** bir bileşene yazılır, asset ise
**elle** oluşturulup **elle** atanması gereken **yeni** bir dosyadır. Aşağıdaki
kırılma tam olarak o farktan doğuyor.

### KAPSAM: "sayıyı serileştir" GENEL bir kural DEĞİL

Ayıran soru tek: **bu sayının başka bir yerde zaten bir sahibi var mı?**

```
sahibi YOK + denge değeri  ► serileştir (maxHealth, damage,
                             attackRange, moveRange)
sahibi VAR ya da DEĞİŞMEZ  ► serileştirme
```

**KARŞI ÖRNEK** aynı dosyada, [`NewCombatant`](#newcombatantteam-team)'ın içinde:
"düşük kalma" ve "ceset" süreleri bilerek serileştiril**medi**, çünkü ikisinin
sahibi zaten `UnitLifecycle`'daki iki sabit. Oraya bir Inspector alanı koymak
aynı sayıya ikinci bir kaynak açardı ve sahnedeki değer sabiti sessizce ezerdi.
Aynı dosya, aynı kurucu, **zıt** karar.

### İŞ BÖLÜMÜ: üç attribute üç ayrı iş yapıyor

```
[SerializeField] ► private alanı SAHNEYE yazdırır
[Min]            ► Inspector'da alt sınırı ZORLAR
[Tooltip]        ► yalnızca anlatır, hiçbir şeyi zorlamaz
```

`SerializeField` silinirse alan sahneye hiç yazılmaz ve değer her Play'de C#
başlatıcısına döner — Inspector'daki sayı sessizce boşa çıkar. `Min` silinirse
`maxHealth`'e sıfır girilebilir ve birim doğar doğmaz ölü olur. `Tooltip`
silinirse hiçbir davranış değişmez. Üçünü "Inspector süslemesi" diye tek torbaya
koymak, hangisinin silinmesinin tehlikeli olduğunu görünmez yapar.

### REDDEDILEN

Sayılar bir varlık tanımı asset'ine taşınır ve bu alanlar tek bir referansa iner:

```csharp
[SerializeField] private UnitDefinition playerDefinition;

[CreateAssetMenu(menuName = "GridStrategy/Unit Definition")]
public sealed class UnitDefinition : ScriptableObject { ... }
```

**KIRILAN:** SAHNE BOZULUR — `.asset` koddan doğmaz, Editor'de üretilir ve
prefab/sahne dosyalarına kod tarafı dokunamaz.

```
iki alan atanmamış kalır -> Awake'te NullReferenceException
null kontrolü eklenirse  -> hiç birim doğmayan bir tahta
derleyici: hiçbir şey der  .  test: adaptör EditMode'da sınanamaz,
                                     hiçbiri kırmızıya dönmez
```

**KAZANIRDI:** birim **çeşidi** ikiden fazla olduğu gün — okçu, süvari, tank; ya
da aynı tanımı yüzlerce birim paylaştığı gün. Bugün ikisi arasındaki tek fark
`Team`, yani paylaşacak bir şey yok.

**KARSILASTIRMA:**

```
const              Inspector'da YOK  -> her denge denemesi bir
                                        derleme turu ister
[SerializeField]   sahnede YAZILI    -> tek sahne, tek kopya;
                                        bugünkü tek okuyucu bu
ScriptableObject   asset'te YAZILI   -> paylaşılır, sürümlenir;
                                        karşılığı bir Editor adımı
```

**TEK CUMLE:** Serileştirmenin bedeli bir Inspector alanı, asset'e taşımanın
bedeli ise koddan doğmayan bir dosyadır.

**Alternatif:** sayıları `const` yapmak. Seçilmedi: tablonun ilk satırı — sayı
bir **denge** değeri olduğu sürece her deneme bir derleme turu ister (**değişmez**
olsaydı `Combatant.ReviveHealthDivisor` gibi doğru olurdu).

---

## moveRange

Hareket menzilinin sahibi bugün **bu alan**, yani Unity katmanı.

`MoveAction.Execute` menzili parametre olarak istiyor ve o kararın gerekçesi
`MoveAction`'ın kendi REDDEDILEN bloğunda yazılı: menzili `Unit`'e koymak, o
tipin "ne yapabileceğini bilmez" sözünü deler ve saldırı tarafının
(`AttackProfile`) verdiği cevapla çelişir. O blok doğru cevabı da söylüyor:
`AttackProfile`'ın ikizi olan bir `MoveProfile`.

`MoveProfile` **artık var** (`GridStrategy.Core`) ve `BattleActions.Move` onu
sayıdan kendisi kuruyor; **sayının** sahibi ise hâlâ burası. Bu bir karar değil,
adı konmuş bir **borç**.

### HARİTA: aynı nesneyle anahtarlanan kaç tablo var

`Unit` bir **sınıftır**; sözlük anahtarı olarak **referans** eşitliği kullanır.
Aynı nesneyle anahtarlanan her yeni tablo, senkron tutulması gereken yeni bir
**doğruluk kaynağıdır** — ve senkronun bir sahibi olmak zorunda.

```
BUGÜN — iki tablo, ikisinin de yazan/silen çifti BELLİ
                 ╔══════════════╗
     ┌───────────╢ Unit nesnesi ╟───────────┐
     ▼           ╚══════════════╝           ▼
unitViews (burada)                   combatants (Battle)
yazan : SpawnUnit                    yazan : Battle.AddUnit
silen : DespawnView                  silen : RemoveReadyForCleanup
     ▲                                       ▲
     └──── ikisini de AYNI iki satır tetikler ┘

REDDEDILEN — üçüncü tablo
                 ╔══════════════╗
     ┌───────────╢ Unit nesnesi ╟──────┬───────────┐
     ▼           ╚══════════════╝      ▼           ▼
unitViews                         combatants    moveRanges
                                                yazan : SpawnUnit
                                                silen : ???
>> KIRILMA NOKTASI <<  silme yolu üçüncü tabloyu BİLMİYOR;
ölmüş bir birim moveRanges'te sonsuza dek canlı kalır ve onu
temizleyecek satırı hiçbir tip talep etmiyor
```

### KAPSAM: yasak "koleksiyon alanı" değil, ANAHTARLI tablo

Kural: aynı nesneyle **anahtarlanmış** ve çağrılar arasında **anlam taşıyan** bir
tablo daha açmak yasak. Anahtarsız ya da anlam taşımayan koleksiyonlar bu kuralın
tamamen dışında.

**KARŞI ÖRNEK** aynı dosyada, [`cleanupBuffer`](#cleanupbuffer): o da bir
koleksiyon **alanıdır** ve kuralı çiğnemez — anahtarı yoktur, çağrılar arasında
hiçbir şey taşımaz, `Battle.RemoveReadyForCleanup` onu her çağrıda temizleyip
yeniden doldurur. Bir durum değil, tahsisten kaçınmak için yeniden kullanılan bir
**kaptır**. İkisini ayıran tek soru: **bu alanı silsem hangi bilgi kaybolur?**
`cleanupBuffer`'da: hiçbiri.

### İŞ BÖLÜMÜ: SAYININ sahibi ile KURALIN sahibi ayrı

```
moveRange (bu alan) ► SAYI  — kaç hücre; sahnede yazılı, Play'e
                      basmadan denenebilir
MoveProfile (Core)  ► KURAL — sayının ne anlama geldiği;
                      BattleActions.Move onu bu sayıdan kurar
```

Bu alan silinirse çağrı yerinde verilecek bir sayı kalmaz. `MoveProfile`
silinirse kural bir MonoBehaviour'ın içine döner ve EditMode'da sınanamaz olur —
yani aşağıdaki KIRILAN'ın yarısı, sözlük hiç doğmadan gerçekleşir. İkisi fazlalık
değil; borç, sayının hâlâ Unity katmanında oturmasında.

### REDDEDILEN

Menzil birime göre değişsin diye `SpawnUnit` parametre alır ve sayı burada
saklanır:

```csharp
private readonly Dictionary<Unit, int> moveRanges =
    new Dictionary<Unit, int>();
```

**KIRILAN:** `Unit` ile anahtarlanan **üçüncü** sözlük doğar — `unitViews`
burada, `combatants` `Battle`'da, bu da burada.

```
temizlikte silmeyi unutan tek satır -> ölmüş birim sonsuza dek
canlı kalır; ve bir SAVAŞ değeri MonoBehaviour'a taşınır
derleyici: hiçbir şey der  .  test: değer artık EditMode'da
                                    sınanamaz, yani onu koruyacak
                                    test YAZILAMAZ
```

**KAZANIRDI:** menzil gerçekten birimden birime değiştiği gün — ama o gün bile
cevap `Combatant`'ın yanına konulan bir `MoveProfile`'dır; bu sözlük onun test
edilemeyen taklidi olurdu.

**TEK CUMLE:** Aynı nesneyle anahtarlanan üçüncü sözlük, senkronunu hiçbir tipin
garanti etmediği üçüncü bir doğruluk kaynağıdır.

---

## placementGhost

Yerleştirme kipi (#10).

**Hayalet gerçek bir `Structure` değildir.** Tahtaya girmez, `Battle` onu bilmez,
hiçbir kural onu görmez; yalnızca bir `SpriteRenderer`'dır ve tek işi
"bırakırsan buraya konur" demektir.

### HARİTA: iki şerit, tahtaya YALNIZ biri dokunur

```
ÖNİZLEME ŞERİDİ — kip açıkken HER kare
  fare hareketi ──► placementGhost.transform.position = ...
                ──► placementGhost.enabled = true/false
  >> TAHTAYA HİÇ YAZMAZ <<  ◄── geri alınacak bir şey DOĞMUYOR

YAZMA ŞERİDİ — yalnız bırakma anında, BİR kez
  CommitPlacement ──► BattleActions.PlaceStructure(battle, ...)
                  ──► tahtaya gerçek Structure girer
                  ──► CreateStructureVisual görseli doğurur

REDDEDILEN — iki şerit BİRLEŞTİRİLSEYDİ
  fare hareketi ──► AddStructure ─► RemoveStructure ─► AddStructure
                    (kare başına bir yazma/silme ÇİFTİ)
  >> İPTAL YOLU ARTIK ZORUNLU << ve unutulabilir  ◄── kırılma
  unutulduğu an: hücreyi kapatan, hedeflenebilen, GÖRÜNMEZ bir bina
```

### KAPSAM: kural ÖNİZLEMEYE özeldir, "görsel üretmeye" değil

Kural: bir görsel **henüz verilmemiş** bir kararı gösteriyorsa, o kararı
**uygulayarak** gösteremez. Zaten var olan bir olguyu gösteren görsel bu kuralın
tamamen dışındadır.

**KARŞI ÖRNEK** aynı dosyada, [`BuildCellVisuals`](#buildcellvisuals): on beş
hücre görseli `Awake`'te doğrudan **üretilir** ve hiçbir şey ihlal edilmez —
çünkü onlar bir önizleme değil, `Battle`'ın zaten sahip olduğu bir olgunun
(`width × height`) yansımasıdır; geri alınacak bir karar yok. Hayaleti onlardan
ayıran tek şey, gösterdiği şeyin **henüz olmaması**.

### İŞ BÖLÜMÜ: gizleme ile yazma AYRI iki yolda

```
CancelPlacement              ► yalnız GÖRSELİ kapatır ve kipi
                               bırakır; tahtaya DOKUNMAZ
BattleActions.PlaceStructure ► tahtaya yazan TEK yol
```

`CancelPlacement` silinirse ekranda sahipsiz bir hayalet ve kapanmayan bir kip
kalır — ama savaşın kaydı **temizdir**. `PlaceStructure` çağrısı silinirse hiçbir
şey yerleşmez — kayıt yine **temizdir**. İki yolun ortak özelliği, kırılmanın
ekranla **sınırlı** kalması; hayalet gerçek olsaydı ikisinden birinin unutulması
doğrudan savaşın kaydını bozardı. Bölüşüm bu: biri görüntüden, öteki gerçeklikten
sorumlu.

### REDDEDILEN

Hayalet gerçek bir yapıdır; kipe girerken tahtaya eklenir, iptalde geri alınır:

```csharp
battle.AddStructure(selectedUnit, ghostStructure, x, y);
// ... iptalde: battle.RemoveStructure(selectedUnit);
```

**KIRILAN:** savaşın **kaydı** imleç hareketiyle mutasyona uğrar; fare her kare
hücre değiştirdiğinde tahtaya yazma/silme çifti gider.

```
iptal yolu unutulur -> hücreyi kapatan, hedeflenen, görünmez
                       bir HAYALET BİNA tahtada kalır
derleyici: hiçbir şey der  .  test: yeşil, çünkü testler Battle'ı
                                    doğrudan kurar ve iptal yolundan
                                    hiç geçmez
```

**KAZANIRDI:** geçerlilik önizlemesi tahtanın **gerçek** cevabını göstermek
zorunda kalsaydı ve `Battle` bir "deneme/geri al" yeteneği kazansaydı — o gün
hayaleti gerçek yapmak tek yol olurdu.

**TEK CUMLE:** Bir önizleme gösterdiği şeyi **üreterek** gösteriyorsa artık
önizleme değil, geri alınması unutulabilen bir yazmadır.

---

## dragThreshold

Eşik **dünya biriminde**, pikselde değil — ve bu bir karardır.

Piksel seçilseydi aynı parmak hareketi 1920'lik ekranda "tıklama", 2560'lık
ekranda "sürükleme" sayılırdı; yani giriş **şekli** ekran çözünürlüğüne bağlı
olarak değişirdi. Aynı tuzağın kardeşi
[`TryReadPointerCell`](#tryreadpointercell)'de zaten yazılı: `ScreenToWorldPoint`
tam da bu yüzden var.

Dünya birimi ayrıca **ölçülebilir** bir anlam taşır: 0,25 "çeyrek hücre".

---

## unityGrid

Unity'nin `Grid` bileşeni **sadece** bir koordinat çevirmenidir: hücre indeksi
↔ dünya konumu. Hiçbir şey çizmez, kaç hücre olduğunu bilmez, oyun durumu
tutmaz. Tuttuğu tek şey ayarlardır (`cellSize`, `cellGap`, `cellLayout`).

---

## battle

Tahtanın ve savaşın durumu **burada değil**, `Battle`'ın içinde yaşar: kaç hücre
var, hangi hücrede kim duruyor, kimin canı ne. Bu alan yalnızca o bütüne bir
tutamaktır.

Burada bir `private UnitGrid board;` alanı vardı ve `Awake` onu kendisi
kuruyordu. O alanın silinmesi bu dosyanın en pahalı satırı: tahtaya yazan tek yol
artık `Battle.AddUnit` ve `Combatant`'ı olmayan bir birimin tahtada durması
imkânsız hâle geldi.

**Derin anlatım:** [03-tahta sahipliği](../../konular/03-tahta-sahipligi.md)

---

## unitViews

Core'daki `Unit` ile ekrandaki görselini eşleyen tablo.

**Anahtar neden `Unit`?** Çünkü **konum** sadece tahtada yaşasın istiyoruz.
Görsel "neredeyim" bilmez; konumu her gerektiğinde `Battle`'dan hesaplanır.
Alternatifi (`GameObject[,]` paralel dizi) konumu iki yerde tutardı ve ikisi
kayarsa hata sessiz olurdu.

`Equals`/`GetHashCode` yazmaya gerek yok: `Unit` bir sınıftır, varsayılan
karşılaştırma **referans** eşitliğidir ve aradığımız zaten tam olarak o nesnenin
kendisi. Değer eşitliği ancak "aynı içerikli iki ayrı `Unit` aynı anahtar
sayılsın" istenirse gerekirdi; istemiyoruz.

### REFAKTÖR NOTU GERÇEKLEŞTİ (seçim çerçevesi)

Not tam olarak bunu öngörüyordu ve aynen öyle oldu: tablo silinmedi, **anahtarı**
değişmedi, yalnızca **değer** tipi `GameObject` yerine `UnitView` oldu. `UnitView`
bu tasarımın yerine geçmedi, **üstüne** geldi.

Kazanılan şey: değer artık "bir nesne" değil, **konuşulabilir** bir arayüz.
Eskiden seçimi uygulamak için adaptör `GetComponent` ile görselin içini
kurcalıyordu; şimdi `view.SetSelected(...)` diyor ve çerçevenin bir çocuk nesnede
yaşadığını hiç bilmiyor.

Aynı anahtar seçimi `Battle` tarafında da devralındı; gerekçesi `Battle`'ın
`combatants` sözlüğünün üstünde ve bu satırlara adıyla atıf yapıyor.

---

## cleanupBuffer

Temizlik süpürmesinin **çıkış** tamponu. Alan olmasının tek sebebi **tahsis**:
her karede yeni bir `List` kurmak kare başına çöp üretirdi.
`Battle.RemoveReadyForCleanup` onu her çağrıda temizleyip yeniden doldurur, yani
bu alanın çağrılar arasında taşıdığı bir anlam **yok** — bir durum değil, yeniden
kullanılan bir kaptır.

---

## gesture

Tıklama ile sürüklemeyi ayıran saf tip. Alan olmasının sebebi **tam olarak**
durum tutması: basıldığı nokta ve eşiğin aşılıp aşılmadığı kareler **arasında**
yaşamak zorunda. `cleanupBuffer`'ın tersi bir alan — orası yeniden kullanılan bir
kap, burası gerçek bir hafıza.

Kurucusu eşiği dışarıdan istiyor (S-03'ün zaman kararının ikizi), bu yüzden
`Awake`'te kuruluyor: serileştirilmiş alan ancak o an okunabilir.

---

## ghostIsCarried

Hayalet fareye **yapıştı** mı. İki giriş şeklini ayıran tek alan budur.

```
sürükle-bırak : Press -> MoveTo... -> DragReleased  -> YERLEŞTİR
                (hiç yapışmaz; bu alan false kalır)
tıkla-bırak   : Press -> ClickReleased -> YAPIŞTI (kipte kal)
                Press -> ClickReleased -> YERLEŞTİR
```

**Neden bir sayaç değil bir bool:** ayrım "kaçıncı tıklama" değil, hayalet fareye
bağlı mı bağlı değil mi. Sayaç yazsaydık üçüncü tıklamanın ne anlama geldiği
tanımsız kalırdı.

`isPlacingStructure` ise "yerleştirme kipinde miyiz" sorusunu taşır ve bir **oyun
durumudur**, çeviri durumu değil — yani `selectedUnit` gibi rol başlığındaki
"hafıza: var" satırının altına düşer.

---

## Awake()

`GetComponent` bir **sorgudur**: bu GameObject'in bileşen listesinde arar ve
bulduğuna referans döner. Hiçbir şey yaratmaz, tekrar çağrılması durumu
değiştirmez. Listede bir `Grid` bulunacağını `RequireComponent` garanti eder;
`Grid`'i "üreten" o değildir.

**Jest neden burada kuruluyor:** eşik Inspector'dan geliyor. Alan bildiriminde
`new PointerGesture(dragThreshold)` yazsaydık serileştirilmiş değer daha
okunmamış olurdu ve nesne her zaman C# başlatıcısındaki sayıyla doğardı —
Inspector'daki değer sessizce hiçbir işe yaramazdı.

**Hayalet, kipte olmadığımız sürece çizilmez.** Sahnede açık bırakılmış olabilir;
`UnitView.Awake`'in `SetSelected(false)` ile yaptığı işin birebir aynısı ve
gerekçesi de aynı: yazılı durumu çalışma zamanı değişmezine çevirmek.

### GEÇİCİ: iki demo birim

Oyun kurulumu geldiğinde buradan kalkacak. **İkisi de gerekli** ve bu bir tercih
değil: saldırı zincirinin kapandığını göstermek için birbirine tıklanabilen
**iki** birim şart, ve `TargetingRules` dost ateşini reddettiği için tarafları
farklı olmak zorunda. Komşu hücreler seçildi ki menzil 1 ile denenebilsin.

---

## OnEnable() ve OnDisable()

Bu abonelik bir **hata düzeltmesidir**, bir özellik değil. Bugüne kadar ekran
yalnız **saldırıdan** sonra tazeleniyordu; oysa `Downed → Dead` geçişi `Tick`'in
içinde, hiçbir tıklama olmadan gerçekleşir. Yani düşmüş bir birim ekranda
**yatık** kalıyor, gri hiç olmuyordu ve hatayı gösterecek tek şey gözdü — hiçbir
test kırmızı değildi.

### HARİTA: aynı bileşen İKİ ayrı ritimle ölçülür

Unity'nin geri çağrıları tek bir zincir değildir: biri **doğumu**, öteki
**etkinliği** ölçer ve ikincisi sınırsız tekrar eder.

```
Awake ─────────────────────────────────────────► OnDestroy
  │           (nesne başına BİR kez)                  ▲
  │                                                   │
  ├──► OnEnable ─ Update ... ─ OnDisable ─┐            │
  │        ▲                              │            │
  │        └────── enabled = true ────────┘            │
  │         >> BU HALKA SINIRSIZ TEKRAR EDER <<        │
  └───────────────────────────────────────────────────┘

① SEÇİLEN    OnEnable +=  /  OnDisable -=      halkaya BAĞLI
             dinleyici sayısı: etkinken bir, kapalıyken sıfır
② OnEnable += ama OnDisable -= YOK    ◄── abonelik BİRİKİR
             dinleyici sayısı: her etkinleşmede bir artar
③ REDDEDILEN Awake +=  /  OnDestroy -=      halkanın DIŞINDA
             dinleyici sayısı: HER ZAMAN bir — kapalıyken bile
             >> "kapalı" sözü tam burada düşer <<
```

**ÇİFTİN SİMETRİSİ, yani ②'nin adı:** `OnEnable` her etkinleşmede **çalışır**,
dolayısıyla `OnDisable`'da bırakılmazsa abonelik **birikir** ve aynı olay iki kez
dinlenir. Bugün bu "iki kat iş" gibi görünür çünkü `SetState` idempotenttir — ve
tam bu yüzden tehlikelidir: hata **sessizdir** ve ilk yan etkili dinleyici
eklendiği gün patlar.

**ASIL KIRILMA sızıntı değil ÖMÜR:** olay, `Battle`'dan bu MonoBehaviour'a
referans **tutar**. `Battle` bu bileşenden **uzun** yaşadığı gün (kayıtlı oyun,
sunucu tarafı simülasyon) bırakılmamış abonelik **yok edilmiş** bir
MonoBehaviour'ı çağırır ve kaynağından çok uzakta patlar.

### KAPSAM: kural OLAY dinlemeye özeldir, "kurulum"a değil

Ayıran soru: **bu iş nesnenin doğumuna mı, etkinliğine mi ait?**

```
etkinliğe ait ► olay aboneliği, girdi dinleme, açık kalan kip
doğuma ait    ► bileşen çözme, nesne kurma, sahne inşası
```

**KARŞI ÖRNEK** aynı dosyada, hemen yukarıdaki [`Awake`](#awake):
`new Battle(...)`, `new PointerGesture(...)` ve `BuildCellVisuals` orada durur ve
doğru yerdedir. `BuildCellVisuals` `OnEnable`'a taşınsaydı bileşen her yeniden
etkinleşmede on beş hücre GameObject'ini **bir kez daha** doğururdu. Yani kural
"her şeyi `OnEnable`'a koy" değil; simetri işin **ritmine** göre seçilir.

### İŞ BÖLÜMÜ: OnDisable İKİ ayrı sözü birden kapatıyor

```
battle.UnitStateChanged -= ► DİNLEME sözü: kapalı bileşen sahneye
                             yazmaz
CancelPlacement()          ► KİP sözü: kapalı bileşen açık bir
                             giriş kipi bırakmaz
```

İlki silinirse kapalı adaptör, `Tick`'lenen bir savaşın görsellerini değiştirmeye
devam eder. İkincisi silinirse yeniden açılan adaptör hayaleti gizli, kipi
**açık** bulur ve bir sonraki tıklama görünmez bir yapı yerleştirir. İkisi de aynı
cümlenin — "kapalı bileşen dünyaya dokunmaz" — yarısını taşıyor; hiçbiri ötekinin
yedeği değil.

### GARANTİ NEREDE BİTİYOR

Bu simetriyi **derleyici değil disiplin** tutuyor: `+=` ile `-=`nin eşleştiğini
denetleyen hiçbir dil mekanizması yok; eksik bir `-=` tek bir uyarı bile üretmez.
Simetri ancak iki metodun yan yana durması ve bu notun okunmasıyla ayakta
kalıyor.

### REDDEDILEN

Abonelik `Awake`'te kurulur, `OnDestroy`'da bırakılır:

```csharp
private void Awake()     { battle.UnitStateChanged += OnUnitStateChanged; }
private void OnDestroy() { battle.UnitStateChanged -= OnUnitStateChanged; }
```

**KIRILAN:** kırılan şey "kapalı" sözünün kendisi; çift abonelik **yok**.

```
bileşen kapatılır -> Update durur, dinleme DEVAM eder
Battle'ı başka bir yol Tick'lerse -> kapalı adaptör sahnedeki
                                     görselleri değiştirmeye devam eder
derleyici: hiçbir şey der  .  test: bugün hiçbiri sormaz
```

**KAZANIRDI:** abonelik nesnenin **ömrü** boyunca bir kez kurulup bir kez
bırakılan bir **kaynak** olsaydı — dosya, soket, bildirim kaydı; onlar açılıp
kapanmayla değil doğup ölmeyle eşleşir.

**TEK CUMLE:** `Awake`/`OnDestroy` nesnenin **doğumunu**, `OnEnable`/`OnDisable`
ise **etkinliğini** eşler; olay dinlemek etkinliğe aittir.

---

## OnUnitStateChanged(Unit unit, UnitState from, UnitState to)

İmza `Action<Unit, UnitState, UnitState>`: **kim**, **nereden**, **nereye**.

"Nereden" bugün **kullanılmıyor** ve bu bir eksiklik değil — olayın taşıdığı bilgi
ile bu dinleyicinin ihtiyacı aynı olmak zorunda değil. Kullanacağı ilk gün adı
hazır: `Alive → Downed` düşme animasyonu, `Downed → Alive` diriliş animasyonu;
ikisi de "nereye"den türetilemez.

---

## Update()

**Derin anlatım:** [07-tıklamadan eyleme](../../konular/07-tiklamadan-eyleme.md)

### ZAMAN HER KARE İLERLER, tıklama olsun olmasın

Ve bu sıra bir karardır: `AdvanceBattleTime` erken çıkışın **altına** konsaydı
savaşın saati yalnızca oyuncu tıkladığında işlerdi, yani düşmüş bir birim el
sürülmediği sürece asla ölmezdi.

### KİP AYRIMI, MEVCUT AKIŞIN ÜSTÜNDE

Altına konsaydı yerleştirme sırasındaki her basış önce `HandleClick`'ten
geçerdi: hayalet taşınırken tahtadaki birimler seçilir, saldırı emri verilir,
hareket denenirdi. Kip, girdinin **anlamını** baştan sona değiştirir; dolayısıyla
ayrım en başta yapılır.

**"hayır" dalı değişmedi:** kip kapalıyken bu dosyanın giriş akışı yerleştirme
kipi eklenmeden önceki hâliyle birebir aynıdır.

### Neden `GetMouseButtonDown`

"Down" = **sadece** basıldığı karede true. Tuşu basılı tutarsan sonraki karelerde
false döner; `GetMouseButton` (Down'suz) ise basılı olduğu her karede true
olurdu. Tek tıklama istiyoruz, o yüzden `Down`. Sorguların tam ayrımı
[girdi okuma notunda](#girdi-okuma-notu).

---

## TryEnterPlacementMode()

**Yapıyı kim koyar:** seçili birim. Bu bir çeviri değil bir **oyun** kuralıdır ve
doğru sahibi burası **değil** — `BattleActions.PlaceStructure` yerleştirmenin
geçerliliğine kendisi karar verir. Buradaki tek şart **teknik**: imzanın istediği
`unit` argümanını verebilmek için elde bir birim olmak zorunda.

**Kipe her girişte hayalet serbesttir:** ilk bırakış onu ya yerleştirir
(sürükleme) ya fareye yapıştırır (tıklama). Bu yüzden `ghostIsCarried` sıfırlanır.

**Jest, kipler arasında taşınmaz.** Sıfırlanmasaydı önceki kipten kalan "Pressed"
fazı, bu kipteki ilk bırakışı sahte bir tıklama olarak okurdu.

---

## UpdatePlacement()

Yerleştirme kipinin tek karesi: hayaleti taşır, jesti besler ve bırakma şekline
göre yerleştirir.

Aşağıdaki üç başlık bu metottaki üç ayrı **KONUM** kararını anlatıyor. Üçü de
"hangi satır nerede duruyor" hakkında olduğu için gövde bir kez, işaretli olarak
burada duruyor (`Debug.Log` satırları kısaltıldı):

```csharp
private void UpdatePlacement()
{
    if (selectedUnit == null)                    // ◄── ① KOYACAK BİRİM kayıp mı
    {
        CancelPlacement();
        return;
    }

    if (Input.GetKeyDown(placementCancelKey))    // ◄── ② İPTAL — bırakıştan ÖNCE
    {
        CancelPlacement();
        return;
    }

    if (!TryReadPointerCell(out float worldX, out float worldY, out int x, out int y))
    {
        return;
    }

    // ◄── ③ HER KARE — hiçbir if'in ya da fazın içinde DEĞİL
    placementGhost.transform.position = CellCentre(x, y);

    PointerPhase phase = FeedGesture(worldX, worldY);

    switch (phase)                               // ◄── BIRAKIŞ burada işlenir
    {
        case PointerPhase.DragReleased:
            CommitPlacement(x, y);
            break;

        case PointerPhase.ClickReleased:
            if (ghostIsCarried) { CommitPlacement(x, y); }
            else { ghostIsCarried = true; gesture.Reset(); }
            break;
    }
}
```

### KOYACAK BİRİM ARADA KAYBOLABİLİR

Ve bu teorik değil, bugün gerçekleşen bir sıra: `AdvanceBattleTime` bu metottan
**önce** koşar ve ceset süresi dolan birimi `DespawnView` ile temizlerken
`selectedUnit`'i null'a çeker. Kastedilen düzenleme ①'in **tümüyle silinmesi** —
yani metodun doğrudan ②'yle başlaması: o gün `CommitPlacement` yoluyla
`PlaceStructure`'a `null` bir birim gider ve savaş katmanı, sebebi ekranda hiç
görünmeyen bir exception atar.

### İPTAL HER ZAMAN ÖNCE

Kastedilen düzenleme ②'nin `switch`'in **altına** taşınması. O gün iptal tuşu,
aynı karede gelen bir bırakışın yerleştirmesinden **sonra** işlenirdi: oyuncu
iptal ettiğini sanır, tahtada bir yapı bulurdu.

### HER KARE, koşulsuz: hayalet fare hücresinin MERKEZİNDE durur

Kastedilen düzenleme ③'ün bir `if (ghostIsCarried)` ya da bir
`case PointerPhase.Dragging:` dalının **içine** alınması. O gün tıkla-bırak
akışında hayalet yerinde donar ve oyuncu nereye koyacağını göremezdi — iki giriş
şeklinin ikisinde de takip etmesi şartının somut karşılığı ③'ün hiçbir dalın
içinde durmamasıdır.

### HAYALETİN GEÇERSİZ HÜCREDE FARKLI GÖRÜNMESİ (kırmızı tint)

**Henüz yapılmıyor** ve tetiği yazılı. Sebep "önemsiz" değil, **sahiplik**:
yerleştirmenin geçerli olup olmadığına `BattleActions.PlaceStructure` karar verir
ve cevabını ancak **yerleştirerek** verir.

Her kare rengi boyamak için ya tahtayı her kare mutasyona uğratmak
([`placementGhost`](#placementghost)'un REDDEDILEN'inin ta kendisi) ya da kuralın
bir **kopyasını** buraya yazmak gerekirdi — "hücre dolu mu, tahta içinde mi" — ve
o kopya, kural büyüdüğü gün (sıra, menzil, maliyet) sessizce **yalan** söylemeye
başlardı: yeşil hayalet, reddedilen yerleştirme.

**ÖNEM KAZANACAĞI KOŞUL, tek cümlede:** `BattleActions` tahtaya dokunmayan bir
soru üyesi kazandığı gün — `CanPlaceStructure(...)` ya da `PlacementOutcome`u
mutasyonsuz hesaplayan bir önizleme — hayalet o üyeyi her kare sorar ve tint
kuralın **kopyasını** değil **cevabını** taşır.

### `default` dalı BİLEREK YOK

Ve bu [`ReactToAttack`](#reacttoattackattackoutcome-outcome-unit-target-int-x-int-y)'teki
kararla **çelişmiyor**: orada switch bir **sonucun** bütün değerlerini karşılamak
zorunda, burada ise beş fazın üçü (`Idle`, `Pressed`, `Dragging`) "henüz bir şey
olmadı" demektir. Bir **sonuç** enum'unda işlenmeyen değer bir hatadır; bir **faz**
enum'unda işlenmeyen faz normal akıştır.

---

## FeedGesture(float worldX, float worldY)

Motorun üç fare sorgusunu `PointerGesture`'ın üç metoduna çevirir ve ortaya çıkan
fazı verir.

Gerekçenin tamamı [girdi okuma notunda](#girdi-okuma-notu); buradaki şekil o
notun kodu. `Down`/`MoveTo` bir if-else zinciri, `Up` ise **ayrı** bir `if` —
çünkü tek bir karede `Down` ve `Up` birlikte true olabilir.

**Derin anlatım:** [07-tıklamadan eyleme](../../konular/07-tiklamadan-eyleme.md)

---

## CommitPlacement(int x, int y)

### Geçerliliğe BU DOSYA KARAR VERMİYOR

Tek satırlık kanıtı şu: metodun içinde ne bir sınır kontrolü, ne bir "hücre dolu
mu" sorusu, ne de bir sıra sorusu var. Hepsi `PlaceStructure`'ın içinde ve orada
kalmalı — çeviri ile karar arasındaki sınır tam olarak burası.

### KİPTEN ÇIKIŞ, sonuçtan BAĞIMSIZ

Ret de bir cevaptır ve oyuncu reddedilen bir yerleştirmeden sonra hayaletin
fareye yapışmaya devam etmesini beklemez. Reddi düzeltmenin yolu kipe yeniden
girmektir, çünkü ret sebebi çoğu zaman hücre değil **birimdir**.

### TEK BİR DEĞERLE KARŞILAŞTIRMA, TAM SWITCH DEĞİL

Ve bu **bilinçli bir eksikliktir**, üslup değil. Buradaki tek soru "kondu mu":
kondu ise görsel doğar, konmadıysa sebebi `Debug.Log` zaten basıyor ve bu
dosyanın ret sebebine göre yapacağı **farklı** bir işi yok.
[`ReactToAttack`](#reacttoattackattackoutcome-outcome-unit-target-int-x-int-y)'teki
tam switch'in sebebi tersi: orada her ret ayrı bir mesaj ve ayrı bir oyuncu
yönlendirmesi üretiyor.

**EŞİK, ve tetiği net:** bir ret sebebi ekranda **farklı** bir şey yaptırdığı gün
bu karşılaştırma, `ReactToAttack` ve `ReactToMove` ile aynı şekle — her ret için
bir dal, `default`'ta `LogError` — çevrilir.

---

## CancelPlacement()

Yerleştirme kipini kapatır ve hayaleti gizler. **Tahtaya dokunmaz.**

İptalin tahtaya dokunmaması bir tesadüf değil, hayaletin gerçek bir yapı
**olmamasının** doğrudan sonucudur: geri alınacak bir şey yok, çünkü yapılmış bir
şey yok. [`placementGhost`](#placementghost)'un REDDEDILEN bloğu bu metodun
alternatif hâlini — `battle.RemoveStructure(...)` çağrısını — ve onun unutulduğu
günkü sonucunu yazıyor.

---

## NewStructure(Unit placer)

**Taraf, yapıyı koyan birimden okunur — Inspector'dan değil.** Ayrı bir alan
koysaydık aynı bilginin ikinci kaynağı doğardı ve düşmanın yaptığı bina oyuncunun
tarafında görünebilirdi; hata sessiz olurdu.

**`AttackProfile` verilmiyor:** `Structure`'ın kurucusu onu isteğe bağlı tutuyor
ve gerekçesi o dosyada yazılı — saldırmayan yapı **kuraldır**, saldıran yapı
istisnadır. Bugün koyduğumuz şey bir depodur.

---

## CreateStructureVisual(int x, int y)

### NEDEN PREFAB DEĞİL, KODLA KURULAN BİR GameObject

Kod tarafı sahne ve prefab dosyalarını üretemez, dolayısıyla atanması gereken bir
alan daha eklemek "Inspector'da boş kalan alan" riskini büyütürdü.

Sprite hayaletten okunuyor: önizlemede görülen şeyin tahtaya konan şeyle **aynı**
görünmesi böylece kurulum adımı gerektirmeden garanti altına alınıyor. Aynı
deseni [`CreateCellVisual`](#createcellvisualint-x-int-y) zaten kullanıyor.

### GÖRSEL BİR TABLOYA KAYDEDİLMİYOR — ve sınırı burada yazıyorum

Bugün onu tekrar bulması gereken hiçbir çağıran yok: yerleşen yapılar yıkılmıyor,
taşınmıyor, seçilmiyor. Yıkım geldiği gün bu görselin sahibi
`Dictionary<Unit, StructureView>` olur ve o tip, `UnitView`'ın kardeşi olarak
doğar; bugün onu yazmak, alıcısı olmayan bir tablo ve senkron tutulması gereken
üçüncü bir sözlük demek olurdu.

Zemin 0, birimler ve yapılar 1: yapı zeminin üstünde çizilir.

---

## TryReadPointerCell

> Tam imza: `TryReadPointerCell(out float worldX, out float worldY, out int x, out int y)`

Fare konumunu dünya koordinatına ve hücre indeksine çevirir. **Bir Unity tipinin
Core'un diline çevrildiği tek yer burasıdır.**

### İKİ ÇAĞIRAN, TEK ÇEVİRİ

Tıklama akışı ([`HandleClick`](#handleclick)) ve yerleştirme kipi
([`UpdatePlacement`](#updateplacement)). Çeviri kopyalansaydı biri değiştiğinde
fare ile hayalet farklı hücreleri gösterirdi ve hiçbir şey patlamazdı —
[`CellCentre`](#cellcentreint-x-int-y)'ın kendi gerekçesinin aynadaki hâli.

### DÖNÜŞ bool + out, nullable DEĞİL

S-05'in kararı. "Kamera yok" bir **programcı** hatasıdır ve çağıranın yapacağı
tek şey akıştan çıkmaktır; `out` ile birlikte tek bir `if` yeter.

### Camera.main ve ScreenToWorldPoint

`Camera.main`, **"MainCamera" etiketli** kamerayı bulur; "ana kamera" diye bir
kavram yoktur, etiket vardır. Etiketli kamera yoksa null döner ve bir sonraki
satır patlardı.

`Input.mousePosition` **ekran** pikselidir: sol alt (0,0), sağ üst
(ekranGenişliği, ekranYüksekliği). Kameranın konumu değildir.
`ScreenToWorldPoint` bu pikseli dünya birimine çevirir ve çeviri **kameraya**
bağlıdır: kamera taşınırsa aynı piksel farklı bir dünya noktasına düşer. Çeviri
olmasaydı 1920'lik ve 2560'lık ekranda aynı tıklama farklı hücreyi seçerdi.
[`dragThreshold`](#dragthreshold)'un **dünya** biriminde ölçülmesinin gerekçesi de
tam olarak bu cümledir.

`Vector3Int` sınırın ötesine geçmez; "tahta içinde mi" sorusunu soran taraf yine
`Battle`'dır, bu metot değil.

---

## AdvanceBattleTime()

Savaşın saatini ilerletir ve ceset süresi dolanları hem savaştan hem ekrandan
kaldırır.

### ZAMANI BURADAN VERMEK ZORUNLU

`UnitLifecycle` bilerek `Time.deltaTime` okumuyor — ölçümü `UnitLifecycle.Tick`'in
üstündeki REDDEDILEN bloğu taşıyor: EditMode'da o değer sıfır değil, 0,017675
dönüyor ve testi sessizce anlamsızlaştırıyordu. Saatin tek gerçek kaynağı
motorda; motoru gören tek katman burası.

### TEMİZLİK NEDEN YOKLAMA DEĞİL TOPLU

Seçenek "her karede her savaşçıyı yokla" ile "`Battle`'a süpürme metodu ekle"
arasındaydı ve gerekçesi `Battle.RemoveReadyForCleanup`'ın REDDEDILEN bloğunda
kod olarak yazılı; özeti tek satır: **yoklama ancak görseli olan birimleri
görür**, oysa temizlenmesi gereken şey savaşın kaydıdır.

### Süpürme ile olay ÇELİŞMİYOR

Bu satırın eski hâli *"`Combatant` durum değişimini dışarı vermiyor, dolayısıyla
`Downed → Dead` geçişini kimse duyamıyor"* diyordu. **Artık duyuluyor**
(`Battle.UnitStateChanged`) ve `OnEnable` tam olarak onu dinliyor.

Süpürmenin gerekçesi yine de **ayakta** kalıyor, çünkü ikisi **farklı** iki
soruya cevap veriyor — S-07'nin üçüncü ayrımı: olay "durum değişti"yi taşır,
süpürme "artık silinebilir"i. Bir birim `Dead`'e geçtiği an ekranda gri olur ama
savaşın kaydından ancak ceset penceresi dolunca çıkar; olay o ikinci anı hiç
bilmez.

---

## BuildCellVisuals()

`LogError`, `Log` değil: bu bir **programcı** hatasıdır (kurulum eksik), oyun
akışının normal bir sonucu değil. Kırmızıdır ve filtrelenebilir. `return` ile
birlikte gelir: sprite yoksa 15 görünmez GameObject üretmektense gürültüyle
durmak yeğdir.

---

## CreateCellVisual(int x, int y)

**Çıplak `transform`** = `this.transform`, yani bu bileşenin bağlı olduğu
GameObject'in `Transform`'u. `Component` sınıfından miras gelir. Ebeveyn-çocuk
hiyerarşisi `GameObject`'te değil `Transform`'da yaşar. Amaç konum değil **toplu
yaşam döngüsü**: tahtayı yok etmek, gizlemek veya taşımak tek çağrıyla 15 hücreye
birden uygulanır.

**Hücrenin merkezi, `CellToWorld` değil:** köşe kullanılsaydı her hücre yarım
kare kaymış görünürdü.

**`AddComponent` bir mutasyondur:** her çağrı yeni bir bileşen ekler.
`GetComponent`'in aksine idempotent değildir, bu yüzden kurulum kodunda yaşar;
kare başına çalışan bir yere konulamaz.

`SpriteRenderer` **çizer**; sprite ise **çizilecek varlıktır**. Çizen ile çizilen
ayrı şeylerdir.

**Çizim önceliği:** aynı katmanda büyük değer üste çizilir. Zemin 0; üzerine
gelen birimler 1 alır ve zeminin üstünde görünür.

---

## PickTerrainSprite(int x, int y)

**Deterministik:** aynı hücre her Play'de aynı sprite'ı alır. Random olsaydı her
çalıştırma farklı görünür ve gördüğün bir hatayı tekrar üretmek imkânsızlaşırdı.

7 ve 13 **asal** sayıdır; çarpanların ortak böleni olmaması düzenli şerit deseni
oluşmasını engeller. `x` ve `y` döngüden gelir, ikisi de `>= 0`; negatif
olabilseydi sonuç negatif çıkabileceği için `Mathf.Abs` gerekirdi.

---

## CellCentre(int x, int y)

Hücre indeksini dünya konumuna çeviren **tek** yer. Üç çağıranı var (zemin
kurulumu, birim doğuşu, hareket) ve üçü de aynı cevabı almak zorunda; çeviri
kopyalansaydı biri değiştiğinde birimler zeminden kayardı ve hiçbir şey
patlamazdı.

---

## SpawnUnit(string name, Team team, int x, int y)

**Önce kural, sonra görsel.** `AddUnit` dolu hücreyi ve tahta dışını exception ile
reddeder; o hata görsel doğmadan patlasın ki ekranda karşılığı olmayan bir birim
asla oluşmasın.

### HARİTA: tahtaya giden kapılar

"Tahtada var" ile "savaşta var" **iki ayrı olgudur**. İkisini **aynı** çağrıda
doğuran tek bir kapı olduğu sürece ayrışamazlar.

```
SEÇİLEN — tek kapı
  SpawnUnit ──► battle.AddUnit ──┬──► UnitGrid'e yerleşir
                >> TEK KAPI <<   └──► combatants'a kaydolur
                                     (ikisi AYNI çağrıda)

REDDEDILEN — yan kapı
  SpawnUnit ──► board.PlaceUnit ─┬──► UnitGrid'e yerleşir
                                 └──► combatants'a KAYDOLMAZ ✗
  ◄── AYRIŞMA NOKTASI: tahtada duran ama savaşta olmayan birim
```

Yan kapının bugün **açılamaması** bir yasak değil bir **yokluk**: bu tipte bir
`UnitGrid` alanı yok ve `Battle.Board` internal. Şekli ve gerekçesi `Battle`'ın
`board` alanının üstündeki sahiplik bloğunda çizili.

### KAPSAM: kural TAHTAYA yazmaya özeldir, sahneye değil

Kural: savaşın kaydını ilgilendiren her yazma `Battle`'dan geçmeli. Savaşın hiç
bilmediği sahne nesneleri bu kuralın dışında.

**KARŞI ÖRNEK** aynı dosyada,
[`CreateCellVisual`](#createcellvisualint-x-int-y): on beş zemin GameObject'i
`Battle`'a **hiç** kaydedilmeden doğar ve doğru olan budur — zemin bir savaşçı
değil, hücrenin üstüne oturan bir resimdir. Onları kaydetmek `combatants`
sözlüğüne savaşmayan girdiler eklerdi. Ayıran soru: **bu şeyin durumunu bir kural
sorguluyor mu?**

### İŞ BÖLÜMÜ: bu metot İKİ kaydı birden açıyor

```
battle.AddUnit(...)       ► SAVAŞ kaydı — tahtadaki yer ve
                            combatants girdisi
unitViews.Add(unit, view) ► EKRAN indeksi — Unit'ten görsele
```

`AddUnit` silinirse ekranda tıklanabilen ama saldırıda "bu savaşta değil" diye
patlayan bir birim doğar. `unitViews.Add` silinirse birim savaşır ama
`TryGetView` her seferinde `LogError` basar: ne taşınır, ne seçilir, ne
grileşir. İkisi aynı şeyi iki kez kaydetmiyor; biri kuralın, öteki ekranın
tarafını tutuyor.

**KAPANIŞ SİMETRİK DEĞİL ve bilerek:** savaş kaydını
`Battle.RemoveReadyForCleanup`, ekran indeksini `DespawnView` kapatır —
ikincisini birincisi tetikler (bkz. [`AdvanceBattleTime`](#advancebattletime)).
Açılış tek satırda, kapanış zincirle olur; bu yüzden kapanış tarafı bu dosyanın
en kolay unutulan yarısıdır.

### REDDEDILEN

Birim doğrudan tahtaya yazılır, savaş kaydı atlanır:

```csharp
board.PlaceUnit(x, y, unit);
```

**KIRILAN:** tahtada duran ama `Combatant`'ı **olmayan** bir birim doğar (aynı
kırılma `Battle`'ın kurucu bloğunda adlandırılmıştı).

```
ekranda görünür, tıklanır, seçilir -> ilk saldırıda
BattleActions.Attack "The unit is not in this battle." diye patlar
(BattleActions'ın kimlik kapısı)
derleyici: hiçbir şey der  .  test: hata ancak Play'de
```

**KAZANIRDI:** tahtada savaşmayan bir şeyin durması gerekseydi — dekor, bayrak,
hedef işareti; ama o tür de `Unit` değil, `Structure`'ın kardeşi olurdu.

**TEK CUMLE:** Tahtaya yazmanın tek kapısı `Battle` olmazsa "tahtada var" ile
"savaşta var" iki ayrı gerçek hâline gelir.

### Instantiate hakkında

`Instantiate`, prefab dosyasından **yeni** bir kopya doğurur. Prefab'ın kendisi
sahneye girmez; sahnede duran her zaman bir kopyadır. İkinci parametre ebeveyni
verir: hücreler gibi birimler de tahtanın çocuğu olur, böylece tahta yok olunca
birlikte gider.

Argüman `UnitView` olduğu için dönüş de `UnitView`'dır — `Instantiate` generic'tir
ve verdiğin tipi geri verir. Bu yüzden burada tek bir `GetComponent` yok: kopya
doğduğu anda zaten aradığımız tipte.

`view.name` yazmak GameObject'in adını değiştirir; `name` property'si `Component`
üzerinden GameObject'e iletilir. Ayrı bir isim alanı yok.

---

## NewCombatant(Team team)

**Yaşam döngüsü pencereleri bilerek serileştirilmedi**, oysa can ve hasar
serileştirildi. Ayrım keyfi değil: "kaç saniye düşük kalır" sorusunun **zaten**
bir sahibi var — `UnitLifecycle.DefaultDownedWindowSeconds` ve
`DefaultCorpseWindowSeconds` adında iki sabit. Buraya bir Inspector alanı koymak
aynı sayıya ikinci bir kaynak açardı ve sahnedeki değer sabiti sessizce ezerdi.

Can ve hasarın ise hiçbir yerde varsayılanı yok; onların ilk sahibi burası.

---

## HandleClick()

Bir tıklamayı hücreye çevirir ve niyete göre dallandırır.

### AKIŞ DEĞİŞMEDİ, ÇEVİRİ TEK SAHİBE İNDİ

Bu metodun ilk üç adımı (kamera var mı, piksel → dünya, dünya → hücre) artık
[`TryReadPointerCell`](#tryreadpointercell)'in içinde. Kopyalanmış olsalardı bu
dosyadaki "bir Unity tipinin Core'un diline çevrildiği tek yer burasıdır" cümlesi
artık **yalan** olurdu — yerleştirme kipi aynı çeviriye ikinci bir çağıran
ekledi.

Dallanmanın kendisi (dolu hücre → saldırı, boş hücre → hareket, kendisi → seçimi
bırak) satır satır aynı kaldı; değişen tek şey çevirinin nereden geldiği.

**Dünya koordinatları burada kullanılmıyor:** tıklama akışının ihtiyacı olan tek
şey hücre indeksi. Jest eşiği ise dünya biriminde ölçüldüğü için yerleştirme
tarafı ikisini birden alır.

### Debug.Log'un ikinci parametresi

"context"tir: Console'da bu satıra tıklayınca Unity Hierarchy'de o nesneyi
vurgular. Metni değiştirmez. 17 nesneli bir sahnede "bunu kim yazdı?" sorusunu
tek tıkla cevaplar.

### Sınır sorusu

Kural `Battle`'da yaşar; adaptörün işi ona uymak, onu tekrar yazmak değil.
Buradaki soru artık tahtaya değil **savaşa** soruluyor ve `Battle` onu `UnitGrid`'e
devrediyor — kuralın metni hâlâ tek yerde.

---

## HandleOccupiedCellClick(Unit clicked, int x, int y)

Dolu hücreye tıklandı: seçim yoksa seç, seçili olan kendisiyse seçimi bırak,
başkasıysa **saldır**.

**Derin anlatım:** [07-tıklamadan eyleme](../../konular/07-tiklamadan-eyleme.md)

### HARİTA: bir tıklamanın ANLAMI nerede kararlaşıyor

Aynı `(x, y)` çifti üç ayrı niyet taşıyabilir; ayrımı yapan şey hücrenin
**içeriği** değil, hücrenin **seçimle olan ilişkisi**:

```
dolu hücreye tıklandı
├── seçim YOK           ──► SEÇ
├── tıklanan == seçili  ──► SEÇİMİ BIRAK        ◄── BU SATIR
│      >> AKIŞ BURADA BİTER << aşağıya hiç inilmez
└── tıklanan != seçili  ──► SALDIR

REDDEDILEN'de orta dal YOKTUR
├── seçim YOK           ──► SEÇ
└── geri kalan her şey  ──► BattleActions.Move
       MoveAction doluluk kontrolünde birimin KENDİSİNİ
       bilerek dışarıda bırakıyor  ──► çağrı KABUL edilir
       ◄── AYRIŞMA: kural "evet" diyor, oyuncu "bırak" demişti
```

### KAPSAM: kural bu TEK tıklamaya özeldir

"Kural kabul ediyorsa niyet de odur" çıkarımı yalnızca kabul ile niyetin
**ayrıştığı** hücrede bozulur; başka her yerde geçerlidir.

**KARŞI ÖRNEK** aynı dosyada,
[`HandleEmptyCellClick`](#handleemptycellclickint-x-int-y): orada **tam olarak
aynı** çağrı — `BattleActions.Move(battle, selectedUnit, x, y, moveRange)` —
hiçbir ön kontrol olmadan yapılır ve **doğrudur**, çünkü seçim varken boş bir
hücreye tıklamanın başka okuması yok. Aynı dosya, aynı çağrı, **zıt** karar; farkı
yaratan tek şey hedef hücrede **seçili birimin** durması.

### İŞ BÖLÜMÜ: NİYET ile GEÇERLİLİK ayrı sorular

```
buradaki ReferenceEquals  ► NİYET — "oyuncu ne demek istedi"
MoveAction'ın kontrolleri ► GEÇERLİLİK — "kural izin veriyor mu"
```

Buradaki dal silinirse geçerlilik "evet" der ve tıklama sessizce bir harekete
dönüşür. `MoveAction`'ın kontrolleri silinirse niyet doğru okunur ama tahta
bozulur. İkisi aynı soruyu iki kez sormuyor: geçerliliğin "evet" dediği yerde
niyetin "hayır" diyebilmesi tam olarak bu dalın var olma sebebi.

Karşılaştırma `ReferenceEquals` ile yapılıyor: `Unit` bir sınıftır ve aradığımız
şey zaten **tam o nesnenin** kendisidir — aynı gerekçe [`unitViews`](#unitviews)'ın
anahtar seçiminin üstünde de yazılı.

### REDDEDILEN

Kendi hücresine tıklamak da bir hareket denemesidir:

```csharp
ReactToMove(
    BattleActions.Move(battle, selectedUnit, x, y, moveRange),
    x, y);
```

**KIRILAN:** `MoveAction.Execute` bu çağrıyı **kabul eder** — doluluk kontrolü
birimin kendisini bilerek dışarıda bırakıyor.

```
oyuncu seçimi bırakmak için tıklar -> hiçbir şey olmaz
sıra sistemi bağlanır -> boş hareket tur bütçesini yer
derleyici: hiçbir şey der  .  test: hem hareket hem seçim "doğru"
                                    davrandığı için kırmızı olmaz
```

**KAZANIRDI:** "yerinde bekle" gerçek bir tur eylemi olduğu gün — nöbet tutmak,
siper almak; o gün seçimi bırakma işi başka bir girdiye (sağ tık, `Esc`) taşınır.

**TEK CUMLE:** Bir eylemin **kabul edilmesi**, o tıklamanın o eylem demek
olduğunu göstermez.

### Mesafe BURADA hesaplanmıyor

Ve hesaplatılmıyor bile: `BattleActions` konumları `Battle`'dan bulup
`GridDistance`'a ölçtürüyor. Saldırı satırının bildiği tek şey "kim kime".

---

## HandleEmptyCellClick(int x, int y)

Boş hücreye tıklandı: seçim varsa **hareket**, yoksa yalnızca bildir. Ön kontrol
yokluğunun gerekçesi
[`HandleOccupiedCellClick`](#handleoccupiedcellclickunit-clicked-int-x-int-y)'in
KAPSAM bölümünde: seçim varken boş bir hücreye tıklamanın başka okuması yok.

---

## ReactToAttack(AttackOutcome outcome, Unit target, int x, int y)

Saldırı sonucuna göre ekranı ve Console'u günceller.

**Derin anlatım:** [06-sonuç enum'ları](../../konular/06-sonuc-enumlari.md)

### SONUÇ BİR EVENT'LE GELMİYOR ve gelmemeli

Soran zaten burada. `UnitLifecycle`'in `StateChanged` olayının üstündeki ayrım
bunu tek cümleyle koyuyor — *"dönüş değeri: soran zaten orada; event: ilgilenen
başka yerde"*. Saldırıyı başlatan da sonucunu gösterecek olan da bu tip,
dolayısıyla araya bir dinleyici koymak yalnızca dolaylılık olurdu.

### GÖRSEL BU DALDAN TAZELENMİYOR ARTIK

Durum değişikliğinin **tek** tetiği `Battle.UnitStateChanged` oldu; saldırı da bir
durum değişikliği ürettiği için olay zaten yolda.

#### HARİTA: "yatık/gri" olgusuna kaç yol var

```
SEÇİLEN — tek yol
  Combatant.State değişti
    ──► Battle.UnitStateChanged
    ──► OnUnitStateChanged ──► ApplyStateVisual
  >> TEK KAYNAK << saldırıdan gelen de, Tick'ten gelen de
  AYNI borudan geçer

REDDEDILEN — iki yol
  Combatant.State değişti ─┬─► olay   ──► ApplyStateVisual
                           └─► bu dal ──► RefreshDowned...
  İki ok AYNI piksele yazıyor ve AYNI cevabı veriyor;
  fazlalık bu yüzden zararsız GÖRÜNÜR.
  ◄── KIRILMA: olay yolu bir gün susarsa saldırıyla düşen
      birim yine DOĞRU görünür; belirti yalnız Tick kaynaklı
      geçişte çıkar, yani hatanın YARISI örtülür
```

### KAPSAM: kural DURUM tazelemeye özeldir

Yasak olan şey bir sonuç dalından ekranı güncellemek **değil**; bir olayın
**zaten** yayınladığı olguyu ikinci kez yazmak.

**KARŞI ÖRNEK** aynı dosyada,
[`ReactToMove`](#reacttomovemoveoutcome-outcome-unit-unit-int-x-int-y)'un `Moved`
dalı: `MoveViewTo` tam olarak bir sonuç dalından çağrılır ve **doğrudur**, çünkü
**konumu** yayınlayan bir olay yok — o dal ikinci kaynak değil, **tek** kaynak.
İki dalı ayıran şey ekranın ne gösterdiği değil, olgunun başka bir yayıncısı olup
olmadığı.

### İŞ BÖLÜMÜ: durum olayla, konum dönüş değeriyle

```
Battle.UnitStateChanged ► DURUM — yatıklık ve renk; tıklama olmadan
                          da değişebildiği için OLAY
MoveOutcome dönüşü      ► KONUM — yalnızca bir emirle değişir,
                          soran zaten burada
```

Olay silinirse `Tick` kaynaklı `Downed → Dead` geçişi ekrana hiç ulaşmaz —
aboneliğin kapattığı hatanın ta kendisi. `MoveViewTo` silinirse birim tahtada
yürür, ekranda yerinde kalır. Bölüşüm **olgunun kaynağına** göre yapılmış:
kendiliğinden değişen olgu olayla, emirle değişen olgu dönüş değeriyle taşınır.

### REDDEDILEN

Olay bağlandıktan sonra elle tazeleme de bu dalda **kalır**:

```csharp
RefreshDownedVisual(target);
```

**KIRILAN:** ekranın aynı olguya **iki** kaynağı olur; ikisi aynı cevabı verdiği
için hata **sessizdir**.

```
olay zinciri kırılırsa -> saldırıyla düşen birim yine DOĞRU görünür,
tek belirti Tick kaynaklı geçişin ekrana hiç ulaşmaması olur —
aboneliğin kapattığı hata
derleyici: hiçbir şey der  .  test: adaptör sınanamaz
```

**KAZANIRDI:** olay "durum değişti"yi değil "bir şey oldu"yu taşısaydı — yalnız
`Tick` kaynaklı geçişler yayınlansaydı bu satır tekrar değil **tek** yol olurdu
(S-07'nin ayrımı).

**TEK CUMLE:** İki kaynak aynı cevabı verdiği sürece fazlalık görünür; biri
sustuğunda diğeri hatayı **örter**.

### MESAJ HEDEFİ DEĞİL SALDIRANI ANLATIYOR

`RejectedActorCannotAct` dalında bu, değerin adındaki **"Actor"** sözcüğünün
doğrudan karşılığı: ret sebebi saldıranın kendi durumu (düşmüş) ya da sırası.
Hedefi değiştirmek üçünde de yardım etmez, bu yüzden log satırı oyuncuyu hedefe
bakmaya davet etmemeli.

**TEK MESAJ, ÜÇ SEBEP** — ve bu bir tavizdir, sözleşmede öyle yazılı: "sırası
değil" ile "birim düşmüş" bugün ayrılmıyor. **EŞİK** aynı yerde duruyor: arayüz
oyuncuya ikisi arasındaki farkı **söylemek** zorunda kaldığı gün değer ikiye
ayrılır. Bugün tek tüketici burası ve burası yalnızca log basıyor.

### `default` LOG DEĞİL LogError

Buraya düşmek "`AttackOutcome`'a yeni bir değer eklendi ve bu switch
güncellenmedi" demektir, yani bir **programcı** hatasıdır. `AttackOutcome`'un
struct REDDEDILEN bloğu şunu diyor: *"switch'te eksik dal derleyiciden görünmez
olur; enum'da görünür"* — ama bir switch **deyimi** için derleyici uyarmaz, o
yüzden görünürlüğü bu dal sağlıyor.

---

## ReactToMove(MoveOutcome outcome, Unit unit, int x, int y)

Hareket sonucuna göre ekranı ve Console'u günceller.

**Derin anlatım:** [06-sonuç enum'ları](../../konular/06-sonuc-enumlari.md)

### `Moved` dalı: görsel tahtayı TAKİP eder, tahtaya yön vermez

Hareketi `MoveAction` çoktan yaptı; buradaki satır yalnızca sonucu gösteriyor.
Ters sırada yazılsaydı (önce görseli taşı, sonra kuralı sor) reddedilen bir
hareket ekranda gerçekleşmiş görünürdü.

### İKİ DAL BUGÜN ULAŞILAMAZ ve yine de yazılı

`RejectedCellOccupied` ve `RejectedInvalidDestination`: boş hücre olduğu
`HandleEmptyCellClick`'te doğrulandı, tahta içi olduğu `HandleClick`'te
doğrulandı. Ama iki kuralın da sahibi bu tip değil: `MoveAction` sırasını
değiştirebilir, `Battle` sınır sorusunu farklı cevaplayabilir. **Yazılı bir dal
bedavadır; sessizce düşen bir dal, Console'da hiç görünmeyen bir hatadır.**

### `RejectedActorCannotAct` — ikizi AttackOutcome'da, aynı adla

İki enum'da aynı adın taşıdığı **üretilebilirlik** farkı bu dosyadan görünmez: bu
değeri `MoveAction` **asla** üretemez (ne `UnitState`'i ne sırayı görür), yalnız
`BattleActions` üretir. Çağıran açısından fark yok, ve ret sebebinin tek işi
çağıranın **yapabileceği** şeyi göstermek olduğu için de olmaması doğru.

---

## ApplyStateVisual(Unit unit, UnitState state)

Bir birimin yaşam durumunu ekrana uygular.

### ADI DEĞİŞTİ

Eski ad `RefreshDownedVisual`, sahibinin cevaplayamayacağı bir soruya cevap
veriyordu. Metot artık "düşme"yi değil **üç durumu** birden uyguluyor; `Downed`
adı, üç değerli bir bilgiyi tek değerli gösteriyordu.

### HARİTA: görsel hangi zincirden besleniyor

```
SEÇİLEN — DURUM zinciri
  Combatant.State ──(olayın `to` parametresi)──►
    ApplyStateVisual(unit, state) ──► view.SetState(state)
  >> TEK DOĞRULUK KAYNAĞI <<  üç değer, üç görsel

REDDEDILEN — SONUÇ zinciri
  AttackOutcome ──► view.SetDowned(outcome == HitAndDowned)
    enum'un taşıdığı bilgi : "az önce NE OLDU"
    ekranın istediği bilgi : "şu an NE DURUMDA"
  ◄── AYRIŞMA NOKTASI: düşmüşe tekrar vurmak Hit döndürür,
      ifade false olur ve düşmüş birim AYAĞA KALKMIŞ görünür
```

### DURUM ARTIK PARAMETRE

Çünkü çağıran onu **zaten** taşıyor: olay `Action<Unit, UnitState, UnitState>`,
yani "nereye"yi elinde tutuyor. `battle.TryGetCombatant` ile tekrar sormak aynı
bilgiyi ikinci kez aramak olurdu — ve iki okuma arasında geçen tek bir `Tick`,
ekrana olayın taşıdığından **farklı** bir durum yazdırabilirdi.

**Görsel, sonuç enum'undan değil durumdan okunuyor** — ve fark önemli:
`AttackOutcome.HitAndDowned` yalnızca "şimdi sormaya değer" der, ne gösterileceğini
söylemez. Tek doğruluk kaynağı `Combatant.State`.

### KAPSAM: kural DURUM gösteren tüketiciye özeldir

Sonuç enum'unu okumak yasak değil; yasak olan ondan bir **durum** türetmek. Bir
**olayı** anlatan tüketici için sonuç doğru kaynaktır.

**KARŞI ÖRNEK** aynı dosyada, `ReactToAttack`'in `Debug.Log` satırları: hepsi
doğrudan `AttackOutcome`'dan dallanır ve **doğrudur**, çünkü bir log satırı "az
önce ne oldu"yu anlatır — tam olarak enum'un taşıdığı şeyi. Aynı enum, aynı
metot, iki farklı tüketici: biri olayı anlatıyor, öteki durumu gösteriyor.

### İŞ BÖLÜMÜ: durumu öğrenmenin iki yolu, iki ayrı soru

```
olayın `to` parametresi ► "DEĞİŞTİĞİ ANDAKİ durum" — bu metot
battle.TryGetCombatant  ► "ŞU ANKİ durum" — DescribeCondition
```

Parametre kaldırılıp burada `TryGetCombatant` sorulsaydı araya giren tek bir
`Tick` ekrana olayın taşıdığından farklı bir durum yazdırabilirdi.
`TryGetCombatant` kaldırılsaydı log satırları birimin canını ve durumunu hiç
yazamazdı. İkisi aynı bilgiyi iki kez okumuyor: biri geçmişin bir **anını**, öteki
**şimdiyi** soruyor.

### GARANTİ NEREDE BİTİYOR

Parametrenin tazeliği, bu dinleyicinin olayın **içinde** eşzamanlı koşmasına
bağlı. Görsel güncelleme bir gün ertelenirse (coroutine, animasyon kuyruğu, bir
kare sonrası) `state` bir **geçmiş** anın fotoğrafına döner ve onu taze tutan
hiçbir şey kalmaz.

### REDDEDILEN

Görsel doğrudan sonuçtan türetilir ve bu metot hiç doğmaz:

```csharp
view.SetDowned(outcome == AttackOutcome.HitAndDowned);
```

**KIRILAN:** ekran ile savaş kaydı **sessizce** ayrışır.

```
diriltme geldiği gün -> ayağa kalkan birim ters durmaya devam eder,
                        çünkü hiçbir AttackOutcome "kalktı" demez
düşmüşe tekrar vurmak -> AttackAction Hit döner, ifade false olur ve
                         düşmüş birim ayağa kalkmış görünür
derleyici: hiçbir şey der  .  test: adaptör sınanamaz
```

**KAZANIRDI:** görsel bir **durumu** değil bir **olayı** gösterseydi — düşme
animasyonu, kan sıçraması; onlar bir kez oynanır ve tazelenecek bir durumları
yoktur.

**TEK CUMLE:** Sonuç enum'u "şimdi sormaya değer" der, "ne gösterileceğini"
yalnızca durumun kendisi söyler.

### ÇEVİRİ ARTIK YOK — ve yokluğu bir kazanç

Burada bir zamanlar `combatant.State != UnitState.Alive` yazıyordu: üç değerli bir
bilgi, bu satırda iki değere iniyordu ve `Downed` ile `Dead` ekranda aynı
görünüyordu. `UnitView`'ın parametresi `UnitState` olunca daraltma ortadan
kalktı; adaptör durumu **olduğu gibi** geçiriyor ve "üç durum nasıl görünür"
sorusunun tek sahibi `UnitView` oldu.

---

## DespawnView(Unit unit)

Savaştan çıkarılmış bir birimin görselini sahneden siler.

### SEÇİM ÖNCE BIRAKILIR, ama ClearSelection ile DEĞİL

O metot görsele `SetSelected(false)` der ve birazdan yok edilecek bir nesneye
çerçeve kapatmak anlamsız bir iştir. Daha önemlisi **sıra**: tablodan silindikten
sonra `ClearSelection` çağrılsaydı `SetSelectionVisual` görseli bulamayıp
`LogError` yazardı — var olmayan bir hata için kırmızı satır.

### Önce tablodan çıkar, sonra sahneden sil

Ters sırada da çalışırdı ama tabloda **yok edilmiş** bir görsel referansı kalırdı
ve Unity'nin aşırı yüklenmiş eşitliği yüzünden o referans "null gibi ama null
değil" bir hâlde dolaşırdı.

---

## SelectUnit(Unit unit)

Verilen birimi seçili yapar ve öncekinin seçimini kaldırır.

**Önce eskiyi temizle:** iki birim aynı anda seçili görünemez. `ClearSelection`
satırı olmasaydı her tıklama bir birimin daha çerçevesini açar ve hiçbiri geri
kapanmazdı.

`ClearSelection` seçim yoksa hiçbir şey yapmaz; erken çıkışın sebebi budur.

---

## SetSelectionVisual(Unit unit, bool isSelected)

Bir birimin görseline seçim durumunu iletir.

Eski `ApplyTint` burada `SpriteRenderer`'ı bulup `color`'ını yazıyordu. O
yaklaşımın kusuru şuydu: renk **çarpma** ile uygulandığı için seçim, birimin
kendi rengini/faction'ını bozuyordu. Artık birimin kendi `SpriteRenderer`'ına
**hiç** dokunulmuyor — `color`'ı `Color.white` kalıyor — ve seçim ayrı bir çerçeve
nesnesinde yaşıyor.

Adaptör o çerçeveyi **görmüyor bile**; sadece niyeti söylüyor.

---

## TryGetView(Unit unit, out UnitView view)

Birimin görselini verir; yoksa gürültüyle şikâyet eder.

Dört çağıranın (seçim, hareket, durum, temizlik) aynı hata mesajını kopyalamaması
için var. **Tabloda olmamak bir oyun olgusu değil bir programcı hatasıdır:**
savaşa giren her birim `SpawnUnit`'ten geçmeli ve tabloya kaydolmalıydı. Bu
yüzden sessiz `false` değil, `LogError` + `false`.

`DescribeCondition` ise bir birimin can ve durum özetini log satırı için
hazırlar; savaşta olmayan bir birim için `"(not in this battle)"` döner.
