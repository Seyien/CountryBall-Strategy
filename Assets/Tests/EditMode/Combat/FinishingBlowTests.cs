using NUnit.Framework;
using GridStrategy.Combat;

namespace GridStrategy.Tests.EditMode.Combat
{
    /// <summary>
    /// BİTİRİCİ VURUŞU koruyan dosya: düşmüş bir bedene vurmak onu kalıcı ölüye
    /// geçirir ve sonucun adı <see cref="AttackOutcome.HitAndFinished"/>'dır.
    ///
    /// Var olma sebebi sahada ölçülmüş bir kusur: düşmüş düşmana yapılan vuruş
    /// "isabet" diyordu, hedef yerinde duruyordu, bekleme süresi harcanıyordu ve
    /// o beden 10 saniye düşmüş + 5 saniye ceset olarak hücreyi işgal ediyordu —
    /// dokunulamaz 15 saniyelik bir baraj. Düşme penceresi artık İKİ tarafa
    /// birden açık: dost kaldırır, düşman bitirir.
    ///
    /// <see cref="AttackActionTests"/> bu turda AKIŞIN cevabını sınıyor. Burada
    /// sınanan şey akışın YAN ETKİLERİ: beklemenin bitirici vuruşta da
    /// harcandığı, ölü hedefin hâlâ geçersiz olduğu, ceset penceresinin hangi
    /// yoldan gelindiğine bakmadığı ve zaman güdümlü kapanışın hiç bozulmadığı.
    ///
    /// Kırmızıya dönerse ya bitirme yolu kapanmıştır ya da açılırken zamanla
    /// gelen kapanışı ezmiştir; ikisi de kodu derlenir bırakır.
    /// </summary>
    public sealed class FinishingBlowTests
    {
        // Düşme canı havuzu maksimum canın yarısı, yani buradaki 10 canlı hedefin
        // havuzu 5. Testlerdeki 5 hasar bir denge sayısı değil, o havuzu TAM
        // boşaltan sayı; bir çentiklik hasar yazılsaydı testler bitirici vuruşu
        // değil havuzun büyüklüğünü ölçerdi.
        private const int DownedPoolOfATenHealthBody = 5;

        private static Combatant NewCombatant(
            int maxHealth = 100,
            int damage = 10,
            int range = 1,
            float cooldownSeconds = 0f,
            Team team = Team.Player)
        {
            return new Combatant(
                new Health(maxHealth),
                new UnitLifecycle(downedWindowSeconds: 10f, corpseWindowSeconds: 5f),
                new AttackProfile(damage: damage, range: range, cooldownSeconds: cooldownSeconds),
                team);
        }

        // Hedefi düşürmenin en kısa yolu doğrudan hasar: kurulum sınanan şeyi
        // (AttackAction) kullansaydı test kendi kendini doğrulamış olurdu.
        private static Combatant NewDownedCombatant(Team team = Team.Enemy)
        {
            Combatant combatant = NewCombatant(maxHealth: 10, team: team);
            combatant.TakeDamage(10);
            Assert.That(combatant.State, Is.EqualTo(UnitState.Downed), "kurulum bozuk");
            return combatant;
        }

        /// <summary>
        /// Bitirici vuruş da bir vuruştur: bekleme süresini sıradan bir isabetle
        /// aynı biçimde harcar.
        ///
        /// Kırmızıya dönerse bitirme yolu bekleme kapısının ÜSTÜNDEN geçmeye
        /// başlamıştır ve hızlı tıklayan oyuncu bütün yerde yatanları tek karede
        /// silebilir.
        /// </summary>
        [Test]
        public void FinishingBlow_SpendsTheCooldownExactlyLikeAnOrdinaryHit()
        {
            Combatant attacker = NewCombatant(
                damage: DownedPoolOfATenHealthBody, range: 1, cooldownSeconds: 2f);
            Combatant target = NewDownedCombatant();

            Assert.That(
                AttackAction.Execute(attacker, target, distance: 1),
                Is.EqualTo(AttackOutcome.HitAndFinished));
            Assert.That(target.State, Is.EqualTo(UnitState.Dead),
                "the finishing blow closes the downed window for good");

            Assert.That(attacker.AttackCooldownRemaining, Is.EqualTo(2f),
                "a finishing blow fills the counter exactly like any other hit");
            Assert.That(attacker.IsAttackReady, Is.False);

            // İkinci yerde yatan hedef aynı karede: saldıran bekliyor olmalı.
            Assert.That(
                AttackAction.Execute(attacker, NewDownedCombatant(), distance: 1),
                Is.EqualTo(AttackOutcome.RejectedOnCooldown));
        }

        /// <summary>
        /// Kalıcı ölü hâlâ GEÇERSİZ hedeftir ve reddedilen vuruş beklemeyi
        /// harcamaz — bitirme yolu açılırken bu iki kural ezilmedi.
        /// </summary>
        [Test]
        public void DeadTarget_IsStillAnInvalidTarget_AndTheAttackerKeepsItsShot()
        {
            Combatant attacker = NewCombatant(
                damage: DownedPoolOfATenHealthBody, range: 1, cooldownSeconds: 2f);
            Combatant target = NewDownedCombatant();
            target.Tick(10.1f);
            Assert.That(target.State, Is.EqualTo(UnitState.Dead), "kurulum bozuk");

            Assert.That(
                AttackAction.Execute(attacker, target, distance: 1),
                Is.EqualTo(AttackOutcome.RejectedInvalidTarget),
                "a corpse has nothing left to finish; it stays an invalid target");

            Assert.That(attacker.AttackCooldownRemaining, Is.EqualTo(0f),
                "a rejected shot never spends the counter");
            Assert.That(attacker.IsAttackReady, Is.True);
        }

