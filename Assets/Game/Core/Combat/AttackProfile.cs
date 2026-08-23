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
    /// Neyi TUTMAZ: kimin saldırdığını, kime saldırıldığını, o anki bekleme
    /// süresini. Bunlar çağrı anına ya da birime ait; tanıma değil.
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
    public sealed class AttackProfile
    {
        // DOĞRULAMA KURUCUDA DURUR: profil HANGİ yoldan gelirse gelsin (kod,
        // test, gelecekteki bir yükleyici) geçersiz değer üretilemez. Tip
        // ScriptableObject'ten türeseydi doğrulama OnValidate'e kayar, yalnızca
        // Inspector'da çalışır ve koddan üretilen profil hiç sınanmazdı; asmdef'in
        // noEngineReferences sınırı da o gün düşerdi.
        // → AttackProfile.md#attackprofileint-damage-int-range
        // DERİN ANLATIM: Docs/deep/konular/02-assembly-duvari.md
        public AttackProfile(int damage, int range)
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

            Damage = damage;
            Range = range;
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
    }
}
