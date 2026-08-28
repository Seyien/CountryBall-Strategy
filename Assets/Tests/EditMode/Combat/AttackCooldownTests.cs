using System;
using NUnit.Framework;
using GridStrategy.Combat;

namespace GridStrategy.Tests.EditMode.Combat
{
    /// <summary>
    /// Saldırının BEDELİNİ koruyan dosya.
    ///
    /// Var olma sebebi ölçülmüş bir oyun kuralı boşluğu: sıra kipi
    /// <c>FreeForAll</c>'a çevrildiğinde saldırı artık sıra harcamamaya başladı
    /// ve geriye hiçbir bedel kalmadı. Tek bir seçim açıkken aynı hedefe üst
    /// üste tıklayan oyuncu, ilk vuruş ekranda görünmeden ikincisini de
    /// indiriyordu; hasar fare hızına bağlıydı.
    ///
    /// Burada sınanan şey bir görsel yumuşatma değil bir KURAL. Kuralın üç
    /// parçası ve üç ayrı sahibi var: eşik <see cref="AttackProfile"/>'da,
    /// kalan süre <see cref="Combatant"/> ile <see cref="Structure"/>'ın
    /// kendisinde, kapı ise <see cref="AttackAction"/>'da. Bu dosya üçünün de
    /// yerinde durduğunu koruyor.
    ///
    /// Parçalar burada yeniden sınanmıyor: menzil
    /// <see cref="AttackResolverTests"/>'te, hedef uygunluğu
    /// <see cref="TargetingRulesTests"/>'te, saldıranın hâli
    /// <see cref="AttackRulesTests"/>'te zaten sınanıyor.
    /// </summary>
    public sealed class AttackCooldownTests
    {
        // REDDEDILEN - AttackCooldownTests.cs:52 yerine:
        //     // BoardAdapter.Update içinde, tahtanın çizim döngüsünde:
        //     if (Time.time - lastAttackAt < 0.4f) { return; }
        // KIRILAN  : bekleme bir SAVAŞ sayısıdır ve motorun çizim döngüsüne
        //            yazılınca aynı kuralın ikinci kopyası doğar.
        //            EditMode testi kuralı hiç göremez -> bu dosya yazılamazdı
        //            otomatik ateş eden kule kapıyı bambaşka bir yoldan atlar
        //            sayı tek olurdu: okçu ile mancınık aynı hızda vururdu
        // KAZANIRDI: bekleme yalnızca bir GÖSTERİM yumuşatması olsaydı —
        //            tıklamayı yutmak değil, animasyonun bitmesini beklemek —
        //            o zaman yeri gerçekten motor tarafıdır.
        // TEK CUMLE: Kuralın sahibi Core'dur; motor katmanı yalnızca zamanı
        //            taşır, kararı değil.
        private static Combatant NewCombatant(
            float cooldownSeconds = 0f,
            int damage = 10,
            int range = 1,
            int maxHealth = 100,
            Team team = Team.Player)
        {
            return new Combatant(
                new Health(maxHealth),
                new UnitLifecycle(downedWindowSeconds: 10f, corpseWindowSeconds: 5f),
                new AttackProfile(damage: damage, range: range, cooldownSeconds: cooldownSeconds),
                team);
        }

        /// <summary>
        /// Verilen tanımı PAYLAŞAN bir savaşçı kurar. Sayacın tanımda değil
        /// örnekte yaşadığını ölçen testlerin tek yardımcısı bu.
        /// </summary>
        private static Combatant NewSharing(AttackProfile shared, Team team)
        {
            return new Combatant(
                new Health(1000),
                new UnitLifecycle(downedWindowSeconds: 10f, corpseWindowSeconds: 5f),
                shared,
                team);
        }

        private static Structure NewTower(
            float cooldownSeconds = 0f,
            int damage = 25,
            int range = 2,
            int maxHealth = 200,
            Team team = Team.Player)
        {
            return new Structure(
                new Health(maxHealth),
                new StructureLifecycle(),
                team,
                new AttackProfile(damage: damage, range: range, cooldownSeconds: cooldownSeconds));
        }

