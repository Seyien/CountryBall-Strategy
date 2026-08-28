using System.Collections.Generic;
using NUnit.Framework;
using GridStrategy.Core;
using GridStrategy.Unity;

namespace GridStrategy.Tests.EditMode.Unity
{
    /// <summary>
    /// <b>BU DOSYA YANSIMA KULLANMIYOR VE BU TURUN ASIL KAZANCI TAM OLARAK BUDUR.</b>
    /// <see cref="BoardAdapterTests"/> özel alan ADLARINA bağlı, çünkü sınadığı
    /// tip bir <c>MonoBehaviour</c> ve <c>Awake</c> EditMode'da hiç koşmuyor.
    /// Kipler ise sade C# sınıfları: <c>new</c> ile kuruluyor, sahte bir tahta
    /// veriliyor ve davranışları doğrudan çağrılıyor.
    ///
    /// <b>SINANAN ŞEY DAVRANIŞ, KURULUM DEĞİL.</b> Buradaki testlerin hiçbiri
    /// sahne kurmuyor, <c>GameObject</c> doğurmuyor ve <c>TearDown</c>
    /// istemiyor — kipler ekranı değil, ekranı yazan tarafı ARIYOR.
    ///
    /// <b>ESKİ TESTLERİN YERİNE GEÇMİYOR.</b> <c>PendingStrikeIsAlive</c> ve
    /// <c>RepeatsPendingStrike</c> testleri <see cref="BoardAdapterTests"/>
    /// içinde AYNEN duruyor; onlar tahtanın kapısını, buradakiler kipin
    /// gövdesini sınıyor ve ikisi birden düşerse hata iki yerde birden görünür.
    /// </summary>
    public sealed class BoardModeTests
    {
        // ══ GEÇİŞLER — TEK KAPININ SINANDIĞI YER ═════════════════════════
        // Eski hâlde geçiş diye bir şey yoktu: altı alan altı ayrı satırda
        // yazılıyordu. Aşağıdaki testlerin ölçtüğü şey o dağınıklığın yerine
        // geçen kural — her geçiş önce Cik, sonra Gir koşturur.

        /// <summary>
        /// Geçiş, açık kipi kapatıp yenisini açar — bu sırayla ve birer kez.
        /// </summary>
        [Test]
        public void Enter_FromOneModeToAnother_RunsExitThenEnterExactlyOnce()
        {
            var trace = new List<string>();
            var idle = new RecordingMode("idle", trace);
            var next = new RecordingMode("next", trace);
            var machine = new BoardModeMachine(idle);

            machine.Enter(next);

            Assert.That(trace, Is.EqualTo(new[] { "idle.Exit", "next.Enter" }));
            Assert.That(machine.Current, Is.SameAs(next));
        }

        /// <summary>
        /// AYNI kipe ikinci kez geçiş sessizce yutulur.
        /// </summary>
        // BU TEST BİR OYUN HATASINI KİLİTLİYOR: yutulmasaydı yerleştirme
        // kipindeyken B tuşuna ikinci kez basmak hayaleti kapatıp yeniden açar,
        // taşınan hayaleti oyuncunun elinden düşürürdü.
        [Test]
        public void Enter_TheSameModeTwice_DoesNothingTheSecondTime()
        {
            var trace = new List<string>();
            var idle = new RecordingMode("idle", trace);
            var next = new RecordingMode("next", trace);
            var machine = new BoardModeMachine(idle);

            machine.Enter(next);
            trace.Clear();

            machine.Enter(next);

            Assert.That(trace, Is.Empty);
            Assert.That(machine.Current, Is.SameAs(next));
        }

