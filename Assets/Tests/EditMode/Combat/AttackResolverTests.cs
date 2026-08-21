using System;
using NUnit.Framework;
using GridStrategy.Combat;

namespace GridStrategy.Tests.EditMode.Combat
{
    public sealed class AttackResolverTests
    {
        // Menzil 2 olan bir saldırı için sınırın TAM olarak nerede olduğunu
        // gösteren tablo. Sınır davranışı yorumla değil testle sabitlenir.
        [TestCase(0, ExpectedResult = true)]   // aynı hücre
        [TestCase(1, ExpectedResult = true)]
        [TestCase(2, ExpectedResult = true)]   // menzilin tam ucu — dahil
        [TestCase(3, ExpectedResult = false)]  // bir hücre ötesi
        public bool IsWithinRange_WithRangeOfTwo(int distance)
        {
            return AttackResolver.IsWithinRange(distance, new AttackProfile(damage: 5, range: 2));
        }

        [Test]
        public void IsWithinRange_MeleeProfile_ReachesAdjacentOnly()
        {
            var melee = new AttackProfile(damage: 12, range: 1);

            Assert.That(AttackResolver.IsWithinRange(1, melee), Is.True, "Bitişik hücre.");
            Assert.That(AttackResolver.IsWithinRange(2, melee), Is.False, "Bir hücre ötesi.");
        }

        [Test]
        public void IsWithinRange_NegativeDistance_Throws()
        {
            var profile = new AttackProfile(damage: 5, range: 2);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => AttackResolver.IsWithinRange(-1, profile));
        }

        [Test]
        public void IsWithinRange_NullProfile_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => AttackResolver.IsWithinRange(1, null));
        }

        // Bu test bir SAHİPLİK sınırını koruyor, bir davranışı değil: aynı
        // profil aynı mesafeyle iki kez sorulduğunda aynı cevabı vermek
        // zorundadır. Kırmızıya dönerse, kurala bir yerden durum sızmış demektir.
        [Test]
        public void IsWithinRange_IsPure_SameInputsAlwaysGiveSameAnswer()
        {
            var profile = new AttackProfile(damage: 5, range: 2);

            // REDDEDILEN - AttackResolverTests.cs:66 yerine:
            //     var first = AttackResolver.IsWithinRange(2, profile);
            //     Assert.That(AttackResolver.IsWithinRange(2, profile), Is.EqualTo(first));
            // KIRILAN  : test kendi çıktısını kendine ölçüt yapar.
            //            kural her çağrıda false dönse -> iki cevap yine eşittir
            //            aranan durum sızıntısı -> görünmez kalır, çünkü sızan
            //            durum da tutarlı olabilir: aynı yanlışı iki kez verir
            //            derleyici: hiçbir şey der  .  test: sonsuza dek YEŞİL
            // KAZANIRDI: beklenen cevap yapılandırmaya ya da rastgele bir tohuma
            //            bağlı olsaydı — literal yazılamayacağı için iki çağrıyı
            //            karşılaştırmak tek yol olurdu.
            // TEK CUMLE: Ölçütü kendi çıktısı olan bir test tutarlılığı ölçer,
            //            doğruluğu değil — saflık ise ikisini birden ister.
            Assert.That(AttackResolver.IsWithinRange(2, profile), Is.True);
            Assert.That(AttackResolver.IsWithinRange(2, profile), Is.True);
            Assert.That(AttackResolver.IsWithinRange(3, profile), Is.False);
            Assert.That(AttackResolver.IsWithinRange(3, profile), Is.False);
        }
    }
}
