using NUnit.Framework;
using GridStrategy.Unity;
using UnityEngine;

namespace GridStrategy.Tests.EditMode.Unity
{
    /// <summary>
    /// Tahtanın ekrana nasıl oturduğunu sınayan testler.
    ///
    /// SAHNE YOK, KAMERA YOK: sınanan tip motoru hiç çağırmıyor. Bu ayrımın
    /// bedeli de var ve burada açıkça yazılı — bu dosyanın YEŞİL olması
    /// sahnedeki kameranın doğru durduğunu SÖYLEMEZ. Onu ancak Play tuşuna
    /// basan bir insan görebilir.
    /// </summary>
    public sealed class BoardFramingTests
    {
        // Operatörün bugünkü tahtası: 5 geniş, 10 boy. Sayı uydurulmadı —
        // kusur tam olarak bu tahtada görüldü.
        private static readonly Rect SmallBoard = new Rect(0f, 0f, 5f, 10f);

        // Kusurdan ÖNCEKİ tahta. İki tahtayı yan yana sınamanın sebebi şu:
        // bayatlama bir SAYININ yanlışlığı değil, bir sayının DEĞİŞMEMESİydi.
        private static readonly Rect LargeBoard = new Rect(0f, 0f, 100f, 50f);

        private const float Aspect = 16f / 9f;

        // Halka (1 hücre) artı üç kuşak. Elle 2,45 yazılmadı: kuşaklar
        // genişletildiği gün bu sayı da kendiliğinden büyüsün.
        private const float DressedMargin = 1f + WorldBackdrop.IslandMargin;

        // ══ ÇERÇEVE TAHTAYI TAKİP EDİYOR MU ══════════════════════════════

        /// <summary>
        /// Küçük tahta küçük çerçeve ister; büyük tahta büyüğünü.
        /// </summary>
        // ██ OPERATÖRÜN BELİRTİSİ: "tahtayı 5x10 yaptım, kamera eski ██
        // ██ çerçevede kaldı" ██
        // Bu iddia eski kodda YAZILAMAZDI: çerçeveyi hesaplayan gövde bir
        // Editor menüsünün içindeydi ve testten çağrılamıyordu. Sınanabilir
        // olması, saf bir tipe ayrılmasının tek ölçülebilir kazancı.
        [Test]
        public void Frame_OnASmallerBoard_ShrinksTheView()
        {
            BoardFrame small = BoardFraming.Frame(SmallBoard, DressedMargin, Aspect);
            BoardFrame large = BoardFraming.Frame(LargeBoard, DressedMargin, Aspect);

            Assert.That(small.HalfHeight, Is.LessThan(large.HalfHeight),
                "5x10 tahta, 100x50 tahtadan daha dar bir çerçeve istemeli");
        }

        /// <summary>
        /// Bugünkü sahnede yazılı olan iki sayı, bu hesabın ürettiğinin aynısı.
        /// </summary>
        // ██ TAŞINMANIN KANITI ██ Formül SceneSetupTool'dan buraya taşındı ve
        // taşınırken değişmediğini gösteren tek şey bu test. Sayılar sahne
        // dosyasından okundu: homeHalfHeight 34,2439 ve homeCentre
        // (41,62927; 20,814634).
        [Test]
        public void Frame_OnTheOldBoard_MatchesTheNumbersWrittenInTheScene()
        {
            BoardFrame frame = BoardFraming.Frame(LargeBoard, DressedMargin, Aspect);

            Assert.That(frame.HalfHeight, Is.EqualTo(34.2439f).Within(0.001f));
            Assert.That(frame.Centre.x, Is.EqualTo(41.62927f).Within(0.001f));
            Assert.That(frame.Centre.y, Is.EqualTo(20.814634f).Within(0.001f));
        }

        // ══ İSTEK 1 — OYNANABİLİR TAHTA PANELİN ALTINA GİRMESİN ══════════

        /// <summary>
        /// Oynanabilir tahtanın tamamı, panellerin boş bıraktığı dikdörtgende.
        /// </summary>
        // İDDİA GÖZLE DEĞİL HESAPLA KURULUYOR: görüş dikdörtgeninden panel
        // payları düşülüyor ve tahtanın dört kenarı da kalan alanın içinde mi
        // diye soruluyor. "Ekranda güzel duruyor" cümlesinin makine karşılığı
        // tam olarak bu.
        [Test]
        public void Frame_KeepsThePlayableBoardOutOfThePanels()
        {
            BoardFrame frame = BoardFraming.Frame(SmallBoard, DressedMargin, Aspect);

            float halfWidth = frame.HalfHeight * Aspect;
            float viewLeft = frame.Centre.x - halfWidth;
            float viewRight = frame.Centre.x + halfWidth;
            float viewBottom = frame.Centre.y - frame.HalfHeight;
            float viewTop = frame.Centre.y + frame.HalfHeight;

            float freeLeft = viewLeft + (BoardFraming.LeftInset() * 2f * halfWidth);
            float freeBottom = viewBottom + (BoardFraming.BottomInset() * 2f * frame.HalfHeight);
            float freeTop = viewTop - (BoardFraming.TopInset() * 2f * frame.HalfHeight);

            Assert.That(SmallBoard.xMin, Is.GreaterThanOrEqualTo(freeLeft),
                "tahtanın solu paletin altında kalmamalı");
            Assert.That(SmallBoard.xMax, Is.LessThanOrEqualTo(viewRight),
                "tahtanın sağı ekranın dışında kalmamalı");
            Assert.That(SmallBoard.yMin, Is.GreaterThanOrEqualTo(freeBottom),
                "tahtanın altı üretim panelinin altında kalmamalı");
            Assert.That(SmallBoard.yMax, Is.LessThanOrEqualTo(freeTop),
                "tahtanın üstü durum şeridinin altında kalmamalı");
        }

