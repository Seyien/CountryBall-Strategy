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

        // DÖRDÜNCÜ DEĞER YAZILMADI: bu imzada sorulacak bir EYLEYEN yok, ve
        // sıra kuralını yapının tarafına sormak tarafsız hiçbir yapının bir daha
        // tahtaya konamaması demekti. Üçlü bugün KAPALI bir küme — her değerin
        // tam olarak bir üreteni var. → PlacementOutcome.md#placed
        /// <summary>Yapı tahtaya kondu ve savaşa katıldı.</summary>
        Placed
    }
}
