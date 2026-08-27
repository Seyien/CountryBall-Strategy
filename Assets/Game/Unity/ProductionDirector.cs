using System;
using System.Collections.Generic;
using GridStrategy.Battle;
using GridStrategy.Combat;
using GridStrategy.Core;
using UnityEngine;

namespace GridStrategy.Unity
{
    // ═══ ROL: YÖNETEN (Coordinator) ══════════════════════════════════
    // kimlik : var ama SAHNE kimliği — sahnede tek bir tane olması beklenir;
    //          oyun kimliği yok, tahtaya hiç çıkmaz
    // hafıza : var — ölçüsü şu: aynı DropAt çağrısı önce bir yapı koyar,
    //          sonra hiçbir şey yapmaz; farkı doğuran şey iki çağrı arasında
    //          tutulan "elimde ne var" durumu (pendingStructure ve
    //          pendingUnit) ve yerleşmiş yapıların defteri
    // Unity  : zorunlu — Update'te Time.deltaTime okuyor; bu tipin
    //          çekirdeğe konamamasının sebebi tam olarak o satır
    // karar  : VERMEZ, SORAR ve UYGULAR — üretim izni ProductionRules'un,
    //          yerleştirme izni tahtanın; buranın işi sırayı doğru kurmak
    /// <summary>
    /// Oyuncunun paletten sürüklediği şeyi tahtaya indiren ve kurulmuş her
    /// binanın üretim sayacını işleten yer.
    ///
    /// EN BASİT HÂLİYLE: bunu bir <b>şantiye şefi</b> gibi düşün.
    ///
    /// Oyuncu soldaki paletten bir bina tutup tahtaya sürüklüyor. Bu sırada üç
    /// ayrı taraf iş yapıyor ve hiçbiri ötekinin işini bilmiyor:
    /// <list type="number">
    /// <item>PALET — düğmeyi çizer, parmağın nereye gittiğini bildirir.
    /// Tahtanın var olduğundan haberi bile yoktur.</item>
    /// <item>TAHTA — hücreleri bilir, "burası boş mu" sorusuna cevap verir.
    /// Paletin var olduğundan haberi yoktur.</item>
    /// <item>ŞANTİYE ŞEFİ (burası) — ikisini konuşturur: paletten "elimde şu
    /// bina var" bilgisini alır, tahtaya "burası uygun mu" diye sorar,
    /// uygunsa "koy" der.</item>
    /// </list>
    ///
    /// BU TİP OLMASAYDI NE OLURDU: paletin doğrudan tahtayı çağırması
    /// gerekirdi. O zaman her yeni panel (üretim paneli, ileride bir teknoloji
    /// ağacı) tahtanın nasıl çalıştığını baştan öğrenmek zorunda kalır ve aynı
    /// "önce hücreyi sor, sonra koy, sonra sayacı başlat" sırası her panelde
    /// yeniden yazılırdı. Sıranın bir yerde yanlış yazılması ise sessiz bir hata
    /// olurdu: bina konmadan sayaç başlar, oyuncu neden beklediğini anlamazdı.
    ///
    /// İKİNCİ İŞİ — ÜRETİM SAATİ: kurulmuş her bina "3 saniyede bir asker"
    /// gibi bir hızda üretim yapıyor. O saniyeleri sayan yer burası
    /// (<c>Update</c> içinde <c>Time.deltaTime</c>). Sayaç savaş çekirdeğine
    /// konamazdı, çünkü çekirdek motoru hiç görmüyor.
    ///
    /// TAHTAYI ADIYLA TANIMAZ: teması yalnızca <see cref="IPlacementBoard"/>
    /// üzerinden — yani kısa bir sipariş formu üzerinden. Gerekçesi o dosyada.
    ///
    /// Neyi BİLMEZ: bir düğmenin nasıl çizildiğini, panellerin kaç satır
    /// olduğunu, simgelerin nereden geldiğini. Üçü de görünüm katmanının işi.
    /// </summary>
    public sealed class ProductionDirector : MonoBehaviour
    {
        // Oyundaki tahta. Bu alan boş kalırsa oyuncu hiçbir bina kuramaz, hiçbir
        // birim üretemez — panel açılır ama sürüklenen şey hiçbir yere düşmez.
        // Sahneden sürüklenir: Hierarchy'deki Board nesnesinin BoardAdapter'ı.
        //
        // TİP NEDEN MonoBehaviour: Inspector bir ARAYÜZ alanına nesne
        // sürükletmez, o yüzden sürüklenen şey somut bileşen olarak alınır.
        // Gerçek sözleşme aşağıdaki `board` alanı; ikisini Awake bağlıyor.
        [Header("Board - drag the component that implements IPlacementBoard")]
        [Tooltip("Any MonoBehaviour that implements IPlacementBoard. Validated loudly on Awake.")]
        [SerializeField] private MonoBehaviour boardBehaviour;

