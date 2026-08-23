# Erişim ve sözleşme — kim görebilir, kim değiştirebilir

> **HANGİ DİL ARACI** — *bu dosyanın anlattığı, ödünç alınmış adlar:*
> `public` · `private` · `internal` · `protected` · `public sealed class` ·
> `public static class` · bir üyeye `private` mi `public` mi yazacağını sorduğun an
>
> **NEREDE GEÇİYOR** — *bu araçların bu projede yaşadığı yerler:*
>
> | dosya | üye |
> |---|---|
> | `Assets/Game/Battle/Battle.cs` | `Board` — ██ `internal UnitGrid Board => board;` · projenin **tek** `internal`i ██ |
> | dört `.asmdef` | `Assets/Game/Core/GridStrategy.Core.asmdef` · `Assets/Game/Core/Combat/GridStrategy.Combat.asmdef` · `Assets/Game/Battle/GridStrategy.Battle.asmdef` · `Assets/Game/Unity/GridStrategy.Unity.asmdef` |
> | `Assets/Game/Battle/BattleActions.cs` | `BattleActions` (`public static class`) |
> | `Assets/Game/Core/Combat/AttackAction.cs` | `AttackAction` (`public static class`) |
> | `Assets/Game/Core/Combat/Structure.cs` | `Structure` (`public sealed class`) |
> | `Assets/Tests/EditMode/Combat/CombatantTests.cs` | erişimin testten görünüşü |
>
> **NE ZAMAN OKU** — *hangi soruyu sorduğunda ya da hangi değişikliğe giriştiğinde:*
> bir üyeye erişim belirteci yazarken, "burada bir `interface` olmalı mıydı" diye
> sorduğunda, ya da `internal` görüp "aynı klasör demek" diye okuduğunda.

**BURAYA KODDAN GELDİYSEN** — ██ gelemezsin: bu belgeye giden **hiçbir** kod
işaretçisi yok. ██ Ölçüldü — `dil/` ağacının kod işaretçisi `DİL:` etiketiyle
yazılır (`konular/` ağacınınki `DERİN ANLATIM:`), ve `Assets/` altında bu
belgeyi anan **sıfır** `DİL:` satırı var. Yani ok bugün **tek yönlü**: buradan
koda gidilir, koddan buraya gelinmez — ██ `Battle.Board`'un yorumundaki işaretçi
bile [`konular/03`](../konular/03-tahta-sahipligi.md)'ü gösteriyor, bu dosyayı
değil. ██

Bu dosya iki soruyu birden sahipleniyor ve ikisi aynı sorunun iki ucu:

```
  ① GÖRÜNÜRLÜK   "bu üyeyi kim GÖREBİLİR"
                 public · private · internal
                 protected · protected internal · private protected

  ② SÖZLEŞME     "çağıran SOMUT bir tipe mi, yoksa bir YETENEK
                 tanımına mı bağlansın"
                 interface · abstract · virtual · override
```

İkisi bir dosyada, çünkü ikisi de tek bir cümlenin parçası: **bir tipin dışarıya
ne söz verdiği.** Erişim belirteci sözün *kime* verildiğini, sözleşme
mekanizmaları sözün *neye* verildiğini yazar.

