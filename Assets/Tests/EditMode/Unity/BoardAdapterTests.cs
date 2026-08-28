using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using GridStrategy.Combat;
using GridStrategy.Core;
using GridStrategy.Unity;
using UnityEngine;
using UnityEngine.TestTools;

namespace GridStrategy.Tests.EditMode.Unity
{
    // Çakışmanın ve takma adın gerekçesi BoardAdapter.cs'in tepesinde uzun uzun
    // yazılı; burada tekrar edilmiyor, yalnızca uygulanıyor. Kısası: çıplak
    // "Battle" önce GridStrategy ad alanının üyelerinde aranır ve orada bir AD
    // ALANI bulunur — CS0118.
    using Battle = global::GridStrategy.Battle.Battle;

    /// <summary>
    /// <b>BU DOSYANIN YOKLUĞU İKİ KUSURU YILLARCA SAKLADI</b> ve ikisi de bir
    /// tıklamayla görülebilirdi:
    /// <list type="bullet">
    /// <item><c>CommitPlacement</c> yapıyı KOYAN birimin kimliğiyle tahtaya
    /// sokmaya çalışıyordu; o kimlik zaten kadroda kayıtlı olduğu için
    /// <c>Battle.AddStructure</c> her geçerli hücrede <c>ArgumentException</c>
    /// atıyordu — <c>PlacementOutcome.Placed</c> üretimde ERİŞİLEMEZDİ.</item>
    /// <item><c>CreateStructureVisual</c> bir GameObject doğuruyor ama hiçbir
    /// tabloya yazmıyordu; enkaz süresi dolunca temizlik görseli bulamıyor,
    /// LogError basıyor ve enkaz ekranda kalıyordu.</item>
    /// </list>
    ///
    /// <b>NEDEN YANSIMA (reflection) — ve neden bu bir taviz olduğu SAKLANMIYOR.</b>
    /// <see cref="BoardAdapter"/> bir <c>MonoBehaviour</c>'dır: <c>new</c> ile
    /// kurulamaz, <c>AddComponent</c> ile doğar ve EditMode'da <c>Awake</c> HİÇ
    /// koşmaz (script <c>[ExecuteAlways]</c> değil — aynı ölçü
    /// <see cref="UnitViewTests"/>'te de yazılı). Yani <c>battle</c>,
    /// <c>unityGrid</c> ve <c>placementGhost</c> alanları burada null doğar ve
    /// onları dolduran tek yol yansımadır. Bedeli açık: bu dosya ÖZEL üye
    /// ADLARINA bağlıdır ve bir alan yeniden adlandırıldığında derleyici değil
    /// bu testler kırmızıya döner — bu yüzden aşağıdaki iki yardımcı, adı
    /// bulamadığında sessizce geçmek yerine AÇIK bir iddiayla düşer.
    ///
    /// <b>EN KÜÇÜK DEĞİŞİKLİK BİLEREK YAPILMADI.</b> Yansımayı gereksiz kılacak
    /// en küçük adım ölçüldü ve raporlandı: yerleştirme kararını taşıyan
    /// çekirdeği (savaş + görsel tablosu + hücre) motora hiç dokunmayan ayrı bir
    /// tipe çıkarmak, <c>BoardAdapter</c>'ı ona bir tutamak bırakmak. O tip
    /// <c>new</c> ile kurulur ve bu dosyadaki her yansıma çağrısı silinir.
    /// Bugün yapılmadı çünkü davranış değiştirmeden önce davranışın SABİTLENMESİ
    /// gerekiyordu; bu dosya tam olarak o sabitlemedir.
    ///
    /// Girdi okuma yolları (<c>Update</c>, <c>HandleClick</c>,
    /// <c>UpdatePlacement</c>, <c>FeedGesture</c>) burada HİÇ sınanmıyor ve bu
    /// bir eksiklik değil bir sınır: <c>Input</c> ile <c>Camera.main</c>
    /// EditMode'da beslenemez. Sınanan şey, o yolların ULAŞTIĞI kararlardır.
    /// </summary>
    public sealed class BoardAdapterTests
    {
        private const int Width = 3;
        private const int Height = 5;
        private const int MaxHealth = 30;
        private const int Damage = 10;
        private const int AttackRange = 1;
        private const int StructureMaxHealth = 50;

        // Yerleştirenin hücresi. Sabit çünkü İKİ ayrı test onun KIMILDAMADIĞINI
        // iddia ediyor ve iddiayı sayıyla yazmak, hücreyi değiştiren günü sessiz
        // bir kırmızıya çevirirdi.
        private const int PlacerX = 1;
        private const int PlacerY = 2;

        private const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;

        private GameObject probe;
        private BoardAdapter adapter;
        private SpriteRenderer ghost;
        private Battle battle;
        private Unit placer;

        // Havuzun prefab'ı ve onun takım karesi. İkisi de yalnız InstallViewPool
        // çağıran testlerde doluyor; ötekiler onlara hiç dokunmuyor.
        private UnitView unitPrefab;
        private Sprite teamIdle;

        // Testin ürettiği geçici Unity nesneleri. Sprite ve Texture2D sahne
        // yıkılınca kendiliğinden gitmez; toplanmasalardı her koşuda sızarlardı.
        private readonly List<Object> disposables = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            probe = new GameObject("BoardProbe");

            // Grid AÇIKÇA ekleniyor, [RequireComponent]'in otomatik eklemesine
            // güvenilmiyor — aynı gerekçe UnitViewTests'te de yazılı: test,
            // sınanan tipin davranışını ölçmeli, Unity'nin kolaylığını değil.
            var grid = probe.AddComponent<Grid>();
            adapter = probe.AddComponent<BoardAdapter>();

            // Hayalet gerçek sahnedeki gibi bir ÇOCUK nesnede yaşıyor. Sprite'ı
            // null kalıyor ve bu bilerek: CreateStructureVisual sprite'ı oradan
            // KOPYALIYOR, yani null bir sprite hiçbir yolu değiştirmez ve testin
            // bir görsel varlığa ihtiyacı yok.
            var ghostObject = new GameObject("Ghost");
            ghostObject.transform.SetParent(probe.transform);
            ghost = ghostObject.AddComponent<SpriteRenderer>();

            battle = new Battle(Width, Height);
            placer = new Unit("Vanguard");
            battle.AddUnit(placer, NewCombatant(Team.Player), PlacerX, PlacerY);

            // AWAKE HİÇ KOŞMADIĞI İÇİN ALANLARI TEST DOLDURUYOR. Inspector
            // alanları da (structureMaxHealth) elle yazılıyor: alan
            // başlatıcısının AddComponent sırasında koşmasına GÜVENMEK, testi
            // ölçmediği bir mekanizmaya bağlardı.
            SetField("unityGrid", grid);
            SetField("battle", battle);
            SetField("placementGhost", ghost);
            SetField("selectedUnit", placer);
            SetField("structureMaxHealth", StructureMaxHealth);
        }

        [TearDown]
        public void TearDown()
        {
            // DestroyImmediate, Destroy değil: EditMode'da karenin sonu hiç
            // gelmez ve sahnede sızıntı kalırdı. Yapı görselleri probe'un
            // çocuğu olduğu için tek çağrı hepsini götürür.
            Object.DestroyImmediate(probe);

            // Prefab probe'un ÇOCUĞU DEĞİL — havuzun ondan doğurduğu kopyalar
            // öyle, kendisi değil; ayrı bir çağrı gerekiyor.
            if (unitPrefab != null)
            {
                Object.DestroyImmediate(unitPrefab.gameObject);
            }

            unitPrefab = null;
            teamIdle = null;

            for (int i = 0; i < disposables.Count; i++)
            {
                Object.DestroyImmediate(disposables[i]);
            }

            disposables.Clear();
        }

        /// <summary>
        /// MADDE 1 — düzeltmenin doğrudan kanıtı: yerleştirme artık patlamıyor,
        /// yapı tahtaya giriyor ve <c>Placed</c> Console'a ulaşıyor.
        /// </summary>
        [Test]
        public void CommitPlacement_OnAFreeCell_PlacesTheStructureAndSaysPlaced()
        {
            // SONUÇ DEĞERİ LOG SATIRINDAN OKUNUYOR ve bu bir çaresizlik değil:
            // CommitPlacement void döner, çünkü tek çağıranının cevaba göre
            // yapacağı farklı bir iş yok. Bugün o cevabın ekrana ulaştığı TEK
            // yol bu satır, dolayısıyla sınanması gereken de bu satır.
            LogAssert.Expect(LogType.Log, new Regex(@"placement at \(1,1\) -> Placed"));

            Invoke("CommitPlacement", 1, 1);

            Assert.That(battle.StructureCount, Is.EqualTo(1));
            Assert.That(battle.TryGetUnit(1, 1, out Unit standing), Is.True,
                "the cell must now hold the structure identity");
            Assert.That(battle.TryGetStructure(standing, out Structure _), Is.True,
                "the identity on that cell must be registered as a structure");
        }

