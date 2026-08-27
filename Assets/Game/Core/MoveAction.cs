using System;
using System.Collections.Generic;

namespace GridStrategy.Core
{
    // ═══ ROL: AKIŞ SAHİBİ (transaction script) ═══════════════════════
    // kimlik : yok — static; iki Execute çağrısını ayıracak bir şey yoktur
    // hafıza : yok — ama ölçüsü "aynı tahta ve aynı sayılar aynı sonucu
    //          verir" DEĞİL, çünkü vermiyor: 2x2'lik bir tahtada
    //          Execute(board, unit, 0, 0, 1, 0, moveRange: 1) çağrısını arka
    //          arkaya iki kez yap; birincisi MoveOutcome.Moved döner, ikincisi
    //          ArgumentException fırlatır — birim artık (0, 0) hücresinde
    //          durmuyor. Ölçü şu: farkı doğuran yer UnitGrid, burası değil —
    //          tahtayı sıfırdan kurup aynı çağrıyı tekrarla, ilk cevabı
    //          yeniden alırsın. Bu tip static'tir ve tek bir alanı yoktur;
    //          DEĞİŞTİRDİĞİ şey tahtanın kendisidir
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
    /// edip etmediğini, sonucu kimin göstereceğini.
    ///
    /// TAHTA IŞINLANIR, EKRAN YÜRÜR — ve bu bir kusur değil, bir iş bölümü:
    /// burada birim tek adımda yeni hücresine yazılır, çünkü kurallar sorulurken
    /// tahtanın yarı yolda olması anlamsız olurdu. Oyuncunun gördüğü yürüyüşü
    /// <see cref="PathFinder"/>'ın çıkardığı yol ile ekran katmanı üretir.
    ///
    /// Birimin DURUMUNU da bilmez — düşmüş bir birim bu akıştan geçer ve yer
    /// değiştirir. Bu bir eksik değil, bir SINIR: durum GridStrategy.Combat'ta
    /// yaşıyor ve GridStrategy.Core onu görmüyor. "Kim hareket edebilir"
    /// sorusunun sahibi oradaki MovementRules'tır; soruyu soran taraf da akışı
    /// bir üst katmandan yürüten BattleActions olur.
    ///
    /// GEREKÇELER: Docs/deep/kod/Core/MoveAction.md
    /// </summary>
    // ÖĞRENME: Docs/ogrenme/01-koda-gomulu-desenler.md — bu tipin deseninin adı
    // AKIŞ SAHİBİ (transaction script) ve ██ bir Command DEĞİL ██: saklanacak bir
    // nesne yok, dolayısıyla geri alma da yeniden oynatma da yok.
    public static class MoveAction
    {
        // KURAL TAHTAYA TAŞINMAZ: tahta kendiyle çelişirdi. Kural UnitGrid'in
        // içine girseydi aynı tip "dolu hücreye yazılabilir mi" sorusuna iki
        // zıt cevap verirdi — PlaceUnit şikâyetsiz yazar, TryMoveUnit
        // reddeder. Taşınan yalnız kural da olmazdı: tahta GridDistance'ı ve
        // MoveOutcome'ı tanımak zorunda kalır, Chebyshev kararı içinde donardı.
        // → MoveAction.md#moveaction-tip

        // MENZİL KİMLİĞİN YANINDA DEĞİL, KURALIN YANINDA YAŞAR: sayı bugün
        // parametre olarak geliyor. Unit'e alan olarak eklemek onun "kim
        // olduğunu bilir, ne yapabileceğini bilmez" rolünü delerdi ve menzil,
        // ikizi AttackProfile'dan ayrı bir yerde yaşamaya başlardı. Paylaşılan
        // tanım MoveProfile olarak doğdu; onu ikinci aşırı yükleme okuyor.
        // → MoveAction.md#executeint-moverange
        //
        // ÖDÜNÇ ALINAN — nameof ve bu metodun fırlattığı üç istisna tipi
        // (ArgumentNullException, ArgumentOutOfRangeException, düz
        // ArgumentException): üçü de OYUNCUYA değil ÇAĞIRANA konuşur ve
        // seçim ölçüsü, istisnanın orta argümanını doldurabilmektir.
        // DİL: Docs/deep/dil/03-hata-bildirme-ve-dogrulama.md
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
            // sonucu değil ÇAĞIRAN hatasıdır: tahta konumun tek sahibidir,
            // dolayısıyla "birim orada değil" demek "benim kaydım tahtanınkiyle
            // ayrışmış" demektir. Bu yüzden cevap MoveOutcome kanalından değil
            // exception kanalından gider — ret sebebi, çağıranın YAPABİLECEĞİ
            // bir şeyi göstermelidir ve "kodun bozuk" bunlardan biri değildir.
            // → MoveAction.md#executeint-moverange
            if (!board.TryGetUnit(fromX, fromY, out Unit standing)
                || !ReferenceEquals(standing, unit))
            {
                throw new ArgumentException(
                    "The unit is not standing on the given source cell.", nameof(unit));
            }

            // SIRA BİR KARARDIR: SINIR ► MENZİL ► DOLULUK. Üç ret sebebi aynı
            // anda doğru olabilir; dönen tek bir tanedir. Önce, diğerleri
            // düzeltilse bile AYAKTA KALAN sebep sorulur — tahta dışı bir hücre
            // ne yaklaşarak ne bekleyerek geçerli olur. Menzil ile doluluk
            // arasında o ölçüt susar; orada ucuz ve YEREL olan öne geçiyor,
            // çünkü doluluk sorusu tahtayı okur ve birim ulaşamadığı bir
            // hücrenin içeriğini hiç öğrenmemiş olur.
            // → MoveAction.md#executeint-moverange
            if (!board.IsInsideGrid(toX, toY))
            {
                return MoveOutcome.RejectedInvalidDestination;
            }