        /// <summary>
        /// Yürürlükte OLMAYAN bir kipi bırakmak hiçbir şeye dokunmaz.
        /// </summary>
        // ŞART OLMASAYDI ÖLÇÜLEN ZARAR ŞUYDU: paletten bina almak ya da temizlik
        // süpürmesi bekleyen vuruşu düşürüyor ve ikisi de yerleştirme kipi
        // AÇIKKEN gelebiliyor — şartsız bir çıkış oyuncunun elindeki hayaleti
        // sessizce düşürürdü.
        [Test]
        public void LeaveIfCurrent_OnAModeThatIsNotCurrent_ChangesNothing()
        {
            var trace = new List<string>();
            var idle = new RecordingMode("idle", trace);
            var open = new RecordingMode("open", trace);
            var other = new RecordingMode("other", trace);
            var machine = new BoardModeMachine(idle);

            machine.Enter(open);
            trace.Clear();

            Assert.That(machine.LeaveIfCurrent(other), Is.False);
            Assert.That(trace, Is.Empty);
            Assert.That(machine.Current, Is.SameAs(open));
        }

        /// <summary>
        /// Yürürlükteki kip bırakılınca Boşta kipine dönülür.
        /// </summary>
        [Test]
        public void LeaveIfCurrent_OnTheCurrentMode_ReturnsToIdle()
        {
            var trace = new List<string>();
            var idle = new RecordingMode("idle", trace);
            var open = new RecordingMode("open", trace);
            var machine = new BoardModeMachine(idle);

            machine.Enter(open);
            trace.Clear();

            Assert.That(machine.LeaveIfCurrent(open), Is.True);
            Assert.That(trace, Is.EqualTo(new[] { "open.Exit", "idle.Enter" }));
            Assert.That(machine.Current, Is.SameAs(idle));
        }

        // ══ İKİ KİP YAN YANA YAŞAYAMAZ — "TEK MAKİNE" KANITI ═════════════
        // Bu turun tasarım kararı buydu: iki ayrı durum makinesi değil, TEK bir
        // kip makinesi. Aşağıdaki test o kararın koda düşmüş hâli.

        /// <summary>
        /// Yerleştirme kipine girmek, yazılı bekleyen vuruşu geçişin KENDİSİNDE
        /// düşürür — hiçbir çağıranın ayrıca iptal yazmasına gerek kalmadan.
        /// </summary>
        // ESKİ HÂLDE BU BİR ELLE YAZILMIŞ SATIRDI ve yazılmadığı her yeni geçiş
        // sessiz bir hata olurdu: bina koyulurken savaşçı kendiliğinden vurur ve
        // oyuncu vuruşun nereden geldiğini anlamazdı.
        [Test]
        public void Enter_PlacementWhileAStrikeIsPending_DropsTheOrderInTheTransition()
        {
            var board = new FakeBoard();
            var strike = new PendingStrikeMode(board);
            var placement = new StructurePlacementMode(board);
            var machine = new BoardModeMachine(new IdleBoardMode());
            board.Machine = machine;

            var attacker = new Unit("Striker");
            var target = new Unit("Raider");
            board.SelectedUnit = attacker;
            board.PutOnBoard(attacker, target);

            machine.Enter(strike);
            strike.Write(attacker, target, 0, 4);
            Assert.That(strike.IsAlive(), Is.True, "setup");

            machine.Enter(placement);

            Assert.That(board.StrikeAttacker, Is.Null);
            Assert.That(board.StrikeTarget, Is.Null);
            Assert.That(machine.Current, Is.SameAs(placement));
        }

        // ══ BEKLEYEN VURUŞUN İPTAL KOŞULLARI — KİPİN İÇİNDE AYNEN ════════
        // Üç koşul da eskiden BoardAdapter.PendingStrikeIsAlive'ın gövdesindeydi.
        // Buradaki testler onların taşınırken bozulmadığını ölçüyor.

        /// <summary>
        /// İki taraf da tahtada ve saldıran hâlâ seçili: emir ayakta.
        /// </summary>
        [Test]
        public void PendingStrike_IsAlive_WithBothSidesOnTheBoardAndTheAttackerSelected_IsTrue()
        {
            var board = new FakeBoard();
            PendingStrikeMode strike = NewStrikeOrder(board, out Unit attacker, out Unit target);

            Assert.That(strike.IsAlive(), Is.True);
            Assert.That(attacker, Is.SameAs(board.SelectedUnit));
            Assert.That(target, Is.SameAs(board.StrikeTarget));
        }

