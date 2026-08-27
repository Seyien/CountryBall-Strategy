using System.Collections.Generic;
using System.IO;
using GridStrategy.Unity;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GridStrategy.EditorTools
{
    /// <summary>
    /// Oyunun sahne kurulumunu TEK TIKLA yapar: görsellerin içe aktarma
    /// ayarları, blueprint varlıkları, birim prefab'ının takım görselleri,
    /// zemin, arayüz panelleri ve çöp kutusu düğmesi.
    ///
    /// NEDEN VAR: bu kurulum elle yapıldığında yirmiden fazla adım ve onlarca
    /// sürükleme demekti; her sürükleme unutulabilir bir borçtu ve unutulan
    /// referans sahnede SESSİZCE ölü bir panel bırakıyordu.
    ///
    /// TEKRARLANABİLİR: nesneleri yalnız yoksa yaratır, ama BAĞLANTILARI her
    /// çalıştırmada yeniden yazar. Böylece araç geliştikçe sahne onunla birlikte
    /// güncellenir; elle silip yeniden kurmak gerekmez.
    /// </summary>
    public static class SceneSetupTool
    {
        private const string ScenePath = "Assets/Scenes/SampleScene.unity";
        private const string BlueprintDir = "Assets/Game/Blueprints";
        private const string PrefabDir = "Assets/Game/Prefabs";
        private const string UnitPrefabPath = PrefabDir + "/Unit.prefab";
        private const string PaletteEntryPath = PrefabDir + "/PaletteEntry.prefab";

        private const string Battle = "Assets/Art/ThirdParty/Kenney/TinyBattle";
        private const string Derived = "Assets/Art/Derived/Kenney/TinyBattle";

        // ── Görsel yolları. Tek yerde toplandı ki bir sprite değiştiğinde
        //    aranacak tek bir liste olsun.
        private const string GrassPlain = Battle + "/Terrain/grass_plain_tile_0000.png";
        private const string GrassTufts = Battle + "/Terrain/grass_tufts_tile_0001.png";
        private const string GrassFlowers = Battle + "/Terrain/grass_flowers_tile_0002.png";

        private const string FriendlyIdle = Battle + "/Units/Friendly/friendly_vanguard_infantry_tile_0142.png";
        private const string FriendlyAttack = Battle + "/Units/Friendly/friendly_vanguard_infantry_attack_tile_0143.png";
        private const string EnemyIdle = Battle + "/Units/Enemy/enemy_vanguard_infantry_tile_0160.png";
        private const string EnemyAttack = Battle + "/Units/Enemy/enemy_vanguard_infantry_attack_tile_0161.png";

        private const string FriendlyScout = Battle + "/Units/Friendly/friendly_air_scout_tile_0136.png";
        private const string EnemyRaider = Battle + "/Units/Enemy/enemy_air_raider_tile_0155.png";

        private const string FriendlyHq = Battle + "/Buildings/friendly_industrial_pump_tile_0048.png";
        private const string FriendlyBarracks = Battle + "/Buildings/friendly_command_depot_tile_0045.png";
        private const string FriendlyFactory = Battle + "/Buildings/friendly_factory_tile_0047.png";
        private const string EnemyHq = Battle + "/Buildings/enemy_headquarters_tile_0066.png";
        private const string EnemyBarracks = Derived + "/Buildings/enemy_command_depot_from_tile_0045.png";
        private const string EnemyFactory = Battle + "/Buildings/enemy_factory_tile_0065.png";

        private const string TrashIcon = Battle + "/UI/icon_trash_tile_0192.png";
        private const string WhiteSquare = "Assets/Art/Generated/ui_white_square_4x4.png";
        private const string CellFrame = "Assets/Art/Generated/ui_cell_frame_16x16.png";

        [MenuItem("CountryBall/Sahneyi Kur (her şey)")]
        public static void BuildEverything()
        {
            ConfigureSpriteImports();
            EnsureBlueprints();
            ConfigureUnitPrefab();
            PaletteEntryView entryPrefab = EnsurePaletteEntryPrefab();

            BoardAdapter board = Object.FindObjectOfType<BoardAdapter>();
            if (board == null)
            {
                Debug.LogError("[SceneSetupTool] Sahnede BoardAdapter yok. Önce SampleScene'i aç.");
                return;
            }

            ConfigureBoard(board);
            EnsureEventSystem();
            Canvas canvas = EnsureCanvas();
            ProductionDirector director = EnsureDirector(board);

            EnsurePalette(canvas, director, entryPrefab);
            EnsureProductionPanel(canvas, director, entryPrefab);
            EnsureTrashButton(canvas, board);
            EnsureStatusBar(canvas, board);
            FrameCamera(board);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            Debug.Log("[SceneSetupTool] Kurulum tamam. Sahneyi kaydet (Ctrl+S).");
        }

        /// <summary>
        /// Komut satırından çağrılan sürüm: sahneyi kendisi açar ve kaydeder.
        /// </summary>
        // BATCHMODE'UN AKTİF SAHNESİ BOŞTUR: menüden çağrıldığında sahne zaten
        // açıktır, komut satırından çağrıldığında değildir.
        public static void BuildEverythingBatch()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            BuildEverything();
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[SceneSetupTool] Batch kurulum tamam ve sahne kaydedildi.");
        }

        // ─────────────────────── GÖRSEL İÇE AKTARMA ───────────────────────

        // 16 piksellik karolar 16 PPU ile TAM BİR HÜCRE eder. Bu ayar yapılmazsa
        // Unity varsayılan 100 PPU kullanır ve her sprite hücrenin altıda biri
        // kadar, minicik çizilir. Point filtre de şart: bilinear, piksel sanatını
        // bulanıklaştırır.
        private static void ConfigureSpriteImports()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Art" });
            int fixedCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }

                bool needsFix = importer.textureType != TextureImporterType.Sprite
                                || importer.spritePixelsPerUnit != 16f
                                || importer.filterMode != FilterMode.Point
                                || importer.textureCompression != TextureImporterCompression.Uncompressed;

                if (!needsFix)
                {
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 16f;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
                fixedCount++;
            }

            if (fixedCount > 0)
            {
                Debug.Log($"[SceneSetupTool] {fixedCount} görselin içe aktarma ayarı düzeltildi.");
            }
        }

        // ─────────────────────────── VARLIKLAR ───────────────────────────

        // Blueprint'ler oyuncunun paletten seçtiği şeylerin TANIMI. Adlar TÜRKÇE
        // ve oyun diliyle: "Command Depot" kimseye bir şey anlatmıyordu.
        private static void EnsureBlueprints()
        {
            Directory.CreateDirectory(BlueprintDir);

            UnitBlueprintAsset piyade = UnitBlueprint(
                "Unit_Piyade", "Piyade", FriendlyIdle, 30, 10, 1);
            UnitBlueprintAsset kesif = UnitBlueprint(
                "Unit_KesifUcagi", "Keşif Uçağı", FriendlyScout, 20, 6, 2);
            UnitBlueprintAsset dusmanPiyade = UnitBlueprint(
                "Unit_DusmanPiyadesi", "Düşman Piyadesi", EnemyIdle, 30, 10, 1);
            UnitBlueprintAsset akinci = UnitBlueprint(
                "Unit_Akinci", "Akıncı", EnemyRaider, 20, 6, 2);

            StructureBlueprint("Structure_Karargah", "Karargâh", FriendlyHq,
                70, new[] { piyade, kesif }, 3f);
            StructureBlueprint("Structure_Kisla", "Kışla", FriendlyBarracks,
                50, new[] { piyade }, 2f);
            StructureBlueprint("Structure_Fabrika", "Fabrika", FriendlyFactory,
                60, new[] { kesif }, 5f);

            StructureBlueprint("Structure_DusmanKarargahi", "Düşman Karargâhı", EnemyHq,
                70, new[] { dusmanPiyade, akinci }, 3f);
            StructureBlueprint("Structure_DusmanKislasi", "Düşman Kışlası", EnemyBarracks,
                50, new[] { dusmanPiyade }, 2f);
            StructureBlueprint("Structure_DusmanFabrikasi", "Düşman Fabrikası", EnemyFactory,
                60, new[] { akinci }, 5f);
        }

        // ALANLAR HER ÇALIŞTIRMADA YENİDEN YAZILIR: varlık zaten varsa erken
        // dönseydi, araçtaki bir düzeltme (yeni sprite, düzeltilmiş ad) eski
        // varlıklara hiç ulaşmazdı.
        private static UnitBlueprintAsset UnitBlueprint(
            string fileName, string displayName, string spritePath,
            int maxHealth, int damage, int attackRange)
        {
            string path = $"{BlueprintDir}/{fileName}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<UnitBlueprintAsset>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<UnitBlueprintAsset>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var so = new SerializedObject(asset);
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("icon").objectReferenceValue = Sprite(spritePath);
            so.FindProperty("maxHealth").intValue = maxHealth;
            so.FindProperty("damage").intValue = damage;
            so.FindProperty("attackRange").intValue = attackRange;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static void StructureBlueprint(
            string fileName, string displayName, string spritePath,
            int maxHealth, UnitBlueprintAsset[] produces, float productionSeconds)
        {
            string path = $"{BlueprintDir}/{fileName}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<StructureBlueprintAsset>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<StructureBlueprintAsset>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var so = new SerializedObject(asset);
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("icon").objectReferenceValue = Sprite(spritePath);
            so.FindProperty("maxHealth").intValue = maxHealth;
            so.FindProperty("productionSeconds").floatValue = productionSeconds;

            SerializedProperty list = so.FindProperty("produces");
            list.arraySize = produces.Length;
            for (int i = 0; i < produces.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = produces[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
        }

        // Birim prefab'ına DÖRT takım görselini yazar: iki takım × iki poz.
        // Bunlar olmadan iki taraf da aynı görünür ve saldırı ekranda okunmaz.
        private static void ConfigureUnitPrefab()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(UnitPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[SceneSetupTool] {UnitPrefabPath} bulunamadı.");
                return;
            }

            var view = prefab.GetComponent<UnitView>();
            if (view == null)
            {
                Debug.LogError("[SceneSetupTool] Unit.prefab üstünde UnitView yok.");
                return;
            }

            var so = new SerializedObject(view);
            so.FindProperty("friendlyIdle").objectReferenceValue = Sprite(FriendlyIdle);
            so.FindProperty("friendlyAttacking").objectReferenceValue = Sprite(FriendlyAttack);
            so.FindProperty("enemyIdle").objectReferenceValue = Sprite(EnemyIdle);
            so.FindProperty("enemyAttacking").objectReferenceValue = Sprite(EnemyAttack);
            so.ApplyModifiedPropertiesWithoutUndo();

            // Gövde çizicisi de mavi bekleme pozuyla başlasın; prefab önizlemesi
            // ile oyundaki ilk kare aynı görünsün.
            var body = prefab.GetComponent<SpriteRenderer>();
            if (body != null)
            {
                var bodySo = new SerializedObject(body);
                bodySo.FindProperty("m_Sprite").objectReferenceValue = Sprite(FriendlyIdle);
                bodySo.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorUtility.SetDirty(prefab);
            AssetDatabase.SaveAssets();
        }

        // Zemin ARTIK ÇİMEN. Eskiden dağınık toprak karoları kullanılıyordu ve
        // savaş alanı kirli/gürültülü görünüyordu; üç çimen varyantı hem sakin
        // hem de tekrar hissini kırıyor.
        private static void ConfigureBoard(BoardAdapter board)
        {
            var so = new SerializedObject(board);

            SerializedProperty terrain = so.FindProperty("terrainSprites");
            var grass = new[] { GrassPlain, GrassTufts, GrassPlain, GrassFlowers };
            terrain.arraySize = grass.Length;
            for (int i = 0; i < grass.Length; i++)
            {
                terrain.GetArrayElementAtIndex(i).objectReferenceValue = Sprite(grass[i]);
            }

            so.FindProperty("healthBarSprite").objectReferenceValue = Sprite(WhiteSquare);
            so.FindProperty("hoverFrameSprite").objectReferenceValue = Sprite(CellFrame);

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(board);
        }

        // Ekranın üstündeki durum şeridi. Sıra tabanlı bir oyunda "sıra kimde"
        // görünmeden oynanamaz; bu şerit o boşluğu kapatıyor.
        private static void EnsureStatusBar(Canvas canvas, BoardAdapter board)
        {
            GameObject go = Child(canvas.transform, "StatusBar")
                            ?? NewUi("StatusBar", canvas.transform, typeof(Image));

            var rect = Need<RectTransform>(go);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, 64f);
            Need<Image>(go).color = new Color(0.07f, 0.08f, 0.10f, 0.85f);

            GameObject turnGo = Child(rect, "TurnLabel") ?? NewUi("TurnLabel", rect, typeof(Text));
            var turnRect = Need<RectTransform>(turnGo);
            turnRect.anchorMin = new Vector2(0f, 0f);
            turnRect.anchorMax = new Vector2(0.42f, 1f);
            turnRect.offsetMin = new Vector2(16f, 0f);
            turnRect.offsetMax = Vector2.zero;
            var turnLabel = Need<Text>(turnGo);
            StyleText(turnLabel, "SIRA: SEN", 24, TextAnchor.MiddleLeft);

            GameObject selGo = Child(rect, "SelectionLabel") ?? NewUi("SelectionLabel", rect, typeof(Text));
            var selRect = Need<RectTransform>(selGo);
            selRect.anchorMin = new Vector2(0.42f, 0f);
            selRect.anchorMax = new Vector2(1f, 1f);
            selRect.offsetMin = Vector2.zero;
            selRect.offsetMax = new Vector2(-16f, 0f);
            var selLabel = Need<Text>(selGo);
            StyleText(selLabel, "Seçim yok", 20, TextAnchor.MiddleRight);

            BattleStatusView view = go.GetComponent<BattleStatusView>()
                                    ?? go.AddComponent<BattleStatusView>();

            var so = new SerializedObject(view);
            so.FindProperty("board").objectReferenceValue = board;
            so.FindProperty("turnLabel").objectReferenceValue = turnLabel;
            so.FindProperty("selectionLabel").objectReferenceValue = selLabel;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static PaletteEntryView EnsurePaletteEntryPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PaletteEntryPath);
            if (existing != null)
            {
                return existing.GetComponent<PaletteEntryView>();
            }

            Directory.CreateDirectory(PrefabDir);

            var root = new GameObject("PaletteEntry", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(104f, 116f);

            // Düğme boyutu SABİTLENİYOR: LayoutGroup içindeki bir öğe, tercih
            // ettiği boyutu söylemezse satır onu ezip birbirinin üstüne bindirir.
            // İç içe geçen paneli çözen satır tam olarak burası.
            var element = root.GetComponent<LayoutElement>();
            element.preferredWidth = 104f;
            element.preferredHeight = 116f;
            element.flexibleWidth = 0f;
            element.flexibleHeight = 0f;

            root.GetComponent<Image>().color = new Color(0.16f, 0.17f, 0.20f, 0.92f);

            // Seçim çerçevesi GÖVDENİN ALTINDA doğuyor ki simge onun üstüne
            // çizilsin; ters sırada seçili düğmenin simgesi kaybolurdu.
            Image frame = AddImageChild(rootRect, "SelectionFrame", new Color(1f, 0.85f, 0.25f, 1f));
            Stretch(frame.rectTransform, -3f);
            frame.enabled = false;

            Image icon = AddImageChild(rootRect, "Icon", Color.white);
            icon.preserveAspect = true;
            icon.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            icon.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            icon.rectTransform.pivot = new Vector2(0.5f, 1f);
            icon.rectTransform.anchoredPosition = new Vector2(0f, -6f);
            icon.rectTransform.sizeDelta = new Vector2(72f, 72f);

            Text label = AddTextChild(rootRect, "Label", string.Empty, 15, TextAnchor.MiddleCenter);
            label.rectTransform.anchorMin = new Vector2(0f, 0f);
            label.rectTransform.anchorMax = new Vector2(1f, 0f);
            label.rectTransform.pivot = new Vector2(0.5f, 0f);
            label.rectTransform.anchoredPosition = new Vector2(0f, 4f);
            label.rectTransform.sizeDelta = new Vector2(-6f, 32f);

            // Uzun ad düğmeyi taşırmasın: sığmayan yazı küçülsün, kırpılmasın.
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 9;
            label.resizeTextMaxSize = 15;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;

            var view = root.AddComponent<PaletteEntryView>();
            var so = new SerializedObject(view);
            so.FindProperty("label").objectReferenceValue = label;
            so.FindProperty("icon").objectReferenceValue = icon;
            so.FindProperty("selectionFrame").objectReferenceValue = frame;
            so.ApplyModifiedPropertiesWithoutUndo();

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, PaletteEntryPath);
            Object.DestroyImmediate(root);

            return saved.GetComponent<PaletteEntryView>();
        }

        // ─────────────────────────── SAHNE ───────────────────────────

        // EventSystem olmadan hiçbir UI düğmesi tıklama ALMAZ ve hata da vermez,
        // sadece sessizce çalışmaz. Bu yüzden ilk kurulan o.
        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }
        }

        // CanvasScaler, Flutter'daki MediaQuery/LayoutBuilder'ın Unity'deki
        // karşılığıdır: arayüzü ekranın GERÇEK boyutuna göre ölçekler. Referans
        // çözünürlüğe göre oranlar sabit kalır, match=0.5 ile hem geniş hem dar
        // ekranlarda dengeli davranır. Bunlar olmadan panel bir ekranda dev,
        // ötekinde okunamaz olur.
        private static Canvas EnsureCanvas()
        {
            Canvas existing = Object.FindObjectOfType<Canvas>();
            GameObject go = existing != null
                ? existing.gameObject
                : new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        private static ProductionDirector EnsureDirector(BoardAdapter board)
        {
            ProductionDirector existing = Object.FindObjectOfType<ProductionDirector>();
            if (existing == null)
            {
                existing = new GameObject("ProductionDirector").AddComponent<ProductionDirector>();
            }

            var so = new SerializedObject(existing);
            so.FindProperty("boardBehaviour").objectReferenceValue = board;
            so.ApplyModifiedPropertiesWithoutUndo();

            return existing;
        }

        private static void EnsurePalette(
            Canvas canvas, ProductionDirector director, PaletteEntryView entryPrefab)
        {
            GameObject go = Child(canvas.transform, "StructurePalette")
                            ?? NewUi("StructurePalette", canvas.transform, typeof(Image));

            // Sol kenar, dikey şerit. Tahta ekranın ortasında kaldığı için
            // panelin altına girmiyor.
            var rect = Need<RectTransform>(go);
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(12f, 0f);
            rect.sizeDelta = new Vector2(124f, -24f);
            Need<Image>(go).color = new Color(0.09f, 0.10f, 0.12f, 0.80f);

            Header(rect, "PlayerHeader", "SENİN", new Vector2(0f, 1f), -6f, new Color(0.45f, 0.75f, 1f));
            RectTransform playerRow = Row(rect, "PlayerRow", new Vector2(0f, 1f), -34f);

            Header(rect, "EnemyHeader", "DÜŞMAN", new Vector2(0f, 0f), 300f, new Color(1f, 0.5f, 0.45f));
            RectTransform enemyRow = Row(rect, "EnemyRow", new Vector2(0f, 0f), 8f);

            StructurePaletteView view = go.GetComponent<StructurePaletteView>()
                                        ?? go.AddComponent<StructurePaletteView>();

            var so = new SerializedObject(view);
            so.FindProperty("director").objectReferenceValue = director;
            so.FindProperty("entryPrefab").objectReferenceValue = entryPrefab;
            so.FindProperty("playerRow").objectReferenceValue = playerRow;
            so.FindProperty("enemyRow").objectReferenceValue = enemyRow;
            Blueprints(so, "playerStructures",
                "Structure_Karargah", "Structure_Kisla", "Structure_Fabrika");
            Blueprints(so, "enemyStructures",
                "Structure_DusmanKarargahi", "Structure_DusmanKislasi", "Structure_DusmanFabrikasi");
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureProductionPanel(
            Canvas canvas, ProductionDirector director, PaletteEntryView entryPrefab)
        {
            GameObject go = Child(canvas.transform, "ProductionPanel")
                            ?? NewUi("ProductionPanel", canvas.transform, typeof(Image));

            // Alt orta. Sol paletin sağından başlıyor ki ikisi ÜST ÜSTE BİNMESİN —
            // eski yerleşimde panel ekranın tamamına yayılıyor ve paletin altına
            // giriyordu.
            var rect = Need<RectTransform>(go);
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(60f, 12f);
            rect.sizeDelta = new Vector2(560f, 172f);
            Need<Image>(go).color = new Color(0.09f, 0.10f, 0.12f, 0.85f);

            Header(rect, "PanelHeader", "ÜRETİLECEK BİRİM", new Vector2(0f, 1f), -4f, Color.white);

            GameObject rowGo = Child(rect, "Row")
                               ?? NewUi("Row", rect, typeof(HorizontalLayoutGroup));
            var row = Need<RectTransform>(rowGo);
            row.anchorMin = new Vector2(0f, 0f);
            row.anchorMax = new Vector2(1f, 1f);
            row.offsetMin = new Vector2(8f, 8f);
            row.offsetMax = new Vector2(-8f, -30f);

            var layout = Need<HorizontalLayoutGroup>(rowGo);
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            GameObject emptyGo = Child(rect, "EmptyLabel") ?? NewUi("EmptyLabel", rect, typeof(Text));
            var emptyLabel = Need<Text>(emptyGo);
            StyleText(emptyLabel, "Üretim yapan bir yapı seç", 20, TextAnchor.MiddleCenter);
            var emptyRect = Need<RectTransform>(emptyGo);
            emptyRect.anchorMin = new Vector2(0f, 0f);
            emptyRect.anchorMax = new Vector2(1f, 1f);
            emptyRect.offsetMin = new Vector2(8f, 8f);
            emptyRect.offsetMax = new Vector2(-8f, -30f);

            ProductionPanelView view = go.GetComponent<ProductionPanelView>()
                                       ?? go.AddComponent<ProductionPanelView>();

            var so = new SerializedObject(view);
            so.FindProperty("director").objectReferenceValue = director;
            so.FindProperty("entryPrefab").objectReferenceValue = entryPrefab;
            so.FindProperty("row").objectReferenceValue = row;
            so.FindProperty("emptyLabel").objectReferenceValue = emptyLabel;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureTrashButton(Canvas canvas, BoardAdapter board)
        {
            GameObject go = Child(canvas.transform, "TrashButton")
                            ?? NewUi("TrashButton", canvas.transform, typeof(Image), typeof(Button));

            // Sağ alt köşe: tahtayı kapatmayan, fareyle en kolay ulaşılan yer.
            var rect = Need<RectTransform>(go);
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-20f, 20f);
            rect.sizeDelta = new Vector2(88f, 88f);

            var image = Need<Image>(go);
            image.sprite = Sprite(TrashIcon);
            image.preserveAspect = true;
            image.color = Color.white;

            // ETİKET SİMGENİN İÇİNDE DEĞİL, ALTINDA. Eskiden ikisi aynı dikdörtgeni
            // paylaşıyordu ve yazı çöp kutusunun içine binmiş görünüyordu.
            GameObject labelGo = Child(rect, "Label") ?? NewUi("Label", rect, typeof(Text));
            var label = Need<Text>(labelGo);
            StyleText(label, "SİL", 17, TextAnchor.UpperCenter);
            var labelRect = Need<RectTransform>(labelGo);
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 0f);
            labelRect.pivot = new Vector2(0.5f, 1f);
            labelRect.anchoredPosition = new Vector2(0f, -4f);
            labelRect.sizeDelta = new Vector2(0f, 22f);

            // ONCLICK KODDAN BAĞLANIYOR ve önce TEMİZLENİYOR: araç ikinci kez
            // çalıştığında aynı çağrı ikinci kez eklenseydi tek tıklama iki
            // kaldırma yapardı.
            var button = Need<Button>(go);
            for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            {
                UnityEventTools.RemovePersistentListener(button.onClick, i);
            }

            UnityEventTools.AddVoidPersistentListener(button.onClick, board.RemoveSelectedFromUi);
        }

        // Tahtanın merkezine bak ve tamamını çerçevele. Kamera bunu yapmazsa
        // oyuncu tahtanın yarısını hiç görmez.
        private static void FrameCamera(BoardAdapter board)
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                Debug.LogWarning("[SceneSetupTool] MainCamera etiketli kamera yok; çerçeveleme atlandı.");
                return;
            }

            var so = new SerializedObject(board);
            float width = so.FindProperty("width").intValue;
            float height = so.FindProperty("height").intValue;

            camera.orthographic = true;
            camera.transform.position = new Vector3(width * 0.5f, height * 0.5f, -10f);

            float aspect = camera.aspect > 0.01f ? camera.aspect : 16f / 9f;
            camera.orthographicSize = Mathf.Max(height * 0.5f, (width * 0.5f) / aspect) + 0.6f;
        }

        // ─────────────────────────── YARDIMCILAR ───────────────────────────

        private static Sprite Sprite(string path)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogError($"[SceneSetupTool] Görsel bulunamadı: {path}");
            }

            return sprite;
        }

        private static GameObject Child(Transform parent, string name)
        {
            Transform found = parent.Find(name);
            return found == null ? null : found.gameObject;
        }

        /// <summary>
        /// Bileşeni verir; nesnede yoksa EKLER.
        /// </summary>
        // BU YARDIMCI BİR KOLAYLIK DEĞİL, BİR ONARIM. Araç ikinci kez
        // çalıştığında sahnedeki nesneleri yeniden yaratmıyor, var olanları
        // buluyor. Ama önceki turda yaratılmış bir nesne, aracın O ZAMANKİ
        // sürümünün eklediği bileşenleri taşır — sonradan eklenen bir bileşen
        // orada YOKTUR. Düz GetComponent bu durumda null döner ve araç
        // NullReferenceException ile patlar; tam olarak böyle bir hata alındı
        // (StructurePalette'te Image yoktu).
        //
        // Kural: sahnede DEVAM EDEBİLECEK bir nesnenin bileşenine bu üye
        // üzerinden erişilir. Yeni yaratıldığı kesin olanlarda gerekmez, ama
        // zararı da yok — ve hangisinin hangisi olduğunu hatırlamak zorunda
        // kalmamak, unutulacak bir kural olmasından iyidir.
        private static T Need<T>(GameObject go) where T : Component
        {
            T existing = go.GetComponent<T>();
            return existing != null ? existing : go.AddComponent<T>();
        }

        private static GameObject NewUi(string name, Transform parent, params System.Type[] parts)
        {
            var types = new List<System.Type> { typeof(RectTransform) };
            types.AddRange(parts);

            var go = new GameObject(name, types.ToArray());
            go.transform.SetParent(parent, worldPositionStays: false);
            return go;
        }

        private static void Blueprints(SerializedObject so, string field, params string[] names)
        {
            SerializedProperty list = so.FindProperty(field);
            list.arraySize = names.Length;
            for (int i = 0; i < names.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<StructureBlueprintAsset>(
                        $"{BlueprintDir}/{names[i]}.asset");
            }
        }

        private static void Header(
            RectTransform parent, string name, string caption,
            Vector2 anchor, float offsetY, Color color)
        {
            GameObject go = Child(parent, name) ?? NewUi(name, parent, typeof(Text));

            var rect = Need<RectTransform>(go);
            rect.anchorMin = new Vector2(0f, anchor.y);
            rect.anchorMax = new Vector2(1f, anchor.y);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, offsetY);
            rect.sizeDelta = new Vector2(-8f, 24f);

            var text = Need<Text>(go);
            StyleText(text, caption, 16, TextAnchor.MiddleCenter);
            text.color = color;
        }

        private static RectTransform Row(
            RectTransform parent, string name, Vector2 anchor, float offsetY)
        {
            GameObject go = Child(parent, name) ?? NewUi(name, parent, typeof(VerticalLayoutGroup));

            var rect = Need<RectTransform>(go);
            rect.anchorMin = new Vector2(0f, anchor.y);
            rect.anchorMax = new Vector2(1f, anchor.y);
            rect.pivot = new Vector2(0.5f, anchor.y);
            rect.anchoredPosition = new Vector2(0f, offsetY);
            rect.sizeDelta = new Vector2(-8f, 380f);

            var layout = Need<VerticalLayoutGroup>(go);
            layout.spacing = 8f;
            layout.childAlignment = anchor.y > 0.5f ? TextAnchor.UpperCenter : TextAnchor.LowerCenter;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            return rect;
        }

        private static Image AddImageChild(RectTransform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, worldPositionStays: false);

            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Text AddTextChild(
            RectTransform parent, string name, string content, int size, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, worldPositionStays: false);

            var text = go.GetComponent<Text>();
            StyleText(text, content, size, anchor);
            return text;
        }

        private static void StyleText(Text text, string content, int size, TextAnchor anchor)
        {
            text.text = content;
            text.fontSize = size;
            text.alignment = anchor;
            text.color = Color.white;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            text.font = BuiltinFont();
        }

        // Bir kez bulunur, sonra paylaşılır.
        private static Font cachedFont;

        /// <summary>
        /// Unity'nin yerleşik fontunu verir.
        /// </summary>
        // ADI SÜRÜME GÖRE DEĞİŞİYOR: 2021.3'te "Arial.ttf", 2022.2 ve sonrasında
        // "LegacyRuntime.ttf". Sıra ÖNEMLİ çünkü GetBuiltinResource bulamadığında
        // sessizce null dönmüyor — Console'a iki satır hata basıyor. Yanlış adı
        // önce denemek, çalışan bir araçta her çağrıda gürültü üretiyordu.
        //
        // Bu yüzden önce bu projenin sürümündeki ad deneniyor, sonra ötekisi; ve
        // sonuç önbelleğe alınıyor ki tek bir kurulumda onlarca kez sorulmasın.
        // Sürüm yükseltildiğinde ikinci ad devreye girer, kod değişmez.
        private static Font BuiltinFont()
        {
            if (cachedFont != null)
            {
                return cachedFont;
            }

#if UNITY_2022_2_OR_NEWER
            cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
            cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif

            if (cachedFont == null)
            {
                Debug.LogWarning(
                    "[SceneSetupTool] Yerleşik font bulunamadı; etiketler yazısız görünebilir.");
            }

            return cachedFont;
        }

        private static void Stretch(RectTransform rect, float padding)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(padding, padding);
            rect.offsetMax = new Vector2(-padding, -padding);
        }
    }
}
