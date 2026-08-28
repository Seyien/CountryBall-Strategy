using NUnit.Framework;
using GridStrategy.Unity;
using UnityEngine;

namespace GridStrategy.Tests.EditMode.Unity
{
    /// <summary>
    /// Kameranın nereye kadar kayabileceğini ve ne kadar yakınlaşabileceğini
    /// sınayan testler.
    ///
    /// SAHNE YOK, KAMERA YOK: sınanan tip motoru hiç çağırmıyor, o yüzden bu
    /// dosya bir Play tuşuna basmadan koşuyor. Kaydırma sınırının "ekranda
    /// gözle bakılarak" doğrulanması gereken bir şey OLMAMASININ sebebi tam
    /// olarak bu ayrım.
    /// </summary>
    public sealed class BoardViewportTests
    {
        // Projenin kendi tahtası: 10x5 hücre. Sayı uydurulmadı — kaydırma
        // kuralının ilginç olmasının sebebi tam da bu tahtanın kameradan KÜÇÜK
        // olması ve testlerin o gerçeği taşıması gerekiyor.
        private static readonly Rect Board = new Rect(0f, 0f, 10f, 5f);

        // ══ EN BOY ORANI — OPERATÖRÜN "%50'DE ORTALANMIYOR" BELİRTİSİ ═══

        /// <summary>
        /// Kurulumdaki oranda hiçbir şey değişmez.
        /// </summary>
        [Test]
        public void FitHalfHeight_AtTheSetupAspect_KeepsTheSetupSize()
        {
            float fit = BoardViewport.FitHalfHeight(
                homeHalfHeight: 5f, homeAspect: 16f / 9f, aspect: 16f / 9f);

            Assert.That(fit, Is.EqualTo(5f).Within(0.0001f));
        }

        /// <summary>
        /// Ekran DARALDIĞINDA kamera açılır — çerçevelenen dünya dikdörtgeni
        /// görünür kalsın diye.
        /// </summary>
        // ██ OPERATÖRÜN BİLDİRDİĞİ KUSUR TAM OLARAK BU HESAPTI ██
        // Eski kodda çerçeveleme Editor aracında BİR KEZ yapılıyor ve bir daha
        // hiç sorulmuyordu; dar bir Game penceresinde kamera aynı yarım
        // yüksekliği koruyor, YATAYDA daha az dünya gösteriyor ve tahtanın
        // kenarları dışarıda kalıyordu. Bu iddia eski kodda yazılamazdı bile —
        // hesap yoktu.
        //
        // SAYININ TÜREYİŞİ: 5 × (16/9) = 8,889 birim yarım genişlik. Kare bir
        // ekranda (oran 1) aynı genişliği göstermenin tek yolu yüksekliğin de
        // 8,889 olması.
        [Test]
        public void FitHalfHeight_OnANarrowerScreen_GrowsToKeepTheFramedRect()
        {
            float fit = BoardViewport.FitHalfHeight(
                homeHalfHeight: 5f, homeAspect: 16f / 9f, aspect: 1f);

            Assert.That(fit, Is.EqualTo(5f * (16f / 9f)).Within(0.0001f));
            Assert.That(fit, Is.GreaterThan(5f), "dar ekranda kamera açılmalı");
        }

        /// <summary>
        /// Ekran GENİŞLEDİĞİNDE kamera daralmaz.
        /// </summary>
        // ASİMETRİ BİLEREK: geniş bir ekranda çerçeve zaten sığıyor ve kamerayı
        // küçültmek tahtayı gereksizce büyütürdü. Kural "dikdörtgen görünsün",
        // "dikdörtgen ekranı tam doldursun" değil.
        [Test]
        public void FitHalfHeight_OnAWiderScreen_DoesNotShrink()
        {
            float fit = BoardViewport.FitHalfHeight(
                homeHalfHeight: 5f, homeAspect: 16f / 9f, aspect: 21f / 9f);

            Assert.That(fit, Is.EqualTo(5f).Within(0.0001f));
        }

        /// <summary>
        /// Anlamsız girdi kurulum değerini olduğu gibi geri verir.
        /// </summary>
        // SIFIR ORAN BİR OYUN OLGUSU DEĞİL, BİR KARE KAZASI: Game penceresi
        // sıfır genişlikte bir kare geçirebiliyor. Bölme yapılsaydı sonsuz bir
        // orthographicSize doğar ve kamera o kareden sonra hiçbir şey
        // göstermezdi.
        [Test]
        public void FitHalfHeight_WithAZeroAspect_ReturnsTheSetupSize()
        {
            Assert.That(
                BoardViewport.FitHalfHeight(5f, 16f / 9f, 0f),
                Is.EqualTo(5f).Within(0.0001f));
        }

