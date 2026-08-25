# AttackProfile

> **Kaynak:** `Assets/Game/Core/Combat/AttackProfile.cs`
> **Ad alanı:** `GridStrategy.Combat` · **Assembly:** `GridStrategy.Combat` (`noEngineReferences: true`)
> **Rol:** Tanım (Profile) — kimliği yok, hafızası yok, karar vermez; sayıyı taşır

Bir saldırı türünün değişmez tanımı: "kılıç 10 hasar, 1 hücre menzil".
**Tanım**dır, varlık değildir — aynı değerlere sahip iki `AttackProfile`
birbirinin **yerine geçebilir** (ölçü `==` değil: `Equals` yazılmadığı için o
karşılaştırma `false` döner) ve yüzlerce asker tek bir örneği paylaşabilir.

Bu yüzden hiçbir alanı sonradan **değişmez**: bir profil oluşturulduktan sonra
sabittir. Değişebilseydi, onu paylaşan her birim habersiz etkilenirdi.

Neyi **tutmaz**: kimin saldırdığını, kime saldırıldığını, o anki bekleme
süresini. Bunlar çağrı anına ya da birime ait; tanıma değil. "Menzile giriyor
mu" sorusunu da `AttackResolver` cevaplar.

Unity notu: bugün düz bir C# nesnesi. `ScriptableObject` kararı geldiğinde rol
başlığındaki `Unity` satırı değişir, **rol** değişmez.

