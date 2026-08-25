# Tahtanın tek sahibi — ikinci yazarın nasıl doğmadığı

> **NEREDE GEÇİYOR** — *bu mekanizmanın kat ettiği kaynak dosyalar, akış sırasıyla:*
> `Assets/Game/Battle/Battle.cs` → `Assets/Game/Core/UnitGrid.cs`
> → `Assets/Game/Unity/BoardAdapter.cs` → `Assets/Game/Battle/BattleActions.cs`
>
> **NE ZAMAN OKU** — *hangi soruyu sorduğunda ya da hangi değişikliğe giriştiğinde:*
> `Battle`'ın kurucusuna bir `UnitGrid` parametresi eklemek istediğinde, `Board`'u
> `public` yapmayı düşündüğünde, ya da "zaten `readonly`, koruma var" dediğinde.

**BURAYA KODDAN GELDİYSEN** — aşağıdaki üyelerin **yorumunda** bu belgeye bir
`DERİN ANLATIM:` işaretçisi var. Yol: `Ctrl+P` → dosya adının ayırt edici
parçasını yaz → `Ctrl+F` ile **üye adını** ara. ***Satır numarası bilerek
yazılmıyor: satır kayar, üye adı kaymaz.***

| dosya | üye | koddan işaretçi |
|---|---|---|
| `Assets/Game/Battle/Battle.cs` | `board` (alan) · `Battle(int, int)` (kurucu) · `Board` (üye) | ✓ |
| `Assets/Game/Core/UnitGrid.cs` | `PlaceUnit` · `TryGetUnit` | ✓ |
| `Assets/Game/Battle/BattleActions.cs` | `Move` — ██ `battle.Board`'un tüm projedeki tek çağırısı ██ | ✓ |
| `Assets/Game/Unity/BoardAdapter.cs` | `battle` (alan — silinen `UnitGrid` alanının yeri) | ✓ |

---

## Sahne

Oyuncu bir askeri seçiyor, boş bir hücreye tıklıyor. Asker oraya yürüyor.

Tahtada tam olarak iki şey değişti: bir hücre boşaldı, bir hücre doldu. Ve bu
yazmayı yapabilen **tek bir kod yolu** var.

"Tek yol" cümlesi çoğu projede bir disiplin sözüdür — birinin hatırlamasına
bağlıdır, birinin unuttuğu gün sessizce düşer. Burada öyle değil. Bu dosya o
cümlenin nasıl bir disiplin olmaktan çıkıp bir **imkânsızlığa** dönüştüğünü
anlatıyor — ve dürüst olmak gerekirse, tam olarak nerede yeniden disipline
döndüğünü.

Çünkü dönüyor. Sonuna kadar oku.

---

## Karakterler

Dört tip var. Hikâyeyi ilginç kılan, üçüncüsünün **bilmediği** şey.

```
╔═ UnitGrid ════════════════════════════════════════════════════╗
║  İşi     : hangi hücrede kim duruyor — tahtanın kendisi       ║
║  Bilir   : kendi ölçüsü, hücre içerikleri, sınırın yeri       ║
║  BİLMEZ  : savaşı. Canı. Tarafı. Sırayı. Kendi SAHİBİNİ.      ║
╚═══════════════════════════════════════════════════════════════╝

╔═ Battle ══════════════════════════════════════════════════════╗
║  İşi     : eşleştirmek — kim nerede, kimin karşılığı hangi    ║
║            savaş parçası                                      ║
║  Bilir   : tahtayı (KENDİ kurduğu), iki sözlüğü, sırayı       ║
║  BİLMEZ  : tek bir oyun kuralı. Menzili, hasarı, ekranı.      ║
╚═══════════════════════════════════════════════════════════════╝

╔═ BoardAdapter ════════════════════════════════════════════════╗
║  İşi     : çevirmenlik. Piksel ile hücre arasında             ║
║  Bilir   : GameObject, Sprite, Grid, fare, prefab             ║
║  BİLMEZ  : >> TAHTAYI << — ve bir zamanlar biliyordu          ║
╚═══════════════════════════════════════════════════════════════╝

╔═ BattleActions ═══════════════════════════════════════════════╗
║  İşi     : akış. Kuralları sırayla sorup sonucu döndürmek     ║
║  Bilir   : savaşı, kuralları — ve tahtayı GÖREBİLİYOR         ║
║  BİLMEZ  : —  ◄── >> garantinin bittiği yer burası <<         ║
╚═══════════════════════════════════════════════════════════════╝
```

