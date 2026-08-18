using System;

namespace GridStrategy.Combat
{
    // ═══ ROL: BİLEŞİK (Aggregate) ════════════════════════════════════
    // kimlik : var — her savaşçı kendi Health ve kendi UnitLifecycle'ına sahip
    // hafıza : var — aynı TakeDamage(10) çağrısı her seferinde farklı sonuç
    // Unity  : gerekmez — parçalarının hiçbiri motora bağlı değil
    // karar  : parçalar ARASINDAKİ kuralı yürütür; parçaların kendi
    //          kurallarına karışmaz
    //
    // NEDEN ADAPTÖR DEĞİL: adaptör iki farklı DİL arasında çeviri yapar
    // (BoardAdapter: Unity'nin Vector3'ü ↔ Core'un int x,y'si). Buradaki üç
    // parça da Core'a ait, aynı dili konuşuyor. Çevrilecek bir şey yok —
    // sahiplenilecek bir bütün var. Ayırt edici soru: "iki tarafın dili farklı
    // mı?" Hayırsa bileşiktir, evetse adaptör.
    /// <summary>
    /// Bir savaşçının bütünü: canı, yaşam döngüsü ve saldırı tanımı bir arada.
    ///
    /// Var olma sebebi tek bir cümleyle: <see cref="Health"/> canın bittiğini
    /// bilir ama <see cref="UnitLifecycle"/>'ı tanımaz; UnitLifecycle düşmeyi
    /// bilir ama canı tanımaz. İkisini birden tanıyan tek yer burasıdır — ve
    /// aralarındaki kuralı yürütmek başka hiçbir tipin işi değildir.
    ///
    /// Parçaların kendi kurallarına KARIŞMAZ: hasar formülünü DamageRules,
    /// menzili AttackResolver, geri sayımı UnitLifecycle sahiplenir. Buradaki
    /// tek kural "ne zaman hangisine haber verilir".
    /// </summary>
    public sealed class Combatant
    {
        // Dirilen birim TAM canla kalkmaz. Oran olarak yazılı, sabit sayı
        // olarak değil: sabit 50 can, maksimumu 40 olan bir birimde tam
        // iyileşme, maksimumu 400 olanda hiç anlamına gelirdi.
        public const int ReviveHealthDivisor = 2;

        private readonly Health health;
        private readonly UnitLifecycle lifecycle;

        public Combatant(Health health, UnitLifecycle lifecycle, AttackProfile attackProfile)
        {
            this.health = health ?? throw new ArgumentNullException(nameof(health));
            this.lifecycle = lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));
            AttackProfile = attackProfile ?? throw new ArgumentNullException(nameof(attackProfile));
        }

        public AttackProfile AttackProfile { get; }

        public UnitState State => lifecycle.State;

        public int CurrentHealth => health.Current;

        public float RemainingSeconds => lifecycle.RemainingSeconds;

        public bool IsReadyForCleanup => lifecycle.IsReadyForCleanup;

        /// <summary>
        /// Hasar uygular ve gerekiyorsa yaşam döngüsüne haber verir.
        ///
        /// Kontrol her KAREde değil, her HASAR OLAYINDA yapılır — ve zaten bu
        /// metodun içindeyiz, yani "canı bitti mi" sorusunu sormanın maliyeti
        /// bir bool okuması. Ayrı bir dinleme mekanizması (event) bugün buna
        /// hiçbir şey katmazdı: haber verecek olan da, duyacak olan da bu tip.
        /// </summary>
        public void TakeDamage(int amount)
        {
            health.TakeDamage(amount);

            if (!health.IsAlive)
            {
                lifecycle.OnHealthDepleted();
            }
        }

        /// <summary>
        /// Düşmüş savaşçıyı ayağa kaldırır. Tam canla değil, maksimumun bir
        /// kesriyle — diriltmek ölümü geri almak değil, riskli bir yatırımdır.
        /// </summary>
        /// <returns>Diriltme gerçekleştiyse true.</returns>
        public bool TryRevive()
        {
            if (!lifecycle.TryRevive())
            {
                return false;
            }

            // Can sıfırdayken iyileştirdiğimiz için sonuç doğrudan payı verir.
            health.Heal(health.Max / ReviveHealthDivisor);
            return true;
        }

        public void Tick(float deltaSeconds)
        {
            lifecycle.Tick(deltaSeconds);
        }
    }
}
