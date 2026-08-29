# Karşılık verme ve menzil — kovalayan birim, yerinde duran taret

> **NEREDE GEÇİYOR** — *bu mekanizmanın kat ettiği kaynak dosyalar, akış sırasıyla:*
> `Assets/Game/Unity/Orders/AttackOrder.cs` → `Assets/Game/Unity/BoardAdapter.cs`
> → `Assets/Game/Battle/BattleActions.cs` → `Assets/Game/Core/Combat/AttackAction.cs`
> → geri dönüş yolu: `Assets/Game/Unity/BoardAdapter.cs` →
> `Assets/Game/Core/ApproachRules.cs` → `Assets/Game/Core/PathFinder.cs`
> → `Assets/Game/Unity/Orders/UnitOrderBook.cs`
>
> **NE ZAMAN OKU** — *hangi soruyu sorduğunda ya da hangi değişikliğe giriştiğinde:*
> "saldırıya uğrayan neden karşılık vermiyor" diye sorduğunda · bir birime
> kovalama davranışı eklemeden önce · "bunun adı Strategy mi" diye sorduğunda ·
> yakın dövüşçü ile okçu için ayrı bir kural yazmaya niyetlendiğinde.

**BURAYA KODDAN GELDİYSEN** — `ApproachRules` ve `ApproachOutcome` tiplerinin
yorumunda bu belgeye bir `DERİN ANLATIM:` işaretçisi var. Yol: `Ctrl+P` → dosya
adının ayırt edici parçasını yaz → `Ctrl+F` ile **üye adını** ara. ***Satır
numarası bilerek yazılmıyor: satır kayar, üye adı kaymaz.***

| dosya | üye | koddan işaretçi |
|---|---|---|
| `Assets/Game/Core/ApproachRules.cs` | `ApproachRules` (tip başlığı) · `Plan` | ✓ |
| `Assets/Game/Core/ApproachOutcome.cs` | `ApproachOutcome` (tip başlığı) | ✓ |
| `Assets/Game/Unity/Orders/AttackOrder.cs` | `Advance` (kesilme dalı) | ██ HENÜZ YOK ██ |
| `Assets/Game/Unity/BoardAdapter.cs` | `ReactToAttack` | ██ HENÜZ YOK ██ |

██ **"HENÜZ YOK" ne demek:** o üye burada gerçekten anlatılıyor, ama **kodun
yorumunda buraya geri getiren bir satır yok**. İşaretçiyi o iki dosyaya
yazacak olan tur, karşılık veren emri yazan turdur — bugün ikisi de başka bir
şeridin elinde ve bu belge onlara **dokunmuyor**. O günü getirecek koşul tek
cümle: karşılık veren emir tipi ağaca indiği gün.

---

## Sahne

Bir savaşçı, kendisine vuran düşmanı **görüyor** ve karşılık veriyor. Ama vuran
taraf üç hücre öteden atmışsa, kılıçlı savaşçının karşılık verecek bir şeyi yok:
menzili 1 ve düşman menzilde değil.

Sahnede olması gereken şu: kılıçlı savaşçı **yürüyor**, saldırganın yanına
varıyor ve orada vuruyor. Yanındaki okçu ise aynı emri alıyor ama **üç hücre
ötede duruyor** ve oradan atıyor. İkisi de "menziline gir" emrini almış
durumda; ikisi farklı yerde duruyor çünkü menzilleri farklı.

Bu belgenin tamamı o tek cümlenin nasıl **tek bir kurala** indiğini ve o kuralın
neden bir desen adı hak ettiğini anlatıyor.

---

## Karakterler

Hikâyeyi ilginç kılan, her karakterin **bilmedikleri**.

**`ApproachRules`** — *bilir:* tahtayı, yürüyecek kimliği, hedefin
koordinatlarını, menzil sayısını. **BİLMEZ:** kimin kime saldırdığını, saldıranın
ayakta olup olmadığını, hedefin geçerli hedef olup olmadığını, menzil sayısının
bir `AttackProfile` içinden geldiğini, hatta yürüyecek şeyin gerçekten
yürüyebildiğini.

**`ApproachOutcome`** — *bilir:* cevabın dört cinsini. **BİLMEZ:** hangisinin
döndüğünü; o kararı `ApproachRules` veriyor.

**`PathFinder`** — *bilir:* iki hücre arasında yürünecek bir yol olup olmadığını.
**BİLMEZ:** neden sorulduğunu. Yaklaşma sorusu ile oyuncunun haritaya tıklaması
onun için aynı sorudur.

