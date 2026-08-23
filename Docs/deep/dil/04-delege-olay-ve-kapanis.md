# Delege, olay ve kapanış — bir fonksiyonu değişkende tutmak

> **Nerede geçiyor:** `UnitLifecycle.StateChanged`, `Combatant.StateChanged`,
> `Battle.stateForwarders`, `Battle.UnitStateChanged`,
> `BoardAdapter.OnEnable`/`OnDisable`
> **Kodda nereden geldin:** `Action<T>`, `event`, `?.Invoke`, lambda
> `(p, n) => …`, metot grubu `+= OnFoo`
> **Ne zaman oku:** bir `event` bildirmeden önce, ya da bir `-=` yazıp
> "aboneliği kaldırdım" dediğinde.

Bu dosya projenin kendi kararlarını değil, projenin **ödünç aldığı** dil
özelliklerini anlatıyor. `Action<T>` .NET'ten, `event` ve lambda C#'ın
kendisinden geliyor; hiçbirinin kodunu biz yazmadık — ama neyi vaat ettiklerini
bilmeden kendi kararlarımızı okuyamayız.

Olayın **proje** tarafı — hangi tip zincire ne ekliyor, `stateForwarders`
sözlüğü neden `Battle`'da duruyor, sökülmezse önce ne patlıyor —
[`../konular/01-olay-zinciri.md`](../konular/01-olay-zinciri.md) dosyasında.
Burada zincir yok; burada zincirin yapıldığı **malzeme** var.

---

## Sahne

`Battle.AddUnit`'in son üç satırı:

```csharp
Action<UnitState, UnitState> forwarder =
    (previous, next) => UnitStateChanged?.Invoke(unit, previous, next);
combatant.StateChanged += forwarder;
```

Üç satır, beş ayrı kavram: bir delege tipi, bir lambda, bir yakalama, bir
null-koşullu çağrı, bir abonelik işleci. Hiçbiri projeye ait değil, hepsi C#'ın.

Ve bu lambda **projedeki tek lambda**. Ölçü: `Assets/Game/` altında `) =>` ara —
tek eşleşme `Battle.cs:227`. Geri kalan bütün abonelikler metot adıyla yazılmış.
Bu tek istisnanın neden istisna olduğu, bu dosyanın omurgası.

---

## Karakterler

```
╔═ System.Action<…> (delege TİPİ) ══════════════════════════════╗
║  Ne yapar : bir METODU değer gibi taşır — değişkende tutulur,  ║
║             parametre olarak geçer, sözlükte saklanır          ║
║  Vaadi    : imzası uyan her metot buraya sığar                 ║
║  BİLMEZ   : metodun ne yaptığını, kimin yazdığını              ║
║  DÖNÜŞ    : ██ YERİ YOK ██ — Action her zaman void'dir         ║
╚═══════════════════════════════════════════════════════════════╝

╔═ event (anahtar kelime — tip DEĞİL) ══════════════════════════╗
║  Ne yapar : bir delege alanının DIŞARIYA açık yüzünü kısar     ║
║  Vaadi    : dışarıdan yalnız `+=` ve `-=`; ne atama ne çağrı   ║
║  BİLMEZ   : kaç abonesi olduğunu — sıfır abone = null          ║
╚═══════════════════════════════════════════════════════════════╝

╔═ lambda / kapanış (closure) ══════════════════════════════════╗
║  Ne yapar : yazıldığı yerde isimsiz bir metot doğurur          ║
║  Vaadi    : çevredeki değişkeni İÇİNE alıp taşır               ║
║  BİLMEZ   : aynı metinle yazılmış ikizini ██ TANIMAZ ██        ║
╚═══════════════════════════════════════════════════════════════╝

╔═ metot grubu (parantezsiz ad: `OnFoo`) ═══════════════════════╗
║  Ne yapar : var olan bir metottan delege üretir                ║
║  Vaadi    : aynı metot + aynı hedef = EŞİT delege, her defa    ║
║  BİLMEZ   : hiçbir şey yakalamaz — yakalayacak kapsamı yok     ║
╚═══════════════════════════════════════════════════════════════╝
```

