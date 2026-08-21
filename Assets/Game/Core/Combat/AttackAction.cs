using System;

namespace GridStrategy.Combat
{
    // ═══ ROL: KURAL (Policy) ═════════════════════════════════════════
    // kimlik : yok — static; iki Execute çağrısını ayıracak bir şey yoktur
    // hafıza : yok — aynı üçlü (saldıran, hedef, mesafe) aynı sonucu verir
    // Unity  : gerekmez — mesafe hazır gelir, sahne kurmak gerekmez
    // karar  : AKIŞI yürütür — hangi kurala hangi SIRAYLA sorulacağını bilir,
    //          ama kuralların hiçbirini kendisi yazmaz
    /// <summary>
    /// Saldırı akışının tek sahibi — ve bu dosyanın var olma sebebi tek cümle:
    /// parçalar hazırdı ama <b>kimse "saldır" demiyordu</b>.
    ///
    /// <see cref="AttackResolver"/> menzili ölçer, <see cref="TargetingRules"/>
    /// uygunluğu söyler, <see cref="Combatant"/> ya da <see cref="Structure"/>
    /// hasarı uygular. Hiçbiri diğerini TANIMAZ ve tanımamalıdır. Onları bir
    /// sıraya dizen tek yer burasıdır.
    ///
    /// İKİ HEDEF TİPİ, İKİ AŞIRI YÜKLEME — tek akış şekli. Birim ve yapı aynı
    /// sırayı paylaşır (hedef uygunluğu → menzil → hasar) çünkü sıra kararı
    /// hedefin ne olduğuna bağlı değil; ayrıştıkları iki nokta da hedefin
    /// doğasından gelir ve alt taraftaki aşırı yüklemede yazılı.
    ///
    /// TAKIM SORUSUNU ARTIK KENDİSİ SORAR. Eskiden sormuyordu ve dost ateşi
    /// kuralı yalnızca <c>BattleActions</c>'ta yaşıyordu — yani aynı kural iki
    /// katmanda iki farklı cevap veriyordu. Kural indi; gerekçesi Execute'un
    /// içindeki REDDEDILEN bloğunda.
    ///
    /// SALDIRANIN KENDİ DURUMUNU DA ARTIK KENDİSİ SORAR. Bu, takım sorusunun
    /// ikizi olan ikinci bir borcun kapanışıdır: hedefin durumu soruluyordu,
    /// SALDIRANIN durumu sorulmuyordu ve düşmüş bir birim hâlâ vurabiliyordu.
    /// Kuralın sahibi <see cref="AttackRules"/>; soru burada soruluyor çünkü
    /// kuralı UYGULAYABİLEN en alt katman bu tip — akışı yürüten tek yer.
    ///
    /// SIRA ÜÇLÜ VE HER BASAMAĞI BİR KARAR: önce SALDIRANIN durumu, sonra
    /// HEDEFİN uygunluğu, sonra MENZİL. Genel ilke aynı: cevabı düzeltmenin
    /// çağırana bir şey kazandırmadığı sebep önce söylenir.
    ///
    /// Neyi BİLMEZ: mesafenin nasıl ölçüldüğünü (hazır alır — aynı desen
    /// <see cref="AttackResolver"/>'da da var), sırayı kimin verdiğini, sonucu
    /// kimin göstereceğini, saldırının kaç kez tekrarlanacağını.
    /// </summary>
    public static class AttackAction
    {
        // REDDEDILEN - AttackAction.cs:65 yerine (bu sınıf hiç doğmaz,
        //              metot Combatant'a taşınır):
        //     public AttackOutcome Attack(Combatant target, int distance)
        //     {
        //         if (!TargetingRules.CanBeAttacked(target.State)) ...
        //     }
        // KIRILAN  : bileşiğin sınırı "kendi parçalarım"dan "tahtadaki herkes"e genişler.
        //            Combatant TargetingRules ve AttackResolver'ı tanımak zorunda kalır
        //            saldırıyı sınamak için HER ZAMAN iki tam Combatant kurulur
        //            derleyici: hiçbir şey der  .  test: kurulum maliyeti sessizce iki katı
        // KAZANIRDI: saldırı saldıranın İÇ durumuna bağlı olsaydı — bekleme süresi,
        //            öfke birikimi, mermi sayısı — kural alanları okumak zorunda kalırdı.
        // TEK CUMLE: Bileşik kendi parçaları arasındaki kuralı yürütür; İKİ bileşik
        //            arasındaki kural ikisinin de dışında durmak zorundadır.

