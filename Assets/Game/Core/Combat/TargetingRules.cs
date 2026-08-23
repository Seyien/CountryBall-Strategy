using System;

namespace GridStrategy.Combat
{
    // ═══ ROL: KURAL (Policy) ═════════════════════════════════════════
    // kimlik : yok — static; iki çağrıyı ayıracak bir şey yoktur
    // hafıza : yok — CanBeAttacked(UnitState.Downed, Team.Player, Team.Enemy)
    //          kaç kez çağrılırsa çağrılsın true. Cevabı belirleyen yalnız
    //          DURUM değil: CanBeAttacked(Alive) true iken
    //          CanBeAttacked(Alive, Player, Player) false döner
    // Unity  : gerekmez — girdi yalnızca enum; sınamak için ne sahne ne kare
    //          gerekir
    // karar  : UYGUNLUK söyler; ne saldırır ne iyileştirir
    /// <summary>
    /// "Bu yetenek bu hedefe uygulanabilir mi?" sorusunun tek sahibi.
    ///
    /// Bu kural üç kez reddedildi ve gerekçesi hep aynıydı: <see cref="Health"/>
    /// hedefin ne olduğunu bilmemeli, <see cref="UnitLifecycle"/> kimin
    /// saldırdığını bilmemeli, <see cref="AttackResolver"/> yalnızca mesafe
    /// ölçmeli. Geriye ÜÇÜNCÜ bir sahip kaldı — burası.
    ///
    /// Neden iki ayrı metot: ayakta olan birim VURULABİLİR ama DİRİLTİLEMEZ; tek
    /// bir "uygun mu" metodu iki hedef kümesinin ayrıştığı bu noktayı taşıyamazdı.
    ///
    /// İKİ DURUM DİLİ KONUŞUR: <see cref="UnitState"/> ve
    /// <see cref="StructureState"/>. Tek enum alternatifinin bedeli her switch'te
    /// asla çalışmayan bir <c>Downed</c> dalıydı.
    ///
    /// GEREKÇELER: Docs/deep/kod/Core/Combat/TargetingRules.md
    /// </summary>
    public static class TargetingRules
    {
        /// <summary>
        /// Saldırı hedefleyebilir mi? <see cref="UnitState.Downed"/> dahildir —
        /// düşmüş birime vurmak "işini bitirme" yoludur ve tasarımın parçasıdır.
        /// Kural bilerek AÇIK uçlu: varsayılan cevabın EVET olması isteniyor.
        /// </summary>
        // GİRDİSİ ENUM OLAN KURAL NESNENİN DIŞINDA YAŞAR: burada girdi kümesi TAM
        // ve SONLU, üç satırda tüketiliyor. Combatant'ın bir property'si olsaydı
        // kuralı okumak için önce Health + UnitLifecycle + AttackProfile kurmak
        // gerekir ve durum matrisi tek enum ile yazılamaz olurdu.
        // → TargetingRules.md#canbeattackedunitstate-state
        // DERİN ANLATIM: Docs/deep/konular/05-yasam-dongusu.md
        public static bool CanBeAttacked(UnitState state)
        {
            return state != UnitState.Dead;
        }

        /// <summary>
        /// Saldırı hedefleyebilir mi — durum VE taraf birlikte. Sırasıyla:
        /// <see cref="Team.None"/> SALDIRAMAZ, aynı takıma saldırılmaz, geri kalanı
        /// durum kuralı — ve o kural burada KOPYALANMIYOR, tek parametreli sürüme
        /// soruluyor.
        ///
        /// <see cref="Team.None"/> HEDEF olarak herkese açıktır: yıkılabilir duvar,
        /// nötr kaynak düğümü, tuzak. Kapalı yapılsaydı tarafsız her şey sonsuza dek
        /// <see cref="AttackOutcome.RejectedInvalidTarget"/> döner ve yolu duvarla
        /// kesilmiş bir yapay zekâ hiçbir çıkış bulamazdı.
        /// </summary>
        // Bu sürüm ilk ikisinin kopyası değil ÇAĞIRANIDIR. Tek parametreli sürüm
        // yine de silinmedi: durumu taraftan BAĞIMSIZ soran gerçek çağıranları var
        // ve silinseydi o testlerin her satırına uydurma bir takım çifti eklenirdi.
        // → TargetingRules.md#canbeattackedunitstate-state-team-attackerteam-team-targetteam
        // DERİN ANLATIM: Docs/deep/konular/04-karar-sirasi.md
        public static bool CanBeAttacked(UnitState state, Team attackerTeam, Team targetTeam)
        {
            if (!IsHostilePairing(attackerTeam, targetTeam))
            {
                return false;
            }

            // Durum kuralı burada KOPYALANMIYOR, soruluyor. Kopyalansaydı
            // "Downed vurulabilir" kararı iki yerde yaşardı ve biri
            // değiştiğinde diğeri sessizce eskirdi.
            return CanBeAttacked(state);
        }