### ***KUTULARIN GERÇEK SATIR KARŞILIĞI***

Dört kutunun dördü de bu projede yaşayan tip. Aşağıda her biri için tanımın
yeri, iddiayı karşılayan **gerçek satır**, ve kutunun hangi ifadesinin o satırda
okunduğu duruyor. ***Satır numarası bilerek yazılmıyor: satır kayar, üye adı
kaymaz.***

**`UnitGrid` bu projede** — `Assets/Game/Core/UnitGrid.cs` → `cells`

```csharp
        private readonly Unit[,] cells;
```

Kutudaki «Bilir : kendi ölçüsü, hücre içerikleri, sınırın yeri» ve «BİLMEZ : …
Kendi SAHİBİNİ» satırlarının **ikisi de** bu tek alanda okunuyor: tipin başka
alanı yok. İçerik bu dizide; ölçü ayrı bir alanda tutulmuyor, `Width` ile
`Height` bu diziden okuyor; ve dizinin yanında "sahibim şu" diye yazılacak
ikinci bir alan yok. Bilmemek bir eksiklik değil, **yazılacak yerin hiç
açılmamış olması**.

**`Battle` bu projede** — `Assets/Game/Battle/Battle.cs` → `Battle(int, int)`

```csharp
        public Battle(int width, int height)
        {
            board = new UnitGrid(width, height);
```

Kutudaki «Bilir : tahtayı (KENDİ kurduğu)» satırının parantez içi kısmı tam
olarak bu üç satır: kurucunun parametre listesinde `UnitGrid` **yok**, iki `int`
var, ve nesne kurucunun **içinde** doğuyor. Parantezdeki "KENDİ" kelimesi bir
vurgu değil, bu `new`'in yerinin adı.

**`BoardAdapter` bu projede** — `Assets/Game/Unity/BoardAdapter.cs` → `battle`

```csharp
        // Tahtanın ve savaşın durumu BURADA DEĞİL, Battle'ın içinde yaşar; bu
        // alan yalnızca o bütüne bir tutamaktır. Burada bir UnitGrid alanı vardı
        // ve silinmesi bu dosyanın en pahalı satırı: tahtaya yazan tek yol artık
        // Battle.AddUnit. → BoardAdapter.md#battle
        // DERİN ANLATIM: Docs/deep/konular/03-tahta-sahipligi.md
        private Battle battle;
```

Kutudaki «BİLMEZ : ***TAHTAYI*** — ve bir zamanlar biliyordu» satırının **iki
yarısı da** bu blokta yazılı: bugünkü tek alan `Battle`, ve silinen `UnitGrid`
alanının bir zamanlar burada durduğu kodun kendi yorumunda kayıtlı. Kutunun "bir
zamanlar" dediği şey bu belgenin hatırlaması değil, kaynağın kendi kaydı.

**`BattleActions` bu projede** — `Assets/Game/Battle/BattleActions.cs` → `Move`

```csharp
            MoveOutcome outcome =
                MoveAction.Execute(battle.Board, unit, fromX, fromY, toX, toY, profile);
```

Kutudaki «tahtayı GÖREBİLİYOR» ve «BİLMEZ : — ◄── ***garantinin bittiği yer
burası***» satırlarının ikisi de tek bir ifadede toplanıyor: `battle.Board`.
`Board` üyesi `internal`, yani kapı yalnız aynı assembly'ye açık — ama açık. Bu
ifade projedeki **tek** `battle.Board` çağrısı; ikincisini yazan bir satır
bugün yok ve onu engelleyen bir şey de yok. Kutunun BİLMEZ hanesinin boş olması
bu yüzden bir övgü değil, bir **uyarı**.

En tuhafı üçüncüsü: **`BoardAdapter` tahtayı bilmiyor.**

Bu bir eksiklik değil, kapanmış bir borç. O tipte bir `private UnitGrid board;`
alanı **vardı** ve `Awake` onu kendisi kuruyordu. Alan silindi. `BoardAdapter`'ın
sınıf özetinde bugün şu cümle duruyor: *"TAHTA ARTIK BURADA DEĞİL."*

**Bütün hikâye o silinen alandan doğuyor.** Aklında tut.

---

## Birinci durak: referans vermek kopyalamaz

`Battle`'ın kurucusu iki türlü yazılabilirdi. Fark tek satır:

```csharp
public Battle(UnitGrid board)        // ✗ reddedildi
public Battle(int width, int height) // ✓ seçildi
```

