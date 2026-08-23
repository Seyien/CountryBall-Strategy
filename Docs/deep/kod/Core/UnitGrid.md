# UnitGrid

> **Kaynak:** `Assets/Game/Core/UnitGrid.cs`
> **Ad alanı:** `GridStrategy.Core` · **Assembly:** `GridStrategy.Core` (`references: []`)
> **Rol:** Varlık (Entity) — kimliği var, hafızası var, tutar ve bildirir

Tahta: hangi hücrede kimin durduğunu tutan tek yer. Aynı 3x5 ölçüdeki iki tahta
aynı tahta değildir; hangi hücrede kimin durduğu örneğe aittir.

`UnityEngine.Grid` koordinat çevirir, bu tip tahtanın DURUMUNU tutar — aynı
kelime, iki ayrı sahip. `IsInsideGrid` kural gibi görünse de kendi ölçüsüne bakan
bir sorgudur, dışarıdan gelen bir politika değil: "dolu hücreye taşınamaz" bir
KURAL'dır ve sahibi `MoveAction`'dır.

| Üye | Karar | Detay |
|---|---|---|
| `UnitGrid(int, int)` | ölçünün pozitifliği burada doğrulanır | [↓](#unitgridint-width-int-height) |
| `Width` / `Height` / `CellCount` | ölçünün tek sahibi dizinin kendisidir; türetilir, saklanmaz | [↓](#width) |
| `IsInsideGrid(int, int)` | tahtanın kendi ölçüsüne bakan sorgu; sormak serbesttir | [↓](#isinsidegridint-x-int-y) |
| `PlaceUnit(int, int, Unit)` | tahta dışı gürültülü, dolu hücre sessiz | [↓](#placeunitint-x-int-y-unit-unit) |
| `RemoveUnit(int, int)` | tahtanın anahtarı koordinattır, kimlik değil | [↓](#removeunitint-x-int-y) |
| `MoveUnit(int, int, int, int)` | tek parça bir işlem, bileşim değil | [↓](#moveunitint-fromx-int-fromy-int-tox-int-toy) |
| `TryGetUnit(int, int, out Unit)` | yokluk imzanın kendisine yazılır | [↓](#trygetunitint-x-int-y-out-unit-unit) |

**İlgili anlatılar:** [03-tahta sahipliği](../../konular/03-tahta-sahipligi.md)

---

## UnitGrid(int width, int height)

İki ön koşul burada duruyor — `width <= 0` ve `height <= 0` — çünkü "tahtanın
ölçüsü pozitif olmalı" kuralının başka bir sahibi yok. Ölçü bu kurucuda dizinin
şekli olarak donar, ve o andan sonra ölçüyü SORAN her üye aynı diziden okur;
bunun neden bir kopyaya çevrilmediği [`Width`](#width) bölümünde.

---

## Width

(`Height` ve `CellCount` de aynı kararın parçası.)

### ÖLÇÜNÜN TEK SAHİBİ DİZİNİN KENDİSİDİR

### HARİTA: "iki sahip" burada ne demek

Ölçüyü dizinin KENDİSİNDEN oku, ikinci bir kopyasını saklama.

```
SEÇİLEN                        REDDEDILEN
╔═══════════════╗              ╔═══════════════╗
║ cells dizisi  ║              ║ cells dizisi  ║
║  gerçek ölçü  ║              ║  gerçek ölçü  ║
╚═══════╤═══════╝              ╚═══════╤═══════╝
        │ GetLength(0)                 │ (yeniden boyutlandırılır)
        ▼                              │        ✗ kopya güncellenmez
     Width  ── türetilmiş               ▼
     (kopya YOK)              ┌─────────────────┐
                              │ readonly width  │ ◄── AYRIŞMA
                              └────────┬────────┘     NOKTASI
                                       ▼
                                 Width ── eski sayıyı döndürür
```

Ayrı bir `width` alanı tutulsaydı aynı bilgi iki yerde yaşardı ve ikisini senkron
tutmak bir yükümlülük olurdu; dizi bir gün yeniden boyutlandırılırsa (tahta
büyümesi) alan sessizce eskirdi.

**Figürün SEÇİLEN sütunu, kaynakta:** `Assets/Game/Core/UnitGrid.cs` → `Width`

```csharp
public int Width => cells.GetLength(0);
```

Sol sütundaki «cells dizisi → `GetLength(0)` → Width ── türetilmiş (kopya YOK)»
okunun tamamı bu tek satır. Oku çizen şey `=>`: bir alan değil, her okumada
yeniden koşan bir gövde — bu yüzden `Width` bir kopya TUTAMAZ, ancak hesaplar.
`Height` ve `CellCount` de aynı şekilde yazılı, dolayısıyla üç üyenin üçü de aynı
tek kaynağa bakıyor.

**REDDEDİLEN sütunun kaynakta karşılığı YOK** ve olmaması kararın kendisidir: <!-- YOK-MUAF · DÜŞÜLDÜ · gerekçe aşağıdaki senette -->
`UnitGrid.cs`'te `width` adında bir alan hiç yazılmadı, tipin tek alanı `cells`.
Sağ sütundaki «readonly width ◄── AYRIŞMA NOKTASI» kutusu bu yüzden gözlenmiş bir
olgu değil, kaçınılmış bir gelecektir; o kutunun doğacağı koşulu bir sonraki
paragraf yazıyor (dizinin yeniden boyutlandırıldığı gün).

> ██ YOKLUK SENEDİ — DÜŞÜLDÜ ██ — saklanan `width` alanı
>
> **GEREKÇE:** Buradaki boşluk bir eksiklik değil bir KARARDIR. Reddedilen sütun
> aynı ölçüyü ikinci bir yerde saklıyor, oysa bu tipin tek alanı `cells` ve
> ölçünün tek sahibi o dizidir. ① dolmuyor: hiçbir oyun özelliği ikinci bir
> kopya istemez. Tahtanın büyümesi gerçek bir özelliktir ve bir gün istenebilir,
> ama o özellik saklanan alanı gerektirmiyor. Tam tersini yapıyor: bir üstteki
> figür ayrışma noktasını tam o gün için çiziyor, çünkü büyüyen dizi saklanan
> sayıyı bayatlatır.
>
> **④ HAYIR.** Reddedilen tasarımı zorunlu kılabilecek tek şey bir ölçüdür: dizi
> okumasının sahiplenilecek kadar pahalı olduğunu gösteren bir ölçü. O ölçü
> alınmadı, ve bunu bir alttaki «Alternatif» satırı zaten yazıyor. Ölçü
> alınmadan burada uydurulacak her özellik, kararı geriye çevirmek için
> uydurulmuş olurdu.

`=>`'nin ne vaat ettiği — ve aynı simgenin ikinci işi —
[dil/05](../../dil/05-deger-referans-ve-kimlik.md)'te.

**Alternatif:** ölçüyü ayrı `readonly int width`/`height` alanlarında saklamak.
Seçilmedi: aynı ölçü iki sahipli olur ve dizi okumasının maliyeti hiç ölçülmedi.

### KAPSAM: kural "hiçbir şeyi saklama" DEĞİL

Türetme, olgunun BAŞKA bir sahibi varsa doğrudur. KARŞI ÖRNEK bir satır yukarıda:
`cells` dizisinin kendisi saklanıyor ve saklanmak zorunda — hangi hücrede kimin
durduğunun kaynağı başka hiçbir yerde yok, tahtanın kendisi o olgunun doğduğu
yer. Aynı dosyada `CellCount` da türetilmiş (`Width * Height`) ve
`UnitGridTests`'teki `CellCount_IsDerived_NotStoredSeparately` bunu tutuyor.

### İŞ BÖLÜMÜ: Width ile IsInsideGrid aynı sayıyı ayrı sorulara çeviriyor

```
dışarıya ÖLÇÜ bildirmek    ► Width / Height / CellCount
içeride SINIR uygulamak    ► IsInsideGrid ve ThrowIfOutsideGrid,
                             ikisi de GetLength'i doğrudan okur
```

İkisi de aynı kaynağa bakar; bu yüzden dışarıya söylenen ölçü ile içeride
uygulanan sınırın ayrışması İMKÂNSIZDIR. `Width` bir alandan okusaydı bu iki cevap
birbirinden kopabilirdi ve tahta, "genişliğim 5" derken 4'te reddeden bir şeye
dönerdi.

---

## IsInsideGrid(int x, int y)

Tahtanın kendi ölçüsüne bakan bir SORGU, dışarıdan gelen bir politika değil — rol
künyesi bunu açıkça yazıyor, ve `MoveAction`'ın "kural tahtaya taşınmaz" bloğu
aynı ayrımı ters yönden doğruluyor: cevabı dizinin boyutunda yazılı olduğu için
bu üye tahtada KALIR.

Sormak serbesttir: tahta dışı bir koordinat için fırlatmaz, `false` döner. Bu
serbestlik `TryGetUnit`'in de dayanağıdır ve ikisinin farkını
`IsInsideGrid_SeparatesOutsideFromEmpty_WhereTryGetUnitCannot` sabitliyor —
detayı [`TryGetUnit`](#trygetunitint-x-int-y-out-unit-unit) bölümünde.

---

## PlaceUnit(int x, int y, Unit unit)

### TAHTA DIŞI GÜRÜLTÜLÜ, DOLU HÜCRE SESSİZ

Tahta dışına yerleştirmek bir ÇAĞIRAN hatasıdır, bir oyun sonucu değil; bu yüzden
burada gürültüyle patlar. `TryGetUnit` ise sessiz kalır, çünkü "bu hücrede kimse
yok" normal bir cevaptır ve kenar taramalarında her kare defalarca sorulur. Aynı
sınıf, iki farklı hata felsefesi — ve ayırıcı şey teknik değil, sorunun KİME AİT
olduğudur.

### HARİTA: çizgi tahtanın KENARINDAN geçiyor

3x2'lik bir tahta. İki ayrı "beklenmedik" durum ve iki ayrı cevap:

```
        x=0     x=1     x=2   ╎  x=3
      ┌───────┬───────┬───────┐╎
  y=1 │       │   A   │       │╎  ◄── ① (3,1) TAHTADA YOK
      ├───────┼───────┼───────┤╎      PlaceUnit -> fırlatır
  y=0 │   B   │       │       │╎      (ArgumentOutOfRange)
      └───────┴───────┴───────┘╎
          ▲                    ╎
          └── ② (0,0) DOLU: PlaceUnit(0,0,C) çağrılırsa B'nin
              üstüne sessizce yazılır, hiçbir şey söylenmez
```

① ile ② arasındaki çizgi teknik değil, SAHİPLİK çizgisidir: ① yalnızca çağıranın
hesabından çıkabilir, tahtanın kendisi öyle bir koordinat üretmez. ② ise oyunun
her turunda doğal olarak olan bir şeydir. Çizgi tahtanın kenarından geçer —
içeride sessizlik, dışarıda gürültü.

### REDDEDILEN

```csharp
public bool TryPlaceUnit(int x, int y, Unit unit)   // tahta dışı -> false
```

**KIRILAN:** dönen `bool` yok sayılabilir ve kod yine derlenir.

```
çağıran sonucu okumaz -> birim hiçbir hücreye yazılmaz
ekranda karşılığı yok -> birim sessizce kaybolur
BoardAdapter'ın "önce KURAL, sonra görsel" sırası anlamsızlaşır
derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
```

**KAZANIRDI:** yerleştirmeyi KULLANICI tetikliyorsa (sürükle-bırak ile tahta
dışına bırakmak) — o gün tahta dışı, sık ve beklenen bir oyun sonucudur.

### KAPSAM: kural "tahta dışı hep fırlatır" DEĞİL

Aynı koordinat, hangi metoda verildiğine göre iki farklı muamele görür ve bu bir
tutarsızlık değil:

```
PlaceUnit(3,1,u)   YAZMA denemesi  -> fırlatır  (çağıran hatası)
TryGetUnit(3,1,..) SORU            -> false döner, sessiz
```

KARŞI ÖRNEK bu dosyanın sonunda, `TryGetUnit`: tahta dışı koordinat için
fırlatmaz, yalnızca `false` döner — çünkü sormak bir hata değildir ve kenar
taraması her karede tahta dışını sorar. `UnitGridTests` bu ayrımı ikiye bölerek
tutuyor: `PlaceUnit_WhenPositionIsOutsideGrid_ThrowsArgumentOutOfRange` ile
`TryGetUnit_WhenPositionIsOutsideGrid_ReturnsFalseAndOutputIsNull`. Ölçüt yön:
YAZMAK niyet bildirir, SORMAK bildirmez.

### İŞ BÖLÜMÜ: iki mekanizma, iki ayrı yanlış

```
ThrowIfOutsideGrid ► var OLMAYAN hücreyi kapatır (çağıran hatası)
dönüş değeri/bool  ► var olan ama BOŞ/DOLU hücreyi anlatır (oyun olgusu)
```

Silinirse ne kırılır: sınır kontrolü gidince `IndexOutOfRangeException` dilin
kendi hatası olarak çıkar — hangi ARGÜMANIN suçlu olduğunu söylemez ve çağıran
onu bir oyun durumu sanıp yutabilir. Sessizlik tarafı gürültüye çevrilirse (dolu
hücre için fırlatmak) tahta, sahibi olmadığı bir kuralı — "dolu hücreye yazılamaz"
— uygulamaya başlar; o kural `MoveAction`'ındır ve iki yerde yaşarsa ayrışır.

**TEK CUMLE:** Tahta dışı koordinat oyunun değil ÇAĞIRANIN hatasıdır, ve çağıran
hatası dönüş değeriyle değil gürültüyle bildirilir.

---

## RemoveUnit(int x, int y)

`RemoveUnit` ve `MoveUnit` hangi felsefeye ait? Ayırıcı çizgi bu dosyada
OKUMA/YAZMA değil — ve bunu görmenin yolu `PlaceUnit`'e dikkatle bakmak: tahta
dışına yazmayı gürültüyle reddeder, ama DOLU bir hücrenin üstüne sessizce yazar.
Yani KOORDİNAT çağıranın sorumluluğudur (gürültülü), HÜCRENİN İÇERİĞİ ise bir
oyun olgusudur (sessiz). İki metot aynı çizgiyi izler: tahta dışı koordinat
patlar, boş hücreyi boşaltmak ya da dolu hücrenin üstüne taşımak hiçbir şikâyet
üretmez.

"Dolu hücreye taşınamaz" bir KURAL'dır ve sahibi `MoveAction`'dır. Buraya da
konsaydı aynı kural iki yerde yaşardı; ikisi ayrışınca hangisinin doğru olduğunu
derleyici söyleyemezdi.

### TAHTANIN ANAHTARI KOORDİNATTIR, KİMLİK DEĞİL

### HARİTA: anahtarı ters çevirmek aramayı doğurur

```
SEÇİLEN — RemoveUnit(x, y)
  çağıran (1,0) der ──► cells[1,0] = null
  ┌───────┬───────┬───────┐
  │       │  ✗    │       │   tek dizin, tek işlem;
  ├───────┼───────┼───────┤   "bulamadım" diye bir hâl YOK
  │       │       │       │
  └───────┴───────┴───────┘

REDDEDILEN — RemoveUnit(Unit unit)
  çağıran birimi verir ──► tahtanın TAMAMI taranır
  ┌───────┬───────┬───────┐
  │  1 →  │  2 →  │  3 →  │   çağıran koordinatı ZATEN
  ├───────┼───────┼───────┤   biliyordu; yine de her hücre
  │  4 →  │  5 ✓  │  6    │   okunur
  └───────┴───────┴───────┘
          ▲
          └── ve tarama boş dönerse: "bu birim tahtada yoktu"
              ile "sildim" AYNI dönüşe düşer  ◄── AYRIŞMA
              NOKTASI: metot void, ikisini ayıracak bir şey yok
```

`MoveUnit` aynı aramayı iki kez yapmak zorunda kalırdı: bir kez kaynağı bulmak,
bir kez hedefi doğrulamak için.

### REDDEDILEN

```csharp
public void RemoveUnit(Unit unit)   // koordinat değil KİMLİK ile
```

**KIRILAN:** çağıranın zaten bildiği bir koordinat için tahta baştan sona taranır;
"bulamadım" ile "sildim" ayırt edilemez ve `MoveUnit` aynı aramayı iki kez yapmak
zorunda kalır.

```
derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
```

**KAZANIRDI:** silmeyi tetikleyen yer birimi TANIYIP hücresini BİLMİYORSA — "ölen
her birimi tahtadan kaldır" gibi bir süpürme, ölüm olayından yalnız `Unit` alır.

### KAPSAM: kural "kimlik anahtar olmaz" DEĞİL

Anahtarı seçen şey, çağıranın ELİNDE NE OLDUĞUdur:

```
çağıranın elinde koordinat var  ► koordinat anahtar  (bu tip)
çağıranın elinde yalnız birim   ► kimlik anahtar     (yan tablolar)
```

KARŞI ÖRNEK aynı ad alanında, `MoveAction.Execute`: o metot hem koordinatı hem
birimi alır ve kimliği bir ANAHTAR olarak değil bir DOĞRULAMA olarak kullanır
(`ReferenceEquals` ile "kaynak hücrede gerçekten o mu duruyor"). Yani kimlik
Core'da da dolaşır; yalnızca tahtanın anahtarı olmaz.

### İŞ BÖLÜMÜ: iki anahtar uzayı, iki ayrı soru

```
"burada kim var"    ► koordinat anahtarlı `cells` dizisi (bu tip)
"bu birim nasıl"    ► Unit anahtarlı yan tablolar (Battle ve
                      BoardAdapter tarafında)
```

Silinirse ne kırılır: tahta kimlikle anahtarlanırsa "burada kim var" sorusu bir
aramaya döner ve her kare tekrarlanır. Yan tablolar koordinatla anahtarlanırsa her
hareket bütün tabloların anahtarını güncellemeyi gerektirir — bir birimi taşımak
dört sözlüğü birden yeniden yazmak olur. İki anahtar birbirinin yedeği değil, iki
ayrı sorunun cevabı.

### GARANTİ NEREDE BİTER

Bu tip aynı `Unit` örneğinin İKİ hücrede birden durmasını engellemez — `PlaceUnit`
yazdığı hücreye bakar, tahtanın geri kalanına bakmaz. Tekillik tahtanın değil,
akışı yürüten `MoveAction`'ın (ve bir üst katmandaki `BattleActions`'ın)
disiplinidir.

**TEK CUMLE:** Tahtanın anahtarı koordinattır; kimlikle silmek anahtarı tersine
çevirir ve her silmeyi bir aramaya döndürür.

---

## MoveUnit(int fromX, int fromY, int toX, int toY)

### MoveUnit TEK PARÇA BİR İŞLEMDİR, BİLEŞİM DEĞİL

`MoveUnit`, `RemoveUnit` + `PlaceUnit` BİLEŞİMİ DEĞİL, tek parça bir işlem. Sebep
tek kelimeyle: YARIM KALMA. Bileşimde silme başarılır, yazma patlar ve arada birim
hiçbir hücrede durmayan bir hayalete döner.

### HARİTA: aradaki hâl

İki adımın ARASINDA tahta hangi hâlde? Zaman soldan sağa akıyor:

```
REDDEDILEN — RemoveUnit(0,0); PlaceUnit(9,9,u);
  t0            t1                    t2
  ┌───┬───┐     ┌───┬───┐             fırlatır
  │ u │   │ ──► │   │   │ ──────────► (9,9 tahtada yok)
  └───┴───┘     └───┴───┘
  geçerli       ◄── GEÇERSİZ HÂL:     tahta t1'de DONAR
                u hiçbir hücrede yok  u kalıcı olarak kayıp

SEÇİLEN — MoveUnit(0,0,9,9)
  t0            (iki koordinat da ÖNCE doğrulanır)
  ┌───┬───┐     fırlatır — tahtaya hiç dokunulmadı
  │ u │   │ ──► ┌───┬───┐
  └───┴───┘     │ u │   │  t0 ile aynı: ara hâl HİÇ DOĞMADI
                └───┴───┘
```

Fark bir sıralama zarafeti değil: soldaki şekilde geçersiz hâl ENGELLENMİYOR,
sağdaki şekilde DOĞMUYOR. İşaretli `t1`, bileşimin çağırana verdiği tek şeydir ve
geri alınamaz.

### REDDEDILEN

```csharp
RemoveUnit(fromX, fromY);  sonra  PlaceUnit(toX, toY, moving);
```

**KIRILAN:** silme başarılır, yazma patlar, birim hiçbir hücrede kalmaz.

```
hedef tahta dışı -> birim tahtadan TAMAMEN silinmiş kalır
kaynak hücre boş -> hedefe null yazılır, oradaki birim gider
derleyici: hiçbir şey der  .  test: ThrowsAndKeepsUnit ve
LeavesDestinationUntouched kırmızıya döner
```

**KAZANIRDI:** ayrılma ve varış GERÇEKTEN iki ayrı olaysa — birim çıkarken hücreye
bir iz (kontrol alanı, tuzak tetiği) bırakıyor ve varışta ayrı bir şey
tetikliyorsa.

### KAPSAM: kural "her ikili tek metot olsun" DEĞİL

Ölçüt tek: iki adımın ARASINDA geçersiz bir tahta hâli doğuyor mu?

```
RemoveUnit tek başına  -> ara hâl yok, sonuç zaten "hücre boş"  ► ayrı kalır
PlaceUnit tek başına   -> ara hâl yok, sonuç zaten "hücre dolu" ► ayrı kalır
ikisi ARDIŞIK          -> arada birim hiçbir hücrede yok        ► birleşir
```

KARŞI ÖRNEK bu dosyanın kendi içinde: `RemoveUnit` ile `PlaceUnit` public kalmaya
devam ediyor ve bu doğru — tek başlarına çağrılan her biri tahtayı bir geçerli
hâlden başka bir geçerli hâle taşır. Yani birleştirilen şey metotlar değil,
ARALARINDAKİ boşluktur.

### İŞ BÖLÜMÜ: doğrulama SIRASI ile yazma SIRASI

"Yarım kalma yok" sözünü iki ayrı satır grubu tutuyor ve ikisi iki ayrı senaryoyu
kapatıyor:

```
iki ThrowIfOutsideGrid ► tahtaya DOKUNMADAN önce fırlatmak
                         (hedef geçersizken kaynağın boşalmaması)
önce kaynağı boşalt,   ► aynı hücreye taşımada birimin
sonra hedefe yaz         silinmemesi (fromX==toX && fromY==toY)
```

Silinirse ne kırılır: doğrulama sırası bozulursa
`MoveUnit_WhenDestinationIsOutside_ThrowsAndKeepsUnit` kırmızıya döner — birim
tahtadan tamamen silinmiş kalır. Yazma sırası ters çevrilirse
`MoveUnit_ToTheSameCell_KeepsTheUnitInPlace` kırmızıya döner: birim hedefe yazılır,
hemen ardından aynı hücre boşaltılır. İki mekanizma, iki ayrı test; hiçbiri
ötekinin yedeği değil.

**TEK CUMLE:** İki adımın ARASINDA geçerli olmayan bir tahta hâli varsa o iki adım
tek bir işlemdir; bileşim yarım kalmayı mümkün kılar.

---

## TryGetUnit(int x, int y, out Unit unit)

### TRY ŞEKLİ: YOKLUK İMZANIN KENDİSİNE YAZILIR

Nullable-return şekli (`FindUnit`) uygulandı, ölçüldü ve kaldırıldı: `<Nullable>`
KAPALI olduğu için null dönüş derleme zamanında hiçbir koruma taşımaz — çağıran
null kontrolünü unutursa derleyici susar. Bu şekil ise çağıranı dallanmaya ZORLAR.

### HARİTA: çağrı yerinde ihmal nasıl görünür

```
REDDEDILEN — Unit FindUnit(int x, int y)
  Unit u = board.FindUnit(1, 2);
  Log(u.Name);        ◄── İHMAL GÖRÜNMEZ: doğru yazılmış kodla
       │                  bu satır HARFİ HARFİNE aynıdır
       ▼
  Play'de NullReferenceException

SEÇİLEN — bool TryGetUnit(int x, int y, out Unit unit)
  if (board.TryGetUnit(1, 2, out Unit u))
  {                   ◄── DURUŞ NOKTASI: "cevap yok" ihtimali
      Log(u.Name);        imzanın kendisinde, dönüş tipinde
  }                       duruyor; atlamak için onu görüp
                          ATMAK gerekir
```

Fark, yokluğun NEREDE yazılı olduğudur: solda dönüş tipinin içinde gizli (`Unit`
hem "var" hem "yok" demek), sağda ayrı bir `bool` olarak dışarıda.

### DÜRÜST NOT — bu bir KARAR değil, bir VARSAYILAN

`<Nullable>` hiç açılmadı. Unity 2021.3'te varsayılan `disable`'dır ve bu projede
onu açacak hiçbir şey yok (ne `Assets/csc.rsp`, ne asmdef alanı, ne
`ProjectSettings`). Yani "kapattık" demek yanlış olur — dokunmadık.

Açmanın maliyeti katmana göre değişir: Core'da (`noEngineReferences`) ucuzdur
çünkü ortada anotasyonsuz Unity API'si yok; Unity katmanında ise
`GetComponent`/`Find` gibi çağrılar anotasyonsuz olduğu için uyarı gürültüsü
üretir. Yani "Core'da aç, Unity'de açma" gerçek bir seçenek.

### REDDEDILEN

```csharp
public Unit FindUnit(int x, int y)   // hücre boşsa null döner
```

**KIRILAN:** `<Nullable>` kapalıyken null dönüş HİÇBİR derleyici koruması taşımaz;
dallanmaya zorlayan tek şey `out` parametresidir.

```
null kontrolü unutulur -> unit.Name yazılır -> Play'de patlar
derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
```

**KAZANIRDI:** `<Nullable>` açılırsa — o gün `Unit?` dönüşü gerçek bir koruma
TAŞIR; ayrıca sonuç bir DALLANMADA değil bir İFADE içinde gerekiyorsa
(`FindUnit(x, y)?.Name ?? "boş"`) null dönüş kazanır.

### KAPSAM: kural "her sorgu Try olsun" DEĞİL

Ölçüt tek: cevabın YOK olma ihtimali var mı, ve kaç tane cevap var?

```
TryGetUnit           yokluk mümkün, iki hâl   ► Try şekli
GridDistance.Between yokluk imkânsız          ► düz int döner
MoveAction.Execute   dört ayrı sonuç          ► enum döner
```

KARŞI ÖRNEK aynı ad alanında, `GridDistance.Between`: dört `int` alır, bir `int`
döndürür ve hiçbir Try şekline ihtiyacı yoktur — iki hücre arasında "uzaklık yok"
diye bir hâl yoktur. Aynı şekilde `MoveAction.Execute` de `bool`/`out` değil enum
döndürür, çünkü orada ayrılması gereken şey iki değil dört hâldir. Try şekli
yokluk için, enum çokluk için.

### İŞ BÖLÜMÜ: iki `false` AYNI ŞEY DEĞİL

Bu metot iki ayrı sebeple `false` döner ve ikisini iki ayrı satır üretir:

```
!IsInsideGrid(x, y)  ► hücre VAR OLMADIĞI için  (dizi taşması
                       yok; erişim hiç denenmez)
unit != null         ► hücre var ama BOŞ olduğu için
```

Silinirse ne kırılır: ilk kontrol gidince tahta dışı bir sorgu
`IndexOutOfRangeException` fırlatır ve "sormak serbesttir" sözü düşer;
`TryGetUnit_WhenPositionIsOutsideGrid_ReturnsFalseAndOutputIsNull` kırmızıya döner.
İkinci satır `return true`ya düşerse boş hücre dolu sayılır.

### GARANTİ NEREDE BİTER

Bilerek kabul edildi: iki sebep TEK bir `false`'a düşer, yani bu metot "tahta
dışı" ile "boş" arasındaki farkı çağırana SÖYLEYEMEZ. Farkı sormak isteyen
`IsInsideGrid`'i ayrıca çağırmak zorundadır — `UnitGridTests`'teki
`IsInsideGrid_SeparatesOutsideFromEmpty_WhereTryGetUnitCannot` tam olarak bu
sınırı sabitliyor.

### `out` TEK BAŞINA DERLEYİCİ KORUMASI DEĞİLDİR

Dürüst olmak gerekirse: dönüş değerini yok sayıp `out` değişkenini doğrudan okumak
da derlenir. Yani koruma `out`un dilsel gücünden değil, ŞEKLİN
GÖRÜNÜRLÜĞÜNDEN gelir — atılan bir `bool` çağrı yerinde göze çarpar, unutulmuş bir
null kontrolü çarpmaz. Gerçek derleyici koruması ancak `<Nullable>` açıldığı gün
gelir; bugün dokunulmamış bir varsayılan olarak kapalı duruyor.

**TEK CUMLE:** Try deseni bir üslup değil, derleyicinin veremediği "önce sor"
zorunluluğunu imzanın kendisine yazmanın tek yoludur.
