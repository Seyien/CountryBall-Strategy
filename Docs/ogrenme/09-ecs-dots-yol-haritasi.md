# ECS ve DOTS — mekanizma, eşik ve genişletme merdiveni

██ **Bu bir "şimdi yapalım" belgesi DEĞİL.** ██ İçinde tek bir "şunu ekle"
cümlesi yok. Üç iş yapıyor ve üçü de ileriye dönük:

```
① MEKANİZMA        ECS'in üç harfi ayrı ayrı ne demek, ve DOTS'un üç parçası
                   neden AYRI şeyler
② EŞİK             bu proje o eşiğe ne kadar uzak — sayıyla, koda karşı
③ MERDİVEN         o kavramları GÖRÜNÜR kılmak için proje nasıl genişletilir
```

Neden şimdi yazılıyor: [`02-sonraki-asamalar.md`](02-sonraki-asamalar.md)
Aşama 5, ECS için bir **tetikleyici koşul** yazmış durumda — "ne zaman gelir"
sorusu kapalı. Kapalı olmayan şey **"gelince ne olur"**: mekanizmanın kendisi bu
ağacın hiçbir yerinde anlatılmıyor. `ECS` kelimesi dört belgede, `DOTS` ikide,
`Burst` ikide, `Job System` birde geçiyor — hepsi tetikleyici satırı. Bu belge o
boşluğu kapatıyor, tetikleyici satırını **tekrar etmiyor**.

██ Ve bir uyarı, en başa: ECS'i bugün bu projeye getirmek bir **iyileştirme
değil**, bir **öğrenme egzersizi** olurdu. ██ İkisi farklı şeylerdir. Bir
iyileştirmenin ölçülmüş bir darboğazı vardır; bir öğrenme egzersizinin ölçülmüş
bir **öğrenme hedefi** vardır. Bu belge ikincisini yazıyor ve ikisini
karıştırmamak için beşinci bölümdeki merdivenin her basamağında "bu neyi
GÖRÜNÜR kılar" satırı ayrı duruyor.

---

## Ölçüm künyesi — bu belgedeki her sayı nereden geldi

██ `performance-research` kuralı: yerel ölçüm, üst kaynak (upstream) rehberliği
ve çıkarım AYRI AYRI yazılır. ██ Bu belgede üç işaret var ve her sayının yanında
biri duruyor:

```
[YEREL ÖLÇÜM]   bu makinede ya da bu depoda, 2026-08-23'te ölçüldü
[BİRİNCİL]      Unity resmî belgesi; URL, paket sürümü ve doğrulama tarihi yazılı
[ÇIKARIM]       yukarıdaki iki sınıftan türetildi; kendisi ölçülmedi
[DOĞRULANMADI]  soruldu, cevaplanamadı. Uydurulmadı — işaretlendi
```

### Konak makine — `performance-research` Adım 1

[YEREL ÖLÇÜM] · 2026-08-23 · `Get-CimInstance` ve `GetLogicalProcessorInformation`
(Win32 API) ile:

```
İşlemci        AMD Ryzen 9 8945HX          16 fiziksel / 32 mantıksal çekirdek
L1 veri        çekirdek başına 32 KB       önbellek satırı 64 bayt
L2             çekirdek başına 1 MB        önbellek satırı 64 bayt
L3             2 × 32 MB                   önbellek satırı 64 bayt
Bellek         31,22 GB
İşletim sistemi Windows 11 Pro 10.0.26200
Sınıf          güçlü iş istasyonu — ██ tipik bir hedef cihaz DEĞİL ██
```

██ Son satır bu belgenin en önemli künye satırı. ██ Bu makinede ölçülen hiçbir
şey bir telefonda ya da bir konsolda aynı çıkmaz; ECS/Job/Burst tartışmasının
tamamı **çekirdek sayısına ve önbellek boyutuna** bağlıdır ve ikisi de burada
üst sınırda. Bir mobil cihazda çekirdek sayısı 8, büyük çekirdek sayısı 1-2,
L3 çoğu zaman yok. Bu belgede "bu makinede" yazmayan bir performans cümlesi
yoktur.

### Bu deponun Unity kurulumu

[YEREL ÖLÇÜM] · 2026-08-23:

```
ProjectSettings/ProjectVersion.txt          m_EditorVersion: 2021.3.45f2
Packages/manifest.json                      com.unity.entities  ── YOK
                                            com.unity.burst     ── doğrudan YOK
Packages/packages-lock.json                 com.unity.burst 1.8.18  "depth": 3
                                            com.unity.mathematics 1.2.6  "depth": 2
Library/PackageCache/                       com.unity.burst@1.8.18  ── DİSKTE VAR
Assets/ altında BurstCompile · IJob ·
NativeArray · Unity.Jobs · Unity.Collections
· Unity.Mathematics                         ── SIFIR eşleşme
```

██ Ölçülmüş çelişki, ve bu belgenin en öğretici tek olgusu: **Burst bu projede
zaten kurulu ve hiçbir şey yapmıyor.** ██ Nereden geldiği de ölçüldü —
`com.unity.2d.aseprite` → `com.unity.2d.common` → `com.unity.burst`. Yani
2D özellik paketinin dolaylı bağımlılığı. Kurulu olması tek bir satır kodu
hızlandırmıyor, çünkü hızlandıracağı kod yok: `[BurstCompile]` işaretli tek bir
metot bile mevcut değil.

### Motorun kendisinde ne var, pakette ne var

[YEREL ÖLÇÜM] · 2026-08-23 · `UnityEngine.CoreModule.dll` (2021.3.45f2), Unity'nin
kendi `Unity.Cecil.dll`'i ile okundu:

```
Unity.Jobs.IJobParallelFor                         ── CoreModule içinde VAR
Unity.Collections.NativeArray`1                    ── CoreModule içinde VAR
Unity.Jobs.LowLevel.Unsafe.JobsUtility             ── CoreModule içinde VAR
   JobsUtility.CacheLineSize    = 64
   JobsUtility.MaxJobThreadCount = 128
```

██ AYRIŞMA NOKTASI ██ — burada ayrışan iki şey: **"DOTS" adı** ile **paket
sınırı**. Job System bir paket değil, motorun **çekirdeğinde**. Bugün, hiçbir
paket kurmadan, `IJobParallelFor` yazılabilir. Burst bir paket ve zaten kurulu.
Entities ise ne kurulu ne de kurulabilir (aşağıda, dördüncü durak). Üçü "DOTS"
diye tek adla anılıyor ama üçünün **teslim yolu bile** farklı.

`CacheLineSize = 64` sayısı ile yukarıdaki donanım ölçümü (64 bayt) birbirini
bağımsız olarak doğruluyor: biri Unity'nin sabiti, öteki işletim sisteminin
işlemciden okuduğu değer.

### Birincil kaynaklar — hepsi 2026-08-23'te doğrulandı

```
Entities 1.0.16 · Archetypes concepts
  docs.unity3d.com/Packages/com.unity.entities@1.0/manual/concepts-archetypes.html
Entities 1.0.16 · Components concepts
  docs.unity3d.com/Packages/com.unity.entities@1.0/manual/concepts-components.html
Entities 1.0.16 · Systems concepts
  docs.unity3d.com/Packages/com.unity.entities@1.0/manual/concepts-systems.html
Entities 1.0.16 · Entity API
  docs.unity3d.com/Packages/com.unity.entities@1.0/api/Unity.Entities.Entity.html
Entities 1.0.16 · Overview (sürüm koşulu)
  docs.unity3d.com/Packages/com.unity.entities@1.0/manual/index.html
Entities 0.51.1-preview.21 · Installation and setup
  docs.unity3d.com/Packages/com.unity.entities@0.51/manual/install_setup.html
Burst 1.8 · Burst compiler
  docs.unity3d.com/Packages/com.unity.burst@1.8/manual/index.html
Burst 1.8 · HPC# overview
  docs.unity3d.com/Packages/com.unity.burst@1.8/manual/csharp-hpc-overview.html
Unity Manual 2021.3 · Job system overview
  docs.unity3d.com/2021.3/Documentation/Manual/JobSystemOverview.html
Unity Manual · The safety system in the C# Job System
  docs.unity3d.com/2020.1/Documentation/Manual/JobSystemSafetySystem.html
```

██ Kaynak sınırı, dürüstçe: ██ son satırdaki güvenlik sistemi sayfasının
**2021.3 sürümündeki** karşılığı bu turda getirilemedi (sunucu 404 döndü);
alıntılar 2020.1 sürümündeki aynı adlı sayfadan. Blittable kısıtı ise 2021.3
"Job system overview" sayfasında ayrıca doğrulandı, yani iddianın kendisi
sürüm sınırında değil — **alıntının kaynağı** o sürümde değil. Fark budur ve
gizlenmedi.

---

## Birinci durak: ECS — üç harf, üç ayrı şey

Kısaltma **E**ntity · **C**omponent · **S**ystem. Üçü aynı anda öğrenilmeye
çalışıldığında hiçbiri öğrenilmiyor, çünkü üçü aynı sorunun parçası değil: ilki
bir **kimlik** kararı, ikincisi bir **depolama** kararı, üçüncüsü bir **döngü
sahipliği** kararı.

### E — Entity: kimlik, veri DEĞİL. Bir sayı.

[BİRİNCİL] Entities 1.0.16, `Unity.Entities.Entity` API sayfası, 2026-08-23'te
doğrulandı: `Entity`, "contains an Index that you can use to access entity data,
and a Version that you can use to check whether the Index is still valid."
İki alanı var — `Index` ("The ID of an entity") ve `Version` ("The generational
version of the entity"). Yani bir varlık **bir sayı çiftidir**. İçinde can yok,
konum yok, taraf yok, davranış yok.

`Version` alanı tek başına bir ders: bir varlık yok edilip indeksi yeniden
kullanıldığında sürüm artar, böylece eski bir `Entity` değeri "aynı indeks ama
başka varlık" durumunu **yakalayabilir**. Nesne modelinde bu sorun hiç doğmaz
(referans ya canlıdır ya değil); indeks modelinde doğar ve sürüm alanı onun
çözümüdür.

██ Bu projede bu işi yapan tip zaten var. ██
[`01-koda-gomulu-desenler.md`](01-koda-gomulu-desenler.md) §9 ona bir ad da
vermiş: **kimlik + yan tablo**.

```
Assets/Game/Core/Unit.cs:41       public sealed class Unit
Assets/Game/Core/Unit.cs:56       public string Name { get; }
```

`Unit`'in **tek** üyesi bir ad ve o ad bile anahtar değil — anahtar referans
eşitliği. Yani `Unit` tam olarak ECS'in `Entity`'sinin yaptığı işi yapıyor:
kendisi hiçbir veri taşımıyor, yalnız **kim olduğunu** taşıyor.

Fark tek satırda: `Entity` bir `struct` ve iki `int`; `Unit` bir `sealed class`,
yani yönetilen yığında bir nesne ve elde tutulan şey ona giden bir **referans**.

### C — Component: saf veri… ama bu cümle TAM DOĞRU DEĞİL

██ En sık tekrarlanan yarı-doğru: "bileşenlerin davranışı yoktur." ██
[BİRİNCİL] Entities 1.0.16 "Components concepts", 2026-08-23'te doğrulandı, tam
tersini söylüyor: "They can contain methods, but it's best practice for them to
just be pure data."

Yani **davranış yasak değil, tavsiye edilmiyor.** Gerçekten zorunlu olan iki şey
başka:

```
① "Use the IComponentData interface, which has no methods, to mark a
   struct as a component type."            ── STRUCT olmak zorunda
