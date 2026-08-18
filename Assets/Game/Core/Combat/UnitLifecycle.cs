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

        public UnitState State { get; private set; }

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
            if (State != UnitState.Alive)
            {
                return;
            }

            State = UnitState.Downed;
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

            State = UnitState.Alive;
            remainingSeconds = 0f;
            return true;
        }

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
                State = UnitState.Dead;
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
