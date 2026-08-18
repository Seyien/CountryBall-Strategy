namespace GridStrategy.Combat
{
    // ═══ ROL: TANIM (Profile) ════════════════════════════════════════
    // kimlik : yok — iki Downed değeri aynı şeydir
    // hafıza : yok — bir değer, kendisi hiçbir şey yapmaz
    // Unity  : gerekmez
    // karar  : vermez — durumu ADLANDIRIR; geçişlere UnitLifecycle karar verir
    /// <summary>
    /// Bir birimin yaşam döngüsündeki üç durumu.
    ///
    /// Neden bool değil: `IsAlive` bir bool'du ve üçüncü durumu ifade edemiyordu.
    /// "Ölü ama 10 saniye içinde diriltilebilir" ne canlıdır ne de kalıcı ölü —
    /// ve hasar almaya DEVAM etmesi gerekir. Bool'la yazılsaydı bu kural sessizce
    /// kaybolurdu.
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