        /// <summary>
        /// MADDE 1 — kusurun KÖKÜ: yapı, kendi kimliğiyle girmeli. Üstteki test
        /// "artık patlamıyor" der; bu test "neden patlamıyor" der ve düzeltmenin
        /// yerine bir kilit vurur.
        /// </summary>
        [Test]
        public void CommitPlacement_GivesTheStructureItsOwnIdentity_NotThePlacers()
        {
            LogAssert.Expect(LogType.Log, new Regex(@"-> Placed"));

            Invoke("CommitPlacement", 1, 1);

            Assert.That(battle.TryGetUnit(1, 1, out Unit standing), Is.True);
            Assert.That(standing, Is.Not.SameAs(placer),
                "reusing the placer identity is exactly what threw ArgumentException");

            // YERLEŞTİREN OLDUĞU YERDE KALIR ve savaşçı olmayı SÜRDÜRÜR. İki
            // iddia da gerekli: kimlik yeniden kullanılsaydı yerleştiren ya
            // ikinci bir hücreye yazılmaya çalışılır ya da yapı sözlüğüne düşerdi.
            Assert.That(battle.TryGetCombatant(placer, out Combatant _), Is.True);
            Assert.That(battle.TryGetStructure(placer, out Structure _), Is.False);
            Assert.That(battle.TryGetPosition(placer, out int x, out int y), Is.True);
            Assert.That(x, Is.EqualTo(PlacerX));
            Assert.That(y, Is.EqualTo(PlacerY));
        }

        /// <summary>
        /// MADDE 2 — yerleşen yapının görseli artık bir tabloya yazılıyor ve
        /// sahnede gerçekten duruyor.
        /// </summary>
        [Test]
        public void CommitPlacement_OnAFreeCell_RegistersTheStructureVisual()
        {
            LogAssert.Expect(LogType.Log, new Regex(@"-> Placed"));

            Invoke("CommitPlacement", 1, 1);

            Assert.That(battle.TryGetUnit(1, 1, out Unit standing), Is.True);
            Assert.That(StructureViews().Count, Is.EqualTo(1));
            Assert.That(StructureViews().ContainsKey(standing), Is.True,
                "the table must be keyed by the structure identity, nothing else");

            // TABLODAKİ NESNE SAHNEDEKİ NESNENİN TA KENDİSİ olmalı: ayrı bir
            // GameObject'e yazılsaydı temizlik yanlış nesneyi silerdi ve enkaz
            // yine ekranda kalırdı — hiçbir şey patlamadan.
            Assert.That(StructureViews()[standing], Is.SameAs(FindChild(standing.Name)));
        }

        /// <summary>
        /// MADDE 2 — aynı hücreye ikinci yerleştirme reddedilir; ne tahtada ne
        /// ekranda ikinci bir şey doğar.
        /// </summary>
        [Test]
        public void CommitPlacement_OnACellThatAlreadyHoldsAStructure_IsRejected()
        {
            LogAssert.Expect(LogType.Log, new Regex(@"-> Placed"));
            Invoke("CommitPlacement", 1, 1);

            // SEÇİM AYAKTA KALIYOR: CommitPlacement kipi bırakır ama seçimi
            // BIRAKMAZ, ve ikinci çağrı tam olarak bu yüzden mümkün.
            LogAssert.Expect(LogType.Log, new Regex(@"placement at \(1,1\) -> RejectedCellOccupied"));
            Invoke("CommitPlacement", 1, 1);

            Assert.That(battle.StructureCount, Is.EqualTo(1));
            Assert.That(StructureViews().Count, Is.EqualTo(1),
                "a rejected placement must not leave a second visual behind");
        }

        /// <summary>
        /// MADDE 2 — reddin ikinci yönü: yerleştirenin KENDİ hücresi de doludur.
        /// Ayrı yazılıyor çünkü doluluk sorusu tek tahtaya soruluyor ve o tek
        /// sorunun birimleri de kapsadığı ancak burada görünür.
        /// </summary>
        [Test]
        public void CommitPlacement_OnTheCellWhereThePlacerStands_IsRejected()
        {
            LogAssert.Expect(LogType.Log, new Regex(@"-> RejectedCellOccupied"));

            Invoke("CommitPlacement", PlacerX, PlacerY);

            Assert.That(battle.StructureCount, Is.EqualTo(0));
            Assert.That(StructureViews().Count, Is.EqualTo(0));
        }

        /// <summary>
        /// MADDE 2 — tahta dışı hücre. Bu dal düzeltmeden ÖNCE de doğru
        /// çalışıyordu ve testi yine de yazılıyor: reddin iki ayrı sebebi var ve
        /// biri düzelirken diğerinin sessizce kaybolmadığını gösteren tek şey bu.
        /// </summary>
        [Test]
        public void CommitPlacement_OutsideTheBoard_IsRejected()
        {
            LogAssert.Expect(LogType.Log, new Regex(@"-> RejectedInvalidCell"));

            Invoke("CommitPlacement", Width, 0);

            Assert.That(battle.StructureCount, Is.EqualTo(0));
            Assert.That(StructureViews().Count, Is.EqualTo(0));
        }

        /// <summary>
        /// MADDE 2 — kusurun asıl ucu: enkaz süresi dolunca yapı hem savaştan
        /// hem TABLODAN düşer. Düzeltmeden önce burası "No view registered for
        /// unit" LogError'ı basardı ve test o hata yüzünden kırmızı olurdu.
        /// </summary>
        [Test]
        public void AdvanceBattleTime_WhenTheRubbleWindowIsOver_RemovesTheStructureVisual()
        {
            LogAssert.Expect(LogType.Log, new Regex(@"-> Placed"));
            Invoke("CommitPlacement", 1, 1);

            Assert.That(battle.TryGetUnit(1, 1, out Unit standing), Is.True);
            Assert.That(battle.TryGetStructure(standing, out Structure structure), Is.True);

            // ENKAZ SAYACI ANCAK CAN BİTİNCE BAŞLAR: Tick'i önce çağırmak
            // ayakta duran bir yapıda hiçbir şey saymazdı.
            Assert.That(structure.TakeDamage(StructureMaxHealth), Is.True, "setup: this hit destroys it");
            battle.Tick(StructureLifecycle.DefaultRubbleWindowSeconds);
            Assert.That(structure.IsReadyForCleanup, Is.True, "setup: the rubble window is over");

            // BU HATA UNITY'NİN, BİZİM DEĞİL — ve beklenmesi bir taviz değil bir
            // KANIT: EditMode'da Object.Destroy "Destroy may not be called from
            // edit mode!" diye bir LogError basar (ölçüldü, birebir bu metin).
            // Yani bu satırın yeşil kalması, temizliğin gerçekten Destroy'a
            // ULAŞTIĞINI söylüyor; üretim kodu doğru metodu çağırıyor, çünkü
            // oyun Play mode'da koşar ve orada DestroyImmediate yanlış olurdu.
            // Nesnenin gerçekten yok olduğunu EditMode'da ölçmenin yolu yok;
            // ölçülebilen şey çağrının yapıldığı ve tablonun boşaldığıdır.
            LogAssert.Expect(LogType.Error, new Regex("Destroy may not be called from edit mode"));
            LogAssert.Expect(LogType.Log, new Regex(@"structure 'Structure_1_1' was cleaned up"));
            Invoke("AdvanceBattleTime");

            Assert.That(battle.StructureCount, Is.EqualTo(0));
            Assert.That(StructureViews().Count, Is.EqualTo(0),
                "the sweep must clear the table, not only the battle record");
        }

        /// <summary>
        /// MADDE 4 — en küçük zafer koşulu Console'a ulaşıyor.
        /// </summary>
        [Test]
        public void AnnounceWinnerIfAny_WhenTheEnemyIsWipedOut_SaysThePlayerWins()
        {
            Combatant enemy = NewCombatant(Team.Enemy);
            battle.AddUnit(new Unit("Raider"), enemy, 2, 2);

            KillOutright(enemy);

            LogAssert.Expect(LogType.Log, new Regex(@"BATTLE OVER - Player wins"));

            Invoke("AnnounceWinnerIfAny");
        }

        /// <summary>
        /// MADDE 4 — DÜŞMÜŞ BİRİM SAVAŞI BİTİRMEZ. Bu test kuralın en kolay
        /// kaçırılan yanını tutuyor: canı biten birim <c>Downed</c>'dır,
        /// <c>Dead</c> değil, ve diriltilebildiği sürece tarafı ayaktadır.
        /// Kural "canı bitti" diye yazılsaydı burası sessizce yeşil kalır ve
        /// oyun kurtarılabilir bir birimin üstüne biterdi.
        /// </summary>
        [Test]
        public void AnnounceWinnerIfAny_WhileTheLastEnemyIsOnlyDowned_DeclaresNobody()
        {
            Combatant enemy = NewCombatant(Team.Enemy);
            battle.AddUnit(new Unit("Raider"), enemy, 2, 2);

            enemy.TakeDamage(MaxHealth);
            Assert.That(enemy.State, Is.EqualTo(UnitState.Downed), "setup");

            // İDDİA LOG'DA DEĞİL KAYITTA: "hiçbir şey yazılmadı" iddiasını
            // LogAssert kuramaz, ama zaferin İKİ girdisinden birini doğrudan
            // okuyabiliriz ve kural o girdiden türüyor.
            Assert.That(battle.HasUnitsLeft(Team.Enemy), Is.True);

            Invoke("AnnounceWinnerIfAny");
        }

