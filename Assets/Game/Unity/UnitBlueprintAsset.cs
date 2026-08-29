using GridStrategy.Combat;
using UnityEngine;

namespace GridStrategy.Unity
{
    // ═══ ROL: TANIM DOSYASI (Authoring asset) ════════════════════════
    // kimlik : var ama DİSK kimliği — her .asset dosyası ayrı bir varlıktır
    //          ve GUID'iyle anılır; oyun kimliği yok, bu tip tahtaya hiç
    //          çıkmaz
    // hafıza : yok — ölçüsü şu: bu nesneye oyun sırasında yazan hiçbir üye
    //          yok. TEK İSTİSNA ve adı konmuş: definition alanı bir oyun
    //          durumu değil, serileştirilmiş sayılardan TÜRETİLMİŞ bir
    //          değerin önbelleği
    // Unity  : zorunlu — ScriptableObject bir UnityEngine tipidir; bu tipin
    //          Core tarafına konamamasının sebebi de tam olarak budur
    // karar  : vermez, ÇEVİRİR — Inspector'daki sayıları düz C# tanımına
    //          dönüştürür ve doğrulamayı o tanımın kurucusuna bırakır
    /// <summary>
    /// Bir birim türünün TASARIMCI DOSYASI. Diskte tek başına duran bir
    /// varlıktır ve onu gösteren herkes AYNI dosyayı gösterir.
    ///
    /// ██ DUVAR BU DOSYADA AYAKTA KALIYOR ██ Ölçüldü:
    /// <c>GridStrategy.Core</c>, <c>GridStrategy.Combat</c> ve
    /// <c>GridStrategy.Battle</c> asmdef'lerinin üçü de
    /// <c>noEngineReferences: true</c> taşıyor, yani o üç katmanda
    /// <c>UnityEngine</c> KULLANILAMAZ. <see cref="ScriptableObject"/> bir
    /// <c>UnityEngine</c> tipidir; dolayısıyla veri varlığı oraya KONAMAZ.
    /// Docs/ogrenme/02-sonraki-asamalar.md dosyasının Aşama 1 bölümü bu
    /// çatalı zaten adlandırmıştı: ya tanım motora bağlanır ve duvar yıkılır,
    /// ya motor tarafında bir sarmalayıcı doğup çekirdeğe düz C# tanımını
    /// ÜRETİR. Bu dosya ikinci yoldur.
    ///
    /// DOĞRULAMA BURADA DEĞİL: <see cref="UnitBlueprint"/>'in kurucusunda.
    /// Aynı belgenin "NE KIRAR" bölümü bunu ölçerek yazmıştı — doğrulama
    /// <c>OnValidate</c>'e kayarsa yalnızca Inspector'da çalışır ve koddan
    /// ya da testten kurulan tanım hiç sınanmaz. Aşağıdaki
    /// <see cref="OnValidate"/> hiçbir şey doğrulamaz, yalnızca önbelleği
    /// düşürür.
    ///
    /// AYNA BELGE: bu tipin gerekçeleri bugün yalnızca bu dosyada.
    /// </summary>
    [CreateAssetMenu(
        menuName = "GridStrategy/Unit Blueprint",
        fileName = "NewUnitBlueprint")]
    public sealed class UnitBlueprintAsset : ScriptableObject
    {
        // BAŞLATICI DEĞERLER BOŞUNA DEĞİL: bir alan varlığın YAML'ında yoksa
        // (dosya bu alan eklenmeden önce yaratılmışsa) motor ona default(T)
        // vermez, C# BAŞLATICISININ değerini verir. Yani aşağıdaki her sayı,
        // eski bir varlığın sessizce sıfırlanmasına karşı yazılmış bir
        // sigortadır. Referans alanlarının başlatıcısı YOKTUR ve null doğarlar
        // — asıl tehlike orada, ve o tehlikeyi taşıyan tek alan icon.
        [Header("Identity shown on the panels")]
        [Tooltip("Label drawn on the palette button. Left empty, the asset file name is used instead.")]
        [SerializeField] private string displayName = "Unit";

        [Tooltip("Icon drawn on the palette button. May be left empty - the icon slot is then hidden.")]
        [SerializeField] private Sprite icon;

