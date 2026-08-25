# "Hayır" demenin dört yolu — ret değerlerinin anatomisi

> **NEREDE GEÇİYOR** — *bu mekanizmanın kat ettiği kaynak dosyalar.* Önce dört
> enum — ***ikisi `Core`/`Combat` tarafında, ikisi `Battle` tarafında; bu bölünme
> bu dosyanın konusudur***:
> `Assets/Game/Core/MoveOutcome.cs` · `Assets/Game/Core/Combat/AttackOutcome.cs` ·
> `Assets/Game/Battle/PlacementOutcome.cs` · `Assets/Game/Battle/ReviveOutcome.cs`
> sonra tüketiciler: `Assets/Game/Battle/BattleActions.cs` → `Assets/Game/Unity/BoardAdapter.cs`
>
> **NE ZAMAN OKU** — *hangi soruyu sorduğunda ya da hangi değişikliğe giriştiğinde:*
> bu dört enum'dan birine yeni bir değer eklemeden önce, ya da "bunlar neden tek
> bir tip değil" diye sorduğunda.

**BURAYA KODDAN GELDİYSEN** — aşağıdaki üyelerin **yorumunda** bu belgeye bir
`DERİN ANLATIM:` işaretçisi var. Yol: `Ctrl+P` → dosya adının ayırt edici
parçasını yaz → `Ctrl+F` ile **üye adını** ara. ***Satır numarası bilerek
yazılmıyor: satır kayar, üye adı kaymaz.***

| dosya | üye | koddan işaretçi |
|---|---|---|
| `Assets/Game/Core/MoveOutcome.cs` | `MoveOutcome` (tip başlığı; anlatılan değer `RejectedActorCannotAct`) | ✓ |
| `Assets/Game/Core/Combat/AttackOutcome.cs` | `AttackOutcome` (tip başlığı) · `HitAndDestroyed` | ✓ |
| `Assets/Game/Battle/PlacementOutcome.cs` | `PlacementOutcome` (tip başlığı; anlatılan değer `Placed`) | ✓ |
| `Assets/Game/Battle/ReviveOutcome.cs` | `ReviveOutcome` (tip başlığı; anlatılan değer `Revived`) | ✓ |
| `Assets/Game/Unity/BoardAdapter.cs` | `ReactToMove` · `ReactToAttack` | ✓ |
| `Assets/Game/Battle/BattleActions.cs` | `Move` · `PlaceStructure` (üretici taraf) | ✓ |

---

## Sahne

Oyuncu bir hücreye tıklıyor. **Hiçbir şey olmuyor.**

Asker yerinde duruyor, ekran kıpırdamıyor, Console'a tek satır düşüyor. Oyuncu
açısından bu tek bir olay: *olmadı.*

Motorun içinde ise "olmadı" diye bir cevap yok. Onun yerine **dört ayrı enum** ve
o enumların içinde **on bir ayrı ret değeri** var. Tıklamanın hangi kapıda
durduğuna göre bunlardan tam biri geri dönüyor ve `BoardAdapter` o değere bakıp
hangi cümleyi yazacağına karar veriyor.

Bu dosya, tek bir "olmadı"nın neden on bir parçaya bölündüğünü anlatıyor — ve
bölmenin nerede durduğunu, çünkü bölünmeyen yerler de en az bölünenler kadar
bilinçli.

---

## Karakterler

Dört sonuç tipi ve iki taraf: onları **üreten** ile onları **tüketen**. Her birinin
bildikleri ve bilmedikleri var, ve bütün hikâye bilmediklerinden çıkıyor.

```
╔═ MoveOutcome ═════════════════ GridStrategy.Core ═════════════╗
║  İşi     : bir hareket DENEMESİNİN sonucunu adlandırmak       ║
║  Bilir   : 5 değer — 4 ret + Moved                            ║
║  BİLMEZ  : >> kendi beşinci değerini nasıl üreteceğini <<     ║
║            UnitState yok, MovementRules yok, TurnState yok    ║
╚═══════════════════════════════════════════════════════════════╝

╔═ AttackOutcome ═══════════════ GridStrategy.Combat ═══════════╗
║  İşi     : bir saldırı DENEMESİNİN sonucunu adlandırmak       ║
║  Bilir   : 6 değer — 3 ret + Hit + 2 ölüm ikizi               ║
║  BİLMEZ  : sıra kimde. "Sıran değil" cevabını ÜRETEMEZ,       ║
║            ama o cevabın ADI burada yazılı                    ║
╚═══════════════════════════════════════════════════════════════╝

╔═ PlacementOutcome ════════════ GridStrategy.Battle ═══════════╗
║  İşi     : bir yerleştirme denemesinin sonucunu adlandırmak   ║
║  Bilir   : 3 değer — 2 ret + Placed                           ║
║  BİLMEZ  : "eyleyen" diye bir şeyi. İmzada özne YOK.          ║
╚═══════════════════════════════════════════════════════════════╝

╔═ ReviveOutcome ═══════════════ GridStrategy.Battle ═══════════╗
║  İşi     : bir diriltme denemesinin sonucunu adlandırmak      ║
║  Bilir   : 4 değer — 3 ret + Revived                          ║
║  BİLMEZ  : >> bugün onu kimsenin OKUMADIĞINI <<               ║
╚═══════════════════════════════════════════════════════════════╝

╔═ BattleActions ═══════════════ üretici ═══════════════════════╗
║  İşi     : kuralları SORMAK, sonra akışı alt katmana bırakmak ║
║  Bilir   : sırayı, tahtayı, savaşçıları, dört enum'un hepsini ║
║  BİLMEZ  : ekranı. Tek bir Debug.Log bile basmaz.             ║
╚═══════════════════════════════════════════════════════════════╝

╔═ BoardAdapter ════════════════ tüketici ══════════════════════╗
║  İşi     : dönen değeri bir ekran davranışına çevirmek        ║
║  Bilir   : GameObject, Sprite, Console, fare                  ║
║  BİLMEZ  : hangi kuralın reddettiğini. Yalnız ADI görür.      ║
╚═══════════════════════════════════════════════════════════════╝
```

