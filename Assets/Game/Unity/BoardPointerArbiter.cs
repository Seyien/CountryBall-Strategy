using GridStrategy.Core;

namespace GridStrategy.Unity
{
    /// <summary>
    /// Sol tuşla yapılan bir işaretçi hareketinin sonucu: haritayı kaydırmak,
    /// tahtaya tıklamak ya da hiçbir şey.
    ///
    /// Sıfırıncı değer <see cref="None"/>: bir karenin normal cevabı "olay yok"
    /// tur ve <c>default</c> ile doğan bir alan da onu söylemelidir.
    /// </summary>
    // Enum sahibinin dosyasında yaşıyor; PointerPhase'in PointerGesture.cs
    // içinde durmasıyla aynı gerekçe: tek üreticisi BoardPointerArbiter ve
    // tipin tek public metodu bu enum'u döndürüyor.
    public enum BoardPointerAction
    {
        /// <summary>Bu karede yapılacak bir şey yok.</summary>
        None = 0,

        /// <summary>Eşik bu karede aşıldı: kaydırma başlasın.</summary>
        PanBegin,

        /// <summary>Kaydırma sürüyor: kamera imleci takip etsin.</summary>
        PanContinue,

        /// <summary>Kaydırma bitti: kamera tutamağı bıraksın.</summary>
        PanEnd,

        /// <summary>Eşik hiç aşılmadan bırakıldı: bu bir tıklamadır.</summary>
        Click
    }

    // ═══ ROL: HAKEM (Arbiter) ════════════════════════════════════════
    // kimlik : var — aynı eşikle kurulmuş iki hakem aynı şey DEĞİLDİR; biri
    //          kaydırma ortasında, öteki boşta olabilir.
    // hafıza : var, ama kendi alanında DEĞİL: bütün hafıza sarmaladığı
    //          PointerGesture'da. Bu tipin tek alanı o jesttir ve ikinci bir
    //          "panning" bayrağı bilerek YOK — kaydırmanın başladığı kare,
    //          MoveTo'nun ÖNCESİ ve SONRASI karşılaştırılarak bulunuyor.
    // Unity  : gerekmez — Input yok, Camera yok, Vector2 yok; üç bool, iki
    //          float ve bir eşik. EditMode'da `new` ile kurulup sınanabiliyor,
    //          tıpkı Modes/ altındaki kipler gibi.
    // karar  : VERİR — "bu kare kaydırma mı, tıklama mı, hiçbiri mi".
    /// <summary>
    /// Sol tuşa basılı geçen bir jestin haritayı KAYDIRMAK mı yoksa tahtaya
    /// TIKLAMAK mı istediğine karar veren tek yer.
    ///
    /// Burada yaşıyor çünkü sol tuşu okuyan tek tip <see cref="BoardAdapter"/>
    /// ve o bir MonoBehaviour: EditMode'da Update'i hiç koşmaz, kararı da
    /// sınanamazdı.
    ///
    /// BİRİM UYARISI: <c>x</c>, <c>y</c> ve eşik AYNI birimde olmalı — sarmaladığı
    /// <see cref="PointerGesture"/> bunu bilmez ve karıştırıldığı gün her şey
    /// derlenir, testler yeşil kalır, yalnız eşik yanlış yerde durur.
    /// </summary>
    // ██ NEDEN PointerGesture'IN ÜSTÜNE, İÇİNE DEĞİL ██
    // PointerGesture bir jestin KİPİNİ söyler ve söylediği şey oyundan
    // bağımsızdır: basıldı, sürüklendi, bırakıldı. Kaydırma ise bir KAMERA
    // fiilidir ve Core'un asmdef'i noEngineReferences taşıyor — o kelimenin
    // oraya girmesi, bir gün Camera'yı da davet ederdi. Sarmalayan tip Unity
    // katmanında, sarmalanan karar Core'da kalıyor.
    //
    // REDDEDILEN - Kararı BoardCameraRig'e vermek.
    //     if (Input.GetMouseButtonDown(0)) { dragAnchorWorld = ...; }
    // KIRILAN: rig, tıklamanın iptal edildiği üç şartın hiçbirini bilmiyor —
    // yerleştirme kipi açık mı (modeOwnsPointer), imleç arayüzün üstünde mi
    // (PointerIsOverUi), tahta hangi hücrede. Üçünü de sorabilmesi için
    // kameranın tahtayı ADIYLA tanıması gerekirdi ve ok bugün tek yön.
    // KAZANIRDI: kamera bir gün tahtasız bir sahnede tek başına gezinirse —
    // o gün iptal şartı da kalmaz ve rig kendi tuşunu kendi okuyabilir.
    // TEK CUMLE: hakemin bilmesi gereken şeylerin hepsi tahtada, hiçbiri
    // kamerada.
    public sealed class BoardPointerArbiter
    {
        // Sarmalanan karar. `readonly` çünkü hakem ömrü boyunca aynı jesti
        // kullanır; eşik değişseydi yeni bir hakem kurulurdu.
        private readonly PointerGesture gesture;

        /// <summary>
        /// Verilen sürükleme eşiğiyle yeni bir hakem kurar.
        /// </summary>
        /// <param name="dragThreshold">
        /// Basışın sürükleme sayılması için gereken en küçük yol; <c>x</c> ve
        /// <c>y</c> ile aynı birimde.
        /// </param>
        // Eşiğin doğrulaması burada TEKRARLANMIYOR: NaN ve negatif kontrolü
        // PointerGesture'ın kurucusunda ve oradan fırlayan istisna aynen
        // yukarı çıkıyor. İkinci bir kontrol, iki kapının bir gün ayrışması
        // demekti.
        public BoardPointerArbiter(float dragThreshold)
        {
            gesture = new PointerGesture(dragThreshold);
        }

