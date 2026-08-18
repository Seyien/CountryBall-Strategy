using System;
using NUnit.Framework;
using GridStrategy.Combat;

namespace GridStrategy.Tests.EditMode.Combat
{
    /// <summary>
    /// Bu dosyanın tamamı 10 ve 5 saniyelik kuralları sınıyor ve saniyeler
    /// içinde değil MİLİSANİYELER içinde bitiyor — çünkü zaman Tick ile
    /// dışarıdan veriliyor. Time.deltaTime okunsaydı bu testler PlayMode'a
    /// düşer ve gerçekten 15 saniye beklerdi.
    /// </summary>
    public sealed class UnitLifecycleTests
    {
        private static UnitLifecycle NewLifecycle()
        {
            return new UnitLifecycle(downedWindowSeconds: 10f, corpseWindowSeconds: 5f);
        }

        [Test]
        public void NewUnit_StartsAlive()
        {
            var lifecycle = NewLifecycle();

            Assert.That(lifecycle.State, Is.EqualTo(UnitState.Alive));
            Assert.That(lifecycle.IsReadyForCleanup, Is.False);
        }

        [Test]
        public void Alive_TickDoesNothing()
        {
            var lifecycle = NewLifecycle();

            lifecycle.Tick(1000f);

            Assert.That(lifecycle.State, Is.EqualTo(UnitState.Alive), "Zaman tek başına öldürmez.");
        }

        [Test]
        public void HealthDepleted_MovesToDowned()
        {
            var lifecycle = NewLifecycle();

            lifecycle.OnHealthDepleted();

            Assert.That(lifecycle.State, Is.EqualTo(UnitState.Downed));
            Assert.That(lifecycle.RemainingSeconds, Is.EqualTo(10f));
        }

        // Pencerenin İKİ yanı da test ediliyor: sınırın nerede olduğu yorumla
        // değil testle sabitlenir.
        [Test]
        public void Downed_JustBeforeWindowCloses_StillDowned()
        {
            var lifecycle = NewLifecycle();
            lifecycle.OnHealthDepleted();

            lifecycle.Tick(9.9f);

            Assert.That(lifecycle.State, Is.EqualTo(UnitState.Downed), "Kurtarma penceresi hâlâ açık.");
        }

        [Test]
        public void Downed_WhenWindowCloses_BecomesDead()
        {
            var lifecycle = NewLifecycle();
            lifecycle.OnHealthDepleted();

            lifecycle.Tick(10.1f);

            Assert.That(lifecycle.State, Is.EqualTo(UnitState.Dead));
            Assert.That(lifecycle.RemainingSeconds, Is.EqualTo(5f), "Ceset sayacı başlamalı.");
            Assert.That(lifecycle.IsReadyForCleanup, Is.False, "Ölmek, kaldırılmak demek değildir.");
        }

        // Zaman parça parça da gelse aynı sonuç: kare süreleri değişkendir,
        // kural kare sayısına değil TOPLAM süreye bağlı olmalı.
        [Test]
        public void Downed_ManySmallTicks_SumToTheSameOutcome()
        {
            var lifecycle = NewLifecycle();
            lifecycle.OnHealthDepleted();

            for (int i = 0; i < 100; i++)
            {
                lifecycle.Tick(0.1f);
            }

            Assert.That(lifecycle.State, Is.EqualTo(UnitState.Dead));
        }

        [Test]
        public void Downed_Revive_ReturnsToAlive()
        {
            var lifecycle = NewLifecycle();
            lifecycle.OnHealthDepleted();
            lifecycle.Tick(9f);

            bool revived = lifecycle.TryRevive();

            Assert.That(revived, Is.True);
            Assert.That(lifecycle.State, Is.EqualTo(UnitState.Alive));
        }

        [Test]
        public void Dead_CannotBeRevived()
        {
            var lifecycle = NewLifecycle();
            lifecycle.OnHealthDepleted();
            lifecycle.Tick(10.1f);

            bool revived = lifecycle.TryRevive();

            Assert.That(revived, Is.False, "Kalıcı ölüm kalıcıdır.");
            Assert.That(lifecycle.State, Is.EqualTo(UnitState.Dead));
        }

        [Test]
        public void Alive_CannotBeRevived()
        {
            var lifecycle = NewLifecycle();

            Assert.That(lifecycle.TryRevive(), Is.False, "Ayakta olan birim diriltilemez.");
        }

        [Test]
        public void Dead_WhenCorpseWindowCloses_RequestsCleanup()
        {
            var lifecycle = NewLifecycle();
            lifecycle.OnHealthDepleted();
            lifecycle.Tick(10.1f);

            lifecycle.Tick(5.1f);

            Assert.That(lifecycle.IsReadyForCleanup, Is.True);
            Assert.That(lifecycle.RemainingSeconds, Is.Zero, "UI negatif sayı göstermemeli.");
        }

        // Downed birime tekrar vurmak onu ANINDA öldürmemeli. "İşini bitirme"
        // ayrı bir kural (düşme canı) ve henüz yazılmadı; bu test o boşluğun
        // sessizce kapanmasını engelliyor.
        [Test]
        public void Downed_HealthDepletedAgain_DoesNotSkipTheWindow()
        {
            var lifecycle = NewLifecycle();
            lifecycle.OnHealthDepleted();
            lifecycle.Tick(3f);

            lifecycle.OnHealthDepleted();

            Assert.That(lifecycle.State, Is.EqualTo(UnitState.Downed));
            Assert.That(lifecycle.RemainingSeconds, Is.EqualTo(7f), "Geri sayım sıfırlanmamalı.");
        }

        [Test]
        public void Tick_NegativeTime_Throws()
        {
            var lifecycle = NewLifecycle();

            Assert.Throws<ArgumentOutOfRangeException>(() => lifecycle.Tick(-0.1f));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void Constructor_NonPositiveWindow_Throws(float seconds)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new UnitLifecycle(downedWindowSeconds: seconds));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new UnitLifecycle(corpseWindowSeconds: seconds));
        }
    }
}
