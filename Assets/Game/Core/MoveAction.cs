using System;

namespace GridStrategy.Core
{
    // ═══ ROL: KURAL (Policy) ═════════════════════════════════════════
    // kimlik : yok — static; iki Execute çağrısını ayıracak bir şey yoktur
    // hafıza : yok — aynı tahta ve aynı sayılar aynı sonucu verir; tuttuğu
    //          hiçbir şey yok, DEĞİŞTİRDİĞİ şey tahtanın kendisi
    // Unity  : gerekmez — tahta düz bir C# nesnesi, sahne kurmak gerekmez
    // karar  : AKIŞI yürütür — hangi kurala hangi SIRAYLA sorulacağını bilir;
    //          uzaklığı GridDistance ölçer, hücreyi UnitGrid yazar
    /// <summary>
    /// Hareket akışının tek sahibi.
    ///
    /// <see cref="GridDistance"/> uzaklığı ölçer, <see cref="UnitGrid"/> hücreyi
    /// tutar ve yazar. İkisi de birbirini TANIMAZ ve tanımamalıdır. Onları bir
    /// sıraya dizen tek yer burasıdır — ve saldırı tarafındaki AttackAction ile
    /// aynı deseni izler.
    ///
    /// Neyi BİLMEZ: sıranın kimde olduğunu, birimin bu turda daha önce hareket
    /// edip etmediğini, yolun üzerinde ne olduğunu (bugün adım adım yürünmüyor,
    /// hücreden hücreye ışınlanılıyor), sonucu kimin göstereceğini.
    ///
    /// Birimin DURUMUNU da bilmez — düşmüş bir birim bu akıştan geçer ve yer
    /// değiştirir. Bu bir eksik değil, bir SINIR: durum GridStrategy.Combat'ta
    /// yaşıyor ve GridStrategy.Core onu görmüyor. "Kim hareket edebilir"
    /// sorusunun sahibi oradaki MovementRules'tır; soruyu soran taraf da akışı
    /// bir üst katmandan yürüten BattleActions olur.
    /// </summary>
    public static class MoveAction
    {
        // REDDEDILEN - MoveAction.cs:70 yerine (bu sınıf hiç doğmaz, kural
        //              tahtanın kendisine taşınır):
        //     public MoveOutcome TryMoveUnit(int fromX, int fromY, int toX,
        //                                    int toY, int moveRange)
        // KIRILAN  : aynı tip aynı soruya iki farklı cevap verir — PlaceUnit dolu
        //            hücrenin üstüne şikâyetsiz yazar, TryMoveUnit reddeder.
        //            UnitGrid GridDistance'ı tanır -> ölçüm kuralı tahtaya girer
        //            Chebyshev/Manhattan kararı    -> tahtanın içinde donar
        //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
        // KAZANIRDI: hücreleri değiştirebilen TEK yol hareket olsaydı ve hareket
        //            kuralı hiç çeşitlenmeyecek olsaydı (tek tip birim, tek tip
        //            adım) — ayrı bir kural tipi o oyunda fazladan bir katman olurdu.
        // TEK CUMLE: Tahta neyin NEREDE olduğunu bilir, neyin OLABİLECEĞİNİ değil;
        //            ikisini tek tipe koymak tipi kendi kuralıyla çelişkiye sokar.

        /// <summary>
        /// Bir hareket denemesini yürütür, kabul edilirse tahtayı GÜNCELLER ve
        /// ne olduğunu döndürür.
        /// </summary>
        /// <param name="moveRange">
        /// Bu birimin bu turda kaç hücre uzağa gidebildiği. 0 geçerlidir ve
        /// "yerinden kımıldayamaz" demektir.
        /// </param>
        // HAREKET MENZİLİ NEREDEN GELİR: bugün parametre olarak.
        //
        // REDDEDILEN - MoveAction.cs:81 yerine (Unit'e alan eklenir, parametre
        //              silinir ve içeride unit.MoveRange okunur):
        //     public Unit(string name, int moveRange) { ... }
        //     public int MoveRange { get; }
        // KIRILAN  : Unit'in "karar vermez, ne yapabileceğini bilmez" rolü delinir;
        //            menzil, ikizi AttackProfile'dan ayrı bir yerde yaşamaya başlar.
        //            derleyici: her new Unit("...") ikinci argüman ister  .  test:
        //            UnitGridTests ve BoardAdapter derlenmez
        // KAZANIRDI: menzil birime göre GERÇEKTEN değişmeye başladığı gün — süvari
        //            üç, piyade bir, yaralı piyade sıfır; o gün de cevap Unit'e alan
        //            eklemek değil, AttackProfile'ın ikizi olan bir MoveProfile'dır.
        // TEK CUMLE: Bir sayı, onu KULLANAN kuralın yanında yaşar; kimliğin yanında
        //            değil — Unit kim olduğunu bilir, ne yapabileceğini bilmez.
        public static MoveOutcome Execute(
            UnitGrid board,
            Unit unit,
            int fromX,
            int fromY,
            int toX,
            int toY,
            int moveRange)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

