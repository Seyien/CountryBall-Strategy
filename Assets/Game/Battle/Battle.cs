using System;
using System.Collections.Generic;
using GridStrategy.Combat;
using GridStrategy.Core;

namespace GridStrategy.Battle
{
    // ═══ ROL: BİLEŞİK (Aggregate) ════════════════════════════════════
    // kimlik : var — aynı ölçüdeki iki savaş aynı savaş değildir; kimin nerede
    //          durduğu ve kimin hangi savaşçı olduğu örneğe aittir
    // hafıza : var — AddUnit'ten sonra aynı TryGetCombatant çağrısı farklı
    //          cevap verir
    // Unity  : gerekmez — noEngineReferences: true; sahne, prefab, Vector2Int
    //          bilmez
    // karar  : parçalar ARASINDAKİ eşleşmeyi sahiplenir — ne tahtanın ne savaşın
    //          kurallarını yazar; ikisini AYNI ANDA tanıyan tek tip olmakla
    //          yetinir
    /// <summary>
    /// Tahtada duran <see cref="Unit"/> ile ona eşlenen savaş parçası —
    /// <see cref="Combatant"/> ya da <see cref="Structure"/> — arasındaki
    /// eşleşmenin tek sahibi.
    ///
    /// Var olma sebebi assembly düzeyinde okunur: <c>GridStrategy.Core</c> konumu
    /// bilir ama savaşı tanımaz, <c>GridStrategy.Combat</c> savaşı bilir ama
    /// tahtayı tanımaz — ve iki assembly birbirini GÖRMEZ. Bu yüzden
    /// <see cref="AttackAction"/>'ın dışarıdan istediği mesafeyi üretebilecek
    /// kimse yoktu. İkisini birden referans eden ilk assembly burasıdır; bu tip
    /// o birleşmenin DURUMUNU tutar, <see cref="BattleActions"/> ise AKIŞINI.
    ///
    /// SAHİPLENDİĞİ ÜÇ ŞEY: kim nerede (tahta + iki sözlük), sıra kimde
    /// (<see cref="Turn"/>) ve kimin durumu değişti
    /// (<see cref="UnitStateChanged"/>). Üçünün de ortak yanı aynı cümledir —
    /// hiçbiri bir KURAL değildir, üçü de bir DURUMdur.
    ///
    /// Neyi BİLMEZ: sıranın kimde olmasının ne anlama geldiğini
    /// (<see cref="TurnRules"/>'ın işi), bir birimin bu turda hareket edip
    /// etmediğini, mesafenin nasıl ölçüldüğünü, saldırının nasıl çözüldüğünü,
    /// sonucu kimin göstereceğini. Burada tek bir oyun kuralı yoktur — yalnızca
    /// "her kayıtlı parçanın tahtada tam olarak bir hücresi vardır" değişmezi.
    /// </summary>
    public sealed class Battle
    {
        // ═══ TAHTAYI BU TİP SAHİPLENİR, DIŞARIDAN ALMAZ ══════════════
        //
        // ── HARİTA: "iki sahip" tam olarak ne demek ─────────────────
        // UnitGrid bir SINIFtır (sealed class) — yani REFERANS tipi. Bir
        // referansı parametre olarak vermek nesneyi KOPYALAMAZ, yalnızca
        // ikinci bir ok açar; verenin oku silinmez.
        //
        //   REDDEDILEN — public Battle(UnitGrid board)
        //   ┌─BoardAdapter─┐                   ┌────Battle────┐
        //   │ board ───────┼────┐       ┌──────┼─ board       │
        //   └──────────────┘    ▼       ▼      └──────────────┘
        //                   ╔══════════════════╗
        //                   ║ UnitGrid nesnesi ║ ← TEK nesne, İKİ ok;
        //                   ╚══════════════════╝   ikisi de YAZABİLİR
        //
        //   SEÇİLEN — public Battle(int width, int height)
        //   ┌─BoardAdapter─┐                   ┌────Battle────┐
        //   │  (alan YOK)  │                   │ board ───────┼──┐
        //   └──────────────┘                   └──────────────┘  ▼
        //                                  ╔═══════════════════════╗
        //                                  ║ nesne KURUCUDA doğdu; ║
        //                                  ║ dışarıda ok HİÇ VAR   ║
        //                                  ║ OLMADI                ║
        //                                  ╚═══════════════════════╝
        //
        // Fark bir YASAK değil, bir İMKÂNSIZLIK: ikinci ok engellenmiyor,
        // hiç doğmuyor. Engellenen bir şey unutulabilir; doğmayan şey
        // unutulamaz.
        //
        // ── KIRILMA ZİNCİRİ (reddedilen imza seçilseydi) ────────────
        //   BoardAdapter kendi okundan board.PlaceUnit(u, x, y) çağırır
        //     -> tahtada bir Unit durur
        //     -> Battle.combatants sözlüğünde o Unit'in karşılığı YOKTUR
        //        (tek kayıt yolu Battle.AddUnit'ti ve o yol atlandı)
        //     -> BattleActions.Attack -> TryGetCombatant başarısız
        //     -> "bu savaşta değil" diye patlar
        //   derleyici: ayrışmayı GÖSTEREMEZ  .  test: yeşil kalır
        //
        // ── "İKİNCİSİNİ KOD SÖYLEMEZ" ───────────────────────────────
        // `public Battle(UnitGrid board)` imzası "tahtayı alıyorum" der;
        // "sen de tutmaya devam edersen bu savaş sessizce bozulur"
        // DEMEZ. Sözleşmenin taşıdığı risk imzada görünmez, yalnızca
        // yorumda yaşayabilir — ve yorum derlenmez. Reddedilme sebebi
        // budur.
        //
        // ── `readonly` BU KIRILMAYA KARŞI SIFIR KORUMA SAĞLAR ───────
        // Aşağıdaki readonly yalnızca ALANI kilitler, nesnenin İÇİNİ
        // değil:
        //     board = new UnitGrid(5, 5);   ✗ derleme hatası
        //     board.PlaceUnit(u, 2, 3);     ✓ tamamen serbest
        // Yani koruma readonly'den gelmiyor; "dışarıda hiç referans yok"
        // olgusundan geliyor.
        //
        // ── SAHİPLİĞİ AYAKTA TUTAN ÜÇ KATMAN ────────────────────────
        //   1. Kurucu `new UnitGrid(...)` yapar  -> doğumda dış ok yok
        //   2. `internal UnitGrid Board`         -> ok assembly dışına
        //      çıkmaz. BoardAdapter GridStrategy.Unity assembly'sinde
        //      yaşar ve bu üyeyi GÖREMEZ. public olsaydı 1. katman aynı
        //      gün boşa çıkardı.
        //   3. `private readonly`                -> en zayıf katman;
        //      yalnızca yeniden atamayı keser (yukarıya bak)
        //
        // Garantinin sınırı: BattleActions AYNI assembly'de olduğu için
        // Board'a erişebilir. Orada "tek yazar" sözünü tutan şey kod
        // değil, BattleActions'ın kendi disiplinidir. Sözleşme assembly
        // duvarında biter — bunu bilerek kabul ettik.
        //
        // ── KAZANIRDI ───────────────────────────────────────────────
        // Tahta savaştan ÖNCE ve BAŞKA bir yerde doluyorsa — kayıt
        // dosyasından yüklenen bir kuşatma, seviye editöründen gelen
        // hazır dizilim; o gün kurucu "kurulmuş tahtayı devral"ı
        // reddedemez. Fatura kaybolmaz, sahibi değişir: o senaryoda ya
        // kurucu tahtayı KOPYALAYARAK almalı ya da "verdikten sonra
        // okunu bırak" sözleşmede açıkça yazılı olmalıdır.
        //
        // KANIT: BoardAdapter'da bir `UnitGrid` alanı YOKTUR; bir zamanlar
        // vardı ve silindi (bkz. oradaki `private Battle battle;` alanının
        // üstündeki not). Yukarıdaki kırılma bu yüzden bir varsayım değil,
        // kapanmış bir borcun kaydıdır.
        //
        // TEK CUMLE: Bir bileşik, değişmezini koruduğu şeyi dışarıdan
        //            almaz — aldığı anda ikinci bir yazar doğar ve onu
        //            derleyici görmez.
        private readonly UnitGrid board;

