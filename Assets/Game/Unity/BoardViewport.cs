using UnityEngine;

namespace GridStrategy.Unity
{
    /// <summary>
    /// Kameranın tahtayı nereye kadar kaydırıp ne kadar yakınlaştırabileceğini
    /// hesaplayan kurallar.
    ///
    /// OYUNDA NE İŞE YARAR: oyuncu haritayı sürükleyerek gezebilir ve tekerlekle
    /// yakınlaşabilir, ama tahtayı ekrandan kaçıramaz — en kötü hâlde bile
    /// tahtanın bir köşesi görünür kalır.
    ///
    /// MOTOR ÇAĞRISI YOK: içeride ne <c>Camera</c> var ne <c>Transform</c>, yalnız
    /// sayı girip sayı çıkıyor. Sahnesiz sınanabilmesinin ve aynı kuralın hem
    /// çalışma zamanında hem Editor aracında kullanılabilmesinin tek sebebi bu.
    /// </summary>
    // ██ NEDEN MonoBehaviour DEĞİL ██
    // Kaydırma ve yakınlaştırma iki AYRI iş: girdiyi okumak (fare, tekerlek,
    // kare) ile sınırı hesaplamak. Birincisi motora bağlı, ikincisi saf
    // aritmetik. Tek sınıfa konsalardı "ekranın dışına taşmıyor mu" sorusu
    // ancak Play tuşuna basıp gözle bakarak cevaplanabilirdi.
    // Girdi tarafı: BoardCameraRig.
    public static class BoardViewport
    {
        /// <summary>
        /// Kurulumda çerçevelenen dünya dikdörtgeni, BAŞKA bir en boy oranında
        /// da tümüyle görünsün diye gereken yarım yükseklik.
        /// </summary>
        /// <param name="homeHalfHeight">Kurulumda yazılan <c>orthographicSize</c>.</param>
        /// <param name="homeAspect">Kurulum anındaki en boy oranı.</param>
        /// <param name="aspect">Şu ANKİ en boy oranı.</param>
        // ██ OPERATÖRÜN BELİRTİSİ: "%50 yaptığımda düzgün ortalanmıyor" ██
        // KÖK SEBEP: çerçeveleme Editor aracında BİR KEZ, o andaki en boy
        // oranıyla hesaplanıyor ve bir daha hiç sorulmuyordu. Game penceresi
        // daraldığında kamera aynı yarım yüksekliği koruyor, yani YATAYDA daha
        // az dünya gösteriyor ve tahtanın kenarları dışarıda kalıyordu.
        //
        // ÇÖZÜM ORANA DEĞİL, DİKDÖRTGENE BAKIYOR: kurulumun çerçevelediği dünya
        // dikdörtgeni (yarım genişlik = homeHalfHeight × homeAspect) sabit bir
        // GERÇEK; ekran oranı değiştiğinde korunması gereken şey o dikdörtgen.
        // Dar bir ekranda aynı genişliği göstermek için yükseklik BÜYÜR.
        //
        // REDDEDİLEN — panel paylarını çalışma zamanında yeniden hesaplamak:
        //     float leftInset = (ScreenMargin + PaletteWidth()) / ReferenceWidth;
        //     ...  // SceneSetupTool.FrameCamera'nın kopyası
        // KIRDIĞI ŞEY: o dört sayının sahibi arayüzü KURAN araç. İkinci bir
        // kopya çalışma zamanına inseydi aynı nicelik iki yerde yazılabilir
        // olurdu ve panel genişliği değiştiği gün ikisi sessizce ayrışırdı.
        // Bugün araç hesaplıyor, kamera yalnız OKUYOR.
        // NE ZAMAN KAZANIRDI: paneller çalışma zamanında yeniden boyutlanmaya
        // başladığı gün — o gün pay artık sabit değil, bir olaydır.
        public static float FitHalfHeight(float homeHalfHeight, float homeAspect, float aspect)
        {
            if (homeHalfHeight <= 0f || homeAspect <= 0f || aspect <= 0f)
            {
                return homeHalfHeight;
            }

            float framedHalfWidth = homeHalfHeight * homeAspect;
            return Mathf.Max(homeHalfHeight, framedHalfWidth / aspect);
        }