        // Yukarıdaki alanın ARAYÜZ hâli. Awake içinde `boardBehaviour as
        // IPlacementBoard` ile bir kez çözülür ve bu tip tahtayla yalnızca
        // buradan konuşur.
        //
        // TUZAK: yanlış bileşen sürüklenirse derleyici susar, cast null verir ve
        // hatayı ancak Awake'teki LogError yakalar.
        private IPlacementBoard board;

        // YERLEŞMİŞ YAPILARIN DEFTERİ. Anahtar Unit çünkü seçim OLAYININ
        // taşıdığı şey odur — anahtarı doğduğu yerde tutmak, her seçimde ikinci
        // bir tabloya uğramayı engelliyor.
        // AYRI BİR "ProductionLedger" TİPİ REDDEDİLDİ: bu sözlüğü dolaşan tek
        // yer aşağıdaki Update ve tek okuyanı aşağıdaki seçim işleyicisi. İki
        // çağıranı olan bir tablo, kendi tipini hak etmiyor.
        // TETİKLEYİCİ: bu defteri motor karesi DIŞINDA bir yerden dolaşmak
        // gerektiği gün — o gün tablo çekirdeğe iner ve döngü onunla birlikte.
        private readonly Dictionary<Unit, StructureProduction> productions =
            new Dictionary<Unit, StructureProduction>();

        // ELDE TUTULAN TANIM. İkisinden en fazla biri dolu olur ve ikisi de
        // boşken sürükleme yoktur. Tek bir "object payload" alanı reddedildi:
        // bırakma anında tipi geri sormak gerekirdi ve o soru, derleyicinin
        // burada bedavaya verdiği ayrımı çalışma zamanına ertelerdi.
        private StructureBlueprint pendingStructure;
        private UnitBlueprint pendingUnit;

        private Team pendingTeam;

        // Üretimi yapacak hat, sürükleme BAŞLARKEN yakalanıyor. Seçim
        // sürüklemenin ortasında değişirse bırakma yine DOĞRU yapıya sorulur;
        // bırakma anında seçime bakılsaydı, birim bambaşka bir barakanın
        // sayacını yakabilirdi.
        private StructureProduction pendingProducer;

        private Unit selectedUnit;
        private StructureProduction selectedProduction;

        /// <summary>
        /// Seçili yapının üretim hattı değiştiğinde haber verir; seçili şey bir
        /// üretici değilse <c>null</c> ile.
        /// </summary>
        public event Action<StructureProduction> SelectedProductionChanged;

        /// <summary>Seçili yapının üretim hattı; yoksa <c>null</c>.</summary>
        public StructureProduction SelectedProduction => selectedProduction;

        /// <summary>Elde bir tanım tutuluyor mu.</summary>
        public bool IsPlacing => pendingStructure != null || pendingUnit != null;

        private void Awake()
        {
            board = boardBehaviour as IPlacementBoard;

            // Eksik ya da YANLIŞ TİPTE atama SESSİZ kalmasın: alan boşsa hiçbir
            // şey yerleşmez ve ekranda tek bir hata görünmez. Aynı gerekçe
            // UnitView'in Awake üyesinde ölçülerek yazılı. İki ayrı yanlış, iki
            // ayrı cümle — çünkü "atamayı unuttum" ile "yanlış nesneyi
            // sürükledim" farklı düzeltmeler ister.
            if (boardBehaviour == null)
            {
                Debug.LogError(
                    "[ProductionDirector] boardBehaviour is not assigned. " +
                    "Drag the board component (the one implementing IPlacementBoard) onto this field.",
                    this);
                return;
            }

            if (board == null)
            {
                Debug.LogError(
                    $"[ProductionDirector] '{boardBehaviour.GetType().Name}' does not implement " +
                    "IPlacementBoard. Placement and production will do nothing.",
                    this);
            }
        }

        private void OnEnable()
        {
            if (board != null)
            {
                board.SelectionChanged += OnSelectionChanged;
                board.UnitRemoved += Forget;
            }
        }

        // ABONELİK SÖKÜLÜYOR VE BU BİR ÜSLUP DEĞİL: sökülmemiş tek bir
        // abonelikte yayıncı, bu bileşenin tamamını erişilebilir tutar. Aynı
        // gerekçe Battle'ın RemoveUnit üyesinin içinde ölçülerek yazılı ve
        // burada uygulanıyor.
        private void OnDisable()
        {
            if (board != null)
            {
                board.SelectionChanged -= OnSelectionChanged;
                board.UnitRemoved -= Forget;
            }
        }

