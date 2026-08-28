using UnityEngine;

namespace GridStrategy.Unity
{
    /// <summary>
    /// Haritada gezinme: sürükleyerek kaydırma, tekerlekle yakınlaştırma ve
    /// ekranın ne kadar daralırsa daralsın tahtayı çerçevede tutması.
    ///
    /// OYUNDA NE İŞE YARAR: oyuncu tahtayı sürükleyerek gezebilir, tekerlekle
    /// yaklaşıp uzaklaşabilir; ama tahtayı ekrandan kaçıramaz ve pencere boyutu
    /// değiştiğinde tahta kaymaz.
    ///
    /// KURALI KENDİ YAZMAZ: nereye kadar gidilebileceğini ve ne kadar
    /// yakınlaşılabileceğini <see cref="BoardViewport"/> hesaplıyor.
    /// </summary>
    // ██ SOL TUŞ ARTIK KAYDIRIYOR — AMA ONU BU DOSYA OKUMUYOR ██
    // Bir önceki turda burada "sol tuş reddedildi" diye bir blok duruyordu ve
    // gerekçesi doğruydu: sol tuşun tahtada zaten iki anlamı vardı ve ikisi de
    // BASMA karesinde karar veriyordu. O şart bugün ortadan kalktı — seçim
    // BIRAKMA karesine taşındı ve kararı BoardPointerArbiter veriyor.
    // Blok siliniyor çünkü artık dünyayı yanlış tarif ediyor; yerine geçen
    // kural aşağıdaki tek okuyan notu.
    //
    // ██ TEK OKUYAN: SOL TUŞ (0) BU DOSYADA HİÇ OKUNMAZ ██
    // Sol tuşu okuyan tek yer BoardAdapter. Bu tip sol tuşu da okusaydı iki
    // bileşen aynı karede aynı düğmeyi görür ve tahtanın iptal şartları
    // (yerleştirme kipi, arayüzün üstündeki basış) kameraya hiç ulaşmazdı.
    // Kural bir prose değil: panButton alanı [Min(1)] taşıyor ve Inspector'a
    // 0 yazılamıyor; Awake ayrıca kelepçeliyor.
    //
    // KAYDIRMANIN TEK YAZARI ÜÇ KOMUT: BeginPan / ContinuePan / EndPan. Yardımcı
    // tuşu okuyan ReadAuxiliaryPanButton da dışarıdan gelen sol tuş da AYNI üç
    // komuttan geçiyor, yani sürükleme durumunun tek sahibi var.
    [RequireComponent(typeof(Camera))]
    public sealed class BoardCameraRig : MonoBehaviour
    {
        [Header("Board - written by CountryBall > Sahneyi Kur (her şey)")]
        [Tooltip("Tahtanın dünya dikdörtgeni. Kurulum aracı yazar; elle doldurulmaz.")]
        [SerializeField] private Rect boardRect = new Rect(0f, 0f, 10f, 5f);

        [Tooltip("Kurulumda hesaplanan orthographicSize. Kurulum aracı yazar.")]
        [SerializeField] private float homeHalfHeight = 5f;

        [Tooltip("Kurulum anındaki en boy oranı. Kurulum aracı yazar.")]
        [SerializeField] private float homeAspect = 16f / 9f;

        [Tooltip("Kurulumda hesaplanan kamera merkezi. Kurulum aracı yazar.")]
        [SerializeField] private Vector2 homeCentre = new Vector2(5f, 2.5f);

        // YARDIMCI TUŞ, ASIL TUŞ DEĞİL: oyuncunun asıl kaydırma tuşu sol tuştur
        // ve onu BoardAdapter okuyor. Buradaki tuş, tahtası olmayan bir sahnede
        // ya da seçim yapmadan gezinmek isteyen bir geliştirici için duruyor.
        // ALT SINIR 1 BİR KURALDIR, bir zevk değil: 0 yazılabilseydi bu tip ile
        // tahta aynı düğmeyi okur ve tahtanın iptal şartları kameraya ulaşmazdı.
        [Header("Pan - auxiliary mouse button; the left button is driven by the board")]
        [Tooltip("1 sağ, 2 orta. Sol tuş (0) burada okunmaz; onu BoardAdapter okur.")]
        [Min(1)]
        [SerializeField] private int panButton = 1;

        [Tooltip("Kaydırırken tahtadan en az kaç hücre görünür kalsın.")]
        [Min(0.5f)]
        [SerializeField] private float minVisibleCells = 2f;