        // ══ YAKINLAŞTIRMA SINIRI ════════════════════════════════════════

        [Test]
        public void ClampHalfHeight_AboveTheCeiling_IsPulledDown()
        {
            Assert.That(
                BoardViewport.ClampHalfHeight(10f, minHalfHeight: 2f, maxHalfHeight: 6f),
                Is.EqualTo(6f).Within(0.0001f));
        }

        [Test]
        public void ClampHalfHeight_BelowTheFloor_IsPushedUp()
        {
            Assert.That(
                BoardViewport.ClampHalfHeight(0.5f, minHalfHeight: 2f, maxHalfHeight: 6f),
                Is.EqualTo(2f).Within(0.0001f));
        }

        /// <summary>
        /// Sınırlar TERS yazılmışsa alt sınır kazanır ve kamera kullanılabilir
        /// kalır.
        /// </summary>
        // BU BİR INSPECTOR KAZASININ TESTİ: min 6, max 2 yazılırsa Mathf.Clamp
        // sessizce max'ı döndürür ve kamera tahtanın içine gömülürdü. Sıra
        // burada bilerek seçildi — önce tavan, sonra taban.
        [Test]
        public void ClampHalfHeight_WithInvertedLimits_KeepsTheFloor()
        {
            Assert.That(
                BoardViewport.ClampHalfHeight(4f, minHalfHeight: 6f, maxHalfHeight: 2f),
                Is.EqualTo(6f).Within(0.0001f));
        }
        // ══ İMLECE YAKINLAŞTIRMA ════════════════════════════════════════

        /// <summary>
        /// İmlecin altındaki nokta, yakınlaştıktan sonra AYNI ekran yerinde
        /// kalıyor.
        /// </summary>
        // ██ OPERATÖRÜN BELİRTİSİ: "farenin bulunduğum noktaya doğru olmuyor" ██
        //
        // ██ BU TEST ÖNCE KIRMIZI VERDİ VE HAKLIYDI ██
        // İlk yazdığım beklenti 8'di ve aritmetiği MERKEZDEN başlatmıştım.
        // Doğru taban imleç: ekranda sabit kalması gereken nokta o. Ürün kodu
        // baştan doğruydu, yanlış olan testti — ve bu, testin neden yazıldığının
        // kendisi.
        //
        // TÜRETİM: imleç merkezin 4 birim sağında, yarım yükseklik 8, yani
        // normalize edilmiş kayma 4/8 = 0,5. Yarım yükseklik 4'e inince aynı
        // 0,5'i korumak için kayma 2 birim olmalı, yani merkez 14 - 2 = 12.
        // Merkez SAĞA gidiyor: imlece doğru yakınlaşmanın gözle görülen hâli bu.
        [Test]
        public void ZoomTowards_KeepsThePointerAtTheSameScreenOffset()
        {
            var centre = new Vector2(10f, 10f);
            var pointer = new Vector2(14f, 10f);

            Vector2 moved = BoardViewport.ZoomTowards(centre, pointer, 8f, 4f);

            Assert.That(moved.x, Is.EqualTo(12f).Within(0.0001f));
            Assert.That(moved.y, Is.EqualTo(10f).Within(0.0001f));
            Assert.That(moved.x, Is.GreaterThan(centre.x), "kamera imlece doğru gitmeli");

            // ██ ASIL İDDİA SAYI DEĞİL, ORAN ██
            // Yukarıdaki iki satır formülü sınıyor; bu üç satır KURALI: imlecin
            // merkeze uzaklığının yarım yüksekliğe oranı — yani ekrandaki yeri —
            // değişmedi. Formül bir gün başka türlü yazılsa bile oyuncu için
            // doğru kalması gereken şey bu.
            float before = (pointer.x - centre.x) / 8f;
            float after = (pointer.x - moved.x) / 4f;
            Assert.That(after, Is.EqualTo(before).Within(0.0001f));
        }

        /// <summary>
        /// Uzaklaşırken de aynı kural: imleç yerinde kalır.
        /// </summary>
        // TÜRETİM: imleç merkezin 6 birim üstünde, yarım yükseklik 4, oran 1,5.
        // Yarım yükseklik 8'e çıkınca aynı oranı korumak için kayma 12 olmalı,
        // yani merkez 16 - 12 = 4. Merkez AŞAĞI iniyor, çünkü uzaklaşırken
        // imleci yerinde tutmanın yolu ondan uzaklaşmak.
        [Test]
        public void ZoomTowards_WhenZoomingOut_AlsoHoldsThePointer()
        {
            var centre = new Vector2(10f, 10f);
            var pointer = new Vector2(10f, 16f);

            Vector2 moved = BoardViewport.ZoomTowards(centre, pointer, 4f, 8f);

            Assert.That(moved.y, Is.EqualTo(4f).Within(0.0001f));

            float before = (pointer.y - centre.y) / 4f;
            float after = (pointer.y - moved.y) / 8f;
            Assert.That(after, Is.EqualTo(before).Within(0.0001f));
        }

