# Yok olan mekanizmalar — C# tarafı

**Alt başlık:** dilin sunduğu ama bu projenin **henüz almadığı** mekanizmalar —
ve onların arkada gerçekte ne yaptığı.

██ Bir mekanizmanın projede **olmadığını** bilmek, onun **arkada ne yaptığını**
bilmek değildir. ██ `Assets/Game/` altında `yield` de `await` de sıfır kez
geçiyor; bu, bu dosyanın **başlangıcı**, sonucu değil. Buradaki tek soru şu:
`yield return` ve `await` gördüğünde **derleyici o metodu neye çeviriyor**.

## Bu dosyanın sınırı — dört sahip, dört ayrı soru

Aynı kelimeler bu depoda dört yerde geçiyor ve dördü **farklı** şeyi anlatıyor;
burada tekrar edilmiyorlar:

| Soru | Sahip | Ne anlatır |
|---|---|---|
| `foreach` arkasındaki `IEnumerator` ne | [../deep/dil/02-koleksiyonlar-ve-salt-okunur.md](../deep/dil/02-koleksiyonlar-ve-salt-okunur.md) | `object Current`, neden generic değil, `System.Object` ≠ `UnityEngine.Object` |
| Coroutine'i **motor** nasıl yürütür | [../deep/konular/08-motor-cagri-dongusu.md](../deep/konular/08-motor-cagri-dongusu.md) | `StartCoroutine`, kare döngüsü, `WaitForSeconds`, `enabled = false` coroutine'i **durdurmaz** |
| Bu mekanizmalar **ne zaman** gelir | [02-sonraki-asamalar.md](02-sonraki-asamalar.md) | tetikleyici koşullar, ilk adım, ne kırar |
| ██ **Derleyici ne üretir** ██ | **bu dosya** | durum makinesi sınıfı, alanlara dönüşen yereller, `MoveNext`, kurucu (builder) |

Yani: motor tarafı `08`'in, `foreach` tarafı `dil/02`'nin, "ne zaman" tarafı
`02`'nin; buradaki tek konu **derleme çıktısı**.

## Ölçüm künyesi — bu dosyadaki iddialar nasıl ölçüldü

██ "Derleyici şunu üretir" diyen her cümle **çalıştırılarak** ölçüldü ██ —
`sharplab.io` gibi bir çevrimiçi araç kullanılmadı; zincir bu makinedeki Unity
kurulumunun **kendi** araçlarıyla kuruldu:

```
DERLEYİCİ   Editor/Data/DotNetSdkRoslyn/csc.dll
            "Microsoft (R) Visual C# Derleyicisi sürüm 3.9.0-5.21120.8"
            — Unity 2021.3.45f2 kurulumunun KENDİ Roslyn'i
HEDEF       -langversion:9.0  -r: Editor/Data/NetStandard/ref/2.1.0/netstandard.dll
            projenin kendi ayarı: GridStrategy.Core.csproj:15  LangVersion 9.0
                                  GridStrategy.Core.csproj:23  netstandard2.1
                                  ProjectSettings.asset:771    apiCompatibilityLevel: 6
SÖKÜCÜ      Editor/Data/MonoBleedingEdge/lib/mono/4.5/ikdasm.exe (mono.exe ile)
KOŞTURUCU   Editor/Data/MonoBleedingEdge/bin/mono.exe — davranış iddiaları için
TARİH  2026-08-23        SÜRÜM  Unity 2021.3.45f2 (88f88f591b2e)
```

Ölçüm kaynağı bu depoya **girmedi**: deneyler geçici bir dizinde derlendi, buraya
yalnız sökülen IL ve koşum çıktısı alıntılandı. ██ Dürüst sınır ██ —
derleyicinin ürettiği **adlar** bir sözleşme değildir (ölçülmüş karşı örneği
birinci durağın sonunda); sözleşme olan şey **şekil**: durum alanı + `MoveNext`
+ alanlara dönüşen yereller.

---

## Birinci durak: `yield return` — derleyici metodu bir SINIFA çevirir

Ölçülen kaynak:

```csharp
public static IEnumerator Yurut(int tekrar)
{
    if (tekrar < 0) throw new ArgumentOutOfRangeException(nameof(tekrar));
    int adim = 0;
    while (adim < tekrar)
    {
        adim++;
        yield return null;
    }
}
```

### ██ Derlendikten sonra bu bir metot DEĞİLDİR ██

Sökülen IL, harfi harfine:

```il
.class auto ansi sealed nested private beforefieldinit '<Yurut>d__0'
       extends [netstandard]System.Object
       implements [netstandard]System.Collections.Generic.IEnumerator`1<object>,
                  [netstandard]System.Collections.IEnumerator,
                  [netstandard]System.IDisposable
{
  .field private int32  '<>1__state'      ◄── NEREDE KALDIK
  .field private object '<>2__current'    ◄── EN SON NE VERDİK
  .field public  int32  tekrar            ◄── ██ ARGÜMAN, ARTIK BİR ALAN ██
  .field private int32  '<adim>5__1'      ◄── ██ YEREL DEĞİŞKEN, ARTIK BİR ALAN ██
}
```

Üç şey birden ölçüldü: **① bir sınıf üretildi**; **② metodun parametresi bir alan
oldu**; ██ **③ yerel değişken bir alan oldu** ██. Üçüncüsü bu dosyanın **en
önemli tek cümlesi**: `adim` bir yığın (stack) değişkeni olsaydı metot döndüğü an
yok olurdu. Alan olduğu için, öbekte (heap) duran o nesne yaşadığı sürece yaşar —
ve **kareler arasında yaşamasının sebebi tam olarak budur**.

### Metodun kendisine ne oldu — 14 bayt

Gövde artık `MoveNext`'te; geriye kalanın IL'i **tamamı**:

```
.method public hidebysig static IEnumerator Yurut(int32 tekrar) cil managed
{
  .custom instance void IteratorStateMachineAttribute::.ctor(Type)
          = ( ... "Ornek+<Yurut>d__0" ... )   ◄── derleyicinin kendi işareti
  // Code size       14 (0xe)
  IL_0000:  ldc.i4.0
  IL_0001:  newobj  instance void Ornek/'<Yurut>d__0'::.ctor(int32)
  IL_0006:  dup
  IL_0007:  ldarg.0
  IL_0008:  stfld   int32 Ornek/'<Yurut>d__0'::tekrar
  IL_000d:  ret
}
```

Okunuşu: **nesneyi kur, argümanı alanına yaz, dön.** Başka hiçbir şey yok.

### ██ Metot çağrıldığında GÖVDESİ KOŞMAZ — ölçüldü ██

Koşturulmuş çıktı, ve yanında IL'in **neden**i:

```
DENEY   Yurut(-1). Gövdenin ilk satırı bir doğrulama:
        if (tekrar < 0) throw new ArgumentOutOfRangeException(...)
