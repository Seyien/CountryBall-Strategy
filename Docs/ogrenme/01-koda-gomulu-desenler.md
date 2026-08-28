# Kodda zaten duran desenler — adlarıyla

***Bu dosya kod **uydurmuyor**. Aşağıdaki dokuz desenin her biri
`Assets/Game/` altında açılıp sayılmış satırlara dayanıyor.***

Bir desenin adını bilmek onu kullanmak değildir; ama kullandığın bir şeyin adını
bilmemek, onu ikinci kez bilinçli olarak seçememek demektir. Bu dosyanın tek
işi o boşluğu kapatmak: projede **gerçekten** duran kararlara adlarını vermek.

***SINIR — bu dosya "UYGULANANLAR" belgesidir.*** Burada yalnız kodda duran
desenler var; on iki desenin **hepsini**, uygulanmayanları ve her birinin
tetikleyici koşulunu arıyorsan yer burası değil,
[13-desen-secim-rehberi.md](13-desen-secim-rehberi.md). Aşağıdaki *"İncelenip
elenen desen adayları"* tablosu bir özettir; o tablonun uzun hâli, motor
karşılığı ve kod taslağı `13`'te.

## Her bölümün beş alanı

| Alan | Ne yazar |
|---|---|
| **HANGİ BASINÇ** | Deseni doğuran somut sıkıntı. Baskı yazılmadan ad yazılmaz |
| **KODDA NEREDE** | `dosya:satır` — doğrulanmış |
| **SOLID KARŞILIĞI** | Hangi harf, ve ihlal edilseydi hangi DOSYA değişmek zorunda kalırdı |
| **REDDEDİLEN** | Gerçek bir rakip seçenek varsa. Her `sealed`'a, her get-only property'ye uygulanmaz — onlar genel kural, karar değil |
| **ÜÇ OYUN** | Slay the Spire · Vampire Survivors · Stardew Valley. Eşleşmeyen satır işaretli: düz yazıda ***EŞLEŞMEZ***, figür ve tablolarda `██ EŞLEŞMEZ ██` |

**SOLID nedir** (beş tasarım ilkesinin baş harfleri, Robert C. Martin):
**S**ingle responsibility (tek sorumluluk — bir tipin değişmesi için tek bir
sebep olmalı), **O**pen/closed (yeni davranış eklenirken var olan kod
değişmemeli), **L**iskov substitution (alt tip, üst tipin yerine sorunsuz
geçebilmeli), **I**nterface segregation (kimse kullanmadığı üyelere bağımlı
kalmamalı), **D**ependency inversion (üst katman alt katmanın somutuna değil,
soyutlamaya bağlı olmalı). Bu beş harf bu projede bir **arayüz ağacıyla** değil,
bir **assembly duvarıyla** uygulanıyor — sebebi 4. desende yazılı.

---

## 1. Saf kural sınıfı (stateless policy class)

Durum tutmayan, yalnızca soru cevaplayan `static` tip. "Politika" burada
"kural metni" demek: hangi girdinin geçerli olduğuna karar veren cümle.

**HANGİ BASINÇ** — "Düşmüş bir birim saldırabilir mi" sorusunun sahibi yoktu ve
cevap akışın içinde, bir `if` olarak doğuyordu. Aynı cümle iki akışta (birime
saldırı, yapıya saldırı) iki kez yazılmak zorunda kalıyordu; iki kopya ayrı
hızda eskiyecekti. Kural metnini tek bir eve koyan tip bu baskıdan doğdu —
`AttackRules.cs:12-15` bu boşluğu kendi sözleriyle anlatıyor: "kim VURUR
sorusunu kimse cevaplamıyordu ve düşmüş bir birim hâlâ vurabiliyordu."

**KODDA NEREDE** — ***on bir*** tip, hepsi `static class` (2026-08-25'te
ölçüldü; bu sayı dokuzdu, üretim katmanı gelince iki tip daha bu şekle girdi):

```
Assets/Game/Core/Combat/AttackRules.cs:21        CanAttack        :38
Assets/Game/Core/Combat/MovementRules.cs:22      CanMove          :47
Assets/Game/Core/Combat/TargetingRules.cs:31     CanBeAttacked    :44 :65 :89 :105
                                                 CanBeRevived     :132 :149
                                                 IsHostilePairing :170
Assets/Game/Core/Combat/AttackResolver.cs:25     IsWithinRange    :38
Assets/Game/Core/Combat/DamageRules.cs:24        ResolveRemaining :33
Assets/Game/Core/Combat/HealingRules.cs:27       ResolveRestored  :37
Assets/Game/Core/Combat/ReviveRules.cs:32        CanRevive        :48
Assets/Game/Core/Combat/ProductionRules.cs:30    CanProduce       :49   ← YENİ
Assets/Game/Core/GridDistance.cs:25              Between          :36
Assets/Game/Battle/TurnRules.cs:28               CanAct           :59 :91
Assets/Game/Battle/VictoryRules.cs:30            Winner           :53
```

Ortak şekil: alan yok, kurucu yok, girdi enum ya da `int`, çıktı `bool` ya da
`int`. `TurnRules.cs:41`'deki `MaxActionsPerTurn` sabiti bu şeklin tek istisnası
gibi görünür — ama `const`'tur, yani durum değil metin.

***ŞEKLİN SINIRI DA ÖLÇÜLDÜ*** — `Assets/Game/` altında toplam **14**
`static class` var; yukarıdaki on bire girmeyen üçü `BattleActions`,
`AttackAction` ve `MoveAction`. Onlar kural değil **eylem**: bir `Battle`
ya da `Combatant` alıp onu **değiştiriyorlar**. Ayrımın ölçüsü tek satır —
kural tipleri hiçbir nesneyi değiştirmez, yalnız cevap verir. `ProductionRules`
ve `VictoryRules` bu sınavı geçtiği için listeye girdi: ikisinde de sıfır alan,
sıfır kurucu.

**SOLID KARŞILIĞI** — **S** (tek sorumluluk). Ölçüsü şu: `TargetingRules`
değişmek için tek bir sebep taşır — "kime uygulanır" kuralının değişmesi.
İhlal edilseydi, yani kural `Combatant`'ın bir property'si olsaydı, kuralı tek
bir satır için sınamak şu üç kurucunun üçünü birden çalıştırmayı gerektirirdi —
üç dosya, tek kural için:

```
Assets/Game/Core/Combat/Health.cs:31   public Health(int max)
UnitLifecycle.cs:46                    public UnitLifecycle(
AttackProfile.cs:49                    public AttackProfile(int damage, int range)
```

`TargetingRules.cs:38-41` bu faturayı zaten yazmış durumda.

Ayrıca **D** (bağımlılık tersine çevirme) — ama arayüzle değil, **veri
yönüyle**: kural tipleri kendilerini çağıran akışları tanımaz. `AttackRules`,
`AttackAction`'ın var olduğunu bilmez; ok tek yönlü.

**REDDEDİLEN** — `MovementRules.CanMove` yerine, `TargetingRules`'tan türetme:

```csharp
public static bool CanMove(UnitState state)
{
    return TargetingRules.CanBeAttacked(state);   // bugün aynı cevap
}
```

**KIRILAN:** İki kural bugün kesişiyor, bağlı değil. "Düşmüş birime vurulur"
kararı değiştiği gün düşmüş birim **yürümeye başlar** ve hiçbir test kırmızıya
dönmez. Kırılan şey bir satır değil, iki kuralın **ayrı ayrı değişebilme
hakkı**. Gerekçe kodda yazılı: `MovementRules.cs:42-46`.

**KAZANIRDI:** Hareket kuralı gerçekten saldırı kuralının **türevi** olarak
tanımlansaydı — örneğin "hedeflenebilen her şey yürüyebilir" diye bir tasarım
kararı yazılı olsaydı. O gün türetme doğru olur, kopya yanlış olurdu.

