using UnityEngine;
using UnityEngine.SceneManagement;

namespace GridStrategy.Unity
{
    /// <summary>
    /// Tahtanın bittiği yerde görünen dünya: kumsal, sığlık ve açık deniz.
    /// Kuşakların boyu TAHTADAN türüyor, çünkü donmuş bir boy tahta
    /// küçüldüğünde ekranı kaplayan bir kum tarlasına dönüşüyor.
    /// TUZAK: hiçbiri oyunun kuralına girmez — çarpıştırıcı yok, hücre yok.
    /// </summary>
    // ██ OPERATÖRÜN ŞİKÂYETİ: "ekranı bir çöl kapladı" ██
    // ÇÖL DEĞİLDİ: 100x50 bir tahta için serilmiş 102,6x52,6 birimlik KUMSAL
    // plakasıydı. Tahta 5x10'a indirildi, plakaların boyu sahnede yazılı
    // kaldığı için hiç değişmedi ve kameranın gördüğü her piksel kumsalın
    // içinde kaldı. Oyuncunun okuduğu şey ise bir kusur değil bir SÖZDÜ:
    // "burada açılacak toprak var." Öyle bir özellik yok.
    //
    // KUŞAK GENİŞLİKLERİ TAHTAYLA ÖLÇEKLENMİYOR, VE BU BİR KARAR: kumsal her
    // tahtada 0,3 hücre. Tahtayla ölçeklenseydi büyük bir haritada kum yine
    // ekranı kaplar ve aynı yanlış söz geri gelirdi. Ölçeklenmesi gereken tek
    // şey plakanın SERİLDİĞİ ALAN — o da tahtadan hesaplanıyor.
    public static class WorldBackdrop
    {
        // 0. katmanın kök nesnesinin adı. ADIYLA BULUNUYOR ÇÜNKÜ HİÇBİR ALAN
        // ONU GÖSTERMİYOR: bir alan eklemek sahneye yeni bir anahtar yazmayı,
        // yani kurulum menüsünün yeniden koşmasını gerektirirdi — oysa bu
        // düzeltmenin ÖLÇÜSÜ tam olarak menüsüz doğru açılmaktır.
        public const string RootName = "WorldBackdrop";

        // Kuşakların adları. Çalışma zamanı bu adlarla var olan plakayı bulup
        // ÜSTÜNE YAZIYOR; yeniden kurmuyor.
        public const string SeaName = "OpenSea";
        public const string ShoalName = "Shoal";
        public const string BeachName = "Beach";

        // Kumsal tam 0,3 hücre: kenar hücresine konmuş en büyük yapının taşma
        // payının birebir aynısı, yani taşan yapı kumun üstünde duruyor.
        // Sığlık onun iki buçuk katı — adayı denizden ayıracak kadar geniş,
        // ekranı yiyecek kadar değil.
        public const float BeachWidth = 0.3f;
        public const float ShoalWidth = 0.8f;

        // Kuşakların dışında kalması gereken en az deniz. Sıfır olsaydı sığlık
        // ekranın kenarına dayanır ve "adanın çevresi su" izlenimi kaybolurdu.
        public const float SeaSliver = 0.35f;

        // Kameranın adaya bırakması gereken pay: tam olarak üç kuşağın
        // toplamı. Elle yazılmış bir sayı olsaydı — ve ilk yazıldığında öyleydi
        // — bir kuşak genişletildiği gün kamera eski payla kalır, kuşak
        // sessizce ekranın dışına taşardı.
        public const float IslandMargin = BeachWidth + ShoalWidth + SeaSliver;

        // Denizin kameradan kaç kat büyük serileceği. TAHTAYA DEĞİL KAMERAYA
        // BAĞLI, çünkü denizin tek işi ekranın kalanını kapatmak. 2,2 kat,
        // 16:9'dan 3,9:1'e kadar bütün en boy oranlarında ekranı kaplıyor.
        public const float SeaOversize = 2.2f;