② "can only contain unmanaged data"        ── yönetilmeyen veri olmak zorunda
```

İkinci madde `string`, `List<T>`, `class` alanlarını ve dolayısıyla bu projedeki
neredeyse her tipi dışarıda bırakır.

**Bu projedeki karşılığı, ve farkı:**

```
Assets/Game/Core/Combat/Health.cs:27       public sealed class Health
Assets/Game/Core/Combat/Health.cs:29       private int current;
Assets/Game/Core/Combat/Health.cs:67       public void TakeDamage(int amount)
Assets/Game/Core/Combat/Health.cs:76       public void Heal(int amount)
```

`Health` bir bileşene **çok yakın**: tek bir olguyu (can) taşıyor, kimliği
bilmiyor, tahtayı bilmiyor. Ama iki metodu var ve bir `class`. `IComponentData`
olamamasının sebebi metotlar **değil** — birincil kaynağa göre metot serbest —
`class` olması.

```
Assets/Game/Core/Combat/AttackProfile.cs:40   public sealed class AttackProfile
Assets/Game/Core/Combat/AttackProfile.cs:72   public int Damage { get; }
Assets/Game/Core/Combat/AttackProfile.cs:78   public int Range { get; }
```

`AttackProfile` daha da yakın: iki `int`, sıfır metot, kurulduktan sonra
değişmez. ECS'te bu tip `struct AttackProfile : IComponentData` olurdu ve tek
değişiklik `class` → `struct` olurdu.

██ [YEREL ÖLÇÜM] 2026-08-23: `Assets/Game/` altında kullanıcı tanımlı **hiçbir
`struct` yok**; 26 adet `sealed class` / `static class` var. ██ Yani ECS'e
geçmek "birkaç tipi işaretlemek" değil, **tip kategorisini** değiştirmek demek —
ve bir `class`ı `struct` yapmak kopyalama semantiğini değiştirir. Değer/referans
ayrımının tamamı burada:
[`../deep/dil/05-deger-referans-ve-kimlik.md`](../deep/dil/05-deger-referans-ve-kimlik.md).

### S — System: bir sorgu üzerinde dönen davranış

[BİRİNCİL] Entities 1.0.16 "Systems concepts", 2026-08-23'te doğrulandı:
"A system provides the logic that transforms component data from its current
state to its next state." İki biçimi var — `ISystem` (bir `struct`, yönetilmeyen)
ve `SystemBase` (bir `class`, yönetilen); ikisi de `OnCreate`, `OnUpdate`,
`OnDestroy` taşıyor. Ve sıra rastgele değil: "system groups update the group's
children in a sorted order", üç varsayılan grupla —
`InitializationSystemGroup`, `SimulationSystemGroup`, `PresentationSystemGroup`.

**Bu projedeki karşılığı, ve farkı:**

```
Assets/Game/Core/Combat/TargetingRules.cs:31   public static class TargetingRules
Assets/Game/Core/Combat/DamageRules.cs:24      public static class DamageRules
Assets/Game/Battle/TurnRules.cs:28             public static class TurnRules
Assets/Game/Core/Combat/MovementRules.cs:22    public static class MovementRules
```

Bu dört tip **saf kuraldır**: alanları yok, girdiyi alır cevabı verir. ECS'in bir
sisteminin **davranış yarısı** tam olarak budur. Eksik olan **döngü yarısı**:

```
Assets/Game/Core/Combat/DamageRules.cs:33   public static int ResolveRemaining(int current, int amount)
Assets/Game/Battle/TurnRules.cs:59          public static bool CanAct(Team unitTeam, Team currentTurn)
Assets/Game/Core/Combat/MovementRules.cs:47 public static bool CanMove(UnitState state)
```

Üçü de **tek bir şey** hakkında soru cevaplıyor. Hiçbiri "bütün X'leri dolaş"
demiyor. Dolaşan tek yer:

```
Assets/Game/Battle/Battle.cs:377   public void Tick(float deltaSeconds)
Assets/Game/Battle/Battle.cs:383   foreach (KeyValuePair<Unit, Combatant> pair in combatants)
Assets/Game/Battle/Battle.cs:394   foreach (KeyValuePair<Unit, Structure> pair in structures)
```

██ Fark burada ve tek cümlelik: `Battle.Tick` **neyi dolaşacağını sabit
biliyor**; bir ECS sistemi **sorar**. ██ ECS'te "hem `Health` hem `AttackProfile`
taşıyan bütün varlıklar" bir sorgudur ve o sorgunun cevabı çalışma zamanında,
bileşen bileşimine göre oluşur. Burada ise iki döngü elle yazılmış ve
`combatants` ile `structures` adları koda gömülü.

İkinci fark: sıra. ECS'te döngüyü bir **zamanlayıcı** çevirir ve sistemlerin
sırası bir gruba ve `[UpdateInGroup]` niteliklerine bağlıdır. Burada sırayı bir
metot gövdesi belirliyor — birinci `foreach`, sonra ikincisi. Ve o sıranın
gerekçesi kodda yazılı: ikisi ayrı metotlara bölünseydi çağıran birini unutabilirdi.

### ██ EN ÖNEMLİ AYRIM: ECS'in asıl kazancı MİMARİ değil YERLEŞİM ██

Buraya kadar anlatılan üç şeyin hiçbiri hızla ilgili değildi. "Veriyi
davranıştan ayır" bir **mimari** fikirdir ve bu projede zaten uygulanmış
durumda — kural tiplerinin alanı yok, varlıkların kuralı yok. Eğer ECS bundan
ibaret olsaydı, bu projeye getirilecek bir şey kalmazdı.

██ ECS'in getirdiği asıl şey ikinci bir karar: aynı tipteki bileşenlerin
bellekte **BİTİŞİK** durması. ██

[BİRİNCİL] Entities 1.0.16 "Archetypes concepts", 2026-08-23'te doğrulandı:

- "All entities and components with the same archetype are stored in uniform
  blocks of memory called chunks. Each chunk consists of 16KiB"
- "A chunk contains an array for each component type, plus an additional array
  to store the entity IDs."
- Diziler "tightly packed": ilk varlık indeks 0'da, ikincisi 1'de, ardışık.

Yani bir *archetype* (aynı bileşen kümesine sahip varlıkların sınıfı) için
bellek şu şekle giriyor:

```
BİR CHUNK — 16 KiB
┌──────────────────────────────────────────────────────────────┐
│ Entity[]        e0 e1 e2 e3 ... eN                           │
│ Health[]        h0 h1 h2 h3 ... hN     ← BİTİŞİK, ardışık     │
│ AttackProfile[] a0 a1 a2 a3 ... aN     ← BİTİŞİK, ardışık     │
└──────────────────────────────────────────────────────────────┘
```

**Aritmetiği** — [ÇIKARIM], iki doğrulanmış sayıdan:

```
chunk boyu           16 KiB   = 16.384 bayt      [BİRİNCİL, Entities 1.0.16]
önbellek satırı      64 bayt                     [YEREL ÖLÇÜM + JobsUtility.CacheLineSize]
────────────────────────────────────────────────
bir chunk            256 önbellek satırı
4 baytlık bir bileşen (int can): satır başına  16 varlık
8 baytlık bir bileşen:                          8 varlık
16 baytlık bir bileşen:                         4 varlık
```

██ Bu çıkarımın sınırı, açıkça: ██ "satır başına 16 varlık" ancak döngü o diziyi
**baştan sona** okursa ve **yalnız o bileşene** dokunursa geçerlidir. İki bileşen
birden okunuyorsa iki ayrı dizi taranır. Ayrıca chunk'ın 16 KiB'ının tamamı
veri değil: bir başlık ve hizalama payı var, chunk kapasitesi bileşen sayısına
ve boyutuna göre değişiyor. Kesin kapasite formülü bu turda **[DOĞRULANMADI]**.

**Nesne modelinde ne oluyor** — bu projede:

```
Assets/Game/Battle/Battle.cs:59   private readonly Dictionary<Unit, Combatant> combatants =
Assets/Game/Battle/Battle.cs:66   private readonly Dictionary<Unit, Structure> structures =
Assets/Game/Battle/Battle.cs:81   private readonly Dictionary<Unit, Action<UnitState, UnitState>> stateForwarders =
Assets/Game/Unity/BoardAdapter.cs:199   private readonly Dictionary<Unit, UnitView> unitViews =
```

[YEREL ÖLÇÜM] 2026-08-23: `Unit` ile anahtarlanmış **dört** yan tablo var; üçü
`Battle.cs`'te, dördüncüsü `BoardAdapter.cs`'te.

Bir `Dictionary<Unit, Combatant>` üzerinden bir savaşçının canına ulaşmak için
atılan adımlar:

```
Unit referansı  ──► sözlüğün kova dizisinde karma araması
                ──► girdi düğümü (yönetilen yığında, yeri çalışma zamanının kararı)
                ──► Combatant nesnesi (ayrı bir yığın nesnesi)
                ──► Health nesnesi (ayrı bir yığın nesnesi)
                ──► int current
