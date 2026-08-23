# Hata bildirme ve doğrulama — kime söylüyorsun, hangi kelimeyle

> **Nerede geçiyor:** `DamageRules.ResolveRemaining`, `HealingRules.ResolveRestored`,
> `UnitGrid.ThrowIfOutsideGrid`, `PointerGesture` kurucusu,
> `BattleActions.RequireCombatant`, `Battle.ThrowIfCannotJoin`, `GridDistance.Between`
> **Kodda nereden geldin:** `nameof`, `ArgumentNullException`,
> `ArgumentOutOfRangeException`, `ArgumentException`, `InvalidOperationException`,
> `Math.Max` / `Math.Min`
> **Ne zaman oku:** bir `throw` satırı yazarken; `nameof(x)` görüp "bu neden düz
> dize değil" diye düşündüğünde; ya da üç `Argument*` tipinden hangisini
> seçeceğine karar veremediğinde.

Bu dosya projenin kendi kararlarını değil, projenin **ödünç aldığı** dil ve BCL
araçlarını anlatıyor. Onların kodunu biz yazmadık; ama neyi vaat ettiklerini
bilmeden kendi `throw` satırlarımızı okuyamayız.

**Bu dosya "istisna mı sonuç değeri mi" sorusunu CEVAPLAMAZ.** O ayrımın —
istisna PROGRAMCIya, sonuç değeri OYUNCUya — sahibi
[`konular/04-karar-sirasi.md`](../konular/04-karar-sirasi.md), *İkinci durak: iki
kanal, iki okuyucu*. Burada bir adım sonrası var: **istisna kanalına düştüğüne
karar verdikten sonra hangi tipi, hangi argümanlarla yazacaksın.**

---

## Sahne

`DamageRules`'un içinde altı satır:

```csharp
if (amount < 0)
{
    throw new ArgumentOutOfRangeException(nameof(amount), amount, "Damage amount cannot be negative.");
}

return Math.Max(0, current - amount);
```

Bu altı satırda projeye ait tek bir tip yok: bir operatör (`nameof`), bir istisna
sınıfı, bir kelepçe fonksiyonu. Üçü de .NET'ten geliyor — ve üçü de projede en
çok tekrarlanan şeyler:

```
Assets/Game içinde sayıldı:

   nameof                                   80 kez
   ────────────────────────────────────────────────
   throw new ArgumentNullException          34 kez  ┐
   throw new ArgumentOutOfRangeException    22 kez  ├── 66 throw satırı
   throw new ArgumentException              10 kez  ┘
   ────────────────────────────────────────────────
   throw new InvalidOperationException       0 kez  ██ HİÇ ██
```

Son satır bir eksiklik değil, bir karar — dördüncü durakta.

---

## Karakterler

