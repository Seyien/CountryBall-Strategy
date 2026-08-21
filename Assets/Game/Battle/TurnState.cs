using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GridStrategy.Combat;

namespace GridStrategy.Battle
{
    // ═══ ROL: VARLIK (Entity) ════════════════════════════════════════
    // kimlik : var — her savaşın kendi tur durumu; iki savaş aynı anda iki
    //          farklı tarafın sırasında olabilir
    // hafıza : var — aynı EndTurn çağrısı farklı sonuç verir: bir devir turu
    //          ilerletir, bir sonraki ilerletmez
    // Unity  : gerekmez — noEngineReferences: true; kare, zaman, sahne bilmez
    // karar  : tutar ve bildirir; kimin ne yapabileceğine TurnRules karar verir
    /// <summary>
    /// Bir savaşın sıra durumu: sıra hangi tarafta ve kaçıncı turdayız.
    ///
    /// Bu tip <b>hiçbir yasak koymaz.</b> "Şu an senin sıran değil" cümlesi bir
    /// KURAL'dır ve <see cref="TurnRules"/>'a aittir. Burada yazılsaydı sıra
    /// BİLGİSİ ile sıra KURALI aynı yerde yaşardı; kuralı sınamak için her
    /// seferinde bir savaş kurmak gerekir, ve "sıra kimde" sorusunun cevabını
    /// isteyen arayüz istemeden kural motorunu da yanında taşırdı.
    ///
    /// ZAMANI YOKTUR. <see cref="UnitLifecycle"/>'ın aksine burada <c>Tick</c>
    /// yok, çünkü tur süreli değil: sıra yalnızca <see cref="EndTurn"/> ile,
    /// yani bir ÇAĞRIYLA devredilir. Bu tek cümle aşağıdaki event kararının
    /// dayanağıdır.
    ///
    /// Neyi BİLMEZ: sahada hangi birimlerin olduğunu, hangi birimin bu turda ne
    /// yaptığını, savaşın bitip bitmediğini, sonucu kimin göstereceğini.
    /// </summary>
    public sealed class TurnState
    {
        /// <summary>
        /// İki taraflı varsayılan dizilim. Savaşların çoğu böyle başlar; başka
        /// bir dizilim isteyen kurucuya kendi listesini verir.
        /// </summary>
        public static readonly IReadOnlyList<Team> DefaultTurnOrder =
            Array.AsReadOnly(new[] { Team.Player, Team.Enemy });

        // İlk tur BİRdir, sıfır değil: bu sayı oyuncuya gösterilmek için var ve
        // arayüz "Tur 1" yazar. Sıfırdan başlasaydı ekrana yazan her yer +1
        // eklerdi, yani "turlar birden sayılır" kuralı bu dosyada değil
        // arayüzde — üstelik her arayüzde ayrı ayrı — yaşardı.
        public const int FirstTurnNumber = 1;

        // REDDEDILEN - TurnState.cs:65 yerine (liste hiç doğmaz; sıra iki değer
        //              arasında sabitlenir ve EndTurn tek satıra iner):
        //     public Team Current { get; private set; }
        //     // EndTurn içinde:
        //     Current = Current == Team.Player ? Team.Enemy : Team.Player;
        // KIRILAN  : takım dizilimi İKİ yerde yaşar — ternary'de ve tur sayacında.
        //            "herkes bir kez oynadı mı" sorusu "Current tekrar Player
        //            oldu"ya iner -> Player, Enemy, Enemy dizilimi (turda iki kez
        //            oynayan hızlı düşman) iki devirde tamamlanmış sayılır
        //            derleyici: hiçbir şey der  .  test: kırmızı —
        //            TurnNumber_AdvancesOnlyAfterEveryEntryHasPlayed
        // KAZANIRDI: oyunda ikiden fazla giriş ASLA olmayacaksa ve tur numarası
        //            her devirde artacaksa — o gün liste, derleyicinin garanti
        //            ettiğini çalışma zamanı doğrulamasına çevirir: boş liste,
        //            Team.None ve null hatalarının hiçbiri bir ternary'de olamaz.
        // TEK CUMLE: Dizilimi VERİ yapmak "kim ne zaman oynar" sorusunu tek yerde
        //            cevaplanabilir kılar; koda gömmek onu her soruya yeniden
        //            yazdırır.
        private readonly Team[] order;