        // ANAHTAR NEDEN Unit: BoardAdapter'ın unitViews alanının üstündeki
        // gerekçe burada aynen geçerli ve DEVRALINIYOR — konum yalnız tahtada
        // yaşasın diye. Orada görsel "neredeyim" bilmiyordu; burada savaşçı
        // bilmiyor. Unit bir sınıftır, varsayılan karşılaştırma referans
        // eşitliğidir ve aradığımız zaten tam olarak o nesnenin kendisi.
        //
        // REDDEDILEN - Battle.cs:91 yerine (savaş durumu hücreye yazılır):
        //     private readonly Combatant[,] combatantsByCell;
        // KIRILAN  : konum İKİ yerde yaşar ve ayrıştırmak tek satır alır.
        //            MoveAction.Execute tahtanın MoveUnit'ini doğrudan çağırır
        //            -> dizinin bundan haberi olmaz -> birim yeni hücresinde
        //            durur, canı eski hücrede kalır, saldıran hayaleti vurur
        //            derleyici: hiçbir şey der  .  test: kırmızı —
        //            Move_ThenAttack_UsesTheNewPosition
        // KARSILASTIRMA:
        //     List<Combatant>      anahtar YOK      -> her soruda taramak zorundasın
        //     Combatant[,]         anahtar = hücre  -> birim hareket edince anahtar bozulur
        //     Dictionary<Unit,..>  anahtar = birim  -> birimin kendisi kalır, bozulmaz
        // KAZANIRDI: savaş durumu birime değil HÜCREYE ait olsaydı — yanan
        //            zemin, üstünde duranı zehirleyen bataklık, tetiklenmiş
        //            tuzak; o gün durumun sahibi hücredir ve birim üstünden
        //            geçen geçici bir ziyaretçidir.
        // TEK CUMLE: Sözlükte anahtar nesnenin KENDİSİ, dizide nesne HAKKINDA bir
        //            bilgi; bilgi değişir, nesne değişmez.
        private readonly Dictionary<Unit, Combatant> combatants =
            new Dictionary<Unit, Combatant>();

        // İKİNCİ SÖZLÜK, İKİNCİ TAHTA DEĞİL. Yapılar birimlerle AYNI UnitGrid'e
        // giriyor; ayrışan tek şey "bu kimliğe hangi savaş parçası eşlendi"
        // sorusudur ve o soru zaten sözlüğün cevapladığı sorudur.
        //
        // REDDEDILEN - Battle.cs:113 yerine (yapılara kendi tahtası verilir):
        //     private readonly UnitGrid structureBoard;
        //     private readonly Dictionary<Structure, ...> structuresByCell;
        // KIRILAN  : "bu hücre dolu mu" sorusu İKİYE bölünür.
        //            dört yoldan biri ikinci tahtayı sormayı atlar -> aynı
        //            hücrede asker ile baraka durur -> mesafe iki ayrı koordinat
        //            uzayından ölçülür ve cevap sessizce yanlış çıkar
        //            derleyici: hiçbir şey der  .  test: yeşil kalır
        // KAZANIRDI: yapılar tahtanın ÜSTÜNDE değil ALTINDA yaşasaydı — zemin
        //            katmanı, üstünden yürünen köprü, birimin üzerinde durduğu
        //            platform; o gün "aynı hücrede iki şey" bir hata değil
        //            tasarımın kendisidir ve tek tahta onu ifade EDEMEZ.
        // TEK CUMLE: İki tahta iki gerçek demektir; aynı soruya iki yerden cevap
        //            veren bir sistem, ikisi ayrıştığı gün hangisinin doğru
        //            olduğunu söyleyemez.
        private readonly Dictionary<Unit, Structure> structures =
            new Dictionary<Unit, Structure>();

