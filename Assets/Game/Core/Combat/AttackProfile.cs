using System;

namespace GridStrategy.Combat
{
    // ═══ ROL: TANIM (Profile) ════════════════════════════════════════
    // kimlik : yok — (10 hasar, 1 menzil) olan iki nesne aynı şeydir;
    //          yüzlerce asker tek bir örneği paylaşabilir
    // hafıza : yok — değerler kurucuda donar, Damage her okumada aynı
    // Unity  : gerekmez — bugün düz C# nesnesi; ScriptableObject kararı
    //          geldiğinde bu satır değişir, rol değişmez
    // karar  : vermez — sayıyı taşır; "menzile giriyor mu" sorusunu AttackResolver cevaplar
    /// <summary>
    /// Bir saldırı türünün değişmez tanımı: "kılıç 10 hasar, 1 hücre menzil".
    /// TANIM'dır, varlık değildir — aynı değerlere sahip iki AttackProfile aynı
    /// şeydir ve yüzlerce asker tek bir örneği paylaşabilir.
    ///
    /// Bu yüzden hiçbir alanı sonradan DEĞİŞMEZ: bir profil oluşturulduktan
    /// sonra sabittir. Değişebilseydi, onu paylaşan her birim habersiz etkilenirdi.
    ///
    /// Neyi TUTMAZ: kimin saldırdığını, kime saldırıldığını, o anki bekleme
    /// süresini. Bunlar çağrı anına ya da birime ait; tanıma değil.
    /// </summary>
    public sealed class AttackProfile
    {
        public AttackProfile(int damage, int range)
        {
            if (damage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damage), damage, "Damage cannot be negative.");
            }

            // Menzil en az 1: sıfır menzilli bir saldırı hiçbir hücreye
            // ulaşamazdı ve sessizce hiçbir işe yaramayan bir birim üretirdi.
            if (range < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(range), range, "Range must be at least 1.");
            }

            Damage = damage;
            Range = range;
        }

        /// <summary>Bir vuruşun ham hasarı. Zırh/direnç burada DEĞİL.</summary>
        public int Damage { get; }

        /// <summary>Kaç hücre uzağa ulaşabildiği. Mesafenin nasıl ölçüldüğünü bilmez.</summary>
        public int Range { get; }
    }
}
