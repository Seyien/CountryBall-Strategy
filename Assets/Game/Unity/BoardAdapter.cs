using System;
using System.Collections.Generic;
using GridStrategy.Battle;
using GridStrategy.Combat;
using GridStrategy.Core;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

namespace GridStrategy.Unity
{
    // ██ BURADAN ÖĞRENMEYE BAŞLIYORSAN: Docs/ogrenme/00-okuma-sirasi.md ██
    // Dosya numaraları SIRA DEĞİL, kimliktir. Doğru sıra orada: 14 adım, 5 oturum.
    // Burası oyunun motor tarafındaki giriş noktası; okuma sırasının başı değil.

    // ═══ ÇIPLAK "Battle" YAZMAK BU DOSYADA DERLEME HATASIDIR (CS0118) ═══
    //
    // ── HARİTA: derleyicinin gördüğü ad ağacı ────────────────────────
    // Bu ağacı KLASÖRLER kurmaz; yalnızca `namespace` satırları kurar.
    // Kanıt aynı projede: Core/Combat/ klasörü diskte Core'un İÇİNDEdir
    // ama ad alanı GridStrategy.Combat'tır — yani Core'un KARDEŞİ.
    //
    //   global::
    //   └── GridStrategy
    //       ├── Battle           ◄── ① AD ALANI
    //       │   ├── Battle       ◄── ② SINIF  (bu dosyanın istediği)
    //       │   ├── BattleActions
    //       │   └── PlacementOutcome
    //       ├── Combat
    //       ├── Core
    //       └── Unity            ◄── BU DOSYA BURADA YAŞIYOR
    //           ├── BoardAdapter
    //           └── UnitView
    //
    // Tek "Battle" kelimesi iki ayrı şeyi adlandırıyor: ① bir ad alanı,
    // ② onun içindeki bir sınıf. Çıplak yazıldığında ① kazanır.
    //
    // ── ARAMA SIRASI: ① neden kazanıyor ──────────────────────────────
    //   SEVİYE 1   GridStrategy.Unity üyeleri: BoardAdapter, UnitView   ✗
    //   SEVİYE 1b  bu ad alanı gövdesindeki using/alias   ◄── ALIAS BURADA
    //   SEVİYE 2   GridStrategy üyeleri: Battle, Combat, Core, Unity    ✓
    //              ██ ARAMA BİTTİ ██ bulunan bir AD ALANI, tip değil
    //                                              ──────► CS0118
    //   SEVİYE 3   dosya başındaki using'ler   ── BURAYA HİÇ GELİNMEZ
    //
    // ALIAS'IN YERİ KURALIN KENDİSİDİR: gövdede durduğu için SEVİYE 1b'de
    // yakalanır ve arama SEVİYE 2'ye hiç çıkmaz. Dosyanın başına, öteki
    // using'lerin yanına taşınsaydı SEVİYE 3'e düşerdi ve CS0118 geri
    // gelirdi — metin harfi harfine aynı, sonuç zıt.
    // → BoardAdapter.md#cs0118-alias
    using Battle = global::GridStrategy.Battle.Battle;