        /// <summary>
        /// Boy DEĞİŞMEDİYSE kamera hiç kımıldamaz.
        /// </summary>
        // ██ SINIRA DAYANMIŞ TEKERLEĞİN TESTİ ██
        // ApplyZoom kelepçelenmiş boyu veriyor; sınırda eski ile yeni aynı
        // oluyor ve k = 1. İstenen boy verilseydi bu iddia kırmızıya döner,
        // oyuncu sınırdayken tekerleği çevirdikçe harita sürüklenirdi.
        [Test]
        public void ZoomTowards_WithAnUnchangedHalfHeight_DoesNotMoveTheCamera()
        {
            var centre = new Vector2(3f, 7f);

            Vector2 moved = BoardViewport.ZoomTowards(centre, new Vector2(99f, -40f), 5f, 5f);

            Assert.That(moved.x, Is.EqualTo(3f).Within(0.0001f));
            Assert.That(moved.y, Is.EqualTo(7f).Within(0.0001f));
        }

        /// <summary>
        /// İmleç tam merkezdeyse kamera kımıldamaz.
        /// </summary>
        // ESKİ DAVRANIŞIN HÂLÂ DOĞRU OLDUĞU TEK NOKTA: merkeze yakınlaşmak.
        [Test]
        public void ZoomTowards_WithThePointerOnTheCentre_DoesNotMoveTheCamera()
        {
            var centre = new Vector2(5f, 5f);

            Vector2 moved = BoardViewport.ZoomTowards(centre, centre, 8f, 2f);

            Assert.That(moved.x, Is.EqualTo(5f).Within(0.0001f));
            Assert.That(moved.y, Is.EqualTo(5f).Within(0.0001f));
        }