En tuhafı birincisi: **`MoveOutcome` kendi beşinci değerini üretemeyen bir
katmanda yaşıyor.** Bu bir hata değil, ölçülmüş bir taviz — beşinci durakta
tamamı yazılı.

### Kutudan gerçek satıra — her kutunun kod karşılığı

Kutular **rolü** anlatıyor; bu bölüm o rolün **hangi satırda** durduğunu
gösteriyor. Satır numarası bilerek yazılmıyor: satır kayar, üye adı kaymaz.

**`MoveOutcome` bu projede** — `Assets/Game/Core/MoveOutcome.cs` → `RejectedActorCannotAct`

```csharp
/// <summary>
/// Hareket eden şu an eylem yapamaz: sırası değil ya da durumu
/// elvermiyor (<c>MovementRules.CanMove</c> — bu tipin GÖREMEDİĞİ bir
/// kural). Bu değeri yalnızca <c>GridStrategy.Battle</c> katmanı üretir.
/// </summary>
RejectedActorCannotAct
```

Kutudaki «***kendi beşinci değerini nasıl üreteceğini***» satırının karşılığı bu
altı satırdır — ve dikkat çekici olan, tipin bunu **kendi belgesinde itiraf
etmesi**. Değeri gerçekten döndüren satır başka bir assembly'de:
`Assets/Game/Battle/BattleActions.cs` → `Move`, içindeki
`return MoveOutcome.RejectedActorCannotAct;`. Aynı dosyada iki ayrı kapı (sıra
ve durum) bu tek değere düşüyor; ikisi de bu enum'un göremediği tipleri soruyor.

**`AttackOutcome` bu projede** — `Assets/Game/Core/Combat/AttackOutcome.cs` → `RejectedActorCannotAct`

```csharp
/// <summary>
/// Saldıran şu an saldıramaz: durumu elvermiyor
/// (<see cref="AttackRules.CanAttack"/>) ya da sırası değil
/// (bu ikincisini yalnızca <c>BattleActions</c> üretir).
/// </summary>
// ALTINCI DEĞER, SONA EKLENDİ: ret ailesinin yanına sokulsaydı aradaki üç
// değer sessizce yeniden numaralanırdı. Üç sebebi (saldıran düşmüş,
// hareket eden düşmüş, sırası değil) BİLEREK tek değerde topluyor; ayrım
// ancak çağıranın dallanması değiştiği gün doğar.
// → AttackOutcome.md#rejectedactorcannotact
RejectedActorCannotAct
```

Kutudaki «"Sıran değil" cevabını ÜRETEMEZ, ama o cevabın ADI burada yazılı»
satırının karşılığı bu bloğun **iki yarısıdır**: son satır adın kendisi — bu
dosyada yazılı; parantez içindeki cümle ise üreteni gösteriyor ve o üreten
burada değil, `Assets/Game/Battle/BattleActions.cs` → `Attack` içindeki
`if (!TurnRules.CanAct(...))` kapısı. `TurnRules` bu enum'un assembly'sinden
görünmez; ad görünür, üreteç görünmez.

**`PlacementOutcome` bu projede** — `Assets/Game/Battle/PlacementOutcome.cs` → `Placed`

```csharp
/// <summary>Yapı tahtaya kondu ve savaşa katıldı.</summary>
Placed
```

Kutudaki «"eyleyen" diye bir şeyi. İmzada özne YOK.» satırının karşılığı bu
üçlünün **eksik dördüncüsüdür**: kardeş enum'ların üçünde de bulunan
`RejectedActorCannotAct` burada hiç yazılmadı. Sebebi üreticinin imzasında
duruyor — `Assets/Game/Battle/BattleActions.cs` → `PlaceStructure`, parametresi
`Battle battle, Unit unit, Structure structure, int x, int y`. Buradaki `unit`
yapının **tahtadaki kimliği**, eylemi yapan taraf değil; kime "sıran mı" diye
sorulacağı imzada yazmıyor.

**`ReviveOutcome` bu projede** — `Assets/Game/Battle/ReviveOutcome.cs` → `Revived`

```csharp
/// <summary>Hedef ayağa kalktı.</summary>
Revived
```

Kutudaki «***bugün onu kimsenin OKUMADIĞINI***» satırının karşılığı bir satır
değil, bir **yokluk** — ve ölçülebilir: bu tipi üreten tek yer
`Assets/Game/Battle/BattleActions.cs` → `Revive`, ve o metodu çağıran her satır
`Assets/Tests/EditMode/Battle/BattleActionsTests.cs` içinde. Üretim tarafında tek
çağıran yok; `Assets/Game/Unity/BoardAdapter.cs` bu tipin adını hiç yazmıyor
(karşılaştır: aynı dosyada `ReactToAttack` ve `ReactToMove` var, `ReactToRevive`
yok).

**`BattleActions` bu projede** — `Assets/Game/Battle/BattleActions.cs` → `Attack`

```csharp
bool attacked = outcome == AttackOutcome.Hit
    || outcome == AttackOutcome.HitAndDowned
    || outcome == AttackOutcome.HitAndDestroyed;

if (attacked)
{
    battle.Turn.EndTurn();
}

return outcome;
```

Kutudaki «ekranı. Tek bir Debug.Log bile basmaz.» satırının karşılığı bu bloğun
**son satırıdır**: metodun cevabı bir yan etkiyle değil, dönüş değeriyle
çıkıyor. Blokta görünen tek yan etki `battle.Turn.EndTurn()` ve o da ekran değil
oyun durumu. Ölçü: bu dosyada `Debug` kelimesi **0 kez**, `UnityEngine` kelimesi
**0 kez** geçiyor — kutunun cümlesi bir üslup tercihi değil, assembly sınırının
sonucu.

**`BoardAdapter` bu projede** — `Assets/Game/Unity/BoardAdapter.cs` → `ReactToMove`

```csharp
case MoveOutcome.RejectedActorCannotAct:
    Debug.Log($"[Board] '{unit.Name}' cannot act right now; the move was rejected. {DescribeCondition(unit)}", this);
    break;

default:
    Debug.LogError($"[Board] Unhandled move outcome: {outcome}.", this);
    break;
```

