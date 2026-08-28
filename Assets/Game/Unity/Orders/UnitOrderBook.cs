using System.Collections.Generic;
using GridStrategy.Core;

namespace GridStrategy.Unity
{
    //   ═══ EMİR DEFTERİ — BİRİM BAŞINA BİR EMİR, TAHTA BAŞINA ÇOK ════
    //
    //     Write(A, "B'ye saldır")   ┐
    //     Write(C, "D'ye saldır")   ├─►  A ─► saldırı emri (B)
    //     Write(A, "E'ye saldır")   ┘    C ─► saldırı emri (D)
    //          ^ A'nın eskisini EZER      (A'nın emri E oldu)
    //
    //     Advance()  her emre TEK TEK sorar; yalnız kendi cevabı
    //                "Devam" olmayan emir düşüyor. Birinin düşmesi
    //                ötekini HİÇ etkilemiyor.
    //
    //   ESKİ HÂLDE: tahtada tek bir emir vardı (pendingStrike dörtlüsü)
    //   ve ikinci Write birincisini SİLİYORDU. Operatörün "iki taraf
    //   paralel olmuyor" şikâyeti tam olarak bu satırdı.

    /// <summary>
    /// Tahtadaki bütün emirlerin defteri: hangi birime ne söylendiğini tutar ve
    /// her kare hepsini bir adım ilerletir.
    ///
    /// OYUNDA NE İŞE YARAR: iki ayrı takımdan üç birim aynı anda kendi hedefine
    /// vurabilir; biri hedefini kaybettiğinde yalnız onunki durur.
    /// </summary>
    // MonoBehaviour DEĞİL ve kiplerle aynı gerekçe: defter ne Update alır ne
    // sahneye bağlanır, bu yüzden EditMode'da `new` ile kurulup sınanabiliyor.
    //
    // BİRİM BAŞINA BİR EMİR — VE BU BİR KURAL, bir kısıt değil: bir savaşçının
    // aynı anda iki hedefe saldırması diye bir şey yok. Değer bir liste olsaydı
    // "hangisi önce" sorusu doğar ve o sorunun bugün cevabı olmazdı.
    public sealed class UnitOrderBook
    {
        private readonly Dictionary<Unit, IUnitOrder> orders = new Dictionary<Unit, IUnitOrder>();

        // İLERLETMENİN GİRDİ TAMPONU. Projede bu üçüncü örnek (cleanupBuffer,
        // structureFireBuffer) ve gerekçe her üçünde aynı: bir emir vuruş
        // yaptırıyor, vuruş bir kimliği tahtadan kaldırabiliyor ve kaldırma bu
        // sözlüğe dokunuyor. Sözlüğü gezerken değiştirmek gezintiyi patlatır,
        // bu yüzden anahtarlar önce kopyalanıyor.
        //
        // ALAN, YEREL DEĞİŞKEN DEĞİL: her karede bir liste kurmak, hiçbir şey
        // olmayan karelerde bile çöp üretirdi.
        private readonly List<Unit> advanceBuffer = new List<Unit>();

        // ██ SÜPÜRMENİN AYRI TAMPONU — TEK TAMPON BİR TUZAKTI ██
        // İkisi de "anahtarları kopyala, sonra sil" yapıyor, yani tek bir liste
        // yetiyormuş gibi görünüyor. Ölçüsü şu: bir emrin vuruşu bir kimliği
        // tahtadan kaldırabilir, kaldırma temizliğe iner ve temizlik
        // CancelTargeting'i çağırır — yani bu üye Advance'in DÖNGÜSÜNÜN İÇİNDEN
        // çağrılabilir. Tampon paylaşılsaydı süpürmenin son satırındaki Clear,
        // hâlâ gezilmekte olan listeyi boşaltır ve Advance kalan emirleri sessizce
        // atlardı — patlamayan, yalnız EKSİK koşan bir kare.
        private readonly List<Unit> sweepBuffer = new List<Unit>();

        /// <summary>Defterde duran emir sayısı.</summary>
        public int Count => orders.Count;

        /// <summary>
        /// Bu birime bir emir yazar; varsa öncekinin YERİNE geçer.
        /// </summary>
        // ÖNCEKİ EMİR SESSİZCE EZİLİR, ve doğrusu bu: aynı savaşçıya verilen
        // ikinci emir birincisini geçersiz kılar — oyuncu fikrini değiştirdi.
        // İkisi birden tutulsaydı vazgeçilen hedef de vurulurdu.
        public void Write(Unit unit, IUnitOrder order)
        {
            if (unit == null || order == null)
            {
                return;
            }

            orders[unit] = order;
        }

