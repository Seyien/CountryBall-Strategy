using NUnit.Framework;
using GridStrategy.Unity;

namespace GridStrategy.Tests.EditMode.Unity
{
    /// <summary>
    /// <b>SOL TUŞUN ÜÇ ANLAMI BURADA AYRILIYOR.</b> Bir basış haritayı
    /// kaydırabilir, tahtaya tıklayabilir ya da hiçbir şey yapmayabilir; hangisi
    /// olduğuna <see cref="BoardPointerArbiter"/> karar veriyor ve bu dosya o
    /// kararı ölçüyor.
    ///
    /// <b>NEDEN SINANABİLİYOR.</b> EditMode'da fare girdisi AKMIYOR: hiçbir test
    /// <c>Input.GetMouseButtonDown</c>'ı doğru döndüremez. Hakem üç bool'u
    /// DIŞARIDAN aldığı için o girdi burada elle üretiliyor — tipin sade bir C#
    /// sınıfı olmasının bütün kazancı bu satırda.
    ///
    /// <b>SINANMAYAN ŞEY:</b> kararın KAMERAYA ulaşması. Onun sahibi
    /// <c>BoardAdapter.ApplyPointerAction</c> ve orada <c>Camera</c> ile
    /// <c>Input</c> var; yeri PlayMode, burası değil.
    /// </summary>
    public sealed class BoardPointerArbiterTests
    {
        // Eşik 1 birim: aşağıdaki testlerin hepsi bu sayıya göre okunmalı.
        // 0,5 "eşiğin altı", 2 "eşiğin üstü" demektir ve ikisi de kasten
        // sınırdan uzak — tam eşik üstündeki davranış PointerGesture'ın işi ve
        // orada zaten sınanıyor.
        private const float Threshold = 1f;

        private static BoardPointerArbiter NewArbiter() => new BoardPointerArbiter(Threshold);

        // Bir karenin fare hâlini okunur kılan üç yardımcı. Çıplak
        // `Advance(true, true, false, ...)` çağrılarında hangi bool'un hangisi
        // olduğu okunmuyordu ve yanlış sıralanmış bir çağrı derlenip yeşil
        // kalırdı.
        private static BoardPointerAction Press(BoardPointerArbiter arbiter, float x, float y, bool blocked = false)
            => arbiter.Advance(true, true, false, x, y, blocked);

        private static BoardPointerAction Hold(BoardPointerArbiter arbiter, float x, float y)
            => arbiter.Advance(false, true, false, x, y, false);

        private static BoardPointerAction Release(BoardPointerArbiter arbiter, float x, float y)
            => arbiter.Advance(false, false, true, x, y, false);

        // ══ BASIŞ: HİÇBİR ŞEY OLMAZ ══════════════════════════════════════

        /// <summary>
        /// Sol tuşa basmak tek başına ne kaydırır ne tıklar.
        /// </summary>
        // BU TEST BİR OYUN HATASINI KİLİTLİYOR ve hatanın adı var: eski hâlde
        // BoardAdapter.Update basma karesinde HandleClick'i çağırıyordu, yani
        // sürüklemeye başlamak için basmak bile bir birim seçiyordu. Kaydırma
        // ancak basış SESSİZ kalırsa mümkün.
        [Test]
        public void Advance_PressAlone_DoesNothing()
        {
            var arbiter = NewArbiter();

            Assert.That(Press(arbiter, 0f, 0f), Is.EqualTo(BoardPointerAction.None));
        }

        /// <summary>
        /// Eşiğin altında kalan bir hareket hâlâ bir tıklama adayıdır.
        /// </summary>
        [Test]
        public void Advance_HeldBelowThreshold_DoesNotPan()
        {
            var arbiter = NewArbiter();
            Press(arbiter, 0f, 0f);

            Assert.That(Hold(arbiter, 0.5f, 0f), Is.EqualTo(BoardPointerAction.None));
        }

        // ══ TIKLAMA: BIRAKMA KARESİNDE ═══════════════════════════════════

        /// <summary>
        /// Eşik hiç aşılmadan bırakılan bir jest tıklamadır.
        /// </summary>
        [Test]
        public void Advance_ReleasedBelowThreshold_Clicks()
        {
            var arbiter = NewArbiter();
            Press(arbiter, 0f, 0f);
            Hold(arbiter, 0.3f, 0f);

            Assert.That(Release(arbiter, 0.5f, 0f), Is.EqualTo(BoardPointerAction.Click));
        }