**`GridDistance`** — *bilir:* iki hücre arasındaki Chebyshev uzaklığını.
**BİLMEZ:** arada engel olup olmadığını. *Uzaklık* ile *ulaşılabilirlik* iki ayrı
sorudur ve bu tip yalnız birincisini cevaplar.

**`UnitOrderBook`** — *bilir:* hangi birime hangi emrin yazıldığını.
**BİLMEZ:** emrin cinsini. `Advance` çağrısı `AttackOrder` ile `ReviveOrder`
arasında hiçbir ayrım yapmıyor ve bu, aşağıdaki Strategy tartışmasının
çekirdeğindeki ölçüm.

**`BoardAdapter`** — *bilir:* savaşı, sahneyi, görsellerin nerede olduğunu, ve
**bir kimliğin savaşçı mı yapı mı olduğunu** (`Battle.TryGetCombatant` ile
`Battle.TryGetStructure` ikilisi). Sonuncusu bu belgenin ikinci çekirdeği.
**BİLMEZ:** yaklaşma hücresinin nasıl seçildiğini.

**`Structure`** — *bilir:* kendi saldırı tanımını taşıyıp taşımadığını
(`CanAttack`). **BİLMEZ:** yürümeyi. Bir taret karşılık verebilir ama yerinden
kımıldayamaz, ve tahtada onu taşıyacak hiçbir üye yok.

---

## Kuralın evi neden Core

Bu bölüm bir üslup tercihi değil, bir **ölçümün** kaydı.

Kural ilk olarak `Assets/Game/Core/Combat/` altına, yani `GridStrategy.Combat`
assembly'sine yazılacaktı. Ölçüm bunu düşürdü:

| assembly | `references` dizisi | `PathFinder`'ı görür mü |
|---|---|---|
| `GridStrategy.Core` | boş | **evet** — `PathFinder` orada yaşıyor |
| `GridStrategy.Combat` | **boş** | hayır |
| `GridStrategy.Battle` | `Core`, `Combat` | evet |
| `GridStrategy.Unity` | `Core`, `Combat`, `Battle` | evet |

> **⌨ KODU AÇ:** `Assets/Game/Core/Combat/GridStrategy.Combat.asmdef` → `references`
> **BAK:** dizi **boş**. Klasörün adı `Core/Combat` olduğu için Combat'ın Core'u
> gördüğü sanılıyor; görmüyor. Klasör ile assembly ayrı şeyler ve ayrımın tamamı
> `02-assembly-duvari.md` içinde yazılı.
> **DÖNÜŞ:** bu dosyanın «Kuralın evi neden Core» bölümü

Duvarın **iki** yüzü var ve yalnız biri fatura kesiyor:

```
Combat  ──✗──►  Core      PathFinder'a ulaşamaz  ──► kural burada YAZILAMAZ
Core    ──✗──►  Combat    AttackProfile'a ulaşamaz ─► kural bunu İSTEMİYOR
                                                       (menzil bir int)
```

İkinci satır kararın tamamı. Kural, saldırı **tanımını** değil o tanımın taşıdığı
**tek sayıyı** istiyor. Girdi `AttackProfile` olsaydı kural Combat'a bağlanır ve
Combat da Core'u görmediği için hiçbir yerde derlenemezdi. Girdi `int` olduğu için
kuralın Combat'tan alacağı hiçbir şey kalmıyor ve Core'da yaşayabiliyor.

Aynı ölçüt ağaçta zaten uygulanmış ve **ters yönde** çalışıyor: `MovementRules`
Combat'ta duruyor çünkü `UnitState` soruyor. Bu kural durum sormuyor, **tahta**
soruyor. Tek ölçüt, iki farklı ev.

***Bunun bir bedeli var ve saklanmıyor:*** `Battle.Board` üyesi `internal`, yani
`GridStrategy.Unity` katmanı `UnitGrid`'e hiç ulaşamıyor. Emir tipi bu kuralı
doğrudan çağıramaz; arada `Battle.TryFindPath`'in birebir ikizi olan tek satırlık
bir köprü üyesi gerekiyor. O köprü bugün **HENÜZ YOK** ve o günü getirecek koşul
şu: karşılık veren emir tipi yazıldığı gün, ilk satırı o köprü olur.

---

## Mekanizma: vuruştan karşılığa