---

## Birinci durak: delege — metoda işaret eden nesne

Testlerdeki en yalın hâli:

```csharp
lifecycle.StateChanged += seen.Add;
//                             ▲
//                    ██ PARANTEZ YOK ██
```

`seen.Add` burada bir çağrı **değil**. Parantez yazılsaydı `Add` hemen çalışır,
`void` dönerdi ve `+=`nin sağına koyulacak bir şey kalmazdı — satır derlenmezdi.
Parantezsiz ad metodun **kendisini** ifade eder; derleyici oradan çağrılabilir
bir nesne üretir.

**Ölçü:** `UnitLifecycleTests.StateChanged_DoesNotFireWhileAliveTicksPass` —
abone eklendikten sonra iki `Tick` geçiyor ve `seen` hâlâ boş. Yani `+=` satırı
`Add`'i çağırmadı, **sakladı**. Aynı testte `OnHealthDepleted()` çağrılsaydı
liste dolardı.

### `Action<…>` nasıl okunur

```
Action  <  UnitState  ,  UnitState  >
  ▲          ▲              ▲
  │          │              └── ikinci parametrenin tipi
  │          └── birinci parametrenin tipi
  └── "hiçbir şey döndürmez" — bu kelimenin kendisi dönüş tipidir

karşılığı olan gerçek metot:

void OnLifecycleStateChanged( UnitState previous , UnitState next )
 ▲                                ▲                    ▲
 └── Action adının içinde saklı   └────────┬───────────┘
                                           └── köşeli parantezin içindekiler
```

`Action`'ın 0 parametreden 16 parametreye kadar hazır sürümleri var. Projede
üçü kullanılıyor ve üçü de aynı zincirin farklı halkaları:

```
UnitLifecycle.StateChanged   Action<UnitState>                     1 parametre
Combatant.StateChanged       Action<UnitState, UnitState>          2 parametre
Battle.UnitStateChanged      Action<Unit, UnitState, UnitState>    3 parametre
```

**Ölçü:** `UnitState x = lifecycle.StateChanged.Invoke(UnitState.Dead);` yaz.
Derlenmez — *"cannot implicitly convert type 'void'"*. `Action`'da dönüş için
ayrılmış bir yer yoktur, o yüzden bir değere atanamaz.

### `Func<T, R>` neden hiç yok

`Func<UnitState, bool>` "bir `UnitState` alır, `bool` döndürür" demek: **son**
tip argümanı dönüş tipidir. Projede tek bir `Func` geçmiyor ve bu tesadüf değil.

```
Action → YAYIN.  N abone, N kez çalışır, cevap toplanmaz
Func   → SORU.   N abone, N kez çalışır,
                 ██ ama geriye YALNIZ SONUNCUNUN cevabı döner ██
```

`Invoke` davet listesini abone olunma sırasıyla dolaşır ve elinde tek bir dönüş
değeri kalır: son çağrının değeri. Bir olayın imzası `Func` olsaydı ilk iki
abonenin döndürdüğü şey sessizce çöpe giderdi.

**Ölçü:** `Func<int>` tipinde bir alana iki abone ekle — biri `1`, öteki `2`
döndürsün. `Invoke()` her zaman `2` verir; `1`'i görmenin hiçbir yolu yoktur.
İkisinin de cevabı lazımsa şekil delege değil, koleksiyon döndüren bir metottur.

Yayın için doğru şekil `Action`. Projede "cevap" isteyen yerler (`AttackAction`,
`MoveAction`) olay değil **dönüş değeri** kullanıyor. Bu ayrımın kendisi kodda
iki karşıt blok olarak duruyor: `UnitLifecycle.StateChanged`'in üstünde *"soran
yokken ilgilenen varsa, şekil event'tir"*, `StructureLifecycle`'da ise aynı
gerekçenin sınanıp **geçersiz** çıktığı hâli — *"event, geçişin soranı yokken
doğar; burada her yıkımın soranı var"*.

