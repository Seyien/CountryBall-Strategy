using System.Collections.Generic;
using GridStrategy.Combat;
using UnityEngine;

namespace GridStrategy.Unity
{
    // ═══ ROL: GÖRÜNÜM (View) ═════════════════════════════════════════
    // kimlik : var ama SAHNE kimliği — sol panelin kendisi
    // hafıza : var — ölçüsü şu: aynı OnEntryClicked çağrısı önce bir düğmeyi
    //          seçili yapar, ikinci kez çağrıldığında ÖNCEKİNİ söndürür;
    //          farkı doğuran şey selectedIndex alanının çağrılar arasında
    //          tuttuğu sıra. Bu bir OYUN durumu değil, PANEL durumudur
    // Unity  : zorunlu — düğmeleri Instantiate ediyor
    // karar  : vermez, YÖNLENDİRİR — hangi tanımın nereye konacağına
    //          ProductionDirector karar verir
    /// <summary>
    /// Sol panel: üstte oyuncunun yapı türleri, altta düşmanınkiler.
    ///
    /// İKİ SIRA, İKİ TAKIM — ve ayrım bir süzgeç değil, İKİ AYRI LİSTEdir.
    /// Tek bir listede takım alanı taşıyan bir tanım tipi alternatifi
    /// reddedildi: aynı baraka tanımı iki tarafta da kullanılabilmeli ve
    /// tanıma taraf yazmak, düşmanın barakasını ikinci bir varlık dosyasına
    /// zorlardı.
    ///
    /// Neyi BİLMEZ: bir hücrenin boş olup olmadığını, üretimin hazır olup
    /// olmadığını, tahtanın nerede durduğunu.
    ///
    /// AYNA BELGE: bu tipin gerekçeleri bugün yalnızca bu dosyada.
    /// </summary>
    public sealed class StructurePaletteView : MonoBehaviour
    {
        [Header("Wiring")]
        [Tooltip("The ProductionDirector in the scene. Left empty, the palette draws but does nothing.")]
        [SerializeField] private ProductionDirector director;

        [Tooltip("Prefab carrying a PaletteEntryView. Left empty, the palette stays empty.")]
        [SerializeField] private PaletteEntryView entryPrefab;

        [Header("Rows - the player row is meant to sit above the enemy row")]
        [Tooltip("Parent of the player entries. Usually a child with a HorizontalLayoutGroup.")]
        [SerializeField] private RectTransform playerRow;

        [Tooltip("Parent of the enemy entries. Usually a child with a HorizontalLayoutGroup.")]
        [SerializeField] private RectTransform enemyRow;

        [Header("Structure types")]
        [Tooltip("Structure blueprints offered to the player, drawn in the top row.")]
        [SerializeField] private StructureBlueprintAsset[] playerStructures = new StructureBlueprintAsset[0];

        [Tooltip("Structure blueprints offered to the enemy, drawn in the bottom row.")]
        [SerializeField] private StructureBlueprintAsset[] enemyStructures = new StructureBlueprintAsset[0];

        // İKİ DİZİ TEK LİSTEYE DÜZLENİYOR ve sıra bir KARARDIR: önce oyuncunun
        // yapıları, sonra düşmanınkiler. Düğme kendi sırasını taşıyor, hangi
        // sırada olduğunu değil — takım sorusu aşağıda tek bir karşılaştırmayla
        // cevaplanıyor ve ikinci bir takım dizisi hiç doğmuyor.
        private readonly List<StructureBlueprintAsset> flattened =
            new List<StructureBlueprintAsset>();

        private readonly List<PaletteEntryView> entries = new List<PaletteEntryView>();

        private int playerCount;
        private int selectedIndex = -1;

        private void Awake()
        {
            if (entryPrefab == null)
            {
                Debug.LogError(
                    "[StructurePaletteView] entryPrefab is not assigned. The palette will be empty.",
                    this);
                return;
            }

            if (director == null)
            {
                // Panel yine de ÇİZİLİYOR: eksik bir referans yüzünden ekranı
                // boş bırakmak, hatayı "hiçbir şey görünmüyor" diye teşhis
                // edilemez bir hâle sokardı. Düğmeler çizilir, tıklanır, ve
                // hiçbir şey olmaz — ama konsolda sebebi yazar.
                Debug.LogError(
                    "[StructurePaletteView] director is not assigned. Entries will draw but do nothing.",
                    this);
            }

            Build(playerStructures, playerRow, Team.Player);
            playerCount = flattened.Count;
            Build(enemyStructures, enemyRow, Team.Enemy);
        }

