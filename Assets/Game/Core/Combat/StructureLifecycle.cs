using System;

namespace GridStrategy.Combat
{
    // ═══ ROL: VARLIK (Entity) ════════════════════════════════════════
    // kimlik : var — ölçüsü şu: iki StructureLifecycle kur, yalnız birinde
    //          OnHealthDepleted() çağır; onun State'i Destroyed ve
    //          RemainingSeconds'ı 8 olur, ötekininki Standing ve 0'da kalır
    // hafıza : var — ölçüsü şu: OnHealthDepleted()'ten sonra Tick(5f)'i
    //          arka arkaya İKİ kez çağır, iki FARKLI cevap alırsın:
    //          birinciden sonra RemainingSeconds 3 ve IsReadyForCleanup
    //          false, ikinciden sonra 0 ve true. Farkı doğuran şey, tipin
    //          kalan saniyeyi çağrılar arasında tutması; burada event yok,
    //          farkı görmenin tek yolu bu iki alanı okumak
    // Unity  : gerekmez — zaman DIŞARIDAN gelir
    // karar  : yalnızca KARAR verir ("artık yıkık", "enkaz kaldırılabilir");
    //          hiçbir şeyi yok etmez, çizmez, sahneye dokunmaz
    /// <summary>
    /// Bir yapının iki durumlu yaşam döngüsü ve enkaz geri sayımı.
    ///
    /// <see cref="UnitLifecycle"/>'ın kısaltılmışı DEĞİL — farklı bir kural
    /// kümesi. Burada <see cref="UnitState.Downed"/>'a denk bir durum ve bir
    /// <c>TryRevive</c> yok; eksik bırakıldıkları için değil, YANLIŞ oldukları
    /// için yoklar. Bir baraka düşüp kurtarılmayı beklemez: ayaktadır ya da
    /// enkazdır.
    ///
    /// ONARIM ile DİRİLTME farkı: diriltme bir DURUM geçişidir (yıkık → ayakta),
    /// onarım ise yalnızca bir SAYI değişikliğidir (can artar, durum aynı kalır).
    /// Bu tip onarımı hiç görmez.
    ///
    /// ZAMANI KENDİ OKUMAZ. <see cref="Tick"/> saniyeyi dışarıdan alır; gerekçe
    /// <see cref="UnitLifecycle"/>'da ölçülerek yazıldı ve burada tekrar
    /// edilmiyor, yalnızca uygulanıyor.
    ///
    /// Neyi BİLMEZ: canın kaç olduğunu (<see cref="Health"/>'in işi), hangi
    /// takıma ait olduğunu (<see cref="Structure"/>'ın işi), sahnede neyin
    /// silineceğini (Unity katmanının işi).
    ///
    /// GEREKÇELER: Docs/deep/kod/Core/Combat/StructureLifecycle.md
    /// </summary>
    // DERİN ANLATIM: Docs/deep/konular/05-yasam-dongusu.md
    public sealed class StructureLifecycle
    {
        // Enkaz penceresi cesetten uzun: yıkık bina bir HARİTA İŞARETİdir. Sayı
        // bir denge düğmesidir ve kurucudan değiştirilebilir; bu dosyanın
        // sahiplendiği kural sayı değil, "sayaç işler ve dolunca temizlik
        // İSTENİR" cümlesidir.
        // → StructureLifecycle.md#defaultrubblewindowseconds
        public const float DefaultRubbleWindowSeconds = 8f;

        private readonly float rubbleWindowSeconds;

        private float remainingSeconds;

        public StructureLifecycle(float rubbleWindowSeconds = DefaultRubbleWindowSeconds)
        {
            if (rubbleWindowSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(rubbleWindowSeconds), rubbleWindowSeconds, "Rubble window must be positive.");
            }

            this.rubbleWindowSeconds = rubbleWindowSeconds;
            State = StructureState.Standing;
        }

        // EVENT, GEÇİŞİN SORANI YOKKEN DOĞAR — burada her yıkımın soranı var.
        // UnitLifecycle'daki gerekçe kopyalanmadı, sınandı ve BURADA GEÇERSİZ
        // çıktı: Tick'in içinde tek bir DURUM geçişi yok, tek geçiş (ayakta →
        // yıkık) her zaman bir hasar çağrısından doğar ve cevabı dönüşle alınır.
        // → StructureLifecycle.md#statechanged

