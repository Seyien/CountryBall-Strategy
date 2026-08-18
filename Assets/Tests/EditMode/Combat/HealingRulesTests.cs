using System;
using NUnit.Framework;
using GridStrategy.Combat;

namespace GridStrategy.Tests.EditMode.Combat
{
    public sealed class HealingRulesTests
    {
        [TestCase(50, 100, 30, ExpectedResult = 80)]
        [TestCase(50, 100, 0, ExpectedResult = 50)]
        [TestCase(90, 100, 30, ExpectedResult = 100)]   // ust kelepce
        [TestCase(0, 100, 50, ExpectedResult = 50)]     // olu birimden dirilme
        [TestCase(100, 100, 10, ExpectedResult = 100)]  // zaten dolu
        public int ResolveRestored_ClampsToMax(int current, int max, int amount)
        {
            return HealingRules.ResolveRestored(current, max, amount);
        }

        [Test]
        public void ResolveRestored_NegativeAmount_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => HealingRules.ResolveRestored(50, 100, -1));
        }

        [Test]
        public void ResolveRestored_NegativeCurrent_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => HealingRules.ResolveRestored(-1, 100, 10));
        }

        [TestCase(0)]
        [TestCase(-5)]
        public void ResolveRestored_NonPositiveMax_Throws(int max)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => HealingRules.ResolveRestored(0, max, 10));
        }
    }
}
