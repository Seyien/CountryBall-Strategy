using NUnit.Framework;
using GridStrategy.Combat;

namespace GridStrategy.Tests.EditMode.Combat
{
    /// <summary>
    /// Uc durumun her biri, iki yetenege ayri ayri cevap verir. Tam matris
    /// yaziliyor cunku bosluk birakilan hucre, ileride birinin varsayim
    /// yurutecegi hucredir.
    /// </summary>
    public sealed class TargetingRulesTests
    {
        [TestCase(UnitState.Alive, ExpectedResult = true)]
        [TestCase(UnitState.Downed, ExpectedResult = true)]   // isini bitirme yolu
        [TestCase(UnitState.Dead, ExpectedResult = false)]
        public bool CanBeAttacked_AllStates(UnitState state)
        {
            return TargetingRules.CanBeAttacked(state);
        }

        [TestCase(UnitState.Alive, ExpectedResult = false)]   // zaten ayakta
        [TestCase(UnitState.Downed, ExpectedResult = true)]
        [TestCase(UnitState.Dead, ExpectedResult = false)]    // kalici olum kalici
        public bool CanBeRevived_AllStates(UnitState state)
        {
            return TargetingRules.CanBeRevived(state);
        }

        // Iki yetenegin hedef kumeleri AYNI degil. Bu test o farki sabitliyor:
        // biri ileride ikisini tek metoda birlestirmeye kalkarsa kirmizi olur.
        [Test]
        public void Downed_IsTheOnlyStateBothAbilitiesAccept()
        {
            Assert.That(TargetingRules.CanBeAttacked(UnitState.Downed), Is.True);
            Assert.That(TargetingRules.CanBeRevived(UnitState.Downed), Is.True);

            Assert.That(TargetingRules.CanBeAttacked(UnitState.Alive), Is.True);
            Assert.That(TargetingRules.CanBeRevived(UnitState.Alive), Is.False,
                "Ayni durum, farkli yetenege farkli cevap.");
        }
    }
}
