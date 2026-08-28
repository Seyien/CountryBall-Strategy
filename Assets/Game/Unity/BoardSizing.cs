using UnityEngine;

namespace GridStrategy.Unity
{
    // ═══ ROL: HESAP (saf çeviri) ═════════════════════════════════════
    // kimlik : yok — statik bir sınıf, örneği hiç doğmaz ve tahtada bir
    //          karşılığı bulunmaz
    // hafıza : yok — aynı girdiye her zaman aynı cevabı verir, iki çağrı
    //          arasında hiçbir şey hatırlamaz
    // Unity  : zorunlu ama YÜZEYSEL — yalnızca Sprite ve Vector3 tiplerini
    //          okuyor, MonoBehaviour DEĞİL; bu yüzden EditMode testinde sahne
    //          kurmadan çağrılabiliyor
    // karar  : vermez, ÇEVİRİR — "kaç hücre kaplasın" tasarım niyetini
    //          motorun anladığı localScale'e döndürür
    /// <summary>
    /// Bir görselin tahtada KAÇ HÜCRE kaplayacağını, motorun anladığı
    /// <c>localScale</c> sayısına çeviren tek yer.
    ///
    /// OYUNDA NE İŞE YARAR: bir binanın komşu binayı boyamadan, ama askerden de
    /// büyük görünerek durmasını sağlar. Yan yana iki yapının üst üste binmesi
    /// tam olarak bu hesabın elle yazılmış bir çarpana bırakılmasından doğuyordu.
    ///
    /// HAM ÇARPAN YAZILMAZ, HESAPLANIR — ve ölçü şu: bugünkü sanatın hepsi 16x16
    /// piksel ve içe aktarma <c>spritePixelsToUnits = 16</c>, yani bir görsel tam
    /// bir hücre çiziliyor. Bu tesadüf yüzünden "ölçek 1,6" yazmak ile "1,6 hücre
    /// kaplasın" demek bugün AYNI sonucu veriyor. 32x32 bir görsel geldiği gün
    /// ayrışırlar: yazılı çarpan sessizce iki katına çıkar, buradaki hesap ise
    /// aynı 1,6 hücreyi üretmeye devam eder.
    ///
    /// EN-BOY ORANI KORUNUR: iki eksene ayrı çarpan yazılsaydı kare olmayan bir
    /// görsel ezilir ve kimse bunu bir hata olarak bildirmezdi. Ortak çarpan iki
    /// eksenin KÜÇÜĞÜNDEN seçiliyor, yani görsel istenen hücre kutusunun içine
    /// sığar, taşmaz.
    ///
    /// BOZUK GİRDİDE PATLAMAZ: ölçülemeyen bir görsel karşısında hesap yapmak
    /// yerine ölçeği 1 bırakır. Gerekçe, kusurun görünürlüğü — sıfır ölçek
    /// nesneyi tamamen kaybeder ve hiçbir hata basmaz, ölçek 1 ise en azından
    /// ekranda durur ve yanlışlığı gözle görülür.
    /// </summary>
    public static class BoardSizing
    {
        // Ölçülemeyen bir görselin çizildiği varsayılan boy: TAM BİR HÜCRE.
        // Bu sayı uydurma değil, bu projenin ölçülmüş normu — 16 piksellik karo
        // 16 PPU ile tam bir hücre eder ve bütün tahta sanatı bugün böyle.
        private const float AssumedCells = 1f;

        // Sıfıra bölmeyi ve anlamsız küçük sayıları kesen eşik. Mathf.Epsilon
        // DEĞİL: kayan noktada 1e-38 gibi bir en-boy oranı zaten çizilemez bir
        // görseldir ve onu "geçerli" saymak, sonuçta sonsuza yakın bir ölçek
        // üretirdi.
        private const float Tiny = 0.0001f;

