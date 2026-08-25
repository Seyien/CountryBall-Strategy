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

        /// <summary>Sol paneldeki düğmenin yazısı.</summary>
        public string DisplayName =>
            string.IsNullOrWhiteSpace(displayName) ? name : displayName;

        /// <summary>Panel düğmesinin simgesi; atanmamışsa <c>null</c>.</summary>
        public Sprite Icon => icon;

        /// <summary>
        /// Bu varlığın düz C# karşılığı. Her okumada AYNI nesne döner — bu
        /// tanımdan konmuş bütün barakalar tek bir tanımı paylaşır.
        /// </summary>
        public StructureBlueprint Definition
        {
            get
            {
                if (definition == null)
                {
                    definition = Build();
                }

                return definition;
            }
        }

        private void OnValidate()
        {
            definition = null;
        }

        /// <summary>
        /// Inspector'daki sayılardan düz C# tanımını kurar; kuramadığını
        /// konsola bildirir.
        /// </summary>
        private StructureBlueprint Build()
        {
            var units = new List<UnitBlueprint>(produces.Length);
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

            return new StructureBlueprint(
                DisplayName,
                maxHealth,
                // MENZİL SIFIRSA PROFİL HİÇ KURULMUYOR — null geçiliyor. damage
                // alanı o durumda okunmuyor bile; "hasarı var ama menzili yok"
                // diye bir yapı türü YOKTUR, çünkü o birim hiçbir hücreye
                // ulaşamaz ve sessizce hiçbir işe yaramaz.
                attackRange < 1 ? null : new AttackProfile(damage, attackRange),
                units,
                index,
                productionSeconds);
        }
    }
}