ÇIKTI   [1] Yurut(-1) CAGRILIYOR
            cagri DONDU, istisna YOK. Donen tip: Deney+<Yurut>d__0
        [2] ilk MoveNext() cagriliyor
            ISTISNA MoveNext icinde: ArgumentOutOfRangeException

MoveNext() içinde — throw BURADA, metodun 14 baytında DEĞİL:
  IL_0021:  ldfld   int32 Ornek/'<Yurut>d__0'::tekrar     ◄── ALANDAN okuyor
  IL_0027:  clt
  IL_002b:  brfalse.s  IL_0038
  IL_002d:  ldstr   "tekrar"
  IL_0032:  newobj  instance void ArgumentOutOfRangeException::.ctor(string)
  IL_0037:  throw                                          ◄── ██ BURADA ██
```

### ██ Bu, `dil/03`'ün konusuyla DOĞRUDAN ÇELİŞEN bir tuzaktır ██

[../deep/dil/03-hata-bildirme-ve-dogrulama.md](../deep/dil/03-hata-bildirme-ve-dogrulama.md)
bu projenin doğrulama sözleşmesini yazıyor: `Assets/Game` içinde 66 `throw`
satırı var ve hepsi metodun **başında**, argüman geldiği anda; `nameof`'un 80
kez yazılmasının sebebi de aynı sözleşme — hata **suçluyu** göstersin diye.

Bir `yield` metodu bu sözleşmeyi **sessizce** bozar:

```
DÜZ METOT                          YIELD METODU
─────────                          ────────────
Cagir(-1)                          Yurut(-1)
   ▼                                  ▼
ArgumentOutOfRangeException        hiçbir şey. Nesne döner.
   │                                  ▼  (belki üç kare sonra, belki
yığın izi ÇAĞIRANI gösterir           ▼   bambaşka bir dosyada)
                                   ilk MoveNext()
                                      ▼
                                   ArgumentOutOfRangeException
                                   yığın izi ██ MoveNext'i gösterir ██
                                   Çağıranın adı orada YOK.
```

Bu projedeki karşılığı somut: `UnitLifecycle` kurucusu iki pencereyi hemen
doğruluyor (`Assets/Game/Core/Combat/UnitLifecycle.cs:50-58`), `Tick` negatif
zamanı hemen reddediyor (`UnitLifecycle.cs:178-182`). Bu tipler bir gün
coroutine'e taşınırsa o doğrulamalar **çağrı anını terk eder**.

**Kaçınma yolunun adı var: ayrık doğrulama.** Doğrulayan bir düz metot, `yield`
içeren **ayrı** bir özel metodu çağırır:

```csharp
// Doğrulama BURADA koşar — çağrı anında, düz bir metotta.
public static IEnumerator Yurut(int tekrar)
{
    if (tekrar < 0)
    {
        throw new ArgumentOutOfRangeException(nameof(tekrar), tekrar, "...");
    }
    return YurutIc(tekrar);
}

// yield YALNIZ burada. Gövdesi ilk MoveNext'e kadar koşmaz — ve koşmaması artık
// zararsız, çünkü doğrulanacak bir şey kalmadı.
private static IEnumerator YurutIc(int tekrar) { /* ... yield return null; */ }
```

██ Bu projede böyle bir metot bugün **yok**, çünkü `yield` yok. Bu satır bir
öneri değil, bir **borç kaydı**: ilk coroutine yazıldığı gün bu ayrım **aynı
gün** yazılmalı. ██ **Durum sayıları — `<>1__state` ne anlatıyor:**

```
ctor(0)          ►  state = 0    "hiç koşmadım, baştan başlayacağım"
MoveNext girer   ►  state = -1   "şu anda gövdemin içindeyim"
yield return     ►  state = 1    "birinci yield'da durdum"
tekrar girer     ►  state = -1   ve ██ o yield'ın hemen ARDINDAN devam eder ██
gövde biter      ►  false döner  "bitti"
IEnumerable biçiminde ek olarak:
                    state = -2   "numaralandırma henüz başlamadı" (şablon nesne)
                    state = -3   "gövdenin içindeyim ve bir try bloğundayım"
```

██ **Duraklayan bir metot diye bir şey yoktur.** ██ Her `MoveNext` çağrısı
sıradan bir metot çağrısıdır: girer, `switch (state)` ile kaldığı yere **atlar**,
bir parça iş yapar, döner. Yığında hiçbir şey donmuyor; donan tek şey, alanlarda
duran değerler.

### Elle yazılmış eşdeğeri — aynı şekil, okunur hâliyle

Aşağısı ürettiği **adları** değil, ürettiği **şekli** taklit eder:

```csharp
// Derleyicinin <Yurut>d__0 için ürettiği ŞEKLİN elle yazılmış karşılığı.
private sealed class YurutDurumMakinesi : IEnumerator
{
    private int state;          // <>1__state
    private object current;     // <>2__current
    public int tekrar;          // ARGÜMAN — alan oldu
    private int adim;           // YEREL   — alan oldu

    public YurutDurumMakinesi(int state) { this.state = state; }
    public object Current => current;
    public void Reset() => throw new NotSupportedException();

    public bool MoveNext()
    {
        switch (state) { case 0: goto BAS; case 1: goto DEVAM; default: return false; }
    BAS:
        state = -1;
        // ██ DOĞRULAMA İŞTE BURADA — çağrı anında DEĞİL ██
        if (tekrar < 0) throw new ArgumentOutOfRangeException(nameof(tekrar));
        adim = 0;
        while (adim < tekrar)
        {
            adim++;
            current = null;
            state = 1;
            return true;        // "yield return null" tam olarak burasıdır
        DEVAM:
            state = -1;         // uyandık; döngü koşuluna geri dön
        }
        return false;           // gövde bitti
    }
}

// Ve metodun kendisi — 14 baytın okunur hâli:
public static IEnumerator Yurut(int tekrar)
    => new YurutDurumMakinesi(0) { tekrar = tekrar };