> **SINIR — sahibi başka dosyalar.** Burada TEKRAR EDİLMEYEN üç şey:
> `sealed`'in ne yaptığı ve neyi vaat etmediği
> [`dil/01`](01-degismezlik-anahtar-kelimeleri.md#dorduncu-durak-sealed-tip-agacini-keser-nesne-grafigini-kesmez)'de;
> assembly duvarının kendisi — kim kimi göremez, klasör ≠ ad alanı ≠ assembly —
> [`konular/02`](../konular/02-assembly-duvari.md#duvarin-engelledigi-sey-gorunurluk)'de;
> tahtanın neden korunduğu ve "ikinci yazar" probleminin tamamı
> [`konular/03`](../konular/03-tahta-sahipligi.md#ucuncu-durak-sahipligi-ayakta-tutan-uc-katman)'te.
> Bu dosya o üçünün ÜSTÜNE oturuyor ve yalnız erişim/sözleşme yüzünü anlatıyor.

---

## ██ ÖNCE SAYILAR — hepsi bu depoya karşı sayıldı ██

Sayma yöntemi, tekrar edilebilir olsun diye açık yazılıyor. Önce `//` ile
başlayan satır yorumları siliniyor (yoksa yorumda geçen `public` kelimesi de
sayılırdı), sonra kelime sayılıyor:

```sh
find Assets/Game -name "*.cs" -exec sed 's://.*::' {} \; \
  | grep -oE "\bpublic\b" | wc -l
```

`Assets/Game` altında, 33 üretim dosyasında:

```
  public              156          interface             0   ██ SIFIR ██
  private              98          abstract              0   ██ SIFIR ██
  internal              1   ◄──    virtual               0   ██ SIFIR ██
  protected             0          override              0   ██ SIFIR ██
  protected internal    0
  private protected     0          sealed class         14
                                   static class         12
```

İki uyarı, ikisi de ölçüldü. **①** Ham `grep` bu sayıları vermez: yorum
satırları silinmeden `public` 158, `private` 100, `internal` 3 çıkıyor — fark
yorumların içinde geçen kelimelerden. Yorum sayısı başka çalışmalarla değişir,
**bildirim sayısı değişmez**; bu tablodaki her sayı bildirim sayısıdır.
**②** `Assets/` altındaki tek `protected` kelimesi bir bildirim değil, bir test
mesajının İÇİNDE İngilizce bir cümlede geçiyor.

### İkinci ölçüm: derlenmiş ikiliden

Sayılar yalnız kaynağa değil **derlenmiş çıktıya** da soruldu.
`Library/ScriptAssemblies/` altındaki dört üretim DLL'i (Unity'nin
2026-08-22 08:08 derlemesi) ECMA-335 üstverisinden okundu:

```
  DLL                          tip   interface   interface    abstract+
                                     tanımı      uygulaması   sealed
  ────────────────────────────────────────────────────────────────────
  GridStrategy.Core.dll          8       0            0          2
  GridStrategy.Combat.dll       18       0            0          8
  GridStrategy.Battle.dll        6       0            0          2
  GridStrategy.Unity.dll         2       0            0          0
  ────────────────────────────────────────────────────────────────────
  TOPLAM                        34       0            0         12

  Erişim damgası `Assembly` taşıyan ÜYE — dört DLL'de toplam:
      Battle.get_Board                            ◄── ██ BİR TANE ██
  Erişim damgası `Family` / `FamORAssem` / `FamANDAssem` taşıyan üye:
      yok
  `Virtual` damgası taşıyan metot:
      yok
```

Üç şey burada kanıtlanıyor ve üçü de kaynak okumasıyla değil **üstveriyle**:
`internal` çalışma zamanında `Assembly` diye yazılır; `static class`
`abstract sealed` diye yazılır (beşinci durak); ve 34 tipin hepsi `Public`
görünürlükte — yani bu projede **örtük `internal` tip yok**.

---

## Sahne

Yeni bir üye yazdın. İmleç satırın başında ve bir kelime bekliyor:

```csharp
??? UnitGrid Board => board;
```

Altı kelime yazabilirsin ve altısı da derlenir. Hiçbiri "yanlış" değil —
**farklı büyüklükte odalar** açıyorlar. Cevap `internal` oldu: bütün depoda bir
kez, ve tam olarak burada.

---

## Karakterler

Altı belirteç. Her kutunun ikinci satırı asıl mesele: **duvar NEREDE bitiyor.**

```
╔═ public ══════════════════════════════════════════════════════╗
║  Kim görür : referans veren HER assembly                      ║
║  Duvar     : ██ YOK ██ — bu üye artık bir SÖZ                 ║
║  BİLMEZ    : kaç çağıranı olduğunu. Geri almak, bilmediğin    ║
║              çağıranları kırmak demektir                      ║
╠═ private ═════════════════════════════════════════════════════╣
║  Kim görür : yalnız içeren TİPİN gövdesi (iç içe tipler dahil)║
║  Duvar     : tipin süslü parantezi                            ║
║  BİLMEZ    : aynı DOSYADAKİ başka bir tipi tanımaz — sınır    ║
║              dosya değil TİP                                  ║
╠═ internal ════════════════════════════════════════════════════╣
║  Kim görür : aynı ASSEMBLY'deki her tip                       ║
║  Duvar     : ██ .asmdef ██ — klasör değil, ad alanı değil     ║
║  BİLMEZ    : test assembly'sini. Referans vermek yetmez;      ║
║              InternalsVisibleTo gerekir ve bu projede YOK     ║
╚═══════════════════════════════════════════════════════════════╝

  Kalan üçü kalıtıma bağlı ve bu projede üçü de sıfır kez geçiyor:

    protected            bu tip + TÜREYENLER  (assembly'den bağımsız)
    protected internal   aynı assembly ██ VEYA ██ türeyenler — BİRLEŞİM
                         ██ internal'den DAHA GENİŞ; kesişim sanılır ██
    private protected    aynı assembly ██ VE ██ türeyenler — KESİŞİM

        ██ ALTISININ ORTAK KÖRLÜĞÜ ██
        Altısı da bir ÜYEYE bakar, o üyenin döndürdüğü NESNEYE
        değil. `private` bir alan, `public` bir metottan geri
        verilebilir; duvar o anda çoktan aşılmıştır. Aynı körlük
        `readonly`de de var: dil/01, "kelime OKA bakar, ucuna değil".
```

### Kutunun GERÇEK SATIRLAR tarafındaki karşılığı

██ Altısı da ÖDÜNÇ, yani aşağıda gösterilen yer **tanım yeri değil KULLANIM
YERİDİR**. ██

**`internal` bu projede** — `Assets/Game/Battle/Battle.cs` → `Board`

```csharp
internal UnitGrid Board => board;
```

██ EN ÖĞRETİCİ SEÇİMİ ██ — kutu altı belirteç taşıyor ama kaynağa bağlanacak satır
tek: `Assets/Game/` altında `internal` bir **belirteç olarak** yalnız burada
geçiyor (ölçüldü; kalan iki eşleşme aynı üyenin üstündeki yorum satırları).
`public` ile `private` onlarca satırda, dolayısıyla ayırt edici değil;
`protected`, `protected internal` ve `private protected` ise kutunun kendi
söylediği gibi sıfır kez. Kutudaki «Duvar: ██ .asmdef ██ — klasör değil, ad alanı
değil» satırının karşılığı tam bu satır ve iddia bu depoda ölçülebilir:
`BattleActions` bu tahtayı GERÇEKTEN kullanıyor —
`MoveAction.Execute(battle.Board, unit, fromX, fromY, toX, toY, profile);` —
çünkü `Assets/Game/Battle/GridStrategy.Battle.asmdef` içinde; `BoardAdapter` ise
`.Board`'u hiç anmıyor çünkü `Assets/Game/Unity/GridStrategy.Unity.asmdef` içinde —
oysa ikisi de aynı `Assets/Game/` ağacında duruyor. ██ Duvarı çizen şey klasör
olsaydı ikisi de görürdü. ██ Ve kutunun «Referans vermek yetmez» uyarısının en
temiz kanıtı da burada: o `.asmdef`in `references` listesinde
`GridStrategy.Battle` **yazılı**, yani `BoardAdapter` `Battle` tipini görüyor —
`Board` üyesini yine de göremiyor. Kutunun «BİLMEZ: test assembly'sini» satırının
faturası da burada: `Board`'u doğrudan sınayan bir EditMode testi yazılamaz, çünkü
`InternalsVisibleTo` bu depoda yok.

Kutunun alt bölümündeki «██ ALTISININ ORTAK KÖRLÜĞÜ ██» satırı da aynı satırda
görünür: `internal` olan şey **üye**, döndürdüğü `UnitGrid` nesnesi değil. Onu bir
kez alan `BattleActions`'ın elinde artık sıradan bir referans var ve duvar o anda
çoktan aşılmıştır.

---

## Birinci durak: sınır nerede biter

### Yazılmadığında ne olur

Erişim belirteci **isteğe bağlıdır** ve yazılmadığında derleyicinin verdiği
cevap bulunduğun yere göre değişir. Ezberlenmesi gereken tek tablo bu:

```
  bildirim yeri                          yazılmazsa
  ──────────────────────────────────────────────────────────────
  ad alanı içinde bir TİP                internal
  bir tipin ÜYESİ (alan, metot, prop.)   private
  bir interface'in üyesi                 public (başka türlü olamaz)
  bir enum'un değeri                     public (başka türlü olamaz)
```

██ Bu projede o varsayılanların hiçbirine güvenilmiyor. ██ 34 tipin 34'ü de
`public` yazıyor — üstveri de bunu doğruluyor (yukarıdaki ikinci ölçüm:
`Public` 34, `NotPublic` 0). Yani "yazmadım, `internal` olur" diye bir satır
yok; her tip kararını açıkça söylüyor.

### Altı belirteç, altı duvar, ve bu projedeki sayıları

| belirteç | duvar NEREDE biter | `Assets/Game` |
|---|---|---|
| `public` | hiçbir yerde — referans veren her assembly görür | **156** |
| `private` | içeren tipin gövdesi | **98** |
| `internal` | ██ assembly ██ (`.asmdef` sınırı) | **1** |
| `protected` | tip ağacı — türeyenler | **0** |
| `protected internal` | assembly ∪ türeyenler (daha geniş) | **0** |
| `private protected` | assembly ∩ türeyenler (daha dar) | **0** |

Tablonun en öğretici satırı dördüncüsü. `protected` sıfır kez geçiyor ve sebebi
bir üslup kararı değil: **bu projede kalıtım satırı toplam iki** ve ikisi de
motorun zorunlu kıldığı satır (dördüncü durak). Türeyen yoksa `protected`
`private` ile aynı odayı açar — yani hiçbir şey söylemez.

██ "Bugün yok" eksik bir cümledir. ██ `protected`'in bu projeye gireceği gün
tektir: gerçek bir alt tip ailesi doğduğu ve o ailenin **ortak ama dışarı
kapalı** bir durumu paylaştığı gün. Bugün böyle bir aile yok; nedeni üçüncü ve
dördüncü duraklarda ölçüyle yazılı.

### Assembly başına dağılım — ve `GridStrategy.Unity`'nin tersliği

Aynı sayım assembly başına yapıldığında bu projenin en anlamlı erişim olgusu
ortaya çıkıyor:

```
  assembly                public   private        şekli
  ──────────────────────────────────────────────────────────────
  GridStrategy.Core          31        7      açık kütüphane
  GridStrategy.Combat        82       18      açık kütüphane
  GridStrategy.Battle        39       11      açık kütüphane + 1 internal
  GridStrategy.Unity          4       62      ██ TERS ██
  ──────────────────────────────────────────────────────────────
  toplam                    156       98
```

`GridStrategy.Unity`'deki dört `public`in tamamı burada:

```
BoardAdapter.cs:110
    public sealed class BoardAdapter : MonoBehaviour

UnitView.cs:43
    public sealed class UnitView : MonoBehaviour

UnitView.cs:142
    public void SetSelected(bool isSelected)
```

Dördüncüsü `UnitView.cs`'in 173. satırındaki `SetState`. ██ `BoardAdapter`'ın
tek bir `public` ÜYESİ yok. ██ Yalnız sınıfın kendisi `public` — o da motorun
bileşeni tanıyabilmesi için.

Okunuşu tek cümle: **`GridStrategy.Unity` bir yaprak.** Onu çağıran başka bir
assembly yok, dolayısıyla söz verecek kimsesi de yok. Aşağıdaki üç assembly ise
kütüphane: yüzeyleri geniş, çünkü hepsinin bir çağıranı var. `UnitView`'ün iki
`public` metodu bu kuralın kanıtı: onları çağıran `BoardAdapter` aynı
assembly'de, ama bu bir karar değil bir tesadüf ve `internal` yazmak
taşınabilirliği bugünkü klasör düzenine bağlamak olurdu.

### ██ AYRIŞMA NOKTASI: `internal`in duvarı ASSEMBLY'dir ██

Bu, dosyanın en pahalı yanlış okuma adayı. `internal`i "aynı klasör" ya da
"aynı ad alanı" diye okuyan biri **bu projede yanılır** ve yanıldığını derleyici
söyleyene kadar fark etmez.

Karşı örnek uydurulmuş değil, diskte duruyor:

```
  Assets/Game/Core/              ← GridStrategy.Core.asmdef BURADA
  │   Unit.cs · UnitGrid.cs · GridDistance.cs · MoveAction.cs · ...
  │
  └── Combat/                    ← GridStrategy.Combat.asmdef BURADA
          Combatant.cs · Health.cs · AttackAction.cs · ...

  KLASÖR olarak  : Combat, Core'un İÇİNDE
  ASSEMBLY olarak: Combat, Core'un KARDEŞİ — references: [], iki yönde de

  ██ Core'a yazılacak bir `internal` üyeyi Core/Combat/ İÇİNDEKİ
     hiçbir dosya göremez. ██ Klasör iç içe, assembly ayrı.
     Ve tersi de doğru: Combat'ın `internal`ini Core göremez.
```

Tersi de aynı depoda: `Battle.cs` ile `BattleActions.cs` aynı klasörde ve aynı
assembly'de — ama `internal`i çalıştıran şey klasör değil, o klasördeki
`.asmdef`. Dosyalar iki alt klasöre bölünüp aralarına ikinci bir `.asmdef`
girseydi aynı satır derlenmezdi.

Klasör, ad alanı ve assembly üçlüsünün hangisinin neyi kontrol ettiği —
[`konular/02`](../konular/02-assembly-duvari.md#uc-ayri-sey-uc-ayri-is)'de.
Burada tekrar edilmiyor; eklenen tek şey `internal`in **o duvarın üstüne
oturduğu**.

### Duvarın bir kapısı var, ve bu projede kullanılmıyor

`InternalsVisibleTo` bir assembly düzeyi niteliğidir ve adı verilen ikinci bir
assembly'ye `internal` üyeleri açar — genellikle test assembly'sine.

```
  ölçüldü:  grep -rn "InternalsVisibleTo" Assets/   →   SIFIR eşleşme
```

Sonucu somut: `GridStrategy.Battle.EditModeTests`, `references` dizisinde
`GridStrategy.Battle`'ı taşıyor — yani `Battle` sınıfını görüyor, ama `Board`
üyesini **göremiyor**. Referans vermek `internal`i açmaz.

██ `HENÜZ YOK → sahipsiz` ██ — bu niteliğin tam mekanizması (imzalı
assembly'lerde açık anahtar zorunluluğu, Unity'de hangi dosyaya yazıldığı) bu
ağaçta anlatılmıyor. Onu yazacak an: bir testin `internal` bir üyeye erişmesi
gerektiği ilk gün.

---

## İkinci durak: `internal UnitGrid Board` — işli örnek

Projenin tek `internal`i. Bu durak dosyanın merkezi, çünkü burada üç şey aynı
anda okunuyor: bir erişim kararı, o kararın hangi somut ihtiyaçtan doğduğu, ve
garantinin nerede bittiği.

### Kod, olduğu gibi

```
Battle.cs:53
        private readonly UnitGrid board;

Battle.cs:107
        internal UnitGrid Board => board;
```

Alan `private`, özellik `internal`. Aralarındaki mesafe tesadüf değil — arada
kurucu duruyor, çünkü tahta **kurucuda doğuyor**.

### Tek çağıran — sayıldı

`Board` üyesinin bütün depoda kaç kullanıcısı var? Soru tahminle değil
`grep -rn "\.Board\b" Assets/ --include=*.cs` ile cevaplandı ve tek satır döndü:

```
BattleActions.cs:207
                MoveAction.Execute(battle.Board, unit, fromX, fromY, toX, toY, profile);
```

██ Bir üye, bir çağıran. ██ Ve o çağıran bir konfor değil bir zorunluluk:
`MoveAction.Execute` imzasında bir `UnitGrid` istiyor, `MoveAction` ise
`GridStrategy.Core`'da yaşıyor ve savaşı tanımıyor. Tahtayı ona uzatabilecek tek
tip `Battle`; uzatmasa hareket hiç çözülemez.

### İki tarafın assembly'si — `.asmdef` okundu

`internal`in çalışıp çalışmadığı tek soruya bakar: **iki dosya aynı assembly'de
mi?** Cevap dosya sisteminde:

```
  Assets/Game/Battle/
  ├── GridStrategy.Battle.asmdef   ← klasörün assembly'sini BU belirler
  ├── Battle.cs · BattleActions.cs · TurnRules.cs · TurnState.cs
  └── PlacementOutcome.cs · ReviveOutcome.cs

  ██ Battle ve BattleActions AYNI assembly'de → `internal` ÇALIŞIYOR. ██
```

Farklı olsaydı ne olurdu, ölçülebilir: `BoardAdapter` `GridStrategy.Unity`'de
yaşıyor ve `references` dizisinde `GridStrategy.Battle` var — yani `Battle`
tipini görüyor. Buna rağmen `battle.Board` yazamaz; o üye onun için **yok**.
`internal` bugün gerçekten bir duvar örüyor, süs değil. Duvarın planı
[`konular/03`](../konular/03-tahta-sahipligi.md#dorduncu-durak-garantinin-bittigi-cizgi)'te
çizili.

### Neden `public` değil, neden `private` değil

`public` olmamasının cevabı bu dosyaya ait değil ve burada tekrar edilmiyor:
"ikinci yazar" probleminin tamamı —tahtaya ikinci bir okun doğması, o oktan
yazan her satırın sözlükleri atlaması, ekranda duran ama savaşta olmayan bir
asker—
[`konular/03`](../konular/03-tahta-sahipligi.md#birinci-durak-referans-vermek-kopyalamaz)'te
hikâye olarak yazılı. Buraya ait olan tek cümle: **`internal` o kararı UYGULAYAN
mekanizmadır** — kararın bir kısmını derleyiciye söyletir, tamamını değil.

`private` olmamasının sebebi ise tek satır: `BattleActions` **ayrı bir tiptir**
ve `private` sınırı tiptir — dosya ya da klasör değil. İkisi aynı klasörde,
aynı ad alanında, aynı assembly'de; `private` bunların hiçbirine bakmaz.

Üçüncü bir yol vardı: `Battle`'a `MoveUnit(from, to)` diye bir üye eklemek ve
tahtayı hiç dışarı vermemek. Seçilmedi, çünkü hareketin kuralı `MoveAction`'da
yaşıyor ve o kuralı `Battle`'a kopyalamak, `Battle`'ı bir oyun kuralı sahibine
çevirirdi — sınıfın kendi özetinin açıkça reddettiği şey.

### **REDDEDILEN** — `internal` yerine:

```csharp
public UnitGrid Board => board;
```

**KIRILAN:** Üye o gün `GridStrategy.Unity`'den de görünür hâle gelir ve
`BoardAdapter` tek satırda tahtaya yazabilir:
`battle.Board.PlaceUnit(x, y, unit);` — alan tutmasına bile gerek yok. O satırla
birlikte tahtada duran ama `Battle.combatants` sözlüğünde karşılığı olmayan bir
`Unit` doğar; ekranda görünür, tıklanabilir, hedeflenebilir, ve ona saldırıldığı
anda "The unit is not in this battle." diye patlar. Derleyici bu ayrışmayı
gösteremez, testler yeşil kalır. Kaybedilen şey tek bir satır değil,
**derleyicinin tuttuğu tek garanti**.

**KAZANIRDI:** Tahta gerçekten bir okuma yüzeyi olsaydı — yani `Board`
değiştirilemez bir görünüm döndürseydi. O gün "kim yazabilir" sorusu tipin kendi
yazılışıyla cevaplanmış olurdu ve `public` bir şey kırmazdı. Bugün öyle değil:
`UnitGrid`'in üç yazma metodu (`PlaceUnit`, `MoveUnit`, `RemoveUnit`) `public`
ve onları eline geçiren herkes tahtayı değiştirebilir.

### Garantinin bittiği yer

`internal` bir duvar örüyor ama duvarın **iç tarafında** hiçbir şey söylemiyor:

```
  GridStrategy.Unity          ║        GridStrategy.Battle
  ─────────────────────       ║        ────────────────────
  BoardAdapter                ║        Battle       ── tahtanın sahibi
    battle.Board  ✗ DERLEMEZ  ║          internal Board
                              ║        BattleActions
         ██ DERLEYİCİ ██      ║          battle.Board  ✓ görüyor
                              ║          ██ SÖZ BURADAN SONRA
                              ║             DİSİPLİNE DAYANIYOR ██
```

Yani `internal` **çağıran kümesini bire indirdi**, sıfıra değil. `BattleActions`
içinde "tahtaya yalnız `Battle` yazar" sözünü tutan şey bir derleyici kuralı
değil, o dosyanın kendi disiplini. Bu bilerek ve bedava olmadan kabul edildi:
karşılığında `MoveAction`'ın imzası bozulmadan kaldı. Tavizin test tarafındaki
yüzü de ölçülebilir — `internal` üyeler test assembly'sinden de görünmediği
için `Board`'u **doğrudan sınayan bir test yazılamaz**; sınanan şey her zaman
`BattleActions.Move`'un sonucu.

---

## Üçüncü durak: `interface` — arkada ne var, ve neden bu projede SIFIR

### Bir arayüz nedir: derleme zamanında görünen üye kümesi

Bir `interface` bir **sözleşmedir**: "bu referansı tutan şu üyeleri çağırabilir"
diyen bir liste. Bir tip o listeyi uyguladığını beyan eder, derleyici de
uyguladığını doğrular.

Kritik olan, sözleşmenin **ne zaman** iş gördüğü: derleme zamanında. Bir arayüz
tipli referans üzerinden yalnız sözleşmedeki üyeler çağrılabilir; nesnenin
geri kalanı **görünmez olur, yok olmaz**.

```
                 ██ TEK NESNE, İKİ FARKLI PENCERE ██

  Combatant somut referansı ─┐
    .TakeDamage()            │      ╔═══════════════════════════╗
    .TryRevive()             ├─────►║   BELLEKTEKİ TEK NESNE    ║
    .CurrentHealth           │      ║   health · lifecycle      ║
    .State  .Team            │      ║   attackProfile · Team    ║
                             │      ║   lastObservedState       ║
  IDamageable arayüz ────────┘      ╚═══════════════════════════╝
  referansı (varsayımsal)                        ▲
    .TakeDamage()                     ██ AYRIŞMA NOKTASI ██
    ██ ötekiler GÖRÜNMÜYOR ██        Görünmüyor ≠ yok. Nesne aynı
                                     nesne, alanları aynı alanlar,
                                     boyutu aynı boyut.
```

### ██ Bir arayüzün YAPMADIĞI dört şey ██

Bunlar tahmin değil, dilin tanımı:

```
  ① İKİNCİ BİR NESNE YARATMAZ
     IDamageable d = combatant;   → ReferenceEquals(d, combatant) TRUE.
     Tek nesne, ikinci bir ok. Kimlik tarafının tamamı: dil/05.

  ② SOMUT ÜYELERİ KALDIRMAZ
     `d` üzerinden TryRevive çağrılamaz — ama nesnede DURUYOR.
     ((Combatant)d).TryRevive() aynı anda derlenir ve çalışır.

  ③ ÖRNEĞİ KÜÇÜLTMEZ
     Nesnenin alan kümesi tipinin yazılışına bağlıdır, ona hangi
     referansla bakıldığına değil. Arayüz eklemek bir alanı silmez.

  ④ BELLEĞİ VE PERFORMANSI OTOMATİK İYİLEŞTİRMEZ
     ██ Bu, en sık duyulan yanlış modeldir ve tersi bile mümkündür:
        arayüz çağrısı somut çağrıdan FARKLI çözülür. ██
```

Değer ve referans tiplerinin, "aynı olmak"ın ve `ReferenceEquals`'in tam
anlatımı [`dil/05`](05-deger-referans-ve-kimlik.md#ikinci-durak-referenceequals-neden-degil)'te.
Buraya ait olan tek cümle: **arayüz tipli bir referans yeni bir kimlik
üretmez.**

### Arka taraf — ve ölçülemeyen kısım

Somut bir sınıfın metodu çağrıldığında hedef, derleme zamanında bilinen tipin
üzerinden çözülür. Bir arayüz üyesi çağrıldığında ise çalışma zamanı, nesnenin
**gerçek tipinden** o arayüzün üye tablosunu bulmak zorundadır — mekanizmanın
adı arayüz gönderim tablosudur. İki çağrı aynı işi yapmıyor.

```
  ██ ÖLÇÜLMEDİ ██
  Bu farkın bu projedeki NANOSANİYE ya da BAYT karşılığı ölçülmedi ve
  bugün ÖLÇÜLEMEZ: dört üretim assembly'sinde interface tanımı 0,
  interface uygulaması 0 (üstveriden okundu) — karşılaştırılacak ikinci
  taraf yok. "Arayüz maliyetlidir" bu belgede bir ÖLÇÜ değil bir
  etikettir, ve etiket yazılmaz.
```

### ██ NE ZAMAN GEREKİR ██

Ölçüt tek cümle, ve `unity-expert-code-quality` skill'inin *Ownership Map*
tablosundan geliyor:

> | Pressure | Smallest candidate | Reject when |
> |---|---|---|
> | one caller needs several independent implementations | narrow interface | entity count alone is the only justification |

```
  GEREKİR   : bir ÇAĞIRAN, aynı yetenek sözleşmesi arkasında GERÇEKTEN
              birden fazla uygulamaya ihtiyaç duyuyorsa
  GEREKMEZ  : ██ varlık SAYISI tek başına gerekçe DEĞİLDİR ██
              "üç tip var, o hâlde bir arayüz olmalı" bir ölçü değil
```

Aynı kaynak tuzağın adını da koyuyor: *"`interface` is not more enterprise than
a concrete class."* Bu bir kalite merdiveni değil, bir uygunluk kararı.

### Bu projede böyle bir çağıran VAR MI — arandı

Aranan şey netti: aynı çağrının iki farklı uygulamayı çalıştırdığı bir yer.
Bulundu, ve tam olarak bir tane:

```
BattleActions.cs:127-129
            AttackOutcome outcome = targetIsStructure
                ? AttackAction.Execute(attackerCombatant, targetStructure, distance)
                : AttackAction.Execute(attackerCombatant, targetCombatant, distance);
```

Bir çağıran, tek bir yetenek ("bu şeye saldırılır"), iki farklı uygulama
(`Combatant` hedef, `Structure` hedef). ██ Bu, kitaptaki arayüz basıncının
şekli. ██ Aynı ikilik üç yerde daha görünüyor: `Battle.Tick` iki döngü
çeviriyor, `Battle.RemoveReadyForCleanup` iki döngü tarıyor,
`Battle.RemoveUnit` iki sözlüğe birden bakıyor.

Ve buna rağmen arayüz yazılmadı. Gerekçe uydurulmuş değil — **kodun kendisinde
duruyor**, `AttackAction`'ın ikinci aşırı yüklemesinin üstünde:

```
AttackAction.cs:121
        // İKİ AŞIRI YÜKLEME, TEK AKIŞ ŞEKLİ: ortak bir IAttackTarget arkasında tek
```

Bloğun tamamı üç şey söylüyor ve üçü de sözleşmenin **içeriğiyle** ilgili,
maliyetle değil:

```
  ① Hedef uygunluğu kuralı TargetingRules'tan HEDEFİN İÇİNE taşınırdı
     — yani saf kural sınıfı deseni tam o noktada bozulurdu
  ② Combatant ile Structure ikisi de o kuralı TANIMAK zorunda kalırdı
     — bugün ikisi de kuralı tanımıyor, kural onları tanıyor
  ③ Arayüzün bool'u Downed ile Destroyed'ı AYNI cevabın arkasına düşürürdü
     — AttackOutcome.HitAndDowned / HitAndDestroyed ayrımı silinirdi

  ██ "Soyutlamanın bugün sildiği tek şey iki metot." ██
     Kazanç iki metot, bedel üç karar. Ölçü bu.
```

Aynı ret, iki tipin şekli tarafından da destekleniyor: `Combatant.TakeDamage`
`void` döndürüyor, `Structure.TakeDamage` ise `bool` ("bu vuruş yıktı mı").
Ortak bir arayüz o iki imzadan **birini seçmek** zorunda kalırdı ve seçilmeyen
taraf bilgisini kaybederdi.

### En yakın alternatifler — hangisi hangi koşulda kazanır

| Aday | Kazandığı koşul | Bu projedeki durumu |
|---|---|---|
| **somut / `sealed` sınıf** | tek kararlı uygulama var, gerçek bir yerine geçme baskısı kanıtlanmadı | ██ SEÇİLEN ██ — 14 `sealed class` |
| **`abstract` taban** | gerçek bir alt tip AİLESİ ortak değişmez durum/davranış paylaşıyor | reddedildi — `Structure.cs:17` bloğu gerekçeyi yazıyor |
| **bileşim (composition)** | yetenekler birbirinden BAĞIMSIZ değişiyor | ██ SEÇİLEN ██ — `Combatant` ve `Structure` |
| **test sahtesi (fake)** | parçalardan biri ağ/dosya/rastgelelik taşıyor ya da kurulumu yavaş | reddedildi — `CombatantTests.cs:18` bloğu gerekçeyi yazıyor |

**Bileşim** bu projenin asıl cevabı: `Combatant` yeteneklerini kalıtımla
devralmıyor, parçaları **alan olarak tutarak** kazanıyor —
`Health` + `UnitLifecycle` (+ `AttackProfile` + `Team`). `Structure` aynı şekli
farklı parçalarla kuruyor: `Health` + `StructureLifecycle`. İkisinin ortak olan
tek parçası `Health`, ve bu tesadüf değil — `Structure`'ın varlığıyla sınanan
iddia bu: can kuralı tipten bağımsızsa, barakanın canı askerin canıyla aynı
sınıfla tutulabilmelidir. Kimlik ise üçüncü bir katman: `Unit` ne birinin ne
ötekinin içinde — `Battle`'ın sözlüklerinde. Desen adları ve tam anlatım
[`ogrenme/01`](../../ogrenme/01-koda-gomulu-desenler.md#5-bilesim-composition-over-inheritance)'de.

**Test sahtesi** de arandı ve o da reddedilmiş — reddin gerekçesi bir testin
içinde yazılı:

```
CombatantTests.cs:18
            //     return new Combatant(new FakeHealth(), new FakeLifecycle(), profile);
```

Bloğun kilit cümlesi tam olarak bu dosyanın konusu: sahte parça yazmak için önce
bir arayüz gerekir, çünkü iki tip de `sealed`. Ve blok kazancı da yazıyor:
sahteleme "yalıtacak bir şey varsa" kazanç; saf ve hızlı parçalarda yalnızca
sınanan kuralı siler.

### ██ K43: bugün gerekmiyor — peki hangi gün gerekir ██

"Bugün yok" eksik bir cümledir. Arayüzü doğuracak somut olaylar, en olasıdan
en az olasıya, ve her biri koda bakılarak seçildi:

```
  ① İKİNCİ BİR HEDEF TÜRÜ
     Bugün "saldırılabilir" iki şey var ve BattleActions.Attack ikisi
     arasında dallanıyor. ÜÇÜNCÜSÜ eklendiği gün AttackAction'a ÜÇÜNCÜ
     bir aşırı yükleme yazılır. ██ Eşik burada: kazanç "iki metot"
     olmaktan çıkıp "N metot ve N dallı bir ifade" olur. ██

  ② KURULUMU PAHALI BİR PARÇA
     CombatantTests'in KAZANIRDI satırı bunu ADIYLA söylüyor: bir parça
     ağ, dosya ya da rastgelelik taşıdığı gün sahte parça ŞART olur — ve
     sahte parça için önce bir arayüz gerekir, çünkü Health ve
     UnitLifecycle `sealed`.

  ③ İKİNCİ BİR MENZİL ÖLÇÜSÜ
     Bugün menzil kuralının tek metni var (AttackResolver). Yükseklik,
     engel ya da menzil tipi (düz / yay) girdiği gün aynı çağıranın iki
     ölçüye ihtiyacı olur — "aynı çağıran, iki uygulama" ölçütü.

  ④ KAYIT / YÜKLEME SAĞLAYICISI
     konular/03'ün kaçış yolu bir BattleSnapshot'tan söz ediyor. Kaynağı
     dosya mı, bulut mu, test tamponu mu sorusu doğduğu gün çağıran tek
     bir yetenek arkasında birden fazla uygulama görür. Bugün böyle bir
     çağıran YOK — snapshot tipi de yok.
```

Dördünün ortak şekli: **arayüz varlık sayısından değil, ÇAĞIRANIN ihtiyacından
doğar.** Bugün hiçbir çağıranın böyle bir ihtiyacı yok ve bu ölçüldü,
varsayılmadı.

---

## Dördüncü durak: `abstract` · `virtual` · `override` — ve `sealed`

Dördü aynı ailenin üyesi: ilk üçü kalıtımı **açar**, dördüncüsü **kapatır**.
`sealed` bu dosyanın konusu değil — ne yaptığı ve neyi vaat etmediği
[`dil/01`](01-degismezlik-anahtar-kelimeleri.md#dorduncu-durak-sealed-tip-agacini-keser-nesne-grafigini-kesmez)'de
yazılı. Buraya ait olan tek satır: bu projede 14 `sealed class` var, yani
kalıtım kapısı **bilerek ve neredeyse her yerde** kapalı.

### Kalıtım satırı: toplam iki, ikisi de zorunlu

`grep -rnE "class\s+\w+\s*:" Assets/Game --include=*.cs` iki satır döndürüyor:

```
BoardAdapter.cs:110
    public sealed class BoardAdapter : MonoBehaviour

UnitView.cs:43
    public sealed class UnitView : MonoBehaviour
```

██ Başka hiçbir tip hiçbir şeyden türemiyor. ██ Ve türeyen bu ikisi bile
**mühürlü** — yön burada okunur: `sealed` bir tipin altını keser, üstünü değil.

Sonucu doğrudan: `abstract`, `virtual`, `override` için **proje içinden örnek
YOK.** Bu dosya uydurma örnek yazmıyor. Üstveri de aynı şeyi söylüyor — dört
üretim DLL'inde `Virtual` damgası taşıyan tek bir metot yok.

### `virtual` / `override`in arka tarafı

Tek cümle: **sanal bir çağrı, çağıranın gördüğü tipi değil nesnenin GERÇEK
tipini seçer.**

```
  Taban t = new Turemis();   t.Calis();

  `Calis` VIRTUAL DEĞİLSE  → Taban.Calis çalışır   (derleyici seçti)
  `Calis` VIRTUAL İSE      → Turemis.Calis çalışır (çalışma zamanı seçti)
                             ██ AYRIŞMA NOKTASI ██
                    Aynı satır, aynı değişken, iki farklı gövde.
                    Seçimi yapan şey `t`nin TİPİ değil, İÇİNDEKİ nesne.
```

### ██ `Awake` ve `Update` BUNLAR DEĞİLDİR ██

En yaygın yanlış model burada:

```
  MonoBehaviour'un Awake / Start / Update / OnEnable / OnDisable
  ──────────────────────────────────────────────────────────────
  virtual DEĞİL  →  override yazamazsın; yazarsan derlenmez
  event   DEĞİL  →  += ile abone olunmaz
  arayüz  DEĞİL  →  hiçbir sözleşmede tanımlı değil

  ██ Onlar AD TABANLI mesaj geri çağrılarıdır: motor tipi tarar,
     o adı taşıyan metodu bulur ve çağırır. Sözleşme derleyicide
     değil, MOTORUN kendisinde yaşıyor. ██
```

Ölçülmüş kanıtı ve bütün sonuçları
[`konular/08`](../konular/08-motor-cagri-dongusu.md#birinci-durak-awake-bir-event-degildir)'de.
Buraya eklenen tek şey **erişim yüzü**: bu projede motor geri çağrılarının hepsi
`private` yazılmış ve bu mümkün, çünkü onları çağıran şey C# erişim kurallarına
tabi değil.

### `abstract` ne zaman, ne zaman değil

Ölçüt yine *Ownership Map* tablosundan:

> | Pressure | Smallest candidate | Reject when |
> |---|---|---|
> | genuine subtype family shares invariant state/behavior | abstract base | inheritance exists only to reuse a few lines |

Yani: gerçek bir alt tip AİLESİ ortak bir DEĞİŞMEZ durumu/davranışı paylaşıyorsa
ve her alt tip taban sözleşmesini koruyorsa gerekir; ██ kalıtım yalnızca birkaç
satırı tekrar kullanmak içinse gerekmez ██. Bu projede sınav bir kez yapıldı ve
**kaybedildi** — gerekçe `Structure`'ın başında duruyor:

```
Structure.cs:17
    // KALITIM AYNI PARÇALAR DEĞİL, AYNI YAŞAM DÖNGÜSÜ DEMEKTİR. `: Combatant`
```

Bloğun ölçüsü net: baraka, `Combatant`'tan devralacağı üyelerin yarısına uymaz
— `TryRevive`, `Downed` hâli, zorunlu `AttackProfile`, kurtarma penceresi. Ve
son cümle bu dosyanın konusuna dokunuyor: **`sealed` bu satıra karşı sıfır
koruma sağlar**, çünkü tartışılan şey kalıtımın yasaklanması değil,
**seçilmemesi**.

---

## Beşinci durak: `static class` — 12 tane, ve `interface`in yerine GEÇMİYOR

### On iki tip, sayıldı

```
  GridStrategy.Battle   BattleActions · TurnRules                       2
  GridStrategy.Combat   AttackAction · AttackResolver · AttackRules
                        DamageRules · HealingRules · MovementRules
                        ReviveRules · TargetingRules                    8
  GridStrategy.Core     GridDistance · MoveAction                       2
                                                                     ────
                                                                       12
```

██ Bunlar `interface`in yerine geçen şey DEĞİL. ██ Başka bir sorunun cevabı:
"durum taşımayan saf kural nereye yazılır". Desenin adı **saf kural sınıfı** ve
tam anlatımı — hangi basınçtan doğduğu, dokuz tipin ortak şekli, SOLID
karşılığı —
[`ogrenme/01`](../../ogrenme/01-koda-gomulu-desenler.md#1-saf-kural-sinifi-stateless-policy-class)'de.
Burada tekrar edilmiyor.

### `static class` derleyicide ne olur

Ölçüldü, üstveriden okundu: **`static class` derlendiğinde `abstract` ve
`sealed` damgalarının İKİSİNİ birden taşır.** On iki tipin on ikisi de öyle.

```
  abstract  →  örnek alınamaz     (new AttackRules() ✗)
  sealed    →  türetilemez        (: AttackRules    ✗)

  ██ Bu yüzden `static sealed class` yazmak derleme hatasıdır (CS0441):
     `sealed` zaten orada; iki kez söylemeye çalışıyorsun. ██
```

### `static class` ne YAPAMAZ — ve dördüncüsü asıl mesele

```
  ① örnek alınamaz            → new ile kurulamaz
  ② kalıtılamaz               → ne türer ne türetilir
  ③ arayüz uygulayamaz        → : IFoo yazılamaz
  ④ ██ PARAMETRE OLARAK GEÇİRİLEMEZ ██
        void Kur(??? kural)   → yazılacak bir tip YOK.
        `AttackRules` bir DEĞER değil, yalnız bir ad.
```

Dördüncüsü bir kısıtlama maddesi değil, **değişim maliyetinin kendisi**: bir
kuralı DEĞİŞTİRİLEBİLİR kılmak istediğin gün önce bir DEĞER gerekir — ya bir
delege (`Func<UnitState, bool>`) ya bir arayüz. İkisi de `static`in yapamadığı
şey. Yani `static` bugünkü kararı ucuzlatıyor, yarınki değişimi
pahalılaştırıyor. ██ Bu bir kusur değil bir TAKAS ve ölçüsü şu: bugün hiçbir
kuralın ikinci bir uygulaması yok. ██ Takas doğru tarafta duruyor — ama nerede
durduğu yazılı olmazsa, ilk değişim isteği geldiği gün kimse sebebi hatırlamaz.

Delegeyi bir alanda tutmanın ne getirip ne götürdüğü ayrı bir dosyanın işi:
[`dil/04`](04-delege-olay-ve-kapanis.md) ve
[`dil/06`](06-delege-arka-taraf.md).

---

## Üç oyun: "aynı işe farklı cevap veren şeyler nasıl ifade edilir"

> ██ DOĞRULAMA SINIRI: üç oyunun da kaynağı KAPALIDIR. ██ Aşağıdaki üç hücrenin
> hiçbiri kaynak koda ya da resmî belgeye karşı **doğrulanmadı**; hepsi
> *oyuncunun gördüğü* olgular. Mekanizma adı bilerek yazılmıyor — bu tabloda
> yalnız **ad** ve **iş** var.

| Oyun | Aynı basıncı taşıyan şeyin ADI ve İŞİ |
|---|---|
| **Slay the Spire** | ██ EŞLEŞMEYEN ██ Kartlar. Tek bir "oyna" eylemi, karta göre bambaşka bir iş: hasar, blok, çekiliş, güç, dönüştürme. Oyuncu hep aynı hareketi yapar; ne olacağını elindeki kart taşır. |
| **Vampire Survivors** | Silahlar. Hepsi kendiliğinden ateşlenir ve hepsi aynı ana ("sayaç doldu") cevap verir — ama biri halka çizer, biri kırbaç savurur, biri hedefe yönelir. |
| **Stardew Valley** | Aletler. Aynı tıklama, aynı karo: balta ağaç keser, kazma taş kırar, kova su alır, orak biçer. Cevabı belirleyen şey hedef değil, elde tutulan alet. |

### ██ EŞLEŞMEYEN SATIR: Slay the Spire ██

En öğretici satır bu, çünkü bizim şeklimiz oraya **oturmuyor** — ve ayıran şey
mekanizma değil, iki sayı:

```
  BİZDE                                 ORADA
  ────────────────────────────          ──────────────────────────────
  "saldırılabilir" iki şey var          yüzlerce kart, ve sayı
  küme KAPALI: üçüncüsü ancak yeni      İÇERİKLE büyür; küme AÇIK
  bir tip YAZILARAK doğar               yeni davranış yeni derleme
  seçim DERLEME zamanında               istemez, seçim ÇALIŞMA
  (aşırı yükleme çözümlemesi)           zamanında ve liste VERİDEN
  ölçü: tek bir ikili koşul, iki dal    ██ o ölçekte "her tip için bir
                                           dal" şekli hiç yazılamaz ██
```

Asimetrinin adı: bizde davranış sayısı **tip sayısıyla** büyüyor ve tip sayısı
iki; orada **içerikle** büyüyor. Şekli seçen şey soyutlamanın güzelliği değil,
**N ve kümenin açık olup olmadığı**. Stardew'in aletleri bize en yakın satır —
orada da küme küçük ve geliştirici tarafından kapatılmış. Vampire Survivors
ortada: küme açık ama davranışlar birbirinden bağımsız, yani bizim **bileşim**
cevabımıza benziyor.

---

## Tek bakışta: bir üye dışarıya ne söz veriyor

```
                        YAZDIĞIN ÜYE
                             │
        ┌────────────────────┼────────────────────┐
        ▼                    ▼                    ▼
   ① KİM GÖRÜR          ② KİM YAZAR          ③ NEYE BAĞLANIR
   erişim belirteci     get-only / set       somut tip / sözleşme

   public   156         { get; }             somut sınıf  ██ 34 ██
   private   98         private set          arayüz       ██  0 ██
   internal   1  ◄──    düz alan             abstract taban██ 0 ██
   protected  0
        │                    │                    │
   duvar: assembly      sahibi: dil/01       sahibi: bu dosya
        │
   ██ TEK ÖRNEK: Battle.Board — garantisi assembly duvarında BİTER ██

  ██ Üçü BAĞIMSIZ ve hiçbiri ötekinin işini yapmaz: ██
     public + get-only bir nesneyi dondurmaz (dil/01)
     private bir alan public bir metottan geri verilebilir
     arayüz görünürlüğü daraltır ama nesneyi küçültmez
```

---

## Kural: iki karar ağacı

### A — bu üyeye hangi erişimi vereceksin

```
① Bu üyeyi tipin DIŞINDAN çağıran var mı — BUGÜN, gerçekten?
      HAYIR → private. Konu kapandı. (98 üyenin cevabı bu)
      EVET  → ②

② Çağıran aynı ASSEMBLY'de mi? (soruyu klasöre değil .asmdef'e sor)
      EVET  → ██ internal ██ — ve garantinin duvarda bittiğini YAZ
      HAYIR → ③

③ Çağıran bu tipten TÜREYEN bir tip mi?
      kalıtım YOKSA → bu dal BOŞ. protected hiçbir şey söylemez.
      kalıtım varsa → protected
                      · aynı assembly ŞARTI da varsa → private protected
                      · assembly'yi UMURSAMIYORSAN  → protected internal
                        (dikkat: bu ikisinin GENİŞİdir, dar olanı değil)

④ Geriye public kaldı. Son soru: bunu bir gün GERİ ALABİLECEK misin?
      ██ public bedava değildir. ██ Daraltmak bir kırıcı değişikliktir
      ve derleyici onu ancak ÇAĞIRANIN tarafında gösterir.
```

### B — sözleşme mi, somut tip mi

```
① Bu yeteneği KAÇ uygulama üzerinden görmek zorunda — ÇAĞIRAN açısından?
      1 → ██ somut / sealed sınıf. Bitti. ██
          Varlık SAYISI bu soruya cevap DEĞİLDİR.
      ≥2 → ②

② İki uygulama gerçek bir AİLE mi — ortak değişmez durum/davranış paylaşıp
   her biri taban sözleşmesini koruyor mu?
      EVET → abstract taban        HAYIR → ③

③ Yetenekler birbirinden BAĞIMSIZ değişiyor mu?
      EVET → ██ BİLEŞİM ██ — parçayı alan olarak tut, devralma
             (bu projenin cevabı: Combatant, Structure)
      HAYIR→ ④

④ İkinci uygulama yalnızca TESTTE mi gerekiyor?
      önce sor: yalıtılacak bir şey VAR MI — ağ, dosya, rastgelelik,
      yavaş kurulum?
      YOKSA → sahte parça sınanan kuralı SİLER. Gerçek parçayı kullan.
              (ölçülmüş örnek: CombatantTests'in reddi)
      VARSA → ⑤

⑤ ██ ARAYÜZ ██ — ve DAR tut: sözleşmeye yalnız ÇAĞIRANIN ihtiyacı olan
   üyeler girer. Fazladan giren her üye, ikinci uygulamanın ödeyeceği
   vergidir.
```

---

## Yanlış hatırlanan üç şey

**"Arayüz kullanmak performansı iyileştirir."** Hayır — ve tersi bile mümkün.
Bir arayüz tipli referans ikinci bir nesne yaratmaz, örneği küçültmez, alan
silmez; yalnız **görünen üye kümesini daraltır**. Çağrı ise somut çağrıdan
farklı çözülür. ██ Bu projede o farkın bedeli ÖLÇÜLMEDİ ██ ve ölçülemez:
dört üretim assembly'sinde arayüz uygulaması sıfır, karşılaştırılacak taraf yok.
Arayüzün gerekçesi hız değil, **çağıranın ikinci bir uygulamaya ihtiyacı**dır.

**"`internal` = aynı klasör."** Hayır — `internal` = **aynı assembly**. Karşı
örnek bu depoda: `Assets/Game/Core/Combat/` klasörü `Assets/Game/Core/`'un
İÇİNDE, ama iki ayrı `.asmdef` var ve ikisinin `references` dizisi de boş.
Core'a yazılacak bir `internal` üyeyi, klasör olarak onun içinde duran
Combat dosyaları **göremez**. Sınırı çizen şey klasör değil, en yakın `.asmdef`.

**"On iki `static class` var, demek ki arayüzlerin yerine onlar konmuş."**
Hayır — ikisi farklı sorunun cevabı. `static class` "durum taşımayan bir kural
nereye yazılır" sorusuna cevap veriyor; arayüz "bir çağıran kaç uygulama
görmeli" sorusuna. ██ Ölçü: bir `static class` arayüz uygulayamaz ve parametre
olarak geçirilemez ██ — yani bir arayüzün yaptığı işi yapamaz bile. İkisi
birbirinin alternatifi değil; biri seçildi diye öteki elenmedi.

---

## Kaçış yolu: bu kararlar tersine çevrilseydi

```
  internal Board'u        → ① katmanı (dışarıda ok HİÇ doğmasın) aynı gün
  public yap                boşa çıkar; BoardAdapter tek satırda tahtaya
                            yazabilir. Derleyici susar, testler yeşil kalır.
                            ██ En ucuz görünen, en pahalı değişiklik. ██

  Battle.board'u          → BattleActions'ın MoveAction'a verecek bir tahtası
  hiç dışarı verme          kalmaz. İki çıkış: ya hareket kuralı Battle'a
                            kopyalanır (Battle bir kural sahibine döner), ya
                            MoveAction'ın imzası değişir (Core, Battle'ı
                            görmek zorunda kalır — duvar delinir).

  IAttackTarget yaz       → AttackAction'ın iki aşırı yüklemesi tek gövdeye
                            iner. KAZANÇ: iki metot. BEDEL: hedef uygunluğu
                            kuralı TargetingRules'tan hedefin İÇİNE taşınır,
                            Downed/Destroyed ayrımı tek bir bool'un arkasına
                            düşer. Gerekçe AttackAction.cs'in kendi bloğunda.

  12 static class'ı       → kurallar parametre olarak geçirilebilir hâle
  örneklenebilir yap        gelir. BEDEL: bugün sıfır olan "hangi kural
                            örneği" sorusu doğar ve her çağıranın bir cevabı
                            olmak zorunda kalır — bir kompozisyon kökü gerekir.

  hepsini public yap      → hiçbir şey KIRILMAZ. Ve mesele tam olarak bu:
                            ██ erişim daraltmanın faydası ödemediğin
                            faturalarda saklıdır. ██ Bedel görünür (bir
                            internal, bir dolaylı çağrı), fayda görünmez
                            (yazılmamış çağıranlar). Karar bu asimetriyle
                            veriliyor.
```

---

## ██ Adı geçen ama anlatılmayan mekanizmalar ██

```
  InternalsVisibleTo (tam mekanizma)   → HENÜZ YOK → sahipsiz.
      Doğacağı an: bir testin internal bir üyeye ihtiyaç duyduğu ilk gün.

  Açık arayüz uygulaması, varsayılan   → HENÜZ YOK → sahipsiz.
  arayüz üyeleri, `new` ile üye gizleme    Doğacağı an: ilk arayüz yazıldığı gün.

  Generic tipler ve kısıtlar           → HENÜZ YOK → kayıtlı; defterde
  (`where T : IFoo`)                       satırı var: [ogrenme/03](../../ogrenme/03-kavram-borc-defteri.md)

  Arayüz gönderim tablosunun ÖLÇÜSÜ    → ÖLÇÜLMEDİ.
      Ölçülebilmesi için önce bir arayüz gerekir.
```

---

## Bunu okuduktan sonra kodda ne göreceksin

`Battle.cs`'teki tek `internal` artık bir üslup tercihi değil —
bir duvarın adı, ve o duvarın nerede bittiği yazılı. `AttackAction.cs`'in
ikinci aşırı yüklemesinin üstündeki blok bir açıklama değil, **reddedilmiş bir
arayüzün gerekçesi**. `BoardAdapter`'ın tek bir `public` üyesi olmaması bir
eksiklik değil, bir yaprak assembly'nin doğal şekli. Ve on iki `static class`
bir alışkanlık değil, bugün doğru olan ve bedeli yazılı bir takas.

Kodda **karar**, burada **ödünç alınan dil özelliğinin sözleşmesi**. İkisi
çelişirse kod kazanır — orası çalışan metin, burası anlatı.

---

## İlgili

- Duvarın kendisi — klasör ≠ ad alanı ≠ assembly:
  [`konular/02`](../konular/02-assembly-duvari.md) ·
  tahtanın sahipliği ve "ikinci yazar":
  [`konular/03`](../konular/03-tahta-sahipligi.md)
- `sealed` / `readonly` / `{ get; }`:
  [`dil/01`](01-degismezlik-anahtar-kelimeleri.md) ·
  kimlik ve `ReferenceEquals`: [`dil/05`](05-deger-referans-ve-kimlik.md)
- Motor geri çağrıları neden `virtual` da `event` de değil:
  [`konular/08`](../konular/08-motor-cagri-dongusu.md)
- Desen adları — saf kural sınıfı, bileşim, kimlik + yan tablo:
  [`ogrenme/01`](../../ogrenme/01-koda-gomulu-desenler.md)
- Bu ağacın yönlendirmesi: [`dil/README.md`](README.md)

---

## ██ SIRADAKİ ADIM ██

> **▶ SIRADA:** ██ Bu dosya 14 adımlık okuma yolunda **yok** ██ —
> [`../../ogrenme/00-okuma-sirasi.md`](../../ogrenme/00-okuma-sirasi.md) yazıldığında henüz sıraya girmemişti. Yolda bir
> yer arıyorsan doğru komşusu **4. adım**: [`01-degismezlik-anahtar-kelimeleri.md`](01-degismezlik-anahtar-kelimeleri.md)
> ("kim YAZABİLİR") ile bu dosya ("kim GÖREBİLİR") aynı sorunun iki ucu.
> **NEDEN ORASI:** bu dosyanın taşıyıcı örneği `Battle.Board`'un `internal`i, ve
> onun proje tarafı [`konular/03`](../konular/03-tahta-sahipligi.md)'te — okuma yolunun **3.** adımı. `internal`in
> çizdiği çemberin `.asmdef` sınırıyla **aynı** çember olduğu ise
> [`konular/02`](../konular/02-assembly-duvari.md)'de, **2.** adımda.
> **YOL HARİTASI:** [`../../ogrenme/00-okuma-sirasi.md`](../../ogrenme/00-okuma-sirasi.md)
