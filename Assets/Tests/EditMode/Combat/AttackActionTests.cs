using System;
using System.Collections.Generic;
using NUnit.Framework;
using GridStrategy.Combat;

namespace GridStrategy.Tests.EditMode.Combat
{
    /// <summary>
    /// Bu dosya parçaları test etmiyor — menzil <see cref="AttackResolverTests"/>'te,
    /// hedef uygunluğu <see cref="TargetingRulesTests"/>'te, hasar
    /// <see cref="CombatantTests"/>'te sınanıyor. Burada sınanan tek şey AKIŞ:
    /// hangi kurala hangi sırayla soruluyor ve sonuç nasıl adlandırılıyor.
    ///
    /// Yedi test bu dosyanın var olma sebebi ve yedisi de bir DAVRANIŞI değil
    /// bir KARARI koruyor:
    /// <list type="bullet">
    /// <item>menzil dışı + ölü hedef → hangi ret sebebi dönüyor (sıra kararı)</item>
    /// <item>menzil dışı + aynı takım → aynı sıra kararının takım ikizi</item>
    /// <item>düşmüş SALDIRAN + geçersiz hedef + menzil dışı → aynı sıra
    /// kararının üçüncü sınavı; saldıranın kendi durumu HEPSİNDEN önce</item>
    /// <item>zaten düşmüş hedefe vuruş → Hit mi HitAndDowned mı (değişim kararı)</item>
    /// <item>aynı takıma vuruş → akış takımı SORUYOR (katman kararı)</item>
    /// <item>düşmüş birim vuramıyor → akış SALDIRANIN durumunu da soruyor
    /// (katman kararının ikizi)</item>
    /// <item>yıkılan yapı → HitAndDestroyed, HitAndDowned değil (adlandırma kararı)</item>
    /// </list>
    /// Hepsi kırmızıya dönerse kod hâlâ "çalışıyor" görünür; kırılan şey
    /// çağıranın sonuca bakarak verdiği karardır.
    ///
    /// TAKIMLAR ARTIK KURULUMUN PARÇASI. Eskiden bu dosyanın yardımcıları takım
    /// yazmıyordu (varsayılan <see cref="Team.None"/>) çünkü akış takımı hiç
    /// sormuyordu. Artık soruyor ve <see cref="Team.None"/> saldıramaz, yani
    /// takımsız bir kurulum her testi aynı ret sebebiyle yeşil gösterirdi —
    /// ölçtüğü şeyi değil.
    /// </summary>
    public sealed class AttackActionTests
    {
        // Varsayılan taraf Player: bu dosyadaki saldıranların çoğu odur ve
        // hedefler açıkça Team.Enemy yazar. "Düşman hedefi" varsayılan
        // YAPILMADI — saldıranla hedefin tarafı testin ölçtüğü şeyin parçası
        // olduğu için ikisinin de görünür olması gerekiyor.
        private static Combatant NewCombatant(
            int maxHealth = 100,
            int damage = 10,
            int range = 1,
            Team team = Team.Player)
        {
            return new Combatant(
                new Health(maxHealth),
                new UnitLifecycle(downedWindowSeconds: 10f, corpseWindowSeconds: 5f),
                new AttackProfile(damage: damage, range: range),
                team);
        }

        // Hedefi Downed'a indirmenin en kısa yolu. Doğrudan TakeDamage
        // kullanılıyor, AttackAction değil: kurulum sınanan şeyi kullanırsa
        // test kendi kendini doğrulamış olur.
        //
        // Takım artık PARAMETRE, ve varsayılanı eski davranış: bu dosyadaki
        // düşmüş savaşçılar önce yalnızca HEDEF olarak kuruluyordu, şimdi
        // SALDIRAN olarak da kuruluyor ve saldıranın tarafı testin ölçtüğü
        // şeyin parçası. Varsayılan sayesinde var olan çağrılar değişmedi.
        private static Combatant NewDownedCombatant(Team team = Team.Enemy)
        {
            var combatant = NewCombatant(maxHealth: 10, team: team);
            combatant.TakeDamage(10);
            Assert.That(combatant.State, Is.EqualTo(UnitState.Downed), "kurulum bozuk");
            return combatant;
        }

        private static Combatant NewDeadCombatant(Team team = Team.Enemy)
        {
            var combatant = NewDownedCombatant(team);
            combatant.Tick(10.1f);
            Assert.That(combatant.State, Is.EqualTo(UnitState.Dead), "kurulum bozuk");
            return combatant;
        }

