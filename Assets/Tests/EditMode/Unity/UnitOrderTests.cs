using System.Collections.Generic;
using NUnit.Framework;
using GridStrategy.Combat;
using GridStrategy.Core;
using GridStrategy.Unity;

namespace GridStrategy.Tests.EditMode.Unity
{
    /// <summary>
    /// Kalıcı emirlerin ve emir defterinin davranışı.
    ///
    /// <b>YANSIMA YOK.</b> <c>UnitOrderBook</c>, <c>AttackOrder</c> ve
    /// <c>ReviveOrder</c> sade C# sınıfları: <c>new</c> ile kuruluyor, sahte bir
    /// tahta veriliyor ve doğrudan çağrılıyorlar. Sahne kurulmuyor,
    /// <c>TearDown</c> gerekmiyor.
    ///
    /// <b>BU DOSYANIN ÖLÇTÜĞÜ ASIL ŞEY ÇOĞULLUK.</b> Operatörün bildirdiği
    /// belirti tek cümleydi: "iki taraf için paralel olarak saldırı aşamalarını
    /// gerçekleştiremiyorum." Sebep bir ayar değil sahiplikti — bekleyen vuruş
    /// TAHTA BAŞINA TEKTİ. Aşağıdaki testler o tekliğin geri gelmesini
    /// engelliyor.
    /// </summary>
    // `using System;` YASAK (CS0104: Object adı UnityEngine.Object ile
    // belirsizleşiyor); tam nitelikli yazılıyor. Bu kural projede ölçüldü.
    public sealed class UnitOrderTests
    {
        // ══ ÇOĞULLUK — BU TURUN ASIL ÖLÇÜTÜ ══════════════════════════════

        /// <summary>
        /// İKİ AYRI TAKIMDAN birer birim aynı anda emir tutabiliyor.
        /// </summary>
        // OPERATÖRÜN BİLDİRDİĞİ BELİRTİ TAM OLARAK BUYDU. Eski hâlde ikinci
        // Write birincinin dört alanını eziyordu, yani bu testin ikinci
        // iddiası KIRMIZI olurdu.
        [Test]
        public void Book_TwoUnitsFromOpposingTeams_BothKeepTheirOwnOrder()
        {
            var board = new FakeOrderBoard();
            var book = new UnitOrderBook();

            var friendly = new Unit("Vanguard");
            var enemy = new Unit("Raider");
            board.PutOnBoard(friendly, enemy);

            book.Write(friendly, new AttackOrder(board, friendly, enemy));
            book.Write(enemy, new AttackOrder(board, enemy, friendly));

            Assert.That(book.Count, Is.EqualTo(2));
            Assert.That(book.TryGet(friendly, out IUnitOrder friendlyOrder), Is.True);
            Assert.That(friendlyOrder.Target, Is.SameAs(enemy));
            Assert.That(book.TryGet(enemy, out IUnitOrder enemyOrder), Is.True);
            Assert.That(enemyOrder.Target, Is.SameAs(friendly));
        }

        /// <summary>
        /// İki takımın emri AYNI KAREDE yürüyor: ikisi de vuruyor.
        /// </summary>
        // DEFTERİN TUTMASI YETMEZ, İLERLEMESİ DE GEREKİR: emirler tutulup da
        // yalnız biri ilerletilseydi ekranda görülen şey yine tek taraflı bir
        // savaş olurdu.
        [Test]
        public void Book_Advance_RunsEveryTeamsOrderInTheSameFrame()
        {
            var board = new FakeOrderBoard();
            var book = new UnitOrderBook();

            var friendly = new Unit("Vanguard");
            var enemy = new Unit("Raider");
            board.PutOnBoard(friendly, enemy);

            book.Write(friendly, new AttackOrder(board, friendly, enemy));
            book.Write(enemy, new AttackOrder(board, enemy, friendly));

            book.Advance();

            Assert.That(board.Strikes.Count, Is.EqualTo(2));
            Assert.That(board.Strikes.Exists(s => ReferenceEquals(s.Attacker, friendly)), Is.True);
            Assert.That(board.Strikes.Exists(s => ReferenceEquals(s.Attacker, enemy)), Is.True);
            Assert.That(book.Count, Is.EqualTo(2), "kalıcı emir isabetten sonra da durur");
        }

