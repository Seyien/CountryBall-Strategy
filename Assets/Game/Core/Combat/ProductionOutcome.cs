namespace GridStrategy.Combat
{
    // ═══ ROL: TANIM (Profile) ════════════════════════════════════════
    // kimlik : yok — iki Allowed değeri aynı şeydir
    // hafıza : yok — ölçüsü şu: bir değişkene Allowed yaz ve bekle, hâlâ
    //          Allowed'dır; "kaç saniye kaldı" bilgisini bu değer taşımaz.
    //          Sayacı StructureProduction ayrı bir alanda tutar
    // Unity  : gerekmez
    // karar  : vermez — üretim isteğinin sonucunu ADLANDIRIR; karara
    //          ProductionRules varır
    /// <summary>
    /// Bir üretim isteğinin sonucu.
    ///
    /// Neden <c>bool</c> değil: dört ret sebebinin dördü de oyuncuya FARKLI bir
    /// cümle söyler — "bu yapı onu üretmiyor", "yıkık bir yapı üretemez",
    /// "tarafsız bir yapı ordu kuramaz", "henüz hazır değil". Tek bir false
    /// dördünü de aynı sessizliğe düşürürdü ve oyuncu neyi yanlış yaptığını
    /// hiçbir zaman öğrenemezdi.
    ///
    /// <see cref="TargetingRules"/>'ın <c>bool</c> dönmesiyle ÇELİŞMİYOR: orada
    /// ret sebepleri oyuncuya söylenecek şeyler değil, aynı cümlenin
    /// parçalarıydı ("bu geçerli bir hedef değil"). Ayrım kaç dal olduğunda
    /// değil, dalların ekranda AYRIŞIP AYRIŞMADIĞINDA.
    ///
    /// AYNA BELGE: bu tipin gerekçeleri bugün yalnızca bu dosyada; sonuç
    /// enumlarının ortak gerekçesi Docs/deep/konular/06-sonuc-enumlari.md
    /// dosyasında yazılı.
    /// </summary>
    public enum ProductionOutcome
    {
        // SIFIRINCI DEĞER BİLEREK BİR RET — gerekçesi MoveOutcome,
        // AttackOutcome ve PlacementOutcome tiplerinde üç kez yazılı ve burada
        // dördüncü kez uygulanıyor. Dört ret arasından BU seçildi çünkü tek
        // ÇAĞIRAN HATASI olan bu: ötekiler oyunun o anki hâlini anlatır,
        // "bilinmeyen birim" ise sorunun kendisinin yanlış sorulduğunu söyler.
        // Toplu ayrılmış bir dizi bu değerle dolduğunda hiçbir birim doğmaz.
        /// <summary>İstenen birim türü bu yapının üretim listesinde yok.</summary>
        RejectedUnknownUnit,

        // TARAFSIZ ÜRETİCİ REDDİ, TargetingRules'taki "Team.None saldıramaz"
        // satırının KOPYASI DEĞİL KARDEŞİDİR: orada iki taraf karşılaştırılıyor,
        // burada tek taraf sorgulanıyor. Ortak cümle şu — tarafsız bir şey
        // EYLEYEN olamaz, ama HEDEF olabilir.
        /// <summary>Üreten yapı tarafsız; tarafsız bir yapı ordu kuramaz.</summary>
        RejectedNeutralProducer,

        /// <summary>Üreten yapı ayakta değil; enkaz birim üretmez.</summary>
        RejectedProducerDestroyed,

        // TEK BEKLENEBİLİR RET BU. Ötekiler beklemekle düzelmez; bu düzelir ve
        // ayrımın kendisi kural sırasına yazılı — ProductionRules bu sebebi EN
        // SONA bırakıyor ki oyuncuya önce düzeltilemeyen sebep söylensin.
        /// <summary>Bekleme süresi henüz dolmadı.</summary>
        RejectedNotReady,

        // BEŞİNCİ BİR "RejectedCellOccupied" YAZILMADI ve yokluğu bir karardır:
        // hücrenin dolu olup olmadığını TAHTA bilir, bu kural tahtayı hiç
        // görmez. O sorunun sahibi zaten var ve adı PlacementOutcome; ikinci bir
        // ev açmak aynı cevabı iki yerde tutmak olurdu.
        /// <summary>Üretim serbest.</summary>
        Allowed
    }
}
