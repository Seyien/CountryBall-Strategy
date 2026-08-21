using System;
using System.Collections.Generic;
using NUnit.Framework;
using GridStrategy.Combat;
using GridStrategy.Core;

namespace GridStrategy.Tests.EditMode.Battle
{
    // "Battle" hem bu ad alanının son parçası hem de sınanan tipin adı. Çıplak
    // yazıldığında C# önce ÜST ad alanının üyelerine bakar ve orada
    // GridStrategy.Tests.EditMode.Battle ad alanını bulur — sonuç CS0118, "ad
    // alanı tip gibi kullanıldı". Takma ad bu çakışmayı ad alanı gövdesinin
    // içinde, yani en dar kapsamda kırar. Aynı tuzağa BoardAdapter da düştü ve
    // aynı takma adı aynı gerekçeyle yazdı; orada gerekçenin uzun hâli var.
    using Battle = global::GridStrategy.Battle.Battle;

    // TurnState'in çakışan bir adı YOK; takma ad yine de yazıldı çünkü
    // alternatifi `using GridStrategy.Battle;` yazmaktı ve o satır, yukarıdaki
    // takma adın kırdığı çakışmayı dosyanın tepesine geri getirirdi —
    // GridStrategy.Battle hem ad alanı hem tip adı olduğu için, aynı ad iki
    // ayrı yoldan içeri girerdi. Tip başına bir takma ad, kapsamı en dar
    // tutuyor.
    using TurnState = global::GridStrategy.Battle.TurnState;

    /// <summary>
    /// Bu dosya parçaları test etmiyor — tahta <see cref="UnitGrid"/>'de, savaş
    /// durumu <see cref="Combatant"/>'ta zaten sınanıyor. Burada sınanan tek şey
    /// EŞLEŞME: birim, savaşçı ve hücre birlikte doğuyor mu, birlikte ölüyor mu,
    /// ve arada YARIM kalmış bir hâl mümkün mü.
    ///
    /// İki test bir davranışı değil bir KARARI koruyor:
    /// <list type="bullet">
    /// <item>aynı birim ikinci kez eklenince tahtaya hiçbir şey yazılmıyor
    /// (yazma sırası kararı)</item>
    /// <item>tahta dışı koordinatta sözlüğe hiçbir şey yazılmıyor (aynı kararın
    /// diğer yönü)</item>
    /// </list>
    /// İkisi de kırmızıya dönerse kod hâlâ "çalışıyor" görünür; kırılan şey
    /// "her kayıtlı savaşçının tahtada tam olarak bir hücresi vardır"
    /// değişmezidir.
    /// </summary>
    public sealed class BattleTests
    {
        private static Combatant NewCombatant(
            int maxHealth = 100,
            int damage = 10,
            int range = 1,
            Team team = Team.Player)
        {
            return new Combatant(
                new Health(maxHealth),
                new UnitLifecycle(downedWindowSeconds: 10f, corpseWindowSeconds: 5f),
                new AttackProfile(damage: damage, range: range),
                team);
        }

        private static Structure NewStructure(
            int maxHealth = 100,
            float rubbleWindowSeconds = 8f,
            Team team = Team.Player)
        {
            // Saldırı profili YOK: bu dosyanın sınadığı hiçbir şey yapının
            // saldırısıyla ilgili değil ve isteğe bağlı parametreyi doldurmak,
            // Structure.cs'te "kural olan davranış saldırmazdır" diye yazılmış
            // kararı testte tersine çevirirdi.
            return new Structure(
                new Health(maxHealth),
                new StructureLifecycle(rubbleWindowSeconds),
                team);
        }

        // Olayları sırasıyla biriktiren en küçük kap. Üç paralel liste yerine
        // tek bir tip: hangi çağrının hangi sıraya ait olduğu böylece indeksle
        // değil, kabın kendisiyle korunuyor.
        private sealed class StateChangeLog
        {
            public readonly List<Unit> Units = new List<Unit>();
            public readonly List<UnitState> From = new List<UnitState>();
            public readonly List<UnitState> To = new List<UnitState>();

            public int Count => Units.Count;

            public void Record(Unit unit, UnitState from, UnitState to)
            {
                Units.Add(unit);
                From.Add(from);
                To.Add(to);
            }
        }

        [Test]
        public void Constructor_BuildsABoardOfTheGivenSize()
        {
            var battle = new Battle(3, 5);

            Assert.That(battle.Width, Is.EqualTo(3));
            Assert.That(battle.Height, Is.EqualTo(5));
            Assert.That(battle.UnitCount, Is.EqualTo(0));
        }

        [TestCase(0, 5)]
        [TestCase(3, 0)]
        [TestCase(-1, 5)]
        public void Constructor_NonPositiveSize_Throws(int width, int height)
        {
            // Doğrulama UnitGrid'e ait; bu test onun Battle'dan da görünür
            // olduğunu, yani ölçü kuralının burada KOPYALANMADIĞINI koruyor.
            Assert.Throws<ArgumentOutOfRangeException>(() => new Battle(width, height));
        }

        [Test]
        public void AddUnit_PlacesTheUnitOnTheBoardAndRegistersItsCombatant()
        {
            var battle = new Battle(3, 5);
            var unit = new Unit("Vanguard");
            Combatant combatant = NewCombatant();

            battle.AddUnit(unit, combatant, 1, 2);

            Assert.That(battle.TryGetUnit(1, 2, out Unit standing), Is.True);
            Assert.That(standing, Is.SameAs(unit));
            Assert.That(battle.TryGetPosition(unit, out int x, out int y), Is.True);
            Assert.That(x, Is.EqualTo(1));
            Assert.That(y, Is.EqualTo(2));
            Assert.That(battle.UnitCount, Is.EqualTo(1));
        }

