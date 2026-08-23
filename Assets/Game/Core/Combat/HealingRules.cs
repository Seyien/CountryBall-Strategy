using System;

namespace GridStrategy.Combat
{
    // ═══ ROL: KURAL (Policy) ═════════════════════════════════════════
    // kimlik : yok — static; iki çağrıyı ayıracak bir şey yoktur
    // hafıza : yok — aynı üç sayı her zaman aynı sonucu verir
    // Unity  : gerekmez — girdi üç int; ResolveRestored(7, 10, 5) çağrısı
    //          için ne sahne ne kare gerekir
    // karar  : hesaplar, yazmaz — yeni canı döndürür, alana dokunmaz
    /// <summary>
    /// İyileştirme formülünün tek sahibi ve <see cref="DamageRules"/>'un
    /// aynadaki eşi. O ALT kelepçeyi taşır (can sıfırın altına inemez), bu ÜST
    /// kelepçeyi (can maksimumu aşamaz).
    ///
    /// Neden aynı sınıfa konmadı: iki kuralın DEĞİŞME SEBEPLERİ farklı. Zırh ve
    /// direnç geldiğinde DamageRules değişir; iyileştirme verimi, "ölüyken
    /// iyileştirilemez" gibi kurallar geldiğinde burası değişir.
    ///
    /// GEREKÇELER: Docs/deep/kod/Core/Combat/HealingRules.md
    /// </summary>
    // AYRI SINIF, DamageRules'un aynadaki eşi: o ALT kelepçeyi taşır, bu ÜST
    // kelepçeyi. Tek metotta işaretli bir delta ile birleşselerdi `amount < 0`
    // doğrulaması YAZILAMAZ olurdu — negatif delta geçerli bir çağrıya döner ve
    // TakeDamage(-3) bir çağıran hatası olmaktan çıkıp sessizce can basardı.
    // → HealingRules.md#healingrules-tip
    public static class HealingRules
    {
        /// <summary>
        /// İyileştirmeden sonraki canı hesaplar. Mevcut canı DEĞİŞTİRMEZ;
        /// yeni değeri döndürür — yazma işi çağırana aittir.
        /// </summary>
        // Üç ön koşul burada durur çünkü bu metodun girdi uzayı sahibinin
        // (Health) ulaşabildiğinden geniştir. `amount < 0` YÖNÜ korur,
        // `Math.Min(max, ...)` SINIRI korur; ikisi ayrı ayrı kırılır.
        // → HealingRules.md#resolverestoredint-current-int-max-int-amount
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
