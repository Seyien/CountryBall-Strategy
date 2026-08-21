using System;

namespace GridStrategy.Core
{
    /// <summary>
    /// Bir işaretçi jestinin içinde bulunduğu kip.
    ///
    /// Beş değerin ikisi GEÇİCİ (<see cref="Pressed"/>, <see cref="Dragging"/>),
    /// ikisi SONUÇ (<see cref="ClickReleased"/>, <see cref="DragReleased"/>),
    /// biri de yokluk (<see cref="Idle"/>). Sonuç değerleri, çağıran bir sonraki
    /// <see cref="PointerGesture.Press"/>'i yapana ya da
    /// <see cref="PointerGesture.Reset"/> diyene kadar okunabilir hâlde KALIR —
    /// böylece kararı üreten kare ile kararı tüketen kare aynı kare olmak
    /// zorunda değildir.
    ///
    /// Sıfırıncı değerin <see cref="Idle"/> olması bilinçli: <c>default</c> ile
    /// doğan bir alan "hiç basılmadı" der ve bu, yeni kurulmuş bir
    /// <see cref="PointerGesture"/>'ın gerçek hâliyle birebir aynıdır.
    /// <c>MoveOutcome</c> ve <c>AttackOutcome</c> sıfırıncı değeri bir RET'e
    /// ayırır; oradaki gerekçe "unutulmuş bir atama sessizce KABUL'e dönüşmesin"
    /// idi ve o gerekçe burada yok — burada unutulmuş atamanın doğal karşılığı
    /// zaten "jest yok"tur.
    /// </summary>
    // Bu enum, sahibinin dosyasında yaşıyor; projenin geri kalanında bir tip
    // bir dosya. Sebep kavramsal: MoveOutcome ve AttackOutcome'ın BİRDEN ÇOK
    // üreticisi ve assembly sınırını aşan tüketicileri var, PointerPhase'in
    // ise tek üreticisi PointerGesture ve tipin bütün imzaları bu enum'u
    // döndürüyor — ayrı dosyada bekleyen bir yarısı yok.
    public enum PointerPhase
    {
        /// <summary>Basılı değil; ortada bir jest yok.</summary>
        Idle = 0,

        /// <summary>Basıldı ama eşik henüz aşılmadı — bu hâlâ bir tıklama olabilir.</summary>
        Pressed,

        /// <summary>Eşik aşıldı; jest artık bir sürüklemedir ve geri dönmez.</summary>
        Dragging,

        /// <summary>Eşik hiç aşılmadan bırakıldı: tıklama tamamlandı.</summary>
        ClickReleased,

        /// <summary>Eşik aşıldıktan sonra bırakıldı: sürükleme tamamlandı.</summary>
        DragReleased
    }

