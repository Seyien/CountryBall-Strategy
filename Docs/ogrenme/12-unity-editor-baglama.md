# Unity editörü — yeni katmanı bağlama

> **Ne yapar:** Yapı türü, üretim ve panel katmanının Unity penceresinde nasıl
> kurulacağını adım adım yazar. Varlık dosyaları, sahne hiyerarşisi, prefab
> kurulumu, bileşen bağlantıları, ve her `[SerializeField]` alanı için
> ***boş bırakılırsa ne olur***.
>
> **Ne YAPMAZ:** Mekanizmayı anlatmaz. *"`ScriptableObject` nedir"* sorusunun
> cevabı [`02-sonraki-asamalar.md`](02-sonraki-asamalar.md) Aşama 1'de,
> *"neden `noEngineReferences` duvarının dışında"* sorusununki orada.
> Mevcut sahnenin onarımını da yazmaz — o
> [`11-unity-penceresi-adim-adim.md`](11-unity-penceresi-adim-adim.md)'in işi ve
> ***önce o okunur.***

**Sırası:** `11` bugünkü kırık sahneyi onarır, `12` (bu dosya) yeni katmanı
kurar. İkisi arasında Play'e basılır: `11` bittiğinde tahta görünür ve `B` tuşu
hayaleti açar, `12` bittiğinde sol panelde yapılar, sağ panelde üretilen
askerler çıkar.

***Bu dosya bir ölçüye dayanıyor:*** serileştirilen bir alan bir sözleşmedir ve
ikinci tarafı kodda değil editördedir. Boş kalan alan üç kademede zarar verir:

```
SESLI          konsola bir sey basar         operator anlar
SESSIZ-OLU     hicbir sey olmaz              "bozuk mu?" dedirtir
SESSIZ-YANLIS  calisir ama YANLIS            en pahalisi
```

---

## BÖLÜM A · Varlıklar ve prefablar — diskte ne var, nasıl içe aktarılır

Tarih: 2026-08-25 · Unity 2021.3.45f2 · Ölçümler o gün diske karşı alındı.
Her adımın sonunda **Doğrula:** satırı vardır — o satırı görmeden sonraki adıma geçme.

### 0 · Bu oturumda diske eklenen YENİ dosyalar

Hiçbir mevcut dosya değiştirilmedi. Eklenenler:

| Dosya | Ne |
|---|---|
| `Assets/Art/Derived/Kenney/TinyBattle/Buildings/enemy_command_depot_from_tile_0045.png` | Düşman kışla sprite'ı (palet takası türevi) |
| `Assets/Art/Derived/Kenney/TinyBattle/Buildings/enemy_industrial_pump_from_tile_0048.png` | Düşman enerji yapısı sprite'ı (palet takası türevi) |
| `Assets/Art/Derived/DERIVED-ASSETS.md` | Türev köken/lisans kaydı |
| `Assets/Art/Generated/ui_white_square_4x4.png` | Boyanabilir beyaz dolgu (sağlık/ilerleme çubuğu, panel zemini) |
| `Assets/Art/Generated/ui_cell_frame_16x16.png` | 1 px beyaz çerçeve, içi şeffaf (hücre vurgusu, menzil) |
| `Assets/Art/Generated/GENERATED-ASSETS.md` | Üretilmiş primitif kaydı |
| ~~`Assets/Game/Prefabs/Structure.prefab`~~ | **2026-08-28'de SİLİNDİ** — guid'ine hiçbir sahne/varlık atıf yapmıyordu, yapı görselini `BoardAdapter.CreateStructureVisual` kodla kuruyor. Gerekçe ve geri gelme tetikleyicisi `Assets/Editor/SceneSetupTool.cs` başında yazılı. |
| `Assets/Game/Prefabs/PlacementGhost.prefab` | Yerleştirme hayaleti prefabı (elle yazıldı) |

`.meta` dosyası kasıtlı olarak YAZILMADI — Unity ilk odak değişiminde
(pencereye tıklayınca) hepsini kendisi üretir.

**Doğrula:** Unity penceresine odaklan, içe aktarma çubuğu aksın.
Project penceresinde `Assets/Art/Derived`, `Assets/Art/Generated` klasörleri
ve `Assets/Game/Prefabs` altında `Structure`, `PlacementGhost` görünür.
Console'da kırmızı hata OLMAMALI. (Sarı "asset database" uyarıları zararsızdır.)

