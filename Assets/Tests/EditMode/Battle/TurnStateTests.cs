using System;
using NUnit.Framework;
using GridStrategy.Battle;
using GridStrategy.Combat;

namespace GridStrategy.Tests.EditMode.Battle
{
    /// <summary>
    /// Bu dosya iki ayrı şeyi sınıyor ve ikisini bilerek karıştırmıyor: sıranın
    /// DÖNMESİ (kim oynuyor) ve turun SAYILMASI (kaçıncı turdayız). İkisi aynı
    /// çağrıdan doğar ama aynı hızda ilerlemez — turun sayılması, dizilimdeki
    /// herkes bir kez oynayana kadar bekler.
    ///
    /// Testlerin çoğu ÜÇ girişli bir dizilim kullanıyor. İki girişli dizilimde
    /// "sarmalınca artar" ile "her ikinci devirde artar" aynı sonucu üretir ve
    /// hangisinin yazıldığı ölçülemez; üçüncü giriş o iki iddiayı ayırır.
    /// </summary>
    public sealed class TurnStateTests
    {
        private static TurnState TwoSided()
        {
            return new TurnState(new[] { Team.Player, Team.Enemy });
        }

        // Üçüncü giriş uydurma bir takım DEĞİL — Team'in bugün yalnızca iki
        // oynanabilir değeri var. Turda iki kez oynayan hızlı bir düşman:
        // dizilimin bir liste olmasının bugünkü gerçek karşılığı budur.
        private static TurnState ThreeEntry()
        {
            return new TurnState(new[] { Team.Player, Team.Enemy, Team.Enemy });
        }

        [Test]
        public void NewBattle_StartsOnTheFirstEntryOfTheOrder()
        {
            var turn = TwoSided();

            Assert.That(turn.Current, Is.EqualTo(Team.Player));
        }

        [Test]
        public void NewBattle_StartsAtTurnOne()
        {
            var turn = TwoSided();

            Assert.That(turn.TurnNumber, Is.EqualTo(1), "UI shows turn 1 without adding anything.");
        }

        [Test]
        public void DefaultConstructor_PlaysPlayerThenEnemy()
        {
            var turn = new TurnState();

            Assert.That(turn.TurnOrder, Is.EqualTo(new[] { Team.Player, Team.Enemy }));
        }

        [Test]
        public void EndTurn_HandsOverToTheNextEntry()
        {
            var turn = TwoSided();

            turn.EndTurn();

            Assert.That(turn.Current, Is.EqualTo(Team.Enemy));
        }

        /// <summary>
        /// DÖNGÜSELLİK KARARINI koruyan test — bir davranışı değil bir KARARI
        /// koruyor. Dizilimin sonu bir DURAK değil, başa dönüş noktasıdır;
        /// savaşın "sıra bitti" diye bir hâli yoktur, çünkü savaşın bitmesine
        /// karar veren bu tip değildir.
        ///
        /// Kırmızıya dönerse biri son girişten sonrasını kapatmış demektir. O
        /// değişiklik hiçbir şeyi patlatmaz: ikinci turda Current son tarafta
        /// takılı kalır, oyuncu sırasını bir daha hiç alamaz ve hata "düşman
        /// turu bitmiyor" diye bildirilir.
        /// </summary>
        [Test]
        public void EndTurn_AfterTheLastEntry_WrapsToTheFirst()
        {
            var turn = TwoSided();

            turn.EndTurn();     // Player -> Enemy
            turn.EndTurn();     // Enemy  -> Player

            Assert.That(turn.Current, Is.EqualTo(Team.Player),
                "the order is a cycle, not a queue that runs out");
        }

        /// <summary>
        /// TUR SAYACI KARARINI koruyan test. Tur numarası her EL DEĞİŞTİRMEDE
        /// değil, dizilimdeki herkes bir kez oynadığında artar. İki seçenek de
        /// yaygın; fark oyunun hedeflerinde ortaya çıkar.
        ///
        /// Kırmızıya dönerse biri sayacı her devirde artırıyor demektir ve
        /// hiçbir şey patlamaz: "3 tur dayan" görevi oyuncuya iki kez oynama
        /// hakkı verir, "2 tur süren zehir" oyuncu daha tekrar oynamadan biter.
        /// </summary>
        [Test]
        public void TurnNumber_DoesNotAdvanceInsideTheRound()
        {
            var turn = TwoSided();

            bool completed = turn.EndTurn();

            Assert.That(completed, Is.False, "handing over to the enemy is not a new turn");
            Assert.That(turn.TurnNumber, Is.EqualTo(1));
        }