        // Durumu bir DEĞER olarak isteyen taramalar için tek kapı. Üç durumun
        // kurulumu üç ayrı yol izliyor (Alive doğrudan, Downed hasarla, Dead
        // hasar + zaman) ve bu ayrım taramanın gövdesine sızmamalı.
        private static Combatant NewCombatantInState(UnitState state, Team team)
        {
            switch (state)
            {
                case UnitState.Downed:
                    return NewDownedCombatant(team);
                case UnitState.Dead:
                    return NewDeadCombatant(team);
                default:
                    return NewCombatant(maxHealth: 100, team: team);
            }
        }

        private static Structure NewStructure(int maxHealth = 100, Team team = Team.Enemy)
        {
            return new Structure(new Health(maxHealth), new StructureLifecycle(), team);
        }

        private static Structure NewDestroyedStructure(Team team = Team.Enemy)
        {
            Structure structure = NewStructure(maxHealth: 10, team: team);
            Assert.That(structure.TakeDamage(10), Is.True, "kurulum bozuk");
            return structure;
        }

        [Test]
        public void Execute_InRangeAliveTarget_HitsAndDealsProfileDamage()
        {
            var attacker = NewCombatant(damage: 30, range: 2, team: Team.Player);
            var target = NewCombatant(maxHealth: 100, team: Team.Enemy);

            AttackOutcome outcome = AttackAction.Execute(attacker, target, distance: 2);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.Hit));
            // Hasar AttackProfile'dan geliyor, AttackAction'dan değil — bu
            // assertion o sahipliği koruyor.
            Assert.That(target.CurrentHealth, Is.EqualTo(70));
        }

        [Test]
        public void Execute_OutOfRange_IsRejectedAndDealsNoDamage()
        {
            var attacker = NewCombatant(damage: 30, range: 1, team: Team.Player);
            var target = NewCombatant(maxHealth: 100, team: Team.Enemy);

            AttackOutcome outcome = AttackAction.Execute(attacker, target, distance: 2);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.RejectedOutOfRange));
            // Reddedilen saldırı hiçbir yan etki bırakmamalı. Bu satır olmasaydı
            // "önce vur sonra kontrol et" hatası testten geçerdi.
            Assert.That(target.CurrentHealth, Is.EqualTo(100));
        }

        [Test]
        public void Execute_DeadTarget_IsRejectedAsInvalidTarget()
        {
            var attacker = NewCombatant(range: 1);
            var target = NewDeadCombatant();

            AttackOutcome outcome = AttackAction.Execute(attacker, target, distance: 1);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.RejectedInvalidTarget));
        }

        /// <summary>
        /// SIRA KARARINI koruyan test. Hedef hem ölü hem menzil dışı — iki ret
        /// sebebi de geçerli, ama yalnız biri DOĞRU. "Geçersiz hedef" doğru olan,
        /// çünkü o birim menzile girse bile saldırılamaz; "menzil dışı" cevabı
        /// yapay zekâya "yaklaş" dedirtir ve sonsuz döngü üretir.
        ///
        /// Bu test kırmızıya dönerse AttackAction içindeki iki kontrolün sırası
        /// değişmiş demektir — ve o değişiklik başka hiçbir testi kırmaz.
        /// </summary>
        [Test]
        public void Execute_DeadTargetOutOfRange_PrefersInvalidTargetOverOutOfRange()
        {
            var attacker = NewCombatant(range: 1);
            var target = NewDeadCombatant();

            AttackOutcome outcome = AttackAction.Execute(attacker, target, distance: 5);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.RejectedInvalidTarget),
                "dead target must be reported as an invalid target even when it is also out of range");
        }

        [Test]
        public void Execute_LethalHit_ReportsHitAndDowned()
        {
            var attacker = NewCombatant(damage: 10, range: 1, team: Team.Player);
            var target = NewCombatant(maxHealth: 10, team: Team.Enemy);

            AttackOutcome outcome = AttackAction.Execute(attacker, target, distance: 1);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.HitAndDowned));
            Assert.That(target.State, Is.EqualTo(UnitState.Downed));
        }

        /// <summary>
        /// DEĞİŞİM KARARINI koruyan test. Hedef zaten düşmüş; bu vuruş onu
        /// DÜŞÜRMÜYOR, BİTİRİYOR — ve iki cevap ayrı kalmak zorunda.
        ///
        /// Kırmızıya dönerse AttackAction sonucu vuruş SONRASI duruma bakarak
        /// belirliyor demektir — ve o hata her vuruşta düşme animasyonu
        /// oynatır, skor tablosuna aynı birim için defalarca puan yazar.
        /// </summary>
        // İDDİA BU TURDA DEĞİŞTİ VE SEBEBİ KURAL DEĞİŞİKLİĞİDİR, testin
        // kolaylığı değil: eskiden bu satır "sonuç Hit, hedef hâlâ Downed"
        // diyordu ve o hâl sahada ölçülmüş bir kusurdu — düşmüş düşmana vurmak
        // bekleme süresini harcıyor, hiçbir şeyi değiştirmiyor ve beden 15
        // saniye boyunca dokunulamaz bir baraj olarak hücreyi işgal ediyordu.
        // Düşme penceresi artık İKİ tarafa da açık: dost kaldırır, düşman
        // bitirir. Testin koruduğu asıl şey değişmedi — "zaten düşmüş olan
        // ikinci kez düşürülemez" iddiası aynen duruyor, yalnız doğru cevabın
        // adı HitAndDowned değil HitAndFinished.
        [Test]
        public void Execute_HittingAnAlreadyDownedTarget_FinishesItInsteadOfDowningItAgain()
        {
            // Hasar DÜŞME CANI kadar: NewDownedCombatant'ın canı 10, havuz onun
            // yarısı. Bir çentiklik hasarla yazılsaydı bu test bitirici vuruşu
            // değil, havuzun büyüklüğünü ölçerdi.
            var attacker = NewCombatant(damage: 5, range: 1);
            var target = NewDownedCombatant();

            AttackOutcome outcome = AttackAction.Execute(attacker, target, distance: 1);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.HitAndFinished),
                "a target that was already downed cannot be downed again by this hit");
            Assert.That(target.State, Is.EqualTo(UnitState.Dead),
                "the finishing blow closes the downed window for good");
        }

        /// <summary>
        /// Bitirmek TEK çentikle olmaz: havuz boşalmadan sonuç sıradan bir
        /// isabettir ve pencere açık kalır.
        /// </summary>
        // BU TESTİN KORUDUĞU ŞEY BİR KESTİRMENİN GERİ GELMEMESİ: düşmüş birime
        // vurmanın onu ANINDA öldürmemesi bu projede yazılı bir karardı ve
        // düşme canı tam olarak o kararın eksik kalan yarısı. Havuz sessizce
        // bire düşürülürse bu satır kırmızıya döner.
        [Test]
        public void Execute_OneNickOnADownedTarget_IsAPlainHitAndLeavesTheWindowOpen()
        {
            var attacker = NewCombatant(damage: 1, range: 1);
            var target = NewDownedCombatant();

            AttackOutcome outcome = AttackAction.Execute(attacker, target, distance: 1);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.Hit));
            Assert.That(target.State, Is.EqualTo(UnitState.Downed),
                "one nick is under the downed pool, so the rescue window survives");
        }

        /// <summary>
        /// Kalıcı ölüye vurmak hiçbir şeyi değiştirmez ve BİTİRDİN demez.
        /// </summary>
        // BU TESTİN TEK İŞİ BİR SINIR ÇİZMEK: bitirici vuruş bir DEĞİŞİMİN adı,
        // bir durumun değil. Describe yalnız State'e baksaydı enkaza yapılan her
        // vuruş "bitirdin" derdi ve ceset sayacı her vuruşta yeniden kurulurdu.
        [Test]
        public void Execute_HittingAnAlreadyDeadTarget_IsRejectedAsAnInvalidTarget()
        {
            var attacker = NewCombatant(damage: 1, range: 1);
            var target = NewDeadCombatant();

            AttackOutcome outcome = AttackAction.Execute(attacker, target, distance: 1);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.RejectedInvalidTarget));
            Assert.That(target.State, Is.EqualTo(UnitState.Dead));
        }

        [Test]
        public void Execute_ZeroDistance_IsAllowed()
        {
            // distance == 0 kararı AttackResolver'a ait ve orada sınanıyor.
            // Buradaki test o kararın AKIŞ tarafından geçersiz kılınmadığını
            // koruyor — biri buraya "kendine saldıramazsın" satırı eklerse
            // kural iki yerde yaşamaya başlar.
            var attacker = NewCombatant(damage: 5, range: 1, team: Team.Player);
            var target = NewCombatant(maxHealth: 100, team: Team.Enemy);

            AttackOutcome outcome = AttackAction.Execute(attacker, target, distance: 0);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.Hit));
        }

        [Test]
        public void Execute_NullAttacker_Throws()
        {
            var target = NewCombatant();

            // SALDIRAN TARAFININ İKİZİ, ve gerekçe bir alttaki testte yazılı:
            // saldıran-yapı aşırı yüklemesi eklendikten sonra çıplak null iki
            // aday arasında belirsiz kalır ve dosya derlenmez. Sınanan şey
            // DEĞİŞMEDİ — hâlâ "null saldıran istisna atar"; değişen tek şey
            // belirsizliği testin kendisinin çözmesi.
            Assert.Throws<ArgumentNullException>(
                () => AttackAction.Execute((Combatant)null, target, distance: 1));
        }

        [Test]
        public void Execute_NullTarget_Throws()
        {
            var attacker = NewCombatant();

            // Dönüştürme ZORUNLU, süs değil: yapı aşırı yüklemesi eklendikten
            // sonra çıplak null iki aday arasında belirsiz kalır ve dosya
            // derlenmez. Belirsizliği testin kendisi çözüyor, aşırı yüklemelerden
            // biri feda edilerek değil.
            Assert.Throws<ArgumentNullException>(
                () => AttackAction.Execute(attacker, (Combatant)null, distance: 1));
        }

        [Test]
        public void Execute_NegativeDistance_Throws()
        {
            // Doğrulama AttackResolver'a ait; bu test onun akıştan da
            // görünür olduğunu koruyor.
            // Hedef BİLEREK düşman ve ayakta: uygunluk kapısından geçilmezse
            // menzil kuralı hiç çağrılmaz ve test doğrulamayı değil sırayı
            // ölçmüş olurdu.
            var attacker = NewCombatant(team: Team.Player);
            var target = NewCombatant(team: Team.Enemy);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => AttackAction.Execute(attacker, target, distance: -1));
        }

        // NEGATİF KONTROL: aşağıdaki dost ateşi reddinin sebebi TAKIM, "saldırı
        // hep reddediliyor" değil. Tek fark hedefin tarafı; bu test yeşil kalıp
        // aşağıdaki kırmızı olursa ölçtüğümüz şey gerçekten takım kuralıdır.
        [Test]
        public void Execute_EnemyTarget_HitsNormally()
        {
            var attacker = NewCombatant(damage: 30, range: 1, team: Team.Player);
            var target = NewCombatant(maxHealth: 100, team: Team.Enemy);

            AttackOutcome outcome = AttackAction.Execute(attacker, target, distance: 1);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.Hit));
            Assert.That(target.CurrentHealth, Is.EqualTo(70));
        }

        /// <summary>
        /// KATMAN KARARINI koruyan test: takım sorusu artık BURADA soruluyor.
        ///
        /// Bu test, bir BORCU sabitleyen testin YERİNE geçti — geçiş sessizce
        /// olmasın diye burada yazılı. Selefi
        /// <c>Execute_SameTeamTarget_StillHits_BecauseTheFlowDoesNotAskAboutTeams (SILINDI)</c>
        /// adındaydı ve tam tersini koruyordu: akışın takımı SORMADIĞINI. O test
        /// bir davranışı değil bir boşluğu sabitliyordu ve boşluk kapandığı an
        /// yalana dönüştü, çünkü aynı kural iki katmanda iki farklı cevap
        /// veriyordu — <see cref="AttackAction"/>'ı doğrudan çağıran kendi
        /// takımını vurabiliyor, akıştan geçen vuramıyordu. Kural indi; selefi
        /// silindi ve yerini bu test aldı.
        ///
        /// Kırmızıya dönerse takım sorusu bu katmandan çıkmış demektir ve dost
        /// ateşi yalnızca akışı KULLANAN çağıranlar için kapalı kalır — yapay
        /// zekâ, tekrar kaydı ve gelecekteki her ikinci akış için açılır.
        /// </summary>
        [Test]
        public void Execute_SameTeamTarget_IsRejected()
        {
            var attacker = NewCombatant(damage: 30, range: 1, team: Team.Player);
            var target = NewCombatant(maxHealth: 100, team: Team.Player);

            AttackOutcome outcome = AttackAction.Execute(attacker, target, distance: 1);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.RejectedInvalidTarget),
                "the flow itself asks the three-parameter targeting rule; friendly fire no longer depends on the caller");
            Assert.That(target.CurrentHealth, Is.EqualTo(100),
                "a rejected attack must not deal damage");
        }

        /// <summary>
        /// SIRA KARARININ TAKIM İKİZİ. Hedef hem aynı takımda hem menzil dışı —
        /// iki ret sebebi de geçerli, ama yalnız biri doğru. "Geçersiz hedef"
        /// doğru olan, çünkü o birime yaklaşmak hiçbir şeyi değiştirmez;
        /// "menzil dışı" cevabı yapay zekâya "yaklaş" dedirtir, yaklaşır ve yine
        /// reddedilir — sonsuz döngü.
        ///
        /// Ölü hedef ikizi (<c>Execute_DeadTargetOutOfRange_...</c>) tek başına
        /// yetmez: o test yalnızca DURUM kuralının menzilden önce sorulduğunu
        /// gösterir. Takım ile durum bugün TEK çağrının (üç parametreli
        /// CanBeAttacked) içinde iki AYRI kapı, ve ayıran senaryo şu: biri o
        /// çağrıyı bölüp menzili DURUM kapısından sonra, TAKIM kapısından önce
        /// sorsaydı ölü hedef testi yeşil kalır, yalnız bu test kırmızıya döner.
        /// </summary>
        [Test]
        public void Execute_SameTeamTargetOutOfRange_PrefersInvalidTargetOverOutOfRange()
        {
            var attacker = NewCombatant(damage: 30, range: 1, team: Team.Player);
            var target = NewCombatant(maxHealth: 100, team: Team.Player);

            AttackOutcome outcome = AttackAction.Execute(attacker, target, distance: 5);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.RejectedInvalidTarget),
                "a same-team target must be reported as an invalid target even when it is also out of range");
        }

        // ─────────────────────────────────────────────────────────────
        // Saldıranın KENDİ durumu
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// KATMAN KARARINI koruyan test: saldıranın kendi durumu artık BURADA
        /// soruluyor.
        ///
        /// Bu, dost ateşi borcunun ikizi olan ikinci bir borcun kapanışı.
        /// <see cref="AttackAction"/> hedefin durumunu soruyordu, SALDIRANIN
        /// durumunu sormuyordu — yani yerde yatan bir birim ateş etmeye devam
        /// ediyordu ve bunu gösterecek tek test yoktu, çünkü kimse o soruyu
        /// sormamıştı.
        ///
        /// Kırmızıya dönerse soru bu katmandan çıkmış demektir ve düşmüş birim
        /// yalnızca akışı KULLANAN çağıranlar için susturulmuş kalır — yapay
        /// zekâ, tekrar kaydı ve gelecekteki her ikinci akış için yeniden
        /// ateşlenir.
        /// </summary>
        [Test]
        public void Execute_DownedAttacker_IsRejectedAndDealsNoDamage()
        {
            Combatant attacker = NewDownedCombatant(Team.Player);
            var target = NewCombatant(maxHealth: 100, team: Team.Enemy);

            AttackOutcome outcome = AttackAction.Execute(attacker, target, distance: 1);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.RejectedActorCannotAct),
                "a downed unit is no longer a player; the rule now lives in AttackRules and is asked here");
            Assert.That(target.CurrentHealth, Is.EqualTo(100),
                "a rejected attack must not deal damage");
        }

        [Test]
        public void Execute_DeadAttacker_IsRejected()
        {
            Combatant attacker = NewDeadCombatant(Team.Player);
            var target = NewCombatant(maxHealth: 100, team: Team.Enemy);

            Assert.That(AttackAction.Execute(attacker, target, distance: 1),
                Is.EqualTo(AttackOutcome.RejectedActorCannotAct));
            Assert.That(target.CurrentHealth, Is.EqualTo(100));
        }

        /// <summary>
        /// SIRA KARARININ ÜÇÜNCÜ SINAVI, ve en sıkı olanı: saldıran düşmüş,
        /// hedef ölü, üstüne menzil dışı. Üç ret sebebi de geçerli ve yalnız
        /// biri doğru.
        ///
        /// "Saldıran eyleyemez" doğru olan, çünkü çağıranın yapabileceği tek
        /// şey onu göstermektedir: hedefi değiştirmek de yaklaşmak da cevabı
        /// DEĞİŞTİRMEZ. Yapay zekâ "geçersiz hedef" cevabını alsaydı bütün
        /// hedef listesini tarar, hepsinde aynı cevabı alır ve nedenini hiç
        /// öğrenemezdi.
        ///
        /// Bu test kırmızıya dönerse AttackAction içindeki üç kontrolün sırası
        /// değişmiş demektir — ve o değişiklik başka hiçbir testi kırmaz.
        /// </summary>
        [Test]
        public void Execute_DownedAttackerWithDeadTargetOutOfRange_PrefersActorCannotAct()
        {
            Combatant attacker = NewDownedCombatant(Team.Player);
            Combatant target = NewDeadCombatant(Team.Enemy);

            AttackOutcome outcome = AttackAction.Execute(attacker, target, distance: 5);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.RejectedActorCannotAct),
                "the attacker's own state must be reported first; nothing the caller does to the target or the distance helps");
        }

        /// <summary>
        /// SIRA KARARININ MENZİL YARISI. Yukarıdaki test üç sebebi birden
        /// yığıyor; bu test yalnızca İKİSİNİ karşılaştırıyor ve kazancı KAPSAM
        /// değil TEŞHİS: hedef geçerli olduğu için kırmızı bir sonuç tek bir şey
        /// söyler — saldıranın durumu menzilden önce sorulmuyor. Üstteki test
        /// kırmızıya döndüğünde üç sebepten hangisinin öne geçtiği okunamaz.
        /// </summary>
        [Test]
        public void Execute_DownedAttackerOutOfRange_PrefersActorCannotActOverOutOfRange()
        {
            Combatant attacker = NewDownedCombatant(Team.Player);
            var target = NewCombatant(maxHealth: 100, team: Team.Enemy);

            Assert.That(AttackAction.Execute(attacker, target, distance: 5),
                Is.EqualTo(AttackOutcome.RejectedActorCannotAct));
        }

        [Test]
        public void Execute_DownedAttackerAgainstStructure_IsRejectedAndDealsNoDamage()
        {
            // Aynı kural, ikinci aşırı yükleme. Bu satır olmasaydı düşmüş bir
            // birim askere vuramaz ama barakayı yıkabilirdi — ve o fark hiçbir
            // yerde yazılı olmazdı.
            Combatant attacker = NewDownedCombatant(Team.Player);
            Structure target = NewStructure(maxHealth: 100);

            Assert.That(AttackAction.Execute(attacker, target, distance: 1),
                Is.EqualTo(AttackOutcome.RejectedActorCannotAct));
            Assert.That(target.CurrentHealth, Is.EqualTo(100));
        }

        /// <summary>
        /// SABİTLEME TESTİ — sözleşmenin adıyla istediği iki testten biri.
        /// <see cref="AttackOutcome.RejectedActorCannotAct"/> bu katmanda
        /// YALNIZCA saldıranın kendi durumundan doğar; hedefin durumu, iki
        /// tarafın takımı ve mesafe onu ASLA üretmez.
        ///
        /// Neden bir tarama: değer üç ayrı sebebi birden kapsıyor (saldıran
        /// düşmüş, hareket eden düşmüş, sırası değil) ve o kapsam bilinçli. Bu
        /// tip yalnızca BİRİNCİSİNİ üretebilir — sırayı görmez, hareketi hiç
        /// bilmez. Tek tek yazılan testler o sınırı gösteremez; ancak "hiçbir
        /// girdiyle" biçiminde bir tarama gösterir.
        ///
        /// Kırmızıya dönerse ya bu tip bilmediği bir soruyu cevaplamaya
        /// başlamıştır ya da ret değeri başka bir sebep için ödünç alınmıştır.
        /// İkisi de kodu derlenir bırakır.
        /// </summary>
        [Test]
        public void AttackAction_ReturnsRejectedActorCannotAct_OnlyForTheActorsOwnState()
        {
            var allStates = new List<UnitState>
            {
                UnitState.Alive, UnitState.Downed, UnitState.Dead
            };
            var allTeams = new List<Team> { Team.None, Team.Player, Team.Enemy };
            var distances = new List<int> { 0, 1, 5 };

            foreach (Team attackerTeam in allTeams)
            {
                foreach (Team targetTeam in allTeams)
                {
                    foreach (UnitState targetState in allStates)
                    {
                        foreach (int distance in distances)
                        {
                            var aliveAttacker = NewCombatant(
                                damage: 1, range: 1, team: attackerTeam);

                            Assert.That(
                                AttackAction.Execute(
                                    aliveAttacker,
                                    NewCombatantInState(targetState, targetTeam),
                                    distance),
                                Is.Not.EqualTo(AttackOutcome.RejectedActorCannotAct),
                                $"an alive attacker was told it cannot act "
                                    + $"(attackerTeam={attackerTeam}, targetTeam={targetTeam}, "
                                    + $"targetState={targetState}, distance={distance})");

                            // Ters yön: saldıranın durumu bozukken cevap, geri
                            // kalan her şeyden BAĞIMSIZ olarak aynı kalmalı.
                            Assert.That(
                                AttackAction.Execute(
                                    NewDownedCombatant(attackerTeam),
                                    NewCombatantInState(targetState, targetTeam),
                                    distance),
                                Is.EqualTo(AttackOutcome.RejectedActorCannotAct),
                                $"a downed attacker must be rejected for its own state "
                                    + $"(attackerTeam={attackerTeam}, targetTeam={targetTeam}, "
                                    + $"targetState={targetState}, distance={distance})");
                        }
                    }
                }
            }

            // Yapı aşırı yüklemesi aynı taramadan geçiyor: iki hedef tipi tek
            // kuralı paylaşıyorsa tarama da paylaşılmalı, yoksa boşluk yalnızca
            // sınanmayan aşırı yüklemede açık kalır.
            foreach (Team attackerTeam in allTeams)
            {
                foreach (Team structureTeam in allTeams)
                {
                    foreach (int distance in distances)
                    {
                        Assert.That(
                            AttackAction.Execute(
                                NewCombatant(damage: 1, range: 1, team: attackerTeam),
                                NewStructure(maxHealth: 100, team: structureTeam),
                                distance),
                            Is.Not.EqualTo(AttackOutcome.RejectedActorCannotAct),
                            $"an alive attacker was told it cannot act against a standing structure "
                                + $"(attackerTeam={attackerTeam}, structureTeam={structureTeam}, distance={distance})");

                        Assert.That(
                            AttackAction.Execute(
                                NewCombatant(damage: 1, range: 1, team: attackerTeam),
                                NewDestroyedStructure(structureTeam),
                                distance),
                            Is.Not.EqualTo(AttackOutcome.RejectedActorCannotAct),
                            $"rubble must be reported as an invalid target, never as an actor problem "
                                + $"(attackerTeam={attackerTeam}, structureTeam={structureTeam}, distance={distance})");
                    }
                }
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Yapıya saldırı
        // ─────────────────────────────────────────────────────────────

        [Test]
        public void Execute_StandingStructure_HitsAndDealsProfileDamage()
        {
            var attacker = NewCombatant(damage: 30, range: 2, team: Team.Player);
            Structure target = NewStructure(maxHealth: 100);

            AttackOutcome outcome = AttackAction.Execute(attacker, target, distance: 2);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.Hit));
            Assert.That(target.CurrentHealth, Is.EqualTo(70));
            Assert.That(target.IsStanding, Is.True);
        }

        /// <summary>
        /// ADLANDIRMA KARARINI koruyan test. Yıkılan bir yapı
        /// <see cref="AttackOutcome.HitAndDestroyed"/> döndürür —
        /// <see cref="AttackOutcome.HitAndDowned"/> değil, çünkü yapı DÜŞMEZ:
        /// düşme kurtarma penceresi açan bir durumdur ve yapıda öyle bir pencere
        /// yok. Düz <see cref="AttackOutcome.Hit"/> de değil, çünkü o cevap yıkım
        /// bilgisini çağıranın elinden alır.
        ///
        /// Kırmızıya dönerse ya iki olgu tek değere katlanmış ya da yıkım
        /// sessizleşmiş demektir; ikisinde de kod derlenmeye devam eder ve hata
        /// ancak ekranda çökmeyen bir bina olarak görülür.
        /// </summary>
        [Test]
        public void Execute_LethalHitOnStructure_ReportsHitAndDestroyedNotHitAndDowned()
        {
            var attacker = NewCombatant(damage: 10, range: 1, team: Team.Player);
            Structure target = NewStructure(maxHealth: 10);

            AttackOutcome outcome = AttackAction.Execute(attacker, target, distance: 1);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.HitAndDestroyed),
                "a structure is destroyed, never downed; reusing HitAndDowned would erase that distinction");
            Assert.That(target.State, Is.EqualTo(StructureState.Destroyed));
        }

        /// <summary>
        /// DEĞİŞİM KARARININ yapı ikizi. Enkaz zaten yıkık; ikinci vuruş onu
        /// yeniden yıkmaz. Kırmızıya dönerse çağıran her isabette yıkım efekti
        /// oynatır ve skor aynı bina için defalarca yazılır.
        ///
        /// Burada ret DEĞİL <see cref="AttackOutcome.RejectedInvalidTarget"/>
        /// bekleniyor ve fark birim ikizine göre kasıtlı: düşmüş birim hâlâ
        /// vurulur (işini bitirme yolu), yıkık yapı vurulmaz (enkazın canı yok).
        /// </summary>
        [Test]
        public void Execute_DestroyedStructure_IsRejectedAsInvalidTarget()
        {
            var attacker = NewCombatant(damage: 10, range: 1, team: Team.Player);
            Structure target = NewStructure(maxHealth: 10);
            Assert.That(target.TakeDamage(10), Is.True, "kurulum bozuk");

            AttackOutcome outcome = AttackAction.Execute(attacker, target, distance: 1);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.RejectedInvalidTarget),
                "rubble is not a valid target; a unit that is merely downed still is");
        }

        [Test]
        public void Execute_StructureOutOfRange_IsRejectedAndDealsNoDamage()
        {
            var attacker = NewCombatant(damage: 30, range: 1, team: Team.Player);
            Structure target = NewStructure(maxHealth: 100);

            AttackOutcome outcome = AttackAction.Execute(attacker, target, distance: 2);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.RejectedOutOfRange));
            Assert.That(target.CurrentHealth, Is.EqualTo(100));
        }

        /// <summary>
        /// Kendi barakanı yıkamazsın. Aynı takım kuralı, aynı tek sahipten —
        /// bu test onun yapı aşırı yüklemesinden de sorulduğunu koruyor.
        /// </summary>
        [Test]
        public void Execute_SameTeamStructure_IsRejected()
        {
            var attacker = NewCombatant(damage: 30, range: 1, team: Team.Player);
            Structure target = NewStructure(maxHealth: 100, team: Team.Player);

            AttackOutcome outcome = AttackAction.Execute(attacker, target, distance: 1);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.RejectedInvalidTarget),
                "your own barracks is not a valid target");
            Assert.That(target.CurrentHealth, Is.EqualTo(100));
        }

        /// <summary>
        /// SIRA KARARININ yapı ikizi: yıkık VE menzil dışı bir yapıya saldırınca
        /// cevap "geçersiz hedef"tir. Yaklaşmak enkazı yeniden bina yapmaz.
        /// </summary>
        [Test]
        public void Execute_DestroyedStructureOutOfRange_PrefersInvalidTargetOverOutOfRange()
        {
            var attacker = NewCombatant(damage: 10, range: 1, team: Team.Player);
            Structure target = NewStructure(maxHealth: 10);
            Assert.That(target.TakeDamage(10), Is.True, "kurulum bozuk");

            AttackOutcome outcome = AttackAction.Execute(attacker, target, distance: 5);

            Assert.That(outcome, Is.EqualTo(AttackOutcome.RejectedInvalidTarget),
                "a destroyed structure must be reported as an invalid target even when it is also out of range");
        }

        [Test]
        public void Execute_NeutralStructure_IsHit()
        {
            // Tarafsız yıkılabilir duvar herkese açık — Team.None'ın var olma
            // sebebi tam olarak budur ve yapı sürümü o kararı DEĞİŞTİRMİYOR.
            var attacker = NewCombatant(damage: 30, range: 1, team: Team.Player);
            Structure target = NewStructure(maxHealth: 100, team: Team.None);

            Assert.That(AttackAction.Execute(attacker, target, distance: 1),
                Is.EqualTo(AttackOutcome.Hit));
            Assert.That(target.CurrentHealth, Is.EqualTo(70));
        }

        [Test]
        public void Execute_NullStructureTarget_Throws()
        {
            var attacker = NewCombatant();

            Assert.Throws<ArgumentNullException>(
                () => AttackAction.Execute(attacker, (Structure)null, distance: 1));
        }
    }
}
