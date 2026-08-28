using System.Collections.Generic;
using GridStrategy.Combat;
using UnityEngine;

namespace GridStrategy.Unity
{
    // ═══ ROL: TANIM DOSYASI (Authoring asset) ════════════════════════
    // kimlik : var ama DİSK kimliği — Barrack.asset ile PowerPlant.asset iki
    //          ayrı dosyadır ve YAPI TÜRÜ AYRIMI tam olarak bu iki dosyanın
    //          varlığından doğar; kodda bir enum yoktur
    // hafıza : yok — tek istisna definition önbelleği; gerekçesi
    //          UnitBlueprintAsset'te yazılı ve burada tekrar edilmiyor
    // Unity  : zorunlu — ScriptableObject bir UnityEngine tipidir
    // karar  : vermez, ÇEVİRİR — ve çeviremediğini SESSİZ bırakmaz;
    //          atanmamış dizi gözü ile aralık dışı varsayılan indis burada
    //          konsola düşer
    /// <summary>
    /// Bir yapı türünün TASARIMCI DOSYASI: barakayı elektrik santralinden
    /// ayıran şey, kodda bir enum değeri değil, bu tipten iki ayrı varlık
    /// dosyası olmasıdır.
    ///
    /// Duvar gerekçesi <see cref="UnitBlueprintAsset"/>'te ölçülerek yazıldı ve
    /// burada tekrar edilmiyor, uygulanıyor: doğrulama düz C# tanımının
    /// kurucusunda, <see cref="OnValidate"/> yalnızca önbelleği düşürüyor.
    ///
    /// BU DOSYANIN FAZLADAN TAŞIDIĞI TEK İŞ GÜRÜLTÜ: bir dizi gözü boş
    /// bırakıldığında ya da varsayılan indis listenin dışına düştüğünde,
    /// çekirdek tarafı bunu SESSİZCE düzeltmek zorunda (orada gösterilecek bir
    /// nesne ve bir konsol yok). Burada var — ve bu yüzden hata burada
    /// bağırıyor.
    ///
    /// AYNA BELGE: bu tipin gerekçeleri bugün yalnızca bu dosyada.
    /// </summary>
    [CreateAssetMenu(
        menuName = "GridStrategy/Structure Blueprint",
        fileName = "NewStructureBlueprint")]
    public sealed class StructureBlueprintAsset : ScriptableObject
    {
        [Header("Identity shown on the palette")]
        [Tooltip("Label drawn on the palette button. Left empty, the asset file name is used instead.")]
        [SerializeField] private string displayName = "Structure";

        [Tooltip("Icon drawn on the palette button. May be left empty - the icon slot is then hidden.")]
        [SerializeField] private Sprite icon;

        // 1,25 ÖLÇÜLDÜ, SEÇİLMEDİ. Eski tek sayı 1,6 idi ve sonucu şuydu: yan
        // yana duran iki bina, çizilen görsel genişliğinin yüzde otuz yedi
        // buçuğu kadar üst üste biniyor ve birbirini boyuyordu. 1,25'te taşma
        // yarıya iniyor, komşuların arasında görünür bir boşluk kalıyor ve bina
        // yine de birimden büyük okunuyor — çünkü birim karoları kenarda bir iki
        // piksel saydam pay taşırken bina karoları kenardan kenara boyalı.
        //
        // BU SAYI TÜR KİMLİĞİNİN PARÇASI: barakayı elektrik santralinden ayıran
        // şey nasıl kodda bir enum değil bu dosyanın varlığıysa, taretin
        // karargâhtan küçük durması da tahtadaki tek bir alan değil bu alandır.
        [Header("Board footprint - how many cells this structure covers")]
        [Tooltip("How many cells the structure covers on the board. 1 means exactly one cell.")]
        [SerializeField, Min(0.1f)] private float boardSizeInCells = 1.25f;

        [Header("Structure numbers - shared by every structure of this type")]
        [Tooltip("Starting and maximum health of every structure of this type.")]
        [SerializeField, Min(1)] private int maxHealth = 50;

        // SIFIR MENZİL BURADA "SALDIRMAZ" DEMEK, ve bu AttackProfile'ın
        // "menzil en az 1" kelepçesiyle ÇELİŞMİYOR: o kelepçe bir saldırı
        // TANIMININ içindedir ve orada sıfır gerçekten anlamsızdır. Buradaki
        // sıfır bir menzil değil, bir YOKLUK işaretidir — Structure'ın saldırı
        // profilini isteğe bağlı yapan kararın Inspector'daki karşılığı.
        // Alternatif ölçüldü ve reddedildi: ayrı bir "saldırır mı" onay kutusu,
        // aynı bilgiyi İKİ alana yazar ve ikisi ayrıştığında hangisinin doğru
        // olduğunu hiçbir derleme hatası söylemezdi.
        [Header("Attack - range 0 means this structure does not attack at all")]
        [Tooltip("Raw damage of a single hit. Ignored entirely when the range below is 0.")]
        [SerializeField, Min(0)] private int damage;

