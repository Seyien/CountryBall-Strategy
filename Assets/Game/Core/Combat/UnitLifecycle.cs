using System;

namespace GridStrategy.Combat
{
    // ═══ ROL: VARLIK (Entity) ════════════════════════════════════════
    // kimlik : var — her birimin kendi durumu ve kendi geri sayımı
    // hafıza : var — aynı Tick(1f) çağrısı duruma göre farklı sonuç verir
    // Unity  : gerekmez — zaman DIŞARIDAN gelir, Time.deltaTime okunmaz
    // karar  : yalnızca KARAR verir ("artık Dead", "ceset kaldırılmalı");
    //          hiçbir şeyi yok etmez, çizmez, sahneye dokunmaz
    /// <summary>
    /// Bir birimin üç durumlu yaşam döngüsü ve geri sayımları.
    ///
    /// ZAMANI KENDİ OKUMAZ. <see cref="Tick"/> saniyeyi dışarıdan alır; içeride
    /// `Time.deltaTime` yoktur. Sebebi ölçülmüş: EditMode'da `Time.deltaTime`
    /// sıfır DEĞİL (0,017675) — yani zamanı içeriden okuyan bir tasarım testte
    /// patlamaz, sessizce anlamsız bir sayıyla çalışır. Dışarıdan almak hem o
    /// sessiz hatayı imkânsız kılar hem de 10 saniyelik kuralı gerçekten 10
    /// saniye beklemeden sınamayı mümkün kılar.
    ///
    /// Neyi BİLMEZ: canın kaç olduğunu (<see cref="Health"/>'in işi), kimin
    /// dirilttiğini, sahnede neyin silineceğini. Yalnızca "hangi durumdayım ve
    /// ne kadar kaldı" sorusunu cevaplar.
    /// </summary>
    public sealed class UnitLifecycle
    {
        public const float DefaultDownedWindowSeconds = 10f;
        public const float DefaultCorpseWindowSeconds = 5f;

        private readonly float downedWindowSeconds;
        private readonly float corpseWindowSeconds;

        // Bulunulan durumun geri sayımı. Alive'da anlamsızdır ve kullanılmaz;
        // tek alanla iki sayaç taşımak, iki alanın senkron kalmasını sağlamaktan
        // basittir — bir anda yalnızca bir geri sayım işler.
        //
        // REDDEDILEN - UnitLifecycle.cs:48 yerine:
        //     private float downedRemaining;
        //     private float corpseRemaining;
        // KIRILAN  : "hangi sayaç işliyor" bilgisi State'in yanında İKİNCİ kez durur.
        //            RemainingSeconds hangisini döndüreceğini State'e sorar
        //            TryRevive ikisini sıfırlamayı unutur -> ceset sayacı diriye taşınır
        //            derleyici: hiçbir şey der  .  test: hata saniyeler sonra görünür
        // KAZANIRDI: iki sayaç AYNI ANDA işlemek zorunda olsaydı — ceset süresi düşme
        //            anında başlayıp Downed boyunca da aksaydı.
        // TEK CUMLE: Bir anda yalnız bir geri sayım işliyorsa iki alan iki gerçek
        //            değil, senkron tutulacak tek gerçeğin iki kopyasıdır.
        private float remainingSeconds;

        public UnitLifecycle(
            float downedWindowSeconds = DefaultDownedWindowSeconds,
            float corpseWindowSeconds = DefaultCorpseWindowSeconds)
        {
            if (downedWindowSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(downedWindowSeconds), downedWindowSeconds, "Downed window must be positive.");
            }

            if (corpseWindowSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(corpseWindowSeconds), corpseWindowSeconds, "Corpse window must be positive.");
            }