**ÜÇ OYUN** — Slay the Spire: bir kartın oynanamamasının sebebi ayrı ayrı
söylenir — enerji yetmiyor, oynanabilir hedef yok, kart bu tur oynanamaz ·
Vampire Survivors: hasar sayısı ile silahın menzili birbirinden bağımsız
değişir, seviye atlamak birini artırırken ötekine dokunmaz · Stardew Valley:
bir tohumun ekilip ekilemeyeceği toprağa, mevsime ve alete ayrı ayrı sorulur;
üç ret farklı mesaj verir.

---

## 2. Akış sahibi (transaction script) — Command DEĞİL

Birden fazla kuralı **belirli bir sırayla** soran, kuralların hiçbirini kendisi
yazmayan tip. "İşlem betiği" (transaction script) adı Martin Fowler'ın: bir
kullanıcı eylemini baştan sona yürüten tek yordam.

**HANGİ BASINÇ** — Parçalar hazırdı ama kimse "saldır" demiyordu.
`AttackResolver` menzili ölçüyor, `TargetingRules` uygunluğu söylüyor,
`Combatant` hasarı uyguluyordu — üçü birbirini **tanımıyordu ve tanımamalıydı**.
Onları bir sıraya dizecek biri gerekiyordu. `AttackAction.cs:16-21` bu cümleyi
birebir taşıyor.

İkinci baskı daha keskin: `AttackAction` mesafeyi **dışarıdan** alıyordu ve o
mesafeyi üretecek kimse yoktu. Konumu `Battle` biliyor, ölçüyü `GridDistance`
yapıyor, çözümü `AttackAction` biliyor — üçü de birbirini tanımıyor. İkinci bir
akış sahibi (`BattleActions`) tam olarak bu yüzden doğdu
(`BattleActions.cs:28-32`).

**KODDA NEREDE**

```
Assets/Game/Core/MoveAction.cs:42             Execute :60 (int menzil) · :164 (MoveProfile)
Assets/Game/Core/Combat/AttackAction.cs:36    Execute :52 (Combatant) · :127 (Structure)
Assets/Game/Battle/BattleActions.cs:50        Attack :58 · Move :158 · Revive :228
                                              PlaceStructure :320
```

Sıranın kendisi bir karar ve kodda yazılı: `MoveAction.cs:106-113` üç ret
sebebinin sırasını (SINIR ► MENZİL ► DOLULUK) ve neden bu sırada olduklarını
anlatıyor. `BattleActions.cs:20-24` aynı iskeleti dört eylem için tanımlıyor:
önce çağıran hataları (istisna), sonra kurallar (sonuç değeri), en sonda tek
yazma ve sıra devri.

**SOLID KARŞILIĞI** — **S** ve **D** birlikte. Akış sahibi "hangi soruyu hangi
sırayla sorarım" sorumluluğunu taşır, kural metnini taşımaz.

İhlalin somut faturası kodda ölçülmüş: kural `MoveAction`'dan `UnitGrid`'in
içine taşınsaydı, tahta **kendiyle çelişirdi** — aynı tip "dolu hücreye
yazılabilir mi" sorusuna iki zıt cevap verirdi (`PlaceUnit` şikâyetsiz yazar,
`TryMoveUnit` reddederdi). Değişmek zorunda kalan dosya
`Assets/Game/Core/UnitGrid.cs` olurdu ve `GridDistance` ile `MoveOutcome`'ı
tanımak zorunda kalır, Chebyshev kararı içinde donardı. Gerekçe
`MoveAction.cs:44-49`'da yazılı.

**REDDEDİLEN** — `AttackAction.Execute`'un iki aşırı yüklemesi yerine, ortak
bir arayüz arkasında tek gövde:

```csharp
public interface IAttackTarget
{
    bool CanBeAttacked(Team attackerTeam);
    bool TakeDamage(int amount);
}

public static AttackOutcome Execute(Combatant attacker, IAttackTarget target, int distance)
```

**KIRILAN:** Hedef uygunluğu kuralı `TargetingRules`'tan **hedefin içine**
taşınır; `Combatant` ile `Structure` ikisi de o kuralı tanımak zorunda kalır.
Daha kötüsü: arayüzün `bool`'u `Downed` ile `Destroyed`'ı aynı cevabın arkasına
düşürür ve `AttackOutcome.HitAndDowned` ile `HitAndDestroyed` ayrımı
(`AttackOutcome.cs:52` ve `:61`) kaybolur. Soyutlamanın bugün sildiği tek şey
**iki metot**. Gerekçe `AttackAction.cs:121-126`'da yazılı.

**KAZANIRDI:** Üçüncü, dördüncü, beşinci hedef tipi geldiği gün — ve o tiplerin
"vuruldu" cevabı gerçekten **aynı** kelimeyle adlandırılabildiği gün. İki tipte
soyutlama iki metot siler; beş tipte beş metot siler ve o gün hesap tersine
döner.

*****BU BİR COMMAND DEĞİL***** — Command deseni bir eylemi **nesneye** bağlar;
nesnenin var olma sebebi eylemi saklamak, kuyruğa almak, geri almak ya da
yeniden oynatmaktır. Buradaki üç tip de `static class`
(`MoveAction.cs:42`, `AttackAction.cs:36`, `BattleActions.cs:50`) ve hiçbirinin
tek bir alanı yok. Saklanabilecek bir nesne olmadığı için geri alma da tekrar
oynatma da yoktur. Command'ın **ne zaman** doğru olacağı
[02-sonraki-asamalar.md](02-sonraki-asamalar.md) kapsamında değil; bu projede
onu isteyecek baskı (hamle geçmişi, tekrar izleme, geri alma düğmesi) henüz
doğmadı — `HENÜZ YOK → bir hamle geçmişi özelliği`.

**ÜÇ OYUN** — Slay the Spire: bir kart oynandığında enerji düşer, hedef seçilir,
etki uygulanır ve tur bilgisi güncellenir — dördü tek bir sırayla olur ·
Vampire Survivors: ***EŞLEŞMEZ*** orada oyuncu tek tek eylem yürütmez, silahlar
kendi zamanlayıcılarıyla ateşler; sıralı bir "eylem akışı" görünmez ·
Stardew Valley: bir aleti kullanmak enerji harcar, toprağı değiştirir ve saati
ilerletir; üçü tek bir tıklamanın ardından belirli bir sırayla olur.

---

## 3. Durum makinesi (state machine) — enum tabanlı

Sonlu sayıda hâl, hâller arasında **yasak geçişler**, ve geçiş kararını veren
tek tip. "Enum tabanlı" ayrımı önemli: hâller ayrı sınıflar değil, tek bir
`enum`'un değerleri.

**HANGİ BASINÇ** — Durumu tutan şey bir `bool`'du ve üçüncü hâli ifade
edemiyordu. "Ölü ama 10 saniye içinde diriltilebilir" ne canlıdır ne kalıcı ölü
— ve hasar almaya **devam etmesi** gerekir. `bool`'la yazıldığında bu kural
sessizce kayboluyordu. Gerekçe `UnitState.cs:20-23`'te yazılı; iki `bool`'un
(`isAlive`, `isDowned`) neden reddedildiği `UnitState.cs:12-15`'te: dört
kombinasyon üretir ve dördüncüsü — ikisi birden doğru — anlamsızdır ama
**yazılabilir**.

İkinci baskı işaretçi tarafında: bir tıklama ile bir sürükleme **başlangıçta
aynıdır**; ayrımı ancak bir hâl geçmişi üretir. Tek bir `IsDragging` bayrağı
`Idle` ile `ClickReleased`'i aynı değere düşürür ve bir jestin **bittiğini**
söyleyemez (`PointerGesture.cs:89-93`).

**KODDA NEREDE** — üç makine, üç ayrı hâl kümesi:

