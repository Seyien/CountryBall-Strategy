using System;
using System.Collections.Generic;

namespace GridStrategy.Core
{
    /// <summary>
    /// "Bu birim şu hücreye YÜRÜYEREK varabilir mi, hangi yoldan?" sorusunun tek
    /// sahibi. A* araması yapar ve basılacak hücreleri sırayla döndürür.
    ///
    /// OYUNDA NE İŞE YARAR: oyuncu haritanın herhangi bir yerine tıklar, birim
    /// dolu hücrelerin ETRAFINDAN dolaşarak oraya yürür. Işınlanma yok, "şu kadar
    /// hücre uzağa gidebilirsin" kısıtı da yok — tek kısıt YOLUN VAR OLMASI.
    ///
    /// BURADA YAŞAR çünkü sorunun cevabı yalnızca tahtaya bakılarak verilir:
    /// hücre içeride mi, dolu mu. Sıra kimde, birim ayakta mı — bunlar başka
    /// katmanların sorusu ve bu dosya onları göremez bile (Core, Combat'ı görmez).
    ///
    /// TUZAK: dönen yol, birimin ŞU AN DURDUĞU hücreyi İÇERMEZ; ilk eleman
    /// atılacak ilk adımdır. Yürüyüş kodu bu yüzden listeyi baştan tüketebilir.
    /// </summary>
    public static class PathFinder
    {
        // Sekiz yön: çapraz gidiş serbest. Referans oyun da böyle yapıyor ve
        // birim köşe dönerken merdiven basamağı gibi kırılmadan yürüyor.
        private static readonly int[] NeighbourX = { 1, -1, 0, 0, 1, 1, -1, -1 };
        private static readonly int[] NeighbourY = { 0, 0, 1, -1, 1, -1, 1, -1 };

        // Düz adım 10, çapraz adım 14 — oran ~1,4, yani kabaca karekök 2. Çapraz
        // adım gerçekten daha uzun olduğu için yol, eşit uzaklıkta düz gitmeyi
        // tercih eder ve yürüyüş göze doğal görünür. Tam sayı seçildi ki maliyet
        // karşılaştırmaları kayan nokta yuvarlamasına takılmasın.
        private const int StraightCost = 10;
        private const int DiagonalCost = 14;

        /// <summary>
        /// Birimin durduğu hücreden hedefe yürüyüş yolunu arar.
        /// </summary>
        /// <param name="mover">
        /// Yürüyecek birim. Kendi durduğu hücre ENGEL SAYILMAZ; sayılsaydı arama
        /// daha ilk adımda kendine çarpıp biterdi.
        /// </param>
        /// <param name="path">
        /// Sırayla basılacak hücreler; hedef son elemandır, çıkış hücresi listede
        /// YOKTUR. Yol bulunamazsa boş liste döner.
        /// </param>
        /// <returns>Hedefe yürünebiliyorsa true.</returns>
        public static bool TryFindPath(
            UnitGrid board,
            Unit mover,
            int fromX,
            int fromY,
            int toX,
            int toY,
            out List<GridStep> path)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (mover == null)
            {
                throw new ArgumentNullException(nameof(mover));
            }

            path = new List<GridStep>();

            // Tahta dışına yol aranmaz: arama alanı zaten oraya uzanmıyor.
            if (!board.IsInsideGrid(fromX, fromY) || !board.IsInsideGrid(toX, toY))
            {
                return false;
            }

            // Zaten oradaysa yürünecek bir şey yok. Bu bir HATA değil: oyuncu
            // seçili birimin kendi hücresine tıklamış olabilir.
            if (fromX == toX && fromY == toY)
            {
                return false;
            }

            // Hedefi başkası tutuyorsa yol aranmaz. Referans oyun burada en yakın
            // boş hücreye kayıyor; biz kaymıyoruz, çünkü oyuncunun tıkladığı
            // yerden başka bir yere gitmek sessiz bir yalan olurdu.
            if (IsBlocked(board, mover, toX, toY))
            {
                return false;
            }

            int width = board.Width;
            int cellCount = width * board.Height;

            var gCost = new int[cellCount];
            var fCost = new int[cellCount];
            var cameFrom = new int[cellCount];
            var closed = new bool[cellCount];
            var opened = new bool[cellCount];

