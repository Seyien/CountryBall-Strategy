using System;
using GridStrategy.Combat;
using NUnit.Framework;

namespace GridStrategy.Tests.EditMode.Combat
{
    /// <summary>
    /// Bu dosyanın iddiaları SAYI hakkındadır, sahibi hakkında değil. Assert
    /// mesajları da bilerek alan sözcüğü kullanmaz ("canlı", "ölü"): bir barakaya
    /// da uygulanan bir kuralı yalnızca askerlerle anlatmak, <c>IsAlive</c> adının
    /// düştüğü tuzağın aynısıdır — sadece yorumda.
    /// </summary>
    public sealed class HealthTests
    {
        [Test]
        public void Constructor_SetsCurrentToMax()
        {
            var health = new Health(max: 10);

            Assert.That(health.Current, Is.EqualTo(10), "A new owner must start undamaged.");
            Assert.That(health.HasRemaining, Is.True, "Full health must report remaining health.");
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
            Assert.That(health.HasRemaining, Is.True, "6 health is still health remaining.");
        }

        [Test]
        public void TakeDamage_WhenAmountEqualsCurrent_ReachesExactlyZero()
        {
            var health = new Health(max: 10);

            health.TakeDamage(10);

            Assert.That(health.Current, Is.EqualTo(0), "Exact lethal damage must land on zero.");
            Assert.That(health.HasRemaining, Is.False, "Zero health has nothing remaining.");
        }

        // ---- clamp ETKİLİ olduğu satırlar -----------------------------------
        // Kural kaldırılsaydı bu iki test negatif değer görürdü: -4 ve -5.

        // REDDEDILEN - HealthTests.cs:66 yerine:
        //     // (bu test ve alttaki ikizi hiç yazılmaz; kelepçe satırları
        //     //  yalnızca DamageRulesTests'in [TestCase] tablosunda durur)
        // KIRILAN  : Health'in kuralı ÇAĞIRDIĞI ve dönen değeri current'a
        //            YAZDIĞI yer hiçbir yerde sınanmaz.
        //            gövde `DamageRules.ResolveRemaining(current, amount);` diye
        //            dönüşü ATARSA -> bütün formül testleri yeşil kalır
        //            derleyici: hiçbir şey der  .  test: bağlantı kopar, ekran
        //            yeşil kalır
        // KAZANIRDI: aynı bağlantıyı uçtan uca yürüten bir test varsa
        //            (CombatantTests hasar → Downed geçişini gerçekten koşuyorsa)
        //            ve Health tek satırlık delegasyon olarak kalacaksa.
        // TEK CUMLE: Formülü sınamak, formülün ÇAĞRILDIĞINI sınamaz.
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
        public void TakeDamage_WhenAlreadyDepleted_StaysAtZero()
        {
            var health = new Health(max: 10);
            health.TakeDamage(10);

            health.TakeDamage(5);

            Assert.That(
                health.Current,
                Is.EqualTo(0),
                "A second hit after depletion must not go below zero; without the rule this would be -5.");
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
                "An owner that starts with no health is a configuration defect.");
        }
    }
}
