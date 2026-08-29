using GridStrategy.Combat;
using GridStrategy.Core;

namespace GridStrategy.Unity
{
    //   ═══ KARŞILIK EMRİ — KOVALAYAN ══════════════════════════════════
    //
    //     karşılık veren VE saldıran tahtada mı ──hayır──► İPTAL
    //              │evet
    //     karşılık verenin görseli yürüyor mu ──evet──► DEVAM (henüz varmadı)
    //              │hayır
    //           MENZİLE GİR  ─────► cevabı OKU, kendi kuralını YAZMA
    //              │
    //     ┌────────┼──────────────────┬─────────────────────┐
    //  AlreadyInRange              MoveTo            RejectedOffBoard
    //     │                           │              RejectedUnreachable
    //    VUR                       DEVAM                    │
    //                                                     İPTAL
    //
    //   AttackOrder'DAN TEK GÖZLENEBİLİR FARK: orada "menzil dışı"
    //   emri DÜŞÜRÜR, burada YÜRÜTÜR. Aynı olgu, iki zıt cevap — ve
    //   fark bir hata değil emrin CİNSİ.

    /// <summary>
    /// Vurulan savaşçının kendiliğinden aldığı karşılık emri: saldırganı kendi
    /// menziline girene kadar KOVALAR, girdiği an vurur.
    ///
    /// OYUNDA NE İŞE YARAR: operatörün bildirdiği eksik tam olarak buydu — üç
    /// hücre öteden vurulan kılıçlı savaşçı seyirci kalıyordu. Artık saldırganın
    /// yanına yürüyüp karşılık veriyor; okçu aynı emri alır ama üç hücre ötede
    /// durup atar.
    /// </summary>
    // ██ MENZİL DIŞINDA OLMAK BU EMRİ DÜŞÜRMEZ, ÇÜNKÜ BÜTÜN MESELE O ██
    // Emrin var olma sebebi menzil dışında olmak; onu bir düşme sebebi yapmak
    // emri doğduğu karede öldürürdü. Emir yalnız üç durumda düşer — saldıran
    // öldü, saldıran tahtadan gitti, saldırgana yol yok — ve üçünün ortak ölçüsü
    // AttackOrder'da yazılı: beklemekle düzelmeyen bir ret, sonsuza kadar
    // tekrarlanan bir rettir.
    //
    // ADIM SAYISINA DAYALI TASMA YOK ve gerekçesi bir ölçüm: tahta 3x5, yani 15
    // hücre. "En fazla 8 adım kovala" gibi bir eşiğin tahtadan türetilmiş hiçbir
    // dayanağı olmazdı ve uydurulmuş bir eşik en kötü cinsten eşiktir — orada
    // durduğu için doğru sanılır. Yeniden açma: tahta kenarı 30'u aştığı gün, ya
    // da operatör "birimim kovalarken savunmasını terk etti" dediği gün.
    //
    // REDDEDILEN - AttackOrder'a bir kovalama bayrağı eklemek.
    //     public AttackOrder(IUnitOrderHost h, Unit a, Unit t, bool chases)
    //     case AttackOutcome.RejectedOutOfRange:
    //         return chases ? OrderProgress.Continue : OrderProgress.Cancelled;
    // KIRILAN: o dal operatörün YAZILI kararını taşıyor — oyuncunun elle verdiği
    // emir menzilden çıkınca ölmeli, yoksa birim "hiç istemediği hâlde tahtanın
    // öteki ucuna" yürür. Bayrak iki davranışı tek tipe katlar ve o kararı siler.
    // KAZANIRDI: iki emrin kare döngüsü baştan sona AYNI olsaydı ve fark
    // gerçekten tek bir cevapta kalsaydı; oysa bu tip yaklaşmayı da soruyor.
    // TEK CUMLE: oyuncunun verdiği emir menzilden çıkınca ölür, birimin kendi
    // aldığı karşılık emri menzile YÜRÜR.
    //
    // Command ailesinin üçüncü üyesi; ikizi StandAndStrikeOrder ile arasındaki
    // seçim Strategy'dir ve seçen satır BoardAdapter.WriteRetaliation içinde.
    // DERİN ANLATIM: Docs/deep/konular/11-karsilik-verme-ve-menzil.md
    public sealed class ChaseAndStrikeOrder : IUnitOrder
    {
        private readonly IUnitOrderHost host;
        private readonly Unit defender;
        private readonly Unit aggressor;

