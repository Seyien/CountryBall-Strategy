using System;
using System.Collections.Generic;
using NUnit.Framework;
using GridStrategy.Combat;

namespace GridStrategy.Tests.EditMode.Combat
{
    /// <summary>
    /// Bu dosya PARÇALARI test etmiyor — onların kendi testleri var. Burada
    /// test edilen tek şey parçalar ARASINDAKİ kural: can bittiğinde yaşam
    /// döngüsünün haberi oluyor mu, dirilme hangi canla dönüyor.
    /// </summary>
    public sealed class CombatantTests
    {
        private static Combatant NewCombatant(int maxHealth = 100, Team team = Team.None)
        {
            // REDDEDILEN - CombatantTests.cs:29 yerine:
            //     return new Combatant(new FakeHealth(), new FakeLifecycle(), profile);
            // KIRILAN  : bu dosyanın sınadığı TEK şey parçalar arasındaki kural;
            //            sahte parça tam o kuralı ölçülemez kılar.
            //            "canı bitti mi" gerçek Health kuralıyla (sıfırda
            //            durdurma) hiç uyuşturulmaz -> oyunda birim düşmez
            //            derleyici: iki tip de sealed, önce arayüz ister  .
            //            test: yeşil kalır ve artık hiçbir bütünlük ölçmez
            // KAZANIRDI: parçalardan biri ağ/dosya/rastgelelik içerseydi ya da
            //            kurulumu yavaş olsaydı — o zaman sahte parça şart.
            // TEK CUMLE: Yalıtmak, yalıtacak bir şey varsa kazançtır; saf ve
            //            hızlı parçalarda yalnızca sınanan kuralı siler.
            return new Combatant(
                new Health(maxHealth),
                new UnitLifecycle(downedWindowSeconds: 10f, corpseWindowSeconds: 5f),
                new AttackProfile(damage: 10, range: 1),
                team);
        }

        [Test]
        public void NewCombatant_StartsInAliveStateWithFullHealth()
        {
            var combatant = NewCombatant();

            Assert.That(combatant.State, Is.EqualTo(UnitState.Alive));
            Assert.That(combatant.CurrentHealth, Is.EqualTo(100));
        }

        [Test]
        public void Damage_ThatDoesNotDeplete_LeavesStateAlone()
        {
            var combatant = NewCombatant();

            combatant.TakeDamage(99);

            Assert.That(combatant.CurrentHealth, Is.EqualTo(1));
            Assert.That(combatant.State, Is.EqualTo(UnitState.Alive), "1 health is still the Alive state.");
        }

        // Bu dosyanın var olma sebebi olan test: Health ile UnitLifecycle
        // birbirini tanımıyor, bağlantıyı Combatant kuruyor.
        [Test]
        public void Damage_ThatDepletesHealth_MovesLifecycleToDowned()
        {
            var combatant = NewCombatant();

            combatant.TakeDamage(100);

            Assert.That(combatant.CurrentHealth, Is.Zero);
            Assert.That(combatant.State, Is.EqualTo(UnitState.Downed));
            Assert.That(combatant.RemainingSeconds, Is.EqualTo(10f), "The rescue window must open.");
        }

        [Test]
        public void Overkill_DoesNotPushHealthBelowZero()
        {
            var combatant = NewCombatant();

            combatant.TakeDamage(1000);

            Assert.That(combatant.CurrentHealth, Is.Zero);
            Assert.That(combatant.State, Is.EqualTo(UnitState.Downed));
        }

        // Öğrenen tasarım kararı: dirilen birim TAM canla kalkmaz.
        [Test]
        public void Revive_RestoresHalfOfMaxHealth()
        {
            var combatant = NewCombatant(maxHealth: 100);
            combatant.TakeDamage(100);

            bool revived = combatant.TryRevive();

            Assert.That(revived, Is.True);
            Assert.That(combatant.State, Is.EqualTo(UnitState.Alive));
            Assert.That(combatant.CurrentHealth, Is.EqualTo(50), "Half of max health.");
        }