    // ═══ ROL: DURUM MAKİNESİ (State Machine) ═════════════════════════
    // kimlik : var — aynı eşiğe sahip iki PointerGesture aynı şey DEĞİLDİR;
    //          biri basılı ve sürükleniyor, öteki boşta olabilir. Bu yüzden
    //          static değil, örneklenen bir sınıf.
    // hafıza : VAR ve tipin varlık sebebi bu — basmanın BAŞLADIĞI yer
    //          (pressX, pressY) ile içinde bulunulan kip. Aynı MoveTo(7, 0)
    //          çağrısı, nereden basıldığına göre iki farklı cevap verir.
    // Unity  : gerekmez — Input yok, Time yok, Vector2 yok, Camera yok;
    //          dört float ve bir eşik. Core'un asmdef'indeki
    //          noEngineReferences = true bu tiple bozulmaz.
    // karar  : VERİR — "bu bir tıklama mıydı, sürükleme mi". GridDistance ile
    //          farkı tam olarak burada: o yalnız ÖLÇER ve hafızasızdır
    //          ("aynı dört sayı her zaman aynı cevabı verir"); bu tip ölçüyü
    //          bir EŞİKLE karşılaştırıp bir KİP'e çevirir ve o kipi HATIRLAR.
    //          Aynı ailenin iki ucu: ölçü kararsız ve hafızasız, karar ise
    //          ölçüye dayanır ama ondan ibaret değildir.
    /// <summary>
    /// Basılı tutulan bir işaretçinin TIKLAMA mı yoksa SÜRÜKLEME mi olduğuna
    /// karar veren saf durum makinesi.
    ///
    /// Bu dosyanın var olma sebebi <c>BoardAdapter.cs</c>'in başında yazılı
    /// EŞİK'tir: "dördüncü kural geldiği gün Core'a tıklamayı niyete çeviren
    /// bir komut sahibi çıkmalı." Yerleştirme kipi bütün bir GİRİŞ KİPİ ekledi
    /// — sürükle-bırak ve tıkla-bırak aynı anda yaşayacak — ve eşik aşıldı.
    /// Cevap <c>BoardAdapter</c>'ı büyütmek değil, kararı dışarı çıkarmaktı:
    /// iki giriş şeklini ayıran tek soru ("basılı tutulurken işaretçi gerçekten
    /// hareket etti mi?") Unity'siz sorulabilen bir sorudur, dolayısıyla
    /// Unity'siz sorulur.
    ///
    /// Neyi BİLMEZ: hangi düğmeye basıldığını, kaç saniyedir basılı olduğunu,
    /// hücreyi, tahtayı, kamerayı, yerleştirilecek yapıyı, hayaletin nerede
    /// durduğunu. Hepsi çağıranın işidir. Bu tip yalnızca bir nokta akışına
    /// bakar ve o akışı bir kipe çevirir.
    ///
    /// BİRİM UYARISI — çağıranın uyması gereken tek sessiz sözleşme budur:
    /// <c>x</c>, <c>y</c> ve <c>dragThreshold</c> AYNI birimde olmalıdır. Tip
    /// bunları piksel mi, dünya birimi mi, ekran oranı mı olduğunu bilmez ve
    /// bilemez; karıştırıldığı gün kod derlenir, testler yeşil kalır ve
    /// yalnızca eşik yanlış yerde durur. Sözleşmeyi tipe yazdıramadığımız için
    /// buraya yazıyoruz.
    /// </summary>
    // REDDEDILEN - PointerGesture.cs:110 yerine (tip girdiyi kendisi okur):
    //     using UnityEngine;
    //     public PointerPhase Sample()   // Input.GetMouseButton... + mousePosition
    // KIRILAN  : Core'un asmdef'indeki noEngineReferences = true kırılır.
    //            EditMode'da sınanamaz -> eşiği sınamak için fare sürüklemek gerekir
    //            jest "sol fare düğmesi" -> dokunmatik ve atanmış tuş dışarıda kalır
    //            derleyici: asmdef ayarı değişmeden derlenmez  .  test: koşamaz
    // KAZANIRDI: proje tek bir giriş cihazına kilitlenseydi ve bu tipin tek çağıranı
    //            olsaydı — o gün üçlü çağrıyı her çağırana yazdırmak tören olurdu.
    // TEK CUMLE: Karar bir CİHAZ'ı tanımaz, tıpkı ölçünün bir VARLIK'ı tanımaması
    //            gibi; girdi dışarıdan gelir, kip içeride doğar.
    //
    // REDDEDILEN - PointerGesture.cs:110 yerine (kip yerine tek bir bayrak):
    //     public bool IsDragging { get; private set; }
    // KIRILAN  : beş kipin beşi tek bool'a sığmaz; Idle ile ClickReleased aynı
    //            değere düşer ve bayrak bir jestin BİTTİĞİNİ söyleyemez.
    //            çağıran ikinci bayrak tutar -> sıfırlanmadığı gün hayalet takılı kalır
    //            derleyici: hiçbir şey der  .  test: _StartsANewGesture yazılamaz
    // KAZANIRDI: yalnızca sürükleme desteklenseydi — tıklama diye bir giriş şekli
    //            olmasaydı enum tek bit taşıyan gereksiz bir tören olurdu.
    // TEK CUMLE: bool bir DEĞERİ taşır, kip ise bir AŞAMAYI; "sürüklüyor mu"
    //            sorusunun cevabı "bitti mi" sorusunu cevaplamaz.
    public sealed class PointerGesture
    {
        // Eşik KARE olarak saklanıyor; karşılaştırma da kare mesafeyle yapılıyor,
        // karekök ALINMIYOR. Asıl sebep hız değil, TİPİN SINIRI: sorulan soru "ne
        // kadar uzağa gitti" değil, "eşiği aştı mı" — bir evet/hayır. Karekök almak
        // kimsenin istemediği bir SAYI üretir; o sayı üretildiği an bir Distance
        // property'si olmak için yalvarır ve tip, GridDistance'ın işini ikinci kez
        // yapmaya başlar. Ölçünün sahibi bu tip değil.
        //
        // İkisi de AYNI cevabı verir ve bu tesadüf değil: kare alma işlemi negatif
        // olmayan sayılarda artan bir fonksiyondur, dolayısıyla sıralamayı korur.
        // Her iki taraf da negatif olamıyor — mesafenin karesi iki karenin toplamı,
        // eşik ise kurucuda doğrulanmış durumda.
        //
        // Karekökten kaçınmanın bir de hesap tarafı var ama ÖLÇMEDİM ve bu yüzden
        // gerekçe olarak SAYMIYORUM: her karede çağrılması onu makul kılar, kanıtlamaz.
        //
        // Bilinen sınır, gizlenmiyor: aşırı büyük bir eşik (yaklaşık 1.8e19 üstü)
        // karesi alınınca float'ta sonsuza taşar ve o jest ASLA sürüklemeye dönmez.
        // Karekökle karşılaştıran sürüm de pratikte aynı cevabı verirdi — o kadar
        // uzağa giden bir işaretçi yok — dolayısıyla bu bir davranış farkı değil,
        // yalnızca farkın nerede doğduğudur.
        //
        // REDDEDILEN - PointerGesture.cs:146 yerine (karekök alınır, eşik ham
        //              hâliyle saklanır):
        //     return Math.Sqrt((dx * dx) + (dy * dy)) > dragThreshold;
        // KIRILAN  : kod yine derlenir; kırılan şey davranış değil tipin ne BİLDİĞİ
        //            — elinde artık bir MESAFE var ve onu saklamamak zorlaşır.
        //            Math.Sqrt double döner -> float eşik sessizce yükseltilir
        //            tam eşikteki eşitlik   -> karekökün yuvarlamasına bağlanır
        //            derleyici: hiçbir şey der  .  test: _ExactlyAtThreshold kırılganlaşır
        // KAZANIRDI: eşik BİRDEN ÇOK biçimde sorulsaydı — "yüzde kaçını aştı" diyen
        //            bir ilerleme çubuğu ya da mesafeyle orantılı bir saydamlık; o gün
        //            mesafenin kendisi bir çıktıdır.
        // TEK CUMLE: Karekök almamak bir hız numarası değil, tipin eline ihtiyacı
        //            OLMAYAN bir sayıyı VERMEME kararıdır.
        private readonly float dragThresholdSquared;

