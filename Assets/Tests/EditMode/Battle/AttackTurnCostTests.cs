using NUnit.Framework;
using GridStrategy.Battle;
using GridStrategy.Combat;
using GridStrategy.Core;

namespace GridStrategy.Tests.EditMode.Battle
{
    // Takma adın gerekçesi BattleTests.cs'te yazılı: çıplak "Battle" bu ad
    // alanının içinde TİP değil AD ALANI olarak çözülür.
    using Battle = global::GridStrategy.Battle.Battle;

    /// <summary>
    /// SALDIRININ SIRAYA MAL OLUP OLMADIĞINI koruyan dosya.
    ///
    /// Var olma sebebi ölçülmüş bir kusur: <c>BattleActions.Attack</c> her
    /// isabette sırayı devrediyordu ve kendiliğinden ateş eden bir kule bunu
    /// SAHİBİNİN sırasını harcayarak yapıyordu. Oyuncu hiçbir şey yapmadan
    /// sırasını kendi binasına kaptırıyordu. Bugün gizliydi çünkü varsayılan kip
    /// <see cref="TurnMode.FreeForAll"/> olduğunda devir zaten boş geçiyor;
    /// <see cref="TurnMode.Alternating"/> seçilen ilk gün görünür olurdu.
    ///
    /// ÇÖZÜM ÇIPLAK BİR BAYRAK DEĞİL, AYRI ADLI BİR ÜYE:
    /// <c>AttackWithoutSpendingTurn</c>. Bu dosya iki yolu YAN YANA sınıyor,
    /// çünkü korunacak şey tek bir yolun davranışı değil ikisinin FARKI.
    ///
    /// Burada sınanan tek şey SIRA MALİYETİ. Menzil, hedef uygunluğu ve hasar
    /// <see cref="BattleActionsTests"/>'te; kipin kendisi
    /// <see cref="TurnModeTests"/>'te sınanıyor ve bu dosya ikisini de
    /// tekrarlamıyor.
    /// </summary>
    public sealed class AttackTurnCostTests
    {
        private static Combatant NewCombatant(
            int maxHealth = 100,
            int damage = 10,
            int range = 1,
            Team team = Team.Player)
        {
            return new Combatant(
                new Health(maxHealth),
                new UnitLifecycle(),
                new AttackProfile(damage: damage, range: range),
                team);
        }

        private static Structure NewTower(int damage = 10, int range = 3, Team team = Team.Player)
        {
            return new Structure(
                new Health(200),
                new StructureLifecycle(),
                team,
                new AttackProfile(damage: damage, range: range));
        }

        // Kule (0,0), düşman (1,1): Chebyshev'de mesafe 1, yani her kule menzili
        // yeter. Kurulum tek yerde çünkü bu dosyadaki her test aynı iki tarafı
        // istiyor ve değişen şey yalnızca hangi üyenin çağrıldığı.
        private static Battle NewBattleWithTowerAndEnemy(
            out Unit towerUnit, out Unit enemyUnit, out Combatant enemy)
        {
            var battle = new Battle(8, 8);

            towerUnit = new Unit("Player tower");
            battle.AddStructure(towerUnit, NewTower(), 0, 0);

            enemyUnit = new Unit("Enemy soldier");
            enemy = NewCombatant(team: Team.Enemy);
            battle.AddUnit(enemyUnit, enemy, 1, 1);

            Assert.That(battle.Turn.Mode, Is.EqualTo(TurnMode.Alternating), "kurulum bozuk");
            Assert.That(battle.Turn.Current, Is.EqualTo(Team.Player), "kurulum bozuk");
            return battle;
        }