        /// <summary>Bu birimin emrini verir; emri yoksa false.</summary>
        public bool TryGet(Unit unit, out IUnitOrder order)
        {
            if (unit == null)
            {
                order = null;
                return false;
            }

            return orders.TryGetValue(unit, out order);
        }

        /// <summary>
        /// Bu birimin emrini siler.
        /// </summary>
        /// <returns>Silinecek bir emir varsa true.</returns>
        public bool Cancel(Unit unit)
        {
            return unit != null && orders.Remove(unit);
        }

        /// <summary>
        /// Bu kimliği HEDEFLEYEN bütün emirleri siler.
        /// </summary>
        /// <returns>Silinen emir sayısı.</returns>
        // ██ NEDEN ERKEN SÜPÜRME — EMİRLER ZATEN KENDİ DÜŞÜYORDU ██
        // Hedefi tahtadan kalkmış bir emir bir sonraki Advance'te konum
        // sorusuna takılıp kendiliğinden iptal olur, yani ARIZA yok. Bu üye
        // yine de var, çünkü kaldırmanın cevabı AYNI KAREDE görünmeli: oyuncu
        // çöp kutusuyla bir birimi sildiğinde ona saldıran savaşçının emri
        // hemen düşer, bir kare gecikmeyle değil. Ölçüsü bir testtir —
        // RemoveSelected'ın hemen ardından defterin boş olduğu iddiası.
        //
        // TERS DİZİN TUTULMUYOR (hedef → emirler): elli hücrelik bir tahtada
        // onlu sayıda emir var; ikinci bir sözlük, aynı gerçeğin ikinci bir
        // yazılabilir kopyası olurdu ve senkronu bozulduğu gün sessizce yanlış
        // cevap verirdi.
        public int CancelTargeting(Unit target)
        {
            if (target == null)
            {
                return 0;
            }

            sweepBuffer.Clear();

            foreach (KeyValuePair<Unit, IUnitOrder> entry in orders)
            {
                if (ReferenceEquals(entry.Value.Target, target))
                {
                    sweepBuffer.Add(entry.Key);
                }
            }

            for (int i = 0; i < sweepBuffer.Count; i++)
            {
                orders.Remove(sweepBuffer[i]);
            }

            int removed = sweepBuffer.Count;
            sweepBuffer.Clear();
            return removed;
        }

        /// <summary>Bütün emirleri siler.</summary>
        public void Clear()
        {
            orders.Clear();
        }

        /// <summary>
        /// Her emri bir kare ilerletir ve işi biten ya da düşen emirleri
        /// defterden siler.
        /// </summary>
        // EMİR SIRASI SÖZLÜĞÜN KENDİ SIRASI ve bu bilinçli bir taviz: iki
        // savaşçının aynı karede hangisinin önce vuracağı, ikisi de aynı
        // hedefin son canını götürdüğünde gözlenebilir. Sıra bugün ölçülebilir
        // bir haksızlık üretmiyor (bekleme süreleri zaten ayrı akıyor), ve
        // garanti verilecek olsaydı sahibi burası değil savaşın kendi defteri
        // olurdu.
        public void Advance()
        {
            if (orders.Count == 0)
            {
                return;
            }

            advanceBuffer.Clear();
            foreach (Unit unit in orders.Keys)
            {
                advanceBuffer.Add(unit);
            }

            for (int i = 0; i < advanceBuffer.Count; i++)
            {
                Unit unit = advanceBuffer[i];

                // EMİR ARADA DÜŞMÜŞ OLABİLİR: kopyalanan anahtarlardan biri
                // işlenirken bir vuruş, bir başkasının emrini defterden
                // kaldırabilir (hedefini tahtadan sildiği için). Sözlükten
                // yeniden okumak o durumu sessizce geçer; tamponun kendisine
                // güvenmek yok edilmiş bir emri bir kez daha koştururdu.
                if (!orders.TryGetValue(unit, out IUnitOrder order))
                {
                    continue;
                }

                if (order.Advance() != OrderProgress.Continue)
                {
                    // ANAHTARLA SİLİNİYOR, DEĞERLE DEĞİL: emir bu satıra kadar
                    // gelirken yerine yenisi yazılmış olabilir ve o zaman
                    // silinmesi gereken şey YOK. Karşılaştırma, biten emrin
                    // hâlâ defterdeki emir olduğunu doğruluyor.
                    if (orders.TryGetValue(unit, out IUnitOrder current)
                        && ReferenceEquals(current, order))
                    {
                        orders.Remove(unit);
                    }
                }
            }

            advanceBuffer.Clear();
        }
    }
}