        // OLAY YÖNLENDİRİCİLERİ. Her savaşçı için BİR tane, AddUnit'te kurulur
        // ve RemoveUnit'te sökülür. Sözlük bir konfor değil zorunluluk:
        // Combatant.StateChanged imzası Action<UnitState, UnitState> ve GÖNDEREN
        // taşımıyor (gerekçe Combatant.cs'te), dolayısıyla kimliği ekleyen şey
        // her birime özel bir KAPANIŞ (closure) olmak zorunda. Kapanışlar
        // birbirine eşit değildir — `combatant.StateChanged -= (f, t) => ...`
        // yazarak abonelik çözülemez, çünkü ikinci lambda birinciyle aynı nesne
        // değildir. Yani sökmek için tam olarak ABONE OLUNAN örneği saklamak
        // gerekir ve saklanacağı yer burasıdır.
        //
        // Maliyet kare başına değil, birim başına: AddUnit'te bir delege ve bir
        // sözlük girdisi. Tick sıcak yolunda tek bir tahsis yok.
        private readonly Dictionary<Unit, Action<UnitState, UnitState>> stateForwarders =
            new Dictionary<Unit, Action<UnitState, UnitState>>();

        /// <summary>
        /// Belirtilen ölçüde boş bir savaş kurar. Ölçü doğrulaması burada
        /// KOPYALANMIYOR; <see cref="UnitGrid"/> kendi kurucusunda zaten
        /// yapıyor ve tek sahibi odur.
        /// </summary>
        public Battle(int width, int height)
        {
            board = new UnitGrid(width, height);

            // Sıra durumu savaşla birlikte DOĞAR ve savaşla birlikte ölür.
            // Sonradan atanabilir olsaydı, sırası henüz kurulmamış bir savaşta
            // Turn.Current okumak NullReferenceException verirdi ve "savaşı
            // kurmayı unuttum" hatası ilk tıklamada değil ilk sıra sorusunda
            // görünürdü.
            Turn = new TurnState();
        }

        // Tahta DIŞARIYA açılmıyor ama assembly içinde açık: BattleActions'ın
        // MoveAction.Execute'a verecek bir UnitGrid'e ihtiyacı var ve o tip bu
        // assembly'de yaşıyor. public olsaydı sahiplik sözü tek satırda
        // çözülürdü — herkes board.PlaceUnit çağırıp sözlüğü atlayabilirdi ve
        // yukarıdaki "iki sahip" kırılması geri gelirdi. internal, sözü
        // derleyiciye söyletir: dışarıdan tahtaya yol yok.
        internal UnitGrid Board => board;

        public int Width => board.Width;

        public int Height => board.Height;

        // AŞAĞIDAKİ İKİ ÜYE ÇAĞIRANI OLDUĞU İÇİN VAR. BoardAdapter kendi
        // `UnitGrid board` alanını tutmuyor (yukarıdaki "iki sahip" gerekçesi),
        // dolayısıyla tahtaya soracağı iki soruyu buraya soruyor: tıklanan
        // hücre tahtanın içinde mi (BoardAdapter.HandleClick) ve kaç hücre
        // kuruldu (BoardAdapter.BuildCellVisuals'ın kapanış log'u).
        //
        // İkisi de tahtaya DEVREDİYOR, kuralı kopyalamıyor: sınır kuralının tek
        // metni UnitGrid.IsInsideGrid'de, çarpım ise UnitGrid.CellCount'ta
        // yaşıyor. Buraya `x >= 0 && x < Width` yazılsaydı aynı kural iki yerde
        // yaşardı ve ikisi ayrışınca hangisinin doğru olduğunu derleyici
        // söyleyemezdi.

        /// <summary>Tahtadaki toplam hücre sayısı.</summary>
        public int CellCount => board.CellCount;

        /// <summary>
        /// Verilen hücre tahtanın içinde mi? Bu bir SORGUdur, bir oyun kuralı
        /// değil — cevabı tahtanın kendi ölçüsü verir.
        /// </summary>
        public bool IsInsideGrid(int x, int y)
        {
            return board.IsInsideGrid(x, y);
        }

        // AD BORCU, AÇIKÇA YAZILIYOR: Unit'in özetinin ilk cümlesi "tahtada yer
        // kaplayan, kimliği olan şey" diyor, dolayısıyla UnitCount adı
        // olduğundan geniş okunuyor — sayacağı şey SAVAŞÇI, tahtadaki her şey
        // değil. S-11'in aynı sınavı: sayı doğru, ad geniş.
        //
        // REDDEDILEN - Battle.cs:211 yerine (ad korunur, ANLAM genişletilir):
        //     public int UnitCount => combatants.Count + structures.Count;
        // KIRILAN  : imza değişmez; kırılan şey cevabın kendisidir ve sessizdir.
        //            tahtaya bir baraka konur -> "kaç askerim kaldı" bir fazla
        //            sayar -> yenilgi koşulu bu sayıya bağlandığı gün oyuncu,
        //            sahada tek askeri yokken deposu ayakta diye kaybetmez
        //            derleyici: hiçbir şey der  .  test: kırmızı —
        //            AddUnit_OnACellHeldByAStructure_ThrowsAndKeepsTheStandingStructure
        // KAZANIRDI: yenilgi koşulu gerçekten "tahtada hiçbir şeyin kalmasın"
        //            olsaydı ve savaşçı ile yapı ayrımı hiçbir sayımda
        //            gerekmeseydi — o gün tek bir toplam, iki sayacı toplayan
        //            her çağıranı bir satırdan kurtarırdı.
        // TEK CUMLE: Bir adın anlamını genişletmek imzayı değil yalnızca cevabı
        //            değiştirir — derleyicinin göremediği tek değişiklik budur.
        //
        // EŞİK: dışarıdan "tahtada kaç şey var" diye soran ilk çağıran doğduğu
        // gün üçüncü bir üye (BoardPieceCount) eklenir; iki sayacın toplamını
        // çağıranda kurmak, aynı toplamı her çağıranda yeniden yazmak olur.
        /// <summary>Bu savaşta kayıtlı savaşçı sayısı — yapılar dahil DEĞİL.</summary>
        public int UnitCount => combatants.Count;

        /// <summary>Bu savaşta kayıtlı yapı sayısı.</summary>
        public int StructureCount => structures.Count;

