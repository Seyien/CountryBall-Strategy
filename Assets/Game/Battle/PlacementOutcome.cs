namespace GridStrategy.Battle
{
    // ═══ ROL: TANIM (Profile) ════════════════════════════════════════
    // kimlik : yok — iki Placed değeri aynı şeydir
    // hafıza : yok — ölçüsü şu: PlaceStructure'ın döndürdüğü Placed'i bir
    //          değişkene al, sonra savaşı istediğin kadar değiştir — hücreyi
    //          doldur, yapıyı yık, sırayı devret — değişken hâlâ Placed'dir.
    //          Değişen tek şey BİR SONRAKİ PlaceStructure çağrısının
    //          döndüreceği değerdir: aynı hücreye ikinci çağrı
    //          RejectedCellOccupied döner, çünkü hücreyi birinci çağrı doldurdu
    // Unity  : gerekmez
    // karar  : vermez — olup biteni ADLANDIRIR; kararı BattleActions verir
    /// <summary>
    /// Bir YERLEŞTİRME denemesinin sonucu.
    ///
    /// ÜÇ DEĞER, VE ÜÇÜ DE OKUNARAK BULUNDU — uydurulmadı; her biri bugün
    /// gerçekten oluşabilen bir duruma karşılık geliyor. DÖRDÜNCÜ bir değer
    /// (<c>RejectedActorCannotAct</c>) yazılmadı ve bu bir unutma değil, bir
    /// karar.
    ///
    /// GEREKÇELER: Docs/deep/kod/Battle/PlacementOutcome.md
    /// </summary>
    // DERİN ANLATIM: Docs/deep/konular/06-sonuc-enumlari.md
    public enum PlacementOutcome
    {
        // Sıfırıncı değer BİLEREK bir RET; gerekçesi MoveOutcome ve AttackOutcome
        // tiplerinde yazılı. Adı RejectedInvalidDestination DEĞİL çünkü
        // yerleştirmede giden bir şey yok: hücre bir varış noktası değil, doğum
        // yeridir — hareketin bir KAYNAK hücresi vardır, bunun yok.
        // → PlacementOutcome.md#rejectedinvalidcell
        /// <summary>Hücre tahtanın dışında.</summary>
        RejectedInvalidCell,

        // AYNI OLGU, İKİ FARKLI CEVAP — ve fark kasıtlı: Battle.AddUnit dolu
        // hücreyi bir istisnayla reddediyor, burada aynı durum bir oyun sonucu.
        // Ayrım olguda değil hücreyi SEÇENDE: orada kayıt seçer, burada fare.
        // → PlacementOutcome.md#rejectedcelloccupied
        /// <summary>Hücrede zaten bir şey duruyor.</summary>
        RejectedCellOccupied,

        // ██ DÖRDÜNCÜ DEĞER GELDİ — VE GEREKÇESİ ÜÇÜNCÜNÜNKİYLE AYNI ██
        // Üstteki blok "RejectedActorCannotAct yazılmadı" diyordu ve o karar
        // DEĞİŞMEDİ: sıra kuralı hâlâ bu imzada sorulmuyor. Gelen değer başka
        // bir soruya ait — bir OYUN kuralına, bir sıra kuralına değil.
        // ÖLÇÜ KORUNUYOR: dörtlü de KAPALI bir küme, her değerin tam olarak bir
        // üreteni var ve bu değerin üreteni BattleActions.PlaceStructure.
        /// <summary>Bu takımın ayakta bir ana kulesi zaten var.</summary>
        // RET, İSTİSNA DEĞİL — ve ayrım ölçütü değişmedi: bu cevabı alan çağıran
        // YAPACAK BİR ŞEY bulabiliyor mu? Bulabiliyor — oyuncuya "önce eskisini
        // yık" der ve oyuncu başka bir bina seçer. Yani bu bir oyun olgusudur,
        // bir çağıran hatası değil.
        RejectedHeadquartersExists,

        /// <summary>Yapı tahtaya kondu ve savaşa katıldı.</summary>
        Placed
    }
}