        // Salt okunur görünüm bir KEZ kuruluyor. Her okumada Array.AsReadOnly
        // çağırmak aynı diziye her seferinde yeni bir sarmalayıcı üretirdi ve
        // arayüz her karede sıra listesini okuduğunda çöp toplayıcıyı beslerdi.
        private readonly ReadOnlyCollection<Team> orderView;

        // Sıranın kimde olduğu TEK yerde duruyor: bir indeks. Ayrıca bir
        // `Team current` alanı tutulsaydı iki alanı senkron tutmak bir ödev
        // olurdu ve EndTurn'de birini güncelleyip diğerini unutmak derleme
        // hatası vermezdi — hata "sıra düşmanda ama düşman oynamıyor" diye
        // bildirilirdi.
        private int index;

        /// <summary>
        /// <see cref="DefaultTurnOrder"/> ile bir savaş kurar: önce oyuncu,
        /// sonra düşman.
        /// </summary>
        public TurnState()
            : this(DefaultTurnOrder)
        {
        }

        /// <summary>
        /// Takım dizilimini vererek bir savaş kurar. Dizilim savaş boyunca
        /// DEĞİŞMEZ; sıra onun üzerinde döner.
        /// </summary>
        /// <param name="turnOrder">
        /// Sıranın izleyeceği taraflar. Aynı taraf birden çok kez geçebilir;
        /// <see cref="Team.None"/> geçemez.
        /// </param>
        public TurnState(IReadOnlyList<Team> turnOrder)
        {
            if (turnOrder == null)
            {
                throw new ArgumentNullException(nameof(turnOrder));
            }

            // Boş dizilim bir denge ayarı değil, bir çağıran hatasıdır: sırası
            // hiç kimsede olmayan bir savaşta Current okunamaz. Gürültülü
            // reddetmek, ilk EndTurn'de sıfıra bölme benzeri bir hatayla
            // patlamaktan iyidir.
            if (turnOrder.Count == 0)
            {
                throw new ArgumentException(
                    "Turn order must contain at least one team.", nameof(turnOrder));
            }

            // Dizilim KOPYALANIYOR. Çağıranın dizisi saklanmış olsaydı, savaş
            // sürerken o diziye yazan bir satır sırayı ortasından değiştirirdi;
            // bir List<Team> küçüldüğünde ise order[index] savaşın ortasında
            // IndexOutOfRangeException atardı. Kopya, "dizilim kurulduğunda
            // bellidir" sözünü tek satırda garanti eder.
            var copy = new Team[turnOrder.Count];
            for (int i = 0; i < turnOrder.Count; i++)
            {
                // REDDEDILEN - TurnState.cs:136 yerine (bu kontrol hiç yazılmaz,
                //              tarafsız taraf da sıraya girer):
                //     copy[i] = turnOrder[i];
                // KIRILAN  : her turda hiçbir şeyin olamadığı bir devir doğar.
                //            TurnRules tarafsızı hiçbir sırada eyletmez -> o
                //            devirde hiçbir birim eyleyemez -> oyun her turda bir
                //            kez donmuş görünür, hata "ara sıra takılıyor" olur
                //            derleyici: hiçbir şey der  .  test: yeşil kalır
                // KAZANIRDI: tarafsızın gerçekten oynayacağı bir şey olduğu gün —
                //            yaban canavarları, yayılan yangın, herkese ateş eden
                //            nötr kuleler; o gün "çevre turu" gerçek bir turdur ve
                //            burada yasaklamak uydurma bir üçüncü takım açtırır.
                // TEK CUMLE: default(Team) tarafsızdır, yani bu satır olmazsa
                //            elemanı atanmayı unutulmuş bir dizi geçerli bir
                //            dizilim sayılır ve savaş olmayan tarafın sırasında başlar.
                if (turnOrder[i] == Team.None)
                {
                    throw new ArgumentException(
                        "Turn order cannot contain the neutral team.", nameof(turnOrder));
                }

                // Aynı takımın listede iki kez geçmesi YASAK DEĞİL: "Player,
                // Enemy, Enemy" turda iki kez oynayan hızlı bir düşmandır ve
                // buradaki hiçbir kural onunla bozulmaz — tur, indeks sarmalınca
                // tamamlanır, kimin kaç kez oynadığına bakmaz.
                //
                // Alternatif: yinelenen girişi reddetmek. Seçilmedi: tekrar
                // yasak olsaydı "düşman turda iki kez oynar" ancak ikinci bir
                // düşman takımı uydurularak yazılırdı ve o takım TargetingRules'a
                // göre birincinin geçerli hedefi olurdu. Tetiği: bu tip takım
                // BAŞINA defter (eylem bütçesi, kaynak) tutmaya başladığı gün.
                copy[i] = turnOrder[i];
            }

            order = copy;
            orderView = Array.AsReadOnly(copy);
            TurnNumber = FirstTurnNumber;
        }