        [Test]
        public void TryGetCombatant_ReturnsTheVeryInstanceThatWasAdded()
        {
            var battle = new Battle(3, 5);
            var unit = new Unit("Vanguard");
            Combatant combatant = NewCombatant();
            battle.AddUnit(unit, combatant, 0, 0);

            Assert.That(battle.TryGetCombatant(unit, out Combatant found), Is.True);
            // SameAs, EqualTo değil: Battle savaşçıyı kopyalamaz, sahiplenir.
            // Kopyalasaydı verilen hasar çağıranın elindeki nesneye hiç
            // ulaşmazdı.
            Assert.That(found, Is.SameAs(combatant));
        }

        /// <summary>
        /// YAZMA SIRASI KARARINI koruyan test. Aynı birim ikinci kez, başka bir
        /// hücreye ekleniyor. Ön kontrol kaldırılırsa PlaceUnit yeni hücreye
        /// ÇOKTAN yazmış olur ve ardından Dictionary.Add patlar: aynı birim
        /// tahtada iki hücrede birden durur.
        ///
        /// Kırmızıya dönerse Battle'ın tek değişmezi ("her savaşçının tam olarak
        /// bir hücresi vardır") delinmiş demektir — ve o delik hiçbir başka
        /// testi kırmaz.
        /// </summary>
        [Test]
        public void AddUnit_SameUnitTwice_ThrowsAndLeavesTheFirstCellUntouched()
        {
            var battle = new Battle(3, 5);
            var unit = new Unit("Vanguard");
            battle.AddUnit(unit, NewCombatant(), 1, 2);

            Assert.Throws<ArgumentException>(() => battle.AddUnit(unit, NewCombatant(), 0, 0));

            Assert.That(battle.TryGetUnit(1, 2, out Unit standing), Is.True,
                "the original cell must still hold the unit");
            Assert.That(standing, Is.SameAs(unit));
            Assert.That(battle.TryGetUnit(0, 0, out Unit _), Is.False,
                "a rejected add must not have written the unit to a second cell");
            Assert.That(battle.UnitCount, Is.EqualTo(1));
        }

        /// <summary>
        /// AYNI KARARIN DİĞER YÖNÜ. Bu kez tahta reddediyor: koordinat dışarıda,
        /// PlaceUnit patlıyor. Kayıt PlaceUnit'ten ÖNCE yazılsaydı sözlükte
        /// hücresi olmayan bir savaşçı kalırdı — TryGetPosition ona false der,
        /// BattleActions ise "bu savaşta değil" diye patlar, oysa UnitCount 1
        /// gösterir.
        /// </summary>
        [Test]
        public void AddUnit_OutsideTheGrid_ThrowsAndRegistersNothing()
        {
            var battle = new Battle(3, 5);
            var unit = new Unit("Vanguard");

            Assert.Throws<ArgumentOutOfRangeException>(
                () => battle.AddUnit(unit, NewCombatant(), 9, 9));

            Assert.That(battle.UnitCount, Is.EqualTo(0),
                "a rejected add must not leave a combatant without a cell");
            Assert.That(battle.TryGetCombatant(unit, out Combatant _), Is.False);
        }

        [Test]
        public void AddUnit_OccupiedCell_ThrowsAndKeepsTheStandingUnit()
        {
            var battle = new Battle(3, 5);
            var standing = new Unit("Vanguard");
            battle.AddUnit(standing, NewCombatant(), 1, 2);
            var newcomer = new Unit("Runner");

            Assert.Throws<ArgumentException>(() => battle.AddUnit(newcomer, NewCombatant(), 1, 2));

            Assert.That(battle.TryGetUnit(1, 2, out Unit found), Is.True);
            Assert.That(found, Is.SameAs(standing),
                "overwriting the cell would leave the evicted combatant registered without a cell");
            Assert.That(battle.UnitCount, Is.EqualTo(1));
        }

        [Test]
        public void AddUnit_NullUnit_Throws()
        {
            var battle = new Battle(3, 5);

            Assert.Throws<ArgumentNullException>(() => battle.AddUnit(null, NewCombatant(), 0, 0));
        }

        [Test]
        public void AddUnit_NullCombatant_Throws()
        {
            var battle = new Battle(3, 5);

            Assert.Throws<ArgumentNullException>(() => battle.AddUnit(new Unit("Vanguard"), null, 0, 0));
        }

        [Test]
        public void RemoveUnit_ClearsTheCellAndTheRegistration()
        {
            var battle = new Battle(3, 5);
            var unit = new Unit("Vanguard");
            battle.AddUnit(unit, NewCombatant(), 1, 2);

            Assert.That(battle.RemoveUnit(unit), Is.True);

            Assert.That(battle.TryGetUnit(1, 2, out Unit _), Is.False);
            Assert.That(battle.TryGetCombatant(unit, out Combatant _), Is.False);
            Assert.That(battle.TryGetPosition(unit, out int _, out int _), Is.False);
            Assert.That(battle.UnitCount, Is.EqualTo(0));
        }

        [Test]
        public void RemoveUnit_LeavesTheOtherUnitsWhereTheyStand()
        {
            var battle = new Battle(3, 5);
            var leaving = new Unit("Vanguard");
            var staying = new Unit("Runner");
            battle.AddUnit(leaving, NewCombatant(), 0, 0);
            battle.AddUnit(staying, NewCombatant(), 2, 4);

            battle.RemoveUnit(leaving);

            Assert.That(battle.TryGetUnit(2, 4, out Unit found), Is.True);
            Assert.That(found, Is.SameAs(staying));
            Assert.That(battle.UnitCount, Is.EqualTo(1));
        }

