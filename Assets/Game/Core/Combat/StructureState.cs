namespace GridStrategy.Combat
{
    // ═══ ROL: TANIM (Profile) ════════════════════════════════════════
    // kimlik : yok — iki Destroyed değeri aynı şeydir
    // hafıza : yok — bir değer; kendisi hiçbir şey yapmaz
    // Unity  : gerekmez
    // karar  : vermez — yapının durumunu ADLANDIRIR; geçişe StructureLifecycle
    //          karar verir
    //
    // REDDEDILEN - StructureState.cs:54 yerine (bu dosya hiç doğmaz, yapılar
    //              UnitState'i yeniden kullanır):
    //     public UnitState State { get; private set; }   // StructureLifecycle içinde
    // KIRILAN  : yapı üzerindeki HER switch, asla çalışamayacak bir Downed dalı açar.
    //            dal `throw` olur         -> hiçbir testin geçmediği ölü kod
    //            dal "ayakta"ya düşer     -> yıkılmış baraka sağlam çizilir
    //            CanBeAttacked(UnitState) yapıları da yönetir -> `state != Dead`
    //            kurtarma penceresi kuralını binalara uygular
    //            derleyici: hiçbir şey der  .  test: ölü dal hiçbir testte geçilmez
    // KAZANIRDI: yapılar Downed'a denk bir ARA duruma kavuşsaydı — "yıkılan baraka
    //            yanar, itfaiye yetişirse ayağa kalkar". O gün tek enum doğru olurdu.
    // KARSILASTIRMA:
    //     UnitState paylaş   üç değer   -> her switch'te asla çalışmayan Downed dalı
    //     bool isDestroyed   iki değer  -> üçüncü durum tartışılamaz, iç içe if doğar
    //     StructureState     iki değer  -> TargetingRules iki dil konuşur (ödenen bedel)
    // TEK CUMLE: İki tip aynı DEĞERLERİ taşıyabilir ama aynı GEÇİŞLERİ taşımıyorsa
    //            enum ortak olamaz; baraka düşmez, yıkılır.
    /// <summary>
    /// Bir yapının iki durumu: ayakta ya da yıkılmış.
    ///
    /// Neden <see cref="UnitState"/> değil: bir baraka DÜŞMEZ. Düşme durumu
    /// (<see cref="UnitState.Downed"/>) "ölü ama kurtarılabilir" demektir ve
    /// tek var olma sebebi diriltme penceresidir. Yapıda böyle bir pencere yok:
    /// yıkılan bina onarılmaz, yeniden İNŞA edilir — ve yeniden inşa, bu tipin
    /// bir geçişi değil, yeni bir yapı nesnesidir.
    ///
    /// Neden <c>bool isDestroyed</c> değil: bugün iki değer var, ama ayrım
    /// enum'da durduğu sürece üçüncü bir durum (bkz. aşağıdaki Rubble bloğu)
    /// tartışılabilir kalır; bool ile o tartışma hiç yazılamaz, yalnızca iç içe
    /// if'lere dönüşür.
    ///
    /// ÖDENEN BEDEL — ve ödendi: <see cref="TargetingRules"/> artık İKİ durum dili
    /// konuşuyor. <c>CanBeAttacked(StructureState)</c> ve taraflı ikizi yazıldı;
    /// bedel bir aşırı yükleme çiftiydi, alternatifin bedeli ise her switch'te
    /// asla çalışmayan bir <see cref="UnitState.Downed"/> dalıydı. Aşırı yüklemeyi
    /// derleyici ister ve gözle görülür; ölü dalı kimse istemez ve görünmez.
    ///
    /// Bedelin tamamı bu çift DEĞİLDİ, ve fark burada yazılı: hedefleme kuralının
    /// yapı tarafı saldırıda ikizlendi, diriltmede İKİZLENMEDİ. Yapı dirilmez —
    /// <see cref="Structure.TryRepair"/> bir onarımdır, durum geçişi değil — ve
    /// bu asimetri <see cref="TargetingRules"/> içinde bir REDDEDILEN bloğu olarak
    /// yazılı. Tek enum seçilseydi bu ayrım hiç sorulamazdı: yapılar birimlerin
    /// diriltme kuralını sessizce devralırdı.
    /// </summary>
    public enum StructureState
    {
        // Sıfırıncı değer BİLEREK "yıkık". Kural şu: atanmayı unutulmuş bir alan,
        // olabilecek en ZARARSIZ şeye benzemeli — ve tahtada zararsız olan şey,
        // hiç olmayan şeydir. Enkaz, "burada bir şey yok"a en yakın değerdir.
        //
        // REDDEDILEN - StructureState.cs:77 yerine:
        //     Standing,
        //     Destroyed
        // KIRILAN  : default(StructureState) artık "ayakta" demek olur. Sıfırıncı
        //            değer gerekçesinin tamamı Team.cs'te yazılı; buradaki FARK,
        //            atanmamış değerin tahtada SAĞLAM bir bina üretmesi —
        //            hedeflenir, saldırı emri doğurur, üretim yaptığı varsayılır.
        //            derleyici: hiçbir şey der  .  test:
        //            Default_StructureStateValue_IsDestroyedNotStanding kırmızıya döner
        // KAZANIRDI: Team.cs'tekiyle aynı — sıfır bir güvenlik değil SIKLIK kararı
        //            olsaydı; tahtadaki yapıların çoğu her an ayakta.
        // TEK CUMLE: Enkaz, "burada bir şey yok"a en yakın değerdir; atanmayı
        //            unutulan alan en zararsız şeye benzemelidir.
        /// <summary>
        /// Yıkılmış. Hedeflenemez, saldırmaz, onarılamaz; geriye yalnızca enkaz
        /// temizliği kalır. Sıfırıncı değer olması güvenlik kararıdır.
        /// </summary>
        Destroyed,

        // REDDEDILEN - StructureState.cs:95 yerine (IsReadyForCleanup bayrağı
        //              yerine üçüncü bir değer):
        //     Destroyed,
        //     Rubble,
        //     Standing
        // KIRILAN  : üçüncü değer hiçbir KURALI değiştirmez — yıkık da enkaz da
        //            saldırmaz, hedeflenmez, onarılmaz.
        //            her switch'te iki dal aynı gövde -> biri güncellenir, biri unutulur
        //            aynı karar UnitLifecycle'da da verildi: Dead + IsReadyForCleanup
        //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
        // KAZANIRDI: enkaz gerçekten ayrı bir kural taşısaydı — yıkık bina hücreyi
        //            kapatırken temizlenmiş enkaz geçişe açılsaydı. O gün fark
        //            bayrakta değil durumda yaşar, UnitGrid bu değeri sorardı.
        // TEK CUMLE: "Artık kaldırılabilirim" bir İSTEKtir, bir durum değil;
        //            istekler bayrakla, durumlar enum'la yazılır.
        /// <summary>Ayakta. Hedeflenebilir, canı azalır, onarılabilir.</summary>
        Standing
    }
}
