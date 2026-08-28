using System;
using System.Collections.Generic;

namespace GridStrategy.Core
{
    // ═══ ROL: VARLIK (Entity) ════════════════════════════════════════
    // kimlik : var — aynı 3x5 ölçüdeki iki tahta aynı tahta değildir;
    //          hangi hücrede kimin durduğu örneğe aittir
    // hafıza : var — PlaceUnit'ten sonra aynı TryGetUnit(1,2) çağrısı
    //          farklı cevap verir
    // Unity  : gerekmez — UnityEngine.Grid koordinat çevirir, bu tip
    //          tahtanın DURUMUNU tutar; aynı kelime, iki ayrı sahip
    // karar  : tutar ve bildirir — IsInsideGrid kural gibi görünse de kendi
    //          ölçüsüne bakan bir sorgudur, dışarıdan gelen bir politika değil
    /// <summary>
    /// Tahta: hangi hücrede kimin durduğunu tutan tek yer, yani konumun tek
    /// sahibi. Tuttuğu şeyin ne olduğunu bilmez — canı, tarafı, durumu
    /// <see cref="Unit"/>'e eşlenen yan tablolarda yaşar.
    ///
    /// "Dolu hücreye taşınamaz" bir KURAL'dır ve bu tipin DEĞİL,
    /// <see cref="MoveAction"/>'ın işidir; burada olsaydı aynı kural iki
    /// yerde yaşar ve ayrıştığı gün hangisinin doğru olduğunu derleyici
    /// söyleyemezdi.
    ///
    /// GEREKÇELER: Docs/deep/kod/Core/UnitGrid.md
    /// </summary>
    public sealed class UnitGrid
    {
        private readonly Unit[,] cells;

        // ██ AYNI GERÇEĞİN İKİNCİ OKUMA YÖNÜ — KOPYASI DEĞİL ██
        // Yukarıdaki dizi "şu hücrede kim var" sorusunu O(1) cevaplıyor; bu
        // sözlük "şu birim nerede" sorusunu O(1) cevaplıyor. İkisi AYNI olguyu
        // taşıyor, yani ayrışabilirler — ve ayrışmalarını engelleyen şey bir
        // disiplin değil bir YAPI: ikisine de yalnız WriteCell dokunuyor.
        //
        // DEĞER int, ÇÜNKÜ Core MOTORU GÖRMÜYOR: `noEngineReferences: true`,
        // yani Vector2Int burada yok. İki alanlık bir struct açmak yerine
        // hücre `y * Width + x` ile paketleniyor; paketleme bu tipin İÇİNDE
        // kalıyor ve dışarıya hiç sızmıyor.
        private readonly Dictionary<Unit, int> occupied = new Dictionary<Unit, int>();

        // Ölçünün pozitifliği burada doğrulanır: "tahtanın ölçüsü pozitif
        // olmalı" kuralının başka sahibi yok. Ölçü bu kurucuda dizinin ŞEKLİ
        // olarak donar; bir daha kopyalanmaz.
        // → UnitGrid.md#unitgridint-width-int-height
        public UnitGrid(int width, int height)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be positive.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be positive.");
            }

