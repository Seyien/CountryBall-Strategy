using GridStrategy.Core;
using NUnit.Framework;

namespace GridStrategy.Tests.EditMode.Core
{
    /// <summary>
    /// Burada sınanan şey bir formül değil, bir SEÇİM: kareli tahtada uzaklık
    /// Chebyshev ile ölçülüyor (çapraz adım da bir adım), Manhattan ile değil.
    ///
    /// İki test bir DAVRANIŞI değil bir KARARI koruyor:
    /// <list type="bullet">
    /// <item>çapraz komşu 1 mi 2 mi → ölçüm metriği kararı</item>
    /// <item>tahta dışı koordinat cevaplanıyor mu → tipin sınırı kararı</item>
    /// </list>
    /// İkisi de kırmızıya dönerse kod hâlâ "çalışıyor" görünür; kırılan şey
    /// menzilin, komşuluğun ve hareketin ne anlama geldiğidir.
    /// </summary>
    public sealed class GridDistanceTests
    {
        [Test]
        public void Between_SameCell_IsZero()
        {
            Assert.That(GridDistance.Between(2, 3, 2, 3), Is.EqualTo(0));
        }

        [TestCase(1, 2, 2, 2, TestName = "Between_OrthogonalNeighbour_Right")]
        [TestCase(1, 2, 0, 2, TestName = "Between_OrthogonalNeighbour_Left")]
        [TestCase(1, 2, 1, 3, TestName = "Between_OrthogonalNeighbour_Up")]
        [TestCase(1, 2, 1, 1, TestName = "Between_OrthogonalNeighbour_Down")]
        public void Between_OrthogonalNeighbour_IsOneStep(int ax, int ay, int bx, int by)
        {
            Assert.That(
                GridDistance.Between(ax, ay, bx, by),
                Is.EqualTo(1),
                "an edge-sharing neighbour is always one step away");
        }

        /// <summary>
        /// ÖLÇÜM KARARINI koruyan test. Çapraz komşu bir adım uzaktadır çünkü
        /// bu oyunda kareler bitişikse çapraz da bitişiktir — bitişiklik
        /// 8 komşu demek, 4 değil.
        ///
        /// Kırmızıya dönerse ölçü Manhattan'a kaymış demektir ve bunun bedeli
        /// bu dosyada bitmez: menzili 1 olan her birim çaprazdaki düşmana
        /// ulaşamaz olur, 3x5'lik tahtada köşe hücrelerin komşu sayısı 3'ten
        /// 2'ye iner ve yapay zekâ bitişikteyken "yaklaşayım" demeye başlar.
        /// </summary>
        [TestCase(1, 2, 2, 3, TestName = "Between_DiagonalNeighbour_UpRight")]
        [TestCase(1, 2, 0, 3, TestName = "Between_DiagonalNeighbour_UpLeft")]
        [TestCase(1, 2, 2, 1, TestName = "Between_DiagonalNeighbour_DownRight")]
        [TestCase(1, 2, 0, 1, TestName = "Between_DiagonalNeighbour_DownLeft")]
        public void Between_DiagonalNeighbour_IsOneStepNotTwo(int ax, int ay, int bx, int by)
        {
            Assert.That(
                GridDistance.Between(ax, ay, bx, by),
                Is.EqualTo(1),
                "a corner-sharing neighbour is one step away under Chebyshev, two under Manhattan");
        }

        [Test]
        public void Between_UnevenGap_IsDecidedByTheLongerAxis()
        {
            // dx = 3, dy = 1. Chebyshev 3 der; Manhattan 4 derdi.
            Assert.That(
                GridDistance.Between(0, 0, 3, 1),
                Is.EqualTo(3),
                "the shorter axis is walked for free while the longer one is being walked");
        }

        [TestCase(0, 0, 2, 4)]
        [TestCase(2, 4, 0, 0)]
        [TestCase(1, 3, 1, 3)]
        public void Between_IsSymmetric(int ax, int ay, int bx, int by)
        {
            Assert.That(
                GridDistance.Between(ax, ay, bx, by),
                Is.EqualTo(GridDistance.Between(bx, by, ax, ay)),
                "distance cannot depend on which cell is asked about first");
        }

        [Test]
        public void Between_NegativeCoordinates_UseTheAbsoluteGap()
        {
            Assert.That(
                GridDistance.Between(-2, -1, 1, 0),
                Is.EqualTo(3),
                "the sign of a coordinate is not part of the gap between two coordinates");
        }

        /// <summary>
        /// SINIR KARARINI koruyan test. Bu tip tahtayı tanımaz: 3x5'lik bir
        /// oyunda bile (9, 9) gibi bir koordinat sorulabilir ve cevaplanır.
        /// "Bu hücre var mı" ayrı bir sorudur ve sahibi UnitGrid.IsInsideGrid.
        ///
        /// Kırmızıya dönerse (ya da derlenmezse) biri bu metoda bir tahta
        /// parametresi ya da bir geçerlilik kontrolü eklemiş demektir — o an
        /// KURAL bir VARLIK'ı tanımak zorunda kalır ve uzaklığı sınamak için
        /// önce bir tahta kurmak gerekir.
        /// </summary>
        [Test]
        public void Between_CoordinatesOutsideAnyBoard_AreStillAnswered()
        {
            Assert.That(
                GridDistance.Between(0, 0, 9, 9),
                Is.EqualTo(9),
                "GridDistance measures a gap; it does not judge whether the cells exist");
            Assert.That(
                GridDistance.Between(-5, 0, -5, 7),
                Is.EqualTo(7),
                "a coordinate off every board still has a well defined distance");
        }
    }
}