```

██ AYRIŞMA NOKTASI ██ — burada ayrışan iki şey: **tahsis bilinci** ile **bellek
yerleşimi bilinci**. Bu projede birincisi var ve yazılı — `Battle.cs:379-382`
sözlük üzerinde doğrudan `foreach` kullanmanın gerekçesini kutulama üzerinden
anlatıyor. İkincisi **hiç düşünülmedi** ve düşünülmemesi doğru bir karardı: iki
birim için düşünülecek bir yerleşim yok. Ama ikisi ayrı şeylerdir ve "sıfır
tahsis" ölçüsü yerleşim hakkında **hiçbir şey** söylemez.

Yığın nesnelerinin gerçekte nerede durduğu, canlılık ve yıkım tarafının tamamı:
[`../deep/dil/07-bellek-canlilik-ve-yikim.md`](../deep/dil/07-bellek-canlilik-ve-yikim.md).
O belge "bir `int` üç ayrı yerde" figürünü zaten çiziyor; bu bölüm onun üstüne
**dördüncü yeri** koyuyor: bir chunk içindeki bitişik dizi.

██ Ve o belgenin dürüst sınırı burada da geçerli: ██ bu projede hangi nesnenin
yığında nereye düştüğü **ölçülmedi** ve ölçülemez de — Memory Profiler bu
projede hiç kullanılmadı ([`03-kavram-borc-defteri.md`](03-kavram-borc-defteri.md),
"Profil çıkarma araçları" satırı `HENÜZ YOK`). Dolayısıyla "sözlük düğümleri
dağınık" cümlesi bir **mekanizma tarifidir**, bu depoda yapılmış bir ölçüm değil.
[DOĞRULANMADI]

---

## İkinci durak: DOTS = ECS + Job System + Burst — ve üçü AYRI şeyler

██ En pahalı yanlış model bu: "DOTS" tek bir şey sanılıyor. ██ Değil. Üç bağımsız
teknoloji, üç ayrı sorunu çözüyor, üçü ayrı ayrı kullanılabiliyor.

| | Ne çözer | Ötekiler olmadan işe yarar mı | Bu projede karşılığı |
|---|---|---|---|
| **Job System** | Aynı işi çok çekirdeğe **güvenli** dağıtmak | ██ EVET ██ — motorun çekirdeğinde, paket bile gerekmiyor | Yok: hiç iş parçacığı kullanılmıyor |
| **Burst** | Yazılmış kodu **hedef işlemciye özgü** yerel koda çevirmek | ██ EVET ██ — job'a da `static` metoda da uygulanabiliyor | Kurulu ama **kullanılmıyor**: sıfır `[BurstCompile]` |
| **ECS (Entities)** | Veriyi **bitişik** yerleştirmek ve sorguyla dolaşmak | ██ EVET ██ — ama en pahalısı, en çok şeyi değiştiren | Yok: kurulu değil, bu Editor sürümünde 1.0 kurulamıyor |

Üçünün ortak paydası bir **veri kısıtı**: üçü de yönetilen referansları sevmiyor.
Ama kısıtın sertliği farklı, ve fark aşağıda.

### Job System — güvenli paralellik. Ve `Task` DEĞİL.

[BİRİNCİL] Unity Manual 2021.3, "Job system overview", 2026-08-23'te doğrulandı:
"Unity's job system lets you create multithreaded code so that your application
can use all available CPU cores to execute your code." Ve kısıt: job'lar yalnız
"blittable data types" erişebilir, çünkü "these types don't need conversion when
passed between managed and native code."

[BİRİNCİL] Unity Manual, "The safety system in the C# Job System", 2026-08-23'te
doğrulandı (sayfa 2020.1 sürümünden; sürüm sınırı künyede yazılı): "A race
condition occurs when the output of one operation depends on the timing of
another process outside of its control." Çözüm: iş parçacığına ana iş
parçacığındaki veriye **referans** verilmiyor, verinin bir **kopyası**
gönderiliyor.

██ Burası [`05-yok-olan-mekanizmalar-csharp.md`](05-yok-olan-mekanizmalar-csharp.md)
ile DOĞRUDAN bağlanıyor ve o belge işin yarısını zaten yapmış. ██ Orada
"Dördüncü durak" `Task` · `Awaitable` · coroutine · iş parçacığının **dört ayrı
şey** olduğunu ayırıyor ve `await`in bir iş parçacığı **yaratmadığını** ölçüyor.
Job onların hiçbiri değil; **beşinci** bir şey. Farkı iki maddede:

```
Task / async      ► "bu iş BEKLEYECEK, ben bu arada başka şey yapayım"
                    Sorun: GECİKME. İş çoğu zaman CPU'da bile değil.
                    Veri paylaşımı programcının sorumluluğunda; derleyici
                    hiçbir yarışı görmez.

Job               ► "bu iş ÇOK, onu N çekirdeğe böleyim"
                    Sorun: İŞ HACMİ. İş kesinlikle CPU'da.
                    Veri paylaşımını SİSTEM denetler; yarış tespit edilir.