        /// <summary>
        /// Yakınlaştırmanın iki ucunu da kelepçeler.
        /// </summary>
        /// <param name="wantedHalfHeight">Tekerleğin istediği yarım yükseklik.</param>
        /// <param name="minHalfHeight">En yakın hâl; bundan küçüğü yok.</param>
        /// <param name="maxHalfHeight">En uzak hâl; bundan büyüğü yok.</param>
        // İKİ UÇ DA GEREKLİ, biri değil: alt sınır olmasaydı tekerlek kamerayı
        // tek bir pikselin içine sokardı, üst sınır olmasaydı tahta ekranda bir
        // noktaya inerdi. Operatörün cümlesi ikisini birden istiyor: "hem
        // uzaklaştırma hem yakınlaştırma olayında da limitimiz olması gerekiyor."
        //
        // SIRA TERS YAZILMIŞSA min KAZANIYOR: Clamp'e ters aralık verilirse
        // Unity sessizce max'ı döndürür. Burada önce max'a, sonra min'e
        // kelepçelemek, yanlış ayarlanmış bir Inspector'da bile kameranın
        // kullanılabilir kalmasını sağlıyor.
        public static float ClampHalfHeight(
            float wantedHalfHeight, float minHalfHeight, float maxHalfHeight)
        {
            float clamped = Mathf.Min(wantedHalfHeight, maxHalfHeight);
            return Mathf.Max(clamped, minHalfHeight);
        }

        /// <summary>
        /// Yakınlaştırmadan sonra, imlecin altındaki dünya noktası YERİNDE
        /// kalsın diye kameranın gitmesi gereken merkez.
        /// </summary>
        /// <param name="centre">Yakınlaştırmadan ÖNCEKİ kamera merkezi.</param>
        /// <param name="pointerWorld">İmlecin altındaki dünya noktası, zoom ÖNCESİ.</param>
        /// <param name="oldHalfHeight">Önceki yarım yükseklik.</param>
        /// <param name="newHalfHeight">Kelepçelenmiş yeni yarım yükseklik.</param>
        // ██ OPERATÖRÜN BELİRTİSİ: "tek bir noktaya yakınlaşıyor, farenin ██
        // ██ bulunduğum noktaya doğru olmuyor" ██
        // KÖK SEBEP: yakınlaştırma yalnız orthographicSize'ı değiştiriyordu.
        // Ortografik bir kamerada ekranın MERKEZİ sabit kalır, yani her
        // yakınlaşma tahtanın ortasına doğru gider — imleç nerede olursa olsun.
        // Haritada bir köşeyi incelemek isteyen oyuncu, yakınlaştıkça oradan
        // uzaklaşıyordu.
        //
        // TÜREVİ: ortografik izdüşümde ekrandaki bir noktanın dünya karşılığı
        // merkeze olan uzaklığıyla DOĞRU ORANTILI. Yarım yükseklik k katına
        // çıkarsa, merkeze olan her uzaklık da k katına çıkar. İmleç noktasını
        // sabit tutmak için merkezin ondan olan uzaklığını aynı k ile ölçeklemek
        // yeterli:
        //
        //     yeni merkez = imlec + (eski merkez - imlec) * k
        //
        // AÇI YOK, EKRAN ÇÖZÜNÜRLÜĞÜ YOK, en boy oranı YOK: üçü de hem eski hem
        // yeni durumda aynı olduğu için sadeleşiyor. Perspektif bir kamerada bu
        // sadeleşme olmazdı ve formül çalışmazdı.
        //
        // REDDEDİLEN — zoom'dan sonra ScreenToWorldPoint'i ikinci kez okumak:
        //     Vector3 before = cam.ScreenToWorldPoint(Input.mousePosition);
        //     cam.orthographicSize = ...;
        //     Vector3 after = cam.ScreenToWorldPoint(Input.mousePosition);
        //     cam.transform.position += before - after;
        // KIRDIĞI ŞEY: doğru sonucu verir ama kuralı KAMERAYA bağlar; sahnesiz
        // sınanamaz ve bu dosyanın "motor çağrısı yok" sözü düşerdi. Aynı sonuç,
        // yukarıdaki tek satırla ölçülebilir biçimde alınıyor.
        // NE ZAMAN KAZANIRDI: kamera perspektife dönseydi — o gün oran sabit
        // olmaz ve formül yerine gerçek izdüşüm gerekirdi.
        public static Vector2 ZoomTowards(
            Vector2 centre, Vector2 pointerWorld, float oldHalfHeight, float newHalfHeight)
        {
            if (oldHalfHeight <= 0f)
            {
                return centre;
            }

            float k = newHalfHeight / oldHalfHeight;
            return pointerWorld + ((centre - pointerWorld) * k);
        }

