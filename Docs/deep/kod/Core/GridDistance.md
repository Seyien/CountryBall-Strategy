# GridDistance

> **Kaynak:** `Assets/Game/Core/GridDistance.cs`
> **Ad alanı:** `GridStrategy.Core` · **Assembly:** `GridStrategy.Core` (`references: []`)
> **Rol:** Kural (Policy) — kimliği yok, hafızası yok, yalnızca ÖLÇER

İki hücre arasındaki uzaklığın tek sahibi. Bu dosyanın var olma sebebi tek cümle:
saldırı akışı mesafeyi hazır alıyordu ama projede o mesafeyi HESAPLAYAN kimse
yoktu. `AttackResolver` mesafeyi bilerek dışarıda bıraktı ("Manhattan mı,
Chebyshev mi" ayrı bir oyun kuralıdır dedi); işte o ayrı kural burada yaşıyor.

Tahtanın kaç hücre olduğunu, hücrede kimin durduğunu, arada engel olup
olmadığını, sıranın kimde olduğunu bilmez. Uzaklık ile ULAŞILABİLİRLİK farklı
sorulardır; burada yalnız ilki cevaplanır.

| Üye | Karar | Detay |
|---|---|---|
| `Between(int, int, int, int)` | ölçü tahtayı ALMAZ; bağımlılık oku tek yönlüdür | [↓](#betweenint-ax-int-ay-int-bx-int-by) |
| `Math.Max(dx, dy)` | Chebyshev: çapraz adım da BİR adımdır | [↓](#between-chebyshev) |

**İlgili anlatılar:** [04-karar sırası](../../konular/04-karar-sirasi.md) ·
[02-assembly duvarı](../../konular/02-assembly-duvari.md)

---

## Between(int ax, int ay, int bx, int by)

### ÖLÇÜ TAHTAYI ALMAZ: BAĞIMLILIK OKU TEK YÖNLÜDÜR

### HARİTA: kimin kimi tanıdığı

Tip tahtayı GÖRMEZ, yalnız iki koordinat çifti alır. Tahta dışı sayılar da
cevaplanır: `(-3, 9)` ile `(0, 0)` arası 9'dur. "Bu hücre var mı" sorusu
`UnitGrid.IsInsideGrid`'in işidir ve iki sorunun tek metotta birleşmesi ikisini
de test edilemez hâle getirirdi.

```
SEÇİLEN — ok yalnızca YUKARIDAN aşağı
  ┌──────────────┐
  │  MoveAction  │  akışı yürüten tek yer
  └───┬──────┬───┘
      │      └──────────────┐
      ▼                     ▼
┌───────────┐        ┌──────────────┐
│ UnitGrid  │        │ GridDistance │ ◄── dört int alır,
│ (VARLIK)  │        │   (KURAL)    │     hiçbir şey tanımaz
└───────────┘        └──────────────┘
       ▲                     ▲
       └── ARALARINDA OK YOK ┘   ◄── DURUŞ NOKTASI

REDDEDILEN — Between(UnitGrid board, ...)
┌───────────┐        ┌──────────────┐
│ UnitGrid  │ ◄──────┤ GridDistance │  yeni ok: KURAL artık
└───────────┘        └──────────────┘  bir VARLIK'a bağımlı
Sonuç: ölçüyü sınamak için önce bir tahta kurmak gerekir;
GridDistanceTests'in yedi testinin yedisi de "new UnitGrid(...)"
ile başlamak zorunda kalır.
```

### REDDEDILEN

```csharp
public static int Between(UnitGrid board, int ax, int ay, int bx, int by)
```

**KIRILAN:** KURAL bir VARLIK'ı tanımak zorunda kalır; uzaklığı sınamak için önce
bir tahta kurmak gerekir ve `GridDistanceTests`'teki her satır
`new UnitGrid(...)` ile başlar. `AttackResolver`'ın "mesafeyi dışarıda bırakma"
gerekçesi de anlamsızlaşır: mesafeyi almak tahtayı almaya döner.

```
derleyici: hiçbir şey der  .  test: yeşil kalır ama ağırlaşır
```

**KAZANIRDI:** uzaklık tahtaya BAĞLI olsaydı — kenarları birbirine dikilmiş
(toroidal) bir haritada iki uç arası 3 genişlikte 1'dir, çünkü sağ kenardan çıkıp
soldan girilir.

### KAPSAM: kural "hiçbir kural tahta almaz" DEĞİL

Yasak olan şey parametre almak değil, ÖLÇÜNÜN bir varlığa bağlanması. Ayırıcı
soru: cevap tahtanın DURUMUNA bakıyor mu?

```
GridDistance.Between  -> cevap yalnız dört sayıya bakar   ► tahta ALMAZ
UnitGrid.IsInsideGrid -> cevap tahtanın ÖLÇÜSÜNE bakar    ► tahtanın ÜYESİ
MoveAction.Execute    -> cevap tahtanın İÇERİĞİNE bakar   ► tahta ALIR
```

KARŞI ÖRNEK aynı ad alanında, `MoveAction.Execute`: ilk parametresi olarak
tahtayı ALIR ve bu doğrudur — "gidebilir mi" sorusunun cevabı hedef hücrede kimin
durduğuna bağlıdır. Aynı klasör, aynı ad alanı, aynı assembly; ters karar, aynı
ölçüt.

### İŞ BÖLÜMÜ: uzaklık ile ULAŞILABİLİRLİK bölüşür

"Oraya gidebilir mi" sorusu tek bir tipin cevaplayabileceği bir soru değil;
`MoveAction.Execute` üç ayrı sahibe sırayla sorar:

```
hücre VAR mı        ► UnitGrid.IsInsideGrid   (tahtanın ölçüsü)
kaç ADIM uzakta     ► GridDistance.Between    (bu dosya)
hücrede BAŞKASI var ► UnitGrid.TryGetUnit     (tahtanın içeriği)
```

Silinirse ne kırılır: `IsInsideGrid` gidince `(9,9)` gibi hiç var olmayan bir
hücre "menzil dışı" diye reddedilir ve sebep yanlış adlandırılır. Bu metot gidince
menzil sorusu hiç sorulamaz ve her ulaşılabilir hücre tahtanın tamamı olur. Üçü
yedek değil, üç ayrı red sebebi.

**TEK CUMLE:** Ölçü tahtayı almazsa tahtasız sınanabilir; aldığı gün ölçüm
olmaktan çıkıp tahtanın bir davranışı olur.

---

## Between: Chebyshev

### ÇAPRAZ BİR ADIMDIR: "BİTİŞİK" TANIMI BURADA KONUR

CHEBYSHEV seçildi: en uzun eksen kaç adımsa uzaklık odur, yani çapraz adım da bir
adımdır. Gerekçe bu projeye özgü: kareli bir taktik tahta ve
`AttackProfile.Range = 1` "bitişik hücre" demek.

### HARİTA: menzil 1 ile ulaşılan hücreler

Aynı tahta, aynı merkez (●), iki ölçü. Hücrelerdeki sayı o hücrenin merkeze
uzaklığı:

```
CHEBYSHEV — Math.Max        MANHATTAN — dx + dy
 ┌───┬───┬───┐               ┌───┬───┬───┐
 │ 1 │ 1 │ 1 │               │ 2 │ 1 │ 2 │ ◄── AYRIŞMA
 ├───┼───┼───┤               ├───┼───┼───┤
 │ 1 │ ● │ 1 │               │ 1 │ ● │ 1 │
 ├───┼───┼───┤               ├───┼───┼───┤
 │ 1 │ 1 │ 1 │               │ 2 │ 1 │ 2 │ ◄── AYRIŞMA
 └───┴───┴───┘               └───┴───┴───┘
  bitişik = 8 komşu           bitişik = 4 komşu
```

İki figür yalnızca DÖRT KÖŞEDE ayrışıyor ve ayrışma tam olarak menzil eşiğinin
üstünde: 1 ile 2 arasında. Menzili 1 olan bir birim için bu, "ulaşır" ile
"ulaşamaz" farkıdır.

3x5'lik bir tahtada bu fark küçük değil: Manhattan'da `(0,0)` köşesinin yalnızca
2 komşusu olur ve menzili 1 olan iki birim çaprazda karşılaşınca birbirine
dokunamadan tur harcar.

### REDDEDILEN

```csharp
return dx + dy;                        // Manhattan
```

**KIRILAN:** çapraz komşu bir adım değil iki adım uzaklaşır ve bitişiklik sekiz
komşudan dört komşuya iner.

```
menzili 1 olan birim -> çaprazdaki düşmana ulaşamaz
yapay zekâ bitişikken -> hâlâ "yaklaş" komutu üretir
derleyici: hiçbir şey der  .  test: _IsOneStepNotTwo kırmızı
```

**KAZANIRDI:** çaprazın BİLEREK yasak olduğu bir oyunda — kuşatma mekaniği "dört
yandan sarıldın" üstüne kuruluysa, ya da iki duvarın köşesinden geçmek geometrik
olarak saçmaysa.

### KAPSAM: seçim yalnızca ÇAPRAZ hücrelerde bağlayıcı

İki ölçü hücrelerin çoğunda AYNI sayıyı verir; ayrışma dar bir kümede. Kesişimi
al:

```
dx == 0 veya dy == 0  -> Math.Max(dx,dy) == dx+dy   ► AYNI
dx > 0 ve dy > 0      -> Math.Max(dx,dy) <  dx+dy   ► FARKLI
```

KARŞI ÖRNEK aynı metodun içinde, bir satır yukarıda: `(0,0)–(0,3)` için `dy`
sıfırdır ve iki ölçü de 3 der. Aynı şey `(2,2)–(2,2)` için de geçerli: ikisi de 0.
Yani bu blok "Manhattan yanlıştır" demiyor — yalnızca çapraz komşuluğun ne
sayılacağına karar veriyor.

Testlerin karşılığı da bu ayrımdır:
`Between_OrthogonalNeighbour_IsOneStep` iki ölçüde de yeşil kalır,
`Between_DiagonalNeighbour_IsOneStepNotTwo` yalnızca Chebyshev'de.

### İŞ BÖLÜMÜ: Abs ile Max ÖRTÜŞMEZ, BÖLÜŞÜR

Bu iki satırda iki ayrı karar var ve ikisi farklı soruyu kapatıyor:

```
Math.Abs  ► YÖN'ü siler: (0,0)→(3,0) ile (3,0)→(0,0) aynı
Math.Max  ► EKSEN birleştirmesini seçer: çapraz tek adım
```

Silinirse ne kırılır: `Abs` gidince negatif fark eksi bir uzaklık üretir ve
`Between_IsSymmetric` ile `Between_NegativeCoordinates_UseTheAbsoluteGap`
kırmızıya döner. `Max` yerine toplama gelirse simetri bozulmaz — kırılan tek şey
"bitişik" tanımıdır ve yalnızca çapraz testi düşer. İki ayrı mekanizma, iki ayrı
test kümesi.

**TEK CUMLE:** Mesafe ölçüsü bir matematik tercihi değil, "bitişik ne demek"
sorusunun cevabıdır; menzil o cevabın üstüne kurulur.