        /// <summary>
        /// AYNI HEDEFE saldıran iki birimden yalnız MENZİLDEN KOPAN durur;
        /// ötekinin emri DEVAM eder.
        /// </summary>
        // OPERATÖRÜN İKİNCİ CÜMLESİ: "aynı hedefe saldıran birden fazla birim
        // varsa her biri KENDİ menzilinden koptuğunda kesmeli." Cevabı veren
        // taraf emir değil AttackAction — ve o, saldıranın KENDİ profilini
        // okuyor. Bu test o bağın kopmadığını ölçüyor.
        [Test]
        public void Book_WhenOneAttackerLosesRange_OnlyThatOrderIsCancelled()
        {
            var board = new FakeOrderBoard();
            var book = new UnitOrderBook();

            var shortRange = new Unit("Vanguard");
            var longRange = new Unit("Archer");
            var target = new Unit("Raider");
            board.PutOnBoard(shortRange, longRange, target);

            // Hedef kaçtı: yakın dövüşçü menzilini kaybetti, okçu kaybetmedi.
            board.NextOutcomeFor(shortRange, AttackOutcome.RejectedOutOfRange);
            board.NextOutcomeFor(longRange, AttackOutcome.Hit);

            book.Write(shortRange, new AttackOrder(board, shortRange, target));
            book.Write(longRange, new AttackOrder(board, longRange, target));

            book.Advance();

            Assert.That(book.TryGet(shortRange, out IUnitOrder _), Is.False,
                "menzilden kopan emir düşmeli");
            Assert.That(book.TryGet(longRange, out IUnitOrder _), Is.True,
                "menzilde kalan emir DEVAM etmeli");
        }

        /// <summary>
        /// Yeni emir eskisinin YERİNE geçer; ikisi birden koşmaz.
        /// </summary>
        // İKİSİ BİRDEN TUTULSAYDI oyuncunun VAZGEÇTİĞİ hedef de vurulurdu.
        [Test]
        public void Book_Write_ReplacesTheUnitsPreviousOrder()
        {
            var board = new FakeOrderBoard();
            var book = new UnitOrderBook();

            var attacker = new Unit("Vanguard");
            var first = new Unit("Raider");
            var second = new Unit("Scout");
            board.PutOnBoard(attacker, first, second);

            book.Write(attacker, new AttackOrder(board, attacker, first));
            book.Write(attacker, new AttackOrder(board, attacker, second));

            Assert.That(book.Count, Is.EqualTo(1));
            Assert.That(book.TryGet(attacker, out IUnitOrder order), Is.True);
            Assert.That(order.Target, Is.SameAs(second));

            book.Advance();

            Assert.That(board.Strikes.Count, Is.EqualTo(1));
            Assert.That(board.Strikes[0].Target, Is.SameAs(second));
        }

        // ══ KALICILIK VE BEKLEME ═════════════════════════════════════════

        /// <summary>
        /// Emir isabetten SONRA da durur: oyuncu tekrar tıklamadan vurmaya
        /// devam eder.
        /// </summary>
        // OPERATÖRÜN BİRİNCİ CÜMLESİ: "bir attacker'a target belirttiğimizde 1
        // kere saldırıyor; tekrardan yönlendirmediğimiz sürece saldırmaya devam
        // etmeli."
        [Test]
        public void AttackOrder_AfterALandedHit_KeepsStanding()
        {
            var board = new FakeOrderBoard();
            var attacker = new Unit("Vanguard");
            var target = new Unit("Raider");
            board.PutOnBoard(attacker, target);

            var order = new AttackOrder(board, attacker, target);

            Assert.That(order.Advance(), Is.EqualTo(OrderProgress.Continue));
            Assert.That(order.Advance(), Is.EqualTo(OrderProgress.Continue));
            Assert.That(order.Advance(), Is.EqualTo(OrderProgress.Continue));
            Assert.That(board.Strikes.Count, Is.EqualTo(3));
        }

        /// <summary>
        /// BEKLEME SÜRESİ EMİRDE İKİNCİ KEZ YAZILMIYOR: emir yalnızca
        /// <c>RejectedOnCooldown</c> cevabını okuyup sessizce bekliyor.
        /// </summary>
        // İDDİA İKİ YARIMDIR: emir düşmüyor (bekleyerek düzelecek bir ret) VE
        // emir kendi kronometresini tutmuyor (her karede yine soruyor). İkinci
        // yarım olmasaydı sayacın sahibi ikiye bölünürdü ve Inspector'daki
        // saniye değiştiği gün ikisi ayrışırdı.
        [Test]
        public void AttackOrder_OnCooldown_WaitsWithoutWritingASecondTimer()
        {
            var board = new FakeOrderBoard();
            var attacker = new Unit("Vanguard");
            var target = new Unit("Raider");
            board.PutOnBoard(attacker, target);
            board.AlwaysOutcome = AttackOutcome.RejectedOnCooldown;

            var order = new AttackOrder(board, attacker, target);

            Assert.That(order.Advance(), Is.EqualTo(OrderProgress.Continue));
            Assert.That(order.Advance(), Is.EqualTo(OrderProgress.Continue));

            Assert.That(board.Strikes.Count, Is.EqualTo(2),
                "emir her karede yeniden soruyor; kendi sayacını tutsaydı ikinci kare hiç sormazdı");
        }

