# Değişmezlik anahtar kelimeleri — hangisi neyi dondurur

> **Nerede geçiyor:** `TurnState.cs` (beşin dördü tek dosyada), `UnitGrid.cells`,
> `Health.Max`, `Unit.Name`, `Combatant.ReviveHealthDivisor`,
> `UnitLifecycle.DefaultDownedWindowSeconds`, on dört `sealed class`
> **Kodda nereden geldin:** `private readonly`, `static readonly`, `const`,
> `{ get; }`, `sealed`
> **Ne zaman oku:** bir alana `readonly` yazmak üzereyken, bir sayının `const` mı
> `static readonly` mı olacağına karar verirken, ya da `sealed` görüp "demek ki bu
> tip güvenli" diye düşündüğünde.

Bu dosya projenin kendi kararlarını değil, projenin **ödünç aldığı dil
özelliklerini** anlatıyor. Bu beş kelimeyi biz tasarlamadık; C# derleyicisi
tasarladı. Ama neyi vaat ettiklerini bilmeden kendi kararlarımızı okuyamayız —
çünkü projenin en pahalı gerekçelerinden biri (`Battle.board`) tam olarak
"`readonly` burada hiçbir şey korumuyor" cümlesiyle başlıyor.

Projede kaç kez geçiyorlar (yorum satırları hariç, yalnız kod):

```
private readonly    17 kullanım /  9 dosya
sealed              14 kullanım / 14 dosya
{ get; }            10 kullanım /  7 dosya
const                6 kullanım /  5 dosya
static readonly      1 kullanım /  1 dosya
```

**Tek cümlelik cevap, gerisi bunun ayrıntısı:** beşi de bir **yazma yolunu**
kapatır, hiçbiri bir **nesnenin içini** dondurmaz.

---

## Sahne

`TurnState`'in ilk otuz satırı bir müze. Beş kelimenin dördü orada, üst üste, ve
dördü de farklı bir şeyi donduruyor:

```csharp
public sealed class TurnState                                    // ①
{
    public static readonly IReadOnlyList<Team> DefaultTurnOrder = // ②
        Array.AsReadOnly(new[] { Team.Player, Team.Enemy });

    public const int FirstTurnNumber = 1;                         // ③

    private readonly Team[] order;                                // ④
    private readonly ReadOnlyCollection<Team> orderView;          // ④

    private int index;                                            // ⑤ hiçbiri
}
```

Beşincisi — `{ get; }` — bu dosyada yok. Bir assembly ötede, `Battle.Turn`'de:

```csharp
public TurnState Turn { get; }
```

Aynı sınıfın hem ② hem ③ ile yazılmış iki sabiti var. Bu bir üslup dalgalanması
değil; birinin **başka türlü yazılması mümkün değil**, ötekininki serbest bir
seçim. Hangisinin hangisi olduğu ikinci durakta.

---

## Karakterler

