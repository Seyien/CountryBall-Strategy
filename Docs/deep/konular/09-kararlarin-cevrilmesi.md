# Kararların çevrilmesi — eski kod, yeni kod ve aradaki cümle

Bu belge bir "değişiklik günlüğü" değil. Değişiklik günlüğü *ne* olduğunu
yazar; burada yazılan şey **hangi ölçüm bir kararı çevirdiği**. Projedeki kural
şu: reddedilen alternatif kodda kalır, silinmez. Bu belge o kuralın zaman
eksenindeki karşılığı — bir karar çevrildiğinde ESKİSİ de burada durur, çünkü
öğrenen için asıl bilgi yeni kodda değil, **ikisinin arasındaki cümlededir**.

Okuma sırası: `Docs/deep/README.md` sınırı çiziyor (kararın kendisi kodda, çok
dosyalı mekanizma burada). Tek tipin gerekçesi için `Docs/deep/kod/` altındaki
ayna belgeye bak.

---

## 1 · Yerleştirme kipi: iki `bool` → kip makinesi

### Eskisi

Kip, kareler arasında yaşayan iki bayrakla anlatılıyordu ve o bayraklar dosyanın
her yerine dağılmıştı:

```csharp
private bool isPlacingStructure;
private bool ghostIsCarried;

// ...ve dokuz ayri yerde
if (isPlacingStructure) { return; }
isPlacingStructure = true;
ghostIsCarried = false;
if (ghostIsCarried) { ... }
isPlacingStructure = false;
```

Bekleyen vuruş da aynı biçimde, dört tekil alanla:

```csharp
private Unit pendingStrikeAttacker;
private Unit pendingStrikeTarget;
private int  pendingStrikeX;
private int  pendingStrikeY;
```

### Kırılan şey

Bu bayraklar birer bayrak değil, birer DURUMDU: "tıklama ne demek", "hayaleti
kim yazar", "Update ne yapmalı" sorularının cevabı tam olarak onlara göre
değişiyordu. Ölçülen zarar bir titreme değil, **sessiz körlüktü** — klavyeli kip
ile sürükleme aynı hayaleti yazıyor, kaybeden taraf hiçbir uyarı vermeden
görünmez hâle geliyordu. İptali "unutmak" da mümkündü, çünkü iptal bir geçişin
sonucu değil, elle yazılan bir satırdı.

### Yenisi

```csharp
public interface IBoardMode
{
    bool OwnsPointer { get; }
    void Enter();
    void Exit();
    void Advance();
    bool ConsumesClick(Unit clicked);
}
```

`BoardModeMachine.Enter` önce `Exit`, sonra `Enter` çağırıyor — yani iptal artık
**hiçbir yerde elle yazılmıyor**, geçişin kendisi yapıyor.

### Referanstan alınan ve reddedilen

Ölçüt referans `IBuildingState`'in üç üyesini veriyordu
(`EndState` / `OnAction` / `UpdateState`). Alınan şey o disiplin. Reddedilen şey
onun dünyaya `Map.Instance` ile ulaşması: bizde bağ `[SerializeField]` ve
yetenekle daraltılmış bir host arayüzü üzerinden geliyor, ve Unity testlerinin
sahnesiz koşabilmesinin tek sebebi bu.

**TEK CÜMLE:** Bir `bool`, cevabı ona göre değişen üç soru varsa artık bir bayrak
değil bir durumdur.

---

## 2 · Kip makinesi → emir tablosu: State'in NEREDE bittiği

Bu, birinci maddenin devamı **ve sınırı**. Kip makinesi doğruydu; yanlış olan
şey ondan fazlasını beklemekti.

### Kırılan şey

Operatör şunu bildirdi: *"iki taraf için paralel olarak saldırı aşamalarını
gerçekleştiremiyorum."* Sebep ayarda değil, sahiplikteydi — bekleyen vuruş
**tahta başına tekti**:

```csharp
private Unit pendingStrikeAttacker;   // TEK alan
private Unit pendingStrikeTarget;     // TEK alan
```

İkinci bir birime emir verildiği an birincisinin emri siliniyordu. Kip makinesi
bunu düzeltemez, çünkü **kip makinesi de tektir**: tahtanın aynı anda tek bir
kipi vardır ve bu doğrudur.

### Ayrımın kendisi

> **State**, TAHTANIN şu an ne yaptığıdır ve **tektir**.
> **Order/Command**, HER BİRİME ne söylendiğidir ve **çoğuldur**.

Kalıcı saldırı çoğuldur: iki takımdan üç birim aynı anda emir taşıyabilmeli, ve
biri menzilden koptuğunda yalnız onunki iptal olmalı. Çoğul bir kavramı tekil
bir makineye sokmak, tam olarak birinci maddedeki hatayı ikinci kez yapmaktır.

### Yenisi (yazıldı)

```csharp
public interface IUnitOrder
{
    Unit Target { get; }
    OrderProgress Advance();   // Continue / Finished / Cancelled
    string Describe();
}

private readonly UnitOrderBook orders = new UnitOrderBook();   // Dictionary<Unit, IUnitOrder>
```

Kip makinesi GİRDİNİN anlamını sahiplenmeye devam eder; emirler ondan bağımsız
yaşar ve birim başına birdir.

**PLANDAN AYRILAN İKİ NOKTA, ve ikisi de ölçüldü:**