    // ═══ GİRİŞ OKUMA NOTU — SOL TUŞU OKUYAN TEK TİP BU ═══════════════
    // ÖLÇÜSÜ ŞU: bütün projede Input.GetMouseButton ile başlayan altı çağrı
    // var ve düğme 0'ı soran ALTISI da bu dosyada; BoardCameraRig sol tuşu
    // HİÇ okumuyor ve okuyamıyor (panButton alanı [Min(1)]). Çakışmanın
    // imkânsızlığı bir söz değil, bir grep sonucudur.
    //
    // SIRADAN TAHTA TIKLAMASI — üçü birden gerekir ve bu YENİ: eskiden
    // Update'teki tek bir GetMouseButtonDown doğrudan HandleClick'e gidiyordu.
    // Sol tuş artık haritayı da kaydırdığı için karar BASMA karesinde
    // verilemez; AdvancePointer üç sorguyu BoardPointerArbiter'a veriyor ve
    // tıklama ancak BIRAKMA karesinde doğuyor.
    //
    // YERLEŞTİRME KİPİ — yine üçü birden: FeedGesture'ı yalnız
    // StructurePlacementMode.Advance çağırıyor. İki akış aynı karede ASLA
    // birlikte koşmaz, çünkü Update'te aralarında şu satır duruyor:
    //     if (modeOwnsPointer) { ... return; }
    // Bir tıklama ile bir sürükleme BAŞLANGIÇTA aynıdır: Down yalnız
    // basıldığı kareyi, GetMouseButton basılı geçen HER kareyi, Up yalnız
    // bırakıldığı kareyi görür — ayrımı ancak ortadaki sorgu üretir.
    //
    // Ayrımın KARARI yine de burada değil: GridStrategy.Core.PointerGesture
    // eşiği ölçüyor, BoardPointerArbiter onu bir eyleme çeviriyor.
    // → BoardAdapter.md#girdi-okuma-notu
    //
    // ═══ ROL: KARMA — ÇEVİRMEN + VARLIK (Adapter + Entity) ═══════════
    // kimlik : var — ölçüsü şu: aynı sahneye İKİ BoardAdapter koy, İKİ AYRI
    //          savaş doğar; battle, unitViews ve selectedUnit'in üçü de örnek
    //          alanıdır, paylaşılan tek bir static alan YOK
    // hafıza : var — ölçüsü şu: AYNI dolu hücreye arka arkaya İKİ kez tıkla,
    //          iki FARKLI şey olur: birincisi birimi SEÇER, ikincisi seçimi
    //          BIRAKIR. Farkı üreten şey selectedUnit'in tıklamalar arasında
    //          yaşamasıdır — çeviri durumu değil, bir OYUN durumudur
    // Unity  : zorunlu — ESKİ ÖLÇÜ YANLIŞ, düzeltildi: BoardAdapterTests artık
    //          VAR ama bu tipi `new` ile kuramıyor; Awake EditMode'da koşmadığı
    //          için alanları YANSIMAYLA dolduruyor, Input ve Camera'ya girmiyor
    // karar  : ikisi birden — piksel→hücre çevirisi (çevirmen işi) ile
    //          "aynı anda tek birim seçili" ve "dolu hücreye tıklamak SALDIRI,
    //          boş hücreye tıklamak HAREKET demektir" kuralları (varlık işi)
    //          aynı tipte
    // KOKU   : evet ve BÜYÜDÜ. EŞİK AŞILDI — ve notu SİLMİYORUM, çünkü bir
    //          eşiğin aşıldığını söyleyen satır, eşiği koyan satır kadar
    //          öğreticidir. Madde #10 bütün bir GİRİŞ KİPİ ekledi ve eşiği
    //          aştı. NASIL KARŞILANDI, ve yarısıyla: jest dışarı çıktı
    //          (PointerGesture), tıklamanın NİYETE çevrilmesi hâlâ burada.
    //          SIRADAKİ EŞİK → BoardAdapter.md#rol
    /// <summary>
    /// Unity dünyası ile motordan bağımsız savaş kuralları arasındaki çevirmen.
    /// KENDİ KURALI DA VAR ve künyedeki "karar: ikisi birden" satırı tam olarak
    /// bunu söylüyor: tıklamanın NİYETE çevrilmesi ÜÇ kararla burada olur —
    /// dolu hücreye tıklamak SALDIRI, boş hücreye tıklamak HAREKET, seçili
    /// birimin KENDİ üstüne tıklamak SEÇİMİ BIRAKIR demektir. Bu niyetin
    /// GEÇERLİ olup olmadığını ise burası bilmez; onu <see cref="Battle"/> ve
    /// <see cref="BattleActions"/> nesnelerine sorar.
    ///
    /// Birim başına GÖRSEL durum artık burada değil, <see cref="UnitView"/>
    /// içinde yaşıyor - o baskı gerçekten doğdu ve bölündü. Buna karşılık input
    /// okuma ve zemin kurulumu hâlâ burada: ikisi de bağımsız değişme baskısı
    /// üretmedi.
    ///
    /// TAHTA ARTIK BURADA DEĞİL. Bu tipte bir <see cref="UnitGrid"/> alanı
    /// vardı; o alan silindi çünkü <see cref="Battle"/> tahtayı kendisi
    /// sahipleniyor.
    ///
    /// GEREKÇELER: Docs/deep/kod/Unity/BoardAdapter.md
    /// </summary>
    // İKİ HOST SÖZLEŞMESİ DAHA, ve ikisi de AŞAĞI bakıyor: kipler bu tipi
    // adıyla hiç tanımıyor, yalnız arayüzünden konuşuyor. Ölçüsü şu — üç kip de
    // EditMode'da sahte bir host ile `new` edilip sınanabiliyor.
    // → Modes/IBoardModeHost.cs
    [RequireComponent(typeof(Grid))]
    public sealed class BoardAdapter : MonoBehaviour, IPlacementBoard,
        IPlacementModeHost, IUnitOrderHost
    {
        [Header("Board size in CELLS, not world units")]
        [SerializeField, Min(1)] private int width = 3;
        [SerializeField, Min(1)] private int height = 5;

        [Header("Terrain sprites - at least one required")]
        [SerializeField] private Sprite[] terrainSprites;

        // Alan tipi GameObject değil UnitView: Inspector artık UnitView
        // TAŞIMAYAN bir prefab'ı kabul etmez, yani "prefab'a bileşen eklemeyi
        // unuttum" hatası Play'e basmadan yakalanır.
        // → BoardAdapter.md#unitprefab
        [Header("Unit prefab")]
        [SerializeField] private UnitView unitPrefab;

        // BİRİM SAYILARI NEREDEN GELİYOR: buradan, düz [SerializeField] olarak.
        // Seçenekleri ayıran şey "kim okur" değil DOSYAYI KİM ÜRETİR — const bir
        // derleme turu ister, .asset ise koddan DOĞMAYAN bir dosyadır ve
        // atanmadığı gün sahneyi bozar; sahne alanı ise zaten var olan bir
        // bileşene yazılır. KAPSAM: sahibi başka yerde olan sayı serileştirilmez
        // (karşı örnek NewCombatant'taki yaşam döngüsü pencereleri).
        // → BoardAdapter.md#maxhealth-damage-attackrange
        [Header("Unit stats - applied to every spawned unit")]
        [Tooltip("Starting and maximum health of each spawned unit.")]
        [SerializeField, Min(1)] private int maxHealth = 30;

        [Tooltip("Raw damage of a single hit, before any resistance.")]
        [SerializeField, Min(0)] private int damage = 10;

        [Tooltip("How many cells away a unit can strike. Must be at least 1.")]
        [SerializeField, Min(1)] private int attackRange = 1;

        // BEKLEME SÜRESİ OLMADAN VURUŞ SINIRSIZDIR: sıfır geçildiğinde savaşçı
        // aynı hedefe kare başına vurabilir ve oyuncunun hızlı tıklaması hasarı
        // yığar. Sayı burada, çünkü bu yolla doğan birimlerin (demo doğuşu)
        // canı ve hasarı da burada; üretim yolundan gelenlerin sahibi ise kendi
        // varlık dosyası. → Combatant.AttackCooldownRemaining
        [Tooltip("Seconds a spawned unit waits between two strikes. 0 means no limit at all.")]
        [SerializeField, Min(0f)] private float attackCooldownSeconds = 1f;

        // SIRA KİPİ: Alternating klasik sıra tabanlı oyun, FreeForAll ise
        // herkesin istediği an oynayabildiği serbest kip.
        //
        // OYUNDA NE İŞE YARAR: serbest kipte oyuncu uzaktaki bir düşmana
        // tıkladığında savaşçı yanına yürür ve VARDIĞI AN vurur; sıra tabanlı
        // kipte yürümek hakkı bitirdiği için vuruş ikinci tıklamaya kalır.
        // Varsayılan serbest, çünkü tek başına denerken sıranın karşı tarafa
        // geçmesi oyuncuyu hiçbir şey öğretmeden bekletiyordu.
        [Header("Turn mode - FreeForAll lets anyone act at any time")]
        [SerializeField] private TurnMode turnMode = TurnMode.FreeForAll;

        // Birimin YÜRÜME HIZI: saniyede kaç hücre. Oyuncu bu sayıyı büyütünce
        // savaşçı gözle görülür biçimde hızlanır; küçültünce ağırlaşır.
        //
        // "Hareket menzili" alanının YERİNE geçti. Menzil, oyuncunun tıklayabildiği
        // yeri birimin çevresindeki birkaç hücreye hapsediyordu; artık haritanın
        // her yerine tıklanabilir ve tek kısıt oraya bir YOLUN bulunmasıdır
        // (PathFinder). Menzil kuralı Core'da duruyor, tahta onu artık sormuyor.
        [Tooltip("Walking speed in cells per second. Cells are 1 unit wide, so 3 means three cells a second.")]
        [SerializeField, Min(0.1f)] private float moveSpeed = 3f;

        // ═══ YERLEŞTİRME KİPİ (#10) ══════════════════════════════════
        // HAYALET GERÇEK BİR Structure DEĞİLDİR: tahtaya girmez, Battle onu
        // bilmez, hiçbir kural onu görmez. Önizleme şeridi tahtaya HİÇ yazmaz;
        // yazma yalnız bırakma anında, BİR kez olur. Hayalet gerçek yapılsaydı
        // savaşın KAYDI imleç hareketiyle mutasyona uğrar, iptal yolu ZORUNLU
        // hâle gelir ve unutulduğu gün tahtada hücreyi kapatan, hedeflenebilen,
        // GÖRÜNMEZ bir bina kalırdı. → BoardAdapter.md#placementghost
        [Header("Placement ghost - assign a child SpriteRenderer, kept disabled at rest")]
        [SerializeField] private SpriteRenderer placementGhost;

        // EŞİK DÜNYA BİRİMİNDE, PİKSELDE DEĞİL — ve bu bir karardır. Piksel
        // seçilseydi aynı parmak hareketi 1920'lik ekranda "tıklama", 2560'lık
        // ekranda "sürükleme" sayılırdı. Dünya birimi ayrıca ÖLÇÜLEBİLİR bir
        // anlam taşır: 0,25 "çeyrek hücre". → BoardAdapter.md#dragthreshold
        [Header("Pointer gesture")]
        [Tooltip("How far the pointer must travel, in WORLD units, before a press counts as a drag.")]
        [SerializeField, Min(0f)] private float dragThreshold = 0.25f;

        // ██ HARİTAYI KAYDIRAN KAMERA — BOŞ BIRAKILIRSA KENDİ BULUNUR ██
        // OYUNDA NE İŞE YARAR: sol tuşla sürüklendiğinde haritanın kayacağı
        // kamera. Boş bırakılırsa MainCamera etiketli kameranın üstündeki
        // BoardCameraRig aranır — sahne kurulum aracı rig'i tam oraya koyuyor,
        // yani bugünkü sahne hiçbir elle bağlama istemeden çalışır.
        // ALAN YİNE DE VAR: iki kameralı bir sahnede hangisinin kayacağını
        // aramaya bırakmak, sessizce yanlış kamerayı kaydırmak olurdu.
        // BOŞ VE BULUNAMAZ ise kaydırma olmaz, tıklama aynen çalışır.
        [Header("Map panning - leave empty to use the MainCamera rig")]
        [Tooltip("Camera rig that the left-drag pans. Empty means: find it on the MainCamera.")]
        [SerializeField] private BoardCameraRig cameraRig;

        [Tooltip("Key that enters structure placement mode. Requires a selected unit.")]
        [SerializeField] private KeyCode placementModeKey = KeyCode.B;

        [Tooltip("Key that cancels structure placement mode without touching the board.")]
        [SerializeField] private KeyCode placementCancelKey = KeyCode.Escape;

        // DİRİLTME KENDİ KİPİNİ AÇMIYOR ve gerekçe İKİNCİ KEZ DEĞİŞTİ: önce
        // "hatırlanacak bir şey yok" deniyordu, sonra "var olan bekleyen-emir
        // kipi o hafızayı taşıyor, cinsini bir bool ayırıyor" oldu. Bugün ne
        // kip var ne bool: hafızayı emrin KENDİ TİPİ taşıyor ve kaldırma, kip
        // makinesine hiç uğramayan ayrı bir emir sınıfı. → Orders/ReviveOrder.cs
        // Seçili birimi/yapıyı kaldıran tuş. Arayüzdeki çöp kutusu düğmesiyle
        // AYNI işi yapar; düğme henüz sahnede yoksa oyun yine de oynanabilsin
        // diye var.
        [Header("Remove selected - same action as the trash button")]
        [SerializeField] private KeyCode removeSelectedKey = KeyCode.Delete;

        /// <summary>
        /// Paletten hiçbir ölçü gelmediğinde yapının kaç hücre kapladığı.
        /// </summary>
        // ESKİ `structureScale` ALANI KALDIRILDI ve gerekçesi sahiplik: ölçünün
        // yeni sahibi yapının kendi tanım varlığı, ve tek bir Inspector sayısı
        // bütün bina türlerine aynı boyu verirdi. Sahnede kalan eski anahtarı
        // Unity sessizce atar; ekranda görünen tek fark, varsayılanın 1,6'dan
        // 1,25'e inmesidir.
        //
        // public VE BU BİR KARAR: sahne kurulum aracı hayaleti aynı ölçüyle
        // çizmek zorunda ve sayıyı kendi dosyasında tekrar yazsaydı ikisi bir gün
        // ayrışır, önizleme ile kurulan bina farklı boyda çıkardı. Sabitin tek
        // sahibi çalışma anı, yani burası.
        public const float DefaultStructureSizeInCells = 1.25f;

        // ÇİMİN DIŞI ARTIK BOŞLUK DEĞİL. Ölçü şu: yapı görseli bir hücreden
        // BÜYÜK çiziliyor (varsayılan 1,25 hücre), yani kenardaki bir bina
        // hücresinden her yöne taşıyor ve taşan kısmının altında hiçbir zemin
        // bulamıyor. Halka o taşmayı bir zemine oturtur ve oyuncuya tahtanın
        // nerede bittiğini gösterir.
        //
        // HALKA OYUNUN KURALINA DAHİL DEĞİL: sınırın tek sahibi hâlâ
        // battle.IsInsideGrid, halka hücrelerine hiçbir şey konamaz. Dizi boş
        // bırakılırsa halka çizilmez ve bugünkü sahneler aynen çalışır.
        [Header("Border ring - decoration only, never part of the board")]
        [SerializeField] private Sprite[] borderSprites;

        [Tooltip("How many decorative cells the ring adds on every side. Zero draws no ring.")]
        [SerializeField, Min(0)] private int borderThickness = 2;

        // Menzilli saldırının uçan görseli: ok ya da büyücü asasının parıltısı.
        // Atanmazsa hiçbir şey uçmaz ve hiçbir hata basılmaz — bitişik vuruşta
        // zaten hamlenin kendisi yetiyor.
        [Header("Ranged attack projectile - optional")]
        [SerializeField] private Sprite projectileSprite;

        // Saldıran bir yapının iki atışı arasındaki bekleme — ama artık YEDEK,
        // birinci cevap değil. Beklemenin sahibi yapının kendi saldırı profili
        // oldu ve bu alan yalnızca oradaki sayı 0 olduğunda devreye giriyor.
        // İKİ SAYAÇ YAN YANA DURURSA TARET İKİSİNİN BÜYÜĞÜ KADAR BEKLER ve
        // Inspector'daki sayı sessizce yalan söyler; sahibi tek olsun diye
        // sıralama böyle. → Structure.AttackProfile
        [Tooltip("Fallback only: used when the structure's own attack profile has no cooldown.")]
        [SerializeField, Min(0.1f)] private float structureFireSeconds = 1.5f;

        // Can barının çizildiği düz beyaz kare. Renk koddan veriliyor, bu yüzden
        // sprite'ın kendisi renksiz olmalı.
        [Header("Health bar - assign the plain white square sprite")]
        [SerializeField] private Sprite healthBarSprite;

        // İmlecin altındaki hücreyi çerçeveleyen görsel. İçi boş bir kare olmalı,
        // yoksa hücrenin içindekini kapatır.
        [Header("Hover highlight - assign the hollow cell frame sprite")]
        [SerializeField] private Sprite hoverFrameSprite;

        // Diriltmenin ESKİ kapısı, bugün bir TAKMA AD: düşmüş bir dosta tıklamak
        // tuşsuz da kaldırıyor. Alan silinmedi çünkü tuşu basılı tutmaya alışmış
        // el aynı yere gitmeye devam ediyor ve o alışkanlığı boşa çıkarmanın
        // oyuncuya kazandırdığı hiçbir şey yok.
        [Header("Revive - the key is an alias now, clicking a fallen ally is enough")]
        [Tooltip("Optional: holding this key while clicking also revives. Clicking a fallen ally already does.")]
        [SerializeField] private KeyCode reviveModifierKey = KeyCode.LeftShift;

        [Header("Structure stats - applied to every placed structure")]
        [Tooltip("Starting and maximum health of each placed structure.")]
        [SerializeField, Min(1)] private int structureMaxHealth = 50;

        // SEÇİLİ TÜRÜN BİLGİ PENCERESİ — sahne bağı burada, açan tıklama
        // HENÜZ burada değil. Bağı SceneSetupTool kuruyor ve bu dosyada
        // bugün hiçbir üye bu alanı sormuyor: bir Unit'ten hangi
        // UnitBlueprintAsset/StructureBlueprintAsset'e ait olduğuna giden
        // bir yol yok — SpawnUnit çıplak bir isimle, CommitPlacement
        // Inspector sayılarıyla kuruyor, ikisi de bir varlık dosyası
        // taşımıyor. O yol kurulmadan bu alanı bir tıklamaya bağlamak,
        // gösterecek hiçbir şeyi olmayan bir pencere açardı.
        // EŞİK: Unit -> BlueprintAsset eşlemesi doğduğu gün (muhtemelen
        // IPlacementBoard sözleşmesinin bir parçası olarak) açan tıklama
        // buraya yazılır.
        [Header("Unit info dialog - built by CountryBall/Sahneyi Kur")]
        [Tooltip("Optional: bir türün ayrıntı penceresi. Bugün hiçbir tıklama açmıyor.")]
        [SerializeField] private UnitInfoDialogView infoDialog;

        // Unity'nin Grid bileşeni SADECE bir koordinat çevirmenidir:
        // hücre indeksi <-> dünya konumu. Hiçbir şey çizmez, kaç hücre
        // olduğunu bilmez, oyun durumu tutmaz. Tuttuğu tek şey ayarlardır
        // (cellSize, cellGap, cellLayout).
        private Grid unityGrid;

        // Tahtanın ve savaşın durumu BURADA DEĞİL, Battle'ın içinde yaşar; bu
        // alan yalnızca o bütüne bir tutamaktır. Burada bir UnitGrid alanı vardı
        // ve silinmesi bu dosyanın en pahalı satırı: tahtaya yazan tek yol artık
        // Battle.AddUnit. → BoardAdapter.md#battle
        // DERİN ANLATIM: Docs/deep/konular/03-tahta-sahipligi.md
        private Battle battle;

        // Core'daki Unit ile ekrandaki görselini eşleyen tablo. Anahtar neden
        // Unit: KONUM yalnız tahtada yaşasın istiyoruz — görsel "neredeyim"
        // bilmez, konumu her gerektiğinde Battle'dan hesaplanır. Paralel bir
        // dizi konumu iki yerde tutardı ve ikisi kayarsa hata sessiz olurdu.
        // Değer tipi GameObject'ten UnitView'a çıktı; adaptör artık çerçevenin
        // hangi nesnede yaşadığını hiç bilmiyor. → BoardAdapter.md#unitviews
        private readonly Dictionary<Unit, UnitView> unitViews =
            new Dictionary<Unit, UnitView>();

        // YAPININ GÖRSELİ İÇİN AYRI TABLO, ve ayrımı doğuran şey DEĞER tipidir:
        // birim görseli prefab'dan doğuyor ve üstünde bir UnitView taşıyor, yapı
        // görseli ise CreateStructureVisual içinde koddan kuruluyor ve kendi
        // bileşenini orada takıyor — tek tabloda birleştirmek ortak ataya
        // (Component) inip temizliğin hangi tarafta olduğunu yeniden sordurmak
        // olurdu.
        //
        // BU TABLONUN YOKLUĞU BİR HATAYDI, bir sadelik değil: yapı görseli
        // doğuyor ama hiçbir yere yazılmıyordu, temizlik süpürmesi onu
        // bulamıyor, LogError basıyor ve enkaz ekranda kalıyordu.
        //
        // ESKİ ÖLÇÜ YANLIŞTI VE DÜZELTİLDİ: burada "yapı görseli bizim hiçbir
        // bileşenimizi taşımıyor" yazıyordu ve o cümle bir eksikliği tarif
        // ediyordu, bir kararı değil. Çıplak GameObject tutulduğu sürece ekranın
        // yapıya söyleyebileceği hiçbir şey yoktu; yıkılan bina ayakta duranla
        // birebir aynı görünüyordu.
        private readonly Dictionary<Unit, StructureView> structureViews =
            new Dictionary<Unit, StructureView>();

        // Şu an seçili birim. null = seçim yok.
        private Unit selectedUnit;

        // Temizlik süpürmesinin ÇIKIŞ tamponu. Alan olmasının tek sebebi
        // TAHSİS: her karede yeni bir List kurmak kare başına çöp üretirdi.
        // Battle.RemoveReadyForCleanup onu her çağrıda temizleyip yeniden
        // doldurur, yani bu alanın çağrılar arasında taşıdığı bir anlam YOK —
        // bir durum değil, yeniden kullanılan bir kaptır.
        private readonly List<Unit> cleanupBuffer = new List<Unit>();

        // Tıklama ile sürüklemeyi ayıran saf tip. cleanupBuffer'ın TERSİ bir
        // alan: basıldığı nokta ve eşiğin aşılıp aşılmadığı kareler ARASINDA
        // yaşamak zorunda, yani gerçek bir hafıza. Kurucusu eşiği dışarıdan
        // istediği için Awake'te kurulur. → BoardAdapter.md#gesture
        private PointerGesture gesture;

        // ██ İKİNCİ BİR JEST, PAYLAŞILAN BİR JEST DEĞİL ██
        // Sol tuşun serbest akıştaki hakemi: haritayı mı kaydırıyoruz, tahtaya
        // mı tıklıyoruz. Üstteki `gesture` yerleştirme kipine ait ve ikisi AYNI
        // nesne DEĞİL — olsaydı kipten çıkarken yapılan bir Reset, o karede
        // sürmekte olan bir kaydırmayı da sessizce düşürürdü. İki akış zaten
        // aynı karede koşmuyor; paylaşılan tek şey eşiğin KENDİSİ ve onun tek
        // bir yazarı var. → BoardPointerArbiter.cs
        private BoardPointerArbiter pointerArbiter;

        // ═══ KİP MAKİNESİ — ALTI BAYRAĞIN YERİNE GEÇEN TEK NESNE ════════
        // Burada `isPlacingStructure` adında bir bool vardı, yanında
        // `ghostIsCarried`, aşağısında bekleyen vuruşun dört alanı. Altısı birer
        // bayrak değil birer DURUMdu: "tıklama ne demek", "hayaleti kim yazar",
        // "Update ne yapmalı" sorularının cevabı tam olarak onlara göre
        // değişiyordu. Bugün cevabı veren tek yer yürürlükteki kip.
        // → Modes/IBoardMode.cs
        private BoardModeMachine modes;
        private StructurePlacementMode placementMode;

        // Savaş bittikten sonraki kip. Girdiyi kapatmanın sahibi burası, çünkü
        // "tıklama ne demek" sorusunun sahibi zaten makine.
        // → Modes/BattleOverBoardMode.cs
        private BattleOverBoardMode battleOverMode;

        // ÜÇÜ DE İLK SORULUŞTA KURULUYOR ve sebebi dilin kendisi: kipler host
        // olarak `this`i istiyor, alan başlatıcısı ise `this`i göremez (CS0027).
        // AWAKE DE OLMAZDI, ve bu ölçüldü: EditMode testleri Awake'i hiç
        // koşturmuyor, orada kurulan bir makine testlerde null doğar ve
        // yansımayla çağrılan her üye NullReferenceException verirdi.
        //
        // KİPLER TAHTA BAŞINA BİRER TANE, static DEĞİL: aynı sahnede iki
        // BoardAdapter iki ayrı savaş doğuruyor ve paylaşılan bir kip, birinin
        // hayaletini ötekinin karesinde taşırdı.
        private BoardModeMachine Modes { get { EnsureModes(); return modes; } }

        private StructurePlacementMode Placement { get { EnsureModes(); return placementMode; } }

        private BattleOverBoardMode BattleOver { get { EnsureModes(); return battleOverMode; } }

        private void EnsureModes()
        {
            if (modes != null)
            {
                return;
            }

            placementMode = new StructurePlacementMode(this);
            battleOverMode = new BattleOverBoardMode();
            modes = new BoardModeMachine(new IdleBoardMode());
        }

        // ═══ EMİR DEFTERİ — KİP MAKİNESİNİN YANINDA, İÇİNDE DEĞİL ═══════
        // Burada bekleyen vuruşun DÖRT TEKİL alanı vardı
        // (pendingStrikeAttacker / pendingStrikeTarget / pendingStrikeX / Y) ve
        // ikinci bir birime emir verildiği an birincisininki siliniyordu.
        // Operatörün "iki taraf paralel olmuyor" şikâyeti bir ayar eksikliği
        // değil bir SAHİPLİK hatasıydı.
        //
        // AYRIM TEK CÜMLE: kip TAHTANIN ne yaptığıdır ve tektir; emir HER
        // BİRİME ne söylendiğidir ve çoğuldur. Emirleri kip makinesine sokmak,
        // çoğul bir kavramı tekil bir sahibe vermek olurdu.
        // → Modes/IBoardMode.cs, Orders/IUnitOrder.cs
        // → Docs/deep/konular/09-kararlarin-cevrilmesi.md (madde 2)
        //
        // ALAN BAŞLATICISINDA KURULUYOR, kiplerin tersine: defter kurucusunda
        // `this` istemiyor, dolayısıyla CS0027 doğmuyor ve EditMode'da hiç
        // null olmuyor.
        private readonly UnitOrderBook orders = new UnitOrderBook();

        // Şu an yerleştirilmekte olan yapının görseli. Paletten sürüklenen
        // düğmenin simgesi buraya yazılır; hem önizleme hayaleti hem de tahtaya
        // konan bina onu kullanır — böylece oyuncunun sürüklerken gördüğü şey
        // ile bıraktığında oluşan şey AYNI olur.
        private Sprite pendingStructureSprite;

        // Sürüklenen binanın kaç hücre kapladığı; palet söylemediyse 0. Ölçü
        // ayrı bir alanda duruyor çünkü hem hayalet hem de kurulan bina onu
        // okuyor ve ikisinin AYNI sayıyı okuması önizlemenin sözünü tutmasının
        // tek yolu.
        private float pendingStructureSizeInCells;

        // Hayaletin sahnede YAZILI olan sprite'ı — Awake'in gördüğü ilk hâli.
        //
        // BU ALAN BİR ONARIM: paletten bir bina bir kez sürüklendikten sonra
        // ProductionDirector her bırakışta SetPlacementVisual(null) çağırıyor,
        // o da hayaletin sprite'ını siliyordu. Klavyeli yerleştirme kipinin tek
        // yedeği o sprite olduğu için bina GÖRÜNMEZ kuruluyordu: boş karenin
        // üstünde havada duran bir can barı ve geçilemeyen bir hücre.
        private Sprite authoredGhostSprite;

        // Sahnede yazılı hayalet rengi — "bırakılabilir" hâlin rengi.
        // authoredGhostSprite'ın ikizi ve aynı sebeple var; gerekçe
        // CaptureAuthoredGhostSprite'ta bir kez yazılı.
        private Color authoredGhostColour = Color.white;

        // Kimlik → can barı. ÜÇÜNCÜ bir tablo ve bilerek: alternatifi her karede
        // GetComponentInChildren çağırmaktı — o da savaşçı ve yapı görsellerinin
        // İÇ YAPISINI (barın bir çocuk olduğu) tahtanın bilgisi hâline getirirdi.
        // Tablo tek yerden doldurulup tek yerden (DespawnView) siliniyor.
        private readonly Dictionary<Unit, HealthBarView> healthBars =
            new Dictionary<Unit, HealthBarView>();

        // GERİ SAYIM ŞERİTLERİ. Can barlarının ikizi ve AYRI bir tablo, çünkü
        // ayrı bir küme: can barı HER kimliğin, geri sayım yalnız savaşçı ÜRETEN
        // binanın. Tek tabloda birleştirilselerdi taretin de boş bir kaydı olur
        // ve "kimin göstergesi var" sorusu tabloya sorulamaz hâle gelirdi.
        private readonly Dictionary<Unit, ProductionTimerView> productionTimers =
            new Dictionary<Unit, ProductionTimerView>();

        // İmleç çerçevesinin çizicisi. Sahnede elle kurulmuyor, Awake'te
        // doğuyor — bir nesne daha sürüklettirmemek için.
        private SpriteRenderer hoverHighlight;

        // Savaşçı görsellerinin havuzu. Awake'te kuruluyor çünkü prefab ve
        // ebeveyn ancak o zaman hazır; alan tanımında kurulsaydı serileştirilmiş
        // prefab referansı henüz okunmamış olurdu.
        private UnitViewPool viewPool;

        // ═══ BURADA BEKLEYEN VURUŞUN DÖRTLÜSÜ DURUYORDU ═══════════════════
        // `pendingStrikeAttacker`, `pendingStrikeTarget`, `pendingStrikeX/Y` ve
        // yanlarında emrin cinsini taşıyan `pendingStrikeIsRevive` bool'u. Beşi
        // de gitti; yerlerini `orders` defteri ve iki emir sınıfı aldı.
        // Kaldırılan alanların eski hâli ve neyi kırdığı:
        // → Docs/deep/konular/09-kararlarin-cevrilmesi.md (madde 2)

        // Yapı başına atış sayacı. Ayrı bir sözlük, çünkü sayacın sahibi savaşın
        // kaydı değil EKRANIN saati: Structure kendi ateş temposunu bilmez ve
        // bilmesi de gerekmez.
        private readonly Dictionary<Unit, float> structureFireTimers =
            new Dictionary<Unit, float>();

        // Atış taramasının ÇIKIŞ tamponu, cleanupBuffer ile aynı gerekçeyle bir
        // alan: bir atış hedefi yıkabilir ve yıkım sözlüğe dokunabilir; sözlüğü
        // gezerken değiştirmek gezintiyi patlatır. Önce anahtarlar kopyalanır.
        private readonly List<Unit> structureFireBuffer = new List<Unit>();

        // GÖRÜNÜM BAŞINA BİR KARO — hücre başına değil. Gerekçesi TileFor'da
        // yazılı; buradaki tek not sözlüğün ANAHTARI: Sprite, çünkü aynı
        // sprite'ı iki farklı hücre istediğinde dönmesi gereken şey aynı nesne.
        private readonly Dictionary<Sprite, TileBase> tileCache =
            new Dictionary<Sprite, TileBase>();

        // ESKİ halka kök nesnesinin adı. Bu bileşen artık böyle bir nesne
        // KURMUYOR — halka bir tilemap oldu — ama ad duruyor, çünkü sahne
        // dosyasında bir öncekinin bıraktığı bir "BorderRing" olabilir ve onu
        // ADIYLA toplamaktan başka yol yok.
        private const string BorderRootName = "BorderRing";

        // İki tilemap'in nesne adları. SABİT, çünkü ikisi de İKİ yerde okunuyor
        // (kurma ve geri bulma) ve iki ayrı dizge literali, biri değiştiği gün
        // sahnede ikinci bir tilemap doğmasına yol açardı.
        private const string GroundMapName = "GroundTilemap";
        private const string BorderMapName = "BorderTilemap";

        // SEÇİM ÇARPANI ARTIK BURADA DEĞİL, StructureView'de. Taşınma sebebi
        // ölçülmüş bir çakışma: yıkılmış bina da aynı SpriteRenderer.color
        // alanını kullanıyor ve iki dosyadan yazılan tek alanda son yazan
        // ötekini siler. Kenarlık çerçevesinin rengi burada KALDI — o ayrı bir
        // çocuk nesnenin kendi rengi, gövde çarpanı değil.

        // Bırakılamayacak bir hücrenin üstündeki hayaletin rengi. Saydamlık
        // KORUNUYOR (0,55): opak bir kırmızı, altındaki hücreyi ve orada duran
        // birimi tamamen örterdi — oysa oyuncunun görmesi gereken tam olarak
        // "orada zaten bir şey var" olgusu.
        private static readonly Color RejectedGhostColour = new Color(1f, 0.30f, 0.28f, 0.55f);

        // Seçim çerçevesinin rengi. Gövde çarpanından DAHA parlak ve bilerek:
        // çarpan sprite'ın kendi rengine bağlı, çerçeve ise kendi rengini
        // doğrudan çiziyor — yani binanın rengi ne olursa olsun aynı görünüyor.
        private static readonly Color StructureSelectionFrameColour = new Color(1f, 0.95f, 0.35f, 1f);

        // Çerçevenin nesne adı. SABİT, çünkü iki yerde okunuyor (kurma ve geri
        // bulma) ve iki ayrı dizge literali, biri değiştiği gün ikinci bir
        // çerçeve doğmasına yol açardı.
        private const string StructureSelectionFrameName = "SelectionFrame";

        // ═══ ÇİZİM SIRASI MERDİVENİ — TEK SAHİP, ALTI BASAMAK ════════════
        // OYUNDA NE İŞE YARAR: aynı hücrede üst üste gelen bina ile asker her
        // seferinde AYNI sırayla çizilir; askerin binanın arkasında kaybolduğu
        // kare olmaz. Yapı ile birim eskiden ikisi de 1 idi ve o eşitlikte
        // hangisinin üste çizileceğini hiçbir kural söylemiyordu.
        //
        // HALKA ZEMİNİN ALTINDA KALIYOR: halka hücreleri oynanabilir alanla
        // hiç kesişmiyor, ama kenardaki bir bina taşarsa halkanın üstüne
        // binmeli — altta kalan bir süs, üste binen bir süsten daha az yalan
        // söyler.
        private const int BorderSortingOrder = -1;
        private const int GroundSortingOrder = 0;
        private const int StructureSortingOrder = 1;
        private const int UnitSortingOrder = 2;
        private const int HoverSortingOrder = 3;
        private const int HealthBarSortingOrder = 4;

        // Geri sayım şeridi can barının BİR ÜSTÜNDE çiziliyor. Aynı yükseklikte
        // olsalardı hangisinin öne geçeceği Unity'nin kendi sırasına kalırdı ve
        // iki şerit birbirini kırpardı.
        private const int ProductionTimerSortingOrder = 5;

        private const int GhostSortingOrder = 6;

        // Can barının, çizilen görselin ÜST kenarından ne kadar yukarıda
        // duracağı — dünya birimi. Sabit bir yükseklik yerine bu pay
        // kullanılıyor, çünkü görselin kendi boyu binadan binaya değişiyor.
        private const float HealthBarMargin = 0.08f;

        // Geri sayım şeridinin CAN BARINDAN ne kadar yukarıda duracağı. Ayrı bir
        // sayı, çünkü ayrı bir mesafe: HealthBarMargin görselin tepesi ile bar
        // arasını ölçüyor, bu ise iki şerit arasını. Tek sabite indirilselerdi
        // barı görselden uzaklaştırmak şeridi de sessizce iterdi.
        private const float ProductionTimerMargin = 0.14f;

        // ═══ IPlacementBoard SÖZLEŞMESİ — 13 ÜYE, TEK YÖN ════════════════
        // Bu tip artık üretim ve yerleştirme katmanının tahtadan istediği her
        // şeyi karşılıyor. OKUN YÖNÜ TEK VE ÖLÇÜLEBİLİR: aşağıdaki hiçbir satır
        // ne ProductionDirector'ı ne bir panel tipini ADIYLA anıyor; tahta
        // yalnızca yayınlıyor ve soruları cevaplıyor. Ters yön reddedildi —
        // tahta o katmanı tanısaydı arayüzün var olma sebebi tam o satırda
        // çökerdi ve iki dosya birbirini tutmak zorunda kalırdı.
        // ÜYELERİN GEREKÇESİ İKİ YERDE BÖLÜNMÜŞ, tekrar edilmiyor: NE İSTENDİĞİ
        // IPlacementBoard.cs'te üye üye yazılı, NASIL KARŞILANDIĞI burada.

        /// <summary>
        /// Tahtada bir birim ya da yapı seçildiğinde haber verir; seçim
        /// kalktığında <c>null</c> ile.
        /// </summary>
        // İKİ YAYIN NOKTASI, İKİSİ DE ÜYENİN SONUNDA: SelectUnit yeni seçimi,
        // ClearSelection null'ı duyurur. ClearSelection'ın erken çıkışı yayının
        // ÜSTÜNDE bırakıldı ve bu bir karardır — altına inseydi seçim yokken
        // yapılan her tıklama aynı boş seçimi tekrar duyururdu, sağ panel de
        // her yayında düğmelerini yeniden kurduğu için görünür bir sebep
        // olmadan kendini sıfırlardı.
        // SEÇİMİN İKİNCİ BİR KOPYASI DOĞMUYOR: seçimin tek sahibi hâlâ
        // selectedUnit alanı; olay onu yayımlar, saklamaz.
        public event Action<Unit> SelectionChanged;

        /// <summary>
        /// Bir kimlik tahtadan tamamen kaldırıldığında haber verir — ceset ya
        /// da enkaz süresi dolduğunda.
        /// </summary>
        // YAYIN AdvanceBattleTime'IN SÜPÜRMESİNDE, ve orası TEK doğru yer:
        // savaşın kadrosundan çıkışın başka bir kapısı yok. DespawnView'ın
        // içine konsaydı sessiz bir sızıntı doğardı — o üye önce yapı tablosuna
        // bakıp erken dönüyor ve görseli olmayan bir kimlikte hiç yayın
        // yapmazdı, oysa olayın sorduğu şey görsel değil kayıttır.
        // BU OLAY OLMASAYDI KİMSE HATA AÇMAZDI: yıkılan her yapının üretim
        // hattı dinleyicinin defterinde sonsuza dek sayardı ve ekranda bunun
        // hiçbir karşılığı görünmezdi.
        public event Action<Unit> UnitRemoved;

        /// <summary>
        /// Sıra el değiştirdiğinde tetiklenir: hangi takım ve kaçıncı tur.
        /// </summary>
        // OLAY, HER KARE OKUNAN BİR PROPERTY DEĞİL — gerekçesi SelectionChanged
        // ile birebir aynı ve orada yazılı. Sırayı gösteren etiket bu olaya
        // abone; olay olmasaydı etiket her karede tahtayı yoklamak zorunda
        // kalırdı ve sıranın ikinci bir kopyası doğardı.
        public event Action<Team, int> TurnChanged;

        /// <summary>
        /// Savaş bittiğinde bir kez tetiklenir: kim kazandı ya da berabere.
        /// </summary>
        // TAHTA EKRANA HÂLÂ ÇİZMİYOR, YALNIZCA HABER VERİYOR — ve bu sınır eski
        // hâlin yazılı sınırının aynısı: sonucu gösterecek arayüzün sahibi bu
        // dosya değil. Değişen tek şey, o sınırın artık bir kapısı olması.
        //
        // OLAY BİR KEZ TETİKLENİR ve garantiyi winnerAnnounced mandalı veriyor;
        // aynı mandal Console satırını da tekil tutuyor, yani iki kanalın
        // tekilliği tek bir alandan geliyor.
        //
        // SONUÇ OLAYIN İÇİNDE TAŞINIYOR, ABONEYE SORDURULMUYOR: dinleyici
        // "kim kazandı" diye tahtaya geri sorsaydı, cevabı üreten kuralın
        // ikinci bir çağıranı doğardı ve o çağrı yayından SONRAKİ bir tahtayı
        // okurdu.
        public event Action<BattleOutcome> BattleEnded;

        // Son duyurulan sıra. Duyurunun İKİ kez yapılmasını engelliyor: tur
        // numarası ile takımın ikisi birden karşılaştırılıyor, çünkü iki
        // oyunculu bir sırada tur numarası aynı kalırken takım değişebilir.
        private Team lastAnnouncedTeam;
        private int lastAnnouncedTurn = -1;

        // Zafer bir kez duyuruldu mu. Mandal olmadan "BATTLE OVER" satırı her
        // ceset durum değiştirdikçe yeniden basılıyordu; oyuncu Console'da aynı
        // cümleyi onlarca kez görüyordu.
        //
        // MANDAL ARTIK İKİNCİ BİR İŞ DAHA YAPIYOR: yayın da onun arkasında
        // duruyor. Bir dinleyici pano açarken tahtanın durumunu değiştirseydi
        // (bir görsel yok etmek gibi) aynı kare içinde buraya geri girilirdi;
        // mandal yayından ÖNCE yazıldığı için o geri giriş erken çıkıyor.
        private bool winnerAnnounced;

        // ═══ İMLEÇ ÇERÇEVESİNİN ÖNBELLEĞİ — DÖRT ANAHTAR, TEK CEVAP ══════
        // OYUNDA NE İŞE YARAR: çerçevenin rengi aynı kalıyor. Değişen tek şey
        // o rengin kaç kez hesaplandığı — hücre, seçim ve seçilinin konumu
        // aynı kaldığı sürece yol araması hiç yapılmıyor.
        //
        // ÖLÇÜ: UpdateHoverHighlight her karede battle.TryFindPath çağırıyordu
        // ve PathFinder çağrı başına bir List, üç int dizisi ve iki bool dizisi
        // tahsis ediyor. Saniyede altmış kare, imleç hiç kımıldamasa bile.
        private int hoverCacheX;
        private int hoverCacheY;
        private int hoverCacheFromX;
        private int hoverCacheFromY;
        private Unit hoverCacheUnit;
        private bool hoverCacheReachable;
        private bool hoverCacheValid;

        // DERİN ANLATIM: Docs/deep/konular/08-motor-cagri-dongusu.md — bu metodu
        // hiçbir satır ÇAĞIRMIYOR ve bir C# `event` de değil; motor onu ADINA
        // bakarak buluyor, çağrı sırası ve koşulları orada ölçüyle yazılı.
        private void Awake()
        {
            // GetComponent bir SORGUdur: bileşen listesinde arar ve bulduğuna
            // referans döner, hiçbir şey yaratmaz. Listede bir Grid bulunacağını
            // RequireComponent garanti eder. → BoardAdapter.md#awake
            unityGrid = GetComponent<Grid>();
            // SIRA KİPİ SAVAŞLA BİRLİKTE DOĞAR, sonradan değiştirilemez: kipin
            // ortasında el değiştirmesi, yarısı sıra tabanlı yarısı serbest
            // oynanmış bir tur bırakırdı.
            battle = new Battle(width, height, turnMode);

            // Eşik Inspector'dan geliyor, bu yüzden jest ancak burada
            // kurulabilir: alan bildiriminde kurulsaydı serileştirilmiş değer
            // daha okunmamış olurdu ve Inspector'daki sayı boşa çıkardı.
            gesture = new PointerGesture(dragThreshold);

            // AYNI EŞİK, İKİ TÜKETİCİ ve bu bir çift yazar DEĞİL: sayıyı yazan
            // tek yer yukarıdaki Inspector alanı, okuyan iki akış var.
            pointerArbiter = new BoardPointerArbiter(dragThreshold);

            // KAMERA BURADA ARANIYOR, HER KAREDE DEĞİL: Camera.main etiketli
            // kamerayı arayan bir sorgudur ve sürükleme boyunca her karede
            // sorulsaydı kaydırma, işin kendisinden pahalı bir aramayı da
            // beraberinde taşırdı.
            if (cameraRig == null && Camera.main != null)
            {
                cameraRig = Camera.main.GetComponent<BoardCameraRig>();
            }

            // SESSİZ KALMIYOR: rig yoksa tıklama çalışır, kaydırma çalışmaz ve
            // operatör bunu ancak sürükleyip hiçbir şey olmadığında anlardı.
            // Log, LogError değil — rig'siz bir sahne bozuk değil, yalnız
            // gezinmesizdir.
            if (cameraRig == null)
            {
                Debug.Log(
                    "[Board] No BoardCameraRig found; left-drag map panning is off. Assign one or add the rig to the MainCamera.",
                    this);
            }

            // ██ ÇERÇEVE ARTIK BURADAN DOĞUYOR, MENÜDEN DEĞİL ██
            // Rig arandıktan HEMEN SONRA: aşağıdaki çağrı rig'e yazıyor ve
            // rig'siz bir sahnede yalnız 0. katmanı düzeltiyor.
            PublishBoardFraming();

            // Hayalet, kipte OLMADIĞIMIZ sürece çizilmez. Sahnede açık
            // bırakılmış olabilir; UnitView.Awake'in SetSelected(false) ile
            // yaptığı işin birebir aynısı — yazılı durumu değişmeze çevirmek.
            if (placementGhost == null)
            {
                Debug.LogError(
                    "[Board] placementGhost is not assigned. Assign a child SpriteRenderer; structure placement mode will refuse to start without it.",
                    this);
            }
            else
            {
                CaptureAuthoredGhostSprite();
                placementGhost.sortingOrder = GhostSortingOrder;
                placementGhost.enabled = false;
            }

            viewPool = new UnitViewPool(unitPrefab, transform);

            BuildCellVisuals();
            BuildHoverHighlight();

            // GEÇİCİ: iki demo birim. İKİSİ de gerekli ve bu bir tercih değil —
            // saldırı zincirinin kapandığını göstermek için birbirine
            // tıklanabilen İKİ birim şart, ve TargetingRules dost ateşini
            // reddettiği için tarafları farklı olmak zorunda.
            if (unitPrefab != null)
            {
                SpawnUnit("Vanguard", Team.Player, 1, 2);
                SpawnUnit("Raider", Team.Enemy, 1, 3);
            }
            else
            {
                Debug.LogError("[Board] unitPrefab is not assigned. Assign the Unit prefab (it must carry a UnitView component) in the Inspector.", this);
            }
        }

        /// <summary>
        /// Oynanabilir tahtanın dünya dikdörtgeni.
        /// </summary>
        // HÜCRE MERKEZLERİNDEN DEĞİL KENARLARINDAN: hücre (0,0) merkezi
        // (0.5, 0.5) olduğu için tahta [0,width] x [0,height] aralığını
        // kaplıyor. Kamera kelepçesi kesişme ölçüyor ve kesişme kenarlarla
        // hesaplanır.
        //
        // battle.Width DEĞİL width: bu özellik Awake'ten ÖNCE de doğru cevap
        // vermeli, oysa savaş nesnesi Awake'te doğuyor. İkisi zaten aynı sayı —
        // savaşı kuran satır (`new Battle(width, height, turnMode)`) bunu tek
        // otorite olarak bağlıyor.
        public Rect WorldRect => new Rect(0f, 0f, width, height);

        /// <summary>
        /// Tahtanın DIŞINDA görünmesi gereken süsün genişliği, dünya birimi.
        /// </summary>
        // İKİ SAHİPTEN TOPLANIYOR, HİÇBİRİ BURADA YAZILI DEĞİL: halkanın
        // kalınlığı bu bileşenin kendi Inspector alanı, kuşakların payı ise
        // 0. katmanı çizen tipin sabiti. Toplam hiçbir yerde yazılmıyor,
        // yalnız hesaplanıyor.
        private float DressedMargin => borderThickness + WorldBackdrop.IslandMargin;

        /// <summary>
        /// Tahtanın dünya dikdörtgenini kameraya ve 0. katmana duyurur.
        /// </summary>
        // ██ OPERATÖRÜN ŞİKÂYETİ: "tahtayı 5x10 yaptım, kamera eski çerçevede" ██
        // KÖK SEBEP: çerçeveyi yazan tek yer SceneSetupTool.AttachCameraRig'ti,
        // yani bir Editor menüsü. Tasarımcı width/height alanını değiştirdiğinde
        // hiçbir şey ona haber vermiyordu ve bayatladığını anlamanın yolu yoktu.
        //
        // YENİDEN BAŞLATMA DA BURADAN DÜZELİYOR: BattleOverView sahneyi
        // SceneManager ile yeniden yüklüyor ve yeniden yükleme, sahnede YAZILI
        // olan çerçeveyi geri okur. Türetme Awake'e geçtiği için yeniden
        // başlatılan savaş da doğru çerçeveyle açılıyor.
        //
        // KÜRESEL ARAMA YOK: rig yukarıda ya Inspector alanından ya da
        // MainCamera üzerinden çözülmüş durumda, kamera da onun kendi nesnesi.
        private void PublishBoardFraming()
        {
            Camera view = cameraRig != null
                ? cameraRig.GetComponent<Camera>()
                : Camera.main;

            // ÇERÇEVE RIG'SİZ DE HESAPLANIYOR: 0. katman kameranın gördüğü
            // alana serilecek ve rig yoksa da bir şeyler serilmesi gerekiyor.
            // Rig'i olmayan bir sahne gezinemez, ama çöl de görmemeli.
            BoardFrame frame = BoardFraming.Frame(
                WorldRect, DressedMargin, view != null ? view.aspect : 0f);

            if (cameraRig != null)
            {
                cameraRig.WriteHomeFraming(
                    WorldRect, frame.Centre, frame.HalfHeight, frame.Aspect);
            }

            WorldBackdrop.Refresh(WorldRect, borderThickness, frame);
        }

        // ═══ ABONELİK — VE NEDEN OnEnable/OnDisable ÇİFTİ ════════════════
        // Bu abonelik bir HATA DÜZELTMESİDİR, bir özellik değil: Downed → Dead
        // geçişi Tick'in içinde, hiçbir tıklama olmadan gerçekleşir ve ekran onu
        // duymuyordu. Awake/OnDestroy çifti REDDEDİLDİ — o çift nesnenin
        // DOĞUMUNU eşler, olay dinlemek ise ETKİNLİĞE aittir; kapalı bir bileşen
        // dinlemeye devam ederdi ve "kapalı" sözü tam orada düşerdi. Simetriyi
        // derleyici değil disiplin tutuyor: eksik bir `-=` tek bir uyarı bile
        // üretmez. → BoardAdapter.md#onenable-ve-ondisable
        // ÖDÜNÇ ALINAN — `event`: `+=` ve `-=` derleyicinin ürettiği gizli bir alana
        // yazar (add_ ve remove_ metotları üstünden), bu yüzden dengesiz kalan bir
        // `+=` tek bir uyarı bile üretmez — üstteki "disiplin" cümlesinin sebebi bu.
        // DİL: Docs/deep/dil/06-delege-arka-taraf.md
        private void OnEnable()
        {
            battle.UnitStateChanged += OnUnitStateChanged;
        }

        private void OnDisable()
        {
            battle.UnitStateChanged -= OnUnitStateChanged;

            // KİP, BİLEŞEN KAPANIRKEN BIRAKILIR ve tek çağrı ikisini birden
            // götürüyor: bırakılmasaydı yeniden açılan adaptör hayaleti gizli,
            // kipi açık bulurdu (fare tıklaması görünmez bir yapı yerleştirirdi)
            // ya da kapalı geçen sürede baştan aşağı değişmiş bir tahtada
            // oyuncunun çoktan unuttuğu bir emri yürütürdü.
            Modes.ToIdle();

            // HAYALET AYRICA KAPATILIYOR, VE BU BİR TEKRAR DEĞİL: hayaleti
            // yalnız kip yazmıyor, sürükleme yolu da yazıyor ve o yol kipin
            // dışında yaşıyor. Kip zaten kapalıyken kalan açık bir hayalet,
            // yeniden açılan adaptörün ekranında sahipsiz durur.
            SetGhostVisible(false);
            gesture?.Reset();

            // KAYDIRMA DA BIRAKILIYOR: tahta kapanırken sürmekte olan bir
            // sürükleme kalırsa kamera, tahta yeniden açıldığında tutamağı
            // eski bir dünya noktasında bulur ve ilk karede oraya sıçrar.
            // `?.` burada GÜVENLİ, çünkü BoardPointerArbiter sade bir C#
            // sınıfı — yukarıdaki cameraRig kontrolünün gerekçesi (yok edilmiş
            // Unity nesnesi) bu tipte YOK.
            ApplyPointerAction(pointerArbiter?.Cancel() ?? BoardPointerAction.None, Vector2.zero);
        }

        /// <summary>
        /// Tahta yok edilirken, kendi kurduğu karo nesnelerini de yok eder.
        /// </summary>
        // ██ ScriptableObject SAHNEYLE BİRLİKTE GİTMEZ ██
        // Bir GameObject sahne kapanınca yok olur; `ScriptableObject.CreateInstance`
        // ile kurulan bir nesnenin sahnede bir kökü YOKTUR ve kimse onu
        // toplamaz. Sahne her yeniden yüklendiğinde bir öncekinin karoları
        // bellekte kalır — patlamayan, yalnız BÜYÜYEN bir sızıntı.
        //
        // OnDisable DEĞİL OnDestroy: bileşen kapanıp yeniden açıldığında
        // (Modes.ToIdle yolundaki gibi) karolar hâlâ geçerli ve yeniden kurmak
        // boşa iş olurdu. Ölçü şu — karoların ömrü bileşenin ömrü, açık
        // kalmasının değil.
        private void OnDestroy()
        {
            foreach (KeyValuePair<Sprite, TileBase> entry in tileCache)
            {
                if (entry.Value != null)
                {
                    Destroy(entry.Value);
                }
            }

            tileCache.Clear();
        }

        /// <summary>
        /// Savaş bir birimin durumunu değiştirdiğinde ekranı tazeler.
        /// </summary>
        // İMZA `Action<Unit, UnitState, UnitState>`: KİM, nereden, nereye.
        // "Nereden" bugün KULLANILMIYOR ve bu bir eksiklik değil; kullanacağı
        // ilk gün adı hazır — düşme ve diriliş animasyonları.
        // → BoardAdapter.md#onunitstatechangedunit-unit-unitstate-from-unitstate-to
        private void OnUnitStateChanged(Unit unit, UnitState from, UnitState to)
        {
            ApplyStateVisual(unit, to);

            // ZAFERİN SORULDUĞU TEK YER BURASI, ve sebebi bu metodun ne olduğu:
            // cevabın değişebildiği tek an bir savaşçının durumunun değiştiği
            // andır. Update'e konsaydı aynı cevap saniyede altmış kez üretilirdi.
            AnnounceWinnerIfAny();
        }

        /// <summary>
        /// Savaşın bitip bitmediğini SORAR; bittiyse tahtayı dondurur, Console'a
        /// tek satır yazar ve sonucu duyurur.
        /// </summary>
        // KURALI BU DOSYA YAZMIYOR ve tek satırlık kanıtı şu: aşağıda ne bir
        // sayım, ne bir durum karşılaştırması, ne de bir taraf tercihi var —
        // kadroyu Battle geziyor, kazananı VictoryRules söylüyor, buranın işi
        // yalnızca cevabı Console'a taşımak.
        //
        // ██ MANDAL ARTIK BURADA, VE ESKİ ÖLÇÜ YANLIŞTI ██
        // Burada "tekrarı UnitLifecycle'ın erken çıkışı engelliyor" yazıyordu ve
        // ölçüm bunu çürüttü: o erken çıkış yalnız AYNI birimin ikinci kez
        // yayılmasını kesiyor, oysa bu üye HER birimin HER durum değişikliğinde
        // çağrılıyor. Savaş bittikten sonra kalan cesetler Downed'dan Dead'e
        // geçtikçe aynı satır yeniden basılıyordu.
        //
        // EKRANA HÂLÂ HİÇBİR ŞEY ÇİZİLMİYOR ve sınır aynı yerde duruyor: sonucu
        // gösterecek arayüzün sahibi bu dosya değil. Değişen tek şey, o arayüzün
        // artık haber alabilmesi.
        //
        // ██ WINNER DEĞİL OUTCOME SORULUYOR ██
        // Winner beraberliği "savaş sürüyor" ile aynı cevaba indiriyor, yani iki
        // taraf da tükendiğinde erken çıkış yapılıyor ve oyun hiçbir şey demeden
        // donuyordu — ne Console'da satır, ne ekranda pano. Outcome o iki hâli
        // ayırdığı için buranın erken çıkışı artık yalnız gerçekten süren bir
        // savaşta yapılıyor. → Battle/BattleOutcome.cs
        private void AnnounceWinnerIfAny()
        {
            if (winnerAnnounced)
            {
                return;
            }

            // NESNE ALAN AŞIRI YÜKLEME, iki bool DEĞİL: eski çağrı savaşı
            // yalnız savaşçılardan soruyordu ve son askeri düşen tarafın ayakta
            // duran barakası hiç sayılmıyordu. İki bool'un yeri karışırsa
            // kazananın TERS okunması riski de bu imzayla birlikte düşüyor.
            BattleOutcome outcome = VictoryRules.Outcome(battle);

            if (outcome == BattleOutcome.Ongoing)
            {
                return;
            }

            // ██ YAYIN EN SONDA — GEÇİŞİN SON İFADESİ ██
            // Olayın anlamı "savaş bitti VE tahta artık girdi almıyor". O cümleyi
            // doğru yapan iki yazım da aşağıdaki Invoke'tan önce tamamlanıyor;
            // yayın araya girseydi pano açılmışken tahta bir kare daha tıklama
            // kabul ederdi ve oyuncu kazandığı savaşta bir hamle daha yapardı.
            //
            // MANDAL KİPTEN DE ÖNCE: dinleyicinin tetikleyebileceği her geri
            // giriş burada kesiliyor, kip geçişi ise makinenin kendi kapısından
            // bir kez geçiyor.
            winnerAnnounced = true;
            Modes.Enter(BattleOver);

            // ██ KONSOL SATIRI SİLİNMEDİ, VE SEBEBİ İKİ AYRI DİNLEYİCİ ██
            // Console'u okuyan geliştirici, panoyu okuyan oyuncu. Aynı olguyu
            // anlatıyorlar ama biri zaman damgalı ve aranabilir bir kayıt, öteki
            // tek karelik bir ekran; pano gelince kaydın değeri düşmüyor.
            // Üstelik satırın metni bir testin yazılı kararı.
            Debug.Log(BattleOverLine(outcome), this);

            BattleEnded?.Invoke(outcome);
        }

        /// <summary>
        /// Console'a düşecek tek satırı seçer.
        /// </summary>
        // CÜMLE HARFİ HARFİNE KORUNDU ve bunu bir test istiyor: BoardAdapterTests
        // içindeki AnnounceWinnerIfAny_WhenTheEnemyIsWipedOut_SaysThePlayerWins
        // "BATTLE OVER - Player wins" desenini bekliyor. Metni sonucun adına
        // çevirmek ("PlayerWon") o testi kırardı ve kırılan şey bir yazım tercihi
        // değil, dördüncü maddenin kanıtıydı.
        //
        // BERABERLİĞİN AYRI CÜMLESİ VAR çünkü eskiden hiç cümlesi yoktu: iki
        // taraf da tükendiğinde üye erken çıkıyor ve karşılıklı yok oluş her iki
        // kanalda da SESSİZ geçiyordu.
        private static string BattleOverLine(BattleOutcome outcome)
        {
            if (outcome == BattleOutcome.Draw)
            {
                return "[Board] BATTLE OVER - a draw; neither side is left in play.";
            }

            Team winner = outcome == BattleOutcome.PlayerWon ? Team.Player : Team.Enemy;
            return $"[Board] BATTLE OVER - {winner} wins.";
        }

        // DERİN ANLATIM: Docs/deep/konular/07-tiklamadan-eyleme.md
        // → BoardAdapter.md#update
        private void Update()
        {
            // ZAMAN HER KARE İLERLER, tıklama olsun olmasın — ve bu sıra bir
            // karardır: erken çıkışın ALTINA konsaydı savaşın saati yalnızca
            // oyuncu tıkladığında işlerdi, yani düşmüş bir birim el sürülmediği
            // sürece asla ölmezdi.
            AdvanceBattleTime();

            // ██ EMİRLER SAVAŞIN SAATİNDEN SONRA, GİRDİDEN ÖNCE ██
            // SAATTEN SONRA, çünkü bekleme sayacını ilerleten şey battle.Tick:
            // üste konsaydı her emir bir kare eski bir sayaç okur ve ilk vuruş
            // bir kare gecikirdi. TEMİZLİKTEN de sonra: AdvanceBattleTime
            // tahtadan kalkan kimlikleri süpürüyor ve emirler böylece o karede
            // hâlâ ayakta olan bir dünyaya soruyor.
            // GİRDİDEN ÖNCE, çünkü oyuncunun bu karede verdiği YENİ emir
            // eskisinin yerine geçer; tersi olsaydı iptal edilmiş bir emir bir
            // kare daha yaşar ve artık istenmeyen hedefe inerdi.
            orders.Advance();

            // KİP, KARE İŞİNDEN ÖNCE KİLİTLENİR ve bu bir yerleşim kazası
            // değil: aşağıdaki iki soru da bu karenin BAŞINDAKİ kipe göre
            // cevaplanmalı. Kip kendini bu karede kapatsa bile çerçeve kapalı
            // kalır ve erken çıkış yine yapılır — eski `if (isPlacingStructure)`
            // dalının davranışı buydu.
            IBoardMode mode = Modes.Current;
            bool modeOwnsPointer = mode.OwnsPointer;

            // KİPİN KARE İŞİ GİRDİDEN ÖNCE: bekleyen vuruş kipi burada
            // yürüyüşün bitip bitmediğini yokluyor, çünkü oyuncunun aynı karede
            // başlattığı yeni bir eylem emri iptal etmeli — tersi olsaydı iptal
            // edilmiş bir emir bir kare daha yaşar ve artık istenmeyen hedefe
            // inerdi.
            mode.Advance();

            // Çerçeve girdiden ÖNCE tazeleniyor: tıklama akışı seçimi
            // değiştirdiğinde çerçevenin rengi aynı karede eskimiş kalmasın.
            UpdateHoverHighlight(modeOwnsPointer);

            // KİP AYRIMI, MEVCUT AKIŞIN ÜSTÜNDE — ve sıra bir karardır. Altına
            // konsaydı yerleştirme sırasındaki her basış önce HandleClick'ten
            // geçerdi: hayalet taşınırken tahtadaki birimler seçilirdi. Kip,
            // girdinin ANLAMINI baştan sona değiştirir.
            if (modeOwnsPointer)
            {
                // YARIM KALMIŞ KAYDIRMA BURADA BİTER: kip işaretçiyi
                // sahiplendiği an harita imleci takip etmeyi bırakmalı. İptal
                // edilmeseydi oyuncu bina taşırken fareyi her oynattığında
                // harita da kayardı — hakem hâlâ "sürüklüyor" der, tahta ise
                // ona bir daha hiç sormazdı.
                ApplyPointerAction(pointerArbiter.Cancel(), Vector2.zero);
                return;
            }

            if (Input.GetKeyDown(placementModeKey))
            {
                TryEnterPlacementMode();
                return;
            }

            // Kaldırma tıklamadan ÖNCE sorulur: tuş basılıyken gelen bir tıklama
            // seçimi değiştirirse oyuncu yanlış nesneyi silmiş olurdu.
            if (Input.GetKeyDown(removeSelectedKey))
            {
                RemoveSelected();
                return;
            }

            AdvancePointer();
        }

        /// <summary>
        /// Sol tuşun bu karedeki hâlini hakeme sorar ve çıkan eylemi uygular:
        /// haritayı kaydırır ya da tahtaya tıklar.
        /// </summary>
        // ██ TIKLAMA ARTIK BIRAKMA KARESİNDE DOĞUYOR ██
        // OYUNDA NE İŞE YARAR: oyuncu sol tuşu basılı tutup haritada
        // gezinebiliyor ve bunu yaparken yanlışlıkla birim seçmiyor.
        // Eskiden burada tek satır vardı — `if (!Input.GetMouseButtonDown(0))
        // { return; }` — ve tıklama BASMA karesinde bitiyordu. O satır durdukça
        // sol sürükleme imkânsızdı: basıldığı an tıklama çoktan olmuş oluyordu.
        //
        // ÜÇ SORGU BİR KEZ OKUNUYOR: aynı karede Input'a ikinci kez sormak
        // ucuzdur ama SAHİPLİĞİ bulanıklaştırır — bir gün ikinci okuma bir
        // if'in içine kayar ve hakem yarım bir kare görür.
        private void AdvancePointer()
        {
            bool pressed = Input.GetMouseButtonDown(0);
            bool held = Input.GetMouseButton(0);
            bool released = Input.GetMouseButtonUp(0);

            // UCUZ ÇIKIŞ: fare boştaysa ekran noktasını dünyaya çevirmeye hiç
            // girilmiyor. Şart `IsActive`i de soruyor, çünkü odak kaybı Up
            // karesini yutabilir ve o durumda üç sorgunun üçü de yanlıştır
            // ama jest hâlâ etkindir — hakemin onu kapatabilmesi gerekiyor.
            if (!pressed && !held && !released && !pointerArbiter.IsActive)
            {
                return;
            }

            Vector2 screenPoint = Input.mousePosition;

            if (!TryScreenPointToWorldCell(screenPoint, out float worldX, out float worldY, out _, out _))
            {
                return;
            }

            // ARAYÜZ SORUSU YALNIZ BASIŞ KARESİNDE. Bugün zararı kameranın
            // çerçevelemesi saklıyor (tahta panellerin altından çıkarılmış), ama
            // dar bir ekran oranında panel tahtanın üstüne biniyor ve üretim
            // düğmesine basan oyuncu aynı anda arkadaki hücreyi de tıklamış
            // oluyor. Her karede sorulsaydı, tahtada başlayıp panelin üstünde
            // biten bir kaydırma yarı yolda donardı.
            BoardPointerAction action = pointerArbiter.Advance(
                pressed, held, released, worldX, worldY, pressed && PointerIsOverUi());

            ApplyPointerAction(action, screenPoint);
        }

        /// <summary>
        /// Hakemin verdiği kararı kameraya ve tahtaya dağıtır.
        /// </summary>
        // KARARI VEREN İLE UYGULAYAN AYRI, ve ayrımın bedeli ölçüldü: karar
        // saf bir tipte olduğu için EditMode'da sınanabiliyor (fare girdisi
        // EditMode'da akmıyor), uygulama ise burada kalıyor çünkü Camera ve
        // Input yalnız burada var.
        //
        // `cameraRig != null` AÇIK YAZILDI, `?.` DEĞİL: BoardCameraRig bir
        // UnityEngine.Object ve yok edilmiş bir nesne C# tarafında null
        // GÖRÜNMEZ; `?.` aşırı yüklenmiş eşitliği atlar. Aynı gerekçe
        // PointerIsOverUi'nin EventSystem kontrolünde de yazılı.
        private void ApplyPointerAction(BoardPointerAction action, Vector2 screenPoint)
        {
            switch (action)
            {
                case BoardPointerAction.PanBegin:
                    if (cameraRig != null)
                    {
                        cameraRig.BeginPan(screenPoint);
                    }

                    break;

                case BoardPointerAction.PanContinue:
                    if (cameraRig != null)
                    {
                        cameraRig.ContinuePan(screenPoint);
                    }

                    break;

                case BoardPointerAction.PanEnd:
                    if (cameraRig != null)
                    {
                        cameraRig.EndPan();
                    }

                    break;

                case BoardPointerAction.Click:
                    HandleClick();
                    break;
            }

            // default DALI BİLEREK YOK: None bir hata değil, bir karenin normal
            // cevabıdır — IBoardMode'un faz switch'indeki gerekçenin aynısı.
        }

        /// <summary>
        /// İmleç bir uGUI ögesinin üstünde mi.
        /// </summary>
        // SAHNEDE EventSystem YOKSA CEVAP "hayır": o hâlde zaten tıklanabilir
        // bir arayüz de yoktur ve bugünkü davranış aynen sürer. null geçişli
        // çağrı yerine açık bir kontrol, çünkü EventSystem.current bir Unity
        // nesnesidir ve `?.` onun aşırı yüklenmiş eşitliğini görmez.
        private static bool PointerIsOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        /// <summary>
        /// Yerleştirme kipine girmeyi dener. Seçili birim ve atanmış bir hayalet
        /// şart.
        /// </summary>
        // YAPIYI KİM KOYAR sorusu bir OYUN kuralıdır ve doğru sahibi burası
        // DEĞİL: BattleActions.PlaceStructure geçerliliğe kendisi karar verir.
        // Buradaki tek şart TEKNİK — imzanın istediği `unit` argümanı.
        // → BoardAdapter.md#tryenterplacementmode
        private void TryEnterPlacementMode()
        {
            if (selectedUnit == null)
            {
                Debug.Log("[Board] Select a unit before entering structure placement mode.", this);
                return;
            }

            if (placementGhost == null)
            {
                Debug.LogError("[Board] Cannot enter placement mode: placementGhost is not assigned.", this);
                return;
            }

            // ÇİZİLECEK BİR SİMGE YOKSA KİPE HİÇ GİRİLMEZ. Eski hâlde yalnız
            // hayaletin VARLIĞI soruluyordu; sprite'ı boşalmış bir hayaletle
            // kipe girilebiliyor, oyuncu ekranda hiçbir şey görmeden tıklıyor ve
            // hücreye GÖRÜNMEZ bir bina oturuyordu. Log, LogError değil: bu bir
            // oyun olgusu — operatör hayalete bir sprite atamayı unutmuş olabilir
            // ve o eksikliği Awake zaten ayrıca söylüyor.
            if (placementGhost.sprite == null)
            {
                Debug.Log(
                    "[Board] Placement mode needs a sprite to preview; assign one on the ghost " +
                    "or pick a building from the palette first.",
                    this);
                return;
            }

            // TEK KAPIDAN GEÇİŞ, VE ELLE YAZILAN İPTALİN SONU: burada
            // `CancelPendingStrike()` çağrısı vardı ve "yeni eylem eskisini
            // düşürür" kuralı her çağıranın hafızasına bırakılmıştı. Bugün
            // düşüren şey geçişin kendisi — açık kip kapanırken emrini siliyor.
            // → Modes/BoardModeMachine.cs
            Modes.Enter(Placement);
        }

        /// <summary>
        /// Motorun üç fare sorgusunu <see cref="PointerGesture"/>'ın üç metoduna
        /// çevirir ve ortaya çıkan fazı verir.
        /// </summary>
        // Down/MoveTo bir if-else zinciri, Up ise AYRI bir if — çünkü tek bir
        // karede Down ve Up birlikte true olabilir.
        // → BoardAdapter.md#feedgesturefloat-worldx-float-worldy
        // DERİN ANLATIM: Docs/deep/konular/07-tiklamadan-eyleme.md
        private PointerPhase FeedGesture(float worldX, float worldY)
        {
            PointerPhase phase = gesture.Phase;

            if (Input.GetMouseButtonDown(0))
            {
                phase = gesture.Press(worldX, worldY);
            }
            else if (Input.GetMouseButton(0))
            {
                phase = gesture.MoveTo(worldX, worldY);
            }

            if (Input.GetMouseButtonUp(0))
            {
                phase = gesture.Release(worldX, worldY);
            }

            return phase;
        }

        /// <summary>
        /// Yerleştirmeyi savaşa SORAR ve cevabına göre ekranı günceller.
        /// </summary>
        private void CommitPlacement(int x, int y)
        {
            // Geçerliliğe BU DOSYA KARAR VERMİYOR ve tek satırlık kanıtı şu:
            // aşağıda ne bir sınır, ne bir doluluk, ne de bir sıra kontrolü var.
            // → BoardAdapter.md#commitplacementint-x-int-y
            Unit placer = selectedUnit;

            // YAPI KENDİ KİMLİĞİYLE TAHTAYA GİRER, KOYANIN KİMLİĞİYLE DEĞİL —
            // ve bu satır bu dosyanın en pahalı düzeltmesi: burada `placer`
            // geçiliyordu, o kimlik ise savaşın kadrosunda ZATEN kayıtlıydı.
            // Battle.AddStructure'ın ortak kapısı ThrowIfCannotJoin aynı kimliği
            // ikinci kez görünce "The unit is already in this battle." diyerek
            // ArgumentException atıyordu, yani PlacementOutcome.Placed üretimde
            // HİÇBİR geçerli hücrede üretilemiyordu.
            //
            // DÜZELTMENİN YERİ ÇAĞIRAN, MOTOR DEĞİL — ölçüldü, iki bağımsız
            // kanıtla: PlaceStructure'ın `unit` parametresinin belgesi o
            // argümanı "yapının tahtadaki kimliği" diye tarif ediyor, ve
            // BattleActionsTests içindeki PlaceStructure_SameIdentityTwice_Throws
            // testi aynı kelepçeyi bilerek koruyor. Kural doğruydu; sözleşmeye
            // uymayan taraf burasıydı.
            var structureUnit = new Unit($"Structure_{x}_{y}");

            // YAPININ SAYILARI HÂLÂ Inspector'DAN: bu yol klavyeli kipin yolu ve
            // NewStructure'ı kullanmaya devam ediyor. Sürükleme yolu aynı kapıya
            // BAŞKA bir yapıyla giriyor, çünkü orada canın sahibi tanım
            // varlığıdır. İki yol bir süre yan yana yaşar.
            PlacementOutcome outcome =
                PlaceStructure(structureUnit, NewStructure(placer), x, y);

            // KİPTEN ÇIKIŞ, sonuçtan BAĞIMSIZ: ret de bir cevaptır ve ret sebebi
            // çoğu zaman hücre değil BİRİMdir; düzeltmenin yolu kipe yeniden
            // girmektir. ÇIKIŞ ARTIK GÖRSELİN ALTINDA ve fark gözlenemez: kipin
            // Cik() işi yalnız hayaletin görünürlüğüne dokunuyor, görselin
            // okuduğu sprite ise kapalı bir SpriteRenderer'da da aynı.
            //
            // ŞARTLI ÇIKIŞ, ŞARTSIZ DEĞİL: bu üye kip AÇIK OLMADAN da
            // çağrılabiliyor (yansımayla koşan testler) ve şartsız bir çıkış o
            // çağrılarda başka bir kipi düşürürdü.
            Modes.LeaveIfCurrent(Placement);

            Debug.Log($"[Board] '{placer.Name}' placement at ({x},{y}) -> {outcome}.", this);
        }

        /// <summary>
        /// Yapıyı tahtaya koyar, savaşa katar ve görselini oluşturur.
        /// </summary>
        /// <returns>
        /// Yerleştirmenin cevabı. Ret sebepleri <see cref="PlacementOutcome"/>
        /// içinde ZATEN adlandırılmış durumda ve buradan bool'a ezilmiyor —
        /// çağıran oyuncuya hangi cümleyi söyleyeceğini ancak sebebi görerek
        /// bilir.
        /// </returns>
        // "TAHTAYA GİRDİ" İLE "EKRANDA VAR" ARTIK TEK KAPIDAN GEÇİYOR ve bu
        // dosyanın bir kez ödediği bir bedelin karşılığı: structureViews
        // tablosunun üstünde yazılı olduğu gibi, yapı görseli bir zamanlar
        // doğuyor ama hiçbir yere yazılmıyordu ve temizlik onu bulamıyordu. İki
        // çağıran (klavyeli kip ve sürükleme) aynı satırları kopyalasaydı o
        // kelepçenin ikinci bir kopyası doğar, biri değiştiği gün öbürü sessizce
        // eskirdi.
        //
        // NewStructure BURADA ÇAĞRILMIYOR ve yokluğu bir karardır: Inspector'daki
        // tek structureMaxHealth sayısı bütün yapı türlerine aynı canı verirdi,
        // oysa canın sahibi artık yapı türünün kendi tanımı. Yapıyı dışarıdan
        // almak, o sahipliği bu dosyanın hiç görmemesini sağlıyor.
        //
        // TEK BİR DEĞERLE KARŞILAŞTIRMA, TAM SWITCH DEĞİL — ve bu bilinçli bir
        // eksikliktir: buradaki tek soru "kondu mu" ve bu dosyanın ret sebebine
        // göre yapacağı FARKLI bir işi yok; sebebi ekrana taşımak çağıranın işi.
        // EŞİK: bir ret sebebi burada farklı bir şey yaptırdığı gün tam switch'e
        // çevrilir.
        public PlacementOutcome PlaceStructure(Unit identity, Structure structure, int x, int y)
        {
            // Önce KURAL, sonra görsel — SpawnUnit'in sırasıyla birebir aynı ve
            // aynı sebeple: reddedilen bir yerleştirme ekranda gerçekleşmiş
            // görünmemeli.
            PlacementOutcome outcome = BattleActions.PlaceStructure(battle, identity, structure, x, y);

            if (outcome == PlacementOutcome.Placed)
            {
                CreateStructureVisual(identity, x, y);
            }

            return outcome;
        }

        // ═══ KİPLERİN PENCERESİ — NE İSTENDİĞİ ARAYÜZDE, NASILI BURADA ═══
        // Üye üye gerekçe TEKRAR EDİLMİYOR: her üyenin neden istendiği
        // Modes/IBoardModeHost.cs'te yazılı, aşağıdaki satırlar yalnız o isteği
        // bu tipin alanlarına bağlıyor. Aynı bölünme IPlacementBoard'da da var.
        //
        // AÇIK ARAYÜZ UYGULAMASI (explicit) VE BU BİR SEÇİM: örtük yazılsalardı
        // tahtanın herkese açık yüzüne on yedi yeni üye eklenirdi, oysa bunları
        // soran tek taraf kiplerin kendisi. Açık yazıldıklarında yalnız host
        // tutamağından görünüyorlar.
        //
        // Log'un ikinci parametresi (context) tam olarak bu pencerenin var olma
        // sebebi: Console satırına tıklandığında vurgulanacak nesne tahtadır ve
        // kipin elinde öyle bir nesne yok.
        Unit IBoardModeHost.SelectedUnit => selectedUnit;

        void IBoardModeHost.Log(string message) => Debug.Log(message, this);

        void IBoardModeHost.LeaveMode(IBoardMode mode) => Modes.LeaveIfCurrent(mode);

        bool IPlacementModeHost.PlacementCancelRequested => Input.GetKeyDown(placementCancelKey);

        void IPlacementModeHost.ShowPlacementGhost(bool visible) => SetGhostVisible(visible);

        void IPlacementModeHost.MovePlacementGhostTo(int x, int y)
        {
            if (placementGhost != null)
            {
                placementGhost.transform.position = CellCentre(x, y);
            }
        }

        bool IPlacementModeHost.TryReadPointerCell(
            out float worldX, out float worldY, out int x, out int y)
        {
            return TryReadPointerCell(out worldX, out worldY, out x, out y);
        }

        // null GEÇİŞLİ ÇAĞRI ŞART: jest Awake'te kuruluyor ve EditMode testleri
        // Awake'i hiç koşturmuyor.
        void IPlacementModeHost.ResetPointerGesture() => gesture?.Reset();

        PointerPhase IPlacementModeHost.FeedPointerGesture(float worldX, float worldY)
            => FeedGesture(worldX, worldY);

        void IPlacementModeHost.CommitPlacement(int x, int y) => CommitPlacement(x, y);

        bool IUnitOrderHost.TryGetCell(Unit unit, out int x, out int y)
        {
            return battle.TryGetPosition(unit, out x, out y);
        }

        bool IUnitOrderHost.IsViewWalking(Unit unit) => IsViewWalking(unit);

        // ██ İKİ İŞ, İKİ ÜYE — VE AYRIM ARTIK BİR BAYRAKTA DEĞİL, TİPTE ██
        // Eski hâlde tek bir ExecuteStrike vardı ve varışta ne yapılacağını
        // `pendingStrikeIsRevive` bool'u söylüyordu. Emir bir nesne olunca o
        // bayrak gereksizleşti: hangi işin yapılacağını emrin KENDİ tipi
        // biliyor ve üçüncü bir emir cinsi doğduğu gün buraya bir dal daha
        // eklenmiyor, bir sınıf daha yazılıyor.
        AttackOutcome IUnitOrderHost.Strike(Unit attacker, Unit target)
        {
            // HÜCRE HER VURUŞTA TAZE OKUNUYOR — ve bu bir ONARIM. Eski emir
            // hedefin YAZILDIĞI andaki hücresini saklıyordu; kalıcı emirde hedef
            // kımıldadığı için mermi ve Console satırı hedefin ARTIK OLMADIĞI
            // hücreyi gösterirdi.
            if (!battle.TryGetPosition(target, out int x, out int y))
            {
                return AttackOutcome.RejectedInvalidTarget;
            }

            AttackOutcome outcome = BattleActions.Attack(battle, attacker, target);

            // ██ BEKLEME BİR OLAY DEĞİL, EMRİN NORMAL HÂLİDİR ██
            // Kalıcı emir her karede vurmayı deniyor ve sayaç dolana kadar
            // "henüz yeniden vuramaz" cevabı alıyor. O cümle burada da
            // yazılsaydı saniyede altmış satır Console'a düşerdi — tek bir
            // bekleme penceresi ekranı yıkardı. Ölçülen sayı: 1 saniyelik bir
            // bekleme, 60 kare/saniyede ~59 gereksiz satır.
            //
            // SUSTURULAN TEK DEĞER BU, ve seçilebilmesinin sebebi kalıcı bir
            // emrin tekrar tekrar üretebildiği YEGÂNE ret olması: ötekilerin
            // hepsi emri aynı karede düşürüyor, yani bir kez yazılıyor.
            if (outcome != AttackOutcome.RejectedOnCooldown)
            {
                ReactToAttack(attacker, outcome, target, x, y);
            }

            return outcome;
        }

        void IUnitOrderHost.Revive(Unit reviver, Unit target)
        {
            if (!battle.TryGetPosition(target, out int x, out int y))
            {
                return;
            }

            ReviveOutcome revived = BattleActions.Revive(battle, reviver, target);
            ReactToRevive(reviver, revived, target, x, y);
        }

        // ██ ÜÇÜNCÜ ÜYE, ÜÇÜNCÜ İŞ — VE HAREKETİN SAHİBİ DEĞİŞMEDİ ██
        // Burada yeni bir yürüyüş yolu açılmıyor: hücreyi ApproachRules söylüyor,
        // tahtayı BattleActions.Move taşıyor, görseli UnitWalker yürütüyor. Bu
        // üye üçünü bir araya getiren tek satırlık bir sıradan ibaret.
        //
        // MENZİL SORUSU AttackRangeOf'A SORULUYOR ve o üye cevabı saldıranın
        // KENDİ profilinden okuyor — savaşçı ya da yapı fark etmeksizin. İkinci
        // kez okunsaydı emrin yürüdüğü menzil ile vuruşun ölçtüğü menzil sessizce
        // ayrışır ve okçu hedefin dibine kadar yürürdü.
        ApproachOutcome IUnitOrderHost.MoveIntoRange(Unit mover, Unit target)
        {
            // HÜCRE HER KAREDE TAZE OKUNUYOR: kaçan bir saldırganın yazıldığı
            // andaki hücresine yürümek, kovalamayı hedefin ARTIK OLMADIĞI yere
            // götürürdü.
            if (!battle.TryGetPosition(target, out int targetX, out int targetY))
            {
                return ApproachOutcome.RejectedOffBoard;
            }

            ApproachOutcome plan = battle.PlanApproach(
                mover, targetX, targetY, AttackRangeOf(mover), out int cellX, out int cellY);

            if (plan != ApproachOutcome.MoveTo)
            {
                return plan;
            }

            // YÜRÜYÜŞ REDDEDİLİRSE CEVAP "ULAŞILAMAZ"A DÖNÜYOR ve bu bir
            // yuvarlama değil: kural yolu var dedi ama savaş hareketi kabul
            // etmedi (birim düştü, sırası yok). Emrin bu ikisine verecek ayrı bir
            // tepkisi yok — ikisi de o kareyi yürünemez yapıyor.
            return TryStartWalk(mover, cellX, cellY)
                ? ApproachOutcome.MoveTo
                : ApproachOutcome.RejectedUnreachable;
        }

        /// <summary>
        /// Sahnede hayalete YAZILMIŞ olan sprite'ı yedek olarak saklar.
        ///
        /// OYUNDA NE İŞE YARAR: palet bir simge söylemediğinde (klavyeli
        /// yerleştirme kipi) hayalet yine de bir şey çizebilsin.
        /// </summary>
        // AYRI BİR ÜYE, Awake'in İÇİNDE İKİ SATIR DEĞİL — ve tek gerekçesi
        // sınanabilirlik: EditMode testleri Awake'i hiç koşturmuyor, yedeğin
        // yazılıp yazılmadığını soran bir test ancak çağırabildiği bir üyeyle
        // yazılabilir.
        private void CaptureAuthoredGhostSprite()
        {
            if (placementGhost != null)
            {
                authoredGhostSprite = placementGhost.sprite;

                // RENK DE YEDEKLENİYOR ve gerekçesi sprite'ınkiyle birebir aynı:
                // önizleme artık hayaletin rengini YAZIYOR, dolayısıyla
                // "geçerli" hâlin rengini bir yerden geri okuması gerekiyor.
                // Sabit bir beyaz yazılsaydı sahnede ayarlanmış saydamlık ilk
                // sürüklemede kalıcı olarak kaybolurdu — bu tuzağa proje
                // sprite tarafında bir kez düştü.
                authoredGhostColour = placementGhost.color;
            }
        }

        /// <summary>
        /// Hayaletin görünürlüğünü yazar.
        /// </summary>
        // AYRI BİR ÜYE, ÇÜNKÜ İKİ AYRI SAHİBİ VAR: kip kendi girişinde ve
        // çıkışında yazıyor, OnDisable ise sürükleme yolunun bıraktığı hayaleti
        // kapatıyor ve o yol hiçbir kipe ait değil. SESSİZ null KONTROLÜ:
        // hayaleti atanmamış bir sahnede Awake ZATEN bir kez bağırıyor.
        private void SetGhostVisible(bool visible)
        {
            if (placementGhost != null)
            {
                placementGhost.enabled = visible;
            }
        }

        /// <summary>
        /// Yerleştirme önizlemesini gösterir ya da gizler.
        ///
        /// Yeni bir mekanizma DEĞİL, var olanın kapısı: tahtadaki hayalet
        /// klavyeli yerleştirme kipinden beri duruyor ve sürükleme için ikinci
        /// bir önizleme çizmek aynı işi yapan iki nesne demek olurdu.
        /// </summary>
        // SESSİZ null KONTROLÜ BİLEREK: aynı eksiklik Awake'te ZATEN bir kez
        // bağırıyor ve TryEnterPlacementMode'da ikinci kez. Orası teşhis,
        // burası hayatta kalma.
        //
        // ██ ÇAKIŞMA KARARI: KİP HAYALETİ SAHİPLENİR, SÜRÜKLEME GERİ ÇEKİLİR ██
        // Aynı hayaleti iki taraf yazıyor ve aşağıdaki erken çıkış o kararın
        // burada duran yarısı. Ölçüm, reddedilen iki seçenek ve geriye kalan
        // artık artık KİPİN YANINDA yazılı ve burada TEKRAR EDİLMİYOR — çünkü
        // sahipliği alan taraf o. → Modes/StructurePlacementMode.cs
        //
        // ERKEN ÇIKIŞ ARTIK BİR BAYRAĞA DEĞİL KİPE SORUYOR ve kazancı ölçülebilir:
        // soru "yerleştirme kipinde miyiz" değil "yürürlükteki kip fareyi
        // sahipleniyor mu" oldu, yani fareyi sahiplenen dördüncü bir kip
        // eklendiği gün bu satırın değişmesi gerekmiyor.
        public void SetPlacementGhost(bool visible, int x, int y)
        {
            if (placementGhost == null)
            {
                return;
            }

            if (Modes.Current.OwnsPointer)
            {
                return;
            }

            placementGhost.enabled = visible;

            // Konum yalnız görünürken yazılıyor: gizli bir hayaleti taşımak
            // hiçbir şeyi değiştirmez ama "gizlerken nereye" sorusunu doğurur
            // ve çağıranı anlamsız bir hücre uydurmaya zorlardı.
            if (visible)
            {
                placementGhost.transform.position = CellCentre(x, y);

                // ██ RENGİ ÇAĞIRAN SÖYLEMİYOR, TAHTA KENDİ OKUYOR ██
                // Verdikt bir parametre olsaydı çağıran onu bir yerden almak
                // zorunda kalırdı ve o yer, kuralın ikinci bir okuyucusu olurdu.
                // Hücreyi bilen taraf zaten cevabı da biliyor.
                placementGhost.color = PreviewAt(x, y) == PlacementPreview.Placeable
                    ? authoredGhostColour
                    : RejectedGhostColour;
            }
        }

        /// <summary>
        /// Inspector'daki sayılardan bir yapı kurar.
        /// </summary>
        // TARAF, YAPIYI KOYAN BİRİMDEN OKUNUR — Inspector'dan DEĞİL; ayrı bir
        // alan aynı bilginin ikinci kaynağı olurdu ve düşmanın yaptığı bina
        // oyuncunun tarafında görünebilirdi. AttackProfile verilmiyor:
        // saldırmayan yapı KURALdır. → BoardAdapter.md#newstructureunit-placer
        private Structure NewStructure(Unit placer)
        {
            Team team = battle.TryGetCombatant(placer, out Combatant combatant)
                ? combatant.Team
                : Team.None;

            return new Structure(new Health(structureMaxHealth), new StructureLifecycle(), team);
        }

        /// <summary>
        /// Yerleşen yapının tahtadaki görselini doğurur.
        /// </summary>
        // NEDEN PREFAB DEĞİL, KODLA KURULAN BİR GameObject: kod tarafı sahne ve
        // prefab dosyalarını üretemez, dolayısıyla yeni bir atanabilir alan
        // "Inspector'da boş kalan alan" riskini büyütürdü. Sprite hayaletten
        // okunuyor.
        //
        // ESKİ ÖLÇÜ YANLIŞTI VE DÜZELTİLDİ: burada "görsel bir tabloya
        // kaydedilmiyor, bugün onu tekrar bulması gereken hiçbir çağıran yok"
        // yazıyordu. Çağıran ilk günden beri VARDI — AdvanceBattleTime'ın
        // süpürmesi enkaz süresi dolan yapıyı DespawnView'a veriyor ve o da
        // görseli tabloda arıyordu; bulamadığı için LogError basıyor, enkaz
        // ekranda kalıyordu. Tablo bu yüzden var.
        // → BoardAdapter.md#createstructurevisualint-x-int-y
        private void CreateStructureVisual(Unit structureUnit, int x, int y)
        {
            // AD KİMLİKTEN OKUNUYOR, BURADA İKİNCİ KEZ KURULMUYOR: aynı metin
            // iki yerde üretilseydi biri değiştiği gün Hierarchy'deki ad ile
            // Console'daki ad ayrışır ve hiçbir şey patlamazdı.
            var structureObject = new GameObject(structureUnit.Name);
            structureObject.transform.SetParent(transform, worldPositionStays: false);
            structureObject.transform.position = CellCentre(x, y);

            var renderer = structureObject.AddComponent<SpriteRenderer>();

            // YAPI KENDİ GÖRSELİYLE ÇİZİLİR. Eskiden hayaletin sprite'ı
            // kullanılıyordu ve sonuç şuydu: tahtaya konan HER yapı aynı binaya
            // benziyordu, üstelik takımı da yanlıştı. Sprite'ı paletten getiren
            // yol SetPlacementVisual.
            // HAYALET GERİ ÇEKİLME SEÇENEĞİ, ZORUNLULUK DEĞİL: paletten gelen
            // simge her zaman doğru cevaptır ve hayalet yalnızca o simgenin hiç
            // gelmediği (klavyeli) yolda devreye giriyor. null kontrolü, hayaleti
            // atanmamış bir sahnede yerleştirmenin istisnayla kesilmemesi için.
            Sprite body = pendingStructureSprite != null
                ? pendingStructureSprite
                : (placementGhost != null ? placementGhost.sprite : null);

            renderer.sprite = body;

            // Zemin 0, yapı 1, birim 2: aynı hücrede üst üste gelen asker
            // binanın ÖNÜNDE çizilir ve sıra her karede aynı kalır.
            renderer.sortingOrder = StructureSortingOrder;

            // ÖLÇEK HAM BİR ÇARPAN DEĞİL, HÜCRE CİNSİNDEN BİR ÖLÇÜ: aynı
            // "1,25 hücre" isteği 16 pikselli sanatta da 32 pikselli sanatta da
            // aynı boyu veriyor. Eski hâlde Inspector'daki tek çarpan bütün bina
            // türlerine aynı sayıyı uyguluyordu ve sanat değiştiği gün bina
            // sessizce büyüyüp küçülüyordu.
            //
            // SPRITE'I OLMAYAN YAPI DA TANIMLI: BoardSizing ölçemediği görsele
            // Vector3.one dönüyor, yani bina bir hücrelik görünmez bir kutu
            // olarak duruyor — hücreyi kapatıyor, can barını taşıyor, yıkılınca
            // temizleniyor. Sessiz kalmıyoruz, çünkü ekranda hiçbir iz
            // bırakmayan kusurların en pahalısı tam olarak budur.
            if (body == null)
            {
                Debug.LogWarning(
                    $"[Board] structure '{structureUnit.Name}' has no sprite; it will hold the cell " +
                    "without being drawn. Pick a building from the palette or assign a ghost sprite.",
                    this);
            }

            structureObject.transform.localScale = StructureLocalScale(body);

            // GÖRÜNÜM BİLEŞENİ BURADA TAKILIYOR, PREFAB'DA BEKLEMİYOR — ve bu
            // REUSABLE olmanın tek satırlık kanıtı: yapı görselleri bu projede
            // yalnız burada doğuyor, dolayısıyla bugünkü on dört yapı tanımı da
            // yarın eklenecek olanlar da bileşeni buradan alıyor. Yeni bir tanım
            // eklemek SIFIR satır ekran kodu istiyor.
            //
            // AddComponent SIRASI GÖZLENEMEZ ama gerekli: [RequireComponent] bir
            // SpriteRenderer garanti eder, o da yukarıda zaten eklenmiş durumda —
            // yani ikinci bir çizici doğmuyor.
            var view = structureObject.AddComponent<StructureView>();

            // TABLOYA YAZMA EN SONDA: yukarıdaki kurulum satırlarından biri
            // patlarsa tabloda yarım kurulmuş bir görsele ok kalmamalı. Aynı
            // sıra Battle.AddUnit'in aboneliği en sona koymasıyla aynı sebepten.
            structureViews.Add(structureUnit, view);

            // BARIN YÜKSEKLİĞİ ÇİZİLEN BOYDAN OKUNUYOR: aynı hesap ölçeği de
            // veriyor, dolayısıyla bar binanın tepesinde duruyor — 1,6 ölçekli
            // bir binada 0,93, 1,25 ölçekli bir askerde 0,725 gibi iki ayrı
            // dünya yüksekliğine kayamıyor.
            AttachHealthBar(
                structureUnit,
                structureObject.transform,
                HealthBarSortingOrder,
                StructureDrawnHeight(body));
        }

        /// <summary>
        /// Bir yapı görselinin ekranda kapladığı YÜKSEKLİK, dünya birimi.
        /// </summary>
        // ÖLÇEKLE AYNI KAYNAKTAN: BoardSizing iki cevabı da tek hesaptan
        // üretiyor, yani bar hiçbir zaman binanın çizildiği boyla çelişmiyor.
        private float StructureDrawnHeight(Sprite sprite)
        {
            return BoardSizing.WorldHeightFor(sprite, PendingSizeInCells, CellSize);
        }

        /// <summary>
        /// Fare konumunu dünya koordinatına ve hücre indeksine çevirir.
        /// </summary>
        // ÇEVİRİ ARTIK BURADA DEĞİL, ALTINDA — ve ayrılmasının sebebi üçüncü bir
        // çağıranın doğması: sürükleme yolu ekran noktasını dışarıdan getiriyor
        // ve Input'a hiç uğramıyor. Geriye kalan tek iş fareyi okumak, ve bu üye
        // tam olarak o kadar. SINIR HÂLÂ SORULMUYOR: iki çağıranı da tahta dışı
        // koordinatla anlamlı bir iş yapıyor (biri "OUTSIDE" yazıyor, öteki
        // hayaleti imlecin altında tutuyor).
        // → BoardAdapter.md#tryreadpointercell
        private bool TryReadPointerCell(out float worldX, out float worldY, out int x, out int y)
        {
            // FAREYİ OKUYAN TEK SATIR BU, çeviri değil: çeviri aşağıdaki ortak
            // çekirdekte yaşıyor ve sürükleme yolu oraya kendi noktasıyla
            // giriyor. Input.mousePosition'ın z bileşeni her zaman sıfırdır,
            // dolayısıyla Vector2'ye inmek hiçbir bilgi kaybettirmiyor.
            return TryScreenPointToWorldCell(Input.mousePosition, out worldX, out worldY, out x, out y);
        }

        /// <summary>
        /// Ekran noktasını tahta hücresine çevirir.
        /// </summary>
        /// <returns>Nokta tahtanın dışına düşüyorsa false.</returns>
        // ██ TryReadPointerCell'İN KARDEŞİ, İKİZİ DEĞİL — VE FARK İKİ TANE ██
        // Birincisi noktanın nereden geldiği: bu üye ekran noktasını DIŞARIDAN
        // alıyor, çünkü sürükle-bırak olayları fareyi zaten okumuş durumda ve
        // tahtanın ikinci kez okuması, parmağın bırakıldığı yer ile sorulan yer
        // arasında bir kare fark açardı.
        // İkincisi ve önemlisi: bu üye tahta SINIRINI SORUYOR, kardeşi sormuyor.
        // Kardeşi soramaz da — HandleClick tahta dışı bir tıklamayı Console'a
        // "OUTSIDE the board" diye yazıyor ve yerleştirme kipi hayaleti tahta
        // dışında da imlecin altında tutuyor; sınır oraya taşınsaydı ikisi de
        // sessizce ölürdü. Burada ise sınırın cevabı "vazgeçildi" demektir:
        // sürükleme tahtanın dışında biterse çağıran hiçbir şey koymaz.
        // AYRIŞMA BİLİNÇLİDİR VE KOPYALANMAMALIDIR; ortak olan tek şey çeviridir
        // ve o zaten tek yerde duruyor.
        public bool TryScreenPointToCell(Vector2 screenPoint, out int x, out int y)
        {
            if (!TryScreenPointToWorldCell(screenPoint, out _, out _, out x, out y))
            {
                return false;
            }

            return battle.IsInsideGrid(x, y);
        }

        /// <summary>
        /// Bir ekran noktasını dünya koordinatına ve hücre indeksine çeviren TEK
        /// yer. Bir Unity tipinin Core'un diline çevrildiği yer de burasıdır.
        /// </summary>
        // ÜÇ ÇAĞIRAN, TEK ÇEVİRİ (tıklama akışı, yerleştirme kipi, sürükleme):
        // kopyalansaydı fare ile hayalet farklı hücreleri gösterirdi ve hiçbir
        // şey patlamazdı. DÖNÜŞ bool + out, nullable DEĞİL — "kamera yok" bir
        // programcı hatasıdır. Camera.main "MainCamera" ETİKETLİ kamerayı bulur.
        // ScreenToWorldPoint çevirisi olmasaydı aynı tıklama 1920'lik ve
        // 2560'lık ekranda farklı hücreyi seçerdi.
        private bool TryScreenPointToWorldCell(
            Vector2 screenPoint, out float worldX, out float worldY, out int x, out int y)
        {
            worldX = 0f;
            worldY = 0f;
            x = 0;
            y = 0;

            if (Camera.main == null)
            {
                Debug.LogError("[Board] No camera tagged MainCamera in the Scene.", this);
                return false;
            }

            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(screenPoint);
            worldX = worldPoint.x;
            worldY = worldPoint.y;

            // Vector3Int sınırın ötesine geçmez; "tahta içinde mi" sorusunu
            // soran taraf yine Battle'dır, bu metot değil.
            Vector3Int cell = unityGrid.WorldToCell(worldPoint);
            x = cell.x;
            y = cell.y;
            return true;
        }

        /// <summary>Hücre tahtanın içinde ve boş mu.</summary>
        // İKİ SORU TEK ÜYEDE ve sıra zorunlu: sınır önce sorulur, çünkü
        // TryGetUnit tahta dışı bir koordinatta anlamlı bir cevap veremez.
        // BU ÜYE OLMASAYDI SIRA BOZULURDU: üretim, hücre sorusundan ÖNCE
        // yapılamaz — üretim bekleme sayacını başlatır ve reddedilen bir
        // yerleştirme o sayacı yakardı; oyuncu neden beklediğini anlamazdı.
        // KURALIN KOPYASI DEĞİL, SORUNUN KAPISI: doluluğun tek sahibi hâlâ
        // tahtanın kendisi ve cevabı burada hesaplanmıyor, soruluyor.
        public bool IsCellFree(int x, int y)
        {
            return PreviewAt(x, y) == PlacementPreview.Placeable;
        }

        /// <summary>
        /// Bu hücreye bir şey konabilir mi, konamazsa NEDEN konamaz.
        /// </summary>
        // OYUNDA NE İŞE YARAR: sürükleme sürerken hayaletin rengi bu cevaba
        // göre yazılıyor; oyuncu parmağını kaldırmadan önce sonucu görüyor.
        //
        // ██ "KONABİLİR Mİ" SORUSUNUN TEK SAHİBİ BURASI ██
        // IsCellFree bu üyeye delege ediyor, kendi kuralını YAZMIYOR — eskiden
        // iki koşulu kendisi taşıyordu ve önizleme ikinci bir kopyasını
        // doğuracaktı. İki kopya, "boş hücre" tanımı değiştiği gün (örneğin
        // enkazın üstüne inşaya izin verildiği gün) sessizce ayrışırdı:
        // bırakma kabul eder, hayalet kırmızı gösterirdi.
        //
        // SIRA BİR KARARDIR: önce tahtanın DIŞI sorulur. Ters sırada, tahtanın
        // dışındaki bir hücre için TryGetUnit çağrılırdı ve orada duran hiçbir
        // şey olmadığı için cevap "boş" olurdu — yani tahta dışı yanlışlıkla
        // Placeable görünürdü.
        public PlacementPreview PreviewAt(int x, int y)
        {
            if (!battle.IsInsideGrid(x, y))
            {
                return PlacementPreview.OutsideBoard;
            }

            return battle.TryGetUnit(x, y, out Unit _)
                ? PlacementPreview.CellOccupied
                : PlacementPreview.Placeable;
        }

        /// <summary>
        /// İmlecin altındaki hücreyi verir — tahtanın DIŞINDA olsa bile.
        /// </summary>
        // <see cref="TryScreenPointToCell"/>'İN İKİZİ VE TEK FARKI SON SATIR:
        // o "tahtanın içinde mi" diye sorup dışarıyı reddediyor, bu sormuyor.
        // İkisi de gerekli ve karıştırılmamalı — BIRAKMA hâlâ içerideki sürümü
        // çağırıyor (tahta dışına bırakmak bir vazgeçmedir), yalnız ÖNİZLEME
        // bunu çağırıyor.
        //
        // ÖNİZLEME NEDEN DIŞARIYI DA İSTİYOR: operatörün cümlesi —
        // "sürükle-bırak yaparken o unit grid'in dışındakileri de hayalet
        // kısmını görebilmeliyiz ama kırmızılı hâlinde." Eski hâlde hayalet
        // tahtanın dışında tamamen KAYBOLUYORDU, yani oyuncu elindeki şeyin
        // hâlâ sürüklendiğini bile göremiyordu.
        public bool TryScreenPointToAnyCell(Vector2 screenPoint, out int x, out int y)
        {
            return TryScreenPointToWorldCell(screenPoint, out _, out _, out x, out y);
        }

        /// <summary>
        /// Bir kimliğin karşılığı olan yapıyı verir; o kimlik bir yapı değilse
        /// false.
        /// </summary>
        // DÜZ BİR İLETİM, ve öyle olması gerekiyor: tahta zaten birimleri ve
        // yapıları ayrı defterlerde tutuyor, buraya ikinci bir "bu bir yapı mı"
        // bayrağı koymak o ayrımın ikinci bir kopyasını üretirdi.
        public bool TryGetStructure(Unit identity, out Structure structure)
        {
            return battle.TryGetStructure(identity, out structure);
        }

        /// <summary>
        /// Bu yapının tepesinde bir sonraki üretime kaç saniye kaldığını
        /// gösterir.
        /// </summary>
        // OYUNDA NE İŞE YARAR: kışlanın ne zaman asker vereceği artık binaya
        // tıklayıp sağ panele bakmayı gerektirmiyor; sayı tahtanın üstünde.
        //
        // ŞERİT İLK ÇAĞRIDA KURULUYOR, DOĞUMDA DEĞİL — ve bu ölçülmüş bir
        // ayrım: yapı yerleştiğinde onun ÜRETİCİ olup olmadığını tahta bilmiyor
        // (`Produces` listesi çekirdek tanımında, tahtaya gelen şey görsel).
        // Doğumda kurulsaydı ya her binaya bir şerit takılırdı ya da tahta
        // üretim listesini ikinci kez okumak zorunda kalırdı. İlk çağrıda
        // kurmak, "kim gösterecek" sorusunun cevabını tek sahipte
        // (ProductionDirector) bırakıyor.
        public void ShowProductionCountdown(Unit identity, float remainingSeconds, float totalSeconds)
        {
            if (identity == null)
            {
                return;
            }

            if (!productionTimers.TryGetValue(identity, out ProductionTimerView timer) || timer == null)
            {
                if (!TryAttachProductionTimer(identity, out timer))
                {
                    return;
                }
            }

            timer.SetRemaining(remainingSeconds, totalSeconds);
        }

        /// <summary>
        /// Bir yapının görseline geri sayım şeridini takar.
        /// </summary>
        /// <returns>Şerit kurulabildiyse true.</returns>
        // AYNI DESENİN İKİNCİ UYGULAMASI ve gerekçeleri AttachHealthBar'da bir
        // kez yazılı, burada tekrar edilmiyor: ters ölçek (şerit sahibinin
        // ölçeğinden etkilenmemeli), yerel yükseklik (konum ebeveynin ölçeğiyle
        // çarpılıyor), havuzdan gelen görselde ZATEN kurulu olma ihtimali.
        //
        // SESSİZ ÇIKIŞ, LogError DEĞİL: sprite'sız bir tahta EditMode testinde
        // normaldir ve bağırsaydı üretim yolundan geçen her test kırmızıya
        // dönerdi — bu tuzağa proje bir kez düştü, 482 testin 6'sı kırılmıştı.
        // Şikâyet BuildHoverHighlight'ta bir kez, doğuşta ediliyor.
        private bool TryAttachProductionTimer(Unit identity, out ProductionTimerView timer)
        {
            timer = null;

            if (healthBarSprite == null
                || !structureViews.TryGetValue(identity, out StructureView structureView)
                || structureView == null)
            {
                return false;
            }

            Transform parent = structureView.transform;
            Vector3 parentScale = parent.localScale;

            // YÜKSEKLİK CAN BARINDAN OKUNUYOR, GÖRSELDEN YENİDEN HESAPLANMIYOR:
            // barın nerede durduğunu bilen tek yer barın kendisi ve iki ayrı
            // hesap, birinin payı değiştiği gün sessizce ayrışırdı. Bar yoksa
            // (sprite atanmamış) şerit de kurulmuyor — zaten yukarıdaki kapı
            // aynı sprite'ı soruyor.
            float localHeight = ProductionTimerMargin / (parentScale.y > 0.0001f ? parentScale.y : 1f);
            if (healthBars.TryGetValue(identity, out HealthBarView bar) && bar != null)
            {
                localHeight += bar.transform.localPosition.y;
            }

            ProductionTimerView existing =
                parent.GetComponentInChildren<ProductionTimerView>(includeInactive: true);
            if (existing != null)
            {
                existing.SetHeightAboveOwner(localHeight);
                productionTimers[identity] = existing;
                timer = existing;
                return true;
            }

            var go = new GameObject("ProductionTimer");
            go.transform.SetParent(parent, worldPositionStays: false);
            go.transform.localScale = new Vector3(
                parentScale.x > 0.0001f ? 1f / parentScale.x : 1f,
                parentScale.y > 0.0001f ? 1f / parentScale.y : 1f,
                1f);

            timer = go.AddComponent<ProductionTimerView>();
            timer.Build(healthBarSprite, ProductionTimerSortingOrder);
            timer.SetHeightAboveOwner(localHeight);

            productionTimers[identity] = timer;
            return true;
        }

        /// <summary>
        /// Savaşın saatini ilerletir ve ceset süresi dolanları hem savaştan hem
        /// ekrandan kaldırır.
        /// </summary>
        // ZAMANI BURADAN VERMEK ZORUNLU: UnitLifecycle bilerek Time.deltaTime
        // okumuyor — EditMode'da o değer sıfır dönmüyor ve testi sessizce
        // anlamsızlaştırıyordu. TEMİZLİK YOKLAMA DEĞİL TOPLU: yoklama ancak
        // GÖRSELİ olan birimleri görür, oysa temizlenmesi gereken şey savaşın
        // kaydıdır. Olay ile süpürme ÇELİŞMEZ, farklı iki soruya cevap veriyor.
        // → BoardAdapter.md#advancebattletime
        private void AdvanceBattleTime()
        {
            battle.Tick(Time.deltaTime);

            // YAPILARIN ATEŞİ SAVAŞIN SAATİYLE AYNI KAREDE İLERLER, ama Tick'in
            // İÇİNDE değil: ateş temposu bir EKRAN ayarıdır (Inspector'daki
            // saniye) ve savaş çekirdeğine taşınsaydı EditMode'da sınanan kural
            // katmanı bir görsel tempoya bağlanırdı.
            AdvanceStructureFire(Time.deltaTime);

            // Can barları temizlikten ÖNCE tazeleniyor: sırası ters olsaydı
            // ölen bir birimin barı, görseli silinmeden önceki son karede eski
            // değerinde donmuş kalırdı.
            RefreshHealthBars();

            // YAPI GÖRSELLERİ AYNI SEBEPLE VE AYNI YERDE: enkaz penceresi
            // temizlikten önce başlıyor ve tam o pencerede yıkık binanın
            // kararmış görünmesi gerekiyor. Temizlikten SONRA çağrılsaydı
            // yıkımın ilk karesi ayakta duran bir bina gösterirdi.
            RefreshStructureVisuals();
            AnnounceTurnIfChanged();

            if (battle.RemoveReadyForCleanup(cleanupBuffer) == 0)
            {
                return;
            }

            for (int i = 0; i < cleanupBuffer.Count; i++)
            {
                DespawnView(cleanupBuffer[i]);

                // YAYIN GÖRSELİN YANINDA, İÇİNDE DEĞİL: DespawnView görseli
                // bulamadığında sessizce dönüyor, oysa bu olayın anlattığı şey
                // savaşın kaydından çıkış — ikisi aynı satıra konsaydı görseli
                // olmayan bir kimlik dinleyiciye hiç duyurulmazdı.
                // SIRA DA BİR KARARDIR: önce ekran temizlenir, sonra haber
                // verilir; ters sırada dinleyici, ekranda hâlâ duran bir enkazı
                // yokmuş gibi işleyebilirdi.
                UnitRemoved?.Invoke(cleanupBuffer[i]);
            }
        }

        /// <summary>
        /// Saldırı profili olan ayakta yapıların kendi bekleme süreleriyle ateş
        /// etmesini sağlar.
        ///
        /// OYUNDA NE İŞE YARAR: kule, karargâh ya da top gibi saldıran binalar
        /// menzillerine giren düşmana kendiliğinden vurur. Bu olmadan "saldıran
        /// yapı" ekranda duran ölü içeriktir: profili vardır, hiç ateş etmez.
        /// </summary>
        // ANAHTARLAR ÖNCE KOPYALANIYOR: bir atış hedefi yıkabilir, yıkım
        // temizliğe kadar gidebilir ve temizlik structureViews'a dokunur —
        // sözlüğü gezerken değiştirmek gezintiyi patlatır.
        //
        // SAYAÇ HEDEF YOKKEN DOLU BEKLER, sıfırlanmaz: kule "yüklü" durur ve
        // menzile giren ilk düşmana anında vurur. Sıfırlansaydı düşman tam
        // sayacın sıfırlandığı karede girdiğinde bir bekleme daha yer ve oyuncu
        // kuleyi tutuk sanırdı.
        //
        // SIRA KURALI ATIŞTAN ÖNCE SORULUYOR ve sayaç o dalda İLERLEMİYOR: sıra
        // tabanlı kipte bekleyen bir kule, sırası gelene kadar her karede
        // reddedilen bir saldırı denemesi yapar ve Console'u ret satırıyla
        // doldururdu.
        private void AdvanceStructureFire(float deltaSeconds)
        {
            if (structureViews.Count == 0)
            {
                return;
            }

            structureFireBuffer.Clear();
            foreach (KeyValuePair<Unit, StructureView> pair in structureViews)
            {
                structureFireBuffer.Add(pair.Key);
            }

            // SIFIR ÖLÇEĞE KARŞI SİGORTAYLA AYNI GEREKÇE: [Min] yalnız Inspector
            // YAZARKEN çalışır, sahnede o anahtar hiç yoksa alan 0 doğar ve her
            // kare ateş eden bir kule ekranı kilitlerdi.
            float fallbackWindow = structureFireSeconds > 0.01f ? structureFireSeconds : 1.5f;

            for (int i = 0; i < structureFireBuffer.Count; i++)
            {
                Unit shooter = structureFireBuffer[i];

                // YIKIK YA DA SALDIRMAYAN YAPI SAYMAZ: enkazın ateş etmesi
                // oyuncuya yıkımın işe yaramadığını söylerdi.
                if (!battle.TryGetStructure(shooter, out Structure structure)
                    || !structure.IsStanding
                    || !structure.CanAttack)
                {
                    structureFireTimers.Remove(shooter);
                    continue;
                }

                if (!battle.Turn.AllowsAction(structure.Team))
                {
                    continue;
                }

                float window = FireWindowFor(structure, fallbackWindow);

                float waited = structureFireTimers.TryGetValue(shooter, out float previous)
                    ? previous + deltaSeconds
                    : deltaSeconds;

                if (waited < window)
                {
                    structureFireTimers[shooter] = waited;
                    continue;
                }

                if (!TryFindStructureTarget(shooter, structure, out Unit target, out int targetX, out int targetY))
                {
                    structureFireTimers[shooter] = window;
                    continue;
                }

                structureFireTimers[shooter] = 0f;

                // SIRAYI HARCAMAYAN YOL: kule kendiliğinden ateş ediyor ve
                // isabette sırayı devretseydi, sıra tabanlı kipte oyuncunun
                // hakkını kendi binası düşmana veriyor olurdu. Oyuncunun
                // tıklayarak yaptırdığı saldırı hâlâ sıradan Attack'i çağırıyor;
                // farkı yaratan şey emri kimin verdiği.
                AttackOutcome outcome =
                    BattleActions.AttackWithoutSpendingTurn(battle, shooter, target);
                ReactToAttack(shooter, outcome, target, targetX, targetY);
            }
        }

        /// <summary>
        /// Bir yapının iki atışı arasında bekleyeceği saniye.
        /// </summary>
        /// <param name="fallback">
        /// Yapının kendi profilinde bekleme yazılı değilse kullanılacak sayı.
        /// </param>
        // TEK SAYI, İKİ KAYNAK VE SIRA BİR KARARDIR: bekleme kuralının sahibi
        // artık Core ve AttackAction, süresi dolmadan gelen atışı zaten
        // RejectedOnCooldown ile reddediyor. Ekranın sayacı o sayıdan
        // beslenmeseydi taret ikisinin BÜYÜĞÜ kadar bekler ve Inspector'daki
        // sayı sessizce yalan söylerdi.
        //
        // TAHTANIN SAYACI TAMAMEN KALDIRILMADI ve gerekçe Console: sayaç
        // silinseydi AdvanceStructureFire her karede saldırıyı DENER, Core her
        // karede reddeder ve ReactToAttack o retleri oyuncuya yazardı. Otomatik
        // ateşin reddi bir oyuncu eylemi değil; yazılması gereken tek ret
        // oyuncunun kendi tıklamasıdır.
        private static float FireWindowFor(Structure structure, float fallback)
        {
            float own = structure.AttackProfile != null
                ? structure.AttackProfile.CooldownSeconds
                : 0f;

            return own > 0.01f ? own : fallback;
        }

        /// <summary>
        /// Bir yapının menzilindeki EN YAKIN geçerli hedefi bulur.
        /// </summary>
        /// <returns>Menzilde vurulabilir bir düşman yoksa false.</returns>
        // MENZİL YAPININ KENDİ PROFİLİNDEN, Inspector'dan DEĞİL: aynı sahnede
        // bir hisar ile bir okçu kulesi yan yana durabilir ve tek bir sayı
        // ikisini de yanlış anlatırdı.
        //
        // EŞİTLİKTE TARAMA SIRASI KAZANIR ve karşılaştırma bilerek katı ("<",
        // "<=" değil): iki düşman aynı uzaklıktaysa tahtayı soldan sağa, alttan
        // yukarı gezerken ÖNCE görülen kazanır. Belirlenimci olması bir konfor
        // değil, aynı durumun aynı sonucu vermesi demek.
        //
        // ██ DÜŞMAN YAPILARI ARTIK HEDEF, VE ESKİ GEREKÇE ÖLÇÜLDÜ ██
        // Burada "binaların birbirini vurması oyunun kararını oyuncudan alırdı"
        // yazıyordu; sonucu şuydu — kuleler düşman üssünü hiç görmüyor,
        // oyuncunun kurduğu savunma hattı düşman binası dibine kadar gelse bile
        // sessiz kalıyordu. Motor bu vuruşu ZATEN destekliyor.
        //
        // EŞİT UZAKLIKTA SAVAŞÇI KAZANIR: asker geri vurur ve yer değiştirir,
        // bina ikisini de yapamaz — önce hareketli tehdidi susturmak, yıkımı
        // erteleyerek daha az hasar yemek demek. Uzaklık farkı varsa tip hiç
        // sorulmuyor; yakın olan kazanıyor.
        private bool TryFindStructureTarget(
            Unit shooter, Structure structure, out Unit target, out int targetX, out int targetY)
        {
            target = null;
            targetX = 0;
            targetY = 0;

            if (!battle.TryGetPosition(shooter, out int fromX, out int fromY))
            {
                return false;
            }

            int range = structure.AttackProfile.Range;
            int bestDistance = int.MaxValue;
            bool bestIsFighter = false;

            // ██ ULAŞAMAYACAĞIN YERE BAKMA ██
            // Burada `for y in battle.Height` / `for x in battle.Width` duruyordu:
            // kule, MENZİLİNİN İÇİNDEKİ en yakın düşmanı bulmak için tahtanın
            // TAMAMINI geziyordu. 10x5'te 50 hücreydi ve bedeli gözlenemezdi;
            // 100x50'de çağrı başına 5000 hücre oldu ve bunların 5000 - (2r+1)²
            // tanesi zaten `distance > range` diye eleniyordu — yani tarama,
            // sonucu baştan belli olan bir işi yapıyordu.
            //
            // ÖLÇEK: pencere (2 × range + 1)². Menzil tanımdan geliyor
            // (`AttackProfile.Range`) ve bugün en büyüğü 4; yani 81 hücre.
            // TAVAN: menzil tahtanın yarısını geçtiği gün pencere tahtanın
            // kendisi kadar olur ve bu kelepçe anlamını yitirir — o gün çözüm
            // pencereyi daraltmak değil, birim defterini gezmektir.
            //
            // KELEPÇE TAHTAYA DA UYGULANIYOR: pencere kenarda taşabilir ve
            // `TryGetUnit` tahta dışı bir hücreye sorulduğunda zaten false
            // döner, ama sınır burada kesiliyor — döngünün kendisi tahtanın
            // dışına hiç çıkmasın diye. Kayıp yok: metrik Chebyshev, yani bu
            // kare menzil kümesinin birebir kendisi (gerekçe aşağıda).
            int minX = Mathf.Max(0, fromX - range);
            int maxX = Mathf.Min(battle.Width - 1, fromX + range);
            int minY = Mathf.Max(0, fromY - range);
            int maxY = Mathf.Min(battle.Height - 1, fromY + range);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (!battle.TryGetUnit(x, y, out Unit standing)
                        || ReferenceEquals(standing, shooter))
                    {
                        continue;
                    }

                    // ██ PENCERE HİÇBİR HEDEF KAÇIRMIYOR — VE BU BİR ██
                    // ██ TAHMİN DEĞİL, METRİĞİN KENDİSİNDEN ÇIKIYOR ██
                    // `GridDistance` CHEBYSHEV: uzaklık = max(|dx|, |dy|).
                    // Yani `distance <= range` ile "|dx| <= range VE
                    // |dy| <= range" AYNI kümedir; yukarıdaki kare, menzil
                    // kümesinin ta kendisi. Manhattan olsaydı kare bir ÜST
                    // küme olurdu ve yine kaçırmazdı.
                    //
                    // O HÂLDE BU SATIR NEDEN DURUYOR: bugün gereksiz, yarın
                    // zorunlu. `GridDistance.Between`'in kendi yorumu metriği
                    // değiştirmenin ne anlama geldiğini tartışıyor; metrik
                    // Öklit'e dönseydi köşeler pencerede kalır ama menzilin
                    // dışına düşerdi ve bu satır olmadan kule köşedeki bir
                    // düşmanı vurmaya çalışırdı. Kelepçenin bedeli bir
                    // karşılaştırma; kaldırmanın bedeli, metriği değiştiren
                    // kişinin buraya bakmak zorunda olduğunu bilmemesi.
                    int distance = GridDistance.Between(fromX, fromY, x, y);
                    if (distance > range)
                    {
                        continue;
                    }

                    if (!IsHostileTarget(standing, structure.Team, out bool isFighter))
                    {
                        continue;
                    }

                    bool better = distance < bestDistance
                                  || (distance == bestDistance && isFighter && !bestIsFighter);
                    if (!better)
                    {
                        continue;
                    }

                    bestDistance = distance;
                    bestIsFighter = isFighter;
                    target = standing;
                    targetX = x;
                    targetY = y;
                }
            }

