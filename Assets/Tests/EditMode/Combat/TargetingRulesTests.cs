using NUnit.Framework;
using GridStrategy.Combat;

namespace GridStrategy.Tests.EditMode.Combat
{
    /// <summary>
    /// Üç durumun her biri, iki yeteneğe ayrı ayrı cevap verir. Tam matris
    /// yazılıyor çünkü boşluk bırakılan hücre, ileride birinin varsayım
    /// yürüteceği hücredir.
    ///
    /// Takım eklendiğinde matris İKİ DÜZLEME ayrıldı, bir çarpıma değil: durum
    /// düzlemi (tek parametreli sürümler) ve takım düzlemi (üç parametreli
    /// sürümler). İki düzlemin birleştiği yerde de birer test var — ama tam
    /// çarpım yazılmadı, sebebi aşağıdaki REDDEDILEN bloğunda.
    ///
    /// Yapılar ÜÇÜNCÜ bir düzlem açtı, dördüncü bir çarpım değil: aynı iki soru
    /// (durum, takım) bu kez <see cref="StructureState"/> ile soruluyor. Yapı
    /// düzleminin birim düzleminden farkı iki testte toplanmış — kuralın ŞEKLİ
    /// (açık uçlu / kapalı uçlu) ve DİRİLTME İKİZİNİN YOKLUĞU. Geri kalan her
    /// satır aynı cevabı veriyor ve bu bir tekrar değil, "kural tek sahipten
    /// geliyor" iddiasının sınanması.
    /// </summary>
    public sealed class TargetingRulesTests
    {
        // Alternatif: yalnız Dead için tek bir [Test]. Seçilmedi: iki yeteneğe
        // FARKLI cevap veren tek durum olan Downed sınanmadan kalır ve "düşmüşe
        // vurmak işini bitirme yoludur" kararını silen bir değişiklik hiçbir
        // satırı kırmızıya döndürmez.
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

        // Takım düzlemi TAM yazılıyor (dokuz hücre), ama tek bir durumda:
        // Alive. Durum ile takım çarpılmıyor çünkü kural çarpmıyor — takım
        // kontrolü durumdan önce ve ondan bağımsız çalışıyor, durum kuralı da
        // kopyalanmıyor.
        //
        // REDDEDILEN - TargetingRulesTests.cs:68 yerine:
        //     [TestCase(UnitState.Alive, Team.Player, Team.Player, ExpectedResult = false)]
        //     [TestCase(UnitState.Downed, Team.Player, Team.Player, ExpectedResult = false)]
        //     ... 27 satır: üç durum × üç saldıran × üç hedef
        // KIRILAN  : matris ölçtüğü şeyi kaybeder.
        //            27 satırın 18'i aynı iki erken dönüşü (tarafsız saldıran,
        //            aynı takım) tekrar eder -> gerçek sınır, durum ile takım
        //            kuralının birleştiği tek nokta, 27 satırın içinde kaybolur
        //            derleyici: hiçbir şey der  .  test: 27 satır da yeşil olur,
        //            ama kural değişince hangisi NEDEN kırmızı okunmaz
        // KAZANIRDI: takım kuralı duruma göre DEĞİŞSEYDİ — "düşmüş düşmana
        //            vurulur ama düşmüş dosta vurulmaz" gibi bir kural girseydi;
        //            o gün çarpım gerçek olurdu.
        // TEK CUMLE: Kural çarpmıyorsa test de çarpmamalı; çarpım, olmayan bir
        //            bağı varmış gibi belgeler.
        [TestCase(Team.Player, Team.Enemy, ExpectedResult = true)]
        [TestCase(Team.Enemy, Team.Player, ExpectedResult = true)]
        [TestCase(Team.Player, Team.Player, ExpectedResult = false)]  // dost ateşi yok
        [TestCase(Team.Enemy, Team.Enemy, ExpectedResult = false)]
        [TestCase(Team.Player, Team.None, ExpectedResult = true)]     // tarafsız hedef açık
        [TestCase(Team.Enemy, Team.None, ExpectedResult = true)]
        [TestCase(Team.None, Team.Player, ExpectedResult = false)]    // tarafsız saldıramaz
        [TestCase(Team.None, Team.Enemy, ExpectedResult = false)]
        [TestCase(Team.None, Team.None, ExpectedResult = false)]
        public bool CanBeAttacked_TeamPlane_OnAliveTarget(Team attackerTeam, Team targetTeam)
        {
            return TargetingRules.CanBeAttacked(UnitState.Alive, attackerTeam, targetTeam);
        }

