using System;

namespace GridStrategy.Combat
{
    // ═══ ROL: VARLIK (Entity) ════════════════════════════════════════
    // kimlik : var — ölçüsü şu: aynı tanımdan İKİ StructureProduction kur,
    //          yalnız birinde Produce çağır; onun RemainingSeconds'ı eşiğe
    //          fırlar, ötekininki 0'da kalır. Yani iki baraka aynı türden
    //          olsa da ayrı ayrı bekler
    // hafıza : var — ölçüsü şu: Produce'tan sonra Tick(1f)'i arka arkaya İKİ
    //          kez çağır, iki FARKLI cevap alırsın: RemainingSeconds düşer ve
    //          IsReady bir noktada false'tan true'ya döner. Farkı doğuran şey
    //          tipin kalan saniyeyi çağrılar arasında tutması
    // Unity  : gerekmez — zaman DIŞARIDAN gelir; gerekçe UnitLifecycle'da
    //          ölçülerek yazıldı ve burada uygulanıyor
    // karar  : ÜRETİR — ama uygunluk kararını kendi vermez, ProductionRules'a
    //          sorar; tahtaya da dokunmaz, doğan savaşçıyı çağırana verir
    /// <summary>
    /// Yerleşmiş TEK bir yapının üretim hattı: hangi türden olduğu, hangi
    /// yapıya bağlı olduğu ve bir sonraki üretime kaç saniye kaldığı.
    ///
    /// <see cref="StructureLifecycle"/>'ın AYNADAKİ İKİZİ: orada bir sayaç
    /// yıkımla başlar ve dolunca bir izin açar (enkaz kaldırılabilir), burada
    /// bir sayaç üretimle başlar ve dolunca bir izin açar (yeniden
    /// üretilebilir). Aynı omurga, ters yön.
    ///
    /// Neyi BİLMEZ: yapının tahtada nerede durduğunu, üretilen birimin nereye
    /// konacağını, ekranda ne göründüğünü. Üçü de motor katmanının işi ve bu
    /// tip üçünü de tip olarak bile yazamaz.
    ///
    /// AYNA BELGE: bu tipin gerekçeleri bugün yalnızca bu dosyada; yaşam
    /// döngüsü ailesinin ortak anlatımı Docs/deep/konular/05-yasam-dongusu.md
    /// dosyasında yazılı.
    /// </summary>
    public sealed class StructureProduction
    {
        private readonly StructureBlueprint blueprint;
        private readonly Structure structure;

        private float remainingSeconds;

        /// <param name="structure">
        /// Bu hattın bağlı olduğu yapı. Referans TUTULUYOR, kopyalanmıyor:
        /// yapının durumu ve tarafı burada okunacak ve ikinci bir kopya, bina
        /// yıkıldığında sessizce eskirdi.
        /// </param>
        public StructureProduction(StructureBlueprint blueprint, Structure structure)
        {
            this.blueprint = blueprint ?? throw new ArgumentNullException(nameof(blueprint));
            this.structure = structure ?? throw new ArgumentNullException(nameof(structure));

            // YENİ YAPI HAZIR DOĞAR, beklemede değil. Alternatif ölçüldü ve
            // reddedildi: bekleyerek doğsaydı yeni konmuş bir baraka ilk saniyelerde
            // hiçbir şey yapmazdı ve oyuncu yerleştirmenin işe yarayıp yaramadığını
            // GÖREMEZDİ. Bekleme süresi üretim HIZINI kelepçeler, ilk birimi değil.
            remainingSeconds = 0f;
        }

        /// <summary>Bu hattın ürettiği yapı türünün tanımı.</summary>
        public StructureBlueprint Blueprint => blueprint;

        /// <summary>
        /// Bu hattın bağlı olduğu yapı. Sağ panel tarafı buradan okur: üretilen
        /// birimin takımı ÜRETEN YAPININ takımıdır.
        /// </summary>
        public Structure Structure => structure;

        /// <summary>Bir sonraki üretime kalan saniye; hazırken 0.</summary>
        public float RemainingSeconds => remainingSeconds;

        /// <summary>
        /// Bekleme süresi doldu mu. Yapının ayakta olup olmadığına BAKMAZ —
        /// bu ikisi bağımsız iki eksendir ve ikisini birden gören tek yer
        /// <see cref="ProductionRules"/>.
        /// </summary>
        // TEK KAYNAĞA İNMEK DOĞRUDUR, DOĞRU KAYNAĞA İNMEK ŞARTTIR — aynı cümle
        // Structure'ın IsStanding üyesinin üstünde de yazılı. Buraya bir
        // "&& structure.IsStanding" eklenseydi, "yıkık yapı üretemez" kuralının
        // İKİ sahibi olurdu ve ret sebebi RejectedProducerDestroyed yerine
        // sessizce RejectedNotReady'ye düşerdi: oyuncu enkazın önünde beklerdi.
        public bool IsReady => remainingSeconds <= 0f;