        // Oran olması mimari karar: sabit sayı farklı maksimumlarda bambaşka
        // anlamlara gelirdi. 7 → 3 satırı ayrıca tam sayı bölmesinin aşağı
        // yuvarladığını sabitliyor.
        //
        // REDDEDILEN - CombatantTests.cs:111 yerine:
        //     (yalnızca yukarıdaki Revive_RestoresHalfOfMaxHealth, tek maksimum
        //      canla — 100 → 50)
        // KIRILAN  : tek maksimumda ORAN ile SABİT SAYI ayırt edilemez.
        //            ReviveHealthDivisor silinip ReviveHealthAmount = 50 yazılır
        //            -> test yeşil geçer, karar korumasız kalır
        //            derleyici: hiçbir şey der  .  test: 40 → 20 ve 400 → 200
        //            satırları o değişikliği yakalayan tek şeydir
        // KAZANIRDI: diriltme oranı tek bir birim türüne özgü olsaydı (oyunda
        //            tek maksimum can varsa) — ikinci vaka aynı şeyi tekrarlardı.
        // TEK CUMLE: Tek bir örnek formülü değil, yalnızca o örneğin sonucunu
        //            sabitler.
        [TestCase(40, ExpectedResult = 20)]
        [TestCase(400, ExpectedResult = 200)]
        [TestCase(7, ExpectedResult = 3)]
        public int Revive_ScalesWithMaxHealth(int maxHealth)
        {
            var combatant = NewCombatant(maxHealth);
            combatant.TakeDamage(maxHealth);
            combatant.TryRevive();
            return combatant.CurrentHealth;
        }

        [Test]
        public void RevivedCombatant_TakesDamageAgainNormally()
        {
            var combatant = NewCombatant(maxHealth: 100);
            combatant.TakeDamage(100);
            combatant.TryRevive();                 // 50 canla ayakta

            combatant.TakeDamage(50);

            Assert.That(combatant.State, Is.EqualTo(UnitState.Downed), "It went down for the second time.");
            Assert.That(combatant.RemainingSeconds, Is.EqualTo(10f), "A full window, not the leftover of the previous one.");
        }

        // Downed bir birim hasar ALMAYA DEVAM eder. Bu, Combatant.TakeDamage'in
        // başında reddedilen "if (!health.HasRemaining) return;" satırının canlı
        // kanıtı: erken çıkış olsaydı yerdekini bitirme yolu tamamen kapanırdı.
        /// <summary>
        /// Düşme canı boşalınca beden BİTİYOR: kurtarma penceresi kapanır.
        /// </summary>
        [Test]
        public void Downed_WhenTheDownedPoolEmpties_TheBodyIsFinished()
        {
            var combatant = NewCombatant(maxHealth: 10);
            combatant.TakeDamage(10);

            combatant.TakeDamage(5);

            Assert.That(combatant.State, Is.EqualTo(UnitState.Dead),
                "the downed pool is half of max health, so five closes it");
        }

        /// <summary>
        /// Bitirmek TEK vuruşluk bir iş değil: havuz canın yarısı kadar dayanır.
        /// </summary>
        // BU TEST OLMASAYDI havuzun büyüklüğü sessizce bire düşebilirdi ve
        // düşmüş beden ilk çentikte ölürdü — reddedilen kestirmenin ta kendisi.
        [Test]
        public void Downed_TakesMoreThanOneNickToFinish()
        {
            var combatant = NewCombatant(maxHealth: 10);
            combatant.TakeDamage(10);

            combatant.TakeDamage(4);

            Assert.That(combatant.State, Is.EqualTo(UnitState.Downed),
                "four is under the five-point downed pool");
        }

        /// <summary>
        /// Canı bir olan birim bile düştüğü karede bitirilemez.
        /// </summary>
        [Test]
        public void Downed_WithMaxHealthOne_StillSurvivesTheFrameItFellIn()
        {
            var combatant = NewCombatant(maxHealth: 1);
            combatant.TakeDamage(1);

            Assert.That(combatant.State, Is.EqualTo(UnitState.Downed),
                "integer division would give a zero pool; the floor of one prevents it");
        }

        /// <summary>
        /// Kaldırılan savaşçı düşme canını da geri alır.
        /// </summary>
        // İKİNCİ DÜŞÜŞ YENİ BİR PENCEREDİR. Havuz bırakılmasaydı bir kez
        // kaldırılan savaşçı ikinci düşüşünde ilk düşüşünün çentikleriyle doğar
        // ve tek vuruşta bitirilirdi.
        [Test]
        public void Revive_ThenFallAgain_OpensAFreshDownedPool()
        {
            var combatant = NewCombatant(maxHealth: 10);
            combatant.TakeDamage(10);
            combatant.TakeDamage(4);
            Assert.That(combatant.TryRevive(), Is.True, "kurulum bozuk");

            combatant.TakeDamage(100);
            combatant.TakeDamage(4);

            Assert.That(combatant.State, Is.EqualTo(UnitState.Downed),
                "the second fall must not inherit the first fall's damage");
        }

