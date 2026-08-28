using GridStrategy.Core;

namespace GridStrategy.Unity
{
    //   ═══ KİP İLE EMRİN SINIRI — TEK CÜMLEDE ═══════════════════════
    //
    //     KİP  (IBoardMode)          EMİR (IUnitOrder)
    //     TAHTA ne yapıyor           HER BİRİME ne söylendi
    //     TEKTİR                     ÇOĞULDUR
    //     BoardModeMachine.Current   UnitOrderBook[birim]
    //
    //     Girdi ne DEMEK  -> kip     Birim ne YAPIYOR -> emir
    //
    //   AYNI KAREDE: tahta yerleştirme kipinde OLABİLİR ve aynı anda
    //   üç savaşçı kendi hedefine vurmaya DEVAM EDER. Eski hâlde bu
    //   imkânsızdı; bekleyen vuruş tahta başına TEKTİ.

    /// <summary>
    /// Bir birime verilmiş, kareler arasında YAŞAYAN emir.
    ///
    /// OYUNDA NE İŞE YARAR: bir savaşçıya hedef gösterdiğinde o hedefe vurmayı
    /// sürdürür; oyuncunun her vuruş için tekrar tıklaması gerekmez.
    /// </summary>
    // NEDEN BİR NESNE, BİR BAYRAK DEĞİL: eski hâlde emir tahtanın DÖRT TEKİL
    // alanındaydı (pendingStrikeAttacker/Target/X/Y) ve ikinci bir birime emir
    // verildiği an birincisininki siliniyordu. Operatörün "iki taraf paralel
    // olmuyor" şikâyeti bir ayar değil bir SAHİPLİK hatasıydı: emir tahtaya
    // değil BİRİME ait.
    // → Docs/deep/konular/09-kararlarin-cevrilmesi.md (madde 2)
    //
    // ÜÇ ÜYE, DÖRT DEĞİL — <c>deltaSeconds</c> BİLEREK YOK. Devir belgesi
    // `Tick(float deltaSeconds)` öneriyordu ve "bağlayıcı değil, ölçü" diyordu;
    // ölçüm onu düşürdü: emrin kendi saati yok. Bekleme süresini Core sayıyor
    // (Combatant'ın sayacını battle.Tick ilerletiyor), yürüyüşü ise ekranın
    // kendi saati. Parametre alınsaydı hiçbir emrin okumadığı bir argüman
    // doğardı ve "bekleme kuralını emir de yazar mı" sorusu açık kalırdı.
    public interface IUnitOrder
    {
        /// <summary>
        /// Emrin ilgilendiği öteki kimlik: saldırılan düşman ya da kaldırılacak
        /// dost.
        /// </summary>
        // İKİ ÇAĞIRAN VAR ve ikisi de bu üyeyi hak ettiriyor: tahtadan kalkan
        // bir kimliği hedefleyen emirleri süpürmek (UnitOrderBook.CancelTargeting)
        // ve "bu tıklama zaten yazılı emrin aynısını mı istiyor" sorusu.
        Unit Target { get; }

        /// <summary>
        /// Emri bir kare ilerletir ve bundan sonraki hâlini söyler.
        /// </summary>
        OrderProgress Advance();

        /// <summary>
        /// Emrin oyuncuya söylenecek tek cümlelik hâli.
        /// </summary>
        // TİP SORGUSUNUN YERİNE GEÇİYOR: bu üye olmasaydı tahta, seçilen birimin
        // emrini yazdırmak için `order is AttackOrder` diye sorardı ve üçüncü
        // bir emir cinsi doğduğu gün o soru sessizce eskirdi.
        string Describe();
    }
}