        /// <summary>
        /// Bir saldırı denemesini yürütür ve ne olduğunu döndürür.
        /// Mesafe DIŞARIDAN gelir; bu tip iki birimin nerede durduğunu bilmez.
        /// </summary>
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

            // SALDIRANIN DURUMU EN BAŞTA SORULUYOR — ve yeri burası, bir üst
            // katman değil. Kuralı uygulayabilen en alt katman bu tip: kural
            // UnitState'i sormak zorunda ve UnitState burada, bu ad alanında
            // yaşıyor. BattleActions'ta sorulsaydı bu metodu DOĞRUDAN çağıran
            // (yapay zekâ, tekrar kaydı, gelecekteki ikinci bir akış) düşmüş
            // birimle vurmaya devam ederdi ve hiçbir test kırmızı olmazdı —
            // dost ateşi borcunun aynısı, bir kural sonra.
            //
            // Alternatif: soruyu BattleActions'ta sormak. Seçilmedi: ikinci bir çağıran aynı boşluğu sessizce yeniden açar.
            //
            // SIRA BİR KARARDIR: saldıranın durumu, hedefin uygunluğundan da
            // ÖNCE sorulur.
            //
            // REDDEDILEN - AttackAction.cs:108 yerine (bu blok hedef ve menzil
            //              kontrollerinin ALTINA taşınır):
            //     if (!TargetingRules.CanBeAttacked(target.State, attacker.Team, target.Team)) ...
            //     if (!AttackResolver.IsWithinRange(distance, attacker.AttackProfile)) ...
            //     if (!AttackRules.CanAttack(attacker.State))
            //     {
            //         return AttackOutcome.RejectedActorCannotAct;
            //     }
            // KIRILAN  : cevap, çağıranı düzeltemeyeceği bir şeyi düzeltmeye çağırır.
            //            düşmüş birim geçersiz hedefe vurur -> cevap "geçersiz hedef"
            //            çağıran hedefi değiştirir, yaklaşır -> her seferinde reddedilir
            //            yapay zekâ bütün hedef listesini boşuna tarar
            //            derleyici: hiçbir şey der  .  test: Execute_DownedAttackerOutOfRange_
            //            PrefersActorCannotActOverOutOfRange tam bunun için yazıldı
            // KAZANIRDI: saldıranın durumunu okumak PAHALI, hedef uygunluğu ucuz olsaydı —
            //            ucuz eleme önce yapılır; bugün ikisi de bir enum karşılaştırması.
            // TEK CUMLE: Sıra bir kurgu değil bir CEVAP kararıdır: çağıranın
            //            düzeltemeyeceği sebep en önce söylenir.
            if (!AttackRules.CanAttack(attacker.State))
            {
                return AttackOutcome.RejectedActorCannotAct;
            }

