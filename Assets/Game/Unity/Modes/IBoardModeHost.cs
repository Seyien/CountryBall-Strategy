using GridStrategy.Core;

namespace GridStrategy.Unity
{
    //   ═══ KİPİN TAHTAYA BAKAN PENCERESİ ════════════════════════════
    //
    //     BoardAdapter (MonoBehaviour)          kipler (sade C#)
    //     ────────────────────────────          ────────────────
    //     Input, Camera, Battle,   ──────►  IPlacementModeHost ──► YERLEŞTİRME
    //     hayalet, jest, görseller ──────►  IPendingStrikeHost ──► BEKLEYEN VURUŞ
    //
    //     Ok TEK YÖN: kip tahtayı arayüzüyle tanır, tahta kipi ADIYLA tanır.
    //     Testte tahtanın yerine sahte bir host konur ve kip aynen koşar.

    // ═══ REFERANSTAN ALINAN VE REDDEDİLEN ════════════════════════════
    // Ölçüt aldığımız depoda (BuildingSystem/State) kipler tahtaya
    // `Map.Instance` üzerinden ulaşıyor. ALINAN: kipin kendi kurucusunda
    // ihtiyaçlarını isteyip alanlarda saklaması. REDDEDİLEN: singleton — o
    // yol tek bir global tahta varsayar, oysa bu projede aynı sahnede iki
    // BoardAdapter iki ayrı savaş doğuruyor ve 46 Unity testi tam da global
    // durum olmadığı için yan yana koşabiliyor.

    /// <summary>
    /// Her kipin tahtadan istediği ORTAK asgari: kim seçili, nereye yazılır ve
    /// kipten nasıl çıkılır.
    /// </summary>
    // ÜÇ SÖZLEŞME TEK DOSYADA ve bu bilerek: ikisi ötekinin daraltılmış hâli,
    // ayrı dosyalarda okunduklarında hangisinin neyi EKLEDİĞİ görünmez olurdu.
    public interface IBoardModeHost
    {
        /// <summary>Tahtada şu an seçili olan kimlik; seçim yoksa null.</summary>
        Unit SelectedUnit { get; }

        /// <summary>
        /// Kipin oyuncuya söylediği cümleyi Console'a yazar.
        /// </summary>
        // KİP Debug.Log'u KENDİ ÇAĞIRMIYOR ve sebebi ölçülebilir: Unity'nin
        // Console satırına tıklandığında vurgulanacak nesne (context) tahtadır,
        // kipin elinde öyle bir nesne yok. Ayrıca sahte host testte cümleyi
        // yakalayabiliyor.
        void Log(string message);

        /// <summary>
        /// Bu kip hâlâ yürürlükteyse Boşta kipine döner.
        /// </summary>
        // "HÂLÂ YÜRÜRLÜKTEYSE" ŞARTI ARAYÜZE YAZILI ve boşuna değil: kipin
        // kendisini kapatması ile tahtanın başka bir kipe geçmesi aynı karede
        // olabilir, ve şartsız bir çıkış o yeni kipi daha doğduğu karede
        // öldürürdü.
        void LeaveMode(IBoardMode mode);
    }

    /// <summary>
    /// Yapı yerleştirme kipinin tahtadan istedikleri: hayalet, jest ve
    /// yerleştirmenin kendisi.
    /// </summary>
    public interface IPlacementModeHost : IBoardModeHost
    {
        /// <summary>Oyuncu bu karede iptal tuşuna bastı mı?</summary>
        // GİRDİ SORUSU TAHTAYA SORULUYOR, kip Input'a HİÇ dokunmuyor — kipin
        // EditMode'da sınanabilmesinin tek sebebi bu satır.
        bool PlacementCancelRequested { get; }

        /// <summary>Önizleme hayaletini gösterir ya da gizler.</summary>
        void ShowPlacementGhost(bool visible);

        /// <summary>Hayaleti verilen hücrenin merkezine taşır.</summary>
        void MovePlacementGhostTo(int x, int y);

        /// <summary>Farenin durduğu hücreyi ve dünya noktasını verir.</summary>
        bool TryReadPointerCell(out float worldX, out float worldY, out int x, out int y);

        /// <summary>Tıklama ile sürüklemeyi ayıran jesti sıfırlar.</summary>
        void ResetPointerGesture();

        /// <summary>Bu karenin fare durumunu jeste verir ve çıkan fazı döner.</summary>
        PointerPhase FeedPointerGesture(float worldX, float worldY);

        /// <summary>Yapıyı verilen hücreye koymayı SAVAŞA sorar.</summary>
        // GEÇERLİLİK KARARI KİPTE DEĞİL ve olmamalı: hangi hücrenin uygun
        // olduğunu BattleActions.PlaceStructure biliyor, kip yalnızca
        // "şimdi koy" diyor.
        void CommitPlacement(int x, int y);
    }

    /// <summary>
    /// Bekleyen vuruş kipinin tahtadan istedikleri: emrin iki kimliği, iki
    /// hayatta kalma sorusu ve vuruşun kendisi.
    /// </summary>
    // İKİ KİMLİK NEDEN TAHTADA DURUYOR — ÖLÇÜLDÜ VE BİR BEDELİ VAR: saldıranı
    // ve hedefi yalnızca kip değil, temizlik de okuyor (tahtadan kalkan kimlik
    // emri de götürür) ve mevcut testler o alanları ADIYLA yazıp okuyor. Hedefin
    // HÜCRESİ ise bu sözleşmede hiç yok, çünkü onu soran tek taraf kipin kendisi.
    public interface IPendingStrikeHost : IBoardModeHost
    {
        /// <summary>Emri yazan saldıran; emir yoksa null.</summary>
        Unit StrikeAttacker { get; }

        /// <summary>Emrin hedefi; emir yoksa null.</summary>
        Unit StrikeTarget { get; }

        /// <summary>Emrin iki kimliğini yazar; öncekini sessizce ezer.</summary>
        // HEDEFİN HÜCRESİ BURADA YOK ve yokluğu ölçülmüş: o sayıyı okuyan tek
        // yer emrin kendi yürümesi, yani kipin İÇİ. Tahtaya yazılsaydı hiçbir
        // çağıranı olmayan iki alan daha doğardı.
        void WriteStrikeOrder(Unit attacker, Unit target);

        /// <summary>Emri siler. Tahtaya ve savaşa HİÇ dokunmaz.</summary>
        void ClearStrikeOrder();

        /// <summary>Bu kimlik hâlâ tahtada bir hücrede duruyor mu?</summary>
        bool IsOnBoard(Unit unit);

        /// <summary>Bu kimliğin görseli şu anda yürüyor mu?</summary>
        bool IsViewWalking(Unit unit);

        /// <summary>Vuruşu savaşa yaptırır ve ekranı sonuca göre günceller.</summary>
        void ExecuteStrike(Unit attacker, Unit target, int x, int y);
    }
}