        // ── ÖLÇÜ: üçüncü girişin AYIRDIĞI iki uygulama ───────────────────
        // Gerçek uygulama (TurnState.EndTurn): index bir ilerler, dizilim
        // uzunluğuna göre sarmalanır, index sıfıra döndüyse TurnNumber artar.
        // Yarışan uygulama: "her ikinci devirde TurnNumber artar". EndTurn'ün
        // dönüş değeri iki dizilimde:
        //
        //   dizilim / uygulama      1. çağrı   2. çağrı   3. çağrı
        //   ────────────────────────────────────────────────────────
        //   [P, E]   sarmal          false      true       false
        //   [P, E]   her-ikinci      false      true       false   ← AYNI
        //   ────────────────────────────────────────────────────────
        //   [P,E,E]  sarmal          false      false      true
        //   [P,E,E]  her-ikinci      false      true       false   ← AYRILDI
        //
        // ██ Aşağıdaki test tam olarak alt bloğun üst satırını iddia ediyor ██
        //
        // ── BU TESTİN AYIRMADIĞI ŞEY, dürüstçe ──────────────────────────
        // Sarmal koşulunu `index != 0` yerine `Current != Team.Player` diye
        // yazan bir uygulama bu DOSYADAKİ hiçbir testi kırmızıya döndürmez:
        // buradaki her dizilim Player ile başlıyor, yani iki koşul aynı
        // satırları seçiyor. Ayıracak veri, Player'ın ikinci kez geçtiği bir
        // dizilim olurdu ([P, E, P]) ve öyle bir vaka YOK. Üçüncü giriş
        // "uzunluğa bağlı sayma" iddiasını ayırır, "sıfırıncı indeks" ile
        // "ilk takım" iddialarını ayırmaz.
        //
        // REDDEDILEN - TurnStateTests.cs:159 yerine (üçüncü giriş hiç
        //              sınanmaz, dosya yalnızca iki girişli dizilimle çalışır):
        //     var turn = TwoSided();
        //     turn.EndTurn();
        //     turn.EndTurn();
        // KIRILAN  : iki girişli dizilimde "indeks başa sarınca artar" ile "her
        //            ikinci devirde artar" AYNI sonucu verir.
        //            EndTurn içine `if (Current == Team.Player)` yazan uygulama
        //            -> yeşil kalır; dizilimin LİSTE olması kararı korumasız
        //            derleyici: hiçbir şey der  .  test: yeşil, ama üçüncü giriş
        //            eklendiği gün tur sayacı sessizce yanlış çalışır
        // KAZANIRDI: dizilim tasarım gereği sonsuza dek iki girişli kalacaksa —
        //            o gün üç girişli vaka oyunun ulaşamayacağı bir
        //            yapılandırmayı sınar ve testi gerçek kullanımdan uzaklaştırır.
        // TEK CUMLE: Bir kuralı ancak onu YANLIŞ yazmanın kırdığı veriyle
        //            sınayabilirsin; iki giriş her iki yazımı da doğrular.
        /// <summary>
        /// TUR SAYACI KARARININ ikinci yarısını koruyan test: sayaç "iki
        /// devirde bir" değil, "dizilimin uzunluğu kadar devirde bir" artar.
        ///
        /// Kırmızıya dönerse tur kuralı takım SAYISINA sabitlenmiş demektir ve
        /// bu, dizilimi bir liste yapmanın tek gerekçesini boşa çıkarır.
        /// </summary>
        [Test]
        public void TurnNumber_AdvancesOnlyAfterEveryEntryHasPlayed()
        {
            var turn = ThreeEntry();

            Assert.That(turn.EndTurn(), Is.False, "Player -> Enemy");
            Assert.That(turn.TurnNumber, Is.EqualTo(1));

            Assert.That(turn.EndTurn(), Is.False, "Enemy -> Enemy (the fast side plays again)");
            Assert.That(turn.TurnNumber, Is.EqualTo(1), "two handovers are not a full round here");

            Assert.That(turn.EndTurn(), Is.True, "Enemy -> Player closes the round");
            Assert.That(turn.TurnNumber, Is.EqualTo(2));
        }