        /// <summary>
        /// Bir görselin istenen hücre sayısı kadar yer kaplaması için gereken
        /// <c>localScale</c>.
        /// </summary>
        /// <param name="sprite">
        /// Çizilecek görsel. <c>null</c> geçmek geçerlidir ve
        /// <see cref="Vector3.one"/> döner — hata basılmaz.
        /// </param>
        /// <param name="sizeInCells">Tahtada kaç hücre kaplasın; 1 = tam bir hücre.</param>
        /// <param name="cellSize">Bir hücrenin dünya birimindeki ölçüsü (Grid.cellSize).</param>
        /// <returns>Z ekseni HER ZAMAN 1; iki düzlem ekseni aynı çarpanı taşır.</returns>
        public static Vector3 LocalScaleFor(Sprite sprite, float sizeInCells, Vector3 cellSize)
        {
            float cellX = Clamp(cellSize.x);
            float cellY = Clamp(cellSize.y);
            float cells = Clamp(sizeInCells);

            // ÖLÇÜLEMEYEN GÖRSELDE ÖLÇEK 1 — ve bu sessiz bir kabul değil, bir
            // duruş: elimizde çizilen boy yoksa hedef boyu tutturduğumuzu İDDİA
            // edemeyiz. Görsel kendi doğal boyunda çizilir, ekranda durur ve
            // yanlışlık gözle görülür.
            if (!TryDrawnSize(sprite, out Vector2 drawn))
            {
                return Vector3.one;
            }

            // KÜÇÜK OLAN KAZANIR: görsel, istenen hücre kutusunun İÇİNE sığar.
            // Büyük olan seçilseydi kare olmayan bir görsel kutudan taşar ve
            // "kaç hücre kaplasın" cümlesi yalan söylerdi.
            float factor = Mathf.Min(cells * cellX / drawn.x, cells * cellY / drawn.y);
            return new Vector3(factor, factor, 1f);
        }

        /// <summary>
        /// Aynı girdiden, görselin ekranda ÇİZİLEN yüksekliğini dünya biriminde
        /// verir.
        /// </summary>
        /// <remarks>
        /// OYUNDA NE İŞE YARAR: can barı gibi başın üstünde duran öğelerin ne
        /// kadar yukarı çıkacağı buradan hesaplanır. Bar, ebeveynin ölçeğinin
        /// tersiyle düzeltildiği hâlde kayması düzeltilmediği için yapıların
        /// üstünde askerinkinden yaklaşık üçte bir hücre daha yukarıda duruyordu.
        ///
        /// TUTARLILIK ŞART, o yüzden ölçek burada yeniden türetilmiyor:
        /// <see cref="LocalScaleFor"/> çağrılıyor. İki hesap ayrı yazılsaydı biri
        /// değiştiğinde bar, görselin başından kopar ve hiçbir şey patlamazdı.
        /// </remarks>
        /// <returns>
        /// Ölçülemeyen görselde hücre yüksekliği döner — asla 0 ya da NaN değil.
        /// </returns>
        public static float WorldHeightFor(Sprite sprite, float sizeInCells, Vector3 cellSize)
        {
            float scaleY = LocalScaleFor(sprite, sizeInCells, cellSize).y;

            if (!TryDrawnSize(sprite, out Vector2 drawn))
            {
                // ÖLÇEK 1 DÖNDÜĞÜ İÇİN BOY DA VARSAYIMDAN GELİYOR: görsel kendi
                // doğal boyunda çiziliyor ve bu projede o boy tam bir hücre.
                return AssumedCells * Clamp(cellSize.y);
            }

            return drawn.y * scaleY;
        }

        /// <summary>
        /// Görselin ölçek 1 iken kapladığı dünya boyutu: <c>rect / pixelsPerUnit</c>.
        /// </summary>
        // TEK KAPI, İKİ ÇAĞIRAN: ölçek ile yükseklik aynı ölçüden doğuyor. Ayrı
        // okunsalardı biri sprite.bounds'a, öteki rect'e bakabilirdi ve ikisi
        // kırpılmış (tight mesh) bir görselde sessizce ayrışırdı.
        private static bool TryDrawnSize(Sprite sprite, out Vector2 drawn)
        {
            drawn = Vector2.one;

            if (sprite == null)
            {
                return false;
            }

            float ppu = sprite.pixelsPerUnit;
            Rect rect = sprite.rect;

            // ÜÇ BOZUK GİRDİ TEK DALDA: sıfır ya da eksi PPU, sıfır enli rect,
            // sıfır boylu rect. Üçünün de sonucu aynı — bu görselin çizilen boyu
            // bilinmiyor — o yüzden üçü de aynı cevabı hak ediyor.
            if (ppu <= Tiny || rect.width <= Tiny || rect.height <= Tiny)
            {
                return false;
            }

            drawn = new Vector2(rect.width / ppu, rect.height / ppu);
            return true;
        }

        /// <summary>
        /// Sıfır ve eksi sayıları 1'e çeker.
        /// </summary>
        // SIFIR HÜCRE ÖLÇÜSÜ EKRANDA HİÇBİR İZ BIRAKMAZ: bütün tahta tek bir
        // noktaya çöker ve konsola tek satır düşmez. Bir hücre varsayılmasının
        // gerekçesi de burada — Grid bileşeninin fabrika ayarı zaten 1.
        private static float Clamp(float value)
        {
            return value > Tiny ? value : 1f;
        }
    }
}
