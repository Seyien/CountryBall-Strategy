using System.Collections.Generic;
using GridStrategy.Core;
using UnityEngine;

namespace GridStrategy.Unity
{
    // ═══ ROL: KARMA — ÇEVİRMEN + VARLIK (Adapter + Entity) ═══════════
    // kimlik : var — sahnedeki tahta bileşeni; ayrıca board, unitViews ve
    //          selectedUnit'in TEK sahibi, yani durum burada ikamet ediyor
    // hafıza : var — selectedUnit bir OYUN durumudur, çeviri durumu değil;
    //          saf bir çevirmenin taşımaması gereken şey tam olarak budur
    // Unity  : zorunlu — Input, Camera, Instantiate, MonoBehaviour
    // karar  : ikisi birden — piksel→hücre çevirisi (çevirmen işi) ile
    //          "aynı anda tek birim seçili" kuralı (varlık işi) aynı tipte
    // KOKU   : evet, ama bilinçli — bu kural bugün Unity'siz test EDİLEMEZ; kural
    //          büyüdüğü gün (çoklu seçim, seçilebilirlik kısıtı) Core'a bir Selection
    //          sahibi çıkmalı. Tek satırlık kural için bugün ayrı katman yalnızca
    //          dolaylılık olurdu — bkz. sınıf yorumundaki "baskısız katman" notu.
    /// <summary>
    /// Unity dünyası ile motordan bağımsız tahta kuralları arasındaki çevirmen.
    /// Kendi kuralı yoktur; her kararı <see cref="UnitGrid"/> nesnesine sorar.
    ///
    /// Birim başına GÖRSEL durum artık burada değil, <see cref="UnitView"/>
    /// içinde yaşıyor - o baskı gerçekten doğdu ve bölündü. Buna karşılık input
    /// okuma ve zemin kurulumu hâlâ burada: ikisi de bağımsız değişme baskısı
    /// üretmedi, baskısız bir katman yalnızca dolaylılık ekler.
    /// </summary>
    [RequireComponent(typeof(Grid))]
    public sealed class BoardAdapter : MonoBehaviour
    {
        [Header("Board size in CELLS, not world units")]
        [SerializeField, Min(1)] private int width = 3;
        [SerializeField, Min(1)] private int height = 5;

        [Header("Terrain sprites - at least one required")]
        [SerializeField] private Sprite[] terrainSprites;

        // Alan tipi GameObject değil UnitView: Inspector artık UnitView TAŞIMAYAN
        // bir prefab'ı kabul etmez. Yani "prefab'a bileşen eklemeyi unuttum"
        // hatası Play'e basmadan, sürükle-bırak anında yakalanır. GameObject
        // tutsaydık aynı hata ancak ilk tıklamada NullReference olarak çıkardı.
        [Header("Unit prefab")]
        [SerializeField] private UnitView unitPrefab;

        // Unity'nin Grid bileşeni SADECE bir koordinat çevirmenidir:
        // hücre indeksi <-> dünya konumu. Hiçbir şey çizmez, kaç hücre
        // olduğunu bilmez, oyun durumu tutmaz. Tuttuğu tek şey ayarlardır
        // (cellSize, cellGap, cellLayout).
        private Grid unityGrid;

        // Tahtanın kuralları ve durumu BURADA yaşar: kaç hücre var ve hangi
        // hücrede kim duruyor. Aynı "grid" kelimesi, iki ayrı sahip.
        private UnitGrid board;

