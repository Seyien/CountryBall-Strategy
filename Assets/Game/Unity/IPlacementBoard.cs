using System;
using GridStrategy.Battle;
using GridStrategy.Combat;
using GridStrategy.Core;
using UnityEngine;

namespace GridStrategy.Unity
{
    /// <summary>
    /// Üretim katmanının tahtadan isteyebileceği şeylerin LİSTESİ.
    ///
    /// EN BASİT HÂLİYLE: bunu bir <b>sipariş formu</b> gibi düşün.
    ///
    /// Oyunda iki taraf var. Biri TAHTA: hücreleri çizen, birimleri tutan,
    /// tıklamayı hücreye çeviren taraf (<c>BoardAdapter</c>). Öteki ÜRETİM
    /// MÜDÜRÜ: oyuncunun paletten sürüklediği binayı alıp "şunu şuraya koy"
    /// diyen taraf (<see cref="ProductionDirector"/>).
    ///
    /// Üretim müdürünün tahtanın nasıl çalıştığını bilmesine gerek yok. Ona
    /// gereken tek şey birkaç soru sorabilmek: <i>"bu hücre boş mu?"</i>,
    /// <i>"şu binayı buraya koyar mısın?"</i>, <i>"önizlemeyi şuraya taşı"</i>.
    /// İşte bu dosya o soruların listesi — fazlası değil.
    ///
    /// NE KAZANDIRIYOR, SOMUT OLARAK: üretim müdürü tahtanın binlerce satırlık
    /// dosyasını değil, bu 13 maddelik listeyi tanıyor. Yani tahtanın içinde ne
    /// değişirse değişsin — hücreler nasıl çizilir, tıklama nasıl okunur,
    /// birimler hangi tabloda tutulur — üretim müdürü etkilenmiyor. Değişiklik
    /// bu listeye dokunmadığı sürece öteki dosya hiç açılmıyor.
    ///
    /// DÜRÜST SINIR: bu ayrım bugün bir DERLEME sınırı değil, bir SÖZ. İki tip
    /// aynı assembly ve aynı namespace içinde; yani üretim müdürü isteseydi
    /// tahtanın somut tipine uzanabilirdi. Onu engelleyen şey derleyici değil,
    /// bu arayüzü kullanma kararı. Gerçek bir duvar isteseydik ikisini ayrı
    /// assembly'lere koymamız gerekirdi.
    ///
    /// BUGÜN TEK UYGULAYICISI VAR: <c>BoardAdapter</c>. Tek olması arayüzü
    /// gereksiz kılmıyor ama kazancını da küçültüyor — asıl kazanç ikinci bir
    /// uygulayıcı doğduğunda (örneğin testlerde sahte bir tahta) ortaya çıkar.
    /// </summary>
    // ARAYÜZ, SOYUT SINIF DEĞİL: uygulayacak tip zaten bir MonoBehaviour ve C#
    // tek kalıtıma izin verir. Soyut sınıf yazsaydık tahtanın motor bileşeni
    // olması ile bu sözleşmeyi taşıması arasında seçim yapmak gerekirdi.
    // DAR TUTULDU: aşağıdaki 13 üyenin her birinin bugün en az bir çağıranı
    // var. "İleride lazım olur" diye tek bir üye eklenmedi, çünkü uygulanmayan
    // bir üye uygulayanı yalan bir söze zorlar.
    //
    // ██ SAYI BİR ÖLÇÜDÜR VE BÜYÜDÜĞÜNDE YAZILIR ██
    // Burada "sekiz" yazıyordu ve üye sayısı 13'e çıktığı hâlde öyle kalmıştı.
    // Bayat bir sayı, bayat bir satır atfıyla aynı sınıftan bir KUSURDUR:
    // arayüzün dar kaldığını iddia eden cümle, tam da darlığın ölçüsünü yanlış
    // söylüyordu. Üye eklendiğinde bu iki sayı da güncellenir.
    public interface IPlacementBoard
    {
        /// <summary>
        /// Tahtada bir birim ya da yapı SEÇİLDİĞİNDE haber verir; seçim
        /// kalktığında <c>null</c> ile.
        /// </summary>
        // OLAY, HER KAREDE SORULAN BİR PROPERTY DEĞİL. Alternatif ölçüldü ve
        // reddedildi: "SelectedUnit" diye bir property konsaydı sağ panel her
        // karede onu okuyup bir öncekiyle karşılaştırmak zorunda kalırdı, yani
        // seçimin ikinci bir kopyası doğardı. Seçimin tek sahibi tahtadır.
        event Action<Unit> SelectionChanged;

        /// <summary>
        /// Bir kimlik tahtadan tamamen kaldırıldığında haber verir — ceset ya
        /// da enkaz süresi dolduğunda.
        /// </summary>
        // OKUN YÖNÜ BİR KARARDIR: tahta YAYINLAR, üretim katmanı DİNLER. Ters
        // yön ölçüldü ve reddedildi — tahtanın temizlik döngüsünden doğrudan
        // ProductionDirector çağrılsaydı, tahta bu katmanın tipini tanımak
        // zorunda kalırdı ve bu arayüzün var olma sebebi (tahtanın bu katmanı
        // hiç görmemesi) tam o satırda çökerdi.
        // BU ÜYE OLMASAYDI SESSİZ BİR SIZINTI DOĞARDI: yıkılan her yapının
        // üretim hattı defterde sonsuza dek kalırdı ve kimse "enkazın hattı
        // neden hâlâ sayıyor" diye hata açmazdı — aynı sessizlik enkaz
        // sayacının kendi dosyasında da adı konmuş durumda.
        event Action<Unit> UnitRemoved;