### 1 · Yeni PNG'lerin import ayarları — projenin ölçülmüş sözleşmesi

Mevcut 17 PNG'nin `.meta` dosyalarından ölçüldü (ör. `friendly_command_depot_tile_0045.png.meta`):

| Ayar | Değer | Meta'daki karşılığı |
|---|---|---|
| Texture Type | Sprite (2D and UI) | `textureType: 8` |
| Sprite Mode | Single | `spriteMode: 1` |
| Pixels Per Unit | **16** | `spritePixelsToUnits: 16` |
| Filter Mode | **Point (no filter)** | `filterMode: 0` |
| Compression | **None** | `textureCompression: 0` |
| Generate Mip Maps | Kapalı | `enableMipMap: 0` |
| Alpha Is Transparency | Açık | `alphaIsTransparency: 1` |
| Wrap Mode | Clamp | `wrapU: 1, wrapV: 1` |
| Pivot | Center | `spritePivot: 0.5, 0.5` |

Unity yeni PNG'yi VARSAYILAN ayarlarla açar (PPU 100, Bilinear, Compressed) —
dördü de yanlış. Dört yeni PNG'nin HER BİRİ için:

1. Project penceresinde PNG'yi seç.
2. Inspector'da tabloya göre ayarla. İpucu: dört dosyayı Ctrl ile birden seçip tek seferde ayarlayabilirsin.
3. Sağ altta **Apply**.

İstisna: `ui_white_square_4x4.png` için PPU **4** yaz (1 dünya birimi = 1 kare
kaplasın diye; 16 bırakırsan sahnede 4 kat küçük görünür, ölçekle telafi edilebilir).

**Doğrula:** PNG seçiliyken Inspector'ın altındaki önizleme keskin (bulanık değil)
görünür. `enemy_command_depot...` önizlemesi kırmızı tonlu binadır; mavi görünüyorsa
yanlış dosyaya bakıyorsun. Inspector üstünde "16x16 ... RGBA32" gibi
sıkıştırmasız bir biçim yazar.

### 2 · Elle yazılan prefablar — neden yazıldı ve nasıl doğrulanır

#### Karar ve risk ölçümü

`Unit.prefab` okundu: 2 GameObject (kök + SelectionOverlay çocuğu), Transform +
SpriteRenderer bileşenleri, tek MonoBehaviour (UnitView). Elle yazma riski üç
kaynaktan gelir ve üçü de burada ölçülüp kapatıldı:

1. **fileID çakışması/kopukluğu** → her iki yeni prefab, tüm yerel referansları
   çözen bir yapısal doğrulayıcıdan geçirildi. Doğrulayıcının kendisi
   bilinen-iyi (Unity'nin yazdığı `Unit.prefab` → GEÇTİ) ve bilinen-kötü
   (kırık referans, bilinmeyen guid, çift anchor → ÜÇÜ DE YAKALANDI) girdiyle sınandı.
2. **guid uydurma** → yeni prefablar YALNIZ mevcut, `.meta`'sından okunmuş
   guid'lere referans verir (depot sprite `dbeaef2f…`, çerçeve sprite `77def7…`,
   yerleşik Sprites-Default malzemesi). Henüz guid'i olmayan yeni PNG'lere
   prefab içinden referans BİLEREK verilmedi — o bağlama Unity içinde yapılır (Adım 4).
3. **şema kayması** → içerik `Unit.prefab`'ın alan alan kopyasıdır; yalnız adlar,
   fileID'ler, sprite guid'i, sortingOrder ve renk değişti. Aynı 2021.3 şeması.

UI (Canvas/RectTransform) prefabları elle YAZILMADI: bileşen sayısı üç kat,
yerleşik kaynak referansları belgesiz, ve panel işi panel kodunun sahipliğiyle
çakışıyor. Onlar için Adım 5'teki Unity-içi prosedür geçerli.

#### `Structure.prefab` içeriği — TARİHSEL KAYIT, DOSYA ARTIK YOK

> Aşağıdaki tarif 2026-08-28'e kadar geçerliydi. Dosya o gün silindi: hiçbir
> yerden okunmuyordu ve taşıdığı üç sayı (kök `sortingOrder 2`, çocuk
> `sortingOrder 1`, `m_LocalScale 1.25`) bugünkü çizim merdiveniyle ve
> `BoardSizing` hesabıyla ÇELİŞİYORDU. Bölüm, elle YAML yazımının neye
> benzediğini gösteren bir örnek olarak duruyor.