            // moveRange == 0 bilerek GEÇERLİ. AttackProfile menzil için en az 1
            // ister çünkü hiçbir hücreye ulaşamayan bir SALDIRI anlamsızdır;
            // hiçbir hücreye gidemeyen bir BİRİM ise anlamlıdır — kök salmış,
            // sersemlemiş, kuşatılmış. Asimetri kasıtlı.
            if (moveRange < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(moveRange), moveRange, "Move range cannot be negative.");
            }

            // Kaynak hücrede gerçekten O birim mi duruyor? Uyuşmazlık bir OYUN
            // sonucu değil, bir ÇAĞIRAN hatasıdır: tahta konumun tek sahibidir,
            // dolayısıyla "birim orada değil" demek "benim kaydım tahtanınkiyle
            // ayrışmış" demektir. PlaceUnit'in gürültülü felsefesi burada da
            // geçerli.
            //
            // REDDEDILEN - MoveAction.cs:118 yerine (MoveOutcome'a bir değer
            //              daha eklenir ve buradan o dönülür):
            //     return MoveOutcome.RejectedUnitNotAtSource;
            // KIRILAN  : bir programcı hatası oyun sonucu kılığına girer; MoveOutcome
            //            artık iki ayrı şeyi birden anlatır — oyunda olanı ve kodun
            //            bozuk olduğunu. Gerekçenin tamamı UnitGrid.PlaceUnit'te.
            //            yeni dal sessizce yutulur -> Play'de yanlış birim ışınlanır
            //            derleyici: hiçbir şey der  .  test: o dal hiç sınanmaz
            // KAZANIRDI: hareket emirleri AĞDAN ya da bir tekrar (replay) kaydından
            //            gelseydi — orada ayrışma beklenen bir durumdur ve çökmek
            //            yerine reddedip senkronizasyon istemek doğru davranıştır.
            // TEK CUMLE: Ret sebebi, çağıranın YAPABİLECEĞİ bir şeyi göstermelidir;
            //            "kodun bozuk" bunlardan biri değildir.
            if (!board.TryGetUnit(fromX, fromY, out Unit standing)
                || !ReferenceEquals(standing, unit))
            {
                throw new ArgumentException(
                    "The unit is not standing on the given source cell.", nameof(unit));
            }

            // SIRA BİR KARARDIR: önce tahta sınırı, sonra menzil, en sonda
            // hücrenin doluluğu.
            //
            // Tahta sınırının başa gelmesi AttackAction'daki gerekçenin aynısı:
            // 3x5'lik bir tahtada (9,9) hücresi ne yaklaşarak ne bekleyerek
            // geçerli olur. Diğer iki sebep düzeltilse bile bu sebep AYAKTA
            // KALIR, dolayısıyla doğru cevap odur.
            //
            // Menzil ile doluluk arasında ise o ölçüt SUSAR: menzili düzeltirsen
            // doluluk sürer, doluluğu düzeltirsen menzil sürer — ikisi de ayakta
            // kalır. Tie-break başka yerden geliyor: menzil sorusu çağıranın
            // zaten verdiği sayılara bakan saf bir aritmetiktir, doluluk sorusu
            // ise TAHTAYI okur. Ucuz ve yerel olan önce sorulur; ayrıca birim
            // ulaşamadığı bir hücrenin içinde kimin durduğunu hiç öğrenmemiş
            // olur.
            //
            // REDDEDILEN - MoveAction.cs:153 yerine (son iki blok yer değiştirir):
            //     if (board.TryGetUnit(toX, toY, out Unit occupant)) ...   // önce doluluk
            // KIRILAN  : menzili 1 olan birim, tahtanın öbür ucundaki dolu hücre
            //            için "orada biri var" cevabı alır.
            //            yol bulucu  -> o hücreyi kalıcı engelli işaretler
            //            sis gelince -> görülemeyen bir hücrenin dolu olduğu sızar
            //            derleyici: hiçbir şey der  .  test: _PrefersOutOfRange kırmızı
            // KAZANIRDI: menzilin pratikte hiç kısıtlamadığı bir aşamada — serbest
            //            yerleştirme turunda moveRange tahtadan büyüktür ve tek
            //            anlamlı soru doluluktur.
            // TEK CUMLE: Önce, diğerleri düzeltilse bile AYAKTA KALAN sebep sorulur;
            //            eşitlikte ucuz ve yerel olan öne geçer.
            if (!board.IsInsideGrid(toX, toY))
            {
                return MoveOutcome.RejectedInvalidDestination;
            }

            if (GridDistance.Between(fromX, fromY, toX, toY) > moveRange)
            {
                return MoveOutcome.RejectedOutOfRange;
            }