        private static Structure NewBarracks(int maxHealth = 200, Team team = Team.Enemy)
        {
            // Saldırı tanımı YOK: silahsız yapı bu oyunun kural hâli.
            return new Structure(new Health(maxHealth), new StructureLifecycle(), team);
        }

        // ─────────────────────────────────────────────────────────────
        // GERİ UYUM: bugüne kadar yazılmış her profil iki argümanlıydı
        // ─────────────────────────────────────────────────────────────

        [Test]
        public void ZeroCooldown_AllowsBackToBackHitsExactlyAsBefore()
        {
            // Bu dosyanın ilk sözü bir güvence: üretimde ve testlerde duran
            // yirmi küsur profil çağrısının hepsi iki argümanlı ve hepsinin
            // yeni anlamı "bekleme yok". Bu test yeşil kaldığı sürece eklenen
            // sayı eski davranışa hiç dokunmamış demektir.
            Combatant attacker = NewCombatant(cooldownSeconds: 0f, damage: 10, team: Team.Player);
            Combatant target = NewCombatant(maxHealth: 100, team: Team.Enemy);

            Assert.That(AttackAction.Execute(attacker, target, distance: 1), Is.EqualTo(AttackOutcome.Hit));
            Assert.That(AttackAction.Execute(attacker, target, distance: 1), Is.EqualTo(AttackOutcome.Hit));
            Assert.That(AttackAction.Execute(attacker, target, distance: 1), Is.EqualTo(AttackOutcome.Hit));

            Assert.That(target.CurrentHealth, Is.EqualTo(70), "three hits must all land");
            Assert.That(attacker.AttackCooldownRemaining, Is.Zero);
            Assert.That(attacker.IsAttackReady, Is.True);
        }

        [Test]
        public void TheTwoArgumentConstructor_StillMeansNoCooldown()
        {
            var old = new AttackProfile(damage: 10, range: 1);

            Assert.That(old.CooldownSeconds, Is.Zero);
            Assert.IsTrue(old == new AttackProfile(damage: 10, range: 1, cooldownSeconds: 0f));
        }

        // ─────────────────────────────────────────────────────────────
        // KAPI: bekleme dolmadan gelen saldırı reddedilir
        // ─────────────────────────────────────────────────────────────

        [Test]
        public void PositiveCooldown_TheImmediateSecondHit_IsRejectedOnCooldown()
        {
            // Operatörün bildirdiği durumun birebir karşılığı: tek seçim açık,
            // aynı konuma iki hızlı tıklama.
            Combatant attacker = NewCombatant(cooldownSeconds: 2f, damage: 10, team: Team.Player);
            Combatant target = NewCombatant(maxHealth: 100, team: Team.Enemy);

            Assert.That(AttackAction.Execute(attacker, target, distance: 1), Is.EqualTo(AttackOutcome.Hit));

            AttackOutcome second = AttackAction.Execute(attacker, target, distance: 1);

            Assert.That(second, Is.EqualTo(AttackOutcome.RejectedOnCooldown));
            Assert.That(target.CurrentHealth, Is.EqualTo(90), "a rejected attack deals no damage");
        }

        [Test]
        public void RejectedOnCooldown_IsNotTheSameAnswerAsRejectedActorCannotAct()
        {
            // İki sebebin AYRI kalması bu turun sözleşmesi: oyuncuya söylenecek
            // cümleler farklı ve farklı davranışlara yol açıyorlar — biri
            // "başka birim seç", öteki "bekle".
            Combatant reloading = NewCombatant(cooldownSeconds: 2f, team: Team.Player);
            Combatant downed = NewCombatant(cooldownSeconds: 2f, maxHealth: 10, team: Team.Player);
            Combatant target = NewCombatant(maxHealth: 100, team: Team.Enemy);

            AttackAction.Execute(reloading, target, distance: 1);
            downed.TakeDamage(10);

            Assert.That(downed.State, Is.EqualTo(UnitState.Downed), "test setup");
            Assert.That(
                AttackAction.Execute(reloading, target, distance: 1),
                Is.EqualTo(AttackOutcome.RejectedOnCooldown));
            Assert.That(
                AttackAction.Execute(downed, target, distance: 1),
                Is.EqualTo(AttackOutcome.RejectedActorCannotAct));
        }

