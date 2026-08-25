# Delegenin arka tarafı — `+=` derleyicide neye dönüşür

> **HANGİ DİL ARACI** — *bu dosyanın anlattığı, ödünç alınmış makine:*
> `+=` / `-=`'in indiği `Delegate.Combine` / `Delegate.Remove` · `Target` +
> `Method` · derleyicinin `event` için ürettiği `add_`/`remove_` metotları · çağrı
> listesi ve `Invoke`
>
> **NEREDE GEÇİYOR** — *bu makinenin bu projede yaşadığı yerler:*
>
> | dosya | üye |
> |---|---|
> | `Assets/Game/Core/Combat/Combatant.cs` | `Combatant(...)` kurucusunun **son iki satırı** · `StateChanged` |
> | `Assets/Game/Core/Combat/UnitLifecycle.cs` | `StateChanged` · `SetState` |
> | `Assets/Game/Battle/Battle.cs` | `stateForwarders` · `UnitStateChanged` |
> | `Assets/Game/Unity/BoardAdapter.cs` | `OnEnable` · `OnDisable` |
>
> **NE ZAMAN OKU** — *hangi soruyu sorduğunda ya da hangi değişikliğe giriştiğinde:*
> bir `+=` satırını bir KURUCUNUN içine yazarken; bir olaya ikinci abone
> eklerken; ya da "abone olmak neyi hayatta tutuyor" diye sorduğunda.

**BURAYA KODDAN GELDİYSEN** — aşağıdaki üyelerin **yorumunda** bu belgeye bir
`DİL:` işaretçisi var (`dil/` ağacının işaretçisi `DERİN ANLATIM:` değil,
***`DİL:`***). Yol: `Ctrl+P` → dosya adı → `Ctrl+F` ile **üye adını** ara.
***Satır numarası bilerek yazılmıyor: satır kayar, üye adı kaymaz.***

| dosya | üye | koddan işaretçi |
|---|---|---|
| `Assets/Game/Core/Combat/Combatant.cs` | `Combatant(...)` kurucusu (son iki satır) | ✓ |
| `Assets/Game/Core/Combat/UnitLifecycle.cs` | `SetState` (`?.Invoke` satırı) | ✓ |
| `Assets/Game/Battle/Battle.cs` | `stateForwarders` | ✓ |
| `Assets/Game/Unity/BoardAdapter.cs` | `OnEnable` | ✓ |

**Bu dosya `dil/` ağacının *en iyi bağlanmış* belgesi: dört üretim
dosyasından dördüne de kod işaretçisi var. Ama dikkat — o dört işaretçi
`04`'ü *atlıyor* ve doğrudan buraya geliyor; oysa `04` okunmadan buraya
girilmemesi gerekiyor (aşağıdaki paragraf bunu yazıyor).**

