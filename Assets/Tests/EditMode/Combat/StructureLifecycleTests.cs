using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using GridStrategy.Combat;

namespace GridStrategy.Tests.EditMode.Combat
{
    /// <summary>
    /// Bu dosya bir yaşam döngüsünü değil, bir YOKLUĞU da sınıyor: yapıda düşme
    /// durumu ve diriltme yolu olmadığı kararını. Davranış testleri kolayca
    /// yazılır; kaldırılan bir yolun geri gelmediğini gösteren testler ise
    /// yazılmazsa hiç kimse fark etmeden geri gelir.
    ///
    /// Zaman burada da Tick ile dışarıdan veriliyor; dosyanın tamamı
    /// milisaniyeler içinde bitiyor.
    /// </summary>
    public sealed class StructureLifecycleTests
    {
        private static StructureLifecycle NewLifecycle()
        {
            return new StructureLifecycle(rubbleWindowSeconds: 8f);
        }

        [Test]
        public void NewStructure_StartsStanding()
        {
            var lifecycle = NewLifecycle();

            Assert.That(lifecycle.State, Is.EqualTo(StructureState.Standing));
            Assert.That(lifecycle.IsReadyForCleanup, Is.False);
            Assert.That(lifecycle.RemainingSeconds, Is.Zero, "A standing structure has no countdown.");
        }

        [Test]
        public void Standing_TickDoesNothing()
        {
            var lifecycle = NewLifecycle();

            lifecycle.Tick(1000f);

            Assert.That(lifecycle.State, Is.EqualTo(StructureState.Standing),
                "Time alone does not demolish a building.");
        }

        [Test]
        public void HealthDepleted_MovesToDestroyedAndReportsTheTransition()
        {
            var lifecycle = NewLifecycle();

            bool destroyedNow = lifecycle.OnHealthDepleted();

            Assert.That(destroyedNow, Is.True, "The call that destroys must say so.");
            Assert.That(lifecycle.State, Is.EqualTo(StructureState.Destroyed));
            Assert.That(lifecycle.RemainingSeconds, Is.EqualTo(8f), "The rubble timer must start.");
        }

        /// <summary>
        /// KARARI koruyan test: yıkık bir yapıya gelen ikinci hasar ne yeni bir
        /// yıkım sayılır ne de enkaz sayacını sıfırlar.
        ///
        /// İkisi de sessizce bozulabilir. Dönüş değeri true olsaydı, çöken bir
        /// binaya düşen her alan hasarı yeni bir yıkım gibi raporlanır ve efekt
        /// her isabette tekrar oynardı. Sayaç sıfırlansaydı, yıkığa değen her
        /// vuruş enkazı ekranda uzatır ve kimse bunu bir hata olarak bildirmezdi.
        ///
        /// Bu test kırmızıya dönerse OnHealthDepleted'daki erken çıkış
        /// değişmiş demektir — ve o değişiklik başka hiçbir testi kırmaz.
        /// </summary>
        [Test]
        public void Destroyed_SecondDepletion_ReportsNothingAndDoesNotRestartTheRubbleTimer()
        {
            var lifecycle = NewLifecycle();
            lifecycle.OnHealthDepleted();
            lifecycle.Tick(3f);                     // 5 saniye kaldı

            bool destroyedNow = lifecycle.OnHealthDepleted();

            Assert.That(destroyedNow, Is.False, "A ruin cannot be destroyed a second time.");
            Assert.That(lifecycle.RemainingSeconds, Is.EqualTo(5f), "The rubble timer must not restart.");
        }

        // Pencerenin İKİ yanı da sınanıyor: sınırın nerede olduğu yorumla değil
        // testle sabitlenir.
        [Test]
        public void Destroyed_JustBeforeWindowCloses_IsNotReadyForCleanup()
        {
            var lifecycle = NewLifecycle();
            lifecycle.OnHealthDepleted();

            lifecycle.Tick(7.9f);

            Assert.That(lifecycle.IsReadyForCleanup, Is.False, "The rubble is still on the map.");
        }

