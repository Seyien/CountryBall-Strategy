using System;

namespace GridStrategy.Combat
{
    // ═══ ROL: VARLIK (Entity) ════════════════════════════════════════
    // kimlik : var — aynı Max ile kurulan iki Health aynı can değildir;
    //          her biri tek bir SAHİBE aittir (bir asker ya da bir yapı)
    // hafıza : var — current çağrılar arasında yaşar; aynı TakeDamage(3)
    //          ikinci kez farklı bir sonuç bırakır
    // Unity  : gerekmez — düz C#, noEngineReferences: true
    // karar  : vermez, uygular — formül DamageRules'a ait, burada yalnızca yazma var
    /// <summary>
    /// Bir sahibin can SAYISI. Sahibinin ne olduğunu bilmez ve bilmemelidir:
    /// aynı sınıf bir askerin de bir barakanın da canını tutar.
    /// Yalnızca sayıyı tutar ve değiştirir; can bittikten SONRA ne olacağını
    /// bilmez. Düşme, yıkılma, sahneden silinme, ödül ve animasyon kararları bu
    /// tipin dışında verilir — böylece can kuralı Unity olmadan test edilebilir.
    ///
    /// Bu tipin hiçbir üyesi bir ALAN sözcüğü kullanmaz ("canlı", "ayakta",
    /// "sağlam"): sayı o yargıyı taşıyamaz. Alan yargısının sahipleri
    /// <see cref="Structure.IsStanding"/> ve <see cref="UnitState"/>'tir.
    /// </summary>
    public sealed class Health
    {
        private int current;

        public Health(int max)
        {
            if (max <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(max), max, "Max health must be positive.");
            }

            Max = max;
            current = max;
        }

        public int Max { get; }

        // REDDEDILEN - Health.cs:50 yerine:
        //     public int Current { get; set; }
        // KIRILAN  : sıfırın altına inmeme kuralını uygulamak ÇAĞIRANIN işi olur.
        //            "canı 3 yap" diyen çağıran -> DamageRules'un formülü atlanır
        //            negatif can yazılır        -> HasRemaining ters cevap verir
        //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızı olmaz
        // KAZANIRDI: ham durumu geri yazmak zorunda olan bir çağıran doğsaydı — kayıt,
        //            yükleme ya da senaryo kurulumu — ve kelepçe o yolun sahibinde dursaydı.
        // TEK CUMLE: Okunan üye ile yazılan üye aynı olmak zorunda değil; yazmak bir
        //            NİYET taşır, okumak taşımaz.
        public int Current => current;

        // Ad bilerek SAYIyı anlatır, sahibi değil. "Alive" bir alan sözcüğüdür ve
        // bu tipin bildiği tek şey olan sayı onu taşıyamaz: bir baraka canlı
        // değildir ama canı KALMIŞTIR. Alan yargısı, alanı bilen sahibe ait —
        // yapı tarafında Structure.IsStanding, birim tarafında UnitState.
        //
        // REDDEDILEN - Health.cs:70 yerine:
        //     public bool IsIntact => current > 0;
        // KIRILAN  : ad, bu tipin hiç ölçmediği bir BÜTÜNLÜK iddiasında bulunur.
        //            100 candan 1'e düşen baraka -> IsIntact hâlâ true der
        //            çağıran "hasar görmemiş" okur -> onarım sırasını buna göre kurar
        //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
        // KAZANIRDI: Health gerçekten bütünlük taşısaydı — tam can ile eksik can
        //            arasında ayrı bir davranış, bir hasar eşiği ya da bir çatlak
        //            durumu olsaydı; o gün ad `current == Max` diye okunurdu.
        // TEK CUMLE: Bir sayı ALAN yargısı taşıyamaz: "sağlam" binanın sözcüğüdür,
        //            "canlı" askerin, ikisini de bilmeyen bir sayaç ikisini de diyemez.
        //
        // Alternatif: IsDepleted. Seçilmedi: olumlu soran her çağırana bir "!" borçlandırır, okuma çift olumsuza döner.
        public bool HasRemaining => current > 0;

        // Hasarın TEK giriş noktası. Bilerek "Current" için bir setter yok:
        // "canı 3 yap" ile "3 hasar al" farklı niyetlerdir ve yalnızca ikincisi
        // sıfırın altına inmeme kuralını taşımak zorundadır. Setter olsaydı
        // çağıran o kuralı atlayabilirdi ve kod yine derlenirdi.
        public void TakeDamage(int amount)
        {
            // Girdi doğrulaması bilerek burada DEĞİL: formül ile onun geçerli
            // girdi aralığı aynı sahibe aittir (DamageRules). İki yerde kontrol
            // etseydik kural iki yerde yaşardı ve biri değişince diğeri sessizce
            // eskiyebilirdi. Bu tip sapmalar derleme hatası vermez.
            // REDDEDILEN - Health.cs:97 yerine:
            //     if (amount < 0)
            //     {
            //         throw new ArgumentOutOfRangeException(nameof(amount), amount, "Damage amount cannot be negative.");
            //     }
            //
            //     current = DamageRules.ResolveRemaining(current, amount);
            // KIRILAN  : aynı ön koşul iki dosyada yaşamaya başlar.
            //            zırh gelir, "negatif hasar" anlam kazanır -> kural genişler
            //            buradaki kopya sessizce eskir -> Health kuraldan farklı davranır
            //            derleyici: hiçbir şey der  .  test: DamageRulesTests yeşil kalır
            // KAZANIRDI: Health'in kendi giriş noktasında kuralınkinden daha DAR bir ön
            //            koşul gerekseydi — kural negatifi hoş görürken bu yol görmeseydi.
            // TEK CUMLE: Bir kuralın metni ile o kuralın geçerli girdi aralığı aynı
            //            sahibindir; ayırırsan ikisi ayrı hızda eskir.
            current = DamageRules.ResolveRemaining(current, amount);
        }

        // TakeDamage'in aynadaki eşi. Aynı desen: formül dışarıda, yazma burada.
        // Girdi doğrulaması yine burada DEĞİL — o da kuralın sahibine ait
        // (HealingRules). Bu metot da sahibinin ne olduğunu bilmez: onarım ile
        // diriltme arasındaki farkı Structure ve Combatant ayırır, burası yalnızca
        // sayıyı yukarı yazar.
        public void Heal(int amount)
        {
            current = HealingRules.ResolveRestored(current, Max, amount);
        }
    }
}