        /// <summary>
        /// MADDE 5 — <c>BattleActions.Revive</c> artık üretimde bir çağıran
        /// taşıyor ve <c>ReviveOutcome</c> ekrana ulaşıyor.
        /// </summary>
        [Test]
        public void TryReviveTarget_OnAFallenAlly_RevivesItAndReportsTheOutcome()
        {
            var target = new Unit("Sapper");
            Combatant ally = NewCombatant(Team.Player);
            battle.AddUnit(target, ally, PlacerX, PlacerY + 1);

            ally.TakeDamage(MaxHealth);
            Assert.That(ally.State, Is.EqualTo(UnitState.Downed), "setup");

            LogAssert.Expect(LogType.Log, new Regex(@"'Sapper' at \(1,3\) was REVIVED"));

            Invoke("TryReviveTarget", target, PlacerX, PlacerY + 1);

            Assert.That(ally.State, Is.EqualTo(UnitState.Alive));
        }

        /// <summary>
        /// MADDE 5 — yapı diriltilmez, ve bu soru adaptörde soruluyor.
        /// <c>BattleActions.Revive</c>'a bir yapı vermek bir oyun sonucu değil
        /// bir ÇAĞIRAN HATASIDIR ve istisna atar; fare ise pekâlâ bir barakayı
        /// gösterebilir. Bu test tam olarak o istisnanın oyuncuya ulaşmadığını
        /// koruyor.
        /// </summary>
        [Test]
        public void TryReviveTarget_OnAStructure_IsRefusedWithoutThrowing()
        {
            LogAssert.Expect(LogType.Log, new Regex(@"-> Placed"));
            Invoke("CommitPlacement", 1, 1);
            Assert.That(battle.TryGetUnit(1, 1, out Unit structureUnit), Is.True);

            LogAssert.Expect(LogType.Log, new Regex(@"holds a structure; structures are not revived"));

            Assert.DoesNotThrow(() => { Invoke("TryReviveTarget", structureUnit, 1, 1); });
        }

        // ══ MENZİLİN SAHİBİ SALDIRANIN KENDİSİ ═══════════════════════════
        // Üç test, üç kaynak: savaşçı, yapı ve hiçbiri. Tek testle yazılsaydı
        // geri çekilme dalı (Inspector değeri) sınanmadan kalırdı ve o dal tam
        // olarak testlerin kendi kurulumunda kullanılan daldır.

        /// <summary>
        /// Menzil savaşçının kendi saldırı profilinden okunur. Bu iddia
        /// olmasaydı menzili 3 olan bir okçu, Inspector'daki 1 yüzünden hedefin
        /// dibine kadar yürümeye devam ederdi ve hiçbir şey patlamazdı.
        /// </summary>
        [Test]
        public void AttackRangeOf_ForACombatant_ReadsItsOwnProfileNotTheInspectorNumber()
        {
            SetField("attackRange", 1);

            var archer = new Unit("Archer");
            battle.AddUnit(
                archer,
                new Combatant(
                    new Health(MaxHealth),
                    new UnitLifecycle(),
                    new AttackProfile(Damage, 3),
                    Team.Player),
                0,
                0);

            Assert.That(Invoke("AttackRangeOf", archer), Is.EqualTo(3));
        }

        /// <summary>
        /// Yapılar için de aynı kaynak: <c>Structure.AttackProfile.Range</c>.
        /// </summary>
        [Test]
        public void AttackRangeOf_ForAStructure_ReadsTheStructuresOwnProfile()
        {
            SetField("attackRange", 1);

            Unit tower = PlaceTower(Team.Player, range: 2, x: 0, y: 0);

            Assert.That(Invoke("AttackRangeOf", tower), Is.EqualTo(2));
        }

        /// <summary>
        /// Savaşın defterinde bulunmayan bir kimlik Inspector değerine düşer.
        /// Sıfır dönseydi her hedef menzil dışı görünürdü.
        /// </summary>
        [Test]
        public void AttackRangeOf_ForAnIdentityOutsideTheBattle_FallsBackToTheInspectorNumber()
        {
            SetField("attackRange", 4);

            Assert.That(Invoke("AttackRangeOf", new Unit("Ghost")), Is.EqualTo(4));
            Assert.That(Invoke("AttackRangeOf", new object[] { null }), Is.EqualTo(4));
        }

        // ══ KENAR HALKASI — SAF GEOMETRİ, SAHNESİZ SINANIYOR ══════════════
        // Halkanın GameObject tarafı burada sınanmıyor ve sınanamaz da: hücre
        // görselleri Awake'te doğuyor. Sınanan şey halkanın NEREYE düştüğü.

        /// <summary>
        /// Halka oynanabilir ızgaranın çevresini sarar ve ona HİÇ dokunmaz.
        /// İkinci iddia birincisinden önemli: halka bir çim hücresini de
        /// kapsasaydı oyunun kuralına dokunmadan zemin desenini bozardı.
        /// </summary>
        [Test]
        public void CollectBorderCells_WithThicknessTwo_SurroundsTheGridWithoutTouchingIt()
        {
            const int RingWidth = 10;
            const int RingHeight = 5;
            const int Thickness = 2;

            var cells = (List<Vector2Int>)Invoke("CollectBorderCells", RingWidth, RingHeight, Thickness);

            // Dış dikdörtgen eksi oynanabilir alan: 14*9 - 10*5 = 76.
            int expected = ((RingWidth + (2 * Thickness)) * (RingHeight + (2 * Thickness)))
                           - (RingWidth * RingHeight);
            Assert.That(cells.Count, Is.EqualTo(expected));

            foreach (Vector2Int cell in cells)
            {
                bool insideTheBoard = cell.x >= 0 && cell.x < RingWidth
                                      && cell.y >= 0 && cell.y < RingHeight;
                Assert.That(insideTheBoard, Is.False,
                    $"({cell.x},{cell.y}) is a playable cell; the ring must never cover one");
            }

            // İki uç köşe: halkanın hem eksi hem artı tarafa uzandığının kanıtı.
            Assert.That(cells, Contains.Item(new Vector2Int(-Thickness, -Thickness)));
            Assert.That(cells, Contains.Item(
                new Vector2Int(RingWidth + Thickness - 1, RingHeight + Thickness - 1)));
        }

        /// <summary>
        /// Kalınlık sıfırsa halka çizilmez. Bu dal, halkası olmayan bugünkü
        /// sahnelerin bozulmadığının tek ölçüsü.
        /// </summary>
        [Test]
        public void CollectBorderCells_WithZeroThickness_DrawsNothing()
        {
            var cells = (List<Vector2Int>)Invoke("CollectBorderCells", Width, Height, 0);

            Assert.That(cells, Is.Empty);
        }

        // ══ BEKLEYEN VURUŞUN İPTAL KOŞULLARI ═════════════════════════════
        // "Yaklaş, sonra vur" hamlesinin ikinci yarısı bir emirdir ve emrin ne
        // zaman DÜŞTÜĞÜ, ne zaman yürüdüğü kadar önemli: düşmeyen bir emir,
        // oyuncunun vazgeçtiği hedefe inen bir vuruş demek.

        /// <summary>
        /// İki taraf da tahtada ve saldıran hâlâ seçili: emir ayakta.
        /// </summary>
        [Test]
        public void PendingStrikeIsAlive_WithBothSidesOnTheBoardAndTheAttackerSelected_IsTrue()
        {
            var target = new Unit("Raider");
            battle.AddUnit(target, NewCombatant(Team.Enemy), 2, 2);

            SetField("pendingStrikeAttacker", placer);
            SetField("pendingStrikeTarget", target);

            Assert.That(Invoke("PendingStrikeIsAlive"), Is.True);
        }

        /// <summary>
        /// SEÇİM DEĞİŞTİ: oyuncu başka bir birime geçtiyse eski emir düşer.
        /// </summary>
        [Test]
        public void PendingStrikeIsAlive_WhenTheSelectionMovedToAnotherUnit_IsFalse()
        {
            var target = new Unit("Raider");
            battle.AddUnit(target, NewCombatant(Team.Enemy), 2, 2);

            var other = new Unit("Sapper");
            battle.AddUnit(other, NewCombatant(Team.Player), 0, 0);

            SetField("pendingStrikeAttacker", placer);
            SetField("pendingStrikeTarget", target);
            SetField("selectedUnit", other);

            Assert.That(Invoke("PendingStrikeIsAlive"), Is.False);
        }

