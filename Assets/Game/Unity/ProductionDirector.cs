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
    /// SİMGENİN TAŞIYICISI, ÇİZİCİSİ DEĞİL: paletten gelen yapı varlığını ve
    /// onun ürettiği birim varlıklarını defterinde tutuyor, çünkü simge yalnız
    /// varlık dosyasında yaşıyor ve çekirdek tanımının içinde taşınamıyor.
    /// Simgeyi isteyen iki taraf var — sağ panel ile sürükleme önizlemesi — ve
    /// ikisi de bu tipe soruyor.
    ///
    /// Neyi BİLMEZ: bir düğmenin nasıl çizildiğini, panellerin kaç satır
    /// olduğunu, bir simgenin ekranda kaç piksel yer kapladığını. Üçü de
    /// görünüm katmanının işi.
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

        // AYNI DEFTERİN MOTOR TARAFI. Yukarıdaki tablo çekirdek tanımını
        // tutuyor ve orada simge diye bir şey YOK; simge yalnız varlık
        // dosyasında yaşıyor. İki tablo aynı anahtarla yazılıp aynı anda
        // siliniyor, yani ayrışmaları için bir yol bırakılmadı.
        // TEK TABLOYA BİRLEŞTİRME REDDEDİLDİ: birleşik bir kayıt tipi, her
        // karede dolaşılan üretim döngüsünü motor tarafındaki bir alana bağlar
        // ve o döngünün çekirdeğe inme yolunu bugünden kapatırdı.
        private readonly Dictionary<Unit, StructureBlueprintAsset> structureAssets =
            new Dictionary<Unit, StructureBlueprintAsset>();

        // ELDE TUTULAN TANIM. İkisinden en fazla biri dolu olur ve ikisi de
        // boşken sürükleme yoktur. Tek bir "object payload" alanı reddedildi:
        // bırakma anında tipi geri sormak gerekirdi ve o soru, derleyicinin
        // burada bedavaya verdiği ayrımı çalışma zamanına ertelerdi.
        private StructureBlueprint pendingStructure;
        private UnitBlueprint pendingUnit;

        // Elde tutulan tanımın VARLIK dosyası. Yerleştirme başarılı olursa
        // deftere bu yazılıyor; sürükleme boşa çıkarsa hiçbir yere yazılmadan
        // düşüyor.
        private StructureBlueprintAsset pendingStructureAsset;

        // Elde tutulan BİRİM tanımının varlık dosyası. Yukarıdakinin ikizi ve
        // aynı sebeple var: simge yalnız varlıkta yaşıyor, tahtaya bırakma anında
        // gövde görselini verecek olan da bu alan. İndis saklamak REDDEDİLDİ —
        // seçim sürüklemenin ortasında değişirse indis başka bir yapının
        // listesine bakardı, oysa varlık referansı sürüklenen şeyin kendisidir.
        private UnitBlueprintAsset pendingUnitAsset;

        private Team pendingTeam;

        // Üretimi yapacak hat, sürükleme BAŞLARKEN yakalanıyor. Seçim
        // sürüklemenin ortasında değişirse bırakma yine DOĞRU yapıya sorulur;
        // bırakma anında seçime bakılsaydı, birim bambaşka bir barakanın
        // sayacını yakabilirdi.
        private StructureProduction pendingProducer;

        private Unit selectedUnit;
        private StructureProduction selectedProduction;

        // Seçili hattın varlık dosyası; simge sorusunun tek cevabı burada.
        // İkisi TEK yerde, aynı satırlarda güncelleniyor — ayrı yerlerden
        // yazılsalardı biri değişip öteki kalır ve panel eski yapının
        // simgelerini çizmeye devam ederdi.
        private StructureBlueprintAsset selectedProductionAsset;

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

                // ██ GERİ SAYIM YALNIZ ÜRETEN BİNANIN TEPESİNDE ██
                // Operatörün cümlesi: "saldırı yapan kulelerin illa ki bunu
                // göstermesine gerek yok, sadece savaşçı üreten yapılardan
                // bahsediyorum." Taretin ve hisarın `Produces` listesi boş,
                // dolayısıyla soru bir takım ya da tür listesi değil, binanın
                // KENDİ tanımı üzerinden cevaplanıyor — ve cevabın sahibi
                // burası değil çekirdek. → StructureProduction.ProducesUnits
                //
                // İKİ TAKIM DA GÖSTERİYOR ve bu bir taraf tutmama kararı:
                // düşman kışlasının sayacı gizlenseydi oyuncu baskının ne zaman
                // geleceğini tahmin etmek zorunda kalırdı, oysa bilgi zaten
                // ekranda duran bir binanın üstünde.
                if (board != null && pair.Value.ProducesUnits)
                {
                    board.ShowProductionCountdown(
                        pair.Key,
                        pair.Value.RemainingSeconds,
                        pair.Value.Blueprint.ProductionSeconds);
                }
            }
        }

        /// <summary>
        /// Sol panelden bir yapı türü alınır. Bırakılana kadar elde tutulur.
        /// </summary>
        /// <param name="asset">
        /// Yapının VARLIK dosyası — tanım değil. Ayrımın bedeli ölçüldü: tanım
        /// çekirdek tarafında yaşıyor ve orada Sprite diye bir tip yok, yani
        /// simge ile tanım tanımın içinde bir arada taşınamaz. Üçüncü bir
        /// parametre olarak simgeyi ayrıca istemek ise aynı varlıktan gelen iki
        /// bilgiyi çağırana yeniden birleştirtirdi; birleşik olan şey zaten
        /// varlığın kendisi, o yüzden içeri o giriyor.
        /// </param>
        public void BeginStructurePlacement(StructureBlueprintAsset asset, Team team)
        {
            if (asset == null)
            {
                return;
            }

            pendingStructure = asset.Definition;
            pendingStructureAsset = asset;
            pendingUnit = null;
            pendingUnitAsset = null;
            pendingProducer = null;
            pendingTeam = team;

            // Tahtaya simge SÜRÜKLEME BAŞLARKEN veriliyor: oyuncu imlecin
            // altında O binayı görsün ve bıraktığında aynısı kurulsun.
            board?.SetPlacementVisual(asset.Icon, asset.BoardSizeInCells);
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
            pendingStructureAsset = null;
            pendingProducer = selectedProduction;

            // VARLIK BURADA YAKALANIYOR, bırakma anında DEĞİL: bırakma anında
            // yeniden sorulsaydı sürüklemenin ortasında değişen bir seçim,
            // bambaşka bir birimin resmini tahtaya indirirdi. Aynı gerekçe bir
            // satır yukarıda pendingProducer için ölçülmüş durumda.
            pendingUnitAsset = ProducedAsset(producedIndex);

            // BU SATIRIN YOKLUĞU GÖRÜLEBİLİR BİR KUSURDU: tahta simgeyi kendi
            // sormuyor, kendisine söyleniyor — ve burada kimse söylemeyince
            // imlecin altında en son sürüklenen YAPININ resmi kalıyordu. Oyuncu
            // bir asker sürükleyip bir baraka bırakıyor gibi görünüyordu.
            board?.SetPlacementVisual(
                ProducedIcon(producedIndex), ProducedSizeInCells(producedIndex));
        }

        /// <summary>
        /// Seçili yapının üretim listesindeki bir birimin simgesi; seçim yoksa
        /// ya da indis listenin dışındaysa <c>null</c>.
        /// </summary>
        /// <remarks>
        /// SESSİZ <c>null</c> BURADA DOĞRU, çünkü bu üye bir KARAR değil bir
        /// GÖRÜNTÜ veriyor: eksik simge boş bir düğme kutusu demektir, oynanamaz
        /// bir oyun değil. Aynı indisin gerçekten geçersiz olduğu durum
        /// <see cref="BeginUnitPlacement"/> içinde zaten konsola düşüyor ve o
        /// gürültünün ikinci bir sahibi olmamalı.
        /// </remarks>
        public Sprite ProducedIcon(int producedIndex)
        {
            UnitBlueprintAsset asset = ProducedAsset(producedIndex);
            return asset == null ? null : asset.Icon;
        }

        /// <summary>
        /// Seçili yapının o indeksteki biriminin tahtada kaç hücre kaplayacağı;
        /// bilinmiyorsa sıfır, yani "tanım söylemiyor".
        /// </summary>
        // ÖLÇÜ SİMGEYLE AYNI KAPIDAN GELİYOR ve ikisi aynı varlık dosyasını
        // okuyor; ayrı bir yol açılsaydı simgeyi bulan bir indeks ölçüyü
        // bulamadığında oyuncu doğru resmi yanlış boyutta sürüklerdi.
        public float ProducedSizeInCells(int producedIndex)
        {
            UnitBlueprintAsset asset = ProducedAsset(producedIndex);
            return asset == null ? 0f : asset.BoardSizeInCells;
        }

        /// <summary>
        /// Seçili yapının üretim listesindeki bir birimin VARLIK dosyası; seçim
        /// yoksa ya da indis listenin dışındaysa <c>null</c>.
        /// </summary>
        // TEK ARAMA, İKİ SORU: simge de gövde görseli de aynı varlıktan geliyor
        // ve arama iki yerde ayrı ayrı yazılsaydı biri sınır kontrolünü kaybettiği
        // gün öteki hâlâ doğru cevap verir, hata da yalnız bir yolda görünürdü.
        private UnitBlueprintAsset ProducedAsset(int producedIndex)
        {
            if (selectedProductionAsset == null)
            {
                return null;
            }

            IReadOnlyList<UnitBlueprintAsset> assets = selectedProductionAsset.ProducedAssets;
            if (producedIndex < 0 || producedIndex >= assets.Count)
            {
                return null;
            }

            return assets[producedIndex];
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

            // ██ ÇEVRİLEN KARAR: TAHTA DIŞINDA GİZLEMEK → KIRMIZI GÖSTERMEK ██
            // Burada `TryScreenPointToCell` duruyordu ve tahtanın dışında false
            // dönüyordu; sonraki satır da hayaleti GİZLİYORDU. Gerekçesi
            // yazılıydı ve o gün doğruydu: "bırakılsaydı oyuncu, parmağını
            // tahtanın dışında kaldırdığında oraya bir şey konacağını sanırdı."
            //
            // KIRILAN ŞEY: gizlemek o yanlış anlamayı önlüyordu ama yerine
            // İKİNCİ bir belirsizlik koyuyordu — elindeki şeyin hâlâ sürüklenip
            // sürüklenmediğini de göremiyordun. Operatörün cümlesi: "o unit
            // grid'in dışındakileri de hayalet kısmını görebilmeliyiz ama
            // kırmızılı hâlinde." Kırmızı hayalet iki soruyu birden cevaplıyor:
            // sürükleme sürüyor VE buraya konmaz.
            //
            // BIRAKMA DALI DEĞİŞMEDİ ve değişmemeli: DropAt hâlâ
            // TryScreenPointToCell çağırıyor, yani tahta dışına bırakmak yine
            // bir vazgeçme. Görünen şey değişti, kural değil.
            // → Docs/deep/konular/09-kararlarin-cevrilmesi.md
            if (board.TryScreenPointToAnyCell(screenPoint, out int x, out int y))
            {
                board.SetPlacementGhost(true, x, y);
                return;
            }

            // BURAYA ANCAK KAMERA YOKSA DÜŞÜLÜR: TryScreenPointToAnyCell yalnız
            // ekran noktasını dünyaya çevirecek kamera bulunamadığında false
            // döner. O hâlde gösterilecek doğru bir hücre de yok.
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
            pendingStructureAsset = null;
            pendingUnit = null;
            pendingUnitAsset = null;
            pendingProducer = null;

            if (board != null)
            {
                board.SetPlacementGhost(false, 0, 0);

                // SİMGE DE BIRAKILIYOR ve bu satır sürükleme yolunun DIŞINDAKİ
                // yolu koruyor: tahtanın klavyeli yerleştirme kipi kurduğu
                // yapıyı en son söylenen simgeyle çiziyor. Sürüklenen birimin
                // simgesi orada asılı kalsaydı, klavyeyle konan bir bina bir
                // askerin resmiyle görünürdü. Sıra da bir karardır — yapı
                // yerleştirme bu çağrıdan ÖNCE bitiyor, yani kurulmuş bina kendi
                // resmini almış oluyor.
                board.SetPlacementVisual(null);
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

            // ELDEKİ SÜRÜKLEME DE DÜŞER, ve bu ÖLÇÜLMÜŞ bir sızıntının kapağı:
            // oyuncu barakadan bir asker sürüklerken çöp kutusuyla o barakayı
            // kaldırırsa, bırakma anında pendingProducer hâlâ yıkılmış hattı
            // gösteriyordu — sayaç yakılıyor ve tahtaya, artık var olmayan bir
            // yapının askeri iniyordu. Defterden silmek yetmiyordu çünkü bu alan
            // deftere değil hattın KENDİSİNE bakıyor.
            if (productions.TryGetValue(identity, out StructureProduction line)
                && ReferenceEquals(line, pendingProducer))
            {
                CancelPlacement();
            }

            productions.Remove(identity);
            structureAssets.Remove(identity);

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

            // VARLIK DA AYNI SATIRDA DEFTERE GİRİYOR: yalnız üretim hattı
            // kaydedilseydi bu yapı seçildiğinde sağ panelin elinde yine yalnız
            // çekirdek tanımı olur ve simge sorusu yeniden cevapsız kalırdı.
            structureAssets[identity] = pendingStructureAsset;
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

            // GÖVDE GÖRSELİ SÜRÜKLEMEDEN GELİYOR, tahtadan sorulmuyor: oyuncunun
            // imlecin altında gördüğü simge ile tahtaya inen görselin AYNI
            // olmasının tek yolu ikisini aynı alandan okumak. Varlık atanmamışsa
            // null geçiyor ve tahta prefab'ın takım karelerinde kalıyor.
            Sprite bodySprite = pendingUnitAsset == null ? null : pendingUnitAsset.Icon;

            var identity = new Unit(pendingUnit.DisplayName);
            if (!board.PlaceUnit(identity, produced, x, y, bodySprite))
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
            StructureBlueprintAsset nextAsset = null;
            if (identity != null)
            {
                productions.TryGetValue(identity, out next);
                structureAssets.TryGetValue(identity, out nextAsset);
            }

            // AYNI SEÇİM İKİNCİ KEZ YAYINLANMIYOR: sağ panel her yayında
            // düğmelerini yeniden kuruyor ve tekrarlanan bir yayın, oyuncunun
            // seçtiği birimi görünür bir sebep olmadan varsayılana geri
            // döndürürdü.
            if (ReferenceEquals(next, selectedProduction))
            {
                return;
            }

            // İKİSİ YAN YANA, YAYINDAN ÖNCE: sağ panel yayını alır almaz simge
            // soruyor ve o soruyu eski varlıkla cevaplamamalı.
            selectedProduction = next;
            selectedProductionAsset = nextAsset;
            SelectedProductionChanged?.Invoke(next);
        }
    }
}