        /// <summary>
        /// SEÇİM DEĞİŞTİ: oyuncu başka bir birime geçtiyse eski emir düşer.
        /// </summary>
        [Test]
        public void PendingStrike_IsAlive_WhenTheSelectionMovedToAnotherUnit_IsFalse()
        {
            var board = new FakeBoard();
            PendingStrikeMode strike = NewStrikeOrder(board, out Unit _, out Unit _);

            var other = new Unit("Sapper");
            board.PutOnBoard(other);
            board.SelectedUnit = other;

            Assert.That(strike.IsAlive(), Is.False);
        }

        /// <summary>
        /// HEDEF TAHTADAN KALKTI: emir düşer.
        /// </summary>
        [Test]
        public void PendingStrike_IsAlive_WhenTheTargetLeftTheBoard_IsFalse()
        {
            var board = new FakeBoard();
            PendingStrikeMode strike = NewStrikeOrder(board, out Unit _, out Unit target);

            board.TakeOffBoard(target);

            Assert.That(strike.IsAlive(), Is.False);
        }

        /// <summary>
        /// SALDIRAN TAHTADAN KALKTI: emir düşer.
        /// </summary>
        // AYRI BİR TEST, ÇÜNKÜ İKİ TARAF AYRI SORULUYOR: tek bir test yazılsaydı
        // koşullardan biri silindiği gün öteki testi hâlâ yeşil tutardı.
        [Test]
        public void PendingStrike_IsAlive_WhenTheAttackerLeftTheBoard_IsFalse()
        {
            var board = new FakeBoard();
            PendingStrikeMode strike = NewStrikeOrder(board, out Unit attacker, out Unit _);

            board.TakeOffBoard(attacker);

            Assert.That(strike.IsAlive(), Is.False);
        }

        // ══ TIKLAMANIN ANLAMI KİPE GÖRE DEĞİŞİYOR ════════════════════════

        /// <summary>
        /// Aynı hedefe gelen ikinci tıklama emrin TEKRARIDIR ve kip onu yutar.
        /// </summary>
        [Test]
        public void PendingStrike_ConsumesClick_OnTheSameTarget_IsTrue()
        {
            var board = new FakeBoard();
            PendingStrikeMode strike = NewStrikeOrder(board, out Unit _, out Unit target);

            Assert.That(strike.ConsumesClick(target), Is.True);
        }

        /// <summary>
        /// BAŞKA bir hedefe tıklamak fikir değiştirmektir; kip onu yutmaz.
        /// </summary>
        [Test]
        public void PendingStrike_ConsumesClick_OnAnotherTarget_IsFalse()
        {
            var board = new FakeBoard();
            PendingStrikeMode strike = NewStrikeOrder(board, out Unit _, out Unit _);

            var other = new Unit("Scout");
            board.PutOnBoard(other);

            Assert.That(strike.ConsumesClick(other), Is.False);
        }

        /// <summary>
        /// Yazılı bir emir yokken hiçbir tıklama tekrar sayılmaz.
        /// </summary>
        // BU TEST BİR KİLİTLENMEYİ ÖNLÜYOR: koşul emirsiz durumda true dönseydi
        // dolu hücreye yapılan her tıklama sessizce tüketilir ve oyuncu hiçbir
        // şey seçemezdi. null argüman ayrıca sınanıyor, çünkü boş hücreye
        // tıklandığında oraya null geliyor ve iki null'un referans eşitliği
        // TRUE'dur.
        [Test]
        public void PendingStrike_ConsumesClick_WithNoOrderWritten_IsFalse()
        {
            var board = new FakeBoard();
            var strike = new PendingStrikeMode(board);

            Assert.That(strike.ConsumesClick(new Unit("Raider")), Is.False);
            Assert.That(strike.ConsumesClick(null), Is.False);
        }