        // Tek bir döşeli kuşağın çizebileceği en çok karo; gerekçesi
        // TileScaleFor'da yazılı.
        public const float TileBudget = 8000f;

        // 0. KATMANIN ÇİZİM SIRASI. BoardAdapter'ın sırası yazılı: zemin 0,
        // birim ve yapı 1, imleç çerçevesi 2, can barı 3, kenar halkası -1.
        // Buradaki üçü o ikisinin de ALTINDA ve aralarında onar birim var:
        // araya yeni bir kuşak girdiğinde numaraları kaydırmak gerekmesin.
        public const int SeaOrder = -40;
        public const int ShoalOrder = -30;
        public const int BeachOrder = -20;

        // Üçü de AYNI beyaz karoyu boyuyor; aralarındaki tek fark renk ve
        // kuşağın boyu. ÖLÇÜLMÜŞ REFERANS: Fatihcill/CountryBall-Strategy'nin
        // kamerası açık gök mavisiyle temizleniyor ve bütün palet o maviye
        // akraba seçildi.
        public static readonly Color SeaColor = new Color(0.243f, 0.561f, 0.769f, 1f);
        public static readonly Color ShoalColor = new Color(0.404f, 0.753f, 0.871f, 1f);
        public static readonly Color BeachColor = new Color(0.949f, 0.851f, 0.651f, 1f);

        // Kameranın arka planı denizin bir tık AÇIĞI. Deniz zaten ekranı
        // kaplıyor, yani bu renk normalde hiç görünmüyor; görüldüğü tek an çok
        // geniş bir ekranda denizin bittiği yer ve orada ufuk gibi okunması
        // isteniyor.
        public static readonly Color SkyColor = new Color(0.271f, 0.588f, 0.788f, 1f);

        private const float FallbackAspect =
            BoardFraming.ReferenceWidth / BoardFraming.ReferenceHeight;

        private const float Tiny = 0.0001f;

        /// <summary>
        /// Sahnede DURAN kuşakların boyunu tahtaya göre yeniden yazar.
        /// </summary>
        /// <param name="board">Oynanabilir tahtanın dünya dikdörtgeni.</param>
        /// <param name="ringThickness">Kenar halkasının kaç hücre olduğu.</param>
        /// <param name="frame">Kameranın gideceği çerçeve; deniz ona serilir.</param>
        // YENİDEN KURMUYOR, ÜSTÜNE YAZIYOR — ve bu ikisinin farkı bir karede
        // görünürdü: eski kök yok edilip yenisi kurulsaydı Unity yok etmeyi
        // karenin SONUNA bıraktığı için bayat kumsal bir kare boyunca yeni
        // kuşakların üstünde kalırdı.
        //
        // REDDEDILEN - kuşakları çalışma zamanında sıfırdan kurmak.
        //     var go = new GameObject(RootName);
        //     var tile = Resources.Load<Sprite>("terrain_water_16x16");
        // KIRILAN: karo yalnız AssetDatabase üzerinden, yani Editor'da
        // okunabiliyor. Çalışma zamanında bulmanın tek yolu onu Resources
        // klasörüne taşımaktı ve o taşıma, sahnede zaten atanmış olan aynı
        // görselin ikinci bir yükleme yolunu açardı.
        // KAZANIRDI: 0. katmanı hiç kurulmamış bir sahnenin Play'de kendini
        // toparlaması istenseydi — bugün istenmiyor, çünkü katmanı kuran araç
        // sahneyi zaten bir kez yazıyor.
        // TEK CUMLE: çalışma zamanı var olan plakanın BOYUNU düzeltir, yokluğunu
        // gidermez.
        public static void Refresh(Rect board, float ringThickness, BoardFrame frame)
        {
            Transform root = FindRoot();
            if (root == null)
            {
                Debug.Log(
                    $"[WorldBackdrop] Sahnede '{RootName}' yok; 0. katman atlandı. " +
                    "CountryBall > Sahneyi Kur onu bir kez kurar.");
                return;
            }

            Apply(root, null, board, ringThickness, frame);
        }