            // TAKIM SORUSU BURADA SORULUYOR — ve bu, bir borcun kapanışıdır.
            // Önceden bu satır tek parametreli CanBeAttacked(UnitState)'ti ve
            // takımı hiç sormuyordu; dost ateşini yalnızca BattleActions
            // engelliyordu. Aynı kural iki katmanda iki farklı cevap veriyordu:
            // akıştan geçen saldırı reddediliyor, bu metodu DOĞRUDAN çağıran
            // (yapay zekâ, tekrar kaydı, gelecekteki ikinci bir akış) kendi
            // takımını vurabiliyordu. Kural artık tek katmanda yaşıyor.
            //
            // REDDEDILEN - AttackAction.cs:137 yerine (soru burada sorulmaz,
            //              her akış kendi ön kontrolünü taşır):
            //     if (!TargetingRules.CanBeAttacked(target.State))
            // KIRILAN  : "dost ateşi yok" kuralı, kuralın kendisine değil ÇAĞIRANIN
            //            dikkatine bağlı kalır.
            //            BattleActions'tan geçen testler yeşil kalır
            //            yapı aşırı yüklemesi boşluğu ikinci kez açar -> kendi barakanı yıkarsın
            //            derleyici: hiçbir şey der  .  test: ikinci çağıran doğunca da yeşil
            // KAZANIRDI: dost ateşi bir oyun KİPİ olsaydı — topçu saçılması, "zorlu" kip —
            //            ama doğru şekil yine kipi parametre alan bir TargetingRules kuralıdır.
            // TEK CUMLE: Bir kural, onu UYGULAYABİLEN en alt katmanda sorulur; yukarıda
            //            kalan kural yalnızca bir yoldan geçenleri korur.
            //
            // SIRA BİR KARARDIR: önce hedef uygunluğu, sonra menzil.
            //
            // Alternatif: menzili önce sormak. Seçilmedi: cesede "menzil dışı" denince yapay zekâ yaklaşır ve yine reddedilir.
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

            // REDDEDILEN - AttackAction.cs:168 yerine:
            //     return target.State == UnitState.Downed
            //         ? AttackOutcome.HitAndDowned
            //         : AttackOutcome.Hit;
            // KIRILAN  : DURUM sorusu, DEĞİŞİM sorusunun yerine geçer.
            //            zaten Downed olan hedefe vuruş -> yine "HitAndDowned" döner
            //            çağıran her vuruşta düşme animasyonu oynatır
            //            skor tablosu aynı birim için defalarca puan yazar
            //            derleyici: hiçbir şey der  .  test: DEĞİŞİM sınanana kadar yeşil
            // KAZANIRDI: Downed bir hedefe saldırmak yasak olsaydı — o zaman bu satıra
            //            gelindiğinde hedef kesinlikle Alive olurdu.
            // TEK CUMLE: "Düştü mü" bir değişim sorusudur, ve tek bir okumayla
            //            cevaplanan her değişim sorusu er ya da geç yalan söyler.
            return stateBeforeHit == UnitState.Alive && target.State == UnitState.Downed
                ? AttackOutcome.HitAndDowned
                : AttackOutcome.Hit;
        }

        /// <summary>
        /// Bir YAPIYA yapılan saldırı denemesini yürütür. Akışın şekli birim
        /// sürümüyle aynıdır — önce hedef uygunluğu, sonra menzil, sonra hasar —
        /// çünkü SIRA kararı hedefin ne olduğuna bağlı değil.
        ///
        /// İki yerde ayrışır ve ikisi de hedefin doğasından gelir:
        /// <list type="number">
        /// <item>uygunluğu <see cref="StructureState"/> söyler, <see cref="UnitState"/>
        /// değil — bir baraka düşmez, yıkılır;</item>
        /// <item>ölüm olayının adı <see cref="AttackOutcome.HitAndDestroyed"/>'dır.</item>
        /// </list>
        /// </summary>
        // REDDEDILEN - AttackAction.cs:202 yerine (iki aşırı yükleme tek gövdede
        //              birleşir; hedefler ortak bir arayüz arkasına alınır):
        //     public interface IAttackTarget
        //     {
        //         bool CanBeAttackedBy(Team attackerTeam);
        //         bool TakeDamage(int amount);
        //     }
        //     public static AttackOutcome Execute(Combatant attacker, IAttackTarget target, int distance)
        // KIRILAN  : hedef uygunluğu kuralı TargetingRules'tan HEDEFİN İÇİNE taşınır.
        //            Combatant ve Structure ikisi de TargetingRules'ı tanır
        //            durum matrisi tek enum ile sınanamaz -> testler nesne kurar
        //            arayüzün bool'u -> Downed ile Destroyed aynı cevabın arkasına düşer
        //            derleyici: hiçbir şey der  .  test: TargetingRulesTests satır sayısı artar
        // KAZANIRDI: hedeflenebilir tip sayısı üçü geçtiği gün — birim, yapı, araç,
        //            tuzak, kapı — aynı akışı beş kez kopyalamak gerçek bir tekrar olurdu.
        // TEK CUMLE: Soyutlama tekrarı sildiği için değil, KURAL sahipliğini bozmadan
        //            sildiği için kazanır; bugün sildiği tek şey iki metot.
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