        [Test]
        public void RemoveUnit_UnknownUnit_ReturnsFalse()
        {
            var battle = new Battle(3, 5);

            // Sessiz false, exception değil: temizlik aynı birim için iki kez
            // çalışabilir ve ikincisinin hiçbir şey yapmaması doğru davranıştır.
            Assert.That(battle.RemoveUnit(new Unit("Ghost")), Is.False);
        }

        [Test]
        public void RemoveUnit_NullUnit_Throws()
        {
            var battle = new Battle(3, 5);

            Assert.Throws<ArgumentNullException>(() => battle.RemoveUnit(null));
        }

        [Test]
        public void AddUnit_AfterRemove_TheSameUnitInstanceCanRejoinElsewhere()
        {
            var battle = new Battle(3, 5);
            var unit = new Unit("Vanguard");
            battle.AddUnit(unit, NewCombatant(), 1, 2);
            battle.RemoveUnit(unit);

            battle.AddUnit(unit, NewCombatant(), 0, 4);

            Assert.That(battle.TryGetPosition(unit, out int x, out int y), Is.True);
            Assert.That(x, Is.EqualTo(0));
            Assert.That(y, Is.EqualTo(4));
            Assert.That(battle.UnitCount, Is.EqualTo(1));
        }

        [Test]
        public void TryGetPosition_UnknownUnit_ReturnsFalseAndAnInvalidCell()
        {
            var battle = new Battle(3, 5);

            Assert.That(battle.TryGetPosition(new Unit("Ghost"), out int x, out int y), Is.False);
            // (0,0) DEĞİL: sıfır geçerli bir hücredir ve dönüşü yok sayan bir
            // çağıran onu sessizce köşe sanardı.
            Assert.That(x, Is.EqualTo(-1));
            Assert.That(y, Is.EqualTo(-1));
        }

        [Test]
        public void TryGetUnit_EmptyOrOutsideCell_ReturnsFalse()
        {
            var battle = new Battle(3, 5);

            Assert.That(battle.TryGetUnit(0, 0, out Unit _), Is.False, "empty cell");
            Assert.That(battle.TryGetUnit(9, 9, out Unit _), Is.False, "outside the board");
        }

        [Test]
        public void TryGetCombatant_UnknownUnit_ReturnsFalse()
        {
            var battle = new Battle(3, 5);

            Assert.That(battle.TryGetCombatant(new Unit("Ghost"), out Combatant found), Is.False);
            Assert.That(found, Is.Null);
        }

        // ═══ ÇAĞIRANI OLDUĞU İÇİN YAZILAN ÜYELER ═════════════════════
        // Dördü de (CellCount, IsInsideGrid, Tick, RemoveReadyForCleanup) bir
        // ÇAĞIRAN doğduğu için yazıldı: BoardAdapter kendi UnitGrid alanını
        // bıraktı ve savaşın saatini çevirmek zorunda kaldı. O çağıran bir
        // MonoBehaviour ve Unity'siz sınanamıyor — ama sorduğu SORULAR düz C#.
        // Aşağıdaki testler tam olarak o yarıyı tutuyor.

        [Test]
        public void CellCount_MatchesTheBoardSize()
        {
            Assert.That(new Battle(3, 5).CellCount, Is.EqualTo(15));
        }

        // DÜRÜST NOT: bu testler bir SAHİPLİĞİ koruyamaz. Battle'a
        // `x >= 0 && x < Width` kopyalansaydı hepsi yeşil kalırdı; kopyanın
        // bedeli ancak UnitGrid'in cevabı değiştiği gün ödenir ve o gün bu
        // dosya sessiz kalır. Koruduğu şey daha dar ve yine de değerli:
        // devretmenin sınırları doğru çeviriyor olması — özellikle üst sınırın
        // dışlayıcı (exclusive) olduğu.
        [TestCase(0, 0, true)]
        [TestCase(2, 4, true, Description = "the far corner is still inside")]
        [TestCase(3, 4, false, Description = "width is exclusive")]
        [TestCase(2, 5, false, Description = "height is exclusive")]
        [TestCase(-1, 0, false)]
        [TestCase(0, -1, false)]
        public void IsInsideGrid_AnswersFromTheBoardsOwnSize(int x, int y, bool expected)
        {
            Assert.That(new Battle(3, 5).IsInsideGrid(x, y), Is.EqualTo(expected));
        }

        [Test]
        public void Tick_AdvancesTheLifecycleOfEveryCombatant()
        {
            var battle = new Battle(3, 5);
            Combatant first = NewCombatant(maxHealth: 10);
            Combatant second = NewCombatant(maxHealth: 10);
            battle.AddUnit(new Unit("First"), first, 0, 0);
            battle.AddUnit(new Unit("Second"), second, 1, 1);

            first.TakeDamage(10);
            second.TakeDamage(10);
            Assert.That(first.State, Is.EqualTo(UnitState.Downed), "setup");
            Assert.That(second.State, Is.EqualTo(UnitState.Downed), "setup");

            // TEK çağrı İKİ savaşçıyı birden ilerletmeli. Biri geride kalırsa
            // "toplu ilerletme" iddiası boştur ve BoardAdapter yalnızca ilk
            // birimin öldüğünü görürdü.
            battle.Tick(10.1f);

            Assert.That(first.State, Is.EqualTo(UnitState.Dead));
            Assert.That(second.State, Is.EqualTo(UnitState.Dead));
        }

