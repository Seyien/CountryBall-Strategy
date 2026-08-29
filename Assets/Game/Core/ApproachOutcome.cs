namespace GridStrategy.Core
{
    // ═══ ROL: TANIM (Profile) ════════════════════════════════════════
    // kimlik : yok — iki MoveTo değeri aynı şeydir
    // hafıza : yok — ölçüsü şu: dönen değeri bir değişkene al, sonra tahtayı
    //          istediğin kadar oynat — birimi taşı, hedefi öldür, duvarı kaldır
    //          — değişken hâlâ MoveTo'dur. Değişen tek şey BİR SONRAKİ Plan
    //          çağrısının döndüreceği değerdir
    // Unity  : gerekmez — enum değerini okumak için ne sahne ne kare gerekir
    // karar  : vermez — olup biteni ADLANDIRIR; kararı ApproachRules verir
    /// <summary>
    /// "Vurabilmek için nerede durmalıyım?" sorusunun cevabının cinsi.
    ///
    /// OYUNDA NE İŞE YARAR: saldırıya uğrayan bir birim karşılık verirken önce
    /// bu soruyu sorar. Yakın dövüşçü saldırganın yanına yürür, menzilli birim
    /// kendi menziline girip durur — ve iki davranış da aşağıdaki dört değerin
    /// hangisinin döndüğüne bakarak ayrılır.
    ///
    /// Neden <c>bool</c> değil: "yürüyeyim mi" tek cevaba üç ayrı soruyu
    /// sıkıştırırdı ve çağıran üçüne farklı tepki verir — zaten menzildeyse
    /// hemen VURUR, bir hücre söylendiyse YÜRÜR, yol yoksa emri DÜŞÜRÜR.
    /// <c>bool</c> ile yazılsaydı bu ayrım çağıranın içinde bu kuralın
    /// mesafe karşılaştırmasını kopyalayan ikinci bir kontrol olarak yeniden
    /// doğardı.
    ///
    /// Kardeşleri <see cref="MoveOutcome"/> ve <c>AttackOutcome</c>; sıfırıncı
    /// değerin RET olması kuralı üçünde de aynı ve gerekçesi aynı satırda
    /// yazılı.
    /// </summary>
    // DERİN ANLATIM: Docs/deep/konular/11-karsilik-verme-ve-menzil.md
    public enum ApproachOutcome
    {
        // SIFIRINCI DEĞER BİLEREK BİR RET DEĞERİ. Bu enum'da tek bir "= 0"
        // yazılı değil; numarayı satır SIRASI belirler. Sıfır, dilin atanmamış
        // her alana verdiği değerdir — AlreadyInRange başa alınsaydı hiç
        // sorulmamış bir soru "zaten menzildesin" cevabını verir ve karşılık
        // veren birim yerinde durup havaya vururdu.
        /// <summary>
        /// Soru sorulamıyor: yürüyecek olan ya da hedef tahtada durmuyor.
        /// </summary>
        RejectedOffBoard,

        /// <summary>
        /// Hedef tahtada, ama menzile girecek boş ve YÜRÜNEBİLİR tek bir hücre
        /// yok — hedefin çevresi dolu ya da aradaki geçit kapalı.
        /// </summary>
        // İKİ SEBEP TEK DEĞERDE TOPLANDI VE ÖLÇÜT SEBEP SAYISI DEĞİL DAVRANIŞ
        // SAYISI: "çevresi dolu" ile "geçit kapalı" çağıranda AYNI şeye yol
        // açıyor — emir düşer. Ayrı iki değer, hiçbir çağıranın ayırmadığı bir
        // farkı tipe yazardı. Ayrım, birine bekleyip ötekine vazgeçen bir
        // çağıran doğduğu gün hak edilir.
        RejectedUnreachable,

        /// <summary>
        /// Yürünecek bir şey yok: durduğu hücreden hedefe zaten ulaşıyor.
        /// Bildirilen hücre birimin KENDİ hücresidir.
        /// </summary>
        // MENZİLLİ BİRİMİN DURDUĞU YER TAM OLARAK BURASI: okçu kendi menziline
        // girdiği an bu değeri alır ve bir adım daha atmaz. Yakın dövüşçü aynı
        // değeri saldırganın bitişiğinde alır. İki davranış, tek değer — fark
        // yalnızca menzil sayısı.
        AlreadyInRange,

        /// <summary>
        /// Menzile girmek için bildirilen hücreye YÜRÜMELİ. Bildirilen hücre
        /// bir ara durak değil, DURULACAK hücredir.
        /// </summary>
        // "ADIM" DEĞİL "DURAK" — ve ad bunu söylemek zorunda: bildirilen hücre
        // yolun ilk basamağı olsaydı çağıran onu her karede yeniden sormak ve
        // yürüyüşü kendi saymak zorunda kalırdı; oysa yürüyüşün sahibi zaten
        // MoveAction ve o, hedef hücreyi alıp yolu kendisi tüketiyor.
        MoveTo
    }
}
