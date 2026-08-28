namespace GridStrategy.Unity
{
    /// <summary>
    /// Sürüklenen bir şeyin, imlecin altındaki hücreye konup konamayacağı.
    ///
    /// OYUNDA NE İŞE YARAR: oyuncu parmağını kaldırmadan ÖNCE görür — hayalet
    /// yeşilse bırakılan şey oraya kurulur, kırmızıysa hiçbir şey olmaz.
    /// </summary>
    // ██ ÜÇ DEĞER, İKİ DEĞİL — VE FARKIN BUGÜN ÇAĞIRANI VAR ██
    // Bir `bool` yetiyormuş gibi görünüyor ("konabilir / konamaz"), ama iki ret
    // sebebi oyuncuya AYRI şeyler söylüyor: tahtanın dışı "içeri gel" demek,
    // dolu hücre "başka yer seç" demek. Bugün ikisini de aynı kırmızı çiziyor
    // ve bu bilinçli bir tasarım tercihi; ayrımın enum'da DURMASI, o tercihin
    // yarın tek satırla değişebilmesi demek.
    //
    // REDDEDİLEN — bool döndürmek:
    //     public bool CanPlaceAt(int x, int y)
    // KIRDIĞI ŞEY: "neden olmadı" sorusunun cevabını çağırana geri kazandırmak
    // için ikinci bir çağrı (IsInsideGrid) gerekirdi ve o çağrı, kuralın
    // ikinci bir yazılabilir kopyası olurdu.
    // NE ZAMAN KAZANIRDI: iki ret sebebinin de sonsuza kadar aynı davranışı
    // ürettiği kesinleşseydi — bugün kesin değil.
    public enum PlacementPreview
    {
        /// <summary>Hücre boş ve tahtanın içinde; bırakılan şey buraya kurulur.</summary>
        Placeable,

        /// <summary>İmleç tahtanın dışında; bırakma bir vazgeçmedir.</summary>
        OutsideBoard,

        /// <summary>Hücrede zaten bir şey duruyor; bırakma reddedilir.</summary>
        CellOccupied,
    }
}