        /// <summary>Ekran noktasını tahta hücresine çevirir.</summary>
        /// <returns>Nokta tahtanın dışına düşüyorsa false.</returns>
        // EKRAN NOKTASI DIŞARIDAN GELİYOR, İÇERİDE OKUNMUYOR: sürükle-bırak
        // olayları fareyi zaten okumuş durumda ve tahtanın ikinci kez okuması
        // parmağın bırakıldığı yer ile sorulan yer arasında bir kare fark
        // açardı.
        bool TryScreenPointToCell(Vector2 screenPoint, out int x, out int y);

        /// <summary>Hücre tahtanın içinde ve boş mu.</summary>
        // BU ÜYE OLMASAYDI SIRA BOZULURDU: üretim, hücre sorusundan ÖNCE
        // yapılamaz — çünkü üretim bekleme sayacını başlatır ve reddedilen bir
        // yerleştirme o sayacı YAKARDI. Oyuncu neden beklediğini anlamazdı.
        bool IsCellFree(int x, int y);

        /// <summary>
        /// Yapıyı tahtaya koyar, savaşa katar ve görselini oluşturur.
        /// </summary>
        // DÖNÜŞ PlacementOutcome, bool DEĞİL: ret sebepleri zaten adlandırılmış
        // durumda ve bu arayüz onları bool'a ezseydi, çağıran oyuncuya hangi
        // cümleyi söyleyeceğini bilemezdi.
        PlacementOutcome PlaceStructure(Unit identity, Structure structure, int x, int y);

        /// <summary>
        /// Savaşçıyı tahtaya koyar, savaşa katar ve görselini oluşturur.
        /// </summary>
        /// <param name="bodySprite">
        /// Üretilen birimin KENDİ gövde görseli; <c>null</c> geçilirse tahta
        /// prefab'daki takım karelerinde kalır.
        /// </param>
        /// <returns>Hücre dolu ya da tahta dışıysa false.</returns>
        // DÖNÜŞ bool, PlacementOutcome DEĞİL — ve asimetri bilerek: yapı
        // yerleştirmenin ret sebepleri bir OYUN eylemidir ve adlandırılmıştır,
        // birim doğurmanın ret sebebi ise bu noktada tek bir olgudur (hücre
        // uygun değil) çünkü geri kalan bütün retler zaten ProductionRules
        // tarafından, bu çağrıdan ÖNCE verilmiştir.
        //
        // SİMGE İÇERİ GİRİYOR, TAHTAYA SORDURULMUYOR — ve gerekçesi
        // SetPlacementVisual'ınkiyle birebir aynı: simge yalnız varlık
        // dosyasında yaşıyor ve çekirdek tanımının içinde taşınamıyor. Tahtaya
        // "bu kimliğin simgesi ne" diye sordurmak, tahtanın varlık defterini
        // tanıması demekti; bu arayüzün var olma sebebi tam o satırda çökerdi.
        // Ölçüm: bu parametre yokken tahtadaki HER birim aynı piyade
        // görünüyordu, sürüklerken doğru simgeyi gören oyuncu bırakınca
        // başkasını buluyordu.
        bool PlaceUnit(Unit identity, Combatant combatant, int x, int y, Sprite bodySprite);

        /// <summary>
        /// Yerleştirme önizlemesini gösterir ya da gizler.
        /// </summary>
        // YENİ BİR MEKANİZMA DEĞİL, VAR OLANIN KAPISI: tahtada bir yerleştirme
        // hayaleti ZATEN var ve klavyeyle açılan yerleştirme kipinde
        // kullanılıyor. Sürükleme sırasında ikinci bir önizleme çizmek, aynı
        // işi yapan iki nesne demek olurdu.
        void SetPlacementGhost(bool visible, int x, int y);

        /// <summary>
        /// Sıradaki yerleştirmenin hangi binaya ait olduğunu söyler; hem
        /// önizleme hayaleti hem de kurulan bina bu görseli kullanır.
        /// </summary>
        // BU ÜYE OLMASAYDI HER BİNA AYNI GÖRÜNÜRDÜ: tahta, kurduğu yapının
        // sprite'ını hayaletin üstünden okuyordu ve hayaletin sprite'ı sahnede
        // sabit atanmıştı. Oyuncu mavi bir karargâh sürükleyip kırmızı bir depo
        // bırakmış oluyordu.
        void SetPlacementVisual(Sprite sprite);