        /// <summary>
        /// Aynı karede hem basılıp hem bırakılan çok hızlı bir tıklama
        /// kaybolmaz.
        /// </summary>
        // BU TEST BİR GERİLEMEYİ KİLİTLİYOR: basış dalı bırakıştan önce
        // koşmasaydı o kare yalnız Press görür, jest etkin kalır ve tıklama bir
        // sonraki karede odak-kaybı dalına düşüp sessizce yutulurdu.
        [Test]
        public void Advance_PressedAndReleasedInTheSameFrame_Clicks()
        {
            var arbiter = NewArbiter();

            Assert.That(
                arbiter.Advance(true, false, true, 0f, 0f, false),
                Is.EqualTo(BoardPointerAction.Click));
        }

        // ══ KAYDIRMA: EŞİĞİN ÜSTÜ ════════════════════════════════════════

        /// <summary>
        /// Eşiği aşan ilk kare kaydırmayı BAŞLATIR, sonraki kareler SÜRDÜRÜR.
        /// </summary>
        // İKİ DEĞERİN AYRI OLMASI KAMERANIN İHTİYACI: BeginPan tutamağı kurar,
        // ContinuePan yalnız noktayı taşır. Tek bir "Pan" değeri olsaydı kamera
        // her karede tutamağı yeniden kurar ve harita hiç kımıldamazdı.
        [Test]
        public void Advance_HeldPastThreshold_BeginsThenContinuesPan()
        {
            var arbiter = NewArbiter();
            Press(arbiter, 0f, 0f);

            Assert.That(Hold(arbiter, 2f, 0f), Is.EqualTo(BoardPointerAction.PanBegin));
            Assert.That(Hold(arbiter, 3f, 0f), Is.EqualTo(BoardPointerAction.PanContinue));
            Assert.That(Hold(arbiter, 4f, 0f), Is.EqualTo(BoardPointerAction.PanContinue));
        }

        /// <summary>
        /// Eşiğin üstünden geri dönen bir işaretçi hâlâ kaydırıyordur.
        /// </summary>
        // TEK YÖNLÜLÜK PointerGesture'ın kuralı; buradaki test onun hakem
        // tarafından KORUNDUĞUNU ölçüyor: geri dönüşte PanBegin'e düşülseydi
        // kamera tutamağı ortada yeniden kurar ve harita zıplardı.
        [Test]
        public void Advance_DraggedBackInsideThreshold_KeepsPanning()
        {
            var arbiter = NewArbiter();
            Press(arbiter, 0f, 0f);
            Hold(arbiter, 2f, 0f);

            Assert.That(Hold(arbiter, 0.1f, 0f), Is.EqualTo(BoardPointerAction.PanContinue));
        }

        // ══ ÇAKIŞMANIN KENDİSİ — BU DOSYANIN ASIL TESTİ ══════════════════

        /// <summary>
        /// Eşik aşıldıktan sonra bırakmak KAYDIRMAYI BİTİRİR ve TIKLAMAZ.
        /// </summary>
        // ██ OPERATÖRÜN İSTEDİĞİ ÇAKIŞMASIZLIK TAM OLARAK BU SATIRDIR ██
        // Haritada gezinip parmağını kaldıran oyuncu, bıraktığı yerdeki birimi
        // SEÇMEMELİ. Bu test kırmızıya döndüğü gün oyunda görülecek şey şudur:
        // haritayı kaydır, bırak, ve altındaki düşman birimi seçilmiş olsun.
        [Test]
        public void Advance_ReleasedAfterDragging_EndsPanAndDoesNotClick()
        {
            var arbiter = NewArbiter();
            Press(arbiter, 0f, 0f);
            Hold(arbiter, 2f, 0f);

            BoardPointerAction action = Release(arbiter, 3f, 0f);

            Assert.That(action, Is.EqualTo(BoardPointerAction.PanEnd));
            Assert.That(action, Is.Not.EqualTo(BoardPointerAction.Click));
        }