Reddedilenin neden reddedildiğini anlamak için tek bir dil olgusunu görmek
yeterli: **`UnitGrid` bir `sealed class`.** Yani referans tipi. Bir referansı
parametre olarak vermek nesneyi kopyalamaz — yalnızca **ikinci bir ok** açar, ve
verenin oku silinmez.

```
  REDDEDILEN — Battle(UnitGrid board)
  ┌─BoardAdapter─┐                   ┌────Battle────┐
  │ board ───────┼────┐       ┌──────┼─ board       │
  └──────────────┘    ▼       ▼      └──────────────┘
                  ╔══════════════════╗
                  ║ UnitGrid nesnesi ║ ◄── TEK nesne, İKİ ok
                  ╚══════════════════╝     >> İKİSİ DE YAZABİLİR <<

  SEÇİLEN — Battle(int width, int height)
  ┌─BoardAdapter─┐                   ┌────Battle────┐
  │  (alan YOK)  │                   │ board ───────┼──┐
  └──────────────┘                   └──────────────┘  ▼
                                 ╔═══════════════════════╗
                                 ║ nesne KURUCUDA doğdu  ║
                                 ║ >> dışarıda ok HİÇ    ║
                                 ║ VAR OLMADI <<         ║
                                 ╚═══════════════════════╝
```

Ayrışma noktası okların sayısı değil, **ikinci okun statüsü**. Soldaki şekilde
ikinci ok *engellenmiyor*; sağdaki şekilde *doğmuyor*.

**Fark bir yasak değil, bir imkânsızlık.** Engellenen bir şey unutulabilir;
doğmayan şey unutulamaz.

### Reddedilen imza seçilseydi ne olurdu

Hikâye şöyle giderdi. `BoardAdapter` elindeki kendi okundan tahtaya doğrudan
yazar:

```csharp
board.PlaceUnit(x, y, unit);      // tahtaya girdi
```

Tahtada bir `Unit` duruyor. Ekranda görseli var. Tıklanabiliyor. Ama
`Battle.combatants` sözlüğünde o `Unit`'in karşılığı **yok** — çünkü kayda giden <!-- YOK-MUAF · KAPSAM DIŞI · çalışma anı olgusu, gerekçe aşağıda -->
tek yol `Battle.AddUnit`'ti ve o yol atlandı.

Sonra oyuncu ona saldırıyor:

```
BattleActions.Attack
   └─► RequireCombatant(battle, unit, ...)
         └─► battle.TryGetCombatant → false
               └─► ArgumentException: "The unit is not in this battle."
```

Ekranda duran, tıklanabilen, hedeflenebilen bir asker — ve savaşta olmadığını
söyleyen bir istisna. Derleyici bu ayrışmayı **gösteremez**. Testler yeşil kalır,
çünkü testler `Battle`'ı doğrudan kurar ve yan kapıdan hiç geçmez.

> ***BEŞ ALAN BAĞLAMIYOR — KAPSAM DIŞI***
>
> **NEDEN SENET YAZILMADI:** Bu bölümün başındaki cümle bir mekanizmanın
> yokluğunu hükmetmiyor. O cümle, reddedilen imza seçilseydi ÇALIŞMA ANINDA
> ortaya çıkacak durumu anlatıyor: tahtaya yan kapıdan giren bir birim kayda hiç
> girmemiş olurdu. Yani anlatılan şey bir tasarım borcu değil, kaçınılmış bir
> dünyanın anlık fotoğrafıdır. Senedin birinci alanı o gün kazanılacak bir
> özellik ister, oysa buradaki tek aday zaten reddedilmiş bir imzadır.
>
> **KAPSAMA NEDEN GİRDİ:** Kapının eşiği yazarın kalın işaretine bakıyor,
> cümlenin kipine bakmıyor. Bu blok o eşiğin bilinen ve yazılı faturasıdır.

### Ve imzanın söylemediği şey

`public Battle(UnitGrid board)` imzası "tahtayı alıyorum" der.

"Sen de tutmaya devam edersen bu savaş sessizce bozulur" **demez**.

Sözleşmenin taşıdığı risk imzada görünmez. Yalnızca bir yorumda yaşayabilir — ve
yorum derlenmez. Reddedilme sebebi tam olarak bu: sözleşmenin yarısı tipin
dışında kalıyor.

---

## İkinci durak: `readonly` burada hiçbir şey korumuyor

Alan bildirimi şöyle:

```csharp
private readonly UnitGrid board;
```