Zincirin **bugün duran** kısmı ile **henüz yazılmamış** kısmı aşağıda ayrı ayrı
işaretli. Satır numarası yok; atıf tip ve üye adıyla.

### Bugün duran zincir — bir vuruş nasıl iniyor

| # | Nerede | Ne oluyor |
|---|---|---|
| 1 | `BoardAdapter.Update` | `UnitOrderBook.Advance` her karede çağrılıyor |
| 2 | `UnitOrderBook.Advance` | defterdeki her emre `IUnitOrder.Advance` soruluyor |
| 3 | `AttackOrder.Advance` | saldıran ve hedef tahtada mı → `IUnitOrderHost.TryGetCell` |
| 4 | `AttackOrder.Advance` | saldıranın görseli yürüyor mu → `IUnitOrderHost.IsViewWalking` |
| 5 | `BoardAdapter` | `IUnitOrderHost.Strike` → hedefin hücresi `Battle.TryGetPosition` ile **taze** okunuyor |
| 6 | `BattleActions.Attack` | `TurnState.AllowsAction` kapısı → `AttackAction.Execute` |
| 7 | `AttackAction.Execute` | `AttackRules.CanAttack` · `TargetingRules.CanBeAttacked` · `AttackResolver` (menzil) · `DamageRules` · `Health` · `UnitLifecycle` |
| 8 | `BattleActions.Attack` | bir `AttackOutcome` dönüyor |
| 9 | `BoardAdapter.Strike` | `RejectedOnCooldown` DIŞINDAKİ her sonuç için `ReactToAttack` çağrılıyor |
| 10 | `BoardAdapter.ReactToAttack` | isabet ise `PlayAttackVisual` / `PlayRangedVisual`, ve Console satırı |
| 11 | `Battle.UnitStateChanged` | durum değiştiyse olay yayılıyor → `BoardAdapter.OnUnitStateChanged` → ekran |

**ONUNCU DURAKTA ZİNCİR BİTİYOR.** Vurulan taraf bu satırdan sonra hiçbir şey
yapmıyor: ne bir emir alıyor, ne bir hücre soruyor, ne bir karşılık veriyor.
Operatörün bildirdiği eksik **tam olarak bu boşluk**.

### Sıra kapısı karşılık vermeyi engelliyor mu — ölçüldü, hayır

Onuncu durakta doğacak karşılık, altıncı duraktaki `TurnState.AllowsAction`
kapısından geçmek zorunda. Kapı bu projede bugün **açık**:

```
BoardAdapter    turnMode = TurnMode.FreeForAll   (Inspector varsayılanı)
                     │
Battle           new Battle(width, height, turnMode)
                     │
TurnState.AllowsAction(team)
    Mode == FreeForAll  ──►  TurnRules.CanAct(team, team)
                                 │
                             unitTeam == Team.None ? false : unitTeam == currentTurn
                                 │
                             team == team  ──►  HER ZAMAN true
```

Yani karşılık verme **tur kapısına takılmıyor**. Bedelini ödeyen tek şey
`AttackProfile.CooldownSeconds`; saldırının sıra harcaması kaldırıldığından beri
vuruşun tek maliyeti o sayı ve o kararın hikâyesi
[09-kararlarin-cevrilmesi.md](09-kararlarin-cevrilmesi.md) madde 4'te yazılı.

***Bu, kipe bağlı bir cevaptır ve kip bir Inspector alanıdır.*** `TurnMode`
`Alternating` yapıldığı gün kapı kapanır ve karşılık veren birim sırası gelene
kadar vuramaz. O gün karşılık verme bozulmaz, **yavaşlar** — ve bu bilinçli:
kapıyı karşılık için delmek, sıranın anlamını tek bir davranış için silmek olurdu.

### Yazılmamış halka — karşılık nasıl doğacak

Aşağıdaki üç durak bugün **HENÜZ YOK**. O günü getirecek koşul: karşılık veren
emir tipi ağaca indiği gün. Yazılacak yer belli, ve bu belge onu **tarif ediyor**,
yazmıyor.

| # | Nerede | Ne olacak |
|---|---|---|
| 10a | `BoardAdapter.ReactToAttack` | isabet dalında: vurulan tarafın defterde bir emri yoksa bir **karşılık emri** yazılır |
| 10b | *seçim noktası* | vurulan kimlik savaşçı mı yapı mı — `Battle.TryGetCombatant` / `Battle.TryGetStructure` |
| 10c | karşılık emrinin `Advance` üyesi | her karede `ApproachRules.Plan` sorulur, cevabına göre yürünür ya da vurulur |

