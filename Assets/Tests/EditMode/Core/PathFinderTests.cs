using System.Collections.Generic;
using GridStrategy.Core;
using NUnit.Framework;

namespace GridStrategy.Tests.EditMode.Core
{
    /// <summary>
    /// Yürüyüşün OYUNDAKİ sözünü koruyan testler: "haritanın herhangi bir yerine
    /// tıkla, birim oraya yürüsün".
    ///
    /// Burada sınanan şey A* algoritmasının kendisi değil, oyuncunun gördüğü dört
    /// davranış: uzak hedefe gidilebiliyor mu, engelin ETRAFINDAN dolaşılıyor mu,
    /// kapatılmış bir hedef reddediliyor mu, ve dönen yol gerçekten YÜRÜNEBİLİR
    /// mi (adımlar bitişik mi, çıkış hücresi listede yok mu).
    ///
    /// Sonuncusu en önemlisi: bitişik olmayan bir yol ekranda ışınlanma olarak
    /// görünür ve tam da bu projede düzeltilen kusur oydu.
    /// </summary>
    public sealed class PathFinderTests
    {
        private static Unit PlaceNewUnit(UnitGrid board, int x, int y, string name)
        {
            var unit = new Unit(name);
            board.PlaceUnit(x, y, unit);
            return unit;
        }

        [Test]
        public void TryFindPath_AcrossAnOpenBoard_ReachesTheFarCorner()
        {
            // Menzil kuralı kalktığı için 8x8'lik bir tahtanın bir ucundan
            // ötekine tek tıklamayla gidilebilmeli.
            var board = new UnitGrid(width: 8, height: 8);
            Unit walker = PlaceNewUnit(board, 0, 0, "walker");

            bool found = PathFinder.TryFindPath(board, walker, 0, 0, 7, 7, out List<GridStep> path);

            Assert.IsTrue(found, "Açık bir tahtada uzak köşeye yol bulunmalıydı.");
            Assert.AreEqual(7, path[path.Count - 1].X);
            Assert.AreEqual(7, path[path.Count - 1].Y);
        }

        [Test]
        public void TryFindPath_ReturnedSteps_AreAdjacentAndExcludeTheStart()
        {
            // Bu test ışınlanmanın geri gelmesini engelliyor: her adım bir
            // öncekinin komşusu olmalı, yoksa görsel iki hücre birden atlar.
            var board = new UnitGrid(width: 6, height: 6);
            Unit walker = PlaceNewUnit(board, 1, 1, "walker");

            PathFinder.TryFindPath(board, walker, 1, 1, 4, 3, out List<GridStep> path);

            Assert.IsNotEmpty(path);
            Assert.IsFalse(
                path[0].X == 1 && path[0].Y == 1,
                "Çıkış hücresi yolda olmamalı; birim zaten orada duruyor.");

            int previousX = 1;
            int previousY = 1;
            foreach (GridStep step in path)
            {
                int dx = System.Math.Abs(step.X - previousX);
                int dy = System.Math.Abs(step.Y - previousY);
                Assert.IsTrue(
                    dx <= 1 && dy <= 1 && (dx + dy) > 0,
                    $"Adım {step} bir öncekinin komşusu değil — bu ekranda ışınlanma olur.");
                previousX = step.X;
                previousY = step.Y;
            }
        }

        [Test]
        public void TryFindPath_WithAWallOfUnits_WalksAroundInsteadOfThrough()
        {
            // Tahtayı ikiye bölen bir duvar kur, tek bir kapı bırak. Yol
            // bulunmalı ve duvarın üstünden GEÇMEMELİ.
            var board = new UnitGrid(width: 5, height: 5);
            Unit walker = PlaceNewUnit(board, 0, 0, "walker");

            for (int y = 0; y < 4; y++)
            {
                PlaceNewUnit(board, 2, y, $"wall{y}");
            }

            bool found = PathFinder.TryFindPath(board, walker, 0, 0, 4, 0, out List<GridStep> path);

            Assert.IsTrue(found, "Duvarda açık bir kapı varken yol bulunmalıydı.");
            foreach (GridStep step in path)
            {
                Assert.IsFalse(
                    step.X == 2 && step.Y < 4,
                    $"Yol duvarın içinden geçiyor: {step}");
            }
        }