            if (GridDistance.Between(fromX, fromY, toX, toY) > moveRange)
            {
                return MoveOutcome.RejectedOutOfRange;
            }

            // "BİRİ" DEĞİL "BAŞKASI": ReferenceEquals bir optimizasyon değil.
            // Kimlik karşılaştırması olmasa tahta, birimin kendisini bir engel
            // sayardı ve seçili birime ikinci kez tıklamak ret üretirdi. Aynı
            // metotta ReferenceEquals iki karşıt şey istiyor: kaynakta kimlik
            // ŞART (yoksa fırlat), hedefte kimlik MUAF (varsa devam).
            // → MoveAction.md#executeint-moverange
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
        ///
        /// GEREKÇELER: Docs/deep/kod/Core/MoveAction.md
        /// </summary>
        /// <param name="profile">
        /// Bu birimin hareket tanımı. <c>null</c> geçilemez: menzilsiz bir
        /// hareket denemesi bir oyun sonucu değil, bir çağıran hatasıdır —
        /// AttackResolver'ın profil için koyduğu koruma ile aynı gerekçe.
        /// </param>
        // AŞIRI YÜKLEME BİR DİKİŞ YERİDİR: kural yalnız int alan sürümde
        // yaşıyor, bu sürüm ona devrediyor. EŞİK — int sürüm son çağıranı
        // gittiği gün silinir; üretimde o gün geldi, ayakta tutan tek şey iki
        // imzayı yan yana isteyen karşılaştırma testi.
        //
        // DURUM KURALI İÇERİ ALINMAZ: "kim hareket edebilir" sorusu Combat'ta
        // ve Core onu GÖRMEZ — o satır derlenmez bile. Kısıtı çözen şey bir
        // referans eklemek değil, akışı bir kat YUKARIDAN (BattleActions)
        // yürütmek. Reddedilen şey dışarıdan bilgi almak değil, YANLIŞ KUTUDAN
        // bilgi almak: MoveProfile Core'da doğduğu için parametre olabiliyor.
        // → MoveAction.md#executemoveprofile-profile
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

        /// <summary>
        /// Menzil SORMAYAN hareket: birim, yolu varsa tahtanın herhangi bir boş
        /// hücresine gider.
        ///
        /// OYUNDA NE İŞE YARAR: oyuncu haritada uzak bir noktaya tıklar ve birim
        /// aradaki hücrelere basarak oraya yürür. "Şu kadar hücre uzağa
        /// gidebilirsin" kısıtının yerini ULAŞILABİLİRLİK aldı — engellerle
        /// çevrili bir hücreye gidilemez, uzak ama açık bir hücreye gidilir.
        ///
        /// Menzilli sürümü SİLMİYORUZ: süvari/piyade gibi tur bazlı menzil
        /// isteyen bir mod geri gelirse kural orada duruyor. Tahta bugün bu
        /// sürümü çağırıyor.
        /// </summary>
        /// <param name="path">
        /// Birimin sırayla basacağı hücreler; çıkış hücresi listede YOKTUR.
        /// Ekran, yürüyüşü bu listeye bakarak canlandırır. Ret hâlinde boştur.
        /// </param>
        // TAHTA ANINDA GÜNCELLENİR, EKRAN GECİKMELİ TAKİP EDER: birim buradan
        // çıktığında hedef hücrede zaten duruyor. Yürüyüşü hücre hücre işlemek
        // (referans oyunun yaptığı) çekirdeği zamana bağlardı; o yol seçilmedi
        // çünkü kurallar o an sorulduğunda tahtanın yarı yolda olması gerekirdi.
        public static MoveOutcome ExecuteAlongPath(
            UnitGrid board,
            Unit unit,
            int fromX,
            int fromY,
            int toX,
            int toY,
            out List<GridStep> path)
        {
            path = new List<GridStep>();

            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

            // Kaynak kaydı ile tahta ayrışmışsa bu bir ÇAĞIRAN hatasıdır —
            // menzilli sürümdeki ile birebir aynı gerekçe.
            if (!board.TryGetUnit(fromX, fromY, out Unit standing)
                || !ReferenceEquals(standing, unit))
            {
                throw new ArgumentException(
                    "The unit is not standing on the given source cell.", nameof(unit));
            }

            // SIRA BİR KARARDIR: SINIR ► DOLULUK ► YOL. Önce, düzeltilse bile
            // ayakta kalan sebep sorulur; en pahalı soru olan arama en sona
            // bırakılır.
            if (!board.IsInsideGrid(toX, toY))
            {
                return MoveOutcome.RejectedInvalidDestination;
            }

            if (board.TryGetUnit(toX, toY, out Unit occupant)
                && !ReferenceEquals(occupant, unit))
            {
                return MoveOutcome.RejectedCellOccupied;
            }

            if (!PathFinder.TryFindPath(board, unit, fromX, fromY, toX, toY, out path))
            {
                return MoveOutcome.RejectedUnreachable;
            }

            board.MoveUnit(fromX, fromY, toX, toY);

            return MoveOutcome.Moved;
        }
    }
}
