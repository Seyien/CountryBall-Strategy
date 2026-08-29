using GridStrategy.Combat;
using GridStrategy.Core;

namespace GridStrategy.Unity
{
    //   ═══ KARŞILIK EMRİ — YERİNDE DURAN ══════════════════════════════
    //
    //     karşılık veren VE saldıran tahtada mı ──hayır──► İPTAL
    //              │evet
    //           VUR  ─────► sonucu OKU
    //              │
    //     ┌────────┴───────────────────────────────┐
    //     isabet / OnCooldown / OutOfRange     InvalidTarget
    //          ──► DEVAM (bekler)              ActorCannotAct
    //                                              ──► İPTAL
    //
    //   İKİZİYLE TEK FARK: burada YAKLAŞMA ADIMI YOK. Bir taret
    //   karşılık verebilir ama yerinden kımıldayamaz, ve tahtada onu
    //   taşıyacak hiçbir üye yok.

    /// <summary>
    /// Vurulan YAPININ kendiliğinden aldığı karşılık emri: saldırgan menzilde
    /// ise vurur, değilse yerinde bekler.
    ///
    /// OYUNDA NE İŞE YARAR: taretine ateş eden savaşçı artık cezasız kalmıyor —
    /// taret ona döner ve menziline girdiği an vurur. Yürümez, çünkü bir bina
    /// yürüyemez.
    /// </summary>
    // ██ İKİNCİ UYGULAMA OLMASININ SEBEBİ BİR EKSİK ÜYE ██
    // Structure kendi saldırı tanımını taşıyabiliyor (CanAttack) ama tahtada
    // onu taşıyacak hiçbir çağrı yok: BattleActions.Move yapı kimliği için
    // "The unit is not in this battle" istisnası atar. Yani yaklaşma adımı bu
    // tipte bir tercihle atlanmıyor, çağrılamıyor.
    //
    // IsViewWalking KORUMASI BİLEREK YOK: ikizinde o satır tahtanın anında
    // taşıdığı birimin görselini beklerken, burada beklenecek bir yürüyüş
    // hiç doğmuyor. Konsaydı hiçbir zaman true dönmeyen bir soru olurdu ve
    // "yapı yürür mü" sorusunun ikinci bir cevap yeri açılırdı.
    //
    // ██ EKRANDA GÖRÜLECEK ETKİ SINIRLI, VE BU YAZILIYOR ██
    // Taretin ateşinin asıl sahibi hâlâ BoardAdapter.AdvanceStructureFire ve o,
    // Update içinde bu defterden ÖNCE koşuyor; bekleme sayacının sahibi ise
    // ikisinin de altındaki Core. Yani menzilde başka bir düşman varken bu emrin
    // vuruşu çoğu pencerede RejectedOnCooldown alır. Gözlenebilir katkısı tek
    // hâlde doğuyor: saldırgan taretin en yakın düşmanı DEĞİLKEN.
    //
    // REDDEDILEN - yapıyı karşılık verenlerin dışında bırakmak.
    //     if (!battle.TryGetCombatant(defender, out Combatant _))
    //     {
    //         return;
    //     }
    // KIRILAN: silahlı bir taret vurulduğunda hiçbir şey yapmazdı ve seçim
    // noktası tek uygulamaya inerdi — Strategy'nin ölçüsü olan "iki uygulama,
    // aynı sözleşme" düşerdi.
    // KAZANIRDI: yapıların hiçbiri AttackProfile taşımasaydı; o gün karşılık
    // veren yapı diye bir şey olmaz ve bu tip gereksiz kalırdı.
    // TEK CUMLE: birim kovalar, yapı yerinde vurur.
    //
    // Command ailesinin dördüncü üyesi; ikizi ChaseAndStrikeOrder ile arasındaki
    // seçim Strategy'dir ve seçen satır BoardAdapter.WriteRetaliation içinde.
    // DERİN ANLATIM: Docs/deep/konular/11-karsilik-verme-ve-menzil.md
    public sealed class StandAndStrikeOrder : IUnitOrder
    {
        private readonly IUnitOrderHost host;
        private readonly Unit defender;
        private readonly Unit aggressor;

        /// <summary>
        /// Emri kurar.
        /// </summary>
        /// <param name="orderHost">Tahtaya bakan pencere.</param>
        /// <param name="orderDefender">Vurulan ve karşılık verecek yapı.</param>
        /// <param name="orderAggressor">Vuran taraf; vurulacak kimlik.</param>
        public StandAndStrikeOrder(IUnitOrderHost orderHost, Unit orderDefender, Unit orderAggressor)
        {
            host = orderHost;
            defender = orderDefender;
            aggressor = orderAggressor;
        }

        /// <inheritdoc />
        public Unit Target => aggressor;

        /// <inheritdoc />
        public OrderProgress Advance()
        {
            if (!host.TryGetCell(defender, out _, out _) || !host.TryGetCell(aggressor, out _, out _))
            {
                return OrderProgress.Cancelled;
            }

            AttackOutcome outcome = host.Strike(defender, aggressor);

            switch (outcome)
            {
                // ██ "MENZİLDEYSEM VUR, DEĞİLSEM BEKLE"NİN İKİNCİ YARISI ██
                // Yapı yürüyemediği için menzil dışı olmak burada bir DURUM
                // değil bir bekleyiş: saldırgan yaklaştığı an aynı emir vurmaya
                // başlar. İptal edilseydi taret, kendisine uzaktan atış yapan
                // düşman ilerlediğinde onu unutmuş olurdu.
                case AttackOutcome.RejectedOutOfRange:

                // Bekleme kuralı burada da İKİNCİ kez yazılmıyor; sayacın sahibi
                // Structure'ın kendisi ve AttackAction o cevabı zaten üretiyor.
                case AttackOutcome.RejectedOnCooldown:

                // KALICILIK: isabet emri BİTİRMEZ, ikizindeki gerekçenin aynısı.
                case AttackOutcome.Hit:
                case AttackOutcome.HitAndDowned:
                case AttackOutcome.HitAndFinished:
                case AttackOutcome.HitAndDestroyed:
                    return OrderProgress.Continue;

                // Saldırgan artık geçerli hedef değil, ya da yapının kendisi
                // yıkıldı. Beklemekle düzelmez.
                case AttackOutcome.RejectedInvalidTarget:
                case AttackOutcome.RejectedActorCannotAct:
                    return OrderProgress.Cancelled;

                // default İPTAL: gerekçesi ikizinde yazılı.
                default:
                    return OrderProgress.Cancelled;
            }
        }

        /// <inheritdoc />
        public string Describe()
        {
            return $"returning fire at '{aggressor.Name}'";
        }
    }
}