        /// <summary>
        /// ZAMAN GÜDÜMLÜ KAPANIŞ BOZULMADI: kimse bitirmezse düşme penceresi
        /// kendi kendine dolar ve beden yine kalıcı ölüye geçer.
        ///
        /// Bu testin var olma sebebi ikinci yolun eklenmiş olması. Bitirme
        /// eklenirken zamanla gelen geçiş sessizce kaybolsaydı, hiç vurulmayan
        /// bir beden sahnede sonsuza dek düşmüş kalırdı ve hiçbir test kırmızıya
        /// dönmezdi.
        /// </summary>
        [Test]
        public void NobodyFinishesIt_TheDownedWindowStillClosesOnItsOwn()
        {
            var lifecycle = new UnitLifecycle(downedWindowSeconds: 10f, corpseWindowSeconds: 5f);
            lifecycle.OnHealthDepleted();
            Assert.That(lifecycle.State, Is.EqualTo(UnitState.Downed), "kurulum bozuk");

            lifecycle.Tick(9.9f);
            Assert.That(lifecycle.State, Is.EqualTo(UnitState.Downed),
                "nobody dies before the window is actually full");

            lifecycle.Tick(0.2f);
            Assert.That(lifecycle.State, Is.EqualTo(UnitState.Dead));
            Assert.That(lifecycle.IsReadyForCleanup, Is.False,
                "the corpse window has only just started");

            lifecycle.Tick(5.1f);
            Assert.That(lifecycle.IsReadyForCleanup, Is.True);
        }

        /// <summary>
        /// İki yol TEK ceset penceresini paylaşır: bitirilen beden ile süresi
        /// dolan beden sahneden aynı sürede kalkar.
        ///
        /// Kırmızıya dönerse ceset süresi ikinci bir yere kopyalanmıştır ve fark
        /// ancak oyunda, iki cesedin farklı zamanlarda yok olmasıyla görülür.
        /// </summary>
        [Test]
        public void FinishedBody_AndTimedOutBody_ShareTheSameCorpseWindow()
        {
            Combatant finished = NewDownedCombatant();
            finished.TakeDamage(DownedPoolOfATenHealthBody);
            Assert.That(finished.State, Is.EqualTo(UnitState.Dead), "kurulum bozuk");

            Combatant timedOut = NewDownedCombatant();
            timedOut.Tick(10.1f);
            Assert.That(timedOut.State, Is.EqualTo(UnitState.Dead), "kurulum bozuk");

            Assert.That(finished.RemainingSeconds, Is.EqualTo(timedOut.RemainingSeconds),
                "the corpse window does not know which road the body arrived on");
            Assert.That(finished.RemainingSeconds, Is.EqualTo(5f));

            finished.Tick(4.9f);
            timedOut.Tick(4.9f);
            Assert.That(finished.IsReadyForCleanup, Is.False);
            Assert.That(timedOut.IsReadyForCleanup, Is.False);

            finished.Tick(0.2f);
            timedOut.Tick(0.2f);
            Assert.That(finished.IsReadyForCleanup, Is.True);
            Assert.That(timedOut.IsReadyForCleanup, Is.True);
        }

        /// <summary>
        /// Kendiliğinden ateş eden bir KULE de bitirir: cevabın adı saldıranın
        /// tipine göre değişmez.
        /// </summary>
        [Test]
        public void TowerFinishingADownedEnemy_AlsoReportsHitAndFinished()
        {
            var tower = new Structure(
                new Health(100),
                new StructureLifecycle(),
                Team.Player,
                new AttackProfile(damage: DownedPoolOfATenHealthBody, range: 3));
            Combatant target = NewDownedCombatant();

            Assert.That(
                AttackAction.Execute(tower, target, distance: 3),
                Is.EqualTo(AttackOutcome.HitAndFinished));
            Assert.That(target.State, Is.EqualTo(UnitState.Dead));
        }

        /// <summary>
        /// Bitirmenin OYUNDAKİ anlamı: pencere iki tarafa da açıktı, düşman önce
        /// davrandı ve dost artık kaldıramaz.
        /// </summary>
        [Test]
        public void AFinishedBody_CanNoLongerBeRevived()
        {
            Combatant target = NewDownedCombatant();
            Assert.That(TargetingRules.CanBeRevived(target.State), Is.True,
                "a downed body can be raised; what this test measures is that door closing");

            Combatant attacker = NewCombatant(damage: DownedPoolOfATenHealthBody, range: 1);
            Assert.That(
                AttackAction.Execute(attacker, target, distance: 1),
                Is.EqualTo(AttackOutcome.HitAndFinished));

            Assert.That(TargetingRules.CanBeRevived(target.State), Is.False);
            Assert.That(target.TryRevive(), Is.False,
                "a finished body cannot be raised; both owners give the same answer");
        }
    }
}
