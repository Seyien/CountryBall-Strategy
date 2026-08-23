using GridStrategy.Combat;
using UnityEngine;

namespace GridStrategy.Unity
{
    // ═══ ROL: GÖRÜNÜM (View) ═════════════════════════════════════════
    // kimlik : var ama SAHNE kimliği — her kopya ayrı bir nesnedir; oyun
    //          kimliği Unit'te yaşar ve bu tip Unit'i hiç görmez
    // hafıza : yok — ne "seçili miyim" ne "hangi durumdayım" burada saklanır;
    //          ikisinin de tek doğruluk kaynağı DIŞARIDA (seçim
    //          BoardAdapter.selectedUnit, durum Combatant.State). TEK İSTİSNA
    //          ve adı konmuş: authoredColor bir OYUN durumu değil, prefab'da
    //          YAZILI bir değerin önbelleğidir. → UnitView.md#rol
    // Unity  : zorunlu ama DAR — ölçüsü şu: new ile kurulamaz, AddComponent
    //          şart (MonoBehaviour + SpriteRenderer); ama Input, Camera ve
    //          Time bu dosyada HİÇ geçmez, UnitViewTests onu EditMode'da
    //          çıplak bir GameObject üstünde sürüyor:
    //          probe.AddComponent<UnitView>()
    // karar  : vermez, uygular — SetSelected(true) gelirse çerçeveyi çizer,
    //          SetState(UnitState.Dead) gelirse gri gösterir; kimin seçileceğini
    //          de kimin öldüğünü de sormaz
    /// <summary>
    /// Bir birimin EKRANDAKİ karşılığı. Tahtanın kurallarını bilmez, nerede
    /// durduğunu bilmez, <see cref="GridStrategy.Core.Unit"/> tipini hiç görmez.
    /// Yalnızca kendi GÖRSEL durumunu (bugün: seçim çerçevesi ve yaşam durumu)
    /// uygular.
    ///
    /// İki üye, İKİ FARKLI GARANTİ KAYNAĞI: seçim çerçevesi bir ÇOCUK nesnededir
    /// ve Inspector'dan gelen referansı boş bırakılabilir, bu yüzden null
    /// kontrolü vardır. Gövde çizicisi ise BU nesnenin üstündedir; varlığını
    /// <see cref="RequireComponent"/> EDITÖRE garanti ettirir, bu yüzden orada
    /// null kontrolü YOKTUR.
    ///
    /// BU TİP SAVAŞIN SÖZLÜĞÜNÜ KONUŞUYOR (<see cref="UnitState"/>) ve bu bir
    /// taviz değil, bilerek ödenmiş bir bedeldir; bedelin adı
    /// <c>GridStrategy.Unity.EditModeTests</c>'in <c>GridStrategy.Combat</c>'a
    /// verdiği referanstır. Kararı çeviren olay <see cref="SetState"/>'in ayna
    /// belgesinde yazılı.
    ///
    /// GEREKÇELER: Docs/deep/kod/Unity/UnitView.md
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class UnitView : MonoBehaviour
    {
        // Prefab'da hazır duran seçim çerçevesinin çizicisi; runtime'da
        // Instantiate EDİLMEZ (çöp üretir, kurulumu koda gömerdi). Tip
        // GameObject değil SpriteRenderer: "çizilsin mi" ve "hangi renk" tek
        // referanstan okunur. Seçim RENGİ burada alan DEĞİL — o bilgi zaten bu
        // çizicinin kendi color alanında yazılı. → UnitView.md#selectionoverlay
        [Header("Selection overlay - assign the child SpriteRenderer from the prefab")]
        [SerializeField] private SpriteRenderer selectionOverlay;

        // Bu renk alanı yukarıdaki "renk alanı tutma" gerekçesiyle ÇELİŞMİYOR:
        // orada reddedilen şey BAŞKA bir nesnede zaten yazılı olan bilginin
        // ikinci kopyasıydı; "düşmüş birim nasıl görünür" sorusunun ise başka
        // sahibi yok. Değer bir ÇARPAN, mutlak renk değil — düşme takım rengini
        // SİLMEZ, KARARTIR. → UnitView.md#downedtint
        [Header("Downed tint - multiplied over the authored body color")]
        [SerializeField] private Color downedTint = new Color(1f, 1f, 1f, 0.45f);

        // ÖLÇÜLMÜŞ SINIR: çarpma KARARTIR, doygunluğu düşürmez. Gövde bugün
        // beyaz olduğu için beyaz × 0,35 gerçekten GRİ verir ve üç durum
        // ekranda ayrışır; gövde takım rengi taşıdığı gün bu çarpan gri değil
        // "koyu kırmızı" üretir ve bu alan yetmez. → UnitView.md#deadtint
        [Header("Dead tint - multiplied over the authored body color")]
        [SerializeField] private Color deadTint = new Color(0.35f, 0.35f, 0.38f, 1f);

        // Birimin KENDİ gövde çizicisi. selectionOverlay'in aksine
        // [SerializeField] DEĞİL: o bir çocukta yaşıyor ve dışarıdan
        // gösterilmek zorunda, bu ise tam bu GameObject'in üstünde ve
        // GetComponent onu her zaman bulur. → UnitView.md#body
        private SpriteRenderer body;

        // Gövdenin PREFAB'DA YAZILI rengi. Bir oyun durumu değil, TÜREV bir
        // değerin önbelleği; "unutulamaz" olma sebebi Body'nin üstünde.
        // → UnitView.md#authoredcolor
        // ÖDÜNÇ ALINAN — `Color` bir struct, yani değer tipi; ama depolama yeri
        // tipin değil SARMALAYANIN sorusudur ve bu alan bir UnitView nesnesinin
        // içinde, yönetilen yığında yaşıyor.
        // DİL: Docs/deep/dil/07-bellek-canlilik-ve-yikim.md
        private Color authoredColor = Color.white;

        // DERİN ANLATIM: Docs/deep/konular/08-motor-cagri-dongusu.md — bu metot
        // BoardAdapter.Awake'in ORTASINDA, Instantiate satırında koşar; motorun
        // çağrı sırasındaki beşinci durak burasıdır.
        private void Awake()
        {
            // SIRA BİR KARARDIR: normalizasyon, selectionOverlay kontrolünün
            // ÜSTÜNDE. Altına konsaydı atanmamış BİR alan, ilgisiz İKİ şeyi
            // birden bozardı — aşağıdaki erken çıkış bu satırı da atlar ve
            // prefab'da ters/soluk kaydedilmiş gövde öyle kalırdı. Doğan her
            // birim AYAKTA başlamak ZORUNDA. → UnitView.md#awake
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

            // Prefab'da çerçeve AÇIK bırakılmış olabilir; doğan her birim
            // seçimsiz başlamak ZORUNDA. Üstteki SetState ile aynı iş.
            SetSelected(false);
        }

        /// <summary>
        /// Gövde çizicisi. Tembel çözülür ve önbelleğe alınır.
        /// </summary>
        // NEDEN AWAKE'TE DEĞİL — ÖLÇÜLMÜŞ bir sebep, üslup değil: Awake
        // EditMode'da HİÇ çalışmaz, dolayısıyla orada kurulan bir referans bu
        // tipi sahnesiz sınanamaz kılardı. YAZILI RENK de tam burada, çizicinin
        // çözüldüğü tek satırda yakalanıyor: gövdeye erişmenin TEK kapısı bu
        // property olduğu için "rengi yakalamadan renge yazmak" diye bir
        // sıralama KURULAMAZ. → UnitView.md#body
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
        // SEÇİM İLE DURUM İKİ BAĞIMSIZ EKSENDİR: bu metot birimin ölü olup
        // olmadığına BAKMAZ. "Ölü birim seçilemez" bir GÖRSEL kural değil bir
        // OYUN kuralıdır; uygulasaydı görünüm, tutmamaya söz verdiği durumu
        // (lastState) tutmak zorunda kalırdı ve "hafıza: yok" satırı düşerdi.
        // Kısıtın bugünkü sahibi eylem katmanı: RejectedActorCannotAct.
        // → UnitView.md#setselectedbool-isselected
        public void SetSelected(bool isSelected)
        {
            // Buradaki kontrol SESSİZ, çünkü aynı hata Awake'te bir kez zaten
            // bağırdı: orası TEŞHİS, burası HAYATTA KALMA.
            if (selectionOverlay == null)
            {
                return;
            }

            // SetActive(false) DEĞİL: GameObject'i kapatmak çocuklarını da
            // kapatır ve OnDisable/OnEnable geri çağrılarını tetikler. İstenen
            // tek şey "bu kareyi çizme"; renderer.enabled tam olarak bunu der.
            selectionOverlay.enabled = isSelected;

            // "Seçili miyim" bilgisi burada SAKLANMIYOR. Tek doğruluk kaynağı
            // BoardAdapter.selectedUnit; ikinci bir bool ikisini kaydırırdı.
        }

        /// <summary>
        /// Birimin yaşam durumunu ekrana uygular: ayakta olmayan gövde dikeyde
        /// ters çevrilir, ve renk çarpanı üç durumu birbirinden ayırır.
        /// </summary>
        /// <param name="state">
        /// Birimin savaştaki durumu. Karşılığı <c>Combatant.State</c>'tir ve
        /// çeviriyi yapan yer artık YOK — adaptör durumu olduğu gibi geçiriyor.
        /// </param>
        // BU KARAR ÇEVRİLDİ: projedeki ilk ters dönen karar burasıdır. Eski
        // kazanan SetDowned(bool) idi; iki bayrak DÖRT kombinasyon taşır ve
        // dördüncüsünün ekranda karşılığı YOKTUR ("ayakta ama gri"). Enum'u
        // içeri almanın bedeli bir assembly referansı, kazancı yalan satırının
        // hiç doğmaması. → UnitView.md#setstateunitstate-state
        public void SetState(UnitState state)
        {
            // Çizici TEK bir yerele alınıyor: property'ye ilk dokunuş hem
            // referansı hem YAZILI RENGİ yakalar ve iki satırın değerlendirme
            // sırasına güvenmek zorunda kalmak istemiyoruz.
            SpriteRenderer bodyRenderer = Body;

            // AYAKTA OLMAYAN HER DURUM YATIK: Downed ve Dead bu eksende AYNI.
            // Üç durumu ayıran şey iki eksenin BİRLEŞİMİ (yatıklık × çarpan).
            bodyRenderer.flipY = state != UnitState.Alive;

            // ÇARPMA, MUTLAK RENK DEĞİL: sol taraf birimin KENDİ kimliği
            // (prefab'da yazılı), sağ taraf YAŞAM durumu (Inspector'da yazılı).
            // Mutlak yazsaydık düşme takım bilgisini SİLERDİ. Alive için çarpan
            // Color.white, yani nötr eleman — ayrı bir dal gerekmiyor ve
            // diriltmede renk birebir geri gelir. Null kontrolü YOK: varlığını
            // RequireComponent garanti eder. → UnitView.md#setstateunitstate-state
            bodyRenderer.color = authoredColor * TintFor(state);

            // "Hangi durumdayım" bilgisi burada SAKLANMIYOR. Tek doğruluk
            // kaynağı Combatant.State; bir alan koysaydık savaş çekirdeği ile
            // ekran sessizce ayrışabilirdi - SetSelected'ın kaçındığı hatanın
            // ikizi.
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
                // demektir, yani bir programcı hatasıdır. Nötr çarpanla dönmek
                // BİR KARAR: bilinmeyen durumda birimi GÖRÜNÜR bırakır;
                // Color.clear bir programcı hatasını oyun hatasına çevirirdi.
                // → UnitView.md#tintforunitstate-state
                default:
                    Debug.LogError($"[UnitView] Unhandled unit state: {state}.", this);
                    return Color.white;
            }
        }
    }
}
