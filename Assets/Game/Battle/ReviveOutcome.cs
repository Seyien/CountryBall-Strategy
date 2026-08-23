namespace GridStrategy.Battle
{
    // ═══ ROL: TANIM (Profile) ════════════════════════════════════════
    // kimlik : yok — iki Revived değeri aynı şeydir
    // hafıza : yok — ölçüsü şu: Revive'ın döndürdüğü Revived'ı bir değişkene
    //          al, sonra hedefi yeni bir saldırıyla tekrar düşür — değişken
    //          hâlâ Revived'dır, çünkü hedefin O ANKİ durumunu değil, geçmiş
    //          bir çağrının cevabını taşır. Hedefin şu anki durumunu
    //          Combatant.State söyler
    // Unity  : gerekmez
    // karar  : vermez — olup biteni ADLANDIRIR; kararı BattleActions verir
    /// <summary>
    /// Bir diriltme DENEMESİNİN sonucu. "Deneme" kelimesi kasıtlı: reddedilen
    /// bir diriltme de bir sonuçtur ve çağıranın onu ayırt etmesi gerekir.
    ///
    /// Neden <c>bool</c> değil: "dirildi mi" dört ayrı soruyu tek cevaba
    /// sıkıştırırdı ve çağıran dördüne farklı tepki verir. Neden
    /// <c>GridStrategy.Combat</c>'ta değil: menzili görebilen tek yer tahtayı ve
    /// savaşı birlikte tanıyan katmandır, sonuç tipi de üreticisiyle aynı yerde
    /// yaşar.
    ///
    /// GEREKÇELER: Docs/deep/kod/Battle/ReviveOutcome.md
    /// </summary>
    // DERİN ANLATIM: Docs/deep/konular/06-sonuc-enumlari.md
    public enum ReviveOutcome
    {
        // Sıfırıncı değer BİLEREK bir RET: default(ReviveOutcome) "dirildi"
        // demek olsaydı, atanması unutulan her alan sessizce bir başarı gibi
        // okunurdu. → ReviveOutcome.md#rejectedinvalidtarget
        /// <summary>
        /// Hedef diriltilemez: ayakta, kalıcı ölü, karşı takımda ya da taraflardan
        /// biri tarafsız (<c>TargetingRules.CanBeRevived</c>).
        /// </summary>
        RejectedInvalidTarget,

        /// <summary>Hedef diriltilebilirdi ama ulaşılamadı.</summary>
        RejectedOutOfRange,

        // Ad iki enum'dan ödünç alındı çünkü cümlesi üçünde de birebir aynı;
        // farklı bir ad çağırana aynı cevabı üçüncü kez öğretirdi. Değer İKİ
        // sebebi birden taşıyor — sıra ve diriltenin durumu — ve çağıran
        // açısından ikisi tek cevaptır. → ReviveOutcome.md#rejectedactorcannotact
        /// <summary>
        /// Dirilten şu an eylem yapamaz. İki sebebi var: sıra o tarafta değil,
        /// ya da diriltenin kendisi ayakta değil
        /// (<c>ReviveRules.CanRevive</c>).
        /// </summary>
        RejectedActorCannotAct,

        // Bu enum saldırınınkiyle BİRLEŞTİRİLMEDİ: iki sözlüğün kesişimi üç,
        // birleşimi yedi. Birleşme her çağırana kendi eylemi için asla
        // dönmeyecek dallar yazdırırdı ve eksik enum değeri için `switch`
        // DEYİMİ uyarı bile üretmez. → ReviveOutcome.md#revived
        /// <summary>Hedef ayağa kalktı.</summary>
        Revived
    }
}