        /// <summary>Yapının o anki durumu. Yeni yapı ayakta doğar.</summary>
        public StructureState State { get; private set; }

        // NEDEN SetState YOK: UnitLifecycle'daki tek giriş noktası deseni event'i
        // tetiklemek içindi; burada event olmadığı için kaybolacak bir yayın da
        // yok ve SetState yalnızca bir yönlendirme katmanı olurdu. Deseni geri
        // getirecek tetikleyici net: event, geçiş kaydı ya da ikinci bir yol.
        // → StructureLifecycle.md#state

        /// <summary>
        /// Enkaz süresi dolduğunda true olur. Bu bir İSTEKtir, bir eylem değil:
        /// sahneden silme işini Unity katmanı yapar. Burada true olması orada
        /// silindiği anlamına gelmez — "karar" ile "uygulama" farklı sahiplerdir.
        /// </summary>
        public bool IsReadyForCleanup { get; private set; }

        /// <summary>
        /// Kalan enkaz süresi. Yapı ayaktayken anlamsızdır ve 0 döner.
        /// </summary>
        public float RemainingSeconds => State == StructureState.Standing ? 0f : remainingSeconds;

        // CEVABI HESAPLAYABİLEN TEK YER ONU DÖNDÜRMELİDİR. `void` bırakılsaydı
        // "bu vuruş mu yıktı" cevabı her çağıranda önce-oku / çağır / sonra-oku
        // diye elle kurulurdu; ilk okumayı unutan tek yerde enkaza değen alan
        // hasarı YENİ bir yıkım sayılır — ses tekrar çalar, skor tekrar artar.
        // → StructureLifecycle.md#onhealthdepleted
        /// <summary>
        /// Canı tükendiğinde çağrılır: ayakta olan yapı yıkılır ve enkaz sayacı başlar.
        /// </summary>
        /// <returns>Yapı BU çağrıyla yıkıldıysa true; zaten yıkıksa false.</returns>
        public bool OnHealthDepleted()
        {
            if (State != StructureState.Standing)
            {
                // İkinci vuruş enkaz sayacını SIFIRLAMAZ. Sıfırlasaydı, yıkık bir
                // binaya rastgele düşen alan hasarı enkazı sonsuza dek ekranda
                // tutardı — ve bu, hiçbir zaman ortaya çıkmayan türden bir hatadır:
                // kimse "enkaz neden hâlâ duruyor" diye bug açmaz.
                // → StructureLifecycle.md#onhealthdepleted
                return false;
            }

            State = StructureState.Destroyed;
            remainingSeconds = rubbleWindowSeconds;
            return true;
        }

        // DURUMU ÇEVİREN YER, DAYANDIĞI OLGUYU GÖRMEK ZORUNDA — bu tip canı
        // GÖRMEZ. Buraya bir TryRepair konsaydı durumu "ayakta"ya çevirir, Current
        // sıfırda kalırdı: değen ilk hasar binayı anında tekrar yıkardı. Onarımın
        // kelepçesi canı ve durumu aynı anda gören Structure.TryRepair'de duruyor.
        // → StructureLifecycle.md#tryrepair

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

            // Ayakta geri sayım yok; erken çıkış burada PERFORMANS için değil,
            // DOĞRULUK için: aşağıdaki çıkarma ayakta bir yapıda anlamsız bir
            // alanı eksiltirdi.
            // → StructureLifecycle.md#tickfloat-deltaseconds
            if (State == StructureState.Standing)
            {
                return;
            }

            remainingSeconds -= deltaSeconds;
            if (remainingSeconds > 0f)
            {
                return;
            }

            // Enkaz süresi doldu. Sayaç sıfırda tutuluyor ki sonraki Tick'ler onu
            // eksiye götürmesin ve UI negatif sayı göstermesin.
            // → StructureLifecycle.md#tickfloat-deltaseconds
            remainingSeconds = 0f;
            IsReadyForCleanup = true;
        }
    }
}
