using System;

namespace GridStrategy.Combat
{
    // ═══ ROL: BİLEŞİK (Aggregate) ════════════════════════════════════
    // kimlik : var — ölçüsü şu: ayrı Health ve ayrı StructureLifecycle ile
    //          İKİ Structure kur, yalnız birine TakeDamage uygula;
    //          ötekinin CurrentHealth'i de IsStanding'i de kımıldamaz
    // hafıza : var — ölçüsü şu: Health(10) ile kurulan bir yapıya
    //          TakeDamage(10)'u arka arkaya İKİ kez uygula, dönüş değerleri
    //          FARKLI olur: birincisi true ("bu vuruş yıktı") ve
    //          IsStanding'i false yapar, ikincisi false — yıkık yapı ikinci
    //          kez yıkılmaz, enkaz sayacı da sıfırlanmaz
    // Unity  : gerekmez — parçalarının hiçbiri motora bağlı değil
    // karar  : parçalar ARASINDAKİ kuralı yürütür; parçaların kendi kurallarına
    //          karışmaz
    // KALITIM AYNI PARÇALAR DEĞİL, AYNI YAŞAM DÖNGÜSÜ DEMEKTİR. `: Combatant`
    // yazmak reddedildi: kalıtım SEÇMELİ değildir ve baraka devralacağı üyelerin
    // yarısına uymaz — TryRevive, Downed hâli, zorunlu AttackProfile, on
    // saniyelik kurtarma penceresi. `sealed` bu satıra karşı sıfır koruma sağlar.
    // → Structure.md#sealed-class-structure
    /// <summary>
    /// Bir yapının bütünü: canı, yaşam döngüsü, tarafı ve (varsa) saldırı tanımı.
    ///
    /// Var olma sebebi <see cref="Combatant"/> ile aynı cümledir:
    /// <see cref="Health"/> canın bittiğini bilir ama
    /// <see cref="StructureLifecycle"/>'ı tanımaz; StructureLifecycle yıkımı
    /// bilir ama canı tanımaz. İkisini birden tanıyan tek yer burasıdır.
    ///
    /// Combatant'ın KOPYASI değildir ve ondan TÜREMEZ. Ortak olan tek şey
    /// <see cref="Health"/>'tir — ve bu bir tesadüf değil, bu tipin varlığıyla
    /// sınanan iddiadır: can kuralı tipten bağımsızsa, bir barakanın canı bir
    /// askerin canıyla aynı sınıfla tutulabilmelidir.
    ///
    /// GEREKÇELER: Docs/deep/kod/Core/Combat/Structure.md
    /// </summary>
    public sealed class Structure
    {
        private readonly Health health;
        private readonly StructureLifecycle lifecycle;

        // İSTEĞE BAĞLI PARAMETRE KURALI YAZDIRIR, ZORUNLU OLAN İSTİSNAYI. Yapıların
        // ÇOĞU saldırmaz; zorunlu imzada her depo ve duvar kendine sahte bir profil
        // uydururdu — üstelik "menzil en az 1" kuralı yüzünden o profil komşu
        // hücreye ulaştığını SÖYLERDİ. Kural imzada okunsun diye isteğe bağlı.
        // → Structure.md#structure
        /// <param name="attackProfile">
        /// Saldırmayan yapılar için <c>null</c>. Kural olan davranış "saldırmaz"dır;
        /// isteğe bağlı parametre, kuralı değil İSTİSNAyı yazdırır.
        /// </param>
        public Structure(
            Health health,
            StructureLifecycle lifecycle,
            Team team,
            AttackProfile attackProfile = null)
        {
            this.health = health ?? throw new ArgumentNullException(nameof(health));
            this.lifecycle = lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));

            // Team.None BİLEREK geçerli: tarafsız yıkılabilir duvar, kapı ya da
            // engel gerçek bir yapıdır. Doğrulama koysaydık Team'in sıfırıncı
            // değerinin var olma sebebini burada iptal etmiş olurduk.
            // → Structure.md#team
            Team = team;

