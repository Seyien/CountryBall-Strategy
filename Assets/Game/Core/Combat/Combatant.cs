using System;

namespace GridStrategy.Combat
{
    // ═══ ROL: BİLEŞİK (Aggregate) ════════════════════════════════════
    // kimlik : var — ölçüsü şu: ayrı Health ve ayrı UnitLifecycle ile İKİ
    //          Combatant kur, yalnız birine TakeDamage uygula; ötekinin
    //          CurrentHealth'i de State'i de kımıldamaz
    // hafıza : var — ölçüsü şu: Health(10) ile kurulan bir savaşçıya
    //          TakeDamage(10)'u arka arkaya İKİ kez uygula, iki FARKLI şey
    //          olur. Birincisi canı 0'a indirir VE State'i Alive'dan Downed'a
    //          çevirir; ikincisi hiçbir şeyi kımıldatmaz — can zaten 0'da
    //          kelepçeli, UnitLifecycle.OnHealthDepleted da Alive değilse
    //          erken döner
    // Unity  : gerekmez — parçalarının hiçbiri motora bağlı değil
    // karar  : parçalar ARASINDAKİ kuralı yürütür; parçaların kendi
    //          kurallarına karışmaz — Team bu satırı DEĞİŞTİRMEDİ: taraf
    //          taşınan bir DEĞERdir, yürütülen bir kural değil. "Aynı takıma
    //          saldırılmaz" hâlâ TargetingRules'ın; bu dosyada Team'i SORAN
    //          tek bir if bile yok
    /// <summary>
    /// Bir savaşçının bütünü: canı, yaşam döngüsü ve saldırı tanımı bir arada.
    ///
    /// Var olma sebebi tek cümleyle: <see cref="Health"/> canın bittiğini bilir
    /// ama <see cref="UnitLifecycle"/>'ı tanımaz; UnitLifecycle düşmeyi bilir ama
    /// canı tanımaz. İkisini birden tanıyan tek yer burasıdır — ve aralarındaki
    /// kuralı yürütmek başka hiçbir tipin işi değildir.
    ///
    /// Parçaların kendi kurallarına KARIŞMAZ: hasar formülünü DamageRules,
    /// menzili AttackResolver, geri sayımı UnitLifecycle sahiplenir. Buradaki
    /// tek kural "ne zaman hangisine haber verilir".
    ///
    /// GEREKÇELER: Docs/deep/kod/Core/Combat/Combatant.md
    /// </summary>
    public sealed class Combatant
    {
        // SABİT SAYININ ANLAMI HER BİRİMDE DEĞİŞİR, ORANINKİ DEĞİŞMEZ. Dirilen
        // birim TAM canla kalkmaz ve pay ORAN olarak yazılı: sabit 50 can,
        // maksimumu 40 olan birimde tam iyileşme, maksimumu 400 olanda hiç
        // demektir — tek satır, üç birimde üç ayrı kural.
        // → Combatant.md#revivehealthdivisor
        public const int ReviveHealthDivisor = 2;

        // DÜŞME CANI, BİTİRMEYİ BİR İŞ HÂLİNE GETİRİR. Düşmüş bedene vurmak onu
        // anında öldürseydi bir okçunun tek çentiği kurtarma penceresini
        // kapatırdı; bu bölen o pencereyi bir YARIŞA çeviriyor — dostu kaldırmaya
        // koşarken düşman bitirmeye koşuyor.
        // SAYI DİRİLTMENİN İKİZİ, ve bu bir tesadüf değil: bir kaldırma canın
        // yarısını geri veriyorsa, bitirme de o kadar can istemeli. İki bölen
        // ayrışsaydı "kaldırılabilir mi" ile "bitirilebilir mi" soruları sessizce
        // farklı cevaplar verirdi.
        // EŞİK: düşme canı tür başına ayarlanmak istendiği gün UnitBlueprint'e
        // taşınır; bugün hiçbir kural onu türe göre farklılaştırmıyor.
        public const int DownedHealthDivisor = 2;

        private readonly Health health;
        private readonly UnitLifecycle lifecycle;

