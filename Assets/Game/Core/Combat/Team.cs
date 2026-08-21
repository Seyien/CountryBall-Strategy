namespace GridStrategy.Combat
{
    // ═══ ROL: TANIM (Profile) ════════════════════════════════════════
    // kimlik : yok — iki Player değeri aynı şeydir
    // hafıza : yok — bir değer
    // Unity  : gerekmez
    // karar  : vermez — tarafı ADLANDIRIR; "kime saldırılır" kararı
    //          TargetingRules'a ait
    /// <summary>
    /// Bir birimin ya da yapının hangi tarafta olduğu.
    ///
    /// Bu tip <b>tek başına hiçbir kural taşımaz.</b> "Aynı takıma saldırılmaz"
    /// bir hedefleme kuralıdır ve <see cref="TargetingRules"/>'a aittir; burada
    /// yazılsaydı taraf bilgisi ile taraf kuralı aynı yerde yaşardı ve dost ateşi
    /// gibi bir mod eklemek bu enum'u değiştirmeyi gerektirirdi.
    /// </summary>
    public enum Team
    {
        // Sıfır BİLEREK tarafsız. default(Team) "oyuncu" olsaydı, takımı
        // atanmayı unutulmuş her birim sessizce oyuncunun tarafında doğardı
        // ve bu hata ancak oyunda, yanlış birime saldırılamadığında görünürdü.
        //
        // REDDEDILEN - Team.cs:35 yerine:
        //     Player,
        //     Enemy
        // KIRILAN  : default(Team) "oyuncu" olur; takımı atanmayı unutulan her şey
        //            sessizce oyuncunun tarafında doğar.
        //            tarafsız hiçbir şey ifade edilemez -> duvar, kaynak düğümü, tuzak
        //            bir tarafa yazılır, TargetingRules'a "şu tip hariç" istisnası girer
        //            derleyici: hiçbir şey der  .  test: hata ancak oyunda görülür
        // KAZANIRDI: oyunda tarafsız hiçbir şey olmayacaksa — o gün üçüncü
        //            değer ölü kod olur ve her switch'te anlamsız bir dal açar.
        // TEK CUMLE: Sıfırıncı değer, atanmayı unutulanın adıdır; en zararsız anlam
        //            ona verilir — ve zararsız olan, hiçbir tarafta olmamaktır.
        None,

        /// <summary>Oyuncunun birimleri.</summary>
        Player,

        /// <summary>Rakip birimler. İki taraflı bir oyunda tek düşman takımı yeter.</summary>
        Enemy
    }
}