        // SIRA DURUMUNUN SAHİBİ BU TİP. Sıra bir DURUMdur ("şu an kimde") ve bu
        // dosyanın rol başlığındaki söz tam olarak durumu sahiplenmektir; sıranın
        // NE ANLAMA GELDİĞİ ise bir kuraldır ve TurnRules'ta yaşıyor. İkisi
        // birbirine karışmıyor: burada tek bir izin/yasak satırı yok.
        //
        // REDDEDILEN - Battle.cs:262 yerine (sıra durumu Unity katmanına,
        //              BoardAdapter'a taşınır ve savaş onu hiç bilmez):
        //     // BoardAdapter.cs içinde:
        //     private readonly TurnState turn = new TurnState();
        //     private void OnEndTurnButton() { turn.EndTurn(); }
        // KIRILAN  : sıra kuralı EditMode'da sınanamaz hâle gelir.
        //            akış sırayı SORMAK zorunda, elindeki tek şey Battle -> sırayı
        //            parametre olarak ister -> yanlış TurnState geçen kod DERLENİR,
        //            ya da hiç sormaz ve sıra sistemi yalnızca ekranda var olur
        //            derleyici: hiçbir şey der  .  test: sahne kurmayı, yani
        //            PlayMode'a inmeyi zorunlu kılar
        // KARSILASTIRMA:
        //     BoardAdapter     sahip = ekran    -> savaş Unity'siz koşamaz, kural sınanamaz
        //     static alan      sahip = süreç    -> iki savaş tek sırayı paylaşır
        //     Battle örneği    sahip = savaş    -> her savaşın kendi sırası olur
        // KAZANIRDI: sıra gerçekten bir SUNUM kavramı olsaydı — savaşın kendisi
        //            eşzamanlı çözülüp arayüz onu sırayla GÖSTERSEYDİ (auto-
        //            battler'ın tam olarak yaptığı şey); o gün TurnState bir
        //            oynatma kafası olurdu ve savaşta karşılığı olmazdı.
        // TEK CUMLE: Bir durumun sahibi onu DEĞİŞTİREN değil, ömrünü PAYLAŞAN
        //            şeydir — sıra ekranla değil savaşla doğar ve savaşla ölür.
        //
        // REDDEDILEN - Battle.cs:262 yerine (durum hiç doğmaz, akış sahibi kendi
        //              static örneğini tutar):
        //     // BattleActions.cs içinde:
        //     private static readonly TurnState Turn = new TurnState();
        // KIRILAN  : yukarıdaki tablonun "static" satırının ölçülebilir yüzü.
        //            NUnit bütün testleri aynı süreçte koşar -> durum test
        //            metotları ARASINDA sızar -> her testin başındaki yeni savaş
        //            temiz sayfa vermez ve testler KOŞMA SIRASINA göre geçer
        //            derleyici: hiçbir şey der  .  test: yeşil, ama yalanı yeşil
        // KAZANIRDI: süreç ömrü boyunca ikinci bir savaş asla kurulmayacaksa —
        //            tek oyunculu, tek sahneli, önizlemesiz bir oyun; o gün örnek
        //            başına durum, iki değer almayacak bir alanın töreni olurdu.
        // TEK CUMLE: static durum, testin komşusunu testin girdisi yapar.
        /// <summary>
        /// Bu savaşın sıra durumu: sıra hangi tarafta ve kaçıncı turdayız.
        ///
        /// Savaşla birlikte kurulur ve DEĞİŞTİRİLEMEZ — savaşın ortasında ikinci
        /// bir <see cref="TurnState"/> atanabilseydi tur numarası sıfırlanır,
        /// sıra rastgele bir tarafa kayar ve bunun hiçbir izi kalmazdı.
        /// Nesnenin KENDİSİ değişkendir (<see cref="TurnState.EndTurn"/>);
        /// değiştirilemez olan, hangi nesne olduğudur.
        /// </summary>
        public TurnState Turn { get; }

        // ZİNCİRİN SON HALKASI — kimliği ekleyen yer burası, ve başka bir yer
        // olamaz: Combatant kendi Unit'ini BİLMEZ (gerekçe Combatant.cs'te),
        // çünkü kimlik parçalarda değil bu sözlükte yaşıyor.
        //
        //     UnitLifecycle.StateChanged  Action<UnitState>              hangi duruma
        //     Combatant.StateChanged      Action<UnitState, UnitState>   nereden nereye
        //     Battle.UnitStateChanged     Action<Unit, UnitState, ...>   KİM, nereden nereye
        //
        // TOPLU SÜPÜRME (RemoveReadyForCleanup) BU OLAYIN YERİNE GEÇMEZ ve
        // KALDIRILMIYOR: ikisi farklı sorulara cevap veriyor. Bu olay "durum
        // değişti" der, süpürme "artık silinebilir" der. Üçüncü Tick
        // IsReadyForCleanup'ı açar ama hiçbir durumu değiştirmez — o an süpürme
        // bir şey bulur, bu olay tetiklenmez.
        //
        // REDDEDILEN - Battle.cs:309 yerine (abonelik kurulur ama RemoveUnit'te
        //              BIRAKILMAZ; kayıt silindiği için nasıl olsa duyulmaz
        //              varsayılır):
        //     combatants.Remove(unit);   // stateForwarders'a hiç dokunulmaz
        // KIRILAN  : varsayım yalnızca Battle.Tick yolu için doğru.
        //            çıkarılan savaşçıya elde kalan referanstan Tick gelir ->
        //            savaşta OLMAYAN bir birim için kimlikli olay yayılır ->
        //            delege bu savaşı tuttuğu için o birim çöp de olamaz
        //            derleyici: hiçbir şey der  .  test: yeşil kalır
        // KAZANIRDI: Combatant savaştan çıkarken KENDİSİ de atılıyor olsaydı — bu
        //            tip onu kuran taraf olsaydı ve dışarıya hiç vermeseydi; o gün
        //            aboneliği bırakmak, birlikte çöpe giden iki nesne arasındaki
        //            bağı elle koparma töreni olurdu.
        // TEK CUMLE: Abonelik bir alan değil bir ÖMÜR sözleşmesidir; bırakmayı
        //            unutan taraf hem yanlış cevap yayar hem nesneyi bellekte tutar.
        /// <summary>
        /// Bu savaştaki bir savaşçının durumu her DEĞİŞTİĞİNDE tetiklenir;
        /// KİMİN, nereden nereye geçtiğini taşır.
        ///
        /// Var olma sebebi tek bir boşluk: <see cref="Tick"/> ile olan
        /// Downed → Dead geçişini SORAN yoktur. Tick'i çeviren taraf oyun
        /// döngüsüdür ve o geçişle ilgilenmez; ilgilenen (görselin durumu, ses,
        /// skor) BAŞKA yerdedir. Saldırı sonucu ise hâlâ dönüş değeriyle
        /// geliyor ve öyle kalmalı — orada soran zaten oradadır.
        ///
        /// Yapılar bu olaya KATILMAZ ve bu bir eksiklik değil:
        /// <see cref="StructureLifecycle"/> bilerek olaysızdır, çünkü onun tek
        /// geçişi (ayakta → yıkık) her zaman bir hasar çağrısından doğar ve o
        /// çağrıyı yapan taraf cevabı zaten dönüş değeriyle alır. Gerekçenin
        /// tamamı o dosyada yazılı; burada tekrar edilmiyor.
        /// </summary>
        public event Action<Unit, UnitState, UnitState> UnitStateChanged;

