namespace GridStrategy.Combat
{
    // ═══ ROL: TANIM (Profile) ════════════════════════════════════════
    // kimlik : yok — iki Downed değeri aynı şeydir
    // hafıza : yok — bir değer, kendisi hiçbir şey yapmaz
    // Unity  : gerekmez
    // karar  : vermez — durumu ADLANDIRIR; geçişlere UnitLifecycle karar verir
    //
    // REDDEDILEN - UnitState.cs:32 yerine:
    //     public bool isAlive;
    //     public bool isDowned;
    // KIRILAN  : iki bayrak dört kombinasyon üretir ve dördüncüsü hiçbir şey demez.
    //            isAlive && isDowned      -> derlenir, anlamı yok, kimse yakalamaz
    //            switch yerine iç içe if  -> unutulan dalı derleyici gösteremez
    //            derleyici: hiçbir şey der  .  test: geçersiz hâl için test bile yazılamaz
    // KAZANIRDI: durum gerçekten iki değerliyse ve üçüncüsünün eklenmeyeceği
    //            tasarımca kesinse — o gün enum bir tipi boşuna eklemiş olurdu.
    // KARSILASTIRMA:
    //     tek bool         iki değer   -> üçüncü durum HİÇ yazılamaz
    //     iki bool         dört hâl    -> biri anlamsız; geçerlilik akılda tutulur
    //     enum UnitState   üç değer    -> geçersiz hâl TİPTE yok, eksik dal görünür
    // TEK CUMLE: Enum geçersiz durumu YAZILAMAZ kılar; bayraklar onu yazılabilir
    //            bırakıp doğruluğu programcının hafızasına devreder.
    /// <summary>
    /// Bir birimin yaşam döngüsündeki üç durumu.
    ///
    /// Neden bool değil: bu tip doğmadan önce durumu tutan şey bir bool'du ve
    /// üçüncü durumu ifade edemiyordu. "Ölü ama 10 saniye içinde diriltilebilir"
    /// ne canlıdır ne de kalıcı ölü — ve hasar almaya DEVAM etmesi gerekir.
    /// Bool'la yazılsaydı bu kural sessizce kaybolurdu.
    /// </summary>
    public enum UnitState
    {
        /// <summary>Ayakta. Hedeflenebilir, canı azalır.</summary>
        Alive,

        /// <summary>
        /// Düşmüş ama kurtarılabilir. Hedeflenmeye ve hasar almaya DEVAM eder —
        /// düşman ya geri sayımın dolmasını bekler ya da gidip bitirir.
        /// </summary>
        Downed,

        /// <summary>Kalıcı ölü. Diriltilemez; geriye yalnızca ceset temizliği kalır.</summary>
        Dead
    }
}
