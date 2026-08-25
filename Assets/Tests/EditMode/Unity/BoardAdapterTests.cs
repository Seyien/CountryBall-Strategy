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

        private object Invoke(string name, params object[] arguments)
        {
            MethodInfo method = typeof(BoardAdapter).GetMethod(name, Hidden);
            Assert.That(method, Is.Not.Null, $"BoardAdapter has no private method named '{name}'");
            return method.Invoke(adapter, arguments);
        }
    }
}
