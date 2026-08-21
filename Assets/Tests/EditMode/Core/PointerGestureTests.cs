using System;
using GridStrategy.Core;
using NUnit.Framework;

namespace GridStrategy.Tests.EditMode.Core
{
    /// <summary>
    /// Burada sınanan şey bir durum makinesinin mekaniği değil, üç KARAR:
    ///
    /// <list type="bullet">
    /// <item>tıklama ile sürükleme AYRI iki sonuç üretir — birleştirilirse
    /// öğrenenin istediği iki giriş şeklinden biri sessizce ölür</item>
    /// <item>Dragging'e bir kez geçildiyse GERİ DÖNÜLMEZ — geri dönerse eşiğin
    /// üstünde titreyen bir el, aynı hareketle her seferinde farklı sonuç
    /// alır</item>
    /// <item>ölçüm basma noktasından yapılır ve tam eşikte hâlâ tıklamadır —
    /// sınırın hangi tarafta olduğu bir seçimdir, kaza değil</item>
    /// </list>
    ///
    /// Üçü de kırılsa kod derlenir, oyun açılır ve hiçbir hata görünmez;
    /// yalnızca giriş yanlış davranır. Bu dosyanın var olma sebebi tam olarak
    /// o sessizliktir.
    ///
    /// Motor YOK: ne Unity, ne Input, ne Time, ne sahne. Jest dört float'tan
    /// ibaret olduğu için sınavı da dört float'tan ibaret — bu, tipin saf
    /// tutulmasının bedeli değil, KARŞILIĞIDIR.
    /// </summary>
    // REDDEDILEN - PointerGestureTests.cs:47 yerine (jest gerçek bir girdi
    //              üzerinden, PlayMode'da sınanır):
    //     [UnityTest]
    //     public IEnumerator Drag_PlacesTheStructure()
    //     {
    //         yield return new WaitForSeconds(0.1f);
    //         // fare olayları enjekte edilir, BoardAdapter üzerinden okunur
    //     }
    // KIRILAN  : kırılan şey KANITIN NEYE AİT OLDUĞU.
    //            test kırmızıya döner -> eşik kararı mı, kamera çevirisi mi,
    //            sahne kurulumu mu, kare zamanlaması mı ayırt edilemez
    //            "tam eşikte hâlâ tıklamadır" sınırı -> enjekte edilen fare
    //            konumlarıyla piksel hassasiyetinde hiç kurulamaz
    //            derleyici: hiçbir şey der  .  test: bazen geçer, bazen geçmez
    // KAZANIRDI: sınanan şey KARAR değil ENTEGRASYON olsaydı — "üçlü çağrı
    //            gerçekten doğru sırayla besleniyor mu" sorusu bu dosyada
    //            cevaplanamaz; sahibi BoardAdapter.FeedGesture ve yeri PlayMode.
    // TEK CUMLE: Bir test kırmızıya döndüğünde neyin kırıldığını söyleyemiyorsa,
    //            ölçtüğü şey karar değil ortamdır.
    public sealed class PointerGestureTests
    {
        // Bütün testlerde aynı eşik: 5. 3-4-5 üçlüsü sayesinde "tam eşik"
        // durumu float'ta KESİN olarak kurulabiliyor (9 + 16 = 25), yani
        // sınır testi yuvarlama şansına bağlı değil.
        private const float Threshold = 5f;

        private static PointerGesture NewGesture()
        {
            return new PointerGesture(Threshold);
        }

        // ─── kurucu ve doğrulama ──────────────────────────────────────

        [Test]
        public void Constructor_StartsIdle()
        {
            var gesture = NewGesture();

            Assert.That(gesture.Phase, Is.EqualTo(PointerPhase.Idle));
            Assert.That(gesture.IsActive, Is.False);
        }

