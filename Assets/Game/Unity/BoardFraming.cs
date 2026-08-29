using UnityEngine;

namespace GridStrategy.Unity
{
    /// <summary>
    /// Kameranın gideceği çerçeve: nereye bakacağı ve ne kadar göreceği.
    /// İkisi tek tipte, çünkü merkez yarım yükseklikten hesaplanıyor.
    /// TUZAK: serileşmez ve serileşmemeli — Unity serileştirici kurucuyu
    /// atlıyor, yani Inspector'a konan bir <c>readonly struct</c> yalan söyler.
    /// </summary>
    public readonly struct BoardFrame
    {
        public BoardFrame(Vector2 centre, float halfHeight, float aspect)
        {
            Centre = centre;
            HalfHeight = halfHeight;
            Aspect = aspect;
        }

        /// <summary>Kameranın bakacağı dünya noktası.</summary>
        public Vector2 Centre { get; }

        /// <summary>Kameranın <c>orthographicSize</c> değeri.</summary>
        public float HalfHeight { get; }

        /// <summary>
        /// Hesapta KULLANILAN en boy oranı; okunamayan bir orana yedek verilmişse
        /// yedeğin kendisi.
        /// </summary>
        // ÜÇÜNCÜ ALAN, ÇÜNKÜ YEDEK İKİ KEZ SEÇİLEMEZ: çerçeveyi hesaplayan yer
        // ile 0. katmanın denizini seren yer aynı oranı kullanmak zorunda. Her
        // biri kendi yedeğini seçseydi kamera bir oranda çerçeveler, deniz
        // başka bir oranda serilir ve ekranın kenarı açıkta kalırdı.
        public float Aspect { get; }
    }

    /// <summary>
    /// Tahtayı ekrana oturtur: oynanabilir alan panellerin altında kalmaz.
    /// Panel payları burada, çünkü paneli KURAN araç ile çerçeveyi HESAPLAYAN
    /// kamera aynı sayıyı okumak zorunda.
    /// TUZAK: aşağıdaki sayılar 1920x1080 referansında PİKSEL, dünya birimi değil.
    /// </summary>
    // ██ BU DOSYA BİR SAHİPLİK TAŞINMASIDIR, YENİ BİR HESAP DEĞİL ██
    // Formülün kendisi SceneSetupTool.FrameCamera'dan birebir geldi ve
    // doğrulaması ölçülebilir: 100x50 tahtada bu dosya 34,2439 yarım yükseklik
    // ve (41,63; 20,81) merkez üretiyor — sahnede yazılı olan iki sayının
    // aynısı. Taşınan şey sayılar değil, onları YAZABİLEN yüzeyin tekliği.
    //
    // TAŞINMANIN SEBEBİ ÖLÇÜLDÜ: tahta 100x50'den 5x10'a indi ve kamera eski
    // çerçevede kaldı. Sebep şuydu — çerçeve yalnız Editor menüsü koştuğunda
    // yazılıyordu, yani `width` alanını değiştiren tasarımcı ile çerçeveyi
    // yazan araç arasında hiçbir bağ yoktu. Bugün tahta kendi dikdörtgenini
    // Awake'te duyuruyor ve bu dosya cevabı hesaplıyor.
    //
    // ÇALIŞMA ZAMANI DA OKUYOR — VE BU BİR KARARIN ÇEVRİLMESİDİR.
    // <see cref="BoardViewport"/> içinde şu blok duruyordu ve gerekçesi o gün
    // doğruydu: "panel payları çalışma zamanına inseydi aynı nicelik iki yerde
    // yazılabilir olurdu". Ters çeviren şart şu ölçümle geldi: sahnedeki bir
    // bileşene YENİ bir serileşmiş alan eklemek işe yaramıyor, çünkü sahnede
    // o anahtar yok ve Unity alanı C# başlatıcısıyla değil TİP VARSAYILANIYLA
    // yüklüyor — yani payları rig'e "veri olarak geçirmek" 0 olarak inerdi.
    // Geriye tek dürüst yol kaldı: payların sahibi çalışma zamanına TAŞINSIN,
    // araç da onları okusun. Kopya doğmadı; sahip yer değiştirdi.
    public static class BoardFraming
    {
        // CanvasScaler'ın referans çözünürlüğü. Paneller bu çözünürlükte
        // piksel cinsinden biliniyor; kamera onları orana çevirip kullanıyor.
        public const float ReferenceWidth = 1920f;
        public const float ReferenceHeight = 1080f;

        // Ekran kenarının tek sahibi: her panel bu payla yerleşiyor ve kamera
        // da aynı sayıyla çerçeveliyor. 1920x1080'de ekran yüksekliğinin
        // ~%2,2'si, yani dokunma hedefinin kenardan gerçekten ayrıldığı en
        // küçük değer.
        public const float ScreenMargin = 24f;

        // Üstteki durum şeridinin yüksekliği.
        public const float StatusBarHeight = 64f;

