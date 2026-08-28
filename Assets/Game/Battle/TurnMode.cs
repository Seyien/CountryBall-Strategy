namespace GridStrategy.Battle
{
    // ═══ ROL: TANIM (Profile) ════════════════════════════════════════
    // kimlik : yok — iki FreeForAll değeri aynı şeydir
    // hafıza : yok — bir savaşın kipi kurulurken belli olur ve TurnState.Mode
    //          içinde `set` yoktur; bu enum yalnızca o kipi ADLANDIRIR
    // Unity  : gerekmez — noEngineReferences: true
    // karar  : vermez — "sırası olmayan eyleyebilir mi" kararı
    //          TurnState.AllowsAction ile TurnRules arasında paylaşılır
    /// <summary>
    /// Bir savaşın sıra KİPİ: sıranın kimde olduğu bir kapı mıdır, yoksa
    /// yalnızca bir gösterge midir.
    ///
    /// OYUNDA NE İŞE YARAR: oyuncu paletten hem kendi hem düşman birimlerini
    /// koyduğu bir kum havuzunda oynuyor ve orada "sıra sende değil" cümlesi
    /// ona yalnızca "tıklıyorum, hiçbir şey olmuyor" olarak görünüyordu.
    /// <see cref="FreeForAll"/> o kapıyı açar; kampanya savaşları
    /// <see cref="Alternating"/> ile aynen eskisi gibi oynanır.
    ///
    /// Bu tip <b>tek başına hiçbir kural taşımaz.</b> "Sırası olmayan eyleyemez"
    /// cümlesi bir kuraldır ve <see cref="TurnRules"/>'a aittir; buradaki iki
    /// üye yalnızca o kuralın SORULUP sorulmayacağını adlandırır.
    /// </summary>
    public enum TurnMode
    {
        // SIFIRINCI DEĞER BUGÜNKÜ DAVRANIŞTIR, ve bu bir tercih değil bir
        // zorunluluk: default(TurnMode) kipi atanmayı unutulmuş her savaşın
        // kipidir. Sıfır FreeForAll olsaydı, kipi hiç yazmayan bir çağıran sıra
        // kapısını sessizce kaldırmış olurdu ve kampanya savaşı kum havuzuna
        // dönerdi — hiçbir derleme hatası çıkmadan.
        /// <summary>
        /// Sıra kapıdır: yalnızca sırası gelen taraf saldırır, yürür, diriltir
        /// ve her eylem sırayı devreder. Bugünkü kampanya davranışı budur.
        /// </summary>
        Alternating = 0,

        /// <summary>
        /// Sıra yalnızca bir göstergedir: tarafsız olmayan her takım her an
        /// eyleyebilir ve hiçbir eylem sırayı devretmez.
        ///
        /// <see cref="Combat.Team.None"/> burada da eyleyemez — tarafsız olan
        /// taraf tutmaz, duvar vurmaz — çünkü bu kipin kaldırdığı tek şey SIRA
        /// kapısıdır, tarafsızlık kapısı değil.
        /// </summary>
        FreeForAll = 1
    }
}
