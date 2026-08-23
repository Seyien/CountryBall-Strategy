using System;

namespace GridStrategy.Combat
{
    // ═══ ROL: KURAL (Policy) ═════════════════════════════════════════
    // kimlik : yok — static sınıf, iki çağrıyı ayırt edecek bir örnek bile yok
    // hafıza : yok — ResolveRemaining(10, 3) her zaman 7
    // Unity  : gerekmez — girdi iki int; ResolveRemaining(10, 3) çağrısı için
    //          ne sahne ne kare gerekir
    // karar  : hesaplar, uygulamaz — dönen değeri Health'e yazmak çağıranın işi
    /// <summary>
    /// Hasar formülünün tek sahibi. Hiçbir durum tutmaz ve hiçbir duruma
    /// dokunmaz: sayı alır, sayı döndürür. Zırh, direnç, kalkan emilimi ve
    /// kritik vuruş çarpanı geldiğinde değişecek tek yer burasıdır —
    /// <see cref="Health"/> o gün hiç değişmeyecek.
    ///
    /// GEREKÇELER: Docs/deep/kod/Core/Combat/DamageRules.md
    /// </summary>
    // FORMÜLÜN GİRDİ UZAYI SAHİBİNİNKİNDEN GENİŞ, o yüzden kural dışarı çıktı:
    // Health `current < 0` ya da `amount < 0` bölgesine hiç giremez ama kural yine
    // de o bölgede cevap vermek zorunda. Formül Health'in içinde private kalsaydı
    // sınanabilir alan Health'in üretebildiği bölgeye inerdi.
    // → DamageRules.md#damagerules-tip
    public static class DamageRules
    {
        /// <summary>
        /// Bir vuruştan sonra geriye kalan canı hesaplar. Mevcut canı değiştirmez;
        /// yeni değeri yalnızca döndürür — yazma işi çağırana aittir.
        /// </summary>
        // Ön koşullar burada durur: bu metodun girdi uzayı sahibinin (Health)
        // ulaşabildiğinden geniştir, o yüzden negatif yollar da sınanabilir.
        // → DamageRules.md#resolveremainingint-current-int-amount
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

            // ALT KELEPÇE: can sıfırın altına inemez. Yukarıdaki iki `throw`
            // yalnızca GİRDİYİ sınar; `current` ile `amount` negatif olmasa bile
            // FARK negatif olabilir ve o yolu kapatan tek şey bu satır. Kelepçe
            // çağırana bırakılsaydı düzeltme çağıran sayısı kadar kopyalanırdı.
            // → DamageRules.md#resolveremainingint-current-int-amount
            return Math.Max(0, current - amount);
        }
    }
}
