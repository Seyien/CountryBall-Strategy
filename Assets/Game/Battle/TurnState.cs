using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GridStrategy.Combat;

namespace GridStrategy.Battle
{
    // ═══ ROL: VARLIK (Entity) ════════════════════════════════════════
    // kimlik : var — her savaşın kendi tur durumu; iki savaş aynı anda iki
    //          farklı tarafın sırasında olabilir
    // hafıza : var — ölçüsü şu: EndTurn()'ü arka arkaya İKİ kez çağır, iki
    //          FARKLI şey olur. [Player, Enemy] diziliminde birinci çağrı
    //          sırayı Player'dan Enemy'ye geçirir ve TurnNumber'a DOKUNMAZ;
    //          ikinci çağrı Enemy'den Player'a döner ve TurnNumber'ı 1
    //          artırır. Farkı doğuran şey, tipin kaçıncı sırada olduğunu
    //          çağrılar arasında hatırlaması.
    // Unity  : gerekmez — noEngineReferences: true; kare, zaman, sahne bilmez
    // karar  : tutar ve bildirir; kimin ne yapabileceğine TurnRules karar verir
    /// <summary>
    /// Bir savaşın sıra durumu: sıra hangi tarafta ve kaçıncı turdayız.
    ///
    /// Bu tip <b>hiçbir yasak koymaz.</b> "Şu an senin sıran değil" cümlesi bir
    /// KURAL'dır ve <see cref="TurnRules"/>'a aittir.
    ///
    /// ZAMANI YOKTUR. <see cref="UnitLifecycle"/>'ın aksine burada <c>Tick</c>
    /// yok, çünkü tur süreli değil: sıra yalnızca <see cref="EndTurn"/> ile,
    /// yani bir ÇAĞRIYLA devredilir.
    ///
    /// Neyi BİLMEZ: sahada hangi birimlerin olduğunu, hangi birimin bu turda ne
    /// yaptığını, savaşın bitip bitmediğini, sonucu kimin göstereceğini.
    ///
    /// GEREKÇELER: Docs/deep/kod/Battle/TurnState.md
    /// </summary>
    public sealed class TurnState
    {
        // BİR TANIM, BİR DURUM DEĞİL — ölçüsü şu: hiçbir savaş kurmadan
        // TurnState.DefaultTurnOrder okunabilir, Current okunamaz. Savaştan
        // savaşa değişmeyen bir dizilim hiçbir savaşa ait değildir; static
        // olması sızıntı değil, amacın kendisi. → TurnState.md#defaultturnorder
        // ÖDÜNÇ ALINAN — `static readonly`: bu alan bir GC KÖKÜdür, yani gösterdiği
        // dizi uygulama boyunca toplanmaz. Aşağıdaki FirstTurnNumber ise `const` ve
        // çalışma zamanında hiç depolanmaz — ikisi aynı şey görünüp ayrışıyor.
        // DİL: Docs/deep/dil/07-bellek-canlilik-ve-yikim.md
        public static readonly IReadOnlyList<Team> DefaultTurnOrder =
            Array.AsReadOnly(new[] { Team.Player, Team.Enemy });

        // İlk tur BİRdir, sıfır değil: bu sayı oyuncuya gösterilmek için var ve
        // arayüz "Tur 1" yazar. Sıfırdan başlasaydı "turlar birden sayılır"
        // kuralı bu dosyada değil her arayüzde ayrı ayrı yaşardı.
        // → TurnState.md#firstturnnumber
        public const int FirstTurnNumber = 1;

        // DİZİLİM VERİDİR, KODA GÖMÜLMÜŞ BİR DAL DEĞİL. Bir ternary yazılsaydı
        // dizilim İKİ yerde yaşardı — devirde ve tur sayacında — ve ikisi de
        // "tam iki taraf var" varsayardı. Liste bir DEĞER olduğu için uzunluğu
        // okunabiliyor ve "herkes bir kez oynadı mı" sorusu türetilebiliyor.
        // → TurnState.md#order
        private readonly Team[] order;

        // Salt okunur görünüm bir KEZ kuruluyor. Her okumada Array.AsReadOnly
        // çağırmak aynı diziye her seferinde yeni bir sarmalayıcı üretirdi ve
        // arayüz her karede sıra listesini okuduğunda çöp toplayıcıyı beslerdi.
        // → TurnState.md#orderview
        private readonly ReadOnlyCollection<Team> orderView;

        // Sıranın kimde olduğu TEK yerde duruyor: bir indeks. Ayrıca bir
        // `Team current` alanı tutulsaydı iki alanı senkron tutmak bir ödev olur,
        // birini güncelleyip diğerini unutmak derleme hatası vermezdi.
        // → TurnState.md#index
        private int index;

        /// <summary>
        /// <see cref="DefaultTurnOrder"/> ile bir savaş kurar: önce oyuncu,
        /// sonra düşman.
        /// </summary>
        public TurnState()
            : this(DefaultTurnOrder)
        {
        }

        // Üç kelepçe üç ayrı kırılmayı kapatıyor ve sertlik sırası kırılmanın
        // GÖRÜNÜRLÜĞÜYLE ters orantılı: null ile boş liste zaten patlardı,
        // tarafsız eleman ise sessizce hiç kimsenin eyleyemediği bir devir
        // doğururdu. Dizilim KOPYALANIYOR; yinelenen giriş bilerek serbest.
        // → TurnState.md#turnstateireadonlylist
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
                // default(Team) TARAFSIZDIR: bu satır olmasaydı elemanı atanmayı
                // unutulmuş bir dizi geçerli sayılır, TurnRules tarafsızı hiçbir
                // sırada eyletmediği için o devirde hiç kimse eyleyemez ve hata
                // "oyun ara sıra takılıyor" diye bildirilirdi.
                // → TurnState.md#turnstateireadonlylist
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

        // NEDEN TurnChanged EVENT'İ YOK: olay, kimsenin SORMADIĞI bir geçiş için
        // vardır; burada geçişi yapan taraf cevabı zaten EndTurn'ün dönüş
        // değeriyle alıyor. Olay yayılabileceği tek an — index yeni, TurnNumber
        // hâlâ eski — oyunun hiçbir anında doğru olmayan bir pencere.
        // → TurnState.md#turnchanged

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

        // NEDEN EYLEM SAYACI BURADA DEĞİL: sorunun "KAÇ kez eyleyebilir" yarısı
        // bir kuraldır ve TurnRules.MaxActionsPerTurn'de yaşıyor; "KAÇ KEZ
        // kullandı" yarısı bir durumdur ama sözlüğün ANAHTARI seçilemiyor —
        // savaşçı bugün tahtada Unit, savaşta Combatant, kuralda Team.
        // → TurnState.md#actionsused

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

            // TUR = index sıfıra döndüğü an. Her devri tur saymak, tur numarasını
            // dizilimin UZUNLUĞUNA bağlardı: "3 tur dayan" iki taraflı savaşta
            // iki, üç girişlide bir oynama hakkı verir — aynı cümle, iki farklı
            // denge. Erken dönüş ile bu artış aynı bilgiyi iki kez vermiyor: biri
            // "devam", diğeri "yeni tur" der. → TurnState.md#endturn
            TurnNumber++;
            return true;
        }
    }
}