        [Tooltip("How many cells away this structure can strike. 0 means it never attacks.")]
        [SerializeField, Min(0)] private int attackRange;

        // KULENİN ATIŞ HIZI ARTIK KENDİ TANIMINDA. Otomatik ateş eden yapı
        // tahtanın Inspector'ındaki tek sayıyla ateş ederse taret ile mancınık
        // aynı hızda vurur; buradaki sayı o tekliği kırıyor. Menzil sıfırken
        // okunmuyor, çünkü saldırmayan yapının saldırı profili hiç kurulmuyor.
        [Tooltip("Seconds between two shots. Ignored when the range above is 0.")]
        [SerializeField, Min(0f)] private float attackCooldownSeconds = 1.5f;

        // BOŞ DİZİ GEÇERLİ VE KURAL HÂLİ: elektrik santrali hiçbir şey üretmez.
        // Başlatıcı boş bir dizi veriyor, null değil — çekirdek tarafı null'ı
        // zaten boş sayıyor ama iki yolun aynı yere çıkması, ikisinin de
        // denenmesi gerektiği anlamına gelmiyor.
        [Header("Production - leave empty for a structure that produces nothing")]
        [Tooltip("Unit types this structure can produce. Empty is valid: a power plant produces nothing.")]
        [SerializeField] private UnitBlueprintAsset[] produces = new UnitBlueprintAsset[0];

        [Tooltip("Which entry of the list above starts selected on the right panel.")]
        [SerializeField, Min(0)] private int defaultProducedIndex;

        // ÜÇ SEÇENEK ÖLÇÜLDÜ, SÜRELİ ÜRETİM KAZANDI — ve gerekçe bu alanın
        // varlığında duruyor. Bu ağaçta Tick(float) omurgası BEŞ tipte zaten
        // var ve sınanmış durumda; ekonomi ise HİÇ yok (enerji, maliyet ve
        // kaynak aramaları sıfır sonuç veriyor). Yani üretim hızını
        // kelepçeleyebilecek tek büyüklük ZAMAN. Sıfır yazmak "anında"
        // demektir, yani ikinci seçenek ayrı bir mekanizma olarak değil bu
        // alanın bir DEĞERİ olarak duruyor.
        [Tooltip("Seconds between two productions. 0 means instant - the structure never waits.")]
        [SerializeField, Min(0f)] private float productionSeconds = 3f;

        private StructureBlueprint definition;

        // İKİ ÖNBELLEK AMA TEK GEÇİŞ, ve tek geçiş olması bu dosyanın en pahalı
        // kararıdır: aşağıdaki Build boş dizi gözlerini ATIYOR, yani ham
        // produces dizisinin i'inci gözü ile tanımın i'inci birimi AYNI ŞEY
        // OLMAK ZORUNDA DEĞİL. Simgeyi ham diziden okuyan bir panel, dizinin
        // ortasında tek bir boş göz varken oyuncuya BAŞKA bir birimin resmini
        // gösterir ve hiçbir şey patlamaz. Bu liste o yüzden tanımla birlikte,
        // aynı döngüde doluyor.
        private UnitBlueprintAsset[] producedAssets;

        /// <summary>Sol paneldeki düğmenin yazısı.</summary>
        public string DisplayName =>
            string.IsNullOrWhiteSpace(displayName) ? name : displayName;

        /// <summary>Panel düğmesinin simgesi; atanmamışsa <c>null</c>.</summary>
        public Sprite Icon => icon;

        /// <summary>
        /// Bu yapının tahtada kaç hücre kapladığı; 1 tam bir hücre demektir.
        /// </summary>
        /// <remarks>
        /// ÖLÇEK DEĞİL, NİYET: sayıyı motorun anladığı <c>localScale</c>'e
        /// çeviren tek yer <see cref="BoardSizing"/>. Taşıyıcı tip kararı ile
        /// tetikleyicisi <see cref="UnitBlueprintAsset.BoardSizeInCells"/>
        /// üstünde ölçülerek yazıldı ve burada tekrar edilmiyor, uygulanıyor.
        /// </remarks>
        public float BoardSizeInCells => boardSizeInCells;

        /// <summary>
        /// Bu varlığın düz C# karşılığı. Her okumada AYNI nesne döner — bu
        /// tanımdan konmuş bütün barakalar tek bir tanımı paylaşır.
        /// </summary>
        public StructureBlueprint Definition
        {
            get
            {
                EnsureBuilt();
                return definition;
            }
        }