        [Test]
        public void Tick_DoesNotAdvanceAUnitThatLeftTheBattle()
        {
            var battle = new Battle(3, 5);
            var unit = new Unit("Vanguard");
            Combatant combatant = NewCombatant(maxHealth: 10);
            battle.AddUnit(unit, combatant, 0, 0);
            combatant.TakeDamage(10);

            battle.RemoveUnit(unit);
            battle.Tick(10.1f);

            // Kayıt silindiği anda saat de durur: savaştan çıkmış bir savaşçının
            // geri sayımını sürdürmek bu tipin işi değil. Sözlük yerine ayrı bir
            // liste tutulsaydı ve silme oradan atlanmış olsaydı, çıkarılmış
            // birim arka planda ölmeye devam ederdi.
            Assert.That(combatant.State, Is.EqualTo(UnitState.Downed));
        }

        /// <summary>
        /// İKİ GEÇİŞ KARARINI koruyan test — ve kritik olan iki cesedin birden
        /// hazır olması. Süpürme tek geçişte yazılsaydı (sözlük üzerinde
        /// dönerken silmek) tek cesetle çalışır GÖRÜNÜR, ikinci cesette
        /// InvalidOperationException fırlatırdı.
        /// </summary>
        [Test]
        public void RemoveReadyForCleanup_SweepsEveryReadyUnitInOnePass()
        {
            var battle = new Battle(3, 5);
            var first = new Unit("First");
            var second = new Unit("Second");
            Combatant firstCombatant = NewCombatant(maxHealth: 10);
            Combatant secondCombatant = NewCombatant(maxHealth: 10);
            battle.AddUnit(first, firstCombatant, 0, 0);
            battle.AddUnit(second, secondCombatant, 1, 1);

            firstCombatant.TakeDamage(10);
            secondCombatant.TakeDamage(10);
            battle.Tick(10.1f);
            battle.Tick(5.1f);

            var removed = new List<Unit>();
            Assert.That(battle.RemoveReadyForCleanup(removed), Is.EqualTo(2));
            Assert.That(removed, Is.EquivalentTo(new[] { first, second }));
            Assert.That(battle.UnitCount, Is.EqualTo(0));
        }

        [Test]
        public void RemoveReadyForCleanup_LeavesUnitsWhoseCorpseTimerIsStillRunning()
        {
            var battle = new Battle(3, 5);
            var fallen = new Unit("Fallen");
            var standing = new Unit("Standing");
            Combatant fallenCombatant = NewCombatant(maxHealth: 10);
            battle.AddUnit(fallen, fallenCombatant, 0, 0);
            battle.AddUnit(standing, NewCombatant(), 1, 1);

            fallenCombatant.TakeDamage(10);
            battle.Tick(10.1f);   // Downed -> Dead; ceset sayacı YENİ başladı

            var removed = new List<Unit>();
            Assert.That(battle.RemoveReadyForCleanup(removed), Is.EqualTo(0),
                "a fresh corpse is not ready yet");
            Assert.That(battle.UnitCount, Is.EqualTo(2));

            battle.Tick(5.1f);

            Assert.That(battle.RemoveReadyForCleanup(removed), Is.EqualTo(1));
            Assert.That(removed, Is.EquivalentTo(new[] { fallen }));
            Assert.That(battle.TryGetUnit(1, 1, out Unit _), Is.True,
                "the living unit must stay on the board");
        }

        [Test]
        public void RemoveReadyForCleanup_FreesTheCellForANewUnit()
        {
            var battle = new Battle(3, 5);
            var fallen = new Unit("Fallen");
            Combatant fallenCombatant = NewCombatant(maxHealth: 10);
            battle.AddUnit(fallen, fallenCombatant, 2, 4);

            fallenCombatant.TakeDamage(10);
            battle.Tick(10.1f);
            battle.Tick(5.1f);
            battle.RemoveReadyForCleanup(new List<Unit>());

            // Hücrenin gerçekten boşaldığının en dürüst kanıtı: AddUnit dolu
            // hücreyi reddediyor, dolayısıyla bu çağrının patlamaması hücrenin
            // boş olduğunu tahtanın kendi ağzından söyletir.
            Assert.That(battle.TryGetUnit(2, 4, out Unit _), Is.False);
            Assert.DoesNotThrow(() => battle.AddUnit(new Unit("Fresh"), NewCombatant(), 2, 4));
        }

        [Test]
        public void RemoveReadyForCleanup_ClearsTheBufferBeforeFillingIt()
        {
            var battle = new Battle(3, 5);
            var removed = new List<Unit> { new Unit("Leftover") };

            Assert.That(battle.RemoveReadyForCleanup(removed), Is.EqualTo(0));
            // Tampon bir ÇIKIŞ kabıdır, üstüne eklenen bir liste değil.
            // Temizlenmeseydi çağıran önceki karenin cesedini ikinci kez
            // sahneden silmeye çalışır ve LogError yazardı.
            Assert.That(removed, Is.Empty);
        }

        [Test]
        public void RemoveReadyForCleanup_NullBuffer_Throws()
        {
            var battle = new Battle(3, 5);

            Assert.Throws<ArgumentNullException>(() => battle.RemoveReadyForCleanup(null));
        }

        // ═══ YAPILAR TAHTAYA BAĞLANIYOR ══════════════════════════════
        // Bu bölümün sınadığı tek iddia şu: bir baraka ile bir asker AYNI
        // tahtada, aynı hücre kurallarıyla, aynı kimlik tipiyle yaşıyor.
        // Ayrışan tek şey o kimliğe hangi savaş parçasının eşlendiği.