        // Basmanın BAŞLADIĞI nokta. Ölçüm her zaman buradan yapılır, bir
        // önceki MoveTo'dan değil. Fark önemli ve sessiz: adım adım ölçseydik
        // yavaşça yüz piksel sürüklenen bir işaretçi hiçbir adımda eşiği
        // aşmazdı ve jest sonuna kadar "tıklama" kalırdı.
        private float pressX;
        private float pressY;

        /// <summary>
        /// Yeni bir jest okuyucusu kurar.
        /// </summary>
        /// <param name="dragThreshold">
        /// Basılı tutulurken bu uzaklığa kadar gidilebilir ve jest hâlâ bir
        /// tıklamadır. <c>x</c>/<c>y</c> ile AYNI birimde olmalıdır.
        /// </param>
        // Eşik DIŞARIDAN geliyor; içeride sabit yok. İçeriden bir sabit
        // okusaydık, eşiği sınamak için o sabiti değiştirmek gerekirdi — ve
        // sabit bir piksel eşiği farklı çözünürlükte farklı anlama gelir,
        // hiçbir test bunu göremez.
        //
        // REDDEDILEN - PointerGesture.cs:230 yerine (negatif eşik sessizce
        //              düzeltilir):
        //     dragThresholdSquared = Math.Max(0f, dragThreshold) * Math.Max(0f, dragThreshold);
        // KIRILAN  : kırılan şey HATANIN GÖRÜNÜRLÜĞÜ — negatif eşik yalnızca yanlış
        //            hesaplanmış bir sayıdan gelebilir.
        //            sıfıra kırpılır -> "her hareket sürüklemedir" diye çalışır
        //            tıklama şekli   -> sessizce ölür
        //            derleyici: hiçbir şey der  .  test: _NegativeThreshold_Throws kırmızı
        // KAZANIRDI: eşik oyuncunun ayarladığı bir kaydırıcıdan ve kaydırıcı bozuk
        //            bir kayıt dosyasından okunsaydı — ama o kırpmanın yeri bu tip
        //            değil, kaydı okuyan yerdir.
        // TEK CUMLE: Kırpmak yanlış hesabı DÜZELTMEZ, görünmez kılar; projenin kendi
        //            deseni de tersidir — iki profil de fırlatır, kırpmaz.
        //
        // REDDEDILEN - PointerGesture.cs:230 yerine (sıfır da yasaklanır,
        //              AttackProfile'ın "range < 1" eşiği birebir kopyalanır):
        //     if (dragThreshold <= 0f) throw new ArgumentOutOfRangeException(...);
        // KIRILAN  : ifade edilebilir oyunlardan biri dilden düşer — sıfır eşik "en
        //            ufak kıpırdama bile sürüklemedir" demektir ve dokunmatik olmayan
        //            hassas bir giriş için geçerli bir ayardır.
        //            derleyici: hiçbir şey der  .  test: _ZeroThreshold_IsAllowed kırmızı
        // KAZANIRDI: giriş cihazı gürültülü olsaydı — parmağın durduğu yerde bile
        //            titreyen bir dokunmatik ekranda sıfır eşik her tıklamayı
        //            sürüklemeye çevirirdi ve "tolerans yok" bir hata olurdu.
        // TEK CUMLE: Asimetri MoveProfile ile AttackProfile arasındakinin aynısı:
        //            ulaşamayan bir SALDIRI anlamsız, toleransı olmayan bir EŞİK anlamlı.
        public PointerGesture(float dragThreshold)
        {
            // NaN kontrolü negatif kontrolünden ÖNCE gelmek zorunda: NaN her
            // karşılaştırmada false verir, dolayısıyla (dragThreshold < 0f)
            // testinden yara almadan geçer. Geçtiği gün eşiğin karesi de NaN
            // olur, her karşılaştırma false döner ve jest ASLA sürüklemeye
            // dönmez — derlenen, fırlatmayan, testleri yeşil ve yarısı ölü bir
            // giriş sistemi. AttackProfile ve MoveProfile'ın int alan
            // kurucularında bu tuzak yoktu; float'a geçmenin bedeli tam olarak
            // bu satırdır.
            if (float.IsNaN(dragThreshold))
            {
                throw new ArgumentException("Drag threshold cannot be NaN.", nameof(dragThreshold));
            }

            // Sıfır GEÇERLİ, negatif değil. MoveProfile'ın "range < 0" eşiğiyle
            // aynı biçim ve aynı istisna tipi; gerekçesi yukarıdaki ikinci
            // REDDEDILEN bloğunda yazılı.
            if (dragThreshold < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(dragThreshold), dragThreshold, "Drag threshold cannot be negative.");
            }