`readonly` görünce refleks olarak "korunmuş" diye okunur. Neyi koruduğuna
bakalım:

```
private readonly UnitGrid board;
          │
          ├── KİLİTLEDİĞİ ŞEY: alanın kendisi
          │      board = new UnitGrid(5, 5);      ✗ derleme hatası
          │
          └── KİLİTLEMEDİĞİ ŞEY: nesnenin içi, ve okun paylaşılması
                 board.PlaceUnit(2, 3, u);        ✓ tamamen serbest
                 board.MoveUnit(0, 0, 4, 4);      ✓ tamamen serbest
                 return board;                    ✓ tamamen serbest
                                                  ▲
                              >> AYRIŞMA NOKTASI <<
                     Bu satır, tam olarak `internal Board`'un
                     yaptığı şey. readonly ona hiç bakmıyor.
```

Üç kırılmanın üçü de `readonly`'nin altından geçiyor. Yani **koruma
`readonly`'den gelmiyor** — "dışarıda hiç referans yok" olgusundan geliyor.

`readonly` yine de yazılı, çünkü ölçüsüz bir maliyeti yok ve niyeti belgeliyor.
Ama korumayı ona yüklemek, bu dosyadaki en pahalı yanlış okuma olurdu.

> **⌨ KODU AÇ:** `Assets/Game/Battle/Battle.cs` → `board` alanı, sonra aynı
> dosyada `Board` üyesi
> **BAK:** alan `private readonly`, üye ise dışarıya **aynı oku** veriyor.
> `readonly` ikinci satırı hiç görmüyor — kilitlediği şey slot, verdiği şey ok.
> **DÖNÜŞ:** bu dosyanın «İkinci durak: `readonly` burada hiçbir şey korumuyor» bölümü

