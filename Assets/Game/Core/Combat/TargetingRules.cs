using System;

namespace GridStrategy.Combat
{
    // ═══ ROL: KURAL (Policy) ═════════════════════════════════════════
    // kimlik : yok — static; iki çağrıyı ayıracak bir şey yoktur
    // hafıza : yok — aynı durum her zaman aynı cevabı verir
    // Unity  : gerekmez
    // karar  : UYGUNLUK söyler; ne saldırır ne iyileştirir
    /// <summary>
    /// "Bu yetenek bu hedefe uygulanabilir mi?" sorusunun tek sahibi.
    ///
    /// Bu kural üç kez reddedildi ve her seferinde gerekçesi aynıydı:
    /// <see cref="Health"/> hedefin ne olduğunu bilmemeli, <see cref="UnitLifecycle"/>
    /// kimin saldırdığını bilmemeli, <see cref="AttackResolver"/> yalnızca mesafe
    /// ölçmeli. Geriye ÜÇÜNCÜ bir sahip kaldı — burası.
    ///
    /// Neden iki ayrı metot: ayakta olan birim VURULABİLİR ama DİRİLTİLEMEZ —
    /// iki hedef kümesinin ayrıştığı nokta budur (düşmüş birim ikisine de açık,
    /// kalıcı ölü ikisine de kapalı). Tek bir "uygun mu" metodu onu taşıyamazdı.
    ///
    /// İKİ DURUM DİLİ KONUŞUR: <see cref="UnitState"/> ve <see cref="StructureState"/>.
    /// Bu bir tekrar değil, StructureState.cs'te BİLEREK ödenen bedeldi ve şimdi
    /// ödendi. Alternatifi tek bir enum'du ve onun bedeli her switch'te asla
    /// çalışmayan bir <c>Downed</c> dalıydı — ikinci aşırı yüklemeyi derleyici
    /// ister, ölü dalı hiç kimse istemez.
    ///
    /// ROL TABLOSU — birebir eşleşmeyen satır en öğretici olandır:
    /// <list type="table">
    /// <item><term>saldırı</term><description>birim: var · yapı: VAR</description></item>
    /// <item><term>diriltme</term><description>birim: var · yapı: <b>YOK</b></description></item>
    /// </list>
    /// Yapının diriltme ikizi eksik değil, YANLIŞ olurdu: yapı dirilmez.
    /// <see cref="Structure.TryRepair"/> bir ONARIMdır — durum geçişi değil, yalnızca
    /// bir sayı değişikliği — ve ön koşulunu ("ayakta olmayan onarılmaz") kendi
    /// içinde taşır. Gerekçenin tamamı aşağıdaki REDDEDILEN bloğunda.
    /// </summary>
    public static class TargetingRules
    {
        /// <summary>
        /// Saldırı hedefleyebilir mi? <see cref="UnitState.Downed"/> dahildir —
        /// düşmüş birime vurmak "işini bitirme" yoludur ve tasarımın parçasıdır.
        /// Buraya Downed'ı kapatan bir satır koymak, Combatant.TakeDamage içinde
        /// reddedilen `if (!health.HasRemaining) return;` ile aynı olurdu.
        /// </summary>
        // REDDEDILEN - TargetingRules.cs:57 yerine (Combatant'ın State
        //              property'sinin yanı):
        //     public bool CanBeAttacked => State != UnitState.Dead;
        // KIRILAN  : kuralı sınamak için üç parçadan bir Combatant kurmak gerekir.
        //            Health + UnitLifecycle + AttackProfile -> her satır nesne kurar
        //            durum matrisi tek enum ile yazılamaz -> matris ölçüsünü kaybeder
        //            derleyici: hiçbir şey der  .  test: yeşil kalır, ölçtüğü şey daralır
        // KAZANIRDI: hedef uygunluğu enum dışında birimin iç durumuna bağlı olsaydı —
        //            zırh, gizlenme, mermi — cevap durumdan değil nesneden gelirdi.
        // TEK CUMLE: Girdisi bir ENUM olan kural nesnenin dışında yaşayabilir, ve
        //            dışarıda yaşayan kural nesne kurmadan sınanır.
        public static bool CanBeAttacked(UnitState state)
        {
            return state != UnitState.Dead;
        }

