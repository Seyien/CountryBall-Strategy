using NUnit.Framework;
using GridStrategy.Combat;
using GridStrategy.Unity;
using UnityEngine;

namespace GridStrategy.Tests.EditMode.Unity
{
    /// <summary>
    /// Unity katmanının Unity'siz sınanamayan kısmı BoardAdapter'dır, bu tip
    /// değil. Ayrımın ölçüsü basit: <see cref="UnitView"/> yalnızca bir
    /// GameObject ister; sahne, kamera, tıklama ve Play mode istemez.
    /// BoardAdapter ise Input, Camera.main, prefab ve Instantiate ister — bu
    /// yüzden onun testi burada YOK ve bu bir kusur değil, o dosyadaki KOKU
    /// etiketinin ölçülmüş hâlidir.
    ///
    /// BoardAdapter'a giren yerleştirme kipi ve olay aboneliği o sınırı
    /// GENİŞLETMEDİ, yalnızca bir kez daha ölçtü: kipin kararını veren
    /// <c>PointerGesture</c> Core'a çıktığı için SINANABİLİR olan parça artık
    /// kendi test dosyasında, sınanamayan parça ise yalnız motorun üç girdi
    /// sorgusunu çevirmekten ibaret kaldı.
    ///
    /// ÖDENEN ASSEMBLY BEDELİ — ve önceden adı konmuştu: bu dosya artık
    /// <see cref="UnitState"/> yazıyor, yani <c>GridStrategy.Unity.EditModeTests</c>
    /// assembly'si <c>GridStrategy.Combat</c>'a referans vermek ZORUNDA.
    /// UnitView.cs'te REDDEDILEN olarak duran blok bu bedeli birebir tahmin
    /// etmişti; `Dead` kendi görselini kazanınca karar çevrildi ve fatura
    /// ödendi. Referans eklenmezse hata bir test kırmızısı değil, bir DERLEME
    /// hatası olur: "The type or namespace name 'UnitState' could not be found".
    ///
    /// AWAKE BU TESTLERDE HİÇ ÇALIŞMAZ. EditMode'da bir MonoBehaviour'ın Awake'i
    /// tetiklenmez (script <c>[ExecuteAlways]</c> değil). Yani bu dosya aynı
    /// zamanda bir tasarım kararını sabitliyor: gövde çizicisi Awake'te
    /// önbelleğe alınsaydı burada sonsuza dek null kalır, SetState sessizce
    /// hiçbir şey yapar ve aşağıdaki testlerin tamamı kırmızıya dönerdi — hem de
    /// sebebi görünmeden. Aynı şey YAZILI RENGİN yakalanması için de geçerli:
    /// yakalama Awake'te olsaydı burada hiç çalışmaz ve renk iddiaları
    /// sınanamazdı.
    /// </summary>
    public sealed class UnitViewTests
    {
        // GÖVDENİN "PREFAB'DA YAZILI" RENGİ. Beyaz DEĞİL ve hiçbir kanalı 0
        // değil; ikisi de bilinçli. Beyaz olsaydı "yazılı renk korunuyor mu"
        // sorusu, çarpanın nötr olup olmamasından bağımsız olarak yeşil
        // dönerdi; sıfır kanal olsaydı o kanal her çarpanda 0 kalır ve renk
        // ayrımı yapay biçimde zayıflardı.
        private static readonly Color AuthoredColor = new Color(0.8f, 0.6f, 0.4f, 1f);

        private GameObject probe;
        private SpriteRenderer body;
        private UnitView view;

        [SetUp]
        public void SetUp()
        {
            probe = new GameObject("UnitProbe");

            // SpriteRenderer AÇIKÇA ekleniyor, [RequireComponent]'in otomatik
            // eklemesine güvenilmiyor: test, UnitView'ın davranışını sınamalı,
            // Unity'nin AddComponent kolaylığını değil. Ayrıca elde bir referans
            // kalıyor ve sonuç doğrudan onun üstünden okunuyor.
            body = probe.AddComponent<SpriteRenderer>();

            // RENK, UnitView EKLENMEDEN ÖNCE yazılıyor ve sıra bir karardır:
            // yazılı renk, çizicinin ilk çözüldüğü anda yakalanıyor. Sonra
            // yazsaydık testin kendisi "yazılı renk" kavramını bozardı.
            body.color = AuthoredColor;

            view = probe.AddComponent<UnitView>();
        }