Karşılık emrinin kare döngüsü, `AttackOrder`'ınkinin **yaklaşma eklenmiş** hâli:

```
   karşılık veren VE saldıran tahtada mı ──hayır──► İPTAL
            │evet
   karşılık verenin görseli yürüyor mu ──evet──► DEVAM (henüz varmadı)
            │hayır
   ApproachRules.Plan(tahta, savunan, saldıranX, saldıranY, menzil)
            │
   ┌────────┼───────────────────┬──────────────────────┐
   │        │                   │                      │
AlreadyInRange              MoveTo             RejectedOffBoard
   │                           │               RejectedUnreachable
  VUR (host.Strike)      YÜRÜ (bildirilen           │
   │                      hücreye)                İPTAL
   │                           │
  sonucu OKU               DEVAM
```

>> **AYRIŞMA NOKTASI:** `MoveTo` dalı **DEVAM** döndürüyor, `AttackOrder`'daki
>> `RejectedOutOfRange` dalı ise **İPTAL**. Aynı olgu — hedef menzilde değil —
>> iki emirde iki zıt cevap alıyor, ve fark bir hata değil emrin **cinsi**. <<

### `AttackOrder`'ın kesilme dalı neden aynen duruyor

`AttackOrder.Advance` içinde `RejectedOutOfRange` dalının başında şu işaret
duruyor: *"OPERATÖRÜN İSTEDİĞİ KESİLME TAM OLARAK BU DAL"*. O satır **yazılı bir
karar kaydıdır** ve bu tur ona dokunmuyor.

Ayrım tek cümlede: **oyuncunun elle verdiği emir menzilden çıkınca ölür, birimin
kendi kendine aldığı karşılık emri menzile YÜRÜR.** Aynı dosyanın yorumu bunun
gerekçesini zaten taşıyor — *"oyuncunun elindeki birim, o hiç istemediği hâlde
tahtanın öteki ucuna yürürdü"*. Karşılık emrinde bu risk yok, çünkü o emri
oyuncu vermedi; birim kendisine vurulduğu için aldı.

İki davranışı tek tipe katlamak, tam olarak o yazılı kararı silmek olurdu.

---

## Neden Strategy — üçüncü koşul

Bu bölüm bir desen adının **hak edildiği anı** ölçüyle kuruyor.

### Ölçü nedir

`Docs/ogrenme/13-desen-secim-rehberi.md` Strategy'yi şöyle tanımlıyor ve bu
belge o tanımla **çelişmiyor**, onu kullanıyor:

> Ölçüsü arayüzün varlığı değildir; ölçüsü, **aynı çağıranın iki uygulama
> arasında seçim yapmasıdır**.

Ve State'ten ayıran satır aynı rehberin yanlış eşleştirme tablosunda:

> Strategy'de seçimi **dışarıdaki çağıran** yapar ve seçim ömür boyu sabit
> kalabilir. State'te seçimi **uygulamanın kendisi** yapar, bir **geçişle**.

### İki uygulama ve tek sözleşme ZATEN var — ve bu yetmiyor

Ölçüldü: `IUnitOrder` bugün **iki** uygulama taşıyor (`AttackOrder`,
`ReviveOrder`) ve `UnitOrderBook.Advance` ikisi arasında **hiçbir ayrım
yapmadan** `Advance` çağırıyor. Yani "iki uygulama, tek sözleşme, tek çağırma
noktası" tablosu bugün **tamam**.

Buna rağmen bugün Strategy yok, ve sebebi rehberin kendi tablosunda yazılı:
*"Yaratım noktası hangi tipi istediğini biliyor; seçim yok."* Bu proje için o
cümlenin somut karşılığı iki satır:

| Bugünkü emir kurma noktası | Ne tıklandı | Hangi tip isteniyor | Çağıran biliyor mu |
|---|---|---|---|
| düşman kimliğe tıklama | ayakta duran düşman | `AttackOrder` | **evet, kod yazılırken** |
| düşmüş dost kimliğe tıklama | `Downed` dost | `ReviveOrder` | **evet, kod yazılırken** |

İki noktada da hangi sınıfın `new`'leneceği **derleme zamanında** bellidir.
Çalışma zamanında seçilecek bir şey yok. Rehberin *"aşırı yükleme Strategy
sanılır"* uyarısıyla aynı sınıftan bir durum: seçim yapılıyor gibi görünüyor,
ama seçen şey programcı, program değil.