---

## İkinci durak: `event` ile düz `Action` alanı farkı

`event` bir tip değil, bir **kısıtlayıcı**. Sil, kod yine derlenir:

```csharp
public event Action<Unit, UnitState, UnitState> UnitStateChanged;   // bugün
public       Action<Unit, UnitState, UnitState> UnitStateChanged;   // düz alan
//     ▲
//   tek fark bu kelime
```

Fark tamamen **dışarıdan ne yazılabildiği**. Aşağıdaki satırların hepsi
`BoardAdapter` içinden, yani `Battle`'ın dışından:

| Dışarıdan yazılan satır | `event` | düz alan |
|---|---|---|
| `battle.UnitStateChanged += OnUnitStateChanged;` | ✓ | ✓ |
| `battle.UnitStateChanged -= OnUnitStateChanged;` | ✓ | ✓ |
| `battle.UnitStateChanged = null;` | ✗ CS0070 | ✓ sessizce hepsini siler |
| `battle.UnitStateChanged = OnUnitStateChanged;` | ✗ CS0070 | ✓ ██ öncekileri EZER ██ |
| `battle.UnitStateChanged?.Invoke(u, a, b);` | ✗ CS0070 | ✓ sahte olay üretir |

**Ölçü:** `BoardAdapter.OnDisable` içindeki `-=` satırını `= null` yap. Derleyici
CS0070 verir: *"The event … can only appear on the left hand side of += or -=
(except when used from within the type …)"*. Sonra aynı satırı `Battle.cs`'in
içine taşı — orada derlenir.

Kısıt **bildiren tipin dışına** uygulanıyor; içeride `event` sıradan bir alandır.
`Battle.AddUnit`'teki lambdanın `UnitStateChanged?.Invoke(...)` yazabilmesinin
sebebi bu: lambda `Battle`'ın gövdesinde duruyor. Aynı kapanışı `Battle` dışında
bir yardımcı sınıfa taşısaydın derlenmezdi — ve olayın **kimin adına**
yayınlandığı sorusu orada patlardı.

### Düz alan olsaydı ne kırılırdı

```
   BoardAdapter                     ses/skor bileşeni
        │                                  │
        └──►  battle.UnitStateChanged  ◄───┘        iki abone, ikisi de sağlam
                        ▲
         üçüncü bir dosya `= OnFoo;` yazar
                        │
                        ▼
              davet listesi TEK elemana iner
    ██ iki abone de sessizce düşer — derleme hatası YOK, uyarı YOK ██
```

`event` bu satırı derleme zamanında imkânsız kılıyor ve karşılığında hiçbir şey
almıyor: `+=`/`-=` zaten dışarının ihtiyacı olan her şey.

Projedeki üç olayın üçü de `event`: `UnitLifecycle.StateChanged`,
`Combatant.StateChanged`, `Battle.UnitStateChanged`. Düz `Action` alanı olarak
bildirilmiş **tek bir olay yok**.

### Ama her `Action` alanı olay değil

```csharp
private readonly Dictionary<Unit, Action<UnitState, UnitState>> stateForwarders;
//                                ▲
//              burada Action bir OLAY değil, sözlüğün DEĞER TİPİ
```

`event` yalnızca "buraya abone olunur" diyen üyelere konur. `stateForwarders`
kimseyi yayına çağırmıyor; sökülecek nesneleri saklıyor. Delegenin birinci
sınıf bir tip olması tam olarak bunu mümkün kılıyor: `int` nasıl sözlükte
duruyorsa fonksiyon da öyle duruyor.

---

## Üçüncü durak: `?.Invoke` — abonesi olmayan olay `null`'dur

```csharp
StateChanged?.Invoke(next);
//          ▲
//   "StateChanged null DEĞİLSE Invoke et"
```