        /// <summary>
        /// Boşta kip hiçbir tıklamayı yutmaz: sıradan akış her zaman çalışır.
        /// </summary>
        [Test]
        public void Idle_ConsumesNoClick()
        {
            var idle = new IdleBoardMode();

            Assert.That(idle.OwnsPointer, Is.False);
            Assert.That(idle.ConsumesClick(new Unit("Raider")), Is.False);
            Assert.That(idle.ConsumesClick(null), Is.False);
        }

        // ══ BEKLEYEN VURUŞUN KARE İŞİ ════════════════════════════════════

        /// <summary>
        /// Görsel hâlâ yürüyorsa vuruş BEKLER ve emir ayakta kalır.
        /// </summary>
        [Test]
        public void PendingStrike_Advance_WhileTheViewIsWalking_WaitsAndKeepsTheOrder()
        {
            var board = new FakeBoard();
            PendingStrikeMode strike = NewStrikeOrder(board, out Unit attacker, out Unit _);
            board.Machine.Enter(strike);
            board.StartWalking(attacker);

            strike.Advance();

            Assert.That(board.Strikes, Is.Empty);
            Assert.That(board.StrikeAttacker, Is.SameAs(attacker));
            Assert.That(board.Machine.Current, Is.SameAs(strike));
        }

        /// <summary>
        /// Yürüyüş bitince vuruş iner — ve emir vuruştan ÖNCE silinir.
        /// </summary>
        // SIRA BİR KARARDIR: saldırı bir durum değişikliği doğuruyor, o zincir
        // temizliğe kadar gidebiliyor ve yarım kalmış bir emrin o sırada ikinci
        // kez okunması aynı vuruşu tekrarlardı.
        [Test]
        public void PendingStrike_Advance_WhenTheWalkEnded_ClearsTheOrderBeforeStriking()
        {
            var board = new FakeBoard();
            PendingStrikeMode strike = NewStrikeOrder(board, out Unit attacker, out Unit target);
            board.Machine.Enter(strike);

            strike.Advance();

            Assert.That(board.Strikes.Count, Is.EqualTo(1));
            Assert.That(board.Strikes[0].Attacker, Is.SameAs(attacker));
            Assert.That(board.Strikes[0].Target, Is.SameAs(target));
            Assert.That(board.Strikes[0].X, Is.EqualTo(2));
            Assert.That(board.Strikes[0].Y, Is.EqualTo(3));

            // EMİR VURUŞ ANINDA ZATEN SİLİNMİŞTİ: sahte tahta, kaydı vuruş
            // çağrısının İÇİNDE okuyor.
            Assert.That(board.Strikes[0].OrderWasStillWritten, Is.False);
            Assert.That(board.Machine.Current, Is.InstanceOf<IdleBoardMode>());
        }

        /// <summary>
        /// Emir düştüyse kipten çıkılır ve hiçbir vuruş yapılmaz.
        /// </summary>
        [Test]
        public void PendingStrike_Advance_WhenTheOrderDied_LeavesTheModeWithoutStriking()
        {
            var board = new FakeBoard();
            PendingStrikeMode strike = NewStrikeOrder(board, out Unit _, out Unit target);
            board.Machine.Enter(strike);
            board.TakeOffBoard(target);

            strike.Advance();

            Assert.That(board.Strikes, Is.Empty);
            Assert.That(board.StrikeAttacker, Is.Null);
            Assert.That(board.Machine.Current, Is.InstanceOf<IdleBoardMode>());
        }

        // ══ YERLEŞTİRME KİPİNİN KARE İŞİ ═════════════════════════════════

        /// <summary>
        /// Kipe girmek hayaleti açar ve jesti sıfırlar.
        /// </summary>
        // JEST SIFIRLANMASAYDI önceki kipten kalan "basılı" fazı, bu kipteki ilk
        // bırakışı sahte bir tıklama olarak okurdu.
        [Test]
        public void Placement_Enter_ShowsTheGhostAndResetsTheGesture()
        {
            var board = new FakeBoard { SelectedUnit = new Unit("Vanguard") };
            var placement = new StructurePlacementMode(board);

            placement.Enter();

            Assert.That(placement.OwnsPointer, Is.True);
            Assert.That(board.GhostVisible, Is.True);
            Assert.That(board.GestureResets, Is.EqualTo(1));
            Assert.That(Said(board, "Placement mode ON"), Is.True);
        }

