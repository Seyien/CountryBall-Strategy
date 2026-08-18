using System;

namespace GridStrategy.Core
{
    // ═══ ROL: VARLIK (Entity) ════════════════════════════════════════
    // kimlik : var — aynı 3x5 ölçüdeki iki tahta aynı tahta değildir;
    //          hangi hücrede kimin durduğu örneğe aittir
    // hafıza : var — PlaceUnit'ten sonra aynı TryGetUnit(1,2) çağrısı
    //          farklı cevap verir
    // Unity  : gerekmez — UnityEngine.Grid koordinat çevirir, bu tip
    //          tahtanın DURUMUNU tutar; aynı kelime, iki ayrı sahip
    // karar  : tutar ve bildirir — IsInsideGrid kural gibi görünse de kendi
    //          ölçüsüne bakan bir sorgudur, dışarıdan gelen bir politika değil
    public sealed class UnitGrid
    {
        private readonly Unit[,] cells;

        public UnitGrid(int width, int height)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be positive.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be positive.");
            }

            cells = new Unit[width, height];
        }

        // Read the array's own shape instead of storing a second copy of it.
        // A stored width could be reassigned and then disagree with the array.
        public int Width => cells.GetLength(0);

        public int Height => cells.GetLength(1);

        public int CellCount => Width * Height;

        public bool IsInsideGrid(int x, int y)
        {
            return x >= 0
                && y >= 0
                && x < cells.GetLength(0)
                && y < cells.GetLength(1);
        }

        // Placing outside the board is a caller defect, not a gameplay outcome,
        // so it fails loudly here while TryGetUnit stays silent for edge scans.
        public void PlaceUnit(int x, int y, Unit unit)
        {
            if (x < 0 || x >= cells.GetLength(0))
            {
                throw new ArgumentOutOfRangeException(nameof(x), x, "x is outside the grid.");
            }

            if (y < 0 || y >= cells.GetLength(1))
            {
                throw new ArgumentOutOfRangeException(nameof(y), y, "y is outside the grid.");
            }

            cells[x, y] = unit;
        }

        // A nullable-return rival (FindUnit) was implemented, compared, and removed.
        // <Nullable> is disabled project-wide, so a null return would carry no
        // compile-time protection while this shape forces the caller to branch.
        // Restore the nullable form if <Nullable> is enabled, or if a caller needs
        // the result inside an expression rather than a branch.
        public bool TryGetUnit(int x, int y, out Unit unit)
        {
            if (!IsInsideGrid(x, y))
            {
                unit = null;
                return false;
            }

            unit = cells[x, y];
            return unit != null;
        }
    }
}