- Kök `Structure`: SpriteRenderer — sprite: `friendly_command_depot`,
  sortingOrder **2**, renk beyaz. Bilerek MonoBehaviour YOK: `StructureView`
  diye bir script henüz yok (kod tarafının işi); kod `GetComponent<SpriteRenderer>()`
  ile sprite'ı tip başına değiştirebilir.
- Çocuk `SelectionOverlay`: SpriteRenderer — sprite: `selection_unit_bracket`,
  sortingOrder **1**, ölçek 1.25, renk sarı (Unit ile aynı değerler),
  **m_Enabled: 0**. Unit'te bu çizici açık durur çünkü UnitView.Awake kapatır;
  Structure'da kapatan script olmadığından prefabda kapalı yazıldı.

#### `PlacementGhost.prefab` içeriği

Tek GameObject: SpriteRenderer — sprite: depot, renk (1, 1, 1, **0.5**)
yarı saydam, sortingOrder **3** (hayalet her şeyin üstünde görünsün),
**m_Enabled: 0** (BoardAdapter'ın Inspector başlığındaki "kept disabled at rest"
sözleşmesine uygun; Awake zaten kapatır ama prefab da kapalı başlar).

#### Unity içinde doğrulama (zorunlu, sırayla)

1. Project → `Assets/Game/Prefabs/Structure` seç.
   **Doğrula:** Önizleme alanında mavi bina görünür. Görünmüyorsa/beyaz kare
   görünüyorsa prefab bozuk demektir — Console'a bak, hatayı raporla.
2. `Structure` dosyasına çift tıkla (Prefab Mode açılır).
   **Doğrula:** Hierarchy'de `Structure > SelectionOverlay` ağacı; kökte
   Sprite Renderer (Order in Layer = 2); çocukta Sprite Renderer kutusu
   İŞARETSİZ (disabled) ve Order in Layer = 1. Console temiz.
3. `PlacementGhost` dosyasına çift tıkla.
   **Doğrula:** Tek nesne; Sprite Renderer işaretsiz; Color alfası 128/0.5;
   Order in Layer = 3.
4. Sahneye test: `Structure` prefabını Hierarchy'ye sürükle, Scene'de mavi bina
   görünür, sonra Ctrl+Z ile geri al (sahne bu turun kapsamı dışındaydı, kirletme).

### 3 · Prefab mı sahne nesnesi mi — ölçülmüş kararlar

