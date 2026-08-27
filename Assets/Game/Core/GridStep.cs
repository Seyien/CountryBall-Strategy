namespace GridStrategy.Core
{
    /// <summary>
    /// Bir yürüyüş yolunun TEK durağı — birimin sırayla basacağı hücrelerden biri.
    ///
    /// OYUNDA NE İŞE YARAR: oyuncu haritada uzak bir noktaya tıkladığında birim
    /// oraya bir çırpıda gitmez; aradaki hücrelere tek tek basarak yürür. Bu tip
    /// o basamaklardan biridir.
    ///
    /// BURADA YAŞAR çünkü yol bir TAHTA kavramıdır; savaşı, sırayı, birimin
    /// durumunu bilmez.
    /// </summary>
    public readonly struct GridStep
    {
        public GridStep(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>Durağın sütunu.</summary>
        public int X { get; }

        /// <summary>Durağın satırı.</summary>
        public int Y { get; }

        public override string ToString()
        {
            return $"({X},{Y})";
        }
    }
}
