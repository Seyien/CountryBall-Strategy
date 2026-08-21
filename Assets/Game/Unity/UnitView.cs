using GridStrategy.Combat;
using UnityEngine;

namespace GridStrategy.Unity
{
    // ═══ ROL: GÖRÜNÜM (View) ═════════════════════════════════════════
    // kimlik : var ama SAHNE kimliği — her kopya ayrı bir nesnedir; oyun
    //          kimliği Unit'te yaşar ve bu tip Unit'i hiç görmez
    // hafıza : yok — ne "seçili miyim" ne "hangi durumdayım" burada saklanır;
    //          ikisinin de tek doğruluk kaynağı DIŞARIDA (seçim
    //          BoardAdapter.selectedUnit, durum Combatant.State); çiziciler
    //          yalnızca onların yansıması.
    //          TEK İSTİSNA ve adı konmuş: authoredColor. O bir OYUN durumu
    //          değil, prefab'da YAZILI bir değerin önbelleğidir — body
    //          referansının önbelleklenmesiyle aynı sınıftan bir şey, aynı
    //          satırda ve aynı anda alınır. Gerekçesi Body'nin üstünde.
    // Unity  : zorunlu — MonoBehaviour + SpriteRenderer; var olma sebebi motorun ta kendisi
    // karar  : vermez, uygular — SetSelected(true) gelirse çerçeveyi çizer,
    //          SetState(UnitState.Dead) gelirse gri gösterir; kimin seçileceğini
    //          de kimin öldüğünü de sormaz
    /// <summary>
    /// Bir birimin EKRANDAKİ karşılığı. Tahtanın kurallarını bilmez, nerede
    /// durduğunu bilmez, <see cref="GridStrategy.Core.Unit"/> tipini hiç görmez.
    /// Yalnızca kendi GÖRSEL durumunu (bugün: seçim çerçevesi ve yaşam durumu)
    /// uygular.
    ///
    /// Var olma sebebi: adaptör "şu birimi seçili göster" demek istediğinde,
    /// çerçevenin bir ÇOCUK nesnede yaşadığını bilmek zorunda kalıyordu.
    /// O bilgi burada kapalı kalır; adaptör yalnızca komutu verir.
    ///
    /// İki üye, İKİ FARKLI GARANTİ KAYNAĞI — ve ayrım bu dosyanın en öğretici
    /// satırı: seçim çerçevesi bir ÇOCUK nesnededir, ona ancak Inspector'dan
    /// verilen bir referansla ulaşılır ve o referans boş bırakılabilir, bu
    /// yüzden null kontrolü vardır. Gövde çizicisi ise BU nesnenin üstündedir;
    /// varlığını <see cref="RequireComponent"/> derleyiciye değil EDITÖRE
    /// garanti ettirir ve GetComponent asla boş dönmez, bu yüzden orada null
    /// kontrolü YOKTUR. Aynı dosyada iki farklı savunma, çünkü iki farklı risk.
    ///
    /// BU TİP ARTIK SAVAŞIN SÖZLÜĞÜNÜ KONUŞUYOR (<see cref="UnitState"/>) ve bu
    /// bir taviz değil, bilerek ödenmiş bir bedeldir; hangi olayın kararı
    /// çevirdiği <see cref="SetState"/>'in üstündeki REDDEDILEN bloğunda yazılı.
    /// Ödenen bedelin adı da orada: <c>GridStrategy.Unity.EditModeTests</c>
    /// artık <c>GridStrategy.Combat</c>'a referans vermek zorunda.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class UnitView : MonoBehaviour
    {
        // Prefab'da hazır duran seçim çerçevesinin çizicisi.
        //
        // Neden runtime'da Instantiate edilmiyor: her seçimde nesne doğurup yok
        // etmek çöp (garbage) üretir ve kurulumu koda gömerdi. Prefab'da duran
        // bir çocuk ise Editor'de görülebilir, sprite'ı ve rengi elle
        // ayarlanabilir; kod yalnızca açıp kapatır.
        //
        // Tip neden GameObject değil SpriteRenderer: bu tek referanstan hem
        // "çizilsin mi" (enabled) hem "hangi renk" (color) doğrudan okunur.
        // GameObject tutsaydık her erişimde GetComponent gerekirdi.
        //
        // Seçim RENGİ burada bir alan DEĞİL. Renk zaten bu SpriteRenderer'ın
        // kendi color alanında yaşıyor ve prefab'da yazılıyor. Ayrıca bir
        // [SerializeField] Color tutsaydık aynı bilginin İKİ kaynağı olurdu
        // (DECISION_TOOLKIT sorusu 1) ve koddaki değer, Editor'de ayarlanan
        // rengi her Awake'te sessizce ezerdi.
        [Header("Selection overlay - assign the child SpriteRenderer from the prefab")]
        [SerializeField] private SpriteRenderer selectionOverlay;

        // AŞAĞIDAKİ İKİ RENK ALANI, YUKARIDAKİ "renk alanı tutma" GEREKÇESİYLE
        // ÇELİŞMİYOR — ve fark tek cümlede duruyor: orada reddedilen şey BAŞKA
        // BİR NESNENİN kendi color alanında ZATEN yazılı olan bilginin ikinci
        // kopyasıydı. Burada ikinci kopya yok: "düşmüş bir birim nasıl görünür"
        // sorusunun projede başka hiçbir sahibi yok. Bir alanın kusuru "renk
        // olması" değil, "aynı bilginin ikinci kaynağı olması"dır.
        //
        // ÇARPAN, MUTLAK DEĞER DEĞİL: değer gövdenin YAZILI rengiyle çarpılır.
        // Böylece Alive için nötr çarpan (Color.white) tek bir kod yolunu
        // korur ve gövde bir gün takım rengi taşıdığında düşme onu SİLMEZ,
        // KARARTIR. Mutlak yazsaydık üç durumdan ikisi takım rengini yok ederdi.
        [Header("Downed tint - multiplied over the authored body color")]
        [SerializeField] private Color downedTint = new Color(1f, 1f, 1f, 0.45f);

        // ÖLÇÜLMÜŞ SINIR, ve süslemeden yazıyorum: çarpma KARARTIR, doygunluğu
        // düşürmez. Gövde bugün beyaz olduğu için (BoardAdapter birimin kendi
        // color'ına hiç dokunmuyor) beyaz × 0,35 gerçekten GRİ verir ve üç
        // durum ekranda ayrışır. Gövde bir gün takım rengi taşıdığı gün bu
        // çarpan "koyu kırmızı" üretir, gri değil — o gün ölü görseli ya bir
        // materyal/shader ya da ayrı bir sprite ister ve bu alan yetmez.
        [Header("Dead tint - multiplied over the authored body color")]
        [SerializeField] private Color deadTint = new Color(0.35f, 0.35f, 0.38f, 1f);

        // Birimin KENDİ gövde çizicisi. selectionOverlay'in aksine [SerializeField]
        // DEĞİL: o bir çocuk nesnede yaşıyor ve dışarıdan gösterilmek zorunda,
        // bu ise tam olarak bu GameObject'in üstünde ve GetComponent onu her
        // zaman bulur. Serileştirilseydi Inspector'da boş bırakılabilen ikinci
        // bir kurulum adımı doğardı — hem de gereksiz yere.
        private SpriteRenderer body;

        // Gövdenin PREFAB'DA YAZILI rengi. Bir oyun durumu değil, yazılı bir
        // değerin önbelleği; gerekçesi ve "unutulamaz" olma sebebi Body'nin
        // üstünde yazılı.
        private Color authoredColor = Color.white;

        private void Awake()
        {
            // SIRA BİR KARARDIR: durum normalizasyonu, selectionOverlay
            // kontrolünün ÜSTÜNDE duruyor. Altına konsaydı, çerçeve referansı
            // atanmamış bir prefab'da aşağıdaki erken çıkış bu satırı da
            // atlardı ve prefab'da ters/soluk kaydedilmiş bir gövde öyle
            // kalırdı — atanmamış BİR alan, ilgisiz İKİ şeyi birden bozardı.
            //
            // Prefab'da gövde ters ya da soluk bırakılmış olabilir; Editor'de
            // "ölü hâli nasıl duruyor" diye bakıp öyle kaydetmek çok kolaydır.
            // Doğan her birim AYAKTA başlamak ZORUNDA. Bu satır, yazılı durumu
            // (authored state) çalışma zamanı değişmezine çevirir — SetSelected(false)
            // ile birebir aynı işi, birebir aynı gerekçeyle yapar.
            SetState(UnitState.Alive);

            // Eksik atama SESSİZ kalmasın: referans boşsa seçim hiç çalışmaz ve
            // ekranda hiçbir hata görünmez. Bir kez, doğuşta, gürültüyle söyle.
            if (selectionOverlay == null)
            {
                Debug.LogError(
                    "[UnitView] selectionOverlay is not assigned. Assign the SelectionOverlay child's SpriteRenderer on the Unit prefab.",
                    this);
                return;
            }

            // Prefab'da çerçeve AÇIK bırakılmış olabilir - Editor'de nasıl
            // göründüğüne bakmak için açıp öyle kaydetmek çok kolaydır.
            // Doğan her birim seçimsiz başlamak ZORUNDA.
            SetSelected(false);
        }

        /// <summary>
        /// Gövde çizicisi. Tembel çözülür ve önbelleğe alınır.
        /// </summary>
        // NEDEN AWAKE'TE DEĞİL — bu ÖLÇÜLMÜŞ bir sebep, üslup değil: Awake
        // EditMode'da HİÇ çalışmaz (bu script [ExecuteAlways] değil). Referans
        // Awake'te kurulsaydı `new GameObject().AddComponent<UnitView>()` ile
        // kurulan bir testte body sonsuza dek null kalır, SetState sessizce
        // hiçbir şey yapar ve üye sınanamaz olurdu — ekranda görünmeyen bir
        // hata, kırmızıya dönmeyen bir test. Tembel çözüm bu tipi sahne
        // kurmadan, Play'e basmadan sınanabilir kılıyor; bedeli ilk çağrıdaki
        // tek bir GetComponent.
        //
        // `body == null` Unity'nin AŞIRI YÜKLENMİŞ eşitliğidir: yok edilmiş bir
        // nesne C# tarafında hâlâ referans taşır ama bu karşılaştırma true
        // döner. Yani nesne yok edilip yeniden kurulursa önbellek kendini
        // tazeler; `ReferenceEquals(body, null)` yazsaydık ölü referansı
        // canlıymış gibi kullanırdık.
        //
        // YAZILI RENK NEDEN TAM BURADA YAKALANIYOR: SetState'in reddettiği eski
        // seçenek, renk soldurmanın "kopyayı almayı unutan bir yol" ürettiğini
        // söylüyordu (blok aşağıda, çevrilmiş hâliyle). O yolu kapatan şey,
        // kopyanın ayrı bir adımda DEĞİL, çizicinin çözüldüğü tek satırda
        // alınmasıdır: gövdeye erişmenin tek kapısı bu property olduğu için
        // "rengi yakalamadan renge yazmak" diye bir sıralama YOKTUR.
        // Yok edilip yeniden kurulan bir nesnede de doğru davranır — önbellek
        // tazelenirken YENİ nesnenin yazılı rengi yakalanır, eskisi değil.
        private SpriteRenderer Body
        {
            get
            {
                if (body == null)
                {
                    body = GetComponent<SpriteRenderer>();
                    authoredColor = body.color;
                }

                return body;
            }
        }

        /// <summary>
        /// Seçim çerçevesini gösterir veya gizler.
        /// </summary>
        // SEÇİM İLE DURUM, İKİ BAĞIMSIZ EKSENDİR — ve bu bir karardır:
        // bu metot birimin ölü olup olmadığına BAKMAZ. "Ölü birim seçilemez"
        // bir GÖRSEL kural değil bir OYUN kuralıdır ve bu tip oyun kuralı
        // uygulamaz.
        //
        // REDDEDILEN - UnitView.cs:205 yerine (görünüm seçilebilirliği kendisi
        //              kısıtlar):
        //     public void SetSelected(bool isSelected)
        //     {
        //         if (lastState == UnitState.Dead) { return; }   // sessiz ret
        //         ...
        //     }
        // KIRILAN  : görünüm, tutmamaya söz verdiği durumu tutmak ZORUNDA kalır.
        //            lastState alanı doğar     -> "hafıza: yok" satırı düşer
        //            SetSelected bazen çalışır -> çağıran neden'i göremez, dönüşü yok
        //            adaptör "seçtim" der      -> ekran seçmez, ikisi sessizce ayrışır
        //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
        // KAZANIRDI: kısıt gerçekten ÇİZİM kuralı olsaydı — çerçeve sprite'ı
        //            yalnızca ayakta duran gövdeye oturuyorsa ve ölü gövdede
        //            ekranda kayıyorsa. O gün sahibi burasıydı.
        // TEK CUMLE: Görünüm bir kuralı uygulayabilmek için o kuralın girdisini
        //            SAKLAMAK zorunda kalıyorsa, kural görünümün değildir.
        //
        // BUGÜNKÜ SAHİP: eylem katmanı — ölü birim seçilebilir ama seçildikten
        // sonra yapabileceği her şey AttackRules ile MovementRules'a takılır:
        // AttackOutcome/MoveOutcome.RejectedActorCannotAct.
        // EŞİK: arayüz tıklamadan ÖNCE "bunu seçemezsin" demek zorunda kaldığı
        // gün (imleç değişimi, soluk çerçeve) kısıt görsel hâle gelir ve bu metot
        // durumu SAKLAYARAK değil, parametre olarak alarak sormaya başlar.
        public void SetSelected(bool isSelected)
        {
            // Buradaki kontrol SESSİZ, çünkü aynı hata Awake'te bir kez zaten
            // bağırdı. Tıklama başına LogError yazmak Console'u doldurur ve
            // asıl mesajı gömerdi.
            if (selectionOverlay == null)
            {
                return;
            }

            // SetActive(false) DEĞİL: GameObject'i kapatmak çocuklarını da
            // kapatır ve OnDisable/OnEnable geri çağrılarını tetikler. Bizim
            // istediğimiz tek şey "bu kareyi çizme". renderer.enabled tam olarak
            // bunu söyler; nesne hayatta kalır, hiçbir yaşam döngüsü olayı
            // tetiklenmez, referanslar bozulmaz.
            //
            // Alternatif olarak rengin alfasını 0 yapmak da görünmez kılardı ama
            // nesne yine ÇİZİLİRDİ (görünmez bir draw call) ve prefab'da
            // ayarlanan gerçek rengi ezerdi.
            selectionOverlay.enabled = isSelected;

            // "Seçili miyim" bilgisi burada SAKLANMIYOR. Tek doğruluk kaynağı
            // BoardAdapter.selectedUnit; burada bir bool daha tutsaydık ikisi
            // kayabilir ve hata sessiz olurdu (DECISION_TOOLKIT sorusu 1).
            // Bu bileşen durumu tutmaz, durumu UYGULAR.
        }

        /// <summary>
        /// Birimin yaşam durumunu ekrana uygular: ayakta olmayan gövde dikeyde
        /// ters çevrilir, ve renk çarpanı üç durumu birbirinden ayırır.
        /// </summary>
        /// <param name="state">
        /// Birimin savaştaki durumu. Karşılığı <c>Combatant.State</c>'tir ve
        /// çeviriyi yapan yer artık YOK — adaptör durumu olduğu gibi geçiriyor.
        /// </param>
        // ═══ BU BLOK ÇEVRİLDİ, SİLİNMEDİ ═════════════════════════════════
        // Projedeki İLK ters dönen karar burasıdır. Çevrilmeden önce kazanan
        // `SetDowned(bool)`'du ve `SetState(UnitState)` bu dosyada REDDEDILEN
        // olarak yazılıydı; o bloğun kendi KAZANIRDI satırı tetiği önceden
        // adlandırmıştı — *"Dead ile Downed FARKLI görünmek zorunda kaldığı
        // gün"*. Tetik geldi: madde #9, "üç durum, iki görsel".
        // ÇEVİREN OLAY, tek cümle: `Dead` kendi görselini kazandı.
        //
        // REDDEDILEN - UnitView.cs:275 yerine (çevrilmeden önceki kazanan;
        //              savaş sözlüğü hiç öğrenilmez, üç durum bayrağa iner):
        //     public void SetDowned(bool isDowned)
        //     {
        //         Body.flipY = isDowned;
        //     }
        // KIRILAN  : bir bool İKİ değer taşır, gösterilecek durum ise ÜÇ tane.
        //            üçüncüsü ikinci üyeyi doğurur -> SetDowned + SetDead
        //            SetDead(true) demeyi unutan yol -> ölü birim ayağa kalkar
        //            iki bayrak dört kombinasyon    -> üçü anlamlı, biri yalan
        //            derleyici: hiçbir şey der  .  test: iki üye ayrı sınandığı
        //            için ikisi de yeşil kalır
        // KAZANIRDI: hâlâ kazanırdı ve koşulu değişmedi — gösterilecek durum
        //            gerçekten İKİ değerli olsaydı; madde #9'dan önce Downed ile
        //            Dead ekranda AYNI görünüyordu ve bool yeterliydi.
        // TEK CUMLE: Enum'u içeri almanın bedeli bir assembly referansı, kazancı
        //            derleyicinin üç dalı da sormasıdır; bool'un bedeli ise
        //            çağıranın kimsenin sormadığı bir sözü tutmak zorunda kalması.
        //
        // ESKİ KAZANANIN BEDELİ ÖDENDİ ve tahmin birebir tuttu: çevrilen blok
        // *"GridStrategy.Unity.EditModeTests'e GridStrategy.Combat referansı
        // eklemek gerekir"* diyordu; bu dosyadaki `using GridStrategy.Combat;`
        // yüzünden UnitViewTests bugün bir UnitState değeri yazabilmek için o
        // referansa muhtaç. Aynı bloğun İKİNCİ iddiası ise ÇÜRÜDÜ: *"'karar
        // vermez, uygular' sözü ilk satırda düşer"* denmişti — düşmedi, çünkü
        // "Dead nasıl görünür" sorusuna bu metot değil Inspector'daki iki çarpan
        // cevap veriyor; tasarım kararı koda değil yazılı veriye taşındı.
        public void SetState(UnitState state)
        {
            // Çizici TEK bir yerel değişkene alınıyor ve bu bir üslup tercihi
            // değil: property'ye ilk dokunuş hem referansı hem YAZILI RENGİ
            // yakalar, ve iki satırın hangi sırayla değerlendirildiğine
            // güvenmek zorunda kalmak istemiyoruz. Yakalama burada, gözle
            // görülür ve tek bir satırda oluyor.
            SpriteRenderer bodyRenderer = Body;

            // AYAKTA OLMAYAN HER DURUM YATIK: Downed ve Dead bu eksende AYNI.
            // Üç durumu ayıran şey iki eksenin BİRLEŞİMİ (yatıklık × çarpan),
            // tek bir eksen değil — Alive/Downed yatıklıkta, Downed/Dead
            // çarpanda, Alive/Dead ikisinde birden ayrışır.
            bodyRenderer.flipY = state != UnitState.Alive;

            // ═══ İKİNCİ ÇEVRİLEN BLOK ════════════════════════════════════
            // Burada RENK SOLDURMAYI reddeden bir blok duruyordu: *"authoredColor
            // bir ALAN olmak ZORUNDA ve o kopya bu tipin 'hafıza: yok' satırını
            // bozar; kopyayı almayı unutan tek bir yol, birimin gerçek rengini
            // kalıcı olarak siler."* ÇEVİREN OLAY aynı: `Dead` kendi görselini
            // kazandı ve üç durumu ayırmanın renksiz bir yolu yok.
            //
            // İTİRAZ NEDEN ÇÜRÜDÜ: "unutmak" bir SIRALAMA riskiydi ve sıralama
            // ortadan kalktı — kopya artık gövdeye erişmenin TEK kapısı olan Body
            // property'sinde, referansın çözüldüğü satırın yanında alınıyor.
            // AYAKTA KALAN YARISI: "gövde rengi bir gün takımı taşır ve düşme onu
            // ezer" hâlâ doğru; cevabı ÇARPMAdır — düşme ezmez, KARARTIR ve takım
            // bilgisi çarpanın altında yaşar. Çarpmanın kendi sınırı deadTint'in
            // üstünde ölçülmüş hâliyle yazılı.
            //
            // Null kontrolü YOK ve bu selectionOverlay ile çelişmiyor:
            // RequireComponent bu çizicinin varlığını garanti eder.
            //
            // REDDEDILEN - UnitView.cs:328 yerine (renk hiç kullanılmaz, üçüncü
            //              durum ikinci bir BOOL eksenine bindirilir):
            //     bodyRenderer.flipY = state != UnitState.Alive;
            //     bodyRenderer.flipX = state == UnitState.Dead;
            // KIRILAN  : hiçbir alan gerekmez — cazip olan yanı bu; kırılan şey
            //            görsel dilin kendisi.
            //            iki eksende ters sprite -> oyuncu "ölü" değil "yanlış
            //            çizilmiş" okur; simetrik sprite'ta hiçbir şey okumaz
            //            derleyici: hiçbir şey der  .  test: YEŞİL kalır, çünkü
            //            flipX/flipY alanlarını okur, ekranı değil
            // KAZANIRDI: gövde rengi bu tipin DIŞINDAN yazılırsa (takım rengi,
            //            hasar yanıp sönmesi) authoredColor gövdenin yazılı rengi
            //            olmaktan çıkar; o gün bool ekseni kusurlu ama dürüsttür.
            // TEK CUMLE: Bir görsel eksenin değeri kaç durum taşıyabildiğiyle
            //            değil, oyuncunun onu ne sandığıyla ölçülür.
            //
            // Alive için çarpan Color.white'tır ve bu NÖTR eleman olduğu için
            // ayrı bir dal gerekmiyor: `authoredColor * white` kayan noktada
            // TAM olarak authoredColor'dır. Yani diriltme geldiği gün renk
            // birebir geri gelir; "yaklaşık geri gelir" değil.
            bodyRenderer.color = authoredColor * TintFor(state);

            // "Hangi durumdayım" bilgisi burada SAKLANMIYOR. Tek doğruluk
            // kaynağı Combatant.State; buraya bir alan koysaydık savaş çekirdeği
            // ile ekran sessizce ayrışabilirdi - tam olarak SetSelected'ın
            // kaçındığı hatanın ikizi.
        }

        /// <summary>
        /// Bir duruma karşılık gelen renk ÇARPANINI verir.
        /// </summary>
        private Color TintFor(UnitState state)
        {
            switch (state)
            {
                // Nötr çarpan: yazılı renk aynen kalır.
                case UnitState.Alive:
                    return Color.white;

                case UnitState.Downed:
                    return downedTint;

                case UnitState.Dead:
                    return deadTint;

                // default LOG DEĞİL LogError: buraya düşmek "UnitState'e
                // dördüncü bir değer eklendi ve bu switch güncellenmedi"
                // demektir, yani bir programcı hatasıdır. Aynı gerekçe
                // BoardAdapter'ın sonuç switch'lerinde de yazılı.
                //
                // Nötr çarpanla dönmek BİR KARAR: bilinmeyen bir durumda birimi
                // GÖRÜNÜR bırakır. Color.clear dönseydi birim ekrandan
                // kaybolurdu ve programcı hatası bir oyun hatasına dönüşürdü.
                default:
                    Debug.LogError($"[UnitView] Unhandled unit state: {state}.", this);
                    return Color.white;
            }
        }
    }
}
