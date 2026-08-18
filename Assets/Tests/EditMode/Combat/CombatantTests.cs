using System;
using NUnit.Framework;
using GridStrategy.Combat;

namespace GridStrategy.Tests.EditMode.Combat
{
    /// <summary>
    /// Bu dosya PARÇALARI test etmiyor — onların kendi testleri var. Burada
    /// test edilen tek şey parçalar ARASINDAKİ kural: can bittiğinde yaşam
    /// döngüsünün haberi oluyor mu, dirilme hangi canla dönüyor.
    /// </summary>
    public sealed class CombatantTests
    {
        private static Combatant NewCombatant(int maxHealth = 100)
        {
            return new Combatant(
                new Health(maxHealth),
                new UnitLifecycle(downedWindowSeconds: 10f, corpseWindowSeconds: 5f),
                new AttackProfile(damage: 10, range: 1));
        }

        [Test]
        public void NewCombatant_IsAliveWithFullHealth()
        {
            var combatant = NewCombatant();

            Assert.That(combatant.State, Is.EqualTo(UnitState.Alive));
            Assert.That(combatant.CurrentHealth, Is.EqualTo(100));
        }

        [Test]
        public void Damage_ThatDoesNotDeplete_LeavesStateAlone()
        {
            var combatant = NewCombatant();

            combatant.TakeDamage(99);

            Assert.That(combatant.CurrentHealth, Is.EqualTo(1));
            Assert.That(combatant.State, Is.EqualTo(UnitState.Alive), "1 can hala candir.");
        }

        // Bu dosyanin var olma sebebi olan test: Health ile UnitLifecycle
        // birbirini tanimiyor, baglantiyi Combatant kuruyor.
        [Test]
        public void Damage_ThatDepletesHealth_MovesLifecycleToDowned()
        {
            var combatant = NewCombatant();

            combatant.TakeDamage(100);

            Assert.That(combatant.CurrentHealth, Is.Zero);
            Assert.That(combatant.State, Is.EqualTo(UnitState.Downed));
            Assert.That(combatant.RemainingSeconds, Is.EqualTo(10f), "Kurtarma penceresi acilmali.");
        }

        [Test]
        public void Overkill_DoesNotPushHealthBelowZero()
        {
            var combatant = NewCombatant();

            combatant.TakeDamage(1000);

            Assert.That(combatant.CurrentHealth, Is.Zero);
            Assert.That(combatant.State, Is.EqualTo(UnitState.Downed));
        }

        // Ogrenen tasarim karari: dirilen birim TAM canla kalkmaz.
        [Test]
        public void Revive_RestoresHalfOfMaxHealth()
        {
            var combatant = NewCombatant(maxHealth: 100);
            combatant.TakeDamage(100);

            bool revived = combatant.TryRevive();

            Assert.That(revived, Is.True);
            Assert.That(combatant.State, Is.EqualTo(UnitState.Alive));
            Assert.That(combatant.CurrentHealth, Is.EqualTo(50), "Maksimumun yarisi.");
        }

        // Oran olmasi mimari karar: sabit sayi farkli maksimumlarda bambaska
        // anlamlara gelirdi.
        [TestCase(40, ExpectedResult = 20)]
        [TestCase(400, ExpectedResult = 200)]
        [TestCase(7, ExpectedResult = 3)]
        public int Revive_ScalesWithMaxHealth(int maxHealth)
        {
            var combatant = NewCombatant(maxHealth);
            combatant.TakeDamage(maxHealth);
            combatant.TryRevive();
            return combatant.CurrentHealth;
        }

        [Test]
        public void RevivedCombatant_TakesDamageAgainNormally()
        {
            var combatant = NewCombatant(maxHealth: 100);
            combatant.TakeDamage(100);
            combatant.TryRevive();                 // 50 canla ayakta

            combatant.TakeDamage(50);

            Assert.That(combatant.State, Is.EqualTo(UnitState.Downed), "Ikinci kez dustu.");
            Assert.That(combatant.RemainingSeconds, Is.EqualTo(10f), "Tam pencere, kalan degil.");
        }

        // Downed bir birim hasar ALMAYA DEVAM eder. Bu, if (!IsAlive) return;
        // satirini reddetme kararinin canli kaniti.
        [Test]
        public void Downed_StillAcceptsDamage()
        {
            var combatant = NewCombatant();
            combatant.TakeDamage(100);

            Assert.DoesNotThrow(() => combatant.TakeDamage(5));
            Assert.That(combatant.State, Is.EqualTo(UnitState.Downed), "Vurmak pencereyi kapatmaz.");
        }

        [Test]
        public void Dead_CannotBeRevived()
        {
            var combatant = NewCombatant();
            combatant.TakeDamage(100);
            combatant.Tick(10.1f);

            Assert.That(combatant.TryRevive(), Is.False);
            Assert.That(combatant.CurrentHealth, Is.Zero, "Basarisiz diriltme can vermez.");
        }

        [Test]
        public void Constructor_NullPart_Throws()
        {
            var health = new Health(10);
            var lifecycle = new UnitLifecycle();
            var profile = new AttackProfile(damage: 1, range: 1);

            Assert.Throws<ArgumentNullException>(() => new Combatant(null, lifecycle, profile));
            Assert.Throws<ArgumentNullException>(() => new Combatant(health, null, profile));
            Assert.Throws<ArgumentNullException>(() => new Combatant(health, lifecycle, null));
        }
    }
}