```

İkinci fark **sahiplik**tir ve adı `NativeArray<T>`. Bir `NativeArray` yönetilen
bir dizi değil: motorun yerel belleğinde duran, ne zaman serbest bırakılacağı
**açıkça** yazılan bir tampon. `int[]` bir job'a verilemez; `NativeArray<int>`
verilebilir. Ve verildiğinde sistem "bu tamponu kim okuyor, kim yazıyor" kaydını
tutar — iki job aynı tampona aynı anda yazmaya kalkarsa **zamanlama anında**
hata verir, çalışma zamanında sessizce bozulmaz.

[YEREL ÖLÇÜM] `JobsUtility.MaxJobThreadCount = 128` ve `CacheLineSize = 64`,
2021.3.45f2'nin `UnityEngine.CoreModule.dll`'inden okundu. İkinci sabit tesadüf
değil: paralel bir job'ta iki iş parçacığı **aynı önbellek satırına** yazarsa
(farklı elemanlar olsa bile) satır sürekli iki çekirdek arasında gidip gelir ve
paralellik kazanç yerine kayıp üretir. Bu olgunun adı **yanlış paylaşım (false
sharing)** ve `CacheLineSize` sabiti tam olarak ondan kaçınmak için var.

`HENÜZ YOK → yanlış paylaşımın ölçülmüş bir örneği; sahip: bu belgenin
"Genişletme merdiveni" 4. basamağı`.

### Burst — bir DERLEYİCİ. "Açınca hızlanır" değil.

[BİRİNCİL] Burst 1.8 belgesi, 2026-08-23'te doğrulandı: "Burst uses LLVM to
translate .NET Intermediate Language (IL) to code that's optimized for
performance on the target CPU architecture." Ve kapsamı: "originally designed
for use with Unity's job system" ama yalnız job'la sınırlı değil — `static`
metotları da derleyebiliyor, "as long as the code inside them belongs to the
supported subset of C#."

██ O son cümledeki "supported subset" bu bölümün tamamı. ██ Burst rastgele C#
derlemez; **HPC#** denen bir alt küme derler.

[BİRİNCİL] Burst 1.8 "HPC# overview" ve tip desteği sayfası, 2026-08-23'te
doğrulandı — yasaklar:

```
► "Burst is working on a subset of .NET that doesn't allow the usage of any
   managed objects/reference types in your code (class in C#)."
► "Any methods related to managed objects, for example, string methods."
► "Catching exceptions catch in a try/catch."
► "Storing to static fields except via Shared Static."
► Yönetilen diziler desteklenmiyor; yerine NativeArray<T>.
```

`throw` tarafı daha da ince: Burst "only supports simple throw patterns" ve
`[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]` koruması olmayan bir `throw`
uyarı üretiyor — çünkü Player derlemesinde bir istisna "always cause the
application to abort."

██ Yanlış modelin kaynağı tam olarak burası. ██ "Burst'ü açtım, hızlanmadı"
diyen kişi çoğu zaman haklıdır ve sebebi ölçülebilir: derlemeye **girmemiştir**.
Burst'ün derleyemediği bir metot sessizce yönetilen yoldan koşar. Yani Burst bir
düğme değil, bir **sözleşme**: kodu o alt kümeye taşımadan hiçbir şey olmaz.

Bu projede ölçülmüş hâli:

```
[YEREL ÖLÇÜM] com.unity.burst 1.8.18  ── kurulu (dolaylı bağımlılık)
[YEREL ÖLÇÜM] [BurstCompile] işaretli metot sayısı  ── 0
[YEREL ÖLÇÜM] Assets/Game/ altında struct sayısı    ── 0
[YEREL ÖLÇÜM] Assets/Game/ altında class sayısı     ── 26
```

██ Yani bugünkü kodun **hiçbiri** HPC# alt kümesine girmiyor. ██ 26 sınıf,
`string` alanlı bir kimlik tipi, `event`/`Action` delegeleri, `Dictionary`,
`ArgumentOutOfRangeException` fırlatan doğrulamalar — listenin tamamı Burst'ün
yasak listesinde. Bu bir kusur değil: bu proje bir tahta oyunu çekirdeği,
Burst'ün hedef kitlesi değil.

`HENÜZ YOK → SIMD ve otomatik vektörleştirme; sahip: bu belgenin
"Araştırılacaklar" bölümü, soru **S7**`.
`HENÜZ YOK → LLVM'in ne olduğu ve IL'den yerel koda dönüşümün aşamaları;
sahip: bu ağaç dışında bir sahip`.

### ECS — birinci durakta anlatıldı. Buradaki tek ek: kurulabilirlik.

██ [BİRİNCİL] ve bu belgedeki en sert tek olgu: ██

```
Entities 1.0.16 · overview sayfası, 2026-08-23'te doğrulandı:
   "To use the Entities package, you must have Unity version
    2022.3.0f1 and later installed."

Entities 0.51.1-preview.21 · installation sayfası, 2026-08-23'te doğrulandı:
   "You must use Unity Editor versions 2020.3.30+ or 2021.3.4+
    with entities 0.51."

Bu deponun sürümü [YEREL ÖLÇÜM]:  2021.3.45f2
```

Sonuç, çıkarım değil doğrudan okuma: **bu Editor sürümünde Entities 1.0 ve
sonrası kurulamaz.** Kurulabilen tek sürüm 0.51 önizlemesi — yani artık
geliştirilmeyen, API'si 1.0'da baştan değişmiş bir dal. Bu, merdivenin son
basamağına bir **ön koşul** ekliyor ve o ön koşul kod değil: Editor sürümü
yükseltmesi.

### ██ Ölçüsüz övgü yasağı ██

Bu belgede "N kat hızlı" biçiminde **tek bir sayı yok** ve bu bir eksiklik değil,
bir kural. `performance-research` kuralı üç şeyi ayırmayı istiyor: yerel ölçüm,
üst kaynak rehberliği, çıkarım. Bir hızlanma oranı bu üçünden hiçbirine tek
başına ait değildir; **bir iş yüküne ve bir donanıma** aittir.

Bir hızlanma sayısının yazılabilmesi için gereken beş şey:

```
① hangi işlemci ve kaç çekirdek        ② hangi Unity sürümü ve Mono mu IL2CPP mu
③ hangi iş yükü, kaç varlık             ④ öncesi ve sonrası AYNI koşulda mı
⑤ ölçüm hangi kanıt kovasında           (EditMode / PlayMode / hedef cihaz)
```

Beşi de yazılmadan bir oran yazmak, okuyucuya **yanlış bir beklenti** kurar ve
bu belgenin en pahalı hatası olurdu. Mentor arşivi de aynı yerde duruyor:
"Reject answers that begin with 'I always use pooling/Jobs/Burst' without a
measured problem."

---

## Üçüncü durak: bu proje ECS'e ne kadar yakın — dürüst ölçüm

Dört satır, ve dördü de koda karşı doğrulandı.

| | BU PROJE | ECS |
|---|---|---|
| **kimlik** | `Unit` nesnesi — referans eşitliği | `Entity` struct — `Index` + `Version` |
| **bileşen depolama** | `Dictionary<Unit, X>` — dört adet, dağınık | chunk içinde bitişik dizi, 16 KiB blok |
| **davranış** | `static` kural sınıfı, tek şeye cevap verir | system, **sorgu** ile küme üstünde koşar |
| **döngüyü kim çevirir** | `Battle.Tick`, elle yazılmış iki `foreach` | zamanlayıcı, sıralı sistem grupları |

██ Her satırın kanıtı: ██

**Satır 1 · kimlik.** `Unit.cs:41` bir `public sealed class Unit` ve tek üyesi
`Unit.cs:56`'daki `public string Name { get; }`. `Equals`/`GetHashCode`
geçersiz kılınmamış — anahtar referans eşitliği. ECS tarafı birincil kaynakla
doğrulandı (yukarıda, `Entity` API).

**Satır 2 · depolama.** [YEREL ÖLÇÜM] 2026-08-23, `Unit` ile anahtarlanmış yan
tablo **sayıldı: dört**.

```
Assets/Game/Battle/Battle.cs:59         private readonly Dictionary<Unit, Combatant> combatants =
Assets/Game/Battle/Battle.cs:66         private readonly Dictionary<Unit, Structure> structures =
Assets/Game/Battle/Battle.cs:81         private readonly Dictionary<Unit, Action<UnitState, UnitState>> stateForwarders =
Assets/Game/Unity/BoardAdapter.cs:199   private readonly Dictionary<Unit, UnitView> unitViews =
```

Üçü `Battle.cs`'te, dördüncüsü Unity katmanında. Dördüncüsünün ayrı yerde olması
tesadüf değil: `UnitView` bir motor tipi ve `Battle` motoru tanımıyor. ECS'te bu
dördü de aynı chunk şemasının parçası olurdu ve **assembly duvarı** o gün
yeniden çizilirdi.

Beşinci bir tablo daha var ve ECS tarafında **karşılığı yok**: tahtanın kendisi.

```
Assets/Game/Core/UnitGrid.cs:26   public sealed class UnitGrid
Assets/Game/Core/UnitGrid.cs:28   private readonly Unit[,] cells;
```

██ Bu, bu projedeki **tek bitişik depo**. ██ `Unit[,]` gerçekten ardışık bir
bellek bloğu — ama içindeki şey veri değil, **referans**. Yani bitişik olan
oklar; okların gösterdiği nesneler değil.

**Satır 3 · davranış.** Dört `static class` yukarıda listelendi. Hiçbirinin alanı
yok, hiçbiri bir küme dolaşmıyor.

**Satır 4 · döngü.** `Battle.cs:377`'deki `Tick` iki `foreach` içeriyor
(`:383` ve `:394`) ve neyi dolaşacağını sabit biliyor. Motor tarafındaki tek
çağıran:

```
Assets/Game/Unity/BoardAdapter.cs:317   private void Update()
Assets/Game/Unity/BoardAdapter.cs:625   private void AdvanceBattleTime()
Assets/Game/Unity/BoardAdapter.cs:627   battle.Tick(Time.deltaTime);
```

██ SONUÇ — dürüst cümle: ██ bu proje ECS'in **mimari** yarısını bütünüyle
yapmış, **yerleşim** yarısını hiç yapmamış durumda. Ve bu tam olarak doğru
karardır, çünkü ikinci yarının bedeli birinci yarının kazancıyla ödenmiyor;
ancak varlık sayısıyla ödeniyor. Varlık sayısı iki.

---

## Dördüncü durak: tetikleyici — ve bu oyunda eşiğe ULAŞILMAZ

Tetikleyici koşulun kendisi zaten yazılı ve burada **tekrar edilmiyor**:
[`02-sonraki-asamalar.md`](02-sonraki-asamalar.md) · Aşama 5, B alanı. Bu bölüm
o satırı **sayıyla sertleştiriyor**.

### Bugünkü ölçü

```
Assets/Game/Unity/BoardAdapter.cs:113   [SerializeField, Min(1)] private int width = 3;
Assets/Game/Unity/BoardAdapter.cs:114   [SerializeField, Min(1)] private int height = 5;
Assets/Game/Unity/BoardAdapter.cs:267   SpawnUnit("Vanguard", Team.Player, 1, 2);
Assets/Game/Unity/BoardAdapter.cs:268   SpawnUnit("Raider", Team.Enemy, 1, 3);
```

```
tahta               3 × 5   =  15 hücre
tahtadaki parça                 2 birim, 0 yapı
kare başına dolaşılan varlık    2
```

Ve o iki varlığın kare başına yaptığı iş de ölçülü. `Battle.Tick` her savaşçının
`Combatant.Tick`'ini çağırıyor (`Combatant.cs:204`), o da `UnitLifecycle.Tick`e
iniyor (`UnitLifecycle.cs:176`) ve orada ilk kontrol şu:

```
Assets/Game/Core/Combat/UnitLifecycle.cs:188   if (State == UnitState.Alive)
```

██ Yani hiçbir şey olmayan bir karede kare başına iş: iki `enum` karşılaştırması
ve iki erken dönüş. ██ Bu bir abartma değil, kodun okunuşu.

### Eşik ne kadar uzak

```
BUGÜN            2 varlık × ~2 karşılaştırma
ECS'in kazandığı eşik   binlerce varlık × kare başına gerçek iş
                        (konum, çarpışma, ömür, hedefleme — her karede)
mesafe                  ██ üç ila dört büyüklük mertebesi ██
```

██ Ve asıl cümle: **sıra tabanlı bir tahta oyununda bu eşiğe ulaşılmaz.** ██
Sebebi tahtanın küçük olması değil — tahta büyütülebilir. Sebebi **tür**: sıra
tabanlı bir oyunda kare başına yapılan iş, tanımı gereği, oyuncunun düşünme
süresi boyunca **sıfıra yakındır**. İş, oyuncu bir hamle yaptığında bir kez
patlar ve biter. ECS'in optimize ettiği şey ise "aynı işi her karede binlerce kez
yapmak". Bu iki profil birbirinin karşıtı.

Bu proje bir gün gerçek zamanlı bir savaşa dönerse cümle değişir. Bugünkü hâliyle
değişmez.

### İkinci sert engel: sürüm duvarı

Yukarıda birincil kaynaktan okundu: Entities 1.0 **2022.3.0f1** ve sonrasını
istiyor; bu depo **2021.3.45f2**. Yani "ECS'i deneyelim" cümlesinin ilk adımı
kod değil:

```
① Editor sürümünü yükselt (2021.3 → 2022.3 LTS ya da üstü)
   ── ve bu tek başına bir tur: paket uyumu, API değişiklikleri, testlerin
      yeniden koşturulması
② VEYA Entities 0.51 önizlemesiyle çalış
   ── artık geliştirilmeyen bir dal; öğrenilen API 1.0'da geçersiz
```

██ İkinci yol bir öğrenme egzersizi için bile **kötü** bir seçim: ██ öğrenilen
şeyin bugünkü karşılığı yok. Birinci yol doğru ama pahalı — ve pahalı olduğu
için merdivenin **en sonunda** duruyor.

### ██ İYİLEŞTİRME mi, ÖĞRENME EGZERSİZİ mi ██

Bu ayrım bu belgenin omurgası ve karıştırılması en pahalı hata:

```
İYİLEŞTİRME             ölçülmüş bir darboğaz var
                        değişiklikten sonra AYNI ölçüm tekrarlanır
                        başarı ölçüsü: sayı düştü mü
                        bu projede bugün: ██ ÖLÇÜLMÜŞ DARBOĞAZ YOK ██

ÖĞRENME EGZERSİZİ       ölçülmüş bir öğrenme hedefi var
                        değişiklikten sonra "ne gördüm" sorulur
                        başarı ölçüsü: kavram görünür oldu mu
                        bu projede bugün: ██ GEÇERLİ ██ — ve merdiveni bu yazıyor
```

Mentor kuralları burada birlikte uygulanıyor. **K22 (tam zamanında)**: bir
mekanizma ihtiyaç doğmadan öğretilmez — bu yüzden bu belge ECS'i *anlatıyor* ama
*getirmiyor*. **K43 ("önemli değil" eksik cümledir)**: bu yüzden her bölümde
"ne zaman önemli olur" satırı var ve o satır ölçülü.

---

## Beşinci durak: genişletme merdiveni — projeyi nasıl büyütürüz

██ Bu bölüm operatörün açık talebi: "o kısımları gerçekten anlayabilmem için
projenin genişletilmesi gerekiyorsa ona göre genişletiriz." ██

Yedi basamak, artan zorlukta. Her basamak altı alan taşıyor:

```
NE EKLENİR          somut: kaç birim, hangi sistem, hangi dosya
GÖRÜNÜR KILAR       ██ esas nokta ██ — hangi kavram gözle görülür hâle gelir
ÖLÇÜM               hangi sayaç, hangi kanıt kovası (EditMode/PlayMode/cihaz)
KIRDIĞI KARAR       bugünkü hangi karar geçersiz olur
MEVCUT KODA DOKUNUR MU
GERİ DÖNÜŞ          nasıl geri alınır
```

### Basamak 0 · Ölçüm aygıtı — ██ ÖN KOŞUL, atlanamaz ██

**NE EKLENİR** — Kod değil. `Assets/Tests/EditMode/Combat/DamageRulesAllocationTests.cs`
zaten tahsis ölçüyor (`:69` negatif kontrol, `:103` sıcak yol). Eksik olan
**süre** ölçümü ve **Profiler penceresi**. Bu basamak: Play'e basıp Profiler'ı
açmak, `Battle.Tick`in kare bütçesindeki payını **bir kez** görmek.

**GÖRÜNÜR KILAR** — "Kare bütçesi" diye bir şeyin var olduğu. Bugün bu projede
kare bütçesi hiç bakılmamış bir kavram; `03-kavram-borc-defteri.md`'de
`HENÜZ YOK`.

**ÖLÇÜM** — Profiler → CPU Usage → `BehaviourUpdate` altında `BoardAdapter.Update`.
Kanıt kovası: **PlayMode**, yani sahne kanıtı. Hedef cihaz kanıtı değil.

**KIRDIĞI KARAR** — Hiçbiri. `02-sonraki-asamalar.md` Aşama 6 bunu zaten
yazıyor: ölçüm, üretim kodunu değiştirmeyen tek maddedir.

**MEVCUT KODA DOKUNUR MU** — ██ Hayır. ██ Tek satır değişmez.

**GERİ DÖNÜŞ** — Gerekmiyor.

██ Bu basamak neden 0 numaralı: ██ 1'den 6'ya kadarki her basamağın "ÖLÇÜM"
satırı bunun var olduğunu varsayıyor. Ölçüm aygıtı olmadan merdiven bir
**hikâyeye** dönüşür.

### Basamak 1 · Tahtayı büyüt — `3×5` → `50×50`

**NE EKLENİR** — Tek bir Inspector değeri. `BoardAdapter.cs:113` ve `:114`
alanları `[SerializeField]`, yani kod derlemeden değiştirilebiliyor.

**GÖRÜNÜR KILAR** — ██ Tam tarama maliyeti. ██ `Battle.cs:528`'deki
`TryGetPosition` tahtayı `Width × Height` tarıyor:

```
Assets/Game/Battle/Battle.cs:528   public bool TryGetPosition(Unit unit, out int x, out int y)
Assets/Game/Battle/Battle.cs:535   for (int cellX = 0; cellX < board.Width; cellX++)
```

```
bugün      3 × 5    =     15 hücre
50 × 50             =  2.500 hücre      ── 166 kat
200 × 200           = 40.000 hücre      ── 2.666 kat
```

██ Bu, `03-kavram-borc-defteri.md`'deki tek ilgili **KISMİ** satırın tam
karşılığı: ██ "Tarama maliyeti ve karmaşıklık — EKSİK: karmaşıklık gösterimi ve
ölçüm; bugün tahta 15 hücre." Bu basamak o eksiği kapatır.

**ÖLÇÜM** — ██ Ve burada dürüst bir düzeltme gerekiyor: ██ [YEREL ÖLÇÜM]
2026-08-23, `TryGetPosition`ın üretim kodundaki çağıranları sayıldı — **iki
tane**, ve ikisi de kare başına yolda **değil**:

```
Assets/Game/Battle/Battle.cs:331        if (TryGetPosition(unit, out int x, out int y))
Assets/Game/Battle/BattleActions.cs:392 if (!battle.TryGetPosition(unit, out x, out y))
```

Birincisi birim çıkarılırken, ikincisi bir eylem denenirken. Yani tahtayı
büyütmek **kare başına** maliyeti değil, **tıklama başına** maliyeti ölçülebilir
kılar. Bu bir kusur değil ama ölçümün adı doğru konmalı: bu bir *gecikme*
ölçümüdür, bir *kare bütçesi* ölçümü değil. `O(n²)` bir taramanın ne demek
olduğunu göstermeye fazlasıyla yeter — ve ECS'le ilgisi yoktur.

**KIRDIĞI KARAR** — İki tanesi:
① `01-koda-gomulu-desenler.md` §9'daki **REDDEDİLEN** karar geri gelir: konum
sözlüğü. O gün "tek yazma kapısı" zorunlu hâle gelir.
② `Assets/Tests/EditMode/` altındaki tahta boyutuna bağlı testler — 26 test
dosyası var ve bir kısmı sabit koordinat kullanıyor. Tahta büyürken **testler
kırılmaz** (büyük tahtada küçük koordinatlar hâlâ geçerli), ama tahta
küçültülürse kırılır.

**MEVCUT KODA DOKUNUR MU** — ██ Hayır. ██ Inspector değeri.

**GERİ DÖNÜŞ** — İki sayıyı geri yaz.

### Basamak 2 · Birim sayısını artır — 2 → 200 → 2.000

**NE EKLENİR** — `BoardAdapter.cs:267-268`'deki iki `SpawnUnit` çağrısı yerine
bir döngü. ██ Bu basamak mevcut koda **dokunuyor** — merdivenin ilk dokunan
basamağı. ██

**GÖRÜNÜR KILAR** — Üç şey birden:
① **Yaratma maliyeti.** `BoardAdapter.cs:739`'daki `Instantiate` bugün yalnız
iki kez çalışıyor. 2.000 kez çalıştığında yükleme süresi **görünür** olur — ve
`02-sonraki-asamalar.md` Aşama 2'nin (nesne havuzu) tetikleyici koşulu tartışma
konusu hâline gelir.
② **Sözlük büyümesi.** Dört yan tablo 2.000 girdiye çıkar. `Dictionary`'nin
yeniden boyutlandırma davranışı ölçülebilir olur.
③ ██ **Ve asıl olan: `Battle.Tick`in kare başına dolaştığı varlık sayısı** ██ —
2'den 2.000'e. Bu, ECS tartışmasının **tek gerçek girdisidir**.

**ÖLÇÜM** — Profiler'da `BoardAdapter.Update` → `Battle.Tick`. Kanıt kovası:
PlayMode. ██ Ve beklenen sonuç dürüstçe yazılmalı: ██ 2.000 varlıkta bile
`Tick`in yaptığı iş `UnitLifecycle.cs:188`'deki erken dönüş yüzünden neredeyse
sıfır kalacaktır. Yani bu basamak tek başına ECS'i **haklı çıkarmaz** — ve bunu
görmek basamağın asıl dersidir.

**KIRDIĞI KARAR** — İki tanesi:
① `BoardAdapter.cs:1007`'deki `Destroy` ve `UnitView`'un `Awake` sözleşmesi —
2.000 görsel için havuz tartışması gerçekten açılır.
② Tahta kapasitesi: 2.000 birim en az `2.000` hücre ister, yani Basamak 1 bunun
**ön koşuludur**.

**MEVCUT KODA DOKUNUR MU** — ██ Evet, iki satır. ██ Ama dokunuş yalıtılabilir:
iki `SpawnUnit` çağrısı bir `[SerializeField] private int demoUnitCount` ile
sarılabilir ve varsayılan 2 kalabilir. O gün mevcut davranış **bit düzeyinde**
korunur.

**GERİ DÖNÜŞ** — Sayaç 2'ye döner.

### Basamak 3 · Kare başına gerçek iş ekle

**NE EKLENİR** — Bugün hareket ışınlanma: bir birim bir karede A'dan B'ye
geçiyor. Bu basamak **sürekli hareket** ekliyor — birim hedefe doğru her karede
biraz ilerliyor.

**GÖRÜNÜR KILAR** — ██ Merdivenin en önemli basamağı bu. ██ Basamak 2'nin
gösterdiği şey "2.000 varlık var ama iş yok"tu. Bu basamak **işi** koyuyor:

```
öncesi   kare başına iş = 2.000 × (bir enum karşılaştırması)
sonrası  kare başına iş = 2.000 × (konum güncelle + hedef kontrol + sınır kontrol)
```

██ İşte ECS'in optimize ettiği profil tam olarak budur ██ ve bu basamak onu ilk
kez **bu projede** üretiyor. Buraya gelmeden ECS hakkında yapılacak her ölçüm
boş bir aralığı ölçer.

**ÖLÇÜM** — Profiler'da `Battle.Tick`in milisaniye payı. Kanıt kovası: PlayMode,
ve ██ burada ilk kez **hedef cihaz** kovası anlamlı hâle gelir ██ — bu makine
(16 çekirdek, 64 MB L3) darboğazı **gizler**.

**KIRDIĞI KARAR** — Üç tanesi, ve üçü de büyük:
① **Sıra tabanlılık.** Sürekli hareket, sıra tabanlı bir oyunda ne demek?
`TurnRules.cs:59`'daki `CanAct` bir hamlenin **anlık** olduğunu varsayıyor.
② **`MoveAction`ın ışınlanma sözleşmesi.** `MoveAction.cs:27` yolun üzerinde ne
olduğunu bilmediğini açıkça yazıyor; sürekli hareket o cümleyi geçersiz kılar ve
yol bulma (`03-kavram-borc-defteri.md`'de `HENÜZ YOK`) gündeme girer.
③ **EditMode test yüzeyi.** Zamana yayılan bir hareket, tek bir `Tick`te bitmez;
testler `Tick`i döngüde çağırmak zorunda kalır.

**MEVCUT KODA DOKUNUR MU** — ██ Evet, derinden. ██ Bu basamak bir "ayar" değil,
bir oyun tasarımı değişikliği. ██ Merdivende geri dönüşü en zor basamak budur. ██

**GERİ DÖNÜŞ** — Ayrı bir dalda yapılmalı. Bu basamak bir Inspector değeriyle
geri alınamaz.

### Basamak 4 · Job System — ██ TEK BAŞINA, ECS'siz, Burst'süz ██

**NE EKLENİR** — Basamak 3'teki konum güncellemesi bir `IJobParallelFor`a taşınır.
Veri bir `NativeArray<float2>`e kopyalanır, job koşar, sonuç geri yazılır.

**GÖRÜNÜR KILAR** — Dört şey ve dördü de bu belgenin ikinci durağının konusu:
① ██ **DOTS'un üç parçasının gerçekten ayrılabildiği.** ██ ECS yok, Entities
paketi yok, Editor sürümü yükseltilmedi — ve yine de paralellik var.
② `NativeArray` sahipliği: `Dispose` kimin işi, `Allocator.TempJob` ne demek.
③ Güvenlik sisteminin **kızması**: iki job aynı diziye yazdırılmaya çalışılınca
zamanlama anında hata. Bunu bir kez görmek, "veri yarışı" kelimesini kalıcı
olarak öğretir.
④ ██ Yanlış paylaşım (false sharing) ██ — `JobsUtility.CacheLineSize = 64`
sabitinin neden var olduğu, ancak burada görünür.

**ÖLÇÜM** — Aynı iş yükü, aynı varlık sayısı, tek iş parçacığı ve job hâli
yan yana. Kanıt kovası: PlayMode ve hedef cihaz. ██ Ve bu makinede ölçülen kazanç
**yanıltıcı olacak**: ██ 16 çekirdek, tipik bir hedef cihazda yok.

**KIRDIĞI KARAR** — İki tanesi:
① ██ `noEngineReferences: true` ██ — [YEREL ÖLÇÜM] `GridStrategy.Core`,
`GridStrategy.Combat` ve `GridStrategy.Battle` asmdef'lerinin üçü de bu satırı
taşıyor. `Unity.Jobs` ve `Unity.Collections` motor çekirdeğinde, yani bir job
yazıldığı gün ilgili asmdef motoru **tanımak zorunda** kalır. Duvarın faturası:
[`../deep/konular/02-assembly-duvari.md`](../deep/konular/02-assembly-duvari.md).
② EditMode testlerinin sahnesiz koşabilmesi — job zamanlaması motor bağımlı.

██ Kaçış yolu var ve önemli: ██ job'lar yalnız Unity katmanında (`GridStrategy.Unity`,
`noEngineReferences: false`) yazılırsa duvar ayakta kalır. O gün çekirdek saf C#
kalır, paralellik motor tarafında yaşar. ██ Bu, merdivendeki en öğretici mimari
karardır ve ECS'e hiç gerek duymaz. ██

**MEVCUT KODA DOKUNUR MU** — Basamak 3'ün eklediği koda dokunur; bugünkü koda
hayır.

**GERİ DÖNÜŞ** — Job çağrısı yerine düz döngü. İkisi yan yana **bırakılabilir**
ve bir bayrakla seçilebilir — ölçüm için doğru olan da budur.

### Basamak 5 · Burst — ██ TEK BAŞINA, Basamak 4'ün üstüne ██

**NE EKLENİR** — Basamak 4'teki job'a bir `[BurstCompile]` niteliği. Başka
hiçbir şey.

**GÖRÜNÜR KILAR** — ██ "Burst açılınca hızlanır" yanlış modelinin ölümü. ██
İki sonuçtan biri çıkar ve ikisi de öğretir:
① Job zaten HPC# alt kümesindeyse derlenir ve bir fark ölçülür.
② Bir yerde bir `string`, bir `class`, bir `try`/`catch` varsa ██ derlenmez ve
hiçbir şey olmaz ██ — Burst Inspector penceresinde bunun **neden** olmadığı
okunabilir.

██ İkinci sonuç birincisinden daha değerli. ██ Bir kez görüldüğünde "Burst bir
düğme değil bir sözleşme" cümlesi ezber olmaktan çıkar.

**ÖLÇÜM** — Aynı job, Burst açık ve kapalı. Kanıt kovası: PlayMode ve ██ özellikle
hedef cihaz ██ — Burst'ün kazancı işlemci mimarisine bağlı ve bu makinedeki sayı
bir telefonu temsil etmez.

**KIRDIĞI KARAR** — Bir tanesi ve ince: `com.unity.burst` bugün **dolaylı** bir
bağımlılık (`"depth": 3`). Kullanılmaya başlandığı gün `manifest.json`'a
**doğrudan** yazılması gerekir; yoksa 2D paketleri kaldırıldığında Burst sessizce
gider.

**MEVCUT KODA DOKUNUR MU** — Hayır.

**GERİ DÖNÜŞ** — Niteliği sil. Tek satır.

### Basamak 6 · ██ Aynı mantığı ECS ile İKİNCİ KEZ yaz ██

**NE EKLENİR** — Editor sürümü yükseltmesi (2022.3+, birincil kaynaktan zorunlu),
`com.unity.entities`, ve Basamak 3'teki hareket mantığının **ikinci bir
uygulaması**: `IComponentData` struct'ları, bir `ISystem`, bir sorgu.

██ Ve buradaki tek doğru yöntem: ██ eskisini **silmeden**. İki uygulama yan yana
durur, aynı iş yükünü koşar, aynı Profiler penceresinde ölçülür.

**GÖRÜNÜR KILAR** — Beş şey:
① Bir varlığın gerçekten bir **sayı** olması (`Index` + `Version`).
② Bileşenin `struct` ve unmanaged olmak **zorunda** olması — 26 sınıfın hiçbiri
olduğu gibi taşınamaz.
③ Bir sorgunun bir `foreach`ten farkı.
④ Zamanlayıcının döngüyü çevirmesi; `[UpdateInGroup]` ile sıra kararı.
⑤ ██ Ve ölçülen fark. ██ Yan yana ölçüm olmadan bu basamağın hiçbir değeri yok.

**ÖLÇÜM** — İki uygulama, aynı varlık sayısı, aynı sahne, aynı derleme, aynı
ısınma. Mentor arşivindeki havuz karşılaştırma sözleşmesi burada da geçerli:
kalite kazananı, hız kazananı, bellek kazananı ve varsayılan kazananı **ayrı
ayrı** raporlanır.

**KIRDIĞI KARAR** — ██ Neredeyse hepsi. ██ `02-sonraki-asamalar.md` Aşama 5'in
D alanı bunu zaten sayıyor: okunabilir nesne modeli, 26 EditMode test dosyasının
kurulum yarısı, `noEngineReferences` duvarı, `Docs/deep/kod/` altındaki 33 ayna
belgenin tip başına bölünmesi.

Buraya bir tane daha eklenir: ██ Editor sürümü yükseltmesi kendi başına bir
turdur ██ ve bu depodaki dört makine kapısı (`check-doc-code-refs.py`,
`check-doc-links.py`, `check-curriculum-coverage.py`, `check-cross-file-refs.py`)
ile 26 test dosyası yükseltmeden sonra yeniden koşturulmalıdır.

**MEVCUT KODA DOKUNUR MU** — Hayır, eğer ikinci uygulama **ayrı bir asmdef**te
yaşarsa. ██ Ve yaşamalıdır. ██

**GERİ DÖNÜŞ** — İkinci asmdef silinir. Editor sürümü geri alınmaz — o karar tek
yönlüdür ve merdivenin en sonunda olmasının sebebi budur.

### Merdivenin özeti

| # | Basamak | Görünür kılar | Mevcut koda dokunur | Geri dönüş |
|---|---|---|---|---|
| 0 | Ölçüm aygıtı | kare bütçesi diye bir şeyin varlığı | hayır | gerekmez |
| 1 | Tahta `50×50` | tam tarama maliyeti, `O(n²)` | hayır | iki sayı |
| 2 | 2.000 birim | varlık sayısı ≠ iş miktarı | iki satır | sayaç |
| 3 | Kare başına iş | ██ ECS'in optimize ettiği profil ██ | evet, derinden | ayrı dal |
| 4 | Job System | üç parçanın ayrılabilirliği, veri yarışı | hayır | bayrak |
| 5 | Burst | "düğme değil sözleşme" | hayır | tek satır |
| 6 | ECS, ikinci uygulama | yerleşim farkı — ölçülerek | hayır (ayrı asmdef) | asmdef sil |

██ Merdivenin okunma kuralı: ██ 0 → 1 → 2 → 3 zorunlu sıradır, çünkü 3 olmadan
4, 5 ve 6'nın ölçeceği bir iş yükü yoktur. 4 ve 5 birbirine bağlıdır. 6 ise
**hiçbir zaman zorunlu değildir** — 0'dan 5'e kadar giden biri DOTS'un üç
parçasından ikisini eliyle çalıştırmış, üçüncüsünü de neden ertelediğini ölçüyle
biliyor olur.

---

## Altıncı durak: gerçek dünya — kim kullanıyor, kim KULLANMIYOR

██ Bu bölümün kuralı sert: uydurma vaka çalışması bu belgenin en pahalı hatası
olurdu. ██ Aşağıda yalnız doğrulanabilen adlar var, ve doğrulanamayanlar
işaretli.

### Doğrulanmış — Unity'nin kendi vaka listesi

[BİRİNCİL, kısmi] 2026-08-23'te doğrulandı. ██ Kaynak sınırı: ██ `unity.com`
alan adı bu turda doğrudan getirilemedi (sunucu 403 döndü); aşağıdaki adlar ve
cümleler **arama dizini üzerinden** okundu. Yani ad ve iş doğrulandı, sayfanın
kendisi elle açılmadı.

```
V Rising            Stunlock Studios   açık dünya, çok oyunculu hayatta kalma
                    unity.com/case-study/v-rising
                    ECS geliştirmenin tamamında kullanılmış; alt sahne (subscene)
                    ve varlık akışı ile açık dünya ölçeklenmiş
Zenith: The Last City  Ramen VR         VR MMO — oynanışın ölçeklenmesi
Detonation Racing   Electric Square     Apple Arcade yarış — belirlenimci oynanış
IXION               Kasedo Games        şehir kurma / hayatta kalma — ağır NPC simülasyonu
```

██ Ve bu listedeki tek **maliyet** sayısı, ki en öğretici olan da o: ██ Stunlock
ekibi Entities 1.0'a geçerken V Rising'in kurulum (authoring) sistemini altı ayda
sıfırdan yeniden yazdı — yaklaşık 1.000 `IConvertGameObjectToEntity` ve 140
`GameObjectConversionSystem`. [BİRİNCİL, kısmi] aynı 403 sınırıyla.

██ Bu sayı bir başarı hikâyesi değil, bir **fiyat etiketi**. ██ ECS'in bedeli bir
kütüphane eklemek değil; kurulum yolunun tamamının yeniden yazılması. Bu depoda
karşılığı: 26 sınıf, 26 EditMode test dosyası, üç asmdef.

### ██ KARŞI ÖRNEK — binlerce varlık, ECS YOK ██

██ Bu, bölümün en değerli satırı, çünkü sezgiyi kırıyor. ██

`02-sonraki-asamalar.md`'nin ÜÇ OYUN satırları Vampire Survivors'ı defalarca
"eşiğin gerçekten aşıldığı yer" diye işaretliyor — ve bu **iş profili** olarak
doğru. Ama iş profilinin doğru olması, çözümün ECS olduğu anlamına gelmiyor.

```
[İKİNCİL KAYNAK, doğrulanmadı] Vampire Survivors'ın ilk sürümü Phaser ile
   (HTML5 / JavaScript) yazıldı; sonra Unity'ye taşındı.
   Kaynaklar ikincil (ansiklopedi ve oyun basını); birincil doğrulama
   YAPILAMADI.

[DOĞRULANABİLİR OLGU] Unity'nin resmî ECS vaka listesinde Vampire Survivors
   ██ YOK ██. Listede V Rising, Zenith, Detonation Racing ve IXION var.
```

██ Sonuç, dikkatli hâliyle: ██ ekranda binlerce varlığın bulunması ECS'i
**gerektirmez**. Bir HTML5 motorunda çalışabilen bir oyun için Unity'nin
Entities paketi zorunlu değildir. ECS bir **seçenektir**, bir eşik geçildiğinde
otomatik olarak doğru olan bir cevap değil.

Bu, aynı zamanda merdivenin Basamak 3'ünün neden 6'dan önce geldiğini açıklıyor:
iş yükünü üretmeden hangi çözümün gerektiği bilinemez, ve iş yükü üretildiğinde
cevap ECS **olmayabilir**.

### Oyun türüne göre eksen — daha güvenli çerçeve

Stüdyo adı vermek yerine **profil** vermek daha güvenli, çünkü profil
doğrulanabilir bir şeydir:

```
ECS'İN KAZANDIĞI PROFİL                    ECS'İN KAZANDIRMADIĞI PROFİL
────────────────────────────────           ──────────────────────────────────
binlerce+ varlık                           onlarca varlık
her karede AYNI işi yapıyorlar             iş oyuncunun tıklamasıyla patlıyor
iş belirlenimci ve veri-yoğun              iş dallanmalı ve karar-yoğun
varlıklar birbirine az bağlı               varlıklar karmaşık ilişkilerde
                                           ── bullet-heaven, büyük simülasyon,
                                           ── kart oyunu, tahta oyunu, sıra
   kalabalık RTS, parçacık dünyaları          tabanlı taktik, bulmaca, anlatı
```

██ Bu proje sağ sütunda ve orada kalması bir eksiklik değil, tür kararının
sonucu. ██

### ECS'in NE KAZANDIRMADIĞI — sağ sütundaki bir oyun için

```
✗ Daha az kod                — daha çok kod: bileşen, sistem, bakma (baking)
✗ Daha okunur kod            — `Combatant.cs:152`'deki tek satırlık property
                               yerine bileşen araması
✗ Daha kolay test            — 26 EditMode dosyası `new` ile nesne kuruyor;
                               ECS'te bir World kurmak gerekir
✗ Daha az hata               — indeks + sürüm modeli yeni bir hata sınıfı
                               getirir: bayat `Entity` değeri
✗ Daha hızlı yükleme         — bakma (baking) adımı yeni bir maliyet
✓ Kazandırdığı tek şey       — çok sayıda varlık üstünde ÖLÇÜLMÜŞ bir
                               kare zamanı kazancı. Sağ sütunda o ölçüm yok.
```

---

## Üç oyun — "varlık sayısı ve kare başı iş nasıl ölçekleniyor"

██ Yalnız ad ve iş; oynanış anlatısı yok. ██ Doğrulanmamış satırlar işaretli.

**Slay the Spire** — Ekranda onlarca eleman: eldeki kartlar, birkaç düşman, bir
avuç kalıntı. Kare başına iş, oyuncu bir kart oynayana kadar sıfıra yakın; oynadığı
anda bir zincir çalışır ve biter. ██ Varlık sayısı sabit, kare başı iş olay
güdümlü. ██ ECS'in kazanacağı hiçbir şey yok — bu profil bu projenin profili.

**Vampire Survivors** — ██ EŞLEŞMEYEN, ama ters yönde. ██ Ekranda aynı anda
yüzlerce/binlerce düşman ve mermi; her karede konum, çarpışma ve ömür güncelleniyor.
██ Varlık sayısı büyüyor VE kare başı iş varlık sayısıyla çarpılıyor — ECS'in
optimize ettiği tek profil budur. ██ Eşleşmemesinin sebebi: bu belgede anlatılan
her şeyin **karşıt örneği**, ve yukarıda ölçüldüğü gibi bunu ECS **kullanmadan**
yapıyor. [İKİNCİL KAYNAK — motor geçmişi doğrulanmadı; ECS kullanmadığı ise
Unity'nin resmî vaka listesinde bulunmamasıyla dolaylı olarak destekleniyor]

**Stardew Valley** — Kasabada yüzlerce varlık (ekin, hayvan, köylü, eşya) ama
çoğunun işi **günde bir kez** çalışıyor: gün sonu hesabı. Kare başına iş, ekranda
görünen küçük bir alt kümeyle sınırlı. ██ Varlık sayısı büyük, kare başı iş
küçük — ikisi ayrışıyor. ██ Bu ayrışma ECS'in eşiğini **uzaklaştırıyor**: çok
varlık tek başına yeterli değil, işin **her karede** tekrarlanması gerekiyor.
[DOĞRULANMADI — bu üç oyunun hiçbirinin iç mimarisi birincil kaynakla
doğrulanmadı; satırlar oyunun gözlemlenebilir davranışından çıkarıldı]

---

## Araştırılacaklar — o gün açılacak sorular

██ `performance-research` çıktısı. Bugün cevaplayamadığım her soru burada; ██
cevaplamış gibi yapılmadı. Her satır dört alan taşıyor.

**S1 · Bir chunk gerçekte kaç varlık alır?**
*Kaynak:* Entities `EntityArchetype.ChunkCapacity` API ve chunk yerleşim belgesi.
*Ölçüm:* Belirli bir bileşen kümesi için `ChunkCapacity` değerini okumak; 16 KiB
eksi başlık payını hesaplamak.
*Hangi kararı değiştirir:* Yukarıdaki "[ÇIKARIM] satır başına 16 varlık"
aritmetiğinin gerçek payını verir; merdiven Basamak 6'nın beklenen kazancını
sayısallaştırır.

**S2 · 2022.3 yükseltmesinin faturası ne?**
*Kaynak:* Unity 2022.3 sürüm notları ve yükseltme kılavuzu; bu deponun paket
listesi (`packages-lock.json`).
*Ölçüm:* Yükseltme sonrası 26 EditMode testinin ve dört makine kapısının koşumu.
*Hangi kararı değiştirir:* Basamak 6'nın ön koşulu. Fatura ağırsa Basamak 5'te
durmak doğru karar olur.

**S3 · Bu projenin hedef platformu ne, ve kaç çekirdeği var?**
*Kaynak:* `ProjectSettings` derleme hedefi; hedef cihazın işlemci künyesi.
*Ölçüm:* Bir Development Build alıp hedef cihazda Profiler'a bağlanmak.
*Hangi kararı değiştirir:* ██ Basamak 4'ün (Job System) tamamı. ██ Bu makinede
16 çekirdek var; hedefte 4 varsa paralellik kazancının tavanı dörtte bire iner.
Bugün bu depoda **hiçbir hedef platform kararı yazılı değil**. [DOĞRULANMADI]

**S4 · Mono ile IL2CPP arasındaki fark bu projede ne kadar?**
*Kaynak:* Unity Manual, betik arka uçları (scripting backends) belgesi.
*Ölçüm:* Aynı iş yükü, iki arka uçla, hedef cihazda.
*Hangi kararı değiştirir:* Burst'ün (Basamak 5) kazancı IL2CPP'ye göre ölçülmeli;
Mono'ya göre ölçülen bir kazanç Player'da geçersizdir.
`03-kavram-borc-defteri.md`'de bu satır zaten `HENÜZ YOK`.

**S5 · `Dictionary<Unit, X>` bu makinede gerçekte nereye düşüyor?**
*Kaynak:* .NET `Dictionary<TKey,TValue>` uygulaması; Unity Memory Profiler.
*Ölçüm:* 2.000 girdilik bir sözlüğün anlık görüntüsü; düğüm dizisinin bitişikliği.
*Hangi kararı değiştirir:* Birinci duraktaki "sözlük düğümleri dağınık" cümlesi
bugün bir **mekanizma tarifi**; bu ölçüm onu bir olguya çevirir ya da çürütür.

**S6 · `NativeArray` tahsisatçıları (`Temp`, `TempJob`, `Persistent`) arasındaki
fark ölçülebilir mi?**
*Kaynak:* Unity Manual, `Allocator` belgesi ve `NativeContainer` sayfaları.
*Ölçüm:* Aynı job, üç tahsisatçıyla; tahsis ve süre.
*Hangi kararı değiştirir:* Basamak 4'ün doğru yazılması. Yanlış tahsisatçı
sessiz bir sızıntı ya da her karede bir tahsis üretir.

**S7 · Burst gerçekten vektörleştiriyor mu (SIMD), ve bunu nasıl görürüm?**
*Kaynak:* Burst belgeleri — Burst Inspector ve `intrinsics` sayfaları.
*Ölçüm:* Burst Inspector'da üretilen derleme çıktısını okumak; vektör
komutlarının varlığı.
*Hangi kararı değiştirir:* Basamak 5'in beklenen kazancı. Bugün Burst'ün
**otomatik** vektörleştirme yaptığına dair birincil bir cümle bulunamadı — yalnız
"low-level intrinsics" ile **elle** SIMD yazılabildiği doğrulandı. [DOĞRULANMADI]

**S8 · Yanlış paylaşım (false sharing) bu iş yükünde gerçekten oluyor mu?**
*Kaynak:* `JobsUtility.CacheLineSize` kullanımını gösteren Unity örnekleri.
*Ölçüm:* Aynı paralel job, iki yerleşimle: bitişik indeksler ve önbellek satırına
hizalanmış indeksler.
*Hangi kararı değiştirir:* Basamak 4'ün ölçümünün doğru okunması. Beklenenden
düşük bir paralellik kazancının en sık sebebi budur.

**S9 · `EntityQuery` ile elle yazılmış `foreach` arasındaki fark yalnız hız mı?**
*Kaynak:* Entities "Systems concepts" ve sorgu belgeleri.
*Ölçüm:* Basamak 6'daki iki uygulamanın kod satır sayısı ve değişiklik maliyeti.
*Hangi kararı değiştirir:* ECS'in **bakım** tarafındaki değeri. Hız kazancı
sıfır çıksa bile sorgu modeli bir mimari kazanç olabilir — ya da olmayabilir.

**S10 · Bu depodaki dört makine kapısı ECS'li bir dünyada ne der?**
*Kaynak:* `Tools/check-doc-code-refs.py` ve `Tools/check-curriculum-coverage.py`
kaynağı.
*Ölçüm:* Basamak 6 sonrası koşum.
*Hangi kararı değiştirir:* `Docs/deep/kod/` ağacının tip başına bölünmesi ECS'te
karşılıksız kalıyor. Belge ağacının **kendisinin** yeniden bölünmesi gerekir mi,
sorusu.

---

## ██ DOĞRULANMADI ██ — bu turda kapatılamayanlar

Dürüstlük listesi. Yukarıdaki metinde geçen ve **doğrulanamayan** her iddia:

```
① unity.com alan adı WebFetch'e 403 döndü.
   V Rising / Zenith / Detonation Racing / IXION adları ve "1.000
   IConvertGameObjectToEntity + 140 GameObjectConversionSystem" sayısı arama
   dizini üzerinden okundu. Sayfalar elle açılamadı.

② Unity Manual'ın 2021.3 sürümündeki "JobSystemSafetySystem" sayfası 404 döndü.
   Alıntılar 2020.1 sürümünden. Blittable kısıtı 2021.3 "Job system overview"
   sayfasında ayrıca doğrulandı.

③ Vampire Survivors'ın motor geçmişi (Phaser → Unity) yalnız ikincil
   kaynaklarda. Birincil doğrulama yok. ECS kullanmadığı ise doğrudan
   kanıtlanmadı — Unity'nin resmî vaka listesinde bulunmamasıyla dolaylı olarak
   destekleniyor. ██ Yokluk kanıtı, kanıt değildir. ██

④ Bir chunk'ın gerçek varlık kapasitesi (16 KiB'ın ne kadarı başlık, ne kadarı
   veri) doğrulanmadı. "Satır başına 16 varlık" bir ÇIKARIM ve üst sınırdır.

