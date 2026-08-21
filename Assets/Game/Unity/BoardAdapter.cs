using System.Collections.Generic;
using GridStrategy.Battle;
using GridStrategy.Combat;
using GridStrategy.Core;
using UnityEngine;

namespace GridStrategy.Unity
{
    // ═══ ÇIPLAK "Battle" YAZMAK BU DOSYADA DERLEME HATASIDIR (CS0118) ═══
    //
    // ── HARİTA: derleyicinin gördüğü ad ağacı ────────────────────────
    // Bu ağacı KLASÖRLER kurmaz; yalnızca `namespace` satırları kurar.
    // Kanıt aynı projede: Core/Combat/ klasörü diskte Core'un İÇİNDEdir
    // ama ad alanı GridStrategy.Combat'tır — yani Core'un KARDEŞİ.
    //
    //   global::
    //   └── GridStrategy
    //       ├── Battle           ◄── ① AD ALANI
    //       │   ├── Battle       ◄── ② SINIF  (bu dosyanın istediği)
    //       │   ├── BattleActions
    //       │   └── PlacementOutcome
    //       ├── Combat
    //       ├── Core
    //       └── Unity            ◄── BU DOSYA BURADA YAŞIYOR
    //           ├── BoardAdapter
    //           └── UnitView
    //
    // Tek "Battle" kelimesi iki ayrı şeyi adlandırıyor: ① bir ad alanı,
    // ② onun içindeki bir sınıf. Çıplak yazıldığında ① kazanır.
    //
    // ── ARAMA SIRASI: ① neden kazanıyor ──────────────────────────────
    // Derleyici en içteki ad alanından başlar, dışa doğru yürür ve HER
    // seviyede önce o ad alanının KENDİ ÜYELERİNE bakar; ancak orada
    // bulamazsa o seviyedeki using'lere döner.
    //
    //   SEVİYE 1   GridStrategy.Unity üyeleri: BoardAdapter, UnitView   ✗
    //   SEVİYE 1b  bu ad alanı gövdesindeki using/alias   ◄── ALIAS BURADA
    //   SEVİYE 2   GridStrategy üyeleri: Battle, Combat, Core, Unity    ✓
    //              ██ ARAMA BİTTİ ██ bulunan bir AD ALANI, tip değil
    //                                              ──────► CS0118
    //   SEVİYE 3   dosya başındaki using'ler   ── BURAYA HİÇ GELİNMEZ
    //
    // Dosyanın 2. satırındaki `using GridStrategy.Battle;` bu adı
    // KURTARAMAZ: arama SEVİYE 2'de bitiyor, o satır SEVİYE 3'te bekliyor.
    // Üst ad alanının bir ÜYESİ, dosya başındaki using'i HER ZAMAN yener.
    //
    // ── ALIAS'IN YERİ KURALIN KENDİSİDİR ─────────────────────────────
    // Alias namespace GÖVDESİNDE olduğu için SEVİYE 1b'de yakalanır ve
    // arama SEVİYE 2'ye hiç çıkmaz. Aynı satır dosyanın başına, öteki
    // using'lerin yanına taşınsaydı SEVİYE 3'e düşerdi ve CS0118 geri
    // gelirdi — metin harfi harfine aynı, sonuç zıt. Yeri tesadüf değil.
    //
    // ── KAPSAM: bu SADECE `Battle` adına özeldir ─────────────────────
    // Kural: bir tip adı yalnızca AYNI ZAMANDA kapsayan zincirde görünen
    // bir AD ALANININ adıysa tuzağa düşer. Kesişimi al:
    //
    //   GridStrategy'nin ad alanları : Battle   Combat  Core  Unity
    //   projedeki 34 tip adı         : Battle   BattleActions  Unit  ...
    //   kesişim                      : { Battle }        ← tek eleman
    //
    // KARŞI ÖRNEK aşağıda, satır ~656: `BattleActions` ve
    // `PlacementOutcome` tam olarak AYNI ad alanında, AYNI klasörde,
    // AYNI assembly'de yaşar — ve alias olmadan çalışırlar, çünkü o
    // adlarda bir ad alanı yok. Onlara alias yazmak gereksiz gürültü
    // olurdu. Yeni bir tip eklerken sorulacak tek soru: adı Battle,
    // Combat, Core veya Unity mi? Değilse `using` yeter.
    //
    // ── İŞ BÖLÜMÜ: using ile alias ÖRTÜŞMEZ, BÖLÜŞÜR ─────────────────
    // Bu dosya GridStrategy.Battle'dan üç tip kullanıyor ve ikisi
    // tamamen farklı yoldan geliyor:
    //
    //   Battle             çakışıyor   ► ALIAS halleder  (using etkisiz)
    //   BattleActions      çakışmıyor  ► using halleder  (alias gereksiz)
    //   PlacementOutcome   çakışmıyor  ► using halleder  (alias gereksiz)
    //
    // Bu yüzden ikisi de gerekli ve hibrit bir kaza değil: üstteki using
    // silinirse BattleActions ile PlacementOutcome kırılır, alias
    // silinirse Battle kırılır.
    //
    // ── `global::` HATAYI ÇÖZMEZ, GELECEĞİ KİLİTLER ──────────────────
    // Alias'ın SAĞ tarafındaki `GridStrategy` adı da çözülmek zorunda ve
    // o çözüm de yukarıdaki aynı sıraya tabi. `global::` aramayı SEVİYE
    // 1-2'yi atlayıp kökten başlatır: ileride buraya `GridStrategy` adlı
    // bir tip ya da ad alanı eklense bile alias'ın hedefi sessizce
    // kaymaz. Yani aynı tuzağın alias'ın KENDİ hedefinde kurulmasını
    // engelleyen bir sigortadır — bugünkü hatanın çözümü değil. Aynı
    // desen ve gerekçe BattleTests ile BattleActionsTests'teki kardeş
    // alias'ların üstünde de yazılı.
    //
    // ── ALTERNATİF VE ASIL KÖK ───────────────────────────────────────
    // Alias yok, her kullanım `GridStrategy.Battle.Battle` diye tam
    // nitelenir: derleme geçer ama tuzağı anlatan tek satır kaybolur
    // (tip bu dosyada BİR kez geçseydi tercih tersine dönerdi). Asıl kök
    // ise ADLANDIRMA: sınıf `BattleState` ya da ad alanı
    // `GridStrategy.Battles` olsaydı tuzak hiç doğmazdı. Ad korundu,
    // bedeli bu blok oldu.
    using Battle = global::GridStrategy.Battle.Battle;

    // ═══ GİRİŞ OKUMA NOTU — ÜÇLÜ GEREKİR, TEKİ YETMEZ ════════════════
    //
    // `Input.GetMouseButtonDown(0)` TEK BAŞINA YETMEZ. Bu dosyada üç ayrı
    // sorgu var ve üçü ÜÇ FARKLI SORUYA cevap veriyor:
    //
    //   GetMouseButtonDown(0)  yalnız BASILDIĞI karede true   -> gesture.Press
    //   GetMouseButton(0)      basılı olduğu HER karede true   -> gesture.MoveTo
    //   GetMouseButtonUp(0)    yalnız BIRAKILDIĞI karede true  -> gesture.Release
    //
    // Neden üçü birden: bir tıklama ile bir sürükleme, BAŞLANGIÇLARINDA
    // birbirinin AYNISIdır. İkisini ayıran tek şey "basılı tutulurken imleç
    // gerçekten hareket etti mi" sorusudur ve o soru ancak basılı geçen
    // karelerde (GetMouseButton) sorulabilir. Yalnız Down okunsaydı sürükleme
    // diye bir kavram bu dosyada YAZILAMAZDI: bırakma anı hiç görülmediği için
    // "nerede bıraktı" da bilinemezdi.
    //
    // Kararın kendisi burada DEĞİL: hangi karenin tıklama, hangisinin sürükleme
    // olduğuna GridStrategy.Core.PointerGesture karar verir. Bu dosyanın işi
    // motorun üç sorusunu o tipin üç metoduna ÇEVİRMEKten ibarettir — çevirmen
    // yarısının ders kitabı örneği.
    //
    // ÇERÇEVE SINIRI, ve bilerek yazıyorum: bir kare içinde Down ve Up AYNI ANDA
    // true olabilir (kare süresinden kısa bir tıklama). Bu yüzden aşağıdaki
    // FeedGesture'da Down/MoveTo bir if-else zinciri, Up ise AYRI bir if'tir;
    // hepsi tek zincire konsaydı o hızlı tıklamanın bırakılışı sessizce
    // yutulur ve hayalet fareye yapışıp kalırdı.
    //
    // ═══ ROL: KARMA — ÇEVİRMEN + VARLIK (Adapter + Entity) ═══════════
    // kimlik : var — sahnedeki tahta bileşeni; ayrıca battle, unitViews ve
    //          selectedUnit'in TEK sahibi, yani durum burada ikamet ediyor
    // hafıza : var — selectedUnit bir OYUN durumudur, çeviri durumu değil;
    //          saf bir çevirmenin taşımaması gereken şey tam olarak budur
    // Unity  : zorunlu — Input, Camera, Time, Instantiate, MonoBehaviour
    // karar  : ikisi birden — piksel→hücre çevirisi (çevirmen işi) ile
    //          "aynı anda tek birim seçili" ve "dolu hücreye tıklamak SALDIRI,
    //          boş hücreye tıklamak HAREKET demektir" kuralları (varlık işi)
    //          aynı tipte
    // KOKU   : evet ve BÜYÜDÜ. Önceki başlık "tek satırlık kural için bugün
    //          ayrı katman yalnızca dolaylılık olurdu" diyordu; o cümle artık
    //          doğru değil, çünkü üç yeni oyun kararı buraya girdi:
    //          (1) tıklamanın NİYETE çevrilmesi (boş→hareket, dolu→saldırı,
    //          kendine→seçimi bırak), (2) savaşın zamanını ilerletme ve ceset
    //          süpürme takvimi, (3) birim sayılarının (can, hasar, menzil,
    //          taraf) yazılı olduğu yer. Üçü de Unity'siz test EDİLEMEZ hâlde
    //          ve üçü de bu tipin "çevirmen" yarısına ait değil.
    //          EŞİK AŞILDI — ve notu SİLMİYORUM, çünkü bir eşiğin aşıldığını
    //          söyleyen satır, eşiği koyan satır kadar öğreticidir.
    //          Yazılı eşik şuydu: *"dördüncü kural geldiği gün Core tarafına
    //          bir 'komut' sahibi çıkmalı: tıklamayı niyete çeviren saf bir
    //          tip."* Madde #10 bütün bir GİRİŞ KİPİ ekledi (yerleştirme) ve
    //          eşiği aştı.
    //          NASIL KARŞILANDI — ve yarısıyla: karar dışarı çıktı, ama
    //          çıkan yarı "niyet" değil "JEST" oldu. GridStrategy.Core.
    //          PointerGesture "bu bir tıklama mıydı yoksa sürükleme mi"
    //          sorusunun tek sahibi; Unity'siz, Time'sız, Vector2'süz ve
    //          EditMode'da sınanabilir. Bu MonoBehaviour ona dört float
    //          veriyor ve dönen fazı uyguluyor — eşiğin istediği şeklin ta
    //          kendisi, yalnız dar bir soru için.
    //          KALAN YARI, ve dürüst adı: tıklamanın NİYETE çevrilmesi
    //          (boş→hareket, dolu→saldırı, kendine→seçimi bırak) hâlâ BURADA,
    //          HandleClick'in içinde ve Unity'siz sınanamaz durumda.
    //          SIRADAKİ EŞİK: bu üç dala DÖRDÜNCÜSÜ eklendiği gün — sıra
    //          kimde (TurnRules yazılı ve burada hâlâ SORULMUYOR), çoklu
    //          seçim, ya da hedef önizlemesi. O gün PointerGesture'ın ikizi
    //          doğmalı: (x, y) + tahtanın durumu alıp bir NİYET değeri
    //          döndüren saf bir tip, ve bu dosyada geriye yalnız o niyetin
    //          uygulanması kalmalı.
    /// <summary>
    /// Unity dünyası ile motordan bağımsız savaş kuralları arasındaki çevirmen.
    /// Kendi kuralı yoktur; her kararı <see cref="Battle"/> ve
    /// <see cref="BattleActions"/> nesnelerine sorar.
    ///
    /// Birim başına GÖRSEL durum artık burada değil, <see cref="UnitView"/>
    /// içinde yaşıyor - o baskı gerçekten doğdu ve bölündü. Buna karşılık input
    /// okuma ve zemin kurulumu hâlâ burada: ikisi de bağımsız değişme baskısı
    /// üretmedi, baskısız bir katman yalnızca dolaylılık ekler.
    ///
    /// TAHTA ARTIK BURADA DEĞİL. Bu tipte bir <see cref="UnitGrid"/> alanı
    /// vardı; o alan silindi çünkü <see cref="Battle"/> tahtayı kendisi
    /// sahipleniyor. İki sahibin bedeli <see cref="Battle"/>'ın kurucusundaki
    /// REDDEDILEN bloğunda yazılı ve o öngörü artık kapanmıştır.
    /// </summary>
    [RequireComponent(typeof(Grid))]
    public sealed class BoardAdapter : MonoBehaviour
    {
        [Header("Board size in CELLS, not world units")]
        [SerializeField, Min(1)] private int width = 3;
        [SerializeField, Min(1)] private int height = 5;

