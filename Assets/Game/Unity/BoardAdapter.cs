using System;
using System.Collections.Generic;
using GridStrategy.Battle;
using GridStrategy.Combat;
using GridStrategy.Core;
using UnityEngine;

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

    // ═══ GİRİŞ OKUMA NOTU — ÜÇLÜ YALNIZ BİR AKIŞTA GEREKİR ═══════════
    // ÖLÇÜSÜ ŞU: bu dosyada Input.GetMouseButton ile başlayan DÖRT çağrı
    // var, üç değil — Update içinde 1 (GetMouseButtonDown), FeedGesture
    // içinde 3 (Down / GetMouseButton / Up). Dördü İKİ AYRI AKIŞA düşer.
    //
    // SIRADAN TAHTA TIKLAMASI — tek sorgu yeter: Update'teki
    // GetMouseButtonDown doğrudan HandleClick'e gider ve PointerGesture'a
    // HİÇ UĞRAMAZ. Seçme, saldırı ve hareket bu yoldan geçer.
    //
    // YERLEŞTİRME KİPİ — üçü birden gerekir: FeedGesture'ın TEK çağıranı
    // UpdatePlacement'tır, o da yalnız isPlacingStructure doğruyken koşar.
    // Bir tıklama ile bir sürükleme BAŞLANGIÇTA aynıdır: Down yalnız
    // basıldığı kareyi, GetMouseButton basılı geçen HER kareyi, Up yalnız
    // bırakıldığı kareyi görür — ayrımı ancak ortadaki sorgu üretir.
    //
    // Ayrımın KARARI yine de burada değil: GridStrategy.Core.PointerGesture.
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
    [RequireComponent(typeof(Grid))]
    public sealed class BoardAdapter : MonoBehaviour, IPlacementBoard
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

        [Tooltip("Key that enters structure placement mode. Requires a selected unit.")]
        [SerializeField] private KeyCode placementModeKey = KeyCode.B;

        [Tooltip("Key that cancels structure placement mode without touching the board.")]
        [SerializeField] private KeyCode placementCancelKey = KeyCode.Escape;

        // BASILI TUTULAN TUŞ, BİR KİP DEĞİL — ve ayrım ölçülebilir: yerleştirme
        // kipi kareler arasında yaşayan İKİ alan gerektirdi (isPlacingStructure,
        // ghostIsCarried), diriltme ise tek bir tıklamanın anlamını değiştirmekle
        // bitiyor ve geriye hatırlanacak hiçbir şey bırakmıyor. Kip yapılsaydı
        // bırakılması gereken üçüncü bir durum doğardı ve OnDisable'ın bırakma
        // listesi uzardı.
        [Tooltip("Hold this key while clicking a fallen ally to REVIVE instead of attacking.")]
        // Seçili birimi/yapıyı kaldıran tuş. Arayüzdeki çöp kutusu düğmesiyle
        // AYNI işi yapar; düğme henüz sahnede yoksa oyun yine de oynanabilsin
        // diye var.
        [Header("Remove selected - same action as the trash button")]
        [SerializeField] private KeyCode removeSelectedKey = KeyCode.Delete;

        // Yapı görselinin hücreye göre büyüklüğü. 1 = tam bir hücre. Birimler
        // 1,25 ölçekle çizildiği için yapı ondan büyük olmalı, yoksa bina
        // askerden küçük görünür.
        [Header("Structure size relative to one cell")]
        [SerializeField, Min(0.5f)] private float structureScale = 1.6f;

        // Can barının çizildiği düz beyaz kare. Renk koddan veriliyor, bu yüzden
        // sprite'ın kendisi renksiz olmalı.
        [Header("Health bar - assign the plain white square sprite")]
        [SerializeField] private Sprite healthBarSprite;

        // İmlecin altındaki hücreyi çerçeveleyen görsel. İçi boş bir kare olmalı,
        // yoksa hücrenin içindekini kapatır.
        [Header("Hover highlight - assign the hollow cell frame sprite")]
        [SerializeField] private Sprite hoverFrameSprite;

        [SerializeField] private KeyCode reviveModifierKey = KeyCode.LeftShift;

        [Header("Structure stats - applied to every placed structure")]
        [Tooltip("Starting and maximum health of each placed structure.")]
        [SerializeField, Min(1)] private int structureMaxHealth = 50;

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
        // görseli ise CreateStructureVisual içinde koddan kuruluyor ve bizim
        // hiçbir bileşenimizi taşımıyor — tek tabloda birleştirmek, olmayan bir
        // bileşeni istemek ya da ortak ataya (Component) inip temizliğin hangi
        // tarafta olduğunu yeniden sordurmak olurdu.
        //
        // BU TABLONUN YOKLUĞU BİR HATAYDI, bir sadelik değil: yapı görseli
        // doğuyor ama hiçbir yere yazılmıyordu, temizlik süpürmesi onu
        // bulamıyor, LogError basıyor ve enkaz ekranda kalıyordu.
        private readonly Dictionary<Unit, GameObject> structureViews =
            new Dictionary<Unit, GameObject>();

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

        // Yerleştirme kipinde miyiz. Bir OYUN durumudur, çeviri durumu değil —
        // yani bu alan da selectedUnit gibi rol başlığındaki "hafıza: var"
        // satırının altına düşer.
        private bool isPlacingStructure;

        // Şu an yerleştirilmekte olan yapının görseli. Paletten sürüklenen
        // düğmenin simgesi buraya yazılır; hem önizleme hayaleti hem de tahtaya
        // konan bina onu kullanır — böylece oyuncunun sürüklerken gördüğü şey
        // ile bıraktığında oluşan şey AYNI olur.
        private Sprite pendingStructureSprite;

        // Kimlik → can barı. ÜÇÜNCÜ bir tablo ve bilerek: alternatifi her karede
        // GetComponentInChildren çağırmaktı — o da savaşçı ve yapı görsellerinin
        // İÇ YAPISINI (barın bir çocuk olduğu) tahtanın bilgisi hâline getirirdi.
        // Tablo tek yerden doldurulup tek yerden (DespawnView) siliniyor.
        private readonly Dictionary<Unit, HealthBarView> healthBars =
            new Dictionary<Unit, HealthBarView>();

        // İmleç çerçevesinin çizicisi. Sahnede elle kurulmuyor, Awake'te
        // doğuyor — bir nesne daha sürüklettirmemek için.
        private SpriteRenderer hoverHighlight;

        // Savaşçı görsellerinin havuzu. Awake'te kuruluyor çünkü prefab ve
        // ebeveyn ancak o zaman hazır; alan tanımında kurulsaydı serileştirilmiş
        // prefab referansı henüz okunmamış olurdu.
        private UnitViewPool viewPool;

        // Hayalet fareye YAPIŞTI mı. İki giriş şeklini ayıran tek alan budur:
        // sürükle-bırak hiç yapıştırmaz, tıkla-bırak ilk bırakışta yapıştırır.
        // Sayaç değil bool, çünkü ayrım "kaçıncı tıklama" değil — hayalet fareye
        // bağlı mı bağlı değil mi. → BoardAdapter.md#ghostiscarried
        private bool ghostIsCarried;

        // ═══ IPlacementBoard SÖZLEŞMESİ — SEKİZ ÜYE, TEK YÖN ═════════════
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

        // Son duyurulan sıra. Duyurunun İKİ kez yapılmasını engelliyor: tur
        // numarası ile takımın ikisi birden karşılaştırılıyor, çünkü iki
        // oyunculu bir sırada tur numarası aynı kalırken takım değişebilir.
        private Team lastAnnouncedTeam;
        private int lastAnnouncedTurn = -1;

        // DERİN ANLATIM: Docs/deep/konular/08-motor-cagri-dongusu.md — bu metodu
        // hiçbir satır ÇAĞIRMIYOR ve bir C# `event` de değil; motor onu ADINA
        // bakarak buluyor, çağrı sırası ve koşulları orada ölçüyle yazılı.
        private void Awake()
        {
            // GetComponent bir SORGUdur: bileşen listesinde arar ve bulduğuna
            // referans döner, hiçbir şey yaratmaz. Listede bir Grid bulunacağını
            // RequireComponent garanti eder. → BoardAdapter.md#awake
            unityGrid = GetComponent<Grid>();
            battle = new Battle(width, height);

            // Eşik Inspector'dan geliyor, bu yüzden jest ancak burada
            // kurulabilir: alan bildiriminde kurulsaydı serileştirilmiş değer
            // daha okunmamış olurdu ve Inspector'daki sayı boşa çıkardı.
            gesture = new PointerGesture(dragThreshold);

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

            // Kip, bileşen kapanırken BIRAKILIR. Bırakılmasaydı yeniden
            // açılan adaptör hayaleti gizli, kipi açık bulurdu: fare tıklaması
            // görünmez bir yapıyı yerleştirirdi.
            CancelPlacement();
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
        /// Savaşın bitip bitmediğini SORAR ve bittiyse Console'a tek satır yazar.
        /// </summary>
        // KURALI BU DOSYA YAZMIYOR ve tek satırlık kanıtı şu: aşağıda ne bir
        // sayım, ne bir durum karşılaştırması, ne de bir taraf tercihi var —
        // kadroyu Battle geziyor, kazananı VictoryRules söylüyor, buranın işi
        // yalnızca cevabı Console'a taşımak.
        //
        // İLANIN TEKRARLANMAMASINI BİR BAYRAK DEĞİL, UnitLifecycle'ın SetState
        // metodundaki "yeni durum eskisine eşitse hiç yayma" erken çıkışı
        // sağlıyor: Dead uç durumdur ve ikinci kez yayılmaz. Buraya bir
        // victoryAnnounced alanı koymak, o kelepçenin ikinci bir kopyasını
        // burada tutmak olurdu.
        //
        // EKRANA HİÇBİR ŞEY ÇİZİLMİYOR ve bu bir eksiklik değil bir SINIR:
        // sonucu gösterecek arayüzün sahibi bu dosya değil.
        private void AnnounceWinnerIfAny()
        {
            // ARGÜMANLAR TAKIM ADIYLA YAZILI ve bu VictoryRules'ta adı konmuş
            // bir borcun karşılığı: imzadaki iki bool aynı tiptedir, yerleri
            // karışırsa kazanan TERS okunur ve derleyici hiçbir şey demez.
            Team winner = VictoryRules.Winner(
                battle.HasUnitsLeft(Team.Player),
                battle.HasUnitsLeft(Team.Enemy));

            if (winner == Team.None)
            {
                return;
            }

            Debug.Log($"[Board] BATTLE OVER - {winner} wins.", this);
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

            // Çerçeve girdiden ÖNCE tazeleniyor: tıklama akışı seçimi
            // değiştirdiğinde çerçevenin rengi aynı karede eskimiş kalmasın.
            UpdateHoverHighlight();

            // KİP AYRIMI, MEVCUT AKIŞIN ÜSTÜNDE — ve sıra bir karardır. Altına
            // konsaydı yerleştirme sırasındaki her basış önce HandleClick'ten
            // geçerdi: hayalet taşınırken tahtadaki birimler seçilirdi. Kip,
            // girdinin ANLAMINI baştan sona değiştirir.
            if (isPlacingStructure)
            {
                UpdatePlacement();
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

            // "Down" = SADECE basıldığı karede true; GetMouseButton (Down'suz)
            // basılı olduğu her karede true olurdu. Tek tıklama istiyoruz.
            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            HandleClick();
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

            isPlacingStructure = true;

            // Kipe her girişte hayalet SERBESTTİR: ilk bırakış onu ya
            // yerleştirir (sürükleme) ya fareye yapıştırır (tıklama).
            ghostIsCarried = false;

            // Jest, kipler ARASINDA taşınmaz. Sıfırlanmasaydı önceki kipten
            // kalan "Pressed" fazı, bu kipteki ilk bırakışı sahte bir tıklama
            // olarak okurdu.
            gesture.Reset();

            placementGhost.enabled = true;
            Debug.Log($"[Board] Placement mode ON for '{selectedUnit.Name}'. Drag and release to place, or click to carry.", this);
        }

        /// <summary>
        /// Yerleştirme kipinin tek karesi: hayaleti taşır, jesti besler ve
        /// bırakma şekline göre yerleştirir.
        /// </summary>
        private void UpdatePlacement()
        {
            // KOYACAK BİRİM ARADA KAYBOLABİLİR ve bu teorik değil:
            // AdvanceBattleTime bu metottan ÖNCE koşar ve ceset süresi dolan
            // birimi temizlerken selectedUnit'i null'a çeker.
            // → BoardAdapter.md#updateplacement
            if (selectedUnit == null)
            {
                CancelPlacement();
                Debug.Log("[Board] Placement mode ended: the placing unit left the battle.", this);
                return;
            }

            // İPTAL HER ZAMAN ÖNCE. Aşağıya konsaydı iptal tuşu, aynı karede
            // gelen bir bırakışın yerleştirmesinden SONRA işlenirdi: oyuncu
            // iptal ettiğini sanır, tahtada bir yapı bulurdu.
            if (Input.GetKeyDown(placementCancelKey))
            {
                CancelPlacement();
                Debug.Log("[Board] Placement mode CANCELLED. The board was not touched.", this);
                return;
            }

            if (!TryReadPointerCell(out float worldX, out float worldY, out int x, out int y))
            {
                return;
            }

            // HER KARE, koşulsuz: hayalet fare hücresinin MERKEZİNDE durur.
            // Yalnız sürüklerken taşınsaydı tıkla-bırak akışında hayalet yerinde
            // donar ve oyuncu nereye koyacağını göremezdi.
            placementGhost.transform.position = CellCentre(x, y);

            // HAYALETİN GEÇERSİZ HÜCREDE FARKLI GÖRÜNMESİ HENÜZ YAPILMIYOR ve
            // sebep "önemsiz" değil, SAHİPLİK: geçerliliğe PlaceStructure karar
            // verir ve cevabını ancak YERLEŞTİREREK verir. Kuralın bir KOPYASINI
            // buraya yazmak, kural büyüdüğü gün sessizce YALAN söylerdi — yeşil
            // hayalet, reddedilen yerleştirme. Önem kazanacağı koşul yazılı:
            // → BoardAdapter.md#updateplacement

            PointerPhase phase = FeedGesture(worldX, worldY);

            switch (phase)
            {
                // SÜRÜKLE-BIRAK: bırakıldığı yer yerleştirilecek yerdir.
                case PointerPhase.DragReleased:
                    CommitPlacement(x, y);
                    break;

                case PointerPhase.ClickReleased:
                    if (ghostIsCarried)
                    {
                        // TIKLA-BIRAK, ikinci tıklama: yerleştir.
                        CommitPlacement(x, y);
                    }
                    else
                    {
                        // TIKLA-BIRAK, ilk tıklama: kipte KAL, hayalet fareyi
                        // takip etmeye devam etsin.
                        ghostIsCarried = true;
                        gesture.Reset();
                        Debug.Log("[Board] Ghost is now carried. Click again to place, or cancel.", this);
                    }

                    break;
            }

            // default DALI BİLEREK YOK ve bu ReactToAttack'teki kararla
            // ÇELİŞMİYOR: beş fazın üçü "henüz bir şey olmadı" demektir. Bir
            // SONUÇ enum'unda işlenmeyen değer bir hatadır; bir FAZ enum'unda
            // işlenmeyen faz normal akıştır.
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
            // girmektir. ÇIKIŞ ARTIK GÖRSELİN ALTINDA ve fark gözlenemez:
            // CancelPlacement yalnız kip bayraklarına ve hayaletin görünürlüğüne
            // dokunuyor, görselin okuduğu sprite ise kapalı bir SpriteRenderer'da
            // da aynı.
            CancelPlacement();

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

        /// <summary>
        /// Yerleştirme kipini kapatır ve hayaleti gizler. Tahtaya DOKUNMAZ.
        /// </summary>
        // Tahtaya dokunmaması bir tesadüf değil, hayaletin gerçek bir yapı
        // OLMAMASININ doğrudan sonucudur: geri alınacak bir şey yok, çünkü
        // yapılmış bir şey yok. → BoardAdapter.md#cancelplacement
        private void CancelPlacement()
        {
            isPlacingStructure = false;
            ghostIsCarried = false;
            gesture?.Reset();

            if (placementGhost != null)
            {
                placementGhost.enabled = false;
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
        // Aynı hayaleti iki taraf yazıyor: placementModeKey ile açılan kip
        // UpdatePlacement içinde HER KARE konumluyor, bu üye ise yalnız
        // sürükleme olaylarında. Ölçülen zarar bir titreme DEĞİL, sessiz bir
        // körlüktür: kip açıkken biten bir sürükleme buraya visible=false ile
        // gelir, UpdatePlacement ise hayaleti yalnızca KONUMLAR ve bir daha hiç
        // açmaz — oyuncu kipte kalır, hayaleti göremez ve sonraki tıklaması
        // görünmeyen bir yapı koyar. Aynı hata OnDisable'ın CancelPlacement
        // çağırma gerekçesinde adıyla yazılı; kapatılan yol oydu, bu ikincisi.
        //
        // ERKEN ÇIKIŞIN YÖNÜ ÖLÇÜYLE SEÇİLDİ: kip KALICI bir durumdur, oyuncu
        // ona bir tuşa basarak girer ve bir tuşa basarak çıkar; sürükleme ise
        // parmak kalkınca biten geçici bir jesttir. Kalıcı olanın önizlemesini
        // geçici olana bozdurmak, oyuncunun kendi verdiği kararı görünmez kılar.
        // KAYBEDİLEN ŞEY YOK: kip açıkken hayalet zaten her karede imlecin
        // hücresine konuyor, yani sürükleyen oyuncu önizlemeyi olduğu gibi
        // görmeye devam ediyor — geri çekilen tek şey ikinci bir yazar.
        //
        // İKİ SEÇENEK REDDEDİLDİ. Birincisi "kipi bırak, titreme zararsız" idi
        // ve üstteki ölçüm onu çürüttü; zarar titreme değil sessiz körlüktür.
        // İkincisi TryEnterPlacementMode'un dinleyicinin IsPlacing üyesine
        // bakıp erken çıkmasıydı: bu, tahtanın üretim katmanını ADIYLA tanıması
        // demekti ve sözleşmenin tek yönlü olma sebebini tam o satırda
        // çökertirdi. Buradaki çözüm tahtanın YALNIZ KENDİ alanına bakıyor.
        //
        // GERİYE KALAN VE SESSİZ OLMAYAN ARTIK: kip açıkken bırakılan bir
        // sürükleme ile kipin kendi bırakışı aynı kareye düşerse ikisi de aynı
        // hücreye yazmayı dener; ikincisi PlacementOutcome.RejectedCellOccupied
        // alır ve iki taraf da bunu Console'a yazar.
        public void SetPlacementGhost(bool visible, int x, int y)
        {
            if (placementGhost == null)
            {
                return;
            }

            if (isPlacingStructure)
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
            renderer.sprite = pendingStructureSprite != null
                ? pendingStructureSprite
                : placementGhost.sprite;

            // Zemin 0, birimler ve yapılar 1: yapı zeminin üstünde çizilir.
            renderer.sortingOrder = 1;

            // YAPI BİRİMDEN BÜYÜK DURUR. Ölçek 1 olduğunda bina tam bir hücre
            // kaplıyordu ve yanındaki 1,25 ölçekli savaşçıdan KÜÇÜK görünüyordu —
            // oysa oyuncunun beklediği tam tersi. Sayı Inspector'da: tahta
            // büyüdükçe ya da sprite değiştikçe koda dönmek gerekmesin.
            // SIFIR ÖLÇEĞE KARŞI SİGORTA: [Min] yalnız Inspector YAZARKEN çalışır,
            // sahnede o anahtar hiç yoksa alan 0 doğar ve yapı GÖRÜNMEZ olur —
            // hata da vermez. Bu, ekranda hiçbir iz bırakmayan türden bir
            // kusurdur, o yüzden değeri burada da savunuyoruz.
            float scale = structureScale > 0.01f ? structureScale : 1.6f;
            structureObject.transform.localScale = new Vector3(scale, scale, 1f);

            // TABLOYA YAZMA EN SONDA: yukarıdaki kurulum satırlarından biri
            // patlarsa tabloda yarım kurulmuş bir görsele ok kalmamalı. Aynı
            // sıra Battle.AddUnit'in aboneliği en sona koymasıyla aynı sebepten.
            structureViews.Add(structureUnit, structureObject);
            AttachHealthBar(structureUnit, structureObject.transform, sortingOrder: 3);
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
        // "OUTSIDE the board" diye yazıyor ve UpdatePlacement hayaleti tahta
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
            return battle.IsInsideGrid(x, y) && !battle.TryGetUnit(x, y, out Unit _);
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

            // Can barları temizlikten ÖNCE tazeleniyor: sırası ters olsaydı
            // ölen bir birimin barı, görseli silinmeden önceki son karede eski
            // değerinde donmuş kalırdı.
            RefreshHealthBars();
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
        /// Sıradaki yerleştirmenin hangi binaya ait olduğunu tahtaya söyler.
        ///
        /// OYUNDA NE İŞE YARAR: oyuncu paletten bir bina sürüklerken imlecin
        /// altında O binayı görür ve bıraktığında aynısı kurulur. Bu çağrı
        /// olmasaydı her bina, hayalete atanmış tek sprite olarak çizilirdi.
        /// </summary>
        // TAHTA SEÇİMİ BİLMEZ, KENDİSİNE SÖYLENİR: hangi binanın seçili olduğunu
        // palet bilir ve palet bu dosyayı tanımaz. Bilgi bu yüzden yukarıdan
        // AŞAĞI akıyor; tahta yukarı doğru soru sormuyor.
        public void SetPlacementVisual(Sprite sprite)
        {
            pendingStructureSprite = sprite;

            if (placementGhost != null && sprite != null)
            {
                placementGhost.sprite = sprite;
            }
        }

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

            for (int x = 0; x < battle.Width; x++)
            {
                for (int y = 0; y < battle.Height; y++)
                {
                    CreateCellVisual(x, y);
                }
            }

            Debug.Log($"[Board] built {battle.Width}x{battle.Height} = {battle.CellCount} cells.", this);
        }

        private void CreateCellVisual(int x, int y)
        {
            var cell = new GameObject($"Cell_{x}_{y}");

            // Çıplak "transform" = this.transform; ebeveyn-çocuk hiyerarşisi
            // GameObject'te değil Transform'da yaşar. Amaç konum değil TOPLU
            // YAŞAM DÖNGÜSÜ: tahtayı yok etmek tek çağrıyla 15 hücreye uygulanır.
            // → BoardAdapter.md#createcellvisualint-x-int-y
            cell.transform.SetParent(transform, worldPositionStays: false);

            // Hücrenin MERKEZİ, CellToWorld değil: köşe kullanılsaydı her hücre
            // yarım kare kaymış görünürdü.
            cell.transform.position = CellCentre(x, y);

            // AddComponent bir MUTASYONdur: her çağrı yeni bir bileşen ekler.
            // GetComponent'in aksine idempotent değildir, bu yüzden kurulum
            // kodunda yaşar; kare başına çalışan bir yere konulamaz.
            var renderer = cell.AddComponent<SpriteRenderer>();

            // SpriteRenderer ÇİZER; sprite ise çizilecek varlıktır. Çizen ile
            // çizilen ayrı şeylerdir.
            renderer.sprite = PickTerrainSprite(x, y);

            // Çizim önceliği: aynı katmanda büyük değer üste çizilir. Zemin 0;
            // üzerine gelen birimler 1 alır ve zeminin üstünde görünür.
            renderer.sortingOrder = 0;
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
        public bool PlaceUnit(Unit identity, Combatant combatant, int x, int y)
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

            unitViews.Add(identity, view);
            AttachHealthBar(identity, view.transform, sortingOrder: 3);
            return true;
        }

        /// <summary>
        /// Bir görselin başının üstüne can barı takar.
        /// </summary>
        // ÇOCUK NESNE, BİLEŞEN DEĞİL: bar birimin KENDİ ölçeğinden etkilenmemeli.
        // Savaşçı 1,25, yapı 1,6 ölçekle çiziliyor; bar aynı nesnenin üstünde
        // yaşasaydı her birimde farklı boyda görünürdü. Ayrı bir çocuk, ebeveynin
        // ölçeğini ters çevirerek bunu tek yerde çözüyor.
        private void AttachHealthBar(Unit identity, Transform parent, int sortingOrder)
        {
            if (healthBarSprite == null)
            {
                Debug.LogError(
                    "[Board] healthBarSprite is not assigned; no health bars will be drawn. " +
                    "Assign the plain white square sprite on the Board component.",
                    this);
                return;
            }

            // HAVUZDAN GELEN GÖRSELDE BAR ZATEN VARDIR. Yeniden kurulsaydı her
            // yeniden kullanımda bir bar daha eklenir ve barlar üst üste birikirdi
            // — havuz kullanan kodların ikinci klasik hatası.
            HealthBarView existing = parent.GetComponentInChildren<HealthBarView>(includeInactive: true);
            if (existing != null)
            {
                existing.SetFraction(1f);
                healthBars[identity] = existing;
                return;
            }

            var go = new GameObject("HealthBar");
            go.transform.SetParent(parent, worldPositionStays: false);

            Vector3 parentScale = parent.localScale;
            go.transform.localScale = new Vector3(
                parentScale.x > 0.0001f ? 1f / parentScale.x : 1f,
                parentScale.y > 0.0001f ? 1f / parentScale.y : 1f,
                1f);

            var bar = go.AddComponent<HealthBarView>();
            bar.Build(healthBarSprite, sortingOrder);

            healthBars[identity] = bar;
        }

        /// <summary>
        /// İmleç çerçevesini bir kez kurar.
        /// </summary>
        private void BuildHoverHighlight()
        {
            if (hoverFrameSprite == null)
            {
                // SESSİZ DEĞİL AMA ÖLÜMCÜL DE DEĞİL: çerçevesiz oyun oynanır,
                // yalnız geri bildirim zayıflar. Bu yüzden LogWarning.
                Debug.LogWarning(
                    "[Board] hoverFrameSprite is not assigned; the cursor highlight is off.",
                    this);
                return;
            }

            var go = new GameObject("HoverHighlight");
            go.transform.SetParent(transform, worldPositionStays: false);

            hoverHighlight = go.AddComponent<SpriteRenderer>();
            hoverHighlight.sprite = hoverFrameSprite;

            // Zemin 0, birim/yapı 1, can barı 3. Çerçeve 2: zeminin ve birimin
            // üstünde görünsün ama can barını kapatmasın.
            hoverHighlight.sortingOrder = 2;
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
        private void UpdateHoverHighlight()
        {
            if (hoverHighlight == null)
            {
                return;
            }

            if (isPlacingStructure || !TryReadPointerCell(out _, out _, out int x, out int y)
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

            bool reachable = battle.TryGetPosition(selectedUnit, out int fromX, out int fromY)
                             && battle.TryFindPath(selectedUnit, fromX, fromY, x, y, out List<GridStep> _);

            hoverHighlight.color = reachable
                ? new Color(0.35f, 0.9f, 0.4f, 0.9f)
                : new Color(0.5f, 0.5f, 0.5f, 0.4f);
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
        /// Bir kimliğin hangi tarafa ait olduğunu söyler.
        /// </summary>
        /// <returns>Kimlik bu savaşta tanınmıyorsa false.</returns>
        // İKİ DEFTER, TEK SORU: takım bilgisi savaşçıda da yapıda da var ama
        // ayrı tablolarda duruyor. Çağıranın hangi tabloya bakacağını bilmesi
        // gerekmesin diye soru burada birleştiriliyor — DespawnView'ın iki
        // tabloyu birleştirmesiyle aynı desen ve aynı gerekçe.
        private bool TryGetTeam(Unit unit, out Team team)
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
                new AttackProfile(damage, attackRange),
                team);
        }

        /// <summary>
        /// Bir tıklamayı hücreye çevirir ve niyete göre dallandırır.
        /// </summary>
        private void HandleClick()
        {
            // AKIŞ DEĞİŞMEDİ, ÇEVİRİ TEK SAHİBE İNDİ: ilk üç adım artık
            // TryReadPointerCell'in içinde ve dallanmanın kendisi satır satır
            // aynı kaldı. Dünya koordinatları burada KULLANILMIYOR — bu akışın
            // ihtiyacı olan tek şey hücre indeksi.
            // → BoardAdapter.md#handleclick
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
                HandleOccupiedCellClick(clicked, x, y);
                return;
            }

            HandleEmptyCellClick(x, y);
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
                Debug.Log($"[Board] ({x},{y}) holds '{clicked.Name}' - SELECTED.", this);
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

            // BASILI TUŞ, TIKLAMANIN ANLAMINI DEĞİŞTİRİR — ve sorusu saldırıdan
            // ÖNCE geliyor. Altına konsaydı düşmüş dostuna tıklayan oyuncunun
            // eli tuşta olsa bile önce bir saldırı denemesi geçerdi; o deneme
            // TargetingRules tarafından reddedilir ve Console'a diriltmeyle
            // hiçbir ilgisi olmayan bir satır yazılırdı.
            if (Input.GetKey(reviveModifierKey))
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
                Debug.Log($"[Board] ({x},{y}) holds friendly '{clicked.Name}' - SELECTION MOVED.", this);
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

            // Mesafe BURADA hesaplanmıyor ve hesaplatılmıyor bile: BattleActions
            // konumları Battle'dan bulup GridDistance'a ölçtürüyor. Bu satırın
            // bildiği tek şey "kim kime".
            AttackOutcome outcome = BattleActions.Attack(battle, selectedUnit, clicked);
            ReactToAttack(outcome, clicked, x, y);
        }

        /// <summary>
        /// Seçili savaşçı hedefe vuramayacak kadar uzaktaysa, ona komşu bir
        /// hücreye yürütür.
        /// </summary>
        /// <returns>
        /// Saldırı denemesi YAPILABİLİR ise true. Yaklaşma başarısızsa false —
        /// o zaman bu tur yalnızca yürüme (ya da hiçbir şey) olur.
        /// </returns>
        // NEDEN AYRI BİR ADIM: hareket ile saldırı iki ayrı eylem ve ikisinin de
        // kendi kuralları var (sıra, durum, menzil). Bunları tek bir "saldır"
        // çağrısının içine gömmek, hareket reddedildiğinde saldırının neden
        // olmadığını söyleyemez hâle getirirdi.
        //
        // SIRA KURALI BİR KEZ HARCANIR: BattleActions.Move başarılı olduğunda
        // sırayı devrediyor, dolayısıyla peşinden gelen saldırı reddedilirdi.
        // Bu yüzden yaklaşma başarılıysa BURADA duruyoruz ve vuruş bir sonraki
        // tıklamaya kalıyor — oyuncuya da bunu söylüyoruz.
        private bool TryCloseInOn(Unit target, int targetX, int targetY)
        {
            if (!battle.TryGetPosition(selectedUnit, out int fromX, out int fromY))
            {
                return true;
            }

            // Zaten menzildeyse yürümeye gerek yok; doğrudan saldırıya geç.
            if (GridDistance.Between(fromX, fromY, targetX, targetY) <= attackRange)
            {
                return true;
            }

            if (!TryFindApproachCell(fromX, fromY, targetX, targetY, out int stepX, out int stepY))
            {
                Debug.Log(
                    $"[Board] '{selectedUnit.Name}' cannot get close enough to '{target.Name}'; " +
                    "every cell around the target is blocked.",
                    this);
                return false;
            }

            MoveOutcome outcome =
                BattleActions.Move(battle, selectedUnit, stepX, stepY, out List<GridStep> path);

            if (outcome != MoveOutcome.Moved)
            {
                ReactToMove(outcome, selectedUnit, stepX, stepY, path);
                return false;
            }

            WalkViewAlong(selectedUnit, path, stepX, stepY);
            Debug.Log(
                $"[Board] '{selectedUnit.Name}' closed in on '{target.Name}' and is now in range. " +
                "Click the target again to strike.",
                this);

            return false;
        }

        /// <summary>
        /// Hedefe vurabilecek, boş ve ULAŞILABİLİR hücrelerin en yakınını bulur.
        /// </summary>
        // ADAYLAR MENZİLE GÖRE ÜRETİLİYOR, KOMŞULUĞA GÖRE DEĞİL: attackRange 2
        // olan bir birim hedefin dibine kadar gitmemeli, vurabildiği yerde
        // durmalı. Aday listesi hedefin çevresindeki kareyi tarar ve her adayı
        // ÜÇ süzgeçten geçirir: menzilde mi, boş mu, yolu var mı.
        private bool TryFindApproachCell(
            int fromX, int fromY, int targetX, int targetY, out int bestX, out int bestY)
        {
            bestX = 0;
            bestY = 0;

            int bestSteps = int.MaxValue;

            for (int dy = -attackRange; dy <= attackRange; dy++)
            {
                for (int dx = -attackRange; dx <= attackRange; dx++)
                {
                    int candidateX = targetX + dx;
                    int candidateY = targetY + dy;

                    if (dx == 0 && dy == 0)
                    {
                        continue;
                    }

                    if (GridDistance.Between(candidateX, candidateY, targetX, targetY) > attackRange)
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

            ReviveOutcome outcome = BattleActions.Revive(battle, selectedUnit, target);
            ReactToRevive(outcome, target, x, y);
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
        private void ReactToAttack(AttackOutcome outcome, Unit target, int x, int y)
        {
            // VURUŞ EKRANDA GÖRÜNÜR — ve yalnızca GERÇEKTEN vurulduğunda.
            // Reddedilen bir saldırıda da hamle oynatılsaydı oyuncu isabet ile
            // reti ayırt edemezdi; gösterim o zaman bilgi taşımaz, gürültü olurdu.
            if (outcome == AttackOutcome.Hit
                || outcome == AttackOutcome.HitAndDowned
                || outcome == AttackOutcome.HitAndDestroyed)
            {
                PlayAttackVisual(selectedUnit, x, y);
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

                // İKİZİN ÖTEKİ YARISI, ve dalın var olması bir tercih değil bir
                // ONARIM: bu değer enum'da baştan beri duruyordu ama burada
                // KARŞILIĞI YOKTU, yani yıkılan her yapı aşağıdaki default'a
                // düşüp "Unhandled attack outcome" diye bir PROGRAMCI hatası
                // basıyordu. Bugüne kadar görünmemesinin sebebi tahtaya hiç yapı
                // konamamasıydı; o yol onarıldı ve dalın yokluğu erişilebilir
                // oldu. Sigortanın bir oyun sonucuna harcanması, sigortayı yok
                // etmekle aynı şeydir.
                //
                // BU SATIR BİR DUYURU DEĞİL, TEK DUYURU: birimler düştüğünde
                // ekranı Battle.UnitStateChanged tazeliyor, ama yapılar o olaya
                // KATILMIYOR — StructureLifecycle bilerek olaysız, çünkü tek
                // geçişini yapan çağrı cevabı zaten dönüş değeriyle alıyor. O
                // çağrının cevabının ulaştığı yer tam olarak burası.
                //
                // GÖRSELE DOKUNULMUYOR ve iki ayrı sebebi var: yapı görseli bir
                // UnitView taşımıyor, yani söylenecek bir durum yok; enkazın
                // sahneden kalkması ise AdvanceBattleTime'ın süpürmesinin işi ve
                // enkaz süresi dolmadan kalkmamalı.
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
                    Debug.Log($"[Board] '{selectedUnit.Name}' cannot act right now; the attack was rejected. {DescribeCondition(selectedUnit)}", this);
                    break;

                // default LOG DEĞİL LogError: buraya düşmek "AttackOutcome'a yeni
                // bir değer eklendi ve bu switch güncellenmedi" demektir, yani
                // bir programcı hatasıdır. Bir switch DEYİMİ için derleyici
                // uyarmaz; görünürlüğü bu dal sağlıyor.
                default:
                    Debug.LogError($"[Board] Unhandled attack outcome: {outcome}.", this);
                    break;
            }
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
        private void ReactToRevive(ReviveOutcome outcome, Unit target, int x, int y)
        {
            switch (outcome)
            {
                case ReviveOutcome.Revived:
                    Debug.Log($"[Board] '{target.Name}' at ({x},{y}) was REVIVED. {DescribeCondition(target)}", this);
                    break;

                case ReviveOutcome.RejectedOutOfRange:
                    Debug.Log($"[Board] '{target.Name}' at ({x},{y}) is too far to be revived.", this);
                    break;

                case ReviveOutcome.RejectedInvalidTarget:
                    Debug.Log($"[Board] '{target.Name}' at ({x},{y}) cannot be revived. {DescribeCondition(target)}", this);
                    break;

                // MESAJ HEDEFİ DEĞİL DİRİLTENİ ANLATIYOR, ReactToAttack'teki
                // ikiziyle aynı sebeple: değerin adındaki "Actor" sözcüğünün
                // karşılığı budur.
                case ReviveOutcome.RejectedActorCannotAct:
                    Debug.Log($"[Board] '{selectedUnit.Name}' cannot act right now; the revive was rejected. {DescribeCondition(selectedUnit)}", this);
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
            // SEÇİM ÖNCE BIRAKILIR, ama ClearSelection ile DEĞİL: birazdan yok
            // edilecek bir nesneye çerçeve kapatmak anlamsızdır, ve sıra ters
            // olsaydı SetSelectionVisual görseli bulamayıp LogError yazardı.
            // → BoardAdapter.md#despawnviewunit-unit
            if (ReferenceEquals(unit, selectedUnit))
            {
                selectedUnit = null;
            }

            // Bar, sahibinin ÇOCUĞU olduğu için sahneden kendiliğinden gidiyor;
            // silinmesi gereken tek şey tabloda kalan ok. Bırakılsaydı tablo
            // savaş boyunca büyür ve RefreshHealthBars ölü kayıtları gezerdi.
            healthBars.Remove(unit);

            // YAPI ÖNCE SORULUYOR ve sıra burada GÖZLENEMEZ: bir kimlik aynı
            // anda hem savaşçı hem yapı OLAMAZ, o kelepçe ThrowIfCannotJoin'de
            // duruyor. Sorunun ayrı bir dal olmasının sebebi sıra değil TİP —
            // yapı görselinde bir UnitView yok, dolayısıyla TryGetView onu
            // aramaya bile gidemez.
            if (structureViews.TryGetValue(unit, out GameObject structureObject))
            {
                // Önce tablodan çıkar, sonra sahneden sil — gerekçe aşağıda,
                // birim dalında bir kez yazılı ve burada TEKRAR EDİLMİYOR.
                structureViews.Remove(unit);
                Destroy(structureObject);

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
        private void SetSelectionVisual(Unit unit, bool isSelected)
        {
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
