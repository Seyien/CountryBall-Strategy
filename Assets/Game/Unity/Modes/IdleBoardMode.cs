using GridStrategy.Core;

namespace GridStrategy.Unity
{
    //   ═══ BOŞTA — VARSAYILAN KİP ═══════════════════════════════════
    //
    //     Gir()  : hiçbir şey
    //     Cik()  : hiçbir şey
    //     Ilerlet: hiçbir şey
    //     Tıklama: KENDİ YUTMAZ ─► tahtanın sıradan akışı çalışır
    //              (seç / bırak / saldır / yürü)
    //
    //     Buraya varan oklar: yerleştirme bitti, vuruş indi, emir düştü.

    /// <summary>
    /// Hiçbir kipin açık olmadığı hâl: tıklama tahtanın sıradan anlamını
    /// taşır — dolu hücre SALDIRI, boş hücre HAREKET, kendi üstü SEÇİMİ BIRAKIR.
    /// </summary>
    // BOŞ GÖVDELER BİR EKSİKLİK DEĞİL, MAKİNENİN ÇALIŞMA ŞARTI: "kip yok"
    // hâlini null ile temsil etseydik her çağrı noktasına bir null kontrolü
    // düşerdi ve unutulan ilk kontrol NullReferenceException olurdu. Boş
    // nesne, o kontrolü tek bir yerde ve bir kez ödüyor.
    //
    // DURUMSUZ OLDUĞU İÇİN TEK ÖRNEK YETERDİ; yine de static bir örnek
    // sunulmuyor, çünkü tahtanın kendi kipini kendi kurması, iki tahtanın
    // hiçbir şeyi paylaşmadığı kuralını görünür tutuyor.
    public sealed class IdleBoardMode : IBoardMode
    {
        /// <summary>
        /// Boşta kip fareyi sahiplenmez: çerçeve çizilir, sıradan tıklama akışı
        /// çalışır ve dışarıdan gelen hayalet yazarı serbesttir.
        /// </summary>
        public bool OwnsPointer => false;

        /// <summary>Girişte yapılacak iş yok.</summary>
        public void Enter()
        {
        }

        /// <summary>Çıkışta toplanacak iz yok.</summary>
        public void Exit()
        {
        }

        /// <summary>Kare başına iş yok.</summary>
        public void Advance()
        {
        }

        /// <summary>
        /// Hiçbir tıklamayı yutmaz.
        /// </summary>
        /// <param name="clicked">Tıklanan hücredeki kimlik; kullanılmıyor.</param>
        public bool ConsumesClick(Unit clicked)
        {
            return false;
        }
    }
}