        // ÖNCEKİ DURUM BURADA HATIRLANIYOR: UnitLifecycle.StateChanged yalnız
        // YENİ durumu taşır. Yeni durumdan TÜRETMEK reddedildi — türetme kuralı
        // sahibindeki geçiş tablosunun tersten yazılmış kopyası olur ve dördüncü
        // durum eklendiği gün yalan söyler.
        // → Combatant.md#lastobservedstate
        private UnitState lastObservedState;

        // SAYAÇ TANIMDA DEĞİL ÖRNEKTE YAŞIYOR, ve ölçüsü şu: aynı AttackProfile
        // ile İKİ savaşçı kur, yalnız birine vurdur; ötekinin bu alanı 0'da
        // kalır. Alan AttackProfile'a konsaydı tanımı paylaşan yüz okçu tek bir
        // bekleme sırasına girer, biri vurunca hepsi susardı. Aynı ayrımın
        // ikizi StructureProduction'da yazılı: eşik tanımda, sayaç örnekte.
        // DÜŞME CANI DÜŞÜNCE DOĞUYOR, kurucuda değil: ayakta duran bir birimin
        // taşıması gereken hiçbir şey yok ve erken kurulan havuz "kaç kere
        // düştü" sorusunu da sessizce yanlış cevaplardı. Diriltme onu bırakıyor,
        // yani ikinci düşüş yeni bir pencere açıyor.
        private Health downedHealth;

        private float attackCooldownRemaining;

        // VARSAYILAN, SORUNUN BİRİMDEN BAĞIMSIZ CEVABI VARSA KONUR. "Kaç can",
        // "kaç saniye", "kaç hasar" sorularının böyle bir cevabı yok; takımınki
        // Team.cs'te zaten verilmiş — sıfır BİLEREK tarafsız. Zorunlu olsaydı her
        // yeni tip doğduğu an bir taraf seçmek zorunda kalırdı.
        // → Combatant.md#combatant
        public Combatant(
            Health health,
            UnitLifecycle lifecycle,
            AttackProfile attackProfile,
            Team team = Team.None)
        {
            // PARÇAYI DIŞARIDAN ALMAK, KİMLİK HAKKINI DIŞARIDA BIRAKIR. Profil
            // içeride kurulsaydı 200 okçu tek TANIMI paylaşamaz, 200 ayrı nesne
            // doğardı. Hangi parçanın paylaşılabileceğine bu tip karar vermez;
            // o kararı parçanın kendi değişmezliği verir.
            // → Combatant.md#combatant
            this.health = health ?? throw new ArgumentNullException(nameof(health));
            this.lifecycle = lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));
            AttackProfile = attackProfile ?? throw new ArgumentNullException(nameof(attackProfile));

            // Doğrulama YOK: Team'in her değeri geçerli bir taraftır, Team.None
            // dahil. Buraya bir aralık kontrolü koymak, Team.cs'teki değer
            // listesini C# dışı bir yerde ikinci kez yazmak olurdu.
            // → Combatant.md#team
            Team = team;

