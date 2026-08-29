using System;
using NUnit.Framework;
using GridStrategy.Battle;
using GridStrategy.Combat;
using GridStrategy.Core;

namespace GridStrategy.Tests.EditMode.Battle
{
    // Takma adın gerekçesi BattleTests.cs'te yazılı: çıplak "Battle" bu ad
    // alanının içinde TİP değil AD ALANI olarak çözülür ve CS0118 verir.
    using Battle = global::GridStrategy.Battle.Battle;

    /// <summary>
    /// "SAVAŞ BİTTİ Mİ, BİTTİYSE NASIL BİTTİ" sorusunun ekrana taşınabilir
    /// cevabını koruyan dosya.
    ///
    /// <see cref="VictoryRulesTests"/> BOZULMADAN duruyor ve bu bilinçli: orada
    /// <c>Winner</c>'ın dört hâli sınanıyor ve o dosyanın ikinci testi
    /// "mutual annihilation is not a victory either" cümlesiyle beraberliğin
    /// bugün AYIRT EDİLMEDİĞİNİ yazılı bir karar hâline getirmiş. O karar
    /// değiştirilmedi — <c>Winner</c> hâlâ iki hâl için de <see cref="Team.None"/>
    /// dönüyor. Buradaki testler onun ÜSTÜNE oturan ikinci okumayı sınıyor.
    ///
    /// Kadro gezilmiyor ve birim öldürülmüyor: kural iki cevabı bir sonuca
    /// çeviriyor, o kadar. O iki cevabın nereden geldiği
    /// <see cref="TeamInPlayTests"/>'in konusu ve burada TEKRARLANMIYOR.
    /// </summary>
    public sealed class BattleOutcomeTests
    {
        /// <summary>
        /// Dört hâlin dördü tek tabloda — ve bu sefer dördü de AYRI cevap
        /// veriyor. Tablo bir vaka listesi değil bir SINIR KÜMESİ: iki bool'un
        /// alabileceği bütün birleşimler burada ve beşincisi yok.
        /// </summary>
        [TestCase(true, true, ExpectedResult = BattleOutcome.Ongoing)]
        [TestCase(true, false, ExpectedResult = BattleOutcome.PlayerWon)]
        [TestCase(false, true, ExpectedResult = BattleOutcome.EnemyWon)]
        [TestCase(false, false, ExpectedResult = BattleOutcome.Draw)]
        public BattleOutcome Outcome_AllFourStates(bool playerInPlay, bool enemyInPlay)
        {
            return VictoryRules.Outcome(playerInPlay, enemyInPlay);
        }

        /// <summary>
        /// Sıfırıncı değerin BİLEREK "savaş sürüyor" olduğunu tutan test.
        ///
        /// Kırmızıya dönerse sıfır hücresine bir zafer taşınmış demektir ve o
        /// gün atanmayı unutulan her <c>BattleOutcome</c> alanı, oyuncuya
        /// kazanmadığı bir savaşı kazanmış gösterir. Derleyici bunu görmez.
        /// </summary>
        [Test]
        public void Ongoing_IsTheZerothValue_SoAnUnassignedFieldIsNeverAVictory()
        {
            Assert.That((int)BattleOutcome.Ongoing, Is.Zero,
                "Ongoing must stay the zeroth value");
            Assert.That(default(BattleOutcome), Is.EqualTo(BattleOutcome.Ongoing),
                "an unassigned field must read as a running battle");
        }