| Aday | Karar | Ölçülen gerekçe |
|---|---|---|
| Yapı görseli | **KOD** (karar 2026-08-28'de TERSİNE DÖNDÜ; prefab silindi) | Çok kopya + çalışma zamanında doğar. Bugünkü kod (`BoardAdapter.CreateStructureVisual`) görseli kodla kuruyor ve sprite'ı hayaletten ödünç alıyor; hedef akışta iki FARKLI yapı tipi ve seçilebilir yapı var — kodla kurulum her tip için satır ekletir, prefab tek Instantiate. **BU TAHMİN ÖLÇÜLDÜ VE TUTMADI:** bugün on yapı türü var ve `CreateStructureVisual`'da tür başına tek satır bile yok — tür kimliği `.asset` dosyalarında, boyut `BoardSizeInCells`'te, sprite palette yaşıyor. Prefab okunmadan durdu ve silindi. |
| Yerleştirme hayaleti | **PREFAB** (`PlacementGhost.prefab`), sahneye BİR kopya | Tek kopya ama sahne dosyası o turun yazma alanı dışındaydı; prefab yapıp sürüklemek operatöre tek adım bırakır. BoardAdapter alanı `[SerializeField] SpriteRenderer placementGhost` sahnede BOŞ (ölçüldü: sahne YAML'ında alan hiç serileşmemiş) ve Awake bunu LogError ile söylüyor. |
| Seçim çerçevesi | **AYRI PREFAB DEĞİL** | Zaten `Unit.prefab` içinde çocuk (yapı tarafında görsel kodla kuruluyor). Ayrı prefab, "hangi nesnenin çocuğu" sorusunu her kullanıcıya yeniden sordurur; UnitView sözleşmesi (Inspector'dan çocuk referansı) mevcut deseni kilitlemiş. |
| UI panelleri | **SAHNE NESNESİ (şimdilik)** | Tek Canvas, tek kopya, tek sahne. Prefab'ın getirisi (çok sahnede yeniden kullanım) bu projede yok; maliyeti (elle yazılamayan karmaşık YAML + panel koduyla çakışma) ölçülür biçimde yüksek. Liste ELEMANI (yapı butonu, asker butonu) ise çok kopyalıdır — istenirse Adım 5 sonunda butonu prefaba çevirmek tek sürüklemedir. |
| Sağlık çubuğu | **ERTELENDİ** — sprite hazır, prefab kararı kod tarafının | Çubuğu kim doğuracak (UnitView mi, ayrı bir HealthBarView mi) kod sahipliği sorusu. Görsel taraf hazır: `ui_white_square_4x4` dolgu + zemin, iki SpriteRenderer, tint kodla. |

### 4 · Yeni sprite'ların prefablara bağlanması (Unity içinde, isteğe bağlı)

Düşman yapı prefabı gerektiğinde elle YAML yazmak yerine Unity'ye yazdır:

> **BU ADIM ARTIK UYGULANAMAZ.** `Structure.prefab` 2026-08-28'de silindi ve
> düşman yapıları prefab çoğaltmayla değil, kendi `StructureBlueprintAsset`
> dosyalarıyla ayrışıyor: sprite, can, hasar, menzil, üretim listesi ve tahtada
> kapladığı hücre sayısı orada yaşıyor. Yeni bir düşman yapısı eklemenin yolu
> `SceneSetupTool.EnsureBlueprints` içine bir satır yazıp menüyü çalıştırmaktır;
> Unity içinde elle prefab çoğaltmak değil.

### 5 · UI panelleri — Unity içi kurulum (görsel taraf)

panel davranış koduna dokunmadan görsel iskelet şöyle kurulur:

1. Hierarchy → sağ tık → UI → Canvas. (EventSystem otomatik gelir.)
   **Doğrula:** Hierarchy'de `Canvas` ve `EventSystem` belirir.
2. Canvas seçiliyken Canvas Scaler bileşeninde
   UI Scale Mode = **Scale With Screen Size**, Reference Resolution 1920x1080.
   **Doğrula:** Game penceresi boyut değiştirince paneller oranını korur.
3. Sol panel: Canvas'a sağ tık → UI → Image, adı `BuildPanel`.
   Rect: sola çapalı (Anchor preset: sol kenar, dikey stretch), genişlik ~200.
   Image'ın Source Image alanına `ui_white_square_4x4`, Color koyu gri + alfa ~200.
   **Doğrula:** Game penceresinin sol şeridi yarı saydam koyu panel.
4. `BuildPanel` altına Vertical Layout Group ekle (üst: Player yapıları, alt: Enemy).
   Her yapı için UI → Button - TextMeshPro; butonun Image'ına yapı sprite'ı.
   İlk TMP nesnesinde Unity "Import TMP Essentials" penceresi açar — **Import** de.
   **Doğrula:** `Assets/TextMeshPro/` klasörü oluşur; buton üstünde LiberationSans
   ile metin görünür. (Projede font dosyası sıfırdı; bu adım o açığı kapatır.)
5. Sağ panel aynı desenle `ProductionPanel`; asker butonlarının Image'ına
   `friendly_vanguard_infantry` vb. Varsayılan seçili görünüm için Button'ın
   Transition = Color Tint, Selected Color'ı sarıya çek.
   **Doğrula:** Play'de butona tıklayınca sarı kalır (seçili), boşluğa tıklayınca döner.
6. Piksel sprite'ları UI'da bulanıksa: sprite'ın Filter Mode'unun Point olduğunu
   (Adım 1) yeniden kontrol et; sorun Canvas'ta değil sprite importunda çözülür.

### 6 · PlacementGhost'un sahneye bağlanması (operatör, 2 dakika)

1. `SampleScene` açıkken Project'ten `PlacementGhost.prefab` dosyasını Hierarchy'deki
   `Board` nesnesinin ÜSTÜNE sürükle (çocuğu olsun).
   **Doğrula:** Hierarchy'de `Board > PlacementGhost`; Scene'de görünmez (çizici kapalı) — bu DOĞRU.
2. `Board` nesnesini seç; Inspector'da Board Adapter'ın **Placement Ghost** alanına
   Hierarchy'deki `PlacementGhost` nesnesini sürükle.
   **Doğrula:** Alanda "PlacementGhost (Sprite Renderer)" yazar.
3. Play → bir birim seç → **B** tuşu.
   **Doğrula:** Yarı saydam bina fareyi izler; Escape iptal eder. Bağlamadan
   önce Play'de Console'da `[Board] placementGhost is not assigned...` hatası
   görünüyordu — artık GÖRÜNMEZ. (Bu hata, bağlantının yapılmadığının kasıtlı alarmıydı.)

### 7 · O turda açık kalanlar — ve bugünkü durumları

- ***KAPANDI*** — yapı/birim üretim verisi (`StructureBlueprintAsset`,
  `UnitBlueprintAsset`) ve üretim kuralları aynı gün yazıldı. Bölüm B onların
  editör tarafını anlatıyor.
- ***KAPANDI*** — panel davranış kodu (`StructurePaletteView`,
  `ProductionPanelView`, `PaletteEntryView`, `ProductionDirector`) aynı gün
  yazıldı. Bölüm B onları bağlıyor.
- Sahne düzenlemeleri (ghost bağlama, Canvas) — operatör; prosedürü yukarıda.
- ***AÇIK*** — dost ateşi göstergesi gibi kural-görselleri. Kuralın kendisi hazır
  (`TargetingRules.IsHostilePairing`), eksik olan onu ekranda gösteren şey.

---

## BÖLÜM B · Sahne, panel ve bağlantılar

Bu belge **operatörün Unity editöründe yapacağı işi** anlatır. Kod tarafı bitti;
burada anlatılan hiçbir adım kod yazmayı gerektirmiyor.

Unity sürümü ölçüldü: **2021.3.45f2**. `com.unity.ugui` **1.0.0** manifest'te var,
yani `UnityEngine.UI` (`Canvas`, `Image`, `Text`, `Button`) ve
`UnityEngine.EventSystems` kullanılabilir. TextMeshPro **kullanılmadı** — sebebi
aşağıda, "Neden eski `Text`" başlığında.

---

### 0 · Önce bilinmesi gereken tek olgu

> **Serileştirilmiş bir alan YAML'da yoksa `default(T)` almaz, C# BAŞLATICISININ
> değerini alır.**

Bu, bu depoda ölçüldü. Pratik sonucu şu: aşağıdaki tablodaki **sayı** alanları
boş bırakılamaz — her zaman bir değerleri vardır ve o değer koddaki başlatıcıdır.
Asıl tehlike **referans** alanlarındadır: onların başlatıcısı yoktur ve `null`
doğarlar. Tablodaki `SESLİ`/`SESSİZ-*` kademeleri tam olarak bu ayrımı ölçüyor.

Kademeler:

```
SESLİ           konsola bir şey basar
SESSİZ-ÖLÜ      hiçbir şey olmaz
SESSİZ-YANLIŞ   çalışır ama YANLIŞ   <- en pahalısı
```

---

### 1 · Varlık dosyaları (önce bunlar, sahne sonra)

Panel kurmadan önce yapı ve birim türlerini üretin. İkisi de `Project`
penceresinden sağ tık → `Create` menüsünden çıkar.

#### 1a · Birim türleri

`Create ▸ GridStrategy ▸ Unit Blueprint`

Örnek iki dosya (klasör önerisi `Assets/Game/Data/Units/`):

| dosya | displayName | maxHealth | damage | attackRange |
|---|---|---|---|---|
| `Rifleman.asset` | Rifleman | 30 | 10 | 1 |
| `Sniper.asset` | Sniper | 20 | 18 | 3 |

#### 1b · Yapı türleri

`Create ▸ GridStrategy ▸ Structure Blueprint`

Örnek iki dosya (klasör önerisi `Assets/Game/Data/Structures/`):

| dosya | displayName | maxHealth | damage | attackRange | produces | defaultProducedIndex | productionSeconds |
|---|---|---|---|---|---|---|---|
| `Barrack.asset` | Barrack | 50 | 0 | **0** | `Rifleman`, `Sniper` | 0 | 3 |
| `PowerPlant.asset` | Power Plant | 80 | 0 | **0** | *(boş)* | 0 | 0 |

> **`attackRange = 0` "saldırmaz" demektir.** Bu bir eksiklik değil, kararın
> kendisi: `Structure` tipinin saldırı profili zaten isteğe bağlı ve yapıların
> çoğu saldırmıyor. Bir yapının saldırmasını istiyorsanız menzili **en az 1**
> yapın; aksi hâlde `damage` alanı hiç okunmaz.

> **`produces` boş bırakmak GEÇERLİDİR.** Elektrik santrali gerçek bir yapıdır
> ve hiçbir şey üretmez; sağ panel o zaman boş etiketi gösterir.

**Yapı TÜRÜ ayrımı kodda bir `enum` DEĞİL, bu iki dosyanın kendisidir.** Üçüncü
bir tür istediğinizde üçüncü bir `.asset` yaratırsınız; hiçbir `.cs` dosyası
değişmez.

---

### 2 · Sahne hiyerarşisi

```
Board                       (var olan tahta nesnesi — BoardAdapter burada)
└── ...

EventSystem                 >> YOKSA HİÇBİR TIKLAMA VE SÜRÜKLEME ÇALIŞMAZ <<
                            GameObject ▸ UI ▸ Event System
                            (bir Canvas eklendiğinde Unity bunu genelde
                             kendiliğinden yaratır — yine de DOĞRULAYIN)

GameDirector                (boş GameObject)
└── ProductionDirector      betik

UICanvas                    Canvas
                              Render Mode : Screen Space - Overlay
                            + CanvasScaler
                              UI Scale Mode : Scale With Screen Size
                            + GraphicRaycaster   >> SÜRÜKLEME BUNA BAĞLI <<
│
├── LeftPanel               Image (arka plan)  + VerticalLayoutGroup
│   │                       StructurePaletteView betik
│   ├── PlayerRow           HorizontalLayoutGroup
│   └── EnemyRow            HorizontalLayoutGroup
│
└── RightPanel              Image (arka plan)  + VerticalLayoutGroup
    │                       ProductionPanelView betik
    ├── ProducedRow         VerticalLayoutGroup
    └── EmptyLabel          Text  ("bu yapı üretmiyor")
```

`LeftPanel`'i ekranın soluna, `RightPanel`'i sağına yerleştirin (RectTransform
anchor'ları). `PlayerRow` **üstte**, `EnemyRow` **altta** — bu sıra
`VerticalLayoutGroup`'un çocuk sırasıdır, koddan gelmez.

---

### 3 · Düğme prefab'ı (`PaletteEntry`)

İki panel de **aynı** prefab'ı kullanır.

```
PaletteEntry                RectTransform
                            Image          >> Raycast Target AÇIK <<
                            PaletteEntryView betik
├── Icon                    Image           (Raycast Target KAPALI)
├── Label                   Text            (Raycast Target KAPALI)
└── SelectionFrame          Image           (Raycast Target KAPALI, başta kapalı)
```

> **Kök nesnedeki `Image` ve `Raycast Target` zorunludur.** Unity'de sürükleme
> olayları yalnızca ışın hedefi olan bir `Graphic` üzerinden gelir. Kök
> nesnede ışın hedefi yoksa `PaletteEntryView` hiçbir olay almaz ve panel
> **sessizce ölü** olur — konsolda tek satır çıkmaz.

Prefab'ı `Assets/Game/Prefabs/` altına koyun.

#### Neden eski `Text`, TextMeshPro değil

`GridStrategy.Unity.asmdef` bugün `"references": ["GridStrategy.Core",
"GridStrategy.Combat", "GridStrategy.Battle"]` taşıyor ve `"overrideReferences":
false`. `UnityEngine.UI` bir **önceden derlenmiş** derlemedir ve bu ayarla
kendiliğinden referanslanır; TextMeshPro ise kendi `.asmdef`'iyle gelir ve
listeye **elle eklenmesi** gerekirdi. Yani TMP kullanmak bu hattın dokunmaması
gereken bir dosyayı değiştirmeyi zorunlu kılardı. TMP'ye geçmek istenirse:
`GridStrategy.Unity.asmdef` ▸ `Assembly Definition References` ▸
`Unity.TextMeshPro` eklenir, sonra `Text` alanları `TMP_Text`'e çevrilir.

---

### 4 · Bileşenleri bağlama

#### 4a · `ProductionDirector` (GameDirector üstünde)

| alan | ne sürüklenir |
|---|---|
| `boardBehaviour` | **Board** nesnesi (üstündeki `BoardAdapter` bileşeni) |

> Bu alan `MonoBehaviour` tipinde çünkü Unity Inspector bir **arayüz** alanına
> nesne sürükletmez. `BoardAdapter` `IPlacementBoard`'u uygulayana kadar burası
> `Awake`'te "does not implement IPlacementBoard" diye **bağıracak** — o mesaj
> bir hata değil, entegrasyonun henüz yapılmadığının bildirimi.

#### 4b · `StructurePaletteView` (LeftPanel üstünde)

| alan | ne sürüklenir |
|---|---|
| `director` | GameDirector |
| `entryPrefab` | `PaletteEntry` prefab'ı |
| `playerRow` | `PlayerRow` |
| `enemyRow` | `EnemyRow` |
| `playerStructures` | `Barrack.asset`, `PowerPlant.asset` |
| `enemyStructures` | `Barrack.asset`, `PowerPlant.asset` |

> Aynı `.asset` dosyası **iki listeye birden** konabilir ve konmalıdır: tanım
> taraf tutmaz, tarafı liste belirler. Bu, "aynı baraka tanımı iki tarafta da
> kullanılır" kararının editördeki karşılığıdır.

#### 4c · `ProductionPanelView` (RightPanel üstünde)

| alan | ne sürüklenir |
|---|---|
| `director` | GameDirector |
| `entryPrefab` | `PaletteEntry` prefab'ı |
| `row` | `ProducedRow` |
| `emptyLabel` | `EmptyLabel` |

#### 4d · `PaletteEntryView` (prefab'ın kökünde, prefab modunda bağlanır)

| alan | ne sürüklenir |
|---|---|
| `label` | çocuk `Label` |
| `icon` | çocuk `Icon` |
| `selectionFrame` | çocuk `SelectionFrame` |

---

### 5 · Her `[SerializeField]` için: boş bırakılırsa ne olur

**27 alan.** Kademe dağılımı: **9 SESLİ · 9 SESSİZ-ÖLÜ · 9 SESSİZ-YANLIŞ**.

#### `UnitBlueprintAsset` — 5 alan

| alan | boş/başlatıcı hâli | ne olur | kademe |
|---|---|---|---|
| `displayName` | `"Unit"` | Boşaltılırsa varlık **dosya adı** kullanılır; tanımlı bir geri çekilme | SESSİZ-ÖLÜ |
| `icon` | `null` | Sol paneldeki simge alanı gizlenir, düğme yalnız yazı gösterir | SESSİZ-ÖLÜ |
| `maxHealth` | `30` | Bütün birimler 30 canla doğar; tasarımcının sayısı değil, başlatıcının | SESSİZ-YANLIŞ |
| `damage` | `10` | `0` yazılırsa birim vurur ama **hiç öldürmez**; hata mesajı yok | SESSİZ-YANLIŞ |
| `attackRange` | `1` | `[Min(1)]` sıfırı engeller; yanlış bir menzil sessizce yakın dövüşe düşürür | SESSİZ-YANLIŞ |

#### `StructureBlueprintAsset` — 8 alan

| alan | boş/başlatıcı hâli | ne olur | kademe |
|---|---|---|---|
| `displayName` | `"Structure"` | Boşaltılırsa varlık dosya adı kullanılır | SESSİZ-ÖLÜ |
| `icon` | `null` | Simge alanı gizlenir | SESSİZ-ÖLÜ |
| `maxHealth` | `50` | Bütün yapılar 50 canla doğar | SESSİZ-YANLIŞ |
| `damage` | `0` | `attackRange` 0 iken **hiç okunmaz**; 0'dan büyük menzille birlikte 0 hasar, yapı vurur ve hiçbir şey olmaz | SESSİZ-YANLIŞ |
| `attackRange` | `0` | >> **En pahalı alan.** << `damage` 15 yazıp menzili unutmak, yapının **hiç saldırmaması** demektir; tek satır uyarı çıkmaz | SESSİZ-YANLIŞ |
| `produces` | boş dizi | Yapı hiçbir şey üretmez ve sağ panel boş etiketi gösterir — santral için DOĞRU, baraka için sessiz felaket. **Dizinin içinde boş bir göz** varsa o göz atılır ve `LogError` basılır | SESSİZ-YANLIŞ (boş göz: SESLİ) |
| `defaultProducedIndex` | `0` | Liste dışına düşerse `LogError` basılır ve 0'a kırpılır | SESLİ |
| `productionSeconds` | `3` | `0` yazmak "anında üretim" demektir; ekonomi olmadığı için oyuncu **sınırsız** birim basar | SESSİZ-YANLIŞ |

#### `ProductionDirector` — 1 alan

| alan | boş hâli | ne olur | kademe |
|---|---|---|---|
| `boardBehaviour` | `null` | `Awake`'te `LogError`; ayrıca yanlış tipte bir bileşen sürüklenirse **ikinci ve farklı** bir `LogError` | SESLİ |

#### `PaletteEntryView` — 3 alan

| alan | boş hâli | ne olur | kademe |
|---|---|---|---|
| `label` | `null` | Düğmede yazı çıkmaz; tıklanır ve çalışır ama okunmaz | SESSİZ-ÖLÜ |
| `icon` | `null` | Simge çizilmez | SESSİZ-ÖLÜ |
| `selectionFrame` | `null` | Seçim **görünmez** olur. Yerleştirme doğru çalışır, ama "biri varsayılan olarak seçili" cümlesinin ekrandaki karşılığı kaybolur | SESSİZ-YANLIŞ |

#### `StructurePaletteView` — 6 alan

| alan | boş hâli | ne olur | kademe |
|---|---|---|---|
| `director` | `null` | `LogError`; düğmeler çizilir, tıklanır, **hiçbir şey yerleşmez** | SESLİ |
| `entryPrefab` | `null` | `LogError`; panel tamamen boş kalır | SESLİ |
| `playerRow` | `null` | `LogError` (kaç yapı türünün çizilemediğini de yazar) | SESLİ |
| `enemyRow` | `null` | `LogError`, aynı biçim | SESLİ |
| `playerStructures` | boş dizi | Üst sıra boş kalır; boş bir göz varsa `LogError` | SESSİZ-ÖLÜ (boş göz: SESLİ) |
| `enemyStructures` | boş dizi | Alt sıra boş kalır; boş bir göz varsa `LogError` | SESSİZ-ÖLÜ (boş göz: SESLİ) |

#### `ProductionPanelView` — 4 alan

| alan | boş hâli | ne olur | kademe |
|---|---|---|---|
| `director` | `null` | `LogError`; panel hiç dolmaz | SESLİ |
| `entryPrefab` | `null` | `LogError`; panel boş kalır | SESLİ |
| `row` | `null` | Çizilecek bir liste varken `LogError` | SESLİ |
| `emptyLabel` | `null` | "Bu yapı üretmiyor" bilgisi hiç gösterilmez | SESSİZ-ÖLÜ |

---

### 6 · Bu prosedür bitince ne ÇALIŞIR, ne ÇALIŞMAZ

**Çalışır** (BoardAdapter entegrasyonundan bağımsız):

- Sol panel iki sıra hâlinde çizilir, `Barrack` ve `Power Plant` düğmeleri görünür.
- Düğmeye tıklamak seçim çerçevesini taşır.
- Bütün eksik atamalar yukarıdaki kademelere göre davranır.

**ÇALIŞMAZ** — `BoardAdapter`, `IPlacementBoard`'u uygulayana kadar:

- Sürükleyip haritaya bırakmak (yapı da birim de yerleşmez).
- Yerleşmiş bir yapıyı seçince sağ panelin dolması.
- Üretim sayacının işlemesi *(işler, ama tetikleyecek bir üretim doğmaz)*.

Entegrasyonun tam listesi bu hattın dönüş raporunda; ana ajan bağlayacak.

---

### 7 · Doğrulama listesi (editör işi bitince tek tek bakılır)

1. `EventSystem` sahnede **var mı**.
2. `UICanvas` üstünde `GraphicRaycaster` **var mı**.
3. `PaletteEntry` prefab'ının **kökünde** `Image` var mı ve `Raycast Target` **açık** mı.
4. Play'e basınca konsolda **yalnızca** "does not implement IPlacementBoard"
   satırı çıkıyor mu — başka bir `LogError` çıkıyorsa yukarıdaki tabloda
   `SESLİ` yazan bir alan boş kalmıştır.
5. Sol panelde üst sırada oyuncunun, alt sırada düşmanın yapıları duruyor mu.
6. `Barrack.asset`'in `produces` listesi **iki** birim taşıyor mu ve içinde
   boş göz **yok** mu (varsa konsol bağırır).