        [Header("Terrain sprites - at least one required")]
        [SerializeField] private Sprite[] terrainSprites;

        // Alan tipi GameObject değil UnitView: Inspector artık UnitView TAŞIMAYAN
        // bir prefab'ı kabul etmez. Yani "prefab'a bileşen eklemeyi unuttum"
        // hatası Play'e basmadan, sürükle-bırak anında yakalanır. GameObject
        // tutsaydık aynı hata ancak ilk tıklamada NullReference olarak çıkardı.
        [Header("Unit prefab")]
        [SerializeField] private UnitView unitPrefab;

        // BİRİM SAYILARI NEREDEN GELİYOR: buradan, düz [SerializeField] olarak.
        //
        // AttackProfile'ın asset bloğundaki KAZANIRDI satırı "hasar ve menzil
        // sayılarını programcı değil tasarımcı ayarlayacaksa" diyor. O gün
        // GELMEDİ — ama yarısı geldi: sayıların artık gerçek bir okuyucusu var
        // ve yeniden derlemeden denenebilmeleri gerekiyor. Inspector alanı bunu
        // verir; asset dosyası bundan fazlasını (paylaşım, birim listesi,
        // sürümleme) verir ve o fazlanın bugün alıcısı yok.
        //
        // REDDEDILEN - BoardAdapter.cs:236 yerine (sayılar bir varlık tanımı
        //              asset'ine taşınır ve bu alanlar tek bir referansa iner):
        //     [SerializeField] private UnitDefinition playerDefinition;
        //     [CreateAssetMenu(menuName = "GridStrategy/Unit Definition")]
        //     public sealed class UnitDefinition : ScriptableObject { ... }
        // KIRILAN  : SAHNE BOZULUR — .asset koddan doğmaz, Editor'de üretilir
        //            ve prefab/sahne dosyalarına kod tarafı dokunamaz.
        //            iki alan atanmamış kalır -> Awake'te NullReferenceException
        //            null kontrolü eklenirse  -> hiç birim doğmayan bir tahta
        //            derleyici: hiçbir şey der  .  test: adaptör EditMode'da
        //            sınanamaz, hiçbiri kırmızıya dönmez
        // KAZANIRDI: birim ÇEŞİDİ ikiden fazla olduğu gün — okçu, süvari, tank;
        //            ya da aynı tanımı yüzlerce birim paylaştığı gün. Bugün ikisi
        //            arasındaki TEK fark Team, yani paylaşacak bir şey yok.
        // KARSILASTIRMA:
        //     const              Inspector'da YOK  -> her denge denemesi bir
        //                                            derleme turu ister
        //     [SerializeField]   sahnede YAZILI    -> tek sahne, tek kopya;
        //                                            bugünkü tek okuyucu bu
        //     ScriptableObject   asset'te YAZILI   -> paylaşılır, sürümlenir;
        //                                            karşılığı bir Editor adımı
        // TEK CUMLE: Serileştirmenin bedeli bir Inspector alanı, asset'e
        //            taşımanın bedeli ise koddan doğmayan bir dosyadır.
        //
        // Alternatif: sayıları `const` yapmak. Seçilmedi: tablonun ilk satırı —
        // sayı bir DENGE değeri olduğu sürece her deneme bir derleme turu ister
        // (DEĞİŞMEZ olsaydı Combatant.ReviveHealthDivisor gibi doğru olurdu).
        [Header("Unit stats - applied to every spawned unit")]
        [Tooltip("Starting and maximum health of each spawned unit.")]
        [SerializeField, Min(1)] private int maxHealth = 30;

        [Tooltip("Raw damage of a single hit, before any resistance.")]
        [SerializeField, Min(0)] private int damage = 10;

        [Tooltip("How many cells away a unit can strike. Must be at least 1.")]
        [SerializeField, Min(1)] private int attackRange = 1;

        // HAREKET MENZİLİNİN SAHİBİ: bugün BU ALAN, yani Unity katmanı.
        //
        // MoveAction.Execute menzili parametre olarak istiyor ve o kararın
        // gerekçesi MoveAction'ın kendi REDDEDILEN bloğunda yazılı: menzili
        // Unit'e koymak, o tipin "ne yapabileceğini bilmez" sözünü deler ve
        // saldırı tarafının (AttackProfile) verdiği cevapla çelişir. O blok
        // doğru cevabı da söylüyor: AttackProfile'ın ikizi olan bir MoveProfile.
        // MoveProfile ARTIK VAR (GridStrategy.Core) ve BattleActions.Move onu
        // sayıdan kendisi kuruyor; SAYININ sahibi ise hâlâ burası. Bu bir karar
        // değil, adı konmuş bir BORÇ.
        //
        // REDDEDILEN - BoardAdapter.cs:271 yerine (menzil birime göre değişsin
        //              diye SpawnUnit parametre alır ve sayı burada saklanır):
        //     private readonly Dictionary<Unit, int> moveRanges =
        //         new Dictionary<Unit, int>();
        // KIRILAN  : Unit ile anahtarlanan ÜÇÜNCÜ sözlük doğar — unitViews
        //            burada, combatants Battle'da, bu da burada.
        //            temizlikte silmeyi unutan tek satır -> ölmüş birim sonsuza
        //            dek canlı kalır; ve bir SAVAŞ değeri MonoBehaviour'a taşınır
        //            derleyici: hiçbir şey der  .  test: değer artık EditMode'da
        //            sınanamaz, yani onu koruyacak test YAZILAMAZ
        // KAZANIRDI: menzil gerçekten birimden birime değiştiği gün — ama o gün
        //            bile cevap Combatant'ın yanına konulan bir MoveProfile'dır;
        //            bu sözlük onun test edilemeyen taklidi olurdu.
        // TEK CUMLE: Aynı nesneyle anahtarlanan üçüncü sözlük, senkronunu hiçbir
        //            tipin garanti etmediği üçüncü bir doğruluk kaynağıdır.
        [Tooltip("How many cells a unit can travel in one move. 0 means rooted.")]
        [SerializeField, Min(0)] private int moveRange = 1;