        /// <summary>
        /// Sıradaki yerleştirmenin görselini VE tahtada kaç hücre kaplayacağını
        /// birlikte söyler; önizleme ile kurulan nesne aynı ölçüyü kullanır.
        /// </summary>
        // ÖLÇÜ SİMGEYLE BİRLİKTE GELİYOR VE AYRI DEĞİL, çünkü ikisi aynı varlık
        // dosyasından okunuyor ve ayrı iki çağrı hâline gelseydi biri
        // gönderilip öteki unutulduğunda oyuncu bir boyutta önizleyip başka bir
        // boyutta bina koyardı — ölçülmüş bir kusurun tam olarak bu biçimi bu
        // turda kapatıldı.
        // SIFIR ÖLÇÜ "tanım söylemiyor" demektir ve tahta kendi varsayılanına
        // düşer; tek argümanlı sürüm de tam olarak bunu yapıyor.
        void SetPlacementVisual(Sprite sprite, float sizeInCells);

        /// <summary>
        /// Bir kimliğin karşılığı olan yapıyı verir; o kimlik bir yapı değilse
        /// false.
        /// </summary>
        // SEÇİLEN ŞEYİN YAPI OLUP OLMADIĞINI SORAN TEK YOL BU. Tahta zaten
        // birimleri ve yapıları ayrı defterlerde tutuyor; buraya ikinci bir
        // "bu bir yapı mı" bayrağı koymak o ayrımın ikinci bir kopyasını
        // üretirdi.
        bool TryGetStructure(Unit identity, out Structure structure);

        /// <summary>
        /// Bu yapının tepesinde bir sonraki üretime kaç saniye kaldığını
        /// gösterir.
        /// </summary>
        /// <param name="identity">Geri sayımı gösterecek yapının kimliği.</param>
        /// <param name="remainingSeconds">Bir sonraki üretime kalan saniye.</param>
        /// <param name="totalSeconds">Bu yapının tam bekleme süresi.</param>
        // ██ BİR SORU DEĞİL, BİR SİPARİŞ — VE OKUN YÖNÜ BU YÜZDEN DEĞİŞMİYOR ██
        // Üstteki iki `Try...` üyesi tahtaya SORU soruyor; bu üye ona İŞ
        // söylüyor ve şekli `SetPlacementGhost` / `SetPlacementVisual` ile
        // birebir aynı. Sebebi sahiplik: sayacın doğruluğu üretim müdürünün
        // (`StructureProduction` onun defterinde), o sayının nasıl ÇİZİLECEĞİ
        // ise tahtanın — görselleri tutan tablo orada.
        //
        // TERSİ ÖLÇÜLDÜ VE REDDEDİLDİ: tahta her karede müdüre "bu binanın kaç
        // saniyesi kaldı" diye sorabilirdi. O zaman ok ters yöne de akar ve
        // bugün TEK YÖNLÜ olan bağ çift yönlü olurdu — arayüzün bütün kazancı
        // (müdür tahtanın 3700 satırını değil bu listeyi tanıyor) tam olarak o
        // gün biterdi.
        //
        // İKİ SAYI, BİR ORAN DEĞİL: 0,4 oranı hem "5 saniyenin 2'si" hem "2
        // saniyenin 0,8'i" olabilir ve gösterge KAÇ SANİYE kaldığını çiziyor.
        // Oran gönderilseydi bu ayrım kaybolur, iki saniyelik kışla ile beş
        // saniyelik fabrika aynı görünürdü.
        void ShowProductionCountdown(Unit identity, float remainingSeconds, float totalSeconds);

        /// <summary>
        /// İmlecin altındaki hücreyi verir — tahtanın DIŞINDA olsa bile.
        /// </summary>
        // <see cref="TryScreenPointToCell"/> İLE İKİZ VE İKİSİ DE GEREKLİ.
        // Ayrım tek cümlede: BIRAKMA içerideki sürümü sorar (tahta dışına
        // bırakmak bir vazgeçmedir ve hiçbir şey kurulmaz), ÖNİZLEME bunu sorar
        // (dışarıda da bir hayalet çizilmeli, kırmızı olarak).
        //
        // İKİSİNİ TEK ÜYEDE BİRLEŞTİRMEK REDDEDİLDİ:
        //     bool TryScreenPointToCell(Vector2 p, out int x, out int y, bool allowOutside)
        // KIRDIĞI ŞEY: bir `bool` parametresi çağrı yerinde okunmaz —
        // `TryScreenPointToCell(p, out x, out y, true)` satırını okuyan kimse
        // `true`nun ne demek olduğunu bilemez. İki ad, iki anlam.
        bool TryScreenPointToAnyCell(Vector2 screenPoint, out int x, out int y);

        /// <summary>
        /// Bu hücreye bir şey konabilir mi, konamazsa NEDEN konamaz.
        /// </summary>
        // ÜRETİM MÜDÜRÜ BUGÜN ÇAĞIRMIYOR ve üye yine de burada: hayaletin
        // rengini tahta kendi yazıyor. Sözleşmede durmasının sebebi, bırakma
        // dalının aynı soruyu `IsCellFree` üzerinden zaten soruyor olması —
        // ikisi TEK kuraldan besleniyor ve bu üye o kuralın adı.
        PlacementPreview PreviewAt(int x, int y);
    }
}
