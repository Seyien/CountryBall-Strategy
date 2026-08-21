using System;

namespace GridStrategy.Combat
{
    // ═══ ROL: KURAL (Policy) ═════════════════════════════════════════
    // kimlik : yok — static; iki çağrıyı ayıracak bir şey yoktur
    // hafıza : yok — aynı üç sayı her zaman aynı sonucu verir
    // Unity  : gerekmez
    // karar  : hesaplar, yazmaz — yeni canı döndürür, alana dokunmaz
    /// <summary>
    /// İyileştirme formülünün tek sahibi ve <see cref="DamageRules"/>'un aynadaki
    /// eşi. O ALT kelepçeyi taşır (can sıfırın altına inemez), bu ÜST kelepçeyi
    /// (can maksimumu aşamaz).
    ///
    /// Neden aynı sınıfa konmadı: iki kuralın değişme sebepleri farklı. Zırh ve
    /// direnç geldiğinde DamageRules değişir; iyileştirme verimi, "ölüyken
    /// iyileştirilemez" gibi kurallar geldiğinde burası değişir. Aynı dosyada
    /// olsalardı her iki sebep de tek dosyayı oynatırdı.
    /// </summary>
    // REDDEDILEN - HealingRules.cs:37 yerine (ayrı sınıf değil, DamageRules.cs
    //              içinde tek metot; delta işaretiyle iki yön):
    //     public static int ResolveHealth(int current, int max, int delta)
    //     {
    //         // delta < 0 => hasar, delta > 0 => iyileştirme
    //         return Math.Clamp(current + delta, 0, max);
    //     }
    // KIRILAN  : hasar "negatif iyileştirme" olunca amount < 0 doğrulaması imkânsızlaşır.
    //            TakeDamage(-3) artık çağıran hatası değil geçerli bir iyileştirmedir
    //            işaret hatası sessizce can BASAR; kimse bunun için bug açmaz
    //            derleyici: hiçbir şey der  .  test:
    //            HealthTests.TakeDamage_WhenAmountIsNegative_Throws silinmek zorunda kalır
    // KAZANIRDI: zırh, direnç ve "ölüyken iyileştirilemez" gibi kurallar hiç
    //            gelmeyecekse ve iki yönün de tek kuralı "0..max arası kelepçe"
    //            kalacaksa — o durumda iki sınıf, tek formülün iki kat bakımıdır.
    // TEK CUMLE: İki kuralı tek metotta birleştirmek ikisinin AYRI doğrulamalarını
    //            da birleştirir; birleşen doğrulama en gevşek olanıdır.
    public static class HealingRules
    {
        /// <summary>
        /// İyileştirmeden sonraki canı hesaplar. Mevcut canı DEĞİŞTİRMEZ;
        /// yeni değeri döndürür — yazma işi çağırana aittir.
        /// </summary>
        public static int ResolveRestored(int current, int max, int amount)
        {
            if (current < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(current), current, "Current health cannot be negative.");
            }

            if (max <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(max), max, "Max health must be positive.");
            }

            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Heal amount cannot be negative.");
            }

            // Üst kelepçe: can maksimumu aşamaz. Alt kelepçe burada YOK, çünkü
            // bu metot yalnızca artırıyor.
            return Math.Min(max, current + amount);
        }
    }
}
