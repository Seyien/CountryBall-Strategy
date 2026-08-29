using GridStrategy.Combat;
using UnityEngine;

namespace GridStrategy.Unity
{
    // ═══ ROL: GÖRÜNÜM (View) ═════════════════════════════════════════
    // kimlik : var ama SAHNE kimliği — her bina ayrı bir nesnedir; oyun
    //          kimliği Unit'te yaşar ve bu tip Unit'i hiç görmez
    // hafıza : ÖNBELLEK — son UYGULANAN durum ve seçim burada tutuluyor,
    //          ama ikisinin de tek doğruluk kaynağı DIŞARIDA (durum
    //          Structure.State, seçim BoardAdapter.selectedUnit). Saklama
    //          sebebi HealthBarView.lastFraction ile birebir aynı: bu üyeler
    //          her karede çağrılıyor ve renk ataması Unity tarafında bir
    //          malzeme güncellemesi tetikliyor
    // Unity  : zorunlu ama DAR — SpriteRenderer ister; Input, Camera ve Time
    //          bu dosyada HİÇ geçmez ve çıplak bir GameObject üstünde
    //          EditMode'da sürülebilir
    // karar  : vermez, uygular — SetState(StructureState.Destroyed) gelirse
    //          binayı karartıp soldurur; neyin yıkıldığını sormaz
    /// <summary>
    /// Bir yapının EKRANDAKİ karşılığı: yıkılan bina kararıp soluklaşır, ayakta
    /// duran bina kendi renginde kalır.
    /// Deseni <see cref="UnitView"/> ile ortak — HUMBLE OBJECT: kural bilmez,
    /// sahne istemez; ve tazelenme yolu <see cref="HealthBarView"/>'ünkiyle
    /// ortak — PULL-BASED REFRESH: olayla değil, her karede durumdan okunarak.
    /// TUZAK: gövde renginin TEK yazanı burasıdır. Yıkım ile seçim aynı
    /// çarpımda birleşiyor; ikisi ayrı yerden yazılsaydı biri ötekini silerdi.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class StructureView : MonoBehaviour
    {
        // OYUNDA NE İŞE YARAR: taretin canı bittiğinde oyuncu, kulenin artık
        // ateş etmeyeceğini enkaz kalkmadan ÖNCE görsün.
        //
        // DİL UnitView'DEN ÖDÜNÇ, YENİ DEĞİL: orada ölü birim koyu bir çarpanla
        // (deadTint) ve düşmüş birim düşük saydamlıkla (downedTint) anlatılıyor.
        // Yapının tek yıkık hâli ikisini birden taşıyor, çünkü oyuncunun
        // öğreneceği tek cümle şu olsun: KOYU VE SOLGUN OLAN ARTIK SAVAŞMIYOR.
        //
        // ÇARPAN, MUTLAK RENK DEĞİL: binanın kendi rengi yazıldığı gün (takım
        // rengi, hasar boyası) yıkım o rengi SİLMEZ, KARARTIR.
        [Header("Destroyed tint - multiplied over the authored body color")]
        [Tooltip("Yıkılmış yapının gövde rengine uygulanan çarpan. Alfa kanalı " +
                 "enkazı soldurur; RGB kanalları karartır.")]
        [SerializeField] private Color destroyedTint = new Color(0.34f, 0.31f, 0.29f, 0.6f);

        // SAYININ SAHİBİ BoardAdapter'DAN BURAYA TAŞINDI ve sebebi tek bir
        // cümle: aynı SpriteRenderer.color alanına iki ayrı dosyadan yazılırsa
        // seçili bir binanın yıkılması seçim rengini, seçimin kaldırılması da
        // yıkım rengini sessizce siler.
        [Header("Selected tint - multiplied over the authored body color")]
        [Tooltip("Seçili yapının gövde rengine uygulanan çarpan. Seçimin ikinci " +
                 "kanalı olan kenarlık çerçevesi BoardAdapter'da yaşıyor.")]
        [SerializeField] private Color selectedTint = new Color(1f, 0.86f, 0.45f, 1f);

        // Yapının KENDİ gövde çizicisi. UnitView'deki ikiziyle aynı gerekçe:
        // tam bu GameObject'in üstünde yaşıyor ve GetComponent onu her zaman
        // bulur, dolayısıyla [SerializeField] değil.
        private SpriteRenderer body;

        // Gövdenin YAZILI rengi. Bir oyun durumu değil, türev bir değerin
        // önbelleği; UnitView.authoredColor ile aynı kategori ve aynı yerde
        // (çizicinin ilk çözüldüğü satırda) yakalanıyor.
        private Color authoredColor = Color.white;

        // BAŞLANGIÇ DEĞERİ AÇIKÇA YAZILI, VE BU BİR SÜS DEĞİL: StructureState'in
        // sıfırıncı değeri Destroyed. Alan varsayılana bırakılsaydı yeni doğan
        // her bina "zaten yıkık" sayılır, ilk SetState(Standing) çağrısı aşağıdaki
        // kısa devreye takılmaz — ama ilk SetState(Destroyed) TAKILIR ve gerçekten
        // yıkılan bina hiç kararmazdı.
        private StructureState appliedState = StructureState.Standing;

        private bool appliedSelection;

        /// <summary>
        /// Gövde çizicisi. Tembel çözülür ve önbelleğe alınır.
        /// </summary>
        // NEDEN AWAKE'TE DEĞİL — gerekçe UnitView.Body'de ölçülerek yazılı ve
        // burada TEKRAR EDİLMİYOR, yalnızca uygulanıyor: Awake EditMode'da hiç
        // çalışmaz ve orada kurulan bir referans bu tipi sahnesiz sınanamaz
        // kılardı.
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
        /// Yapının yaşam durumunu ekrana uygular: yıkılmış bina kararır ve
        /// soluklaşır, ayakta duran bina yazılı renginde kalır.
        /// </summary>
        /// <param name="state">
        /// Yapının savaştaki durumu. Karşılığı <c>Structure.State</c>'tir ve
        /// adaptör onu olduğu gibi geçirir; bu tip hiçbir çeviri yapmaz.
        /// </param>
        // DEĞİŞMEDİYSE DOKUNMA — gerekçe HealthBarView.SetFraction'da bir kez
        // yazılı ve burada uygulanıyor: bu üye her karede çağrılıyor, renk
        // ataması ise bir malzeme güncellemesi tetikliyor.
        //
        // REDDEDILEN - yıkık binayı UnitView gibi dikeyde ters çevirmek.
        //     bodyRenderer.flipY = state != StructureState.Standing;
        // KIRILAN: bina bir asker değil; ters duran çatı "yıkıldı" değil "ekran
        // bozuldu" diye okunur, üstelik bir hücreden geniş binada temel hâlâ
        // kendi hücresinde durduğu için görsel komşu hücreye taşar.
        // KAZANIRDI: renk körü bir oyuncu için ikinci bir kanal açardı ve iki
        // kanal tek kanaldan her zaman daha okunaklıdır.
        // TEK CUMLE: yatıklık düşen bir GÖVDENİN dili, çöken bir BİNANIN değil;
        // ikinci kanal gerçek bir moloz görseli geldiği gün oradan gelecek.
        public void SetState(StructureState state)
        {
            if (appliedState == state)
            {
                return;
            }

            appliedState = state;
            ApplyTint();
        }

        /// <summary>
        /// Seçim rengini açar ya da kapatır.
        /// </summary>
        // SEÇİMİN İKİNCİ KANALI BURADA DEĞİL: kenarlık çerçevesi hâlâ
        // BoardAdapter'da kuruluyor, çünkü çerçevenin sprite'ı adaptörün
        // Inspector alanından geliyor. Buraya taşımak o alanı da taşımak, yani
        // operatöre yeni bir sürükleme borcu yazmak olurdu.
        public void SetSelected(bool isSelected)
        {
            if (appliedSelection == isSelected)
            {
                return;
            }

            appliedSelection = isSelected;
            ApplyTint();
        }

        /// <summary>
        /// İki ekseni tek renk çarpımında birleştirir.
        /// </summary>
        // ÇARPIM SIRASI GÖZLENEMEZ, ama BİRLEŞMESİ zorunlu: yıkım ile seçim ayrı
        // ayrı yazsaydı sonuncusu ötekini siler ve hata "bazen seçim görünmüyor"
        // diye çıkardı — operatörün bu dosyadan önce bir kez bildirdiği belirtinin
        // aynısı.
        private void ApplyTint()
        {
            Body.color = authoredColor
                         * TintFor(appliedState)
                         * (appliedSelection ? selectedTint : Color.white);
        }

        /// <summary>
        /// Bir duruma karşılık gelen renk ÇARPANINI verir.
        /// </summary>
        private Color TintFor(StructureState state)
        {
            switch (state)
            {
                // Nötr çarpan: yazılı renk aynen kalır.
                case StructureState.Standing:
                    return Color.white;

                case StructureState.Destroyed:
                    return destroyedTint;

                // default LOG DEĞİL LogError — gerekçesi UnitView.TintFor'da bir
                // kez yazılı: buraya düşmek StructureState'e üçüncü bir değer
                // eklenip bu switch'in güncellenmediği anlamına gelir, yani bir
                // programcı hatasıdır. Nötr çarpanla dönmek bilinmeyen durumdaki
                // binayı GÖRÜNÜR bırakıyor.
                default:
                    Debug.LogError($"[StructureView] Unhandled structure state: {state}.", this);
                    return Color.white;
            }
        }
    }
}
