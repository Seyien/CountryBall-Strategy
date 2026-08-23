namespace GridStrategy.Combat
{
    // ═══ ROL: TANIM (Profile) ════════════════════════════════════════
    // kimlik : yok — iki Downed değeri aynı şeydir
    // hafıza : yok — ölçüsü şu: bir değişkene UnitState.Downed yaz ve
    //          saatlerce bekle, hâlâ Downed'dır. Kurtarma penceresinin
    //          dolduğunu FARK EDEN şey bu tip değil, sayacı tutan
    //          UnitLifecycle.Tick'tir; değer nereden geldiğini de taşımaz,
    //          onu Combatant ayrı bir alanda (lastObservedState) tutuyor
    // Unity  : gerekmez
    // karar  : vermez — durumu ADLANDIRIR; geçişlere UnitLifecycle karar verir
    // GEÇERSİZ HÂL TİPTE VAR OLMAMALI. İki bool (isAlive, isDowned) reddedildi:
    // dört kombinasyon üretir ve dördüncüsü — ikisi birden doğru — anlamsızdır
    // ama YAZILABİLİR. Enum'da o hücre tipte hiç yok; engellenmiyor, doğmuyor.
    // Derleyicinin eksik dal uyarısı yalnız switch İFADESİnde gelir, ikramiyedir.
    // → UnitState.md#enum-unitstate
    /// <summary>
    /// Bir birimin yaşam döngüsündeki üç durumu.
    ///
    /// Neden bool değil: bu tip doğmadan önce durumu tutan şey bir bool'du ve
    /// üçüncü durumu ifade edemiyordu. "Ölü ama 10 saniye içinde
    /// diriltilebilir" ne canlıdır ne de kalıcı ölü — ve hasar almaya DEVAM
    /// etmesi gerekir. Bool'la yazılsaydı bu kural sessizce kaybolurdu.
    ///
    /// GEREKÇELER: Docs/deep/kod/Core/Combat/UnitState.md
    /// </summary>
    // DERİN ANLATIM: Docs/deep/konular/05-yasam-dongusu.md
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
