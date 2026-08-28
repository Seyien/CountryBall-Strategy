using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using GridStrategy.Combat;
using GridStrategy.Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace GridStrategy.Tests.EditMode.Unity
{
    /// <summary>
    /// <b>BU DOSYA TEK BİR EŞİTLİĞİ KORUYOR:</b> bir yapı varlığının
    /// <see cref="StructureBlueprintAsset.ProducedAssets"/> listesi ile onun
    /// çekirdek tanımındaki <see cref="StructureBlueprint.Produces"/> listesi
    /// AYNI SIRADA, AYNI UZUNLUKTADIR.
    ///
    /// NEDEN BU KADAR KIRILGAN: sağ panel bir birimin ADINI çekirdek
    /// tanımından, SİMGESİNİ varlık listesinden okuyor ve ikisini de aynı
    /// indisle istiyor. Tanım kurulurken atanmamış dizi gözleri ATILIYOR; varlık
    /// listesi atmasaydı ham dizinin i'inci gözü ile tanımın i'inci birimi
    /// ayrışır ve panel bir birimin adının yanına başkasının resmini çizerdi.
    /// Bu kusurun ekranda tek bir hata satırı yok: oyuncu bir asker sürükler,
    /// bambaşka bir asker doğar ve hiçbir şey patlamaz.
    ///
    /// EN TEHLİKELİ DURUM ORTADAKİ BOŞ GÖZ: sondaki boş göz iki listenin
    /// uzunluğunu eşit tuttuğu sürece kusuru gizler, ortadaki ise indisleri
    /// KAYDIRIR. Aşağıdaki testlerden biri tam olarak o kaymayı bekliyor.
    ///
    /// NEDEN SerializedObject: iki varlık tipinin de bütün alanları
    /// <c>private</c> ve yalnız <c>[SerializeField]</c> ile serileştiriliyor —
    /// yani koddan yazmanın motor tarafındaki tek dürüst yolu budur. Alanlara
    /// yansımayla yazmak da mümkündü ama reddedildi: yansıma serileştirmeyi
    /// atlar ve <c>OnValidate</c>'i hiç tetiklemez, oysa sınanan şeyin yarısı
    /// tam olarak varlığın Inspector'dan yazıldığı yolun kendisidir.
    ///
    /// BEDELİ SAKLANMIYOR: bu dosya ÖZEL ALAN ADLARINA bağlıdır. Bir alan
    /// yeniden adlandırıldığında derleyici susar, aşağıdaki yardımcı ise
    /// bulamadığı adı açık bir iddiayla bildirir.
    /// </summary>
    public sealed class ProducedAssetIndexParityTests
    {
        // Ekranda görünen adlar BİRBİRİNDEN AYRI ve hiçbiri ötekinin öneki
        // değil: eşitlik iddiaları bu adların üstünden okunuyor ve benzeşen iki
        // ad, kaymış bir indisi yeşil gösterebilirdi.
        private const string Alpha = "Alpha Trooper";
        private const string Beta = "Beta Trooper";
        private const string Gamma = "Gamma Trooper";

        // Testte doğan her motor nesnesi buraya yazılıyor. EditMode'da sahne
        // temizlenmez; bırakılan bir ScriptableObject sonraki testlere sarkar.
        private readonly List<Object> spawned = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            // DestroyImmediate, Destroy değil: Destroy karenin sonunu bekler ve
            // EditMode'da o kare hiç gelmez. Aynı ölçü UnitViewTests içinde de
            // yazılı.
            for (int i = 0; i < spawned.Count; i++)
            {
                Object.DestroyImmediate(spawned[i]);
            }

            spawned.Clear();
        }

        /// <summary>
        /// Boş göz yokken iki liste birebir örtüşür — kural hâli budur.
        /// </summary>
        [Test]
        public void ProducedAssets_WithNoEmptySlots_MirrorsTheDefinitionIndexForIndex()
        {
            UnitBlueprintAsset alpha = NewUnit(Alpha);
            UnitBlueprintAsset beta = NewUnit(Beta);
            UnitBlueprintAsset gamma = NewUnit(Gamma);

            StructureBlueprintAsset barrack = NewStructure("Barrack", 0, alpha, beta, gamma);

            AssertParity(barrack);

            Assert.That(barrack.ProducedAssets[0], Is.SameAs(alpha));
            Assert.That(barrack.ProducedAssets[1], Is.SameAs(beta));
            Assert.That(barrack.ProducedAssets[2], Is.SameAs(gamma));
        }

        /// <summary>
        /// ORTADAKİ BOŞ GÖZ: tanım onu atıyor, varlık listesi de atmak zorunda.
        /// </summary>
        // BU TEST OLMASAYDI KUSUR SESSİZ KALIRDI ve sessizliğin ölçüsü şu: ham
        // produces dizisini indeksleyen bir panel burada patlamaz, yalnızca
        // YANLIŞ resmi çizer. Aşağıdaki iddia o yanlışı bir kırmızıya çeviriyor.
        [Test]
        public void ProducedAssets_WithAHoleInTheMiddle_StaysAlignedWithTheDefinition()
        {
            UnitBlueprintAsset alpha = NewUnit(Alpha);
            UnitBlueprintAsset gamma = NewUnit(Gamma);

            StructureBlueprintAsset barrack = NewStructure("Barrack", 0, alpha, null, gamma);

            // Boş göz SESSİZ atlanmıyor; tasarımcıya bir satır düşüyor ve o
            // satırı beklemek bu testin işi.
            ExpectEmptySlotError(1);

            AssertParity(barrack);

            Assert.That(barrack.ProducedAssets.Count, Is.EqualTo(2),
                "the dropped slot must shrink both lists, not just the definition");

            // ASIL TUZAK TAM BURADA: ham dizide 1'inci göz BOŞ, tanımda ise
            // Gamma. Varlık listesi ham diziyi izleseydi bu satır ya null
            // okurdu ya da Alpha'yı ikinci kez gösterirdi.
            Assert.That(barrack.ProducedAssets[1], Is.SameAs(gamma));
            Assert.That(barrack.Definition.Produces[1].DisplayName, Is.EqualTo(Gamma));
        }

        /// <summary>
        /// Simge ile ad AYNI birimden gelmek zorunda; sağ panelin çizdiği şey
        /// tam olarak bu ikilidir.
        /// </summary>
        [Test]
        public void ProducedAssets_WithAHoleInTheMiddle_KeepsEachIconOnItsOwnName()
        {
            UnitBlueprintAsset alpha = NewUnit(Alpha);
            UnitBlueprintAsset gamma = NewUnit(Gamma);

            Sprite alphaIcon = NewIcon();
            Sprite gammaIcon = NewIcon();
            SetIcon(alpha, alphaIcon);
            SetIcon(gamma, gammaIcon);

            StructureBlueprintAsset barrack = NewStructure("Barrack", 0, alpha, null, gamma);

            ExpectEmptySlotError(1);

            // İDDİA ADIN ÜSTÜNDEN KURULUYOR, indisin üstünden değil: panelin
            // yaptığı şey de tam olarak bu — adı bir listeden, simgeyi ötekinden
            // okuyup yan yana çiziyor.
            Assert.That(barrack.Definition.Produces[0].DisplayName, Is.EqualTo(Alpha));
            Assert.That(barrack.ProducedAssets[0].Icon, Is.SameAs(alphaIcon));

            Assert.That(barrack.Definition.Produces[1].DisplayName, Is.EqualTo(Gamma));
            Assert.That(barrack.ProducedAssets[1].Icon, Is.SameAs(gammaIcon));
        }

        /// <summary>
        /// Hiçbir şey üretmeyen yapı: iki liste de boş, konsola tek satır
        /// düşmüyor.
        /// </summary>
        [Test]
        public void ProducedAssets_ForAStructureThatProducesNothing_IsEmpty()
        {
            StructureBlueprintAsset plant = NewStructure("Power Plant", 0);

            AssertParity(plant);

            Assert.That(plant.ProducedAssets.Count, Is.EqualTo(0));
            Assert.That(plant.Definition.CanProduce, Is.False,
                "an empty production list is the rule, not an error");
        }

        /// <summary>
        /// Varsayılan indis listenin dışına taşarsa kırpılır — ve kırpma iki
        /// listenin eşitliğini BOZMAZ.
        /// </summary>
        [Test]
        public void Definition_WithADefaultIndexPastTheEnd_FallsBackToZeroAndKeepsBothListsAligned()
        {
            UnitBlueprintAsset alpha = NewUnit(Alpha);
            UnitBlueprintAsset beta = NewUnit(Beta);

            StructureBlueprintAsset barrack = NewStructure("Barrack", 5, alpha, beta);

            LogAssert.Expect(
                LogType.Error,
                new Regex(@"defaultProducedIndex 5 but only 2 produced unit"));

            Assert.That(barrack.Definition.DefaultProducedIndex, Is.EqualTo(0));

            AssertParity(barrack);
        }

        /// <summary>
        /// Boş listede tek uygun varsayılan 0'dır; başka bir sayı yazılmışsa
        /// bağırarak kırpılır.
        /// </summary>
        [Test]
        public void Definition_WhenNothingIsProducedButADefaultIndexIsSet_FallsBackToZero()
        {
            StructureBlueprintAsset plant = NewStructure("Power Plant", 3);

            LogAssert.Expect(
                LogType.Error,
                new Regex(@"produces nothing but carries defaultProducedIndex 3"));

            Assert.That(plant.Definition.DefaultProducedIndex, Is.EqualTo(0));
            Assert.That(plant.ProducedAssets.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// İki liste TEK geçişten doğuyor: ikinci okuma yeni bir liste kurmaz.
        /// </summary>
        // ÖNBELLEĞİN PAYLAŞILMASI BİR AYRINTI DEĞİL, EŞİTLİĞİN DAYANAĞI: iki
        // liste ayrı ayrı kurulsaydı biri tazelenip öteki bayat kalabilirdi ve
        // aradaki fark yalnız ekranda görünürdü.
        [Test]
        public void ProducedAssets_ReadTwice_ReturnsTheSameCachedListAsTheDefinition()
        {
            UnitBlueprintAsset alpha = NewUnit(Alpha);
            StructureBlueprintAsset barrack = NewStructure("Barrack", 0, alpha);

            Assert.That(barrack.ProducedAssets, Is.SameAs(barrack.ProducedAssets));
            Assert.That(barrack.Definition, Is.SameAs(barrack.Definition));
        }

        /// <summary>
        /// İki listenin uzunluğunu ve her indisteki adı karşılaştırır.
        /// </summary>
        private static void AssertParity(StructureBlueprintAsset asset)
        {
            IReadOnlyList<UnitBlueprint> produces = asset.Definition.Produces;
            IReadOnlyList<UnitBlueprintAsset> assets = asset.ProducedAssets;

            Assert.That(assets.Count, Is.EqualTo(produces.Count),
                "the definition and the asset list must drop the very same slots");

            for (int i = 0; i < produces.Count; i++)
            {
                Assert.That(assets[i], Is.Not.Null, $"produced asset {i} is missing");
                Assert.That(assets[i].DisplayName, Is.EqualTo(produces[i].DisplayName),
                    $"index {i} points at two different units");
            }
        }

        /// <summary>
        /// Atanmamış dizi gözü için beklenen konsol satırını kurar.
        /// </summary>
        private static void ExpectEmptySlotError(int slot)
        {
            LogAssert.Expect(
                LogType.Error,
                new Regex(@"empty slot at produces\[" + slot + @"\]"));
        }

        private UnitBlueprintAsset NewUnit(string displayName)
        {
            var asset = ScriptableObject.CreateInstance<UnitBlueprintAsset>();
            spawned.Add(asset);

            var serialized = new SerializedObject(asset);
            Field(serialized, "displayName").stringValue = displayName;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return asset;
        }

        private void SetIcon(UnitBlueprintAsset asset, Sprite icon)
        {
            var serialized = new SerializedObject(asset);
            Field(serialized, "icon").objectReferenceValue = icon;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// Bellek içinde bir yapı varlığı kurar. Dizide <c>null</c> geçmek
        /// serbesttir — atanmamış bir Inspector gözünün karşılığı odur.
        /// </summary>
        private StructureBlueprintAsset NewStructure(
            string displayName,
            int defaultProducedIndex,
            params UnitBlueprintAsset[] produced)
        {
            var asset = ScriptableObject.CreateInstance<StructureBlueprintAsset>();
            spawned.Add(asset);

            var serialized = new SerializedObject(asset);
            Field(serialized, "displayName").stringValue = displayName;

            // TEK APPLY, EN SONDA: alanlar teker teker uygulansaydı yarım
            // kurulmuş bir varlık üzerinde OnValidate koşabilir ve önbellek
            // henüz yazılmamış bir diziden doğardı.
            SerializedProperty list = Field(serialized, "produces");
            list.arraySize = produced.Length;
            for (int i = 0; i < produced.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = produced[i];
            }

            // [Min(0)] yalnız Inspector çizerken kelepçeliyor; buradan taşan bir
            // sayı yazmak serbest ve iki test tam olarak o taşmayı sınıyor.
            Field(serialized, "defaultProducedIndex").intValue = defaultProducedIndex;

            serialized.ApplyModifiedPropertiesWithoutUndo();

            return asset;
        }

        private Sprite NewIcon()
        {
            var texture = new Texture2D(2, 2);
            spawned.Add(texture);

            Sprite icon = Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f));
            spawned.Add(icon);

            return icon;
        }

        /// <summary>
        /// Bir alanı adıyla bulur ve bulamadığında SESSİZ geçmez.
        /// </summary>
        // BULAMAYINCA AÇIK BİR İDDİAYLA DÜŞÜYOR: null dönseydi hata, alan adının
        // değiştiği yerde değil, çok sonra bir NullReference olarak görünürdü.
        private static SerializedProperty Field(SerializedObject serialized, string fieldName)
        {
            SerializedProperty property = serialized.FindProperty(fieldName);

            Assert.That(property, Is.Not.Null,
                $"the serialized field '{fieldName}' no longer exists; this test file must follow the rename");

            return property;
        }
    }
}
