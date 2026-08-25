# MoveAction

> **Kaynak:** `Assets/Game/Core/MoveAction.cs`
> **Ad alanı:** `GridStrategy.Core` · **Assembly:** `GridStrategy.Core` (`references: []`)
> **Rol:** Kural (Policy) — kimliği yok, hafızası yok; AKIŞI yürütür

Hareket akışının tek sahibi. `GridDistance` uzaklığı ölçer, `UnitGrid` hücreyi
tutar ve yazar. İkisi de birbirini TANIMAZ ve tanımamalıdır. Onları bir sıraya
dizen tek yer burasıdır — ve saldırı tarafındaki `AttackAction` ile aynı deseni
izler.

Neyi BİLMEZ: sıranın kimde olduğunu, birimin bu turda daha önce hareket edip
etmediğini, yolun üzerinde ne olduğunu (bugün adım adım yürünmüyor, hücreden
hücreye ışınlanılıyor), sonucu kimin göstereceğini.

Birimin DURUMUNU da bilmez — düşmüş bir birim bu akıştan geçer ve yer değiştirir.
Bu bir eksik değil, bir SINIR: durum `GridStrategy.Combat`'ta yaşıyor ve
`GridStrategy.Core` onu görmüyor.

| Üye | Karar | Detay |
|---|---|---|
| `MoveAction` (tip) | kural tahtaya taşınmaz: tahta kendiyle çelişirdi | [↓](#moveaction-tip) |
| `Execute(int moveRange)` | menzil kimliğin yanında değil, kuralın yanında yaşar; ret sırası bir karardır | [↓](#executeint-moverange) |
| `Execute(MoveProfile profile)` | aşırı yükleme bir dikiş yeridir; durum kuralı içeri alınmaz | [↓](#executemoveprofile-profile) |

**İlgili anlatılar:** [04-karar sırası](../../konular/04-karar-sirasi.md) ·
[02-assembly duvarı](../../konular/02-assembly-duvari.md)

---

## MoveAction (tip)

### KURAL TAHTAYA TAŞINMAZ: TAHTA KENDİYLE ÇELİŞİRDİ

### HARİTA: aynı tipe iki cevap birden düşerdi

Kural `UnitGrid`'in içine girseydi, tahta "dolu hücreye yazılabilir mi" sorusuna
İKİ ayrı cevap veren tek bir tip olurdu:

```
┌─ UnitGrid (reddedilen dünyada) ──────────────────────────┐
│  PlaceUnit(x, y, u)  dolu hücre ► ŞİKÂYETSİZ üstüne yazar│
│  TryMoveUnit(...)     dolu hücre ► REDDEDER               │ ◄── ÇELİŞKİ
└──────────────────────────────────────────────────────────┘
  Aynı sınıf, aynı soru, iki zıt cevap. Hangisinin "tahtanın
  kuralı" olduğunu söyleyen hiçbir şey yok.

Taşınan yalnız kural da olmazdı; kuralın İHTİYAÇLARI da taşınırdı:

  UnitGrid ──yeni ok──► GridDistance   (menzil ölçmek için)
           ──yeni ok──► MoveOutcome    (ret sebebi döndürmek için)
  ve Chebyshev/Manhattan kararı tahtanın İÇİNDE donardı  ◄── DURUŞ
                                                             NOKTASI

SEÇİLEN — tahta yalnız "ne nerede", kural bir kat üstte:
  MoveAction ──► UnitGrid      (hücreyi oku/yaz)
             ──► GridDistance  (uzaklığı ölç)
  UnitGrid ile GridDistance birbirini TANIMAZ.
```

### REDDEDILEN

Bu sınıf hiç doğmaz, kural tahtanın kendisine taşınır:

```csharp
public MoveOutcome TryMoveUnit(int fromX, int fromY, int toX,
                               int toY, int moveRange)
```

**KIRILAN:** aynı tip aynı soruya iki farklı cevap verir — `PlaceUnit` dolu
hücrenin üstüne şikâyetsiz yazar, `TryMoveUnit` reddeder.

```
UnitGrid GridDistance'ı tanır -> ölçüm kuralı tahtaya girer
Chebyshev/Manhattan kararı    -> tahtanın içinde donar
derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
```

**KAZANIRDI:** hücreleri değiştirebilen TEK yol hareket olsaydı ve hareket kuralı
hiç çeşitlenmeyecek olsaydı (tek tip birim, tek tip adım) — ayrı bir kural tipi o
oyunda fazladan bir katman olurdu.

### KAPSAM: kural "tahtada hiç mantık olmaz" DEĞİL

Ayırıcı soru: cevap tahtanın KENDİ ölçüsünden mi çıkıyor, yoksa dışarıdan gelen
bir politikadan mı?

```
IsInsideGrid   cevap dizinin boyutunda yazılı   ► tahtanın ÜYESİ
MoveUnit       tahtanın kendi tutarlılığı       ► tahtanın ÜYESİ
menzil kuralı  cevap dışarıdan gelen bir sayıda ► BURAYA ait
doluluk kuralı cevap oyunun tercihinde          ► BURAYA ait
```

KARŞI ÖRNEK aynı ad alanında: `UnitGrid.IsInsideGrid` bir kural gibi görünür ve
tahtada KALIR — o dosyanın rol notu bunu açıkça yazıyor ("kendi ölçüsüne bakan bir
sorgu, dışarıdan gelen bir politika değil"). Aynı şekilde `MoveUnit`'in atomikliği
de tahtada kalır. Yani tahtadan çıkarılan şey mantık değil, POLİTİKA.

### İŞ BÖLÜMÜ: üç tip, üç ayrı soru

```
ne NEREDE           ► UnitGrid       (tutar ve yazar)
kaç ADIM uzakta     ► GridDistance   (yalnız ölçer)
ne OLABİLİR, hangi  ► bu tip         (sırayı ve sonucu bilir)
SIRAYLA sorulur
```

Silinirse ne kırılır: bu tip silinirse iki bileşen ayakta kalır ama onları sıraya
dizen kimse olmaz — her çağıran kendi sırasını yazar ve sıralar ayrışır.
`UnitGrid` silinirse konumun sahibi kalmaz; `GridDistance` silinirse "bitişik ne
demek" kararı yeniden her çağırana dağılır. Üçü aynı işi üç kez yapmıyor.

**TEK CUMLE:** Tahta neyin NEREDE olduğunu bilir, neyin OLABİLECEĞİNİ değil;
ikisini tek tipe koymak tipi kendi kuralıyla çelişkiye sokar.

---

## Execute(int moveRange)

Bir hareket denemesini yürütür, kabul edilirse tahtayı GÜNCELLER ve ne olduğunu
döndürür. `moveRange` bu birimin bu turda kaç hücre uzağa gidebildiğidir;
`0` geçerlidir ve "yerinden kımıldayamaz" demektir.

`moveRange == 0` bilerek GEÇERLİ. `AttackProfile` menzil için en az 1 ister çünkü
hiçbir hücreye ulaşamayan bir SALDIRI anlamsızdır; hiçbir hücreye gidemeyen bir
BİRİM ise anlamlıdır — kök salmış, sersemlemiş, kuşatılmış. Asimetri kasıtlı ve
ikizinin gerekçesi [`MoveProfile.md`](MoveProfile.md#moveprofileint-range)'de.

---

### MENZİL KİMLİĞİN YANINDA DEĞİL, KURALIN YANINDA YAŞAR

HAREKET MENZİLİ NEREDEN GELİR: bugün parametre olarak.

#### HARİTA: sayının üç olası evi

```
① Unit'İN İÇİNDE        ② PARAMETRE (bu imza)   ③ MoveProfile
┌───────────────┐       ┌──────────────┐        ┌──────────────┐
│ Unit          │       │ Unit         │        │ Unit         │
│  Name         │       │  Name        │        │  Name        │
│  MoveRange ◄──│       └──────────────┘        └──────────────┘
└───────────────┘              │                       │
  sayı KİMLİĞİN                │ sayı çağırandan       │ Range
  yanında doğar                ▼                       ▼
  ◄── ① REDDEDİLDİ      Execute(..., int)     Execute(..., profile)
```

| sütun | ① | ② | ③ |
|---|---|---|---|
| sayının sahibi | kimlik | çağıran | paylaşılan TANIM |
| ikizle hiza | `AttackProfile`'la KOPUK | — | `AttackProfile` ile aynı şekil |
| bedeli | `Unit` "ne yapabileceğini" bilmeye başlar | her çağıran sayıyı taşır | fazladan bir tip |

③ bugün `MoveProfile` olarak DOĞDU ve bu dosyanın ikinci aşırı yüklemesi onu
okuyor; ② ise kuralın kendisini taşıyan imza olarak duruyor. Reddedilen tek ev
①'dir.

#### REDDEDILEN

`Unit`'e alan eklenir, parametre silinir ve içeride `unit.MoveRange` okunur:

```csharp
public Unit(string name, int moveRange) { ... }
public int MoveRange { get; }
```

**KIRILAN:** `Unit`'in "karar vermez, ne yapabileceğini bilmez" rolü delinir;
menzil, ikizi `AttackProfile`'dan ayrı bir yerde yaşamaya başlar.

```
derleyici: her new Unit("...") ikinci argüman ister
test: UnitGridTests ve BoardAdapter derlenmez
```

**KAZANIRDI:** menzil birime göre GERÇEKTEN değişmeye başladığı gün — süvari üç,
piyade bir, yaralı piyade sıfır; o gün de cevap `Unit`'e alan eklemek değil,
`AttackProfile`'ın ikizi olan bir `MoveProfile`'dır.

#### KAPSAM: kural "Unit'e hiçbir şey eklenmez" DEĞİL

Ölçüt: alan KİM OLDUĞUNU mu söylüyor, NE YAPABİLECEĞİNİ mi?

```
Name        kim olduğu          ► Unit'te yaşar, doğru yerde
MoveRange   ne yapabileceği     ► Unit'te yaşamaz
Health/Team ne yapabileceği     ► zaten Combat'ta, Unit'te değil
```

KARŞI ÖRNEK `Unit`'in kendisinde: `Name` bir alandır, orada durur ve doğrudur — o
dosyanın rol notu bunu "kim olduğunu bilir, ne yapacağını bilmez" diye yazıyor.
Aynı tipe can, taraf ya da durum da eklenmedi; menzil bu listede bir istisna değil,
kuralın dördüncü örneği.

#### İŞ BÖLÜMÜ: üç ayrı sahip, üç ayrı soru

```
KİM        ► Unit          (kimlik ve ad)
NE KADAR   ► MoveProfile   (paylaşılan tanım) veya çıplak int
İZİN VAR MI► bu tip        (sayıyı tahtayla karşılaştıran kural)
```

Silinirse ne kırılır: sayı `Unit`'e taşınırsa aynı bilgi hem `Unit`'te hem
`AttackProfile`'ın ikizinde yaşar ve iki tanım ayrışır; üstelik `new Unit("...")`
yazan HER satır ikinci bir argüman ister — `UnitGridTests` ve `BoardAdapter`
dahil. Bu tip sayıyı okumazsa menzil hiç uygulanmaz. Aynı sayının üç kopyası
değil, bir sayı ve iki ayrı sorumluluk.

**TEK CUMLE:** Bir sayı, onu KULLANAN kuralın yanında yaşar; kimliğin yanında
değil — `Unit` kim olduğunu bilir, ne yapabileceğini bilmez.

---

### KAYNAK HÜCRE UYUŞMAZLIĞI: İKİ KANAL, İKİ AYRI OKUYUCU

Kaynak hücrede gerçekten O birim mi duruyor? Uyuşmazlık bir OYUN sonucu değil, bir
ÇAĞIRAN hatasıdır: tahta konumun tek sahibidir, dolayısıyla "birim orada değil"
demek "benim kaydım tahtanınkiyle ayrışmış" demektir. `PlaceUnit`'in gürültülü
felsefesi burada da geçerli.

#### HARİTA: iki kanal, iki ayrı okuyucu

Bu metodun ürettiği her cevap iki kanaldan birine düşer ve kanalı belirleyen şey
CEVABIN OKUYUCUSUdur:

```
① MoveOutcome kanalı ─────────────► okuyucusu OYUN
   RejectedInvalidDestination   "oraya tıklama"
   RejectedCellOccupied         "bekle ya da başka hücre"
   RejectedOutOfRange           "önce yaklaş"
   Moved                        "oldu"
   her satırın karşısında oyuncunun YAPABİLECEĞİ bir şey var

② exception kanalı ──────────────► okuyucusu PROGRAMCI
   board == null                çağıranın hatası
   unit == null                 çağıranın hatası
   moveRange < 0                çağıranın hatası
   kaynak hücre uyuşmuyor       çağıranın hatası ◄── BU SATIR
```

Uyuşmazlık ①'e konsaydı o sütuna "oyuncunun yapabileceği şey: hiçbiri, kod bozuk"
diye bir satır girerdi — ve kanalın tanımı o satırla birlikte çökerdi. Ayrım
kanalın ADInda değil, sütununda.

#### REDDEDILEN

`MoveOutcome`'a bir değer daha eklenir ve buradan o dönülür:

```csharp
return MoveOutcome.RejectedUnitNotAtSource;
```

**KIRILAN:** bir programcı hatası oyun sonucu kılığına girer; `MoveOutcome` artık
iki ayrı şeyi birden anlatır — oyunda olanı ve kodun bozuk olduğunu. Gerekçenin
tamamı `UnitGrid.PlaceUnit`'te, yani
[`UnitGrid.md`](UnitGrid.md#placeunitint-x-int-y-unit-unit)'de.

```
yeni dal sessizce yutulur -> Play'de yanlış birim ışınlanır
derleyici: hiçbir şey der  .  test: o dal hiç sınanmaz
```

**KAZANIRDI:** hareket emirleri AĞDAN ya da bir tekrar (replay) kaydından
gelseydi — orada ayrışma beklenen bir durumdur ve çökmek yerine reddedip
senkronizasyon istemek doğru davranıştır.

#### KAPSAM: kural "uyuşmazlık hep fırlatır" DEĞİL

Ölçüt, ayrışmanın KİMİN kaydında olduğu:

```
kaynak hücre ile birim ayrışmış -> çağıranın kaydı bozuk ► fırlat
hedef hücrede başkası var       -> oyunun kendi olgusu   ► değer dön
```

KARŞI ÖRNEK bu metodun birkaç satır aşağısında: hedef hücrede başka bir birim
durması da bir "olmaz" hâlidir ama fırlatmaz, `RejectedCellOccupied` döner — çünkü
orada ayrışan bir kayıt yok, sadece tahtanın o anki hâli var. Aynı metot, iki zıt
cevap, tek ölçüt.

#### İŞ BÖLÜMÜ: iki kanal ÖRTÜŞMEZ, BÖLÜŞÜR

```
exception   ► akışın DEVAM ETMEMESİ gereken durumlar
              (null tahta, null birim, negatif menzil, ayrışmış kayıt)
MoveOutcome ► akışın normal işleyip bir cevap ürettiği durumlar
```

Silinirse ne kırılır: bu kontrol kalkarsa ayrışma sessizleşir — tahtada başka bir
birim duran hücreden hareket "başarıyla" yürür ve yanlış birim ışınlanır;
`Execute_UnitNotOnTheSourceCell_Throws` ile
`Execute_AnotherUnitOnTheSourceCell_Throws` kırmızıya döner. `MoveOutcome` tarafı
fırlatmaya çevrilirse oyunun her geçersiz tıklaması bir çökmeye dönüşür. İki kanal
aynı işi iki kez yapmıyor; iki ayrı okuyucuya yazıyor.

**TEK CUMLE:** Ret sebebi, çağıranın YAPABİLECEĞİ bir şeyi göstermelidir; "kodun
bozuk" bunlardan biri değildir.

---

### SIRA BİR KARARDIR: SINIR ► MENZİL ► DOLULUK

Önce tahta sınırı, sonra menzil, en sonda hücrenin doluluğu.

#### HARİTA: akış nerede duruyor

3x5'lik bir tahta, menzili 1 olan bir birim, hedef `(9,9)`. Üç ret sebebi de AYNI
ANDA doğru; dönen tek bir tane olacak:

```
SEVİYE 1  board.IsInsideGrid(9,9)          ✗
          >> AKIŞ BİTTİ << ──► RejectedInvalidDestination
SEVİYE 2  GridDistance.Between > moveRange ✗  ── BURAYA GELİNMEZ
SEVİYE 3  hedefte BAŞKASI var mı           ✗  ── BURAYA GELİNMEZ
```

Seviyeler arasında `return` var; tablo bir liste değil, bir duruş sırası. Hangi
satırın kazandığı, çağıranın gördüğü tek ret sebebini belirler.

Tahta sınırının başa gelmesi `AttackAction`'daki gerekçenin aynısı: 3x5'lik bir
tahtada `(9,9)` hücresi ne yaklaşarak ne bekleyerek geçerli olur. Diğer iki sebep
düzeltilse bile bu sebep AYAKTA KALIR, dolayısıyla doğru cevap odur.

Menzil ile doluluk arasında ise o ölçüt SUSAR: menzili düzeltirsen doluluk sürer,
doluluğu düzeltirsen menzil sürer — ikisi de ayakta kalır. Tie-break başka yerden
geliyor: menzil sorusu çağıranın zaten verdiği sayılara bakan saf bir
aritmetiktir, doluluk sorusu ise TAHTAYI okur. Ucuz ve yerel olan önce sorulur;
ayrıca birim ulaşamadığı bir hücrenin içinde kimin durduğunu hiç öğrenmemiş olur.

#### REDDEDILEN

Son iki blok yer değiştirir:

```csharp
if (board.TryGetUnit(toX, toY, out Unit occupant)) ...   // önce doluluk
```

**KIRILAN:** menzili 1 olan birim, tahtanın öbür ucundaki dolu hücre için "orada
biri var" cevabı alır.

```
yol bulucu  -> o hücreyi kalıcı engelli işaretler
sis gelince -> görülemeyen bir hücrenin dolu olduğu sızar
derleyici: hiçbir şey der  .  test: _PrefersOutOfRange kırmızı
```

**KAZANIRDI:** menzilin pratikte hiç kısıtlamadığı bir aşamada — serbest
yerleştirme turunda `moveRange` tahtadan büyüktür ve tek anlamlı soru doluluktur.

#### KAPSAM: dayanıklılık ölçütü HER ÇİFTİ ayıramaz

Ölçüt yalnız bir sebep ötekini KAPSIYORSA konuşur:

```
sınır  ile menzil   -> sınır, menzil düzeltilse bile kalır ► sınır önce
sınır  ile doluluk  -> aynı şekilde                        ► sınır önce
menzil ile doluluk  -> ikisi de ayakta kalır  ◄── ÖLÇÜT SUSAR
```

KARŞI ÖRNEK bu bloğun kendi konusunda: son çift için ölçüt hiçbir şey söylemiyor
ve karar BAŞKA bir gerekçeyle veriliyor — menzil sorusu çağıranın zaten verdiği
sayılara bakan saf bir aritmetik, doluluk sorusu ise TAHTAYI okuyor; ucuz ve yerel
olan öne geçiyor. Yani bu blok tek bir kural değil, biri susunca ötekine geçen İKİ
ölçüt anlatıyor.

#### İŞ BÖLÜMÜ: iki ölçüt, iki ayrı çift, iki ayrı test

```
DAYANIKLILIK ► SEVİYE 1'i tepeye koyar
               Execute_OutsideBoardAndOutOfRange_PrefersInvalidDestination
UCUZ+YEREL   ► SEVİYE 2 ile 3 arasını çözer
               Execute_OccupiedCellOutOfRange_PrefersOutOfRange
```

Silinirse ne kırılır: birinci ölçüt gidince var olmayan bir hücre "menzil dışı"
diye reddedilir ve sebep yanlış adlandırılır. İkinci ölçüt gidince menzili 1 olan
bir birim tahtanın öbür ucundaki hücre hakkında "orada biri var" bilgisi alır — sis
geldiği gün göremediği bir hücrenin içeriği sızmış olur. İki ölçüt aynı şeyi iki
kez söylemiyor; ikisi iki farklı çifti ayırıyor.

**TEK CUMLE:** Önce, diğerleri düzeltilse bile AYAKTA KALAN sebep sorulur;
eşitlikte ucuz ve yerel olan öne geçer.

---

### DOLULUK SORUSU: "BİRİ" DEĞİL, "BAŞKASI"

#### HARİTA: birim kendi hücresinde de bir "doluluk"

A birimi `(1,1)`'de duruyor ve oyuncu yeniden `(1,1)`'e tıklıyor:

```
        x=0     x=1     x=2
      ┌───────┬───────┬───────┐
  y=1 │       │   A   │       │  hedef (1,1) = A'nın KENDİ
      ├───────┼───────┼───────┤  hücresi
  y=0 │       │       │       │
      └───────┴───────┴───────┘

  "orada BİRİ var mı"    ► evet (A)   -> RejectedCellOccupied
                           ◄── A kendi kuralına takıldı
  "orada BAŞKASI var mı" ► hayır (o A) -> akış sürer, birim
                           aynı hücrede kalır
```

Ayrım tek bir `ReferenceEquals` çağrısında ve o çağrı bir optimizasyon değil:
KİMLİK karşılaştırması olmadan tahta, birimin kendisini bir engel sayar.

#### REDDEDILEN

```csharp
if (board.TryGetUnit(toX, toY, out Unit _))   // "orada biri var mı"
```

**KIRILAN:** birim kendi hücresine taşınmak istediğinde KENDİSİ tarafından
engellenmiş sayılır; seçili birimin üstüne ikinci kez tıklamak sık bir durumdur ve
yapay zekâ o hücreyi kalıcı olarak engelli işaretler.

```
derleyici: hiçbir şey der  .  test: _MovingToItsOwnCell kırmızı
```

**KAZANIRDI:** "yerinde kalmak" ayrı bir oyun eylemi olsaydı (bekle, nöbet tut) ve
kendi bedeli olsaydı — o gün aynı hücreye hareket bir hata olurdu ve reddetmek
çağıranı doğru komuta yollardı.

#### KAPSAM: kural "kimlik hep hariç tutulur" DEĞİL

Bu metotta `ReferenceEquals` İKİ kez geçiyor ve iki karşıt şey istiyor:

```
kaynak hücre  ► `!ReferenceEquals(standing, unit)` ise FIRLAT
                (orada O olmalı — kimlik ŞART)
hedef hücre   ► `!ReferenceEquals(occupant, unit)` ise REDDET
                (orada O olabilir — kimlik MUAF)
```

KARŞI ÖRNEK yukarıda, aynı metodun içinde: kaynak kontrolü kimliği hariç tutmaz,
tam tersine TALEP eder. Yani kimlik karşılaştırması bir üslup değil; her iki yerde
de "bu birim kendisi mi" sorusunun cevabı farklı bir işe yarıyor.

#### İŞ BÖLÜMÜ: iki kontrol, iki ayrı yanlış

```
kaynak kontrolü ► çağıranın kaydı ile tahtanın ayrışmasını yakalar
hedef kontrolü  ► birimin kendini engellemesini önler
```

Silinirse ne kırılır: hedefteki kimlik muafiyeti kalkarsa
`Execute_MovingToItsOwnCell_IsAllowed` kırmızıya döner ve seçili birime ikinci kez
tıklamak bir ret üretir — yapay zekâ o hücreyi kalıcı engelli işaretler. Kaynaktaki
kimlik şartı kalkarsa `Execute_AnotherUnitOnTheSourceCell_Throws` kırmızıya döner
ve yanlış birim taşınır. Aynı çağrı, iki ayrı görev.

**TEK CUMLE:** "Dolu mu" sorusu her zaman "BAŞKASI var mı" diye sorulur; kendi
kuralına takılan bir birim o kuralın kurbanıdır.

---

## Execute(MoveProfile profile)

Aynı hareket akışı, menzili çıplak bir sayı yerine `MoveProfile`'dan okuyarak.
Kuralın kendisi burada TEKRARLANMIYOR; üstteki sürüme devrediliyor. `null`
geçilemez: menzilsiz bir hareket denemesi bir oyun sonucu değil, bir çağıran
hatasıdır — `AttackResolver`'ın profil için koyduğu koruma ile aynı gerekçe.

### İKİ İMZA, TEK KURAL: AŞIRI YÜKLEME BİR DİKİŞ YERİDİR

#### HARİTA: kural nerede yaşıyor, çağıranlar nerede

```
┌── Execute(..., int moveRange) ─────────┐
│  KURALIN KENDİSİ burada:               │
│  sıra, üç ret sebebi, MoveUnit çağrısı │
└───────────────▲────────────────────────┘
                │ devir (kural TEKRARLANMIYOR)
┌───────────────┴────────────────────────┐
│  Execute(..., MoveProfile profile)     │
│  yalnız null kontrolü + profile.Range  │
└────────────────────────────────────────┘

imza              çağıranları
───────────────   ────────────────────────────────────────
int moveRange     MoveActionTests, BattleActionsTests
                  >> ÜRETİMDE ÇAĞIRANI KALMADI <<  ◄── ①
MoveProfile       BattleActions.Move — tek üretim çağıranı
                  + MoveActionTests'in karşılaştırma testi
```

① işaretli satır bu bloğun konusu: `int` sürümü ayakta tutan şey artık üretim
değil, iki imzanın YAN YANA durmasını gerektiren tek bir test —
`Execute_WithProfile_MatchesTheIntOverload`.

#### EŞİK — int alan sürüm ne zaman silinir

Son `int` çağıranı gittiği gün. ÜRETİMDE o gün geldi: `BattleActions.Move` artık
profil sürümünü çağırıyor ve `int` sürümün kalan bütün çağıranları testlerdir.
Sürüm yine de duruyor, çünkü `MoveActionTests`'teki
`Execute_WithProfile_MatchesTheIntOverload` ancak iki imza yan yana durursa
yazılabilir. İKİSİ DE aynı kuralı yürütür — biri diğerini çağırıyor.

#### REDDEDILEN

Aşırı yükleme eklenmez, `int` alan sürümün imzası doğrudan değiştirilir:

```csharp
public static MoveOutcome Execute(UnitGrid board, Unit unit,
    int fromX, int fromY, int toX, int toY, MoveProfile profile)
```

**KIRILAN:** `moveRange` geçen HER çağıran tek seferde düşer; bugün bunların hepsi
`MoveActionTests` ile `BattleActionsTests`'in içinde. Üstelik "profil sürümü
kuralı KOPYALAMIYOR" sınavı, iki imza yan yana durmadan yazılamayacağı için
tamamen kalkar.

```
derleyici: iki test assembly'si derlenmez  .  test: koşamaz bile
```

**KAZANIRDI:** iki sürümün uzun süre yan yana yaşayacağı bir dünyada — o gün
"hangisi gerçek kural" sorusu belirsizleşirdi ve tek imza bu belirsizliği baştan
keserdi.

#### KAPSAM: kural "her yeni ihtiyaç aşırı yükleme" DEĞİL

Aşırı yükleme yalnız İKİ İMZA AYNI KURALI yürütüyorsa doğrudur:

```
aynı kural, farklı GİRDİ ŞEKLİ   -> int / MoveProfile     ► aşırı yükleme
farklı kural, aynı ad            -> ayrı bir tip ya da ad ► yasak
```

KARŞI ÖRNEK bu bölümün konusu olan gövdenin kendisi. **Dikkat: `Execute` bu
dosyada İKİ kez tanımlı ve buradaki karşı örnek ikincisidir:**

```csharp
// ① KURALI TAŞIYAN SÜRÜM — gövdesi yukarıdaki
//   "Execute(int moveRange)" bölümünün konusu
public static MoveOutcome Execute(
    UnitGrid board, Unit unit,
    int fromX, int fromY, int toX, int toY, int moveRange)

// ② PROFİL SÜRÜMÜ — bu bölümün konusu; GÖVDESİNİN TAMAMI şu:
public static MoveOutcome Execute(
    UnitGrid board, Unit unit,
    int fromX, int fromY, int toX, int toY, MoveProfile profile)
{
    if (profile == null)
    {
        throw new ArgumentNullException(nameof(profile));   // ◄── kendi TEK işi
    }

    return Execute(board, unit, fromX, fromY, toX, toY, profile.Range);
    // ◄── kural burada YAZILMIYOR, ①'e devrediliyor
}
```

②'nin gövdesinde ne ret sırası, ne menzil karşılaştırması, ne `MoveUnit` çağrısı
var: profil sürümü kuralı KOPYALAMIYOR, yalnız null kontrolü yapıp `int` sürüme
devrediyor. Kural iki kez yazılsaydı bu iki imza aynı işin iki adı olmaktan çıkar,
iki ayrı kural olurdu — ve hangisinin gerçek olduğunu derleyici söyleyemezdi.

#### İŞ BÖLÜMÜ: hangi imza neyi kapatıyor

```
int sürüm     ► KURALI taşır; menzil doğrulaması (`moveRange < 0`)
                dahil bütün akış burada
profil sürüm  ► null profilini keser ve Range'i okur; ürüne
                bakan kapı bu
```

Silinirse ne kırılır: `int` sürüm silinirse kural profil sürümüne KOPYALANMAK
zorunda kalır ve `Execute_WithProfile_MatchesTheIntOverload` yazılamaz hâle gelir —
iki imzayı karşılaştıran sınav ortadan kalkar. Profil sürümü silinirse
`BattleActions.Move`'un tek çağrı yolu kapanır ve menzil yeniden çıplak bir sayı
olarak dolaşır.

**TEK CUMLE:** Aşırı yükleme, imzayı değiştirmenin bedelini ÖDEMEDEN aynı sonuca
götüren yoldur; ömrünü yukarıdaki EŞİK notu sınırlar.

---

### DURUM KURALI İÇERİ ALINMAZ, AKIŞ YUKARIDAN YÜRÜR

#### HARİTA: var olmayan ok ve onun yerine seçilen yol

```
REDDEDILEN — kuralı içeri almak
┌─ GridStrategy.Core (references: []) ─┐
│  MoveAction ─ ─ ─ ✗ ─ ─ ─ ─ ─ ─ ─ ─ ─┼─► MovementRules
└──────────────────────────────────────┘    UnitState
  Bu ok ÇİZİLEMEZ: asmdef'in references dizisi boş olduğu için
  tip adı derlenmez. Ok'u çizmek için referans eklemek gerekir
  ve o gün tahta kodu hasarı, takımı, yaşam döngüsünü GÖRÜR.

SEÇİLEN — akışı bir kat yukarıdan yürütmek
┌─ GridStrategy.Battle ─────────────────────────────────┐
│  BattleActions.Move                                   │
│    1. MovementRules.CanMove(...)  ◄── durum sorusu    │
│       hayır ► RejectedActorCannotAct (Core'a hiç      │
│               inilmez)                                │
│    2. evet  ► MoveAction.Execute(...) ────────────┐   │
└──────────────────────────────────────────────────┼────┘
┌─ GridStrategy.Core ──────────────────────────────▼────┐
│  MoveAction: yalnız tahta-şekilli sorular             │
└───────────────────────────────────────────────────────┘

Ok yönü tersine dönmedi; SORU yukarı taşındı. Kısıtı çözen şey
bir referans değil, bir katman seçimi.
```

#### REDDEDILEN

Durum kuralı akışın içine alınır, profil yerine birimin durumu sorulur:

```csharp
if (!MovementRules.CanMove(state)) return MoveOutcome.Rejected...;
```

**KIRILAN:** bu satır DERLENMEZ; Core'un asmdef'i Combat'a referans vermiyor.

```
referans eklenir -> tahta kodu hasarı ve yaşam döngüsünü görür
kural yine girse -> MoveOutcome'da bu ret için değer yok, biri
                    ödünç alınır ve çağırana yalan söylenir
derleyici: tip bulunamadı der  .  test: Core derlenmez, koşamaz
```

**KAZANIRDI:** durum Core'a taşınsaydı — "canlı/düşmüş/ölü" bir tahta kavramı
sayılsaydı; o gün Combat'ın ayrı assembly olma sebebi kalmazdı.

#### KAPSAM: kısıt "parametre alma" DEĞİL, "Combat'ı görme"

Ayırıcı ölçüt tipin hangi kutuda tanımlı olduğu:

```
MoveProfile   GridStrategy.Core'da   ► parametre olabilir ✓
UnitGrid      GridStrategy.Core'da   ► parametre olabilir ✓
UnitState     GridStrategy.Combat'ta ► adı bile yazılamaz ✗
```

KARŞI ÖRNEK bu imzanın kendisinde: `MoveProfile profile` bir parametredir, bir
tanımı taşır ve hiçbir sınırı zorlamaz — çünkü Core'da doğdu.
[`MoveProfile.md`](MoveProfile.md#moveprofile-tip) bunu tersinden yazıyor: profil
Combat'ta doğsaydı bu imza derlenmezdi. Yani reddedilen şey dışarıdan bilgi almak
değil, YANLIŞ KUTUDAN bilgi almak.

#### İŞ BÖLÜMÜ: iki katman, iki ret ailesi

```
TAHTA-şekilli retler  ► bu tip: geçersiz hedef, menzil, doluluk
EYLEYEN-şekilli ret   ► BattleActions: RejectedActorCannotAct
                        (sırası değil / durumu elvermiyor)
```

Silinirse ne kırılır: bu tip durum sorusunu da üstlenirse Core derlenmez; kural
yine de zorla içeri sokulursa `MoveOutcome`'da o ret için değer olmadığından biri
ödünç alınır ve çağırana yanlış sebep söylenir. Üst katman kontrolü atlarsa düşmüş
bir birim tahtada yer değiştirir. İki katman aynı soruyu iki kez sormuyor; Core'un
hiç sormadığını `MoveActionTests`'teki
`Move_DoesNotAskAboutUnitState_BecauseCoreCannotSeeCombat` sabitliyor.

**TEK CUMLE:** Assembly sınırı bir yavaşlatıcı değil bir KISIT'tır; doğru cevap
kuralı içeri almak değil, akışı bir kat YUKARIDAN yürütmektir.