```

`goto`'lar tesadüf değil: derleyicinin ürettiği IL de tam olarak budur
(`brfalse.s`, `beq.s`, `br.s`). C#'ta bir `switch` dalından döngünün
**ortasına** atlamanın başka bir yazımı yoktur; bu makineyi elle yazan kimse
yok, derleyici yazıyor.

### ██ Adın kendisi bir sözleşme DEĞİL — ölçülmüş karşı örnek ██

Aynı kaynak dosya, yalnız derleyici bayrağı değişti:

```
-optimize-  (Debug)    ►  .field private int32 '<adim>5__1'
-optimize+  (Release)  ►  .field private int32 '<adim>5__2'   ██ FARKLI SON EK ██
```

Sınıfın adı (`<Yurut>d__0`) iki koşumda da aynı kaldı, ama alan adının sayısal
son eki değişti. Ders: **ada güvenme, şekle güven.** Kararlı olan şey şu üçü:
① bir durum alanı var · ② yereller ve argümanlar alan oldu · ③ gövde
`MoveNext`'e taşındı ve girişi bir `switch`/atlama tablosu.

### `IEnumerable` dönersen ŞEKİL DEĞİŞİR — ikinci ölçüm

`IEnumerable<int>` döndüren bir `yield` metodu derlendi; üretilen **tek** sınıf
bu kez **iki** rolü birden üstleniyor ve `GetEnumerator` sebebi veriyor:

```
.class ... '<Say>d__0'
       implements IEnumerable`1<int32>, IEnumerable,
                  IEnumerator`1<int32>, IEnumerator, IDisposable
{
  .field private int32 '<>l__initialThreadId'   ◄── ██ BU NE ██
  .field private int32 n                        ◄── çalışan kopyanın argümanı
  .field public  int32 '<>3__n'                 ◄── ██ ŞABLONUN argümanı ██
  ... '<>1__state' · '<>2__current' · '<i>5__2'
}

GetEnumerator():
  if (state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
  {   state = 0;  sonuc = this;   }   ◄── ██ KENDİSİNİ verir — tahsis YOK ██
  else
  {   sonuc = new '<Say>d__0'(0);  }  ◄── ██ YENİ nesne — tahsis VAR ██
  sonuc.n = this.'<>3__n';            ◄── argüman şablondan kopyalanır
  return sonuc;
```

Okunuşu: **ilk** `foreach` — aynı iş parçacığında, henüz kullanılmamış nesne
üzerinde — fazladan tahsis yapmaz, nesne kendisini verir; **ikinci** `foreach`
yeni bir nesne doğurur, iş parçacığı farklıysa ilki bile doğurur.
██ Bu, bu projenin tahsis kültürüne doğrudan bağlanan bir ölçüdür. ██
`Assets/Game/Battle/Battle.cs:373-376` savaşçı kümesini `IEnumerable` olarak
dışarı **açmamayı** seçerken "numaralandırıcı arayüz ardında kutulanır, her
`Update` bir tahsis yapardı" diyor; yukarıdaki `else` dalı aynı ailenin ikinci
faturasıdır. Tahsis ölçümü:
[02-sonraki-asamalar.md](02-sonraki-asamalar.md) Aşama 6.

### `try` / `finally` → `Dispose`, ve `yield`'in yazılamadığı yer

Aynı ölçümde gövdedeki `finally` ayrı bir metoda çıkarıldı; `Dispose` çağırıyor:

```
.method private ... instance void System.IDisposable.Dispose()
{
  if (state != -3 && state != 1) return;     // gövdenin içinde değiliz: iş yok
  .try { leave IL_001a; }
  finally { this.'<>m__Finally1'(); }        ◄── KAYNAKTAKİ finally BURADA
}
```

██ Bir yineleyicinin `finally` bloğu, numaralandırma **yarıda kesilirse** ancak
`Dispose` çağrıldığında koşar. ██ `foreach` bunu senin için yapar (kendi
`try/finally`'sini üretir); elle `MoveNext` çağıran taraf — ki coroutine'de motor
tam olarak bunu yapar — `Dispose` çağırmazsa o `finally` **hiç koşmaz**.
`HENÜZ YOK → sahip: bu projede tek bir finally bloğu yok`; serbest bırakılacak
yönetilmeyen kaynak da yok
([../deep/dil/07-bellek-canlilik-ve-yikim.md](../deep/dil/07-bellek-canlilik-ve-yikim.md)).
Son sınır: `yield return`, dönüş tipi bir **yineleyici arayüzü** olmayan bir
metotta geçersizdir. Sahip etiketi **`C# dili`** — bir Unity kuralı değil, `.NET`
kütüphanesi kuralı da değil:

```
DENEY   public static int Yanlis() { yield return 1; }
ÇIKTI   error CS1624: 'int' bir yineleyici arabirimi türü olmadığından
                      'Hata.Yanlis()' gövdesi yineleyici bloğu olamaz
```

İzin verilen dört tip: `IEnumerator`, `IEnumerable`, `IEnumerator<T>`,
`IEnumerable<T>`. Unity'nin `StartCoroutine`'i bunlardan **yalnız**
`IEnumerator` alır; o sınırın sahibi
[../deep/konular/08-motor-cagri-dongusu.md](../deep/konular/08-motor-cagri-dongusu.md).

---

## İkinci durak: `async` / `await` — aynı fikir, farklı makine

██ `await` de bir durum makinesi üretir. ██ Şekil neredeyse birebir aynı: durum
alanı, alanlara dönüşen yereller, gövdeyi taşıyan bir `MoveNext`. **Fark tek bir
soruda:** o `MoveNext`'i bir dahaki sefer **kim** çağırır. Ölçülen kaynak ve
üretilen sınıf:

```csharp
public static async Task<int> Getir(int x)
{
    int yerel = x + 1;
    await Task.Yield();
    return yerel * 2;
}
```

```
.class ... nested private '<Getir>d__1'
       implements [netstandard]System.Runtime.CompilerServices.IAsyncStateMachine
{
  .field public  int32 '<>1__state'                              ◄── AYNI FİKİR
  .field public  AsyncTaskMethodBuilder`1<int32> '<>t__builder'  ◄── ██ YENİ ██
  .field public  int32 x                                         ◄── argüman → alan
  .field private int32 '<yerel>5__1'                             ◄── yerel  → alan
  .field private YieldAwaitable/YieldAwaiter '<>u__1'            ◄── ██ YENİ ██
}
```

İki yeni alan, iki yeni iş. `<>t__builder` — **kurucu (builder)**: `Task`'ı
üretir, sonucu yazar (`SetResult`), istisnayı yazar (`SetException`), devamı
zamanlar (`AwaitUnsafeOnCompleted`); sökülen IL'de dördü de görünüyor.
`<>u__1` — **bekleyici (awaiter)**: `await` edilen şeyin "bitti mi" sorusunu
cevaplar. IL'de `call YieldAwaiter::get_IsCompleted()` var — ██ beklenen şey
zaten bitmişse durum makinesi **hiç duraklamaz**, düz akar. ██

Metodun kendisi yine bir kurulum metodu:

```
Getir(int32 x):
  newobj   '<Getir>d__1'
  builder = AsyncTaskMethodBuilder`1<int32>::Create()
  .x = ldarg.0   ·   .state = -1
  builder.Start<'<Getir>d__1'>(ref makine)    ◄── ██ İLK MoveNext BURADA ██
  return   builder.get_Task()                 ◄── çağırana Task döner
```

██ Şimdi farkı `yield` ile karşılaştır: ██ `Yurut()` çağrısı gövdeden **tek
satır** koşturmuyordu; `Getir()` çağrısı `builder.Start(...)` ile ilk
`MoveNext`'i **hemen** çağırır — yani `async` bir metodun gövdesi, ilk `await`'e
kadar **çağıranın iş parçacığında, senkron olarak** koşar. Aynı şekil, bu
noktada birbirinin tam tersi.