        [Header("Zoom - mouse wheel")]
        [Tooltip("Tekerleğin bir tıkında yarım yüksekliği kaç birim değiştirdiği.")]
        [Min(0.01f)]
        [SerializeField] private float zoomStep = 0.8f;

        [Tooltip("En yakın hâl: yarım yükseklik bundan küçük olamaz.")]
        [Min(0.5f)]
        [SerializeField] private float minHalfHeight = 2f;

        [Tooltip("En uzak hâl, kurulum çerçevesinin katı olarak. 1 = hiç uzaklaşma.")]
        [Min(1f)]
        [SerializeField] private float maxZoomOutFactor = 1.6f;

        private Camera boardCamera;

        // Sürüklemenin BAŞLADIĞI andaki dünya noktası. Her karede yeniden
        // okunmuyor ve sebebi ölçülebilir: kamera kaydıkça imlecin altındaki
        // dünya noktası da kayar, yani fark her karede sıfıra yakınsar ve
        // sürükleme "ağır" hissettirirdi. Sabit bir tutamak, imlecin altındaki
        // noktayı parmağa yapıştırıyor.
        private Vector3 dragAnchorWorld;
        private bool dragging;

        // Sürüklemeyi süren işaretçinin EN SON bilinen ekran noktası. Alan
        // olmasının sebebi ÇAĞRI SIRASI: sol tuşu okuyan tahta Update'te
        // konuşuyor, kamera ise LateUpdate'te uyguluyor. Input.mousePosition
        // burada ikinci kez okunsaydı iki okuma arasında bir kare açılır ve
        // harita imleçten yarım kare geride kalırdı.
        private Vector2 panPointerScreen;

        // Son görülen en boy oranı. Kare başına yeniden çerçevelemek yerine
        // yalnız DEĞİŞTİĞİNDE hesaplamak için: Game penceresi sabitken bu
        // karşılaştırma bir çarpma bile yapmıyor.
        private float lastAspect = -1f;

        /// <summary>Tahtanın dünya dikdörtgeni; kurulum aracı yazar.</summary>
        // ÜÇ ÜYE DE SETTER, ÇÜNKÜ TEK YAZAN VAR: SceneSetupTool. Alanları public
        // yapmak yerine yazma yolunu adlandırmak, Inspector'dan elle doldurmanın
        // bir HATA olduğunu söylüyor — sayının sahibi araç.
        public void WriteHomeFraming(Rect board, Vector2 centre, float halfHeight, float aspect)
        {
            boardRect = board;
            homeCentre = centre;
            homeHalfHeight = halfHeight;
            homeAspect = aspect;
        }

        /// <summary>
        /// Kaydırma başlasın: verilen ekran noktası tutamak olur ve harita o
        /// noktadan itibaren parmağa yapışır.
        /// </summary>
        // TUTAMAK BASILDIĞI YER DEĞİL, EŞİĞİN AŞILDIĞI YER: tahta bu çağrıyı
        // basış karesinde değil, sürükleme eşiği geçildiğinde yapıyor. Basış
        // noktası verilseydi harita, eşiğin aşıldığı an bir çeyrek hücre
        // ZIPLARDI; bugünkü hâlde o çeyrek hücre bir ölü bölge ve eşiğin zaten
        // istediği şey tam olarak bu.
        public void BeginPan(Vector2 screenPoint)
        {
            if (boardCamera == null)
            {
                return;
            }

            panPointerScreen = screenPoint;
            dragAnchorWorld = boardCamera.ScreenToWorldPoint(screenPoint);
            dragging = true;
        }

        /// <summary>
        /// Kaydırma sürüyor: işaretçinin yeni ekran noktası.
        /// </summary>
        // SÜRÜKLEME BAŞLAMADIYSA SESSİZCE YUTULUYOR: bir kaydırmayı ContinuePan
        // BAŞLATAMAZ, çünkü tutamağı olmayan bir sürükleme kamerayı ilk karede
        // tahtanın öbür ucuna fırlatırdı.
        public void ContinuePan(Vector2 screenPoint)
        {
            if (!dragging)
            {
                return;
            }

            panPointerScreen = screenPoint;
        }

        /// <summary>
        /// Kaydırma bitti: tutamak bırakılır.
        /// </summary>
        // ÇAĞRILMASI HER ZAMAN GÜVENLİ: sürükleme zaten yoksa bu satır hiçbir
        // şey değiştirmez. Hızlı savurmada tahtanın PanBegin'siz gönderdiği
        // PanEnd bu yüzden zararsız.
        public void EndPan()
        {
            dragging = false;
        }