### ÜÇÜNCÜ KOŞUL — karşılık verme

Karşılık verme ile **ilk kez** şu üçlü bir arada oluyor:

**① Tek bir çağıran.** `ReactToAttack`'in isabet dalı. Bir tane, ve emrin cinsini
o yazacak.

**② Çağrı anında bilinmeyen bir olgu.** *Savunan yürüyebiliyor mu?* Bu sorunun
cevabı kod yazılırken bilinmiyor; vurulan kimliğin savaşçı mı yapı mı olduğuna
bağlı ve o cevap ancak vuruş indiği anda, `Battle.TryGetCombatant` /
`Battle.TryGetStructure` ikilisine sorularak öğreniliyor.

**③ İki uygulama, aynı sözleşme.** Kovalayan karşılık emri ile yerinde duran
karşılık emri; ikisi de `IUnitOrder`, ikisi de `UnitOrderBook`'a yazılıyor,
`Advance` ikisini de ayırt etmeden ilerletiyor.

Üçü bir aradayken cümle şu olur:

> **Birim kovalar, yapı yerinde vurur — ve hangisinin olacağını, çağrı anında
> bakılan tek bir olgu seçer.**

Üç durumun yan yana hâli:

| | emir kurma noktası | seçimi kim yapıyor | ne zaman belli oluyor | desen |
|---|---|---|---|---|
| **①** düşmana tıklama | oyuncunun tıklaması | **programcı** — kodda tek bir `new AttackOrder` | derleme zamanı | Command |
| **②** düşmüş dosta tıklama | oyuncunun tıklaması | **programcı** — kodda tek bir `new ReviveOrder` | derleme zamanı | Command |
| **③** karşılık verme | vuruşun isabet etmesi | **program** — savunan yürüyebiliyor mu | çalışma zamanı | **Strategy** |

>> **AYRIŞMA NOKTASI:** ilk iki satırda emrin cinsini **tıklamanın kendisi**
>> belirliyor; üçüncüde tıklama yok. Emri doğuran olay (bir vuruşun isabet
>> etmesi) hangi cinsin gerekeceği hakkında **hiçbir şey söylemiyor**. <<

### İtiraz: bu bir fabrika dalı değil mi

Dürüst cevap: **kısmen, ve ayrımın eşiği yazılabilir.**

Seçim tek bir `if` ile yapılıp iki farklı `new` çağrısına dallanıyorsa, o şeklin
adı Strategy değil bir **kurucu dalıdır** — rehberin *"Factory ile kurucu
sarmalayıcısı"* satırıyla aynı sınıftan. Strategy'nin ölçüsü (aynı çağıran, iki
uygulama, çalışma zamanı) sağlanıyor; ama **şekil** henüz çağıranın içindeki bir
dal.

Eşik şudur ve bugünden yazılıyor: **üçüncü bir karşılık davranışı doğduğu gün**
— örneğin "kaçan birim" ya da "menzile girmeyip yerinde bekleyen birim" — dal
çağıranın dışına çıkar ve seçimi taşıyan şey bir `if` değil, `UnitBlueprint`
üstünde duran bir alan olur. O gün şekil de adına yetişir.

Bugün yazılabilecek en dürüst cümle: **ölçü sağlandı, şekil yarısında.**

---

## Ne Strategy değil

En sık karıştırılan üç komşu. Üçü de bu projede **var** ya da **adıyla
reddedilmiş** durumda; hiçbiri uydurma bir karşı örnek değil.

### Command — emrin kendisi

`IUnitOrder`, `AttackOrder`, `ReviveOrder` ve `UnitOrderBook` **Command**'dır.
Ölçüsü: eylem çağrıldıktan sonra da **yaşıyor** — bir `Dictionary<Unit,
IUnitOrder>` içinde duruyor ve her kare bir adım ilerliyor.

Karşılık emri de bir Command olacak. **Command ile Strategy burada birbirinin
alternatifi değil; biri ötekinin taşıyıcısı.** Strategy olan şey emrin kendisi
değil, iki emir cinsi arasındaki **seçim**.

Ayıran ölçü: *nesne çağrıdan sonra yaşıyor mu?* Yaşıyorsa Command. `AttackAction`
ve `BattleActions` yaşamıyor — onların adı **akış sahibi**.

### State — tahtanın kipi

`IBoardMode` ve `BoardModeMachine` **State**'tir. İki uygulama taşıyor ama seçimi
`BoardModeMachine` bir **geçişle** yapıyor, çağıran değil.

