# Değer, referans ve kimlik — "aynı" sözcüğünün dört ayrı ölçüsü

> **HANGİ DİL ARACI** — *bu dosyanın anlattığı, ödünç alınmış adlar:*
> `ReferenceEquals` · `out` · `enum` · `=>` · `switch` · `%`
>
> **NEREDE GEÇİYOR** — *bu araçların bu projede yaşadığı yerler:*
>
> | dosya | üye |
> |---|---|
> | `Assets/Game/Core/MoveAction.cs` | `Execute` |
> | `Assets/Game/Battle/Battle.cs` | `TryGetPosition` · `stateForwarders` |
> | `Assets/Game/Unity/BoardAdapter.cs` | `HandleOccupiedCellClick` · `DespawnView` |
> | `Assets/Game/Core/Combat/AttackProfile.cs` | `AttackProfile` (tip başlığı) |
> | `Assets/Game/Core/UnitGrid.cs` | `TryGetUnit` · `Width` |
> | `Assets/Game/Battle/TurnState.cs` | `Current` · `EndTurn` |
> | `Assets/Game/Unity/UnitView.cs` | `TintFor` |
>
> **NE ZAMAN OKU** — *hangi soruyu sorduğunda ya da hangi değişikliğe giriştiğinde:*
> iki şeyin "aynı" olup olmadığını soran bir satır yazarken; `==` yazıp cevabın
> neden `false` geldiğini anlamadığında; ya da bir `switch`'e yeni bir `case`
> eklerken.

**BURAYA KODDAN GELDİYSEN** — ██ gelemezsin: bu belgeye giden **hiçbir** kod
işaretçisi yok. ██ Ölçüldü — `dil/` ağacının kod işaretçisi `DİL:` etiketiyle
yazılır (`konular/` ağacınınki `DERİN ANLATIM:`), ve `Assets/` altında bu
belgeyi anan **sıfır** `DİL:` satırı var. Yani ok bugün **tek yönlü**: buradan
koda gidilir, koddan buraya gelinmez.

██ Buraya belgeden gelen yol canlı: [`dil/07`](07-bellek-canlilik-ve-yikim.md)
bu dosyayı **açık ön koşul** sayıyor (`dil/05` semantiği anlatır, `dil/07`
depolamayı). ██

Bu dosya projenin kendi kararlarını değil, projenin **ödünç aldığı** dil
özelliklerini anlatıyor. Bunların hiçbirini biz tasarlamadık; ama ne vaat
ettiklerini bilmeden kendi kararlarımızı okuyamayız.

Altı özellik, tek soru: **bir şeyin "aynı" olması ne demek, ve C# bunu nasıl
soruyor.**

---

## Sahne

Oyuncu seçili askerine ikinci kez tıklıyor. Çerçeve kayboluyor, seçim bırakılıyor.

Bunu yapan tek satır `BoardAdapter`'da:

```csharp
if (ReferenceEquals(clicked, selectedUnit))
```

Burada `clicked == selectedUnit` de yazılabilirdi ve bugün **birebir aynı cevabı
verirdi**. İki satırın hangi durumda ayrıştığı, ve neden bu projede birinin
seçilip ötekinin seçilmediği — hikâye bu.

---

## Karakterler

```
╔═ DEĞER TİPİ (int, enum, float, Color, Vector3) ═══════════════╗
║  Ne yapar : atandığı ve geçirildiği her yerde KOPYALANIR       ║
║  Vaadi    : elindeki senin, kimse arkandan değiştiremez        ║
║  BİLMEZ   : nereden kopyalandığını. Kaynağa geri yol YOK.      ║
╚═══════════════════════════════════════════════════════════════╝

╔═ REFERANS TİPİ (class: Unit, UnitGrid, AttackProfile) ════════╗
║  Ne yapar : değişken nesneyi DEĞİL, nesnenin ADRESİNİ tutar    ║
║  Vaadi    : aynı nesneyi iki yerden görebilirsin               ║
║  BİLMEZ   : kaç kişinin elinde olduğunu ██ KRİTİK ██           ║
╚═══════════════════════════════════════════════════════════════╝

╔═ ==  (operatör) ══════════════════════════════════════════════╗
║  Ne yapar : tipin SÖYLEDİĞİ şeyi sorar                         ║
║  Vaadi    : hiçbiri — anlamını tip belirler                    ║
║  BİLMEZ   : senin neyi kastettiğini. AŞIRI YÜKLENEBİLİR.       ║
╚═══════════════════════════════════════════════════════════════╝

╔═ ReferenceEquals (System.Object'in STATIC metodu) ════════════╗
║  Ne yapar : "bu iki ok AYNI kutuyu mu gösteriyor" diye sorar   ║
║  Vaadi    : cevabı hiçbir tip değiştiremez — static, virtual   ║
║             değil, ezilemez                                    ║
║  BİLMEZ   : içeriği. İki özdeş kutuya "farklı" der.            ║
╚═══════════════════════════════════════════════════════════════╝
```

