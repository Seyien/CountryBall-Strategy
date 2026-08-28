using System;
using NUnit.Framework;
using GridStrategy.Combat;

namespace GridStrategy.Tests.EditMode.Combat
{
    /// <summary>
    /// SALDIRAN BİR YAPI olduğunda akışın hâlâ aynı sırayı izlediğini koruyan
    /// dosya. Parçalar burada sınanmıyor — menzil
    /// <see cref="AttackResolverTests"/>'te, hedef uygunluğu
    /// <see cref="TargetingRulesTests"/>'te, yıkım
    /// <see cref="StructureLifecycleTests"/>'te zaten sınanıyor.
    ///
    /// Var olma sebebi ölçülmüş bir P0'dı: <c>Structure.CanAttack</c> ve
    /// <c>Structure.AttackProfile</c> vardı, blueprint varlığı hasar ve menzil
    /// alanlarını taşıyordu, ama saldıran-yapı aşırı yüklemesi HİÇ YOKTU.
    /// Oyuncu kendi kulesini seçip düşmana tıkladığında akış barakayı bir
    /// savaşçı sanıp istisna fırlatıyordu.
    ///
    /// Sekiz test bir davranışı değil bir KARARI koruyor:
    /// <list type="bullet">
    /// <item>ayakta ve silahlı kule menzildeki düşmanı vuruyor — dört aşırı
    /// yüklemenin yeni olanına gerçekten ulaşılıyor</item>
    /// <item>öldürücü vuruş HitAndDowned diyor — adlandırma kararı saldıranın
    /// tipine bağlı değil, HEDEFİN tipine bağlı</item>
    /// <item>kendi tarafına vuruş reddediliyor — takım kuralı yapı saldıranda da
    /// soruluyor, üst katmana bırakılmıyor</item>
    /// <item>menzil dışı reddediliyor — kulenin menzili kendi profilinden
    /// geliyor, uydurma bir sabitten değil</item>
    /// <item>profili olmayan depo istisna ATMIYOR, reddediliyor — saldırmayan
    /// yapı kuraldır, çağıran hatası değil</item>
    /// <item>enkaz saldıramıyor — durum kuralının yapı ikizi gerçekten
    /// soruluyor</item>
    /// <item>kule düşman barakasını yıkıyor — dördüncü bileşim de gerçek, uydurma
    /// bir tamlık kaygısı değil</item>
    /// <item>null saldıran ve null hedef istisna atıyor — çağıran hatası hiçbir
    /// zaman bir oyun sonucu kılığına girmiyor</item>
    /// </list>
    /// </summary>
    public sealed class StructureAttackActionTests
    {
        // Varsayılan olarak SİLAHLI bir kule: bu dosyadaki saldıranların çoğu
        // odur. Profilsiz yapı ayrı bir yardımcıya değil aynı yardımcının
        // `attackProfile: null` çağrısına düşüyor, çünkü sınanan şey tam olarak
        // o parametrenin iki değeri arasındaki fark.
        private static Structure NewTower(
            int maxHealth = 100,
            int damage = 25,
            int range = 2,
            Team team = Team.Player)
        {
            return new Structure(
                new Health(maxHealth),
                new StructureLifecycle(),
                team,
                new AttackProfile(damage: damage, range: range));
        }

        private static Structure NewDepot(int maxHealth = 100, Team team = Team.Player)
        {
            // Saldırı tanımı YOK — saldırmayan yapı kuraldır ve isteğe bağlı
            // parametre tam olarak bunu yazdırır.
            return new Structure(new Health(maxHealth), new StructureLifecycle(), team);
        }

        private static Structure NewBarracks(int maxHealth = 100, Team team = Team.Enemy)
        {
            return new Structure(new Health(maxHealth), new StructureLifecycle(), team);
        }

        private static Combatant NewCombatant(int maxHealth = 100, Team team = Team.Enemy)
        {
            return new Combatant(
                new Health(maxHealth),
                new UnitLifecycle(downedWindowSeconds: 10f, corpseWindowSeconds: 5f),
                new AttackProfile(damage: 10, range: 1),
                team);
        }

