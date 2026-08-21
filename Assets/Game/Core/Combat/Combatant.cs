using System;

namespace GridStrategy.Combat
{
    // ═══ ROL: BİLEŞİK (Aggregate) ════════════════════════════════════
    // kimlik : var — her savaşçı kendi Health ve kendi UnitLifecycle'ına sahip
    // hafıza : var — aynı TakeDamage(10) çağrısı her seferinde farklı sonuç
    // Unity  : gerekmez — parçalarının hiçbiri motora bağlı değil
    // karar  : parçalar ARASINDAKİ kuralı yürütür; parçaların kendi
    //          kurallarına karışmaz — Team bu satırı DEĞİŞTİRMEDİ: taraf
    //          taşınan bir DEĞERdir, yürütülen bir kural değil. "Aynı takıma
    //          saldırılmaz" hâlâ TargetingRules'ın; burada tek bir if bile yok
    //
    // NEDEN ADAPTÖR DEĞİL: adaptör iki farklı DİL arasında çeviri yapar
    // (BoardAdapter: Unity'nin Vector3'ü ↔ Core'un int x,y'si). Buradaki üç
    // parça da Core'a ait, aynı dili konuşuyor. Çevrilecek bir şey yok —
    // sahiplenilecek bir bütün var. Ayırt edici soru: "iki tarafın dili farklı
    // mı?" Hayırsa bileşiktir, evetse adaptör.
    /// <summary>
    /// Bir savaşçının bütünü: canı, yaşam döngüsü ve saldırı tanımı bir arada.
    ///
    /// Var olma sebebi tek bir cümleyle: <see cref="Health"/> canın bittiğini
    /// bilir ama <see cref="UnitLifecycle"/>'ı tanımaz; UnitLifecycle düşmeyi
    /// bilir ama canı tanımaz. İkisini birden tanıyan tek yer burasıdır — ve
    /// aralarındaki kuralı yürütmek başka hiçbir tipin işi değildir.
    ///
    /// Parçaların kendi kurallarına KARIŞMAZ: hasar formülünü DamageRules,
    /// menzili AttackResolver, geri sayımı UnitLifecycle sahiplenir. Buradaki
    /// tek kural "ne zaman hangisine haber verilir".
    /// </summary>
    public sealed class Combatant
    {
        // Dirilen birim TAM canla kalkmaz. Oran olarak yazılı, sabit sayı
        // olarak değil: sabit 50 can, maksimumu 40 olan bir birimde tam
        // iyileşme, maksimumu 400 olanda hiç anlamına gelirdi.
        //
        // REDDEDILEN - Combatant.cs:47 yerine:
        //     public const int ReviveHealthAmount = 50;
        // KIRILAN  : aynı sabit iki birimde iki ayrı kural olur.
        //            maksimumu 40 olan birim  -> 50 can "tam iyileşme" demek
        //            maksimumu 400 olan birim -> aynı 50 can "hiç" demek
        //            derleyici: hiçbir şey der  .  test: Revive_ScalesWithMaxHealth kırılır
        // KAZANIRDI: diriltmenin gücü birimden BAĞIMSIZ olsun isteniyorsa — tankları
        //            zayıflatıp ucuz birimleri güçlendiren bilinçli bir denge kararı.
        // TEK CUMLE: Sabit sayı birimden bağımsız görünür ama anlamı her birimde
        //            değişir; oran her birimde aynı şeyi söyler.
        public const int ReviveHealthDivisor = 2;

        private readonly Health health;
        private readonly UnitLifecycle lifecycle;