        // ██ AÇIKLAMA BOŞ DOĞUYOR VE BOŞLUĞU BİR EKSİKLİK DEĞİL ██
        // Türlerin kurgusu operatörün. Ölçülmüş sayılardan türetilmiş bir taslak
        // önerilebilir ama uydurma kurgu YAZILMAZ, çünkü uydurulmuş bir cümle
        // bilgi penceresinde ölçülmüş sayıların yanında durur ve okuyan ikisini
        // aynı güvenle okur.
        // Boş bırakılan açıklama pencerede bir boşluk değil, GİZLENMİŞ bir
        // etiket üretiyor; ölçüsü UnitInfoDialogView'in Present üyesinde.
        [Tooltip("Bilgi penceresinde görünen açıklama. Boş bırakılırsa etiket hiç çizilmez.")]
        [SerializeField, TextArea(3, 8)] private string description;

        // BİR ASKER TAM BİR HÜCRE KAPLAR, ve 1 sayısı burada bir varsayılan
        // değil bir KURAL: birim hücresinden taşarsa yanındaki hücreyi boyar ve
        // oyuncu "oraya yürüyebilir miyim" sorusunu artık gözle cevaplayamaz.
        // Görsel yine de hücreden bir tık küçük okunur, çünkü bu sanatta birim
        // karolarının kenarında bir iki piksellik saydam pay var.
        //
        // ALAN BURADA, BoardAdapter'DA DEĞİL — ve ölçü şu: boyut TÜR kimliğinin
        // parçasıdır, tahtanın değil. Tahtada tek bir sayı durduğu sürece bütün
        // türler aynı büyüklükte olmak zorundaydı ve taretin karargâhtan küçük
        // olması hiçbir yere yazılamıyordu.
        [Header("Board footprint - how many cells this unit covers")]
        [Tooltip("How many cells the unit covers on the board. 1 means exactly one cell.")]
        [SerializeField, Min(0.1f)] private float boardSizeInCells = 1f;

        [Header("Combat numbers - shared by every unit of this type")]
        [Tooltip("Starting and maximum health of every unit of this type.")]
        [SerializeField, Min(1)] private int maxHealth = 30;

        [Tooltip("Raw damage of a single hit, before any resistance.")]
        [SerializeField, Min(0)] private int damage = 10;

        [Tooltip("How many cells away this unit can strike. Must be at least 1.")]
        [SerializeField, Min(1)] private int attackRange = 1;

        // İKİ VURUŞ ARASINDAKİ BEKLEME, SALDIRININ TEK BEDELİ. Sıra kuralı
        // FreeForAll kipinde kalktığından beri bir saldırı hiçbir şey harcamıyor
        // ve ölçülen sonuç şuydu: oyuncu aynı hedefe üst üste tıklayınca vuruşlar
        // yığılıyor, hasar fare hızına bağlanıyordu. Buradaki sayı bir EŞİK;
        // kalan süreyi savaşçının kendi örneği (Combatant) tutuyor, tıpkı üretim
        // sayacının yapı başına yaşaması gibi.
        // SIFIR HÂLÂ GEÇERLİ ve "bekleme yok" demek — eski davranışı isteyen
        // varlık dosyası bu alanı sıfıra çeker.
        [Tooltip("Seconds between two attacks. 0 means no wait at all.")]
        [SerializeField, Min(0f)] private float attackCooldownSeconds = 1f;

        // ÖNBELLEK BİR OYUN DURUMU DEĞİL — ve aradaki fark bu tipin en pahalı
        // satırıdır. Docs/ogrenme/02-sonraki-asamalar.md dosyasının Aşama 1
        // bölümü "EN SIK HATA" başlığıyla şunu yazıyor: bir varlık çalışma
        // zamanı durumu taşımaya başlarsa onu gösteren yüzlerce birim aynı
        // değeri paylaşır ve değer Editor'de KALICI olur. Buradaki alan o
        // hatanın örneği değil, TERSİ: içinde tutulan şey serileştirilmiş
        // sayılardan türetilmiş, değişmez bir tanımdır ve tam da PAYLAŞILSIN
        // diye tutulur.
        // ÖLÇÜSÜ: aynı varlıktan iki kez Definition oku, ReferenceEquals true
        // döner. BoardAdapter'ın NewCombatant üyesinden üretilmiş iki savaşçının
        // profili için aynı ölçü false döner — paylaşım iddiasının bugünkü
        // durumu tam olarak bu.
        private UnitBlueprint definition;