### Dört kutunun GERÇEK SATIRLAR tarafındaki karşılığı

██ Dördü de ÖDÜNÇ, yani aşağıda gösterilen yer **tanım yeri değil KULLANIM
YERİDİR** ██ — değer/referans ayrımını da `==`'i de biz tasarlamadık; onları
çalıştıran satırları biz yazdık.

**DEĞER TİPİ bu projede** — `Assets/Game/Unity/UnitView.cs` → `SetState(UnitState)`

```csharp
bodyRenderer.color = authoredColor * TintFor(state);
```

██ EN ÖĞRETİCİ SEÇİMİ ██ — kutunun saydığı beş tipten üçü tek satırda: `state` bir
`enum`, `authoredColor` ile `TintFor`'un döndürdüğü bir `Color`. Seçilme sebebi
kutunun «BİLMEZ: nereden kopyalandığını. Kaynağa geri yol YOK.» satırının burada
**görünür bir garantiye** dönüşmesi. Kutudaki «atandığı ve geçirildiği her yerde
KOPYALANIR» satırı bu satırın iki ayrı yerinde birden okunuyor: `state`,
`TintFor`'a kopya olarak giriyor ve o gövdede ne yapılırsa yapılsın çağıranın
parametresi kımıldamıyor; `authoredColor` da çarpıma kopya olarak giriyor, bu
yüzden prefab'da yazılı özgün renk her `SetState` çağrısında bozulmadan kalıyor.
Bu iki güvence sözdiziminden değil, tipin değer tipi olmasından geliyor.

**REFERANS TİPİ bu projede** — `Assets/Game/Core/UnitGrid.cs` → `MoveUnit(int, int, int, int)`

```csharp
Unit moving = cells[fromX, fromY];
```

██ EN ÖĞRETİCİ SEÇİMİ ██ — kutunun adlandırdığı üç tipten (`Unit`, `UnitGrid`,
`AttackProfile`) ikisi bu tek metotta buluşuyor. Kutudaki «değişken nesneyi DEĞİL,
nesnenin ADRESİNİ tutar» satırının karşılığı tam bu satır: `moving`'e kopyalanan
şey askerin kendisi değil, ona giden ok. Aynı metodun son iki satırı
(`cells[fromX, fromY] = null;` ve `cells[toX, toY] = moving;`) bunu görünür
kılıyor — hareket eden şey nesne değil **oklar**; `Unit` örneği bellekte hiç
kımıldamıyor. Kutunun «BİLMEZ: kaç kişinin elinde olduğunu ██ KRİTİK ██» satırı da
burada: bu iki satırın arasındaki anda aynı `Unit`'e iki ok bakıyor ve dizinin
bundan haberi yok. ██ Aynı körlüğün bir üst kattaki faturası ██:
[`../konular/03-tahta-sahipligi.md`](../konular/03-tahta-sahipligi.md).

**`==` bu projede** — `Assets/Game/Unity/UnitView.cs` → `Awake()`

```csharp
if (selectionOverlay == null)
```

██ EN ÖĞRETİCİ SEÇİMİ ██ — aynı satır `SetSelected` içinde de duruyor; `Awake`'teki
seçildi, çünkü orada operatörün cevabı bir `LogError`'a bağlanıyor, yani `==`'in ne
söylediği ekranda görünür hâle geliyor. Kutudaki «tipin SÖYLEDİĞİ şeyi sorar»
satırının karşılığı tam bu satır: `SpriteRenderer`, `UnityEngine.Object`'ten türüyor
ve o tip `==`'i AŞIRI YÜKLÜYOR. Bu satır «referans `null` mı» diye sormuyor; «yerel
tarafta bir eş var mı» diye soruyor — Inspector'da atanmamış bir alan da,
`Destroy` edilmiş bir nesne de burada `true` üretir. Kutunun «BİLMEZ: senin neyi
kastettiğini. AŞIRI YÜKLENEBİLİR.» satırı buradan okunuyor: **birebir aynı
sözdizimi** `Unit` üstünde yazılsaydı sıradan bir referans karşılaştırması olurdu.
İki tarafın ayrımı ([`07`](07-bellek-canlilik-ve-yikim.md)) bu satırın altında
yatıyor.

**`ReferenceEquals` bu projede** — `Assets/Game/Unity/BoardAdapter.cs` → `HandleOccupiedCellClick`

```csharp
if (ReferenceEquals(clicked, selectedUnit))
```