        /// <summary>
        /// Görsel hâlâ yürüyorsa emir BEKLER ve savaşa hiç dokunmaz.
        /// </summary>
        // BEKLEME EKRANIN SAATİNE BAĞLI: tahta hareketi çoktan işledi, beklenen
        // tek şey görselin hedefin yanına VARMASI. Bu satır olmasaydı savaşçı
        // yolun ortasındayken vurur ve mermi varmadığı hücreden kalkardı.
        [Test]
        public void AttackOrder_WhileTheViewIsWalking_WaitsWithoutStriking()
        {
            var board = new FakeOrderBoard();
            var attacker = new Unit("Vanguard");
            var target = new Unit("Raider");
            board.PutOnBoard(attacker, target);
            board.StartWalking(attacker);

            var order = new AttackOrder(board, attacker, target);

            Assert.That(order.Advance(), Is.EqualTo(OrderProgress.Continue));
            Assert.That(board.Strikes, Is.Empty);
        }

        // ══ EMRİN DÜŞTÜĞÜ ÜÇ HÂL ═════════════════════════════════════════

        /// <summary>
        /// Hedef tahtadan kalktı: emir düşer ve savaşa HİÇ dokunulmaz.
        /// </summary>
        // DOKUNULMAMASI İDDİANIN YARISI: konumu olmayan bir kimliğe saldırı
        // çağrısı bir oyun sonucu değil bir İSTİSNA üretir.
        [Test]
        public void AttackOrder_WhenTheTargetLeftTheBoard_IsCancelledWithoutStriking()
        {
            var board = new FakeOrderBoard();
            var attacker = new Unit("Vanguard");
            var target = new Unit("Raider");
            board.PutOnBoard(attacker, target);
            board.TakeOffBoard(target);

            var order = new AttackOrder(board, attacker, target);

            Assert.That(order.Advance(), Is.EqualTo(OrderProgress.Cancelled));
            Assert.That(board.Strikes, Is.Empty);
        }

        /// <summary>
        /// Saldıran tahtadan kalktı: emir düşer.
        /// </summary>
        [Test]
        public void AttackOrder_WhenTheAttackerLeftTheBoard_IsCancelled()
        {
            var board = new FakeOrderBoard();
            var attacker = new Unit("Vanguard");
            var target = new Unit("Raider");
            board.PutOnBoard(attacker, target);
            board.TakeOffBoard(attacker);

            var order = new AttackOrder(board, attacker, target);

            Assert.That(order.Advance(), Is.EqualTo(OrderProgress.Cancelled));
        }

        /// <summary>
        /// Bekleyerek düzelmeyen retler emri düşürür.
        /// </summary>
        // ÜÇ RET, TEK KURAL: hedef geçersizse, saldıran eylem yapamıyorsa ya da
        // menzil koptuysa aynı cevap sonsuza kadar tekrarlanırdı.
        [TestCase(AttackOutcome.RejectedOutOfRange)]
        [TestCase(AttackOutcome.RejectedInvalidTarget)]
        [TestCase(AttackOutcome.RejectedActorCannotAct)]
        public void AttackOrder_OnARejectionThatWaitingCannotFix_IsCancelled(AttackOutcome rejection)
        {
            var board = new FakeOrderBoard();
            var attacker = new Unit("Vanguard");
            var target = new Unit("Raider");
            board.PutOnBoard(attacker, target);
            board.AlwaysOutcome = rejection;

            var order = new AttackOrder(board, attacker, target);

            Assert.That(order.Advance(), Is.EqualTo(OrderProgress.Cancelled));
        }

        /// <summary>
        /// Emrin SEÇİMLE hiçbir bağı yok.
        /// </summary>
        // ██ BU TESTİN VAR OLMA SEBEBİ BİR BAĞIN KOPARILMASI ██
        // Eski emir "saldıran hâlâ seçili mi" diye soruyordu, yani seçimi
        // bırakmak emri iptal ediyordu. Operatör emrin yazıldığı an seçimin
        // bırakılmasını istedi; o bağ dururken bu imkânsızdı. Bağın koptuğunun
        // kanıtı, emrin tahtadan sorabileceği bir seçim sorusunun ARTIK
        // OLMAMASIDIR — sahte tahta o üyeyi hiç taşımıyor ve emir yine koşuyor.
        [Test]
        public void AttackOrder_NeverAsksTheBoardWhoIsSelected()
        {
            Assert.That(typeof(IUnitOrderHost).GetProperty("SelectedUnit"), Is.Null,
                "emir seçimi sorabiliyorsa seçimi bırakmak emri iptal eder");
            Assert.That(typeof(IUnitOrderHost).GetMethods().Length, Is.EqualTo(5),
                "IPendingStrikeHost dokuz üyeydi; daralmazsa pattern kozmetik kalmış demektir");
        }

        // ══ KALDIRMA EMRİ — İKİNCİ CİNS, İKİNCİ BAYRAK DEĞİL ═════════════