```
Assets/Game/Core/Combat/UnitLifecycle.cs:31       Alive → Downed → Dead
   hâl kümesi   Assets/Game/Core/Combat/UnitState.cs:28    (Alive :31 · Downed :37 · Dead :40)
   tek giriş    SetState :90        yasak geçiş kapısı :96
   geçiş yolu   OnHealthDepleted :126 (yalnız Alive'dan) · TryRevive :147 (yalnız Downed'dan)
                Tick :169 (Downed → Dead :192)

Assets/Game/Core/Combat/StructureLifecycle.cs:42  Standing → Destroyed
   hâl kümesi   Assets/Game/Core/Combat/StructureState.cs:36  (Destroyed :47 · Standing :55)
   geçiş yolu   OnHealthDepleted :103 (yalnız Standing'ten) · Tick :130

Assets/Game/Core/PointerGesture.cs:96             Idle → Pressed → Dragging → *Released
   hâl kümesi   Assets/Game/Core/PointerGesture.cs:27   (Idle :30 … DragReleased :42)
   geçiş yolu   Press :188 · MoveTo :212 · Release :248 · Reset :278
```

Yasak geçişlerin somut hâli: `UnitLifecycle.cs:137`'teki `if (State !=
UnitState.Alive) return;` satırı, düşmüş bir birime tekrar vurmanın onu **anında
öldürmesini** engeller. `StructureLifecycle.cs:105`'teki ikizi ise ikinci
vuruşun enkaz sayacını **sıfırlamasını** engeller — sıfırlasaydı yıkık binaya
düşen alan hasarı enkazı sonsuza dek ekranda tutardı (`:107-111`).

**SOLID KARŞILIĞI** — **S** ve **O** birlikte.
**S**: `UnitLifecycle` yalnızca "hangi durumdayım ve ne kadar kaldı" sorusunu
cevaplar; canın kaç olduğunu bilmez (`UnitLifecycle.cs:25-27`).
**O**: `UnitState`'e dördüncü bir değer (`Stunned`) eklendiğinde ne olacağı
**bugünden** yazılmış durumda — kurallar **beyaz liste** biçiminde
(`== Alive`), kara liste (`!= Downed && != Dead`) biçiminde değil. İki biçim
bugün aynı cevabı verir; fark dördüncü değer eklendiği gün doğar. Kara liste
olsaydı değişmek zorunda kalacak dosyalar `MovementRules.cs`, `AttackRules.cs`
ve `TargetingRules.cs` olurdu — ve hiçbiri derleme hatası vermezdi. Gerekçe
`MovementRules.cs:36-39`'da ölçülmüş.

**REDDEDİLEN** — `UnitState` ve `StructureState` yerine tek ortak enum:

```csharp
public enum LifeState { Alive, Downed, Dead, Standing, Destroyed }
```

**KIRILAN:** Her `switch`'te asla çalışmayan bir `Downed` dalı doğar; bir
barakanın "düşmüş" hâli tipte **var olur** ve yazılabilir hâle gelir.
`TargetingRules.cs:25-27` bu bedeli adıyla yazmış: "Tek enum alternatifinin
bedeli her switch'te asla çalışmayan bir `Downed` dalıydı."

**KAZANIRDI:** Birim ile yapının yaşam döngüsü gerçekten **aynı** kurala
yaklaşsaydı — örneğin binalar da kurtarma penceresi kazansaydı ve `TryRevive`
ikisinde de anlamlı olsaydı. O gün iki enum bir kopya olurdu, bir ayrım değil.

*****BU BİR GoF STATE DEĞİL***** — GoF State deseninde her hâl **kendi
sınıfıdır** ve geçiş, nesnenin içindeki hâl referansının değişmesidir. Burada
hâl bir `enum` değeri, geçiş bir `switch`/`if`. Ayrım ölçülebilir: üretim
kodunda `interface`, `abstract`, `virtual` ve `override` kelimelerinin **hiçbiri
geçmiyor**. Enum tabanlı makine üç-beş hâlde daha okunur; hâl başına onlarca
satır davranış biriktiğinde GoF State kazanmaya başlar.

**ÜÇ OYUN** — Slay the Spire: bir düşman "niyet"ini gösterir, sonra uygular,
sonra sırayı devreder — üç ayrı hâl ve aralarında yalnız tek yön var ·
Vampire Survivors: bir düşman doğar, kovalar, ölür ve yere hazine bırakır;
ölmüş bir düşman kovalamaya geri dönemez · Stardew Valley: bir ekin tohum,
fide, hasat edilebilir hâllerini sırayla geçer ve sulanmadığı gün ilerlemez.

---

## 4. Katman sınırı çevirmeni (boundary adapter)

Bir tarafın dilini (piksel, kare, sahne nesnesi) öteki tarafın diline (hücre,
tur, kural) çeviren tek tip. Buradaki "adapter" sözcüğü **katman sınırı**
anlamında; GoF'un arayüz dönüştürücüsü anlamında değil (ayrım aşağıda).

**HANGİ BASINÇ** — Savaş kurallarının Unity olmadan sınanabilmesi isteniyordu.
Bu bir üslup tercihi değil, ölçülmüş bir kazanç: `Assets/Tests/EditMode/` altında
26 test dosyası var ve bunların çoğu sahne kurmadan koşuyor. Ama girdi (`Input`),
kamera (`Camera`), zaman (`Time.deltaTime`) ve nesne doğurma (`Instantiate`)
motorsuz yaşayamaz. İki dünyayı ayıran bir duvar ve o duvarı geçen tek bir kapı
gerekiyordu.

**KODDA NEREDE** — duvarın kendisi dört `.asmdef` dosyasında:

```
Assets/Game/Core/GridStrategy.Core.asmdef             references: []   noEngineReferences: true
Assets/Game/Core/Combat/GridStrategy.Combat.asmdef    references: []   noEngineReferences: true
Assets/Game/Battle/GridStrategy.Battle.asmdef         references: [Core, Combat]   noEngineReferences: true
Assets/Game/Unity/GridStrategy.Unity.asmdef           references: [Core, Combat, Battle]   noEngineReferences: false
```

***KAPI ARTIK TEK DOSYA DEĞİL.*** Eskiden bu satır "Kapı tek dosya:
`BoardAdapter.cs`" diyordu ve o gün doğruydu. 2026-08-25'te ölçüldü:
`GridStrategy.Unity` altında **9** dosya var, **8**'i motoru gerçekten
**çağırıyor**. Duvarın yeri değişmedi — kapının **genişliği** değişti:

```
Time.deltaTime      okunur   BoardAdapter.cs:931       → battle.Tick(...)
                             ProductionDirector.cs:150 → production.Tick(...)
Instantiate         çağrılır BoardAdapter.cs:1078
                             ProductionPanelView.cs:142 . StructurePaletteView.cs:119
Destroy             çağrılır BoardAdapter.cs:1454 . :1473
                             ProductionPanelView.cs:179