            dragThresholdSquared = dragThreshold * dragThreshold;
            Phase = PointerPhase.Idle;
        }

        /// <summary>
        /// Jestin şu anki kipi. Bırakma kipleri
        /// (<see cref="PointerPhase.ClickReleased"/>,
        /// <see cref="PointerPhase.DragReleased"/>) bir sonraki
        /// <see cref="Press"/> ya da <see cref="Reset"/> çağrısına kadar
        /// okunabilir kalır.
        /// </summary>
        // Alternatif: kipi dışarıdan da yazılabilir yapmak (public set). Seçilmedi:
        // kipe karar vermek bu tipin TEK işi ve iptalin tek doğru yolu Reset'tir.
        public PointerPhase Phase { get; private set; }

        /// <summary>
        /// İşaretçi şu an basılı mı: <see cref="PointerPhase.Pressed"/> ya da
        /// <see cref="PointerPhase.Dragging"/>.
        /// </summary>
        // Türetilmiş, saklanmıyor. İkinci bir bool alan olsaydı iki kaynak
        // doğardı ve biri güncellenmediği gün kod derlenirdi.
        public bool IsActive => Phase == PointerPhase.Pressed || Phase == PointerPhase.Dragging;

        /// <summary>
        /// İşaretçi basıldı: yeni bir jest başlar ve ölçüm noktası burasıdır.
        /// </summary>
        /// <returns>Her zaman <see cref="PointerPhase.Pressed"/>.</returns>
        // Zaten basılıyken gelen ikinci bir Press jesti YENİDEN BAŞLATIR,
        // fırlatmaz. Gerekçe çağıranın gerçekliği: bu metodu besleyen şey bir
        // Update döngüsü ve pencerenin odağını kaybetmek, alt+tab, ya da
        // enjekte edilmiş bir girdi bırakma olayını yutabilir. Yutulduğu gün
        // doğru davranış oyunu düşürmek değil, bir sonraki basmayı yeni bir
        // jest saymaktır — kullanıcının niyeti de zaten budur.
        //
        // REDDEDILEN - PointerGesture.cs:262 yerine:
        //     if (IsActive) throw new InvalidOperationException("Pointer is pressed.");
        // KIRILAN  : yutulmuş TEK bir bırakma olayı, sonraki tıklamada oyunu düşürür.
        //            alt+tab, odak kaybı -> bırakma olayı hiç gelmez
        //            sonraki Press       -> jest yerine çökme üretir
        //            derleyici: hiçbir şey der  .  test: _RestartsFromTheNewOrigin kırmızı
        // KAZANIRDI: çağıran bir test ya da kayıttan oynatma olsaydı — orada bozuk
        //            sıra bir girdi hıçkırığı değil, kaydın bozulduğunun kanıtıdır ve
        //            sessizce düzeltilmesi asıl hatayı gizlerdi.
        // TEK CUMLE: Güvenilmez bir kaynağa istisna ile cevap vermek hatayı
        //            raporlamak değil, onu kullanıcıya TAŞIMAKTIR.
        public PointerPhase Press(float x, float y)
        {
            pressX = x;
            pressY = y;
            Phase = PointerPhase.Pressed;
            return Phase;
        }