```
╔═ nameof(x) — bir OPERATÖR ════════════════════════════════════╗
║  Ne yapar : bir adı derleme zamanında dize sabitine çevirir   ║
║  Vaadi    : ad değişirse DERLEME DURUR                        ║
║  BİLMEZ   : değeri. Yalnız ADI görür, `x`'in içindekini asla  ║
║             ██ ve nitelenmiş adı vermez: son parçayı verir ██ ║
╚═══════════════════════════════════════════════════════════════╝

╔═ ArgumentNullException ═══════════════════════════════════════╗
║  Ne yapar : "bu argüman null geldi" der                       ║
║  Vaadi    : tek argüman alır — parametrenin ADI               ║
║  BİLMEZ   : null'ın neden geldiğini. Kimi suçlayacağını       ║
║             yalnız sen söylersin                              ║
╚═══════════════════════════════════════════════════════════════╝

╔═ ArgumentOutOfRangeException ═════════════════════════════════╗
║  Ne yapar : "bu argüman geçerli aralığın dışında" der         ║
║  Vaadi    : SAYIYI DA TAŞIR — ActualValue alanında            ║
║  BİLMEZ   : aralığın ne olduğunu. Sınırı mesaja sen yazarsın  ║
║              ██ orta argüman boş bırakılırsa tipin tek        ║
║              üstünlüğü çöpe gider ██                          ║
╚═══════════════════════════════════════════════════════════════╝

╔═ ArgumentException ═══════════════════════════════════════════╗
║  Ne yapar : "tip doğru, değer/bileşim geçersiz" der           ║
║  Vaadi    : üçünün ATASIdır — catch (ArgumentException) üçünü ║
║             birden yakalar                                    ║
║  BİLMEZ   : gösterilecek bir sayı. Taşıyacak alanı yok        ║
║             ██ argüman sırası ÖTEKİLERİN TERSİ ██             ║
╚═══════════════════════════════════════════════════════════════╝

╔═ InvalidOperationException ═══════════════════════════════════╗
║  Ne yapar : "argüman değil, NESNENİN O ANKİ DURUMU uygun      ║
║             değil" der                                        ║
║  Vaadi    : hiçbir parametreyi suçlamaz                       ║
║  BİLMEZ   : bu projede hiç fırlatılmadığını ██ 0 çağrı ██     ║
╚═══════════════════════════════════════════════════════════════╝

╔═ Math.Max / Math.Min ═════════════════════════════════════════╗
║  Ne yapar : iki sayıdan büyüğünü/küçüğünü döndürür            ║
║  Vaadi    : dalsız, değişkensiz, tek ifade                    ║
║  BİLMEZ   : KELEPÇE mi ÖLÇÜ mü olduğunu — bunu operandların   ║
║             statüsü söyler, fonksiyonun adı değil             ║
╚═══════════════════════════════════════════════════════════════╝
```

---

## Birinci durak: `nameof` — önce ölç, sonra inan

`nameof(amount)` ile `"amount"` çalışma zamanında **tıpatıp aynı** dizeyi
üretir. O hâlde neden 80 kez `nameof` yazılmış?

Cevap çalışma zamanında değil, **derleme zamanında**:

```
DENEY  —  DamageRules.ResolveRemaining'in `amount` parametresini
          `damageAmount` diye yeniden adlandır.
          throw satırına PARMAK SÜRME. Derle.

  şimdiki kod:
     throw new ArgumentOutOfRangeException(nameof(amount), amount, "...");
                                                  ▲
     ██ DERLENMEZ ██  CS0103: "The name 'amount' does not exist in the
                      current context". Proje kırmızı. Düzeltmeden
                      devam edemezsin.

  düz dize olsaydı:
     throw new ArgumentOutOfRangeException("amount", damageAmount, "...");
                                            ▲
     DERLENİR. Testler yeşil. Ve satır artık YALAN SÖYLÜYOR:
     hata, artık var olmayan bir parametreyi suçluyor.
```

Ölçü tek cümlelik: **`nameof` yanlış olduğunda derlenmez, düz dize yanlış
olduğunda derlenir.** Fark "IDE yeniden adlandırma aracı ikisini de günceller"
değil — o araç kullanılırsa ikisi de güncellenir zaten. Fark, **araç
kullanılmadığında**: elle düzeltme, birleştirme çakışması, kopyala-yapıştırılmış
bir guard bloğu. `nameof` bu üç yolun üçünde de derleyiciyi bekçi yapar; dize
hiçbirinde yapmaz.

### Bu ipi testler TUTMUYOR

Doğrulandı: `Assets/Tests` içinde **`ParamName` kelimesi hiç geçmiyor**. Testler
yalnızca istisnanın TİPİNİ iddia ediyor:

```csharp
Assert.Throws<ArgumentException>(
    () => BattleActions.Revive(battle, medic, new Unit("Ghost")));
```

```
     hangi parametrenin suçlandığı  ──►  hiçbir test bakmıyor
     hangi tipin atıldığı           ──►  testler bakıyor
                       ██ AYRIŞMA ██
     dolayısıyla paramName'i doğru tutan TEK mekanizma derleyici,
     yani nameof'un kendisi. Düz dizeye geçilse kimse fark etmezdi.
```

