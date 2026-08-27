namespace GridStrategy.Core
{
    // ═══ ROL: TANIM (Profile) ════════════════════════════════════════
    // kimlik : yok — iki Moved değeri aynı şeydir
    // hafıza : yok — ölçüsü şu: MoveAction.Execute'un döndürdüğü Moved'ı bir
    //          değişkene al, sonra tahtayı istediğin kadar değiştir — birimi
    //          geri taşı, hücreyi doldur, başka birim yerleştir — değişken
    //          hâlâ Moved'dır. Değişen tek şey BİR SONRAKİ Execute çağrısının
    //          döndüreceği değerdir
    // Unity  : gerekmez
    // karar  : vermez — olup biteni ADLANDIRIR; kararı MoveAction verir
    /// <summary>
    /// Bir hareket DENEMESİNİN sonucu. "Deneme" kelimesi kasıtlı: reddedilen
    /// bir hareket de bir sonuçtur ve çağıranın onu ayırt etmesi gerekir.
    ///
    /// Neden <c>bool</c> değil: "taşındı mı" tek cevaba üç ayrı soruyu
    /// sıkıştırırdı ve çağıran üçüne farklı tepki verir — tahta dışı bir
    /// tıklama sessizce yutulur, dolu hücre uyarı ister, menzil dışı ise yol
    /// bulucuya "önce yaklaş" der. <c>bool</c> ile yazılsaydı bu ayrım
    /// çağıranın içinde MoveAction'ın kurallarını kopyalayan ikinci bir
    /// kontrol olarak yeniden doğardı.
    ///
    /// GEREKÇELER: Docs/deep/kod/Core/MoveOutcome.md
    /// </summary>
    // DERİN ANLATIM: Docs/deep/konular/02-assembly-duvari.md + Docs/deep/konular/06-sonuc-enumlari.md
    public enum MoveOutcome
    {
        // SIFIRINCI DEĞER BİLEREK BİR RET DEĞERİ. Bu enum'da tek bir "= 0"
        // yazılı değil; numarayı satır SIRASI belirler. Sıfır, dilin atanmamış
        // her alana verdiği değerdir — Moved başa alınsaydı hiç hareket
        // denenmeden okunan bir alan "taşındı" derdi ve derleyici susardı.
        // → MoveOutcome.md#rejectedinvaliddestination
        RejectedInvalidDestination,

        // ÜÇ RET SEBEBİ TEK "Rejected" DEĞERİNE İNDİRİLEBİLİRDİ, İNDİRİLMEDİ.
        // Ayıran ölçüt sebep sayısı değil DAVRANIŞ sayısı: bu sebep bir tur
        // sonra DEĞİŞEBİLİR ("bekle, hücre boşalır"), geçersiz hedef ise ASLA
        // değişmez ("bir daha hiç deneme"). Tek değer bu çizgiyi siler.
        // → MoveOutcome.md#rejectedcelloccupied
        /// <summary>Hedef hücrede başka bir birim duruyor.</summary>
        // DERİN ANLATIM: Docs/deep/konular/04-karar-sirasi.md
        RejectedCellOccupied,

        /// <summary>
        /// Hedef hücre tahtada ve boş, ama bu turda ulaşılamıyor. Bir tur
        /// sonra değişebilir; çağıranın işi "önce yaklaş".
        /// → MoveOutcome.md#rejectedoutofrange
        /// </summary>
        RejectedOutOfRange,

        /// <summary>
        /// Birim tahtada eski hücresinden yeni hücresine geçti. Enum'un tek
        /// KABUL değeri; sıfırıncı sırada bilerek DEĞİL.
        /// → MoveOutcome.md#moved
        /// </summary>
        Moved,

        // BEŞİNCİ DEĞER — VE SAHİBİ ONU ÜRETEMEZ. Cevabı MovementRules'ta ve
        // o tip Core'dan görünmüyor; bu değeri döndürebilen tek yer Battle
        // katmanındaki BattleActions'tır. Bilerek verilmiş bir taviz: tipe,
        // sahibinin asla üretemeyeceği bir değer eklendi ve gizlenmedi.
        // → MoveOutcome.md#rejectedactorcannotact
        /// <summary>
        /// Hareket eden şu an eylem yapamaz: sırası değil ya da durumu
        /// elvermiyor (<c>MovementRules.CanMove</c> — bu tipin GÖREMEDİĞİ bir
        /// kural). Bu değeri yalnızca <c>GridStrategy.Battle</c> katmanı üretir.
        /// </summary>
        RejectedActorCannotAct,

        /// <summary>
        /// Hedef hücre tahtada ve boş, ama oraya YÜRÜNEBİLECEK bir yol yok —
        /// birimlerle ya da tahta kenarıyla çevrili.
        ///
        /// Oyuncu tarafı: tıkladığı yere gidilemeyeceğini söyleyen mesaj budur.
        /// <see cref="RejectedOutOfRange"/> ile karıştırılmamalı; o "menzil"
        /// kuralının cevabıydı ve tahta artık menzil sormuyor, ULAŞILABİLİRLİK
        /// soruyor. Yolu <see cref="PathFinder"/> arar.
        /// </summary>
        RejectedUnreachable
    }
}