            this.downedWindowSeconds = downedWindowSeconds;
            this.corpseWindowSeconds = corpseWindowSeconds;
            State = UnitState.Alive;
        }

        /// <summary>
        /// Durum her DEĞİŞTİĞİNDE tetiklenir ve yeni durumu taşır. Kurucudaki
        /// ilk atama tetiklemez — o bir geçiş değil, başlangıçtır; ve o anda
        /// abone olabilmiş kimse yoktur.
        ///
        /// NEDEN DÖNÜŞ DEĞERİ DEĞİL — bu ayrım bu dosyanın en pahalı satırı:
        /// <see cref="AttackAction.Execute"/> zaten "düşürdü" bilgisini
        /// DÖNDÜRÜYOR ve orada event gereksiz olurdu, çünkü soran zaten
        /// oradadır. Ama <see cref="Tick"/> ile olan Downed → Dead geçişini
        /// SORAN YOKTUR: Tick'i çeviren taraf oyun döngüsüdür ve o geçişle
        /// ilgilenmez; ilgilenen (ceset efekti, ses, skor) BAŞKA yerdedir.
        ///
        /// Tek cümlelik ayrım:
        /// <b>Dönüş değeri — soran zaten orada. Event — ilgilenen başka yerde.</b>
        /// </summary>
        public event Action<UnitState> StateChanged;

        public UnitState State { get; private set; }

        // REDDEDILEN - UnitLifecycle.cs:113 yerine (event hiç doğmaz, çağıran
        //              her kare State'i okuyup önceki değerle karşılaştırır):
        //     var before = lifecycle.State;
        //     lifecycle.Tick(dt);
        //     if (lifecycle.State != before) { /* tepki */ }
        // KIRILAN  : "önceki durumu hatırlamak" her çağıranın kendi işi olur.
        //            UI, ses ve skor üç ayrı kopya tutar -> biri unutur, hata sessizdir
        //            geçişi yalnız Tick'i çeviren görür -> başka dinleyici HİÇ göremez
        //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
        // KARSILASTIRMA:
        //     dönüş değeri  soran zaten orada  -> bu geçişi soran yok, cevap boşa gider
        //     her kare oku  soran belirsiz     -> hatırlama işi her çağırana dağılır
        //     event         ilgilenen başkada  -> geçiş, Tick'i hiç bilmeyene de ulaşır
        // KAZANIRDI: tek bir çağıran olsaydı ve o çağıran zaten her kare durumu
        //            okuyor olsaydı — o gün event yalnızca bir dolaylılık katmanı olurdu.
        // TEK CUMLE: Geçişi SORAN varsa dönüş değeri yeter; soran yokken ilgilenen
        //            varsa tek dürüst şekil event'tir.

        /// <summary>
        /// Durumu değiştirir ve dinleyenlere haber verir. Tek giriş noktası
        /// olması kasıtlı: State'e doğrudan yazan bir satır kalsaydı, o geçiş
        /// sessizce kaybolurdu ve hata "bazen event gelmiyor" şeklinde çıkardı.
        /// </summary>
        private void SetState(UnitState next)
        {
            // Aynı duruma geçiş bir DEĞİŞİM değildir; event tetiklenmez.
            // Bu satır olmasaydı bir dinleyici aynı geçişi iki kez duyabilir
            // ve iki kez ses çalabilirdi.
            if (State == next)
            {
                return;
            }

            State = next;
            StateChanged?.Invoke(next);
        }

        // REDDEDILEN - UnitLifecycle.cs:143 yerine:
        //     public event Action OnReadyForCleanup;
        // KIRILAN  : olay Tick'in ORTASINDA tetiklenir ve abone hemen yok etmeye başlar.
        //            dönmekte olan güncelleme döngüsü yok edilmiş birime dokunur
        //            abonelik çözülmezse saf Core nesnesi ölü Unity nesnesini tutar
        //            derleyici: hiçbir şey der  .  test: Core testleri yeşil kalır
        // KAZANIRDI: temizliği isteyen taraf ile Tick'i çeviren taraf farklı olsaydı —
        //            bayrağı kimse okumazdı ve haber vermek şart olurdu.
        // TEK CUMLE: Üstteki event kararının TERS yönü: burada okuyan zaten Tick'i
        //            çeviren taraf, o yüzden bayrak yeter ve bayrak kimseyi tutmaz.
        /// <summary>
        /// Ceset süresi dolduğunda true olur. Bu bir İSTEKtir, bir eylem değil:
        /// sahneden silme işini Unity katmanı yapar. Burada true olması, orada
        /// silindiği anlamına gelmez — bu ayrım bilinçlidir, çünkü "karar" ile
        /// "uygulama" farklı sahiplerdir.
        /// </summary>
        public bool IsReadyForCleanup { get; private set; }

        /// <summary>
        /// Kalan geri sayım. UI bu sayıyı gösterecek ("5 saniye sonra
        /// kaldırılacak"). <see cref="UnitState.Alive"/> iken anlamsızdır ve 0 döner.
        /// </summary>
        public float RemainingSeconds => State == UnitState.Alive ? 0f : remainingSeconds;

        /// <summary>Canı tükendiğinde çağrılır: ayakta olan birim düşer.</summary>
        public void OnHealthDepleted()
        {
            // Bilerek yalnızca Alive'dan çalışır. Downed bir birime tekrar vurmak
            // onu ANINDA öldürmemeli — "işini bitirme" ayrı bir kural (düşme canı)
            // ve o kural henüz yazılmadı. Buraya sessizce koymak, tasarımdaki iki
            // ayrı Downed→Dead yolunu bire indirirdi.
            //
            // REDDEDILEN - UnitLifecycle.cs:175 yerine:
            //     if (State == UnitState.Downed)
            //     {
            //         State = UnitState.Dead;
            //         remainingSeconds = corpseWindowSeconds;
            //         return;
            //     }
            // KIRILAN  : düşmüş birime değen tek bir sıyırık kurtarma penceresini kapatır.
            //            alan hasarı olan her saldırı istemeden "bitirme" hâline gelir
            //            düşme canı kuralının yazılacağı yer kalmaz
            //            derleyici: hiçbir şey der  .  test: Downed_HealthDepletedAgain_
            //            DoesNotSkipTheWindow kırmızı olur
            // KAZANIRDI: tasarım "yerdekine vuran bitirir" derse ve bitirmenin ayrı bir
            //            maliyeti, menzili ya da süresi olmayacaksa.
            // TEK CUMLE: Downed'ın var olma sebebi bir PENCEREdir; o pencereyi atlayan
            //            kısayol, durumun kendisini de gereksizleştirir.
            if (State != UnitState.Alive)
            {
                return;
            }

            SetState(UnitState.Downed);
            remainingSeconds = downedWindowSeconds;
        }

        /// <summary>
        /// Düşmüş birimi ayağa kaldırır. Yalnızca <see cref="UnitState.Downed"/>
        /// iken başarılı olur; kalıcı ölü diriltilemez.
        /// </summary>
        /// <returns>Diriltme gerçekleştiyse true.</returns>
        public bool TryRevive()
        {
            if (State != UnitState.Downed)
            {
                return false;
            }

            SetState(UnitState.Alive);
            remainingSeconds = 0f;
            return true;
        }

        // REDDEDILEN - UnitLifecycle.cs:218 yerine:
        //     public void Tick()
        //     {
        //         float deltaSeconds = Time.deltaTime;
        // KIRILAN  : ölçüldü — EditMode'da Time.deltaTime sıfır DEĞİL, 0,017675 döner.
        //            test patlamaz, sessizce 0,017675'lik adımlarla ilerler
        //            "10.1f verince öldü" diyen test hiç yazılamaz
        //            dosya UnityEngine'e bağlanır -> testler PlayMode'a düşer
        //            derleyici: hiçbir şey der  .  test: yeşil kalır, hiçbir şey ölçmez
        // KAZANIRDI: tip bir MonoBehaviour olsaydı ve zamanın tek bir kaynağı
        //            bulunsaydı — herkes aynı sayıyı geçiyorsa parametre seçim sunmaz.
        // TEK CUMLE: Zamanı DIŞARIDAN almak bir test kolaylığı değil, testin hiç
        //            yazılamayacağı bir sessizliği imkânsız kılmaktır.
        /// <summary>
        /// Zamanı ilerletir. Saniye DIŞARIDAN gelir — bu tipin Unity'ye
        /// bağlanmamasının ve EditMode'da sınanabilmesinin tek sebebi budur.
        /// </summary>
        public void Tick(float deltaSeconds)
        {
            if (deltaSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(deltaSeconds), deltaSeconds, "Time cannot move backwards.");
            }

            // Alive'da geri sayım yok; erken çıkış burada PERFORMANS için değil,
            // DOĞRULUK için: aşağıdaki çıkarma Alive'da anlamsız bir alanı
            // eksiltirdi.
            if (State == UnitState.Alive)
            {
                return;
            }

            remainingSeconds -= deltaSeconds;
            if (remainingSeconds > 0f)
            {
                return;
            }

            if (State == UnitState.Downed)
            {
                // Kurtarma penceresi doldu: kalıcı ölüm ve ceset sayacı başlar.
                SetState(UnitState.Dead);
                remainingSeconds = corpseWindowSeconds;
                return;
            }

            // Dead: ceset süresi doldu. Sayaç sıfırda tutuluyor ki sonraki
            // Tick'ler onu eksiye götürmesin ve UI negatif sayı göstermesin.
            remainingSeconds = 0f;
            IsReadyForCleanup = true;
        }
    }
}