        [Test]
        public void AddStructure_PlacesTheStructureOnTheBoardAndRegistersIt()
        {
            var battle = new Battle(3, 5);
            var depot = new Unit("Depot");
            Structure structure = NewStructure();

            battle.AddStructure(depot, structure, 1, 2);

            Assert.That(battle.TryGetUnit(1, 2, out Unit standing), Is.True);
            Assert.That(standing, Is.SameAs(depot));
            Assert.That(battle.TryGetStructure(depot, out Structure found), Is.True);
            Assert.That(found, Is.SameAs(structure));
            Assert.That(battle.StructureCount, Is.EqualTo(1));
        }

        /// <summary>
        /// TEK TAHTA KARARINI koruyan test — ve bu dosyadaki en pahalı satır.
        /// Bir asker ile bir baraka aynı hücreye giremez.
        ///
        /// Yapılara kendi <c>UnitGrid</c>'i verilseydi bu çağrı SESSİZCE
        /// başarılı olurdu: iki tahta birbirinin doluluğunu görmez, hiçbir
        /// derleme hatası çıkmaz ve tahtada aynı hücrede iki şey durur.
        /// Kırmızıya dönmesi "iki tahta" kırılmasının geri geldiğinin tek
        /// işaretidir.
        /// </summary>
        [Test]
        public void AddStructure_OnACellHeldByAUnit_ThrowsAndKeepsTheStandingUnit()
        {
            var battle = new Battle(3, 5);
            var soldier = new Unit("Vanguard");
            battle.AddUnit(soldier, NewCombatant(), 1, 2);

            Assert.Throws<ArgumentException>(
                () => battle.AddStructure(new Unit("Depot"), NewStructure(), 1, 2));

            Assert.That(battle.TryGetUnit(1, 2, out Unit standing), Is.True);
            Assert.That(standing, Is.SameAs(soldier),
                "a structure must not overwrite the unit standing on that cell");
            Assert.That(battle.StructureCount, Is.Zero,
                "a rejected add must not leave a structure without a cell");
        }

        /// <summary>
        /// AYNI KARARIN DİĞER YÖNÜ. Bu kez hücreyi tutan taraf yapıdır.
        /// İki yön ayrı ayrı yazılıyor çünkü iki tahtalı bir uygulamada
        /// yalnızca birini sormak (en olası yarım uygulama) diğerini sessizce
        /// açık bırakır.
        /// </summary>
        [Test]
        public void AddUnit_OnACellHeldByAStructure_ThrowsAndKeepsTheStandingStructure()
        {
            var battle = new Battle(3, 5);
            var depot = new Unit("Depot");
            battle.AddStructure(depot, NewStructure(), 1, 2);

            Assert.Throws<ArgumentException>(
                () => battle.AddUnit(new Unit("Vanguard"), NewCombatant(), 1, 2));

            Assert.That(battle.TryGetUnit(1, 2, out Unit standing), Is.True);
            Assert.That(standing, Is.SameAs(depot));
            Assert.That(battle.UnitCount, Is.Zero);
        }

        /// <summary>
        /// TEK KİMLİK, TEK KAYIT kararını koruyan test. Aynı
        /// <see cref="Unit"/> hem savaşçı hem yapı olamaz.
        ///
        /// İzin verilseydi <c>Tick</c> aynı kimliği iki kez işletir ve
        /// <c>RemoveReadyForCleanup</c> onu çıkış tamponuna iki kez yazardı;
        /// çağıran aynı görseli iki kez silmeye çalışırdı. Hiçbiri patlamaz —
        /// yalnızca yanlıştır.
        /// </summary>
        [Test]
        public void AddStructure_WithAnIdentityThatIsAlreadyACombatant_IsRejected()
        {
            var battle = new Battle(3, 5);
            var identity = new Unit("Vanguard");
            battle.AddUnit(identity, NewCombatant(), 0, 0);

            Assert.Throws<ArgumentException>(
                () => battle.AddStructure(identity, NewStructure(), 2, 4));

            Assert.That(battle.StructureCount, Is.Zero);
            Assert.That(battle.TryGetUnit(2, 4, out Unit _), Is.False,
                "a rejected add must not have written the identity to a second cell");
        }

        [Test]
        public void AddUnit_WithAnIdentityThatIsAlreadyAStructure_IsRejected()
        {
            var battle = new Battle(3, 5);
            var identity = new Unit("Depot");
            battle.AddStructure(identity, NewStructure(), 0, 0);

            Assert.Throws<ArgumentException>(
                () => battle.AddUnit(identity, NewCombatant(), 2, 4));

            Assert.That(battle.UnitCount, Is.Zero);
            Assert.That(battle.TryGetUnit(2, 4, out Unit _), Is.False);
        }

        [Test]
        public void AddStructure_OutsideTheGrid_ThrowsAndRegistersNothing()
        {
            var battle = new Battle(3, 5);
            var depot = new Unit("Depot");

            Assert.Throws<ArgumentOutOfRangeException>(
                () => battle.AddStructure(depot, NewStructure(), 9, 9));

            Assert.That(battle.StructureCount, Is.Zero);
            Assert.That(battle.TryGetStructure(depot, out Structure _), Is.False);
        }

        [Test]
        public void AddStructure_NullArgument_Throws()
        {
            var battle = new Battle(3, 5);

            Assert.Throws<ArgumentNullException>(
                () => battle.AddStructure(null, NewStructure(), 0, 0));
            Assert.Throws<ArgumentNullException>(
                () => battle.AddStructure(new Unit("Depot"), null, 0, 0));
        }

