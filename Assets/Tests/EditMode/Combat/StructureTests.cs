using System;
using NUnit.Framework;
using GridStrategy.Combat;

namespace GridStrategy.Tests.EditMode.Combat
{
    /// <summary>
    /// Bu dosya PARÇALARI test etmiyor — onların kendi testleri var. Burada
    /// sınanan iki şey var: parçalar arasındaki kural (can bitince yaşam
    /// döngüsünün haberi oluyor mu) ve bir İDDİA.
    ///
    /// İddia şu: <see cref="Health"/> "tipten bağımsız bir can kuralı"dır.
    /// Aylardır tek kullanıcısı <see cref="Combatant"/> olduğu için hiç
    /// sınanmadı. <see cref="Structure"/> ikinci kullanıcıdır ve
    /// Health_BehavesIdenticallyForAStructureAndForACombatant testi o iddianın
    /// tek kanıtıdır.
    ///
    /// Aynı iddia, <c>Health.IsAlive</c> adının neden <c>HasRemaining</c> olduğunu
    /// da ortaya çıkardı: KURAL tipten bağımsızdı, yanlış olan yalnızca ADdı.
    /// </summary>
    public sealed class StructureTests
    {
        private static Structure NewBarracks(int maxHealth = 100)
        {
            return new Structure(
                new Health(maxHealth),
                new StructureLifecycle(rubbleWindowSeconds: 8f),
                Team.Player);
        }

        private static Structure NewTower(int maxHealth = 100)
        {
            return new Structure(
                new Health(maxHealth),
                new StructureLifecycle(rubbleWindowSeconds: 8f),
                Team.Player,
                new AttackProfile(damage: 10, range: 3));
        }

        [Test]
        public void NewStructure_StandsWithFullHealth()
        {
            var barracks = NewBarracks();

            Assert.That(barracks.State, Is.EqualTo(StructureState.Standing));
            Assert.That(barracks.IsStanding, Is.True);
            Assert.That(barracks.CurrentHealth, Is.EqualTo(100));
        }

        [Test]
        public void Damage_ThatDoesNotDeplete_LeavesStateAlone()
        {
            var barracks = NewBarracks();

            bool destroyedNow = barracks.TakeDamage(99);

            Assert.That(destroyedNow, Is.False);
            Assert.That(barracks.CurrentHealth, Is.EqualTo(1));
            Assert.That(barracks.State, Is.EqualTo(StructureState.Standing), "1 health still stands.");
        }

        // Bu dosyanın var olma sebebi olan test: Health ile StructureLifecycle
        // birbirini tanımıyor, bağlantıyı Structure kuruyor.
        [Test]
        public void Damage_ThatDepletesHealth_DestroysAndReportsIt()
        {
            var barracks = NewBarracks();

            bool destroyedNow = barracks.TakeDamage(100);

            Assert.That(destroyedNow, Is.True);
            Assert.That(barracks.CurrentHealth, Is.Zero);
            Assert.That(barracks.State, Is.EqualTo(StructureState.Destroyed));
            Assert.That(barracks.RemainingSeconds, Is.EqualTo(8f), "The rubble timer must start.");
        }

        [Test]
        public void Damage_AfterDestruction_ReportsNoNewDestruction()
        {
            var barracks = NewBarracks();
            barracks.TakeDamage(100);

            bool destroyedNow = barracks.TakeDamage(5);

            Assert.That(destroyedNow, Is.False, "Rubble cannot be destroyed twice.");
            Assert.That(barracks.State, Is.EqualTo(StructureState.Destroyed));
        }

        [Test]
        public void Overkill_DoesNotPushHealthBelowZero()
        {
            var barracks = NewBarracks();

            barracks.TakeDamage(1000);

            Assert.That(barracks.CurrentHealth, Is.Zero);
            Assert.That(barracks.State, Is.EqualTo(StructureState.Destroyed));
        }