        // ═══ YERLEŞTİRME KİPİ (#10) ══════════════════════════════════
        //
        // HAYALET GERÇEK BİR Structure DEĞİLDİR. Tahtaya girmez, Battle onu
        // bilmez, hiçbir kural onu görmez; yalnızca bir SpriteRenderer'dır ve
        // tek işi "bırakırsan buraya konur" demektir.
        //
        // REDDEDILEN - BoardAdapter.cs:295 yerine (hayalet gerçek bir yapıdır;
        //              kipe girerken tahtaya eklenir, iptalde geri alınır):
        //     battle.AddStructure(selectedUnit, ghostStructure, x, y);
        //     // ... iptalde: battle.RemoveStructure(selectedUnit);
        // KIRILAN  : savaşın KAYDI imleç hareketiyle mutasyona uğrar; fare her
        //            kare hücre değiştirdiğinde tahtaya yazma/silme çifti gider.
        //            iptal yolu unutulur -> hücreyi kapatan, hedeflenen, görünmez
        //            bir HAYALET BİNA tahtada kalır
        //            derleyici: hiçbir şey der  .  test: yeşil, çünkü testler
        //            Battle'ı doğrudan kurar ve iptal yolundan hiç geçmez
        // KAZANIRDI: geçerlilik önizlemesi tahtanın GERÇEK cevabını göstermek
        //            zorunda kalsaydı ve Battle bir "deneme/geri al" yeteneği
        //            kazansaydı — o gün hayaleti gerçek yapmak tek yol olurdu.
        // TEK CUMLE: Bir önizleme gösterdiği şeyi ÜRETEREK gösteriyorsa artık
        //            önizleme değil, geri alınması unutulabilen bir yazmadır.
        [Header("Placement ghost - assign a child SpriteRenderer, kept disabled at rest")]
        [SerializeField] private SpriteRenderer placementGhost;

        // EŞİK DÜNYA BİRİMİNDE, PİKSELDE DEĞİL — ve bu bir karardır.
        // Piksel seçilseydi aynı parmak hareketi 1920'lik ekranda "tıklama",
        // 2560'lık ekranda "sürükleme" sayılırdı; yani giriş ŞEKLİ ekran
        // çözünürlüğüne bağlı olarak değişirdi. Aynı tuzağın kardeşi
        // HandleClick'te zaten yazılı: ScreenToWorldPoint tam da bu yüzden var.
        // Dünya birimi ayrıca ÖLÇÜLEBİLİR bir anlam taşır: 0,25 "çeyrek hücre".
        [Header("Pointer gesture")]
        [Tooltip("How far the pointer must travel, in WORLD units, before a press counts as a drag.")]
        [SerializeField, Min(0f)] private float dragThreshold = 0.25f;

        [Tooltip("Key that enters structure placement mode. Requires a selected unit.")]
        [SerializeField] private KeyCode placementModeKey = KeyCode.B;

        [Tooltip("Key that cancels structure placement mode without touching the board.")]
        [SerializeField] private KeyCode placementCancelKey = KeyCode.Escape;

        [Header("Structure stats - applied to every placed structure")]
        [Tooltip("Starting and maximum health of each placed structure.")]
        [SerializeField, Min(1)] private int structureMaxHealth = 50;

        // Unity'nin Grid bileşeni SADECE bir koordinat çevirmenidir:
        // hücre indeksi <-> dünya konumu. Hiçbir şey çizmez, kaç hücre
        // olduğunu bilmez, oyun durumu tutmaz. Tuttuğu tek şey ayarlardır
        // (cellSize, cellGap, cellLayout).
        private Grid unityGrid;

        // Tahtanın ve savaşın durumu BURADA DEĞİL, Battle'ın içinde yaşar: kaç
        // hücre var, hangi hücrede kim duruyor, kimin canı ne. Bu alan yalnızca
        // o bütüne bir tutamaktır.
        //
        // Burada bir `private UnitGrid board;` alanı vardı ve Awake onu kendisi
        // kuruyordu. O alanın silinmesi bu dosyanın en pahalı satırı: tahtaya
        // yazan tek yol artık Battle.AddUnit ve Combatant'ı olmayan bir birimin
        // tahtada durması imkânsız hâle geldi.
        private Battle battle;

        // Core'daki Unit ile ekrandaki görselini eşleyen tablo.
        //
        // Anahtar neden Unit? Çünkü KONUM sadece tahtada yaşasın istiyoruz.
        // Görsel "neredeyim" bilmez; konumu her gerektiğinde Battle'dan
        // hesaplanır. Alternatifi (GameObject[,] paralel dizi) konumu iki
        // yerde tutardı ve ikisi kayarsa hata sessiz olurdu.
        //
        // Equals/GetHashCode yazmaya gerek yok: Unit bir sınıftır, varsayılan
        // karşılaştırma REFERANS eşitliğidir ve aradığımız zaten tam olarak o
        // nesnenin kendisi. Değer eşitliği ancak "aynı içerikli iki ayrı Unit
        // aynı anahtar sayılsın" istenirse gerekirdi; istemiyoruz.
        //
        // REFAKTÖR NOTU GERÇEKLEŞTİ (seçim çerçevesi): not tam olarak bunu
        // öngörüyordu ve aynen öyle oldu - tablo silinmedi, ANAHTARI değişmedi,
        // yalnızca DEĞER tipi GameObject yerine UnitView oldu. UnitView bu
        // tasarımın yerine geçmedi, üstüne geldi.
        //
        // Kazanılan şey: değer artık "bir nesne" değil, KONUŞULABİLİR bir
        // arayüz. Eskiden seçimi uygulamak için adaptör GetComponent ile
        // görselin içini kurcalıyordu; şimdi view.SetSelected(...) diyor ve
        // çerçevenin bir çocuk nesnede yaşadığını hiç bilmiyor.
        //
        // Aynı anahtar seçimi Battle tarafında da devralındı; gerekçesi
        // Battle'ın combatants sözlüğünün üstünde ve bu satırlara adıyla
        // atıf yapıyor.
        private readonly Dictionary<Unit, UnitView> unitViews =
            new Dictionary<Unit, UnitView>();

        // Şu an seçili birim. null = seçim yok.
        private Unit selectedUnit;

        // Temizlik süpürmesinin ÇIKIŞ tamponu. Alan olmasının tek sebebi
        // TAHSİS: her karede yeni bir List kurmak kare başına çöp üretirdi.
        // Battle.RemoveReadyForCleanup onu her çağrıda temizleyip yeniden
        // doldurur, yani bu alanın çağrılar arasında taşıdığı bir anlam YOK —
        // bir durum değil, yeniden kullanılan bir kaptır.
        private readonly List<Unit> cleanupBuffer = new List<Unit>();

        // Tıklama ile sürüklemeyi ayıran saf tip. Alan olmasının sebebi TAM
        // OLARAK durum tutması: basıldığı nokta ve eşiğin aşılıp aşılmadığı
        // kareler ARASINDA yaşamak zorunda. cleanupBuffer'ın tersi bir alan —
        // orası yeniden kullanılan bir kap, burası gerçek bir hafıza.
        //
        // Kurucusu eşiği dışarıdan istiyor (S-03'ün zaman kararının ikizi), bu
        // yüzden Awake'te kuruluyor: serileştirilmiş alan ancak o an okunabilir.
        private PointerGesture gesture;

        // Yerleştirme kipinde miyiz. Bir OYUN durumudur, çeviri durumu değil —
        // yani bu alan da selectedUnit gibi rol başlığındaki "hafıza: var"
        // satırının altına düşer.
        private bool isPlacingStructure;

        // Hayalet fareye YAPIŞTI mı. İki giriş şeklini ayıran tek alan budur.
        //
        //   sürükle-bırak : Press -> MoveTo... -> DragReleased  -> YERLEŞTİR
        //                   (hiç yapışmaz; bu alan false kalır)
        //   tıkla-bırak   : Press -> ClickReleased -> YAPIŞTI (kipte kal)
        //                   Press -> ClickReleased -> YERLEŞTİR
        //
        // Neden bir sayaç değil bir bool: ayrım "kaçıncı tıklama" değil, hayalet
        // fareye bağlı mı bağlı değil mi. Sayaç yazsaydık üçüncü tıklamanın ne
        // anlama geldiği tanımsız kalırdı.
        private bool ghostIsCarried;

        private void Awake()
        {
            // GetComponent bir SORGUdur: bu GameObject'in bileşen listesinde
            // arar ve bulduğuna referans döner. Hiçbir şey yaratmaz, tekrar
            // çağrılması durumu değiştirmez. Listede bir Grid bulunacağını
            // RequireComponent garanti eder; Grid'i "üreten" o değildir.
            unityGrid = GetComponent<Grid>();
            battle = new Battle(width, height);

            // Eşik Inspector'dan geliyor, bu yüzden jest nesnesi ancak burada
            // kurulabilir: alan bildiriminde `new PointerGesture(dragThreshold)`
            // yazsaydık serileştirilmiş değer daha okunmamış olurdu ve nesne
            // her zaman C# başlatıcısındaki sayıyla doğardı — Inspector'daki
            // değer sessizce hiçbir işe yaramazdı.
            gesture = new PointerGesture(dragThreshold);

            // Hayalet, kipte OLMADIĞIMIZ sürece çizilmez. Sahnede açık
            // bırakılmış olabilir; UnitView.Awake'in SetSelected(false) ile
            // yaptığı işin birebir aynısı ve gerekçesi de aynı: yazılı durumu
            // çalışma zamanı değişmezine çevirmek.
            if (placementGhost == null)
            {
                Debug.LogError(
                    "[Board] placementGhost is not assigned. Assign a child SpriteRenderer; structure placement mode will refuse to start without it.",
                    this);
            }
            else
            {
                placementGhost.enabled = false;
            }

            BuildCellVisuals();

            // GEÇİCİ: iki demo birim. Oyun kurulumu geldiğinde buradan kalkacak.
            // İKİSİ de gerekli ve bu bir tercih değil: saldırı zincirinin
            // kapandığını göstermek için birbirine tıklanabilen İKİ birim şart,
            // ve TargetingRules dost ateşini reddettiği için tarafları farklı
            // olmak zorunda. Komşu hücreler seçildi ki menzil 1 ile denenebilsin.
            if (unitPrefab != null)
            {
                SpawnUnit("Vanguard", Team.Player, 1, 2);
                SpawnUnit("Raider", Team.Enemy, 1, 3);
            }
            else
            {
                Debug.LogError("[Board] unitPrefab is not assigned. Assign the Unit prefab (it must carry a UnitView component) in the Inspector.", this);
            }
        }