Kutudaki «hangi kuralın reddettiğini. Yalnız ADI görür.» satırının karşılığı
`case` satırıdır: elindeki tek şey bir enum değeri. Bu değeri `TurnRules.CanAct`
mi yoksa `MovementRules.CanMove` mu ürettiği — `BattleActions.Move`'da ikisi de
aynı değeri döndürüyor — bu dalda **sorulamaz**. Metnin `DescribeCondition(unit)`
ile durumu ayrıca okuması da bunun kanıtı: sebep dönen değerde taşınmadığı için
sonradan aranıyor. Alttaki `default` dalı ise bilmemenin dürüst hâli: adı
tanımayan bir değer geldiğinde sessizce yutulmuyor, `LogError` basılıyor.

---

## Birinci durak: sıfırıncı hücre

`MoveOutcome`'da tek bir `= 0` bile yazılı değil. Üyeler yazıldıkları satır
sırasına göre numaralanıyor. Yani "sıfırıncı değer" bir **isimlendirme** kararı
değil, bir **yerleştirme** kararı.

```
   index   SEÇİLEN                        REDDEDILEN (Moved başa alınsa)
   ─────   ────────────────────────────   ──────────────────────────────
     0     RejectedInvalidDestination     Moved                  ◄── ██ ██
     1     RejectedCellOccupied           RejectedInvalidDestination
     2     RejectedOutOfRange             RejectedCellOccupied
     3     Moved                          RejectedOutOfRange
     4     RejectedActorCannotAct         RejectedActorCannotAct
              │                                    │
              ▼                                    ▼
     `private MoveOutcome last;`        `private MoveOutcome last;`
      atanmamış hâl = bir RET            atanmamış hâl = "TAŞINDI"
      zararsız: zaten hiçbir şey         >> hiç hareket denenmeden
      olmadı demektir                       ekran "taşındı" der <<
```

İki sütunda da **beş ad aynı**, anlamları da aynı. Değişen tek şey `Moved`'ın
satır numarası. Ve o tek satır, dilin atanmamış her alana verdiği değeri bir
başarıya bağlıyor.

### Sıfıra düşmenin üç yolu — üçü de derleyiciden sessiz geçer

```
   default(MoveOutcome)              ► açıkça yazılır, kimse şaşırmaz
   new MoveOutcome[n] hücreleri      ► dizi ayrıldığı anda n tane sıfır doğar
   atanmayı unutulan bir alan        ► >> burada kimse bir şey YAZMAZ <<
```

Üçüncüsü tehlikeli olan. Bir alan bildirdin, atamayı unuttun, kod derlendi,
testler yeşil kaldı. Alanın içinde sıfır var ve sıfır "taşındı" diyorsa ekran hiç
denenmemiş bir hareketi başarılı gösteriyor.

**Bu üç yolu kapatmanın yolu yok.** Seçilebilen tek şey sıfır hücresinde *ne
durduğu.* O yüzden sıfırıncı değer bir varsayılan değil, bir **sigorta**: en
zararsız cevap "olmadı"dır.

### Ama kural "sıfır hep RET olsun" değil

Ölçüt tek: **atanmamış hâlin doğal karşılığı ne?** Sıfır, o karşılığı adlandıran
değere verilir.

```
   MoveOutcome     atanmamış = "hiçbir şey denenmedi"  ► sıfır bir RET
   AttackOutcome   atanmamış = "hiçbir şey denenmedi"  ► sıfır bir RET
   PlacementOutcome                          aynısı    ► sıfır bir RET
   ReviveOutcome                             aynısı    ► sıfır bir RET
   ─────────────────────────────────────────────────────────────────────
   PointerPhase    atanmamış = "hiç basılmadı"         ► sıfır Idle
                                                         ◄── RET DEĞİL
```

`PointerGesture.cs`'te `PointerPhase.Idle = 0` açıkça yazılı ve bir ret değeri
değil — orada atanmamış bir alanın doğal karşılığı gerçekten "jest yok"tur ve
sıfır tam da onu adlandırıyor. Aynı ad alanı, zıt görünen karar, **aynı ölçüt.**

Kural şu değil: *sıfırı ret'e ayır.*
Kural şu: **sıfırı, unutulmuş atamanın DOĞRU karşılığına ayır.**

---

## İkinci durak: yeni değer neden sona eklenir

`AttackOutcome` altı değer taşıyor ve altıncısı — `RejectedActorCannotAct` — ret
ailesinin **yanına değil, sona** eklendi. İçgüdü tam tersini söyler: üç ret
değeri yan yana dursun, iki vuruş değeri yan yana dursun, kaynak okunaklı olsun.

Bakılması gereken şey, o "okunaklılık" için ödenen fatura:

```
   index   BUGÜN                      "ret ailesinin yanına" eklenseydi
   ─────   ──────────────────────     ─────────────────────────────────
     0     RejectedInvalidTarget      RejectedInvalidTarget        aynı
     1     RejectedOutOfRange         RejectedOutOfRange           aynı
     2     Hit                        RejectedActorCannotAct  ◄── YENİ
     3     HitAndDowned               Hit                     ◄── 2'ydi
     4     HitAndDestroyed            HitAndDowned            ◄── 3'tü
     5     RejectedActorCannotAct     HitAndDestroyed         ◄── 4'tü
                                      >> TEK EKLEME, ÜÇ DEĞER KAYDI <<
```

Sağdaki sütunda hiçbir **ad** değişmedi. Değişen tek şey üç değerin **sayısı**.
Ve enum değerlerinin sayıları şu üç yerde sessizce saklanır:

```
   kayıt dosyası (serileştirme)   ► sayı yazılır, ad yazılmaz
   Unity .asset / prefab alanı    ► sayı yazılır, ad yazılmaz
   ağ paketi / tekrar kaydı       ► sayı yazılır, ad yazılmaz
```

Bugün bu projede o üç yerin **hiçbiri yok** — yani kırılma ölçülebilir değil.
Karar yine de sona ekleme yönünde, çünkü sona eklemek **geri alınabilir** olan;
ortaya eklemek geri alınamayanı riske atar ve karşılığında yalnızca kaynaktaki
görsel düzeni satın alır.

