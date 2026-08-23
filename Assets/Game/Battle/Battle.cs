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
    /// tahtayı tanımaz — ve iki assembly birbirini GÖRMEZ. İkisini birden
    /// referans eden ilk assembly burasıdır; bu tip o birleşmenin DURUMUNU tutar,
    /// <see cref="BattleActions"/> ise AKIŞINI.
    ///
    /// SAHİPLENDİĞİ ÜÇ ŞEY: kim nerede (tahta + iki sözlük), sıra kimde
    /// (<see cref="Turn"/>) ve kimin durumu değişti
    /// (<see cref="UnitStateChanged"/>). Üçü de bir KURAL değil bir DURUMdur.
    ///
    /// Neyi BİLMEZ: sıranın kimde olmasının ne anlama geldiğini
    /// (<see cref="TurnRules"/>'ın işi), bir birimin bu turda hareket edip
    /// etmediğini, mesafenin nasıl ölçüldüğünü, saldırının nasıl çözüldüğünü,
    /// sonucu kimin göstereceğini. Burada tek bir oyun kuralı yoktur — yalnızca
    /// "her kayıtlı parçanın tahtada tam olarak bir hücresi vardır" değişmezi.
    ///
    /// GEREKÇELER: Docs/deep/kod/Battle/Battle.md
    /// </summary>
    // DERİN ANLATIM: Docs/deep/konular/02-assembly-duvari.md
    // HARİTA: Docs/deep/00-iskelet.md — oyunun ne olduğu, hangi tasarım basıncının
    // hangi parçayı doğurduğu ve hangi sorunun hangi dosyaya gittiği; sistemin
    // tamamı tek sayfada ve bu tip onun ortasında duruyor.
    public sealed class Battle
    {
        // TAHTAYI BU TİP SAHİPLENİR, DIŞARIDAN ALMAZ. Kurucu bir UnitGrid
        // ALSAYDI aynı nesneye ikinci bir ok doğardı ve o oktan yazan her satır
        // sözlükleri atlardı — tahtada duran ama savaşta olmayan bir birim.
        // Koruma readonly'den gelmiyor: dışarıda referansın HİÇ doğmamasından.
        // → Battle.md#board
        // DERİN ANLATIM: Docs/deep/konular/03-tahta-sahipligi.md
        private readonly UnitGrid board;

        // ANAHTAR NESNENİN KENDİSİ, HÜCRE DEĞİL: hücreyle anahtarlansaydı birim
        // her hareket ettiğinde anahtar bozulur, savaşçı eski hücrede kalırdı.
        // Nesne anahtarı tahtaya hiç bakmıyor; güncellenecek bir şey yok.
        // → Battle.md#combatants
        private readonly Dictionary<Unit, Combatant> combatants =
            new Dictionary<Unit, Combatant>();

        // İKİNCİ SÖZLÜK, İKİNCİ TAHTA DEĞİL: yapılar birimlerle AYNI UnitGrid'e
        // giriyor, yani "bu hücre dolu mu" sorusunun tek sahibi hâlâ tahta.
        // Ayrı bir structureBoard o soruyu ikiye böler ve iki cevabın ikisi de
        // "hayır" diyebilirdi. → Battle.md#structures
        private readonly Dictionary<Unit, Structure> structures =
            new Dictionary<Unit, Structure>();

        // OLAY YÖNLENDİRİCİLERİ, her savaşçı için BİR tane. Sözlük bir konfor
        // değil zorunluluk: abone edilen şey her birime özel bir KAPANIŞ ve
        // kapanışlar birbirine eşit değildir — aynı metni ikinci kez yazarak
        // abonelik ÇÖZÜLEMEZ, sökmek için tam olarak o örnek saklanmalı.
        // → Battle.md#stateforwarders
        // DERİN ANLATIM: Docs/deep/konular/01-olay-zinciri.md — dört durak (sayaç ->
        // savaşçı -> kayıt memuru -> çevirmen), hangi aboneliğin NEDEN sözlük
        // gerektirdiği ve sökülmezse önce neyin patladığı orada hikâye olarak.
        // ÖDÜNÇ ALINAN — `Delegate`: `+=` ile `-=` derleyicide Combine ve Remove
        // çağrılarına iner, Remove ise hedef artı metot eşitliğine bakar; üstteki
        // "kapanışlar birbirine eşit değildir" cümlesinin makinesi orada.
        // DİL: Docs/deep/dil/06-delege-arka-taraf.md
        private readonly Dictionary<Unit, Action<UnitState, UnitState>> stateForwarders =
            new Dictionary<Unit, Action<UnitState, UnitState>>();

        /// <summary>
        /// Belirtilen ölçüde boş bir savaş kurar. Ölçü doğrulaması burada
        /// KOPYALANMIYOR; <see cref="UnitGrid"/> kendi kurucusunda zaten
        /// yapıyor ve tek sahibi odur.
        /// </summary>
        // DERİN ANLATIM: Docs/deep/konular/03-tahta-sahipligi.md
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
        // MoveAction.Execute'a verecek bir UnitGrid'e ihtiyacı var. public
        // olsaydı sahiplik sözü tek satırda çözülürdü; internal, sözü
        // derleyiciye söyletir. → Battle.md#board-internal
        // DERİN ANLATIM: Docs/deep/konular/03-tahta-sahipligi.md
        internal UnitGrid Board => board;

        public int Width => board.Width;

        public int Height => board.Height;

        // AŞAĞIDAKİ İKİ ÜYE ÇAĞIRANI OLDUĞU İÇİN VAR ve ikisi de tahtaya
        // DEVREDİYOR, kuralı kopyalamıyor: sınır kuralının tek metni
        // UnitGrid.IsInsideGrid'de, çarpım UnitGrid.CellCount'ta yaşıyor.
        // → Battle.md#cellcount

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

        // AD BORCU, AÇIKÇA YAZILIYOR: bu projede Unit "tahtada yer kaplayan,
        // kimliği olan şey" demek, dolayısıyla UnitCount adı saydığından geniş
        // okunuyor. Toplamı döndürmek imzayı değiştirmez, yalnızca cevabı
        // değiştirir — derleyicinin göremediği tek değişiklik türü.
        // → Battle.md#unitcount
        /// <summary>Bu savaşta kayıtlı savaşçı sayısı — yapılar dahil DEĞİL.</summary>
        public int UnitCount => combatants.Count;

        /// <summary>Bu savaşta kayıtlı yapı sayısı.</summary>
        public int StructureCount => structures.Count;

        // SIRA DURUMUNUN SAHİBİ BU TİP, çünkü bir durumun sahibi onu DEĞİŞTİREN
        // değil ömrünü PAYLAŞAN şeydir: sıra savaşla doğar, savaşla ölür. Ekrana
        // taşınsaydı kural EditMode'da sınanamazdı; static bir alana konsaydı
        // durum test metotları arasında sızardı. → Battle.md#turn
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
        // olamaz: Combatant kendi Unit'ini BİLMEZ, çünkü kimlik parçalarda değil
        // bu tipin sözlüğünde yaşıyor. Toplu süpürme bu olayın yerine GEÇMEZ:
        // olay "durum değişti" der, süpürme "artık silinebilir".
        // → Battle.md#unitstatechanged
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
        /// çağrıyı yapan taraf cevabı zaten dönüş değeriyle alır.
        /// </summary>
        // DERİN ANLATIM: Docs/deep/konular/01-olay-zinciri.md — dört durak (sayaç ->
        // savaşçı -> kayıt memuru -> çevirmen), hangi aboneliğin NEDEN sözlük
        // gerektirdiği ve sökülmezse önce neyin patladığı orada hikâye olarak.
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

            // İKİNCİ KELEPÇE, ve yönü tersi: üstteki "bu KİMLİK zaten içeride
            // mi" diye sorar, bu "bu PARÇA zaten başka bir kimliğe bağlı mı"
            // diye. Açık kalan yön sessizdir — aynı Combatant iki Unit altında
            // kayıtlıysa tek geçiş iki kimlikle yayılır ve dinleyici aynı ölümü
            // iki kez görür. → Battle.md#addunitunit-combatant-int-int
            if (combatants.ContainsValue(combatant))
            {
                throw new ArgumentException(
                    "This combatant is already registered under another unit.", nameof(combatant));
            }

            // SIRA BİR KARARDIR: bütün ret sebepleri HİÇBİR ŞEY yazılmadan
            // sorulur, sonra iki yazma arka arkaya yapılır. Ön kontrol olmasaydı
            // çakışmayı Dictionary.Add bildirirdi — ama PlaceUnit çoktan
            // yazmış olurdu ve aynı Unit iki hücrede birden dururdu.
            // → Battle.md#addunitunit-combatant-int-int
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
        // → Battle.md#addstructureunit-structure-int-int
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

        // İKİ EKLEME YOLUNUN ORTAK KAPISI: ret sebepleri burada TEK kez yazılı.
        // Dolu hücre burada bir ÇAĞIRAN HATASIdır çünkü üstüne yazmak, hücresi
        // olmayan bir savaşçı bırakırdı. Tahta dışı koordinat burada SORULMUYOR
        // — sorusunun sahibi UnitGrid.PlaceUnit ve o hiçbir hücreye dokunmadan
        // patlıyor. → Battle.md#throwifcannotjoinunit-int-int
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
        // Kimliğe göre silmek tahtayı taratır ve bu maliyet UnitGrid.RemoveUnit'in
        // KAZANIRDI satırında ÖNCEDEN adı konmuştu. Dönüş bool: "bu savaşta
        // yoktu" bir çağıran hatası değil, çünkü temizlik aynı birim için iki kez
        // çalışabilir. → Battle.md#removeunitunit
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
                // ABONELİK BURADA BIRAKILIYOR. Yalnız sözlükten silmek, çağıranın
                // elinde kalan Combatant üzerinden savaşta OLMAYAN bir birim için
                // kimlikli olay yayılmasına izin verirdi; delege bu savaşı
                // tuttuğu için o birim çöp de olamazdı.
                // → Battle.md#removeunitunit
                // ÖDÜNÇ ALINAN — GC kökü ve OKUN YÖNÜ: ok yayıncıdan aboneye gider,
                // yani sökülmemiş tek bir abonelikte elde kalan Combatant bu savaşın
                // tamamını erişilebilir tutar; zinciri koparan satır aşağıdaki `-=`.
                // DİL: Docs/deep/dil/07-bellek-canlilik-ve-yikim.md
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
        // NEDEN BURADA: savaşçı kümesinin sahibi bu tip ve kümeyi dolaşabilen
        // başka kimse yok. Küme `IEnumerable` olarak dışarı açılsaydı döngü de
        // dışarı taşınır, numaralandırıcı arayüz ardında KUTULANIR ve her Update
        // bir tahsis yapardı. → Battle.md#tickfloat-deltaseconds
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
        // InvalidOperationException fırlatır. Süpürmenin baktığı küme SAVAŞ
        // KAYDIdır, görsel tablo değil: görselsiz eklenen bir savaşçı görsel
        // tabloda YOKTUR ve asla temizlenmezdi.
        // → Battle.md#removereadyforcleanuplist
        // DERİN ANLATIM: Docs/deep/konular/05-yasam-dongusu.md
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
        // KONUM HİÇ ÖNBELLEĞE ALINMIYOR: ikinci bir positions sözlüğü tutulsaydı
        // MoveAction tahtayı doğrudan değiştirir, sözlük duymaz ve birim
        // yaklaşmış olduğu hâlde saldırı "menzil dışı" derdi. Önbellek bir hız
        // kararı değil, ikinci bir doğruluk kaynağı yaratma kararıdır.
        // → Battle.md#trygetpositionunit-out-int-out-int
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