GetComponent        çağrılır BoardAdapter.cs:298 (Grid) . UnitView.cs:125 (SpriteRenderer)
Input               okunur   BoardAdapter.cs Update/UpdatePlacement gövdeleri
```

***DOKUZUNCU DOSYA BİR AYRIMI GÖRÜNÜR KILIYOR*** — `IPlacementBoard.cs` motoru
**çağırmıyor**, yalnız bir motor tipinin ADINI taşıyor: `TryScreenPointToCell`
bir `Vector2` alıyor. Bir tipi adlandırmak ile onu çağırmak aynı şey değildir;
arayüzün kendi künyesi de bunu yazıyor (*"Unity: gerekmez ama VAR"*). Dosyanın
çekirdeğe değil Unity katmanına konmasının tek sebebi bu ad.

***HÜKÜM AYAKTA, ve ölçüsü şu:*** bu dokuz dosyanın dokuzu da aynı
assembly'de ve o assembly'nin **dışında** motora değen **sıfır** dosya var.
Duvarın altındaki üç assembly'de `noEngineReferences: true` — yani "kapı tek
dosya" iddiası "kapı tek **assembly**"ye dönüştü ve asıl korunan şey buydu.

Duvarın altında ne olduğu da ölçülü: `UnitLifecycle.cs:185`'daki `Tick` saniyeyi
**dışarıdan** alır ve içeride `Time.deltaTime` yoktur. Sebebi ölçülmüş ve
`UnitLifecycle.cs:163-166`'de yazılı: EditMode'da `Time.deltaTime` sıfır **değil**,
0,017675 döner — zamanı içeriden okuyan tasarım testte patlamaz, sessizce
anlamsız bir sayıyla yürür.

**SOLID KARŞILIĞI** — **D** (bağımlılık tersine çevirme), ama arayüzle değil
**assembly yönüyle**. Ölçüsü şu: `GridStrategy.Unity`, `GridStrategy.Battle`'ı
tanır; `GridStrategy.Battle` `GridStrategy.Unity`'yi **tanıyamaz** — o satır
derlenmez bile.

İhlal edilseydi hangi dosya değişirdi: `Assets/Game/Core/GridStrategy.Core.asmdef`
ve `Assets/Game/Core/Combat/GridStrategy.Combat.asmdef`'in `noEngineReferences`
satırı `false`'a dönerdi. O gün `AttackResolver.cs:38`'i sınamak için bir sahne
kurmak gerekirdi ve `Assets/Tests/EditMode/Combat/` altındaki testlerin tamamı
PlayMode'a taşınırdı. Duvarı kuran şeyin `noEngineReferences` **değil**,
asmdef'in boş `references` listesi olduğu `AttackResolver.cs:31-35`'te yazılı.

**REDDEDİLEN** — `BoardAdapter.cs:48`'teki ad takması (alias) yerine, dosya
başına taşınmış hâli:

```csharp
using UnityEngine;
using Battle = global::GridStrategy.Battle.Battle;   // dosyanın BAŞINDA

namespace GridStrategy.Unity
{
    public sealed class BoardAdapter : MonoBehaviour { ... }
}
```

**KIRILAN:** Derleyicinin ad arama sırası. Ad alanı **gövdesinde** duran takma
ad SEVİYE 1b'de yakalanır ve arama SEVİYE 2'ye hiç çıkmaz; dosyanın başına
taşındığında SEVİYE 3'e düşer ve arama önce `GridStrategy` üyelerini bulur —
`Battle` bir **ad alanı** olarak çözülür ve CS0118 geri gelir. Metin harfi
harfine aynı, sonuç zıt. Harita `BoardAdapter.cs:13-47`'te çizili.

**KAZANIRDI:** `GridStrategy.Battle` ad alanı ile `Battle` sınıfının adları
çakışmasaydı — yani sınıfın adı `BattleState` olsaydı. O gün takma ada hiç gerek
kalmaz, dosya başındaki sıradan bir `using` yeterdi.

*****BU BİR GoF ADAPTER DEĞİL***** — GoF Adapter bir tipin arayüzünü **başka
bir arayüze** çevirir; ölçüsü, çevrilen hedef arayüzün var olmasıdır.

***BU İDDİANIN DAYANAĞI 2026-08-25'te DEĞİŞTİ, HÜKMÜ DEĞİŞMEDİ.*** Eskiden
ölçü şuydu: `BoardAdapter` hiçbir arayüz uygulamıyor. Bugün **uyguluyor** —
`BoardAdapter.cs:111` `MonoBehaviour`'ın yanına `IPlacementBoard`'u da yazıyor
ve sekiz üyesinin sekizi de o sözleşmenin karşılığı. Yani "arayüz yok"
dayanağı düştü.

Hüküm yine de ayakta, çünkü GoF Adapter'ın asıl ölçüsü arayüzün varlığı değil
***ADAPTE EDİLENİN*** varlığıdır: Adapter, sarmaladığı **başka bir nesneye**
çağrıyı çevirerek devreder. `BoardAdapter`'ın böyle bir sarmaladığı yok —
tahtayı kendisi tutuyor (`battle` alanı, `BoardAdapter.cs:201`). Çevirdiği şey
bir arayüz değil, iki **dünya**: ekran noktası ile hücre koordinatı. Doğru ad
hâlâ "katman sınırı çevirmeni".

***YENİ ÖLÇÜ*** — `IPlacementBoard` bir Adapter hedefi değil bir **dikiş**:
uygulayanı tek (`BoardAdapter`), çağıranı tek (`ProductionDirector`), ve var
olma sebebi `ProductionDirector`'ın tahtanın somut tipini hiç görmemesi. Bir
GoF Adapter'da hedef arayüzü çağıran taraf ile adapte edilen taraf **ayrı**
tiplerdir; burada adapte edilen taraf **yok**. Dosya bunu kendisi de itiraf ediyor: künyesi
`BoardAdapter.cs:68` "KARMA — ÇEVİRMEN + VARLIK" diyor ve `:83-88`'de bir **koku
notu** taşıyor — eşiğin aşıldığı yazılı, silinmemiş.

**ÜÇ OYUN** — Slay the Spire: kart üstündeki sayı ile ekrandaki animasyon ayrı
şeyler; sayı değişmeden animasyon oynamaz · Vampire Survivors: ekrandaki yüzlerce
düşman görselinin arkasında konum ve can ayrı tutulur, görsel yalnız takip eder ·
Stardew Valley: bir ekinin büyüme günü ile ekrandaki sprite'ı ayrı ilerler;
oyun kapalıyken de gün geçer.

---

## 5. Bileşim (composition over inheritance)

Bir tipin yeteneklerini **kalıtımla devralmak** yerine, parçaları alan olarak
**tutarak** kazanması.

**HANGİ BASINÇ** — `Health` canın bittiğini bilir ama `UnitLifecycle`'ı tanımaz;
`UnitLifecycle` düşmeyi bilir ama canı tanımaz. İkisini birden tanıyan bir yer
gerekiyordu ve o yer ikisinin de **dışında** olmalıydı — aksi hâlde `Health`
sahibinin ne olduğunu bilmek zorunda kalırdı ve aynı sınıf hem askerin hem
barakanın canını tutamazdı (`Health.cs:15-19`).

**KODDA NEREDE**

```
Assets/Game/Core/Combat/Combatant.cs:35
    health     :44   (Health)
    lifecycle  :45   (UnitLifecycle)
    AttackProfile :126
    Team       :141
Assets/Game/Core/Combat/Structure.cs:37
    health     :39   (Health)
    lifecycle  :40   (StructureLifecycle)
    Team       :75 · AttackProfile :78 (isteğe bağlı — ctor :51)
```

`Combatant.cs:152`'deki `public UnitState State => lifecycle.State;` satırı
bileşimin görünen yüzü: dışarıya tek bir tip görünüyor, cevabı parça veriyor.

***ÖLÇÜLDÜ (2026-08-25)*** — `Assets/Game/` altındaki **46** üretim dosyasının
hiçbirinde `abstract`, `virtual` ve `override` kelimeleri **geçmiyor** — üçü de
sıfır, ve bu sayı dosya sayısı 33'ten 46'ya çıkarken değişmedi. Yani bu projede
kalıtım bir **hiyerarşi kurma aracı** olarak hâlâ hiç kullanılmıyor.

***BİR DAYANAK DÜŞTÜ:*** eskiden bu paragraf "`interface` de geçmiyor" diyordu.
Bugün geçiyor — tam **bir** tanım var: `IPlacementBoard.cs:39`. Ama bu, kalıtım
iddiasını çürütmez çünkü arayüz uygulamak **üye devralmak** değildir; devralınan
bir gövde yok, yalnız imza var. `abstract`/`virtual`/`override` sıfırlığı bu
ayrımın tam ölçüsüdür ve o sıfır duruyor.

***İKİNCİ DAYANAK BÜYÜDÜ:*** "Motor tarafında tek kalıtım var" artık yanlış.
Taban listesi taşıyan tip **sekiz** ve sekizi de `Assets/Game/Unity/` altında:

```
MonoBehaviour'dan (6)   BoardAdapter.cs:111 . PaletteEntryView.cs:39
                        ProductionDirector.cs:35 . ProductionPanelView.cs:36
                        StructurePaletteView.cs:30 . UnitView.cs:43