        // Palet düğmesinin ölçüleri ve paletin kaç sütun olduğu.
        public const float EntryWidth = 108f;
        public const float EntryHeight = 122f;
        public const float EntrySpacing = 8f;
        public const int PaletteColumns = 2;

        // Üretim panelinin iç ölçüleri: başlık şeridi ve iç pay.
        public const float PanelHeader = 34f;
        public const float PanelPadding = 8f;

        // Oynanabilir tahtanın çevresinde, panellerin ALTINA GİRMEYEN bir
        // hücrelik nefes payı.
        public const float PlayMargin = 1f;

        // En boy oranı okunamadığında kullanılan yedek. Referans
        // çözünürlükten TÜRETİLİYOR: 16/9 diye yazılsaydı referans bir gün
        // değiştiğinde yedek sessizce başka bir ekranı tarif ederdi.
        private const float FallbackAspect = ReferenceWidth / ReferenceHeight;

        /// <summary>Sol paletin genişliği, piksel.</summary>
        public static float PaletteWidth()
        {
            return PaletteColumns * EntryWidth
                   + (PaletteColumns - 1) * EntrySpacing
                   + 2f * EntrySpacing;
        }

        /// <summary>Alttaki üretim panelinin yüksekliği, piksel.</summary>
        public static float ProductionHeight()
        {
            return EntryHeight + PanelHeader + 2f * PanelPadding;
        }

        /// <summary>Solda panellerin yediği ekran payı, oran olarak.</summary>
        public static float LeftInset()
        {
            return (ScreenMargin + PaletteWidth()) / ReferenceWidth;
        }

        /// <summary>Üstte durum şeridinin yediği ekran payı, oran olarak.</summary>
        public static float TopInset()
        {
            return StatusBarHeight / ReferenceHeight;
        }

        /// <summary>Altta üretim panelinin yediği ekran payı, oran olarak.</summary>
        public static float BottomInset()
        {
            return (ScreenMargin + ProductionHeight()) / ReferenceHeight;
        }

        /// <summary>
        /// Tahtayı ve çevresindeki süsü ekrana sığdıran çerçeve.
        /// </summary>
        /// <param name="board">Oynanabilir tahtanın dünya dikdörtgeni.</param>
        /// <param name="dressedMargin">
        /// Tahtanın DIŞINDA görünmesi gereken süs payı, dünya birimi: kenar
        /// halkası artı 0. katmanın kuşakları.
        /// </param>
        /// <param name="aspect">Şu anki en boy oranı; 0 verilirse yedek kullanılır.</param>
        // ██ İKİ İSTEĞİN BÜYÜĞÜ ALINIYOR, BİRİ DEĞİL ██
        // İSTEK 1 oynanabilir tahta ARTI bir hücre payın, panellerin BOŞ
        // bıraktığı dikdörtgene sığmasıdır. İSTEK 2 ise tahta ARTI bütün süsün
        // TÜM görüntüye sığmasıdır. Yalnız birincisi olsaydı adanın kenarı
        // ekran dışında kalırdı; yalnız ikincisi olsaydı tahtanın altı üretim
        // panelinin altına girerdi.
        //
        // KAYMA PAYDAYA GİRİYOR: kamera panelleri boşaltmak için sola ve aşağı
        // kaymış durumda, yani kaydığı yönde o kadar az görüyor. Kaymayı hesaba
        // katmayan bir "sığar" hesabı adanın üst sırasını ekranın dışında
        // bırakırdı. Mutlak değer, kayma ters yöne döndüğünde de dar kenarı
        // bulsun diye.
        public static BoardFrame Frame(Rect board, float dressedMargin, float aspect)
        {
            float safeAspect = aspect > 0.01f ? aspect : FallbackAspect;

            // BOŞ ALANIN MERKEZİ EKRANIN MERKEZİ DEĞİL: solda ve altta panel
            // var, sağda ve üstte neredeyse yok. Tahtanın boş alanın ortasına
            // oturması için kameranın ters yöne kayması gerekiyor.
            float shiftX = LeftInset() * 0.5f;
            float shiftY = (BottomInset() - TopInset()) * 0.5f;

            float freeWidth = 1f - LeftInset();
            float freeHeight = 1f - TopInset() - BottomInset();

            float playSize = Mathf.Max(
                (board.height + 2f * PlayMargin) / (2f * freeHeight),
                (board.width + 2f * PlayMargin) / (2f * safeAspect * freeWidth));

            float dressedHalfHeight = board.height * 0.5f + dressedMargin;
            float dressedHalfWidth = board.width * 0.5f + dressedMargin;

            float dressedSize = Mathf.Max(
                dressedHalfHeight / (1f - 2f * Mathf.Abs(shiftY)),
                dressedHalfWidth / (safeAspect * (1f - 2f * Mathf.Abs(shiftX))));

            float halfHeight = Mathf.Max(playSize, dressedSize);
            float viewHeight = 2f * halfHeight;

            var centre = new Vector2(
                board.center.x - (shiftX * viewHeight * safeAspect),
                board.center.y - (shiftY * viewHeight));

            return new BoardFrame(centre, halfHeight, safeAspect);
        }
    }
}
