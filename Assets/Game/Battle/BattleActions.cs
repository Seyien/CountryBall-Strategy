using System;
using GridStrategy.Combat;
using GridStrategy.Core;

namespace GridStrategy.Battle
{
    // ═══ ROL: KURAL (Policy) ═════════════════════════════════════════
    // kimlik : yok — static
    // hafıza : yok — aynı savaş ve aynı birimler aynı sonucu verir; tuttuğu
    //          hiçbir şey yok, DEĞİŞTİRDİĞİ şey Battle'ın tahtası ve savaşçıları
    // Unity  : gerekmez — noEngineReferences: true
    // karar  : AKIŞI yürütür; kuralların hiçbirini kendisi yazmaz
    /// <summary>
    /// Tahta ile savaşı birleştiren akışın tek sahibi.
    ///
    /// Bu dosyanın var olma sebebi tek satır: <see cref="AttackAction"/> mesafeyi
    /// DIŞARIDAN alıyordu ve o mesafeyi üretecek kimse yoktu. Üreteni
    /// <see cref="GridDistance"/>, kimin nerede durduğunu <see cref="Battle"/>,
    /// saldırının çözümünü <see cref="AttackAction"/> biliyor — üçü de birbirini
    /// TANIMAZ. Onları bir sıraya dizen tek yer burasıdır.
    ///
    /// DÖRT EYLEM, TEK ŞEKİL: saldırı, hareket, diriltme, yerleştirme. Dördü de
    /// aynı iskeleti izler — önce ÇAĞIRAN HATALARI (bu savaşta mı, tahtada mı,
    /// sayı geçerli mi), sonra KURALLAR, en sonda tek bir yazma. İskeletin sırası
    /// kasıtlı: bir çağıran hatası hiçbir zaman bir oyun sonucu kılığına
    /// girmemeli.
    ///
    /// SIRA KURALINI ARTIK SORUYOR. <see cref="TurnState"/> ile
    /// <see cref="TurnRules"/> yazılmıştı ama üretimde tek bir çağıranları yoktu;
    /// sıra sistemi vardı ve hiçbir şeyi ENGELLEMİYORDU. Soruyu soran taraf
    /// burasıdır çünkü sıranın sahibi <see cref="Battle"/> ve o bilgiyi ilk
    /// gören akış budur. Kural aşağı İNMEDİ ve inemez: "aktif takım" diye bir
    /// kavram <c>GridStrategy.Combat</c>'ta yoktur — kuralı uygulayabilen en alt
    /// katman bu.
    ///
    /// Neyi BİLMEZ: mesafenin Chebyshev mi Manhattan mı olduğunu, hasarın nasıl
    /// hesaplandığını, hedefin neden uygun olduğunu, sıranın nasıl devredildiğini,
    /// sonucu kimin göstereceğini. Buradaki her <c>if</c> bir kuralı SORAR;
    /// hiçbiri bir kural YAZMAZ.
    /// </summary>
    public static class BattleActions
    {
        /// <summary>
        /// Bir saldırı denemesini yürütür: konumları <see cref="Battle"/>'dan
        /// bulur, mesafeyi <see cref="GridDistance"/>'a ölçtürür ve saldırıyı
        /// <see cref="AttackAction"/>'a çözdürür.
        ///
        /// Hedef bir BİRİM de olabilir bir YAPI da; hangisi olduğunu bu metot
        /// <see cref="Battle.TryGetStructure"/>'a SORAR.
        /// </summary>
        public static AttackOutcome Attack(Battle battle, Unit attacker, Unit target)
        {
            if (battle == null)
            {
                throw new ArgumentNullException(nameof(battle));
            }

            if (attacker == null)
            {
                throw new ArgumentNullException(nameof(attacker));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            // ÖNCE bütün "bu savaşta mı" soruları, SONRA kurallar. Sıra kasıtlı:
            // bir çağıran hatası hiçbir zaman bir oyun sonucu kılığına
            // girmemeli, dolayısıyla tek bir AttackOutcome dönmeden önce
            // hepsinin cevaplanmış olması gerekir.
            Combatant attackerCombatant = RequireCombatant(battle, attacker, nameof(attacker));

            // HEDEFİN NE OLDUĞU SORULUYOR, TAŞINMIYOR. Cevabı zaten bilen tek
            // yer Battle; çağıranın elinde o bilgi yok ve olması da gerekmiyor.
            //
            // REDDEDILEN - BattleActions.cs:98 yerine (hedefin tipi çağırandan
            //              bir bayrakla gelir):
            //     public static AttackOutcome Attack(Battle battle, Unit attacker,
            //                                        Unit target, bool targetIsStructure)
            // KIRILAN  : çağıran, Battle'ın ZATEN bildiği bir şeyi taşır.
            //            BoardAdapter tipi kendi görsel tablosundan çıkarır ->
            //            bir yerde `false` geçer -> akış barakayı
            //            RequireCombatant'a düşürüp ilgisiz bir istisna atar
            //            derleyici: yanlış bayrağı GÖRMEZ  .  test: yeşil kalır
            // KAZANIRDI: hedef tipi çağıranın KENDİ kararı olsaydı — aynı hücrede
            //            hem bir birim hem bir yapı durabilseydi (köprü, tuzak) ve
            //            "hangisine vuruyorsun" gerçek bir seçim olsaydı.
            // TEK CUMLE: Cevabı zaten bilen tarafa sorulmayan her soru, çağıranda
            //            yanlış doldurulabilen bir parametreye dönüşür.
            bool targetIsStructure = battle.TryGetStructure(target, out Structure targetStructure);

            // Birim tarafı yalnızca hedef bir yapı DEĞİLSE aranıyor: yapıların
            // Combatant'ı yok ve olmamalı (Structure, Combatant'tan türemiyor —
            // gerekçe Structure.cs'in başında).
            Combatant targetCombatant = targetIsStructure
                ? null
                : RequireCombatant(battle, target, nameof(target));

            RequireCell(battle, attacker, nameof(attacker), out int attackerX, out int attackerY);
            RequireCell(battle, target, nameof(target), out int targetX, out int targetY);

            // SIRA KURALI HER ŞEYDEN ÖNCE SORULUYOR — hedefin uygunluğundan da,
            // menzilden de önce. Ölçüt S-06'nın sıra dersinin ölçütü: cevabı
            // düzeltmenin çağırana BİR ŞEY KAZANDIRMADIĞI sebep önce söylenir.
            // Sıra sende değilken başka bir hedef seçmek de, yaklaşmak da
            // hiçbir şeyi değiştirmez.
            //
            // Takım bilgisi SAVAŞÇIDAN geliyor, birimden değil: Unit tarafı
            // bilmez (gerekçe Combatant.Team'in üstünde yazılı) ve bu akış onu
            // ikinci bir yerden okumaya kalksaydı taraf iki sahipli olurdu.
            //
            // REDDEDILEN - BattleActions.cs:137 yerine (sıra sorusu hedefin
            //              uygunluğundan SONRA sorulur, yani AttackAction'a
            //              devredildikten sonra):
            //     AttackOutcome outcome = AttackAction.Execute(
            //         attackerCombatant, targetCombatant, distance);
            //     if (outcome != AttackOutcome.Hit && outcome != AttackOutcome.HitAndDowned)
            //     {
            //         return outcome;
            //     }
            //     if (!TurnRules.CanAct(attackerCombatant.Team, battle.Turn.Current))
            //     {
            //         return AttackOutcome.RejectedActorCannotAct;
            //     }
            // KIRILAN  : hasar ZATEN İNMİŞ olur; ret yalnızca SONUÇ metninde kalır.
            //            "vurdu" cevabı hedefin canının çoktan azaldığı demektir
            //            -> sırası gelmemiş oyuncunun vuruşu tahtada gerçekleşir
            //            derleyici: hiçbir şey der  .  test: kırmızı —
            //            Attack_WhenItIsNotTheAttackersTurn_IsRejectedAndDealsNoDamage
            // KAZANIRDI: sıra kuralı bir SİMÜLASYON kısıtı değil bir ARAYÜZ
            //            kısıtı olsaydı — motor her hamleyi çözüp gösterseydi ve
            //            sırayı yalnızca ekran engelleseydi ("ne olurdu"
            //            göstergesi, tekrar kaydı önizlemesi).
            // TEK CUMLE: Geri alınamaz bir işlemden SONRA sorulan kural bir kural
            //            değil, bir açıklamadır.
            if (!TurnRules.CanAct(attackerCombatant.Team, battle.Turn.Current))
            {
                return AttackOutcome.RejectedActorCannotAct;
            }

            // TAKIM ÖN KONTROLÜ BURADAN KALKTI — ve bu, bir borcun kapanışıdır.
            // Eskiden bu satırların yerinde ikinci bir TargetingRules.CanBeAttacked
            // çağrısı vardı; o çağrı, AttackAction takımı sormadığı dönemde dost
            // ateşini engelleyen TEK yerdi. AttackAction artık kuralı kendisi
            // soruyor (S-12: kural uygulayabilen en alt katmana iner), dolayısıyla
            // buradaki soru bir koruma değil yalnızca bir TEKRAR.
            //
            // Kaldırmanın güvenli olduğunun kanıtı testte yazılıydı ve önceden
            // yazılmıştı:
            // Attack_SameTeamTarget_IsRejectedByTheFlowAndByAttackActionAlike
            // — o testin ikinci yarısı akışı ATLAYIP doğrudan AttackAction'a soruyor
            // ve aynı cevabı alıyor. Ön kontrol kalktı, o test kırmızıya dönmedi —
            // tam olarak öngörüldüğü gibi.
            //
            // REDDEDILEN - BattleActions.cs:180 yerine (tekrar korunur, "iki kere
            //              sormak zarar vermez" diye):
            //     if (!TargetingRules.CanBeAttacked(
            //             targetCombatant.State, attackerCombatant.Team, targetCombatant.Team))
            //     {
            //         return AttackOutcome.RejectedInvalidTarget;
            //     }
            // KIRILAN  : kopya, hedef tipi başına ÇOĞALIR.
            //            yapı hedefinde State bir StructureState'tir -> aynı ön
            //            kontrol için ikinci bir dal gerekir -> kural değişince
            //            (dost ateşi kipi) iki yerden biri güncellenmeyi unutur
            //            derleyici: hiçbir şey der  .  test: hepsi yeşil kalır
            // KAZANIRDI: ön kontrol PAHALI bir işi engelleseydi — mesafe hesabı
            //            bir yol bulma çağrısı olsaydı ya da AttackAction bir ağ
            //            isteği yapsaydı; o gün ucuz eleme önde durur.
            // TEK CUMLE: Aynı kuralı iki kere sormak bugün bedava, yarın iki
            //            farklı cevaptır.

            // Mesafe BURADA hesaplanmıyor, HESAPLATILIYOR. Chebyshev kararı
            // GridDistance.Between'in içindeki REDDEDILEN bloğunda yaşıyor ve
            // buraya kopyalanmış bir Math.Max(Math.Abs(...), ...) o kararı
            // ikinci bir yere yazardı.
            // Attack_DiagonalNeighbourWithRangeOne_Hits testi tam olarak bu
            // sahipliği koruyor: çapraz komşu Chebyshev'de 1, Manhattan'da 2.
            int distance = GridDistance.Between(attackerX, attackerY, targetX, targetY);

            // İKİ AŞIRI YÜKLEME, TEK AKIŞ. Dallanma burada bitiyor çünkü ayrılan
            // tek şey hedefin TİPİ; sıra, mesafe ve saldıran iki dalda da aynı.
            AttackOutcome outcome = targetIsStructure
                ? AttackAction.Execute(attackerCombatant, targetStructure, distance)
                : AttackAction.Execute(attackerCombatant, targetCombatant, distance);

            // SIRA BURADA DEVREDİLİR — ve bu satır olmadan oyun KIRIKTI.
            // TurnRules.CanAct üç eylemde de soruluyordu ama TurnState.EndTurn
            // üretimde HİÇ çağrılmıyordu: Player sonsuza kadar oynardı, Enemy
            // hiç sıra almazdı, ve hiçbir test kırmızı olmazdı çünkü her test
            // tek bir eylemi sınıyordu. Bir kuralı SORMAK ile onu İŞLETMEK
            // farklı işlerdir; ilki yazılmıştı, ikincisi yazılmamıştı.
            //
            // Neden burada, BoardAdapter'da değil: S-12. Sırayı ekranda
            // devretseydik, akışın ikinci bir çağıranı doğduğu gün (yapay zekâ,
            // tekrar kaydı, bir test) sırayı devretmeyi unuturdu ve kural
            // yalnızca fare ile oynandığında geçerli olurdu.
            //
            // BEYAZ LİSTE, kara liste değil — MovementRules.CanMove ile aynı
            // gerekçe: yarın eklenen bir ret değeri (ör. RejectedNoAmmo) kara
            // listede varsayılan olarak "eylem sayılır" ve sırayı SESSİZCE
            // yakar. Beyaz listede aynı değer varsayılan olarak sırayı harcamaz
            // ve hata en fazla "sıram bitmedi" olur — geri alınabilir yön.
            //
            // REDDEDILEN - BattleActions.cs:221 yerine (reddedilen deneme de
            //              sırayı yakar, "düşünüp doğru hamleyi bul" diye):
            //     battle.Turn.EndTurn();
            //     return outcome;
            // KIRILAN  : kırılan şey oynanabilirlik.
            //            menzil dışı bir hücreye tek bir yanlış tıklama olur ->
            //            tur biter -> RejectedActorCannotAct dönen çağrı bile
            //            sırayı yakar, yani "sıran değil" cevabı sıranı bitirir
            //            derleyici: hiçbir şey der  .  test: yeşil kalır
            // KAZANIRDI: eylem bir KAYNAK harcasaydı (mermi, mana, hareket
            //            puanı) ve deneme o kaynağı zaten tüketiyorsa — o gün
            //            ret de bir maliyettir ve sırayı yakması tutarlıdır.
            // TEK CUMLE: Beyaz liste yanlışı "sıram bitmedi"ye, kara liste
            //            yanlışı "turumu kaybettim"e düşürür — biri geri
            //            alınabilir, diğeri değil.
            bool attacked = outcome == AttackOutcome.Hit
                || outcome == AttackOutcome.HitAndDowned
                || outcome == AttackOutcome.HitAndDestroyed;

            if (attacked)
            {
                battle.Turn.EndTurn();
            }

            return outcome;
        }

        /// <summary>
        /// Bir hareket denemesini yürütür: birimin bulunduğu hücreyi
        /// <see cref="Battle"/>'dan bulur, sıra ve durum kurallarını SORAR, geri
        /// kalan kararı <see cref="MoveAction"/>'a bırakır.
        /// </summary>
        /// <param name="moveRange">
        /// Bu birimin bu turda kaç hücre uzağa gidebildiği. Sayının nereden
        /// geldiği hâlâ çağıranın sorunu — gerekçesi MoveAction'ın int menzil
        /// alan Execute sürümünün üstündeki "HAREKET MENZİLİ NEREDEN GELİR"
        /// bloğunda.
        /// </param>
        public static MoveOutcome Move(Battle battle, Unit unit, int toX, int toY, int moveRange)
        {
            if (battle == null)
            {
                throw new ArgumentNullException(nameof(battle));
            }

            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

            // PROFİL BURADA KURULUYOR — VE KURULLARDAN ÖNCE. İki ayrı sebep:
            //
            // (1) Sayıyı tipine çevirmenin yeri burasıdır. MoveProfile Core'da
            //     doğdu ve MoveAction'ın profil alan aşırı yüklemesi onu bekliyor;
            //     imzası dondurulmuş olan bu metot ise hâlâ çıplak bir int
            //     alıyor. Çeviriyi yapacak yer, ikisini de tanıyan akıştır —
            //     MoveAction'ın profil alan Execute sürümünün üstündeki EŞİK
            //     notu bu çağıranı adıyla anıyor.
            // (2) Negatif menzil bir ÇAĞIRAN HATASIdır ve bu dosyanın iskeleti
            //     çağıran hatalarını kurallardan önce sorar. Kurulum aşağı
            //     alınsaydı, sırası gelmemiş bir birim için negatif menzil sessizce
            //     RejectedActorCannotAct'e dönerdi ve bozuk sayı hiç görülmezdi.
            //
            // Doğrulamanın METNİ burada değil: eşik ("0 geçerli, negatif değil")
            // MoveProfile'ın kendi kurucusunda yaşıyor. Bunun görünür bir bedeli
            // var ve yazılıyor — fırlayan ArgumentOutOfRangeException'ın
            // ParamName'i artık "moveRange" değil "range".
            //
            // Kare başına tahsis DEĞİL: bu metot bir oyuncu ya da yapay zekâ
            // hamlesi başına bir kez çağrılıyor, her karede değil. Ölçüt
            // DamageRulesAllocationTests'in koruduğu çizgi — sıcak yolda çöp yok —
            // ve bu yol sıcak değil. EŞİK: bir yol bulucu aynı kareyi yüzlerce
            // kez denemeye başladığı gün profil çağırandan gelmeli, çünkü o gün
            // yol GERÇEKTEN sıcak olur.
            var profile = new MoveProfile(moveRange);

            Combatant combatant = RequireCombatant(battle, unit, nameof(unit));
            RequireCell(battle, unit, nameof(unit), out int fromX, out int fromY);

            // İKİ KURAL, TEK RET DEĞERİ — ve sıraları GÖZLENEMEZ. İkisi de aynı
            // MoveOutcome.RejectedActorCannotAct'i döndürdüğü için hangisinin
            // önce sorulduğunu hiçbir test ayırt edemez. Bu dürüst bir sınır ve
            // öyle yazılıyor: burada korunacak bir sıra kararı YOK, çünkü sıra
            // kararı ancak farklı cevaplar arasında var olur. Sıra kuralının
            // önde durması bir tercih — savaş dışı kısıt, savaş içi kısıttan
            // önce — ve doğruluğu değil yalnızca okunuşu etkiliyor.
            //
            // Değer İKİYE ayrıldığı gün (eşiği MoveOutcome.cs'te yazılı) bu sıra
            // ölçülebilir hâle gelir ve o gün bir karar olur; bugün değil.
            if (!TurnRules.CanAct(combatant.Team, battle.Turn.Current))
            {
                return MoveOutcome.RejectedActorCannotAct;
            }

            // DÜŞMÜŞ BİRİM ARTIK YÜRÜYEMEZ. Bu satırın yerinde bir borç notu
            // duruyordu: "kuralın SAHİBİ YOK". Sahip doğdu — MovementRules —
            // ve akış onu SORUYOR, kuralı kendisi yazmıyor. Borcu sabitleyen
            // test (Move_DownedUnit_StillMoves_BecauseNoRuleOwnsMovementState (SILINDI))
            // silindi; yerine kararı koruyan Move_DownedUnit_IsRejected geldi.
            //
            // Kural neden burada soruluyor da MoveAction'ın içinde değil:
            // MovementRules ve UnitState GridStrategy.Combat'ta, MoveAction ise
            // GridStrategy.Core'da ve Core, Combat'ı GÖRMEZ. S-12'nin "kuralı
            // uygulayabilen en alt katman" ölçütü tam olarak burada duruyor —
            // alt katman kuralı SORAMAZ, çünkü sormak için gereken tipi
            // göremiyor. Gerekçenin tamamı MoveAction.cs'in profil aşırı
            // yüklemesinin üstündeki REDDEDILEN bloğunda yazılı.
            if (!MovementRules.CanMove(combatant.State))
            {
                return MoveOutcome.RejectedActorCannotAct;
            }

            // Alternatif: MoveAction'ın int alan aşırı yüklemesini çağırmayı
            // sürdürmek. Seçilmedi: o sürümün kalkma eşiği MoveAction.cs'te
            // yazılı ve son int çağıranı burasıydı; çağrı değişmezse eşik hiç
            // tetiklenmez ve MoveProfile üretimde çağıranı olmayan bir tip kalır.
            MoveOutcome outcome =
                MoveAction.Execute(battle.Board, unit, fromX, fromY, toX, toY, profile);

            // Sıra devri — gerekçesi Attack'te tek kez yazılı, burada TEKRAR
            // EDİLMİYOR. Beyaz liste burada tek değere iniyor çünkü MoveOutcome'un
            // tek başarı değeri var; kara liste yazılsaydı üç ret değerinin üçünü
            // de saymak gerekirdi ve dördüncüsü eklendiği gün sessizce sıra
            // yakardı.
            if (outcome == MoveOutcome.Moved)
            {
                battle.Turn.EndTurn();
            }

            return outcome;
        }

        /// <summary>
        /// Bir diriltme denemesini yürütür: <see cref="Combatant.TryRevive"/> ve
        /// <c>TargetingRules.CanBeRevived</c> yazılmıştı ve üretimde tek bir
        /// çağıranları yoktu — bu metot onları bir EYLEME bağlıyor.
        ///
        /// Sıra: sıra kuralı → hedefin uygunluğu (durum VE taraf) → menzil →
        /// diriltme. İskelet saldırınınkiyle aynı ve olması gereken de bu: iki
        /// eylem de "bir hedefe bir şey uygula" ailesinden.
        /// </summary>
        public static ReviveOutcome Revive(Battle battle, Unit reviver, Unit target)
        {
            if (battle == null)
            {
                throw new ArgumentNullException(nameof(battle));
            }

            if (reviver == null)
            {
                throw new ArgumentNullException(nameof(reviver));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            Combatant reviverCombatant = RequireCombatant(battle, reviver, nameof(reviver));
            Combatant targetCombatant = RequireCombatant(battle, target, nameof(target));

            RequireCell(battle, reviver, nameof(reviver), out int reviverX, out int reviverY);
            RequireCell(battle, target, nameof(target), out int targetX, out int targetY);

            if (!TurnRules.CanAct(reviverCombatant.Team, battle.Turn.Current))
            {
                return ReviveOutcome.RejectedActorCannotAct;
            }

            // DİRİLTENİN KENDİ DURUMU ARTIK SORULUYOR — ve bu, adı konmuş bir
            // borcun kapanışıdır. Bu satırların yerinde bir EŞİK notu duruyordu:
            // "ReviveRules doğduğu gün bu satırların yerine tek bir soru gelir
            // ve sabitleyen test kırmızıya döner." Tip doğdu, soru geldi, test
            // silindi; yerine kararı koruyan Revive_DownedReviver_IsRejected
            // gelmiş durumda.
            //
            // Aşağıdaki iki REDDEDILEN bloğu KALDIRILMADI. Onlar "neden ayrı bir
            // tip" sorusunun cevabı ve o soru kural yazıldıktan sonra da geçerli;
            // silinselerdi geriye yalnızca sonuç kalırdı, kararın kendisi değil.
            //
            // REDDEDILEN - BattleActions.cs:426 yerine (saldırı kuralı ödünç
            //              alınır):
            //     if (!AttackRules.CanAttack(reviverCombatant.State))
            //     {
            //         return ReviveOutcome.RejectedActorCannotAct;
            //     }
            // KIRILAN  : ad yalan söyler ve yalanı derleyici göremez — türetmenin
            //            reddi MovementRules ile AttackRules'ta zaten yazılı.
            //            yaralı sıhhiyeci kuralı doğar -> AttackRules değişir ->
            //            diriltme de onunla birlikte değişir
            //            derleyici: hiçbir şey der  .  test: hiçbiri kırılmaz
            // KAZANIRDI: tasarım "eyleyebilmek" diye TEK bir kavrama inseydi —
            //            o gün üç kural bir kurala iner ve adı hiçbirinin adı
            //            olmazdı.
            // TEK CUMLE: Bir kuralı başka bir kuraldan türetmek iki kararı tek
            //            satıra bağlar; ayrılmaları gereken gün sessiz geçer.
            //
            // REDDEDILEN - BattleActions.cs:426 yerine (kural burada YAZILIR):
            //     if (reviverCombatant.State != UnitState.Alive)
            //     {
            //         return ReviveOutcome.RejectedActorCannotAct;
            //     }
            // KIRILAN  : akış bir kural YAZMIŞ olur — rol başlığındaki tek sözün
            //            ihlali, ve aynı ihlal bu dosyada hareket için de reddedildi.
            //            kuralı çağırmanın tek yolu bu metot olur -> sınamak için
            //            bir Battle kurup içine düşmüş savaşçı koymak gerekir
            //            derleyici: hiçbir şey der  .  test: kuralı değil akışı sınar
            // KAZANIRDI: kuralın TEK çağıranı bu olsaydı ve başka hiçbir anlamı
            //            olmasaydı — o gün ayrı bir tip yalnızca bir dolaylılık
            //            katmanı olurdu.
            // TEK CUMLE: Akışa yazılan kural, savaş kurmadan sorulamaz hâle gelir;
            //            ayrı bir tip onu tek başına sınanabilir tutar.
            //
            // Ailenin üçüncü ve son üyesi: MovementRules "kim yürür", AttackRules
            // "kim vurur", ReviveRules "kim kaldırır". Üçü de bugün aynı satırı
            // taşıyor (state == Alive) ve bu bir TESADÜF — üçünü birden kayda
            // geçiren test ReviveRulesTests'teki
            // ThreeActorRules_StillAgree_WhichIsWhyTheyMustStaySeparate. Ayrıldıkları
            // gün (yaralı sıhhiyeci vuramaz ama kaldırabilir) burada hiçbir şey
            // değişmez; türetme yazan proje o günü hiçbir test kırılmadan kaçırır.
            if (!ReviveRules.CanRevive(reviverCombatant.State))
            {
                return ReviveOutcome.RejectedActorCannotAct;
            }

            if (!TargetingRules.CanBeRevived(
                    targetCombatant.State, reviverCombatant.Team, targetCombatant.Team))
            {
                return ReviveOutcome.RejectedInvalidTarget;
            }

            int distance = GridDistance.Between(reviverX, reviverY, targetX, targetY);

            // DİRİLTME MENZİLİ BUGÜN SALDIRI PROFİLİNDEN GELİYOR — ve bu bir
            // taviz, öyle de yazılıyor. Menzil kavramının bu projedeki tek sahibi
            // AttackProfile; "erişim" ölçüsü orada yaşıyor ve ikinci bir sahip
            // yok. Yani bir okçu üç hücre öteden diriltir. Oyun açısından
            // tartışmalı, tasarım açısından dürüst: uydurulmuş bir sayıdan iyidir.
            //
            // REDDEDILEN - BattleActions.cs:466 yerine (menzil burada yazılır):
            //     const int ReviveRange = 1;
            //     if (distance > ReviveRange)
            //     {
            //         return ReviveOutcome.RejectedOutOfRange;
            //     }
            // KIRILAN  : akış bir DENGE SAYISI sahiplenir.
            //            sayı akış dosyasına gömülür -> tasarımcı onu değiştirmek
            //            için kural dosyasını değil akışı düzenler -> sayı iki
            //            birim için de aynı olur, "menzilli sıhhiyeci" yazılamaz
            //            derleyici: hiçbir şey der  .  test: yeşil kalır
            // KAZANIRDI: diriltme menzili bir DENGE değil bir KURAL olsaydı —
            //            "diriltmek her zaman bitişik hücreden yapılır" evrensel
            //            bir oyun kuralıysa; o gün sayı yine buraya değil o
            //            kuralın sahibine yazılır.
            // TEK CUMLE: Bu dosyada başka hiçbir sayı yok ve olmaması bir karar —
            //            menzil AttackProfile'da, tur bütçesi TurnRules'ta yaşar.
            //
            // EŞİK: bir birimin DESTEK erişimi ile HASAR erişimi ayrıldığı gün
            // (menzilli sıhhiyeci, bitişik-diriltme kuralı) buraya bir
            // ReviveProfile ya da SupportProfile gelir ve bu satır onu sorar.
            if (!AttackResolver.IsWithinRange(distance, reviverCombatant.AttackProfile))
            {
                return ReviveOutcome.RejectedOutOfRange;
            }

            // TryRevive'ın dönüşü YOK SAYILMIYOR. Buraya ulaşıldığında hedefin
            // Downed olduğu bir satır önce doğrulandı, yani false dönmesi
            // Combatant.State ile UnitLifecycle.State'in ayrıştığı anlamına
            // gelir — bugün imkânsız, çünkü biri diğerine devrediyor. Dönüşü
            // sessizce atmak, o imkânsızlığı bir VARSAYIMA çevirirdi; okumak
            // onu bir DEĞİŞMEZ olarak tutuyor ve ayrışma bir gün doğarsa çağıran
            // "dirildi" yalanını almaz.
            if (!targetCombatant.TryRevive())
            {
                return ReviveOutcome.RejectedInvalidTarget;
            }

            // Sıra devri. Burada beyaz listeye gerek yok: bu satıra ulaşmanın tek
            // yolu bütün ret dallarını geçmektir, yani "başarılı" durumu akışın
            // KONUMU söylüyor, bir değer karşılaştırması değil. Attack ve Move'da
            // karşılaştırma gerekiyor çünkü orada sonuç bir alt katmandan geliyor
            // ve bu metot onu görmeden döndürüyor.
            battle.Turn.EndTurn();

            return ReviveOutcome.Revived;
        }

        /// <summary>
        /// Bir yapıyı tahtaya koyar ve savaşa katar.
        ///
        /// Bu metot <see cref="Battle.AddStructure"/>'ın önündeki KURAL kapısıdır:
        /// oradaki katılım bir çağıran sözleşmesidir (dolu hücre bir istisnadır),
        /// buradaki yerleştirme ise bir OYUN eylemidir ve reddi bir sonuç
        /// döndürür. İkisinin farkı PlacementOutcome.cs'te yazılı.
        /// </summary>
        /// <param name="unit">
        /// Yapının tahtadaki kimliği. Bir baraka da bir <see cref="Unit"/>'tir —
        /// "tahtada yer kaplayan, kimliği olan şey" — ve ikinci bir tahta
        /// açmamanın bedeli tam olarak budur.
        /// </param>
        public static PlacementOutcome PlaceStructure(
            Battle battle, Unit unit, Structure structure, int x, int y)
        {
            if (battle == null)
            {
                throw new ArgumentNullException(nameof(battle));
            }

            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

            if (structure == null)
            {
                throw new ArgumentNullException(nameof(structure));
            }

            // SIRA BİR KARARDIR: önce tahta sınırı, sonra doluluk. Aynı sıra
            // MoveAction'da da var ve gerekçesi devralınıyor: tahta dışı bir
            // hücre BEKLEYEREK de geçerli olmaz, dolu bir hücre ise bir tur
            // sonra boşalabilir. Düzeltilemeyen sebep önce söylenir.
            if (!battle.IsInsideGrid(x, y))
            {
                return PlacementOutcome.RejectedInvalidCell;
            }

            // Doluluk sorusu TAHTAYA soruluyor, ikinci bir deftere değil:
            // barakalar da tahtada yer kapladığı için bu tek soru hem birimleri
            // hem yapıları kapsıyor. İkinci bir tahta açsaydık burada İKİ soru
            // olurdu ve biri unutulduğu gün aynı hücrede iki şey dururdu —
            // hiçbir derleme hatası çıkmadan.
            if (battle.TryGetUnit(x, y, out Unit _))
            {
                return PlacementOutcome.RejectedCellOccupied;
            }

            // "BU BİRİM ZATEN SAVAŞTA" BURADA BİR RET DEĞİL, BİR ÇAĞIRAN
            // HATASI — ve kontrolü kopyalanmıyor, AddStructure'a bırakılıyor.
            // Aynı kimliği ikinci kez yerleştirmek bir oyuncu hamlesi değil,
            // çağıranın kaydının Battle'ınkiyle ayrışmasıdır; RequireCombatant'ın
            // üstündeki gerekçe bu satır için de geçerli.
            //
            // Yıkık bir yapının yerleştirilmesi için ret değeri YOK: bugün
            // tahtaya konmadan yıkılmış bir Structure üretebilen tek yol testtir.
            // Kayıt dosyasından hazır yapı yüklendiği gün soru gerçek olur ve o
            // gün sahibi bu akış değil, yükleyici olur.
            battle.AddStructure(unit, structure, x, y);

            return PlacementOutcome.Placed;
        }

        // "BU SAVAŞTA DEĞİL" BİR OYUN SONUCU DEĞİL, BİR ÇAĞIRAN HATASIDIR.
        // Battle konumun ve eşleşmenin tek sahibi; tanımadığı bir birimle
        // çağrılması "benim kaydım Battle'ınkiyle ayrışmış" demektir. Aynı
        // felsefe MoveAction'ın kaynak hücre kontrolündeki REDDEDILEN bloğunda
        // zaten uygulanmış durumda.
        //
        // REDDEDILEN - BattleActions.cs:580 yerine (yeni bir BattleOutcome tipi
        //              doğar ve bu dosya onu döndürür):
        //     public enum BattleOutcome { RejectedUnknownUnit, Attacked, Moved }
        // KIRILAN  : bir programcı hatası oyun sonucu kılığına girer.
        //            hata artık patlamaz -> sessizce yutulabilen bir dala döner
        //            -> çağıran hem BattleOutcome hem AttackOutcome üstünde
        //            switch yazar ve Hit / HitAndDowned ayrımı sarmalayıcıya gömülür
        //            derleyici: hiçbir şey der  .  test: Attack_UnknownAttacker_Throws kırmızı
        // KAZANIRDI: emirler AĞDAN ya da bir tekrar kaydından gelseydi — orada
        //            "o birim artık yok" beklenen bir ayrışmadır ve çökmek
        //            yerine reddedip senkronizasyon istemek doğru davranıştır;
        //            MoveAction'ın kaynak hücre bloğundaki KAZANIRDI satırı
        //            aynı kapıyı açık bırakıyor.
        // TEK CUMLE: Sonuç tipi OYUNCUNUN yapabildiklerini adlandırır;
        //            programcının yapamayacağı şey istisnadır, çünkü onu ele
        //            almanın doğru bir yolu yoktur.
        private static Combatant RequireCombatant(Battle battle, Unit unit, string paramName)
        {
            if (!battle.TryGetCombatant(unit, out Combatant combatant))
            {
                throw new ArgumentException("The unit is not in this battle.", paramName);
            }

            return combatant;
        }

        // Tahtaya yalnız Battle.AddUnit ve Battle.AddStructure yazar; dolayısıyla
        // "tahtada durmuyor" ile "bu savaşta değil" aynı cümledir ve iki ayrı
        // mesaj yazmak o değişmezi ikinci bir yerde anlatmak olurdu.
        private static void RequireCell(
            Battle battle, Unit unit, string paramName, out int x, out int y)
        {
            if (!battle.TryGetPosition(unit, out x, out y))
            {
                throw new ArgumentException("The unit is not in this battle.", paramName);
            }
        }
    }
}
