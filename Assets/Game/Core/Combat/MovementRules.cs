namespace GridStrategy.Combat
{
    // ═══ ROL: KURAL (Policy) ═════════════════════════════════════════
    // kimlik : yok — static; iki çağrıyı ayıracak bir şey yoktur
    // hafıza : yok — aynı durum her zaman aynı cevabı verir
    // Unity  : gerekmez
    // karar  : UYGUNLUK söyler; ne taşır ne tahtaya dokunur
    /// <summary>
    /// "Bu birim hareket edebilir mi?" sorusunun tek sahibi.
    ///
    /// Bu tipin var olma sebebi tek cümleydi: <see cref="TargetingRules"/>
    /// "kime vurulur" ve "kim diriltilir" sorularını cevaplıyordu, "kim hareket
    /// eder" sorusunu ise KİMSE cevaplamıyordu — ve sahipsiz kural sessizce en
    /// yanlış yerde, akışın içinde doğuyordu.
    ///
    /// Neden Combat'ta: kural birimin DURUMUNU sormak zorunda ve
    /// <see cref="UnitState"/> burada yaşıyor. Hareket AKIŞI (MoveAction)
    /// Core'da, hareket KURALI burada — çünkü akış tahtayı, kural savaşı tanır.
    ///
    /// GEREKÇELER: Docs/deep/kod/Core/Combat/MovementRules.md
    /// </summary>
    public static class MovementRules
    {
        /// <summary>
        /// Hareket edebilir mi? Yalnızca <see cref="UnitState.Alive"/>.
        ///
        /// <see cref="UnitState.Downed"/> için cevap HAYIR, ve bu
        /// <see cref="TargetingRules.CanBeAttacked(UnitState)"/> ile bilerek
        /// çelişir: düşmüş birim hâlâ geçerli bir HEDEFTİR ama artık bir OYUNCU
        /// değildir.
        ///
        /// <see cref="UnitState.Dead"/> için cevap da HAYIR, ama sebebi farklı:
        /// Downed bir kural gereği durdurulur, Dead zaten oyunda değildir. İki
        /// HAYIR'ın aynı olması bir tesadüftür, bir kural değil.
        /// </summary>
        // BEYAZ LİSTE YENİ DEĞERİ REDDEDER, KARA LİSTE KABUL EDER. `== Alive`
        // biçimi bilerek seçildi: dördüncü bir durum (Stunned, Fleeing) eklendiği
        // gün kara liste onu sessizce YÜRÜYEBİLİR sayardı. Sessizce yanılmayı
        // göze almadığımız taraf yetki tarafıdır; hedeflenebilirlik maruziyettir.
        // → MovementRules.md#canmoveunitstate-state
        //
        // KURALDAN TÜRETME, VERİDEN TÜRETME DEĞİLDİR. Bu satırı TargetingRules'tan
        // türetmek reddedildi: hareket kuralı saldırı kuralının TÜREVİ olurdu ve
        // "düşmüş birime vurulur" değiştiği gün düşmüş birim yürümeye başlardı.
        // Üç eyleyen kuralı bugün aynı satırı taşıyor; bu bir kesişme, bağ değil.
        // → MovementRules.md#canmoveunitstate-state
        public static bool CanMove(UnitState state)
        {
            return state == UnitState.Alive;
        }

        // İKİNCİ PARAMETRE ÇOĞU ZAMAN İKİNCİ BİR KURALDIR. Takım aşırı yüklemesi
        // BİLEREK yok: hareketin ikinci bir tarafı yoktur, birim kendi kendine
        // yürür. `unitTeam != activeTeam` satırı SIRA kuralıdır ve sahibi
        // TurnRules'tır; buraya alınsaydı aynı cümle iki evde yaşardı.
        // → MovementRules.md#canmoveunitstate-team-team
    }
}
