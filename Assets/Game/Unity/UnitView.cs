using UnityEngine;

namespace GridStrategy.Unity
{
    // ═══ ROL: GÖRÜNÜM (View) ═════════════════════════════════════════
    // kimlik : var ama SAHNE kimliği — her kopya ayrı bir nesnedir; oyun
    //          kimliği Unit'te yaşar ve bu tip Unit'i hiç görmez
    // hafıza : yok — "seçili miyim" burada saklanmaz; tek doğruluk kaynağı
    //          BoardAdapter.selectedUnit, buradaki renderer yalnızca yansıması
    // Unity  : zorunlu — MonoBehaviour + SpriteRenderer; var olma sebebi motorun ta kendisi
    // karar  : vermez, uygular — SetSelected(true) gelirse çizer, kimin seçileceğini sormaz
    /// <summary>
    /// Bir birimin EKRANDAKİ karşılığı. Tahtanın kurallarını bilmez, nerede
    /// durduğunu bilmez, <see cref="GridStrategy.Core.Unit"/> tipini hiç görmez.
    /// Yalnızca kendi GÖRSEL durumunu (bugün: seçim çerçevesi) uygular.
    ///
    /// Var olma sebebi: adaptör "şu birimi seçili göster" demek istediğinde,
    /// çerçevenin bir ÇOCUK nesnede yaşadığını bilmek zorunda kalıyordu.
    /// O bilgi burada kapalı kalır; adaptör yalnızca komutu verir.
    /// </summary>
    public sealed class UnitView : MonoBehaviour
    {
        // Prefab'da hazır duran seçim çerçevesinin çizicisi.
        //
        // Neden runtime'da Instantiate edilmiyor: her seçimde nesne doğurup yok
        // etmek çöp (garbage) üretir ve kurulumu koda gömerdi. Prefab'da duran
        // bir çocuk ise Editor'de görülebilir, sprite'ı ve rengi elle
        // ayarlanabilir; kod yalnızca açıp kapatır.
        //
        // Tip neden GameObject değil SpriteRenderer: bu tek referanstan hem
        // "çizilsin mi" (enabled) hem "hangi renk" (color) doğrudan okunur.
        // GameObject tutsaydık her erişimde GetComponent gerekirdi.
        //
        // Seçim RENGİ burada bir alan DEĞİL. Renk zaten bu SpriteRenderer'ın
        // kendi color alanında yaşıyor ve prefab'da yazılıyor. Ayrıca bir
        // [SerializeField] Color tutsaydık aynı bilginin İKİ kaynağı olurdu
        // (DECISION_TOOLKIT sorusu 1) ve koddaki değer, Editor'de ayarlanan
        // rengi her Awake'te sessizce ezerdi.
        [Header("Selection overlay - assign the child SpriteRenderer from the prefab")]
        [SerializeField] private SpriteRenderer selectionOverlay;

        private void Awake()
        {
            // Eksik atama SESSİZ kalmasın: referans boşsa seçim hiç çalışmaz ve
            // ekranda hiçbir hata görünmez. Bir kez, doğuşta, gürültüyle söyle.
            if (selectionOverlay == null)
            {
                Debug.LogError(
                    "[UnitView] selectionOverlay is not assigned. Assign the SelectionOverlay child's SpriteRenderer on the Unit prefab.",
                    this);
                return;
            }

            // Prefab'da çerçeve AÇIK bırakılmış olabilir - Editor'de nasıl
            // göründüğüne bakmak için açıp öyle kaydetmek çok kolaydır.
            // Doğan her birim seçimsiz başlamak ZORUNDA. Bu satır, yazılı
            // durumu (authored state) çalışma zamanı değişmezine çevirir.
            SetSelected(false);
        }

        /// <summary>
        /// Seçim çerçevesini gösterir veya gizler.
        /// </summary>
        public void SetSelected(bool isSelected)
        {
            // Buradaki kontrol SESSİZ, çünkü aynı hata Awake'te bir kez zaten
            // bağırdı. Tıklama başına LogError yazmak Console'u doldurur ve
            // asıl mesajı gömerdi.
            if (selectionOverlay == null)
            {
                return;
            }

            // SetActive(false) DEĞİL: GameObject'i kapatmak çocuklarını da
            // kapatır ve OnDisable/OnEnable geri çağrılarını tetikler. Bizim
            // istediğimiz tek şey "bu kareyi çizme". renderer.enabled tam olarak
            // bunu söyler; nesne hayatta kalır, hiçbir yaşam döngüsü olayı
            // tetiklenmez, referanslar bozulmaz.
            //
            // Alternatif olarak rengin alfasını 0 yapmak da görünmez kılardı ama
            // nesne yine ÇİZİLİRDİ (görünmez bir draw call) ve prefab'da
            // ayarlanan gerçek rengi ezerdi.
            selectionOverlay.enabled = isSelected;

            // "Seçili miyim" bilgisi burada SAKLANMIYOR. Tek doğruluk kaynağı
            // BoardAdapter.selectedUnit; burada bir bool daha tutsaydık ikisi
            // kayabilir ve hata sessiz olurdu (DECISION_TOOLKIT sorusu 1).
            // Bu bileşen durumu tutmaz, durumu UYGULAR.
        }
    }
}