        /// <summary>
        /// Bir birim üretmeyi dener. Uygunsa savaşçıyı kurar ve bekleme
        /// sayacını başlatır.
        /// </summary>
        /// <param name="requested">Üretilmesi istenen birim türü.</param>
        /// <param name="produced">
        /// Doğan savaşçı; ret durumunda <c>null</c>. Tahtaya konması ÇAĞIRANIN
        /// işi — bu tip tahtayı görmez.
        /// </param>
        // TEK GİRİŞ NOKTASI, İKİ ADIMLIK SÖZLEŞME DEĞİL. Alternatif "önce
        // ProductionRules'a sor, sonra Begin çağır" idi ve reddedildi: ikinci
        // adımı unutan tek çağıran sonsuz birim üretirdi ve hiçbir test kırmızıya
        // dönmezdi. Aynı gerekçe StructureLifecycle'ın OnHealthDepleted üyesinde
        // ölçülerek yazılı — cevabı hesaplayabilen tek yer onu döndürmelidir.
        public ProductionOutcome Produce(UnitBlueprint requested, out Combatant produced)
        {
            produced = null;

            // Liste karşılaştırması BURADA, kuralda değil: kural bir listeyi
            // görseydi tanıma bağlanır ve girdi kümesi enum olmaktan çıkardı.
            bool producesRequested = requested != null && Contains(requested);

            ProductionOutcome outcome = ProductionRules.CanProduce(
                structure.State,
                structure.Team,
                producesRequested,
                IsReady);

            if (outcome != ProductionOutcome.Allowed)
            {
                return outcome;
            }

            // SIRA BİR KARARDIR: önce savaşçı kurulur, sonra sayaç başlar. Tersi
            // olsaydı ve kurma bir istisna atsaydı, sayaç dolmuş ama hiçbir birim
            // doğmamış olurdu — oyuncu beklerken elinde hiçbir şey olmazdı.
            produced = requested.CreateCombatant(structure.Team);
            remainingSeconds = blueprint.ProductionSeconds;

            return ProductionOutcome.Allowed;
        }

        /// <summary>
        /// Zamanı ilerletir. Saniye DIŞARIDAN gelir — bu tipin motora
        /// bağlanmamasının ve EditMode'da sınanabilmesinin tek sebebi budur.
        /// </summary>
        public void Tick(float deltaSeconds)
        {
            // Geriye akan zaman bir ÇAĞIRAN hatasıdır ve gürültüyle patlar; aynı
            // kelepçe UnitLifecycle ve StructureLifecycle tiplerinde de var ve
            // mesajı bilerek birebir aynı — üç yerde üç farklı cümle okuyanı
            // üçünün farklı kurallar olduğuna inandırırdı.
            if (deltaSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(deltaSeconds), deltaSeconds, "Time cannot move backwards.");
            }

            // Hazırken geri sayım yok; erken çıkış burada başarım için değil,
            // DOĞRULUK için: aşağıdaki çıkarma sıfırı eksiye götürür ve
            // RemainingSeconds ekranda negatif bir sayı gösterirdi.
            if (remainingSeconds <= 0f)
            {
                return;
            }

            remainingSeconds -= deltaSeconds;
            if (remainingSeconds < 0f)
            {
                // Sayaç sıfırda TUTULUYOR, eksiye kaymıyor. Aynı kelepçe
                // StructureLifecycle'ın Tick üyesinde de var ve gerekçesi orada
                // yazılı: eksiye kayan bir sayaç, sonraki üretimin bekleme
                // süresini sessizce KISALTIRDI.
                remainingSeconds = 0f;
            }
        }

        /// <summary>
        /// İstenen birim türü bu yapının üretim listesinde mi.
        /// </summary>
        // DÖNGÜ, LINQ DEĞİL: Contains bir arayüz üzerinden çağrılsaydı
        // numaralandırıcı KUTULANIR ve her üretim isteği bir tahsis üretirdi.
        // Aynı gerekçe Battle'ın Tick üyesinde de yazılı. Karşılaştırma
        // REFERANS eşitliği — tanımlar paylaşılan nesnelerdir ve aynı tanımın
        // iki kopyası zaten olmamalıdır.
        private bool Contains(UnitBlueprint requested)
        {
            for (int i = 0; i < blueprint.Produces.Count; i++)
            {
                if (ReferenceEquals(blueprint.Produces[i], requested))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
