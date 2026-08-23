namespace GridStrategy.Combat
{
    // ═══ ROL: TANIM (Profile) ════════════════════════════════════════
    // kimlik : yok — iki Hit değeri aynı şeydir
    // hafıza : yok — ölçüsü şu: dönen Hit değerini bir değişkende sakla,
    //          araya yüz saldırı daha gir, sonra tekrar oku — değer aynı.
    //          Enum bir sabittir; onu değiştirecek bir metot yazılamaz
    // Unity  : gerekmez — enum değerini okumak için ne sahne ne kare gerekir
    // karar  : vermez — olup biteni ADLANDIRIR; kararı AttackAction verir
    /// <summary>
    /// Bir saldırı DENEMESİNİN sonucu. "Deneme" kelimesi kasıtlı: reddedilen bir
    /// saldırı da bir sonuçtur ve çağıranın onu ayırt etmesi gerekir.
    ///
    /// HEDEF TİPİNDEN BAĞIMSIZ: aynı enum hem <see cref="Combatant"/>'a hem
    /// <see cref="Structure"/>'a yapılan saldırıyı adlandırır; ayrışan tek şey
    /// ölüm olayının adıdır (<see cref="HitAndDowned"/> ↔
    /// <see cref="HitAndDestroyed"/>).
    ///
    /// GEREKÇELER: Docs/deep/kod/Core/Combat/AttackOutcome.md
    /// </summary>
    // ENUM, STRUCT DEĞİL: "hangi durum" sorusunu TİPE sordurur. Rejected /
    // DamageDealt / Downed alanlarını taşıyan bir struct'ta alanların ikisi her
    // çağrıda anlamsız kalır ve eksik switch dalı derleyiciden de görünmez olur.
    // `bool` ise üç ayrı soruyu tek cevaba sıkıştırırdı.
    // → AttackOutcome.md#attackoutcome-tip
    // DERİN ANLATIM: Docs/deep/konular/06-sonuc-enumlari.md
    public enum AttackOutcome
    {
        // SIFIRINCI DEĞER BİLEREK BİR RET. `default(AttackOutcome)`, sıfırlanmış
        // dizi hücreleri ve atanmayı unutulan alanlar hep buraya düşer; burada
        // "Hit" dursaydı atanmamış değer BAŞARILI bir saldırı gibi okunurdu.
        // Yeni değerler bu yüzden SONA eklenir, araya değil.
        // → AttackOutcome.md#rejectedinvalidtarget
        RejectedInvalidTarget,

        /// <summary>Menzil dışı. Hedef geçerliydi ama ulaşılamadı.</summary>
        // RejectedInvalidTarget ile tek bir `Rejected` altında BİRLEŞTİRİLMEDİ:
        // çağıran birinde YAKLAŞIR, öbüründe HEDEF DEĞİŞTİRİR.
        // → AttackOutcome.md#rejectedoutofrange
        RejectedOutOfRange,

        /// <summary>Hasar uygulandı; hedef hâlâ ayakta.</summary>
        // İki hedef tipinde de AYNI cümle; ayrışan tek çift HitAndDowned ↔
        // HitAndDestroyed.
        // → AttackOutcome.md#hit
        Hit,

        /// <summary>Hasar uygulandı ve hedef bu vuruşla düştü.</summary>
        // "Bu vuruşla" taşıyıcı bir ifade: değer bir DURUM değil bir GEÇİŞ
        // adlandırır — zaten düşmüş hedefe vuruş Hit döner, HitAndDowned değil.
        // → AttackOutcome.md#hitanddowned
        HitAndDowned,

        /// <summary>Hasar uygulandı ve YAPI bu vuruşla yıkıldı.</summary>
        // BEŞİNCİ DEĞER, AYRI BİR ENUM DEĞİL: bir baraka düşmez, yıkılır ve
        // HitAndDowned'ı yeniden kullanmak iki olguyu ayırt edilemez kılardı. İki
        // ayrı enum açılsaydı her tüketici paralel bir switch taşır, ikizler
        // zamanla ayrışır ve tek `default: LogError` koruması ikiye bölünürdü.
        // → AttackOutcome.md#hitanddestroyed
        // DERİN ANLATIM: Docs/deep/konular/06-sonuc-enumlari.md
        HitAndDestroyed,

        /// <summary>
        /// Saldıran şu an saldıramaz: durumu elvermiyor
        /// (<see cref="AttackRules.CanAttack"/>) ya da sırası değil
        /// (bu ikincisini yalnızca <c>BattleActions</c> üretir).
        /// </summary>
        // ALTINCI DEĞER, SONA EKLENDİ: ret ailesinin yanına sokulsaydı aradaki üç
        // değer sessizce yeniden numaralanırdı. Üç sebebi (saldıran düşmüş,
        // hareket eden düşmüş, sırası değil) BİLEREK tek değerde topluyor; ayrım
        // ancak çağıranın dallanması değiştiği gün doğar.
        // → AttackOutcome.md#rejectedactorcannotact
        RejectedActorCannotAct
    }
}
