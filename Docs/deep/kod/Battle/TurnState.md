# TurnState

> **Kaynak:** `Assets/Game/Battle/TurnState.cs`
> **Ad alanı:** `GridStrategy.Battle` · **Assembly:** `GridStrategy.Battle`
> (`noEngineReferences: true`)
> **Rol:** Varlık (Entity) — kimliği var, hafızası var, tutar ve bildirir

Bir savaşın sıra durumu: sıra hangi tarafta ve kaçıncı turdayız.

Bu tip **hiçbir yasak koymaz.** "Şu an senin sıran değil" cümlesi bir KURAL'dır ve
`TurnRules`'a aittir. Burada yazılsaydı sıra BİLGİSİ ile sıra KURALI aynı yerde
yaşardı; kuralı sınamak için her seferinde bir savaş kurmak gerekir, ve "sıra
kimde" sorusunun cevabını isteyen arayüz istemeden kural motorunu da yanında
taşırdı.

**ZAMANI YOKTUR.** `UnitLifecycle`'ın aksine burada `Tick` yok, çünkü tur süreli
değil: sıra yalnızca `EndTurn` ile, yani bir ÇAĞRIYLA devredilir. Bu tek cümle
aşağıdaki [event kararının](#turnchanged) dayanağıdır.

| Üye | Karar | Detay |
|---|---|---|
| `DefaultTurnOrder` | bir TANIM, bir durum değil — static olması doğru | [↓](#defaultturnorder) |
| `FirstTurnNumber` | ilk tur birdir; sayma kuralı arayüzde yaşamaz | [↓](#firstturnnumber) |
| `order` | dizilim VERİdir, koda gömülmüş bir dal değil | [↓](#order) |
| `orderView` | salt okunur görünüm bir kez kurulur | [↓](#orderview) |
| `index` | sıranın kimde olduğu TEK yerde | [↓](#index) |
| `TurnState()` | dürüst varsayılan: soruyu değiştirmiyor | [↓](#turnstate) |
| `TurnState(IReadOnlyList)` | üç kelepçe, üç ayrı kırılma; dizilim kopyalanır | [↓](#turnstateireadonlylist) |
| `TurnChanged` | YAZILMADI — olay akış sahibini atlardı | [↓](#turnchanged) |
| `Current` | dizilimden türetilir, ayrıca tutulmaz | [↓](#current) |
| `TurnNumber` | yalnız herkes bir kez oynayınca artar | [↓](#turnnumber) |
| `TurnOrder` | dışarıya salt okunur açılır | [↓](#turnorder) |
| `actionsUsed` | YAZILMADI — sözlüğün anahtarı seçilemiyor | [↓](#actionsused) |
| `EndTurn()` | sarmal turu işaretler; dönüş değeri çağıranın tek kanalı | [↓](#endturn) |

**İlgili anlatılar:** [04-karar sırası](../../konular/04-karar-sirasi.md) ·
[05-yaşam döngüsü](../../konular/05-yasam-dongusu.md)

---

## DefaultTurnOrder

İki taraflı varsayılan dizilim: önce oyuncu, sonra düşman. Savaşların çoğu böyle
başlar; başka bir dizilim isteyen kurucuya kendi listesini verir.

`public static readonly` olması bir sızıntı değil, amacın kendisi: bir dizilim
**TANIM**dır, hiçbir savaşa ait değildir ve iki savaşın onu paylaşması doğrudur.
Aynı sınıftaki karşılığı `TurnRules.MaxActionsPerTurn`. Ayıran şey `static`
kelimesi değil, o alanın savaştan savaşa değişip değişmediği — karşılaştırması
[`Battle.Turn`](Battle.md#turn) belgesinde.

---

## FirstTurnNumber

İlk tur **BİR**dir, sıfır değil: bu sayı oyuncuya gösterilmek için var ve arayüz
"Tur 1" yazar. Sıfırdan başlasaydı ekrana yazan her yer `+1` eklerdi, yani
"turlar birden sayılır" kuralı bu dosyada değil arayüzde — üstelik her arayüzde
ayrı ayrı — yaşardı.

---

## order

### DİZİLİM VERİDİR, KODA GÖMÜLMÜŞ BİR DAL DEĞİL

```
  REDDEDILEN — ternary
  ╔═ EndTurn ═══════════════════╗  Current = Current == Player
  ║ dizilim #1 : Player ↔ Enemy ║            ? Enemy : Player
  ╚═════════════════════════════╝
  ╔═ tur sayacı ════════════════╗  "herkes oynadı mı" sorusu
  ║ dizilim #2 : Player'a dönüş ║  "Current tekrar Player oldu"ya
  ╚═════════════════════════════╝  iner
  >> İKİ YER — ve ikisi de "tam iki taraf var" varsayıyor <<
  Player, Enemy, Enemy dizilimi İKİ devirde tamamlanmış sayılır

  SEÇİLEN — Team[] order
  ╔══════════════ order ══════════════╗
  ║ [0] Player   [1] Enemy   [2] ...  ║  >> TEK YER <<
  ╚═══════════════════╤═══════════════╝
                      └── index sarmalı ──► tur tamamlandı
  Dizilim artık bir DAL değil bir DEĞER; uzunluğu okunabiliyor,
  dolayısıyla "herkes bir kez oynadı" sorusu türetilebiliyor.
```

### KAPSAM: "diziliş koda yazılmaz" diye bir kural YOK

Ölçüt: yazılan şey bir **DEĞER** mi, bir **KARAR** mı?

**KARŞI ÖRNEK** aynı dosyada, birkaç satır yukarıda:
[`DefaultTurnOrder`](#defaultturnorder) `new[] { Team.Player, Team.Enemy }` diye
harfi harfine koda yazılmış ve doğrusu bu — o, alanın alabileceği DEĞERLERDEN
biri, bir varsayılan. Reddedilen ternary ise bir değer değil, dizilimi okunamaz
kılan bir KARAR: uzunluğu yok, dolaşılamaz, kopyalanamaz. Aynı iki takım, bir
yerde doğru bir yerde yanlış; ayıran şey takımların adı değil, yazıldıkları şeklin
sorulabilir olması.

### İŞ BÖLÜMÜ: üç alan, üç ayrı soru

```
  order       "kim, hangi sırayla"  DEĞİŞMEZ, kurulumda donar
  index       "şu an kaçıncı giriş" TEK değişken
  TurnNumber  "kaçıncı tur"         index sarmalından türetilir
```

Üçü ÖRTÜŞMÜYOR ve bir `Team current` alanı bilerek YOK (gerekçesi
[`index`](#index) başlığında): olsaydı `order` ile `index`'in cevabı üçüncü bir
yerde tekrarlanırdı. `order` silinirse sıra bir dala geri döner; `index`
silinirse "sıra kimde" sorusu cevapsız kalır; `TurnNumber` silinirse tur numarası
her çağıranda sarmal sayılarak kurulur ve her biri kendi tanımını yazar.

### REDDEDILEN

Liste hiç doğmaz; sıra iki değer arasında sabitlenir ve `EndTurn` tek satıra
iner:

```csharp
public Team Current { get; private set; }
// EndTurn içinde:
Current = Current == Team.Player ? Team.Enemy : Team.Player;
```

**KIRILAN:** takım dizilimi İKİ yerde yaşar — ternary'de ve tur sayacında.

```
"herkes bir kez oynadı mı" sorusu "Current tekrar Player oldu"ya iner
  -> Player, Enemy, Enemy dizilimi (turda iki kez oynayan hızlı düşman)
     iki devirde tamamlanmış sayılır
derleyici: hiçbir şey der
test: kırmızı — TurnNumber_AdvancesOnlyAfterEveryEntryHasPlayed
```

**KAZANIRDI:** oyunda ikiden fazla giriş ASLA olmayacaksa ve tur numarası her
devirde artacaksa — o gün liste, derleyicinin garanti ettiğini çalışma zamanı
doğrulamasına çevirir: boş liste, `Team.None` ve `null` hatalarının hiçbiri bir
ternary'de olamaz.

**TEK CUMLE:** Dizilimi VERİ yapmak "kim ne zaman oynar" sorusunu tek yerde
cevaplanabilir kılar; koda gömmek onu her soruya yeniden yazdırır.

---

## orderView

Salt okunur görünüm bir **KEZ** kuruluyor. Her okumada `Array.AsReadOnly`
çağırmak aynı diziye her seferinde yeni bir sarmalayıcı üretirdi ve arayüz her
karede sıra listesini okuduğunda çöp toplayıcıyı beslerdi.

---

## index

Sıranın kimde olduğu **TEK yerde** duruyor: bir indeks. Ayrıca bir `Team current`
alanı tutulsaydı iki alanı senkron tutmak bir ödev olurdu ve `EndTurn`'de birini
güncelleyip diğerini unutmak derleme hatası vermezdi — hata "sıra düşmanda ama
düşman oynamıyor" diye bildirilirdi.

---

## TurnState()

`DefaultTurnOrder`'a devreden parametresiz kurucu. Bu bir **dürüst varsayılan**:
"hangi dizilim" sorusunu yaygın bir dizilimle cevaplıyor, soruyu değiştirmiyor.
Karşıtı, `TurnRules`'ta reddedilen
[uydurma sıfır](TurnRules.md#canactteam-team) — o, hiç sorulmamış bir soruyu
cevaplıyordu.

---

## TurnState(IReadOnlyList)

Takım dizilimini vererek bir savaş kurar. Dizilim savaş boyunca **DEĞİŞMEZ**;
sıra onun üzerinde döner.

**Dizilim KOPYALANIYOR.** Çağıranın dizisi saklanmış olsaydı, savaş sürerken o
diziye yazan bir satır sırayı ortasından değiştirirdi; bir `List<Team>`
küçüldüğünde ise `order[index]` savaşın ortasında `IndexOutOfRangeException`
atardı. Kopya, "dizilim kurulduğunda bellidir" sözünü tek satırda garanti eder.

**Boş dizilim** bir denge ayarı değil, bir çağıran hatasıdır: sırası hiç kimsede
olmayan bir savaşta `Current` okunamaz. Gürültülü reddetmek, ilk `EndTurn`'de
sıfıra bölme benzeri bir hatayla patlamaktan iyidir.

### HARİTA: atanmayı UNUTAN bir eleman ne olur

```
  Team:  None = 0  ◄── >> default(Team) <<
         Player = 1
         Enemy  = 2

  new Team[3] ──► [ None, None, None ]
       │           ▲ hiçbiri "eksik" GÖRÜNMÜYOR
       ▼
  TurnState(...) ──✗── bu satır reddeder
       │   (satır silinseydi ▼)
  order[i] = None ──► Current = None
       └──► TurnRules.CanAct(x, None) HER ZAMAN false
            ◄── >> o devirde hiç kimse eyleyemez <<
```

Hata "oyun ara sıra takılıyor" diye bildirilir; sıfır değerin tarafsız olması bir
kaza değil, `Team`'in kararı — ve bedeli tam olarak o satır.

### KAPSAM: her şüpheli dizilim reddedilmez

Ölçüt: eleman **GEÇERSİZ** bir değer mi, yoksa yalnızca **alışılmadık** bir düzen
mi?

**KARŞI ÖRNEK** aynı döngünün hemen altında: yinelenen giriş (Player, Enemy,
Enemy) REDDEDİLMİYOR ve bu bilerek — turda iki kez oynayan hızlı bir düşmandır,
buradaki hiçbir kural onunla bozulmaz. Aynı üç satırda bir değer atılıyor, bir
düzen kabul ediliyor; ayıran şey dizilimin garipliği değil, elemanın kendisinin
bir taraf ADLANDIRIP adlandırmadığı.

**Alternatif:** yinelenen girişi reddetmek. Seçilmedi: tekrar yasak olsaydı
"düşman turda iki kez oynar" ancak ikinci bir düşman takımı uydurularak yazılırdı
ve o takım `TargetingRules`'a göre birincinin geçerli hedefi olurdu. Tetiği: bu
tip takım BAŞINA defter (eylem bütçesi, kaynak) tutmaya başladığı gün.

### İŞ BÖLÜMÜ: kurucudaki üç kelepçe

```
  turnOrder == null    ► referans yok
  Count == 0           ► sırası hiç kimsede olmayan savaş
  eleman == None       ► sırası OLMAYAN tarafta olan savaş
```

Üçü aynı soruyu üç kez sormuyor, üç ayrı kırılmayı kapatıyor. `null` silinirse
ilk `Count` okumasında patlar; `Count` silinirse `order[index]` ilk `Current`
okumasında patlar; tarafsızlık satırı silinirse **hiçbir şey patlamaz** —
sessizce ölü bir devir doğar. Sertlik sırası kırılmanın GÖRÜNÜRLÜĞÜYLE ters
orantılı ve öyle olması gerekiyor.

### REDDEDILEN

Tarafsızlık kontrolü hiç yazılmaz, tarafsız taraf da sıraya girer:

```csharp
copy[i] = turnOrder[i];
```

**KIRILAN:** her turda hiçbir şeyin olamadığı bir devir doğar.

```
TurnRules tarafsızı hiçbir sırada eyletmez
  -> o devirde hiçbir birim eyleyemez
  -> oyun her turda bir kez donmuş görünür, hata "ara sıra takılıyor" olur
derleyici: hiçbir şey der  ·  test: yeşil kalır
```

**KAZANIRDI:** tarafsızın gerçekten oynayacağı bir şey olduğu gün — yaban
canavarları, yayılan yangın, herkese ateş eden nötr kuleler; o gün "çevre turu"
gerçek bir turdur ve burada yasaklamak uydurma bir üçüncü takım açtırır.

**TEK CUMLE:** `default(Team)` tarafsızdır, yani bu satır olmazsa elemanı
atanmayı unutulmuş bir dizi geçerli bir dizilim sayılır ve savaş olmayan tarafın
sırasında başlar.

---

## TurnChanged

**Bu olay YAZILMADI.** `UnitLifecycle`'ın gerekçesi kopyalanmadı, bu tipe
uygulandı ve İKİ yarısı ayrı ayrı sınandı. Oradaki cümle şuydu: *"dönüş değeri —
soran zaten orada; event — ilgilenen başka yerde."*

**Birinci yarı GEÇERSİZ:** bu tipte kimsenin sormadığı bir geçiş yok.
`UnitLifecycle`'da event'i haklı çıkaran şey `Tick`'in içindeki Downed → Dead
geçişiydi — zamanla, kimse sormadan oluyordu. Burada zaman yok; sıra yalnızca
`EndTurn` ile değişir ve o çağrıyı yapan taraf cevabı dönüş değeriyle alır.

**İkinci yarı** — asıl sınav burada, ve ilk bakışta event'i HAKLI çıkarıyor: tur
değişimini duymak isteyen çok (tur başlığı, yapay zekâ sürücüsü, etki süresi
sayaçları) ve hiçbiri `EndTurn`'ü çağıran taraf değil. Buna rağmen event
reddedildi; gerekçe aşağıda ve `StructureLifecycle`'ınkinin kopyası **değil**:
orada ilgilenen zaten çağırandı, burada ilgilenen başkadır ama araya AKIŞ SAHİBİ
girer.

### HARİTA: olayın yayılacağı AN

```
  EndTurn() gövdesi:
    index = (index + 1) % order.Length;   ◄── ① sıra YENİ
  ┌─── olay tam BURADA yayılırdı ────────────────────────┐
  │  index      : YENİ taraf                             │
  │  TurnNumber : ESKİ tur    >> TUTARSIZ PENCERE <<     │
  └──────────────────────────────────────────────────────┘
    if (index != 0) return false;
    TurnNumber++;                          ◄── ② tur YENİ

  >> ①+② birlikte doğru olduğu tek an EndTurn'ün DÖNÜŞÜDÜR <<
```

Pencerede `TurnRules`'a soran bir dinleyici, oyunun hiçbir anında doğru olmayan
bir cevap alır ve bunu gösterebilecek test yok.

### KAPSAM: bu tipte bildirim yok DEĞİL

Ölçüt: geçişi kimse SORMUYOR mu, yoksa soran var da mı geç mi duyuyor?

**KARŞI ÖRNEK** aynı dosyada, [`EndTurn`](#endturn)'ün `bool` dönüşü: o da bir
bildirimdir ve reddedilmedi — "tur tamamlandı mı" sorusunun cevabını çağıranın
kendi başına kuramayacağı orada yazılı. Reddedilen şey BİLDİRİM değil, bildirimin
akış sahibini **ATLAYAN** biçimi. Dinleyen taraf cevabı akış sahibinden alırsa
pencere kapanmış olur.

### İŞ BÖLÜMÜ: iki kanal, iki farklı soran

```
  EndTurn() dönüşü   ► "bu devir bir TUR muydu"  soran = çağıran
  Current/TurnNumber ► "şu an ne durumdayız"     soran = herkes
```

İkisi ÖRTÜŞMÜYOR: biri bir GEÇİŞİ, diğeri bir DURUMU veriyor. Dönüş silinirse
çağıran "oku, çağır, tekrar oku, karşılaştır" üçlüsünü yazar ve ilk okumayı
unutan gün tur atlanmış görünür. Özellikler silinirse sırayı duymayan hiç kimse
öğrenemez. Üçüncü bir kanal — olay — bu ikisinin arasına girmeye çalıştığı için
reddedildi.

### REDDEDILEN

```csharp
public event Action<Team> TurnChanged;
```

**KIRILAN:** dinleyen taraf akışın sahibini ATLAYIP doğrudan buraya bağlanır.

```
olay EndTurn'ün ORTASINDA yayılır
  -> index yeni tarafı, TurnNumber hâlâ eski turu gösterir
  -> o an TurnRules'a soran dinleyici oyunun hiçbir anında doğru
     olmayan bir cevap alır
derleyici: hiçbir şey der  ·  test: yeşil kalır
```

**KAZANIRDI:** sıra ÇAĞRISIZ da değişebilseydi — süreli tur ("30 saniyede
oynamazsan pas geçersin"), bağlantısı kopan oyuncunun otomatik devri ya da sırayı
çeviren bir sunucu; o gün geçişi bir `Tick` doğurur ve olay onu duymanın TEK yolu
olur.

**TEK CUMLE:** Olay, kimsenin SORMADIĞI bir geçiş için vardır; burada geçişi yapan
taraf cevabı zaten dönüş değeriyle alıyor.

---

## Current

Sıranın o an hangi tarafta olduğu. Dizilimden **türetilir**, ayrıca tutulmaz —
gerekçesi [`index`](#index) başlığında.

---

## TurnNumber

Kaçıncı turdayız. İlk tur `FirstTurnNumber`'dır ve sayı yalnızca dizilimdeki
**herkes** bir kez oynadığında artar; ne zaman arttığı [`EndTurn`](#endturn)
başlığında.

---

## TurnOrder

Bu savaşın takım dizilimi — salt okunur bir görünüm. Dışarıdan değiştirilemez;
değiştirilebilseydi sıra savaşın ortasında kayardı. Görünümün her okumada değil
bir kez kurulmasının gerekçesi [`orderView`](#orderview) başlığında.

---

## actionsUsed

**Bu sözlük YAZILMADI.** "Bir birim turda kaç kez eyleyebilir" sorusunun iki
yarısı var ve ikisi ayrı yerlere ait: **KAÇ** sorusu bir kuraldır ve
[`TurnRules.MaxActionsPerTurn`](TurnRules.md#maxactionsperturn)'de yaşar; **KAÇ
KEZ KULLANDI** sorusu bir durumdur, çünkü aynı birime aynı anda sorulan aynı soru
farklı cevap verir. İkincisi bu tipe konmadı.

### HARİTA: aynı savaşçının ÜÇ temsili

```
  tahtada   Unit       ◄── UnitGrid'in anahtarı        (Core)
  savaşta   Combatant  ◄── Battle.combatants'ın değeri (Combat)
  kuralda   Team       ◄── TurnRules'un gördüğü tek şey (Combat)

  Dictionary< ? , int > actionsUsed
              ▲
              └── >> ÜÇÜNDEN HANGİSİ? <<
```

Bu dosyanın `using` listesinde `GridStrategy.Core` YOK: reddedilen satır
derlenmek için önce yeni bir bağımlılık ister. `Team` ise görünür ama bir
SAVAŞÇIYI adlandırmıyor — bir tarafı adlandırıyor. Yani anahtar seçilemiyor
değil; seçilebilecek doğru anahtar bu tipin görüş alanında hiç yok.

### KAPSAM: bu tip koleksiyon tutamaz DEĞİL

Ölçüt: koleksiyonun **ANAHTARININ** sahibi bu tip mi?

**KARŞI ÖRNEK** aynı dosyada, [`order`](#order) alanı: o da bir koleksiyon ve
sorunsuz duruyor, çünkü tuttuğu şey `Team` — bu tipin gördüğü, kopyaladığı ve
doğruladığı bir değer. Reddedilen sözlüğün anahtarı ise başka bir assembly'nin
kimliği. Ayıran şey koleksiyon olması değil, anahtarın kimin sözlüğünde doğduğu.

### İŞ BÖLÜMÜ: sorunun iki yarısı, iki ayrı yer

```
  "KAÇ kez eyleyebilir"   KURAL  ► TurnRules.MaxActionsPerTurn ✓
  "KAÇ KEZ kullandı"      DURUM  ► sahibi HENÜZ YOK            ✗
```

İlk yarı yazıldı ve yeri belli. İkinci yarı bilerek boş: bugün `TurnRules` onu bir
PARAMETRE olarak alıyor, yani sayacın sahibi çağıran. Sabit buraya taşınsaydı
kural sayıyı okuyamaz hâle gelirdi
([gerekçesi](TurnRules.md#maxactionsperturn)); sayaç buraya konsaydı anahtarı
olmayan bir sözlük doğardı. İkisi de reddedildiği için bugün yalnızca eksik olan
yazılı — **eksik ile yanlış aynı şey değil.**

### REDDEDILEN

```csharp
private readonly Dictionary<Unit, int> actionsUsed =
    new Dictionary<Unit, int>();
public int ActionsUsedBy(Unit unit) { ... }
// ve EndTurn içinde: actionsUsed.Clear();
```

**KIRILAN:** anahtar seçilemiyor — savaşçı bugün tahtada `Unit`, savaşta
`Combatant`, kuralda `Team` diye üç tiple temsil ediliyor.

```
elinde başka bir tip olan çağıran SORAMAZ
  -> kendi eşlemesini kurar
  -> sayaç yine iki yerde yaşar
kaldırılan birim de sözlükte kalır ve savaş boyunca bellekte tutulur
derleyici: hiçbir şey der  ·  test: yeşil kalır
```

**KAZANIRDI:** savaşçının TEK bir kimliği olduğu gün — `Unit` ile `Combatant`
birleştiğinde ya da akış sahibi buraya kararlı bir savaş içi indeks verdiğinde; o
gün sıfırlama anını bilen tek yer burası olduğu için sözlüğün yeri de burasıdır.

**TEK CUMLE:** Bir sözlüğün yeri, anahtarının sahibinin yaşadığı yerdir — bu tip
o eşlemenin sahibi değil.

---

## EndTurn()

Sırayı dizilimdeki bir sonraki tarafa devreder.

**NEDEN DÖNÜŞ DEĞERİ:** "tur tamamlandı mı" sorusunun cevabını çağıran kendi
başına kuramaz — kurmak için takım dizilimini bilmek gerekir ve o bilgi burada
yaşar. Çağıranın elindeki tek alternatif "önce `TurnNumber`'ı oku, çağır, tekrar
oku, karşılaştır" üçlüsüdür; aynı üç satır arayüzde, yapay zekâda ve etki süresi
sayacında üç kez doğar ve birinde ilk okuma unutulursa hata sessizdir — tur
atlanmış görünür.

Sarmal **tam turu** işaretler: dizilimin başına dönmek, herkesin bir kez oynadığı
demektir. Tek girişli bir dizilimde bu her devirde olur ve doğrudur — tek taraflı
bir savaşta her devir bir turdur.

### HARİTA: sarmal neyi İŞARETLER

```
  order = [ Player, Enemy, Enemy ]   (turda iki oynayan düşman)

  devir        1     2     3     4     5     6
  index    0 ► 1  ►  2  ►  0  ►  1  ►  2  ►  0
                           ▲                 ▲
                 >> SARMAL <<      >> SARMAL <<
  TurnNumber   1     1     1     2     2     2     3
                           ▲                 ▲ burada artar

  >> TUR = index sıfıra döndüğü an << , yani herkes bir kez

  REDDEDILEN — her devir bir tur
  TurnNumber   1     2     3     4     5     6
  "3 tur dayan" bu dizilimde BİR oynama hakkı verir, iki
  taraflıda İKİ verir ── aynı cümle, iki farklı denge.
```

### KAPSAM: "her devir tur değildir" evrensel DEĞİL

Ölçüt dizilimin uzunluğu: tek girişli bir dizilimde `index` her devirde sıfıra
döner.

**KARŞI ÖRNEK** bu metodun kendi içinde, birkaç satır yukarıda: tek taraflı bir
savaşta HER devir bir turdur ve erken dönüş oraya hiç uğramaz — aynı kod, aynı
satırlar, farklı sonuç. Yani reddedilen davranış yanlış değil; yalnızca TEK bir
dizilim uzunluğu için doğru olan şeyi bütün dizilimlere uyguluyordu.

### İŞ BÖLÜMÜ: erken dönüş ile sayaç

```
  if (index != 0) return false;  ► tur İÇİ el değiştirme
  TurnNumber++; return true;     ► tur SINIRI
```

İkisi aynı bilgiyi iki kez vermiyor: biri çağırana "devam" der, diğeri "yeni tur"
der ve tek bir çağrı ikisinden yalnız birini döndürür. Erken dönüş silinirse sayaç
her devirde artar (reddedilen dünya). Sayaç artışı silinirse dönüş değeri hâlâ
doğru olur ama tur numarası sonsuza dek 1 kalır — süre sayan her etki sessizce
ölümsüzleşir.

### REDDEDILEN

Sarmal hiç beklenmez, her devir turu ilerletir ve erken dönüş silinir:

```csharp
index = (index + 1) % order.Length;
TurnNumber++;
return true;
```

**KIRILAN:** tur numarası dizilimin **UZUNLUĞUNA** bağlanır.

```
"3 tur dayan" iki taraflı savaşta iki, üç girişlide bir oynama hakkı verir
"2 tur süren zehir" düşman oynamadan biter
derleyici: hiçbir şey der
test: kırmızı — TurnNumber_DoesNotAdvanceInsideTheRound
```

**KAZANIRDI:** tur numarası oyuncuya HİÇ gösterilmeyecek, yalnızca olayları
sıralayan bir damga (kayıt sırası, tekrar dosyası ordinali) olsaydı — orada
aranan şey "kaçıncı tur" değil "hangisi önce oldu"dur.

**TEK CUMLE:** Tur, herkesin bir kez oynadığı andır; her devri tur saymak dengeyi
tasarımcının hiç dokunmadığı bir sayıya bağlar.