        [Test]
        public void Destroyed_WhenRubbleWindowCloses_RequestsCleanup()
        {
            var lifecycle = NewLifecycle();
            lifecycle.OnHealthDepleted();

            lifecycle.Tick(8.1f);

            Assert.That(lifecycle.IsReadyForCleanup, Is.True);
            Assert.That(lifecycle.RemainingSeconds, Is.Zero, "The UI must not show a negative countdown.");
            Assert.That(lifecycle.State, Is.EqualTo(StructureState.Destroyed),
                "Cleanup is a request, not a fourth state.");
        }

        // Zaman parça parça da gelse aynı sonuç: kare süreleri değişkendir, kural
        // kare sayısına değil TOPLAM süreye bağlı olmalı.
        [Test]
        public void Destroyed_ManySmallTicks_SumToTheSameOutcome()
        {
            var lifecycle = NewLifecycle();
            lifecycle.OnHealthDepleted();

            for (int i = 0; i < 100; i++)
            {
                lifecycle.Tick(0.1f);
            }

            Assert.That(lifecycle.IsReadyForCleanup, Is.True);
        }

        [Test]
        public void Destroyed_NoAmountOfTime_ReturnsItToStanding()
        {
            var lifecycle = NewLifecycle();
            lifecycle.OnHealthDepleted();

            lifecycle.Tick(1000f);

            Assert.That(lifecycle.State, Is.EqualTo(StructureState.Destroyed),
                "Demolition is permanent; rebuilding is a new object.");
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
                () => new StructureLifecycle(rubbleWindowSeconds: seconds));
        }

        /// <summary>
        /// SIFIRINCI DEĞER kararını koruyan test. <c>default(StructureState)</c>
        /// "yıkık"tır, "ayakta" değil — çünkü atanmayı unutulmuş bir alan, olabilecek
        /// en zararsız şeye benzemeli ve tahtada zararsız olan şey hiç olmayan şeydir.
        ///
        /// Bu test kırmızıya dönerse enum'daki iki değerin sırası değişmiş demektir.
        /// O değişiklik başka hiçbir testi kırmaz ve ürettiği hata derlemede değil,
        /// oyunun ortasında görünür: sıfırlanmış bir dizi hücresi ya da eksik okunan
        /// bir kayıt, tahtada sapasağlam bir bina gibi davranır.
        /// </summary>
        [Test]
        public void Default_StructureStateValue_IsDestroyedNotStanding()
        {
            Assert.That(default(StructureState), Is.EqualTo(StructureState.Destroyed),
                "An unassigned state must not read as an intact building.");
        }

        /// <summary>
        /// KARARI koruyan test — ve bu dosyadaki tek yansıma (reflection) kullanımı.
        /// Sınanan şey bir davranış değil bir YOKLUK: yapının yaşam döngüsünde
        /// diriltmeye ya da durumu geri almaya yarayan bir yol YOKTUR.
        ///
        /// Yansıma bilerek: "şu metot çağrılınca şu olur" yazılabilir ama "şu metot
        /// HİÇ YOK" yalnızca böyle yazılabilir. Biri UnitLifecycle'dan TryRevive'ı
        /// kopyalayıp yapıştırdığı gün — ki bu iki dosyanın benzerliği yüzünden en
        /// olası hata budur — derleyici memnun, bütün diğer testler yeşil kalır.
        /// Burası kırmızıya döner.
        /// </summary>
        [Test]
        public void LifecycleApi_HasNoResurrectionPath()
        {
            string[] resurrectionLikeMethods = typeof(StructureLifecycle)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(method => method.Name)
                .Where(name => name.Contains("Revive") || name.Contains("Repair"))
                .ToArray();

            Assert.That(resurrectionLikeMethods, Is.Empty,
                "A destroyed structure is rebuilt, never revived or repaired by its lifecycle.");
        }
    }
}
