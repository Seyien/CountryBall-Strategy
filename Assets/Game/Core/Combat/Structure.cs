using System;

namespace GridStrategy.Combat
{
    // ═══ ROL: BİLEŞİK (Aggregate) ════════════════════════════════════
    // kimlik : var — her yapı kendi Health ve kendi StructureLifecycle'ına sahip
    // hafıza : var — aynı TakeDamage(10) çağrısı her seferinde farklı sonuç
    // Unity  : gerekmez — parçalarının hiçbiri motora bağlı değil
    // karar  : parçalar ARASINDAKİ kuralı yürütür; parçaların kendi kurallarına
    //          karışmaz
    //
    // REDDEDILEN - Structure.cs:37 yerine:
    //     public sealed class Structure : Combatant
    // KIRILAN  : baraka, askerin yaşam döngüsünü DEVRALIR ve hiçbiri ona uymaz.
    //            TryRevive() devralınır  -> bina "diriltilebilir" olur
    //            AttackProfile zorunlu   -> saldırmayan depo sahte profil uydurur
    //            State artık UnitState   -> binaya hiç olmayacak Downed hâli gelir
    //            derleyici: hiçbir şey der  .  test: Repair_AfterDestruction
    //            _IsRejected'in koruduğu ayrım ORTADAN KALKAR
    // KAZANIRDI: her bina düşüp kurtarılma penceresi açsaydı ve her binanın
    //            savunma ateşi olsaydı — o gün üç dosya üç kopya kural olurdu.
    // TEK CUMLE: Kalıtım "aynı parçalara sahip" demek değil, "aynı yaşam
    //            döngüsünden geçer" demektir; baraka geçmiyor.
    /// <summary>
    /// Bir yapının bütünü: canı, yaşam döngüsü, tarafı ve (varsa) saldırı tanımı.
    ///
    /// Var olma sebebi <see cref="Combatant"/> ile aynı cümledir: <see cref="Health"/>
    /// canın bittiğini bilir ama <see cref="StructureLifecycle"/>'ı tanımaz;
    /// StructureLifecycle yıkımı bilir ama canı tanımaz. İkisini birden tanıyan tek
    /// yer burasıdır.
    ///
    /// Combatant'ın KOPYASI değildir ve ondan TÜREMEZ (gerekçe yukarıda). Ortak olan
    /// tek şey <see cref="Health"/>'tir — ve bu bir tesadüf değil, bu tipin varlığıyla
    /// sınanan iddiadır: can kuralı tipten bağımsızsa, bir barakanın canı bir askerin
    /// canıyla aynı sınıfla tutulabilmelidir.
    /// </summary>
    public sealed class Structure
    {
        private readonly Health health;
        private readonly StructureLifecycle lifecycle;

        // REDDEDILEN - Structure.cs:73 yerine (saldırı tanımı zorunlu olur,
        //              Combatant'taki gibi):
        //     public Structure(Health health, StructureLifecycle lifecycle, Team team, AttackProfile attackProfile)
        //     {
        //         AttackProfile = attackProfile ?? throw new ArgumentNullException(nameof(attackProfile));
        //     }
        // KIRILAN  : saldırmayan her depo ve duvar kendine sahte bir profil uydurur.
        //            damage: 0 yazılır      -> "saldıramaz"ın TİPSİZ işaretçisi doğar
        //            "menzil en az 1" kuralı -> uydurulan profil bir YALANA döner
        //            derleyici: hiçbir şey der  .  test:
        //            Structure_WithoutAttackProfile_CannotAttack derlenemez
        // KAZANIRDI: iki ayrı tip (Tower / Building) yazılsaydı ve kuleler saldırıya
        //            özgü DURUM taşısaydı — bekleme süresi, cephane, hedef hafızası.
        // TEK CUMLE: İsteğe bağlı parametre KURALI yazdırır, zorunlu parametre
        //            İSTİSNAyı; burada kural "yapı saldırmaz"dır.
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
            Team = team;

            // Takım sonradan DEĞİŞMEZ. Ele geçirilebilir bina istenirse bu bir
            // kural değişikliğidir (kim, ne kadar sürede, hangi mesafeden) ve o
            // kuralın sahibi bu tip olmayabilir; bugün readonly kalması, o kararın
            // bir setter'ın içinde sessizce verilmesini engelliyor.
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
        // REDDEDILEN - Structure.cs:113 yerine (tek kaynak canın kendisi olsun):
        //     public bool IsStanding => health.HasRemaining;
        // KIRILAN  : ad tek kaynağa iner ama YANLIŞ kaynağa; 41 testin 40'ı yeşil
        //            kalır (ÖLÇÜLDÜ, yalıtılmış koşu).
        //            yıkık binanın canı iyileştirilir -> bina kendiliğinden ayağa kalkar
        //            TryRepair'in kelepçesi artık kendi koruduğu şeyi sorar
        //            moloz sayacı hâlâ duruma bağlı -> sayı ile durum sessizce ayrışır
        //            derleyici: hiçbir şey der  .  test: DestroyedStructure_HealthItselfStillHeals
        // KAZANIRDI: yıkım geri sayımı, moloz penceresi ve onarım yasağı hiç
        //            olmasaydı — o gün StructureLifecycle fazlalık, tek kaynak can olurdu.
        // TEK CUMLE: Can bir SAYIdır, ayakta olmak bir ALAN yargısıdır; tek kaynağa
        //            inmek doğrudur ama kaynağın DOĞRU olanı seçilmelidir.
        public bool IsStanding => lifecycle.State == StructureState.Standing;

        public int CurrentHealth => health.Current;

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
            // yüzden ikisini birleştirmek en kolay göründüğü yer. HasRemaining
            // sayıyı söyler, IsStanding durumu; ikisi bilerek ayrı kalır (gerekçe
            // IsStanding'in üstündeki REDDEDILEN bloğunda).
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
            // kaldırmanın yasak olduğu tek yer can ile durumu AYNI ANDA gören yerdir.
            // Yaşam döngüsü tek başına izin verseydi sıfır canla ayakta duran bir
            // bina üretirdi (gerekçenin tamamı StructureLifecycle'daki TryRepair
            // reddinde yazılı).
            if (!IsStanding)
            {
                return false;
            }

            // Miktar doğrulaması bilerek burada DEĞİL: negatif onarımın ne olduğuna
            // HealingRules karar verir ve aynı ön koşulu burada kopyalasaydık kural
            // iki yerde yaşardı.
            health.Heal(amount);
            return true;
        }

        public void Tick(float deltaSeconds)
        {
            lifecycle.Tick(deltaSeconds);
        }
    }
}
