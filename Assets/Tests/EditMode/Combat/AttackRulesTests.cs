using System.Linq;
using System.Reflection;
using NUnit.Framework;
using GridStrategy.Combat;

namespace GridStrategy.Tests.EditMode.Combat
{
    /// <summary>
    /// Üç durumun her biri tek bir soruya cevap veriyor, dolayısıyla matris tek
    /// satırlı: durum düzlemi var, takım düzlemi YOK. Boşluk bırakılmıyor çünkü
    /// boş bırakılan hücre, ileride birinin varsayım yürüteceği hücredir.
    ///
    /// Dosyanın ikinci işi bir KARŞILAŞTIRMA: aynı durum, saldırı ile hedefleme
    /// sorularına farklı cevap veriyor. O fark bu tipin var olma sebebidir —
    /// düşmüş birim hâlâ vurulur ama artık vuramaz — ve yalnızca burada kırmızı
    /// üretebilir.
    ///
    /// Üçüncü işi bir KAYIT: bu kural ile <see cref="MovementRules.CanMove"/>
    /// bugün üç durumda da AYNI cevabı veriyor. O tesadüf yazılı hâle
    /// getiriliyor, çünkü yazılmadığı gün birinin ikisini tek metoda katlaması
    /// an meselesidir.
    /// </summary>
    public sealed class AttackRulesTests
    {
        [TestCase(UnitState.Alive, ExpectedResult = true)]
        [TestCase(UnitState.Downed, ExpectedResult = false)]  // yerde yatan vuramaz
        [TestCase(UnitState.Dead, ExpectedResult = false)]    // zaten oyunda değil
        public bool CanAttack_AllStates(UnitState state)
        {
            return AttackRules.CanAttack(state);
        }

        /// <summary>
        /// ASİMETRİ KARARINI koruyan test — ve bu tipin var olma sebebini tek
        /// satırda gösteren yer. Düşmüş bir birim hâlâ geçerli bir HEDEFTİR ama
        /// artık bir SALDIRAN değildir.
        ///
        /// Ayrım kasıtlı: düşmüş birime vurmak "işini bitirme" yoludur ve
        /// tasarımın parçasıdır; düşmüş birimin vurabilmesi ise o tasarımı
        /// anlamsız kılardı — yerden ateş etmeye devam eden bir birim hiç
        /// düşmemiş sayılır.
        ///
        /// Kırmızıya dönerse ya saldırı kuralı hedefleme kuralından TÜRETİLMİŞ
        /// ya da ikisi tek metoda katlanmış demektir. İki durumda da kod
        /// derlenir; kırılan şey iki sorunun ayrı sorular olduğu gerçeğidir.
        /// </summary>
        [Test]
        public void DownedUnit_IsStillAttackableButCannotAttack()
        {
            Assert.That(
                TargetingRules.CanBeAttacked(UnitState.Downed),
                Is.True,
                "a downed unit remains a valid target; finishing it off is part of the design");
            Assert.That(
                AttackRules.CanAttack(UnitState.Downed),
                Is.False,
                "the same downed unit answers the attacking question in the opposite direction");
        }

        /// <summary>
        /// SAHİPLİK KARARINI koruyan test. Saldıranın kendi durumu tek taraflı
        /// bir sorudur; "kime vurabilirim" ise iki taraflıdır ve onun sahibi
        /// zaten var — <see cref="TargetingRules"/>'ın üç parametreli
        /// <c>CanBeAttacked</c>'ı.
        ///
        /// Test kuralın ŞEKLİNİ ölçüyor, cevabını değil: tek bir CanAttack var
        /// ve tek bir durum parametresi alıyor. Biri
        /// <c>CanAttack(UnitState, Team)</c> eklediği gün bu satır kırmızı olur —
        /// o aşırı yükleme hiçbir davranışı bozmaz, yalnızca "tarafsız
        /// saldıramaz" kuralını sessizce ikinci bir eve taşır ve TargetingRules
        /// ile ayrışmaya başlar.
        ///
        /// Şekli aynı gerekçeyle koruyan ikizi
        /// <see cref="MovementRulesTests.CanMove_TakesStateAloneAndHasNoTeamOverload"/>.
        /// </summary>
        [Test]
        public void CanAttack_TakesStateAloneAndHasNoTeamOverload()
        {
            MethodInfo[] canAttackOverloads = typeof(AttackRules)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(method => method.Name == nameof(AttackRules.CanAttack))
                .ToArray();

            Assert.That(
                canAttackOverloads.Length,
                Is.EqualTo(1),
                "the attacker's own state is a one-sided question; the team rule already has an owner");

            ParameterInfo[] parameters = canAttackOverloads[0].GetParameters();

            Assert.That(parameters.Length, Is.EqualTo(1));
            Assert.That(
                parameters[0].ParameterType,
                Is.EqualTo(typeof(UnitState)),
                "the attacking rule answers from unit state alone; sides belong to TargetingRules");
        }

        /// <summary>
        /// KARAKTERİZASYON testi — bir kararı korumuyor, bir TESADÜFÜ kayda
        /// geçiriyor, ve ikisinin farkı burada yazılı olmak zorunda.
        ///
        /// <see cref="AttackRules.CanAttack"/> ile
        /// <see cref="MovementRules.CanMove"/> bugün üç durumun ÜÇÜNDE DE aynı
        /// cevabı veriyor. Tam da bu yüzden ikisini tek metoda katlamak
        /// caziptir — ve tam da bu yüzden yanlıştır: iki kuralın bugün
        /// kesişiyor olması onları aynı kural yapmaz.
        ///
        /// Bu test kırmızıya döndüğü gün YANLIŞ bir şey olmuş değildir: tasarım
        /// ikisini ayırmıştır ("yaralı birim yürüyemez ama vurabilir", "kök
        /// salmış birim vurur ama yürümez"). O gün bu test SİLİNİR, ve silinmesi
        /// kaydın kendisidir — iki kuralın ayrıldığı an bir dosya değişikliği
        /// olarak görünür. Katlama yapılsaydı o an hiçbir yerde görünmezdi.
        /// </summary>
        [TestCase(UnitState.Alive)]
        [TestCase(UnitState.Downed)]
        [TestCase(UnitState.Dead)]
        public void CanAttack_AndCanMove_StillAgree_WhichIsWhyTheyMustStaySeparate(UnitState state)
        {
            Assert.That(
                AttackRules.CanAttack(state),
                Is.EqualTo(MovementRules.CanMove(state)),
                "the two rules coincide today; the day they diverge, delete this test rather than merging them");
        }
    }
}