        // ═══ ABONELİK — VE NEDEN OnEnable/OnDisable ÇİFTİ ════════════════
        //
        // Bu abonelik bir HATA DÜZELTMESİDİR, bir özellik değil. Bugüne kadar
        // ekran yalnız SALDIRIDAN sonra tazeleniyordu; oysa Downed → Dead
        // geçişi Tick'in içinde, hiçbir tıklama olmadan gerçekleşir. Yani
        // düşmüş bir birim ekranda YATIK kalıyor, gri hiç olmuyordu ve hatayı
        // gösterecek tek şey gözdü — hiçbir test kırmızı değildi.
        //
        // ÇİFTİN SİMETRİSİ: OnEnable her etkinleşmede ÇALIŞIR, dolayısıyla
        // OnDisable'da bırakılmazsa abonelik BİRİKİR ve aynı olay iki kez
        // dinlenir. Bugün bu "iki kat iş" gibi görünür çünkü SetState
        // idempotenttir — ve tam bu yüzden tehlikelidir: hata SESSİZDİR ve ilk
        // yan etkili dinleyici eklendiği gün patlar.
        //
        // ASIL KIRILMA sızıntı değil ÖMÜR: olay, Battle'dan bu MonoBehaviour'a
        // referans TUTAR. Battle bu bileşenden UZUN yaşadığı gün (kayıtlı oyun,
        // sunucu tarafı simülasyon) bırakılmamış abonelik YOK EDİLMİŞ bir
        // MonoBehaviour'ı çağırır ve kaynağından çok uzakta patlar.
        //
        // REDDEDILEN - BoardAdapter.cs:479 yerine (abonelik Awake'te kurulur,
        //              OnDestroy'da bırakılır):
        //     private void Awake()    { battle.UnitStateChanged += OnUnitStateChanged; }
        //     private void OnDestroy() { battle.UnitStateChanged -= OnUnitStateChanged; }
        // KIRILAN  : kırılan şey "kapalı" sözünün kendisi; çift abonelik YOK.
        //            bileşen kapatılır -> Update durur, dinleme DEVAM eder
        //            Battle'ı başka bir yol Tick'lerse -> kapalı adaptör
        //            sahnedeki görselleri değiştirmeye devam eder
        //            derleyici: hiçbir şey der  .  test: bugün hiçbiri sormaz
        // KAZANIRDI: abonelik nesnenin ÖMRÜ boyunca bir kez kurulup bir kez
        //            bırakılan bir KAYNAK olsaydı — dosya, soket, bildirim kaydı;
        //            onlar açılıp kapanmayla değil doğup ölmeyle eşleşir.
        // TEK CUMLE: Awake/OnDestroy nesnenin DOĞUMUNU, OnEnable/OnDisable ise
        //            ETKİNLİĞİNİ eşler; olay dinlemek etkinliğe aittir.
        private void OnEnable()
        {
            battle.UnitStateChanged += OnUnitStateChanged;
        }

        private void OnDisable()
        {
            battle.UnitStateChanged -= OnUnitStateChanged;

            // Kip, bileşen kapanırken BIRAKILIR. Bırakılmasaydı yeniden
            // açılan adaptör hayaleti gizli, kipi açık bulurdu: fare tıklaması
            // görünmez bir yapıyı yerleştirirdi.
            CancelPlacement();
        }

        /// <summary>
        /// Savaş bir birimin durumunu değiştirdiğinde ekranı tazeler.
        /// </summary>
        // İMZA `Action<Unit, UnitState, UnitState>`: KİM, nereden, nereye.
        // "Nereden" bugün KULLANILMIYOR ve bu bir eksiklik değil — olayın
        // taşıdığı bilgi ile bu dinleyicinin ihtiyacı aynı olmak zorunda değil.
        // Kullanacağı ilk gün adı hazır: Alive → Downed düşme animasyonu,
        // Downed → Alive diriliş animasyonu; ikisi de "nereye"den türetilemez.
        private void OnUnitStateChanged(Unit unit, UnitState from, UnitState to)
        {
            ApplyStateVisual(unit, to);
        }

        private void Update()
        {
            // ZAMAN HER KARE İLERLER, tıklama olsun olmasın — ve bu sıra bir
            // karardır: erken çıkışın ALTINA konsaydı savaşın saati yalnızca
            // oyuncu tıkladığında işlerdi, yani düşmüş bir birim el sürülmediği
            // sürece asla ölmezdi.
            AdvanceBattleTime();

            // KİP AYRIMI, MEVCUT AKIŞIN ÜSTÜNDE — ve sıra bir karardır.
            // Altına konsaydı yerleştirme sırasındaki her basış önce
            // HandleClick'ten geçerdi: hayalet taşınırken tahtadaki birimler
            // seçilir, saldırı emri verilir, hareket denenirdi. Kip, girdinin
            // ANLAMINI baştan sona değiştirir; dolayısıyla ayrım en başta
            // yapılır.
            //
            // "hayır" dalı DEĞİŞMEDİ: kip kapalıyken bu dosyanın giriş akışı
            // yerleştirme kipi eklenmeden önceki hâliyle birebir aynıdır.
            if (isPlacingStructure)
            {
                UpdatePlacement();
                return;
            }

            if (Input.GetKeyDown(placementModeKey))
            {
                TryEnterPlacementMode();
                return;
            }

            // "Down" = SADECE basıldığı karede true. Tuşu basılı tutarsan sonraki
            // karelerde false döner; GetMouseButton (Down'suz) ise basılı olduğu
            // her karede true olurdu. Tek tıklama istiyoruz, o yüzden Down.
            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            HandleClick();
        }

        /// <summary>
        /// Yerleştirme kipine girmeyi dener. Seçili birim ve atanmış bir hayalet
        /// şart.
        /// </summary>
        // YAPIYI KİM KOYAR: seçili birim. Bu bir çeviri değil bir OYUN
        // kuralıdır ve doğru sahibi burası DEĞİL — BattleActions.PlaceStructure
        // yerleştirmenin geçerliliğine kendisi karar verir. Buradaki tek şart
        // teknik: imzanın istediği `unit` argümanını verebilmek için elde bir
        // birim olmak zorunda.
        private void TryEnterPlacementMode()
        {
            if (selectedUnit == null)
            {
                Debug.Log("[Board] Select a unit before entering structure placement mode.", this);
                return;
            }

            if (placementGhost == null)
            {
                Debug.LogError("[Board] Cannot enter placement mode: placementGhost is not assigned.", this);
                return;
            }

            isPlacingStructure = true;

            // Kipe her girişte hayalet SERBESTTİR: ilk bırakış onu ya
            // yerleştirir (sürükleme) ya fareye yapıştırır (tıklama).
            ghostIsCarried = false;

            // Jest, kipler ARASINDA taşınmaz. Sıfırlanmasaydı önceki kipten
            // kalan "Pressed" fazı, bu kipteki ilk bırakışı sahte bir tıklama
            // olarak okurdu.
            gesture.Reset();

            placementGhost.enabled = true;
            Debug.Log($"[Board] Placement mode ON for '{selectedUnit.Name}'. Drag and release to place, or click to carry.", this);
        }

        /// <summary>
        /// Yerleştirme kipinin tek karesi: hayaleti taşır, jesti besler ve
        /// bırakma şekline göre yerleştirir.
        /// </summary>
        private void UpdatePlacement()
        {
            // KOYACAK BİRİM ARADA KAYBOLABİLİR — ve bu teorik değil, bugün
            // gerçekleşen bir sıra: AdvanceBattleTime bu metottan ÖNCE koşar ve
            // ceset süresi dolan birimi DespawnView ile temizlerken
            // selectedUnit'i null'a çeker. Bu kontrol olmasaydı yerleştirme
            // anında PlaceStructure'a null gider ve savaş katmanı, sebebi
            // ekranda hiç görünmeyen bir exception atardı.
            if (selectedUnit == null)
            {
                CancelPlacement();
                Debug.Log("[Board] Placement mode ended: the placing unit left the battle.", this);
                return;
            }

            // İPTAL HER ZAMAN ÖNCE. Aşağıya konsaydı iptal tuşu, aynı karede
            // gelen bir bırakışın yerleştirmesinden SONRA işlenirdi: oyuncu
            // iptal ettiğini sanır, tahtada bir yapı bulurdu.
            if (Input.GetKeyDown(placementCancelKey))
            {
                CancelPlacement();
                Debug.Log("[Board] Placement mode CANCELLED. The board was not touched.", this);
                return;
            }

            if (!TryReadPointerCell(out float worldX, out float worldY, out int x, out int y))
            {
                return;
            }

            // HER KARE, koşulsuz: hayalet fare hücresinin MERKEZİNDE durur.
            // Yalnız sürüklerken taşınsaydı tıkla-bırak akışında hayalet
            // yerinde donar ve oyuncu nereye koyacağını göremezdi — iki giriş
            // şeklinin ikisinde de takip etmesi şartının somut karşılığı bu
            // satırın koşulsuz olmasıdır.
            placementGhost.transform.position = CellCentre(x, y);

            // HAYALETİN GEÇERSİZ HÜCREDE FARKLI GÖRÜNMESİ (kırmızı tint):
            // HENÜZ YAPILMIYOR ve tetiği yazılı. Sebep "önemsiz" değil,
            // SAHİPLİK: yerleştirmenin geçerli olup olmadığına
            // BattleActions.PlaceStructure karar verir ve cevabını ancak
            // YERLEŞTİREREK verir. Her kare rengi boyamak için ya tahtayı her
            // kare mutasyona uğratmak (yukarıdaki REDDEDILEN'in ta kendisi) ya
            // da kuralın bir KOPYASINI buraya yazmak gerekirdi — "hücre dolu mu,
            // tahta içinde mi" — ve o kopya, kural büyüdüğü gün (sıra, menzil,
            // maliyet) sessizce YALAN söylemeye başlardı: yeşil hayalet,
            // reddedilen yerleştirme.
            // ÖNEM KAZANACAĞI KOŞUL, tek cümlede: BattleActions tahtaya
            // dokunmayan bir soru üyesi kazandığı gün — `CanPlaceStructure(...)`
            // ya da `PlacementOutcome`u mutasyonsuz hesaplayan bir önizleme —
            // hayalet o üyeyi her kare sorar ve tint kuralın KOPYASINI değil
            // CEVABINI taşır.

            PointerPhase phase = FeedGesture(worldX, worldY);

            switch (phase)
            {
                // SÜRÜKLE-BIRAK: bırakıldığı yer yerleştirilecek yerdir.
                case PointerPhase.DragReleased:
                    CommitPlacement(x, y);
                    break;

                case PointerPhase.ClickReleased:
                    if (ghostIsCarried)
                    {
                        // TIKLA-BIRAK, ikinci tıklama: yerleştir.
                        CommitPlacement(x, y);
                    }
                    else
                    {
                        // TIKLA-BIRAK, ilk tıklama: kipte KAL, hayalet fareyi
                        // takip etmeye devam etsin.
                        ghostIsCarried = true;
                        gesture.Reset();
                        Debug.Log("[Board] Ghost is now carried. Click again to place, or cancel.", this);
                    }

                    break;
            }

            // default DALI BİLEREK YOK ve bu ReactToAttack'teki kararla
            // ÇELİŞMİYOR: orada switch bir sonucun BÜTÜN değerlerini karşılamak
            // zorunda, burada ise beş fazın üçü (Idle, Pressed, Dragging)
            // "henüz bir şey olmadı" demektir. Bir sonuç enum'unda işlenmeyen
            // değer bir hatadır; bir faz enum'unda işlenmeyen faz normal akıştır.
        }