        [TearDown]
        public void TearDown()
        {
            // DestroyImmediate, Destroy değil: Destroy karenin sonunu bekler ve
            // EditMode'da o kare hiç gelmez, sahnede sızıntı kalırdı.
            Object.DestroyImmediate(probe);
        }

        [Test]
        public void SetState_Alive_KeepsTheAuthoredOrientationAndColor()
        {
            Assert.That(body.flipY, Is.False, "setup: the authored orientation");

            view.SetState(UnitState.Alive);

            Assert.That(body.flipY, Is.False);

            // Alive'ın çarpanı Color.white, yani çarpmanın NÖTR elemanı.
            // Bu satır o nötrlüğü sabitliyor: bir gün Alive'a da bir tint
            // verilirse burası kırmızıya döner ve karar sessizce değişemez.
            Assert.That(body.color, Is.EqualTo(AuthoredColor));
        }

        [Test]
        public void SetState_Downed_FlipsTheBodyVertically()
        {
            view.SetState(UnitState.Downed);

            Assert.That(body.flipY, Is.True);
        }

        [Test]
        public void SetState_Dead_AlsoFlipsTheBodyVertically()
        {
            // ÖLÜ BİRİM DE YATIKTIR ve bu bir karardır, bir kopyala-yapıştır
            // değil: ayakta olmayan her durum aynı yönelimi paylaşır. Downed ile
            // Dead'i ayıran eksen yönelim değil RENKTİR — hangi eksenin neyi
            // ayırdığı aşağıdaki üçlü testte sabitleniyor.
            view.SetState(UnitState.Dead);

            Assert.That(body.flipY, Is.True);
        }

        [Test]
        public void SetState_ThreeStates_ProduceThreeDistinctVisuals()
        {
            // BU TEST "ÜÇ DURUM, İKİ GÖRSEL" HATASINI KORUYOR: hatanın geri
            // gelmesi tam olarak burada kırmızıya döner. Bir görsel =
            // (yönelim, renk) İKİLİSİ; üç ikili birbirinden farklı olmak
            // zorunda ve hangi eksende ayrıştıkları önemli değil.
            //
            // ÖLÇÜ, üç ikili bugün şöyle: Alive (flipY false, yazılı renk),
            // Downed (flipY true, bir tint), Dead (flipY true, BAŞKA bir tint).
            // Alive her iki eksende ayrışıyor, Downed ile Dead yalnız renkte —
            // yani üç iddiadan yalnız sonuncusu tek eksene dayanıyor.
            //
            // (Bu satır eskiden "MADDE #9" diye numaralı bir maddeye atıf
            // yapıyordu; o numaranın karşılığı bu depoda YOK — ne kodda ne
            // Docs altında. Numara yerine hatanın kendi adı yazıldı.)
            view.SetState(UnitState.Alive);
            bool aliveFlip = body.flipY;
            Color aliveColor = body.color;

            view.SetState(UnitState.Downed);
            bool downedFlip = body.flipY;
            Color downedColor = body.color;

            view.SetState(UnitState.Dead);
            bool deadFlip = body.flipY;
            Color deadColor = body.color;

            Assert.That(
                aliveFlip != downedFlip || aliveColor != downedColor,
                Is.True,
                "Alive and Downed must not look the same.");

            Assert.That(
                aliveFlip != deadFlip || aliveColor != deadColor,
                Is.True,
                "Alive and Dead must not look the same.");

            // ÜÇLÜNÜN EN KIRILGAN KENARI: Alive her iki eksende de ayrışıyor,
            // Downed ile Dead ise YALNIZ renkte. Yani "üç durum iki görsel"
            // hatasına geri düşmenin tek yolu bu çift; mutasyon da tam buraya
            // yapılıyor.
            Assert.That(
                downedFlip != deadFlip || downedColor != deadColor,
                Is.True,
                "Downed and Dead must not look the same - that is exactly the bug this test guards.");
        }

