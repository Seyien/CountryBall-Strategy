# Motorun çağrı döngüsü — kimse çağırmadığı hâlde koşan metotlar

> **NEREDE GEÇİYOR** — *bu mekanizmanın kat ettiği kaynak dosyalar, akış sırasıyla:*
> `Assets/Game/Unity/BoardAdapter.cs` → `Assets/Game/Unity/UnitView.cs`
> → ***duvar*** → `Assets/Game/Battle/Battle.cs`
> ayrıca motor ayarı: `ProjectSettings/EditorSettings.asset` (`m_EnterPlayModeOptionsEnabled`)
>
> **NE ZAMAN OKU** — *hangi soruyu sorduğunda ya da hangi değişikliğe giriştiğinde:*
> `Assets/Game/Unity/` altına üçüncü bir `MonoBehaviour` eklemeden önce, "bu satır
> neden `Start`'ta değil de `Awake`'te" diye sorduğunda, ya da ilk
> `StartCoroutine`'i yazmaya kalktığında.

**BURAYA KODDAN GELDİYSEN** — aşağıdaki üyelerin **yorumunda** bu belgeye bir
`DERİN ANLATIM:` işaretçisi var. Yol: `Ctrl+P` → dosya adının ayırt edici
parçasını yaz → `Ctrl+F` ile **üye adını** ara. ***Satır numarası bilerek
yazılmıyor: satır kayar, üye adı kaymaz.***

| dosya | üye | koddan işaretçi |
|---|---|---|
| `Assets/Game/Unity/BoardAdapter.cs` | `Awake` | ✓ |
| `Assets/Game/Unity/UnitView.cs` | `Awake` | ✓ |
| `Assets/Game/Core/PointerGesture.cs` | `Reset` — ██ Unity mesaj ADI taşıyor, mesaj DEĞİL ██ | ✓ |
| `Assets/Game/Core/Combat/UnitLifecycle.cs` | `Tick` — ██ motorun `Update`'i DEĞİL, elle çevrilen sayaç ██ | ✓ |
| `Assets/Tests/EditMode/Combat/UnitLifecycleTests.cs` | `IEnumerator`'un reddedildiği blok | ✓ |
| `Assets/Game/Unity/BoardAdapter.cs` | `OnEnable` · `OnDisable` · `Update` | ██ HENÜZ YOK ██ |

██ **"HENÜZ YOK" ne demek:** o üye burada gerçekten anlatılıyor, ama **kodun
yorumunda buraya geri getiren bir satır yok**. Üçünün de yorumunda başka bir
belgeye işaretçi var (`OnEnable`/`OnDisable` → `dil/06`, `Update` → `konular/07`);
yani işaretçi eksikliği değil, ██ bu belgeye giden işaretçinin yokluğu ██.
Listeden silmedim; silmek boşluğu görünmez kılardı. ██

---

## AD ÇAKIŞMASI — önce bunu oku

Bu ağaçta **"yaşam döngüsü" adını taşıyan iki ayrı şey** var ve birbirlerine
hiç dokunmuyorlar:

```
   [05-yasam-dongusu.md]              [BU DOSYA — 08]
   ─────────────────────              ────────────────
   OYUNUN durum makinesi              MOTORUN çağrı döngüsü
   Alive → Downed → Dead              Awake → OnEnable → Update → ...
   sahibi : UnitLifecycle             sahibi : UnityEngine
   yeri   : Assets/Game/Core/         yeri   : Assets/Game/Unity/
   zamanı : saniye (Tick'e verilen)   zamanı : kare (motorun saydığı)
   Unity  : >> HİÇ GEÇMEZ <<          Unity  : >> KONUNUN TAMAMI <<
```

Ölçüsü tek satır: `GridStrategy.Combat.asmdef` içinde `noEngineReferences: true`
yazar, yani `UnitLifecycle`'ın yaşadığı assembly `UnityEngine.dll`'i **hiç
görmez**. `BoardAdapter.cs` ise beşinci satırında `using UnityEngine;` yazar.

İkisi tek bir noktada buluşur:

```
BoardAdapter.cs:931   battle.Tick(Time.deltaTime);
```

Motorun karesi orada saniyeye çevrilir ve duvarın
öte yanına *yalnızca bir sayı olarak* geçer.

Oyunun durum makinesi için: [05-yasam-dongusu.md](05-yasam-dongusu.md).
Bu dosya motorun kendi çağrı sırasını anlatıyor.

---

## Sürüm damgası

```
ProjectSettings/ProjectVersion.txt
    m_EditorVersion             : 2021.3.45f2
    m_EditorVersionWithRevision : 2021.3.45f2 (88f88f591b2e)

Doğrulama tarihi : 2026-08-23
Doğrulama şekli  : repodaki dosyalara + yerel Editor kurulumuna karşı
                   C:/Program Files/Unity/Hub/Editor/2021.3.45f2/Editor/Data/
```

***Sürüme bağlanmamış bir motor iddiası kusurdur.*** Buradaki her "Unity
şunu yapar" cümlesi 2021.3.45f2 içindir; Unity 6 ile ayrıldığı yerler adıyla
işaretli.

Ve bir sınır: aşağıdaki iddiaların hiçbiri bu turda **Editor koşturularak**
ölçülmedi — bu bir belge turu. Bu yüzden her motor iddiasının yanında
**okuyucunun koşturabileceği ölçü** yazılı. İddia bir etikettir; ölçü onu kanıta
çevirir.

---

## Sahne

Play'e basıyorsun. Tahta beliriyor, üstünde iki asker duruyor. Sen hiçbir şey
yapmadan düşmüş bir askerin sayacı işlemeye başlıyor.

Şimdi `BoardAdapter.cs`'i aç ve **`Awake()` metodunu kimin çağırdığını ara.**
Bulamayacaksın. `SpawnUnit`'i `Awake` çağırıyor, `HandleClick`'i `Update`
çağırıyor — ama `Awake`'i ve `Update`'i bu projede **hiçbir satır çağırmıyor.**

```
Assets/ altında `Awake()` yazan ÇAĞRI sayısı   :  0
Assets/ altında `Update()` yazan ÇAĞRI sayısı  :  0
Yine de her ikisi de koşuyor.
```

Bu dosya o boşluğu anlatıyor: çağıran kim, adı nasıl buluyor, hangi sırayla
çağırıyor, ve bunun **C# `event`'iyle hiçbir ilgisi olmadığı**.

---

## Karakterler

```
╔═ UnityEngine (motor) ═════════════════════════════════════════╗
║  İşi   : kareyi saymak, adı bilinen metotları çağırmak        ║
║  Bilir : hangi bileşen canlı, hangi GameObject etkin, hangi   ║
║          tip hangi mesaj adını TANIMLAMIŞ                     ║
║  BİLMEZ: >> SENİN OYUNUNU << · Battle'ı · UnitState'i ·       ║
║          metodun ne yaptığını                                 ║
╚═══════════════════════════════════════════════════════════════╝
╔═ BoardAdapter : MonoBehaviour ════════════════════════════════╗
║  İşi   : motor tarafı ile motorsuz çekirdek arasında çeviri   ║
║  Bilir : kare kavramını, Input'u, Camera'yı, Time'ı           ║
║  BİLMEZ: kendi metotlarını kimin çağırdığını — gerek de yok   ║
║  ÖLÇÜ  : 4 geri çağrı tanımlı, 0'ı bu repodan çağrılıyor      ║
╚═══════════════════════════════════════════════════════════════╝
╔═ UnitView : MonoBehaviour ════════════════════════════════════╗
║  İşi   : bir birimin ekrandaki karşılığı                      ║
║  BİLMEZ: >> KARE DİYE BİR ŞEYİ << — `Update`'i YOK, `Time` bu ║
║          dosyada hiç geçmez; yalnız SÖYLENDİĞİNDE iş yapar    ║
║  ÖLÇÜ  : 1 geri çağrı tanımlı (Awake), o kadar                ║
╚═══════════════════════════════════════════════════════════════╝
╔═ Battle (düz C# sınıfı) ══════════════════════════════════════╗
║  İşi   : savaşın kurallarını yürütmek                         ║
║  BİLMEZ: >> KAREYİ << · >> TIKLAMAYI << · Awake diye bir      ║
║          kavramı · MonoBehaviour'ın var olduğunu              ║
║  ÖLÇÜ  : asmdef'inde `noEngineReferences: true` — o dosyada   ║
║          `MonoBehaviour` kelimesi DERLENMEZ                   ║
╚═══════════════════════════════════════════════════════════════╝
╔═ PointerGesture (düz C# sınıfı, Core) ════════════════════════╗
║  İşi   : tıklama ile sürüklemeyi ayırmak                      ║
║  BİLMEZ: >> ADININ MOTOR İÇİN BİR ANLAMI OLDUĞUNU << — bir    ║
║          `Reset()` metodu var, `Reset` gerçekten bir Unity    ║
║          mesaj adı, ve bu tipte HİÇBİR anlamı yok             ║
╚═══════════════════════════════════════════════════════════════╝
```

Sonuncusu bu dosyanın en öğretici karakteri; onu birinci durakta açıyoruz.

### Kutudan gerçek satıra — her kutunun kod karşılığı