        /// <summary>
        /// Bir birimi savaşa katar: tahtaya yerleştirir ve savaş durumuyla
        /// eşler. Üçü — birim, savaşçı, konum — TEK çağrıda gelir; çünkü
        /// ayrılırlarsa aralarında yarım kalmış bir hâl doğar.
        /// </summary>
        public void AddUnit(Unit unit, Combatant combatant, int x, int y)
        {
            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

            if (combatant == null)
            {
                throw new ArgumentNullException(nameof(combatant));
            }

            ThrowIfCannotJoin(unit, x, y);

            // İKİNCİ KELEPÇE, ve yönü tersi. Üstteki kural "bu KİMLİK zaten
            // içeride mi" diye sorar; bu "bu PARÇA zaten başka bir kimliğe bağlı
            // mı" diye sorar. İkisi ayrı sorular ve yalnız biri yazılırsa açık
            // kalan yön sessizdir: aynı Combatant iki Unit altında kayıtlıysa
            // StateChanged için İKİ forwarder taşır, tek geçiş iki kimlikle
            // yayılır ve dinleyici aynı ölümü iki kez görür. Hiçbir derleme
            // hatası çıkmaz.
            //
            // Maliyet dürüstçe: ContainsValue sözlüğü baştan sona tarar (O(n)).
            // 3x5 tahtada bu ölçülemez; EŞİK, tahtanın onlarca birime çıktığı
            // ve ekleme sıcak yola girdiği gündür.
            //
            // Alternatif: eşleşmeyi ters yönde de tutan ikinci bir sözlük
            // (Dictionary<Combatant, Unit>). Seçilmedi: arama sabit zamana iner
            // ama eşleşme ÜÇÜNCÜ bir yerde yaşamaya başlar ve üçünün güncel
            // kalması RemoveUnit'in dikkatine bağlanır; tetiği üstteki EŞİK.
            if (combatants.ContainsValue(combatant))
            {
                throw new ArgumentException(
                    "This combatant is already registered under another unit.", nameof(combatant));
            }

            // SIRA BİR KARARDIR: bütün ret sebepleri HİÇBİR ŞEY yazılmadan
            // sorulur, sonra iki yazma arka arkaya yapılır. PlaceUnit tahta dışı
            // koordinatta kendi kontrolünü yazmadan önce yapar, dolayısıyla bu
            // noktadan sonra patlayabilecek tek şey odur ve o da tahtaya
            // dokunmadan patlar.
            //
            // REDDEDILEN - Battle.cs:373 yerine (ön kontrol yok, çakışmayı
            //              Dictionary.Add'in kendi hatası bildirir):
            //     board.PlaceUnit(x, y, unit);
            //     combatants.Add(unit, combatant);
            // KIRILAN  : YARIM KALMA — UnitGrid.MoveUnit'in var olma sebebi.
            //            aynı Unit başka bir hücreye ikinci kez eklenir ->
            //            PlaceUnit ÇOKTAN yazmıştır -> Dictionary.Add patlar ve
            //            birim tahtada İKİ hücrede birden durur
            //            derleyici: hiçbir şey der  .  test: kırmızı —
            //            AddUnit_SameUnitTwice_ThrowsAndLeavesTheFirstCellUntouched
            // KAZANIRDI: Battle tahtayı SAHİPLENMESEYDİ — yerleştirme dışarıda
            //            yapılsaydı bu metot yalnız sözlüğe yazardı ve
            //            Dictionary.Add'in kendi hatası fazlasıyla yeterdi.
            // TEK CUMLE: İki yazma varsa bütün ret sebepleri ilk yazmadan ÖNCE
            //            sorulur; yoksa hata mesajı doğru, tahta yanlış kalır.
            board.PlaceUnit(x, y, unit);
            combatants.Add(unit, combatant);

            // ABONELİK EN SONDA — reddedilen her ekleme geriye TEK BİR abone
            // bırakmamalı. Yukarıdaki PlaceUnit tahta dışı koordinatta patlar ve
            // o noktada ne sözlükte ne olay listesinde bir iz kalır.
            //
            // Kapanış `unit`'i yakalıyor: kimliği ekleyen şey tam olarak bu.
            // Aynı kapanışı RemoveUnit'in sökebilmesi için saklamak zorundayız
            // (gerekçe stateForwarders'ın üstünde).
            Action<UnitState, UnitState> forwarder =
                (previous, next) => UnitStateChanged?.Invoke(unit, previous, next);
            combatant.StateChanged += forwarder;
            stateForwarders.Add(unit, forwarder);
        }