ScriptableObject'ten (2) StructureBlueprintAsset.cs:37 . UnitBlueprintAsset.cs:45
```

***Ayrımın kendisi iddianın asıl konusuydu ve GÜÇLENDİ:*** çekirdek üç
assembly'de (`Core`, `Combat`, `Battle`) taban listesi taşıyan tip sayısı
**sıfır** — 30 dosya, 30 tip, sıfır kalıtım. Sekizin sekizi de motor tipinden
türüyor ve hiçbiri **proje-yerel** bir tabandan türemiyor. Yani kalıtım bu
projede bir tasarım aracı değil, motorun kapıda istediği **giriş bileti**;
duvarın öte yanında bileti isteyen kimse olmadığı için orada sıfır kalıyor.

**SOLID KARŞILIĞI** — **L** (Liskov). Ölçüsü şu: bir alt tip üst tipin yerine
geçebiliyorsa devraldığı **her** üye onda anlamlı olmalıdır.
`Structure.cs:17-20` bu sınavı açıkça uyguluyor ve `: Combatant` yazmayı
reddediyor: baraka devralacağı üyelerin yarısına uymaz — `TryRevive`, `Downed`
hâli, zorunlu `AttackProfile`, on saniyelik kurtarma penceresi.

İhlal edilseydi hangi dosya değişirdi: `Assets/Game/Core/Combat/Combatant.cs`
kendisi. `TryRevive` (`:190`) bir barakada anlamsız olduğu için `virtual` olmak
ve `Structure` içinde `return false;` diye ezilmek zorunda kalırdı; aynı şey
`ReviveHealthDivisor` (`:42`) ve `RemainingSeconds` (`:156`) için de geçerli.
Kalıtım **seçmeli değildir** — `sealed` bu satıra karşı sıfır koruma sağlar
(`Structure.cs:20`).

**REDDEDİLEN** — `Structure` yerine kalıtım:

```csharp
public sealed class Structure : Combatant
{
    public Structure(Health health, Team team) : base(health, new UnitLifecycle(), null, team) { }
}
```

**KIRILAN:** `AttackProfile` `Combatant.cs:72`'de `null` reddediliyor — yani
saldırmayan bir baraka kurulamaz. Kelepçe gevşetilse bile `Downed` hâli
barakada yazılabilir hâle gelir ve `AttackOutcome.HitAndDowned` ile
`HitAndDestroyed` ayrımı (`AttackOutcome.cs:52` ve `:61`) anlamını yitirir.

**KAZANIRDI:** Barakalar gerçekten askerlerin bir **alt türü** olsaydı — yani
kurtarma penceresi, diriltme ve düşme hâli binalarda da anlamlı olsaydı. O gün
`Structure` bir kopya olurdu, ayrı bir tip değil.

**ÜÇ OYUN** — Slay the Spire: bir kartın enerji maliyeti, hedef sayısı ve etkisi
ayrı ayrı tanımlanır; "saldırı kartı" diye tek bir kalıp yoktur ·
Vampire Survivors: bir silah menzil, hız ve hasar parçalarından kurulur ve
yükseltmeler bu parçalara ayrı ayrı dokunur · Stardew Valley: bir eşya
satılabilir, hediye edilebilir, yenebilir olabilir; bunlar bir tür ağacından
değil, eşyanın taşıdığı özelliklerden gelir.

---

## 6. Paylaşılan değişmez tanım (Flyweight'in iç durum yarısı)

Yüzlerce nesnenin **tek bir** değişmez tanım nesnesini paylaşması; örneğe özel
değişen durumun tanımın **dışında** kalması.

**HANGİ BASINÇ** — 200 okçunun her biri kendi `(10 hasar, 1 menzil)` nesnesini
taşısaydı 200 ayrı nesne doğardı ve hepsi aynı şeyi söylerdi. Ama daha ağır olan
baskı bellek değil **doğruluk**: profil değişebilir olsaydı, onu paylaşan her
birim habersiz etkilenirdi (`AttackProfile.cs:22-23`).

**KODDA NEREDE**

```
Assets/Game/Core/Combat/AttackProfile.cs:40   sealed class · ctor :45 · Damage :68 · Range :74
Assets/Game/Core/MoveProfile.cs:42            sealed class · ctor :47 · Range :64
```

İkisinde de `set` yok, alan yok, doğrulama kurucuda. `AttackProfile.cs:6-11`
paylaşımın ölçüsünü de yazmış: ölçü `==` **değil** (Equals yazılmadığı için o
karşılaştırma `false` döner), ölçü **yerine geçebilirlik**.

Karşı yönü de kodda: örneğe özel değişen durum tanımın içine **girmiyor**.
Can `Health.cs:29`'da, hâl `UnitLifecycle.cs:82`'de, taraf `Combatant.cs:145`'de
yaşıyor. `AttackProfile.cs:68-70` zırhın, direncin ve kritik çarpanının neden
buraya eklenmediğini yazıyor: tanım sessizce bir **formüle** dönerdi.

**SOLID KARŞILIĞI** — **S**. `AttackProfile` değişmek için tek bir sebep taşır:
bir saldırı türünün tanımının değişmesi. İhlal edilseydi — yani hasar formülü
bu tipe girseydi — değişmek zorunda kalan dosya
`Assets/Game/Core/Combat/DamageRules.cs` olurdu; oradaki `ResolveRemaining`
(`:33`) çağıranı olmayan bir metoda dönerdi ve `DamageRules.cs:19-22`'de yazılı
kazanç (formülün girdi uzayı sahibininkinden geniş olduğu için negatif yolların
da sınanabilmesi) kaybolurdu.

**REDDEDİLEN** — `MoveProfile.cs:42`'daki `sealed class` yerine `readonly struct`:

```csharp
public readonly struct MoveProfile
{
    public MoveProfile(int range) { ... }
    public int Range { get; }
}
```

**KIRILAN:** İki ayrı şey. ① `default(MoveProfile)` kurucuyu **atlar** ve
`Range` sıfır doğar; sıfır burada "kök salmış" demek olduğu için o örnek
kusursuz görünür — `readonly` ve get-only bu kapıyı **kapatmaz**, ikisi de
değişmeyi engeller, doğuşu değil (`MoveProfile.cs:15-19`). ② İkizi
`AttackProfile` tarafında her alan okumasında ve her parametre geçişinde yeni
bir **kopya** doğar; "yüzlerce asker tek profili paylaşır" cümlesi sessizce
yalan olur ve `AttackResolver.cs:40`'taki `null` koruması anlamsızlaşır
(`AttackProfile.cs:30-34`).

**KAZANIRDI:** Tanım gerçekten tek bir sayıdan ibaret kalsaydı **ve** kurucusuz
doğmanın anlamsız olduğu bir aralık taşısaydı — örneğin sıfırın geçersiz olduğu
bir menzil. O gün `default` tuzağı kendiliğinden kapanır ve `struct`'ın kopya
maliyeti bir `int` kadar olurdu.

*****TAM BİR FLYWEIGHT DEĞİL***** — Flyweight deseninin iki yarısı var: (a)
paylaşılan değişmez **iç durum**, (b) o paylaşımı yöneten bir **havuz/fabrika**.
Bu projede (a) var, (b) yok — profilleri üreten yer düz bir `new`, ve iki demo
birimin ikisi de **kendi** profilini alıyor: `NewCombatant` her çağrıda yeni bir
`AttackProfile` kuruyor.

```
BoardAdapter.cs:1089   private Combatant NewCombatant(Team team)
BoardAdapter.cs:1099   new AttackProfile(damage, attackRange),
```

Yani paylaşım bugün **mümkün** ama **yapılmıyor**.
`HENÜZ YOK → 02-sonraki-asamalar.md · Aşama 1 (ScriptableObject)`.

**ÜÇ OYUN** — Slay the Spire: aynı kartın iki kopyası aynı metni ve aynı
maliyeti taşır, ama biri yükseltilmişse ötekinden ayrılır · Vampire Survivors:
ekrandaki yüzlerce aynı düşman aynı hasar ve hız tanımını paylaşır, canları
ayrıdır · Stardew Valley: her "kırmızı lahana" aynı satış fiyatını ve aynı
büyüme süresini taşır, ama hangi tarlada olduğu her birine özeldir.

---

## 7. Sonuç değeri kanalı (result enum) — istisna yerine

Bir denemenin sonucunu, istisna fırlatmak yerine **adlandırılmış bir değer**
olarak döndürmek; ve reddin sebebini çağıranın ayırt edebilmesi.

**HANGİ BASINÇ** — "Taşındı mı" sorusu tek bir `bool`'a sıkıştırıldığında üç
ayrı soru aynı cevaba düşüyordu ve çağıran üçüne farklı tepki veriyordu: tahta
dışı bir tıklama sessizce yutulur, dolu hücre uyarı ister, menzil dışı ise yol
bulucuya "önce yaklaş" der. `bool` ile yazılsaydı bu ayrım, çağıranın içinde
`MoveAction`'ın kurallarını kopyalayan **ikinci bir kontrol** olarak yeniden
doğardı (`MoveOutcome.cs:16-21`).

İkinci ve daha ince baskı: hangi cevabın istisna, hangisinin sonuç değeri
olacağı. Ölçü kodda yazılı: ret sebebi çağıranın **yapabileceği** bir şeyi
göstermelidir ve "kodun bozuk" bunlardan biri değildir (`MoveAction.cs:92-98`).

**KODDA NEREDE**

```
Assets/Game/Core/MoveOutcome.cs:26            5 değer  · sıfırıncı :33 (RejectedInvalidDestination)
Assets/Game/Core/Combat/AttackOutcome.cs:27   6 değer  · sıfırıncı :34 (RejectedInvalidTarget)
Assets/Game/Battle/PlacementOutcome.cs:24     yerleştirme sonucu
Assets/Game/Battle/ReviveOutcome.cs:25        diriltme sonucu
```

Sıfırıncı değer kararı dört enum'da da aynı ve bilinçli: `MoveOutcome.cs:28-32`
şunu yazıyor — sıfır, dilin atanmamış her alana verdiği değerdir; `Moved` başa
alınsaydı hiç hareket denenmeden okunan bir alan "taşındı" derdi ve derleyici
susardı. Aynı karar `Team.cs:24-28`'de (`None` sıfırıncı) ve
`StructureState.cs:47`'de (`Destroyed` sıfırıncı) tekrarlanıyor.

**SOLID KARŞILIĞI** — **O** (açık/kapalı) ve **I** (arayüz ayrımı) birlikte.
**O**: `AttackOutcome.cs:68-72` yeni değerlerin neden **sona** eklendiğini
yazıyor — ret ailesinin yanına sokulsaydı aradaki üç değer sessizce yeniden
numaralanırdı.
**I**: her çağıran yalnızca ilgilendiği değerlere dallanır; bir sonuç
`struct`'ında `Rejected`, `DamageDealt` ve `Downed` alanlarının ikisi her
çağrıda anlamsız kalırdı (`AttackOutcome.cs:21-24`).

İhlal edilseydi hangi dosya değişirdi: `Assets/Game/Unity/BoardAdapter.cs`.
Oradaki `ReactToMove` (`:1310`) ve saldırı ikizi `ReactToAttack` (`:1232`),
beş ret sebebi tek bir `bool`'a inseydi, sebebi **yeniden hesaplamak** zorunda
kalırdı — yani `MoveAction`'ın kurallarını ikinci kez yazardı.

**REDDEDİLEN** — `MoveOutcome`'un üç ret değeri yerine tek bir `Rejected`:

```csharp
public enum MoveOutcome { Rejected, Moved, RejectedActorCannotAct }
```

**KIRILAN:** Ayıran ölçüt **sebep sayısı değil davranış sayısı**. "Hücre dolu"
bir tur sonra değişebilir ("bekle, hücre boşalır"); "geçersiz hedef" **asla**
değişmez ("bir daha hiç deneme"). Tek değer bu çizgiyi siler ve yol bulucu
sonsuza dek yeniden dener. Gerekçe `MoveOutcome.cs:35-38`'de yazılı.

**KAZANIRDI:** Çağıran tarafta gerçekten **tek** bir davranış kalsaydı — yani
her ret sebebi aynı log satırına ve aynı geri bildirime düşseydi. O gün beş
değer, dört tanesi hiç okunmayan bir çeşitlilik olurdu.

**ÜÇ OYUN** — Slay the Spire: bir kart oynanamadığında oyun sebebi ayırt eder —
enerji, hedef, ya da kartın kendi kısıtı · Vampire Survivors: ***EŞLEŞMEZ***
orada oyuncuya dönen bir "ret sebebi" kanalı yok; hasar ya olur ya olmaz ·
Stardew Valley: bir eşya konulamadığında sandık dolu mu, eşya konulamaz mı, yer
uygun değil mi — üçü ayrı geri bildirim verir.

---

## 8. Gözlemci (Observer) — C# `event` ile

Bir olayın olduğunu, kimin ilgilendiğini bilmeden duyurmak. C#'ta `event`
anahtar kelimesi bunu dile gömüyor.

**HANGİ BASINÇ** — Dönüş değeri **yetmiyordu**. `UnitLifecycle.Tick` içindeki
`Downed → Dead` geçişini **soran** kimse yok: `Tick`'i çeviren taraf oyun
döngüsüdür ve o geçişle ilgilenmez. İlgilenen (görselin durumu, ses, skor)
**başka yerdedir**. Kural kodda tek cümleyle yazılı: "SORAN YOKKEN İLGİLENEN
VARSA, ŞEKİL EVENT'TİR" (`UnitLifecycle.cs:72`).

Bunun **tersi** de kodda ölçülmüş: `StructureLifecycle`'da olay **yok** ve bu
bir unutma değil. Oradaki tek geçiş (ayakta → yıkık) her zaman bir hasar
çağrısından doğar ve cevabı dönüşle alınır (`StructureLifecycle.cs:67-71`,
`OnHealthDepleted :103` `bool` döndürüyor). Aynı gerekçe, zıt sonuç.

**KODDA NEREDE** — üç duraklık bir zincir ve her durakta bir şey kazanılıyor:

```
① Assets/Game/Core/Combat/UnitLifecycle.cs:80
   public event Action<UnitState> StateChanged;              taşıdığı: YENİ durum