        /// <summary>
        /// Kipten çıkmak hayaleti gizler ve taşınan hayaleti bırakır.
        /// </summary>
        [Test]
        public void Placement_Exit_HidesTheGhost()
        {
            var board = new FakeBoard { SelectedUnit = new Unit("Vanguard") };
            var placement = new StructurePlacementMode(board);

            placement.Enter();
            placement.Exit();

            Assert.That(board.GhostVisible, Is.False);
        }

        /// <summary>
        /// İptal tuşu kipi kapatır ve tahtaya HİÇ dokunmaz.
        /// </summary>
        [Test]
        public void Placement_Advance_WhenTheCancelKeyIsDown_LeavesWithoutPlacing()
        {
            var board = new FakeBoard { SelectedUnit = new Unit("Vanguard") };
            StructurePlacementMode placement = EnterPlacement(board);
            board.PlacementCancelRequested = true;

            placement.Advance();

            Assert.That(board.Placements, Is.Empty);
            Assert.That(board.Machine.Current, Is.InstanceOf<IdleBoardMode>());
            Assert.That(Said(board, "CANCELLED"), Is.True);
        }

        /// <summary>
        /// Koyacak birim savaştan düşerse kip kendini kapatır.
        /// </summary>
        // TEORİK DEĞİL: savaşın saati kipin kare işinden ÖNCE ilerliyor ve ceset
        // süresi dolan birimi temizlerken seçimi de null'a çekiyor.
        [Test]
        public void Placement_Advance_WhenThePlacingUnitLeftTheBattle_LeavesTheMode()
        {
            var board = new FakeBoard { SelectedUnit = new Unit("Vanguard") };
            StructurePlacementMode placement = EnterPlacement(board);
            board.SelectedUnit = null;

            placement.Advance();

            Assert.That(board.Placements, Is.Empty);
            Assert.That(board.Machine.Current, Is.InstanceOf<IdleBoardMode>());
            Assert.That(Said(board, "left the battle"), Is.True);
        }

        /// <summary>
        /// SÜRÜKLE-BIRAK: bırakıldığı yer yerleştirilecek yerdir.
        /// </summary>
        [Test]
        public void Placement_Advance_OnDragRelease_PlacesAtTheReleasedCell()
        {
            var board = new FakeBoard { SelectedUnit = new Unit("Vanguard") };
            StructurePlacementMode placement = EnterPlacement(board);
            board.PointerCell(1, 4);
            board.NextPhase = PointerPhase.DragReleased;

            placement.Advance();

            Assert.That(board.Placements, Is.EqualTo(new[] { (1, 4) }));
        }

        /// <summary>
        /// TIKLA-BIRAK, ilk bırakış: hayalet fareye YAPIŞIR, yapı KONMAZ.
        /// </summary>
        [Test]
        public void Placement_Advance_OnTheFirstClickRelease_CarriesTheGhostInsteadOfPlacing()
        {
            var board = new FakeBoard { SelectedUnit = new Unit("Vanguard") };
            StructurePlacementMode placement = EnterPlacement(board);
            board.PointerCell(1, 4);
            board.NextPhase = PointerPhase.ClickReleased;

            placement.Advance();

            Assert.That(board.Placements, Is.Empty);
            Assert.That(Said(board, "Ghost is now carried"), Is.True);
            Assert.That(board.Machine.Current, Is.SameAs(placement), "kipte KALINIR");
        }

        /// <summary>
        /// TIKLA-BIRAK, ikinci bırakış: taşınan hayalet o hücreye konur.
        /// </summary>
        [Test]
        public void Placement_Advance_OnTheSecondClickRelease_Places()
        {
            var board = new FakeBoard { SelectedUnit = new Unit("Vanguard") };
            StructurePlacementMode placement = EnterPlacement(board);
            board.NextPhase = PointerPhase.ClickReleased;

            board.PointerCell(1, 4);
            placement.Advance();

            board.PointerCell(2, 0);
            placement.Advance();

            Assert.That(board.Placements, Is.EqualTo(new[] { (2, 0) }),
                "ikinci tıklama BIRAKILDIĞI hücreye koyar, ilkine değil");
        }

