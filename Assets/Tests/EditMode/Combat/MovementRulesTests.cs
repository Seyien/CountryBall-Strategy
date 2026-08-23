using System.Linq;
using System.Reflection;
using NUnit.Framework;
using GridStrategy.Combat;

namespace GridStrategy.Tests.EditMode.Combat
{
    /// <summary>
    /// Üç durumun her biri tek bir soruya cevap veriyor, dolayısıyla matris
    /// tek satırlı: durum düzlemi var, takım düzlemi YOK. Boşluk bırakılmıyor
    /// çünkü boş bırakılan hücre, ileride birinin varsayım yürüteceği
    /// hücredir.
    ///
    /// Dosyanın ikinci işi bir KARŞILAŞTIRMA: aynı durum, hareket ile
    /// hedefleme sorularına farklı cevap veriyor. O fark bu tipin var olma
    /// sebebidir ve yalnızca burada kırmızı üretebilir.
    /// </summary>
    public sealed class MovementRulesTests
    {
        // REDDEDILEN - MovementRulesTests.cs:39 yerine (kural kendi başına
        //              hiç sınanmaz, yalnızca akış üzerinden sınanır):
        //     [Test] public void Move_DownedUnit_IsRejected()
        //     {
        //         var battle = ...; // düşmüş bir Combatant kurulur
        //         Assert.That(BattleActions.Move(battle, unit, 1, 3, 1),
        //                     Is.Not.EqualTo(MoveOutcome.Moved));
        //     }
        // KIRILAN  : kural kendi başına ölçülemez olur.
        //            akış testi kırmızıya döner -> kural mı yanlış, akış mı
        //            sormayı unuttu; test hangisi olduğunu söyleyemez
        //            o test BattleActionsTests'e yazılırdı -> Battle katmanının
        //            dosyası; kuralın kendi dosyasında hiçbir koruma kalmazdı
        //            derleyici: hiçbir şey der  .  test: yanlış yeri gösterir
        // KAZANIRDI: kuralın TEK çağıranı olsaydı ve o çağıran dışında hiçbir
        //            anlamı olmasaydı — o gün ayrı bir birim testi kuralı değil,
        //            kuralın kopyasını sınardı.
        // TEK CUMLE: Yalnız akış üstünden sınanan bir kural, kırıldığında kimin
        //            kırıldığını söyleyemez.
        [TestCase(UnitState.Alive, ExpectedResult = true)]
        [TestCase(UnitState.Downed, ExpectedResult = false)]  // yerde yatan kaçamaz
        [TestCase(UnitState.Dead, ExpectedResult = false)]    // zaten oyunda değil
        public bool CanMove_AllStates(UnitState state)
        {
            return MovementRules.CanMove(state);
        }

        /// <summary>
        /// ASİMETRİ KARARINI koruyan test — ve bu tipin var olma sebebini tek
        /// satırda gösteren yer. Düşmüş bir birim hâlâ geçerli bir HEDEFTİR
        /// ama artık bir OYUNCU değildir.
        ///
        /// Bu ayrım kasıtlı: düşmüş birime vurmak "işini bitirme" yoludur ve
        /// tasarımın parçasıdır; düşmüş birimin kaçabilmesi ise o tasarımı
        /// anlamsız kılardı — sürünüp giden bir birim hiç düşmemiş sayılır.
        ///
        /// Kırmızıya dönerse biri hareket kuralını hedefleme kuralından
        /// TÜRETMİŞ demektir. Böyle bir kod bugün üç durumda da doğru cevap
        /// verir; kırılma ileride, hedefleme kuralı değiştiği gün ve hiçbir
        /// test kırılmadan gelir.
        /// </summary>
        [Test]
        public void DownedUnit_IsStillAttackableButCannotMove()
        {
            Assert.That(
                TargetingRules.CanBeAttacked(UnitState.Downed),
                Is.True,
                "a downed unit remains a valid target; finishing it off is part of the design");
            Assert.That(
                MovementRules.CanMove(UnitState.Downed),
                Is.False,
                "the same downed unit answers the movement question in the opposite direction");
        }

        /// <summary>
        /// SAHİPLİK KARARINI koruyan test. Hareket kuralının sahibi TEK: bu
        /// tip. "Sıra kimde" sorusu buraya benzer ama buranın işi değildir —
        /// o soru TurnRules'ın ve Battle katmanında yaşıyor.
        ///
        /// Test kuralın ŞEKLİNİ ölçüyor, cevabını değil: tek bir CanMove var
        /// ve tek bir durum parametresi alıyor. Biri
        /// <c>CanMove(UnitState, Team, Team)</c> eklediği gün bu satır kırmızı
        /// olur — o aşırı yükleme hiçbir davranışı bozmaz, yalnızca sıra
        /// kuralını sessizce ikinci bir eve taşır ve TurnRules ile ayrışmaya
        /// başlar.
        /// </summary>
        [Test]
        public void CanMove_TakesStateAloneAndHasNoTeamOverload()
        {
            // ÖDÜNÇ ALINAN — `System.Reflection` (`typeof`, `GetMethods`,
            // `BindingFlags`, `MethodInfo`, `ParameterInfo`) artı `System.Linq`:
            // tipi DERLENDİKTEN SONRA sorgular. Ne çağırır ne de davranış ölçer;
            // aldığı tek şey ŞEKİL — hangi üyeler var, imzaları ne.
            // `BindingFlags` bir SIRA değil bir KÜME filtresidir ve iki çiftten
            // birer üye ZORUNLUDUR: örnek/statik ve açık/gizli. `Static`
            // yazılmasaydı (ve `Instance` da yazılmasaydı) sonuç boş dönerdi —
            // "her ikisi de" değil, "hiçbiri" demektir.
            // `DeclaredOnly` ise bu iddiayı BUGÜN çevirmez: alttaki ad süzgeci
            // `object`ten miras kalan üyeleri zaten eliyor. Kaldırılırsa test
            // yine yeşil kalır; ad süzgeci kaldırılırsa kalmaz.
            // ÖLÇÜ: MovementRules'a ikinci bir CanMove aşırı yüklemesi ekle ve
            // hiçbir gövdeyi değiştirme — aşağıdaki ilk iddia kırmızıya döner,
            // dosyadaki davranış testlerinin hepsi yeşil kalır. Şekil ile
            // davranışın ayrıldığı tek nokta budur.
            //
            // ÖDÜNÇ ALINAN — `nameof(MovementRules.CanMove)`: adı düz metin
            // yerine DERLEYİCİYE yazdırır ve yalnız SON parçayı verir
            // ("CanMove", nitelenmiş ad değil). Metot yeniden adlandırılırsa bu
            // satır derlenmez; `"CanMove"` yazılsaydı liste sessizce boşalır ve
            // uzunluk iddiası 1 yerine 0 görürdü — yani kırmızı olur ama YANLIŞ
            // sebebi gösterirdi.
            // DİL: Docs/deep/dil/03-hata-bildirme-ve-dogrulama.md
            MethodInfo[] canMoveOverloads = typeof(MovementRules)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(method => method.Name == nameof(MovementRules.CanMove))
                .ToArray();

            Assert.That(
                canMoveOverloads.Length,
                Is.EqualTo(1),
                "movement has no second side, so the rule must not grow a team overload");

            ParameterInfo[] parameters = canMoveOverloads[0].GetParameters();

            Assert.That(parameters.Length, Is.EqualTo(1));
            Assert.That(
                parameters[0].ParameterType,
                Is.EqualTo(typeof(UnitState)),
                "the movement rule answers from unit state alone; turn order belongs to the battle layer");
        }
    }
}