        /// <summary>
        /// Bir yapıyı savaşa katar: BİRİMLERLE AYNI tahtaya yerleştirir ve yapı
        /// durumuyla eşler. Şekli <see cref="AddUnit"/>'in birebir ikizidir ve
        /// bu tesadüf değil — ikisi de aynı değişmezi koruyor: her kayıtlı
        /// parçanın tahtada tam olarak bir hücresi vardır.
        ///
        /// Bir <see cref="Unit"/> aynı anda hem savaşçı hem yapı OLAMAZ; ikinci
        /// kayıt reddedilir. İzin verilseydi tek hücrede iki savaş parçası
        /// yaşardı, <see cref="Tick"/> ikisini birden işletirdi ve
        /// <see cref="RemoveReadyForCleanup"/> aynı kimliği listeye iki kez
        /// yazardı — çağıran aynı görseli iki kez silmeye çalışırdı.
        /// </summary>
        // ABONELİK YOK, ve bu bir unutma değil: StructureLifecycle bilerek
        // olaysızdır (gerekçe o dosyada). Buraya bir yönlendirici yazmak, olmayan
        // bir olaya abone olmaya çalışmak olurdu — kod derlenmezdi bile.
        public void AddStructure(Unit unit, Structure structure, int x, int y)
        {
            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

            if (structure == null)
            {
                throw new ArgumentNullException(nameof(structure));
            }

            ThrowIfCannotJoin(unit, x, y);

            // AddUnit'teki parça kelepçesinin ikizi; gerekçe orada yazılı ve
            // burada TEKRAR EDİLMİYOR. Ortak kapıya konamamasının sebebi tip:
            // ThrowIfCannotJoin yalnız Unit görür, parça iki farklı tiptir ve
            // ikisini birden görecek bir imza generic olmak zorunda kalırdı.
            if (structures.ContainsValue(structure))
            {
                throw new ArgumentException(
                    "This structure is already registered under another unit.", nameof(structure));
            }

            // Yazma sırası AddUnit'inkiyle AYNI ve aynı sebeple: önce bütün ret
            // sebepleri, sonra iki yazma arka arkaya. Kopyalanan şey bir kural
            // değil bir SIRA; kuralların metni ThrowIfCannotJoin'de tek kez
            // duruyor.
            board.PlaceUnit(x, y, unit);
            structures.Add(unit, structure);
        }

        // İKİ EKLEME YOLUNUN ORTAK KAPISI. Ret sebepleri burada TEK kez yazılı;
        // AddUnit ile AddStructure'ın ikisinde de kopyalansaydı, "aynı kimlik
        // ikinci kez giremez" kuralı iki yerde yaşardı ve yalnız birine yapı
        // sözlüğünü sormak (ilk yazılışta en olası hata) hiçbir derleme hatası
        // vermeden aynı Unit'i hem savaşçı hem baraka yapardı.
        //
        // DOLU HÜCRE BURADA BİR ÇAĞIRAN HATASIDIR ve bu, UnitGrid'in
        // sessizliğiyle çelişmez. UnitGrid hücre içeriği konusunda susar çünkü
        // onun için doluluk bir olgudur; burada ise değişmezin kendisidir:
        // PlaceUnit üstüne yazsaydı eski parça tahtadan silinir ama sözlükte
        // kayıtlı kalırdı — hücresi olmayan bir savaşçı. "Dolu hücreye
        // taşınamaz" kuralıyla da karışmaz: o kural MoveAction'ın ve HAREKET
        // hakkında; bu satır YERLEŞTİRME hakkında ve sebebi oyun dengesi değil,
        // bu tipin bütünlüğü.
        //
        // Tahta dışı koordinat BURADA sorulmuyor — sorusunun sahibi
        // UnitGrid.PlaceUnit ve o, hiçbir hücreye dokunmadan önce patlıyor.
        // Buraya bir IsInsideGrid kontrolü eklemek aynı kuralı ikinci bir yerde
        // yazmak olurdu; CellCount ve IsInsideGrid'in devretme gerekçesi
        // yukarıda aynen geçerli.
        private void ThrowIfCannotJoin(Unit unit, int x, int y)
        {
            if (combatants.ContainsKey(unit) || structures.ContainsKey(unit))
            {
                throw new ArgumentException("The unit is already in this battle.", nameof(unit));
            }

            if (board.TryGetUnit(x, y, out Unit _))
            {
                throw new ArgumentException(
                    "The target cell is already occupied by another unit.", nameof(x));
            }
        }

        /// <summary>
        /// Bir birimi ya da yapıyı savaştan çıkarır: hücresini boşaltır ve
        /// kaydını siler. Ölüm, yıkım ve temizlik yolu burasıdır.
        ///
        /// TEK metot, iki sözlük — çünkü çağıranın elinde yalnızca bir
        /// <see cref="Unit"/> var ve onun hangi sözlükte olduğunu bilmek bu
        /// tipin işi. İkiye bölünseydi (RemoveUnit / RemoveStructure) her
        /// çağıran önce "bu bir yapı mı" diye sormak zorunda kalırdı ve o soru,
        /// cevabı zaten burada duran bir bilginin çağıranlara dağıtılması olurdu.
        /// </summary>
        /// <returns>Birim bu savaşta kayıtlıysa true.</returns>
        // Kimliğe göre silmek tahtayı taratır — ve bu maliyet
        // UnitGrid.RemoveUnit'in üstündeki REDDEDILEN bloğunun KAZANIRDI
        // satırında ÖNCEDEN adı konmuş bir durumdur: "silmeyi tetikleyen yer
        // birimi TANIYIP hücresini BİLMİYORSA". Temizliği tetikleyen şey
        // Combatant.IsReadyForCleanup olacak ve o bayrak hücreden hiç haberdar
        // değil. Yani tarama burada bir ihmal değil, o notun gerçekleşmiş hâli.
        //
        // Dönüş bool: "bu savaşta yoktu" bir ÇAĞIRAN hatası değil. Temizlik aynı
        // birim için iki kez çalışabilir (bir tur döngüsü, bir de ölüm olayı) ve
        // ikincisinin sessizce hiçbir şey yapması doğru davranıştır.
        public bool RemoveUnit(Unit unit)
        {
            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

            bool wasCombatant = combatants.ContainsKey(unit);
            bool wasStructure = structures.ContainsKey(unit);

            if (!wasCombatant && !wasStructure)
            {
                return false;
            }

            // Koordinat tahtanın kendisinden geliyor, dolayısıyla RemoveUnit'in
            // sınır kontrolü burada asla patlamaz — yarım kalma riski yok.
            if (TryGetPosition(unit, out int x, out int y))
            {
                board.RemoveUnit(x, y);
            }

            if (wasCombatant)
            {
                // ABONELİK BURADA BIRAKILIYOR. Bırakmama, sessiz sızıntının ders
                // kitabı örneği ve gerekçesinin tamamı UnitStateChanged'in
                // üstünde yazılı. Sıra önemli değil (sözlükten silmekle abonelik
                // arasında bir bağ yok) ama İKİSİ de yapılmalı: yalnız sözlükten
                // silmek, çağıranın elinde kalan Combatant üzerinden savaşta
                // olmayan bir birim için kimlikli olay yayılmasına izin verir.
                if (stateForwarders.TryGetValue(unit, out Action<UnitState, UnitState> forwarder))
                {
                    combatants[unit].StateChanged -= forwarder;
                    stateForwarders.Remove(unit);
                }

                combatants.Remove(unit);
            }
            else
            {
                structures.Remove(unit);
            }

            return true;
        }

