namespace GridStrategy.Unity
{
    //   ═══ TEK KAPI — HER GEÇİŞ BURADAN GEÇER ═══════════════════════
    //
    //     Enter(yeni)                 LeaveIfCurrent(kip)
    //         │                              │
    //         ▼                              ▼
    //     aynı kip mi? ──evet──► hiçbir şey  yürürlükte mi? ──hayır──► false
    //         │hayır                              │evet
    //         ▼                                   ▼
    //     eskisi.Exit() ──► current = yeni ──► Enter(Boşta)
    //         │
    //         ▼
    //     yenisi.Enter()
    //   SIRA DEĞİŞMEZ: önce Cik, sonra atama, sonra Gir.

    /// <summary>
    /// Tahtanın kip makinesi: hangi kipin yürürlükte olduğunu tutar ve geçişi
    /// TEK yerden yaptırır.
    /// </summary>
    // BU TİPİN VAR OLMA SEBEBİ DAĞINIK ATAMALARDI. Ölçüm eski hâlde şuydu:
    // `isPlacingStructure` beş satırda, bekleyen vuruşun dört alanı sekiz
    // satırda yazılıyordu ve "girerken şunu da iptal et" kuralı her çağıranın
    // hafızasına bırakılmıştı. Tek kapı, o kuralı hafızadan alıp koda koyuyor.
    //
    // MonoBehaviour DEĞİL: makine ne Update alır ne sahneye bağlanır, bu yüzden
    // EditMode'da doğrudan kurulup sınanabilir.
    public sealed class BoardModeMachine
    {
        // Boşta kipi ayrı bir alanda tutuluyor çünkü ÇIKIŞIN VARIŞ NOKTASI o:
        // her kip er ya da geç buraya döner ve dönülecek yeri her çağırana
        // taşıtmak, aynı cevabın birden çok kopyasını doğururdu.
        private readonly IBoardMode idle;

        private IBoardMode current;

        /// <summary>
        /// Makineyi Boşta kipiyle başlatır.
        /// </summary>
        /// <param name="idleMode">Hiçbir kip yürürlükte değilken geçerli olan kip.</param>
        // KURUCUDA Enter ÇAĞRILMIYOR ve yokluğu bir karardır: Boşta kipinin
        // girişte yapacağı hiçbir iş yok, ve çağırsaydık tahtanın alanları
        // henüz kurulmamışken bir kipin ekrana dokunmasına kapı açardık.
        public BoardModeMachine(IBoardMode idleMode)
        {
            idle = idleMode;
            current = idleMode;
        }

        /// <summary>
        /// Şu anda yürürlükte olan kip.
        /// </summary>
        public IBoardMode Current => current;

        /// <summary>
        /// Verilen kipe geçer: önce açık kip kapanır, sonra yenisi açılır.
        /// </summary>
        // AYNI KİPE GEÇİŞ SESSİZCE YUTULUYOR ve bu bir kolaylık değil bir
        // koruma: yutulmasaydı yerleştirme kipindeyken B tuşuna ikinci kez
        // basmak hayaleti kapatıp yeniden açar, taşınan hayaleti düşürürdü.
        public void Enter(IBoardMode next)
        {
            if (next == null || ReferenceEquals(next, current))
            {
                return;
            }

            // ÖNCE ÇIKIŞ, SONRA ATAMA: ters sırada eski kipin Cik() işi kendini
            // yürürlükte SANMAYAN bir makinede koşardı ve LeaveMode gibi
            // "hâlâ ben miyim" soran her üye yanlış cevap verirdi.
            current.Exit();
            current = next;
            current.Enter();
        }

        /// <summary>
        /// Boşta kipine döner.
        /// </summary>
        public void ToIdle()
        {
            Enter(idle);
        }

        /// <summary>
        /// Verilen kip hâlâ yürürlükteyse Boşta kipine döner.
        /// </summary>
        /// <returns>Geçiş gerçekten yapıldıysa true.</returns>
        // ŞARTLI ÇIKIŞ, ŞARTSIZ ÇIKIŞTAN AYRI DURUYOR ve ayrımı ölçüm doğurdu:
        // bekleyen vuruşu düşüren çağrıların bir kısmı (paletten bina almak,
        // temizlik süpürmesi) yerleştirme kipi AÇIKKEN de gelebiliyor. Şartsız
        // olsaydı o çağrılar oyuncunun elindeki hayaleti sessizce düşürürdü.
        public bool LeaveIfCurrent(IBoardMode mode)
        {
            if (!ReferenceEquals(mode, current))
            {
                return false;
            }

            ToIdle();
            return true;
        }
    }
}
