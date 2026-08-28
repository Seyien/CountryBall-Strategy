using System.Collections.Generic;
using NUnit.Framework;
using GridStrategy.Unity;
using UnityEditor;
using UnityEngine;

namespace GridStrategy.Tests.EditMode.Unity
{
    /// <summary>
    /// Bu dosyanın sınadığı iddia tek cümleyle şudur: <c>boardSizeInCells</c>
    /// sayısı bir ÖLÇEK değil, bir HÜCRE SAYISIDIR — ve aynı sayı 16x16 ile
    /// 32x32 sanat için aynı şeyi ifade eder.
    ///
    /// NEDEN SINANABİLİYOR: <see cref="BoardSizing"/> bir MonoBehaviour değil.
    /// Sahne, kamera, Play mode ya da Awake istemiyor; yalnız bir Sprite ile iki
    /// sayı istiyor. Hesap BoardAdapter'ın içinde kalsaydı bu dosya yazılamazdı
    /// ve tahtanın en görünür kusuru yalnız gözle denetlenirdi.
    ///
    /// ESKİ YAZIM BU TESTLERİN ÇOĞUNU GEÇERDİ ve geçmesi bir şey kanıtlamazdı:
    /// bütün tahta sanatı bugün 16x16 ve içe aktarma 16 PPU, yani "ölçek 1,25"
    /// yazmak ile "1,25 hücre kaplasın" demek AYNI sonucu veriyor. Ayrımı
    /// gösteren tek test 32x32 olanı; gerisi onun etrafındaki kelepçedir.
    /// </summary>
    public sealed class BoardSizingTests
    {
        // Kayan noktada bölme sırası son bitlerde oynayabiliyor; eşik o
        // oynamadan büyük, ölçülmek istenen her farktan küçük.
        private const float Tolerance = 0.0001f;

        // Testin ürettiği Sprite ve Texture2D birer Unity nesnesidir ve test
        // bitince kendiliğinden gitmezler. Toplanmasalardı her koşuda sızarlardı
        // — aynı gerekçe UnitViewTests'te de yazılı.
        private readonly List<Object> disposables = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object item in disposables)
            {
                if (item != null)
                {
                    Object.DestroyImmediate(item);
                }
            }