        [Test]
        public void Downed_StillAcceptsDamage()
        {
            var combatant = NewCombatant();
            combatant.TakeDamage(100);

            Assert.DoesNotThrow(() => combatant.TakeDamage(5));
            Assert.That(combatant.State, Is.EqualTo(UnitState.Downed), "Hitting a downed unit must not close the rescue window.");
        }

        [Test]
        public void Dead_CannotBeRevived()
        {
            var combatant = NewCombatant();
            combatant.TakeDamage(100);
            combatant.Tick(10.1f);

            Assert.That(combatant.TryRevive(), Is.False);
            Assert.That(combatant.CurrentHealth, Is.Zero, "A rejected revive must not restore health.");
        }

        [Test]
        public void Constructor_NullPart_Throws()
        {
            var health = new Health(10);
            var lifecycle = new UnitLifecycle();
            var profile = new AttackProfile(damage: 1, range: 1);

            Assert.Throws<ArgumentNullException>(() => new Combatant(null, lifecycle, profile));
            Assert.Throws<ArgumentNullException>(() => new Combatant(health, null, profile));
            Assert.Throws<ArgumentNullException>(() => new Combatant(health, lifecycle, null));
        }

        // Combatant tarafı TAŞIR, tarafla ilgili hiçbir karar VERMEZ. Aşağıdaki
        // üç test yalnızca taşımayı sınıyor; "kime saldırılır" sorusunun testi
        // TargetingRulesTests'te. Üçüncü vaka Team.None: tarafsızlık da geçerli
        // bir taraftır, kurucunun reddedeceği bir değer değil.
        //
        // REDDEDILEN - CombatantTests.cs:190 yerine (üç kabul vakası yerine bir ret):
        //     Assert.Throws<ArgumentOutOfRangeException>(
        //         () => new Combatant(health, lifecycle, profile, (Team)99));
        // KIRILAN  : Combatant'a bir doğrulama satırı BORÇLANDIRIR ve o satır
        //            Team.cs'teki değer listesini ikinci kez yazar.
        //            enum'a dördüncü değer eklenir -> ilk kırmızı bu testten
        //            gelir ve YANLIŞ yeri gösterir
        //            derleyici: hiçbir şey söylemez  .  test: kırmızı olur ama
        //            Team.cs yerine CombatantTests.cs'i suçlar
        // KAZANIRDI: takım değeri dışarıdan gelseydi — kayıt dosyası, ağ paketi,
        //            seviye editörü çıktısı; o gün (Team)99 gerçek bir girdidir
        //            ve onu sınırda durdurmak kurucunun işidir.
        // TEK CUMLE: Geçerli enum değerlerini derleyici zaten biliyorsa, aynı
        //            listeyi bir kurucuya yazdırmak testi yalancı şahit yapar.
        [TestCase(Team.None)]
        [TestCase(Team.Player)]
        [TestCase(Team.Enemy)]
        public void Constructor_StoresTeamUnchanged(Team team)
        {
            Assert.That(NewCombatant(team: team).Team, Is.EqualTo(team));
        }

        /// <summary>
        /// VARSAYILAN DEĞER KARARINI koruyan test. Kurucunun dördüncü parametresi
        /// isteğe bağlı ve atlanmış takım Team.None demek — "sessizce oyuncunun
        /// tarafı" değil.
        ///
        /// Kırmızıya dönerse varsayılan Player ya da Enemy'ye çevrilmiş demektir
        /// ve o günden sonra takımı atanmayı unutulmuş her birim bir tarafta
        /// doğar; hata ancak yanlış birime saldırılamadığında görülür.
        /// </summary>
        [Test]
        public void Constructor_WithoutTeam_DefaultsToNeutral()
        {
            Assert.That(NewCombatant().Team, Is.EqualTo(Team.None),
                "an unspecified team must mean neutral, never a silently assigned side");
        }

        // Taraf kuruluşta sabitlenir: düşüp dirilmek onu değiştirmez. Bu test,
        // readonly property'nin ÇALIŞMA ZAMANINDAKİ tanığı — derleyici zaten
        // atamayı engelliyor, ama yaşam döngüsünün tarafı sıfırlamadığını
        // gösteren başka hiçbir şey yok.
        [Test]
        public void Team_SurvivesDownAndRevive()
        {
            var combatant = NewCombatant(maxHealth: 100, team: Team.Enemy);

            combatant.TakeDamage(100);
            combatant.TryRevive();

            Assert.That(combatant.Team, Is.EqualTo(Team.Enemy), "the lifecycle must not reset the side");
        }

