using GridStrategy.Core;
using NUnit.Framework;

namespace GridStrategy.Tests.EditMode.Core
{
    /// <summary>
    /// <see cref="MoveProfile"/>'ın belgesindeki "3 menzil olan iki nesne AYNI
    /// ŞEYDİR" cümlesini KANITLAYAN testler.
    ///
    /// Bu cümle dosyada baştan beri yazılıydı ama tip onu uygulamıyordu: düz bir
    /// sınıf kimliğe göre karşılaştırılır, dolayısıyla iki ayrı MoveProfile(3)
    /// eşit DEĞİLDİ ve belge sessizce yanlıştı. Tip <c>record</c>'a çevrildi;
    /// aşağıdaki testler o boşluğun kapandığını ve bir daha açılmayacağını
    /// koruyor.
    ///
    /// Testin oyun tarafındaki karşılığı şu: "bu birimin hareket tanımı
    /// süvarininkiyle aynı mı" sorusu artık alan alan karşılaştırmadan, tek bir
    /// <c>==</c> ile sorulabiliyor.
    /// </summary>
    public sealed class ProfileValueEqualityTests
    {
        [Test]
        public void TwoProfilesWithTheSameRange_AreEqual()
        {
            var a = new MoveProfile(3);
            var b = new MoveProfile(3);

            Assert.IsFalse(ReferenceEquals(a, b), "Test iki AYRI nesne kurmalı.");
            Assert.IsTrue(a == b, "Aynı menzilli iki tanım eşit sayılmalı.");
            Assert.IsTrue(a.Equals(b));
        }

        [Test]
        public void TwoProfilesWithDifferentRanges_AreNotEqual()
        {
            var cavalry = new MoveProfile(3);
            var infantry = new MoveProfile(1);

            Assert.IsFalse(cavalry == infantry);
            Assert.IsTrue(cavalry != infantry);
        }

        [Test]
        public void EqualProfiles_ShareTheSameHashCode()
        {
            // Sözlük anahtarı olarak kullanılabilmesinin şartı bu. Eşitlik
            // yazılıp hash unutulsaydı, eşit iki tanım bir Dictionary'de AYRI
            // kutulara düşer ve hata yalnız büyük veride görünürdü.
            var a = new MoveProfile(2);
            var b = new MoveProfile(2);

            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void ZeroRange_IsStillValidAndEqualToAnotherZero()
        {
            // Sıfır menzil oyunda "kök salmış" demek ve geçerli bir tanım.
            // Kural record'a çevrilirken kaybolmadı.
            var rooted = new MoveProfile(0);
            var alsoRooted = new MoveProfile(0);

            Assert.AreEqual(0, rooted.Range);
            Assert.IsTrue(rooted == alsoRooted);
        }

        [Test]
        public void NegativeRange_IsStillRejected()
        {
            // record'a çevirmek DOĞRULAMAYI atlamaz: kurucu hâlâ tek kapı.
            // `record struct` seçilseydi default(T) bu kapıyı atlardı — o yüzden
            // seçilmedi (ve C# 9'da zaten derlenmiyor).
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new MoveProfile(-1));
        }
    }
}
