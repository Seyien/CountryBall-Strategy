using NUnit.Framework;
using GridStrategy.Combat;
using GridStrategy.Unity;
using UnityEngine;

namespace GridStrategy.Tests.EditMode.Unity
{
    /// <summary>
    /// <see cref="StructureView"/> ikizi <see cref="UnitView"/> ile aynı sınırın
    /// içinde: yalnızca bir GameObject ister, sahne ve Play mode istemez. Bu
    /// dosya o sınırı bir kez daha ölçüyor.
    ///
    /// AWAKE BU TESTLERDE HİÇ ÇALIŞMAZ ve gerekçe UnitViewTests'te bir kez
    /// yazılı: EditMode'da bir MonoBehaviour'ın Awake'i tetiklenmez. Gövde
    /// çizicisinin ve YAZILI RENGİN tembel çözülmesi tam bu yüzden bir tasarım
    /// kararı; Awake'te yapılsaydı aşağıdaki iddiaların hepsi sınanamazdı.
    ///
    /// BU DOSYA SAHNE İDDİASI TAŞIMAZ. Yeşil olması "yıkılan bina oyunda
    /// görünüyor" demek DEĞİL; söylediği tek şey, kendisine yıkım söylenen bir
    /// görünümün gövde rengini doğru yazdığı. Söylemenin gerçekten olduğu yer
    /// BoardAdapter.RefreshStructureVisuals ve onun kanıtı bu kovada değil.
    /// </summary>
    public sealed class StructureViewTests
    {
        // GÖVDENİN "YAZILI" RENGİ. Beyaz DEĞİL ve hiçbir kanalı 0 değil; gerekçe
        // UnitViewTests'teki ikizinde yazılı ve burada tekrar edilmiyor.
        private static readonly Color AuthoredColor = new Color(0.8f, 0.6f, 0.4f, 1f);

        private GameObject probe;
        private SpriteRenderer body;
        private StructureView view;

        [SetUp]
        public void SetUp()
        {
            probe = new GameObject("StructureProbe");

            // SpriteRenderer AÇIKÇA ekleniyor, [RequireComponent]'in otomatik
            // eklemesine güvenilmiyor — gerekçe UnitViewTests'te yazılı: test,
            // StructureView'ün davranışını sınamalı, Unity'nin AddComponent
            // kolaylığını değil.
            body = probe.AddComponent<SpriteRenderer>();

            // RENK, StructureView EKLENMEDEN ÖNCE yazılıyor ve sıra bir karardır:
            // yazılı renk, çizicinin ilk çözüldüğü anda yakalanıyor.
            body.color = AuthoredColor;

            view = probe.AddComponent<StructureView>();
        }

        [TearDown]
        public void TearDown()
        {
            // DestroyImmediate, Destroy değil: Destroy karenin sonunu bekler ve
            // EditMode'da o kare hiç gelmez.
            Object.DestroyImmediate(probe);
        }

        /// <summary>
        /// SIFIRINCI DEĞER TUZAĞINI koruyan test, ve bu dosyadaki en pahalı
        /// satır burasıdır. <c>default(StructureState)</c> <c>Destroyed</c>'dur;
        /// görünümün "son uygulanan durum" alanı varsayılana bırakılsaydı ilk
        /// yıkım çağrısı "zaten yıkıktı" diye kısa devreye takılır ve gerçekten
        /// yıkılan bina HİÇ kararmazdı — hiçbir derleyici uyarısı olmadan.
        /// </summary>
        [Test]
        public void SetState_Destroyed_AsTheVeryFirstCall_StillDarkensTheBody()
        {
            view.SetState(StructureState.Destroyed);

            Assert.That(body.color, Is.Not.EqualTo(AuthoredColor),
                "the first Destroyed call must not be swallowed by the change guard");
        }