        /// <summary>
        /// Motorun üç fare sorgusunu <see cref="PointerGesture"/>'ın üç metoduna
        /// çevirir ve ortaya çıkan fazı verir.
        /// </summary>
        // Gerekçenin tamamı dosyanın başındaki GİRİŞ OKUMA NOTU'nda; buradaki
        // şekil o notun kodu. Down/MoveTo bir if-else zinciri, Up ise AYRI bir
        // if — çünkü tek bir karede Down ve Up birlikte true olabilir.
        private PointerPhase FeedGesture(float worldX, float worldY)
        {
            PointerPhase phase = gesture.Phase;

            if (Input.GetMouseButtonDown(0))
            {
                phase = gesture.Press(worldX, worldY);
            }
            else if (Input.GetMouseButton(0))
            {
                phase = gesture.MoveTo(worldX, worldY);
            }

            if (Input.GetMouseButtonUp(0))
            {
                phase = gesture.Release(worldX, worldY);
            }

            return phase;
        }

        /// <summary>
        /// Yerleştirmeyi savaşa SORAR ve cevabına göre ekranı günceller.
        /// </summary>
        private void CommitPlacement(int x, int y)
        {
            // Geçerliliğe BU DOSYA KARAR VERMİYOR. Tek satırlık kanıtı şu:
            // aşağıda ne bir sınır kontrolü, ne bir "hücre dolu mu" sorusu, ne
            // de bir sıra sorusu var. Hepsi PlaceStructure'ın içinde ve orada
            // kalmalı — çeviri ile karar arasındaki sınır tam olarak burası.
            Unit placer = selectedUnit;
            PlacementOutcome outcome =
                BattleActions.PlaceStructure(battle, placer, NewStructure(placer), x, y);

            // KİPTEN ÇIKIŞ, sonuçtan BAĞIMSIZ: ret de bir cevaptır ve oyuncu
            // reddedilen bir yerleştirmeden sonra hayaletin fareye yapışmaya
            // devam etmesini beklemez. Reddi düzeltmenin yolu kipe yeniden
            // girmektir, çünkü ret sebebi çoğu zaman hücre değil BİRİMdir.
            CancelPlacement();

            // TEK BİR DEĞERLE KARŞILAŞTIRMA, TAM SWITCH DEĞİL — ve bu bilinçli
            // bir eksikliktir, üslup değil. Buradaki tek soru "kondu mu": kondu
            // ise görsel doğar, konmadıysa sebebi aşağıdaki Debug.Log zaten
            // basıyor ve bu dosyanın ret sebebine göre yapacağı FARKLI bir işi
            // yok. ReactToAttack'teki tam switch'in sebebi tersi: orada her ret
            // ayrı bir mesaj ve ayrı bir oyuncu yönlendirmesi üretiyor.
            //
            // EŞİK, ve tetiği net: bir ret sebebi ekranda FARKLI bir şey
            // yaptırdığı gün bu karşılaştırma, ReactToAttack ve ReactToMove ile
            // aynı şekle — her ret için bir dal, default'ta LogError — çevrilir.
            if (outcome == PlacementOutcome.Placed)
            {
                CreateStructureVisual(x, y);
            }

            Debug.Log($"[Board] '{placer.Name}' placement at ({x},{y}) -> {outcome}.", this);
        }

        /// <summary>
        /// Yerleştirme kipini kapatır ve hayaleti gizler. Tahtaya DOKUNMAZ.
        /// </summary>
        // İptalin tahtaya dokunmaması bir tesadüf değil, hayaletin gerçek bir
        // yapı OLMAMASININ doğrudan sonucudur: geri alınacak bir şey yok, çünkü
        // yapılmış bir şey yok. Alanların üstündeki REDDEDILEN bloğu bu metodun
        // alternatif hâlini — `battle.RemoveStructure(...)` çağrısını — ve onun
        // unutulduğu günkü sonucunu yazıyor.
        private void CancelPlacement()
        {
            isPlacingStructure = false;
            ghostIsCarried = false;
            gesture?.Reset();

            if (placementGhost != null)
            {
                placementGhost.enabled = false;
            }
        }

        /// <summary>
        /// Inspector'daki sayılardan bir yapı kurar.
        /// </summary>
        // TARAF, YAPIYI KOYAN BİRİMDEN OKUNUR — Inspector'dan DEĞİL. Ayrı bir
        // alan koysaydık aynı bilginin ikinci kaynağı doğardı ve düşmanın
        // yaptığı bina oyuncunun tarafında görünebilirdi; hata sessiz olurdu.
        //
        // AttackProfile verilmiyor: Structure'ın kurucusu onu isteğe bağlı
        // tutuyor ve gerekçesi o dosyada yazılı — saldırmayan yapı KURALdır,
        // saldıran yapı istisnadır. Bugün koyduğumuz şey bir depodur.
        private Structure NewStructure(Unit placer)
        {
            Team team = battle.TryGetCombatant(placer, out Combatant combatant)
                ? combatant.Team
                : Team.None;

            return new Structure(new Health(structureMaxHealth), new StructureLifecycle(), team);
        }

        /// <summary>
        /// Yerleşen yapının tahtadaki görselini doğurur.
        /// </summary>
        // NEDEN PREFAB DEĞİL, KODLA KURULAN BİR GameObject: kod tarafı sahne
        // ve prefab dosyalarını üretemez, dolayısıyla atanması gereken bir alan
        // daha eklemek "Inspector'da boş kalan alan" riskini büyütürdü.
        // Sprite hayaletten okunuyor: önizlemede görülen şeyin tahtaya konan
        // şeyle AYNI görünmesi böylece kurulum adımı gerektirmeden garanti
        // altına alınıyor. Aynı deseni CreateCellVisual zaten kullanıyor.
        //
        // GÖRSEL BİR TABLOYA KAYDEDİLMİYOR ve sınırı burada yazıyorum: bugün
        // onu tekrar bulması gereken hiçbir çağıran yok — yerleşen yapılar
        // yıkılmıyor, taşınmıyor, seçilmiyor. Yıkım geldiği gün bu görselin
        // sahibi `Dictionary<Unit, StructureView>` olur ve o tip, UnitView'ın
        // kardeşi olarak doğar; bugün onu yazmak, alıcısı olmayan bir tablo
        // ve senkron tutulması gereken üçüncü bir sözlük demek olurdu.
        private void CreateStructureVisual(int x, int y)
        {
            var structureObject = new GameObject($"Structure_{x}_{y}");
            structureObject.transform.SetParent(transform, worldPositionStays: false);
            structureObject.transform.position = CellCentre(x, y);

            var renderer = structureObject.AddComponent<SpriteRenderer>();
            renderer.sprite = placementGhost.sprite;

            // Zemin 0, birimler ve yapılar 1: yapı zeminin üstünde çizilir.
            renderer.sortingOrder = 1;
        }

        /// <summary>
        /// Fare konumunu dünya koordinatına ve hücre indeksine çevirir.
        /// Bir Unity tipinin Core'un diline çevrildiği TEK yer burasıdır.
        /// </summary>
        // İKİ ÇAĞIRAN, TEK ÇEVİRİ: tıklama akışı (HandleClick) ve yerleştirme
        // kipi (UpdatePlacement). Çeviri kopyalansaydı biri değiştiğinde fare
        // ile hayalet farklı hücreleri gösterirdi ve hiçbir şey patlamazdı —
        // CellCentre'ın kendi özetindeki gerekçenin aynadaki hâli.
        //
        // DÖNÜŞ bool + out, nullable DEĞİL: S-05'in kararı. "Kamera yok" bir
        // programcı hatasıdır ve çağıranın yapacağı tek şey akıştan çıkmaktır;
        // out ile birlikte tek bir if yeter.
        //
        // Camera.main, "MainCamera" ETİKETLİ kamerayı bulur; "ana kamera" diye
        // bir kavram yoktur, etiket vardır. Etiketli kamera yoksa null döner ve
        // bir sonraki satır patlardı.
        //
        // Input.mousePosition EKRAN pikselidir: sol alt (0,0), sağ üst
        // (ekranGenişliği, ekranYüksekliği). Kameranın konumu değildir.
        // ScreenToWorldPoint bu pikseli dünya birimine çevirir ve çeviri
        // KAMERAYA bağlıdır: kamera taşınırsa aynı piksel farklı bir dünya
        // noktasına düşer. Çeviri olmasaydı 1920'lik ve 2560'lık ekranda aynı
        // tıklama farklı hücreyi seçerdi. dragThreshold'un DÜNYA biriminde
        // ölçülmesinin gerekçesi de tam olarak bu cümledir.
        private bool TryReadPointerCell(out float worldX, out float worldY, out int x, out int y)
        {
            worldX = 0f;
            worldY = 0f;
            x = 0;
            y = 0;

            if (Camera.main == null)
            {
                Debug.LogError("[Board] No camera tagged MainCamera in the Scene.", this);
                return false;
            }

            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldX = worldPoint.x;
            worldY = worldPoint.y;

            // Vector3Int sınırın ötesine geçmez; "tahta içinde mi" sorusunu
            // soran taraf yine Battle'dır, bu metot değil.
            Vector3Int cell = unityGrid.WorldToCell(worldPoint);
            x = cell.x;
            y = cell.y;
            return true;
        }