⑤ Bu projede hiçbir nesnenin yönetilen yığında nereye düştüğü ölçülmedi.
   Memory Profiler bu depoda hiç kullanılmadı.

⑥ Burst'ün OTOMATİK vektörleştirme yaptığı doğrulanmadı. Doğrulanan tek şey
   elle SIMD yazılabildiği.

⑦ Bu projenin hedef platformu ve hedef cihazı yazılı DEĞİL. Bu belgedeki
   paralellik ve önbellek cümlelerinin hepsi bu makineye (16 çekirdek,
   64 bayt satır, 64 MB L3) ait ve bir hedef cihazı temsil etmiyor.

⑧ Üç oyunun (Slay the Spire, Vampire Survivors, Stardew Valley) iç mimarisi
   birincil kaynakla doğrulanmadı; satırlar gözlemlenebilir davranıştan
   çıkarıldı.

⑨ Hiçbir hızlanma oranı ("N kat") yazılmadı — çünkü beş koşulun hiçbiri
   sağlanamadı. Bu bir eksiklik değil, bir kural.
```

---

## Kural — ECS'e ne zaman bakarsın

```
                    Kare başına ölçülmüş bir darboğaz var mı?
                                 │
                 ┌───────────────┴───────────────┐
                HAYIR                           EVET
                 │                               │
        ██ DUR. Bakma. ██          Darboğazın sahibi kim (Profiler)?
        Basamak 0'ı yap:                         │
        önce ölç.                ┌───────────────┼──────────────┐
                              çizim          bellek/GC      CPU, çok varlık
                                 │               │               │
                            ECS DEĞİL       ECS DEĞİL            │
                            (toplu çizim)   (tahsis, havuz)      │
                                                     Aynı iş binlerce varlıkta
                                                     her karede tekrarlanıyor mu?
                                                          │
                                            ┌─────────────┴─────────────┐
                                          HAYIR                       EVET
                                            │                           │
                                     ECS DEĞİL                 ① Job System dene
                                     (algoritma, veri                (en ucuz)
                                      yapısı, erken çıkış)     ② Burst ekle
                                                                    (sözleşme)
                                                               ③ HÂLÂ yetmiyorsa
                                                                  ve ekip bedeli
                                                                  ödeyebiliyorsa
                                                                  ██ ECS ██