        /// <summary>
        /// Hayalet HER KARE imlecin hücresine oturur.
        /// </summary>
        // KOŞULSUZ OLMASI ŞART: yalnız sürüklerken taşınsaydı tıkla-bırak
        // akışında hayalet yerinde donar ve oyuncu nereye koyacağını göremezdi.
        [Test]
        public void Placement_Advance_MovesTheGhostEvenWhenNothingIsReleased()
        {
            var board = new FakeBoard { SelectedUnit = new Unit("Vanguard") };
            StructurePlacementMode placement = EnterPlacement(board);
            board.PointerCell(2, 1);
            board.NextPhase = PointerPhase.Idle;

            placement.Advance();

            Assert.That(board.GhostX, Is.EqualTo(2));
            Assert.That(board.GhostY, Is.EqualTo(1));
            Assert.That(board.Placements, Is.Empty);
        }

        /// <summary>
        /// Fare tahtanın dışındaysa kip hiçbir şey yapmaz ve kipte kalır.
        /// </summary>
        [Test]
        public void Placement_Advance_WhenThePointerCannotBeRead_DoesNothing()
        {
            var board = new FakeBoard { SelectedUnit = new Unit("Vanguard") };
            StructurePlacementMode placement = EnterPlacement(board);
            board.PointerReadable = false;
            board.NextPhase = PointerPhase.DragReleased;

            placement.Advance();

            Assert.That(board.Placements, Is.Empty);
            Assert.That(board.Machine.Current, Is.SameAs(placement));
        }

        /// <summary>
        /// Sahte tahtaya yazılan satırlardan biri bu parçayı taşıyor mu?
        /// </summary>
        // AYRI BİR YARDIMCI, ÇÜNKÜ İDDİA CÜMLENİN TAMAMINA DEĞİL PARÇASINA
        // BAKIYOR: tam metne kilitlenmek, cümlenin bir kelimesi düzeldiği gün
        // ölçmediği bir şey yüzünden kırmızıya dönerdi.
        private static bool Said(FakeBoard board, string fragment)
        {
            return board.Lines.Exists(line => line.Contains(fragment));
        }

        /// <summary>
        /// Emri yazılı, iki tarafı da tahtada duran bir bekleyen vuruş kurar.
        /// </summary>
        private static PendingStrikeMode NewStrikeOrder(
            FakeBoard board, out Unit attacker, out Unit target)
        {
            attacker = new Unit("Striker");
            target = new Unit("Raider");
            board.PutOnBoard(attacker, target);
            board.SelectedUnit = attacker;

            var strike = new PendingStrikeMode(board);
            strike.Write(attacker, target, 2, 3);
            return strike;
        }

        /// <summary>
        /// Yerleştirme kipini kurar, makineye sokar ve girdirir.
        /// </summary>
        private static StructurePlacementMode EnterPlacement(FakeBoard board)
        {
            var placement = new StructurePlacementMode(board);
            board.Machine.Enter(placement);
            return placement;
        }

        /// <summary>
        /// Yalnız girip çıktığını KAYDEDEN kip: geçiş sırasını sınamak için.
        /// </summary>
        // GERÇEK KİPLERLE SINANMADI ve bu bilerek: geçişin kendisini ölçmek
        // isterken gerçek kiplerin hayalet ve emir işleri iddiaya karışırdı.
        private sealed class RecordingMode : IBoardMode
        {
            private readonly string name;
            private readonly List<string> trace;

            public RecordingMode(string modeName, List<string> sharedTrace)
            {
                name = modeName;
                trace = sharedTrace;
            }

            public bool OwnsPointer => false;

            public void Enter()
            {
                trace.Add($"{name}.Enter");
            }

