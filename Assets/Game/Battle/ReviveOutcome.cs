namespace GridStrategy.Battle
{
    // ═══ ROL: TANIM (Profile) ════════════════════════════════════════
    // kimlik : yok — iki Revived değeri aynı şeydir
    // hafıza : yok — bir değer; kendisi hiçbir şey yapmaz
    // Unity  : gerekmez
    // karar  : vermez — olup biteni ADLANDIRIR; kararı BattleActions verir
    /// <summary>
    /// Bir diriltme DENEMESİNİN sonucu. "Deneme" kelimesi kasıtlı: reddedilen
    /// bir diriltme de bir sonuçtur ve çağıranın onu ayırt etmesi gerekir.
    ///
    /// NEDEN BU KATMANDA, <c>GridStrategy.Combat</c>'ta DEĞİL: diriltmenin
    /// AKIŞI burada yaşıyor. <see cref="GridStrategy.Combat.Combatant.TryRevive"/>
    /// kendi payına düşeni bir <c>bool</c> ile cevaplıyor ve doğru cevap o —
    /// o tip menzili, sırayı ve tarafı GÖRMEZ. Menzili görebilen tek yer
    /// tahtayı ve savaşı birlikte tanıyan katmandır; sonuç tipi de üreticisiyle
    /// aynı yerde yaşar. Aynı gerekçe <see cref="PlacementOutcome"/> için de
    /// geçerli.
    ///
    /// Neden <c>bool</c> değil: "dirildi mi" dört ayrı soruyu tek cevaba
    /// sıkıştırırdı ve çağıran dördüne farklı tepki verir — sırası değilse
    /// sessiz kalınır, geçersiz hedefte bir uyarı sesi çalar, menzil dışında
    /// yapay zekâ "önce yaklaş" der, diriltmede bir animasyon ve muhtemelen bir
    /// ses oynar. Bu ayrım <c>bool</c> ile yazılsaydı çağıranın içinde ikinci
    /// bir kontrol olarak yeniden doğardı — ve o kontrol akışın kurallarını
    /// kopyalardı.
    /// </summary>
    public enum ReviveOutcome
    {
        // SIFIRINCI DEĞER BİLEREK BİR RET. Gerekçesi MoveOutcome.cs ve
        // AttackOutcome.cs'te yazılı; burada TEKRARLANMIYOR, atıfta bulunuluyor.
        // Kısacası: default(ReviveOutcome) "dirildi" demek olsaydı, atanması
        // unutulan her alan sessizce bir başarı gibi okunurdu.
        /// <summary>
        /// Hedef diriltilemez: ayakta, kalıcı ölü, karşı takımda ya da taraflardan
        /// biri tarafsız (<c>TargetingRules.CanBeRevived</c>).
        /// </summary>
        RejectedInvalidTarget,

        /// <summary>Hedef diriltilebilirdi ama ulaşılamadı.</summary>
        RejectedOutOfRange,

        // ÜÇÜNCÜ DEĞER, VE ADI İKİ ENUM'DAN DAHA ÖDÜNÇ ALINDI —
        // AttackOutcome.RejectedActorCannotAct ve MoveOutcome.RejectedActorCannotAct.
        // Anlamı üçünde de tek cümle: eylemi yapan taraf şu an eylem yapamaz.
        // Farklı bir ad seçmek, çağıranı aynı cevabı üçüncü kez öğrenmek zorunda
        // bırakırdı.
        //
        // İKİ SEBEBİ BİRDEN TAŞIYOR ve ikisini de akış soruyor: sıra
        // (TurnRules.CanAct) ve diriltenin kendi durumu (ReviveRules.CanRevive).
        // İkinci kuralın kendi tipi var, ödünç ALINMADI: AttackRules.CanAttack'in
        // adı yalan söylerdi — diriltmek saldırmak değildir — ve bir kuralı
        // başka bir kuraldan türetmenin reddi hem MovementRules'ta hem
        // AttackRules'ta yazılı. Ayrımın tamamı BattleActions.Revive'ın yanında.
        /// <summary>
        /// Dirilten şu an eylem yapamaz. İki sebebi var: sıra o tarafta değil,
        /// ya da diriltenin kendisi ayakta değil
        /// (<c>ReviveRules.CanRevive</c>).
        /// </summary>
        RejectedActorCannotAct,

        // REDDEDILEN - ReviveOutcome.cs:79 yerine (bu enum hiç doğmaz,
        //              diriltme AttackOutcome'u paylaşır):
        //     AttackOutcome outcome = BattleActions.Revive(battle, reviver, target);
        //     // ve "Revived" için AttackOutcome'a altıncı bir değer eklenir
        // KIRILAN  : paylaşmanın şartı (S-13) tutmuyor — ret sebepleri ve başarı
        //            cümlesi BİREBİR aynı değil.
        //            Hit, HitAndDowned ve HitAndDestroyed diriltmede karşılıksız
        //            -> her çağıran "bu bana asla dönmez" diye üç dal yazar; ters
        //            yönde eklenen Revived saldırı switch'inde işlenmeden kalır
        //            derleyici: switch DEYİMİnde uyarı bile üretmez  .  test: yeşil
        // KAZANIRDI: diriltme bir SALDIRI çeşidi olsaydı — negatif hasar veren
        //            bir yetenek, ya da "hedefe bir şey uygula" diye tek bir
        //            akışa indirgenmiş bir yetenek sistemi; o gün S-13 birleşmeyi
        //            emrederdi.
        // TEK CUMLE: İki sonuç tipi ancak sözlükleri birebir aynıysa birleşir;
        //            değilse birleşme her çağırana asla dönmeyecek dallar yazdırır.
        /// <summary>Hedef ayağa kalktı.</summary>
        Revived
    }
}