        /// <summary>
        /// Bir YAPI saldırı hedefleyebilir mi? Yalnızca
        /// <see cref="StructureState.Standing"/>. Yıkılmış bir yapı geçerli hedef
        /// değildir: enkazın canı yoktur, vurulması hiçbir şeyi değiştirmez.
        /// </summary>
        // KAPALI UÇLU (`== Standing`), birim ikizi ise AÇIK uçlu — ve bu bilinçli:
        // yapıda "işini bitirme" gibi bir ara durum yok. `!= Destroyed` bugün aynı
        // cevabı verir; fark Rubble ya da Damaged eklendiği gün doğar ve o gün açık
        // uç yeni değeri SESSİZCE hedeflenebilir sayardı.
        // → TargetingRules.md#canbeattackedstructurestate-state
        // DERİN ANLATIM: Docs/deep/konular/05-yasam-dongusu.md
        public static bool CanBeAttacked(StructureState state)
        {
            return state == StructureState.Standing;
        }

        /// <summary>
        /// Bir YAPI saldırı hedefleyebilir mi — durum VE taraf birlikte.
        ///
        /// Taraf kuralı birim sürümüyle BİREBİR aynıdır ve bu bir tesadüf değil:
        /// "kime vurulur"un cevabı hedefin ne OLDUĞUNA değil hangi TARAFTA
        /// olduğuna bağlıdır. Kural bu yüzden kopyalanmıyor, iki sürüm de aynı
        /// <c>IsHostilePairing</c>'i soruyor.
        /// </summary>
        // Kendi barakanı yıkmak, kendi tankını vurmakla aynı hatadır; tarafsız
        // duvar ise iki sürümde de herkese açıktır.
        // → TargetingRules.md#canbeattackedstructurestate-state-team-attackerteam-team-targetteam
        public static bool CanBeAttacked(StructureState state, Team attackerTeam, Team targetTeam)
        {
            if (!IsHostilePairing(attackerTeam, targetTeam))
            {
                return false;
            }

            return CanBeAttacked(state);
        }

        // YAPININ DİRİLTME İKİZİ YOK — unutulmuş bir satır değil, kararın kendisi.
        // Yapı DİRİLMEZ: yıkık bina onarılmaz, yeniden İNŞA edilir.
        // Structure.TryRepair bir ONARIMdır (durum geçişi değil, yalnızca bir sayı
        // değişikliği) ve ön koşulunu kendi içinde taşır; uydurma bir
        // CanBeRepaired aynı ön koşulu ikinci bir eve koyar ve burası canı hiç
        // görmediği için ikisi sessizce ayrışır.
        // → TargetingRules.md#targetingrules-tip

        /// <summary>
        /// Diriltme hedefleyebilir mi? Yalnızca <see cref="UnitState.Downed"/>:
        /// ayakta olanın diriltmeye ihtiyacı yok, kalıcı ölü artık kurtarılamaz.
        /// </summary>
        // AYRI METOT, PARAMETRE DEĞİL: yetenek cevabı DARALTMIYOR, TERS ÇEVİRİYOR
        // — Alive için saldırı EVET, diriltme HAYIR. Tek metoda katlansaydı iki
        // hedef kümesinin kesiştiği tek nokta (Downed) bir enum parametresinin
        // arkasına düşer ve karşılaştırılamaz olurdu.
        // → TargetingRules.md#canberevivedunitstate-state
        public static bool CanBeRevived(UnitState state)
        {
            return state == UnitState.Downed;
        }

        /// <summary>
        /// Diriltme hedefleyebilir mi — durum VE taraf birlikte.
        ///
        /// Taraf kuralı saldırının KOPYASI değil, TERSİ: saldırı FARKLI takım
        /// ister, diriltme AYNI takımı. <see cref="Team.None"/> burada İKİ tarafta
        /// da kapalı; yalnız "aynı takım" yazılsaydı None == None sınavı geçer ve
        /// bir duvar başka bir duvarı diriltirdi.
        /// </summary>
        // Düşmanını ayağa kaldırmak bir yetenek değil bir hatadır; bu metot
        // olmasaydı yazılacak ilk diriltme yeteneği tam o hatayı yapardı ve hiçbir
        // test kırmızıya dönmezdi.
        // → TargetingRules.md#canberevivedunitstate-state-team-reviverteam-team-targetteam
        public static bool CanBeRevived(UnitState state, Team reviverTeam, Team targetTeam)
        {
            if (reviverTeam == Team.None || targetTeam == Team.None)
            {
                return false;
            }

            if (reviverTeam != targetTeam)
            {
                return false;
            }

            return CanBeRevived(state);
        }

        // SALDIRININ TARAF KURALI — TEK YERDE. Birim ve yapı sürümleri bunu ortak
        // soruyor; kopyalansaydı "dost ateşi yok" kararı iki yerde yaşardı.
        // Diriltmenin taraf kuralı buraya KATLANMADI: o kural bunun kopyası değil
        // TERSİ. `attackerTeam == Team.None` satırı ismen gerekli, çünkü
        // `default(Team)` geçerli bir Team'dir ve her toplu ayırma onunla dolar.
        // → TargetingRules.md#ishostilepairingteam-attackerteam-team-targetteam
        private static bool IsHostilePairing(Team attackerTeam, Team targetTeam)
        {
            if (attackerTeam == Team.None)
            {
                return false;
            }

            // Tarafsız HEDEF bilerek açık: yıkılabilir duvarın var olma sebebi
            // yıkılmaktır. Kapalı olsaydı tarafsız her şey sonsuza dek geçersiz
            // hedef olurdu.
            return attackerTeam != targetTeam;
        }
    }
}
