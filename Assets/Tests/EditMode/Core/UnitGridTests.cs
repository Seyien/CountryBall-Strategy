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
    }
}
