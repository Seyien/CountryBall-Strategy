# MoveOutcome

> **Kaynak:** `Assets/Game/Core/MoveOutcome.cs`
> **Ad alanı:** `GridStrategy.Core` · **Assembly:** `GridStrategy.Core` (`references: []`)
> **Rol:** Tanım (Profile) — kimliği yok, hafızası yok, karar vermez; olup biteni ADLANDIRIR

Bir hareket DENEMESİNİN sonucu. "Deneme" kelimesi kasıtlı: reddedilen bir hareket
de bir sonuçtur ve çağıranın onu ayırt etmesi gerekir. Kararı `MoveAction` verir,
bu tip yalnızca adı taşır.

**Neden `bool` değil:** "taşındı mı" tek cevaba üç ayrı soruyu sıkıştırırdı ve
çağıran üçüne farklı tepki verir — tahta dışı bir tıklama sessizce yutulur, dolu
hücre "orada biri var" uyarısı ister, menzil dışı ise yol bulucuya "önce yaklaş"
der. `bool` ile yazılsaydı bu ayrım çağıranın içinde ikinci bir kontrol olarak
yeniden doğardı — ve o kontrol `MoveAction`'ın kurallarını kopyalardı.

| Üye | Karar | Detay |
|---|---|---|
| `RejectedInvalidDestination` | sıfırıncı değer bilerek bir RET değeridir | [↓](#rejectedinvaliddestination) |
| `RejectedCellOccupied` | üç ret sebebi tek değere indirilebilirdi — indirilmedi | [↓](#rejectedcelloccupied) |
| `RejectedOutOfRange` | "şimdilik gidilemez": bir tur sonra değişebilir | [↓](#rejectedoutofrange) |
| `Moved` | tek KABUL değeri; sıfırıncı sırada bilerek değil | [↓](#moved) |
| `RejectedActorCannotAct` | beşinci değer — ve sahibi onu ÜRETEMEZ | [↓](#rejectedactorcannotact) |

**İlgili anlatılar:** [02-assembly duvarı](../../konular/02-assembly-duvari.md) ·
[06-sonuç enum'ları](../../konular/06-sonuc-enumlari.md) ·
[04-karar sırası](../../konular/04-karar-sirasi.md)

---

## RejectedInvalidDestination

### SIFIRINCI DEĞER BİLEREK BİR RET DEĞERİ

### HARİTA: numarayı ad değil SIRA belirler

Bu enum'da tek bir `= 0` bile yazılı değil; üyeler yazıldıkları satır sırasına
göre numaralanır. Yani "sıfırıncı değer" bir isimlendirme kararı değil, bir
YERLEŞTİRME kararıdır.

```
SEÇİLEN (bugünkü sıra)         REDDEDILEN (Moved başa alınır)
0 RejectedInvalidDestination   0 Moved                  ◄── ①
1 RejectedCellOccupied         1 RejectedInvalidDestination
2 RejectedOutOfRange           2 RejectedCellOccupied
3 Moved                        3 RejectedOutOfRange
4 RejectedActorCannotAct       4 RejectedActorCannotAct
         │                               │
         ▼                               ▼
`private MoveOutcome last;`     `private MoveOutcome last;`
atanmamış hâl = bir RET         atanmamış hâl = "TAŞINDI"
zararsız: zaten hiçbir şey      ◄── ① KIRILMA NOKTASI: hiç
olmadı demektir                     hareket denenmeden ekran
                                    "taşındı" der
```

Sağdaki sütunda hatalı olan tek şey ① işaretli satırın YERİdir; beş adın beşi de
aynı, anlamları da aynı. Sıfır, dilin atanmamış her alana verdiği değer olduğu
için o satır bir isim değil, bir VARSAYILAN ANLAM seçer.

### REDDEDILEN

```csharp
Moved,                            // sıfırıncı değer BAŞARI olur
RejectedInvalidDestination,
```

**KIRILAN:** `default(MoveOutcome)` artık "taşındı" demek olur.

```
"private MoveOutcome lastOutcome;" -> sıfırla, yani "taşındı" doğar
hiç hareket denenmeden             -> ekran "taşındı" der
derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
```

**KAZANIRDI:** tip bir `[Flags]` maskesi olsaydı — o gün sıfır "hiç ret sebebi
yok" demek olurdu ve sebepler BİRLEŞEBİLİRDİ; bugün akış ilk redde duruyor, tek
sebep dönüyor.

### KAPSAM: kural "sıfır hep RET olsun" DEĞİL

Ölçüt tek: atanmamış hâlin doğal karşılığı ne? Sıfır, o karşılığı ADLANDIRAN
değere verilir.

```
MoveOutcome    atanmamış = "hiçbir şey denenmedi"  ► sıfır bir RET
AttackOutcome  aynı gerekçe                        ► sıfır bir RET
PointerPhase   atanmamış = "hiç basılmadı"         ► sıfır Idle
```

KARŞI ÖRNEK aynı ad alanında, `PointerGesture.cs`'in başındaki enum:
`PointerPhase.Idle = 0` bir ret değeri DEĞİLDİR ve olmamalıdır — orada atanmamış
bir alanın doğal karşılığı zaten "jest yok"tur ve sıfır tam da onu adlandırır. O
dosyanın özeti bu ayrımı açıkça yazıyor. Yani kural "sıfırı RET'e ayır" değil,
"sıfırı unutulmuş atamanın DOĞRU karşılığına ayır"dır.

### İŞ BÖLÜMÜ: sıra ile akışın TAMLIĞI bölüşür

Çağıranın eline yanlış bir sonuç geçmesini iki ayrı şey engelliyor ve ikisi iki
ayrı yoldan gelen değeri kapatıyor:

```
HİÇ atanmamış değer  ► bu sıralama       (alanın varsayılanı)
Execute'un DÖNDÜĞÜ   ► MoveAction'ın her dalda return etmesi
değer                  (üç ret + Moved)
```

Silinirse ne kırılır: sıra bozulursa okunmamış bir alan "taşındı" der ve akış hiç
çalışmadan yalan söyler. Akışın tamlığı bozulursa (bir dal sessizce düşerse)
derleyici zaten susmaz — bu yüzden ikisinden yalnız biri derleyici korumasına
sahiptir ve korumasız olan tam da bu satırdır.

**TEK CUMLE:** Sıfırıncı enum değeri bir isim değil, ATANMAMIŞ hâlin anlamıdır; o
hâl asla başarı olmamalı.

---

## RejectedCellOccupied

> Hedef hücrede başka bir birim duruyor.
> **Derin anlatım:** [04-karar sırası](../../konular/04-karar-sirasi.md)

### ÜÇ RET SEBEBİ TEK DEĞERE İNDİRİLEBİLİRDİ — İNDİRİLMEDİ

### HARİTA: sebep başına çağıranın davranışı

Ayıran ölçüt "üç ayrı şey oldu" değil; ret sebebinin bir TUR SONRA hâlâ geçerli
olup olmadığı. Sütunlar bunu gösteriyor:

```
ret sebebi                   bir tur sonra?  çağıranın işi
──────────────────────────   ─────────────   ─────────────────
RejectedInvalidDestination   ASLA değişmez   hücreyi bir daha
                                             hiç deneme  ◄── ①
RejectedCellOccupied         DEĞİŞEBİLİR     bekle, hücre boşalır
RejectedOutOfRange           DEĞİŞEBİLİR     önce YAKLAŞ, sonra dene
```

① işaretli satır tek başına bir kümede: kalıcı olan tek sebep o. Alttaki ikisi
geçici. Tek "Rejected" değeri bu çizgiyi siler ve çağıran üç satıra tek davranış
yazmak zorunda kalır — hangisini yazarsa yazsın öteki iki satırda yanlış olur.

### REDDEDILEN

Üç değer bire iner:

```csharp
Rejected,
Moved
```

**KIRILAN:** çağıran "asla gidilemez" ile "şimdilik gidilemez"i ayıramaz.

```
tahta dışı hücre -> yapay zekâ her turda yeniden dener
dolu hücre       -> kalıcı vazgeçer, oysa bir tur sonra boşalır
derleyici: hiçbir şey der  .  test: iki SIRA testi hedefsiz kalır
```

**KAZANIRDI:** sonucu yalnızca arayüz tüketseydi ve tek yaptığı şey geçersiz
tıklamada bir uyarı sesi çalmak olsaydı — üç değer aynı sesi çalmanın üç yolu
olurdu.

### KAPSAM: kural "her sebep kendi değerini alsın" DEĞİL

KARŞI ÖRNEK bu enum'un kendi içinde, son değer: `RejectedActorCannotAct` ÜÇ ayrı
sebebi (hareket eden düşmüş, saldıran düşmüş, sırası değil) bilerek TEK değerde
topluyor — ve bu doğru, çünkü üçünde de çağıranın yapabileceği tek şey aynı:
beklemek ya da başka birim seçmek. Aynı dosya, aynı enum, ters karar, aynı ölçüt.
Yani ayırıcı şey sebep sayısı değil, DAVRANIŞ sayısıdır.

### İŞ BÖLÜMÜ: değerler ile SIRA bölüşür

Bir hamlede birden çok ret sebebi aynı anda doğru olabilir (tahta dışı VE menzil
dışı). O yüzden iki ayrı mekanizma gerekiyor:

```
HANGİ sebepler ifade edilebilir ► bu enum'un üç ayrı değeri
Hangisi DÖNER                   ► MoveAction'daki kontrol sırası
```

Silinirse ne kırılır: değerler tek "Rejected"a inerse sıra kararı anlamsızlaşır —
hangi sebep kazanırsa kazansın çağıran aynı şeyi görür. Sıra kaybolursa değerler
anlamını korur ama cevap kararsız hâle gelir; sırayı tutan testler
`Execute_OccupiedCellOutOfRange_PrefersOutOfRange` ve
`Execute_OutsideBoardAndOutOfRange_PrefersInvalidDestination` tam olarak bunu
sabitliyor. İkisi yedek değil: biri kelimeleri, öteki cümleyi belirliyor.

**TEK CUMLE:** Ret sebepleri ancak çağıranın DAVRANIŞI değişiyorsa ayrılır;
burada "bekle" ile "vazgeç" tam olarak o davranış farkıdır.

---

## RejectedOutOfRange

> Hedef hücre tahtada ve boş, ama bu turda ulaşılamıyor.

Yukarıdaki tablonun üçüncü satırı: bir tur sonra DEĞİŞEBİLİR bir sebep. Çağıranın
işi "önce yaklaş, sonra dene". Bu değerin `RejectedCellOccupied`'dan ayrı
durmasının gerekçesi [bir üstteki bölümde](#rejectedcelloccupied), hangi sırayla
sorulduğunun gerekçesi ise [`MoveAction.md`](MoveAction.md)'de.

---

## Moved

> Birim tahtada eski hücresinden yeni hücresine geçti.

Enum'un tek KABUL değeri ve sırada üçüncü sırada duruyor. Başa alınmamasının
gerekçesi [`RejectedInvalidDestination`](#rejectedinvaliddestination)
bölümündedir: sıfır, unutulmuş bir atamanın anlamıdır ve o anlam asla başarı
olmamalı.

---

## RejectedActorCannotAct

> Hareket eden şu an eylem yapamaz: sırası değil ya da durumu elvermiyor
> (`MovementRules.CanMove` — bu tipin GÖREMEDİĞİ bir kural). Bu değeri yalnızca
> `GridStrategy.Battle` katmanı üretir.

### BEŞİNCİ DEĞER — VE SAHİBİ ONU ÜRETEMEZ

Bu enum'un dosyası `GridStrategy.Core`'da, akış sahibi `MoveAction` da orada; ama
ne `MovementRules` ne `UnitState` ne de `TurnState` Core'dan GÖRÜNÜR. Yani
aşağıdaki değeri döndürebilecek tek yer `GridStrategy.Battle`'daki
`BattleActions`'tır.

### HARİTA: değeri kim üretebilir

```
┌─ GridStrategy.Core (references: []) ────────────┐
│  MoveOutcome  ← tip BURADA tanımlı              │
│  MoveAction.Execute üretebildikleri:            │
│     RejectedInvalidDestination   ✓              │
│     RejectedCellOccupied         ✓              │
│     RejectedOutOfRange           ✓              │
│     Moved                        ✓              │
│     RejectedActorCannotAct       ✗ ◄── SORUNUN  │
│        cevabı MovementRules.CanMove'da ve o tip │
│        bu kutudan GÖRÜNMEZ; adını yazmak bile   │
│        derlenmez                                │
└──────────────────────┬──────────────────────────┘
                       │ Battle, Core'u görür
                       │ (ok tek yönlü)
┌─ GridStrategy.Battle ▼──────────────────────────┐
│  BattleActions.Move                             │
│     sıra kontrolü         ► değeri döndürür  ◄──┼─ ① TEK
│     MovementRules.CanMove ► değeri döndürür  ◄──┼─ ② ÜRETİCİ
│     ikisi de geçerse      ► akış Core'a iner    │
└─────────────────────────────────────────────────┘
```

Tipin TANIMLANDIĞI kutu ile değerin ÜRETİLDİĞİ kutu farklı; ① ve ② işaretli iki
dal, bu enum'un beşinci değerinin dünyadaki tek çıkış noktası.

Bu bir taviz ve öyle yazılıyor: bir tipe, sahibinin asla üretemeyeceği bir değer
eklendi. Gizlemek yerine SABİTLENDİ — `BattleActionsTests`'teki
`MoveAction_NeverReturnsRejectedActorCannotAct` hiçbir girdiyle bu değerin
Core'dan çıkmadığını tutuyor. Assembly sınırı bir KISITTIR, ve kısıtın izini
enum'da bırakmak onu unutulmaz kılar.

### ANLAMI TEK CÜMLE

Eylemi yapan taraf şu an eylem yapamaz. Üç ayrı sebebi birden kapsar — hareket
eden düşmüş, saldıran düşmüş, sırası değil. Kapsaması bilinçli: çağıranın
dallanması üçünde de aynıdır, çünkü hedef hücreyi değiştirmek hiçbirinde yardım
etmez. Ret sebebi, çağıranın YAPABİLECEĞİ bir şeyi göstermelidir; burada
yapılabilecek tek şey beklemek ya da başka bir birim seçmektir.

**Neden `AttackOutcome`'daki değerle AYNI AD:** iki enum ayrı ama çağıranın
sorusu tek — "bu birim şu an eyleyebilir mi". İki farklı ad, aynı cevabı iki kez
öğrenmek zorunda bırakırdı.

### EŞİK — bu değer ne zaman İKİYE ayrılır

Arayüz oyuncuya "sıran değil" ile "birim düşmüş" arasındaki farkı SÖYLEMEK
zorunda kaldığı gün. O gün ayrım Battle katmanına kendi sonuç tipiyle iner.
Bugün ayrılmıyor, çünkü tek tüketici `BoardAdapter` ve o yalnızca log basıyor.

### REDDEDILEN

Bu değer hiç doğmaz; `BattleActions` kendi sarmalayıcı sonuç tipini alır:

```csharp
public enum BattleRejection { None, NotYourTurn, ActorCannotAct }
public readonly struct BattleMoveOutcome { Rejection; Core; }
```

**KIRILAN:** bugün hiçbir çağıran o ikili ayrımı KULLANMIYOR.

```
her test outcome.Core yazar -> Rejection sütunu hep None
reddedilmiş hamlede         -> Core ne olacak, cevabı tip vermez
derleyici: hiçbir şey der  .  test: hepsi bilgisizce değişir
```

**KAZANIRDI:** yukarıdaki EŞİK aşıldığı gün — arayüz iki sebebi ayrı ayrı
söylemek zorunda kaldığında ikili şekil zaten doğru cevaptır ve o gün Core'un
enum'u bu değeri geri verebilir.

### KAPSAM: bu bir DESEN değil, tek bir taviz

"Tipe sahibinin üretemeyeceği bir değer ekle" genel bir kural DEĞİL; bu enum'da
beş değerin yalnız biri öyle.

```
Core'un üretebildiği   : { InvalidDestination, CellOccupied,
                           OutOfRange, Moved }
enum'un taşıdığı       : yukarıdakiler + { ActorCannotAct }
fark                   : { ActorCannotAct }   ← tek eleman
```

KARŞI ÖRNEK aynı dosyada, yukarıdaki dört değer: hepsinin sebebi Core'un
GÖREBİLDİĞİ şeylerden çıkar — tahtanın sınırı, hücrenin içeriği, iki koordinat
arasındaki uzaklık. Onlar için ek bir tip ya da ek bir kanıt gerekmez. Yeni bir
değer eklerken sorulacak tek soru: bu sebebi Core'daki bir tip söyleyebilir mi?
Söyleyebiliyorsa burada bir taviz yok, söyleyemiyorsa aynı borç yeniden doğar.

### İŞ BÖLÜMÜ: asmdef ile test ÖRTÜŞMEZ, BÖLÜŞÜR

"Bu değer Core'dan çıkmaz" sözünü iki ayrı mekanizma tutuyor ve ikisi iki farklı
kırılmayı kapatıyor:

```
KAZAYLA yazılması  ► asmdef: references boş, MovementRules
                     adı derlenmez  (derleme zamanı, kesin)
BAŞKA yoldan       ► BattleActionsTests'teki
sızması              MoveAction_NeverReturnsRejectedActorCannotAct
                     (çalışma zamanı, davranışsal)
```

Silinirse ne kırılır: asmdef'e Combat referansı eklenirse birinci koruma aynı gün
düşer ve kuralı tutan tek şey test kalır. Test silinirse bugün hiçbir şey
kırılmaz — ta ki referans eklenene kadar; o gün sızıntıyı söyleyecek kimse olmaz.
Biri kapıyı kilitliyor, öteki kapının kilitli kaldığını her koşuda ölçüyor.

### GARANTİ NEREDE BİTER

İkisi de bu değerin Core'dan ÇIKMAMASINI tutar; Battle katmanında doğru üretilip
üretilmediğini tutmaz. Orada sözü tutan şey `BattleActions`'ın kendi kontrol
sırasıdır.

**TEK CUMLE:** Bugün hiçbir çağıranın sormadığı bir ayrımı tipe yazmak, her
çağrıda anlamsız kalan bir alan üretir.