            // SALDIRANIN DURUMU BURADA DA SORULUYOR — ve bu tekrar değil,
            // kuralın İKİ akışta da uygulanmasıdır. Kuralın METNİ tek yerde
            // (AttackRules); burada yalnızca soruluyor. Bu satır olmasaydı
            // düşmüş bir birim askere vuramaz ama barakayı yıkabilirdi ve o
            // fark hiçbir yerde yazılı olmazdı — yalnızca hangi aşırı yüklemeye
            // düştüğüne bağlı olurdu.
            //
            // Saldıranın tipi İKİ akışta da Combatant, yani soru DEĞİŞMİYOR;
            // değişen tek şey hedefin dili (StructureState ↔ UnitState).
            // AttackRules'ın yapı ikizi bu yüzden YOK: yapı saldırmaz, saldıran
            // hep bir Combatant'tır. Aynı gerekçe TargetingRules'ta diriltmenin
            // yapı ikizi için de yazılı — birebir eşleşmeyen satır bir eksiklik
            // değil, bir karardır.
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

            // BURADA "ÖNCEKİ DURUMU OKU" DESENİ YOK — ve bu bir eksiklik değil,
            // Structure'ın sözleşmesinin farkı. Structure.TakeDamage zaten "yapı BU
            // vuruşla yıkıldı mı" cevabını DÖNDÜRÜYOR; birim tarafında Combatant.
            // TakeDamage void olduğu için değişimi çağıranın kendisi ölçmek
            // zorundaydı. Aynı deseni burada tekrarlamak, cevabı zaten veren bir
            // metodun cevabını görmezden gelmek olurdu.
            //
            // REDDEDILEN - AttackAction.cs:269 yerine (birim sürümündeki desen
            //              körü körüne kopyalanır):
            //     StructureState stateBeforeHit = target.State;
            //     target.TakeDamage(attacker.AttackProfile.Damage);
            //     return stateBeforeHit == StructureState.Standing
            //         && target.State == StructureState.Destroyed
            //         ? AttackOutcome.HitAndDestroyed
            //         : AttackOutcome.Hit;
            // KIRILAN  : "bu vuruş mu yıktı" kararı, cevabı zaten veren metodun dışında
            //            İKİNCİ kez verilir.
            //            bugün aynı cevabı verir -> hiçbir test ayırt edemez
            //            yıkım koşulu değişir — can sıfıra inmeden çöken bina — burası eskir
            //            derleyici: hiçbir şey der  .  test: yeşil kalır, sahiplik kayar
            // KAZANIRDI: Structure.TakeDamage void olsaydı ya da bool'u "hasar uygulandı"
            //            gibi başka bir soruyu cevaplasaydı.
            // TEK CUMLE: Simetri bir gerekçe değildir; cevabı zaten veren bir metodun
            //            cevabını görmezden gelmek, kararı sahibinden geri almaktır.
            return target.TakeDamage(attacker.AttackProfile.Damage)
                ? AttackOutcome.HitAndDestroyed
                : AttackOutcome.Hit;
        }
    }
}
