using System;
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
    /// "BİR TAKIM NE ZAMAN OYUNDAN ÇIKAR" sorusunu koruyan dosya.
    ///
    /// Var olma sebebi ölçülmüş bir kusur: zafer koşulu yalnızca askerlere
    /// bakıyordu, yani düşmanın son askeri düştüğü anda barakası ve kulesi
    /// AYAKTAYKEN "kazandın" yazıyordu. Yapılar zaten saldırılabilir hedef;
    /// hedef olan bir şeyin yıkılmadan sayılmaması oyuncuya yıkacak bir şey
    /// bırakmıyordu.
    ///
    /// KARAR: ayakta bir yapısı kalan takım hâlâ oyundadır — üssünü yıkmadan
    /// kazanmış sayılmazsın. Enkaz saymaz.
    ///
    /// İKİ ÜYE, İKİ AYRI SORU ve bu dosya ikisini de koruyor:
    /// <c>HasUnitsLeft</c> hâlâ yalnız savaşçıları sayıyor (anlamı DEĞİŞMEDİ),
    /// <c>IsTeamInPlay</c> ise onu kapsayıp yapıları da soruyor. İkincisi
    /// birincisinin yerine geçseydi, "kaç savaşçı kaldı" sorusunu soran her
    /// çağıran sessizce yanlış cevap alırdı.
    ///
    /// Saf kural (<c>VictoryRules.Winner(bool, bool)</c>) burada
    /// TEKRARLANMIYOR; dört hâlini <see cref="VictoryRulesTests"/> sınıyor.
    /// Burada sınanan şey kurala hangi İKİ girdinin verildiği.
    /// </summary>
    public sealed class TeamInPlayTests
    {
        private static Combatant NewCombatant(int maxHealth = 10, Team team = Team.Player)
        {
            return new Combatant(
                new Health(maxHealth),
                new UnitLifecycle(downedWindowSeconds: 10f, corpseWindowSeconds: 5f),
                new AttackProfile(damage: 10, range: 1),
                team);
        }

        private static Structure NewStructure(int maxHealth = 50, Team team = Team.Enemy)
        {
            return new Structure(new Health(maxHealth), new StructureLifecycle(), team);
        }

        // Bir savaşçıyı kalıcı ölüye götürmenin tam yolu: önce can, sonra düşme
        // penceresi. Kısayol yok, çünkü kısayol tam da sınanan kuralı atlardı.
        private static void KillOutright(Combatant combatant, int maxHealth)
        {
            combatant.TakeDamage(maxHealth);
            combatant.Tick(10.1f);
            Assert.That(combatant.State, Is.EqualTo(UnitState.Dead), "kurulum bozuk");
        }

        /// <summary>
        /// KUSURUN KENDİSİ: düşmanın son askeri öldü ama barakası ayakta —
        /// kazanan yok.
        /// </summary>
        [Test]
        public void WhenTheLastEnemySoldierFalls_ButItsBarracksStands_NobodyHasWonYet()
        {
            var battle = new Battle(8, 8);

            battle.AddUnit(new Unit("Player soldier"), NewCombatant(team: Team.Player), 0, 0);

            var enemyUnit = new Unit("Enemy soldier");
            Combatant enemy = NewCombatant(team: Team.Enemy);
            battle.AddUnit(enemyUnit, enemy, 5, 5);
            battle.AddStructure(new Unit("Enemy barracks"), NewStructure(), 6, 6);

            KillOutright(enemy, maxHealth: 10);

            Assert.That(battle.HasUnitsLeft(Team.Enemy), Is.False,
                "the soldier question is unchanged: no soldiers are left");
            Assert.That(battle.IsTeamInPlay(Team.Enemy), Is.True,
                "a standing barracks keeps the team in the game");
            Assert.That(VictoryRules.Winner(battle), Is.EqualTo(Team.None),
                "you have not won until the base is down");
        }

        /// <summary>
        /// Üs de yıkılınca kazanan ilan edilir — kural bir tarafı sonsuza dek
        /// oyunda tutmuyor.
        /// </summary>
        [Test]
        public void OnceTheLastEnemyStructureIsRubble_ThePlayerWins()
        {
            var battle = new Battle(8, 8);

            battle.AddUnit(new Unit("Player soldier"), NewCombatant(team: Team.Player), 0, 0);

            Combatant enemy = NewCombatant(team: Team.Enemy);
            battle.AddUnit(new Unit("Enemy soldier"), enemy, 5, 5);

            Structure barracks = NewStructure();
            battle.AddStructure(new Unit("Enemy barracks"), barracks, 6, 6);

            KillOutright(enemy, maxHealth: 10);
            Assert.That(barracks.TakeDamage(50), Is.True, "kurulum bozuk");

            Assert.That(battle.IsTeamInPlay(Team.Enemy), Is.False,
                "rubble is not a base; it keeps nobody in the game");
            Assert.That(VictoryRules.Winner(battle), Is.EqualTo(Team.Player));
        }

        /// <summary>
        /// ENKAZ SAYMAZ: yıkılmış bir yapı tek başına bir takımı oyunda tutamaz.
        ///
        /// Kırmızıya dönerse ayakta olma kapısı düşmüş demektir ve bir kez üs
        /// kuran taraf sonsuza dek yenilmez olur — savaş hiç bitmez.
        /// </summary>
        [Test]
        public void RubbleAloneDoesNotKeepATeamInPlay()
        {
            var battle = new Battle(8, 8);

            Structure wreck = NewStructure();
            battle.AddStructure(new Unit("Enemy barracks"), wreck, 6, 6);
            Assert.That(wreck.TakeDamage(50), Is.True, "kurulum bozuk");

            Assert.That(battle.IsTeamInPlay(Team.Enemy), Is.False);
        }

        /// <summary>
        /// <c>HasUnitsLeft</c>'İN ANLAMI DEĞİŞMEDİ: yapılar hâlâ o soruya
        /// karışmıyor. İki üye aynı savaşta farklı cevap veriyor ve fark tam da
        /// yapının kendisi.
        /// </summary>
        [Test]
        public void HasUnitsLeft_StillIgnoresStructures_WhileIsTeamInPlayCountsThem()
        {
            var battle = new Battle(8, 8);
            battle.AddStructure(new Unit("Enemy barracks"), NewStructure(), 6, 6);

            Assert.That(battle.HasUnitsLeft(Team.Enemy), Is.False,
                "this member answers a narrower question and must keep answering it");
            Assert.That(battle.IsTeamInPlay(Team.Enemy), Is.True);
        }

        /// <summary>
        /// Düşmüş bir savaşçı hâlâ takımı oyunda tutar — diriltilebilir olduğu
        /// için. Yapı kuralı bu eski kararı ezmedi.
        /// </summary>
        [Test]
        public void ADownedSoldierWithNoStructures_StillKeepsTheTeamInPlay()
        {
            var battle = new Battle(8, 8);

            Combatant enemy = NewCombatant(team: Team.Enemy);
            battle.AddUnit(new Unit("Enemy soldier"), enemy, 5, 5);
            enemy.TakeDamage(10);
            Assert.That(enemy.State, Is.EqualTo(UnitState.Downed), "kurulum bozuk");

            Assert.That(battle.IsTeamInPlay(Team.Enemy), Is.True);
        }

        /// <summary>
        /// İKİ TARAF DA BİTTİĞİNDE BUGÜNKÜ SONUÇ KORUNUYOR: kazanan yok.
        /// </summary>
        [Test]
        public void WhenBothSidesAreGone_ThereIsStillNoWinner()
        {
            var battle = new Battle(8, 8);

            Combatant player = NewCombatant(team: Team.Player);
            battle.AddUnit(new Unit("Player soldier"), player, 0, 0);
            Structure playerBase = NewStructure(team: Team.Player);
            battle.AddStructure(new Unit("Player barracks"), playerBase, 1, 1);

            Combatant enemy = NewCombatant(team: Team.Enemy);
            battle.AddUnit(new Unit("Enemy soldier"), enemy, 5, 5);
            Structure enemyBase = NewStructure();
            battle.AddStructure(new Unit("Enemy barracks"), enemyBase, 6, 6);

            KillOutright(player, maxHealth: 10);
            KillOutright(enemy, maxHealth: 10);
            Assert.That(playerBase.TakeDamage(50), Is.True, "kurulum bozuk");
            Assert.That(enemyBase.TakeDamage(50), Is.True, "kurulum bozuk");

            Assert.That(VictoryRules.Winner(battle), Is.EqualTo(Team.None),
                "mutual destruction has no winner, exactly as before");
        }

        /// <summary>
        /// Boş savaşta da kazanan yok — kural bir tarafı yokluğuyla galip
        /// saymıyor.
        /// </summary>
        [Test]
        public void AnEmptyBattle_HasNoWinner()
        {
            var battle = new Battle(8, 8);

            Assert.That(battle.IsTeamInPlay(Team.Player), Is.False);
            Assert.That(battle.IsTeamInPlay(Team.Enemy), Is.False);
            Assert.That(VictoryRules.Winner(battle), Is.EqualTo(Team.None));
        }

        /// <summary>
        /// Savaş verilmemişse bu bir ÇAĞIRAN hatasıdır, bir berabere değil:
        /// sessizce <see cref="Team.None"/> dönmek "kimse kazanmadı" ile
        /// "kimse sormadı" hâllerini ayırt edilemez kılardı.
        /// </summary>
        [Test]
        public void Winner_NullBattle_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => VictoryRules.Winner(null));
        }

        // ═══ ANA KULE DALI ═══════════════════════════════════════════════
        // Üstteki testler İMHA kuralını koruyor ve o kural DEĞİŞMEDİ; aşağıdaki
        // üç test, ana kule kurmuş bir takım için devreye giren İKİNCİ dalı
        // koruyor. İkisi bir arada olmalı, çünkü asıl kırılma noktası dalların
        // KESİŞİMİ: hangi takımın hangi kuralla yaşadığını söyleyen tek şey
        // "hiç ana kule kurdu mu" sorusudur.

        private static Structure NewHeadquarters(int maxHealth = 50, Team team = Team.Enemy)
        {
            return new Structure(
                new Health(maxHealth), new StructureLifecycle(), team,
                attackProfile: null, isHeadquarters: true);
        }

        /// <summary>
        /// ANA KULE AYAKTAYKEN takım oyunda — askeri kalmasa bile.
        /// </summary>
        // İMHA KURALININ TERSİ BİR CEVAP ve tam da bu yüzden sınanıyor: eski
        // dalda "askeri yok ama yapısı var" cevabı da true'ydu, yani bu test tek
        // başına iki dalı AYIRT ETMEZ. Ayrımı bir sonraki test yapıyor.
        [Test]
        public void WithAStandingHeadquarters_TheTeamIsInPlay()
        {
            var battle = new Battle(8, 8);
            battle.AddStructure(new Unit("Enemy HQ"), NewHeadquarters(), 6, 6);

            Assert.That(battle.HasEverPlacedHeadquarters(Team.Enemy), Is.True);
            Assert.That(battle.IsTeamInPlay(Team.Enemy), Is.True);
        }

        /// <summary>
        /// KURALIN KENDİSİ: ana kule yıkılınca takım oyundan çıkar — askerleri
        /// ve öteki binaları AYAKTA olsa bile.
        /// </summary>
        // BU TEST ESKİ KURALLA GEÇMEZ ve ölçüsü şu: aynı kurulumda eski dal
        // "askeri var" diyip true dönerdi. Yani bu satır, iki dalın gerçekten
        // ayrıldığını kanıtlayan tek satır.
        [Test]
        public void WhenTheHeadquartersFalls_TheTeamIsOut_EvenWithSoldiersAndOtherBuildingsAlive()
        {
            var battle = new Battle(8, 8);

            Structure hq = NewHeadquarters(maxHealth: 10);
            battle.AddStructure(new Unit("Enemy HQ"), hq, 6, 6);

            // ÖTEKİ İKİSİ BİLEREK SAĞ BIRAKILIYOR: kuralın "yalnız kuleye bakar"
            // olduğunu ancak sağ kalan başka şeylerle sınayabiliriz.
            battle.AddUnit(new Unit("Enemy soldier"), NewCombatant(team: Team.Enemy), 5, 5);
            battle.AddStructure(new Unit("Enemy barracks"), NewStructure(), 4, 4);

            hq.TakeDamage(10);
            Assert.That(hq.State, Is.EqualTo(StructureState.Destroyed), "kurulum bozuk");

            Assert.That(battle.HasUnitsLeft(Team.Enemy), Is.True,
                "the soldier question is unchanged: a soldier is still alive");
            Assert.That(battle.IsTeamInPlay(Team.Enemy), Is.False,
                "the headquarters branch overrides the annihilation branch");
        }

        /// <summary>
        /// ANA KULE HİÇ KURULMAMIŞSA eski imha kuralı aynen sürüyor.
        /// </summary>
        // BU TESTİN YOKLUĞU YENİ KURALI KENDİ KENDİNİ YEMİŞ HÂLDE BIRAKIRDI:
        // tek dala indirilseydi, ana kulesi olmayan bir taraf ya hiç
        // kaybedemez ya da daha ilk karede kaybetmiş olurdu. Serbest
        // yerleştirme, kum havuzu ve öteki testlerin hepsi bu daldan geçiyor.
        [Test]
        public void WithNoHeadquartersEverPlaced_TheOldAnnihilationRuleStillApplies()
        {
            var battle = new Battle(8, 8);
            battle.AddStructure(new Unit("Enemy barracks"), NewStructure(), 6, 6);

            Assert.That(battle.HasEverPlacedHeadquarters(Team.Enemy), Is.False);
            Assert.That(battle.IsTeamInPlay(Team.Enemy), Is.True,
                "a standing building still keeps a headquarters-less team in play");
        }
    }
}
