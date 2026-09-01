using System;
using System.Collections.Generic;
using GridStrategy.Combat;
using GridStrategy.Core;

namespace GridStrategy.Battle
{
    // ═══ ROL: AKIŞ SAHİBİ (transaction script) ═══════════════════════
    // kimlik : yok — static
    // hafıza : yok — ama ölçüsü "aynı çağrı aynı cevabı verir" DEĞİL, çünkü
    //          vermiyor: Attack(battle, a, b)'yi arka arkaya iki kez çağır;
    //          isabet eden birinci çağrı battle.Turn.EndTurn()'ü çağırdığı için
    //          ikincisi RejectedActorCannotAct döner. Ölçü şu: farkı doğuran
    //          yer Battle, burası değil — aynı Battle'ı sıfırdan kurup aynı
    //          çağrıyı tekrarla, ilk cevabı yeniden alırsın. Bu tip static'tir
    //          ve tek bir alanı yoktur; DEĞİŞTİRDİĞİ şey Battle'ın tahtası,
    //          savaşçıları ve sırasıdır
    // Unity  : gerekmez — noEngineReferences: true
    // karar  : AKIŞI yürütür; kuralların hiçbirini kendisi yazmaz
    //
    // DÖRT EYLEMİN ORTAK İSKELETİ, SIRA BU DOSYANIN TEK YAPISAL KARARI: önce
    // çağıran hataları (ADIM 0-1, istisna), sonra kurallar (ADIM 2-5, sonuç
    // değeri), en sonda TEK YAZMA ve sıra devri (ADIM 6-7). Çizginin üstü soru,
    // altı olgu: bir kural geri dönülemez adımın ALTINA düşerse kural olmaktan
    // çıkıp açıklamaya döner. → BattleActions.md#battleactions-iskelet
    /// <summary>
    /// Tahta ile savaşı birleştiren akışın tek sahibi.
    ///
    /// Bu dosyanın var olma sebebi tek satır: <see cref="AttackAction"/> mesafeyi
    /// DIŞARIDAN alıyordu ve o mesafeyi üretecek kimse yoktu. Üreteni
    /// <see cref="GridDistance"/>, kimin nerede durduğunu <see cref="Battle"/>,
    /// saldırının çözümünü <see cref="AttackAction"/> biliyor — üçü de birbirini
    /// TANIMAZ. Onları bir sıraya dizen tek yer burasıdır.
    ///
    /// DÖRT EYLEM, TEK ŞEKİL: saldırı, hareket, diriltme, yerleştirme. Sıra
    /// kuralını soran taraf da burasıdır, çünkü "aktif takım" diye bir kavram
    /// <c>GridStrategy.Combat</c>'ta yoktur — kuralı uygulayabilen en alt katman
    /// bu.
    ///
    /// Neyi BİLMEZ: mesafenin Chebyshev mi Manhattan mı olduğunu, hasarın nasıl
    /// hesaplandığını, hedefin neden uygun olduğunu, sıranın nasıl devredildiğini,
    /// sonucu kimin göstereceğini. Buradaki her <c>if</c> bir kuralı SORAR;
    /// hiçbiri bir kural YAZMAZ.
    ///
    /// GEREKÇELER: Docs/deep/kod/Battle/BattleActions.md
    /// </summary>
    // DERİN ANLATIM: Docs/deep/konular/04-karar-sirasi.md
    // HARİTA: Docs/deep/00-iskelet.md — dört eylemin bu dosyada buluşmasının
    // sebebi sistemin tamamına bakınca görünüyor: hangi tasarım basıncı hangi
    // parçayı doğurdu ve hangi soru hangi dosyaya gidiyor.
    public static class BattleActions
    {
        /// <summary>
        /// Bir saldırı denemesini yürütür: konumları <see cref="Battle"/>'dan
        /// bulur, mesafeyi <see cref="GridDistance"/>'a ölçtürür ve saldırıyı
        /// <see cref="AttackAction"/>'a çözdürür.
        ///
        /// SALDIRAN da HEDEF de bir BİRİM ya da bir YAPI olabilir; hangisi
        /// olduğunu bu metot <see cref="Battle.TryGetStructure"/>'a SORAR.
        ///
        /// OYUNDA NE İŞE YARAR: oyuncu kendi kulesini seçip düşmana
        /// tıkladığında ateş eden yol budur. Eskiden aynı tıklama oyunu
        /// patlatıyordu — saldıran koşulsuz bir savaşçı sanılıyordu.
        /// </summary>
        // DERİN ANLATIM: Docs/deep/konular/04-karar-sirasi.md
        private static AttackOutcome Strike(
            Battle battle, Unit attacker, Unit target, bool spendsTurn)
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
            // SALDIRAN DA BİR YAPI OLABİLİR, ve bu soru hedefinkinden ÖNCE
            // soruluyor: eskiden burada koşulsuz bir RequireCombatant vardı ve
            // kendi kulesini seçip düşmana tıklayan oyuncu "bu birim savaşta
            // değil" istisnasıyla karşılaşıyordu — bir oyun kuralı değil, bir
            // çökme. Cevabı zaten bilen tek yer Battle; çağırandan bir
            // `attackerIsStructure` bayrağı istemek aynı çökmeyi yanlış
            // dolduran her çağırana geri verirdi.
            bool attackerIsStructure =
                battle.TryGetStructure(attacker, out Structure attackerStructure);

