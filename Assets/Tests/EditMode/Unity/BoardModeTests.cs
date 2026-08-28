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
    /// <b>BEKLEYEN VURUŞ TESTLERİ BU DOSYADAN GİTTİ.</b> Üçüncü kip
    /// (<c>PendingStrikeMode</c>) kaldırıldı ve emirler kip olmaktan çıktı;
    /// onların yerini <c>UnitOrderTests</c> aldı. Buradaki testler artık
    /// yalnız geçişin kendisini ve yerleştirme kipini ölçüyor.
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

        // ══ KİP YAN YANA BİR EMİRLE YAŞAYABİLİR — VE BU ÇEVRİLMİŞ BİR KARAR ══
        // Burada `Enter_PlacementWhileAStrikeIsPending_DropsTheOrderInTheTransition`
        // duruyordu (SILINDI): yerleştirme kipine girmek yazılı emri geçişin
        // kendisinde düşürüyordu. O test bir KARAR KAYDIYDI ve dünyada değişen
        // şey şudur — emir artık tahtaya değil BİRİME ait. Bina koymak, üç
        // savaşçının sürmekte olan saldırısını neden kessin? Yerini alan iddia
        // UnitOrderTests içinde: emirler kip makinesinden bağımsız yaşıyor.
        //
        // Aynı kaldırmayla giden ötekiler: PendingStrike_IsAlive_* (4),
        // PendingStrike_ConsumesClick_* (3), PendingStrike_Advance_* (3) ve
        // Idle_ConsumesNoClick. Toplam ON İKİ test gitti; iddialarının
        // karşılığı UnitOrderTests'te yeniden yazıldı ve ConsumesClick,
        // çağıranı kalmadığı için ARAYÜZDEN de çıktı.
        // → Docs/deep/konular/09-kararlarin-cevrilmesi.md (madde 2)

        /// <summary>
        /// Boşta kip fareyi sahiplenmez: sıradan tıklama akışı her zaman çalışır.
        /// </summary>
        [Test]
        public void Idle_OwnsNoPointer()
        {
            Assert.That(new IdleBoardMode().OwnsPointer, Is.False);
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
        }

        /// <summary>
        /// Kiplerin tahtaya bakan penceresinin sahtesi.
        /// </summary>
        // SINGLETON YOK VE OLMAMASI ÖLÇÜLEBİLİR: her test kendi tahtasını
        // kuruyor, hiçbir test ötekinden bir durum devralmıyor ve bu yüzden
        // TearDown'a gerek kalmıyor.
        private sealed class FakeBoard : IPlacementModeHost
        {
            public readonly List<string> Lines = new List<string>();
            public readonly List<(int X, int Y)> Placements = new List<(int, int)>();
            // ADLANDIRILMIŞ DEMET, `record` DEĞİL: konumsal bir record `init`
            // erişimcisi üretiyor ve o da IsExternalInit istiyor — bu tip
            // GridStrategy.Core içinde `internal` yaşadığı için test derlemesinden
            // görünmüyor (ölçüldü: CS0518). Demet aynı okunaklılığı bedelsiz veriyor.
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
        }
    }
}