        // ÖNCEKİ DURUM BURADA HATIRLANIYOR, çünkü UnitLifecycle.StateChanged
        // yalnızca YENİ durumu taşıyor. "Nereden nereye" sorusunu cevaplamak
        // için bir yerin geçmişi tutması gerekiyor ve o yer, geçişi dışarı veren
        // taraf olmalı — dinleyicilerin her biri kendi kopyasını tutarsa aynı
        // hatırlama işi üç yerde doğar (UnitLifecycle.cs'te reddedilen
        // "her kare State'i oku ve karşılaştır" seçeneğinin ta kendisi).
        //
        // REDDEDILEN - Combatant.cs:79 yerine (alan hiç doğmaz; önceki durum
        //              YENİ durumdan TÜRETİLİR, çünkü her geçişin tek bir
        //              kaynağı var):
        //     private static UnitState PreviousOf(UnitState next)
        //     {
        //         // Downed'a yalnız Alive'dan, Dead'e yalnız Downed'dan,
        //         // Alive'a yalnız Downed'dan gelinir.
        //         return next == UnitState.Alive ? UnitState.Downed
        //             : next == UnitState.Downed ? UnitState.Alive
        //             : UnitState.Downed;
        //     }
        // KIRILAN  : geçiş tablosu, sahibinin dışında ve tersinden İKİNCİ kez yazılır.
        //            bugün üç geçişte de doğru cevap verir -> tam bu yüzden tehlikeli
        //            UnitLifecycle'a dördüncü durum girer -> burası yalan söylemeye başlar
        //            dinleyici "Alive'dan geldi" duyar, oysa birim düşmüştü
        //            derleyici: hiçbir şey der  .  test: hiçbiri kırılmadan yalan başlar
        // KAZANIRDI: geçiş tablosu gerçekten tek yönlü ve dallanmasız olsaydı — Alive →
        //            Dead, başka hiçbir şey — o gün "önceki durum" diye bir soru olmazdı.
        // TEK CUMLE: Türetilebilen her bilgi türetilmeli değildir; türetme kuralı
        //            tablonun KOPYASIysa tablo iki yerde yaşamaya başlar.
        private UnitState lastObservedState;

        // Takımın VARSAYILANI var, diğer üç parçanın yok — ve bu tutarsızlık
        // bilinçli. "Kaç can", "kaç saniye", "kaç hasar" sorularının birimden
        // bağımsız bir cevabı yoktur; takımın cevabı ise Team.cs'te zaten
        // verilmiş: sıfır BİLEREK tarafsız. Atlanan takım, uydurulmuş bir taraf
        // değil, açıkça tarafsızlık demektir.
        //
        // REDDEDILEN - Combatant.cs:103 yerine (varsayılan yok, zorunlu):
        //     public Combatant(Health health, UnitLifecycle lifecycle,
        //                      AttackProfile attackProfile, Team team)
        // KIRILAN  : takım, kurucunun DÖRDÜNCÜ zorunlu sorusu olur.
        //            Constructor_NullPart_Throws'un üç çağrısı da takım yazar
        //            o test null korumasını değil imzayı sınıyormuş gibi okunur
        //            yeni her tip — yıkılabilir yapı, tuzak — doğduğu an taraf seçer
        //            derleyici: hiçbir şey der  .  test: yeşil kalır, yalnız gürültülenir
        // KAZANIRDI: oyunda tarafsız hiçbir şey olmayacaksa — o gün varsayılan, takımı
        //            atanmayı unutulmuş birimi sessizce Team.None yapardı.
        // TEK CUMLE: Varsayılan ancak sorunun birimden bağımsız bir cevabı VARSA konur;
        //            "kaç can" sorusunun yok, "hangi taraf" sorusunun var.
        public Combatant(
            Health health,
            UnitLifecycle lifecycle,
            AttackProfile attackProfile,
            Team team = Team.None)
        {
            // REDDEDILEN - Combatant.cs:118 yerine:
            //     this.health = new Health(maxHealth);
            // KIRILAN  : parçayı içeride kurmak, TANIM rolündeki AttackProfile'ı
            //            VARLIK'a çevirir ve paylaşımı imkânsızlaştırır.
            //            200 okçu tek profili paylaşamaz -> 200 kopya doğar
            //            test gerçek parçayı değil kurucunun ürettiğini sınar
            //            derleyici: hiçbir şey der  .  test: yeşil kalır, ölçüsü kayar
            // KAZANIRDI: parça sayısı hiç artmayacaksa ve paylaşım hiç gerekmiyorsa —
            //            kurucu çağıranı üç nesne kurma külfetinden kurtarırdı.
            // TEK CUMLE: Parçalarını dışarıdan almak, o parçaların KİMLİĞİNE karar
            //            verme hakkını da dışarıda bırakmaktır.
            this.health = health ?? throw new ArgumentNullException(nameof(health));
            this.lifecycle = lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));
            AttackProfile = attackProfile ?? throw new ArgumentNullException(nameof(attackProfile));

