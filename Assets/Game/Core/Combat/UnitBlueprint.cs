using System;

namespace GridStrategy.Combat
{
    // ═══ ROL: TANIM (Definition) ═════════════════════════════════════
    // kimlik : yok — ölçüsü şu: aynı sayıları taşıyan iki UnitBlueprint'i
    //          birbirinin yerine koy, hiçbir çağıranın cevabı değişmez.
    //          Ölçü `==` DEĞİL — Equals yazılmadı; ölçü YERİNE
    //          GEÇEBİLİRLİK
    // hafıza : yok — ölçüsü şu: CreateCombatant'ı arka arkaya İKİ kez
    //          çağır, bu nesneye yazılan tek bir alan yoktur ve ikinci
    //          çağrı birincinin bıraktığı hiçbir izi okumaz. Can ve hâl
    //          ÜRETİLEN nesnede doğar, burada değil
    // Unity  : gerekmez — düz C# nesnesi; bu klasörün asmdef'indeki
    //          noEngineReferences: true bu tiple bozulmaz. Tasarımcının
    //          göreceği dosya bu tipin KENDİSİ değil, onu üreten motor
    //          tarafındaki UnitBlueprintAsset
    // karar  : vermez — sayıları TAŞIR ve onlardan bir savaşçı KURAR;
    //          "üretilebilir mi" sorusunun sahibi ProductionRules
    /// <summary>
    /// Bir birim TÜRÜNÜN değişmez tanımı: "piyade 30 can, 10 hasar, 1 menzil".
    ///
    /// <see cref="AttackProfile"/>'ın taşıdığı iddianın BİR ÜST KATI: orada
    /// paylaşılan şey bir saldırı tanımıydı, burada paylaşılan şey bir birim
    /// türünün tamamı. İkisi de aynı cümleyi kuruyor — tanım paylaşılır,
    /// varlık kopyalanır.
    ///
    /// Neyi TUTMAZ: canın KAÇ kaldığını, birimin hangi hâlde olduğunu, hangi
    /// tarafta olduğunu, tahtada nerede durduğunu. Dördü de örneğe aittir ve
    /// dördü de <see cref="CreateCombatant"/>'ın ürettiği nesnede doğar.
    /// Bu ayrım bu dosyanın var olma sebebidir: bir tanım örnek durumu
    /// taşımaya başladığı gün, onu paylaşan yüzlerce birim aynı canı paylaşır.
    ///
    /// AYNA BELGE: bu tipin gerekçeleri bugün yalnızca bu dosyada; ayrı bir
    /// derin anlatım henüz yazılmadı.
    /// </summary>
    // PAYLAŞIM İDDİASI BURADA ÖLÇÜLEBİLİR OLMAK ZORUNDA. AttackProfile ve
    // MoveProfile "yüzlerce asker tek örneği paylaşabilir" diyor ama ölçüldü:
    // üretimde profil kuran iki satır var ve ikisi de HER çağrıda yeni bir
    // örnek doğuruyor, yani bugünkü paylaşım SIFIR. Bu tip aynı iddiayı
    // çoğaltmıyor, KAPATIYOR: aşağıdaki attackProfile alanı bir kez kurulur ve
    // CreateCombatant onu her savaşçıya AYNI referans olarak verir.
    // ÖLÇÜSÜ: aynı tanımdan üretilmiş iki savaşçının AttackProfile'ı için
    // ReferenceEquals true döner; BoardAdapter'ın NewCombatant üyesinden
    // üretilmiş iki savaşçı için false döner.
    public sealed class UnitBlueprint
    {
        // DOĞRULAMA KURUCUDA DURUR, OnValidate'te DEĞİL — gerekçe AttackProfile'da
        // ölçülerek yazıldı ve burada tekrar edilmiyor, UYGULANIYOR: bu tip motor
        // tarafındaki sarmalayıcıdan da, testten de, ileride bir yükleyiciden de
        // kurulabilir; doğrulama Inspector'a taşınsaydı yalnızca birinci yol
        // sınanırdı.
        public UnitBlueprint(string displayName, int maxHealth, AttackProfile attackProfile)
        {
            // AD BOŞ GEÇİLEMEZ ÇÜNKÜ ADIN TEK OKUYUCUSU EKRAN. Unit.Name "insanın
            // okuması için" diye adlandırılmıştı ve orada boş ad zararsızdı; burada
            // değil — bu ad sol panelde bir düğmenin üstünde çıkar ve boş bir düğme
            // tıklanabilir ama okunamaz.
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("Display name cannot be empty.", nameof(displayName));
            }

