using System;
using System.Linq;
using System.Reflection;
using GridStrategy.Core;
using NUnit.Framework;

namespace GridStrategy.Tests.EditMode.Core
{
    /// <summary>
    /// Bu dosya parçaları test etmiyor — uzaklık <see cref="GridDistanceTests"/>'te,
    /// hücre yazma <see cref="UnitGridTests"/>'te sınanıyor. Burada sınanan tek
    /// şey AKIŞ: hangi kurala hangi sırayla soruluyor, sonuç nasıl adlandırılıyor
    /// ve reddedilen bir hareket tahtada iz bırakıyor mu.
    ///
    /// Dört test bir DAVRANIŞI değil bir KARARI koruyor:
    /// <list type="bullet">
    /// <item>dolu + menzil dışı → hangi ret sebebi dönüyor (sıra kararı)</item>
    /// <item>tahta dışı + menzil dışı → hangi ret sebebi dönüyor (sıra kararı)</item>
    /// <item>kendi hücresine hareket → izin mi ret mi (doluluk sorusunun şekli)</item>
    /// <item>birimin durumu hiç sorulmuyor → KATMAN kararı, dosyanın sonunda</item>
    /// </list>
    /// Dördü de kırmızıya dönerse kod hâlâ "çalışıyor" görünür; kırılan şey
    /// çağıranın sonuca bakarak verdiği karardır.
    /// </summary>
    public sealed class MoveActionTests
    {
        // 3x5: x ekseni 0..2, y ekseni 0..4. Eksenlerin farklı olması bilerek —
        // birinin yerine diğerini yazan bir hata sessiz kalmasın.
        private static UnitGrid NewBoard()
        {
            return new UnitGrid(width: 3, height: 5);
        }

        // Kurulum tahtaya doğrudan PlaceUnit ile yazıyor, MoveAction ile değil:
        // kurulum sınanan şeyi kullanırsa test kendi kendini doğrulamış olur.
        private static Unit PlaceNewUnit(UnitGrid board, int x, int y, string name)
        {
            var unit = new Unit(name);
            board.PlaceUnit(x, y, unit);
            return unit;
        }

        [Test]
        public void Execute_ValidStep_MovesTheUnitOnTheBoard()
        {
            UnitGrid board = NewBoard();
            Unit mover = PlaceNewUnit(board, 1, 2, "kral");

            MoveOutcome outcome = MoveAction.Execute(board, mover, 1, 2, 1, 3, moveRange: 1);

            Assert.That(outcome, Is.EqualTo(MoveOutcome.Moved));
            Assert.That(
                board.TryGetUnit(1, 2, out Unit _),
                Is.False,
                "the source cell must be empty after a successful move");
            Assert.That(
                board.TryGetUnit(1, 3, out Unit arrived) && ReferenceEquals(arrived, mover),
                Is.True,
                "the destination must hold the exact same Unit instance, not a copy");
        }

        [Test]
        public void Execute_DestinationOutsideBoard_IsRejectedAndTheUnitStaysPut()
        {
            UnitGrid board = NewBoard();
            Unit mover = PlaceNewUnit(board, 1, 2, "kral");

            MoveOutcome outcome = MoveAction.Execute(board, mover, 1, 2, 9, 9, moveRange: 99);

            Assert.That(outcome, Is.EqualTo(MoveOutcome.RejectedInvalidDestination));
            // Reddedilen hareket hiçbir yan etki bırakmamalı. Bu satır olmasaydı
            // "önce taşı sonra kontrol et" hatası testten geçerdi.
            Assert.That(board.TryGetUnit(1, 2, out Unit _), Is.True);
        }

        [Test]
        public void Execute_OutOfRange_IsRejectedAndTheUnitStaysPut()
        {
            UnitGrid board = NewBoard();
            Unit mover = PlaceNewUnit(board, 0, 0, "kral");

            MoveOutcome outcome = MoveAction.Execute(board, mover, 0, 0, 0, 2, moveRange: 1);

            Assert.That(outcome, Is.EqualTo(MoveOutcome.RejectedOutOfRange));
            Assert.That(board.TryGetUnit(0, 0, out Unit _), Is.True);
            Assert.That(board.TryGetUnit(0, 2, out Unit _), Is.False);
        }

        [Test]
        public void Execute_OccupiedDestination_IsRejectedAndNeitherUnitMoves()
        {
            UnitGrid board = NewBoard();
            Unit mover = PlaceNewUnit(board, 1, 2, "kral");
            Unit blocker = PlaceNewUnit(board, 1, 3, "asker");

            MoveOutcome outcome = MoveAction.Execute(board, mover, 1, 2, 1, 3, moveRange: 1);

            Assert.That(outcome, Is.EqualTo(MoveOutcome.RejectedCellOccupied));
            Assert.That(
                board.TryGetUnit(1, 2, out Unit stayed) && ReferenceEquals(stayed, mover),
                Is.True,
                "a rejected move must not empty the source cell");
            Assert.That(
                board.TryGetUnit(1, 3, out Unit held) && ReferenceEquals(held, blocker),
                Is.True,
                "the blocker must not be overwritten by the rejected move");
        }