② Assets/Game/Core/Combat/Combatant.cs:111
   public event Action<UnitState, UnitState> StateChanged;   kazandığı: ÖNCEKİ durum
   çevirici :114 · önceki durumu tutan alan :52
   abonelik kurucunun EN SONUNDA :86

③ Assets/Game/Battle/Battle.cs:179
   public event Action<Unit, UnitState, UnitState> UnitStateChanged;  kazandığı: KİMLİK
   kapanış (closure) üretimi :219 · abonelik :221 · sözlük :74

④ Assets/Game/Unity/BoardAdapter.cs:288 OnEnable  += · :282 OnDisable  -=
   dinleyici :299 → ApplyStateVisual :943 → UnitView.SetState (UnitView.cs:173)
```

`Battle.cs:81`'teki `stateForwarders` sözlüğü bu desenin en pahalı ayrıntısı:
abone edilen şey her birime özel bir **kapanış** (closure — çevresindeki
değişkeni içine alan anonim fonksiyon) ve kapanışlar birbirine eşit değildir.
Aynı metni ikinci kez yazarak abonelik **çözülemez**; sökmek için tam olarak o
örneğin saklanması gerekir. Sökme yeri `Battle.cs:347-351`.

Mekanizmanın tam hikâyesi: [../deep/konular/01-olay-zinciri.md](../deep/konular/01-olay-zinciri.md).
Delegenin, `event`in ve kapanış kimliğinin dil tarafı:
[../deep/dil/04-delege-olay-ve-kapanis.md](../deep/dil/04-delege-olay-ve-kapanis.md).

**SOLID KARŞILIĞI** — **D** (bağımlılık tersine çevirme). Ölçüsü şu:
`UnitLifecycle` kendisini dinleyenin ne olduğunu bilmez; ok **yayıncıdan
aboneye** değil, **aboneden yayıncıya** kuruluyor.

İhlal edilseydi hangi dosya değişirdi: `Assets/Game/Core/Combat/UnitLifecycle.cs`.
Ekranı doğrudan güncellemesi için `UnityEngine`'i tanıması gerekirdi ve
`Assets/Game/Core/Combat/GridStrategy.Combat.asmdef`'in `noEngineReferences: true`
satırı düşerdi. O gün `Assets/Tests/EditMode/Combat/UnitLifecycleTests.cs`
sahnesiz koşamazdı.

**REDDEDİLEN** — `Combatant.cs:111`'deki kendi olayı yerine, iç olayı `add`/`remove`
ile dışarı aktarmak:

```csharp
public event Action<UnitState> StateChanged
{
    add    { lifecycle.StateChanged += value; }
    remove { lifecycle.StateChanged -= value; }
}
```

**KIRILAN:** Aktarım, dış dinleyicinin bağını **iç parçaya** düşürür ve o bağ
`Combatant` yok olsa bile kopmaz. Ayrıca imza tek değerli kalır — "önceden
neydi" bilgisi hiç doğmaz ve `Combatant.cs:47-51`'de yazılı olan şey olur:
yeni durumdan **türetmek**, sahibindeki geçiş tablosunun tersten yazılmış
kopyası olurdu ve dördüncü durum eklendiği gün yalan söylerdi.

**KAZANIRDI:** `Combatant` gerçekten şeffaf bir sarmalayıcı olsaydı — yani
zenginleştirecek hiçbir bilgisi olmasaydı. O gün aktarım bir katman siler, bir
katman eklemez.

**ÜÇ OYUN** — Slay the Spire: bir düşman öldüğünde reçete tetiklenir, altın
düşer ve savaş bitiş kontrolü yapılır; ölümü bildiren taraf bunların hiçbirini
bilmez · Vampire Survivors: bir seviye atlandığında yükseltme ekranı açılır,
oyun durur ve müzik kısılır · Stardew Valley: gün bittiğinde ekinler büyür,
hayvanlar üretir ve kasaba etkinlikleri ilerler — üçünü de aynı "gün bitti"
duyurusu tetikler.

---

## 9. Kimlik + yan tablo (identity with side tables)

Bir varlığın **yalnız kimliğini** taşıyan çıplak bir tip, ve o kimliğe
anahtarlanmış ayrı tablolar. "Yan tablo" burada `Dictionary<Kimlik, Parça>`
demek.

**HANGİ BASINÇ** — Tahtada duran şeyin ne **olduğu** ile ne **yapabildiği**
farklı assembly'lerde yaşıyor. `GridStrategy.Core` konumu bilir ama savaşı
tanımaz; `GridStrategy.Combat` savaşı bilir ama tahtayı tanımaz — ve ikisi
birbirini **görmez**. Bir askerin canını `Unit`'in içine koymak bu duvarı
yıkardı. Gerekçe `Unit.cs:35-37` ve `Battle.cs:23-27`'de yazılı.

İkinci baskı: aynı tahtada hem askerler hem binalar duruyor. Tür başına ikinci
bir kimlik tipi, ikinci bir tahtayı **zorunlu** kılar ve "bu hücre dolu mu"
sorusu ikiye bölünür (`Unit.cs:20-24`).

**KODDA NEREDE**

```
kimlik   Assets/Game/Core/Unit.cs:41       tek alan: Name :56 (get-only) · başka üye yok
tahta    Assets/Game/Core/UnitGrid.cs:26   Unit[,] cells :28
yan tablolar
         Assets/Game/Battle/Battle.cs:59   Dictionary<Unit, Combatant>
         Assets/Game/Battle/Battle.cs:66   Dictionary<Unit, Structure>
         Assets/Game/Battle/Battle.cs:81   Dictionary<Unit, Action<UnitState, UnitState>>
         Assets/Game/Unity/BoardAdapter.cs:199  Dictionary<Unit, UnitView>