```
╔═ readonly (alan) ═════════════════════════════════════════════╗
║  Ne yapar : alana ikinci kez atamayı yasaklar                  ║
║  Vaadi    : bu alan kurucudan SONRA başka bir nesneye          ║
║             bakmaya başlamaz                                    ║
║  BİLMEZ   : baktığı nesnenin içinde ne olduğunu                 ║
╚═══════════════════════════════════════════════════════════════╝

╔═ const ═══════════════════════════════════════════════════════╗
║  Ne yapar : değeri DERLEME anında çağrı yerine gömer            ║
║  Vaadi    : hiçbir çalışma-zamanı okuması yok; sabit sayı       ║
║             derleyicinin gördüğü her yerde kullanılabilir       ║
║  BİLMEZ   : değeri KOPYALAYAN assembly'nin yeniden derlenip     ║
║             derlenmediğini                                      ║
╚═══════════════════════════════════════════════════════════════╝

╔═ static readonly ═════════════════════════════════════════════╗
║  Ne yapar : tipe ait tek bir alanı bir kez doldurur             ║
║  Vaadi    : okuyan HER zaman güncel değeri görür                ║
║  BİLMEZ   : hiçbir sabit-bağlamda (varsayılan parametre,        ║
║             `case`, attribute) kullanılamayacağını — orada       ║
║             derleyici onu reddeder                              ║
╚═══════════════════════════════════════════════════════════════╝

╔═ { get; } (get-only otomatik property) ═══════════════════════╗
║  Ne yapar : gizli bir `readonly` destek alanı üretir            ║
║  Vaadi    : kurucudan sonra hiçbir satır bu property'ye         ║
║             atayamaz — tipin KENDİ metotları dahil              ║
║  BİLMEZ   : döndürdüğü nesnenin değişip değişmediğini           ║
╚═══════════════════════════════════════════════════════════════╝

╔═ sealed (sınıf) ══════════════════════════════════════════════╗
║  Ne yapar : bu tipten türemeyi yasaklar                         ║
║  Vaadi    : `Health` yazan yerde gerçekten `Health` var;        ║
║             davranış bir alt sınıf tarafından değiştirilmemiş   ║
║  BİLMEZ   : alanlarının işaret ettiği nesnelerin kaç sahibi     ║
║             olduğunu                                            ║
╚═══════════════════════════════════════════════════════════════╝

        ██ BEŞİNİN DE ORTAK KÖRLÜĞÜ ██
        Beş "BİLMEZ" satırı aynı şeyi söylüyor: kelime OKA bakar,
        okun UCUNA değil. Ucundaki nesneyi donduran tek şey, o
        nesnenin kendi tipinin yazılışıdır.
```

---

## Birinci durak: `readonly` — slot kilitli, içerik serbest

En temiz örnek `UnitGrid`. Tek alanı var ve `readonly`:

```csharp
private readonly Unit[,] cells;
```

Bu alan her turda yazılıyor. Derleyici hiç itiraz etmiyor:

```csharp
public void PlaceUnit(int x, int y, Unit unit)
{
    ThrowIfOutsideGrid(x, y, nameof(x), nameof(y));

    cells[x, y] = unit;      // ← readonly alanın İÇİNE yazma
}
```

Ne dondu, ne donmadı:

```
private readonly Unit[,] cells;
          │
          ├── ██ KİLİTLEDİĞİ ŞEY: OKUN KENDİSİ ██
          │      cells = new Unit[9, 9];       ✗ CS0191
          │      "A readonly field cannot be assigned to
          │       (except in a constructor or a variable initializer)"
          │
          └── KİLİTLEMEDİĞİ ŞEY: OKUN UCU
                 cells[x, y] = unit;           ✓ PlaceUnit'in tek satırı
                 cells[x, y] = null;           ✓ RemoveUnit'in tek satırı
                                               ▲
                        ██ AYRIŞMA NOKTASI ██
                Tahtanın BÜTÜN mutasyonu bu iki satırdan geçiyor
                ve ikisi de readonly'nin altından geçiyor. Yani
                `readonly` tahtanın DEĞİŞMEZLİĞİ hakkında tek
                kelime söylemiyor — yalnız "hangi tahta" sorusu
                hakkında konuşuyor.
```

**Ölçü:** `PlaceUnit`'in gövdesine `cells = new Unit[3, 3];` ekle → derlenmez
(CS0191). Altındaki `cells[x, y] = unit;` satırını sil → derlenir, testler patlar.
Derleyicinin nöbet tuttuğu kapı ile oyunun geçtiği kapı aynı kapı değil.

### `readonly` yazmanın hâlâ serbest olduğu iki yer

```
kurucu içinde          cells = new Unit[width, height];       ✓
alan başlatıcısında    private readonly Dictionary<Unit, Combatant> combatants
                           = new Dictionary<Unit, Combatant>();  ✓
başka her yerde        ✗ CS0191
```

`Battle`'ın dört `readonly` alanının üçü ikinci biçimde, biri (`board`)
birincide. Aradaki fark üslup değil: `board`'un değeri kurucuya gelen `width`/
`height`'a bağlı, ötekilerinki hiçbir şeye bağlı değil.

