using System;
using NUnit.Framework;
using GridStrategy.Battle;
using GridStrategy.Combat;

namespace GridStrategy.Tests.EditMode.Battle
{
    /// <summary>
    /// Kural iki DÜZLEME ayrılıyor, bir çarpıma değil: taraf düzlemi (sıra kimde)
    /// ve bütçe düzlemi (hakkı kaldı mı). İki düzlemin birleştiği yerde de birer
    /// test var — ama tam çarpım yazılmadı, sebebi aşağıdaki REDDEDILEN
    /// bloğunda.
    ///
    /// Dosyanın son testi TurnState ile TurnRules'ı birlikte çalıştırıyor: bu
    /// ikisinin bir savaş akışına ihtiyaç duymadan birbirine yetmesi bilinçli
    /// bir sınırdır ve o test onu sabitliyor.
    /// </summary>
    public sealed class TurnRulesTests
    {
        // REDDEDILEN - TurnRulesTests.cs:35 yerine:
        //     [TestCase(Team.Player, Team.Player, 0, ExpectedResult = true)]
        //     [TestCase(Team.Player, Team.Player, 1, ExpectedResult = false)]
        //     ... 27 satır: üç birim tarafı × üç sıra tarafı × üç bütçe değeri
        // KIRILAN  : matris ölçtüğü şeyi kaybeder — gerekçenin tam hâli
        //            TargetingRulesTests'in aynı biçimdeki bloğunda.
        //            27 satırın 18'i aynı erken dönüşü tekrar eder -> gerçek
        //            sınır (taraf ile bütçenin birleştiği tek nokta) kaybolur
        //            derleyici: hiçbir şey der  .  test: 27 satır da yeşil olur
        //            ama kural değişince hangisinin NEDEN kırmızı olduğu okunmaz
        // KAZANIRDI: bütçe TARAFA göre değişseydi — "düşman turda iki eylem
        //            yapar" gibi bir zorluk ayarı girseydi; o gün çarpım gerçek
        //            olur ve boş hücre birinin varsayım yürüteceği yer olurdu.
        // TEK CUMLE: Çarpım yazmak kuralı değil, kuralın olmadığı kadar çok
        //            satırı belgeler.
        [TestCase(Team.Player, Team.Player, ExpectedResult = true)]
        [TestCase(Team.Enemy, Team.Enemy, ExpectedResult = true)]
        [TestCase(Team.Player, Team.Enemy, ExpectedResult = false)]
        [TestCase(Team.Enemy, Team.Player, ExpectedResult = false)]
        [TestCase(Team.None, Team.Player, ExpectedResult = false)]   // tarafsız eyleyemez
        [TestCase(Team.None, Team.Enemy, ExpectedResult = false)]
        [TestCase(Team.None, Team.None, ExpectedResult = false)]     // aynı taraf ama tarafsız
        [TestCase(Team.Player, Team.None, ExpectedResult = false)]   // sırası olmayan bir taraf
        [TestCase(Team.Enemy, Team.None, ExpectedResult = false)]
        public bool CanAct_TeamPlane(Team unitTeam, Team currentTurn)
        {
            return TurnRules.CanAct(unitTeam, currentTurn);
        }

        /// <summary>
        /// TARAFSIZ BİRİM KARARINI koruyan test. Team.None hiçbir sırada
        /// eyleyemez — TargetingRules'ın "tarafsız saldıramaz" kararının aynı
        /// yerden devamı.
        ///
        /// Kırmızıya dönerse tek satırlık bir eşitlik kontrolü kalmış demektir:
        /// None == None doğrudur ve tarafsız şeyler kendi "sıraları" geldiğinde
        /// eylemeye başlar. Bunun sessiz hâli daha kötüsü — default(Team)
        /// tarafsız olduğu için takımı atanmayı unutulmuş her birim buraya
        /// düşer.
        /// </summary>
        [Test]
        public void CanAct_NeutralUnit_NeverActs()
        {
            Assert.That(TurnRules.CanAct(Team.None, Team.None), Is.False,
                "a neutral unit does not get a turn just because nobody else has one");
            Assert.That(TurnRules.CanAct(Team.None, Team.None, actionsUsedThisTurn: 0), Is.False,
                "an untouched action budget does not buy a turn either");
        }

        [TestCase(0, ExpectedResult = true)]
        [TestCase(1, ExpectedResult = false)]    // tek hak, ve harcandı
        [TestCase(5, ExpectedResult = false)]
        public bool CanAct_BudgetPlane_OnItsOwnTurn(int actionsUsedThisTurn)
        {
            return TurnRules.CanAct(Team.Player, Team.Player, actionsUsedThisTurn);
        }