        /// <summary>
        /// HEDEF TAHTADAN KALKTI. Emir düşmeseydi bir sonraki kare savaşta artık
        /// bulunmayan bir kimliğe saldırı çağırır ve bu bir oyun sonucu değil
        /// bir istisna üretirdi.
        /// </summary>
        [Test]
        public void PendingStrikeIsAlive_WhenTheTargetLeftTheBattle_IsFalse()
        {
            var target = new Unit("Raider");
            battle.AddUnit(target, NewCombatant(Team.Enemy), 2, 2);

            SetField("pendingStrikeAttacker", placer);
            SetField("pendingStrikeTarget", target);

            Assert.That(battle.RemoveUnit(target), Is.True, "setup");

            Assert.That(Invoke("PendingStrikeIsAlive"), Is.False);
        }

        // ══ YAPI YÜRÜMEZ — İKİ ÇAĞIRAN, TEK SORU ═════════════════════════
        // İki test de aynı istisnayı tutuyor: "The unit is not in this battle".
        // Konsolda İKİ ayrı yığın olarak ölçüldü ve ikisi de oyunu kesiyordu.

        /// <summary>
        /// Seçili bir bina varken boş hücreye tıklamak hareket yoluna HİÇ
        /// girmemeli.
        /// </summary>
        [Test]
        public void HandleEmptyCellClick_WithAStructureSelected_RefusesToWalkInsteadOfThrowing()
        {
            Unit tower = PlaceTower(Team.Player, range: 2, x: 0, y: 0);
            SetField("selectedUnit", tower);

            LogAssert.Expect(LogType.Log, new Regex("structures do not walk"));

            Assert.DoesNotThrow(() => { Invoke("HandleEmptyCellClick", 2, 4); });

            // BİNA KIMILDAMADI: iddia log'da değil kayıtta, çünkü asıl korkulan
            // şey satırın yazılmaması değil binanın yürümesi.
            Assert.That(battle.TryGetPosition(tower, out int x, out int y), Is.True);
            Assert.That(x, Is.EqualTo(0));
            Assert.That(y, Is.EqualTo(0));
        }

        /// <summary>
        /// Seçili bir bina varken düşmana tıklamak da yaklaşma adımını atlar ve
        /// doğrudan saldırıya gider.
        /// </summary>
        [Test]
        public void HandleOccupiedCellClick_WithAStructureSelected_NeverEntersTheMovePath()
        {
            Unit tower = PlaceTower(Team.Player, range: 2, x: 0, y: 0);
            SetField("selectedUnit", tower);

            var enemy = new Unit("Raider");
            battle.AddUnit(enemy, NewCombatant(Team.Enemy), 1, 0);

            Assert.DoesNotThrow(() => { Invoke("HandleOccupiedCellClick", enemy, 1, 0); });

            Assert.That(battle.TryGetPosition(tower, out int x, out int y), Is.True);
            Assert.That(x, Is.EqualTo(0));
            Assert.That(y, Is.EqualTo(0));
        }

        // ══ ATEŞ EDEN YAPININ HEDEF SEÇİMİ ═══════════════════════════════
        // Sayacın kendisi sınanmıyor (Time'a bağlı değil ama sıra kipine bağlı);
        // sınanan şey SEÇİM: en yakın, ayakta, düşman.

        /// <summary>
        /// En yakın düşman kazanır, dostlar hiç aday olmaz ve eşitlikte tahtayı
        /// tarama sırası karar verir.
        /// </summary>
        [Test]
        public void TryFindStructureTarget_PicksTheNearestEnemyAndNeverAnAlly()
        {
            Unit tower = PlaceTower(Team.Player, range: 2, x: 1, y: 0);
            Assert.That(battle.TryGetStructure(tower, out Structure structure), Is.True, "setup");

            battle.AddUnit(new Unit("Ally"), NewCombatant(Team.Player), 0, 1);

            var near = new Unit("Near");
            battle.AddUnit(near, NewCombatant(Team.Enemy), 1, 1);

            // Aynı uzaklıkta ikinci bir düşman: tarama sırası (önce küçük x)
            // yüzünden 'Near' kazanmalı.
            battle.AddUnit(new Unit("AlsoNear"), NewCombatant(Team.Enemy), 2, 1);

            object[] arguments = { tower, structure, null, 0, 0 };
            Assert.That(InvokeWithArguments("TryFindStructureTarget", arguments), Is.True);

            Assert.That(arguments[2], Is.SameAs(near));
            Assert.That(arguments[3], Is.EqualTo(1));
            Assert.That(arguments[4], Is.EqualTo(1));
        }

        /// <summary>
        /// DÜŞMÜŞ BİRİME ATEŞ EDİLMEZ: kule düşmüş bir bedeni döverken ayaktaki
        /// tehdidi görmezden gelirdi.
        /// </summary>
        [Test]
        public void TryFindStructureTarget_SkipsAFallenEnemyAndTakesTheStandingOne()
        {
            Unit tower = PlaceTower(Team.Player, range: 2, x: 1, y: 0);
            Assert.That(battle.TryGetStructure(tower, out Structure structure), Is.True, "setup");

            Combatant fallen = NewCombatant(Team.Enemy);
            battle.AddUnit(new Unit("Fallen"), fallen, 1, 1);
            fallen.TakeDamage(MaxHealth);
            Assert.That(fallen.State, Is.EqualTo(UnitState.Downed), "setup");

            var standing = new Unit("Standing");
            battle.AddUnit(standing, NewCombatant(Team.Enemy), 0, 2);

            object[] arguments = { tower, structure, null, 0, 0 };
            Assert.That(InvokeWithArguments("TryFindStructureTarget", arguments), Is.True);

            Assert.That(arguments[2], Is.SameAs(standing));
        }

        /// <summary>
        /// Düşman YAPISI da hedeftir. Kule bu iddia olmadan düşman üssünün
        /// dibinde sessiz duruyordu: tarama yalnız savaşçı defterine bakıyor,
        /// hücrede bir bina bulunca atlıyordu.
        /// </summary>
        [Test]
        public void TryFindStructureTarget_WithOnlyAnEnemyBuildingInRange_TakesTheBuilding()
        {
            Unit tower = PlaceTower(Team.Player, range: 2, x: 0, y: 0);
            Assert.That(battle.TryGetStructure(tower, out Structure structure), Is.True, "setup");

            Unit enemyBase = PlaceTower(Team.Enemy, range: 1, x: 1, y: 1);

            object[] arguments = { tower, structure, null, 0, 0 };
            Assert.That(InvokeWithArguments("TryFindStructureTarget", arguments), Is.True);

            Assert.That(arguments[2], Is.SameAs(enemyBase));
        }

        /// <summary>
        /// EŞİT UZAKLIKTA SAVAŞÇI KAZANIR: asker geri vurur ve yer değiştirir,
        /// bina ikisini de yapamaz. Bu iddia olmadan kazananı yalnızca tarama
        /// sırası belirlerdi ve karar hiçbir yerde yazılı olmazdı.
        /// </summary>
        [Test]
        public void TryFindStructureTarget_AtTheSameDistance_PrefersTheFighterOverTheBuilding()
        {
            Unit tower = PlaceTower(Team.Player, range: 2, x: 2, y: 0);
            Assert.That(battle.TryGetStructure(tower, out Structure structure), Is.True, "setup");

            // Bina ÖNCE taranıyor (küçük x), savaşçı sonra: sıra kaybetse bile
            // savaşçının kazanması tercihin gerçekten yazıldığını gösterir.
            PlaceTower(Team.Enemy, range: 1, x: 1, y: 1);

            var fighter = new Unit("Raider");
            battle.AddUnit(fighter, NewCombatant(Team.Enemy), 2, 1);

            object[] arguments = { tower, structure, null, 0, 0 };
            Assert.That(InvokeWithArguments("TryFindStructureTarget", arguments), Is.True);

            Assert.That(arguments[2], Is.SameAs(fighter));
        }

        // ══ SEÇİMİN DÜŞMESİ TEK KAPIDAN DUYURULUR ════════════════════════

        /// <summary>
        /// Seçili birim tahtadan kalkınca <c>SelectionChanged</c> yayınlanır.
        ///
        /// Ölçüldü: <c>DespawnView</c> alanı doğrudan null'lıyordu ve durum
        /// şeridi ölmüş savaşçının canını anlatmaya devam ediyordu. Üretim
        /// paneli temizleniyordu çünkü onun dinleyicisi AYRICA
        /// <c>UnitRemoved</c>'a abone — iki dinleyici arasındaki bu sessiz fark
        /// tam olarak iki kapının farkıydı.
        /// </summary>
        [Test]
        public void DespawnView_OnTheSelectedUnit_AnnouncesThatTheSelectionIsGone()
        {
            InstallViewPool();
            Unit fighter = SpawnFighter("Doomed", Team.Player, 0, 0);
            SetField("selectedUnit", fighter);

            int announcements = 0;
            Unit announced = fighter;
            adapter.SelectionChanged += unit =>
            {
                announcements++;
                announced = unit;
            };

            Invoke("DespawnView", fighter);

            Assert.That(announcements, Is.EqualTo(1), "the drop must be announced exactly once");
            Assert.That(announced, Is.Null);
            Assert.That(GetField("selectedUnit"), Is.Null);
        }

