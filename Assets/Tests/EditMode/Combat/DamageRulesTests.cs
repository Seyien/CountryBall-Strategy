using System;
using GridStrategy.Combat;
using NUnit.Framework;

namespace GridStrategy.Tests.EditMode.Combat
{
    // Bu dosyanın varlığı, DamageRules'u ayrı bir dosyaya çıkarmanın somut
    // kazancıdır: formül artık tek bir Health nesnesi kurmadan sınanabiliyor.
    // Formül Health icinde private kalsaydi, her formul testi once bir savasci
    // yaratmak zorunda kalirdi.
    public sealed class DamageRulesTests
    {
        [TestCase(10, 4, 6, TestName = "OrdinaryHit")]
        [TestCase(10, 10, 0, TestName = "ExactlyLethalHit")]
        [TestCase(10, 14, 0, TestName = "OverkillClampsInsteadOfMinusFour")]
        [TestCase(0, 5, 0, TestName = "HitOnAlreadyDeadClampsInsteadOfMinusFive")]
        [TestCase(10, 0, 10, TestName = "ZeroDamageChangesNothing")]
        public void ResolveRemaining_ReturnsExpectedValue(int current, int amount, int expected)
        {
            int remaining = DamageRules.ResolveRemaining(current, amount);

            Assert.That(remaining, Is.EqualTo(expected));
        }

        [Test]
        public void ResolveRemaining_NeverReturnsNegative()
        {
            for (int amount = 0; amount <= 50; amount++)
            {
                int remaining = DamageRules.ResolveRemaining(10, amount);

                Assert.That(
                    remaining,
                    Is.GreaterThanOrEqualTo(0),
                    "The lower clamp must hold for every damage amount, not only the sampled ones.");
            }
        }

        [Test]
        public void ResolveRemaining_WhenAmountIsNegative_Throws()
        {
            Assert.That(
                () => DamageRules.ResolveRemaining(10, -3),
                Throws.TypeOf<ArgumentOutOfRangeException>(),
                "Negative damage would silently heal.");
        }

        [Test]
        public void ResolveRemaining_WhenCurrentIsNegative_Throws()
        {
            // Health uzerinden bu duruma ASLA gelinmez, cunku Health'in kendi
            // kelepcesi current'i sifirin altina indirmez. Bu test, DamageRules
            // artik public oldugu ve baska cagiranlari olabilecegi icin var:
            // sozlesmesini kendisi korur.
            Assert.That(
                () => DamageRules.ResolveRemaining(-1, 3),
                Throws.TypeOf<ArgumentOutOfRangeException>(),
                "A negative current health is a caller defect, not a gameplay outcome.");
        }
    }
}
