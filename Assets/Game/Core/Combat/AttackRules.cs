namespace GridStrategy.Combat
{
    // ═══ ROL: KURAL (Policy) ═════════════════════════════════════════
    // kimlik : yok — static; iki çağrıyı ayıracak bir şey yoktur
    // hafıza : yok — aynı durum her zaman aynı cevabı verir
    // Unity  : gerekmez
    // karar  : UYGUNLUK söyler; ne vurur ne hasar uygular
    /// <summary>
    /// "Bu birim saldırabilir mi?" sorusunun tek sahibi.
    ///
    /// Bu tipin var olma sebebi ölçülmüş bir boşluktu: <see cref="TargetingRules"/>
    /// "kime vurulur"u, <see cref="MovementRules"/> "kim yürür"ü sahiplendi;
    /// "kim VURUR" sorusunu ise kimse cevaplamıyordu. Sonucu bir oynanış
    /// hatasıydı — düşmüş bir birim hâlâ vurabiliyordu, çünkü
    /// <see cref="AttackAction"/> hedefin durumunu soruyor, SALDIRANIN durumunu
    /// sormuyordu.
    ///
    /// Neden <see cref="MovementRules"/>'a ikinci bir metot DEĞİL: projenin
    /// kendi yerleşik örneği <see cref="DamageRules"/> / <see cref="HealingRules"/>.
    /// İkisi de tek metotlu, ikisi de "ne kadar" ailesinden ve ikisi de AYRI
    /// dosyalar. Kural başına bir sahip; aynı ailedendir diye bir araya
    /// getirilen iki kural, biri değiştiğinde diğerini de düzenlemek zorunda
    /// bırakır ve dosyanın adı hangisinin sahibi olduğunu söylemez olur.
    ///
    /// Neyi BİLMEZ: hedefin kim olduğunu (<see cref="TargetingRules"/>'ın işi),
    /// menzili (<see cref="AttackResolver"/>'ın işi), sıranın kimde olduğunu
    /// (TurnRules'ın işi, ve o kural bir üst katmanda yaşıyor).
    /// </summary>
    public static class AttackRules
    {
        /// <summary>
        /// Saldırabilir mi? Yalnızca <see cref="UnitState.Alive"/>.
        ///
        /// <see cref="UnitState.Downed"/> için cevap HAYIR ve bu
        /// <see cref="TargetingRules.CanBeAttacked(UnitState)"/> ile bilerek
        /// çelişir: düşmüş birim hâlâ geçerli bir HEDEFTİR ama artık bir
        /// SALDIRAN değildir. Aynı asimetri hareket tarafında da var
        /// (<see cref="MovementRules.CanMove"/>) ve sebebi tektir — düşmek
        /// oyundan çıkmak değil, OYNAMAYI bırakmaktır.
        ///
        /// <see cref="UnitState.Dead"/> için cevap da HAYIR, ama sebebi farklı:
        /// Downed bir kural gereği durdurulur, Dead zaten oyunda değildir. İki
        /// HAYIR'ın aynı olması bir tesadüftür, bir kural değil.
        /// </summary>
        // Beyaz liste biçimi — MovementRules.CanMove ve
        // TargetingRules.CanBeRevived ile aynı şekil, aynı gerekçe. Dördüncü
        // bir durum (Stunned, Petrified, Fleeing) eklendiği gün bu metot onu
        // SESSİZCE saldırgan saymaz; kararı yeniden vermek için buraya gelinir.
        //
        // REDDEDILEN - AttackRules.cs:73 yerine (kara liste biçimi):
        //     return state != UnitState.Downed && state != UnitState.Dead;
        // KIRILAN  : gerekçenin tamamı MovementRules'taki aynı reddin içinde yazılı;
        //            buradaki FARK yalnızca yüklem — yeni durum varsayılan olarak
        //            YÜRÜYEBİLİR değil SALDIRABİLİR sayılır ve sersemletilmiş
        //            birim vurmaya devam eder.
        //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
        // KAZANIRDI: MovementRules'takiyle aynı eşik — saldırabilenler ÇOĞUNLUK olsaydı.
        // TEK CUMLE: Üç kuralda tekrarlanan şey BİÇİMdir, gerekçe değil; gerekçe
        //            tek evde durur.
        //
        // REDDEDILEN - AttackRules.cs:73 yerine (kural kendi başına yazılmaz,
        //              hareket kuralından DEVRALINIR):
        //     return MovementRules.CanMove(state);
        // KIRILAN  : türetme reddinin tamamı MovementRules'ta yazılı; buradaki
        //            FARK, ayrılma gününün oyun diliyle adının konmuş olması —
        //            "yaralı birim yürüyemez ama vurabilir" dendiği gün saldırı
        //            kuralı hareket kuralının kuyruğunda sessizce sürüklenir.
        //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
        // KAZANIRDI: tasarım "eyleyebilmek" diye TEK bir kavrama inseydi — o gün
        //            adı ne CanMove ne CanAttack olurdu.
        // TEK CUMLE: Türetme, iki kuralın ayrıştığı günü hiçbir test kırmadan
        //            geçirir.
        public static bool CanAttack(UnitState attackerState)
        {
            return attackerState == UnitState.Alive;
        }

        // TAKIM AŞIRI YÜKLEMESİ BİLEREK YOK. "Kime vurabilirim" iki tarafın
        // sorusudur ve onun sahibi zaten var: TargetingRules'ın üç parametreli
        // CanBeAttacked'ı. "Ben vurabilir miyim" ise tek taraflıdır — S-15'in
        // hareket için verdiği kararın birebir ikizi.
        //
        // REDDEDILEN - AttackRules.cs:75 yerine:
        //     public static bool CanAttack(UnitState attackerState, Team attackerTeam)
        //     {
        //         if (attackerTeam == Team.None) { return false; }
        //         return CanAttack(attackerState);
        //     }
        // KIRILAN  : takım aşırı yüklemesi reddinin tamamı MovementRules'ta; buradaki
        //            FARK, ikinci evin TurnRules değil TargetingRules.IsHostilePairing
        //            olması — tarafsız ama SALDIRGAN şeyler (yaban canavarı, herkese
        //            ateş eden nötr kule) eklendiği gün orası güncellenir, burası kalır.
        //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
        // KAZANIRDI: saldıranın durumu ile tarafı TEK bir soruda birleşseydi —
        //            "bu birim bu tur zaten vurdu mu" da aynı yerden gelseydi.
        // TEK CUMLE: "Kime vurabilirim" iki tarafın sorusudur ve sahibi başkası;
        //            "ben vurabilir miyim" tek taraflıdır ve sahibi burası.
    }
}
