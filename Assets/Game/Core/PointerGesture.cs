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
    /// <see cref="PointerGesture"/>'ın gerçek hâliyle birebir aynıdır — yani
    /// <c>MoveOutcome</c>'un "sıfır bir RET olsun" gerekçesi burada YOK.
    ///
    /// GEREKÇELER: Docs/deep/kod/Core/PointerGesture.md
    /// </summary>
    // Bu enum, sahibinin dosyasında yaşıyor; projenin geri kalanında bir tip
    // bir dosya. Sebep kavramsal: tek üreticisi PointerGesture ve tipin bütün
    // imzaları bu enum'u döndürüyor — ayrı dosyada bekleyen bir yarısı yok.
    // → PointerGesture.md#pointerphase
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
    //          (pressX, pressY) ile Phase, çağrılar arasında yaşar. Ölçüsü şu:
    //          dragThreshold'u 5 olan bir örnekte Press(0, 0) sonrası
    //          MoveTo(7, 0) PointerPhase.Dragging döner; AYNI MoveTo(7, 0),
    //          Press(6, 0) sonrası PointerPhase.Pressed döner. Çağrı ve
    //          argüman birebir aynı, cevap farklı — farkı doğuran şey tipin
    //          nereden basıldığını hatırlaması.
    // Unity  : gerekmez — Input yok, Time yok, Vector2 yok, Camera yok;
    //          dört float ve bir eşik. Core'un asmdef'indeki
    //          noEngineReferences = true bu tiple bozulmaz.
    // karar  : VERİR — "bu bir tıklama mıydı, sürükleme mi". GridDistance ile
    //          farkı tam olarak burada: o yalnız ÖLÇER ve hafızasızdır; bu tip
    //          ölçüyü bir EŞİKLE karşılaştırıp bir KİP'e çevirir ve HATIRLAR.
    /// <summary>
    /// Basılı tutulan bir işaretçinin TIKLAMA mı yoksa SÜRÜKLEME mi olduğuna
    /// karar veren saf durum makinesi.
    ///
    /// Bu dosyanın var olma sebebi <c>BoardAdapter.cs</c>'in başında yazılı
    /// EŞİK'tir: "dördüncü kural geldiği gün Core'a tıklamayı niyete çeviren
    /// bir komut sahibi çıkmalı." Yerleştirme kipi bütün bir GİRİŞ KİPİ ekledi
    /// — sürükle-bırak ve tıkla-bırak aynı anda yaşayacak — ve eşik aşıldı.
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
    /// yalnızca eşik yanlış yerde durur.
    ///
    /// GEREKÇELER: Docs/deep/kod/Core/PointerGesture.md
    /// </summary>
    // GİRDİ DIŞARIDAN GELİR: tip cihazı TANIMAZ. Üç Input sorgusunu
    // BoardAdapter okur, asmdef duvarını yalnız dört float geçer. Duvarın
    // altında "sol fare düğmesi" diye bir kavram yok. Duvarı geçemeyen şey
    // VERİ değil CİHAZ BİLGİSİ: eşik de dışarıdan gelir ve geçer.
    //
    // KİP BİR AŞAMADIR: beş hâl tek bool'a sığmaz. Tek bir "IsDragging" bayrağı
    // geçiş tablosunun yalnız bir sütununu taşır; Idle ile ClickReleased aynı
    // değere düşer ve bayrak bir jestin BİTTİĞİNİ söyleyemez. Reddedilen şey
    // bool değil, kipin YERİNE geçen bool — IsActive türetilmiş olduğu için
    // aynı eleştiriye girmez.
    // → PointerGesture.md#pointergesture-tip
    // DERİN ANLATIM: Docs/deep/konular/07-tiklamadan-eyleme.md
    public sealed class PointerGesture
    {
        // KAREKÖK ALINMIYOR ve sebep hız değil, TİPİN SINIRI: sorulan soru "ne
        // kadar uzağa gitti" değil "eşiği aştı mı" — bir evet/hayır. Karekök
        // kimsenin istemediği bir SAYI üretir; o sayı doğduğu an bir Distance
        // property'si için yalvarır ve tip, GridDistance'ın işini ikinci kez
        // yapmaya başlar. Kare alma sıralamayı koruduğu için cevap AYNI.
        // Bilinen sınır gizlenmiyor: yaklaşık 1.8e19 üstü bir eşik karesi
        // alınınca float'ta sonsuza taşar ve o jest asla sürüklemeye dönmez.
        // → PointerGesture.md#dragthresholdsquared
        private readonly float dragThresholdSquared;

        // Basmanın BAŞLADIĞI nokta. Ölçüm her zaman buradan yapılır, bir
        // önceki MoveTo'dan değil. Fark önemli ve sessiz: adım adım ölçseydik
        // yavaşça yüz piksel sürüklenen bir işaretçi hiçbir adımda eşiği
        // aşmazdı ve jest sonuna kadar "tıklama" kalırdı.
        private float pressX;
        private float pressY;

        // EŞİK DIŞARIDAN GELİR: bu tipte sabit yok. İçeriden okunan bir sabit
        // olsaydı eşiği sınamak için o sabiti değiştirmek gerekirdi; üstelik
        // sabit bir piksel eşiği farklı çözünürlükte farklı anlama gelir ve
        // hiçbir test bunu göremez. Aşağıdaki iki kontrol aynı kapıyı iki kez
        // kilitlemez: IsNaN KARŞILAŞTIRILAMAYAN sayıyı, negatif kontrolü
        // karşılaştırılabilir ama ANLAMSIZ sayıyı keser.
        // → PointerGesture.md#pointergesturefloat-dragthreshold
        //
        // ÖDÜNÇ ALINAN — nameof, ArgumentException ve
        // ArgumentOutOfRangeException: aynı parametre iki ayrı istisna tipi
        // alır ve ayıran şey üslup değil, sorunun cinsi.
        // DİL: Docs/deep/dil/03-hata-bildirme-ve-dogrulama.md
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
        // KİPİN TEK YAZARI BU TİPTİR. public set olsaydı geçiş tablosu bir SÖZ
        // olmaktan çıkıp bir öneriye dönerdi: "Dragging'den Pressed'e dönüş
        // yok" güvencesi tek bir atamayla aşılırdı. Reset bunun karşı örneği
        // değil — o bir kip ADI almaz, yalnız "iptal" der; hedefi tip seçer.
        // → PointerGesture.md#phase
        public PointerPhase Phase { get; private set; }

        /// <summary>
        /// İşaretçi şu an basılı mı: <see cref="PointerPhase.Pressed"/> ya da
        /// <see cref="PointerPhase.Dragging"/>.
        /// </summary>
        // Türetilmiş, saklanmıyor. İkinci bir bool alan olsaydı iki kaynak
        // doğardı ve biri güncellenmediği gün kod derlenirdi.
        // → PointerGesture.md#isactive
        public bool IsActive => Phase == PointerPhase.Pressed || Phase == PointerPhase.Dragging;

        /// <summary>
        /// İşaretçi basıldı: yeni bir jest başlar ve ölçüm noktası burasıdır.
        /// </summary>
        /// <returns>Her zaman <see cref="PointerPhase.Pressed"/>.</returns>
        // Zaten basılıyken gelen ikinci bir Press jesti YENİDEN BAŞLATIR,
        // fırlatmaz. Gerekçe çağıranın gerçekliği: bu metodu besleyen şey bir
        // Update döngüsü ve alt+tab ya da odak kaybı bir bırakma olayını
        // yutabilir; yutulduğu gün doğru davranış oyunu düşürmek değildir.
        // Hoşgörü SIRAYA özel, DEĞERE değil — kurucu bozuk eşik için fırlatır.
        // → PointerGesture.md#pressfloat-x-float-y
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
        // BURASI KARARIN KENDİSİ: kip yalnızca Pressed iken yeniden hesaplanır,
        // Dragging'e geçildikten sonra GERİ DÖNMEZ. Eşiği aşıp geri gelen bir
        // işaretçi hâlâ sürüklüyordur; her karede yeniden karar veren bir kip,
        // eşiğin üstünde titreyen elde Pressed ile Dragging arasında atlar.
        // Tek yönlülük bir if ile değil YAPI ile korunuyor: Dragging'den çıkan
        // bir yol yok, o yüzden bir if de yok.
        // → PointerGesture.md#movetofloat-x-float-y
        // DERİN ANLATIM: Docs/deep/konular/07-tiklamadan-eyleme.md
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
        // İKİ SONUCUN AYRI OLMASI BİR KARARDIR: sürükleme bırakıldığında
        // yerleştirme BİTER, tıklama bırakıldığında hayalet fareyi TAKİP
        // ETMEYE devam eder. Tek "Released" değeri iki giriş şeklinden birini
        // tamamen öldürürdü.
        //
        // BIRAKMA KONUMU DA EŞİKTEN GEÇİYOR — parametreler süs değil: bırakma
        // karesinde GetMouseButton false döner, yani o karenin konumu bu tipe
        // SADECE buradan ulaşır ve hızlı savurmada eşik tam orada aşılır.
        // BEDELİ gizlenmiyor: Pressed iken bırakılan bir jest, son konum eşiğin
        // DIŞINDAYSA ClickReleased değil DragReleased döner.
        // → PointerGesture.md#releasefloat-x-float-y
        // DERİN ANLATIM: Docs/deep/konular/07-tiklamadan-eyleme.md
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
        // hatanın "ölçüm nereden yapıldı" sorusuna yanlış cevap vermesidir.
        // → PointerGesture.md#reset
        public void Reset()
        {
            Phase = PointerPhase.Idle;
            pressX = 0f;
            pressY = 0f;
        }

        // Ölçüm HER ZAMAN basma noktasından. Karşılaştırma KESİN büyüktür:
        // tam eşik kadar gidilmiş bir jest hâlâ bir tıklamadır. Eşik "bu kadar
        // oynayabilirsin ve hâlâ tıklıyorsun" diye okunur; sınır dahildir.
        // → PointerGesture.md#exceedsdragthresholdfloat-x-float-y
        private bool ExceedsDragThreshold(float x, float y)
        {
            float dx = x - pressX;
            float dy = y - pressY;
            return ((dx * dx) + (dy * dy)) > dragThresholdSquared;
        }
    }
}
