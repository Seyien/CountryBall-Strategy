namespace GridStrategy.Combat
{
    // ═══ ROL: TANIM (Profile) ════════════════════════════════════════
    // kimlik : yok — iki Destroyed değeri aynı şeydir
    // hafıza : yok — ölçüsü şu: bir değişkene StructureState.Destroyed yaz
    //          ve bekle, hâlâ Destroyed'dır; "ne kadar zamandır enkaz"
    //          bilgisini bu değer taşımaz. Sayacı StructureLifecycle ayrı
    //          bir alanda (remainingSeconds) tutar, IsReadyForCleanup'ı o açar
    // Unity  : gerekmez
    // karar  : vermez — yapının durumunu ADLANDIRIR; geçişe StructureLifecycle
    //          karar verir
    // AYNI DEĞERLER, AYNI GEÇİŞLER DEMEK DEĞİLDİR. UnitState'i paylaşmak
    // reddedildi: birimin grafiğinde Downed'a GERİ DÖNEN bir ok var, yapının
    // grafiğinde yok. Ortak enum, yapı üzerindeki her switch'e asla çalışmayan
    // bir Downed dalı açardı; ödenen bedel TargetingRules'taki aşırı yükleme.
    // → StructureState.md#enum-structurestate
    /// <summary>
    /// Bir yapının iki durumu: ayakta ya da yıkılmış.
    ///
    /// Neden <see cref="UnitState"/> değil: bir baraka DÜŞMEZ. Düşme durumu
    /// (<see cref="UnitState.Downed"/>) "ölü ama kurtarılabilir" demektir ve
    /// tek var olma sebebi diriltme penceresidir. Yapıda böyle bir pencere yok:
    /// yıkılan bina onarılmaz, yeniden İNŞA edilir — ve yeniden inşa, bu tipin
    /// bir geçişi değil, yeni bir yapı nesnesidir.
    ///
    /// Neden <c>bool isDestroyed</c> değil: bugün iki değer var, ama ayrım
    /// enum'da durduğu sürece üçüncü bir durum tartışılabilir kalır; bool ile o
    /// tartışma hiç yazılamaz, yalnızca iç içe if'lere dönüşür.
    ///
    /// ÖDENEN BEDEL — ve ödendi: <see cref="TargetingRules"/> artık İKİ durum
    /// dili konuşuyor. Bedelin tamamı o çift değil; asimetrinin geri kalanı
    /// belgede yazılı.
    ///
    /// GEREKÇELER: Docs/deep/kod/Core/Combat/StructureState.md
    /// </summary>
    public enum StructureState
    {
        // ATANMAYI UNUTULAN ALAN, TAHTADA BİR ŞEY ÜRETMEMELİ. Sıfırıncı değer
        // BİLEREK "yıkık": enkaz, "burada bir şey yok"a en yakın değerdir.
        // Standing sıfır olsaydı `new StructureState[9]` tahtada dokuz SAĞLAM
        // bina üretirdi. Koruma addan değil, üyenin sıfırıncı KONUMUNDAN gelir.
        // → StructureState.md#destroyed
        /// <summary>
        /// Yıkılmış. Hedeflenemez, saldırmaz, onarılamaz; geriye yalnızca enkaz
        /// temizliği kalır. Sıfırıncı değer olması güvenlik kararıdır.
        /// </summary>
        Destroyed,

        // BİR DEĞER, EN AZ BİR KURALI DEĞİŞTİREREK YERİNİ HAK EDER. Üçüncü bir
        // Rubble değeri reddedildi: hedeflenme, saldırı ve onarım kurallarının
        // hiçbirinde Destroyed'dan ayrışmıyor — ayrışan tek şey "artık
        // kaldırılabilirim" ve o bir İSTEK, IsReadyForCleanup'ta duruyor.
        // → StructureState.md#rubble
        /// <summary>Ayakta. Hedeflenebilir, canı azalır, onarılabilir.</summary>
        Standing
    }
}
