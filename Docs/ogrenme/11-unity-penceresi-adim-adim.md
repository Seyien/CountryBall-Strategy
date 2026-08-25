# Unity penceresi — adım adım

> **Ne zaman oku:** [`00-okuma-sirasi.md`](00-okuma-sirasi.md)'nın ***ADIM 8***'ini
> bitirdikten sonra, ***DURMA NOKTASI 4***'e girmeden **önce**. Gerekçesi
> ölçüldü ve aşağıda yazılı.
> **Ne yapar:** `BoardAdapter` ve `UnitView` üstündeki **on altı** serileştirilmiş
> alanın her biri için tek satır verir: Inspector'da hangi başlık altında
> görünür, oraya ne sürüklenir, boş ya da yanlış bırakılırsa kod ne yapar, ve
> hangi `.cs` satırında tanımlıdır. ***Sahnede yazılı OLMAYAN bir alanın hangi
> değeri aldığını depodan ölçerek cevaplar*** ve bir alanın eksikliğini üç
> kademeye ayırır: sesli · sessiz-ölü · sessiz-yanlış. Sonra bugünkü sahnenin
> ölçülmüş eksiğini pencere pencere onartır.
> **Ne YAPMAZ:** hiçbir mekanizmayı anlatmaz — serileştirmenin arkasında ne
> olduğu [`08-unity-altyapisi.md`](08-unity-altyapisi.md)'nin işidir, tıklamanın
> eyleme dönüşmesi [`konular/07`](../deep/konular/07-tiklamadan-eyleme.md)'nin.
> Ve ***koddaki yerleştirme kusurunu ONARMAZ***; yalnızca onu **görülebilir**
> hâle getirir.

***Bu dosya bir prosedürdür.*** Diğer on dosya "neden böyle" der; bu dosya
"şimdi nereye tıklıyorsun" der. Ayrımı akılda tutmanın en ucuz yolu şu: bu
dosyadaki hiçbir cümle bir tasarım kararını savunmaz.

**Sürüm bağı:** aşağıdaki menü yolları ve pencere adları
***Unity 2021.3.45f2*** içindir. Ölçü:

```
ProjectSettings/ProjectVersion.txt
    m_EditorVersion: 2021.3.45f2
```

---

## Bu dosya okuma sırasının neresinde

***Karar: ADIM 8 ile DURMA NOKTASI 4 arasına, ADIM 8b olarak.***

Gerekçe üç ölçüye dayanıyor ve üçü de `00-okuma-sirasi.md` üstünde sayıldı.

**ÖLÇÜ 1 — Editör ilk kez nerede açılıyor.** Belgede altı durma noktası var.
Beşi bir komut ya da bir test dosyası açtırıyor; ***yalnızca DURMA NOKTASI 4
Unity penceresini açtırıyor.*** Sayım komutu ve çıktısı:

```
grep -c "run-editmode-tests.ps1" Docs/ogrenme/00-okuma-sirasi.md     # -> 3
grep -c "Unity'yi ac"           Docs/ogrenme/00-okuma-sirasi.md      # -> 1
```

Yani Unity penceresine geçiş noktası bir tane ve yeri belli. Bu dosya oraya
takılmazsa hiçbir yere takılmaz.

**ÖLÇÜ 2 — DURMA NOKTASI 4'ün ikinci senaryosu bugün BAŞLAMIYOR.** Metin şunu
yaptırıyor: *"Bir birim seç, `B`'ye bas (yerleştirme kipi açılır, hayalet
belirir), sürükle, tahta içindeki boş bir hücreye bırak."* Parantez içindeki
iki olayın ikisi de bugün gerçekleşmiyor, çünkü `placementGhost` sahnede
atanmamış:

```
BoardAdapter.cs:466   if (placementGhost == null)
BoardAdapter.cs:468   Debug.LogError("[Board] Cannot enter placement mode: placementGhost is not assigned.", this);
```

`return` ile birlikte geldiği için kip hiç açılmıyor. Sürükleyecek bir hayalet
yok, dolayısıyla o durma noktasının anlatmak istediği istisna hiç görünmüyor.
***Onarım bu adımın ÜSTÜNDE olmak zorunda; altında olsaydı durma noktası kendi
ilk cümlesinde düşerdi.***

`placementGhost` bu zincirin **tek** halkası değil: `placementModeKey` de
sahnede yazılı değil ve o alan düşerse yukarıdaki iki satıra hiç gelinmez —
`B` tuşu sessizce hiçbir şey yapar. Hangi halkanın canlı olduğu ölçüldü ve
aşağıda, "Sahnede yazılı olmayan bir alan hangi değeri alır" başlığı altında
yazılı. ***İki halkada da sonuç aynı: ② başlamıyor.***

**ÖLÇÜ 3 — ADIM 9'dan SONRAYA koymak neden reddedildi.** İlk akla gelen yer
`konular/08-motor-cagri-dongusu.md`'nin (ADIM 9) ardıdır: motoru anlamadan
Editör'e girmek erken görünür. Ama sıra bu kararı zaten vermiş — DURMA NOKTASI
4, ADIM 9'un **önünde** duruyor ve Play'e bastırıyor. Bu dosyayı 9'dan sonraya
koymak, operatörü önce kırık bir sahneye çarptırıp sonra tamir aletini vermek
olurdu. Sıra dosyası Unity'yi zaten erken açıyor; ben o kararı değiştirmiyorum,
yalnızca boşluğunu dolduruyorum.

**Ön koşul olarak neyin gerektiği:** ADIM 8. Bu dosyadaki her doğrulama adımı
bir Console satırı okutuyor ve o satırların hangi akıştan geldiğini `konular/07`
anlatıyor. ADIM 8 okunmadan Console bir gürültü listesidir.

---

## Tahtanın bugünkü kurulumu — tek bakışta

```
   SAHNE : Assets/Scenes/SampleScene.unity        (2 kok GameObject, olculdu)
   ────────────────────────────────────────────────────────────────────────
   Main Camera          Transform . Camera . AudioListener
     |                  m_TagString: MainCamera   <<< TryReadPointerCell BUNU arar
     |                  orthographic: 1 . size: 5 . pos (1.5, 2.5, -10)
     |
   Board                Transform . Grid . BoardAdapter (Script)
     |                  Grid: m_CellSize (1,1,1) . m_CellGap (0,0,0)
     |
     +-- (bos)          >> BURASI BOS: hicbir cocuk yok <<
                        m_Children: []  -- olculdu, sahne dosyasinda
                        Yerlestirme hayaleti tam BURAYA gelecek.

   PREFAB : Assets/Game/Prefabs/Unit.prefab
   ────────────────────────────────────────────────────────────────────────
   Unit                 Transform . SpriteRenderer (sortingOrder 2) . UnitView
     |                  govde sprite: friendly_vanguard_infantry_tile_0142.png
     |
     +-- SelectionOverlay  Transform . SpriteRenderer (sortingOrder 1)
                           sprite: selection_unit_bracket_tile_0061.png
                           UnitView.selectionOverlay BU cizicye bagli
```

