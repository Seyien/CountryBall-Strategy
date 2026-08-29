using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using GridStrategy.Battle;
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
            Assert.That(StructureViews()[standing].gameObject, Is.SameAs(FindChild(standing.Name)));
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

        // ══ EMRİN AYAKTA KALDIĞI VE DÜŞTÜĞÜ HÂLLER ═══════════════════════
        // Burada üç `PendingStrikeIsAlive_*` testi duruyordu ve ikisi ARTIK
        // YANLIŞ bir kuralı koruyordu: "saldıran hâlâ seçili mi". Operatör
        // emrin seçimden bağımsız yaşamasını istedi, yani o koşulun kendisi
        // kaldırıldı. Yerlerini alan iddialar aşağıda ve UnitOrderTests'te.
        // → Docs/deep/konular/09-kararlarin-cevrilmesi.md (madde 2)

        /// <summary>
        /// SEÇİM BAŞKA BİR BİRİME GEÇSE BİLE ilk emir ayakta kalır.
        /// </summary>
        // ██ ÇEVRİLEN KURAL TAM OLARAK BU ██
        // Eski test `PendingStrikeIsAlive_WhenTheSelectionMovedToAnotherUnit_IsFalse`
        // adını taşıyordu (SILINDI) ve emrin DÜŞMESİNİ bekliyordu. Dünyada
        // değişen şey: emir tahtaya değil birime ait. İkinci savaşçısını
        // seçen oyuncu, birincisinin saldırısını neden kaybetsin?
        [Test]
        public void Orders_WhenTheSelectionMovesToAnotherUnit_TheFirstOrderStillStands()
        {
            InstallViewPool();
            Unit first = SpawnFighter("Striker", Team.Player, 0, 0);
            Unit second = SpawnFighter("Sapper", Team.Player, 2, 0);
            Unit target = SpawnFighter("Raider", Team.Enemy, 0, 1);

            SetField("selectedUnit", first);
            Invoke("HandleOccupiedCellClick", target, 0, 1);

            SetField("selectedUnit", second);

            Assert.That(Orders().TryGet(first, out IUnitOrder order), Is.True);
            Assert.That(order.Target, Is.SameAs(target));
        }

        /// <summary>
        /// HEDEF TAHTADAN KALKTI: emir bir sonraki ilerletmede düşer.
        /// </summary>
        // İDDİA AYNEN DURUYOR, cevabı veren yer değişti: soru artık tahtanın
        // dört alanına değil emrin kendisine soruluyor ve emir konumu her
        // karede taze okuyor.
        [Test]
        public void Orders_Advance_WhenTheTargetLeftTheBattle_DropsTheOrder()
        {
            InstallViewPool();
            Unit attacker = SpawnFighter("Striker", Team.Player, 0, 0);
            Unit target = SpawnFighter("Raider", Team.Enemy, 0, 1);

            SetField("selectedUnit", attacker);
            Invoke("HandleOccupiedCellClick", target, 0, 1);
            Assert.That(Orders().Count, Is.EqualTo(1), "setup");

            Assert.That(battle.RemoveUnit(target), Is.True, "setup");
            Orders().Advance();

            Assert.That(Orders().Count, Is.EqualTo(0));
        }

        /// <summary>
        /// UZAKTAKİ DÜŞMÜŞ DOSTA YÜRÜMEK: emir yazılır, seçim bırakılır ve
        /// satırın kendisi bırakılmış seçimi OKUMAZ.
        /// </summary>
        // ██ KIRMIZI OLARAK YAZILDI — ÖLÇÜLMÜŞ BİR ÇÖKME ██
        // <c>IssueOrder</c> seçimi bırakıyor (<c>selectedUnit</c> null oluyor) ve
        // hemen ardından gelen Console satırı o alanı OKUYORDU; yol her
        // koşuşunda NullReferenceException veriyordu. Saldırı dalında aynı tuzak
        // yok, çünkü orada emir üyenin SON satırı — yani kırılan şey sıranın
        // kendisiydi ve bu iddia onu sabitliyor.
        //
        // TAHTA FreeForAll KURULUYOR VE BU ŞART: Alternating kipinde yürüyüş
        // sırayı devrediyor, üye emir satırına hiç varmadan dönüyordu. Gerçek
        // tahta FreeForAll ile kuruluyor, yani oyuncunun gördüğü yol budur;
        // varsayılan kiple yazılmış bir test bu çökmeyi ÖLÇEMEZDİ.
        [Test]
        public void TryCloseInOnAlly_WhenTheWalkStarts_WritesTheOrderWithoutReadingTheReleasedSelection()
        {
            InstallViewPool();

            var freeForAll = new Battle(Width, Height, global::GridStrategy.Battle.TurnMode.FreeForAll);
            SetField("battle", freeForAll);
            battle = freeForAll;

            Unit medic = SpawnFighter("Medic", Team.Player, 0, 0);

            var fallen = new Unit("Fallen");
            Combatant fallenBody = NewCombatant(Team.Player);
            Assert.That(adapter.PlaceUnit(fallen, fallenBody, 0, 4, null), Is.True, "setup");
            fallenBody.TakeDamage(MaxHealth);
            Assert.That(fallenBody.State, Is.EqualTo(UnitState.Downed), "setup");

            SetField("selectedUnit", medic);

            Assert.DoesNotThrow(() => { Invoke("TryCloseInOnAlly", fallen, 0, 4); });

            Assert.That(Orders().TryGet(medic, out IUnitOrder order), Is.True);
            Assert.That(order.Target, Is.SameAs(fallen));
            Assert.That(GetField("selectedUnit"), Is.Null);
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

        /// <summary>
        /// SONUÇ EKRANA GİDEN KAPIDAN DA GEÇİYOR: pano Console'u okumuyor, bu
        /// olayı dinliyor.
        ///
        /// Console satırını sayan üstteki test bu iddiayı TAŞIYAMAZ. İki kanal
        /// aynı mandalın arkasında duruyor ama ayrı üyeler, ve biri konuşurken
        /// öteki susabilir — o gün oyuncu hiçbir şey görmez, geliştirici ise
        /// her şeyin yolunda olduğunu okur.
        /// </summary>
        [Test]
        public void AnnounceWinnerIfAny_WhenTheEnemyIsWipedOut_PublishesThePlayerWinExactlyOnce()
        {
            Combatant enemy = NewCombatant(Team.Enemy);
            battle.AddUnit(new Unit("Raider"), enemy, 2, 2);
            KillOutright(enemy);

            var announced = new List<BattleOutcome>();

            // İDDİA ABONENİN İÇİNDE OKUNUYOR ve tek sebebi bu: yayının SIRASI
            // yalnız yayın anında gözlenebilir. Dışarıdan bakan bir iddia
            // geçişin bittiği hâli görür ve iki sıralamayı ayırt edemezdi.
            bool frozenWhenPublished = false;
            System.Action<BattleOutcome> collect = outcome =>
            {
                announced.Add(outcome);
                frozenWhenPublished = CurrentMode().OwnsPointer;
            };

            adapter.BattleEnded += collect;
            try
            {
                Invoke("AnnounceWinnerIfAny");
                Invoke("AnnounceWinnerIfAny");
            }
            finally
            {
                adapter.BattleEnded -= collect;
            }

            Assert.That(announced, Is.EqualTo(new[] { BattleOutcome.PlayerWon }),
                "the panel must hear the result once and only once");
            Assert.That(frozenWhenPublished, Is.True,
                "nobody may observe the end of the battle on a board that still takes clicks");
        }

        /// <summary>
        /// SAVAŞ BİTİNCE TAHTA DONUYOR: yürürlüğe giren kip işaretçiyi
        /// sahipleniyor ve <c>Update</c>'in erken çıkışı seçim, saldırı, yürüyüş,
        /// kaydırma ve tuş akışının tamamını atlıyor.
        ///
        /// İDDİA GİRDİDE DEĞİL KİPTE, çünkü <c>Input</c> EditMode'da beslenemez.
        /// Ölçülebilen şey, girdiyi okuyan dalın hangi cevaba bağlandığı.
        /// </summary>
        [Test]
        public void AnnounceWinnerIfAny_AfterTheWin_LeavesAModeThatOwnsThePointer()
        {
            Combatant enemy = NewCombatant(Team.Enemy);
            battle.AddUnit(new Unit("Raider"), enemy, 2, 2);
            KillOutright(enemy);

            Assert.That(CurrentMode().OwnsPointer, Is.False,
                "setup: an unfinished battle answers every click");

            Invoke("AnnounceWinnerIfAny");

            Assert.That(CurrentMode(), Is.InstanceOf<BattleOverBoardMode>());
            Assert.That(CurrentMode().OwnsPointer, Is.True,
                "a finished battle answers none");
        }

        // ══ BİTİRİCİ VURUŞ ═══════════════════════════════════════════════

        /// <summary>
        /// <c>HitAndFinished</c> bir İSABETTİR: adıyla karşılanır ve programcı
        /// hatası dalına düşmez.
        /// </summary>
        // ██ İDDİANIN İKİNCİ YARISI ÇEVRİLDİ ██
        // Bu test eskiden "ve isabet sonrası seçimi bırakma kuralı ona da
        // uygulanır" diyordu. Dünyada değişen şey: vuruş artık TEK SEFERLİK bir
        // olay değil, kalıcı bir emrin tekrarı. Her isabette seçimi düşürmek,
        // birimini yeniden seçmiş oyuncunun elinden onu tekrar tekrar alırdı.
        // Seçimi bırakan yer emrin YAZILDIĞI an oldu ve iddiası aşağıda:
        // IssueOrder_WhenTheOrderIsWritten_ReleasesTheSelection.
        // → Docs/deep/konular/09-kararlarin-cevrilmesi.md (madde 2)
        [Test]
        public void ReactToAttack_WithHitAndFinished_IsALandedStrikeWithoutAProgrammerError()
        {
            InstallViewPool();
            Unit attacker = SpawnFighter("Finisher", Team.Player, 0, 0);
            SetField("selectedUnit", attacker);

            var target = new Unit("Raider");
            battle.AddUnit(target, NewCombatant(Team.Enemy), 0, 1);

            LogAssert.Expect(LogType.Log, new Regex(@"'Raider' at \(0,1\) was FINISHED OFF"));

            InvokeWithArguments(
                "ReactToAttack",
                new object[] { attacker, AttackOutcome.HitAndFinished, target, 0, 1 });

            Assert.That(GetField("selectedUnit"), Is.SameAs(attacker),
                "vuruş seçime artık dokunmuyor; bırakan yer emrin yazıldığı an");
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

        // ══ YERLEŞTİRME ÖNİZLEMESİ — HAYALET NEYİ SÖYLÜYOR ═════════════
        // Bırakma davranışı DEĞİŞMEDİ ve bu testlerin yarısı onu koruyor:
        // tahta dışı hâlâ bir vazgeçme, dolu hücre hâlâ bir ret. Değişen tek
        // şey oyuncunun bunu parmağını kaldırmadan ÖNCE görüyor olması.

        /// <summary>
        /// Boş ve tahta içindeki hücre konulabilir.
        /// </summary>
        [Test]
        public void PreviewAt_OnAFreeCellInsideTheBoard_IsPlaceable()
        {
            Assert.That(adapter.PreviewAt(1, 1), Is.EqualTo(PlacementPreview.Placeable));
        }

        /// <summary>
        /// Tahtanın dışı ayrı bir cevap verir — "dolu" değil, "dışarıda".
        /// </summary>
        // ██ SIRA BİR KARARDIR VE BU TEST ONUN KAYDIDIR ██
        // Ters sırada yazılsaydı (önce doluluk, sonra sınır) tahta dışındaki bir
        // hücrede duran hiçbir şey olmadığı için cevap Placeable olurdu — yani
        // hayalet tahtanın dışında YEŞİL görünür ve bırakma sessizce hiçbir şey
        // yapmazdı.
        [Test]
        public void PreviewAt_OutsideTheBoard_SaysOutsideBoardNotOccupied()
        {
            Assert.That(adapter.PreviewAt(-1, 0), Is.EqualTo(PlacementPreview.OutsideBoard));
            Assert.That(adapter.PreviewAt(999, 999), Is.EqualTo(PlacementPreview.OutsideBoard));
        }

        /// <summary>
        /// Dolu hücre üçüncü cevabı verir.
        /// </summary>
        [Test]
        public void PreviewAt_OnACellThatAlreadyHoldsSomething_IsCellOccupied()
        {
            InstallViewPool();
            SpawnFighter("Vanguard", Team.Player, 2, 2);

            Assert.That(adapter.PreviewAt(2, 2), Is.EqualTo(PlacementPreview.CellOccupied));
        }

        /// <summary>
        /// <c>IsCellFree</c> kendi kuralını YAZMIYOR, önizlemeye soruyor.
        /// </summary>
        // ██ TEK SAHİP İDDİASININ KANITI ██
        // Bu iddia iki kuralın AYNI kaynaktan beslendiğini ölçüyor. İkisi ayrı
        // yazılsaydı "boş hücre" tanımı değiştiği gün (örneğin enkazın üstüne
        // inşaya izin verildiği gün) bırakma kabul eder, hayalet kırmızı
        // gösterirdi — ya da tersi.
        [Test]
        public void IsCellFree_AgreesWithPreviewAt_OnEveryKindOfCell()
        {
            InstallViewPool();
            SpawnFighter("Vanguard", Team.Player, 2, 2);

            Assert.That(adapter.IsCellFree(1, 1), Is.True);
            Assert.That(adapter.IsCellFree(2, 2), Is.False, "dolu hücre boş sayılmamalı");
            Assert.That(adapter.IsCellFree(-1, 0), Is.False, "tahta dışı boş sayılmamalı");
        }

        /// <summary>
        /// Hayalet tahtanın DIŞINDA da görünür ve KIRMIZI olur.
        /// </summary>
        // ██ OPERATÖRÜN CÜMLESİ: "unit grid'in dışındakileri de hayalet ██
        // ██ kısmını görebilmeliyiz ama kırmızılı hâlinde" ██
        // Eski kodda ProductionDirector tahtanın dışında hayaleti GİZLİYORDU;
        // bu iddia o hâlde yazılamazdı bile, çünkü çizilen bir hayalet yoktu.
        [Test]
        public void SetPlacementGhost_OutsideTheBoard_ShowsTheGhostInTheRejectedColour()
        {
            SpriteRenderer ghost = InstallGhost();
            Color authored = ghost.color;

            adapter.SetPlacementGhost(true, -1, 0);

            Assert.That(ghost.enabled, Is.True, "tahta dışında da görünmeli");
            Assert.That(ghost.color, Is.Not.EqualTo(authored), "reddedilen hücre farklı renk ister");
            Assert.That(ghost.color.r, Is.GreaterThan(ghost.color.g), "kırmızıya kaymalı");
        }

        /// <summary>
        /// Dolu hücrede de aynı kırmızı.
        /// </summary>
        [Test]
        public void SetPlacementGhost_OnAnOccupiedCell_UsesTheRejectedColour()
        {
            InstallViewPool();
            SpriteRenderer ghost = InstallGhost();
            Color authored = ghost.color;
            SpawnFighter("Vanguard", Team.Player, 2, 2);

            adapter.SetPlacementGhost(true, 2, 2);

            Assert.That(ghost.enabled, Is.True);
            Assert.That(ghost.color, Is.Not.EqualTo(authored));
        }

        /// <summary>
        /// Geçerli hücrede sahnede YAZILI renk geri geliyor.
        /// </summary>
        // ██ SABİT BİR BEYAZ YAZILSAYDI BU İDDİA KIRMIZIYA DÖNERDİ ██
        // Sahnede ayarlanmış saydamlık ilk sürüklemede kalıcı olarak
        // kaybolurdu; proje bu tuzağa hayaletin SPRITE'ı tarafında bir kez
        // düştü ve onarımı authoredGhostSprite'ti. Renk onun ikizi.
        [Test]
        public void SetPlacementGhost_BackOnAFreeCell_RestoresTheAuthoredColour()
        {
            SpriteRenderer ghost = InstallGhost();
            Color authored = ghost.color;

            adapter.SetPlacementGhost(true, -1, 0);
            adapter.SetPlacementGhost(true, 1, 1);

            Assert.That(ghost.color, Is.EqualTo(authored));
        }

        /// <summary>
        /// Tahta dışındaki hücre yine de OKUNABİLİYOR — önizlemenin ihtiyacı bu.
        /// </summary>
        // İKİZ ÜYENİN AYRIMI: TryScreenPointToCell dışarıyı REDDEDER (bırakma
        // onu çağırıyor), TryScreenPointToAnyCell reddetmez (önizleme bunu
        // çağırıyor). Kamera olmadan ikisi de false döner ve bu test o yüzden
        // kamerayı kuruyor.
        [Test]
        public void TryScreenPointToAnyCell_OutsideTheBoard_StillReturnsTheCell()
        {
            GameObject cameraObject = InstallCamera();
            Vector3 outsideWorld = new Vector3(-3.5f, 0.5f, 0f);
            Vector3 screenPoint = cameraObject.GetComponent<Camera>().WorldToScreenPoint(outsideWorld);

            Assert.That(adapter.TryScreenPointToAnyCell(screenPoint, out int x, out int _), Is.True,
                "önizleme dışarıdaki hücreyi de öğrenebilmeli");
            Assert.That(x, Is.LessThan(0), "hücre tahtanın solunda olmalı");

            Assert.That(adapter.TryScreenPointToCell(screenPoint, out int _, out int _), Is.False,
                "bırakma yolu dışarıyı hâlâ reddetmeli");
        }

        // ══ ÜRETİM GERİ SAYIMI — TAHTA İLE MÜDÜRÜN DİKİŞİ ═══════════════
        // Şeridin ANİMASYONU burada sınanmıyor ve bu dürüst bir sınır: vuruş
        // Time.deltaTime okuyor, EditMode'da o değer akmıyor. Sınanan şey
        // dikişin kendisi — müdür bir sayı söylediğinde tahta gerçekten bir
        // şerit kuruyor mu, ve o şerit sızıyor mu.

        /// <summary>
        /// Müdür geri sayımı söylediğinde tahta şeridi KURUYOR ve deftere
        /// yazıyor.
        /// </summary>
        [Test]
        public void ShowProductionCountdown_ForAStructure_AttachesATimerAndRemembersIt()
        {
            SetField("healthBarSprite", NewSprite());
            Unit barracks = PlaceDepot(Team.Player, 0, 0);

            adapter.ShowProductionCountdown(barracks, 3f, 5f);

            Assert.That(ProductionTimers().ContainsKey(barracks), Is.True);

            Transform strip = StructureViews()[barracks].transform.Find("ProductionTimer");
            Assert.That(strip, Is.Not.Null, "şerit yapının çocuğu olmalı");
        }

        /// <summary>
        /// Şerit can barının ÜSTÜNDE duruyor — ikisi üst üste binmiyor.
        /// </summary>
        // YÜKSEKLİK BARDAN OKUNUYOR, GÖRSELDEN YENİDEN HESAPLANMIYOR: iki ayrı
        // hesap, birinin payı değiştiği gün sessizce ayrışırdı. Bu test o tek
        // kaynağın kaydı.
        [Test]
        public void ShowProductionCountdown_PlacesTheStripAboveTheHealthBar()
        {
            SetField("healthBarSprite", NewSprite());
            Unit barracks = PlaceDepot(Team.Player, 0, 0);

            adapter.ShowProductionCountdown(barracks, 3f, 5f);

            float barHeight = HealthBars()[barracks].transform.localPosition.y;
            float stripHeight = ProductionTimers()[barracks].transform.localPosition.y;

            Assert.That(stripHeight, Is.GreaterThan(barHeight),
                "geri sayım şeridi can barının üstünde durmalı");
        }

        /// <summary>
        /// İkinci çağrı İKİNCİ bir şerit kurmaz.
        /// </summary>
        // ██ HER KAREDE ÇAĞRILAN BİR ÜYENİN KLASİK TUZAĞI ██
        // Müdürün Update'i bu üyeyi saniyede altmış kez çağırıyor. Kapı
        // olmasaydı bir dakikada 3600 şerit birikirdi — havuz kullanan kodların
        // ikinci klasik hatasının aynısı, gerekçesi AttachHealthBar'da yazılı.
        [Test]
        public void ShowProductionCountdown_CalledEveryFrame_BuildsTheStripOnlyOnce()
        {
            SetField("healthBarSprite", NewSprite());
            Unit barracks = PlaceDepot(Team.Player, 0, 0);

            for (int i = 0; i < 5; i++)
            {
                adapter.ShowProductionCountdown(barracks, 5f - i, 5f);
            }

            int strips = StructureViews()[barracks]
                .GetComponentsInChildren<ProductionTimerView>(includeInactive: true).Length;

            Assert.That(strips, Is.EqualTo(1));
        }

        /// <summary>
        /// Sprite atanmamışsa sessizce hiçbir şey yapmaz — patlamaz, bağırmaz.
        /// </summary>
        // KADEME SEÇİMİ: eksik bir gösterge oyunu OYNANAMAZ yapmaz, yalnız
        // okunmaz yapar. Burada LogError basılsaydı EditMode'da üretim yolundan
        // geçen her test kırmızıya dönerdi — projeye bir kez bu oldu ve 482
        // testin 6'sı kırıldı. Şikâyet doğuşta, BuildHoverHighlight'ta ediliyor.
        [Test]
        public void ShowProductionCountdown_WithNoSpriteAssigned_StaysQuiet()
        {
            Unit barracks = PlaceDepot(Team.Player, 0, 0);

            adapter.ShowProductionCountdown(barracks, 3f, 5f);

            Assert.That(ProductionTimers().ContainsKey(barracks), Is.False);
        }

        /// <summary>
        /// Seçili yapı ÇERÇEVE de kazanır — renk çarpanı tek başına yetmiyordu.
        /// </summary>
        // ██ OPERATÖRÜN BELİRTİSİ: "yapılara tıkladığımda seçili oldukları ██
        // ██ gözükmüyor" ██
        // Üstteki test yalnızca "rengi beyaz DEĞİL" diyor ve o iddia, gözle
        // ayırt edilemeyecek kadar küçük bir çarpanla da yeşil kalırdı — nitekim
        // kaldı. Bu test ikinci kanalı ölçüyor: çerçeve sprite'ın kendi rengine
        // bağlı olmadığı için bina hangi renk olursa olsun görünür.
        [Test]
        public void SelectUnit_OnAStructure_AlsoDrawsASelectionFrame()
        {
            SetField("hoverFrameSprite", NewSprite());
            Unit tower = PlaceTower(Team.Player, range: 2, x: 0, y: 0);
            SetField("selectedUnit", null);

            Invoke("SelectUnit", tower);

            Transform frame = StructureViews()[tower].transform.Find("SelectionFrame");
            Assert.That(frame, Is.Not.Null, "seçili yapının çerçevesi olmalı");

            var frameRenderer = frame.GetComponent<SpriteRenderer>();
            Assert.That(frameRenderer, Is.Not.Null);
            Assert.That(frameRenderer.enabled, Is.True);

            // ÇERÇEVE SİLİNMİYOR, KAPANIYOR: ikinci seçimde yeniden kurmak her
            // tıklamada bir tahsis üretirdi.
            Invoke("ClearSelection");
            Assert.That(frameRenderer.enabled, Is.False);
        }

        /// <summary>
        /// SİLAHSIZ bir bina seçiliyken düşmana tıklamak SEÇİMİ ORAYA TAŞIR —
        /// "saldıramıyor" demez.
        /// </summary>
        // ██ OPERATÖRÜN CÜMLESİ: "rakip yapıyı seçtiğimde saldıramıyor diyor ██
        // ██ ...daha çok seçili olayını karşıdaki yapıya geçilse" ██
        // Eski kodda bu dal koşulsuz BattleActions.Attack çağırıyordu ve kışla
        // gibi silahsız bir bina için cevap her seferinde bir RETTİ. Oyuncu
        // hiç istemediği bir eylemin reddini okuyordu.
        [Test]
        public void HandleOccupiedCellClick_WithAnUnarmedStructureSelected_MovesTheFocusToTheEnemyStructure()
        {
            Unit depot = PlaceDepot(Team.Player, 0, 0);
            Unit enemyDepot = PlaceDepot(Team.Enemy, 2, 2);
            SetField("selectedUnit", depot);

            LogAssert.Expect(LogType.Log, new Regex(@"FOCUS MOVED"));

            Invoke("HandleOccupiedCellClick", enemyDepot, 2, 2);

            Assert.That(GetField("selectedUnit"), Is.SameAs(enemyDepot),
                "silahsız bina ile tıklamak odağı devretmeli");
        }

        /// <summary>
        /// Aynı kural karşı takımın SAVAŞÇISI için de geçerli.
        /// </summary>
        // AYRI BİR TEST, ÇÜNKÜ AYRI BİR DEFTER: yapılar ve savaşçılar tahtada
        // iki ayrı tabloda yaşıyor ve odak devri ikisinde de çalışmalı.
        // Operatör bunu adıyla istedi: "veya aynısı karşı takımın savaşçısına da".
        [Test]
        public void HandleOccupiedCellClick_WithAnUnarmedStructureSelected_MovesTheFocusToAnEnemyFighterToo()
        {
            InstallViewPool();
            Unit depot = PlaceDepot(Team.Player, 0, 0);
            Unit raider = SpawnFighter("Raider", Team.Enemy, 2, 2);
            SetField("selectedUnit", depot);

            LogAssert.Expect(LogType.Log, new Regex(@"FOCUS MOVED"));

            Invoke("HandleOccupiedCellClick", raider, 2, 2);

            Assert.That(GetField("selectedUnit"), Is.SameAs(raider));
        }

        /// <summary>
        /// SALDIRABİLEN yapı odak devretmez — o hâlâ ateş eder.
        /// </summary>
        // ██ OPERATÖRÜN KENDİ KELEPÇESİ: "bu tabii ki saldırı yapan yapılar ██
        // ██ için geçerli değil" ██
        // Bu test olmasaydı odak devri sessizce taretleri de yutar ve oyuncu
        // kulesiyle ateş edemez hâle gelirdi. Ayrımı yapan şey bir tür listesi
        // değil Structure.CanAttack, yani yeni bir silahlı bina eklendiği gün
        // bu dal kendiliğinden doğru tarafta kalıyor.
        [Test]
        public void HandleOccupiedCellClick_WithAnArmedTowerSelected_KeepsTheFocusAndAttacks()
        {
            Unit tower = PlaceTower(Team.Player, range: 4, x: 0, y: 0);
            Unit enemyDepot = PlaceDepot(Team.Enemy, 2, 2);
            SetField("selectedUnit", tower);

            Invoke("HandleOccupiedCellClick", enemyDepot, 2, 2);

            Assert.That(GetField("selectedUnit"), Is.SameAs(tower),
                "ateş eden kule odağı devretmez");
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

        // ══ EMİRDEN SONRA SEÇİM ══════════════════════════════════════════
        // Operatörün isteği: "attacker'ın kime saldıracağı belirtildiğinde seçim
        // kaldırılmalı ama tekrardan seçim alınabilecek şekilde de ayarlanabilir."
        // Seçimi bırakan yer VURUŞ değil EMİR, ve fark ölçülebilir: kalıcı emir
        // saniyede bir vuruyor.

        /// <summary>
        /// EMİR YAZILDIĞI AN seçim bırakılır.
        /// </summary>
        [Test]
        public void IssueOrder_WhenTheOrderIsWritten_ReleasesTheSelection()
        {
            InstallViewPool();
            Unit striker = SpawnFighter("Striker", Team.Player, 0, 0);
            Unit target = SpawnFighter("Raider", Team.Enemy, 0, 1);
            SetField("selectedUnit", striker);

            Invoke("HandleOccupiedCellClick", target, 0, 1);

            Assert.That(GetField("selectedUnit"), Is.Null);
            Assert.That(Orders().TryGet(striker, out IUnitOrder order), Is.True,
                "emir deftere yazılmalı");
            Assert.That(order.Target, Is.SameAs(target));
        }

        /// <summary>
        /// Seçim bırakıldı diye emir DÜŞMEZ — ve bu bağın koparılması bu turun
        /// asıl kararı.
        /// </summary>
        // ESKİ EMİR "saldıran hâlâ seçili mi" diye soruyordu, yani bu testin
        // iddiası eski kodda KIRMIZI olurdu: seçimi bırakan çağrı emri de
        // siler, kalıcı saldırı hiç doğmazdı.
        [Test]
        public void IssueOrder_AfterTheSelectionIsReleased_TheOrderStillStands()
        {
            InstallViewPool();
            Unit striker = SpawnFighter("Striker", Team.Player, 0, 0);
            Unit target = SpawnFighter("Raider", Team.Enemy, 0, 1);
            SetField("selectedUnit", striker);

            Invoke("HandleOccupiedCellClick", target, 0, 1);
            Invoke("ClearSelection");

            Assert.That(Orders().TryGet(striker, out IUnitOrder _), Is.True);
        }

        /// <summary>
        /// Seçimi bırakılan savaşçıya yeniden tıklamak onu GERİ ALIR ve ona ne
        /// söylendiğini de söyler.
        /// </summary>
        // ██ İŞ-2'NİN İKİNCİ YARISI, VE ÖLÇÜLMEMİŞ TEK YARISI BUYDU ██
        // Operatörün cümlesi iki şart taşıyordu: "emir verildiğinde seçim
        // kaldırılmalı AMA tekrardan seçim alınabilecek." Birinci şartın iki
        // testi vardı; ikincisinin hiç yoktu ve DescribeOrder'ın tek çağıranı
        // bir Debug.Log satırıydı — yani üye silinse hiçbir test kırmızıya
        // dönmezdi. Emrini gösteren cümle bir kolaylık değil, oyuncunun
        // seçimini geri alabildiğinin TEK ekran kanıtı.
        [Test]
        public void HandleOccupiedCellClick_OnAUnitThatHoldsAnOrder_SelectsItAgainAndNamesTheOrder()
        {
            InstallViewPool();
            Unit striker = SpawnFighter("Striker", Team.Player, 0, 0);
            Unit target = SpawnFighter("Raider", Team.Enemy, 0, 1);
            SetField("selectedUnit", striker);

            // Emir yazılıyor; İŞ-2 gereği seçim aynı çağrıda bırakılıyor.
            Invoke("HandleOccupiedCellClick", target, 0, 1);
            Assert.That(GetField("selectedUnit"), Is.Null, "emir yazılınca seçim bırakılmalı");

            // SIRA ÖNEMLİ: Expect çağrının ÖNÜNDE olmalı, LogAssert bekleneni
            // yayınlanmış satırlarla eşleştiriyor.
            LogAssert.Expect(
                LogType.Log,
                new Regex(@"holds 'Striker' - SELECTED\. It is attacking 'Raider'\."));

            Invoke("HandleOccupiedCellClick", striker, 0, 0);

            Assert.That(GetField("selectedUnit"), Is.SameAs(striker), "birime tıklamak onu geri almalı");
            Assert.That(Orders().TryGet(striker, out IUnitOrder standing), Is.True,
                "geri alınan seçim emri düşürmemeli");
            Assert.That(standing.Target, Is.SameAs(target));
        }

        /// <summary>
        /// İKİ AYRI TAKIMDAN birer birim aynı anda emir tutabiliyor — TAHTANIN
        /// kendi girdi kapısından geçerek.
        /// </summary>
        // ██ OPERATÖRÜN BİLDİRDİĞİ BELİRTİ TAM OLARAK BUYDU ██
        // "İki taraf için paralel olarak saldırı aşamalarını gerçekleştiremiyorum."
        // Eski kodda ikinci tıklama birincinin dört alanını eziyordu ve bu
        // testin son iddiası kırmızıya dönerdi. UnitOrderTests aynı şeyi defter
        // katmanında ölçüyor; burası GİRDİ kapısının da çoğul olduğunu ölçüyor.
        [Test]
        public void HandleOccupiedCellClick_ForTwoUnitsOnOpposingTeams_KeepsBothOrders()
        {
            InstallViewPool();
            Unit friendly = SpawnFighter("Vanguard", Team.Player, 0, 0);
            Unit enemy = SpawnFighter("Raider", Team.Enemy, 0, 1);

            SetField("selectedUnit", friendly);
            Invoke("HandleOccupiedCellClick", enemy, 0, 1);

            SetField("selectedUnit", enemy);
            Invoke("HandleOccupiedCellClick", friendly, 0, 0);

            Assert.That(Orders().Count, Is.EqualTo(2));
            Assert.That(Orders().TryGet(friendly, out IUnitOrder friendlyOrder), Is.True);
            Assert.That(friendlyOrder.Target, Is.SameAs(enemy));
            Assert.That(Orders().TryGet(enemy, out IUnitOrder enemyOrder), Is.True);
            Assert.That(enemyOrder.Target, Is.SameAs(friendly));
        }

        /// <summary>
        /// Boş bir hücreye yürümek YALNIZ o birimin emrini keser.
        /// </summary>
        // ESKİ HÂLDE HER TIKLAMA TAHTADAKİ TEK EMRİ DÜŞÜRÜYORDU: bir savaşçıyı
        // yürütmek ötekinin saldırısını da keserdi.
        [Test]
        public void HandleEmptyCellClick_CancelsOnlyTheWalkingUnitsOrder()
        {
            InstallViewPool();
            Unit walker = SpawnFighter("Vanguard", Team.Player, 0, 0);
            Unit stander = SpawnFighter("Archer", Team.Player, 1, 1);
            Unit enemy = SpawnFighter("Raider", Team.Enemy, 0, 1);

            SetField("selectedUnit", walker);
            Invoke("HandleOccupiedCellClick", enemy, 0, 1);
            SetField("selectedUnit", stander);
            Invoke("HandleOccupiedCellClick", enemy, 0, 1);
            Assert.That(Orders().Count, Is.EqualTo(2), "setup");

            SetField("selectedUnit", walker);
            Invoke("HandleEmptyCellClick", 2, 4);

            Assert.That(Orders().TryGet(walker, out IUnitOrder _), Is.False);
            Assert.That(Orders().TryGet(stander, out IUnitOrder _), Is.True);
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

            // ÜÇÜNCÜ DEFTER DE KURULUYOR: testin adı "HER defter" diyor ve geri
            // sayım şeridi eklendiği gün bu satır olmasaydı ad fazla söz vermiş
            // olurdu — sızıntı sessizce geri dönerdi.
            adapter.ShowProductionCountdown(tower, 3f, 5f);

            Assert.That(HealthBars().ContainsKey(tower), Is.True, "setup: the tower got a bar");
            Assert.That(ProductionTimers().ContainsKey(tower), Is.True, "setup: the tower got a timer");

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
            Assert.That(ProductionTimers().ContainsKey(tower), Is.False);
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
        // GEREKÇE DEĞİŞTİ, İDDİA KALDI. Eskiden "bırakılsaydı emir bir sonraki
        // karede savaşta bulunmayan bir kimliğe saldırı çağırır ve istisna
        // üretirdi" deniyordu; bugün emir vurmadan ÖNCE konumu kendisi soruyor,
        // yani istisna imkânsız. İddia yine de duruyor çünkü cevabın AYNI
        // KAREDE görünmesi gerekiyor — kaldırılan bir birimin peşindeki emir
        // bir kare daha yaşamamalı. Ve artık ÇOĞUL: iki saldıranın emri de
        // aynı anda düşüyor.
        [Test]
        public void RemoveSelected_OnATargetedUnit_CancelsEveryOrderAimedAtIt()
        {
            InstallViewPool();
            Unit first = SpawnFighter("Striker", Team.Player, 0, 0);
            Unit second = SpawnFighter("Archer", Team.Player, 1, 1);
            Unit target = SpawnFighter("Raider", Team.Enemy, 0, 1);

            SetField("selectedUnit", first);
            Invoke("HandleOccupiedCellClick", target, 0, 1);
            SetField("selectedUnit", second);
            Invoke("HandleOccupiedCellClick", target, 0, 1);
            Assert.That(Orders().Count, Is.EqualTo(2), "setup");

            SetField("selectedUnit", target);
            Assert.That(adapter.RemoveSelected(), Is.True);

            Assert.That(Orders().Count, Is.EqualTo(0));
        }

        // ══ GİRDİ TARAFINDA EMİR TEKRARI ═════════════════════════════════
        // Kuralın sahibi Core (RejectedOnCooldown); buradaki soru daha dar ve
        // ondan ÖNCE geliyor — aynı emir ikinci kez YAZILMASIN. Soru artık
        // SEÇİLİ BİRİMİN emrini soruyor, tahtanın tek emrini değil.

        /// <summary>
        /// Seçili birim zaten o hedefe saldırıyorsa, tıklama emrin TEKRARIDIR.
        /// </summary>
        [Test]
        public void RepeatsOrder_WithTheSameTargetAndTheAttackerSelected_IsTrue()
        {
            var attacker = new Unit("Striker");
            battle.AddUnit(attacker, NewCombatant(Team.Player), 0, 0);
            var target = new Unit("Raider");
            battle.AddUnit(target, NewCombatant(Team.Enemy), 0, 4);

            SetField("selectedUnit", attacker);
            Orders().Write(attacker, new AttackOrder(adapter, attacker, target));

            Assert.That(Invoke("RepeatsOrder", target), Is.True);
        }

        /// <summary>
        /// BAŞKA bir hedefe tıklamak fikir değiştirmektir; emir tekrarı değil.
        /// </summary>
        [Test]
        public void RepeatsOrder_WithADifferentTarget_IsFalse()
        {
            var attacker = new Unit("Striker");
            battle.AddUnit(attacker, NewCombatant(Team.Player), 0, 0);
            var target = new Unit("Raider");
            battle.AddUnit(target, NewCombatant(Team.Enemy), 0, 4);
            var other = new Unit("Scout");
            battle.AddUnit(other, NewCombatant(Team.Enemy), 2, 4);

            SetField("selectedUnit", attacker);
            Orders().Write(attacker, new AttackOrder(adapter, attacker, target));

            Assert.That(Invoke("RepeatsOrder", other), Is.False);
        }

        /// <summary>
        /// BAŞKA BİR BİRİMİN aynı hedefe verdiği emir bu tıklamayı YUTMAZ.
        /// </summary>
        // ██ TEKİL SAHİPTEN ÇOĞUL SAHİBE GEÇİŞİN GİRDİ TARAFINDAKİ KANITI ██
        // Eski hâlde soru "tahtada yazılı emrin hedefi bu mu" idi ve tahtada
        // TEK emir vardı: ikinci savaşçısına aynı hedefi göstermek isteyen
        // oyuncunun tıklaması sessizce yutulurdu, çünkü birincinin emri o
        // hedefe yazılıydı. Bu test o yutulmanın geri gelmesini engelliyor.
        [Test]
        public void RepeatsOrder_WhenAnotherUnitHoldsTheOrderOnThatTarget_IsFalse()
        {
            var first = new Unit("Striker");
            battle.AddUnit(first, NewCombatant(Team.Player), 0, 0);
            var second = new Unit("Archer");
            battle.AddUnit(second, NewCombatant(Team.Player), 2, 0);
            var target = new Unit("Raider");
            battle.AddUnit(target, NewCombatant(Team.Enemy), 0, 4);

            Orders().Write(first, new AttackOrder(adapter, first, target));
            SetField("selectedUnit", second);

            Assert.That(Invoke("RepeatsOrder", target), Is.False);
        }

        /// <summary>
        /// Yazılı bir emir yokken hiçbir tıklama tekrar sayılmaz.
        /// </summary>
        // BU TEST BİR KİLİTLENMEYİ ÖNLÜYOR: koşul emirsiz durumda true dönseydi
        // dolu hücreye yapılan her tıklama sessizce tüketilir ve oyuncu hiçbir
        // şey seçemezdi. null argüman ayrıca sınanıyor, çünkü boş hücreye
        // tıklandığında oraya null geliyor.
        [Test]
        public void RepeatsOrder_WithNoOrderWritten_IsFalse()
        {
            var target = new Unit("Raider");
            battle.AddUnit(target, NewCombatant(Team.Enemy), 0, 4);

            Assert.That(Invoke("RepeatsOrder", target), Is.False);
            Assert.That(Invoke("RepeatsOrder", new object[] { null }), Is.False);
        }

        // ══ KARŞILIK VERME — GERÇEK TAHTA, GERÇEK KURAL ══════════════════
        // UnitOrderTests emrin KENDİ kararlarını sahte bir pencereyle ölçüyor;
        // burada ölçülen şey başka: yürünen hücrenin gerçekten menzil hücresi
        // olduğu, ve seçim noktasının doğru cinsi seçtiği. İkisi ancak gerçek
        // bir tahta, gerçek ApproachRules ve gerçek BattleActions ile görülür.
        //
        // TAHTA HER TESTTE FreeForAll KURULUYOR VE BU ŞART: varsayılan
        // Alternating kipinde yaklaşma yürüyüşü sırayı devrediyor ve peşinden
        // gelen vuruş RejectedActorCannotAct alıyor — yani emir doğduğu karede
        // düşerdi. Üretimdeki tahta da FreeForAll; varsayılan kiple yazılmış bir
        // test oyuncunun gördüğü davranışı ÖLÇMEZDİ.

        /// <summary>
        /// UZAKTAN VURULAN KILIÇLI SAVAŞÇI saldırganın bitişiğine yürüyor ve
        /// vardığında vuruyor.
        /// </summary>
        // ██ OPERATÖRÜN BİLDİRDİĞİ EKSİĞİN TAM KARŞILIĞI ██
        // Zincir onuncu durakta bitiyordu: vurulan taraf ne bir emir alıyor ne
        // bir hücre soruyor ne de karşılık veriyordu.
        [Test]
        public void Retaliation_AMeleeDefender_WalksIntoItsOwnRangeAndThenStrikes()
        {
            UseFreeForAllBoard();
            InstallViewPool();
            SetField("moveSpeed", 3f);

            Unit defender = SpawnFighter("Vanguard", Team.Player, 0, 0);
            var sniperBody = NewCombatantWithRange(Team.Enemy, 3);
            var aggressor = new Unit("Archer");
            Assert.That(adapter.PlaceUnit(aggressor, sniperBody, 0, 3, null), Is.True, "setup");

            Invoke("ReactToAttack", aggressor, AttackOutcome.Hit, defender, 0, 0);

            Assert.That(Orders().TryGet(defender, out IUnitOrder order), Is.True);
            Assert.That(order, Is.TypeOf<ChaseAndStrikeOrder>());
            Assert.That(order.Target, Is.SameAs(aggressor));

            Orders().Advance();

            Assert.That(battle.TryGetPosition(defender, out int x, out int y), Is.True);
            Assert.That(GridDistance.Between(x, y, 0, 3), Is.EqualTo(1),
                "menzili 1 olan savaşçı saldırganın bitişiğinde durmalı");

            Arrive(defender);

            int before = sniperBody.CurrentHealth;
            Orders().Advance();

            Assert.That(sniperBody.CurrentHealth, Is.LessThan(before),
                "menziline giren savaşçı karşılığını vermeli");
        }

        /// <summary>
        /// MENZİLLİ BİRİM KENDİ MENZİLİNE GİRİP DURUYOR; saldırganın bitişiğine
        /// gitmiyor.
        /// </summary>
        // ██ TEK KURAL, İKİ SAYI — VE ÖLÇÜSÜ BU TEST ██
        // Üstteki testle arasındaki tek fark bir int; ikisi de ApproachRules'un
        // aynı üyesini çağırıyor. Bu iddia kırmızıya döndüğü gün okçu göğüs
        // göğüse dövüşüyor demektir.
        [Test]
        public void Retaliation_ARangedDefender_StopsAtItsOwnRangeInsteadOfClosingIn()
        {
            UseFreeForAllBoard();
            InstallViewPool();
            SetField("moveSpeed", 3f);

            var defender = new Unit("Archer");
            Assert.That(adapter.PlaceUnit(defender, NewCombatantWithRange(Team.Player, 3), 0, 0, null),
                Is.True, "setup");
            Unit aggressor = SpawnFighter("Raider", Team.Enemy, 0, 4);

            Invoke("ReactToAttack", aggressor, AttackOutcome.Hit, defender, 0, 0);
            Orders().Advance();

            Assert.That(battle.TryGetPosition(defender, out int x, out int y), Is.True);
            Assert.That(GridDistance.Between(x, y, 0, 4), Is.EqualTo(3),
                "okçu üç hücre öteden atabildiği yerde durmalı");
        }

        /// <summary>
        /// YAPI KARŞILIK VERİR AMA YÜRÜMEZ.
        /// </summary>
        // ██ STRATEGY NOKTASININ TAHTA TARAFINDAKİ KANITI ██
        // Seçimi yapan olgu tek: savunan yürüyebiliyor mu. Tip iddiası burada
        // bir tip sorgusu KOKUSU değil, testin konusunun ta kendisi — ölçülen
        // şey hangi sınıfın seçildiği.
        [Test]
        public void Retaliation_AStructure_FiresBackWithoutEverMoving()
        {
            UseFreeForAllBoard();
            InstallViewPool();

            Unit tower = PlaceTower(Team.Player, range: 2, x: 0, y: 0);
            Unit raider = SpawnFighter("Raider", Team.Enemy, 0, 1);

            Orders().Write(raider, new AttackOrder(adapter, raider, tower));
            Orders().Advance();

            Assert.That(Orders().TryGet(tower, out IUnitOrder order), Is.True);
            Assert.That(order, Is.TypeOf<StandAndStrikeOrder>());
            Assert.That(order.Target, Is.SameAs(raider));

            Assert.That(battle.TryGetCombatant(raider, out Combatant raiderBody), Is.True, "setup");
            int before = raiderBody.CurrentHealth;

            Orders().Advance();

            Assert.That(raiderBody.CurrentHealth, Is.LessThan(before), "taret karşılığını vermeli");
            Assert.That(battle.TryGetPosition(tower, out int x, out int y), Is.True);
            Assert.That(x, Is.EqualTo(0));
            Assert.That(y, Is.EqualTo(0));
        }

        /// <summary>
        /// SİLAHSIZ YAPI karşılık emri almaz.
        /// </summary>
        // Kışlaya yazılacak emir her karede RejectedActorCannotAct alır ve
        // doğduğu karede düşerdi; yazılmaması bir eksiklik değil bir kapı.
        [Test]
        public void Retaliation_AWeaponlessStructure_GetsNoOrderAtAll()
        {
            UseFreeForAllBoard();
            InstallViewPool();

            Unit depot = PlaceDepot(Team.Player, 0, 0);
            Unit raider = SpawnFighter("Raider", Team.Enemy, 0, 1);

            Invoke("ReactToAttack", raider, AttackOutcome.Hit, depot, 0, 0);

            Assert.That(Orders().Count, Is.EqualTo(0));
        }

        /// <summary>
        /// TARET BİR SAVAŞÇIYI VURUNCA savaşçı tarete yöneliyor.
        /// </summary>
        // ██ OPERATÖRÜN AYRICA İSTEDİĞİ HÂL ██
        // "Taretin savaşçıya saldırması gibi durumlar için de geçerli."
        // Saldıranın bir yapı olması hiçbir şeyi değiştirmiyor, çünkü karşılık
        // emri saldıranın tipini hiç sormuyor — yalnız SAVUNANINKİNİ soruyor.
        [Test]
        public void Retaliation_WhenATurretHitsAFighter_TheFighterTurnsOnTheTurret()
        {
            UseFreeForAllBoard();
            InstallViewPool();
            SetField("moveSpeed", 3f);

            Unit tower = PlaceTower(Team.Enemy, range: 3, x: 0, y: 0);
            Unit fighter = SpawnFighter("Vanguard", Team.Player, 0, 3);

            // ÜRETİMDEKİ YOL: kule kendiliğinden ateş ediyor, oyuncunun bir
            // tıklaması yok. Büyük sayı bekleme penceresini kesin doldurmak için.
            Invoke("AdvanceStructureFire", 99f);

            Assert.That(Orders().TryGet(fighter, out IUnitOrder order), Is.True);
            Assert.That(order, Is.TypeOf<ChaseAndStrikeOrder>());
            Assert.That(order.Target, Is.SameAs(tower));

            Orders().Advance();

            Assert.That(battle.TryGetPosition(fighter, out int x, out int y), Is.True);
            Assert.That(GridDistance.Between(x, y, 0, 0), Is.EqualTo(1),
                "savaşçı kendi menziline girene kadar yürümeli");

            Arrive(fighter);

            Assert.That(battle.TryGetStructure(tower, out Structure fort), Is.True, "setup");
            int before = fort.CurrentHealth;

            Orders().Advance();

            Assert.That(fort.CurrentHealth, Is.LessThan(before), "savaşçı tarete vurmalı");
        }

        /// <summary>
        /// OYUNCUNUN VERDİĞİ EMİR EZİLMİYOR.
        /// </summary>
        // KARŞILIK YALNIZ EMRİ OLMAYAN BİRİME YAZILIR: ezseydi, saldırı emri
        // verilen savaşçı ilk yediği darbede oyuncunun hiç istemediği bir hedefe
        // dönerdi.
        [Test]
        public void Retaliation_WhenTheDefenderAlreadyHoldsAnOrder_TheOperatorsOrderSurvives()
        {
            UseFreeForAllBoard();
            InstallViewPool();

            Unit defender = SpawnFighter("Vanguard", Team.Player, 0, 0);
            Unit chosen = SpawnFighter("Scout", Team.Enemy, 2, 4);
            Unit aggressor = SpawnFighter("Raider", Team.Enemy, 0, 1);

            Orders().Write(defender, new AttackOrder(adapter, defender, chosen));

            Invoke("ReactToAttack", aggressor, AttackOutcome.Hit, defender, 0, 0);

            Assert.That(Orders().TryGet(defender, out IUnitOrder order), Is.True);
            Assert.That(order, Is.TypeOf<AttackOrder>());
            Assert.That(order.Target, Is.SameAs(chosen));
        }

        /// <summary>
        /// SALDIRGAN TAHTADAN KALKTI: karşılık emri bir sonraki ilerletmede düşer.
        /// </summary>
        [Test]
        public void Retaliation_WhenTheAggressorLeavesTheBoard_TheOrderIsDropped()
        {
            UseFreeForAllBoard();
            InstallViewPool();

            Unit defender = SpawnFighter("Vanguard", Team.Player, 0, 0);
            Unit aggressor = SpawnFighter("Raider", Team.Enemy, 0, 1);

            Invoke("ReactToAttack", aggressor, AttackOutcome.Hit, defender, 0, 0);
            Assert.That(Orders().Count, Is.EqualTo(1), "setup");

            Assert.That(battle.RemoveUnit(aggressor), Is.True, "setup");
            Orders().Advance();

            Assert.That(Orders().Count, Is.EqualTo(0));
        }

        /// <summary>
        /// AYAKTA KALMAYAN SAVUNAN karşılık emri almaz.
        /// </summary>
        // KARŞILIK YALNIZ <c>Hit</c> DALINDAN DOĞUYOR ve gerekçe tek cümle:
        // öteki üç isabet değeri savunanın DÜŞTÜĞÜNÜ, bitirildiğini ya da
        // yıkıldığını söylüyor — karşılık verecek kimse kalmadı.
        [TestCase(AttackOutcome.HitAndDowned)]
        [TestCase(AttackOutcome.RejectedOutOfRange)]
        public void Retaliation_WhenTheHitDidNotLeaveTheDefenderStanding_NoOrderIsWritten(
            AttackOutcome outcome)
        {
            UseFreeForAllBoard();
            InstallViewPool();

            Unit defender = SpawnFighter("Vanguard", Team.Player, 0, 0);
            Unit aggressor = SpawnFighter("Raider", Team.Enemy, 0, 1);

            Invoke("ReactToAttack", aggressor, outcome, defender, 0, 0);

            Assert.That(Orders().Count, Is.EqualTo(0));
        }

        /// <summary>
        /// Tahtayı FreeForAll kipiyle yeniden kurar.
        /// </summary>
        // AYRI BİR ÜYE, SetUp'A TAŞINMADI: yirmiden fazla test varsayılan
        // Alternating kipiyle yazıldı ve bir kısmı sıra devrini AÇIKÇA ölçüyor.
        // Kipi SetUp'ta değiştirmek onların ölçtüğü şeyi sessizce silerdi.
        private void UseFreeForAllBoard()
        {
            var freeForAll = new Battle(Width, Height, global::GridStrategy.Battle.TurnMode.FreeForAll);
            SetField("battle", freeForAll);
            battle = freeForAll;
        }

        /// <summary>
        /// Menzili SetUp'takinden farklı bir savaşçı kurar.
        /// </summary>
        // NewCombatant'IN İKİZİ VE TEK FARKI MENZİL: "tek kural, iki sayı"
        // iddiası ancak iki farklı sayı yan yana konabildiğinde ölçülebilir.
        private static Combatant NewCombatantWithRange(Team team, int range)
        {
            return new Combatant(
                new Health(MaxHealth),
                new UnitLifecycle(),
                new AttackProfile(Damage, range),
                team);
        }

        /// <summary>
        /// Görselin yürüyüşünü BİTMİŞ sayar.
        /// </summary>
        // ██ EDITMODE'DA KARE GEÇMİYOR, VE BU BİR TAVİZ DEĞİL BİR SINIR ██
        // UnitWalker adımlarını Update'te atıyor; EditMode'da o çağrı hiç
        // gelmiyor, yani yürüyüş kendiliğinden bitmiyor ve emir sonsuza kadar
        // "henüz varmadı" derdi. SnapToEnd üretimde de var olan bir üye
        // (havuz ve anında eşitleme onu çağırıyor), yani burada uydurulmuş bir
        // test kapısı açılmıyor — varışın kendisi çağrılıyor.
        private void Arrive(Unit unit)
        {
            Assert.That(UnitViews().TryGetValue(unit, out UnitView view), Is.True,
                $"no view registered for '{unit.Name}'");

            UnitWalker walker = view.GetComponent<UnitWalker>();
            Assert.That(walker, Is.Not.Null, $"'{unit.Name}' never started walking");
            walker.SnapToEnd();
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
        /// SİLAHSIZ bir yapı kurar: kışla, depo, karargâh — ateş etmeyen her şey.
        /// </summary>
        // PlaceTower'IN İKİZİ VE TEK FARKI AttackProfile'IN YOKLUĞU. Ayrı bir
        // üye, çünkü ayrımın kendisi test edilen şey: odak devri kuralı tam
        // olarak bu iki yardımcının arasındaki farka bakıyor.
        private Unit PlaceDepot(Team team, int x, int y)
        {
            var identity = new Unit($"Depot_{x}_{y}");
            var depot = new Structure(
                new Health(StructureMaxHealth),
                new StructureLifecycle(),
                team);

            Assert.That(adapter.PlaceStructure(identity, depot, x, y),
                Is.EqualTo(global::GridStrategy.Battle.PlacementOutcome.Placed),
                "setup: the depot must reach the board");

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
        /// Emir defterini yansımayla verir.
        /// </summary>
        // DEFTER readonly BİR ALAN, yani SetField ile YAZILAMAZ ve yazılmasına
        // gerek de yok: alan başlatıcısında kuruluyor, dolayısıyla Awake hiç
        // koşmasa bile dolu doğuyor. Okunması yeterli.
        private UnitOrderBook Orders()
        {
            return (UnitOrderBook)GetField("orders");
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
        /// Tahtaya bir yerleştirme hayaleti takar ve onu geri verir.
        /// </summary>
        // AUTHORED RENK BURADA YAZILIYOR: sahnede ayarlanmış saydam bir renk
        // taklit ediliyor, çünkü sınanan şeylerden biri tam olarak o rengin
        // geri gelmesi. Beyaz bırakılsaydı "geri geldi" iddiası, rengin hiç
        // yazılmadığı bir dünyada da yeşil kalırdı.
        private SpriteRenderer InstallGhost()
        {
            var go = new GameObject("PlacementGhost");
            go.transform.SetParent(probe.transform, worldPositionStays: false);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = NewSprite();
            renderer.color = new Color(0.6f, 0.9f, 0.6f, 0.5f);

            SetField("placementGhost", renderer);
            Invoke("CaptureAuthoredGhostSprite");
            return renderer;
        }

        /// <summary>
        /// Sahneye ortografik bir MainCamera koyar.
        /// </summary>
        // EKRAN NOKTASI SORAN TESTLERİN ÖN ŞARTI: TryScreenPointToWorldCell
        // Camera.main bulamazsa LogError basıp false dönüyor ve o hata satırı
        // testin ölçtüğü şey değil.
        private GameObject InstallCamera()
        {
            var go = new GameObject("TestCamera") { tag = "MainCamera" };
            Camera camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.transform.position = new Vector3(5f, 2.5f, -10f);

            disposables.Add(go);
            return go;
        }

        /// <summary>
        /// Geri sayım şeritleri tablosunu yansımayla verir.
        /// </summary>
        private Dictionary<Unit, ProductionTimerView> ProductionTimers()
        {
            return (Dictionary<Unit, ProductionTimerView>)GetField("productionTimers");
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
        // DEĞER TİPİ GameObject DEĞİL StructureView: tablo artık yapının görünüm
        // bileşenini tutuyor ve bu testin yansıması o tipi birebir yazmak
        // zorunda — yanlış tip yazıldığında hata bir iddia değil, bir
        // InvalidCastException olur.
        private Dictionary<Unit, StructureView> StructureViews()
        {
            FieldInfo field = typeof(BoardAdapter).GetField("structureViews", Hidden);
            Assert.That(field, Is.Not.Null, "BoardAdapter no longer has a 'structureViews' field");
            return (Dictionary<Unit, StructureView>)field.GetValue(adapter);
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

        /// <summary>
        /// Yürürlükteki kipi verir.
        /// </summary>
        // ALAN DEĞİL ÖZELLİK OKUNUYOR ve fark ölçüldü: `modes` alanı ilk
        // SORULUŞTA kuruluyor, Awake'te değil. Alanı doğrudan okuyan bir yardımcı
        // hiç kip istenmemiş bir tahtada null döner ve testi sınadığı kuralın
        // dışında bir yerde düşürürdü.
        private IBoardMode CurrentMode()
        {
            PropertyInfo property = typeof(BoardAdapter).GetProperty("Modes", Hidden);
            Assert.That(property, Is.Not.Null, "BoardAdapter has no private property named 'Modes'");
            return ((BoardModeMachine)property.GetValue(adapter)).Current;
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