        // Diriltme düzlemi saldırının AYNASI değil: burada AYNI takım geçerli,
        // farklı takım değil — ve Team.None her iki tarafta da kapalı.
        [TestCase(Team.Player, Team.Player, ExpectedResult = true)]
        [TestCase(Team.Enemy, Team.Enemy, ExpectedResult = true)]
        [TestCase(Team.Player, Team.Enemy, ExpectedResult = false)]   // düşmanını kaldırmazsın
        [TestCase(Team.Enemy, Team.Player, ExpectedResult = false)]
        [TestCase(Team.Player, Team.None, ExpectedResult = false)]    // duvar diriltilmez
        [TestCase(Team.Enemy, Team.None, ExpectedResult = false)]
        [TestCase(Team.None, Team.Player, ExpectedResult = false)]    // duvar diriltmez
        [TestCase(Team.None, Team.Enemy, ExpectedResult = false)]
        [TestCase(Team.None, Team.None, ExpectedResult = false)]
        public bool CanBeRevived_TeamPlane_OnDownedTarget(Team reviverTeam, Team targetTeam)
        {
            return TargetingRules.CanBeRevived(UnitState.Downed, reviverTeam, targetTeam);
        }

        // İki düzlemin birleştiği nokta: takım kuralı durum kuralını EZMEZ.
        // Bu iki satır olmasaydı, üç parametreli sürümün durum kuralını hiç
        // sormadığı (ya da kopyaladığı) fark edilmezdi.
        [Test]
        public void TeamRule_DoesNotOverrideStateRule()
        {
            Assert.That(TargetingRules.CanBeAttacked(UnitState.Dead, Team.Player, Team.Enemy), Is.False,
                "a dead enemy is still an invalid attack target");
            Assert.That(TargetingRules.CanBeRevived(UnitState.Alive, Team.Player, Team.Player), Is.False,
                "a standing ally does not need reviving, regardless of team");
        }

        /// <summary>
        /// TARAFSIZ HEDEF KARARINI koruyan test — bir davranışı değil bir KARARI
        /// koruyor. Team.None hedefe saldırılabilir olması bir tasarım seçimiydi:
        /// None'ın var olma sebebi yıkılabilir duvar ve nötr kaynak düğümü, ve
        /// yıkılamayan duvar dekordur.
        ///
        /// Kırmızıya dönerse biri tarafsızı "kimseye kapalı" yapmış demektir. O
        /// değişiklik hiçbir şeyi patlatmaz, sadece tarafsız her şey sonsuza dek
        /// geçersiz hedef olur ve yolu duvarla kesilmiş yapay zekâ çıkış bulamaz.
        /// </summary>
        [Test]
        public void CanBeAttacked_NeutralTarget_IsOpenToEverySide()
        {
            Assert.That(TargetingRules.CanBeAttacked(UnitState.Alive, Team.Player, Team.None), Is.True,
                "a neutral target is attackable by the player side");
            Assert.That(TargetingRules.CanBeAttacked(UnitState.Alive, Team.Enemy, Team.None), Is.True,
                "a neutral target is attackable by the enemy side too; neutral means open, not protected");
        }