        // ══ İSTEK 2 — SÜSÜN TAMAMI GÖRÜNSÜN ═════════════════════════════

        /// <summary>
        /// Tahta artı halka artı kuşaklar, görüntünün tamamına sığar.
        /// </summary>
        [Test]
        public void Frame_KeepsTheWholeIslandOnScreen()
        {
            BoardFrame frame = BoardFraming.Frame(SmallBoard, DressedMargin, Aspect);

            float halfWidth = frame.HalfHeight * Aspect;

            Assert.That(frame.Centre.x - halfWidth,
                Is.LessThanOrEqualTo(SmallBoard.xMin - DressedMargin + 0.001f));
            Assert.That(frame.Centre.x + halfWidth,
                Is.GreaterThanOrEqualTo(SmallBoard.xMax + DressedMargin - 0.001f));
            Assert.That(frame.Centre.y - frame.HalfHeight,
                Is.LessThanOrEqualTo(SmallBoard.yMin - DressedMargin + 0.001f));
            Assert.That(frame.Centre.y + frame.HalfHeight,
                Is.GreaterThanOrEqualTo(SmallBoard.yMax + DressedMargin - 0.001f));
        }

        // ══ BOZUK GİRDİ ══════════════════════════════════════════════════

        /// <summary>
        /// Okunamayan bir en boy oranı çerçeveyi öldürmez, yedeğe düşürür.
        /// </summary>
        // KAMERA HENÜZ BİR KARE ÇİZMEDİYSE aspect 0 gelebiliyor ve o hâlde
        // bölme sonsuz üretirdi. Yedek, referans çözünürlüğün oranı.
        [Test]
        public void Frame_WithAnUnreadableAspect_FallsBackToTheReferenceRatio()
        {
            BoardFrame fallback = BoardFraming.Frame(SmallBoard, DressedMargin, 0f);
            BoardFrame explicitRatio = BoardFraming.Frame(
                SmallBoard, DressedMargin,
                BoardFraming.ReferenceWidth / BoardFraming.ReferenceHeight);

            Assert.That(fallback.HalfHeight,
                Is.EqualTo(explicitRatio.HalfHeight).Within(0.0001f));
            Assert.That(fallback.Aspect,
                Is.EqualTo(explicitRatio.Aspect).Within(0.0001f));
        }

        // ══ 0. KATMAN — KUŞAKLAR TAHTAYLA BİRLİKTE KÜÇÜLÜYOR MU ══════════

        /// <summary>
        /// Küçük tahtanın kuşağında desen kabalaşmaz: ölçek tam 1.
        /// </summary>
        // KÜÇÜK TAHTADA ÖLÇEK 1 KALMALI: 1'in üstüne çıkarsa aynı karo daha
        // büyük çizilir ve 0. katman tahtanın ritmini kaybeder. 5x10 tahtanın
        // en büyük kuşağı bile bütçenin çok altında.
        [Test]
        public void TileScaleFor_OnASmallBoardBand_StaysAtOne()
        {
            Sprite tile = OneCellTile();

            Assert.That(WorldBackdrop.TileScaleFor(tile, new Vector2(8.6f, 13.6f)),
                Is.EqualTo(1f).Within(0.0001f));
        }

        /// <summary>
        /// Mesh bütçesini aşan bir alanda karo BÜYÜR, alan kırpılmaz.
        /// </summary>
        // ██ TAVANIN SINANDIĞI YER ██ Operatör tahtayı 100x50 yaptığında Unity
        // "Requires 161872 vertices" deyip denizi HİÇ çizmemişti. Ölçek
        // büyümesi o sessiz kaybın tek panzehiri.
        [Test]
        public void TileScaleFor_AboveTheBudget_GrowsTheTile()
        {
            Sprite tile = OneCellTile();

            float scale = WorldBackdrop.TileScaleFor(tile, new Vector2(400f, 220f));

            Assert.That(scale, Is.GreaterThan(1f));

            // BÜTÇE GERÇEKTEN TUTUYOR MU: ölçeklenmiş karonun kapladığı alanla
            // yeniden sayılan karo adedi bütçenin altında kalmalı.
            float tiles = (400f / scale) * (220f / scale);
            Assert.That(tiles, Is.LessThanOrEqualTo(WorldBackdrop.TileBudget + 0.001f));
        }

        /// <summary>
        /// Ölçülemeyen karoda ölçek 1 döner; hesap yapılmaz.
        /// </summary>
        [Test]
        public void TileScaleFor_WithNoTile_StaysAtOne()
        {
            Assert.That(WorldBackdrop.TileScaleFor(null, new Vector2(8f, 13f)),
                Is.EqualTo(1f).Within(0.0001f));
        }

        /// <summary>
        /// Bu projenin karosu: 16x16 piksel, 16 PPU, yani tam bir hücre.
        /// </summary>
        // SAYI UYDURULMADI: bütün tahta sanatı bugün 16x16 ve içe aktarma
        // spritePixelsToUnits = 16. Karo bir hücreden başka bir boy çizseydi
        // bütçe hesabı da başka bir sayı verirdi.
        private static Sprite OneCellTile()
        {
            var texture = new Texture2D(16, 16);
            return Sprite.Create(texture, new Rect(0f, 0f, 16f, 16f), Vector2.zero, 16f);
        }
    }
}
