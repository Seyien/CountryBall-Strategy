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
    /// "kime vurulur" ve "kim diriltilir" sorularını cevaplıyordu, "kim
    /// hareket eder" sorusunu ise KİMSE cevaplamıyordu — ve sahipsiz kural
    /// sessizce en yanlış yerde, akışın içinde doğuyordu.
    ///
    /// Neden Combat'ta: kural birimin DURUMUNU sormak zorunda ve
    /// <see cref="UnitState"/> burada yaşıyor. Hareket AKIŞI (MoveAction)
    /// Core'da, hareket KURALI burada — çünkü akış tahtayı, kural savaşı
    /// tanır. Core, Combat'ı görmediği için akışın bu kuralı kendi içinden
    /// sorması mümkün değildir; soran taraf BattleActions'tır, tıpkı
    /// <see cref="TargetingRules"/>'ın takımlı sürümünü onun sorması gibi.
    /// </summary>
    public static class MovementRules
    {
        /// <summary>
        /// Hareket edebilir mi? Yalnızca <see cref="UnitState.Alive"/>.
        ///
        /// <see cref="UnitState.Downed"/> için cevap HAYIR, ve bu
        /// <see cref="TargetingRules.CanBeAttacked(UnitState)"/> ile bilerek çelişir:
        /// düşmüş birim hâlâ geçerli bir HEDEFTİR ama artık bir OYUNCU
        /// değildir. Yerde yatan bir birimin kaçabilmesi, "işini bitirme"
        /// tasarımını anlamsız kılardı — düşman gelene kadar sürünüp giden
        /// bir birim hiç düşmemiş sayılır.
        ///
        /// <see cref="UnitState.Dead"/> için cevap da HAYIR, ama sebebi
        /// farklı: Downed bir kural gereği durdurulur, Dead zaten oyunda
        /// değildir. İki HAYIR'ın aynı olması bir tesadüftür, bir kural
        /// değil.
        /// </summary>
        // Bilinmeyen bir enum değeri karşısında cevap HAYIR — beyaz liste
        // biçimi bilerek seçildi ve TargetingRules.CanBeRevived'ın şekli
        // birebir bu. Dördüncü bir durum (Fleeing, Stunned, Petrified)
        // eklendiği gün bu metot onu SESSİZCE kabul etmez; kararı yeniden
        // vermek için buraya gelinir.
        //
        // REDDEDILEN - MovementRules.cs:72 yerine (kara liste biçimi,
        //              TargetingRules.CanBeAttacked'ın şekli):
        //     return state != UnitState.Downed && state != UnitState.Dead;
        // KIRILAN  : bugünkü üç durumda aynı cevabı verir; kırılan şey İLERİDE olur.
        //            UnitState'e eklenen her yeni durum -> varsayılan YÜRÜYEBİLİR
        //            sersemletilmiş birim eklenir -> hiçbir test kırılmadan yürür
        //            derleyici: hiçbir şey der  .  test: eksik kural ancak oyunda görülür
        // KAZANIRDI: hareket edebilenlerin ÇOĞUNLUK olacağı bir tasarımda — on
        //            durumun sekizi yürüyorsa beyaz liste her yeni durumda
        //            düzenlenir ve asıl kural iki istisnanın altında kaybolur.
        // TEK CUMLE: Beyaz liste yeni değeri REDDEDER, kara liste KABUL eder;
        //            sessizce yanılmak istemediğimiz taraf yetki tarafıdır.
        //
        // REDDEDILEN - MovementRules.cs:72 yerine (kural kendi başına
        //              yazılmaz, hedefleme kuralından TÜRETİLİR):
        //     return TargetingRules.CanBeAttacked(state)
        //            && !TargetingRules.CanBeRevived(state);
        // KIRILAN  : bugün ÜÇ durumda da doğru cevabı verir — tehlikeli olan tam bu.
        //            hareket kuralı artık saldırı kuralının TÜREVİdir
        //            "düşmüş birime vurulur" değişir -> düşmüş birim yürümeye başlar
        //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
        // KAZANIRDI: hareket gerçekten "hedeflenebilir ama diriltilemez" olmanın
        //            TANIMI olsaydı — o gün üç metot değil bir metot olurdu ve
        //            adı CanMove olmazdı.
        // TEK CUMLE: İki kuralın bugün kesişmesi onları aynı kural yapmaz;
        //            türetme, kesişmenin bittiği günü sessiz kılar.
        public static bool CanMove(UnitState state)
        {
            return state == UnitState.Alive;
        }

        // TAKIM AŞIRI YÜKLEMESİ BİLEREK YOK — ve TargetingRules'la arasındaki
        // bu asimetri öğreticidir. Hedeflemenin takımlı sürümü var çünkü "kime
        // saldırabilirim" İKİ TARAFIN sorusudur: bir saldıran, bir hedef.
        // Hareketin ikinci tarafı yoktur; birim kendi kendine yürür.
        //
        // "Sıra kimde" sorusu buraya benzeyebilir ama buranın işi değildir:
        // o soru TurnRules'ın ve Battle katmanında yaşıyor.
        //
        // REDDEDILEN - MovementRules.cs:74 yerine:
        //     public static bool CanMove(UnitState state, Team unitTeam, Team activeTeam)
        //     {
        //         if (unitTeam == Team.None) { return false; }
        //         if (unitTeam != activeTeam) { return false; }
        //         return CanMove(state);
        //     }
        // KIRILAN  : ikinci satır SIRA kuralıdır ve sahibi TurnRules'tır; kural
        //            o an iki evde birden yaşar.
        //            TurnRules'a "aynı takım üst üste oynar" istisnası eklenir
        //            -> burası haberdar olmaz, iki cevap ayrışır
        //            Team parametresi Battle'ın bilgisini Combat'a taşır
        //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
        // KAZANIRDI: hareketin gerçekten İKİNCİ BİR TARAFI olsaydı — itme,
        //            sürükleme, zorla yer değiştirme. O gün cevap sıra kuralı
        //            değil sahiplik kuralı olurdu.
        // TEK CUMLE: Bir metoda ikinci parametre eklemek çoğu zaman ikinci bir
        //            KURALI eve almaktır; kural başına bir sahip.
    }
}
