using System;
using System.Collections.Generic;

namespace GridStrategy.Combat
{
    // ═══ ROL: TANIM (Definition) ═════════════════════════════════════
    // kimlik : yok — iki "Barrack" tanımı aynı şeydir; bir haritadaki
    //          otuz baraka tek bir örneği paylaşabilir
    // hafıza : yok — ölçüsü şu: bu nesneye yazan hiçbir üye yok; bekleme
    //          süresi SAYAÇ değil EŞİK olarak duruyor ve sayaç
    //          StructureProduction'da, yani yapı BAŞINA yaşıyor
    // Unity  : gerekmez — düz C# nesnesi; tasarımcının göreceği dosya
    //          motor tarafındaki StructureBlueprintAsset
    // karar  : vermez — "ne üretir", "ne kadar canı var", "saldırır mı"
    //          sorularını CEVAPLAR; "şu an üretebilir mi" sorusunun
    //          sahibi ProductionRules
    /// <summary>
    /// Bir yapı TÜRÜNÜN değişmez tanımı: barakayı elektrik santralinden ayıran
    /// şey budur.
    ///
    /// <b>Bu projede yapı türü bir enum DEĞİL, bu tipin bir örneğidir.</b>
    /// Gerekçesi aşağıda ölçülerek yazılı ve tetikleyici koşulu da orada.
    ///
    /// Neyi TUTMAZ: yapının o anki canını, ayakta olup olmadığını, tarafını,
    /// tahtada nerede durduğunu, bir sonraki üretime kaç saniye kaldığını.
    /// Sonuncusu ayrıca önemli: bekleme süresi burada bir EŞİKtir, sayaç
    /// değil — sayacı tutan yer <see cref="StructureProduction"/> ve o, yapı
    /// başına bir tanedir.
    ///
    /// AYNA BELGE: bu tipin gerekçeleri bugün yalnızca bu dosyada; ayrı bir
    /// derin anlatım henüz yazılmadı.
    /// </summary>
    // ENUM REDDEDİLDİ VE ÖLÇÜSÜ ŞU: bir enum ancak EN AZ BİR KURALI
    // dallandırdığında yerini hak eder — StructureState.md'deki "Rubble"
    // reddinin ölçüsü de buydu. Yapı türü bugün hiçbir kuralı dallandırmıyor:
    // sol panel takıma göre ayrılıyor, sağ panel Produces listesini okuyor,
    // saldırı sorusu AttackProfile'ın null olup olmadığına bakıyor, üretim
    // sorusu Produces'ın boş olup olmadığına. Dördü de VERİ, hiçbiri dal
    // değil. Bir StructureKind enum'u eklenseydi "bu yapı ne" sorusunun İKİ
    // sahibi olurdu (enum ve bu tanım) ve ayrıştıkları gün hiçbir derleme
    // hatası çıkmazdı.
    // TETİKLEYİCİ: bir kural türe göre dallandığı gün — örneğin "elektrik
    // santrali menzilindeki barakalar daha hızlı üretir" — enum yerini hak
    // eder, çünkü o gün dal gerçekten doğar.
    public sealed class StructureBlueprint
    {
        // Liste DIŞARIDAN alınıp İÇERİDE kopyalanıyor. Kopyalanmasaydı çağıran
        // elindeki listeyi sonradan değiştirerek bu tanımdan doğmuş BÜTÜN
        // yapıların ne ürettiğini değiştirebilirdi — ve tanımın değişmezliği,
        // paylaşılabilirliğinin tek dayanağıdır.
        private readonly UnitBlueprint[] produces;