        /// <summary>
        /// SIRA KARARINI koruyan test. Hedef hücre hem dolu hem menzil dışı —
        /// iki ret sebebi de doğrudur, ama yalnız biri KULLANIŞLIDIR.
        ///
        /// "Menzil dışı" doğru olan, çünkü doluluk bilgisi bu birimin
        /// ulaşamadığı bir hücreye ait: yol bulucu onu duyarsa hücreyi kalıcı
        /// engelli işaretler, oysa sorun hücre değil mesafedir.
        ///
        /// Bu test kırmızıya dönerse MoveAction içindeki iki kontrolün sırası
        /// değişmiş demektir — ve o değişiklik başka hiçbir testi kırmaz.
        /// </summary>
        [Test]
        public void Execute_OccupiedCellOutOfRange_PrefersOutOfRange()
        {
            UnitGrid board = NewBoard();
            Unit mover = PlaceNewUnit(board, 0, 4, "kral");
            PlaceNewUnit(board, 2, 0, "asker");

            MoveOutcome outcome = MoveAction.Execute(board, mover, 0, 4, 2, 0, moveRange: 1);

            Assert.That(
                outcome,
                Is.EqualTo(MoveOutcome.RejectedOutOfRange),
                "an unreachable cell must be reported as out of range, not as occupied");
        }

        /// <summary>
        /// SIRA KARARINI koruyan ikinci test. Hedef hem tahta dışı hem menzil
        /// dışı. "Tahta dışı" doğru olan, çünkü o sebep diğeri düzeltilse bile
        /// AYAKTA KALIR: 3x5'lik tahtada (9,9) hücresi ne yaklaşarak ne
        /// bekleyerek geçerli olur.
        ///
        /// Kırmızıya dönerse sınır kontrolü menzilin arkasına düşmüş demektir ve
        /// çağıran "yaklaşırsam olur" cevabı alıp sonsuza kadar yaklaşmaya
        /// çalışır.
        /// </summary>
        [Test]
        public void Execute_OutsideBoardAndOutOfRange_PrefersInvalidDestination()
        {
            UnitGrid board = NewBoard();
            Unit mover = PlaceNewUnit(board, 1, 2, "kral");

            MoveOutcome outcome = MoveAction.Execute(board, mover, 1, 2, 9, 9, moveRange: 1);

            Assert.That(
                outcome,
                Is.EqualTo(MoveOutcome.RejectedInvalidDestination),
                "a cell that can never exist must not be reported as merely out of range");
        }

        /// <summary>
        /// ÖLÇÜM KARARININ akıştan görünen yüzü. Uzaklık kararı GridDistance'a
        /// ait ve orada sınanıyor; bu test o kararın AKIŞ tarafından yeniden
        /// yazılmadığını koruyor.
        ///
        /// Kırmızıya dönerse ya ölçü Manhattan'a kaymıştır ya da biri buraya
        /// ikinci bir mesafe hesabı yazmıştır — ve o an aynı kural iki yerde
        /// yaşamaya başlar.
        /// </summary>
        [Test]
        public void Execute_DiagonalStepWithMoveRangeOne_IsAllowed()
        {
            UnitGrid board = NewBoard();
            Unit mover = PlaceNewUnit(board, 1, 2, "kral");

            MoveOutcome outcome = MoveAction.Execute(board, mover, 1, 2, 2, 3, moveRange: 1);

            Assert.That(
                outcome,
                Is.EqualTo(MoveOutcome.Moved),
                "a diagonal neighbour is one step away, so move range 1 reaches it");
        }

        /// <summary>
        /// DOLULUK SORUSUNUN ŞEKLİNİ koruyan test. Birim kendi hücresine
        /// "taşındığında" orada duran tek şey kendisidir ve kendisi bir engel
        /// değildir.
        ///
        /// Kırmızıya dönerse doluluk kontrolü "orada BAŞKASI var mı" olmaktan
        /// çıkıp "orada BİRİ var mı" olmuş demektir. Bedeli tıklama akışında
        /// görünür: seçili birimin üstüne ikinci kez tıklamak "orada biri var"
        /// uyarısı üretir ve o biri oyuncunun kendi birimidir.
        /// </summary>
        [Test]
        public void Execute_MovingToItsOwnCell_IsAllowed()
        {
            UnitGrid board = NewBoard();
            Unit mover = PlaceNewUnit(board, 1, 2, "kral");

            MoveOutcome outcome = MoveAction.Execute(board, mover, 1, 2, 1, 2, moveRange: 0);

            Assert.That(
                outcome,
                Is.EqualTo(MoveOutcome.Moved),
                "a unit cannot block itself out of the cell it is already standing on");
            Assert.That(
                board.TryGetUnit(1, 2, out Unit stayed) && ReferenceEquals(stayed, mover),
                Is.True,
                "a zero length move must leave the unit exactly where it was");
        }