### Devamı KİM çağırır — asıl fark

```
COROUTINE                                ASYNC / AWAIT
─────────                                ─────────────
MoveNext'i MOTOR çağırır                 MoveNext'i BEKLEYİCİNİN DEVAMI çağırır
   her karede bir kez                       görev bittiğinde bir kez
   sahibi bir MonoBehaviour                 sahibi yok — bir geri çağrı zinciri
   "ne zaman" cevabını Current verir        "ne zaman" cevabını görev verir
      null → sonraki kare                      tamamlandığı an
      WaitForSeconds(2f) → 2 saniye sonra
   ██ kare döngüsü durursa DURUR ██        ██ kare döngüsü durursa DURMAZ ██
```

Son satır bu ayrımın en pahalı sonucudur; üçüncü durakta geri geliyor.

### ██ Debug'da SINIF, Release'de STRUCT — ölçüldü ██

```
                        ITERATOR              ASYNC
-optimize-  (Debug)     extends Object        extends Object       ← sınıf
-optimize+  (Release)   extends Object        extends ValueType    ← ██ STRUCT ██
```

`yield` makinesi **her zaman** bir sınıftır (`IEnumerator` bir arayüz; nesnenin
referans olarak dolaşması gerekir). `async` makinesi Release'de bir `struct`'a
dönüşür ve bu bir **tahsis** kararıdır: `struct` hâlinde metot ilk `await`'e
gelmeden **biterse** makine yığında kalır, öbekte hiçbir şey doğmaz ("sıfır
tahsisli async" budur); ██ gerçekten duraklarsa makine KUTULANIR ██ ve öbeğe
kopyalanır (`builder.AwaitUnsafeOnCompleted`). `class` hâlinde ise her çağrıda
bir nesne doğar, duraklasın duraklamasın.

██ Bunun bu proje için ölçülmüş bir anlamı var. ██
[02-sonraki-asamalar.md](02-sonraki-asamalar.md) Aşama 6 "Editor'deki bir sayı
hedef cihazdaki davranışı kanıtlamaz" diyor; yukarıdaki tablo o cümlenin en somut
örneğidir — Editor'de ölçülen bir `async` metodun tahsisi **derleyici ayarının**
sonucudur, kodun değil. `DamageRulesAllocationTests` disiplini bir gün bir
`async` yola uygulanırsa, ölçüm **hem** EditMode **hem** optimize edilmiş yapı
üzerinde tekrarlanmadıkça bir şey kanıtlamaz.

### ██ `async void` — istisna YAKALANAMAZ ██

Önce **neden**: sökülen IL'de üç `async` biçimin üç ayrı kurucusu var.

```
async Task<int>  ►  .field AsyncTaskMethodBuilder`1<int32> '<>t__builder'
async Task       ►  .field AsyncTaskMethodBuilder            (aynı aile)
async void       ►  .field AsyncVoidMethodBuilder '<>t__builder'   ██ FARKLI ██
```

Sebep tek cümle: ██ dönen bir görev yoksa, istisnayı **taşıyacak yer** de
yoktur. ██ `AsyncTaskMethodBuilder.SetException(e)` istisnayı `Task`'ın içine
koyar; `AsyncVoidMethodBuilder.SetException(e)`'nin koyacağı bir `Task` yoktur ve
tek seçeneği onu **yakalanamayacak** bir yere fırlatmaktır. Şimdi **ölçü** —
iki metot, aynı `try/catch`, aynı koşum:

```
[3] async Task istisnasi try/catch ile yakalanabilir mi
    YAKALANDI: InvalidOperationException / async Task istisnasi
[4] async void istisnasi ayni try/catch ile yakalanabilir mi
    Unhandled Exception: System.InvalidOperationException: async void istisnasi
      at Deney.Ates ()
      at System.Runtime.CompilerServices.AsyncMethodBuilderCore+<>c.<ThrowAsync>b__7_1
      at System.Threading.QueueUserWorkItemCallback.WaitCallback_Context
      at System.Threading.ThreadPoolWorkQueue.Dispatch
    [ERROR] FATAL UNHANDLED EXCEPTION   ── süreç ÖLDÜ. Çıkış kodu: 255
```

Yığın izi mekanizmayı **adıyla** gösteriyor: `AsyncMethodBuilderCore.ThrowAsync`
istisnayı bir `QueueUserWorkItemCallback` ile **başka bir iş parçacığına** attı.
`catch` bloğu o iş parçacığında değildi; `catch` hiç çalışmadı.

| | `async Task` | `async void` |
|---|---|---|
| Kurucu | `AsyncTaskMethodBuilder` | `AsyncVoidMethodBuilder` |
| Çağırana ne döner | `Task` — beklenebilir | **hiçbir şey** |
| İstisna nereye gider | `Task`'ın içine | çağrı **bağlamına** fırlatılır |
| `try/catch` yakalar mı | ██ evet ██ (ölçüldü) | ██ hayır ██ (ölçüldü) |
| Bitişini bekleyebilir misin | evet | ██ hayır — bittiğini bilemezsin ██ |
| Ne zaman doğru | neredeyse her zaman | yalnız bir olay işleyicisi imzası zorluyorsa |

██ Uyarı: ölçüm bir **konsol** programında yapıldı; orada
`SynchronizationContext` yoktu ve istisna havuza düştü. Unity'de bir
`SynchronizationContext` **var** (aşağıda ölçüldü) ve orada istisnanın nereye
düşeceği farklıdır — bu projede `async void` hiç yazılmadığı için **Unity
içindeki tam davranış bu turda DOĞRULANMADI**. Değişmeyen kısım kanıtlı: `catch`
yakalamıyor ve çağıran bitişini bekleyemiyor. ██

### ██ `await` bir İŞ PARÇACIĞI yaratmaz ██ — üç ölçüm, tek koşum

"async = paralel", bu konudaki en yaygın yanlış model. Ölçüsü, tek koşum:

```
ANA is parcacigi = 1     SynchronizationContext.Current = YOK (null)

[A] SynchronizationContext YOKKEN await:
      await ONCESI = 1   await SONRASI = 4  ◄── devam BAŞKA parçacıkta sürdü
[B] SynchronizationContext VARKEN await (Unity'nin yaptığı şey):
      await ONCESI = 1   await SONRASI = 1  ◄── ██ AYNI parçacığa geri döndü ██
[C] await IS PARCACIGI yaratir mi (Task.Run ile karşılaştırma):
      Task.Run cagiran       = 1
      Task.Run lambda icinde = 5   ◄── ██ İŞ PARÇACIĞINI YARATAN BU ██
      Task.Run await SONRASI = 5
```

Okunuşu üç cümle. ① **`await`'in kendisi hiçbir iş parçacığı yaratmadı** —
[C]'de yeni parçacığı doğuran şey `Task.Run`, yani havuza **iş atmak**; `await`
yalnızca "bittiğinde şuradan devam et" diyor. ② **`await` sonrası nerede devam
edeceğin bir bağlam sorusudur** — [A]'da bağlam yoktu, devam havuzda sürdü;
[B]'de bir `SynchronizationContext` kuruldu ve devam çağıranın parçacığında
sürdü. ③ Yani "`async` arka planda çalışır" **yanlıştır**; "`Task.Run` arka
planda çalışır" doğrudur — ikisi ayrı mekanizma.

**Unity tarafı — `SynchronizationContext` ölçüsü ve sınırı.**
[B]'deki bağlam elle yazılmış bir taklitti; Unity'nin gerçeğini ölçtüm:

```
2021.3.45f2 · Editor/Data/Managed/UnityEngine/UnityEngine.CoreModule.dll içinde
    "UnitySynchronizationContext"  ►  1 kez geçiyor
    "SynchronizationContext"       ►  3 kez geçiyor
```

██ Yani mekanizma bu sürümde **vardır** ve motorun kendi dosyasında yaşar. ██
Bunun ötesi — "Unity ana iş parçacığında bir `UnitySynchronizationContext` kurar,
dolayısıyla `await` sonrası ana iş parçacığında devam eder" — bu turda
**DOĞRULANMADI**: bir PlayMode koşumu ister ve bu projede çalıştırılacak tek
satır `async` kod yok. Ölçülen şey tipin **varlığı**.

**`await`'in derleyici kapıları — ölçülmüş hata kodu.**

```
DENEY   public static int AwaitAsyncsiz()
        { int x = await Task.FromResult(1); return x; }
ÇIKTI   error CS4032: 'await' işleci yalnızca bir async metot içerisinde
                      kullanılabilir. ... dönüş tipini 'Task<int>' olarak
                      değiştirmeyi düşünün.
```

Aynı hata `IEnumerator` dönen bir metodun içine `await` konduğunda da çıktı — yani
██ `yield` makinesi ile `await` makinesi **aynı metotta birleşemez**. ██
Birleşenin ayrı bir adı var ve bu kurulumda **derleniyor**:

```
DENEY   public static async IAsyncEnumerable<int> AsyncIterator()
        { await Task.Yield(); yield return 1; }
ÇIKTI   hata yok — derlendi.
ÖLÇÜ    netstandard.dll (ref/2.1.0) içinde "IAsyncEnumerable`1" ► 1 eşleşme
```

`HENÜZ YOK → sahip: bu ağaçta yok; ilk eşzamansız akış doğduğu gün.` Bu projede
`IAsyncEnumerable` sıfır kez geçiyor; burada yalnız **kapının açık olduğu**
ölçüldü, mekanizma öğretilmedi.

---

## Üçüncü durak: ██ İPTAL — coroutine ölür, `Task` ÖLMEZ ██

Bu, iki makine arasındaki en tehlikeli fark — ve bir dil farkı değil, bir
**sahiplik** farkı:

```
COROUTINE                          TASK / AWAIT
─────────                          ────────────
sahibi bir MonoBehaviour           ██ SAHİBİ YOK ██
   ► GameObject Destroy edilir        ► hiçbir şey olmaz
     coroutine ÖLÜR                     devam çağrılmaya devam eder
   ► GameObject SetActive(false)      ► hiçbir şey olmaz
     coroutine DURUR
   ► bileşen enabled = false          ► hiçbir şey olmaz
     ██ DURMAZ ██  (ölçüsü konular/08'de)
   ► sahne yeniden yüklenir           ► ██ HİÇBİR ŞEY OLMAZ ██
     coroutine ölür                     görev hâlâ koşuyor
```

Sağ sütun tek bir sonuç doğurur: `await`'in devamı **yok edilmiş** bir Unity
nesnesine dokunabilir — ve o dokunuş sessiz değildir, `MissingReferenceException`
çıkar. Bir `null` denetimi bunu **çözmez**: yıkılmış bir `UnityEngine.Object`
`== null` der ama `ReferenceEquals(x, null)` `false` der; iki cevap da doğrudur,
ikisi iki ayrı ömrü gösterir (ayrımın sahibi
[../deep/dil/07-bellek-canlilik-ve-yikim.md](../deep/dil/07-bellek-canlilik-ve-yikim.md),
dördüncü durak). Buradaki tek katkı: **coroutine'de bu soruyu hiç sormazsın,
çünkü coroutine zaten ölmüştür; `Task`'ta her devamda sormak zorundasın.**

██ `CancellationToken` bu yüzden var. ██ Mentor katmanının kuralı tek cümlelik:
*iptal, ömrün kendisidir* — token'ın **kökü** işin gerçek sahibinin ömrüne
bağlanır (bileşen yıkımı, bileşen kapanması, sahne ömrü, uygulama çıkışı,
kullanıcı eylemi, zaman aşımı). Kural `unity-game-dev-mentor` →
`references/async-and-concurrency.archive` → *Cancellation Is Lifetime*; oradaki
ikinci yarı daha keskin: **yalnızca uyarı susturmak için token geçirmek**,
altındaki işlem hâlâ yıkılmış durumu değiştirebiliyorsa iptal değildir.

`HENÜZ YOK → sahip: bu ağaçta yok; ilk CancellationToken yazıldığı gün.` Ölçü:
`Assets/Game/` altında sıfır kez geçiyor. Motor tarafının tamamı
[../deep/konular/08-motor-cagri-dongusu.md](../deep/konular/08-motor-cagri-dongusu.md)'de.

---

## Dördüncü durak: `Task` · `Awaitable` · coroutine · iş parçacığı — DÖRT AYRI ŞEY

██ Dördü de "zamana yayılan iş" için kullanılır, ama dördü ayrı mekanizmadır. ██

| | **coroutine** | **`Task`** | **`Awaitable`** | **iş parçacığı / havuz** |
|---|---|---|---|---|
| **Kim başlatır** | `MonoBehaviour.StartCoroutine` | çağıran; `builder.Start` | çağıran; Unity API'si üretir | `Task.Run` · `new Thread` |
| **Kim devam ettirir** | motorun kare döngüsü — her karede bir `MoveNext` | bekleyicinin devamı; bağlam varsa ona gönderilir | Unity'nin oyuncu döngüsü (PlayerLoop) | işletim sistemi zamanlayıcısı |
| **Hangi iş parçacığı** | ██ her zaman ana ██ | bağlam yoksa **havuz** (ölçüldü: 1 → 4); bağlam varsa çağıranın parçacığı (ölçüldü: 1 → 1) | ana parçacık; `BackgroundThreadAsync` ile **açıkça** havuza geçilir | ██ ana DEĞİL ██ (ölçüldü: 1 → 5) |
| **İptali kim sahiplenir** | `MonoBehaviour` — yok edilince ölür | ██ kimse ██ — token yazmazsan iptal yok | `Cancel()` + `CancellationToken` | token ya da elle bayrak |
| **Tahsis eder mi** | evet — makine **her zaman sınıf** (ölçüldü) | Release'de makine **struct**, duraklarsa kutulanır (ölçüldü) + `Task` nesnesi | havuzlanmış tip — ██ aynı örneği İKİ kez `await` etme ██ | parçacık başına yığın; havuz onu paylaştırır |
| **Unity API'sine dokunabilir mi** | evet | ██ yalnız ana parçacıkta devam ediyorsa ██ | evet (ana parçacık devamında) | ██ hayır ██ |
| **Bu projede** | ██ 0 ██ | ██ 0 ██ | ██ bu sürümde tip yok ██ | ██ 0 ██ |

### ██ `Awaitable` bu sürümde VAR MI — ölçüldü ██

```
"Awaitable" dizesini taşıyan dosya sayısı — Editor/Data/Managed/UnityEngine/ altında
  Unity 2021.3.45f2  ►   0 dosya   ██ TİP BU SÜRÜMDE YOK ██
  Unity 6000.5.7f1   ►  16 dosya   ██ VAR ██ (CoreModule.dll ve .xml dâhil)

6000.5.7f1 · UnityEngine.CoreModule.xml içinde UnityEngine.Awaitable üyesi: 31 adet
   T:UnityEngine.Awaitable      "Custom Unity type that can be awaited and used
        as an async return type in the C# asynchronous programming model."
   M:...BackgroundThreadAsync   "Resumes execution on a ThreadPool background thread."
   M:...Cancel                  "... the awaiter receives a
                                 System.OperationCanceledException."
```

Sonuç, sürüme damgalı: ██ **`Awaitable` bu projenin sürümünde (2021.3.45f2)
mevcut değildir.** ██ Bu bir tercih değil, bir sürüm olgusu — ve
[02-sonraki-asamalar.md](02-sonraki-asamalar.md) diliyle söylenirse tetikleyici
koşulu bir kod olayı bile değil: **Unity sürümünün yükseltilmesi**.
██ DOĞRULANMADI ██ — `Awaitable`'ın **ilk hangi sürümde** geldiği. Elde iki uç
nokta var; aradaki sürümler bu makinede kurulu değil, tahmin yazılmıyor.

**`UniTask` — yalnız adıyla.**
Üçüncü taraf bir paket; `Task`'ın Unity'nin oyuncu döngüsüne oturan karşılığı
diye anılır. ██ Bu projede **yok**; ölçüsü tek satır: `Packages/manifest.json`
içindeki 41 bağımlılığın hiçbirinde `UniTask` ya da `Cysharp` geçmiyor. ██ Adı
yalnızca dördüncü bir seçenek olduğunu göstermek için geçiyor.
`HENÜZ YOK → sahip: unity-game-dev-mentor → references/async-and-concurrency.archive
→ UniTask Rule` — seçilme koşulu orada yazılı ve ilk maddesi bu ağacın diliyle
aynı: **önce ölçü, sonra paket**.

---

## Beşinci durak: bu projede neden HİÇBİRİ YOK — ve bu bir eksiklik mi

Sayım, kendim saydım:

```
Assets/Game/  ·  33 üretim dosyası  ·  2026-08-23
    IEnumerator 0 · yield 0 · async 0 · await 0 · Task 0 · Awaitable 0
    Coroutine 0 · Thread 0 · lock ( 0 · Interlocked 0 · volatile 0
    System.Linq 0 · ArrayPool 0 · System.Buffers 0

Assets/ altında (26 test dosyası dâhil)  ►  4 satır, DÖRDÜ DE YORUM
    Assets/Tests/EditMode/Combat/UnitLifecycleTests.cs:71-72
    Assets/Tests/EditMode/Core/PointerGestureTests.cs:31,33
```

Dördü de **REDDEDİLEN** bloklarının içinde — `IEnumerator` bu projede bugüne dek
yalnızca **elenen** seçeneği tarif etmek için yazılmış
([../deep/konular/08-motor-cagri-dongusu.md](../deep/konular/08-motor-cagri-dongusu.md)).

### Sebep: zaman DIŞARIDAN veriliyor — ve bu bir zincir

Asıl sebep "oyun türü" değil, kodda yazılı bir **karar**: zamanı hiçbir çekirdek
tip kendi okumuyor.

```
BoardAdapter.Update()                             ← motor çağırır (Unity katmanı)
   BoardAdapter.cs:323   AdvanceBattleTime()
      BoardAdapter.cs:627   battle.Tick(Time.deltaTime)  ◄── ██ Time.deltaTime BURADA BİTER ██
         Battle.cs:377        Tick(float deltaSeconds)
            Battle.cs:383/:383  foreach → Combatant.Tick / Structure.Tick
                                   UnitLifecycle.cs:176   Tick(float deltaSeconds)
                                      ██ Time.deltaTime YOK. Saniye bir ARGÜMAN. ██
```

Ölçüsü kodda yazılı (`UnitLifecycle.cs:163-166`): EditMode'da `Time.deltaTime`
sıfır değil `0,017675` dönüyor — zamanı içeriden okuyan tasarım testte
**patlamaz**, sessizce anlamsız bir sayıyla yürür.

██ İşte kazanç ölçüsü: ██ `Tick(10.1f)` yazan bir EditMode testi, gerçek zamanda
**10 saniye beklemeden** düşme penceresinin tam yerini sınayabiliyor. Aynı iddia
bir coroutine'e taşınsaydı `yield return new WaitForSeconds(10.1f)` olurdu ve üç
şey birden değişirdi: test EditMode'dan PlayMode'a düşer, dosyanın süresi
milisaniyeden **dakikaya** çıkar, ve kırmızılığı kurala değil **o günkü kare
süresine** bağlanır. ██ Yani "`async` yok" bir eksiklik değil, ölçülmüş bir
**kazanç**: 26 test dosyasının sahnesiz ve gerçek zaman beklemeden koşabilmesinin
tek sebebi budur. ██

### "Bugün gerekmiyor" eksik bir cümledir — her mekanizma için koşulu

██ Her satırda bir **koşul** var; koşulsuz bir satır bu ağaçta ihlaldir. ██

| Mekanizma | Bugün 0, çünkü | ██ Onu ÖNEMLİ hâle getirecek koşul ██ |
|---|---|---|
| coroutine | savaş **anlık** karar veriyor: vuruş, hasar ve görsel tazeleme aynı karede biter | Bir işin **kare arasına yayılması** gerektiği gün — vuruşun havada geçen bir süresi olduğu ilk gün. Üç somut kapı `konular/08`'de sayılı |
| `async` / `await` | ne dosya ne ağ var; beklenecek bir **dış olay** yok | Diskten ya da ağdan veri okunduğu gün (kayıt/yükleme, uzak yapılandırma). Ölçü: `UnityWebRequest` ya da `File.*` ilk kez çağrıldığında |
| `Task` | aynı; ayrıca beklenecek bir **sonuç** yok | Bir API `Task` **döndürdüğü** gün — seçim senin değil, imzanın. İkinci koşul: aynı sonucu birden çok bekleyenin paylaşması |
| `Awaitable` | ██ tip bu sürümde yok ██ (ölçüldü) | Unity 6'ya yükseltildiği **ve** kare zamanlamalı bir bekleme gerektiği gün |
| `UniTask` | `manifest.json`'da yok | Yerleşik `Awaitable`/coroutine **ölçülmüş** bir yetersizlik gösterdiği gün — tahsis ya da gecikme sayısıyla |
| iş parçacığı / `Task.Run` | kare başına iş neredeyse sıfır: `Tick` iki `Combatant` ve sıfır `Structure` dolaşıyor | Unity'ye **dokunmayan**, ölçülmüş biçimde ağır bir hesabın ana parçacığı bloke ettiği gün (harita üretimi, büyük veri ayrıştırma) |
| `lock` / `Interlocked` | tek iş parçacığı var; paylaşılan değiştirilebilir durum yok — `static` alan sayısı 1 ve o `readonly` | İkinci bir iş parçacığı **aynı** veriye dokunduğu gün. Sırası önemli: önce iş parçacığı, sonra kilit |
| `CancellationToken` | iptal edilecek uzun iş yok | İlk `Task` ya da `Awaitable` yazıldığı gün — ██ aynı gün, sonra değil ██ |
| `IAsyncEnumerable` | eşzamansız akış yok (kapı açık: derleniyor) | Bir kaynağın **parça parça** ve zamana yayılarak geldiği gün (akışlı indirme, satır satır dosya) |
| `ArrayPool<T>` | kare başına dizi tahsisi yok; tamponlar bir kez kuruluyor (`BoardAdapter.cs:210`) | Kare başına **boyutu değişen** geçici dizi ihtiyacı ölçüldüğü gün |

---

## Altıncı durak: nesne havuzunun BCL yarısı — tek paragraf

██ Havuzun Unity yarısı burada **değil**. ██ Deaktif etme, `Awake`'in bir daha
çalışmaması, `OnEnable` tekrarı ve sıfırlama sözleşmesi
[02-sonraki-asamalar.md](02-sonraki-asamalar.md) Aşama 2'nin konusu; buraya
yalnız .NET tarafı düşüyor. `System.Buffers.ArrayPool<T>` paylaşılan bir dizi
havuzudur: `Rent(minimumLength)` **en az** istediğin kadar uzun bir dizi verir —
██ tam olarak istediğin kadar değil ██, bu yüzden `array.Length`'e değil kendi
tuttuğun uzunluğa güvenmen gerekir — `Return(array)` geri verir. Geri vermezsen
sızıntı değil **kazanç kaybı** olur; iki kez geri verirsen aynı diziyi iki ayrı
sahip alır ve hata sessizdir. Ölçü: `ArrayPool` bu projenin derlendiği
`netstandard.dll` (ref/2.1.0) içinde **mevcut** (kapı açık), `Assets/Game/`
altında `System.Buffers` ve `ArrayPool` **0 kez** geçiyor. Uzatılmıyor: sahipsiz
bir `Rent`/`Return` çifti, `-=` unutulan bir aboneliğin sessizliğiyle aynı
ailedendir ([../deep/dil/07-bellek-canlilik-ve-yikim.md](../deep/dil/07-bellek-canlilik-ve-yikim.md)).

---

## Üç oyun — "zamana yayılan bir iş nasıl yürütülür"

| Oyun | Zamana yayılan iş neye benziyor |
|---|---|
| **Slay the Spire** | ██ EŞLEŞMEZ ██ Bir kart oynandığında etkiler sırayla çözülür ama saat işlemez: sıradaki adımı bekleyen şey **oyuncunun kendisidir**. Zamana değil, **sıraya** yayılan bir iş |
| **Vampire Survivors** | Silahlar bir **bekleme süresiyle** ateşlenir ve ekrandaki her mermi kendi ömrünü sayar; sandık açılışı oyunu durdurup bir seçim ekranı gösterir |
| **Stardew Valley** | Gün sonunda ekinler büyür, hayvanlar ürün verir, mektup gelir; ayrıca balık tutma ve maden kırma bir vuruşun **süresi** boyunca sürer |

Eşleşmeyen satırın sebebi bu projeyle **aynı**: sıra tabanlı bir oyunda
"beklemek" bir mekanizma değil, bir arayüz seçimidir — ve tam olarak bu yüzden
burada bir `yield` yok. ██ DOĞRULANMADI ██ — üç oyunun da kaynak kodu kapalıdır;
yukarıdaki satırlar **oynanış gözlemidir**, uygulama iddiası değil: hiçbiri
"Stardew Valley coroutine kullanır" demiyor.

---

## Kural: zamana yayılan bir iş için hangisini seçersin

```
① İŞ, UNITY NESNELERİNE DOKUNUYOR MU?
      evet → iş parçacığı ELENDİ, ③'e geç          hayır → ②
② İŞ, ÖLÇÜLMÜŞ BİÇİMDE İŞLEMCİ-AĞIR MI (kare bütçesini yiyor mu)?
      hayır → ③
      evet  → iş parçacığı / havuz. ██ "Ölçülmüş" zorunlu: profil çıktısı
              olmadan bu dala girilmez — 02-sonraki-asamalar.md · Aşama 6 ██
③ BEKLENEN ŞEY BİR DIŞ OLAY MI (disk, ağ, kullanıcı), YOKSA ZAMAN MI?
      dış olay → ④                                 zaman/kare → ⑤
④ BEKLENEN ŞEY ZATEN Task DÖNDÜRÜYOR MU?
      evet  → async/await.  ██ İPTAL TOKENI AYNI GÜN YAZILIR ██
      hayır → yine async/await, ama devamın ANA PARÇACIKTA olduğunu DOĞRULA;
              yoksa Unity API'sine dokunamazsın
⑤ İŞİN SAHİBİ BİR MonoBehaviour MU VE ONUNLA BİRLİKTE ÖLMELİ Mİ?
      evet  → coroutine. Sahiplik ve iptal BEDAVA gelir
      hayır → durum makinesini KENDİN yaz: bir enum + bir sayaç + Tick(delta)
              ██ BU PROJENİN BUGÜN YAPTIĞI ŞEY BUDUR ██
              UnitLifecycle.cs:82 (hâl) + :44 (sayaç) + :169 (Tick)
```

██ Beşinci soruda "hayır" demek bir kayıp değil. ██ Bu proje o dalı seçti ve
kazandığı şey ölçülü: üç çekirdek assembly `noEngineReferences: true` taşıyor,
26 EditMode test dosyası sahnesiz koşuyor, zaman testte bir **argüman**.

---

## Yanlış hatırlanan üç şey

**① "`async` paraleldir / arka planda çalışır."** ██ DEĞİL. ██ Ölçüldü:
`await`'in kendisi hiçbir iş parçacığı yaratmadı; yeni parçacığı doğuran şey
`Task.Run`'dı (ana = 1, lambda içi = 5). Devamın **nerede** koşacağı ayrı bir
sorudur ve cevabı bağlamdır: bağlam yokken havuza düştü (1 → 4), bağlam varken
çağıranın parçacığında sürdü (1 → 1). `async` bir **eşzamanlılık**
mekanizmasıdır, bir **paralellik** mekanizması değil.

**② "Coroutine bir iş parçacığıdır."** ██ DEĞİL. ██ Coroutine ana iş
parçacığında, kare döngüsünün **içinde** koşar; `MoveNext()` gövdesi bir saniye
sürerse kare bir saniye uzar. Derleyici tarafından bakınca daha da açık:
coroutine, `IEnumerator` uygulayan **sıradan bir nesnedir** — motor onun
`MoveNext`'ini her karede bir kez çağırıyor, o kadar. Ölçüsü `konular/08`'de.

**③ "Bir `yield` metodunun başındaki `if (x < 0) throw` çağıranı korur."**
██ KORUMAZ. ██ Ölçüldü: `Yurut(-1)` istisna atmadan **döndü**; istisna ilk
`MoveNext()` içinde çıktı ve yığın izi çağıranı değil `MoveNext`'i gösterdi. IL
bunu kanıtlıyor: `throw` `MoveNext`'in içinde, metodun 14 baytlık gövdesinde
değil. Tek çare **ayrık doğrulamadır**.

---

## Kaçış yolu: bu mekanizmalar olmadan nasıl idare ediliyor

Bu projenin cevabı yokluk değil, **elle yazılmış bir durum makinesi**:

```
DERLEYİCİNİN ÜRETTİĞİ MAKİNE            BU PROJENİN ELLE YAZDIĞI MAKİNE
────────────────────────────            ───────────────────────────────
'<>1__state'  (int, gizli)              UnitLifecycle.cs:82   State (UnitState enum)
'<adim>5__1'  (gizli alan)              UnitLifecycle.cs:44   remainingSeconds
MoveNext()    (motor çağırır)           UnitLifecycle.cs:176  Tick(float deltaSeconds)
                                                              ██ ÇAĞIRAN DIŞARIDA ██
adı okunmaz, ayıklanamaz                adı okunur, testte doğrudan çağrılabilir
kare süresine bağlı                     ██ saniye bir ARGÜMAN ██
```

**KAZANDIRDIĞI:** 26 EditMode test dosyası sahnesiz koşuyor; `Tick(10.1f)`
gerçek zamanda beklemiyor; üç çekirdek assembly motoru hiç tanımıyor.

**KAYBETTİRDİĞİ, dürüstçe:** her yeni "zamana yayılan iş" için bir enum değeri,
bir sayaç ve bir `Tick` dalı elle yazılıyor. Beş hâlde bu okunur; yirmi hâlde
`switch` şişer. ██ Coroutine'in kazandığı şey tam olarak burasıdır: karmaşık ve
sıralı bir akışı **doğrusal** yazabilmek. ██ Bedeli de yazılı: akış sahibini bir
`MonoBehaviour`'a bağlar, zamanı kareye bağlar ve testi PlayMode'a düşürür.

**NE ZAMAN TERSİNE DÖNER:** bir tek metotta beş ya da daha fazla ardışık bekleme
adımı biriktiğinde — "kalkış → vuruş → geri tepme → hasar → toparlanma" gibi. O
gün elle yazılan makine bir sıra numarası taşımaya başlar ve okunurluğu
coroutine'e kaptırır. Bugün böyle bir metot **yok**: en uzun zamana yayılan iş
iki adımlı (`Downed → Dead → temizlik`).

---

## Bu turda DOĞRULANMADI diye işaretlenenler

██ Aşağıdakiler bu dosyada **iddia edilmedi**; listelenme sebebi bir sonraki
turun nereden başlayacağını bilmesi: ██

```
① UnitySynchronizationContext'in ne zaman kurulduğu ve await sonrası devamın
   gerçekten ana iş parçacığında sürdüğü.
   ÖLÇÜLEN: tip 2021.3.45f2'nin CoreModule.dll'inde var (1 eşleşme).
   EKSİK  : çalışma zamanı davranışı — bir PlayMode koşumu ister.
② Unity içinde bir async void istisnasının nereye düştüğü.
   ÖLÇÜLEN: konsolda süreç ÖLDÜ, yığın izi ThreadPool'u gösterdi.
   EKSİK  : Unity Console davranışı. Bu projede async void yok.
③ Awaitable'ın İLK hangi sürümde geldiği.
   ÖLÇÜLEN: 2021.3.45f2 → yok; 6000.5.7f1 → var.  EKSİK: aradaki sürümler.
④ Üç oyunun hangi mekanizmayı kullandığı — üçünün de kaynağı kapalı.
⑤ Derleyicinin ürettiği ADLARIN kararlılığı.
   ÖLÇÜLEN: aynı kaynak, iki ayar, FARKLI son ek ('<adim>5__1' / '<adim>5__2').
   SONUÇ  : ad bir sözleşme değildir; bu dosya adları ÖRNEK olarak gösteriyor,
            kural olarak değil. Kural olan şey ŞEKİL.
```

---

## İlgili

- Bu ağacın yönlendirmesi: [README.md](README.md) · kodda **zaten** duran
  desenler: [01-koda-gomulu-desenler.md](01-koda-gomulu-desenler.md)
- Bu mekanizmalar **ne zaman** gelir: [02-sonraki-asamalar.md](02-sonraki-asamalar.md)
  · kapsama tablosu: [03-kavram-borc-defteri.md](03-kavram-borc-defteri.md)
- `IEnumerator`'un birinci hayatı (`foreach`): [../deep/dil/02-koleksiyonlar-ve-salt-okunur.md](../deep/dil/02-koleksiyonlar-ve-salt-okunur.md)
- Coroutine'in motor tarafı: [../deep/konular/08-motor-cagri-dongusu.md](../deep/konular/08-motor-cagri-dongusu.md)
- Gecikmiş doğrulamanın çelişkisi: [../deep/dil/03-hata-bildirme-ve-dogrulama.md](../deep/dil/03-hata-bildirme-ve-dogrulama.md)
- İptal, ömür ve yıkılmış nesne: [../deep/dil/07-bellek-canlilik-ve-yikim.md](../deep/dil/07-bellek-canlilik-ve-yikim.md)
- Ödünç alınan dil özellikleri: [../deep/dil/README.md](../deep/dil/README.md)
  · anlatı biçiminin kendisi: [../deep/README.md](../deep/README.md)