██ EN ÖĞRETİCİ SEÇİMİ ██ — üretimde iki çağrı var, ötekisi aynı dosyanın
`DespawnView`'ında; seçilen bu, çünkü bu dosyanın «Sahne» bölümü zaten bu satırı
gösteriyor ve kutunun vaadi burada bir **karara** dönüşüyor. Kutudaki «cevabı
hiçbir tip değiştiremez — static, virtual değil, ezilemez» satırının karşılığı tam
bu satır: `Unit` bir gün `Equals`/`==` ezip adı aynı olan iki birimi "eşit"
sayarsa bu satırın cevabı yine de değişmez, çünkü aranan şey eşitlik değil TAM O
NESNEnin kendisi. Kutunun «BİLMEZ: içeriği» satırı da burada bir kusur değil,
aranan özelliğin ta kendisidir — bir üst satırdaki `==` kutusuyla arasındaki bütün
fark bu.

---

## Birinci durak: kopyalanan mı, paylaşılan mı

Ayrım tek cümlede: **değer tipi kopyalanır, referans tipi paylaşılır.**

**Ölçü — bir metoda ver, içinde değiştir, dışarıda ne olduğuna bak.**
Projede iki örnek yan yana duruyor:

```
board.MoveUnit(fromX, fromY, toX, toY)
      ▲         ▲
      │         └── int'ler: KOPYA gider. MoveUnit içinde ne yaparsa
      │             yapsın, çağıranın fromX'i değişmez.
      │
      └── UnitGrid: ADRES gider. cells[fromX,fromY] = null satırı
          ██ ÇAĞIRANIN tahtasını değiştirir ██
```

`MoveAction`'ın rol başlığındaki cümle tam olarak bunu ölçüyor:

```
MoveAction  ► static, TEK BİR ALANI YOK, hiçbir şey hatırlamıyor

  aynı çağrıyı arka arkaya iki kez yap:
    Execute(board, unit, 0,0, 1,0, moveRange: 1)
       1. çağrı ►  MoveOutcome.Moved
       2. çağrı ►  ArgumentException      ██ FARKLI CEVAP ██

  Hafızasız bir tipin cevabı nasıl değişti?
       └── değişen şey tipin içi değil, ELİNE VERİLEN nesne:
           board bir REFERANS, birim artık (0,0)'da durmuyor
```

Tahtayı sıfırdan kurup aynı çağrıyı tekrarla — ilk cevabı yeniden alırsın.
Fark tipe değil, paylaşılan nesneye ait.

### Değer tipinin en görünür kanıtı: Unity'nin `Vector3`'ü

`Vector3` bir `struct`, yani değer tipi. Sonucu derleyicide görülüyor:

```csharp
cell.transform.position = CellCentre(x, y);   // ✓ BÖYLE yazılı
cell.transform.position.x = 3f;               // ✗ DERLENMEZ (CS1612)
```

**Ölçü:** ikinci satırı yaz, derle. Hata mesajı "dönen değer bir değişken
olmadığı için değiştirilemez" der. Çünkü `position` bir property'dir ve
çağrıldığında `Vector3`'ün **bir kopyasını** döndürür; o kopyanın `x`'ini
değiştirmek hiçbir işe yaramayacağı için derleyici baştan reddeder.

`UnitView`'daki renk hesabı aynı sebeple güvenli:

```csharp
bodyRenderer.color = authoredColor * TintFor(state);
//                   ▲
//    Color da bir struct: çarpma YENİ bir değer üretir,
//    authoredColor'a DOKUNMAZ
```

**Ölçü:** `SetState(Dead)` çağır, sonra `SetState(Alive)` çağır. Renk prefab'daki
değere **birebir** döner. `Color` bir sınıf olsaydı ilk çarpma `authoredColor`'ı
bozabilir ve diriliş soluk bir birim üretirdi.

---

## İkinci durak: `ReferenceEquals` neden `==` değil

### ██ Bu projede yakalanan canlı hata ██

`AttackProfile` kendini şöyle tanımlıyor: *"aynı değerlere sahip iki
AttackProfile birbirinin YERİNE GEÇEBİLİR."* Cümle doğru. Tuzak, okuyucunun onu
sınamak için uzanacağı ilk araçta: `==` **tam tersini** söyler.

```csharp
var a = new AttackProfile(10, 1);
var b = new AttackProfile(10, 1);

a == b                  ►  false   ██ "AYNI ŞEY" denilen yerde ██
a.Equals(b)             ►  false
ReferenceEquals(a, b)   ►  false
```

Sebep `AttackProfile.cs`'te bir kodun **varlığı değil yokluğu**: dosyada
`Equals`, `GetHashCode` ya da `operator ==` geçersiz kılınmıyor. Böyle bir tipte
`==` `System.Object`'ten miras kalan **referans** karşılaştırmasıdır.

```
     a ──► ╔═════════════╗        b ──► ╔═════════════╗
           ║ Damage = 10 ║              ║ Damage = 10 ║
           ║ Range  =  1 ║              ║ Range  =  1 ║
           ╚═════════════╝              ╚═════════════╝
              İKİ AYRI KUTU, aynı içerik
              ██ == içeriğe HİÇ BAKMAZ ██
```

