# TurnRules

> **Kaynak:** `Assets/Game/Battle/TurnRules.cs`
> **Ad alanı:** `GridStrategy.Battle` · **Assembly:** `GridStrategy.Battle`
> **Rol:** Kural (Policy) — kimliği yok, hafızası yok, yalnızca İZİN söyler

"Bu birim **şu an** eyleyebilir mi?" sorusunun tek sahibi.

Bu tipin var olma sebebi `MoveAction`'ın kendi başlığında zaten yazılıydı: *"Neyi
BİLMEZ: sıranın kimde olduğunu, birimin bu turda daha önce hareket edip
etmediğini."* O iki soru sahipsizdi, ve sahipsiz kaldığı sürece cevapları her
çağıranın içinde ayrı ayrı doğacaktı — arayüzde bir `if`, yapay zekâda bir
başkası, ve ikisi ilk denge değişikliğinde ayrışacaktı.

`TurnState` sıranın KİMDE olduğunu tutar; burası o bilgiden ne ÇIKTIĞINI söyler.
Ayrım `Team` ile `TargetingRules` arasındaki ayrımın aynısıdır: taraf bir
değerdir, tarafın ne yapabildiği bir kuraldır, ve ikisi aynı yerde yaşamaz.

| Üye | Karar | Detay |
|---|---|---|
| `MaxActionsPerTurn` | eşik kuralın metnidir, duruma taşınmaz | [↓](#maxactionsperturn) |
| `CanAct(Team, Team)` | yalnız SIRAYI sorar; ikinci soruyu sormaz | [↓](#canactteam-team) |
| `CanAct(Team, Team, int)` | değer alır, nesne almaz; sıra kuralını KOPYALAMAZ, sorar | [↓](#canactteam-team-int) |

**İlgili anlatılar:** [04-karar sırası](../../konular/04-karar-sirasi.md) ·
[02-assembly duvarı](../../konular/02-assembly-duvari.md)

---

## MaxActionsPerTurn

Bir birimin kendi turunda kaç kez eyleyebileceği. Bugün **bir**: turun bir anlamı
olması için "sıra sende" cümlesinin bir sınırı olması gerekir, yoksa sırası gelen
birim aynı turda yirmi kez vurur ve sıra sistemi yalnızca sırayı geciktirmiş olur.

### HARİTA: sayı taşınırsa İMZAYA ne olur

```
  SEÇİLEN — sayı kuralın içinde
  ╔══════════════ TurnRules ══════════════╗
  ║ MaxActionsPerTurn = 1   ◄── metnin parçası
  ║ CanAct(...) { used < MaxActionsPerTurn }
  ╚═══════════════════════════════════════╝
    çağıran ──► CanAct(team, current, used)      3 argüman

  REDDEDILEN — sayı TurnState'e taşınır
  ╔═ TurnState ═══════╗     ╔═══════ TurnRules ═══════╗
  ║ MaxActionsPerTurn ║     ║ sayıyı OKUYAMAZ         ║
  ╚═════════▲═════════╝     ╚════════════╤════════════╝
            └── DÖRDÜNCÜ parametre ──────┘
    çağıran ──► CanAct(team, current, used, max)  4 argüman
    ◄── ██ dördüncüyü yanlış dolduran çağıran birime fazladan
        eylem hakkı verir; imza bunu göremez ██
```

Kayan şey sayının yeri değil, **SORUMLULUK**: sınırı beyan etmek kuraldan çıkıp
her çağırana dağılır.

### KAPSAM: kurala parametre geçmek yasak DEĞİL

Ölçüt: geçilen şey KURALIN metni mi, ÇAĞIRANIN durumu mu?

**KARŞI ÖRNEK** aynı dosyada, aşağıdaki `actionsUsedThisTurn` parametresi: o da
dışarıdan geliyor ve doğrusu bu — birimin bu turda kaç eylem harcadığı bir
DURUMdur, `static` ve hafızasız bir tip onu tutamaz. Aynı karşılaştırmanın iki
yanı: sol taraf çağırandan gelir, sağ taraf gelmez. Ayıran şey parametre sayısı
değil, değerin savaştan savaşa değişip değişmediği.

### İŞ BÖLÜMÜ: eşik burada, sayaç dışarıda

```
  MaxActionsPerTurn     EŞİK   ► kuralın metni, tek yer
  actionsUsedThisTurn   SAYAÇ  ► çağıranın durumu, her çağrıda
```

İkisi ÖRTÜŞMÜYOR; birlikte tek bir karşılaştırma kuruyor. Eşik dışarı taşınırsa
yukarıdaki dört argümanlı şekil doğar. Sayaç içeri taşınırsa anahtarı olmayan bir
sözlük gerekir ve o reddedilme [`TurnState.actionsUsed`](TurnState.md#actionsused)
başlığı altında yazılı. Yani sayı ile sayaç birbirinin yerine geçmiyor — biri
kuralın, diğeri savaşın malı.

### REDDEDILEN

Sabit buradan silinir, sayı savaşın durumuna taşınır:

```csharp
// TurnState içinde:
public int MaxActionsPerTurn { get; }
```

**KIRILAN:** kural, kuralı sahiplenmeyi bırakır.

```
CanAct sayıyı okuyamaz
  -> DÖRDÜNCÜ parametre olarak ister
  -> onu yanlış dolduran çağıran birime fazladan eylem hakkı verir
derleyici: hiçbir şey der
test: CanAct_BudgetPlane_OnItsOwnTurn tablosu [TestCase] ile yazılamaz olur
      — özniteliğe nesne konmaz
```

**KAZANIRDI:** sınır savaştan savaşa DEĞİŞSEYDİ — "yıldırım modunda herkes iki
kez oynar" gibi bir mod düğmesi ya da zorluk seviyesine bağlı bir ayar; o gün sayı
gerçekten durumdur ve buradaki sabit her mod için yeniden derleme gerektirirdi.

**TEK CUMLE:** Bir sabit kuralın metninin parçasıdır; onu duruma taşımak kuralı
her çağıranın yeniden beyan ettiği bir sayıya çevirir.

---

## CanAct(Team, Team)

Sıra bu tarafta mı? Bütçeye **bakmaz** — yalnızca sıranın kimde olduğunu sorar.

`Team.None` hiçbir sırada eyleyemez, ve gerekçe `TargetingRules`'ınkiyle aynı
yerden gelir: tarafsız olan taraf tutmaz, duvar vurmaz. Burada ayrıca bir sessiz
hatayı da kapatır — `default(Team)` tarafsızdır, yani takımı atanmayı unutulmuş
bir birim HER ŞEYİ değil HİÇBİR ŞEYİ yapabilir olur ve eksiklik ilk denemede
görülür.

Gövdedeki tarafsızlık denetimi yalnızca **bir yanda** duruyor: ikinci bir
`currentTurn == Team.None` kontrolü gereksiz, çünkü gerçek bir takım zaten
`None`'a eşit değildir ve `None`-`None` hâli ilk satırda kapanır. İkinci kez
yazmak, tek kuralı iki kural gibi gösterirdi.

### HARİTA: iki soru, iki imza

```
  "sıra bu tarafta mı"       CanAct(team, current)
       soranın elinde: TAKIM

  "sıra + bütçe"             CanAct(team, current, used)
       soranın elinde: TAKIM + SAYAÇ

  REDDEDILEN — tek imza, uydurma sıfır
  arayüz düşman birimlerini soluklaştırır
       │  elinde ne birim ne sayaç var
       └──► CanAct(team, current, actionsUsedThisTurn: 0)
            ◄── ██ cevap "eyleyebilir" ██ — oysa sorulan şey
                eylem değil SIRAydı; sıfır bir varsayılan değil,
                sorulmamış bir sorunun uydurma cevabı
```

### KAPSAM: varsayılanlar yasak DEĞİL

Ölçüt: varsayılan AYNI soruyu yaygın bir değerle mi cevaplıyor, yoksa BAŞKA bir
soruyu mu?

**KARŞI ÖRNEK** aynı ad alanında,
[`TurnState`'in parametresiz kurucusu](TurnState.md#turnstate): o da bir
varsayılan (`DefaultTurnOrder`'a devrediyor) ve dürüst, çünkü "hangi dizilim"
sorusunu yaygın bir dizilimle cevaplıyor — soruyu değiştirmiyor. Reddedilen sıfır
ise "bütçesi kaldı mı" sorusunu, hiç sorulmamışken cevaplıyor.

### İŞ BÖLÜMÜ: iki aşırı yükleme ÖRTÜŞMEZ, ZİNCİRLENİR

```
  iki parametreli  ► SIRA kuralının tek metni
  üç parametreli   ► BÜTÇE kuralının tek metni + sıra kuralını
                     kopyalamaz, ona SORAR
```

Bu yüzden ikisi bir çift değil bir zincir. İki parametreli sürüm silinirse
"tarafsız eyleyemez" kararı üç parametreliye kopyalanır ve elinde sayaç olmayan
çağıranlar uydurma sıfır geçmeye zorlanır. Üç parametreli sürüm silinirse bütçe
kuralı sahipsiz kalır ve her çağıran kendi karşılaştırmasını yazar.

### REDDEDILEN

Bu aşırı yükleme hiç doğmaz, yalnızca üç parametreli sürüm kalır ve taraf
sorusunu soran uydurma bir sıfır geçer:

```csharp
TurnRules.CanAct(unitTeam, currentTurn, actionsUsedThisTurn: 0)
```

**KIRILAN:** uydurma sıfır, çağrıyı bir yalana çevirir.

```
arayüz düşman birimlerini soluklaştırırken elinde ne birim ne sayaç vardır
  -> sıfır geçer
  -> cevap "eyleyebilir" olur, oysa sorulan şey eylem değil SIRAydı
derleyici: hiçbir şey der
test: CanAct_TeamPlane matrisi ölçmediği bir sütun taşır ve taraf kararı
      bütçenin altında kalır
```

**KAZANIRDI:** sıra sorusu HİÇBİR zaman birimsiz sorulmayacaksa — o gün iki aşırı
yükleme hangisinin gerçek kural olduğunu belirsizleştirir ve biri sessizce eskir.

**TEK CUMLE:** İki farklı soru varsa iki imza vardır; birini diğerinin
varsayılanıyla sormak, sorulmayan soruyu cevaplamış olur.

---

## CanAct(Team, Team, int)

Sıra bu tarafta mı **ve** birimin bu turda eylem hakkı kaldı mı?

Sıra kuralı burada **kopyalanmıyor**, iki parametreli sürüme soruluyor.
Kopyalansaydı "tarafsız eyleyemez" kararı iki yerde yaşardı ve biri değiştiğinde
diğeri sessizce eskirdi.

Negatif harcama bir oyun sonucu değil, bir **çağıran hatasıdır**: "eksi bir kez
hareket etmiş" diye bir durum yok. Sessizce sıfır saymak, sayacı bozuk olan
çağırana sonsuz eylem hakkı verirdi.

**Alternatif:** tür başına ayrı sınır (`MaxMovesPerTurn` ile `MaxAttacksPerTurn`)
ve türü taşıyan bir enum. Seçilmedi: bugün cevap eylem TÜRÜNE bağlı değil —
tetiği klasik "önce yürü, sonra vur" turu — ve ayrı sayaçlar her çağırana
ölçülmeyen bir argüman doldurtur.

### HARİTA: bağımlılık YÖNÜ

```
  SEÇİLEN
  ╔═ TurnState (VARLIK) ═╗
  ║ Current, TurnNumber  ║──okunur──► Team , int
  ╚══════════════════════╝                │
  ╔═ TurnRules (KURAL) ══╗◄──değer alır───┘
  ║ hiçbir tipi TUTMAZ   ║  ██ KURAL VARLIĞI TANIMAZ ██
  ╚══════════════════════╝

  REDDEDILEN
  ╔═ TurnRules (KURAL) ══╗──tanır──► ╔═ TurnState (VARLIK) ═╗
  ╚══════════════════════╝           ╚══════════════════════╝
       ◄── ██ OK TERSİNE DÖNDÜ ██
  -> kuralı sınamak için geçerli dizilimli bir savaş kurmak gerekir
  -> [TestCase] argümanı SABİT olmak zorunda
  -> TurnState örneği özniteliğin içine konamaz
  -> taraf matrisi tek bir tablodan dokuz ayrı metoda dağılır
```

### KAPSAM: "kural hiçbir tipi tanımaz" DEĞİL

Ölçüt: tanınan şeyin KİMLİĞİ ve HAFIZASI var mı?

**KARŞI ÖRNEK** bu dosyanın ikinci satırında: `using GridStrategy.Combat;` — bu
tip `Team`'i tanıyor ve tanımak zorunda. `Team` bir DEĞER: iki `Team.Player` aynı
şeydir, hafızası yoktur, özniteliğe sabit olarak yazılabilir. `TurnState` ise
kimliği ve hafızası olan bir VARLIK. Kuralın yasağı bağımlılık değil, **ÖMÜR**
bağımlılığı.

### İŞ BÖLÜMÜ: üç parametre, üç ayrı sahip

```
  unitTeam             ► Combatant'ın malı  "soran kim"
  currentTurn          ► TurnState'in malı  "sıra kimde"
  actionsUsedThisTurn  ► çağıranın malı     "kaç harcadı"
```

Üçü ÜÇ ayrı yerden geliyor ve imza bunu görünür kılıyor. `TurnState` geçilseydi
ikinci ve üçüncü tek nesnenin arkasına girerdi ve kuralın hangisini okuduğu
imzadan okunamazdı. Herhangi biri düşerse kural cevap üretemez — bu üç değer
eksiksiz ve fazlasız; kuralın tamamı o satırda görünüyor.

### REDDEDILEN

Değerler yerine savaşın kendisi geçilir:

```csharp
public static bool CanAct(Team unitTeam, TurnState turn,
                          int actionsUsedThisTurn)
```

**KIRILAN:** bağımlılık yönü tersine döner — KURAL, VARLIK'ı tanır.

```
kuralı sınamak için geçerli dizilimli bir savaş kurmak gerekir
  -> öznitelik argümanı sabit olmak zorunda
  -> TurnState örneği [TestCase] içine konulamaz
  -> taraf matrisi dokuz metoda dağılır
derleyici: hiçbir şey der  ·  test: tablolar var olamaz
```

**KAZANIRDI:** cevap savaşın BİRDEN ÇOK alanına bağlı olsaydı — sıra, tur
numarası, kalan süre ve bütçe birlikte; o gün dört ayrı değeri elden geçirmek
tören olurdu.

**TEK CUMLE:** Bir kurala nesne geçmek onu o nesnenin ömrüne bağlar; değer geçmek
kuralı tek başına sorulabilir bırakır.
