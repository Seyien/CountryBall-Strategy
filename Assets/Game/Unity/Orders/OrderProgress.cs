namespace GridStrategy.Unity
{
    /// <summary>
    /// Bir emrin bu kareden sonraki hâli.
    ///
    /// OYUNDA NE İŞE YARAR: saldırı emri verilen savaşçı, oyuncu onu yeniden
    /// yönlendirene kadar vurmaya devam eder; hedefi kaçarsa emir düşer. Bu üç
    /// değer o farkın kendisidir.
    /// </summary>
    // ÜÇ DEĞER, İKİ DEĞİL — ve ayrımın bugün çağıranı var: <c>AttackOrder</c>
    // hiçbir zaman Finished dönmez (kalıcı emrin kendiliğinden biteceği bir an
    // yoktur), <c>ReviveOrder</c> ise hiçbir zaman "hedef kaçtı" demez. İki
    // değere indirilseydi "kaldırma tamamlandı" ile "hedef elden kaçtı" aynı
    // cevabın arkasına düşer ve operatörün istediği kesilme davranışı bir
    // testle ayırt edilemez olurdu.
    //
    // → Docs/deep/konular/09-kararlarin-cevrilmesi.md (madde 2)
    public enum OrderProgress
    {
        /// <summary>Emir ayakta; bir sonraki karede yine sorulacak.</summary>
        Continue,

        /// <summary>Emir kendi işini bitirdi ve defterden düşer.</summary>
        Finished,

        /// <summary>Dünya emrin altından kaydı: hedef kaçtı, düştü ya da tahtadan kalktı.</summary>
        Cancelled,
    }
}