            // ABONELİK KURUCUNUN EN SONUNDA — bütün doğrulamalardan SONRA: null
            // kontrollerinden biri patlarsa geriye abone olunmuş bir
            // UnitLifecycle kalmamalı. Aboneliğin çözüldüğü yer yok ve bu ihmal
            // değil; bu tip lifecycle'ının SAHİBİ, abonelik sınır geçmiyor.
            // → Combatant.md#combatant
            // ÖDÜNÇ ALINAN — `+=`: bir kayıt işlemidir ve kaydedilen şey `this`,
            // yani kurulmayı henüz bitirmemiş nesnenin kendisi; üstteki "en sonda"
            // kararı tam olarak bu mekanizmanın üstüne kurulu.
            // DİL: Docs/deep/dil/06-delege-arka-taraf.md
            lastObservedState = this.lifecycle.State;
            this.lifecycle.StateChanged += OnLifecycleStateChanged;
        }

        // OLAY KENDİNİ TAŞIRSA, EŞLEŞMEYE İKİNCİ BİR SAHİP DOĞAR. İmza geçişi
        // taşır, KİMLİĞİ taşımaz: bu tip kendi Unit'ini bilmez, kimliği ekleyen
        // halka Battle'dır. İç olayı add/remove ile dışarı vermek de reddedildi —
        // aktarım, dış dinleyicinin bağını iç parçaya düşürür ve o bağ kopmaz.
        // → Combatant.md#statechanged

        /// <summary>
        /// Bu savaşçının durumu her DEĞİŞTİĞİNDE tetiklenir ve geçişi
        /// <b>nereden nereye</b> olarak taşır.
        ///
        /// Var olma sebebi <see cref="UnitLifecycle.StateChanged"/>'in eksiğidir:
        /// o olay yalnızca YENİ durumu bildiriyor. KİMLİK TAŞIMAZ ve bu
        /// bilinçlidir — bu tip kendi <see cref="GridStrategy.Core.Unit"/>'ini
        /// bilmez; kimliği ekleyen halka bir üst katmandadır.
        /// </summary>
        // DERİN ANLATIM: Docs/deep/konular/01-olay-zinciri.md — dört durak (sayaç ->
        // savaşçı -> kayıt memuru -> çevirmen), hangi aboneliğin NEDEN sözlük
        // gerektirdiği ve sökülmezse önce neyin patladığı orada hikâye olarak.
        public event Action<UnitState, UnitState> StateChanged;

        /// <summary>
        /// İç parçanın tek değerli olayını iki değerli hâle çevirir. Ayrı bir
        /// metot olması kasıtlı: kurucuda yazılmış bir lambda, aboneliği
        /// çözmenin mümkün olduğu tek yeri de yok ederdi.
        /// </summary>
        private void OnLifecycleStateChanged(UnitState next)
        {
            UnitState previous = lastObservedState;

            // Önce hatırla, SONRA yay. Tersi olsaydı dinleyicinin olay içinde
            // yaptığı bir çağrı (diriltme, hasar) ikinci bir geçiş doğurabilir
            // ve o geçiş "önceki durum" olarak hâlâ eskisini görürdü.
            // → Combatant.md#onlifecyclestatechangedunitstate-next
            lastObservedState = next;
            StateChanged?.Invoke(previous, next);
        }

        public AttackProfile AttackProfile { get; }

        /// <summary>
        /// Bir sonraki vuruşa kalan saniye; vurmaya hazırken 0.
        /// </summary>
        // OYUNDA NE İŞE YARAR: ekranın bir gün çizeceği "yeniden dolduruyor"
        // çubuğunun okuyacağı sayı burası. Bugün hiçbir çizen yok ve üye yine
        // de açık — sebebi RemainingSeconds ile aynı: bir sayaç görünmezse
        // oyuncu neden vuramadığını ancak deneyerek öğrenir.
        public float AttackCooldownRemaining => attackCooldownRemaining;

        /// <summary>
        /// Bu savaşçının bekleme süresi doldu mu. Savaşçının HÂLİNE bakmaz —
        /// düşmüş bir birim de "beklemesi bitmiş" olabilir ve o ayrı soruyu
        /// <see cref="AttackRules"/> cevaplar.
        /// </summary>
        // TEK KAYNAĞA İNMEK DOĞRUDUR, DOĞRU KAYNAĞA İNMEK ŞARTTIR — aynı cümle
        // StructureProduction'ın IsReady üyesinin üstünde de yazılı ve buradaki
        // karşılığı şu: buraya bir "&& State == UnitState.Alive" eklenseydi
        // "düşmüş birim vuramaz" kuralının İKİ sahibi olurdu ve ret sebebi
        // RejectedActorCannotAct yerine sessizce RejectedOnCooldown'a düşerdi —
        // oyuncu cesedin beklemesini bitirmesini beklerdi.
        public bool IsAttackReady => attackCooldownRemaining <= 0f;

        /// <summary>
        /// Bir vuruşu HARCAR: bekleme dolmuşsa sayacı baştan başlatır ve
        /// <c>true</c> döner, dolmamışsa hiçbir şeye dokunmadan <c>false</c>.
        /// </summary>
        /// <returns>Vuruş hakkı bu çağrıyla alındıysa true.</returns>
        // TEK GİRİŞ NOKTASI, İKİ ADIMLIK SÖZLEŞME DEĞİL — gerekçesi
        // StructureProduction'ın Produce üyesinde ölçülerek yazılı ve burada
        // uygulanıyor: "önce IsAttackReady'yi sor, sonra sayacı başlat" deseninde
        // ikinci adımı unutan tek çağıran sınırsız hasar verirdi ve hiçbir test
        // kırmızıya dönmezdi, çünkü unutulan adımın kendi testi olmaz.
        // SAYACIN UZUNLUĞU TANIMDAN OKUNUYOR, burada bir sabit yok: iki okçu
        // farklı hızda vurabilsin diye sayı AttackProfile'da yaşıyor.
        public bool TryBeginAttackCooldown()
        {
            if (attackCooldownRemaining > 0f)
            {
                return false;
            }

            attackCooldownRemaining = AttackProfile.CooldownSeconds;
            return true;
        }

        /// <summary>
        /// Bu savaşçının tarafı.
        ///
        /// Neden <see cref="GridStrategy.Core.Unit"/>'te değil de burada: tarafı
        /// soran bütün sorular ("kime vurulur", "kim diriltilir") bu ad alanında
        /// soruluyor. Unit'e konsaydı <c>GridStrategy.Core</c> ad alanı
        /// <c>GridStrategy.Combat</c>'i tanımak zorunda kalırdı.
        /// </summary>
        // CEVABI İKİ ÇAĞRI ARASINDA DEĞİŞEBİLEN SORU, KURAL TAŞIYAMAZ. Taraf
        // kurulurken belli olur ve DEĞİŞMEZ: `set` eklenseydi onay ile hasarın
        // uygulanması arasına giren tek atama dost ateşini açardı. `private set`
        // yetmez — o satırı bu tipin kendi metodu da yazabilirdi.
        // → Combatant.md#team
        public Team Team { get; }

        // ÜÇ DEĞERLİ GERÇEĞİ İKİ KUTUYA BÖLEN KISAYOL, BİRİNİ YUTAR. Durumu
        // soran TEK üye bu; yanına `IsAlive` gibi bir bool eklemek reddedildi —
        // düşmüş birim "canlı" değildir ama hâlâ vurulabilir ve diriltilebilir.
        // Health.HasRemaining da bu boşluğu dolduramaz: o SAYIyı söyler.
        // → Combatant.md#state
        public UnitState State => lifecycle.State;

        public int CurrentHealth => health.Current;

        /// <summary>
        /// Bu savaşçının tam can değeri. Ekranın can barını çizebilmesi için
        /// gerekli: "17 can" tek başına bir oran vermez, oyuncu 17'nin çok mu az
        /// mı olduğunu ancak tavanı bilirse anlar.
        /// </summary>
        public int MaxHealth => health.Max;

        public float RemainingSeconds => lifecycle.RemainingSeconds;

        public bool IsReadyForCleanup => lifecycle.IsReadyForCleanup;

        /// <summary>
        /// Hasar uygular ve gerekiyorsa yaşam döngüsüne haber verir.
        ///
        /// Kontrol her KAREde değil, her HASAR OLAYINDA yapılır ve zaten bu
        /// metodun içindeyiz: "canı bitti mi" sorusunun maliyeti bir bool
        /// okuması. Ayrı bir dinleme mekanizması bugün buna hiçbir şey katmazdı.
        /// </summary>
        public void TakeDamage(int amount)
        {
            // SESSİZ BİR HAYIR, KURALI İKİNCİ BİR EVE TAŞIR. Bu satırın üstüne
            // `if (!health.HasRemaining) return;` eklenmesi reddedildi: "kime
            // vurulur" sorusunun sahibi TargetingRules ve bu metot `void` döner —
            // reddedilen hasar çağırana söylenemezdi.
            // → Combatant.md#takedamageint-amount
            // DÜŞMÜŞ BEDEN AYRI BİR HAVUZDAN EKSİLİYOR. Aynı Health'e yazmak
            // ölçüldü ve işe yaramıyor: can zaten sıfırda, DamageRules onu
            // sıfırda tutuyor, yani vuruş hiçbir şeyi değiştirmiyordu — oyuncu
            // bekleme süresini harcıyor, "isabet" cümlesini okuyor ve yerde
            // yatan düşman kılını kıpırdatmıyordu.
            if (lifecycle.State == UnitState.Downed)
            {
                downedHealth = downedHealth ?? new Health(DownedHealthPoolFor(health.Max));
                downedHealth.TakeDamage(amount);

                if (!downedHealth.HasRemaining)
                {
                    lifecycle.OnDownedHealthDepleted();
                }

                return;
            }

            health.TakeDamage(amount);

            // Soru SAYIya soruluyor, alana değil: "canı kaldı mı". Alan cevabını
            // (Alive / Downed / Dead) bu satırdan sonra UnitLifecycle verir.
            // → Combatant.md#takedamageint-amount
            if (!health.HasRemaining)
            {
                lifecycle.OnHealthDepleted();
            }
        }

        /// <summary>
        /// Düşmüş savaşçıyı ayağa kaldırır. Tam canla değil, maksimumun bir
        /// kesriyle — diriltmek ölümü geri almak değil, riskli bir yatırımdır.
        /// </summary>
        /// <returns>Diriltme gerçekleştiyse true.</returns>
        public bool TryRevive()
        {
            if (!lifecycle.TryRevive())
            {
                return false;
            }

            // Can sıfırdayken iyileştirdiğimiz için sonuç doğrudan payı verir;
            // HealingRules'ın üst kelepçesi maksimumu aşmayı zaten engelliyor.
            // → Combatant.md#tryrevive
            health.Heal(health.Max / ReviveHealthDivisor);

            // Kaldırılan beden düşme canını da BIRAKIYOR: bırakılmasaydı ikinci
            // kez düşen bir savaşçı, ilk düşüşünde yediği çentiklerle doğar ve
            // tek vuruşta bitirilirdi.
            downedHealth = null;
            return true;
        }

        /// <summary>
        /// Zamanı bu savaşçının İKİ sayacına birden iletir: yaşam döngüsünün
        /// geri sayımına ve bir sonraki vuruşun beklemesine.
        /// </summary>
        // İKİNCİ BİR TİK YOLU AÇILMADI — ve bu kararın ölçüsü Battle.Tick'te:
        // orada savaşçıları gezen TEK bir döngü var ve o döngü bu metodu
        // çağırıyor. Bekleme için ayrı bir TickAttackCooldown yazılsaydı,
        // Battle'ın "İKİNCİ DÖNGÜ, TEK ÇAĞRI" notunun altını oyar ve çağıranı
        // ikisini birden çağırmakla yükümlü kılardı; birini unutan gün saldırı
        // sonsuza dek beklemede kalırdı — hiçbir test kırmızı olmadan.
        // SIRA BİR KARARDIR: önce yaşam döngüsü, sonra bekleme. Geriye akan
        // zamanın kelepçesi UnitLifecycle.Tick'in ilk satırında duruyor ve
        // koşulsuz atıyor, yani bu sıra o kelepçeyi aşağıdaki çıkarmaya da
        // BEDAVA uyguluyor; tersi olsaydı negatif bir delta beklemeyi UZATIR ve
        // istisna ancak ondan sonra atılırdı.
        public void Tick(float deltaSeconds)
        {
            lifecycle.Tick(deltaSeconds);

            // Hazırken geri sayım yok; erken çıkış burada başarım için değil
            // DOĞRULUK için — aşağıdaki çıkarma sıfırı eksiye götürür ve
            // AttackCooldownRemaining ekranda negatif bir sayı gösterirdi.
            if (attackCooldownRemaining <= 0f)
            {
                return;
            }

            attackCooldownRemaining -= deltaSeconds;
            if (attackCooldownRemaining < 0f)
            {
                // Sayaç sıfırda TUTULUYOR, eksiye kaymıyor: aynı kelepçe
                // StructureProduction'ın Tick üyesinde de var ve gerekçesi orada
                // yazılı — eksiye kayan bir sayaç sonraki vuruşun beklemesini
                // sessizce KISALTIRDI.
                attackCooldownRemaining = 0f;
            }
        }

        /// <summary>
        /// Düşmüş bedenin ne kadar dayandığını verir; en az bir vuruşluk.
        /// </summary>
        // TABAN BİR, ÇÜNKÜ SIFIR HAVUZ BİTİRMEYİ GERİ ANINDA YAPARDI: canı 1 olan
        // bir birim için tam sayı bölmesi 0 verir ve o birim düştüğü karede
        // bitirilebilir hâle gelirdi — düzeltmenin adı da kuralın kendisi.
        private static int DownedHealthPoolFor(int maxHealth)
        {
            int pool = maxHealth / DownedHealthDivisor;
            return pool < 1 ? 1 : pool;
        }

    }
}