        /// <summary>
        /// Kaldırma TEK SEFERLİKTİR: işini yapar ve defterden düşer.
        /// </summary>
        // SALDIRININ TERSİNE ve ayrım tipin kendisinde: eski hâlde bu farkı
        // `pendingStrikeIsRevive` adlı tek bir bool taşıyordu.
        [Test]
        public void ReviveOrder_OnArrival_RevivesOnceAndFinishes()
        {
            var board = new FakeOrderBoard();
            var reviver = new Unit("Medic");
            var fallen = new Unit("Vanguard");
            board.PutOnBoard(reviver, fallen);

            var order = new ReviveOrder(board, reviver, fallen);

            Assert.That(order.Advance(), Is.EqualTo(OrderProgress.Finished));
            Assert.That(board.Revives.Count, Is.EqualTo(1));
            Assert.That(board.Revives[0].Target, Is.SameAs(fallen));
        }

        /// <summary>
        /// Kaldırma da varışı bekler ve yürürken kimseyi kaldırmaz.
        /// </summary>
        [Test]
        public void ReviveOrder_WhileTheViewIsWalking_Waits()
        {
            var board = new FakeOrderBoard();
            var reviver = new Unit("Medic");
            var fallen = new Unit("Vanguard");
            board.PutOnBoard(reviver, fallen);
            board.StartWalking(reviver);

            var order = new ReviveOrder(board, reviver, fallen);

            Assert.That(order.Advance(), Is.EqualTo(OrderProgress.Continue));
            Assert.That(board.Revives, Is.Empty);
        }

        /// <summary>
        /// Bir birimin saldırı emri, kaldırma emriyle DEĞİŞTİRİLEBİLİR.
        /// </summary>
        // İKİ CİNS TEK DEFTERDE: eski hâlde bu geçiş bir bool'un sırasına
        // bağlıydı ve bayrak yanlış sırada yazıldığında kaldırma emri kendi
        // cinsini yazıldığı anda unutuyordu.
        [Test]
        public void Book_AnAttackOrderCanBeReplacedByAReviveOrder()
        {
            var board = new FakeOrderBoard();
            var book = new UnitOrderBook();

            var actor = new Unit("Vanguard");
            var enemy = new Unit("Raider");
            var fallen = new Unit("Scout");
            board.PutOnBoard(actor, enemy, fallen);

            book.Write(actor, new AttackOrder(board, actor, enemy));
            book.Write(actor, new ReviveOrder(board, actor, fallen));

            book.Advance();

            Assert.That(board.Strikes, Is.Empty);
            Assert.That(board.Revives.Count, Is.EqualTo(1));
            Assert.That(book.Count, Is.EqualTo(0), "kaldırma bitince defterden düşer");
        }

        // ══ DEFTERİN TEMİZLİĞİ ═══════════════════════════════════════════

        /// <summary>
        /// Bir kimliği HEDEFLEYEN bütün emirler aynı anda düşer.
        /// </summary>
        // ÇOĞULLUĞUN ÖTEKİ YÜZÜ: aynı hedefe saldıran üç savaşçının üçünün emri
        // de, hedef tahtadan kalktığında AYNI KAREDE düşmeli.
        [Test]
        public void Book_CancelTargeting_DropsEveryOrderAimedAtThatIdentity()
        {
            var board = new FakeOrderBoard();
            var book = new UnitOrderBook();

            var first = new Unit("Vanguard");
            var second = new Unit("Archer");
            var bystander = new Unit("Medic");
            var doomed = new Unit("Raider");
            var other = new Unit("Scout");
            board.PutOnBoard(first, second, bystander, doomed, other);

            book.Write(first, new AttackOrder(board, first, doomed));
            book.Write(second, new AttackOrder(board, second, doomed));
            book.Write(bystander, new AttackOrder(board, bystander, other));

            Assert.That(book.CancelTargeting(doomed), Is.EqualTo(2));
            Assert.That(book.Count, Is.EqualTo(1));
            Assert.That(book.TryGet(bystander, out IUnitOrder _), Is.True);
        }

        /// <summary>
        /// Emri düşen birim defterden çıkar; ötekiler kalır.
        /// </summary>
        [Test]
        public void Book_Advance_RemovesOnlyTheOrdersThatEnded()
        {
            var board = new FakeOrderBoard();
            var book = new UnitOrderBook();

            var standing = new Unit("Vanguard");
            var losing = new Unit("Archer");
            var target = new Unit("Raider");
            board.PutOnBoard(standing, losing, target);

            book.Write(standing, new AttackOrder(board, standing, target));
            book.Write(losing, new AttackOrder(board, losing, target));
            board.NextOutcomeFor(losing, AttackOutcome.RejectedInvalidTarget);

            book.Advance();

            Assert.That(book.Count, Is.EqualTo(1));
            Assert.That(book.TryGet(standing, out IUnitOrder _), Is.True);
        }

