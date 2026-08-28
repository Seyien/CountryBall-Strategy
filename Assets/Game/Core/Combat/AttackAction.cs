using System;

namespace GridStrategy.Combat
{
    // ═══ ROL: AKIŞ SAHİBİ (transaction script) ═══════════════════════
    // kimlik : yok — static; iki Execute çağrısını ayıracak bir şey yoktur
    // hafıza : yok — ama ölçüsü "aynı üçlü aynı sonucu verir" DEĞİL, çünkü
    //          vermiyor: Execute hedefi DEĞİŞTİRİR. 20 canlı bir hedefe 10
    //          hasarla arka arkaya iki kez çağır; birincisi Hit, ikincisi
    //          HitAndDowned döner. Farkı doğuran şey burada saklanan bir alan
    //          değil, hedefin kendi canı: hafıza Combatant'ta, burada değil
    // Unity  : gerekmez — mesafe hazır gelir, sahne kurmak gerekmez
    // karar  : AKIŞI yürütür — hangi kurala hangi SIRAYLA sorulacağını bilir,
    //          ama kuralların hiçbirini kendisi yazmaz
    /// <summary>
    /// Saldırı akışının tek sahibi — ve var olma sebebi tek cümle: parçalar
    /// hazırdı ama <b>kimse "saldır" demiyordu</b>. <see cref="AttackResolver"/>
    /// menzili ölçer, <see cref="TargetingRules"/> uygunluğu söyler,
    /// <see cref="Combatant"/> ya da <see cref="Structure"/> hasarı uygular;
    /// hiçbiri diğerini TANIMAZ ve tanımamalıdır. Onları bir sıraya dizen tek yer
    /// burasıdır.
    ///
    /// İKİ SALDIRAN TİPİ ÇARPI İKİ HEDEF TİPİ, DÖRT AŞIRI YÜKLEME — tek akış
    /// şekli. Sıra DÖRTLÜ ve her
    /// basamağı bir karar: önce SALDIRANIN durumu (<see cref="AttackRules"/>),
    /// sonra HEDEFİN uygunluğu, sonra MENZİL, en sonda BEKLEME süresi. Genel
    /// ilke: çağıranın düzeltemeyeceği sebep önce söylenir; beklemenin en sonda
    /// olması bu ilkenin devamıdır ve gerekçesi ilk aşırı yüklemede yazılı.
    ///
    /// Takım sorusunu ve saldıranın kendi durumunu ARTIK KENDİSİ sorar; ikisi de
    /// bir üst katmanda kalmış iki borcun kapanışıdır.
    ///
    /// Neyi BİLMEZ: mesafenin nasıl ölçüldüğünü, sırayı kimin verdiğini, sonucu
    /// kimin göstereceğini, saldırının kaç kez tekrarlanacağını.
    ///
    /// GEREKÇELER: Docs/deep/kod/Core/Combat/AttackAction.md
    /// </summary>
    public static class AttackAction
    {
        // İKİ BİLEŞİĞİN ARASINDAKİ KURAL İKİSİNİN DE DIŞINDA DURUR. Metot
        // Combatant'ın içine taşınsaydı bileşiğin sınırı "kendi parçalarım"dan
        // "tahtadaki herkes"e genişler, Combatant TargetingRules ile
        // AttackResolver'ı tanımak zorunda kalırdı. Ayıraç nesne sayısı: tek
        // nesnenin kuralı içeride, İKİ nesnenin kuralı ikisinin de dışında durur.
        // → AttackAction.md#attackaction-tip