        [Test]
        public void SetState_DownedThenDead_ChangesTheColorWithoutChangingTheOrientation()
        {
            // Ekrana ULAŞMAYAN geçişin sınandığı yer burası: Downed → Dead
            // Tick'in içinde olur. Adaptör tarafını EditMode sınayamaz, ama
            // görünüm tarafını sınayabilir — geçiş uygulandığında ekranda
            // gerçekten bir şeyin DEĞİŞTİĞİ burada sabitleniyor.
            view.SetState(UnitState.Downed);
            Color downedColor = body.color;

            view.SetState(UnitState.Dead);

            Assert.That(body.flipY, Is.True, "a dead unit stays down");
            Assert.That(body.color, Is.Not.EqualTo(downedColor));
        }

        [Test]
        public void SetState_Alive_AfterDead_RestoresTheAuthoredColorExactly()
        {
            // Diriltme geldiği gün bu satır tek koruma olacak: görsel geri
            // dönebilmeli, yoksa ayağa kalkan birim ekranda ters ve gri kalır.
            // "Yaklaşık" değil TAM eşitlik isteniyor ve isteyebiliyoruz, çünkü
            // Alive'ın çarpanı 1'dir ve kayan noktada 1 ile çarpmak kayıpsızdır.
            view.SetState(UnitState.Dead);

            view.SetState(UnitState.Alive);

            Assert.That(body.flipY, Is.False);
            Assert.That(body.color, Is.EqualTo(AuthoredColor));
        }

        [Test]
        public void SetState_NeverLosesTheAuthoredColor_AcrossRepeatedTransitions()
        {
            // YAZILI RENGİN ÖNBELLEĞİ BİR KEZ alınır; her SetState çağrısında
            // yeniden okunsaydı ikinci çağrı, birincinin yazdığı SOLUK rengi
            // "yazılı renk" sanır ve gövde her geçişte biraz daha kararırdı —
            // kod derlenir, tek bir geçişi sınayan test yeşil kalır, hata ancak
            // uzun bir oyunda görünürdü. Bu test o birikmeyi yakalar.
            for (int i = 0; i < 5; i++)
            {
                view.SetState(UnitState.Downed);
                view.SetState(UnitState.Dead);
                view.SetState(UnitState.Alive);
            }

            Assert.That(body.color, Is.EqualTo(AuthoredColor));
        }

        [Test]
        public void SetSelected_WithoutAnOverlay_IsSilentAndDoesNotThrow()
        {
            // selectionOverlay burada atanmamış - prefab'da boş bırakılmış
            // olmasıyla aynı durum. Awake bir kez bağırır (ve EditMode'da o bile
            // çalışmaz); tıklama başına LogError yazmak Console'u doldurur ve
            // asıl mesajı gömerdi. Bu yüzden sessiz kalmak bir davranıştır,
            // ihmal değil.
            Assert.DoesNotThrow(() => view.SetSelected(true));
            Assert.DoesNotThrow(() => view.SetSelected(false));
        }

        [Test]
        public void SetSelected_OnADeadUnit_LeavesTheDeadVisualIntact()
        {
            // SEÇİM İLE DURUM İKİ BAĞIMSIZ EKSEN — ve bu kararın koruması bu
            // test. SetSelected duruma BAKMAZ; baksaydı UnitView durumu
            // saklamak zorunda kalırdı ve "hafıza: yok" satırı düşerdi.
            // "Ölü birim seçilemez" kuralının sahibi eylem katmanıdır
            // (RejectedActorCannotAct), görünüm değil.
            //
            // SINIR, dürüstçe: çerçevenin gerçekten ÇİZİLDİĞİ burada
            // sınanamıyor, çünkü selectionOverlay bir çocuk nesnede yaşıyor ve
            // yalnız Inspector'dan atanabiliyor. Sınanan şey daha dar ve yine de
            // kararın taşıyıcısı: seçim çağrısı gövde görselini EZMİYOR.
            view.SetState(UnitState.Dead);
            Color deadColor = body.color;

            view.SetSelected(true);

            Assert.That(body.color, Is.EqualTo(deadColor), "selection must not repaint the body");
            Assert.That(body.flipY, Is.True, "selection must not stand a dead unit back up");
        }
    }
}