            return target != null;
        }

        /// <summary>
        /// Bu kimlik ateş edilebilir bir düşman mı: ayakta bir savaşçı ya da
        /// yıkılmamış bir yapı.
        /// </summary>
        /// <param name="isFighter">Hedef bir savaşçıysa true, yapıysa false.</param>
        // DÜŞMÜŞ BİRİME ATEŞ EDİLMİYOR: onun ölümünü zaten kendi sayacı
        // getiriyor ve kule düşmüş bir bedene ateş ederek ayaktaki tehdidi
        // görmezden gelirdi. Aynı ayrım yapı tarafında "enkaza ateş etme"
        // olarak duruyor.
        private bool IsHostileTarget(Unit standing, Team shooterTeam, out bool isFighter)
        {
            isFighter = false;

            if (battle.TryGetCombatant(standing, out Combatant combatant))
            {
                isFighter = true;
                return combatant.Team != shooterTeam && combatant.State == UnitState.Alive;
            }

            return battle.TryGetStructure(standing, out Structure other)
                   && other.Team != shooterTeam
                   && other.IsStanding;
        }

        /// <summary>
        /// Sıradaki yerleştirmenin hangi binaya ait olduğunu tahtaya söyler.
        ///
        /// OYUNDA NE İŞE YARAR: oyuncu paletten bir bina sürüklerken imlecin
        /// altında O binayı görür ve bıraktığında aynısı kurulur. Bu çağrı
        /// olmasaydı her bina, hayalete atanmış tek sprite olarak çizilirdi.
        /// </summary>
        // TAHTA SEÇİMİ BİLMEZ, KENDİSİNE SÖYLENİR: hangi binanın seçili olduğunu
        // palet bilir ve palet bu dosyayı tanımaz. Bilgi bu yüzden yukarıdan
        // AŞAĞI akıyor; tahta yukarı doğru soru sormuyor.
        // ██ null SİMGE BİR "DEĞİŞİKLİK YOK" DEĞİL, BİR SİLME EMRİDİR ██
        // Eski hâl yalnız sprite doluyken yazıyordu ve sonucu şuydu: simgesi
        // olmayan bir birim sürüklenirken hayalet ÖNCEKİ binanın görselini
        // taşımaya devam ediyordu — oyuncu ekranda bir baraka görüp elinde bir
        // asker tutuyordu. Görsel yalan söylemesin diye null artık hem
        // pendingStructureSprite'ı hem hayaletin sprite'ını siliyor.
        //
        // HAYALET AYRICA GİZLENİYOR: sprite'ı silinmiş ama açık kalan bir
        // SpriteRenderer ekranda hiçbir şey çizmez, yani gizlemek görünüşte
        // gereksizdir. Gereksiz değil: açık kalan hayalet, bir sonraki
        // SetPlacementGhost(true, ...) çağrısı gelmeden de "taşınıyor" görünen
        // bir durumdur ve o durumu okuyan tek şey bu bileşenin kendisi.
        public void SetPlacementVisual(Sprite sprite)
        {
            SetPlacementVisual(sprite, sizeInCells: 0f);
        }

        /// <summary>
        /// Sıradaki yerleştirmenin simgesini VE kaç hücre kapladığını söyler.
        ///
        /// OYUNDA NE İŞE YARAR: oyuncunun sürüklerken gördüğü hayalet, bıraktığı
        /// binayla aynı BOYDA olur — önizleme artık yalnız resmi değil ölçüyü de
        /// vaat ediyor.
        /// </summary>
        /// <param name="sizeInCells">
        /// Yapının tanımındaki <c>BoardSizeInCells</c>. Sıfır ya da eksi ise
        /// palet ölçü söylememiş demektir ve varsayılan kullanılır.
        /// </param>
        // AYRI BİR AŞIRI YÜKLEME, tek imzayı DEĞİŞTİRMEK DEĞİL: tek argümanlı
        // sürüm IPlacementBoard sözleşmesinde duruyor ve onu değiştirmek
        // arayüzü uygulayan sahte tahtaları da kırardı.
        public void SetPlacementVisual(Sprite sprite, float sizeInCells)
        {
            // ██ PALETTEN BİNA ALMAK ARTIK HİÇBİR EMRİ DÜŞÜRMÜYOR ██
            // Burada `CancelPendingStrike()` duruyordu ve gerekçesi şuydu:
            // "fareyle bina alan oyuncunun savaşçısı sürükleme sürerken
            // kendiliğinden vururdu." O cümle emrin TAHTAYA ait olduğu dünyada
            // doğruydu. Emir birime ait olunca tam tersine döndü: oyuncunun
            // paletten bina sürüklerken savaşçısının vurmaya DEVAM etmesi,
            // paralel oyunun kendisidir. Bina koymak savaşçıya verilmiş bir
            // emri neden kessin?
            // → Docs/deep/konular/09-kararlarin-cevrilmesi.md (madde 2)

            pendingStructureSprite = sprite;
            pendingStructureSizeInCells = sizeInCells;

            if (placementGhost == null)
            {
                return;
            }

            // ██ null SİMGE PALETİ UNUTTURUR, HAYALETİ YOK ETMEZ ██
            // Eski hâl hayaletin sprite'ını da siliyordu ve ProductionDirector
            // her bırakışta (başarılı ya da başarısız) buraya null geçtiği için
            // sahnede YAZILI olan yedek bir daha geri gelmiyordu: B tuşuna basan
            // oyuncu hiçbir hayalet göremiyor, tıkladığı hücreye görünmez bir
            // bina oturuyordu. Silme emrinin anlamı korunuyor — paletin simgesi
            // gerçekten unutuluyor — yalnız yedek sağ kalıyor.
            placementGhost.sprite = sprite != null ? sprite : authoredGhostSprite;

            if (sprite == null)
            {
                placementGhost.enabled = false;
            }

            // ÖNİZLEME İLE SONUÇ TEK HESAPTAN: hayaletin ölçeği de binanınkiyle
            // aynı üyeden geliyor, yani oyuncunun gördüğü boy ile kurulan boy
            // ayrışamıyor. Gizli hayaletin ölçeği de yazılıyor, çünkü klavyeli
            // kip onu bir sonraki B tuşunda AYNEN açıyor ve eski binanın boyunda
            // açılmasının hiçbir gerekçesi olmazdı.
            placementGhost.transform.localScale = StructureLocalScale(placementGhost.sprite);
        }

        /// <summary>
        /// Bir yapı görselinin hücreye oturan yerel ölçeği.
        /// </summary>
        // ÖLÇÜNÜN SAHİBİ BoardSizing, BU DOSYA DEĞİL: burada ne bir piksel-birim
        // bölmesi ne de bir en-boy düzeltmesi var. Buranın tek işi hangi sayının
        // sorulacağına karar vermek — paletin söylediği ölçü, o susmuşsa
        // varsayılan.
        private Vector3 StructureLocalScale(Sprite sprite)
        {
            return BoardSizing.LocalScaleFor(sprite, PendingSizeInCells, CellSize);
        }

        /// <summary>
        /// Bir hücrenin dünya ölçüsü; ızgara henüz bulunmamışsa bir birimlik kare.
        /// </summary>
        // GERİ ÇEKİLME ŞART VE ÖLÇÜLDÜ: `unityGrid` yalnız Awake'te doluyor,
        // EditMode testleri ise Awake'i hiç koşturmuyor — çıplak bir
        // `unityGrid.cellSize` okuması ölçüye uğrayan her testi
        // NullReferenceException ile düşürürdü.
        private Vector3 CellSize => unityGrid != null ? unityGrid.cellSize : Vector3.one;

        /// <summary>
        /// Sıradaki yapının ölçüsü: palet söylediyse onunki, susmuşsa varsayılan.
        /// </summary>
        // TEK YERDE, İKİ ÇAĞIRAN: ölçek ile can barı yüksekliği aynı sayıyı
        // okumak zorunda, yoksa bar binanın tepesinden kayardı.
        private float PendingSizeInCells => pendingStructureSizeInCells > 0.01f
            ? pendingStructureSizeInCells
            : DefaultStructureSizeInCells;

        /// <summary>
        /// Şu an seçili olan savaşçıyı ya da yapıyı oyundan tamamen kaldırır.
        ///
        /// OYUNDA NE İŞE YARAR: oyuncunun yanlış yere koyduğu binayı ya da artık
        /// istemediği birimi geri alma yolu. Arayüzdeki ÇÖP KUTUSU düğmesi bu
        /// metodu çağırır; klavyedeki <see cref="removeSelectedKey"/> de aynı
        /// yere gider — iki kapı, TEK mekanizma.
        /// </summary>
        /// <returns>Kaldırılacak bir seçim varsa ve kaldırıldıysa true.</returns>
        // TEK GİRİŞ NOKTASI, ÇÜNKÜ SIRA KIRILGAN: kaldırmanın üç katmanı var ve
        // ikisi ötekini göremiyor. Sırayı burada bir kez yazıp her çağıranı buraya
        // yönlendirmek, aynı sıranın düğme ve klavye yollarında AYRI AYRI
        // yazılmasını (ve bir gün ayrışmasını) engelliyor.
        public bool RemoveSelected()
        {
            if (selectedUnit == null)
            {
                Debug.Log("[Board] Nothing is selected; there is nothing to remove.", this);
                return false;
            }

            Unit doomed = selectedUnit;

            // ① SEÇİM ÖNCE BIRAKILIR ki panel ve palet, birazdan var olmayacak bir
            // kimliği göstermeyi bıraksın. ClearSelection kullanılıyor çünkü
            // görsel HÂLÂ ayakta — çerçeveyi kapatacak biri var.
            ClearSelection();

            // ② SAVAŞIN KAYDINDAN ÇIKAR. Tahtadaki hücre de burada boşalır;
            // yapılmasaydı ekranda kimse durmadığı hâlde o hücreye yeni bir şey
            // konamazdı.
            if (!battle.RemoveUnit(doomed))
            {
                Debug.LogWarning($"[Board] '{doomed.Name}' was not part of this battle.", this);
            }

            // ③ EKRANDAN SİL, ④ SONRA DUYUR. Sıra temizlik döngüsündekinin
            // birebir aynısı ve gerekçesi orada yazılı: ters sırada dinleyici,
            // ekranda hâlâ duran bir nesneyi yokmuş gibi işlerdi.
            DespawnView(doomed);
            UnitRemoved?.Invoke(doomed);

            Debug.Log($"[Board] '{doomed.Name}' was removed by the player.", this);
            return true;
        }

        /// <summary>
        /// Çöp kutusu düğmesinin bağlandığı kapı.
        /// </summary>
        // AYRI BİR ÜYE, ÇÜNKÜ IMZA ŞART: Unity'nin düğme olayı yalnızca void
        // dönen metotları kalıcı dinleyici olarak bağlayabilir. RemoveSelected'ın
        // bool dönüşü çağıranlar için anlamlı, o yüzden değiştirilmedi — kural
        // tek yerde kaldı, buraya yalnız imza uyarlandı.
        public void RemoveSelectedFromUi()
        {
            RemoveSelected();
        }

        private void BuildCellVisuals()
        {
            // HALKA ÇİMDEN ÖNCE ve aşağıdaki erken çıkışın ÜSTÜNDE: kenar
            // görselleri ile zemin görselleri iki ayrı Inspector alanından
            // geliyor ve birinin eksikliği ötekini yok saymamalı.
            BuildBorderVisuals();

            // LogError, Log değil: bu bir PROGRAMCI hatasıdır (kurulum eksik),
            // oyun akışının normal bir sonucu değil. return ile birlikte gelir —
            // sprite yoksa 15 görünmez GameObject üretmektense gürültüyle durmak
            // yeğdir. → BoardAdapter.md#buildcellvisuals
            if (terrainSprites == null || terrainSprites.Length == 0)
            {
                Debug.LogError(
                    "[Board] terrainSprites is empty. Assign at least one Sprite in the Inspector.",
                    this);
                return;
            }

            // ██ ÇEVRİLEN KARAR: HÜCRE BAŞINA BİR GameObject → TEK Tilemap ██
            // Burada şu duruyordu ve 10x5'lik bir tahtada kusursuzdu:
            //
            //     for (int x = 0; x < battle.Width; x++)
            //     {
            //         for (int y = 0; y < battle.Height; y++)
            //         {
            //             CreateCellVisual(x, y);   // new GameObject + SpriteRenderer
            //         }
            //     }
            //
            // KIRILAN ŞEY BİR ÖLÇÜMDÜR, BİR ZEVK DEĞİL: operatör tahtayı
            // 100x50 yaptı ve Console şunu bastı — "[Board] built 100x50 =
            // 5000 cells." Halkayla birlikte 5616 GameObject, 5616 Transform,
            // 5616 SpriteRenderer. Üçü de her karede ayrı ayrı kültürleniyor
            // (culling) ve sıralanıyor.
            // NE ZAMAN KAZANIRDI: hücrelerin TEK TEK canlanması gerektiği gün —
            // her hücrenin kendi animasyonu, kendi çarpıştırıcısı ya da kendi
            // tıklama alanı olsaydı. Bu tahtada hiçbiri yok: tıklamayı Grid
            // matematiği çözüyor, hücreler doğduktan sonra hiç değişmiyor.
            // → Docs/deep/konular/09-kararlarin-cevrilmesi.md (madde 10)
            Tilemap ground = EnsureTilemap(GroundMapName, GroundSortingOrder);
            if (ground == null)
            {
                return;
            }

            var bounds = new BoundsInt(0, 0, 0, battle.Width, battle.Height, 1);
            var tiles = new TileBase[battle.Width * battle.Height];

            for (int y = 0; y < battle.Height; y++)
            {
                for (int x = 0; x < battle.Width; x++)
                {
                    // İNDEKS DÜZENİ Tilemap'İN KENDİ DÜZENİ: satır satır, x
                    // hızlı eksen. Ters yazılsaydı desen 90 derece dönerdi ve
                    // hiçbir şey patlamazdı — sessiz bir görsel hata.
                    tiles[x + (y * battle.Width)] = TileFor(PickTerrainSprite(x, y));
                }
            }

            // TEK ÇAĞRI, 5000 SetTile DEĞİL: her SetTile kendi karo bloğunu
            // yeniden inşa ettiriyor; blok hâlinde yazmak o işi bir kez yapıyor.
            ground.SetTilesBlock(bounds, tiles);

            Debug.Log(
                $"[Board] built {battle.Width}x{battle.Height} = {battle.CellCount} cells on one tilemap.",
                this);
        }

        /// <summary>
        /// Bu sprite'ı çizen karoyu verir; aynı sprite için hep AYNI karo.
        /// </summary>
        // ██ FLYWEIGHT — VE BU KEZ ADI KONMUŞ HÂLİ ██
        // Projede Flyweight zaten vardı (UnitBlueprint, AttackProfile: paylaşılan
        // değişmez tanımlar). Bu üye aynı desenin üçüncü örneği ve baskısı
        // ÖLÇÜLMÜŞ: 5000 hücre en fazla `terrainSprites.Length` farklı görünüm
        // taşıyor. Karo başına bir nesne kurulsaydı 5000 ScriptableObject
        // doğardı; sözlük onu görünüm sayısına indiriyor.
        //
        // DIŞSAL DURUM KARONUN İÇİNDE DEĞİL: "hangi hücre" bilgisi Tilemap'in
        // kendi tablosunda yaşıyor, karoda yalnız "nasıl görünür" var. Flyweight'i
        // Flyweight yapan ayrım tam olarak budur.
        private TileBase TileFor(Sprite sprite)
        {
            if (sprite == null)
            {
                return null;
            }

            if (tileCache.TryGetValue(sprite, out TileBase cached))
            {
                return cached;
            }

            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            tileCache[sprite] = tile;
            return tile;
        }

        /// <summary>
        /// Adı verilen tilemap'i bulur, yoksa kurar.
        /// </summary>
        // ÇOCUK NESNE, BU NESNENİN ÜSTÜNDE BİLEŞEN DEĞİL: Tilemap kendi
        // TilemapRenderer'ını istiyor ve iki katman (zemin, halka) iki AYRI
        // sıralama değeri taşıyor. Tek nesneye iki Tilemap konamaz.
        //
        // GRID ARANMIYOR ÇÜNKÜ ZATEN BURADA: Tilemap ebeveyn zincirinde bir
        // Grid istiyor ve bu bileşen Grid ile AYNI nesnede duruyor
        // (`unityGrid = GetComponent<Grid>()`), yani çocuk olmak yetiyor.
        private Tilemap EnsureTilemap(string name, int sortingOrder)
        {
            Transform existing = transform.Find(name);
            if (existing != null)
            {
                var found = existing.GetComponent<Tilemap>();
                if (found != null)
                {
                    // ESKİ İÇERİK SİLİNİYOR: araç ya da bir önceki koşum aynı
                    // adı bırakmış olabilir ve üstüne yazmak eski karoları
                    // tahtanın dışında asılı bırakırdı.
                    found.ClearAllTiles();
                    return found;
                }
            }

            var go = new GameObject(name);
            go.transform.SetParent(transform, worldPositionStays: false);
            go.transform.localPosition = Vector3.zero;

            var map = go.AddComponent<Tilemap>();
            var mapRenderer = go.AddComponent<TilemapRenderer>();
            mapRenderer.sortingOrder = sortingOrder;
            return map;
        }

        /// <summary>
        /// Hücre koordinatından zemin sprite'ı seçer.
        /// </summary>
        private Sprite PickTerrainSprite(int x, int y)
        {
            // DETERMİNİSTİK: aynı hücre her Play'de aynı sprite'ı alır; Random
            // olsaydı gördüğün bir hatayı tekrar üretmek imkânsızlaşırdı. 7 ve 13
            // asaldır — ortak bölen olmaması düzenli şerit desenini engeller.
            // → BoardAdapter.md#pickterrainspriteint-x-int-y
            int index = (x * 7 + y * 13) % terrainSprites.Length;
            return terrainSprites[index];
        }

        /// <summary>
        /// Oynanabilir ızgaranın çevresine yalnız GÖRSEL bir kenar halkası çizer.
        ///
        /// OYUNDA NE İŞE YARAR: çimin dışı artık boşluk değil. Kenardaki bir yapı
        /// kendi hücresinden büyük çizildiği için her yöne taşıyor ve taşan
        /// kısmının altında zemin bulamıyordu; halka o taşmayı yutar ve oyuncuya
        /// tahtanın nerede bittiğini gözle gösterir.
        /// </summary>
        // ██ HALKA OYUNUN KURALINA DAHİL DEĞİL ██ ve tek satırlık kanıtı şu:
        // aşağıda ne battle.IsInsideGrid'e ne IsCellFree'ye dokunuluyor, hiçbir
        // kimlik doğmuyor, savaşa hiçbir şey katılmıyor. Halka hücrelerinin
        // indeksleri eksi taraftadır ve sınır kuralı onları zaten reddediyor —
        // yani kural katmanının değişmesi GEREKMİYOR, bu bizim lehimize.
        //
        // KONUM AYNI KAPIDAN: CellCentre, yani unityGrid.GetCellCenterWorld.
        // İkinci bir konum hesabı açılsaydı hücre boyu ya da aralık değiştiği
        // gün halka çimden kayardı ve hiçbir şey patlamazdı.
        //
        // DİZİ BOŞ BIRAKILIRSA HALKA ÇİZİLMEZ: bugünkü sahneler bu üyeden hiç
        // etkilenmesin diye. Aynı şey kalınlık sıfır olduğunda da geçerli.
        private void BuildBorderVisuals()
        {
            ClearBorderVisuals();

            if (borderSprites == null || borderSprites.Length == 0 || borderThickness <= 0)
            {
                return;
            }

            Tilemap border = EnsureTilemap(BorderMapName, BorderSortingOrder);
            if (border == null)
            {
                return;
            }

            // ██ GEOMETRİ SAHİBİ DEĞİŞMEDİ ██
            // CollectBorderCells hâlâ hangi hücrelerin halkaya ait olduğunu
            // söylüyor ve KENDİ testleri duruyor. Değişen tek şey o listenin
            // ne ürettiği: eskiden hücre başına bir GameObject, şimdi bir karo
            // dizisindeki bir girdi.
            List<Vector2Int> cells = CollectBorderCells(battle.Width, battle.Height, borderThickness);

            int minX = -borderThickness;
            int minY = -borderThickness;
            int sizeX = battle.Width + (2 * borderThickness);
            int sizeY = battle.Height + (2 * borderThickness);

            // DİZİ BAŞTA BOŞ, yani oynanabilir alanın karşılığı null kalıyor.
            // null bir karo "burada hiçbir şey yok" demek — halkanın çime hiç
            // karışmamasının güvencesi artık bir atlama satırı değil, dizinin
            // kendi boşluğu.
            var tiles = new TileBase[sizeX * sizeY];

            for (int i = 0; i < cells.Count; i++)
            {
                int x = cells[i].x;
                int y = cells[i].y;
                tiles[(x - minX) + ((y - minY) * sizeX)] = TileFor(PickBorderSprite(x, y));
            }

            border.SetTilesBlock(new BoundsInt(minX, minY, 0, sizeX, sizeY, 1), tiles);
        }

        /// <summary>
        /// Halkanın kaplayacağı hücreleri sayar: dış dikdörtgenin tamamı, eksi
        /// oynanabilir ızgara.
        /// </summary>
        // ÖLÇÜ ALANDAN DEĞİL PARAMETREDEN GELİYOR ve bu bir test kararı: bu üye
        // motora, sahneye ve Inspector'a hiç dokunmuyor, dolayısıyla EditMode'da
        // Awake koşmadan sınanabiliyor. Halkanın hücre kümesi bu dosyada
        // sınanabilen tek geometri parçası ve bu yüzden ayrı duruyor.
        private List<Vector2Int> CollectBorderCells(int width, int height, int thickness)
        {
            var cells = new List<Vector2Int>();

            if (thickness <= 0 || width <= 0 || height <= 0)
            {
                return cells;
            }

            for (int y = -thickness; y < height + thickness; y++)
            {
                for (int x = -thickness; x < width + thickness; x++)
                {
                    // OYNANABİLİR ALAN ATLANIR ve bu tek satır, halkanın çime hiç
                    // karışmamasının da güvencesi: aynı hücreye ikinci bir görsel
                    // konsaydı zemin deseni kenarlarda değişmiş görünürdü.
                    if (x >= 0 && x < width && y >= 0 && y < height)
                    {
                        continue;
                    }

                    cells.Add(new Vector2Int(x, y));
                }
            }

            return cells;
        }

        /// <summary>
        /// Halkayı sahneden kaldırır.
        /// </summary>
        // ██ BU ÜYE GÖÇTEN SONRA DA GEREKLİ, VE SEBEBİ TEK CÜMLE ██
        // Halka artık bir tilemap; bu bileşen "BorderRing" adlı bir nesne
        // KURMUYOR. Ama sahne dosyasında bir öncekinin bıraktığı bir tane
        // OLABİLİR ve o nesne, karolarıyla birlikte yeni halkanın altında
        // asılı kalırdı. Adıyla toplamaktan başka yol yok.
        //
        // BURADA `borderRoot = null;` SATIRI DURUYORDU ve göçle birlikte ölü
        // kaldı: alan yalnız yazılıyor, hiç okunmuyordu. Yalnız yazılan bir
        // alan bir durum değil, bir kalıntıdır — silindi.
        private void ClearBorderVisuals()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child.name == BorderRootName)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Halka hücresinin görselini seçer.
        /// </summary>
        // PickTerrainSprite ile AYNI belirlenimci düzen; tek fark mutlak değer.
        // Halka hücrelerinin yarısı eksi koordinatlarda yaşıyor ve C# içinde eksi
        // bir sayının kalanı da eksidir — dizinin dışına düşer ve halka ilk
        // hücresinde istisnayla patlardı.
        private Sprite PickBorderSprite(int x, int y)
        {
            int index = Mathf.Abs((x * 7) + (y * 13)) % borderSprites.Length;
            return borderSprites[index];
        }

        /// <summary>
        /// Hücre indeksini dünya konumuna çeviren TEK yer. Üç çağıranı var
        /// (zemin kurulumu, birim doğuşu, hareket) ve üçü de aynı cevabı almak
        /// zorunda; çeviri kopyalansaydı biri değiştiğinde birimler zeminden
        /// kayardı ve hiçbir şey patlamazdı.
        /// </summary>
        private Vector3 CellCentre(int x, int y)
        {
            return unityGrid.GetCellCenterWorld(new Vector3Int(x, y, 0));
        }

        /// <summary>
        /// Savaşa bir birim katar ve ekrandaki karşılığını doğurur.
        /// </summary>
        private void SpawnUnit(string name, Team team, int x, int y)
        {
            // GÖVDE ARTIK PlaceUnit'TE ve bu üye yalnızca kimliği ile savaşçısını
            // kuruyor. Ayrımın sebebi sahiplik: buradaki iki `new` çağrısı
            // Inspector'daki sayılara bağlı ve o sayıların sahibi bu dosya,
            // oysa sürükleme yolu savaşçısını üretim tanımından getiriyor.
            // → BoardAdapter.md#spawnunitstring-name-team-team-int-x-int-y
            PlaceUnit(new Unit(name), NewCombatant(team), x, y);
        }

        /// <summary>
        /// Savaşçıyı tahtaya koyar, savaşa katar ve görselini oluşturur.
        /// </summary>
        /// <returns>Hücre dolu ya da tahta dışıysa false.</returns>
        // DÖNÜŞ bool, PlacementOutcome DEĞİL — ve yapı yerleştirmeyle arasındaki
        // asimetri bilerek: yapının ret sebepleri bir OYUN eylemidir ve
        // adlandırılmıştır, birim doğurmanın ret sebebi ise bu noktada tek bir
        // olgudur (hücre uygun değil), çünkü geri kalan bütün retler bu
        // çağrıdan ÖNCE, üretim kurallarında verilmiştir.
        //
        // "TAHTADA VAR" İLE "SAVAŞTA VAR" İKİ AYRI OLGUDUR ve ayrışmamaları
        // ikisini AYNI çağrıda doğuran tek kapıya bağlı. O kapı artık burası ve
        // iki çağıranı var (demo doğuşu ve sürükleme). Yan kapı — tahtaya yazıp
        // savaşçıyı atlamak — tahtada duran ama Combatant'ı OLMAYAN bir birim
        // üretirdi; bugün açılamaması bir yasak değil bir YOKLUK.
        //
        // HÜCRE SORUSU ÖNCE VE exception DEĞİL: AddUnit dolu hücreyi istisnayla
        // reddeder, oysa buraya oyuncunun parmağı geliyor ve dolu bir hücreye
        // bırakmak bir çağıran hatası değil sıradan bir oyun olgusudur.
        //
        // İKİ İMZA, TEK GÖVDE: bu kısa olan yalnızca demo doğuşunun kapısı ve
        // elinde bir simge YOK — Inspector'daki sayılardan doğan birimin varlık
        // dosyası da yok. Aşırı yükleme, o çağıranı anlamsız bir null yazmaya
        // zorlamamak için duruyor.
        public bool PlaceUnit(Unit identity, Combatant combatant, int x, int y)
        {
            return PlaceUnit(identity, combatant, x, y, bodySprite: null);
        }

        /// <summary>
        /// Savaşçıyı tahtaya koyar ve ekrandaki görselini KENDİ gövdesiyle
        /// doğurur.
        /// </summary>
        /// <param name="bodySprite">
        /// Üretilen birimin kendi görseli; <c>null</c> ise prefab'ın takım
        /// kareleri geçerli kalır.
        /// </param>
        public bool PlaceUnit(Unit identity, Combatant combatant, int x, int y, Sprite bodySprite)
        {
            if (!IsCellFree(x, y))
            {
                return false;
            }

            // Önce KURAL, sonra görsel — tahta dışı koordinat hâlâ istisnayla
            // patlar ve o patlama görsel doğmadan olsun ki ekranda karşılığı
            // olmayan bir birim asla oluşmasın.
            battle.AddUnit(identity, combatant, x, y);

            // Instantiate prefab'dan YENİ bir kopya doğurur; ikinci parametre
            // ebeveyni verir, böylece tahta yok olunca birimler de gider.
            // Argüman UnitView olduğu için dönüş de UnitView'dır — bu yüzden
            // burada tek bir GetComponent yok.
            // GÖRSEL HAVUZDAN GELİYOR, Instantiate'ten değil. Ölen birimler
            // sahneden silinmiyor, saklanıp yeniden kullanılıyor — gerekçesi
            // UnitViewPool'un başında.
            UnitView view = viewPool.Rent(CellCentre(x, y), $"Unit_{identity.Name}_{x}_{y}");

            // TAKIM RENGİ DOĞUŞTA VERİLİR: oyuncunun kendi birimini düşmanınkinden
            // ayırt etmesinin tek yolu bu. Savaşçıdan okunuyor, çağırandan değil —
            // takımın tek sahibi Combatant.
            view.SetTeam(combatant.Team);

            // SIRA: önce takım, sonra kendi gövdesi. Ters sırada takım karesi
            // üretilen birimin resmini ezerdi ve hata sessiz kalırdı — ekranda
            // yalnızca "yine aynı piyade" görünürdü. null geçmek bir SİLME
            // emridir ve tam da bu yüzden geçiliyor: havuzdan gelen görsel bir
            // önceki sahibinin gövdesini taşıyor olabilir.
            view.SetBodySprite(bodySprite);

            // ÇİZİM SIRASI TAHTADAN YAZILIYOR, prefab'dan değil: merdivenin tek
            // sahibi bu dosya ve prefab'daki sayı 1'de kalsaydı asker aynı
            // hücredeki binayla eşitlenir, hangisinin üste çizileceği tanımsız
            // kalırdı. Gövde çizicisi görselin KENDİ nesnesinde yaşıyor; seçim
            // çerçevesi ayrı bir çocukta ve ona dokunulmuyor.
            var body = view.GetComponent<SpriteRenderer>();
            if (body != null)
            {
                body.sortingOrder = UnitSortingOrder;
            }

            unitViews.Add(identity, view);

            // BARIN YÜKSEKLİĞİ ÇİZİLİ GÖRSELDEN ÖLÇÜLÜYOR ve yapı yolundan
            // ayrılmasının sebebi tek: savaşçının ölçüsü prefab'ın kendi
            // ölçeğinde yaşıyor, PlaceUnit'e bir tanım varlığı hiç gelmiyor.
            // İki yol da aynı sayıyı üretiyor — biri hesaplıyor, öteki ölçüyor.
            AttachHealthBar(
                identity,
                view.transform,
                HealthBarSortingOrder,
                MeasureDrawnHeight(view.transform));
            return true;
        }

        /// <summary>
        /// Ekranda duran bir görselin kapladığı yüksekliği ölçer, dünya birimi.
        /// </summary>
        /// <returns>Ölçülecek bir sprite yoksa bir hücre yüksekliği.</returns>
        // ÖLÇÜM GetComponent İLE, InChildren DEĞİL: seçim çerçevesi bir çocukta
        // yaşıyor ve o çocuk gövdeden büyük olduğu için barı havada bırakırdı.
        private float MeasureDrawnHeight(Transform owner)
        {
            var renderer = owner.GetComponent<SpriteRenderer>();
            if (renderer == null || renderer.sprite == null)
            {
                return CellSize.y;
            }

            return renderer.sprite.bounds.size.y * owner.localScale.y;
        }

        /// <summary>
        /// Bir görselin başının üstüne can barı takar.
        /// </summary>
        // ÇOCUK NESNE, BİLEŞEN DEĞİL: bar birimin KENDİ ölçeğinden etkilenmemeli.
        // Savaşçı ile yapı farklı ölçeklerde çiziliyor; bar aynı nesnenin üstünde
        // yaşasaydı her birimde farklı boyda görünürdü. Ayrı bir çocuk, ebeveynin
        // ölçeğini ters çevirerek bunu tek yerde çözüyor.
        /// <param name="drawnWorldHeight">
        /// Sahibinin ekranda kapladığı yükseklik, dünya birimi. Bar bu boyun
        /// yarısının biraz üstüne oturur.
        /// </param>
        // ██ KAYMANIN KÖKÜ: ÖLÇEK DÜZELTİLİYORDU, KONUM DÜZELTİLMİYORDU ██
        // Aşağıdaki 1/parentScale barın BOYUNU her sahipte aynı tutuyor, ama
        // HealthBarView konumu ebeveynin YEREL uzayına yazıyor ve o uzay da
        // ölçekle çarpılıyor. Ölçüldü: 1,6 ölçekli yapıda bar 0,93 dünya
        // biriminde, 1,25 ölçekli savaşçıda 0,725'te duruyordu — yaklaşık üçte
        // bir hücrelik bir fark ve ikisi de görselin tepesiyle ilgisiz.
        private void AttachHealthBar(
            Unit identity, Transform parent, int sortingOrder, float drawnWorldHeight)
        {
            // SESSİZ ÇIKIŞ, ÇÜNKÜ ŞİKÂYET ZATEN AWAKE'TE EDİLDİ — ve bir kez.
            //
            // Burası eskiden LogError basıyordu ve ölçüsü şuydu: EditMode
            // testlerinde tahtanın serileştirilmiş alanları hiç dolmaz, yani bu
            // dal her doğan birimde bir kez tetiklenip 482 testin 6'sını
            // kırıyordu. Kırılan testlerin can barıyla hiçbir ilgisi yoktu;
            // hepsi yerleştirme akışını sınıyordu.
            //
            // KADEME DE DÜŞTÜ: eksik can barı oyunu OYNANAMAZ yapmaz, yalnız
            // okunmaz yapar. Kademe hatanın büyüklüğüne göre seçilir; zeminin
            // yokluğu LogError'dır çünkü tahta hiç çizilmez, barın yokluğu
            // uyarıdır. → hoverFrameSprite ile aynı kademe, aynı gerekçe.
            if (healthBarSprite == null)
            {
                return;
            }

            // HAVUZDAN GELEN GÖRSELDE BAR ZATEN VARDIR. Yeniden kurulsaydı her
            // yeniden kullanımda bir bar daha eklenir ve barlar üst üste birikirdi
            // — havuz kullanan kodların ikinci klasik hatası.
            Vector3 parentScale = parent.localScale;

            // YEREL YÜKSEKLİK, DÜNYA YÜKSEKLİĞİ DEĞİL: bar bir çocuk olduğu için
            // yazdığımız sayı ebeveynin ölçeğiyle çarpılıyor; istediğimiz dünya
            // yüksekliğini almanın tek yolu o çarpanı burada bölmek.
            float wanted = (drawnWorldHeight * 0.5f) + HealthBarMargin;
            float localHeight = parentScale.y > 0.0001f ? wanted / parentScale.y : wanted;

            HealthBarView existing = parent.GetComponentInChildren<HealthBarView>(includeInactive: true);
            if (existing != null)
            {
                existing.SetFraction(1f);
                existing.SetHeightAboveOwner(localHeight);
                healthBars[identity] = existing;
                return;
            }

            var go = new GameObject("HealthBar");
            go.transform.SetParent(parent, worldPositionStays: false);

            go.transform.localScale = new Vector3(
                parentScale.x > 0.0001f ? 1f / parentScale.x : 1f,
                parentScale.y > 0.0001f ? 1f / parentScale.y : 1f,
                1f);

            var bar = go.AddComponent<HealthBarView>();
            bar.Build(healthBarSprite, sortingOrder);
            bar.SetHeightAboveOwner(localHeight);

            healthBars[identity] = bar;
        }

        /// <summary>
        /// İmleç çerçevesini bir kez kurar.
        /// </summary>
        // GÖRSEL EKSİKLİKLER DOĞUŞTA VE TEK SEFER BİLDİRİLİR. Bildirim,
        // eksikliğin ETKİSİNİN görüldüğü yerde değil burada yapılıyor: can barı
        // kurulumunun içinde bağırılsaydı mesaj her doğan birimde tekrarlanır,
        // Console'u doldurur ve EditMode testlerinde beklenmeyen hata logu
        // sayılırdı — nitekim öyle oldu ve 482 testin 6'sını kırdı.
        //
        // KADEME UYARI, HATA DEĞİL: bu iki alanın yokluğu oyunu oynanamaz
        // yapmaz, yalnız okunmaz yapar. Aynı dosyada terrainSprites'ın yokluğu
        // LogError'dır, çünkü o olmadan tahta hiç çizilmez. Kademe, kusurun
        // BÜYÜKLÜĞÜNE göre seçiliyor.
        private void BuildHoverHighlight()
        {
            if (healthBarSprite == null)
            {
                Debug.LogWarning(
                    "[Board] healthBarSprite is not assigned; no health bars will be drawn. " +
                    "Run CountryBall > Sahneyi Kur (her şey), or assign the plain white square " +
                    "sprite on the Board component.",
                    this);
            }

            if (hoverFrameSprite == null)
            {
                Debug.LogWarning(
                    "[Board] hoverFrameSprite is not assigned; the cursor highlight is off.",
                    this);
                return;
            }

            var go = new GameObject("HoverHighlight");
            go.transform.SetParent(transform, worldPositionStays: false);

            hoverHighlight = go.AddComponent<SpriteRenderer>();
            hoverHighlight.sprite = hoverFrameSprite;

            // Çerçeve zeminin ve birimin üstünde görünsün ama can barını
            // kapatmasın; merdivendeki yeri tam bu ikisinin arası.
            hoverHighlight.sortingOrder = HoverSortingOrder;
            hoverHighlight.enabled = false;
        }

        /// <summary>
        /// İmlecin altındaki hücreyi çerçeveler ve oraya tıklamanın NE anlama
        /// geldiğini renkle söyler.
        /// </summary>
        // OYUNCU TIKLAMADAN ÖNCE BİLMELİ: yeşil "buraya yürürüm", kırmızı
        // "buraya vururum", gri "burada bir şey olmaz". Bu geri bildirim
        // olmadan oyuncu her hamleyi deneyerek öğreniyordu ve reddedilen
        // hamlelerin sebebi yalnızca Console'da görünüyordu.
        //
        // KURAL KOPYALANMIYOR, SORULUYOR: "yürüyebilir miyim" sorusunu
        // PathFinder cevaplıyor — burada ikinci bir menzil/yol hesabı YOK.
        // Kopyalansaydı çerçeve yeşil yanıp hamle reddedilebilirdi.
        /// <param name="modeOwnsPointer">
        /// Yürürlükteki kip fareyi sahipleniyorsa true; çerçeve o karede çizilmez.
        /// </param>
        // CEVAP DIŞARIDAN GELİYOR, KİPE BURADA SORULMUYOR ve sebebi sıra:
        // Update kipin kare işini bu çağrıdan ÖNCE yaptırıyor ve kip kendini o
        // sırada kapatmış olabilir. Soruyu burada sorsaydık kipin kapandığı
        // karede çerçeve bir kare erken yanardı.
        private void UpdateHoverHighlight(bool modeOwnsPointer)
        {
            if (hoverHighlight == null)
            {
                return;
            }

            if (modeOwnsPointer || !TryReadPointerCell(out _, out _, out int x, out int y)
                || !battle.IsInsideGrid(x, y))
            {
                hoverHighlight.enabled = false;
                return;
            }

            hoverHighlight.enabled = true;
            hoverHighlight.transform.position = CellCentre(x, y);

            bool occupied = battle.TryGetUnit(x, y, out Unit standing);

            if (selectedUnit == null)
            {
                // Seçim yokken tek anlamlı eylem SEÇMEK; dolu hücre onu vaat eder.
                hoverHighlight.color = occupied
                    ? new Color(1f, 1f, 1f, 0.75f)
                    : new Color(1f, 1f, 1f, 0.25f);
                return;
            }

            if (occupied)
            {
                // Kendi üstü = seçimi bırakma, başkası = saldırı.
                hoverHighlight.color = ReferenceEquals(standing, selectedUnit)
                    ? new Color(1f, 1f, 1f, 0.55f)
                    : new Color(1f, 0.3f, 0.25f, 0.9f);
                return;
            }

            hoverHighlight.color = IsHoverReachable(x, y)
                ? new Color(0.35f, 0.9f, 0.4f, 0.9f)
                : new Color(0.5f, 0.5f, 0.5f, 0.4f);
        }

        /// <summary>
        /// Seçili birim imlecin altındaki hücreye yürüyebilir mi.
        /// </summary>
        // ÖNBELLEK TAHTA TARAFINDA, PathFinder'da DEĞİL: yol arayıcı her
        // çağıranına doğru cevabı vermek zorunda ve orada tutulacak bir hafıza
        // ikinci bir çağıranın sorusunu sessizce yanlış cevaplardı. Burada ise
        // sorunun ne zaman değiştiğini biliyoruz — hücre, seçim ya da seçilinin
        // durduğu yer.
        //
        // DÖRDÜNCÜ BİR ANAHTAR YOK ve eksikliği bilerek: yolu kapatan başka bir
        // birim kımıldadığında renk bir süre eskimiş kalabilir. Bedeli tek bir
        // karenin rengi, kazancı kare başına yedi tahsis.
        private bool IsHoverReachable(int x, int y)
        {
            if (!battle.TryGetPosition(selectedUnit, out int fromX, out int fromY))
            {
                hoverCacheValid = false;
                return false;
            }

            if (hoverCacheValid
                && hoverCacheX == x && hoverCacheY == y
                && hoverCacheFromX == fromX && hoverCacheFromY == fromY
                && ReferenceEquals(hoverCacheUnit, selectedUnit))
            {
                return hoverCacheReachable;
            }

            hoverCacheReachable =
                battle.TryFindPath(selectedUnit, fromX, fromY, x, y, out List<GridStep> _);

            hoverCacheX = x;
            hoverCacheY = y;
            hoverCacheFromX = fromX;
            hoverCacheFromY = fromY;
            hoverCacheUnit = selectedUnit;
            hoverCacheValid = true;

            return hoverCacheReachable;
        }

        /// <summary>
        /// Sıra değiştiyse duyurur.
        /// </summary>
        // SIRANIN SAHİBİ TAHTA DEĞİL, TurnState — burada saklanan tek şey "en son
        // neyi duyurdum" bilgisi, sıranın kendisi değil. İkisi karıştırılsaydı
        // ekran ile kural sessizce ayrışabilirdi.
        private void AnnounceTurnIfChanged()
        {
            Team current = battle.Turn.Current;
            int number = battle.Turn.TurnNumber;

            if (number == lastAnnouncedTurn && current == lastAnnouncedTeam)
            {
                return;
            }

            lastAnnouncedTurn = number;
            lastAnnouncedTeam = current;
            TurnChanged?.Invoke(current, number);
        }

        /// <summary>
        /// Bu tahtanın sıra kipi: sıra devrediliyor mu, yoksa herkes her an
        /// oynayabiliyor mu.
        /// </summary>
        // OKUMA, MEKANİZMA DEĞİL: sayı zaten burada duruyor ve tahtayı kuran
        // satır (`new Battle(width, height, turnMode)`) onu tek otorite olarak
        // kullanıyor. Açılmasının tek sebebi durum şeridi — FreeForAll kipinde
        // tur numarası hiç ilerlemiyor, yani şerit ÖLÜ bir sayı gösteriyordu ve
        // şeridin bunu bilmesinin başka yolu yok. → BattleStatusView.cs
        public TurnMode TurnMode => turnMode;

        /// <summary>
        /// Bir kimliğin hangi tarafa ait olduğunu söyler.
        /// </summary>
        /// <returns>Kimlik bu savaşta tanınmıyorsa false.</returns>
        // İKİ DEFTER, TEK SORU: takım bilgisi savaşçıda da yapıda da var ama
        // ayrı tablolarda duruyor. Çağıranın hangi tabloya bakacağını bilmesi
        // gerekmesin diye soru burada birleştiriliyor — DespawnView'ın iki
        // tabloyu birleştirmesiyle aynı desen ve aynı gerekçe.
        //
        // ARTIK public: durum şeridi seçili şeyin TARAFINI yazıyor ve rengini o
        // taraftan alıyor. İKİNCİ BİR CEVAP ÜRETİLMEDİ, var olan cevabın kapısı
        // açıldı — şerit takımı kendi hesaplasaydı savaşın defteriyle ayrışırdı.
        public bool TryGetTeam(Unit unit, out Team team)
        {
            if (unit != null && battle.TryGetCombatant(unit, out Combatant combatant))
            {
                team = combatant.Team;
                return true;
            }

            if (unit != null && battle.TryGetStructure(unit, out Structure structure))
            {
                team = structure.Team;
                return true;
            }

            team = Team.None;
            return false;
        }

        /// <summary>
        /// Bir kimliği oyuncuya okunur biçimde anlatır: adı, canı, hasarı.
        /// </summary>
        /// <returns>Kimlik bu savaşta tanınmıyorsa false.</returns>
        // METİN BURADA KURULUYOR ÇÜNKÜ KAYNAK BURADA: canı ve hasarı bilen tek
        // yer savaşın defteri. Etiketin kendisi bu tipi tanımıyor, yalnızca
        // hazır cümleyi alıyor.
        public bool TryDescribe(Unit unit, out string description)
        {
            description = string.Empty;

            if (unit == null)
            {
                return false;
            }

            if (battle.TryGetCombatant(unit, out Combatant combatant))
            {
                string state = combatant.State == UnitState.Alive
                    ? "ayakta"
                    : combatant.State == UnitState.Downed ? "düşmüş" : "ölü";

                description =
                    $"{unit.Name}  ·  {combatant.CurrentHealth}/{combatant.MaxHealth} can  ·  " +
                    $"{combatant.AttackProfile.Damage} hasar  ·  {state}";
                return true;
            }

            if (battle.TryGetStructure(unit, out Structure structure))
            {
                description =
                    $"{unit.Name}  ·  {structure.CurrentHealth}/{structure.MaxHealth} can  ·  yapı";
                return true;
            }

            return false;
        }

        /// <summary>
        /// Her can barını sahibinin güncel canına göre tazeler.
        /// </summary>
        // HER KARE SORULUYOR, OLAYA BAĞLANMIYOR: canın değiştiğini duyuran bir
        // olay yok (Health sessiz bir tip) ve bir tane eklemek savaş çekirdeğini
        // ekran için değiştirmek olurdu. Tahtadaki nesne sayısı onlarla ölçüldüğü
        // için okuma ucuz; bar zaten değişmediğinde hiçbir iş yapmıyor.
        private void RefreshHealthBars()
        {
            foreach (KeyValuePair<Unit, HealthBarView> pair in healthBars)
            {
                if (pair.Value == null)
                {
                    continue;
                }

                if (battle.TryGetCombatant(pair.Key, out Combatant combatant))
                {
                    pair.Value.SetFraction(combatant.MaxHealth <= 0
                        ? 0f
                        : (float)combatant.CurrentHealth / combatant.MaxHealth);
                    continue;
                }

                if (battle.TryGetStructure(pair.Key, out Structure structure))
                {
                    pair.Value.SetFraction(structure.MaxHealth <= 0
                        ? 0f
                        : (float)structure.CurrentHealth / structure.MaxHealth);
                }
            }
        }

        /// <summary>
        /// Her yapı görselini sahibinin güncel durumuna göre tazeler.
        ///
        /// OYUNDA NE İŞE YARAR: taretin canı bittiğinde oyuncu kulenin çöktüğünü
        /// görür. Bu olmadan yıkım yalnız Console'da yaşıyordu; ekranda yıkık
        /// bina ayakta duranla BİREBİR aynı görünüyor ve enkaz penceresi boyunca
        /// oyuncu vuruşunun işe yarayıp yaramadığını bilemiyordu.
        /// </summary>
        // HER KARE SORULUYOR, OLAYA BAĞLANMIYOR — ve seçim RefreshHealthBars'ın
        // hemen üstünde ölçülerek yazılmış olanın aynısı: StructureLifecycle
        // bilerek SESSİZ bir tip ve ona ekran için bir olay eklemek, savaş
        // çekirdeğini ekran için değiştirmek olurdu. Tahtadaki yapı sayısı
        // onlarla ölçüldüğü için okuma ucuz; StructureView zaten değişmeyen
        // karelerde hiçbir iş yapmıyor.
        //
        // TAVAN: yapı sayısı oyuncunun yerleştirmesinden geliyor ve tahtanın
        // hücre sayısıyla sınırlı; bu döngü hiçbir şey TAHSİS etmiyor, yalnız
        // sözlüğü geziyor. Ölçülen tavan YOK — ölçüldüğü gün sahibi bu satır.
        //
        // REDDEDILEN - durumu ReactToAttack'in HitAndDestroyed dalından yazmak.
        //     case AttackOutcome.HitAndDestroyed:
        //         structureViews[target].SetState(StructureState.Destroyed);
        // KIRILAN: ApplyStateVisual'ın üstünde yazılı kural — görsel SONUÇ
        // enum'undan değil DURUMdan okunur; AttackOutcome "az önce ne oldu"yu
        // taşır, ekranın istediği ise "şu an ne durumda"dır. Yıkımın ikinci bir
        // yolu doğduğu gün (alan hasarı, süre dolunca çökme) o yol da tazelemeyi
        // hatırlamak zorunda kalır ve unutmak DERLEME hatası vermez.
        // KAZANIRDI: renk yalnız yıkım anında bir kez yazılırdı, her karede
        // sorulmazdı.
        // TEK CUMLE: yıkımın soranı bugün tek ama ekranın sorusu "şu an ne
        // durumda" olduğu için cevabın kaynağı sonuç değil durum olmalı.
        private void RefreshStructureVisuals()
        {
            foreach (KeyValuePair<Unit, StructureView> pair in structureViews)
            {
                if (pair.Value == null)
                {
                    continue;
                }

                // KAYDI OLMAYAN GÖRSEL SESSİZCE ATLANIYOR: süpürme ile bu tazeleme
                // aynı karede çalışıyor ve savaştan çıkmış bir kimliğin görseli bir
                // kare daha tabloda durabilir. LogError basmak, normal bir temizlik
                // sırasını programcı hatası gibi gösterirdi.
                if (battle.TryGetStructure(pair.Key, out Structure structure))
                {
                    pair.Value.SetState(structure.State);
                }
            }
        }

        /// <summary>
        /// Inspector'daki sayılardan bir savaşçı kurar.
        /// </summary>
        private Combatant NewCombatant(Team team)
        {
            // YAŞAM DÖNGÜSÜ PENCERELERİ BİLEREK SERİLEŞTİRİLMEDİ, oysa can ve
            // hasar serileştirildi: "kaç saniye düşük kalır" sorusunun ZATEN bir
            // sahibi var (UnitLifecycle'daki iki sabit) ve sahnedeki bir alan o
            // sabiti sessizce ezerdi. Canın ve hasarın ilk sahibi ise burası.
            // → BoardAdapter.md#newcombatantteam-team
            return new Combatant(
                new Health(maxHealth),
                new UnitLifecycle(),
                new AttackProfile(damage, attackRange, attackCooldownSeconds),
                team);
        }

        /// <summary>
        /// Bir tıklamayı hücreye çevirir ve niyete göre dallandırır.
        /// </summary>
        private void HandleClick()
        {
            // ÇEVİRİ TEK SAHİPTE: ilk üç adım TryReadPointerCell'in içinde ve
            // dünya koordinatları burada KULLANILMIYOR — bu akışın ihtiyacı olan
            // tek şey hücre indeksi. Dallanma da aynı kaldı; değişen tek şey
            // bekleyen vuruşun nerede düştüğü.
            // → BoardAdapter.md#handleclick
            //
            // ██ TIKLAMA ARTIK HİÇBİR EMRİ KENDİLİĞİNDEN DÜŞÜRMÜYOR ██
            // Burada üç `CancelPendingStrike()` çağrısı vardı ve gerekçesi
            // doğruydu: emir TAHTAYA aitken her yeni tıklama onu geçersiz
            // kılardı. Emir birime ait olunca o gerekçe düştü — başka bir
            // savaşçıyı seçmek, boş bir hücreyi ıskalamak ya da tahtanın
            // dışına tıklamak, ÜÇÜNCÜ bir birimin sürmekte olan saldırısını
            // neden kessin? İptal artık niyeti gerçekten değiştiren tek dalda:
            // seçili birime yeni bir emir verildiğinde (yenisi eskisinin yerine
            // geçer) ve onu yürümeye yolladığında.
            // → Docs/deep/konular/09-kararlarin-cevrilmesi.md (madde 2)
            if (!TryReadPointerCell(out _, out _, out int x, out int y))
            {
                return;
            }

            // Debug.Log'un ikinci parametresi "context"tir: Console'da bu satıra
            // tıklayınca Unity Hierarchy'de o nesneyi vurgular. Sınır kuralı
            // Battle'da yaşar; adaptörün işi ona uymak, onu tekrar yazmak değil.
            if (!battle.IsInsideGrid(x, y))
            {
                Debug.Log($"[Board] ({x},{y}) is OUTSIDE the {battle.Width}x{battle.Height} board.", this);
                return;
            }

            if (battle.TryGetUnit(x, y, out Unit clicked))
            {
                // AYNI EMİR İKİNCİ KEZ YAZILMAZ ve tıklama burada TÜKETİLİR.
                // Kalıcı emirle birlikte gerekçe DEĞİŞTİ ve daralttı: eskiden
                // bu dal ikinci bir VURUŞ ödemesini engelliyordu, bugün emir
                // zaten sürüyor — engellediği şey, yürümekte olan savaşçının
                // emrinin sıfırdan yeniden kurulması ve seçimin ikinci kez
                // bırakılması.
                if (RepeatsOrder(clicked))
                {
                    Debug.Log(
                        $"[Board] '{selectedUnit.Name}' already has orders on " +
                        $"'{clicked.Name}'; the order stands.",
                        this);
                    return;
                }

                HandleOccupiedCellClick(clicked, x, y);
                return;
            }

            HandleEmptyCellClick(x, y);
        }

        /// <summary>
        /// Bu tıklama, SEÇİLİ birimin zaten taşıdığı emrin aynısını mı istiyor?
        /// </summary>
        // OYUNDA NE İŞE YARAR: emrini verdiği savaşçıyı yeniden seçip aynı
        // hedefe tıklayan oyuncu, sürmekte olan saldırıyı sıfırdan kurmasın.
        //
        // SORU ARTIK KİPE DEĞİL DEFTERE SORULUYOR ve fark ölçülebilir: kip
        // TEKTİ, yani "yazılı emir" diye tek bir emir tanıyordu. Defterde
        // beş emir olabilir ve soru yalnız SEÇİLİ olanınkini ilgilendirir —
        // başka bir savaşçının aynı hedefe verdiği emir bu tıklamayı yutmamalı.
        // → Orders/UnitOrderBook.cs
        private bool RepeatsOrder(Unit clicked)
        {
            return clicked != null
                   && selectedUnit != null
                   && orders.TryGet(selectedUnit, out IUnitOrder standing)
                   && ReferenceEquals(standing.Target, clicked);
        }

        /// <summary>
        /// Dolu hücreye tıklandı: seçim yoksa seç, seçili olan kendisiyse
        /// seçimi bırak, başkasıysa SALDIR.
        /// </summary>
        // → BoardAdapter.md#handleoccupiedcellclickunit-clicked-int-x-int-y
        // DERİN ANLATIM: Docs/deep/konular/07-tiklamadan-eyleme.md
        private void HandleOccupiedCellClick(Unit clicked, int x, int y)
        {
            if (selectedUnit == null)
            {
                SelectUnit(clicked);

                // EMRİ DE SÖYLENİYOR ve bu İŞ-2'nin ikinci yarısı: emir
                // yazıldığı an seçim bırakılıyor, ama birime tekrar tıklamak
                // onu geri alıyor VE ona ne söylendiğini gösteriyor. Emri
                // olmayan birimde ek boş dizge, yani satır aynen eski hâlinde.
                Debug.Log($"[Board] ({x},{y}) holds '{clicked.Name}' - SELECTED.{DescribeOrder(clicked)}", this);
                return;
            }

            // KENDİ ÜSTÜNE TIKLAMAK SEÇİMİ BIRAKIR — ve bu dal NİYETİ taşıyor,
            // GEÇERLİLİĞİ değil. Silinseydi MoveAction çağrıyı KABUL ederdi
            // (doluluk kontrolü birimin KENDİSİNİ bilerek dışarıda bırakıyor) ve
            // seçimi bırakmak isteyen oyuncu sessizce boş bir harekete düşerdi.
            // Karşılaştırma ReferenceEquals ile: Unit bir sınıftır ve aradığımız
            // zaten TAM O NESNEnin kendisidir.
            // → BoardAdapter.md#handleoccupiedcellclickunit-clicked-int-x-int-y
            if (ReferenceEquals(clicked, selectedUnit))
            {
                ClearSelection();
                Debug.Log($"[Board] ({x},{y}) holds the selected unit - DESELECTED.", this);
                return;
            }

            // ██ DÜŞMÜŞ BİR DOSTA TIKLAMAK ARTIK DİRİLTMEK DEMEK ██
            // Tuş ZORUNLU olmaktan çıktı ve gerekçe ölçüldü: düşmüş bir dosta
            // tıklamanın başka anlamlı bir karşılığı yok — kendi takımına
            // saldıramazsın, dolu bir hücreye yürüyemezsin, ve onu seçmek
            // oyuncuyu seç/bırak döngüsüne kilitliyordu. Tuş takma ad olarak
            // duruyor: kas hafızası bozulmasın diye basılı tutmak da aynı yere
            // gidiyor.
            //
            // SORU SALDIRIDAN ÖNCE: altına konsaydı önce bir saldırı denemesi
            // geçerdi, TargetingRules onu reddederdi ve Console'a diriltmeyle
            // hiç ilgisi olmayan bir satır yazılırdı.
            if (Input.GetKey(reviveModifierKey) || IsFallenAlly(clicked))
            {
                TryReviveTarget(clicked, x, y);
                return;
            }

            // KENDİ TARAFINA TIKLAMAK SEÇİMİ DEĞİŞTİRİR, SALDIRI DEĞİLDİR.
            //
            // Bu dal olmadan oyuncu ikinci savaşçısını seçemiyordu: tıklama
            // doğrudan saldırı akışına düşüyor, kural onu reddediyor ve ekranda
            // hiçbir şey olmuyordu. Oyuncunun gördüğü şey "tıklıyorum, bir şey
            // olmuyor" idi — sessiz bir kilitlenme.
            //
            // AYRIM TAKIMDAN OKUNUYOR, TİPTEN DEĞİL: hedef bir yapı da olabilir
            // ve kendi binana tıklamak da onu SEÇMELİ (üretim paneli ancak
            // seçiliyken açılıyor). "Aynı takım mı" sorusunun tek kaynağı savaşın
            // defteri; burada ikinci bir kopya tutulmuyor.
            if (TryGetTeam(clicked, out Team clickedTeam)
                && TryGetTeam(selectedUnit, out Team selectedTeam)
                && clickedTeam == selectedTeam)
            {
                SelectUnit(clicked);
                Debug.Log(
                    $"[Board] ({x},{y}) holds friendly '{clicked.Name}' - SELECTION MOVED.{DescribeOrder(clicked)}",
                    this);
                return;
            }

            // YAPI YÜRÜMEZ, DOĞRUDAN ATEŞ EDER — ve bu dal ölçülmüş bir
            // çökmenin onarımı: seçili bir bina düşmana tıklandığında akış
            // TryCloseInOn üzerinden BattleActions.Move'a giriyor ve savaşçı
            // defterinde bulunmayan kimlik yüzünden "The unit is not in this
            // battle" istisnası atılıyordu. Yaklaşma adımı burada tamamen
            // atlanıyor: bina olduğu yerden vurabiliyorsa vurur, vuramıyorsa
            // menzil reddini oyuncuya söyler.
            //
            // YAPIYA KALICI EMİR YAZILMIYOR ve bu bir eksiklik değil: ateş eden
            // yapı zaten AdvanceStructureFire ile kendiliğinden ve kendi bekleme
            // süresiyle en yakın düşmana vuruyor. İkinci bir kalıcılık sahibi
            // eklemek, aynı kulenin iki ayrı tempoyla ateş etmesi demek olurdu.
            // Oyuncunun tıklaması burada bir emir değil, TEK bir atış isteği.
            if (IsStructureIdentity(selectedUnit))
            {
                // ██ SİLAHSIZ BİNA SEÇİLİYKEN TIKLAMA BİR RET DEĞİL, BİR ODAK ██
                // Operatör: "bir yapıyı seçerken rakip yapıyı seçtiğimde
                // saldıramıyor diyor... daha çok seçili olayını karşıdaki yapıya
                // veya karşı takımın savaşçısına geçilse."
                //
                // ESKİ HÂLDE BURASI KOŞULSUZ SALDIRIYORDU ve kışla gibi silahsız
                // bir bina seçiliyken her tıklama `RejectedAttackerCannotAttack`
                // üretiyordu: oyuncu hiç istemediği bir eylemin reddini okuyordu.
                // Oysa kışlayla düşmana tıklamanın tek anlamlı karşılığı
                // "şimdi ONA bakıyorum" — çünkü kışla saldıramaz, yürüyemez ve
                // düşmana yapabileceği hiçbir şey yok.
                //
                // SALDIRABİLEN YAPI BU DALDAN GEÇMİYOR ve bu operatörün kendi
                // kelepçesi: "bu tabii ki saldırı yapan yapılar için geçerli
                // değil." Taret seçiliyken tıklama yine TEK bir atış isteği.
                //
                // SORU `Structure.CanAttack`'E SORULUYOR, bir tür listesine
                // değil: silahın sahibi tanımdaki AttackProfile ve o alan
                // doluysa bina saldırabilir. İsim listesi tutulsaydı yeni bir
                // silahlı bina eklendiği gün sessizce odak devrederdi.
                if (!CanStructureAttack(selectedUnit))
                {
                    TransferFocusTo(clicked, x, y);
                    return;
                }

                AttackOutcome structureOutcome = BattleActions.Attack(battle, selectedUnit, clicked);
                ReactToAttack(selectedUnit, structureOutcome, clicked, x, y);
                return;
            }

            // UZAKTAKİ DÜŞMANA TIKLAMAK "ORAYA GİT VE VUR" DEMEKTİR. Oyuncu
            // menzili gözüyle ölçmek zorunda kalmasın diye: tıkladığı hedef
            // uzaktaysa savaşçı önce yanına yürür, sonra vurur. İkisi TEK bir
            // hamledir ve tek bir hak harcar.
            if (!TryCloseInOn(clicked, x, y))
            {
                return;
            }

            // ██ TIKLAMA ARTIK BİR VURUŞ DEĞİL, BİR EMİR YAZIYOR ██
            // Burada `BattleActions.Attack` doğrudan çağrılıyordu ve operatörün
            // bildirdiği eksik tam olarak o satırdı: "bir attacker'a target
            // belirttiğimizde 1 kere saldırıyor". Vuruşu emir yapıyor, ve emir
            // oyuncu yeniden yönlendirene ya da hedef menzilden çıkana kadar
            // duruyor. MENZİLDEYKEN DE EMİR YAZILIYOR, yalnız uzaktayken değil:
            // yakındaki hedefe tıklamak da "buna vurmaya devam et" demektir.
            //
            // Mesafe BURADA hesaplanmıyor ve hesaplatılmıyor bile: emir her
            // karede AttackAction'a soruyor, o da konumları Battle'dan bulup
            // GridDistance'a ölçtürüyor. Bu satırın bildiği tek şey "kim kime".
            IssueOrder(selectedUnit, new AttackOrder(this, selectedUnit, clicked));
        }

        /// <summary>
        /// Seçili savaşçı hedefe vuramayacak kadar uzaktaysa, ona komşu bir
        /// hücreye yürütür.
        /// </summary>
        /// <returns>
        /// Saldırı EMRİ yazılabilir ise true — hedef ya zaten menzilde, ya da
        /// yürüyüş yola çıktı. Yaklaşma başarısızsa false; o zaman bu tıklama
        /// hiçbir emir doğurmaz ve savaşçının varsa eski emri OLDUĞU GİBİ kalır.
        /// </returns>
        // CEVABIN ANLAMI DEĞİŞTİ: eskiden "ŞİMDİ vurulabilir mi" demekti ve
        // yürüyüş başladığında false dönüyordu, çünkü vuruş bir kipin işiydi.
        // Bugün emir yürüyüşü kendisi bekliyor, dolayısıyla iki hâl de aynı
        // cevabı hak ediyor ve ayrım tek yerde kaldı: emrin YAZILIP
        // yazılmayacağı.
        //
        // NEDEN AYRI BİR ADIM: hareket ile saldırı iki ayrı eylem ve ikisinin de
        // kendi kuralları var (sıra, durum, menzil). Bunları tek bir "saldır"
        // çağrısının içine gömmek, hareket reddedildiğinde saldırının neden
        // olmadığını söyleyemez hâle getirirdi.
        //
        // SIRA KURALI YALNIZ SIRA TABANLI KİPTE HARCANIR: Alternating kipinde
        // BattleActions.Move başarılı olduğunda sırayı devrediyor, dolayısıyla
        // peşinden gelen saldırı reddedilirdi ve vuruş ikinci tıklamaya kalırdı.
        // FreeForAll kipinde EndTurn sırayı ilerletmediği için o gerekçe düşüyor
        // ve vuruş yürüyüş biter bitmez kendiliğinden oluyor. Ayrımı burada bir
        // bayrak değil battle.Turn.AllowsAction söylüyor.
        private bool TryCloseInOn(Unit target, int targetX, int targetY)
        {
            if (!battle.TryGetPosition(selectedUnit, out int fromX, out int fromY))
            {
                return true;
            }

            int range = AttackRangeOf(selectedUnit);

            // Zaten menzildeyse yürümeye gerek yok; doğrudan saldırıya geç.
            if (GridDistance.Between(fromX, fromY, targetX, targetY) <= range)
            {
                return true;
            }

            if (!TryFindApproachCell(fromX, fromY, targetX, targetY, range, out int stepX, out int stepY))
            {
                Debug.Log(
                    $"[Board] '{selectedUnit.Name}' cannot get close enough to '{target.Name}'; " +
                    "every cell around the target is blocked.",
                    this);
                return false;
            }

            if (!TryStartWalk(selectedUnit, stepX, stepY))
            {
                return false;
            }

            // SIRASI GEÇMİŞSE ZİNCİR KURULMAZ ve eski cümle aynen kalıyor: sıra
            // tabanlı kipte hareket hakkı bitirdi, vuruş gerçekten ikinci
            // tıklamayı bekliyor. Buraya koşulsuz bir bekleyen vuruş yazmak,
            // sırası olmayan bir birimin her karede reddedilen bir saldırı
            // denemesi yapmasına ve Console'un ret satırıyla dolmasına yol açardı.
            if (!TryGetTeam(selectedUnit, out Team team) || !battle.Turn.AllowsAction(team))
            {
                Debug.Log(
                    $"[Board] '{selectedUnit.Name}' closed in on '{target.Name}' and is now in range. " +
                    "Click the target again to strike.",
                    this);
                return false;
            }

            // EMİR ARTIK BURADA YAZILMIYOR, ÇAĞIRAN YAZIYOR — ve tek sebebi
            // sahiplik: yürüyerek varılan hedefle zaten menzilde olan hedef
            // AYNI emri hak ediyor. İki yerde ayrı ayrı yazılsaydı, emrin şekli
            // değiştiği gün ikisinden biri sessizce eskirdi. Bu üye yalnız
            // "yaklaşma başladı mı" sorusunu cevaplıyor.
            Debug.Log(
                $"[Board] '{selectedUnit.Name}' is closing in on '{target.Name}' and will strike on arrival.",
                this);

            return true;
        }

        /// <summary>
        /// Birimi bir hücreye yürütür: önce savaşa sorar, kabul edilirse görseli
        /// yola çıkarır.
        /// </summary>
        /// <returns>Hareket kabul edildi ve görsel yola çıktıysa true.</returns>
        // ██ YÜRÜYEN BİRİME İKİNCİ EMİR VERİLMEZ ██
        // Ölçüm şuydu: MoveAction birimi tahtada ANINDA taşıyor, görsel ise
        // gecikmeli takip ediyor. Yürüyüşün ortasında verilen ikinci emir eski
        // durakları siliyor ve asker, görselin O ANKİ noktasından yeni yolun ilk
        // durağına DÜZ gidiyordu — tahtanın ortasından, başka birimlerin
        // üstünden kesip geçerek.
        //
        // RET, YOLU BİRLEŞTİRMEK DEĞİL — ve seçim ölçülebilir: birleştirmek
        // ekranın tahtayı yakalamasını daha da geciktirir, oysa ret ikisini
        // ayrık tutuyor. RET MOVE'DAN ÖNCE: sonrasına konsaydı tahta çoktan
        // taşınmış olur ve görselin reddi ikisini kalıcı olarak ayırırdı.
        private bool TryStartWalk(Unit unit, int toX, int toY)
        {
            if (RefuseWhileWalking(unit))
            {
                return false;
            }

            MoveOutcome outcome = BattleActions.Move(battle, unit, toX, toY, out List<GridStep> path);

            if (outcome != MoveOutcome.Moved)
            {
                ReactToMove(outcome, unit, toX, toY, path);
                return false;
            }

            WalkViewAlong(unit, path, toX, toY);
            return true;
        }

        /// <summary>
        /// Görseli hâlâ yolda olan bir birime yeni hareket emri verilemez;
        /// verilmişse oyuncuya sakin bir satırla söylenir.
        /// </summary>
        /// <returns>Emir reddedildiyse true.</returns>
        private bool RefuseWhileWalking(Unit unit)
        {
            if (!IsViewWalking(unit))
            {
                return false;
            }

            Debug.Log($"[Board] '{unit.Name}' is still walking; wait until it arrives.", this);
            return true;
        }

        /// <summary>
        /// Saldıranın KENDİ menzili; savaşın defterinde bulunamazsa
        /// Inspector'daki varsayılan.
        /// </summary>
        // MENZİLİN SAHİBİ ARTIK SALDIRANIN KENDİSİ, Inspector'daki tek sayı
        // DEĞİL. Ölçüsü şu: menzili 3 olan bir okçu, Inspector'daki 1 ile
        // hesaplandığında hedefin dibine kadar yürüyordu — ekranda görünen şey
        // "okçu göğüs göğüse dövüşüyor" oluyordu, oysa üç hücre uzaktan
        // vurabiliyordu. Aynı soru yapılar için de tek kaynaktan cevaplanıyor.
        //
        // INSPECTOR DEĞERİ KALIYOR ama artık bir VARSAYILAN: kimliği savaşın
        // defterinde bulunmayan bir seçim (test kurulumu, yarım kalmış temizlik)
        // yine de bir sayı almalı — sıfır dönseydi her hedef menzil dışı olurdu.
        private int AttackRangeOf(Unit unit)
        {
            if (unit == null)
            {
                return attackRange;
            }

            if (battle.TryGetCombatant(unit, out Combatant combatant)
                && combatant.AttackProfile != null)
            {
                return combatant.AttackProfile.Range;
            }

            if (battle.TryGetStructure(unit, out Structure structure)
                && structure.AttackProfile != null)
            {
                return structure.AttackProfile.Range;
            }

            return attackRange;
        }

        /// <summary>
        /// Bu kimlik savaşın defterinde bir YAPI mı?
        /// </summary>
        // TEK SORU, İKİ ÇAĞIRAN — ve tek yerde durması ölçülmüş bir arızanın
        // dersi: aynı "The unit is not in this battle" istisnası İKİ ayrı yığında
        // görüldü (boş hücreye tıklama ve düşmana tıklama). Soru iki yerde ayrı
        // ayrı sorulsaydı biri onarıldığında öteki üretimde patlamaya devam
        // ederdi.
        private bool IsStructureIdentity(Unit unit)
        {
            return unit != null && battle.TryGetStructure(unit, out Structure _);
        }

        /// <summary>
        /// Hedefe vurabilecek, boş ve ULAŞILABİLİR hücrelerin en yakınını bulur.
        /// </summary>
        // ADAYLAR MENZİLE GÖRE ÜRETİLİYOR, KOMŞULUĞA GÖRE DEĞİL: menzili 2 olan
        // bir birim hedefin dibine kadar gitmemeli, vurabildiği yerde durmalı.
        // Aday listesi hedefin çevresindeki kareyi tarar ve her adayı ÜÇ
        // süzgeçten geçirir: menzilde mi, boş mu, yolu var mı.
        //
        // MENZİL ARTIK PARAMETRE, ALAN DEĞİL: sayının sahibi saldıranın kendi
        // saldırı profili ve o soruyu AttackRangeOf bir kez cevaplıyor. Burada
        // ikinci kez okunsaydı çağıranın hesapladığı menzil ile burada
        // kullanılan menzil sessizce ayrışabilirdi.
        private bool TryFindApproachCell(
            int fromX, int fromY, int targetX, int targetY, int range, out int bestX, out int bestY)
        {
            bestX = 0;
            bestY = 0;

            int bestSteps = int.MaxValue;

            for (int dy = -range; dy <= range; dy++)
            {
                for (int dx = -range; dx <= range; dx++)
                {
                    int candidateX = targetX + dx;
                    int candidateY = targetY + dy;

                    if (dx == 0 && dy == 0)
                    {
                        continue;
                    }

                    if (GridDistance.Between(candidateX, candidateY, targetX, targetY) > range)
                    {
                        continue;
                    }

                    if (!IsCellFree(candidateX, candidateY))
                    {
                        continue;
                    }

                    if (!battle.TryFindPath(
                            selectedUnit, fromX, fromY,
                            candidateX, candidateY, out List<GridStep> candidatePath))
                    {
                        continue;
                    }

                    // EN AZ ADIM KAZANIR, en kısa kuş uçuşu değil: engellerin
                    // etrafından dolaşan bir hücre, düz çizgide yakın görünüp
                    // gerçekte uzak olabilir.
                    if (candidatePath.Count < bestSteps)
                    {
                        bestSteps = candidatePath.Count;
                        bestX = candidateX;
                        bestY = candidateY;
                    }
                }
            }

            return bestSteps != int.MaxValue;
        }

        /// <summary>
        /// Diriltme denemesini savaşa SORAR ve cevabına göre Console'u günceller.
        ///
        /// Bu metot <see cref="BattleActions.Revive"/>'ın üretimdeki TEK
        /// çağıranıdır. O eylem yazılmış, on iki testle korunmuş ve belgelenmişti
        /// ama hiçbir girdiye bağlı değildi; <see cref="ReviveOutcome"/> bu dosyada
        /// HİÇ geçmiyordu, yani ekrana ulaşan bir cevabı da yoktu. Bağlanması,
        /// Revive'ın kendi belgesinde <c>Combatant.TryRevive</c> için yazdığı
        /// cümlenin bir üst katmanda birebir tekrarıdır.
        /// </summary>
        private void TryReviveTarget(Unit target, int x, int y)
        {
            // YAPI DİRİLTİLMEZ, ve soru BURADA soruluyor — BattleActions'ta
            // değil: orada bir yapıyı hedef göstermek bir oyun sonucu değil bir
            // ÇAĞIRAN HATASIdır ve RequireCombatant istisna atar. Fareyle
            // tıklanan hücre ise pekâlâ bir baraka olabilir; oyuncunun eli bir
            // çağıran hatası üretemez.
            if (!battle.TryGetCombatant(target, out Combatant _))
            {
                Debug.Log($"[Board] ({x},{y}) holds a structure; structures are not revived.", this);
                return;
            }

            // BİNA KİMSEYİ KALDIRAMAZ ve cümle burada söyleniyor: motor bu
            // denemeye artık istisna değil RejectedActorCannotAct dönüyor, ama
            // o cevabın oyuncuya ulaşan hâli "şu an eylem yapamaz" olurdu —
            // oysa bina bekleyerek de kaldıramayacak.
            if (IsStructureIdentity(selectedUnit))
            {
                Debug.Log(
                    $"[Board] '{selectedUnit.Name}' is a structure; structures do not revive anyone.",
                    this);
                return;
            }

            // UZAKTAKİ DÜŞMÜŞ DOSTA TIKLAMAK "YANINA GİT VE KALDIR" DEMEK.
            // Saldırıdaki yaklaş-sonra-vur zincirinin ikizi ve AYNI makineyi
            // kullanıyor; ikinci bir bekleyen-emir makinesi açılmadı, emrin
            // cinsini tek bir bayrak taşıyor.
            if (!TryCloseInOnAlly(target, x, y))
            {
                return;
            }

            ReviveOutcome outcome = BattleActions.Revive(battle, selectedUnit, target);
            ReactToRevive(selectedUnit, outcome, target, x, y);
        }

        /// <summary>
        /// Seçili savaşçı düşmüş dostuna ulaşamayacak kadar uzaktaysa yanına
        /// yürütür ve diriltmeyi varışa erteler.
        /// </summary>
        /// <returns>
        /// Diriltme ŞİMDİ denenebilir ise true. Yürüyüş başladıysa ya da
        /// yaklaşma başarısızsa false.
        /// </returns>
        // TryCloseInOn'UN KOPYASI DEĞİL, KARDEŞİ — ve ayrı durmasının sebebi
        // ölçülebilir: o üye başarısız yaklaşmada saldırı cümlesini yazıyor ve
        // sırası geçmiş savaşçıya "tekrar tıkla, vur" diyor. İkisi tek üyede
        // birleştirilseydi her satırda "bu bir vuruş mu, kaldırma mı" diye
        // sorulurdu; menzil, aday hücre ve yürüyüş hesabı ise ZATEN paylaşılıyor.
        private bool TryCloseInOnAlly(Unit target, int targetX, int targetY)
        {
            if (!battle.TryGetPosition(selectedUnit, out int fromX, out int fromY))
            {
                return true;
            }

            int range = AttackRangeOf(selectedUnit);

            if (GridDistance.Between(fromX, fromY, targetX, targetY) <= range)
            {
                return true;
            }

            if (!TryFindApproachCell(fromX, fromY, targetX, targetY, range, out int stepX, out int stepY))
            {
                Debug.Log(
                    $"[Board] '{selectedUnit.Name}' cannot get close enough to '{target.Name}'; " +
                    "every cell around the fallen ally is blocked.",
                    this);
                return false;
            }

            if (!TryStartWalk(selectedUnit, stepX, stepY))
            {
                return false;
            }

            if (!TryGetTeam(selectedUnit, out Team team) || !battle.Turn.AllowsAction(team))
            {
                Debug.Log(
                    $"[Board] '{selectedUnit.Name}' reached '{target.Name}'. " +
                    "Click the fallen ally again to revive.",
                    this);
                return false;
            }

            // EMİR BURADA YAZILIYOR, SALDIRININ TERSİNE — ve fark bir kararın
            // kendisi: kaldırmanın KALICI hâli yok. Menzildeki bir dostu
            // kaldırmak tek çağrıda başlayıp biten bir eylem, dolayısıyla
            // yalnız YÜRÜYEREK varılan kaldırma bir emir gerektiriyor. Saldırı
            // ikisinde de emir yazıyor çünkü menzildeki hedefe vurmak da
            // "vurmaya DEVAM et" demek.
            //
            // ██ CÜMLE ÖNCE, EMİR SONRA — VE SIRA ZORUNLU ██
            // <c>IssueOrder</c> seçimi bırakıyor, yani <c>selectedUnit</c> ondan
            // sonra null. Ters sırada yazılıyken bu Console satırı her koşuşunda
            // NullReferenceException veriyordu ve yürüyerek varılan kaldırma yolu
            // tamamen kapalıydı. Saldırı dalında aynı tuzak yok, çünkü orada emir
            // üyenin SON satırı — yani kırılan şey kural değil SIRAYDI.
            // → Test: TryCloseInOnAlly_WhenTheWalkStarts_
            //   WritesTheOrderWithoutReadingTheReleasedSelection
            Debug.Log(
                $"[Board] '{selectedUnit.Name}' is walking to '{target.Name}' and will revive on arrival.",
                this);
            IssueOrder(selectedUnit, new ReviveOrder(this, selectedUnit, target));

            return false;
        }

        /// <summary>
        /// Boş hücreye tıklandı: seçim varsa HAREKET, yoksa yalnızca bildir.
        /// </summary>
        private void HandleEmptyCellClick(int x, int y)
        {
            if (selectedUnit == null)
            {
                Debug.Log($"[Board] ({x},{y}) is inside the board and EMPTY.", this);
                return;
            }

            // YAPI YÜRÜMEZ ve soru BattleActions.Move'dan ÖNCE soruluyor. Aynı
            // eksikliğin ikinci yüzü: seçili bir bina varken boş hücreye tıklamak
            // da hareket yoluna giriyor ve "The unit is not in this battle"
            // istisnasıyla oyunu kesiyordu. Sakin bir satır yeter — oyuncu bir
            // kural ihlali yapmadı, yalnızca yürüyemeyecek bir şeyi seçmişti.
            if (IsStructureIdentity(selectedUnit))
            {
                Debug.Log($"[Board] '{selectedUnit.Name}' is a structure; structures do not walk.", this);
                return;
            }

            // YÜRÜYEN BİRİM YENİ EMİR ALMAZ, ve soru Move'dan ÖNCE: gerekçesi
            // TryStartWalk'ta bir kez yazılı ve burada tekrar edilmiyor. Bu
            // akış TryStartWalk'ı kullanmıyor çünkü kabul edilen hareketin
            // cümlesini de yazması gerekiyor.
            if (RefuseWhileWalking(selectedUnit))
            {
                return;
            }

            // ██ YÜRÜMEK, SALDIRI EMRİNİ İPTAL EDEN TEK TIKLAMADIR ██
            // Ve iptalin yeri REDDİN ALTI: yürüyemeyen bir birime "boş hücreye
            // tıkladın" diye emrini kaybettirmek, oyuncunun hiç istemediği bir
            // ceza olurdu. Burada iptal edilen şey de yalnız BU birimin emri;
            // tahtadaki öteki savaşçılar hedeflerine vurmaya devam eder.
            CancelOrder(selectedUnit);

            // Yol da buradan çıkıyor: tahta hareketi ANINDA işler, ekran ise
            // birimi bu duraklardan geçirerek gecikmeli takip eder.
            MoveOutcome outcome = BattleActions.Move(battle, selectedUnit, x, y, out List<GridStep> path);
            ReactToMove(outcome, selectedUnit, x, y, path);
        }

        /// <summary>
        /// Saldırı sonucuna göre ekranı ve Console'u günceller.
        /// </summary>
        // SONUÇ BİR EVENT'LE GELMİYOR ve gelmemeli: soran zaten burada, araya
        // bir dinleyici koymak yalnızca dolaylılık olurdu.
        // → BoardAdapter.md#reacttoattackattackoutcome-outcome-unit-target-int-x-int-y
        // DERİN ANLATIM: Docs/deep/konular/06-sonuc-enumlari.md
        // SALDIRAN ARTIK PARAMETRE, selectedUnit DEĞİL — ve bu bir zorunluluk:
        // ateş eden bir kulenin seçili olması gerekmiyor, saldıranı seçimden
        // okumak o vuruşu yanlış birime yazardı (yanlış hamle, yanlış Console
        // satırı, mermi yanlış hücreden kalkar).
        //
        // KARŞILIK VERME BU ÜYEDEN DOĞUYOR ve zincirin ONUNCU durağı burasıydı:
        // vurulan taraf bu satırdan sonra hiçbir şey yapmıyordu.
        // DERİN ANLATIM: Docs/deep/konular/11-karsilik-verme-ve-menzil.md
        private void ReactToAttack(Unit attacker, AttackOutcome outcome, Unit target, int x, int y)
        {
            // ██ DURUM YAZIMI EKRANDAN ÖNCE ██
            // Karşılık emri bir DURUM yazıyor (defter), aşağısı ise bir DUYURU
            // (hamle, mermi, Console). Sıra ters olsaydı, aşağıdaki satırların
            // birinden tetiklenen bir okuyucu defteri henüz karşılıksız görürdü.
            if (outcome == AttackOutcome.Hit)
            {
                WriteRetaliation(target, attacker);
            }

            // VURUŞ EKRANDA GÖRÜNÜR — ve yalnızca GERÇEKTEN vurulduğunda.
            // Reddedilen bir saldırıda da hamle oynatılsaydı oyuncu isabet ile
            // reti ayırt edemezdi; gösterim o zaman bilgi taşımaz, gürültü olurdu.
            bool landed = outcome == AttackOutcome.Hit
                          || outcome == AttackOutcome.HitAndDowned
                          || outcome == AttackOutcome.HitAndFinished
                          || outcome == AttackOutcome.HitAndDestroyed;

            if (landed)
            {
                PlayAttackVisual(attacker, x, y);
                PlayRangedVisual(attacker, x, y);
            }

            switch (outcome)
            {
                // GÖRSEL BU DALDAN TAZELENMİYOR ARTIK: durum değişikliğinin TEK
                // tetiği Battle.UnitStateChanged oldu ve saldırı da bir durum
                // değişikliği ürettiği için olay zaten yolda. Elle tazeleme
                // burada kalsaydı ekranın aynı olguya İKİ kaynağı olurdu; ikisi
                // aynı cevabı verdiği için hata SESSİZ kalır ve olay yolu bir
                // gün sustuğunda hatanın YARISI örtülürdü.
                // → BoardAdapter.md#reacttoattackattackoutcome-outcome-unit-target-int-x-int-y
                case AttackOutcome.HitAndDowned:
                    Debug.Log($"[Board] '{target.Name}' at ({x},{y}) was hit and went DOWN.", this);
                    break;

                // DÜŞMÜŞ HEDEF BİTİRİLDİ: kurtarma penceresi dolmadan vurulan
                // bir beden artık kalıcı olarak ölüyor. BU BİR İSABETTİR ve
                // yukarıdaki beyaz listede öyle sayılıyor — hamle oynuyor,
                // mermi uçuyor ve isabet sonrası seçimi bırakma kuralı buna da
                // uygulanıyor. Sakin bir cümle yeter: oyuncu bir kural ihlali
                // yapmadı, tersine emrini tamamladı.
                case AttackOutcome.HitAndFinished:
                    Debug.Log($"[Board] '{target.Name}' at ({x},{y}) was FINISHED OFF.", this);
                    break;

                // İKİZİN ÖTEKİ YARISI, ve dalın var olması bir tercih değil bir
                // ONARIM: bu değer enum'da baştan beri duruyordu ama burada
                // KARŞILIĞI YOKTU, yani yıkılan her yapı aşağıdaki default'a
                // düşüp "Unhandled attack outcome" diye bir PROGRAMCI hatası
                // basıyordu. Bugüne kadar görünmemesinin sebebi tahtaya hiç yapı
                // konamamasıydı; o yol onarıldı ve dalın yokluğu erişilebilir
                // oldu. Sigortanın bir oyun sonucuna harcanması, sigortayı yok
                // etmekle aynı şeydir.
                //
                // BU SATIR ARTIK TEK DUYURU DEĞİL, VE ESKİ ÖLÇÜ TAM BURADA
                // YANLIŞTI: burada "yapı görseli bir UnitView taşımıyor, yani
                // söylenecek bir durum yok" yazıyordu ve o cümle bir eksikliği
                // karar gibi gösteriyordu. Yıkım ekranda hiç görünmüyordu.
                //
                // GÖRSELE HÂLÂ BURADAN DOKUNULMUYOR, ama sebep değişti: yapının
                // durumunu ekrana taşıyan yer RefreshStructureVisuals ve o,
                // cevabı sonuç enum'undan değil Structure.State'ten okuyor.
                // Yapıların Battle.UnitStateChanged'e KATILMAMASI ise duruyor —
                // StructureLifecycle bilerek olaysız, çünkü tek geçişini yapan
                // çağrı cevabı zaten dönüş değeriyle alıyor.
                //
                // Enkazın sahneden kalkması AdvanceBattleTime'ın süpürmesinin
                // işi ve enkaz süresi dolmadan kalkmamalı.
                //
                // DescribeCondition ÇAĞRILMIYOR ve bu ölçülmüş bir kaçınma: o üye
                // cevabını savaşçı defterinden okuyor, yapı kimliği orada hiç
                // bulunmuyor ve satır, savaşta duran bir parça için "(not in this
                // battle)" yazardı.
                case AttackOutcome.HitAndDestroyed:
                    Debug.Log($"[Board] structure '{target.Name}' at ({x},{y}) was hit and DESTROYED.", this);
                    break;

                case AttackOutcome.Hit:
                    Debug.Log($"[Board] '{target.Name}' at ({x},{y}) was hit. {DescribeCondition(target)}", this);
                    break;

                case AttackOutcome.RejectedOutOfRange:
                    Debug.Log($"[Board] '{target.Name}' at ({x},{y}) is OUT OF RANGE.", this);
                    break;

                case AttackOutcome.RejectedInvalidTarget:
                    Debug.Log($"[Board] '{target.Name}' at ({x},{y}) is not a valid target. {DescribeCondition(target)}", this);
                    break;

                // MESAJ HEDEFİ DEĞİL SALDIRANI ANLATIYOR — değerin adındaki
                // "Actor" sözcüğünün doğrudan karşılığı. TEK MESAJ, ÜÇ SEBEP ve
                // bu bir tavizdir: "sırası değil" ile "birim düşmüş" bugün
                // ayrılmıyor; arayüz farkı SÖYLEMEK zorunda kaldığı gün ayrılır.
                case AttackOutcome.RejectedActorCannotAct:
                    Debug.Log($"[Board] '{attacker.Name}' cannot act right now; the attack was rejected. {DescribeCondition(attacker)}", this);
                    break;

                // BEKLEME SÜRESİ BİR OYUN OLGUSUDUR, programcı hatası DEĞİL — bu
                // yüzden Log, LogError değil; aynı ayrım bu switch'in default
                // dalında ters yönde duruyor. Dalın kendisi bir ONARIM: değer
                // adıyla karşılanmasaydı arka arkaya tıklayan her oyuncu
                // Console'da kırmızı bir "Unhandled attack outcome" görürdü,
                // oysa yaptığı şey yalnızca sabırsızlanmaktı.
                //
                // RejectedActorCannotAct İLE BİRLEŞTİRİLMEDİ ve gerekçe
                // oyuncunun göreceği CÜMLE: "şu an eylem yapamaz" düşmüş bir
                // birimi anlatır ve oyuncuyu başka bir birim seçmeye iter,
                // oysa burada birim sapasağlam — yalnız biraz beklemesi
                // gerekiyor.
                case AttackOutcome.RejectedOnCooldown:
                    Debug.Log($"[Board] '{attacker.Name}' henüz yeniden vuramaz.", this);
                    break;

                // default LOG DEĞİL LogError: buraya düşmek "AttackOutcome'a yeni
                // bir değer eklendi ve bu switch güncellenmedi" demektir, yani
                // bir programcı hatasıdır. Bir switch DEYİMİ için derleyici
                // uyarmaz; görünürlüğü bu dal sağlıyor.
                default:
                    Debug.LogError($"[Board] Unhandled attack outcome: {outcome}.", this);
                    break;
            }

            // ██ SEÇİM ARTIK BURADAN BIRAKILMIYOR ██
            // Burada `if (landed) ReleaseSelectionAfterStrike(attacker);`
            // duruyordu ve gerekçesi doğruydu: vuruş TEK SEFERLİK bir olaydı,
            // isabet ettiği an oyuncunun o birimle işi bitmiş sayılırdı.
            // Kalıcı emirle o dünya değişti — aynı emir saniyede bir vuruyor ve
            // her isabette seçimi düşürmek, birimini gözlemek ya da
            // yönlendirmek için yeniden seçen oyuncunun elinden onu tekrar
            // tekrar alırdı. Seçimi bırakan yer artık emrin YAZILDIĞI an.
            // → IssueOrder
            // → Docs/deep/konular/09-kararlarin-cevrilmesi.md (madde 2)
        }

        /// <summary>
        /// Hareket sonucuna göre ekranı ve Console'u günceller.
        /// </summary>
        // → BoardAdapter.md#reacttomovemoveoutcome-outcome-unit-unit-int-x-int-y
        // DERİN ANLATIM: Docs/deep/konular/06-sonuc-enumlari.md
        private void ReactToMove(MoveOutcome outcome, Unit unit, int x, int y, List<GridStep> path)
        {
            switch (outcome)
            {
                case MoveOutcome.Moved:
                    // Görsel tahtayı TAKİP eder, tahtaya yön vermez: hareketi
                    // MoveAction çoktan yaptı. Ters sırada yazılsaydı reddedilen
                    // bir hareket ekranda gerçekleşmiş görünürdü.
                    WalkViewAlong(unit, path, x, y);
                    Debug.Log($"[Board] '{unit.Name}' is walking to ({x},{y}) — {path.Count} step(s).", this);
                    break;

                // Oyuncu tıkladı ama oraya varılamıyor: hedef, birimler ya da
                // tahta kenarıyla çevrili. Menzil kuralının yerini bu aldı.
                case MoveOutcome.RejectedUnreachable:
                    Debug.Log($"[Board] '{unit.Name}' cannot reach ({x},{y}); no path is open.", this);
                    break;

                // BUGÜN ULAŞILAMAZ: tahta menzil sormayan sürümü çağırıyor.
                // Yazılı kalıyor çünkü menzilli sürüm Core'da hâlâ duruyor ve
                // tur bazlı bir mod geri geldiği gün bu dal yeniden canlanır.
                case MoveOutcome.RejectedOutOfRange:
                    Debug.Log($"[Board] ({x},{y}) is out of range for '{unit.Name}'.", this);
                    break;

                // AŞAĞIDAKİ İKİ DAL BUGÜN ULAŞILAMAZ ve yine de yazılı: iki
                // kuralın da sahibi bu tip değil. Yazılı bir dal bedavadır;
                // sessizce düşen bir dal, Console'da hiç görünmeyen bir hatadır.
                case MoveOutcome.RejectedCellOccupied:
                    Debug.Log($"[Board] ({x},{y}) is occupied; '{unit.Name}' stayed put.", this);
                    break;

                case MoveOutcome.RejectedInvalidDestination:
                    Debug.Log($"[Board] ({x},{y}) is not a cell on this board.", this);
                    break;

                // İKİZİ AttackOutcome'da, aynı adla — ama bu değeri MoveAction
                // ASLA üretemez (ne UnitState'i ne sırayı görür), yalnız
                // BattleActions üretir. Çağıran açısından fark yok ve ret
                // sebebinin tek işi yapılabileni göstermek olduğu için doğru.
                case MoveOutcome.RejectedActorCannotAct:
                    Debug.Log($"[Board] '{unit.Name}' cannot act right now; the move was rejected. {DescribeCondition(unit)}", this);
                    break;

                default:
                    Debug.LogError($"[Board] Unhandled move outcome: {outcome}.", this);
                    break;
            }
        }

        /// <summary>
        /// Diriltme sonucuna göre Console'u günceller.
        /// </summary>
        // TAM SWITCH, ReactToAttack'teki kısmi karşılaştırmanın TERSİ — ve fark
        // ölçülebilir: orada tek soru "kondu mu" idi, burada dört cevabın dördü
        // de oyuncuya farklı bir şey söylüyor. GÖRSEL BURADAN TAZELENMİYOR:
        // diriltme bir DURUM geçişi doğurur ve o geçişin tek tetiği
        // Battle.UnitStateChanged'dır; elle tazeleme ekranın aynı olguya ikinci
        // bir kaynağı olması demekti.
        // DİRİLTEN ARTIK PARAMETRE, selectedUnit DEĞİL — ve bu bir zorunluluk:
        // kaldırma emri artık yürüyüşün SONUNDA da yürüyebiliyor ve o an
        // seçimin hâlâ aynı savaşçıda durduğunu varsaymak, cümleyi yanlış
        // birime yazma riskini kabul etmek olurdu.
        private void ReactToRevive(Unit reviver, ReviveOutcome outcome, Unit target, int x, int y)
        {
            switch (outcome)
            {
                case ReviveOutcome.Revived:
                    Debug.Log($"[Board] '{target.Name}' at ({x},{y}) was REVIVED. {DescribeCondition(target)}", this);
                    break;

                // CÜMLE ARTIK "çok uzak" DEMİYOR, ve sebebi yeni akış: uzak bir
                // dosta tıklamak yaklaşmayı KENDİSİ başlatıyor, dolayısıyla
                // buraya düşen bir menzil reddi yürüyüşün beklenmedik biçimde
                // yetmediği anlamına geliyor.
                case ReviveOutcome.RejectedOutOfRange:
                    Debug.Log(
                        $"[Board] '{reviver.Name}' is not close enough to revive '{target.Name}' at ({x},{y}).",
                        this);
                    break;

                case ReviveOutcome.RejectedInvalidTarget:
                    Debug.Log($"[Board] '{target.Name}' at ({x},{y}) cannot be revived. {DescribeCondition(target)}", this);
                    break;

                // MESAJ HEDEFİ DEĞİL DİRİLTENİ ANLATIYOR, ReactToAttack'teki
                // ikiziyle aynı sebeple: değerin adındaki "Actor" sözcüğünün
                // karşılığı budur.
                case ReviveOutcome.RejectedActorCannotAct:
                    Debug.Log($"[Board] '{reviver.Name}' cannot act right now; the revive was rejected. {DescribeCondition(reviver)}", this);
                    break;

                default:
                    Debug.LogError($"[Board] Unhandled revive outcome: {outcome}.", this);
                    break;
            }
        }

        /// <summary>
        /// Bir birimin yaşam durumunu ekrana uygular.
        /// </summary>
        // ADI DEĞİŞTİ: eski ad RefreshDownedVisual üç değerli bir bilgiyi tek
        // değerli gösteriyordu. GÖRSEL, SONUÇ ENUM'UNDAN DEĞİL DURUMDAN okunur —
        // AttackOutcome "az önce ne oldu"yu taşır, ekranın istediği ise "şu an ne
        // durumda"dır; sonuçtan türetilseydi düşmüşe tekrar vurmak birimi ayağa
        // kaldırırdı. DURUM ARTIK PARAMETRE, çünkü olay onu zaten taşıyor.
        // → BoardAdapter.md#applystatevisualunit-unit-unitstate-state
        private void ApplyStateVisual(Unit unit, UnitState state)
        {
            if (!TryGetView(unit, out UnitView view))
            {
                return;
            }

            // ÇEVİRİ ARTIK YOK — ve yokluğu bir kazanç: burada bir zamanlar üç
            // değerli bilgi iki değere iniyordu ve Downed ile Dead ekranda aynı
            // görünüyordu. Adaptör durumu OLDUĞU GİBİ geçiriyor.
            view.SetState(state);
        }

        /// <summary>
        /// Birimin görselini, yolun duraklarından geçirerek hedefe YÜRÜTÜR.
        ///
        /// OYUNDA NE İŞE YARAR: oyuncunun ışınlanma yerine yürüyüş gördüğü tek
        /// yer burası. Tahta hareketi çoktan işledi; bu çağrı yalnızca ekranı
        /// gecikmeli olarak oraya taşır.
        /// </summary>
        // YÜRÜTÜCÜ SAHNEDE ELLE BAĞLANMAZ, BURADA TAKILIR: prefab'a bir bileşen
        // daha eklemek operatöre bir sürükleme borcu daha yazardı ve unutulduğu
        // gün birim sessizce ışınlanmaya geri dönerdi. Bileşen kendi başına
        // hiçbir kural bilmediği için runtime'da eklenmesinin bir bedeli yok.
        /// <summary>
        /// Saldıranın vuruş hamlesini oynatır: silah kalkar, hedefe doğru bir
        /// adım atılır ve yerine dönülür.
        /// </summary>
        // YÜRÜTÜCÜYLE AYNI DESEN: bileşen prefab'da değil, ilk ihtiyaç anında
        // runtime'da takılıyor. Gerekçe orada yazılı ve burada tekrarlanmıyor —
        // operatöre bir sürükleme borcu daha yazmamak.
        private void PlayAttackVisual(Unit attacker, int targetX, int targetY)
        {
            if (attacker == null || !TryGetView(attacker, out UnitView view))
            {
                return;
            }

            UnitAttackView attackView = view.GetComponent<UnitAttackView>();
            if (attackView == null)
            {
                attackView = view.gameObject.AddComponent<UnitAttackView>();
            }

            attackView.Play(CellCentre(targetX, targetY));
        }

        /// <summary>
        /// Menzilli bir vuruşta saldırandan hedefe bir mermi uçurur: ok ya da
        /// büyücünün asasından çıkan parıltı.
        ///
        /// OYUNDA NE İŞE YARAR: uzaktan vuran birimin saldırısı bugün ekranda
        /// yalnız hedefin canında görünüyordu; oyuncu vuruşun NEREDEN geldiğini
        /// göremiyordu. Uçan bir görsel o bağı kuruyor.
        /// </summary>
        // BİTİŞİK VURUŞTA MERMİ YOK ve bu bir eksiklik değil bir ayrım: bir
        // hücrelik uçuş, ok atıldığını değil ekranın tıkandığını düşündürürdü.
        // Yakın vuruşun anlatıcısı zaten hamlenin kendisi.
        //
        // KAYNAK HÜCRE SAVAŞTAN OKUNUYOR, GÖRSELDEN DEĞİL: saldıran bir yapı
        // olabilir ve yapının UnitView'ı yoktur. Aynı soru iki tip için tek
        // kapıdan (Battle.TryGetPosition) cevaplanıyor.
        private void PlayRangedVisual(Unit attacker, int targetX, int targetY)
        {
            if (projectileSprite == null || attacker == null)
            {
                return;
            }

            if (!battle.TryGetPosition(attacker, out int fromX, out int fromY))
            {
                return;
            }

            if (GridDistance.Between(fromX, fromY, targetX, targetY) <= 1)
            {
                return;
            }

            ProjectileView.Fire(
                transform,
                projectileSprite,
                CellCentre(fromX, fromY),
                CellCentre(targetX, targetY));
        }

        /// <summary>
        /// Emri deftere yazar ve SEÇİMİ BIRAKIR.
        /// </summary>
        // ██ SEÇİM EMİR YAZILDIĞI AN BIRAKILIYOR — ÇEVRİLMİŞ BİR KARAR ██
        // Eskiden seçim, İSABET EDEN bir vuruştan SONRA bırakılıyordu
        // (ReleaseSelectionAfterStrike) ve o üyenin iki koruma satırı vardı:
        // "saldıran seçili olan mı" ve "saldıran bir yapı mı". Kalıcı emirle
        // birlikte o yer yanlış oldu — emir tekrar tekrar vuruyor ve her
        // isabette seçimi düşürmek, birimini yeniden seçmiş oyuncunun elinden
        // onu ikinci kez alırdı.
        //
        // İKİ KORUMA SATIRI DA GEREKSİZLEŞTİ VE SİLİNDİ, çünkü artık bu üyeye
        // ulaşan tek yol OYUNCUNUN kendi tıklaması: kendiliğinden ateş eden
        // kule buradan geçmiyor (emri yok), yapı seçimi ise saldırı dalından
        // ÖNCE dönüyor. Koruma bir koşuldan yapıya taşındı.
        //
        // EMİR SEÇİMDEN BAĞIMSIZ YAŞAR ve bu satırın çalışabilmesinin tek
        // sebebi o: eski emir "saldıran hâlâ seçili mi" diye soruyordu, yani
        // seçimi bırakmak emri iptal ederdi. O soru IUnitOrderHost'ta artık YOK.
        // → Orders/IUnitOrderHost.cs
        private void IssueOrder(Unit unit, IUnitOrder order)
        {
            orders.Write(unit, order);

            if (ReferenceEquals(unit, selectedUnit))
            {
                ClearSelection();
            }
        }

        /// <summary>
        /// Vurulan tarafa, emri YOKSA, saldırgana karşılık veren bir emir yazar.
        /// </summary>
        // ██ BU PROJEDEKİ STRATEGY NOKTASI TAM OLARAK AŞAĞIDAKİ İKİLİ ██
        // Emir kurmanın öteki iki noktasında (düşmana tıklama, düşmüş dosta
        // tıklama) hangi sınıfın new'leneceği DERLEME zamanında bellidir —
        // seçen şey programcı. Burada seçen şey program: emri doğuran olay bir
        // vuruşun isabet etmesi ve o olay, hangi cinsin gerekeceği hakkında
        // hiçbir şey söylemiyor. Cevabı ancak vuruş indiği anda savaşın defteri
        // veriyor.
        //
        // SEÇİMİN DAYANDIĞI TEK OLGU: savunan YÜRÜYEBİLİYOR MU. Savaşçıysa
        // kovalar, yapıysa yerinde vurur.
        //
        // IssueOrder ÇAĞRILMIYOR ve bu bir zorunluluk: o üye emri yazdıktan
        // sonra seçimi bırakıyor, oysa karşılık emrini oyuncu vermedi. Elindeki
        // birim vurulduğu her an seçimi düşen bir tahta, oyuncunun elinden
        // birimini saniyede bir alırdı.
        // → IssueOrder
        // DERİN ANLATIM: Docs/deep/konular/11-karsilik-verme-ve-menzil.md
        private void WriteRetaliation(Unit defender, Unit aggressor)
        {
            // ██ OYUNCUNUN EMRİ EZİLMEZ ██
            // Deftere yazılmış bir emir oyuncunun kendi kararıdır; karşılık onu
            // ezseydi, saldırı emri verilen savaşçı ilk yediği darbede hedefini
            // değiştirir ve oyuncu bunu hiç istememiş olurdu. Karşılık yalnız
            // emri OLMAYAN birime yazılır.
            if (orders.TryGet(defender, out IUnitOrder _))
            {
                return;
            }

            if (battle.TryGetCombatant(defender, out Combatant _))
            {
                orders.Write(defender, new ChaseAndStrikeOrder(this, defender, aggressor));
                return;
            }

            // SİLAHSIZ YAPI KARŞILIK VERMEZ ve soru Structure.CanAttack'e
            // soruluyor: bir kışla vurulduğunda ona yazılacak emir her karede
            // RejectedActorCannotAct alır ve doğduğu karede düşerdi.
            //
            // REDDEDILEN - seçimi UnitBlueprint üstünde bir alana taşımak.
            //     retaliation = defender.Blueprint.RetaliationKind switch { ... };
            // KIRILAN: bugün karşılık cinsi İKİ ve ikisini ayıran olgu bir tasarım
            // tercihi değil bir yetenek — yapı yürüyemiyor. Alan, var olmayan bir
            // seçimi oyun verisine yazar ve hangi değerin yapıya konacağını kimse
            // söyleyemez.
            // KAZANIRDI: üçüncü bir karşılık davranışı doğduğu gün — "kaçan birim"
            // ya da "menzile girmeyip bekleyen birim" — çünkü o gün ayıran şey
            // artık bir yetenek değil bir tercih olur.
            // TEK CUMLE: iki cinste bir if, üçte bir eşleme, dörtte bir fabrika.
            if (battle.TryGetStructure(defender, out Structure fort) && fort.CanAttack)
            {
                orders.Write(defender, new StandAndStrikeOrder(this, defender, aggressor));
            }
        }

        /// <summary>
        /// Tıklanan kimlik, seçili birimin DÜŞMÜŞ bir yoldaşı mı.
        /// </summary>
        // SORUNUN İKİ YARISI DA GEREKLİ: yalnız "düşmüş mü" sorulsaydı düşmüş
        // bir DÜŞMANA tıklamak da diriltme yoluna girer ve kural katmanı onu
        // reddederken oyuncu saldıramadığını sanırdı.
        private bool IsFallenAlly(Unit clicked)
        {
            return battle.TryGetCombatant(clicked, out Combatant fallen)
                   && fallen.State == UnitState.Downed
                   && TryGetTeam(selectedUnit, out Team mine)
                   && fallen.Team == mine;
        }

        /// <summary>
        /// Bu birimin emrini unutur. Tahtaya ve savaşa HİÇ dokunmaz.
        /// </summary>
        // TEK BİRİMİN EMRİ, TAHTANINKİ DEĞİL — ve fark bu turun konusu: eski
        // CancelPendingStrike tahtadaki TEK emri silerdi, yani bir savaşçıyı
        // yürütmek ötekinin saldırısını da keserdi. Bugün iptal edilen şey
        // yalnız verilen kimliğin emri.
        private bool CancelOrder(Unit unit)
        {
            return orders.Cancel(unit);
        }

        /// <summary>
        /// Bu yapı saldırabiliyor mu — silahı var mı.
        /// </summary>
        /// <returns>Kimlik bir yapı DEĞİLSE de false.</returns>
        // "YAPI DEĞİL" İLE "SİLAHSIZ YAPI" AYNI CEVABI VERİYOR ve bu güvenli:
        // tek çağıran zaten IsStructureIdentity dalının içinde, yani kimliğin
        // yapı olduğu orada sorulmuş durumda. Ayrı bir cevap üretilseydi hiçbir
        // çağıranın ayırt etmediği bir üçüncü hâl doğardı.
        private bool CanStructureAttack(Unit unit)
        {
            return battle.TryGetStructure(unit, out Structure structure) && structure.CanAttack;
        }

        /// <summary>
        /// Tıklanan şeyi seçili yapar: bu tıklama bir eylem değil, bir bakış.
        /// </summary>
        // AYRI BİR ÜYE, İKİ SATIRLIK OLMASINA RAĞMEN — ve gerekçesi operatörün
        // kendi kelimesi: "reusable şekilde otomatik selection'ı ona göre
        // ayarlatsa." Bugün tek çağıranı var (silahsız yapı dalı), ama kural
        // artık bir ADA sahip: "yapabileceğin bir şey yoksa tıklama odak
        // devridir." İkinci bir dal doğduğunda (örneğin düşmüş bir savaşçıyla
        // düşmana tıklamak) kopyalanacak bir gövde değil, çağrılacak bir üye var.
        //
        // SelectUnit ZATEN ESKİ SEÇİMİ TEMİZLİYOR, o yüzden burada ClearSelection
        // yok: iki kez temizlemek, dinleyiciyi bir kez daha boş seçimle
        // uyandırmaktan başka bir şey yapmazdı.
        private void TransferFocusTo(Unit clicked, int x, int y)
        {
            SelectUnit(clicked);

            Debug.Log(
                $"[Board] ({x},{y}) holds '{clicked.Name}' - FOCUS MOVED.{DescribeOrder(clicked)}",
                this);
        }

        /// <summary>
        /// Bu birimin emri varsa oyuncuya söylenecek eki verir; yoksa boş dizge.
        /// </summary>
        // OYUNDA NE İŞE YARAR: emrini verip seçimi bırakılan savaşçıya tekrar
        // tıklayan oyuncu, ona ne söylediğini görsün.
        //
        // AYRI BİR ÜYE VE İKİ ÇAĞIRAN: seçim iki ayrı daldan yapılıyor (hiçbir
        // şey seçili değilken ve dost bir birime geçerken) ve cümle ikisinde de
        // aynı olmalı. DescribeCondition'ın deseni birebir aynı sebeple ayrı.
        private string DescribeOrder(Unit unit)
        {
            return orders.TryGet(unit, out IUnitOrder order) ? $" It is {order.Describe()}." : string.Empty;
        }

        /// <summary>
        /// Bu kimliğin görseli şu anda yürüyor mu?
        /// </summary>
        // TryGetView KULLANILMIYOR ve bu bilinçli: o üye görsel bulamadığında
        // LogError basıyor, oysa burada görselsiz bir kimlik gayet normal bir
        // cevap üretir — yürümüyor. Sorunun her karede sorulduğu düşünülürse
        // gürültü de tek satır kalmazdı.
        private bool IsViewWalking(Unit unit)
        {
            if (unit == null || !unitViews.TryGetValue(unit, out UnitView view) || view == null)
            {
                return false;
            }

            UnitWalker walker = view.GetComponent<UnitWalker>();
            return walker != null && walker.IsWalking;
        }

        private void WalkViewAlong(Unit unit, List<GridStep> path, int x, int y)
        {
            if (!TryGetView(unit, out UnitView view))
            {
                return;
            }

            UnitWalker walker = view.GetComponent<UnitWalker>();
            if (walker == null)
            {
                walker = view.gameObject.AddComponent<UnitWalker>();
            }

            // HAMLE YÜRÜYÜŞTEN ÖNCE İPTAL EDİLİR. İkisi de aynı
            // transform.position'a yazıyor ve vuruş hamlesi bitince kendi
            // "dinlenme" noktasına GERİ yazıyordu: 0,28 saniyelik pencerede
            // yürüyen asker vurduğu noktaya sıçrayıp geri dönüyordu. İptali
            // bugüne kadar yalnız havuz çağırıyordu, yani yalnız ölen birim
            // temizleniyordu.
            UnitAttackView attackView = view.GetComponent<UnitAttackView>();
            if (attackView != null)
            {
                attackView.Cancel();

                // POZ DA BIRAKILIYOR: iptal yalnız hamleyi durduruyor ve
                // "saldırıyorum" karesini kapatan satır hamlenin SONUNDA
                // yaşıyor — yani iptal edilen bir hamlede hiç çalışmıyor.
                // Bırakılmasaydı asker bütün yolu silahı kalkık yürürdü.
                view.SetAttacking(false);
            }

            // Yol boşsa (savunma amaçlı) hedefe otur: ekran ile tahtanın
            // ayrışması, yavaş yürümekten çok daha kötü bir hatadır.
            if (path == null || path.Count == 0)
            {
                view.transform.position = CellCentre(x, y);
                return;
            }

            var waypoints = new List<Vector3>(path.Count);
            for (int i = 0; i < path.Count; i++)
            {
                waypoints.Add(CellCentre(path[i].X, path[i].Y));
            }

            walker.Walk(waypoints, moveSpeed);
        }

        /// <summary>
        /// Savaştan çıkarılmış bir birimin ya da yapının görselini sahneden siler.
        ///
        /// TEK metot, iki tablo — ve şekli <see cref="Battle.RemoveUnit"/>'in
        /// birebir ikizi, aynı sebeple: süpürme tamponu savaşçıları ve yapıları
        /// AYNI listede veriyor, dolayısıyla çağıranın elinde yalnızca bir
        /// <see cref="Unit"/> var ve o kimliğin hangi tabloda olduğunu bilmek bu
        /// tipin işi.
        /// </summary>
        private void DespawnView(Unit unit)
        {
            // ██ SEÇİM ARTIK TEK KAPIDAN DÜŞÜYOR ██
            // Burada `selectedUnit = null` yazılıydı ve ölçüm şuydu: seçili
            // birim öldüğünde SelectionChanged hiç yayınlanmıyor, durum şeridi
            // ölmüş savaşçının canını anlatmaya devam ediyordu. Üretim paneli
            // temizleniyordu çünkü ProductionDirector AYRICA UnitRemoved'a
            // abone — iki dinleyici arasındaki bu sessiz fark tam olarak iki
            // kapının farkıydı.
            //
            // ESKİ GEREKÇE ÖLÇÜLDÜ VE DÜŞTÜ: "yok edilecek nesneye çerçeve
            // kapatmak" tehlikeli değil, çünkü görsel bu satırda HÂLÂ ayakta —
            // silinmesi aşağıda. Havuza geri verilen bir görselde ise çerçeveyi
            // kapatmak zorunlu bile: kapatılmasaydı sıradaki kiracı seçili
            // görünürdü.
            if (ReferenceEquals(unit, selectedUnit))
            {
                ClearSelection();
            }

            // TAHTADAN KALKAN BİR KİMLİK EMİRLERİ DE GÖTÜRÜR — kendi emrini VE
            // kendisini hedefleyen bütün emirleri. İkinci çağrı ÇOĞUL, ve
            // çoğulluğu bu turun kazancı: aynı hedefe saldıran üç savaşçının
            // üçünün emri de aynı anda düşer.
            //
            // ESKİ GEREKÇE ("bırakılsaydı savaşta bulunmayan bir kimliğe
            // saldırı çağrılır ve istisna atardı") ARTIK GEÇERLİ DEĞİL: emir
            // vurmadan önce konumu kendisi soruyor ve bulamazsa iptal oluyor.
            // Süpürme yine de burada, çünkü cevabın AYNI KAREDE görünmesi
            // gerekiyor — çöp kutusuyla silinen bir birimin peşindeki emir bir
            // kare daha yaşamamalı.
            CancelOrder(unit);
            orders.CancelTargeting(unit);

            // Yıkılan yapının atış sayacı da düşer; bırakılsaydı sözlük savaş
            // boyunca büyür ve tarama ölü kayıtları gezerdi.
            structureFireTimers.Remove(unit);

            // Bar, sahibinin ÇOCUĞU olduğu için sahneden kendiliğinden gidiyor;
            // silinmesi gereken tek şey tabloda kalan ok. Bırakılsaydı tablo
            // savaş boyunca büyür ve RefreshHealthBars ölü kayıtları gezerdi.
            healthBars.Remove(unit);

            // Şerit de aynı sebeple: sahibinin çocuğu olduğu için sahneden
            // kendiliğinden gidiyor, tabloda kalan ok elle siliniyor.
            productionTimers.Remove(unit);

            // YAPI ÖNCE SORULUYOR ve sıra burada GÖZLENEMEZ: bir kimlik aynı
            // anda hem savaşçı hem yapı OLAMAZ, o kelepçe ThrowIfCannotJoin'de
            // duruyor. Sorunun ayrı bir dal olmasının sebebi sıra değil TİP —
            // yapı görselinde bir UnitView yok, dolayısıyla TryGetView onu
            // aramaya bile gidemez.
            if (structureViews.TryGetValue(unit, out StructureView structureView))
            {
                // Önce tablodan çıkar, sonra sahneden sil — gerekçe aşağıda,
                // birim dalında bir kez yazılı ve burada TEKRAR EDİLMİYOR.
                // Yok edilen şey BİLEŞEN değil onu taşıyan nesne: bileşeni tek
                // başına silmek sahnede sahipsiz bir çizici bırakırdı.
                structureViews.Remove(unit);
                Destroy(structureView.gameObject);

                Debug.Log($"[Board] structure '{unit.Name}' was cleaned up and left the battle.", this);
                return;
            }

            if (!TryGetView(unit, out UnitView view))
            {
                return;
            }

            // Önce tablodan çıkar, sonra sahneden sil. Ters sırada tabloda YOK
            // EDİLMİŞ bir görsel referansı kalırdı ve Unity'nin aşırı yüklenmiş
            // eşitliği yüzünden "null gibi ama null değil" hâlde dolaşırdı.
            unitViews.Remove(unit);

            // YOK EDİLMİYOR, HAVUZA GERİ VERİLİYOR. Eskiden burada Destroy vardı;
            // her ölüm bir yıkım, her doğum bir tahsis demekti. Artık görsel
            // gizlenip saklanıyor ve bir sonraki birim onu devralıyor.
            //
            // TABLODAN ÇIKARMA HÂLÂ ÖNCE: havuza verilen bir görselin tabloda
            // kalan oku, o görsel BAŞKA bir birime kiralandığında iki kimliğin
            // aynı nesneyi göstermesine yol açardı.
            viewPool.Return(view);

            Debug.Log($"[Board] '{unit.Name}' was cleaned up and left the battle.", this);
        }

        /// <summary>
        /// Verilen birimi seçili yapar ve öncekinin seçimini kaldırır.
        /// </summary>
        private void SelectUnit(Unit unit)
        {
            // Önce eskiyi temizle: iki birim aynı anda seçili görünemez.
            // Bu satır olmasaydı her tıklama bir birimin daha çerçevesini açar
            // ve hiçbiri geri kapanmazdı.
            ClearSelection();

            selectedUnit = unit;
            SetSelectionVisual(unit, true);

            // YAYIN EN SONDA: dinleyici uyandığında tahtanın seçim durumu ve
            // ekrandaki çerçevesi ZATEN tutarlı olmalı. Üste konsaydı dinleyici,
            // henüz yazılmamış bir seçimi sorabilirdi.
            //
            // BİR SEÇİMDEN ÖTEKİNE GEÇERKEN ARADA null GEÇİYOR — üstteki
            // ClearSelection kendi yayınını yapıyor — ve bu ÖLÇÜLMÜŞ, sınırlı bir
            // bedeldir: ara durum hiçbir kareye çıkmaz, yalnız dinleyici aynı
            // tıklamada iki kez uyanır. Yayını bastırmak için ClearSelection'ın
            // gövdesini buraya kopyalamak REDDEDİLDİ; o kopya "iki birim aynı
            // anda seçili görünemez" kelepçesini iki yere böler ve biri
            // değiştiği gün öbürü sessizce eskir.
            SelectionChanged?.Invoke(unit);
        }

        /// <summary>
        /// Seçimi kaldırır. Seçim yoksa hiçbir şey yapmaz.
        /// </summary>
        private void ClearSelection()
        {
            if (selectedUnit == null)
            {
                return;
            }

            SetSelectionVisual(selectedUnit, false);
            selectedUnit = null;

            // YAYIN ERKEN ÇIKIŞIN ALTINDA ve bu bir yerleşim kazası değil: üste
            // konsaydı seçim ZATEN yokken yapılan her tıklama aynı boş seçimi
            // yeniden duyururdu. Dinleyici her yayında sağ panelini yeniden
            // kuruyor, yani tekrar bedavaya gelmiyor.
            SelectionChanged?.Invoke(null);
        }

        /// <summary>
        /// Bir birimin görseline seçim durumunu iletir.
        /// </summary>
        // YAPI DA SEÇİLDİĞİNİ GÖSTERİR, ve bu dal ölçülmüş iki arızayı birden
        // kapatıyor: oyuncu kendi binasına tıkladığında (üretim paneli ancak
        // seçiliyken açıldığı için bu NORMAL akış) Console on iki satır
        // "No view registered for unit" hatası basıyordu, ve binanın seçili
        // olduğu ekranda HİÇ görünmüyordu.
        //
        // RENK ÇARPANI ARTIK BURADA HESAPLANMIYOR, YALNIZCA İSTENİYOR — ve eski
        // ölçü ("binanın kendi rengi hiç yazılmıyor, çarpan bir şeyin üstüne
        // binmiyor") artık YANLIŞ: yıkılan bina da aynı alana yazıyor. İki
        // ekseni tek çarpımda birleştiren yer StructureView; adaptör hangi
        // rengin çıkacağını bilmiyor, yalnız niyeti söylüyor.
        private void SetSelectionVisual(Unit unit, bool isSelected)
        {
            if (unit != null && structureViews.TryGetValue(unit, out StructureView structureView)
                && structureView != null)
            {
                structureView.SetSelected(isSelected);

                // ██ TEK BAŞINA RENK ÇARPANI YETMEDİ — VE BU ÖLÇÜLDÜ ██
                // Operatör: "bazen yapılara tıkladığımda ne yazık ki seçili
                // oldukları gözükmüyor." Sebep yukarıdaki satırın istediği şeyin
                // kendisi: StructureView'deki selectedTint bir ÇARPAN ve binanın
                // kendi sprite'ı zaten sıcak renkliyse çarpım neredeyse hiçbir
                // şeyi değiştirmiyor. Sarımsı bir kışlada seçili ile seçilmemiş
                // hâl gözle ayırt edilemiyordu.
                //
                // ÇÖZÜM SAVAŞÇIDAN GELİYOR, YENİ BİR FİKİRDEN DEĞİL: savaşçının
                // seçimi zaten ayrı bir ÇERÇEVE nesnesinde yaşıyor
                // (UnitView.selectionOverlay) ve tam da bu yüzden gövde rengine
                // bağlı değil. Yapı bugüne kadar o mekanizmanın dışında
                // kalmıştı; artık aynı dili konuşuyor.
                //
                // ÇARPAN SİLİNMEDİ, ÜSTÜNE KONDU: ikisi birlikte "seçili" hâlini
                // iki ayrı kanaldan (renk + kenar) anlatıyor ve tek kanal
                // kaybolduğunda öteki ayakta kalıyor.
                SetStructureSelectionFrame(structureView.gameObject, isSelected);
                return;
            }

            if (!TryGetView(unit, out UnitView view))
            {
                return;
            }

            // Eski ApplyTint burada SpriteRenderer'ı bulup color'ını yazıyordu ve
            // renk ÇARPMA ile uygulandığı için seçim, birimin kendi rengini
            // bozuyordu. Artık seçim ayrı bir çerçeve nesnesinde yaşıyor; adaptör
            // o çerçeveyi görmüyor bile, sadece niyeti söylüyor.
            // → BoardAdapter.md#setselectionvisualunit-unit-bool-isselected
            view.SetSelected(isSelected);
        }

        /// <summary>
        /// Bir yapının çevresindeki seçim çerçevesini açar ya da kapatır.
        /// </summary>
        // ÇERÇEVE İLK SEÇİMDE KURULUYOR, DOĞUMDA DEĞİL: hiç seçilmeyen bir bina
        // için kurulan nesne, hiç çizilmeyecek bir çöp olurdu. Aynı "ilk
        // ihtiyaçta kur" deseni can barında ve geri sayım şeridinde de var.
        //
        // ÖLÇEK YAZILMIYOR ve bu bir unutma değil: çerçeve yapının ÇOCUĞU,
        // yapının localScale'i ise onun çizili boyu. Yerel ölçek 1 bırakıldığında
        // çerçeve tam binanın boyuna oturuyor — iki hücre eninde bir karargâhta
        // da, tek hücrelik bir kışlada da. Ters ölçek uygulansaydı (can barının
        // yaptığı gibi) çerçeve her binada aynı boyda kalır ve büyük binanın
        // içinde asılı dururdu.
        //
        // SIRA İMLEÇ ÇERÇEVESİYLE AYNI RAFTA (HoverSortingOrder) ve çakışma
        // zararsız: ikisi de içi boş birer kenarlık ve seçim çerçevesi binanın
        // BOYUNDA, imleç çerçevesi ise tek HÜCRE boyunda — bir hücreden geniş
        // binada seçim çerçevesi imlecin dışına taşıp görünür kalıyor.
        private void SetStructureSelectionFrame(GameObject structureObject, bool isSelected)
        {
            Transform existing = structureObject.transform.Find(StructureSelectionFrameName);

            if (existing != null)
            {
                var existingRenderer = existing.GetComponent<SpriteRenderer>();
                if (existingRenderer != null)
                {
                    existingRenderer.enabled = isSelected;
                }

                return;
            }

            // SEÇİM KALDIRILIRKEN KURULMUYOR: kapatılacak bir şey yoksa
            // kurulacak bir şey de yok. Bu kapı olmasaydı her ClearSelection
            // çağrısı, hiç seçilmemiş binalara da birer çerçeve takardı.
            if (!isSelected || hoverFrameSprite == null)
            {
                return;
            }

            var go = new GameObject(StructureSelectionFrameName);
            go.transform.SetParent(structureObject.transform, worldPositionStays: false);
            go.transform.localPosition = Vector3.zero;

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = hoverFrameSprite;
            renderer.color = StructureSelectionFrameColour;
            renderer.sortingOrder = HoverSortingOrder;
        }

        /// <summary>
        /// Birimin görselini verir; yoksa gürültüyle şikâyet eder.
        /// </summary>
        // Dört çağıranın (seçim, hareket, durum, temizlik) aynı hata mesajını
        // kopyalamaması için var. Tabloda olmamak bir OYUN olgusu değil bir
        // PROGRAMCI hatasıdır; bu yüzden sessiz false değil, LogError + false.
        // → BoardAdapter.md#trygetviewunit-unit-out-unitview-view
        private bool TryGetView(Unit unit, out UnitView view)
        {
            if (unitViews.TryGetValue(unit, out view))
            {
                return true;
            }

            // YAPININ SAVAŞÇI GÖRÜNÜMÜ YOKTUR ve bu bir programcı hatası değil
            // bir TİP olgusudur: bina görseli koddan kuruluyor ve üstünde hiçbir
            // UnitView taşımıyor. Bu erken çıkış olmadan seçim, hamle ve durum
            // yollarının hepsi bina için kırmızı satır basıyordu — ölçüldü, tek
            // bir seçim on iki hata üretiyordu. TEŞHİS SİLİNMİYOR, yalnız kapsamı
            // daralıyor: savaşçı için tabloda bulunmamak hâlâ gerçek bir arıza.
            if (structureViews.ContainsKey(unit))
            {
                return false;
            }

            Debug.LogError($"[Board] No view registered for unit '{unit.Name}'.", this);
            return false;
        }

        /// <summary>
        /// Bir birimin can ve durum özetini log satırı için hazırlar.
        /// </summary>
        private string DescribeCondition(Unit unit)
        {
            if (!battle.TryGetCombatant(unit, out Combatant combatant))
            {
                return "(not in this battle)";
            }

            return $"health={combatant.CurrentHealth}, state={combatant.State}";
        }
    }
}