        [Test]
        public void WeaponlessStructure_IsStillRejectedAsActorCannotAct()
        {
            // Silahsız bir deponun reddi bekleme sebebine KAYMADI: yapının
            // sayacı hazır duruyor ve ret sebebini veren kapı yukarıda.
            Structure depot = NewBarracks(team: Team.Player);
            Combatant target = NewCombatant(maxHealth: 100, team: Team.Enemy);

            Assert.That(depot.IsAttackReady, Is.True);
            Assert.That(
                AttackAction.Execute(depot, target, distance: 1),
                Is.EqualTo(AttackOutcome.RejectedActorCannotAct));
        }

        // ─────────────────────────────────────────────────────────────
        // SINIR: süre = bekleme olduğunda kim kazanır
        // ─────────────────────────────────────────────────────────────

        [Test]
        public void TickingExactlyTheCooldown_LetsTheAttackerHitAgain_TheBoundaryFavoursTheAttacker()
        {
            // SINIRIN ADI BURADA KONUYOR: geçen süre beklemeye TAM eşit
            // olduğunda saldıran kazanır, bekleme değil. Aynı seçim
            // StructureProduction'da da yapılmıştı ve iki sayaç arasında farklı
            // bir sınır, oyuncunun aynı saymayı iki kez öğrenmesi demek olurdu.
            Combatant attacker = NewCombatant(cooldownSeconds: 2f, damage: 10, team: Team.Player);
            Combatant target = NewCombatant(maxHealth: 100, team: Team.Enemy);

            AttackAction.Execute(attacker, target, distance: 1);
            attacker.Tick(2f);

            Assert.That(attacker.AttackCooldownRemaining, Is.Zero);
            Assert.That(attacker.IsAttackReady, Is.True);
            Assert.That(AttackAction.Execute(attacker, target, distance: 1), Is.EqualTo(AttackOutcome.Hit));
            Assert.That(target.CurrentHealth, Is.EqualTo(80));
        }

        [Test]
        public void TickingJustUnderTheCooldown_StillRejects()
        {
            Combatant attacker = NewCombatant(cooldownSeconds: 2f, damage: 10, team: Team.Player);
            Combatant target = NewCombatant(maxHealth: 100, team: Team.Enemy);

            AttackAction.Execute(attacker, target, distance: 1);
            attacker.Tick(1.5f);

            Assert.That(attacker.AttackCooldownRemaining, Is.EqualTo(0.5f));
            Assert.That(attacker.IsAttackReady, Is.False);
            Assert.That(
                AttackAction.Execute(attacker, target, distance: 1),
                Is.EqualTo(AttackOutcome.RejectedOnCooldown));
        }

        [Test]
        public void TheCounterNeverFallsBelowZero()
        {
            // Eksiye kayan bir sayaç SONRAKİ vuruşun beklemesini kısaltırdı ve
            // hata yalnız uzun duraklamalardan sonra görünürdü.
            Combatant attacker = NewCombatant(cooldownSeconds: 1f, team: Team.Player);
            Combatant target = NewCombatant(maxHealth: 100, team: Team.Enemy);

            AttackAction.Execute(attacker, target, distance: 1);
            attacker.Tick(5f);

            Assert.That(attacker.AttackCooldownRemaining, Is.Zero);
        }

        [Test]
        public void NegativeTime_StillThrows_AndTheCooldownIsNotStretched()
        {
            // Geriye akan zamanın kelepçesi yaşam döngüsünde duruyor ve bekleme
            // ondan SONRA eksiliyor; sıra tersine dönseydi negatif bir delta
            // beklemeyi uzatır, istisna ancak ondan sonra atılırdı.
            Combatant attacker = NewCombatant(cooldownSeconds: 2f, team: Team.Player);
            Combatant target = NewCombatant(maxHealth: 100, team: Team.Enemy);

            AttackAction.Execute(attacker, target, distance: 1);

            Assert.Throws<ArgumentOutOfRangeException>(() => attacker.Tick(-1f));
            Assert.That(attacker.AttackCooldownRemaining, Is.EqualTo(2f));
        }