        // ═══ DURUM DEĞİŞİKLİĞİNİN DIŞARI VERİLMESİ ═══════════════════
        // Bu bölümün sınadığı tek şey ÇEVİRİdir: UnitLifecycle'ın tek değerli
        // olayı ("hangi duruma") burada iki değerli hâle geliyor ("nereden
        // nereye"). Geçişlerin kendisi UnitLifecycleTests'te zaten sınanıyor;
        // burada tekrarlanmıyor.
        //
        // ── HARİTA: çevirinin hangi durakta olduğu ──────────────────
        //   UnitLifecycle.StateChanged   Action<UnitState>          1 değer
        //             │  += OnLifecycleStateChanged  (metot grubu)
        //             ▼
        //   Combatant.StateChanged       Action<UnitState,UnitState> 2 değer
        //             │  += log.Record               (metot grubu)   ◄── BU BÖLÜM
        //             ▼
        //   TransitionLog                iki paralel liste
        //
        // Bir üstteki durak (Battle'ın kimliği ekleyişi) bu dosyada YOK; onun
        // testleri BattleTests'te. Ölçü: aşağıdaki testlerin hiçbiri bir Unit
        // kurmuyor — kimlik bu katmanda henüz yok.
        // DERİN ANLATIM: Docs/deep/konular/01-olay-zinciri.md

        // Geçişleri sırasıyla biriktiren en küçük kap. İki paralel liste
        // yerine tek tip: hangi "nereden"in hangi "nereye" ile geldiği
        // indeksle değil, kabın kendisiyle korunuyor.
        private sealed class TransitionLog
        {
            public readonly List<UnitState> From = new List<UnitState>();
            public readonly List<UnitState> To = new List<UnitState>();

            public int Count => From.Count;

            public void Record(UnitState from, UnitState to)
            {
                From.Add(from);
                To.Add(to);
            }
        }

        /// <summary>
        /// ÇEVİRİNİN KENDİSİNİ koruyan test: olay "nereden" bilgisini taşımalı.
        ///
        /// Önceki durum yeni durumdan TÜRETİLSEYDİ bu test bugün yine yeşil
        /// kalırdı — geçiş tablosu şu an tek yönlü. Bu yüzden aşağıdaki üç vaka
        /// birlikte yazıldı: üçü de aynı anda doğru kalmayan bir türetme yazmak
        /// zordur ve tabloyu ikinci kez yazan her deneme burada takılır.
        /// </summary>
        [Test]
        public void StateChanged_CarriesBothTheOldAndTheNewState()
        {
            var combatant = NewCombatant(maxHealth: 10);
            var log = new TransitionLog();
            // ÖDÜNÇ ALINAN — metot grubu (`log.Record`): parantez YOK, yani
            // çağrı değil. Derleyici `log` ALICISINI de içine alan bir delege
            // üretiyor; olay listesinde duran şey `Record` metodu değil, "şu
            // log nesnesinin Record'u" çiftidir. Aşağıdaki
            // StateChanged_IsPerCombatant_NotShared testi tam bu çiftin
            // örneğe bağlı olduğunu ölçüyor.
            // DİL: Docs/deep/dil/04-delege-olay-ve-kapanis.md
            combatant.StateChanged += log.Record;

            combatant.TakeDamage(10);   // Alive -> Downed
            combatant.TryRevive();      // Downed -> Alive
            combatant.TakeDamage(10);   // Alive -> Downed
            combatant.Tick(10.1f);      // Downed -> Dead

            Assert.That(log.Count, Is.EqualTo(4));
            Assert.That(log.From, Is.EqualTo(new[]
            {
                UnitState.Alive, UnitState.Downed, UnitState.Alive, UnitState.Downed
            }));
            Assert.That(log.To, Is.EqualTo(new[]
            {
                UnitState.Downed, UnitState.Alive, UnitState.Downed, UnitState.Dead
            }));
        }

        /// <summary>
        /// "BİR ŞEY OLDU" İLE "DURUM DEĞİŞTİ" AYRIMINI koruyan test —
        /// <see cref="UnitLifecycle"/> katmanındaki ikizinin bu tipteki hâli.
        ///
        /// Üçüncü <c>Tick</c> ceset süresini bitirir ve
        /// <c>IsReadyForCleanup</c>'ı açar; bu bir geçiş DEĞİLDİR ve olay
        /// tetiklenmemelidir. Kırmızıya dönerse toplu süpürme ile durum olayı
        /// aynı soruyu cevaplamaya başlamış demektir ve ikisinden biri
        /// gereksizleşmiş gibi görünür — oysa gereksizleşen hiçbiri değildir.
        /// </summary>
        [Test]
        public void StateChanged_DoesNotFireWhenOnlyTheCleanupFlagOpens()
        {
            var combatant = NewCombatant(maxHealth: 10);
            combatant.TakeDamage(10);
            combatant.Tick(10.1f);      // Downed -> Dead

            var log = new TransitionLog();
            combatant.StateChanged += log.Record;
            combatant.Tick(5.1f);       // yalnızca temizlik bayrağı

            Assert.That(combatant.IsReadyForCleanup, Is.True, "setup");
            Assert.That(log.Count, Is.Zero,
                "becoming ready for cleanup is not a state transition");
        }