        /// <summary>
        /// Ürettiği birimlerin VARLIK dosyaları; sırası ve uzunluğu
        /// <see cref="StructureBlueprint.Produces"/> ile birebir aynıdır.
        ///
        /// OYUNDA NE İŞE YARAR: sağ paneldeki üretim düğmelerinin simgesi ve
        /// bir birim sürüklenirken imlecin altında duran resim buradan gelir.
        /// Simge bir <c>UnityEngine</c> tipidir ve çekirdek tarafındaki tanımın
        /// içinde taşınamaz; zincir bu yüzden varlıktan panele kadar açık
        /// duruyor.
        /// </summary>
        public IReadOnlyList<UnitBlueprintAsset> ProducedAssets
        {
            get
            {
                EnsureBuilt();
                return producedAssets;
            }
        }

        private void OnValidate()
        {
            // İKİSİ BİRLİKTE DÜŞÜYOR: yalnız biri düşseydi bir dizi gözünün
            // boşaltılması iki listenin uzunluğunu ayırır ve sağ panel, adı
            // yazan birimin yanına BAŞKA bir birimin simgesini çizerdi.
            definition = null;
            producedAssets = null;
        }

        /// <summary>
        /// Tanım ile varlık listesini bir kez kurar; ikisi de aynı geçişten
        /// doğar.
        /// </summary>
        private void EnsureBuilt()
        {
            if (definition == null)
            {
                Build();
            }
        }

        /// <summary>
        /// Inspector'daki sayılardan düz C# tanımını ve ona eşlik eden varlık
        /// listesini kurar; kuramadığını konsola bildirir.
        /// </summary>
        private void Build()
        {
            var units = new List<UnitBlueprint>(produces.Length);
            var assets = new List<UnitBlueprintAsset>(produces.Length);
            for (int i = 0; i < produces.Length; i++)
            {
                if (produces[i] == null)
                {
                    // SESSİZ DÜZELTME BURADA BİTİYOR. Çekirdek tarafı boş gözü
                    // atmak zorunda çünkü orada gösterilecek bir nesne yok;
                    // burada var. Atanmamış bir göz, sağ panelde eksik bir birim
                    // demektir ve o eksikliği oyuncu değil tasarımcı görmeli.
                    Debug.LogError(
                        $"[StructureBlueprintAsset] '{name}' has an empty slot at produces[{i}]. " +
                        "The slot is dropped; assign a UnitBlueprintAsset or shrink the array.",
                        this);
                    continue;
                }

                units.Add(produces[i].Definition);

                // AYNI SATIRDA, AYNI ATLAMAYLA: bir varlık bu listeye ancak
                // tanımı bir üstteki satırda kabul edildiyse giriyor. İki liste
                // ayrı döngülerde dolsaydı yukarıdaki `continue` yalnız birine
                // uygulanır ve indisler sessizce kayardı.
                assets.Add(produces[i]);
            }

            // KELEPÇE KOPYA DEĞİL, ÇEVİRİ: çekirdekteki kurucu aralık dışı indisi
            // bir istisnayla reddediyor ve o red DOĞRU. Ama tasarımcının bir dizi
            // gözünü boşaltması indisi sessizce aralık dışına düşürebilir, ve o
            // durumda oyunun hiç açılmaması bir tasarım hatasının cezası olarak
            // fazla ağır. Burada indis kırpılıyor ve kırpma BAĞIRIYOR.
            int index = defaultProducedIndex;
            if (units.Count > 0 && index >= units.Count)
            {
                Debug.LogError(
                    $"[StructureBlueprintAsset] '{name}' has defaultProducedIndex {index} " +
                    $"but only {units.Count} produced unit(s). Falling back to 0.",
                    this);
                index = 0;
            }
            else if (units.Count == 0 && index != 0)
            {
                Debug.LogError(
                    $"[StructureBlueprintAsset] '{name}' produces nothing but carries " +
                    $"defaultProducedIndex {index}. Falling back to 0.",
                    this);
                index = 0;
            }

            var built = new StructureBlueprint(
                DisplayName,
                maxHealth,
                // MENZİL SIFIRSA PROFİL HİÇ KURULMUYOR — null geçiliyor. damage
                // alanı o durumda okunmuyor bile; "hasarı var ama menzili yok"
                // diye bir yapı türü YOKTUR, çünkü o birim hiçbir hücreye
                // ulaşamaz ve sessizce hiçbir işe yaramaz.
                attackRange < 1 ? null : new AttackProfile(damage, attackRange, attackCooldownSeconds),
                units,
                index,
                productionSeconds);

            // İKİ ALAN EN SONDA yazılıyor: yukarıdaki kurucu fırlatırsa geriye
            // yarım dolu bir önbellek kalmasın ve bir sonraki okuma kurulumu
            // baştan denesin.
            definition = built;
            producedAssets = assets.ToArray();
        }
    }
}