        /// <summary>
        /// Savaşın saatini ilerletir ve ceset süresi dolanları hem savaştan hem
        /// ekrandan kaldırır.
        /// </summary>
        // ZAMANI BURADAN VERMEK ZORUNLU: UnitLifecycle bilerek Time.deltaTime
        // okumuyor — ölçümü UnitLifecycle.Tick'in üstündeki REDDEDILEN bloğu
        // taşıyor: EditMode'da o değer sıfır değil, 0,017675 dönüyor ve testi
        // sessizce anlamsızlaştırıyordu. Saatin tek gerçek kaynağı motorda;
        // motoru gören tek katman burası.
        //
        // TEMİZLİK NEDEN YOKLAMA DEĞİL TOPLU: seçenek "her karede her savaşçıyı
        // yokla" ile "Battle'a süpürme metodu ekle" arasındaydı ve gerekçesi
        // Battle.RemoveReadyForCleanup'ın REDDEDILEN bloğunda kod olarak
        // yazılı; özeti tek satır: yoklama ancak GÖRSELİ olan birimleri görür,
        // oysa temizlenmesi gereken şey savaşın kaydıdır.
        //
        // BU SATIRIN ESKİ HÂLİ "Combatant durum değişimini dışarı vermiyor,
        // dolayısıyla Downed → Dead geçişini kimse duyamıyor" diyordu. ARTIK
        // DUYULUYOR (Battle.UnitStateChanged) ve OnEnable tam olarak onu
        // dinliyor. Süpürmenin gerekçesi yine de AYAKTA kalıyor, çünkü ikisi
        // FARKLI iki soruya cevap veriyor — S-07'nin üçüncü ayrımı: olay
        // "durum değişti"yi taşır, süpürme "artık silinebilir"i. Bir birim
        // Dead'e geçtiği an ekranda gri olur ama savaşın kaydından ancak ceset
        // penceresi dolunca çıkar; olay o ikinci anı hiç bilmez.
        private void AdvanceBattleTime()
        {
            battle.Tick(Time.deltaTime);

            if (battle.RemoveReadyForCleanup(cleanupBuffer) == 0)
            {
                return;
            }

            for (int i = 0; i < cleanupBuffer.Count; i++)
            {
                DespawnView(cleanupBuffer[i]);
            }
        }

        private void BuildCellVisuals()
        {
            // LogError, Log değil: bu bir PROGRAMCI hatasıdır (kurulum eksik),
            // oyun akışının normal bir sonucu değil. Kırmızıdır ve filtrelenebilir.
            // return ile birlikte gelir: sprite yoksa 15 görünmez GameObject
            // üretmektense gürültüyle durmak yeğdir.
            if (terrainSprites == null || terrainSprites.Length == 0)
            {
                Debug.LogError(
                    "[Board] terrainSprites is empty. Assign at least one Sprite in the Inspector.",
                    this);
                return;
            }

            for (int x = 0; x < battle.Width; x++)
            {
                for (int y = 0; y < battle.Height; y++)
                {
                    CreateCellVisual(x, y);
                }
            }

            Debug.Log($"[Board] built {battle.Width}x{battle.Height} = {battle.CellCount} cells.", this);
        }

        private void CreateCellVisual(int x, int y)
        {
            var cell = new GameObject($"Cell_{x}_{y}");

            // Çıplak "transform" = this.transform, yani BU bileşenin bağlı olduğu
            // GameObject'in Transform'u. Component sınıfından miras gelir.
            // Ebeveyn-çocuk hiyerarşisi GameObject'te değil Transform'da yaşar.
            // Amaç konum değil TOPLU YAŞAM DÖNGÜSÜ: tahtayı yok etmek, gizlemek
            // veya taşımak tek çağrıyla 15 hücreye birden uygulanır.
            cell.transform.SetParent(transform, worldPositionStays: false);

            // Hücrenin MERKEZİ, CellToWorld değil: köşe kullanılsaydı her hücre
            // yarım kare kaymış görünürdü.
            cell.transform.position = CellCentre(x, y);

            // AddComponent bir MUTASYONdur: her çağrı yeni bir bileşen ekler.
            // GetComponent'in aksine idempotent değildir, bu yüzden kurulum
            // kodunda yaşar; kare başına çalışan bir yere konulamaz.
            var renderer = cell.AddComponent<SpriteRenderer>();

            // SpriteRenderer ÇİZER; sprite ise çizilecek varlıktır. Çizen ile
            // çizilen ayrı şeylerdir.
            renderer.sprite = PickTerrainSprite(x, y);

            // Çizim önceliği: aynı katmanda büyük değer üste çizilir. Zemin 0;
            // üzerine gelen birimler 1 alır ve zeminin üstünde görünür.
            renderer.sortingOrder = 0;
        }

        /// <summary>
        /// Hücre koordinatından zemin sprite'ı seçer.
        /// </summary>
        private Sprite PickTerrainSprite(int x, int y)
        {
            // DETERMİNİSTİK: aynı hücre her Play'de aynı sprite'ı alır.
            // Random olsaydı her çalıştırma farklı görünür ve gördüğün bir hatayı
            // tekrar üretmek imkansızlaşırdı. 7 ve 13 asal sayıdır; çarpanların
            // ortak böleni olmaması düzenli şerit deseni oluşmasını engeller.
            // x ve y döngüden gelir, ikisi de >= 0; negatif olabilseydi sonuç
            // negatif çıkabileceği için Mathf.Abs gerekirdi.
            int index = (x * 7 + y * 13) % terrainSprites.Length;
            return terrainSprites[index];
        }

        /// <summary>
        /// Hücre indeksini dünya konumuna çeviren TEK yer. Üç çağıranı var
        /// (zemin kurulumu, birim doğuşu, hareket) ve üçü de aynı cevabı almak
        /// zorunda; çeviri kopyalansaydı biri değiştiğinde birimler zeminden
        /// kayardı ve hiçbir şey patlamazdı.
        /// </summary>
        private Vector3 CellCentre(int x, int y)
        {
            return unityGrid.GetCellCenterWorld(new Vector3Int(x, y, 0));
        }

        /// <summary>
        /// Savaşa bir birim katar ve ekrandaki karşılığını doğurur.
        /// </summary>
        private void SpawnUnit(string name, Team team, int x, int y)
        {
            var unit = new Unit(name);

            // Önce KURAL, sonra görsel. AddUnit dolu hücreyi ve tahta dışını
            // exception ile reddeder; o hata görsel doğmadan patlasın ki ekranda
            // karşılığı olmayan bir birim asla oluşmasın.
            //
            // REDDEDILEN - BoardAdapter.cs:1002 yerine (birim doğrudan tahtaya
            //              yazılır, savaş kaydı atlanır):
            //     board.PlaceUnit(x, y, unit);
            // KIRILAN  : tahtada duran ama Combatant'ı OLMAYAN bir birim doğar
            //            (aynı kırılma Battle'ın kurucu bloğunda adlandırılmıştı).
            //            ekranda görünür, tıklanır, seçilir -> ilk saldırıda
            //            BattleActions.Attack "The unit is not in this battle."
            //            diye patlar (BattleActions'ın kimlik kapısı)
            //            derleyici: hiçbir şey der  .  test: hata ancak Play'de
            // KAZANIRDI: tahtada savaşmayan bir şeyin durması gerekseydi —
            //            dekor, bayrak, hedef işareti; ama o tür de Unit değil,
            //            Structure'ın kardeşi olurdu.
            // TEK CUMLE: Tahtaya yazmanın tek kapısı Battle olmazsa "tahtada
            //            var" ile "savaşta var" iki ayrı gerçek hâline gelir.
            battle.AddUnit(unit, NewCombatant(team), x, y);

            // Instantiate, prefab dosyasından YENİ bir kopya doğurur. Prefab'ın
            // kendisi sahneye girmez; sahnede duran her zaman bir kopyadır.
            // İkinci parametre ebeveyni verir: hücreler gibi birimler de
            // tahtanın çocuğu olur, böylece tahta yok olunca birlikte gider.
            //
            // Argüman UnitView olduğu için dönüş de UnitView'dır - Instantiate
            // generic'tir ve verdiğin tipi geri verir. Bu yüzden burada tek bir
            // GetComponent yok: kopya doğduğu anda zaten aradığımız tipte.
            //
            // view.name yazmak GameObject'in adını değiştirir; name property'si
            // Component üzerinden GameObject'e iletilir. Ayrı bir isim alanı yok.
            UnitView view = Instantiate(unitPrefab, transform);
            view.name = $"Unit_{unit.Name}_{x}_{y}";
            view.transform.position = CellCentre(x, y);

            unitViews.Add(unit, view);
        }

        /// <summary>
        /// Inspector'daki sayılardan bir savaşçı kurar.
        /// </summary>
        private Combatant NewCombatant(Team team)
        {
            // YAŞAM DÖNGÜSÜ PENCERELERİ BİLEREK SERİLEŞTİRİLMEDİ, oysa can ve
            // hasar serileştirildi. Ayrım keyfi değil: "kaç saniye düşük kalır"
            // sorusunun ZATEN bir sahibi var —
            // UnitLifecycle.DefaultDownedWindowSeconds ve
            // DefaultCorpseWindowSeconds adında iki sabit. Buraya bir Inspector
            // alanı koymak aynı sayıya ikinci bir kaynak açardı ve sahnedeki
            // değer sabiti sessizce ezerdi. Can ve hasarın ise hiçbir yerde
            // varsayılanı yok; onların ilk sahibi burası.
            return new Combatant(
                new Health(maxHealth),
                new UnitLifecycle(),
                new AttackProfile(damage, attackRange),
                team);
        }

