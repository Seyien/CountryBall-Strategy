using System.Collections.Generic;
using NUnit.Framework;
using GridStrategy.Combat;
using GridStrategy.Core;

namespace GridStrategy.Tests.EditMode.Battle
{
    // Takma adın gerekçesi BattleTests.cs'te uzun uzun yazılı ve burada tekrar
    // edilmiyor: "Battle" hem bu ad alanının son parçası hem sınanan tipin adı,
    // çıplak yazıldığında sonuç CS0118 olur.
    using Battle = global::GridStrategy.Battle.Battle;

    /// <summary>
    /// EKRANIN HER KARE SORDUĞU SORUNUN ÇEKİRDEK KANITI. Yıkılan yapının
    /// görselini tazeleyen yol (<c>BoardAdapter.RefreshStructureVisuals</c>)
    /// bir olaya değil, bir SORGUYA dayanıyor: her karede
    /// <c>Battle.TryGetStructure</c> çağrılıyor ve <c>Structure.State</c>
    /// okunuyor.
    ///
    /// O tasarımın ayakta durması tek bir olguya bağlı: yıkılmış yapının kaydı,
    /// enkaz penceresi kapanana kadar savaşta DURMALI. Kayıt yıkım anında
    /// silinseydi sorgu ilk karede cevapsız kalır, ekran hiçbir şey göstermeden
    /// bina aniden yok olurdu — ve hiçbir test kırmızıya dönmezdi, çünkü kural
    /// katmanı açısından her şey doğru çalışıyor olurdu.
    ///
    /// Bu dosya <see cref="StructureLifecycle"/>'ın sayaçlarını sınamıyor —
    /// onların testi StructureLifecycleTests'te. Buradaki tek soru KAYDIN
    /// ÖMRÜ: pencere boyunca bulunabiliyor mu, pencere kapanınca bırakılıyor mu.
    /// </summary>
    public sealed class StructureRubbleWindowTests
    {
        private const float RubbleWindowSeconds = 8f;

        // Saldırı profili YOK — gerekçe BattleTests'teki ikizinde yazılı:
        // isteğe bağlı parametreyi doldurmak, "kural olan davranış saldırmazdır"
        // kararını testte tersine çevirirdi.
        private static Structure NewStructure(int maxHealth = 10)
        {
            return new Structure(
                new Health(maxHealth),
                new StructureLifecycle(RubbleWindowSeconds),
                Team.Player);
        }

        /// <summary>
        /// PENCERENİN İÇİ. Enkaz süresi dolmadan yapılan sorgu hâlâ cevap
        /// veriyor ve cevabı <see cref="StructureState.Destroyed"/>.
        /// </summary>
        [Test]
        public void TryGetStructure_InsideTheRubbleWindow_StillAnswersDestroyed()
        {
            var battle = new Battle(3, 3);
            var tower = new Unit("Turret");
            Structure structure = NewStructure();
            battle.AddStructure(tower, structure, 1, 1);

            structure.TakeDamage(10);
            Assert.That(structure.State, Is.EqualTo(StructureState.Destroyed), "setup");

            // 7,9 saniye pencerenin İÇİNDE: 8 saniyelik enkaz süresi dolmadı.
            battle.Tick(RubbleWindowSeconds - 0.1f);

            Assert.That(battle.TryGetStructure(tower, out Structure found), Is.True,
                "the screen queries this every frame; the record must survive the window");
            Assert.That(found.State, Is.EqualTo(StructureState.Destroyed));
        }

        /// <summary>
        /// PENCERENİN İÇİ, İKİNCİ SINIR: süpürme henüz hiçbir şey bulmuyor.
        /// Üstteki testten AYRI, çünkü iddiası ayrı bir sahibe ait —
        /// biri sorgunun cevabını, öteki temizliğin kararını koruyor. Tek testte
        /// birleştirilseydi, süpürme erken çalışmaya başladığı gün hangi yarının
        /// kırıldığı okunamazdı.
        /// </summary>
        [Test]
        public void RemoveReadyForCleanup_InsideTheRubbleWindow_TakesNothing()
        {
            var battle = new Battle(3, 3);
            var tower = new Unit("Turret");
            Structure structure = NewStructure();
            battle.AddStructure(tower, structure, 1, 1);

            structure.TakeDamage(10);
            battle.Tick(RubbleWindowSeconds - 0.1f);

            var removed = new List<Unit>();

            Assert.That(battle.RemoveReadyForCleanup(removed), Is.EqualTo(0));
            Assert.That(battle.StructureCount, Is.EqualTo(1));
        }

        /// <summary>
        /// PENCERENİN KAPANIŞI. Süre dolduğunda kayıt bırakılıyor ve sorgu artık
        /// cevap vermiyor — ekranın yıkık görseli tam bu anda sahneden kalkıyor.
        /// </summary>
        [Test]
        public void TryGetStructure_AfterTheRubbleWindowCloses_NoLongerFindsTheStructure()
        {
            var battle = new Battle(3, 3);
            var tower = new Unit("Turret");
            Structure structure = NewStructure();
            battle.AddStructure(tower, structure, 1, 1);

            structure.TakeDamage(10);
            battle.Tick(RubbleWindowSeconds + 0.1f);

            var removed = new List<Unit>();
            Assert.That(battle.RemoveReadyForCleanup(removed), Is.EqualTo(1), "setup: the sweep must take it");

            Assert.That(battle.TryGetStructure(tower, out Structure _), Is.False);
            Assert.That(battle.StructureCount, Is.EqualTo(0));
        }

        /// <summary>
        /// AYAKTA DURAN YAPI HİÇ DEĞİŞMİYOR — negatif kontrol. Üstteki üç test
        /// yeşilken bu kırmızıya dönerse hata "enkaz kalkmıyor" değil "her yapı
        /// enkaz sayılıyor" olur; iki arıza ekranda AYNI görünmez ama sorgu
        /// katmanında ayırt edilemez.
        /// </summary>
        [Test]
        public void TryGetStructure_ForAnUndamagedStructure_KeepsAnsweringStanding()
        {
            var battle = new Battle(3, 3);
            var depot = new Unit("Depot");
            battle.AddStructure(depot, NewStructure(), 0, 0);

            battle.Tick(RubbleWindowSeconds + 0.1f);

            Assert.That(battle.TryGetStructure(depot, out Structure found), Is.True);
            Assert.That(found.State, Is.EqualTo(StructureState.Standing));
        }
    }
}