        /// <summary>
        /// Boş defterin ilerletilmesi hiçbir şey yapmaz ve patlamaz.
        /// </summary>
        // SESSİZ SINIR: tahta savaşın her karesinde bu üyeyi çağırıyor ve
        // emirlerin çoğu kare boş.
        [Test]
        public void Book_Advance_WithNoOrders_DoesNothing()
        {
            var book = new UnitOrderBook();

            Assert.DoesNotThrow(() => book.Advance());
            Assert.That(book.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// null bir kimlik deftere hiç girmez ve hiçbir soruyu bozmaz.
        /// </summary>
        // TAHTA null SEÇİMLE DE ÇAĞIRABİLİYOR: Cancel yolları seçili birimi
        // doğrudan veriyor ve seçim boş olabilir.
        [Test]
        public void Book_WithNullArguments_StaysEmptyAndAnswersFalse()
        {
            var board = new FakeOrderBoard();
            var book = new UnitOrderBook();
            var unit = new Unit("Vanguard");

            book.Write(null, new AttackOrder(board, unit, unit));
            book.Write(unit, null);

            Assert.That(book.Count, Is.EqualTo(0));
            Assert.That(book.TryGet(null, out IUnitOrder _), Is.False);
            Assert.That(book.Cancel(null), Is.False);
            Assert.That(book.CancelTargeting(null), Is.EqualTo(0));
        }

        /// <summary>
        /// Emirlerin oyuncuya söylenen hâli hedefin adını taşır.
        /// </summary>
        // OYUNDA NE İŞE YARAR: emrini verip seçimi bırakılan savaşçıya tekrar
        // tıklayan oyuncu, ona ne söylediğini görsün.
        [Test]
        public void Orders_Describe_NamesTheTarget()
        {
            var board = new FakeOrderBoard();
            var actor = new Unit("Vanguard");
            var enemy = new Unit("Raider");
            var fallen = new Unit("Scout");

            Assert.That(new AttackOrder(board, actor, enemy).Describe(), Does.Contain("Raider"));
            Assert.That(new ReviveOrder(board, actor, fallen).Describe(), Does.Contain("Scout"));
            Assert.That(new ChaseAndStrikeOrder(board, actor, enemy).Describe(), Does.Contain("Raider"));
            Assert.That(new StandAndStrikeOrder(board, actor, enemy).Describe(), Does.Contain("Raider"));
        }

        // ══ KARŞILIK EMRİ — KOVALAYAN ════════════════════════════════════
        // Bu bölümün ölçtüğü şey emrin KENDİ kararları: ne zaman yürür, ne zaman
        // vurur, ne zaman düşer. Yürünecek hücrenin DOĞRU seçildiği burada
        // sınanmıyor — o sorunun sahibi ApproachRules ve cevabı gerçek bir
        // tahtayla BoardAdapterTests içinde ölçülüyor.

        /// <summary>
        /// Menzil dışındaki saldırgana YÜRÜNÜR; o karede vurulmaz.
        /// </summary>
        // OPERATÖRÜN BİLDİRDİĞİ EKSİK TAM OLARAK BUYDU: üç hücre öteden vurulan
        // kılıçlı savaşçı seyirci kalıyordu.
        [Test]
        public void ChaseAndStrikeOrder_WhenTheAggressorIsOutOfReach_WalksInsteadOfStriking()
        {
            var board = new FakeOrderBoard();
            var defender = new Unit("Vanguard");
            var aggressor = new Unit("Archer");
            board.PutOnBoard(defender, aggressor);
            board.NextApproach = ApproachOutcome.MoveTo;

            var order = new ChaseAndStrikeOrder(board, defender, aggressor);

            Assert.That(order.Advance(), Is.EqualTo(OrderProgress.Continue));
            Assert.That(board.Approaches.Count, Is.EqualTo(1));
            Assert.That(board.Strikes, Is.Empty, "yürüyen birim aynı karede vuramaz");
        }

        /// <summary>
        /// Kendi menziline girince vurur ve emir AYAKTA kalır.
        /// </summary>
        [Test]
        public void ChaseAndStrikeOrder_OnceInsideItsOwnRange_StrikesAndKeepsStanding()
        {
            var board = new FakeOrderBoard();
            var defender = new Unit("Vanguard");
            var aggressor = new Unit("Archer");
            board.PutOnBoard(defender, aggressor);
            board.NextApproach = ApproachOutcome.AlreadyInRange;

            var order = new ChaseAndStrikeOrder(board, defender, aggressor);

            Assert.That(order.Advance(), Is.EqualTo(OrderProgress.Continue));
            Assert.That(order.Advance(), Is.EqualTo(OrderProgress.Continue));
            Assert.That(board.Strikes.Count, Is.EqualTo(2));
            Assert.That(board.Strikes[0].Target, Is.SameAs(aggressor));
        }

        /// <summary>
        /// MENZİL DIŞINDA OLMAK BU EMRİ DÜŞÜRMEZ — <c>AttackOrder</c> ile tek
        /// gözlenebilir fark.
        /// </summary>
        // ██ AYRIŞMA NOKTASININ KİLİDİ ██
        // Aynı olgu (hedef menzilde değil) iki emirde iki ZIT cevap alıyor.
        // Bu iddia kırmızıya döndüğü gün karşılık emri doğduğu karede ölür,
        // çünkü karşılık emrinin var olma sebebi menzil dışında olmaktır.
        [Test]
        public void ChaseAndStrikeOrder_WhenTheStrikeSaysOutOfRange_KeepsTheOrderAlive()
        {
            var board = new FakeOrderBoard();
            var defender = new Unit("Vanguard");
            var aggressor = new Unit("Archer");
            board.PutOnBoard(defender, aggressor);
            board.NextApproach = ApproachOutcome.AlreadyInRange;
            board.AlwaysOutcome = AttackOutcome.RejectedOutOfRange;

            var order = new ChaseAndStrikeOrder(board, defender, aggressor);

            Assert.That(order.Advance(), Is.EqualTo(OrderProgress.Continue));

            // AYNI GİRDİ AttackOrder'DA İPTAL ÜRETİYOR ve o dal operatörün
            // yazılı kararı; ikisi yan yana durduğunda fark bir hata değil cins.
            var manual = new AttackOrder(board, defender, aggressor);
            Assert.That(manual.Advance(), Is.EqualTo(OrderProgress.Cancelled));
        }

        /// <summary>
        /// Saldırgana YOL YOKSA emir düşer — tasmanın üçüncü ucu.
        /// </summary>
        [Test]
        public void ChaseAndStrikeOrder_WhenThereIsNoPathToTheAggressor_IsCancelled()
        {
            var board = new FakeOrderBoard();
            var defender = new Unit("Vanguard");
            var aggressor = new Unit("Archer");
            board.PutOnBoard(defender, aggressor);
            board.NextApproach = ApproachOutcome.RejectedUnreachable;

            var order = new ChaseAndStrikeOrder(board, defender, aggressor);

            Assert.That(order.Advance(), Is.EqualTo(OrderProgress.Cancelled));
            Assert.That(board.Strikes, Is.Empty);
        }

        /// <summary>
        /// Saldırgan tahtadan kalktı: emir düşer ve savaşa HİÇ dokunulmaz.
        /// </summary>
        // İKİ İDDİA DA GEREKLİ: konumu olmayan bir kimliğe yaklaşma ya da
        // saldırı çağrısı bir oyun sonucu değil bir istisna üretir.
        [Test]
        public void ChaseAndStrikeOrder_WhenTheAggressorLeftTheBoard_IsCancelledWithoutTouchingTheBattle()
        {
            var board = new FakeOrderBoard();
            var defender = new Unit("Vanguard");
            var aggressor = new Unit("Archer");
            board.PutOnBoard(defender, aggressor);
            board.TakeOffBoard(aggressor);

            var order = new ChaseAndStrikeOrder(board, defender, aggressor);

            Assert.That(order.Advance(), Is.EqualTo(OrderProgress.Cancelled));
            Assert.That(board.Approaches, Is.Empty);
            Assert.That(board.Strikes, Is.Empty);
        }

        /// <summary>
        /// Saldırgan öldü ya da karşılık veren düştü: beklemekle düzelmeyen ret
        /// emri düşürür.
        /// </summary>
        [TestCase(AttackOutcome.RejectedInvalidTarget)]
        [TestCase(AttackOutcome.RejectedActorCannotAct)]
        public void ChaseAndStrikeOrder_OnARejectionThatWaitingCannotFix_IsCancelled(AttackOutcome rejection)
        {
            var board = new FakeOrderBoard();
            var defender = new Unit("Vanguard");
            var aggressor = new Unit("Archer");
            board.PutOnBoard(defender, aggressor);
            board.NextApproach = ApproachOutcome.AlreadyInRange;
            board.AlwaysOutcome = rejection;

            var order = new ChaseAndStrikeOrder(board, defender, aggressor);

            Assert.That(order.Advance(), Is.EqualTo(OrderProgress.Cancelled));
        }

        /// <summary>
        /// Görsel yürürken ne yaklaşma sorulur ne de vurulur.
        /// </summary>
        // SORU YAKLAŞMADAN ÖNCE: tahta hareketi ANINDA işledi, yani yaklaşma
        // sorulsaydı "zaten menzildesin" cevabı gelir ve savaşçı yolun
        // ortasındayken vururdu.
        [Test]
        public void ChaseAndStrikeOrder_WhileTheViewIsWalking_NeitherApproachesNorStrikes()
        {
            var board = new FakeOrderBoard();
            var defender = new Unit("Vanguard");
            var aggressor = new Unit("Archer");
            board.PutOnBoard(defender, aggressor);
            board.StartWalking(defender);

            var order = new ChaseAndStrikeOrder(board, defender, aggressor);

            Assert.That(order.Advance(), Is.EqualTo(OrderProgress.Continue));
            Assert.That(board.Approaches, Is.Empty);
            Assert.That(board.Strikes, Is.Empty);
        }

        // ══ KARŞILIK EMRİ — YERİNDE DURAN ════════════════════════════════

        /// <summary>
        /// Yapı karşılık verir ama YÜRÜMEZ: tahtadan tek bir hareket istemez.
        /// </summary>
        // ██ STRATEGY AYRIMININ KENDİSİ BU İDDİA ██
        // İki uygulama aynı sözleşmeyi taşıyor ve aynı deftere yazılıyor; fark
        // yalnız burada görünüyor — bu emir hareket üyesine HİÇ dokunmuyor.
        [Test]
        public void StandAndStrikeOrder_WhenItRetaliates_NeverAsksTheBoardToMove()
        {
            var board = new FakeOrderBoard();
            var fort = new Unit("Tower");
            var aggressor = new Unit("Raider");
            board.PutOnBoard(fort, aggressor);

            var order = new StandAndStrikeOrder(board, fort, aggressor);

            Assert.That(order.Advance(), Is.EqualTo(OrderProgress.Continue));
            Assert.That(order.Advance(), Is.EqualTo(OrderProgress.Continue));

            Assert.That(board.Approaches, Is.Empty, "yapı yürüyemez");
            Assert.That(board.Strikes.Count, Is.EqualTo(2));
        }

        /// <summary>
        /// Menzil dışındaki saldırgan için yapı BEKLER; emri düşmez.
        /// </summary>
        // "MENZİLDEYSEM VUR, DEĞİLSEM BEKLE" — iptal edilseydi taret, uzaktan
        // atış yapan düşman ilerlediğinde onu unutmuş olurdu.
        [Test]
        public void StandAndStrikeOrder_WhenTheAggressorIsOutOfRange_WaitsInsteadOfDying()
        {
            var board = new FakeOrderBoard();
            var fort = new Unit("Tower");
            var aggressor = new Unit("Raider");
            board.PutOnBoard(fort, aggressor);
            board.AlwaysOutcome = AttackOutcome.RejectedOutOfRange;

            var order = new StandAndStrikeOrder(board, fort, aggressor);

            Assert.That(order.Advance(), Is.EqualTo(OrderProgress.Continue));
            Assert.That(order.Advance(), Is.EqualTo(OrderProgress.Continue));
        }

        /// <summary>
        /// Saldırgan geçerli hedef olmaktan çıktı ya da yapı yıkıldı: emir düşer.
        /// </summary>
        [TestCase(AttackOutcome.RejectedInvalidTarget)]
        [TestCase(AttackOutcome.RejectedActorCannotAct)]
        public void StandAndStrikeOrder_OnARejectionThatWaitingCannotFix_IsCancelled(AttackOutcome rejection)
        {
            var board = new FakeOrderBoard();
            var fort = new Unit("Tower");
            var aggressor = new Unit("Raider");
            board.PutOnBoard(fort, aggressor);
            board.AlwaysOutcome = rejection;

            var order = new StandAndStrikeOrder(board, fort, aggressor);

            Assert.That(order.Advance(), Is.EqualTo(OrderProgress.Cancelled));
        }

        /// <summary>
        /// Saldırgan tahtadan kalktı: yapının emri de düşer.
        /// </summary>
        [Test]
        public void StandAndStrikeOrder_WhenTheAggressorLeftTheBoard_IsCancelled()
        {
            var board = new FakeOrderBoard();
            var fort = new Unit("Tower");
            var aggressor = new Unit("Raider");
            board.PutOnBoard(fort, aggressor);
            board.TakeOffBoard(aggressor);

            var order = new StandAndStrikeOrder(board, fort, aggressor);

            Assert.That(order.Advance(), Is.EqualTo(OrderProgress.Cancelled));
            Assert.That(board.Strikes, Is.Empty);
        }

        /// <summary>
        /// Defter iki karşılık cinsini AYIRT ETMEDEN ilerletiyor.
        /// </summary>
        // ██ STRATEGY'NİN ÜÇÜNCÜ KOŞULU: TEK SÖZLEŞME ██
        // İki uygulama, tek defter, tek çağırma noktası. Bu iddia kırmızıya
        // döndüğü gün seçim artık çağıranda değil defterde yapılıyor demektir ve
        // desenin adı da değişir.
        [Test]
        public void Book_AdvancesBothRetaliationKinds_WithoutTellingThemApart()
        {
            var board = new FakeOrderBoard();
            var book = new UnitOrderBook();

            var fighter = new Unit("Vanguard");
            var fort = new Unit("Tower");
            var aggressor = new Unit("Raider");
            board.PutOnBoard(fighter, fort, aggressor);

            book.Write(fighter, new ChaseAndStrikeOrder(board, fighter, aggressor));
            book.Write(fort, new StandAndStrikeOrder(board, fort, aggressor));

            book.Advance();

            Assert.That(board.Strikes.Count, Is.EqualTo(2));
            Assert.That(book.Count, Is.EqualTo(2), "iki karşılık emri de kalıcıdır");
        }

        /// <summary>
        /// Emirlerin tahtaya bakan penceresinin sahtesi.
        /// </summary>
        // SAVAŞ KURULMUYOR ve bu bilerek: burada ölçülen şey emrin KENDİ
        // kararları — "ne zaman düşer, ne zaman bekler, ne zaman vurur". Gerçek
        // bir Battle kurulsaydı testler AttackAction'ın kurallarını ikinci kez
        // sınar ve emrin kendi mantığı o gürültünün altında kaybolurdu.
        private sealed class FakeOrderBoard : IUnitOrderHost
        {
            private readonly HashSet<Unit> onBoard = new HashSet<Unit>();
            private readonly HashSet<Unit> walking = new HashSet<Unit>();
            private readonly Dictionary<Unit, AttackOutcome> plannedOutcomes =
                new Dictionary<Unit, AttackOutcome>();

            // ADLANDIRILMIŞ DEMET, `record` DEĞİL: konumsal bir record `init`
            // erişimcisi üretiyor ve o da IsExternalInit istiyor — bu tip
            // GridStrategy.Core içinde `internal` yaşadığı için test
            // derlemesinden görünmüyor (ölçüldü: CS0518).
            public readonly List<(Unit Attacker, Unit Target)> Strikes =
                new List<(Unit, Unit)>();

            public readonly List<(Unit Reviver, Unit Target)> Revives =
                new List<(Unit, Unit)>();

            /// <summary>Yaklaşma sorusunu SORAN her çağrı buraya yazılıyor.</summary>
            // BOŞ KALMASI DA BİR İDDİA: yerinde duran karşılık emri bu üyeye hiç
            // dokunmuyor ve Strategy ayrımı tam olarak orada görünüyor.
            public readonly List<(Unit Mover, Unit Target)> Approaches =
                new List<(Unit, Unit)>();

            /// <summary>Hiçbir saldıran için ayrı cevap yazılmadıysa dönen sonuç.</summary>
            public AttackOutcome AlwaysOutcome = AttackOutcome.Hit;

            /// <summary>Yaklaşma sorusuna verilecek cevap.</summary>
            // VARSAYILAN "ZATEN MENZİLDE" ve bu bir kolaylık değil ölçü: bu sahte
            // tahta hücre tutmuyor, dolayısıyla bir mesafe hesabı taklit etmesi
            // ApproachRules'u ikinci kez yazmak olurdu. Kuralın kendi doğruluğu
            // ApproachRulesTests'te, gerçek tahtadaki karşılığı BoardAdapterTests'te.
            public ApproachOutcome NextApproach = ApproachOutcome.AlreadyInRange;

            public void PutOnBoard(params Unit[] units)
            {
                for (int i = 0; i < units.Length; i++)
                {
                    onBoard.Add(units[i]);
                }
            }

            public void TakeOffBoard(Unit unit)
            {
                onBoard.Remove(unit);
            }

            public void StartWalking(Unit unit)
            {
                walking.Add(unit);
            }

            /// <summary>
            /// Bu saldıranın alacağı cevabı yazar.
            /// </summary>
            // SALDIRAN BAŞINA, tahta başına değil — ve testin ölçtüğü şey tam
            // olarak bu: aynı hedefe vuran iki savaşçı FARKLI cevaplar alabilir,
            // çünkü menzili saldıranın kendi profili belirliyor.
            public void NextOutcomeFor(Unit attacker, AttackOutcome outcome)
            {
                plannedOutcomes[attacker] = outcome;
            }

            public bool TryGetCell(Unit unit, out int x, out int y)
            {
                x = 0;
                y = 0;
                return unit != null && onBoard.Contains(unit);
            }

            public bool IsViewWalking(Unit unit)
            {
                return unit != null && walking.Contains(unit);
            }

            public AttackOutcome Strike(Unit attacker, Unit target)
            {
                Strikes.Add((attacker, target));

                return plannedOutcomes.TryGetValue(attacker, out AttackOutcome planned)
                    ? planned
                    : AlwaysOutcome;
            }

            public void Revive(Unit reviver, Unit target)
            {
                Revives.Add((reviver, target));
            }

            public ApproachOutcome MoveIntoRange(Unit mover, Unit target)
            {
                Approaches.Add((mover, target));
                return NextApproach;
            }
        }
    }
}