            // Savaşçı tarafı yalnızca saldıran bir yapı DEĞİLSE aranıyor; ne
            // yapı ne savaşçı olan bir kimlik hâlâ istisna atar ve atmalı —
            // "bu savaşta değil" bir çağıran hatasıdır, bir ret değil.
            Combatant attackerCombatant = attackerIsStructure
                ? null
                : RequireCombatant(battle, attacker, nameof(attacker));

            // HEDEFİN NE OLDUĞU SORULUYOR, TAŞINMIYOR. Cevabı zaten bilen tek yer
            // Battle; çağırandan bir `targetIsStructure` bayrağı istemek, o
            // bayrağı yanlış dolduran bir çağıranın barakayı RequireCombatant'a
            // düşürüp ilgisiz bir istisna atmasına yol açardı.
            // → BattleActions.md#attack-targetisstructure
            bool targetIsStructure = battle.TryGetStructure(target, out Structure targetStructure);

            // Birim tarafı yalnızca hedef bir yapı DEĞİLSE aranıyor: yapıların
            // Combatant'ı yok ve olmamalı (Structure, Combatant'tan türemiyor —
            // gerekçe Structure.cs'in başında).
            Combatant targetCombatant = targetIsStructure
                ? null
                : RequireCombatant(battle, target, nameof(target));

            RequireCell(battle, attacker, nameof(attacker), out int attackerX, out int attackerY);
            RequireCell(battle, target, nameof(target), out int targetX, out int targetY);

            // TARAF SAVAŞ PARÇASINDAN GELİYOR, BİRİMDEN DEĞİL: Unit tarafı
            // bilmez, ve bir kule de bir taraf tutar. Tek satırda birleşiyorlar
            // çünkü sıra kuralı saldıranın NE olduğuna bakmaz.
            Team attackerTeam = attackerIsStructure
                ? attackerStructure.Team
                : attackerCombatant.Team;

            // SIRA KURALI HER ŞEYDEN ÖNCE SORULUYOR — hedefin uygunluğundan da,
            // menzilden de önce. Aşağıya, AttackAction.Execute'un ALTINA
            // alınsaydı ret geldiğinde hasar çoktan inmiş olurdu ve "reddedildi"
            // bir kural değil bir metin olurdu.
            //
            // SORU ARTIK SIRA DURUMUNA SORULUYOR, KURALA DOĞRUDAN DEĞİL: kum
            // havuzunda sıra bir kapı değil yalnızca bir gösterge ve bunu bilen
            // tek yer TurnState. Kuralın METNİ hâlâ TurnRules'ta; kalkan şey
            // "hangi kural sorulacak" kararının bu dosyaya dağılmasıydı.
            // → BattleActions.md#attack-turnrulescanact
            if (!battle.Turn.AllowsAction(attackerTeam))
            {
                return AttackOutcome.RejectedActorCannotAct;
            }

