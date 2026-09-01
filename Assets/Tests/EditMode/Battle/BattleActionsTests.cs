using System;
using System.Linq;
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
    /// Bu dosya parçaları test etmiyor — mesafe <see cref="GridDistance"/>'ta,
    /// menzil <see cref="AttackResolver"/>'da, hedef uygunluğu
    /// <see cref="TargetingRules"/>'ta, hareket kuralları
    /// <see cref="MoveAction"/>'da zaten sınanıyor. Burada sınanan tek şey
    /// BİRLEŞTİRME: konum tahtadan mı geliyor, mesafeyi kim ölçüyor, hangi soru
    /// hangi katmanda soruluyor.
    ///
    /// Yedi test bir davranışı değil bir KARARI koruyor:
    /// <list type="bullet">
    /// <item>çapraz komşuya menzil 1 ile vuruş — mesafe GridDistance'tan geliyor,
    /// buraya kopyalanmış bir formülden değil</item>
    /// <item>hareketten sonra saldırı — konum her seferinde tahtadan okunuyor,
    /// önbellekten değil</item>
    /// <item>aynı takıma vuruş — takım kuralı artık AttackAction'ın KENDİSİNDE
    /// yaşıyor; bu akış onu yalnızca çağırıyor. Özet daha önce bunun tersini
    /// söylüyordu ve o cümle bir BORCU tarif ediyordu, bir kararı değil</item>
    /// <item>sırası gelmemiş birim — sıra kuralı hedefin uygunluğundan da
    /// menzilden de ÖNCE soruluyor ve hasar hiç inmiyor</item>
    /// <item>düşmüş birim hareket edemiyor — kuralın sahibi MovementRules,
    /// soran taraf burası. Bu test bir BORCU sabitleyen selefinin YERİNE geçti</item>
    /// <item>MoveAction hiçbir girdiyle RejectedActorCannotAct döndürmüyor —
    /// Core'a, sahibinin üretemeyeceği bir değer koyduğumuz sabitleniyor</item>
    /// <item>tanınmayan birim — çağıran hatası patlıyor, bir oyun sonucuna
    /// dönüşmüyor</item>
    /// </list>
    ///
    /// SIRA ARTIK KURULUMUN PARÇASI. <see cref="TurnState.DefaultTurnOrder"/>
    /// önce oyuncuyu oynatır, dolayısıyla <see cref="Team.Player"/> saldıranlar
    /// ve hareket edenler kutudan çıktığı gibi geçer. Sırası gelmemiş bir tarafı
    /// sınayan testler <c>battle.Turn.EndTurn()</c> ile sırayı devrediyor —
    /// uydurma bir kurulum değil, oyunun kendi tek geçiş yolu.
    /// </summary>
    public sealed class BattleActionsTests
    {
        private static Combatant NewCombatant(
            int maxHealth = 100,
            int damage = 10,
            int range = 1,
            Team team = Team.Player)
        {
            return new Combatant(
                new Health(maxHealth),
                new UnitLifecycle(downedWindowSeconds: 10f, corpseWindowSeconds: 5f),
                new AttackProfile(damage: damage, range: range),
                team);
        }

        // Birim, savaşçı ve hücre tek çağrıda doğar; testlerin kurulum gürültüsü
        // burada kalsın diye. Dönen Unit çağıranın elindeki tek tutamak — konumu
        // saklamıyoruz çünkü konumun tek sahibi tahta.
        private static Unit AddUnit(Battle battle, string name, int x, int y, Combatant combatant)
        {
            var unit = new Unit(name);
            battle.AddUnit(unit, combatant, x, y);
            return unit;
        }

        private static Combatant CombatantOf(Battle battle, Unit unit)
        {
            Assert.That(battle.TryGetCombatant(unit, out Combatant combatant), Is.True, "kurulum bozuk");
            return combatant;
        }

        // Düşmüş bir savaşçı: hasar DOĞRUDAN uygulanıyor, akıştan geçmiyor.
        // Kurulum sınanan şeyi kullanırsa test kendi kendini doğrulamış olur.
        private static Combatant NewDownedCombatant(Team team = Team.Player)
        {
            Combatant combatant = NewCombatant(maxHealth: 100, team: team);
            combatant.TakeDamage(100);
            Assert.That(combatant.State, Is.EqualTo(UnitState.Downed), "kurulum bozuk");
            return combatant;
        }

        private static Structure NewStructure(int maxHealth = 100, Team team = Team.Enemy)
        {
            return new Structure(new Health(maxHealth), new StructureLifecycle(), team);
        }

        // Bir baraka da tahtada bir Unit ile temsil ediliyor — "tahtada yer
        // kaplayan, kimliği olan şey". İkinci bir tahta açmamanın bedeli bu
        // satır; kazancı ise "bu hücre dolu mu" sorusunun TEK kalması.
        private static Unit AddStructure(
            Battle battle, string name, int x, int y, Structure structure)
        {
            var unit = new Unit(name);
            battle.AddStructure(unit, structure, x, y);
            return unit;
        }

        // ─────────────────────────────────────────────────────────────
        // Attack
        // ─────────────────────────────────────────────────────────────

        [Test]
        public void Attack_AdjacentEnemy_HitsAndDealsProfileDamage()
        {
            var battle = new Battle(3, 5);
            Unit attacker = AddUnit(battle, "Vanguard", 0, 0, NewCombatant(damage: 30, team: Team.Player));
            Unit target = AddUnit(battle, "Raider", 1, 0, NewCombatant(maxHealth: 100, team: Team.Enemy));

            AttackOutcome outcome = BattleActions.Attack(battle, attacker, target);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.Hit));
            Assert.That(CombatantOf(battle, target).CurrentHealth, Is.EqualTo(70));
        }

        /// <summary>
        /// MESAFE SAHİPLİĞİNİ koruyan test. Saldıran (0,0), hedef (1,1), menzil 1.
        /// Chebyshev'de uzaklık 1 — vuruş geçer. Manhattan'da 2 olurdu ve cevap
        /// <see cref="AttackOutcome.RejectedOutOfRange"/> olurdu.
        ///
        /// Kırmızıya dönerse ya GridDistance'ın Chebyshev kararı değişmiştir ya
        /// da — daha kötüsü — biri bu akışa kendi mesafe formülünü yazmıştır.
        /// İkinci durumda GridDistanceTests yeşil kalır ve iki ölçü sessizce
        /// ayrışır.
        /// </summary>
        [Test]
        public void Attack_DiagonalNeighbourWithRangeOne_Hits()
        {
            var battle = new Battle(3, 5);
            Unit attacker = AddUnit(battle, "Vanguard", 0, 0, NewCombatant(damage: 30, range: 1, team: Team.Player));
            Unit target = AddUnit(battle, "Raider", 1, 1, NewCombatant(maxHealth: 100, team: Team.Enemy));

            AttackOutcome outcome = BattleActions.Attack(battle, attacker, target);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.Hit),
                "a diagonal neighbour is one step away; a Manhattan formula copied into this flow would report two");
            Assert.That(CombatantOf(battle, target).CurrentHealth, Is.EqualTo(70));
        }

        [Test]
        public void Attack_TwoCellsAwayWithRangeOne_IsOutOfRange()
        {
            // Yukarıdaki çapraz testinin pozitif kontrolü: menzil GERÇEKTEN
            // sınırlıyor, her mesafe kabul edilmiyor.
            var battle = new Battle(3, 5);
            Unit attacker = AddUnit(battle, "Vanguard", 0, 0, NewCombatant(damage: 30, range: 1, team: Team.Player));
            Unit target = AddUnit(battle, "Raider", 2, 0, NewCombatant(maxHealth: 100, team: Team.Enemy));

            AttackOutcome outcome = BattleActions.Attack(battle, attacker, target);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.RejectedOutOfRange));
            Assert.That(CombatantOf(battle, target).CurrentHealth, Is.EqualTo(100));
        }

        [Test]
        public void Attack_TwoCellsAwayWithRangeTwo_Hits()
        {
            // Menzil AttackProfile'dan geliyor, akıştan değil.
            var battle = new Battle(3, 5);
            Unit attacker = AddUnit(battle, "Archer", 0, 0, NewCombatant(damage: 30, range: 2, team: Team.Player));
            Unit target = AddUnit(battle, "Raider", 2, 0, NewCombatant(maxHealth: 100, team: Team.Enemy));

            Assert.That(BattleActions.Attack(battle, attacker, target), Is.EqualTo(AttackOutcome.Hit));
        }

        [Test]
        public void Attack_LethalHit_ReportsHitAndDowned()
        {
            var battle = new Battle(3, 5);
            Unit attacker = AddUnit(battle, "Vanguard", 0, 0, NewCombatant(damage: 10, team: Team.Player));
            Unit target = AddUnit(battle, "Raider", 1, 0, NewCombatant(maxHealth: 10, team: Team.Enemy));

            AttackOutcome outcome = BattleActions.Attack(battle, attacker, target);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.HitAndDowned));
            Assert.That(CombatantOf(battle, target).State, Is.EqualTo(UnitState.Downed));
        }

        [Test]
        public void Attack_DeadTarget_IsRejectedAsInvalidTarget()
        {
            var battle = new Battle(3, 5);
            Unit attacker = AddUnit(battle, "Vanguard", 0, 0, NewCombatant(damage: 10, team: Team.Player));
            Combatant targetCombatant = NewCombatant(maxHealth: 10, team: Team.Enemy);
            Unit target = AddUnit(battle, "Raider", 1, 0, targetCombatant);
            targetCombatant.TakeDamage(10);
            targetCombatant.Tick(10.1f);
            Assert.That(targetCombatant.State, Is.EqualTo(UnitState.Dead), "kurulum bozuk");

            Assert.That(BattleActions.Attack(battle, attacker, target),
                Is.EqualTo(AttackOutcome.RejectedInvalidTarget));
        }

        /// <summary>
        /// KATMAN KARARINI koruyan test — ve koruduğu karar DEĞİŞTİ: kural artık
        /// TEK katmanda yaşıyor. Bu test önceden
        /// <c>Attack_SameTeamTarget_IsRejectedByTheFlow_WhileAttackActionAloneStillHits</c>
        /// (ESKI AD) — aramaya kalkma, bugünkü adı bu metodun kendisidir
        /// adındaydı ve adının ikinci yarısı bir BORCU anlatıyordu: takım sorusu
        /// yalnızca burada soruluyor, <see cref="AttackAction"/> tek başına hâlâ
        /// vuruyordu. Soru aşağı indi; adın o yarısı yalana döndüğü için silindi.
        ///
        /// Testin iki yarısı artık bir AYRIMI değil bir ÖZDEŞLİĞİ gösteriyor:
        /// akıştan geçen saldırı ile akışı atlayıp doğrudan yapılan saldırı aynı
        /// cevabı veriyor. Kuralın tek sahibi olmasının gözlenebilir tanımı budur.
        ///
        /// BORÇ KAPANDI: bu akışta artık bir takım ön kontrolü YOK.
        /// <c>BattleActions.Attack</c> yalnız sırayı soruyor, takımı hiç
        /// sormuyor; tek sahip <see cref="AttackAction"/>. Testin ikinci yarısı
        /// o kaldırmayı güvenli kılan kanıttı ve bugün onun tanığı.
        /// </summary>
        [Test]
        public void Attack_SameTeamTarget_IsRejectedByTheFlowAndByAttackActionAlike()
        {
            var battle = new Battle(3, 5);
            Unit attacker = AddUnit(battle, "Vanguard", 0, 0, NewCombatant(damage: 30, team: Team.Player));
            Unit target = AddUnit(battle, "Runner", 1, 0, NewCombatant(maxHealth: 100, team: Team.Player));

            AttackOutcome outcome = BattleActions.Attack(battle, attacker, target);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.RejectedInvalidTarget),
                "the flow does not ask about teams at all; AttackAction owns that rule");
            Assert.That(CombatantOf(battle, target).CurrentHealth, Is.EqualTo(100),
                "a rejected attack must not deal damage");

            // Aynı iki savaşçı, bu kez akış atlanarak doğrudan AttackAction'a
            // veriliyor: kural artık ORADA da sorulduğu için cevap aynı ve hasar
            // inmiyor. Bu yarı, akıştaki ön kontrolün kaldırılmasını GÜVENLİ
            // kılan tek kanıttır.
            AttackOutcome direct = AttackAction.Execute(
                CombatantOf(battle, attacker), CombatantOf(battle, target), distance: 1);

            Assert.That(direct, Is.EqualTo(AttackOutcome.RejectedInvalidTarget),
                "AttackAction now asks the team rule itself; the friendly-fire gap no longer lives there");
            Assert.That(CombatantOf(battle, target).CurrentHealth, Is.EqualTo(100),
                "the direct call deals no damage either; the rule has a single owner");
        }

        /// <summary>
        /// SIRA KARARINI koruyan test. Hedef hem aynı takımda hem menzil dışı —
        /// iki ret sebebi de geçerli, ama yalnız biri doğru. "Geçersiz hedef"
        /// doğru olan, çünkü o birime yaklaşmak hiçbir şeyi değiştirmez;
        /// "menzil dışı" cevabı yapay zekâya "yaklaş" dedirtir ve sonsuz döngü
        /// üretir.
        ///
        /// GARANTİNİN TEK SAHİBİ VAR: cevabı <see cref="AttackAction"/> veriyor,
        /// çünkü bu akışta takım sorusu hiç sorulmuyor — hedef uygunluğu
        /// menzilden önce, orada, tek çağrıda soruluyor (gerekçesi orada, ölü
        /// hedef için yazılı). Bu test o sıranın akıştan da göründüğünü
        /// koruyor, sırayı burada KURDUĞUNU değil.
        /// </summary>
        [Test]
        public void Attack_SameTeamTargetOutOfRange_PrefersInvalidTargetOverOutOfRange()
        {
            var battle = new Battle(3, 5);
            Unit attacker = AddUnit(battle, "Vanguard", 0, 0, NewCombatant(damage: 30, range: 1, team: Team.Player));
            Unit target = AddUnit(battle, "Runner", 2, 4, NewCombatant(maxHealth: 100, team: Team.Player));

            AttackOutcome outcome = BattleActions.Attack(battle, attacker, target);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.RejectedInvalidTarget),
                "a same-team target must be reported as an invalid target even when it is also out of range");
        }

        /// <summary>
        /// CEVABI DEĞİŞEN test — ve değişimin kendisi bir karar, o yüzden
        /// burada yazılı. Bu test önceden
        /// <c>Attack_NeutralAttacker_IsRejected</c> (ESKI AD) adındaydı ve
        /// <see cref="AttackOutcome.RejectedInvalidTarget"/> bekliyordu.
        ///
        /// Team.None hâlâ saldıramaz — duvar vurmaz — ama artık soruyu SORAN
        /// değişti: sıra kuralı hedef uygunluğunun ÖNÜNE geçti ve tarafsız takım
        /// hiçbir sırada eyleyemez. İki kural aynı verdiği hükümde birleşiyor,
        /// yalnızca dıştaki önce cevap veriyor.
        ///
        /// Kırmızıya dönerse sıra kontrolü ya kalkmış ya da geriye alınmıştır;
        /// ikisi de kodu derlenir bırakır.
        /// </summary>
        [Test]
        public void Attack_NeutralAttacker_IsRejectedAsActorCannotAct()
        {
            var battle = new Battle(3, 5);
            Unit attacker = AddUnit(battle, "Wall", 0, 0, NewCombatant(damage: 30, team: Team.None));
            Unit target = AddUnit(battle, "Raider", 1, 0, NewCombatant(maxHealth: 100, team: Team.Enemy));

            Assert.That(BattleActions.Attack(battle, attacker, target),
                Is.EqualTo(AttackOutcome.RejectedActorCannotAct),
                "the neutral team is never on turn, and the turn rule is asked before target eligibility");
            Assert.That(CombatantOf(battle, target).CurrentHealth, Is.EqualTo(100));
        }

        [Test]
        public void Attack_NeutralTarget_IsHit()
        {
            // Tarafsız HEDEF herkese açık: yıkılabilir duvarın var olma sebebi
            // yıkılmaktır. Takım kuralının iki yönü asimetrik ve bu test o
            // asimetrinin birleştirme katmanında korunduğunu gösteriyor.
            var battle = new Battle(3, 5);
            Unit attacker = AddUnit(battle, "Vanguard", 0, 0, NewCombatant(damage: 30, team: Team.Player));
            Unit target = AddUnit(battle, "Wall", 1, 0, NewCombatant(maxHealth: 100, team: Team.None));

            Assert.That(BattleActions.Attack(battle, attacker, target), Is.EqualTo(AttackOutcome.Hit));
            Assert.That(CombatantOf(battle, target).CurrentHealth, Is.EqualTo(70));
        }

        /// <summary>
        /// ÇAĞIRAN HATASI KARARINI koruyan test. Battle'ın tanımadığı bir birimle
        /// saldırmak bir oyun sonucu değil, kaydın Battle ile ayrışmış olmasıdır.
        ///
        /// Kırmızıya dönerse biri bu durumu bir AttackOutcome değerine ya da yeni
        /// bir BattleOutcome tipine çevirmiş demektir — ve o dal genellikle
        /// sessizce yutulur; hata Play'de, hiç vuramayan bir birim olarak görünür.
        /// </summary>
        [Test]
        public void Attack_UnknownAttacker_Throws()
        {
            var battle = new Battle(3, 5);
            Unit target = AddUnit(battle, "Raider", 1, 0, NewCombatant(team: Team.Enemy));

            Assert.Throws<ArgumentException>(
                () => BattleActions.Attack(battle, new Unit("Ghost"), target));
        }

        [Test]
        public void Attack_UnknownTarget_Throws()
        {
            var battle = new Battle(3, 5);
            Unit attacker = AddUnit(battle, "Vanguard", 0, 0, NewCombatant(team: Team.Player));

            Assert.Throws<ArgumentException>(
                () => BattleActions.Attack(battle, attacker, new Unit("Ghost")));
        }

        [Test]
        public void Attack_NullArgument_Throws()
        {
            var battle = new Battle(3, 5);
            Unit attacker = AddUnit(battle, "Vanguard", 0, 0, NewCombatant(team: Team.Player));
            Unit target = AddUnit(battle, "Raider", 1, 0, NewCombatant(team: Team.Enemy));

            Assert.Throws<ArgumentNullException>(() => BattleActions.Attack(null, attacker, target));
            Assert.Throws<ArgumentNullException>(() => BattleActions.Attack(battle, null, target));
            Assert.Throws<ArgumentNullException>(() => BattleActions.Attack(battle, attacker, null));
        }

        // ─────────────────────────────────────────────────────────────
        // Move
        // ─────────────────────────────────────────────────────────────

        [Test]
        public void Move_ToAnEmptyCellInRange_UpdatesTheBoard()
        {
            var battle = new Battle(3, 5);
            Unit unit = AddUnit(battle, "Vanguard", 1, 2, NewCombatant());

            MoveOutcome outcome = BattleActions.Move(battle, unit, toX: 1, toY: 3, moveRange: 1);

            Assert.That(outcome, Is.EqualTo(MoveOutcome.Moved));
            Assert.That(battle.TryGetPosition(unit, out int x, out int y), Is.True);
            Assert.That(x, Is.EqualTo(1));
            Assert.That(y, Is.EqualTo(3));
            Assert.That(battle.TryGetUnit(1, 2, out Unit _), Is.False, "the old cell must be empty");
        }

        [Test]
        public void Move_DiagonalStepWithMoveRangeOne_IsAllowed()
        {
            // Hareket tarafındaki mesafe de GridDistance'tan geliyor; MoveAction
            // onu zaten sınıyor. Buradaki test yalnızca akışın gerçek konumu
            // geçirdiğini koruyor.
            var battle = new Battle(3, 5);
            Unit unit = AddUnit(battle, "Vanguard", 1, 2, NewCombatant());

            Assert.That(BattleActions.Move(battle, unit, toX: 2, toY: 3, moveRange: 1),
                Is.EqualTo(MoveOutcome.Moved));
        }

        [Test]
        public void Move_OntoAnotherUnit_IsRejectedAndNothingMoves()
        {
            var battle = new Battle(3, 5);
            Unit mover = AddUnit(battle, "Vanguard", 1, 2, NewCombatant());
            AddUnit(battle, "Runner", 1, 3, NewCombatant());

            MoveOutcome outcome = BattleActions.Move(battle, mover, toX: 1, toY: 3, moveRange: 1);

            Assert.That(outcome, Is.EqualTo(MoveOutcome.RejectedCellOccupied));
            Assert.That(battle.TryGetPosition(mover, out int x, out int y), Is.True);
            Assert.That(x, Is.EqualTo(1));
            Assert.That(y, Is.EqualTo(2));
        }

        [Test]
        public void Move_OutOfRange_IsRejected()
        {
            var battle = new Battle(3, 5);
            Unit unit = AddUnit(battle, "Vanguard", 0, 0, NewCombatant());

            Assert.That(BattleActions.Move(battle, unit, toX: 2, toY: 4, moveRange: 1),
                Is.EqualTo(MoveOutcome.RejectedOutOfRange));
        }

        [Test]
        public void Move_OutsideTheBoard_IsRejectedAsInvalidDestination()
        {
            var battle = new Battle(3, 5);
            Unit unit = AddUnit(battle, "Vanguard", 0, 0, NewCombatant());

            Assert.That(BattleActions.Move(battle, unit, toX: -1, toY: 0, moveRange: 1),
                Is.EqualTo(MoveOutcome.RejectedInvalidDestination));
        }

        /// <summary>
        /// KONUM SAHİPLİĞİNİ koruyan test — bu dosyanın en önemli satırı.
        /// Saldırı önce menzil dışı, birim bir adım yaklaşıyor, sonra vuruyor.
        ///
        /// Kırmızıya dönerse Battle konumu bir yerde ÖNBELLEĞE almış ve hareket
        /// onu güncellememiş demektir. Hata sessizdir: hiçbir şey patlamaz,
        /// yalnızca birim yeni hücresinde durup eski hücresinden saldırır.
        /// </summary>
        [Test]
        public void Move_ThenAttack_UsesTheNewPosition()
        {
            var battle = new Battle(3, 5);
            Unit attacker = AddUnit(battle, "Vanguard", 0, 0, NewCombatant(damage: 30, range: 1, team: Team.Player));
            Unit target = AddUnit(battle, "Raider", 2, 0, NewCombatant(maxHealth: 100, team: Team.Enemy));

            Assert.That(BattleActions.Attack(battle, attacker, target),
                Is.EqualTo(AttackOutcome.RejectedOutOfRange), "the setup is wrong");

            Assert.That(BattleActions.Move(battle, attacker, toX: 1, toY: 0, moveRange: 1),
                Is.EqualTo(MoveOutcome.Moved));

            // BAŞARILI HAREKET SIRAYI DEVRETTİ. Bu satır testin gürültüsü değil,
            // ikinci bir kararın kanıtı: bir birim bir turda BİR eylem yapar
            // (TurnRules.MaxActionsPerTurn == 1) ve bütçeyi harcayan şey bir
            // sayaç değil, sıranın kendisinin devredilmesidir. Bu satır olmadan
            // aşağıdaki saldırı RejectedActorCannotAct döner.
            //
            // Sıra Player -> Enemy'ye geçti; bir devir daha Player'a döndürüyor.
            Assert.That(battle.Turn.Current, Is.EqualTo(Team.Enemy),
                "a successful move must hand the turn over");
            battle.Turn.EndTurn();

            Assert.That(BattleActions.Attack(battle, attacker, target), Is.EqualTo(AttackOutcome.Hit),
                "the attack must be measured from the cell the unit stands on now");
            Assert.That(CombatantOf(battle, target).CurrentHealth, Is.EqualTo(70));
        }

        /// <summary>
        /// KATMAN KARARINI koruyan test — ve bir BORCU sabitleyen testin YERİNE
        /// geçti; geçiş sessiz olmasın diye burada yazılı.
        ///
        /// Selefi <c>Move_DownedUnit_StillMoves_BecauseNoRuleOwnsMovementState (SILINDI)</c>
        /// adındaydı ve tam TERSİNİ koruyordu: düşmüş bir birimin hâlâ
        /// yürüyebildiğini. O test bir davranışı değil bir BOŞLUĞU
        /// sabitliyordu — "kim hareket eder" sorusunun sahibi yoktu — ve kendi
        /// özetinde şunu yazıyordu: <i>"biri o kuralı yazarsa bu test kırmızıya
        /// dönmelidir."</i> Kural yazıldı (<see cref="MovementRules"/>), test
        /// kırmızıya döndü, ve öngörüldüğü gibi SİLİNDİ. Yerini bu aldı.
        ///
        /// Kural nerede: metni <see cref="MovementRules"/>'ta, sorusu bu akışta.
        /// <see cref="MoveAction"/>'ın kendisi soramaz — o GridStrategy.Core'da
        /// ve Core, <see cref="UnitState"/>'i GÖRMEZ. Kuralı uygulayabilen en
        /// alt katman burasıdır.
        ///
        /// Kırmızıya dönerse ya soru akıştan çıkmıştır ya da ret değeri
        /// değişmiştir; ikisinde de düşmüş birim yeniden yürümeye başlar ve
        /// "işini bitirme" tasarımı anlamsızlaşır — sürünüp giden bir birim hiç
        /// düşmemiş sayılır.
        /// </summary>
        [Test]
        public void Move_DownedUnit_IsRejected()
        {
            var battle = new Battle(3, 5);
            Combatant combatant = NewDownedCombatant();
            Unit unit = AddUnit(battle, "Vanguard", 1, 2, combatant);

            MoveOutcome outcome = BattleActions.Move(battle, unit, toX: 1, toY: 3, moveRange: 1);

            Assert.That(outcome, Is.EqualTo(MoveOutcome.RejectedActorCannotAct),
                "a downed unit cannot walk away; MovementRules owns the rule and this flow asks it");
            Assert.That(battle.TryGetPosition(unit, out int x, out int y), Is.True);
            Assert.That(x, Is.EqualTo(1), "a rejected move must not touch the board");
            Assert.That(y, Is.EqualTo(2), "a rejected move must not touch the board");
        }

        /// <summary>
        /// SABİTLEME TESTİ — sözleşmenin adıyla istediği iki testten ikincisi,
        /// ve koruduğu şey bir davranış değil bir TAVİZ.
        ///
        /// <see cref="MoveOutcome"/> <c>GridStrategy.Core</c>'da yaşıyor ve
        /// akışının sahibi <see cref="MoveAction"/> de orada — ama
        /// <see cref="MoveOutcome.RejectedActorCannotAct"/> değerini o ASLA
        /// üretemez, çünkü ne <see cref="UnitState"/>'i ne sırayı görebiliyor.
        /// Yani bir tipe, sahibinin üretemeyeceği bir değer eklendi. Bu test onu
        /// gizlemek yerine SABİTLİYOR.
        ///
        /// İki ayrı kanıt veriyor ve ikisi farklı şeyleri koruyor:
        /// <list type="number">
        /// <item>DERLEME düzeyi — Core'un assembly'si Combat'ı referans etmiyor,
        /// yani kural oraya bir gün "yolunu bulup" giremez. Biri o referansı
        /// eklerse burası kırmızıya döner ve S-01 ile S-08'in bütün gerekçesi
        /// tek satırda gözden geçirilir.</item>
        /// <item>DAVRANIŞ düzeyi — hiçbir hedef hücre, hiçbir menzil ve hiçbir
        /// aşırı yükleme bu değeri üretmiyor.</item>
        /// </list>
        /// </summary>
        [Test]
        public void MoveAction_NeverReturnsRejectedActorCannotAct()
        {
            string[] coreReferences = typeof(MoveAction).Assembly
                .GetReferencedAssemblies()
                .Select(assembly => assembly.Name)
                .ToArray();

            Assert.That(
                coreReferences,
                Has.No.Member("GridStrategy.Combat"),
                "GridStrategy.Core must not see the combat layer; that is why MoveAction cannot ask the movement rule");

            for (int toX = -1; toX <= 3; toX++)
            {
                for (int toY = -1; toY <= 3; toY++)
                {
                    for (int moveRange = 0; moveRange <= 4; moveRange++)
                    {
                        // Tahta her denemede yeniden kuruluyor: kabul edilen bir
                        // hareket tahtayı DEĞİŞTİRİYOR ve bir sonraki denemenin
                        // kaynağı kaymış olurdu.
                        var board = new UnitGrid(3, 3);
                        var mover = new Unit("Vanguard");
                        board.PlaceUnit(1, 1, mover);
                        board.PlaceUnit(1, 2, new Unit("Blocker"));

                        Assert.That(
                            MoveAction.Execute(board, mover, 1, 1, toX, toY, moveRange),
                            Is.Not.EqualTo(MoveOutcome.RejectedActorCannotAct),
                            $"the core flow produced an actor rejection it cannot possibly own "
                                + $"(toX={toX}, toY={toY}, moveRange={moveRange})");

                        var profileBoard = new UnitGrid(3, 3);
                        var profileMover = new Unit("Vanguard");
                        profileBoard.PlaceUnit(1, 1, profileMover);
                        profileBoard.PlaceUnit(1, 2, new Unit("Blocker"));

                        Assert.That(
                            MoveAction.Execute(
                                profileBoard, profileMover, 1, 1, toX, toY, new MoveProfile(moveRange)),
                            Is.Not.EqualTo(MoveOutcome.RejectedActorCannotAct),
                            $"the profile overload must answer exactly like the int one "
                                + $"(toX={toX}, toY={toY}, moveRange={moveRange})");
                    }
                }
            }
        }

        [Test]
        public void Move_UnknownUnit_Throws()
        {
            var battle = new Battle(3, 5);

            Assert.Throws<ArgumentException>(
                () => BattleActions.Move(battle, new Unit("Ghost"), toX: 0, toY: 0, moveRange: 1));
        }

        [Test]
        public void Move_NullArgument_Throws()
        {
            var battle = new Battle(3, 5);
            Unit unit = AddUnit(battle, "Vanguard", 0, 0, NewCombatant());

            Assert.Throws<ArgumentNullException>(
                () => BattleActions.Move(null, unit, toX: 1, toY: 0, moveRange: 1));
            Assert.Throws<ArgumentNullException>(
                () => BattleActions.Move(battle, null, toX: 1, toY: 0, moveRange: 1));
        }

        // ─────────────────────────────────────────────────────────────
        // Sıra — TurnState/TurnRules'ın üretimdeki İLK çağıranı
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// SIRA KARARINI koruyan test — ve üretimde ilk çağıranın kapattığı en
        /// büyük boşluk. <see cref="TurnState"/> ile <see cref="TurnRules"/>
        /// yazılmıştı, sınanmıştı ve üretimde tek bir çağıranları yoktu: sıra
        /// sistemi vardı ve hiçbir şeyi ENGELLEMİYORDU.
        ///
        /// İkinci assertion birinciden daha önemli: reddin HASAR İNMEDEN
        /// gelmesi gerekiyor. Sıra sorusu akışın sonuna konsaydı dönüş değeri
        /// yine "reddedildi" derdi ama hedefin canı çoktan azalmış olurdu — ve
        /// ilk assertion o hatayı göremezdi.
        /// </summary>
        [Test]
        public void Attack_WhenItIsNotTheAttackersTurn_IsRejectedAndDealsNoDamage()
        {
            var battle = new Battle(3, 5);
            Unit attacker = AddUnit(battle, "Vanguard", 0, 0, NewCombatant(damage: 30, team: Team.Player));
            Unit target = AddUnit(battle, "Raider", 1, 0, NewCombatant(maxHealth: 100, team: Team.Enemy));

            Assert.That(battle.Turn.Current, Is.EqualTo(Team.Player), "kurulum bozuk");
            battle.Turn.EndTurn();

            AttackOutcome outcome = BattleActions.Attack(battle, attacker, target);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.RejectedActorCannotAct),
                "the player may not act on the enemy turn");
            Assert.That(CombatantOf(battle, target).CurrentHealth, Is.EqualTo(100),
                "a turn rejection must arrive before any damage lands");
        }

        /// <summary>
        /// NEGATİF KONTROL: yukarıdaki reddin sebebi SIRA, "saldırı hep
        /// reddediliyor" değil. Tek fark saldıranın tarafı; bu test yeşil kalıp
        /// üstteki kırmızı olursa ölçtüğümüz şey gerçekten sıra kuralıdır.
        /// </summary>
        [Test]
        public void Attack_OnItsOwnTurn_TheOtherSideCanActToo()
        {
            var battle = new Battle(3, 5);
            Unit raider = AddUnit(battle, "Raider", 0, 0, NewCombatant(damage: 30, team: Team.Enemy));
            Unit vanguard = AddUnit(battle, "Vanguard", 1, 0, NewCombatant(maxHealth: 100, team: Team.Player));

            battle.Turn.EndTurn();

            Assert.That(BattleActions.Attack(battle, raider, vanguard), Is.EqualTo(AttackOutcome.Hit));
            Assert.That(CombatantOf(battle, vanguard).CurrentHealth, Is.EqualTo(70));
        }

        /// <summary>
        /// SIRA KARARININ SIRASINI koruyan test. Saldıran hem sırası gelmemiş,
        /// hem hedefi geçersiz (aynı takım), hem de menzil dışı — üç ret sebebi
        /// de geçerli ve yalnız biri doğru.
        ///
        /// "Eyleyemez" doğru olan, çünkü çağıranın yapabileceği tek şey onu
        /// göstermektedir: hedefi değiştirmek de yaklaşmak da cevabı
        /// DEĞİŞTİRMEZ. Yapay zekâ "geçersiz hedef" cevabını alsaydı bütün hedef
        /// listesini tarar, hepsinde aynı cevabı alır ve asıl engeli hiç
        /// öğrenemezdi — S-06'nın sıra dersinin dördüncü sınavı.
        ///
        /// Kırmızıya dönerse sıra kontrolü akışın içinde AŞAĞI kaymış demektir
        /// ve o değişiklik başka hiçbir testi kırmaz.
        /// </summary>
        [Test]
        public void Attack_OutOfTurnAgainstAnInvalidTargetOutOfRange_PrefersActorCannotAct()
        {
            var battle = new Battle(3, 5);
            Unit attacker = AddUnit(battle, "Vanguard", 0, 0, NewCombatant(damage: 30, range: 1, team: Team.Player));
            Unit target = AddUnit(battle, "Runner", 2, 4, NewCombatant(maxHealth: 100, team: Team.Player));

            battle.Turn.EndTurn();

            Assert.That(BattleActions.Attack(battle, attacker, target),
                Is.EqualTo(AttackOutcome.RejectedActorCannotAct),
                "the turn rule stands in front of both target eligibility and range");
        }

        /// <summary>
        /// #13'ün akıştan görünen yüzü: düşmüş bir birim artık vuramıyor.
        /// Kuralın sahibi <c>AttackRules</c> ve sorusu bir katman aşağıda
        /// (<see cref="AttackAction"/>) soruluyor — bu test yalnızca kararın bu
        /// akıştan da geçtiğini koruyor.
        /// </summary>
        [Test]
        public void Attack_DownedAttacker_IsRejectedAndDealsNoDamage()
        {
            var battle = new Battle(3, 5);
            Unit attacker = AddUnit(battle, "Vanguard", 0, 0, NewDownedCombatant(Team.Player));
            Unit target = AddUnit(battle, "Raider", 1, 0, NewCombatant(maxHealth: 100, team: Team.Enemy));

            Assert.That(BattleActions.Attack(battle, attacker, target),
                Is.EqualTo(AttackOutcome.RejectedActorCannotAct));
            Assert.That(CombatantOf(battle, target).CurrentHealth, Is.EqualTo(100));
        }

        [Test]
        public void Move_WhenItIsNotTheUnitsTurn_IsRejectedAndNothingMoves()
        {
            var battle = new Battle(3, 5);
            Unit unit = AddUnit(battle, "Vanguard", 1, 2, NewCombatant(team: Team.Player));

            battle.Turn.EndTurn();

            Assert.That(BattleActions.Move(battle, unit, toX: 1, toY: 3, moveRange: 1),
                Is.EqualTo(MoveOutcome.RejectedActorCannotAct));
            Assert.That(battle.TryGetPosition(unit, out int x, out int y), Is.True);
            Assert.That(x, Is.EqualTo(1));
            Assert.That(y, Is.EqualTo(2), "a turn rejection must not touch the board");
        }

        /// <summary>
        /// ÇAĞIRAN HATASI KARARINI koruyan test, hareket tarafında. Negatif
        /// menzil bir oyun sonucu değil bir çağıran hatasıdır ve sıra kuralının
        /// ARKASINA saklanmamalı: sırası gelmemiş bir birim için bozuk sayı da
        /// patlamalı, yoksa hata ancak sıra geldiğinde görünür.
        ///
        /// Doğrulamanın metni artık <see cref="MoveProfile"/>'ın kurucusunda —
        /// bu akış onu kurallardan ÖNCE kuruyor, tam da bu yüzden.
        /// </summary>
        [Test]
        public void Move_NegativeRangeOutOfTurn_StillThrows()
        {
            var battle = new Battle(3, 5);
            Unit unit = AddUnit(battle, "Vanguard", 1, 2, NewCombatant(team: Team.Player));

            battle.Turn.EndTurn();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => BattleActions.Move(battle, unit, toX: 1, toY: 3, moveRange: -1));
        }

        // ─────────────────────────────────────────────────────────────
        // Yapı hedefleme — akış hedefin NE olduğunu Battle'a soruyor
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// DALLANMA KARARINI koruyan test. Hedefin bir yapı olduğunu akış
        /// <see cref="Battle.TryGetStructure"/>'a SORUYOR; çağıran bir bayrak
        /// taşımıyor.
        ///
        /// Kırmızıya dönerse ya soru sorulmuyordur (akış barakayı bir Combatant
        /// sanıp "bu savaşta değil" diye patlar) ya da yapı aşırı yüklemesine
        /// hiç ulaşılmıyordur.
        /// </summary>
        [Test]
        public void Attack_StructureTarget_HitsAndDealsProfileDamage()
        {
            var battle = new Battle(3, 5);
            Unit attacker = AddUnit(battle, "Vanguard", 0, 0, NewCombatant(damage: 30, range: 1, team: Team.Player));
            Structure barracks = NewStructure(maxHealth: 100, team: Team.Enemy);

            // Çapraz komşu: mesafe yapı tarafında da GridDistance'tan geliyor.
            // Manhattan'a kopyalanmış bir formül burada 2 der ve vuruş
            // "menzil dışı" olur.
            Unit target = AddStructure(battle, "Barracks", 1, 1, barracks);

            Assert.That(BattleActions.Attack(battle, attacker, target), Is.EqualTo(AttackOutcome.Hit));
            Assert.That(barracks.CurrentHealth, Is.EqualTo(70));
            Assert.That(barracks.IsStanding, Is.True);
        }

        [Test]
        public void Attack_LethalHitOnStructure_ReportsHitAndDestroyed()
        {
            var battle = new Battle(3, 5);
            Unit attacker = AddUnit(battle, "Vanguard", 0, 0, NewCombatant(damage: 10, range: 1, team: Team.Player));
            Structure barracks = NewStructure(maxHealth: 10, team: Team.Enemy);
            Unit target = AddStructure(battle, "Barracks", 1, 0, barracks);

            Assert.That(BattleActions.Attack(battle, attacker, target),
                Is.EqualTo(AttackOutcome.HitAndDestroyed),
                "a structure is destroyed, never downed; the naming decision must survive the flow");
            Assert.That(barracks.State, Is.EqualTo(StructureState.Destroyed));
        }

        [Test]
        public void Attack_OwnStructure_IsRejectedAsInvalidTarget()
        {
            var battle = new Battle(3, 5);
            Unit attacker = AddUnit(battle, "Vanguard", 0, 0, NewCombatant(damage: 30, range: 1, team: Team.Player));
            Structure barracks = NewStructure(maxHealth: 100, team: Team.Player);
            Unit target = AddStructure(battle, "Barracks", 1, 0, barracks);

            Assert.That(BattleActions.Attack(battle, attacker, target),
                Is.EqualTo(AttackOutcome.RejectedInvalidTarget),
                "your own barracks is not a valid target, through the flow as well");
            Assert.That(barracks.CurrentHealth, Is.EqualTo(100));
        }

        [Test]
        public void Attack_StructureOutOfRange_IsRejected()
        {
            var battle = new Battle(3, 5);
            Unit attacker = AddUnit(battle, "Vanguard", 0, 0, NewCombatant(damage: 30, range: 1, team: Team.Player));
            Structure barracks = NewStructure(maxHealth: 100, team: Team.Enemy);
            Unit target = AddStructure(battle, "Barracks", 2, 4, barracks);

            Assert.That(BattleActions.Attack(battle, attacker, target),
                Is.EqualTo(AttackOutcome.RejectedOutOfRange));
            Assert.That(barracks.CurrentHealth, Is.EqualTo(100));
        }

        // ─────────────────────────────────────────────────────────────
        // Diriltme — TryRevive ve CanBeRevived'ın üretimdeki İLK çağıranı
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// <c>Combatant.TryRevive</c> ile <c>TargetingRules.CanBeRevived</c>
        /// yazılmıştı, sınanmıştı ve üretimde tek bir çağıranları yoktu — bir
        /// kurtarma penceresi vardı ve onu kullanmanın hiçbir yolu yoktu.
        ///
        /// Can oranı burada KOPYALANMIYOR: sayı <c>Combatant.ReviveHealthDivisor</c>
        /// kararından geliyor ve bu test yalnızca akışın o kararı bozmadığını
        /// koruyor.
        /// </summary>
        [Test]
        public void Revive_AdjacentDownedAlly_RaisesItWithHalfMaxHealth()
        {
            var battle = new Battle(3, 5);
            Unit medic = AddUnit(battle, "Medic", 0, 0, NewCombatant(team: Team.Player));
            Combatant fallen = NewDownedCombatant(Team.Player);
            Unit ally = AddUnit(battle, "Vanguard", 1, 0, fallen);

            ReviveOutcome outcome = BattleActions.Revive(battle, medic, ally);

            Assert.That(outcome, Is.EqualTo(ReviveOutcome.Revived));
            Assert.That(fallen.State, Is.EqualTo(UnitState.Alive));
            Assert.That(fallen.CurrentHealth, Is.EqualTo(50),
                "the revive fraction belongs to Combatant; this flow must not restate it");
        }

        [Test]
        public void Revive_AliveTarget_IsRejectedAsInvalidTarget()
        {
            var battle = new Battle(3, 5);
            Unit medic = AddUnit(battle, "Medic", 0, 0, NewCombatant(team: Team.Player));
            Unit ally = AddUnit(battle, "Vanguard", 1, 0, NewCombatant(team: Team.Player));

            Assert.That(BattleActions.Revive(battle, medic, ally),
                Is.EqualTo(ReviveOutcome.RejectedInvalidTarget),
                "a unit that is still standing has nothing to be raised from");
        }

        [Test]
        public void Revive_DownedEnemy_IsRejectedAsInvalidTarget()
        {
            // Düşmanını ayağa kaldırmak bir yetenek değil, bir hatadır. Kural
            // TargetingRules'ta yazılı; bu test onun akıştan da görünür
            // olduğunu koruyor.
            var battle = new Battle(3, 5);
            Unit medic = AddUnit(battle, "Medic", 0, 0, NewCombatant(team: Team.Player));
            Combatant fallen = NewDownedCombatant(Team.Enemy);
            Unit enemy = AddUnit(battle, "Raider", 1, 0, fallen);

            Assert.That(BattleActions.Revive(battle, medic, enemy),
                Is.EqualTo(ReviveOutcome.RejectedInvalidTarget));
            Assert.That(fallen.State, Is.EqualTo(UnitState.Downed));
        }

        [Test]
        public void Revive_OutOfRange_IsRejected()
        {
            var battle = new Battle(3, 5);
            Unit medic = AddUnit(battle, "Medic", 0, 0, NewCombatant(range: 1, team: Team.Player));
            Unit ally = AddUnit(battle, "Vanguard", 2, 4, NewDownedCombatant(Team.Player));

            Assert.That(BattleActions.Revive(battle, medic, ally),
                Is.EqualTo(ReviveOutcome.RejectedOutOfRange));
        }

        /// <summary>
        /// SIRA KARARININ DİRİLTME İKİZİ. Hedef hem kalıcı ölü hem menzil dışı;
        /// doğru cevap "geçersiz hedef", çünkü yaklaşmak bir cesedi geri
        /// getirmez. "Menzil dışı" cevabı yapay zekâya "yaklaş" dedirtir ve
        /// sonsuz döngü üretir — saldırı tarafındaki ikizinin aynısı.
        /// </summary>
        [Test]
        public void Revive_DeadTargetOutOfRange_PrefersInvalidTargetOverOutOfRange()
        {
            var battle = new Battle(3, 5);
            Unit medic = AddUnit(battle, "Medic", 0, 0, NewCombatant(range: 1, team: Team.Player));
            Combatant fallen = NewDownedCombatant(Team.Player);
            Unit ally = AddUnit(battle, "Vanguard", 2, 4, fallen);
            fallen.Tick(10.1f);
            Assert.That(fallen.State, Is.EqualTo(UnitState.Dead), "kurulum bozuk");

            Assert.That(BattleActions.Revive(battle, medic, ally),
                Is.EqualTo(ReviveOutcome.RejectedInvalidTarget),
                "a permanently dead target must be reported as invalid even when it is also out of range");
        }

        [Test]
        public void Revive_WhenItIsNotTheReviversTurn_IsRejectedAndNothingIsRaised()
        {
            var battle = new Battle(3, 5);
            Unit medic = AddUnit(battle, "Medic", 0, 0, NewCombatant(team: Team.Player));
            Combatant fallen = NewDownedCombatant(Team.Player);
            Unit ally = AddUnit(battle, "Vanguard", 1, 0, fallen);

            battle.Turn.EndTurn();

            Assert.That(BattleActions.Revive(battle, medic, ally),
                Is.EqualTo(ReviveOutcome.RejectedActorCannotAct));
            Assert.That(fallen.State, Is.EqualTo(UnitState.Downed),
                "a turn rejection must arrive before the target is touched");
        }

        /// <summary>
        /// KARARI KORUYAN test — ve bir borcun kapanış kaydı.
        ///
        /// Bu satırların yerinde
        /// <c>Revive_DownedReviver_StillRevives_BecauseNoRuleOwnsReviverState</c>
        /// (SILINDI) duruyordu: açık bir boşluğu sabitleyen,
        /// "biri kuralı yazdığı gün bilerek kırmızıya dönsün" diye yazılmış bir
        /// test. Kural yazıldı (<see cref="ReviveRules"/>), test kırmızıya döndü,
        /// ve yerini bu aldı. Geçiş sessiz olmadı — bu dosyada ikinci kez.
        /// Birincisi hareket tarafındaydı ve aynı deseni izlemişti.
        ///
        /// Korunan karar: diriltmek bir EYLEMDİR ve eyleyenin kendisi ayakta
        /// olmalıdır. Kuralın sahibi akış değil, <see cref="ReviveRules"/>;
        /// burada yalnızca SORULUYOR. Kuralı bu metodun içine yazmanın reddi
        /// <c>BattleActions.Revive</c>'da KOD olarak duruyor.
        /// </summary>
        [Test]
        public void Revive_DownedReviver_IsRejected()
        {
            var battle = new Battle(3, 5);
            Unit medic = AddUnit(battle, "Medic", 0, 0, NewDownedCombatant(Team.Player));
            Combatant fallen = NewDownedCombatant(Team.Player);
            Unit ally = AddUnit(battle, "Vanguard", 1, 0, fallen);

            Assert.That(BattleActions.Revive(battle, medic, ally),
                Is.EqualTo(ReviveOutcome.RejectedActorCannotAct),
                "a fallen medic cannot lift anyone");
            Assert.That(fallen.State, Is.EqualTo(UnitState.Downed),
                "the rejected revive must leave the target on the ground");
        }

        /// <summary>
        /// SIRA DEVRİ — ve bu testler bir ÇÖKMEYİ değil bir SESSİZLİĞİ koruyor.
        ///
        /// <c>TurnRules.CanAct</c> üç eylemde de soruluyordu ama
        /// <c>TurnState.EndTurn</c> üretimde HİÇ çağrılmıyordu. Sonuç: Player
        /// sonsuza kadar oynar, Enemy hiç sıra almazdı. Hiçbir test kırmızı
        /// değildi çünkü her test TEK bir eylem sınıyordu — kırılma yalnızca
        /// İKİNCİ eylemde görünür.
        ///
        /// Bir kuralı SORMAK ile onu İŞLETMEK farklı işlerdir; ilki yazılmıştı.
        /// </summary>
        [Test]
        public void SuccessfulAttack_HandsTheTurnOver()
        {
            var battle = new Battle(3, 5);
            Unit attacker = AddUnit(battle, "Vanguard", 0, 0, NewCombatant(team: Team.Player));
            Unit target = AddUnit(battle, "Raider", 1, 0, NewCombatant(maxHealth: 100, team: Team.Enemy));

            Assert.That(battle.Turn.Current, Is.EqualTo(Team.Player));
            Assert.That(BattleActions.Attack(battle, attacker, target), Is.EqualTo(AttackOutcome.Hit));

            Assert.That(battle.Turn.Current, Is.EqualTo(Team.Enemy),
                "one action per turn: a landed attack ends the attacker's turn");
            Assert.That(BattleActions.Attack(battle, attacker, target),
                Is.EqualTo(AttackOutcome.RejectedActorCannotAct),
                "the same unit cannot act twice in one turn");
        }

        /// <summary>
        /// Ret sırayı YAKMAZ — ve bu, sıra devrinin ikinci yarısı.
        ///
        /// Beyaz liste burada ölçülüyor: yalnızca gerçekten olmuş bir eylem
        /// sırayı devreder. Kara liste yazılsaydı bu test bugün de yeşil olurdu
        /// ve yarın eklenen bir ret değeri sırayı sessizce yakardı. Oynanış
        /// karşılığı: menzil dışına yapılan tek bir yanlış tıklama turu
        /// bitirmemelidir.
        /// </summary>
        [Test]
        public void RejectedAction_DoesNotHandTheTurnOver()
        {
            var battle = new Battle(3, 5);
            Unit attacker = AddUnit(battle, "Vanguard", 0, 0, NewCombatant(range: 1, team: Team.Player));
            Unit target = AddUnit(battle, "Raider", 2, 4, NewCombatant(maxHealth: 100, team: Team.Enemy));

            Assert.That(BattleActions.Attack(battle, attacker, target),
                Is.EqualTo(AttackOutcome.RejectedOutOfRange));
            Assert.That(battle.Turn.Current, Is.EqualTo(Team.Player),
                "a rejected attempt must not cost the player their turn");

            Assert.That(BattleActions.Move(battle, attacker, toX: 2, toY: 4, moveRange: 1),
                Is.EqualTo(MoveOutcome.RejectedOutOfRange));
            Assert.That(battle.Turn.Current, Is.EqualTo(Team.Player),
                "a rejected move must not cost the player their turn either");
        }

        /// <summary>
        /// Yerleştirme sırayı devretmez — çünkü EYLEYENİ yok.
        ///
        /// <c>PlaceStructure</c>'ın imzasında bir eyleyen taşınmıyor ve sıra
        /// kuralı da bu yüzden sorulmuyor (gerekçe <c>PlacementOutcome.cs</c>'te
        /// KOD olarak yazılı). Sormayan bir eylem devredemez de; bu test o
        /// simetriyi sabitliyor.
        /// </summary>
        [Test]
        public void PlaceStructure_DoesNotHandTheTurnOver()
        {
            var battle = new Battle(3, 5);
            Unit depot = new Unit("Depot");

            Assert.That(BattleActions.PlaceStructure(battle, depot, NewStructure(), 1, 1),
                Is.EqualTo(PlacementOutcome.Placed));
            Assert.That(battle.Turn.Current, Is.EqualTo(Team.Player),
                "placement carries no actor, so it spends no actor's turn");
        }

        [Test]
        public void Revive_UnknownUnit_Throws()
        {
            var battle = new Battle(3, 5);
            Unit medic = AddUnit(battle, "Medic", 0, 0, NewCombatant(team: Team.Player));

            Assert.Throws<ArgumentException>(
                () => BattleActions.Revive(battle, medic, new Unit("Ghost")));
            Assert.Throws<ArgumentException>(
                () => BattleActions.Revive(battle, new Unit("Ghost"), medic));
        }

        [Test]
        public void Revive_NullArgument_Throws()
        {
            var battle = new Battle(3, 5);
            Unit medic = AddUnit(battle, "Medic", 0, 0, NewCombatant(team: Team.Player));
            Unit ally = AddUnit(battle, "Vanguard", 1, 0, NewDownedCombatant(Team.Player));

            Assert.Throws<ArgumentNullException>(() => BattleActions.Revive(null, medic, ally));
            Assert.Throws<ArgumentNullException>(() => BattleActions.Revive(battle, null, ally));
            Assert.Throws<ArgumentNullException>(() => BattleActions.Revive(battle, medic, null));
        }

        // ─────────────────────────────────────────────────────────────
        // Yerleştirme — #10'un çekirdek tarafı
        // ─────────────────────────────────────────────────────────────

        [Test]
        public void PlaceStructure_OnAnEmptyCell_PlacesAndJoinsTheBattle()
        {
            var battle = new Battle(3, 5);
            var unit = new Unit("Barracks");
            Structure barracks = NewStructure(maxHealth: 100, team: Team.Player);

            PlacementOutcome outcome = BattleActions.PlaceStructure(battle, unit, barracks, 1, 1);

            Assert.That(outcome, Is.EqualTo(PlacementOutcome.Placed));
            Assert.That(battle.StructureCount, Is.EqualTo(1));
            Assert.That(battle.TryGetStructure(unit, out Structure placed), Is.True);
            Assert.That(placed, Is.SameAs(barracks));
            Assert.That(battle.TryGetUnit(1, 1, out Unit standing), Is.True,
                "a structure occupies a board cell like any other unit");
            Assert.That(standing, Is.SameAs(unit));
        }

        /// <summary>
        /// SIRA KARARINI koruyan test, yerleştirme tarafında: tahta sınırı
        /// doluluktan ÖNCE soruluyor. Tahta dışı bir hücre beklemekle geçerli
        /// olmaz, dolu bir hücre bir tur sonra boşalabilir — düzeltilemeyen sebep
        /// önce söylenir. Aynı ölçüt <see cref="MoveAction"/>'da da yazılı.
        /// </summary>
        [Test]
        public void PlaceStructure_OutsideTheBoard_IsRejectedAsInvalidCell()
        {
            var battle = new Battle(3, 5);

            Assert.That(
                BattleActions.PlaceStructure(battle, new Unit("Barracks"), NewStructure(), 3, 0),
                Is.EqualTo(PlacementOutcome.RejectedInvalidCell));
            Assert.That(battle.StructureCount, Is.EqualTo(0),
                "a rejected placement must not join the battle");
        }

        /// <summary>
        /// AYNI OLGUNUN İKİ CEVABINI koruyan test. <c>Battle.AddStructure</c>
        /// dolu hücreyi bir ÇAĞIRAN HATASI sayıyor ve patlıyor; bu akış aynı
        /// durumu bir OYUN SONUCU olarak döndürüyor. Fark çağıranın kim olduğu:
        /// katılım bir kayıt işidir, yerleştirme bir fare hamlesidir.
        ///
        /// Kırmızıya dönerse ya ön kontrol kalkmış (oyuncunun tıklaması bir
        /// istisnaya dönüşür) ya da yerleştirme dolu hücrenin üstüne yazmış
        /// demektir — ikincisinde aynı hücrede iki şey durur ve hiçbir derleme
        /// hatası çıkmaz.
        /// </summary>
        [Test]
        public void PlaceStructure_OnAnOccupiedCell_IsRejectedAndNothingIsPlaced()
        {
            var battle = new Battle(3, 5);
            AddUnit(battle, "Vanguard", 1, 1, NewCombatant());

            PlacementOutcome outcome = BattleActions.PlaceStructure(
                battle, new Unit("Barracks"), NewStructure(), 1, 1);

            Assert.That(outcome, Is.EqualTo(PlacementOutcome.RejectedCellOccupied));
            Assert.That(battle.StructureCount, Is.EqualTo(0));
            Assert.That(battle.TryGetUnit(1, 1, out Unit standing), Is.True);
            Assert.That(standing.Name, Is.EqualTo("Vanguard"), "the standing unit must be untouched");
        }

        /// <summary>
        /// SAHİPSİZLİK KARARINI koruyan test: yerleştirmenin bir EYLEYENİ yok,
        /// dolayısıyla sıra kuralı da sorulmuyor.
        ///
        /// İmzada eyleyen diye bir taraf geçmiyor; geçen tek taraf
        /// YERLEŞTİRİLEN yapının kendi takımı. Onu eyleyenin takımı sanmak
        /// ölçülebilir biçimde kırılır: <see cref="Team.None"/> hiçbir sırada
        /// eyleyemez, yani tarafsız yıkılabilir duvarlar tahtaya bir daha hiç
        /// konamazdı. Gerekçenin tamamı PlacementOutcome.cs'te.
        ///
        /// Kırmızıya dönerse biri sıra kuralını buraya da eklemiştir — ve o gün
        /// doğru düzeltme kuralı çıkarmak değil, imzaya gerçek bir eyleyen
        /// eklemektir.
        /// </summary>
        [Test]
        public void PlaceStructure_NeutralStructureOutOfTurn_IsStillPlaced()
        {
            var battle = new Battle(3, 5);
            battle.Turn.EndTurn();

            Assert.That(
                BattleActions.PlaceStructure(
                    battle, new Unit("Wall"), NewStructure(team: Team.None), 1, 1),
                Is.EqualTo(PlacementOutcome.Placed),
                "placement carries no actor; borrowing the structure's own team would freeze out every neutral wall");
            Assert.That(battle.StructureCount, Is.EqualTo(1));
        }

        [Test]
        public void PlaceStructure_SameIdentityTwice_Throws()
        {
            // "Bu birim zaten savaşta" bir ret değil, kaydın Battle ile
            // ayrışmasıdır — ve kontrolü bu akışta KOPYALANMIYOR, Battle'a
            // bırakılıyor.
            var battle = new Battle(3, 5);
            var unit = new Unit("Barracks");

            Assert.That(BattleActions.PlaceStructure(battle, unit, NewStructure(), 1, 1),
                Is.EqualTo(PlacementOutcome.Placed));

            Assert.Throws<ArgumentException>(
                () => BattleActions.PlaceStructure(battle, unit, NewStructure(), 2, 2));
        }

        // ═══ TAKIM BAŞINA TEK ANA KULE ═══════════════════════════════════
        // Kuralın var olma sebebi zafer koşulunun kendisi: ana kulesi yıkılan
        // taraf kaybediyor, dolayısıyla ikinci bir kule kurmaya izin veren bir
        // oyunda o koşul hiçbir zaman gerçekleşmezdi.

        private static Structure NewHeadquarters(int maxHealth = 100, Team team = Team.Enemy)
        {
            return new Structure(
                new Health(maxHealth), new StructureLifecycle(), team,
                attackProfile: null, isHeadquarters: true);
        }

        [Test]
        public void PlaceStructure_ASecondHeadquartersForTheSameTeam_IsRejected()
        {
            var battle = new Battle(3, 5);

            Assert.That(
                BattleActions.PlaceStructure(battle, new Unit("HQ"), NewHeadquarters(), 1, 1),
                Is.EqualTo(PlacementOutcome.Placed));

            Assert.That(
                BattleActions.PlaceStructure(battle, new Unit("HQ 2"), NewHeadquarters(), 2, 2),
                Is.EqualTo(PlacementOutcome.RejectedHeadquartersExists));

            Assert.That(battle.StructureCount, Is.EqualTo(1),
                "a rejected placement writes nothing");
        }

        /// <summary>
        /// KURAL TAKIM BAŞINA, TAHTA BAŞINA DEĞİL: iki tarafın da kendi ana
        /// kulesi olmalı.
        /// </summary>
        // BU TESTİN YOKLUĞU KURALI SESSİZCE İKİYE KATLARDI: "ayakta ana kule
        // var mı" sorusu takımı sormasaydı, oyuncunun kulesini kuran ikinci
        // çağrı düşmanın kulesini reddederdi ve düşman hiç kule kuramazdı.
        [Test]
        public void PlaceStructure_AHeadquartersForTheOtherTeam_IsPlaced()
        {
            var battle = new Battle(3, 5);

            BattleActions.PlaceStructure(
                battle, new Unit("Enemy HQ"), NewHeadquarters(team: Team.Enemy), 1, 1);

            Assert.That(
                BattleActions.PlaceStructure(
                    battle, new Unit("Player HQ"), NewHeadquarters(team: Team.Player), 2, 2),
                Is.EqualTo(PlacementOutcome.Placed));
        }

        /// <summary>
        /// YIKILMIŞ bir kule ikinciyi engellemez.
        /// </summary>
        // SORU "AYAKTA MI" DİYE YAZILDI, "HİÇ KURDU MU" DİYE DEĞİL — ve bu test
        // o tercihi koruyor. Ters yazılsaydı ret, savaş bittikten sonra da
        // konuşmaya devam ederdi; oysa kulesi yıkılan taraf zaten kaybetmiş
        // durumda ve onu ayrıca kelepçelemenin bir karşılığı yok.
        [Test]
        public void PlaceStructure_AfterTheHeadquartersFell_ASecondOneIsAllowed()
        {
            var battle = new Battle(3, 5);

            Structure hq = NewHeadquarters(maxHealth: 10);
            BattleActions.PlaceStructure(battle, new Unit("HQ"), hq, 1, 1);

            hq.TakeDamage(10);
            Assert.That(hq.State, Is.EqualTo(StructureState.Destroyed), "kurulum bozuk");

            Assert.That(
                BattleActions.PlaceStructure(battle, new Unit("HQ 2"), NewHeadquarters(), 2, 2),
                Is.EqualTo(PlacementOutcome.Placed));
        }

        [Test]
        public void PlaceStructure_NullArgument_Throws()
        {
            var battle = new Battle(3, 5);

            Assert.Throws<ArgumentNullException>(
                () => BattleActions.PlaceStructure(null, new Unit("Barracks"), NewStructure(), 1, 1));
            Assert.Throws<ArgumentNullException>(
                () => BattleActions.PlaceStructure(battle, null, NewStructure(), 1, 1));
            Assert.Throws<ArgumentNullException>(
                () => BattleActions.PlaceStructure(battle, new Unit("Barracks"), null, 1, 1));
        }
    }
}