        // ZAMANI BURADAN VERMEK ZORUNLU: StructureProduction bilerek
        // Time.deltaTime okumaz — okusaydı EditMode'da sınanamazdı. Aynı
        // sözleşmenin ikinci uygulaması; birincisi tahtanın savaş saatinde.
        private void Update()
        {
            // Sözlük üzerinde DOĞRUDAN foreach: Dictionary<,>.Enumerator bir
            // struct'tır ve burada bir arayüz ardında saklanmadığı için
            // kutulanmaz. Aynı döngü IEnumerable üzerinden dönseydi kare başına
            // bir tahsis üretirdi — gerekçe Battle'ın Tick üyesinde yazılı.
            foreach (KeyValuePair<Unit, StructureProduction> pair in productions)
            {
                pair.Value.Tick(Time.deltaTime);
            }
        }

        /// <summary>
        /// Sol panelden bir yapı türü alınır. Bırakılana kadar elde tutulur.
        /// </summary>
        /// <param name="icon">
        /// Bu binanın görseli. Tahtaya iletiliyor ki oyuncu sürüklerken imlecin
        /// altında O binayı görsün ve bıraktığında aynısı kurulsun. Görsel bir
        /// EKRAN bilgisidir; <paramref name="definition"/> içinde taşınmaz,
        /// çünkü tanım Core tarafında yaşıyor ve orada Sprite diye bir tip yok.
        /// </param>
        public void BeginStructurePlacement(StructureBlueprint definition, Team team, Sprite icon)
        {
            if (definition == null)
            {
                return;
            }

            pendingStructure = definition;
            pendingUnit = null;
            pendingProducer = null;
            pendingTeam = team;

            board?.SetPlacementVisual(icon);
        }

        /// <summary>
        /// Sağ panelden, seçili yapının ürettiği birimlerden biri alınır.
        /// </summary>
        /// <param name="producedIndex">
        /// Seçili yapının üretim listesindeki sıra. İNDİS, tanım referansı
        /// değil: sağ panel zaten o listeyi sırayla çiziyor ve indis, panelin
        /// gördüğü şey ile buranın ürettiği şeyin AYNI liste olduğunu garanti
        /// eder.
        /// </param>
        public void BeginUnitPlacement(int producedIndex)
        {
            // Seçim yoksa elde tutulacak bir şey de yok. Sessiz çıkış: bu bir
            // programcı hatası değil, oyuncunun seçimi kaldırmasıyla doğal
            // olarak oluşan bir durum.
            if (selectedProduction == null)
            {
                return;
            }

            IReadOnlyList<UnitBlueprint> produces = selectedProduction.Blueprint.Produces;
            if (producedIndex < 0 || producedIndex >= produces.Count)
            {
                Debug.LogError(
                    $"[ProductionDirector] Produced index {producedIndex} is outside the " +
                    $"list of {produces.Count} unit(s). The panel and the blueprint disagree.",
                    this);
                return;
            }

            // pendingTeam BURADA YAZILMIYOR ve yokluğu bir karardır: üretilen
            // birimin takımı ÜRETEN YAPININ takımıdır ve o bilgi zaten
            // pendingProducer'ın içinde duruyor. Buraya kopyalasaydık aynı
            // cevabın ikinci sahibi doğardı ve ele geçirilebilir bina eklendiği
            // gün ikisi sessizce ayrışırdı.
            pendingUnit = produces[producedIndex];
            pendingStructure = null;
            pendingProducer = selectedProduction;
        }

        /// <summary>
        /// Sürükleme sürerken çağrılır; tahtadaki önizlemeyi taşır.
        /// </summary>
        public void DragTo(Vector2 screenPoint)
        {
            if (board == null || !IsPlacing)
            {
                return;
            }

            if (board.TryScreenPointToCell(screenPoint, out int x, out int y))
            {
                board.SetPlacementGhost(true, x, y);
                return;
            }

            // Tahta dışına çıkan sürükleme önizlemeyi GİZLER, son geçerli
            // hücrede BIRAKMAZ. Bırakılsaydı oyuncu, parmağını tahtanın dışında
            // kaldırdığında oraya bir şey konacağını sanırdı.
            board.SetPlacementGhost(false, 0, 0);
        }

        /// <summary>
        /// Sürükleme biter: elde tutulan tanım bırakıldığı hücreye konur.
        /// </summary>
        public void DropAt(Vector2 screenPoint)
        {
            if (board == null || !IsPlacing)
            {
                CancelPlacement();
                return;
            }

            if (!board.TryScreenPointToCell(screenPoint, out int x, out int y))
            {
                // Tahta dışına bırakmak bir HATA değil, bir VAZGEÇMEdir; oyuncu
                // paneli açıp fikrini değiştirebilmeli ve bunun için bir konsol
                // satırı doğmamalı.
                CancelPlacement();
                return;
            }

            if (pendingStructure != null)
            {
                DropStructure(x, y);
            }
            else
            {
                DropUnit(x, y);
            }

            CancelPlacement();
        }