`Board`'un altındaki on beş `Cell_x_y` nesnesi sahne dosyasında **yok**; onları
`BuildCellVisuals` Play sırasında kodla üretir. Hierarchy'de ancak Play'e
bastıktan sonra görünürler ve Play'den çıkınca kaybolurlar.

---

## On altı alanın tam listesi

İki tablo, iki ayrı sahip. Birincisinin sahibi **sahnedeki** `Board` nesnesi,
ikincisininki **prefab**. Bu ayrım pratikte şu demektir: birinci tablonun
alanlarını Hierarchy'de bir nesne seçerek, ikincisininkileri Project'te prefab'ı
açarak düzenlersin.

### `BoardAdapter` — 13 alan (sahnedeki `Board` nesnesinde)

| Alan · tip | Inspector başlığı ve alan adı | Oraya ne gelir | Boş ya da sınırda bırakılırsa — koddan ölçüldü | Tanım |
|---|---|---|---|---|
| `width` · `int` | **Board size in CELLS, not world units** → Width | Elle yazılır | Boş kalamaz; `[Min(1)]` 0'ı ve negatifi engeller. Değiştiği anda tahta ölçüsü değişir, çünkü sayı doğrudan `Battle` kurucusuna gider. 1 yazılırsa tek sütunluk bir tahta doğar ve kimse şikâyet etmez | `BoardAdapter.cs:113` |
| `height` · `int` | **Board size in CELLS, not world units** → Height | Elle yazılır | `width` ile aynı; ikisi birlikte `UnitGrid`'in dizi ölçüsünü kurar. Sıfır yazılamaz, çünkü hem `[Min(1)]` hem `UnitGrid` kurucusu reddeder | `BoardAdapter.cs:114` |
| `terrainSprites` · `Sprite[]` | **Terrain sprites - at least one required** → Terrain Sprites | **Project** penceresinden `.png` dosyaları listeye sürüklenir | Boş ya da `null` ise `BuildCellVisuals` kırmızı bir satır yazıp **döner**: on beş hücre görselinin **hiçbiri** doğmaz. Oyun yine de oynanır — tıklama, seçim, hareket ve saldırı zeminden bağımsızdır — ama ekran boştur | `BoardAdapter.cs:117` |
| `unitPrefab` · `UnitView` | **Unit prefab** → Unit Prefab | **Project** penceresinden `Assets/Game/Prefabs/Unit.prefab` sürüklenir | Boş ise `Awake` kırmızı satır yazar ve **iki demo birim hiç doğmaz**. Sonuç zincirleme: seçilecek birim olmadığı için `B` tuşu da çalışmaz. Alanın tipi `GameObject` değil `UnitView` olduğu için, `UnitView` **taşımayan** bir prefab'ı Inspector zaten kabul etmez | `BoardAdapter.cs:124` |
| `maxHealth` · `int` | **Unit stats - applied to every spawned unit** → Max Health | Elle yazılır | Boş kalamaz; `[Min(1)]` ve `Health` kurucusu 0'ı reddeder. Doğan **her** birime uygulanır, taraf ayrımı yoktur | `BoardAdapter.cs:135` |
| `damage` · `int` | **Unit stats…** → Damage | Elle yazılır | `[Min(0)]` sıfıra **izin verir** ve `AttackProfile` da negatif olmayanı kabul eder. 0 yazılırsa saldırı geçerli sayılır, Console `was hit` der, ama can hiç düşmez ve hiçbir birim asla düşmez | `BoardAdapter.cs:138` |
| `attackRange` · `int` | **Unit stats…** → Attack Range | Elle yazılır | `[Min(1)]` 0'ı engeller; engellemeseydi `AttackProfile` kurucusu istisna atardı. İkinci bir kapı, aynı kural | `BoardAdapter.cs:141` |
| `moveRange` · `int` | **Unit stats…** → Move Range | Elle yazılır | `[Min(0)]` sıfıra izin verir ve 0 **kök salmış birim** demektir: her hareket `RejectedOutOfRange` döner. Alan `Header`'ı "Unit stats" olduğu için Inspector'da saldırı sayılarının hemen altında görünür | `BoardAdapter.cs:150` |
| `placementGhost` · `SpriteRenderer` | **Placement ghost - assign a child SpriteRenderer, kept disabled at rest** → Placement Ghost | **Hierarchy**'den, `SpriteRenderer` taşıyan bir **sahne** nesnesi sürüklenir | ***BUGÜN BOŞ.*** Boşken iki şey olur: `Awake` bir kırmızı satır yazar, ve `B` tuşu yerleştirme kipini **hiç açmaz**. Project'teki bir prefab varlığı sürüklenemez — kod her karede onun `transform.position`'ını yazar, bu yüzden sahnede yaşayan bir nesne şart | `BoardAdapter.cs:160` |
| `dragThreshold` · `float` | **Pointer gesture** → Drag Threshold | Elle yazılır (dünya birimi, piksel değil) | `[Min(0f)]` sıfıra izin verir. 0 yazılırsa karşılaştırma **kesin büyüktür** olduğu için hiç kıpırdamayan bir basış hâlâ "tıklama"dır, ama bir piksellik kayma bile "sürükleme" sayılır | `BoardAdapter.cs:168` |
| `placementModeKey` · `KeyCode` | **Pointer gesture** → Placement Mode Key | Açılır listeden seçilir | Bir enum alanı; listenin ilk girdisi `None`'dır ve kodda `None` için bir kontrol **yoktur**. `None` seçilirse `TryEnterPlacementMode` hiç çağrılmaz ve `B`'ye basınca Console **sessiz** kalır — üç kademeden ***SESSİZ-ÖLÜ***. Yerleştirme kipini açacak başka bir yol da yok | `BoardAdapter.cs:171` |
| `placementCancelKey` · `KeyCode` | **Pointer gesture** → Placement Cancel Key | Açılır listeden seçilir | Yukarıdakinin ikizi. Sonucu daha ağır: iptal tuşu düşerse kipten çıkmanın tek yolu bir yerleştirme denemesi olur, çünkü `CommitPlacement` sonuçtan bağımsız olarak kipi kapatır | `BoardAdapter.cs:174` |
| `structureMaxHealth` · `int` | **Structure stats - applied to every placed structure** → Structure Max Health | Elle yazılır | `[Min(1)]` ve `Health` kurucusu 0'ı reddeder. Bugün bu alanın etkisini ekranda **görmek mümkün değil**, çünkü hiçbir yapı tahtaya yerleşemiyor — sebebi aşağıda | `BoardAdapter.cs:178` |