        /// <summary>
        /// İki sorgunun BİRLİKTE okunmasını sabitleyen test: bir kimlik için
        /// <c>TryGetCombatant</c> ile <c>TryGetStructure</c>'dan tam olarak biri
        /// true döner. Bu ayrım kaybolursa "bu kimlik ne" sorusunun cevabı
        /// belirsizleşir ve çağıran yanlış parçayı okur.
        /// </summary>
        [Test]
        public void TryGetStructure_AndTryGetCombatant_AnswerForExactlyOneIdentityKind()
        {
            var battle = new Battle(3, 5);
            var soldier = new Unit("Vanguard");
            var depot = new Unit("Depot");
            battle.AddUnit(soldier, NewCombatant(), 0, 0);
            battle.AddStructure(depot, NewStructure(), 1, 1);

            Assert.That(battle.TryGetCombatant(soldier, out Combatant _), Is.True);
            Assert.That(battle.TryGetStructure(soldier, out Structure _), Is.False);
            Assert.That(battle.TryGetStructure(depot, out Structure _), Is.True);
            Assert.That(battle.TryGetCombatant(depot, out Combatant _), Is.False);
        }

        [Test]
        public void TryGetStructure_UnknownUnit_ReturnsFalse()
        {
            var battle = new Battle(3, 5);

            Assert.That(battle.TryGetStructure(new Unit("Ghost"), out Structure found), Is.False);
            Assert.That(found, Is.Null);
        }

        [Test]
        public void TryGetStructure_NullUnit_Throws()
        {
            var battle = new Battle(3, 5);

            Assert.Throws<ArgumentNullException>(() => battle.TryGetStructure(null, out Structure _));
        }

        [Test]
        public void TryGetPosition_FindsAStructureToo_BecauseItStandsOnTheSameBoard()
        {
            var battle = new Battle(3, 5);
            var depot = new Unit("Depot");
            battle.AddStructure(depot, NewStructure(), 2, 4);

            Assert.That(battle.TryGetPosition(depot, out int x, out int y), Is.True);
            Assert.That(x, Is.EqualTo(2));
            Assert.That(y, Is.EqualTo(4));
        }

        /// <summary>
        /// YAPILARIN DA SAATİ ÇEVRİLİYOR kararını koruyan test. Tek bir
        /// <c>Tick</c> çağrısı hem ceset hem enkaz sayacını ilerletmeli.
        ///
        /// Yapı döngüsü kaldırılırsa enkaz sayacı hiç işlemez ve yıkık bina
        /// tahtada SONSUZA DEK kalır. Hiçbir şey patlamaz — kimse "enkaz neden
        /// hâlâ duruyor" diye hata açmaz; bu test o sessizliğin tek panzehiri.
        /// </summary>
        [Test]
        public void Tick_AdvancesStructureLifecyclesAsWellAsCombatants()
        {
            var battle = new Battle(3, 5);
            var soldier = new Unit("Vanguard");
            var depot = new Unit("Depot");
            Combatant combatant = NewCombatant(maxHealth: 10);
            Structure structure = NewStructure(maxHealth: 10, rubbleWindowSeconds: 8f);
            battle.AddUnit(soldier, combatant, 0, 0);
            battle.AddStructure(depot, structure, 1, 1);

            combatant.TakeDamage(10);
            structure.TakeDamage(10);
            Assert.That(structure.State, Is.EqualTo(StructureState.Destroyed), "setup");

            // 10,1 saniye ikisini birden aşıyor: ceset penceresi 10, enkaz
            // penceresi 8. Tek çağrı iki sayacı da bitirmeli.
            battle.Tick(10.1f);

            Assert.That(structure.IsReadyForCleanup, Is.True,
                "the rubble timer must run inside the same Tick that runs corpse timers");
            Assert.That(combatant.State, Is.EqualTo(UnitState.Dead));
        }

        /// <summary>
        /// TEK SÜPÜRME kararını koruyan test: ceset ile enkaz aynı geçişte,
        /// aynı çıkış tamponuna yazılır. Ayrı bir sweep metodu doğsaydı çağıran
        /// ikisini de çağırmakla yükümlü olurdu.
        /// </summary>
        [Test]
        public void RemoveReadyForCleanup_SweepsRubbleAndCorpsesInTheSamePass()
        {
            var battle = new Battle(3, 5);
            var soldier = new Unit("Vanguard");
            var depot = new Unit("Depot");
            Combatant combatant = NewCombatant(maxHealth: 10);
            Structure structure = NewStructure(maxHealth: 10, rubbleWindowSeconds: 8f);
            battle.AddUnit(soldier, combatant, 0, 0);
            battle.AddStructure(depot, structure, 1, 1);

            combatant.TakeDamage(10);
            structure.TakeDamage(10);
            battle.Tick(10.1f);   // Downed -> Dead; enkaz sayacı 8'den 10.1 düştü
            battle.Tick(5.1f);    // ceset süresi doldu

            var removed = new List<Unit>();
            Assert.That(battle.RemoveReadyForCleanup(removed), Is.EqualTo(2));
            Assert.That(removed, Is.EquivalentTo(new[] { soldier, depot }));
            Assert.That(battle.UnitCount, Is.Zero);
            Assert.That(battle.StructureCount, Is.Zero);
            Assert.That(battle.TryGetUnit(1, 1, out Unit _), Is.False,
                "the rubble cell must be free again");
        }