        /// <summary>
        /// SINAV. Bu test bir davranışı değil bir İDDİAYI koruyor: <see cref="Health"/>
        /// tipten bağımsızdır. Aynı hasar dizisi bir barakaya ve bir askere
        /// uygulanıyor; can tarafında — hem sayıda hem
        /// <see cref="Health.HasRemaining"/>'de — hiçbir adımda fark çıkmıyor, oysa
        /// yaşam döngüleri tamamen ayrışıyor (biri yıkılıyor, diğeri düşüyor).
        ///
        /// Health'e "yapıysa şöyle" diyen tek bir dal eklenirse burası kırmızıya
        /// döner — ve o dal HealthTests'i kırmayabilir, çünkü orada yalnızca tek bir
        /// kullanıcı varmış gibi yazılmış vakalar var. Bu dosya, iddianın ikinci
        /// kullanıcı tarafından sınandığı tek yerdir.
        ///
        /// AYRICA <c>IsAlive</c> → <c>HasRemaining</c> adlandırma kararının kanıtı:
        /// son üç iddia, İKİ sahibin SAYIsı aynıyken ALAN yargısının üç ayrı
        /// sözcükle (Destroyed / Downed / IsStanding) verildiğini gösteriyor. Sayı
        /// ortak, yargı ortak değil — adın alan sözcüğü taşıyamamasının sebebi tam
        /// olarak bu satırlarda duruyor.
        /// </summary>
        [Test]
        public void Health_BehavesIdenticallyForAStructureAndForACombatant()
        {
            // Health'ler ayrı yerel değişkende tutuluyor: HasRemaining'i ne
            // Structure ne de Combatant dışarı veriyor ve BİLEREK vermiyor —
            // sahipler alan sözcüğünü (IsStanding / State) yayımlar, sayıyı değil.
            var barracksHealth = new Health(100);
            var soldierHealth = new Health(100);

            var barracks = new Structure(
                barracksHealth,
                new StructureLifecycle(rubbleWindowSeconds: 8f),
                Team.Player);
            var soldier = new Combatant(
                soldierHealth,
                new UnitLifecycle(downedWindowSeconds: 10f, corpseWindowSeconds: 5f),
                new AttackProfile(damage: 10, range: 1));

            int[] hits = { 30, 30, 30, 30 };
            foreach (int hit in hits)
            {
                barracks.TakeDamage(hit);
                soldier.TakeDamage(hit);

                Assert.That(barracks.CurrentHealth, Is.EqualTo(soldier.CurrentHealth),
                    "Health must not care who owns it.");
                Assert.That(barracksHealth.HasRemaining, Is.EqualTo(soldierHealth.HasRemaining),
                    "HasRemaining must answer the same for a building and for a soldier.");
            }

            Assert.That(barracks.CurrentHealth, Is.Zero);
            Assert.That(barracksHealth.HasRemaining, Is.False, "The number ran out for the building.");
            Assert.That(soldierHealth.HasRemaining, Is.False, "The number ran out for the soldier too.");

            // Sayı aynı, yargı farklı: ayrışma yaşam döngüsünde olmalı, canda değil.
            // Bu üç satır adın neden HasRemaining olduğunu tek başına anlatır.
            Assert.That(barracks.State, Is.EqualTo(StructureState.Destroyed));
            Assert.That(soldier.State, Is.EqualTo(UnitState.Downed));
            Assert.That(barracks.IsStanding, Is.False,
                "The field word belongs to the field owner, never to the number.");
        }

        /// <summary>
        /// SINAVIN İKİNCİ YARISI ve onarım kelepçesinin gerekçesi. <see cref="Health"/>
        /// yıkımdan haberdar değildir: yıkılmış bir yapının canı doğrudan
        /// iyileştirilirse sayı gerçekten artar ve durum yıkık kalır.
        ///
        /// Bu bir hata değil, doğru iş bölümüdür — ama "yıkığı onarma" yasağının
        /// neden <see cref="Structure"/>'da durduğunu da kanıtlar: canı ve durumu
        /// aynı anda gören tek yer orasıdır. Kelepçe StructureLifecycle'a konsaydı
        /// sıfır canla ayakta duran bir bina üretirdi.
        /// </summary>
        [Test]
        public void DestroyedStructure_HealthItselfStillHeals_WhichIsWhyRepairIsGuardedInStructure()
        {
            var health = new Health(max: 50);
            var barracks = new Structure(health, new StructureLifecycle(), Team.Player);
            barracks.TakeDamage(50);

            health.Heal(20);

            Assert.That(barracks.CurrentHealth, Is.EqualTo(20), "Health knows nothing about demolition.");
            Assert.That(barracks.State, Is.EqualTo(StructureState.Destroyed), "State did not follow the number.");
            Assert.That(barracks.TryRepair(10), Is.False, "Structure is the only place that sees both.");
        }

        [Test]
        public void Repair_WhileStanding_RestoresHealthWithoutTouchingState()
        {
            var barracks = NewBarracks(maxHealth: 100);
            barracks.TakeDamage(40);

            bool repaired = barracks.TryRepair(25);

            Assert.That(repaired, Is.True);
            Assert.That(barracks.CurrentHealth, Is.EqualTo(85));
            Assert.That(barracks.State, Is.EqualTo(StructureState.Standing));
        }

        [Test]
        public void Repair_CannotExceedMaxHealth()
        {
            var barracks = NewBarracks(maxHealth: 100);
            barracks.TakeDamage(10);

            barracks.TryRepair(1000);

            Assert.That(barracks.CurrentHealth, Is.EqualTo(100), "The healing clamp works for structures too.");
        }

