namespace GridStrategy.Core
{
    // ═══ ROL: TANIM (Profile) ════════════════════════════════════════
    // kimlik : yok — iki Moved değeri aynı şeydir
    // hafıza : yok — bir değer; kendisi hiçbir şey yapmaz
    // Unity  : gerekmez
    // karar  : vermez — olup biteni ADLANDIRIR; kararı MoveAction verir
    /// <summary>
    /// Bir hareket DENEMESİNİN sonucu. "Deneme" kelimesi kasıtlı: reddedilen
    /// bir hareket de bir sonuçtur ve çağıranın onu ayırt etmesi gerekir.
    ///
    /// Neden <c>bool</c> değil: "taşındı mı" tek cevaba üç ayrı soruyu
    /// sıkıştırırdı ve çağıran üçüne farklı tepki verir — tahta dışı bir
    /// tıklama sessizce yutulur, dolu hücre "orada biri var" uyarısı ister,
    /// menzil dışı ise yol bulucuya "önce yaklaş" der. <c>bool</c> ile
    /// yazılsaydı bu ayrım çağıranın içinde ikinci bir kontrol olarak
    /// yeniden doğardı — ve o kontrol MoveAction'ın kurallarını kopyalardı.
    /// </summary>
    public enum MoveOutcome
    {
        // Sıfırıncı değer BİLEREK bir RET değeri.
        //
        // REDDEDILEN - MoveOutcome.cs:35 yerine:
        //     Moved,                            // sıfırıncı değer BAŞARI olur
        //     RejectedInvalidDestination,
        // KIRILAN  : default(MoveOutcome) artık "taşındı" demek olur.
        //            "private MoveOutcome lastOutcome;" -> sıfırla, yani "taşındı" doğar
        //            hiç hareket denenmeden -> ekran "taşındı" der
        //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
        // KAZANIRDI: tip bir [Flags] maskesi olsaydı — o gün sıfır "hiç ret sebebi
        //            yok" demek olurdu ve sebepler BİRLEŞEBİLİRDİ; bugün akış ilk
        //            redde duruyor, tek sebep dönüyor.
        // TEK CUMLE: Sıfırıncı enum değeri bir isim değil, ATANMAMIŞ hâlin anlamıdır;
        //            o hâl asla başarı olmamalı.
        RejectedInvalidDestination,

        // Üç ret sebebi TEK bir "Rejected" değerine indirilebilirdi. İndirilmedi.
        //
        // REDDEDILEN - MoveOutcome.cs:52 yerine (üç değer bire iner):
        //     Rejected,
        //     Moved
        // KIRILAN  : çağıran "asla gidilemez" ile "şimdilik gidilemez"i ayıramaz.
        //            tahta dışı hücre -> yapay zekâ her turda yeniden dener
        //            dolu hücre       -> kalıcı vazgeçer, oysa bir tur sonra boşalır
        //            derleyici: hiçbir şey der  .  test: iki SIRA testi hedefsiz kalır
        // KAZANIRDI: sonucu yalnızca arayüz tüketseydi ve tek yaptığı şey geçersiz
        //            tıklamada bir uyarı sesi çalmak olsaydı — üç değer aynı sesi
        //            çalmanın üç yolu olurdu.
        // TEK CUMLE: Ret sebepleri ancak çağıranın DAVRANIŞI değişiyorsa ayrılır;
        //            burada "bekle" ile "vazgeç" tam olarak o davranış farkıdır.
        /// <summary>Hedef hücrede başka bir birim duruyor.</summary>
        RejectedCellOccupied,

        /// <summary>Hedef hücre tahtada ve boş, ama bu turda ulaşılamıyor.</summary>
        RejectedOutOfRange,

        /// <summary>Birim tahtada eski hücresinden yeni hücresine geçti.</summary>
        Moved,

        // BEŞİNCİ DEĞER — VE SAHİBİ ONU ÜRETEMEZ. Bu enum'un dosyası
        // GridStrategy.Core'da, akış sahibi MoveAction da orada; ama ne
        // MovementRules ne UnitState ne de TurnState Core'dan GÖRÜNÜR. Yani
        // aşağıdaki değeri döndürebilecek tek yer GridStrategy.Battle'daki
        // BattleActions'tır.
        //
        // Bu bir taviz ve öyle yazılıyor: bir tipe, sahibinin asla
        // üretemeyeceği bir değer eklendi. Gizlemek yerine SABİTLENDİ —
        // BattleActionsTests'teki MoveAction_NeverReturnsRejectedActorCannotAct
        // hiçbir girdiyle bu değerin Core'dan çıkmadığını tutuyor. Assembly
        // sınırı bir KISITTIR, ve kısıtın izini enum'da bırakmak onu
        // unutulmaz kılar.
        //
        // ANLAMI TEK CÜMLE: eylemi yapan taraf şu an eylem yapamaz. Üç ayrı
        // sebebi birden kapsar — hareket eden düşmüş, saldıran düşmüş, sırası
        // değil. Kapsaması bilinçli: çağıranın dallanması üçünde de aynıdır,
        // çünkü hedef hücreyi değiştirmek hiçbirinde yardım etmez. Ret sebebi,
        // çağıranın YAPABİLECEĞİ bir şeyi göstermelidir; burada yapılabilecek
        // tek şey beklemek ya da başka bir birim seçmektir.
        //
        // Neden AttackOutcome'daki değerle AYNI AD: iki enum ayrı ama çağıranın
        // sorusu tek — "bu birim şu an eyleyebilir mi". İki farklı ad, aynı
        // cevabı iki kez öğrenmek zorunda bırakırdı.
        //
        // EŞİK — bu değer ne zaman İKİYE ayrılır: arayüz oyuncuya "sıran değil"
        // ile "birim düşmüş" arasındaki farkı SÖYLEMEK zorunda kaldığı gün. O
        // gün ayrım Battle katmanına kendi sonuç tipiyle iner. Bugün
        // ayrılmıyor, çünkü tek tüketici BoardAdapter ve o yalnızca log basıyor.
        //
        // REDDEDILEN - MoveOutcome.cs:107 yerine (bu değer hiç doğmaz;
        //              BattleActions kendi sarmalayıcı sonuç tipini alır):
        //     public enum BattleRejection { None, NotYourTurn, ActorCannotAct }
        //     public readonly struct BattleMoveOutcome { Rejection; Core; }
        // KIRILAN  : bugün hiçbir çağıran o ikili ayrımı KULLANMIYOR.
        //            her test outcome.Core yazar -> Rejection sütunu hep None
        //            reddedilmiş hamlede         -> Core ne olacak, cevabı tip vermez
        //            derleyici: hiçbir şey der  .  test: hepsi bilgisizce değişir
        // KAZANIRDI: yukarıdaki EŞİK aşıldığı gün — arayüz iki sebebi ayrı ayrı
        //            söylemek zorunda kaldığında ikili şekil zaten doğru cevaptır ve
        //            o gün Core'un enum'u bu değeri geri verebilir.
        // TEK CUMLE: Bugün hiçbir çağıranın sormadığı bir ayrımı tipe yazmak, her
        //            çağrıda anlamsız kalan bir alan üretir.
        /// <summary>
        /// Hareket eden şu an eylem yapamaz: sırası değil ya da durumu
        /// elvermiyor (<c>MovementRules.CanMove</c> — bu tipin GÖREMEDİĞİ bir
        /// kural). Bu değeri yalnızca <c>GridStrategy.Battle</c> katmanı üretir.
        /// </summary>
        RejectedActorCannotAct
    }
}
