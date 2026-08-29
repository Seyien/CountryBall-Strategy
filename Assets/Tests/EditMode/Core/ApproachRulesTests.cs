using System;
using GridStrategy.Core;
using NUnit.Framework;

namespace GridStrategy.Tests.EditMode.Core
{
    /// <summary>
    /// Karşılık vermenin OYUNDAKİ sözünü koruyan testler: "sana vuran birime
    /// karşılık ver, ve kendi menziline girene kadar YÜRÜ".
    ///
    /// Burada sınanan şey bir arama algoritması değil, üç davranış: yakın
    /// dövüşçü saldırganın yanına gidiyor mu, menzilli birim kendi menziline
    /// girip DURUYOR mu, ve kuşatılmış bir hedef için kural sessizce bir hücre
    /// uydurmak yerine "yol yok" diyor mu.
    ///
    /// Dosyanın taşıyıcı iddiası tek satırda: aynı üye hem menzil 1 hem menzil 3
    /// için çağrılıyor ve kodda tür başına bir dal yok. O iddia kırıldığı gün
    /// <c>Plan_SameRuleServesBothRanges_DiffersOnlyByTheNumber</c> kırmızıya
    /// döner.
    /// </summary>
    public sealed class ApproachRulesTests
    {
        private static Unit PlaceNewUnit(UnitGrid board, int x, int y, string name)
        {
            var unit = new Unit(name);
            board.PlaceUnit(x, y, unit);
            return unit;
        }

        [Test]
        public void Plan_MeleeRangeAndAdjacentTarget_StaysPut()
        {
            // Menzili 1 olan birim saldırganın bitişiğindeyse bir adım daha
            // atmamalı. Atsaydı iki birim ekranda birbirinin etrafında sonsuza
            // dek dönerdi.
            var board = new UnitGrid(width: 6, height: 6);
            Unit defender = PlaceNewUnit(board, 2, 2, "defender");
            PlaceNewUnit(board, 3, 2, "attacker");

            ApproachOutcome outcome = ApproachRules.Plan(
                board, defender, targetX: 3, targetY: 2, range: 1, out int cellX, out int cellY);

            Assert.That(outcome, Is.EqualTo(ApproachOutcome.AlreadyInRange));
            Assert.That(cellX, Is.EqualTo(2), "zaten menzildeyken bildirilen hücre birimin KENDİ hücresidir");
            Assert.That(cellY, Is.EqualTo(2));
        }

        [Test]
        public void Plan_MeleeRangeAndDistantTarget_WalksToTheNeighbouringCell()
        {
            // OPERATÖRÜN İSTEDİĞİ DAVRANIŞ TAM OLARAK BU: uzaktan vurulan yakın
            // dövüşçü seyirci kalmıyor, saldırganın yanına yürüyor.
            var board = new UnitGrid(width: 6, height: 6);
            Unit defender = PlaceNewUnit(board, 0, 0, "defender");
            PlaceNewUnit(board, 5, 5, "attacker");

            ApproachOutcome outcome = ApproachRules.Plan(
                board, defender, targetX: 5, targetY: 5, range: 1, out int cellX, out int cellY);

            Assert.That(outcome, Is.EqualTo(ApproachOutcome.MoveTo));
            Assert.That(
                GridDistance.Between(cellX, cellY, 5, 5),
                Is.EqualTo(1),
                "bildirilen hücre hedefin menzil 1 komşuluğunda olmalı");
            Assert.That(cellX, Is.EqualTo(4), "adaylar arasından yürüyene EN YAKIN olanı seçilir");
            Assert.That(cellY, Is.EqualTo(4));
        }