        /// <summary>
        /// Tek karede eşiği aşan hızlı bir savurma da tıklamaz.
        /// </summary>
        // BEDELİ GİZLENMİYOR: bu jest hiç PanBegin görmedi, yani kamera hiç
        // kaymadı — ve yine de tıklama üretmiyor. PointerGesture.Release'in
        // bırakma konumunu da eşikten geçirmesinin doğrudan sonucu.
        [Test]
        public void Advance_FlickedPastThresholdOnRelease_DoesNotClick()
        {
            var arbiter = NewArbiter();
            Press(arbiter, 0f, 0f);

            Assert.That(Release(arbiter, 5f, 0f), Is.EqualTo(BoardPointerAction.PanEnd));
        }

        // ══ ARAYÜZ: BASIŞ ORADA BAŞLADIYSA JEST HİÇ DOĞMAZ ═══════════════

        /// <summary>
        /// Arayüzün üstünde başlayan bir basış ne kaydırır ne tıklar.
        /// </summary>
        // ÜÇ KAREYİ BİRDEN ÖLÇÜYOR ve sebebi şu: eski hâlde iptal edilen şey
        // TEK karelik tıklamaydı. Sürükleme çok kareli — yalnız ilk kareyi
        // iptal etmek, üretim düğmesine basıp fareyi sürükleyen oyuncunun
        // haritasını kaydırırdı.
        [Test]
        public void Advance_PressBlockedByUi_NeitherPansNorClicks()
        {
            var arbiter = NewArbiter();

            Assert.That(Press(arbiter, 0f, 0f, blocked: true), Is.EqualTo(BoardPointerAction.None));
            Assert.That(Hold(arbiter, 5f, 0f), Is.EqualTo(BoardPointerAction.None));
            Assert.That(Release(arbiter, 6f, 0f), Is.EqualTo(BoardPointerAction.None));
        }

        /// <summary>
        /// Tahtada başlayıp arayüzün üstünde biten bir kaydırma kesilmez.
        /// </summary>
        // ARAYÜZ SORUSU YALNIZ BASIŞ KARESİNDE SORULUYOR ve bu testin ölçtüğü
        // şey o kararın kendisi: her karede sorulsaydı harita, imleç panelin
        // kenarına değdiği an donardı.
        [Test]
        public void Advance_PressStartedOnBoard_KeepsPanningOverUi()
        {
            var arbiter = NewArbiter();
            Press(arbiter, 0f, 0f);
            Hold(arbiter, 2f, 0f);

            Assert.That(
                arbiter.Advance(false, true, false, 3f, 0f, true),
                Is.EqualTo(BoardPointerAction.PanContinue));
        }

        // ══ JESTİN YARIDA KALMASI ════════════════════════════════════════

        /// <summary>
        /// Bırakma karesi hiç gelmezse (odak kaybı) kaydırma yine de biter.
        /// </summary>
        // BU TEST BİR OYUN HATASINI KİLİTLİYOR: alt+tab, Up karesini yutar.
        // Yutulduğu gün jest sonsuza kadar etkin kalır ve oyuncu fareye
        // dokunmadan pencereye döndüğünde harita imleci takip etmeye devam
        // ederdi.
        [Test]
        public void Advance_ButtonVanishedWithoutRelease_EndsPan()
        {
            var arbiter = NewArbiter();
            Press(arbiter, 0f, 0f);
            Hold(arbiter, 2f, 0f);

            Assert.That(
                arbiter.Advance(false, false, false, 2f, 0f, false),
                Is.EqualTo(BoardPointerAction.PanEnd));
            Assert.That(arbiter.IsActive, Is.False);
        }

        /// <summary>
        /// Eşik aşılmadan yutulan bir bırakış tıklama ÜRETMEZ.
        /// </summary>
        // GÖRÜLMEYEN BİR BIRAKIŞ BİR NİYET DEĞİLDİR: oyuncu parmağını
        // kaldırdığını hiç söylemedi, pencere odağı kaydı. Buradan tıklama
        // üretmek, oyuncunun vermediği bir emri uydurmak olurdu.
        [Test]
        public void Advance_ButtonVanishedBeforeThreshold_DoesNotClick()
        {
            var arbiter = NewArbiter();
            Press(arbiter, 0f, 0f);

            Assert.That(
                arbiter.Advance(false, false, false, 0.2f, 0f, false),
                Is.EqualTo(BoardPointerAction.None));
            Assert.That(arbiter.IsActive, Is.False);
        }

