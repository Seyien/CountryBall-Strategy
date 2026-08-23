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
    /// İKİ HEDEF TİPİ, İKİ AŞIRI YÜKLEME — tek akış şekli. Sıra ÜÇLÜ ve her
    /// basamağı bir karar: önce SALDIRANIN durumu (<see cref="AttackRules"/>),
    /// sonra HEDEFİN uygunluğu, sonra MENZİL. Genel ilke: çağıranın
    /// düzeltemeyeceği sebep önce söylenir.
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
            return stateBeforeHit == UnitState.Alive && target.State == UnitState.Downed
                ? AttackOutcome.HitAndDowned
                : AttackOutcome.Hit;
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
            // vuramaz ama barakayı yıkabilirdi. AttackRules'ın yapı ikizi YOK:
            // yapı saldırmaz, saldıran hep bir Combatant'tır.
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
    }
}