        [TestCase(-0.001f)]
        [TestCase(-1f)]
        [TestCase(float.NegativeInfinity)]
        public void Constructor_NegativeThreshold_Throws(float dragThreshold)
        {
            Assert.That(
                () => new PointerGesture(dragThreshold),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// NaN KORUMASINI koruyan test. NaN her karşılaştırmada <c>false</c>
        /// verdiği için negatif kontrolünden yara almadan geçer; geçtiği gün
        /// eşiğin karesi de NaN olur ve jest hiçbir zaman sürüklemeye dönmez.
        ///
        /// Kırmızıya dönerse kurucudaki NaN kontrolü kalkmış demektir ve
        /// kaybedilen şey bir istisna değil, GİRİŞİN YARISI: fırlatmayan,
        /// derlenen ve sürüklemeyi hiç tanımayan bir oyun.
        /// </summary>
        [Test]
        public void Constructor_NaNThreshold_Throws()
        {
            Assert.That(
                () => new PointerGesture(float.NaN),
                Throws.TypeOf<ArgumentException>(),
                "a NaN threshold silently disables dragging instead of failing");
        }

        /// <summary>
        /// ASİMETRİ KARARINI koruyan test. Sıfır eşik geçerlidir ve "en ufak
        /// kıpırdama bile sürüklemedir" demektir — <c>AttackProfile</c>'daki
        /// <c>range &lt; 1</c> eşiği buraya KOPYALANMADI, çünkü hiçbir hücreye
        /// ulaşamayan bir saldırı anlamsızken hiç toleransı olmayan bir eşik
        /// anlamlıdır. Aynı asimetri <c>MoveProfile</c> ile <c>AttackProfile</c>
        /// arasında zaten yaşıyor.
        ///
        /// Kırmızıya dönerse biri üç tipi "tutarlılık" adına simetrik hâle
        /// getirmiş demektir; kaybedilen şey hassas girişin ifade edilebilirliği.
        /// </summary>
        [Test]
        public void Constructor_ZeroThreshold_IsAllowed()
        {
            var gesture = new PointerGesture(0f);
            gesture.Press(0f, 0f);

            Assert.That(
                gesture.MoveTo(0.0001f, 0f),
                Is.EqualTo(PointerPhase.Dragging),
                "a zero threshold means every movement counts as a drag");
        }

        // ─── basma ────────────────────────────────────────────────────

        [Test]
        public void Press_EntersPressedPhase()
        {
            var gesture = NewGesture();

            Assert.That(gesture.Press(10f, 20f), Is.EqualTo(PointerPhase.Pressed));
            Assert.That(gesture.Phase, Is.EqualTo(PointerPhase.Pressed));
            Assert.That(gesture.IsActive, Is.True);
        }

        /// <summary>
        /// HOŞGÖRÜ KARARINI koruyan test. Bırakma olayı yutulmuşsa (odak
        /// kaybı, alt+tab) ikinci bir basma gelir; tip fırlatmak yerine yeni
        /// bir jest başlatır ve ölçümü YENİ noktadan yapar.
        ///
        /// Kırmızıya dönerse ya kurucuya bir <c>InvalidOperationException</c>
        /// dönmüş ya da eski basma noktası korunuyor demektir. İkincisi daha
        /// sinsi: jest, oyuncunun hiç geçmediği bir mesafeyi ölçer ve ilk
        /// kıpırdamada sürüklemeye atlar.
        /// </summary>
        [Test]
        public void Press_WhileAlreadyPressed_RestartsFromTheNewOrigin()
        {
            var gesture = NewGesture();
            gesture.Press(0f, 0f);

            Assert.That(gesture.Press(100f, 100f), Is.EqualTo(PointerPhase.Pressed));

            // Yeni noktadan 3 birim: eşiğin altında. Eski nokta korunsaydı
            // mesafe 100'ün üstünde olurdu ve kip Dragging'e atlardı.
            Assert.That(
                gesture.MoveTo(103f, 100f),
                Is.EqualTo(PointerPhase.Pressed),
                "a restarted gesture measures from the new press origin");
        }

        [Test]
        public void Press_AfterClickReleased_StartsANewGesture()
        {
            var gesture = NewGesture();
            gesture.Press(0f, 0f);
            gesture.Release(0f, 0f);

            Assert.That(gesture.Press(0f, 0f), Is.EqualTo(PointerPhase.Pressed));
            Assert.That(gesture.IsActive, Is.True);
        }

        // ─── eşik ─────────────────────────────────────────────────────

        [Test]
        public void MoveTo_BelowThreshold_StaysPressed()
        {
            var gesture = NewGesture();
            gesture.Press(0f, 0f);

            Assert.That(
                gesture.MoveTo(2f, 2f),
                Is.EqualTo(PointerPhase.Pressed),
                "a pointer that has not passed the threshold may still be a click");
        }

        /// <summary>
        /// SINIR KARARINI koruyan test. Karşılaştırma KESİN büyüktür: tam eşik
        /// kadar gidilmiş bir jest hâlâ tıklamadır. Eşik "bu kadar oynayabilirsin
        /// ve hâlâ tıklıyorsun" diye okunuyor, "buraya gelince sürüklüyorsun"
        /// diye değil.
        ///
        /// 3-4-5 üçlüsü bilerek seçildi: 9 + 16 = 25 float'ta KESİN, dolayısıyla
        /// bu test yuvarlamaya değil karara bakıyor. Kırmızıya dönerse ya
        /// karşılaştırma <c>&gt;=</c> olmuş ya da araya bir <c>Math.Sqrt</c>
        /// girip eşitliği yuvarlamanın eline bırakmıştır.
        /// </summary>
        [Test]
        public void MoveTo_ExactlyAtThreshold_StaysPressed()
        {
            var gesture = NewGesture();
            gesture.Press(0f, 0f);

            Assert.That(
                gesture.MoveTo(3f, 4f),
                Is.EqualTo(PointerPhase.Pressed),
                "the threshold distance itself is still inside click tolerance");
        }

        [Test]
        public void MoveTo_BeyondThreshold_BecomesDragging()
        {
            var gesture = NewGesture();
            gesture.Press(0f, 0f);

            Assert.That(gesture.MoveTo(0f, 6f), Is.EqualTo(PointerPhase.Dragging));
            Assert.That(gesture.IsActive, Is.True);
        }

        /// <summary>
        /// ÖLÇÜM NOKTASI KARARINI koruyan test. Mesafe her zaman BASMA
        /// noktasından ölçülür, bir önceki <c>MoveTo</c>'dan değil.
        ///
        /// Kırmızıya dönerse ölçüm adım adım yapılıyor demektir ve sonucu şu
        /// olur: yavaşça yüz birim sürüklenen bir işaretçi hiçbir adımda eşiği
        /// aşmaz, jest sonuna kadar "tıklama" kalır ve yavaş sürükleyen oyuncu
        /// için sürükle-bırak diye bir şey yoktur.
        /// </summary>
        [Test]
        public void MoveTo_MeasuresFromThePressOrigin_NotFromTheLastMove()
        {
            var gesture = NewGesture();
            gesture.Press(0f, 0f);

            // Her adım tek başına eşiğin altında (3 ve 3), toplam ise üstünde (6).
            Assert.That(gesture.MoveTo(3f, 0f), Is.EqualTo(PointerPhase.Pressed));
            Assert.That(
                gesture.MoveTo(6f, 0f),
                Is.EqualTo(PointerPhase.Dragging),
                "distance accumulates from the press origin, not step by step");
        }

        // ─── tek yönlülük ─────────────────────────────────────────────

        /// <summary>
        /// TEK YÖNLÜLÜK KARARINI koruyan test. Eşiği aşan bir jest, eşiğin
        /// içine geri dönse bile sürüklemedir.
        ///
        /// Kırmızıya dönerse kip her karede yeniden hesaplanıyor demektir.
        /// Bedeli iki katlı: (1) eşiğin tam üstünde titreyen bir el saniyede
        /// onlarca kez kip değiştirir ve bırakma anındaki tesadüfi konum
        /// sonucu belirler; (2) sürüklediği yapıyı beğenmeyip geri getiren
        /// oyuncu elini kaldırınca <c>ClickReleased</c> alır — yerleştirme
        /// bitmez, hayalet takip etmeye devam eder ve oyuncu bıraktığını sanır.
        /// </summary>
        [Test]
        public void MoveTo_BackInsideThreshold_StaysDragging()
        {
            var gesture = NewGesture();
            gesture.Press(0f, 0f);
            gesture.MoveTo(0f, 20f);

            Assert.That(
                gesture.MoveTo(1f, 1f),
                Is.EqualTo(PointerPhase.Dragging),
                "a pointer that once crossed the threshold is still dragging");
        }

        /// <summary>
        /// Aynı TEK YÖNLÜLÜK kararının uç hâli: işaretçi tam olarak başladığı
        /// noktaya geri döner. Mesafe sıfırdır ve yeniden hesaplayan her sürüm
        /// burada kesinlikle <c>Pressed</c> der.
        /// </summary>
        [Test]
        public void MoveTo_ReturningToPressOrigin_StaysDragging()
        {
            var gesture = NewGesture();
            gesture.Press(7f, 7f);
            gesture.MoveTo(7f, 40f);

            Assert.That(
                gesture.MoveTo(7f, 7f),
                Is.EqualTo(PointerPhase.Dragging),
                "returning to the origin does not undo a drag");
        }

        // ─── bırakma: iki sonuç ───────────────────────────────────────

        /// <summary>
        /// İKİ SONUÇ KARARININ birinci yarısı. Eşik hiç aşılmadan bırakılan
        /// jest <c>ClickReleased</c> üretir ve çağıran bunu "kipte kal,
        /// hayalet fareyi takip etmeye devam etsin" diye okur.
        /// </summary>
        [Test]
        public void Release_WhilePressed_IsClickReleased()
        {
            var gesture = NewGesture();
            gesture.Press(10f, 10f);
            gesture.MoveTo(11f, 11f);

            Assert.That(gesture.Release(11f, 11f), Is.EqualTo(PointerPhase.ClickReleased));
            Assert.That(gesture.IsActive, Is.False);
        }

        /// <summary>
        /// İKİ SONUÇ KARARININ ikinci yarısı. Eşiği aşmış jest
        /// <c>DragReleased</c> üretir ve çağıran bunu "yerleştir ve kipten çık"
        /// diye okur.
        /// </summary>
        [Test]
        public void Release_WhileDragging_IsDragReleased()
        {
            var gesture = NewGesture();
            gesture.Press(0f, 0f);
            gesture.MoveTo(0f, 30f);

            Assert.That(gesture.Release(0f, 30f), Is.EqualTo(PointerPhase.DragReleased));
            Assert.That(gesture.IsActive, Is.False);
        }

        /// <summary>
        /// İKİ SONUÇ KARARINI doğrudan koruyan test — ötekiler değerleri tek
        /// tek sınıyor, bu onların FARKLI olduğunu sınıyor.
        ///
        /// Kırmızıya dönerse iki kip tek değere indirilmiş demektir ve
        /// çağıranın dallanması yarıya iner: <c>BoardAdapter</c> "yerleştir ve
        /// çık" ile "kipte kal" arasında seçim yapamadığı için birini seçmek
        /// zorunda kalır, hangisini seçerse seçsin öğrenenin istediği iki giriş
        /// şeklinden biri ölür.
        /// </summary>
        [Test]
        public void Release_ClickAndDrag_ProduceDifferentPhases()
        {
            var clicked = NewGesture();
            clicked.Press(0f, 0f);
            PointerPhase clickResult = clicked.Release(1f, 1f);

            var dragged = NewGesture();
            dragged.Press(0f, 0f);
            dragged.MoveTo(0f, 30f);
            PointerPhase dragResult = dragged.Release(0f, 30f);

            Assert.That(
                clickResult,
                Is.Not.EqualTo(dragResult),
                "a click and a drag must be distinguishable by their release phase");
        }

        /// <summary>
        /// BIRAKMA KONUMU KARARINI koruyan test. Bırakma karesinin konumu bu
        /// tipe yalnızca <c>Release</c> üzerinden ulaşır — Unity'de o karede
        /// <c>GetMouseButton</c> false, <c>GetMouseButtonUp</c> true döner.
        /// Bu yüzden son konum da aynı eşikten geçirilir.
        ///
        /// Kırmızıya dönerse <c>Release</c> koordinatlarını yok sayıyor
        /// demektir ve hızlı bir savurma, geniş bir yay çizilmiş olmasına
        /// rağmen tıklama sayılır. Daha kötüsü: cevap artık konumun HANGİ
        /// metottan geldiğine bağlıdır, yani motorun kare düzeni saf tipin
        /// içine sızmıştır.
        /// </summary>
        [Test]
        public void Release_CrossingThresholdOnTheReleaseFrame_IsDragReleased()
        {
            var gesture = NewGesture();
            gesture.Press(0f, 0f);

            // MoveTo hiç eşiği aşmadı; bütün mesafe bırakma karesinde katedildi.
            Assert.That(gesture.MoveTo(1f, 0f), Is.EqualTo(PointerPhase.Pressed));

            Assert.That(
                gesture.Release(40f, 0f),
                Is.EqualTo(PointerPhase.DragReleased),
                "the release position is measured by the same threshold rule");
        }

        // ─── sonuç kipleri kalıcı ve etkin değil ──────────────────────

        [Test]
        public void MoveTo_AfterRelease_DoesNotResumeTheGesture()
        {
            var gesture = NewGesture();
            gesture.Press(0f, 0f);
            gesture.Release(0f, 0f);

            Assert.That(
                gesture.MoveTo(500f, 500f),
                Is.EqualTo(PointerPhase.ClickReleased),
                "a finished gesture is not revived by pointer movement");
        }

        [Test]
        public void Release_WhenNotActive_LeavesPhaseUnchanged()
        {
            var gesture = NewGesture();

            Assert.That(gesture.Release(3f, 3f), Is.EqualTo(PointerPhase.Idle));
        }

        [Test]
        public void MoveTo_WithoutPress_IsIgnored()
        {
            var gesture = NewGesture();

            Assert.That(gesture.MoveTo(999f, 999f), Is.EqualTo(PointerPhase.Idle));
            Assert.That(gesture.IsActive, Is.False);
        }

        [TestCase(PointerPhase.Idle, false)]
        [TestCase(PointerPhase.Pressed, true)]
        [TestCase(PointerPhase.Dragging, true)]
        [TestCase(PointerPhase.ClickReleased, false)]
        [TestCase(PointerPhase.DragReleased, false)]
        public void IsActive_IsTrueOnlyWhilePressedOrDragging(PointerPhase phase, bool expected)
        {
            var gesture = NewGesture();
            DriveTo(gesture, phase);

            Assert.That(gesture.Phase, Is.EqualTo(phase));
            Assert.That(gesture.IsActive, Is.EqualTo(expected));
        }

        // ─── sıfırlama ────────────────────────────────────────────────

        [Test]
        public void Reset_ReturnsToIdle()
        {
            var gesture = NewGesture();
            gesture.Press(0f, 0f);
            gesture.MoveTo(0f, 30f);

            gesture.Reset();

            Assert.That(gesture.Phase, Is.EqualTo(PointerPhase.Idle));
            Assert.That(gesture.IsActive, Is.False);
        }

        /// <summary>
        /// SIFIRLAMANIN KAPSAMINI koruyan test: <c>Reset</c> yalnız kipi değil
        /// BASMA NOKTASINI da unutur.
        ///
        /// Kırmızıya dönerse iptal edilmiş bir jestin koordinatları nesnede
        /// kalıyor demektir ve bir sonraki jest, oyuncunun hiç geçmediği bir
        /// mesafeyi ölçerek ilk kıpırdamada sürüklemeye atlar.
        /// </summary>
        [Test]
        public void Reset_ForgetsThePressOrigin()
        {
            var gesture = NewGesture();
            gesture.Press(100f, 100f);
            gesture.MoveTo(100f, 130f);

            gesture.Reset();
            gesture.Press(0f, 0f);

            Assert.That(
                gesture.MoveTo(3f, 4f),
                Is.EqualTo(PointerPhase.Pressed),
                "a reset gesture measures from its own new origin");
        }

        [Test]
        public void Reset_OnAFreshGesture_ChangesNothing()
        {
            var gesture = NewGesture();

            gesture.Reset();

            Assert.That(gesture.Phase, Is.EqualTo(PointerPhase.Idle));
        }

        // ─── yardımcı ─────────────────────────────────────────────────

        // Testin kendisi bir kip kurmak zorunda; kipi kurmanın tek yolu tipin
        // kendi çağrılarıdır ve bu bilerek böyle — Phase dışarıdan yazılabilir
        // olsaydı bu yardımcı iki satıra inerdi, ama o gün tipin tek işi
        // (kipe ne zaman geçileceğine karar vermek) elinden alınmış olurdu.
        private static void DriveTo(PointerGesture gesture, PointerPhase phase)
        {
            switch (phase)
            {
                case PointerPhase.Idle:
                    break;
                case PointerPhase.Pressed:
                    gesture.Press(0f, 0f);
                    break;
                case PointerPhase.Dragging:
                    gesture.Press(0f, 0f);
                    gesture.MoveTo(0f, 30f);
                    break;
                case PointerPhase.ClickReleased:
                    gesture.Press(0f, 0f);
                    gesture.Release(0f, 0f);
                    break;
                case PointerPhase.DragReleased:
                    gesture.Press(0f, 0f);
                    gesture.MoveTo(0f, 30f);
                    gesture.Release(0f, 30f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(phase), phase, "Unknown pointer phase.");
            }
        }
    }
}
