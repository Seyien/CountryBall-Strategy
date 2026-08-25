# MovementRules

> **Kaynak:** `Assets/Game/Core/Combat/MovementRules.cs`
> **Ad alanı:** `GridStrategy.Combat` · **Assembly:** `GridStrategy.Combat` (`noEngineReferences: true`)
> **Rol:** Kural (Policy) — kimliği yok, hafızası yok, **uygunluk** söyler

"Bu birim hareket edebilir mi?" sorusunun tek sahibi. Ne taşır ne tahtaya dokunur.

Bu tipin var olma sebebi tek cümleydi: `TargetingRules` "kime vurulur" ve "kim
diriltilir" sorularını cevaplıyordu, "kim hareket eder" sorusunu ise **kimse**
cevaplamıyordu — ve sahipsiz kural sessizce en yanlış yerde, akışın içinde
doğuyordu.

**Neden Combat'ta:** kural birimin DURUMUNU sormak zorunda ve `UnitState` burada
yaşıyor. Hareket AKIŞI (`MoveAction`) Core'da, hareket KURALI burada — çünkü akış
tahtayı, kural savaşı tanır. Core, Combat'ı görmediği için akışın bu kuralı kendi
içinden sorması mümkün değildir; soran taraf `BattleActions`'tır, tıpkı
`TargetingRules`'ın takımlı sürümünü onun sorması gibi.