Karşılık verme buraya **düşmüyor** ve ölçüsü tek satırda: kip **tektir** (tahta
başına bir tane), emir **çoğuldur** (birim başına bir tane). Karşılık veren üç
birim aynı karede üç ayrı emir taşıyabilir; tahtanın kipi bu sırada hiç
değişmiyor.

İkisinin sınırı `IUnitOrder` dosyasının başındaki figürde zaten çizili:
*"Girdi ne DEMEK → kip. Birim ne YAPIYOR → emir."*

### Factory — HENÜZ YOK, ne zaman doğar

Bu projede Factory **adıyla reddedilmiş** durumda ve reddin kaydı
[10-geri-alinan-kararlar.md](10-geri-alinan-kararlar.md) bölüm 5'te. Reddin
gerekçesi tek ölçüm: *fabrikanın seçeceği tip yok* — `Assets/Game` altında sıfır
`abstract`, sıfır `virtual`, ve on birim varlığının onu da tek bir `m_Script`
GUID'i taşıyor.

Karşılık verme bu ölçümü **değiştirmiyor**: iki emir sınıfı da `sealed` olacak,
aralarında kalıtım olmayacak, ve seçen şey bir tip hiyerarşisi değil bir olgu.

O günü getirecek koşul: **karşılık emri cinsi üçü aştığı gün.** İki cinste bir
`if`, üç cinste bir eşleme, dörtte bir fabrika. Bugün sayı iki.

---

## Üç oyun

Tablo **doldurulmuş** hâlde. Eşleşmeyen hücre `██ EŞLEŞMEZ ██` ile işaretli ve
yanında **neden** eşleşmediği yazılı — çünkü eşleşmemenin sebebi çoğu zaman
eşleşmenin kendisinden daha çok şey öğretiyor.

| | **Karşılık verme** (vurulan taraf cevap veriyor mu) | **Menzile yaklaşma** (cevap vermek için yer değiştiriyor mu) | **Tek kural, iki sayı** (davranış farkı bir sayıya mı iniyor) |
|---|---|---|---|
| **Slay the Spire** | **eşleşir.** Thorns ve Flame Barrier tam olarak bu: sana vuran, vurduğu için hasar alır. Cevap otomatik ve oyuncunun bir hamlesi değil | `██ EŞLEŞMEZ ██` — oyunda **konum yok**. Düşmanlar bir sırada duruyor, aralarında hücre ve mesafe kavramı yok; yaklaşılacak bir yer olmadığı için yaklaşma diye bir soru da yok | `██ EŞLEŞMEZ ██` — davranış farkı bir sayıya inmiyor, **karta** iniyor. İki kart arasındaki fark bir menzil değil bir metin |
| **Vampire Survivors** | `██ EŞLEŞMEZ ██` — düşman vurulduğu için tepki **vermiyor**, çünkü zaten sana doğru geliyordu. Vuruş onun davranışını değiştirmiyor; değiştirecek bir davranış hâli yok | **eşleşir, ve en saf hâliyle.** Oyundaki düşman davranışının **tamamı** yaklaşmadır: oyuncuya yürü, temasa gir. Bu belgedeki kuralın menzil 1 hâlinin karşılığı | **eşleşir.** Düşmanlar arasındaki gözlenebilir fark bir dal değil bir sayı: hız, can, hasar. Aynı "oyuncuya yürü" kuralı yüzlerce düşmanı sürüyor |
| **Stardew Valley** | `██ EŞLEŞMEZ ██` — canavarı tetikleyen şey vurulmak değil, oyuncunun **yakınına girmesi**. Tetik bir olay değil bir yarıçap; vurulmadan da kovalamaya başlar | **eşleşir, kısmen.** Yarasa ve slime oyuncuya doğru geliyor, Squid Kid ise durup ateş ediyor — yani "kendi menziline gir" davranışı gözle görülüyor | `██ EŞLEŞMEZ ██` — fark bir sayıya inmiyor. Yarasanın düzensiz uçuşu, slime'ın zıplaması ve Squid Kid'in durup atması aynı kuralın üç ayarı değil, üç ayrı davranış |

**Tablodan çıkan tek cümle:** bu projenin ihtiyacı Vampire Survivors'ın
sütununa en yakın duruyor — yaklaşma tek kural, fark tek sayı — ve tam da bu
yüzden `ApproachRules` içinde tür başına bir dal yok. Stardew'ın sütunu ise
reddedilen şeklin canlı örneği: orada dallanma **hak edilmiş**, çünkü davranışlar
gerçekten farklı; burada hak edilmemiş olurdu, çünkü fark yalnızca bir sayı.

