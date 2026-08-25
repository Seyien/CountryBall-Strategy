using NUnit.Framework;
using GridStrategy.Combat;

namespace GridStrategy.Tests.EditMode.Battle
{
    // Tip başına bir takma ad; gerekçesi BattleTests.cs'te bir kez yazılı ve
    // burada tekrar edilmiyor. Kısası: çıplak "Battle" bu ad alanının içinde bir
    // AD ALANINA çözülür ve CS0118 verir.
    using VictoryRules = global::GridStrategy.Battle.VictoryRules;

    /// <summary>
    /// Bu dosya bir kadro gezmiyor, bir savaş kurmuyor ve bir birim öldürmüyor —
    /// çünkü sınanan tip de bunların hiçbirini yapmıyor. <c>VictoryRules</c>
    /// yalnızca İKİ cevabı bir sonuca çeviriyor ve dört hâlin dördü de burada
    /// tek satırla kuruluyor. Kural bir <c>Battle</c> alsaydı "iki taraf da
    /// tükendi" hâlini sınamak için gerçekten iki tarafı tüketmek gerekirdi;
    /// imzanın değer almasının bedeli işte o farktır.
    ///
    /// Kadro tarafındaki ikizi <c>BattleTests</c> içinde yaşıyor: orada
    /// <c>HasUnitsLeft</c>'in düşmüş birimi SAYDIĞI, ölüyü saymadığı ve yapıları
    /// hiç görmediği sınanıyor. İkisi birlikte okunmalı — biri girdiyi, öteki
    /// girdinin sonuca çevrilmesini koruyor.
    /// </summary>
    public sealed class VictoryRulesTests
    {
        /// <summary>
        /// Dört hâlin dördü tek tabloda. Tablo bir vaka listesi değil bir SINIR
        /// KÜMESİ: iki bool'un alabileceği bütün kombinasyonlar burada ve
        /// beşincisi yok.
        /// </summary>
        [TestCase(true, true, ExpectedResult = Team.None)]
        [TestCase(false, false, ExpectedResult = Team.None)]
        [TestCase(true, false, ExpectedResult = Team.Player)]
        [TestCase(false, true, ExpectedResult = Team.Enemy)]
        public Team Winner_AllFourStates(bool playerHasUnitsLeft, bool enemyHasUnitsLeft)
        {
            return VictoryRules.Winner(playerHasUnitsLeft, enemyHasUnitsLeft);
        }

        /// <summary>
        /// Yukarıdaki tablonun ilk satırı iki AYRI olguyu tek cevaba indiriyor ve
        /// bu test o indirmeyi AÇIKÇA sabitliyor: "savaş sürüyor" ile "iki taraf
        /// da tükendi" bugün ayırt EDİLMİYOR. Ayırt edilmesi gereken gün burası
        /// kırmızıya döner ve karar sessizce değişemez.
        /// </summary>
        [Test]
        public void Winner_BothSidesAlikeMeansNobodyWins_WhetherFullOrEmpty()
        {
            Assert.That(VictoryRules.Winner(true, true), Is.EqualTo(Team.None),
                "the battle is still running");
            Assert.That(VictoryRules.Winner(false, false), Is.EqualTo(Team.None),
                "mutual annihilation is not a victory either");
        }

        /// <summary>
        /// Sıfırıncı değerin BİLEREK bir "kazanan yok" olduğunu tutan test.
        /// <see cref="Team.None"/> sıfırdır ve <c>default(Team)</c> ile aynı
        /// şeydir; bu değer bir gün "kazandı" anlamına gelirse atanmayı unutulan
        /// her alan sessizce bir zafer gibi okunurdu.
        /// </summary>
        [Test]
        public void Winner_NeverReturnsNeutralAsAWinner()
        {
            Assert.That((int)Team.None, Is.Zero, "Team.None must stay the zeroth value");
            Assert.That(VictoryRules.Winner(true, false), Is.Not.EqualTo(Team.None));
            Assert.That(VictoryRules.Winner(false, true), Is.Not.EqualTo(Team.None));
        }
    }
}