        /// <summary>
        /// Kameranın merkezini, tahtadan en az <paramref name="minVisible"/>
        /// birim görünür kalacak şekilde kelepçeler.
        /// </summary>
        /// <param name="wantedCentre">Sürüklemenin götürmek istediği merkez.</param>
        /// <param name="board">Tahtanın dünya dikdörtgeni.</param>
        /// <param name="halfHeight">Kameranın yarım yüksekliği.</param>
        /// <param name="aspect">En boy oranı; yarım genişlik bundan türüyor.</param>
        /// <param name="minVisible">
        /// Her eksende görünür kalması gereken en az tahta uzunluğu, dünya birimi.
        /// </param>
        // ██ OPERATÖRÜN KURALI: "GTA'da karakteri belli bir yere kadar ██
        // ██ götürebiliyorsun" — ama burada duvar tahtanın KENARI değil ██
        //
        // KLASİK ÇÖZÜM BU TAHTADA ÇALIŞMIYOR ve sebebi ölçülebilir. Alışılmış
        // kural "görüş dikdörtgeni tahtanın İÇİNDE kalsın"dır:
        //     cx = Mathf.Clamp(cx, board.xMin + halfWidth, board.xMax - halfWidth);
        // KIRDIĞI ŞEY: bu tahta 10x5 ve kamera onu tümüyle görüyor, yani
        // halfWidth tahtanın yarısından BÜYÜK. Aralık ters dönüyor, Clamp tek
        // bir noktaya çöküyor ve KAYDIRMA HİÇ OLMUYOR. Operatörün istediği şey
        // tam da o küçük tahtada gezebilmek.
        // NE ZAMAN KAZANIRDI: tahta ekrandan büyüdüğü gün — 40x40 bir harita
        // için doğru kural odur, bunun için değil.
        //
        // BURADAKİ KURAL ÖRTÜŞMEYE BAKIYOR: görüş dikdörtgeni ile tahta
        // dikdörtgeni her eksende en az minVisible kadar KESİŞSİN. Operatörün
        // cümlesi bu: "en sol köşeye kaydık, en sağ üstte en azından tuğlaları
        // görmemiz lazım." Kesişim şartı iki uçlu bir aralığa dönüşüyor ve
        // kamera o aralıkta serbestçe geziyor.
        //
        //     tahta  [bMin ─────────── bMax]
        //     goru        [cx-h ─── cx+h]
        //     sart:  ortusme >= minVisible
        //     =>     cx <= bMax + h - minVisible
        //            cx >= bMin - h + minVisible
        public static Vector2 ClampCentre(
            Vector2 wantedCentre, Rect board, float halfHeight, float aspect, float minVisible)
        {
            float halfWidth = halfHeight * (aspect > 0f ? aspect : 1f);

            return new Vector2(
                ClampAxis(wantedCentre.x, board.xMin, board.xMax, halfWidth, minVisible),
                ClampAxis(wantedCentre.y, board.yMin, board.yMax, halfHeight, minVisible));
        }

        /// <summary>
        /// Tek eksende kelepçeleme.
        /// </summary>
        // AYRI BİR ÜYE, İKİ KEZ YAZILMIŞ BİR GÖVDE DEĞİL: iki eksen aynı kuralı
        // farklı yarım uzunlukla uyguluyor ve kopyalansaydı biri düzeltildiğinde
        // öteki sessizce eski kalırdı.
        private static float ClampAxis(
            float wanted, float min, float max, float halfExtent, float minVisible)
        {
            // İSTENEN ÖRTÜŞME TAHTADAN BÜYÜKSE tahtanın kendisi tavan olur.
            // Bu kapı olmasaydı 5 birimlik bir tahtada minVisible 8 yazılınca
            // aralık ters döner ve kamera tahtadan UZAKLAŞMAYA zorlanırdı.
            float required = Mathf.Min(minVisible, max - min);

            float lower = min - halfExtent + required;
            float upper = max + halfExtent - required;

            // ARALIK YİNE DE TERS DÖNEBİLİR: görüş alanı tahtadan çok küçükse
            // (çok yakınlaşma) lower, upper'ı geçer. O hâlde tek doğru cevap
            // tahtanın ortası — kamera tahtanın içinde kalır.
            if (lower > upper)
            {
                return (min + max) * 0.5f;
            }

            return Mathf.Clamp(wanted, lower, upper);
        }
    }
}