        /// <summary>
        /// Zamanı bu savaştaki HER savaşçıya iletir. Saniye dışarıdan gelir ve
        /// burada da okunmaz — <see cref="UnitLifecycle"/>'ın "zamanı kendi
        /// okumaz" sözü bu tipte de geçerli; <c>Time.deltaTime</c>'ı çeviren
        /// yer Unity katmanıdır.
        ///
        /// Burada tek bir kural yok: doğrulama <see cref="UnitLifecycle.Tick"/>'in,
        /// geçişler <see cref="Combatant"/>'ın. Buranın sahiplendiği tek şey
        /// "kimler var" bilgisi — ve onu bilen tek tip bu.
        /// </summary>
        // NEDEN BURADA: savaşçı kümesinin sahibi bu tip. Zamanı ilerletmek için
        // o kümeyi dolaşmak gerekir ve kümeyi dolaşabilen başka kimse yok.
        //
        // REDDEDILEN - Battle.cs:564 yerine (bu metot doğmaz; küme dışarı
        //              açılır ve döngü çağıranda yaşar):
        //     public IEnumerable<Unit> Units => combatants.Keys;
        // KIRILAN  : kare başına ÇÖP, ve döngü her çağıranda yeniden doğar.
        //            KeyCollection numaralandırıcısı IEnumerable ardında KUTULANIR
        //            -> her Update bir tahsis yapar -> döngü MonoBehaviour içine
        //            düşerse EditMode'da hiç sınanamaz
        //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
        // KAZANIRDI: zaman savaşçıdan savaşçıya FARKLI aksaydı — yavaşlatma
        //            alanı, hızlandırma büyüsü, donmuş birim; o gün tek bir
        //            deltaSeconds ile yapılan toplu ilerletme yanlış cevaptır
        //            ve çağıranın kendi çarpanıyla dolaşması gerekir.
        // TEK CUMLE: Kümeyi dışarı açmak döngüyü de dışarı taşır; taşınan döngü
        //            hem çöp üretir hem her çağıranda yeniden yazılır.
        public void Tick(float deltaSeconds)
        {
            // Sözlük üzerinde DOĞRUDAN foreach: Dictionary<,>.Enumerator bir
            // struct'tır ve burada bir arayüz ardında saklanmadığı için
            // kutulanmaz. Aynı döngü `IEnumerable` üzerinden dönseydi kare
            // başına bir tahsis üretirdi.
            foreach (KeyValuePair<Unit, Combatant> pair in combatants)
            {
                pair.Value.Tick(deltaSeconds);
            }

            // İKİNCİ DÖNGÜ, TEK ÇAĞRI. Yapıların saatini çevirmek ayrı bir metoda
            // (TickStructures) konsaydı çağıran ikisini de çağırmakla yükümlü
            // olurdu ve birini unutan gün enkaz sonsuza dek tahtada kalırdı —
            // hiçbir test kırmızı olmadan, çünkü "enkaz neden hâlâ duruyor" diye
            // kimse hata açmaz (aynı sessizlik StructureLifecycle.cs'te de adı
            // konmuş durumda).
            foreach (KeyValuePair<Unit, Structure> pair in structures)
            {
                pair.Value.Tick(deltaSeconds);
            }
        }

        /// <summary>
        /// Ceset süresi dolmuş savaşçıları VE enkaz süresi dolmuş yapıları
        /// savaştan çıkarır — hücrelerini boşaltır, kayıtlarını siler — ve
        /// hangilerinin çıkarıldığını verilen listeye yazar.
        ///
        /// <b><see cref="UnitStateChanged"/> BU METODUN YERİNE GEÇMEZ</b> ve
        /// geçmemeli. İkisi farklı sorulara cevap veriyor: olay "durum değişti"
        /// der, bu metot "artık silinebilir" der. Somut ayrım tek bir Tick'te
        /// görünür — ceset sayacının dolduğu Tick hiçbir DURUM değiştirmez,
        /// yalnızca bir bayrak açar; o an bu metot bir şey bulur ve olay hiç
        /// tetiklenmez. Yapılar tarafında ayrım daha da keskin:
        /// <see cref="Structure"/>'ın hiçbir olayı yok, yani enkazı bulan tek
        /// yol burasıdır.
        /// </summary>
        /// <param name="removed">
        /// Çıkarılan birimlerin yazılacağı tampon. Metot onu ÖNCE TEMİZLER:
        /// bu bir ÇIKIŞ tamponudur, üstüne eklenen bir liste değil. Çağıran
        /// aynı listeyi her karede yeniden kullanır ve kare başına tahsis
        /// olmaz. <c>ICollection&lt;Unit&gt;</c> değil <c>List&lt;Unit&gt;</c>,
        /// çünkü ikinci geçişte indeksle dolaşmak gerekiyor — arayüzde indeks
        /// yok, numaralandırıcı ise yine kutulanırdı.
        /// </param>
        /// <returns>Çıkarılan parça sayısı — savaşçılar ve yapılar birlikte.</returns>
        // İKİ GEÇİŞ ZORUNLU: sözlük üzerinde dönerken silmek
        // InvalidOperationException fırlatır. Önce adaylar toplanır, sonra
        // silinir — tek geçişte yazmayı denemek çalışır GÖRÜNÜR (tek ceset
        // varken) ve ikinci ceset doğduğu gün patlar.
        //
        // REDDEDILEN - Battle.cs:637 yerine (bu metot hiç doğmaz; Unity
        //              katmanı her karede kendi görsel tablosunu dolaşıp
        //              savaşçıları YOKLAR):
        //     foreach (Unit unit in unitViews.Keys)
        //     {
        //         if (battle.TryGetCombatant(unit, out Combatant c)
        //             && c.IsReadyForCleanup)
        //         {
        //             battle.RemoveUnit(unit);
        //         }
        //     }
        // KIRILAN  : süpürme yalnızca GÖRSELİ olan birimleri görür.
        //            savaşa görselsiz bir savaşçı eklenir (takviye, yapay zekâ)
        //            -> asla temizlenmez -> hücresini sonsuza dek tutar ve tahta
        //            sessizce dolar; gövde de bir MonoBehaviour'a kopyalanır
        //            derleyici: hiçbir şey der  .  test: RemoveReadyForCleanup_* düz C# koşamaz
        // KAZANIRDI: temizliğin ölçütü savaş durumu değil GÖRSEL bir şey
        //            olsaydı — ölüm animasyonu bitmeden silinmesin, ekran
        //            dışındaki ceset hemen gitsin; o gün ölçüt Unity tarafında
        //            yaşar, bu tip onu bilemez ve yoklama doğru yerdedir.
        // TEK CUMLE: "Kim savaşta" sorusunun cevabı bir görsel tabloya kayarsa,
        //            görseli olmayan her şey savaşta yokmuş gibi davranır.
        public int RemoveReadyForCleanup(List<Unit> removed)
        {
            if (removed == null)
            {
                throw new ArgumentNullException(nameof(removed));
            }

            removed.Clear();

            foreach (KeyValuePair<Unit, Combatant> pair in combatants)
            {
                if (pair.Value.IsReadyForCleanup)
                {
                    removed.Add(pair.Key);
                }
            }

            // Yapılar AYNI tampona yazılıyor ve çağıran ikisini ayırt etmiyor —
            // ayırt etmesine gerek de yok, çünkü elindeki iş ikisinde de aynı:
            // o kimliğin görselini sahneden kaldırmak. Ayrı bir liste, çağırana
            // iki döngü yazdırırdı ve ikinci döngüyü unutan gün enkaz ekranda
            // kalırdı.
            //
            // Aynı kimliğin listeye İKİ kez girmesi imkânsız: ThrowIfCannotJoin
            // bir Unit'in aynı anda iki sözlükte olmasını engelliyor. O kelepçe
            // kalkarsa burası sessizce bozulur — çağıran aynı görseli iki kez
            // silmeye çalışır ve ikincisi hata basar.
            foreach (KeyValuePair<Unit, Structure> pair in structures)
            {
                if (pair.Value.IsReadyForCleanup)
                {
                    removed.Add(pair.Key);
                }
            }

            // RemoveUnit'in dönüşü BİLEREK yok sayılıyor: aday listesi bir
            // satır önce bu sözlükten geldi, dolayısıyla false dönmesi mümkün
            // değil. Kontrol etmek imkânsız bir dal açardı.
            for (int i = 0; i < removed.Count; i++)
            {
                RemoveUnit(removed[i]);
            }

            return removed.Count;
        }