        /// <summary>
        /// SIRA ÖNCELİĞİ KARARINI koruyan test. Dolu bir eylem bütçesi sırayı
        /// SATIN ALMAZ: sıra karşı taraftaysa cevap, birim hiç oynamamış olsa
        /// bile hayırdır.
        ///
        /// Kırmızıya dönerse iki kural VEYA ile birleştirilmiş demektir ve
        /// sonucu tam olarak bu kanalın var olma sebebidir: herkes her an
        /// saldırabilir hâle döner, üstelik bu kez sıra sistemi varmış gibi
        /// görünerek.
        /// </summary>
        [Test]
        public void FullBudget_DoesNotOverrideTheTurnRule()
        {
            Assert.That(TurnRules.CanAct(Team.Enemy, Team.Player, actionsUsedThisTurn: 0), Is.False,
                "a rested enemy still waits for its own turn");
        }

        /// <summary>
        /// TEK SORU KARARINI koruyan test. Hareket ile saldırı bugün AYNI
        /// bütçeyi paylaşıyor: birim turda bir kez eyler, ve o eylemin hareket
        /// mi saldırı mı olduğunu bu kural sormaz.
        ///
        /// Kırmızıya dönerse biri bütçeyi türe göre ikiye ayırmış demektir. Bu
        /// yanlış olmayabilir — "önce yürü, sonra vur" klasik bir turdur — ama
        /// sessizce olamaz: ayrım, MaxActionsPerTurn'ü büyütmekle değil, kuralı
        /// hangi türün sorulduğunu bilecek şekilde yeniden yazmakla yapılır.
        /// </summary>
        [Test]
        public void MoveAndAttack_ShareOneBudgetToday()
        {
            Assert.That(TurnRules.MaxActionsPerTurn, Is.EqualTo(1));
            // ÖDÜNÇ ALINAN — `const`: sayıyı ÇAĞIRAN assembly'ye derleme anında
            // KOPYALAR, çağrı anında okumaz. MaxActionsPerTurn `public const int`
            // olduğu için yukarıdaki satırın bu test DLL'inin IL'inde aldığı biçim
            // şudur — iki taraf da literal:
            //     Assert.That(1, Is.EqualTo(1));
            // ÖLÇÜ: TurnRules'taki sayıyı 2 yap, YALNIZ onun assembly'sini derle ve
            // eski test DLL'ini koru; satır hâlâ yeşil kalır. Unity bağımlı
            // assembly'yi de yeniden derlediği için bugün oluşamaz; kelime `static
            // readonly` olsaydı hiç oluşamazdı. Bu satır bir kusur DEĞİL, kusurun
            // nerede doğabileceğinin kaydı.
            // DİL: Docs/deep/dil/01-degismezlik-anahtar-kelimeleri.md

            // İlk eylem harcandı. Turun geri kalanında hiçbir eylem kalmadı —
            // ne ikinci bir hareket, ne bir saldırı.
            Assert.That(TurnRules.CanAct(Team.Player, Team.Player, actionsUsedThisTurn: 1), Is.False,
                "a unit that has already acted cannot spend a second action of any kind");
        }

        [Test]
        public void CanAct_NegativeSpentActions_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => TurnRules.CanAct(Team.Player, Team.Player, actionsUsedThisTurn: -1));
        }

        // Kuralın girdisi bir DEĞER; bu test o değeri TurnState'ten alıyor ve
        // ikisinin arasında başka hiçbir tipe ihtiyaç olmadığını gösteriyor.
        [Test]
        public void CanAct_FollowsTurnStateThroughAFullRound()
        {
            var turn = new TurnState(new[] { Team.Player, Team.Enemy });

            Assert.That(TurnRules.CanAct(Team.Player, turn.Current), Is.True);
            Assert.That(TurnRules.CanAct(Team.Enemy, turn.Current), Is.False);

            turn.EndTurn();

            Assert.That(TurnRules.CanAct(Team.Player, turn.Current), Is.False,
                "the player's units go quiet the moment the turn is handed over");
            Assert.That(TurnRules.CanAct(Team.Enemy, turn.Current), Is.True);

            turn.EndTurn();

            Assert.That(TurnRules.CanAct(Team.Player, turn.Current), Is.True,
                "and they answer again on the next round");
        }
    }
}