        /// <summary>
        /// Aynı duruma "geçişin" olay üretmediğini koruyan test. Düşmüş bir
        /// birime tekrar vurmak onu yeniden düşürmez; dinleyici aynı geçişi iki
        /// kez duyup iki kez ses çalmamalı.
        /// </summary>
        [Test]
        public void StateChanged_DoesNotFireForARepeatedHitOnADownedCombatant()
        {
            var combatant = NewCombatant(maxHealth: 10);
            combatant.TakeDamage(10);

            var log = new TransitionLog();
            combatant.StateChanged += log.Record;

            // SAYI BU TURDA 5'TEN 4'E İNDİ VE SEBEBİ KURALIN KENDİSİ: düşme canı
            // artık canın yarısı, yani burada 5. Beş hasar havuzu TAM boşaltır ve
            // bitirici geçişi tetiklerdi — testin ölçmek istediği şey o geçiş
            // değil, DÜŞME geçişinin tekrar etmemesi. Dört hasar havuzu boşaltmaz
            // ve iddia olduğu gibi kalır.
            combatant.TakeDamage(4);

            Assert.That(log.Count, Is.Zero,
                "hitting a downed combatant repeats no transition");
        }

        [Test]
        public void StateChanged_DoesNotFireWhileTheCombatantIsBeingBuilt()
        {
            // Kurucudaki ilk durum bir GEÇİŞ değil, bir BAŞLANGIÇtır — ve o anda
            // abone olabilmiş kimse zaten yoktur. Test bunu dolaylı olarak
            // sabitliyor: kurulumdan hemen sonra abone olan bir dinleyici,
            // gerçekten bir şey olana kadar sessiz kalmalı.
            //
            // BU TESTİN KORUMADIĞI ŞEY, dürüstçe: Combatant kurucusu aboneliği
            // BÜTÜN null denetimlerinden SONRA kuruyor ve bu bir karardır —
            // denetimlerden biri patlarsa geriye abone olunmuş bir UnitLifecycle
            // kalmamalı. Buradaki test o sırayı ölçmez, çünkü kurucu zaten
            // başarılı: ölçtüğü şey "başlangıç olay yaymaz". Sıra kararını
            // ölçecek test, patlayan bir kurucudan SONRA lifecycle'ın hâlâ
            // abonesiz olduğunu iddia ederdi; bu dosyada Constructor_NullPart_Throws
            // yalnızca istisnanın tipini görüyor. Aynı kararın Battle
            // katmanındaki ikizi KORUNUYOR — BattleTests'teki
            // AddUnit_OutsideTheGrid_ThrowsAndRegistersNothing reddedilen bir
            // eklemeden sonra sözlüğün boş kaldığını iddia ediyor. Boşluk
            // burada, ve bilerek kayda geçiyor.
            var combatant = NewCombatant(maxHealth: 10);
            var log = new TransitionLog();

            combatant.StateChanged += log.Record;

            Assert.That(combatant.State, Is.EqualTo(UnitState.Alive));
            Assert.That(log.Count, Is.Zero);
        }

        /// <summary>
        /// İKİ SAVAŞÇININ OLAYLARININ AYRI olduğunu koruyan test. Abonelik
        /// örneğe aittir; bir savaşçının düşmesi diğerinin dinleyicisine
        /// ulaşmamalı.
        ///
        /// Kırmızıya dönerse olay bir yerde <c>static</c> hâle gelmiş demektir —
        /// ve o gün ekranda yanlış birim yere düşer.
        /// </summary>
        [Test]
        public void StateChanged_IsPerCombatant_NotShared()
        {
            var first = NewCombatant(maxHealth: 10);
            var second = NewCombatant(maxHealth: 10);
            var log = new TransitionLog();
            first.StateChanged += log.Record;

            second.TakeDamage(10);

            Assert.That(log.Count, Is.Zero,
                "one combatant's transition must not reach another combatant's listeners");
        }
    }
}