        /// <summary>
        /// Saldırı hedefleyebilir mi — durum VE taraf birlikte.
        ///
        /// Üç kural, bu sırayla:
        /// <list type="number">
        /// <item><see cref="Team.None"/> SALDIRAMAZ. Tarafsız olan taraf tutmaz;
        /// duvar vurmaz.</item>
        /// <item>Aynı takıma saldırılmaz — dost ateşi bu oyunda yok.</item>
        /// <item>Geri kalanı durum kuralı, ve o kural burada KOPYALANMIYOR;
        /// tek parametreli sürüme soruluyor.</item>
        /// </list>
        ///
        /// <see cref="Team.None"/> HEDEF olarak herkese açıktır — kimseye kapalı
        /// değil. Sebebi <see cref="Team"/>'in None'ı niçin taşıdığıdır: yıkılabilir
        /// duvar, nötr kaynak düğümü, tuzak. Yıkılamayan duvar dekordur ve dekorun
        /// <see cref="Health"/>'e ihtiyacı yoktur; kapalı yapılsaydı tarafsız her
        /// şey sonsuza dek <see cref="AttackOutcome.RejectedInvalidTarget"/> döner
        /// ve yolu duvarla kesilmiş bir yapay zekâ hiçbir çıkış bulamazdı.
        /// </summary>
        // AttackAction bu sürümü değil taraflı sürümü çağırır; tek parametreli
        // sürüm yine de silinmedi, çünkü durumu taraftan BAĞIMSIZ soran gerçek
        // çağıranları var: DownedUnit_IsStillAttackableButCannotAttack ve
        // DownedUnit_IsStillAttackableButCannotMove, "aynı birim hedeftir ama
        // eyleyen değildir" asimetrisini takım parametresi hiç yazmadan sabitler.
        //
        // REDDEDILEN - TargetingRules.cs:102 yerine (üstteki tek parametreli sürüm
        //              silinir, AttackAction.Execute'un birim hedefli aşırı
        //              yüklemesindeki hedef uygunluğu çağrısı şuna döner):
        //     if (!TargetingRules.CanBeAttacked(target.State, attacker.Team, target.Team))
        // KIRILAN  : durum kuralını taraftan BAĞIMSIZ sormanın yolu kalmaz.
        //            Downed_IsTheOnlyStateBothAbilitiesAccept taraf ölçmüyor
        //            her satırına uydurma bir Player/Enemy çifti eklenir
        //            "Downed vurulabilir" kararı takım gürültüsünün altında kalır
        //            derleyici: hiçbir şey der  .  test: yeşil kalır, ölçtüğü şey bulanır
        // KAZANIRDI: hedef uygunluğu HİÇBİR zaman taraftan bağımsız sorulmayacaksa — o
        //            gün iki aşırı yükleme hangisinin gerçek kural olduğunu belirsizleştirir.
        // TEK CUMLE: İki eksenli bir kuralın tek eksenli sürümü, o ekseni yalnız
        //            başına sormak isteyen gerçek bir çağıran varsa silinmez.
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
        ///
        /// Birim ikizinin AYNISI DEĞİL ve olmaması bilinçli: orada kural
        /// <c>state != Dead</c>, yani "düşmüş birim hâlâ vurulur" — çünkü işini
        /// bitirme yolu tasarımın parçası. Yapıda öyle bir ara durum yok, o
        /// yüzden burada kural KAPALI uçlu yazıldı.
        /// </summary>
        // REDDEDILEN - TargetingRules.cs:136 yerine (birim sürümünün şekli
        //              körü körüne kopyalanır):
        //     public static bool CanBeAttacked(StructureState state)
        //         => state != StructureState.Destroyed;
        // KIRILAN  : açık uçlu kural, gelecekteki her yeni değeri SESSİZCE kabul eder.
        //            bugün ikisi de aynı cevabı verir -> hiçbir test seçimi koruyamaz
        //            Rubble geri gelir -> `!= Destroyed` enkazı geçerli hedef yapar
        //            oyuncu enkaza ateş etmeye devam eder
        //            derleyici: hiçbir şey der  .  test: yeni değer eklenince de yeşil
        // KAZANIRDI: eklenecek her yeni durumun varsayılan olarak HEDEFLENEBİLİR olması
        //            istenseydi — Damaged, Burning, Reinforced hepsi ayakta sayılsaydı.
        // TEK CUMLE: Açık uçlu kural yeni değeri kabul eder, kapalı uçlu kural onu
        //            SORDURUR; enum büyüdükçe fark eden tek şey budur.
        public static bool CanBeAttacked(StructureState state)
        {
            return state == StructureState.Standing;
        }

