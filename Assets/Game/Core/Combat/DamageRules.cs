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
    // REDDEDILEN - DamageRules.cs:31 yerine (formül Health.cs içinde private kalsaydı):
    //     private int ResolveRemaining(int amount)
    //     {
    //         return Math.Max(0, current - amount);
    //     }
    // KIRILAN  : formülün sınır durumları yalnızca bir Health nesnesi üzerinden
    //            sınanabilir; Health'in asla giremediği durumlar hiç sınanamaz.
    //            ResolveRemaining(-1, 3) sözleşme testi yazılamaz -> negatif yol kör kalır
    //            derleyici: hiçbir şey der  .  test: DamageRulesTests ve
    //            DamageRulesAllocationTests'in formül testleri derlenemez
    // KAZANIRDI: kural dörtten fazla alanı aynı anda okumak zorunda kalırsa — current,
    //            Max, zırh, kalkan — o gün parametre listesi uzar ve kural örnek
    //            metoduna döner; geri dönüş ucuz (ABSOLUTE_F): dosyayı sil, metodu geri taşı.
    // TEK CUMLE: Bir formülü sahibinin İÇİNDE tutmak, formülü sahibinin
    //            ulaşabildiği durumlarla sınırlar.
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
            // REDDEDILEN - DamageRules.cs:68 yerine (kelepçe çağırana bırakılsaydı):
            //     return current - amount;
            //     // ...ve Health.TakeDamage içinde, atamadan SONRA:
            //     current = DamageRules.ResolveRemaining(current, amount);
            //     if (current < 0) { current = 0; }
            // KIRILAN  : alt kelepçe formülün SÖZLEŞMESİ olmaktan çıkar, çağıranın işi olur.
            //            Health dışından çağıran herkes düzeltmeyi kendi eliyle tekrarlar
            //            bugün testler, yarın baraka -> biri unutur, can eksiye iner
            //            derleyici: hiçbir şey der  .  test:
            //            ResolveRemaining_NeverReturnsNegative süpürmesi kırmızıya döner
            // KAZANIRDI: aşırı hasarın MİKTARI bir yerde okunmak zorunda kalırsa — baraka
            //            yıkımı "canı ne kadar aştı" değerini yıkım şiddeti olarak
            //            kullanacaksa, ham fark kelepçeyle silinmemelidir.
            // TEK CUMLE: Bir değişmez onu üreten yerde tutulur; çağırana bırakılan
            //            değişmez, çağıran sayısı kadar kopyalanır.
            return Math.Max(0, current - amount);
        }
    }
}