        // ══ ZAFER BİR KEZ DUYURULUR ══════════════════════════════════════

        /// <summary>
        /// "BATTLE OVER" satırı bir kez basılır.
        ///
        /// LogAssert KULLANILMIYOR ve bu bir çaresizlik değil: o kapı beklenmeyen
        /// <c>Log</c> satırlarını başarısızlık saymıyor, yani tekrarı görmezdi.
        /// Sayan tek şey Console'a gerçekten kaç satır düştüğü.
        /// </summary>
        [Test]
        public void AnnounceWinnerIfAny_CalledAgainAfterTheWin_PrintsTheLineOnlyOnce()
        {
            Combatant enemy = NewCombatant(Team.Enemy);
            battle.AddUnit(new Unit("Raider"), enemy, 2, 2);
            KillOutright(enemy);

            int lines = 0;
            Application.LogCallback counter = (condition, stackTrace, type) =>
            {
                if (condition.Contains("BATTLE OVER"))
                {
                    lines++;
                }
            };

            Application.logMessageReceived += counter;
            try
            {
                Invoke("AnnounceWinnerIfAny");
                Invoke("AnnounceWinnerIfAny");
                Invoke("AnnounceWinnerIfAny");
            }
            finally
            {
                Application.logMessageReceived -= counter;
            }

            Assert.That(lines, Is.EqualTo(1));
        }

        // ══ BİTİRİCİ VURUŞ ═══════════════════════════════════════════════

        /// <summary>
        /// <c>HitAndFinished</c> bir İSABETTİR: adıyla karşılanır, programcı
        /// hatası dalına düşmez ve isabet sonrası seçimi bırakma kuralı ona da
        /// uygulanır.
        /// </summary>
        [Test]
        public void ReactToAttack_WithHitAndFinished_IsALandedStrikeAndReleasesTheSelection()
        {
            InstallViewPool();
            Unit attacker = SpawnFighter("Finisher", Team.Player, 0, 0);
            SetField("selectedUnit", attacker);

            var target = new Unit("Raider");
            battle.AddUnit(target, NewCombatant(Team.Enemy), 0, 1);

            LogAssert.Expect(LogType.Log, new Regex(@"'Raider' at \(0,1\) was FINISHED OFF"));
            LogAssert.Expect(LogType.Log, new Regex(@"struck; the selection was released"));

            InvokeWithArguments(
                "ReactToAttack",
                new object[] { attacker, AttackOutcome.HitAndFinished, target, 0, 1 });

            Assert.That(GetField("selectedUnit"), Is.Null);
        }

        // ══ İMLEÇ ÇERÇEVESİNİN ÖNBELLEĞİ ═════════════════════════════════

        /// <summary>
        /// Aynı hücre ve aynı seçim için yol İKİNCİ kez aranmaz.
        ///
        /// İDDİA ESKİMİŞ BİR CEVAPLA KURULUYOR ve başka türlü kurulamazdı:
        /// tahsis sayısını EditMode'dan görmenin yolu yok, ama arama gerçekten
        /// tekrarlansaydı cevap DEĞİŞİRDİ — hedef hücre bu arada doldu.
        /// </summary>
        [Test]
        public void IsHoverReachable_AskedTwiceForTheSameCell_DoesNotSearchAgain()
        {
            Assert.That(Invoke("IsHoverReachable", 0, 0), Is.True, "setup: the cell must start reachable");

            battle.AddUnit(new Unit("Wall"), NewCombatant(Team.Enemy), 0, 0);

            Assert.That(Invoke("IsHoverReachable", 0, 0), Is.True,
                "a second search would have found the cell blocked");
        }

        /// <summary>
        /// Anahtar değişince önbellek düşer: aynı hücre, BAŞKA bir seçili birim.
        /// Bu iddia olmadan önbellek sonsuza kadar ilk cevabı verirdi.
        /// </summary>
        [Test]
        public void IsHoverReachable_AfterTheSelectionChanges_SearchesAgain()
        {
            Assert.That(Invoke("IsHoverReachable", 0, 0), Is.True, "setup");

            battle.AddUnit(new Unit("Wall"), NewCombatant(Team.Enemy), 0, 0);

            var scout = new Unit("Scout");
            battle.AddUnit(scout, NewCombatant(Team.Player), 2, 4);
            SetField("selectedUnit", scout);

            Assert.That(Invoke("IsHoverReachable", 0, 0), Is.False);
        }

        // ══ CAN BARININ YÜKSEKLİĞİ ═══════════════════════════════════════

        /// <summary>
        /// Bar, sahibinin ÇİZİLEN boyunun tepesinde durur — sahibin ölçeği ne
        /// olursa olsun.
        ///
        /// Ölçüldü: bar 1,6 ölçekli yapıda 0,93, 1,25 ölçekli savaşçıda 0,725
        /// dünya biriminde duruyordu; ikisi de görselin tepesiyle ilgisizdi.
        /// Sebep, <c>HealthBarView</c>'un yazdığı yerel yüksekliğin ebeveynin
        /// ölçeğiyle ÇARPILMASIydı.
        /// </summary>
        [Test]
        public void AttachHealthBar_PutsTheBarOnTopOfTheDrawnSprite_WhateverTheOwnerScaleIs()
        {
            SetField("healthBarSprite", NewSprite());

            Assert.That(WorldBarHeight(scale: 1.6f, drawnHeight: 1.6f), Is.EqualTo(0.88f).Within(0.0001f));
            Assert.That(WorldBarHeight(scale: 1.25f, drawnHeight: 1.25f), Is.EqualTo(0.705f).Within(0.0001f));

            // AYNI ÇİZİLİ BOY, İKİ FARKLI ÖLÇEK: bar aynı yerde durmalı. Eski
            // hâlde bu iki sayı birbirinden 0,2 dünya birimi ayrılıyordu.
            Assert.That(
                WorldBarHeight(scale: 1.6f, drawnHeight: 1f),
                Is.EqualTo(WorldBarHeight(scale: 1.25f, drawnHeight: 1f)).Within(0.0001f));
        }

        /// <summary>
        /// Verilen ölçek ve çizili boy için barın DÜNYA yüksekliğini ölçer.
        /// </summary>
        // ÖLÇÜM YEREL DEĞERİ ÖLÇEKLE ÇARPARAK YAPILIYOR, çünkü kusurun ta
        // kendisi o çarpımdı: bar ebeveynin yerel uzayında yaşıyor.
        private float WorldBarHeight(float scale, float drawnHeight)
        {
            var owner = new GameObject($"Owner_{scale}_{drawnHeight}");
            owner.transform.SetParent(probe.transform);
            owner.transform.localScale = new Vector3(scale, scale, 1f);

            InvokeWithArguments(
                "AttachHealthBar",
                new object[] { new Unit($"Bar_{scale}_{drawnHeight}"), owner.transform, 4, drawnHeight });

            Transform bar = owner.transform.Find("HealthBar");
            Assert.That(bar, Is.Not.Null, "the bar must have been created");

            return bar.localPosition.y * scale;
        }

        // ══ SÜRÜKLEME HAYALETİ ═══════════════════════════════════════════