        /// <summary>
        /// Hücrede duran birimi verir. Boş hücre ve tahta dışı koordinat
        /// sessizce false döner — <see cref="UnitGrid.TryGetUnit"/> ile aynı
        /// felsefe, çünkü bu metot onun önündeki ince bir kapıdan ibaret.
        /// </summary>
        public bool TryGetUnit(int x, int y, out Unit unit)
        {
            return board.TryGetUnit(x, y, out unit);
        }

        /// <summary>
        /// Birimin savaş durumunu verir. Kayıtlı değilse false.
        /// </summary>
        public bool TryGetCombatant(Unit unit, out Combatant combatant)
        {
            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

            return combatants.TryGetValue(unit, out combatant);
        }

        /// <summary>
        /// Birimin yapı durumunu verir. Kayıtlı değilse — ya da o kimlik bir
        /// savaşçıysa — false.
        ///
        /// <see cref="TryGetCombatant"/> ile birlikte okunduğunda bu ikili,
        /// "bu kimlik ne" sorusunun tek cevabıdır: iki çağrıdan tam olarak biri
        /// true döner. Bir <c>IsStructure(Unit)</c> kısayolu eklenmedi — cevabı
        /// zaten alan bir çağıran için ikinci bir arama, cevabı almayan bir
        /// çağıran için de dallanmayı ATLAMANIN yoludur; <c>out</c> şekli
        /// çağıranı dallanmaya zorluyor ve gerekçesi UnitGrid.cs'te yazılı.
        /// </summary>
        public bool TryGetStructure(Unit unit, out Structure structure)
        {
            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

            return structures.TryGetValue(unit, out structure);
        }

        /// <summary>
        /// Birimin tahtadaki hücresini verir. Konumun TEK kaynağı tahtadır;
        /// bu metot her çağrıda oraya sorar.
        /// </summary>
        // KONUM HİÇ ÖNBELLEĞE ALINMIYOR — her çağrıda tahta taranıyor.
        //
        // REDDEDILEN - Battle.cs:747 yerine (konum ikinci bir sözlükte tutulur
        //              ve AddUnit ile RemoveUnit onu günceller):
        //     private readonly Dictionary<Unit, (int x, int y)> positions;
        // KIRILAN  : konum iki sahipli olur — üstteki sözlük gerekçesinin aynısı,
        //            bu kez önbellek kılığında.
        //            MoveAction tahtayı DOĞRUDAN değiştirir -> sözlük duymaz ->
        //            birim yaklaşmış olduğu hâlde saldırı "menzil dışı" der
        //            derleyici: hiçbir şey der  .  test: Move_ThenAttack_UsesTheNewPosition kırmızı
        // KAZANIRDI: tahta ÖLÇÜLEBİLİR biçimde büyüseydi — 200x200'lük bir
        //            harita, kare başına yüzlerce mesafe sorgusu ve profiler'da
        //            görünen bir tarama maliyeti; o gün önbellek gerekir ama
        //            bedeli tahtaya yazan her yolun Battle'dan geçmesidir.
        // TEK CUMLE: Önbellek bir hız kararı değil, ikinci bir doğruluk kaynağı
        //            yaratma kararıdır.
        public bool TryGetPosition(Unit unit, out int x, out int y)
        {
            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

            for (int cellX = 0; cellX < board.Width; cellX++)
            {
                for (int cellY = 0; cellY < board.Height; cellY++)
                {
                    if (board.TryGetUnit(cellX, cellY, out Unit standing)
                        && ReferenceEquals(standing, unit))
                    {
                        x = cellX;
                        y = cellY;
                        return true;
                    }
                }
            }

            // Bulunamayınca (0,0) DEĞİL (-1,-1): sıfır geçerli bir hücredir ve
            // dönüşü yok sayan bir çağıran onu sessizce köşe sanardı. -1 ise
            // tahtaya verildiği anda UnitGrid tarafından gürültüyle reddedilir.
            x = -1;
            y = -1;
            return false;
        }
    }
}
