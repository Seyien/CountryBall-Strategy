using GridStrategy.Battle;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GridStrategy.Unity
{
    /// <summary>
    /// Savaş bitince ekranı kaplayan pano: "KAZANDIN", "KAYBETTİN" ya da
    /// "BERABERE", altında bir yeniden başlat düğmesi. Savaş sürerken kapalıdır.
    /// TUZAK: bileşen panonun KÖKÜNDE değil, kökün ÜSTÜNDE yaşar — kapattığı
    /// nesnenin üstünde olsaydı kapandığı an aboneliğini de kaybederdi.
    /// </summary>
    // HİÇBİR ŞEY HESAPLAMAZ: tahtanın yayınladığı tek olaya abone olur ve gelen
    // sonucu yazar. "Kim kazandı" sorusunun sahibi savaşın kuralıdır.
    public sealed class BattleOverView : MonoBehaviour
    {
        [Header("Board - drag the Board object")]
        [Tooltip("Savaşın bittiğini duyuran tahta. Boş kalırsa pano hiç açılmaz.")]
        [SerializeField] private BoardAdapter board;

        // ██ AÇILIP KAPANAN NESNE BU BİLEŞENİN KENDİ NESNESİ DEĞİL ██
        // Bir GameObject kapandığında üstündeki her bileşenin OnDisable'ı koşar
        // ve bu bileşen aboneliğini orada bırakıyor. Pano bu bileşenin üstünde
        // olsaydı, "savaş sürerken kapalı" isteği aboneliği de kapatır ve olay
        // geldiğinde dinleyen kimse kalmazdı — pano bir daha hiç açılmazdı.
        [Header("Panel - built by CountryBall/Sahneyi Kur")]
        [Tooltip("Savaş bitince açılan çocuk nesne. Boş kalırsa yalnız yazı değişir, pano görünmez.")]
        [SerializeField] private GameObject panel;

        [Tooltip("Sonucu yazan büyük etiket. Boş kalırsa pano açılır ama cümlesiz kalır.")]
        [SerializeField] private Text titleLabel;

        // ██ ÜÇ RENGİN TEK YAZILABİLİR SAHİBİ BURASI ██
        // SceneSetupTool panonun zeminini ve düzenini yazıyor ama bu üç alana
        // HİÇ dokunmuyor. Dokunsaydı aynı renk hem sahnede hem araçta yaşar ve
        // Inspector'dan yapılan her değişiklik bir sonraki kurulumda sessizce
        // geri alınırdı.
        //
        // İKİ RENK KOMŞUDAN ÖDÜNÇ DEĞİL, AYNI ANLAMIN İKİNCİ KOPYASI: mavi biz,
        // kırmızı onlar kuralı BattleStatusView'de de yazılı ve orada seçimin
        // tarafını boyuyor. Ortak bir sahibe çıkarılmadı çünkü bugün paylaşılan
        // şey sayılar değil, o kuralın kendisi — üçüncü bir tüketici doğduğu gün
        // renkler bir ScriptableObject paletine taşınır.
        [Header("Result colours - blue is a win, red is a loss")]
        [Tooltip("Oyuncu kazandığında başlığın rengi.")]
        [SerializeField] private Color winColour = new Color(0.45f, 0.75f, 1f);

        [Tooltip("Düşman kazandığında başlığın rengi.")]
        [SerializeField] private Color loseColour = new Color(1f, 0.5f, 0.45f);

        [Tooltip("İki taraf da tükendiğinde başlığın rengi.")]
        [SerializeField] private Color drawColour = new Color(0.85f, 0.85f, 0.85f);

        /// <summary>
        /// Düğmenin çağırdığı üye: savaşı baştan kurar.
        /// </summary>
        // ██ SAHNEYİ MOTOR YENİDEN YÜKLÜYOR, ELLE BİR SIFIRLAMA YAZILMADI ██
        // Ölçü şu: savaşın bütün durumu sahnenin içinde doğuyor. Battle nesnesi
        // BoardAdapter.Awake'te kuruluyor, birimler orada doğuyor, görsel havuzu
        // orada kuruluyor ve üretim hattı sahnedeki bir bileşende yaşıyor. Sahne
        // yeniden yüklendiğinde bunların hepsi yeniden doğar; elle yazılmış bir
        // sıfırlama ise Awake'in yaptığı her işi İKİNCİ kez, ve her yeni alan
        // eklendiğinde bir gün eksik olarak sayardı.
        //
        // SAHNENİN KİMLİĞİ BURAYA YAZILMIYOR, SORULUYOR: buildIndex o anda açık
        // olan sahneyi gösteriyor. Sahne adı sabit olarak yazılsaydı sahne
        // yeniden adlandırıldığı gün derleyici susar, düğme çalışma anında
        // patlardı.
        //
        // REDDEDILEN - savaşı sahneye dokunmadan sıfırlayan bir yönetici tip.
        //     public void RestartBattle()
        //     {
        //         gameState.ResetBattle();   // yeni bir MonoBehaviour
        //     }
        // KIRILAN: o tipin sıfırlayacağı şeylerin listesi Awake'in içinde
        // duruyor ve iki listenin aynı kalmasını hiçbir şey zorlamıyor; sessizce
        // ayrıldıkları gün oyun bir öncekinin cesetleriyle başlar.
        // KAZANIRDI: yeniden başlatmanın sahne dışında yaşayan bir şeyi
        // koruması gerekseydi — puan tablosu, açılmış bölümler — sahne yükleme
        // tek başına yetmezdi.
        // TEK CUMLE: motorun cevabı bu oyunun bütün durumunu kapsıyor, o yüzden
        // araya bir katman koymanın karşılığı yok.
        public void RestartBattle()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        // ABONELİK OnEnable/OnDisable ÇİFTİNDE, komşusunun birebir gerekçesiyle:
        // Awake'te abone olup hiç bırakmamak, nesne kapatıldığında ölü bir
        // dinleyici bırakırdı. → BattleStatusView.cs
        private void OnEnable()
        {
            // PANO HER AÇILIŞTA KAPATILIYOR: sahnede açık bırakılmış olabilir ve
            // araç onu açıkken kaydedebilir. Yazılı durumu değişmeze çevirmek,
            // UnitView.Awake'in SetSelected(false) ile yaptığı işin aynısı.
            ShowPanel(false);

            if (board == null)
            {
                Debug.LogError(
                    "[BattleOverView] board is not assigned; the result panel will never open.",
                    this);
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

        /// <summary>
        /// Sonucu ekrana yazar ve panoyu açar.
        /// </summary>
        // ONGOING DALI YOK ve bu bir eksiklik değil: tahta yalnız savaş
        // bittiğinde yayın yapıyor, yani bu üye "sürüyor" cevabını hiç görmez.
        // Uydurma bir dal, hiç koşmayacak bir cümleyi bakımda tutardı.
        private void OnBattleEnded(BattleOutcome outcome)
        {
            if (titleLabel != null)
            {
                titleLabel.text = OutcomeCaption(outcome);
                titleLabel.color = OutcomeColour(outcome);
            }

            // PANO EN SONDA AÇILIYOR: yazı ve renk yazılmadan açılsaydı oyuncu
            // bir kare boyunca bir önceki savaşın cümlesini görürdü.
            ShowPanel(true);
        }

        private void ShowPanel(bool visible)
        {
            if (panel != null)
            {
                panel.SetActive(visible);
            }
        }

        /// <summary>
        /// Sonucun oyuncu tarafından okunan cümlesi.
        /// </summary>
        // CÜMLENİN SAHİBİ BURASI, SceneSetupTool DEĞİL: araç etikete yalnızca
        // düzen için bir yer tutucu yazıyor ve o yer tutucu hiç görünmüyor,
        // çünkü pano açılmadan önce buradan geçiliyor.
        private static string OutcomeCaption(BattleOutcome outcome)
        {
            switch (outcome)
            {
                case BattleOutcome.PlayerWon:
                    return "KAZANDIN";

                case BattleOutcome.EnemyWon:
                    return "KAYBETTİN";

                default:
                    return "BERABERE";
            }
        }

        private Color OutcomeColour(BattleOutcome outcome)
        {
            switch (outcome)
            {
                case BattleOutcome.PlayerWon:
                    return winColour;

                case BattleOutcome.EnemyWon:
                    return loseColour;

                default:
                    return drawColour;
            }
        }
    }
}