Boş bir davet listesi diye bir şey yok. Hiç abone yoksa alanın değeri `null`:

```
başlangıç        StateChanged = null
+= handler1      StateChanged = [handler1]
+= handler2      StateChanged = [handler1, handler2]
-= handler2      StateChanged = [handler1]
-= handler1      StateChanged = ██ null ██   ◄── boş listeye DÜŞMEZ, null'a döner
```

**Ölçü (projede koşuyor):**
`UnitLifecycleTests.StateChanged_WithNoSubscribers_DoesNotThrow` — hiç abone
eklemeden `new UnitLifecycle().OnHealthDepleted()` çağrılıyor ve atmıyor. `?`
silinirse aynı satır `NullReferenceException` atar. Üretimde abone garantisi
yok: UI kapalıyken de birim düşer.

### Delege nesnesi DEĞİŞMEZ — `?.` bunun üstüne kurulu

`+=` bir listeye ekleme yapmaz. Eskisiyle yenisini birleştiren **yeni bir nesne**
üretir ve alana onu yazar:

```
   alandaki nesne: [h1]          eklenen: h2
              │                       │
              └──────────┬────────────┘
                         ▼
              YENİ NESNE [h1, h2]  ──►  alana atanır
   ██ eski [h1] nesnesi olduğu gibi durur; kimse ona dokunmadı ██
```

İki sonucu var:

- `?.` alanı **bir kez** okur, sonucu geçici bir yere alır, onu çağırır.
  `if (StateChanged != null) StateChanged.Invoke(next);` iki ayrı okuma yapar —
  bu projede tek iş parçacığı olduğu için ikisi de doğru çalışır, ama `?.` daha
  kısa ve kontrolü çağrıya bağlar.
- **Yayın sürerken yapılan `-=` o yayını etkilemez.** `Invoke`, çağrı anında
  eline aldığı listeyi sonuna kadar dolaşır.

**Ölçü:** `lifecycle.StateChanged`'e iki abone ekle; birincisi kendi gövdesinde
ikinciyi `-=` etsin. `OnHealthDepleted()` çağır — ikinci abone o tur **yine
çalışır**. Sökülme ancak bir sonraki yayında görünür.

---

## Dördüncü durak: kapanış kimliği — `-=` neye bakıyor

```csharp
Action<UnitState, UnitState> forwarder =
    (previous, next) => UnitStateChanged?.Invoke(unit, previous, next);
//                                        ▲
//                              `unit` YAKALANDI
```

**Ölçü — gerçekten yakalıyor mu:** lambdanın önüne `static` yaz —
`static (previous, next) => …`. Derlenmez. Statik anonim fonksiyon ne çevredeki
bir yerel değişkene (`unit`) ne de `this`e (`UnitStateChanged` bir örnek üyesi)
dokunabilir. Derleyicinin reddi, yakalamanın kanıtıdır.

Yakalama bedava değil. Derleyici görünmez bir sınıf üretir, `unit`i onun alanına
yazar, lambdayı o sınıfın metodu yapar. Yani **her `AddUnit` çağrısı ayrı bir
nesne doğurur** — metin tek, nesne N tane.

### İki delege ne zaman eşittir

`-=` arka planda `Delegate.Remove` çağırır ve eşitliği **iki şeye** bakarak
ölçer:

```
╔═ delege eşitliği ═════════════════════════════════════════════╗
║  ① HEDEF nesne aynı mı?                                        ║
║       metot grubunda : `this` (abone olan nesne)               ║
║       kapanışta      : derleyicinin ürettiği görünmez örnek    ║
║                                                                ║
║  ② METOT aynı mı?                                              ║
║       metot grubunda : yazdığın metot                          ║
║       kapanışta      : derleyicinin ürettiği metot — her       ║
║                        lambda METNİ ayrı bir metottur          ║
╚═══════════════════════════════════════════════════════════════╝
```

Buradan iki kural düşüyor:

