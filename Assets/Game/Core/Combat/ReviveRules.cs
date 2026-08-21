namespace GridStrategy.Combat
{
    // ═══ ROL: KURAL (Policy) ═════════════════════════════════════════
    // kimlik : yok — static; iki çağrıyı ayıracak bir şey yoktur
    // hafıza : yok — aynı durum her zaman aynı cevabı verir
    // Unity  : gerekmez
    // karar  : UYGUNLUK söyler; ne diriltir ne can yazar
    /// <summary>
    /// "Bu birim başkasını diriltebilir mi?" sorusunun tek sahibi.
    ///
    /// Ailenin üçüncü ve son üyesi. Üçü birlikte EYLEYENİN durumunu sahipleniyor,
    /// her biri tek bir yetenek için:
    ///
    /// <list type="bullet">
    /// <item><see cref="MovementRules"/> — kim yürür</item>
    /// <item><see cref="AttackRules"/> — kim vurur</item>
    /// <item><c>ReviveRules</c> — kim kaldırır</item>
    /// </list>
    ///
    /// Hedefin durumu bunların hiçbirinde değil; o <see cref="TargetingRules"/>'ın
    /// işi ve orada da iki ayrı soru olarak duruyor (<c>CanBeAttacked</c> ile
    /// <c>CanBeRevived</c>). Yani matris iki eksenli: EYLEYEN × HEDEF, ve her
    /// hücrenin kendi sahibi var.
    ///
    /// Bu tip doğana kadar düşmüş bir birim yerdeki arkadaşını ayağa
    /// kaldırabiliyordu. Boşluk gizlenmemişti: <c>BattleActions.Revive</c> onu
    /// adı konmuş bir EŞİK olarak taşıyordu ve sabitleyen test
    /// (<c>Revive_DownedReviver_StillRevives_BecauseNoRuleOwnsReviverState</c>
    /// (SILINDI) — aramaya kalkma, o test artık yok ve olmaması doğrudur)
    /// kural yazıldığı gün kırmızıya dönmek üzere yazılmıştı. Bugün o gün;
    /// test silindi ve yerine kararı koruyan
    /// <c>Revive_DownedReviver_IsRejected</c> geldi.
    ///
    /// ÖLÇÜLMÜŞ VE DÜRÜSTÇE YAZILIYOR: bugün üç kural da aynı satırı taşıyor
    /// (<c>state == Alive</c>) ve birini diğerinden türetmek üç durumda da doğru
    /// cevap verir; bu bir tesadüftür, bir tasarım değil. Üçünü BİRDEN kayda geçiren
    /// tek test bu tipin kendi dosyasındadır —
    /// <c>ReviveRulesTests.ThreeActorRules_StillAgree_WhichIsWhyTheyMustStaySeparate</c>;
    /// <see cref="AttackRules"/> tarafındaki karakterizasyon testi yalnızca İKİLİ
    /// sürümü taşır. Bedel bugün üç dosya, kazanç ayrıldıkları gün hiçbir çağıranın değişmemesi.
    ///
    /// Ayrılacakları gün de tahmin değil, oyunun kendi diliyle adlandırılabilir:
    /// yaralı bir sıhhiyeci vuramadığı hâlde arkadaşını kaldırabilmelidir. O gün
    /// <c>ReviveRules</c> ile <see cref="AttackRules"/> ayrışır ve türetme
    /// yazmış olan proje bunu HİÇBİR TEST KIRILMADAN kaçırır.
    ///
    /// Neyi BİLMEZ: hedefin diriltilebilir olup olmadığını
    /// (<see cref="TargetingRules.CanBeRevived(UnitState)"/>'in işi), diriltme
    /// menzilini (bugün sahipsiz — <c>BattleActions.Revive</c> saldırı menzilini
    /// ödünç alıyor ve o taviz orada yazılı), sıranın kimde olduğunu
    /// (TurnRules'ın işi, bir üst katmanda).
    /// </summary>
    public static class ReviveRules
    {
        /// <summary>
        /// Diriltebilmenin tek koşulu: eyleyenin kendisi ayakta olmalı.
        ///
        /// Beyaz liste — <c>== Alive</c>, <c>!= Downed &amp;&amp; != Dead</c> değil.
        /// Gerekçe <see cref="MovementRules"/>'ta yazılı ve burada tekrar
        /// edilmiyor: kara liste, yeni bir <see cref="UnitState"/> değerini
        /// varsayılan olarak YETKİLİ kılar ve bunu hiçbir derleme hatası
        /// göstermez.
        /// </summary>
        // REDDEDILEN - ReviveRules.cs:81 yerine (düşmüş birim kendini
        //              diriltemesin diye hedef de sorulur):
        //     public static bool CanRevive(UnitState reviverState, UnitState targetState)
        //     {
        //         if (reviverState != UnitState.Alive) { return false; }
        //         return targetState == UnitState.Downed;
        //     }
        // KIRILAN  : ikinci satır TargetingRules.CanBeRevived'ın birebir kopyasıdır
        //            ve o kural HEDEF eksenine ait; matrisin iki hücresi tek
        //            metoda çöker.
        //            diriltilebilir durumlar kümesi değişir -> orası güncellenir,
        //            burası kalır ve akış hedef kuralı EVET derken reddeder
        //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
        // KAZANIRDI: eyleyen ile hedefin BİRLİKTE anlam kazandığı bir kural olsaydı
        //            — "kendini dirilteme". O gün sorulan durum değil KİMLİK olurdu.
        // TEK CUMLE: Eyleyenin durumu ile hedefin durumu iki ayrı eksendir; tek
        //            metot ikisini birden sahiplenemez.
        public static bool CanRevive(UnitState reviverState)
        {
            return reviverState == UnitState.Alive;
        }
    }
}