        [Test]
        public void Plan_RangeThreeExactlyAtTheEdge_StaysPut()
        {
            // TAM SINIR: uzaklık menzile EŞİT. Karşılaştırma `<` yazılsaydı okçu
            // menzilinin son hücresinde durup vurmak yerine bir adım daha atar
            // ve gereksiz yere yaklaşırdı.
            var board = new UnitGrid(width: 8, height: 8);
            Unit archer = PlaceNewUnit(board, 0, 0, "archer");
            PlaceNewUnit(board, 3, 3, "attacker");

            ApproachOutcome outcome = ApproachRules.Plan(
                board, archer, targetX: 3, targetY: 3, range: 3, out int cellX, out int cellY);

            Assert.That(outcome, Is.EqualTo(ApproachOutcome.AlreadyInRange));
            Assert.That(cellX, Is.EqualTo(0));
            Assert.That(cellY, Is.EqualTo(0));
        }

        [Test]
        public void Plan_RangeThreeWellInside_DoesNotStepCloser()
        {
            // MENZİLLİ BİRİM YAKLAŞMAZ. Uzaklık 2, menzil 3 — kural burada bir
            // hücre söylerse okçu düşmanın kucağına yürür ve menzilli olmanın
            // oyundaki anlamı kalmaz.
            var board = new UnitGrid(width: 8, height: 8);
            Unit archer = PlaceNewUnit(board, 1, 1, "archer");
            PlaceNewUnit(board, 3, 3, "attacker");

            ApproachOutcome outcome = ApproachRules.Plan(
                board, archer, targetX: 3, targetY: 3, range: 3, out int cellX, out int cellY);

            Assert.That(outcome, Is.EqualTo(ApproachOutcome.AlreadyInRange));
            Assert.That(cellX, Is.EqualTo(1));
            Assert.That(cellY, Is.EqualTo(1));
        }

        [Test]
        public void Plan_TargetSealedInACorner_ReportsUnreachable()
        {
            // Hedefin menzil karesindeki her hücre DOLU. Kural burada bir hücre
            // uydurmak yerine "yol yok" demeli; uydursaydı emir sonsuza dek
            // ulaşılamayan bir hücreye yürümeye çalışırdı.
            var board = new UnitGrid(width: 6, height: 6);
            Unit defender = PlaceNewUnit(board, 5, 5, "defender");
            PlaceNewUnit(board, 0, 0, "attacker");
            PlaceNewUnit(board, 1, 0, "wall-a");
            PlaceNewUnit(board, 0, 1, "wall-b");
            PlaceNewUnit(board, 1, 1, "wall-c");

            ApproachOutcome outcome = ApproachRules.Plan(
                board, defender, targetX: 0, targetY: 0, range: 1, out _, out _);

            Assert.That(outcome, Is.EqualTo(ApproachOutcome.RejectedUnreachable));
        }

        [Test]
        public void Plan_PathBlockedByAWall_ReportsUnreachable()
        {
            // Bu, üsttekinden AYRI bir olgu: hedefin çevresindeki hücreler BOŞ
            // ama oraya yürünemiyor. İkisi tek değere düşüyor çünkü çağıran
            // ikisinde de aynı şeyi yapıyor — emri düşürüyor.
            var board = new UnitGrid(width: 5, height: 5);
            Unit defender = PlaceNewUnit(board, 4, 2, "defender");
            PlaceNewUnit(board, 0, 2, "attacker");
            for (int y = 0; y < 5; y++)
            {
                PlaceNewUnit(board, 2, y, "wall-" + y);
            }

            ApproachOutcome outcome = ApproachRules.Plan(
                board, defender, targetX: 0, targetY: 2, range: 1, out _, out _);

            Assert.That(outcome, Is.EqualTo(ApproachOutcome.RejectedUnreachable));
        }

        [Test]
        public void Plan_TargetOutsideTheBoard_ReportsOffBoard()
        {
            // HEDEF YOK. Emir hedefini tahtadan kalkmış bir kimliğe yazmış
            // olabilir; kural o durumda soruyu cevaplamayı REDDEDER, çünkü
            // tahta dışı bir koordinatın çevresinde aday aramak sessizce boş
            // dönerdi ve "yol yok" ile "hedef yok" aynı cümleye düşerdi.
            var board = new UnitGrid(width: 5, height: 5);
            Unit defender = PlaceNewUnit(board, 2, 2, "defender");

            ApproachOutcome outcome = ApproachRules.Plan(
                board, defender, targetX: 9, targetY: 9, range: 1, out _, out _);

            Assert.That(outcome, Is.EqualTo(ApproachOutcome.RejectedOffBoard));
        }