---

## Tasma

Kovalayan birim sonsuza kadar kovalamasın — ama tasmanın **nereden** takıldığı
bir karardır ve bu tur onu şöyle verdi.

### Emir yalnız üç durumda düşer

| # | Düşme sebebi | Kuraldan gelen cevap | Neden bu bir son |
|---|---|---|---|
| 1 | hedef öldü | — (`AttackOutcome.RejectedInvalidTarget`) | beklemekle düzelmez |
| 2 | hedef tahtadan gitti | `ApproachOutcome.RejectedOffBoard` | beklemekle düzelmez |
| 3 | hedefe yol yok | `ApproachOutcome.RejectedUnreachable` | tahta kapalı; yürümek onu açmıyor |

Üçünün ortak ölçüsü tek cümle: **beklemekle düzelmeyen bir ret, sonsuza kadar
tekrarlanan bir rettir.** Bu cümle uydurulmadı; `AttackOrder.Advance` içinde
bugün yazılı ve karşılık emri onu devralıyor.

### Menzil dışında olmak emri DÜŞÜRMEZ

Ve sebebi tek satır: **bütün mesele o.** Menzil dışında olmak, karşılık emrinin
var olma sebebidir; onu düşme sebebi yapmak emri doğduğu karede öldürürdü.

Bu, `AttackOrder` ile karşılık emri arasındaki **tek gözlenebilir fark** ve
yukarıdaki ayrışma noktasında da işaretli.

### Adım sayısına dayalı tasma KONMADI

Konmadı, ve gerekçesi bir ölçüm:

```
BoardAdapter    [SerializeField, Min(1)] private int width  = 3;
                [SerializeField, Min(1)] private int height = 5;
                                                    ────────
                                          3 × 5  =  15 hücre
```

15 hücrelik bir tahtada en uzun yürüyüş bile birkaç adım. Buraya bir eşik
yazılsaydı — "en fazla 8 adım kovala" — o sayının hiçbir dayanağı olmazdı:
tahtanın kendisinden türetilmiş değil, gözle uydurulmuş olurdu. Ve uydurulmuş
bir eşik en kötü cinsten bir eşiktir, çünkü yanlış olduğu gün kimse onu
sorgulamaz — orada durduğu için doğru sanılır.

***Tasmanın gerçek sınırı bugün tahtanın kendisi:*** kapalı bir tahtada üçüncü
düşme sebebi zaten devreye giriyor, açık bir tahtada ise kovalama birkaç adımda
bitiyor.

**O günü getirecek koşul yazılıdır:** tahta kenarı 30'u aştığı gün, ya da
operatör "birimim düşmanı kovalarken savunmasını terk etti" diye bildirdiği gün.
İkisinden biri olduğunda eşik uydurulmaz, **ölçülür** — ve sahibi bu kural değil,
adım sayan tek yer olan emrin kendisi olur.

---

## Bugün ne var, ne yok

Bu belgenin en çok yanlış okunabilecek bölümü burası, bu yüzden ayrı duruyor.

