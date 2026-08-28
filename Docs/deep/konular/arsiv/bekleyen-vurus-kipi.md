# `PendingStrikeMode` — bekleyen vuruşun tahta başına tek kipi

| | |
|---|---|
| Sahibi olduğu dosya | `Assets/Game/Unity/Modes/PendingStrikeMode.cs` |
| Yerine geçen | `IUnitOrder`, `UnitOrderBook`, `AttackOrder`, `ReviveOrder` |
| Hikâyesi | [../09-kararlarin-cevrilmesi.md](../09-kararlarin-cevrilmesi.md) madde 2 |
| Kaydı | [../10-geri-alinan-kararlar.md](../10-geri-alinan-kararlar.md) bölüm 3 |
| Kaynağı | `HEAD` işlemesindeki dosyanın tamamı, 174 satır |

Bu dosya çalışma ağacında **silinmiş** durumda. Silme işlendiği gün kod ağaçtan
tamamen kalkacak, ve o gün buradaki kopya tek kopya olacak.

Kırılan şey sayıydı, biçim değil. Bekleyen vuruş tahta başına **tekti**, oysa
emir birim başına olmalıydı. Aşağıdaki `host.StrikeAttacker` ve
`host.StrikeTarget` okumalarının hepsi o tekliğin izidir: emrin sahibi kip
değil, **tahta**.

---

## Dosyanın tamamı, birebir

