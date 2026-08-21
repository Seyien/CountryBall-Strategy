namespace GridStrategy.Battle
{
    // ═══ ROL: TANIM (Profile) ════════════════════════════════════════
    // kimlik : yok — iki Placed değeri aynı şeydir
    // hafıza : yok — bir değer; kendisi hiçbir şey yapmaz
    // Unity  : gerekmez
    // karar  : vermez — olup biteni ADLANDIRIR; kararı BattleActions verir
    /// <summary>
    /// Bir YERLEŞTİRME denemesinin sonucu.
    ///
    /// ÜÇ DEĞER, VE ÜÇÜ DE OKUNARAK BULUNDU — uydurulmadı. Her biri bugün
    /// gerçekten oluşabilen bir duruma karşılık geliyor:
    /// <list type="bullet">
    /// <item>tahta dışı hücre — fare tahtanın kenarının dışındayken bırakılır;
    /// <c>UnitGrid.PlaceUnit</c> orada gürültüyle patlar, dolayısıyla akış onu
    /// çağırmadan ÖNCE sormak zorunda</item>
    /// <item>dolu hücre — <c>Battle.AddUnit</c> aynı durumu bir ÇAĞIRAN HATASI
    /// sayıyor; burada neden bir oyun sonucu olduğu aşağıda yazılı</item>
    /// <item>yerleşti</item>
    /// </list>
    ///
    /// DÖRDÜNCÜ bir değer (<c>RejectedActorCannotAct</c>) yazılmadı ve bu bir
    /// unutma değil, aşağıdaki REDDEDILEN bloğunun konusu.
    /// </summary>
    public enum PlacementOutcome
    {
        // Sıfırıncı değer BİLEREK bir RET. Gerekçesi MoveOutcome.cs ve
        // AttackOutcome.cs'te yazılı; burada tekrarlanmıyor.
        //
        // ADI NEDEN RejectedInvalidDestination DEĞİL: yerleştirmede giden bir
        // şey yok. "Hedef hücre" bir yolculuğun sonunu anlatır; burada hücre bir
        // varış noktası değil, doğum yeridir. Aynı adı kullanmak iki farklı
        // olguyu tek kelimenin arkasına koyardı ve ileride "yerleştirme de bir
        // hareket midir" diye sorulmasına yol açardı — cevabı hayır, çünkü
        // hareketin bir KAYNAK hücresi vardır.
        /// <summary>Hücre tahtanın dışında.</summary>
        RejectedInvalidCell,

        // AYNI OLGU, İKİ FARKLI CEVAP — ve fark kasıtlı. Battle.AddUnit dolu
        // hücreyi bir ArgumentException ile reddediyor; burada aynı durum bir
        // oyun sonucu. Çelişki değil, ÇAĞIRANIN farkı:
        //
        //     AddUnit         çağıran hücreyi BİLEREK seçer (kayıt dosyası,
        //                     seviye dizilimi, spawn tablosu); dolu hücre onun
        //                     kaydının tahtayla ayrışması demektir
        //     PlaceStructure  çağıran hücreyi FARE ile seçer; dolu hücreye
        //                     tıklamak bozuk bir kayıt değil, sıradan bir hamle
        //
        // Aynı ayrımı MoveAction zaten bir kez yapmıştı: kaynak hücre uyuşmazlığı
        // patlar (kayıt ayrışması), dolu hedef hücre ise MoveOutcome döndürür
        // (sıradan bir hamle).
        //
        // REDDEDILEN - PlacementOutcome.cs:73 yerine (bu enum hiç doğmaz,
        //              PlaceStructure Battle.AddStructure'ın hatalarını olduğu
        //              gibi geçirir):
        //     public static void PlaceStructure(Battle battle, Unit unit,
        //                                       Structure structure, int x, int y)
        //     {
        //         battle.AddStructure(unit, structure, x, y);
        //     }
        // KIRILAN  : oyuncunun dolu bir hücreye tıklaması bir İSTİSNA olur.
        //            BoardAdapter her bırakmayı try/catch ile sarar -> catch
        //            "hangi hata benim, hangisi oyuncunun" sorusunu mesaj metnine
        //            bakarak cevaplar -> bu enum string karşılaştırmasına dönüşür
        //            derleyici: hiçbir şey der  .  test: yeşil kalır
        // KAZANIRDI: yerleştirme yalnız EDİTÖRDEN yapılsaydı — seviye kurulumu
        //            bir kayıt işidir ve orada dolu hücre gerçekten bozuk veridir;
        //            o gün gürültüyle patlamak doğru davranıştır.
        // TEK CUMLE: Çağıran hatası ile oyuncu hamlesinin ayrımı
        //            BattleActions.cs'te yazılı; burada aynı ayrım hücreyi KİMİN
        //            seçtiğine bakılarak uygulanıyor.
        /// <summary>Hücrede zaten bir şey duruyor.</summary>
        RejectedCellOccupied,

        // DÖRDÜNCÜ DEĞER YAZILMADI — ve "sıra kimde" sorusunun burada
        // sorulmaması bir eksiklik değil, bir karardır.
        //
        // REDDEDILEN - PlacementOutcome.cs:100 yerine (sıra kuralı yerleştirmeye
        //              de uygulanır ve dördüncü bir değer doğar):
        //     RejectedActorCannotAct
        //     // ve BattleActions.PlaceStructure içinde:
        //     if (!TurnRules.CanAct(structure.Team, battle.Turn.Current))
        //     {
        //         return PlacementOutcome.RejectedActorCannotAct;
        //     }
        // KIRILAN  : imzada EYLEYEN yok; satır, yapının tarafını eyleyenin
        //            tarafı sanıyor. Structure.Team bilerek Team.None olabilir
        //            (tarafsız duvar, kapı) -> tarafsız hiçbir sırada eyleyemez
        //            -> nötr hiçbir yapı tahtaya bir daha konamaz
        //            derleyici: hiçbir şey  .  test: PlaceStructure_Neutral
        //            StructureOutOfTurn_IsStillPlaced kırmızı
        // KAZANIRDI: yerleştirme gerçekten bir TUR EYLEMİ olduğu gün — sınırlı
        //            inşa hakkı, kaynak maliyeti, "turda bir bina" kuralı; o gün
        //            imza gerçek bir eyleyen alır (Unit builder ya da Team
        //            placingTeam) ve kural o eyleyene sorulur.
        // TEK CUMLE: Olmayan özneyi bir başkasının tarafından ödünç almak, doğru
        //            kuralı yanlış şeye sormaktır — S-15'in cümlesi burada birebir
        //            geçerli.
        /// <summary>Yapı tahtaya kondu ve savaşa katıldı.</summary>
        Placed
    }
}