        [Test]
        public void Execute_ArmedTowerAgainstEnemyInRange_Hits()
        {
            Structure attacker = NewTower(damage: 25, range: 2, team: Team.Player);
            var target = NewCombatant(maxHealth: 100, team: Team.Enemy);

            AttackOutcome outcome = AttackAction.Execute(attacker, target, distance: 2);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.Hit));
            Assert.That(target.CurrentHealth, Is.EqualTo(75),
                "damage must come from the tower's own profile");
        }

        /// <summary>
        /// Adlandırma kararı saldıranın tipine bağlı DEĞİL: bir savaşçı düşer,
        /// bir yapı yıkılır — kim vurursa vursun.
        /// </summary>
        [Test]
        public void Execute_LethalTowerHitOnCombatant_ReportsHitAndDowned()
        {
            Structure attacker = NewTower(damage: 25, range: 2, team: Team.Player);
            var target = NewCombatant(maxHealth: 20, team: Team.Enemy);

            Assert.That(AttackAction.Execute(attacker, target, distance: 1),
                Is.EqualTo(AttackOutcome.HitAndDowned));
            Assert.That(target.State, Is.EqualTo(UnitState.Downed));
        }

        /// <summary>
        /// Takım kuralı yapı saldıranda da BU KATMANDA soruluyor. Üst katmana
        /// bırakılsaydı, bu metodu doğrudan çağıran bir kule kendi askerini
        /// vururdu.
        /// </summary>
        [Test]
        public void Execute_SameTeamTarget_IsRejectedAsInvalidTarget()
        {
            Structure attacker = NewTower(damage: 25, range: 2, team: Team.Player);
            var target = NewCombatant(maxHealth: 100, team: Team.Player);

            Assert.That(AttackAction.Execute(attacker, target, distance: 1),
                Is.EqualTo(AttackOutcome.RejectedInvalidTarget));
            Assert.That(target.CurrentHealth, Is.EqualTo(100), "no damage may land on a rejection");
        }

        [Test]
        public void Execute_TargetBeyondTowerRange_IsRejected()
        {
            Structure attacker = NewTower(damage: 25, range: 2, team: Team.Player);
            var target = NewCombatant(maxHealth: 100, team: Team.Enemy);

            Assert.That(AttackAction.Execute(attacker, target, distance: 3),
                Is.EqualTo(AttackOutcome.RejectedOutOfRange));
            Assert.That(target.CurrentHealth, Is.EqualTo(100));
        }

        /// <summary>
        /// SALDIRMAYAN YAPI BİR KURALDIR, BİR ÇAĞIRAN HATASI DEĞİL. Bu test
        /// kırmızıya dönerse oyuncunun bir depoya tıklaması oyunu patlatır —
        /// tam olarak düzeltilen kırılmanın ikizi.
        /// </summary>
        [Test]
        public void Execute_StructureWithoutAttackProfile_IsRejectedAndDoesNotThrow()
        {
            Structure attacker = NewDepot(team: Team.Player);
            var target = NewCombatant(maxHealth: 100, team: Team.Enemy);

            AttackOutcome outcome = AttackOutcome.Hit;
            Assert.DoesNotThrow(
                () => outcome = AttackAction.Execute(attacker, target, distance: 1),
                "a structure without an attack profile is a rule, not a caller error");

            Assert.That(outcome, Is.EqualTo(AttackOutcome.RejectedActorCannotAct));
            Assert.That(target.CurrentHealth, Is.EqualTo(100));
        }

        [Test]
        public void Execute_DestroyedTower_CannotAttack()
        {
            Structure attacker = NewTower(maxHealth: 10, damage: 25, range: 2, team: Team.Player);
            Assert.That(attacker.TakeDamage(10), Is.True, "kurulum bozuk");

            var target = NewCombatant(maxHealth: 100, team: Team.Enemy);

            Assert.That(AttackAction.Execute(attacker, target, distance: 1),
                Is.EqualTo(AttackOutcome.RejectedActorCannotAct),
                "rubble does not shoot");
            Assert.That(target.CurrentHealth, Is.EqualTo(100));
        }

        [Test]
        public void Execute_TowerAgainstEnemyBarracks_ReportsHitAndDestroyed()
        {
            Structure attacker = NewTower(damage: 25, range: 2, team: Team.Player);
            Structure target = NewBarracks(maxHealth: 40, team: Team.Enemy);

            Assert.That(AttackAction.Execute(attacker, target, distance: 1),
                Is.EqualTo(AttackOutcome.Hit));
            Assert.That(AttackAction.Execute(attacker, target, distance: 1),
                Is.EqualTo(AttackOutcome.HitAndDestroyed));
            Assert.That(target.State, Is.EqualTo(StructureState.Destroyed));
        }

        [Test]
        public void Execute_NullArguments_Throw()
        {
            Structure attacker = NewTower();

            Assert.Throws<ArgumentNullException>(
                () => AttackAction.Execute((Structure)null, NewCombatant(), distance: 1));
            Assert.Throws<ArgumentNullException>(
                () => AttackAction.Execute(attacker, (Combatant)null, distance: 1));
            Assert.Throws<ArgumentNullException>(
                () => AttackAction.Execute(attacker, (Structure)null, distance: 1));
        }
    }
}
