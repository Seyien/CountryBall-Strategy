using System;
using GridStrategy.Battle;
using GridStrategy.Combat;
using GridStrategy.Core;
using UnityEngine;

namespace GridStrategy.Unity
{
    // ═══ ROL: SÖZLEŞME (Seam) ════════════════════════════════════════
    // kimlik : yok — arayüzün örneği olmaz; kimliği UYGULAYAN taşır
    // hafıza : yok — hiçbir üyesi bir şey saklamaz; saklayan taraf
    //          uygulayandır ve bugün o taraf tahtayı zaten tutuyor
    // Unity  : gerekmez ama VAR — Vector2 bir UnityEngine tipidir ve bu
    //          arayüz ekran noktası konuşmak zorunda; işte bu yüzden
    //          dosya Unity katmanında, çekirdekte değil
    // karar  : vermez — motor tarafındaki tahtadan NE İSTENDİĞİNİ
    //          adlandırır; nasıl yapıldığı uygulayanın işi
    /// <summary>
    /// Üretim ve yerleştirme katmanının tahtadan istediği HER ŞEY.
    ///
    /// <b>Bu arayüz var olduğu için <see cref="ProductionDirector"/> tahtanın
    /// somut tipini hiç görmüyor</b> — ve bu bir soyutlama merakı değil, bir
    /// SAHİPLİK sınırıdır: tahtayı çizen dosya başka bir hattın malı ve o
    /// dosyaya dokunulmadan bu katman yazılabilmeliydi.
    ///
    /// Uygulayan tek tip <c>BoardAdapter</c>; bağlanma 2026-08-25'te yapıldı.
    /// Tek aday olması tesadüf değil: sözleşmenin her üyesi tahtanın KENDİ
    /// alanına bakıyor, yani başka bir tip bunları cevaplayamaz. Alan
    /// atanmazsa <see cref="ProductionDirector"/> konsola bağırır.
    ///
    /// AYNA BELGE: bu tipin gerekçeleri bugün yalnızca bu dosyada.
    /// </summary>
    // ARAYÜZ, SOYUT SINIF DEĞİL: uygulayacak tip zaten bir MonoBehaviour ve C#
    // tek kalıtıma izin verir. Soyut sınıf yazsaydık tahtanın motor bileşeni
    // olması ile bu sözleşmeyi taşıması arasında seçim yapmak gerekirdi.
    // DAR TUTULDU: aşağıdaki sekiz üyenin her birinin bugün en az bir çağıranı
    // var. "İleride lazım olur" diye tek bir üye eklenmedi, çünkü uygulanmayan
    // bir üye uygulayanı yalan bir söze zorlar.
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
        /// <returns>Hücre dolu ya da tahta dışıysa false.</returns>
        // DÖNÜŞ bool, PlacementOutcome DEĞİL — ve asimetri bilerek: yapı
        // yerleştirmenin ret sebepleri bir OYUN eylemidir ve adlandırılmıştır,
        // birim doğurmanın ret sebebi ise bu noktada tek bir olgudur (hücre
        // uygun değil) çünkü geri kalan bütün retler zaten ProductionRules
        // tarafından, bu çağrıdan ÖNCE verilmiştir.
        bool PlaceUnit(Unit identity, Combatant combatant, int x, int y);

        /// <summary>
        /// Yerleştirme önizlemesini gösterir ya da gizler.
        /// </summary>
        // YENİ BİR MEKANİZMA DEĞİL, VAR OLANIN KAPISI: tahtada bir yerleştirme
        // hayaleti ZATEN var ve klavyeyle açılan yerleştirme kipinde
        // kullanılıyor. Sürükleme sırasında ikinci bir önizleme çizmek, aynı
        // işi yapan iki nesne demek olurdu.
        void SetPlacementGhost(bool visible, int x, int y);

        /// <summary>
        /// Bir kimliğin karşılığı olan yapıyı verir; o kimlik bir yapı değilse
        /// false.
        /// </summary>
        // SEÇİLEN ŞEYİN YAPI OLUP OLMADIĞINI SORAN TEK YOL BU. Tahta zaten
        // birimleri ve yapıları ayrı defterlerde tutuyor; buraya ikinci bir
        // "bu bir yapı mı" bayrağı koymak o ayrımın ikinci bir kopyasını
        // üretirdi.
        bool TryGetStructure(Unit identity, out Structure structure);
    }
}