| parça | durumu | sahibi |
|---|---|---|
| `ApproachRules` ve `ApproachOutcome` | **ağaçta, testli** | bu tur |
| `ApproachRulesTests` (on bir dava) | **ağaçta** | bu tur |
| `Battle` üstündeki köprü üyesi (`UnitGrid`'i kurala taşıyan) | ██ HENÜZ YOK ██ | emir turu |
| `IUnitOrderHost`'un hareket üyesi | ██ HENÜZ YOK ██ | emir turu |
| Karşılık veren emir tipi (kovalayan) | ██ HENÜZ YOK ██ | emir turu |
| Karşılık veren emir tipi (yerinde duran) | ██ HENÜZ YOK ██ | emir turu |
| `ReactToAttack` içindeki seçim noktası | ██ HENÜZ YOK ██ | emir turu |

Beş satırın hepsini getirecek koşul aynı: karşılık veren emir turu koştuğu gün.
`AttackOrder.cs` ve `Assets/Game/Unity/` ağacı bu tura **kapalıydı** ve
kapalılığı bir eksiklik değil bir sınır — o dosyalar aynı anda başka bir şeridin
elindeydi.

---

## Açık kalan

Bu belgenin **kapatamadığı** sınırlar. Boş bırakılmıyor, çünkü yazılmayan bir
sınır ertesi gün bir varsayıma dönüşüyor.

**① Aynı hedefe koşan iki karşılık emri birbirini engelleyebilir.**
`ApproachRules` adayları tek tek geziyor ve ilk yürünebileni seçiyor; iki birim
aynı karede aynı hücreyi seçebilir. İkincisi vardığında hücre dolu olur ve
hareket reddedilir. Bugün bunu çözen hiçbir şey yok, ve bir rezervasyon
mekanizması **bilerek** yazılmadı: ölçülmemiş bir soruna karşı ikinci bir
defter açardı. Ölçülecek gün: aynı hedefe karşılık veren birim sayısı ikiyi
aştığında.

**② En yakın aday, en kısa yol demek değil.** Kural adayları Chebyshev
uzaklığına göre sıralıyor, yürüyüş uzunluğuna göre değil. Duvarlı bir tahtada
"bana en yakın görünen" aday, etrafından dolaşmak gerektiği için aslında en uzak
olabilir. Bu bilinçli bir yaklaşıklık; onu düzeltmek her aday için tam bir yol
maliyeti hesaplamayı gerektirir ve maliyeti aşağıdaki üçüncü maddeye biner.

**③ Ölçek borcu yazılı ama ölçülmedi.** En kötü hâlde menzil karesindeki her boş
aday için bir yol araması koşuyor; menzil 3'te bu 48 aramaya kadar çıkabilir ve
her arama tahtanın tamamı kadar dizi tahsis ediyor (`PathFinder`'ın kendi yazılı
borcu). Yazılı tavan: menzil ≤ 3, tahta ≤ ~1000 hücre. **Hiçbir Profiler ölçümü
alınmadı** — tavan bir tahmindir, bir ölçüm değil.

**④ Yapının karşılık vermesi bu belgede tarif edildi, sınanmadı.** `Structure`
bir `AttackProfile` taşıyabiliyor ve `CanAttack` bunu bildiriyor; ama bir yapının
karşılık verdiği tek bir test yok ve olamaz — testin sınayacağı emir tipi henüz
yazılmadı.

**⑤ "Ölçü sağlandı, şekil yarısında" hükmü elle tutuluyor.** Hiçbir kapı, emir
seçiminin bir `if` mi yoksa bir veri alanı mı olduğunu ölçmüyor. O satır bir
sonraki turda yeniden okunmazsa sessizce bayatlar.

**⑥ Bu belge `Docs/ogrenme/13-desen-secim-rehberi.md` ile çelişmiyor ama onu
ESKİTİYOR.** Rehberin Strategy bölümü *"bu desen projede yok"* diyor ve
`IUnitOrder` satırının gerekçesi olarak *"yaratım noktası hangi tipi istediğini
biliyor; seçim yok"* yazıyor. Karşılık emri indiği gün o gerekçe **tam olarak**
düşer. Rehberin kendi tetikleyici koşulu ise başka bir eksene bakıyor — otomatik
**hedef seçimi** — yani rehber bu kapıyı öngörmemiş, yanlış yazmamış. Rehber bu
tur **güncellenmedi** ve güncellenmemesi bir karar: o satır kod indikten sonra
düzeltilir, öncesinde değil.

---

## İlgili

- Emrin kendisi ve kip ile sınırı: [09-kararlarin-cevrilmesi.md](09-kararlarin-cevrilmesi.md)
- Geri almanın yedi alanı ve reddedilen `Factory`: [10-geri-alinan-kararlar.md](10-geri-alinan-kararlar.md)
- Kuralın Core'da yaşamasını zorlayan duvar: [02-assembly-duvari.md](02-assembly-duvari.md)
- Ret sırası ve sıra kapısının yeri: [04-karar-sirasi.md](04-karar-sirasi.md)
- Sıfırıncı enum değerinin neden ret olduğu: [06-sonuc-enumlari.md](06-sonuc-enumlari.md)
- Tahtanın tek yazarlı olması, ve `Battle.Board`'un neden `internal` olduğu: [03-tahta-sahipligi.md](03-tahta-sahipligi.md)
- Desen adlarının ayırıcı testi: [../../ogrenme/13-desen-secim-rehberi.md](../../ogrenme/13-desen-secim-rehberi.md)
- Üye başına gerekçeler: [../kod/README.md](../kod/README.md)
- Bu ağacın yönlendirmesi: [../README.md](../README.md)