            for (int i = 0; i < cellCount; i++)
            {
                gCost[i] = int.MaxValue;
                cameFrom[i] = -1;
            }

            int startIndex = Index(fromX, fromY, width);
            int goalIndex = Index(toX, toY, width);

            gCost[startIndex] = 0;
            fCost[startIndex] = Heuristic(fromX, fromY, toX, toY);

            var open = new List<int> { startIndex };
            opened[startIndex] = true;

            while (open.Count > 0)
            {
                // Açık kümenin en ucuz düğümü. Düz liste taraması O(n); bu oyunun
                // tahtası birkaç yüz hücre olduğu için öncelik kuyruğu kurmanın
                // karmaşıklığı kazancından büyük olurdu.
                int current = open[0];
                int currentSlot = 0;
                for (int i = 1; i < open.Count; i++)
                {
                    if (fCost[open[i]] < fCost[current])
                    {
                        current = open[i];
                        currentSlot = i;
                    }
                }

                if (current == goalIndex)
                {
                    Rebuild(cameFrom, startIndex, goalIndex, width, path);
                    return true;
                }

                open.RemoveAt(currentSlot);
                opened[current] = false;
                closed[current] = true;

                int cx = current % width;
                int cy = current / width;

                for (int n = 0; n < NeighbourX.Length; n++)
                {
                    int nx = cx + NeighbourX[n];
                    int ny = cy + NeighbourY[n];

                    if (!board.IsInsideGrid(nx, ny))
                    {
                        continue;
                    }

                    int neighbour = Index(nx, ny, width);

                    if (closed[neighbour] || IsBlocked(board, mover, nx, ny))
                    {
                        continue;
                    }

                    int stepCost = NeighbourX[n] != 0 && NeighbourY[n] != 0
                        ? DiagonalCost
                        : StraightCost;

                    int candidate = gCost[current] + stepCost;
                    if (candidate >= gCost[neighbour])
                    {
                        continue;
                    }

                    gCost[neighbour] = candidate;
                    fCost[neighbour] = candidate + Heuristic(nx, ny, toX, toY);
                    cameFrom[neighbour] = current;

                    if (!opened[neighbour])
                    {
                        open.Add(neighbour);
                        opened[neighbour] = true;
                    }
                }
            }

            // Açık küme tükendi: hedef, tahta kenarı ve başka birimlerle çevrili.
            return false;
        }

        // Bir hücre yalnızca BAŞKA bir birim orada dururken engeldir. Kimlik
        // karşılaştırması şart — yürüyenin kendi hücresi engel sayılsaydı arama
        // hiç başlayamazdı.
        private static bool IsBlocked(UnitGrid board, Unit mover, int x, int y)
        {
            return board.TryGetUnit(x, y, out Unit occupant)
                   && !ReferenceEquals(occupant, mover);
        }

        // Octile mesafe: çapraz adımların ucuzluğunu hesaba katan, gerçek maliyeti
        // ASLA aşmayan tahmin. Aşsaydı A* en kısa yolu bulmayı garanti edemez,
        // birim gözle görülür biçimde dolambaçlı yürürdü.
        private static int Heuristic(int ax, int ay, int bx, int by)
        {
            int dx = Math.Abs(ax - bx);
            int dy = Math.Abs(ay - by);
            int diagonal = Math.Min(dx, dy);
            int straight = Math.Max(dx, dy) - diagonal;
            return (DiagonalCost * diagonal) + (StraightCost * straight);
        }

        private static int Index(int x, int y, int width)
        {
            return (y * width) + x;
        }

        // Hedeften geriye zincirlenip ters çevrilir. Çıkış hücresi bilerek listeye
        // alınmaz — birim zaten orada duruyor, oraya "yürümesi" gerekmiyor.
        private static void Rebuild(
            int[] cameFrom,
            int startIndex,
            int goalIndex,
            int width,
            List<GridStep> path)
        {
            int cursor = goalIndex;
            while (cursor != startIndex && cursor != -1)
            {
                path.Add(new GridStep(cursor % width, cursor / width));
                cursor = cameFrom[cursor];
            }

            path.Reverse();
        }
    }
}