        /// <summary>
        /// Emri kurar.
        /// </summary>
        /// <param name="orderHost">Tahtaya bakan pencere.</param>
        /// <param name="orderDefender">Vurulan ve karşılık verecek savaşçı.</param>
        /// <param name="orderAggressor">Vuran taraf; kovalanacak ve vurulacak kimlik.</param>
        public ChaseAndStrikeOrder(IUnitOrderHost orderHost, Unit orderDefender, Unit orderAggressor)
        {
            host = orderHost;
            defender = orderDefender;
            aggressor = orderAggressor;
        }

        /// <inheritdoc />
        // HEDEF SALDIRGANDIR: saldırgan tahtadan kalktığında bu emri süpüren
        // UnitOrderBook.CancelTargeting cevabını buradan okuyor.
        public Unit Target => aggressor;

        /// <inheritdoc />
        public OrderProgress Advance()
        {
            // Gerekçe AttackOrder'da bir kez yazılı ve burada tekrar edilmiyor:
            // konumu olmayan bir kimliğe eylem çağrısı istisna üretir.
            if (!host.TryGetCell(defender, out _, out _) || !host.TryGetCell(aggressor, out _, out _))
            {
                return OrderProgress.Cancelled;
            }

            // ██ YÜRÜRKEN VURULMAZ, VE SORU YAKLAŞMADAN ÖNCE ██
            // Tahta hareketi ANINDA işliyor, görsel gecikmeli takip ediyor.
            // Aşağıdaki yaklaşma sorusu bu satırın üstüne alınsaydı, tahtada
            // çoktan menzile girmiş ama görseli hâlâ yolun ortasında olan
            // savaşçı vurur ve mermi varmadığı hücreden kalkardı.
            if (host.IsViewWalking(defender))
            {
                return OrderProgress.Continue;
            }

            ApproachOutcome approach = host.MoveIntoRange(defender, aggressor);

            switch (approach)
            {
                // KOVALAMANIN KENDİSİ BU DAL: yürüyüş yola çıktı, emir yaşıyor.
                case ApproachOutcome.MoveTo:
                    return OrderProgress.Continue;

                // Menzile girildi; aşağıdaki vuruşa düşülüyor.
                case ApproachOutcome.AlreadyInRange:
                    break;

                // TASMANIN İKİ UCU: saldırgan tahtadan gitti ya da ona yürünecek
                // hiçbir yol yok. İkisi de beklemekle düzelmez — kapalı bir
                // tahtayı yürümek açmıyor.
                case ApproachOutcome.RejectedOffBoard:
                case ApproachOutcome.RejectedUnreachable:
                    return OrderProgress.Cancelled;

                // default İPTAL: gerekçesi aşağıdaki vuruş switch'inde yazılı ve
                // ikisi aynı cümledir.
                default:
                    return OrderProgress.Cancelled;
            }

            AttackOutcome outcome = host.Strike(defender, aggressor);

            switch (outcome)
            {
                // Bekleme kuralı burada da İKİNCİ kez yazılmıyor; sayacın
                // sahibi Core ve gerekçesi AttackOrder'da tek kez yazılı.
                case AttackOutcome.RejectedOnCooldown:
                    return OrderProgress.Continue;

                // ██ AYRIŞMA NOKTASI — AttackOrder BURADA İPTAL EDİYOR ██
                // Saldırgan kaçtı ve menzilden çıktı; bu emir için bu bir son
                // değil bir BAŞLANGIÇ. Bir sonraki karede yukarıdaki yaklaşma
                // sorusu MoveTo döner ve savaşçı peşinden yürür.
                case AttackOutcome.RejectedOutOfRange:
                    return OrderProgress.Continue;

                // KALICILIK: isabet emri BİTİRMEZ. Saldırgan düştüğünde de emir
                // duruyor ve onu düşüren şey bir dal değil, saldırganın tahtadan
                // kalkması — yukarıdaki konum sorusu o kareyi zaten yakalıyor.
                case AttackOutcome.Hit:
                case AttackOutcome.HitAndDowned:
                case AttackOutcome.HitAndFinished:
                case AttackOutcome.HitAndDestroyed:
                    return OrderProgress.Continue;

                // Saldırgan artık geçerli hedef değil (öldü), ya da karşılık
                // veren artık eylem yapamaz (düştü). Beklemekle düzelmez.
                case AttackOutcome.RejectedInvalidTarget:
                case AttackOutcome.RejectedActorCannotAct:
                    return OrderProgress.Cancelled;

                // default İPTAL, DEVAM DEĞİL: AttackOutcome'a yeni bir değer
                // eklendiği gün emrin sonsuza kadar koşmasındansa düşmesi yeğdir.
                default:
                    return OrderProgress.Cancelled;
            }
        }

        /// <inheritdoc />
        public string Describe()
        {
            return $"striking back at '{aggressor.Name}'";
        }
    }
}
