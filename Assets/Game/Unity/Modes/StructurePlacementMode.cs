using GridStrategy.Core;

namespace GridStrategy.Unity
{
    //   ═══ YAPI YERLEŞTİRME — klavyeli kip ══════════════════════════
    //
    //     BOŞTA ──B tuşu (seçili birim + hayalet şart)──► YERLEŞTİRME
    //
    //     Gir()  : hayalet açılır, jest sıfırlanır, hayalet SERBEST
    //     Ilerlet: hayalet imlecin hücresine oturur, jest beslenir
    //              sürükle-bırak ──► koy      tıkla-bırak ──► taşı
    //              taşırken tıkla ──► koy     iptal tuşu  ──► çık
    //     Cik()  : hayalet gizlenir, jest sıfırlanır, taşıma düşer
    //
    //     YERLEŞTİRME ──iptal | yapı kondu | seçim savaştan düştü──► BOŞTA

    /// <summary>
    /// Oyuncunun bina koyduğu kip: hayalet fareyi takip eder, bırakma şekli
    /// yapının nereye konacağını söyler.
    ///
    /// OYUNDA NE İŞE YARAR: bu kip açıkken tıklamak birim seçmez, bina koyar.
    /// Girdinin anlamını baştan sona değiştiren şey tam olarak budur.
    /// </summary>
    // ██ ÇAKIŞMA KARARI: KİP HAYALETİ SAHİPLENİR, SÜRÜKLEME GERİ ÇEKİLİR ██
    // Aynı hayaleti iki taraf yazıyor: bu kip Ilerlet içinde HER KARE
    // konumluyor, tahtanın SetPlacementGhost üyesi ise yalnız sürükleme
    // olaylarında. Ölçülen zarar bir titreme DEĞİL, sessiz bir körlüktür: kip
    // açıkken biten bir sürükleme hayaleti kapatıyor, kip ise onu yalnızca
    // KONUMLUYOR ve bir daha hiç açmıyordu — oyuncu kipte kalıyor, hayaleti
    // göremiyor ve sonraki tıklaması görünmeyen bir yapı koyuyordu.
    //
    // ERKEN ÇIKIŞIN YÖNÜ ÖLÇÜYLE SEÇİLDİ: kip KALICI bir durumdur, oyuncu ona
    // bir tuşa basarak girer ve bir tuşa basarak çıkar; sürükleme ise parmak
    // kalkınca biten geçici bir jesttir. Kalıcı olanın önizlemesini geçici
    // olana bozdurmak, oyuncunun kendi verdiği kararı görünmez kılar. KAYBEDİLEN
    // ŞEY YOK: kip açıkken hayalet zaten her karede imlecin hücresine konuyor,
    // geri çekilen tek şey ikinci bir yazar.
    //
    // İKİ SEÇENEK REDDEDİLDİ. Birincisi "kipi bırak, titreme zararsız" idi ve
    // üstteki ölçüm onu çürüttü; zarar titreme değil sessiz körlüktür. İkincisi
    // tahtanın, üretim katmanının IsPlacing üyesine bakıp erken çıkmasıydı: bu,
    // tahtanın o katmanı ADIYLA tanıması demekti ve sözleşmenin tek yönlü olma
    // sebebini tam o satırda çökertirdi.
    //
    // GERİYE KALAN VE SESSİZ OLMAYAN ARTIK: kip açıkken bırakılan bir sürükleme
    // ile kipin kendi bırakışı aynı kareye düşerse ikisi de aynı hücreye yazmayı
    // dener; ikincisi reddedilir ve iki taraf da bunu Console'a yazar.
    public sealed class StructurePlacementMode : IBoardMode
    {
        private readonly IPlacementModeHost host;

        // Hayalet fareye YAPIŞTI mı. İki giriş şeklini ayıran tek alan budur:
        // sürükle-bırak hiç yapıştırmaz, tıkla-bırak ilk bırakışta yapıştırır.
        // Sayaç değil bool, çünkü ayrım "kaçıncı tıklama" değil — hayalet
        // fareye bağlı mı bağlı değil mi.
        //
        // ARTIK TAHTANIN ALANI DEĞİL VE KAZANCI ÖLÇÜLEBİLİR: bu bayrak yalnız
        // bu kip yaşarken anlamlı, kip kapandığında hiçbir cevaba katılmıyor.
        // Tahtada dururken ise kipin kapalı olduğu her karede de okunabiliyordu.
        private bool ghostIsCarried;

        /// <summary>
        /// Kipi kurar ve tahtaya bakan penceresini alır.
        /// </summary>
        /// <param name="placementHost">Hayaleti, jesti ve yerleştirmeyi yapan taraf.</param>
        public StructurePlacementMode(IPlacementModeHost placementHost)
        {
            host = placementHost;
        }

