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

        private readonly Health health;
        private readonly UnitLifecycle lifecycle;

        // ÖNCEKİ DURUM BURADA HATIRLANIYOR: UnitLifecycle.StateChanged yalnız
        // YENİ durumu taşır. Yeni durumdan TÜRETMEK reddedildi — türetme kuralı
        // sahibindeki geçiş tablosunun tersten yazılmış kopyası olur ve dördüncü
        // durum eklendiği gün yalan söyler. Tipin tek yazılabilir üyesi.
        // → Combatant.md#lastobservedstate
        private UnitState lastObservedState;

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
            return true;
        }

        public void Tick(float deltaSeconds)
        {
            lifecycle.Tick(deltaSeconds);
        }
    }
}