        private void Awake()
        {
            boardCamera = GetComponent<Camera>();

            // SOL TUŞ BU DOSYADA OKUNAMAZ ve kural burada da kilitleniyor:
            // [Min(1)] Inspector'ı kesiyor, bu satır ise eski bir sahnede
            // serileştirilmiş 0'ı kesiyor — öznitelik yalnız yeni yazımı
            // etkiler, zaten diskte duran değeri değiştirmez.
            if (panButton < 1)
            {
                Debug.LogWarning(
                    "[BoardCameraRig] panButton was 0 (left); the left button belongs to the board. Falling back to 1 (right).",
                    this);
                panButton = 1;
            }

            // ORTOGRAFİK ZORUNLU: perspektif bir kamerada orthographicSize
            // hiçbir şey yapmaz ve bütün bu dosya sessizce ölü kalırdı.
            if (!boardCamera.orthographic)
            {
                Debug.LogWarning(
                    "[BoardCameraRig] Camera is not orthographic; pan and zoom limits will not apply.",
                    this);
            }
        }

        private void OnEnable()
        {
            // İLK KARE DE ÇERÇEVELENİYOR: Awake'te bırakılsaydı kurulumdan
            // farklı bir en boy oranıyla açılan ilk kare eski çerçeveyi
            // gösterirdi — operatörün bildirdiği kayma tam olarak o kare.
            lastAspect = -1f;
            dragging = false;
        }

        private void LateUpdate()
        {
            if (boardCamera == null)
            {
                return;
            }

            float aspect = boardCamera.aspect > 0.01f ? boardCamera.aspect : homeAspect;

            // ██ SIRA BİR KARARDIR: ÖNCE ORAN, SONRA TEKERLEK, SONRA SÜRÜKLEME ██
            // Oran, yakınlaştırmanın TAVANINI belirliyor; tekerlek o tavana göre
            // kelepçeleniyor; sürüklemenin sınırı ise ortaya çıkan yarım
            // yüksekliğe bağlı. Ters sırada bir kare boyunca eski tavanla
            // kelepçelenmiş bir kamera görünürdü.
            ApplyAspect(aspect);
            ApplyZoom(aspect);
            ApplyPan(aspect);
        }

        /// <summary>
        /// En boy oranı değiştiyse çerçeveyi yeniden kurar.
        /// </summary>
        private void ApplyAspect(float aspect)
        {
            if (Mathf.Approximately(aspect, lastAspect))
            {
                return;
            }

            lastAspect = aspect;

            float fit = BoardViewport.FitHalfHeight(homeHalfHeight, homeAspect, aspect);

            // YALNIZ BÜYÜTÜYOR, KÜÇÜLTMÜYOR: oyuncu yakınlaşmışken pencere
            // yeniden boyutlandığında onun yakınlaşmasını geri almak, elinden
            // bir kararı almak olurdu. Tavan yükseliyor, kamera olduğu yerde
            // kalıyor — ancak tavanın ALTINA düştüyse yukarı çekiliyor.
            boardCamera.orthographicSize = BoardViewport.ClampHalfHeight(
                Mathf.Max(boardCamera.orthographicSize, fit),
                minHalfHeight,
                fit * maxZoomOutFactor);
        }

        private void ApplyZoom(float aspect)
        {
            float wheel = Input.mouseScrollDelta.y;
            if (Mathf.Approximately(wheel, 0f))
            {
                return;
            }

            float fit = BoardViewport.FitHalfHeight(homeHalfHeight, homeAspect, aspect);

            // SIRA BİR KARARDIR: imlecin dünya noktası, boy DEĞİŞMEDEN önce
            // okunuyor. Sonra okunsaydı zaten kaymış bir noktayı sabitlemeye
            // çalışırdık ve yakınlaştırma imleçten kaçardı.
            Vector3 pointerWorld = boardCamera.ScreenToWorldPoint(Input.mousePosition);
            float oldHalfHeight = boardCamera.orthographicSize;

            // İŞARET TERS: tekerleği ileri itmek YAKINLAŞTIRIR, yani yarım
            // yüksekliği KÜÇÜLTÜR. Aynı yönü kullanan her harita programının
            // deyimi bu ve tersi oyuncuya "bozuk" hissettirir.
            float newHalfHeight = BoardViewport.ClampHalfHeight(
                oldHalfHeight - (wheel * zoomStep),
                minHalfHeight,
                fit * maxZoomOutFactor);

            boardCamera.orthographicSize = newHalfHeight;

            // ██ KELEPÇELENMİŞ BOY KULLANILIYOR, İSTENEN BOY DEĞİL ██
            // Sınıra dayanmış bir tekerlek hareketinde boy hiç değişmiyor
            // (k = 1) ve kamera da hiç kaymıyor. İstenen boy verilseydi kamera,
            // gerçekleşmeyen bir yakınlaşma için kayardı — oyuncu sınırdayken
            // tekerleği çevirdikçe harita sürüklenirdi.
            Vector3 centre = boardCamera.transform.position;
            Vector2 shifted = BoardViewport.ZoomTowards(
                new Vector2(centre.x, centre.y),
                new Vector2(pointerWorld.x, pointerWorld.y),
                oldHalfHeight,
                newHalfHeight);

            // KELEPÇE BURADA YOK: sınırı bir sonraki satırda ApplyPan uyguluyor
            // ve tek sahip olarak kalması gerekiyor. İki yerde kelepçelenseydi
            // biri değiştiği gün ikisi ayrışırdı.
            boardCamera.transform.position = new Vector3(shifted.x, shifted.y, centre.z);
        }