        // ─────────────────────────────────────────────────────────────
        // REDDEDİLEN SALDIRI SAYACA DOKUNMAZ
        // ─────────────────────────────────────────────────────────────

        [Test]
        public void RejectedOnCooldownAttack_NeitherAdvancesNorRestartsTheCounter()
        {
            // BU TESTİN OYUNDAKİ KARŞILIĞI: hızlı tıklamak beklemeyi
            // UZATMAMALI. Sayaç her reddedilen tıklamada baştan başlasaydı,
            // sabırsız oyuncu kendini sonsuza dek bekletirdi.
            Combatant attacker = NewCombatant(cooldownSeconds: 2f, damage: 10, team: Team.Player);
            Combatant target = NewCombatant(maxHealth: 100, team: Team.Enemy);

            AttackAction.Execute(attacker, target, distance: 1);
            attacker.Tick(1.5f);

            AttackAction.Execute(attacker, target, distance: 1);

            Assert.That(attacker.AttackCooldownRemaining, Is.EqualTo(0.5f),
                "a rejected attack must leave the counter exactly where it was");

            attacker.Tick(0.5f);

            Assert.That(AttackAction.Execute(attacker, target, distance: 1), Is.EqualTo(AttackOutcome.Hit));
            Assert.That(target.CurrentHealth, Is.EqualTo(80));
        }

        [Test]
        public void AttackRejectedOutOfRange_DoesNotSpendTheCooldown()
        {
            // Bekleme kapısının EN SONDA durmasının ölçülebilir sonucu: menzil
            // dışına yapılan boş bir tıklama vuruş hakkını yakmaz.
            Combatant attacker = NewCombatant(cooldownSeconds: 2f, damage: 10, range: 1, team: Team.Player);
            Combatant target = NewCombatant(maxHealth: 100, team: Team.Enemy);

            Assert.That(
                AttackAction.Execute(attacker, target, distance: 5),
                Is.EqualTo(AttackOutcome.RejectedOutOfRange));
            Assert.That(attacker.IsAttackReady, Is.True);
            Assert.That(attacker.AttackCooldownRemaining, Is.Zero);
            Assert.That(AttackAction.Execute(attacker, target, distance: 1), Is.EqualTo(AttackOutcome.Hit));
        }

        [Test]
        public void AttackRejectedOnInvalidTarget_DoesNotSpendTheCooldown()
        {
            Combatant attacker = NewCombatant(cooldownSeconds: 2f, damage: 10, team: Team.Player);
            Combatant friend = NewCombatant(maxHealth: 100, team: Team.Player);
            Combatant enemy = NewCombatant(maxHealth: 100, team: Team.Enemy);

            Assert.That(
                AttackAction.Execute(attacker, friend, distance: 1),
                Is.EqualTo(AttackOutcome.RejectedInvalidTarget));
            Assert.That(attacker.AttackCooldownRemaining, Is.Zero);
            Assert.That(AttackAction.Execute(attacker, enemy, distance: 1), Is.EqualTo(AttackOutcome.Hit));
        }

        // ─────────────────────────────────────────────────────────────
        // SAYAÇ TANIMDA DEĞİL ÖRNEKTE
        // ─────────────────────────────────────────────────────────────

