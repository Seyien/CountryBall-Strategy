using GridStrategy.Core;

namespace GridStrategy.Unity
{
    /// <summary>
    /// Düşmüş bir dostun yanına varınca onu kaldıran, TEK SEFERLİK emir.
    ///
    /// OYUNDA NE İŞE YARAR: uzaktaki düşmüş dosta tıklamak "yanına git ve
    /// kaldır" demektir; oyuncunun varışı gözleyip ikinci kez tıklaması gerekmez.
    /// </summary>
    // ██ BU TİP BİR BAYRAĞIN YERİNE GEÇTİ ██
    // Eski hâlde emrin CİNSİ tahtada `pendingStrikeIsRevive` adlı tek bir bool
    // idi ve o bool'un var olma sebebi yazılıydı: "ikinci bir bekleyen-emir
    // KİPİ açmamanın bedeli tam olarak bu tek bool". Emir bir nesne olunca o
    // bedel sıfırlandı — ikinci cins, ikinci sınıf demek; defter ikisini de
    // aynı gözden okuyor. Command pattern'in bu turda kazandırdığı ikinci şey
    // budur ve birincisinden (çoğulluk) bağımsızdır.
    //
    // TEK SEFERLİK, saldırının tersine: kaldırılan dost ayağa kalkar ve
    // tekrarlanacak bir iş kalmaz. Kalıcı olsaydı ayakta duran bir dosta her
    // karede reddedilen bir kaldırma denemesi yapılırdı.
    public sealed class ReviveOrder : IUnitOrder
    {
        private readonly IUnitOrderHost host;
        private readonly Unit reviver;
        private readonly Unit target;

        /// <summary>
        /// Emri kurar.
        /// </summary>
        /// <param name="orderHost">Tahtaya bakan pencere.</param>
        /// <param name="orderReviver">Kaldırmaya giden savaşçı.</param>
        /// <param name="orderTarget">Düşmüş dost.</param>
        public ReviveOrder(IUnitOrderHost orderHost, Unit orderReviver, Unit orderTarget)
        {
            host = orderHost;
            reviver = orderReviver;
            target = orderTarget;
        }

        /// <inheritdoc />
        public Unit Target => target;

        /// <inheritdoc />
        public OrderProgress Advance()
        {
            // Gerekçe AttackOrder'da bir kez yazılı ve burada tekrar edilmiyor:
            // konumu olmayan bir kimliğe eylem çağrısı istisna üretir.
            if (!host.TryGetCell(reviver, out _, out _) || !host.TryGetCell(target, out _, out _))
            {
                return OrderProgress.Cancelled;
            }

            if (host.IsViewWalking(reviver))
            {
                return OrderProgress.Continue;
            }

            host.Revive(reviver, target);

            // SONUÇ SORULMUYOR ve bu bir karar: kaldırma reddedilse bile
            // (kurtarma penceresi dolmuş, dost çoktan ayakta) emir BİTMİŞTİR —
            // bekleyerek düzelecek bir şey yok. Cevabı oyuncuya ReactToRevive
            // yazıyor.
            return OrderProgress.Finished;
        }

        /// <inheritdoc />
        public string Describe()
        {
            return $"reviving '{target.Name}'";
        }
    }
}