        [Test]
        public void Execute_ZeroMoveRange_RejectsEveryStep()
        {
            // Menzil 0 geçerlidir ve "kımıldayamaz" demektir. AttackProfile
            // menzil için en az 1 ister; asimetri MoveAction'da gerekçelendirildi.
            UnitGrid board = NewBoard();
            Unit mover = PlaceNewUnit(board, 1, 2, "kral");

            MoveOutcome outcome = MoveAction.Execute(board, mover, 1, 2, 1, 3, moveRange: 0);

            Assert.That(outcome, Is.EqualTo(MoveOutcome.RejectedOutOfRange));
        }

        [Test]
        public void Execute_UnitNotOnTheSourceCell_Throws()
        {
            UnitGrid board = NewBoard();
            Unit mover = PlaceNewUnit(board, 1, 2, "kral");

            // Tahta konumun tek sahibidir; çağıranın kaydı onunla ayrışmışsa bu
            // bir oyun sonucu değil, bir programcı hatasıdır.
            Assert.Throws<ArgumentException>(
                () => MoveAction.Execute(board, mover, 0, 0, 0, 1, moveRange: 1),
                "moving a unit from a cell it is not standing on is a caller defect");
        }

        [Test]
        public void Execute_AnotherUnitOnTheSourceCell_Throws()
        {
            UnitGrid board = NewBoard();
            PlaceNewUnit(board, 1, 2, "kral");
            var stranger = new Unit("yabanci");

            Assert.Throws<ArgumentException>(
                () => MoveAction.Execute(board, stranger, 1, 2, 1, 3, moveRange: 1),
                "the source cell must hold the exact unit being moved, not just any unit");
        }

        [Test]
        public void Execute_SourceOutsideBoard_Throws()
        {
            UnitGrid board = NewBoard();
            var mover = new Unit("kral");

            Assert.Throws<ArgumentException>(
                () => MoveAction.Execute(board, mover, 9, 9, 1, 2, moveRange: 1));
        }

        [Test]
        public void Execute_NullBoard_Throws()
        {
            var mover = new Unit("kral");

            Assert.Throws<ArgumentNullException>(
                () => MoveAction.Execute(null, mover, 0, 0, 0, 1, moveRange: 1));
        }

        [Test]
        public void Execute_NullUnit_Throws()
        {
            UnitGrid board = NewBoard();

            Assert.Throws<ArgumentNullException>(
                () => MoveAction.Execute(board, null, 0, 0, 0, 1, moveRange: 1));
        }

        [Test]
        public void Execute_NegativeMoveRange_Throws()
        {
            UnitGrid board = NewBoard();
            Unit mover = PlaceNewUnit(board, 1, 2, "kral");

            Assert.Throws<ArgumentOutOfRangeException>(
                () => MoveAction.Execute(board, mover, 1, 2, 1, 3, moveRange: -1));
        }

