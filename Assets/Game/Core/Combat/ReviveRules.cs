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
    /// Ailenin üçüncü ve son üyesi. Üçü birlikte EYLEYENİN durumunu
    /// sahipleniyor, her biri tek bir yetenek için:
    ///
    /// <list type="bullet">
    /// <item><see cref="MovementRules"/> — kim yürür</item>
    /// <item><see cref="AttackRules"/> — kim vurur</item>
    /// <item><c>ReviveRules</c> — kim kaldırır</item>
    /// </list>
    ///
    /// Hedefin durumu bunların hiçbirinde değil; o
    /// <see cref="TargetingRules"/>'ın işi ve orada da iki ayrı soru olarak
    /// duruyor. Yani matris iki eksenli: EYLEYEN × HEDEF, ve her hücrenin kendi
    /// sahibi var.
    ///
    /// Neyi BİLMEZ: hedefin diriltilebilir olup olmadığını
    /// (<see cref="TargetingRules.CanBeRevived(UnitState)"/>'in işi), diriltme
    /// menzilini (bugün sahipsiz), sıranın kimde olduğunu (TurnRules'ın işi,
    /// bir üst katmanda).
    ///
    /// GEREKÇELER: Docs/deep/kod/Core/Combat/ReviveRules.md
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
        // EYLEYEN İLE HEDEF İKİ AYRI EKSENDİR. İkinci bir `targetState`
        // parametresi reddedildi: o satır TargetingRules.CanBeRevived'ın birebir
        // kopyası olurdu ve matrisin iki hücresi tek metoda çökerdi. Bu tip yalnız
        // DURUM sorar; menzil bugün sahipsiz, sıra TurnRules'ın.
        // → ReviveRules.md#canreviveunitstate-reviverstate
        public static bool CanRevive(UnitState reviverState)
        {
            return reviverState == UnitState.Alive;
        }
    }
}