        /// <summary>
        /// Elde tutulan tanımı bırakır ve önizlemeyi kapatır.
        /// </summary>
        public void CancelPlacement()
        {
            pendingStructure = null;
            pendingUnit = null;
            pendingProducer = null;

            if (board != null)
            {
                board.SetPlacementGhost(false, 0, 0);
            }
        }

        /// <summary>
        /// Tahtadan kaldırılmış bir yapının üretim hattını defterden siler.
        /// </summary>
        // TAHTANIN UnitRemoved OLAYININ DİNLEYİCİSİ, ve ayrıca elle de
        // çağrılabilir. Her karede bütün defteri gezip "hâlâ tahtada mı" diye
        // sormak reddedildi: tahtanın zaten bildiği bir cevabı ikinci kez
        // hesaplamak olurdu ve maliyeti defterin boyuyla büyürdü.
        public void Forget(Unit identity)
        {
            if (identity == null)
            {
                return;
            }

            productions.Remove(identity);

            // Silinen şey seçiliyse seçim de düşer; düşmeseydi sağ panel yıkılmış
            // bir barakanın ürettiklerini göstermeye devam ederdi.
            if (ReferenceEquals(identity, selectedUnit))
            {
                OnSelectionChanged(null);
            }
        }

        private void DropStructure(int x, int y)
        {
            var identity = new Unit(pendingStructure.DisplayName);
            Structure structure = pendingStructure.CreateStructure(pendingTeam);

            PlacementOutcome outcome = board.PlaceStructure(identity, structure, x, y);
            if (outcome != PlacementOutcome.Placed)
            {
                // Ret bir OYUN olgusudur, bir programcı hatası değil — bu yüzden
                // LogError değil Log. Aynı ayrım UnitView'in TintFor üyesinde
                // ters yönde yazılı: orada default dalına düşmek gerçekten bir
                // programcı hatasıydı.
                Debug.Log($"[ProductionDirector] Structure placement rejected: {outcome}.", this);
                return;
            }

            // DEFTERE HER YAPI YAZILIYOR, yalnızca üretenler değil. Elektrik
            // santrali de burada duruyor ve Produces listesi boş olduğu için
            // hiçbir şey üretemiyor. Süzgeç konsaydı sağ panel, seçilen şeyin
            // "üretmeyen bir yapı" mı yoksa "hiç tanımadığım bir şey" mi olduğunu
            // ayırt edemezdi.
            productions[identity] = new StructureProduction(pendingStructure, structure);
        }

        private void DropUnit(int x, int y)
        {
            // SIRA BİR KARARDIR VE BU SIRA TERSİNE ÇEVRİLEMEZ: hücre önce
            // sorulur, üretim sonra. Tersi olsaydı dolu bir hücreye yapılan
            // başarısız bir bırakma, bekleme sayacını YAKAR ve oyuncu hiçbir
            // birim almadan beklemeye başlardı — üstelik hiçbir hata mesajı
            // görmeden.
            if (!board.IsCellFree(x, y))
            {
                Debug.Log("[ProductionDirector] Unit placement rejected: the cell is not free.", this);
                return;
            }

            ProductionOutcome outcome = pendingProducer.Produce(pendingUnit, out Combatant produced);
            if (outcome != ProductionOutcome.Allowed)
            {
                Debug.Log($"[ProductionDirector] Production rejected: {outcome}.", this);
                return;
            }

            var identity = new Unit(pendingUnit.DisplayName);
            if (!board.PlaceUnit(identity, produced, x, y))
            {
                // Buraya düşmek, yukarıdaki hücre sorusu ile bu çağrı arasında
                // tahtanın fikrini değiştirdiği anlamına gelir — yani iki üyenin
                // sözleşmesi ayrışmıştır. Sayaç bu noktada zaten başladı ve geri
                // alınmıyor: geri alma, "üretim başladı" olgusunu tersine
                // çevirebilen ikinci bir yol açardı ve o yolun sahibi yok.
                Debug.LogError(
                    "[ProductionDirector] The board accepted the cell as free but refused the unit. " +
                    "IsCellFree and PlaceUnit disagree.",
                    this);
            }
        }

        private void OnSelectionChanged(Unit identity)
        {
            selectedUnit = identity;

            StructureProduction next = null;
            if (identity != null)
            {
                productions.TryGetValue(identity, out next);
            }

            // AYNI SEÇİM İKİNCİ KEZ YAYINLANMIYOR: sağ panel her yayında
            // düğmelerini yeniden kuruyor ve tekrarlanan bir yayın, oyuncunun
            // seçtiği birimi görünür bir sebep olmadan varsayılana geri
            // döndürürdü.
            if (ReferenceEquals(next, selectedProduction))
            {
                return;
            }

            selectedProduction = next;
            SelectedProductionChanged?.Invoke(next);
        }
    }
}