        // Core'daki Unit ile ekrandaki görselini eşleyen tablo.
        //
        // Anahtar neden Unit? Çünkü KONUM sadece board'da yaşasın istiyoruz.
        // Görsel "neredeyim" bilmez; konumu her gerektiğinde board'dan
        // hesaplanır. Alternatifi (GameObject[,] paralel dizi) konumu iki
        // yerde tutardı ve ikisi kayarsa hata sessiz olurdu.
        //
        // Equals/GetHashCode yazmaya gerek yok: Unit bir sınıftır, varsayılan
        // karşılaştırma REFERANS eşitliğidir ve aradığımız zaten tam olarak o
        // nesnenin kendisi. Değer eşitliği ancak "aynı içerikli iki ayrı Unit
        // aynı anahtar sayılsın" istenirse gerekirdi; istemiyoruz.
        //
        // REFAKTÖR NOTU GERÇEKLEŞTİ (seçim çerçevesi): not tam olarak bunu
        // öngörüyordu ve aynen öyle oldu - tablo silinmedi, ANAHTARI değişmedi,
        // yalnızca DEĞER tipi GameObject yerine UnitView oldu. UnitView bu
        // tasarımın yerine geçmedi, üstüne geldi.
        //
        // Kazanılan şey: değer artık "bir nesne" değil, KONUŞULABİLİR bir
        // arayüz. Eskiden seçimi uygulamak için adaptör GetComponent ile
        // görselin içini kurcalıyordu; şimdi view.SetSelected(...) diyor ve
        // çerçevenin bir çocuk nesnede yaşadığını hiç bilmiyor.
        private readonly Dictionary<Unit, UnitView> unitViews =
            new Dictionary<Unit, UnitView>();

        // Şu an seçili birim. null = seçim yok.
        private Unit selectedUnit;

        private void Awake()
        {
            // GetComponent bir SORGUdur: bu GameObject'in bileşen listesinde
            // arar ve bulduğuna referans döner. Hiçbir şey yaratmaz, tekrar
            // çağrılması durumu değiştirmez. Listede bir Grid bulunacağını
            // RequireComponent garanti eder; Grid'i "üreten" o değildir.
            unityGrid = GetComponent<Grid>();
            board = new UnitGrid(width, height);
            BuildCellVisuals();

            // GEÇİCİ: tek bir demo birim. Seçme/hareket özelliği gelince buradan
            // kalkacak ve birimler oyun kurulumundan gelecek.
            if (unitPrefab != null)
            {
                SpawnUnit(1, 2, new Unit("Vanguard"));
            }
            else
            {
                Debug.LogError("[Board] unitPrefab is not assigned. Assign the Unit prefab (it must carry a UnitView component) in the Inspector.", this);
            }
        }

        private void Update()
        {
            // "Down" = SADECE basıldığı karede true. Tuşu basılı tutarsan sonraki
            // karelerde false döner; GetMouseButton (Down'suz) ise basılı olduğu
            // her karede true olurdu. Tek tıklama istiyoruz, o yüzden Down.
            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            ReportClickedCell();
        }

        private void BuildCellVisuals()
        {
            // LogError, Log değil: bu bir PROGRAMCI hatasıdır (kurulum eksik),
            // oyun akışının normal bir sonucu değil. Kırmızıdır ve filtrelenebilir.
            // return ile birlikte gelir: sprite yoksa 15 görünmez GameObject
            // üretmektense gürültüyle durmak yeğdir.
            if (terrainSprites == null || terrainSprites.Length == 0)
            {
                Debug.LogError(
                    "[Board] terrainSprites is empty. Assign at least one Sprite in the Inspector.",
                    this);
                return;
            }

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    CreateCellVisual(x, y);
                }
            }