        /// <summary>
        /// İşaretçi basılıyken yeni bir konuma taşındı.
        /// </summary>
        /// <returns>
        /// Eşik aşıldıysa <see cref="PointerPhase.Dragging"/>, aşılmadıysa
        /// <see cref="PointerPhase.Pressed"/>. Jest etkin değilse kip
        /// değişmeden döner.
        /// </returns>
        // BURASI KARARIN KENDİSİ: kip yalnızca Pressed iken yeniden
        // hesaplanıyor. Dragging'e bir kez geçildiğinde bu metot hiçbir şey
        // ölçmez ve GERİ DÖNMEZ. Sebep tek cümle: eşiği aşıp geri gelen bir
        // işaretçi hâlâ sürüklüyordur. İnsan eli bir hedefe götürüp geri
        // çeker; her karede yeniden karar veren bir kip, eşiğin tam üstünde
        // titreyen bir işaretçide saniyede onlarca kez Pressed ile Dragging
        // arasında atlar ve hayalet, oyuncunun hiç istemediği bir anda
        // yerleşir.
        //
        // Bu tek yönlülük bir if ile değil, YAPI ile korunuyor: Dragging'den
        // çıkan bir yol yok. Bir davranışı korumanın en ucuz yolu onu
        // ifade edilemez kılmaktır.
        //
        // REDDEDILEN - PointerGesture.cs:305 yerine (kip her karede yeniden
        //              hesaplanır ve geri dönebilir):
        //     Phase = ExceedsDragThreshold(x, y) ? PointerPhase.Dragging
        //                                        : PointerPhase.Pressed;
        // KIRILAN  : oyuncu sürüklediği yapıyı başladığı hücreye geri getirip
        //            bırakınca jest Pressed'e düşer.
        //            bırakma ClickReleased üretir -> yerleştirme hiç bitmez
        //            eşikte titreyen el           -> aynı hareket farklı davranır
        //            derleyici: hiçbir şey der  .  test: _StaysDragging ikisi de kırmızı
        // KAZANIRDI: jest iptal edilebilir olsaydı — başladığı yere dönmek
        //            "vazgeçtim" demek olsaydı; ama dönülen yer Pressed değil ayrı
        //            bir Cancelled kipi olurdu.
        // TEK CUMLE: Bir davranışı korumanın en ucuz yolu onu İFADE EDİLEMEZ
        //            kılmaktır; Dragging'den çıkan bir yol yok, o yüzden bir if de yok.
        public PointerPhase MoveTo(float x, float y)
        {
            if (Phase != PointerPhase.Pressed)
            {
                return Phase;
            }

            if (ExceedsDragThreshold(x, y))
            {
                Phase = PointerPhase.Dragging;
            }

            return Phase;
        }

