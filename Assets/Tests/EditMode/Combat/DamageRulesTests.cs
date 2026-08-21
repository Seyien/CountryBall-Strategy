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
        // REDDEDILEN - DamageRulesTests.cs:29 yerine:
        //     [Test]
        //     public void ResolveRemaining_OrdinaryHit_SubtractsExactly()
        //     { ... }
        //     // ...ve aynı gövdenin dört kopyası daha
        // KIRILAN  : sınır KÜMESİ tek yerde okunamaz hâle gelir.
        //            "10, 14 → 0" ile "0, 5 → 0" arasındaki fark (eksi dört ile
        //            eksi beş) beş ayrı gövdeye dağılır
        //            zırh geldiği gün -> yeni sınır bir satır değil yeni bir
        //            metot ister; tablo büyümez, dosya büyür
        //            derleyici: hiçbir şey der  .  test: beşi de yeşil kalır
        // KAZANIRDI: satırlar aynı gövdeyi paylaşamaz hâle gelirse — biri değer,
        //            biri fırlatma bekliyorsa ya da her vaka kendi kurulumunu
        //            istiyorsa.
        // TEK CUMLE: [TestCase] tablosu vakaları değil SINIR KÜMESİNİ belgeler;
        //            ayrı metotlar aynı kümeyi beş parçaya böler.
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
            // Health üzerinden bu duruma ASLA gelinmez, çünkü Health'in kendi
            // kelepçesi current'i sıfırın altına indirmez. Bu test, DamageRules
            // artık public olduğu ve başka çağıranları olabileceği için var:
            // sözleşmesini kendisi korur.
            Assert.That(
                () => DamageRules.ResolveRemaining(-1, 3),
                Throws.TypeOf<ArgumentOutOfRangeException>(),
                "A negative current health is a caller defect, not a gameplay outcome.");
        }
    }
}