### Bu projede `readonly`'nin gerçekten koruduğu bir şey var mı

`Battle.board` için **yok** — ve orada koruma başka bir yerden geliyor.
Tekrarlamıyorum, gerekçesi ve üç katmanlı haritası burada:
`Docs/deep/konular/03-tahta-sahipligi.md` → *"İkinci durak: `readonly` burada
hiçbir şey korumuyor"*.

Gerçekten koruduğu yer, ucu **zaten değişmez olan** alanlar:

```csharp
private readonly float downedWindowSeconds;   // UnitLifecycle
private float remainingSeconds;               // aynı tip, readonly DEĞİL
```

Bu ikisi aynı dosyada yan yana ve fark bilerek konmuş: pencere kurucuda
doğrulanıp donuyor, geri sayım her `Tick`'te yazılıyor. `float` bir değer tipi
olduğu için "okun ucu" diye bir şey yok — burada `readonly` gerçekten tam bir
kilit.

**Ölçü:** `Tick` içine `downedWindowSeconds = 0f;` yaz → CS0191. Aynı metot
`remainingSeconds -= deltaSeconds;` yazıyor ve derleniyor. Aradaki tek fark bir
kelime.

---

## İkinci durak: `const` ile `static readonly` aynı işi yapmıyor

Projede altı `const` ve bir `static readonly` var. İkisi de "bu sayı değişmez"
diye okunur; derleyici için tamamen farklı iki şeydir.

```
  const int FirstTurnNumber = 1;
  ═══════════════════════════════════════════════════════════════
  DERLEME ANI                          ÇALIŞMA ANI
  ───────────                          ───────────
  TurnNumber = FirstTurnNumber;        TurnNumber = 1;
                     │                              ▲
                     └── derleyici burada ──────────┘
                         ██ DEĞERİ KOPYALAR ██
                         (IL'de `ldc.i4.1`; alanı okuyan
                          hiçbir komut YOK)


  static readonly IReadOnlyList<Team> DefaultTurnOrder = ...;
  ═══════════════════════════════════════════════════════════════
  DERLEME ANI                          ÇALIŞMA ANI
  ───────────                          ───────────
  : this(DefaultTurnOrder)             ldsfld ──► TurnState sınıfının
                     │                            statik alanı okunur
                     └── derleyici burada          │
                         BİR OKUMA BIRAKIR   ◄─────┘

  ██ AYRIŞMA NOKTASI: alttaki alan ÇALIŞMA ANINDA okunuyor,
     üstteki sayı ise okunacak bir yer bile bırakmadan yok oldu. ██
```

### Neden bazıları `const` olmak ZORUNDA

Üçünün seçim şansı yok. `UnitLifecycle`'ın kurucusuna bak:

```csharp
public UnitLifecycle(
    float downedWindowSeconds = DefaultDownedWindowSeconds,
    float corpseWindowSeconds = DefaultCorpseWindowSeconds)
```

Varsayılan parametre değeri, imzanın kendisine gömülür — çağıran taraf onu
derleme anında yazar. Yani **derleme-zamanı sabiti olmak zorunda**.

**Ölçü:** `UnitLifecycle.cs`'te

```csharp
public const float DefaultDownedWindowSeconds = 10f;
```

satırını

```csharp
public static readonly float DefaultDownedWindowSeconds = 10f;
```

yap. Alan satırı derlenir; **kurucu satırı derlenmez**:
`CS1736: Default parameter value for 'downedWindowSeconds' must be a
compile-time constant`. `StructureLifecycle.DefaultRubbleWindowSeconds` de aynı
sebeple `const`.

Aynı kelepçe iki yerde daha çalışır. İkisi de bu projede bugün kullanılmıyor,
ama denemesi bir satır:

```csharp
// herhangi bir metot gövdesinde:
switch (n) { case TurnRules.MaxActionsPerTurn: break; }
//                ▲ const ile derlenir; static readonly ile CS0150
//                  "A constant value is expected"

// BoardAdapter.cs'te (attribute'lar için Unity referansı gerekiyor):
[SerializeField, Range(0, TurnRules.MaxActionsPerTurn)] private int x;
//                       ▲ const ile derlenir; static readonly ile CS0182
//                         "An attribute argument must be a constant expression"
```

### Neden `DefaultTurnOrder` `static readonly` olmak ZORUNDA

Ters yönde bir zorunluluk. `const` yalnız şu tipleri kabul eder: `sbyte`…`ulong`,
`char`, `float`, `double`, `decimal`, `bool`, `string`, `enum` — ve referans
tipleri **yalnız `null` değeriyle**.

```csharp
public const IReadOnlyList<Team> DefaultTurnOrder =
    Array.AsReadOnly(new[] { Team.Player, Team.Enemy });
```

**Ölçü:** bu satırı yaz → `CS0134: 'TurnState.DefaultTurnOrder' is of type
'IReadOnlyList<Team>'. A const field of a reference type other than string can
only be initialized with null.` İki ayrı sebeple birden reddedilir: tip referans
tipi, **ve** sağ taraf bir metot çağrısı — çalışma anında olan bir şey.

`Team` bir `enum` olduğu için `const Team x = Team.Player;` yazılabilirdi; ama
`Team[]` ya da `IReadOnlyList<Team>` yazılamaz. Sınır tipin kendisinde, adında
değil.

### Serbest seçim olan üçü ve `const`'un assembly sınırında yaptığı şey

Kalan üçü — `TurnRules.MaxActionsPerTurn`, `TurnState.FirstTurnNumber`,
`Combatant.ReviveHealthDivisor` — hiçbir sabit-bağlamda kullanılmıyor. `static
readonly` olsalardı da derlenirlerdi. `const` seçilmesi bir tercih.

Tercihin bedeli assembly sınırında ödenir:

```
  GridStrategy.Battle.dll              GridStrategy.Battle.EditModeTests.dll
  ╔══════════════════════════╗         ╔══════════════════════════════════╗
  ║ public const int         ║         ║ Assert.That(                     ║
  ║   MaxActionsPerTurn = 1; ║         ║   TurnRules.MaxActionsPerTurn,   ║
  ╚══════════════════════════╝         ║   Is.EqualTo(1));                ║
              │                        ╚══════════════════════════════════╝
              │ derleme anında                        │
              └──────── KOPYALANIR ──────────────────►│
                                                      ▼
                                    test DLL'inin IL'inde yazan şey:
                                    Assert.That(1, Is.EqualTo(1));
                                                ▲
                        ██ AYRIŞMA NOKTASI ██
              Sayı ARTIK İKİ DOSYADA. Tanımlayan DLL 2'ye çekilse
              ve KULLANAN DLL yeniden derlenmese, test hâlâ 1
              okur ve YEŞİL geçer.

  static readonly olsaydı ok hiç kopyalanmaz, her çağrıda
  tanımlayan DLL'den ◄── OKUNURDU.
```

**Bu projede o senaryo bugün oluşamaz** ve garantinin bittiği çizgi tam olarak
burada: Unity, bir asmdef'in kaynağı değiştiğinde ona **bağımlı** olan
asmdef'leri de yeniden derler — `GridStrategy.Battle` değişince
`GridStrategy.Battle.EditModeTests` de derlenir. Kopya bayatlayamaz.

Bayatlayabildiği yer, kaynaktan derlenmeyen bir tüketicidir: `Assets/Plugins`
altına atılmış hazır bir `.dll`, bir paket, bir SDK. Yani `const`'un bu bedeli
bu projede **potansiyel**, gerçek değil — ve bunu bilmek, "const kötüdür" diye
ezberlemekten daha kullanışlı.

`ReviveHealthDivisor` ayrıca `public` ama tek okuyucusu kendi dosyası
(`health.Heal(health.Max / ReviveHealthDivisor);`). `public` olması bir kullanım
değil, **kuralın adını görünür kılma** kararı.

