using System.Collections.Generic;
using System.Reflection;
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
    /// BoardAdapter ise Input, Camera.main, prefab ve Instantiate ister — ESKİ
    /// ÖLÇÜ ("onun testi burada YOK") ARTIK YANLIŞ: <see cref="BoardAdapterTests"/>
    /// VAR ve o sınırı yansımayla aşıyor, ortadan kaldırarak değil.
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

        private const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;

        private GameObject probe;
        private SpriteRenderer body;
        private UnitView view;

        // Testin ürettiği geçici görseller. Sprite ve Texture2D birer Unity
        // nesnesidir ve sahne yıkılınca kendiliğinden gitmezler; toplanmasalardı
        // her koşuda sızarlardı.
        private readonly List<Object> disposables = new List<Object>();

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

            for (int i = 0; i < disposables.Count; i++)
            {
                Object.DestroyImmediate(disposables[i]);
            }

            disposables.Clear();
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

        // ══ ÜRETİLEN BİRİMİN KENDİ GÖVDESİ ═══════════════════════════════
        // Ölçülen kusur: tahtadaki her savaşçı prefab'ın dört karesinden birini
        // alıyordu, yani menzili 1 olan bir savaşçı ile bambaşka bir birim
        // ekranda ayırt edilemiyordu.

        /// <summary>
        /// Kendi gövdesi verilen bir birim onu SALDIRI pozunda da korur, ve
        /// <c>null</c> onu takım karesine geri verir.
        /// </summary>
        [Test]
        public void SetBodySprite_BeatsTheTeamFrameInBothPoses_AndNullHandsItBack()
        {
            Sprite idle = NewSprite();
            Sprite attacking = NewSprite();
            Sprite own = NewSprite();

            SetHiddenField("friendlyIdle", idle);
            SetHiddenField("friendlyAttacking", attacking);

            view.SetTeam(Team.Player);
            Assert.That(body.sprite, Is.SameAs(idle), "setup: the team frame is the default body");

            view.SetBodySprite(own);
            Assert.That(body.sprite, Is.SameAs(own));

            // KARARIN KORUMASI TAM BU SATIR: birim başına ikinci bir saldırı
            // karesi olmadığı için geçersiz kılma saldırı pozunu da yutuyor.
            // Takım karesine düşseydi okçu vuruş anında bir an piyadeye
            // dönüşürdü ve bu test kırmızıya dönerdi.
            view.SetAttacking(true);
            Assert.That(body.sprite, Is.SameAs(own), "the strike pose must not swap the silhouette");

            view.SetAttacking(false);
            Assert.That(body.sprite, Is.SameAs(own));

            view.SetBodySprite(null);
            Assert.That(body.sprite, Is.SameAs(idle), "clearing must hand the body back to the team frame");
        }

        /// <summary>
        /// Takım kareleri geçersiz kılmadan SONRA seçilse bile gövdeyi ezmez.
        /// </summary>
        // SIRA BİR KELEPÇE OLMASIN DİYE: tahtadaki yol takımı önce, gövdeyi
        // sonra veriyor ama o sıra bir gün ters dönerse hata SESSİZ kalırdı —
        // ekranda yalnız "yine aynı piyade" görünürdü.
        [Test]
        public void SetTeam_CalledAfterAnOverride_DoesNotEraseIt()
        {
            Sprite idle = NewSprite();
            Sprite own = NewSprite();

            SetHiddenField("friendlyIdle", idle);

            view.SetBodySprite(own);
            view.SetTeam(Team.Player);

            Assert.That(body.sprite, Is.SameAs(own));
        }

        /// <summary>
        /// Gövdeyi değiştirmek yaşam durumunun rengine DOKUNMAZ.
        /// </summary>
        // İKİ EKSEN BAĞIMSIZ KALMALI: sprite kimliği, renk çarpanı ise yaşam
        // durumunu anlatıyor. Geçersiz kılma rengi de yazsaydı düşmüş bir birim
        // yeni gövdesiyle birlikte ayağa kalkmış gibi görünürdü.
        [Test]
        public void SetBodySprite_LeavesTheStateTintUntouched()
        {
            view.SetState(UnitState.Dead);
            Color deadColor = body.color;
            bool deadFlip = body.flipY;

            view.SetBodySprite(NewSprite());

            Assert.That(body.color, Is.EqualTo(deadColor));
            Assert.That(body.flipY, Is.EqualTo(deadFlip));
        }

        // ══ HAVUZUN SESSİZ HATASI ════════════════════════════════════════

        /// <summary>
        /// Havuza geri verilen görsel, gövde geçersiz kılmasını BIRAKIR.
        /// </summary>
        // BU TESTİN KORUDUĞU HATA SESSİZDİR: temizlenmeseydi havuzdan çıkan bir
        // sonraki birim önceki sahibinin resmiyle doğardı — hiçbir istisna,
        // hiçbir konsol satırı, yalnız ekranda yanlış birim. Havuz kullanan
        // kodların en sık hatası budur ve bu dosyada adı konmuş durumda.
        [Test]
        public void UnitViewPool_Return_ClearsTheBodyOverride()
        {
            Sprite idle = NewSprite();
            Sprite own = NewSprite();

            SetHiddenField("friendlyIdle", idle);
            view.SetTeam(Team.Player);
            view.SetBodySprite(own);
            Assert.That(body.sprite, Is.SameAs(own), "setup: the view carries its own body");

            // PREFAB OLARAK KENDİSİ VERİLİYOR: havuz yalnız boşken Instantiate
            // eder ve bu test hiç boşalmıyor, yani prefab argümanı burada bir
            // kullanılmayan alandan ibaret.
            var pool = new UnitViewPool(view, null);

            pool.Return(view);

            Assert.That(body.sprite, Is.SameAs(idle),
                "a pooled view must go back to the team frame, not keep the previous unit's body");
            Assert.That(pool.IdleCount, Is.EqualTo(1));
        }

        /// <summary>
        /// Havuzdan ÖDÜNÇ ALINAN görsel de temiz doğar.
        /// </summary>
        // İKİ YOL AYNI GARANTİYİ VERMELİ: havuz sıfırlamayı hem geri alırken hem
        // ödünç verirken yapıyor ve ikisinden birinin düşmesi ötekinin arkasında
        // saklanabilirdi.
        [Test]
        public void UnitViewPool_Rent_HandsOutAViewWithoutThePreviousBody()
        {
            Sprite idle = NewSprite();
            Sprite own = NewSprite();

            SetHiddenField("friendlyIdle", idle);
            view.SetTeam(Team.Player);

            var pool = new UnitViewPool(view, null);
            pool.Return(view);

            // Geri verildikten SONRA elle kirletiliyor: sıfırlamanın ödünç verme
            // tarafını sınamanın tek yolu, geri verme tarafının temizlediği şeyi
            // yeniden yazmak.
            view.SetBodySprite(own);

            UnitView rented = pool.Rent(Vector3.zero, "Rented");

            Assert.That(rented, Is.SameAs(view), "setup: the pool must hand back the same view");
            Assert.That(body.sprite, Is.SameAs(idle));
        }

        // ══ HAVUZUN İKİNCİ SESSİZ HATASI: SOLGUN RENK DEVRALINIYOR ═══════
        // OPERATÖRÜN BİLDİRDİĞİ BELİRTİ: bir savaşçı öldükten sonra fabrikadan
        // aynı tipte bir birim üretildiğinde, yeni birim ölünün solgun rengiyle
        // doğuyordu. Havuzun sıfırlama listesi dört üye sayıyordu ve beşincisi —
        // yaşam durumu — unutulmuştu; ne derleyici ne de o gün var olan testler
        // bunu söyleyebilirdi, çünkü ikisi de "liste eksiksiz mi" diye sormaz.
        //
        // İKİNCİ BELİRTİ, OPERATÖRÜN GÖRMEDİĞİ: aynı görsel BAŞ AŞAĞI da
        // doğuyordu. Gövde neredeyse simetrik olduğu için ekranda ayırt
        // edilememişti ve tam bu yüzden ayrı bir test satırı hak ediyor.

        /// <summary>
        /// Düşmüş hâlden geçen bir görsel, yeniden kiralandığında yazılı rengine
        /// döner.
        /// </summary>
        [Test]
        public void UnitViewPool_Rent_AfterADownedViewIsReturned_RestoresTheAuthoredColor()
        {
            view.SetState(UnitState.Downed);
            Assert.That(body.color, Is.Not.EqualTo(AuthoredColor), "setup: the view really is faded");

            var pool = new UnitViewPool(view, null);
            pool.Return(view);
            UnitView rented = pool.Rent(Vector3.zero, "Rented");

            Assert.That(rented, Is.SameAs(view), "setup: the pool must hand back the same view");
            Assert.That(body.color, Is.EqualTo(AuthoredColor),
                "a recycled view must not carry the previous unit's downed tint");
        }

        /// <summary>
        /// Aynı görsel yeniden kiralandığında ayağa da kalkar.
        /// </summary>
        // RENKTEN AYRI BİR TEST ve bu bir tekrar değil: iki eksenin tek sahibi
        // SetState olduğu için bir gün biri yazılıp öteki unutulabilir. Renk
        // iddiası tek başına o günü yakalayamazdı.
        [Test]
        public void UnitViewPool_Rent_AfterADownedViewIsReturned_StandsTheBodyBackUp()
        {
            view.SetState(UnitState.Downed);
            Assert.That(body.flipY, Is.True, "setup: the view really is lying down");

            var pool = new UnitViewPool(view, null);
            pool.Return(view);
            pool.Rent(Vector3.zero, "Rented");

            Assert.That(body.flipY, Is.False, "a recycled view must not be born upside down");
        }

        /// <summary>
        /// Ölü hâlden geçen görsel için de aynısı geçerlidir.
        /// </summary>
        // ÖLÜ AYRI SINANIYOR ÇÜNKÜ ÇARPANI AYRI: Downed ile Dead'i ayıran tek
        // eksen renktir ve yalnız birini sınamak, ötekinin çarpanının havuzdan
        // sızmasına açık kapı bırakırdı.
        [Test]
        public void UnitViewPool_Rent_AfterADeadViewIsReturned_RestoresTheAliveVisual()
        {
            view.SetState(UnitState.Dead);
            Assert.That(body.color, Is.Not.EqualTo(AuthoredColor), "setup: the view really is greyed out");

            var pool = new UnitViewPool(view, null);
            pool.Return(view);
            pool.Rent(Vector3.zero, "Rented");

            Assert.That(body.flipY, Is.False);
            Assert.That(body.color, Is.EqualTo(AuthoredColor));
        }

        /// <summary>
        /// Havuza hiç girmemiş, ilk kez doğan bir görsel de ayakta başlar.
        /// </summary>
        // İKİ YOL AYNI GARANTİYİ VERMELİ ve burada sınanan yol İADE DEĞİL:
        // Rent, havuz boşken gerçekten Instantiate eder ve o kopya geri verme
        // yolundan hiç geçmez. Çizici doğrudan çevriliyor, yani "prefab ters
        // kaydedilmiş" durumu taklit ediliyor; renge dokunulmuyor, çünkü kopyanın
        // yazılı rengi tam da çizicisinden okunacak.
        [Test]
        public void UnitViewPool_Rent_WithoutAReturn_StillHandsOutAnAliveView()
        {
            body.flipY = true;

            var pool = new UnitViewPool(view, null);
            UnitView rented = pool.Rent(Vector3.zero, "FirstRent");

            // Kopya da bir sahne nesnesidir ve toplanmazsa her koşuda sızardı.
            disposables.Add(rented.gameObject);

            Assert.That(pool.CreatedCount, Is.EqualTo(1), "setup: an empty pool must instantiate");
            Assert.That(rented, Is.Not.SameAs(view), "setup: this is a fresh copy, not the source");

            // Sonuç ÖNCE null'a karşı sınanıyor: RequireComponent bunu garanti
            // eder ama garanti bir gün düşerse hata bir NullReferenceException
            // değil, adı konmuş bir kırmızı olarak görünsün.
            SpriteRenderer rentedBody = rented.GetComponent<SpriteRenderer>();
            Assert.That(rentedBody, Is.Not.Null, "the clone must carry its own body renderer");

            Assert.That(rentedBody.flipY, Is.False, "a newborn view must stand up");
            Assert.That(rentedBody.color, Is.EqualTo(AuthoredColor));
        }

        /// <summary>
        /// Sıfırlama kapısı görünür eksenlerin HEPSİNİ tek çağrıda kapatır.
        /// </summary>
        // BU TEST LİSTENİN KENDİSİNİ KORUYOR, havuzu değil: üç eksen (gövde
        // görseli, duruş, renk) tek iddiada birleşiyor, çünkü hatanın tarifi
        // "biri unutuldu" idi. Kapı UnitView'e taşındı ve testi de listenin
        // yanında duruyor.
        [Test]
        public void ResetVisuals_ClearsEveryVisibleAxisAtOnce()
        {
            Sprite idle = NewSprite();
            Sprite own = NewSprite();

            SetHiddenField("friendlyIdle", idle);
            view.SetTeam(Team.Player);
            view.SetBodySprite(own);
            view.SetState(UnitState.Dead);
            view.SetSelected(true);

            view.ResetVisuals();

            Assert.That(body.sprite, Is.SameAs(idle), "the previous unit's body must be gone");
            Assert.That(body.flipY, Is.False, "the view must be standing again");
            Assert.That(body.color, Is.EqualTo(AuthoredColor), "the authored colour must be back");
        }

        /// <summary>
        /// Bir kez kullanılıp atılan geçici sprite üretir.
        /// </summary>
        // DOKU DA TOPLANIYOR: Sprite.Create dokuya bir ok tutuyor ve yalnız
        // sprite yok edilirse doku sahnede kalırdı.
        private Sprite NewSprite()
        {
            var texture = new Texture2D(4, 4);
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f));

            disposables.Add(sprite);
            disposables.Add(texture);
            return sprite;
        }

        // ADI BULAMAYINCA SESSİZ GEÇMEK YASAK: yansıma yanlış bir ada
        // sorulduğunda null döner ve SetValue bir NullReferenceException'a
        // dönerdi — testin neyi ölçtüğü kaybolurdu. Aynı disiplin
        // BoardAdapterTests'te de yazılı.
        private void SetHiddenField(string name, object value)
        {
            FieldInfo field = typeof(UnitView).GetField(name, Hidden);
            Assert.That(field, Is.Not.Null, $"UnitView has no private field named '{name}'");
            field.SetValue(view, value);
        }
    }
}