        /// <summary>
        /// Anlamsız eski boy kamerayı olduğu yerde bırakır.
        /// </summary>
        [Test]
        public void ZoomTowards_WithAZeroOldHalfHeight_ReturnsTheCentre()
        {
            var centre = new Vector2(1f, 2f);

            Vector2 moved = BoardViewport.ZoomTowards(centre, new Vector2(9f, 9f), 0f, 4f);

            Assert.That(moved.x, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(moved.y, Is.EqualTo(2f).Within(0.0001f));
        }


        // ══ KAYDIRMA SINIRI — "EN AZINDAN TUĞLALARI GÖRMELİYİZ" ═════════

        /// <summary>
        /// Tahtanın üstündeki bir merkez hiç kımıldamaz.
        /// </summary>
        [Test]
        public void ClampCentre_AlreadyInsideTheBoard_DoesNotMove()
        {
            Vector2 clamped = BoardViewport.ClampCentre(
                new Vector2(5f, 2.5f), Board, halfHeight: 3f, aspect: 2f, minVisible: 2f);

            Assert.That(clamped.x, Is.EqualTo(5f).Within(0.0001f));
            Assert.That(clamped.y, Is.EqualTo(2.5f).Within(0.0001f));
        }

        /// <summary>
        /// Çok uzağa sürüklemek kelepçelenir ve tahtadan tam olarak istenen
        /// kadarı görünür kalır.
        /// </summary>
        // ██ OPERATÖRÜN CÜMLESİ: "en sol köşeye kaydık mesela en sağ üstte ██
        // ██ en azından tuğlaları görmemiz lazım" ██
        // Sayının türeyişi: yarım genişlik 3 × 2 = 6. Üst sınır
        // xMax + halfWidth - minVisible = 10 + 6 - 2 = 14. O merkezde görüş
        // [8, 20] aralığını kaplıyor, tahta [0, 10] ve kesişim tam 2 birim.
        [Test]
        public void ClampCentre_DraggedFarAway_KeepsExactlyTheRequestedOverlap()
        {
            Vector2 clamped = BoardViewport.ClampCentre(
                new Vector2(1000f, 1000f), Board, halfHeight: 3f, aspect: 2f, minVisible: 2f);

            Assert.That(clamped.x, Is.EqualTo(14f).Within(0.0001f));
            Assert.That(clamped.y, Is.EqualTo(6f).Within(0.0001f));

            // İDDİANIN ASIL HÂLİ: sayı değil, ÖRTÜŞME. Yukarıdaki iki satır
            // formülü, bu üç satır KURALI sınıyor — formül değişse bile oyuncu
            // için doğru kalması gereken şey bu.
            float overlapX = Mathf.Min(clamped.x + 6f, Board.xMax) - Mathf.Max(clamped.x - 6f, Board.xMin);
            float overlapY = Mathf.Min(clamped.y + 3f, Board.yMax) - Mathf.Max(clamped.y - 3f, Board.yMin);
            Assert.That(overlapX, Is.EqualTo(2f).Within(0.0001f));
            Assert.That(overlapY, Is.EqualTo(2f).Within(0.0001f));
        }

        /// <summary>
        /// Ters yöne sürüklemek de aynı kuralla kelepçelenir.
        /// </summary>
        [Test]
        public void ClampCentre_DraggedFarTheOtherWay_IsClampedSymmetrically()
        {
            Vector2 clamped = BoardViewport.ClampCentre(
                new Vector2(-1000f, -1000f), Board, halfHeight: 3f, aspect: 2f, minVisible: 2f);

            Assert.That(clamped.x, Is.EqualTo(-4f).Within(0.0001f));
            Assert.That(clamped.y, Is.EqualTo(-1f).Within(0.0001f));
        }

        /// <summary>
        /// Tahta görüş alanından KÜÇÜK olsa bile kaydırma çalışır.
        /// </summary>
        // ██ BU TEST, REDDEDİLEN KLASİK ÇÖZÜMÜN KARAR KAYDIDIR ██
        // Alışılmış kural "görüş dikdörtgeni tahtanın içinde kalsın"dır:
        //     cx = Mathf.Clamp(cx, board.xMin + halfWidth, board.xMax - halfWidth);
        // Burada halfWidth 6, tahta yarısı 5; aralık [6, 4] yani TERS dönüyor ve
        // Clamp tek bir noktaya çöküyor — kaydırma HİÇ olmuyor. Operatörün
        // istediği şey tam da bu küçük tahtada gezebilmek, dolayısıyla o kural
        // bu oyunda yanlış. Bu iddia, klasik çözüm geri getirilirse kırmızıya
        // döner.
        [Test]
        public void ClampCentre_OnABoardSmallerThanTheView_StillMovesWithTheDrag()
        {
            Vector2 clamped = BoardViewport.ClampCentre(
                new Vector2(12f, 2.5f), Board, halfHeight: 3f, aspect: 2f, minVisible: 2f);

            Assert.That(clamped.x, Is.EqualTo(12f).Within(0.0001f),
                "küçük tahtada da kaydırılabilmeli");
        }

        /// <summary>
        /// Çok yakınlaşıldığında kamera tahtanın ORTASINA çekilir.
        /// </summary>
        // ARALIK TERS DÖNDÜĞÜNDE TEK DOĞRU CEVAP BU: görüş alanı istenen
        // örtüşmeden küçükse "en az şu kadarı görünsün" şartı sağlanamaz ve
        // kelepçe kendi kendisiyle çelişir. Kamerayı tahtanın üstünde tutmak,
        // oyuncuyu boşluğa bakar hâlde bırakmaktan iyidir.
        [Test]
        public void ClampCentre_ZoomedInPastTheRequiredOverlap_FallsBackToTheBoardCentre()
        {
            Vector2 clamped = BoardViewport.ClampCentre(
                new Vector2(1000f, 1000f), Board, halfHeight: 1f, aspect: 1f, minVisible: 8f);

            Assert.That(clamped.y, Is.EqualTo(2.5f).Within(0.0001f), "tahtanın dikey ortası");
        }

        /// <summary>
        /// İstenen örtüşme tahtadan büyükse tahtanın kendisi tavan olur.
        /// </summary>
        // BU KAPI OLMASAYDI 5 BİRİMLİK BİR TAHTADA minVisible 8 yazıldığında
        // aralık ters döner ve kamera tahtadan UZAKLAŞMAYA zorlanırdı — yani
        // "daha çok görünsün" ayarı tam tersini yapardı.
        [Test]
        public void ClampCentre_WhenTheRequestedOverlapExceedsTheBoard_UsesTheBoardInstead()
        {
            Vector2 wide = BoardViewport.ClampCentre(
                new Vector2(1000f, 2.5f), Board, halfHeight: 3f, aspect: 2f, minVisible: 40f);

            // required = min(40, 10) = 10 -> upper = 10 + 6 - 10 = 6
            Assert.That(wide.x, Is.EqualTo(6f).Within(0.0001f));
        }
    }
}