### Doğru ölçü "eşitlik" değil, YERİNE GEÇEBİLİRLİK

```
YANLIŞ ÖLÇÜ    a == b  ►  false
               → "demek ki aynı şey değiller"     ██ SAPMA BURADA ██

DOĞRU ÖLÇÜ     b'yi, a'nın geçtiği HER YERE koy:
               ► AttackResolver aynı cevabı verir
               ► DamageRules aynı hasarı hesaplar
               ► hiçbir çağıranın davranışı değişmez
               = YERİNE GEÇEBİLİR
                 (bu yüzden yüzlerce asker tek örneği paylaşabilir)
```

`AttackProfile.cs`'in rol başlığı bu tuzağı adıyla taşıyor: ölçünün `==`
olmadığını, çünkü `Equals` yazılmadığı için o karşılaştırmanın `false`
döneceğini açıkça yazıyor. Buradaki mekanizma o satırın arkasındaki dil kuralı.

### `ReferenceEquals` ne yapar, kim ezebilir

```
public static bool ReferenceEquals(object objA, object objB)
       ▲                                ▲
       └── STATIC: sanal değil, ezilemez │
                                         └── iki adres, tek soru:
                                             aynı kutu mu?
```

Kimse bir tipe "benim `ReferenceEquals`'ım farklı çalışsın" diyemez.
Ezilebilen `==` ve `Equals`; ezilemeyen `ReferenceEquals`. **Niyet okumak** için
kullanılmasının sebebi bu: satır "değerleri değil, TAM O NESNEYİ soruyorum" diye
bağırıyor ve bu ifade gelecekte kimse tarafından değiştirilemiyor.

Projede üç kullanım, üçü de aynı soruyu soruyor:

```
MoveAction.Execute       kaynakta  ► kimlik ŞART  (yoksa fırlat)
MoveAction.Execute       hedefte   ► kimlik MUAF  (kendisiyse engel değil)
Battle.TryGetPosition              ► tahtayı tarar, TAM O birimi arar
BoardAdapter (2 yerde)             ► seçili birim tam O birim mi
```

`MoveAction` en öğreticisi: **aynı metotta** `ReferenceEquals` iki karşıt şey
istiyor. Kaynak hücrede kimlik tutmuyorsa çağıran hatalıdır; hedef hücrede kimlik
tutuyorsa birim kendi kendine engel olmuyor demektir ve hareket devam eder.

### Bölüşme: `==` nerede DOĞRU seçim, ve neden

```
                        ==                    ReferenceEquals
                        ──                    ───────────────
Unit (düz sınıf)        referans              referans
                        ██ BUGÜN AYNI CEVAP ██

SpriteRenderer          Unity'nin AŞIRI       referans
GameObject              YÜKLEDİĞİ operatör
(UnityEngine.Object)    ██ AYRIŞIRLAR ██
```

`UnityEngine.Object` `operator ==`'i bilerek aşırı yükler: **yok edilmiş** bir
nesne `null`'a eşit sayılır.

```
Destroy(view.gameObject) çağrıldıktan SONRA:

   body == null                 ►  true    ◄── Unity öyle diyor
   ReferenceEquals(body, null)  ►  false   ◄── C# öyle diyor
                                             ██ İKİSİ DE DOĞRU ██

   yönetilen C# sarmalayıcısı hâlâ ayakta duruyor;
   ARKASINDAKİ yerel motor nesnesi yok edildi
```

Bu yüzden `UnitView.Body`'deki `if (body == null)` **`ReferenceEquals`'a
çevrilemez**: orada istenen şey tam olarak Unity'nin cevabıdır.
`BoardAdapter.DespawnView`'daki "önce tablodan çıkar, sonra Destroy et" sırası da
aynı olgunun sonucu — ters sırada tabloda "null gibi ama null değil" bir referans
kalırdı.

**Kapsam sınırı:** `Unit` düz bir C# sınıfıdır, `==`'i aşırı yüklemez; orada iki
satır bugün aynı cevabı verir. `ReferenceEquals` bir hız kazancı için değil, biri
o tipe bir gün `operator ==` yazdığında cümlenin anlamı kaymasın diye seçildi.

---

## Üçüncü durak: `enum` gerçekte nedir

Arka planda bir `int`, üstünde adlar:

```
public enum Team          derleyicinin gördüğü
{                         ────────────────────
    None,     ─────────►  0   ◄── ██ default(Team) BURAYA DÜŞER ██
    Player,   ─────────►  1
    Enemy     ─────────►  2
}
```

**Ölçü:** `(int)Team.Enemy` yaz, `2` basar. Sayı `= 0` diye yazılmış olduğu için
değil, **satır sırası** öyle olduğu için 0/1/2.

Sıfırıncı değerin özel olması buradan geliyor: dilin atanmamış her alana verdiği
değer sıfırdır ve o sıfır, listenin ilk üyesinin adını alır.

