using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GridStrategy.Unity
{
    // ═══ ROL: GÖRÜNÜM (View) ═════════════════════════════════════════
    // kimlik : var ama SAHNE kimliği — her kopya ayrı bir düğmedir; oyun
    //          kimliği yok ve bu tip hiçbir tanım tipini GÖRMEZ
    // hafıza : yok — "seçili miyim" burada saklanmaz; tek doğruluk kaynağı
    //          paneldir. TEK İSTİSNA ve adı konmuş: Index bir oyun durumu
    //          değil, panelin bu düğmeye verdiği ADRESTİR
    // Unity  : zorunlu ve GENİŞ — MonoBehaviour, ayrıca sürükleme
    //          olaylarını alabilmesi için sahnede bir EventSystem ve ışın
    //          hedefi olan bir Graphic gerekiyor
    // karar  : vermez, BİLDİRİR — hangi tanımın tutulduğunu, hangi takımın
    //          olduğunu, bırakmanın geçerli olup olmadığını sormaz
    /// <summary>
    /// İki paneldeki TEK bir düğme: bir yazı, bir simge ve bir seçim çerçevesi.
    ///
    /// <b>Bu tip <see cref="StructureBlueprintAsset"/>'i de
    /// <see cref="UnitBlueprintAsset"/>'i de tip olarak yazmaz</b> — ve bu,
    /// iki panelin aynı düğmeyi paylaşabilmesinin tek sebebidir. Düğme ne
    /// tuttuğunu bilmez, yalnızca kendi SIRASINI bilir; ne tuttuğunu bilen
    /// taraf onu KURAN paneldir.
    ///
    /// Neyi BİLMEZ: neyin yerleştirilebileceğini, hangi hücrenin boş olduğunu,
    /// üretimin hazır olup olmadığını. Üçü de yönetenin işi.
    ///
    /// AYNA BELGE: bu tipin gerekçeleri bugün yalnızca bu dosyada.
    /// </summary>
    // OLAY, DOĞRUDAN ÇAĞRI DEĞİL: bu tip yöneteni de paneli de tip olarak
    // yazmıyor. Alternatif ölçüldü ve reddedildi — düğmenin içine bir
    // ProductionDirector alanı konsaydı, sağ panelin düğmeleriyle sol panelin
    // düğmeleri aynı yöntemi çağırırdı ve "hangi paneldeyim" sorusu düğmenin
    // içinde yeniden doğardı.
    [RequireComponent(typeof(RectTransform))]
    public sealed class PaletteEntryView : MonoBehaviour,
        IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        // ÜÇ REFERANS DA ÇOCUK NESNELERDE YAŞIYOR ve üçü de boş bırakılabilir,
        // bu yüzden üçünde de null kontrolü var. Gerekçe UnitView'de ölçülerek
        // yazılı: [RequireComponent] yalnızca BU nesnenin üstündeki bileşeni
        // garanti eder, çocuktakini etmez.
        [Header("Wire the children of the entry prefab")]
        [Tooltip("Text drawn on the button. Left empty, the entry shows no label.")]
        [SerializeField] private Text label;

        [Tooltip("Icon drawn on the button. Left empty, the entry shows no icon.")]
        [SerializeField] private Image icon;

        [Tooltip("Frame shown while this entry is the selected one. Left empty, selection is invisible.")]
        [SerializeField] private Image selectionFrame;

        /// <summary>Düğmeye tıklandı.</summary>
        public event Action<PaletteEntryView> Clicked;

        /// <summary>Sürükleme başladı.</summary>
        public event Action<PaletteEntryView> DragBegan;

        /// <summary>Sürükleme sürüyor; ekran noktası taşınıyor.</summary>
        public event Action<PaletteEntryView, Vector2> Dragged;

        /// <summary>Sürükleme bitti; ekran noktası bırakıldı.</summary>
        public event Action<PaletteEntryView, Vector2> DragEnded;

        /// <summary>
        /// Bu düğmenin kendi panelindeki sırası. Bir oyun durumu değil, panelin
        /// verdiği ADRESTİR: panel bu sayıyla kendi listesine geri döner.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Düğmeyi kurar. Panel her yeniden çizimde bunu çağırır.
        /// </summary>
        /// <param name="entryIndex">Panelin kendi listesindeki sıra.</param>
        /// <param name="text">Düğmenin yazısı.</param>
        /// <param name="sprite">
        /// Düğmenin simgesi; <c>null</c> geçilebilir ve o zaman simge alanı
        /// GİZLENİR. Sağ panel bugün her zaman <c>null</c> geçiyor — gerekçesi
        /// oradaki üretim listesinin motor tarafındaki varlığı değil, düz C#
        /// tanımını taşıması.
        /// </param>
        public void Bind(int entryIndex, string text, Sprite sprite)
        {
            Index = entryIndex;

            if (label != null)
            {
                label.text = text;
            }

            if (icon != null)
            {
                // enabled, SetActive DEĞİL: nesneyi kapatmak çocuklarını da
                // kapatır ve OnEnable/OnDisable geri çağrılarını tetikler.
                // İstenen tek şey "bu kareyi çizme" — aynı karar UnitView'in
                // SetSelected üyesinde de yazılı.
                icon.enabled = sprite != null;
                icon.sprite = sprite;
            }

            SetSelected(false);
        }

        /// <summary>
        /// Seçim çerçevesini gösterir ya da gizler.
        /// </summary>
        // "SEÇİLİ MİYİM" BURADA SAKLANMIYOR: tek doğruluk kaynağı paneldir ve
        // panel her seferinde bütün düğmelerine tek tek söyler. Bir bool
        // konsaydı panelin listesi ile düğmelerin hâli sessizce ayrışabilirdi —
        // aynı hatanın ikizi UnitView'de adı konmuş durumda.
        public void SetSelected(bool isSelected)
        {
            if (selectionFrame == null)
            {
                return;
            }

            selectionFrame.enabled = isSelected;
        }

        // TIKLAMA İLE SÜRÜKLEME AYNI OLAY DEĞİL VE UNITY BUNU KENDİ AYIRIYOR:
        // OnPointerClick, sürükleme eşiği aşıldıysa ZATEN çağrılmaz. Bu ayrımı
        // burada elle kurmak (bir mesafe eşiği tutmak) motorun zaten yaptığı işi
        // ikinci kez yapmak olurdu — ve bu ağaçta o eşiğin bir sahibi de var:
        // tahtanın kendi işaretçi jesti, ama o DÜNYA birimleriyle ölçüyor,
        // burası ekran pikseliyle.
        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            DragBegan?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Dragged?.Invoke(this, eventData.position);
        }

        // BIRAKMA NOKTASI OLAYDAN OKUNUYOR, fareden değil: olay parmağın
        // KALKTIĞI noktayı taşır ve fareyi burada ikinci kez okumak, dokunmatik
        // bir girdide her zaman yanlış cevap verirdi — parmak kalktıktan sonra
        // okunacak bir konum yoktur.
        public void OnEndDrag(PointerEventData eventData)
        {
            DragEnded?.Invoke(this, eventData.position);
        }
    }
}
