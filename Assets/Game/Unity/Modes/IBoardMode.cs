namespace GridStrategy.Unity
{
    //   ═══ KİP GEÇİŞ ŞEMASI — TEK MAKİNE, İKİ KİP ═══════════════════
    //
    //                        ┌─────────┐
    //          B tuşu   ┌────┤  BOŞTA  │
    //                   ▼    └─────────┘
    //        ┌────────────────┐
    //        │ YAPI           │
    //        │ YERLEŞTİRME    │
    //        └───────┬────────┘
    //     iptal / yapı kondu
    //                └──────► BOŞTA
    //
    //   ÜÇÜNCÜ KİP (BEKLEYEN VURUŞ) BU TURDA KALDIRILDI ve yerini kip
    //   OLMAYAN bir şey aldı: emir defteri. Sebep tek cümlede —
    //   kip TAHTANIN ne yaptığıdır ve TEKTİR; emir HER BİRİME ne
    //   söylendiğidir ve ÇOĞULDUR. Bir savaşçının emri yazılıyken üç
    //   savaşçının daha emri yazılı olabilmeli, ve bu bir kip makinesine
    //   sığmaz. → Orders/IUnitOrder.cs

    // ═══ NEDEN TEK MAKİNE — ÖLÇÜLDÜ, VARSAYILMADI ════════════════════
    // Eski gerekçe şuydu: "yerleştirme açık" ile "emir yazılı" aynı karede asla
    // birlikte doğru olamaz, dolayısıyla iki bayrak değil bir kipin iki
    // değeridir. O ölçüm DOĞRUYDU ve bugün ARTIK GEÇERLİ DEĞİL — çünkü
    // istenen şeyin kendisi değişti: operatör, bina sürüklerken savaşçısının
    // vurmaya DEVAM etmesini istiyor. İkisi artık aynı karede birlikte doğru,
    // ve bu yüzden ikisi ayrı sahiplerde yaşıyor: kip makinesinde bir kip,
    // defterde n emir.
    //
    // MAKİNENİN KAZANCI DURUYOR: yerleştirmenin iptali hâlâ hiçbir çağıranın
    // hafızasında değil, geçişin kendisinde.

    // REDDEDILEN - RemoveSelected'ı dördüncü bir kip yapmak.
    // KIRILAN: kaldırma tek çağrıda başlayıp biten bir eylem; kareler arasında
    // yaşayan hiçbir alanı yok ve bir sonraki tıklamanın anlamını
    // değiştirmiyor. Kip yapılsaydı Gir ile Cik aynı satırda koşar, makineye
    // hiçbir zaman gözlenemeyen bir durum eklenirdi.
    // KAZANIRDI: silme işleminin de bir onay adımı olsaydı (çöp kutusuna
    // tıkla, sonra hedefi seç) o bekleme gerçekten bir kip olurdu.
    // TEK CUMLE: kip, kareler arasında YAŞAYAN bir cevaptır ve kaldırmanın
    // bugün yaşayacak bir cevabı yok.

    /// <summary>
    /// Tahtanın o andaki KİPİ: bir tıklamanın ne anlama geldiğini ve karenin
    /// ne iş yaptığını bu tip belirler.
    ///
    /// OYUNDA NE İŞE YARAR: aynı fare tıklaması bir kipte birim seçer, ötekinde
    /// bina koyar. Cevabı veren yer artık dağınık bayraklar değil, o anda
    /// yürürlükte olan tek nesnedir.
    /// </summary>
    // MonoBehaviour DEĞİL ve bu tasarımın asıl kazancı: kipler kendi Update'ini
    // almaz, tahta onları çağırır. Bunun ölçülebilir sonucu şu — üç kip de
    // EditMode'da `new` ile kurulup sınanabiliyor, sahne kurmaya gerek yok.
    //
    // ARAYÜZ BİLEREK DÖRT ÜYE: bugün tahtanın kiplere sorduğu soru bu kadar.
    // Beşinci bir üye (örneğin "kipin adı") kimsenin sormadığı bir soru olurdu.
    public interface IBoardMode
    {
        /// <summary>
        /// Bu kip fareyi ve klavyeyi TEK BAŞINA mı sahipleniyor?
        /// </summary>
        // ÜÇ SORUYU TEK ÜYE CEVAPLIYOR ve üçü de aynı cevabı istiyor: sıradan
        // tıklama akışı çalışmaz, imleç çerçevesi kapanır, dışarıdan gelen
        // hayalet yazarı geri çekilir. Üçünü ayrı üyelere bölmek bugün hiçbir
        // kipte farklı cevap üretmezdi.
        bool OwnsPointer { get; }

        /// <summary>
        /// Kipe girildiğinde bir kez koşar: ekranı ve girdiyi kipe hazırlar.
        /// </summary>
        void Enter();

        /// <summary>
        /// Kipten çıkarken bir kez koşar: kipin bıraktığı her izi toplar.
        /// </summary>
        // GEÇİŞİN TEMİZLİĞİ BURADA, çağıranda DEĞİL: eski hâlde "yerleştirmeye
        // girerken bekleyen vuruşu iptal et" satırı elle yazılıydı ve
        // yazılmadığı her yeni geçiş sessiz bir hata olurdu.
        void Exit();

        /// <summary>
        /// Kipin kare başına işi. Zaman ve girdi tahtadan gelir.
        /// </summary>
        void Advance();

        // ═══ BURADA DÖRDÜNCÜ BİR ÜYE VARDI: ConsumesClick(Unit) ═══════
        // Bir önceki turda eklenmişti ve tek çağıranı vardı: "aynı hedefe gelen
        // ikinci tıklama yazılı emrin tekrarı mı" sorusu. O soruyu bugün emir
        // defteri cevaplıyor — ve DAHA DOĞRU cevaplıyor, çünkü soru artık
        // "tahtada yazılı emir" değil "SEÇİLİ BİRİMİN emri" hakkında.
        // Geriye kalan iki kip de koşulsuz false dönüyordu; kimsenin sormadığı
        // bir soruya iki uydurma cevap, arayüzü yalancı yapardı.
        // → BoardAdapter.RepeatsOrder, Orders/UnitOrderBook.cs
    }
}
