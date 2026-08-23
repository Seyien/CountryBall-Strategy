# Delegenin arka tarafı — `+=` derleyicide neye dönüşür

> **Nerede geçiyor:** `Combatant` kurucusunun son iki satırı,
> `UnitLifecycle.StateChanged`, `Combatant.StateChanged`,
> `Battle.UnitStateChanged`, `BoardAdapter.OnEnable`/`OnDisable`
> **Kodda nereden geldin:** `this.lifecycle.StateChanged += OnLifecycleStateChanged;`
> **Ne zaman oku:** bir `+=` satırını bir KURUCUNUN içine yazarken; bir olaya
> ikinci abone eklerken; ya da "abone olmak neyi hayatta tutuyor" diye
> sorduğunda.

[`04-delege-olay-ve-kapanis.md`](04-delege-olay-ve-kapanis.md) bu malzemenin
**sözleşmesini** anlatıyor: `Action<…>` nasıl okunur, `Func` neden hiç yok,
`event` ile düz alanın farkı, abonesi olmayan olayın neden `null` olduğu, iki
kapanışın neden eşit olmadığı. Orayı okumadan buraya girme; burada hiçbiri
tekrar edilmiyor. Bu dosya bir alt kat:

```
04 sorar:  "+= ne VAAT EDİYOR"          ── sözleşme
06 sorar:  "+= ÇALIŞTIĞINDA ne oluyor"  ── nesne, derleyicinin ürettiği
                                           metotlar, çağrı listesi
```

Olayın **proje** tarafı — hangi halka neyi ekliyor, sözlük neden `Battle`'da —
[`../konular/01-olay-zinciri.md`](../konular/01-olay-zinciri.md)'nde. Burada
zincir yok; zincirin çalıştığı **makine** var.

---

## Sahne

Operatörün sorduğu iki satır — `Combatant` kurucusunun sonu:

```csharp
// Combatant.cs — kurucunun son iki satırı
lastObservedState = this.lifecycle.State;
this.lifecycle.StateChanged += OnLifecycleStateChanged;
```

Üstlerindeki yorum bu iki satırın **kararını** söylüyor: en sonda duruyorlar,
çünkü doğrulamalardan biri patlarsa geriye abone olunmuş bir `UnitLifecycle`
kalmamalı. Karar doğru — ama bir mekanizmanın üstüne kurulu ve mekanizma hiçbir
yerde yazılı değil: *"`+=` çalıştığı an ne oldu ki, sonrasında bir `throw`
TEHLİKELİ hâle geldi?"*

Cevap tek cümlede: **`+=` bir kayıt işlemidir ve kaydedilen şey `this`.**
Yarım kurulmuş bir nesnenin kendisi, o satırda, başka bir nesnenin listesine
girer. Geri kalan her şey bunun sonucu.

---

## Karakterler

```
╔═ System.Delegate  ·  sahip: .NET kütüphanesi ═════════════════╗
║  Ne yapar : İKİ ALANLI bir NESNE tutar — Target ve Method      ║
║  Vaadi    : ikisi de public okunur; çağrıldığında Method'u     ║
║             Target ÜSTÜNDE çalıştırır                          ║
║  BİLMEZ   : Target'ın kurulmayı bitirip bitirmediğini          ║
║             ██ BU DOSYANIN TAMAMI BU SATIRDAN ÇIKIYOR ██       ║
╚═══════════════════════════════════════════════════════════════╝

╔═ MulticastDelegate + GetInvocationList()  ·  .NET kütüphanesi ╗
║  Ne yapar : Delegate'ten türer, ÜSTÜNE bir çağrı listesi koyar;║
║             GetInvocationList() o listeyi KOPYALAYIP verir     ║
║  Vaadi    : `delegate` ile üretilen HER tip buradan türer      ║
║             (Action<…> dahil); sıra = abonelik sırası          ║
║  BİLMEZ   : abonelerin birbirini — biri patlarsa ötekini       ║
║             KURTARMAZ ██ Invoke try/catch TAŞIMAZ ██           ║
╚═══════════════════════════════════════════════════════════════╝

╔═ Delegate.Combine / Delegate.Remove  ·  .NET kütüphanesi ═════╗
║  Ne yapar : iki delegeyi birleştirir / birinden ötekini çıkarır║
║  Vaadi    : her çağrıda ██ YENİ NESNE ██; verilenler kımıldamaz║
║  BİLMEZ   : aynı hedef+metodun listeye ikinci kez girdiğini —  ║
║             ELEMEZ. Remove yalnız SONUNCU eşleşmeyi çıkarır.   ║
╚═══════════════════════════════════════════════════════════════╝

╔═ event (anahtar kelime)  ·  sahip: C# dili ═══════════════════╗
║  Ne yapar : TEK alan gibi yazılır, ÜÇ üye üretir               ║
║             (gizli alan + add_X + remove_X)                    ║
║  Vaadi    : dışarıya yalnız add ve remove yüzünü gösterir      ║
║  BİLMEZ   : ██ İÇERİDE hiçbir şey ██ — bildiren tipin gövdesi  ║
║             gizli alanı sıradan bir alan gibi görür            ║
╚═══════════════════════════════════════════════════════════════╝
```

---

## Birinci durak: delegenin İÇİ — `Target` + `Method`

Bir delege bir "fonksiyon işaretçisi" **değildir**. İki alanlı bir nesnedir:

```
   this.lifecycle.StateChanged += OnLifecycleStateChanged;
                                  ▲ derleyici burada bir NESNE kurar:
                                    new Action<UnitState>(this.OnLifecycle…)

         ╔═════════════════════════════════════════╗
         ║  Target  ──►  o Combatant örneği (this) ║   ██ `this` ARTIK BU
         ║  Method  ──►  OnLifecycleStateChanged   ║      NESNENİN İÇİNDE ██
         ╚═════════════════════════════════════════╝
```

`Target` ve `Method`, `System.Delegate`'in **public** üyeleri: okunabilirler.
Statik bir metot abone edilseydi `Target` `null` olurdu — ama bu projede statik
abone **yok**; üç aboneliğin üçü de örnek metodu (ölçü aşağıda).

### Ölçü — Target'ı gerçekten okumak

`StateChanged` bir `event`. Dışarıdan **okunamaz**: bildiren tipin dışında
yazılan `lifecycle.StateChanged.GetInvocationList()` derlenmez, **CS0070** verir
(yasak satırların tam listesi
[04'ün ikinci durağında](04-delege-olay-ve-kapanis.md#ikinci-durak-event-ile-duz-action-alani-farki)).
Yani ölçü üyesi `UnitLifecycle`'ın **içine** yazılır — geçici olarak:

```csharp
// UnitLifecycle.cs içine GEÇİCİ olarak ekle, ölçtükten sonra sil:
public string DescribeSubscribers()
{
    if (StateChanged == null) { return "abone yok"; }
    var parts = new List<string>();
    foreach (Delegate d in StateChanged.GetInvocationList())
    {
        parts.Add((d.Target?.GetType().Name ?? "null (static)") + " . " + d.Method.Name);
    }
    return string.Join(" | ", parts);
}
```

İki ayrı yerden çağır, **iki farklı cevap** gelir:

```
① üretim yolu — Combatant kurucusu abone oldu
   new Combatant(new Health(10), lifecycle, new AttackProfile(1, 1));
   lifecycle.DescribeSubscribers()   ►  "Combatant . OnLifecycleStateChanged"
                                          ▲             ▲
                                          Target        Method

② test yolu — UnitLifecycleTests.StateChanged_FiresOnceWhenHealthIsDepleted
   lifecycle.StateChanged += seen.Add;          // seen: List<UnitState>
   lifecycle.DescribeSubscribers()   ►  "List`1 . Add"
      ██ Target o LİSTE ██ — testin kendisi değil, `this` değil, LİSTE
```

②'nin tek başına kapattığı bir ders var: `seen.Add` yazdığında abone ettiğin şey
senin metodun değil, **listenin** metodu — ve o listenin kendisi delegenin içine
giriyor, yani `lifecycle` yaşadığı sürece `seen` toplanamaz.
`GetType().Name` gerçekten `` List`1 `` basar; generic tiplerin çalışma anındaki
adı böyledir, kırık değil.

### Bunun açtığı iki kapı

**(a) Kapanış kimliği.** İki delegenin eşitliğini ölçen şey tam olarak bu iki
alandır — `-=`'in neye baktığı
[04'ün dördüncü durağında](04-delege-olay-ve-kapanis.md#dorduncu-durak-kapanis-kimligi---neye-bakiyor)
yazılı. Buradaki katkı **neden** o iki alan olduğu: delegenin başka alanı yok.

**(b) Sızıntı yönü.** Ok yayıncıdan aboneye gider:

```
   UnitLifecycle                       Combatant
   ╔═══════════════════════╗           ╔══════════════════╗
   ║ gizli StateChanged    ║           ║ health           ║
   ║   └─ Delegate         ║           ║ lifecycle        ║
   ║        Target ────────╫──────────►║ lastObservedState║
   ╚═══════════════════════╝           ╚══════════════════╝
   ██ YAYINCI ABONEYİ TUTAR ██ — Combatant'a başka hiçbir referans
   kalmasa bile lifecycle yaşadığı sürece toplanamaz. Sezginin TERSİ.
```

Bu yönün zincir üzerindeki sonucu
[`../konular/01-olay-zinciri.md`](../konular/01-olay-zinciri.md)'nin "Sökülmezse
ne olur" bölümünde; buradaki katkı oku çizen alanın **adı**: `Target`.

**Kapsam — bu projede yön neden zararsız:** `Combatant` `lifecycle`'ın sahibi;
ikisi birlikte doğar, birlikte ölür, ok bir sahiplik sınırını geçmiyor.
██ Sınırı geçen tek abonelik `Battle.AddUnit`'teki ██ ve orada bırakma zorunlu.

---

## İkinci durak: `event` derleyicide NEYE dönüşür

`Combatant.cs`'te yazdığın **tek** satır —
`public event Action<UnitState, UnitState> StateChanged;` — derleyicide **üç**
üye oluyor. `Action<…>` yerine `A` yazarak:

```
① private A StateChanged;                ◄── gizli DESTEK ALANI
② public void add_StateChanged(A value)  ◄── `+=` buraya gider
③ public void remove_StateChanged(A value) ◄── `-=` buraya gider
```

Alanın kendisi hiçbir zaman dışarıdan görünmez. ②'nin derleyici tarafından
üretilen gövdesi, sadeleştirilmiş hâliyle:

```csharp
public void add_StateChanged(A value)
{
    A before = this.StateChanged, seen;
    do
    {
        seen = before;
        A combined = (A)Delegate.Combine(seen, value);          // ① YENİ nesne
        before = Interlocked.CompareExchange(                   // ③ yalnız
            ref this.StateChanged, combined, seen);             //   EKLEMEYİ korur
    }
    while (before != seen);                          // ② gizli alana o yazılır
}
```

### `+=` bir "listeye ekleme" DEĞİL

[04](04-delege-olay-ve-kapanis.md#delege-nesnesi-degismez-bunun-ustune-kurulu)
"delege nesnesi değişmezdir" diyor. Buradaki katkı bunun **sebebi**:
`Delegate.Combine` var olan nesneyi değiştirmiyor, yeni bir tane döndürüyor —
ve bu koşturulabilir bir ölçüye çevrilebilir.

```csharp
// UnitLifecycle.cs içine GEÇİCİ:  public object Held() => StateChanged;
lifecycle.StateChanged += seen.Add;   object first  = lifecycle.Held();
lifecycle.StateChanged += seen.Add;   object second = lifecycle.Held();
//                       ▲ AYNI hedef, AYNI metot

ReferenceEquals(first, second)   ►  false    ██ Combine YENİ nesne verdi ██
lifecycle.OnHealthDepleted();
seen                             ►  [Downed, Downed]   ██ İKİ KEZ ██
```

Son satır az bilinen yarısı: `Delegate.Combine` **elemez**. Aynı hedef+metot
ikinci kez eklenirse çağrı listesinde iki giriş olur ve abone iki kez çalışır.
Karşılığında `Delegate.Remove` yalnız **sonuncu** eşleşmeyi çıkarır — bir `-=`
bir `+=`'i dengeler, iki `+=`'i değil. Simetriyi tutan şey derleyici değil,
çağıranın disiplini.

**Bu projede bugün ölçüsü:** üretimdeki üç `+=` üç ayrı dosyada ve her biri bir
kez çalışıyor (`Combatant.cs:90`, `Battle.cs:228`, `BoardAdapter.cs:290`).
██ Önem kazanacağı gün: ██ `BoardAdapter.OnEnable` bir `OnDisable` görmeden
ikinci kez çağrıldığı gün — Unity'de bu, bileşen kapatılıp açıldığında
olağandır; simetriyi bugün tutan tek şey `OnDisable`'daki eşleşen `-=` satırı.

### `event`in TEK işi

`event` sözcüğü ne ekleme yapıyor ne çağrı listesi tutuyor. Tek işi **destek
alanını dışarıya kapatmak**:

```
BİLDİREN TİPİN İÇİNDE (UnitLifecycle.cs)     DIŞINDA (Combatant.cs)
──────────────────────────────────────       ──────────────────────
StateChanged?.Invoke(next);         ✓        ✗ CS0070
StateChanged = null;                ✓        ✗ CS0070
StateChanged.GetInvocationList()    ✓        ✗ CS0070
StateChanged += OnFoo;              ✓        ✓  (add_StateChanged)
StateChanged -= OnFoo;              ✓        ✓  (remove_StateChanged)
```

**Ölçü:** `Combatant.cs`'in kurucusundaki `+=` satırının altına
`this.lifecycle.StateChanged = null;` yaz ve derle. CS0070 gelir: *"The event
… can only appear on the left hand side of += or -= (except when used from
within the type …)"*. Aynı satırı `UnitLifecycle.cs`'in içine taşı — derlenir.
Kısıt **assembly**'ye değil **bildiren tipe** bakıyor: `Combatant` ile
`UnitLifecycle` aynı ad alanında, aynı assembly'de, aynı klasörde duruyor ve
kısıt yine de geçerli. Birinci duraktaki ölçü üyesinin neden `UnitLifecycle`'ın
**içine** yazılmak zorunda olduğunun cevabı budur.

Sahip etiketleri: `event` = **C# dili**. `Delegate.Combine` / `Delegate.Remove`
ve `Interlocked.CompareExchange` = **.NET kütüphanesi** — sonuncusunun vaadi dar
ve sınırı burada: **abone EKLEMEYİ** iki iş parçacığına karşı korur, **yayını**
██ KORUMAZ ██.

---

## Üçüncü durak: çağrı listesi ve `Invoke`

Bir delege birden çok aboneyi bir dizide taşır. `GetInvocationList()` o diziyi
kopyalayıp verir; `Invoke` onu **abonelik sırasıyla** yürütür.

```
StateChanged  =  [ h1 , h2 , h3 ]
                   │    │    │
      Invoke(x) ───┴────┴────┴──►  h1(x)  sonra  h2(x)  sonra  h3(x)
                                   ██ SIRA = abone olunma sırası ██
```

### ██ DÖNÜŞ DEĞERİ TUZAĞI ██

[04](04-delege-olay-ve-kapanis.md#funct-r-neden-hic-yok) "`Func` neden hiç yok"
diyor. Arka taraftan sebebi şu: `Invoke` listeyi dolaşırken elinde **tek** bir
dönüş değeri yeri var ve her adım öncekini üzerine yazıyor.

```csharp
Func<int> f = null;
f += () => 1;
f += () => 2;
int r = f();        //  r  ►  2   — 1'i görmenin HİÇBİR yolu yok: birinci
                    //   lambda çalıştı, döndürdü, değer ikinci adımda
                    //   üzerine yazıldı. İstisna yok, uyarı yok.
```

N aboneden N−1'inin cevabı sessizce kayboluyor; bir olayın imzası bu yüzden
`Func` olamaz. Projede `Func` sıfır kez geçiyor — ölçü: `Assets/` altında
`Func<` ara, hiç eşleşme yok.

### ██ İSTİSNA TUZAĞI ██

`MulticastDelegate.Invoke` bir `try`/`catch` **taşımaz**:

```
StateChanged.Invoke(Downed)
     ├─► h1(Downed)  ── fırlattı
     ├─✗ h2(Downed)  ██ HİÇ ÇAĞRILMADI ██
     └─✗ h3(Downed)  ██ HİÇ ÇAĞRILMADI ██ ─► istisna yayıncının Invoke
                                             satırından yukarı çıkar
```

Ve yayıncı için ikinci bir bedel var: **`Invoke` satırından SONRAKİ kendi
satırları da çalışmaz.** Bu projede o satırın ne olduğu tuzağı soyut olmaktan
çıkarıyor:

```csharp
// UnitLifecycle.OnHealthDepleted — son iki satır
SetState(UnitState.Downed);              // ← içinde StateChanged?.Invoke var
remainingSeconds = downedWindowSeconds;  // ██ BİR ABONE FIRLATIRSA ÇALIŞMAZ ██

//  State = Downed  ✓ yazıldı (SetState Invoke'tan ÖNCE atıyor)
//  remainingSeconds ✗ 0'da kaldı → ilk Tick(0.016f): Alive değil,
//  0 - 0,016 ≤ 0, Downed → SetState(Dead)
//  ██ 10 saniyelik kurtarma penceresi TEK KAREDE yok oldu ██
```

**Aynı dosyadan karşı örnek** — `TryRevive`'daki `SetState(Alive);` satırının
ardından gelen `remainingSeconds = 0f;` çalışmasa da zararsızdır:
`RemainingSeconds` property'si `Alive` iken zaten `0` döndürüyor. Ayıran ölçüt,
`Invoke`tan sonraki satırın bir **değişmez** mi kurduğu, yoksa zaten sağlanmış
bir şeyi mi tekrar yazdığı.

**Bu projede bugün kaç abone var:** üç olayın da üretimde **birer** abonesi var
ve hiçbiri fırlatmıyor. `Battle.UnitStateChanged`'in ikinci abonesi hiç olmadı:
tek dinleyici `BoardAdapter.OnEnable` ve `BoardAdapter.Awake` kendi `Battle`'ını
kendisi kuruyor (`BoardAdapter.cs:238`), yani bir savaşa ikinci adaptör
bağlanamıyor. Testlerde de her `[Test]` kendi `Battle`'ını kurup tek bir `+=`
yazıyor (`BattleTests.cs:804, 845, 874, 897`).

██ Önem kazanacağı gün ██ — **`Battle.UnitStateChanged`'e ikinci bir abone (ses,
skor, başarım) eklendiği gün** iki şey birden doğar. (1) Sıra bir davranış hâline
gelir: görsel mi önce güncellenir ses mi önce çalar, ve bunu belirleyen şey
`OnEnable` sıralaması — yani Unity'nin bileşen sırası. (2) Yeni abone fırlattığı
gün `BoardAdapter` görseli **hiç** güncellenmez ve yukarıdaki `remainingSeconds`
hasarı üretimde görünür. İkisini de görünür kılan koşul aynı satırdır.

---

## Dördüncü durak: `?.Invoke` — az bilinen ikinci işi

```csharp
StateChanged?.Invoke(previous, next);      // Combatant.OnLifecycleStateChanged
```

**Birinci işi:** abonesi olmayan olay `null`'dur, `?` olmadan
`NullReferenceException` — ölçüsü ve projede koşan testi
[04'ün üçüncü durağında](04-delege-olay-ve-kapanis.md#ucuncu-durak-invoke-abonesi-olmayan-olay-nulldur).

**İkinci işi** — ve elle yazılan null kontrolünden farkı burada:

```
StateChanged?.Invoke(previous, next);      if (X != null) X.Invoke(...);
  ① alanı YEREL bir değişkene kopyala        ① ALANI oku ve sına
  ② YEREL değişkeni null'a karşı sına        ② ALANI ██ İKİNCİ KEZ ██ oku
  ③ YEREL değişken üstünden Invoke et           ve Invoke et
     ██ alanı BİR KEZ okur ██                      ▲
                                     ██ İKİ OKUMA ARASI: başka bir yol -=
                                        yapıp alanı null'a düşürebilir ██
```

`?.` bu aralığı kapatıyor, çünkü ikinci okuma diye bir şey yok. İkinci duraktaki
değişmezlikle birleşince bir garantiye dönüşüyor: kopyalanan delege nesnesi
**değişmez** olduğu için yayın başladıktan sonra listeye yapılan hiçbir
ekleme/çıkarma o yayına giremez. ([04](04-delege-olay-ve-kapanis.md#delege-nesnesi-degismez-bunun-ustune-kurulu)
sonucu söylüyor; buradaki katkı, sonucu üreten iki mekanizmayı — tek okuma ve
`Combine`'ın yeni nesnesi — yan yana koymak.)

**Bu projede ölçüsü — tek iş parçacığı mı:** `Assets/Game/` altında `Thread`,
`Task<`, `async `, `await `, `IJob`, `Parallel.`, `lock (` ara: **hiç eşleşme
yok**. Zaman `Tick(float)` ile dışarıdan geliyor ve onu çeviren tek yer Unity'nin
ana döngüsü. İki okuma arasına girecek ikinci bir yol bugün **yok**; `?.` ile
elle yazılan kontrol bugün birebir aynı sonucu verir. ██ Önem kazanacağı gün ██:
`Tick` bir Unity Job'ından ya da bir `Task`tan çağrıldığı gün, ya da bir abone
kendi `-=`'ini bir zamanlayıcı geri çağrısından yazdığı gün — o gün
`if (X != null) X.Invoke()` bir yarış penceresi taşır, `?.Invoke` taşımaz.

Sahip etiketi: `?.` = **C# dili**. Vaadi dar ve tam olarak şu: **sol taraf bir
kez değerlendirilir.** `Invoke`ın iş parçacığı güvenli olduğunu ██ VAAT ETMEZ ██.

---

## Beşinci durak: ██ OPERATÖRÜN SORDUĞU PASAJ ██ — abonelik neden EN SONDA

Kurucunun gövdesi, sırasıyla:

```csharp
public Combatant(Health health, UnitLifecycle lifecycle,
                 AttackProfile attackProfile, Team team = Team.None)
{
    this.health = health ?? throw new ArgumentNullException(nameof(health));          // ①
    this.lifecycle = lifecycle ?? throw new ArgumentNullException(nameof(lifecycle)); // ②
    AttackProfile = attackProfile                                                    // ③
        ?? throw new ArgumentNullException(nameof(attackProfile));
    Team = team;                                                                     // ④

    lastObservedState = this.lifecycle.State;                                        // ⑤
    this.lifecycle.StateChanged += OnLifecycleStateChanged;                          // ⑥
}
```

Mekanizma, ⑥ çalıştığı an:

```
   ⑥ ─► new Action<UnitState>(this.OnLifecycleStateChanged)
        │     Target = ██ bu Combatant ██  (kurulumu HENÜZ bitmedi)
        └─► lifecycle.add_StateChanged(...) → Delegate.Combine → listeye girdi

   BU ANDAN İTİBAREN:  lifecycle ──► bu Combatant'a ULAŞABİLİR
```

Kurucu ⑥'dan sonra bitiyor, dolayısıyla bugün sorun yok. Sorunu doğuran şey
sıranın **bozulması**.

### REDDEDİLEN — abonelik doğrulamalardan ÖNCE

```csharp
// Combatant.cs kurucusu — reddedilen sıra:
this.health = health ?? throw new ArgumentNullException(nameof(health));
this.lifecycle = lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));

lastObservedState = this.lifecycle.State;
this.lifecycle.StateChanged += OnLifecycleStateChanged;   // ← ① BURADA abone olundu

AttackProfile = attackProfile ?? throw new ArgumentNullException(nameof(attackProfile));
//                                     ▲
//                            ② BURASI FIRLATIRSA
Team = team;
```

**KIRILAN:** ② fırlar, `new` bir nesne **döndürmez** — çağıranın elinde hiçbir
`Combatant` yoktur, dolayısıyla `-=` yazabileceği bir yer de yoktur. Ama ① zaten
çalışmıştır: yarım kurulmuş `Combatant`, `lifecycle`'ın çağrı listesinde
**durmaktadır**. `lifecycle` bunu bilmez; kurucunun fırlattığı ona hiçbir yolla
söylenmez, bildiği tek şey listesinde bir `Target` olduğudur. Zincirin geri
kalanı `lifecycle`'ı kimin tuttuğuna bağlı ve iki dal da gerçek:

```
A) lifecycle'a başka referans YOK
   → yarım Combatant + lifecycle birlikte çöp olur, görünür zarar YOK
     ██ ama bu bir tasarım güvencesi değil, o günkü çağrı yerinin şansı ██

B) çağıran lifecycle'ı ELİNDE TUTUYOR (parametre olarak o verdi — normal yol)
   → yarım Combatant toplanamaz: Target onu tutuyor (birinci durak)
   → çağıran aynı lifecycle'ı İKİNCİ bir Combatant'a verirse
        çağrı listesi = [ yarım Combatant , sağlam Combatant ]
   → bir sonraki geçişte İKİSİ de çalışır; yarımın alanları atanmamıştır
        (AttackProfile = null, Team = Team.None) ve üçüncü duraktaki sıra
        kuralı gereği ÖNCE o çalışır. Bir NullReferenceException fırlatırsa
        ██ sağlam Combatant o yayını HİÇ duymaz ██
```

**KAZANIRDI:** Bu sıra yalnız tek bir senaryoda doğru olurdu — **kurucunun
kendisi bir geçiş DOĞURDUĞU** gün. Kurucu `health.Current == 0` görüp
`lifecycle.OnHealthDepleted()` çağırsaydı o geçiş bir yayın üretirdi ve sonradan
abone olan `Combatant` onu **kaçırırdı**; `lastObservedState` daha doğmadan bayat
olurdu. O dünyada sıra "doğrula → abone ol → tetikle" olmak zorundadır.

Bugün o dünyada değiliz ve ölçüsü var: `UnitLifecycle`'ın kurucusu durumu
`State = UnitState.Alive;` diye **`State`'in kendi `private set`'i üzerinden**
yazıyor, `SetState` üzerinden değil — ve yayını yapan tek yer `SetState`'tir,
yani kuruluş bir yayın üretmiyor. `Combatant`'ın kurucusu da `lifecycle`'ın
yalnızca `State`'ini **okuyor**. Kurucu boyunca sıfır yayın.

### Aynı kuralın bir üst katta ÇALIŞAN testi

Aynı sıra kararı `Battle.AddUnit`'te bir kez daha veriliyor ve orada kararı
koruyan bir test **var**:

```
BattleTests.UnitStateChanged_IsNotWiredByARejectedAdd
   battle.AddUnit(soldier, combatant, 9, 9)  ► ArgumentOutOfRangeException
   sonra battle.UnitStateChanged += log.Record  ve  combatant.TakeDamage(10)
   ►  log.Count == 0  ██ reddedilen ekleme TEK BİR abone bırakmadı ██
```

`Combatant` kurucusunda bu testin karşılığı **yok**: kurucu fırlattığında elinde
sınayacak bir nesne kalmıyor, testin şekli de farklı olmak zorunda (birinci
duraktaki `DescribeSubscribers` gibi bir ölçü üyesi gerekirdi). Kuralı bugün
tutan şey testler değil, satır sırası.

### İŞ BÖLÜMÜ: ⑤ ile ⑥ örtüşmez, bölüşür

```
⑤ lastObservedState = this.lifecycle.State;
      kapatır  : "ilk yayında ÖNCEKİ durum ne olacak"
      silinirse: alan default(UnitState) = Alive kalır ve bugün DOĞRU cevabı
                 verir ██ ama bu bir tesadüf ██ — UnitState listesinde Alive
                 başta duruyor. Sıra değişirse yanlış "önceki durum" yayılır.

⑥ this.lifecycle.StateChanged += OnLifecycleStateChanged;
      kapatır  : "geçişleri kim duyacak"
      silinirse: hiçbir geçiş duyulmaz, Combatant.StateChanged bir daha hiç
                 tetiklenmez, ekran donuk kalır
```

⑤'in ⑥'dan **önce** olması da ayrı bir karar: ⑥'dan sonraki ilk yayın
`lastObservedState`i okumak zorunda. Ters sırada bugün gözle görülür fark yok,
çünkü kurucu ile ilk yayın arasında hiçbir şey çalışmıyor; ██ önem kazanacağı
gün ██, `+=` ile kurucunun sonu arasına yayın üretebilen bir satır girdiği gün.

### "Aboneliğin çözüldüğü yer yok" — bunun KOŞULU

Yorumun son cümlesi bir ihmal itirafı değil, bir sahiplik cümlesi: `Combatant`
`lifecycle`'ın **sahibi**, ikisi aynı anda doğar aynı anda çöp olur, abonelik
bir sahiplik sınırı geçmiyor — sökülecek bir şey yok. Ölçü: `Combatant.cs`'te
`lifecycle` `private readonly` bir alan ve dışarıya **hiçbir üye** onu vermiyor
(`State`, `RemainingSeconds`, `IsReadyForCleanup` üçü de `lifecycle`'ın
**değerini** geçiriyor, kendisini değil); erişimi olan tek yol, onu kurucuya
veren çağırandır.

██ Borç doğuracağı koşul ██ — sahiplik değiştiği gün: `lifecycle` bir havuzdan
gelmeye başladığı, ya da iki `Combatant` bilerek aynı `lifecycle`'ı paylaştığı
gün. O gün yayıncı aboneden **uzun yaşar** ve `-=` borcu doğar; borcun şekli de
belli — `Combatant` bir `Detach()` üyesi kazanır ve onu çağırmayı unutmanın
bedeli `Battle.RemoveUnit`'teki ile aynı olur: hata yok, uyarı yok, sessiz bir
hayalet abone.

Bu kararın **gerekçe** tarafı — hangi alternatifin neden reddedildiği —
[`../kod/Core/Combat/Combatant.md`](../kod/Core/Combat/Combatant.md#karar-3-abonelik-kurucunun-en-sonunda)'de.
Burada yalnız mekanizma var.

---

## Altıncı durak: `event` ile Unity mesaj geri çağrıları AYNI ŞEY DEĞİL

`BoardAdapter`'ın üç satırı iki dünyayı yan yana koyuyor:

```csharp
private void OnEnable()                             // ← Unity dünyası
{                                                   //
    battle.UnitStateChanged += OnUnitStateChanged;  // ← C# dünyası
}
```

`OnEnable` bir olay abonesi **değildir**: hiçbir yerde `+=` ile bir yere
yazılmamıştır ve yazılamaz. `OnUnitStateChanged` ise abonedir.

| | `event Action<…>` (`UnitStateChanged`) | Unity mesajı (`OnEnable`, `Awake`, `Update`) |
|---|---|---|
| sahip | **C# dili** + **.NET kütüphanesi** | **Unity motoru** |
| nasıl bağlanır | `+=` ile, çalışma anında | ██ AD ile ██ — motor metodu adına göre arar |
| delege üretilir mi | evet | ██ hayır ██ |
| `Target` / `Method` | var, okunabilir | yok — ortada delege nesnesi yok |
| çağrı listesi | var, sıralı, çok elemanlı | yok — bileşen başına tek metot |
| `-=` ile sökülür mü | evet | ██ hayır ██ — bileşen yok edilene kadar bağlı |
| `private` engel mi | evet: dışarıdan `+=` yazılamaz | ██ hayır ██ — motor `private` metodu çağırır |
| yanlış yazılırsa | derleme hatası (ad çözülmez) | ██ SESSİZ ██ — hiç çağrılmaz |

**Ölçü:** `BoardAdapter.OnEnable`'ı `OnEnabled` diye yeniden adlandır. Kod
derlenir, tek bir uyarı çıkmaz, abonelik bir daha hiç kurulmaz — ekran durum
değişikliklerini duymaz. Aynı yanlışı `OnUnitStateChanged` üstünde yap (`+=`
satırındaki adı bozmadan metodu yeniden adlandır): derleyici **CS0103** verir.
Aradaki fark iki farklı mekanizma olduğunun kanıtıdır. İkinci ölçü `private`
üstünden: `OnEnable` `private`'dır ve motor onu yine de çağırır — çağıran taraf
C# erişim kurallarından geçmiyor.

> **SINIR — sahibi başka bir dosya:** Unity yaşam döngüsünün kendisi
> (`Awake` → `OnEnable` → `Start` → `Update` → `OnDisable` → `OnDestroy` sırası,
> `OnEnable`/`OnDisable` çiftinin neden `Awake`/`OnDestroy` çiftine tercih
> edildiği) bu dosyanın işi değil:
> [`konular/08-motor-cagri-dongusu.md`](../konular/08-motor-cagri-dongusu.md#ikinci-durak-cagri-sirasi-sahipleriyle-ezberle-degil).
> Burada yalnız **fark** var.

---

## Üç oyun: "bir şeyin olduğunu başkasına nasıl duyurursun"

| Oyun | Aynı basıncı taşıyan şeyin ADI ve İŞİ |
|---|---|
| **Slay the Spire** | Kalıntılar ve güçler. Bir kart oynandığında, hasar alındığında, tur bittiğinde ellerindeki iş tetiklenir. Aynı ana kayıtlı birden çok kalıntı vardır ve **hangisinin önce işlediği sonucu değiştirir** — oyuncu bunu envanter sırasında görür. |
| **Vampire Survivors** | Binlerce düşmanın her biri. Ölen düşman deneyim taşı bırakır, sayacı ilerletir, bazen bir silahı tetikler — aynı an, ekranda aynı karede yüzlerce kez. |
| **Stardew Valley** | Gün sonu. Tek bir anda ekinler büyür, hayvanlar ürün verir, makineler işini bitirir, takvim ilerler; birbirini tanımayan çok sayıda farklı iş aynı ana bağlıdır. |

### ██ EŞLEŞMEYEN SATIR: Vampire Survivors ██

En öğretici satır bu, çünkü bizim mekanizmamız oraya **oturmuyor**:

```
BİZDE (Battle.AddUnit)                  ORADA (binlerce varlık)
──────────────────────                  ───────────────────────
birim başına 1 kapanış nesnesi          varlık başına 1 kapanış + 1 liste
          + 1 sözlük girdisi            girişi → × binlerce nesne, ve her
          + 1 çağrı listesi girişi      ölümde bir dolaylı delege çağrısı
ölçü: sahnede 2 birim var (Vanguard,    ██ o ölçekte olay başına TAHSİS ve
Raider — BoardAdapter.Awake) → 2 kapanış   İŞ YÜKÜ demektir ██
```

Asimetrinin adı: bizde abone sayısı **birim sayısıyla** büyüyor ve birim sayısı
iki. O tarafta aynı şekil binlerce nesne ve kare başına binlerce dolaylı çağrı
üretirdi; o ölçekte "haber ver" yerine "her kare listeyi tara" şekli kazanır.
Şekli seçen şey mekanizmanın güzelliği değil, **N**.

### `HENÜZ YOK` satırları

```
Slay the Spire'ın SIRA duyarlılığı  ► bizde HENÜZ YOK. Ölçü: üç olayın da
   üretimde TEK abonesi var, sıra diye bir gözlem yok. Doğuracak aşama:
   Battle.UnitStateChanged'e ikinci abone (ses / skor) eklendiği aşama.

Stardew'in "tek an, çok iş"i        ► bizde HENÜZ YOK. Ölçü: TurnState tur
   değişimini olayla duyurmuyor, EndTurn bir bool döndürüyor ve soran zaten
   orada. Doğuracak aşama: tur başına iş yapan ikinci bir sistem (yenilenme,
   gelir, hava) doğduğu aşama.

Vampire Survivors'ın ÖLÇEĞİ         ► bizde YOK, ve bu bir yol haritası değil
   bir SINIR: şekil değişmeden ölçek büyürse fatura tahsis olarak gelir.
```

---

## Tek bakışta: `+=` satırından yayına

```
   lifecycle.StateChanged += OnLifecycleStateChanged;
        │  DERLEYİCİ (C# dili)
        ▼
   new Action<UnitState>(this.OnLifecycleStateChanged)
        Target = this  ██ YARIM KURULMUŞSA BURADA YAKALANIR ██
        Method = OnLifecycleStateChanged
        │  add_StateChanged(...) ← event'in ürettiği metot
        ▼
   Delegate.Combine(eski, yeni) ← .NET: ██ YENİ NESNE ██, eski kımıldamadı
        │  Interlocked.CompareExchange ile gizli alana yazıldı
        ▼
   gizli alan: [ h1 , h2 , h3 ]  ← sıra = abonelik sırası
        │  yayın anı: StateChanged?.Invoke(next)
        ▼
   ① alan YEREL değişkene KOPYALANIR  ② null mı bakılır  ③ liste SIRAYLA yürür
        └─► h1 fırlarsa ██ h2 ve h3 HİÇ çağrılmaz ██, ve yayıncının
            Invoke'tan SONRAKİ satırları da çalışmaz
```

---

## Kural: bir `+=` satırını nereye yazarsın

```
① Bu satır bir KURUCUNUN içinde mi?      hayır → ③   ·   evet → ②

② Kurucunun geri kalanı FIRLATABİLİR mi (null/aralık kontrolü, sözlüğe ekleme)?
      evet  → ██ += EN SONA ██ — fırlayan kurucu geriye nesne döndürmez,
              dolayısıyla -= yazacak bir yer de bırakmaz. Başka koruma YOK.
      hayır → yine en sona yaz; bedeli sıfır ve bir sonraki doğrulama
              eklendiğinde kural kendiliğinden tutar

③ Kurucunun/metodun KENDİSİ bir yayın üretiyor mu (abone olduğun nesne
   üstünde durum değiştiren bir çağrı)?
      evet  → ██ += o çağrıdan ÖNCE ██, yoksa ilk geçiş kaçırılır; sıra
              "doğrula → abone ol → tetikle" olur
      hayır → ②'nin cevabı geçerli

④ Yayıncı, aboneden UZUN yaşayacak mı? (ölçü: yayıncıya erişimi olan başka
   kimse var mı)
      hayır → -= borcu YOK; nesne ölünce olay da ölür
      evet  → ██ -= BORCU DOĞDU ██, derleyici hatırlatmaz; sökme yerini
              AYNI turda yaz (OnEnable ↔ OnDisable gibi)

⑤ Olayın ikinci bir abonesi olabilir mi?
      evet → SIRA bir davranış hâline gelir ve bir abonenin fırlatması
             ötekileri sessizce düşürür. Fırlatabilecek abone kendi gövdesini
             try/catch'e alır — ██ Invoke bunu senin için YAPMAZ ██
```

---

## Yanlış hatırlanan üç şey

**"Delege bir fonksiyon işaretçisidir."** Değil. İki alanlı bir nesnedir:
`Target` (üstünde çalıştırılacak nesne) ve `Method`. İşaretçinin hedefi yoktur;
delegenin vardır ve o hedef **hayatta tutulur**. Abone olmak, yayıncıya abonenin
adresini vermektir — sızıntı okunun yayıncıdan aboneye gitmesinin sebebi budur.

**"`+=` aboneyi listeye ekler."** Eklemez. `Delegate.Combine`'ı çağırır, o
**yeni bir delege nesnesi** döndürür ve gizli alana o yazılır. Eski nesne olduğu
gibi durur — yayın sürerken yapılan bir `-=`'in o yayını etkilememesinin sebebi
tam olarak bu. "Liste" dediğimiz şey her `+=`'te yeniden doğar.

**"Olay yayınlamak güvenlidir; biri patlasa ötekiler yine çalışır."** Çalışmaz.
`Invoke` bir `try`/`catch` taşımaz: birinci abone fırlarsa ikinci ve üçüncü hiç
çağrılmaz, istisna yayıncının `Invoke` satırından yukarı çıkar ve yayıncının o
satırdan sonraki kendi satırları da çalışmaz —
`UnitLifecycle.OnHealthDepleted`'te bu, `remainingSeconds`in atanmaması, yani
10 saniyelik kurtarma penceresinin tek karede yok olması demektir.

---

## Kaçış yolu: `event` + delege yerine ne olurdu

```
dinleyici arayüzü listesi (List<IStateListener>)
     → Target/Method ikilisi ortadan kalkar: elinde ZATEN nesne vardır,
       Remove referansla çalışır, sökme sessizce başarısız olamaz. Bedeli
       her dinleyici için bir tip; ve tutma yönü DEĞİŞMEZ —
       ██ liste de dinleyiciyi hayatta tutar ██.

zayıf referanslı olay (WeakReference<T> ile abone tutmak)
     → tutma yönü kırılır, yayıncı aboneyi hayatta tutmaz. Bedeli iki katlı:
       her yayında bir canlılık sınaması ve ölü abonelerin ne zaman
       temizleneceği sorusu. Bu projede kazancı SIFIR — tek sahiplik sınırı
       Battle'da ve orası zaten açıkça söküyor.

her kare TARAMA (olay yok; Battle her Tick'te durumları karşılaştırır)
     → Vampire Survivors satırının şekli. Tahsis yok, çağrı listesi yok, sıra
       sorunu yok, istisna tuzağı yok. Bedeli "önceki durum"u bilen ikinci bir
       tablo ve N birim için kare başına N karşılaştırma: 2 birimde israf,
       5.000 birimde doğru şekil.

dönüş değeri (olay yerine metodun cevabı)
     → UnitLifecycle'ın kendi yorumunda tartılmış ve REDDEDİLMİŞ: Tick
       içindeki Downed → Dead geçişini SORAN yoktur. Kaçış yolu değil,
       YANLIŞ yol.
```

`event Action<…>` bugünkü N'de dördünün ortasını tutuyor: ek tip yok, yayın
başına tahsis yok, sökme yeri tek bir katmanda toplanmış. Ödenen faturalar bu
dosyanın duraklarıdır — `Target`ın tuttuğu referans, sıranın görünmezliği ve
`Invoke`ın istisna taşımaması.

---

Kodda **karar**, burada **ödünç alınan makinenin işleyişi**. İkisi çelişirse kod
kazanır — orası çalışan metin, burası anlatı. Ve bu ağaçta ikinci bir otorite
daha var: dil ve BCL davranışı için son söz **derleyicinin ve çalışma
zamanının**. Yukarıdaki her iddia ya bir derleyici hata koduna (CS0070, CS0103)
ya da koşturulabilir bir deneye bağlı; bağlanmamış bir iddia varsa o bir
kusurdur, üslup değil.
