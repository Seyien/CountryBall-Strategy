# Battle

> **Kaynak:** `Assets/Game/Battle/Battle.cs`
> **Ad alanı:** `GridStrategy.Battle` · **Assembly:** `GridStrategy.Battle`
> (`noEngineReferences: true`)
> **Rol:** Bileşik (Aggregate) — kimliği var, hafızası var; eşleşmeyi sahiplenir,
> kural yazmaz

Tahtada duran `Unit` ile ona eşlenen savaş parçası — `Combatant` ya da
`Structure` — arasındaki eşleşmenin tek sahibi.

Var olma sebebi assembly düzeyinde okunur: `GridStrategy.Core` konumu bilir ama
savaşı tanımaz, `GridStrategy.Combat` savaşı bilir ama tahtayı tanımaz — ve iki
assembly birbirini **GÖRMEZ**. Bu yüzden `AttackAction`'ın dışarıdan istediği
mesafeyi üretebilecek kimse yoktu. İkisini birden referans eden ilk assembly
burasıdır; bu tip o birleşmenin **DURUMUNU** tutar,
[`BattleActions`](BattleActions.md) ise **AKIŞINI**.

**SAHİPLENDİĞİ ÜÇ ŞEY:** kim nerede (tahta + iki sözlük), sıra kimde
([`Turn`](#turn)) ve kimin durumu değişti
([`UnitStateChanged`](#unitstatechanged)). Üçünün de ortak yanı aynı cümledir —
hiçbiri bir KURAL değildir, üçü de bir DURUMdur.

| Üye | Karar | Detay |
|---|---|---|
| `board` | tahta dışarıdan ALINMAZ; ikinci ok hiç doğmaz | [↓](#board) |
| `Board` (internal) | ok assembly dışına çıkmaz | [↓](#board-internal) |
| `combatants` | anahtar nesnenin KENDİSİ, hücre değil | [↓](#combatants) |
| `structures` | ikinci sözlük evet, ikinci tahta hayır | [↓](#structures) |
| `stateForwarders` | kapanış kimliği: sökmek için aynı örnek gerekir | [↓](#stateforwarders) |
| `Battle(int, int)` | sıra durumu savaşla birlikte doğar | [↓](#battleint-int) |
| `Width` / `Height` / `CellCount` / `IsInsideGrid` | tahtaya DEVREDER, kuralı kopyalamaz | [↓](#cellcount) |
| `UnitCount` | ad borcu: sayı doğru, ad geniş | [↓](#unitcount) |
| `Turn` | durumun sahibi ömrünü PAYLAŞAN şeydir | [↓](#turn) |
| `UnitStateChanged` | kimliği ekleyen basamak burada | [↓](#unitstatechanged) |
| `AddUnit(Unit, Combatant, int, int)` | bütün ret sebepleri ilk yazmadan ÖNCE | [↓](#addunitunit-combatant-int-int) |
| `AddStructure(Unit, Structure, int, int)` | `AddUnit`'in ikizi; abonelik yok | [↓](#addstructureunit-structure-int-int) |
| `ThrowIfCannotJoin(Unit, int, int)` | iki ekleme yolunun ortak kapısı | [↓](#throwifcannotjoinunit-int-int) |
| `RemoveUnit(Unit)` | aboneliği de bırakır; dönüş bool | [↓](#removeunitunit) |
| `Tick(float)` | kümeyi açmak döngüyü de dışarı taşır | [↓](#tickfloat-deltaseconds) |
| `RemoveReadyForCleanup(List)` | süpürmenin kaynağı SAVAŞ KAYDI | [↓](#removereadyforcleanuplist) |
| `TryGetPosition(Unit, out int, out int)` | konum önbelleğe ALINMAZ | [↓](#trygetpositionunit-out-int-out-int) |

**İlgili anlatılar:** [02-assembly duvarı](../../konular/02-assembly-duvari.md) ·
[03-tahta sahipliği](../../konular/03-tahta-sahipligi.md) ·
[01-olay zinciri](../../konular/01-olay-zinciri.md) ·
[05-yaşam döngüsü](../../konular/05-yasam-dongusu.md)

---

## board

**TAHTAYI BU TİP SAHİPLENİR, DIŞARIDAN ALMAZ.**

### HARİTA: "iki sahip" tam olarak ne demek

`UnitGrid` bir **SINIFtır** (`sealed class`) — yani REFERANS tipi. Bir referansı
parametre olarak vermek nesneyi KOPYALAMAZ, yalnızca ikinci bir ok açar; verenin
oku silinmez.

```
  REDDEDILEN — public Battle(UnitGrid board)
  ┌─BoardAdapter─┐                   ┌────Battle────┐
  │ board ───────┼────┐       ┌──────┼─ board       │
  └──────────────┘    ▼       ▼      └──────────────┘
                  ╔══════════════════╗
                  ║ UnitGrid nesnesi ║ ← TEK nesne, İKİ ok;
                  ╚══════════════════╝   ikisi de YAZABİLİR

  SEÇİLEN — public Battle(int width, int height)
  ┌─BoardAdapter─┐                   ┌────Battle────┐
  │  (alan YOK)  │                   │ board ───────┼──┐
  └──────────────┘                   └──────────────┘  ▼
                                 ╔═══════════════════════╗
                                 ║ nesne KURUCUDA doğdu; ║
                                 ║ dışarıda ok HİÇ VAR   ║
                                 ║ OLMADI                ║
                                 ╚═══════════════════════╝
```

Fark bir **YASAK** değil, bir **İMKÂNSIZLIK**: ikinci ok engellenmiyor, hiç
doğmuyor. Engellenen bir şey unutulabilir; doğmayan şey unutulamaz.

### KIRILMA ZİNCİRİ (reddedilen imza seçilseydi)

```
BoardAdapter kendi okundan board.PlaceUnit(u, x, y) çağırır
  -> tahtada bir Unit durur
  -> Battle.combatants sözlüğünde o Unit'in karşılığı YOKTUR
     (tek kayıt yolu Battle.AddUnit'ti ve o yol atlandı)
  -> BattleActions.Attack -> TryGetCombatant başarısız
  -> "bu savaşta değil" diye patlar
derleyici: ayrışmayı GÖSTEREMEZ  ·  test: yeşil kalır
```

### "İKİNCİSİNİ KOD SÖYLEMEZ"

`public Battle(UnitGrid board)` imzası "tahtayı alıyorum" der; "sen de tutmaya
devam edersen bu savaş sessizce bozulur" **DEMEZ**. Sözleşmenin taşıdığı risk
imzada görünmez, yalnızca yorumda yaşayabilir — ve yorum derlenmez. Reddedilme
sebebi budur.

### `readonly` BU KIRILMAYA KARŞI SIFIR KORUMA SAĞLAR

`readonly` yalnızca **ALANI** kilitler, nesnenin **İÇİNİ** değil:

```csharp
board = new UnitGrid(5, 5);   // ✗ derleme hatası
board.PlaceUnit(u, 2, 3);     // ✓ tamamen serbest
```

Yani koruma `readonly`'den gelmiyor; "dışarıda hiç referans yok" olgusundan
geliyor. Aynı yanılgının kardeşi
[`Health.Current`](../Core/Combat/Health.md#current)'ta get-only property için
yazılı.

### SAHİPLİĞİ AYAKTA TUTAN ÜÇ KATMAN

```
  1. Kurucu `new UnitGrid(...)` yapar  -> doğumda dış ok yok
  2. `internal UnitGrid Board`         -> ok assembly dışına çıkmaz.
     BoardAdapter GridStrategy.Unity assembly'sinde yaşar ve bu üyeyi
     GÖREMEZ. public olsaydı 1. katman aynı gün boşa çıkardı.
  3. `private readonly`                -> en zayıf katman; yalnızca
     yeniden atamayı keser
```

**Garantinin sınırı:** `BattleActions` AYNI assembly'de olduğu için `Board`'a
erişebilir. Orada "tek yazar" sözünü tutan şey kod değil, `BattleActions`'ın
kendi disiplinidir. Sözleşme assembly duvarında biter — bunu bilerek kabul ettik.

### KAZANIRDI

Tahta savaştan ÖNCE ve BAŞKA bir yerde doluyorsa — kayıt dosyasından yüklenen bir
kuşatma, seviye editöründen gelen hazır dizilim; o gün kurucu "kurulmuş tahtayı
devral"ı reddedemez. Fatura kaybolmaz, sahibi değişir: o senaryoda ya kurucu
tahtayı KOPYALAYARAK almalı ya da "verdikten sonra okunu bırak" sözleşmede açıkça
yazılı olmalıdır.

### KANIT

`BoardAdapter`'da bir `UnitGrid` alanı **YOKTUR**; bir zamanlar vardı ve silindi
(bkz. oradaki `private Battle battle;` alanının üstündeki not). Yukarıdaki
kırılma bu yüzden bir varsayım değil, kapanmış bir borcun kaydıdır.

**TEK CUMLE:** Bir bileşik, değişmezini koruduğu şeyi dışarıdan almaz — aldığı
anda ikinci bir yazar doğar ve onu derleyici görmez.

---

## Board (internal)

Tahta **DIŞARIYA** açılmıyor ama assembly içinde açık: `BattleActions`'ın
`MoveAction.Execute`'a verecek bir `UnitGrid`'e ihtiyacı var ve o tip bu
assembly'de yaşıyor.

`public` olsaydı sahiplik sözü tek satırda çözülürdü — herkes `board.PlaceUnit`
çağırıp sözlüğü atlayabilirdi ve yukarıdaki [iki sahip](#board) kırılması geri
gelirdi. `internal`, sözü **derleyiciye söyletir**: dışarıdan tahtaya yol yok.

---

## combatants

**ANAHTAR NEDEN `Unit`:** `BoardAdapter`'ın `unitViews` alanının üstündeki
gerekçe burada aynen geçerli ve **DEVRALINIYOR** — konum yalnız tahtada yaşasın
diye. Orada görsel "neredeyim" bilmiyordu; burada savaşçı bilmiyor. `Unit` bir
sınıftır, varsayılan karşılaştırma referans eşitliğidir ve aradığımız zaten tam
olarak o nesnenin kendisi.

### HARİTA: hareket anında ANAHTARA ne olur

```
  REDDEDILEN — Combatant[,] , anahtar = HÜCRE
    önce   [2,3] ──► Combatant(A)
    board.MoveUnit(2,3 ► 2,4)   ◄── MoveAction doğrudan çağırır
    sonra  [2,3] ──► Combatant(A)   ██ ANAHTAR BOZULDU ██
           [2,4] ──► (boş)          birim yeni hücrede,
                                    canı eski hücrede

  SEÇİLEN — Dictionary<Unit,Combatant> , anahtar = NESNE
    önce   Unit(A) ──► Combatant(A)
    board.MoveUnit(2,3 ► 2,4)
    sonra  Unit(A) ──► Combatant(A)  ██ DEĞİŞMEDİ ██
           anahtar tahtaya HİÇ bakmıyor; kırılacak bağ yok
```

Fark bir dikkat farkı değil: reddedilen şekilde anahtarı güncel tutmak birinin
**HATIRLAMASINA** bağlı, seçilen şekilde güncellenecek bir şey yok.

### KAPSAM: "hücreyi anahtar yapma" diye bir kural YOK

Ölçüt: anahtar, hareketle **DEĞİŞEN** bir şey mi?

**KARŞI ÖRNEK** aynı dosyada, [`board`](#board) alanının kendisi: `UnitGrid` tam
olarak hücreyle anahtarlanmıştır (`[x,y] ──► Unit`) ve doğrusu da budur — onun
cevapladığı soru "bu hücrede kim var" ve hareketi yazan tek yol `MoveUnit` olduğu
için anahtar ile içerik birlikte taşınır. Orada hücre anahtar olmakla doğru,
burada yanlış; ayıran şey tablonun şekli değil, güncellemenin TEK yolla yapılıp
yapılmadığı.

### İŞ BÖLÜMÜ: board ile combatants ÖRTÜŞMEZ, BÖLÜŞÜR

```
  board       "hangi hücrede KİM"   ► konum   (UnitGrid sahibi)
  combatants  "o kim HANGİ PARÇA"   ► eşleşme (bu tip sahibi)
```

İkisi hiçbir soruyu ortak cevaplamıyor: `board` bir `Combatant` tanımaz,
`combatants` bir koordinat tanımaz. `board` silinirse konum sahipsiz kalır ve
[`TryGetPosition`](#trygetpositionunit-out-int-out-int)'ın soracağı yer kalmaz;
`combatants` silinirse tahtada duran bir `Unit`'in savaş karşılığı bulunamaz ve
`AttackAction` mesafeyi kime uygulayacağını bilemez. Reddedilen şekil bu bölüşmeyi
bozardı — konum İKİ yerde yaşardı.

### REDDEDILEN

Savaş durumu hücreye yazılır:

```csharp
private readonly Combatant[,] combatantsByCell;
```

**KIRILAN:** konum İKİ yerde yaşar ve ayrıştırmak tek satır alır.

```
MoveAction.Execute tahtanın MoveUnit'ini doğrudan çağırır
  -> dizinin bundan haberi olmaz
  -> birim yeni hücresinde durur, canı eski hücrede kalır,
     saldıran hayaleti vurur
derleyici: hiçbir şey der
test: kırmızı — Move_ThenAttack_UsesTheNewPosition
```

**KARSILASTIRMA:**

```
  List<Combatant>      anahtar YOK      -> her soruda taramak zorundasın
  Combatant[,]         anahtar = hücre  -> birim hareket edince anahtar bozulur
  Dictionary<Unit,..>  anahtar = birim  -> birimin kendisi kalır, bozulmaz
```

**KAZANIRDI:** savaş durumu birime değil **HÜCREYE** ait olsaydı — yanan zemin,
üstünde duranı zehirleyen bataklık, tetiklenmiş tuzak; o gün durumun sahibi
hücredir ve birim üstünden geçen geçici bir ziyaretçidir.

**TEK CUMLE:** Sözlükte anahtar nesnenin KENDİSİ, dizide nesne HAKKINDA bir bilgi;
bilgi değişir, nesne değişmez.

---

## structures

**İKİNCİ SÖZLÜK, İKİNCİ TAHTA DEĞİL.** Yapılar birimlerle AYNI `UnitGrid`'e
giriyor; ayrışan tek şey "bu kimliğe hangi savaş parçası eşlendi" sorusudur ve o
soru zaten sözlüğün cevapladığı sorudur.

### HARİTA: kaç tahta, kaç defter

```
  SEÇİLEN — TEK tahta, İKİ defter
  ╔═════════ UnitGrid ═════════╗  "bu hücre dolu mu" ► TEK SORU
  ║ [2,3] ──► Unit(asker)      ║
  ║ [4,1] ──► Unit(baraka)     ║  ██ DOLULUĞUN TEK SAHİBİ ██
  ╚════════════╤═══════════════╝
         ┌─────┴──────┐
         ▼            ▼
    combatants    structures   ◄── yalnız "bu KİMLİK ne" sorusu

  REDDEDILEN — İKİ tahta
  ╔═ UnitGrid ═╗      ╔═ structureBoard ═╗
  ║ [2,3] asker║      ║ [2,3] baraka     ║ ◄── ██ AYNI HÜCRE ██
  ╚════════════╝      ╚══════════════════╝     ve çelişen kimse yok
  "dolu mu" sorusu İKİYE bölünür; dört yerleştirme yolundan biri
  ikinci tahtayı sormayı unutur ── ██ KIRILMA NOKTASI ██
```

### KAPSAM: ikiye bölmek yasak DEĞİL

Ölçüt: bölünen şey bir **DEPO** mu, yoksa bir **SORUNUN CEVABI** mı?

**KARŞI ÖRNEK** hemen yukarıda: [`combatants`](#combatants) ile `structures` da
iki ayrı defter ve bu kabul edilmiş bir bölünme, çünkü ikisi tek bir soruyu ("bu
kimlik ne") bölüşüyor ve tam olarak biri `true` dönüyor — güvencesi
`TryGetStructure`'ın özetinde yazılı. `structureBoard` ise aynı sorunun cevabını
ikiye bölerdi ve iki cevabın ikisi de "hayır" diyebilirdi.

### İŞ BÖLÜMÜ: doluluk ile kimlik ayrı mekanizmalarda

```
  board                   ► "hücre dolu mu"   tek soru, tek yer
  combatants + structures ► "kimlik ne"        bölüşülmüş
  ThrowIfCannotJoin       ► bölüşmenin kelepçesi
```

[`ThrowIfCannotJoin`](#throwifcannotjoinunit-int-int) silinirse aynı `Unit` iki
deftere birden girer ve "tam olarak biri true döner" sözü düşer;
[`RemoveReadyForCleanup`](#removereadyforcleanuplist) aynı kimliği tampona iki kez
yazar. `board` silinirse doluluk sorusu iki deftere sorulmak zorunda kalır ve
reddedilen şekle geri dönülür.

### REDDEDILEN

Yapılara kendi tahtası verilir:

```csharp
private readonly UnitGrid structureBoard;
private readonly Dictionary<Structure, ...> structuresByCell;
```

**KIRILAN:** "bu hücre dolu mu" sorusu İKİYE bölünür.

```
dört yoldan biri ikinci tahtayı sormayı atlar
  -> aynı hücrede asker ile baraka durur
  -> mesafe iki ayrı koordinat uzayından ölçülür ve cevap sessizce
     yanlış çıkar
derleyici: hiçbir şey der  ·  test: yeşil kalır
```

**KAZANIRDI:** yapılar tahtanın ÜSTÜNDE değil **ALTINDA** yaşasaydı — zemin
katmanı, üstünden yürünen köprü, birimin üzerinde durduğu platform; o gün "aynı
hücrede iki şey" bir hata değil tasarımın kendisidir ve tek tahta onu ifade
EDEMEZ.

**TEK CUMLE:** İki tahta iki gerçek demektir; aynı soruya iki yerden cevap veren
bir sistem, ikisi ayrıştığı gün hangisinin doğru olduğunu söyleyemez.

---

## stateForwarders

**OLAY YÖNLENDİRİCİLERİ.** Her savaşçı için BİR tane, `AddUnit`'te kurulur ve
`RemoveUnit`'te sökülür.

Sözlük bir konfor değil **zorunluluk**: `Combatant.StateChanged` imzası
`Action<UnitState, UnitState>` ve **GÖNDEREN** taşımıyor, dolayısıyla kimliği
ekleyen şey her birime özel bir **KAPANIŞ** (closure) olmak zorunda. Kapanışlar
birbirine eşit değildir — `combatant.StateChanged -= (f, t) => ...` yazarak
abonelik çözülemez, çünkü ikinci lambda birinciyle aynı nesne değildir. Yani
sökmek için tam olarak ABONE OLUNAN örneği saklamak gerekir.

### HARİTA: kapanış KİMLİĞİ (closure identity)

```
  AddUnit'te bir kez yazılıyor:
    forwarder = (p, n) => UnitStateChanged?.Invoke(unit, p, n)
    ╔══ delege nesnesi #1 ══╗──abone──► Combatant.StateChanged
    ╚═══════════════════════╝
    └──saklanıyor──► stateForwarders[unit]

  RemoveUnit'te aynı METİN yeniden yazılsaydı:
    ╔══ delege nesnesi #2 ══╗──✗ eşit DEĞİL──► çözülemez
    ╚═══════════════════════╝
    -= sessizce hiçbir şey yapmaz  ◄── ██ ABONE YERİNDE KALIR ██
```

██ Eşitlik ölçütü **METİN** değil **NESNE** ██ — bu bir dil kuralı (delegate
eşitliği hedef + metot çiftine bakar ve her lambda ifadesi yeni bir örnek üretir),
bir Unity ayrıntısı değil.

### KAPSAM: her abonelik saklanmaz

Ölçüt: aboneliği çözecek gün, elde AYNI nesne kalacak mı?

**KARŞI ÖRNEK** aynı dosyada,
[`AddStructure`](#addstructureunit-structure-int-int): orada hiçbir abonelik
kurulmuyor (`StructureLifecycle` bilerek olaysız), dolayısıyla saklanacak bir şey
de yok. Bir metot adı doğrudan abone edilseydi
(`c.StateChanged += OnStateChanged`) yine saklamaya gerek kalmazdı — aynı metot
grubu her seferinde eşit sayılır. Saklamayı zorunlu kılan şey **ABONELİK** değil,
**KAPANIŞ**.

**Maliyet** kare başına değil, birim başına: `AddUnit`'te bir delege ve bir sözlük
girdisi. `Tick` sıcak yolunda tek bir tahsis yok.

---

## Battle(int, int)

Belirtilen ölçüde boş bir savaş kurar. Ölçü doğrulaması burada **KOPYALANMIYOR**;
`UnitGrid` kendi kurucusunda zaten yapıyor ve tek sahibi odur.

**Sıra durumu savaşla birlikte DOĞAR ve savaşla birlikte ölür.** Sonradan
atanabilir olsaydı, sırası henüz kurulmamış bir savaşta `Turn.Current` okumak
`NullReferenceException` verirdi ve "savaşı kurmayı unuttum" hatası ilk tıklamada
değil ilk sıra sorusunda görünürdü.

---

## CellCount

`Width`, `Height`, `CellCount` ve `IsInsideGrid`, dördü de **tahtaya DEVREDİYOR**,
kuralı kopyalamıyor.

`CellCount` ile `IsInsideGrid` **çağıranı olduğu için** var: `BoardAdapter` kendi
`UnitGrid board` alanını tutmuyor ([iki sahip gerekçesi](#board)), dolayısıyla
tahtaya soracağı iki soruyu buraya soruyor — tıklanan hücre tahtanın içinde mi
(`BoardAdapter.HandleClick`) ve kaç hücre kuruldu
(`BoardAdapter.BuildCellVisuals`'ın kapanış kaydı).

Sınır kuralının tek metni `UnitGrid.IsInsideGrid`'de, çarpım ise
`UnitGrid.CellCount`'ta yaşıyor. Buraya `x >= 0 && x < Width` yazılsaydı aynı
kural iki yerde yaşardı ve ikisi ayrışınca hangisinin doğru olduğunu derleyici
söyleyemezdi.

---

## UnitCount

**AD BORCU, AÇIKÇA YAZILIYOR:** `Unit`'in özetinin ilk cümlesi "tahtada yer
kaplayan, kimliği olan şey" diyor, dolayısıyla `UnitCount` adı olduğundan geniş
okunuyor — sayacağı şey **SAVAŞÇI**, tahtadaki her şey değil. S-11'in aynı sınavı:
sayı doğru, ad geniş.

### HARİTA: adın KAPSADIĞI küme ile SAYILAN küme

```
  ┌──────────────── UnitGrid ────────────────┐
  │  ┌─ combatants ─┐    ┌─ structures ─┐    │
  │  │   asker x3   │    │   baraka x2  │    │
  │  └──────────────┘    └──────────────┘    │
  └──────────────────────────────────────────┘
        ▲                        ▲
        │ UnitCount BUNU sayar   │ ad BUNU da kapsıyor gibi
        ██ 3 ██                  ██ okuyucunun beklediği 5 ██
```

Ayrışma noktası imzada **DEĞİL**: `int UnitCount` iki dünyada da aynı satır.
Ayrışan tek şey dönen sayı — derleyicinin göremediği tek değişiklik türü budur.

### KAPSAM: bu borç TEK bir üyeye ait

Ölçüt kesişim testi: adın kapsadığı küme = sayılan küme mi?

```
  CellCount       ad: her hücre     sayar: her hücre     ✓
  StructureCount  ad: yapılar       sayar: yapılar       ✓
  UnitCount       ad: her Unit      sayar: savaşçılar    ✗
```

**KARŞI ÖRNEK** aynı dosyada, hemen aşağıdaki `StructureCount` ile yukarıdaki
[`CellCount`](#cellcount): ikisinin de adı sayısı kadar geniş ve ikisine de böyle
bir not düşülmedi. Borcun kaynağı bu tipin sayma alışkanlığı değil, TEK bir
kelime: `Unit` bu projede "tahtada yer kaplayan, kimliği olan şey" demek.

### İŞ BÖLÜMÜ: iki sayaç, üçüncüsü YOK

```
  UnitCount       ► savaşçılar   yenilgi koşulunun sorusu
  StructureCount  ► yapılar      inşa/enkaz sorusu
  (toplam)        ► YOK          çağıranı doğduğu gün eklenir
```

İkisi ÖRTÜŞMÜYOR: aynı `Unit` iki sayaçta birden olamaz — güvencesi
[`ThrowIfCannotJoin`](#throwifcannotjoinunit-int-int). `UnitCount` toplamı
dönseydi yenilgi koşulu sessizce yanlışlanırdı; `StructureCount` silinirse enkaz
sayısı çağıranda sözlük taranarak kurulurdu.

### REDDEDILEN

Ad korunur, **ANLAM** genişletilir:

```csharp
public int UnitCount => combatants.Count + structures.Count;
```

**KIRILAN:** imza değişmez; kırılan şey cevabın kendisidir ve sessizdir.

```
tahtaya bir baraka konur
  -> "kaç askerim kaldı" bir fazla sayar
  -> yenilgi koşulu bu sayıya bağlandığı gün oyuncu, sahada tek askeri
     yokken deposu ayakta diye kaybetmez
derleyici: hiçbir şey der
test: kırmızı —
      AddUnit_OnACellHeldByAStructure_ThrowsAndKeepsTheStandingStructure
```

**KAZANIRDI:** yenilgi koşulu gerçekten "tahtada hiçbir şeyin kalmasın" olsaydı ve
savaşçı ile yapı ayrımı hiçbir sayımda gerekmeseydi — o gün tek bir toplam, iki
sayacı toplayan her çağıranı bir satırdan kurtarırdı.

**TEK CUMLE:** Bir adın anlamını genişletmek imzayı değil yalnızca cevabı
değiştirir — derleyicinin göremediği tek değişiklik budur.

**EŞİK:** dışarıdan "tahtada kaç şey var" diye soran ilk çağıran doğduğu gün
üçüncü bir üye (`BoardPieceCount`) eklenir; iki sayacın toplamını çağıranda
kurmak, aynı toplamı her çağıranda yeniden yazmak olur.

---

## Turn

**SIRA DURUMUNUN SAHİBİ BU TİP.** Sıra bir **DURUM**dur ("şu an kimde") ve bu
dosyanın rol başlığındaki söz tam olarak durumu sahiplenmektir; sıranın NE ANLAMA
GELDİĞİ ise bir kuraldır ve [`TurnRules`](TurnRules.md)'ta yaşıyor. İkisi
birbirine karışmıyor: burada tek bir izin/yasak satırı yok.

### HARİTA: durumun sahibi = ömrünü PAYLAŞAN

```
  ömür ekseni ►
  süreç  ├─────────────────────────────────────────────────┤
  sahne  │      ├───────────────────────────────┤          │
  savaş  │      │  ├─savaş#1─┤     ├─savaş#2─┤  │          │

  static alan    sahip = SÜREÇ  ► iki savaş TEK sırayı paylaşır
  BoardAdapter   sahip = SAHNE  ► savaş Unity'siz koşamaz
  Battle örneği  sahip = SAVAŞ  ◄── ██ ÖMÜRLER TAM ÇAKIŞIYOR ██
```

Sıra savaşla doğar, savaşla ölür; üstteki iki kutu da savaştan **UZUN** yaşıyor ve
fazlalık ömrün bedeli sızıntıdır — birinde test metotları arasına, diğerinde
sahneye.

### KAPSAM: static'in kendisi yasak değil

Ölçüt: saklanan şey **DURUM** mu, **TANIM** mı? Tanım süreç ömrü alabilir.

**KARŞI ÖRNEK** aynı ad alanında,
[`TurnState.DefaultTurnOrder`](TurnState.md#defaultturnorder): o üye
`public static readonly` ve doğrusu bu — bir dizilim **TANIM**ıdır, hiçbir savaşa
ait değildir ve iki savaşın onu paylaşması bir sızıntı değil, amacın kendisidir.
[`TurnRules.MaxActionsPerTurn`](TurnRules.md#maxactionsperturn) de aynı sınıfta.
Ayıran şey `static` kelimesi değil, o alanın savaştan savaşa DEĞİŞİP değişmediği.

### `{ get; }` BU KIRILMAYA KARŞI KISMİ KORUMA SAĞLAR

Get-only property yalnızca **HANGİ NESNE** olduğunu kilitler, nesnenin **İÇİNİ**
değil:

```csharp
battle.Turn = new TurnState();   // ✗ derleme hatası
battle.Turn.EndTurn();           // ✓ tamamen serbest
```

İkincisi zaten **İSTENEN** şey — sıra devri budur. Yani buradaki koruma "tur
numarası ortada sıfırlanamaz"dır; "sıra değişemez" değil.

### İŞ BÖLÜMÜ: DURUM burada, KURAL TurnRules'ta

```
  Battle.Turn        ► "sıra kimde"        durum, örnek başına
  TurnRules.CanAct   ► "ondan ne çıkar"    kural, static ve saf
```

`Turn` silinip kural buraya yazılsaydı sıra bilgisini isteyen her arayüz kural
motorunu da yanında taşırdı. `TurnRules` silinip karşılaştırma buraya yazılsaydı
kural savaş kurmadan sınanamaz hâle gelirdi. İkisi ÖRTÜŞMÜYOR: biri bir olguyu
tutuyor, diğeri o olgudan bir izin üretiyor.

### REDDEDILEN (1)

Sıra durumu Unity katmanına, `BoardAdapter`'a taşınır ve savaş onu hiç bilmez:

```csharp
// BoardAdapter içinde:
private readonly TurnState turn = new TurnState();
private void OnEndTurnButton() { turn.EndTurn(); }
```

**KIRILAN:** sıra kuralı EditMode'da sınanamaz hâle gelir.

```
akış sırayı SORMAK zorunda, elindeki tek şey Battle
  -> sırayı parametre olarak ister
  -> yanlış TurnState geçen kod DERLENİR, ya da hiç sormaz ve sıra
     sistemi yalnızca ekranda var olur
derleyici: hiçbir şey der
test: sahne kurmayı, yani PlayMode'a inmeyi zorunlu kılar
```

**KARSILASTIRMA:**

```
  BoardAdapter     sahip = ekran    -> savaş Unity'siz koşamaz, kural sınanamaz
  static alan      sahip = süreç    -> iki savaş tek sırayı paylaşır
  Battle örneği    sahip = savaş    -> her savaşın kendi sırası olur
```

**KAZANIRDI:** sıra gerçekten bir **SUNUM** kavramı olsaydı — savaşın kendisi
eşzamanlı çözülüp arayüz onu sırayla GÖSTERSEYDİ (auto-battler'ın tam olarak
yaptığı şey); o gün `TurnState` bir oynatma kafası olurdu ve savaşta karşılığı
olmazdı.

**TEK CUMLE:** Bir durumun sahibi onu DEĞİŞTİREN değil, ömrünü PAYLAŞAN şeydir —
sıra ekranla değil savaşla doğar ve savaşla ölür.

### REDDEDILEN (2)

Durum hiç doğmaz, akış sahibi kendi `static` örneğini tutar:

```csharp
// BattleActions içinde:
private static readonly TurnState Turn = new TurnState();
```

**KIRILAN:** yukarıdaki tablonun "static" satırının ölçülebilir yüzü.

```
NUnit bütün testleri aynı süreçte koşar
  -> durum test metotları ARASINDA sızar
  -> her testin başındaki yeni savaş temiz sayfa vermez ve testler
     KOŞMA SIRASINA göre geçer
derleyici: hiçbir şey der  ·  test: yeşil, ama yalanı yeşil
```

**KAZANIRDI:** süreç ömrü boyunca ikinci bir savaş asla kurulmayacaksa — tek
oyunculu, tek sahneli, önizlemesiz bir oyun; o gün örnek başına durum, iki değer
almayacak bir alanın töreni olurdu.

**TEK CUMLE:** `static` durum, testin komşusunu testin girdisi yapar.

---

## UnitStateChanged

**ZİNCİRİN SON HALKASI** — kimliği ekleyen yer burası, ve başka bir yer olamaz:
`Combatant` kendi `Unit`'ini **BİLMEZ**, çünkü kimlik parçalarda değil bu sözlükte
yaşıyor.

```
  UnitLifecycle.StateChanged  Action<UnitState>              hangi duruma
  Combatant.StateChanged      Action<UnitState, UnitState>   nereden nereye
  Battle.UnitStateChanged     Action<Unit, UnitState, ...>   KİM, nereden nereye
```

### HARİTA: kimliğin EKLENDİĞİ basamak

```
  UnitLifecycle.StateChanged        "hangi duruma"
         ▲  Combatant devreder
  Combatant.StateChanged            "nereden nereye"
         ▲  stateForwarders kapanışı  ██ KİMLİK BURADA EKLENİR ██
  Battle.UnitStateChanged           "KİM, nereden nereye"
```

Basamağı ekleyebilecek ilk yer, kimliği ilk gören yerdir — yani burası.

### KAPSAM: her parça bu zincire katılmaz

Ölçüt: geçişi **SORAN** biri var mı? Varsa dönüş değeri yeter.

**KARŞI ÖRNEK** aynı dosyada,
[`AddStructure`](#addstructureunit-structure-int-int): orada hiçbir yönlendirici
kurulmuyor ve bu bir unutma değil — `Structure`'ın tek geçişi (ayakta → yıkık) her
zaman bir hasar **ÇAĞRISINDAN** doğar ve o çağrıyı yapan taraf cevabı zaten dönüş
değeriyle alır. Savaşçı tarafında Downed → Dead geçişini `Tick` yapar ve `Tick`'i
çeviren taraf onunla ilgilenmez; olayı doğuran fark tam olarak budur.

### İŞ BÖLÜMÜ: olay ile süpürme ÖRTÜŞMEZ

```
  UnitStateChanged        ► "DURUM DEĞİŞTİ"     savaşçılar
  RemoveReadyForCleanup   ► "ARTIK SİLİNEBİLİR" savaşçılar+yapılar
```

Ayrım tek bir `Tick`'te görünür: ceset sayacının dolduğu `Tick` hiçbir **DURUM**
değiştirmez, yalnızca bir bayrak açar — o an süpürme bir şey bulur, olay hiç
tetiklenmez. Olay silinirse Downed → Dead geçişini duyan kimse kalmaz (görsel eski
durumda donar); süpürme silinirse enkaz sonsuza dek tahtada kalır ve `Structure`'ın
hiçbir olayı olmadığı için bunu bildiren tek yol da yok olur.

**Toplu süpürme bu olayın yerine GEÇMEZ ve KALDIRILMIYOR** — ikisi farklı sorulara
cevap veriyor.

### REDDEDILEN

Abonelik kurulur ama `RemoveUnit`'te **BIRAKILMAZ**; kayıt silindiği için nasıl
olsa duyulmaz varsayılır:

```csharp
combatants.Remove(unit);   // stateForwarders'a hiç dokunulmaz
```

**KIRILAN:** varsayım yalnızca `Battle.Tick` yolu için doğru.

```
çıkarılan savaşçıya elde kalan referanstan Tick gelir
  -> savaşta OLMAYAN bir birim için kimlikli olay yayılır
  -> delege bu savaşı tuttuğu için o birim çöp de olamaz
derleyici: hiçbir şey der  ·  test: yeşil kalır
```

**KAZANIRDI:** `Combatant` savaştan çıkarken KENDİSİ de atılıyor olsaydı — bu tip
onu kuran taraf olsaydı ve dışarıya hiç vermeseydi; o gün aboneliği bırakmak,
birlikte çöpe giden iki nesne arasındaki bağı elle koparma töreni olurdu.

**TEK CUMLE:** Abonelik bir alan değil bir ÖMÜR sözleşmesidir; bırakmayı unutan
taraf hem yanlış cevap yayar hem nesneyi bellekte tutar.

---

## AddUnit(Unit, Combatant, int, int)

Bir birimi savaşa katar: tahtaya yerleştirir ve savaş durumuyla eşler. Üçü —
birim, savaşçı, konum — **TEK çağrıda** gelir; çünkü ayrılırlarsa aralarında yarım
kalmış bir hâl doğar.

### İKİNCİ KELEPÇE: `combatants.ContainsValue`

Yönü tersi. Üstteki kural ([`ThrowIfCannotJoin`](#throwifcannotjoinunit-int-int))
"bu **KİMLİK** zaten içeride mi" diye sorar; bu "bu **PARÇA** zaten başka bir
kimliğe bağlı mı" diye sorar.

```
  ThrowIfCannotJoin   Unit ──?──► sözlükler   (ANAHTAR yönü)
    "bu KİMLİK zaten içeride mi"
  ContainsValue       sözlükler ──?──► Combatant (DEĞER yönü)
    "bu PARÇA zaten başka bir kimliğe bağlı mı"

  Yalnız ilki yazılsaydı açık kalan yön SESSİZ:
    Unit(A) ─┐
             ├──► Combatant(X)  ██ İKİ forwarder ██
    Unit(B) ─┘                  tek geçiş İKİ kimlikle yayılır
                                dinleyici aynı ölümü iki kez
                                görür, derleyici susar
```

**Maliyet dürüstçe:** `ContainsValue` sözlüğü baştan sona tarar (O(n)). 3x5 tahtada
bu ölçülemez; **EŞİK**, tahtanın onlarca birime çıktığı ve eklemenin sıcak yola
girdiği gündür.

**Alternatif:** eşleşmeyi ters yönde de tutan ikinci bir sözlük
(`Dictionary<Combatant, Unit>`). Seçilmedi: arama sabit zamana iner ama eşleşme
**ÜÇÜNCÜ** bir yerde yaşamaya başlar ve üçünün güncel kalması `RemoveUnit`'in
dikkatine bağlanır; tetiği üstteki EŞİK.

### SIRA BİR KARARDIR: sorular önce, yazmalar sonra

Bütün ret sebepleri **HİÇBİR ŞEY yazılmadan** sorulur, sonra iki yazma arka arkaya
yapılır. `PlaceUnit` tahta dışı koordinatta kendi kontrolünü yazmadan önce yapar,
dolayısıyla bu noktadan sonra patlayabilecek tek şey odur ve o da tahtaya
dokunmadan patlar.

```
  SEÇİLEN
    ThrowIfCannotJoin          kimlik içeride mi     SORU
    combatants.ContainsValue   parça bağlı mı        SORU
  ────────────────────────────────────────────────────────
    board.PlaceUnit(...)       ██ YAZMA 1 ██
    combatants.Add(...)        ██ YAZMA 2 ██
    StateChanged += forwarder  ██ YAZMA 3 ██

  REDDEDILEN — soru yok, çakışmayı Dictionary.Add bildirir
    board.PlaceUnit(...)       ██ YAZMA 1 ██ ── BAŞARILI
    combatants.Add(...)        ✗ patlar
  ◄── ██ YARIM KALMA ██ hata mesajı doğru, tahta yanlış:
      aynı Unit İKİ hücrede birden durur
```

**KAPSAM: çizginin altında patlamak her zaman yasak değil**

Ölçüt: patlayan şey patlamadan **ÖNCE** bir şey yazıyor mu?

**KARŞI ÖRNEK** bu bloğun kendi konusu: tahta dışı koordinat denetimi buraya
KOPYALANMADI ve `PlaceUnit`'in kendi kontrolüne bırakıldı — o kontrol hiçbir
hücreye dokunmadan patlıyor, dolayısıyla çizginin altında olması zararsız. Aynı
satırda duran iki denetimden biri yukarı taşındı, diğeri taşınmadı; ayıran şey
sırayla ilgili bir tercih değil, **yan etkinin varlığı**.

**İŞ BÖLÜMÜ: üç yazma, tek geri alma noktası**

```
  YAZMA 1  board       hücreyi tutar   ► RemoveUnit boşaltır
  YAZMA 2  combatants  eşleşmeyi kurar ► RemoveUnit siler
  YAZMA 3  abonelik    kimliği ekler   ► RemoveUnit çözer
```

Üçü de aynı ret kapısının **ARKASINDA** duruyor ve bu yüzden hepsi ya olur ya hiç
olmaz. Abonelik bilerek **EN SONDA**: reddedilen bir ekleme geriye tek bir abone
bile bırakmamalı. Sıra ters olsaydı `PlaceUnit`'in patladığı çağrı, savaşta
olmayan bir birim için kimlikli olay yayan bir delege bırakırdı.

### REDDEDILEN

Ön kontrol yok, çakışmayı `Dictionary.Add`'in kendi hatası bildirir:

```csharp
board.PlaceUnit(x, y, unit);
combatants.Add(unit, combatant);
```

**KIRILAN:** YARIM KALMA — `UnitGrid.MoveUnit`'in var olma sebebi.

```
aynı Unit başka bir hücreye ikinci kez eklenir
  -> PlaceUnit ÇOKTAN yazmıştır
  -> Dictionary.Add patlar ve birim tahtada İKİ hücrede birden durur
derleyici: hiçbir şey der
test: kırmızı — AddUnit_SameUnitTwice_ThrowsAndLeavesTheFirstCellUntouched
```

**KAZANIRDI:** `Battle` tahtayı **SAHİPLENMESEYDİ** — yerleştirme dışarıda
yapılsaydı bu metot yalnız sözlüğe yazardı ve `Dictionary.Add`'in kendi hatası
fazlasıyla yeterdi.

**TEK CUMLE:** İki yazma varsa bütün ret sebepleri ilk yazmadan ÖNCE sorulur;
yoksa hata mesajı doğru, tahta yanlış kalır.

---

## AddStructure(Unit, Structure, int, int)

Bir yapıyı savaşa katar: **BİRİMLERLE AYNI** tahtaya yerleştirir ve yapı durumuyla
eşler. Şekli [`AddUnit`](#addunitunit-combatant-int-int)'in birebir ikizidir ve bu
tesadüf değil — ikisi de aynı değişmezi koruyor: her kayıtlı parçanın tahtada tam
olarak bir hücresi vardır.

Bir `Unit` aynı anda hem savaşçı hem yapı **OLAMAZ**; ikinci kayıt reddedilir.
İzin verilseydi tek hücrede iki savaş parçası yaşardı, `Tick` ikisini birden
işletirdi ve `RemoveReadyForCleanup` aynı kimliği listeye iki kez yazardı — çağıran
aynı görseli iki kez silmeye çalışırdı.

**ABONELİK YOK, ve bu bir unutma değil:** `StructureLifecycle` bilerek olaysızdır.
Buraya bir yönlendirici yazmak, olmayan bir olaya abone olmaya çalışmak olurdu —
kod derlenmezdi bile. Gerekçenin uzun hâli [`UnitStateChanged`](#unitstatechanged)
başlığında.

Parça kelepçesi ([`ContainsValue`](#addunitunit-combatant-int-int)) `AddUnit`'teki
ile ikiz; gerekçe orada yazılı ve burada **TEKRAR EDİLMİYOR**. Ortak kapıya
konamamasının sebebi tip: `ThrowIfCannotJoin` yalnız `Unit` görür, parça iki
farklı tiptir ve ikisini birden görecek bir imza generic olmak zorunda kalırdı.

Yazma sırası `AddUnit`'inkiyle **AYNI** ve aynı sebeple: önce bütün ret sebepleri,
sonra iki yazma arka arkaya. Kopyalanan şey bir kural değil bir **SIRA**;
kuralların metni `ThrowIfCannotJoin`'de tek kez duruyor.

---

## ThrowIfCannotJoin(Unit, int, int)

**İKİ EKLEME YOLUNUN ORTAK KAPISI.** Ret sebepleri burada TEK kez yazılı;
`AddUnit` ile `AddStructure`'ın ikisinde de kopyalansaydı, "aynı kimlik ikinci kez
giremez" kuralı iki yerde yaşardı ve yalnız birine yapı sözlüğünü sormak (ilk
yazılışta en olası hata) hiçbir derleme hatası vermeden aynı `Unit`'i hem savaşçı
hem baraka yapardı.

**DOLU HÜCRE BURADA BİR ÇAĞIRAN HATASIDIR** ve bu, `UnitGrid`'in sessizliğiyle
çelişmez. `UnitGrid` hücre içeriği konusunda susar çünkü onun için doluluk bir
olgudur; burada ise değişmezin kendisidir: `PlaceUnit` üstüne yazsaydı eski parça
tahtadan silinir ama sözlükte kayıtlı kalırdı — **hücresi olmayan bir savaşçı**.

"Dolu hücreye taşınamaz" kuralıyla da karışmaz: o kural `MoveAction`'ın ve
**HAREKET** hakkında; bu satır **YERLEŞTİRME** hakkında ve sebebi oyun dengesi
değil, bu tipin bütünlüğü. Aynı olgunun bir oyun sonucu olduğu yer için
[`PlacementOutcome.RejectedCellOccupied`](PlacementOutcome.md#rejectedcelloccupied).

**Tahta dışı koordinat BURADA sorulmuyor** — sorusunun sahibi
`UnitGrid.PlaceUnit` ve o, hiçbir hücreye dokunmadan önce patlıyor. Buraya bir
`IsInsideGrid` kontrolü eklemek aynı kuralı ikinci bir yerde yazmak olurdu;
[`CellCount` ve `IsInsideGrid`'in devretme gerekçesi](#cellcount) burada aynen
geçerli.

---

## RemoveUnit(Unit)

Bir birimi ya da yapıyı savaştan çıkarır: hücresini boşaltır ve kaydını siler.
Ölüm, yıkım ve temizlik yolu burasıdır.

**TEK metot, iki sözlük** — çünkü çağıranın elinde yalnızca bir `Unit` var ve onun
hangi sözlükte olduğunu bilmek bu tipin işi. İkiye bölünseydi (`RemoveUnit` /
`RemoveStructure`) her çağıran önce "bu bir yapı mı" diye sormak zorunda kalırdı
ve o soru, cevabı zaten burada duran bir bilginin çağıranlara dağıtılması olurdu.

**Kimliğe göre silmek tahtayı taratır** — ve bu maliyet `UnitGrid.RemoveUnit`'in
üstündeki REDDEDILEN bloğunun KAZANIRDI satırında **ÖNCEDEN adı konmuş** bir
durumdur: "silmeyi tetikleyen yer birimi TANIYIP hücresini BİLMİYORSA". Temizliği
tetikleyen şey `Combatant.IsReadyForCleanup` olacak ve o bayrak hücreden hiç
haberdar değil. Yani tarama burada bir ihmal değil, o notun gerçekleşmiş hâli.

**Dönüş `bool`:** "bu savaşta yoktu" bir **ÇAĞIRAN hatası değil**. Temizlik aynı
birim için iki kez çalışabilir (bir tur döngüsü, bir de ölüm olayı) ve ikincisinin
sessizce hiçbir şey yapması doğru davranıştır.

Koordinat tahtanın kendisinden geliyor, dolayısıyla `board.RemoveUnit`'in sınır
kontrolü burada asla patlamaz — yarım kalma riski yok.

### ABONELİK BURADA BIRAKILIYOR

Bırakmama, sessiz sızıntının ders kitabı örneği ve gerekçesinin tamamı
[`UnitStateChanged`](#unitstatechanged) başlığında. Sıra önemli değil (sözlükten
silmekle abonelik arasında bir bağ yok) ama **İKİSİ de** yapılmalı: yalnız
sözlükten silmek, çağıranın elinde kalan `Combatant` üzerinden savaşta olmayan bir
birim için kimlikli olay yayılmasına izin verir.

---

## Tick(float deltaSeconds)

Zamanı bu savaştaki **HER** savaşçıya iletir. Saniye dışarıdan gelir ve burada da
okunmaz — `UnitLifecycle`'ın "zamanı kendi okumaz" sözü bu tipte de geçerli;
`Time.deltaTime`'ı çeviren yer Unity katmanıdır.

Burada tek bir kural yok: doğrulama `UnitLifecycle.Tick`'in, geçişler
`Combatant`'ın. Buranın sahiplendiği tek şey "kimler var" bilgisi — ve onu bilen
tek tip bu.

**NEDEN BURADA:** savaşçı kümesinin sahibi bu tip. Zamanı ilerletmek için o kümeyi
dolaşmak gerekir ve kümeyi dolaşabilen başka kimse yok.

### HARİTA: numaralandırıcı nerede KUTULANIR

```
  SEÇİLEN   foreach (var pair in combatants)
    Dictionary<,>.Enumerator ──► struct, YIĞINDA kalır
    ██ kare başına 0 tahsis ██

  REDDEDILEN  public IEnumerable<Unit> Units => combatants.Keys;
    KeyCollection.Enumerator ──► IEnumerator<Unit> ARDINDA
            ╔═══════════════╗
            ║   KUTULANIR   ║ ◄── ██ her Update bir tahsis ██
            ╚═══════════════╝
```

Ayrışma noktası döngünün metni değil, değişkenin **STATİK TİPİ**: aynı `foreach`,
arayüz ardında kutular, somut tip üstünde kutulamaz. Bu bir dil kuralı (struct
enumerator + arayüz dönüşümü), bir derleyici ayarı değil.

### KAPSAM: her koleksiyon içeride tutulmaz

Ölçüt: dışarı açılan şey her **KARE** mi dolaşılacak?

**KARŞI ÖRNEK** aynı dosyada,
[`RemoveReadyForCleanup`](#removereadyforcleanuplist)'ın `List<Unit> removed`
parametresi: orada bir koleksiyon dışarıyla paylaşılıyor ve sorun yok, çünkü
tamponu çağıran tutuyor ve her karede **YENİDEN KULLANIYOR**. Aynı üyede
`ICollection<Unit>` yazılmadığının gerekçesi de bu satırın kardeşi — arayüz
üstünde indeks yok, numaralandırıcı yine kutulanırdı.

### İŞ BÖLÜMÜ: iki döngü, TEK çağrı

```
  birinci döngü  ► savaşçıların saati   Combatant.Tick
  ikinci döngü   ► yapıların saati      Structure.Tick
```

Ayrı bir `TickStructures` yazılsaydı çağıran ikisini de çağırmakla yükümlü olurdu
ve birini unutan gün enkaz sonsuza dek tahtada kalırdı — hiçbir test kırmızı
olmadan, çünkü "enkaz neden hâlâ duruyor" diye kimse hata açmaz. Birinci döngü
silinirse ceset sayaçları hiç işlemez ve Downed → Dead geçişi hiç doğmaz.

### REDDEDILEN

Bu metot doğmaz; küme dışarı açılır ve döngü çağıranda yaşar:

```csharp
public IEnumerable<Unit> Units => combatants.Keys;
```

**KIRILAN:** kare başına **ÇÖP**, ve döngü her çağıranda yeniden doğar.

```
KeyCollection numaralandırıcısı IEnumerable ardında KUTULANIR
  -> her Update bir tahsis yapar
  -> döngü MonoBehaviour içine düşerse EditMode'da hiç sınanamaz
derleyici: hiçbir şey der  ·  test: hiçbiri kırmızıya dönmez
```

**KAZANIRDI:** zaman savaşçıdan savaşçıya **FARKLI** aksaydı — yavaşlatma alanı,
hızlandırma büyüsü, donmuş birim; o gün tek bir `deltaSeconds` ile yapılan toplu
ilerletme yanlış cevaptır ve çağıranın kendi çarpanıyla dolaşması gerekir.

**TEK CUMLE:** Kümeyi dışarı açmak döngüyü de dışarı taşır; taşınan döngü hem çöp
üretir hem her çağıranda yeniden yazılır.

---

## RemoveReadyForCleanup(List)

Ceset süresi dolmuş savaşçıları **VE** enkaz süresi dolmuş yapıları savaştan
çıkarır — hücrelerini boşaltır, kayıtlarını siler — ve hangilerinin çıkarıldığını
verilen listeye yazar.

[`UnitStateChanged`](#unitstatechanged) **BU METODUN YERİNE GEÇMEZ** ve geçmemeli.
İkisi farklı sorulara cevap veriyor: olay "durum değişti" der, bu metot "artık
silinebilir" der. Somut ayrım tek bir `Tick`'te görünür — ceset sayacının dolduğu
`Tick` hiçbir DURUM değiştirmez, yalnızca bir bayrak açar; o an bu metot bir şey
bulur ve olay hiç tetiklenmez. Yapılar tarafında ayrım daha da keskin:
`Structure`'ın hiçbir olayı yok, yani enkazı bulan tek yol burasıdır.

`removed` bir **ÇIKIŞ tamponudur**, üstüne eklenen bir liste değil: metot onu ÖNCE
temizler. Çağıran aynı listeyi her karede yeniden kullanır ve kare başına tahsis
olmaz. `ICollection<Unit>` değil `List<Unit>`, çünkü ikinci geçişte indeksle
dolaşmak gerekiyor — arayüzde indeks yok, numaralandırıcı ise yine kutulanırdı.

**İKİ GEÇİŞ ZORUNLU:** sözlük üzerinde dönerken silmek
`InvalidOperationException` fırlatır. Önce adaylar toplanır, sonra silinir — tek
geçişte yazmayı denemek çalışır **GÖRÜNÜR** (tek ceset varken) ve ikinci ceset
doğduğu gün patlar.

`RemoveUnit`'in dönüşü **BİLEREK** yok sayılıyor: aday listesi bir satır önce bu
sözlükten geldi, dolayısıyla `false` dönmesi mümkün değil. Kontrol etmek imkânsız
bir dal açardı.

### HARİTA: süpürmenin BAKTIĞI küme

```
  SEÇİLEN — kaynak: SAVAŞ KAYDI
    combatants ∪ structures ──► IsReadyForCleanup ──► removed
    ██ görselsiz savaşçı da bu kümededir ██

  REDDEDILEN — kaynak: GÖRSEL TABLO
    unitViews ──► battle.TryGetCombatant ──► removed
        │
        └── savaşa görselsiz eklenen savaşçı (takviye, yapay zekâ)
            bu kümede YOK ◄── ██ asla temizlenmez; hücresini
            sonsuza dek tutar ve tahta sessizce dolar ██
```

İki küme bugün eşit olduğu için fark **GÖRÜNMEZ**; ayrıştıkları gün fark bir hata
değil, bir **SESSİZLİK** olarak çıkar.

### KAPSAM: görsel tablo yasak bir kaynak değil

Ölçüt: sorulan soru **SAVAŞA** mı ait, **EKRANA** mı?

**KARŞI ÖRNEK** bu metodun kendi imzasında: `removed` tamponu tam olarak görsel
tablo için doldurulur — "hangi görseli sahneden kaldırayım" sorusunun cevabı odur
ve o soru gerçekten ekrana ait. Yani görselden savaşa doğru okumak yanlış, savaştan
görsele doğru yazmak doğru; **yön**, kaynağın kendisinden daha belirleyici.

### İŞ BÖLÜMÜ: iki döngü + bir kelepçe

```
  savaşçı döngüsü ► ceset süresi dolanlar    ─┐ AYNI tampon
  yapı döngüsü    ► enkaz süresi dolanlar    ─┘
  ThrowIfCannotJoin ► aynı kimliğin iki kez yazılmasını engeller
```

Çağıran ikisini ayırt etmiyor ve etmesine gerek de yok — elindeki iş ikisinde de
aynı. Ayrı listeler çağırana iki döngü yazdırırdı ve ikinciyi unutan gün enkaz
ekranda kalırdı. Kelepçe kalkarsa aynı kimlik tampona iki kez girer ve çağıran
aynı görseli iki kez silmeye çalışır; kelepçe burada bir konfor değil, bu metodun
**sessiz ön koşulu**.

### REDDEDILEN

Bu metot hiç doğmaz; Unity katmanı her karede kendi görsel tablosunu dolaşıp
savaşçıları **YOKLAR**:

```csharp
foreach (Unit unit in unitViews.Keys)
{
    if (battle.TryGetCombatant(unit, out Combatant c)
        && c.IsReadyForCleanup)
    {
        battle.RemoveUnit(unit);
    }
}
```

**KIRILAN:** süpürme yalnızca **GÖRSELİ olan** birimleri görür.

```
savaşa görselsiz bir savaşçı eklenir (takviye, yapay zekâ)
  -> asla temizlenmez
  -> hücresini sonsuza dek tutar ve tahta sessizce dolar
gövde de bir MonoBehaviour'a kopyalanır
derleyici: hiçbir şey der  ·  test: RemoveReadyForCleanup_* düz C# koşamaz
```

**KAZANIRDI:** temizliğin ölçütü savaş durumu değil **GÖRSEL** bir şey olsaydı —
ölüm animasyonu bitmeden silinmesin, ekran dışındaki ceset hemen gitsin; o gün
ölçüt Unity tarafında yaşar, bu tip onu bilemez ve yoklama doğru yerdedir.

**TEK CUMLE:** "Kim savaşta" sorusunun cevabı bir görsel tabloya kayarsa, görseli
olmayan her şey savaşta yokmuş gibi davranır.

---

## TryGetPosition(Unit, out int, out int)

Birimin tahtadaki hücresini verir. Konumun **TEK kaynağı** tahtadır; bu metot her
çağrıda oraya sorar.

**KONUM HİÇ ÖNBELLEĞE ALINMIYOR** — her çağrıda tahta taranıyor.

Bulunamayınca `(0,0)` **DEĞİL** `(-1,-1)`: sıfır geçerli bir hücredir ve dönüşü yok
sayan bir çağıran onu sessizce köşe sanardı. `-1` ise tahtaya verildiği anda
`UnitGrid` tarafından gürültüyle reddedilir.

### HARİTA: konumun kaç KAYNAĞI var

```
  SEÇİLEN
        ╔═══ UnitGrid ═══╗ ██ TEK GERÇEK ██
        ╚═══════▲════════╝
      ┌─────────┼──────────────┐
      │ yazar   │ sorar        │ sorar
  MoveAction   bu metot    BattleActions

  REDDEDILEN — ikinci bir positions sözlüğü
        ╔═══ UnitGrid ═══╗      ╔═══ positions ═══╗
        ╚═══════▲════════╝      ╚════════▲════════╝
      yazar     │                        │ yalnız AddUnit /
  MoveAction ───┘                        │ RemoveUnit yazar
                                         ██ HABERİ OLMAZ ██
```

`MoveAction` tahtayı **DOĞRUDAN** değiştirir; sözlük duymaz. Birim yaklaşmıştır ama
saldırı hâlâ "menzil dışı" der.

### KAPSAM: her ikinci depo bir önbellek DEĞİLDİR

Ölçüt: saklanan şeyin **BAŞKA** bir kaynağı var mı?

**KARŞI ÖRNEK** aynı dosyada, [`stateForwarders`](#stateforwarders): o da ikinci
bir sözlük ve kabul edildi, çünkü sakladığı delege örneğinin başka hiçbir kaynağı
**YOK** — yeniden üretilemez, çünkü ikinci lambda birinciyle aynı nesne değildir.
Bir önbellek var olan bir gerçeği **KOPYALAR**; `stateForwarders` hiçbir yerde
olmayan bir şeyi **TUTAR**. Ayıran şey sözlük olması değil, ayrışabilecek bir
aslının bulunup bulunmadığı.

### İŞ BÖLÜMÜ: iki yön, iki maliyet

```
  board.TryGetUnit(x, y, ...)  hücre ──► birim   O(1)
  bu metot                     birim ──► hücre   O(en·boy)
```

Aynı tabloya iki yönden bakılıyor ve tahta yalnız birini ucuz veriyor. Bu metot
silinirse tarama kaybolmaz — her çağırana bir kez daha yazılır ve o kopyaların biri
sınırı yanlış kurduğu gün birim "tahtada yok" sayılır. Ucuz yön silinirse tek
hücrelik bir soru için de tam tarama gerekir.

### REDDEDILEN

Konum ikinci bir sözlükte tutulur ve `AddUnit` ile `RemoveUnit` onu günceller:

```csharp
private readonly Dictionary<Unit, (int x, int y)> positions;
```

**KIRILAN:** konum iki sahipli olur — [üstteki sözlük gerekçesinin](#combatants)
aynısı, bu kez önbellek kılığında.

```
MoveAction tahtayı DOĞRUDAN değiştirir
  -> sözlük duymaz
  -> birim yaklaşmış olduğu hâlde saldırı "menzil dışı" der
derleyici: hiçbir şey der
test: Move_ThenAttack_UsesTheNewPosition kırmızı
```

**KAZANIRDI:** tahta **ÖLÇÜLEBİLİR** biçimde büyüseydi — 200x200'lük bir harita,
kare başına yüzlerce mesafe sorgusu ve profiler'da görünen bir tarama maliyeti; o
gün önbellek gerekir ama bedeli tahtaya yazan her yolun `Battle`'dan geçmesidir.

**TEK CUMLE:** Önbellek bir hız kararı değil, ikinci bir doğruluk kaynağı yaratma
kararıdır.