            // Takım sonradan DEĞİŞMEZ. Ele geçirilebilir bina istenirse bu bir
            // kural değişikliğidir (kim, ne kadar sürede, hangi mesafeden) ve o
            // kuralın sahibi bu tip olmayabilir; bugün readonly kalması, o kararın
            // bir setter'ın içinde sessizce verilmesini engelliyor.
            // → Structure.md#team
            AttackProfile = attackProfile;
        }

        /// <summary>Yapının tarafı. <see cref="Team.None"/> tarafsız yapıları anlatır.</summary>
        public Team Team { get; }

        /// <summary>Saldırı tanımı; saldırmayan yapılarda <c>null</c>.</summary>
        public AttackProfile AttackProfile { get; }

        /// <summary>
        /// Bu yapı saldırabilir mi. Çağıranın <c>AttackProfile != null</c> yazmasını
        /// engellemek için var: aynı null kontrolü üç çağıranda üç kez doğmasın.
        /// </summary>
        public bool CanAttack => AttackProfile != null;

        public StructureState State => lifecycle.State;

        /// <summary>
        /// Yapı ayakta mı. Bu, <see cref="Health"/>'in cevaplayamayacağı sorudur:
        /// can bir SAYIdır, ayakta olmak bir ALAN yargısıdır. Alan yargısının
        /// sahibi, alanı bilen taraftır — burada <see cref="StructureLifecycle"/>.
        /// </summary>
        // TEK KAYNAĞA İNMEK DOĞRUDUR; DOĞRU KAYNAĞA İNMEK ŞARTTIR. Ayakta olmak
        // bir ALAN yargısıdır ve kaynağı durumdur, can değil. `health.HasRemaining`
        // yazılsaydı yıkık binanın canı iyileştirildiğinde bina kendiliğinden
        // kalkardı — ölçüldü: 41 testin 40'ı yeşil kalıyor.
        // → Structure.md#isstanding
        public bool IsStanding => lifecycle.State == StructureState.Standing;

        public int CurrentHealth => health.Current;

        /// <summary>
        /// Bu yapının tam can değeri — ikizi <c>Combatant.MaxHealth</c> ile aynı
        /// gerekçeyle var: ekran, can barını ancak tavanı bilirse çizebilir.
        /// </summary>
        public int MaxHealth => health.Max;

        public float RemainingSeconds => lifecycle.RemainingSeconds;

        public bool IsReadyForCleanup => lifecycle.IsReadyForCleanup;

        /// <summary>
        /// Hasar uygular ve gerekiyorsa yaşam döngüsüne haber verir.
        /// </summary>
        /// <returns>Yapı BU vuruşla yıkıldıysa true.</returns>
        public bool TakeDamage(int amount)
        {
            health.TakeDamage(amount);

            // IsStanding neden ayrı yaşıyor: alan yargısı, alanı BİLEN sahibe ait.
            // Burası canı ve yaşam döngüsünü aynı anda gören tek yer — ve tam bu
            // yüzden ikisini birleştirmek en kolay göründüğü yer.
            // → Structure.md#isstanding
            if (!health.HasRemaining)
            {
                return lifecycle.OnHealthDepleted();
            }

            return false;
        }

        /// <summary>
        /// Ayakta olan yapıyı onarır. Onarım DİRİLTME DEĞİLDİR: yalnızca canı
        /// artırır, durumu değiştirmez ve yıkılmış bir yapıda çalışmaz. Yıkık bina
        /// onarılmaz — yeniden inşa edilir, ki o da yeni bir <see cref="Structure"/>
        /// nesnesidir.
        /// </summary>
        /// <returns>Onarım uygulandıysa true; yapı yıkıksa false.</returns>
        public bool TryRepair(int amount)
        {
            // Kelepçe burada, StructureLifecycle'da değil: yıkık bir yapıyı ayağa
            // kaldırmanın yasak olduğu tek yer can ile durumu AYNI ANDA gören
            // yerdir. Yaşam döngüsü tek başına izin verseydi sıfır canla ayakta
            // duran bir bina üretirdi.
            // → Structure.md#tryrepairint-amount
            if (!IsStanding)
            {
                return false;
            }

            // Miktar doğrulaması bilerek burada DEĞİL: negatif onarımın ne olduğuna
            // HealingRules karar verir ve aynı ön koşulu burada kopyalasaydık kural
            // iki yerde yaşardı.
            // → Structure.md#tryrepairint-amount
            health.Heal(amount);
            return true;
        }

        public void Tick(float deltaSeconds)
        {
            lifecycle.Tick(deltaSeconds);
        }
    }
}
