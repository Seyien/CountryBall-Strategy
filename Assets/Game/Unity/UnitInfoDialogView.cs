using GridStrategy.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace GridStrategy.Unity
{
    /// <summary>
    /// Bir birim ya da yapı türünün bilgi penceresi: adı, simgesi, sayıları ve
    /// açıklaması. Kapalı doğar, kapalı yaşar; yalnız açıldığında görünür.
    ///
    /// OYUNDA NE İŞE YARAR: oyuncu bir türün canını, hasarını ve menzilini
    /// öğrenmek için varlık dosyasını açmak zorunda kalmıyor.
    ///
    /// TUZAK: bileşen pencerenin KÖKÜNDE değil, kökün ÜSTÜNDE yaşar — kapattığı
    /// nesnenin üstünde olsaydı kapandığı an hem aboneliğini hem Escape tuşunu
    /// kaybederdi.
    /// </summary>
    // HİÇBİR ŞEY HESAPLAMAZ: satırların sahibi BlueprintSummary, cümlelerin
    // sahibi tasarımcının varlık dosyası. Bu tip yalnız GÖSTERİYOR ve kapanma
    // yollarını topluyor.
    //
    // ██ TAHTANIN ÖLÇÜSÜ HİÇ SORULMUYOR ██
    // Yazılı yasak: boardRect, width, height ve BoardSizing okunmayacak. Pencere
    // ekranın kendi ölçüsüne yaslanıyor (perde ebeveynini kaplıyor, içerik
    // kaydırma taşıyor), yani tahta 5x10'dan 100x50'ye çıktığında bu dosyada tek
    // bir satır değişmiyor. Ölçüsü şu: aşağıda `board` alanı YALNIZ savaşın
    // bittiğini duymak için var, tahtanın ölçüsünü sormak için değil.
    public sealed class UnitInfoDialogView : MonoBehaviour
    {
        // ██ BU ALAN ÖLÇÜ SORMUYOR, TEK BİR OLAY DİNLİYOR ██
        // Savaş bitince pano açılıyor ve iki modal aynı anda duramaz. Bağ bir
        // ölçü bağı olsaydı yukarıdaki yasak düşerdi; bu bir OLAY bağı.
        [Header("Board - drag the Board object")]
        [Tooltip("Savaşın bittiğini duyuran tahta. Boş kalırsa pencere pano açılırken kapanmaz.")]
        [SerializeField] private BoardAdapter board;

        // ██ AÇILIP KAPANAN NESNE BU BİLEŞENİN KENDİ NESNESİ DEĞİL ██
        // Bir GameObject kapandığında üstündeki her bileşenin OnDisable'ı koşar
        // ve Update'i durur. Pencere bu bileşenin üstünde olsaydı, kapandığı an
        // Escape tuşunu dinleyen kimse kalmaz ve pencere bir daha hiç açılmazdı.
        // Gerekçenin ikizi BattleOverView'de yazılı ve ikisi birlikte okunmalı.
        [Header("Dialog - built by CountryBall/Sahneyi Kur")]
        [Tooltip("Açılıp kapanan çocuk nesne. Boş kalırsa pencere hiç görünmez.")]
        [SerializeField] private GameObject dialog;

        [Tooltip("Türün adını yazan başlık. Boş kalırsa pencere adsız açılır.")]
        [SerializeField] private Text titleLabel;

        [Tooltip("Türün simgesi. Simgesiz bir tür açıldığında bu nesne gizlenir.")]
        [SerializeField] private Image iconImage;

        [Tooltip("Can, hasar, menzil ve üretim satırları. Metnin sahibi BlueprintSummary.")]
        [SerializeField] private Text statsLabel;

        [Tooltip("Tasarımcının yazdığı açıklama. Boş bırakılan tür için bu nesne gizlenir.")]
        [SerializeField] private Text descriptionLabel;

        /// <summary>
        /// Pencere şu anda açık mı.
        /// </summary>
        // CEVABIN TEK KAYNAĞI NESNENİN KENDİ HÂLİ, bir bool alan DEĞİL: ikinci
        // bir bayrak tutulsaydı, pencereyi Inspector'dan kapatan biri bayrağı
        // açık bırakır ve Escape kapalı bir pencereyi "kapatmaya" çalışırdı.
        public bool IsOpen => dialog != null && dialog.activeSelf;

        /// <summary>
        /// Bir birim türünü gösterir.
        /// </summary>
        /// <param name="asset">Gösterilecek tür; <c>null</c> ise pencere açılmaz.</param>
        public void Show(UnitBlueprintAsset asset)
        {
            if (asset == null)
            {
                return;
            }

            Present(
                asset.DisplayName,
                asset.Icon,
                BlueprintSummary.Describe(asset.Definition),
                asset.Description);
        }

        /// <summary>
        /// Bir yapı türünü gösterir.
        /// </summary>
        /// <param name="asset">Gösterilecek tür; <c>null</c> ise pencere açılmaz.</param>
        // İKİ AYRI ÜYE, TEK BİR object PARAMETRESİ DEĞİL: ortak bir taban tip
        // yok (ikisi de ScriptableObject'ten ayrı ayrı türüyor) ve object alan
        // bir imza, yanlış tipi DERLEME zamanında değil çalışma zamanında
        // yakalardı.
        public void Show(StructureBlueprintAsset asset)
        {
            if (asset == null)
            {
                return;
            }

            Present(
                asset.DisplayName,
                asset.Icon,
                BlueprintSummary.Describe(asset.Definition),
                asset.Description);
        }

        /// <summary>
        /// Pencereyi kapatır. Zaten kapalıysa hiçbir şey yapmaz.
        /// </summary>
        // ÜÇ KAPANMA YOLUNUN TEK VARIŞ NOKTASI: çarpı düğmesi, perdeye tıklama
        // ve Escape. Üçü ayrı ayrı yazılsaydı biri düzeltildiğinde ötekiler
        // eskirdi — ve kapanmayan bir modal, oyuncuyu tahtadan tamamen keser.
        public void Close()
        {
            ShowDialog(false);
        }

        // ABONELİK OnEnable/OnDisable ÇİFTİNDE, BattleOverView'in birebir
        // gerekçesiyle: Awake'te abone olup hiç bırakmamak, nesne kapatıldığında
        // ölü bir dinleyici bırakırdı.
        private void OnEnable()
        {
            // PENCERE HER AÇILIŞTA KAPATILIYOR: sahnede açık bırakılmış olabilir
            // ve araç onu açıkken kaydedebilir. Yazılı durumu değişmeze çevirmek,
            // BattleOverView'in OnEnable'ında yaptığı işin aynısı.
            ShowDialog(false);

            if (board == null)
            {
                return;
            }

            board.BattleEnded += OnBattleEnded;
        }

        private void OnDisable()
        {
            if (board == null)
            {
                return;
            }

            board.BattleEnded -= OnBattleEnded;
        }

        // ██ AYNI ANDA TEK MODAL — VE BU ÜYE O SÖZLEŞMENİN TAMAMI ██
        // Savaş bitince pano ekranı kaplıyor. Bilgi penceresi açık kalsaydı iki
        // modal üst üste binerdi ve panonun yeniden başlat düğmesi bu pencerenin
        // perdesinin altında kalabilirdi.
        //
        // SORU PANOYA SORULMUYOR, OLAY DİNLENİYOR: pano bu tipe tanıtılsaydı iki
        // görsel birbirini tanır ve üçüncü bir modal doğduğu gün her biri
        // ötekileri saymak zorunda kalırdı.
        private void OnBattleEnded(BattleOutcome outcome)
        {
            Close();
        }

        // ESCAPE HER KAREDE SORULMUYOR, YALNIZ AÇIKKEN: kapalı bir pencere için
        // tuş okumak, aynı tuşu bekleyen başka bir dinleyici doğduğu gün
        // ikisinin de aynı basışı yemesine yol açardı.
        private void Update()
        {
            if (!IsOpen)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }

        /// <summary>
        /// Alanları yazar ve pencereyi açar.
        /// </summary>
        // PENCERE EN SONDA AÇILIYOR: satırlar yazılmadan açılsaydı oyuncu bir
        // kare boyunca bir önceki türün sayılarını görürdü. Gerekçenin ikizi
        // BattleOverView'in OnBattleEnded üyesinde yazılı.
        private void Present(string title, Sprite icon, string stats, string description)
        {
            if (titleLabel != null)
            {
                titleLabel.text = title;
            }

            if (statsLabel != null)
            {
                statsLabel.text = stats;
            }

            // SİMGESİZ TÜR İÇİN NESNE GİZLENİYOR, boş bir kare bırakılmıyor:
            // atanmamış bir Image beyaz bir dikdörtgen çizer ve oyuncu onu bir
            // simge sanar. Varlık dosyası simgeyi boş bırakmaya zaten izin
            // veriyor ve o iznin ekrandaki karşılığı budur.
            if (iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.gameObject.SetActive(icon != null);
            }

            // AÇIKLAMASI OLMAYAN TÜR İÇİN ETİKET GİZLENİYOR ve bugün bu dal
            // KURALIN KENDİSİ: yirmi dört türün açıklaması operatörden bekleniyor
            // ve o metinler yazılana kadar boş bir etiket, düzende sebepsiz bir
            // boşluk bırakırdı.
            if (descriptionLabel != null)
            {
                descriptionLabel.text = description;
                descriptionLabel.gameObject.SetActive(!string.IsNullOrWhiteSpace(description));
            }

            ShowDialog(true);
        }

        private void ShowDialog(bool visible)
        {
            if (dialog != null)
            {
                dialog.SetActive(visible);
            }
        }
    }
}