---

## Üçüncü durak: `{ get; }` — derleyicinin gizli `readonly` alanı

`Unit`'in taşıdığı tek şey bir ad, ve gövdesi bundan ibaret:

```csharp
public Unit(string name)
{
    Name = name;          // ← atama BURADA serbest
}

public string Name { get; }
```

Derleyici bunu açtığında ortaya `readonly` çıkıyor:

```
  YAZDIĞIN                        DERLEYİCİNİN ÜRETTİĞİ
  ════════                        ═════════════════════
                                  private readonly string <Name>k__BackingField;
  public string Name { get; }  ──► public string Name
                                  {
                                      get { return <Name>k__BackingField; }
                                  }        ▲
                                           │ set YOK — ne public, ne private
  Name = name;                 ──► <Name>k__BackingField = name;
  (kurucu içinde)                          ▲
                                ██ AYRIŞMA NOKTASI ██
                    Bu atamayı derleyici doğrudan DESTEK ALANINA
                    yazar. `readonly` alan kuralı geçerli:
                    yalnız kurucuda serbest. Bir metot içinde aynı
                    satır property kuralına çarpar ve reddedilir.
```

**Ölçü:** `Health.TakeDamage`'in içine `Max = 5;` yaz →
`CS0200: Property or indexer 'Health.Max' cannot be assigned to -- it is read
only.` Aynı satır `Health`'in kurucusunda duruyor ve derleniyor.

### `{ get; }` ile `{ get; private set; }` arasındaki gerçek fark

Projede ikisi de var ve ayrım keskin:

```
{ get; }                              { get; private set; }
─────────────────────────             ─────────────────────────
destek alanı  readonly                destek alanı  yazılabilir
yazan         yalnız kurucu           yazan         tipin her metodu
──────────────────────────            ──────────────────────────
Health.Max                            UnitLifecycle.State
Unit.Name                             UnitLifecycle.IsReadyForCleanup
Combatant.Team                        TurnState.TurnNumber
Combatant.AttackProfile
Structure.Team, Structure.AttackProfile
AttackProfile.Damage, AttackProfile.Range
MoveProfile.Range
Battle.Turn
```

`Combatant.Team`'in üstündeki yorum bu farkı zaten yazmış: *"`private set`
yetmez — o satırı bu tipin kendi metodu da yazabilirdi."* Yani `{ get; }`
seçmek "dışarıya kapalı" demek değil, **"kurucudan sonra kimseye açık değil"**
demek.

### Ve `{ get; }`'ın da bir okun ucu var

`Battle.Turn` bunun en dürüst örneği, çünkü kod bunu kendisi söylüyor:

> *"Nesnenin KENDİSİ değişkendir; değiştirilemez olan, hangi nesne olduğudur."*

**Ölçü:** varsayılan dizilimle (`[Player, Enemy]`) kurulmuş bir `Battle`'da:

```
battle.Turn = new TurnState();   ✗ CS0200 — HANGİ nesne olduğu donuk
battle.Turn.EndTurn();           ✓ Current: Player ──► Enemy
                                     TurnNumber: 1 (DEĞİŞMEDİ)
battle.Turn.EndTurn();           ✓ Current: Enemy  ──► Player
                                     TurnNumber: 1 ──► 2
                                     ▲
                       ██ AYRIŞMA NOKTASI ██
        Aynı `{ get; }` property'nin arkasındaki nesne iki çağrıda
        iki farklı sonuç verdi. Donmuş olan tek şey `Turn`'ün hangi
        `TurnState`'e baktığı; ne gösterdiği değil.
```

Zincir ancak en zayıf halkası kadar donuk. `Combatant.AttackProfile { get; }`
gerçekten uçtan uca donuk, ama bunu sağlayan `{ get; }` değil: `AttackProfile`
tipinin **kendi** `Damage` ve `Range` üyelerinin de `{ get; }` olması.

---

## Dördüncü durak: `sealed` — tip ağacını keser, nesne grafiğini kesmez