Üstelik o görsel düzen zaten yok:

```
   kaynaktaki sıra = akış sırası MI?
   ────────────────────────────────
   index 4  HitAndDestroyed   ◄── beşinci sırada duran bir BAŞARI değeri
                                  ret ailesinin ortasında değil, ARADA
   >> Sıra bilgisini enum'dan okumaya çalışan göz burada ZATEN yanılır <<
```

Yani ret değerlerini kümelemenin taşıdığı tek fayda — "sırayla bakarsam anlarım" —
bu enum'da hiç var olmamış. Fayda sıfır, risk sıfırdan büyük.

### İkizler: `HitAndDowned` ↔ `HitAndDestroyed`

Aynı enum hem `Combatant`'a hem `Structure`'a yapılan saldırıyı adlandırıyor. Üç
ret sebebi ve `Hit` ikisinde de **birebir aynı cümle.** Ayrışan tek şey ölümün
adı:

```
   değer                    Combatant   Structure
   ──────────────────────   ─────────   ─────────
   RejectedInvalidTarget        ✓           ✓
   RejectedOutOfRange           ✓           ✓
   RejectedActorCannotAct       ✓           ✓
   Hit                          ✓           ✓
   ──────────────────────   ─────────   ─────────
   HitAndDowned                 ✓           ✗
   HitAndDestroyed              ✗           ✓
                            >> AYRIŞAN TEK ÇİFT <<
```

Neden `HitAndDowned` yeniden kullanılmıyor: **bir baraka düşmez, yıkılır.**
"Düşme" bir kurtarma penceresi açan durumdur (`ReviveOutcome` tam olarak o
pencereye hizmet ediyor); yapıda öyle bir pencere yok. Aynı değeri iki farklı
olguya vermek, çağıranın onları ayırt etmesini imkânsız kılardı.

Neden düz `Hit` dönülmüyor: yıkım bilgisi çağıranın elinden alınırdı ve çağıran
onu geri kazanmak için saldırıdan **sonra** `State` okumak zorunda kalırdı. O
okuma da yanlış cevap verirdi — zaten yıkık bir enkaza vurmak da
`State == Destroyed` gösterir.

Ve neden ikinci bir `StructureAttackOutcome` enum'u açılmadı: ortak dört değer
kopyalansaydı her tüketici **paralel bir switch** taşırdı ve tek `default: LogError`
koruması ikiye bölünürdü. Ayıraç net — *ret sebepleri aynı cümle mi?* Burada üçü
de birebir aynı, o yüzden tek enum ve ayrışan tek çift o enum'a **eklendi.**

---

## Üçüncü durak: yazılmayan dördüncü değer

`PlacementOutcome`'un üç değeri var ve üçü de **okunarak bulundu** — uydurulmadı.
Her biri akıştaki bir kapıya birebir karşılık geliyor:

```
   ┌─ BattleActions.PlaceStructure ────────────────────────────┐
   │                                                            │
   │   IsInsideGrid(x, y)      ──► RejectedInvalidCell     ①   │
   │           │ geçti                                          │
   │           ▼                                                │
   │   TryGetUnit(x, y, out _) ──► RejectedCellOccupied    ②   │
   │           │ geçti                                          │
   │           ▼                                                │
   │   AddStructure(...) döndü ──► Placed                  ③   │
   │           │                                                │
   │           └─► "bu birim zaten savaşta" ──► ArgumentException
   │               >> BU ENUM'A GİRMEZ — çağıran hatası <<     │
   └────────────────────────────────────────────────────────────┘

   üç değer  ◄──►  üç kapı.  Artan yok, eksik yok.
```

Eşleşme birebir: her değerin tam olarak bir üreteni var ve `PlaceStructure`'da bu
üçünün dışında sonuç döndüren satır yok.

### Dördüncüsü neden yok

Diğer üç akışta (`Attack`, `Move`, `Revive`) `RejectedActorCannotAct` var. Aynı
değeri buraya da eklemek doğal görünür. Ekleyemezsin, ve sebep imzada:

```
   Attack / Move / Revive              PlaceStructure
   ┌──── EYLEYEN ────┐                 ┌──── EYLEYEN ────┐
   │   Unit actor    │                 │      YOK        │
   └────────┬────────┘                 └────────┬────────┘
            │ Combatant.Team                    │ >> ödünç alınabilecek
            ▼                                   ▼    tek alan: <<
   TurnRules.CanAct(team, ...)          structure.Team
            │                                   │
            │                                   ▼
            │                          Structure.Team bir SAHİPLİK değil
            │                          bir AİDİYET: nötr duvar → Team.None
            │                                   │
            ▼                                   ▼
        doğru soru                     CanAct(Team.None, ...) → HER ZAMAN false
                                       >> nötr hiçbir yapı tahtaya bir daha
                                          konamaz <<
```

Olmayan özneyi bir başkasının tarafından ödünç almak, doğru kuralı yanlış şeye
sormaktır. Ve bu sefer sessiz kalmıyor: `PlaceStructure_NeutralStructureOutOfTurn_IsStillPlaced`
kırmızıya döner.

Değerin kendisi reddedilmedi — **öznesi olmayan bir imzada aranması** reddedildi.
Karşı örnek aynı ad alanında, yirmi satır ötede: `ReviveOutcome.RejectedActorCannotAct`
var ve doğrusu bu, çünkü `Revive`'ın imzasında `reviver` diye gerçek bir eyleyen
duruyor.

---

## Dördüncü durak: üç ret, üç ayrı tepki — ve sıfır tüketici

`ReviveOutcome`'un üç reddi, çağıranın **üç farklı davranışına** karşılık geliyor.
Enum'un var olma sebebi tam olarak bu tablo:

```
   ret değeri                SUÇLADIĞI ÖZNE   çağıranın yapacağı şey
   ───────────────────────   ──────────────   ──────────────────────────
   RejectedInvalidTarget     HEDEF            uyarı sesi — başka hedef
   RejectedOutOfRange        MESAFE           yapay zekâ: "önce YAKLAŞ"
   RejectedActorCannotAct    EYLEYEN          sessiz kal — bekle
                             ▲
              >> Üçü ÖRTÜŞMÜYOR çünkü üçü farklı bir özneyi suçluyor <<
```