### `UnitView` — 3 alan (prefab üstünde)

| Alan · tip | Inspector başlığı ve alan adı | Oraya ne gelir | Boş bırakılırsa — koddan ölçüldü | Tanım |
|---|---|---|---|---|
| `selectionOverlay` · `SpriteRenderer` | **Selection overlay - assign the child SpriteRenderer from the prefab** → Selection Overlay | Prefab'ın **kendi** hiyerarşisinden `SelectionOverlay` çocuğu sürüklenir | Boşsa `Awake` kırmızı bir satır yazar ve **erken döner**; `SetSelected` de sessizce döner, yani seçim çerçevesi hiç görünmez. Erken dönüşün üstünde kalan `SetState(UnitState.Alive)` yine de koşar — sıra bilinçli | `UnitView.cs:51` |
| `downedTint` · `Color` | **Downed tint - multiplied over the authored body color** → Downed Tint | Renk seçiciden | Bir `struct`; "boş" hâli `(0,0,0,0)`'dır. Değer bir **çarpan** olduğu için sıfır çarpan, düşmüş birimi tamamen saydam yapar: birim ekrandan kaybolur ama tahtada durmaya devam eder | `UnitView.cs:59` |
| `deadTint` · `Color` | **Dead tint - multiplied over the authored body color** → Dead Tint | Renk seçiciden | `downedTint` ile aynı mekanizma. Bugünkü gövde beyaz olduğu için gri bir çarpan gerçekten gri verir; gövde bir gün takım rengi taşırsa aynı çarpan gri değil koyu bir takım rengi üretir | `UnitView.cs:66` |

### Yukarıdaki her hükmün dayandığı satır

Tablodaki "boş bırakılırsa" sütunu tahmin değil. Her biri şu satırlardan
okundu:

```
ZEMIN
BoardAdapter.cs:959   if (terrainSprites == null || terrainSprites.Length == 0)
BoardAdapter.cs:1015   int index = (x * 7 + y * 13) % terrainSprites.Length;
BoardAdapter.cs:975   Debug.Log($"[Board] built {battle.Width}x{battle.Height} = {battle.CellCount} cells.", this);
      >> 646 dogruysa 662'ye HIC gelinmez: aradaki return donguyu atlar. <<

BIRIM
BoardAdapter.cs:326   if (unitPrefab != null)
BoardAdapter.cs:1078   UnitView view = Instantiate(unitPrefab, transform);
BoardAdapter.cs:1097                   new Health(maxHealth),
BoardAdapter.cs:1099                   new AttackProfile(damage, attackRange),
BoardAdapter.cs:1221   MoveOutcome outcome = BattleActions.Move(battle, selectedUnit, x, y, moveRange);

HAYALET
BoardAdapter.cs:312                       "[Board] placementGhost is not assigned. Assign a child SpriteRenderer; structure placement mode will refuse to start without it.",
BoardAdapter.cs:468   Debug.LogError("[Board] Cannot enter placement mode: placementGhost is not assigned.", this);
BoardAdapter.cs:483   placementGhost.enabled = true;
BoardAdapter.cs:801   renderer.sprite = placementGhost.sprite;
      >> 573'un anlami: hayaletin sprite'i AYNI ZAMANDA yapinin sprite'idir. <<

OLCU VE JEST
BoardAdapter.cs:299   battle = new Battle(width, height);
BoardAdapter.cs:304   gesture = new PointerGesture(dragThreshold);
BoardAdapter.cs:434   if (Input.GetKeyDown(placementModeKey))
BoardAdapter.cs:507   if (Input.GetKeyDown(placementCancelKey))
BoardAdapter.cs:773   return new Structure(new Health(structureMaxHealth), new StructureLifecycle(), team);

SINIRLARIN GERCEK SAHIPLERI  -- [Min] tek kapi degil, ikinci kapi kodda
UnitGrid.cs:36        if (width <= 0)
AttackProfile.cs:59   if (range < 1)
PointerGesture.cs:296 return ((dx * dx) + (dy * dy)) > dragThresholdSquared;
AttackAction.cs:107   return stateBeforeHit == UnitState.Alive && target.State == UnitState.Downed

GORUNUM
UnitView.cs:100           "[UnitView] selectionOverlay is not assigned. Assign the SelectionOverlay child's SpriteRenderer on the Unit prefab.",
UnitView.cs:93        SetState(UnitState.Alive);
UnitView.cs:154       selectionOverlay.enabled = isSelected;
UnitView.cs:190       bodyRenderer.color = authoredColor * TintFor(state);
UnitView.cs:210               return downedTint;
UnitView.cs:213               return deadTint;
```

`PointerGesture.cs:296` satırında karşılaştırılan değer `dragThresholdSquared`
ve işaret **kesin büyüktür**, "büyük eşit" değil. Tablodaki eşik hükmü tam
olarak bu tek karakterden çıkıyor.

---

## Sahnede YAZILI OLMAYAN bir alan hangi değeri alır

***Bu, bu dosyanın en pahalı sorusudur ve tablodaki her "boş bırakılırsa"
hükmü ona dayanıyor.*** Sahne dosyası on üç alanın dördünü taşıyor. Kalan
dokuz için iki rakip hüküm var ve ikisi ***zıt*** bir tahta üretiyor:

```
HUKUM A  "BASLATICI YASAR"
         Unity yonetilen nesneyi once KURAR (C# alan baslaticilari kosar),
         sonra YAML'da BULUNAN anahtarlari uzerine yazar. YAML'da olmayan
         alan, baslatici degerinde KALIR.
             maxHealth 30 . damage 10 . placementModeKey KeyCode.B ...

HUKUM B  "default(T) KALIR"
         Serilestirme her alani YAML'dan doldurur; bulunmayan alan tipinin
         sifir degerinde kalir.
             maxHealth 0 . damage 0 . placementModeKey KeyCode.None ...
```

### Ölçüm — depo bu soruyu KENDİSİ cevaplıyor

Sahne için doğrudan bir ölçü yok (aşağıda yazılı), ama **prefab için var**.
`Unit.prefab` tam olarak aynı durumda: `UnitView`'in üç alanından YAML'da
yalnız `selectionOverlay` yazılı, `downedTint` ve `deadTint` yazılı **değil**.
Ve Unity o prefab'ı içe aktarıp sonucu `Library/Artifacts/` altına yazmış.