        [Test]
        public void RemoveUnit_RemovesAStructureAndFreesItsCell()
        {
            var battle = new Battle(3, 5);
            var depot = new Unit("Depot");
            battle.AddStructure(depot, NewStructure(), 1, 2);

            Assert.That(battle.RemoveUnit(depot), Is.True);

            Assert.That(battle.TryGetUnit(1, 2, out Unit _), Is.False);
            Assert.That(battle.TryGetStructure(depot, out Structure _), Is.False);
            Assert.That(battle.StructureCount, Is.Zero);
            Assert.DoesNotThrow(() => battle.AddUnit(new Unit("Fresh"), NewCombatant(), 1, 2));
        }

        // ═══ SIRA DURUMUNUN SAHİBİ SAVAŞ ═════════════════════════════

        [Test]
        public void Turn_IsBuiltWithTheBattleAndStartsWithTheDefaultOrder()
        {
            var battle = new Battle(3, 5);

            Assert.That(battle.Turn, Is.Not.Null,
                "a battle without a turn state cannot answer whose turn it is");
            Assert.That(battle.Turn.Current, Is.EqualTo(Team.Player));
            Assert.That(battle.Turn.TurnNumber, Is.EqualTo(TurnState.FirstTurnNumber));
        }

        /// <summary>
        /// SIRA DURUMUNUN ÖRNEK BAŞINA OLDUĞUNU koruyan test.
        ///
        /// <c>BattleActions</c> içinde <c>static</c> bir <c>TurnState</c>
        /// tutulsaydı bu test kırmızıya dönerdi — ve kırılan şey yalnızca iki
        /// savaş değil, testlerin YALITIMIdır: NUnit aynı süreçte koştuğu için
        /// static durum test metotları arasında sızar ve testler koşma sırasına
        /// göre geçmeye başlar.
        /// </summary>
        [Test]
        public void Turn_IsPerBattle_NeverSharedBetweenTwoBattles()
        {
            var first = new Battle(3, 5);
            var second = new Battle(3, 5);

            first.Turn.EndTurn();

            Assert.That(first.Turn.Current, Is.EqualTo(Team.Enemy));
            Assert.That(second.Turn.Current, Is.EqualTo(Team.Player),
                "ending a turn in one battle must not move the other battle's turn");
            Assert.That(second.Turn, Is.Not.SameAs(first.Turn));
        }

        // ═══ KİMLİK İLİŞKİLENDİRİLMİŞ DURUM DEĞİŞİKLİĞİ ══════════════
        // Zincir: UnitLifecycle (hangi duruma) -> Combatant (nereden nereye)
        // -> Battle (KİM, nereden nereye). Kimliği ekleyen halka burasıdır
        // çünkü Combatant kendi Unit'ini bilmez.

        /// <summary>
        /// ZİNCİRİN TAMAMINI koruyan test: kimlik, kaynak durum ve hedef durum
        /// birlikte ulaşmalı ve geçişler SIRAYLA gelmeli.
        ///
        /// Üçüncü <c>Tick</c> yalnızca <c>IsReadyForCleanup</c>'ı açar ve
        /// hiçbir durumu değiştirmez — olay tetiklenmemeli. "Bir şey oldu" ile
        /// "durum değişti" aynı şey değildir; toplu süpürmenin bu olayın yerine
        /// geçmemesinin sebebi tam olarak budur.
        /// </summary>
        [Test]
        public void UnitStateChanged_CarriesTheIdentityAndFiresOncePerRealTransition()
        {
            var battle = new Battle(3, 5);
            var soldier = new Unit("Vanguard");
            var bystander = new Unit("Runner");
            Combatant combatant = NewCombatant(maxHealth: 10);
            battle.AddUnit(soldier, combatant, 0, 0);
            battle.AddUnit(bystander, NewCombatant(), 1, 1);

            var log = new StateChangeLog();
            battle.UnitStateChanged += log.Record;

            combatant.TakeDamage(10);   // Alive -> Downed
            battle.Tick(10.1f);         // Downed -> Dead
            battle.Tick(5.1f);          // yalnızca temizlik bayrağı; geçiş YOK

            Assert.That(log.Count, Is.EqualTo(2),
                "the cleanup flag is not a state transition");
            Assert.That(log.Units[0], Is.SameAs(soldier),
                "the listener must learn WHICH unit changed");
            Assert.That(log.Units[1], Is.SameAs(soldier));
            Assert.That(log.From[0], Is.EqualTo(UnitState.Alive));
            Assert.That(log.To[0], Is.EqualTo(UnitState.Downed));
            Assert.That(log.From[1], Is.EqualTo(UnitState.Downed));
            Assert.That(log.To[1], Is.EqualTo(UnitState.Dead));
        }