```csharp
Team t;              // hiç atanmadı
// t == Team.None    ► true — tarafsız, hiçbir sırada eyleyemez
```

`Team.None` bilerek başta duruyor; `Player` başta olsaydı takımı atanmayı
unutulmuş her birim sessizce oyuncunun tarafında doğardı.

### Adlandırılmamış değerler de geçerlidir

```csharp
UnitState s = (UnitState)99;   // ██ DERLENİR, ÇALIŞIR, PATLAMAZ ██
```

Bir `enum` değişkeni, arkasındaki `int`'in alabildiği **her** değeri alabilir.
99'un bir adı yok ama tip sistemi onu reddetmiyor. Bu, aşağıdaki `switch`
durağının tam sebebi: derleyici hiçbir zaman "bütün değerler işlendi" diyemez,
çünkü değerlerin sonu yok — yalnızca **bildirilmiş adların** sonu var.

> **Bu enum'ların PROJE tarafı** — sıfırıncı değerin neden ret olduğu, dört sonuç
> enum'unun neden tek tipe indirilmediği, bir asmdef'in bir enum değerini nasıl
> üretilemez kıldığı: `Docs/deep/konular/06-sonuc-enumlari.md`. Burada yalnız dil
> kuralı var, orada kararlar.

---

## Dördüncü durak: `out` parametre

```
public bool TryGetUnit(int x, int y, out Unit unit)
                                     ▲
                    çağıranın DEĞİŞKENİNE yazma izni

ÇAĞIRAN                                METOT
───────                                ─────
Unit u;                                unit = null;        (dal 1)
board.TryGetUnit(9, 9, out u);         unit = cells[x,y];  (dal 2)
      ██ u ATANMIŞ döner ██            ██ HER dalda ██
```

**Ölçü — derleyici zorlar:** `UnitGrid.TryGetUnit`'in ilk dalındaki
`unit = null;` satırını sil ve derle. `CS0177` alırsın: *"out parametresi 'unit',
metottan çıkılmadan önce atanmalıdır."* Bu bir çalışma anı hatası değil,
**derleme** hatasıdır — metodun hiçbir yolu `out`'u atamadan bitiremez.

### Neden `bool` + `out`, neden nullable dönüş değil

`UnitGrid` bir zamanlar `FindUnit` diye nullable dönen bir metot taşıyordu;
uygulandı ve **kaldırıldı**. Gerekçe kodda yazılı ve iki parçalı:

```
① <Nullable> KAPALI  ► `Unit?` diye bir söz derleme zamanında hiçbir şey
                       korumaz; unutan çağıran uyarı bile almaz

② ŞEKLİN GÖRÜNÜRLÜĞÜ ► `out` çağrı yerinde GÖZE ÇARPAR:
                       if (board.TryGetUnit(x, y, out Unit u))
                          └── dallanma cümlenin içine yazılmış
```

**Dürüst sınır:** koruma `out`'un dilsel gücünden gelmiyor. `out` yalnızca
değişkenin **atanmış** olmasını garanti eder, **anlamlı** olmasını değil:

```csharp
board.TryGetUnit(9, 9, out Unit u);   // ✓ derlenir, bool ATILDI
// u == null — hiçbir uyarı yok
```

Yani `out` "kontrol etmeyi unutamazsın" demez; "değişkenin çöp değerle
kalmayacak" der. Geri kalanı şekil disiplini.

### İki küçük sözdizimi notu

```csharp
if (board.TryGetUnit(toX, toY, out Unit occupant))     // yerinde bildirim
if (!TryReadPointerCell(out _, out _, out int x, out int y))
//                      ▲
//        ATIK (discard): "atanacak ama umurumda değil"
```

`out _` bir değişken değil; derleyiciye "buraya bir yer ayır ve adını bana
sorma" demenin yolu. `BoardAdapter.HandleClick` dünya koordinatlarını böyle
atıyor — o akışın ihtiyacı olan tek şey hücre indeksi.

---

## Beşinci durak: `=>` — aynı simge, iki ayrı iş

```
                    =>
                    │
      ┌─────────────┴─────────────┐
      │                           │
① ÜYE GÖVDESİ               ② LAMBDA
  (expression-bodied)         (delege üretir)

public int Width            Action<UnitState, UnitState> f =
    => cells.GetLength(0);      (previous, next) => ...;

"bu üyenin gövdesi          "burada bir NESNE doğuyor;
 şu ifadedir"                onun kimliği var"
```

İkisinin ortak yanı yalnızca simge. Karıştırmanın bedeli beşinci durağın ikinci
yarısında.

### ① Expression-bodied üye — alan DEĞİL, her okumada yeniden hesaplanır

```csharp
public int Width => cells.GetLength(0);       // UnitGrid
public int CellCount => Width * Height;
public Team Current => order[index];          // TurnState
```