        /// <summary>
        /// Bir takımın yapı türlerini kendi sırasına çizer.
        /// </summary>
        private void Build(StructureBlueprintAsset[] source, RectTransform row, Team team)
        {
            if (row == null)
            {
                Debug.LogError(
                    $"[StructurePaletteView] The row for {team} is not assigned. " +
                    $"Its {source.Length} structure type(s) will not be drawn.",
                    this);
                return;
            }

            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] == null)
                {
                    // Boş göz SESSİZ atlanmıyor: sol panelde eksik bir yapı türü,
                    // oyuncunun hiç göremeyeceği bir eksikliktir — o eksikliği
                    // tasarımcı görmeli.
                    Debug.LogError(
                        $"[StructurePaletteView] Empty slot in the {team} list at index {i}. " +
                        "The slot is skipped; assign a StructureBlueprintAsset or shrink the array.",
                        this);
                    continue;
                }

                PaletteEntryView entry = Instantiate(entryPrefab, row);
                entry.Bind(flattened.Count, source[i].DisplayName, source[i].Icon);

                entry.Clicked += OnEntryClicked;
                entry.DragBegan += OnEntryDragBegan;
                entry.Dragged += OnEntryDragged;
                entry.DragEnded += OnEntryDragEnded;

                flattened.Add(source[i]);
                entries.Add(entry);
            }
        }

        // TIKLAMA YALNIZCA SEÇER, YERLEŞTİRMEZ — ve bu bir eksiklik değil, bir
        // sınır. Tıklayarak yerleştirmek, tahtaya yapılan BİR SONRAKİ tıklamanın
        // anlamını değiştirmeyi gerektirir; o tıklamanın sahibi tahtanın kendi
        // bileşeni ve o dosya bu hattın malı değil. Sürükle-bırak yolu ise
        // tamamen bu dosyaların içinde kalıyor.
        // TETİKLEYİCİ: tahta bileşeni "yerleştirme kipindeyim" sorusunu dışarıya
        // açtığı gün.
        private void OnEntryClicked(PaletteEntryView entry)
        {
            Select(entry.Index);
        }

        private void OnEntryDragBegan(PaletteEntryView entry)
        {
            Select(entry.Index);

            if (director == null)
            {
                return;
            }

            // VARLIĞIN KENDİSİ GİDİYOR, tanımı ve simgesi ayrı ayrı değil: ikisi
            // de bu dosyada zaten aynı satırdan okunuyordu ve ayrı ayrı
            // geçirmek, karşı tarafa aynı varlığı yeniden birleştirme işi
            // bırakıyordu.
            director.BeginStructurePlacement(flattened[entry.Index], TeamOf(entry.Index));
        }

        private void OnEntryDragged(PaletteEntryView entry, Vector2 screenPoint)
        {
            if (director != null)
            {
                director.DragTo(screenPoint);
            }
        }

        private void OnEntryDragEnded(PaletteEntryView entry, Vector2 screenPoint)
        {
            if (director != null)
            {
                director.DropAt(screenPoint);
            }
        }

        /// <summary>
        /// Takım sorusunun tek cevabı: düzlenmiş listede oyuncunun yapıları
        /// ÖNCE geliyor.
        /// </summary>
        // İKİNCİ BİR TAKIM LİSTESİ REDDEDİLDİ: aynı bilgi zaten sıranın
        // KENDİSİNDE yazılı ve ikinci bir liste, bir gün biri eklenip öteki
        // eklenmediğinde sessizce ayrışırdı.
        private Team TeamOf(int index)
        {
            return index < playerCount ? Team.Player : Team.Enemy;
        }

        /// <summary>
        /// Seçim çerçevesini taşır. Panelin kendi durumudur, oyunun değil.
        /// </summary>
        private void Select(int index)
        {
            if (selectedIndex >= 0 && selectedIndex < entries.Count)
            {
                entries[selectedIndex].SetSelected(false);
            }

            selectedIndex = index;

            if (selectedIndex >= 0 && selectedIndex < entries.Count)
            {
                entries[selectedIndex].SetSelected(true);
            }
        }
    }
}