        /// <summary>
        /// null simge bir "değişiklik yok" değil bir SİLME emridir. Eski hâlde
        /// hayalet, simgesi olmayan bir birim sürüklenirken ÖNCEKİ binanın
        /// görselini taşımaya devam ediyordu.
        /// </summary>
        [Test]
        public void SetPlacementVisual_WithNull_ClearsTheGhostAndHidesIt()
        {
            var texture = new Texture2D(4, 4);
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f));

            try
            {
                adapter.SetPlacementVisual(sprite);
                ghost.enabled = true;
                Assert.That(ghost.sprite, Is.SameAs(sprite), "setup");

                adapter.SetPlacementVisual(null);

                Assert.That(ghost.sprite, Is.Null, "the ghost must not keep the previous building");
                Assert.That(ghost.enabled, Is.False, "a ghost with nothing to show must not stay visible");
                Assert.That(GetField("pendingStructureSprite"), Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(sprite);
                Object.DestroyImmediate(texture);
            }
        }

        /// <summary>
        /// null simge PALETİ unutturur, sahnede YAZILI olan yedeği yok etmez.
        ///
        /// BU OTURUMDA ÜRETİLMİŞ BİR REGRESYONUN KİLİDİ: paletten bir bina bir
        /// kez sürüklendikten sonra <c>ProductionDirector.CancelPlacement</c>
        /// her bırakışta buraya null geçiyor, hayaletin sprite'ı siliniyordu.
        /// Klavyeli yerleştirme kipinin tek yedeği o sprite olduğu için bina
        /// GÖRÜNMEZ kuruluyordu — boş karenin üstünde havada duran bir can barı
        /// ve geçilemeyen bir hücre.
        /// </summary>
        [Test]
        public void SetPlacementVisual_WithNull_RestoresTheSpriteWrittenInTheScene()
        {
            Sprite authored = NewSprite();
            Sprite fromPalette = NewSprite();

            ghost.sprite = authored;
            Invoke("CaptureAuthoredGhostSprite");

            adapter.SetPlacementVisual(fromPalette);
            Assert.That(ghost.sprite, Is.SameAs(fromPalette), "setup: the palette icon must win while it lasts");

            adapter.SetPlacementVisual(null);

            Assert.That(ghost.sprite, Is.SameAs(authored),
                "the authored fallback is the only thing keyboard placement can draw");
            Assert.That(GetField("pendingStructureSprite"), Is.Null,
                "forgetting the palette icon is still the point of a null sprite");
            Assert.That(ghost.enabled, Is.False,
                "a ghost nobody asked for must not stay visible");
        }

        // ══ YAPI SEÇİMİ ══════════════════════════════════════════════════

        /// <summary>
        /// Bina seçmek Console'a hata YAZMAZ ve seçim ekranda görünür.
        ///
        /// Ölçüldü: oyuncu kendi binasına tıkladığında (üretim paneli ancak
        /// seçiliyken açıldığı için bu NORMAL akış) Console on iki satır
        /// "No view registered for unit" hatası basıyordu. Bu testin yeşil
        /// kalması o hatanın geri gelmediğinin tek ölçüsü: beklenmeyen bir
        /// LogError testi kendiliğinden kırmızıya çevirir.
        /// </summary>
        [Test]
        public void SelectUnit_OnAStructure_TintsItWithoutLoggingAnError()
        {
            Unit tower = PlaceTower(Team.Player, range: 2, x: 0, y: 0);

            // Önceki seçim BIRAKILIYOR: yerleştiren savaşçının görseli yok ve
            // onun seçimini kapatmak, bu testin ölçmediği bir LogError üretirdi.
            SetField("selectedUnit", null);

            Invoke("SelectUnit", tower);

            var renderer = StructureViews()[tower].GetComponent<SpriteRenderer>();
            Assert.That(renderer, Is.Not.Null);
            Assert.That(renderer.color, Is.Not.EqualTo(Color.white),
                "a selected structure must look different from an idle one");

            Invoke("ClearSelection");
            Assert.That(renderer.color, Is.EqualTo(Color.white));
        }

        // ══ ÜRETİLEN BİRİMİN KENDİ GÖVDESİ ═══════════════════════════════
        // Ölçülen kusur: PlaceUnit her birimi havuzdan alıp yalnız SetTeam
        // çağırıyordu, yani tahtadaki her savaşçı prefab'ın dört karesinden
        // birini alıyordu. Sürüklerken doğru simgeyi gören oyuncu, bıraktığında
        // hep aynı piyadeyi buluyordu.

        /// <summary>
        /// Verilen gövde görseli doğan görselin üstüne YAZILIR.
        /// </summary>
        [Test]
        public void PlaceUnit_WithABodySprite_PutsItOnTheSpawnedView()
        {
            InstallViewPool();

            Sprite own = NewSprite();
            var archer = new Unit("Archer");

            Assert.That(adapter.PlaceUnit(archer, NewCombatant(Team.Player), 0, 0, own), Is.True);

            Assert.That(BodyOf(archer), Is.SameAs(own),
                "the produced unit must reach the board with its own body");
        }

        /// <summary>
        /// Gövde verilmeyen bir birim takım karesinde kalır.
        /// </summary>
        // ESKİ ÇAĞIRAN KORUNUYOR: demo doğuşunun elinde bir varlık dosyası yok
        // ve null geçmesi bir eksiklik değil, tam olarak doğru cevap.
        [Test]
        public void PlaceUnit_WithoutABodySprite_KeepsTheTeamFrame()
        {
            InstallViewPool();

            var vanguard = new Unit("Vanguard2");

            Assert.That(adapter.PlaceUnit(vanguard, NewCombatant(Team.Player), 0, 0, null), Is.True);

            Assert.That(BodyOf(vanguard), Is.SameAs(teamIdle));
        }

        /// <summary>
        /// HAVUZUN SESSİZ HATASI, tahtanın kendi yolundan sınanıyor: kaldırılan
        /// bir birimin gövdesi, aynı görseli devralan bir sonraki birime
        /// GEÇMEZ.
        /// </summary>
        // BU TESTİN KORUDUĞU HATA HİÇBİR İZ BIRAKMAZ: ne istisna, ne konsol
        // satırı — yalnız ekranda yanlış birim. CreatedCount iddiası, testin
        // gerçekten havuzdan geçtiğini kanıtlıyor; ikinci bir Instantiate olsaydı
        // temizlik hiç sınanmamış olurdu.
        [Test]
        public void PlaceUnit_ReusingAPooledView_DoesNotInheritThePreviousBody()
        {
            InstallViewPool();

            Sprite own = NewSprite();
            var archer = new Unit("Archer");
            Assert.That(adapter.PlaceUnit(archer, NewCombatant(Team.Player), 0, 0, own), Is.True);

            SetField("selectedUnit", archer);
            Assert.That(adapter.RemoveSelected(), Is.True, "setup: the archer must leave the board");

            var pikeman = new Unit("Pikeman");
            Assert.That(adapter.PlaceUnit(pikeman, NewCombatant(Team.Player), 0, 1, null), Is.True);

            Assert.That(ViewPool().CreatedCount, Is.EqualTo(1),
                "setup: the second unit must have reused the pooled view");
            Assert.That(BodyOf(pikeman), Is.SameAs(teamIdle),
                "a pooled view must not carry the previous unit's body");
        }

        // ══ SALDIRIDAN SONRA SEÇİM ═══════════════════════════════════════
        // Oyuncunun isteği: saldırı emri verilen birim seçili kalmasın, oyuncu
        // hemen başka bir şey seçebilsin.

        /// <summary>
        /// İSABET seçimi bırakır.
        /// </summary>
        [Test]
        public void ReactToAttack_AfterALandedHit_ReleasesTheSelection()
        {
            InstallViewPool();
            Unit striker = SpawnFighter("Striker", Team.Player, 0, 0);
            Unit target = SpawnFighter("Raider", Team.Enemy, 0, 1);
            SetField("selectedUnit", striker);

            Invoke("ReactToAttack", striker, AttackOutcome.Hit, target, 0, 1);

            Assert.That(GetField("selectedUnit"), Is.Null);
        }

        /// <summary>
        /// REDDEDİLEN saldırı seçimi bırakmaz — oyuncu tekrar denemek isteyecek.
        /// </summary>
        [Test]
        public void ReactToAttack_OnARejectedAttack_KeepsTheSelection()
        {
            InstallViewPool();
            Unit striker = SpawnFighter("Striker", Team.Player, 0, 0);
            Unit target = SpawnFighter("Raider", Team.Enemy, 2, 4);
            SetField("selectedUnit", striker);

            Invoke("ReactToAttack", striker, AttackOutcome.RejectedOutOfRange, target, 2, 4);

            Assert.That(GetField("selectedUnit"), Is.SameAs(striker));
        }

        /// <summary>
        /// BEKLEME SÜRESİ bir isabet değildir: sakin bir satır yazılır ve seçim
        /// yerinde kalır.
        /// </summary>
        // W2-A'nın Core'a koyduğu kuralın ekrandaki karşılığı. Bu dal olmasaydı
        // arka arkaya tıklayan oyuncu Console'da kırmızı bir "Unhandled attack
        // outcome" görürdü, oysa yaptığı tek şey sabırsızlanmaktı.
        [Test]
        public void ReactToAttack_OnCooldown_SaysSoCalmlyAndKeepsTheSelection()
        {
            InstallViewPool();
            Unit striker = SpawnFighter("Striker", Team.Player, 0, 0);
            Unit target = SpawnFighter("Raider", Team.Enemy, 0, 1);
            SetField("selectedUnit", striker);

            LogAssert.Expect(LogType.Log, new Regex("henüz yeniden vuramaz"));

            Invoke("ReactToAttack", striker, AttackOutcome.RejectedOnCooldown, target, 0, 1);

            Assert.That(GetField("selectedUnit"), Is.SameAs(striker));
        }

        /// <summary>
        /// Kendiliğinden ateş eden kule OYUNCUNUN seçimini düşürmez.
        /// </summary>
        // BU TESTİN KORUDUĞU REGRESYON DOĞRUDAN OYNANIŞTIR: saldıranı seçimden
        // okuyan bir bırakma, her kule atışında oyuncunun elindeki savaşçıyı
        // bırakırdı ve oyuncu neden seçimini kaybettiğini anlayamazdı.
        [Test]
        public void ReactToAttack_WhenAStructureFiresOnItsOwn_LeavesThePlayersSelectionAlone()
        {
            InstallViewPool();
            Unit tower = PlaceTower(Team.Player, range: 2, x: 0, y: 0);
            Unit target = SpawnFighter("Raider", Team.Enemy, 0, 2);
            Unit striker = SpawnFighter("Striker", Team.Player, 1, 1);
            SetField("selectedUnit", striker);

            Invoke("ReactToAttack", tower, AttackOutcome.Hit, target, 0, 2);

            Assert.That(GetField("selectedUnit"), Is.SameAs(striker));
        }

        /// <summary>
        /// Seçili bir YAPI vurduğunda seçim kalır: o seçim aynı zamanda üretim
        /// panelini açık tutuyor.
        /// </summary>
        [Test]
        public void ReactToAttack_WithTheStructureItselfSelected_KeepsTheSelection()
        {
            InstallViewPool();
            Unit tower = PlaceTower(Team.Player, range: 2, x: 0, y: 0);
            Unit target = SpawnFighter("Raider", Team.Enemy, 0, 2);
            SetField("selectedUnit", tower);

            Invoke("ReactToAttack", tower, AttackOutcome.Hit, target, 0, 2);

            Assert.That(GetField("selectedUnit"), Is.SameAs(tower),
                "a selected structure keeps the production panel open");
        }

        // ══ KALDIRMA ═════════════════════════════════════════════════════
        // Dört soru, dördü de ayrı ayrı sınanıyor: düşmüş savaşçı, ayakta yapı,
        // enkaz, ve kaldırmanın bekleyen vuruşa etkisi.

        /// <summary>
        /// DÜŞMÜŞ bir savaşçı kaldırılabilir; kaldırma yaşam durumuna BAKMAZ.
        /// </summary>
        [Test]
        public void RemoveSelected_OnADownedFighter_StillRemovesIt()
        {
            InstallViewPool();

            var wounded = new Unit("Sapper");
            Combatant combatant = NewCombatant(Team.Player);
            Assert.That(adapter.PlaceUnit(wounded, combatant, 0, 0, null), Is.True);

            combatant.TakeDamage(MaxHealth);
            Assert.That(combatant.State, Is.EqualTo(UnitState.Downed), "setup");

            SetField("selectedUnit", wounded);

            Assert.That(adapter.RemoveSelected(), Is.True);
            Assert.That(battle.TryGetCombatant(wounded, out Combatant _), Is.False);
            Assert.That(UnitViews().ContainsKey(wounded), Is.False);
            Assert.That(battle.TryGetUnit(0, 0, out Unit _), Is.False, "the cell must be free again");
        }

        /// <summary>
        /// Bir YAPININ kaldırılması dört defteri birden temizler: tahta, görsel,
        /// can barı ve atış sayacı.
        /// </summary>
        // OLAY DA SINANIYOR: UnitRemoved yayılmasaydı üretim katmanı yıkılan
        // barakanın hattını sonsuza dek saymaya devam ederdi ve kimse fark
        // etmezdi — sızıntının adı IPlacementBoard'da yazılı.
        [Test]
        public void RemoveSelected_OnAStructure_ClearsEveryLedgerAndAnnouncesIt()
        {
            SetField("healthBarSprite", NewSprite());

            Unit tower = PlaceTower(Team.Player, range: 2, x: 0, y: 0);
            SetField("selectedUnit", tower);
            FireTimers()[tower] = 1.2f;

            Assert.That(HealthBars().ContainsKey(tower), Is.True, "setup: the tower got a bar");

            Unit announced = null;
            adapter.UnitRemoved += identity => announced = identity;

            // Destroy EditMode'da bir hata satırı basar ve bu testin ölçtüğü şey
            // değil; aynı beklenti enkaz temizliği testinde de duruyor.
            LogAssert.Expect(LogType.Error, new Regex("Destroy may not be called from edit mode"));

            Assert.That(adapter.RemoveSelected(), Is.True);

            Assert.That(battle.StructureCount, Is.EqualTo(0));
            Assert.That(StructureViews(), Is.Empty);
            Assert.That(FireTimers().ContainsKey(tower), Is.False);
            Assert.That(HealthBars().ContainsKey(tower), Is.False);
            Assert.That(announced, Is.SameAs(tower));
        }

        /// <summary>
        /// ENKAZ da kaldırılabilir: ayakta olmayan bir yapı hâlâ tahtada durur
        /// ve oyuncu onu temizleyebilmeli.
        /// </summary>
        [Test]
        public void RemoveSelected_OnRubble_StillRemovesIt()
        {
            Unit tower = PlaceTower(Team.Player, range: 1, x: 0, y: 0);
            Assert.That(battle.TryGetStructure(tower, out Structure structure), Is.True, "setup");
            Assert.That(structure.TakeDamage(StructureMaxHealth), Is.True, "setup: this hit destroys it");
            Assert.That(structure.IsStanding, Is.False, "setup: the tower is now rubble");

            SetField("selectedUnit", tower);

            LogAssert.Expect(LogType.Error, new Regex("Destroy may not be called from edit mode"));

            Assert.That(adapter.RemoveSelected(), Is.True);
            Assert.That(battle.StructureCount, Is.EqualTo(0));
            Assert.That(battle.TryGetUnit(0, 0, out Unit _), Is.False);
        }

        /// <summary>
        /// Kaldırılan HEDEF, kendisine yazılmış bekleyen vuruşu da götürür.
        /// </summary>
        // BIRAKILSAYDI EMİR BİR SONRAKİ KAREDE savaşta artık bulunmayan bir
        // kimliğe saldırı çağırırdı ve o çağrı bir oyun sonucu değil bir istisna
        // üretirdi.
        [Test]
        public void RemoveSelected_OnAPendingStrikeTarget_CancelsTheOrder()
        {
            InstallViewPool();
            Unit attacker = SpawnFighter("Striker", Team.Player, 0, 0);
            Unit target = SpawnFighter("Raider", Team.Enemy, 0, 1);

            SetField("selectedUnit", attacker);
            Invoke("SchedulePendingStrike", attacker, target, 0, 1);
            Assert.That(Invoke("PendingStrikeIsAlive"), Is.True, "setup");

            SetField("selectedUnit", target);
            Assert.That(adapter.RemoveSelected(), Is.True);

            Assert.That(GetField("pendingStrikeAttacker"), Is.Null);
            Assert.That(GetField("pendingStrikeTarget"), Is.Null);
        }

        // ══ GİRDİ TARAFINDA SALDIRI YIĞILMASI ════════════════════════════
        // Kuralın sahibi Core (RejectedOnCooldown); buradaki soru daha dar ve
        // ondan ÖNCE geliyor — aynı emir ikinci kez YAZILMASIN.

        /// <summary>
        /// Aynı hedefe gelen ikinci tıklama, yazılı emrin TEKRARIDIR.
        /// </summary>
        [Test]
        public void RepeatsPendingStrike_WithTheSameTargetAndTheAttackerSelected_IsTrue()
        {
            var attacker = new Unit("Striker");
            battle.AddUnit(attacker, NewCombatant(Team.Player), 0, 0);
            var target = new Unit("Raider");
            battle.AddUnit(target, NewCombatant(Team.Enemy), 0, 4);

            SetField("selectedUnit", attacker);
            Invoke("SchedulePendingStrike", attacker, target, 0, 4);

            Assert.That(Invoke("RepeatsPendingStrike", target), Is.True);
        }

        /// <summary>
        /// BAŞKA bir hedefe tıklamak fikir değiştirmektir; emir tekrarı değil.
        /// </summary>
        [Test]
        public void RepeatsPendingStrike_WithADifferentTarget_IsFalse()
        {
            var attacker = new Unit("Striker");
            battle.AddUnit(attacker, NewCombatant(Team.Player), 0, 0);
            var target = new Unit("Raider");
            battle.AddUnit(target, NewCombatant(Team.Enemy), 0, 4);
            var other = new Unit("Scout");
            battle.AddUnit(other, NewCombatant(Team.Enemy), 2, 4);

            SetField("selectedUnit", attacker);
            Invoke("SchedulePendingStrike", attacker, target, 0, 4);

            Assert.That(Invoke("RepeatsPendingStrike", other), Is.False);
        }

        /// <summary>
        /// Yazılı bir emir yokken hiçbir tıklama tekrar sayılmaz.
        /// </summary>
        // BU TEST BİR KİLİTLENMEYİ ÖNLÜYOR: koşul emirsiz durumda true dönseydi
        // dolu hücreye yapılan her tıklama sessizce tüketilir ve oyuncu hiçbir
        // şey seçemezdi.
        [Test]
        public void RepeatsPendingStrike_WithNoOrderWritten_IsFalse()
        {
            var target = new Unit("Raider");
            battle.AddUnit(target, NewCombatant(Team.Enemy), 0, 4);

            Assert.That(Invoke("RepeatsPendingStrike", target), Is.False);
            Assert.That(Invoke("RepeatsPendingStrike", new object[] { null }), Is.False);
        }

        /// <summary>
        /// Saldırı profili TAŞIYAN bir yapıyı tahtaya koyar ve kimliğini verir.
        /// </summary>
        // AYRI BİR YARDIMCI, ÇÜNKÜ CommitPlacement KULLANILAMAZ: o yol yapıyı
        // NewStructure ile kuruyor ve NewStructure bilerek saldırı profili
        // vermiyor (saldırmayan yapı KURALdır). Ateş eden yapıyı sınamak için
        // istisnayı elle kurmak gerekiyor.
        private Unit PlaceTower(Team team, int range, int x, int y)
        {
            var identity = new Unit($"Tower_{x}_{y}");
            var tower = new Structure(
                new Health(StructureMaxHealth),
                new StructureLifecycle(),
                team,
                new AttackProfile(Damage, range));

            // TAM NİTELİKLİ AD, dosya başına bir using DEĞİL: bu dosyanın
            // tepesindeki takma ad çıplak "Battle" kelimesini kurtarıyor ve
            // GridStrategy.Battle ad alanını toptan içeri almak o kurtarmayı
            // gereksiz yere sınardı. Tek bir enum için bedeli buna değmez.
            Assert.That(adapter.PlaceStructure(identity, tower, x, y),
                Is.EqualTo(global::GridStrategy.Battle.PlacementOutcome.Placed),
                "setup: the tower must reach the board");

            return identity;
        }

        /// <summary>
        /// Inspector'daki sayıların test karşılığı. Ayrı bir metot çünkü üç
        /// testte üç kez kurulması gerekiyor ve kopyalanan sayılar birbirinden
        /// ayrıldığı gün hiçbir şey patlamaz, yalnızca testler anlamsızlaşırdı.
        /// </summary>
        private static Combatant NewCombatant(Team team)
        {
            return new Combatant(
                new Health(MaxHealth),
                new UnitLifecycle(),
                new AttackProfile(Damage, AttackRange),
                team);
        }

        /// <summary>
        /// Bir savaşçıyı <c>Dead</c> hâline getirir: önce canını bitirir, sonra
        /// kurtarma penceresini doldurur. İKİ ADIM ZORUNLU — tek vuruş yalnızca
        /// düşürür, öldürmez.
        /// </summary>
        private void KillOutright(Combatant combatant)
        {
            combatant.TakeDamage(MaxHealth);
            battle.Tick(UnitLifecycle.DefaultDownedWindowSeconds);
            Assert.That(combatant.State, Is.EqualTo(UnitState.Dead), "setup: the combatant must be permanently dead");
        }

        /// <summary>
        /// Görsel havuzunu kurar ve tahtaya takar.
        /// </summary>
        // AWAKE HİÇ KOŞMADIĞI İÇİN viewPool null DOĞAR ve PlaceUnit'e giden her
        // yol NullReferenceException verirdi. SetUp'ta değil, AYRI bir üyede:
        // her testte kurmak, havuza hiç uğramayan yirmi testin sahnesine de bir
        // prefab bırakırdı.
        //
        // TAKIM KARELERİ ATANIYOR ve bu bir konfor değil ölçünün kendisi:
        // "üretilen birimin gövdesi takım karesini EZİYOR mu" sorusu, ancak
        // ezilecek bir kare varken sınanabilir.
        private void InstallViewPool()
        {
            var prefabObject = new GameObject("UnitPrefab");
            prefabObject.AddComponent<SpriteRenderer>();
            unitPrefab = prefabObject.AddComponent<UnitView>();

            teamIdle = NewSprite();
            SetHiddenOn(unitPrefab, "friendlyIdle", teamIdle);
            SetHiddenOn(unitPrefab, "friendlyAttacking", NewSprite());
            SetHiddenOn(unitPrefab, "enemyIdle", teamIdle);
            SetHiddenOn(unitPrefab, "enemyAttacking", NewSprite());

            SetField("viewPool", new UnitViewPool(unitPrefab, probe.transform));
        }

        /// <summary>
        /// Havuzdan görseli olan bir savaşçıyı tahtaya koyar ve kimliğini verir.
        /// </summary>
        // GÖRSELİ OLMASI ŞART: saldırı gösterimi TryGetView'a uğruyor ve görseli
        // olmayan bir savaşçı için o üye LogError basar — testin ölçmediği bir
        // kırmızı.
        private Unit SpawnFighter(string name, Team team, int x, int y)
        {
            var identity = new Unit(name);
            Assert.That(adapter.PlaceUnit(identity, NewCombatant(team), x, y, null), Is.True,
                $"setup: '{name}' must reach ({x},{y})");
            return identity;
        }

        /// <summary>
        /// Bir kimliğin ekrandaki gövde görselini verir.
        /// </summary>
        private Sprite BodyOf(Unit identity)
        {
            Assert.That(UnitViews().TryGetValue(identity, out UnitView view), Is.True,
                $"no view registered for '{identity.Name}'");
            return view.GetComponent<SpriteRenderer>().sprite;
        }

        /// <summary>
        /// Bir kez kullanılıp atılan geçici sprite üretir.
        /// </summary>
        // DOKU DA TOPLANIYOR: Sprite.Create dokuya bir ok tutuyor ve yalnız
        // sprite yok edilirse doku sahnede kalırdı.
        private Sprite NewSprite()
        {
            var texture = new Texture2D(4, 4);
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f));

            disposables.Add(sprite);
            disposables.Add(texture);
            return sprite;
        }

        /// <summary>
        /// Savaşçı görselleri tablosunu yansımayla verir.
        /// </summary>
        private Dictionary<Unit, UnitView> UnitViews()
        {
            return (Dictionary<Unit, UnitView>)GetField("unitViews");
        }

        /// <summary>
        /// Yapıların atış sayacı tablosunu yansımayla verir.
        /// </summary>
        private Dictionary<Unit, float> FireTimers()
        {
            return (Dictionary<Unit, float>)GetField("structureFireTimers");
        }

        /// <summary>
        /// Can barı tablosunu yansımayla verir.
        /// </summary>
        private Dictionary<Unit, HealthBarView> HealthBars()
        {
            return (Dictionary<Unit, HealthBarView>)GetField("healthBars");
        }

        /// <summary>
        /// Tahtanın görsel havuzunu yansımayla verir.
        /// </summary>
        private UnitViewPool ViewPool()
        {
            return (UnitViewPool)GetField("viewPool");
        }

        /// <summary>
        /// Prefab üstündeki gizli bir alana yazar.
        /// </summary>
        // AYRI BİR ÜYE, ÇÜNKÜ HEDEF FARKLI: SetField tahtaya yazıyor, bu ise
        // görsele. İkisi tek üyede birleştirilseydi çağıran her seferinde tipi
        // de söylemek zorunda kalırdı.
        private static void SetHiddenOn(UnitView target, string name, object value)
        {
            FieldInfo field = typeof(UnitView).GetField(name, Hidden);
            Assert.That(field, Is.Not.Null, $"UnitView has no private field named '{name}'");
            field.SetValue(target, value);
        }

        /// <summary>
        /// Yapı görselleri tablosunu yansımayla verir.
        /// </summary>
        private Dictionary<Unit, GameObject> StructureViews()
        {
            FieldInfo field = typeof(BoardAdapter).GetField("structureViews", Hidden);
            Assert.That(field, Is.Not.Null, "BoardAdapter no longer has a 'structureViews' field");
            return (Dictionary<Unit, GameObject>)field.GetValue(adapter);
        }

        private GameObject FindChild(string name)
        {
            Transform child = probe.transform.Find(name);
            Assert.That(child, Is.Not.Null, $"no child GameObject named '{name}'");
            return child.gameObject;
        }

        // ADI BULAMAYINCA SESSİZ GEÇMEK YASAK. Yansıma yanlış bir ada
        // sorulduğunda null döner; iddia olmadan SetValue bir
        // NullReferenceException'a, Invoke ise sessiz bir hiçliğe dönerdi ve
        // testin neyi ölçtüğü kaybolurdu.
        private void SetField(string name, object value)
        {
            FieldInfo field = typeof(BoardAdapter).GetField(name, Hidden);
            Assert.That(field, Is.Not.Null, $"BoardAdapter has no private field named '{name}'");
            field.SetValue(adapter, value);
        }

        private object GetField(string name)
        {
            FieldInfo field = typeof(BoardAdapter).GetField(name, Hidden);
            Assert.That(field, Is.Not.Null, $"BoardAdapter has no private field named '{name}'");
            return field.GetValue(adapter);
        }

        private object Invoke(string name, params object[] arguments)
        {
            MethodInfo method = typeof(BoardAdapter).GetMethod(name, Hidden);
            Assert.That(method, Is.Not.Null, $"BoardAdapter has no private method named '{name}'");
            return method.Invoke(adapter, arguments);
        }

        /// <summary>
        /// <c>out</c> parametreli özel bir üyeyi çağırır ve dolan argümanları
        /// çağırana bırakır.
        /// </summary>
        // AYRI BİR ÜYE, ÇÜNKÜ Invoke DİZİYİ GERİ VERMİYOR: params imzası
        // çağıranın elinde bir dizi bırakmıyor ve yansıma out değerlerini tam
        // olarak o diziye yazıyor. İkisi tek üyede birleştirilseydi sıradan
        // çağrılar da her seferinde bir dizi kurmak zorunda kalırdı.
        private object InvokeWithArguments(string name, object[] arguments)
        {
            MethodInfo method = typeof(BoardAdapter).GetMethod(name, Hidden);
            Assert.That(method, Is.Not.Null, $"BoardAdapter has no private method named '{name}'");
            return method.Invoke(adapter, arguments);
        }
    }
}