Projede on dört `sealed class` var. On ikisi hiçbir şeyden türemiyor; ikisi —
`BoardAdapter` ve `UnitView` — `MonoBehaviour`'dan türüyor **ve yine de
mühürlü**. Yön burada okunur: `sealed` bir tipin **altını** keser, üstünü değil.
Ayrıca on iki `static class` var (`TurnRules`, `DamageRules`, `MoveAction`…) ve
onlar zaten örtük olarak mühürlü — `static sealed class` yazmak derleme
hatasıdır (`CS0441`).

```
  ╔═ sealed class Health ═════════════════════════════════════════╗
  ║                                                               ║
  ║   private int current;                                        ║
  ║   public int Max { get; }                                     ║
  ║                                                               ║
  ╚═══════════════════════════════════════════════════════════════╝
        ▲                                        │
        │  class GodHealth : Health { }          │ ama bu ok
        │  ✗ CS0509                              │ paylaşılabilir
        │  "cannot derive from sealed type"      ▼
        │                            ┌──── aynı Health örneği ────┐
        │                            │                            │
   ██ KESTİĞİ ŞEY ██          Combatant a                  Combatant b
   yukarı doğru büyüme        (sealed)                     (sealed)
                                     ▲                            ▲
                              ██ AYRIŞMA NOKTASI ██
                    İkisi de `sealed`, ikisinin de alanı
                    `private readonly Health health;`.
                    Üç kelepçe üst üste — ve hiçbiri bu
                    paylaşımı görmüyor.
```

**Ölçü** — hiçbir dosyayı değiştirmeden, bir EditMode testine yazılıp
çalıştırılabilir:

```csharp
var shared  = new Health(30);
var profile = new AttackProfile(damage: 10, range: 1);

var a = new Combatant(shared, new UnitLifecycle(), profile, Team.Player);
var b = new Combatant(shared, new UnitLifecycle(), profile, Team.Enemy);

a.TakeDamage(10);

// a.CurrentHealth  →  20
// b.CurrentHealth  →  20     ██ b'ye kimse vurmadı ██
```

`Combatant` mühürlü, alanı `readonly`, `Team`'i `{ get; }` — ve düşman biriminin
canı dost birime vurunca eksildi. Üç anahtar kelimenin hiçbiri bu kapıya
bakmıyor; kapının adı **paylaşılan değiştirilebilir durum** ve onu kapatan tek
şey `Health`'in kendi tipinin yazılışı olurdu.

### Paylaşım her zaman hata değil — kritik olan neyin paylaşıldığı

`AttackProfile.cs`'in başındaki yorum aynı paylaşımı **isteyerek** kuruyor:

> *"`sealed class` olduğu için yüzlerce asker TEK örneğe ok tutar."*

Fark tek satırda:

```
paylaşılan şeyin İÇİ değişebiliyorsa   → Health        → sessiz hata
paylaşılan şeyin İÇİ değişemiyorsa     → AttackProfile → kazanç (tahsis yok)
                                          ▲
                       ██ AYRIŞMA NOKTASI ██
                    Ayrımı yapan `sealed` değil — iki tip de sealed.
                    Yapan şey AttackProfile'ın iki üyesinin de
                    `{ get; }`, Health'in `current`'ının ise düz
                    bir `private int` olması.
```

### `sealed`'in vaat ETTİĞİ şey

Ucuz ama gerçek: `Health` yazan bir metodun eline gelen şey **kesinlikle**
`Health`'tir. Bir alt sınıfın `TakeDamage`'i gizleyip başka bir formül
uygulaması, `Structure`'ın `IsStanding`'ini yalan söyletmesi mümkün değil.
`Structure`'ın başındaki gerekçe de tam bunu diyor — ve dürüstçe ekliyor:
*"`sealed` bu satıra karşı sıfır koruma sağlar"*, çünkü orada tartışılan şey
kalıtımın **yasaklanması** değil, kalıtımın **seçilmemesi**.

---

## Kural: hangi kelimeyi yazacaksın

