using NUnit.Framework;
using UnityEngine;

namespace GridStrategy.Tests.EditMode.Unity
{
    /// <summary>
    /// Unity'nin kendi <see cref="Grid"/> bileseninin davranisini olcen bir
    /// CHARACTERIZATION testidir. Bizim kodumuzu degil, uzerine insa ettigimiz
    /// varsayimi dogrular.
    ///
    /// Neden var: cellGap eklendiginde tiklamanin hangi hucreye dustugu konusunda
    /// iki rakip model vardi. Elle tiklayarak olcmek nisan hatasi tasidi, bu yuzden
    /// olcum buraya tasindi. Play mode, tiklama ve Scene gerekmez.
    ///
    /// Bu test kirmizi olursa Unity'nin davranisi degismis demektir; kodumuz degil.
    /// </summary>
    public sealed class GridCellGapCharacterizationTests
    {
        private const float CellSize = 1f;
        private const float CellGap = 0.5f;

        // Bir hucrenin koordinat araligi cellSize + cellGap kadardir.
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
            // Cizilen alan [0, 1]; bosluk [1, 1.5). Bosluk onceki hucreye aittir.
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
            // Bu test iki modeli birbirinden ayirir.
            // Ardina modeli : hucre 0 araligi [0.00, 1.50)  -> -0.1 disarida
            // Ortalanmis    : hucre 0 araligi [-0.25, 1.25) -> -0.1 hala hucre 0
            Assert.That(
                CellXAt(-0.1f),
                Is.EqualTo(-1),
                "Ilk hucrenin SOLUNDA bosluk yoktur; bosluk her hucrenin ardina eklenir.");
            Assert.That(
                CellXAt(-0.001f),
                Is.EqualTo(-1),
                "Sifirin hemen solu zaten tahtanin disidir.");
        }

        [Test]
        public void WorldToCell_ReproducesTheFourMeasuredClicks()
        {
            // Ogrenci tarafindan Play mode'da uretilen gercek olcumler.
            // Dordu de "bosluk hucrenin ardina eklenir" modeliyle uyusur.
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