**Ölçü 1 — atanamaz.** `board.Width = 5;` yaz, derle: `CS0200`, *"property salt
okunur olduğu için atanamaz."* Sınıfın **içinden** de atanamaz; ortada yazılacak
bir alan yok.

**Ölçü 2 — hafızası yok, her okumada koşar.** `CellCount`'u bir kez oku: iki
property çağrısı, iki `GetLength` çağrısı olur. `Width` bir alan olsaydı bir
okuma bir bellek erişimi olurdu.

**Ölçü 3 — kimse atamadan değişir.** `TurnState.Current`'ı oku (`Player`),
`EndTurn()` çağır, tekrar oku (`Enemy`). Arada `Current`'a hiç kimse yazmadı;
değişen `index`.

**Aynı dosyadan karşı örnek** — `TurnState`'te `=>` OLMAYAN bir property:

```csharp
public int TurnNumber { get; private set; }   // gerçek bir alanı VAR
...
TurnNumber++;                                 // ✓ içeriden atanabilir
```

`Current` içeriden bile atanamaz, `TurnNumber` içeriden atanabilir. İkisi de
"dışarıdan salt okunur" görünüyor, ikisinin **doğası** farklı.

### ② Lambda — bir DELEGENİN KİMLİĞİ vardır

Burası bu dosyanın konusuna geri bağlanan yer. Bir lambda bir nesne üretir ve o
nesnenin kimliği vardır:

```csharp
Action<UnitState, UnitState> forwarder =
    (previous, next) => UnitStateChanged?.Invoke(unit, previous, next);
combatant.StateChanged += forwarder;
stateForwarders.Add(unit, forwarder);   // ██ SAKLANMAK ZORUNDA ██
```

**Ölçü:** aboneliği sökmek için aynı metni ikinci kez yaz:

```csharp
combatant.StateChanged -= (p, n) => UnitStateChanged?.Invoke(unit, p, n);
// ► HİÇBİR ŞEY OLMAZ. Abonelik ayakta kalır, hata mesajı da yok.
```

```
   `unit`'i YAKALAYAN her lambda değerlendirmesi
   yeni bir kapanış nesnesi doğurur:

      1. yazım ──► ╔═ kapanış #1 ═╗ ◄── += ile eklenen BU
                   ║ unit = Asker ║
                   ╚══════════════╝
      2. yazım ──► ╔═ kapanış #2 ═╗ ◄── -= bunu arıyor, listede YOK
                   ║ unit = Asker ║
                   ╚══════════════╝
                     ██ AYNI METİN, AYRI NESNE ██
```

`Battle.stateForwarders` sözlüğü bu yüzden var — bir konfor değil, zorunluluk.
Kodda yazılı hâliyle: *"kapanışlar birbirine eşit değildir — aynı metni ikinci
kez yazarak abonelik ÇÖZÜLEMEZ."*

**Kapsam:** hiçbir şey yakalamayan bir lambda için derleyici tek bir delegeyi
önbelleğe alabilir ve `-=` şans eseri çalışabilir. Bu bir derleyici
optimizasyonu, bir dil garantisi değil — kural her zaman aynı: **söktüğün şeyin
referansını sakla.**

> Bu aboneliğin dört durağı ve sökülmezse önce neyin patladığı:
> `Docs/deep/konular/01-olay-zinciri.md`.

---

## Altıncı durak: `switch` deyimi eksik dal için UYARMAZ

```
   switch (outcome)                  switch DEYİMİ (statement)
   {                                 ─────────────────────────
       case A: ... break;            enum'a yeni değer eklendi:
       case B: ... break;              derleyici : ██ SUSAR ██
       // yeni değer C yok             çalışma anı: hiçbir dal tutmaz,
   }                                               ██ SESSİZCE GEÇİLİR ██

   var x = outcome switch            switch İFADESİ (expression)
   {                                 ─────────────────────────
       A => ...,                     enum'a yeni değer eklendi:
       B => ...,                       derleyici : ██ CS8509 UYARISI ██
   };                                  çalışma anı: SwitchExpressionException
```

Farkın sebebi keyfî değil: bir **ifade** bir değer üretmek zorundadır ve hiçbir
dal tutmazsa ortada üretilecek değer yoktur. Bir **deyim** ise hiçbir şey
yapmadan da bitebilir — "hiçbir şey yapma" onun için geçerli bir sonuçtur.

**Neden CS8509 bir uyarı, bir hata değil:** üçüncü durakta görüldüğü gibi
`(UnitState)99` geçerli bir değerdir. Derleyici yalnız **bildirilmiş adları**
sayabilir, gerçek değer kümesini değil. Yani tam kapsama diye bir şey
kanıtlanamaz; kanıtlanabilen tek şey "yazdığın adların hepsi var mı".

### Projede: el yazımı `default` dalı bu boşluğu kapatıyor

```csharp
// UnitView.TintFor
default:
    Debug.LogError($"[UnitView] Unhandled unit state: {state}.", this);
    return Color.white;
```