        // NEDEN TurnChanged EVENT'İ YOK — UnitLifecycle'ın gerekçesi kopyalanmadı,
        // bu tipe uygulandı ve İKİ yarısı ayrı ayrı sınandı. Oradaki cümle şuydu:
        // "dönüş değeri — soran zaten orada; event — ilgilenen başka yerde."
        //
        // Birinci yarı GEÇERSİZ: bu tipte kimsenin sormadığı bir geçiş yok.
        // UnitLifecycle'da event'i haklı çıkaran şey Tick'in içindeki
        // Downed → Dead geçişiydi — zamanla, kimse sormadan oluyordu. Burada
        // zaman yok; sıra yalnızca EndTurn ile değişir ve o çağrıyı yapan taraf
        // cevabı dönüş değeriyle alır.
        //
        // İkinci yarı — asıl sınav burada, ve ilk bakışta event'i HAKLI çıkarıyor:
        // tur değişimini duymak isteyen çok (tur başlığı, yapay zekâ sürücüsü,
        // etki süresi sayaçları) ve hiçbiri EndTurn'ü çağıran taraf değil. Buna
        // rağmen event reddedildi; gerekçe aşağıda ve StructureLifecycle'ınkinin
        // kopyası DEĞİL: orada ilgilenen zaten çağırandı, burada ilgilenen
        // başkadır ama araya AKIŞ SAHİBİ girer.
        //
        // REDDEDILEN - TurnState.cs:195 yerine:
        //     public event Action<Team> TurnChanged;
        // KIRILAN  : dinleyen taraf akışın sahibini ATLAYIP doğrudan buraya bağlanır.
        //            olay EndTurn'ün ORTASINDA yayılır -> index yeni tarafı,
        //            TurnNumber hâlâ eski turu gösterir -> o an TurnRules'a soran
        //            dinleyici oyunun hiçbir anında doğru olmayan bir cevap alır
        //            derleyici: hiçbir şey der  .  test: yeşil kalır
        // KAZANIRDI: sıra ÇAĞRISIZ da değişebilseydi — süreli tur ("30 saniyede
        //            oynamazsan pas geçersin"), bağlantısı kopan oyuncunun
        //            otomatik devri ya da sırayı çeviren bir sunucu; o gün geçişi
        //            bir Tick doğurur ve olay onu duymanın TEK yolu olur.
        // TEK CUMLE: Olay, kimsenin SORMADIĞI bir geçiş için vardır; burada geçişi
        //            yapan taraf cevabı zaten dönüş değeriyle alıyor.

        /// <summary>
        /// Sıranın o an hangi tarafta olduğu. Dizilimden TÜRETİLİR, ayrıca
        /// tutulmaz.
        /// </summary>
        public Team Current => order[index];

        /// <summary>
        /// Kaçıncı turdayız. İlk tur <see cref="FirstTurnNumber"/>'dır ve sayı
        /// yalnızca dizilimdeki HERKES bir kez oynadığında artar.
        /// </summary>
        public int TurnNumber { get; private set; }

        /// <summary>
        /// Bu savaşın takım dizilimi — salt okunur bir görünüm. Dışarıdan
        /// değiştirilemez; değiştirilebilseydi sıra savaşın ortasında kayardı.
        /// </summary>
        public IReadOnlyList<Team> TurnOrder => orderView;

