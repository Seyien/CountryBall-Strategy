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
    // Unity  : zorunlu — ölçüsü şu: Assets/Tests/EditMode/Unity/ klasörüne
    //          bak; UnitViewTests VAR, BoardAdapterTests YOK. Input, Camera,
    //          Time ve Instantiate kullandığı için new ile kurulamaz
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
    public sealed class BoardAdapter : MonoBehaviour
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

        // HAREKET MENZİLİNİN SAHİBİ: bugün BU ALAN, yani Unity katmanı — ve bu
        // bir karar değil, adı konmuş bir BORÇ. Kuralın sahibi MoveProfile
        // (Core), yalnız SAYI hâlâ burada. Menzili Unit başına bir
        // Dictionary<Unit, int> içinde tutmak REDDEDİLDİ: aynı nesneyle
        // anahtarlanan ÜÇÜNCÜ tablo, senkronunu hiçbir tipin garanti etmediği
        // üçüncü bir doğruluk kaynağı olurdu. → BoardAdapter.md#moverange
        [Tooltip("How many cells a unit can travel in one move. 0 means rooted.")]
        [SerializeField, Min(0)] private int moveRange = 1;

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

        // Hayalet fareye YAPIŞTI mı. İki giriş şeklini ayıran tek alan budur:
        // sürükle-bırak hiç yapıştırmaz, tıkla-bırak ilk bırakışta yapıştırır.
        // Sayaç değil bool, çünkü ayrım "kaçıncı tıklama" değil — hayalet fareye
        // bağlı mı bağlı değil mi. → BoardAdapter.md#ghostiscarried
        private bool ghostIsCarried;

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

            BuildCellVisuals();

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
            PlacementOutcome outcome =
                BattleActions.PlaceStructure(battle, placer, NewStructure(placer), x, y);

            // KİPTEN ÇIKIŞ, sonuçtan BAĞIMSIZ: ret de bir cevaptır ve ret sebebi
            // çoğu zaman hücre değil BİRİMdir; düzeltmenin yolu kipe yeniden
            // girmektir.
            CancelPlacement();

            // TEK BİR DEĞERLE KARŞILAŞTIRMA, TAM SWITCH DEĞİL — ve bu bilinçli
            // bir eksikliktir: buradaki tek soru "kondu mu" ve bu dosyanın ret
            // sebebine göre yapacağı FARKLI bir işi yok. EŞİK: bir ret sebebi
            // ekranda farklı bir şey yaptırdığı gün tam switch'e çevrilir.
            if (outcome == PlacementOutcome.Placed)
            {
                CreateStructureVisual(x, y);
            }

            Debug.Log($"[Board] '{placer.Name}' placement at ({x},{y}) -> {outcome}.", this);
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
        // okunuyor. GÖRSEL BİR TABLOYA KAYDEDİLMİYOR: bugün onu tekrar bulması
        // gereken hiçbir çağıran yok.
        // → BoardAdapter.md#createstructurevisualint-x-int-y
        private void CreateStructureVisual(int x, int y)
        {
            var structureObject = new GameObject($"Structure_{x}_{y}");
            structureObject.transform.SetParent(transform, worldPositionStays: false);
            structureObject.transform.position = CellCentre(x, y);

            var renderer = structureObject.AddComponent<SpriteRenderer>();
            renderer.sprite = placementGhost.sprite;

            // Zemin 0, birimler ve yapılar 1: yapı zeminin üstünde çizilir.
            renderer.sortingOrder = 1;
        }

        /// <summary>
        /// Fare konumunu dünya koordinatına ve hücre indeksine çevirir.
        /// Bir Unity tipinin Core'un diline çevrildiği TEK yer burasıdır.
        /// </summary>
        // İKİ ÇAĞIRAN, TEK ÇEVİRİ (tıklama akışı ve yerleştirme kipi):
        // kopyalansaydı fare ile hayalet farklı hücreleri gösterirdi ve hiçbir
        // şey patlamazdı. DÖNÜŞ bool + out, nullable DEĞİL — "kamera yok" bir
        // programcı hatasıdır. Camera.main "MainCamera" ETİKETLİ kamerayı bulur.
        // ScreenToWorldPoint çevirisi olmasaydı aynı tıklama 1920'lik ve
        // 2560'lık ekranda farklı hücreyi seçerdi.
        // → BoardAdapter.md#tryreadpointercell
        private bool TryReadPointerCell(out float worldX, out float worldY, out int x, out int y)
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

            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldX = worldPoint.x;
            worldY = worldPoint.y;

            // Vector3Int sınırın ötesine geçmez; "tahta içinde mi" sorusunu
            // soran taraf yine Battle'dır, bu metot değil.
            Vector3Int cell = unityGrid.WorldToCell(worldPoint);
            x = cell.x;
            y = cell.y;
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

            if (battle.RemoveReadyForCleanup(cleanupBuffer) == 0)
            {
                return;
            }

            for (int i = 0; i < cleanupBuffer.Count; i++)
            {
                DespawnView(cleanupBuffer[i]);
            }
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
            var unit = new Unit(name);

            // Önce KURAL, sonra görsel. AddUnit dolu hücreyi ve tahta dışını
            // exception ile reddeder; o hata görsel doğmadan patlasın ki ekranda
            // karşılığı olmayan bir birim asla oluşmasın.
            //
            // "Tahtada var" ile "savaşta var" İKİ ayrı olgudur ve ayrışmamaları
            // ikisini AYNI çağrıda doğuran tek kapıya bağlı. Yan kapı
            // (board.PlaceUnit) tahtada duran ama Combatant'ı OLMAYAN bir birim
            // üretirdi; bugün açılamaması bir yasak değil bir YOKLUK.
            // → BoardAdapter.md#spawnunitstring-name-team-team-int-x-int-y
            battle.AddUnit(unit, NewCombatant(team), x, y);

            // Instantiate prefab'dan YENİ bir kopya doğurur; ikinci parametre
            // ebeveyni verir, böylece tahta yok olunca birimler de gider.
            // Argüman UnitView olduğu için dönüş de UnitView'dır — bu yüzden
            // burada tek bir GetComponent yok.
            UnitView view = Instantiate(unitPrefab, transform);
            view.name = $"Unit_{unit.Name}_{x}_{y}";
            view.transform.position = CellCentre(x, y);

            unitViews.Add(unit, view);
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

            // Mesafe BURADA hesaplanmıyor ve hesaplatılmıyor bile: BattleActions
            // konumları Battle'dan bulup GridDistance'a ölçtürüyor. Bu satırın
            // bildiği tek şey "kim kime".
            AttackOutcome outcome = BattleActions.Attack(battle, selectedUnit, clicked);
            ReactToAttack(outcome, clicked, x, y);
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

            MoveOutcome outcome = BattleActions.Move(battle, selectedUnit, x, y, moveRange);
            ReactToMove(outcome, selectedUnit, x, y);
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
        private void ReactToMove(MoveOutcome outcome, Unit unit, int x, int y)
        {
            switch (outcome)
            {
                case MoveOutcome.Moved:
                    // Görsel tahtayı TAKİP eder, tahtaya yön vermez: hareketi
                    // MoveAction çoktan yaptı. Ters sırada yazılsaydı reddedilen
                    // bir hareket ekranda gerçekleşmiş görünürdü.
                    MoveViewTo(unit, x, y);
                    Debug.Log($"[Board] '{unit.Name}' moved to ({x},{y}).", this);
                    break;

                case MoveOutcome.RejectedOutOfRange:
                    Debug.Log($"[Board] ({x},{y}) is further than {moveRange} cell(s) for '{unit.Name}'.", this);
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
        /// Görseli hücrenin merkezine taşır.
        /// </summary>
        private void MoveViewTo(Unit unit, int x, int y)
        {
            if (!TryGetView(unit, out UnitView view))
            {
                return;
            }

            view.transform.position = CellCentre(x, y);
        }

        /// <summary>
        /// Savaştan çıkarılmış bir birimin görselini sahneden siler.
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

            if (!TryGetView(unit, out UnitView view))
            {
                return;
            }

            // Önce tablodan çıkar, sonra sahneden sil. Ters sırada tabloda YOK
            // EDİLMİŞ bir görsel referansı kalırdı ve Unity'nin aşırı yüklenmiş
            // eşitliği yüzünden "null gibi ama null değil" hâlde dolaşırdı.
            unitViews.Remove(unit);
            // ÖDÜNÇ ALINAN — `Destroy`: yalnız motor tarafındaki eşi yıkım sırasına
            // sokar, yönetilen C# nesnesini geri VERMEZ; `System.Object` ile
            // `UnityEngine.Object` ayrımı tam bu satırda görünür.
            // DİL: Docs/deep/dil/07-bellek-canlilik-ve-yikim.md
            Destroy(view.gameObject);

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