Üçüncüsü tek başına **iki sebebi** taşıyor: sıra o tarafta değil
(`TurnRules.CanAct`) ya da diriltenin kendisi ayakta değil (`ReviveRules.CanRevive`).
Bu bilinçli bir birleştirme — çağıran açısından ikisi de aynı cümle: *şimdi olmaz.*

Ve bir şey daha, dosyadan okunmuyor ama gerçek:

```
   ReviveOutcome'un bugünkü TÜKETİCİLERİ
   ──────────────────────────────────────
   BattleActions.Revive     ► üretir
   BattleActionsTests       ► okur, sabitler
   BoardAdapter             ► >> HİÇ GÖRMÜYOR <<
```

Yani üç ret sebebi bugün ekranda **hiçbir farklı davranış üretmiyor**; farkı yalnız
testler görüyor. Ayrımı haklı çıkaran şey bugünkü tüketici değil, ayrımın
**tüketici doğduğu gün hazır olması.** Tersi mümkün değil: tek `bool` dönen bir
`Revive`'ın üç sebebi, tüketici doğduğu gün akıştan **yeniden çıkarılamaz** —
bilgiyi üreten yerde kaybettin.

---

## Beşinci durak: aynı ad, iki enum, tek duvar

`RejectedActorCannotAct` üç enum'da birden var ve anlamı üçünde de tek cümle:
*eylemi yapan taraf şu an eylem yapamaz.* Farklı adlar seçmek, çağıranı aynı
cevabı üç kez öğrenmek zorunda bırakırdı.

Ama **üretilebilirlikleri** aynı değil, ve farkı yaratan şey bir asmdef dosyası.

```
   ┌─ GridStrategy.Combat ── references: [] ────────────────────┐
   │   AttackOutcome   ← tip BURADA                             │
   │   AttackRules, UnitState, MovementRules ← hepsi BURADA     │
   │                                                             │
   │   AttackAction.Execute üretebildikleri:                     │
   │     RejectedInvalidTarget    ✓    Hit              ✓        │
   │     RejectedOutOfRange       ✓    HitAndDowned     ✓        │
   │     RejectedActorCannotAct   ✓    HitAndDestroyed  ✓        │
   │                              ▲                              │
   │            AttackRules.CanAttack AYNI KUTUDA → sorabilir    │
   │                                              >> 6 / 6 <<    │
   └─────────────────────────────────────────────────────────────┘

   ┌─ GridStrategy.Core ── references: [] ──────────────────────┐
   │   MoveOutcome     ← tip BURADA                             │
   │   MovementRules, UnitState, TurnState ← >> BURADA DEĞİL << │
   │                                                             │
   │   MoveAction.Execute üretebildikleri:                       │
   │     RejectedInvalidDestination  ✓                           │
   │     RejectedCellOccupied        ✓                           │
   │     RejectedOutOfRange          ✓                           │
   │     Moved                       ✓                           │
   │     RejectedActorCannotAct      ✗ ◄── MovementRules.CanMove │
   │                                       bu kutudan GÖRÜNMEZ;  │
   │                                       ADINI YAZMAK bile     │
   │                                       DERLENMEZ             │
   │                                              >> 4 / 5 <<    │
   └──────────────────────────┬──────────────────────────────────┘
                              │ Battle, Core'u ve Combat'ı GÖRÜR
                              │ (ok tek yönlü — geri dönüşü yok)
   ┌─ GridStrategy.Battle ────▼──────────────────────────────────┐
   │   BattleActions.Move                                        │
   │     TurnRules.CanAct(...)        ► değeri döndürür  ◄── ①  │
   │     MovementRules.CanMove(...)   ► değeri döndürür  ◄── ②  │
   │     ikisi de geçerse             ► akış Core'a iner         │
   │                                                              │
   │   >> Bu enum'un beşinci değerinin DÜNYADAKİ TEK ÇIKIŞI <<   │
   └──────────────────────────────────────────────────────────────┘
```

Tipin **tanımlandığı** kutu ile değerin **üretildiği** kutu farklı. `MoveOutcome.cs`
`GridStrategy.Core`'da yaşıyor, beşinci değerini üretebilen tek yer
`GridStrategy.Battle`.

### Bir asmdef bir enum değerini nasıl yasaklar

Yasak, enum'un içinde yazılı değil — enum bütün değerlerini herkese açar. Yasak
**değeri üretmek için gereken kanıtın** görünmezliğinde:

```
   MoveAction'ın içinde yazmak istediğin satır:
       if (!MovementRules.CanMove(state)) return MoveOutcome.RejectedActorCannotAct;
           ▲                       ▲
           │                       └─ UnitState: Combat'ta. GÖRÜNMEZ.
           └─ MovementRules: Combat'ta. GÖRÜNMEZ.

   Sonuç: satır DERLENMEZ. Enum değeri erişilebilir, KANIT değil.
```

Yani asmdef bir değeri değil, o değeri **hak etmenin yolunu** kapatıyor.

