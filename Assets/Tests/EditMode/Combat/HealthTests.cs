using System;
using GridStrategy.Combat;
using NUnit.Framework;

namespace GridStrategy.Tests.EditMode.Combat
{
    public sealed class HealthTests
    {
        [Test]
        public void Constructor_SetsCurrentToMax()
        {
            var health = new Health(max: 10);

            Assert.That(health.Current, Is.EqualTo(10), "A new fighter must start undamaged.");
            Assert.That(health.IsAlive, Is.True, "A new fighter must be alive.");
        }

        // ---- clamp ATIL olduğu satırlar -------------------------------------
        // Bu iki testte "sıfırın altına inme" kuralı hiçbir şey değiştirmiyor.
        // Yine de yazılıyorlar: kuralın normal vuruşu BOZMADIĞINI kanıtlıyorlar.

        [Test]
        public void TakeDamage_WhenAmountIsLessThanCurrent_SubtractsExactly()
        {
            var health = new Health(max: 10);

            health.TakeDamage(4);

            Assert.That(health.Current, Is.EqualTo(6), "Ordinary damage must subtract exactly.");
            Assert.That(health.IsAlive, Is.True, "6 health is still alive.");
        }

        [Test]
        public void TakeDamage_WhenAmountEqualsCurrent_ReachesExactlyZero()
        {
            var health = new Health(max: 10);

            health.TakeDamage(10);

            Assert.That(health.Current, Is.EqualTo(0), "Exact lethal damage must land on zero.");
            Assert.That(health.IsAlive, Is.False, "Zero health is not alive.");
        }

        // ---- clamp ETKİLİ olduğu satırlar -----------------------------------
        // Kural kaldırılsaydı bu iki test negatif değer görürdü: -4 ve -5.

        [Test]
        public void TakeDamage_WhenAmountExceedsCurrent_ClampsToZeroInsteadOfNegative()
        {
            var health = new Health(max: 10);

            health.TakeDamage(14);

            Assert.That(
                health.Current,
                Is.EqualTo(0),
                "Overkill must clamp to zero; without the rule this would be -4.");
        }

        [Test]
        public void TakeDamage_WhenAlreadyDead_StaysAtZero()
        {
            var health = new Health(max: 10);
            health.TakeDamage(10);

            health.TakeDamage(5);

            Assert.That(
                health.Current,
                Is.EqualTo(0),
                "A second hit on a dead fighter must not go below zero; without the rule this would be -5.");
        }

        // ---- sınır ve sözleşme ----------------------------------------------

        [Test]
        public void TakeDamage_WhenAmountIsZero_LeavesCurrentUnchanged()
        {
            var health = new Health(max: 10);

            health.TakeDamage(0);

            Assert.That(health.Current, Is.EqualTo(10), "A zero-damage hit is legal and changes nothing.");
        }

        [Test]
        public void TakeDamage_WhenAmountIsNegative_Throws()
        {
            var health = new Health(max: 10);

            Assert.That(
                () => health.TakeDamage(-3),
                Throws.TypeOf<ArgumentOutOfRangeException>(),
                "Negative damage would silently heal; that is a caller defect, not a gameplay outcome.");
        }

        [Test]
        public void Constructor_WhenMaxIsNotPositive_Throws()
        {
            Assert.That(
                () => new Health(max: 0),
                Throws.TypeOf<ArgumentOutOfRangeException>(),
                "A fighter that starts dead is a configuration defect.");
        }
    }
}