            cells = new Unit[width, height];
        }

        // ÖLÇÜNÜN TEK SAHİBİ DİZİNİN KENDİSİDİR. Üçü de türetilir, hiçbiri
        // saklanmaz: ayrı bir width alanı tutulsaydı aynı ölçü iki sahipli
        // olur ve dizi bir gün yeniden boyutlandırıldığında alan sessizce
        // eskirdi. Dışarıya bildirilen ölçü ile içeride uygulanan sınır aynı
        // diziden okunduğu için ayrışamazlar.
        // → UnitGrid.md#width
        public int Width => cells.GetLength(0);

        public int Height => cells.GetLength(1);

        public int CellCount => Width * Height;

        // Tahtanın kendi ölçüsüne bakan bir SORGU, dışarıdan gelen bir
        // politika değil — bu yüzden tahtada kalır. Sormak serbesttir: tahta
        // dışı koordinat için fırlatmaz, false döner.
        // → UnitGrid.md#isinsidegridint-x-int-y
        public bool IsInsideGrid(int x, int y)
        {
            return x >= 0
                && y >= 0
                && x < cells.GetLength(0)
                && y < cells.GetLength(1);
        }

        // TAHTA DIŞI GÜRÜLTÜLÜ, DOLU HÜCRE SESSİZ. Tahta dışına yerleştirmek
        // bir ÇAĞIRAN hatasıdır ve gürültüyle patlar; dolu hücrenin üstüne
        // yazmak ise oyunun her turunda olan normal bir olgudur ve sessizdir.
        // Çizgi teknik değil SAHİPLİK çizgisi, ve tahtanın kenarından geçer.
        // → UnitGrid.md#placeunitint-x-int-y-unit-unit
        // DERİN ANLATIM: Docs/deep/konular/03-tahta-sahipligi.md
        public void PlaceUnit(int x, int y, Unit unit)
        {
            ThrowIfOutsideGrid(x, y, nameof(x), nameof(y));

            WriteCell(x, y, unit);
        }

        // TAHTANIN ANAHTARI KOORDİNATTIR, KİMLİK DEĞİL. Kimlikle silmek
        // anahtarı tersine çevirir: çağıranın zaten bildiği bir koordinat
        // için tahta baştan sona taranır ve "bulamadım" ile "sildim" aynı
        // void dönüşe düşer. Anahtarı seçen şey, çağıranın elinde ne olduğu.
        // → UnitGrid.md#removeunitint-x-int-y
        /// <summary>
        /// Hücreyi boşaltır. Hücre zaten boşsa hiçbir şey olmaz.
        /// </summary>
        public void RemoveUnit(int x, int y)
        {
            ThrowIfOutsideGrid(x, y, nameof(x), nameof(y));

            WriteCell(x, y, null);
        }

        // TEK PARÇA BİR İŞLEM, RemoveUnit + PlaceUnit BİLEŞİMİ DEĞİL. Sebep
        // tek kelimeyle YARIM KALMA: bileşimde silme başarılır, yazma patlar
        // ve arada birim hiçbir hücrede durmayan bir hayalete döner. Burada o
        // ara hâl engellenmiyor — HİÇ DOĞMUYOR, çünkü doğrulama önce biter.
        // → UnitGrid.md#moveunitint-fromx-int-fromy-int-tox-int-toy
        /// <summary>
        /// Bir hücredeki birimi başka bir hücreye taşır.
        /// Kaynak hücre boşsa hiçbir şey olmaz; hedef doluysa üstüne yazar —
        /// "dolu hücreye taşınamaz" kuralı bu tipin değil, çağıranın işidir.
        /// </summary>
        public void MoveUnit(int fromX, int fromY, int toX, int toY)
        {
            // Her iki koordinat da TEK BİR hücreye dokunmadan önce doğrulanır.
            // Sıra burada bir hata değil, tam olarak "yarım kalma"nın panzehiri.
            ThrowIfOutsideGrid(fromX, fromY, nameof(fromX), nameof(fromY));
            ThrowIfOutsideGrid(toX, toY, nameof(toX), nameof(toY));

            Unit moving = cells[fromX, fromY];
            if (moving == null)
            {
                return;
            }

            // Önce kaynağı boşalt, sonra hedefe yaz. Aynı hücreye taşımada
            // (fromX == toX && fromY == toY) sıra önemlidir: tersi olsaydı
            // birim hedefe yazılır, hemen ardından aynı hücre boşaltılırdı.
            WriteCell(fromX, fromY, null);
            WriteCell(toX, toY, moving);
        }

        /// <summary>
        /// Bu birim tahtada duruyor mu, duruyorsa hangi hücrede.
        /// </summary>
        // ██ ÇEVRİLEN KARAR: HER ÇAĞRIDA TARAMA → SÖZLÜK ██
        // Bu cevabı Battle şöyle üretiyordu ve 10x5'lik bir tahtada bedeli
        // gözlenemezdi:
        //
        //     for (int cellX = 0; cellX < board.Width; cellX++)
        //         for (int cellY = 0; cellY < board.Height; cellY++)
        //             if (board.TryGetUnit(cellX, cellY, out Unit standing)
        //                 && ReferenceEquals(standing, unit)) { ... }
        //
        // KIRILAN ŞEY İKİ ÖLÇÜMÜN ÇARPIMI: tahta 100x50 oldu (çağrı başına
        // 5000 hücre) VE kalıcı saldırı emri doğdu — her emir her karede bu
        // soruyu ÜÇ kez soruyor (saldıran, hedef, vuruşun kendisi). On emirde
        // saniyede milyonlarca hücre yoklaması. İkisinden yalnız biri olsaydı
        // eski hâl hâlâ doğruydu.
        // → Docs/deep/konular/09-kararlarin-cevrilmesi.md (madde 11)
        //
        // SORU BURAYA TAŞINDI, Battle'DA KALMADI: cevabı O(1) verebilmenin tek
        // yolu hücrelerin yazıldığı yerde bir dizin tutmak ve o yer burası.
        // Battle'da tutulsaydı dizin, mutasyonları görmeyen bir tipte yaşar ve
        // her yazmada elle güncellenmesi gerekirdi.
        public bool TryGetPosition(Unit unit, out int x, out int y)
        {
            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

            if (!occupied.TryGetValue(unit, out int packed))
            {
                // Bulunamayınca (0,0) DEĞİL (-1,-1): sıfır geçerli bir hücredir
                // ve dönüşü yok sayan bir çağıran onu sessizce köşe sanardı.
                x = -1;
                y = -1;
                return false;
            }

            x = packed % Width;
            y = packed / Width;
            return true;
        }

        /// <summary>
        /// Bir hücreyi yazar ve ters dizini AYNI çağrıda günceller.
        /// </summary>
        // ██ TEK YAZMA NOKTASI — VE BU BİR ÜSLUP DEĞİL, KELEPÇE ██
        // İki gerçek (dizi ve sözlük) ancak ikisine de aynı satırda dokunulursa
        // ayrışamaz. Üç mutasyon üyesi (PlaceUnit, RemoveUnit, MoveUnit) kendi
        // gövdelerinde `cells[x, y] = ...` yazsaydı, dördüncü bir üye eklendiği
        // gün sözlüğü güncellemeyi unutmak SESSİZ bir hata olurdu: birim
        // ekranda doğru yerde durur, kural katmanı onu başka hücrede sanırdı.
        //
        // ÖNCEKİ SAKİN DÜŞÜYOR: bir hücrenin üstüne yazmak, orada duran birimi
        // tahtadan düşürür. Dizinden silinmeseydi o birim, artık kimsenin
        // olmadığı bir hücrede duruyor görünürdü.
        private void WriteCell(int x, int y, Unit unit)
        {
            Unit previous = cells[x, y];
            if (previous != null)
            {
                occupied.Remove(previous);
            }

            cells[x, y] = unit;

            if (unit != null)
            {
                occupied[unit] = (y * Width) + x;
            }
        }

        // Üç yazma metodunun ortak sınır kontrolü. Parametre adı DIŞARIDAN
        // geliyor çünkü fırlatılan exception hangi ARGÜMANIN suçlu olduğunu
        // söylemeli: MoveUnit'te "x" değil "toX" yazmalı. Silinirse yerine
        // dilin kendi IndexOutOfRangeException'ı geçer ve o, hangi argümanın
        // suçlu olduğunu söylemez — belgede "İŞ BÖLÜMÜ: iki mekanizma, iki
        // ayrı yanlış" başlığı altında.
        // → UnitGrid.md#placeunitint-x-int-y-unit-unit
        private void ThrowIfOutsideGrid(int x, int y, string xParamName, string yParamName)
        {
            if (x < 0 || x >= cells.GetLength(0))
            {
                throw new ArgumentOutOfRangeException(xParamName, x, "x is outside the grid.");
            }

            if (y < 0 || y >= cells.GetLength(1))
            {
                throw new ArgumentOutOfRangeException(yParamName, y, "y is outside the grid.");
            }
        }

        // TRY ŞEKLİ: YOKLUK İMZANIN KENDİSİNE YAZILIR. Nullable dönüş
        // (FindUnit) uygulandı ve kaldırıldı: <Nullable> KAPALI olduğu için
        // null dönüş derleme zamanında hiçbir koruma taşımaz, out ise çağıranı
        // dallanmaya zorlar. Koruma out'un dilsel gücünden değil ŞEKLİN
        // GÖRÜNÜRLÜĞÜNDEN gelir — atılan bir bool çağrı yerinde göze çarpar.
        // İki ayrı sebep (hücre yok / hücre boş) tek false'a düşer; farkı
        // sormak isteyen IsInsideGrid'i ayrıca çağırmak zorundadır.
        // → UnitGrid.md#trygetunitint-x-int-y-out-unit-unit
        // DERİN ANLATIM: Docs/deep/konular/03-tahta-sahipligi.md
        public bool TryGetUnit(int x, int y, out Unit unit)
        {
            if (!IsInsideGrid(x, y))
            {
                unit = null;
                return false;
            }

            unit = cells[x, y];
            return unit != null;
        }
    }
}
