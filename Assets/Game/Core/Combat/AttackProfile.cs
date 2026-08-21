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
    // REDDEDILEN - AttackProfile.cs:35 yerine:
    //     public readonly struct AttackProfile
    // KIRILAN  : paylaşılan TEK örnek her çağrıda kopyaya döner — "yüzlerce asker
    //            tek profili paylaşır" cümlesi sessizce yalan olur.
    //            struct null olamaz -> AttackResolver'daki null koruması anlamsızlaşır
    //            derleyici: null atamayı der  .  test: IsWithinRange_NullProfile_Throws
    //            derlenmez ve o sözleşme sınanamaz kalır
    // KAZANIRDI: profil paylaşılmayıp birim başına saklansaydı ve her karede
    //            binlerce kez okunsaydı — iki int kopyalamak referans takibinden
    //            ucuz kalırdı.
    // TEK CUMLE: TANIM paylaşılır, kopyalanmaz; kopyalanan tanım artık TEK tanım
    //            değildir.
    public sealed class AttackProfile
    {
        // REDDEDILEN - AttackProfile.cs:52 yerine (kurucu silinir, tip
        //              ScriptableObject'ten türer):
        //     [SerializeField] private int damage;
        //     [SerializeField] private int range;
        //     private void OnValidate() { range = Mathf.Max(1, range); }
        // KIRILAN  : doğrulama OnValidate'e taşınır ve yalnızca Inspector'da çalışır;
        //            koddan üretilen profil hiç sınanmaz.
        //            asmdef'in noEngineReferences sınırı düşer -> saf kural motora bağlanır
        //            derleyici: asmdef sınırını der  .  test: AttackProfileTests'in
        //            Throws testleri derlenmez
        // KAZANIRDI: hasar ve menzil sayılarını programcı değil TASARIMCI
        //            ayarlayacaksa — asset olarak Inspector'dan, yeniden
        //            derlemeden dengelenirdi.
        // TEK CUMLE: Bir kuralı motora bağlamak onu sınanabilir olmaktan çıkarır;
        //            asmdef sınırı bu kararın yazılı hâlidir.
        public AttackProfile(int damage, int range)
        {
            if (damage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damage), damage, "Damage cannot be negative.");
            }

            // Menzil en az 1: sıfır menzilli bir saldırı hiçbir hücreye
            // ulaşamazdı ve sessizce hiçbir işe yaramayan bir birim üretirdi.
            // Alternatif: `if (range < 0)` — 0 menzil geçerli olurdu. Seçilmedi: sebebi hemen yukarıda; 0 ancak kendi hücresine uygulanan bir yetenek geldiği gün "sadece kendi hücrem" anlamını kazanır.
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
