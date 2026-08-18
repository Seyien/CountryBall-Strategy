using System;

namespace GridStrategy.Combat
{
    // ═══ ROL: KURAL (Policy) ═════════════════════════════════════════
    // kimlik : yok — static sınıf, iki çağrıyı ayırt edecek bir örnek bile yok
    // hafıza : yok — ResolveRemaining(10, 3) her zaman 7
    // Unity  : gerekmez
    // karar  : hesaplar, uygulamaz — dönen değeri Health'e yazmak çağıranın işi
    /// <summary>
    /// Hasar formülünün tek sahibi.
    /// Hiçbir durum tutmaz ve hiçbir duruma dokunmaz: sayı alır, sayı döndürür.
    /// Zırh, direnç, kalkan emilimi ve kritik vuruş çarpanı geldiğinde değişecek
    /// tek yer burasıdır — <see cref="Health"/> o gün hiç değişmeyecek.
    /// </summary>
    public static class DamageRules
    {
        /// <summary>
        /// Bir vuruştan sonra geriye kalan canı hesaplar. Mevcut canı değiştirmez;
        /// yeni değeri yalnızca döndürür — yazma işi çağırana aittir.
        /// </summary>
        public static int ResolveRemaining(int current, int amount)
        {
            if (current < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(current), current, "Current health cannot be negative.");
            }

            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Damage amount cannot be negative.");
            }

            // Alt kelepçe (clamp): can sıfırın altına inemez.
            // Üst kelepçe burada YOK, çünkü bu metot yalnızca azaltıyor.
            // Heal geldiğinde kendi metodunu ve kendi üst kelepçesini getirecek:
            // Math.Min(max, current + amount).
            return Math.Max(0, current - amount);
        }
    }
}