Aktarılmış veri bloğu, alanları tip ağacındaki sırayla yan yana tutuyor:

```
Library/Artifacts/70/70d165bf82478cddaff9b762fc0e77a4        (11.852 bayt)

  tip agaci sirasi : selectionOverlay -> downedTint -> deadTint

  ofset 10804  int64  4140981733856144320   <<< selectionOverlay
                                                 prefab YAML'inde YAZILI olan
                                                 fileID ile BIREBIR ayni
  ofset 10812  float  1
  ofset 10816  float  1
  ofset 10820  float  1
  ofset 10824  float  0.45      >> downedTint = (1, 1, 1, 0.45) <<
  ofset 10828  float  0.35
  ofset 10832  float  0.35
  ofset 10836  float  0.38
  ofset 10840  float  1         >> deadTint = (0.35, 0.35, 0.38, 1) <<
```

Karşılığı kodda:

```
UnitView.cs:59        [SerializeField] private Color downedTint = new Color(1f, 1f, 1f, 0.45f);
UnitView.cs:66        [SerializeField] private Color deadTint = new Color(0.35f, 0.35f, 0.38f, 1f);
```

***Sekiz kayan sayı, iki alan başlatıcısıyla bayt bayt aynı.*** Prefab
YAML'inde o iki alan hiç yok. ***HÜKÜM A ölçüldü, HÜKÜM B çürüdü.***

Zincir kapalı: prefab dosyası bu iki alandan önce kaydedilmiş, betik içe
aktarma anında ikisini de taşıyordu (tip ağacı ikisini de listeliyor), ve
üretilen veri başlatıcı değerlerini taşıyor.

**Ölçüm aracının kendi sınaması.** Bu blok bir kez YANLIŞ okundu ve yakalayan
şey aşağıdaki iki denetim oldu. Aynı arama önce Unity'nin içe aktarma
üstverisinde koşturuldu; orada prefab'ın YAML'de **yazılı** olan değerlerini
(kök konumu, çerçeve ölçeği, çerçeve rengi) de bulamadı. ***Bilinen-iyi girdide
boş dönen bir arama, hiçbir hüküm veremez*** — o okuma atıldı. Doğru dosya
bulunduğunda altı bilinen-iyi değerin altısı da eşleşti ve iki uydurma değer
(`0.4567891`, `7.7777`) hiç eşleşmedi.

**Ölçünün sınırı, abartılmadan.** Ölçülen yol **prefab içe aktarımıdır**.
Sahne için aynı ölçü yapılamadı: `Library/` altındaki 19.589 dosya tarandı ve
`SampleScene.unity`'nin içe aktarılmış bir kopyası **bulunamadı** (sahneler
Editör'de doğrudan yüklenir, `Artifacts/` altına yazılmaz). Arkadaki
serileştirme yolu aynı yoldur, ama sahne tarafı ayrıca ölçülmedi.

### İki hükmü Play'de bir bakışta ayıran şey

Bu ayrımı operatör tek koşturmada kapatır ve ölçü koddadır:

```
BoardAdapter.cs:1097                   new Health(maxHealth),
Health.cs:37          if (max <= 0)

HUKUM A dogruysa  -> maxHealth 30 . Health kurucusu memnun . IKI birim dogar
HUKUM B dogruysa  -> maxHealth 0  . Health kurucusu ISTISNA atar
                     ArgumentOutOfRangeException: Max health must be positive.
                     >> Awake SpawnUnit'te patlar, tahtada HIC birim olmaz <<
```

***Yani Game penceresinde iki asker görüyorsan HÜKÜM A yürürlüktedir ve soru
kapanmıştır.*** Görmüyorsan Console'un ilk satırı sana hangi kurucunun
şikâyet ettiğini söyler.

### Sınırı kim savunuyor — `[Min]` değil

`[Min(1)]` bir Inspector çizici niteliğidir: **yazma** anını kelepçeler,
**yükleme** anını değil. Bu depoda ölçülebilen kısmı şudur — gerçek ikinci kapı
alan bildiriminin yanında değil, sayının **gittiği kurucularda** duruyor ve o
kapılar istisna atıyor:

```
Health.cs:37          if (max <= 0)
AttackProfile.cs:59   if (range < 1)
UnitGrid.cs:36        if (width <= 0)
```

Bu üç satır, `[Min]` hiç olmasaydı da yerinde dururdu. ***Nitelik bir kolaylık,
kurucu bir kural.***

### Üç kademe — bir alanın eksikliği nasıl görünür

Bir alanın eksik olması her zaman aynı şekilde görünmez ve aradaki fark
operatörün kaybettiği saatlerdir:

```
SESLI          konsola bir sey basar          -> operator ne oldugunu OKUR
SESSIZ-OLU     hicbir sey olmaz               -> operator "bozuk mu" diye dusunur
SESSIZ-YANLIS  calisir ama YANLIS calisir     -> en pahalisi, cunku hic sorulmaz
```

Bugünkü sahnedeki dokuz eksik alanın kademesi, ölçülen HÜKÜM A altında:

| Eksik alan | Kademe | Neden — koddan ölçüldü |
|---|---|---|
| `placementGhost` | ***SESSİZ-ÖLÜ***, sonra **SESLİ** | Tek gerçek arıza budur ve iki hükümde de aynıdır: başlatıcısı **yok**. `Awake` bir kırmızı satır basar (SESLİ), ama `B` tuşuna basıldığında kip sessizce açılmaz — ikinci kırmızı satır ancak `TryEnterPlacementMode` çağrıldığında gelir |
| `maxHealth` | HÜKÜM A'da zararsız · HÜKÜM B'de **SESLİ** | `Health.cs:37` sıfırı istisna ile reddeder; sessiz kalması mümkün değil |
| `attackRange` | HÜKÜM A'da zararsız · HÜKÜM B'de **SESLİ** | `AttackProfile.cs:59` sıfırı istisna ile reddeder |
| `structureMaxHealth` | HÜKÜM A'da zararsız · HÜKÜM B'de **SESLİ** ama **geç** | Aynı `Health` kapısı, ama ancak bir yapı yerleştirilirken çağrılır; bugün o yola hiç girilmiyor |
| `damage` | HÜKÜM A'da zararsız · HÜKÜM B'de ***SESSİZ-YANLIŞ*** | `[Min(0)]` da `AttackProfile` da sıfırı **kabul eder**. Saldırı geçerli sayılır, Console `was hit` yazar, can hiç düşmez. Hiçbir kapı şikâyet etmez |
| `moveRange` | HÜKÜM A'da zararsız · HÜKÜM B'de ***SESSİZ-YANLIŞ*** | Sıfır "kök salmış" demektir ve bu geçerli bir değerdir; her hareket sessizce `RejectedOutOfRange` döner |
| `dragThreshold` | HÜKÜM A'da zararsız · HÜKÜM B'de ***SESSİZ-YANLIŞ*** | `PointerGesture` yalnız NaN'ı ve negatifi reddeder; sıfır eşik her kımıldamayı sürükleme sayar |
| `placementModeKey` | HÜKÜM A'da zararsız · HÜKÜM B'de ***SESSİZ-ÖLÜ*** | `BoardAdapter.cs:434` tuşu sorar ve `KeyCode.None` hiçbir tuşla eşleşmezse `TryEnterPlacementMode` hiç çağrılmaz: `B`'ye basılır, Console **sessiz** kalır |
| `placementCancelKey` | HÜKÜM A'da zararsız · HÜKÜM B'de ***SESSİZ-ÖLÜ*** | Aynı mekanizma, `BoardAdapter.cs:507`'de |