        /// <summary>
        /// Kuşakları verilen karoyla kurar; zaten duruyorlarsa boyunu yazar.
        /// </summary>
        /// <param name="root">Kuşakların ebeveyni.</param>
        /// <param name="tile">Üç kuşağın da boyanacağı karo.</param>
        public static void Build(
            Transform root, Sprite tile, Rect board, float ringThickness, BoardFrame frame)
        {
            if (root == null || tile == null)
            {
                return;
            }

            Apply(root, tile, board, ringThickness, frame);
        }

        /// <summary>
        /// Kuşakların dünya ölçüsünü hesaplar ve üçünü de yerine oturtur.
        /// </summary>
        // ADA = TAHTA ARTI HALKA, ÖTESİ DENİZ. Kuşaklar halkanın DIŞINDAN
        // başlıyor; içeriden başlasalardı toprağın üstüne binerlerdi.
        private static void Apply(
            Transform root, Sprite tile, Rect board, float ringThickness, BoardFrame frame)
        {
            Vector2 islandCentre = board.center;
            float islandWidth = board.width + (2f * ringThickness);
            float islandHeight = board.height + (2f * ringThickness);

            // ██ DENİZ HESAPLANMIŞ ÇERÇEVEYİ OKUYOR, KAMERANIN O ANKİ HÂLİNİ ██
            // ██ DEĞİL ██
            // Kamera çerçeveye LateUpdate'te gidiyor, oysa bu üye Awake'te
            // koşuyor. `view.orthographicSize` okunsaydı deniz, bir sonraki
            // karede terk edilecek olan BAYAT boya göre serilir ve ilk karede
            // ekranın kenarı açıkta kalırdı.
            //
            // DENİZ KAMERAYA GÖRE ORTALANIYOR, TAHTAYA GÖRE DEĞİL: kamera
            // panelleri boşaltmak için sola ve aşağı kaymış durumda ve deniz
            // tahtaya göre ortalansaydı kameranın kaydığı yönde ekranın kenarı
            // açıkta kalırdı.
            Vector2 seaCentre = frame.Centre;
            float seaHeight = 2f * frame.HalfHeight * SeaOversize;
            float seaAspect = frame.Aspect > 0.01f ? frame.Aspect : FallbackAspect;

            Plate(root, SeaName, tile, SeaColor,
                seaCentre, new Vector2(seaHeight * seaAspect, seaHeight), SeaOrder);

            Plate(root, ShoalName, tile, ShoalColor, islandCentre,
                new Vector2(islandWidth + (2f * ShoalWidth), islandHeight + (2f * ShoalWidth)),
                ShoalOrder);

            Plate(root, BeachName, tile, BeachColor, islandCentre,
                new Vector2(islandWidth + (2f * BeachWidth), islandHeight + (2f * BeachWidth)),
                BeachOrder);
        }

        /// <summary>
        /// Tek bir kuşağı bulur ya da kurar, sonra boyunu yazar.
        /// </summary>
        // KARO GERİLMİYOR, DÖŞENİYOR. Ölçek verilseydi 16 pikselik desen ekran
        // boyunda tek bir lekeye dönerdi; SpriteDrawMode.Tiled aynı karoyu yan
        // yana tekrarlıyor ve deseni hücre ölçüsünde tutuyor — yani 0. katman
        // ile tahta AYNI ritmi paylaşıyor.
        private static void Plate(
            Transform root, string name, Sprite tile, Color color,
            Vector2 centre, Vector2 size, int order)
        {
            Transform found = root.Find(name);
            if (found == null)
            {
                if (tile == null)
                {
                    return;
                }

                var created = new GameObject(name);
                created.transform.SetParent(root, worldPositionStays: false);
                found = created.transform;
            }

            var renderer = found.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = found.gameObject.AddComponent<SpriteRenderer>();
            }

            if (tile != null)
            {
                renderer.sprite = tile;
            }