        [Test]
        public void TryFindPath_WhenTheTargetIsWalledOff_Fails()
        {
            // Hedef tamamen kuşatılmışsa oyuncuya "oraya gidemezsin" denmeli;
            // sessizce başka bir hücreye gitmek bir yalan olurdu.
            var board = new UnitGrid(width: 5, height: 5);
            Unit walker = PlaceNewUnit(board, 0, 0, "walker");

            // (4,4) köşesini üç komşusunu da kapatarak kuşat.
            PlaceNewUnit(board, 3, 4, "block1");
            PlaceNewUnit(board, 3, 3, "block2");
            PlaceNewUnit(board, 4, 3, "block3");

            bool found = PathFinder.TryFindPath(board, walker, 0, 0, 4, 4, out List<GridStep> path);

            Assert.IsFalse(found, "Kuşatılmış hedefe yol bulunmamalıydı.");
            Assert.IsEmpty(path);
        }

        [Test]
        public void TryFindPath_ToAnOccupiedCell_Fails()
        {
            var board = new UnitGrid(width: 4, height: 4);
            Unit walker = PlaceNewUnit(board, 0, 0, "walker");
            PlaceNewUnit(board, 2, 2, "occupant");

            bool found = PathFinder.TryFindPath(board, walker, 0, 0, 2, 2, out List<GridStep> path);

            Assert.IsFalse(found, "Dolu hücreye yürünemez.");
            Assert.IsEmpty(path);
        }

        [Test]
        public void TryFindPath_ToItsOwnCell_Fails()
        {
            // Oyuncu seçili birimin kendi hücresine tıklamış olabilir; bu bir
            // hata değil, yürünecek bir şey olmamasıdır.
            var board = new UnitGrid(width: 4, height: 4);
            Unit walker = PlaceNewUnit(board, 1, 1, "walker");

            bool found = PathFinder.TryFindPath(board, walker, 1, 1, 1, 1, out List<GridStep> path);

            Assert.IsFalse(found);
            Assert.IsEmpty(path);
        }

        [Test]
        public void ExecuteAlongPath_MovesTheUnitAndReportsTheRoute()
        {
            // Tahta ANINDA güncellenir: birim çağrı döndüğünde hedefte durur.
            // Ekranın gecikmeli takibi bu katmanın sorunu değil.
            var board = new UnitGrid(width: 6, height: 6);
            Unit walker = PlaceNewUnit(board, 0, 0, "walker");

            MoveOutcome outcome = MoveAction.ExecuteAlongPath(
                board, walker, 0, 0, 5, 5, out List<GridStep> path);

            Assert.AreEqual(MoveOutcome.Moved, outcome);
            Assert.IsTrue(board.TryGetUnit(5, 5, out Unit arrived));
            Assert.AreSame(walker, arrived);
            Assert.IsFalse(board.TryGetUnit(0, 0, out Unit _), "Eski hücre boşalmalıydı.");
            Assert.IsNotEmpty(path);
        }

        [Test]
        public void ExecuteAlongPath_WhenUnreachable_RejectsAndLeavesTheBoardUntouched()
        {
            var board = new UnitGrid(width: 5, height: 5);
            Unit walker = PlaceNewUnit(board, 0, 0, "walker");
            PlaceNewUnit(board, 3, 4, "block1");
            PlaceNewUnit(board, 3, 3, "block2");
            PlaceNewUnit(board, 4, 3, "block3");

            MoveOutcome outcome = MoveAction.ExecuteAlongPath(
                board, walker, 0, 0, 4, 4, out List<GridStep> _);

            Assert.AreEqual(MoveOutcome.RejectedUnreachable, outcome);
            Assert.IsTrue(board.TryGetUnit(0, 0, out Unit stayed));
            Assert.AreSame(walker, stayed, "Reddedilen hareket tahtada iz bırakmamalı.");
        }

        [Test]
        public void ExecuteAlongPath_OutsideTheBoard_IsRejectedBeforeTheSearchRuns()
        {
            var board = new UnitGrid(width: 4, height: 4);
            Unit walker = PlaceNewUnit(board, 0, 0, "walker");

            MoveOutcome outcome = MoveAction.ExecuteAlongPath(
                board, walker, 0, 0, 9, 9, out List<GridStep> _);

            Assert.AreEqual(MoveOutcome.RejectedInvalidDestination, outcome);
        }
    }
}