Bu, `nameof`'un burada süs değil **tek koruma** olduğu anlamına geliyor.

### `nameof` ne DEĞİL

- **Reflection değil.** Çalışma zamanında hiçbir tip incelenmez; derleyici
  satırı bir dize sabitine çevirir. Maliyeti sıfırdır.
- **Değeri okumaz.** `nameof(amount)` `"amount"` verir, `-3` değil. Sayıyı
  taşıyan şey `ArgumentOutOfRangeException`'ın ikinci argümanıdır.
- **Nitelenmiş ad vermez.** `nameof(AttackRules.CanAttack)` → `"CanAttack"`.
  `"AttackRules.CanAttack"` **değil**. Yalnızca son parça gelir.

---

## İkinci durak: `nameof`'un ürünü bir DİZEDİR, ve yolculuk eder

`nameof` yalnızca `throw` satırının içinde yazılmaz. `UnitGrid`'de üç yazma
metodu tek bir sınır kapısını paylaşıyor ve parametre adı o kapıya **dize olarak
taşınıyor**:

```
PlaceUnit(int x, int y, Unit unit)
   └─ ThrowIfOutsideGrid(x, y, nameof(x), nameof(y))
                                  │          │
                                 "x"        "y"      ◄── dize BURADA doğdu

MoveUnit(int fromX, int fromY, int toX, int toY)
   ├─ ThrowIfOutsideGrid(fromX, fromY, nameof(fromX), nameof(fromY))
   │                                      "fromX"       "fromY"
   └─ ThrowIfOutsideGrid(toX,   toY,   nameof(toX),   nameof(toY))
                                          "toX"         "toY"
        │
        ▼
ThrowIfOutsideGrid(int x, int y, string xParamName, string yParamName)
   throw new ArgumentOutOfRangeException(xParamName, x, "x is outside the grid.");
                                             ▲
   ██ BURADA nameof YAZILAMAZ ██
      `nameof(x)` yazılsaydı dört çağrının dördü de "x" derdi ve
      MoveUnit'in hatası hangi köşeyi suçladığını söyleyemezdi:
      "toY tahta dışı" ile "fromY tahta dışı" aynı cümleye düşerdi
```

Kural: **`nameof`, adın GÖRÜLDÜĞÜ yerde çağrılır; ortak kapıya kadar sıradan bir
dize olarak taşınır.** Aynı desen `BattleActions`'ta da var — `RequireCombatant`
ve `RequireCell` bir `string paramName` alıyor, çağıranlar `nameof(attacker)` /
`nameof(target)` / `nameof(reviver)` / `nameof(unit)` besliyor.

**Bedeli:** `paramName` artık `nameof` korumasının dışında bir parametre. Yanlış
dize geçilirse derleyici susar. Koruma, `nameof`'un çağrı yerinde durmasından
geliyor — ortak kapıya bir dize sabiti yazılsaydı zincirin tamamı çürürdü.

### `nameof`'un ikinci yüzü: parametre değil ÜYE adı

Üç test dosyasında `nameof` bambaşka bir iş yapıyor:

```csharp
MethodInfo[] canAttackOverloads = typeof(AttackRules)
    .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
    .Where(method => method.Name == nameof(AttackRules.CanAttack))
    .ToArray();
```

Bu test bir davranışı değil bir ŞEKLİ koruyor: "tek bir `CanAttack` var ve tek
bir durum parametresi alıyor". Reflection metodu ADIYLA arıyor — yani burada bir
dize zorunlu.