            disposables.Clear();
        }

        // ── BUGÜNKÜ SANAT: 16 piksel karo, 16 PPU, bir birimlik hücre.

        [Test]
        public void LocalScaleFor_OneCellOnSixteenPixelArt_IsExactlyOne()
        {
            Sprite sprite = MakeSprite(16, 16, pixelsPerUnit: 16f);

            Vector3 scale = BoardSizing.LocalScaleFor(sprite, 1f, Vector3.one);

            Assert.AreEqual(1f, scale.x, Tolerance);
            Assert.AreEqual(1f, scale.y, Tolerance);
        }

        [Test]
        public void LocalScaleFor_StructureDefaultOnSixteenPixelArt_IsExactly125()
        {
            Sprite sprite = MakeSprite(16, 16, pixelsPerUnit: 16f);

            Vector3 scale = BoardSizing.LocalScaleFor(sprite, 1.25f, Vector3.one);

            Assert.AreEqual(1.25f, scale.x, Tolerance);
            Assert.AreEqual(1.25f, scale.y, Tolerance);
        }

        // ── MEKANİZMANIN ASIL İDDİASI. 32x32 bir görsel 16 PPU ile İKİ birim
        //    çiziliyor, yani ölçek 1 iken zaten iki hücre kaplıyor. Aynı 1,25
        //    hücre isteği bu yüzden 0,625 ölçek üretmeli. Eski yazım burada
        //    1,25 yazar ve bina sessizce iki buçuk hücre kaplardı.
        [Test]
        public void LocalScaleFor_SameCellCountOnDoubleSizedArt_HalvesTheScale()
        {
            Sprite sixteen = MakeSprite(16, 16, pixelsPerUnit: 16f);
            Sprite thirtyTwo = MakeSprite(32, 32, pixelsPerUnit: 16f);

            Vector3 small = BoardSizing.LocalScaleFor(sixteen, 1.25f, Vector3.one);
            Vector3 large = BoardSizing.LocalScaleFor(thirtyTwo, 1.25f, Vector3.one);

            Assert.AreEqual(1.25f, small.x, Tolerance);
            Assert.AreEqual(0.625f, large.x, Tolerance);
        }

        // İKİ GÖRSEL AYNI DÜNYA BOYUNU KAPLIYOR — yukarıdaki testin aynı olguyu
        // ölçekten değil SONUÇTAN okuyan hâli. Ölçek sayısına bakmak "0,625
        // doğru mu" sorusunu insana bırakır; çizilen boy sorunun kendisidir.
        [Test]
        public void WorldHeightFor_SameCellCountOnDifferentArt_GivesSameHeight()
        {
            Sprite sixteen = MakeSprite(16, 16, pixelsPerUnit: 16f);
            Sprite thirtyTwo = MakeSprite(32, 32, pixelsPerUnit: 16f);

            float small = BoardSizing.WorldHeightFor(sixteen, 1.25f, Vector3.one);
            float large = BoardSizing.WorldHeightFor(thirtyTwo, 1.25f, Vector3.one);

            Assert.AreEqual(1.25f, small, Tolerance);
            Assert.AreEqual(small, large, Tolerance);
        }

        // ── HÜCRE ÖLÇÜSÜ 1 DEĞİLKEN. Grid.cellSize operatörün Inspector'ındaki
        //    bir sayı ve bir gün 1 olmayabilir; hesap ona bağlı değilse tahta
        //    büyüdüğü gün bütün yapılar eski boyunda kalırdı.

        [Test]
        public void LocalScaleFor_HalfSizedCells_HalvesTheScale()
        {
            Sprite sprite = MakeSprite(16, 16, pixelsPerUnit: 16f);

            Vector3 scale = BoardSizing.LocalScaleFor(sprite, 1.25f, new Vector3(0.5f, 0.5f, 1f));

            Assert.AreEqual(0.625f, scale.x, Tolerance);
        }

        [Test]
        public void LocalScaleFor_DoubleSizedCells_DoublesTheScale()
        {
            Sprite sprite = MakeSprite(16, 16, pixelsPerUnit: 16f);
            var cell = new Vector3(2f, 2f, 1f);

            Assert.AreEqual(2f, BoardSizing.LocalScaleFor(sprite, 1f, cell).x, Tolerance);
            Assert.AreEqual(2f, BoardSizing.WorldHeightFor(sprite, 1f, cell), Tolerance);
        }

        // ── İKİ ÜYE TEK HESAPTAN DOĞUYOR. Ayrı yazılsalardı biri değiştiğinde
        //    can barı görselin başından kopar ve hiçbir şey patlamazdı; bu test
        //    tam olarak o sessiz ayrışmayı yakalıyor.

        [Test]
        public void WorldHeightFor_MatchesDrawnHeightTimesLocalScale()
        {
            Sprite sprite = MakeSprite(16, 16, pixelsPerUnit: 16f);
            var cell = new Vector3(1.5f, 1.5f, 1f);

            Vector3 scale = BoardSizing.LocalScaleFor(sprite, 1.1f, cell);
            float drawnHeight = sprite.rect.height / sprite.pixelsPerUnit;

            Assert.AreEqual(
                drawnHeight * scale.y,
                BoardSizing.WorldHeightFor(sprite, 1.1f, cell),
                Tolerance);
        }

        // KARE OLMAYAN GÖRSEL EZİLMİYOR: iki eksene ayrı çarpan yazılsaydı
        // 16x32 bir bina hücrenin içine sıkıştırılır ve kimse bunu bir hata
        // olarak bildirmezdi. Ortak çarpan küçük eksenden seçildiği için görsel
        // kutunun İÇİNE sığıyor — yüksekliği tam bir hücre, genişliği yarım.
        [Test]
        public void LocalScaleFor_TallSprite_KeepsAspectAndFitsInsideTheBox()
        {
            Sprite tall = MakeSprite(16, 32, pixelsPerUnit: 16f);

            Vector3 scale = BoardSizing.LocalScaleFor(tall, 1f, Vector3.one);

            Assert.AreEqual(scale.x, scale.y, Tolerance, "En-boy oranı korunmalı.");
            Assert.AreEqual(0.5f, scale.x, Tolerance);
            Assert.AreEqual(1f, BoardSizing.WorldHeightFor(tall, 1f, Vector3.one), Tolerance);
        }

        [Test]
        public void LocalScaleFor_AlwaysLeavesTheThirdAxisAtOne()
        {
            Sprite sprite = MakeSprite(16, 16, pixelsPerUnit: 16f);

            Assert.AreEqual(1f, BoardSizing.LocalScaleFor(sprite, 2f, new Vector3(3f, 3f, 7f)).z);
            Assert.AreEqual(1f, BoardSizing.LocalScaleFor(null, 2f, Vector3.one).z);
        }

        // ── BOZUK GİRDİLER. Hiçbiri istisna atmıyor, hiçbiri sıfır ölçek
        //    üretmiyor — sıfır ölçek nesneyi ekrandan tamamen kaldırır ve tek
        //    satır bile hata basmaz, yani en pahalı olan hatadır.

        [Test]
        public void LocalScaleFor_NullSprite_FallsBackToOneWithoutThrowing()
        {
            Assert.AreEqual(Vector3.one, BoardSizing.LocalScaleFor(null, 1.25f, Vector3.one));
        }

        [Test]
        public void WorldHeightFor_NullSprite_FallsBackToOneCellHigh()
        {
            // BELGELENMİŞ VARSAYIM: ölçek 1 döndüğüne göre görsel kendi doğal
            // boyunda çizilir ve bu projenin ölçülmüş normu "bir sprite tam bir
            // hücre". Hücre iki birim ise cevap da iki birim.
            Assert.AreEqual(1f, BoardSizing.WorldHeightFor(null, 1.25f, Vector3.one), Tolerance);
            Assert.AreEqual(
                2f, BoardSizing.WorldHeightFor(null, 1.25f, new Vector3(2f, 2f, 1f)), Tolerance);
        }

        [Test]
        public void LocalScaleFor_ZeroCellSize_TreatsTheCellAsOneUnit()
        {
            Sprite sprite = MakeSprite(16, 16, pixelsPerUnit: 16f);

            Vector3 scale = BoardSizing.LocalScaleFor(sprite, 1.25f, Vector3.zero);

            Assert.AreEqual(1.25f, scale.x, Tolerance);
        }

        [Test]
        public void LocalScaleFor_NegativeCellSize_TreatsTheCellAsOneUnit()
        {
            Sprite sprite = MakeSprite(16, 16, pixelsPerUnit: 16f);

            Vector3 scale = BoardSizing.LocalScaleFor(sprite, 1f, new Vector3(-4f, -4f, 1f));

            Assert.AreEqual(1f, scale.x, Tolerance);
        }

        [Test]
        public void LocalScaleFor_ZeroOrNegativeCellCount_FallsBackToOneCell()
        {
            Sprite sprite = MakeSprite(16, 16, pixelsPerUnit: 16f);

            Assert.AreEqual(1f, BoardSizing.LocalScaleFor(sprite, 0f, Vector3.one).x, Tolerance);
            Assert.AreEqual(1f, BoardSizing.LocalScaleFor(sprite, -3f, Vector3.one).x, Tolerance);
        }

        /// <summary>
        /// Sıfır PPU'lu bir sprite MOTOR TARAFINDAN üretilemiyor; dolayısıyla
        /// ölçülemeyen görsel dalına oyundan gelinemez.
        /// </summary>
        // İDDİA BU TURDA DÜZELTİLDİ VE SEBEBİ ÖLÇÜLDÜ. Önceki hâli sıfır PPU ile
        // bir sprite kurup "iki yolu da kabul ediyor" diyordu; motor o kurulumu
        // ArgumentException ile reddettiği için istisna BoardSizing'e hiç
        // varmadan testin kendi kurulum satırından çıkıyordu. Yani test yeşil
        // olduğu sürece hiçbir şey ölçmüyor, kırmızı olduğunda da yanlış yeri
        // gösteriyordu.
        //
        // BUGÜNKÜ HÂLİ BİR KARAKTERİZASYON TESTİ: motorun kelepçesini adıyla
        // sabitliyor ve BoardSizing'deki sıfır-PPU savunmasının kime hizmet
        // ettiğini yazıyor — sprite'ı elle kuran çağırana, motora değil. Savunma
        // silinmiyor, çünkü bedeli yok ve ölçülemeyen görselin cevabı null
        // görselinkiyle AYNI olmak zorunda.
        [Test]
        public void ZeroPixelsPerUnit_IsRefusedByTheEngine_SoTheGuardServesHandBuiltSprites()
        {
            // TAM NİTELİKLİ AD, `using System;` DEĞİL — ölçüldü: bu dosya
            // UnityEngine'i de kullandığı için System eklemek `Object` adını
            // UnityEngine.Object ile belirsiz hâle getiriyor ve dosya derlenmiyor.
            Assert.Throws<System.ArgumentException>(
                () => MakeSprite(16, 16, pixelsPerUnit: 0f),
                "Unity refuses a zero pixels-per-unit sprite at construction");

            Assert.AreEqual(Vector3.one, BoardSizing.LocalScaleFor(null, 1.25f, Vector3.one));
            Assert.AreEqual(1f, BoardSizing.WorldHeightFor(null, 1.25f, Vector3.one), Tolerance);
        }

        // ── TANIM DOSYALARININ VARSAYILANLARI. Bu testler bir hesabı değil bir
        //    TASARIM SÖZÜNÜ sabitliyor: yapı birimden büyük doğar. Varsayılan
        //    sessizce 1'e düşseydi bina ile asker aynı boyda görünür ve hiçbir
        //    şey patlamazdı.

        [Test]
        public void UnitBlueprintAsset_DefaultsToExactlyOneCell()
        {
            var asset = ScriptableObject.CreateInstance<UnitBlueprintAsset>();
            disposables.Add(asset);

            Assert.AreEqual(1f, asset.BoardSizeInCells, Tolerance);
        }

        [Test]
        public void StructureBlueprintAsset_DefaultsToTheMeasuredCeiling()
        {
            var asset = ScriptableObject.CreateInstance<StructureBlueprintAsset>();
            disposables.Add(asset);

            Assert.AreEqual(1.25f, asset.BoardSizeInCells, Tolerance);
        }

        [Test]
        public void StructureBlueprintAsset_DefaultIsBiggerThanTheUnitDefault()
        {
            var unit = ScriptableObject.CreateInstance<UnitBlueprintAsset>();
            var structure = ScriptableObject.CreateInstance<StructureBlueprintAsset>();
            disposables.Add(unit);
            disposables.Add(structure);

            Assert.Greater(structure.BoardSizeInCells, unit.BoardSizeInCells);
        }

        // ALAN GERÇEKTEN SERİLEŞİYOR MU: yukarıdaki testler yalnız C#
        // başlatıcısını okuyor ve alan [SerializeField] taşımasa bile geçerdi.
        // Bu ikisi SerializedObject üstünden yazıp property'den okuyor, yani
        // Inspector'ın gördüğü yol ile oyunun okuduğu yolun AYNI alan olduğunu
        // sınıyor — SceneSetupTool da tam olarak bu yoldan yazıyor.

        [Test]
        public void StructureBlueprintAsset_BoardSizeInCells_IsWrittenThroughSerialization()
        {
            var asset = ScriptableObject.CreateInstance<StructureBlueprintAsset>();
            disposables.Add(asset);

            var so = new SerializedObject(asset);
            SerializedProperty cells = so.FindProperty("boardSizeInCells");
            Assert.IsNotNull(cells, "boardSizeInCells serileştirilmiş bir alan olmalı.");

            cells.floatValue = 1.1f;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(1.1f, asset.BoardSizeInCells, Tolerance);
        }

        [Test]
        public void UnitBlueprintAsset_BoardSizeInCells_IsWrittenThroughSerialization()
        {
            var asset = ScriptableObject.CreateInstance<UnitBlueprintAsset>();
            disposables.Add(asset);

            var so = new SerializedObject(asset);
            SerializedProperty cells = so.FindProperty("boardSizeInCells");
            Assert.IsNotNull(cells, "boardSizeInCells serileştirilmiş bir alan olmalı.");

            cells.floatValue = 1.4f;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(1.4f, asset.BoardSizeInCells, Tolerance);
        }

        /// <summary>
        /// Testin kendi görselini üretir; diskten hiçbir şey okumaz.
        /// </summary>
        // DİSKTEN OKUMUYOR ve bu bilerek: gerçek bir PNG okunsaydı test, o
        // dosyanın içe aktarma ayarına bağlanır ve birinin PPU'yu değiştirmesi
        // burayı kırardı. Ölçülmek istenen şey içe aktarma değil, HESAP.
        private Sprite MakeSprite(int width, int height, float pixelsPerUnit)
        {
            var texture = new Texture2D(width, height);
            disposables.Add(texture);

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, width, height),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit);

            if (sprite != null)
            {
                disposables.Add(sprite);
            }

            return sprite;
        }
    }
}
