using NUnit.Framework;
using GridStrategy.Combat;

namespace GridStrategy.Tests.EditMode.Combat
{
    /// <summary>
    /// <see cref="StructureProduction"/>'ın SAYAÇ tarafı ile TÜR tarafını
    /// ayıran testler.
    ///
    /// Ayrımın kendisi bir operatör cümlesinden doğdu: <i>"saldırı yapan
    /// kulelerin illa ki bunu göstermesine gerek yok, sadece savaşçı üreten
    /// yapılardan bahsediyorum."</i> Ekrandaki geri sayım şeridi bu iki soruyu
    /// birden soruyor — "bu bina üretir mi" (tür, hiç değişmez) ve "kaç saniye
    /// kaldı" (sayaç, her kare değişir) — ve ikisini karıştıran bir uygulama
    /// taretin tepesine boş bir gösterge asardı.
    /// </summary>
    public sealed class StructureProductionTests
    {
        private static UnitBlueprint NewSoldier()
        {
            return new UnitBlueprint("Piyade", 30, new AttackProfile(damage: 8, range: 1));
        }

        private static StructureBlueprint NewBarracksBlueprint(float productionSeconds = 5f)
        {
            return new StructureBlueprint(
                "Kışla",
                maxHealth: 120,
                attackProfile: null,
                produces: new[] { NewSoldier() },
                defaultProducedIndex: 0,
                productionSeconds: productionSeconds);
        }

        private static StructureBlueprint NewTurretBlueprint()
        {
            return new StructureBlueprint(
                "Taret",
                maxHealth: 90,
                attackProfile: new AttackProfile(damage: 12, range: 3),
                produces: null,
                defaultProducedIndex: 0,
                productionSeconds: 0f);
        }

        private static Structure NewStructure()
        {
            return new Structure(
                new Health(120),
                new StructureLifecycle(rubbleWindowSeconds: 8f),
                Team.Player);
        }

        /// <summary>
        /// Savaşçı üreten bina "üretirim" der — geri sayım şeridinin çıkma
        /// şartı budur.
        /// </summary>
        [Test]
        public void ProducesUnits_WithANonEmptyProducesList_IsTrue()
        {
            var line = new StructureProduction(NewBarracksBlueprint(), NewStructure());

            Assert.That(line.ProducesUnits, Is.True);
        }

        /// <summary>
        /// SİLAHLI AMA ÜRETMEYEN bina "üretmem" der.
        /// </summary>
        // ██ OPERATÖRÜN KELEPÇESİ TAM OLARAK BU TEST ██
        // Taretin bir AttackProfile'ı VAR ve üretim listesi BOŞ. Cevap saldırı
        // yeteneğine bakılarak verilseydi (ya da tersine, ada bakılarak) bu
        // iddia kırmızıya dönerdi. İki eksen ayrı ve bu test o ayrımın kaydı.
        [Test]
        public void ProducesUnits_ForAnArmedStructureThatProducesNothing_IsFalse()
        {
            var line = new StructureProduction(NewTurretBlueprint(), NewStructure());

            Assert.That(line.ProducesUnits, Is.False);
        }

        /// <summary>
        /// Üretim yaptıktan sonra da "üretirim" demeye devam eder: tür sabittir,
        /// sayaç değişkendir.
        /// </summary>
        // İKİ ÜYENİN AYRI KALMASININ ÖLÇÜSÜ BU: tek üyede birleştirilmiş
        // olsalardı, üretimden hemen sonraki kışla taretle aynı cevabı verir ve
        // gösterge tam da geri sayımın başladığı anda kaybolurdu.
        [Test]
        public void ProducesUnits_StaysTrueWhileTheCooldownIsRunning()
        {
            UnitBlueprint soldier = NewSoldier();
            StructureBlueprint barracks = new StructureBlueprint(
                "Kışla",
                maxHealth: 120,
                attackProfile: null,
                produces: new[] { soldier },
                defaultProducedIndex: 0,
                productionSeconds: 5f);

            var line = new StructureProduction(barracks, NewStructure());

            Assert.That(line.Produce(soldier, out Combatant _), Is.EqualTo(ProductionOutcome.Allowed));
            Assert.That(line.IsReady, Is.False, "üretimden sonra sayaç dolu olmalı");
            Assert.That(line.RemainingSeconds, Is.EqualTo(5f).Within(0.0001f));
            Assert.That(line.ProducesUnits, Is.True, "tür değişmez; değişen yalnız sayaç");
        }

        /// <summary>
        /// Sayaç saniye saniye düşüyor ve sıfırın ALTINA inmiyor — göstergenin
        /// negatif bir sayı çizmemesinin tek sebebi bu.
        /// </summary>
        [Test]
        public void RemainingSeconds_TicksDownAndStopsAtZero()
        {
            UnitBlueprint soldier = NewSoldier();
            StructureBlueprint barracks = new StructureBlueprint(
                "Kışla",
                maxHealth: 120,
                attackProfile: null,
                produces: new[] { soldier },
                defaultProducedIndex: 0,
                productionSeconds: 3f);

            var line = new StructureProduction(barracks, NewStructure());
            line.Produce(soldier, out Combatant _);

            line.Tick(1f);
            Assert.That(line.RemainingSeconds, Is.EqualTo(2f).Within(0.0001f));

            line.Tick(1f);
            Assert.That(line.RemainingSeconds, Is.EqualTo(1f).Within(0.0001f));

            // TAŞAN TİK: kalan bir saniyeye beş saniye veriliyor ve sayaç eksiye
            // KAYMIYOR. Kayarsa bir sonraki üretimin beklemesi sessizce kısalır.
            line.Tick(5f);
            Assert.That(line.RemainingSeconds, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(line.IsReady, Is.True);
        }

        /// <summary>
        /// Yeni kurulan bina HAZIR doğar; şerit ilk karede geri sayım
        /// göstermez.
        /// </summary>
        [Test]
        public void ProducesUnits_OnAFreshLine_IsTrueAndTheLineIsAlreadyReady()
        {
            var line = new StructureProduction(NewBarracksBlueprint(), NewStructure());

            Assert.That(line.ProducesUnits, Is.True);
            Assert.That(line.IsReady, Is.True);
            Assert.That(line.RemainingSeconds, Is.EqualTo(0f).Within(0.0001f));
        }
    }
}