            // Can EŞİĞİ burada değil Health'te doğrulanıyor gibi görünebilir; öyle
            // değil. Health(max) kendi kelepçesini taşır ama o kelepçe ancak
            // savaşçı DOĞDUĞUNDA çalışır — yani tasarımcının yazdığı sıfır, hatayı
            // ilk birim üretilene kadar saklar. Eşiği tanımın kurucusuna koymak,
            // hatayı varlık doğmadan görünür kılar.
            if (maxHealth < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxHealth), maxHealth, "Max health must be at least 1.");
            }

            // ZORUNLU, İSTEĞE BAĞLI DEĞİL — ve bu Structure'ın tam TERSİ. Orada
            // saldırı profili isteğe bağlıydı çünkü yapıların çoğu saldırmaz;
            // burada zorunlu çünkü Combatant'ın kurucusu null profili zaten
            // istisnayla reddediyor. İsteğe bağlı yazsaydık, kural kodun iki ayrı
            // yerinde iki farklı cevap verirdi ve ikinci cevabı ancak oyun
            // sırasında görürdük.
            AttackProfile = attackProfile
                ?? throw new ArgumentNullException(nameof(attackProfile));

            DisplayName = displayName;
            MaxHealth = maxHealth;
        }

        /// <summary>Ekranda görünen ad. Tahtadaki kimlik DEĞİLDİR — o
        /// <c>GridStrategy.Core.Unit</c>'te yaşar ve bu tip onu hiç görmez.</summary>
        // BU TİP Unit'İ GÖREMEZ, GÖRMEK İSTEMEDİĞİ İÇİN DEĞİL: Unit
        // GridStrategy.Core'da yaşıyor ve bu klasörün asmdef'inde references
        // listesi BOŞ. Yani "tanım kimliği kurmasın" kararı burada bir üslup
        // tercihi değil, derleyicinin dayattığı bir olgudur.
        public string DisplayName { get; }

        /// <summary>Bu türden doğan her birimin başlangıç ve azami canı.</summary>
        public int MaxHealth { get; }

        /// <summary>
        /// Bu türün saldırı tanımı. Bu türden doğan BÜTÜN savaşçılar bu TEK
        /// nesneyi paylaşır — tipin var olma sebebi olan iddia budur.
        /// </summary>
        public AttackProfile AttackProfile { get; }

        /// <summary>
        /// Bu tanımdan yeni bir savaşçı kurar. Taraf DIŞARIDAN gelir: aynı
        /// piyade tanımı iki tarafta da kullanılır ve tanımın taraf tutması
        /// düşmanın piyadesini ikinci bir dosyaya zorlardı.
        /// </summary>
        /// <returns>
        /// Her çağrıda YENİ bir <see cref="Combatant"/>: canı ve yaşam döngüsü
        /// örneğe özeldir. Paylaşılan tek parça saldırı tanımıdır.
        /// </returns>
        // TANIMIN VARLIK KURMASI BİR ROL KAYMASI DEĞİL, ROLÜN TAMAMLANMASIDIR.
        // Alternatif ölçüldü ve reddedildi: kurma işi çağırana bırakılsaydı, üç
        // parçayı (Health, UnitLifecycle, AttackProfile) doğru sırayla birleştirme
        // sözleşmesi her çağıranda yeniden yazılırdı — ve BoardAdapter'ın
        // NewCombatant üyesi tam olarak o kopyanın bugünkü tek örneğidir. İkinci
        // kopya doğduğu gün biri saldırı profilini paylaşır, öteki paylaşmaz ve
        // fark hiçbir derleme hatası vermeden ekrana düşer.
        public Combatant CreateCombatant(Team team)
        {
            // Health ve UnitLifecycle HER ÇAĞRIDA YENİ — burası tanımın örnek
            // durumu taşımamasının uygulandığı tek satırdır. AttackProfile ise
            // alandan geçiyor, kopyalanmıyor: değişmez olduğu için paylaşılması
            // güvenli, ve paylaşıldığı için iddia gerçek.
            return new Combatant(
                new Health(MaxHealth),
                new UnitLifecycle(),
                AttackProfile,
                team);
        }
    }
}