```
`nameof(AttackRules.CanAttack)`      "CanAttack"  ← rename ile birlikte gider
                                     CanAttack silinirse ██ DERLENMEZ ██

`"CanAttack"` düz dize olsaydı       CanAttack yeniden adlandırıldığında
                                     Where(...) SIFIR sonuç döner
                                     → Assert.That(length, Is.EqualTo(1)) kırmızı
                                     → ama YANLIŞ SEBEPLE kırmızı:
                                       "şekil bozuldu" değil "metot yok"
                                     ██ testi okuyan yanlış yerde arar ██
```

Yani her iki durumda da bir şey kırılıyor; fark **kırılmanın hangi cümleyi
söylediği**. `nameof` "adı güncelle" der, dize "kural bozuldu" diye yalan söyler.

---

## Üçüncü durak: dört tip, tek soru

Önce soyağacı — üçünün akraba, dördüncünün yabancı olduğunu görmeden seçim
yapılamaz:

```
System.Exception
   └── SystemException
         │
         ├── ArgumentException                "argüman geçersiz"
         │     ├── ArgumentNullException          null geldi
         │     └── ArgumentOutOfRangeException    aralık dışı (+ sayıyı taşır)
         │
         └── InvalidOperationException
               ██ AYRIŞMA NOKTASI ██
               Argument ailesinin ALTINDA DEĞİL. Çünkü suçladığı
               şey argüman değil: NESNENİN O ANKİ DURUMU.
               catch (ArgumentException) bunu YAKALAMAZ.
```

### Seçim ölçüsü: orta argümanı doldurabiliyor musun

`ArgumentOutOfRangeException`'ın üç argümanlı sürümü ötekilerde olmayan tek şeyi
taşır: **sayının kendisi**.

```csharp
throw new ArgumentOutOfRangeException(nameof(range), range, "Range must be at least 1.");
//                                                    ▲
//                                    ActualValue — logda -7 diye görünür
```

Ölçü budur ve projedeki 32 `ArgumentOutOfRangeException`/`ArgumentException`
satırının tamamında tutuyor:

```
     Argüman TEK BAŞINA, bir sınırın dışında mı?
     (ActualValue alanına yazınca okuyana bir şey söylüyor mu?)

       EVET ──► ArgumentOutOfRangeException(nameof(p), p, "...")
                width = 0            → "pozitif olmalıydı"
                range = -1           → "en az 1 olmalıydı"
                deltaSeconds = -0.5f → "zaman geri akmaz"
                actionsUsedThisTurn = -1

       HAYIR ──► ArgumentException("...", nameof(p))
                 boş dizilim       → argüman bir LİSTE, sayı değil
                 listede Team.None → argüman bir LİSTE, sayı değil
                 bu savaşta değil  → argüman bir Unit; suç İLİŞKİDE
                 hücre dolu        → argüman bir koordinat ve TAMAMEN
                                     GEÇERLİ; suç tahtanın o anki hâlinde
                 NaN               → ██ sayı ama hiçbir eksende yer
                                        tutmuyor ██
```

Son satır tek istisnayı işaretliyor ve tesadüf değil: **NaN bir eksende yer
tutmaz.** "Çok büyük" ya da "çok küçük" diye bir cevabı yoktur, dolayısıyla
`ActualValue` alanına yazılsa bile hiçbir şey söylemez.

### `PointerGesture`: aynı parametre, iki farklı istisna

Sınırın kodda gözlenebildiği tek yer:

```
PointerGesture(float dragThreshold)

   dragThreshold = NaN   ──► ArgumentException("Drag threshold cannot be NaN.",
                                               nameof(dragThreshold))
                             ██ eksende DEĞİL ██

   dragThreshold = -1f   ──► ArgumentOutOfRangeException(nameof(dragThreshold),
                                                dragThreshold, "...cannot be negative.")
                             eksende, ve solunda

   dragThreshold = 0f    ──► GEÇERLİ (en ufak kıpırdama sürüklemedir)

   ██ SIRA ZORUNLU ██  NaN kontrolü ÖNCE gelmek zorunda: NaN her
      karşılaştırmada false verir, yani (dragThreshold < 0f) testinden
      yara almadan geçer.
```

