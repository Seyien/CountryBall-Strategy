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

### Yenisi (planlanan şekil)

```csharp
public interface IUnitOrder
{
    OrderProgress Tick(float deltaSeconds);   // Devam / Bitti / Iptal
}

private readonly Dictionary<Unit, IUnitOrder> orders;
```

Kip makinesi GİRDİNİN anlamını sahiplenmeye devam eder; emirler ondan bağımsız
yaşar ve birim başına birdir.

**TEK CÜMLE:** Bir pattern'in sınırı, sahiplendiği şeyin çokluğudur — tekil bir
sahip çoğul bir kavramı taşıyamaz.

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

## Bu belgeye ne eklenir, ne eklenmez

| eklenir | eklenmez |
|---|---|
| Bir kararın ÇEVRİLDİĞİ an ve çeviren ölçüm | yeni bir üye eklenmesi |
| Eski kodun kendisi (kısa, okunur bir parça) | dosya taşıma, yeniden adlandırma |
| Denenip GERİ ALINAN bir değişiklik ve neden geri alındığı | yazım düzeltmesi |
| Bir pattern'in NEREDE bittiği | pattern'in ne olduğunun tarifi |

Satır numarası verilmez. Bu belge kodun yanında yaşar ve satır numaraları her
turda kayar; atıf tip ve üye adıyla yapılır.