        /// <summary>
        /// ABONELİĞİ BIRAKMA KARARINI koruyan test — bu dosyadaki tek sızıntı
        /// testi.
        ///
        /// Birim savaştan çıkarıldıktan SONRA, çağıranın elinde kalan
        /// <see cref="Combatant"/> üzerinden çevrilen bir <c>Tick</c> hâlâ
        /// gerçek bir durum geçişi üretir. <c>RemoveUnit</c> aboneliği
        /// bırakmazsa o geçiş savaşta OLMAYAN bir kimlikle dinleyiciye ulaşır;
        /// dinleyici kimliği tablosunda arar, bulamaz ve ya sessizce hiçbir şey
        /// yapar ya da null'a dokunur.
        ///
        /// <c>battle.Tick</c> ile sınamak YETMEZ: kayıt silindiği için o yol
        /// zaten savaşçıya uğramaz ve abonelik bırakılmasa bile test yeşil
        /// kalırdı. Bu yüzden saat DOĞRUDAN savaşçıya çevriliyor.
        /// </summary>
        [Test]
        public void UnitStateChanged_StopsReachingListenersAfterTheUnitLeavesTheBattle()
        {
            var battle = new Battle(3, 5);
            var soldier = new Unit("Vanguard");
            Combatant combatant = NewCombatant(maxHealth: 10);
            battle.AddUnit(soldier, combatant, 0, 0);

            var log = new StateChangeLog();
            battle.UnitStateChanged += log.Record;

            combatant.TakeDamage(10);   // Alive -> Downed, savaştayken duyulmalı
            Assert.That(log.Count, Is.EqualTo(1), "setup");

            battle.RemoveUnit(soldier);
            combatant.Tick(10.1f);      // Downed -> Dead, artık duyulmamalı

            Assert.That(log.Count, Is.EqualTo(1),
                "a removed unit must not keep reporting through the battle it left");
        }

        /// <summary>
        /// Reddedilen bir eklemenin geriye ABONELİK bırakmadığını koruyan test.
        /// Abonelik yazma işlemlerinden önce kurulsaydı, tahta dışı koordinatta
        /// patlayan bir ekleme savaşa hiç girmemiş bir savaşçıyı olay zincirine
        /// bağlamış olurdu.
        /// </summary>
        [Test]
        public void UnitStateChanged_IsNotWiredByARejectedAdd()
        {
            var battle = new Battle(3, 5);
            var soldier = new Unit("Vanguard");
            Combatant combatant = NewCombatant(maxHealth: 10);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => battle.AddUnit(soldier, combatant, 9, 9));

            var log = new StateChangeLog();
            battle.UnitStateChanged += log.Record;
            combatant.TakeDamage(10);

            Assert.That(log.Count, Is.Zero,
                "a combatant that never joined must not reach the battle's listeners");
        }

        /// <summary>
        /// Yapıların olay zincirine KATILMADIĞINI sabitleyen test.
        /// <see cref="StructureLifecycle"/> bilerek olaysızdır; enkazı bulmanın
        /// tek yolu <c>RemoveReadyForCleanup</c>'tır. Bir gün yapı geçişleri de
        /// bu olaydan yayılırsa bu test kırmızıya döner ve karar, sessizce
        /// değişmek yerine yeniden tartışılır.
        /// </summary>
        [Test]
        public void UnitStateChanged_StaysSilentForStructures()
        {
            var battle = new Battle(3, 5);
            var depot = new Unit("Depot");
            Structure structure = NewStructure(maxHealth: 10, rubbleWindowSeconds: 8f);
            battle.AddStructure(depot, structure, 0, 0);

            var log = new StateChangeLog();
            battle.UnitStateChanged += log.Record;

            structure.TakeDamage(10);   // Standing -> Destroyed
            battle.Tick(8.1f);          // enkaz süresi doldu

            Assert.That(log.Count, Is.Zero,
                "structure transitions are answered by return values, not by this event");
        }

        /// <summary>
        /// KARARI KORUYAN test. Kelepçenin İKİNCİ yönü: aynı <see cref="Combatant"/>
        /// örneği ikinci bir kimliğe bağlanamaz.
        ///
        /// Kimlik yönü (aynı Unit iki kez) ayrı testlerde korunuyor. Bu ikisi
        /// AYNI kural değil ve yalnız birini yazmak, en olası yarım uygulamayı
        /// sessiz bırakır: iki kimliğe bağlı bir savaşçı StateChanged için İKİ
        /// forwarder taşır, tek geçiş iki kimlikle yayılır ve dinleyen taraf
        /// aynı ölümü iki kez görür. Hiçbir derleme hatası çıkmaz.
        /// </summary>
        [Test]
        public void AddUnit_WithACombatantAlreadyBoundToAnotherIdentity_IsRejected()
        {
            var battle = new Battle(3, 5);
            Combatant shared = NewCombatant();
            battle.AddUnit(new Unit("Vanguard"), shared, 0, 0);

            Assert.Throws<ArgumentException>(
                () => battle.AddUnit(new Unit("Impostor"), shared, 2, 4));

            Assert.That(battle.UnitCount, Is.EqualTo(1),
                "a combatant bound to one identity must not join a second time");
            Assert.That(battle.TryGetUnit(2, 4, out Unit _), Is.False,
                "the rejected add must leave the board untouched");
        }

        /// <summary>
        /// Yapı tarafındaki ikizi. Ayrı yazılıyor çünkü kelepçe ortak kapıya
        /// konamadı: <c>ThrowIfCannotJoin</c> yalnız <see cref="Unit"/> görür,
        /// parça iki farklı tiptir. Kural iki yerde yaşadığı sürece iki testle
        /// korunmak zorunda — biri silinirse o yön sessizce açılır.
        /// </summary>
        [Test]
        public void AddStructure_WithAStructureAlreadyBoundToAnotherIdentity_IsRejected()
        {
            var battle = new Battle(3, 5);
            Structure shared = NewStructure();
            battle.AddStructure(new Unit("Depot"), shared, 0, 0);

            Assert.Throws<ArgumentException>(
                () => battle.AddStructure(new Unit("Ghost"), shared, 2, 4));

            Assert.That(battle.StructureCount, Is.EqualTo(1),
                "a structure bound to one identity must not join a second time");
            Assert.That(battle.TryGetUnit(2, 4, out Unit _), Is.False,
                "the rejected add must leave the board untouched");
        }

    }
}