        /// <summary>
        /// TEKRARLANAN TARAF KARARINI koruyan test. Aynı takımın dizilimde iki
        /// kez geçmesi yasak değil: turda iki kez oynayan hızlı bir düşman
        /// gerçek bir denge tasarımıdır.
        ///
        /// Kırmızıya dönerse biri yinelenen girişi reddetmiş demektir ve bedeli
        /// burada görünmez: o tasarımı isteyen kişi ikinci bir düşman takımı
        /// uydurmak zorunda kalır, o takım TargetingRules'a göre birinciden
        /// FARKLI bir taraftır ve iki düşman birbirini geçerli hedef görür.
        /// </summary>
        [Test]
        public void RepeatedTeam_PlaysTwiceInOneRound()
        {
            var turn = ThreeEntry();

            turn.EndTurn();
            Assert.That(turn.Current, Is.EqualTo(Team.Enemy));

            turn.EndTurn();
            Assert.That(turn.Current, Is.EqualTo(Team.Enemy), "the same side is up again");
        }

        [Test]
        public void SingleEntryOrder_CompletesARoundOnEveryEndTurn()
        {
            var turn = new TurnState(new[] { Team.Player });

            bool completed = turn.EndTurn();

            Assert.That(completed, Is.True, "with one side every hand-over is a full round");
            Assert.That(turn.Current, Is.EqualTo(Team.Player));
            Assert.That(turn.TurnNumber, Is.EqualTo(2));
        }

        [Test]
        public void ManyRounds_KeepCountingWithoutDrift()
        {
            var turn = TwoSided();

            for (int i = 0; i < 20; i++)
            {
                turn.EndTurn();
            }

            Assert.That(turn.TurnNumber, Is.EqualTo(11), "20 hand-overs close 10 rounds on top of turn 1");
            Assert.That(turn.Current, Is.EqualTo(Team.Player));
        }

        /// <summary>
        /// TARAFSIZ SIRA KARARINI koruyan test. Team.None sıraya GİRMEZ, ve
        /// gerekçe Team.cs'in None'ı niçin taşıdığıyla aynı: None yıkılabilir
        /// duvardır, nötr kaynak düğümüdür, tuzaktır — oynayan bir taraf değil.
        ///
        /// Kırmızıya dönerse tarafsız taraf sıraya girmiş demektir. Hiçbir şey
        /// patlamaz: her turda, hiçbir birimin hiçbir şey yapamadığı bir devir
        /// açılır ve oyun düzenli aralıklarla donmuş görünür. Daha kötüsü
        /// default(Team) tarafsız olduğu için, elemanı atanmayı unutulmuş bir
        /// dizi de sessizce geçerli sayılır.
        /// </summary>
        [Test]
        public void Constructor_OrderContainingTheNeutralTeam_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => new TurnState(new[] { Team.Player, Team.None, Team.Enemy }));

            // Atanmamış bir dizi de aynı kapıya çıkar: default(Team) == None.
            Assert.Throws<ArgumentException>(() => new TurnState(new Team[2]));
        }

        [Test]
        public void Constructor_NullOrder_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new TurnState(null));
        }

        [Test]
        public void Constructor_EmptyOrder_Throws()
        {
            Assert.Throws<ArgumentException>(() => new TurnState(new Team[0]));
        }

        [Test]
        public void Constructor_CopiesTheOrder_LaterMutationDoesNotLeakIn()
        {
            var caller = new[] { Team.Player, Team.Enemy };
            var turn = new TurnState(caller);

            caller[1] = Team.Player;

            turn.EndTurn();
            Assert.That(turn.Current, Is.EqualTo(Team.Enemy),
                "the order is fixed when the battle is built, not read back from the caller");
        }

        [Test]
        public void TurnOrder_IsExposedAsAReadOnlyView()
        {
            var turn = ThreeEntry();

            Assert.That(turn.TurnOrder, Is.EqualTo(new[] { Team.Player, Team.Enemy, Team.Enemy }));
            Assert.That(turn.TurnOrder, Is.Not.InstanceOf<Team[]>(),
                "handing out the backing array would let a caller rewrite the order mid-battle");
        }
    }
}