        /// <summary>
        /// TARAFSIZ SALDIRAN KARARINI koruyan test. Team.None saldıramaz, ve
        /// sebebi Team.cs'in sıfırı bilerek None yapmasıyla aynı: takımı
        /// atanmayı unutulmuş bir birim hiçbir şeye saldıramaz, HER ŞEYE değil.
        ///
        /// Kırmızıya dönerse takımsız birim herkesi hedef görmeye başlar — kendi
        /// tarafı dahil — ve dost ateşi hatası ancak oyunda görülür.
        /// </summary>
        [Test]
        public void CanBeAttacked_NeutralAttacker_IsRejected()
        {
            Assert.That(TargetingRules.CanBeAttacked(UnitState.Alive, Team.None, Team.Player), Is.False,
                "a unit with no team assigned must fail loudly rather than target everyone");
        }

        /// <summary>
        /// ASİMETRİ KARARINI koruyan test. Aynı tarafsız hedef, iki yeteneğe
        /// TERS cevap veriyor: saldırıya açık, diriltmeye kapalı. Bu kasıtlı —
        /// duvarı yıkmak tasarımın parçası, duvarı ayağa kaldırmak değil.
        ///
        /// Kırmızıya dönerse biri iki kuralı simetrik hâle getirmiş demektir ve
        /// simetri burada yanlış: diriltme AYNI takım ister, saldırı FARKLI.
        /// </summary>
        [Test]
        public void NeutralTarget_IsAttackableButNeverRevivable()
        {
            Assert.That(TargetingRules.CanBeAttacked(UnitState.Downed, Team.Player, Team.None), Is.True);
            Assert.That(TargetingRules.CanBeRevived(UnitState.Downed, Team.Player, Team.None), Is.False,
                "the same neutral target answers the two abilities in opposite directions");
        }

        // İki yeteneğin hedef kümeleri AYNI değil. Bu test o farkı sabitliyor:
        // biri ileride ikisini tek metoda birleştirmeye kalkarsa kırmızı olur.
        [Test]
        public void Downed_IsTheOnlyStateBothAbilitiesAccept()
        {
            Assert.That(TargetingRules.CanBeAttacked(UnitState.Downed), Is.True);
            Assert.That(TargetingRules.CanBeRevived(UnitState.Downed), Is.True);

            Assert.That(TargetingRules.CanBeAttacked(UnitState.Alive), Is.True);
            Assert.That(TargetingRules.CanBeRevived(UnitState.Alive), Is.False,
                "Ayni durum, farkli yetenege farkli cevap.");
        }

        // ─────────────────────────────────────────────────────────────
        // Yapı düzlemi — StructureState
        // ─────────────────────────────────────────────────────────────

        [TestCase(StructureState.Standing, ExpectedResult = true)]
        [TestCase(StructureState.Destroyed, ExpectedResult = false)]
        public bool CanBeAttacked_AllStructureStates(StructureState state)
        {
            return TargetingRules.CanBeAttacked(state);
        }

        /// <summary>
        /// ŞEKİL KARARINI koruyan test: yapı kuralı birim kuralının kopyası
        /// DEĞİL. Birimde kural açık uçlu (<c>state != Dead</c>) çünkü düşmüş
        /// birime vurmak tasarımın parçası; yapıda kapalı uçlu
        /// (<c>state == Standing</c>) çünkü enkazın canı yok.
        ///
        /// İki değerli bir enum'da bu fark ölçülemez — bu yüzden test şekli
        /// DAVRANIŞTAN değil, iki kuralın aynı soruya ters cevap verdiği tek
        /// noktadan okuyor: "ölü ama henüz temizlenmemiş" hâl birimde hedeftir,
        /// yapıda değildir.
        ///
        /// Kırmızıya dönerse biri iki kuralı simetrik hâle getirmiştir ve
        /// simetri burada yanlış.
        /// </summary>
        [Test]
        public void DeadUnitAndDestroyedStructure_AreNotTheSameQuestion()
        {
            Assert.That(TargetingRules.CanBeAttacked(UnitState.Downed), Is.True,
                "a downed unit is still a target; finishing it off is part of the design");
            Assert.That(TargetingRules.CanBeAttacked(StructureState.Destroyed), Is.False,
                "rubble has no health left; a structure has no equivalent of the downed window");
        }