        /// <summary>
        /// KARARI koruyan test: onarım DİRİLTME DEĞİLDİR. Yıkılmış bir yapı
        /// onarılamaz; yeniden inşa edilir ve yeniden inşa yeni bir nesnedir.
        ///
        /// Bu test kırmızıya dönerse ya <see cref="Structure.TryRepair"/>'deki durum
        /// kelepçesi kalkmış ya da <see cref="StructureLifecycle"/>'a reddedilen
        /// TryRepair geçişi eklenmiş demektir. İkisi de aynı sessiz hatayı üretir:
        /// sıfır canla ayakta duran, gelen ilk 1 hasarla yeniden çöken bir bina —
        /// ve şikâyet "bina bazen anında yıkılıyor" diye gelir, sebebi buralarda
        /// aranmaz.
        /// </summary>
        [Test]
        public void Repair_AfterDestruction_IsRejected()
        {
            var barracks = NewBarracks(maxHealth: 100);
            barracks.TakeDamage(100);

            bool repaired = barracks.TryRepair(100);

            Assert.That(repaired, Is.False, "A ruin is rebuilt, not repaired.");
            Assert.That(barracks.State, Is.EqualTo(StructureState.Destroyed));
            Assert.That(barracks.CurrentHealth, Is.Zero, "A rejected repair must not restore health.");
        }

        /// <summary>
        /// KARARI koruyan test: saldırı tanımı OPSİYONELdir. Bir depo saldırmaz ve
        /// bunu söylemek için sahte bir profil (damage: 0) uydurmak zorunda değildir.
        ///
        /// Bu test derlenemez hâle gelirse — ki kural değişince olacağı budur, kırmızı
        /// değil DERLENMEZ — saldırı profili zorunlu hâle gelmiş demektir. O gün
        /// "saldıramaz" bilgisi tipten çıkıp <c>Damage == 0</c> gibi tipsiz bir
        /// işaretçiye dönüşür ve her çağıran onu kendi if'iyle yeniden keşfeder.
        /// </summary>
        [Test]
        public void Structure_WithoutAttackProfile_CannotAttack()
        {
            var warehouse = NewBarracks();

            Assert.That(warehouse.CanAttack, Is.False);
            Assert.That(warehouse.AttackProfile, Is.Null, "Not attacking is the rule, not the exception.");
        }

        [Test]
        public void Towers_WithAnAttackProfile_CanAttackAndMayShareOneProfile()
        {
            var profile = new AttackProfile(damage: 10, range: 3);
            var first = new Structure(new Health(100), new StructureLifecycle(), Team.Player, profile);
            var second = new Structure(new Health(100), new StructureLifecycle(), Team.Player, profile);

            Assert.That(first.CanAttack, Is.True);
            Assert.That(first.AttackProfile.Range, Is.EqualTo(3));
            // AttackProfile bir TANIM'dır: iki kule tek örneği paylaşabilir.
            Assert.That(second.AttackProfile, Is.SameAs(first.AttackProfile));
        }

        [Test]
        public void DestroyedTower_KeepsItsProfileButNoLongerStands()
        {
            var tower = NewTower();

            tower.TakeDamage(100);

            // Profil silinmiyor: "saldırabilir mi" sorusunun cevabı tanımda değil
            // durumda. Bu ayrım TargetingRules'a ait ve burada kopyalanmıyor.
            Assert.That(tower.CanAttack, Is.True, "The definition survives the building.");
            Assert.That(tower.IsStanding, Is.False, "Standing is the question that decides targeting.");
        }

        [Test]
        public void NeutralWall_IsBuiltWithTeamNone()
        {
            var wall = new Structure(new Health(20), new StructureLifecycle(), Team.None);

            Assert.That(wall.Team, Is.EqualTo(Team.None), "A destructible neutral wall is a real structure.");
            Assert.That(wall.TakeDamage(20), Is.True);
        }

        [Test]
        public void Tick_ForwardsTimeToTheLifecycle()
        {
            var barracks = NewBarracks();
            barracks.TakeDamage(100);

            barracks.Tick(8.1f);

            Assert.That(barracks.IsReadyForCleanup, Is.True);
        }

        [Test]
        public void Constructor_NullRequiredPart_Throws()
        {
            var health = new Health(10);
            var lifecycle = new StructureLifecycle();

            Assert.Throws<ArgumentNullException>(
                () => new Structure(null, lifecycle, Team.Player));
            Assert.Throws<ArgumentNullException>(
                () => new Structure(health, null, Team.Player));
            // Combatant'tan AYRILAN satır: orada null profil atardı, burada geçerli.
            Assert.DoesNotThrow(
                () => new Structure(health, lifecycle, Team.Player, null));
        }
    }
}
