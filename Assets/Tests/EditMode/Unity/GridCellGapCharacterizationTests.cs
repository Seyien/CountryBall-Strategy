using NUnit.Framework;
using UnityEngine;

namespace GridStrategy.Tests.EditMode.Unity
{
    /// <summary>
    /// Unity'nin kendi <see cref="Grid"/> bileşeninin davranışını ölçen bir
    /// CHARACTERIZATION testidir. Bizim kodumuzu değil, üzerine inşa ettiğimiz
    /// varsayımı doğrular.
    ///
    /// Neden var: cellGap eklendiğinde tıklamanın hangi hücreye düştüğü konusunda
    /// iki rakip model vardı. Elle tıklayarak ölçmek nişan hatası taşıdı, bu yüzden
    /// ölçüm buraya taşındı. Play mode, tıklama ve Scene gerekmez.
    ///
    /// Bu test kırmızı olursa Unity'nin davranışı değişmiş demektir; kodumuz değil.
    /// </summary>
    public sealed class GridCellGapCharacterizationTests
    {
        private const float CellSize = 1f;
        private const float CellGap = 0.5f;

        // Bir hücrenin koordinat aralığı cellSize + cellGap kadardır.
        private const float Stride = CellSize + CellGap;

        private GameObject probe;
        private Grid grid;

        [SetUp]
        public void SetUp()
        {
            probe = new GameObject("GridProbe");
            grid = probe.AddComponent<Grid>();
            grid.cellSize = new Vector3(CellSize, CellSize, 0f);
            grid.cellGap = new Vector3(CellGap, CellGap, 0f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(probe);
        }

        [Test]
        public void WorldToCell_InsideTheDrawnArea_ReturnsThatCell()
        {
            Assert.That(CellXAt(0f), Is.EqualTo(0), "Hucrenin sol kenari.");
            Assert.That(CellXAt(0.5f), Is.EqualTo(0), "Hucrenin ortasi.");
            Assert.That(CellXAt(0.999f), Is.EqualTo(0), "Cizilen alanin son pikseli.");
        }

        [Test]
        public void WorldToCell_InTheGapAfterACell_StillReturnsThatCell()
        {
            // Çizilen alan [0, 1]; boşluk [1, 1.5). Boşluk önceki hücreye aittir.
            Assert.That(CellXAt(1.0f), Is.EqualTo(0), "Boslugun basi.");
            Assert.That(CellXAt(1.25f), Is.EqualTo(0), "Boslugun ortasi.");
            Assert.That(CellXAt(1.499f), Is.EqualTo(0), "Boslugun sonu.");
        }

        [Test]
        public void WorldToCell_AtTheStrideBoundary_MovesToTheNextCell()
        {
            Assert.That(CellXAt(Stride), Is.EqualTo(1), "Sinir tam olarak cellSize + cellGap.");
            Assert.That(CellXAt(2f * Stride), Is.EqualTo(2));
        }

        [Test]
        public void WorldToCell_TheGapTrailsEachCell_ItIsNotCentredAroundIt()
        {
            // Bu test iki modeli birbirinden ayırır.
            // Ardına modeli : hücre 0 aralığı [0.00, 1.50)  -> -0.1 dışarıda
            // Ortalanmış    : hücre 0 aralığı [-0.25, 1.25) -> -0.1 hâlâ hücre 0
            Assert.That(
                CellXAt(-0.1f),
                Is.EqualTo(-1),
                "There is no gap to the LEFT of the first cell; the gap trails each cell.");
            Assert.That(
                CellXAt(-0.001f),
                Is.EqualTo(-1),
                "Sifirin hemen solu zaten tahtanin disidir.");
        }

        [Test]
        public void WorldToCell_ReproducesTheFourMeasuredClicks()
        {
            // Ogrenci tarafindan Play mode'da uretilen gercek olcumler.
            // Dördü de "boşluk hücrenin ardına eklenir" modeliyle uyuşur.
            Assert.That(CellXAt(0.103f), Is.EqualTo(0));
            Assert.That(CellXAt(0.515f), Is.EqualTo(0));
            Assert.That(CellXAt(3.592f), Is.EqualTo(2));
            Assert.That(CellXAt(4.377f), Is.EqualTo(2), "4.377 < 4.5 oldugu icin hala hucre 2.");
        }

        private int CellXAt(float worldX)
        {
            return grid.WorldToCell(new Vector3(worldX, 0f, 0f)).x;
        }
    }
}
