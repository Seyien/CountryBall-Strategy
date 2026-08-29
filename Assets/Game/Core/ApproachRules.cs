using System;
using System.Collections.Generic;

namespace GridStrategy.Core
{
    // ═══ ROL: KURAL (Policy) ═════════════════════════════════════════
    // kimlik : yok — static; iki çağrıyı ayıracak bir şey yoktur
    // hafıza : yok — ölçüsü şu: aynı tahtada aynı iki hücre ve aynı menzil
    //          sayısıyla yapılan iki Plan çağrısı aynı cevabı verir. Kaçıncı
    //          kare olduğunu, kimin kaç kez sorduğunu, birimin kaç adımdır
    //          kovaladığını bu tip SAYMAZ; sayan biri olacaksa o çağırandır
    // Unity  : gerekmez — girdi bir tahta, bir kimlik ve üç sayı; sınamak
    //          için ne sahne ne kare gerekir
    // karar  : HÜCRE söyler; ne yürütür ne vurur ne emir yazar
    /// <summary>
    /// "Hedefe kendi menzilimle vurabilmek için hangi hücrede durmalıyım?"
    /// sorusunun tek sahibi.
    ///
    /// OYUNDA NE İŞE YARAR: saldırıya uğrayan taraf seyirci kalmıyor. Karşılık
    /// verirken kendi menziline girene kadar YÜRÜYOR — yakın dövüşçü
    /// saldırganın yanına gidiyor, okçu üç hücre öteden durup atıyor. İkisi de
    /// bu tipe aynı soruyu soruyor.
    ///
    /// <see cref="GridDistance"/> "kaç adım uzakta" der, <see cref="PathFinder"/>
    /// "oraya yürünür mü" der; ikisinin arasında kalan "PEKİ NEREYE" sorusunun
    /// sahibi yoktu ve sahipsiz kural sessizce çağıranın içinde doğardı.
    ///
    /// Neyi BİLMEZ: kimin kime saldırdığını, saldıranın ayakta olup olmadığını,
    /// hedefin geçerli hedef olup olmadığını, menzil sayısının nereden geldiğini.
    /// Bir yapının hiç yürüyemediğini de bilmez — o soru bu tipin DIŞINDA,
    /// çağıranın kendi katmanında cevaplanıyor ve gerekçesi belgede yazılı.
    ///
    /// DERİN ANLATIM: Docs/deep/konular/11-karsilik-verme-ve-menzil.md
    /// </summary>
    // ██ BURASI Core, Combat DEĞİL — VE BUNU BİR TERCİH DEĞİL BİR ASMDEF SEÇTİ ██
    // Ölçüldü: GridStrategy.Combat.asmdef'in references dizisi BOŞ, yani Combat
    // katmanı GridStrategy.Core'u GÖREMİYOR ve PathFinder orada yaşıyor. Kural
    // Combat'a konsaydı yol soramaz, yol sormadan da hiçbir hücre söyleyemezdi.
    // Ters yön de kapalı: Core da Combat'ı görmüyor, ama bu kuralın Combat'tan
    // İSTEDİĞİ hiçbir tip yok — menzil buraya AttackProfile olarak değil bir
    // int olarak giriyor. Yani duvarın iki yüzünden yalnız biri fatura kesiyor
    // ve kural o faturayı ödemeyen tarafta duruyor.
    //
    // MovementRules'un kendi yorumu aynı ölçütü ters yönde uyguluyor ve bu iki
    // dosya birlikte okunmalı: "akış tahtayı, kural savaşı tanır" diyerek
    // hareket KURALINI Combat'a koyuyor, çünkü o kural UnitState soruyor. Bu
    // kural ise durum sormuyor, TAHTA soruyor — bu yüzden aynı ölçüt onu
    // Core'da tutuyor.
    public static class ApproachRules
    {
        /// <summary>
        /// Menzile girmek için durulacak hücreyi arar. Tahtaya HİÇ DOKUNMAZ.
        /// </summary>
        /// <param name="board">Kimin nerede durduğunu bilen tahta.</param>
        /// <param name="mover">
        /// Yürüyecek kimlik. Kendi hücresi engel sayılmaz; bu ayrımı
        /// <see cref="PathFinder"/> yapıyor ve burada tekrarlanmıyor.
        /// </param>
        /// <param name="targetX">Hedefin sütunu.</param>
        /// <param name="targetY">Hedefin satırı.</param>
        /// <param name="range">
        /// Kaç hücre uzağa ulaşılabildiği. NESNE DEĞİL SAYI: burada bir saldırı
        /// tanımı değil, o tanımın taşıdığı tek sayı isteniyor. Tanımın kendisi
        /// istenseydi bu dosya Combat katmanına bağlanır ve o katman Core'u
        /// görmediği için kural derlenemezdi.
        /// </param>
        /// <param name="cellX">Durulacak hücrenin sütunu.</param>
        /// <param name="cellY">Durulacak hücrenin satırı.</param>
        /// <returns>
        /// Cevabın cinsi. İki KABUL değerinde de bildirilen hücre "vurabilmek
        /// için durulması gereken hücre"dir; zaten menzildeyken o hücre birimin
        /// kendi hücresidir. İki RET değerinde bildirilen hücre anlamsızdır.
        /// </returns>
        // ═══ TEK KURAL, İKİ SAYI DEĞİL ═════════════════════════════════════
        // Yakın dövüşçü ile okçu bu üyeyi AYNI biçimde çağırır; aralarındaki tek
        // fark `range` argümanının değeridir. Menzili 1 olan birim hedefin
        // bitişiğine kadar yürür, menzili 3 olan birim üç hücre ötede durur ve
        // ikisini de aşağıdaki tek arama üretir.
        //
        // REDDEDILEN - tür başına dal: yakın dövüş ve menzil için ayrı kurallar.
        //     if (isMelee)
        //     {
        //         return AdjacentCell(board, mover, targetX, targetY, out cellX, out cellY);
        //     }
        //
        //     return FiringCell(board, mover, targetX, targetY, range, out cellX, out cellY);
        // KIRILAN: "yakın dövüşçü" diye bir tip yok — ölçüldü, ayıran tek şey
        // AttackProfile.Range sayısı ve o sayı bir .asset dosyasından geliyor.
        // Dal, var olmayan bir türü kodda VARMIŞ gibi gösterir ve menzili 2 olan
        // ilk birim doğduğu gün hangi dala düşeceğini kimse söyleyemez; üstelik
        // iki dal zamanla ayrışır ve çapraz komşuluk yalnız birinde düzeltilir.
        // KAZANIRDI: yakın dövüş menzile DEĞİL başka bir ölçüte bağlansaydı —
        // örneğin hedefe temas etmek zorunda olsaydı — iki dal iki ayrı soru
        // sorardı ve o gün bölünme hak edilirdi.
        // TEK CUMLE: iki davranışın arasındaki fark bir SAYIYSA, kodda bir DAL
        // değil bir parametre olmalıdır.
        public static ApproachOutcome Plan(
            UnitGrid board,
            Unit mover,
            int targetX,
            int targetY,
            int range,
            out int cellX,
            out int cellY)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (mover == null)
            {
                throw new ArgumentNullException(nameof(mover));
            }

