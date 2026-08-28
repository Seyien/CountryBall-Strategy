using GridStrategy.Battle;
using GridStrategy.Combat;
using GridStrategy.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GridStrategy.Unity
{
    /// <summary>
    /// Ekranın üstündeki durum şeridi: SEÇİLİ ŞEY KİMİN ve NE HÂLDE.
    ///
    /// OYUNDA NE İŞE YARAR: oyuncu tıkladığı şeyin kendi tarafından mı düşmandan
    /// mı olduğunu rengiyle bir bakışta görür, canını ve hasarını da yanında
    /// okur. Bu şerit olmadan o bilgiye ancak Console'a bakarak ulaşılıyordu.
    ///
    /// HİÇBİR ŞEY HESAPLAMAZ: tahtanın yayınladığı iki olaya abone olur ve gelen
    /// cevabı yazar. Takımın da canın da tek sahibi savaşın defteridir.
    /// </summary>
    // ██ "SIRA: SEN" ÇEVRİLMİŞ BİR KARARDIR ██
    // Üst etiket şunu yazıyordu: `SIRA: SEN · 1. tur`. Ölçüm şu: tahta
    // FreeForAll kipinde kuruluyor (`turnMode = TurnMode.FreeForAll`) ve o
    // kipte `TurnState.EndTurn` sırayı HİÇ devretmiyor, `TurnNumber` de hiç
    // artmıyor. Yani etiket, oyunun ilk karesinde yazılan ve bir daha asla
    // değişmeyen ÖLÜ bir sayı gösteriyordu — üstelik oyuncuya var olmayan bir
    // kural (sıra beklemek) öğretiyordu.
    //
    // YENİ BİR MEKANİZMA EKLENMEDİ: sıra sistemi ne kaldırıldı ne değiştirildi,
    // `TurnChanged` aboneliği duruyor ve Alternating kipinde etiket aynen eski
    // cümlesini yazıyor. Değişen tek şey, sıranın konuşmadığı bir kipte o
    // etiketin BAŞKA bir doğruyu söylemesi.
    //
    // İKİ RENK YENİ DEĞİL: `playerColour` / `enemyColour` zaten sıranın tarafını
    // boyamak için duruyordu; bugün seçimin tarafını boyuyorlar. Aynı iki alan,
    // aynı iki anlam — mavi biz, kırmızı onlar.
    public sealed class BattleStatusView : MonoBehaviour
    {
        [Header("Board - drag the Board object")]
        [SerializeField] private BoardAdapter board;

        [Header("Labels")]
        [SerializeField] private Text turnLabel;
        [SerializeField] private Text selectionLabel;

        [Header("Side colours - blue is ours, red is theirs")]
        [SerializeField] private Color playerColour = new Color(0.45f, 0.75f, 1f);
        [SerializeField] private Color enemyColour = new Color(1f, 0.5f, 0.45f);

        [Tooltip("Colour of the top label when nothing is selected.")]
        [SerializeField] private Color idleColour = new Color(0.75f, 0.75f, 0.75f);

        // ABONELİK OnEnable/OnDisable ÇİFTİNDE: Awake'te abone olup hiç
        // bırakmamak, nesne kapatıldığında ölü bir dinleyici bırakırdı. Çift
        // hâlinde yazılması, birinin unutulmasını gözle görülür kılıyor.
        private void OnEnable()
        {
            if (board == null)
            {
                Debug.LogError(
                    "[BattleStatusView] board is not assigned; the status bar will stay empty.",
                    this);
                return;
            }

            board.TurnChanged += OnTurnChanged;
            board.SelectionChanged += OnSelectionChanged;

            // BAŞLANGIÇ DEĞERİ ELLE YAZILIYOR: olaylar yalnız DEĞİŞİMDE
            // tetikleniyor, dolayısıyla ilk kare boş kalırdı.
            OnSelectionChanged(null);
        }

        private void OnDisable()
        {
            if (board == null)
            {
                return;
            }

            board.TurnChanged -= OnTurnChanged;
            board.SelectionChanged -= OnSelectionChanged;
        }

        // SIRA TABANLI KİPTE ESKİ CÜMLE AYNEN GEÇERLİ ve bu dal o yüzden
        // silinmedi: Alternating kipinde sıra gerçekten devrediliyor, numara
        // gerçekten artıyor ve oyuncunun bilmesi gereken ilk şey yine bu.
        // FreeForAll'da ise etiket seçimin sahibi, dolayısıyla buradan
        // yazılmıyor — iki sahip aynı etikete yazsaydı hangisinin kazandığı
        // karenin sırasına kalırdı.
        private void OnTurnChanged(Team team, int turnNumber)
        {
            if (turnLabel == null || board.TurnMode == TurnMode.FreeForAll)
            {
                return;
            }

            bool player = team == Team.Player;
            turnLabel.text = player
                ? $"SIRA: SEN  ·  {turnNumber}. tur"
                : $"SIRA: DÜŞMAN  ·  {turnNumber}. tur";
            turnLabel.color = player ? playerColour : enemyColour;
        }

        private void OnSelectionChanged(Unit unit)
        {
            WriteSide(unit);

            if (selectionLabel == null)
            {
                return;
            }

            if (unit == null)
            {
                selectionLabel.text = "Seçim yok  —  bir birime tıkla";
                return;
            }

            selectionLabel.text = board.TryDescribe(unit, out string description)
                ? description
                : unit.Name;
        }

        /// <summary>
        /// Üst etikete seçili şeyin TARAFINI yazar ve rengini o taraftan alır.
        /// </summary>
        // AYRI BİR ÜYE, OnSelectionChanged'in İÇİNDE İKİ DAL DEĞİL: alt etiketin
        // erken çıkışı (selectionLabel null) üst etiketi de susturuyordu, oysa
        // ikisi ayrı alanlar ve biri atanmadan öteki atanmış olabilir.
        //
        // SIRA TABANLI KİPTE HİÇ YAZMIYOR: orada etiketin sahibi OnTurnChanged.
        private void WriteSide(Unit unit)
        {
            if (turnLabel == null || board.TurnMode != TurnMode.FreeForAll)
            {
                return;
            }

            if (unit == null)
            {
                turnLabel.text = "SAVAŞ SÜRÜYOR  —  seçim yok";
                turnLabel.color = idleColour;
                return;
            }

            // TAKIM SAVAŞIN DEFTERİNDEN OKUNUYOR, burada ikinci bir kopya
            // tutulmuyor. Kimlik defterde bulunamazsa (yarım kalmış temizlik,
            // test kurulumu) taraf yazılmıyor — uydurulmuş bir renk, oyuncuya
            // yanlış bir düşman göstermekten daha kötüdür.
            if (!board.TryGetTeam(unit, out Team team))
            {
                turnLabel.text = "SAVAŞ SÜRÜYOR";
                turnLabel.color = idleColour;
                return;
            }

            bool ours = team == Team.Player;
            turnLabel.text = ours ? "SENİN TAKIMIN" : "DÜŞMAN TAKIM";
            turnLabel.color = ours ? playerColour : enemyColour;
        }
    }
}
