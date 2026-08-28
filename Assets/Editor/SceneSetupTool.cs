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

        // BURADA ÜÇÜNCÜ BİR YOL YOK: Structure.prefab SİLİNDİ. Ölçü, guid
        // taramasıydı — dosyanın guid'ine Assets içinde kendi .meta'sı dışında
        // SIFIR atıf vardı, çünkü yapı görselini BoardAdapter koddan kuruyor.
        // Docs/ogrenme/12-unity-editor-baglama.md o prefabı "kodla kurulum her
        // tip için satır ekletir" gerekçesiyle savunmuştu ve o gerekçe artık
        // ÖLÇÜLEBİLİR biçimde yanlış: bugün on yapı türü var ve yapı görselini
        // kuran üyede tür başına tek satır yok, tür kimliği .asset dosyalarında
        // yaşıyor. Üstelik dosya yanlış sayılar taşıyordu — kökte çizim sırası 2
        // ve bir çocukta elle yazılmış 1.25 ölçek, yani tam da bu turun ortadan
        // kaldırdığı türden ham çarpan.
        //
        // GERİ GELME TETİKLEYİCİSİ: bir yapı görselinin TEK bir SpriteRenderer
        // ile anlatılamadığı gün — takım bayrağı, hasar katmanı ya da seçim
        // halkası gibi kardeş çiziciler doğduğunda. O gün prefab elle YAML
        // yazılarak değil, bu araçta üretilir ve boyutu yine BoardSizing'den
        // türetilir.

        private const string Battle = "Assets/Art/ThirdParty/Kenney/TinyBattle";
        private const string Derived = "Assets/Art/Derived/Kenney/TinyBattle";
        private const string Dungeon = "Assets/Art/ThirdParty/Kenney/TinyDungeon";
        private const string Town = "Assets/Art/ThirdParty/Kenney/TinyTown";

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

        // Menzilli birimin görseli. OKÇU/BÜYÜCÜ PİYADE GÖRSELİ ELDE YOK: dost
        // tarafta yalnızca kılıçlı piyade ile jet var, düşman tarafta ise
        // "enemy_ranged_infantry_tile_0161" adlı dosya piksel piksel
        // "enemy_vanguard_infantry_attack_tile_0161" ile örtüşüyor — yani
        // düşman piyadesinin SALDIRI KARESİ, ikinci bir adla duruyor. Onu
        // menzilli birim yapsaydık tahtada saldıran piyadeden ayırt edilemezdi.
        // Elde kalan tek simetrik ikili bu iki araç: dost nakliye kamyonu ile
        // düşmanın çift namlulu ağır aracı.
        private const string FriendlyArtillery = Battle + "/Units/Friendly/friendly_support_transport_tile_0131.png";
        private const string EnemyArtillery = Battle + "/Units/Enemy/enemy_heavy_vehicle_tile_0158.png";

        private const string FriendlyHq = Battle + "/Buildings/friendly_industrial_pump_tile_0048.png";
        private const string FriendlyBarracks = Battle + "/Buildings/friendly_command_depot_tile_0045.png";
        private const string FriendlyFactory = Battle + "/Buildings/friendly_factory_tile_0047.png";
        private const string FriendlyTurret = Battle + "/Buildings/friendly_turret_tile_0049.png";
        private const string FriendlyFort = Battle + "/Buildings/friendly_fort_tile_0050.png";
        private const string EnemyHq = Battle + "/Buildings/enemy_headquarters_tile_0066.png";
        private const string EnemyBarracks = Derived + "/Buildings/enemy_command_depot_from_tile_0045.png";
        private const string EnemyFactory = Battle + "/Buildings/enemy_factory_tile_0065.png";
        private const string EnemyTurret = Battle + "/Buildings/enemy_turret_tile_0067.png";
        private const string EnemyFort = Battle + "/Buildings/enemy_fort_tile_0068.png";

        // Menzilli vuruşta uçan cisim. OK GÖRSELİ ELDE YOK; TinyDungeon
        // ekipmanları arasından mavi kristalli asa seçildi, kılıç ile çekiç
        // 16 pikselde havada dönen bir cisim olarak okunmuyordu.
        private const string ProjectileStaff = Dungeon + "/Equipment/support_staff_tile_0130.png";

        // Tahtanın çevresine serilen toprak halkası. Çimin bittiği yerde ekran
        // artık boş kalmıyor; halka aynı zamanda kenar hücrelerindeki yapıların
        // taşmasını da yutuyor — taşma 1,25 hücrelik tavanda her yöne 0,125
        // hücre, yani eski 1,6'nın taşırdığının yarısından az.
        private const string DirtPlain = Town + "/Terrain/Dirt/dirt_fill_plain_tile_0025.png";
        private const string DirtScatterA = Town + "/Terrain/Dirt/dirt_fill_scatter_a_tile_0039.png";
        private const string DirtScatterB = Town + "/Terrain/Dirt/dirt_fill_scatter_b_tile_0040.png";
        private const string DirtScatterC = Town + "/Terrain/Dirt/dirt_fill_scatter_c_tile_0041.png";
        private const string DirtScatterD = Town + "/Terrain/Dirt/dirt_fill_scatter_d_tile_0042.png";

        private const string TrashIcon = Battle + "/UI/icon_trash_tile_0192.png";
        private const string WhiteSquare = "Assets/Art/Generated/ui_white_square_4x4.png";
        private const string CellFrame = "Assets/Art/Generated/ui_cell_frame_16x16.png";

        // 0. KATMANIN KAROSU ve NEDEN ÜRETİLDİ: elimizdeki bütün zemin karoları
        // tek tek ölçüldü ve ikisi de TEK RENKTEN ibaret çıktı —
        // dirt_fill_plain'in 256 pikselinin 256'sı, grass_plain'in 256
        // pikselinin 256'sı birebir aynı renk. Yani o karolardan serilen geniş
        // bir tabaka ile kameranın düz arka plan rengi EKRANDA AYNI ŞEYDİR.
        // Dağınık varyantlarda ise 256 pikselin ancak 19-21'i desen taşıyor ve o
        // desenin tonu boyanamıyor: toprak karosunun mavi kanalı 108'de tavan
        // yaptığı için çarpım filtresiyle deniz mavisi elde edilemiyor.
        //
        // Bu karo bembeyaz doğuyor: SpriteRenderer.color ile çarpıldığı için
        // istenen HER renge boyanabiliyor, 256 pikselinin 24'ü tabandan koyu
        // olduğu için boyandıktan sonra da dalga dokusunu koruyor.
        private const string WaterTile = "Assets/Art/Generated/terrain_water_16x16.png";

        // ── Arayüz ölçüleri. Paletteki düğme, üretim panelindeki düğme ve
        //    ikisini saran ızgara AYNI sayıları kullanır; üçü ayrı yerde
        //    yazılsaydı biri büyütüldüğünde öteki taşardı.
        //    44 piksel dokunma hedefi alt sınırdır ve bu ölçüler onun iki katı.
        private const float EntryWidth = 108f;
        private const float EntryHeight = 122f;
        private const float EntrySpacing = 8f;

        // Simge kutusu 16 pikselik karonun TAM DÖRT KATI. Ara bir sayı (60, 72)
        // seçilseydi pikseller eşit büyümez, kimi satır kalın kimi ince
        // görünürdü — piksel sanatında en görünür kusur budur.
        private const float EntryIconSize = 64f;
        private const float EntryLabelHeight = 40f;
        private const int PaletteColumns = 2;
        private const float StatusBarHeight = 64f;

        // Kenar halkasının kaç hücre kalınlığında olduğu. AYNI SAYI İKİ YERDE
        // OKUNUR: tahtaya yazılır ve kamera çerçevesine eklenir. İkisi ayrı
        // yazılsaydı halka kalınlaştığında kamera onu ekran dışında bırakırdı.
        //
        // İKİ HÜCREDEN BİRE İNDİ ve gerekçesi ölçüldü. Halkanın iki işi vardı:
        // kenar hücresindeki 1,6 ölçekli yapının 0,3 birimlik taşmasını yutmak
        // ve tahtaya çerçeve hissi vermek. Birinci işi bir hücre fazlasıyla
        // yapıyor (0,3 < 1); ikinci işi ise artık 0. katmanın üç kuşağı
        // yapıyor. İki hücrede ısrar etmenin ölçülmüş bedeli şuydu: 10x5 tahta
        // 16:9 ekranda halkayla birlikte 14x9 birim tutuyor ve kameranın
        // adaya bırakabildiği pay 0,5 birime düşüyordu — kumsal o payı tek
        // başına yiyor, sığlık ile deniz ekranın dışında kalıyordu. Halka bir
        // hücre olunca aynı ekran payıyla dört kuşak da görünüyor.
        private const int BorderThickness = 1;

        // Üretim panelinin iç ölçüleri. BU İKİSİ ESKİDEN EnsureProductionPanel
        // İÇİNDE YEREL SABİTTİ; kamera çerçevesi artık panelin kapladığı ekran
        // payını hesaba katmak zorunda olduğu için ikinci bir okuyucuları var.
        // Kopyalansalardı panel büyüdüğü gün kamera onu görmezden gelir ve
        // tahtanın altı sessizce örtülürdü.
        private const float PanelHeader = 34f;
        private const float PanelPadding = 8f;

        // Panellerin ekran kenarına olan payları.
        private const float PaletteMargin = 12f;
        private const float ProductionMargin = 14f;

        // CanvasScaler'ın referans çözünürlüğü. Panellerin payı bu çözünürlükte
        // piksel cinsinden biliniyor; kamera onu orana çevirip kullanıyor.
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;

        // ── 0. KATMAN: adanın çevresindeki dünya.
        //    Kuşakların genişliği hücre cinsinden. Kumsal tam 0,3 — kenar
        //    hücresine konmuş 1,6 ölçekli bir yapının taşma payının birebir
        //    aynısı, yani taşan yapı kumun üstünde duruyor. Sığlık onun iki
        //    buçuk katı: adayı denizden ayıracak kadar geniş, ekranı yiyecek
        //    kadar değil.
        //
        //    ÜÇÜ DE ÖLÇÜLDÜ, ÇÜNKÜ İLK DEĞERLER EKRANA SIĞMIYORDU: kumsal 0,45
        //    ve sığlık 1,6 iken sığlık kuşağı 10x5 tahtada ekranın üstünden
        //    1,55 birim taşıyordu — yani deniz yukarıda hiç görünmüyor, ada da
        //    ada gibi okunmuyordu.
        private const float BeachWidth = 0.3f;
        private const float ShoalWidth = 0.8f;

        // Kuşakların dışında kalması gereken en az deniz. Sıfır olsaydı sığlık
        // ekranın kenarına dayanır ve "adanın çevresi su" izlenimi kaybolurdu.
        private const float SeaSliver = 0.35f;

        // Denizin kameradan kaç kat büyük serileceği. 2,2 kat, 16:9'dan 3,9:1'e
        // kadar bütün en-boy oranlarında ekranı tamamen kaplıyor; kaplayamadığı
        // uçlarda da kameranın arka planı denizle aynı aileden olduğu için taşma
        // görünmüyor.
        private const float SeaOversize = 2.2f;

        // 0. KATMANIN ÇİZİM SIRASI. BoardAdapter'ın sırası yazılı: zemin 0,
        // birim ve yapı 1, imleç çerçevesi 2, can barı 3, kenar halkası -1.
        // Buradaki üçü o ikisinin de ALTINDA ve aralarında onar birim var:
        // araya yeni bir kuşak girdiğinde numaraları kaydırmak gerekmesin.
        private const int SeaOrder = -40;
        private const int ShoalOrder = -30;
        private const int BeachOrder = -20;

        // 0. katmanın kök nesnesinin adı. YENİDEN KURULUMDA ADIYLA TOPLANIP YOK
        // EDİLİYOR: sahnede duran ve bir alandan görünmeyen nesne yalnız adıyla
        // bulunabilir; toplanmasaydı her kurulumda üstüne bir deniz daha
        // binerdi. Aynı ölçü BoardAdapter'ın "BorderRing" halkasında da geçerli.
        private const string BackdropRootName = "WorldBackdrop";

        // Kameranın tahtaya bırakacağı paylar. PlayMargin oynanabilir tahtanın
        // çevresinde, panellerin ALTINA GİRMEYEN bir hücrelik nefes payı.
        private const float PlayMargin = 1f;

        // IslandMargin YAZILMIYOR, TÜRETİLİYOR: halkanın dışında görünmesi
        // gereken pay, tam olarak üç kuşağın toplamı. Elle yazılmış bir sayı
        // olsaydı — ve ilk yazıldığında öyleydi — bir kuşak genişletildiği gün
        // kamera eski payla kalır, kuşak sessizce ekranın dışına taşardı.
        // Ölçüldüğünde tam bu oldu: 1,6 birimlik sığlık, 0,5 birimlik payla
        // ekranın üstünden 1,55 birim taşıyordu.
        private const float IslandMargin = BeachWidth + ShoalWidth + SeaSliver;

        // ── 0. KATMANIN RENKLERİ. Üçü de AYNI beyaz karoyu boyuyor; aralarındaki
        //    tek fark renk ve kuşağın boyu.
        //    ÖLÇÜLMÜŞ REFERANS: Fatihcill/CountryBall-Strategy'nin kamerası
        //    (0.259, 0.753, 0.906) ile temizleniyor, yani açık gök mavisi.
        //    Operatörün "daha hoş" dediği yön bu; bütün palet o maviye akraba
        //    seçildi ve fabrika ayarı olan lacivert-grisi tamamen bırakıldı.
        private static readonly Color SeaColor = new Color(0.243f, 0.561f, 0.769f, 1f);
        private static readonly Color ShoalColor = new Color(0.404f, 0.753f, 0.871f, 1f);
        private static readonly Color BeachColor = new Color(0.949f, 0.851f, 0.651f, 1f);

        // Kameranın arka planı denizin bir tık AÇIĞI. Deniz zaten ekranı
        // kaplıyor, yani bu renk normalde hiç görünmüyor; görüldüğü tek an çok
        // geniş bir ekranda denizin bittiği yer ve orada ufuk gibi okunması
        // isteniyor. Bu yüzden sky mavisi ama denizle aynı aileden.
        private static readonly Color SkyColor = new Color(0.271f, 0.588f, 0.788f, 1f);

        [MenuItem("CountryBall/Sahneyi Kur (her şey)")]
        public static void BuildEverything()
        {
            ConfigureSpriteImports();
            EnsureBlueprints();
            ConfigureUnitPrefab();
            PaletteEntryView entryPrefab = EnsurePaletteEntryPrefab();

            BoardAdapter board = Object.FindAnyObjectByType<BoardAdapter>();
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
            EnsurePlacementGhost(board);

            // SIRA ÖNEMLİ: 0. katmanın denizi kameranın nereye baktığını ve ne
            // kadar gördüğünü okuyor, o yüzden çerçeveleme ondan ÖNCE koşuyor.
            Camera camera = FrameCamera(board);
            EnsureWorldBackdrop(board, camera);

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
            // DİSKTEN GELEN YENİ DOSYA ÖNCE İÇE AKTARILIYOR. Bu araç, Unity
            // odakta değilken klasöre eklenmiş bir görselden ancak bu satırdan
            // sonra haberdar olur; 0. katmanın su karosu tam olarak öyle geldi
            // ve bu satır olmasaydı ilk kurulumda "görsel bulunamadı" yazıp
            // denizi hiç kurmazdı.
            AssetDatabase.Refresh();

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

                // MESH TİPİ AYRI BİR NESNEDEN OKUNUYOR: TextureImporter bu ayarı
                // doğrudan bir özellik olarak açmıyor, yalnız TextureImporterSettings
                // taşıyor.
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);

                bool needsFix = importer.textureType != TextureImporterType.Sprite
                                || importer.spritePixelsPerUnit != 16f
                                || importer.filterMode != FilterMode.Point
                                || importer.textureCompression != TextureImporterCompression.Uncompressed
                                || settings.spriteMeshType != SpriteMeshType.FullRect;

                if (!needsFix)
                {
                    continue;
                }

                // FULL RECT ŞART, çünkü SpriteRenderer bir karoyu ancak tam
                // dörtgen ağ ile DÖŞEYEBİLİYOR (SpriteDrawMode.Tiled); sıkı ağda
                // Unity uyarı basıp karoyu gererek çiziyor ve 0. katmanın dokusu
                // dev lekelere dönüşüyor. Ölçüldü: bu projedeki bütün görseller
                // zaten FullRect, yani bu satır bugün hiçbir dosyayı yeniden
                // aktarmıyor — yalnız yarın eklenecek olanı garantiye alıyor.
                //
                // ÖNCE SetTextureSettings, SONRA tek tek alanlar: bu nesne
                // textureType ve filterMode gibi alanları da taşıyor, yani sırayı
                // ters kursaydık aşağıdaki kuralları eski değerlerle ezerdi.
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);

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

            // 0,8 SANİYE — tahtanın en hızlı vuruşu, saniyede 12,5 hasar.
            // Piyade menzil 1 ile vuruyor, yani her vuruş için düşmanın dibine
            // yürümek zorunda; hızı o riskin karşılığı.
            UnitBlueprintAsset piyade = UnitBlueprint(
                "Unit_Piyade", "Piyade", FriendlyIdle, 30, 10, 1, 0.8f);

            // 1 SANİYE — saniyede 6 hasar, piyadenin yarısından az. Keşif uçağı
            // bir hücre daha uzaktan vuruyor ve 20 canla en ince birim; işi
            // dövmek değil görmek ve taciz etmek.
            UnitBlueprintAsset kesif = UnitBlueprint(
                "Unit_KesifUcagi", "Keşif Uçağı", FriendlyScout, 20, 6, 2, 1f);

            // TOP ARABASI — tahtanın cam topu. 18 can: on hasarlı piyade onu İKİ
            // vuruşta düşürür, yani korumasız bıraktığın anda kaybedersin.
            // 12 hasar: tahtanın en sert tek vuruşu, çünkü bedeli o kırılganlık.
            // 3 menzil: her yöne üç hücre. Piyadenin 1, keşif uçağının 2 ve
            // taretin 2 menzilinin HEPSİNİN dışından vurur — bu birimin tek
            // varlık nedeni, karşılık göremeyeceği bir uzaklıktan dövmek.
            // 2,4 SANİYE — tahtanın en yavaş vuruşu, saniyede 5 hasar.
            // MENZİLİN BEDELİ BU: top arabası üç hücre uzaktan, yani piyadenin,
            // keşif uçağının ve taretin hepsinin dışından vuruyor. Hızlı da
            // vursaydı yakın dövüşün var olma sebebi kalmazdı — piyade saniyede
            // 12,5 hasar veriyor, yani göğüs göğüse gelmek hâlâ iki buçuk kat
            // daha çok hasar demek.
            UnitBlueprintAsset topArabasi = UnitBlueprint(
                "Unit_TopArabasi", "Top Arabası", FriendlyArtillery, 18, 12, 3, 2.4f);

            UnitBlueprintAsset dusmanPiyade = UnitBlueprint(
                "Unit_DusmanPiyadesi", "Düşman Piyadesi", EnemyIdle, 30, 10, 1, 0.8f);
            UnitBlueprintAsset akinci = UnitBlueprint(
                "Unit_Akinci", "Akıncı", EnemyRaider, 20, 6, 2, 1f);

            // Düşman tarafın aynadaki eşi: sayılar birebir aynı, çünkü iki taraf
            // arasındaki tek fark renk olmalı.
            UnitBlueprintAsset dusmanTopu = UnitBlueprint(
                "Unit_DusmanTopAraci", "Düşman Top Aracı", EnemyArtillery, 18, 12, 3, 2.4f);

            // ── SALDIRMAYAN YAPILAR: menzil 0 yazıldığı sürece bu yapılar
            //    savaşa hiç girmez; hasar sayısı da o yüzden 0.
            // SALDIRMAYAN YAPILARIN BEKLEMESİ DE 2 SANİYE. Menzil 0 olduğu
            // sürece bu sayı hiç okunmuyor; hepsine taretin temposu yazılıyor ki
            // bir gün bu yapılardan biri silahlandırıldığında tahtada ikinci bir
            // ateş hızı doğmasın.
            // KARARGÂH TAVANDA: oyuncunun tahtaya baktığında ilk bulması
            //    gereken yapı, ve kaybedince oyunu bitiren yapı da o.
            StructureBlueprint("Structure_Karargah", "Karargâh", FriendlyHq,
                70, 0, 0, 2f, new[] { piyade, kesif }, 3f, StructureSizeCeiling);
            // KIŞLA 1,15: tek tip asker basan en küçük üretim binası, yine de
            //    birimin 1 hücresinin üstünde ki bina olduğu okunsun.
            StructureBlueprint("Structure_Kisla", "Kışla", FriendlyBarracks,
                50, 0, 0, 2f, new[] { piyade }, 2f, 1.15f);
            // FABRİKA 1,2: iki ayrı araç üretiyor ve canı kışladan yüksek;
            //    tahtadaki ağırlığı karargâh ile kışlanın arasında durmalı.
            StructureBlueprint("Structure_Fabrika", "Fabrika", FriendlyFactory,
                60, 0, 0, 2f, new[] { kesif, topArabasi }, 5f, 1.2f);

            // ── TARET — tahtanın ilk SALDIRAN yapısı.
            //    45 can: en zayıf yapı (kışla 50, fabrika 60, karargâh 70), çünkü
            //    menzili canıyla satın alıyor.
            //    8 hasar: piyadenin 10'unun altında ve piyadenin 30 canının çok
            //    altında — taret hiçbir savaşçıyı tek vuruşta öldüremez, en iyi
            //    ihtimalle dört vuruşta düşürür.
            //    2 menzil: 10x5 tahtada her yöne iki hücre, yani ortadan konunca
            //    tahtanın yarısı. 3 olsaydı tek taret tahtanın yetmişini tarar
            //    ve NEREYE koyduğun önemsizleşirdi; 1 olsaydı yakın dövüşten
            //    farkı kalmazdı. Bu menzil ayrıca top arabasının 3'ünün altında
            //    kalıyor — taret sökmenin bir yolu böylece açık duruyor.
            //    Üretim yok: taret asker basmaz, yalnızca vurur.
            //    2 SANİYE BEKLEME: saniyede 4 hasar, yani tahtanın en yavaş
            //    savaşçısı olan top arabasından bile daha yavaş. GEREKÇE, TARETİN
            //    ELİ OLMAMASI: oyuncunun birimleri ancak tıklandıklarında
            //    vuruyor, taret ise kendiliğinden ve durmadan ateş ediyor.
            //    Piyadenin 0,8 saniyesi verilseydi bir taret, hiçbir emek
            //    istemeden tahtanın en verimli silahı olurdu.
            //    1,1 HÜCRE — tahtanın en küçük yapısı, çünkü taret bir bina
            //    değil bir SİLAH YUVASI; yine de birimin 1 hücresinin üstünde
            //    kalıyor ki uzaktan bakan oyuncu onu asker sanmasın.
            StructureBlueprint("Structure_Taret", "Taret", FriendlyTurret,
                45, 8, 2, 2f, NoUnits, 0f, 1.1f);

            // ── HİSAR — vurmayan ama yıkılmayan duvar.
            //    110 can: karargâhın bir buçuk katından fazlası; on hasarlı
            //    piyadenin on bir vuruşuna dayanır, yani bir koridoru gerçekten
            //    tıkar. Menzil 0 ve üretim yok: tek işi orada durmak.
            //    1,15 HÜCRE — kışla ile aynı ağırlık: hisar bir koridoru tıkar,
            //    ama tıkadığı koridorun kendisini göremez hâle getirmemeli.
            StructureBlueprint("Structure_Hisar", "Hisar", FriendlyFort,
                110, 0, 0, 2f, NoUnits, 0f, 1.15f);

            // Aynadaki eş: boyut merdiveni de birebir kopyalanıyor, çünkü iki
            // taraf arasındaki tek fark renk olmalı.
            StructureBlueprint("Structure_DusmanKarargahi", "Düşman Karargâhı", EnemyHq,
                70, 0, 0, 2f, new[] { dusmanPiyade, akinci }, 3f, StructureSizeCeiling);
            StructureBlueprint("Structure_DusmanKislasi", "Düşman Kışlası", EnemyBarracks,
                50, 0, 0, 2f, new[] { dusmanPiyade }, 2f, 1.15f);
            StructureBlueprint("Structure_DusmanFabrikasi", "Düşman Fabrikası", EnemyFactory,
                60, 0, 0, 2f, new[] { akinci, dusmanTopu }, 5f, 1.2f);
            StructureBlueprint("Structure_DusmanTareti", "Düşman Tareti", EnemyTurret,
                45, 8, 2, 2f, NoUnits, 0f, 1.1f);
            StructureBlueprint("Structure_DusmanHisari", "Düşman Hisarı", EnemyFort,
                110, 0, 0, 2f, NoUnits, 0f, 1.15f);
        }

        // Üretim yapmayan yapıların boş listesi. Her çağrıda `new[0]` yazmak
        // yerine tek bir okunur ad: "bu yapı asker basmaz" cümlesi imzada görünsün.
        private static readonly UnitBlueprintAsset[] NoUnits = new UnitBlueprintAsset[0];

        // YAPI BOYUTUNUN ÖLÇÜLMÜŞ TAVANI, bir tercih değil. Eski tek sayı 1,6
        // idi ve yan yana iki bina, çizilen genişliğin yüzde otuz yedi buçuğu
        // kadar üst üste biniyordu. Bu sayının üstüne çıkan her yapı komşusunu
        // boyamaya başlar, o yüzden aşağıdaki merdivenin hiçbir basamağı bunu
        // aşmıyor. Yerleştirme hayaleti de aynı sayıyı kullanıyor.
        //
        // SAYI BURAYA KOPYALANMIYOR, TAHTADAN OKUNUYOR — ve bu, kendi kuralımızın
        // gereği: aynı tasarım niceliğinin iki yazılabilir sahibi olursa hangisinin
        // doğru olduğunu hiçbir derleyici söylemez. Araç bu sayıyı varlık
        // dosyalarına yazıyor, tahta ise tanım susunca aynı sayıya düşüyor; ikisi
        // ayrıştığı gün oyuncu önizlemede bir boyut görüp başka bir boyutta bina
        // koyar ve ekranda görünen tek şey bir tuhaflık olurdu.
        private const float StructureSizeCeiling = BoardAdapter.DefaultStructureSizeInCells;

        // ALANLAR HER ÇALIŞTIRMADA YENİDEN YAZILIR: varlık zaten varsa erken
        // dönseydi, araçtaki bir düzeltme (yeni sprite, düzeltilmiş ad) eski
        // varlıklara hiç ulaşmazdı.
        private static UnitBlueprintAsset UnitBlueprint(
            string fileName, string displayName, string spritePath,
            int maxHealth, int damage, int attackRange, float attackCooldownSeconds)
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

            // BİR ASKER TAM BİR HÜCRE, VE BU SAYI ÇAĞRI SATIRINDA DEĞİL BURADA.
            // Gerekçe, kuralın tekliği: birim hücresinden taşarsa komşu hücreyi
            // boyar ve oyuncu "oraya yürüyebilir miyim" sorusunu gözle
            // cevaplayamaz. Altı çağrı satırına altı kez 1 yazsaydık, birini
            // değiştirmek yalnızca o birimi bozmakla kalmaz, kuralın kural
            // olduğunu da görünmez yapardı — yapı merdiveni ise gerçekten
            // türden türe değiştiği için çağrı satırında duruyor.
            SerializedProperty cells = Optional(so, "boardSizeInCells");
            if (cells != null)
            {
                cells.floatValue = 1f;
            }

            // BEKLEME SÜRESİ DE ARTIK BURADAN GEÇİYOR. Alan varlıkta kendi
            // başlatıcısıyla doğuyor, yani yazılmasa da oyun çalışırdı; ama o
            // zaman tahtanın saldırı temposu hiçbir tasarım belgesinde
            // görünmez, yalnız bir alanın varsayılanında saklı kalırdı.
            SerializedProperty cooldown = Optional(so, "attackCooldownSeconds");
            if (cooldown != null)
            {
                cooldown.floatValue = attackCooldownSeconds;
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(asset);
            return asset;
        }

        /// <summary>
        /// Bir yapı türünün tanım dosyasını yazar.
        /// </summary>
        // HASAR VE MENZİL ARTIK BURADAN GEÇİYOR. StructureBlueprintAsset bu iki
        // alanı en baştan taşıyordu ama bu yardımcı onlara hiç dokunmuyordu; her
        // yapı, dosyada yeri hazır dururken SALDIRMAYAN doğuyordu. Taret tam
        // olarak bu boşluk yüzünden kurulamıyordu.
        //
        // Menzil 0 "bu yapı hiç saldırmaz" demektir ve hasar o durumda okunmaz;
        // yine de 0 yazılıyor ki çağrı satırına bakan biri niyeti görsün.
        private static void StructureBlueprint(
            string fileName, string displayName, string spritePath,
            int maxHealth, int damage, int attackRange, float attackCooldownSeconds,
            UnitBlueprintAsset[] produces, float productionSeconds, float boardSizeInCells)
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
            so.FindProperty("damage").intValue = damage;
            so.FindProperty("attackRange").intValue = attackRange;
            so.FindProperty("productionSeconds").floatValue = productionSeconds;

            // BOYUT ÇAĞRI SATIRINDAN GELİYOR, ÇÜNKÜ GERÇEKTEN DEĞİŞİYOR: taret
            // bir silah yuvası, karargâh bir üs ve ikisinin tahtada aynı yeri
            // kaplaması oyuncuya yanlış bir hiyerarşi anlatır.
            SerializedProperty cells = Optional(so, "boardSizeInCells");
            if (cells != null)
            {
                cells.floatValue = boardSizeInCells;
            }

            // SALDIRMAYAN YAPIDA DA YAZILIYOR: menzil 0 iken bu sayı hiç
            // okunmuyor, ama boş bırakılsaydı yapının menzili bir gün
            // yükseltildiğinde tempo, kimsenin seçmediği bir varsayılandan
            // gelirdi.
            SerializedProperty cooldown = Optional(so, "attackCooldownSeconds");
            if (cooldown != null)
            {
                cooldown.floatValue = attackCooldownSeconds;
            }

            // VARSAYILAN ÜRETİM İNDİSİ SIFIRA ÇEKİLİYOR: üretim listesi kısalan
            // bir yapıda eski indis aralık dışında kalır ve varlık her açılışta
            // konsola hata basar. Liste burada yazıldığı üstüne indis de burada
            // yazılmalı.
            so.FindProperty("defaultProducedIndex").intValue = 0;

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

        /// <summary>
        /// Tahtanın görünüşünü ve savaş kipini yazar.
        /// </summary>
        // Zemin ARTIK ÇİMEN. Eskiden dağınık toprak karoları kullanılıyordu ve
        // savaş alanı kirli/gürültülü görünüyordu; üç çimen varyantı hem sakin
        // hem de tekrar hissini kırıyor.
        //
        // TAHTANIN ÖLÇÜLERİNE VE SAYILARINA DOKUNULMUYOR. width, height, damage,
        // maxHealth gibi alanlar sahnede operatörün elleriyle ayarlanmış
        // durumda; araç onları yeniden yazsaydı her kurulumda o ayarları geri
        // alırdı. Burada yalnızca GÖRSEL bağlantılar ve kip yazılır.
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

            // KENAR HALKASI: çimin bittiği yer artık boş ekran değil, toprak.
            // Sade karo listede DÖRT kez geçiyor, dağınık olanlar birer kez —
            // halka böylece sakin kalıyor ve tekrar hissi yine de kırılıyor.
            SerializedProperty borders = Optional(so, "borderSprites");
            if (borders != null)
            {
                var dirt = new[]
                {
                    DirtPlain, DirtScatterA, DirtPlain, DirtScatterB,
                    DirtPlain, DirtScatterC, DirtPlain, DirtScatterD,
                };

                borders.arraySize = dirt.Length;
                for (int i = 0; i < dirt.Length; i++)
                {
                    borders.GetArrayElementAtIndex(i).objectReferenceValue = Sprite(dirt[i]);
                }
            }

            // TEK HÜCRE KALINLIK: kenardaki yapılar 1,6 ölçekle çiziliyor ve
            // hücrelerinden her yöne 0,3 birim taşıyor; bir hücrelik halka o
            // taşmayı üç kat fazlasıyla örtüyor. Halka eskiden iki hücreydi ve
            // fazladan sıranın işi tahtaya çerçeve hissi vermekti — o işi artık
            // 0. katmanın kumsal, sığlık ve deniz kuşakları yapıyor. Ekran payı
            // ölçülü bir kaynak: ikinci sıra durduğu sürece o üç kuşaktan
            // ikisi ekrana sığmıyordu.
            SerializedProperty thickness = Optional(so, "borderThickness");
            if (thickness != null)
            {
                thickness.intValue = BorderThickness;
            }

            // SERBEST KİP: bu sahne bir kum havuzu — oyuncu iki tarafı da
            // kuruyor, sırayı bekleten bir kampanya yok. Alternating kipinde
            // düşman yapısını koyup denemek imkânsızdı.
            //
            // ENUM SIRA NUMARASIYLA YAZILIYOR: TurnMode.FreeForAll bildirimdeki
            // ikinci üye, yani enumValueIndex 1.
            SerializedProperty turn = Optional(so, "turnMode");
            if (turn != null)
            {
                turn.enumValueIndex = 1;
            }

            SerializedProperty projectile = Optional(so, "projectileSprite");
            if (projectile != null)
            {
                projectile.objectReferenceValue = Sprite(ProjectileStaff);
            }

            // 2 SANİYE — VE BU SAYI TARETİN attackCooldownSeconds'IYLA BİLEREK
            // AYNI. Aynı kural artık iki yerde yaşıyor: tahtanın üstündeki bu
            // alan ve yapı tanımındaki bekleme süresi. İkisinden hangisinin
            // sözünün geçtiği bugün kesin değil (o çakışmayı başka bir dosya
            // çözüyor); ikisine de aynı sayı yazılırsa hangisi kazanırsa kazansın
            // tahtadaki tempo değişmiyor. Sayıların ayrışması, ayrışmanın
            // ekranda hiçbir iz bırakmadan tempoyu değiştirmesi demekti.
            SerializedProperty fireSeconds = Optional(so, "structureFireSeconds");
            if (fireSeconds != null)
            {
                fireSeconds.floatValue = 2f;
            }

            // CAN BARI VE İMLEÇ ÇERÇEVESİ ARTIK BURADAN YAZILIYOR. İkisi de
            // sahnede elle bağlanmıştı ve bu bir borçtu: bağlantı koptuğu gün
            // BoardAdapter yalnızca bir uyarı basıyor, oyun sessizce barsız
            // oynanıyor. Yapıların "kötü görünmesi" şikâyetinin bir ayağı tam
            // olarak burada: canı görünmeyen bir yapı sadece bir resimdir.
            SerializedProperty healthBar = Optional(so, "healthBarSprite");
            if (healthBar != null)
            {
                healthBar.objectReferenceValue = Sprite(WhiteSquare);
            }

            SerializedProperty hoverFrame = Optional(so, "hoverFrameSprite");
            if (hoverFrame != null)
            {
                hoverFrame.objectReferenceValue = Sprite(CellFrame);
            }

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
            rect.sizeDelta = new Vector2(0f, StatusBarHeight);
            Need<Image>(go).color = new Color(0.07f, 0.08f, 0.10f, 0.92f);

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

        /// <summary>
        /// Paletteki ve üretim panelindeki tek bir düğmenin prefab'ını kurar:
        /// üstte simge, altta ad, ikisinin çevresinde seçim çerçevesi.
        /// </summary>
        // DÜZEN ARTIK HER KURULUMDA YENİDEN YAZILIYOR. Eski sürüm prefab varsa
        // hemen dönüyordu; yani düğmenin boyutu, yazı tipi ya da simge kutusu
        // burada düzeltildiğinde bir kez kurulmuş projelerde HİÇBİR ŞEY
        // değişmiyordu — düzeltme yalnızca temiz bir klasörde görünüyordu.
        // Aracın geri kalanı bağlantıları her seferinde yeniden yazıyor; bu
        // dosyanın da aynı kuralla yaşaması gerekiyordu.
        //
        // PREFAB YERİNDE AÇILIYOR, SİLİNİP YENİDEN YARATILMIYOR: silmek dosyanın
        // GUID'ini değiştirir ve o GUID'e bakan her serileştirilmiş alan
        // (sahnedeki iki görünüm) sessizce boşalır.
        private static PaletteEntryView EnsurePaletteEntryPrefab()
        {
            Directory.CreateDirectory(PrefabDir);

            bool existed = AssetDatabase.LoadAssetAtPath<GameObject>(PaletteEntryPath) != null;
            GameObject root = existed
                ? PrefabUtility.LoadPrefabContents(PaletteEntryPath)
                : new GameObject("PaletteEntry", typeof(RectTransform));

            var rootRect = Need<RectTransform>(root);
            rootRect.sizeDelta = new Vector2(EntryWidth, EntryHeight);

            // Düğme boyutu SABİTLENİYOR: LayoutGroup içindeki bir öğe, tercih
            // ettiği boyutu söylemezse satır onu ezip birbirinin üstüne bindirir.
            // İç içe geçen paneli çözen satır tam olarak burası. (Paletteki
            // GridLayoutGroup hücre boyutunu kendisi dayattığı hâlde bu alanlar
            // gerekli: üretim panelindeki HorizontalLayoutGroup onları okuyor.)
            var element = Need<LayoutElement>(root);
            element.preferredWidth = EntryWidth;
            element.preferredHeight = EntryHeight;
            element.minWidth = EntryWidth;
            element.minHeight = EntryHeight;
            element.flexibleWidth = 0f;
            element.flexibleHeight = 0f;

            // Zemin, arkasındaki çimenden ayrılacak kadar koyu: yazı beyaz
            // kaldığı sürece okunurluk zeminin koyuluğuna bağlı.
            Need<Image>(root).color = new Color(0.13f, 0.14f, 0.17f, 0.96f);

            Image frame = ImageChild(rootRect, "SelectionFrame", new Color(1f, 0.85f, 0.25f, 1f));
            Stretch(frame.rectTransform, -3f);
            frame.enabled = false;

            Image icon = ImageChild(rootRect, "Icon", Color.white);
            icon.preserveAspect = true;
            icon.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            icon.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            icon.rectTransform.pivot = new Vector2(0.5f, 1f);
            icon.rectTransform.anchoredPosition = new Vector2(0f, -8f);
            icon.rectTransform.sizeDelta = new Vector2(EntryIconSize, EntryIconSize);

            Text label = TextChild(rootRect, "Label", string.Empty, 16, TextAnchor.MiddleCenter);
            label.rectTransform.anchorMin = new Vector2(0f, 0f);
            label.rectTransform.anchorMax = new Vector2(1f, 0f);
            label.rectTransform.pivot = new Vector2(0.5f, 0f);
            label.rectTransform.anchoredPosition = new Vector2(0f, 4f);
            label.rectTransform.sizeDelta = new Vector2(-8f, EntryLabelHeight);

            // Uzun ad düğmeyi taşırmasın: sığmayan yazı küçülsün, kırpılmasın.
            // Alt sınır 11: "Düşman Fabrikası" iki satıra sarıldığında bu boyda
            // hâlâ okunuyor, altına inince okunmuyor.
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 11;
            label.resizeTextMaxSize = 16;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;

            // ÇİZİM SIRASI AÇIKÇA YAZILIYOR. Seçim çerçevesi en altta olmalı ki
            // simge onun üstüne düşsün; sıra ters olsaydı seçili düğmenin simgesi
            // sarı dikdörtgenin altında kaybolurdu. Eskiden bu sıra "önce hangi
            // çocuk yaratıldıysa" kazasına bırakılmıştı — prefab yeniden
            // yazıldığında o kaza artık geçerli değil.
            frame.transform.SetSiblingIndex(0);
            icon.transform.SetSiblingIndex(1);
            label.transform.SetSiblingIndex(2);

            var view = Need<PaletteEntryView>(root);
            var so = new SerializedObject(view);
            so.FindProperty("label").objectReferenceValue = label;
            so.FindProperty("icon").objectReferenceValue = icon;
            so.FindProperty("selectionFrame").objectReferenceValue = frame;
            so.ApplyModifiedPropertiesWithoutUndo();

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, PaletteEntryPath);

            if (existed)
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            else
            {
                Object.DestroyImmediate(root);
            }

            return saved.GetComponent<PaletteEntryView>();
        }

        // ─────────────────────────── SAHNE ───────────────────────────

        // EventSystem olmadan hiçbir UI düğmesi tıklama ALMAZ ve hata da vermez,
        // sadece sessizce çalışmaz. Bu yüzden ilk kurulan o.
        private static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() == null)
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
            Canvas existing = Object.FindAnyObjectByType<Canvas>();
            GameObject go = existing != null
                ? existing.gameObject
                : new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        private static ProductionDirector EnsureDirector(BoardAdapter board)
        {
            ProductionDirector existing = Object.FindAnyObjectByType<ProductionDirector>();
            if (existing == null)
            {
                existing = new GameObject("ProductionDirector").AddComponent<ProductionDirector>();
            }

            var so = new SerializedObject(existing);
            so.FindProperty("boardBehaviour").objectReferenceValue = board;
            so.ApplyModifiedPropertiesWithoutUndo();

            return existing;
        }

        /// <summary>
        /// Sol kenardaki yapı paletini kurar: üstte senin yapıların, altta
        /// düşmanınkiler.
        /// </summary>
        // PALET ARTIK İKİ SÜTUNLU. Taret ve hisar eklenince her tarafta beş
        // yapı oldu; tek sütunlu eski düzende bir taraf 622 piksel istiyordu ve
        // iki taraf 1080 piksellik ekrana sığmıyordu — alttaki düğmeler
        // ekranın dışına taşardı. İki sütun aynı beş düğmeyi 370 piksele
        // indiriyor ve düğme boyutunu küçültmek gerekmiyor.
        //
        // PANEL DURUM ŞERİDİNİN ALTINDAN BAŞLIYOR: eskiden üst kenardan 12
        // piksel başlıyordu ve 64 piksellik şeridin altına giriyordu, yani ilk
        // başlık yarı yarıya örtülüydü.
        private static void EnsurePalette(
            Canvas canvas, ProductionDirector director, PaletteEntryView entryPrefab)
        {
            GameObject go = Child(canvas.transform, "StructurePalette")
                            ?? NewUi("StructurePalette", canvas.transform, typeof(Image));

            var playerNames = new[]
            {
                "Structure_Karargah", "Structure_Kisla", "Structure_Fabrika",
                "Structure_Taret", "Structure_Hisar",
            };

            var enemyNames = new[]
            {
                "Structure_DusmanKarargahi", "Structure_DusmanKislasi",
                "Structure_DusmanFabrikasi", "Structure_DusmanTareti",
                "Structure_DusmanHisari",
            };

            // Panel genişliği sütun sayısından TÜRETİLİYOR; düğme büyüdüğünde
            // panel onunla birlikte büyür, elle bir sayı düzeltmek gerekmez.
            // Hesabın kendisi PaletteWidth'te, çünkü kamera da aynı sayıyı
            // okuyor: panelin altında kalmayan alanı ancak panelin genişliğini
            // bilerek hesaplayabiliyor.
            float panelWidth = PaletteWidth();

            var rect = Need<RectTransform>(go);
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(12f, -StatusBarHeight * 0.5f);
            rect.sizeDelta = new Vector2(panelWidth, -(24f + StatusBarHeight));
            Need<Image>(go).color = new Color(0.09f, 0.10f, 0.12f, 0.88f);

            Header(rect, "PlayerHeader", "SENİN", new Vector2(0f, 1f), -6f, new Color(0.45f, 0.75f, 1f));
            RectTransform playerRow = Row(rect, "PlayerRow", new Vector2(0f, 1f), -34f, playerNames.Length);

            // Düşman başlığı, düşman ızgarasının TAM ÜSTÜNE oturuyor: 8 piksel
            // alt pay + ızgara boyu + 6 piksel aralık + 24 piksel başlık.
            // Eskiden buraya sabit 300 yazılıydı ve başlık ızgaranın içine
            // düşüyordu.
            float enemyHeaderY = 8f + GridHeight(enemyNames.Length) + 30f;
            Header(rect, "EnemyHeader", "DÜŞMAN", new Vector2(0f, 0f), enemyHeaderY, new Color(1f, 0.5f, 0.45f));
            RectTransform enemyRow = Row(rect, "EnemyRow", new Vector2(0f, 0f), 8f, enemyNames.Length);

            StructurePaletteView view = go.GetComponent<StructurePaletteView>()
                                        ?? go.AddComponent<StructurePaletteView>();

            var so = new SerializedObject(view);
            so.FindProperty("director").objectReferenceValue = director;
            so.FindProperty("entryPrefab").objectReferenceValue = entryPrefab;
            so.FindProperty("playerRow").objectReferenceValue = playerRow;
            so.FindProperty("enemyRow").objectReferenceValue = enemyRow;
            Blueprints(so, "playerStructures", playerNames);
            Blueprints(so, "enemyStructures", enemyNames);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureProductionPanel(
            Canvas canvas, ProductionDirector director, PaletteEntryView entryPrefab)
        {
            GameObject go = Child(canvas.transform, "ProductionPanel")
                            ?? NewUi("ProductionPanel", canvas.transform, typeof(Image));

            // Alt orta. Palet artık iki sütunlu ve 240 piksel geniş ama ekranın
            // yalnızca sol şeridini tutuyor; panel ortada durabildiği için sağa
            // kaydırma payı kaldırıldı — panel tahtanın altında, ortalanmış.
            //
            // ÖLÇÜLER DÜĞMEDEN TÜRETİLİYOR: başlık, alt pay ve bir düğme boyu.
            // Panel yüksekliği elle yazılsaydı düğme büyüdüğünde düğmenin altı
            // kırpılırdı. İKİ SAYI ARTIK SINIF SABİTİ (PanelHeader, PanelPadding)
            // çünkü kamera da onları okuyor.

            var rect = Need<RectTransform>(go);
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 14f);
            rect.sizeDelta = new Vector2(
                5f * EntryWidth + 4f * EntrySpacing + 2f * PanelPadding,
                ProductionHeight());
            Need<Image>(go).color = new Color(0.09f, 0.10f, 0.12f, 0.92f);

            Header(rect, "PanelHeader", "ÜRETİLECEK BİRİM", new Vector2(0f, 1f), -6f, Color.white);

            GameObject rowGo = Child(rect, "Row")
                               ?? NewUi("Row", rect, typeof(HorizontalLayoutGroup));
            var row = Need<RectTransform>(rowGo);
            row.anchorMin = new Vector2(0f, 0f);
            row.anchorMax = new Vector2(1f, 1f);
            row.offsetMin = new Vector2(PanelPadding, PanelPadding);
            row.offsetMax = new Vector2(-PanelPadding, -PanelHeader);

            var layout = Need<HorizontalLayoutGroup>(rowGo);
            layout.spacing = EntrySpacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            GameObject emptyGo = Child(rect, "EmptyLabel") ?? NewUi("EmptyLabel", rect, typeof(Text));
            var emptyLabel = Need<Text>(emptyGo);
            StyleText(emptyLabel, "Üretim yapan bir yapı seç", 20, TextAnchor.MiddleCenter);
            emptyLabel.color = new Color(0.72f, 0.74f, 0.78f, 1f);
            var emptyRect = Need<RectTransform>(emptyGo);
            emptyRect.anchorMin = new Vector2(0f, 0f);
            emptyRect.anchorMax = new Vector2(1f, 1f);
            emptyRect.offsetMin = new Vector2(PanelPadding, PanelPadding);
            emptyRect.offsetMax = new Vector2(-PanelPadding, -PanelHeader);

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

        /// <summary>
        /// Tahtayı ekrana yerleştirir: ada bütünüyle görünür, oynanabilir kısım
        /// da arayüz panellerinin ALTINA GİRMEZ.
        /// </summary>
        /// <returns>Çerçevelenen kamera; sahnede yoksa null.</returns>
        // ESKİ FORMÜL PANELLERİ HİÇ BİLMİYORDU: yalnız tahtayı ve halkayı
        // ölçüyor, sonra tahtayı ekranın TAM ORTASINA koyuyordu. Oysa ekranın
        // ortası boş değil — solda 252 piksel palet, altta 186 piksel üretim
        // paneli, üstte 64 piksel durum şeridi var.
        //
        // ÖLÇÜLDÜ (10x5 tahta, 16:9, halka 2 hücre, eski formülle 4,90 birim):
        //   • oynanabilir tahta panelin altında DEĞİLDİ — paletin iç kenarı
        //     tahtanın 1,42 birim solunda, üretim panelinin üst kenarı 0,71
        //     birim altında kalıyordu. Yani tahtanın kurtulmuş olması bir
        //     güvence değil, sayıların rastlantısıydı;
        //   • KENAR HALKASI İSE GERÇEKTEN ÖRTÜLÜYDU: iki hücrelik halkanın alt
        //     kuşağının yüzde 64'ü üretim panelinin, sol kuşağının yüzde 29'u
        //     paletin, üst kuşağının yüzde 9'u durum şeridinin altındaydı.
        //     Halka 0. katmanın en içteki kuşağı; örtüldüğü sürece tahta ada
        //     gibi okunmuyor.
        //
        // KAMERA ARTIK İKİ İSTEĞİN BÜYÜĞÜNÜ ALIYOR:
        //   1) oynanabilir tahta + bir hücre pay, panellerin BOŞ bıraktığı
        //      dikdörtgene sığsın — rastlantı değil, kural;
        //   2) ada (tahta + halka) ARTI 0. katmanın üç kuşağı, tüm görüntüye
        //      sığsın — pay IslandMargin'de kuşaklardan türetiliyor.
        // Yalnız birincisi olsaydı adanın kenarı ekran dışında kalırdı; yalnız
        // ikincisi olsaydı halkanın örtülmesi sürerdi.
        //
        // BEDELİ ÖLÇÜLDÜ: 4,90 birim yerine 5,58 birim, yani tahta ekran
        // genişliğinin yüzde 57'si yerine yüzde 50'si. 16:9, 16:10, 4:3 ve 21:9
        // oranlarının dördünde de üç şey birden doğru kalıyor: tahta panelsiz
        // alanın içinde, ada bütünüyle görünür ve adanın DÖRT YANINDA da deniz
        // görünüyor (16:9'da en darı üstte 0,35 birim).
        private static Camera FrameCamera(BoardAdapter board)
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                Debug.LogWarning("[SceneSetupTool] MainCamera etiketli kamera yok; çerçeveleme atlandı.");
                return null;
            }

            var so = new SerializedObject(board);
            float width = so.FindProperty("width").intValue;
            float height = so.FindProperty("height").intValue;

            camera.orthographic = true;
            float aspect = camera.aspect > 0.01f ? camera.aspect : 16f / 9f;

            // Panellerin yediği ekran payı ORAN olarak. Piksel değil oran,
            // çünkü kamera dünyayı birimle ölçüyor ve iki ölçü ancak burada
            // buluşabiliyor. Sayılar CanvasScaler'ın referans çözünürlüğünde
            // geçerli; match 0,5 ile başka en boy oranlarında birkaç yüzde
            // kayıyorlar ve PlayMargin o kaymayı zaten yutuyor.
            float leftInset = (PaletteMargin + PaletteWidth()) / ReferenceWidth;
            float topInset = StatusBarHeight / ReferenceHeight;
            float bottomInset = (ProductionMargin + ProductionHeight()) / ReferenceHeight;

            // BOŞ ALANIN MERKEZİ EKRANIN MERKEZİ DEĞİL: solda ve altta panel
            // var, sağda ve üstte neredeyse yok. Tahtanın boş alanın ortasına
            // oturması için kameranın ters yöne kayması gerekiyor.
            float shiftX = leftInset * 0.5f;
            float shiftY = (bottomInset - topInset) * 0.5f;

            // İSTEK 1 — oynanabilir tahta panelsiz dikdörtgene sığsın.
            float freeWidth = 1f - leftInset;
            float freeHeight = 1f - topInset - bottomInset;
            float playSize = Mathf.Max(
                (height + 2f * PlayMargin) / (2f * freeHeight),
                (width + 2f * PlayMargin) / (2f * aspect * freeWidth));

            // İSTEK 2 — ada tümüyle görünsün. KAYMA PAYDAYA GİRİYOR: kamera
            // kaydığı yönde o kadar az görüyor, yani kaymayı hesaba katmayan bir
            // "sığar" hesabı adanın üst sırasını ekranın dışında bırakırdı.
            // Mutlak değer, kayma ters yöne döndüğünde de dar kenarı bulsun diye.
            float islandHalfHeight = height * 0.5f + BorderThickness + IslandMargin;
            float islandHalfWidth = width * 0.5f + BorderThickness + IslandMargin;
            float islandSize = Mathf.Max(
                islandHalfHeight / (1f - 2f * Mathf.Abs(shiftY)),
                islandHalfWidth / (aspect * (1f - 2f * Mathf.Abs(shiftX))));

            camera.orthographicSize = Mathf.Max(playSize, islandSize);

            float viewHeight = 2f * camera.orthographicSize;
            camera.transform.position = new Vector3(
                width * 0.5f - shiftX * viewHeight * aspect,
                height * 0.5f - shiftY * viewHeight,
                -10f);

            // ZEMİN RENGİ ARTIK GÖK MAVİSİ. Fabrika ayarı olan lacivert grisi
            // (0.192, 0.302, 0.475) ekranda "kurulmamış proje" gibi duruyordu;
            // yerine konan gece yeşili de kapalı bir renkti. ÖLÇÜLMÜŞ REFERANS
            // (Fatihcill/CountryBall-Strategy) kamerasını açık gök mavisiyle
            // temizliyor ve operatörün "daha hoş" dediği şey buydu.
            //
            // ALFA 1 YAZILIYOR: sahnedeki renk a: 0 ile duruyordu. Opak bir
            // kamera zemininde alfanın söyleyecek sözü yok ama saydam bir renk
            // Inspector'a bakan birine "burada bir şey eksik" diye okunur.
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = SkyColor;
            return camera;
        }

        /// <summary>
        /// Oynanabilir ızgaranın dışında kalan dünyayı kurar: kumsal, sığlık ve
        /// açık deniz.
        /// </summary>
        // OYUNDA NE İŞE YARIYOR: tahtanın bittiği yerde ekran artık boş bir renk
        // değil, bakılacak bir yer. Tahta bir adanın tepesi gibi okunuyor ve
        // dıştan içe sıra şu: deniz, sığlık, kumsal, toprak halka, çim.
        //
        // ÜÇ SEÇENEK ÖLÇÜLDÜ, ÜÇÜNCÜSÜ SEÇİLDİ:
        //   (a) yalnız kameranın arka plan rengini değiştirmek — tek satır, ama
        //       ekranda hâlâ tek düz renk kalıyor, yani "0. katmanda görünecek
        //       bir görüntü" isteği karşılanmıyor;
        //   (b) elimizdeki toprak/çim karolarından ikinci bir geniş tabaka
        //       sermek — ÖLÇÜM BU SEÇENEĞİ ÇÜRÜTTÜ: dirt_fill_plain ile
        //       grass_plain'in 256 pikselinin 256'sı birebir aynı renk, yani o
        //       karodan serilen tabaka ile (a) EKRANDA AYNI ŞEY; dağınık
        //       varyantların 256 pikselinin de ancak 19-21 tanesi desen taşıyor ve toprağın
        //       mavi kanalı 108'de tavan yaptığı için çarpım filtresiyle deniz
        //       mavisi hiç çıkmıyor;
        //   (c) BOYANABİLİR bir karo üretip dıştan içe daralan kuşaklar sermek —
        //       seçilen. Tek bir beyaz karo üç ayrı renge boyanıyor, dokusunu
        //       koruyor ve kuşakların kenarı adaya siluet veriyor. Düz bir renk
        //       ile arasındaki fark da tam olarak o siluet.
        //
        // OYUNUN KURALINA HİÇ GİRMİYOR: burada kurulan hiçbir nesnenin
        // çarpıştırıcısı yok, hiçbiri BoardAdapter'ın çocuğu değil ve tahtanın
        // "bu hücre içeride mi, boş mu" sorusuna dokunan tek satır yok. Fare de
        // etkilenmiyor: tahta tıklamayı fiziksel ışınla değil, ekran noktasını
        // hücreye çevirerek okuyor.
        private static void EnsureWorldBackdrop(BoardAdapter board, Camera camera)
        {
            ClearWorldBackdrop();

            // Sprite() DEĞİL, doğrudan yükleme: karonun yokluğu bir hata değil
            // bir yokluk. Araç 0. katmanı atlayıp geri kalan kurulumu bitirebilmeli.
            var tile = AssetDatabase.LoadAssetAtPath<Sprite>(WaterTile);
            if (tile == null)
            {
                Debug.LogWarning(
                    $"[SceneSetupTool] {WaterTile} bulunamadı; 0. katman kurulmadı.");
                return;
            }

            var so = new SerializedObject(board);
            float width = so.FindProperty("width").intValue;
            float height = so.FindProperty("height").intValue;

            var root = new GameObject(BackdropRootName);
            var boardCentre = new Vector2(width * 0.5f, height * 0.5f);

            // Adanın dış ölçüsü: oynanabilir tahta ARTI kenar halkası. Halkanın
            // kalınlığı burada da okunuyor çünkü kuşaklar halkanın dışından
            // başlamak zorunda; içeriden başlasalardı toprağın üstüne binerlerdi.
            float islandWidth = width + 2f * BorderThickness;
            float islandHeight = height + 2f * BorderThickness;

            // DENİZ KAMERAYA GÖRE ORTALANIYOR, TAHTAYA GÖRE DEĞİL. Kamera
            // panelleri boşaltmak için sola ve aşağı kaymış durumda; deniz
            // tahtaya göre ortalansaydı kameranın kaydığı yönde ekranın kenarı
            // açıkta kalırdı.
            Vector2 seaCentre = camera != null
                ? new Vector2(camera.transform.position.x, camera.transform.position.y)
                : boardCentre;

            float seaHeight = camera != null
                ? 2f * camera.orthographicSize * SeaOversize
                : islandHeight * SeaOversize * 2f;
            float seaAspect = camera != null && camera.aspect > 0.01f ? camera.aspect : 16f / 9f;

            Plate(root.transform, "OpenSea", tile, SeaColor,
                seaCentre, new Vector2(seaHeight * seaAspect, seaHeight), SeaOrder);

            Plate(root.transform, "Shoal", tile, ShoalColor, boardCentre,
                new Vector2(islandWidth + 2f * ShoalWidth, islandHeight + 2f * ShoalWidth),
                ShoalOrder);

            Plate(root.transform, "Beach", tile, BeachColor, boardCentre,
                new Vector2(islandWidth + 2f * BeachWidth, islandHeight + 2f * BeachWidth),
                BeachOrder);
        }

        /// <summary>
        /// 0. katmanın tek bir kuşağını serer.
        /// </summary>
        // KARO GERİLMİYOR, DÖŞENİYOR. Ölçek verilseydi 16 pikselik desen ekran
        // boyunda tek bir lekeye dönerdi; SpriteDrawMode.Tiled aynı karoyu yan
        // yana tekrarlıyor ve deseni hücre ölçüsünde tutuyor — yani 0. katman
        // ile tahta AYNI ritmi paylaşıyor.
        private static void Plate(
            Transform parent, string name, Sprite tile, Color color,
            Vector2 centre, Vector2 size, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, worldPositionStays: false);
            go.transform.position = new Vector3(centre.x, centre.y, 0f);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = tile;
            renderer.color = color;

            // SIRA ÖNEMLİ: size yalnız döşeme kipinde yazılabiliyor, kip Simple
            // kaldığı sürece sessizce yok sayılıyor.
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.tileMode = SpriteTileMode.Continuous;
            renderer.size = size;
            renderer.sortingOrder = order;
        }

        /// <summary>
        /// Önceki kurulumdan kalan 0. katmanı sahneden kaldırır.
        /// </summary>
        // ADIYLA TOPLANIYOR ve bu, aracın "her çalıştırmada yeniden yaz" kuralının
        // gereği: kuşakların boyu kameraya bağlı, yani tahta büyüdüğünde eskisi
        // artık doğru değil. Toplanmasaydı sahne her kurulumda bir deniz daha
        // kazanır ve üst üste binen kuşaklar sessizce koyulaşırdı.
        private static void ClearWorldBackdrop()
        {
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] != null && roots[i].name == BackdropRootName)
                {
                    Object.DestroyImmediate(roots[i]);
                }
            }
        }

        /// <summary>
        /// Yerleştirme hayaletini, yapının gerçekten görüneceği hâle yaklaştırır.
        /// </summary>
        // ÜÇ ÖLÇÜLMÜŞ KUSUR: hayalet sahnede ölçek 1 ile, TAM OPAK ve GÖRSELSİZ
        // duruyordu. Oyuncu ilk ikisini şöyle yaşıyordu: bir hücre boyunda bir
        // bina görüp bırakıyor, yere her yöne taşan bir bina düşüyordu; opak
        // hayalet ise altındaki hücreyi kapattığı için "buraya konur mu"
        // sorusunun cevabını gizliyordu.
        //
        // ÜÇÜNCÜSÜ SESSİZ VE KALICIYDI: bu araç hayaletin sprite'ına HİÇ
        // dokunmuyordu, yani sahnedeki görsel bir kez silindiğinde geri
        // getirmenin tek yolu elle sürüklemekti. Araç "bağlantıları her
        // çalıştırmada yeniden yazar" diyorsa bir bağlantıyı atlaması bir
        // istisna değil, bir borçtur.
        //
        // ÖLÇEK ARTIK TÜRETİLİYOR: eskiden BoardAdapter'daki structureScale
        // okunuyordu ve o alan gitti. Yerine geçen şey bir sayı değil bir HESAP
        // — hayalet, yapının kaplayacağı hücre sayısını görselin kendi
        // ölçüsünden ölçeğe çeviriyor, yani 32x32 bir bina görseli geldiği gün
        // önizleme sessizce iki katına çıkmıyor.
        private static void EnsurePlacementGhost(BoardAdapter board)
        {
            var so = new SerializedObject(board);
            SerializedProperty ghostProperty = Optional(so, "placementGhost");
            var ghost = ghostProperty != null
                ? ghostProperty.objectReferenceValue as SpriteRenderer
                : null;

            if (ghost == null)
            {
                Debug.LogWarning(
                    "[SceneSetupTool] placementGhost bağlı değil; hayalet biçimlendirilmedi.");
                return;
            }

            // KOŞULSUZ YAZILIYOR, "boşsa doldur" DEĞİL: eksik bağlantıyı yalnız
            // ilk kurulumda onaran bir araç, silinen bağlantıyı hiç onarmaz ve
            // kusur tam da bu yüzden kalıcıydı. Kışlanın görseli seçildi çünkü
            // hayalet paletten bir simge gelmediğinde konan yapının GERÇEK
            // görseli oluyor, ve klavyeyle konan yapının en makul karşılığı
            // tahtanın en ucuz üretim binası.
            ghost.sprite = Sprite(FriendlyBarracks);

            // HANGİ YAPININ SEÇİLDİĞİ HENÜZ BİLİNMİYOR, o yüzden merdivenin
            // TAVANI yazılıyor: önizlemenin gerçek yapıdan bir tık büyük olması,
            // küçük kalmasından daha az yanıltıcı — hiçbir yapı hayaletten
            // taşmıyor.
            ghost.transform.localScale = BoardSizing.LocalScaleFor(
                ghost.sprite, StructureSizeCeiling, CellSizeOf(board));

            ghost.color = new Color(1f, 1f, 1f, 0.55f);

            // ÇİZİM SIRASI MERDİVENİN EN ÜSTÜ: halka -1, zemin 0, yapı 1,
            // birim 2, imleç çerçevesi 3, can barı 4 (dolgusu 5), hayalet 6.
            // Sayı BoardAdapter'ın Awake'te yazdığıyla aynı olmak zorunda; ayrı
            // olsaydı sahnede bir şey, oyunda başka bir şey görünürdü.
            ghost.sortingOrder = 6;
            EditorUtility.SetDirty(ghost);
        }

        /// <summary>
        /// Tahtanın hücre ölçüsü; Grid bileşeni yoksa bir birimlik hücre.
        /// </summary>
        // HÜCRE ÖLÇÜSÜ ARACIN SAYISI DEĞİL, TAHTANIN SAYISI. Burada 1 yazılsaydı
        // hücresi büyütülmüş bir tahtada hayalet doğru büyüklükte kalır ama
        // yapılar onunla birlikte büyümezdi — önizleme ile sonuç ayrışırdı.
        private static Vector3 CellSizeOf(BoardAdapter board)
        {
            // TAM NİTELENMİŞ AD: bu dosya hem UnityEngine hem UnityEditor
            // kullanıyor ve kısa ad iki ad alanı arasında gezinmeye açık.
            var grid = board.GetComponent<UnityEngine.Grid>();
            return grid != null ? grid.cellSize : Vector3.one;
        }

        // ─────────────────────────── YARDIMCILAR ───────────────────────────

        /// <summary>
        /// Sol paletin genişliği ve üretim panelinin yüksekliği.
        /// </summary>
        // İKİ OKUYUCU: paneli kuran üye ile kamerayı çerçeveleyen üye. Sayılar
        // orada da burada da yazılsaydı düğme büyüdüğü gün panel büyür, kamera
        // ise eski payı kullanmaya devam eder ve tahtanın kenarı sessizce
        // panelin altına girerdi.
        private static float PaletteWidth()
        {
            return PaletteColumns * EntryWidth
                   + (PaletteColumns - 1) * EntrySpacing
                   + 2f * EntrySpacing;
        }

        private static float ProductionHeight()
        {
            return EntryHeight + PanelHeader + 2f * PanelPadding;
        }

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

        /// <summary>
        /// Bir tarafın yapı düğmelerini taşıyan iki sütunlu ızgarayı kurar.
        /// </summary>
        // IZGARA, DİKEY ŞERİDİN YERİNİ ALDI. VerticalLayoutGroup her düğmeyi alt
        // alta diziyordu ve beşinci yapı ekranın dışına taşıyordu.
        // GridLayoutGroup hücre boyutunu KENDİSİ dayatır — düğmedeki
        // LayoutElement'e bakmaz — yani düğme ile ızgara aynı sabitlerden
        // beslenmezse ikisi ayrışır. Bu yüzden ikisi de EntryWidth/EntryHeight
        // okuyor.
        private static RectTransform Row(
            RectTransform parent, string name, Vector2 anchor, float offsetY, int entryCount)
        {
            GameObject go = Child(parent, name) ?? NewUi(name, parent, typeof(GridLayoutGroup));

            // ESKİ DÜZEN BİLEŞENİ SİLİNİYOR. Bu nesne önceki bir kurulumda
            // yaratıldıysa üstünde hâlâ VerticalLayoutGroup vardır; iki düzen
            // bileşeni aynı çocuklar üstünde kavga eder ve düğmeler birbirinin
            // üstüne biner. Araç geliştikçe sahne onunla birlikte güncellensin
            // isteniyorsa, eklemek kadar KALDIRMAK da bu aracın işidir.
            DropComponent<VerticalLayoutGroup>(go);
            DropComponent<HorizontalLayoutGroup>(go);

            var rect = Need<RectTransform>(go);
            rect.anchorMin = new Vector2(0f, anchor.y);
            rect.anchorMax = new Vector2(1f, anchor.y);
            rect.pivot = new Vector2(0.5f, anchor.y);
            rect.anchoredPosition = new Vector2(0f, offsetY);
            rect.sizeDelta = new Vector2(-2f * EntrySpacing, GridHeight(entryCount));

            var layout = Need<GridLayoutGroup>(go);
            layout.cellSize = new Vector2(EntryWidth, EntryHeight);
            layout.spacing = new Vector2(EntrySpacing, EntrySpacing);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = PaletteColumns;
            layout.startAxis = GridLayoutGroup.Axis.Horizontal;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.padding = new RectOffset(0, 0, 0, 0);

            return rect;
        }

        // Kaç düğme varsa ızgaranın kaç piksel olacağını söyler. Palet paneli,
        // düşman başlığının yeri ve ızgaranın kendi boyu AYNI hesaptan çıkıyor.
        private static float GridHeight(int entryCount)
        {
            int lines = Mathf.CeilToInt(entryCount / (float)PaletteColumns);
            return lines * EntryHeight + Mathf.Max(0, lines - 1) * EntrySpacing;
        }

        /// <summary>
        /// Nesnede varsa o bileşeni siler.
        /// </summary>
        private static void DropComponent<T>(GameObject go) where T : Component
        {
            T stale = go.GetComponent<T>();
            if (stale != null)
            {
                Object.DestroyImmediate(stale);
            }
        }

        /// <summary>
        /// Alanı verir; yoksa null döner ve konsola tek satırlık bir uyarı
        /// bırakır.
        /// </summary>
        // BU ARAÇ PARALEL YAZILAN ALANLARA DOKUNUYOR. BoardAdapter'daki
        // borderSprites, turnMode gibi alanlar başka bir dosyanın işi; o dosya
        // henüz yerine oturmamışken FindProperty null döner ve düz kullanım
        // NullReferenceException ile TÜM kurulumu düşürür — sahne yarı kurulu
        // kalır. Eksik bir alanın cezası, kurulan onca şeyin çöpe gitmesi
        // olmamalı: burada yalnızca o alan atlanır ve neyin eksik olduğu yazılır.
        private static SerializedProperty Optional(SerializedObject so, string field)
        {
            SerializedProperty property = so.FindProperty(field);
            if (property == null)
            {
                Debug.LogWarning(
                    $"[SceneSetupTool] '{field}' alanı {so.targetObject.GetType().Name} üstünde yok; atlandı.");
            }

            return property;
        }

        // BULUR YA DA YARATIR. Bu ikisi eskiden koşulsuz YENİ çocuk yaratıyordu
        // ve o yüzden yalnızca bir kez, boş bir prefab kurulurken çağrılabilirdi.
        // Düğme düzeni artık her kurulumda yeniden yazıldığı için ikinci çağrı
        // aynı çocuğu bulmak zorunda; yaratsaydı prefab her çalıştırmada bir
        // "Icon" daha kazanırdı.
        private static Image ImageChild(RectTransform parent, string name, Color color)
        {
            GameObject go = Child(parent, name) ?? NewUi(name, parent, typeof(Image));

            var image = Need<Image>(go);
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Text TextChild(
            RectTransform parent, string name, string content, int size, TextAnchor anchor)
        {
            GameObject go = Child(parent, name) ?? NewUi(name, parent, typeof(Text));

            var text = Need<Text>(go);
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