        /// <summary>
        /// KUSURUN KENDİSİ: kendiliğinden ateş eden kule isabet ediyor ama sıra
        /// oyuncuda kalıyor.
        ///
        /// Kırmızıya dönerse oyuncunun hakkını yine kendi binası harcıyordur.
        /// </summary>
        [Test]
        public void AttackWithoutSpendingTurn_OnHit_LeavesTheTurnWhereItWas()
        {
            Battle battle = NewBattleWithTowerAndEnemy(
                out Unit towerUnit, out Unit enemyUnit, out Combatant enemy);

            AttackOutcome outcome =
                BattleActions.AttackWithoutSpendingTurn(battle, towerUnit, enemyUnit);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.Hit),
                "the free path still lands a real hit; only the turn cost differs");
            Assert.That(enemy.CurrentHealth, Is.EqualTo(90),
                "damage is unchanged; the caller's intent does not touch the numbers");

            Assert.That(battle.Turn.Current, Is.EqualTo(Team.Player),
                "automatic tower fire must not hand the turn to the enemy");
            Assert.That(battle.Turn.TurnNumber, Is.EqualTo(TurnState.FirstTurnNumber),
                "no turn was consumed, so the counter did not move either");
        }

        /// <summary>
        /// İKİZ TEST: normal yol DEĞİŞMEDİ. Bu dosyanın asıl ölçtüğü şey ikisinin
        /// farkı, ve fark ancak ikisi yan yana durduğunda görünür.
        /// </summary>
        [Test]
        public void Attack_OnHit_StillHandsTheTurnOver()
        {
            Battle battle = NewBattleWithTowerAndEnemy(
                out Unit towerUnit, out Unit enemyUnit, out Combatant enemy);

            AttackOutcome outcome = BattleActions.Attack(battle, towerUnit, enemyUnit);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.Hit));
            Assert.That(enemy.CurrentHealth, Is.EqualTo(90));
            Assert.That(battle.Turn.Current, Is.EqualTo(Team.Enemy),
                "the ordinary path is untouched: a landed strike still ends the turn");
        }

        /// <summary>
        /// Kule arka arkaya iki kez ateş etse bile oyuncunun sırası duruyor —
        /// ölçülen zararın tam tersi.
        /// </summary>
        [Test]
        public void TwoAutomaticShots_DoNotCostThePlayerTwoTurns()
        {
            Battle battle = NewBattleWithTowerAndEnemy(
                out Unit towerUnit, out Unit enemyUnit, out Combatant enemy);

            BattleActions.AttackWithoutSpendingTurn(battle, towerUnit, enemyUnit);
            BattleActions.AttackWithoutSpendingTurn(battle, towerUnit, enemyUnit);

            Assert.That(enemy.CurrentHealth, Is.EqualTo(80), "iki atış da indi");
            Assert.That(battle.Turn.Current, Is.EqualTo(Team.Player));
            Assert.That(battle.Turn.TurnNumber, Is.EqualTo(TurnState.FirstTurnNumber));
        }

        /// <summary>
        /// Kulenin ateşinden SONRA oyuncunun kendi askeri hâlâ oynayabiliyor ve
        /// sırayı devreden O oluyor.
        ///
        /// Bu, kusurun oyuncuya görünen yüzü: eskiden kule ateş ettiği anda
        /// oyuncunun eli boşa çıkıyordu.
        /// </summary>
        [Test]
        public void AfterAutomaticTowerFire_ThePlayersOwnSoldierCanStillAct()
        {
            var battle = new Battle(8, 8);

            var towerUnit = new Unit("Player tower");
            battle.AddStructure(towerUnit, NewTower(), 0, 0);

            var soldierUnit = new Unit("Player soldier");
            battle.AddUnit(soldierUnit, NewCombatant(team: Team.Player), 1, 0);

            var enemyUnit = new Unit("Enemy soldier");
            Combatant enemy = NewCombatant(team: Team.Enemy);
            battle.AddUnit(enemyUnit, enemy, 2, 0);

            Assert.That(
                BattleActions.AttackWithoutSpendingTurn(battle, towerUnit, enemyUnit),
                Is.EqualTo(AttackOutcome.Hit));

            Assert.That(
                BattleActions.Attack(battle, soldierUnit, enemyUnit),
                Is.EqualTo(AttackOutcome.Hit),
                "the player's own soldier still has its turn after the tower fired");

            Assert.That(enemy.CurrentHealth, Is.EqualTo(80));
            Assert.That(battle.Turn.Current, Is.EqualTo(Team.Enemy),
                "the soldier's commanded strike is what ends the turn");
        }

        /// <summary>
        /// SIRA KAPISI AÇILMADI: sırayı HARCAMAMAK, sıra kuralını atlamak
        /// değildir.
        ///
        /// Kırmızıya dönerse yeni yol bir kaçak kapı olmuştur ve sırası
        /// gelmemiş bir taraf onun üstünden oynayabilir.
        /// </summary>
        [Test]
        public void AttackWithoutSpendingTurn_StillAsksWhoseTurnItIs()
        {
            Battle battle = NewBattleWithTowerAndEnemy(
                out Unit towerUnit, out Unit enemyUnit, out Combatant enemy);

            battle.Turn.EndTurn();
            Assert.That(battle.Turn.Current, Is.EqualTo(Team.Enemy), "kurulum bozuk");

            Assert.That(
                BattleActions.AttackWithoutSpendingTurn(battle, towerUnit, enemyUnit),
                Is.EqualTo(AttackOutcome.RejectedActorCannotAct),
                "not spending the turn is not the same as ignoring whose turn it is");
            Assert.That(enemy.CurrentHealth, Is.EqualTo(100), "reddedilen atış hasar vermez");
        }

        /// <summary>
        /// Reddedilen bir atış sırayı zaten devretmez; yeni yolun beyaz listesi
        /// eski yolunkiyle aynı değerleri sayıyor.
        /// </summary>
        [Test]
        public void ARejectedShot_MovesNothingOnEitherPath()
        {
            var battle = new Battle(8, 8);

            var towerUnit = new Unit("Short ranged tower");
            battle.AddStructure(towerUnit, NewTower(range: 1), 0, 0);

            var enemyUnit = new Unit("Distant enemy");
            Combatant enemy = NewCombatant(team: Team.Enemy);
            battle.AddUnit(enemyUnit, enemy, 5, 5);

            Assert.That(
                BattleActions.AttackWithoutSpendingTurn(battle, towerUnit, enemyUnit),
                Is.EqualTo(AttackOutcome.RejectedOutOfRange));
            Assert.That(
                BattleActions.Attack(battle, towerUnit, enemyUnit),
                Is.EqualTo(AttackOutcome.RejectedOutOfRange));

            Assert.That(enemy.CurrentHealth, Is.EqualTo(100));
            Assert.That(battle.Turn.Current, Is.EqualTo(Team.Player),
                "a rejected strike never ends the turn, on either path");
        }

        /// <summary>
        /// BİTİRİCİ VURUŞ DA BİR İSABETTİR: sırayı devreden değerler listesi onu
        /// da sayıyor.
        ///
        /// Kırmızıya dönerse yeni sonuç değeri beyaz listeye eklenmemiştir ve
        /// düşmüş bir düşmanı bitiren oyuncu sırasını hiç bitirmez — sonsuz
        /// oynayabilir.
        /// </summary>
        [Test]
        public void Attack_FinishingADownedEnemy_AlsoEndsTheTurn()
        {
            var battle = new Battle(8, 8);

            var soldierUnit = new Unit("Player soldier");
            battle.AddUnit(soldierUnit, NewCombatant(damage: 5, team: Team.Player), 0, 0);

            var enemyUnit = new Unit("Downed enemy");
            Combatant enemy = NewCombatant(maxHealth: 10, team: Team.Enemy);
            battle.AddUnit(enemyUnit, enemy, 1, 0);

            enemy.TakeDamage(10);
            Assert.That(enemy.State, Is.EqualTo(UnitState.Downed), "kurulum bozuk");

            Assert.That(
                BattleActions.Attack(battle, soldierUnit, enemyUnit),
                Is.EqualTo(AttackOutcome.HitAndFinished));
            Assert.That(battle.Turn.Current, Is.EqualTo(Team.Enemy),
                "finishing a body is a landed strike and costs the turn like any other");
        }
    }
}