```
metot grubu    += / -= OnFoo       ① aynı `this`  ② aynı OnFoo   ──► EŞİT ✓
yeni lambda    -= (p, n) => …      ① BAŞKA örnek  ② BAŞKA metot  ──► EŞİT DEĞİL ✗
```

Ve `Remove` eşleşme bulamazsa: listeyi **olduğu gibi** geri verir.

```csharp
combatant.StateChanged -= (p, n) => UnitStateChanged?.Invoke(unit, p, n);
//                        ╰─ metin aynı, NESNE başka
//  ██ liste kımıldamaz. İstisna YOK, uyarı YOK, dönüş değeri YOK. ██
```

Sessizliğin sebebi: `-=` bir ifade değil bir atama; "kaç eleman çıkardım" diye
soracağın bir yer hiç yok.

**Ölçü (projede koşuyor):**
`BattleTests.UnitStateChanged_StopsReachingListenersAfterTheUnitLeavesTheBattle`
— `RemoveUnit`'ten sonra `combatant.Tick(10.1f)` gerçek bir `Downed → Dead`
geçişi üretiyor ve dinleyicinin sayacı 1'de kalıyor. `RemoveUnit` sözlükteki
nesne yerine aynı metni yeniden yazarak `-=` yapsaydı sayaç 2 olurdu — ve
derleyici tek bir kelime söylemezdi.

`Battle.stateForwarders` sözlüğü tam olarak bu sessizliği kapatmak için var:
sökülecek **nesneyi** elde tutuyor. Sözlüğün neden `Battle` katmanında durduğu
ve sökülmezse önce neyin patladığı
[`../konular/01-olay-zinciri.md`](../konular/01-olay-zinciri.md)'nde.

---

## Beşinci durak: metot grubu mu, lambda mı

Projedeki üç aboneliğin **şekli**:

```
Combatant kurucusu
   lifecycle.StateChanged += OnLifecycleStateChanged;
   └─ METOT GRUBU  ·  yakalama yok  ·  saklanmıyor  ·  hiç sökülmüyor

Battle.AddUnit
   combatant.StateChanged += forwarder;
   └─ KAPANIŞ  ·  `unit` yakalanıyor  ·  ██ sözlükte saklanıyor ██
      ·  RemoveUnit söküyor

BoardAdapter.OnEnable
   battle.UnitStateChanged += OnUnitStateChanged;
   └─ METOT GRUBU  ·  yakalama yok  ·  saklanmıyor  ·  OnDisable söküyor
```

Üçüncüsü öğretici: **sökülüyor ama saklanmıyor.** Saklanmasına gerek yok, çünkü
`-= OnUnitStateChanged` her yazıldığında eşit bir delege üretir.

**Ölçü:** `OnEnable`'daki `+=` ile `OnDisable`'daki `-=` iki **ayrı** delege
nesnesi yaratır — aynı nesne değildir. Buna rağmen sökme çalışır, çünkü eşitlik
nesne adresine değil (hedef, metot) çiftine bakar. Aynı deneyi lambdayla yap,
sökme başarısız olur.

`Combatant`'ın kurucudaki tercihi de bilinçli: aynı iş orada bir lambda ile
yazılsaydı o nesne hiçbir yerde tutulmaz ve aboneliği çözmenin **tek yolu**
kapanırdı. Gerekçe `Combatant.cs`'te, `OnLifecycleStateChanged`'in üstünde.

---

## Kural: hangi şekli seçersin

