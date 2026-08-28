using GridStrategy.Core;

namespace GridStrategy.Unity
{
    //   ═══ KİPİN TAHTAYA BAKAN PENCERESİ ════════════════════════════
    //
    //     BoardAdapter (MonoBehaviour)          kipler (sade C#)
    //     ────────────────────────────          ────────────────
    //     Input, Camera, Battle,   ──────►  IPlacementModeHost ──► YERLEŞTİRME
    //     hayalet, jest, görseller ──────►  IUnitOrderHost ──────► EMİRLER
    //                                       (Orders/, kip DEĞİL)
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

    // ═══ BURADA ÜÇÜNCÜ BİR SÖZLEŞME DURUYORDU: IPendingStrikeHost ═══
    // Dokuz üyesi vardı (altısı kendisinin, üçü IBoardModeHost'tan) ve bekleyen
    // vuruşun TEK emrini tahtada tutuyordu: StrikeAttacker, StrikeTarget,
    // WriteStrikeOrder, ClearStrikeOrder, IsOnBoard, IsViewWalking,
    // ExecuteStrike. Yerini dört üyeli `IUnitOrderHost` aldı ve emirler kip
    // olmaktan çıktı — çünkü kip TEKTİR, emir ÇOĞUL.
    //
    // Bir önceki devir belgesinin bu tura koyduğu ölçüt buydu: "IPlacementModeHost
    // (7 üye) ve IPendingStrikeHost (9 üye) kalan bağımlılığın fotoğrafı; god
    // object bölündükçe daralmalı, daralmazlarsa pattern kozmetik kalmış
    // demektir." Dokuz üye dört üyeye indi ve sahibi değişti.
    // → Orders/IUnitOrderHost.cs
    // → Docs/deep/konular/09-kararlarin-cevrilmesi.md (madde 2)
}