        /// <param name="attackProfile">
        /// Saldırmayan yapılar için <c>null</c>. İsteğe bağlılık
        /// <see cref="Structure"/>'ın kurucusundan DEVRALINIYOR ve gerekçesi
        /// orada yazılı: kural olan davranış "saldırmaz"dır.
        /// </param>
        /// <param name="produces">
        /// Bu yapının ürettiği birim türleri. Boş olabilir — elektrik santrali
        /// gerçek bir yapıdır ve hiçbir şey üretmez.
        /// </param>
        /// <param name="defaultProducedIndex">
        /// Sağ panelde AÇILIŞTA seçili duracak birimin sırası. Liste boşsa 0
        /// olmak zorundadır.
        /// </param>
        /// <param name="productionSeconds">
        /// İki üretim arasındaki bekleme. 0 geçerlidir ve "anında" demektir.
        /// </param>
        public StructureBlueprint(
            string displayName,
            int maxHealth,
            AttackProfile attackProfile,
            IReadOnlyList<UnitBlueprint> produces,
            int defaultProducedIndex,
            float productionSeconds)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("Display name cannot be empty.", nameof(displayName));
            }

            if (maxHealth < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxHealth), maxHealth, "Max health must be at least 1.");
            }

            // SIFIR SANİYE GEÇERLİ, negatif değil — ve bu, StructureLifecycle'ın
            // "enkaz penceresi POZİTİF olmalı" kelepçesinin KOPYASI DEĞİL, bilerek
            // gevşetilmiş hâlidir. Orada sıfır anlamsızdı: hiç görünmeyen bir enkaz
            // yoktur. Burada sıfırın anlamı var ve adı konmuş: ANINDA ÜRETİM.
            // Eşiği 1'e çekseydik "anında üretim" ikinci bir mekanizma olarak
            // yeniden yazılmak zorunda kalırdı.
            if (productionSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(productionSeconds), productionSeconds,
                    "Production time cannot be negative.");
            }

            // BOŞ LİSTE İLE null AYNI ŞEY SAYILIYOR ve bu bir kolaylık değil bir
            // karar: "hiçbir şey üretmeyen yapı" bu oyunun kural hâlidir, istisnası
            // değil. null'ı istisnayla reddetseydik her duvar ve her depo kendine
            // boş bir dizi uydurmak zorunda kalırdı.
            this.produces = produces == null
                ? Array.Empty<UnitBlueprint>()
                : CopyWithoutHoles(produces);

            // İNDİS ARALIĞI BURADA KELEPÇELENİYOR ÇÜNKÜ TEK OKUYUCUSU EKRAN.
            // Aralık dışı bir indis, sağ paneli açan ilk tıklamada patlardı — yani
            // hata tanım kurulurken değil, oyunun ortasında görünürdü. Boş listede
            // tek geçerli değer 0: "hiçbir şey üretmeyen yapının varsayılanı" diye
            // bir şey yok ve uydurulmuş bir indis o yokluğu gizlerdi.
            if (defaultProducedIndex < 0
                || defaultProducedIndex >= Math.Max(1, this.produces.Length))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(defaultProducedIndex), defaultProducedIndex,
                    "Default produced index is outside the produced unit list.");
            }

            DisplayName = displayName;
            MaxHealth = maxHealth;
            AttackProfile = attackProfile;
            DefaultProducedIndex = defaultProducedIndex;
            ProductionSeconds = productionSeconds;
        }

        /// <summary>Ekranda görünen ad; sol paneldeki düğmenin yazısı.</summary>
        public string DisplayName { get; }

        /// <summary>Bu türden doğan her yapının başlangıç ve azami canı.</summary>
        public int MaxHealth { get; }

        /// <summary>
        /// Saldırı tanımı; saldırmayan yapı türlerinde <c>null</c>. İki atış
        /// arasındaki bekleme de bu nesnenin içindedir
        /// (<see cref="AttackProfile.CooldownSeconds"/>).
        /// </summary>
        // İKİ BEKLEME SÜRESİ YAN YANA DURUYOR VE KARIŞMIYORLAR, çünkü ayrı iki
        // şeyi ölçüyorlar: aşağıdaki ProductionSeconds iki BİRİM arasındaki
        // bekleme, profildeki ise iki ATIŞ arasındaki. İkincisi buraya düz bir
        // alan olarak konsaydı saldırmayan her yapı anlamsız bir atış
        // beklemesi taşır ve saldıran yapının sayısı sahibinden ayrı düşerdi.
        public AttackProfile AttackProfile { get; }

        /// <summary>İki ÜRETİM arasındaki bekleme eşiği; 0 ise anında.</summary>
        public float ProductionSeconds { get; }

        /// <summary>Sağ panelde açılışta seçili duracak birimin sırası.</summary>
        public int DefaultProducedIndex { get; }

        /// <summary>Bu yapının ürettiği birim türleri; boş olabilir.</summary>
        // DİZİ DIŞARIYA IReadOnlyList OLARAK VERİLİYOR, dizinin kendisi değil:
        // dizi verilseydi çağıran bir elemanı değiştirerek bu tanımdan doğmuş
        // bütün yapıların üretim listesini bozabilirdi. IEnumerable de reddedildi
        // — sağ panel indise göre çalışıyor ve numaralandırıcı her açılışta bir
        // tahsis üretirdi.
        public IReadOnlyList<UnitBlueprint> Produces => produces;

        /// <summary>
        /// Bu yapı türü birim üretir mi. Çağıranın <c>Produces.Count &gt; 0</c>
        /// yazmasını engellemek için var; <see cref="Structure.CanAttack"/> ile
        /// aynı gerekçe, aynı şekil.
        /// </summary>
        public bool CanProduce => produces.Length > 0;

        /// <summary>
        /// Açılışta seçili duracak birim türü; hiçbir şey üretmeyen yapılarda
        /// <c>null</c>.
        /// </summary>
        public UnitBlueprint DefaultProduced =>
            produces.Length == 0 ? null : produces[DefaultProducedIndex];

        /// <summary>
        /// Bu tanımdan yeni bir yapı kurar. Taraf DIŞARIDAN gelir — aynı baraka
        /// tanımı hem oyuncunun hem düşmanın tarafında kullanılır.
        /// </summary>
        // ENKAZ PENCERESİ BİLEREK SERİLEŞTİRİLMEDİ: "yıkık yapı kaç saniye
        // ekranda kalır" sorusunun ZATEN bir sahibi var (StructureLifecycle'daki
        // sabit) ve bir tanım alanı o sabiti sessizce ezerdi. Aynı gerekçe
        // BoardAdapter'ın NewCombatant üyesinin üstünde de yazılı; burada
        // tekrar edilmiyor, uygulanıyor.
        public Structure CreateStructure(Team team)
        {
            return new Structure(
                new Health(MaxHealth),
                new StructureLifecycle(),
                team,
                AttackProfile);
        }

        /// <summary>
        /// Gelen listeyi kopyalar ve <c>null</c> elemanları ATAR.
        /// </summary>
        // BOŞ DİZİ GÖZÜ SESSİZ DEĞİL GÜRÜLTÜLÜ OLMALIYDI — ve burada olamıyor.
        // Motor tarafındaki dizide atanmamış bir gözün karşılığı null'dır ve o
        // null'ı BURADA bir istisnaya çevirmek, tasarımcının bir dizi gözünü
        // doldurmayı unutmasını oyunu açılmaz hâle getirirdi. Karar: bu tip
        // sessizce ATAR, gürültüyü ise motor tarafındaki sarmalayıcı yapar —
        // orada tasarımcıya gösterilecek bir nesne ve bir konsol var, burada yok.
        private static UnitBlueprint[] CopyWithoutHoles(IReadOnlyList<UnitBlueprint> source)
        {
            var kept = new List<UnitBlueprint>(source.Count);
            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] != null)
                {
                    kept.Add(source[i]);
                }
            }

            return kept.ToArray();
        }
    }
}