        /// <summary>
        /// Ekranda görünen ad. Alan boş bırakıldıysa varlık dosyasının adı
        /// kullanılır — boş bir düğme tıklanabilir ama okunamaz.
        /// </summary>
        public string DisplayName =>
            string.IsNullOrWhiteSpace(displayName) ? name : displayName;

        /// <summary>Panel düğmesinin simgesi; atanmamışsa <c>null</c>.</summary>
        public Sprite Icon => icon;

        /// <summary>
        /// Bilgi penceresinde görünen açıklama; yazılmamışsa boş.
        /// </summary>
        // DisplayName'İN TERSİNE BOŞLUK DOLDURULMUYOR: ad boş kaldığında dosya
        // adına düşmek okunabilir bir düğme kurtarıyor, ama açıklamanın yerine
        // dosya adını yazmak oyuncuya bir cümle vaat edip bir etiket göstermek
        // olurdu.
        public string Description => description;

        /// <summary>
        /// Bu birimin tahtada kaç hücre kapladığı; 1 tam bir hücre demektir.
        /// </summary>
        /// <remarks>
        /// Sayının kendisi bir ÖLÇEK DEĞİL — motorun anladığı ölçeğe çeviren tek
        /// yer <see cref="BoardSizing"/>. Ayrım 32x32 bir görsel geldiği gün
        /// ölçülebilir hâle gelir: bu sayı aynı kalır, ölçek yarıya iner.
        ///
        /// TAŞIYICI TİP AÇILMADI ve tetikleyicisi yazılı: bugün elde TEK bir
        /// sayı ve tek bir değişmez var ("sıfırdan büyük"), onu da
        /// <c>[Min]</c> ile <see cref="BoardSizing"/> birlikte tutuyor. Bir
        /// <c>[Serializable] struct</c> her varlık dosyasına bir YAML katmanı ve
        /// Inspector'a bir açılır ok eklerdi; <c>readonly struct</c> ise
        /// serileştiricinin alanları yansımayla yazması yüzünden
        /// değişmezliği hakkında YALAN söylerdi. Tip, tahtada TUTARLI KALMASI
        /// GEREKEN İKİNCİ bir sayı doğduğu gün açılır — ayrı en ve boy, ya da
        /// birden çok hücre işgal eden bir ayak izi.
        /// </remarks>
        public float BoardSizeInCells => boardSizeInCells;

        /// <summary>
        /// Bu varlığın düz C# karşılığı. Her okumada AYNI nesne döner.
        /// </summary>
        // TEMBEL KURULUM, Awake'te DEĞİL — ve gerekçe UnitView'in Body üyesinin
        // üstünde ölçülerek yazılı, burada uygulanıyor: ScriptableObject'in
        // yaşam döngüsü geri çağrıları EditMode'da güvenilir değildir, tembel
        // property ise ilk okuyanın kim olduğuna bakmaz.
        public UnitBlueprint Definition
        {
            get
            {
                if (definition == null)
                {
                    // Kurucu fırlatabilir ve fırlatMASI istenen davranıştır:
                    // geçersiz bir tanımla sessizce oyuna devam etmek, hatayı
                    // ilk savaşa kadar saklardı. [Min] öznitelikleri Inspector
                    // tarafında zaten aynı eşikleri tutuyor, yani bu istisna
                    // ancak varlık koddan ya da bozuk bir YAML'dan gelirse doğar.
                    definition = new UnitBlueprint(
                        DisplayName,
                        maxHealth,
                        new AttackProfile(damage, attackRange, attackCooldownSeconds));
                }

                return definition;
            }
        }

        // HİÇBİR ŞEY DOĞRULAMAZ, YALNIZCA ÖNBELLEĞİ DÜŞÜRÜR. Buraya bir
        // doğrulama konsaydı yukarıdaki bütün gerekçe çökerdi: kural Inspector'a
        // taşınmış olurdu ve koddan kurulan tanım hiç sınanmazdı. Yaptığı tek iş,
        // tasarımcı oyun sırasında sayıyı değiştirdiğinde bayat tanımın
        // yaşamasını engellemek.
        private void OnValidate()
        {
            definition = null;
        }
    }
}