> **▶ ARA DURAK:** [02-assembly-duvari.md](02-assembly-duvari.md#ikinci-fatura-bir-enum-sahibinin-uretemedigi-bir-deger-tasiyor)
> **NEDEN:** yukarıdaki yasağın taşıyıcı gerekçesi `asmdef` kelimesinin
> üstünde duruyor ve bu dosya `asmdef`'i tanımlamıyor. `02` aynı `MoveOutcome`
> değerini **öteki yönden** anlatıyor: burada "enum ne kaybetti", orada "duvar ne
> kesti". ***Aynı satır, iki fatura.***
> **DÖNÜŞ:** bu dosyanın [«İki koruma, iki farklı kırılma»](#iki-koruma-iki-farkli-kirilma) bölümü

> **⌨ KODU AÇ:** `Assets/Game/Core/MoveOutcome.cs` → `RejectedActorCannotAct`,
> sonra `Assets/Game/Core/MoveAction.cs` → `Execute`
> **BAK:** değer birinci dosyada **bildirilmiş**; ikinci dosyada onu döndürecek
> tek bir satır bile yok — çünkü kanıtı üreten `MovementRules` adı orada
> aranamıyor. Değer erişilebilir, kanıt değil.
> **DÖNÜŞ:** bu dosyanın «Bir asmdef bir enum değerini nasıl yasaklar» bölümü

### İki koruma, iki farklı kırılma

Bu söz — "bu değer Core'dan çıkmaz" — tek bir mekanizmayla tutulmuyor:

```
   KAZAYLA yazılması   ► asmdef references: []
                         MovementRules adı derlenmez
                         ◄── derleme zamanı, KESİN

   BAŞKA yoldan        ► BattleActionsTests:515
   sızması               MoveAction_NeverReturnsRejectedActorCannotAct
                         ◄── çalışma zamanı, DAVRANIŞSAL
```

İkisi yedek değil. asmdef'e bir `GridStrategy.Combat` referansı eklenirse birinci
koruma **aynı gün düşer** ve kuralı tutan tek şey test kalır. Test silinirse bugün
hiçbir şey kırılmaz — ta ki o referans eklenene kadar; o gün sızıntıyı söyleyecek
kimse olmaz.

**Biri kapıyı kilitliyor, öteki kapının kilitli kaldığını her koşuda ölçüyor.**

Ve garantinin sınırı: ikisi de bu değerin Core'dan **çıkmamasını** tutar; Battle'da
doğru üretilip üretilmediğini tutmaz. Orada sözü tutan şey `BattleActions`'ın kendi
kontrol sırasıdır.

---

## Tek bakışta: üretici × tüketici

```
  SONUÇ TİPİ          ÜRETİCİ                       kaç değer   TÜKETİCİ
  ─────────────────   ───────────────────────────   ─────────   ──────────────────────
  MoveOutcome         MoveAction     (Core)           4 / 5     BoardAdapter.ReactToMove
                      BattleActions.Move (Battle)     1 / 5     >> TAM switch <<
                                     >> 4+1, ÇAKIŞMA YOK <<      + default: LogError

  AttackOutcome       AttackAction   (Combat)         6 / 6     BoardAdapter.ReactToAttack
                      BattleActions.Attack            1 / 6     >> TAM switch <<
                                     (zaten üretilebilir olan)   + default: LogError

  PlacementOutcome    BattleActions.PlaceStructure    3 / 3     BoardAdapter
                                                                 tek `== Placed`
                                                                 ◄── switch YOK

  ReviveOutcome       BattleActions.Revive            4 / 4     >> EKRAN TÜKETİCİSİ YOK <<
                                                                 yalnız testler
```

Bu matriste okunacak **üç** şey var:

```
① MoveOutcome satırı TEK OLAN: iki üretici arasında değer kümesi ÇAKIŞMIYOR.
   Core'un ürettiği 4 değere Battle hiç dokunmuyor; Battle'ın ürettiği 1 değeri
   Core hiç üretemiyor. Bölünme rastgele değil, ASMDEF ÇİZGİSİ.

② AttackOutcome satırında BattleActions'ın ürettiği tek değer, AttackAction'ın
   ZATEN üretebildiği bir değer. Yani orada bir taviz YOK — aynı cevabı iki ayrı
   kapı veriyor: biri saldıranın durumuna, öteki sıraya bakıyor.

③ >> Tüketici sütunu üç FARKLI şekil taşıyor ve üçü de doğru. <<
   tam switch  ► her ret ayrı bir mesaj üretiyor
   tek `==`    ► ret sebebine göre yapılacak FARKLI bir iş yok
   yok         ► değeri okuyan ekran kodu henüz doğmadı
```

---

## Ret değerlerini birleştirmenin bedeli

"Üç ret yerine tek `Rejected` yazsak ne olur" sorusunun cevabı tek cümle değil.
Kaybedilen şey enum'un içinde değil, **çağıranın kaybettiği daldadır:**

```
  birleştirilen değerler          çağıranın KAYBETTİĞİ dal
  ────────────────────────────    ────────────────────────────────────────
  RejectedOutOfRange              yapay zekânın "önce YAKLAŞ" dalı
      + RejectedInvalidTarget     → yaklaşmak geçersiz hedefte İŞE YARAMAZ,
                                     yapay zekâ sonsuza kadar yaklaşır
  ────────────────────────────    ────────────────────────────────────────
  RejectedInvalidDestination      "asla gidilemez" ile "şimdilik gidilemez"
      + RejectedCellOccupied      → tahta dışı hücreyi her turda yeniden dener
                                     VEYA dolu hücreden kalıcı vazgeçer
                                     >> hangisini yazarsan yaz, öteki YANLIŞ <<
  ────────────────────────────    ────────────────────────────────────────
  Hit + HitAndDowned              düşme animasyonu ve skor kaydı HER vuruşta
                                  tetiklenir
  ────────────────────────────    ────────────────────────────────────────
  HitAndDowned + HitAndDestroyed  kurtarma penceresi: biri açar, öteki AÇMAZ
                                  → enkaza sıhhiyeci gönderilir
  ────────────────────────────    ────────────────────────────────────────
  RejectedActorCannotAct'ın       >> BUGÜN HİÇBİR DAL KAYBOLMUYOR <<
  üç sebebi (sıra / saldıran      üçünde de çağıranın yapabileceği tek şey
  düşmüş / hareket eden düşmüş)   aynı: BEKLE. O yüzden BİRLEŞTİRİLDİ.
```

Son satır bütün tabloyu okutan satır. Ayıraç **sebep sayısı değil, DAVRANIŞ
sayısı.** `MoveOutcome` üç ret sebebini bilerek ayırdı ve aynı dosyada üç sebebi
bilerek birleştirdi — zıt görünen iki karar, tek ölçüt.

> **◀ DÖNÜŞ:** [04-karar-sirasi.md](04-karar-sirasi.md#dorduncu-durak-sira-iner-ama-ayni-sirayla-iner) — «Üçüncü durak: iki ölçüt»ten
> geldiysen artık şunu biliyorsun: aynı ölçüt (*"ayıraç sebep sayısı değil,
> DAVRANIŞ sayısı"*) iki dosyada iki kez kuruluyor — **burada *kurulur*, orada
> bir *sıra kararına uygulanır*** · oraya dön ve dördüncü duraktan devam et

### Ayrımın doğacağı gün, ve tetiği

`RejectedActorCannotAct` ikiye ne zaman ayrılır, kaynakta yazılı:

```
  >> EŞİK <<  arayüz oyuncuya "sıran değil" ile "birim düşmüş" farkını
              SÖYLEMEK zorunda kaldığı gün.

  Bugün neden aşılmadı: tek ekran tüketicisi BoardAdapter ve o yalnızca
  Debug.Log basıyor. İki dal aynı satırı iki kez yazardı.
```

---

## Kural: yeni bir ret sebebi eklerken

Sırayla sor. Her adımın çıkışı bir eylem, "duruma bakarız" yok.

```
① Bu sebep, ÇAĞIRANIN yapabileceği bir şeyi gösteriyor mu?
      HAYIR → ekleme. Ret sebebi çağırana bir çıkış yolu söylemelidir.
              "Sistem hatası" bir enum değeri değil, bir istisnadır.
      EVET  → ②

② Çağıran bu sebebe, MEVCUT bir sebeple AYNI mı davranır?
      AYNI  → >> EKLEME << Mevcut değerin kapsamına gir.
              (RejectedActorCannotAct üç sebebi böyle topluyor)
      FARKLI → ③

③ Bu sebebi, enum'un yaşadığı ASSEMBLY'deki bir tip söyleyebilir mi?
      EVET  → ④ — burada taviz yok
      HAYIR → değer, sahibinin ÜRETEMEYECEĞİ bir değer olur.
              Ekleyebilirsin ama üç şey birden yapman gerekir:
                a. enum'un yanına gerekçeyi YAZ (gizleme)
                b. gerçek üreticiyi ADIYLA söyle
                c. "asla buradan çıkmaz" testini AÇ
              (MoveOutcome.RejectedActorCannotAct'ın bedeli tam olarak bu üçü)
              → ④

④ Bu değerin akışta bir ÜRETENİ var mı? (imzada özne, kodda kapı)
      HAYIR → >> EKLEME << Eşi olmayan bir değer doğar; çağıran asla
              dönmeyecek bir dal yazar.
              (PlacementOutcome'un dördüncü değeri tam burada düştü)
      EVET  → ⑤

⑤ Değeri ARAYA mı SONA mı koyuyorsun?
      SONA  → >> DOĞRU <<
      ARAYA → sonrasındaki her değerin sayısı sessizce kayar.
              Tek kabul edilebilir gerekçe: kaynaktaki sıra GERÇEKTEN
              akış sırası ise ve hiçbir yerde sayı saklanmıyorsa.
              Bu enum'larda ikisi de geçerli değil → SONA.
```

Ve sıfırıncı hücre için ayrı, tek soruluk bir kural:

```
   Bu enum'un SIFIR hücresinde ne var?
      bir BAŞARI  → >> DUR << Atanmamış her alan, hiç denenmemiş bir işi
                    başarılı gösterir. Derleyici susar, test yeşil kalır.
      bir RET     → devam
      ne başarı   → o hâlin doğal karşılığını adlandırıyor mu diye bak
      ne ret        (PointerPhase.Idle böyle doğru)
```

---

## Üretici ile tüketici arasındaki sözleşme

Enum'a bir değer eklediğinde **hiçbir şey kırılmaz.** Kırılmaması sorunun kendisi.

```
   switch (outcome)                    switch DEYİMİ (statement)
   {                                   ────────────────────────────
       case A: ... break;              yeni bir değer eklendiğinde:
       case B: ... break;                derleyici: >> SUSAR <<
       // yeni değer C burada YOK        çalışma anı: hiçbir dal eşleşmez,
   }                                                  switch sessizce geçilir
```

C#'ta bir `switch` **deyimi** için eksik enum dalı bir uyarı bile üretmez. Bir
`switch` **ifadesi** (expression) `CS8509` verir, ama bu iki tüketici deyim
kullanıyor. O yüzden görünürlüğü derleyici değil, **el yazımı bir dal** sağlıyor:

```csharp
default:
    Debug.LogError($"[Board] Unhandled attack outcome: {outcome}.", this);
    break;
```

`LogError`, `Log` değil — çünkü buraya düşmek bir oyun durumu değil, bir
**programcı hatası:** enum'a değer eklendi ve switch güncellenmedi.

Sözleşmenin tamamı bu iki satırda:

```
  ╔═ ÜRETİCİ (BattleActions) ═══════════════════════════════════╗
  ║  değer EKLEYEBİLİR — kimseye sormadan, derleme kırılmadan   ║
  ╚═══════════════════════════════╤═════════════════════════════╝
                                  │  >> derleyici burada SUSAR <<
                                  ▼
  ╔═ TÜKETİCİ (BoardAdapter) ═══════════════════════════════════╗
  ║  yeni değeri işlemezse: default dalına düşer                ║
  ║    default: LogError  ► Console'da GÖRÜNÜR      ◄── ReactTo*║
  ║    default: YOK       ► sessizce hiçbir şey olmaz           ║
  ╚═════════════════════════════════════════════════════════════╝
```

`ReactToAttack` ve `ReactToMove` bu sigortayı taşıyor. Yerleştirme akışında
taşımıyor — orada tek soru "kondu mu" ve reddin sebebi zaten `Debug.Log`'un
içinde `{outcome}` olarak basılıyor. **Eşik yazılı:** bir ret sebebi ekranda
farklı bir şey yaptırdığı gün o karşılaştırma da tam switch şekline çevrilir.

---

## Yanlış hatırlanan üç şey

**"Sıfırıncı değer kuralı, ilk değeri `None` yapmaktır."** Değil. Bu dört enum'un
hiçbirinde `None` yok. Kural, sıfırı **atanmamış hâlin doğru karşılığına** vermek.
`MoveOutcome`'da o karşılık `RejectedInvalidDestination`, `PointerPhase`'de `Idle`.
`None` yalnızca `[Flags]` maskelerinde doğru cevap — orada sıfır "hiçbir bayrak
yok" demektir ve bu enum'lar maske değil, tek değer dönüyorlar.

**"`RejectedActorCannotAct` iki enum'da olduğu için kopya kod."** İki enum ayrı ama
çağıranın sorusu tek: *bu birim şu an eyleyebilir mi.* Ad paylaşmakla **tip**
paylaşmak farklı şeyler. Tek bir adı paylaşmak için o adın cümlesinin aynı olması
yeter; tipi paylaşmak için **bütün** adların aynı olması gerekir — ve
`AttackOutcome` ile `ReviveOutcome`'un kesişimi 3, birleşimi 7. Birleşselerdi her
çağıran kendi eylemi için asla dönmeyecek dallar yazardı.

**"`ReviveOutcome`'un üç reddi fazla, kimse okumuyor."** Bugün ekran okumuyor,
doğru. Ama ayrımı bugün kurmazsan yarın **geri getiremezsin:** `bool` dönen bir
akışta "menzil dışı mıydı, hedef mi geçersizdi" sorusunun cevabı akışın içinde
kaybolur ve çağıran onu ancak `BattleActions`'ın kurallarını **kopyalayarak**
yeniden üretir. Bilgiyi üretildiği yerde daraltmak tek yönlü bir kapıdır.

---

## Kaçış yolu: bu tasarımdan nasıl kaçılırdı

Üç ayrı kaçış var ve üçü de kaynakta adıyla reddedildi.

### ① `bool` dönmek

```csharp
✗ public static bool Move(Battle battle, Unit unit, int toX, int toY, int range)
```

"Taşındı mı" tek cevaba **üç ayrı soruyu** sıkıştırır. Ayrım kaybolmaz — çağıranın
**içinde** ikinci bir kontrol olarak yeniden doğar:

```
   BoardAdapter:                          ve o kontrol MoveAction'ın
     if (!moved)                          kurallarını KOPYALAR
     {                                          │
         if (!board.IsInside(x, y)) ...         ▼
         else if (board.HasUnit(x, y)) ...   >> İKİ YERDE İKİ KURAL,
         else if (dist > range) ...             biri değişince öteki
     }                                          sessizce YALAN SÖYLER <<
```

**Kaçınılan şey `bool`'un kendisi değil, kuralın ikinci kopyası.**

### ② İstisna atmak

```csharp
✗ throw new InvalidMoveException("cell occupied");
```

Bu proje istisnayı **kullanıyor** — ama başka bir soru için. Ayıraç `PlacementOutcome`'da
tek satırla yazılı: *o hücreyi kim seçti?*

```
                   OLGU: (x, y) dolu
                           │
            ┌──────────────┴───────────────┐
            ▼                              ▼
   Battle.AddUnit                  BattleActions.PlaceStructure
   hücreyi KAYIT seçti             hücreyi FARE seçti
   (seviye dizilimi, spawn         (oyuncunun sıradan bir hamlesi)
    tablosu, kayıt dosyası)
   ╔═ ArgumentException ═╗         ╔═ RejectedCellOccupied ═╗
   ╚═════════════════════╝         ╚════════════════════════╝
   >> AYRIM olguda DEĞİL, hücreyi SEÇENDE <<
```

Oyuncu hamlesini istisnaya çevirseydin `BoardAdapter` her bırakmayı `try/catch`
ile sarardı ve `catch` bloğu "hangi hata benim, hangisi oyuncunun" sorusunu **hata
mesajının metnine bakarak** cevaplardı. Enum, bir `string` karşılaştırmasına
dönüşürdü.

### ③ Tek `Rejected` değeri

```csharp
✗ public enum MoveOutcome { Rejected, Moved }
```

Sıfırıncı değer hâlâ doğru, isimler hâlâ okunaklı, tip hâlâ enum. Ve yine de
yanlış — bedeli yukarıdaki **"birleştirmenin bedeli"** tablosunda satır satır
yazılı. Kazanacağı gün de yazılı: sonucu yalnızca arayüz tüketseydi ve tek yaptığı
şey geçersiz tıklamada bir uyarı sesi çalmak olsaydı, üç değer aynı sesi çalmanın
üç yolu olurdu.

**Kendi tipini tasarlarken** ölçüt hep aynı: ret değerlerinin sayısı, çağıranın
**farklı davranışlarının** sayısıdır. Ne bir eksik, ne bir fazla.

---

## Bunu okuduktan sonra kodda ne göreceksin

Dört enum dosyasındaki bloklar artık kısa okunuyor: her biri **kendi** kararını ve
**kendi** reddedilen alternatifini söylüyor, ortak mekanizmayı anlatmıyor. Sıfırıncı
hücre gerekçesi `MoveOutcome.cs` ile `AttackOutcome.cs`'te bir kez yazılı, öteki
ikisi ona atıfta bulunuyor. Assembly duvarının tamamı `MoveOutcome.cs`'te,
`AttackOutcome.cs` yalnızca farkı söylüyor.

Kodda karar, burada hikâye. İkisi çelişirse **kod kazanır** — orası çalışan metin,
burası anlatı.

---

## ***SIRADAKİ ADIM***

> **▶ SIRADA:** [`04-karar-sirasi.md`](04-karar-sirasi.md) — okuma yolunun **7.** adımı
> **NEDEN ORASI:** **numara sırası `04 → 06` diyor; bağımlılık *tersini*
> diyor** ve sen doğru olanı izledin. Bu dosya ölçütü **kurdu** (dört enum, on
> bir ret değeri, tam tablo); `04` onu bir **sıra kararına uyguluyor**. Ayrıca
> `04`'ün iki körlüğü (`AttackRules` sırayı soramaz, `MoveAction` durumu soramaz)
> doğrudan **2. adımda** kapattığın duvara dayanıyor — iki ön koşulun da hazır.
> **YOL HARİTASI:** [`../../ogrenme/00-okuma-sirasi.md`](../../ogrenme/00-okuma-sirasi.md)
