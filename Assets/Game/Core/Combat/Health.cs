using System;

namespace GridStrategy.Combat
{
    // ═══ ROL: VARLIK (Entity) ════════════════════════════════════════
    // kimlik : var — ölçüsü şu: new Health(10) ile İKİ nesne kur ve yalnız
    //          birine TakeDamage(3) uygula; onun Current'ı 7, ötekininki 10
    //          kalır. Sahibini BİLMEZ — bu tipte sahibe giden hiçbir alan
    //          yok; "kimin canı" sorusunu Combatant ile Structure cevaplar
    // hafıza : var — current çağrılar arasında yaşar; aynı TakeDamage(3)
    //          ikinci kez farklı bir sonuç bırakır
    // Unity  : gerekmez — düz C#, noEngineReferences: true
    // karar  : vermez, uygular — formül DamageRules'a ait, burada yalnızca yazma var
    /// <summary>
    /// Bir sahibin can SAYISI. Sahibinin ne olduğunu bilmez ve bilmemelidir:
    /// aynı sınıf bir askerin de bir barakanın da canını tutar. Can bittikten
    /// SONRA ne olacağını da bilmez — düşme, yıkılma, ödül ve animasyon
    /// kararları dışarıda verilir; bu bilgisizlik sayesinde can kuralı Unity
    /// olmadan sınanabiliyor.
    ///
    /// Hiçbir üyesi bir ALAN sözcüğü kullanmaz ("canlı", "ayakta", "sağlam"):
    /// sayı o yargıyı taşıyamaz. Alan yargısının sahipleri
    /// <see cref="Structure.IsStanding"/> ve <see cref="UnitState"/>'tir.
    ///
    /// GEREKÇELER: Docs/deep/kod/Core/Combat/Health.md
    /// </summary>
    public sealed class Health
    {
        private int current;

        public Health(int max)
        {
            // Bu kontrol bir KOPYA DEĞİL: "maksimum can pozitif olmalı"
            // kuralının sahibi bu tip. Aşağıda TakeDamage için reddedilen
            // doğrulamayla karıştırma — oradaki kuralın sahibi DamageRules.
            // → Health.md#healthint-max
            if (max <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(max), max, "Max health must be positive.");
            }

            Max = max;
            current = max;
        }

        // Kurucuda konur, bir daha yazılmaz. "Yazılması" diye bir olgu olmadığı
        // için ne set'i ne de SetMax'i var. → Health.md#max
        public int Max { get; }

        // OKUNAN ÜYE İLE YAZILAN ÜYE AYNI OLMAK ZORUNDA DEĞİL. `current`a giden
        // her yol bir kuraldan geçer (TakeDamage → DamageRules, Heal →
        // HealingRules). `set` eklemek ÜÇÜNCÜ bir ok açar ve kelepçeyi atlar:
        // negatif can yazılabilir hâle gelir, derleyici de testler de susar.
        // → Health.md#current
        public int Current => current;

        // SAYI, ALAN YARGISI TAŞIYAMAZ. Ad bilerek sayıyı anlatır, sahibini
        // değil: bir baraka "canlı" değildir ama canı KALMIŞTIR. `IsIntact`
        // bu tipin hiç ölçmediği bir bütünlük iddiasında bulunurdu.
        // → Health.md#hasremaining
        public bool HasRemaining => current > 0;

        // Hasarın TEK giriş noktası. Girdi doğrulaması bilerek burada DEĞİL:
        // formül ile onun geçerli girdi aralığı aynı sahibindir (DamageRules).
        // Buraya kopyalansaydı zırh eklendiği gün iki kopya ayrı hızda eskirdi.
        // → Health.md#takedamageint-amount
        public void TakeDamage(int amount)
        {
            current = DamageRules.ResolveRemaining(current, amount);
        }

        // TakeDamage'in aynadaki eşi. Aynı desen: formül dışarıda (HealingRules),
        // yazma burada. Onarım ile diriltme arasındaki farkı Structure ve
        // Combatant ayırır; burası yalnızca sayıyı yukarı yazar.
        // → Health.md#healint-amount
        public void Heal(int amount)
        {
            current = HealingRules.ResolveRestored(current, Max, amount);
        }
    }
}