        /// <summary>
        /// Şu an sürmekte olan bir jest var mı: basılı ya da sürükleniyor.
        /// </summary>
        // Çağıranın UCUZ ÇIKIŞI için: fare hiç kımıldamayan bir karede tahta,
        // ekran noktasını dünyaya çevirmeden geri dönebilsin diye.
        public bool IsActive => gesture.IsActive;

        /// <summary>
        /// Bu karenin fare durumunu alır ve tahtanın ne yapması gerektiğini
        /// söyler.
        /// </summary>
        /// <param name="pressed">Sol tuş BU karede basıldı mı.</param>
        /// <param name="held">Sol tuş bu karede basılı mı.</param>
        /// <param name="released">Sol tuş BU karede bırakıldı mı.</param>
        /// <param name="x">İşaretçinin yatay konumu, eşikle aynı birimde.</param>
        /// <param name="y">İşaretçinin dikey konumu, eşikle aynı birimde.</param>
        /// <param name="pressBlocked">
        /// Basış reddedilsin mi — imleç arayüzün üstündeyse doğru geçilir.
        /// Yalnız BASIŞ karesinde bakılır: yarısı tahtada başlayıp arayüzün
        /// üstünde biten bir kaydırma kesilmez.
        /// </param>
        // ██ DÖRT DAL, VE SIRALARI BİRER KARARDIR ██
        // BASIŞ ÖNCE: aynı karede hem Down hem Up gelebilir (çok hızlı tıklama)
        // ve basış işlenmeseydi o tıklama kaybolurdu — bugünkü
        // GetMouseButtonDown yolu onu yakalıyor, yenisi de yakalamalı.
        // BIRAKIŞ İKİNCİ: kararın verildiği yer burası, çünkü sonucun adı
        // ancak eşiğin aşılıp aşılmadığı bilindiğinde konulabilir.
        // DÜĞME KALKMIŞ AMA BIRAKIŞ GÖRÜLMEMİŞ ÜÇÜNCÜ: alt+tab ya da odak
        // kaybı Up karesini yutabilir; yutulduğu gün jest sonsuza kadar etkin
        // kalır ve harita fare bırakılmış olmasına rağmen imleci takip eder.
        // BASILI TUTMA SON: eşik kararı, öncesi ve sonrası karşılaştırılarak.
        public BoardPointerAction Advance(
            bool pressed, bool held, bool released, float x, float y, bool pressBlocked)
        {
            if (pressed)
            {
                // ARAYÜZÜN ÜSTÜNDE BAŞLAYAN BASIŞ JESTİ HİÇ DOĞURMAZ: eski
                // hâlde yalnız o karenin tıklaması iptal ediliyordu, çünkü
                // tıklama zaten tek kareydi. Sürükleme çok kareli: iptal
                // basışta verilmezse oyuncu üretim düğmesine basıp fareyi
                // sürükleyerek haritayı kaydırırdı.
                if (pressBlocked)
                {
                    gesture.Reset();
                    return BoardPointerAction.None;
                }

                gesture.Press(x, y);
            }

            if (!gesture.IsActive)
            {
                return BoardPointerAction.None;
            }

            if (released)
            {
                // ██ EŞİK AŞILDIYSA TIKLAMA YOK — VE BEDELİ GİZLENMİYOR ██
                // PointerGesture.Release bırakma konumunu da eşikten geçirir,
                // yani tek karede eşiği aşan hızlı bir savurma PanBegin hiç
                // görmeden PanEnd üretir. Kameranın tarafında zararsız:
                // EndPan tutamağı zaten olmayan bir sürüklemeden bırakır.
                return gesture.Release(x, y) == PointerPhase.DragReleased
                    ? BoardPointerAction.PanEnd
                    : BoardPointerAction.Click;
            }

            if (!held)
            {
                bool wasPanning = gesture.Phase == PointerPhase.Dragging;
                gesture.Reset();
                return wasPanning ? BoardPointerAction.PanEnd : BoardPointerAction.None;
            }

            // KAYDIRMANIN BAŞLADIĞI KARE, İKİNCİ BİR BAYRAKLA DEĞİL, JESTİN
            // ÖNCESİ VE SONRASI KARŞILAŞTIRILARAK bulunuyor. Ayrı bir bool
            // alan olsaydı aynı gerçeğin iki kaynağı doğar ve biri
            // güncellenmediği gün kod yine derlenirdi.
            PointerPhase before = gesture.Phase;
            PointerPhase after = gesture.MoveTo(x, y);

            if (after != PointerPhase.Dragging)
            {
                return BoardPointerAction.None;
            }

            return before == PointerPhase.Dragging
                ? BoardPointerAction.PanContinue
                : BoardPointerAction.PanBegin;
        }

        /// <summary>
        /// Sürmekte olan jesti iptal eder ve gerekiyorsa kaydırmayı kapatır.
        /// Yerleştirme kipi işaretçiyi sahiplendiğinde ve tahta kapanırken
        /// çağrılır.
        /// </summary>
        /// <returns>
        /// Kaydırma sürüyorduysa <see cref="BoardPointerAction.PanEnd"/>,
        /// aksi hâlde <see cref="BoardPointerAction.None"/>.
        /// </returns>
        // İPTAL BİR EYLEM DÖNDÜRÜYOR, void DEĞİL: kaydırma ortasında B tuşuna
        // basan oyuncunun kamerası, kip açıldıktan sonra da imleci takip
        // etmeye devam ederdi — iptalin kameraya söylenecek bir sözü var.
        public BoardPointerAction Cancel()
        {
            bool wasPanning = gesture.Phase == PointerPhase.Dragging;
            gesture.Reset();
            return wasPanning ? BoardPointerAction.PanEnd : BoardPointerAction.None;
        }
    }
}