`Tick(float deltaSeconds)` yerine `Advance()`. Emrin kendi saati YOK: bekleme
süresini Core sayıyor (`Combatant`'ın sayacını `battle.Tick` ilerletiyor),
yürüyüşü ise ekranın kendi saati. Parametre alınsaydı hiçbir emrin okumadığı bir
argüman doğardı — ve "bekleme kuralını emir de yazar mı" sorusu açık kalırdı.

`Target` ve `Describe()` plana eklendi, çünkü ikisinin de çağıranı var: tahtadan
kalkan bir kimliği hedefleyen emirleri süpürmek ve "bu tıklama zaten yazılı
emrin aynısı mı" sorusu `Target`'ı okuyor; seçilen birimin emrini oyuncuya
söyleyen satır ise `Describe()`'ı. İkincisi bir tip sorgusunun (`order is
AttackOrder`) yerine geçiyor.

**TEK CÜMLE:** Bir pattern'in sınırı, sahiplendiği şeyin çokluğudur — tekil bir
sahip çoğul bir kavramı taşıyamaz.

---

## 2a · Seçim: isabetten sonra → EMİR yazıldığı an

### Eskisi

```csharp
if (landed)
{
    ReleaseSelectionAfterStrike(attacker);
}

private void ReleaseSelectionAfterStrike(Unit attacker)
{
    if (!ReferenceEquals(attacker, selectedUnit)) { return; }
    if (IsStructureIdentity(attacker))            { return; }

    ClearSelection();
    Debug.Log($"[Board] '{attacker.Name}' struck; the selection was released.", this);
}
```

### Kırılan şey

Vuruş TEK SEFERLİK bir olaydı, dolayısıyla "isabet etti, oyuncunun bu birimle
işi bitti" doğruydu. Kalıcı emirle o dünya değişti: aynı emir bekleme süresi
dolduğu her an yeniden vuruyor. Her isabette seçimi düşürmek, birimini gözlemek
ya da yönlendirmek için yeniden seçen oyuncunun elinden onu **tekrar tekrar**
alırdı.

Bu ayrımın ikinci yarısı daha derinde duruyordu: eski emir ayakta kalmak için
"saldıran hâlâ seçili mi" diye soruyordu.

```csharp
// PendingStrikeMode.IsAlive()
if (!ReferenceEquals(host.StrikeAttacker, host.SelectedUnit))
{
    return false;
}
```

Yani **seçimi bırakmak emri iptal ediyordu**. Operatörün istediği şey ("emir
verildiğinde seçim kaldırılsın") bu bağ dururken imkânsızdı; ikisi birbirinin
karşıtıydı.

### Yenisi

Seçimi bırakan yer emrin YAZILDIĞI an oldu ve bağ, bir bayrakla değil **üyeyi
silerek** koptu — `IUnitOrderHost`'ta `SelectedUnit` diye bir soru yok:

```csharp
private void IssueOrder(Unit unit, IUnitOrder order)
{
    orders.Write(unit, order);

    if (ReferenceEquals(unit, selectedUnit))
    {
        ClearSelection();
    }
}
```

Eski üyenin iki koruma satırı da gereksizleşti ve silindi: bu üyeye ulaşan tek
yol oyuncunun kendi tıklaması, kendiliğinden ateş eden kule buraya hiç gelmiyor
(emri yok) ve yapı seçimi saldırı dalından ÖNCE dönüyor. Koruma bir koşuldan
YAPIYA taşındı.

**TEK CÜMLE:** İki kural birbirinin karşıtıysa biri yanlış yerdedir; doğru
onarım koşul eklemek değil, yanlış yerdeki soruyu silmektir.

---

## 2a-i · Aynı kararın ödenmemiş yarısı: cümle SONRA → cümle ÖNCE

Bu madde 2a'nın devamı değil, onun **ödenmemiş bedeli**. `IssueOrder` seçimi
bırakıyor; bırakılan seçimi okuyan hiçbir satır o çağrının ARDINDA kalamaz.
Saldırı dalında bu kendiliğinden doğruydu — emir üyenin son satırı. Diriltme
dalında değildi.

### Eskisi

```csharp
IssueOrder(selectedUnit, new ReviveOrder(this, selectedUnit, target));
Debug.Log(
    $"[Board] '{selectedUnit.Name}' is walking to '{target.Name}' and will revive on arrival.",
    this);
```

### Kırılan şey

`selectedUnit` ikinci satırda **null**: `IssueOrder` → `ClearSelection` →
`selectedUnit = null`. Yürüyerek varılan kaldırma yolu her koşuşunda
`NullReferenceException` veriyordu, yani yol tamamen kapalıydı.

Kaybolan kural yeni değildi; aynı turda silinen `ReactToAttack` satırının
yanında adıyla yazılıydı: *"SEÇİM EN SONDA BIRAKILIYOR ve sıra bir
karardır: ClearSelection kendi yayınını yapıyor, üste konsaydı sağ panel
vuruşun anlatıldığı satırdan ÖNCE yeniden kurulurdu."* Kural doğruydu;
taşınırken **yarısı** taşındı.

Testlerin görmemesinin sebebi de ölçüldü ve bir kapalı nokta: tahta gerçekte
`FreeForAll` ile kuruluyor, `BoardAdapterTests` ise varsayılan `Alternating`
ile. O kipte yürüyüş sırayı devrediyor ve üye emir satırına hiç varmadan
dönüyor — yani varsayılan kiple yazılmış HİÇBİR test bu çökmeyi ölçemezdi.

### Yenisi

```csharp
Debug.Log(
    $"[Board] '{selectedUnit.Name}' is walking to '{target.Name}' and will revive on arrival.",
    this);
IssueOrder(selectedUnit, new ReviveOrder(this, selectedUnit, target));
```

Ölçüsü bir test ve o test tahtayı **bilerek `FreeForAll` ile** kuruyor:
`TryCloseInOnAlly_WhenTheWalkStarts_WritesTheOrderWithoutReadingTheReleasedSelection`.

**TEK CÜMLE:** Bir çağrı görünmez bir yan etki taşıyorsa, onu okuyan her satır
artık o çağrının ÖNÜNDE yaşamak zorundadır.

---

## 2b · İptal: her tıklama → yalnız niyeti değiştiren tıklama

### Eskisi

Emir tahtaya ait olduğu için, tahtaya değen HER şey onu düşürüyordu:

```csharp
// HandleClick — üç dal, üç iptal
if (!TryReadPointerCell(...)) { CancelPendingStrike(); return; }
if (!battle.IsInsideGrid(x, y)) { CancelPendingStrike(); ... }
CancelPendingStrike();
HandleOccupiedCellClick(clicked, x, y);

// SetPlacementVisual — paletten bina almak
// "PALETTEN BİR ŞEY ALMAK DA BİR EYLEMDİR ve bekleyen vuruşu düşürür."
CancelPendingStrike();

// BoardModeMachine.Enter — yerleştirme kipine girmek
// açık kipin Exit() işi emri siliyordu
```

### Kırılan şey

Bunların hepsi emir TAHTANIN iken doğruydu. Emir birime ait olunca hepsi
tersine döndü: ikinci savaşçısını seçmek, tahtanın dışını ıskalamak ya da
paletten bina sürüklemek, ÜÇÜNCÜ bir birimin sürmekte olan saldırısını neden
kessin? Operatörün istediği paralel oyunun kendisi tam olarak bu — bina
sürüklerken savaşçının vurmaya devam etmesi.

`Enter_PlacementWhileAStrikeIsPending_DropsTheOrderInTheTransition` adlı test bu
kuralın **karar kaydıydı** ve silindi. Dünyada değişen şey yazıldı: emrin sahibi
tahta değil, birim.

### Yenisi

İptal artık niyeti gerçekten değiştiren iki dalda:

```csharp
// yeni emir eskisinin yerine geçer — defterin kendi kuralı
orders.Write(unit, order);

// yürümek: bu birimin emrini keser, ötekilerin emrine dokunmaz
CancelOrder(selectedUnit);
```

### Yan sonuç — ölü bir arayüz üyesi

`IBoardMode.ConsumesClick(Unit)` bir önceki turda eklenmişti ve tek çağıranı
"aynı hedefe gelen ikinci tıklama emrin tekrarı mı" sorusuydu. O soruyu bugün
emir defteri cevaplıyor ve daha doğru cevaplıyor — soru artık "tahtada yazılı
emir" değil "SEÇİLİ BİRİMİN emri" hakkında. Kalan iki kip de koşulsuz `false`
dönüyordu; kimsenin sormadığı bir soruya iki uydurma cevap arayüzü yalancı
yapardı, üye silindi.

**TEK CÜMLE:** Bir iptal kuralı, iptal edilen şeyin SAHİBİ değişince yeniden
sorulmak zorundadır.

---

## 2c · Emrin cinsi: bir `bool` → ikinci bir tip

### Eskisi

```csharp
// Bekleyen emrin CİNSİ: yürüyüş bitince vurulacak mı, yoksa düşmüş dost
// ayağa mı kaldırılacak.
private bool pendingStrikeIsRevive;

void IPendingStrikeHost.ExecuteStrike(Unit attacker, Unit target, int x, int y)
{
    if (pendingStrikeIsRevive)
    {
        pendingStrikeIsRevive = false;
        ReviveOutcome revived = BattleActions.Revive(battle, attacker, target);
        ReactToRevive(attacker, revived, target, x, y);
        return;
    }

    AttackOutcome outcome = BattleActions.Attack(battle, attacker, target);
    ReactToAttack(attacker, outcome, target, x, y);
}
```

Bayrağın gerekçesi kendi yanında yazılıydı ve **o gün doğruydu**: *"'yaklaş,
sonra yap' makinesinin ikinci bir kopyasını açmamanın bedeli tam olarak bu tek
bool."* İkinci bir KİP açmak, aynı üç iptal koşulunun ikinci bir kopyasını
doğururdu.

### Kırılan şey

Bedelin kendisi yok oldu. Emir bir NESNE olunca ikinci cins, ikinci bir kip
değil ikinci bir SINIF demek — ve defter ikisini de aynı gözden okuyor. Bayrağın
sırası da bir tuzaktı: `SchedulePendingRevive`, bayrağı `Write` çağrısından
SONRA yazmak zorundaydı, çünkü `WriteStrikeOrder` onu sıfırlıyordu. Ters sırada
kaldırma emri kendi cinsini yazıldığı anda unuturdu.

### Yenisi

```csharp
public sealed class ReviveOrder : IUnitOrder   // AttackOrder'ın kardeşi
{
    public OrderProgress Advance()
    {
        ...
        host.Revive(reviver, target);
        return OrderProgress.Finished;   // TEK SEFERLİK — saldırı Continue döner
    }
}
```

Sıraya bağlı hiçbir şey kalmadı; iki cinsin farkını taşıyan şey tipin kendisi.

**TEK CÜMLE:** Bir bayrağın gerekçesi "ikinci bir X açmamak" ise, X ucuzladığı
gün bayrak gerekçesiz kalır.

---

## 2d · Hedefin hücresi: emir yazılırken → her vuruşta taze

### Eskisi

```csharp
private int targetX;
private int targetY;

public void Write(Unit attacker, Unit target, int x, int y)
{
    targetX = x;
    targetY = y;
    host.WriteStrikeOrder(attacker, target);
}
```

### Kırılan şey

Tek seferlik bir vuruşta hedef, emirle vuruş arasında en fazla bir yürüyüş boyu
kımıldıyordu. Kalıcı emirde hedef **sürekli** kımıldıyor: saklanan hücre ikinci
vuruşta çoktan eskimiş olurdu ve mermi, hedefin ARTIK OLMADIĞI hücreye uçardı.

### Yenisi

```csharp
AttackOutcome IUnitOrderHost.Strike(Unit attacker, Unit target)
{
    if (!battle.TryGetPosition(target, out int x, out int y))
    {
        return AttackOutcome.RejectedInvalidTarget;
    }
    ...
}
```

**TEK CÜMLE:** Bir kere okunan bir konum, bir kere yapılan bir iş içindir.

---

## 3 · Düşmüş birim: anlık ölüm → düşme canı

Bu maddenin öğreticiliği, çevrilen kararın **bir tur içinde çevrilip geri
alınmış** olmasında.

### Eskisi (ve bugün yine geçerli olan)

```csharp
public void OnHealthDepleted()
{
    if (State != UnitState.Alive)
    {
        return;
    }

    SetState(UnitState.Downed);
    remainingSeconds = downedWindowSeconds;
}
```

Yanındaki yorum, eksik kuralı adıyla söylüyordu: *"'işini bitirme' ayrı bir
kural (düşme canı) ve henüz yazılmadı; buraya sessizce koymak yerini de yok
ederdi."*

### Denenen ve GERİ ALINAN

Kapı `Downed → Dead` geçişini de yapacak biçimde genişletildi. Üç test aynı anda
kırmızıya döndü ve üçü de eski kuralı **bilerek** koruyordu; biri adıyla
`Downed_HealthDepletedAgain_DoesNotSkipTheWindow` diyordu. Aynı zamanda başka
bir test dosyası *"finishing it off is part of the design"* yazıyordu.

Yani proje iki şeyi birden söylüyordu: **bitirme tasarımın parçası, ama anlık
olmamalı.** Denenen değişiklik ikinciyi çiğniyordu.

### Yenisi

Bitirme ayrı bir kaynağa bağlandı:

```csharp
public const int DownedHealthDivisor = 2;   // ReviveHealthDivisor'un ikizi

if (lifecycle.State == UnitState.Downed)
{
    downedHealth = downedHealth ?? new Health(DownedHealthPoolFor(health.Max));
    downedHealth.TakeDamage(amount);

    if (!downedHealth.HasRemaining)
    {
        lifecycle.OnDownedHealthDepleted();
    }

    return;
}
```

Sayı keyfi değil: bir kaldırma canın yarısını geri veriyorsa, bitirme de o kadar
can istemeli. İki bölen ayrışsaydı "kaldırılabilir mi" ile "bitirilebilir mi"
sessizce farklı cevaplar verirdi.

**TEK CÜMLE:** Gerekçesi yazılı bir test bir karar kaydıdır; onu geçmenin tek
yolu dünyada NEyin değiştiğini söylemektir.

---

## 4 · Saldırının bedeli: sıra → bekleme süresi

### Eskisi

```csharp
if (!TurnRules.CanAct(attackerCombatant.Team, battle.Turn.Current))
{
    return AttackOutcome.RejectedActorCannotAct;
}
// ...
if (attacked) { battle.Turn.EndTurn(); }
```

### Kırılan şey

Oyuncu bu yapıda iki paleti de kendisi kullanıyor; sıra kapısı ona "tıklıyorum,
hiçbir şey olmuyor" olarak görünüyordu. Kapı kaldırıldı — **ve saldırının tek
bedeli oydu.** Sonuç: fareye ne kadar hızlı basılırsa o kadar hasar.

### Yenisi

Bedel, eşiği tanımda sayacı örnekte olan bir bekleme süresi oldu — projenin
`StructureProduction`'da zaten yerleşmiş deseni:

```csharp
public AttackProfile(int damage, int range, float cooldownSeconds = 0f)
```

Varsayılan sıfır olduğu için eski davranış birebir korunuyor; sayıyı Inspector
besliyor.

**TEK CÜMLE:** Bir kapıyı kaldırmadan önce o kapının başka NE ödettiğini sor.

---

## 5 · Boyut: ham çarpan → varlığın kendi ölçüsünden türetme

### Eskisi

```csharp
float scale = structureScale > 0.01f ? structureScale : 1.6f;
structureObject.transform.localScale = new Vector3(scale, scale, 1f);
```

Ve aynı nicelik dört yerde birden yazılıydı: ölü bir prefab'ın
`m_LocalScale`'inde, sahne alanında, üç kod sabitinde, ve can barının onu geri
alan `1f / parentScale` bölmesinde. Tür kimliğinin sahibi olan varlık dosyasında
ise **hiç yoktu**.

### Yenisi

```csharp
public static Vector3 LocalScaleFor(Sprite sprite, float sizeInCells, Vector3 cellSize)
```

Tasarımcı **kaç hücre** der; `localScale` sprite'ın kendi `rect`'i ve
`pixelsPerUnit`'inden hesaplanır. Böylece aynı sayı 16x16 ve 32x32 sanat için
aynı şeyi ifade eder — ham çarpan yazılsaydı 32x32 bir sprite geldiği gün sessizce
iki katı olurdu.

**TEK CÜMLE:** Görsel türetilmiş değer yazılmaz, hesaplanır; ve bir niceliğin
iki yazılabilir sahibi varsa hiç sahibi yoktur.

---

## 6 · Durum şeridi: sıranın sahibi → SEÇİMİN tarafı

Bu maddenin öğreticiliği, çevrilen şeyin bir MEKANİZMA değil bir CÜMLE
olmasında. Kod eksik değildi; söylediği şey doğru değildi.

### Eskisi

```csharp
private void OnTurnChanged(Team team, int turnNumber)
{
    if (turnLabel == null)
    {
        return;
    }

    bool player = team == Team.Player;
    turnLabel.text = player
        ? $"SIRA: SEN  ·  {turnNumber}. tur"
        : $"SIRA: DÜŞMAN  ·  {turnNumber}. tur";
    turnLabel.color = player ? playerColour : enemyColour;
}
```

### Kırılan şey

Tahta `TurnMode.FreeForAll` ile kuruluyor ve o kipte `TurnState.EndTurn` sırayı
hiç devretmiyor, tur numarası hiç artmıyor. Yani şerit, oyunun ilk karesinde
yazılan ve bir daha asla değişmeyen ÖLÜ bir sayı gösteriyordu — üstelik
oyuncuya var olmayan bir kural (sıra beklemek) öğretiyordu.

Buradaki tuzak, eksiğin bir MEKANİZMA gibi görünmesi: "tur ilerlemiyorsa tur
sistemini FreeForAll'a da bağla" demek kolaydı. Reddedildi. Sıra kapısı bir
önceki turda ölçülerek KALDIRILMIŞTI (madde 4) ve geri getirilecek olan şey,
oyuncunun tıklamasını yutan tam olarak o kapıydı.

### Yenisi

Yeni mekanizma yok; `TurnChanged` aboneliği de yerinde duruyor. Değişen tek şey,
sıranın konuşmadığı kipte etiketin BAŞKA bir doğruyu söylemesi:

```csharp
private void OnTurnChanged(Team team, int turnNumber)
{
    if (turnLabel == null || board.TurnMode == TurnMode.FreeForAll)
    {
        return;   // burada etiketin sahibi WriteSide
    }
    ...
}

private void WriteSide(Unit unit)
{
    if (turnLabel == null || board.TurnMode != TurnMode.FreeForAll) { return; }
    ...
    bool ours = team == Team.Player;
    turnLabel.text = ours ? "SENİN TAKIMIN" : "DÜŞMAN TAKIM";
    turnLabel.color = ours ? playerColour : enemyColour;
}
```

İki koşul birbirinin TERSİ (`==` ve `!=`) ve bu bilerek: aynı etikete iki sahip
yazsaydı hangisinin kazandığı karenin sırasına kalırdı. İki renk de yeni değil —
`playerColour` / `enemyColour` zaten sıranın tarafını boyuyordu, bugün seçimin
tarafını boyuyor.

**TEK CÜMLE:** Ölü bir sayının onarımı, onu canlandıracak bir mekanizma değil;
o yerin neyi söylemesi gerektiğini yeniden sormaktır.

---

## 7 · Kenar boşluğu: adı olan iki yalan → tek sahip

### Eskisi

```csharp
// Panellerin ekran kenarına olan payları.
private const float PaletteMargin = 12f;
private const float ProductionMargin = 14f;
```

Ve panelleri KURAN satırlar bu iki sabiti hiç okumuyordu:

```csharp
rect.anchoredPosition = new Vector2(12f, -StatusBarHeight * 0.5f);   // palet
rect.anchoredPosition = new Vector2(0f, 14f);                        // üretim
rect.anchoredPosition = new Vector2(-20f, 20f);                      // çöp kutusu
turnRect.offsetMin = new Vector2(16f, 0f);                           // durum şeridi
```

### Kırılan şey

Operatörün bildirdiği belirti şuydu: *"sağ alttaki sil düğmesi direkt köşeye
yapışık, düzgün değil."* Ölçüm daha kötüsünü gösterdi — düğmenin "SİL" etiketi
düğmenin ALTINDA yaşıyor (22 piksel) ve 20 piksellik payla ekranın DIŞINA
taşıyordu.

Asıl kusur o tek düğme değildi: dört ayrı kenar payı vardı, ikisinin adı vardı
ve o iki adı **yalnız kamera** okuyordu. Yani panel bir gün kaydırılsa kamera
eski payla çerçevelemeye devam eder, tahtanın kenarı sessizce panelin altına
girerdi. Adı olan bir sabit, kimse okumuyorsa bir belge değil bir yalandır.

### Yenisi

```csharp
private const float ScreenMargin = 24f;
private const float TrashLabelHeight = 22f;   // AYRI SAYI, ÇÜNKÜ AYRI ŞEY
```

Bütün paneller ve kamera aynı sayıyı okuyor. `TrashLabelHeight` ayrı duruyor ve
ayrılığı bir zevk değil bir ölçü: kenar payı bütün panellerin ORTAK payı, bu ise
tek bir düğmenin KENDİ taşmasıdır ve yalnız onun alt payına ekleniyor
(`ScreenMargin + TrashLabelHeight`). Tek sayıya indirilseydi ya etiket yine
taşardı ya da öteki üç panel gereksiz yere içeri kaçardı.

24 sayısı 12 ile 14'ün ortalaması değil: 1920x1080 referansında ekran
yüksekliğinin ~%2,2'si, yani dokunma hedefinin kenardan gerçekten ayrıldığı en
küçük değer.

**TEK CÜMLE:** Adı olup okunmayan bir sabit, bir sahip değil ikinci bir
gerçektir — ve iki gerçek ayrıştığı gün ikisi de sessiz kalır.

---

## 8 · Yerleştirme hayaleti: tahta dışında GİZLEMEK → KIRMIZI göstermek

Bu madde, gerekçesi yazılı ve o gün DOĞRU olan bir kararın, ikinci bir
belirsizlik ürettiği için çevrilmesi.

### Eskisi

```csharp
// ProductionDirector.DragTo
if (board.TryScreenPointToCell(screenPoint, out int x, out int y))
{
    board.SetPlacementGhost(true, x, y);
    return;
}

// Tahta dışına çıkan sürükleme önizlemeyi GİZLER, son geçerli
// hücrede BIRAKMAZ. Bırakılsaydı oyuncu, parmağını tahtanın dışında
// kaldırdığında oraya bir şey konacağını sanırdı.
board.SetPlacementGhost(false, 0, 0);
```

### Kırılan şey

Yorumun savunduğu tehlike gerçekti: hayaleti son geçerli hücrede bırakmak,
oyuncuya oraya bir şey konacağını söylerdi. Ama seçilen çare o tehlikeyi
önlerken **ikinci bir belirsizlik** üretti — hayalet tamamen kaybolduğu için
oyuncu elindeki şeyin hâlâ sürüklenip sürüklenmediğini de göremiyordu.

Operatörün cümlesi üçüncü bir yolu adıyla istedi: *"o unit grid'in dışındakileri
de hayalet kısmını görebilmeliyiz ama kırmızılı hâlinde."* Kırmızı hayalet iki
soruyu birden cevaplıyor — sürükleme sürüyor VE buraya konmaz.

Aynı boşluğun ikinci yarısı daha sessizdi: **dolu bir hücrenin üstünde hayalet
yeşil görünüyordu.** Bırakma zaten reddediliyordu (`DropUnit` `IsCellFree`
soruyor, `DropStructure` `PlaceStructure`'ın sonucunu okuyor), yani kural
doğruydu; yalnız oyuncu onu ancak parmağını kaldırdıktan SONRA öğreniyordu.

### Yenisi

```csharp
// ProductionDirector.DragTo — artik disariyi da soruyor
if (board.TryScreenPointToAnyCell(screenPoint, out int x, out int y))
{
    board.SetPlacementGhost(true, x, y);
    return;
}
```

```csharp
// BoardAdapter — "konabilir mi" sorusunun TEK sahibi
public PlacementPreview PreviewAt(int x, int y)
{
    if (!battle.IsInsideGrid(x, y))
    {
        return PlacementPreview.OutsideBoard;
    }

    return battle.TryGetUnit(x, y, out Unit _)
        ? PlacementPreview.CellOccupied
        : PlacementPreview.Placeable;
}

public bool IsCellFree(int x, int y)
{
    return PreviewAt(x, y) == PlacementPreview.Placeable;
}
```

**BIRAKMA DALI DEĞİŞMEDİ ve değişmemeliydi:** `DropAt` hâlâ
`TryScreenPointToCell` çağırıyor, yani tahta dışına bırakmak yine bir
vazgeçme. Değişen şey GÖRÜNEN, kural değil — ve iki üyenin ikiz kalması
(`...ToCell` reddeder, `...ToAnyCell` reddetmez) o ayrımın kendisi.

`IsCellFree`'nin kendi kuralını bırakıp önizlemeye delege etmesi bu maddenin
sessiz yarısı: iki kopya kalsaydı "boş hücre" tanımı değiştiği gün bırakma
kabul eder, hayalet kırmızı gösterirdi.

**TEK CÜMLE:** Bir çare, önlediği yanlış anlamanın yerine ikinci bir yanlış
anlama koyuyorsa henüz çare değildir.

---

## 9 · Kamera çerçevesi: kurulumda BİR KEZ → her oran değişiminde

### Eskisi

Çerçeveleme yalnız Editor aracında, aracın koştuğu andaki en boy oranıyla
yapılıyordu ve çalışma zamanında bir daha hiç sorulmuyordu:

```csharp
// SceneSetupTool.FrameCamera
float aspect = camera.aspect > 0.01f ? camera.aspect : 16f / 9f;
...
camera.orthographicSize = Mathf.Max(playSize, islandSize);
camera.transform.position = new Vector3(...);
```

### Kırılan şey

Operatör: *"haritanın gridini %50 yaptığımda düzgün bir şekilde ortalanmıyor."*
Game penceresi daraldığında kamera aynı yarım yüksekliği koruyor, yani YATAYDA
daha az dünya gösteriyor ve tahtanın kenarları dışarıda kalıyordu. Kusur bir
hesap hatası değil, bir **zamanlama** hatasıydı: doğru hesap yanlış anda,
yalnız bir kez yapılıyordu.

### Yenisi

Sayıların sahibi hâlâ araç; çalışma zamanında yapılan tek şey o çerçevenin
BAŞKA bir oranda da görünür kalmasını sağlamak:

```csharp
public static float FitHalfHeight(float homeHalfHeight, float homeAspect, float aspect)
{
    float framedHalfWidth = homeHalfHeight * homeAspect;
    return Mathf.Max(homeHalfHeight, framedHalfWidth / aspect);
}
```

Panel paylarının çalışma zamanına ikinci bir kopyasının inmesi REDDEDİLDİ ve
gerekçesi bu turun kendi dersiydi: `PaletteMargin` / `ProductionMargin`
yalanı da tam olarak aynı biçimde doğmuştu — adı olan ama kimsenin okumadığı
bir sayı.

**TEK CÜMLE:** Doğru hesabın yanlış anda yapılması da bir hatadır, ve onarımı
hesabı değiştirmek değil sahibini korurken zamanını düzeltmektir.

---

## 10 · Zemin: hücre başına bir GameObject → tek Tilemap

Bu maddenin öğreticiliği, çevrilen kararın **yanlış olmamasında**. Eski kod
kusursuzdu; kusurlu olan şey, doğruluğunun hangi sayıya bağlı olduğunun hiçbir
yerde yazmamasıydı.

### Eskisi

```csharp
for (int x = 0; x < battle.Width; x++)
{
    for (int y = 0; y < battle.Height; y++)
    {
        CreateCellVisual(x, y);
    }
}

private void CreateCellVisual(int x, int y)
{
    var cell = new GameObject($"Cell_{x}_{y}");
    cell.transform.SetParent(transform, worldPositionStays: false);
    cell.transform.position = CellCentre(x, y);

    var renderer = cell.AddComponent<SpriteRenderer>();
    renderer.sprite = PickTerrainSprite(x, y);
    renderer.sortingOrder = GroundSortingOrder;
}
```

Kenar halkası da aynı şekli taşıyordu (`CreateBorderVisual`).

### Kırılan şey

10x5'lik bir tahtada bu kod 50 nesne kuruyordu ve **hiçbir ölçüde kötü
değildi** — okunur, ayıklanabilir, her hücre Hierarchy'de adıyla duruyordu.

Operatör tahtayı 100x50 yaptı. Console'un kendi satırı ölçümdür:

```
[Board] built 100x50 = 5000 cells.
```

Halkayla birlikte (`borderThickness = 2`) **5616 GameObject**, 5616 Transform,
5616 SpriteRenderer. Üçü de her karede ayrı ayrı kültürleniyor ve sıralanıyor.
Aynı anda 0. katmanın döşeli deniz kuşağı Unity'nin tek-mesh sınırını aştı:

```
Cannot generate 9 slice most likely because the size is too big.
Requires 161872 vertices and 242808 indices
```

161872 / 4 = **40468 karo**; tavan 65535 köşe, yani 16383 karo. Unity mesh'i
kurmayı reddetti — deniz hiç çizilmedi.

### Yenisi

```csharp
Tilemap ground = EnsureTilemap(GroundMapName, GroundSortingOrder);
var bounds = new BoundsInt(0, 0, 0, battle.Width, battle.Height, 1);
var tiles = new TileBase[battle.Width * battle.Height];

for (int y = 0; y < battle.Height; y++)
{
    for (int x = 0; x < battle.Width; x++)
    {
        tiles[x + (y * battle.Width)] = TileFor(PickTerrainSprite(x, y));
    }
}

ground.SetTilesBlock(bounds, tiles);
```

5616 çizici yerine **iki** çizici (zemin + halka). Karo nesneleri görünüm
başına paylaşılıyor (`TileFor`, Flyweight) — hücre başına değil.

Deniz tarafında sayı değil **ölçek** düzeltildi: `TileScaleFor` karo sayısını
bir bütçenin altına indiriyor, kaplanan dünya alanı aynı kalıyor.

### REDDEDİLEN — ve neden hiçbiri pattern değildi

| aday | neden değil |
|---|---|
| **Object Pool** | Havuz, doğup ölen nesneler içindir. Hücreler `Awake`'te bir kez doğuyor ve hiç ölmüyor; havuzun çözdüğü baskı burada YOK. |
| **Flyweight (tek başına)** | Zaten vardı: `terrainSprites` paylaşılan bir dizi ve sprite'lar hücreler arasında ortak. Nesne sayısını sprite paylaşımı azaltmıyor — azaltan şey ÇİZİCİ sayısı. |
| **Görünen hücreleri kültürlemek (culling)** | Elle yazılmış bir Tilemap olurdu; Unity'nin `TilemapRenderer`'ı bunu zaten öbek öbek yapıyor. |
| **Tahta boyutunu kısıtlamak** | Operatörün oyun kararını koda kısıtlatmak. Sayının sahibi tasarımcı. |

**PATTERN GEREKMEDİ VE BU MADDENİN ASIL DERSİ BU.** Aranan şey bir GoF deseni
değil, **motorun kendi sahibiydi**. Bir baskıya pattern aramadan önce sorulacak
soru şu: *bu işi motor zaten yapıyor mu?*

### Ne zaman ESKİSİ kazanırdı

Hücrelerin TEK TEK canlanması gerektiği gün: her hücrenin kendi animasyonu,
kendi çarpıştırıcısı ya da kendi tıklama alanı olsaydı. Bu tahtada hiçbiri yok
— tıklamayı `Grid` matematiği çözüyor ve hücreler doğduktan sonra hiç
değişmiyor. O gün gelirse doğru cevap "Tilemap'ten geri dön" değil, "canlanan
hücreler için AYRI bir katman aç" olur.

**TEK CÜMLE:** Bir kararın doğruluğu bir sayıya bağlıysa, o sayı kararın
yanında yazılı olmalıdır; yazılı değilse karar doğru değil, yalnızca henüz
yanlış değildir.

---

## 11 · Konum sorusu: her çağrıda tarama → ters dizin

Bu maddenin öğreticiliği, kusuru doğuran şeyin **tek bir karar değil, iki
kararın çarpımı** olmasında. İkisi de ayrı ayrı doğruydu ve ikisinin
incelemesinin de ötekine bakmak için bir sebebi yoktu.

### Eskisi

```csharp
// Battle.TryGetPosition
for (int cellX = 0; cellX < board.Width; cellX++)
{
    for (int cellY = 0; cellY < board.Height; cellY++)
    {
        if (board.TryGetUnit(cellX, cellY, out Unit standing)
            && ReferenceEquals(standing, unit))
        {
            x = cellX;
            y = cellY;
            return true;
        }
    }
}
```

### Kırılan şey

10x5'lik bir tahtada çağrı başına en fazla 50 hücre yoklanıyordu ve vuruş tek
seferlikti. İki şey ayrı ayrı değişti:

| değişen | tek başına sonucu |
|---|---|
| tahta 100x50 oldu | çağrı başına 50 → **5000** yoklama |
| kalıcı saldırı emri doğdu | emir başına kare başına **3** çağrı (saldıran · hedef · vuruş) |

İkisinden yalnız biri olsaydı eski kod hâlâ doğruydu. Çarpımları:
on emir × 3 çağrı × 5000 hücre × 60 kare = **saniyede ~9 milyon yoklama**.

Bir önceki devir belgesi bu günü **adıyla** öngörmüştü: *"Kalıcı emirler her
karede konum soracağı için bu, İŞ-1 ile birlikte gerçekten ısınacak yer
burasıdır... o gün önce `TryGetPosition`'ın sözlüğe alınması denenir, Burst
değil."* Tetikleyici ateşledi ve yazılı karar uygulandı — yeni bir tartışma
açılmadı.

### Yenisi

Soru, cevabı O(1) verebilecek tek yere taşındı: hücreleri YAZAN tipe.

```csharp
// UnitGrid — TEK yazma noktasi, iki gercek ayrisamaz
private void WriteCell(int x, int y, Unit unit)
{
    Unit previous = cells[x, y];
    if (previous != null)
    {
        occupied.Remove(previous);
    }

    cells[x, y] = unit;

    if (unit != null)
    {
        occupied[unit] = (y * Width) + x;
    }
}
```

`Battle.TryGetPosition` silinmedi, **delege edildi**: çağıranları savaşın
defterini tanıyor, tahtanın iç tipini değil. Sorunun sahibi değişti,
sözleşmesi değişmedi — `(-1,-1)` sözü dahil.

### İki karar, ikisi de gerekçeli

**Dizin neden `Battle`'da değil `UnitGrid`'de:** `Battle`'da tutulsaydı,
mutasyonları görmeyen bir tipte yaşar ve her yazmada elle güncellenmesi
gerekirdi. Dördüncü bir yazma üyesi eklendiği gün onu unutmak **sessiz** bir
hata olurdu — birim ekranda doğru yerde durur, kural katmanı onu başka hücrede
sanırdı.

**Değer neden `int`:** `GridStrategy.Core` motoru görmüyor
(`noEngineReferences: true`), yani `Vector2Int` orada yok. İki alanlık bir
struct açmak yerine hücre `y * Width + x` ile paketleniyor ve paketleme tipin
İÇİNDE kalıyor.

### Testler hızı değil SENKRONU ölçüyor

Sekiz yeni iddia — taşıma, kaldırma, üstüne yazma, kendi hücresine taşıma,
başka birimin üstüne taşıma — hepsi tek bir soruyu soruyor: dizi ile dizin
hâlâ aynı şeyi mi söylüyor. Hız ölçen bir test yazılmadı ve yazılmamalıydı:
EditMode'da ölçülen bir süre, hedef donanımda hiçbir şeyi kanıtlamaz.

**TEK CÜMLE:** İki ayrı ayrı doğru kararın çarpımı yanlış olabilir, ve o
çarpımı hiçbir inceleme görmez çünkü her inceleme yalnız kendi kararına bakar.

---

## 12 · Sol tuşun anlamı: BASMA karesi → BIRAKMA karesi

Bu maddenin öğreticiliği, çevrilen şeyin bir tip ya da bir sayı değil, bir
ZAMANLAMA olmasında: aynı tıklama, aynı yerde, bir kare sonra.

### Eskisi

```csharp
// BoardAdapter.Update
// "Down" = SADECE basıldığı karede true; GetMouseButton (Down'suz)
// basılı olduğu her karede true olurdu. Tek tıklama istiyoruz.
if (!Input.GetMouseButtonDown(0))
{
    return;
}

if (PointerIsOverUi())
{
    return;
}

HandleClick();
```

Ve kamera rig'inin başında, bir önceki turda yazılmış şu blok duruyordu:

```csharp
// ██ SAĞ TUŞ, SOL TUŞ DEĞİL — VE BU ÖLÇÜLMÜŞ BİR REDDETME ██
// Sola üçüncü bir anlam vermek, seçimi basma karesinden BIRAKMA karesine
// ertelemeyi gerektirir; o değişiklik bugünkü tıklama testlerinin
// tamamını ve yerleştirme jestini birden etkiler.
```

### Kırılan şey

O reddetme **doğruydu ve bedeli doğru saymıştı** — yalnız bedelin ödenmeye
değer olup olmadığını operatör henüz söylememişti. Söyledi: *"tıklı basıp
haritada gezinebilmem lazımdı, Clash of Clans gibimsi."*

Basma karesinde karar veren bir seçim ile sol sürükleme birbirinin karşıtıdır:
sürükleme başlamadan önce zaten bir şey seçilmiş olur. Bu, ikinci maddedeki
"iki kural birbirinin karşıtıysa biri yanlış yerdedir" ile aynı biçim, bu kez
bir zaman ekseninde.

### Yenisi

Karar bir eşiğe bağlandı ve tıklama bırakışa ertelendi:

```
BASIS  ->  aday jest, hicbir sey olmaz
   esigi GECERSE   ->  KAYDIRMA, birakista tiklama YOK
   esigi GECMEZSE  ->  TIKLAMA, BIRAKMA karesinde
```

Yeni bir jest tipi YAZILMADI: `PointerGesture` bu ayrımı zaten yapıyordu ve
yalnız yerleştirmeye hizmet ediyordu. `BoardPointerArbiter` onu sarıp bir
EYLEME çeviriyor — saf C#, motor çağrısı yok, EditMode'da 19 testle sınanıyor.

Rig'in yukarıdaki reddetme bloğu **silindi**, çünkü gerekçesi artık dünyayı
yanlış tarif ediyor. Yerine ters yönde bir kilit kondu: rig sol tuşu okuyamaz.

```csharp
[Min(1)] private int panButton = 1;   // Inspector kelepcesi

if (panButton < 1)                    // diskteki eski 0'i kesen ikinci kol
{
    panButton = 1;
}
```

**KİLİT İKİ KOLLU VE BU BİR ÜSLUP DEĞİL:** `[Min(1)]` yalnız Inspector'ı
kelepçeliyor, sahne dosyasında kalmış eski bir `0` onu hiç görmez.

**TEK CÜMLE:** Ölçülmüş bir reddetme, ölçtüğü bedelin ödenmeye değer olduğu
gün geçersizleşir — ve o gün silinmesi gereken şey kod değil, gerekçenin
kendisidir.

---

## Bu belgeye ne eklenir, ne eklenmez

| eklenir | eklenmez |
|---|---|
| Bir kararın ÇEVRİLDİĞİ an ve çeviren ölçüm | yeni bir üye eklenmesi |
| Eski kodun kendisi (kısa, okunur bir parça) | dosya taşıma, yeniden adlandırma |
| Denenip GERİ ALINAN bir değişiklik ve neden geri alındığı | yazım düzeltmesi |
| Bir pattern'in NEREDE bittiği | pattern'in ne olduğunun tarifi |

Satır numarası verilmez. Bu belge kodun yanında yaşar ve satır numaraları her
turda kayar; atıf tip ve üye adıyla yapılır.
