using System;

namespace GridStrategy.Core
{
    // ═══ ROL: KURAL (Policy) ═════════════════════════════════════════
    // kimlik : yok — static; iki Between çağrısını ayıracak bir şey yoktur
    // hafıza : yok — aynı dört sayı her zaman aynı cevabı verir
    // Unity  : gerekmez — sahne, kamera, Vector2Int bilmez; dört int alır
    // karar  : yalnızca ÖLÇER — "menzile giriyor mu" saldırı kuralının,
    //          "gidebilir mi" MoveAction'ın işidir
    /// <summary>
    /// İki hücre arasındaki uzaklığın TEK sahibi.
    ///
    /// Bu dosyanın var olma sebebi tek cümle: saldırı akışı mesafeyi hazır
    /// alıyordu ama projede o mesafeyi HESAPLAYAN kimse yoktu. AttackResolver
    /// mesafeyi bilerek dışarıda bıraktı ("Manhattan mı, Chebyshev mi" ayrı
    /// bir oyun kuralıdır dedi); işte o ayrı kural burada yaşıyor.
    ///
    /// Neyi BİLMEZ: tahtanın kaç hücre olduğunu, hücrede kimin durduğunu,
    /// arada engel olup olmadığını, sıranın kimde olduğunu. Uzaklık ile
    /// ULAŞILABİLİRLİK farklı sorulardır; burada yalnız ilki cevaplanır.
    ///
    /// GEREKÇELER: Docs/deep/kod/Core/GridDistance.md
    /// </summary>
    public static class GridDistance
    {
        /// <summary>
        /// İki hücre arasındaki uzaklık: kaç adımda gidilir.
        /// Çapraz adım da BİR adımdır.
        /// </summary>
        // Tahtayı ALMAZ, dört int alır: tahta dışı sayılar da cevaplanır.
        // Bir tahta parametresi eklemek KURAL'ı bir VARLIK'a bağlardı ve
        // uzaklığı sınamak için önce bir tahta kurmak gerekirdi. "Bu hücre
        // var mı" ayrı bir sorudur ve sahibi UnitGrid.IsInsideGrid'dir.
        // → GridDistance.md#betweenint-ax-int-ay-int-bx-int-by
        public static int Between(int ax, int ay, int bx, int by)
        {
            int dx = Math.Abs(ax - bx);
            int dy = Math.Abs(ay - by);

            // CHEBYSHEV: en uzun eksen kaç adımsa uzaklık odur, yani çapraz
            // adım da BİR adımdır. Bu bir matematik tercihi değil, "bitişik ne
            // demek" sorusunun cevabı — AttackProfile.Range = 1 onun üstüne
            // kuruluyor. Manhattan'a geçmek bitişikliği sekiz komşudan dörde
            // indirir ve menzili 1 olan birim çaprazdakine ulaşamaz.
            // → GridDistance.md#between-chebyshev
            return Math.Max(dx, dy);
        }
    }
}