        /// <summary>
        /// AYRIM BURADA DOĞUYOR, ÖTEKİ ÜYE DEĞİŞMEDEN: <c>Winner</c> iki hâli
        /// hâlâ tek cevaba indiriyor, <c>Outcome</c> ikisini ayırıyor.
        ///
        /// İlk iki iddia bir GERİLEME KAPISI: yeni okumayı eklemek için eski
        /// üyenin davranışını değiştirmek gerekmediğini sabitliyorlar.
        /// </summary>
        [Test]
        public void Outcome_SeparatesTheDrawFromTheRunningBattle_WhileWinnerStillMergesThem()
        {
            Assert.That(VictoryRules.Winner(true, true), Is.EqualTo(Team.None),
                "the older reading is unchanged");
            Assert.That(VictoryRules.Winner(false, false), Is.EqualTo(Team.None),
                "the older reading is unchanged");

            Assert.That(VictoryRules.Outcome(true, true), Is.EqualTo(BattleOutcome.Ongoing));
            Assert.That(VictoryRules.Outcome(false, false), Is.EqualTo(BattleOutcome.Draw));
        }

        /// <summary>
        /// İki okuma KAZANAN konusunda asla ayrışmaz.
        ///
        /// Bu testin işi bir davranışı değil bir SAHİPLİĞİ korumak: kazananın
        /// kim olduğu kuralının tek bir yazarı var. Biri <c>Outcome</c>'un
        /// içine kuralın ikinci bir kopyasını yazdığı gün — bir taraf tercihi,
        /// bir karşılaştırma — iki üye sessizce ayrışabilir ve burası kırmızıya
        /// döner.
        /// </summary>
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void Outcome_NamesAWinnerExactlyWhenWinnerDoes(bool playerInPlay, bool enemyInPlay)
        {
            Team winner = VictoryRules.Winner(playerInPlay, enemyInPlay);
            BattleOutcome outcome = VictoryRules.Outcome(playerInPlay, enemyInPlay);

            Assert.That(outcome == BattleOutcome.PlayerWon, Is.EqualTo(winner == Team.Player),
                "the player wins in both readings or in neither");
            Assert.That(outcome == BattleOutcome.EnemyWon, Is.EqualTo(winner == Team.Enemy),
                "the enemy wins in both readings or in neither");
        }

        /// <summary>
        /// Nesne alan yol iki girdiyi de savaşın kendisinden topluyor.
        ///
        /// İKİ VAKA, ÇÜNKÜ TEK VAKA BİR ÖLÇÜ ALETİ DEĞİL: yalnız boş savaş
        /// sınansaydı, her zaman <c>Draw</c> dönen bir gövde de yeşil kalırdı.
        /// </summary>
        // İDDİA KADRONUN NASIL SAYILDIĞINA GİRMİYOR — ayakta bir yapının takımı
        // oyunda tuttuğu TeamInPlayTests'in konusu ve orada sınanıyor. Burada
        // sınanan tek şey, nesne alan yolun gerçekten Battle'a sorduğu.
        [Test]
        public void Outcome_ReadsBothAnswersFromTheBattleItself()
        {
            var running = new Battle(8, 8);
            running.AddUnit(new Unit("Vanguard"), NewCombatant(Team.Player), 0, 0);
            running.AddUnit(new Unit("Raider"), NewCombatant(Team.Enemy), 5, 5);

            Assert.That(VictoryRules.Outcome(running), Is.EqualTo(BattleOutcome.Ongoing),
                "both sides are on the board");

            Assert.That(VictoryRules.Outcome(new Battle(8, 8)), Is.EqualTo(BattleOutcome.Draw),
                "an empty battle has nobody left in play");
        }

        /// <summary>
        /// Savaş verilmemişse bu bir ÇAĞIRAN hatasıdır, bir berabere değil.
        ///
        /// Sessizce <see cref="BattleOutcome.Draw"/> dönmek "kimse kazanmadı"
        /// ile "kimse sormadı" hâllerini ayırt edilemez kılardı ve pano hiç
        /// oynanmamış bir savaş için açılırdı.
        /// </summary>
        [Test]
        public void Outcome_NullBattle_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => VictoryRules.Outcome(null));
        }

        private static Combatant NewCombatant(Team team)
        {
            return new Combatant(
                new Health(10),
                new UnitLifecycle(downedWindowSeconds: 10f, corpseWindowSeconds: 5f),
                new AttackProfile(damage: 10, range: 1),
                team);
        }
    }
}
