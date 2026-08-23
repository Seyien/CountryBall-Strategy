namespace GridStrategy.Combat
{
    // ═══ ROL: TANIM (Profile) ════════════════════════════════════════
    // kimlik : yok — iki Player değeri aynı şeydir
    // hafıza : yok — ölçüsü şu: Team.Player'ı programın iki ayrı anında oku,
    //          ikisi de aynı sabittir; bu tipte yazılacak alan yoktur. Tarafı
    //          TUTAN yer Combatant.Team ile Structure.Team ve orada da `set`
    //          yok — taraf kurulurken belli olur
    // Unity  : gerekmez
    // karar  : vermez — tarafı ADLANDIRIR; "kime saldırılır" kararı
    //          TargetingRules'a ait
    /// <summary>
    /// Bir birimin ya da yapının hangi tarafta olduğu.
    ///
    /// Bu tip <b>tek başına hiçbir kural taşımaz.</b> "Aynı takıma saldırılmaz"
    /// bir hedefleme kuralıdır ve <see cref="TargetingRules"/>'a aittir; burada
    /// yazılsaydı taraf bilgisi ile taraf kuralı aynı yerde yaşardı ve dost
    /// ateşi gibi bir mod eklemek bu enum'u değiştirmeyi gerektirirdi.
    ///
    /// GEREKÇELER: Docs/deep/kod/Core/Combat/Team.md
    /// </summary>
    public enum Team
    {
        // SIFIRINCI DEĞER, ATANMAYI UNUTULANIN ADIDIR. Sıfır BİLEREK tarafsız:
        // default(Team) "oyuncu" olsaydı, takımı atanmayı unutulmuş her birim
        // sessizce oyuncunun tarafında doğardı ve hata ancak oyunda görünürdü.
        // Koruma "None" sözcüğünden değil, o üyenin taşıdığı SAYIdan geliyor.
        // → Team.md#none
        None,

        /// <summary>Oyuncunun birimleri.</summary>
        Player,

        /// <summary>Rakip birimler. İki taraflı bir oyunda tek düşman takımı yeter.</summary>
        Enemy
    }
}