        [Test]
        public void TwoCombatantsSharingOneProfile_WaitSeparately()
        {
            // Bu dosyanın en pahalı iddiası: eşik paylaşılır, sayaç
            // paylaşılmaz. Alan AttackProfile'a konsaydı bu testin ikinci
            // savaşçısı da susardı ve yüz okçulu bir ordu tek okçu gibi vururdu.
            var shared = new AttackProfile(damage: 10, range: 1, cooldownSeconds: 2f);
            Combatant first = NewSharing(shared, Team.Player);
            Combatant second = NewSharing(shared, Team.Player);
            Combatant target = NewSharing(shared, Team.Enemy);

            Assert.That(first.AttackProfile, Is.SameAs(second.AttackProfile), "test setup");

            Assert.That(AttackAction.Execute(first, target, distance: 1), Is.EqualTo(AttackOutcome.Hit));

            Assert.That(first.AttackCooldownRemaining, Is.EqualTo(2f));
            Assert.That(second.AttackCooldownRemaining, Is.Zero);
            Assert.That(AttackAction.Execute(second, target, distance: 1), Is.EqualTo(AttackOutcome.Hit));
        }

        [Test]
        public void TickingOneCombatant_DoesNotTickAnother()
        {
            var shared = new AttackProfile(damage: 10, range: 1, cooldownSeconds: 2f);
            Combatant first = NewSharing(shared, Team.Player);
            Combatant second = NewSharing(shared, Team.Player);
            Combatant target = NewSharing(shared, Team.Enemy);

            AttackAction.Execute(first, target, distance: 1);
            AttackAction.Execute(second, target, distance: 1);
            first.Tick(2f);

            Assert.That(first.AttackCooldownRemaining, Is.Zero);
            Assert.That(second.AttackCooldownRemaining, Is.EqualTo(2f));
        }

        // ─────────────────────────────────────────────────────────────
        // TANIMIN DEĞER EŞİTLİĞİ ÜÇÜNCÜ SAYIYI DA GÖRÜYOR
        // ─────────────────────────────────────────────────────────────

        [Test]
        public void ProfilesDifferingOnlyInCooldown_AreNotEqual()
        {
            // Hızlı vuran hançer ile yavaş vuran balta AYNI hasarı ve AYNI
            // menzili taşıyabilir; onları ayıran tek sayı bekleme.
            var dagger = new AttackProfile(damage: 10, range: 1, cooldownSeconds: 0.25f);
            var axe = new AttackProfile(damage: 10, range: 1, cooldownSeconds: 2f);

            Assert.IsFalse(dagger == axe);
            Assert.IsTrue(dagger != axe);
            Assert.IsFalse(dagger.Equals(axe));
        }

        [Test]
        public void ProfilesWithTheSameThreeNumbers_AreEqualAndShareAHashCode()
        {
            var a = new AttackProfile(damage: 10, range: 1, cooldownSeconds: 0.75f);
            var b = new AttackProfile(damage: 10, range: 1, cooldownSeconds: 0.75f);

            Assert.IsFalse(ReferenceEquals(a, b), "Test iki AYRI nesne kurmalı.");
            Assert.IsTrue(a == b);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void NegativeCooldown_IsRejected()
        {
            // Sıfır GEÇERLİ ve adı "bekleme yok"; negatif ise bir çağıran
            // hatası ve gürültüyle patlıyor.
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new AttackProfile(damage: 10, range: 1, cooldownSeconds: -0.5f));
            Assert.That(new AttackProfile(damage: 10, range: 1, cooldownSeconds: 0f).CooldownSeconds, Is.Zero);
        }

        // ─────────────────────────────────────────────────────────────
        // YAPI TARAFI: sayacı kulenin KENDİSİ tutar
        // ─────────────────────────────────────────────────────────────

        [Test]
        public void TowerOnCooldown_RejectsItsSecondShot()
        {
            // Bu turda kule menzilindeki düşmanı gördüğü her KAREde ateş
            // ediyordu; beklemesiz bir kulede hasar kare hızına bağlıydı.
            Structure tower = NewTower(cooldownSeconds: 1f, damage: 25, range: 2, team: Team.Player);
            Combatant target = NewCombatant(maxHealth: 100, team: Team.Enemy);

            Assert.That(AttackAction.Execute(tower, target, distance: 2), Is.EqualTo(AttackOutcome.Hit));
            Assert.That(
                AttackAction.Execute(tower, target, distance: 2),
                Is.EqualTo(AttackOutcome.RejectedOnCooldown));
            Assert.That(target.CurrentHealth, Is.EqualTo(75), "only the first shot lands");
        }

