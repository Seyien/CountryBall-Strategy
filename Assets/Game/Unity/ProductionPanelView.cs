using System.Collections.Generic;
using GridStrategy.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace GridStrategy.Unity
{
    // ═══ ROL: GÖRÜNÜM (View) ═════════════════════════════════════════
    // kimlik : var ama SAHNE kimliği — sağ panelin kendisi
    // hafıza : var — ölçüsü şu: seçim iki kez değişirse aynı Rebuild çağrısı
    //          farklı düğmeler üretir; farkı doğuran şey entries listesinin
    //          çağrılar arasında tuttuğu düğmeler. PANEL durumudur, oyunun
    //          değil
    // Unity  : zorunlu — düğmeleri Instantiate ve Destroy ediyor
    // karar  : vermez, YANSITIR — hangi yapının seçili olduğuna tahta,
    //          neyin üretilebileceğine ProductionRules karar verir
    /// <summary>
    /// Sağ panel: seçili yapının ürettiği birim türleri, biri VARSAYILAN
    /// olarak seçili.
    ///
    /// Varsayılanı bu panel SEÇMEZ, tanımdan OKUR — hangi birimin açılışta
    /// seçili duracağı bir tasarım kararıdır ve sahibi
    /// <see cref="StructureBlueprint"/>'tir. Panel bir varsayılan uydursaydı
    /// (örneğin hep ilki), tasarımcının varlık dosyasına yazdığı sayı sessizce
    /// ölürdü.
    ///
    /// SİMGE YOK VE YOKLUĞU ÖLÇÜLDÜ: seçili yapının üretim listesi düz C#
    /// tanımlarından oluşuyor, motor tarafındaki varlık dosyalarından değil —
    /// ve simge yalnızca varlık dosyasında yaşıyor. Tanımdan varlığa geri
    /// dönen bir dizin açmak, bugün hiçbir kuralın istemediği ikinci bir
    /// tablo demekti.
    /// TETİKLEYİCİ: sağ paneldeki simge oyun için gerçekten gerektiği gün.
    ///
    /// AYNA BELGE: bu tipin gerekçeleri bugün yalnızca bu dosyada.
    /// </summary>
    public sealed class ProductionPanelView : MonoBehaviour
    {
        [Header("Wiring")]
        [Tooltip("The ProductionDirector in the scene. Left empty, the panel never fills.")]
        [SerializeField] private ProductionDirector director;

        [Tooltip("Prefab carrying a PaletteEntryView. Left empty, the panel stays empty.")]
        [SerializeField] private PaletteEntryView entryPrefab;

        [Tooltip("Parent of the produced-unit entries. Usually a child with a VerticalLayoutGroup.")]
        [SerializeField] private RectTransform row;

        [Tooltip("Shown when nothing is selected or the selection produces nothing. May be left empty.")]
        [SerializeField] private Text emptyLabel;

        private readonly List<PaletteEntryView> entries = new List<PaletteEntryView>();

        // SEÇİLİ HATTIN İKİNCİ BİR KOPYASI TUTULMUYOR: tek sahibi
        // ProductionDirector ve panel ona indisle geri dönüyor. Bir alan
        // konsaydı "kalan süre" gibi bir gösterge yazmak kolaylaşırdı — ama o
        // göstergenin bugün bir isteyeni yok ve alan, olmayan bir özelliğin
        // sessiz davetiyesi olurdu.
        private int selectedIndex = -1;

        private void Awake()
        {
            if (entryPrefab == null)
            {
                Debug.LogError(
                    "[ProductionPanelView] entryPrefab is not assigned. The panel will stay empty.",
                    this);
            }

            if (director == null)
            {
                Debug.LogError(
                    "[ProductionPanelView] director is not assigned. The panel will never fill.",
                    this);
            }
        }

        // ABONELİK OnEnable'DA, Awake'te DEĞİL — ve sökülmesi OnDisable'da.
        // Awake'te abone olup hiç sökmemek, bu bileşen kapatıldığında yayıncıyı
        // hâlâ ona bağlı bırakırdı; aynı gerekçe tahtanın kendi abonelik çiftinde
        // de yazılı.
        private void OnEnable()
        {
            if (director == null)
            {
                Rebuild(null);
                return;
            }

            director.SelectedProductionChanged += Rebuild;

            // AÇILIŞTA BİR KEZ ELLE OKUNUYOR: olay yalnızca DEĞİŞİMDE yayınlanır
            // ve bu panel, seçim zaten yapılmışken açılmış olabilir. Bu satır
            // olmasaydı panel bir sonraki seçim değişikliğine kadar boş kalır ve
            // sebebi hiçbir yerde görünmezdi.
            Rebuild(director.SelectedProduction);
        }

        private void OnDisable()
        {
            if (director != null)
            {
                director.SelectedProductionChanged -= Rebuild;
            }
        }

        /// <summary>
        /// Paneli seçili yapının üretim listesine göre yeniden kurar.
        /// </summary>
        // YIKIP YENİDEN KURUYOR, HAVUZLAMIYOR — ve bu ölçülmüş bir karardır:
        // bu metot kare başına değil, oyuncu SEÇİM DEĞİŞTİRDİĞİNDE koşuyor.
        // Havuzun azaltacağı maliyet burada ölçülebilir değil, çünkü maliyet
        // yok. Aynı ölçü Docs/ogrenme/02-sonraki-asamalar.md dosyasının nesne
        // havuzu bölümünde tahta için de yapılmış durumda.
        // TETİKLEYİCİ: bu metot bir kullanıcı eylemi olmadan çağrılmaya
        // başladığı gün.
        private void Rebuild(StructureProduction next)
        {
            Clear();

            bool hasList = next != null && next.Blueprint.CanProduce;

            if (emptyLabel != null)
            {
                emptyLabel.enabled = !hasList;
            }

            if (!hasList || entryPrefab == null || row == null)
            {
                if (hasList && row == null)
                {
                    Debug.LogError(
                        "[ProductionPanelView] row is not assigned; the produced units cannot be drawn.",
                        this);
                }

                return;
            }

            IReadOnlyList<UnitBlueprint> produces = next.Blueprint.Produces;
            for (int i = 0; i < produces.Count; i++)
            {
                PaletteEntryView entry = Instantiate(entryPrefab, row);

                // Simge bilerek null: gerekçe bu tipin belgesinde yazılı.
                entry.Bind(i, produces[i].DisplayName, null);

                entry.Clicked += OnEntryClicked;
                entry.DragBegan += OnEntryDragBegan;
                entry.Dragged += OnEntryDragged;
                entry.DragEnded += OnEntryDragEnded;

                entries.Add(entry);
            }

            // VARSAYILAN TANIMDAN OKUNUYOR. Kelepçesi tanımın kurucusunda duruyor,
            // yani buraya aralık dışı bir sayı gelemez ve burada ikinci bir
            // kelepçe kurmak o kuralı ikinci bir eve koymak olurdu.
            Select(next.Blueprint.DefaultProducedIndex);
        }

        /// <summary>
        /// Bütün düğmeleri söker. Abonelikler ÖNCE kesiliyor.
        /// </summary>
        // SIRA BİR KARARDIR: abonelik kesilmeden Destroy çağrılsaydı, aynı karede
        // gelen bir sürükleme olayı yok edilmekte olan bir düğme üzerinden
        // yayınlanırdı. Destroy Unity'de karenin SONUNDA gerçekleşir, yani nesne
        // o ana kadar hâlâ olay yayabilir.
        private void Clear()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                PaletteEntryView entry = entries[i];

                entry.Clicked -= OnEntryClicked;
                entry.DragBegan -= OnEntryDragBegan;
                entry.Dragged -= OnEntryDragged;
                entry.DragEnded -= OnEntryDragEnded;

                Destroy(entry.gameObject);
            }

            entries.Clear();
            selectedIndex = -1;
        }

        private void OnEntryClicked(PaletteEntryView entry)
        {
            Select(entry.Index);
        }

        private void OnEntryDragBegan(PaletteEntryView entry)
        {
            Select(entry.Index);

            if (director != null)
            {
                director.BeginUnitPlacement(entry.Index);
            }
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
        /// Seçim çerçevesini taşır. Panelin kendi durumudur.
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