        // Takım düzlemi yapı için de TAM yazılıyor: kural aynı sahipten geliyor
        // ama "aynı sahip" bir iddiadır ve iddia sınanmadan kalırsa, iki aşırı
        // yüklemeden biri sessizce ayrışabilir.
        [TestCase(Team.Player, Team.Enemy, ExpectedResult = true)]
        [TestCase(Team.Enemy, Team.Player, ExpectedResult = true)]
        [TestCase(Team.Player, Team.Player, ExpectedResult = false)]  // kendi barakanı yıkmazsın
        [TestCase(Team.Enemy, Team.Enemy, ExpectedResult = false)]
        [TestCase(Team.Player, Team.None, ExpectedResult = true)]     // tarafsız duvar açık
        [TestCase(Team.Enemy, Team.None, ExpectedResult = true)]
        [TestCase(Team.None, Team.Player, ExpectedResult = false)]    // tarafsız saldıramaz
        [TestCase(Team.None, Team.Enemy, ExpectedResult = false)]
        [TestCase(Team.None, Team.None, ExpectedResult = false)]
        public bool CanBeAttacked_TeamPlane_OnStandingStructure(Team attackerTeam, Team targetTeam)
        {
            return TargetingRules.CanBeAttacked(StructureState.Standing, attackerTeam, targetTeam);
        }

        [Test]
        public void TeamRule_DoesNotOverrideStructureStateRule()
        {
            Assert.That(
                TargetingRules.CanBeAttacked(StructureState.Destroyed, Team.Player, Team.Enemy), Is.False,
                "a destroyed enemy structure is still an invalid attack target");
        }

        /// <summary>
        /// ASİMETRİ KARARINI koruyan test — rol tablosunda birebir eşleşmeyen tek
        /// satır. Birimde iki yetenek var (saldırı + diriltme); yapıda yalnız
        /// biri, ve eksik olan unutulmuş değil YANLIŞ olurdu: yapı dirilmez.
        ///
        /// Bir metodun YOKLUĞU doğrudan sınanamaz, o yüzden test asimetrinin
        /// gözlenebilir yarısını sabitliyor: aynı "artık yok" hâli birimde
        /// diriltmeye AÇIK, yapıda hiçbir yeteneğe açık değil — ve yapıdaki onarım
        /// ön koşulu <see cref="TargetingRules"/>'ta değil
        /// <see cref="Structure.TryRepair"/>'in kendi içinde duruyor.
        ///
        /// Kırmızıya dönerse ya yapıya bir diriltme/onarım hedefleme kuralı
        /// eklenmiş ya da onarımın kelepçesi Structure'dan sökülmüştür.
        /// </summary>
        [Test]
        public void StructureHasNoReviveTwin_RepairKeepsItsOwnPrecondition()
        {
            Assert.That(TargetingRules.CanBeRevived(UnitState.Downed), Is.True,
                "a downed unit can be revived; that is what the downed window is for");

            var destroyed = new Structure(new Health(10), new StructureLifecycle(), Team.Enemy);
            Assert.That(destroyed.TakeDamage(10), Is.True, "kurulum bozuk");

            Assert.That(TargetingRules.CanBeAttacked(destroyed.State), Is.False,
                "a destroyed structure is not an attack target");
            Assert.That(destroyed.TryRepair(5), Is.False,
                "repair is not revival: its precondition lives in Structure, not in the targeting rules");
            Assert.That(destroyed.State, Is.EqualTo(StructureState.Destroyed),
                "a rejected repair must not resurrect the structure");
        }
    }
}