***Tablonun okunma biçimi şudur:*** ölçülen hüküm A olduğu için bugün canlı olan
tek satır birincisidir. Kalan sekiz satır bir tehdit envanteridir ve sebebi
yazılı — HÜKÜM B'nin dört ***SESSİZ*** satırı, bir hatayı saatlerce
görünmez tutabilecek tek sınıftır. ***Bu yüzden ADIM E'de sahne kaydedilir:***
kayıttan sonra on üç alanın hepsi YAML'a yazılır ve soru bir daha sorulmaz.

---

## Bugünkü sahne — neyin kırık olduğu, neyin OLMADIĞI

***Önce bir yanlış model kapatılıyor.*** Bu turda ilk verilen tarif "zemin
karoları görünmüyor, çünkü `terrainSprites` boş" diyordu. ***Ölçüm bunu
çürüttü.*** Sahne dosyasında dizi boş değil, dört elemanı var ve dördü de
diskteki gerçek dosyalara çözülüyor:

```
grep -o "guid: [0-9a-f]\{32\}" Assets/Scenes/SampleScene.unity | sort -u
   -> 7 farkli guid

Dordu terrainSprites'in elemanlari ve hepsi cozuluyor:
   6cb1936eb8bc475386243b8168a2815f -> .../TinyTown/Terrain/Dirt/dirt_fill_scatter_a_tile_0039.png
   fd73cbf9932743beaf2dba8e88b11e60 -> .../TinyTown/Terrain/Dirt/dirt_fill_scatter_b_tile_0040.png
   24f13717de7f410eab688c1090351d86 -> .../TinyTown/Terrain/Dirt/dirt_fill_scatter_c_tile_0041.png
   f4e8502bea254ed4864b4f54f8c00d13 -> .../TinyTown/Terrain/Dirt/dirt_fill_scatter_d_tile_0042.png
```

Dördü de `textureType: 8` (Sprite) ve `spritePixelsToUnits: 16` ile içe
aktarılmış, `Grid` bileşeninin hücre ölçüsü ise `(1,1,1)`. Yani her karo tam
bir hücreyi dolduruyor. ***Zemin doğuyor.*** Play'e basınca Console'da
`[Board] built 3x5 = 15 cells.` satırını göreceksin.

**Yanlış ölçüm nasıl doğdu — ve genelleştirilebilir ders.** İlk ölçüm YAML'da
`terrainSprites:` **anahtar satırında durdu** ve altındaki liste elemanlarını
hiç görmedi. Bir YAML dizisinin boş olup olmadığı anahtar satırından
okunamaz — anahtar satırı yalnız "alan yazılı" der. Sayılacak olan şey
altındaki `- ` satırlarıdır:

```
sed -n '/^  terrainSprites:/,/^  [a-zA-Z]/p' Assets/Scenes/SampleScene.unity | grep -c '^  - '
   -> 4

OZ-SINAMA (komut bos donebiliyor mu):
   sentetik "terrainSprites: []" girdisinde   -> 0
   sentetik iki elemanli girdide              -> 2
```

***Bir sayma komutunu, sıfır dönmesi gereken bir girdide sınamadan
kullanma.*** Bu turda yanlış teşhis tam olarak bu adımın atlanmasından doğdu.

Hangi karonun nereye düştüğü de rastgele değil, `BoardAdapter.cs:702`
formülünden çıkıyor ve her Play'de aynı:

```
   dizin = (x * 7 + y * 13) % 4          x sutun, y satir

     y=4 |  0   3   2
     y=3 |  3   2   1
     y=2 |  2   1   0
     y=1 |  1   0   3
     y=0 |  0   3   2
         +------------
            x=0 x=1 x=2

   OZ-SINAMA: dizi tek elemanli olsa butun hucreler 0 dizinini alirdi
   (dogrulandi); dort elemanli oldugunda dort dizin de kullaniliyor
   (dogrulandi). Formul gercekten dagitiyor.
```

### Gerçekten kırık olan tek şey

Sahne dosyası `BoardAdapter`'ın **on üç** alanından yalnız **dördünü** taşıyor.
Sayım komutu ve bugünkü çıktısı:

```
sed -n '/guid: 99975536c95574b4c9004444d6bc33a6/,/^--- /p' Assets/Scenes/SampleScene.unity \
  | grep -cE '^  (width|height|terrainSprites|unitPrefab|maxHealth|damage|attackRange|moveRange|placementGhost|dragThreshold|placementModeKey|placementCancelKey|structureMaxHealth):'
   -> 4          (yazili olanlar: width . height . terrainSprites . unitPrefab)
```

Eksik dokuzun sekizi bir **sayı** ya da bir **enum** alanı ve sekizinin de C#
tarafında bir alan başlatıcısı var. Bu başlatıcıların yaşayıp yaşamadığı bu
dosyanın en pahalı sorusudur; ölçüldü ve cevabı yukarıda, "Sahnede YAZILI
OLMAYAN bir alan hangi değeri alır" başlığı altında. Kısası: ***yaşıyorlar.***

Dokuzuncu alan ise bir **referans** alanı ve başlatıcısı yok — yani o, ölçümün
hangi tarafa düştüğünden **bağımsız** olarak boş:

```
BoardAdapter.cs:161   [SerializeField] private SpriteRenderer placementGhost;
```

İkinci ve daha güçlü ölçü şu: sahnede referans verilebilecek bir çizici zaten
**hiç yok**.