        // REDDEDILEN - MoveActionTests.cs:276 yerine (karar, durumu doğrudan
        //              sınayan bir testle korunur):
        //     [Test] public void Execute_DownedUnit_IsRejected()
        //     {
        //         Assert.That(MoveAction.Execute(board, mover, 1, 2, 1, 3,
        //                     UnitState.Downed, moveRange: 1),
        //                     Is.Not.EqualTo(MoveOutcome.Moved));
        //     }
        // KIRILAN  : DERLENMEZ — ve tam bu yüzden karar doğrudur.
        //            UnitState GridStrategy.Combat'ta -> Core.EditModeTests
        //            asmdef'i yalnızca GridStrategy.Core'a referans veriyor
        //            derleyici: tipi bulamaz der  .  test: sınayabilseydi kural
        //            zaten yanlış katmanda olurdu
        // KAZANIRDI: durum Core'a taşınsaydı; o gün bu test hem yazılabilir hem
        //            de doğru yer olurdu — ama o gün Combat'ın ayrı bir assembly
        //            olmasının sebebi de kalmazdı.
        // TEK CUMLE: Yazılamayan test kararın kanıtıdır; aşağıdaki test bu
        //            yüzden hareketi değil SINIRI ölçüyor.
        /// <summary>
        /// KATMAN KARARINI koruyan test — bir davranışı değil bir SINIRI koruyor.
        ///
        /// <see cref="MoveAction"/> bir birimin DURUMUNU hiç sormaz; düşmüş bir
        /// birim de bu akıştan geçer. Eksik değil, GridStrategy.Core'un
        /// GridStrategy.Combat'ı GÖRMEMESİNİN sonucu: UnitState orada yaşıyor ve
        /// bu katman onu tip olarak bile yazamaz. Sahibi MovementRules, akışa
        /// bağlanması Battle'ın işi.
        ///
        /// Ölçünün SINIRI bilerek kabul edildi: derleyici KULLANILMAYAN referansı
        /// üstveriye yazmaz, yani test sınırın GERÇEKTEN geçildiği anda kırmızı
        /// olur — ölçmek istediğimiz şey zaten oydu.
        ///
        /// Kırmızıya dönerse "kod bozuldu" DEĞİL "assembly sınırı değişti"
        /// demektir; doğru tepki testi düzeltmek değil, kararı yeniden tartışmak.
        /// </summary>
        [Test]
        public void Move_DoesNotAskAboutUnitState_BecauseCoreCannotSeeCombat()
        {
            Assembly core = typeof(MoveAction).Assembly;

            string[] siblingGameAssemblies = core.GetReferencedAssemblies()
                .Select(reference => reference.Name)
                .Where(name => name != null
                               && name.StartsWith("GridStrategy.", StringComparison.Ordinal))
                .ToArray();

            Assert.That(
                siblingGameAssemblies,
                Is.Empty,
                "GridStrategy.Core must reference no sibling game assembly; MoveAction cannot ask about UnitState because UnitState lives in GridStrategy.Combat");

            bool everyParameterIsCoreOrBcl = typeof(MoveAction)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .SelectMany(method => method.GetParameters())
                .All(parameter => parameter.ParameterType.Assembly == core
                                  || parameter.ParameterType.Assembly == typeof(int).Assembly);

            Assert.That(
                everyParameterIsCoreOrBcl,
                Is.True,
                "every MoveAction parameter must come from Core itself or the base class library, never from a combat type");
        }

        /// <summary>
        /// AŞIRI YÜKLEMENİN aynı kuralı yürüttüğünü sabitleyen test. Profil
        /// alan sürüm kuralı KOPYALAMIYOR, sayı alan sürüme devrediyor —
        /// kopyalasaydı iki sürüm zamanla ayrışır ve hangisinin gerçek kural
        /// olduğu belirsizleşirdi.
        ///
        /// Kırmızıya dönerse biri profil sürümüne ikinci bir akış yazmış
        /// demektir.
        /// </summary>
        [Test]
        public void Execute_WithProfile_MatchesTheIntOverload()
        {
            UnitGrid intBoard = NewBoard();
            Unit intMover = PlaceNewUnit(intBoard, 1, 2, "kral");

            UnitGrid profileBoard = NewBoard();
            Unit profileMover = PlaceNewUnit(profileBoard, 1, 2, "kral");

            MoveOutcome fromInt =
                MoveAction.Execute(intBoard, intMover, 1, 2, 0, 4, moveRange: 1);
            MoveOutcome fromProfile =
                MoveAction.Execute(profileBoard, profileMover, 1, 2, 0, 4, new MoveProfile(range: 1));

            Assert.That(
                fromProfile,
                Is.EqualTo(fromInt),
                "the profile overload must delegate to the int one, not run a second copy of the rule");
            Assert.That(fromProfile, Is.EqualTo(MoveOutcome.RejectedOutOfRange));
        }

        // Menzil 0 profille de ifade edilebilir: AttackProfile 0 menzili
        // reddeder, MoveProfile etmez ve gerekçesi MoveProfile.cs'te yazılı.
        [Test]
        public void Execute_ZeroRangeProfile_RejectsEveryStep()
        {
            UnitGrid board = NewBoard();
            Unit mover = PlaceNewUnit(board, 1, 2, "kral");

            MoveOutcome outcome =
                MoveAction.Execute(board, mover, 1, 2, 1, 3, new MoveProfile(range: 0));

            Assert.That(
                outcome,
                Is.EqualTo(MoveOutcome.RejectedOutOfRange),
                "a zero range profile describes a rooted unit, and a rooted unit reaches no other cell");
        }

        [Test]
        public void Execute_NullProfile_Throws()
        {
            UnitGrid board = NewBoard();
            Unit mover = PlaceNewUnit(board, 1, 2, "kral");

            // Menzilsiz bir hareket denemesi bir oyun sonucu değil, bir
            // çağıran hatasıdır — AttackResolver'ın profil için koyduğu
            // koruma ile aynı gerekçe.
            Assert.Throws<ArgumentNullException>(
                () => MoveAction.Execute(board, mover, 1, 2, 1, 3, (MoveProfile)null));
        }
    }
}
