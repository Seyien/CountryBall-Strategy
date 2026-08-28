using GridStrategy.Combat;
using GridStrategy.Core;

namespace GridStrategy.Unity
{
    /// <summary>
    /// Emirlerin tahtaya bakan penceresi: kim nerede duruyor, görsel hâlâ
    /// yürüyor mu, ve eylemin kendisi.
    /// </summary>
    // DÖRT ÜYE — VE BU SAYININ KENDİSİ BİR ÖLÇÜ. Yerini aldığı
    // <c>IPendingStrikeHost</c> dokuz üyeydi (üçü IBoardModeHost'tan). Bir
    // önceki devir belgesi ölçütü şöyle koymuştu: "god object bölündükçe bu
    // arayüzler daralmalı; daralmazlarsa pattern kozmetik kalmış demektir."
    //
    // NE YOK VE NEDEN: SelectedUnit yok — ve yokluğu bu turun ASIL kararı.
    // Eski emir "saldıran hâlâ seçili mi" diye soruyordu, yani seçimi bırakmak
    // emri iptal ediyordu. Operatör emrin seçimden BAĞIMSIZ yaşamasını istedi;
    // bağ, üyeyi silerek koptu — bir bayrakla değil.
    //
    // LeaveMode yok: emri defterden düşüren şey emrin kendi cevabı
    // (<see cref="OrderProgress"/>), dışarıdan bir çağrı değil.
    // Log yok: emrin yazacağı bir cümle yok; sonucun cümlesini tahta yazıyor.
    public interface IUnitOrderHost
    {
        /// <summary>
        /// Bu kimlik tahtada bir hücrede duruyor mu, duruyorsa nerede.
        /// </summary>
        // "TAHTADA MI" İLE "NEREDE" TEK ÜYE, iki değil: cevabı veren tek çağrı
        // (Battle.TryGetPosition) ikisini birden veriyor ve ayrı sorulsalardı
        // aynı tarama iki kez koşardı. HÜCRE HER KAREDE TAZE OKUNUYOR ve bu bir
        // onarım: eski emir hedefin YAZILDIĞI andaki hücresini saklıyordu, yani
        // kaçan bir hedefe atılan mermi hedefin ARTIK OLMADIĞI hücreye uçuyordu.
        bool TryGetCell(Unit unit, out int x, out int y);

        /// <summary>Bu kimliğin görseli şu anda yürüyor mu?</summary>
        // BEKLEME EKRANIN SAATİNE BAĞLI, tahtanın saatine değil: tahta hareketi
        // çoktan işledi, beklenen tek şey görselin hedefin yanına VARMASI.
        bool IsViewWalking(Unit unit);

        /// <summary>
        /// Vuruşu savaşa yaptırır, ekranı günceller ve ne olduğunu döner.
        /// </summary>
        // SONUÇ GERİ DÖNÜYOR ÇÜNKÜ EMRİN HAYATI ONA BAĞLI: "menzil dışı" emri
        // düşürür, "henüz vuramaz" düşürmez. Emir bu ayrımı KENDİ yazsaydı
        // bekleme kuralı ikinci kez yazılmış olurdu; oysa AttackAction cevabı
        // zaten üretiyor ve emir yalnızca okuyor.
        AttackOutcome Strike(Unit attacker, Unit target);

        /// <summary>Düşmüş dostu kaldırmayı savaşa yaptırır ve ekranı günceller.</summary>
        // SONUÇ DÖNMÜYOR, vuruşun tersine: kaldırma TEK SEFERLİKTİR ve
        // sonucundan bağımsız olarak biter. Dönseydi hiçbir emrin okumadığı bir
        // cevap doğardı.
        void Revive(Unit reviver, Unit target);
    }
}
