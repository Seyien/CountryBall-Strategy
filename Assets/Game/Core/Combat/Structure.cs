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

        // SAYAÇ YAPI BAŞINA, ve gerekçesi ikizi Combatant'ta ölçülerek yazılı:
        // eşik tanımda (AttackProfile), kalan süre örnekte. Aynı kule tanımından
        // dizilmiş beş kule ayrı ayrı bekler; alan tanımda olsaydı biri ateş
        // edince beşi birden susardı.
        private float attackCooldownRemaining;

        // İSTEĞE BAĞLI PARAMETRE KURALI YAZDIRIR, ZORUNLU OLAN İSTİSNAYI. Yapıların
        // ÇOĞU saldırmaz; zorunlu imzada her depo ve duvar kendine sahte bir profil
        // uydururdu — üstelik "menzil en az 1" kuralı yüzünden o profil komşu
        // hücreye ulaştığını SÖYLERDİ. Kural imzada okunsun diye isteğe bağlı.
        // → Structure.md#structure
        /// <param name="attackProfile">
        /// Saldırmayan yapılar için <c>null</c>. Kural olan davranış "saldırmaz"dır;
        /// isteğe bağlı parametre, kuralı değil İSTİSNAyı yazdırır.
        /// </param>
        /// <param name="isHeadquarters">
        /// Bu yapı takımın ANA KULESİ mi. Kural olan davranış "değil"dir, o
        /// yüzden isteğe bağlı — imza istisnayı yazdırır, kuralı değil; aynı
        /// gerekçe bir satır yukarıda attackProfile için ölçülmüş durumda.
        /// </param>
        public Structure(
            Health health,
            StructureLifecycle lifecycle,
            Team team,
            AttackProfile attackProfile = null,
            bool isHeadquarters = false)
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

            // ANA KULE OLMAK ÖRNEĞİN DEĞİL TÜRÜN GERÇEĞİ, ve buraya tanımdan
            // KOPYALANIYOR: tipin kendisi tanımı görmüyor (StructureBlueprint
            // bu katmanda değil, bir üstte) ve görmesi de istenmiyor. Kopya
            // yerine tanım referansı tutulsaydı bu tip bir üst katmana bağlanır
            // ve Battle bir tanım defteri taşımak zorunda kalırdı.
            IsHeadquarters = isHeadquarters;
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

        /// <summary>
        /// Bu yapı takımın ANA KULESİ mi.
        ///
        /// OYUNDA NE İŞE YARAR: ana kulesi yıkılan taraf savaşı kaybeder ve her
        /// takım en fazla bir tane kurabilir. İki kuralın da girdisi bu üye.
        /// </summary>
        // TÜR GERÇEĞİ, ÖRNEK GERÇEĞİ DEĞİL — ve ayrımı görmek için iki soruyu
        // yan yana koymak yeter: "bu BİNA ana kule mi" sabittir ve ömrü boyunca
        // değişmez, "bu TAKIMIN ayakta ana kulesi var mı" ise her yıkımda
        // değişir. İkincisinin sahibi bu tip değil, yapı defterini tutan Battle.
        //
        // bool, ENUM DEĞİL — ve bu bir eksiklik değil bir eşik: bugün ayırt
        // edilmesi gereken iki rol var (ana kule / öteki hepsi). Projenin geri
        // kalanı rolleri VERİYLE anlatıyor — Taret'i taret yapan `range > 0`,
        // Hisar'ı duvar yapan boş `Produces` listesi — ama "ana kule" mevcut
        // hiçbir sayıdan türetilemiyor, o yüzden kendi alanını hak ediyor.
        // TETİKLEYİCİ: ayırt edilmesi gereken ÜÇÜNCÜ bir rol doğduğu gün
        // (örneğin "kaynak binası") bu bool bir StructureRole enum'una döner.
        // Üçüncü vaka gelmeden enum yazmak, olmayan bir baskıya desen kurmaktır.
        public bool IsHeadquarters { get; }

        /// <summary>
        /// Bir sonraki atışa kalan saniye; atışa hazırken 0.
        /// </summary>
        // OYUNDA NE İŞE YARAR: kule menzilindeki düşmanı gördüğü her karede
        // değil, bu sayı boşaldıkça ateş eder. Otomatik ateş eden bir yapıda
        // beklemesiz saldırı, kare hızı ne kadar yüksekse o kadar hasar demekti.
        public float AttackCooldownRemaining => attackCooldownRemaining;

        /// <summary>
        /// Bu yapının bekleme süresi doldu mu. Yapının ayakta olup olmadığına ve
        /// silahı olup olmadığına BAKMAZ — üçü bağımsız üç eksendir.
        /// </summary>
        // ÜÇ EKSENİ TEK SORUYA SIKIŞTIRMAK BİRİNİ YUTAR: buraya bir
        // "&& CanAttack" eklenseydi, silahsız bir deponun reddi
        // RejectedActorCannotAct yerine RejectedOnCooldown olurdu ve oyuncu
        // deponun ateş etmesini beklerdi. Üçünü birden gören tek yer AttackAction.
        public bool IsAttackReady => attackCooldownRemaining <= 0f;

        /// <summary>
        /// Bir atışı HARCAR: bekleme dolmuşsa sayacı baştan başlatır ve
        /// <c>true</c> döner, dolmamışsa hiçbir şeye dokunmadan <c>false</c>.
        /// </summary>
        /// <returns>Atış hakkı bu çağrıyla alındıysa true.</returns>
        // SİLAHSIZ YAPI BURADA PATLAMIYOR, ve bu Structure'ın kendi kuralının
        // devamı: saldırı profili isteğe bağlı olduğu için bir depo da bu metodu
        // görebilir. Cevabı "bekleme yok" — o yapının reddi ZATEN AttackAction'ın
        // CanAttack kapısında veriliyor ve buraya konacak ikinci bir ret aynı
        // olguya ikinci bir sebep adı takardı.
        public bool TryBeginAttackCooldown()
        {
            if (attackCooldownRemaining > 0f)
            {
                return false;
            }

            attackCooldownRemaining = AttackProfile == null ? 0f : AttackProfile.CooldownSeconds;
            return true;
        }

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

        /// <summary>
        /// Zamanı bu yapının İKİ sayacına birden iletir: enkaz geri sayımına ve
        /// bir sonraki atışın beklemesine.
        /// </summary>
        // Şekli ve sırası ikizi Combatant.Tick ile birebir aynı; gerekçesi
        // orada tek kez yazılı ve burada tekrar edilmiyor, uygulanıyor —
        // Battle.Tick'in yapı döngüsü bu metodu zaten çağırıyor, ikinci bir tik
        // yolu açılmadı.
        public void Tick(float deltaSeconds)
        {
            lifecycle.Tick(deltaSeconds);

            if (attackCooldownRemaining <= 0f)
            {
                return;
            }

            attackCooldownRemaining -= deltaSeconds;
            if (attackCooldownRemaining < 0f)
            {
                attackCooldownRemaining = 0f;
            }
        }
    }
}