            // AYNI KELEPÇE, AYNI CÜMLE: AttackProfile kurucusu da menzili 1'in
            // altında kabul etmiyor. Burada tekrar sorulmasının sebebi kopya
            // değil sınır: bu üye bir saldırı tanımı görmüyor, elinde çıplak
            // bir sayı var ve sıfır menzil arama döngüsünü sessizce boş
            // gezdirirdi.
            if (range < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(range), range, "Range must be at least 1.");
            }

            cellX = 0;
            cellY = 0;

            // HÜCRE TAHTADAN TAZE OKUNUYOR, ÇAĞIRANDAN ALINMIYOR. Bir fromX /
            // fromY parametresi eklemek konumun İKİNCİ bir yazarını doğururdu
            // ve o kusur bu projede bir kez ödendi: eski bekleyen vuruş hedefin
            // yazıldığı andaki hücresini saklıyordu, mermi hedefin artık
            // olmadığı yere uçuyordu. Tek yazar tahtadır.
            if (!board.TryGetPosition(mover, out int fromX, out int fromY))
            {
                return ApproachOutcome.RejectedOffBoard;
            }

            if (!board.IsInsideGrid(targetX, targetY))
            {
                return ApproachOutcome.RejectedOffBoard;
            }

            // ZATEN MENZİLDEYİM DALI EN BAŞTA, ve sırası bir karar: aşağıdaki
            // arama çalıştırılsaydı hedefin çevresindeki hücreleri gezer, boş
            // birini bulur ve ayakta duran okçuyu boşuna bir hücre yürütürdü.
            // Bildirilen hücre birimin kendi hücresi — böylece iki KABUL
            // değerinde de aynı cümle geçerli kalıyor: "vurmak için burada dur".
            if (GridDistance.Between(fromX, fromY, targetX, targetY) <= range)
            {
                cellX = fromX;
                cellY = fromY;
                return ApproachOutcome.AlreadyInRange;
            }

