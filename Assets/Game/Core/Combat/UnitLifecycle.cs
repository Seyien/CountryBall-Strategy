using System;

namespace GridStrategy.Combat
{
    // ═══ ROL: VARLIK (Entity) ════════════════════════════════════════
    // kimlik : var — ölçüsü şu: iki UnitLifecycle kur, yalnız birinde
    //          OnHealthDepleted() çağır; onun State'i Downed ve
    //          RemainingSeconds'ı 10 olur, ötekininki Alive ve 0'da kalır
    // hafıza : var — ölçüsü şu: OnHealthDepleted()'ten sonra Tick(11f)'i arka
    //          arkaya İKİ kez çağır, iki FARKLI şey olur. Birincisi kurtarma
    //          penceresini kapatır: State Downed'dan Dead'e geçer ve
    //          StateChanged tetiklenir. İkincisi State'e DOKUNMAZ, yalnızca
    //          ceset sayacını bitirip IsReadyForCleanup'ı true yapar. Farkı
    //          doğuran şey, tipin kalan saniyeyi çağrılar arasında tutması
    // Unity  : gerekmez — zaman DIŞARIDAN gelir, Time.deltaTime okunmaz
    // karar  : yalnızca KARAR verir ("artık Dead", "ceset kaldırılmalı");
    //          hiçbir şeyi yok etmez, çizmez, sahneye dokunmaz
    /// <summary>
    /// Bir birimin üç durumlu yaşam döngüsü ve geri sayımları.
    ///
    /// ZAMANI KENDİ OKUMAZ. <see cref="Tick"/> saniyeyi dışarıdan alır; içeride
    /// <c>Time.deltaTime</c> yoktur. Sebebi ölçülmüş ve gerekçesi Tick'in
    /// üstünde duruyor.
    ///
    /// Neyi BİLMEZ: canın kaç olduğunu (<see cref="Health"/>'in işi), kimin
    /// dirilttiğini, sahnede neyin silineceğini. Yalnızca "hangi durumdayım ve
    /// ne kadar kaldı" sorusunu cevaplar.
    ///
    /// GEREKÇELER: Docs/deep/kod/Core/Combat/UnitLifecycle.md
    /// </summary>
    public sealed class UnitLifecycle
    {
        public const float DefaultDownedWindowSeconds = 10f;
        public const float DefaultCorpseWindowSeconds = 5f;

        private readonly float downedWindowSeconds;
        private readonly float corpseWindowSeconds;

        // AYNI ANDA BİR TANESİ İŞLİYORSA, ALAN DA BİR TANEDİR. Bulunulan durumun
        // geri sayımı; Alive'da anlamsızdır ve kullanılmaz. İki ayrı sayaç alanı
        // reddedildi — "hangisi işliyor" bilgisi zaten State'te duruyor ve ikinci
        // kayıt, senkron tutulacak bir söze dönüşürdü.
        // → UnitLifecycle.md#remainingseconds-alan
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
        /// </summary>
        // SORAN YOKKEN İLGİLENEN VARSA, ŞEKİL EVENT'TİR. Dönüş değeri yetmezdi:
        // Tick içindeki Downed → Dead geçişini SORAN yoktur, ilgilenen (ceset
        // efekti, ses, skor) başka yerdedir. Bedeli yayıncıdan aboneye güçlü
        // referanstır ve bilerek ödendi.
        // → UnitLifecycle.md#statechanged
        // DERİN ANLATIM: Docs/deep/konular/01-olay-zinciri.md — dört durak (sayaç ->
        // savaşçı -> kayıt memuru -> çevirmen), hangi aboneliğin NEDEN sözlük
        // gerektirdiği ve sökülmezse önce neyin patladığı orada hikâye olarak.
        public event Action<UnitState> StateChanged;

        public UnitState State { get; private set; }


        /// <summary>
        /// Durumu değiştirir ve dinleyenlere haber verir. Tek giriş noktası
        /// olması kasıtlı: State'e doğrudan yazan bir satır kalsaydı, o geçiş
        /// sessizce kaybolurdu ve hata "bazen event gelmiyor" şeklinde çıkardı.
        /// </summary>
        private void SetState(UnitState next)
        {
            // Aynı duruma geçiş bir DEĞİŞİM değildir; event tetiklenmez. Bu satır
            // olmasaydı bir dinleyici aynı geçişi iki kez duyabilir ve iki kez
            // ses çalabilirdi.
            // → UnitLifecycle.md#setstateunitstate-next
            if (State == next)
            {
                return;
            }

            State = next;
            StateChanged?.Invoke(next);
        }

        // İSTEK BAYRAKLA SÖYLENİR, ÇÜNKÜ BAYRAK KİMSEYİ TUTMAZ. Temizlik için
        // bir event reddedildi: Tick'in ORTASINDA tetiklenir, abone hemen yok
        // etmeye başlar ve dönmekte olan döngü yok edilmiş birime dokunurdu.
        // Üstteki event kararının TERS yönü — okuyan zaten Tick'i çeviren taraf.
        // → UnitLifecycle.md#isreadyforcleanup
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
        // DERİN ANLATIM: Docs/deep/konular/05-yasam-dongusu.md
        public void OnHealthDepleted()
        {
            // KURTARMA PENCERESİNİ ATLAYAN KESTİRME, DURUMU DA SİLER. Kapı bilerek
            // yalnız Alive'dan geçirir: düşmüş birime tekrar vurmak onu ANINDA
            // öldürmemeli — "işini bitirme" ayrı bir kuraldır (düşme canı) ve o
            // kural henüz yazılmadı; buraya sessizce koymak yerini de yok ederdi.
            // → UnitLifecycle.md#onhealthdepleted
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

        // ZAMANI DIŞARIDAN ALMAK, SESSİZ BİR YANLIŞI İMKÂNSIZ KILAR. Ölçüldü:
        // EditMode'da Time.deltaTime sıfır DEĞİL, 0,017675 döner — zamanı
        // içeriden okuyan tasarım testte patlamaz, sessizce anlamsız bir sayıyla
        // yürür ve "Tick(10.1f) verince öldü" diyen test hiç yazılamaz.
        // → UnitLifecycle.md#tickfloat-deltaseconds
        /// <summary>
        /// Zamanı ilerletir. Saniye DIŞARIDAN gelir — bu tipin Unity'ye
        /// bağlanmamasının ve EditMode'da sınanabilmesinin tek sebebi budur.
        /// </summary>
        // DERİN ANLATIM: Docs/deep/konular/05-yasam-dongusu.md
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
            // → UnitLifecycle.md#tickfloat-deltaseconds
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