        /// <summary>
        /// Bir saldırı denemesini yürütür ve ne olduğunu döndürür.
        /// Mesafe DIŞARIDAN gelir; bu tip iki birimin nerede durduğunu bilmez.
        /// Cevap merdiveni üç basamak: saldıranın durumu, hedef uygunluğu, menzil.
        /// </summary>
        // → AttackAction.md#executecombatant-attacker-combatant-target-int-distance
        // DERİN ANLATIM: Docs/deep/konular/02-assembly-duvari.md
        public static AttackOutcome Execute(Combatant attacker, Combatant target, int distance)
        {
            if (attacker == null)
            {
                throw new ArgumentNullException(nameof(attacker));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            // SALDIRANIN DURUMU EN BAŞTA SORULUYOR, ve yeri burası: kuralı
            // uygulayabilen en alt katman bu tip, çünkü kural UnitState'i okur ve
            // UnitState bu ad alanında yaşıyor. SIRA BİR CEVAP KARARIDIR — çağıranın
            // DÜZELTEMEYECEĞİ sebep en önce söylenir; bu blok hedef ve menzil
            // kontrollerinin altına taşınsaydı çağıran boş yere hedef değiştirir ya
            // da yaklaşır, her seferinde buraya çarpardı.
            // → AttackAction.md#executecombatant-attacker-combatant-target-int-distance
            if (!AttackRules.CanAttack(attacker.State))
            {
                return AttackOutcome.RejectedActorCannotAct;
            }

            // TAKIM SORUSU BURADA SORULUYOR — bir borcun kapanışı. Kural, onu
            // UYGULAYABİLEN en alt katmana indi: önceden dost ateşini yalnızca
            // BattleActions engelliyordu ve bu metodu DOĞRUDAN çağıran kendi
            // takımını vurabiliyordu. SIRA da bir karardır: önce hedef uygunluğu,
            // sonra menzil — cesede "menzil dışı" denirse yapay zekâ boşuna yaklaşır.
            // → AttackAction.md#executecombatant-attacker-combatant-target-int-distance
            if (!TargetingRules.CanBeAttacked(target.State, attacker.Team, target.Team))
            {
                return AttackOutcome.RejectedInvalidTarget;
            }

            if (!AttackResolver.IsWithinRange(distance, attacker.AttackProfile))
            {
                return AttackOutcome.RejectedOutOfRange;
            }

            // BEKLEME EN SON KAPI, ve yeri bir karar: buraya kadar gelen bir
            // saldırıda hedef geçerli VE menzilde demektir, yani "henüz yeniden
            // vuramaz" cevabı oyuncuya gerçekten yapabileceği tek şeyi söyler —
            // beklemek. Bekleme saldıranın durumuyla birlikte en BAŞA konsaydı,
            // sayacı dolu bir okçu uzaktaki bir cesede tıklandığında "yeniden
            // vuramaz" derdi; oyuncu bekler, sonra "menzil dışı" duyar ve
            // yaklaşır, sonra "geçersiz hedef" duyar. Aynı yanlış sıra
            // AttackAction'ın hedef-menzil kararında da ölçülmüştü.
            // SORMAK VE HARCAMAK TEK ÇAĞRI: `IsAttackReady` okunup sayaç ayrı
            // bir satırda başlatılsaydı, dört aşırı yüklemenin birinde unutulan
            // ikinci satır o dalı sınırsız hasara açardı.
            if (!attacker.TryBeginAttackCooldown())
            {
                return AttackOutcome.RejectedOnCooldown;
            }

            // Durumu vuruştan ÖNCE oku. Sonucu ayırt etmenin tek yolu bu:
            // "düştü mü" sorusu bir DEĞİŞİM sorusudur, bir durum sorusu değil.
            // Sonradan okunan State tek başına yeterli olmaz — hedef zaten
            // Downed'ken vurulmuş da olabilir.
            UnitState stateBeforeHit = target.State;

            target.TakeDamage(attacker.AttackProfile.Damage);

            // "DÜŞTÜ MÜ" BİR DEĞİŞİM SORUSUDUR, durum sorusu değil. Tek okuma
            // (yalnızca target.State) zaten Downed olan hedefe yapılan vuruşta da
            // "HitAndDowned" derdi: enkaza her vuruşta düşme animasyonu oynar,
            // skor tablosu aynı birim için defalarca puan yazar. İki koşul iki ayrı
            // yalanı eler; `stateBeforeHit`i koruyan şey bir modifier değil,
            // atamanın TakeDamage çağrısından ÖNCE olmasıdır.
            // → AttackAction.md#executecombatant-attacker-combatant-target-int-distance
            return Describe(stateBeforeHit, target.State);
        }

        /// <summary>
        /// Bir YAPIYA yapılan saldırı denemesini yürütür. Akışın şekli birim
        /// sürümüyle aynıdır — saldıranın durumu, hedef uygunluğu, menzil, hasar —
        /// çünkü SIRA kararı hedefin ne olduğuna bağlı değil.
        ///
        /// İki yerde ayrışır ve ikisi de hedefin doğasından gelir: uygunluğu
        /// <see cref="StructureState"/> söyler (bir baraka düşmez, yıkılır) ve
        /// ölüm olayının adı <see cref="AttackOutcome.HitAndDestroyed"/>'dır.
        /// </summary>
        // İKİ AŞIRI YÜKLEME, TEK AKIŞ ŞEKLİ: ortak bir IAttackTarget arkasında tek
        // gövdeye inilseydi hedef uygunluğu kuralı TargetingRules'tan HEDEFİN
        // İÇİNE taşınır, Combatant ile Structure ikisi de o kuralı tanımak zorunda
        // kalır ve arayüzün bool'u Downed ile Destroyed'ı aynı cevabın arkasına
        // düşürürdü. Soyutlamanın bugün sildiği tek şey iki metot.
        // → AttackAction.md#executecombatant-attacker-structure-target-int-distance
        public static AttackOutcome Execute(Combatant attacker, Structure target, int distance)
        {
            if (attacker == null)
            {
                throw new ArgumentNullException(nameof(attacker));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            // SALDIRANIN DURUMU BURADA DA SORULUYOR — tekrar değil, kuralın İKİ
            // akışta da uygulanması. Kuralın METNİ tek yerde (AttackRules); burada
            // yalnızca soruluyor. Bu satır olmasaydı düşmüş bir birim askere
            // vuramaz ama barakayı yıkabilirdi. AttackRules'ın yapı ikizi ARTIK
            // VAR (CanStructureAttack) ve bu satır onu çağırmıyor: burada saldıran
            // bir Combatant'tır, ayrımı yapan şey aşırı yüklemenin kendisi.
            // → AttackAction.md#executecombatant-attacker-structure-target-int-distance
            if (!AttackRules.CanAttack(attacker.State))
            {
                return AttackOutcome.RejectedActorCannotAct;
            }

            // Sıra birim sürümüyle AYNI ve gerekçesi de aynı: menzil dışındaki bir
            // ENKAZA saldırınca cevap "menzil dışı" olsaydı, yapay zekâ yaklaşır ve
            // yine reddedilirdi — sonsuz döngü, ve hiçbir test kırmızı olmazdı.
            if (!TargetingRules.CanBeAttacked(target.State, attacker.Team, target.Team))
            {
                return AttackOutcome.RejectedInvalidTarget;
            }

            if (!AttackResolver.IsWithinRange(distance, attacker.AttackProfile))
            {
                return AttackOutcome.RejectedOutOfRange;
            }

            // Bekleme yine EN SON kapı ve gerekçesi birim hedefli sürümde tek
            // kez yazılı; değişen tek şey hedefin tipi. Kapı buradan da
            // eksilmiyor: bu satır olmasaydı hızlı tıklayan oyuncu barakayı bir
            // karede yıkardı.
            if (!attacker.TryBeginAttackCooldown())
            {
                return AttackOutcome.RejectedOnCooldown;
            }

            // BURADA "ÖNCEKİ DURUMU OKU" DESENİ YOK — eksiklik değil, sözleşme
            // farkı: Structure.TakeDamage "bu vuruş yıktı mı" cevabını zaten
            // DÖNDÜRÜYOR, Combatant.TakeDamage ise void. Deseni buraya kopyalamak
            // kararı sahibinden geri almak olurdu ve yıkım koşulu değiştiği gün
            // (can sıfıra inmeden çöken bina) buradaki ikinci karar eskirdi.
            // → AttackAction.md#executecombatant-attacker-structure-target-int-distance
            return target.TakeDamage(attacker.AttackProfile.Damage)
                ? AttackOutcome.HitAndDestroyed
                : AttackOutcome.Hit;
        }

        // ─────────────────────────────────────────────────────────────
        // SALDIRAN BİR YAPI — ölçülmüş bir P0'ın kapanışı
        //
        // Structure.CanAttack ile Structure.AttackProfile yazılmıştı, blueprint
        // varlığı hasar ve menzil alanlarını taşıyordu, ama saldıran-yapı aşırı
        // yüklemesi hiç yoktu: oyuncu kendi kulesini seçip düşmana tıkladığında
        // akış barakayı bir Combatant sanıp ArgumentException fırlatıyordu.
        // Aşağıdaki iki metot o boşluğun tamamı.
        //
        // SIRA BİREBİR AYNI — saldıranın durumu, hedefin uygunluğu, menzil,
        // hasar. Kopyalanan şey bir kural değil bir SIRA; dördünün de metni
        // hâlâ tek sahiplerinde duruyor.
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Bir YAPININ bir savaşçıya saldırısını yürütür.
        ///
        /// OYUNDA NE İŞE YARAR: oyuncunun kulesi menzilindeki düşman askerine
        /// ateş eder. Kule enkazsa, düşman değilse ya da uzaktaysa hiçbir hasar
        /// inmez ve sebep dönüş değerinde okunur.
        /// </summary>
        public static AttackOutcome Execute(Structure attacker, Combatant target, int distance)
        {
            if (attacker == null)
            {
                throw new ArgumentNullException(nameof(attacker));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (!AttackRules.CanStructureAttack(attacker.State))
            {
                return AttackOutcome.RejectedActorCannotAct;
            }

            // SALDIRMAYAN YAPI BİR İSTİSNA DEĞİL, KURALIN KENDİSİ — yapıların
            // ÇOĞU saldırmaz ve Structure.AttackProfile bu yüzden isteğe bağlı.
            // Buraya bir ArgumentNullException konsaydı, bir depoya tıklayan
            // oyuncu oyunu patlatırdı; ret ise ona yalnızca "bu bina vurmaz"
            // der. İki soru ayrı iki dal çünkü ayrı iki şey soruyorlar: üstteki
            // yıkılıp yıkılmadığını, bu hiç silahı olup olmadığını.
            if (!attacker.CanAttack)
            {
                return AttackOutcome.RejectedActorCannotAct;
            }

            if (!TargetingRules.CanBeAttacked(target.State, attacker.Team, target.Team))
            {
                return AttackOutcome.RejectedInvalidTarget;
            }

            if (!AttackResolver.IsWithinRange(distance, attacker.AttackProfile))
            {
                return AttackOutcome.RejectedOutOfRange;
            }

            // Kulenin beklemesi de EN SON kapı ve sayacı KULENİN KENDİSİ tutar:
            // bu turda kule menzilindeki düşmanı gördüğü her karede ateş
            // ediyordu, yani hasar kare hızına bağlıydı. Sayaç yapı başına
            // olduğu için yan yana duran iki kule birbirinin sırasını beklemez.
            if (!attacker.TryBeginAttackCooldown())
            {
                return AttackOutcome.RejectedOnCooldown;
            }

            // "Düştü mü" yine bir DEĞİŞİM sorusu ve deseni birim sürümünden
            // devralıyor; gerekçesi orada tek kez yazılı, burada tekrar
            // edilmiyor. Değişen tek şey saldıranın tipi.
            UnitState stateBeforeHit = target.State;

            target.TakeDamage(attacker.AttackProfile.Damage);

            return Describe(stateBeforeHit, target.State);
        }

        /// <summary>
        /// Bir YAPININ başka bir yapıya saldırısını yürütür — kule barakayı
        /// yıkar.
        ///
        /// Dördüncü aşırı yükleme uydurma bir tamlık kaygısıyla değil, akışın
        /// gerçek bir dalını kapattığı için var: saldıranı yapı olarak bulan
        /// çağıran hedefin ne olduğunu ayrıca soruyor ve dört bileşimin dördü
        /// de sahada oluşabiliyor.
        /// </summary>
        public static AttackOutcome Execute(Structure attacker, Structure target, int distance)
        {
            if (attacker == null)
            {
                throw new ArgumentNullException(nameof(attacker));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (!AttackRules.CanStructureAttack(attacker.State))
            {
                return AttackOutcome.RejectedActorCannotAct;
            }

            if (!attacker.CanAttack)
            {
                return AttackOutcome.RejectedActorCannotAct;
            }

            if (!TargetingRules.CanBeAttacked(target.State, attacker.Team, target.Team))
            {
                return AttackOutcome.RejectedInvalidTarget;
            }

            if (!AttackResolver.IsWithinRange(distance, attacker.AttackProfile))
            {
                return AttackOutcome.RejectedOutOfRange;
            }

            // Dördüncü dalda da aynı kapı: dördünden birinde eksik kalsaydı
            // oyuncu o bileşimi bulur ve beklemesiz vururdu.
            if (!attacker.TryBeginAttackCooldown())
            {
                return AttackOutcome.RejectedOnCooldown;
            }

            // Yıkım kararının sahibi yine Structure.TakeDamage'ın dönüşü;
            // "önceki durumu oku" deseni burada da yok ve sebebi savaşçı hedefli
            // yapı sürümünde tek kez yazılı.
            return target.TakeDamage(attacker.AttackProfile.Damage)
                ? AttackOutcome.HitAndDestroyed
                : AttackOutcome.Hit;
        }

        /// <summary>
        /// Bir vuruşun oyuncuya nasıl anlatılacağını, hedefin vuruştan ÖNCEKİ ve
        /// SONRAKİ hâlini karşılaştırarak seçer.
        /// </summary>
        // ÜÇ CEVAP TEK KARŞILAŞTIRMADAN ÇIKIYOR ve ortak olmasının sebebi iki
        // aşırı yüklemenin aynı soruyu sorması: saldıranın tipi savaşçı da yapı
        // da olsa hedefin geçirdiği değişim aynı. İkinci bir kopya yazılsaydı
        // bitirici vuruş bir yolda anlatılır, ötekinde susardı.
        private static AttackOutcome Describe(UnitState before, UnitState after)
        {
            if (before == UnitState.Alive && after == UnitState.Downed)
            {
                return AttackOutcome.HitAndDowned;
            }

            // BİTİRİCİ VURUŞ. Karşılaştırma yine DEĞİŞİM üstünden: hedef zaten
            // ölüyken gelen vuruş hiçbir şeyi değiştirmez ve bu satır ona
            // "bitirdin" dedirtmez.
            if (before == UnitState.Downed && after == UnitState.Dead)
            {
                return AttackOutcome.HitAndFinished;
            }

            return AttackOutcome.Hit;
        }

    }
}