            // GÖRSELSİZ PLAKA ÖLÇÜLEMEZ: karo bilinmeden döşeme sayısı da
            // bilinemez, yani ölçek hesabı anlamsız olurdu. Sessiz geçilmiyor —
            // ekranda görünmeyen bir kuşağın sebebi konsolda yazsın.
            if (renderer.sprite == null)
            {
                Debug.LogWarning(
                    $"[WorldBackdrop] '{name}' kuşağının görseli yok; boyu yazılmadı.",
                    renderer);
                return;
            }

            found.position = new Vector3(centre.x, centre.y, 0f);

            float scale = TileScaleFor(renderer.sprite, size);
            found.localScale = new Vector3(scale, scale, 1f);

            renderer.color = color;

            // SIRA ÖNEMLİ: size yalnız döşeme kipinde yazılabiliyor, kip Simple
            // kaldığı sürece sessizce yok sayılıyor.
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.tileMode = SpriteTileMode.Continuous;

            // BOY YEREL UZAYDA YAZILIYOR: transform ölçeği onu zaten çarpıyor,
            // yani istenen dünya boyunu almanın tek yolu o çarpanı burada
            // bölmek. Aynı düzeltme can barında da var ve gerekçesi orada yazılı.
            renderer.size = size / scale;
            renderer.sortingOrder = order;
        }

        /// <summary>
        /// Döşeli bir kuşağın karo sayısını mesh bütçesinin altında tutan ölçek.
        /// </summary>
        /// <returns>1 ya da daha büyük bir çarpan; bütçe aşılmıyorsa tam 1.</returns>
        // ██ TAVAN: 8000 KARO = 32000 KÖŞE, motorun sınırı 65535 ██
        // HANGİ SAYI: BoardAdapter'ın Inspector'daki width/height alanları;
        // plakanın alanı onlardan türüyor. BUGÜN: 5x10 tahta, en büyük kuşak
        // deniz ve karo sayısı bütçenin çok altında — ölçek 1. TAVAN NASIL
        // KURULDU: operatör tahtayı 100x50 yaptığında Unity şunu bastı —
        // "Requires 161872 vertices and 242808 indices" — ve deniz HİÇ
        // çizilmedi. YUKARISINI KİM DEVRALIR: bu üye; ölçeği büyütüyor, alanı
        // kırpmıyor, çünkü denizin var olma sebebi ekranın kenarını kapatmak.
        //
        // BÜTÇE TAVANIN YARISI: Unity'nin kendi iç payları (kenar karoları,
        // yuvarlama) sayıyı birkaç yüz artırabiliyor ve tam tavana yaslanan bir
        // bütçe bir gün sessizce aşardı.
        //
        // KAREKÖK, BÖLME DEĞİL: karo sayısı ALANLA büyüyor, yani ölçeği k
        // yapmak sayıyı k KARE kadar azaltıyor. Doğrudan bölünseydi büyük
        // tahtada bütçe yine aşılırdı.
        public static float TileScaleFor(Sprite tile, Vector2 size)
        {
            if (tile == null)
            {
                return 1f;
            }

            Vector2 tileWorld = tile.bounds.size;
            if (tileWorld.x <= Tiny || tileWorld.y <= Tiny)
            {
                return 1f;
            }

            float tiles = (size.x / tileWorld.x) * (size.y / tileWorld.y);
            if (tiles <= TileBudget)
            {
                return 1f;
            }

            return Mathf.Sqrt(tiles / TileBudget);
        }

        /// <summary>
        /// 0. katmanın kök nesnesini sahnenin köklerinde arar.
        /// </summary>
        // GetRootGameObjects, GameObject.Find DEĞİL: ikincisi KAPALI nesneyi
        // bulamaz ve kapalı bırakılmış bir 0. katman, açıldığı gün yine bayat
        // boyla açılırdı. Arama Awake'te bir kez yapılıyor, kare başına değil.
        private static Transform FindRoot()
        {
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] != null && roots[i].name == RootName)
                {
                    return roots[i].transform;
                }
            }

            return null;
        }
    }
}