        /// <summary>
        /// Bu kip fareyi ve klavyeyi tek başına sahiplenir.
        /// </summary>
        // SIRADAN TIKLAMA AKIŞI ÇALIŞMAZ, ve sıra bir karardır: çalışsaydı
        // hayalet taşınırken tahtadaki birimler de seçilirdi.
        public bool OwnsPointer => true;

        /// <summary>
        /// Kipe girer: hayaleti açar ve jesti temiz bir sayfayla başlatır.
        /// </summary>
        // JEST KİPLER ARASINDA TAŞINMAZ. Sıfırlanmasaydı önceki kipten kalan
        // "basılı" fazı, bu kipteki ilk bırakışı sahte bir tıklama olarak okurdu.
        public void Enter()
        {
            // Kipe her girişte hayalet SERBESTTİR: ilk bırakış onu ya
            // yerleştirir (sürükleme) ya fareye yapıştırır (tıklama).
            ghostIsCarried = false;

            host.ResetPointerGesture();
            host.ShowPlacementGhost(true);
            host.Log(
                $"[Board] Placement mode ON for '{host.SelectedUnit.Name}'. " +
                "Drag and release to place, or click to carry.");
        }

        /// <summary>
        /// Kipten çıkar: hayaleti gizler. Tahtaya DOKUNMAZ.
        /// </summary>
        // TAHTAYA DOKUNMAMASI BİR TESADÜF DEĞİL, hayaletin gerçek bir yapı
        // OLMAMASININ doğrudan sonucudur: geri alınacak bir şey yok, çünkü
        // yapılmış bir şey yok.
        public void Exit()
        {
            ghostIsCarried = false;
            host.ResetPointerGesture();
            host.ShowPlacementGhost(false);
        }

        /// <summary>
        /// Kipin tek karesi: hayaleti taşır, jesti besler ve bırakma şekline
        /// göre yerleştirir.
        /// </summary>
        public void Advance()
        {
            // KOYACAK BİRİM ARADA KAYBOLABİLİR ve bu teorik değil: savaşın
            // saati bu metottan ÖNCE ilerliyor ve ceset süresi dolan birimi
            // temizlerken seçimi de null'a çekiyor.
            if (host.SelectedUnit == null)
            {
                host.LeaveMode(this);
                host.Log("[Board] Placement mode ended: the placing unit left the battle.");
                return;
            }

            // İPTAL HER ZAMAN ÖNCE. Aşağıya konsaydı iptal tuşu, aynı karede
            // gelen bir bırakışın yerleştirmesinden SONRA işlenirdi: oyuncu
            // iptal ettiğini sanır, tahtada bir yapı bulurdu.
            if (host.PlacementCancelRequested)
            {
                host.LeaveMode(this);
                host.Log("[Board] Placement mode CANCELLED. The board was not touched.");
                return;
            }

            if (!host.TryReadPointerCell(out float worldX, out float worldY, out int x, out int y))
            {
                return;
            }

            // HER KARE, koşulsuz: hayalet fare hücresinin MERKEZİNDE durur.
            // Yalnız sürüklerken taşınsaydı tıkla-bırak akışında hayalet
            // yerinde donar ve oyuncu nereye koyacağını göremezdi.
            host.MovePlacementGhostTo(x, y);

            // HAYALETİN GEÇERSİZ HÜCREDE FARKLI GÖRÜNMESİ HENÜZ YAPILMIYOR ve
            // sebep "önemsiz" değil, SAHİPLİK: geçerliliğe yerleştirmenin
            // kendisi karar veriyor ve cevabını ancak YERLEŞTİREREK veriyor.
            // Kuralın bir KOPYASINI buraya yazmak, kural büyüdüğü gün sessizce
            // yalan söylerdi — yeşil hayalet, reddedilen yerleştirme.

            PointerPhase phase = host.FeedPointerGesture(worldX, worldY);

            switch (phase)
            {
                // SÜRÜKLE-BIRAK: bırakıldığı yer yerleştirilecek yerdir.
                case PointerPhase.DragReleased:
                    host.CommitPlacement(x, y);
                    break;

                case PointerPhase.ClickReleased:
                    if (ghostIsCarried)
                    {
                        // TIKLA-BIRAK, ikinci tıklama: yerleştir.
                        host.CommitPlacement(x, y);
                    }
                    else
                    {
                        // TIKLA-BIRAK, ilk tıklama: kipte KAL, hayalet fareyi
                        // takip etmeye devam etsin.
                        ghostIsCarried = true;
                        host.ResetPointerGesture();
                        host.Log("[Board] Ghost is now carried. Click again to place, or cancel.");
                    }

                    break;
            }

            // default DALI BİLEREK YOK: beş fazın üçü "henüz bir şey olmadı"
            // demektir. Bir SONUÇ enum'unda işlenmeyen değer bir hatadır; bir
            // FAZ enum'unda işlenmeyen faz normal akıştır.
        }
    }
}