**Ölçü:** `TintFor((UnitState)99)` çağır. Hiçbir `case` tutmaz, `default` çalışır,
Console'da kırmızı bir satır belirir ve birim **görünür** kalır. O dal
olmasaydı derleme sessiz, çalışma sessiz, ekran sessiz olurdu.

`Color.clear` dönmek bir programcı hatasını görünmez bir oyun hatasına çevirirdi
— bu yüzden nötr çarpanla dönüyor. `BoardAdapter.ReactToAttack` ve `ReactToMove`
aynı sigortayı taşıyor.

### Karşı örnek — aynı dosyada `default`'suz bir `switch`

```csharp
// BoardAdapter.UpdatePlacement
switch (phase)
{
    case PointerPhase.DragReleased:  ...
    case PointerPhase.ClickReleased: ...
}                                        ◄── default YOK, ve BİLEREK
```

Kural evrensel değil; ayıran ölçüt enum'un **ne tür bir şey adlandırdığı**:

```
   SONUÇ enum'u (AttackOutcome, MoveOutcome, UnitState)
        işlenmeyen değer = ██ HATA ██        ► default: LogError ŞART

   FAZ enum'u (PointerPhase)
        beş fazın üçü "henüz bir şey olmadı" demek
        işlenmeyen faz = NORMAL AKIŞ         ► default GEREKMEZ
```

> Üretici ile tüketici arasındaki sözleşmenin tamamı ve hangi akışın neden tam
> `switch` taşımadığı: `Docs/deep/konular/06-sonuc-enumlari.md`.

---

## Yedinci durak: `%` — dairesel sayaç

```csharp
index = (index + 1) % order.Length;      // TurnState.EndTurn
```

`%` bölmeden **kalanı** verir. Bir sayacı sabit bir aralığa hapsetmenin en kısa
yolu bu:

```
order = [Player, Enemy]        order.Length = 2

  index    (index+1) % 2     Current      EndTurn() döner
  ─────    ────────────      ───────      ──────────────
    0   ──►     1            Enemy        false   (tur sürüyor)
    1   ──►     0  ██        Player       true    ██ TUR BİTTİ ██
                   └── SARMAL: başa dönmek = herkes bir kez oynadı
```

Sarmalın kendisi bir bilgi taşıyor. `TurnState` "tur tamamlandı mı" sorusunu
ayrı bir sayaçla değil, `index != 0` kontrolüyle cevaplıyor — çünkü sıfıra
dönmek, dizilimin sonuna gelmekle aynı olgudur.

İkinci kullanım sayacı değil, **eşleme** yapıyor:

```csharp
int index = (x * 7 + y * 13) % terrainSprites.Length;   // PickTerrainSprite
```

Sınırsız büyüyebilen bir sayıyı `[0, Length)` aralığına indiriyor. 7 ve 13 asal
olduğu için düzenli şerit deseni oluşmuyor, ve aynı hücre her `Play`'de aynı
sprite'ı alıyor.

### ██ Tuzak: `%` matematiksel mod DEĞİL ██

```
C#'ta kalanın işareti BÖLÜNENİN işaretini alır:

   -1 % 3   ►  -1      ██ 2 DEĞİL ██
    7 % 3   ►   1
   -7 % 3   ►  -1
```

**Ölçü:** `terrainSprites[(-1) % 3]` yaz — `IndexOutOfRangeException`.

**Bu projede neden güvenli, ve sınır nerede:**

```
TurnState.EndTurn        index 0'da başlar, (index+1) hiç negatif olamaz  ✓
PickTerrainSprite        x ve y BuildCellVisuals'ın 0'dan başlayan
                         döngülerinden geliyor                            ✓
                         ██ ama imza int alıyor: negatif x veren
                            İKİNCİ bir çağıran doğduğu gün patlar ██
```

Negatif girdi ihtimali olan bir yerde doğru şekil `((a % n) + n) % n`.

---

## Kural: "aynı mı" sorusunu nasıl sorarsın

```
① Karşılaştırdığın şey bir DEĞER tipi mi (int, enum, float, Color)?
      evet → == kullan; kopya karşılaştırması, sürprizi yok
             ██ ReferenceEquals KULLANMA — kutulanır, HER ZAMAN false ██
      hayır → ②

② Tip UnityEngine.Object'ten mi türüyor?
   (MonoBehaviour, SpriteRenderer, GameObject, UnitView…)
      evet → == KULLAN, ve bilerek: aşırı yükleme "yok edilmiş"i de
             null sayar; ReferenceEquals burada YANILTIR
      hayır → ③

③ Sorduğun soru "TAM O NESNE mi"?
      evet → ReferenceEquals — kimse ezemez, niyet satırda yazılı
      hayır (değerleri aynı mı) → ④

④ Tip Equals/GetHashCode geçersiz kılıyor mu?
      evet  → ==/Equals doğru cevabı verir
      hayır → ██ == SANA REFERANS CEVABI VERİR ██   (AttackProfile böyle)
              soruyu değiştir: "eşit mi" değil "YERİNE GEÇEBİLİR Mİ"
```