        /// <summary>
        /// İşaretçi bırakıldı. Jest, eşiğin aşılıp aşılmadığına göre
        /// <see cref="PointerPhase.ClickReleased"/> ya da
        /// <see cref="PointerPhase.DragReleased"/> ile biter.
        /// </summary>
        /// <returns>
        /// Jest etkinse iki bırakma kipinden biri; etkin değilse kip
        /// değişmeden döner.
        /// </returns>
        // İKİ SONUCUN AYRI OLMASI BİR KARARDIR, üslup değil. Çağıran ikisine
        // farklı cevap verir: sürükleme bırakıldığında yerleştirme BİTER;
        // tıklama bırakıldığında hayalet fareyi TAKİP ETMEYE DEVAM EDER ve
        // ikinci bir tıklama yerleştirir. Tek değere indirmek iki giriş
        // şeklini tek şekle indirger: sürükle-bırak ile tıkla-bırak aynı
        // anda yaşayamaz.
        //
        // Bırakma konumu da eşikten GEÇİRİLİYOR — parametreler süs değil.
        // Sebep: Unity'de bırakma karesinde GetMouseButton false, yalnızca
        // GetMouseButtonUp true döner; yani o karenin konumu bu tipe SADECE
        // buradan ulaşır. Hızlı bir savurmada eşiğin aşıldığı ilk kare tam da
        // o karedir. Konumu yok saysaydık cevap, konumun HANGİ metottan
        // geldiğine bağlı olurdu — saf bir tipin içine motorun kare düzeninin
        // sızması demek olurdu bu. BEDELİ ödendi ve gizlenmiyor: "Release,
        // Pressed iken -> ClickReleased" artık tek başına doğru değil; Pressed
        // iken bırakılan bir jest, son konum eşiğin DIŞINDAYSA DragReleased
        // döner. Kip, karar anındaki kiptir ve son konum o karardan önce
        // hesaba katılır.
        //
        // REDDEDILEN - PointerGesture.cs:374 yerine (iki sonuç tek değere
        //              indirilir):
        //     public enum PointerPhase { Idle, Pressed, Dragging, Released }
        // KIRILAN  : çağıran "yerleştir ve kipten çık" ile "kipte kal, hayalet takip
        //            etsin" arasında seçim yapamaz; hangisini seçerse seçsin giriş
        //            şekillerinden biri tamamen ölür.
        //            derleyici: hiçbir şey der  .  test: _ProduceDifferentPhases kırmızı
        // KAZANIRDI: yalnızca TEK giriş şekli desteklenseydi — o gün iki değer,
        //            çağıranın her switch'inde aynı gövdeyi iki kez yazdırırdı.
        // TEK CUMLE: İki değer ancak çağıran onlara FARKLI davranıyorsa ayrıdır;
        //            burada biri kipi bitirir, öteki kipte tutar.
        //
        // REDDEDILEN - PointerGesture.cs:374 yerine (bırakma konumu yok
        //              sayılır, parametreler yalnızca simetri için durur):
        //     Phase = Phase == PointerPhase.Dragging ? PointerPhase.DragReleased
        //                                            : PointerPhase.ClickReleased;
        // KIRILAN  : aynı nokta akışı, hangi metottan girdiğine göre farklı cevap
        //            alır; hızlı savurmada bütün mesafe bırakma karesinde katedilir.
        //            MoveTo eşiği hiç görmez -> jest ClickReleased döner
        //            oyuncu geniş yay çizer  -> yapı yerleşmez, bıraktığını sanır
        //            derleyici: hiçbir şey der  .  test: _IsDragReleased kırmızıya döner
        // KAZANIRDI: çağıran bırakma karesinde de MoveTo çağırmayı garanti etseydi —
        //            o gün son konum tipe zaten ulaşmış olurdu ve Release'in konum
        //            alması gereksiz tekrar olurdu.
        // TEK CUMLE: Zorlanamayan bir garantiye yaslanmak sözleşmeyi yorumun eline
        //            bırakmaktır; bu yüzden Release son konumu kendisi ölçer.
        public PointerPhase Release(float x, float y)
        {
            if (!IsActive)
            {
                return Phase;
            }

            if (Phase == PointerPhase.Pressed && ExceedsDragThreshold(x, y))
            {
                Phase = PointerPhase.Dragging;
            }

            Phase = Phase == PointerPhase.Dragging
                ? PointerPhase.DragReleased
                : PointerPhase.ClickReleased;

            return Phase;
        }