### ██ Argüman sırası ters ██ — ve derleyici uyarmaz

Projedeki 66 `throw` satırının tamamı bu üç kalıba uyuyor:

```
                              1. argüman     2. argüman     3. argüman
  ArgumentNullException        paramName          —              —
  ArgumentOutOfRangeException  paramName      actualValue     message
  ArgumentException            message        paramName          —
                                  ██ SIRA TERS ██
```

Ölçü: `ArgumentException`'ın iki argümanını yer değiştir. **Derlenir** — ikisi de
`string`. Çalışma zamanında mesaj `"unit"`, parametre adı ise
`"The unit is not in this battle."` olur. Hiçbir test bunu görmez (yukarıda:
`ParamName` iddiası yok), hiçbir uyarı çıkmaz. Bu satırı doğru tutan tek şey
okumaktır.

`ArgumentNullException`'ın tek argümanlı sürümü de aynı tuzağı taşır: verdiğin
dize **mesaj değil ADdır**. `new ArgumentNullException("Unit cannot be null.")`
derlenir ve parametre adı olarak o cümleyi kaydeder. Projedeki 34 satırın 34'ü de
tek argümanlı ve `nameof` besliyor.

---

## Dördüncü durak: fırlatılmayan istisna

`InvalidOperationException` bu projede **hiç fırlatılmıyor**. Üç yerde adı
geçiyor, üçü de "fırlatılmadığını" anlatmak için:

```
Battle.cs:413              foreach sırasında Dictionary değiştirilirse .NET
                           bunu fırlatır → RemoveReadyForCleanup'ın iki
                           geçiş yapmasının sebebi
                           (mekanizma: dil/02-koleksiyonlar-ve-salt-okunur.md)

Docs/deep/kod/Core/        REDDEDİLEN alternatif — .cs'te DEĞİL, gerekçe
PointerGesture.md:598      belgesinde yazılı:
                              if (IsActive) throw new
                                  InvalidOperationException("Pointer is pressed.");
                           KIRILAN: alt+tab ile yutulmuş TEK bir bırakma
                           olayı, sonraki tıklamada oyunu düşürürdü

PointerGestureTests:138    o reddin bekçisi:
                           Press_WhileAlreadyPressed_RestartsFromTheNewOrigin
```

Neden boş? Çünkü `InvalidOperationException`'ın tarif ettiği durum —
*"argüman değil, nesnenin o anki durumu uygun değil"* — bu projede zaten **başka
bir kanala** bağlanmış:

```
   "nesnenin şu anki durumu bu işlemi kaldırmıyor"
                        │
        ┌───────────────┴───────────────┐
        ▼                               ▼
  InvalidOperationException      Rejected* sonuç değeri
  (BCL'in verdiği isim)          (bu projenin seçtiği yol)
        │                               │
  çağıran ne yapsın?             çağıran ne yapsın?
  ── hiçbir şey; çökme           ── bekle / yaklaş / başka hedef seç
        │                               │
        └──── ██ AYRIM ÖLÇÜTÜ: karşısında yapılacak
                 bir şey var mı? ██ ────┘

   sıra sende değil        →  RejectedActorCannotAct
   düşmüş birim yürüyemez  →  RejectedActorCannotAct
   hücre dolu              →  RejectedCellOccupied
   pointer zaten basılı    →  yeni jest başlat (ret bile değil)
```

Yani sıfır, tipin gereksiz olduğunu değil, **projenin o soruya istisnayla cevap
vermemeye karar verdiğini** gösteriyor. Ölçütün kendisi
[`konular/04-karar-sirasi.md`](../konular/04-karar-sirasi.md)'te yazılı.