Kutular **rolü** anlatıyor; bu bölüm o rolün **hangi satırda** durduğunu
gösteriyor. Satır numarası bilerek yazılmıyor: satır kayar, üye adı kaymaz.

**Birinci kutu ötekilerden farklı: motorun bu depoda bir *tanım satırı yok*.
Onun için verilen yer bir tanım değil, *etkinin gözlendiği* yerdir.**

**`UnityEngine (motor)` bu projede** — ***TANIM DEĞİL, ETKİ*** · gözlem yeri:
`Assets/Game/Unity/BoardAdapter.cs` → `AdvanceBattleTime`

```csharp
private void AdvanceBattleTime()
{
    battle.Tick(Time.deltaTime);

    if (battle.RemoveReadyForCleanup(cleanupBuffer) == 0)
    {
        return;
    }
```

Kutudaki «kareyi saymak, adı bilinen metotları çağırmak» satırının bu depodaki
karşılığı **iki gözlemdir, ikisi de bir tanım değil**. Birincisi: bu metodu
çağıran tek satır `Update`'in içinde, `Update`'i çağıran satır ise bu depoda
**hiç yok** (sayı bu dosyanın en başında). İkincisi: `Time.deltaTime` bir sayı
**okumuyor**, motorun kare başlamadan önce yazdığı bir değeri **teslim alıyor** —
`Time` tipinin gövdesi `Assets/` altında bulunmaz, `UnityEngine`'in içindedir.
Yani motorun "kareyi saydığı" bu depoda görünmez; görünen tek şey, sayının
`battle.Tick(...)` çağrısına **girdiği andır**. Çağrının motorun kare planında
hangi yuvada durduğu ayrı bir dosyanın ölçüsü:
[`../../ogrenme/08-unity-altyapisi.md` → 3.2](../../ogrenme/08-unity-altyapisi.md#32-oyun-dongusu-kim-cagiriyor-sira-kimin-karari).

**`BoardAdapter` bu projede** — `Assets/Game/Unity/BoardAdapter.cs` → `Awake`

```csharp
unityGrid = GetComponent<Grid>();
battle = new Battle(width, height);
```

Kutudaki «motor tarafı ile motorsuz çekirdek arasında çeviri» satırının karşılığı
bu **iki komşu satırdır**: birincisi motora soruyor (`Grid` bir `UnityEngine`
tipi), ikincisi assembly'sinde `noEngineReferences: true` yazan bir katmanın
tipini kuruyor. Sınır iki satır arasında geçiyor ve iki satır aynı metotta.
Kutunun «4 geri çağrı tanımlı, 0'ı bu repodan çağrılıyor» ölçüsünün karşılığı da
bu dosyadaki dört imzadır — `Awake`, `OnEnable`, `OnDisable`, `Update` — ve
dördü de `private`; erişilemez olmaları motoru durdurmuyor.

**`UnitView` bu projede** — `Assets/Game/Unity/UnitView.cs` → `Awake`

```csharp
private void Awake()
{
    // SIRA BİR KARARDIR: normalizasyon, selectionOverlay kontrolünün
    // ÜSTÜNDE. Altına konsaydı atanmamış BİR alan, ilgisiz İKİ şeyi
    // birden bozardı — aşağıdaki erken çıkış bu satırı da atlar ve
    // prefab'da ters/soluk kaydedilmiş gövde öyle kalırdı. Doğan her
    // birim AYAKTA başlamak ZORUNDA. → UnitView.md#awake
    SetState(UnitState.Alive);
```

Kutudaki «1 geri çağrı tanımlı (Awake), o kadar» ölçüsünün karşılığı bu imzadır;
dosyada motorun adına bakarak bulabileceği ikinci bir metot yok. «***KARE DİYE
BİR ŞEYİ*** — `Update`'i YOK, `Time` bu dosyada hiç geçmez» satırının karşılığı
ise bu bloğun **bittiği yerdir**: `Awake` bir kez koşuyor ve tipin geri kalanı
(`SetSelected`, `SetState`) `public` — yani söylendiğinde çalışıyor, kare
başına değil. `Time` sözcüğü dosyada bir kez geçiyor ve o da bu olguyu yazan
yorum satırının kendisi.

**`Battle` bu projede** — `Assets/Game/Battle/Battle.cs` → `Tick`

```csharp
public void Tick(float deltaSeconds)
{
    // Sözlük üzerinde DOĞRUDAN foreach: Dictionary<,>.Enumerator bir
    // struct'tır ve burada bir arayüz ardında saklanmadığı için
    // kutulanmaz. Aynı döngü `IEnumerable` üzerinden dönseydi kare
    // başına bir tahsis üretirdi.
    foreach (KeyValuePair<Unit, Combatant> pair in combatants)
    {
        pair.Value.Tick(deltaSeconds);
    }
```

Kutudaki «***KAREYİ*** · ***TIKLAMAYI*** · Awake diye bir kavramı» satırının
karşılığı **parametrenin tipidir**: kare, bu imzayı geçerken sıradan bir `float`a
dönüşüyor ve adı bile "kare" demiyor. Kutunun ölçü satırındaki dosya
`Assets/Game/Battle/GridStrategy.Battle.asmdef`; içinde `"noEngineReferences": true`
yazıyor, yani buraya `Time.deltaTime` yazan biri bir çalışma anı hatası değil bir
**derleme** hatası alır. Yukarıdaki yorumun "kare başına bir tahsis" cümlesi de
bu sınırın kanıtı: tip kareyi göremiyor ama kare başına çağrıldığını **bilerek**
yazılmış.

**`PointerGesture` bu projede** — `Assets/Game/Core/PointerGesture.cs` → `Reset`

```csharp
public void Reset()
{
    Phase = PointerPhase.Idle;
    pressX = 0f;
    pressY = 0f;
}
```

Kutudaki «***ADININ MOTOR İÇİN BİR ANLAMI OLDUĞUNU***» satırının karşılığı bu
metot **ile** onu saran tip bildiriminin birlikte okunmasıdır:
`public sealed class PointerGesture` — devamında `: MonoBehaviour` yok. Motor
`Reset` adını yalnızca `MonoBehaviour`'dan türeyen tiplerde arıyor; bu tipte ad
yalnızca bir addır ve metodu çalıştıran tek şey onu yazan çağrıdır —
`BoardAdapter` içinde üç yerde: `TryEnterPlacementMode`, `UpdatePlacement`,
`CancelPlacement`. Aynı ad, iki farklı tipte, iki farklı anlam — ve farkı
doğuran şey adın kendisi değil, **taban tipin varlığı**.

---

## Birinci durak: ***`Awake` bir `event` DEĞİLDİR***

Konunun en pahalı yanlış modeli şöyle kurulur: "`Awake` bir olay, Unity onu
tetikliyor, ben de dinliyorum." Cümle kulağa doğru geliyor ve **tek kelimesi
bile doğru değil.**

| C# `event` | Unity mesaj geri çağrısı |
|---|---|
| `+=` / `-=` ile **abone olunur** | **ada göre BULUNUR**, abone olunmaz |
| çağrı listesi tutar — **birden çok** dinleyici | **tek metot, tek sınıf** — liste yok |
| her abone bir `Target` + `Method` **nesnesidir** | ortada nesne yok; **ad** çözülür |
| dinleyicinin **erişilebilir** olması gerekir | `private` olabilir, motor **yine çağırır** |
| **sen yayınlarsın** (`?.Invoke(...)`) | **motor yayınlar**; senin elinde tetik yok |
| aboneliği bırakmayı unutursan **sızıntı** olur | unutulacak abonelik **yoktur** |
| `-=` ile istediğin an **bırakabilirsin** | metodu **silmeden** bırakamazsın |

Delege mekanizmasının kendisi — `Action<…>`'ın nasıl okunduğu, `?.Invoke`'un
neden gerektiği, kapanış kimliğinin `-=`'yi nasıl bozduğu — burada **tekrar
edilmiyor**; sahibi
[../dil/04-delege-olay-ve-kapanis.md](../dil/04-delege-olay-ve-kapanis.md).
Derleyici tarafındaki hâli — `+=`'nin neye dönüştüğü, `Target` + `Method`
ikilisi — için
[../dil/06-delege-arka-taraf.md](../dil/06-delege-arka-taraf.md).
**O dosya aynı sınırı *öteki yönden* çiziyor**: oradaki
[altıncı durak](../dil/06-delege-arka-taraf.md#altinci-durak-event-ile-unity-mesaj-geri-cagrilari-ayni-sey-degil)
delegeden bakıp "bu bir Unity mesajı değil" diyor; buradaki tablo motordan bakıp
"bu bir `event` değil" diyor. Sıra ve sahiplik bu dosyanın işi, delegenin içi
onun.

İkisini aynı projede, arka arkaya iki satırda görebilirsin:

```
BoardAdapter.cs:351   battle.UnitStateChanged += OnUnitStateChanged;
                      ▲ C# event
BoardAdapter.cs:349   private void OnEnable()
                      ▲ Unity mesajı
                      ▲                     ▲
                      │                     └─ bu metodu MOTOR bulur
                      └─ bu satırı SEN yazarsın, motorun haberi yoktur
```

Olayın kendi zinciri: [01-olay-zinciri.md](01-olay-zinciri.md).

> **▶ ARA DURAK:** [../dil/04-delege-olay-ve-kapanis.md](../dil/04-delege-olay-ve-kapanis.md#ikinci-durak-event-ile-duz-action-alani-farki)
> **NEDEN:** yukarıdaki yedi satırlık tablonun **sol sütunu** tanımsız. "`event`
> DEĞİLDİR" cümlesi ancak `event`'in ne **olduğu** bilinirse bir şey söylüyor —
> ve bu dosya onu tanımlamıyor, bilerek tekrar etmiyor. Sol sütunun beş satırı
> (`+=`, çağrı listesi, `Target`+`Method`, `?.Invoke`, sızıntı) orada kuruluyor.
> **DÖNÜŞ:** bu dosyanın [«`private void Awake()` — motor bunu nasıl buluyor»](#private-void-awake-motor-bunu-nasil-buluyor) bölümü

> **⌨ KODU AÇ:** `Assets/Game/Unity/BoardAdapter.cs` → `OnEnable`
> **BAK:** metodun **kendisi** bir Unity mesajı (motor onu adıyla bulur),
> **gövdesindeki** satır ise bir C# `event` aboneliği (`+=` sen yazarsın). İki
> mekanizma iki satır arayla yan yana duruyor — ***tablonun iki sütunu tek
> ekranda***.
> **DÖNÜŞ:** bu dosyanın «Birinci durak: `Awake` bir `event` DEĞİLDİR» bölümü

### `private void Awake()` — motor bunu nasıl buluyor

Cevap **her karede yansımayla arama yapmak değil**; ada göre bir kez çözüp
önbelleğe almak. Sırayı ayır:

```
① SENİN YAZDIĞIN ŞEY
   `private void Awake()` → BoardAdapter tipinin üstünde SIRADAN bir
                            örnek metodu
   sahip: `C# dili` — derleyici bu ada Unity anlamı YÜKLEMEZ
        │  ortada override yok, arayüz yok, çağıran satır yok
        v
② MOTORUN BİLDİĞİ ŞEY
   UnityEngine'in mesaj kataloğu: "Awake", "OnEnable", "Update", ...
   adlarının ne anlama geldiği ve NE ZAMAN çağrılacağı
   sahip: `UnityEngine API` — yerel (native) taraf
        │  tip ilk kez kullanıldığında taranır, sonuç ÖNBELLEĞE alınır
        v
③ EŞLEŞME → motor o metodu, o örneğin üstünde çağırır
   >> ARAMA BİTTİ << isim çözüldü, artık her karede aranmaz
```

Üç koşul var ve **hepsi birden** gerekiyor:

```
UYGUNLUK    : tip MonoBehaviour'dan türemeli       ← yoksa hiç bakılmaz
BİLDİRİM    : belgelenmiş adı/imzayı yazmalısın    ← "Awake2" işe yaramaz
CANLI ALICI : gerçek bir bileşen ÖRNEĞİ olmalı     ← tipin varlığı yetmez
```

***`private` motoru durdurmaz.*** Erişim belirteci C# tarafının kuralıdır;
motorun çağrı yolu C# çağrı yolu değildir. Bu projedeki **beş geri çağrının
beşi de `private`** ve beşi de koşar. `public void Awake()` yazsan hiçbir şey
kazanmaz, yalnızca dışarıya boş bir kapı açarsın.

**İddia:** `private` bir `Awake` çağrılır. **Ölçüsü:** `BoardAdapter.Awake`
(satır 225) `private` ve içinde `battle = new Battle(width, height)` var.
Çağrılmasaydı `Update`'in ilk satırı `battle.Tick(...)` daha ilk karede
`NullReferenceException` atardı. Oyun açılıyorsa metot çağrılmıştır — bu
koşturulacak bir deney değil, hâlihazırda koşan bir kanıt.

### KAPSAM: kural **ada** özeldir, "metot yazmaya" değil

Karşı örnek aynı repoda ve bilerek orada:

```
Assets/Game/Core/PointerGesture.cs:281
    public void Reset()
```

`Reset` **gerçek bir Unity mesaj adıdır** — Editor'de bileşen menüsünden "Reset"
seçildiğinde çağrılan mesajın adı. Burada hiçbir motor anlamı yoktur, çünkü ①.
koşul düşer:

```
PointerGesture → MonoBehaviour'dan TÜREMİYOR
               → asmdef'i GridStrategy.Core, noEngineReferences: true
               → o assembly UnityEngine.dll'i hiç GÖRMEZ
               → motor bu tipi hiç TARAMAZ
```

Sonuç: `Reset()` yalnızca `TryEnterPlacementMode` onu çağırdığı için koşar:

```
BoardAdapter.cs:481   gesture.Reset();
```

**Ölçüsü:** o tek satırı sil, metot ölü koda döner. Aynı deneyi
`BoardAdapter.Awake` için **kuramazsın** — silinecek bir çağrı yok.

### İŞ BÖLÜMÜ: adın iki sahibi örtüşmez, bölüşür

```
`C# dili`          : metodun VARLIĞI, gövdesi, erişim belirteci, imzası
`UnityEngine API`  : o adın ANLAMI, çağrılma ZAMANI, çağrı SIRASI

adı boz (Awake → Baslat) → derleyici TEK KELİME ETMEZ; kod derlenir,
                           oyun açılır, `battle` sonsuza dek null kalır
tipi boz (: MonoBehaviour → derleyici GetComponent'te PATLAR,
          satırını sil)     Play'e hiç basılamaz
```

***İlk hata sessiz, ikincisi gürültülü — tehlikeli olan sessiz olandır.***

---

## İkinci durak: çağrı sırası — sahipleriyle, ezberle değil

```
bileşen örneği yüklenebilir/etkin hâle gelir
        v
   Awake        ── bileşen örneği başına BİR kez
        v
   OnEnable     ── HER etkinleşmede TEKRAR
        v
   Start        ── bir kez, bu örneğin İLK Update'inden hemen önce
        ├──────────── KARE DÖNGÜSÜ ────────────┐
        v                                      │
   Update       ── çizilen kare başına 0 veya 1│
   LateUpdate   ── bütün Update'lerden SONRA   │
        └───────────────────────────────────────┘
        v
   OnDisable    ── HER etkisizleşmede TEKRAR
        v
   OnDestroy    ── nesne yok edildiğinde
```

Bu bir **öğrenme haritasıdır**, motorun tam kare planı değil. Fizik, çizim,
canlandırma, sahne olayları ve platform geri çağrıları buraya **bu projede
basınç doğmadığı için** eklenmedi.

| Geri çağrı | GARANTİ | GARANTİ **DEĞİL** | Oraya ait iş |
|---|---|---|---|
| `Awake` | örnek başına bir kez; aynı örneğin `Start`'ından önce; bileşenin `enabled` kutusu kapalı olsa bile (GameObject etkinse) | **başka bir nesnenin** `Awake`'ine göre sırası; deaktif GameObject'te hiç çağrılmadığı — etkinleşene kadar **ertelenir** | kendi kendine yeten kurulum: kendi alanları, `GetComponent` ile kendi bileşeni |
| `OnEnable` | etkin-ve-açık hâle gelen her geçişte; `Awake`'ten sonra | bir kez olduğu — **tekrar tekrar** çağrılır | geri alınabilir iş: olaya abone olmak, kayıt açmak |
| `Start` | örnek başına bir kez; **bütün** ilk-sahne `Awake`'lerinden sonra; bu örneğin ilk `Update`'inden önce | bileşen hiç etkinleşmezse çağrıldığı — beklemede kalır | başka nesnelerin kurulmuş olmasına **güvenen** tek seferlik iş |
| `Update` | çizilen kare başına en fazla bir kez; yalnız etkin-ve-açıkken | sabit aralıklarla çağrıldığı — **kare süresi değişkendir** | girdi okuma, zaman ilerletme, kare başına çeviri |
| `LateUpdate` | o karedeki bütün `Update`'lerden sonra | bileşen kapanmışsa koştuğu | başkasının `Update`'te yazdığını **takip eden** iş: kamera, bağlı görsel |
| `OnDisable` | etkin-ve-açıklıktan çıkan her geçişte; `OnDestroy`'dan önce | "nesne öldü" demek olduğu — çoğu zaman **sadece kapandı** | `OnEnable`'ın simetriği: abonelik bırakma, kip iptali |
| `OnDestroy` | `Awake`'i koşmuş bir nesne yok edildiğinde | `Awake`'i hiç koşmamış nesnede çağrıldığı | son temizlik: yönetilmeyen kaynak, dışarıya verilmiş kayıt |

> **◀ DÖNÜŞ:** [../dil/06-delege-arka-taraf.md](../dil/06-delege-arka-taraf.md#altinci-durak-event-ile-unity-mesaj-geri-cagrilari-ayni-sey-degil) — «Altıncı durak: `event` ile Unity mesaj geri çağrıları aynı şey değil»den
> geldiysen artık şunu biliyorsun: `OnEnable`/`OnDisable` çiftinin **sırası** bu
> tablodan geliyor, senin yazdığın bir sözleşmeden değil — ***`OnEnable` tekrar
> eder, `Start` etmez***, ve abonelik simetrisi tam olarak bu tekrarın üstüne
> kurulu · oraya dön ve delegenin arka tarafından devam et

"Neden iki tane kurulum geri çağrısı var" sorusunun cevabı tek cümle:

```
>> BÜTÜN Awake'ler BÜTÜN Start'lardan önce koşar. <<
   (ilk sahne yüklemesinde var olan nesneler için — Instantiate bunu bozar)

   nesne A                          nesne B
   Awake  ── kendi alanlarını       Awake  ── kendi alanlarını
      └────────── ikisi de bitti ──────────┘
                      >> SINIR <<
      ┌────────── şimdi güvenilebilir ─────┐
   Start  ── B'nin kurulduğuna     Start  ── A'nın kurulduğuna
             GÜVENİR                        GÜVENİR
```

Kural: **kendi bağımlılığın `Awake`'e, başkasının bağımlılığı `Start`'a.**
`Awake` içinde başka bir nesnenin `Awake`'inin koşmuş olduğunu varsayarsan,
bugün tutan sıra yarın sahne yeniden düzenlendiğinde sessizce kırılır.

### `OnEnable` tekrar eder, `Start` etmez

```
bileşen aç → kapat → aç
   OnEnable ─ OnDisable ─ OnEnable      ← ÜÇ çağrı
   Awake · Start                        ← BİRER çağrı, tekrar YOK
```

Bu ayrım burada soyut değil: `BoardAdapter` aboneliğini `Awake`'te değil
`OnEnable`'da açar, çünkü `Awake`/`OnDestroy` çifti nesnenin **doğumunu** eşler,
olay dinlemek ise **etkinliğe** aittir. Tam gerekçe
[../kod/Unity/BoardAdapter.md](../kod/Unity/BoardAdapter.md#onenable-ve-ondisable)
içinde; burada tekrar edilmiyor.

**Tuzak: `OnEnable` tekrar ettiği için oraya *tek seferlik* iş koyamazsın.**
Bir liste doldurmak, bir nesne yaratmak, bir sayacı sıfırlamak — hepsi ikinci
açılışta ikinci kez koşar.

### Etkinlik matrisi: ezberleme, türet

```
H = GameObject.activeInHierarchy   (nesne ve bütün ataları etkin mi)
E = Behaviour.enabled              (bileşenin kendi kutusu açık mı)
A = H VE E                         (etkin-ve-açık)

A 0 → 1 ⇒ OnEnable        A 0 → 0 ⇒ hiçbiri
A 1 → 0 ⇒ OnDisable       A 1 → 1 ⇒ hiçbiri
```

`SetActive`, bir atanın etkinleşmesi ve `enabled` yazıcısı — üçü de A'yı
değiştirebilecek **sebeplerdir**, hiçbiri kuralın kendisi değildir.

**İddia:** bileşenin kutusu kapalıyken bile `Awake` koşar (GameObject etkinse).
**Ölçüsü:** bu projede bu durum **hiç yok** — `Assets/Scenes/SampleScene.unity`
içinde `m_IsActive: 0` sayısı **sıfır** (iki GameObject, ikisi de `1`;
`Assets/Game/Prefabs/Unit.prefab` de aynı). Yani bugün her yol "etkin" yoludur
ve ertelenmiş `Awake` bu projede gözlenemez. Ölçmek isteyen okuyucu `Board`
nesnesini Inspector'dan deaktif edip Play'e basmalı ve `Awake`'in **hiç**
koşmadığını görmeli.

### Bunu kendin ölç: iki bileşen, bir günlük

***Buradaki hiçbir iddiayı bana güvenerek kabul etme.***

```
1. Geçici bir MonoBehaviour yaz (repoya EKLEME, deney bitince sil):
      yedi geri çağrının her birinin başına tek satır —
      Debug.Log($"{Time.frameCount} {name} Awake")
      Awake · OnEnable · Start · Update · LateUpdate · OnDisable · OnDestroy
2. İKİ ayrı GameObject'e tak; adlarını A ve B koy.
3. Play'e bas, Console'u zaman sırasına al (sıralamayı KAPATMA).

BEKLENEN:
      A Awake      B Awake            ← ikisi de Start'lardan ÖNCE
      A OnEnable   B OnEnable
      A Start      B Start
      A Update     B Update  (her kare)
      A LateUpdate B LateUpdate

      >> A'nın mı B'nin mi önce geldiği GARANTİ DEĞİL <<
      Gözlem bir kez tuttu diye kural sanma. Sınadığın iddia
      "bütün Awake'ler bütün Start'lardan önce" — "A önce" DEĞİL.

4. Play'deyken B'nin bileşen kutusunu kapat-aç:
      B OnDisable · B OnEnable  ·  Awake ve Start TEKRAR ETMEZ
5. Play'den çık:  OnDisable · OnDestroy
```

Bu deney, aşağıdaki Domain Reload durağı yeşil olmadan **güvenilmez**.

---

## Üçüncü durak: ***BU PROJEDE GERÇEKTE NE VAR***

Sayarak. Uydurma örnek yok.

### Tanımlı olanlar — beş tane, iki dosyada

| # | Geri çağrı | Yeri | Ne yapıyor |
|---|---|---|---|
| 1 | `Awake` | `Assets/Game/Unity/BoardAdapter.cs:293` | `GetComponent<Grid>()`, `new Battle(w,h)`, `new PointerGesture(eşik)`, hayaleti kapatır, zemini kurar, iki demo birim doğurur |
| 2 | `OnEnable` | `Assets/Game/Unity/BoardAdapter.cs:349` | `battle.UnitStateChanged += OnUnitStateChanged` — tek satır |
| 3 | `OnDisable` | `Assets/Game/Unity/BoardAdapter.cs:354` | aynı aboneliği bırakır **ve** `CancelPlacement()` çağırır |
| 4 | `Update` | `Assets/Game/Unity/BoardAdapter.cs:416` | zaman ilerletme + kip ayrımı + üç girdi sorgusu |
| 5 | `Awake` | `Assets/Game/Unity/UnitView.cs:86` | `SetState(Alive)`, sonra `SetSelected(false)` — doğan birimi normalleştirir |

**Toplam: 5.** Başka `MonoBehaviour` yok, başka geri çağrı yok.

`Update`'in her karede ne yaptığı burada **tekrar edilmiyor** — akışın tamamı
[07-tiklamadan-eyleme.md](07-tiklamadan-eyleme.md), satır satır gerekçesi
[../kod/Unity/BoardAdapter.md](../kod/Unity/BoardAdapter.md#update).

### Tanımlı OLMAYANLAR — ve neden gerekmemiş

| Geri çağrı | Neden gerekmemiş |
|---|---|
| `Start` ██ YOK ██ | Kurulumun tamamı **kendi kendine yeter**. `BoardAdapter.Awake` yalnız kendi bileşenini, kendi alanlarını ve kendi doğurduğu nesneleri kurar; başka bir nesnenin kurulmuş olmasına hiç güvenmez. `Start`'ın var olma sebebi tam olarak o bekleyiştir; bekleyiş yoksa geri çağrı da yok. |
| `LateUpdate` ██ YOK ██ | Başkasının `Update`'te yazdığını takip eden hiçbir şey yok. Kamera sabit, birim görselleri hücre merkezine **anında** konuyor. |
| `FixedUpdate` ██ YOK ██ | Fizik yok. `Rigidbody` yok, çarpışma yok. |
| `OnDestroy` ██ YOK ██ | Bırakılacak şey kalmıyor: tek dış bağ olan olay aboneliği `OnDisable`'da bırakılıyor ve **`OnDisable`, `OnDestroy`'dan önce çağrılır**. Yönetilmeyen kaynak (dosya, ağ) yok. |
| `OnValidate` ██ YOK ██ | Inspector doğrulaması **attribute** ile yapılıyor: `[SerializeField, Min(1)]`. Kod isteyen bir çapraz alan kuralı doğmadı. |
| `OnApplicationPause`/`Focus`/`Quit` ██ YOK ██ | Kaydedilecek durum yok; kapanınca kaybolacak bir ilerleme henüz yok. |
| `OnMouseDown` ve akrabaları ██ YOK ██ | Tıklama çarpıştırıcı üzerinden değil, `Camera.main.ScreenToWorldPoint` ile hücreye çevrilerek okunuyor — gerekçesi [07-tiklamadan-eyleme.md](07-tiklamadan-eyleme.md) içinde. |

Ayrıca `Assets/` altında **uygulanmış** hiçbir `[ExecuteAlways]`,
`[ExecuteInEditMode]`, `[DefaultExecutionOrder]` ya da
`[RuntimeInitializeOnLoadMethod]` yok. Sayının tam hâli, çünkü ölçü yuvarlanmaz:
`ExecuteAlways` kelimesi `Assets/` altında **bir kez** geçiyor —
`Assets/Tests/EditMode/Unity/UnitViewTests.cs:31`, ve orada bir **yorumun
içinde**, "bu script `[ExecuteAlways]` değil" cümlesini kurmak için. Yani
uygulanmış attribute sayısı **0**, metinde geçiş sayısı **1**.

***Bu projede çağrı sırasını elle zorlayan hiçbir şey yok*** — sıranın tamamı
motorun varsayılan sözleşmesi. Aşağıdaki "Kaçış yolu" bunun neden bir eksiklik
değil bir tercih olduğunu anlatıyor.

### ***Projedeki tek gerçek sıra bağımlılığı***

```
BoardAdapter.Awake                    BoardAdapter.OnEnable
  battle = new Battle(w, h);  ───────►  battle.UnitStateChanged += ...
        ▲                                     ▲
   alanı KURAN                           alanı KULLANAN
        └──────── Awake, OnEnable'dan ÖNCE ───┘
                  >> BU GARANTİ OLMASA KOD ÇÖKERDİ <<
```

**Ölçüsü:** ters olsaydı `OnEnable`'daki `battle` `null` olur ve daha ilk karede
`NullReferenceException` atardı. Oyun açılıyorsa sıra tutmuştur — projenin
kendisinin sürekli koşan bir sınavı.

### İkinci gerçek: bir `Awake`'in içinde başka bir `Awake`

`BoardAdapter.Awake` sonunda `SpawnUnit` çağırıyor, o da
`Instantiate(unitPrefab, transform)` yapıyor. `Instantiate` yeni bir GameObject
doğurur ve doğan nesnenin `Awake`'i (`UnitView.cs:86`) beklemeye alınmaz —
**çağrı dönmeden** koşar:

```
BoardAdapter.Awake  başlar
   ├─ GetComponent<Grid>() · new Battle(...) · new PointerGesture(...)
   ├─ BuildCellVisuals()
   ├─ SpawnUnit("Vanguard", ...) └─ Instantiate → >> UnitView.Awake KOŞAR <<
   └─ SpawnUnit("Raider",   ...) └─ Instantiate → >> UnitView.Awake KOŞAR <<
BoardAdapter.Awake  biter
        v
BoardAdapter.OnEnable
```

**"Bütün `Awake`'ler bütün `Start`'lardan önce" sözü *ilk sahne yüklemesinde
var olan* nesneler içindir.** Oyun ortasında doğan bir nesnenin `Awake`'i,
koşan karenin ortasında, başkalarının `Start`'ından **sonra** gerçekleşir.

**Ölçüsü:** yukarıdaki günlük deneyini bu iki tipe uygula; Console'da
`UnitView Awake` satırları `BoardAdapter OnEnable`'dan **önce** görünmeli.

### Üçüncü gerçek: `Awake` EditMode'da HİÇ koşmaz

Bu, projedeki bir tasarım kararının doğrudan sebebi:

```
Assets/Tests/EditMode/Unity/UnitViewTests.cs
  "AWAKE BU TESTLERDE HİÇ ÇALIŞMAZ. EditMode'da bir MonoBehaviour'ın
   Awake'i tetiklenmez (script [ExecuteAlways] değil)."
```

Bu yüzden `UnitView` gövde çizicisini `Awake`'te önbelleğe **almıyor**; tembel
çözülen bir `Body` property'si kullanıyor. `Awake`'e konsaydı EditMode
testlerinde `body` sonsuza dek `null` kalır, `SetState` sessizce hiçbir şey
yapmaz ve o dosyadaki testlerin tamamı **sebebi görünmeden** kırmızıya dönerdi.
Tam gerekçe [../kod/Unity/UnitView.md](../kod/Unity/UnitView.md#body) içinde.

**"Hangi geri çağrıya koyacağım" yalnız bir zamanlama sorusu değil; *neyin
sınanabilir kalacağı* sorusu.**

---

## Dördüncü durak: ***SINIR — motor tarafı / motorsuz çekirdek***

```
   ┌─────────────── MOTOR TARAFI ────────────────┐
   │  UnityEngine — kare sayar, adı bilinen      │
   │     │           metodu çağırır              │
   │     v                                       │
   │  BoardAdapter.Update                        │
   │     │  Input.GetMouseButtonDown(0)          │
   │     │  Camera.main.ScreenToWorldPoint(...)  │
   │     │  Time.deltaTime                       │
   │     v                                       │
   │  battle.Tick(Time.deltaTime)                │
   └─────────────────┬───────────────────────────┘
   ██████████████████│█████████████████████████████
   ██  A S S E M B L Y   D U V A R I             ██
   ██  noEngineReferences: true                  ██
   ██  UnityEngine.dll bu tarafta YOK            ██
   ██████████████████│█████████████████████████████
                     │  duvardan geçen tek şey: bir `float`
                     v
   ┌────────────── MOTORSUZ ÇEKİRDEK ────────────┐
   │  Battle.Tick(float deltaSeconds)            │
   │     v                                       │
   │  Combatant.Tick → UnitLifecycle.Tick        │
   │  >> MonoBehaviour YOK · Awake YOK ·         │
   │     KARE YOK · Time YOK · Input YOK <<      │
   └─────────────────────────────────────────────┘
```

`Battle` bir kareyi bilmez, bir tıklamayı bilmez. Ona **saniye** verilir ve o
saniyenin nereden geldiğini sormaz. Duvarın kendi hikâyesi:
[02-assembly-duvari.md](02-assembly-duvari.md).

**REDDEDİLEN** — `Battle` bir `MonoBehaviour` olsaydı:

```csharp
public sealed class Battle : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;

    private void Update()
    {
        Tick(Time.deltaTime);
    }
}
```

**KIRILAN:**
- `GridStrategy.Battle.asmdef` içindeki `noEngineReferences: true` **düşer**;
  assembly `UnityEngine.dll`'e bağlanır ve duvarın dört faturasının dayanağı
  yok olur.
- `BoardAdapter.Awake`'teki şu satır **derleme hatasıdır** — `MonoBehaviour`
  `new` ile kurulamaz. Yerine bir GameObject + `AddComponent<Battle>()` gerekir;
  kurucu argümanları hiçbir yerden geçirilemez.

  ```
  BoardAdapter.cs:299   battle = new Battle(width, height);
  ```

- Kurucunun yerine ikinci bir `Init(w, h)` metodu doğar, onunla birlikte **yeni
  bir yasak durum**: "kurulmuş ama Init edilmemiş `Battle`". Bugün böyle bir
  durum yok, çünkü kurucu geçilemez.
- `Assets/Tests/EditMode/Battle/` altındaki testler `Battle`'ı çıplak kurdukları
  için sahne istemiyor; hepsi GameObject + `AddComponent` + `Init` üçlüsüne
  mecbur kalır. Ölçüsü: `UnitLifecycleTests` bugün `Tick(10.1f)` yazıp
  pencerenin **tam** yerini sınıyor; kareye bağlansaydı aynı iddia
  `WaitForSeconds` beklemeye düşer ve kırmızılığı kurala değil o günkü kare
  süresine bağlanırdı — bu tuzağın adı o dosyada zaten REDDEDİLEN olarak yazılı.
- `Time.deltaTime`'ı içeriden okuyan tasarım testte **patlamaz**, sessizce
  anlamsız bir sayıyla yürür: `UnitLifecycle.cs:164` ölçmüş — EditMode'da
  `Time.deltaTime` sıfır değil, `0,017675` döner.

**KAZANIRDI:** savaşın kendisi **kare kare canlandırma yürüten** bir şey
olsaydı — mermi uçsa, birim hücreden hücreye kayarak gitse, vuruş anı bir
canlandırma karesine denk gelse — ve EditMode'da sınanması hiç istenmeseydi. O
senaryoda zamanı içeriden okumak kayıp değil kazançtır; `width`/`height`'ı
Inspector'da görmek de öyle. Bugün ikisi de doğru değil: savaş **anlık** karar
veriyor ve sınanabilirlik projenin en pahalı kazanımı.

---

## Beşinci durak: `IEnumerator`'un İKİ AYRI HAYATI

```
BİRİNCİ HAYAT ── foreach'in arkasındaki gezgin
                 `object Current`, `bool MoveNext()`; neden generic değil
                 >> SAHİBİ BAŞKA DOSYA << burada TEKRAR EDİLMİYOR
İKİNCİ HAYAT  ── bir coroutine'in GÖVDESİ
                 aynı arayüz, bambaşka bir iş   >> BU DURAK <<
```

Birinci hayat:
[../dil/02-koleksiyonlar-ve-salt-okunur.md](../dil/02-koleksiyonlar-ve-salt-okunur.md).
Aşağısı yalnızca ikincisi.

### `yield return` gördüğünde derleyici ne yapar

***Metot "duraklamaz".*** Duraklayan bir metot diye bir şey yoktur:

```
SEN YAZARSIN                        DERLEYİCİ ÜRETİR
IEnumerator Yurut()                 sealed class <Yurut>d__0 : IEnumerator
{                                   {
    int adim = 0;                       int <adim>5__1;    ← YEREL DEĞİL, ALAN
    yield return null;                  int <>1__state;    ← nerede kaldık
    adim++;                             object <>2__current;
    yield return null;                  bool MoveNext()
}                                       { switch (<>1__state) { ... } }
                                    }
```

**Yerel değişkenler o gizli nesnenin *ALANLARINA* dönüşür.** Kare arasında
yaşamalarının sebebi budur — yığında (stack) değil, öbekte (heap) duran bir
nesnenin içinde yaşarlar.

```
kare 1: MoveNext() → state 0 → adim = 0, current = null, state = 1, döner
kare 2: MoveNext() → state 1 → adim++,   current = null, state = 2, döner
kare 3: MoveNext() → state 2 → gövde bitti, false döner → coroutine ÖLDÜ
```

Metot bir kez koşup bitmez; **her karede bir kez, kaldığı yerden** koşar.

### `StartCoroutine` ne yapıyor

```
StartCoroutine(Yurut())
   ├─ Yurut() çağrısı >> GÖVDEYİ KOŞTURMAZ << — yalnızca durum makinesi
   │  nesnesini üretip döner; tek satır bile çalışmaz
   └─ motor o nesneyi kendi listesine koyar
          └─ her karede MoveNext() çağırır ve dönen `Current`e BAKAR:
                `null`               → bir sonraki kare
                `WaitForSeconds(2f)` → bir NESNE; motor "2 saniye geçene
                                       kadar MoveNext çağırma" der
                başka bir IEnumerator→ onu bitirene kadar bekler
```

***`WaitForSeconds` hiçbir şey beklemez.*** İçinde döngü, uyku ya da zamanlayıcı
yoktur — bir **işaret nesnesidir**. Bekleyen taraf motordur; nesne yalnızca "ne
kadar" bilgisini taşır.

### ***SAHİP AYRIMI***

```
`C# dili`          : `yield return` / `yield break` anahtar kelimeleri ve
                     derleyicinin ürettiği durum makinesi sınıfı
`.NET kütüphanesi` : `System.Collections.IEnumerator` arayüzü
                     (`object Current`, `bool MoveNext()`, `void Reset()`)
`UnityEngine API`  : `StartCoroutine` · `StopCoroutine` · `StopAllCoroutines` ·
                     `Coroutine` · `WaitForSeconds` · `WaitForEndOfFrame`
```

İlk ikisi Unity'siz bir konsol programında da vardır ve aynı çalışır.
**İddianın ölçüsü:** yerel 2021.3.45f2 kurulumunda
`Managed/UnityEngine/UnityEngine.CoreModule.dll` içinde `StartCoroutine` dizesi
**7 kez**, `WaitForSeconds` **2 kez** geçiyor — ikisi de motorun kendi
dosyasında yaşıyor, .NET'in değil.

***Coroutine bir iş parçacığı (thread) DEĞİLDİR.*** Aynı iş parçacığında,
kare döngüsünün içinde koşar. `MoveNext()` gövdesi bir saniye sürerse **kare bir
saniye uzar**; hiçbir şey paralel gitmez. "Arka planda çalışıyor" cümlesi bu
mekanizma için yanlıştır.

### ***SAHİP TUZAĞI: coroutine `MonoBehaviour`'a bağlıdır***

```
StartCoroutine → MonoBehaviour'ın bir metodu
                 >> `Battle` gibi motorsuz bir tip coroutine BAŞLATAMAZ <<
                 çünkü noEngineReferences: true — o kelime derlenmez bile

GameObject SetActive(false) → coroutine >> DURUR << ve geri dönmez
GameObject Destroy(...)     → coroutine >> ÖLÜR <<
bileşen enabled = false     → coroutine >> DURMAZ <<  ◄── EN SIK YANILGI
```

Son satır bu projede doğrudan bir tuzak kurar: `BoardAdapter` bir `OnDisable`
**taşıyor** ve orada gerçekten temizlik yapıyor. Biri bir gün bu tipe coroutine
eklerse, simetri görüntüsüne bakıp "coroutine de kapanır" diye düşünecek.
**Kapanmaz** — onu ancak `StopCoroutine`, `SetActive(false)` ya da yok etme
durdurur.

**Ölçüsü:** bir bileşene `while(true) { Debug.Log(Time.frameCount); yield return
null; }` koy, Play'e bas, Inspector'dan bileşenin kutusunu kapat. Console akmaya
**devam eder**. Sonra GameObject'i deaktif et — o an durur.
Ek sınır: deaktif bir GameObject üzerinde `StartCoroutine` çağrılamaz.

### ***BU PROJEDE COROUTINE VAR MI***

Sayıldı:

```
Assets/Game/ altında
    StartCoroutine · StopCoroutine · IEnumerator · yield return ·
    WaitForSeconds                    >> HEPSİ SIFIR. Bir tane bile yok. <<

Assets/ altında (testler dâhil)  :  4 SATIR — ve dördü de YORUM
    Assets/Tests/EditMode/Combat/UnitLifecycleTests.cs:71-72
    Assets/Tests/EditMode/Core/PointerGestureTests.cs:31,33
```

Dördü de **REDDEDİLEN blokların içinde**. Yani `IEnumerator` bu projede bugüne
dek yalnızca **eleneni** anlatmak için yazılmış:

```
UnitLifecycleTests.cs — REDDEDİLEN:
    [UnityTest] public IEnumerator ... { yield return new WaitForSeconds(10.1f);
    KIRILAN : test EditMode'dan PlayMode'a düşer; dosyanın süresi
              milisaniyeden dakikaya çıkar
```

**Coroutine bu projede bir *eksiklik değil, bir yokluk*.** Eksiklik
yapılması gerekip yapılmayan şeydir; yokluk henüz basıncı doğmamış şeydir.
Hangi gün geleceği belirsiz değil — üç somut kapı var:

```
① SALDIRI CANLANDIRMASI kare arasına yayıldığı gün.
   Bugün `ReactToAttack` sonucu ANINDA uyguluyor: vuruş, hasar ve görsel
   tazeleme aynı karede biter. Vuruşun havada geçen bir süresi olduğu gün
   o süreyi tutacak bir gövde gerekir.
② DÜŞME VE DİRİLİŞ CANLANDIRMASI geldiği gün.
   Yeri şimdiden ayrılmış: `OnUnitStateChanged(Unit, UnitState from,
   UnitState to)` imzasındaki `from` BUGÜN KULLANILMIYOR ve kodda gerekçesi
   yazılı — "kullanacağı ilk gün adı hazır: düşme ve diriliş animasyonları".
③ SIRALI TUR CANLANDIRMASI geldiği gün.
   `TurnState.DefaultTurnOrder` bir dizilim tutuyor ama tur geçişi anlık.
   "Şimdi düşman oynuyor" diye görünen bir süre olduğu gün soru doğar.
```

### Coroutine'e en yakın alternatifler — yalnız adlarıyla

***Hiçbiri bu projede yok, hiçbiri burada öğretilmiyor.***

| Ad | Tek cümlelik tanım | Durum |
|---|---|---|
| `Awaitable` | Unity'nin kendi "beklenebilir" tipi; coroutine'in yerine `async`/`await` yazımıyla kullanılır | ██ HENÜZ YOK ██ → **bu sürümde mevcut bile değil.** Ölçüldü: 2021.3.45f2 kurulumunda `Managed/UnityEngine/` altındaki hiçbir dosyada `Awaitable` dizesi geçmiyor (0 eşleşme, 2026-08-23). Unity 6'ya geçilen aşamada gündeme gelir. |
| `Task` | .NET'in "sonucu ileride gelecek iş" nesnesi (`System.Threading.Tasks`) | ██ HENÜZ YOK ██ → ağdan ya da diskten veri okunduğu aşama (kayıt/yükleme, çevrimiçi eşleşme). Bugün ne dosya ne ağ var. |
| `UniTask` | üçüncü taraf bir paket; `Task`'ın çöp üretmeyen ve kare döngüsüne oturan karşılığı | ██ HENÜZ YOK ██ → `Packages/manifest.json` içinde geçmiyor (doğrulandı). Bekleme maliyeti **ölçülmüş** bir sorun hâline geldiği aşama. Önce ölçü, sonra paket. |

---

## Altıncı durak: Domain Reload — sessiz kanıt kirleticisi

Yukarıdaki günlük deneyini koşturmadan önce bunu doğrula, yoksa gördüğün sıra
senin kodunun değil Editor ayarının sonucu olabilir.

Sorun: "Enter Play Mode" seçenekleri açık ve Domain Reload devre dışı ise,
Play'den çıkıp yeniden girdiğinde **.NET uygulama alanı yeniden yüklenmez** —
statik alanlar ve statik olay abonelikleri **hayatta kalır**. O zaman ikinci
Play'de gördüğün sıra, birincisinden artakalan durumla kirlenmiş olur.

Bu projedeki **gerçek** değer:

```
ProjectSettings/EditorSettings.asset
    m_EnterPlayModeOptionsEnabled: 0     ◄── >> BELİRLEYİCİ SATIR <<
    m_EnterPlayModeOptions: 3
```

`Enabled: 0` → seçenekler **kapalı** → Unity varsayılan davranışı uygular →
Play'e girerken Domain Reload **yapılır**, statikler sıfırlanır, abonelikler
temizlenir. ***Bu projede yaşam döngüsü kanıtı bugün TEMİZ.***

***İkinci satır bir tuzaktır.*** `m_EnterPlayModeOptions: 3` orada duruyor ve
"iki seçenek de kapatılmış" gibi okunuyor. **Etkisizdir** — birinci satır `0`
olduğu sürece bu değer hiç uygulanmaz. Yalnız ikinci satıra bakan biri tam ters
sonuca varır.

Doğrulama sınırı, dürüstçe: yerel kurulumun kendi belge dosyası
(`Managed/UnityEditor.xml`) `EnterPlayModeOptions.None`, `.DisableDomainReload`
ve `.DisableSceneReload` üyelerinin **varlığını** doğruluyor. `3` sayısının
hangi bitlere karşılık geldiği bu turda **doğrulanmadı** ve gerekmiyor da —
karar birinci satırda veriliyor.

**Ayrıca** bu ayar bozulsa bile bu projenin kaybedecek statik durumu **yok**.
Sayıldı: `Assets/Game/` altında tek bir statik alan var —

```
Assets/Game/Battle/TurnState.cs:44
    public static readonly IReadOnlyList<Team> DefaultTurnOrder =
```

`static readonly`, içeriği değişmez. Değiştirilebilir statik alan sayısı: **0**.
Statik olay sayısı: **0**. Öteki 57 `static` geçişinin hepsi `static class` ve
`static` metot — yani **hafızası olmayan** kural tipleri (`BattleActions`,
`TurnRules`, `AttackAction`, `DamageRules`, ...).

***Yani bu proje Domain Reload kapatılsa bile bugün kirlenmez.*** Bu şans değil
tasarım sonucu: kural tipleri hafızasız tutulduğu için saklanacak bir şey yok.
Ama bu **bugünün** cümlesi — ilk `static` sayaç yazıldığı gün bu durak yeniden
okunmalı.

---

## Yedinci durak: kare başına ne koşuyor, kim başlatıyor

**Doğrulama sınırı: tablonun *yalnız son iki satırı* bu repoya karşı
doğrulandı. Üç oyun satırı genel oyun bilgisidir; bu turda kaynak koda ya da
resmî belgeye karşı *doğrulanmadı* ve öyle okunmalıdır.**

| Oyun | Kare başına gerçekten ne koşuyor | Kim başlatıyor |
|---|---|---|
| **Slay the Spire** | ██ EŞLEŞMİYOR ██ Sırasını bekleyen bir masada kare başına yapılacak oyun işi **neredeyse yok**: ekran yeniden çizilir, o kadar. İş, kartın oynandığı anda doğar ve bir eylem sırası hâlinde tek tek boşalır — kareyle değil **oyuncunun hamlesiyle** tetiklenir. | altındaki çerçeve ekranı her kare yeniden çizer; ama oyun kararını **oyuncunun kart oynaması** başlatır |
| **Vampire Survivors** | Ekrandaki **her** düşman her kare biraz yaklaşır, her silahın bekleme sayacı biraz iner, yüzlerce gövde birbirine değiyor mu diye bakılır. Kare başına iş **nesne sayısıyla büyür** ve oyunun tamamı bu büyümenin üstüne kuruludur. | motorun kendi kare akışı; oyuncu hiçbir şeye basmasa da aynı iş koşar |
| **Stardew Valley** | `Game1` sınıfının güncelleme metodu saniyede 60 kez koşar: oyuncu adımı, çizim kareleri, kasabalıların günlük programı ilerler. Oyun içi saat ise her karede değil, gerçek zamanda yaklaşık yedi saniyede bir on dakika atlar. | `MonoGame`'in `Game` sınıfı akışı yürütür ve `Game1`'in güncelleme metodunu çağırır |
| **CountryBall (bu proje)** | `battle.Tick(Time.deltaTime)` bütün savaşçıların ve yapıların sayaçlarını ilerletir; ardından temizliğe hazır olanlar toplu süpürülür; sonra en fazla üç girdi sorgusu okunur. Tıklama olmasa da **saat işler** — sıra bir karardır: erken çıkışın altına konsaydı düşmüş bir birim el sürülmedikçe ölmezdi. | `UnityEngine`, `BoardAdapter.Update`'i ada göre bulur ve her karede çağırır |
| **KARŞILIĞI OLMAYAN SATIR** | Kare arasına yayılmış bir hareket: bir birimin hücreden hücreye **kayarak** gitmesi. Bugün `view.transform.position = CellCentre(x, y)` ile **anında** ışınlanıyor. | ██ HENÜZ YOK ██ → hareketin kayarak gösterildiği aşama yaratır; o gün ya bir `Update` gövdesi ya bir coroutine doğar ve bu dosyanın beşinci durağı yeniden okunur |

***En öğretici satır birincisidir.*** "Kare başına ne koşuyor" sorusunun cevabı
bazı oyunlarda **"neredeyse hiçbir şey"**dir, ve bu bir eksiklik değil bir tür
farkıdır. Sıra tabanlı bir oyunda iş **olaya** bağlıdır; kare yalnızca çizim
için döner. Bu proje ikisinin **arasında**: kararı olay veriyor (tıklama), ama
zamanı kare taşıyor (`Tick`).

### İKİ AYRI ***"paralel"*** — karıştırılan yer burası

Soru şöyle geliyor: *"bir savaşçıyı saldırttım, hemen ardından ikincisini
saldırttım — ikisi kendi içlerinde paralel savaşmaya devam eder mi, yoksa
sıra sıra mı olur?"* Cevap ikiye ayrılmadan verilemez.

```
TASARIM PARALELLIGI          "ikisi de ayni anda ilerliyor mu"
   EVET. Her kare battle.Tick(Time.deltaTime) BUTUN savascilari dolasir.
   Hicbiri otekini beklemez, hicbiri sira almaz.

YURUTME PARALELLIGI          "ayni anda IKI islemci cekirdeginde mi kosuyor"
   HAYIR. Tek is parcaciginda, arka arkaya. Bir kare icinde once #1 biter,
   sonra #2 baslar.
```

Ve bu bir eksiklik değil. Ölçü: `Assets/Game/` altında `System.Threading` **0**,
`Thread` **0**, `Task` **0**, `IJob`/`JobHandle` **0**, `Burst` **0**,
`async`/`await` **0**, `lock` **0**, `Interlocked` **0**. Bütün oyun tek bir
zincirden akıyor:

```
UnityEngine  ── her kare ──►  BoardAdapter.Update()
                                 │
                                 └─► AdvanceBattleTime()
                                        │
                                        └─► battle.Tick(Time.deltaTime)
                                               ├─► foreach combatants  →  Combatant.Tick
                                               └─► foreach structures  →  Structure.Tick
```

`foreach` sırayla döner. Ama bir kare 16 milisaniyedir ve bu projede sayılar
onlarla ifade ediliyor — sıra kullanıcıya **hiç** görünmez. Kingdom Rush,
Bloons ve Clash of Clans da yüzlerce varlığı aynı biçimde, tek iş parçacığında
yürütür. İş parçacığı ya da `IJob` ihtiyacı **binlerce** varlıkta doğar, ve o
gün geldiğinde kararı Profiler verir — tek bir `Update` kare bütçesini aşıyorsa.

***Ama asıl cevap bu bile değil.*** Bugün bu projede ***"savaşmaya devam etmek"***
diye bir şey **yok**. `AttackAction.Execute` bir `AttackOutcome` döndürür ve
biter: tıklama anında hasar yazılır, iş kapanır. Süreye yayılan tek şey
düşme/ceset geri sayımıdır (`remainingSeconds -= deltaSeconds`).

Yani soru bugün **öznesizdir**. Sürekli savaş, bir saldırı bekleme sayacı
(`Tick` içinde inen bir sayaç, dolunca ateş) eklendiği gün doğar — ve o mekanizma
da tek iş parçacığında koşar, yine paralel *hissettirir*.

---

## Tek bakışta zincir

```
 UnityEngine ── kare sayar ──┐  ADA GÖRE bulur — ABONE OLMAZ
                             │  >> BURASI BİR C# event DEĞİL <<
                             v
 ilk kez ───► Awake          BoardAdapter:225 · UnitView:79
              │              new Battle(w,h) · Instantiate → iç Awake'ler
              │              >> BU SIRA OLMASA KOD ÇÖKER <<
              v
 her açılışta ► OnEnable     BoardAdapter:277 · UnitStateChanged += ...
              v
              Start          >> YOK <<  başkasına güvenen iş yok
              v
 her karede ─► Update        BoardAdapter:306
              ├── battle.Tick(Time.deltaTime) ──► >> DUVAR << ──► Battle
              │                                    (kare YOK · Awake YOK)
              └── Input / Camera sorguları ─────► >> DUVARI GEÇMEZ <<  ✗
              v
 her kapanışta► OnDisable    BoardAdapter:282 · -= ve CancelPlacement()
              v
              OnDestroy      >> YOK <<  bırakılacak şey kalmıyor

 coroutine >> SIFIR << — StartCoroutine/IEnumerator/yield: hiçbiri yok
```

---

## Kural: bu iş hangi geri çağrıya ait

Sırayla sor, ilk "evet"te dur:

```
① İş motorsuz yapılabilir mi? (kare, girdi, kamera, sahne gerekmiyor mu?)
   EVET → >> HİÇBİR GERİ ÇAĞRIYA AİT DEĞİL << Core/ ya da Battle/ altında düz
          bir C# tipine koy; zaman gerekiyorsa PARAMETRE al (Tick(float)).
          Ölçüsü: EditMode'da sahnesiz sınanabiliyor mu?

② İş yalnızca kendi bileşenini/alanlarını mı kuruyor?
   EVET → Awake   (GetComponent, new, Inspector değeriyle kurulan nesne)
          >> TUZAK: EditMode testleri Awake'i HİÇ görmez << Sınanabilir
          kalması gereken kurulumu tembel bir property'ye koy (UnitView.Body).

③ İş BAŞKA bir nesnenin kurulmuş olmasına mı güveniyor?
   EVET → Start   >> ÖNCE SOR: gerçekten güvenmeli mi? << Açık bir Inspector
          referansı ya da açık bir kurucu çoğu zaman daha iyidir. Bu projede
          böyle bir iş YOK ve bu iyi bir işaret.

④ İş GERİ ALINABİLİR mi — açılıp kapanması gerekiyor mu?
   EVET → OnEnable + OnDisable  >> İKİSİNİ AYNI ANDA YAZ <<  Simetriyi
          derleyici TUTMAZ: eksik bir `-=` tek uyarı bile üretmez.
          ██ TUZAK: burası TEKRAR EDER. Tek seferlik iş koyma.

⑤ İş her kare mi koşmalı?
   EVET → Update  >> ÖNCE SOR: gerçekten her kare mi? << Olayla tetiklenebilen
          bir iş Update'e konursa kare başına boş çalışma üretir. Burada ilk
          satır Tick (zaman her kare ilerlemeli), ikincisi erken çıkış.

⑥ İş BAŞKASININ o karede yazdığını mı takip ediyor?     EVET → LateUpdate

⑦ İş kare arasına YAYILAN bir süre mi tutuyor?
   EVET → coroutine (`IEnumerator` + `StartCoroutine`)
          >> SAHİP: bu tip MonoBehaviour mu? Değilse başlatamaz. <<
          >> İPTAL: bileşen kapanınca DURMAZ. Kim durduracak? <<

⑧ İş nesne yok olurken mi yapılmalı?
   EVET → OnDestroy  >> ÖNCE SOR: OnDisable yetmiyor mu? << OnDisable ondan
          önce koşar; abonelik oraya aitse OnDestroy gereksizdir. Bu projede
          tam olarak bu yüzden OnDestroy YOK.
```

---

## Yanlış hatırlanan üç şey

**1. "`Awake` bir `event`'tir; Unity onu tetikler, ben abone olurum."**
***Değil, ve bu yanılgı üç ayrı yanlış davranış üretir.*** `+=` ile abone
olunacak bir şey yok, `-=` ile bırakılacak bir şey yok, birden çok dinleyici
olamaz. Motor **ada göre bulur**. Bu yüzden `Awake` adını değiştirmek bir
derleme hatası değil, **sessiz bir çöküş** üretir. Pratik zararı: "abonelik"
sandığın şeyi bırakmaya çalışırsın (yapamazsın), ya da sızıntı sanıp gereksiz
bir `OnDestroy` yazarsın (bırakılacak bir şey yok).

**2. "Bileşeni kapatınca coroutine durur."**
**Durmaz.** `enabled = false` coroutine'i durdurmaz; onu ancak `StopCoroutine`/
`StopAllCoroutines`, GameObject'in `SetActive(false)` ile deaktif edilmesi ya da
nesnenin yok edilmesi durdurur. Yanılgı bu projede özellikle tehlikeli, çünkü
`BoardAdapter` bir `OnDisable` **taşıyor** ve orada gerçekten temizlik yapıyor —
simetri görüntüsü, olmayan bir garantiyi düşündürüyor.

**3. "`Update` sabit aralıklarla koşar, yani `Time.deltaTime` sabittir."**
Değil. `Update` **çizilen kare başına** koşar ve kare süresi makineye, sahneye,
o anki yüke göre değişir. Sonucu bu projede doğrudan görülüyor: `UnitLifecycle`
zamanı **dışarıdan** alır, tam da bu değişkenlik yüzünden. Ölçülmüş hâli kodda
yazılı — EditMode'da `Time.deltaTime` sıfır değil `0,017675` döner; içeriden
okuyan bir tasarım testte patlamaz, **sessizce anlamsız bir sayıyla yürür**.

---

## Kaçış yolu: bu döngüden nasıl kaçılırdı

**① Her şeyi tek bir `MonoBehaviour`'a koymak.** Kaçış değil teslim olmaktır:
kural, görsel ve girdi tek dosyada olur, EditMode'da hiçbiri sınanamaz. Bu proje
tam tersini yaptı ve bedelini ölçtü — `BoardAdapter` bugün **çevirmen**, karar
verici değil. Faturası [02-assembly-duvari.md](02-assembly-duvari.md) içinde
dört kalem hâlinde yazılı.

**② `[DefaultExecutionOrder]` ile sırayı zorlamak.** Bugün sıfır kullanım var ve
bu iyi. Sırayı zorlamak gerçek bir bağımlılığı **görünmez** kılar: iki tip
birbirine bağlıdır ama bağ hiçbir dosyada değil, bir ayar penceresindedir. Açık
bir referans ya da açık bir kurucu her zaman daha okunurdur. `BoardAdapter`
`Battle`'ı **kendisi kuruyor** — sıraya ihtiyaç duymamasının sebebi bu.

**③ Kendi kare akışını yazmak.** Tek bir `MonoBehaviour` `Update`'i olur, o da
bütün sistemleri elle sırayla çağırır. Gerçek bir teknik, büyük projelerde
gerçekten kullanılır. Bedeli: motorun ücretsiz verdiği her davranışı
(etkinleşme, deaktifleşme, yok olma) kendin yazarsın. ***HENÜZ YOK*** → sistem
sayısı ikiyi geçtiği ve aralarındaki sıranın **gerçekten** önem kazandığı
aşamada tartışılır. İki `MonoBehaviour` için tartışılmaz bile.

**Neden kaçılmadı:** kaçılacak bir şey yoktu. Motorun çağrı döngüsü bu projede
**beş metotluk bir yüzeye** sıkışmış ve o beş metodun tamamı
`Assets/Game/Unity/` altında. Geri kalan her şey — savaş, kural, durum, jest —
duvarın öte yanında, motor diye bir şeyin varlığından habersiz yaşıyor.

---

## Bunu okuduktan sonra kodda ne göreceksin

- `BoardAdapter.cs:232` — `private void Awake()`. Artık `private`'ın motoru
  durdurmadığını ve bu metodu hiçbir satırın çağırmadığını biliyorsun.
- `BoardAdapter.cs:288` ve `:290` — arka arkaya iki satır, **iki bambaşka
  mekanizma**: biri motorun ada göre bulduğu bir mesaj, öteki senin elinle
  yazdığın bir C# olay aboneliği.
- `BoardAdapter.cs:627` — `battle.Tick(Time.deltaTime)`. Motor tarafının son
  satırı; bundan sonrası duvarın öte yanı.
- `Assets/Game/Core/PointerGesture.cs:281` — `public void Reset()`. Bir Unity
  mesaj adı taşıyor ve motor bu tipi hiç görmüyor.
- `Assets/Game/Battle/TurnState.cs:44` — projenin **tek** statik alanı, ve
  `readonly`. Domain Reload durağının neden bugün sakin olduğunun sebebi.
- `Assets/Tests/EditMode/Combat/UnitLifecycleTests.cs:71` — `IEnumerator`
  kelimesinin geçtiği iki yerden biri, ve **reddedilmiş** olanı.

---

## İlgili

- Oyunun durum makinesi (ad çakışması): [05-yasam-dongusu.md](05-yasam-dongusu.md)
- Duvarın kendi hikâyesi: [02-assembly-duvari.md](02-assembly-duvari.md)
- `Update`'in içindeki akış: [07-tiklamadan-eyleme.md](07-tiklamadan-eyleme.md)
- Olay zinciri (`+=` tarafı): [01-olay-zinciri.md](01-olay-zinciri.md)
- Delege ve `event`: [../dil/04-delege-olay-ve-kapanis.md](../dil/04-delege-olay-ve-kapanis.md)
- Delegenin derleyici tarafı, aynı sınır öteki yönden:
  [../dil/06-delege-arka-taraf.md](../dil/06-delege-arka-taraf.md)
- `IEnumerator`'un birinci hayatı: [../dil/02-koleksiyonlar-ve-salt-okunur.md](../dil/02-koleksiyonlar-ve-salt-okunur.md)
- Üye başına gerekçeler: [../kod/Unity/BoardAdapter.md](../kod/Unity/BoardAdapter.md) ·
  [../kod/Unity/UnitView.md](../kod/Unity/UnitView.md)
- Bu ağacın yönlendirmesi: [../README.md](../README.md)

---

## ***SIRADAKİ ADIM***

> **▶ SIRADA:** [`01-olay-zinciri.md`](01-olay-zinciri.md) — okuma yolunun **10.** adımı
> **NEDEN ORASI:** ***numarası `01` ama okuma yolunun sonlarında*** — çünkü bir
> GİRİŞ değil, bir **DÜĞÜM**: üç ipliğin bağlandığı yer. İkisini artık kapattın
> (`konular/02` 2. adımda, `konular/05` 5. adımda); üçüncüsü (`dil/04`) bilerek
> **sonraya** bırakıldı — `01` delegenin ne **yaptığını** gösteriyor, `dil/04` ne
> **vaat ettiğini**. Zinciri önce gör, sözleşmeyi sonra oku. ***Bu adımdan önce***
> [`../../ogrenme/00-okuma-sirasi.md`](../../ogrenme/00-okuma-sirasi.md)'ndaki
> **DURMA NOKTASI 5**'i geç: iki bileşenli günlük deneyini kendin koştur, sonra
> geçici script'i **sil**.
> **YOL HARİTASI:** [`../../ogrenme/00-okuma-sirasi.md`](../../ogrenme/00-okuma-sirasi.md)