            // Doğrulama YOK: Team'in her değeri geçerli bir taraftır, Team.None
            // dahil. Buraya bir aralık kontrolü koymak, Team.cs'teki değer
            // listesini C# dışı bir yerde ikinci kez yazmak olurdu.
            Team = team;

            // ABONELİK KURUCUNUN EN SONUNDA — bütün doğrulamalardan SONRA.
            // Sıra bir karardır: yukarıdaki üç null kontrolünden biri patlarsa
            // geriye abone olunmuş bir UnitLifecycle kalmamalı. Çağıran aynı
            // lifecycle örneğini ikinci bir Combatant'a verebilir (hiçbir şey
            // engellemiyor); yarım kalmış bir kurulumdan artan abonelik o gün
            // ölü bir nesneyi olayla birlikte hayatta tutar ve dinleyici aynı
            // geçişi iki kez duyar.
            //
            // Aboneliğin ÇÖZÜLDÜĞÜ bir yer yok ve bu bir ihmal değil: bu tip
            // kendi lifecycle'ının SAHİBİ. İkisi birlikte doğar, birlikte
            // çöpe gider — abonelik bir sahiplik sınırını GEÇMİYOR. Sınırı
            // geçen abonelik Battle'da (Combatant → Battle) ve orada bırakma
            // zorunlu; gerekçesi Battle.cs'te yazılı.
            // İkisi de `this.` ile yazılı: parametre alanı GÖLGELİYOR ve iki
            // satırın aynı nesneye baktığını okuyanın çıkarmak zorunda kalması
            // gereksiz bir yük.
            lastObservedState = this.lifecycle.State;
            this.lifecycle.StateChanged += OnLifecycleStateChanged;
        }

        // ═══ #7 — DURUM DEĞİŞİKLİĞİ ZİNCİRİNİN ORTA HALKASI ═══════════
        // UnitLifecycle.StateChanged  Action<UnitState>              hangi duruma
        // Combatant.StateChanged      Action<UnitState, UnitState>   nereden nereye
        // Battle.UnitStateChanged     Action<Unit, UnitState, ...>   KİM, nereden nereye
        //
        // Bu halka KİMLİK taşımaz ve taşıyamaz: bu tip kendi Unit'ini BİLMEZ.
        // Kimlik parçalarda değil, sözlükte yaşıyor — aynı gerekçe unitViews'ta
        // ve Battle.combatants'ta zaten iki kez yazılı. Kimliği ekleyen halka
        // Battle'dır, çünkü eşleşmenin tek sahibi odur.
        //
        // REDDEDILEN - Combatant.cs:210 yerine (olay kendisini de taşır ve
        //              Battle tek bir dinleyiciyle kurtulur):
        //     public event Action<Combatant, UnitState, UnitState> StateChanged;
        // KIRILAN  : Battle'ın elinde Combatant olur, Unit olmaz — kimliğe TERS arama gerekir.
        //            sözlüğü baştan tara -> her geçişte UnitCount kadar karşılaştırma
        //            Dictionary<Combatant, Unit> tut -> eşleşme İKİ sahipli olur
        //            aynı Combatant iki Unit'e kayıtlıysa hangisi olduğu hiç bilinmez
        //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
        // KARSILASTIRMA:
        //     Action<UnitState>             yalnız yeni durum -> "nereden" dinleyiciye kalır
        //     Action<Combatant, ...>        kendini taşır     -> Battle ters arama yapar
        //     Action<UnitState, UnitState>  geçişi taşır      -> kimliği üst halka ekler
        // KAZANIRDI: Unit ile Combatant TEK bir tipte birleşseydi — o gün "kendini
        //            taşımak" kimliği taşımakla aynı şey olurdu.
        // TEK CUMLE: Kimlik parçada değil sözlükte yaşar; kendini taşıyan bir olay
        //            kimlik taşıyor SANILIR ve eşleşmeye ikinci bir sahip doğurur.

        // REDDEDILEN - Combatant.cs:210 yerine (proxy hiç doğmaz; iç parçanın
        //              olayı olduğu gibi dışarı verilir):
        //     public event Action<UnitState> StateChanged
        //     {
        //         add { lifecycle.StateChanged += value; }
        //         remove { lifecycle.StateChanged -= value; }
        //     }
        // KIRILAN  : dış dinleyici doğrudan İÇ parçaya bağlanır ve bağ kopmaz.
        //            bu tip aradan çekilse bile lifecycle'a tutunan bağ kalır
        //            kapsülleme tek satırda biter -> parça artık gizli değil
        //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
        // KAZANIRDI: dinleyicilerin hiçbiri "nereden" sorusunu sormasaydı — o gün
        //            add/remove aktarımı hem daha kısa hem daha dürüst olurdu.
        // TEK CUMLE: Bir olayı olduğu gibi geçirmek onu SAHİPLENMEK değildir; proxy
        //            kapsüllemeyi delerken hiçbir şey eklemez.
        /// <summary>
        /// Bu savaşçının durumu her DEĞİŞTİĞİNDE tetiklenir ve geçişi
        /// <b>nereden nereye</b> olarak taşır.
        ///
        /// Var olma sebebi <see cref="UnitLifecycle.StateChanged"/>'in
        /// eksiğidir: o olay yalnızca YENİ durumu bildiriyor. "Nereden" sorusunu
        /// soran gerçek bir tüketici var — üç durumun iki görseli olduğu için
        /// arayüz Alive → Downed ile Downed → Dead geçişlerine farklı cevap
        /// verir.
        ///
        /// KİMLİK TAŞIMAZ ve bu bilinçlidir: bu tip kendi
        /// <see cref="GridStrategy.Core.Unit"/>'ini bilmez. Kimliği ekleyen
        /// halka bir üst katmandadır.
        /// </summary>
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
        /// <c>GridStrategy.Combat</c>'i tanımak zorunda kalır ve
        /// <see cref="TargetingRules"/> takımı öğrenmek için bir Combatant'tan
        /// bir Unit'e ulaşmak zorunda kalırdı — bugün ikisi arasında böyle bir
        /// bağ yok ve bu işi kurmak için bağ açmaya değmez.
        /// </summary>
        // Taraf kurulurken belli olur ve DEĞİŞMEZ. Diğer üç parça da readonly;
        // dördüncüsünün yazılabilir olması tipin tek sözünü — "kurulduğun anda
        // ne olduğun bellidir" — tek satırda bozardı.
        //
        // REDDEDILEN - Combatant.cs:246 yerine:
        //     public Team Team { get; set; }
        // KIRILAN  : "aynı takım mı" sorusunun cevabı iki çağrı arasında DEĞİŞEBİLİR olur.
        //            AttackAction hedefi onaylar -> araya giren tek atama tarafı çevirir
        //            hasar uygulanana kadar dost ateşi açılır -> ancak oyunda görülür
        //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
        // KAZANIRDI: ele geçirme gerçek bir mekanik olsaydı — zihin kontrolü, bayrak
        //            devri, taraf değiştiren paralı asker.
        // TEK CUMLE: Kurulurken belli olan şey readonly yazılır; yazılabilir bir taraf
        //            "kurulduğun anda ne olduğun bellidir" sözünü tek satırda bozar.
        public Team Team { get; }

        // Durumu soran TEK üye bu. Yanına bir kısayol bool eklemek reddedildi.
        //
        // REDDEDILEN - Combatant.cs:283 yerine (State'in yanında, onun kısayolu):
        //     public bool IsAlive => lifecycle.State == UnitState.Alive;
        // KIRILAN  : iki soru aynı birim hakkında ZIT cevap verir.
        //            Downed birim -> IsAlive false, ama CanBeAttacked hâlâ true
        //            aynı birim -> TryRevive hâlâ başarılı, çağıran hangisine baksın
        //            kurtarma penceresi "ölü sayılan" birimlerde sessizce kullanılmaz olur
        //            derleyici: hiçbir şey der  .  test: Downed_StillAcceptsDamage örtbas eder
        // KAZANIRDI: yaşam döngüsü gerçekten iki değerli olsaydı — Alive/Dead, kurtarma
        //            penceresi yok — o gün enum bir tipi boşuna eklemiş olurdu.
        // TEK CUMLE: UnitState tam olarak "iki bayrak dört kombinasyon üretir, üçü
        //            anlamlıdır" diye doğdu — gerekçesi UnitState.cs'te — ve yanına
        //            konan tek bir bool o iki kaynaklı gerçeği geri getirir.
        //
        // Health.HasRemaining de bu boşluğu dolduramaz ve bilerek doldurmuyor: o
        // SAYIyı söyler, State ALAN yargısını. Downed bir birim ile Dead bir
        // birimin canı aynıdır — ikisi de sıfır — ama biri kurtarılabilir,
        // diğeri değildir. Farkı yalnızca yaşam döngüsü bilir.
        public UnitState State => lifecycle.State;

        public int CurrentHealth => health.Current;

        public float RemainingSeconds => lifecycle.RemainingSeconds;

        public bool IsReadyForCleanup => lifecycle.IsReadyForCleanup;

        /// <summary>
        /// Hasar uygular ve gerekiyorsa yaşam döngüsüne haber verir.
        ///
        /// Kontrol her KAREde değil, her HASAR OLAYINDA yapılır — ve zaten bu
        /// metodun içindeyiz, yani "canı bitti mi" sorusunu sormanın maliyeti
        /// bir bool okuması. Ayrı bir dinleme mekanizması (event) bugün buna
        /// hiçbir şey katmazdı: haber verecek olan da, duyacak olan da bu tip.
        /// </summary>
        public void TakeDamage(int amount)
        {
            // REDDEDILEN - Combatant.cs:296 satırının üstüne eklenmesi reddedildi:
            //     if (!health.HasRemaining) return;
            // KIRILAN  : üç ayrı sebep aynı yönü gösterir, en pahalısı ortadaki.
            //            ölçüldü: gövdenin tamamı 0,92 ns -> kâr karede 1,1 milyon çağrı ister
            //            Downed birim hasar almaya devam eder -> "bitirme" yolu kapanır
            //            "kime vurulur" TargetingRules'ın -> kural iki yerde eskir
            //            derleyici: hiçbir şey der  .  test: Downed_StillAcceptsDamage kırmızı
            // KAZANIRDI: hasar almak bir DOSYA/AĞ işi tetikleseydi — ölüm kaydı, sunucuya
            //            bildirim — o zaman erken çıkış nanosaniye değil milisaniye kazandırırdı.
            // TEK CUMLE: Erken çıkış bir performans kararı gibi görünür, oysa burada bir
            //            KURAL kararıdır: yerdekine vurmayı kapatır.
            health.TakeDamage(amount);

            // Soru SAYIya soruluyor, alana değil: "canı kaldı mı". Alan cevabını
            // (Alive / Downed / Dead) bu satırdan sonra UnitLifecycle verir.
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

            // Can sıfırdayken iyileştirdiğimiz için sonuç doğrudan payı verir.
            health.Heal(health.Max / ReviveHealthDivisor);
            return true;
        }

        public void Tick(float deltaSeconds)
        {
            lifecycle.Tick(deltaSeconds);
        }
    }
}
