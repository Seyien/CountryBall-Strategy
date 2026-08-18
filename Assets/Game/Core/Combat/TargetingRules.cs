using System;

namespace GridStrategy.Combat
{
    // ═══ ROL: KURAL (Policy) ═════════════════════════════════════════
    // kimlik : yok — static; iki çağrıyı ayıracak bir şey yoktur
    // hafıza : yok — aynı durum her zaman aynı cevabı verir
    // Unity  : gerekmez
    // karar  : UYGUNLUK söyler; ne saldırır ne iyileştirir
    /// <summary>
    /// "Bu yetenek bu hedefe uygulanabilir mi?" sorusunun tek sahibi.
    ///
    /// Bu kural üç kez reddedildi ve her seferinde gerekçesi aynıydı:
    /// <see cref="Health"/> hedefin ne olduğunu bilmemeli, <see cref="UnitLifecycle"/>
    /// kimin saldırdığını bilmemeli, <see cref="AttackResolver"/> yalnızca mesafe
    /// ölçmeli. Geriye ÜÇÜNCÜ bir sahip kaldı — burası.
    ///
    /// Neden iki ayrı metot: aynı hedef, soran yeteneğe göre FARKLI cevap verir.
    /// Düşmüş bir birim hem vurulabilir hem diriltilebilir; ölü birim ikisine de
    /// kapalıdır. Tek bir "uygun mu" metodu bu ayrımı taşıyamazdı.
    /// </summary>
    public static class TargetingRules
    {
        /// <summary>
        /// Saldırı hedefleyebilir mi? <see cref="UnitState.Downed"/> dahildir —
        /// düşmüş birime vurmak "işini bitirme" yoludur ve tasarımın parçasıdır.
        /// Buraya Downed'ı kapatan bir satır koymak, ilk turda reddettiğimiz
        /// `if (!IsAlive) return;` hatasının aynısı olurdu.
        /// </summary>
        public static bool CanBeAttacked(UnitState state)
        {
            return state != UnitState.Dead;
        }

        /// <summary>
        /// Diriltme hedefleyebilir mi? Yalnızca <see cref="UnitState.Downed"/>:
        /// ayakta olanın diriltmeye ihtiyacı yok, kalıcı ölü artık kurtarılamaz.
        /// </summary>
        public static bool CanBeRevived(UnitState state)
        {
            return state == UnitState.Downed;
        }
    }
}
