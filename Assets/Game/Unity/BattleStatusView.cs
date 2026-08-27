using GridStrategy.Combat;
using GridStrategy.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GridStrategy.Unity
{
    /// <summary>
    /// Ekranın üstündeki durum şeridi: SIRA KİMDE ve ŞU AN NE SEÇİLİ.
    ///
    /// OYUNDA NE İŞE YARAR: sıra tabanlı bir oyunda oyuncunun bilmesi gereken ilk
    /// şey sıranın kendisinde olup olmadığıdır. Bu şerit olmadan oyuncu, hamlesi
    /// neden reddedildiğini ancak Console'a bakarak öğrenebiliyordu.
    ///
    /// HİÇBİR ŞEY HESAPLAMAZ: tahtanın yayınladığı iki olaya abone olur ve gelen
    /// cümleyi yazar. Sıranın da seçimin de tek sahibi tahtadır.
    /// </summary>
    public sealed class BattleStatusView : MonoBehaviour
    {
        [Header("Board - drag the Board object")]
        [SerializeField] private BoardAdapter board;

        [Header("Labels")]
        [SerializeField] private Text turnLabel;
        [SerializeField] private Text selectionLabel;

        [Header("Turn colours")]
        [SerializeField] private Color playerColour = new Color(0.45f, 0.75f, 1f);
        [SerializeField] private Color enemyColour = new Color(1f, 0.5f, 0.45f);

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

        private void OnTurnChanged(Team team, int turnNumber)
        {
            if (turnLabel == null)
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
    }
}