            Debug.Log($"[Board] built {board.Width}x{board.Height} = {board.CellCount} cells.", this);
        }

        private void CreateCellVisual(int x, int y)
        {
            var cell = new GameObject($"Cell_{x}_{y}");

            // Çıplak "transform" = this.transform, yani BU bileşenin bağlı olduğu
            // GameObject'in Transform'u. Component sınıfından miras gelir.
            // Ebeveyn-çocuk hiyerarşisi GameObject'te değil Transform'da yaşar.
            // Amaç konum değil TOPLU YAŞAM DÖNGÜSÜ: tahtayı yok etmek, gizlemek
            // veya taşımak tek çağrıyla 15 hücreye birden uygulanır.
            cell.transform.SetParent(transform, worldPositionStays: false);

            // Hücrenin MERKEZİ, CellToWorld değil: köşe kullanılsaydı her hücre
            // yarım kare kaymış görünürdü.
            cell.transform.position = unityGrid.GetCellCenterWorld(new Vector3Int(x, y, 0));

            // AddComponent bir MUTASYONdur: her çağrı yeni bir bileşen ekler.
            // GetComponent'in aksine idempotent değildir, bu yüzden kurulum
            // kodunda yaşar; kare başına çalışan bir yere konulamaz.
            var renderer = cell.AddComponent<SpriteRenderer>();

            // SpriteRenderer ÇİZER; sprite ise çizilecek varlıktır. Çizen ile
            // çizilen ayrı şeylerdir.
            renderer.sprite = PickTerrainSprite(x, y);

            // Çizim önceliği: aynı katmanda büyük değer üste çizilir. Zemin 0;
            // üzerine gelen birimler 1 alır ve zeminin üstünde görünür.
            renderer.sortingOrder = 0;
        }

        /// <summary>
        /// Hücre koordinatından zemin sprite'ı seçer.
        /// </summary>
        private Sprite PickTerrainSprite(int x, int y)
        {
            // DETERMİNİSTİK: aynı hücre her Play'de aynı sprite'ı alır.
            // Random olsaydı her çalıştırma farklı görünür ve gördüğün bir hatayı
            // tekrar üretmek imkansızlaşırdı. 7 ve 13 asal sayıdır; çarpanların
            // ortak böleni olmaması düzenli şerit deseni oluşmasını engeller.
            // x ve y döngüden gelir, ikisi de >= 0; negatif olabilseydi sonuç
            // negatif çıkabileceği için Mathf.Abs gerekirdi.
            int index = (x * 7 + y * 13) % terrainSprites.Length;
            return terrainSprites[index];
        }

        /// <summary>
        /// Tahtaya bir birim yerleştirir ve ekrandaki karşılığını doğurur.
        /// </summary>
        private void SpawnUnit(int x, int y, Unit unit)
        {
            // Önce KURAL, sonra görsel. PlaceUnit tahta dışına yazmayı exception
            // ile reddeder; o hata görsel doğmadan patlasın ki ekranda karşılığı
            // olmayan bir birim asla oluşmasın.
            board.PlaceUnit(x, y, unit);

            // Instantiate, prefab dosyasından YENİ bir kopya doğurur. Prefab'ın
            // kendisi sahneye girmez; sahnede duran her zaman bir kopyadır.
            // İkinci parametre ebeveyni verir: hücreler gibi birimler de
            // tahtanın çocuğu olur, böylece tahta yok olunca birlikte gider.
            //
            // Argüman UnitView olduğu için dönüş de UnitView'dır - Instantiate
            // generic'tir ve verdiğin tipi geri verir. Bu yüzden burada tek bir
            // GetComponent yok: kopya doğduğu anda zaten aradığımız tipte.
            //
            // view.name yazmak GameObject'in adını değiştirir; name property'si
            // Component üzerinden GameObject'e iletilir. Ayrı bir isim alanı yok.
            UnitView view = Instantiate(unitPrefab, transform);
            view.name = $"Unit_{unit.Name}_{x}_{y}";
            view.transform.position = unityGrid.GetCellCenterWorld(new Vector3Int(x, y, 0));

            unitViews.Add(unit, view);
        }

        private void ReportClickedCell()
        {
            // Camera.main, "MainCamera" ETİKETLİ kamerayı bulur; "ana kamera"
            // diye bir kavram yoktur, etiket vardır. Etiketli kamera yoksa null
            // döner ve bir sonraki satır patlardı.
            if (Camera.main == null)
            {
                Debug.LogError("[Board] No camera tagged MainCamera in the Scene.", this);
                return;
            }

            // Input.mousePosition EKRAN pikselidir: sol alt (0,0), sağ üst
            // (ekranGenişliği, ekranYüksekliği). Kameranın konumu değildir.
            // ScreenToWorldPoint bu pikseli dünya birimine çevirir ve çeviri
            // KAMERAYA bağlıdır: kamera taşınırsa aynı piksel farklı bir dünya
            // noktasına düşer. Çeviri olmasaydı 1920'lik ve 2560'lık ekranda
            // aynı tıklama farklı hücreyi seçerdi.
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Bir Unity tipinin Core'un diline çevrildiği TEK yer burasıdır.
            // Vector3Int sınırın ötesine geçmez.
            Vector3Int cell = unityGrid.WorldToCell(worldPoint);
            int x = cell.x;
            int y = cell.y;

            // Debug.Log'un ikinci parametresi "context"tir: Console'da bu satıra
            // tıklayınca Unity Hierarchy'de o nesneyi vurgular. Metni değiştirmez.
            // 17 nesneli bir sahnede "bunu kim yazdı?" sorusunu tek tıkla cevaplar.
            //
            // Kural Core'da yaşar; adaptörün işi ona uymak, onu tekrar yazmak değil.
            if (!board.IsInsideGrid(x, y))
            {
                Debug.Log($"[Board] ({x},{y}) is OUTSIDE the {board.Width}x{board.Height} board.", this);
                return;
            }

            if (board.TryGetUnit(x, y, out Unit unit))
            {
                SelectUnit(unit);
                Debug.Log($"[Board] ({x},{y}) holds '{unit.Name}' - SELECTED.", this);
            }
            else
            {
                ClearSelection();
                Debug.Log($"[Board] ({x},{y}) is inside the board and EMPTY.", this);
            }
        }

        /// <summary>
        /// Verilen birimi seçili yapar ve öncekinin seçimini kaldırır.
        /// </summary>
        private void SelectUnit(Unit unit)
        {
            // Önce eskiyi temizle: iki birim aynı anda seçili görünemez.
            // Bu satır olmasaydı her tıklama bir birimin daha çerçevesini açar
            // ve hiçbiri geri kapanmazdı.
            ClearSelection();

            selectedUnit = unit;
            SetSelectionVisual(unit, true);
        }

        /// <summary>
        /// Seçimi kaldırır. Seçim yoksa hiçbir şey yapmaz.
        /// </summary>
        private void ClearSelection()
        {
            if (selectedUnit == null)
            {
                return;
            }

            SetSelectionVisual(selectedUnit, false);
            selectedUnit = null;
        }

        /// <summary>
        /// Bir birimin görseline seçim durumunu iletir.
        /// </summary>
        private void SetSelectionVisual(Unit unit, bool isSelected)
        {
            if (!unitViews.TryGetValue(unit, out UnitView view))
            {
                // Tabloda yoksa bu bir programcı hatasıdır: board'a giren her
                // birim SpawnUnit'ten geçmeli ve tabloya kaydolmalıydı.
                Debug.LogError($"[Board] No view registered for unit '{unit.Name}'.", this);
                return;
            }

            // Eski ApplyTint burada SpriteRenderer'ı bulup color'ını yazıyordu.
            // O yaklaşımın kusuru şuydu: renk ÇARPMA ile uygulandığı için
            // seçim, birimin kendi rengini/faction'ını bozuyordu. Artık birimin
            // kendi SpriteRenderer'ına HİÇ dokunulmuyor - color'ı Color.white
            // kalıyor - ve seçim ayrı bir çerçeve nesnesinde yaşıyor.
            //
            // Adaptör o çerçeveyi görmüyor bile; sadece niyeti söylüyor.
            view.SetSelected(isSelected);
        }
    }
}
