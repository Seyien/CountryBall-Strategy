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
    /// SIRA KAPISININ İKİ KİPİNİ koruyan dosya.
    ///
    /// Var olma sebebi operatörün şikâyeti: "bir savaşçıdan bir savaşçıya
    /// geçmeye çalıştığımda sıra meselesi önemli olmamalı; hangisi birbirine
    /// tıklayıp savaşabiliyorsa yapmalı." Oyuncu kum havuzunda paletten hem
    /// kendi hem karşı tarafın birimlerini koyuyor ve sıra kapısı ona "tıklıyorum,
    /// hiçbir şey olmuyor" olarak görünüyordu.
    ///
    /// İKİ YÖNÜ DE KORUYOR ve ikincisi daha önemli: yeni kip çalışsın diye
    /// ESKİ kipin gevşemediği. <see cref="TurnMode.Alternating"/> varsayılan
    /// kaldığı sürece kampanya savaşları bugünkü kurallarla oynanır.
    ///
    /// Burada sınanan tek şey KAPI. Menzil, hedef uygunluğu, hareket kuralları
    /// ve diriltme <see cref="BattleActionsTests"/> ile
    /// <see cref="TurnStateTests"/>'te zaten sınanıyor; bu dosya onların
    /// hiçbirini tekrarlamıyor.
    ///
    /// Son iki test farklı bir kırılmayı koruyor ve BURADA duruyorlar çünkü
    /// ölçtükleri şey aynı: <c>BattleActions.Attack</c>'in saldıran tarafındaki
    /// KAPI. Biri sıranın, öteki saldıranın TİPİNİN o kapıda oyuncuyu
    /// kilitlemediğini söylüyor.
    /// </summary>
    public sealed class TurnModeTests
    {
        private static Combatant NewCombatant(
            int maxHealth = 100,
            int damage = 30,
            int range = 1,
            Team team = Team.Player)
        {
            return new Combatant(
                new Health(maxHealth),
                new UnitLifecycle(downedWindowSeconds: 10f, corpseWindowSeconds: 5f),
                new AttackProfile(damage: damage, range: range),
                team);
        }

        private static Unit AddUnit(Battle battle, string name, int x, int y, Combatant combatant)
        {
            var unit = new Unit(name);
            battle.AddUnit(unit, combatant, x, y);
            return unit;
        }

        private static Unit AddStructure(Battle battle, string name, int x, int y, Structure structure)
        {
            var unit = new Unit(name);
            battle.AddStructure(unit, structure, x, y);
            return unit;
        }

        // ─────────────────────────────────────────────────────────────
        // Varsayılan kip — bugünkü davranış hiç gevşemedi
        // ─────────────────────────────────────────────────────────────

        [Test]
        public void Battle_WithoutMode_IsAlternating()
        {
            var battle = new Battle(3, 3);

            Assert.That(battle.Turn.Mode, Is.EqualTo(TurnMode.Alternating),
                "the campaign mode must remain what you get when you say nothing");
        }

        /// <summary>
        /// ESKİ KİP GEVŞEMEDİ: menzilde duran, ayakta, düşman bir hedefe sırası
        /// gelmemiş bir birim hâlâ vuramıyor ve hasar hiç inmiyor.
        /// </summary>
        [Test]
        public void Attack_Alternating_TeamOutOfTurnIsStillRejected()
        {
            var battle = new Battle(3, 3);
            Unit raider = AddUnit(battle, "Raider", 0, 0, NewCombatant(damage: 30, team: Team.Enemy));
            Combatant vanguardCombatant = NewCombatant(maxHealth: 100, team: Team.Player);
            Unit vanguard = AddUnit(battle, "Vanguard", 1, 0, vanguardCombatant);

            // Sıra dizilimin başında, yani oyuncuda; düşman henüz oynayamaz.
            Assert.That(battle.Turn.Current, Is.EqualTo(Team.Player), "kurulum bozuk");

            Assert.That(BattleActions.Attack(battle, raider, vanguard),
                Is.EqualTo(AttackOutcome.RejectedActorCannotAct));
            Assert.That(vanguardCombatant.CurrentHealth, Is.EqualTo(100));
        }

        [Test]
        public void Move_Alternating_TeamOutOfTurnIsStillRejected()
        {
            var battle = new Battle(3, 3);
            Unit raider = AddUnit(battle, "Raider", 0, 0, NewCombatant(team: Team.Enemy));

            Assert.That(BattleActions.Move(battle, raider, 0, 1, out _),
                Is.EqualTo(MoveOutcome.RejectedActorCannotAct));
            Assert.That(battle.TryGetUnit(0, 0, out Unit _), Is.True, "the unit must not have moved");
        }

        // ─────────────────────────────────────────────────────────────
        // Kum havuzu — sıra bir kapı değil, yalnızca bir gösterge
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// OYUNDA NE İŞE YARAR: oyuncu paletten koyduğu düşman birimini seçip
        /// kendi askerine tıklıyor ve vuruş OLUYOR. Kırmızıya dönerse kum
        /// havuzunda tıklama yine sessizce yutulur.
        /// </summary>
        [Test]
        public void Attack_FreeForAll_TeamOutOfTurnStillHits()
        {
            var battle = new Battle(3, 3, TurnMode.FreeForAll);
            Unit raider = AddUnit(battle, "Raider", 0, 0, NewCombatant(damage: 30, team: Team.Enemy));
            Combatant vanguardCombatant = NewCombatant(maxHealth: 100, team: Team.Player);
            Unit vanguard = AddUnit(battle, "Vanguard", 1, 0, vanguardCombatant);

            Assert.That(battle.Turn.Current, Is.EqualTo(Team.Player),
                "the indicator still says Player; only the gate is gone");

            Assert.That(BattleActions.Attack(battle, raider, vanguard),
                Is.EqualTo(AttackOutcome.Hit));
            Assert.That(vanguardCombatant.CurrentHealth, Is.EqualTo(70));
        }

        [Test]
        public void Move_FreeForAll_TeamOutOfTurnStillMoves()
        {
            var battle = new Battle(3, 3, TurnMode.FreeForAll);
            Unit raider = AddUnit(battle, "Raider", 0, 0, NewCombatant(team: Team.Enemy));

            Assert.That(BattleActions.Move(battle, raider, 0, 1, out _),
                Is.EqualTo(MoveOutcome.Moved));
            Assert.That(battle.TryGetUnit(0, 1, out Unit _), Is.True);
        }

        /// <summary>
        /// KUM HAVUZUNDA DEVİR YOKTUR. Sıra ilerleseydi, oyuncu düşman birimiyle
        /// oynadığında tur sayacı artar ve süreli her şey savaşın ortasında
        /// kendiliğinden biterdi.
        /// </summary>
        [Test]
        public void EndTurn_FreeForAll_DoesNotAdvanceAndReportsFalse()
        {
            var turn = new TurnState(TurnMode.FreeForAll);

            Assert.That(turn.EndTurn(), Is.False);
            Assert.That(turn.EndTurn(), Is.False, "twice is still no turn");
            Assert.That(turn.Current, Is.EqualTo(Team.Player));
            Assert.That(turn.TurnNumber, Is.EqualTo(TurnState.FirstTurnNumber));
        }

        /// <summary>
        /// Aynı kararın AKIŞ üzerinden ikizi: başarılı bir saldırı bile sırayı
        /// yakmıyor, yani oyuncu arka arkaya oynayabiliyor.
        /// </summary>
        [Test]
        public void Attack_FreeForAll_DoesNotBurnTheTurn()
        {
            var battle = new Battle(3, 3, TurnMode.FreeForAll);
            Unit vanguard = AddUnit(battle, "Vanguard", 0, 0, NewCombatant(damage: 30, team: Team.Player));
            Combatant raiderCombatant = NewCombatant(maxHealth: 100, team: Team.Enemy);
            Unit raider = AddUnit(battle, "Raider", 1, 0, raiderCombatant);

            Assert.That(BattleActions.Attack(battle, vanguard, raider), Is.EqualTo(AttackOutcome.Hit));
            Assert.That(BattleActions.Attack(battle, vanguard, raider), Is.EqualTo(AttackOutcome.Hit));

            Assert.That(raiderCombatant.CurrentHealth, Is.EqualTo(40));
            Assert.That(battle.Turn.Current, Is.EqualTo(Team.Player));
            Assert.That(battle.Turn.TurnNumber, Is.EqualTo(TurnState.FirstTurnNumber));
        }

        /// <summary>
        /// KALKAN TEK KAPI SIRA KAPISIDIR. Tarafsızlık kapısı duruyor: tarafsız
        /// olan taraf tutmaz, duvar vurmaz — kip ne olursa olsun.
        /// </summary>
        [Test]
        public void AllowsAction_FreeForAll_NeutralTeamStillCannotAct()
        {
            var turn = new TurnState(TurnMode.FreeForAll);

            Assert.That(turn.AllowsAction(Team.None), Is.False);
            Assert.That(turn.AllowsAction(Team.Player), Is.True);
            Assert.That(turn.AllowsAction(Team.Enemy), Is.True,
                "the side whose turn it is not may still act here");
        }

        [Test]
        public void Attack_FreeForAll_NeutralAttackerIsStillRejected()
        {
            var battle = new Battle(3, 3, TurnMode.FreeForAll);
            Unit wall = AddUnit(battle, "Wall", 0, 0, NewCombatant(damage: 30, team: Team.None));
            Combatant raiderCombatant = NewCombatant(maxHealth: 100, team: Team.Enemy);
            Unit raider = AddUnit(battle, "Raider", 1, 0, raiderCombatant);

            Assert.That(BattleActions.Attack(battle, wall, raider),
                Is.EqualTo(AttackOutcome.RejectedActorCannotAct));
            Assert.That(raiderCombatant.CurrentHealth, Is.EqualTo(100));
        }

        // ─────────────────────────────────────────────────────────────
        // Saldıranın TİPİ de bir kapıydı — ve kapalıydı
        //
        // Bu iki test kip değil TİP soruyor. Aynı dosyada duruyorlar çünkü aynı
        // satırı koruyorlar: BattleActions.Attack'in saldıran tarafı. Ayrı bir
        // dosyaya konsalardı Combat testlerinin göremediği bir yere düşerlerdi —
        // o derleme birimi BattleActions'ı hiç tanımıyor.
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// OYUNDA NE İŞE YARAR: oyuncu kendi kulesini seçip düşmana tıkladığında
        /// oyun ARTIK PATLAMIYOR, ateş ediyor. Eskiden akış kuleyi bir savaşçı
        /// sanıp "bu birim savaşta değil" istisnası fırlatıyordu.
        /// </summary>
        [Test]
        public void Attack_StructureAttacker_ShootsInsteadOfThrowing()
        {
            var battle = new Battle(4, 4);
            var tower = new Structure(
                new Health(100),
                new StructureLifecycle(),
                Team.Player,
                new AttackProfile(damage: 25, range: 2));
            Unit towerUnit = AddStructure(battle, "Tower", 0, 0, tower);

            Combatant raiderCombatant = NewCombatant(maxHealth: 100, team: Team.Enemy);
            Unit raider = AddUnit(battle, "Raider", 2, 0, raiderCombatant);

            AttackOutcome outcome = AttackOutcome.RejectedActorCannotAct;
            Assert.DoesNotThrow(() => outcome = BattleActions.Attack(battle, towerUnit, raider),
                "an attacking structure must not be mistaken for a missing combatant");

            Assert.That(outcome, Is.EqualTo(AttackOutcome.Hit));
            Assert.That(raiderCombatant.CurrentHealth, Is.EqualTo(75));

            // Yapı da saldırınca sırayı devrediyor — kip Alternating olduğu
            // sürece bu davranış savaşçılarınkiyle aynı kalmalı.
            Assert.That(battle.Turn.Current, Is.EqualTo(Team.Enemy));
        }

        /// <summary>
        /// SALDIRMAYAN YAPI BİR KURALDIR: oyuncu bir depoya tıkladığında oyun
        /// patlamaz, yalnızca hiçbir şey olmaz — ve sebep dönüş değerinde yazar.
        /// </summary>
        [Test]
        public void Attack_StructureAttackerWithoutProfile_IsRejectedAndDoesNotThrow()
        {
            var battle = new Battle(4, 4);
            var depot = new Structure(new Health(100), new StructureLifecycle(), Team.Player);
            Unit depotUnit = AddStructure(battle, "Depot", 0, 0, depot);

            Combatant raiderCombatant = NewCombatant(maxHealth: 100, team: Team.Enemy);
            Unit raider = AddUnit(battle, "Raider", 1, 0, raiderCombatant);

            AttackOutcome outcome = AttackOutcome.Hit;
            Assert.DoesNotThrow(() => outcome = BattleActions.Attack(battle, depotUnit, raider));

            Assert.That(outcome, Is.EqualTo(AttackOutcome.RejectedActorCannotAct));
            Assert.That(raiderCombatant.CurrentHealth, Is.EqualTo(100));
            Assert.That(battle.Turn.Current, Is.EqualTo(Team.Player),
                "a rejected attack must not burn the turn");
        }
    }
}
