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

        [Header("Combat numbers - shared by every unit of this type")]
        [Tooltip("Starting and maximum health of every unit of this type.")]
        [SerializeField, Min(1)] private int maxHealth = 30;

        [Tooltip("Raw damage of a single hit, before any resistance.")]
        [SerializeField, Min(0)] private int damage = 10;

        [Tooltip("How many cells away this unit can strike. Must be at least 1.")]
        [SerializeField, Min(1)] private int attackRange = 1;

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
                        new AttackProfile(damage, attackRange));
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
