using System;

namespace GridStrategy.Combat
{
    // ═══ ROL: VARLIK (Entity) ════════════════════════════════════════
    // kimlik : var — aynı Max ile kurulan iki Health aynı can değildir;
    //          her biri tek bir savaşçıya aittir
    // hafıza : var — current çağrılar arasında yaşar; aynı TakeDamage(3)
    //          ikinci kez farklı bir sonuç bırakır
    // Unity  : gerekmez — düz C#, noEngineReferences: true
    // karar  : vermez, uygular — formül DamageRules'a ait, burada yalnızca yazma var
    /// <summary>
    /// Tek bir savaşçının can durumu.
    /// Yalnızca sayıyı tutar ve değiştirir; ölümden SONRA ne olacağını bilmez.
    /// Sahneden silinme, ödül, animasyon ve tahtadan çıkarma kararları bu tipin
    /// dışında verilir — böylece can kuralı Unity olmadan test edilebilir.
    /// </summary>
    public sealed class Health
    {
        private int current;

        public Health(int max)
        {
            if (max <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(max), max, "Max health must be positive.");
            }

            Max = max;
            current = max;
        }

        public int Max { get; }

        public int Current => current;

        public bool IsAlive => current > 0;

        // Hasarın TEK giriş noktası. Bilerek "Current" için bir setter yok:
        // "canı 3 yap" ile "3 hasar al" farklı niyetlerdir ve yalnızca ikincisi
        // sıfırın altına inmeme kuralını taşımak zorundadır. Setter olsaydı
        // çağıran o kuralı atlayabilirdi ve kod yine derlenirdi.
        public void TakeDamage(int amount)
        {
            // Girdi doğrulaması bilerek burada DEĞİL: formül ile onun geçerli
            // girdi aralığı aynı sahibe aittir (DamageRules). İki yerde kontrol
            // etseydik kural iki yerde yaşardı ve biri değişince diğeri sessizce
            // eskiyebilirdi. Bu tip sapmalar derleme hatası vermez.
            current = DamageRules.ResolveRemaining(current, amount);
        }

        // TakeDamage'in aynadaki esi. Ayni desen: formul disarida, yazma burada.
        // Girdi dogrulamasi yine burada DEGIL - o da kuralin sahibine ait.
        public void Heal(int amount)
        {
            current = HealingRules.ResolveRestored(current, Max, amount);
        }
    }
}
