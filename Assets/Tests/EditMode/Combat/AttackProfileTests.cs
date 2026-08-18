using System;
using NUnit.Framework;
using GridStrategy.Combat;

namespace GridStrategy.Tests.EditMode.Combat
{
    public sealed class AttackProfileTests
    {
        [Test]
        public void Constructor_StoresDamageAndRange()
        {
            var profile = new AttackProfile(damage: 10, range: 2);

            Assert.That(profile.Damage, Is.EqualTo(10));
            Assert.That(profile.Range, Is.EqualTo(2));
        }

        // Sıfır hasar geçerli: "vurur ama can eksiltmez" bir yetenek olabilir
        // (sersemletme, itme). Negatif hasar ise iyileştirme demek olurdu ve o
        // ayrı bir kuraldır — bu tipin sessizce üstlenmesi yanlış olurdu.
        [Test]
        public void Constructor_ZeroDamage_IsAllowed()
        {
            Assert.That(new AttackProfile(damage: 0, range: 1).Damage, Is.Zero);
        }

        [Test]
        public void Constructor_NegativeDamage_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new AttackProfile(damage: -1, range: 1));
        }

        // Menzil 0 reddediliyor çünkü hiçbir hücreye ulaşamayan bir saldırı,
        // kırmızı vermeden işe yaramayan bir birim üretirdi.
        [TestCase(0)]
        [TestCase(-3)]
        public void Constructor_RangeBelowOne_Throws(int range)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new AttackProfile(damage: 5, range: range));
        }
    }
}
