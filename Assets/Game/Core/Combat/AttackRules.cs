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
    /// Neyi BİLMEZ: hedefin kim olduğunu, menzili, sıranın kimde olduğunu.
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

        // TAKIM AŞIRI YÜKLEMESİ BİLEREK YOK: "kime vurabilirim" iki tarafın
        // sorusudur ve sahibi TargetingRules.IsHostilePairing; "ben vurabilir
        // miyim" tek taraflıdır ve sahibi burası. Team parametresi eklenirse bu
        // metot iki taraflı soruların İKİNCİ evi olur ve tarafsız ama SALDIRGAN
        // şeyler geldiği gün hangi evin güncelleneceği belirsizleşir.
        // → AttackRules.md#attackrules-tip
    }
}
