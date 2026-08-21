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
    /// </summary>
    public static class GridDistance
    {
        /// <summary>
        /// İki hücre arasındaki uzaklık: kaç adımda gidilir.
        /// Çapraz adım da BİR adımdır.
        /// </summary>
        // Tip tahtayı GÖRMEZ, yalnız iki koordinat çifti alır. Tahta dışı
        // sayılar da cevaplanır: (-3, 9) ile (0, 0) arası 9'dur. "Bu hücre
        // var mı" sorusu UnitGrid.IsInsideGrid'in işidir ve iki sorunun tek
        // metotta birleşmesi ikisini de test edilemez hâle getirirdi.
        //
        // REDDEDILEN - GridDistance.cs:50 yerine:
        //     public static int Between(UnitGrid board, int ax, int ay, int bx, int by)
        // KIRILAN  : KURAL bir VARLIK'ı tanımak zorunda kalır; uzaklığı sınamak
        //            için önce bir tahta kurmak gerekir ve GridDistanceTests'teki
        //            her satır "new UnitGrid(...)" ile başlar.
        //            AttackResolver'ın "mesafeyi dışarıda bırakma" gerekçesi de
        //            anlamsızlaşır: mesafeyi almak tahtayı almaya döner.
        //            derleyici: hiçbir şey der  .  test: yeşil kalır ama ağırlaşır
        // KAZANIRDI: uzaklık tahtaya BAĞLI olsaydı — kenarları birbirine dikilmiş
        //            (toroidal) bir haritada iki uç arası 3 genişlikte 1'dir,
        //            çünkü sağ kenardan çıkıp soldan girilir.
        // TEK CUMLE: Ölçü tahtayı almazsa tahtasız sınanabilir; aldığı gün ölçüm
        //            olmaktan çıkıp tahtanın bir davranışı olur.
        public static int Between(int ax, int ay, int bx, int by)
        {
            int dx = Math.Abs(ax - bx);
            int dy = Math.Abs(ay - by);

            // CHEBYSHEV seçildi: en uzun eksen kaç adımsa uzaklık odur, yani
            // çapraz adım da bir adımdır. Gerekçe bu projeye özgü: kareli bir
            // taktik tahta ve AttackProfile.Range = 1 "bitişik hücre" demek.
            // Chebyshev'de bitişik 8 komşudur, Manhattan'da 4. 3x5'lik bir
            // tahtada bu fark küçük değil: Manhattan'da (0,0) köşesinin
            // yalnızca 2 komşusu olur ve menzili 1 olan iki birim çaprazda
            // karşılaşınca birbirine dokunamadan tur harcar.
            //
            // REDDEDILEN - GridDistance.cs:72 yerine:
            //     return dx + dy;                        // Manhattan
            // KIRILAN  : çapraz komşu bir adım değil iki adım uzaklaşır ve
            //            bitişiklik sekiz komşudan dört komşuya iner.
            //            menzili 1 olan birim -> çaprazdaki düşmana ulaşamaz
            //            yapay zekâ bitişikken -> hâlâ "yaklaş" komutu üretir
            //            derleyici: hiçbir şey der  .  test: _IsOneStepNotTwo kırmızı
            // KAZANIRDI: çaprazın BİLEREK yasak olduğu bir oyunda — kuşatma
            //            mekaniği "dört yandan sarıldın" üstüne kuruluysa, ya da
            //            iki duvarın köşesinden geçmek geometrik olarak saçmaysa.
            // TEK CUMLE: Mesafe ölçüsü bir matematik tercihi değil, "bitişik ne
            //            demek" sorusunun cevabıdır; menzil o cevabın üstüne kurulur.
            return Math.Max(dx, dy);
        }
    }
}