            // TAKIM ÖN KONTROLÜ BURADAN KALKTI ve bu bir borcun kapanışı: kuralın
            // METNİ aşağı indi (AttackAction artık TargetingRules'a kendisi
            // soruyor), silinmedi. Kalkan şey kuralın kendisi değil, aynı kuralın
            // İKİNCİ kez sorulması — kopya, hedef tipi başına çoğalırdı.
            // → BattleActions.md#attack-targetingrulescanbeattacked

            // Mesafe BURADA hesaplanmıyor, HESAPLATILIYOR. Chebyshev kararı
            // GridDistance.Between'in içinde yaşıyor ve buraya kopyalanmış bir
            // Math.Max(Math.Abs(...), ...) o kararı ikinci bir yere yazardı.
            // Attack_DiagonalNeighbourWithRangeOne_Hits testi tam olarak bu
            // sahipliği koruyor: çapraz komşu Chebyshev'de 1, Manhattan'da 2.
            int distance = GridDistance.Between(attackerX, attackerY, targetX, targetY);

            // DÖRT AŞIRI YÜKLEME, TEK AKIŞ. Dallanma burada bitiyor çünkü
            // ayrılan tek şey iki tarafın TİPİ; sıra, mesafe ve ret sıralaması
            // dört dalda da aynı. Ortak bir arayüz arkasında tek çağrıya
            // inilseydi hedef uygunluğu kuralı TargetingRules'tan parçaların
            // İÇİNE taşınır ve "düştü" ile "yıkıldı" aynı bool'un arkasına
            // düşerdi.
            AttackOutcome outcome;
            if (attackerIsStructure)
            {
                outcome = targetIsStructure
                    ? AttackAction.Execute(attackerStructure, targetStructure, distance)
                    : AttackAction.Execute(attackerStructure, targetCombatant, distance);
            }
            else
            {
                outcome = targetIsStructure
                    ? AttackAction.Execute(attackerCombatant, targetStructure, distance)
                    : AttackAction.Execute(attackerCombatant, targetCombatant, distance);
            }

            // SIRA BURADA DEVREDİLİR — ve bu satır olmadan oyun KIRIKTI: kural
            // soruluyordu ama EndTurn üretimde hiç çağrılmıyordu. Liste BEYAZ,
            // kara değil: yarın eklenen bir ret değeri kara listede varsayılan
            // olarak sırayı YAKAR, beyaz listede yakmaz — hata en fazla "sıram
            // bitmedi" olur ve o yön geri alınabilir.
            // → BattleActions.md#attack-endturn
            bool attacked = outcome == AttackOutcome.Hit
                || outcome == AttackOutcome.HitAndDowned
                || outcome == AttackOutcome.HitAndFinished
                || outcome == AttackOutcome.HitAndDestroyed;

            if (attacked && spendsTurn)
            {
                battle.Turn.EndTurn();
            }

            return outcome;
        }

        /// <summary>
        /// Oyuncunun verdiği saldırı emri: isabet ederse sırayı devreder.
        /// </summary>
        public static AttackOutcome Attack(Battle battle, Unit attacker, Unit target)
        {
            return Strike(battle, attacker, target, spendsTurn: true);
        }