> **▶ ARA DURAK:** [04-delege-olay-ve-kapanis.md](04-delege-olay-ve-kapanis.md#birinci-durak-delege-metoda-isaret-eden-nesne)
> **NEDEN:** **zincirdeki *tek açık ön koşul beyanı* budur.** `04` bu
> malzemenin **sözleşmesini** anlatıyor: `Action<…>` nasıl okunur, `Func` neden
> hiç yok, `event` ile düz alanın farkı, abonesi olmayan olayın neden `null`
> olduğu, iki kapanışın neden eşit olmadığı. ***Orayı okumadan buraya girme;
> burada hiçbiri tekrar edilmiyor.***
> **DÖNÜŞ:** bu dosyanın [«Birinci durak: delegenin İÇİ — `Target` + `Method`»](#birinci-durak-delegenin-ici-target-method) bölümü

Bu dosya bir alt kat:

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
║             >> BU DOSYANIN TAMAMI BU SATIRDAN ÇIKIYOR <<       ║
╚═══════════════════════════════════════════════════════════════╝

╔═ MulticastDelegate + GetInvocationList()  ·  .NET kütüphanesi ╗
║  Ne yapar : Delegate'ten türer, ÜSTÜNE bir çağrı listesi koyar;║
║             GetInvocationList() o listeyi KOPYALAYIP verir     ║
║  Vaadi    : `delegate` ile üretilen HER tip buradan türer      ║
║             (Action<…> dahil); sıra = abonelik sırası          ║
║  BİLMEZ   : abonelerin birbirini — biri patlarsa ötekini       ║
║             KURTARMAZ >> Invoke try/catch TAŞIMAZ <<           ║
╚═══════════════════════════════════════════════════════════════╝

╔═ Delegate.Combine / Delegate.Remove  ·  .NET kütüphanesi ═════╗
║  Ne yapar : iki delegeyi birleştirir / birinden ötekini çıkarır║
║  Vaadi    : her çağrıda >> YENİ NESNE <<; verilenler kımıldamaz║
║  BİLMEZ   : aynı hedef+metodun listeye ikinci kez girdiğini —  ║
║             ELEMEZ. Remove yalnız SONUNCU eşleşmeyi çıkarır.   ║
╚═══════════════════════════════════════════════════════════════╝

╔═ event (anahtar kelime)  ·  sahip: C# dili ═══════════════════╗
║  Ne yapar : TEK alan gibi yazılır, ÜÇ üye üretir               ║
║             (gizli alan + add_X + remove_X)                    ║
║  Vaadi    : dışarıya yalnız add ve remove yüzünü gösterir      ║
║  BİLMEZ   : >> İÇERİDE hiçbir şey << — bildiren tipin gövdesi  ║
║             gizli alanı sıradan bir alan gibi görür            ║
╚═══════════════════════════════════════════════════════════════╝
```

### Dört kutunun GERÇEK SATIRLAR tarafındaki karşılığı

Yukarıdaki dört kutu ödünç alınmış makineyi anlatıyor. Aşağıdaki dört blok o
makinenin bu depoda hangi satırda çalıştığını gösteriyor. **Dördü de ÖDÜNÇ TİP,
yani gösterilen yer *tanım yeri değil KULLANIM YERİDİR*** — `System.Delegate`'i
biz yazmadık, onu çalıştıran satırı biz yazdık.

**`System.Delegate` bu projede** — `Assets/Game/Core/Combat/Combatant.cs` → `Combatant(...)` kurucusu

```csharp
this.lifecycle.StateChanged += OnLifecycleStateChanged;
```

Kutudaki «İKİ ALANLI bir NESNE tutar — Target ve Method» satırının karşılığı tam
bu satır: burada bir delege nesnesi DOĞUYOR. `Target` = kurulmakta olan
`Combatant` örneği (`this`), `Method` = `OnLifecycleStateChanged`. Kutunun
«BİLMEZ: Target'ın kurulmayı bitirip bitirmediğini» satırı da buradan okunuyor —
`Target` bu satırda kurulmayı HENÜZ BİTİRMEMİŞTİR; kurucunun gövdesi bir satır
sonra kapanıyor, ama araya bir doğrulama girseydi `lifecycle`'ın çağrı listesinde
yarım bir nesne kalırdı. ***Kutunun «BU DOSYANIN TAMAMI BU SATIRDAN ÇIKIYOR»
işareti işte bu `.cs` satırını gösteriyor*** — beşinci durak baştan sona bunun
üstüne kurulu.

**`MulticastDelegate` bu projede** — `Assets/Game/Core/Combat/UnitLifecycle.cs` → `OnHealthDepleted`

```csharp
SetState(UnitState.Downed);
remainingSeconds = downedWindowSeconds;
```

***EN ÖĞRETİCİ SEÇİMİ*** — bu kutunun iki yarısı ayrı yerlerde okunuyor ve yalnız
biri kaynakta var. `GetInvocationList()` yarısının **bu projede karşılığı YOK**:
`Assets/` altında sıfır eşleşme, bu yüzden birinci duraktaki `DescribeSubscribers`
ölçüsü üyeyi GEÇİCİ olarak ekletiyor. Doğacağı koşul: bir aboneyi adıyla teşhis
etmek gerektiği gün. `Invoke` yarısının karşılığı ise burada ve üretimde duran en
pahalı iki satır bunlar. Kutunun «BİLMEZ: abonelerin birbirini — biri patlarsa
ötekini KURTARMAZ ***Invoke try/catch TAŞIMAZ***» satırı tam olarak bu iki satırın
**arasından** geçiyor: birinci satır içeride `StateChanged?.Invoke(next)`
çalıştırıyor; bir abone fırlatırsa istisna oradan yukarı çıkar ve kurtarma
penceresinin süresini kuran ikinci satır hiç çalışmaz.

> ***YOKLUK SENEDİ*** — `GetInvocationList()`
>
> **① HANGİ ÖZELLİK:** Oyuncu bugün bir birim öldüğünde ekranda yalnızca
> tahtanın boyandığını görüyor. Kaç düşmanın kaldığını, kazanmaya ne kaldığını
> hiçbir yerde okuyamıyor. Zafer koşulu paneli geldiği gün aynı ölüm iki iş
> birden yapacak: tahta boyanacak ve "kalan düşman" sayacı bir düşecek.
>
> **② NEREYE BAĞLANIR:** `Assets/Game/Battle/Battle.cs` → `AddUnit`. Yükseltmeyi
> yapan yönlendirici kapanış bu metodun içinde kuruluyor. İkinci durak
> `Assets/Game/Unity/BoardAdapter.cs` → `OnEnable`; bugünkü tek abone orada
> bağlanıyor ve panel de onun yanına girecek.
>
> **③ NE KIRAR:** Bugünkü yükseltme ne `try`/`catch` taşıyor ne de abone başına
> yalıtım. Tek abone varken bu görünmüyor. İkinci abone panel olduğu gün panelin
> fırlattığı bir istisna çağrı listesini ortadan kesiyor. `BoardAdapter` o ölümü
> hiç duymuyor ve az önce ölen birim tahtada ayakta kalıyor. Hangi test kızarır?
> Bugün hiçbiri: `Assets/Tests/EditMode/Battle/BattleTests.cs` dört
> `UnitStateChanged` testi taşıyor ve dördü de tek abone kuruyor. Fırlatan abone
> kuran testi yazmak o günün ilk borcudur.
>
> **④ KARARMETRE:** `GetInvocationList()` hiç var olmasaydı da zafer koşulu
> paneli istenir miydi? EVET. Zafer koşulunu göstermeyen bir strateji oyunu
> eksiktir; bu özellik oyunun yol haritasında duruyor, dilin yol haritasında
> değil. Mekanizmayı elverişliden zorunluya çeviren değişmez şudur:
> ***tahta, hangi birimin canlı olduğu konusunda asla yalan söylemeyecek***.
> Bu cümle olmadan iki abone de sorunsuz çalışır. Bu cümleyle biri ötekini
> susturamaz, ve abone başına yalıtımın tek yolu çağrı listesini elle gezmektir.
>
> **⑤ ARAŞTIRMA BORCU:** GEREKİYOR, adresi `performance-research`. Soru üç
> parçalıdır. (a) `Delegate.GetInvocationList()` her çağrıda yeni bir dizi
> tahsis ediyor mu, üç elemanlı bir liste için çağrı başına kaç bayt düşüyor?
> (b) `AddUnit` ile `RemoveUnit` listeyi değiştirdiğine göre dizi iki yükseltme
> arasında önbelleğe alınabilir mi, geçersizleştirme hangi üyeye düşer?
> (c) Her elemanı somut temsilci tipine çevirip doğrudan çağırmak,
> `Delegate.DynamicInvoke`'un IL2CPP altında taşıdığı yansıma maliyetinden
> kaçınıyor mu? Kanıt biçimi tarihli birincil kaynak ile yerel bir EditMode
> tahsis ölçümüdür.

**`Delegate.Combine` / `Delegate.Remove` bu projede** — `Assets/Game/Unity/BoardAdapter.cs` → `OnEnable`

```csharp
private void OnEnable()
{
    battle.UnitStateChanged += OnUnitStateChanged;
}
```

***EN ÖĞRETİCİ SEÇİMİ*** — üretimde üç `+=` var (`Combatant` kurucusu,
`Battle.AddUnit`, `BoardAdapter.OnEnable`); seçilen bu, çünkü kutunun İKİ adı da
yalnız burada yan yana duruyor: aynı dosyanın birkaç satır aşağısındaki `OnDisable`
`battle.UnitStateChanged -= OnUnitStateChanged;` satırıyla `Delegate.Remove`
tarafını yazıyor. Kutudaki «her çağrıda ***YENİ NESNE***; verilenler kımıldamaz»
satırının karşılığı bu `+=`; kutunun «BİLMEZ: aynı hedef+metodun listeye ikinci
kez girdiğini — ELEMEZ. Remove yalnız SONUNCU eşleşmeyi çıkarır.» satırının
faturası da burada kesiliyor: `OnEnable` bir `OnDisable` görmeden ikinci kez
çağrılırsa `Combine` şikâyet etmez, aynı abone listeye iki kez girer ve tek bir
`-=` yalnız sonuncusunu çıkarır. Simetriyi tutan tek şey bu iki metodun çifti.

**`event` bu projede** — `Assets/Game/Core/Combat/UnitLifecycle.cs` → `StateChanged`

```csharp
public event Action<UnitState> StateChanged;
```

***EN ÖĞRETİCİ SEÇİMİ*** — üretimde üç `event` bildirimi var
(`UnitLifecycle.StateChanged`, `Combatant.StateChanged`, `Battle.UnitStateChanged`);
seçilen bu, çünkü ikinci duraktaki CS0070 ölçüsü de bunun üstünde koşuyor.
Kutudaki «TEK alan gibi yazılır, ÜÇ üye üretir (gizli alan + add_X + remove_X)»
satırının karşılığı tam bu satır: yazılan bir tane, derleyicinin ürettiği üç tane.
Kutunun «BİLMEZ: ***İÇERİDE hiçbir şey*** — bildiren tipin gövdesi gizli alanı
sıradan bir alan gibi görür» satırı ise aynı dosyanın `SetState` metodundaki
`StateChanged?.Invoke(next);` satırından okunuyor: o satır bildiren tipin
**içinde** olduğu için derleniyor, `Combatant.cs`'e taşınsaydı CS0070 verirdi.
Duvarı kuran `event`, duvarın içinde kalan tarafa hiçbir şey yapmıyor.

---

## Birinci durak: delegenin İÇİ — `Target` + `Method`

Bir delege bir "fonksiyon işaretçisi" **değildir**. İki alanlı bir nesnedir:

```
   this.lifecycle.StateChanged += OnLifecycleStateChanged;
                                  ▲ derleyici burada bir NESNE kurar:
                                    new Action<UnitState>(this.OnLifecycle…)

         ╔═════════════════════════════════════════╗
         ║  Target  ──►  o Combatant örneği (this) ║   >> `this` ARTIK BU
         ║  Method  ──►  OnLifecycleStateChanged   ║      NESNENİN İÇİNDE <<
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
      >> Target o LİSTE << — testin kendisi değil, `this` değil, LİSTE
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
   >> YAYINCI ABONEYİ TUTAR << — Combatant'a başka hiçbir referans
   kalmasa bile lifecycle yaşadığı sürece toplanamaz. Sezginin TERSİ.
```

Bu yönün zincir üzerindeki sonucu
[`../konular/01-olay-zinciri.md`](../konular/01-olay-zinciri.md)'nin "Sökülmezse
ne olur" bölümünde; buradaki katkı oku çizen alanın **adı**: `Target`.

**Kapsam — bu projede yön neden zararsız:** `Combatant` `lifecycle`'ın sahibi;
ikisi birlikte doğar, birlikte ölür, ok bir sahiplik sınırını geçmiyor.
***Sınırı geçen tek abonelik `Battle.AddUnit`'teki*** ve orada bırakma zorunlu.

> **◀ DÖNÜŞ:** [07-bellek-canlilik-ve-yikim.md](07-bellek-canlilik-ve-yikim.md#kod-bunu-gercekte-ne-yapiyor-sokme-yeri-var) — «Kod bunu GERÇEKTE ne yapıyor: sökme yeri var»dan
> geldiysen artık şunu biliyorsun: oku çizen alanın **adı** `Target`, ve o alan
> delegenin **içinde** duruyor — ***yayıncı aboneyi tutar, sezginin tersi*** ·
> oraya dön ve sökme yerinden devam et

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

ReferenceEquals(first, second)   ►  false    >> Combine YENİ nesne verdi <<
lifecycle.OnHealthDepleted();
seen                             ►  [Downed, Downed]   >> İKİ KEZ <<
```

Son satır az bilinen yarısı: `Delegate.Combine` **elemez**. Aynı hedef+metot
ikinci kez eklenirse çağrı listesinde iki giriş olur ve abone iki kez çalışır.
Karşılığında `Delegate.Remove` yalnız **sonuncu** eşleşmeyi çıkarır — bir `-=`
bir `+=`'i dengeler, iki `+=`'i değil. Simetriyi tutan şey derleyici değil,
çağıranın disiplini.

**Bu projede bugün ölçüsü:** üretimdeki üç `+=` üç ayrı dosyada ve her biri bir
kez çalışıyor (`Combatant.cs:90`, `Battle.cs:228`, `BoardAdapter.cs:351`).
***Önem kazanacağı gün:*** `BoardAdapter.OnEnable` bir `OnDisable` görmeden
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
***KORUMAZ***.

---

## Üçüncü durak: çağrı listesi ve `Invoke`

Bir delege birden çok aboneyi bir dizide taşır. `GetInvocationList()` o diziyi
kopyalayıp verir; `Invoke` onu **abonelik sırasıyla** yürütür.

```
StateChanged  =  [ h1 , h2 , h3 ]
                   │    │    │
      Invoke(x) ───┴────┴────┴──►  h1(x)  sonra  h2(x)  sonra  h3(x)
                                   >> SIRA = abone olunma sırası <<
```

### ***DÖNÜŞ DEĞERİ TUZAĞI***

[04](04-delege-olay-ve-kapanis.md#funct-r-neden-hic-yok) "`Func` neden hiç yok"
diyor. Arka taraftan sebebi şu: `Invoke` listeyi dolaşırken elinde **tek** bir
dönüş değeri yeri var. Her abone kendi cevabını o yere yazar. Bir sonraki abone
aynı yere yazınca öncekinin cevabı gider.

***Üzerine yazılan şey CEVAP'tır, ABONE değil.*** Üç abonenin üçü de çalışır.
Hiçbiri listeden silinmez. Kaybolan tek şey ilk ikisinin cevabıdır.

```
                                 ┌──────────────┐
 h1() çalıştı ─► cevabı 1 ──────►│   r  =  1    │  yazıldı
 h2() çalıştı ─► cevabı 2 ──────►│   r  =  2    │  üstüne yazıldı
 h3() çalıştı ─► cevabı 3 ──────►│   r  =  3    │  üstüne yazıldı
                                 └──────────────┘
                                  TEK BİR YER      hayatta kalan: SONUNCU
```

```csharp
int sayac = 0;
Func<int> f = null;
f += () => { sayac++; return 1; };
f += () => { sayac++; return 2; };

int r = f();
//  r      ►  2    yalnız CEVAP kayboldu
//  sayac  ►  2    İKİSİ DE ÇALIŞTI — yan etki kaybolmadı
```

`sayac`ın 2 olması birinci lambdanın gerçekten koştuğunun kanıtıdır. İstisna
yok, uyarı yok, atlanan abone yok. Yalnız cevap yok.

**`=` ile karıştırma** — ***"üzerine yazma"*** sözcüğü `=` için apayrı bir şey
anlatır ve orada aboneler gerçekten silinir:

```
f  =  h3     ►  liste: [ h3 ]           h1 ile h2 SİLİNDİ
f += h3      ►  liste: [ h1, h2, h3 ]   hiçbiri silinmedi
```

İkinci durakta okuduğun CS0070 duvarının kapattığı şey tam olarak birinci
satırdır. Bir olaya dışarıdan `=` yazabilseydin, başkasının aboneliklerini tek
satırda silebilirdin. `event` dışarıya `+=` ile `-=` bırakır — ***"ekle"*** ve
***"kendi çıkardığını çıkar"***. Başkasınınkine dokunamazsın.

N aboneden N−1'inin cevabı sessizce kayboluyor; bir olayın imzası bu yüzden
`Func` olamaz. Projede `Func` sıfır kez geçiyor — ölçü: `Assets/` altında
`Func<` ara, hiç eşleşme yok.

### ***İSTİSNA TUZAĞI***

`MulticastDelegate.Invoke` bir `try`/`catch` **taşımaz**:

```
StateChanged.Invoke(Downed)
     ├─► h1(Downed)  ── fırlattı
     ├─✗ h2(Downed)  >> HİÇ ÇAĞRILMADI <<
     └─✗ h3(Downed)  >> HİÇ ÇAĞRILMADI << ─► istisna yayıncının Invoke
                                             satırından yukarı çıkar
```

Ve yayıncı için ikinci bir bedel var: **`Invoke` satırından SONRAKİ kendi
satırları da çalışmaz.**

***Sebebi tek cümledir: `Invoke` bir gönderme değil, bir çağrıdır.*** Abonenin
metodunu o anda, aynı iş parçacığında, aynı çağrı yığınında çalıştırır ve o metot
bitmeden geri dönmez. Ortada bir kuyruk yoktur. Bu yüzden `Invoke`tan sonraki
satır ***"biraz sonra"*** çalışacak bir satır değil; yığının tamamı geri dönene
kadar sırası ***hiç gelmeyen*** bir satırdır.

Zincirin yığın hâli
[../konular/01-olay-zinciri.md](../konular/01-olay-zinciri.md#ayni-zincir-ikinci-bir-cizimle-dort-adim-degil-dort-cerceve)
bölümünde çizili. İstisna oraya düştüğünde yukarı nasıl tırmandığı şudur:

```
        ApplyStateVisual  FIRLATTI                    en derin cerceve
   ^    |
   |    BoardAdapter.OnUnitStateChanged     yarim kaldi
   |    Battle forwarder                    yarim kaldi
   |    Combatant.OnLifecycleStateChanged   yarim kaldi   lastObservedState YAZILDI
   |    UnitLifecycle.SetState              yarim kaldi   State           YAZILDI
   |    UnitLifecycle.OnHealthDepleted      yarim kaldi   remainingSeconds YAZILMADI
   |
 istisna yukari tirmaniyor
```

`State` yazılı kaldı çünkü ataması `Invoke`tan **önceydi**. `remainingSeconds`
hiç yazılmadı çünkü **sonraydı**. Ayıran tek şey satır sırasıdır — ve bu projede
o sıranın bedeli şudur:

```csharp
// UnitLifecycle.OnHealthDepleted — son iki satır
SetState(UnitState.Downed);              // ← içinde StateChanged?.Invoke var
remainingSeconds = downedWindowSeconds;  // >> BİR ABONE FIRLATIRSA ÇALIŞMAZ <<

//  State = Downed  ✓ yazıldı (SetState Invoke'tan ÖNCE atıyor)
//  remainingSeconds ✗ 0'da kaldı → ilk Tick(0.016f): Alive değil,
//  0 - 0,016 ≤ 0, Downed → SetState(Dead)
//  >> 10 saniyelik kurtarma penceresi TEK KAREDE yok oldu <<
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

***Önem kazanacağı gün*** — **`Battle.UnitStateChanged`'e ikinci bir abone (ses,
skor, başarım) eklendiği gün** iki şey birden doğar. (1) Sıra bir davranış hâline
gelir: görsel mi önce güncellenir ses mi önce çalar, ve bunu belirleyen şey
`OnEnable` sıralaması — yani Unity'nin bileşen sırası. (2) Yeni abone fırlattığı
gün `BoardAdapter` görseli **hiç** güncellenmez ve yukarıdaki `remainingSeconds`
hasarı üretimde görünür. İkisini de görünür kılan koşul aynı satırdır.

> **⌨ KODU AÇ:** `Assets/Game/Core/Combat/UnitLifecycle.cs` → `OnHealthDepleted`,
> sonra aynı dosyada `TryRevive`
> **BAK:** iki metodun **şekli aynı** — `SetState(...)` çağrısı, ardından bir
> alan ataması. Biri bir **değişmez** kuruyor (pencerenin süresi), öteki zaten
> sağlanmış bir şeyi tekrar yazıyor. ***Aynı şekil, iki ayrı risk.***
> **DÖNÜŞ:** bu dosyanın «***İSTİSNA TUZAĞI***» bölümü

> **◀ DÖNÜŞ:** [../konular/01-olay-zinciri.md](../konular/01-olay-zinciri.md#3-bir-abone-firlarsa-sokulme-degil-yayin-faturasi) — «③ Bir abone FIRLARSA»dan
> geldiysen artık şunu biliyorsun: `Invoke`'un `try`/`catch` taşımaması bir
> gözden kaçış değil, `MulticastDelegate`'in tanımı — ***hatanın kaynağı ile
> faturasını ödeyen ayrışıyor***, ve aynı dosyadaki `TryRevive` bunun zararsız
> ikizini taşıyor · oraya dön ve kaldığın yerden devam et
---

### Bu iki tuzağı da kapatan yapı

Üstteki iki tuzak ayrı arızalardır ama aynı yapı ikisini birden kapatır: yayıncı
ile aboneyi ayıran bir **dağıtıcı**. Ayrıntısı ve bu projedeki tetikleyici
koşulu şurada yazılı:
[../../ogrenme/02-sonraki-asamalar.md — Aşama 3 · Olay veri yolu](../../ogrenme/02-sonraki-asamalar.md#asama-3-olay-veri-yolu-event-bus).

```
ARIZA A — İSTİSNA YAYILMASI          bu bölümün konusu
  bir abone fırlatır → kalan aboneler hiç çağrılmaz
                     → yayıncının Invoke'tan SONRAKİ satırları da çalışmaz
  KAPATAN: dağıtıcı GetInvocationList() üstünde tek tek dolaşır ve
           her aboneyi kendi try/catch'ine alır

ARIZA B — ABONELİK ŞİŞMESİ / SIZINTI  ayrı arıza, aynı yapı kapatıyor
  += , -= 'den daha sık çalışır → aynı abone listede iki kez
                                → yayıncı aboneyi hayatta tutar (sızıntı)
  KAPATAN: dağıtıcı aboneliğe karşılık bir iptal tokeni verir;
           ömrü derleyici değil o token tutar
```

**Bu projede B bugün YOK, ve tutan şey derleyici değil disiplindir.** Ölçü:
`Assets/Game` altında olay aboneliği için `+=` üç yerde, `-=` iki yerde geçiyor.
`BoardAdapter` simetrik — `OnEnable`de `+=` (`BoardAdapter.cs:290`), `OnDisable`da
`-=` (`BoardAdapter.cs:295`). `Battle` yönlendiricisini sözlükten alıp söküyor
(`Battle.cs:349`). Eksik bir `-=` tek bir uyarı bile üretmez; bunu `BoardAdapter.cs:304`
kendi yorumunda zaten söylüyor.

> **◀ DÖNÜŞ:** [../../ogrenme/02-sonraki-asamalar.md](../../ogrenme/02-sonraki-asamalar.md#asama-3-olay-veri-yolu-event-bus)
> — «Aşama 3 · Olay veri yolu»na. Oradaki ***KAPATTIĞI ÖLÇÜLMÜŞ ARIZA***
> tablosunun A ve B satırlarının mekanizması budur.


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
  ② YEREL değişkeni null'a karşı sına        ② ALANI >> İKİNCİ KEZ << oku
  ③ YEREL değişken üstünden Invoke et           ve Invoke et
     >> alanı BİR KEZ okur <<                      ▲
                                     >> İKİ OKUMA ARASI: başka bir yol -=
                                        yapıp alanı null'a düşürebilir <<
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
elle yazılan kontrol bugün birebir aynı sonucu verir. ***Önem kazanacağı gün***:
`Tick` bir Unity Job'ından ya da bir `Task`tan çağrıldığı gün, ya da bir abone
kendi `-=`'ini bir zamanlayıcı geri çağrısından yazdığı gün — o gün
`if (X != null) X.Invoke()` bir yarış penceresi taşır, `?.Invoke` taşımaz.

Sahip etiketi: `?.` = **C# dili**. Vaadi dar ve tam olarak şu: **sol taraf bir
kez değerlendirilir.** `Invoke`ın iş parçacığı güvenli olduğunu ***VAAT ETMEZ***.

---

## Beşinci durak: ***OPERATÖRÜN SORDUĞU PASAJ*** — abonelik neden EN SONDA

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
        │     Target = >> bu Combatant <<  (kurulumu HENÜZ bitmedi)
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
     >> ama bu bir tasarım güvencesi değil, o günkü çağrı yerinin şansı <<

B) çağıran lifecycle'ı ELİNDE TUTUYOR (parametre olarak o verdi — normal yol)
   → yarım Combatant toplanamaz: Target onu tutuyor (birinci durak)
   → çağıran aynı lifecycle'ı İKİNCİ bir Combatant'a verirse
        çağrı listesi = [ yarım Combatant , sağlam Combatant ]
   → bir sonraki geçişte İKİSİ de çalışır; yarımın alanları atanmamıştır
        (AttackProfile = null, Team = Team.None) ve üçüncü duraktaki sıra
        kuralı gereği ÖNCE o çalışır. Bir NullReferenceException fırlatırsa
        >> sağlam Combatant o yayını HİÇ duymaz <<
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
   ►  log.Count == 0  >> reddedilen ekleme TEK BİR abone bırakmadı <<
```

`Combatant` kurucusunda bu testin karşılığı **yok**: kurucu fırlattığında elinde <!-- YOK-MUAF · KAPSAM DIŞI · gerekçe aşağıda -->
sınayacak bir nesne kalmıyor, testin şekli de farklı olmak zorunda (birinci
duraktaki `DescribeSubscribers` gibi bir ölçü üyesi gerekirdi). Kuralı bugün
tutan şey testler değil, satır sırası.

> ***BEŞ ALAN BAĞLAMIYOR — KAPSAM DIŞI***
>
> **NEDEN SENET YAZILMADI:** Yukarıdaki cümle bir mekanizmanın değil bir TESTİN
> yokluğunu hükmediyor. Senedin birinci alanı oyunda görünen bir özellik ister.
> Bir test ise oyuncunun ekranında hiçbir şeyi değiştirmez, dolayısıyla ① bu
> şekle oturmuyor. Bu borcun doğru sahibi başka bir senedin ③ alanıdır: kural
> "hangi test kızarır" sorusunu orada sorar ve "bugün hiçbiri" cevabını orada
> meşru sayar. Bu dosyanın `GetInvocationList()` senedi ③ alanında tam olarak
> öyle bir borç taşıyor.
>
> **VURGU NEYİ İŞARETLİYOR:** Buradaki kalın `yok` bir hükmün altını çizmiyor,
> cümlenin cevap sözcüğünü kalınlaştırıyor. Kapı bu lehçeyi zaten kapsam dışında
> tutuyor. Anma yine de kapsama girdi, çünkü taramada `karşılık` lehçesi o
> konumu önce tuttu ve cevap sözcüğünü içine aldı.
>
> **BORÇ SİLİNMEDİ:** Testin şekli belgede yazılı duruyor. Kurucu fırlattığında
> elde sınayacak bir nesne kalmıyor ve bir ölçü üyesi gerekiyor. Kuralı bugün
> tutan şeyin satır sırası olduğu da bir üstteki cümlede yazılı.

### İŞ BÖLÜMÜ: ⑤ ile ⑥ örtüşmez, bölüşür

```
⑤ lastObservedState = this.lifecycle.State;
      kapatır  : "ilk yayında ÖNCEKİ durum ne olacak"
      silinirse: alan default(UnitState) = Alive kalır ve bugün DOĞRU cevabı
                 verir >> ama bu bir tesadüf << — UnitState listesinde Alive
                 başta duruyor. Sıra değişirse yanlış "önceki durum" yayılır.

⑥ this.lifecycle.StateChanged += OnLifecycleStateChanged;
      kapatır  : "geçişleri kim duyacak"
      silinirse: hiçbir geçiş duyulmaz, Combatant.StateChanged bir daha hiç
                 tetiklenmez, ekran donuk kalır
```

⑤'in ⑥'dan **önce** olması da ayrı bir karar: ⑥'dan sonraki ilk yayın
`lastObservedState`i okumak zorunda. Ters sırada bugün gözle görülür fark yok,
çünkü kurucu ile ilk yayın arasında hiçbir şey çalışmıyor; ***önem kazanacağı
gün***, `+=` ile kurucunun sonu arasına yayın üretebilen bir satır girdiği gün.

### "Aboneliğin çözüldüğü yer yok" — bunun KOŞULU

Yorumun son cümlesi bir ihmal itirafı değil, bir sahiplik cümlesi: `Combatant`
`lifecycle`'ın **sahibi**, ikisi aynı anda doğar aynı anda çöp olur, abonelik
bir sahiplik sınırı geçmiyor — sökülecek bir şey yok. Ölçü: `Combatant.cs`'te
`lifecycle` `private readonly` bir alan ve dışarıya **hiçbir üye** onu vermiyor
(`State`, `RemainingSeconds`, `IsReadyForCleanup` üçü de `lifecycle`'ın
**değerini** geçiriyor, kendisini değil); erişimi olan tek yol, onu kurucuya
veren çağırandır.

***Borç doğuracağı koşul*** — sahiplik değiştiği gün: `lifecycle` bir havuzdan
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

> **▶ ARA DURAK:** [../konular/08-motor-cagri-dongusu.md](../konular/08-motor-cagri-dongusu.md#ikinci-durak-cagri-sirasi-sahipleriyle-ezberle-degil)
> **NEDEN:** yukarıdaki farkın **sağ sütunu** burada tanımlanmıyor. Unity yaşam
> döngüsünün kendisi (`Awake` → `OnEnable` → `Start` → `Update` → `OnDisable` →
> `OnDestroy` sırası, ve `OnEnable`/`OnDisable` çiftinin neden `Awake`/`OnDestroy`
> çiftine tercih edildiği) o dosyanın işi. **Burada yalnız *fark* var, sıra
> değil.**
> **DÖNÜŞ:** bu dosyanın [«Altıncı durak: `event` ile Unity mesaj geri çağrıları aynı şey değil»](#altinci-durak-event-ile-unity-mesaj-geri-cagrilari-ayni-sey-degil) bölümü

---

## Üç oyun: "bir şeyin olduğunu başkasına nasıl duyurursun"

| Oyun | Aynı basıncı taşıyan şeyin ADI ve İŞİ |
|---|---|
| **Slay the Spire** | Kalıntılar ve güçler. Bir kart oynandığında, hasar alındığında, tur bittiğinde ellerindeki iş tetiklenir. Aynı ana kayıtlı birden çok kalıntı vardır ve **hangisinin önce işlediği sonucu değiştirir** — oyuncu bunu envanter sırasında görür. |
| **Vampire Survivors** | Binlerce düşmanın her biri. Ölen düşman deneyim taşı bırakır, sayacı ilerletir, bazen bir silahı tetikler — aynı an, ekranda aynı karede yüzlerce kez. |
| **Stardew Valley** | Gün sonu. Tek bir anda ekinler büyür, hayvanlar ürün verir, makineler işini bitirir, takvim ilerler; birbirini tanımayan çok sayıda farklı iş aynı ana bağlıdır. |

### ***EŞLEŞMEYEN SATIR: Vampire Survivors***

En öğretici satır bu, çünkü bizim mekanizmamız oraya **oturmuyor**:

```
BİZDE (Battle.AddUnit)                  ORADA (binlerce varlık)
──────────────────────                  ───────────────────────
birim başına 1 kapanış nesnesi          varlık başına 1 kapanış + 1 liste
          + 1 sözlük girdisi            girişi → × binlerce nesne, ve her
          + 1 çağrı listesi girişi      ölümde bir dolaylı delege çağrısı
ölçü: sahnede 2 birim var (Vanguard,    >> o ölçekte olay başına TAHSİS ve
Raider — BoardAdapter.Awake) → 2 kapanış   İŞ YÜKÜ demektir <<
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
        Target = this  >> YARIM KURULMUŞSA BURADA YAKALANIR <<
        Method = OnLifecycleStateChanged
        │  add_StateChanged(...) ← event'in ürettiği metot
        ▼
   Delegate.Combine(eski, yeni) ← .NET: >> YENİ NESNE <<, eski kımıldamadı
        │  Interlocked.CompareExchange ile gizli alana yazıldı
        ▼
   gizli alan: [ h1 , h2 , h3 ]  ← sıra = abonelik sırası
        │  yayın anı: StateChanged?.Invoke(next)
        ▼
   ① alan YEREL değişkene KOPYALANIR  ② null mı bakılır  ③ liste SIRAYLA yürür
        └─► h1 fırlarsa >> h2 ve h3 HİÇ çağrılmaz <<, ve yayıncının
            Invoke'tan SONRAKİ satırları da çalışmaz
```

---

## Kural: bir `+=` satırını nereye yazarsın

```
① Bu satır bir KURUCUNUN içinde mi?      hayır → ③   ·   evet → ②

② Kurucunun geri kalanı FIRLATABİLİR mi (null/aralık kontrolü, sözlüğe ekleme)?
      evet  → >> += EN SONA << — fırlayan kurucu geriye nesne döndürmez,
              dolayısıyla -= yazacak bir yer de bırakmaz. Başka koruma YOK.
      hayır → yine en sona yaz; bedeli sıfır ve bir sonraki doğrulama
              eklendiğinde kural kendiliğinden tutar

③ Kurucunun/metodun KENDİSİ bir yayın üretiyor mu (abone olduğun nesne
   üstünde durum değiştiren bir çağrı)?
      evet  → >> += o çağrıdan ÖNCE <<, yoksa ilk geçiş kaçırılır; sıra
              "doğrula → abone ol → tetikle" olur
      hayır → ②'nin cevabı geçerli

④ Yayıncı, aboneden UZUN yaşayacak mı? (ölçü: yayıncıya erişimi olan başka
   kimse var mı)
      hayır → -= borcu YOK; nesne ölünce olay da ölür
      evet  → >> -= BORCU DOĞDU <<, derleyici hatırlatmaz; sökme yerini
              AYNI turda yaz (OnEnable ↔ OnDisable gibi)

⑤ Olayın ikinci bir abonesi olabilir mi?
      evet → SIRA bir davranış hâline gelir ve bir abonenin fırlatması
             ötekileri sessizce düşürür. Fırlatabilecek abone kendi gövdesini
             try/catch'e alır — >> Invoke bunu senin için YAPMAZ <<
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
       >> liste de dinleyiciyi hayatta tutar <<.

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

---

## ***SIRADAKİ ADIM***

> **▶ SIRADA:** [`07-bellek-canlilik-ve-yikim.md`](07-bellek-canlilik-ve-yikim.md) — okuma yolunun **12.** adımı
> **NEDEN ORASI:** bu dosya oku çizen alanın **adını** verdi (`Target`); `dil/07`
> o okun **bellek faturasını** ölçüyor — yedi hop, ve tek bir `Combatant`
> referansı bütün savaşı erişilebilir tutuyor. ***Aynı eksik `-=`, iki ayrı
> fatura: `konular/01` davranış faturasını, `dil/07` bellek faturasını veriyor.***
> **UYARI:** `dil/07` kendi başında `dil/05`'i ön koşul sayıyor (`05` semantiği,
> `07` depolamayı anlatır). `dil/05`'i henüz okumadıysan `07`'nin dört soruluk
> figürü yine de ayakta; ***depolama bölümünde sıkışırsan `dil/05`'e geç, sonra
> dön***.
> **YOL HARİTASI:** [`../../ogrenme/00-okuma-sirasi.md`](../../ogrenme/00-okuma-sirasi.md)