        /// <summary>
        /// Bir YAPI saldırı hedefleyebilir mi — durum VE taraf birlikte.
        ///
        /// Taraf kuralı birim sürümüyle BİREBİR aynıdır ve bu bir tesadüf değil:
        /// "kime vurulur" sorusunun cevabı hedefin ne OLDUĞUNA değil hangi TARAFTA
        /// olduğuna bağlıdır. Kural bu yüzden kopyalanmıyor, iki sürüm de aynı
        /// <c>IsHostilePairing</c>'i soruyor. Kendi barakanı yıkmak, kendi tankını
        /// vurmakla aynı hatadır; tarafsız duvar ise iki sürümde de herkese açıktır
        /// — zaten <see cref="Team.None"/>'ın var olma sebebi odur.
        /// </summary>
        public static bool CanBeAttacked(StructureState state, Team attackerTeam, Team targetTeam)
        {
            if (!IsHostilePairing(attackerTeam, targetTeam))
            {
                return false;
            }

            return CanBeAttacked(state);
        }

        // YAPININ DİRİLTME İKİZİ YOK — ve bu, unutulmuş bir satır değil, kararın
        // kendisi. Rol tablosunda birebir eşleşmeyen tek satır burasıdır.
        //
        // Yapı DİRİLMEZ: yıkık bina onarılmaz, yeniden İNŞA edilir ve yeniden inşa
        // bu enum'un bir geçişi değil, yepyeni bir Structure nesnesidir
        // (StructureState.cs ve StructureLifecycle.cs aynı kararı iki kez yazıyor).
        // Structure.TryRepair bir ONARIMdır ve diriltmeyle karıştırılmamalıdır:
        // diriltme bir DURUM geçişidir (yıkık → ayakta), onarım yalnızca bir SAYI
        // değişikliğidir (can artar, durum aynı kalır).
        //
        // REDDEDILEN - TargetingRules.cs:203 yerine (simetri uğruna uydurma bir
        //              ikiz eklenir):
        //     public static bool CanBeRepaired(StructureState state)
        //     {
        //         return state == StructureState.Standing;
        //     }
        // KIRILAN  : aynı ön koşul iki yerde yaşar; bu metot kuralı devralmaz, ÇOĞALTIR.
        //            Structure.TryRepair'in `if (!IsStanding)` kelepçesi kaldırılamaz
        //            burası canı hiç görmez -> "Damaged da onarılır" ikisini ayırır
        //            çağıran, kuralın evet dediği onarımın neden false döndüğünü anlamaz
        //            derleyici: hiçbir şey der  .  test: iki taraf ayrı ayrı yeşil kalır
        // KAZANIRDI: onarım gerçekten HEDEFLENEN bir yetenek olsaydı — menzilli tamirci,
        //            alan etkili onarım büyüsü, düşman binasını onaramama kuralı.
        // TEK CUMLE: Eksik ikiz bir boşluk değil bir cevaptır: onarım bir hedefleme
        //            sorusu değil, nesnenin kendi değişmezidir.

