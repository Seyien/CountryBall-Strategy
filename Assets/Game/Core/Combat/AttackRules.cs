namespace GridStrategy.Combat
{
    // ═══ ROL: KURAL (Policy) ═════════════════════════════════════════
    // kimlik : yok — static; iki çağrıyı ayıracak bir şey yoktur
    // hafıza : yok — aynı durum her zaman aynı cevabı verir
    // Unity  : gerekmez — girdi tek enum; CanAttack'i çağırmak için ne sahne
    //          ne kare gerekir
    // karar  : UYGUNLUK söyler; ne vurur ne hasar uygular
    /// <summary>
    /// "Bu birim saldırabilir mi?" sorusunun tek sahibi.
    ///
    /// Var olma sebebi ÖLÇÜLMÜŞ bir boşluktu: <see cref="TargetingRules"/> "kime
    /// vurulur"u, <see cref="MovementRules"/> "kim yürür"ü sahiplendi; "kim
    /// VURUR" sorusunu kimse cevaplamıyordu ve düşmüş bir birim hâlâ
    /// vurabiliyordu.
    ///
    /// İKİ METOT, TEK SORU: savaşçı için <see cref="CanAttack(UnitState)"/>, yapı
    /// için <see cref="CanStructureAttack"/>. Bir kule de vurur ve "kim VURUR"un
    /// tek evi burasıdır.
    ///
    /// Neyi BİLMEZ: hedefin kim olduğunu, menzili, sıranın kimde olduğunu,
    /// saldıranın bir saldırı tanımı taşıyıp taşımadığını.
    ///
    /// GEREKÇELER: Docs/deep/kod/Core/Combat/AttackRules.md
    /// </summary>
    public static class AttackRules
    {
        /// <summary>
        /// Saldırabilir mi? Yalnızca <see cref="UnitState.Alive"/>.
        ///
        /// <see cref="UnitState.Downed"/> için cevap HAYIR ve bu
        /// <see cref="TargetingRules.CanBeAttacked(UnitState)"/> ile bilerek
        /// çelişir: düşmüş birim hâlâ geçerli bir HEDEFTİR ama artık bir
        /// SALDIRAN değildir.
        /// </summary>
        // BEYAZ LİSTE: yalnızca Alive. Kara liste (`!= Downed && != Dead`) bugün
        // AYNI cevabı verir; fark dördüncü değer eklendiği gün doğar — Stunned
        // sessizce saldırgan sayılırdı. Kural MovementRules.CanMove'dan da
        // TÜRETİLMEZ: türetme, iki kuralın ayrıştığı günü hiçbir test kırmadan
        // geçirirdi.
        // → AttackRules.md#canattackunitstate-attackerstate
        // DERİN ANLATIM: Docs/deep/konular/04-karar-sirasi.md
        public static bool CanAttack(UnitState attackerState)
        {
            return attackerState == UnitState.Alive;
        }

        // AYRI AD, AYRI EV DEĞİL: soru aynı ("bu şey vurabilir mi"), girdi tipi
        // farklı, ve ikisi de bu sınıfta duruyor — ayrı bir StructureAttackRules
        // açılsaydı "kim VURUR" sorusunun İKİ evi olurdu ve yarın eklenecek bir
        // sersemletme durumu ikisinden yalnızca birine yazılırdı.
        //
        // ADIN `CanAttack` OLMAMASI BİR TASARIM SEÇİMİ DEĞİL, BİR SÖZÜN BEDELİ:
        // AttackRulesTests içindeki CanAttack_TakesStateAloneAndHasNoTeamOverload
        // yansımayla sayıyor ve tam olarak BİR CanAttack görmek istiyor. O testin
        // koruduğu şey "saldıranın kendi durumu tek taraflı bir sorudur" kararı
        // ve bu metot o kararı hiç zorlamıyor — yine tek taraflı, yine tek
        // parametre. Ayrı ad, testin metnini gevşetmeden kuralı aynı eve koymanın
        // tek yolu; testin sayacı bir gün tipe göre daraltılırsa bu ad tek satırda
        // `CanAttack` aşırı yüklemesine döner.
        /// <summary>
        /// Bir YAPI saldırabilir mi? Yalnızca <see cref="StructureState.Standing"/>.
        ///
        /// OYUNDA NE İŞE YARAR: oyuncu kendi kulesini seçip düşmana tıkladığında
        /// vuruşun olup olmayacağını bu kural belirler; enkaz hâline gelmiş bir
        /// kule artık ateş etmez.
        ///
        /// Bu metot yapının saldırı TANIMINI görmez: profili olmayan bir baraka
        /// da "ayakta"dır. İki soru ayrı durur çünkü ayrı şeyler soruyorlar —
        /// biri yıkılıp yıkılmadığını, öteki hiç silahı olup olmadığını; profil
        /// sorusunun sahibi <see cref="Structure.CanAttack"/>.
        /// </summary>
        // KAPALI UÇLU (`== Standing`), tıpkı TargetingRules'ın yapı sürümü gibi:
        // `!= Destroyed` bugün aynı cevabı verir ve fark yalnızca üçüncü bir
        // durum eklendiği gün doğar — o gün açık uç yeni değeri sessizce
        // SALDIRGAN sayardı.
        public static bool CanStructureAttack(StructureState attackerState)
        {
            return attackerState == StructureState.Standing;
        }

        // TAKIM AŞIRI YÜKLEMESİ BİLEREK YOK: "kime vurabilirim" iki tarafın
        // sorusudur ve sahibi TargetingRules.IsHostilePairing; "ben vurabilir
        // miyim" tek taraflıdır ve sahibi burası. Team parametresi eklenirse bu
        // metot iki taraflı soruların İKİNCİ evi olur ve tarafsız ama SALDIRGAN
        // şeyler geldiği gün hangi evin güncelleneceği belirsizleşir.
        // → AttackRules.md#attackrules-tip
    }
}