        /// <summary>
        /// Jesti sıfırlar: kip <see cref="PointerPhase.Idle"/> olur ve basma
        /// noktası unutulur. Yerleştirme kipinden çıkarken, jest yarıda iptal
        /// edilirken ya da aynı örnek yeni bir jest için kullanılmadan önce
        /// çağrılır.
        /// </summary>
        // Basma noktasını da temizliyor. Kip Idle iken o iki sayı zaten
        // okunmuyor, dolayısıyla bu satır davranışı değiştirmez — ama iptal
        // edilmiş bir jestin koordinatlarını nesnede bırakmak, bir sonraki
        // hatanın "ölçüm nereden yapıldı" diye sorulduğunda yanlış cevap
        // vermesi demektir.
        public void Reset()
        {
            Phase = PointerPhase.Idle;
            pressX = 0f;
            pressY = 0f;
        }

        // Ölçüm HER ZAMAN basma noktasından. Karşılaştırma KESİN büyüktür:
        // tam eşik kadar gidilmiş bir jest hâlâ bir tıklamadır. Eşik "bu kadar
        // oynayabilirsin ve hâlâ tıklıyorsun" diye okunur; sınır dahildir.
        private bool ExceedsDragThreshold(float x, float y)
        {
            float dx = x - pressX;
            float dy = y - pressY;
            return ((dx * dx) + (dy * dy)) > dragThresholdSquared;
        }
    }
}