            return SearchNearestFiringCell(
                board, mover, fromX, fromY, targetX, targetY, range, out cellX, out cellY);
        }

        // ADAYLAR HEDEFİN ÇEVRESİNDE TANIMLANIR, YÜRÜYENİN ÇEVRESİNDE DEĞİL:
        // soru "nereye gidebilirim" değil, "nereden vurabilirim". Aday kümesi
        // bu yüzden hedefin menzil karesidir ve içinden bana EN YAKIN olanı
        // seçiliyor.
        //
        // DIŞ DÖNGÜ UZAKLIĞIN KENDİSİ, BİR LİSTE DEĞİL: adayları toplayıp
        // sıralamak her çağrıda bir liste tahsis ederdi ve bu üye her karede
        // çağrılıyor. Üçgen eşitsizliği dış döngünün sınırlarını veriyor —
        // Chebyshev bir metrik olduğu için hiçbir aday [uzaklık - menzil,
        // uzaklık + menzil] aralığının dışına düşemez.
        private static ApproachOutcome SearchNearestFiringCell(
            UnitGrid board,
            Unit mover,
            int fromX,
            int fromY,
            int targetX,
            int targetY,
            int range,
            out int cellX,
            out int cellY)
        {
            cellX = 0;
            cellY = 0;

            int distance = GridDistance.Between(fromX, fromY, targetX, targetY);
            int nearestPossible = distance - range;
            int farthestPossible = distance + range;

            // ██ ÖLÇEK BORCU — YAZILI, ONARILMADI ██
            // Bu arama, en kötü hâlde menzil karesindeki her boş hücre için bir
            // A* koşturuyor ve PathFinder'ın kendi maliyeti tahtanın TAMAMI
            // kadar. Menzil 1'de aday sayısı 8, menzil 3'te 48; yani kötü
            // senaryoda çağrı başına 48 tam tahta taraması.
            //
            // TAVAN: menzil ≤ 3 ve tahta ≤ ~1000 hücre. Bu tavan PathFinder'ın
            // kendi yazılı tavanının ÜSTÜNE biniyor, onun yerine geçmiyor.
            // Yeniden açma: menzili 4'ü aşan bir birim doğduğu gün, ya da
            // Profiler'da karşılık veren birim sayısı arttıkça kare başına GC
            // sıçraması görüldüğü gün. O gün doğru onarım aday başına A*
            // koşturmayı bırakıp yürüyenden TEK bir yayılma yapmak ve ilk
            // menzile giren hücrede durmaktır.
            for (int step = nearestPossible; step <= farthestPossible; step++)
            {
                for (int offsetY = -range; offsetY <= range; offsetY++)
                {
                    for (int offsetX = -range; offsetX <= range; offsetX++)
                    {
                        int candidateX = targetX + offsetX;
                        int candidateY = targetY + offsetY;

                        if (GridDistance.Between(candidateX, candidateY, fromX, fromY) != step)
                        {
                            continue;
                        }

                        if (!IsFreeCell(board, candidateX, candidateY))
                        {
                            continue;
                        }

                        // YOL VARLIĞI SORULUYOR, YOLUN KENDİSİ DEĞİL: dönen
                        // liste burada okunmuyor çünkü çağıran hedef hücreyi
                        // alıp yürüyüşü kendi başlatıyor. Listeyi buradan
                        // dışarı vermek, aynı yolun ikinci bir kopyasını
                        // yaratır ve tahta arada değiştiğinde o kopya bayatlar.
                        if (PathFinder.TryFindPath(
                                board, mover, fromX, fromY, candidateX, candidateY, out List<GridStep> _))
                        {
                            cellX = candidateX;
                            cellY = candidateY;
                            return ApproachOutcome.MoveTo;
                        }
                    }
                }
            }

            return ApproachOutcome.RejectedUnreachable;
        }

        // HEDEFİN KENDİ HÜCRESİ DE BU KAPIDAN ELENİYOR ve ayrı bir kontrol
        // yazılmadı: hedef kendi hücresinde duruyor, yani hücre dolu. Ayrıca
        // yazılsaydı "hedef nerede" sorusunun ikinci bir cevabı doğar ve hedef
        // tahtadan kalktığında ikisi ayrışırdı.
        private static bool IsFreeCell(UnitGrid board, int x, int y)
        {
            return board.IsInsideGrid(x, y) && !board.TryGetUnit(x, y, out Unit _);
        }

        // TASMA BU TİPTE YOK VE YOKLUĞU BİR KARAR: kovalamayı kaç adım sonra
        // bırakacağını bu kural söyleyemez, çünkü kaç adım yürüdüğünü bilen tek
        // yer emrin kendisi. Buraya bir adım sayacı konsaydı kural hafıza tutar
        // ve künyedeki "hafıza: yok" satırı yalan olurdu.
        // TETİKLEYİCİ: bir birimin kovalamayı bırakması OYUNDA istendiği gün —
        // ve o gün sayacın evi emir tipidir, burası değil.
    }
}