```csharp
using GridStrategy.Core;

namespace GridStrategy.Unity
{
    //   ═══ BEKLEYEN VURUŞ — "yaklaş, sonra vur" ═════════════════════
    //
    //     BOŞTA ──uzaktaki hedefe tıklama (yürüyüş başladı)──► BEKLEYEN VURUŞ
    //
    //     Gir()  : iş yok; emri yazan çağrı geçişten hemen sonra gelir
    //     Ilerlet: emir ayakta mı ──hayır──► çık
    //              görsel yürüyor mu ──evet──► bekle
    //              ikisi de hayır ──► emri sil, SONRA vur
    //     Tıklama: AYNI hedefe ikinci tıklama YUTULUR (emir tekrar edilmez)
    //     Cik()  : emir silinir; tahtaya ve savaşa HİÇ dokunulmaz
    //
    //     BEKLEYEN VURUŞ ──vuruş indi | emir düştü | B tuşu──► BOŞTA

    /// <summary>
    /// Uzaktaki düşmana tıklandığında yazılan emir: savaşçı önce yürür, vuruş
    /// yürüyüş EKRANDA bittiği an olur.
    ///
    /// OYUNDA NE İŞE YARAR: birim daha yolun ortasındayken saldırı pozu
    /// oynasaydı ekran, tahtada olup bitene göre yalan söylerdi.
    /// </summary>
    // BEKLEME EKRANIN SAATİNE BAĞLI, tahtanın saatine değil: tahta hareketi
    // çoktan işledi, beklenen tek şey görselin hedefin yanına VARMASI.
    //
    // EMRİN İKİ KİMLİĞİ BU NESNENİN İÇİNDE DURMUYOR ve bedeli açıkça yazılı:
    // saldıranı ve hedefi temizlik de okuyor (tahtadan kalkan kimlik emri de
    // götürür), ve mevcut testler o alanları tahtada ADIYLA yazıp okuyor. Kip
    // o ikisinin sahibi değil, ANLAMININ sahibidir — "ne zaman düşer, ne zaman
    // yürür, hangi tıklamayı yutar" sorularının üçü de burada cevaplanıyor.
    public sealed class PendingStrikeMode : IBoardMode
    {
        private readonly IPendingStrikeHost host;

        // Hedefin, emir YAZILDIĞI andaki hücresi. Ayrıca tutuluyor çünkü hem
        // Console satırı hem merminin varış noktası onu istiyor ve hedef vuruş
        // anına kadar tahtadan kalkmış olabilir.
        //
        // TAHTADA DEĞİL BURADA, ve ayrımı ölçüm doğurdu: iki kimliği temizlik de
        // okuyor, bu iki sayıyı ise hiç kimse — sahibi tek olan şeyi paylaşılan
        // bir yere yazmak, hiçbir çağıranı olmayan iki alan doğururdu.
        private int targetX;
        private int targetY;

        /// <summary>
        /// Kipi kurar ve tahtaya bakan penceresini alır.
        /// </summary>
        /// <param name="strikeHost">Emri saklayan ve vuruşu yaptıran taraf.</param>
        public PendingStrikeMode(IPendingStrikeHost strikeHost)
        {
            host = strikeHost;
        }

        /// <summary>
        /// Bu kip fareyi sahiplenmez: oyuncu beklerken tahtayı kullanmaya devam
        /// eder ve yeni bir eylem emri düşürür.
        /// </summary>
        public bool OwnsPointer => false;

        /// <summary>Girişte yapılacak iş yok; emri yazan çağrı hemen ardından gelir.</summary>
        // GİRİŞ BOŞ VE EMİR AYRI BİR ÇAĞRIYLA YAZILIYOR: emri kurucuya almak,
        // her yeni emirde yeni bir kip nesnesi doğurmak demekti — kare başına
        // çöp üreten bir tasarım.
        public void Enter()
        {
        }

        /// <summary>
        /// Emri unutur. Tahtaya ve savaşa HİÇ dokunmaz.
        /// </summary>
        // DOKUNMAMASININ SEBEBİ YERLEŞTİRME KİPİYLE BİREBİR AYNI: henüz
        // yapılmış bir şey yok, dolayısıyla geri alınacak bir şey de yok.
        //
        // İPTALİN TEK KAPISI BURASI OLDU: "yerleştirmeye girerken emri düşür"
        // kuralı artık hiçbir çağıranın hafızasında değil, geçişin kendisinde.
        public void Exit()
        {
            targetX = 0;
            targetY = 0;
            host.ClearStrikeOrder();
        }

        /// <summary>
        /// Yürüyüşü biten savaşçının bekleyen vuruşunu yürütür.
        /// </summary>
        public void Advance()
        {
            // EMİRSİZ BİR KİP DE ÖLÜ SAYILIYOR: eski hâlde saldıran null iken
            // yalnızca çıkılıyordu, burada kipten de çıkılıyor. Fark
            // gözlenemez, çünkü silinecek alan zaten boş ve Boşta kip aynı
            // tıklamaya aynı cevabı veriyor.
            if (!IsAlive())
            {
                host.LeaveMode(this);
                return;
            }

            if (host.IsViewWalking(host.StrikeAttacker))
            {
                return;
            }

            Unit attacker = host.StrikeAttacker;
            Unit target = host.StrikeTarget;
            int x = targetX;
            int y = targetY;

            // EMİR ÖNCE SİLİNİR, sonra vurulur: saldırı bir durum değişikliği
            // doğuruyor, o zincir temizliğe kadar gidebiliyor ve yarım kalmış
            // bir emrin o sırada ikinci kez okunması aynı vuruşu tekrarlardı.
            host.LeaveMode(this);

            host.ExecuteStrike(attacker, target, x, y);
        }

        /// <summary>
        /// Bu tıklama, zaten yazılmış emrin AYNISINI mı istiyor?
        /// </summary>
        /// <param name="clicked">Tıklanan hücrede duran kimlik; boşsa null.</param>
        // OYUNDA NE İŞE YARAR: yaklaşan savaşçısına sabırsızlanıp hedefe ikinci
        // kez tıklayan oyuncu iki vuruş ödemesin.
        //
        // YÜRÜYOR MU DİYE SORULMUYOR ve yokluğu bir karardır: görsel varmışsa
        // Ilerlet zaten AYNI karede, girdiden ÖNCE vuruşu yürütüyor ve emir o
        // noktada silinmiş oluyor.
        public bool ConsumesClick(Unit clicked)
        {
            return ReferenceEquals(clicked, host.StrikeTarget) && IsAlive();
        }

        /// <summary>
        /// Emir hâlâ anlamlı mı: saldıran seçili mi ve iki taraf da tahtada
        /// duruyor mu.
        /// </summary>
        // ÜÇ İPTAL KOŞULU TEK ÜYEDE, ve ayrı bir üye olması bilerek: koşullar
        // Ilerlet'in içine gömülseydi motor koşmadan sınanamazlardı. Dördüncü
        // koşul olan "oyuncu başka bir eylem başlattı" burada YOK, çünkü onun
        // sahibi girdi tarafı — başka bir hedefe ya da boş bir hücreye giden
        // her tıklama emri düşürüyor, aynı hedefe gelen tekrar ise emri korur.
        public bool IsAlive()
        {
            if (host.StrikeAttacker == null || host.StrikeTarget == null)
            {
                return false;
            }

            // SEÇİM DEĞİŞTİ: oyuncu başka bir birime geçtiyse eski emir düşer.
            if (!ReferenceEquals(host.StrikeAttacker, host.SelectedUnit))
            {
                return false;
            }

            // SALDIRAN ya da HEDEF tahtadan kalktı. Soru konum üzerinden
            // soruluyor çünkü temizlik ikisini de aynı kapıdan çıkarıyor ve
            // konumu olmayan bir kimliğe saldırı çağrısı istisna atardı.
            return host.IsOnBoard(host.StrikeAttacker) && host.IsOnBoard(host.StrikeTarget);
        }

        /// <summary>
        /// Emri yazar: iki kimlik tahtaya, hedefin hücresi buraya.
        /// </summary>
        // ÖNCEKİ EMİR SESSİZCE EZİLİR, ve doğrusu bu: aynı savaşçıya verilen
        // ikinci emir birincisini geçersiz kılar. İki emir birden tutulsaydı
        // oyuncunun vazgeçtiği hedef de vurulurdu.
        public void Write(Unit attacker, Unit target, int x, int y)
        {
            targetX = x;
            targetY = y;
            host.WriteStrikeOrder(attacker, target);
        }
    }
}
```

## Neden burada duruyor

Tip ağaçtan kalkıyor ve 09'un madde 2'si ondan yalnızca `IBoardMode` arayüzünü
ve birkaç satırı taşıyor. Kipin **anlamının** sahibi olduğu üç soru
(`Ilerlet` ne zaman düşer, hangi tıklamayı yutar, hangi üç koşulda emir ölür)
ancak tam metinde okunuyor.