Projedeki hiçbir tip ④'te "evet" demiyor — ve `Unit` için bu bir eksiklik değil
bir **karar**: `Battle.combatants`, `Battle.structures` ve
`BoardAdapter.unitViews` sözlüklerinin anahtarı referans kimliğidir. `Unit`
`Equals`/`GetHashCode` ezseydi aynı isimli iki asker tek sözlük girdisine
çökerdi.

---

## Yanlış hatırlanan dört şey

**"`ReferenceEquals`, `==`'in hızlı sürümüdür."** Değil — **farklı bir soru**.
`==` tipin söylediğini sorar, `ReferenceEquals` adresi sorar. Değer tipleriyle
büsbütün yanlış: `ReferenceEquals(Team.Player, Team.Player)` **false** döner,
çünkü her iki değer de `object`'e kutulanırken ayrı birer nesne doğar.

**"`enum` yalnızca listedeki değerleri alabilir."** Alamaz demek doğru olurdu
ama alabilir: `(UnitState)99` derlenir ve çalışır. `enum` adlandırılmış bir
`int`'tir; tip sistemi adları bilir, değer kümesini sınırlamaz.

**"`=>` gördüm, demek ki lambda."** İki ayrı özellik aynı simgeyi paylaşıyor.
Üye bildiriminde gövde kısaltmasıdır ve **nesne üretmez**; ifade içinde bir
delege üretir ve o delegenin **kimliği vardır** — `-=` için saklaman gereken şey
tam olarak o kimliktir.

**"`%` matematiksel moddur, sonucu hep pozitiftir."** Değil. C#'ta kalanın
işareti bölünenin işaretini alır: `-1 % 3` sonucu `-1`. Dizi indeksi olarak
kullanılan her `%` bu yüzden bölünenin negatif olamayacağını kanıtlamak zorunda.

---

## Kaçış yolu: bu özellikler olmasaydı ne olurdu

```
ReferenceEquals yerine ==      → bugün aynı çalışır; biri o tipe operator ==
                                 yazdığı gün seçim mantığı SESSİZCE kayar

Unit'e Equals/GetHashCode      → aynı isimli iki asker tek sözlük anahtarına
                                 çöker; ikisi de aynı görsele bağlanır

out yerine nullable dönüş      → <Nullable> kapalı olduğu için derleme
                                 zamanında SIFIR koruma; ret sessizleşir

enum yerine int sabitler       → derleyici tip karışıklığını yakalayamaz;
                                 MoveOutcome yerine AttackOutcome geçirilir

expression-bodied yerine alan  → Width ile dizinin gerçek boyu iki sahipli
                                 olur, biri eskidiği gün derleyici susar

switch yerine if-else zinciri  → değişen bir şey yok; C# bu boşluğu ne
                                 switch'te ne if'te kapatıyor. Kapatan tek
                                 şey el yazımı default dalı.

% yerine if (index == n) i = 0 → çalışır, ama "sarmal = tur bitti" bilgisi
                                 iki satıra dağılır ve biri unutulabilir
```

---

Kodda **karar**, burada **ödünç alınan özelliğin sözleşmesi**. İkisi çelişirse
kod kazanır — orası çalışan metin, burası anlatı.

---

## ██ SIRADAKİ ADIM ██

> **▶ SIRADA:** [`02-koleksiyonlar-ve-salt-okunur.md`](02-koleksiyonlar-ve-salt-okunur.md) · [`03-hata-bildirme-ve-dogrulama.md`](03-hata-bildirme-ve-dogrulama.md) — okuma yolunun **13.** adımının kalanı, ██ sıra serbest ██
> **NEDEN ORASI:** üçü de **referans** belge; asıl işlevleri bir soru doğduğunda
> açılmak. Bu dosya "aynı" sözcüğünün dört ölçüsünü verdi — `dil/02` aynı soruyu
> **koleksiyon** üzerinde soruyor (`IReadOnlyList` ≠ değişmez), `dil/03` ise
> "yanlış" olduğunda hangi kelimeyle bağıracağını.
> **BORÇ KAPANDI:** [`07-bellek-canlilik-ve-yikim.md`](07-bellek-canlilik-ve-yikim.md) bu dosyayı açık ön koşul sayıyordu;
> orada depolama bölümünde sıkıştıysan ██ artık geri dönebilirsin ██.
> **SONRA:** `Docs/ogrenme/` ağacı — `01` → `03` → `02`, yolun **14.** ve son adımı.
> **YOL HARİTASI:** [`../../ogrenme/00-okuma-sirasi.md`](../../ogrenme/00-okuma-sirasi.md)
