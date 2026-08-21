using System;
using GridStrategy.Core;
using NUnit.Framework;

namespace GridStrategy.Tests.EditMode.Core
{
    public sealed class UnitGridTests
    {
        [Test]
        public void TryGetUnit_WhenCellIsOccupied_ReturnsTrueAndOutputsStoredUnit()
        {
            var grid = new UnitGrid(width: 3, height: 5);
            var kral = new Unit("kral");
            grid.PlaceUnit(1, 2, kral);

            bool found = grid.TryGetUnit(1, 2, out Unit foundUnit);

            Assert.That(found, Is.True, "An occupied cell must report success.");
            Assert.That(
                ReferenceEquals(foundUnit, kral),
                Is.True,
                "The output must identify the exact stored Unit instance, not a copy.");
        }

        [Test]
        public void TryGetUnit_WhenCellIsEmpty_ReturnsFalseAndOutputIsNull()
        {
            var grid = new UnitGrid(width: 3, height: 5);

            bool found = grid.TryGetUnit(0, 0, out Unit foundUnit);

            Assert.That(found, Is.False, "An empty cell is a normal absence, not an error.");
            Assert.That(
                foundUnit,
                Is.Null,
                "The out parameter is still assigned on the failure path; its value is meaningless.");
        }

        [Test]
        public void TryGetUnit_WhenPositionIsOutsideGrid_ReturnsFalseAndOutputIsNull()
        {
            var grid = new UnitGrid(width: 3, height: 5);

            bool found = grid.TryGetUnit(9, 9, out Unit foundUnit);

            Assert.That(found, Is.False, "Outside the board, absence is the expected answer.");
            Assert.That(foundUnit, Is.Null, "Every return path must assign the out parameter.");
        }

        [Test]
        public void Grid_AxesAreNotInterchangeable_WhenWidthAndHeightDiffer()
        {
            // width 3 means x is 0..2; height 5 means y is 0..4.
            var grid = new UnitGrid(width: 3, height: 5);

            Assert.That(
                grid.IsInsideGrid(2, 4),
                Is.True,
                "x=2 is the last valid x and y=4 is the last valid y.");
            Assert.That(
                grid.IsInsideGrid(4, 2),
                Is.False,
                "x=4 exceeds width 3, even though 4 would be a valid y.");
        }

        [Test]
        public void IsInsideGrid_SeparatesOutsideFromEmpty_WhereTryGetUnitCannot()
        {
            var grid = new UnitGrid(width: 3, height: 5);

            Assert.That(
                grid.IsInsideGrid(0, 0),
                Is.True,
                "An empty cell is still a real cell, so placing there is allowed.");
            Assert.That(
                grid.IsInsideGrid(9, 9),
                Is.False,
                "TryGetUnit returns false for both cases; only this member tells them apart.");
        }

        [Test]
        public void WidthAndHeight_ReportTheArrayShape()
        {
            var grid = new UnitGrid(width: 3, height: 5);

            Assert.That(grid.Width, Is.EqualTo(3), "Width is dimension 0.");
            Assert.That(grid.Height, Is.EqualTo(5), "Height is dimension 1.");
            Assert.That(
                grid.IsInsideGrid(grid.Width, grid.Height),
                Is.False,
                "One step past the reported size must already be outside.");
        }

        [Test]
        public void CellCount_IsDerived_NotStoredSeparately()
        {
            var grid = new UnitGrid(width: 3, height: 5);

            Assert.That(grid.CellCount, Is.EqualTo(15), "3 * 5 is computed on each read, so it cannot drift.");
        }

        [Test]
        public void PlaceUnit_WhenPositionIsOutsideGrid_ThrowsArgumentOutOfRange()
        {
            var grid = new UnitGrid(width: 3, height: 5);
            var kral = new Unit("kral");

            Assert.Throws<ArgumentOutOfRangeException>(
                () => grid.PlaceUnit(9, 9, kral),
                "Placing outside the board is a caller defect and must fail loudly.");
        }