            public void Exit()
            {
                trace.Add($"{name}.Exit");
            }

            public void Advance()
            {
                trace.Add($"{name}.Advance");
            }

            public bool ConsumesClick(Unit clicked)
            {
                return false;
            }
        }

        /// <summary>
        /// Kiplerin tahtaya bakan penceresinin sahtesi.
        /// </summary>
        // SINGLETON YOK VE OLMAMASI ÖLÇÜLEBİLİR: her test kendi tahtasını
        // kuruyor, hiçbir test ötekinden bir durum devralmıyor ve bu yüzden
        // TearDown'a gerek kalmıyor.
        private sealed class FakeBoard : IPlacementModeHost, IPendingStrikeHost
        {
            private readonly HashSet<Unit> onBoard = new HashSet<Unit>();
            private readonly HashSet<Unit> walking = new HashSet<Unit>();

            public readonly List<string> Lines = new List<string>();
            public readonly List<(int X, int Y)> Placements = new List<(int, int)>();
            // ADLANDIRILMIŞ DEMET, `record` DEĞİL: konumsal bir record `init`
            // erişimcisi üretiyor ve o da IsExternalInit istiyor — bu tip
            // GridStrategy.Core içinde `internal` yaşadığı için test derlemesinden
            // görünmüyor (ölçüldü: CS0518). Demet aynı okunaklılığı bedelsiz veriyor.
            public readonly List<(Unit Attacker, Unit Target, int X, int Y, bool OrderWasStillWritten)>
                Strikes = new List<(Unit, Unit, int, int, bool)>();

            public BoardModeMachine Machine = new BoardModeMachine(new IdleBoardMode());

            public Unit SelectedUnit { get; set; }

            public bool PlacementCancelRequested { get; set; }

            public bool PointerReadable = true;
            public int PointerX;
            public int PointerY;
            public PointerPhase NextPhase = PointerPhase.Idle;

            public bool GhostVisible;
            public int GhostX = -1;
            public int GhostY = -1;
            public int GestureResets;

            public Unit StrikeAttacker { get; private set; }

            public Unit StrikeTarget { get; private set; }

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

            public void PointerCell(int x, int y)
            {
                PointerReadable = true;
                PointerX = x;
                PointerY = y;
            }

            public void Log(string message)
            {
                Lines.Add(message);
            }

            public void LeaveMode(IBoardMode mode)
            {
                Machine.LeaveIfCurrent(mode);
            }

            public void ShowPlacementGhost(bool visible)
            {
                GhostVisible = visible;
            }

            public void MovePlacementGhostTo(int x, int y)
            {
                GhostX = x;
                GhostY = y;
            }

            public bool TryReadPointerCell(out float worldX, out float worldY, out int x, out int y)
            {
                worldX = PointerX;
                worldY = PointerY;
                x = PointerX;
                y = PointerY;
                return PointerReadable;
            }

            public void ResetPointerGesture()
            {
                GestureResets++;
            }

            public PointerPhase FeedPointerGesture(float worldX, float worldY)
            {
                return NextPhase;
            }

            public void CommitPlacement(int x, int y)
            {
                Placements.Add((x, y));
            }

            public void WriteStrikeOrder(Unit attacker, Unit target)
            {
                StrikeAttacker = attacker;
                StrikeTarget = target;
            }

            public void ClearStrikeOrder()
            {
                StrikeAttacker = null;
                StrikeTarget = null;
            }

            public bool IsOnBoard(Unit unit)
            {
                return unit != null && onBoard.Contains(unit);
            }

            public bool IsViewWalking(Unit unit)
            {
                return unit != null && walking.Contains(unit);
            }

            public void ExecuteStrike(Unit attacker, Unit target, int x, int y)
            {
                // KAYIT VURUŞUN İÇİNDE OKUNUYOR: "emir önce silinir, sonra
                // vurulur" sırasını ölçmenin tek yolu bu an.
                Strikes.Add((attacker, target, x, y, StrikeAttacker != null));
            }
        }
    }
}
