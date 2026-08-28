using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using GridStrategy.Core;
using GridStrategy.Unity;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GridStrategy.Tests.EditMode.Unity
{
    using Battle = global::GridStrategy.Battle.Battle;

    /// <summary>
    /// Zeminin ve kenar halkasının EKRANA nasıl indiğini sınayan testler.
    ///
    /// <b>BU KATMANIN İLK TESTLERİ.</b> Zemin bugüne kadar hücre başına bir
    /// GameObject kuruyordu ve hiçbir test ona dokunmuyordu; kusur da tam
    /// oradan çıktı — operatör tahtayı 100x50 yaptığında Console
    /// <i>"[Board] built 100x50 = 5000 cells"</i> yazdı ve hiçbir şey kırmızıya
    /// dönmedi, çünkü sayacak bir iddia yoktu.
    /// </summary>
    // TAHTA BÜYÜK KURULUYOR VE BU BİR TERCİH DEĞİL, ÖLÇÜNÜN KENDİSİ: 10x5'lik
    // bir tahtada 50 GameObject ile bir tilemap arasındaki fark gözlenemez.
    // Kusuru doğuran ölçek neyse, testin ölçtüğü ölçek de o olmalı.
    public sealed class BoardTilemapTests
    {
        private const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;

        // Operatörün bildirdiği tahta. Sayı uydurulmadı — Console satırından
        // alındı.
        private const int BigWidth = 100;
        private const int BigHeight = 50;

        private GameObject probe;
        private BoardAdapter adapter;
        private readonly List<Object> disposables = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            probe = new GameObject("BoardProbe");
            Grid grid = probe.AddComponent<Grid>();
            adapter = probe.AddComponent<BoardAdapter>();

            SetField("unityGrid", grid);
            SetField("terrainSprites", new[] { NewSprite(), NewSprite(), NewSprite() });
            SetField("borderSprites", new[] { NewSprite() });
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < disposables.Count; i++)
            {
                if (disposables[i] != null)
                {
                    Object.DestroyImmediate(disposables[i]);
                }
            }

            disposables.Clear();

            if (probe != null)
            {
                Object.DestroyImmediate(probe);
            }
        }

        /// <summary>
        /// 5000 hücre TEK bir çizici ile çiziliyor — hücre başına bir nesneyle
        /// değil.
        /// </summary>
        // ██ BU TEST, ÇEVRİLEN KARARIN KAYDIDIR ██
        // Eski kod her hücre için `new GameObject($"Cell_{x}_{y}")` çağırıyordu.
        // O hâlde bu iddia kırmızıya dönerdi: 100x50 tahtada 5000 SpriteRenderer
        // doğardı. Kural bir hız TAHMİNİ değil, sayılabilir bir OLGU — kaç çizici
        // var.
        [Test]
        public void BuildCellVisuals_OnFiveThousandCells_DrawsThemWithASingleRenderer()
        {
            UseBoard(BigWidth, BigHeight);

            Invoke("BuildCellVisuals");

            SpriteRenderer[] perCellRenderers = probe.GetComponentsInChildren<SpriteRenderer>(true);
            Assert.That(perCellRenderers.Length, Is.Zero,
                "hücre başına SpriteRenderer kalmamalı");

            TilemapRenderer[] maps = probe.GetComponentsInChildren<TilemapRenderer>(true);
            Assert.That(maps.Length, Is.EqualTo(2),
                "biri zemin biri halka olmak üzere tam iki tilemap");
        }

        /// <summary>
        /// Oynanabilir her hücreye bir karo yazılıyor.
        /// </summary>
        // ÇİZİCİ SAYMAK YETMEZ: tek bir tilemap kurup içine hiçbir karo
        // koymamak da üstteki iddiayı geçerdi. Bu test "az nesne" ile "doğru
        // ekran" arasındaki farkı tutuyor.
        [Test]
        public void BuildCellVisuals_PutsATileOnEveryPlayableCell()
        {
            UseBoard(6, 4);

            Invoke("BuildCellVisuals");

            Tilemap ground = FindMap("GroundTilemap");
            for (int x = 0; x < 6; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    Assert.That(ground.GetTile(new Vector3Int(x, y, 0)), Is.Not.Null,
                        $"({x},{y}) hücresinde karo yok");
                }
            }
        }

        /// <summary>
        /// Halka, oynanabilir alanın ÜSTÜNE karo yazmıyor.
        /// </summary>
        // ESKİ KODDA BU KURALI BİR `continue` SATIRI TAŞIYORDU; bugün dizinin
        // kendi boşluğu taşıyor. İddia aynı kaldı: aynı hücreye iki görsel
        // konsaydı zemin deseni kenarlarda değişmiş görünürdü.
        [Test]
        public void BuildBorderVisuals_LeavesThePlayableAreaEmptyOnItsOwnMap()
        {
            UseBoard(6, 4);

            Invoke("BuildCellVisuals");

            Tilemap border = FindMap("BorderTilemap");

            Assert.That(border.GetTile(new Vector3Int(-1, -1, 0)), Is.Not.Null,
                "halkanın kendi hücresinde karo olmalı");
            Assert.That(border.GetTile(new Vector3Int(2, 2, 0)), Is.Null,
                "oynanabilir hücre halka haritasında BOŞ kalmalı");
        }

        /// <summary>
        /// Aynı sprite için hep AYNI karo nesnesi dönüyor.
        /// </summary>
        // ██ FLYWEIGHT'İN ÖLÇÜSÜ BU TEST ██
        // Karo başına bir nesne kurulsaydı 5000 ScriptableObject doğardı ve
        // hiçbir şey patlamazdı — yalnız bellek sessizce büyürdü. Paylaşımın
        // kanıtı bir sayı değil, KİMLİK: iki çağrının aynı nesneyi döndürmesi.
        [Test]
        public void TileFor_CalledTwiceWithTheSameSprite_ReturnsTheSameInstance()
        {
            Sprite sprite = NewSprite();

            object first = Invoke("TileFor", sprite);
            object second = Invoke("TileFor", sprite);

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.SameAs(first), "aynı görünüm ikinci bir karo doğurmamalı");
        }

        /// <summary>
        /// Farklı sprite farklı karo alır.
        /// </summary>
        // ÜSTTEKİ İDDİANIN KARŞI KUTBU: önbellek her zaman aynı nesneyi
        // döndürseydi (anahtar yok sayılsaydı) o test yine yeşil kalırdı ve
        // bütün tahta tek bir dokuyla çizilirdi.
        [Test]
        public void TileFor_WithDifferentSprites_ReturnsDifferentTiles()
        {
            object first = Invoke("TileFor", NewSprite());
            object second = Invoke("TileFor", NewSprite());

            Assert.That(second, Is.Not.SameAs(first));
        }

        /// <summary>
        /// İkinci kurulum ikinci bir tilemap doğurmuyor.
        /// </summary>
        // HAVUZ KULLANAN KODLARIN KLASİK HATASININ TİLEMAP HÂLİ: aynı ad
        // aranmasaydı her yeniden kurulum sahneye bir tilemap daha eklerdi ve
        // eskiler altta karolarıyla asılı kalırdı.
        [Test]
        public void BuildCellVisuals_RunTwice_ReusesTheSameTilemaps()
        {
            UseBoard(6, 4);

            Invoke("BuildCellVisuals");
            Invoke("BuildCellVisuals");

            Assert.That(probe.GetComponentsInChildren<TilemapRenderer>(true).Length, Is.EqualTo(2));
        }

        private void UseBoard(int width, int height)
        {
            SetField("battle", new Battle(width, height));
        }

        private Tilemap FindMap(string name)
        {
            Transform child = probe.transform.Find(name);
            Assert.That(child, Is.Not.Null, $"'{name}' adlı tilemap yok");

            var map = child.GetComponent<Tilemap>();
            Assert.That(map, Is.Not.Null, $"'{name}' bir Tilemap taşımıyor");
            return map;
        }

        private Sprite NewSprite()
        {
            var texture = new Texture2D(4, 4);
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f));

            disposables.Add(sprite);
            disposables.Add(texture);
            return sprite;
        }

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