| Üye | Karar | Detay |
|---|---|---|
| `CanMove(UnitState)` | beyaz liste; kuraldan türetme reddedildi | [↓](#canmoveunitstate-state) |
| `CanMove(UnitState, Team, Team)` | **yazılmadı** — hareketin ikinci tarafı yok | [↓](#canmoveunitstate-team-team) |

**İlgili anlatılar:** [05-yaşam döngüsü](../../../konular/05-yasam-dongusu.md) ·
[04-karar sırası](../../../konular/04-karar-sirasi.md) ·
[02-assembly duvarı](../../../konular/02-assembly-duvari.md)

---

## CanMove(UnitState state)

Hareket edebilir mi? Yalnızca `UnitState.Alive`.

`UnitState.Downed` için cevap HAYIR, ve bu `TargetingRules.CanBeAttacked(UnitState)`
ile **bilerek çelişir**: düşmüş birim hâlâ geçerli bir HEDEFTİR ama artık bir OYUNCU
değildir. Yerde yatan bir birimin kaçabilmesi, "işini bitirme" tasarımını anlamsız
kılardı — düşman gelene kadar sürünüp giden bir birim hiç düşmemiş sayılır.

`UnitState.Dead` için cevap da HAYIR, ama sebebi farklı: `Downed` bir kural gereği
durdurulur, `Dead` zaten oyunda değildir. İki HAYIR'ın aynı olması bir tesadüftür,
bir kural değil.

### İŞ BÖLÜMÜ: bu satırın üstünde İKİ karar var, örtüşmezler

Tek satır, **iki ayrı saldırıya karşı iki ayrı kararla** savunuluyor ve her biri
farklı bir GELECEĞİ kapatıyor:

```
biçim kararı (beyaz liste) ► UnitState'e YENİ DEĞER eklendiği gün
kaynak kararı (türetme yok)► hedefleme KURALI değiştiği gün
```

Biçim kararı silinirse eklenen dördüncü durum sessizce yürür. Kaynak kararı
silinirse durum kümesi hiç değişmeden, yalnızca "düşmüş birime vurulur" kuralı
değiştiği gün düşmüş birim yürümeye başlar. Aynı satırı koruyorlar ama aynı şeyi
korumuyorlar.

### Karar 1 — BEYAZ LİSTE YENİ DEĞERİ REDDEDER, KARA LİSTE KABUL EDER

Bilinmeyen bir enum değeri karşısında cevap HAYIR — beyaz liste biçimi bilerek
seçildi ve `ReviveRules.CanRevive`'ın şekli birebir bu. Dördüncü bir durum
(`Fleeing`, `Stunned`, `Petrified`) eklendiği gün bu metot onu SESSİZCE kabul
etmez; kararı yeniden vermek için buraya gelinir.

#### HARİTA: iki biçim BUGÜN aynı, YARIN ayrışır

Ayrışma noktası bugünkü satırlarda değil, henüz yazılmamış satırda. Tablo tam da
onu göstermek için dördüncü bir satır taşıyor:

```
  durum            beyaz liste          kara liste
                   (== Alive)           (!= Downed && != Dead)
  ──────────────────────────────────────────────────────────────
  Alive            ✓ EVET               ✓ EVET
  Downed           ✗ HAYIR              ✗ HAYIR
  Dead             ✗ HAYIR              ✗ HAYIR
  ──────────────────────────────────────────────────────────────
  Stunned (yarın)  ✗ HAYIR              ✓ EVET   ◄── >> AYRIŞMA <<
                   varsayılan REDDEDER  varsayılan KABUL EDER
  (UnitState)9     ✗ HAYIR              ✓ EVET   ◄── cast ile gelen
```

Üç satır aynı, dördüncü ve beşinci zıt. Bugünkü testlerin hiçbiri aradaki farkı
GÖREMEZ; fark ancak enum büyüdüğü gün doğar ve o gün de kimse buraya bakmaz —
çünkü bakılacak bir hata mesajı yoktur.

#### KAPSAM: "her zaman beyaz liste" DEĞİL

Ayırt edici soru: **bu metot bir YETKİ mi veriyor, yoksa bir MARUZİYET mi
bildiriyor?**

```
  CanMove          eyleyene bir yetki verir   -> beyaz liste
  CanRevive        eyleyene bir yetki verir   -> beyaz liste
  CanBeAttacked    hedefin maruziyetini söyler-> kara liste
```

Karşı örnek aynı ad alanında, `TargetingRules.CanBeAttacked`:

```csharp
return state != UnitState.Dead;
```

Orası bilerek KARA listedir ve doğrudur — yeni bir durumun sessizce "vurulabilir"
olması kazanılmış bir hak değil, maruz kalınan bir şeydir; ayrıca hedeflenebilirlik
orada ÇOĞUNLUK cevabıdır. Yani bu karar "kara liste kötüdür" demiyor; sessizce
yanılmayı göze ALMADIĞIMIZ tarafın yetki tarafı olduğunu söylüyor.

#### `enum` parametresi kapalı küme sanmak

Okuyucu korumayı tipe yazabilir: "parametre enum, üç değerden biri gelir". C#'ta
enum kapalı bir küme DEĞİLDİR; `CanMove((UnitState)9)` derlenir ve çalışır. Beyaz
listenin ikinci ve sessiz kazancı budur: tanımsız sayı da HAYIR alır. Kara liste
ona EVET derdi.

#### REDDEDILEN

Kara liste biçimi, `TargetingRules.CanBeAttacked`'ın şekli:

```csharp
return state != UnitState.Downed && state != UnitState.Dead;
```

**KIRILAN:** bugünkü üç durumda aynı cevabı verir; kırılan şey İLERİDE olur.

```
UnitState'e eklenen her yeni durum -> varsayılan YÜRÜYEBİLİR
sersemletilmiş birim eklenir -> hiçbir test kırılmadan yürür
derleyici: hiçbir şey der  ·  test: eksik kural ancak oyunda görülür
```

**KAZANIRDI:** hareket edebilenlerin ÇOĞUNLUK olacağı bir tasarımda — on durumun
sekizi yürüyorsa beyaz liste her yeni durumda düzenlenir ve asıl kural iki
istisnanın altında kaybolur.

**TEK CUMLE:** Beyaz liste yeni değeri REDDEDER, kara liste KABUL eder; sessizce
yanılmak istemediğimiz taraf yetki tarafıdır.

### Karar 2 — KURALDAN TÜRETME, VERİDEN TÜRETME DEĞİLDİR

#### HARİTA: oklar nereden geliyor

```
  REDDEDILEN — türetme
    UnitState ─► TargetingRules.CanBeAttacked ─┐
    UnitState ─► TargetingRules.CanBeRevived ──┼─► CanMove
                                                ◄── >> HAREKET,
                                                SALDIRININ TÜREVİ <<
    "düşmüş birime vurulur" değişir -> ok boyunca buraya taşınır

  SEÇİLEN — üçü de AYNI kaynaktan, birbirinden DEĞİL
    UnitState ─► CanMove                  (MovementRules)
    UnitState ─► CanAttack                (AttackRules)
    UnitState ─► CanRevive                (ReviveRules)
    UnitState ─► CanBeAttacked/CanBeRevived (TargetingRules)
                 ◄── >> ARALARINDA HİÇ OK YOK <<
```

Üç kural bugün aynı satırı taşıyor (`state == Alive`) ama bu bir kesişimdir, bir
bağ değil; ölçülmüş hâli [ReviveRules.md](ReviveRules.md)'de ve üçünü birden kayda
geçiren tek test oradadır.

#### KAPSAM: türetmenin tamamı yasak DEĞİL

Ayırt edici soru: **türetilen şey bir VERİ mi, bir KURAL mı?**

Karşı örnek aynı ad alanında, `Structure.cs`:

```csharp
public bool CanAttack => AttackProfile != null;
```

Bu da bir türevdir ve doğrudur — çünkü türetildiği şey bir kural değil, o nesnenin
kendi VERİSİdir. Veri değişince türev değişmeli zaten; kural değişince türev
DEĞİŞMEMELİ. Yasak olan ikincisi.

#### İŞ BÖLÜMÜ: matris iki eksenli, her hücrenin sahibi var

```
                         EYLEYEN ekseni      HEDEF ekseni
  yürümek                MovementRules       (hedef yok)
  vurmak                 AttackRules         TargetingRules.CanBeAttacked
  kaldırmak              ReviveRules         TargetingRules.CanBeRevived
```

Bu dosya EYLEYEN sütununun bir hücresini tutuyor. Türetme yazılsaydı hücre kendi
cevabını üretmeyi bırakır, komşu SÜTUNDAN okurdu. Bu karar silinirse iki eksen tek
eksene çöker ve "kim yürür" sorusunun sahibi kalmaz — tipin var olma sebebi de o
sahipsizlikti.

#### GARANTİ NEREDE BİTER

Ayrı yazılmak, ayrı KALACAKLARI anlamına gelmiyor: üç satır bugün birebir aynı ve
bir sonraki okuyucu haklı olarak "bunlar tek metot olmalı" diyecek. Onları ayrı
tutan şey derleyici değil, bu karar ile `ReviveRules`'taki kardeşidir;
ayrılacakları gün de orada adıyla yazılı (vuramayan ama arkadaşını kaldırabilen
yaralı sıhhiyeci).

#### REDDEDILEN

Kural kendi başına yazılmaz, hedefleme kuralından TÜRETİLİR:

```csharp
return TargetingRules.CanBeAttacked(state)
       && !TargetingRules.CanBeRevived(state);
```

**KIRILAN:** bugün ÜÇ durumda da doğru cevabı verir — tehlikeli olan tam bu.

```
hareket kuralı artık saldırı kuralının TÜREVİdir
"düşmüş birime vurulur" değişir -> düşmüş birim yürümeye başlar
derleyici: hiçbir şey der  ·  test: hiçbiri kırmızıya dönmez
```

**KAZANIRDI:** hareket gerçekten "hedeflenebilir ama diriltilemez" olmanın TANIMI
olsaydı — o gün üç metot değil bir metot olurdu ve adı `CanMove` olmazdı.

**TEK CUMLE:** İki kuralın bugün kesişmesi onları aynı kural yapmaz; türetme,
kesişmenin bittiği günü sessiz kılar.

---

## CanMove(UnitState, Team, Team)

**İKİNCİ PARAMETRE ÇOĞU ZAMAN İKİNCİ BİR KURALDIR.**

Takım aşırı yüklemesi **bilerek yok** — ve `TargetingRules`'la arasındaki bu
asimetri öğreticidir. Hedeflemenin takımlı sürümü var çünkü "kime saldırabilirim"
İKİ TARAFIN sorusudur: bir saldıran, bir hedef. Hareketin ikinci tarafı yoktur;
birim kendi kendine yürür.

"Sıra kimde" sorusu buraya benzeyebilir ama buranın işi değildir: o soru
`TurnRules`'ın ve Battle katmanında yaşıyor.

### HARİTA: "bu birim şimdi yürüyebilir mi" kaç sahipli

Oyuncunun sorduğu tek soru, kodda ÜÇ ayrı sahibe bölünmüş; onları birleştiren yer
de dördüncü bir sahiptir:

```
  SORULAN ŞEY            SAHİP             KATMAN
  ─────────────────────────────────────────────────────────────
  "durumu uygun mu"      MovementRules     Combat  ◄── >> BURASI <<
  "sırası mı"            TurnRules         Battle
  "gideceği yol var mı"  MoveAction        Core
  üçünü SORAN            BattleActions     Battle  ◄── birleştiren
```

Reddedilen imzanın ikinci satırı (`unitTeam != activeTeam`) ikinci satırdaki
kuralı BU satıra taşır; iki sahip aynı cümleyi söylemeye başlar ve ayrıştıkları gün
hangisinin doğru olduğunu kimse bilemez.

### KAPSAM: "takım parametresi almak" yasak DEĞİL

Ayırt edici soru: **sorunun İKİNCİ BİR TARAFI var mı?**

Karşı örnek aynı ad alanında, `TargetingRules`:

```csharp
public static bool CanBeAttacked(UnitState state,
                                 Team attackerTeam, Team targetTeam)
```

Orada iki takım parametresi tamamen doğrudur, çünkü "kime saldırabilirim" gerçekten
iki taraflı bir sorudur: bir saldıran, bir hedef. Hareketin ikinci tarafı yoktur —
birim kendi kendine yürür — ve tarafı olmayan bir soruya taraf parametresi eklemek,
o parametreyi haklı çıkarmak için başka bir evden bir kural ödünç almayı
gerektirir. Bu karar ikinci parametreyi değil, ödünç alınan kuralı reddediyor.

### İŞ BÖLÜMÜ: üç sahip örtüşmez, bölüşür

```
MovementRules  ► birimin KENDİ durumu     (Alive mi)
TurnRules      ► sıranın kimde olduğu     (takım ekseni)
MoveAction     ► tahtanın izin verip vermediği (mesafe, dolu hücre)
```

`MovementRules` silinirse düşmüş birim yürür. `TurnRules` silinirse oyuncu rakip
birimini oynatır. `MoveAction` silinirse birim duvardan geçer. Üçü aynı soruyu üç
FARKLI eksende cevaplıyor; birleştirme hakkı yalnız üçünü birden gören katmanın —
`BattleActions`'ın.

### `static` hiçbir sınır koymaz

Okuyucu "bu tip static, durum tutmaz, o hâlde kural sızmaz" diye kredi verebilir.
`static` yalnızca örnek durumu tutmamayı garanti eder; PARAMETRE yoluyla başka bir
katmanın bilgisini eve almayı hiç engellemez. Reddedilen imza tamamen `static`'tir
ve tam da bu yüzden tehlikelidir: sızıntı alanlarda değil, imzada olurdu.

### REDDEDILEN

```csharp
public static bool CanMove(UnitState state, Team unitTeam, Team activeTeam)
{
    if (unitTeam == Team.None) { return false; }
    if (unitTeam != activeTeam) { return false; }
    return CanMove(state);
}
```

**KIRILAN:** ikinci satır SIRA kuralıdır ve sahibi `TurnRules`'tır; kural o an iki
evde birden yaşar.

```
TurnRules'a "aynı takım üst üste oynar" istisnası eklenir
-> burası haberdar olmaz, iki cevap ayrışır
Team parametresi Battle'ın bilgisini Combat'a taşır
derleyici: hiçbir şey der  ·  test: hiçbiri kırmızıya dönmez
```

**KAZANIRDI:** hareketin gerçekten İKİNCİ BİR TARAFI olsaydı — itme, sürükleme,
zorla yer değiştirme. O gün cevap sıra kuralı değil sahiplik kuralı olurdu.

**TEK CUMLE:** Bir metoda ikinci parametre eklemek çoğu zaman ikinci bir KURALI eve
almaktır; kural başına bir sahip.