        // NEDEN EYLEM SAYACI BURADA DEĞİL — "bir birim turda kaç kez eyleyebilir"
        // sorusunun iki yarısı var ve ikisi ayrı yerlere ait: KAÇ sorusu bir
        // kuraldır ve TurnRules.MaxActionsPerTurn'de yaşar; KAÇ KEZ KULLANDI
        // sorusu bir durumdur, çünkü aynı birime aynı anda sorulan aynı soru
        // farklı cevap verir. İkincisi bu tipe konmadı.
        //
        // REDDEDILEN - TurnState.cs:247 üstüne eklenmesi reddedildi:
        //     private readonly Dictionary<Unit, int> actionsUsed =
        //         new Dictionary<Unit, int>();
        //     public int ActionsUsedBy(Unit unit) { ... }
        //     // ve EndTurn içinde: actionsUsed.Clear();
        // KIRILAN  : anahtar seçilemiyor — savaşçı bugün tahtada Unit, savaşta
        //            Combatant, kuralda Team diye üç tiple temsil ediliyor.
        //            elinde başka bir tip olan çağıran SORAMAZ -> kendi eşlemesini
        //            kurar -> sayaç yine iki yerde yaşar; kaldırılan birim de
        //            sözlükte kalır ve savaş boyunca bellekte tutulur
        //            derleyici: hiçbir şey der  .  test: yeşil kalır
        // KAZANIRDI: savaşçının TEK bir kimliği olduğu gün — Unit ile Combatant
        //            birleştiğinde ya da akış sahibi buraya kararlı bir savaş içi
        //            indeks verdiğinde; o gün sıfırlama anını bilen tek yer burası
        //            olduğu için sözlüğün yeri de burasıdır.
        // TEK CUMLE: Bir sözlüğün yeri, anahtarının sahibinin yaşadığı yerdir —
        //            bu tip o eşlemenin sahibi değil.

        /// <summary>
        /// Sırayı dizilimdeki bir sonraki tarafa devreder.
        ///
        /// NEDEN DÖNÜŞ DEĞERİ: "tur tamamlandı mı" sorusunun cevabını çağıran
        /// kendi başına kuramaz — kurmak için takım dizilimini bilmek gerekir ve
        /// o bilgi burada yaşar. Çağıranın elindeki tek alternatif "önce
        /// TurnNumber'ı oku, çağır, tekrar oku, karşılaştır" üçlüsüdür; aynı üç
        /// satır arayüzde, yapay zekâda ve etki süresi sayacında üç kez doğar ve
        /// birinde ilk okuma unutulursa hata sessizdir — tur atlanmış görünür.
        /// </summary>
        /// <returns>
        /// Bu devir bir TURU tamamladıysa true; tur içindeki bir el değiştirmeyse
        /// false.
        /// </returns>
        public bool EndTurn()
        {
            index = (index + 1) % order.Length;

            // Sarmal TAM turu işaretler: dizilimin başına dönmek, herkesin bir
            // kez oynadığı demektir. Tek girişli bir dizilimde bu her devirde
            // olur ve doğrudur — tek taraflı bir savaşta her devir bir turdur.
            if (index != 0)
            {
                return false;
            }

            // REDDEDILEN - TurnState.cs:276 yerine (sarmal hiç beklenmez, her
            //              devir turu ilerletir ve yukarıdaki erken dönüş silinir):
            //     index = (index + 1) % order.Length;
            //     TurnNumber++;
            //     return true;
            // KIRILAN  : tur numarası dizilimin UZUNLUĞUNA bağlanır.
            //            "3 tur dayan" iki taraflı savaşta iki, üç girişlide bir
            //            oynama hakkı verir; "2 tur süren zehir" düşman oynamadan biter
            //            derleyici: hiçbir şey der  .  test: kırmızı —
            //            TurnNumber_DoesNotAdvanceInsideTheRound
            // KAZANIRDI: tur numarası oyuncuya HİÇ gösterilmeyecek, yalnızca
            //            olayları sıralayan bir damga (kayıt sırası, tekrar
            //            dosyası ordinali) olsaydı — orada aranan şey "kaçıncı
            //            tur" değil "hangisi önce oldu"dur.
            // TEK CUMLE: Tur, herkesin bir kez oynadığı andır; her devri tur
            //            saymak dengeyi tasarımcının hiç dokunmadığı bir sayıya
            //            bağlar.
            TurnNumber++;
            return true;
        }
    }
}