        [Test]
        public void SetState_Standing_KeepsTheAuthoredColor()
        {
            view.SetState(StructureState.Standing);

            // Standing'in çarpanı Color.white, yani çarpmanın NÖTR elemanı. Bu
            // satır o nötrlüğü sabitliyor: ayakta duran binaya bir gün çarpan
            // verilirse burası kırmızıya döner ve karar sessizce değişemez.
            Assert.That(body.color, Is.EqualTo(AuthoredColor));
        }

        /// <summary>
        /// YIKIK HÂLİN İKİ KANALI: karartma ve soldurma. Test tam renk DEĞİL,
        /// YÖN sınıyor — çarpanın sayısı bir denge düğmesidir ve teste
        /// kopyalansaydı sayı iki yerde yaşardı. Yön ters çevrilirse (yıkık bina
        /// AÇILIRSA) bu iddia kırmızıya döner.
        /// </summary>
        [Test]
        public void SetState_Destroyed_DarkensAndFadesTheBody()
        {
            view.SetState(StructureState.Destroyed);

            Assert.That(body.color.r, Is.LessThan(AuthoredColor.r), "red channel");
            Assert.That(body.color.g, Is.LessThan(AuthoredColor.g), "green channel");
            Assert.That(body.color.b, Is.LessThan(AuthoredColor.b), "blue channel");
            Assert.That(body.color.a, Is.LessThan(AuthoredColor.a), "alpha channel");
        }

        /// <summary>
        /// ÇARPAN, MUTLAK RENK DEĞİL. Yıkım yazılı rengi SİLMİYOR, üstüne
        /// biniyor; ayağa dönen bina rengini birebir geri alıyor. Mutlak renk
        /// yazılsaydı bu test yeşil kalır ama binanın takım rengi kaybolurdu —
        /// o yüzden iddia "geri döndü" değil, "BİREBİR geri döndü".
        /// </summary>
        [Test]
        public void SetState_BackToStanding_RestoresTheAuthoredColorExactly()
        {
            view.SetState(StructureState.Destroyed);
            view.SetState(StructureState.Standing);

            Assert.That(body.color, Is.EqualTo(AuthoredColor));
        }

        /// <summary>
        /// İKİ EKSEN TEK ALANDA YAŞIYOR VE BİRBİRİNİ SİLMİYOR. Bu testin
        /// koruduğu şey bir renk değil bir SAHİPLİK: seçim çarpanı bir zamanlar
        /// BoardAdapter'da, doğrudan SpriteRenderer.color'a yazılıyordu. Yıkım
        /// da aynı alana yazmaya başladığında son yazan ötekini silerdi —
        /// oyuncu ya seçtiği enkazı ayakta görürdü ya da seçimini kaybederdi.
        /// </summary>
        [Test]
        public void SetSelected_False_LeavesTheDestroyedTintInPlace()
        {
            view.SetState(StructureState.Destroyed);
            Color destroyedOnly = body.color;

            view.SetSelected(true);
            Assert.That(body.color, Is.Not.EqualTo(destroyedOnly),
                "setup: selection must change something while the structure is rubble");

            view.SetSelected(false);

            Assert.That(body.color, Is.EqualTo(destroyedOnly),
                "clearing the selection must not resurrect the standing look");
        }

        /// <summary>
        /// Aynı değişmezin ÖTEKİ YÖNÜ: seçili bir bina yıkıldığında seçim
        /// görünürlüğü kaybolmaz. İki yön ayrı ayrı sınanıyor, çünkü tek yönlü
        /// bir kelepçe (yalnız SetState'in seçimi okuması) bu iddiayı geçemez.
        /// </summary>
        [Test]
        public void SetState_Destroyed_WhileSelected_KeepsTheSelectionTintApplied()
        {
            view.SetSelected(true);
            Color selectedStanding = body.color;

            view.SetState(StructureState.Destroyed);
            Color selectedRubble = body.color;

            view.SetSelected(false);
            Color plainRubble = body.color;

            Assert.That(selectedRubble, Is.Not.EqualTo(selectedStanding), "rubble must read differently");
            Assert.That(selectedRubble, Is.Not.EqualTo(plainRubble), "selection must still read differently");
        }
    }
}