        [Test]
        public void TowerBecomesReadyAfterItsOwnTick()
        {
            Structure tower = NewTower(cooldownSeconds: 1f, damage: 25, range: 2, team: Team.Player);
            Combatant target = NewCombatant(maxHealth: 100, team: Team.Enemy);

            AttackAction.Execute(tower, target, distance: 2);

            Assert.That(tower.AttackCooldownRemaining, Is.EqualTo(1f));

            tower.Tick(1f);

            Assert.That(tower.IsAttackReady, Is.True);
            Assert.That(AttackAction.Execute(tower, target, distance: 2), Is.EqualTo(AttackOutcome.Hit));
            Assert.That(target.CurrentHealth, Is.EqualTo(50));
        }

        [Test]
        public void TwoTowersSharingOneProfile_WaitSeparately()
        {
            var shared = new AttackProfile(damage: 25, range: 2, cooldownSeconds: 1f);
            var left = new Structure(new Health(200), new StructureLifecycle(), Team.Player, shared);
            var right = new Structure(new Health(200), new StructureLifecycle(), Team.Player, shared);
            Combatant target = NewCombatant(maxHealth: 100, team: Team.Enemy);

            Assert.That(AttackAction.Execute(left, target, distance: 2), Is.EqualTo(AttackOutcome.Hit));

            Assert.That(right.AttackCooldownRemaining, Is.Zero, "the neighbour tower keeps its own counter");
            Assert.That(AttackAction.Execute(right, target, distance: 2), Is.EqualTo(AttackOutcome.Hit));
            Assert.That(target.CurrentHealth, Is.EqualTo(50));
        }

        [Test]
        public void TowerAgainstAnEnemyStructure_AlsoWaits()
        {
            Structure tower = NewTower(cooldownSeconds: 1f, damage: 25, range: 2, team: Team.Player);
            Structure barracks = NewBarracks(maxHealth: 200, team: Team.Enemy);

            Assert.That(AttackAction.Execute(tower, barracks, distance: 2), Is.EqualTo(AttackOutcome.Hit));
            Assert.That(
                AttackAction.Execute(tower, barracks, distance: 2),
                Is.EqualTo(AttackOutcome.RejectedOnCooldown));
            Assert.That(barracks.CurrentHealth, Is.EqualTo(175));
        }

        [Test]
        public void CombatantAgainstAStructure_AlsoWaits()
        {
            // Dört aşırı yüklemenin dördünde de kapı duruyor; biri eksik
            // kalsaydı oyuncu o bileşimi bulur ve beklemesiz vururdu.
            Combatant attacker = NewCombatant(cooldownSeconds: 2f, damage: 10, team: Team.Player);
            Structure barracks = NewBarracks(maxHealth: 200, team: Team.Enemy);

            Assert.That(AttackAction.Execute(attacker, barracks, distance: 1), Is.EqualTo(AttackOutcome.Hit));
            Assert.That(
                AttackAction.Execute(attacker, barracks, distance: 1),
                Is.EqualTo(AttackOutcome.RejectedOnCooldown));
            Assert.That(barracks.CurrentHealth, Is.EqualTo(190));
        }

        [Test]
        public void ARubbleTowerIsRejectedAsActorCannotAct_NotOnCooldown()
        {
            // Yıkık kulenin sayacı hazır olabilir; onu susturan şey bekleme
            // değil, hâli.
            Structure tower = NewTower(cooldownSeconds: 1f, maxHealth: 50, team: Team.Player);
            Combatant target = NewCombatant(maxHealth: 100, team: Team.Enemy);

            tower.TakeDamage(50);

            Assert.That(tower.IsAttackReady, Is.True, "test setup");
            Assert.That(
                AttackAction.Execute(tower, target, distance: 2),
                Is.EqualTo(AttackOutcome.RejectedActorCannotAct));
        }
    }
}