        /// <summary>
        /// Bir tıklamayı hücreye çevirir ve niyete göre dallandırır.
        /// </summary>
        private void HandleClick()
        {
            // AKIŞ DEĞİŞMEDİ, ÇEVİRİ TEK SAHİBE İNDİ. Bu metodun ilk üç adımı
            // (kamera var mı, piksel → dünya, dünya → hücre) artık
            // TryReadPointerCell'in içinde. Kopyalanmış olsalardı bu dosyadaki
            // "Bir Unity tipinin Core'un diline çevrildiği TEK yer burasıdır"
            // cümlesi artık YALAN olurdu — yerleştirme kipi aynı çeviriye
            // ikinci bir çağıran ekledi. Dallanmanın kendisi (dolu hücre →
            // saldırı, boş hücre → hareket, kendisi → seçimi bırak) satır satır
            // aynı kaldı; değişen tek şey çevirinin nereden geldiği.
            //
            // Dünya koordinatları burada KULLANILMIYOR: tıklama akışının
            // ihtiyacı olan tek şey hücre indeksi. Jest eşiği ise dünya
            // biriminde ölçüldüğü için yerleştirme tarafı ikisini birden alır.
            if (!TryReadPointerCell(out _, out _, out int x, out int y))
            {
                return;
            }

            // Debug.Log'un ikinci parametresi "context"tir: Console'da bu satıra
            // tıklayınca Unity Hierarchy'de o nesneyi vurgular. Metni değiştirmez.
            // 17 nesneli bir sahnede "bunu kim yazdı?" sorusunu tek tıkla cevaplar.
            //
            // Kural Battle'da yaşar; adaptörün işi ona uymak, onu tekrar yazmak
            // değil. Buradaki soru artık tahtaya değil savaşa soruluyor ve
            // Battle onu UnitGrid'e devrediyor — kuralın metni hâlâ tek yerde.
            if (!battle.IsInsideGrid(x, y))
            {
                Debug.Log($"[Board] ({x},{y}) is OUTSIDE the {battle.Width}x{battle.Height} board.", this);
                return;
            }

            if (battle.TryGetUnit(x, y, out Unit clicked))
            {
                HandleOccupiedCellClick(clicked, x, y);
                return;
            }

            HandleEmptyCellClick(x, y);
        }

        /// <summary>
        /// Dolu hücreye tıklandı: seçim yoksa seç, seçili olan kendisiyse
        /// seçimi bırak, başkasıysa SALDIR.
        /// </summary>
        private void HandleOccupiedCellClick(Unit clicked, int x, int y)
        {
            if (selectedUnit == null)
            {
                SelectUnit(clicked);
                Debug.Log($"[Board] ({x},{y}) holds '{clicked.Name}' - SELECTED.", this);
                return;
            }

            // KENDİ ÜSTÜNE TIKLAMAK SEÇİMİ BIRAKIR.
            //
            // REDDEDILEN - BoardAdapter.cs:1117 yerine (kendi hücresine tıklamak
            //              da bir hareket denemesidir):
            //     ReactToMove(
            //         BattleActions.Move(battle, selectedUnit, x, y, moveRange),
            //         x, y);
            // KIRILAN  : MoveAction.Execute bu çağrıyı KABUL eder — doluluk
            //            kontrolü birimin kendisini bilerek dışarıda bırakıyor.
            //            oyuncu seçimi bırakmak için tıklar -> hiçbir şey olmaz
            //            sıra sistemi bağlanır -> boş hareket tur bütçesini yer
            //            derleyici: hiçbir şey der  .  test: hem hareket hem
            //            seçim "doğru" davrandığı için kırmızı olmaz
            // KAZANIRDI: "yerinde bekle" gerçek bir tur eylemi olduğu gün —
            //            nöbet tutmak, siper almak; o gün seçimi bırakma işi
            //            başka bir girdiye (sağ tık, Esc) taşınır.
            // TEK CUMLE: Bir eylemin KABUL edilmesi, o tıklamanın o eylem
            //            demek olduğunu göstermez.
            if (ReferenceEquals(clicked, selectedUnit))
            {
                ClearSelection();
                Debug.Log($"[Board] ({x},{y}) holds the selected unit - DESELECTED.", this);
                return;
            }

            // Mesafe BURADA hesaplanmıyor ve hesaplatılmıyor bile: BattleActions
            // konumları Battle'dan bulup GridDistance'a ölçtürüyor. Bu satırın
            // bildiği tek şey "kim kime".
            AttackOutcome outcome = BattleActions.Attack(battle, selectedUnit, clicked);
            ReactToAttack(outcome, clicked, x, y);
        }

        /// <summary>
        /// Boş hücreye tıklandı: seçim varsa HAREKET, yoksa yalnızca bildir.
        /// </summary>
        private void HandleEmptyCellClick(int x, int y)
        {
            if (selectedUnit == null)
            {
                Debug.Log($"[Board] ({x},{y}) is inside the board and EMPTY.", this);
                return;
            }

            MoveOutcome outcome = BattleActions.Move(battle, selectedUnit, x, y, moveRange);
            ReactToMove(outcome, selectedUnit, x, y);
        }

        /// <summary>
        /// Saldırı sonucuna göre ekranı ve Console'u günceller.
        /// </summary>
        // SONUÇ BİR EVENT'LE GELMİYOR ve gelmemeli: soran zaten burada.
        // UnitLifecycle'in StateChanged olayının üstündeki ayrım bunu tek
        // cümleyle koyuyor — "dönüş değeri: soran zaten orada; event:
        // ilgilenen başka yerde". Saldırıyı başlatan da sonucunu gösterecek
        // olan da bu tip, dolayısıyla araya bir dinleyici koymak yalnızca
        // dolaylılık olurdu.
        private void ReactToAttack(AttackOutcome outcome, Unit target, int x, int y)
        {
            switch (outcome)
            {
                // GÖRSEL BU DALDAN TAZELENMİYOR ARTIK. Durum değişikliğinin
                // TEK tetiği Battle.UnitStateChanged oldu; saldırı da bir durum
                // değişikliği ürettiği için olay zaten yolda.
                //
                // REDDEDILEN - BoardAdapter.cs:1177 yerine (olay bağlandıktan
                //              sonra elle tazeleme de bu dalda KALIR):
                //     RefreshDownedVisual(target);
                // KIRILAN  : ekranın aynı olguya İKİ kaynağı olur; ikisi aynı
                //            cevabı verdiği için hata SESSİZDİR.
                //            olay zinciri kırılırsa -> saldırıyla düşen birim yine
                //            DOĞRU görünür, tek belirti Tick kaynaklı geçişin
                //            ekrana hiç ulaşmaması olur — aboneliğin kapattığı hata
                //            derleyici: hiçbir şey der  .  test: adaptör sınanamaz
                // KAZANIRDI: olay "durum değişti"yi değil "bir şey oldu"yu
                //            taşısaydı — yalnız Tick kaynaklı geçişler yayınlansaydı
                //            bu satır tekrar değil TEK yol olurdu (S-07'nin ayrımı).
                // TEK CUMLE: İki kaynak aynı cevabı verdiği sürece fazlalık
                //            görünür; biri sustuğunda diğeri hatayı ÖRTER.
                case AttackOutcome.HitAndDowned:
                    Debug.Log($"[Board] '{target.Name}' at ({x},{y}) was hit and went DOWN.", this);
                    break;

                case AttackOutcome.Hit:
                    Debug.Log($"[Board] '{target.Name}' at ({x},{y}) was hit. {DescribeCondition(target)}", this);
                    break;

                case AttackOutcome.RejectedOutOfRange:
                    Debug.Log($"[Board] '{target.Name}' at ({x},{y}) is OUT OF RANGE.", this);
                    break;

                case AttackOutcome.RejectedInvalidTarget:
                    Debug.Log($"[Board] '{target.Name}' at ({x},{y}) is not a valid target. {DescribeCondition(target)}", this);
                    break;

                // MESAJ HEDEFİ DEĞİL SALDIRANI ANLATIYOR ve bu, değerin
                // adındaki "Actor" sözcüğünün doğrudan karşılığı: ret sebebi
                // saldıranın kendi durumu (düşmüş) ya da sırası. Hedefi
                // değiştirmek üçünde de yardım etmez, bu yüzden log satırı
                // oyuncuyu hedefe bakmaya davet etmemeli.
                //
                // TEK MESAJ, ÜÇ SEBEP — ve bu bir tavizdir, sözleşmede öyle
                // yazılı: "sırası değil" ile "birim düşmüş" bugün ayrılmıyor.
                // EŞİK aynı yerde duruyor: arayüz oyuncuya ikisi arasındaki
                // farkı SÖYLEMEK zorunda kaldığı gün değer ikiye ayrılır.
                // Bugün tek tüketici burası ve burası yalnızca log basıyor.
                case AttackOutcome.RejectedActorCannotAct:
                    Debug.Log($"[Board] '{selectedUnit.Name}' cannot act right now; the attack was rejected. {DescribeCondition(selectedUnit)}", this);
                    break;

                // default LOG DEĞİL LogError: buraya düşmek "AttackOutcome'a yeni
                // bir değer eklendi ve bu switch güncellenmedi" demektir, yani
                // bir programcı hatasıdır. AttackOutcome'un struct REDDEDILEN
                // bloğu şunu diyor: "switch'te EKSİK DAL derleyiciden görünmez
                // olur; enum'da görünür" — ama bir switch DEYİMİ için
                // derleyici uyarmaz, o yüzden görünürlüğü bu dal sağlıyor.
                default:
                    Debug.LogError($"[Board] Unhandled attack outcome: {outcome}.", this);
                    break;
            }
        }