        [Test]
        public void Constructor_WhenWidthOrHeightIsNotPositive_ThrowsArgumentOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new UnitGrid(width: 0, height: 5),
                "A grid with no columns is not a valid board.");
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new UnitGrid(width: 3, height: -1),
                "A negative height is a caller defect.");
        }

        [Test]
        public void RemoveUnit_WhenCellIsOccupied_LeavesTheCellEmpty()
        {
            var grid = new UnitGrid(width: 3, height: 5);
            grid.PlaceUnit(1, 2, new Unit("kral"));

            grid.RemoveUnit(1, 2);

            Assert.That(
                grid.TryGetUnit(1, 2, out Unit _),
                Is.False,
                "A removed unit must leave a cell that reports itself as empty.");
        }

        /// <summary>
        /// FELSEFE KARARINI koruyan test. Bu dosyadaki ayırıcı çizgi okuma ile
        /// yazma arasında değil, KOORDİNAT ile İÇERİK arasında: tahta dışı bir
        /// koordinat çağıranın hatasıdır ve gürültüyle patlar, hücrenin içinde
        /// ne olduğu ise bir oyun olgusudur ve sessiz kalır.
        ///
        /// Aynı testte iki iddia var çünkü korunan şey iki davranış değil, tek
        /// bir ayrımdır. Kırmızıya dönerse ayrım bozulmuştur: ya boş hücreyi
        /// boşaltmak bir hataya dönmüştür (o zaman her çağıran silmeden önce
        /// TryGetUnit sormak zorunda kalır), ya da tahta dışı yazma sessizce
        /// yutulmaya başlamıştır.
        /// </summary>
        [Test]
        public void RemoveUnit_IsLoudAboutCoordinates_AndSilentAboutContent()
        {
            var grid = new UnitGrid(width: 3, height: 5);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => grid.RemoveUnit(9, 9),
                "Removing outside the board is a caller defect and must fail loudly.");
            Assert.DoesNotThrow(
                () => grid.RemoveUnit(0, 0),
                "Emptying an already empty cell is a normal no-op, not an error.");
        }

        [Test]
        public void MoveUnit_CarriesTheSameInstance_AndEmptiesTheSource()
        {
            var grid = new UnitGrid(width: 3, height: 5);
            var kral = new Unit("kral");
            grid.PlaceUnit(1, 2, kral);

            grid.MoveUnit(1, 2, 2, 4);

            Assert.That(
                grid.TryGetUnit(1, 2, out Unit _),
                Is.False,
                "The source cell must be empty after the move.");
            Assert.That(
                grid.TryGetUnit(2, 4, out Unit moved) && ReferenceEquals(moved, kral),
                Is.True,
                "The destination must hold the exact same instance, not a copy.");
        }

        [Test]
        public void MoveUnit_ToTheSameCell_KeepsTheUnitInPlace()
        {
            var grid = new UnitGrid(width: 3, height: 5);
            var kral = new Unit("kral");
            grid.PlaceUnit(1, 2, kral);

            grid.MoveUnit(1, 2, 1, 2);

            Assert.That(
                grid.TryGetUnit(1, 2, out Unit stayed) && ReferenceEquals(stayed, kral),
                Is.True,
                "Clearing the source and writing the destination must not cancel each other out.");
        }

        /// <summary>
        /// ATOMİKLİK KARARINI koruyan test. MoveUnit bir RemoveUnit + PlaceUnit
        /// bileşimi DEĞİLDİR; iki koordinat da tek bir hücreye dokunulmadan önce
        /// doğrulanır.
        ///
        /// Kırmızıya dönerse MoveUnit bileşime dönmüş demektir: silme başarılır,
        /// yazma tahta dışı olduğu için patlar ve birim hiçbir hücrede durmayan
        /// bir hayalete döner. Exception yine atılır, yani "hata yakalandı" gibi
        /// görünür — kaybolan şey birimdir.
        /// </summary>
        [Test]
        public void MoveUnit_WhenDestinationIsOutside_ThrowsAndKeepsUnit()
        {
            var grid = new UnitGrid(width: 3, height: 5);
            var kral = new Unit("kral");
            grid.PlaceUnit(1, 2, kral);

            Assert.Throws<ArgumentOutOfRangeException>(() => grid.MoveUnit(1, 2, 9, 9));

            Assert.That(
                grid.TryGetUnit(1, 2, out Unit stayed) && ReferenceEquals(stayed, kral),
                Is.True,
                "A failed move must not delete the unit from the board.");
        }

        [Test]
        public void MoveUnit_WhenSourceIsOutside_Throws()
        {
            var grid = new UnitGrid(width: 3, height: 5);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => grid.MoveUnit(9, 9, 1, 2),
                "An out of board source coordinate is a caller defect, same as the destination.");
        }

        /// <summary>
        /// ATOMİKLİK KARARINI koruyan ikinci test ve bileşimin en sinsi kırılması.
        /// Boş bir kaynaktan taşımak hiçbir şey yapmamalıdır.
        ///
        /// Kırmızıya dönerse MoveUnit boş kaynaktan okuduğu null'ı hedefe
        /// yazıyor demektir — yani hedefte duran birim SESSİZCE silinir. Hiçbir
        /// exception atılmaz, hiçbir log çıkmaz; birim ekrandan kaybolur.
        /// </summary>
        [Test]
        public void MoveUnit_WhenSourceIsEmpty_LeavesDestinationUntouched()
        {
            var grid = new UnitGrid(width: 3, height: 5);
            var kral = new Unit("kral");
            grid.PlaceUnit(2, 4, kral);

            grid.MoveUnit(0, 0, 2, 4);

            Assert.That(
                grid.TryGetUnit(2, 4, out Unit stayed) && ReferenceEquals(stayed, kral),
                Is.True,
                "Moving from an empty cell must not write null over the destination.");
        }

        /// <summary>
        /// SAHİPLİK KARARINI koruyan test. "Dolu hücreye taşınamazsın" bir
        /// KURAL'dır ve sahibi MoveAction'dır; tahta o kuralı bilmez ve üstüne
        /// yazar — tıpkı PlaceUnit'in dolu hücrenin üstüne şikâyetsiz yazması gibi.
        ///
        /// Kırmızıya dönerse aynı kural iki yerde yaşamaya başlamıştır ve ikisi
        /// ayrıştığı gün hangisinin doğru olduğunu derleyici söyleyemez.
        /// </summary>
        [Test]
        public void MoveUnit_WhenDestinationIsOccupied_OverwritesBecauseTheRuleLivesElsewhere()
        {
            var grid = new UnitGrid(width: 3, height: 5);
            var kral = new Unit("kral");
            var asker = new Unit("asker");
            grid.PlaceUnit(1, 2, kral);
            grid.PlaceUnit(1, 3, asker);

            grid.MoveUnit(1, 2, 1, 3);

            Assert.That(
                grid.TryGetUnit(1, 3, out Unit arrived) && ReferenceEquals(arrived, kral),
                Is.True,
                "The board stores what it is told; occupancy is a policy the board does not own.");
        }
    }
}
