using System;

namespace GridStrategy.Combat
{
    // ═══ ROL: TANIM (Profile) ════════════════════════════════════════
    // kimlik : yok — ölçüsü şu: bir birimin profilini aynı sayıları taşıyan
    //          başka bir (10 hasar, 1 menzil) örneğiyle değiştir; hiçbir
    //          çağıranın cevabı değişmez. Ölçü `==` DEĞİL — Equals
    //          yazılmadığı için o karşılaştırma false döner; ölçü YERİNE
    //          GEÇEBİLİRLİK — bu yüzden yüzlerce asker tek bir örneği
    //          paylaşabilir
    // hafıza : yok — değerler kurucuda donar, Damage her okumada aynı
    // Unity  : gerekmez — bugün düz C# nesnesi; ScriptableObject kararı
    //          geldiğinde bu satır değişir, rol değişmez
    // karar  : vermez — sayıyı taşır; "menzile giriyor mu" sorusunu AttackResolver cevaplar
    /// <summary>
    /// Bir saldırı türünün değişmez tanımı: "kılıç 10 hasar, 1 hücre menzil".
    /// TANIM'dır, varlık değildir — aynı değerlere sahip iki AttackProfile
    /// birbirinin YERİNE GEÇEBİLİR ve yüzlerce asker tek bir örneği
    /// paylaşabilir.
    ///
    /// Bu yüzden hiçbir alanı sonradan DEĞİŞMEZ: değişebilseydi, onu paylaşan
    /// her birim habersiz etkilenirdi.
    ///
    /// Neyi TUTMAZ: kimin saldırdığını, kime saldırıldığını, bir sonraki
    /// vuruşa KALAN süreyi. Sonuncusu önemli: bekleme süresi burada bir
    /// EŞİKtir, sayaç değil — sayacı tutan yer <see cref="Combatant"/> ile
    /// <see cref="Structure"/>, çünkü aynı tanımı paylaşan iki okçu ayrı ayrı
    /// bekler. Aynı ayrım <see cref="StructureBlueprint.ProductionSeconds"/>
    /// ile <see cref="StructureProduction"/> arasında da yazılı.
    ///
    /// GEREKÇELER: Docs/deep/kod/Core/Combat/AttackProfile.md
    /// </summary>
    // TANIM PAYLAŞILIR, KOPYALANMAZ: `sealed class` olduğu için yüzlerce asker
    // TEK örneğe ok tutar. `readonly struct` olsaydı her alan okumasında ve her
    // parametre geçişinde yeni bir kopya doğar, "tek profili paylaşır" cümlesi
    // sessizce yalan olur ve `null` hâli kalmadığı için AttackResolver'daki null
    // koruması anlamsızlaşırdı.
    // → AttackProfile.md#attackprofile-tip
    // ÖĞRENME: Docs/ogrenme/02-sonraki-asamalar.md — yukarıdaki "paylaşabilir" bir
    // İMKÂNdır, bugünkü kullanım değil: üretimde profil kuran TEK yer BoardAdapter
    // içindeki NewCombatant ve her birime yeni bir örnek veriyor, paylaşım sıfır.
    // Paylaşımı gerçeğe çeviren aşama ScriptableObject; tetikleyici koşulu orada.
    // ═══ record, class DEĞİL ═══════════════════════════════════════════
    // Künyedeki "Ölçü `==` DEĞİL — Equals yazılmadığı için o karşılaştırma false
    // döner" satırı bir İTİRAFtı: tip değer semantiği vaat ediyor ama
    // uygulamıyordu. `record` bunu derleyiciye yaptırıyor; (10 hasar, 1 menzil)
    // olan iki profil artık gerçekten eşit. Üçüncü sayı eklenirken bu cümleyi
    // GENİŞLETMEK gerekmedi: derleyicinin ürettiği eşitlik bütün alanları
    // gezdiği için bekleme süresi farkı iki profili kendiliğinden ayırdı — elle
    // yazılmış bir Equals olsaydı yeni alan orada unutulur ve hızlı vuran okçu
    // ile yavaş vuran okçu sessizce aynı tanım sayılırdı.
    //
    // İkizi MoveProfile ile aynı karar, aynı gerekçe — ayrıntı orada yazılı.
    // Paylaşım cümlesi de bozulmuyor: record bir SINIFtır, hâlâ tek örneğe ok
    // tutulur ve null olabilir, yani AttackResolver'daki null koruması anlamlı
    // kalır. `readonly struct` seçilseydi ikisi de düşerdi.
    public sealed record AttackProfile
    {
        // DOĞRULAMA KURUCUDA DURUR: profil HANGİ yoldan gelirse gelsin (kod,
        // test, gelecekteki bir yükleyici) geçersiz değer üretilemez. Tip
        // ScriptableObject'ten türeseydi doğrulama OnValidate'e kayar, yalnızca
        // Inspector'da çalışır ve koddan üretilen profil hiç sınanmazdı; asmdef'in
        // noEngineReferences sınırı da o gün düşerdi.
        // → AttackProfile.md#attackprofileint-damage-int-range
        // DERİN ANLATIM: Docs/deep/konular/02-assembly-duvari.md
        // ÜÇÜNCÜ SAYI VARSAYILANLI GELDİ, ZORUNLU DEĞİL: bugünkü yirmi küsur
        // çağrının hepsi iki argümanla yazılmış ve hepsinin bugünkü anlamı
        // "bekleme yok". Zorunlu yazılsaydı her çağıran kendi sıfırını
        // uydurmak zorunda kalır, kural yirmi yerde tekrarlanırdı; varsayılan
        // ise kuralı TEK yerde, imzada tutuyor.
        /// <param name="cooldownSeconds">
        /// İki vuruş arasındaki bekleme. 0 geçerlidir ve "sınırsız" demektir —
        /// eşik <see cref="StructureBlueprint.ProductionSeconds"/> ile aynı
        /// gerekçeyle gevşetildi, sıfırın burada da bir adı var.
        /// </param>
        public AttackProfile(int damage, int range, float cooldownSeconds = 0f)
        {
            if (damage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damage), damage, "Damage cannot be negative.");
            }