```
grep -c "^SpriteRenderer:" Assets/Scenes/SampleScene.unity     -> 0
grep -c "^SpriteRenderer:" Assets/Game/Prefabs/Unit.prefab     -> 2   (bilinen-iyi kontrol)
```

Prefab üstündeki iki çizici, komutun boşa düşmediğini kanıtlıyor: desen
çalışıyor, sahnede gerçekten sıfır tane var. ***Yani `placementGhost` bugün
sadece atanmamış değil; atanabileceği bir aday da yok. Onarım bir sürükleme
değil, önce bir NESNE YARATMA işidir.***

### Bunun DURMA NOKTASI 4 ve `08` için anlamı

[`08-unity-altyapisi.md`](08-unity-altyapisi.md)'nin Editör listesi bu soruyu
açık bırakmıştı: `B` tuşuna basınca iki kırmızı satırdan hangisi çıkacak?
Yukarıdaki ölçü onu kapatıyor.

```
ONARIMDAN ONCE  -> [Board] Cannot enter placement mode: placementGhost is not assigned.
                   kip HIC acilmaz . DURMA NOKTASI 4'un ikinci senaryosu BASLAMAZ

ONARIMDAN SONRA -> ArgumentException: The unit is already in this battle.
                   Parameter name: unit
                   kip ACILIR, hayalet gorunur, ve BIRAKMA karesinde patlar
```

***Onarım kusuru düzeltmiyor; kusuru ULAŞILABİLİR yapıyor.*** İkisi farklı
şeyler ve karışması pahalı: `00-okuma-sirasi.md` yerleştirme kusurunu bu turda
düzeltmeyi bilerek reddediyor, çünkü o istisnayı Play'de görmek turun en
öğretici on dakikası. Bu dosya o on dakikayı **mümkün** kılar, iptal etmez.

---

## Onarım — pencere pencere

Beş adım. Her adımın sonunda ne göreceğin yazılı; görmediysen o adımda dur.

**Hazırlık:** Unity Hub'dan projeyi aç. Project penceresinde
`Assets/Scenes/SampleScene.unity` dosyasına **çift** tıkla. Hierarchy
penceresinde tam iki kök nesne olmalı: `Main Camera` ve `Board`.

### ADIM A · Hayalet nesnesini yarat

Hierarchy penceresinde `Board` nesnesine **sağ** tıkla → `Create Empty`. Yeni
çocuk nesne `Board`'un altında doğar. Adına **`PlacementGhost`** yaz (F2 ya da
Inspector'ın en üstündeki ad kutusu).

**Görünür sonuç:** Hierarchy'de `Board`'un solunda bir açma üçgeni belirir ve
altında `PlacementGhost` durur. `Board` seçiliyken Inspector'da hâlâ üç bileşen
görünür (`Transform` · `Grid` · `Board Adapter`); yeni nesne ayrı bir satırdır,
`Board`'un bileşeni değil.

**Dur ve rapor:** Nesne `Board`'un **altında** değil de kökte doğduysa,
Hierarchy'de `PlacementGhost`'u tutup `Board`'un üstüne sürükleyerek çocuk yap.
Kod bunu zorunlu tutmuyor — konumu her karede dünya koordinatıyla yazıyor — ama
alanın kendi `Header` metni "child SpriteRenderer" diyor ve tahtayı tek çağrıda
yok etmek isteyen `SetParent` kararıyla tutarlı kalması iyidir.

### ADIM B · Çizici bileşenini ekle

Hierarchy'de `PlacementGhost` seçiliyken Inspector penceresinin en altındaki
**`Add Component`** düğmesine bas, arama kutusuna `Sprite Renderer` yaz ve
listeden seç.

**Görünür sonuç:** Inspector'da `Transform`'un altında `Sprite Renderer`
bileşeni belirir. `Sprite` alanı `None (Sprite)` yazar.

**Dur ve rapor:** Bileşen listesinde `Sprite Renderer` yerine `Sprite Mask` ya
da `Sprite Shape` seçtiysen dur; `placementGhost` alanının tipi tam olarak
`SpriteRenderer` ve Inspector başka bir tipi kabul etmez.

### ADIM C · Hayalete bir sprite ver

Project penceresinde
`Assets/Art/ThirdParty/Kenney/TinyBattle/Buildings/` klasörüne git.
`friendly_command_depot_tile_0045.png` dosyasını, `PlacementGhost` seçiliyken
Inspector'daki `Sprite Renderer` → **Sprite** alanına sürükle.

Sonra aynı bileşende **Sorting Order** alanına `1` yaz.

**Görünür sonuç:** Scene penceresinde küçük bir bina karosu belirir. Inspector'da
`Sprite` alanı artık dosya adını gösterir.

**Dur ve rapor:** `.png` sürüklendiğinde alan kabul etmiyorsa, Project'te
dosyanın solundaki açma üçgenine bas ve **içindeki** `Sprite` alt varlığını
sürükle. `Sorting Order` neden `1`: zemin karoları `0` alıyor
(`BoardAdapter.cs:781`), kodla doğan yapılar `1`
(`BoardAdapter.cs:576`), prefab'daki birim gövdesi `2`. Hayalet `0` kalsaydı
zeminle aynı katmanda çizilir ve hangisinin üstte olacağı belirsizleşirdi.

### ADIM D · Alanı bağla

Hierarchy'de **`Board`** nesnesine tıkla. Inspector'da `Board Adapter (Script)`
bileşenini bul. İçinde **Placement Ghost** başlıklı, `None (Sprite Renderer)`
yazan bir alan var. Hierarchy'den `PlacementGhost` nesnesini tutup **o alanın
üstüne bırak**.

**Görünür sonuç:** Alan artık `PlacementGhost (Sprite Renderer)` yazar.

**Dur ve rapor:** Alan sürüklemeyi reddediyorsa iki sebep olabilir ve ikisi de
ölçülebilir: ya nesnede `Sprite Renderer` yok (ADIM B atlanmış), ya da Project
penceresinden bir **varlık** sürüklenmiş. Alan bir **sahne** nesnesi ister.

### ADIM E · On üç alanı gözle doğrula, sonra kaydet

`Board` seçiliyken Inspector'daki `Board Adapter (Script)` bileşeninin **bütün**
alanlarına bak ve şu değerleri gör:

```
Width 3 . Height 5 . Terrain Sprites 4 eleman . Unit Prefab dolu
Max Health 30 . Damage 10 . Attack Range 1 . Move Range 1
Placement Ghost dolu (ADIM D)
Drag Threshold 0.25 . Placement Mode Key B . Placement Cancel Key Escape
Structure Max Health 50
```