```

██ Karar ağacının en önemli özelliği: ECS'e giden yolda **üç kapı** var ve üçü de
"hayır" diyebilir. ██ ECS son çare değil — ECS **en pahalı** çare, ve pahalılığı
hız değil **değiştirme maliyeti** cinsinden.

Bu projede bugün ağacın **ilk düğümünde** duruluyor ve cevap "HAYIR". Doğru
hamle: Basamak 0.

---

## Yanlış hatırlanan üç şey

```
"DOTS = ECS"
   DEĞİL. DOTS üç ayrı teknolojinin adı ve üçü ayrı ayrı kullanılabiliyor.
   ÖLÇÜ, bu depodan: Job System motorun ÇEKİRDEĞİNDE (UnityEngine.CoreModule.dll
   içinde Unity.Jobs.IJobParallelFor var — 2026-08-23'te Cecil ile okundu),
   Burst bir PAKET ve zaten kurulu (com.unity.burst 1.8.18, "depth": 3),
   Entities ise NE kurulu NE de bu Editor sürümünde kurulabilir
   (Entities 1.0 belgesi: "you must have Unity version 2022.3.0f1 and later").
   ██ Üç teslim yolu, üç ayrı karar. ██

"Burst açılınca hızlanır"
   DEĞİL. Burst rastgele C# derlemez; HPC# denen bir alt küme derler ve o alt
   küme managed nesne, class, yönetilen dizi, string metotları ve try/catch
   YASAKLAR (Burst 1.8 HPC# belgesi, 2026-08-23'te doğrulandı).
   Derleyemediği bir metot SESSİZCE yönetilen yoldan koşar.
   ÖLÇÜ, bu depodan: Burst kurulu, [BurstCompile] işaretli metot sayısı 0,
   kullanıcı tanımlı struct sayısı 0, class sayısı 26.
   ██ Yani bugünkü kodun hiçbiri o alt kümeye girmiyor. ██
   Burst bir düğme değil, bir SÖZLEŞME.

"ECS'in kazancı veriyi davranıştan ayırmasıdır"
   YARIM. O ayrım bir MİMARİ karar ve bu projede ZATEN yapılmış: kural
   tiplerinin alanı yok (TargetingRules.cs:31, DamageRules.cs:24,
   TurnRules.cs:28, MovementRules.cs:22 — dördü de `static class`), varlıkların
   kuralı yok (Unit.cs:41 — tek üye bir ad).
   ██ ECS'in ikinci ve asıl kararı YERLEŞİM: ██ aynı tipteki bileşenler 16 KiB'lık
   chunk'larda BİTİŞİK dizilerde durur (Entities 1.0.16 belgesi, 2026-08-23).
   Bu projede depo dört Dictionary ve bitişiklik diye bir şey yok.
   Birinci yarıyı yapıp ikincisini yapmamak TAM OLARAK doğru karardır —
   ikinci yarının bedeli ancak varlık sayısıyla ödenir. Varlık sayısı iki.
```

---

## Kaçış yolu — ECS olmadan aynı sorunlar nasıl çözülür

██ Bu bölüm merdivenin sigortası: ██ Basamak 3'te iş yükü üretildiğinde ilk
refleks ECS olmamalı. Aynı sorunların ECS'siz cevapları:

```
SORUN                          ECS'SİZ CEVAP                        BU PROJEDEKİ HÂLİ
─────────────────────────      ──────────────────────────────       ──────────────────
Çok varlık, çok tarama         Uzamsal bölümleme (ızgara kovası)    UnitGrid ZATEN bir ızgara
Kare başına gereksiz iş        Erken çıkış                          UnitLifecycle.cs:188
Kare başına tahsis             Tampon yeniden kullanımı             BoardAdapter.cs:210 cleanupBuffer
Yaratma/yok etme maliyeti      Nesne havuzu                         02-sonraki-asamalar.md Aşama 2
Tek çekirdek dolu              Job System — ██ ECS'SİZ ██           Basamak 4
Yorumlanan kod yavaş           Burst — ██ ECS'SİZ ██                Basamak 5
Dağınık bellek                 Struct dizisi (SoA) elle             HENÜZ YOK → Basamak 6 öncesi denenebilir
```

██ Son satır merdivende olmayan gizli bir basamak ve kasten öyle: ██ ECS'e
geçmeden, elle bir `struct` dizisi (`Health[]`, `Position[]`) yazıp aynı bitişik
yerleşimi kendi ellerinle kurmak mümkündür. Kazancın büyük kısmı oradan gelir,
ve bedeli bir paket bile değildir. Merdivende ayrı bir basamak olarak
yazılmamasının sebebi: Basamak 6'nın **karşılaştırma tarafı** zaten bu — ECS
uygulamasının yanına konulacak "elle bitişik" uygulaması. Bir gün ölçülürse,
bu belgeye o gün bir Basamak 5.5 eklenir.

---

## Alıntı çapaları

Aşağıdaki satırlar bu belgede geçen satır numaralarının **çapasıdır**. Her satır
`Tools/check-doc-code-refs.py`'nin ALINTI katmanına, o numarada duran kodun
BİREBİR metnini verir. Tablo hücrelerindeki atıflar alıntı biçimine giremez —
o biçim atfın satır BAŞINDA olmasını ister. Kod kaydığında kızacak olan yer
burasıdır.

```
Assets/Game/Core/Unit.cs:41                    public sealed class Unit
Assets/Game/Core/Unit.cs:56                    public string Name { get; }
Assets/Game/Core/UnitGrid.cs:26                public sealed class UnitGrid
Assets/Game/Core/UnitGrid.cs:28                private readonly Unit[,] cells;
Assets/Game/Battle/Battle.cs:59                private readonly Dictionary<Unit, Combatant> combatants =
Assets/Game/Battle/Battle.cs:66                private readonly Dictionary<Unit, Structure> structures =
Assets/Game/Battle/Battle.cs:81                private readonly Dictionary<Unit, Action<UnitState, UnitState>> stateForwarders =
Assets/Game/Battle/Battle.cs:331               if (TryGetPosition(unit, out int x, out int y))
Assets/Game/Battle/Battle.cs:377               public void Tick(float deltaSeconds)
Assets/Game/Battle/Battle.cs:383               foreach (KeyValuePair<Unit, Combatant> pair in combatants)
Assets/Game/Battle/Battle.cs:394               foreach (KeyValuePair<Unit, Structure> pair in structures)
Assets/Game/Battle/Battle.cs:528               public bool TryGetPosition(Unit unit, out int x, out int y)
Assets/Game/Battle/Battle.cs:535               for (int cellX = 0; cellX < board.Width; cellX++)
Assets/Game/Battle/BattleActions.cs:392        if (!battle.TryGetPosition(unit, out x, out y))
Assets/Game/Battle/TurnRules.cs:28             public static class TurnRules
Assets/Game/Battle/TurnRules.cs:59             public static bool CanAct(Team unitTeam, Team currentTurn)
Assets/Game/Core/Combat/AttackProfile.cs:40    public sealed class AttackProfile
Assets/Game/Core/Combat/AttackProfile.cs:72    public int Damage { get; }
Assets/Game/Core/Combat/AttackProfile.cs:78    public int Range { get; }
Assets/Game/Core/Combat/Combatant.cs:152       public UnitState State => lifecycle.State;
Assets/Game/Core/Combat/Combatant.cs:204       public void Tick(float deltaSeconds)
Assets/Game/Core/Combat/DamageRules.cs:24      public static class DamageRules
Assets/Game/Core/Combat/DamageRules.cs:33      public static int ResolveRemaining(int current, int amount)
Assets/Game/Core/Combat/Health.cs:27           public sealed class Health
Assets/Game/Core/Combat/Health.cs:29           private int current;
Assets/Game/Core/Combat/Health.cs:67           public void TakeDamage(int amount)
Assets/Game/Core/Combat/Health.cs:76           public void Heal(int amount)
Assets/Game/Core/Combat/MovementRules.cs:22    public static class MovementRules
Assets/Game/Core/Combat/MovementRules.cs:47    public static bool CanMove(UnitState state)
Assets/Game/Core/Combat/TargetingRules.cs:31   public static class TargetingRules
Assets/Game/Core/Combat/UnitLifecycle.cs:176   public void Tick(float deltaSeconds)
Assets/Game/Core/Combat/UnitLifecycle.cs:188   if (State == UnitState.Alive)
Assets/Game/Unity/BoardAdapter.cs:113          [SerializeField, Min(1)] private int width = 3;
Assets/Game/Unity/BoardAdapter.cs:114          [SerializeField, Min(1)] private int height = 5;
Assets/Game/Unity/BoardAdapter.cs:199          private readonly Dictionary<Unit, UnitView> unitViews =
Assets/Game/Unity/BoardAdapter.cs:210          private readonly List<Unit> cleanupBuffer = new List<Unit>();
Assets/Game/Unity/BoardAdapter.cs:267          SpawnUnit("Vanguard", Team.Player, 1, 2);
Assets/Game/Unity/BoardAdapter.cs:268          SpawnUnit("Raider", Team.Enemy, 1, 3);
Assets/Game/Unity/BoardAdapter.cs:317          private void Update()
Assets/Game/Unity/BoardAdapter.cs:625          private void AdvanceBattleTime()
Assets/Game/Unity/BoardAdapter.cs:627          battle.Tick(Time.deltaTime);
Assets/Game/Unity/BoardAdapter.cs:739          UnitView view = Instantiate(unitPrefab, transform);
Assets/Game/Unity/BoardAdapter.cs:1007         Destroy(view.gameObject);
Assets/Tests/EditMode/Combat/DamageRulesAllocationTests.cs:69    public void Olcum_Aygiti_Tahsisi_Gorebiliyor()
Assets/Tests/EditMode/Combat/DamageRulesAllocationTests.cs:103   public void ResolveRemaining_Hic_Tahsis_Yapmaz()
```

---

## İlgili

- Bu ağacın yönlendirmesi: [README.md](README.md)
- Okuma sırası (bu belge en sonda): [00-okuma-sirasi.md](00-okuma-sirasi.md)
- Kodda **zaten** duran desenler — özellikle §9 kimlik + yan tablo:
  [01-koda-gomulu-desenler.md](01-koda-gomulu-desenler.md)
- ECS'in **tetikleyici koşulu** (bu belge onu tekrar etmez):
  [02-sonraki-asamalar.md](02-sonraki-asamalar.md) · Aşama 5
- Kapsama tablosu: [03-kavram-borc-defteri.md](03-kavram-borc-defteri.md)
- `Task` · `Awaitable` · coroutine · iş parçacığı ayrımı — Job'un beşinci şey
  olduğu buradan anlaşılır:
  [05-yok-olan-mekanizmalar-csharp.md](05-yok-olan-mekanizmalar-csharp.md)
- Bellek katmanı, canlılık ve yıkım — chunk yerleşimi bunun üstüne konuyor:
  [../deep/dil/07-bellek-canlilik-ve-yikim.md](../deep/dil/07-bellek-canlilik-ve-yikim.md)
- Kutulama ve numaralandırıcı tarafı:
  [../deep/dil/02-koleksiyonlar-ve-salt-okunur.md](../deep/dil/02-koleksiyonlar-ve-salt-okunur.md)
- Değer ve referans ayrımı — `class` → `struct` kararının arka planı:
  [../deep/dil/05-deger-referans-ve-kimlik.md](../deep/dil/05-deger-referans-ve-kimlik.md)
- Assembly duvarının faturaları — Basamak 4 ve 6 bu duvarı zorlar:
  [../deep/konular/02-assembly-duvari.md](../deep/konular/02-assembly-duvari.md)
- Motorun çağrı döngüsü ve Domain Reload:
  [../deep/konular/08-motor-cagri-dongusu.md](../deep/konular/08-motor-cagri-dongusu.md)
- Üç ağacın yönlendirmesi: [../deep/README.md](../deep/README.md)