        private void ApplyPan(float aspect)
        {
            ReadAuxiliaryPanButton();

            // KELEPÇE SÜRÜKLEME OLMASA DA KOŞUYOR ve bu bir israf değil, bir
            // ONARIM: tekerlek yakınlaştırdığında görüş alanı küçülür ve dün
            // geçerli olan bir merkez bugün sınırın dışında kalabilir. Yalnız
            // sürüklerken kelepçelenseydi kamera, oyuncu fareyi bırakır bırakmaz
            // sınırın dışında asılı kalırdı.
            Vector3 centre = boardCamera.transform.position;

            if (dragging)
            {
                // TUTAMAK SÜRÜKLEMEDEN SONRA OKUNUYOR: kamera henüz kımıldamadan
                // imlecin dünya noktası alınıyor ve fark, kameranın gitmesi
                // gereken yol. Ters çıkarma, haritayı parmağın YÖNÜNE
                // sürüklüyor — dünyayı iterek, kamerayı değil.
                Vector3 pointerWorld = boardCamera.ScreenToWorldPoint(panPointerScreen);
                centre += dragAnchorWorld - pointerWorld;
            }

            Vector2 clamped = BoardViewport.ClampCentre(
                new Vector2(centre.x, centre.y),
                boardRect,
                boardCamera.orthographicSize,
                aspect,
                minVisibleCells);

            // Z KORUNUYOR: ortografik bir kamerada z görüntüyü değiştirmez ama
            // yakın/uzak düzlemi belirler. Sıfırlansaydı kamera tahtanın
            // düzlemine iner ve hiçbir şey çizilmezdi.
            boardCamera.transform.position = new Vector3(clamped.x, clamped.y, centre.z);
        }

        /// <summary>
        /// Yardımcı kaydırma tuşunu okur ve onu da dışarıdan gelen sol tuşla
        /// AYNI üç komuta çevirir.
        /// </summary>
        // BU METOT BİR ÇEVİRMEN, İKİNCİ BİR YOL DEĞİL — ve fark ölçülebilir:
        // sürükleme durumunu (dragging, dragAnchorWorld, panPointerScreen)
        // yazan tek yer BeginPan/ContinuePan/EndPan üçlüsü kaldı. Eskiden bu
        // satırlar alanlara DOĞRUDAN yazıyordu; sol tuş eklendiğinde aynı
        // alanların ikinci bir yazarı doğacaktı.
        //
        // İKİ KAYNAK AYNI ANDA BASILI OLABİLİR (sol + sağ) ve kazanan SON
        // KONUŞANDIR. Bilinçli: iki parmakla aynı haritayı iki yöne çekmenin
        // doğru bir cevabı yok, kilitlemenin bedeli ise gerçek — sağ tuş
        // basılıyken sol tuşla kaydırmayı denemek sessizce ölürdü.
        private void ReadAuxiliaryPanButton()
        {
            if (Input.GetMouseButtonDown(panButton))
            {
                BeginPan(Input.mousePosition);
            }
            else if (Input.GetMouseButton(panButton))
            {
                ContinuePan(Input.mousePosition);
            }

            // AYRI BİR if, else DEĞİL: tek bir karede hem Down hem Up doğru
            // olabilir ve o karede sürükleme başlayıp bitmelidir.
            if (Input.GetMouseButtonUp(panButton))
            {
                EndPan();
            }
        }
    }
}