        /// <summary>
        /// Hareket sonucuna göre ekranı ve Console'u günceller.
        /// </summary>
        private void ReactToMove(MoveOutcome outcome, Unit unit, int x, int y)
        {
            switch (outcome)
            {
                case MoveOutcome.Moved:
                    // Görsel tahtayı TAKİP eder, tahtaya yön vermez: hareketi
                    // MoveAction çoktan yaptı, buradaki satır yalnızca sonucu
                    // gösteriyor. Ters sırada yazılsaydı (önce görseli taşı,
                    // sonra kuralı sor) reddedilen bir hareket ekranda gerçekleşmiş
                    // görünürdü.
                    MoveViewTo(unit, x, y);
                    Debug.Log($"[Board] '{unit.Name}' moved to ({x},{y}).", this);
                    break;

                case MoveOutcome.RejectedOutOfRange:
                    Debug.Log($"[Board] ({x},{y}) is further than {moveRange} cell(s) for '{unit.Name}'.", this);
                    break;

                // AŞAĞIDAKİ İKİ DAL BUGÜN ULAŞILAMAZ ve yine de yazılı. Boş
                // hücre olduğu HandleEmptyCellClick'te doğrulandı, tahta içi
                // olduğu HandleClick'te doğrulandı. Ama iki kuralın da sahibi bu
                // tip değil: MoveAction sırasını değiştirebilir, Battle sınır
                // sorusunu farklı cevaplayabilir. Yazılı bir dal bedavadır;
                // sessizce düşen bir dal, Console'da hiç görünmeyen bir hatadır.
                case MoveOutcome.RejectedCellOccupied:
                    Debug.Log($"[Board] ({x},{y}) is occupied; '{unit.Name}' stayed put.", this);
                    break;

                case MoveOutcome.RejectedInvalidDestination:
                    Debug.Log($"[Board] ({x},{y}) is not a cell on this board.", this);
                    break;

                // İKİZİ AttackOutcome'da, aynı adla — ve iki enum'da aynı adın
                // taşıdığı ÜRETİLEBİLİRLİK farkı bu dosyadan görünmez: bu değeri
                // MoveAction ASLA üretemez (ne UnitState'i ne sırayı görür),
                // yalnız BattleActions üretir. Çağıran açısından fark yok, ve
                // ret sebebinin tek işi çağıranın YAPABİLECEĞİ şeyi göstermek
                // olduğu için de olmaması doğru.
                case MoveOutcome.RejectedActorCannotAct:
                    Debug.Log($"[Board] '{unit.Name}' cannot act right now; the move was rejected. {DescribeCondition(unit)}", this);
                    break;

                default:
                    Debug.LogError($"[Board] Unhandled move outcome: {outcome}.", this);
                    break;
            }
        }

        /// <summary>
        /// Bir birimin yaşam durumunu ekrana uygular.
        /// </summary>
        // ADI DEĞİŞTİ: eski ad `RefreshDownedVisual`, sahibinin
        // cevaplayamayacağı bir soruya cevap veriyordu. Metot artık "düşme"yi
        // değil ÜÇ durumu birden uyguluyor; Downed adı, üç değerli bir bilgiyi
        // tek değerli gösteriyordu.
        //
        // DURUM ARTIK PARAMETRE, çünkü çağıran onu ZATEN taşıyor: olay
        // `Action<Unit, UnitState, UnitState>`, yani "nereye"yi elinde tutuyor.
        // battle.TryGetCombatant ile tekrar sormak aynı bilgiyi ikinci kez
        // aramak olurdu — ve iki okuma arasında geçen tek bir Tick, ekrana
        // olayın taşıdığından FARKLI bir durum yazdırabilirdi.
        //
        // GÖRSEL, SONUÇ ENUM'UNDAN DEĞİL DURUMDAN OKUNUYOR — ve fark önemli:
        // AttackOutcome.HitAndDowned yalnızca "şimdi sormaya değer" der, ne
        // gösterileceğini söylemez. Tek doğruluk kaynağı Combatant.State.
        //
        // REDDEDILEN - BoardAdapter.cs:1303 yerine (görsel doğrudan sonuçtan
        //              türetilir ve bu metot hiç doğmaz):
        //     view.SetDowned(outcome == AttackOutcome.HitAndDowned);
        // KIRILAN  : ekran ile savaş kaydı SESSİZCE ayrışır.
        //            diriltme geldiği gün -> ayağa kalkan birim ters durmaya
        //            devam eder, çünkü hiçbir AttackOutcome "kalktı" demez
        //            düşmüşe tekrar vurmak -> AttackAction Hit döner,
        //            ifade false olur ve düşmüş birim ayağa kalkmış görünür
        //            derleyici: hiçbir şey der  .  test: adaptör sınanamaz
        // KAZANIRDI: görsel bir DURUMU değil bir OLAYI gösterseydi — düşme
        //            animasyonu, kan sıçraması; onlar bir kez oynanır ve
        //            tazelenecek bir durumları yoktur.
        // TEK CUMLE: Sonuç enum'u "şimdi sormaya değer" der, "ne gösterileceğini"
        //            yalnızca durumun kendisi söyler.
        private void ApplyStateVisual(Unit unit, UnitState state)
        {
            if (!TryGetView(unit, out UnitView view))
            {
                return;
            }

            // ÇEVİRİ ARTIK YOK — ve yokluğu bir kazanç. Burada bir zamanlar
            // `combatant.State != UnitState.Alive` yazıyordu: üç değerli bir
            // bilgi, bu satırda iki değere iniyordu ve Downed ile Dead ekranda
            // aynı görünüyordu. UnitView'ın parametresi UnitState olunca
            // daraltma ortadan kalktı; adaptör durumu OLDUĞU GİBİ geçiriyor ve
            // "üç durum nasıl görünür" sorusunun tek sahibi UnitView oldu.
            view.SetState(state);
        }

        /// <summary>
        /// Görseli hücrenin merkezine taşır.
        /// </summary>
        private void MoveViewTo(Unit unit, int x, int y)
        {
            if (!TryGetView(unit, out UnitView view))
            {
                return;
            }

            view.transform.position = CellCentre(x, y);
        }

        /// <summary>
        /// Savaştan çıkarılmış bir birimin görselini sahneden siler.
        /// </summary>
        private void DespawnView(Unit unit)
        {
            // SEÇİM ÖNCE BIRAKILIR, ama ClearSelection ile DEĞİL: o metot
            // görsele SetSelected(false) der ve birazdan yok edilecek bir
            // nesneye çerçeve kapatmak anlamsız bir iştir. Daha önemlisi sıra:
            // tablodan silindikten sonra ClearSelection çağrılsaydı
            // SetSelectionVisual görseli bulamayıp LogError yazardı — var olmayan
            // bir hata için kırmızı satır.
            if (ReferenceEquals(unit, selectedUnit))
            {
                selectedUnit = null;
            }

            if (!TryGetView(unit, out UnitView view))
            {
                return;
            }

            // Önce tablodan çıkar, sonra sahneden sil. Ters sırada da çalışırdı
            // ama tabloda YOK EDİLMİŞ bir görsel referansı kalırdı ve Unity'nin
            // aşırı yüklenmiş eşitliği yüzünden o referans "null gibi ama null
            // değil" bir hâlde dolaşırdı.
            unitViews.Remove(unit);
            Destroy(view.gameObject);

            Debug.Log($"[Board] '{unit.Name}' was cleaned up and left the battle.", this);
        }

        /// <summary>
        /// Verilen birimi seçili yapar ve öncekinin seçimini kaldırır.
        /// </summary>
        private void SelectUnit(Unit unit)
        {
            // Önce eskiyi temizle: iki birim aynı anda seçili görünemez.
            // Bu satır olmasaydı her tıklama bir birimin daha çerçevesini açar
            // ve hiçbiri geri kapanmazdı.
            ClearSelection();

            selectedUnit = unit;
            SetSelectionVisual(unit, true);
        }

        /// <summary>
        /// Seçimi kaldırır. Seçim yoksa hiçbir şey yapmaz.
        /// </summary>
        private void ClearSelection()
        {
            if (selectedUnit == null)
            {
                return;
            }

            SetSelectionVisual(selectedUnit, false);
            selectedUnit = null;
        }

        /// <summary>
        /// Bir birimin görseline seçim durumunu iletir.
        /// </summary>
        private void SetSelectionVisual(Unit unit, bool isSelected)
        {
            if (!TryGetView(unit, out UnitView view))
            {
                return;
            }

            // Eski ApplyTint burada SpriteRenderer'ı bulup color'ını yazıyordu.
            // O yaklaşımın kusuru şuydu: renk ÇARPMA ile uygulandığı için
            // seçim, birimin kendi rengini/faction'ını bozuyordu. Artık birimin
            // kendi SpriteRenderer'ına HİÇ dokunulmuyor - color'ı Color.white
            // kalıyor - ve seçim ayrı bir çerçeve nesnesinde yaşıyor.
            //
            // Adaptör o çerçeveyi görmüyor bile; sadece niyeti söylüyor.
            view.SetSelected(isSelected);
        }

        /// <summary>
        /// Birimin görselini verir; yoksa gürültüyle şikâyet eder.
        /// </summary>
        // Dört çağıranın (seçim, hareket, durum, temizlik) aynı hata mesajını kopyalamaması
        // için var. Tabloda olmamak bir OYUN olgusu değil bir PROGRAMCI
        // hatasıdır: savaşa giren her birim SpawnUnit'ten geçmeli ve tabloya
        // kaydolmalıydı. Bu yüzden sessiz false değil, LogError + false.
        private bool TryGetView(Unit unit, out UnitView view)
        {
            if (unitViews.TryGetValue(unit, out view))
            {
                return true;
            }

            Debug.LogError($"[Board] No view registered for unit '{unit.Name}'.", this);
            return false;
        }

        /// <summary>
        /// Bir birimin can ve durum özetini log satırı için hazırlar.
        /// </summary>
        private string DescribeCondition(Unit unit)
        {
            if (!battle.TryGetCombatant(unit, out Combatant combatant))
            {
                return "(not in this battle)";
            }

            return $"health={combatant.CurrentHealth}, state={combatant.State}";
        }
    }
}