```
① Dışarısı bir CEVAP mı bekliyor, HABER mi?
      cevap → dönüş değeri kullan; delege bile gerekmez
              (AttackAction/MoveAction böyle: soran zaten orada)
      haber → ②

② Bu üyeyi dışarıdan biri EZEBİLMELİ mi?
      evet  → düz Action alanı (bu projede hiç yok, ve olmamalı)
      hayır → ██ event ██ — bedeli sıfır, += / -= zaten yeterli

③ Tetiklerken: her zaman `?.Invoke`
      abonesiz olay boş liste değil NULL'dur; `?` olmadan NRE

④ Abone olurken bir şey YAKALAMAN gerekiyor mu?
      hayır → metot grubu (`+= OnFoo`). Sökmek için `-= OnFoo` yeter,
              saklamaya gerek yok.
      evet  → lambda üret ── ve ⑤

⑤ İleride sökecek misin?
      hayır → saklama; nesne ölünce olay da ölür
      evet  → ██ ÜRETTİĞİN NESNEYİ SAKLA ██
              ikinci kez yazılan aynı metin onu SÖKMEZ
```

④'e "evet" demenin bu projedeki tek gerçek sebebi — N kaynağa abone olmak ve
olayın kimlik taşımaması — zincirin kendi kararı;
[`../konular/01-olay-zinciri.md`](../konular/01-olay-zinciri.md)'nde ayrıntısıyla
duruyor. Buradaki ⑤ sadece o kararın **dil tarafındaki faturası**.

---

## Yanlış hatırlanan üç şey

**"`event` bir tip."** Değil. Tip `Action<…>`; `event` o alanın dışarıya açık
yüzünü `+=`/`-=` ile sınırlayan bir değiştirici. Kelimeyi sil, kod derlenmeye
devam eder — yalnız dışarıya iki yeni hak doğar: ezmek ve sahte olay yayınlamak.

**"`?.Invoke`'daki soru işareti savunma amaçlı, gerekmez."** Gerekir. Abonesi
olmayan olay boş liste değil, `null`'dur. `UnitLifecycle` abonesiz de çalışmak
zorunda: bir birim UI kapalıyken de düşer.

**"`-=` çalışmadıysa hata verir."** Vermez. `Delegate.Remove` eşleşme bulamazsa
listeyi aynen geri döndürür. Yanlış nesneyle sökme denemesinin tek belirtisi,
haftalar sonra gelen bir hayalet yayındır — ve o yayın patladığında imleç
`-=` satırında değil, dinleyicinin içinde olur.

---

## Kaçış yolu: `event Action<…>` yerine ne olurdu

```
public delegate void StateChangedHandler(UnitState previous, UnitState next);
        → adlandırılmış delege tipi. Kazancı: parametrelerin ADI olur, imza
          belgelenir. Bedeli: her imza için ayrı bir tip bildirimi ve okurun
          "bu ne alıyordu" diye başka dosyaya gitmesi.

EventHandler<StateChangedArgs>
        → .NET'in (sender, args) geleneği. Gönderen imzada gelir, yani kapanış
          gereksizleşirdi ██ ama bu proje gönderen olarak Combatant'ı değil
          Unit'i istiyor ██ ve her yayında bir args nesnesi doğardı.

düz Action<…> alanı
        → aynı yayın; dışarıya `= null` ve sahte `Invoke` açık. Kazandırdığı
          hiçbir şey yok.

dinleyici arayüzü (IStateListener listesi)
        → kimlik sorunu biter: nesneyi zaten sen tutuyorsun, `Remove` referansla
          çalışır. Bedeli: her dinleyici için bir tip, ve BoardAdapter'ın tek
          metodu bir sınıfa dönüşür.
```

`event Action<…>` dördünün ortasını tutuyor: dışarıya yalnız abonelik açık, ek
tip yok, yayın başına tahsis yok. Ödenen tek fatura kapanış kimliği — ve o
fatura tek bir yerde, `Battle.stateForwarders`'da toplanmış.

`EventHandler` geleneğinin neden seçilmediği — göndereni imzaya koymanın bu
mimaride faturayı **azaltıp sıfırlamadığı** —
[`../konular/01-olay-zinciri.md`](../konular/01-olay-zinciri.md)'nin son
bölümünde ölçülerek tartışılıyor.

---

Kodda **karar**, burada **ödünç alınan dil özelliğinin sözleşmesi**. İkisi
çelişirse kod kazanır — orası çalışan metin, burası anlatı.