sistemler  (durumsuz kural tipleri — 1. desen) · akış sahipleri (2. desen)
```

Anahtarı ayakta tutan şey `Unit.cs`'te bir kodun **varlığı değil yokluğu**: ne
`Equals` ne `GetHashCode` geçersiz kılınmış, dolayısıyla anahtar referans
eşitliğidir. Ada göre `Equals` eklendiği gün "Piyade" adlı iki ayrı birim üç
assembly'deki sözlüklerde tek girdiye çökerdi (`Unit.cs:51-55`).

**SOLID KARŞILIĞI** — **S** ve **I** birlikte. `Unit` değişmek için tek bir
sebep taşır: kimliğin kendisinin değişmesi — ki bu hiç olmuyor. **I**: hiçbir
tüketici kullanmadığı üyeye bağımlı değil; `UnitGrid` tuttuğu şeyin ne olduğunu
bilmez (`UnitGrid.cs:15-17`).

İhlal edilseydi hangi dosya değişirdi: `Assets/Game/Core/GridStrategy.Core.asmdef`.
Can ve durum `Unit`'e girseydi `Core`, `Combat`'ı referans etmek zorunda kalırdı
— ve o an `MoveProfile.cs:21-23`'te yazılı olan ayrım (hareket menzili tahtaya,
saldırı menzili savaşa ait) çökerdi.

**REDDEDİLEN** — `Battle.cs:528`'deki `TryGetPosition` yerine bir konum tablosu:

```csharp
private readonly Dictionary<Unit, (int x, int y)> positions = new Dictionary<Unit, (int, int)>();
```

**KIRILAN:** İkinci bir **doğruluk kaynağı**. `MoveAction.Execute`
(`MoveAction.cs:63`) tahtayı doğrudan değiştirir; sözlük bunu duymaz ve birim
yaklaşmış olduğu hâlde saldırı "menzil dışı" der. Önbellek bir hız kararı değil,
ikinci bir doğruluk kaynağı yaratma kararıdır (`Battle.cs:523-526`).

**KAZANIRDI:** Tahta boyutu gerçekten ölçülebilir bir darboğaz olduğunda —
`TryGetPosition` bugün `Width × Height` hücreyi tarıyor (`Battle.cs:535-547`)
ve tahta `3×5` (`BoardAdapter.cs:114-115`), yani en fazla 15 hücre. Ölçü
büyüdüğünde ve tarama profilde göründüğünde tablo doğru seçim olur — ama o gün
**tek yazma kapısı** zorunlu hâle gelir.

*****BU ECS DEĞİL***** — Şekil ECS'e (Entity Component System — varlık, bileşen,
sistem üçlüsü) **yakın**: `Unit` bir varlık kimliği gibi, sözlükler bileşen
depoları gibi, durumsuz kural tipleri sistemler gibi duruyor. Ama üç şey
eksik ve bunlar ECS'in **tanımı**: bileşenlerin bitişik dizilerde tutulması,
bir sistem döngüsünün bileşen kümesine göre iş dağıtması, ve bellek
yerleşiminin (chunk) performans için tasarlanması. Burada depo bir
`Dictionary`, yani bitişik değil dağınık. Farkın tamamı ve ne zaman
kapanacağı: [02-sonraki-asamalar.md](02-sonraki-asamalar.md) · Aşama 5.

**ÜÇ OYUN** — Slay the Spire: bir kartın kimliği ile üstündeki geçici etkiler
ayrı tutulur; aynı kart iki farklı savaşta farklı güçlenmiş olabilir ·
Vampire Survivors: ekrandaki her düşman aynı tanıma bakar ama kendi canını ve
konumunu taşır · Stardew Valley: bir köylünün adı ile ona duyulan yakınlık ayrı
kaydedilir; ad değişmez, sayı her hediyede değişir.

---

## İncelenip elenen desen adayları

Aşağıdakilerin hiçbiri bu projede **yok**. Yokluk bir eksiklik değil, bir
ölçüm sonucudur — ve ne zaman doğacakları
[02-sonraki-asamalar.md](02-sonraki-asamalar.md) ile
[03-kavram-borc-defteri.md](03-kavram-borc-defteri.md)'de yazılı.

| Aday | Neden yok — ölçü |
|---|---|
| Command | `MoveAction.cs:42`, `AttackAction.cs:36`, `BattleActions.cs:50` üçü de `static class`; saklanacak nesne yok, geri alma yok |
| Strategy | ***ÖLÇÜ DEĞİŞTİ, HÜKÜM DEĞİŞMEDİ*** — üretim kodunda artık **altı** `interface` var ve altısı da `GridStrategy.Unity` içinde: `IPlacementBoard`, `IBoardMode`, `IBoardModeHost`, `IPlacementModeHost`, `IUnitOrder`, `IUnitOrderHost`. İkisi birden fazla uygulama taşıyor ama ikisi de Strategy değil: `IUnitOrder` (`AttackOrder`, `ReviveOrder`) bir **Command**'dır ve yaratım noktası hangi tipi istediğini bilir; `IBoardMode` (`IdleBoardMode`, `StructurePlacementMode`) bir **State**'tir ve seçimi `BoardModeMachine` bir geçişle yapar. Strategy'nin ölçüsü arayüz sayısı değil, aynı çağıranın iki uygulama arasında **seçim** yapmasıdır — ve projede otomatik hedef seçen bir algoritma hiç yok; hedefi oyuncu tıklıyor. Ayrıntı: [13-desen-secim-rehberi.md](13-desen-secim-rehberi.md) |
| Factory | Üç doğum yolu var — `UnitBlueprint.CreateCombatant`, `StructureBlueprint.CreateStructure` ve `BoardAdapter.NewCombatant` — ve **üçü de** dönüş tipini çağırana söylüyor. `StructureProduction.Produce` üyesi somut tipi `out Combatant produced` parametresiyle **imzasında** taşıyor. `Combatant` ve `Structure` ikisi de `sealed`, üretilen somut tip sayısı **bir**. ***Nesne üretmek fabrika değildir; fabrikanın ölçüsü çağıranın dönüş TİPİNİ bilmemesidir.*** Ayrıntı: [13-desen-secim-rehberi.md](13-desen-secim-rehberi.md) |
| Singleton | Üretim kodunda değiştirilebilir hiçbir `static` alan yok; tek `static` alan `TurnState.cs:44` ve `readonly` + salt okunur görünüm |
| Service Locator | Kayıt defteri yok; bağımlılıklar kurucudan ya da `[SerializeField]`'den geliyor |
| Decorator | Sözleşme artık **var** (`IPlacementBoard`), ama Decorator'ın ölçüsü sözleşme değil: aynı sözleşmeyi uygulayıp **başka bir uygulamayı sarmalayan** bir tip gerekir. Uygulayan tek ve hiçbir şeyi sarmalamıyor — sıfır |
| MVP / MVC | `UnitView.cs:43` edilgen bir görünüm, ama karşısında bir sunucu (presenter) tipi yok; niyet çevirisi `BoardAdapter` içinde <!-- ATIF-MUAF: tablo hücresi; alıntı biçimi atfın satır BAŞINDA olmasını ister, tablo satırında mümkün değil --> |
| Nesne havuzu | `Assets/` altında `Pool` kelimesi hiç geçmiyor; `Instantiate` (`BoardAdapter.cs:1078`) ve `Destroy` (`BoardAdapter.cs:1473`) doğrudan çağrılıyor |
| ScriptableObject | ***ARTIK VAR*** — 2026-08-25'te iki tip türedi: `UnitBlueprintAsset.cs:45` ve `StructureBlueprintAsset.cs:37`. `AttackProfile.cs:13` ve `:44`'teki **karar notları** hâlâ yerinde ve hâlâ doğru: o tip bilerek düz C# kaldı, çünkü doğrulamasını kurucuda yapıyor. Yani aday "yok" değil, ***kısmen uygulandı*** |
| Olay veri yolu | Ortak bir yayın noktası yok; **10** `event` doğrudan zincir hâlinde bağlı (8. desen). Sayı 2026-08-25'te üçten ona çıktı — ama veri yolunun ölçüsü sayı değil **dolaylılık**: onun onu da yayıncısını tip olarak tanıyan bir aboneye gidiyor, ortada kayıt defteri yok |
| Coroutine / `async` | `Assets/Game/` altında `IEnumerator`, `yield`, `async`, `Task`, `Awaitable` kelimelerinin hiçbiri geçmiyor |

---

## Alıntı çapaları

Aşağıdaki satırlar bu belgede geçen satır numaralarının **çapasıdır**. Her satır
`Tools/check-doc-code-refs.py`'nin ALINTI katmanına, o numarada duran kodun
BİREBİR metnini verir. Ölçüldü: ALINTI katmanı 3 satırlık kaymayı bile %100
yakalıyor, YAKIN AD katmanı 6 satırlık kaymanın %1'ini. Tablo hücrelerindeki ve
cümle içindeki atıflar alıntı biçimine giremez — o biçim atfın satır BAŞINDA
olmasını ister. Kod kaydığında kızacak olan yer burasıdır; kızdığı gün bu
belgede geçen aynı numaraların hepsi elden geçirilir.

```
Assets/Game/Core/MoveAction.cs:42             public static class MoveAction
Assets/Game/Battle/BattleActions.cs:50        public static class BattleActions
Assets/Game/Core/Combat/AttackProfile.cs:40   public sealed class AttackProfile
Assets/Game/Core/MoveProfile.cs:42            public sealed class MoveProfile
Assets/Game/Unity/BoardAdapter.cs:111         public sealed class BoardAdapter : MonoBehaviour
Assets/Game/Unity/BoardAdapter.cs:194         private Grid unityGrid;
Assets/Game/Unity/BoardAdapter.cs:209         private readonly Dictionary<Unit, UnitView> unitViews =
Assets/Game/Unity/BoardAdapter.cs:931         battle.Tick(Time.deltaTime);
Assets/Game/Unity/BoardAdapter.cs:1078         UnitView view = Instantiate(unitPrefab, transform);
Assets/Game/Unity/BoardAdapter.cs:1089         private Combatant NewCombatant(Team team)
Assets/Game/Unity/BoardAdapter.cs:1473        Destroy(view.gameObject);
Assets/Game/Battle/Battle.cs:59               private readonly Dictionary<Unit, Combatant> combatants =
Assets/Game/Battle/Battle.cs:66               private readonly Dictionary<Unit, Structure> structures =
Assets/Game/Battle/Battle.cs:81               private readonly Dictionary<Unit, Action<UnitState, UnitState>> stateForwarders =
Assets/Game/Battle/TurnState.cs:44            public static readonly IReadOnlyList<Team> DefaultTurnOrder =
Assets/Game/Unity/UnitView.cs:173             public void SetState(UnitState state)
```


## İlgili

- Bu ağacın yönlendirmesi: [README.md](README.md)
- Eksiklerin tetikleyici koşulları: [02-sonraki-asamalar.md](02-sonraki-asamalar.md)
- Kapsama tablosu: [03-kavram-borc-defteri.md](03-kavram-borc-defteri.md)
- Tip başına gerekçeler: [../deep/kod/README.md](../deep/kod/README.md)
- Mekanizma anlatıları: [../deep/konular/](../deep/konular/)