        /// <summary>
        /// Boşta geçen kareler hiçbir şey üretmez.
        /// </summary>
        // BİR TIKLAMA İKİ KEZ OKUNMAZ: bırakma kipleri PointerGesture'da bir
        // sonraki Press'e kadar OKUNABİLİR KALIYOR, ve hakem o kalıcılığı
        // eyleme çevirseydi tek tıklama her karede yeniden tıklardı.
        [Test]
        public void Advance_IdleFramesAfterAClick_ProduceNothing()
        {
            var arbiter = NewArbiter();
            Press(arbiter, 0f, 0f);
            Release(arbiter, 0f, 0f);

            Assert.That(
                arbiter.Advance(false, false, false, 0f, 0f, false),
                Is.EqualTo(BoardPointerAction.None));
        }

        // ══ İPTAL — KİP İŞARETÇİYİ SAHİPLENDİĞİNDE ═══════════════════════

        /// <summary>
        /// Kaydırma sürerken iptal etmek kamerayı da bırakır.
        /// </summary>
        // BU TEST BİR OYUN HATASINI KİLİTLİYOR: kaydırırken B tuşuna basan
        // oyuncu yerleştirme kipine girer; iptal PanEnd döndürmeseydi kamera
        // hâlâ "sürüklüyor" durumunda kalır ve bina taşınırken harita da
        // kayardı.
        [Test]
        public void Cancel_WhilePanning_EndsPan()
        {
            var arbiter = NewArbiter();
            Press(arbiter, 0f, 0f);
            Hold(arbiter, 2f, 0f);

            Assert.That(arbiter.Cancel(), Is.EqualTo(BoardPointerAction.PanEnd));
            Assert.That(arbiter.IsActive, Is.False);
        }

        /// <summary>
        /// Kaydırma yokken iptal etmek kameraya hiçbir şey söylemez.
        /// </summary>
        [Test]
        public void Cancel_WhenNothingIsHappening_DoesNothing()
        {
            var arbiter = NewArbiter();

            Assert.That(arbiter.Cancel(), Is.EqualTo(BoardPointerAction.None));
        }

        /// <summary>
        /// Eşik aşılmadan iptal edilen bir basış tıklamaya dönüşmez.
        /// </summary>
        [Test]
        public void Cancel_WhilePressedBelowThreshold_DoesNotClick()
        {
            var arbiter = NewArbiter();
            Press(arbiter, 0f, 0f);

            Assert.That(arbiter.Cancel(), Is.EqualTo(BoardPointerAction.None));
        }

        /// <summary>
        /// İptal edilen bir jest, aynı düğme basılı kalsa bile geri dönmez.
        /// </summary>
        // İPTAL SONRASI DÜĞME HÂLÂ BASILI OLABİLİR: oyuncu kaydırırken B'ye
        // bastı ve sol tuşu bırakmadı. O basılı düğme jesti yeniden
        // BAŞLATABİLSEYDİ, kip kapanır kapanmaz harita hiçbir yeni basış
        // olmadan kaymaya devam ederdi.
        [Test]
        public void Advance_HeldAfterCancel_DoesNotResumePanning()
        {
            var arbiter = NewArbiter();
            Press(arbiter, 0f, 0f);
            Hold(arbiter, 2f, 0f);
            arbiter.Cancel();

            Assert.That(Hold(arbiter, 5f, 0f), Is.EqualTo(BoardPointerAction.None));
        }

        // ══ EŞİĞİN DOĞRULAMASI — SAHİBİNE DEVREDİLDİ ═════════════════════

        /// <summary>
        /// Bozuk bir eşikle hakem hiç kurulamaz.
        /// </summary>
        // İSTİSNA BU TİPTEN DEĞİL, PointerGesture'ın kurucusundan geliyor ve
        // test tam da bunu ölçüyor: doğrulama KOPYALANMADI. Kopyalansaydı iki
        // kapı bir gün ayrışır ve biri geçirdiğini öteki reddederdi.
        // Eşiğin kendi kuralları PointerGestureTests.Constructor_NegativeThreshold_Throws
        // içinde sınanıyor; buradaki test yalnız DEVRİN yapıldığını ölçüyor.
        [Test]
        public void Constructor_NegativeThreshold_Throws()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(
                () => new BoardPointerArbiter(-1f));
        }

        /// <summary>
        /// Sayı olmayan bir eşikle de hakem kurulamaz.
        /// </summary>
        [Test]
        public void Constructor_NaNThreshold_Throws()
        {
            Assert.Throws<System.ArgumentException>(
                () => new BoardPointerArbiter(float.NaN));
        }
    }
}