        [Test]
        public void Plan_MoverNotOnTheBoard_ReportsOffBoard()
        {
            // Yürüyecek olanın kendisi tahtadan kalkmışsa da aynı cevap: soru
            // sorulamıyor. Konum tahtadan okunduğu için bu dal çağıranın
            // sakladığı bayat bir hücreyle ATLANAMAZ.
            var board = new UnitGrid(width: 5, height: 5);
            var ghost = new Unit("ghost");
            PlaceNewUnit(board, 1, 1, "attacker");

            ApproachOutcome outcome = ApproachRules.Plan(
                board, ghost, targetX: 1, targetY: 1, range: 1, out _, out _);

            Assert.That(outcome, Is.EqualTo(ApproachOutcome.RejectedOffBoard));
        }

        [Test]
        public void Plan_SameRuleServesBothRanges_DiffersOnlyByTheNumber()
        {
            // ██ BU DOSYANIN TAŞIYICI TESTİ ██ Aynı tahta, aynı iki hücre, aynı
            // üye — değişen tek şey menzil sayısı. Kural tür başına dallansaydı
            // bu testi geçmek için iki ayrı çağrı yazmak gerekirdi.
            var board = new UnitGrid(width: 12, height: 12);
            Unit defender = PlaceNewUnit(board, 0, 0, "defender");
            PlaceNewUnit(board, 9, 9, "attacker");

            ApproachOutcome melee = ApproachRules.Plan(
                board, defender, targetX: 9, targetY: 9, range: 1, out int meleeX, out int meleeY);
            ApproachOutcome ranged = ApproachRules.Plan(
                board, defender, targetX: 9, targetY: 9, range: 3, out int rangedX, out int rangedY);

            Assert.That(melee, Is.EqualTo(ApproachOutcome.MoveTo));
            Assert.That(ranged, Is.EqualTo(ApproachOutcome.MoveTo));
            Assert.That(GridDistance.Between(meleeX, meleeY, 9, 9), Is.EqualTo(1));
            Assert.That(GridDistance.Between(rangedX, rangedY, 9, 9), Is.EqualTo(3));
            Assert.That(
                meleeX == rangedX && meleeY == rangedY,
                Is.False,
                "menzili uzun olan birim daha erken durmalı");
        }

        [Test]
        public void Plan_DoesNotMoveAnyone()
        {
            // SORU SORMAK TAHTAYI DEĞİŞTİRMEZ. Bu üye kare başına çağrılıyor;
            // her çağrı tahtayı oynatsaydı tek bir karşılık savaşı altüst
            // ederdi.
            var board = new UnitGrid(width: 6, height: 6);
            Unit defender = PlaceNewUnit(board, 0, 0, "defender");
            PlaceNewUnit(board, 5, 5, "attacker");

            ApproachRules.Plan(board, defender, targetX: 5, targetY: 5, range: 1, out _, out _);

            Assert.That(board.TryGetPosition(defender, out int x, out int y), Is.True);
            Assert.That(x, Is.EqualTo(0));
            Assert.That(y, Is.EqualTo(0));
        }

        [Test]
        public void Plan_RangeBelowOne_Throws()
        {
            // Sıfır menzil arama döngüsünü boş gezdirir ve sessizce "yol yok"
            // döndürürdü; AttackProfile kurucusuyla aynı kelepçe burada da
            // gürültü çıkarıyor.
            var board = new UnitGrid(width: 4, height: 4);
            Unit defender = PlaceNewUnit(board, 0, 0, "defender");

            Assert.Throws<ArgumentOutOfRangeException>(
                () => ApproachRules.Plan(board, defender, 2, 2, 0, out _, out _));
        }
    }
}
