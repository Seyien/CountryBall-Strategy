using System.Linq;
using System.Reflection;
using NUnit.Framework;
using GridStrategy.Combat;

namespace GridStrategy.Tests.EditMode.Combat
{
    /// <summary>
    /// Ailenin üçüncü ve son üyesinin testleri. Yapısı kardeşleriyle aynı
    /// (<c>AttackRulesTests</c>, <c>MovementRulesTests</c>) ve bu bilerek: aynı
    /// şekle sahip üç kuralın testleri de aynı şekle sahip olmalı, yoksa
    /// aralarındaki gerçek farkı görmek için üç ayrı okuma biçimi gerekir.
    ///
    /// Dosyanın ikinci işi bir KARŞILAŞTIRMA: aynı durum, "kim kaldırır" ile
    /// "kim kaldırılır" sorularına farklı cevap veriyor. Düşmüş bir birim
    /// diriltilebilir ama diriltemez — ve o fark bu tipin var olma sebebi.
    ///
    /// Üçüncü işi kardeşininkiyle aynı KAYIT: bu kural, <see cref="AttackRules"/>
    /// ve <see cref="MovementRules"/> ile bugün üç durumda da AYNI cevabı
    /// veriyor. Tesadüf yazılı hâle getiriliyor çünkü yazılmadığı gün birinin
    /// üçünü tek metoda katlaması hiçbir testi kırmaz.
    /// </summary>
    public sealed class ReviveRulesTests
    {
        [TestCase(UnitState.Alive, ExpectedResult = true)]
        [TestCase(UnitState.Downed, ExpectedResult = false)]  // yerde yatan kaldıramaz
        [TestCase(UnitState.Dead, ExpectedResult = false)]    // zaten oyunda değil
        public bool CanRevive_AllStates(UnitState state)
        {
            return ReviveRules.CanRevive(state);
        }

        /// <summary>
        /// KARARI KORUYAN test. İki eksen bir arada: <c>Downed</c> bir birim
        /// <see cref="TargetingRules.CanBeRevived(UnitState)"/>'in HEDEFİ olabilir
        /// ama <c>ReviveRules</c>'ın EYLEYENİ olamaz.
        ///
        /// Bu satır, iki kuralı tek metoda katlamayı imkânsız kılan tek ölçümdür:
        /// aynı girdi, iki soruya iki farklı cevap. Katlanırlarsa bu test
        /// kırmızıya döner ve katlama sessiz olamaz.
        /// </summary>
        [Test]
        public void DownedUnit_CanBeRevivedButCannotRevive()
        {
            Assert.That(TargetingRules.CanBeRevived(UnitState.Downed), Is.True,
                "a fallen unit is exactly who revival exists for");
            Assert.That(ReviveRules.CanRevive(UnitState.Downed), Is.False,
                "but a fallen unit cannot be the one doing the lifting");
        }

        /// <summary>
        /// Takım aşırı yüklemesi YOK — ve yokluğu sınanıyor, çünkü bir metodun
        /// yokluğu ancak böyle sabitlenebilir. Gerekçe S-15: diriltmenin iki
        /// tarafı vardır ama TARAF sorusu (dost mu düşman mı kaldırılır)
        /// <see cref="TargetingRules"/>'ın üç parametreli sürümüne ait; bu tip
        /// yalnız EYLEYENİN kendi durumunu bilir.
        /// </summary>
        [Test]
        public void CanRevive_TakesStateAloneAndHasNoTeamOverload()
        {
            MethodInfo[] overloads = typeof(ReviveRules)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(method => method.Name == nameof(ReviveRules.CanRevive))
                .ToArray();

            Assert.That(overloads.Length, Is.EqualTo(1),
                "the team question belongs to TargetingRules, not to the actor rule");
            Assert.That(overloads[0].GetParameters().Single().ParameterType,
                Is.EqualTo(typeof(UnitState)));
        }

        /// <summary>
        /// KARAKTERİZASYON testi — bir kararı değil bir TESADÜFÜ kaydediyor.
        ///
        /// Üç eyleyen kuralı bugün aynı satırı taşıyor. Bu test onların
        /// ayrıldığı gün kırmızıya döner ve o kırmızı bir HATA DEĞİLDİR: silinir,
        /// ve silinmesi ayrımın gerçekleştiğinin kaydı olur. Aynı deseni
        /// <c>AttackRulesTests</c> ikili olarak taşıyor; bu üçlü sürümü.
        ///
        /// Neden yazılıyor: yazılmadığı gün birinin üçünü tek metoda katlaması
        /// hiçbir testi kırmaz — ve katlama, ayrılmaları gereken günü hiçbir
        /// test kırılmadan kaçırmanın tek yolu.
        /// </summary>
        [TestCase(UnitState.Alive)]
        [TestCase(UnitState.Downed)]
        [TestCase(UnitState.Dead)]
        public void ThreeActorRules_StillAgree_WhichIsWhyTheyMustStaySeparate(UnitState state)
        {
            bool canRevive = ReviveRules.CanRevive(state);

            Assert.That(canRevive, Is.EqualTo(AttackRules.CanAttack(state)),
                "reviving and attacking still answer alike; the day they diverge, delete this case");
            Assert.That(canRevive, Is.EqualTo(MovementRules.CanMove(state)),
                "reviving and moving still answer alike; the day they diverge, delete this case");
        }
    }
}
