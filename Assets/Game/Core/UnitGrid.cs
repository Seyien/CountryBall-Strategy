using System;

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
    public sealed class UnitGrid
    {
        private readonly Unit[,] cells;

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

        // Ölçüyü dizinin KENDİSİNDEN oku, ikinci bir kopyasını saklama.
        // Ayrı bir width alanı tutulsaydı aynı bilgi iki yerde yaşardı ve ikisini
        // senkron tutmak bir yükümlülük olurdu; dizi bir gün yeniden
        // boyutlandırılırsa (tahta büyümesi) alan sessizce eskirdi.
        //
        // Alternatif: ölçüyü ayrı readonly int width/height alanlarında saklamak.
        // Seçilmedi: aynı ölçü iki sahipli olur ve dizi okumasının maliyeti hiç
        // ölçülmedi.
        public int Width => cells.GetLength(0);

        public int Height => cells.GetLength(1);

        public int CellCount => Width * Height;

        public bool IsInsideGrid(int x, int y)
        {
            return x >= 0
                && y >= 0
                && x < cells.GetLength(0)
                && y < cells.GetLength(1);
        }

        // Tahta dışına yerleştirmek bir ÇAĞIRAN hatasıdır, bir oyun sonucu değil;
        // bu yüzden burada gürültüyle patlar. TryGetUnit ise sessiz kalır, çünkü
        // "bu hücrede kimse yok" normal bir cevaptır ve kenar taramalarında her
        // kare defalarca sorulur. Aynı sınıf, iki farklı hata felsefesi — ve
        // ayırıcı şey teknik değil, sorunun KİME AİT olduğudur.
        //
        // REDDEDILEN - UnitGrid.cs:73 yerine:
        //     public bool TryPlaceUnit(int x, int y, Unit unit)   // tahta dışı -> false
        // KIRILAN  : dönen bool yok sayılabilir ve kod yine derlenir.
        //            çağıran sonucu okumaz -> birim hiçbir hücreye yazılmaz
        //            ekranda karşılığı yok -> birim sessizce kaybolur
        //            BoardAdapter'ın "önce KURAL, sonra görsel" sırası anlamsızlaşır
        //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
        // KAZANIRDI: yerleştirmeyi KULLANICI tetikliyorsa (sürükle-bırak ile
        //            tahta dışına bırakmak) — o gün tahta dışı, sık ve beklenen
        //            bir oyun sonucudur.
        // TEK CUMLE: Tahta dışı koordinat oyunun değil ÇAĞIRANIN hatasıdır, ve
        //            çağıran hatası dönüş değeriyle değil gürültüyle bildirilir.
        public void PlaceUnit(int x, int y, Unit unit)
        {
            ThrowIfOutsideGrid(x, y, nameof(x), nameof(y));

            cells[x, y] = unit;
        }

        // RemoveUnit ve MoveUnit hangi felsefeye ait? Ayırıcı çizgi bu dosyada
        // OKUMA/YAZMA değil — ve bunu görmenin yolu PlaceUnit'e dikkatle bakmak:
        // tahta dışına yazmayı gürültüyle reddeder, ama DOLU bir hücrenin üstüne
        // sessizce yazar. Yani KOORDİNAT çağıranın sorumluluğudur (gürültülü),
        // HÜCRENİN İÇERİĞİ ise bir oyun olgusudur (sessiz). Yeni iki metot aynı
        // çizgiyi izler: tahta dışı koordinat patlar, boş hücreyi boşaltmak ya da
        // dolu hücrenin üstüne taşımak hiçbir şikâyet üretmez.
        //
        // "Dolu hücreye taşınamaz" bir KURAL'dır ve sahibi MoveAction'dır. Buraya
        // da konsaydı aynı kural iki yerde yaşardı; ikisi ayrışınca hangisinin
        // doğru olduğunu derleyici söyleyemezdi.
        //
        // REDDEDILEN - UnitGrid.cs:106 yerine:
        //     public void RemoveUnit(Unit unit)   // koordinat değil KİMLİK ile
        // KIRILAN  : çağıranın zaten bildiği bir koordinat için tahta baştan sona
        //            taranır; "bulamadım" ile "sildim" ayırt edilemez ve MoveUnit
        //            aynı aramayı iki kez yapmak zorunda kalır.
        //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
        // KAZANIRDI: silmeyi tetikleyen yer birimi TANIYIP hücresini BİLMİYORSA —
        //            "ölen her birimi tahtadan kaldır" gibi bir süpürme, ölüm
        //            olayından yalnız Unit alır.
        // TEK CUMLE: Tahtanın anahtarı koordinattır; kimlikle silmek anahtarı
        //            tersine çevirir ve her silmeyi bir aramaya döndürür.
        /// <summary>
        /// Hücreyi boşaltır. Hücre zaten boşsa hiçbir şey olmaz.
        /// </summary>
        public void RemoveUnit(int x, int y)
        {
            ThrowIfOutsideGrid(x, y, nameof(x), nameof(y));

            cells[x, y] = null;
        }

        // MoveUnit, RemoveUnit + PlaceUnit BİLEŞİMİ DEĞİL, tek parça bir işlem.
        // Sebep tek kelimeyle: YARIM KALMA. Bileşimde silme başarılır, yazma
        // patlar ve arada birim hiçbir hücrede durmayan bir hayalete döner.
        //
        // REDDEDILEN - UnitGrid.cs:134 yerine:
        //     RemoveUnit(fromX, fromY);  sonra  PlaceUnit(toX, toY, moving);
        // KIRILAN  : silme başarılır, yazma patlar, birim hiçbir hücrede kalmaz.
        //            hedef tahta dışı -> birim tahtadan TAMAMEN silinmiş kalır
        //            kaynak hücre boş -> hedefe null yazılır, oradaki birim gider
        //            derleyici: hiçbir şey der  .  test: ThrowsAndKeepsUnit ve
        //            LeavesDestinationUntouched kırmızıya döner
        // KAZANIRDI: ayrılma ve varış GERÇEKTEN iki ayrı olaysa — birim çıkarken
        //            hücreye bir iz (kontrol alanı, tuzak tetiği) bırakıyor ve
        //            varışta ayrı bir şey tetikliyorsa.
        // TEK CUMLE: İki adımın ARASINDA geçerli olmayan bir tahta hâli varsa o iki
        //            adım tek bir işlemdir; bileşim yarım kalmayı mümkün kılar.
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
            cells[fromX, fromY] = null;
            cells[toX, toY] = moving;
        }

        // Üç yazma metodunun ortak sınır kontrolü. Parametre adı dışarıdan
        // geliyor çünkü fırlatılan exception hangi ARGÜMANIN suçlu olduğunu
        // söylemeli: MoveUnit'te "x" değil "toX" yazmalı.
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

        // Nullable-return şekli (FindUnit) uygulandı, ölçüldü ve kaldırıldı:
        // <Nullable> KAPALI olduğu için null dönüş derleme zamanında hiçbir
        // koruma taşımaz — çağıran null kontrolünü unutursa derleyici susar.
        // Bu şekil ise çağıranı dallanmaya ZORLAR.
        //
        // DÜRÜST NOT — bu bir KARAR değil, bir VARSAYILAN: <Nullable> hiç
        // açılmadı. Unity 2021.3'te varsayılan `disable`'dır ve bu projede onu
        // açacak hiçbir şey yok (ne Assets/csc.rsp, ne asmdef alanı, ne
        // ProjectSettings). Yani "kapattık" demek yanlış olur — dokunmadık.
        // Açmanın maliyeti katmana göre değişir: Core'da (noEngineReferences)
        // ucuzdur çünkü ortada anotasyonsuz Unity API'si yok; Unity katmanında
        // ise GetComponent/Find gibi çağrılar anotasyonsuz olduğu için uyarı
        // gürültüsü üretir. Yani "Core'da aç, Unity'de açma" gerçek bir seçenek.
        //
        // REDDEDILEN - UnitGrid.cs:195 yerine:
        //     public Unit FindUnit(int x, int y)   // hücre boşsa null döner
        // KIRILAN  : <Nullable> kapalıyken null dönüş HİÇBİR derleyici koruması
        //            taşımaz; dallanmaya zorlayan tek şey out parametresidir.
        //            null kontrolü unutulur -> unit.Name yazılır -> Play'de patlar
        //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
        // KAZANIRDI: <Nullable> açılırsa — o gün Unit? dönüşü gerçek bir koruma
        //            TAŞIR; ayrıca sonuç bir DALLANMADA değil bir İFADE içinde
        //            gerekiyorsa (FindUnit(x, y)?.Name ?? "boş") null dönüş kazanır.
        // TEK CUMLE: Try deseni bir üslup değil, derleyicinin veremediği "önce sor"
        //            zorunluluğunu imzanın kendisine yazmanın tek yoludur.
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