            // Doluluk sorusu "orada BİRİ var mı" değil, "orada BAŞKASI var mı".
            //
            // REDDEDILEN - MoveAction.cs:177 yerine:
            //     if (board.TryGetUnit(toX, toY, out Unit _))   // "orada biri var mı"
            // KIRILAN  : birim kendi hücresine taşınmak istediğinde KENDİSİ
            //            tarafından engellenmiş sayılır; seçili birimin üstüne
            //            ikinci kez tıklamak sık bir durumdur ve yapay zekâ o
            //            hücreyi kalıcı olarak engelli işaretler.
            //            derleyici: hiçbir şey der  .  test: _MovingToItsOwnCell kırmızı
            // KAZANIRDI: "yerinde kalmak" ayrı bir oyun eylemi olsaydı (bekle, nöbet
            //            tut) ve kendi bedeli olsaydı — o gün aynı hücreye hareket
            //            bir hata olurdu ve reddetmek çağıranı doğru komuta yollardı.
            // TEK CUMLE: "Dolu mu" sorusu her zaman "BAŞKASI var mı" diye sorulur;
            //            kendi kuralına takılan bir birim o kuralın kurbanıdır.
            if (board.TryGetUnit(toX, toY, out Unit occupant)
                && !ReferenceEquals(occupant, unit))
            {
                return MoveOutcome.RejectedCellOccupied;
            }

            // Tahtayı tek bir çağrıyla değiştir. Buradaki RemoveUnit + PlaceUnit
            // ikilisi UnitGrid.MoveUnit'in var olma sebebiydi; o gerekçe orada
            // yazılı.
            board.MoveUnit(fromX, fromY, toX, toY);

            return MoveOutcome.Moved;
        }

        /// <summary>
        /// Aynı hareket akışı, menzili çıplak bir sayı yerine
        /// <see cref="MoveProfile"/>'dan okuyarak. Kuralın kendisi burada
        /// TEKRARLANMIYOR; üstteki sürüme devrediliyor.
        /// </summary>
        /// <param name="profile">
        /// Bu birimin hareket tanımı. <c>null</c> geçilemez: menzilsiz bir
        /// hareket denemesi bir oyun sonucu değil, bir çağıran hatasıdır —
        /// AttackResolver'ın profil için koyduğu koruma ile aynı gerekçe.
        /// </param>
        // EŞİK — int alan sürüm ne zaman silinir: son int çağıranı gittiği gün.
        // ÜRETİMDE o gün geldi: BattleActions.Move artık profil sürümünü
        // çağırıyor ve int sürümün kalan bütün çağıranları testlerdir. Sürüm
        // yine de duruyor, çünkü MoveActionTests'teki
        // Execute_WithProfile_MatchesTheIntOverload ancak iki imza yan yana
        // durursa yazılabilir. İKİSİ DE aynı kuralı yürütür — biri diğerini
        // çağırıyor.
        //
        // REDDEDILEN - MoveAction.cs:236 yerine (aşırı yükleme eklenmez, int
        //              alan sürümün imzası doğrudan değiştirilir):
        //     public static MoveOutcome Execute(UnitGrid board, Unit unit,
        //         int fromX, int fromY, int toX, int toY, MoveProfile profile)
        // KIRILAN  : moveRange geçen HER çağıran tek seferde düşer; bugün bunların
        //            hepsi MoveActionTests ile BattleActionsTests'in içinde. Üstelik
        //            "profil sürümü kuralı KOPYALAMIYOR" sınavı, iki imza yan yana
        //            durmadan yazılamayacağı için tamamen kalkar.
        //            derleyici: iki test assembly'si derlenmez  .  test: koşamaz bile
        // KAZANIRDI: iki sürümün uzun süre yan yana yaşayacağı bir dünyada — o gün
        //            "hangisi gerçek kural" sorusu belirsizleşirdi ve tek imza bu
        //            belirsizliği baştan keserdi.
        // TEK CUMLE: Aşırı yükleme, imzayı değiştirmenin bedelini ÖDEMEDEN aynı
        //            sonuca götüren yoldur; ömrünü yukarıdaki EŞİK notu sınırlar.
        //
        // REDDEDILEN - MoveAction.cs:236 yerine (durum kuralı akışın içine
        //              alınır, profil yerine birimin durumu sorulur):
        //     if (!MovementRules.CanMove(state)) return MoveOutcome.Rejected...;
        // KIRILAN  : bu satır DERLENMEZ; Core'un asmdef'i Combat'a referans vermiyor.
        //            referans eklenir -> tahta kodu hasarı ve yaşam döngüsünü görür
        //            kural yine girse -> MoveOutcome'da bu ret için değer yok, biri
        //                                ödünç alınır ve çağırana yalan söylenir
        //            derleyici: tip bulunamadı der  .  test: Core derlenmez, koşamaz
        // KAZANIRDI: durum Core'a taşınsaydı — "canlı/düşmüş/ölü" bir tahta kavramı
        //            sayılsaydı; o gün Combat'ın ayrı assembly olma sebebi kalmazdı.
        // TEK CUMLE: Assembly sınırı bir yavaşlatıcı değil bir KISIT'tır; doğru cevap
        //            kuralı içeri almak değil, akışı bir kat YUKARIDAN yürütmektir.
        public static MoveOutcome Execute(
            UnitGrid board,
            Unit unit,
            int fromX,
            int fromY,
            int toX,
            int toY,
            MoveProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            return Execute(board, unit, fromX, fromY, toX, toY, profile.Range);
        }
    }
}
