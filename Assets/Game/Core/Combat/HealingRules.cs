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