| Üye | Karar | Detay |
|---|---|---|
| `AttackProfile` (tip) | `sealed class` — TANIM paylaşılır, kopyalanmaz | [↓](#attackprofile-tip) |
| `AttackProfile(int, int)` | doğrulama kurucuda; `OnValidate`'e kaçamaz | [↓](#attackprofileint-damage-int-range) |
| `Damage` | ham hasar; zırh/direnç burada DEĞİL | [↓](#damage) |
| `Range` | kaç hücre uzağa ulaşır; mesafeyi ÖLÇMEZ | [↓](#range) |

**İlgili anlatılar:** [02-assembly duvarı](../../../konular/02-assembly-duvari.md)

---

## AttackProfile (tip)

### HARİTA: aynı profili yüzlerce asker "paylaşınca" ne oluyor

```
SEÇİLEN — sealed class (REFERANS tipi)
  asker#1 ─┐
  asker#2 ─┼──► ╔══════════════════════╗
    ...    │    ║ AttackProfile(10, 1) ║  ◄── TEK nesne, N ok
  asker#N ─┘    ╚══════════════════════╝

REDDEDILEN — readonly struct (DEĞER tipi)
  asker#1 ──► [10,1]  ┐
  asker#2 ──► [10,1]  ├─ N ayrı KOPYA
    ...               │
  asker#N ──► [10,1]  ┘
  ◄── AYRIŞMA: kopya her alan okumasında ve her parametre geçişinde
      yeniden doğar; ayrıca `null` diye bir hâl KALMAZ
```

### KAPSAM: bu ad alanında değer tipi yasak DEĞİL

Ayıraç, **paylaşımın** sözleşmenin parçası olup olmadığıdır:

```
paylaşım sözleşmede     ► referans tipi (AttackProfile)
iki eşit değer aynı
şeydir, paylaşımın
anlamı yok              ► değer tipi (AttackOutcome, UnitState, Team
                          — üçü de enum)
```

Karşı örnek aynı ad alanında: `AttackOutcome` bir enum'dur, düpedüz değer tipi,
ve orada doğru seçim odur — iki `Hit` değeri aynı şeydir, paylaşılacak bir
kimlik yoktur. Bu dosyanın kendi rol başlığı da aynı ayrımı yapıyor: "(10
hasar, 1 menzil) olan iki nesne aynı şeydir" — yani **kimlik yok, ama paylaşım
var**.

### İŞ BÖLÜMÜ: sınıf olmak ile değişmez olmak AYRI şeyler

```
`sealed class`        ► paylaşımı mümkün kılar (tek nesne, N ok)
                        ve `null` hâlini var eder
get-only property'ler ► paylaşılan nesnenin sonradan değiştirilmesini
                        engeller
```

Fazlalık değil bölüşme: sınıf olmasaydı paylaşım kopyaya dönerdi ve
`AttackResolver`'daki null koruması anlamsız kalırdı; get-only olmasaydı
paylaşım ayakta kalır ama tek bir yazma yüzlerce askeri habersiz etkilerdi.

### Get-only burada yetiyor, genel olarak yetmez

`Damage` ve `Range` `int`, yani **değer** tipi; get-only olmaları gerçek bir
değişmezlik veriyor. Aynı yazım referans tipi bir alanda (bir liste, bir dizi,
ikinci bir profil) yalnızca **atamayı** keser, işaret edilen nesnenin **içini**
dondurmaz. Bu tipe böyle bir alan eklendiği gün "hiçbir alanı sonradan
değişmez" cümlesi bu satırdan değil, o alanın kendi tipinden gelmek zorunda
kalacak.

### REDDEDILEN

```csharp
public readonly struct AttackProfile
```

**KIRILAN:** paylaşılan tek örnek her çağrıda kopyaya döner — "yüzlerce asker
tek profili paylaşır" cümlesi sessizce yalan olur.

```
struct null olamaz -> AttackResolver'daki null koruması anlamsızlaşır
derleyici: null atamayı der  ·  test: IsWithinRange_NullProfile_Throws
derlenmez ve o sözleşme sınanamaz kalır
```

**KAZANIRDI:** profil paylaşılmayıp birim başına saklansaydı ve her karede
binlerce kez okunsaydı — iki `int` kopyalamak referans takibinden ucuz kalırdı.

**TEK CUMLE:** TANIM paylaşılır, kopyalanmaz; kopyalanan tanım artık TEK tanım
değildir.

---

## AttackProfile(int damage, int range)

Doğrulama kurucuda durur: profil **hangi yoldan gelirse gelsin** (kod, test,
gelecekteki bir yükleyici) geçersiz değer üretilemez.

### HARİTA: assembly'ler ve motorun nerede başladığı

```
GridStrategy.Combat     references: []   noEngine: TRUE
  AttackProfile, AttackRules, TargetingRules, ...
  ◄── ScriptableObject BURAYA GİREMEZ: UnityEngine yok

GridStrategy.Core       references: []   noEngine: TRUE
  UnitGrid, GridDistance, MoveAction, ...

GridStrategy.Battle     ► Core, Combat   noEngine: TRUE

GridStrategy.Unity      ► Core, Combat, Battle
                        noEngine: FALSE  ◄── MOTOR BURADA
  BoardAdapter, UnitView — MonoBehaviour, SerializeField

>> DUVAR << Combat ile Unity arasında; ok yalnızca yukarıdan aşağı
(Unity ► Combat) akar, tersi DERLENMEZ.
```

### KAPSAM: ScriptableObject bu projede yasak DEĞİL

Reddedilen şey desenin kendisi değil **yeri**: bu assembly. Aynı ihtiyacın
meşru evi bir duvar ötededir.

Karşı örnek aynı projede: `GridStrategy.Unity` assembly'sinin
`noEngineReferences` değeri FALSE ve orada yaşayan `BoardAdapter` ile
`UnitView` motoru doğrudan kullanır. Yani "motora bağlanma" genel bir yasak
değil, katmana özel bir sınır.

### İŞ BÖLÜMÜ: kurucu doğrulaması ile asmdef duvarı

```
kurucudaki iki `throw`        ► profil HANGİ yoldan gelirse gelsin
                                geçersiz değer üretilemez
asmdef'in noEngineReferences  ► doğrulamanın OnValidate'e kaçmasını
                                en baştan imkânsız kılar
```

İkisi aynı işi iki kez yapmaz: `throw`lar silinirse duvar ayakta kalır ama
menzili 0 olan bir profil üretilebilir; duvar kaldırılırsa bugün **hiçbir şey
kırılmaz** — tehlikeli olan tam da budur, çünkü `OnValidate`'e kayan doğrulama
ancak koddan üretilen ilk profilde görünür ve o an testler çoktan yeşildir.

### REDDEDILEN

Kurucu silinir, tip `ScriptableObject`'ten türer:

```csharp
[SerializeField] private int damage;
[SerializeField] private int range;
private void OnValidate() { range = Mathf.Max(1, range); }
```

**KIRILAN:** doğrulama `OnValidate`'e taşınır ve yalnızca Inspector'da çalışır;
koddan üretilen profil hiç sınanmaz.

```
asmdef'in noEngineReferences sınırı düşer -> saf kural motora bağlanır
derleyici: asmdef sınırını der  ·  test: AttackProfileTests'in
Throws testleri derlenmez
```

**KAZANIRDI:** hasar ve menzil sayılarını programcı değil **tasarımcı**
ayarlayacaksa — asset olarak Inspector'dan, yeniden derlemeden dengelenirdi.

**TEK CUMLE:** Bir kuralı motora bağlamak onu sınanabilir olmaktan çıkarır;
asmdef sınırı bu kararın yazılı hâlidir.

### Menzil en az 1

Sıfır menzilli bir saldırı hiçbir hücreye ulaşamazdı ve sessizce hiçbir işe
yaramayan bir birim üretirdi.

**Alternatif:** `if (range < 0)` — 0 menzil geçerli olurdu. Seçilmedi: sebebi
yukarıda; 0 ancak kendi hücresine uygulanan bir yetenek geldiği gün "sadece
kendi hücrem" anlamını kazanır.

---

## Damage

Bir vuruşun **ham** hasarı. Zırh, direnç, kalkan emilimi ve kritik çarpanı
burada **değil** — onların evi `DamageRules`.

Get-only otomatik property: kurucuda konur, bir daha yazılmaz. Neden bunun
gerçek bir değişmezlik verdiği (ve nerede vermeyeceği)
[yukarıda](#get-only-burada-yetiyor-genel-olarak-yetmez) yazılı.

---

## Range

Kaç hücre uzağa ulaşabildiği. **Mesafenin nasıl ölçüldüğünü bilmez** — o ayrı
bir oyun kuralıdır ve `GridStrategy.Core`'da yaşar; bu sayı yalnızca eşiği
verir, karşılaştırmayı `AttackResolver.IsWithinRange` yapar.

Kurucudaki `range < 1` kelepçesi bu property'nin değişmezidir; gerekçesi
[yukarıda](#menzil-en-az-1).
