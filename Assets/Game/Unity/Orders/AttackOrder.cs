using GridStrategy.Combat;
using GridStrategy.Core;

namespace GridStrategy.Unity
{
    //   ═══ KALICI SALDIRI EMRİ — HER KARE SORULAN DÖRT SORU ══════════
    //
    //     saldıran VE hedef tahtada mı ──hayır──► İPTAL
    //              │evet
    //     saldıranın görseli yürüyor mu ──evet──► DEVAM (henüz varmadı)
    //              │hayır
    //           VUR  ─────► sonucu OKU, kendi kuralını YAZMA
    //              │
    //     ┌────────┴───────────────────────────────┐
    //     Hit / Downed / Finished / Destroyed      RejectedOnCooldown
    //          ──► DEVAM (kalıcı!)                     ──► DEVAM (sessiz)
    //     OutOfRange / InvalidTarget / CannotAct
    //          ──► İPTAL
    //
    //   "KESİLME" TEK YERDE: hedef kaçınca AttackAction zaten
    //   RejectedOutOfRange döner. Menzil kuralı burada İKİNCİ kez
    //   yazılmıyor; okunuyor.

    /// <summary>
    /// Bir savaşçıya verilen kalıcı saldırı emri: hedef menzilde durduğu sürece
    /// vurmayı sürdürür.
    ///
    /// OYUNDA NE İŞE YARAR: operatörün bildirdiği eksik tam olarak buydu — hedef
    /// gösterilen savaşçı BİR KEZ vuruyor, sonra duruyordu. Artık oyuncu onu
    /// yeniden yönlendirene kadar vuruyor; hedef kaçıp menzilden çıkarsa emir
    /// kesiliyor ve YALNIZ o savaşçınınki kesiliyor.
    /// </summary>
    // MENZİLDEN KOPMA HER SALDIRAN İÇİN AYRI ÖLÇÜLÜYOR ve bunun bedeli sıfır:
    // ölçen taraf emir değil, AttackAction'ın kendisi — ve o, saldıranın KENDİ
    // profilini okuyor. Aynı hedefe vuran iki savaşçıdan menzili uzun olan
    // vurmaya devam ederken kısa olanınki düşer, çünkü iki ayrı emir iki ayrı
    // cevap alıyor.
    //
    // KOVALAMA YOK, ve bu operatörün kendi cümlesi: "hedef kaçıp menzilden
    // çıkarsa kesilmeli." Emrin peşinden yürümesi ikinci bir hareket sahibi
    // doğururdu ve oyuncunun elindeki birim, o hiç istemediği hâlde tahtanın
    // öteki ucuna yürürdü.
    public sealed class AttackOrder : IUnitOrder
    {
        private readonly IUnitOrderHost host;
        private readonly Unit attacker;
        private readonly Unit target;

        /// <summary>
        /// Emri kurar.
        /// </summary>
        /// <param name="orderHost">Tahtaya bakan pencere.</param>
        /// <param name="orderAttacker">Emri taşıyan savaşçı.</param>
        /// <param name="orderTarget">Vurulacak kimlik.</param>
        public AttackOrder(IUnitOrderHost orderHost, Unit orderAttacker, Unit orderTarget)
        {
            host = orderHost;
            attacker = orderAttacker;
            target = orderTarget;
        }

        /// <inheritdoc />
        public Unit Target => target;

        /// <inheritdoc />
        public OrderProgress Advance()
        {
            // İKİ KİMLİK DE SORULUYOR ve sırası gözlenemez; sorulmasının sebebi
            // ölçülmüş: konumu olmayan bir kimliğe saldırı çağrısı bir oyun
            // sonucu değil bir İSTİSNA üretir. Ayrı bir cümle de gerekmiyor —
            // DespawnView tahtadan kalkan kimliği zaten Console'a yazıyor.
            if (!host.TryGetCell(attacker, out _, out _) || !host.TryGetCell(target, out _, out _))
            {
                return OrderProgress.Cancelled;
            }

            // HENÜZ VARMADI: uzaktaki hedefe verilen emir önce bir yürüyüş
            // başlatıyor ve vuruş, görsel hedefin yanına VARINCA oluyor. Bu
            // satır olmasaydı savaşçı daha yolun ortasındayken saldırı pozu
            // oynar, ekran tahtada olup bitene göre yalan söylerdi.
            if (host.IsViewWalking(attacker))
            {
                return OrderProgress.Continue;
            }

            AttackOutcome outcome = host.Strike(attacker, target);

            switch (outcome)
            {
                // ██ BEKLEME KURALI BURADA İKİNCİ KEZ YAZILMIYOR ██
                // Sayacın sahibi Core: eşiği AttackProfile, sayacı Combatant
                // tutuyor ve AttackAction "henüz vuramaz"ı KENDİSİ söylüyor.
                // Emir o cevabı sessizce yutup bekliyor. Burada bir kronometre
                // tutulsaydı aynı niceliğin iki yazılabilir sahibi olurdu ve
                // Inspector'daki saniye değiştiği gün ikisi ayrışırdı.
                case AttackOutcome.RejectedOnCooldown:
                    return OrderProgress.Continue;

                // KALICILIĞIN KENDİSİ BU DÖRT DAL: isabet emri BİTİRMEZ.
                // Düşmüş bir hedefe vurmaya devam etmek de bilerek — bitirme
                // tasarımın parçası ve hedef tahtadan kalktığı an yukarıdaki
                // konum sorusu emri zaten düşürüyor.
                case AttackOutcome.Hit:
                case AttackOutcome.HitAndDowned:
                case AttackOutcome.HitAndFinished:
                case AttackOutcome.HitAndDestroyed:
                    return OrderProgress.Continue;

                // ██ OPERATÖRÜN İSTEDİĞİ KESİLME TAM OLARAK BU DAL ██
                // Hedef kaçtı ve BU saldıranın menzilinden çıktı. Cevabı veren
                // AttackResolver, saldıranın KENDİ AttackProfile'ıyla; yani aynı
                // hedefe vuran öteki savaşçının emri, o hâlâ menzildeyse
                // yaşamaya devam eder. Cümleyi tahta ReactToAttack'te zaten
                // yazdı; burada ikincisi yazılsaydı tek olgu iki satır olurdu.
                case AttackOutcome.RejectedOutOfRange:

                // HEDEF ARTIK GEÇERLİ DEĞİL (öldü, ya da kendi takımına döndü)
                // ve SALDIRAN ARTIK EYLEM YAPAMAZ (düştü). İkisi de emri
                // düşürür: bekleyerek düzelmeyen bir ret, sonsuza kadar
                // tekrarlanan bir rettir.
                case AttackOutcome.RejectedInvalidTarget:
                case AttackOutcome.RejectedActorCannotAct:
                    return OrderProgress.Cancelled;

                // default İPTAL, DEVAM DEĞİL: AttackOutcome'a yeni bir değer
                // eklendiği gün bu switch güncellenmezse emrin sonsuza kadar
                // koşmasındansa düşmesi yeğdir. Programcı hatasının sesini
                // tahtanın kendi switch'i (LogError) zaten çıkarıyor.
                default:
                    return OrderProgress.Cancelled;
            }
        }

        /// <inheritdoc />
        public string Describe()
        {
            return $"attacking '{target.Name}'";
        }
    }
}