        /// <summary>
        /// Diriltme hedefleyebilir mi? Yalnızca <see cref="UnitState.Downed"/>:
        /// ayakta olanın diriltmeye ihtiyacı yok, kalıcı ölü artık kurtarılamaz.
        /// </summary>
        // REDDEDILEN - TargetingRules.cs:203 yerine (CanBeAttacked ile birlikte tek
        //              metoda katlanır):
        //     public static bool IsValidTarget(UnitState state, AbilityKind kind)
        //         => kind == AbilityKind.Revive ? state == UnitState.Downed : state != UnitState.Dead;
        // KIRILAN  : çağıran hangi yeteneği sorduğunu bir enum ile TAŞIMAK zorunda kalır.
        //            iki hedef kümesi tek metodun içine gizlenir -> karşılaştırılamaz
        //            Downed_IsTheOnlyStateBothAbilitiesAccept ölçtüğü şeyi kaybeder
        //            derleyici: hiçbir şey der  .  test: yeşil kalır, kesişimi göremez
        // KAZANIRDI: yetenek sayısı ona çıksaydı ve her biri aynı durum kümesinden
        //            farklı bir dilim isteseydi — on ayrı metot yerine tek tablo kazanırdı.
        // TEK CUMLE: Aynı hedef soran yeteneğe göre FARKLI cevap veriyorsa, yetenek bir
        //            parametre değil ayrı bir metottur.
        public static bool CanBeRevived(UnitState state)
        {
            return state == UnitState.Downed;
        }

        /// <summary>
        /// Diriltme hedefleyebilir mi — durum VE taraf birlikte.
        ///
        /// Taraf kuralı saldırının KOPYASI değil, TERSİ: saldırı FARKLI takım
        /// ister, diriltme AYNI takımı. Düşmanını ayağa kaldırmak bir yetenek
        /// değil, bir hatadır — ve bu metot var olmasaydı, yazılacak ilk
        /// diriltme yeteneği tam olarak o hatayı yapardı, üstelik hiçbir test
        /// kırmızıya dönmeden.
        ///
        /// <see cref="Team.None"/> burada İKİ tarafta da kapalı — ve bu,
        /// saldırıdaki kararla bilerek çelişir. Tarafsız hedefe saldırılabilir
        /// çünkü yıkılabilir duvarın var olma sebebi yıkılmaktır; tarafsız hedef
        /// diriltilemez çünkü duvarı ayağa kaldırmak kimsenin yeteneği değildir.
        /// Tarafsız DİRİLTEN de yoktur: yalnız "aynı takım" yazılsaydı
        /// None == None sınavı geçer ve bir duvar başka bir duvarı diriltirdi.
        /// </summary>
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

        // SALDIRININ TARAF KURALI — tek yerde. Birim ve yapı sürümleri bu metodu
        // ortak soruyor; kopyalasaydık "dost ateşi yok" kararı iki yerde yaşardı
        // ve biri değiştiğinde diğeri sessizce eskirdi. Kuralın metnini tek yerde
        // tutmak, bu dosyanın durum kuralı için zaten verdiği kararın aynısı.
        //
        // Diriltmenin taraf kuralı BURAYA KATLANMADI ve katlanmamalı: o kural
        // bunun kopyası değil TERSİ (aynı takım ister) ve Team.None'ı İKİ tarafta
        // da kapatır. Ortak bir "taraf kontrolü" metodu ikisini bir bayrakla
        // birleştirseydi, çağıran hangi yönü sorduğunu parametreyle taşımak
        // zorunda kalır ve iki kuralın ters olduğu gerçeği bir bool'un arkasına
        // gizlenirdi.
        //
        // REDDEDILEN - TargetingRules.cs:264 yerine (bu blok hiç yazılmaz,
        //              kural yalnız "aynı takım" olur):
        //     if (attackerTeam == targetTeam) { return false; }
        //     return CanBeAttacked(state);
        // KIRILAN  : takımı atanmayı unutulmuş birim — Team.cs'te sıfır bilerek None —
        //            kendi tarafı DAHİL herkesi geçerli hedef görür.
        //            birim vurur, testler yeşildir -> hata yalnız oyunda görülür
        //            kendi okçun kendi tankını vurur -> sebebi hiçbir yerde yazmaz
        //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
        // KAZANIRDI: tarafsız ama SALDIRGAN şeyler Team.None ile ifade edilecekse —
        //            yaban canavarları, herkese ateş eden nötr kuleler.
        // TEK CUMLE: Enum'un sıfırıncı değeri atanmamış her alanın varsayılanıdır, o
        //            yüzden kural onu ismen sormak zorundadır.
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
