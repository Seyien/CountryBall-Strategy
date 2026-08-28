using GridStrategy.Core;

namespace GridStrategy.Unity
{
    //   ═══ KİP GEÇİŞ ŞEMASI — TEK MAKİNE, ÜÇ KİP ════════════════════
    //
    //                        ┌─────────┐
    //          B tuşu   ┌────┤  BOŞTA  ├────┐  uzak hedefe tıklama
    //                   ▼    └─────────┘    ▼
    //        ┌────────────────┐      ┌────────────────────┐
    //        │ YAPI           │      │ BEKLEYEN VURUŞ     │
    //        │ YERLEŞTİRME    │      │ (yürü, sonra vur)  │
    //        └───────┬────────┘      └─────────┬──────────┘
    //     iptal / yapı kondu          vuruş indi / emir düştü
    //                └──────► BOŞTA ◄──────────┘
    //   YAN GEÇİŞ YOK: B tuşu, emri geçişin kendisinde düşürür.

    // ═══ NEDEN TEK MAKİNE — ÖLÇÜLDÜ, VARSAYILMADI ════════════════════
    // İki ayrı makine kurulacaktı; ölçüm onu çürüttü. Yerleştirme kipine
    // girmek bekleyen vuruşu her koşulda iptal ediyor, ve ters yön de kapalı:
    // bekleyen vuruşu yazan tek satır HandleOccupiedCellClick'in içinde ve o
    // yola ancak Update'in yerleştirme dalından ÇIKAMADIĞI karelerde
    // varılıyor. Yani "yerleştirme açık" ile "emir yazılı" aynı karede asla
    // birlikte doğru olamaz; iki bayrak değil, bir kipin iki değeriydi.
    //
    // KAZANCI SATIRLA ÖLÇÜLEBİLİR: iptal artık hiçbir yerde elle yazılmıyor,
    // geçişin kendisi yapıyor — açık kipin Cik() işi emri siliyor.

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

        /// <summary>
        /// Bu tıklamayı kip kendisi mi yutuyor?
        /// </summary>
        /// <param name="clicked">Tıklanan hücrede duran kimlik; boşsa null.</param>
        /// <returns>Tıklama burada bitiyorsa true; sıradan akışa gidecekse false.</returns>
        bool ConsumesClick(Unit clicked);
    }
}
