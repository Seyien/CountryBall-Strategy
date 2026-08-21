namespace GridStrategy.Combat
{
    // ═══ ROL: TANIM (Profile) ════════════════════════════════════════
    // kimlik : yok — iki Hit değeri aynı şeydir
    // hafıza : yok — bir değer; kendisi hiçbir şey yapmaz
    // Unity  : gerekmez
    // karar  : vermez — olup biteni ADLANDIRIR; kararı AttackAction verir
    /// <summary>
    /// Bir saldırı DENEMESİNİN sonucu. "Deneme" kelimesi kasıtlı: reddedilen
    /// bir saldırı da bir sonuçtur ve çağıranın onu ayırt etmesi gerekir.
    ///
    /// HEDEF TİPİNDEN BAĞIMSIZ: aynı enum hem <see cref="Combatant"/>'a hem
    /// <see cref="Structure"/>'a yapılan saldırıyı adlandırır. Ret sebepleri ve
    /// "vurdu" cevabı ikisinde de birebir aynı cümledir; ayrışan tek şey ÖLÜM
    /// olayının adıdır ve o da tek bir değerle ifade ediliyor
    /// (<see cref="HitAndDowned"/> ↔ <see cref="HitAndDestroyed"/>).
    ///
    /// Neden <c>bool</c> değil: "saldırı oldu mu" üç ayrı soruyu tek cevaba
    /// sıkıştırırdı — reddedildi mi, vurdu mu, düşürdü mü. Çağıran üçüne farklı
    /// tepki verir: ret sessizdir (belki bir uyarı sesi), vuruş bir efekt ister,
    /// düşürme bir animasyon ve muhtemelen bir skor. <c>bool</c> ile yazılsaydı
    /// bu ayrım çağıranın içinde ikinci bir kontrol olarak yeniden doğardı.
    /// </summary>
    public enum AttackOutcome
    {
        // Sıfırıncı değer BİLEREK bir RET değeri.
        //
        // REDDEDILEN - AttackOutcome.cs:42 yerine:
        //     Hit,
        //     HitAndDowned,
        //     RejectedInvalidTarget,
        //     RejectedOutOfRange
        // KIRILAN  : default(AttackOutcome) artık "vurdu" demek olur. Sıfırıncı
        //            değer gerekçesinin tamamı Team.cs'te yazılı; buradaki FARK,
        //            atanmamış değerin BAŞARILI bir saldırı gibi okunması —
        //            sıfırlanmış dizi hücresi hasar uygulanmış sayılır.
        //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
        // KAZANIRDI: Team.cs'tekiyle aynı — sıfır bir güvenlik değil SIKLIK
        //            kararı olsaydı (histogram sıkıştırması).
        // TEK CUMLE: Sıfırıncı değer bir varsayılan değil bir SİGORTAdır; en
        //            zararsız cevap "olmadı"dır.
        RejectedInvalidTarget,

        /// <summary>Menzil dışı. Hedef geçerliydi ama ulaşılamadı.</summary>
        RejectedOutOfRange,

        /// <summary>Hasar uygulandı; hedef hâlâ ayakta.</summary>
        Hit,

        // REDDEDILEN - AttackOutcome.cs:67 yerine (enum tamamen kalkar):
        //     public readonly struct AttackOutcome
        //     {
        //         public bool Rejected { get; }
        //         public int DamageDealt { get; }
        //         public bool Downed { get; }
        //     }
        // KIRILAN  : üç alanın ikisi her çağrıda anlamsız kalır ve anlamı tip söylemez.
        //            ret durumunda DamageDealt sıfır mı tanımsız mı -> çağıran hatırlar
        //            switch'te EKSİK DAL derleyiciden görünmez olur; enum'da görünür
        //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
        // KAZANIRDI: UI hasar sayısını göstermek istediği gün — "12 hasar" yazmak
        //            için miktar gerekir ve enum onu taşıyamaz. O gün bu enum
        //            struct'ın İÇİNE bir alan olarak girer; silinmez.
        // TEK CUMLE: Enum "hangi durum" sorusunu TİPE sordurur, struct çağıranın
        //            hafızasına; bugün sorulan soru "hangi durum".
        /// <summary>Hasar uygulandı ve hedef bu vuruşla düştü.</summary>
        HitAndDowned,

        // BEŞİNCİ DEĞER, AYRI BİR ENUM DEĞİL.
        //
        // Neden HitAndDowned yeniden kullanılmıyor: bir baraka DÜŞMEZ, yıkılır.
        // "Düşme" (bkz. StructureState.cs) kurtarma penceresi açan bir durumdur;
        // yapıda öyle bir pencere yok. Aynı değeri iki farklı olguya vermek,
        // çağıranın onları ayırt etmesini imkânsız kılardı.
        //
        // Neden düz Hit dönülmüyor: yıkım bilgisi çağıranın elinden alınırdı ve
        // çağıran onu geri kazanmak için saldırıdan SONRA State'i okumak zorunda
        // kalırdı — StructureLifecycle'ın "dönüş değeri: soran zaten orada"
        // kararının tam tersi. Üstelik o okuma yanlış cevap verirdi: zaten yıkık
        // bir enkaza vurmak da "State == Destroyed" gösterir.
        //
        // REDDEDILEN - AttackOutcome.cs:100 yerine (yapılar kendi enum'unu alır):
        //     public enum StructureAttackOutcome
        //     {
        //         RejectedInvalidTarget,
        //         RejectedOutOfRange,
        //         Hit,
        //         HitAndDestroyed
        //     }
        // KIRILAN  : her tüketici PARALEL bir switch taşır ve ikizler zamanla ayrışır.
        //            bugün iki tüketici var -> BattleActions, BoardAdapter.ReactToAttack
        //            biri yeni değeri işler, diğeri işlemez -> switch DEYİMİ uyarmaz
        //            tek `default: LogError` koruması ikiye bölünür -> yıkım duyurulmaz
        //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
        // KAZANIRDI: yapı saldırıları, birimlerde HİÇ karşılığı olmayan değerler
        //            kazandığı gün — kuşatma bonusu, kısmi çökme, duvar kaybı.
        // TEK CUMLE: İki olgunun ret sebepleri AYNI cümleyse tek enum doğrudur ve
        //            farklı olan tek değer o enum'a EKLENİR, ikinci enum açılmaz.
        /// <summary>Hasar uygulandı ve YAPI bu vuruşla yıkıldı.</summary>
        HitAndDestroyed,

        // ALTINCI DEĞER, VE SONA EKLENDİ — ret ailesinin yanına değil.
        //
        // Diğer üç ret değerinin yanına sokmak, aradaki üç değeri sessizce
        // yeniden numaralandırırdı. Bugün bu enum'un sayılarını saklayan hiçbir
        // yer yok, yani kırılma ÖLÇÜLEBİLİR değil — ama sona eklemek geri
        // alınabilir olanıdır ve kaynaktaki sıra zaten akış sırası DEĞİL:
        // HitAndDestroyed beşinci sırada durup bir BAŞARI değeridir. Sıra
        // bilgisini enum'dan okumaya çalışan bir gözün burada zaten yanılacak
        // olması, ret değerlerini kümelemenin taşıdığı tek faydayı da siler.
        //
        // ANLAMI TEK CÜMLE: eylemi yapan taraf şu an eylem yapamaz. Üç ayrı
        // sebebi birden kapsar — saldıran düşmüş, hareket eden düşmüş, sırası
        // değil — ve kapsaması BİLİNÇLİ: çağıranın dallanması üçünde de aynıdır.
        // Hedefi ya da hedef hücreyi değiştirmek hiçbirinde yardım etmez.
        //
        // AYNI AD MoveOutcome'da DA VAR ve üretilebilirlikleri FARKLI:
        // burada AttackAction (Combat) bu değeri kendisi üretebilir, çünkü
        // UnitState'i görür ve AttackRules'a sorar; MoveOutcome tarafında ise
        // sahibi MoveAction onu ASLA üretemez. Farkın tamamı ve EŞİĞİ
        // MoveOutcome.cs'te yazılı; burada tekrarlanmıyor.
        //
        // REDDEDILEN - AttackOutcome.cs:140 yerine (tek değer yerine iki ayrı
        //              sebep):
        //     RejectedNotYourTurn,
        //     RejectedAttackerCannotAct
        // KIRILAN  : çağıranın dallanması bugün ikisinde de AYNI — BoardAdapter
        //            yalnızca log basıyor — yani iki dal aynı satırı iki kez yazar.
        //            birini işlemeyi unutan switch DEYİMİ -> "sıran değil" ekrana ulaşmaz
        //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
        // KAZANIRDI: arayüz oyuncuya "sıran değil" ile "birim düşmüş" farkını
        //            SÖYLEMEK zorunda kaldığı gün; tetiği MoveOutcome.cs'te yazılı.
        // TEK CUMLE: Bir ayrım, çağıranın DALLANMASINI değiştirdiği gün doğar;
        //            bugün değiştirmiyor, yalnızca maliyet ekliyor.
        /// <summary>
        /// Saldıran şu an saldıramaz: durumu elvermiyor
        /// (<see cref="AttackRules.CanAttack"/>) ya da sırası değil
        /// (bu ikincisini yalnızca <c>BattleActions</c> üretir).
        /// </summary>
        RejectedActorCannotAct
    }
}