```
① Bu bir ALAN mı, bir SABİT mi, bir TİP mi?
      tip  → ⑤
      alan → ②
      sabit→ ③

② Alan kurucudan sonra BAŞKA bir nesneye bakacak mı?
      evet → readonly YAZMA (yalan olur)
      hayır→ readonly yaz — ama ██ ucundaki nesne için
             hiçbir şey vaat etmediğini bil ██
             Değer tipiyse (float, int) tam kilit.
             Referans tipiyse yalnız niyet beyanı.

③ Değerin tipi const olabiliyor mu?
      (sayı / char / bool / string / enum, ya da null)
      hayır → static readonly ZORUNLU        (DefaultTurnOrder)
      evet  → ④

④ Değer bir SABİT BAĞLAMDA geçecek mi?
      (varsayılan parametre · case etiketi · attribute argümanı)
      evet → const ZORUNLU                   (Default*WindowSeconds)
      hayır→ serbest seçim:
               aynı assembly'de kalacaksa    → ikisi de doğru
               dışarıya DLL olarak gidiyorsa → static readonly daha güvenli
                                               (kopya bayatlayamaz)

⑤ Tipten türeyen olacak mı?
      hayır (bugünkü on dört tipin hepsi) → sealed yaz
      evet → sealed yazma; ama ██ sealed'i silmek nesne
             paylaşımı hakkında hiçbir şeyi değiştirmez ██
```

---

## Yanlış hatırlanan dört şey

**"`readonly` alan = değişmez nesne."** Değil. `UnitGrid.cells` `readonly` ve
tahtanın her hamlesi onun içine yazıyor. `readonly` **oku** dondurur, **ucunu**
değil.

**"`const` ile `static readonly` aynı şey, biri kısayol."** Değil. Üçü mecburen
`const` (varsayılan parametre), biri mecburen `static readonly` (referans tipi).
Yalnız üçünde seçim var ve seçimin bedeli assembly sınırında ödeniyor.

**"`{ get; }` dışarıya kapalı demek."** Eksik. Kurucudan sonra **herkese**
kapalı — tipin kendi metotlarına da. Yalnız dışarıya kapatmak isteyen
`{ get; private set; }` yazar; ikisi projede yan yana duruyor
(`Health.Max` ile `TurnState.TurnNumber`).

**"`sealed` sınıfı güvenli yapar."** Yalnız kalıtım yönünde. Aynı `Health`
örneğini iki `Combatant` paylaşabilir ve `sealed` bunu ne engeller ne görür.

---

## Kaçış yolu: bu beş kelime yerine ne olurdu

```
readonly'yi sil            → hiçbir çalışan davranış değişmez; yalnız
                             "bu alan bir kez kurulur" niyeti kaybolur
                             ve ikinci bir atama derleme hatası vermez

const'u static readonly    → üç dosyada derleme kırılır (CS1736);
yap                          kalan üçünde hiçbir şey olmaz

static readonly'yi         → CS0134; referans tipi const olamaz.
const yap                    Tek çıkış: `Team[]` alanını her okumada
                             yeniden kurmak — kare başına tahsis

{ get; } yerine            → tipin kendi metotları da yazabilir hâle
{ get; private set; }        gelir; Combatant.Team'in gerekçesi tam
                             olarak bunu reddediyor

sealed'i sil               → türetme açılır: bir alt sınıf Health'in
                             TakeDamage'ini gizleyip formülü sessizce
                             değiştirebilir. Nesne paylaşımı hakkında
                             ise hiçbir şey değişmez
```

Beşi birlikte, tek bir cümlenin beş ayrı yüzü: **"bu değeri kimin, ne zaman
yazabileceğini derleyiciye söylet."** Hiçbiri "bu nesnenin içi donuk" demiyor;
o cümlenin sahibi hep karşı taraftaki tipin kendi yazılışı.

---

Kodda **karar**, burada **ödünç alınan dil özelliğinin sözleşmesi**. İkisi
çelişirse kod kazanır — orası çalışan metin, burası anlatı.
