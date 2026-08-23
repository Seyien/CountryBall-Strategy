# MoveProfile

> **Kaynak:** `Assets/Game/Core/MoveProfile.cs`
> **Ad alanı:** `GridStrategy.Core` · **Assembly:** `GridStrategy.Core` (`references: []`, `noEngineReferences: true`)
> **Rol:** Tanım (Value/Definition) — kimliği yok, hafızası yok, karar vermez; sayıyı TAŞIR

Bir hareket türünün değişmez tanımı: "süvari 3 hücre, piyade 1". `MoveAction`'ın
bugün çıplak bir `int` olarak da aldığı hareket menzilinin sahibi — o dosyada
"`AttackProfile`'ın ikizi olan bir `MoveProfile`" diye adı konmuş tipin kendisi.

Neyi TUTMAZ: kimin hareket ettiğini, bu turda daha önce hareket edilip
edilmediğini, yolun üzerinde ne olduğunu, birimin durumunu. Sonuncusu bilerek:
"düşmüş birim hareket edebilir mi" sorusunun sahibi Combat katmanındaki
`MovementRules`'tır ve bu tip onu tip olarak bile yazamaz.

| Üye | Karar | Detay |
|---|---|---|
| `MoveProfile` (tip) | `sealed class` seçildi; dosya Core'da, ikizi Combat'ta | [↓](#moveprofile-tip) |
| `MoveProfile(int range)` | sıfır menzil GEÇERLİ; eşik kopyalanmaz, gerekçesi taşınır | [↓](#moveprofileint-range) |
| `Range` | bir turda kaç hücre; ölçünün nasıl yapıldığını bilmez | [↓](#range) |

**İlgili anlatılar:** [02-assembly duvarı](../../konular/02-assembly-duvari.md)

---

## MoveProfile (tip)

### NEDEN CORE'DA, İKİZİ AttackProfile GİBİ COMBAT'TA DEĞİL

Hareket menzili TAHTAYA ait bir kavramdır, saldırı menzili SAVAŞA. Hareketin
ihtiyacı olan her şey — hücre, uzaklık, sınır — zaten Core'da yaşıyor; hasarın,
takımın, yaşam döngüsünün hareketle hiçbir işi yok. Bu yüzden `AttackProfile`
Combat'ta KALIR: onun taşıdığı `Damage` sayısının Core'da karşılığı olan bir
kavram bile yoktur.

Kararın mekanik yüzü de aynı yere çıkıyor: `MoveAction` Core'da ve Core, Combat'ı
GÖRMEZ. Profil Combat'ta doğsaydı `MoveAction` onu parametre olarak alamazdı;
almak için Core'un Combat'a referans vermesi gerekirdi ve iki assembly'yi ayrı
tutmanın bütün gerekçesi çöpe giderdi.

Yani ikiz, ikizinin bir kat ALTINDA yaşıyor. Bu asimetri bir kusur değil, kararın
kendisidir.

---

### SINIF SEÇİLDİ: KURUCU ATLANABİLİR OLMAMALI

### HARİTA: bir örneğe giden yollar

Soru "kopyalanır mı" değil, "kurucuya UĞRAMADAN bir örnek doğabilir mi". İki
şeklin kapı sayısı farklı:

```
SEÇİLEN — sealed class
  new MoveProfile(3) ──► ┌──────────────┐
                         │ range < 0 ?  │ ◄── TEK KAPI
                         └──────┬───────┘
                                ▼
                         ╔══════════════╗
                         ║ Range = 3    ║
                         ╚══════════════╝
  `default` bir örnek DEĞİL, null referanstır -> ikinci kapı yok

REDDEDILEN — readonly struct
  new MoveProfile(3) ──► ┌──────────────┐
                         │ range < 0 ?  │
                         └──────┬───────┘
                                ▼
                         ╔══════════════╗
  default(MoveProfile) ──►║ Range = 0    ║ ◄── İKİNCİ KAPI:
  new MoveProfile[8]  ──►╚══════════════╝     kurucu HİÇ ÇALIŞMAZ
  bir sınıfın atanmamış alanı ─┘
```

Kritik nokta işaretli kapıda: dilin kendisi, parametresiz bir struct örneğini
bütün alanları sıfırlayarak üretir ve buna izin vermemenin bir yolu yoktur. Bu
bir kod kusuru değil, DEĞER TİPİ semantiğinin kendisidir — struct seçmek o kapıyı
açmakla eş anlamlıdır.

### REDDEDILEN

```csharp
public readonly struct MoveProfile
```

**KIRILAN:** `AttackProfile`'ın struct REDDEDILEN bloğundaki gerekçe (null
koruması derlenmez olur) burada AYNI DEĞİL; asıl bedel SIFIRIN ANLAMLI OLMASI.

```
default(MoveProfile) -> kurucu atlanır, Range sıfır doğar
sıfır "kök salmış" demek -> kımıldamayan birim kusursuz derlenir
derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
```

**KAZANIRDI:** profil paylaşılmayıp birim başına saklansaydı ve yol bulucu her
karede binlerce hücre için okusaydı — o gün struct kazanırdı, ama önce sıfırın
"atanmadı" ile karışmayacağı bir yol bulunmalıdır.

### KAPSAM: kural "struct kullanma" DEĞİL

İkinci kapı ancak SIFIR HÂLİ GEÇERLİ GÖRÜNÜYORSA zarar verir. Ayırıcı soru: bütün
alanları sıfır olan bir örnek, kurucunun kabul edeceği bir değere mi benziyor?

```
MoveProfile   sıfır = "kımıldayamaz"  -> GEÇERLİ görünür  ► tehlikeli
PointerPhase  sıfır = Idle            -> zaten doğru hâl  ► zararsız
MoveOutcome   sıfır = bir RET değeri  -> bilerek öyle     ► zararsız
```

KARŞI ÖRNEK aynı ad alanında ve aynı ailede: `PointerGesture.cs`'teki
`PointerPhase.Idle = 0` bir DEĞER tipidir, varsayılanı serbestçe doğar ve orada
bu bir kusur değil karardır — "hiç basılmadı", yeni kurulmuş bir jestin gerçek
hâliyle birebir aynıdır. Yani atlanabilen kurucu her yerde değil, yalnız sıfırın
YALAN SÖYLEDİĞİ yerde borçtur.

### İŞ BÖLÜMÜ: üç mekanizma, üç ayrı delik

`Range`'in her okumada geçerli bir sayı olmasını üç ayrı şey tutuyor:

```
sealed class       ► kurucusuz örnek doğmasını engeller (yukarıdaki kapı)
range < 0 kontrolü ► geçersiz ARGÜMANI kurucuda keser
get-only property  ► kuruluştan SONRA değişmeyi keser
```

Silinirse ne kırılır: sınıf struct olursa ilk kapı açılır ve öteki ikisi bunu
göremez. Kurucu kontrolü gidince `new MoveProfile(-4)` geçer ve
`Constructor_NegativeRange_Throws` kırmızıya döner. Property `set` alsaydı doğru
kurulmuş bir profil sonradan bozulurdu. Üçü aynı şeyi iki kez yapmıyor; üç ayrı
deliği kapatıyor.

### HANGİ MEKANİZMA BU KAPIYI KAPATMAZ

`readonly` ve get-only property'nin ikisi de bu kırılmaya karşı SIFIR koruma
sağlar: ikisi de DEĞİŞMEYİ engeller, DOĞUŞU değil. Sıfır `Range` zaten doğduğu
anda oradadır ve hiç değiştirilmez — yani her iki mekanizma da onu kusursuz bir
değer sanır. Korumayı veren tek şey referans tipi olmaktır.

**TEK CUMLE:** struct'ın bedeli KOPYALANMASI değil kurucusunun ATLANABİLMESİDİR,
ve varsayılan değer geçerli bir değerse bunu kimse fark etmez.

---

### DOSYANIN YERİ: OK YÖNÜ, KLASÖR ZEVKİ DEĞİL

**Alternatif:** dosyayı `Combat/` altına, `AttackProfile`'ın yanına koymak.
Seçilmedi: Core, Combat'ı görmez ve `MoveAction` profili parametre olarak alamaz
— menzil bir gün SAVAŞ DURUMUNA bağlanana kadar da böyle kalır.

### HARİTA: asmdef okları ve olmayan ok

Bu ağacı klasörler değil, asmdef'lerin `references` dizileri kurar;
`GridStrategy.Core`'unki BOŞ bir dizidir.

```
┌──────────────────┐        ┌────────────────────┐
│ GridStrategy.Core│        │ GridStrategy.Combat│
│  MoveAction      │        │  AttackProfile     │
│  MoveProfile ◄───┼── burada│  AttackRules      │
└──────────────────┘        └────────────────────┘
         ▲   ✗ OK YOK — references: []   ◄── DURUŞ NOKTASI
         └───────────────────────┘
Profil sağdaki kutuda doğsaydı, soldaki MoveAction onu parametre
olarak yazamazdı: tip adı derlenmezdi.
```

### KAPSAM: "menzil" kelimesi tek başına yer belirlemez

KARŞI ÖRNEK ikizin kendisi: `AttackProfile` aynı kelimeyi (`Range`) taşır ve
Combat'ta KALIR — çünkü yanında `Damage` vardır ve hasarın Core'da karşılığı olan
bir kavram yoktur. Ayırıcı ölçüt kelime değil, tipin ihtiyaç duyduğu kavramların
hangi kutuda yaşadığıdır.

### İŞ BÖLÜMÜ: asmdef ile klasör ÖRTÜŞMEZ

```
bağımlılığı UYGULAYAN   ► asmdef'in references dizisi (derleme
                          zamanı; ihlal derlenmez)
okunurluğu SAĞLAYAN     ► klasör ve dosya yerleşimi (yalnız insan
                          için; hiçbir şeyi zorlamaz)
```

KLASÖR BU KORUMAYI VERMEZ, ve bunun kanıtı aynı projede duruyor: `Core/Combat/`
diskte Core'un İÇİNDEdir ama ad alanı `GridStrategy.Combat`, yani Core'un
KARDEŞİdir (aynı gözlem `BoardAdapter`'ın CS0118 bloğunda da yazılı). Yani bu
dosyayı `Combat/` klasörüne taşımak tek başına hiçbir şeyi bozmazdı; bozan şey ad
alanının ve asmdef'in değişmesi olurdu. asmdef silinirse iki assembly tek
assembly'ye düşer ve ayrımın tamamı bir isimlendirme geleneğine iner.

`Unit`'e `int MoveRange` alanı eklemek seçeneği burada TEKRAR EDİLMİYOR: o karar
[`MoveAction.md`](MoveAction.md)'de, hareket menzilinin nereden geldiğini anlatan
yerde yazılı ve sonucu tam olarak bu tiptir.

---

## MoveProfile(int range)

### SIFIR MENZİL GEÇERLİ: EŞİK KOPYALANMAZ, GEREKÇESİ TAŞINIR

MENZİL 0 BURADA GEÇERLİ, `AttackProfile`'da DEĞİL. Asimetri kasıtlı ve gerekçesi
`MoveAction`'da yazılı: hiçbir hücreye ulaşamayan bir SALDIRI anlamsızdır, hiçbir
hücreye gidemeyen bir BİRİM anlamlıdır — kök salmış, sersemlemiş, kuşatılmış.

### HARİTA: üç eşiğin sayı doğrusundaki yeri

Aynı projede üç kurucu bir sayıyı doğruluyor ve üçünün kesme noktası aynı yerde
DEĞİL. Sıfırın hangi tarafta kaldığı, o tipin ifade edebildiği oyunu belirliyor:

```
         -2   -1    0    1    2    3
  ────────┼────┼────╬────┼────┼────┼──►
  ◄── fırlatma bölgesi ▲ kabul bölgesi ──►
                       └── ÜÇ TİPİN AYRIŞTIĞI TEK NOKTA
```

| tip | negatif | 0 | pozitif |
|---|---|---|---|
| `MoveProfile` | fırlatır | **kabul** | kabul |
| `AttackProfile` | fırlatır | **FIRLATIR** ◄── AYRIŞMA | kabul |
| `PointerGesture` | fırlatır (NaN de) | **kabul** | kabul |

Üçünün de fırlattığı yer aynı: negatif taraf. Ayrıştıkları tek nokta sıfırın
kendisi — ve o nokta bir üslup değil, oyunun ifade edebildiği durum kümesidir.

### REDDEDILEN

`AttackProfile`'ın kurucusundaki `range < 1` eşiği birebir kopyalanır:

```csharp
if (range < 1) throw new ArgumentOutOfRangeException(...);
```

**KIRILAN:** "sıfır menzil geçerlidir" kararı iki yerde birbirine TERS yaşamaya
başlar: `int` alan sürüm sıfırı kabul eder, profil alan sürüm sıfırlı bir profil
KURAMAZ.

```
aşırı yüklemeler -> aynı işin iki adı olmaktan çıkar
derleyici: hiçbir şey der
test: _ZeroMoveRange_RejectsEveryStep profil tarafında yazılamaz
```

**KAZANIRDI:** "kımıldayamaz" ayrı bir durum olarak ifade edilseydi — Combat
tarafındaki bir sersemletme etkisi bunu üstlenseydi; o gün menzil hep pozitif
olur ve sıfır tek sayıya iki anlam yüklerdi.

### KAPSAM: kural "her eşik sıfırı kabul etsin" DEĞİL

Ölçüt tek: sıfır, o kavramda İFADE EDİLEBİLİR bir oyun durumu mu?

```
sıfır hareket menzili -> "kök salmış birim"      ► anlamlı ► kabul
sıfır saldırı menzili -> hiçbir hücreye vuramaz  ► anlamsız ► ret
```

KARŞI ÖRNEK ikizin kendisi: `AttackProfile`'ın kurucusu `range < 1` ile kesiyor
ve bu bir tutarsızlık değil — orada sıfır bir oyun durumu adlandırmıyor, yalnızca
hiç kullanılamayacak bir profil üretiyor. İki eşik farklı, ölçüt aynı.

### İŞ BÖLÜMÜ: aynı kavramın İKİ KAPISI var

Hareket menzili motora iki ayrı yoldan giriyor ve her yolun kendi bekçisi var:

```
int alan sürüm     ► MoveAction.Execute içindeki `moveRange < 0`
profil alan sürüm  ► bu kurucudaki `range < 0`
```

İkisi aynı kümeyi kabul ettiği için aşırı yükleme gerçekten aynı işin iki adıdır.
Buradaki eşik 1'e çekilseydi kapılar ayrışırdı: `int` sürüm sıfırı kabul ederken
profil sürümü sıfırlı bir profil KURAMAZDI ve
`Execute_ZeroMoveRange_RejectsEveryStep`'in profil ikizi olan
`Execute_ZeroRangeProfile_RejectsEveryStep` yazılamaz hâle gelirdi. Bekçilerden
biri silinirse o kapı korumasız kalır; ikisi yedek değil, iki ayrı giriş.

**TEK CUMLE:** Aynı kavramın iki kapısı varsa ikisi de aynı değer kümesini kabul
etmeli; yoksa aşırı yükleme değil, iki ayrı kural olurlar.

---

## Range

Bir turda kaç hücre uzağa gidebildiği. Get-only otomatik property: kurucuda konur,
bir daha yazılmaz.

Mesafenin nasıl ölçüldüğünü bilmez — o karar
[`GridDistance`](GridDistance.md#between-chebyshev)'ın. `0` geçerlidir ve
"yerinden kımıldayamaz" demektir; gerekçesi
[kurucunun bölümünde](#moveprofileint-range).