            // Menzil en az 1: sıfır menzilli bir saldırı hiçbir hücreye
            // ulaşamazdı ve sessizce hiçbir işe yaramayan bir birim üretirdi.
            // → AttackProfile.md#menzil-en-az-1
            if (range < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(range), range, "Range must be at least 1.");
            }

            // NEGATİF BEKLEME REDDEDİLİYOR AMA SIFIR REDDEDİLMİYOR, ve bu
            // ayrım menzilinkinin tersi: sıfır menzil hiçbir hücreye ulaşmayan
            // bir birim üretirdi, sıfır bekleme ise oyunun BUGÜNKÜ davranışıdır
            // ve adı konmuş bir tanımdır. Eşiği 1'e çekseydik "sınırsız vuruş"
            // ikinci bir mekanizma olarak yeniden yazılmak zorunda kalırdı.
            if (cooldownSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cooldownSeconds), cooldownSeconds, "Attack cooldown cannot be negative.");
            }

            Damage = damage;
            Range = range;
            CooldownSeconds = cooldownSeconds;
        }

        /// <summary>Bir vuruşun ham hasarı. Zırh/direnç burada DEĞİL.</summary>
        // Zırh, direnç, kalkan ve kritik çarpanı DamageRules'un evinde; buraya
        // eklenseydi TANIM sessizce bir formüle dönerdi.
        // → AttackProfile.md#damage
        public int Damage { get; }

        /// <summary>Kaç hücre uzağa ulaşabildiği. Mesafenin nasıl ölçüldüğünü bilmez.</summary>
        // Yalnızca EŞİĞİ verir; karşılaştırmayı AttackResolver yapar. Kurucudaki
        // `range < 1` kelepçesi bu property'nin değişmezidir.
        // → AttackProfile.md#range
        public int Range { get; }

        /// <summary>
        /// İki vuruş arasında geçmesi gereken saniye; 0 ise bekleme yoktur.
        /// </summary>
        // OYUNDA NE İŞE YARAR: oyuncu fareye ne kadar hızlı basarsa bassın bu
        // sayı dolmadan ikinci vuruş inmez. Saldırının sırayı harcaması
        // kalktığından beri vurmanın başka hiçbir bedeli yoktu — tek bir seçim
        // açıkken aynı hedefe üst üste tıklamak hasarı katlıyordu; bedeli geri
        // koyan sayı bu.
        // SAYAÇ DEĞİL EŞİK, ve ayrımın ölçüsü şu: bu property iki okuma
        // arasında asla değişmez, oysa bir sonraki vuruşa kalan süre her
        // Tick'te değişir. Kalanı burada tutsaydık aynı tanımı paylaşan yüz
        // okçu tek bir bekleme sırasına girerdi — biri vurunca hepsi susardı.
        // → Combatant.AttackCooldownRemaining
        public float CooldownSeconds { get; }
    }
}