**Kapsam sınırı:** bu, `InvalidOperationException`'ın yanlış bir tip olduğu
anlamına gelmez. Kütüphane yazıyorsan ve çağıranın elinde düzeltilecek bir şey
yoksa (kapalı bir akışa yazmak, tüketilmiş bir enumerator'ı ilerletmek) doğru
tiptir. Burada boş olmasının sebebi tip değil, **çağıranın oyuncu olması**.

---

## Beşinci durak: `Math.Max` / `Math.Min` — kelepçe

İki kural sınıfı ayna ikizi: biri ALT kelepçeyi taşır, öteki ÜST.

```csharp
// DamageRules.ResolveRemaining
return Math.Max(0, current - amount);      // can sıfırın altına inemez

// HealingRules.ResolveRestored
return Math.Min(max, current + amount);    // can maksimumu aşamaz
```

### Neden `if` değil

`if` sürümü aynı sonucu verir:

```csharp
int result = current - amount;
if (result < 0)
{
    result = 0;
}

return result;
```

Fark satır sayısı değil, **üç ayrı şey**:

```
  Math.Max(0, current - amount)        if (result < 0) { result = 0; }
  ───────────────────────────────      ────────────────────────────────
  ① yerel değişken YOK                 ① `result` adında bir yazılabilir
     araya girilecek nokta yok            değişken doğar; iki satır boyunca
                                          ona başka bir şey de yazılabilir

  ② dal YOK — tek yol                  ② iki yol; ikisi de sınanmalı

  ③ ifade `return`'ün ÜSTÜNDE          ③ kelepçe `return`'den üç satır
     durur; "dönen değerin alt             uzakta; okuyucu ikisini kafasında
     sınırı 0" tek satırda okunur          birleştirmek zorunda
```

`if` yanlış değil; kaybettiği şey **ifade olma** özelliği. Kelepçe bir dal değil
bir sınırdır ve sınır, sonucun tanımının parçasıdır.

**Ölçü:** `Math.Max(0, ...)` sarmalını sil, `current - amount` döndür.
`ResolveRemaining_NeverReturnsNegative` testi kırmızıya döner — 0'dan 50'ye kadar
her hasar değerini deniyor ve 10 canlı birime 11 hasar vurulduğunda `-1` çıkıyor.

**İkinci ölçü — kelepçe neden burada, çağıranda değil:** `DamageRules`'un bugün
üretimde tek çağıranı var (`Health.TakeDamage`, `Health.cs:69`). Kelepçe çağırana
bırakılsaydı aynı `if` her çağırana kopyalanırdı ve ikinci çağıran eklendiği gün
biri unutulurdu — hiçbir derleme hatası çıkmadan. Kuralın kendisi zaten bu yüzden
`Health`'in dışında: formülün girdi uzayı sahibininkinden geniş.

### ██ Her `Math.Max` kelepçe DEĞİL ██ — aynı projeden karşı örnek

```
Math.Max(0,   current - amount)     ← 0 bir SINIR. Bir taraf sabit,
Math.Min(max, current + amount)       öteki taraf hesaplanan değer.
                                      Anlamı: "şu çizgiyi geçme"

Math.Max(dx, dy)   (GridDistance.Between)
   ██ KELEPÇE DEĞİL ██  İki taraf da EŞİT STATÜDE ölçüm.
                        Ne biri sabit, ne biri sınır.
                        Anlamı: "en uzun eksen kaç adımsa uzaklık odur"
                        — yani Chebyshev mesafesi, bir oyun kuralı.
```

Ayırt etme ölçüsü: **operandlardan biri sınır mı, yoksa ikisi de ölçüm mü?**
`Math.Max(0, x)` ile `Math.Max(dx, dy)` aynı fonksiyondur ve farklı iki şey
söyler; fonksiyonun adı bunu söylemez, operandların statüsü söyler.

### ██ Hangi `Math` ██ — ve neden `Mathf` hiç yok

```
System.Math          .NET'in matematik sınıfı. netstandard.dll içinde.
                     Max(int, int) → int; hiçbir dönüşüm yok.

UnityEngine.Mathf    Unity'nin matematik sınıfı. Max(float, float) → float
                     (int aşırı yüklemesi de var). Motor derlemesinde yaşar.
```

Projede `Mathf` **hiç geçmiyor** — ve bu bir tercih değil, bir duvar:
`GridStrategy.Core`, `GridStrategy.Combat` ve `GridStrategy.Battle` asmdef'lerinin
üçü de `"noEngineReferences": true` taşıyor. `Mathf` o üç derlemeden **yazılamaz
bile**; yazılsa derlenmez.

Yani `Math.Max` burada bir zevk meselesi değil, [assembly
duvarının](../konular/02-assembly-duvari.md) doğrudan sonucu. İkisini birbirinin
yerine geçer sanan okuyucu, duvarın nerede durduğunu da yanlış çizer.

---

## Bütün kanal tek bakışta

`BattleActions.Move` — bir çağrının geçtiği bütün kapılar, sırasıyla:

```
BattleActions.Move(battle, unit, toX, toY, moveRange)
   │
   ├─ battle == null           ──► ArgumentNullException(nameof(battle))       ┐
   ├─ unit   == null           ──► ArgumentNullException(nameof(unit))         │
   ├─ new MoveProfile(moveRange)                                               │  İSTİSNA
   │     └─ range < 0          ──► ArgumentOutOfRangeException(                │  KANALI
   │        (MoveProfile'ın         nameof(range), range, "...")               │
   │         içinde, param                                                     │
   │         adı `range`)                                                      │
   ├─ RequireCombatant         ──► ArgumentException("The unit is not          │  okuyucu:
   │                                   in this battle.", paramName)            │  PROGRAMCI
   └─ RequireCell              ──► ArgumentException(aynı mesaj, paramName)    ┘
   │
   ═══════════════════════════════════════════════════════════ ██ ÇİZGİ ██
   │
   ├─ TurnRules.CanAct  ✗      ──► MoveOutcome.RejectedActorCannotAct          ┐
   ├─ MovementRules.CanMove ✗  ──► MoveOutcome.RejectedActorCannotAct          │  SONUÇ
   └─ MoveAction.Execute                                                       │  KANALI
         ├─ tahta dışı hedef   ──► MoveOutcome.RejectedInvalidDestination      │
         ├─ menzil dışı        ──► MoveOutcome.RejectedOutOfRange              │  okuyucu:
         ├─ hücre dolu         ──► MoveOutcome.RejectedCellOccupied            │  OYUNCU
         └─ başarılı           ──► MoveOutcome.Moved                           ┘
```

Çizginin ÜSTÜNDE `nameof` altı kez geçiyor ve altında hiç geçmiyor. Sebep basit:
altta suçlanacak bir parametre yok — orada suçlanan şey oyuncunun hamlesi.

---

## Kural: `throw` satırını nasıl yazacaksın

```
① Argüman null mı?
      evet ──► ArgumentNullException(nameof(p))
               ██ tek argüman, ve o argüman AD ██
      hayır ──► ②

② Argüman TEK BAŞINA bir sınırın dışında mı?
      evet ──► ArgumentOutOfRangeException(nameof(p), p, "sınırı söyleyen cümle")
               ██ orta argümanı ATLAMA — tipin tek üstünlüğü o ██
      hayır ──► ③

③ Sayı olmayan, ya da sayı olup da suçu BAŞKA BİR ŞEYLE İLİŞKİDE
  olan bir geçersizlik mi?
      evet ──► ArgumentException("ne olduğunu söyleyen cümle", nameof(p))
               ██ SIRA TERS: önce mesaj, sonra ad ██
      hayır ──► ④

④ Suçlanan argüman değil, nesnenin O ANKİ DURUMU mu?
      ── çağıranın yapabileceği bir şey VAR mı?
            var ──► istisna DEĞİL: bir Rejected* sonuç değeri
                    ██ bu projenin 66 throw'unun tamamı ①②③'te ██
            yok ──► InvalidOperationException
```

Bir kelepçe yazacaksan ayrı bir soru:

```
Bir sınırın altına/üstüne düşmeyi engelliyorsan  ──► Math.Max / Math.Min
İki ölçümden birini seçiyorsan                   ──► gene Math.Max, ama bu
                                                     bir KELEPÇE DEĞİL; yorumu
                                                     da öyle yazma
```

---

## Yanlış hatırlanan beş şey

**"`nameof` çalışma zamanında adı okur."** Hayır. Derleyici satırı bir dize
sabitine çevirir; çalışma zamanında ne reflection ne tip incelemesi vardır.
Maliyeti düz dizeninkiyle aynıdır — sıfır.

**"`nameof(AttackRules.CanAttack)` tam nitelenmiş adı verir."** Hayır,
yalnızca `CanAttack` verir. `nameof` her zaman **son parçayı** döndürür.

**"`ArgumentException`'ın ilk argümanı parametre adıdır."** Hayır, mesajdır.
Sıra `ArgumentOutOfRangeException`'ınkinin tam tersidir ve ikisi de `string`
olduğu için derleyici hiçbir şey demez.

**"`catch (ArgumentException)` yazarsam testim de üçünü birden yakalar."** İlki
doğru — `ArgumentNullException` ve `ArgumentOutOfRangeException` ondan türer.
Ama NUnit'in `Assert.Throws<T>` metodu **tam tip** eşleşmesi ister; türemiş
olanı yakalamaz. Gevşek olanı `Assert.Catch<T>`'dir. Projedeki
`Assert.Throws<ArgumentException>` satırları yeşil, çünkü `RequireCombatant`
gerçekten tam olarak `ArgumentException` fırlatıyor.

**"`Math.Max` gördüm, demek ki kelepçe."** Her zaman değil.
`GridDistance.Between`'teki `Math.Max(dx, dy)` bir kelepçe değil bir ölçüdür —
Chebyshev mesafesi. Ayırt eden şey operandların statüsü: biri sınır mı, yoksa
ikisi de ölçüm mü.

---

## Kaçış yolu: bunların yerine ne olurdu

```
nameof yerine "amount"        → derlenir, sessizce eskir; hiçbir test görmez
                                (ParamName iddiası yok — ölçüldü, 0 satır)

paramName'i hiç vermemek      → ArgumentOutOfRangeException() parametresiz
                                sürümü var; mesaj "Specified argument was out
                                of the range of valid values." olur ve HANGİ
                                argüman olduğu kaybolur

her şeye ArgumentException    → derlenir; ActualValue kaybolur, catch ayrımı
                                kaybolur, testlerdeki tip iddiası hiçbir şeyi
                                ayırt etmez hâle gelir

throw yerine Debug.Assert     → System.Diagnostics.Debug.Assert [Conditional
                                ("DEBUG")] taşır: Release derlemesinde çağrı
                                HİÇ ÜRETİLMEZ. Üretimde koruma sıfır.
                                (UnityEngine.Debug.Assert ayrı bir üyedir ve
                                noEngineReferences yüzünden buradan zaten
                                erişilemez)

kelepçe yerine if             → aynı sonuç, ama bir yazılabilir yerel değişken
                                ve bir dal doğar; sınır return'den uzaklaşır

kelepçeyi çağırana bırakmak   → düzeltme çağıran sayısı kadar kopyalanır;
                                ikinci çağıran eklendiği gün negatif can
                                yazılır ve derleyici susar
```

Seçilen bileşim — `nameof` + üç tipli aile + ifade kelepçesi — altısının da
ortasını tutuyor: ad derleyici korumasında, sayı istisnanın içinde, sınır
dönüşün üstünde, ve hiçbiri Unity'ye bağımlı değil.

---

Kodda **karar**, burada **ödünç alınan aracın sözleşmesi**. İkisi çelişirse kod
kazanır — orası çalışan metin, burası anlatı.
