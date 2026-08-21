using System;

namespace GridStrategy.Combat
{
    // ═══ ROL: VARLIK (Entity) ════════════════════════════════════════
    // kimlik : var — her yapının kendi durumu ve kendi enkaz sayacı
    // hafıza : var — aynı Tick(1f) çağrısı duruma göre farklı sonuç verir
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
    /// ONARIM ile DİRİLTME farkı — bu dosyanın en kolay karıştırılan satırı:
    /// diriltme bir DURUM geçişidir (yıkık → ayakta), onarım ise yalnızca bir
    /// SAYI değişikliğidir (can artar, durum aynı kalır). Bu tip onarımı hiç
    /// görmez; onarım <see cref="Health"/>'e aittir ve ayakta olan bir yapıda
    /// yapılır. Yıkılmış bir yapı onarılmaz — yeniden inşa edilir, ki o da bu
    /// tipin bir geçişi değil, yepyeni bir nesnedir.
    ///
    /// ZAMANI KENDİ OKUMAZ. <see cref="Tick"/> saniyeyi dışarıdan alır; içeride
    /// <c>Time.deltaTime</c> yoktur. Gerekçe <see cref="UnitLifecycle"/>'da
    /// ölçülerek yazıldı ve burada tekrar edilmiyor, yalnızca uygulanıyor:
    /// EditMode'da o değer sıfır DEĞİL, yani zamanı içeriden okuyan tasarım
    /// testte patlamaz — sessizce anlamsız bir sayıyla çalışır.
    ///
    /// Neyi BİLMEZ: canın kaç olduğunu (<see cref="Health"/>'in işi), hangi
    /// takıma ait olduğunu (<see cref="Structure"/>'ın işi), sahnede neyin
    /// silineceğini (Unity katmanının işi).
    /// </summary>
    public sealed class StructureLifecycle
    {
        // Enkaz penceresi cesetten uzun: yıkık bina bir HARİTA İŞARETİdir,
        // oyuncu ona bakıp "burada bir şey oldu" der. Sayı bir denge düğmesidir
        // ve kurucudan değiştirilebilir; bu dosyanın sahiplendiği kural sayı
        // değil, "sayaç işler ve dolunca temizlik İSTENİR" cümlesidir.
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

        // NEDEN StateChanged EVENT'İ YOK — UnitLifecycle'daki gerekçe körü körüne
        // kopyalanmadı, sınandı ve BURADA GEÇERSİZ çıktı. Oradaki tek cümle şuydu:
        // "dönüş değeri — soran zaten orada; event — ilgilenen başka yerde." Orada
        // event'i haklı çıkaran şey Tick'in içindeki Downed → Dead geçişiydi: onu
        // kimse SORMUYORDU, Tick'i çeviren oyun döngüsü o geçişle ilgilenmiyordu.
        // Burada Tick'in içinde hiçbir DURUM geçişi yok — Tick yalnızca bir bayrak
        // açıyor. Tek geçiş (ayakta → yıkık) her zaman bir hasar çağrısından doğar
        // ve o çağrıyı yapan taraf cevabı zaten dönüş değeriyle alır.
        //
        // REDDEDILEN - StructureLifecycle.cs:84 yerine:
        //     public event Action<StructureState> StateChanged;
        // KIRILAN  : aynı olgu iki yoldan birden duyurulur ve çağıran ikisini de duyar.
        //            OnHealthDepleted "bu çağrı yıktı mı" cevabını zaten döndürüyor
        //            hem dönüşü okuyan hem abone olan UI -> yıkım sesi iki kez çalar
        //            abonelik çözülmezse saf Core nesnesi ölü Unity nesnesini canlı tutar
        //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
        // KAZANIRDI: yapı, kimsenin ÇAĞRI yapmadığı bir yoldan yıkılabilseydi —
        //            yanıp kendiliğinden çöken bina, enerjisi kesilen kule, sahipsiz
        //            alan hasarı. O gün geçişin "soranı" olmazdı.
        // TEK CUMLE: Dönüş değeri "soran zaten orada" demektir, event "ilgilenen
        //            başka yerde"; burada her yıkımın bir soranı var.

        /// <summary>Yapının o anki durumu. Yeni yapı ayakta doğar.</summary>
        public StructureState State { get; private set; }

        // NEDEN SetState YOK: UnitLifecycle'daki tek giriş noktası deseninin
        // gerekçesi yorumunda yazılı — "State'e doğrudan yazan bir satır kalsaydı,
        // o geçiş sessizce kaybolurdu" ve hata "bazen event gelmiyor" diye çıkardı.
        // Burada event yok, dolayısıyla kaybolacak bir şey de yok; SetState bugün
        // yalnızca bir yönlendirme katmanı olurdu. Deseni geri getirecek tetikleyici
        // nettir ve bilerek yazılıyor: bu tipe bir event, bir geçiş kaydı (log) ya
        // da ikinci bir geçiş yolu eklendiği GÜN, State'e yazan tek satır kalmalı.

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

        // REDDEDILEN - StructureLifecycle.cs:121 yerine (dönüş değeri yok,
        //              metot void kalır ve çağıran durumu kendisi okur):
        //     public void OnHealthDepleted()
        // KIRILAN  : "bu vuruş mu yıktı" cevabı her çağıranda üç satırla elle kurulur.
        //            önce State'i oku, çağır, tekrar oku -> Structure'da, UI'da, skorda
        //            birinde ilk okuma unutulur -> enkaza vurmak yeni yıkım sayılır
        //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
        // KAZANIRDI: geçişle ilgilenen taraf çağıran DEĞİLSE — o gün yukarıda
        //            reddedilen event doğru cevaba dönerdi.
        // TEK CUMLE: Cevabı hesaplayabilen tek yer onu DÖNDÜRMELİDİR; yoksa aynı
        //            hesap her çağıranda yeniden doğar.
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
                return false;
            }

            State = StructureState.Destroyed;
            remainingSeconds = rubbleWindowSeconds;
            return true;
        }

        // NEDEN TryRevive YOK — bu satır bir eksiklik değil, bir karar:
        //
        // REDDEDILEN - StructureLifecycle.cs:160 üstüne eklenmesi reddedildi:
        //     public bool TryRepair()
        //     {
        //         if (State != StructureState.Destroyed) { return false; }
        //         State = StructureState.Standing;
        //         remainingSeconds = 0f;
        //         return true;
        //     }
        // KIRILAN  : bu tip canı GÖRMEZ; durumu "ayakta"ya çevirir, Current sıfırda kalır.
        //            sıfır canla ayakta bina -> değen ilk hasar onu anında tekrar yıkar
        //            hata "bina bazen hemen yıkılıyor" diye gelir, sebebi burada aranmaz
        //            derleyici: hiçbir şey der  .  test: Repair_AfterDestruction_IsRejected
        // KAZANIRDI: tasarım "çöken bina enkaz penceresi dolmadan yerinde ayağa
        //            kaldırılabilir" derse — pencere ve geri sayım zaten burada.
        // TEK CUMLE: Durumu değiştiren yer canı da görmek zorundadır; görmüyorsa
        //            o geçiş onun değildir.

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
            remainingSeconds = 0f;
            IsReadyForCleanup = true;
        }
    }
}