> **▶ ARA DURAK:** [../dil/01-degismezlik-anahtar-kelimeleri.md](../dil/01-degismezlik-anahtar-kelimeleri.md#birinci-durak-readonly-slot-kilitli-icerik-serbest)
> **NEDEN:** yukarıdaki figür `readonly`'nin **neyi** kilitlemediğini gösteriyor
> ama **neden** kilitlemediğini dil düzeyinde vermiyor — "slot" ile "içerik"
> ayrımı bu dosyanın değil, C#'ın kararı. Aynı dosya `readonly`'nin bu projede
> gerçekten koruduğu tek yeri de ölçüyor (`UnitLifecycle`'ın iki `float` alanı).
> **DÖNÜŞ:** bu dosyanın [«Üçüncü durak: sahipliği ayakta tutan üç katman»](#ucuncu-durak-sahipligi-ayakta-tutan-uc-katman) bölümü

> **◀ DÖNÜŞ:** [../dil/01-degismezlik-anahtar-kelimeleri.md](../dil/01-degismezlik-anahtar-kelimeleri.md#bu-projede-readonlynin-gercekten-korudugu-bir-sey-var-mi) — «Bu projede `readonly`'nin gerçekten koruduğu bir şey var mı»dan
> geldiysen artık şunu biliyorsun: `Battle.board`'daki korumanın kaynağı
> `readonly` değil, **dışarıda hiç referans olmaması** olgusu — ve o olguyu üç
> katman birlikte ayakta tutuyor · oraya dön ve gerçekten koruduğu iki `float`
> alanından devam et

---

## Üçüncü durak: sahipliği ayakta tutan üç katman

Üç ayrı mekanizma aynı sözü tutuyor. Aynı güçte değiller — ve sıralamaları
sezginin tersi.

```
  katman                  ne yapıyor                  arkasında kim var
  ────────────────────────────────────────────────────────────────────────
  ① kurucuda `new`        ikinci ok DOĞMAZ            >> hiç kimse <<
                                                       bir olgu, kural değil

  ② `internal Board`      ok assembly duvarını        >> DERLEYİCİ <<
                          aşamaz                       GridStrategy.Unity
                                                       bu üyeyi GÖREMEZ

  ③ `private readonly`    alan yeniden atanamaz       derleyici — ama
                                                       >> YANLIŞ KAPIDA <<
                                                       nöbet tutuyor
                                                       (yukarıya bak)
                          ▲
     >> EN GÜÇLÜ KATMANIN ARKASINDA DERLEYİCİ YOK <<
     ①'in tuttuğu söz, kimsenin `BoardAdapter`'a yeniden bir
     `UnitGrid` alanı EKLEMEMESİNE bağlı değil — böyle bir alanı
     doldurabileceği bir kaynak kalmadığı için.
```

Sıralamanın anlamı şu: **en zayıf katman ③.** Silinse hiçbir şey kırılmaz,
yalnızca niyet kaybolur. **En güçlü katman ①**, ama onu derleyici doğrulamıyor —
o bir tasarım olgusu. Ve ② tam ortada duruyor: derleyicinin gerçekten iş yaptığı
tek yer, ve ①'in ömrünü uzatan şey.

`Board` `public` olsaydı ① aynı gün boşa çıkardı. Silinen alanın geri gelmesi
için bir alan bile gerekmezdi:

```csharp
battle.Board.PlaceUnit(x, y, unit);   // tek satır, alan yok, borç geri
```

---

## Dördüncü durak: garantinin bittiği çizgi

`internal` bir duvar çiziyor. Duvarın planı şöyle:

```
        GridStrategy.Unity        ║        GridStrategy.Battle
   (noEngineReferences: false)    ║      (noEngineReferences: true)
                                  ║
   ┌── BoardAdapter ──────┐       ║      ┌── Battle ────────────┐
   │  private Battle      │───────╫─────►│  private readonly    │
   │  battle;             │       ║      │  UnitGrid board;     │
   │                      │       ║      │                      │
   │  `Board`'u GÖREMEZ   │       ║      │  internal Board      │
   └──────────────────────┘       ║      └──────────▲───────────┘
                                  ║                 │ GÖRÜYOR
              >> DUVAR <<         ║      ┌──────────┴───────────┐
        derleyici burada durur    ║      │   BattleActions      │
                                  ║      │   (aynı assembly)    │
                                  ║      └──────────────────────┘
                                  ║              ▲
                                  ║   >> SÖZ BURADA KODA DEĞİL
                                  ║      DİSİPLİNE DAYANIYOR <<
```

`BattleActions` aynı assembly'de yaşıyor, dolayısıyla `Board`'a erişebiliyor.
Orada "tahtaya yalnız `Battle` yazar" sözünü tutan şey bir derleyici kuralı
değil, o dosyanın kendi disiplini.

Bu bilerek kabul edildi — ve bedava kabul edilmedi. `Board`'un var olma sebebi
tek bir imza:

```csharp
// MoveAction.cs — GridStrategy.Core
public static MoveOutcome Execute(UnitGrid board, Unit unit, ...)
```

Hareketi çözen tip `Core`'da yaşıyor ve **bir `UnitGrid` istiyor.** `Battle` ona
tahtayı uzatamazsa hareket hiç çözülemez. `Board` o uzatmanın tek sebebi.

Ölçülebilir hâli: `Board` üyesinin üretimde **tam olarak bir çağıranı** var.

```
BattleActions.cs:207
    MoveAction.Execute(battle.Board, unit, fromX, fromY, toX, toY, profile);
    >> Board'un tek çağırısı — tüm projede <<
```

Test assembly'si bile göremiyor: `GridStrategy.Battle.EditModeTests`
`GridStrategy.Battle`'a referans veriyor ama projede hiçbir `InternalsVisibleTo`
yok, dolayısıyla `internal` üyeler testlerin de dışında kalıyor.

**Sözleşme assembly duvarında biter.** Bunun ötesi dikkat.

> **▶ ARA DURAK:** [02-assembly-duvari.md](02-assembly-duvari.md#duvarin-engelledigi-sey-gorunurluk)
> **NEDEN:** bu bölümün taşıyıcı cümlesi **assembly** kelimesinin üstünde
> duruyor: garantiyi bitiren çizgi klasör sınırı da değil, ad alanı sınırı da
> değil — `.asmdef`'in çizdiği görünürlük sınırı. O sınırın ne olduğu bu dosyada
> tanımlanmıyor.
> **DÖNÜŞ:** bu dosyanın [«Beşinci durak: tahtanın kendi iç sözleşmesi»](#besinci-durak-tahtanin-kendi-ic-sozlesmesi) bölümü

> **⌨ KODU AÇ:** `Assets/Game/Battle/BattleActions.cs` → `Move`
> **BAK:** `battle.Board` ifadesi **tüm projede** yalnız burada geçiyor. Sayarak
> doğrula: `grep -n 'battle.Board' Assets/Game/Battle/BattleActions.cs` tek satır
> döndürmeli. "Tek yol" cümlesinin ölçüsü bu tek satır.
> **DÖNÜŞ:** bu dosyanın «Dördüncü durak: garantinin bittiği çizgi» bölümü

---

## Beşinci durak: tahtanın kendi iç sözleşmesi

`Battle` tahtaya güveniyor. Neye güvendiğine bakmak gerek — çünkü `UnitGrid`
sorulara iki farklı üslupla cevap veriyor ve ayıran şey teknik değil.

```
                      tahta DIŞI koordinat      hücre BOŞ / DOLU
  ──────────────────────────────────────────────────────────────────────
  PlaceUnit           >> FIRLATIR <<            sessizce üstüne yazar
  RemoveUnit          >> FIRLATIR <<            sessizce hiçbir şey
  MoveUnit            >> FIRLATIR <<            sessizce üstüne yazar
  ──────────────────────────────────────────────────────────────────────
  TryGetUnit             false döner            false / true
  IsInsideGrid           false döner                 —
                              ▲
        >> AYRIŞMA: aynı koordinat, iki ayrı muamele <<
        Ayıran şey koordinat değil YÖN:
           YAZMAK bir niyet bildirir → yanlış koordinat ÇAĞIRAN hatasıdır
           SORMAK  niyet bildirmez   → yanlış koordinat normal bir cevaptır
        Ve kenar taraması her karede tahta dışını sorar; oraya
        istisna koymak oyunun kendi akışını hata sayardı.
```

Üç yazma metodunun ortak kapısı `ThrowIfOutsideGrid` ve **private** — kural bir
kez yazılı, dışarı hiç açılmıyor.

`Battle` bu sözleşmeye **yaslanıyor, kopyalamıyor.** İki yerde görünür:

```
  ThrowIfCannotJoin      sınır kontrolü YAZMAZ   ─► PlaceUnit'e bırakır,
                                                    çünkü o hiçbir hücreye
                                                    dokunmadan patlar
  Battle.RemoveUnit      koordinat TAHTADAN gelir ─► sınır kontrolü asla
                                                    tetiklenmez; yarım
                                                    kalma riski sıfır
```

Aynı devretme alışkanlığı ölçüde de var — ve zinciri görmeye değer:

```
  Battle.Width  ────►  UnitGrid.Width  ────►  cells.GetLength(0)
   (devretme)            (türetme)            >> TEK GERÇEK <<
       │                     │
       └─ kopya YOK          └─ kopya YOK
```

`UnitGrid.Width` bir **alan değil**, diziden okunan bir ifade. Ayrı bir
`readonly int width` tutulsaydı aynı ölçü iki sahipli olurdu ve dizi bir gün
yeniden boyutlandırıldığında alan sessizce eskirdi — tahta "genişliğim 5" derken
4'te reddeden bir şeye dönerdi. Üç halka, sıfır kopya: ayrışacak bir şey yok.

---

## Altıncı durak: çevirmenin soramadığı sorular

`BoardAdapter`'ın elinde tahta olmadığı için tahtaya soracağı her şeyi
`Battle`'a soruyor. Tahtaya ait soruların tam listesi:

```
  BoardAdapter soruyor          Battle devrediyor        cevabı üreten
  ────────────────────────────────────────────────────────────────────────
  battle.Width / Height    ──►  board.Width / Height  ──►  cells.GetLength
  battle.CellCount         ──►  board.CellCount       ──►  Width * Height
  battle.IsInsideGrid(x,y) ──►  board.IsInsideGrid    ──►  dizi sınırları
  battle.TryGetUnit(x,y,…) ──►  board.TryGetUnit      ──►  cells[x,y]
  ────────────────────────────────────────────────────────────────────────
                    >> HEPSİ OKUMA <<
  ────────────────────────────────────────────────────────────────────────
  battle.PlaceUnit(…)      ◄── >> BÖYLE BİR ÜYE YOK <<
  battle.MoveUnit(…)       ◄── >> BÖYLE BİR ÜYE YOK <<
  battle.RemoveUnit(x, y)  ◄── >> BÖYLE BİR ÜYE YOK <<
```

İşaretli boşluk bu dosyanın en önemli figürü.

**`Battle` her OKUMAYI devrediyor, hiçbir YAZMAYI devretmiyor.** Tahtaya yazan
üç üyesi var — `AddUnit`, `AddStructure`, `RemoveUnit` — ve üçü de tahtaya *ve*
bir sözlüğe **aynı çağrıda** yazıyor. Yalnızca tahtaya yazan tek bir üye yok.

Değişmez tam olarak burada yaşıyor: *her kayıtlı parçanın tahtada tam olarak bir
hücresi vardır.* İki kayıt aynı çağrıda doğduğu sürece ayrışamazlar.

Tek istisna hareket: `MoveAction` tahtaya sözlüğe dokunmadan yazıyor. Bu
değişmezi bozmuyor çünkü hareket bir **kayıt** değiştirmiyor, yalnızca bir hücre
değiştiriyor — ve sözlüklerin anahtarı hücre değil `Unit` olduğu için orada
güncellenecek bir şey yok.

---

## Bütün ok haritası tek bakışta

```
  ┌── BoardAdapter (GridStrategy.Unity) ────────────────────────┐
  │                                                             │
  │   battle = new Battle(width, height);   ◄── TARİF verdi,    │
  │        │                                    NESNE değil     │
  │        ▼                                                    │
  │   battle.Width / IsInsideGrid / TryGetUnit   ► OKUMA        │
  │   battle.AddUnit(unit, combatant, x, y)      ► YAZMA        │
  │   BattleActions.Move(battle, unit, x, y, r)  ► YAZMA        │
  │                                                             │
  │   >> bir UnitGrid alanı YOK — silindi <<                    │
  └─────────────────────────────┬───────────────────────────────┘
                                │
  ══════════════ ASSEMBLY DUVARI ═════════════════════════════════
                                │
  ┌── Battle (GridStrategy.Battle) ────────────────────────────┐
  │                                                            │
  │   private readonly UnitGrid board;  ◄── kurucuda `new`     │
  │        ▲                                                   │
  │        │ >> tek ok <<                                      │
  │   internal UnitGrid Board => board;                        │
  │        ▲                                                   │
  └────────┼───────────────────────────────────────────────────┘
           │ görüyor — aynı assembly
  ┌────────┴── BattleActions ──────────────────────────────────┐
  │   MoveAction.Execute(battle.Board, ...)                    │
  │   >> tek çağıran; söz buradan sonra DİSİPLİN <<            │
  └────────────────────────────────────────────────────────────┘
```

**İki katman, iki farklı garanti türü.** Duvarın üstünde derleyici konuşuyor;
altında bir dosyanın kendi sözü.

---

## Kural: bir nesneyi dışarıdan almalı mısın

Bu tasarımdan çıkan ölçüt tek bir soru dizisi. Kendi tipini yazarken sırayla sor:

```
① Bu nesne, tipinin bir DEĞİŞMEZİNİ mi taşıyor?
      HAYIR → parametre olarak al. Konu kapandı.
      EVET  → ②

② Nesne bir REFERANS tipi mi?
      HAYIR (struct / düz veri) → kopyalanır, ikinci ok doğmaz. Kapandı.
      EVET  → ③

③ Veren taraf okunu BIRAKACAK mı — ve bunu kim garanti ediyor?
      derleyici → böyle bir dil özelliği YOK. Bu dal boş.
      yorum     → >> RİSK << yorum derlenmez; sözleşmenin yarısı
                  imzanın dışında kalır
      hiç doğmasın → >> KURUCUDA `new` << — ölçüyü al, nesneyi alma

④ Nesneyi yine de dışarı vermek zorunda mısın?
      hayır → private tut, iş biter.
      evet  → görünürlüğü İHTİYACIN OLAN EN DAR duvara çek.
              aynı assembly yetiyorsa `internal`, `public` DEĞİL —
              ve o duvarın ötesinde garantin bittiğini yaz.
```

Dikkat: `readonly` bu ağacın **hiçbir dalında geçmiyor.** Geçmemesi tesadüf
değil — bu ağaç okların sayısıyla ilgileniyor, alanların değişkenliğiyle değil.

---

## Yanlış hatırlanan dört şey

**"`readonly` nesneyi korur."** Alanı korur. Nesnenin içine yazmayı, nesneyi
dışarı vermeyi, hatta `Board` gibi bir üyeyle onu başka bir tipe uzatmayı hiç
görmez. Bu dosyadaki üç kırılmanın üçü de `readonly`'nin altından geçiyor.

**"`internal` bir gevşemedir; `public` olsa ne fark ederdi."** Fark, tek
derleyici garantisinin kendisi. `public` olduğu gün `BoardAdapter` bir alan bile
tutmadan `battle.Board.PlaceUnit(x, y, unit)` yazabilir — silinen alan tek satır
olarak geri gelir ve bu kez silinecek bir alan bile olmaz.

**"Tahtayı dışarıdan almak yasak."** Yasak değil, bugün gereksiz. Yasak olsaydı
kaçış yolunun yazılı olmasına gerek kalmazdı; aşağıda duruyor.

**"`Battle`, `UnitGrid`'i sarmalıyor — yani bir wrapper."** Değil. Bir
sarmalayıcı üyeleri **olduğu gibi** dışarı açar. Burada okuma açık, yazma kapalı:
tahtanın üç yazma üyesinin hiçbirinin `Battle`'da karşılığı yok. Bu bir
sarmalayıcı değil, bir **kelepçe** — ve iki şekli ayıran şey açılan üye sayısı
değil, hangi yönün açıldığı.

---

## Kaçış yolu: kurulmuş tahtayı devralmak

Reddedilen imzanın haklı çıkacağı bir gün var, ve kodda adı konmuş: **tahta
savaştan önce ve başka bir yerde doluyorsa.** Kayıt dosyasından yüklenen bir
kuşatma. Seviye editöründen gelen hazır dizilim. O gün kurucu "kurulmuş tahtayı
devral"ı reddedemez.

Fatura kaybolmaz — **sahibi değişir.** Üç ödeme şekli var, ikisi pahalı:

```
  (a) kurucu tahtayı KOPYALAYARAK alır
      ├─ bugün UnitGrid'in kopyalayan bir kurucusu YOK; yazılması gerekir
      ├─ `cells` dizisi klonlanır, içindeki `Unit` referansları PAYLAŞILIR
      │   (kimlik kopyalanamaz — kopyalanırsa iki savaş aynı askeri
      │    farklı iki nesne sanar)
      └─ maliyet: yerleştirme başına değil, savaş başına bir dizi tahsisi

  (b) sözleşme "verdikten sonra okunu bırak" der
      └─ >> ve yorum derlenmez << — reddedilme sebebinin ta kendisi

  (c) kurucuya NESNE değil VERİ ver          ◄── >> ucuz olan bu <<
      Battle.Load(BattleSnapshot snapshot)
        ├─ snapshot düz veri: ölçü + (kimlik, parça, x, y) listesi
        ├─ `new UnitGrid`'i yine BU TİP yapar → ① katmanı ayakta kalır
        ├─ her satır için AddUnit çağrılır   → iki kayıt aynı çağrıda doğar
        └─ dışarıda hiçbir zaman bir UnitGrid oku OLMAZ
```

(c)'nin işe yaramasının sebebi ikinci durakta yazılı: **veri ok taşımaz, kopya
taşır.** Bugünkü `Battle(int, int)` zaten bu şeklin en yalın hâli — tarifi tek
bir ölçüden ibaret olan hâli. Kayıt dosyası günü geldiğinde değişecek şey
kurucunun *aldığı şeyin tipi* değil, tarifin *zenginliği*.

Yani kaçış yolu aslında bir kaçış değil: aynı kararın daha büyük bir tarifle
tekrarı.

---

## Bunu okuduktan sonra kodda ne göreceksin

`Battle.cs`'te `board` alanının üstündeki blok kararın kendisini söylüyor:
reddedilen imzayı, kırılma zincirini, üç katmanı ve garantinin sınırını.
`UnitGrid.cs`'te `PlaceUnit` ile `TryGetUnit`'in üstündeki bloklar tahtanın kendi
hata felsefesini. `BoardAdapter.cs`'te `private Battle battle;` alanının üstünde
tek bir cümle: burada bir `UnitGrid` alanı vardı ve silindi.

Zincirin tamamı — silinen alandan `MoveAction`'ın imzasına kadar — burada.

Kodda karar, burada hikâye. İkisi çelişirse **kod kazanır** — orası çalışan
metin, burası anlatı.

---

## ***SIRADAKİ ADIM***

> **▶ SIRADA:** [`../dil/01-degismezlik-anahtar-kelimeleri.md`](../dil/01-degismezlik-anahtar-kelimeleri.md) — okuma yolunun **4.** adımı
> **NEDEN ORASI:** bu dosyanın ikinci durağı `readonly`'nin **korumadığı** şeyi
> gösterdi ama **neden** korumadığını dil düzeyinde vermedi. `dil/01` veriyor —
> ve o dosya zaten buraya geri işaret ediyor. ***İki dosya bilerek bölüşmüş:
> `03` kararı, `dil/01` kelimenin sözleşmesini.***
> **YOL HARİTASI:** [`../../ogrenme/00-okuma-sirasi.md`](../../ogrenme/00-okuma-sirasi.md)