        /// <summary>
        /// Kendiliğinden ateş eden bir yapının saldırısı: isabet etse bile
        /// sırayı DEVRETMEZ.
        /// </summary>
        // AYRI BİR ADLI ÜYE, ÇIPLAK BİR BOOL DEĞİL — ve farkı çağrı yerinde
        // okunuyor: `Attack(battle, kule, hedef, false)` satırını okuyan biri
        // false'un neyi kapattığını bilmez. Emri KİMİN verdiği bu tasarımın tek
        // ayrımı ve adın kendisi onu söylüyor.
        //
        // ÖLÇÜLEN ZARAR: kule her 2 saniyede bir ateş edip sırayı düşmana
        // veriyordu, yani oyuncunun hakkını kendi binası harcıyordu. Bugün
        // varsayılan kip FreeForAll olduğu için gizliydi; Alternating seçilen
        // ilk gün görünür olurdu.
        public static AttackOutcome AttackWithoutSpendingTurn(
            Battle battle, Unit attacker, Unit target)
        {
            return Strike(battle, attacker, target, spendsTurn: false);
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
        // DERİN ANLATIM: Docs/deep/konular/03-tahta-sahipligi.md + Docs/deep/konular/06-sonuc-enumlari.md
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

            // PROFİL BURADA VE KURALLARDAN ÖNCE KURULUYOR: sayıyı tipine çeviren
            // yer ikisini de tanıyan akıştır, ve negatif menzil bir ÇAĞIRAN
            // HATASIdır. Aşağı alınsaydı, sırası gelmemiş bir birim için bozuk
            // sayı sessizce RejectedActorCannotAct'e dönerdi ve hiç görülmezdi.
            // → BattleActions.md#move-moveprofile
            var profile = new MoveProfile(moveRange);

            Combatant combatant = RequireCombatant(battle, unit, nameof(unit));
            RequireCell(battle, unit, nameof(unit), out int fromX, out int fromY);

            // İKİ KURAL, TEK RET DEĞERİ — ve sıraları GÖZLENEMEZ: ikisi de aynı
            // MoveOutcome.RejectedActorCannotAct'i döndürüyor. Burada korunacak
            // bir sıra kararı YOK, çünkü sıra kararı ancak farklı cevaplar
            // arasında var olur.
            //
            // Kipi bilen tek yer TurnState; gerekçesi Attack'te tek kez yazılı.
            // → BattleActions.md#move-turnrulescanact
            if (!battle.Turn.AllowsAction(combatant.Team))
            {
                return MoveOutcome.RejectedActorCannotAct;
            }

            // DÜŞMÜŞ BİRİM ARTIK YÜRÜYEMEZ, ve kural burada soruluyor çünkü
            // MoveAction bunu SORAMAZ: MovementRules ile UnitState
            // GridStrategy.Combat'ta, MoveAction ise Core'da ve Core, Combat'ı
            // GÖRMEZ. Kural inebildiği kadar iner; inebildiği yer burası.
            // → BattleActions.md#move-movementrulescanmove
            if (!MovementRules.CanMove(combatant.State))
            {
                return MoveOutcome.RejectedActorCannotAct;
            }

            // Alternatif: MoveAction'ın int alan aşırı yüklemesini çağırmayı
            // sürdürmek. Seçilmedi: o sürümün kalkma eşiği MoveAction'da yazılı
            // ve son int çağıranı burasıydı; çağrı değişmezse eşik hiç tetiklenmez
            // ve MoveProfile üretimde çağıranı olmayan bir tip kalır.
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
        /// Menzil SORMAYAN hareket: oyuncu haritada bir yere tıklar, birim yolu
        /// varsa oraya yürür.
        ///
        /// OYUNDA NE İŞE YARAR: tahtanın bugün kullandığı hareket budur. "Bu tur
        /// en fazla şu kadar hücre" kısıtı kalktı; yerine ULAŞILABİLİRLİK geldi.
        /// Sıra kuralı ve durum kuralı DEĞİŞMEDİ — sırası olmayan ya da düşmüş
        /// bir birim hâlâ yürüyemez.
        /// </summary>
        /// <param name="path">
        /// Birimin sırayla basacağı hücreler; ekran yürüyüşü buna bakarak
        /// canlandırır. Ret hâlinde boştur.
        /// </param>
        public static MoveOutcome Move(
            Battle battle,
            Unit unit,
            int toX,
            int toY,
            out List<GridStep> path)
        {
            path = new List<GridStep>();

            if (battle == null)
            {
                throw new ArgumentNullException(nameof(battle));
            }

            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

            Combatant combatant = RequireCombatant(battle, unit, nameof(unit));
            RequireCell(battle, unit, nameof(unit), out int fromX, out int fromY);

            // İki kural, tek ret değeri — menzilli sürümdeki ile birebir aynı
            // gerekçe, bu yüzden orada yazılı ve burada tekrarlanmıyor.
            if (!battle.Turn.AllowsAction(combatant.Team))
            {
                return MoveOutcome.RejectedActorCannotAct;
            }

            if (!MovementRules.CanMove(combatant.State))
            {
                return MoveOutcome.RejectedActorCannotAct;
            }

            MoveOutcome outcome =
                MoveAction.ExecuteAlongPath(battle.Board, unit, fromX, fromY, toX, toY, out path);

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

            // YAPI DİRİLTEMEZ VE BU BİR KURALDIR, İSTİSNA DEĞİL. Aynı hata
            // saldırı ve hareket yollarında bu turda kapatıldı; diriltme yolu
            // gözden kaçmıştı ve sahadaki karşılığı şuydu: oyuncu kendi kulesini
            // seçip düşmüş bir dosta tıkladığında oyun ArgumentException
            // fırlatıyordu. Tanınmayan bir kimlik için istisna DURUYOR — o
            // gerçekten bir programcı hatası.
            if (battle.TryGetStructure(reviver, out Structure _))
            {
                return ReviveOutcome.RejectedActorCannotAct;
            }

            Combatant reviverCombatant = RequireCombatant(battle, reviver, nameof(reviver));
            Combatant targetCombatant = RequireCombatant(battle, target, nameof(target));

            RequireCell(battle, reviver, nameof(reviver), out int reviverX, out int reviverY);
            RequireCell(battle, target, nameof(target), out int targetX, out int targetY);

            // DİRİLTME MEKANİZMASI DEĞİŞMEDİ: aşağıdaki üç kural — dirilticinin
            // durumu, hedefin uygunluğu, menzil — aynen duruyor. Kum havuzunun
            // açtığı tek kapı bu satır.
            if (!battle.Turn.AllowsAction(reviverCombatant.Team))
            {
                return ReviveOutcome.RejectedActorCannotAct;
            }

            // DİRİLTENİN KENDİ DURUMU AYRI BİR TİPE SORULUYOR. Kural ne buraya
            // YAZILDI (o zaman savaş kurmadan sınanamaz olurdu) ne de
            // AttackRules'tan TÜRETİLDİ (o zaman "yaralı sıhhiyeci vuramaz ama
            // kaldırabilir" günü sessizce kırılırdı). Üç aktör kuralı bugün aynı
            // satırı taşıyor ve bu bir TESADÜF — ayrı durmalarının sebebi bu.
            // → BattleActions.md#revive-reviverulescanrevive
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

            // DİRİLTME MENZİLİ BUGÜN SALDIRI PROFİLİNDEN GELİYOR — bir taviz, ve
            // gizlenmiyor: "erişim" kavramının bu projedeki tek sahibi
            // AttackProfile, yani bir okçu üç hücre öteden diriltir. Buraya bir
            // `const int ReviveRange` yazmak, akışa bir DENGE SAYISI sahiplendirir
            // ve bu dosyada başka hiçbir sayı yok.
            // → BattleActions.md#revive-attackresolveriswithinrange
            if (!AttackResolver.IsWithinRange(distance, reviverCombatant.AttackProfile))
            {
                return ReviveOutcome.RejectedOutOfRange;
            }

            // TryRevive'ın dönüşü YOK SAYILMIYOR. Buraya ulaşıldığında hedefin
            // Downed olduğu bir satır önce doğrulandı, yani false dönmesi
            // Combatant.State ile UnitLifecycle.State'in ayrıştığı anlamına
            // gelir — bugün imkânsız. Dönüşü sessizce atmak o imkânsızlığı bir
            // VARSAYIMA çevirirdi. → BattleActions.md#revive-tryrevive
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
        /// döndürür. İkisinin farkı PlacementOutcome'un belgesinde yazılı.
        /// </summary>
        /// <param name="unit">
        /// Yapının tahtadaki kimliği. Bir baraka da bir <see cref="Unit"/>'tir —
        /// "tahtada yer kaplayan, kimliği olan şey" — ve ikinci bir tahta
        /// açmamanın bedeli tam olarak budur.
        /// </param>
        // DERİN ANLATIM: Docs/deep/konular/04-karar-sirasi.md + Docs/deep/konular/06-sonuc-enumlari.md
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

            // ██ HER TAKIMDA EN FAZLA BİR ANA KULE ██
            // OYUNDA NE İŞE YARAR: ana kulesi yıkılan taraf kaybediyor. Kural
            // ikinci bir kule kurmaya izin verseydi, oyuncu her yıkımdan sonra
            // yenisini dikerdi ve zafer koşulu hiçbir zaman gerçekleşmezdi —
            // yani kuralın kendisi kendini iptal ederdi.
            //
            // SORU "AYAKTA MI" DİYE SORULUYOR, "HİÇ KURDU MU" DİYE DEĞİL, ve
            // fark bir tasarım kararı: kulesi yıkılan taraf zaten kaybetmiş
            // durumda ve o hâlde ikinci bir kule kurmasını engellemenin bir
            // anlamı yok. Battle.HasEverPlacedHeadquarters buraya konsaydı
            // aynı ret, savaş bittikten sonra da konuşmaya devam ederdi.
            //
            // SIRA BİR KARARDIR: hücre soruları ÖNCE. Bu ret üste konsaydı,
            // tahtanın DIŞINA konmak istenen ikinci bir ana kule "kule zaten
            // var" cevabını alırdı — oysa düzeltilemeyen sebep hücrenin kendisi.
            //
            // KURAL BURADA, PALETTE DEĞİL: sol panelin düğmeyi sönük göstermesi
            // ayrı bir iştir ve o gün geldiğinde bu üyeye SORARAK yapılır.
            // Palette yazılsaydı ikinci bir kural sahibi doğar ve klavyeli yol
            // ile sürükleme yolu farklı cevaplar verirdi.
            if (structure.IsHeadquarters && battle.HasStandingHeadquarters(structure.Team))
            {
                return PlacementOutcome.RejectedHeadquartersExists;
            }

            // "BU BİRİM ZATEN SAVAŞTA" BURADA BİR RET DEĞİL, BİR ÇAĞIRAN
            // HATASI — ve kontrolü kopyalanmıyor, AddStructure'a bırakılıyor.
            // Yıkık bir yapı için de ret değeri YOK: bugün tahtaya konmadan
            // yıkılmış bir Structure üretebilen tek yol testtir.
            // → BattleActions.md#placestructure-addstructure
            battle.AddStructure(unit, structure, x, y);

            return PlacementOutcome.Placed;
        }

        // "BU SAVAŞTA DEĞİL" BİR OYUN SONUCU DEĞİL, BİR ÇAĞIRAN HATASIDIR: Battle
        // konumun ve eşleşmenin tek sahibi, tanımadığı bir birimle çağrılması
        // "benim kaydım Battle'ınkiyle ayrışmış" demektir. Ayrım ölçütü tek — bu
        // cevabı alan çağıran YAPACAK BİR ŞEY bulabilir mi? Bulamıyorsa istisna.
        // → BattleActions.md#requirecombatantbattle-unit-string
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
        // → BattleActions.md#requirecellbattle-unit-string-out-int-out-int
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