Sıfır ya da `None` gördüğün her alana ***doğru değeri elle yaz***. Yukarıdaki
sayılar, alan tablosundaki **Tanım** sütununun gösterdiği satırlarda duran C#
alan başlatıcılarının birebir aynısıdır — ikisi örnek:

```
BoardAdapter.cs:136   [SerializeField, Min(1)] private int maxHealth = 30;
BoardAdapter.cs:172   [SerializeField] private KeyCode placementModeKey = KeyCode.B;
```

Sonra `File` → `Save` (`Ctrl + S`).

**Görünür sonuç:** Hierarchy'deki sahne adının yanındaki `*` işareti kaybolur.

***Bu adım soruyu kalıcı olarak kapatır.*** Kaydettikten sonra sahne dosyası on
üç alanın **hepsini** taşır; ADIM D öncesindeki `4` sayısı `13` olur. O andan
sonra "başlatıcı mı yaşıyor, `default(T)` mi kalıyor" sorusunun bu sahne için
bir hükmü kalmaz — çünkü hiçbir alan artık eksik değildir. Sayım komutu yukarıda,
"Gerçekten kırık olan tek şey" başlığının altında.

---

## Doğrulama — beş adımın ardından ne görülmeli

### ① Play'e bas ve Console'u oku

Console penceresini aç (`Window` → `General` → `Console`), üstteki üç süzgecin
(hata · uyarı · bilgi) üçü de açık olsun, sonra araç çubuğundan ▶.

```
BEKLENEN ILK SATIR
   [Board] built 3x5 = 15 cells.

BEKLENEN KIRMIZI SATIR SAYISI
   0

ONARIMDAN ONCE burada tam BIR kirmizi satir vardi ve kaynagi:
BoardAdapter.cs:312                       "[Board] placementGhost is not assigned. Assign a child SpriteRenderer; structure placement mode will refuse to start without it.",
```

***Bu tek satırın kaybolması, ADIM A-E'nin tuttuğunun kanıtıdır.*** Hâlâ
görünüyorsa alan bağlanmamıştır; ADIM D'ye dön.

Console'da bunun yerine şu satırı görürsen tablo başka bir şey söylüyor:

```
ArgumentOutOfRangeException: Max health must be positive.
   >> maxHealth SIFIR gelmis: sahnede yazili degil ve baslatici da yasamamis.
   >> Yani HUKUM B yururlukte. ADIM E'ye don, alanlari ELLE doldur, kaydet.
```

### ② Tahtayı ve birimleri gör

Game penceresinde 3 sütun × 5 satır toprak karo ve iki asker figürü olmalı.
Askerler `(1,2)` ve `(1,3)` hücrelerinde, yani orta sütunda alt alta.

**İkisinin aynı göründüğüne şaşırma ve bunu bir arıza sanma.** Ölçü: her iki
birim de aynı `unitPrefab`'tan doğuyor ve prefab'ın gövde sprite'ı tek:
`friendly_vanguard_infantry_tile_0142.png`. Taraf bilgisi `Combatant` içinde
yaşıyor, ekranda değil. Taraflar ekranda ayrışsın istendiği gün gereken şey
belli: `SpawnUnit`'in aldığı `Team` değerine göre farklı bir prefab ya da farklı
bir sprite seçen bir alan. O gün gelene kadar iki asker ikizdir.

### ③ Dört dalı sına — ADIM 8'in niyet tablosu

```
bir askere tikla       -> secim cercevesi acilir
                          Console: [Board] (1,2) holds 'Vanguard' - SELECTED.
bos hucreye tikla      -> asker yurur
                          Console: [Board] 'Vanguard' moved to (0,2).
otekine tikla          -> vurur
                          Console: [Board] 'Raider' at (1,3) was hit. health=20, state=Alive
ayni askere tekrar     -> secim birakilir
                          Console: [Board] (0,2) holds the selected unit - DESELECTED.
```

Hareket bir hücreden uzağa gitmiyorsa sebebi `moveRange` alanıdır ve varsayılan
değeri `1`'dir; bu bir arıza değil.

### ④ `B` tuşunu dene — ve beklenen istisnayı gör

Bir birim seçiliyken klavyeden `B`.

***Bu tuş aynı zamanda bir SINAMADIR.*** `B` hiçbir şey yapmıyorsa — Console
sessiz, hayalet yok, hata yok — sorun onarımda değil, `placementModeKey`
alanındadır ve yukarıdaki HÜKÜM B yürürlükte demektir. O durumda Inspector'da
`Placement Mode Key` alanını aç ve elle `B` seç, sonra sahneyi yeniden kaydet.

```
BEKLENEN
   [Board] Placement mode ON for 'Vanguard'. Drag and release to place, or click to carry.
   + Game penceresinde hayalet karo fareyi izlemeye baslar

SONRA: tahta ICINDEKI bos bir hucreye surukleyip birak
   ArgumentException: The unit is already in this battle.
   Parameter name: unit
```

***Bu istisna beklenen sonuçtur ve senin hatan değildir.*** Zincirin ölçülmüş
hâli ve neden başarılı olması gereken tek dalın patladığı
[`00-okuma-sirasi.md`](00-okuma-sirasi.md)'nın DURMA NOKTASI 4 bölümünde satır
satır yazılı. ***Düzeltmeyi bu turda yapma.***

Ayrıca iptal yolunu da sına: `B` ile kipe gir, `Escape`'e bas.

```
BEKLENEN
   [Board] Placement mode CANCELLED. The board was not touched.
   + hayalet kaybolur, tahtada hicbir sey degismez
```

### ⑤ Play'den çık ve kalıcılığı doğrula

▶ düğmesine tekrar basıp Play'den çık. Hierarchy'de on beş `Cell_x_y` nesnesi
ve iki `Unit_...` nesnesi kaybolur; `PlacementGhost` **kalır**.

***Ayrım kuralın kendisidir:*** Play sırasında kodun ürettiği nesneler Play
bitince yok olur; Play **dışında** Editör'de yaptığın ve kaydettiğin değişiklik
kalır. Bu yüzden ADIM A-E'nin hiçbiri Play sırasında yapılmaz.

---

## `.meta` ve GUID — neden Explorer'dan dosya taşınmaz

Sahne dosyası hiçbir varlığı **yoluyla** anmaz. Yedi tane `guid:` satırı taşır
ve bağlar bunların üstünde kurulur:

```
Assets/Scenes/SampleScene.unity
   m_Script:       {fileID: 11500000, guid: 99975536c95574b4c9004444d6bc33a6, type: 3}
   unitPrefab:     {fileID: 220021581834759902, guid: eccbfd11703739d47803cc41b3adf540, type: 3}
   terrainSprites: 4 eleman, dordu de ayri guid

Karsiliklari .meta dosyalarinda yasiyor:
   Assets/Game/Unity/BoardAdapter.cs.meta   guid: 99975536c95574b4c9004444d6bc33a6
   Assets/Game/Prefabs/Unit.prefab.meta     guid: eccbfd11703739d47803cc41b3adf540
                                                  ^^^ AYNI 32 karakter
```

Doğrulama komutu ve bilinen-kötü karşılığı:

```
grep -rl "guid: 6cb1936eb8bc475386243b8168a2815f" Assets --include=*.meta
   -> .../TinyTown/Terrain/Dirt/dirt_fill_scatter_a_tile_0039.png.meta

grep -rl "guid: deadbeefdeadbeefdeadbeefdeadbeef" Assets --include=*.meta
   -> hic eslesme                   (komutun bos donebildiginin kaniti)
```

***Sonuç doğrudan bir prosedür kuralı üretir.*** Bir dosyayı Windows
Explorer'dan taşırsan `.png` gider, `.png.meta` yerinde kalır. Unity taşınan
dosyayı yeni bir varlık sanar, ona **yeni** bir GUID üretir, ve o GUID'e işaret
eden her satır kopar: sahnedeki `terrainSprites` elemanı `Missing`'e döner,
`m_Script` bağı kopan bir betikte bileşen `Missing (Mono Script)` olur ve
Inspector'daki bütün değerler görünmez olur.

**Kural:** Unity varlıkları **Project penceresinden** taşınır, silinir ve
yeniden adlandırılır. Project penceresi `.meta` dosyasını varlıkla birlikte
taşır; Explorer taşımaz.

`Assets/` altında bugün 114 `.meta` var (`find Assets -name "*.meta" | wc -l`)
ve hepsi depoda. Serileştirmenin arkasında ne olduğu, `.meta` içinde GUID'in
yanında başka ne durduğu ve maliyetinin ne olduğu
[`08-unity-altyapisi.md`](08-unity-altyapisi.md)'de anlatılı; bu dosya yalnızca
prosedür tarafını yazıyor.

---

## Seçilen / reddedilen

## SEÇİLEN
`ADIM 8` → ***bu dosya (ADIM 8b)*** → `DURMA NOKTASI 4` → `ADIM 9`.
Sahne onarımı **operatörün eliyle**, Editör'de yapılır.

## REDDEDİLEN 1 · Sahneyi bu turda dosya düzeyinde onarmak
`SampleScene.unity` bir metin dosyası ve `placementGhost` satırı elle
yazılabilirdi. Reddedildi: sahneye referans verilecek `SpriteRenderer` **yok**
(ölçüldü: sahnede sıfır tane), yani onarım bir satır değil dört nesne/bileşen
değişikliği demek. Elle yazılan `fileID` değerlerinin tutarlılığını hiçbir kapı
denetlemiyor ve tutmadığı gün hata Inspector'da `Missing` olarak, sebebi
görünmeden çıkar.

## REDDEDİLEN 2 · Yerleştirme kusurunu bu dosyada düzeltmek
`00-okuma-sirasi.md` bunu zaten reddetti ve gerekçesi ölçülü: istisnayı Play'de
görmek, *"ikisi çelişirse kod kazanır"* kuralının koşturulabilir tek örneği. Bu
dosya kusuru **ulaşılabilir** kılıyor; bu, düzeltmenin tersidir.

## REDDEDİLEN 3 · Bu dosyayı `ADIM 9`'un ardına koymak
İlk bakışta doğru görünür: motor tarafı `konular/08`'de anlatılıyor. Ama DURMA
NOKTASI 4, ADIM 9'un **önünde** Play'e bastırıyor. Sonraya konsaydı operatör
önce kırık sahneye çarpar, tamir aletini sonra bulurdu.

## REDDEDİLEN 4 · `08-unity-altyapisi.md`'nin sekiz adımını buraya kopyalamak
O liste serileştirmenin **canlı hâlini** gösteriyor; bu dosya bir kurulum
tarifi. Kopya, iki yerde iki ayrı doğruluk kaynağı üretir ve biri değiştiği gün
öteki sessizce yalan söyler. Burada yalnızca `08`'in açık bıraktığı tek soru
kapatıldı: `B` tuşunun hangi kırmızı satırı ürettiği.

## REDDEDİLEN 5 · "Sahnede olmayan alan `default(T)` kalır" hükmünü yazmak
Bu tura iki kez, iki ayrı kaynaktan geldi ve ikna edici bir zinciri vardı:
`placementModeKey` sahnede yazılı değil → `KeyCode.None` → `B` tuşu ölü. Zincir
mantıklı ama **ilk halkası ölçülmemişti.** Ölçüldüğünde çürüdü: `Unit.prefab`'ın
içe aktarılmış verisinde, prefab YAML'inde **hiç bulunmayan** `downedTint` ve
`deadTint` alanları C# başlatıcı değerlerini bayt bayt taşıyor. Ölçü yukarıda.
***Reddedilen şey hükmün kendisi değil, ölçülmeden yazılmasıydı*** — bu yüzden
karşı hüküm silinmedi, üç kademeli tabloda bir tehdit envanteri olarak duruyor
ve operatörün onu bir Play'de ayırt etmesi sağlandı.

## REDDEDİLEN 6 · Alan tablosunu `deep/kod/Unity/BoardAdapter.md`'ye eklemek
Ayna belgeler "bu üye neden böyle" sorusunu cevaplar; buradaki tablo "bu alana
ne sürüklenir" sorusunu cevaplar. İkinci soru koda değil **pencereye** dair ve
kod değişmeden de bayatlayabilir (Unity sürümü değiştiğinde). Ayrı sahip, ayrı
dosya.

---

## İlgili

- Bu adımın öncesi ve sonrası: [00-okuma-sirasi.md](00-okuma-sirasi.md)
- Serileştirmenin arkasında ne var: [08-unity-altyapisi.md](08-unity-altyapisi.md)
- Tıklamanın eyleme dönüşmesi: [`konular/07`](../deep/konular/07-tiklamadan-eyleme.md)
- Alanların üye düzeyinde gerekçeleri: [`deep/kod/Unity/BoardAdapter.md`](../deep/kod/Unity/BoardAdapter.md) · [`deep/kod/Unity/UnitView.md`](../deep/kod/Unity/UnitView.md)
- Bu ağacın yönlendirmesi: [README.md](README.md)
- Üst düzey belge haritası: [../README.md](../README.md)
