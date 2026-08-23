# Yok olan mekanizmalar — motorun sunduğu ama bu projenin henüz almadığı

██ Bir mekanizmanın projede **olmadığını** bilmek, onun **arkada ne yaptığını**
bilmek DEĞİLDİR. ██ Bu dosya ikincisini yazıyor. `grep` sana beş sıfır verir;
sıfırlar hangi işleyişin yokluğunu ölçtüğünü söylemez.

## `02-sonraki-asamalar.md` ile iş bölümü — burada ne YOK

| Soru | Cevabın sahibi |
|---|---|
| Bu ne zaman gerekli olur, hangi somut olayla | [02-sonraki-asamalar.md](02-sonraki-asamalar.md) |
| O gün geldiğinde **arkada ne oluyor** | **burası** |
| Bugün kodda hangi desen zaten duruyor | [01-koda-gomulu-desenler.md](01-koda-gomulu-desenler.md) |
| Hangi kavramın sahibi var, hangisi borç | [03-kavram-borc-defteri.md](03-kavram-borc-defteri.md) |

██ Tetikleyici koşullar burada **tekrar edilmiyor**; ██ "ne zaman" sorusunun tek
sahibi `02`'dir ve bu dosya o satırlara link verir:

```
02  ►  ScriptableObject NE ZAMAN gelir      ►  üç tetikleyicinin biri doğduğunda
04  ►  ScriptableObject GELDİĞİNDE ne olur  ►  tip Component olmaktan çıkar,
                                               kurucu artık ÇAĞRILMAZ, doğrulama
                                               OnValidate'e kayar, değer diske
                                               yazılır ve Editor'de KALIR
```

## Sürüm damgası

```
ProjectSettings/ProjectVersion.txt  ►  2021.3.45f2  (revision 88f88f591b2e)

Doğrulama tarihi : 2026-08-23
Doğrulama şekli  : ① repo dosyaları (Assets/ · ProjectSettings/ · Docs/)
                   ② yerel Editor kurulumunun API belgesi — .../2021.3.45f2/
                      Editor/Data/Managed/UnityEngine/UnityEngine.CoreModule.xml
                   ③ aynı klasördeki UnityEngine.CoreModule.dll üzerinde
                      YANSIMA sorgusu (tip ağacı ve generic kısıtlar)
```

██ Sürüme bağlanmamış bir motor iddiası kusurdur. ██ Bu turda **Editor
koşturulmadı**, Play'e basılmadı, Profiler açılmadı — bu bir belge turudur. Bu
yüzden her mekanizmanın **③ ÖLÇÜ** alanı var: iddia bir etikettir, ölçü onu
kanıta çevirir. Ölçülemeyen iddia "doğrulanmadı" diye işaretli.

## Her mekanizmanın altı alanı

| Alan | Ne yazar |
|---|---|
| **① ARKADA NE OLUYOR** | Gerçek işleyiş: hangi tablo, hangi arama, hangi maliyet, hangi ömür |
| **② SAHİP ETİKETİ** | Bu mekanizmayı kim veriyor — dil mi, kütüphane mi, motor mu, Editor mü, sen mi |
| **③ ÖLÇÜ** | Kafanda ya da Editor'de **koşturabileceğin** deney |
| **④ BU PROJEDE NEREYE DÜŞERDİ** | Tam `dosya:satır`, ve bugün o işi ne yapıyor |
| **⑤ NE KIRAR** | Getirildiği gün hangi mevcut karar çöker |
| **⑥ EN YAKIN ALTERNATİF** | Ve hangi koşulda o kazanır |

## Sahip etiketi sözlüğü

██ "Unity'nin özelliği" diye anılan şeylerin çoğu Unity'nin değildir. ██

| Etiket | Ne demek | Örnek |
|---|---|---|
| **C# dili** | Derleyici verir; Unity olmadan da vardır | `static` · `where T : class` · `Action<T>` |
| **.NET kütüphanesi** | BCL'den ödünç; motor bilmez | `Delegate.Combine` · `Dictionary<,>` |
| **UnityEngine API** | Motorun çalışma zamanı; derlenmiş oyunda da vardır | `DontDestroyOnLoad` · `FindObjectOfType` · `ScriptableObject` · `ObjectPool<T>` · `Camera.main` |
| **Unity attribute** | Bir ETİKET; kendi başına hiçbir şey yapmaz, onu OKUYAN bir taraf gerekir | `[SerializeField]` · `[CreateAssetMenu]` · `[RequireComponent]` |
| **Editor aracı** | Yalnız Editor'de vardır; derlenmiş oyunda YOKTUR | `.asset` üretme menüsü · Inspector çizimi · `OnValidate` |
| **proje kararı** | Hiçbir yerde tanımlı değil; yazan koyar, yazan bozar | `Instance` adı · sıfırlama sözleşmesinin metni |

Ayrımın ölçüsü: **etiketi çıkar, ne kalır.** `static`'i çıkarırsan kod
derlenmez — dil. `[SerializeField]`'ı çıkarırsan kod derlenir ama Inspector alanı
kaybolur — attribute. `Instance` adını `Current` yaparsan hiçbir şey olmaz —
proje kararı.

---

## 0 · Ölçülen yokluk — ve yokluğun sınırı

İki kapsamda sayıldı (2026-08-23):

| Aranan | `Assets/Game/` (üretim) | `Assets/` (test dahil) |
|---|---|---|
| `Instance` · `DontDestroyOnLoad` | 0 | 0 |
| `FindObjectOfType` · `FindObjectsOfType` · `GameObject.Find` | 0 | 0 |
| `ScriptableObject` | 0 kod, **2 yorum** | 2 yorum |
| `Pool` (harf duyarsız) · `SceneManager` · `LoadScene` · `Resources.` | 0 | 0 |

İki `ScriptableObject` geçişinin ikisi de yorum, ikisi de aynı dosyada, ve ikisi
de bir borcun yazılı hâli — `AttackProfile.cs:13-14`: "bugün düz C# nesnesi;
ScriptableObject kararı geldiğinde bu satır değişir, rol değişmez."

██ Ama yokluk mutlak değil. ██ Bu tablo bir tuzak üretebilir: "bu projede global
arama yok" cümlesi **yanlıştır**. `Camera.main` tam olarak bir aramadır ve
projede iki satırda çalışıyor (ayrıntısı üçüncü mekanizmada). Bir mekanizmanın
**adını** aramak, mekanizmayı aramak değildir.

---

## 1 · `static Instance` — Singleton

### ① ARKADA NE OLUYOR

**Bir `static` alan nerede yaşar.** Örnek alanları nesnenin içinde durur; iki
`Combatant` iki ayrı `Health` taşır. `static` alan **hiçbir örneğin içinde
değildir** — tipin kendi deposundadır, ömrü tipin yüklü olduğu uygulama alanına
bağlıdır. Ölçüsü projede duruyor: hiçbir `Battle` kurmadan
`TurnState.DefaultTurnOrder` (`TurnState.cs:40`) okunabilir, `Turn.Current`
okunamaz. Depolama tarafının tamamı
[../deep/dil/07-bellek-canlilik-ve-yikim.md](../deep/dil/07-bellek-canlilik-ve-yikim.md)'de.

**Bir `static` alan bir GC KÖKÜDÜR.** Kök kümesini o dosyanın
[üçüncü durağı](../deep/dil/07-bellek-canlilik-ve-yikim.md#ucuncu-durak-kok-kumesi-ve-erisilebilirlik)
anlatıyor; tekrar etmiyorum. **Üzerine koyduğum şey zincirin başı.** Orada
ölçülmüş bir zincir var: tek bir `Combatant` referansı, delege hedefi üzerinden
`Battle`'ı ve oradan bütün savaşı erişilebilir tutuyor — yedi hop.

```
BUGÜN                              static Instance OLSAYDI
canlı yerel ► Battle ► her şey     Battle.Instance ► Battle ► her şey
      ▲                                  ▲
metot bitince ok DÜŞER             ██ KÖK ██ — hiç düşmez
toplama MÜMKÜN                     toplama İMKÂNSIZ, oyun kapanana kadar
```

██ "Sızdırır" bir etiket değil bir SAYIDIR: ██ kök kümesinden erişilebilen nesne
sayısı. Bugün `BoardAdapter` yok olunca `battle` alanına kimse ok tutmaz ve iki
sözlük topluca erişilemez olur. Statik alanla bu **hiç** olmaz; ikinci savaş
başlasa bile birincisi bellekte durur.

**Domain Reload ile ilişkisi — en sık vuran hata.**
[Altıncı durak](../deep/konular/08-motor-cagri-dongusu.md#altinci-durak-domain-reload-sessiz-kanit-kirleticisi)
bu projede belirleyici satırın `m_EnterPlayModeOptionsEnabled: 0` olduğunu ve
Domain Reload'ın **yapıldığını** belgeliyor; üzerine koyduğum şey **kapatıldığı
gün ne olacağı**. Reload kapatıldığında `.NET` uygulama alanı yeniden yüklenmez:
statikler Play oturumları **arasında yaşar**. İkinci Play'de `Instance` hâlâ
doludur ve içindeki nesne birinci oturumdan kalmadır, yani `Destroy` edilmiştir.
Kritik ayrım burada ve
[Unity'nin iki ömrü](../deep/dil/07-bellek-canlilik-ve-yikim.md#dorduncu-durak-unitynin-iki-omru)
durağında ölçülmüş:

```
  if (Instance == null) { Instance = this; }
      Unity'nin AŞIRI YÜKLENMİŞ == 'i "yerel taraf yaşıyor mu" diye sorar
      yıkılmış nesne için TRUE  ►  koruma KENDİNİ ONARIR

  if (ReferenceEquals(Instance, null)) { Instance = this; }
      C# "aynı kutu mu" diye sorar; kutu duruyor  ►  FALSE
      ►  Instance ESKİ, YIKILMIŞ nesnede KALIR
      ►  sonraki her Instance.X → MissingReferenceException ya da sessiz yanlış

  ██ Ve hiçbiri kendini onarmaz: static bir OLAY aboneliği. ██
     Yıkılmış nesnenin metodu davet listesinde durmaya devam eder.
```

██ İlk Play çalışır, ikincisi bozulur, ve kırılan satır hiç değişmemiştir. ██

**`Instance` bir SÖZLEŞME DEĞİL, bir GELENEKTİR.** `static` "tek örnek" demez,
"tipe ait alan" der. Ölçüsü: `Instance` alanına kaç kez yazılabileceğini
sınırlayan **hiçbir dil kuralı yoktur**. İki sahne nesnesi iki `Awake`
koşturur; ikincisi ya birinciyi ezer ya kendini yok eder — ikisi de **elle
yazılmış** davranıştır.

Karşılaştır: `AttackProfile.cs:55-58`'deki `range < 1` kelepçesi gerçekten
zorlanıyor, ama gücü kurucuda olmasından değil **tek kapı** olmasından geliyor —
profil üretmenin başka yolu yok, dolayısıyla `Range < 1` bir profil **doğamaz**.
`Instance` için böyle bir tek kapı yoktur: her `Awake`, her test kurulumu, her
editör betiği ona yazabilir. Kelepçeyi koyan derleyici değil, yazan kişidir.

**Test yalıtımı.** Statik durum test metotları **arasında taşınır**; testler ayrı
ayrı yeşil, birlikte kırmızı olur ve sıra bağımlı bir hata doğar. Riskin bugün
sıfır olmasının sebebi kodda yazılı — `Battle.cs:136`: "static bir alana konsaydı
durum test metotları arasında sızardı."

`HENÜZ YOK → Unity attribute` — `[RuntimeInitializeOnLoadMethod]`, statik durumu
oyun başlarken elle sıfırlamak için kullanılan etiket. Bu projede sıfır satır;
gerekmiyor, çünkü sıfırlanacak statik durum yok.

### ② SAHİP ETİKETİ

```
static anahtar kelimesi          ►  C# dili
alanın adının `Instance` olması  ►  proje kararı (Current, Main, Shared de olur)
"tek örnek" garantisi            ►  proje kararı — ██ derleyici VERMEZ ██
Domain Reload'ın sıfırlaması     ►  Editor aracı (Play'e giriş davranışı)
sahneler arası yaşam             ►  UnityEngine API — AYRI mekanizma, bkz. 2
```

██ Üçü ayrı karardır: teklik · sahneler arası ömür · global erişim. ██ Ayrımın
kendisi `02`'nin
[Aşama 4](02-sonraki-asamalar.md#asama-4-singleton-ve-reddedilisin-kendisi)'ünde.

### ③ ÖLÇÜ

**(a) Domain Reload deneyi.** Bir `MonoBehaviour`'a `static int playCount;` koy,
`Awake`'te artır ve yazdır. Play'e bas, çık, tekrar bas.

```
Enter Play Mode Options KAPALI (bugünkü: m_EnterPlayModeOptionsEnabled: 0)
    ardışık Play  ►  1, 1, 1, ...
Reload Domain KAPATILIRSA
    ardışık Play  ►  1, 2, 3, ...   ██ statik alan oturumu AŞTI ██
```

**(b) İkilik deneyi.** Sahnedeki `Board` nesnesini çoğalt. Bugün ne olur kodda
yazılı — `BoardAdapter.cs:65-67`: "paylaşılan tek bir static alan YOK", yani iki
adaptör iki ayrı `new Battle(width, height)` kurar. `Instance` olsaydı ikinci
`Awake` birinciyi ezerdi ve **hangisinin kazandığı çağrı sırasına bağlı**
olurdu — o sıranın neden garanti edilmediği
[ikinci durakta](../deep/konular/08-motor-cagri-dongusu.md#ikinci-durak-cagri-sirasi-sahipleriyle-ezberle-degil).

**(c) Sızıntı deneyi.** İki EditMode testi yaz: birincisi `Instance`'ı doldursun,
ikincisi `Instance == null` beklesin. Tek tek koştur — ikisi de yeşil. Birlikte
koştur — ikincisi kırmızı. Ölçü testin kendisi değil, testleri **ayırt eden
şeydir**: bugün 26 test dosyası kendi nesnesini `new` ile kuruyor.

### ④ BU PROJEDE NEREYE DÜŞERDİ

`Battle`'ın sahibi tek bir yer — bir alan ve bir satır:

```csharp
// BoardAdapter.cs:187
private Battle battle;

// BoardAdapter.cs:231   (Awake'in içinde)
battle = new Battle(width, height);
```

██ ÖLÇÜLDÜ ██ — `battle.` ifadesi `BoardAdapter.cs` içinde **15 kez** geçiyor ve
`Assets/Game/` altında başka **hiçbir dosyada** geçmiyor. Savaşa erişen tek tip
var, o da savaşı **kuran** tiptir; `Battle.Instance` diye bir şey yok ve
kimsenin ona ihtiyacı olmadı. Bunun kazandırdığı üç şey:

① **`width` ve `height`'ın nereden geldiği görünür** — ikisi de
`[SerializeField, Min(1)]` alanı (`:109`, `:110`). `Instance` olsaydı savaşın
boyutunu kimin verdiği çağrı yerinden okunamazdı.

② **`GridStrategy.Battle` motoru hiç tanımıyor** (`asmdef` `noEngineReferences:
true` — okundu) ve `Battle` bir `MonoBehaviour` değil; savaş sahne olmadan `new`
ile kurulabiliyor. Bir `static Instance` bunu teknik olarak bozmazdı; bozacağı
şey **testin ne kadarını kurabildiğidir** — her test kendi savaşını kuramaz,
paylaşılanı temizlemek zorunda kalır.

③ **Sahiplik zinciri tek yönlü** — tahtanın tek yazarı olması
[../deep/konular/03-tahta-sahipligi.md](../deep/konular/03-tahta-sahipligi.md)'nin
konusu; `Instance` o zincire ikinci bir giriş kapısı açardı.

### ⑤ NE KIRAR

| Bugün duran karar | `static Instance` geldiğinde |
|---|---|
| `BoardAdapter.cs:65-67` — iki adaptör iki ayrı savaş | Çöker: ikisi tek savaşı paylaşır ya da biri diğerini ezer |
| `Battle.cs:136` — "static alana konsaydı testler arasında sızardı" | Yazılı gerekçe geçersizleşir; sızıntı gerçek olur |
| 26 EditMode dosyasının `new` ile kurulumu | Her testin başında `Instance = null;` disiplini doğar — tek unutuş sıra bağımlı bir hata üretir |
| Domain Reload temizliğinin **bedava** olması | Ayar artık bir tercih değil, bir **bağımlılık** olur |
| Kök kümesinin bugün küçük olması | Savaşın tamamı oyun ömrü boyunca erişilebilir kalır |

### ⑥ EN YAKIN ALTERNATİF

| Alternatif | Hangi koşulda kazanır |
|---|---|
| **Bugünkü: kompozisyon kökü** (`BoardAdapter.Awake`) | Tek sahne, tek kök, sahibi belli. Bugünkü koşul budur |
| **Serileştirilmiş referans** | İkinci bir tip savaşı **görmek** zorunda kaldığında; bağımlılık Inspector'da görünür kalır |
| **Kurucu parametresi** | Zaten kullanılıyor: `Combatant.cs:59` dört bağımlılığı imzada alıyor |
| **`static readonly` tablo** | Veri gerçekten değişmezse. Tek örnek `TurnState.DefaultTurnOrder`, `Array.AsReadOnly` ile sarılı |
| **Açıkça dağıtılan servis** | Global erişim gerçekten şartsa. `Instance` o gün bile yanlış cevaptır: ihtiyaç **ömürdür**, erişim değil |

---

## 2 · `DontDestroyOnLoad`

### ① ARKADA NE OLUYOR

██ Sahneyi değil, **nesnenin sahnesini** değiştirir. ██ En sık yanlış model
budur: "bu nesne artık silinmez" diye okunur. Yaptığı iş nesneyi
`DontDestroyOnLoad` adlı **özel bir sahneye taşımaktır**; yeni sahne
yüklendiğinde motor eski sahnedeki nesneleri yıkar, taşınan nesne artık o
sahnede olmadığı için yıkım listesine **hiç girmez**. Editor'de Hierarchy'de ayrı
bir başlık altında görünmesinin sebebi de budur — görsel ayrım bir süsleme
değil, mekanizmanın kendisinin görüntüsü.

Yerel 2021.3.45f2 belgesinden doğrulanan imza ve dönüş yolu:

```
M:UnityEngine.Object.DontDestroyOnLoad(UnityEngine.Object)
    "Do not destroy the target Object when loading a new Scene."

M:UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(
      UnityEngine.GameObject, UnityEngine.SceneManagement.Scene)
```

Üç şey imzada okunuyor: aldığı şey bir `UnityEngine.Object`, döndürdüğü şey
**yok**, ve **geri alma yolu yok** — dönmek başka bir tipten başka bir çağrı
ister. ██ Asimetri buradadır: gitmek tek çağrı, dönmek başka bir API. ██

**Yalnız KÖK nesnede çalışır mı — ██ DOĞRULANMADI ██.** Yerel API belgesi kök
koşulundan **hiç söz etmiyor**, yalnızca yukarıdaki tek cümleyi veriyor; Editor
koşturulmadığı için davranış da gözlenmedi. Bu yüzden burada iddia olarak
**yazılmıyor**. Doğrulanabilir olan şu: `Transform` hiyerarşisinde ebeveyni olan
bir nesne ebeveyninin sahnesinde yaşar, ve bu projede `CreateCellVisual` tam
olarak buna güveniyor:

```csharp
// BoardAdapter.cs:660-662
// Amaç konum değil TOPLU YAŞAM DÖNGÜSÜ: tahtayı yok etmek tek çağrıyla
// 15 hücreye uygulanır.
cell.transform.SetParent(transform, worldPositionStays: false);
```

Kök koşulu bunun motor tarafındaki karşılığı *olmalıdır* — ama *olmalıdır* bir
kanıt değildir.

**Yığılma sorunu.** Nesne sahne yüklemesiyle yok olmadığı için, onu **yeniden
doğuran** kod ikinci bir kopya üretir: sahneyi üç kez yükle, başlık altında üç
tane bulursun. Yığılmayı önlemek için çoğu kod `Instance` korumasına başvurur —
birinci mekanizmayla neden karıştırıldığının cevabı da budur: iki ayrı karar tek
satırda birbirine yapışır.

**Ömür etkisi.** Nesne yok olmadığı için `OnDestroy` **gelmez**, `OnDisable` de
gelmez; `OnEnable`/`OnDisable` çiftinde kurulan her abonelik sahne geçişinde
canlı kalır. Bu, birinci mekanizmadaki kök zincirinin motor tarafındaki ikizidir.

### ② SAHİP ETİKETİ

```
DontDestroyOnLoad · "DontDestroyOnLoad" sahnesi ►  UnityEngine API
Hierarchy'deki ayrı başlık                      ►  Editor aracı (derlemede yok)
sahneye dönüşte tekrar doğuran kod              ►  proje kararı
```

`HENÜZ YOK → UnityEngine API` — `SceneManager.LoadScene`, `LoadSceneMode.Additive`
ve `Scene` yapısı. Üçü de bu mekanizmanın **ön koşuludur** ve üçü de bu projede
sıfır satır.

### ③ ÖLÇÜ

**(a) Nesne hangi sahnede.** `gameObject.scene` ile aktif sahne
karşılaştırıldığında taşınma **görünür** olur: çağrıdan önce ikisi aynı, sonra
farklı. ██ Bu deney bu projede koşturulamaz ██ — yükleyecek ikinci sahne yok
(ölçü: `find Assets -name "*.unity"` → **1** dosya).

**(b) Yığılma sayacı.** Sahneyi üç kez yükle ve başlık altındaki nesne sayısını
say. Beklenen: koruma yoksa 3, varsa 1. Deneyin öğrettiği şey sayı değil,
**korumanın nereye yazıldığıdır**.

**(c) Abonelik ömrü deneyi.** Kalıcı nesneye bir olay abone et, sahneyi yeniden
yükle, olayı **bir kez** yayınla ve dinleyicinin kaç kez koştuğunu say. Birden
fazlaysa kalıcı nesne her yüklemede yeniden abone olmuştur —
`Delegate.Combine` aynı hedefi **elemez**, ayrıntısı beşinci mekanizmada.

### ④ BU PROJEDE NEREYE DÜŞERDİ

██ Hiçbir yere — ve bu bir eksiklik değil, bir kapsam ölçüsüdür. ██ Sahne sayısı
**1**; `SceneManager`, `LoadScene` ve `DontDestroyOnLoad` üretim kodunda **0**.
Bugün bu işi yapan şey **sahnenin kendisidir**: bütün nesnelerin ömrü Play
oturumuna eşittir, sahne bir kez yüklenir, dolayısıyla "sahne değişirken ne
olacak" sorusu sorulmuyor bile. Sahne dosyası okundu; içinde tam 2 GameObject var:

```
Assets/Scenes/SampleScene.unity
    Main Camera   ─ Transform · Camera · AudioListener
    Board         ─ Transform · Grid · BoardAdapter (MonoBehaviour)
```

Kalıcılık gerektirecek adaylar (müzik çalar, kayıt sistemi, ayarlar) bunların
hiçbiri değil — ve olmadıkları için soru da doğmuyor.

### ⑤ NE KIRAR

① **`Awake`'in demo satırları ikinci kez koşar.** `BoardAdapter.Awake` sahne
yüklendiğinde iki birim doğuruyor (`:260-261`). Adaptör kalıcı yapılsaydı ve
sahne yeniden yüklenseydi, kalıcı adaptörün **yanına** yeni sahnenin adaptörü
düşerdi: iki adaptör, iki savaş, dört birim.

② **`OnEnable`/`OnDisable` simetrisinin dayanağı değişir.** Bugün simetriyi
disiplin tutuyor ve bu kodda yazılı — `BoardAdapter.cs:275-276`: "eksik bir `-=`
tek bir uyarı bile üretmez". Risk bugün küçük çünkü nesne sahneyle birlikte yok
oluyor; kalıcı bir nesnede aynı eksiklik **oturum boyu** yaşar.

③ **`unitViews` sözlüğünün ömrü sahneden kopar.** Bugün sözlük adaptörle doğup
ölüyor. Kalıcı bir adaptörde sözlük ikinci sahneye **eski girdilerle** girer ve
içindeki `UnitView` referansları yıkılmış nesnelere işaret eder — "null gibi ama
null değil" hâlinin toplu hâli.

### ⑥ EN YAKIN ALTERNATİF

| Alternatif | Hangi koşulda kazanır |
|---|---|
| **Tek sahne** (bugünkü) | Oyun tek bir ekrandan ibaretse |
| **Additive sahne yükleme** | Kalıcı olması gereken şey bir **nesne** değil bir **katman** ise (arayüz, ışık) |
| **Kalıcı bootstrap sahnesi** | Kalıcılık gerçekten gerekiyorsa: bir sahne hiç boşaltılmaz, ötekiler üstüne yüklenir. ██ Yığılma doğmaz, çünkü nesne hiç yeniden doğmaz ██ |
| **Veriyi taşımak, nesneyi değil** | Taşınacak şey davranış değil **değer** ise (skor, seçilen zorluk); açık bir parametre olarak geçilir |

---

## 3 · `FindObjectOfType` / `FindObjectsOfType`

### ① ARKADA NE OLUYOR

**Ne tarar.** Yüklü nesnelerin tamamını tarar ve tipe uyanı döndürür; tekil sürüm
**ilkini** verir, çoğul sürüm bir **dizi** ayırır ve hepsini doldurur. Yerel
2021.3.45f2 belgesinden doğrulanan aşırı yükleme kümesi:

```
FindObjectOfType()            FindObjectsOfType()
FindObjectOfType(bool)        FindObjectsOfType(bool)
FindObjectOfType(Type)        FindObjectsOfType(Type)
FindObjectOfType(Type,bool)   FindObjectsOfType(Type,bool)
                              FindObjectsOfTypeAll(Type)
                              FindObjectsOfTypeIncludingAssets(Type)
```

██ Bir sürüm tuzağı, ölçülmüş: ██ "yeni API Unity 2022'de geldi" cümlesi bu sürüm
için **yanlıştır** — `FindObjectsByType(Type, FindObjectsInactive,
FindObjectsSortMode)` ve `FindAnyObjectByType()` aynı kurulumda zaten var. Fark
tek kelimede: eski sürüm sonucu **sıralar**, yeni sürüm sıralamayı bir
**parametre** yapar. Sıralama istemiyorsan ödemezsin — ve bu, maliyetin nereden
geldiğini gösteriyor: tarama **artı** sıralama.

**Deaktif nesneleri bulmaz.** Parametresiz sürüm için belgenin kendi cümlesi:
*"Returns the first active loaded object of Type type."* Sessiz bir `null`
kaynağıdır — nesne sahnede **durur**, sadece kapalıdır, arama onu görmez.
`bool includeInactive` alan sürüm bu kapıyı açar, ama onu çağırmayı hatırlaman
gerekir; derleyici hatırlatmaz.

**Maliyet ne ile orantılı.** Yüklü nesne sayısıyla — ve bu projede o sayı
hesaplanabilir, çünkü sahne ve prefab dosyaları okundu:

```
Awake bittiğinde sahnede kaç GameObject var:
  sahnede yazılı olanlar        2   Main Camera · Board
  BuildCellVisuals              15  3 × 5   (BoardAdapter.cs:643-649)
  iki birim × prefab başına 2   4   Unit + SelectionOverlay çocuğu
                              ────
                               21  GameObject   (~46 bileşen)
```

██ 21 nesnede tarama ölçülemez: bu projede maliyet argümanı GEÇERSİZDİR ██ ve onu
ileri sürmek dürüst olmaz.

**██ Asıl kusur maliyet DEĞİL: bağımlılık GÖRÜNMEZ olur. ██** Bir tipin neye
ihtiyacı olduğu üç yerden okunur ve üçünün görünürlüğü aynı değildir:

```
KURUCU PARAMETRESİ     ►  imzada durur; derleyici çağıranı ZORLAR
                          Combatant.cs:59 — dört bağımlılık da imzada
SERİLEŞTİRİLMİŞ ALAN   ►  Inspector'da durur; boş bırakılırsa GÖRÜLÜR
                          BoardAdapter'da 13 alan · UnitView'da 3 alan
METOT GÖVDESİNDE ARAMA ►  ██ HİÇBİR YERDE DURMAZ ██ — yalnız o metodun
                          gövdesini okuyan görür
```

Kalite kapısının kuralı bunu tek satırda söylüyor
(`unity-csharp-quality-flow.archive`, *Dependency visibility*): "reject broad
`Find`/locator access as a default." ██ Kural yazılı; arka tarafı budur:
reddedilen şey yavaşlık değil, **imzanın yalan söylemesidir** — parametresiz bir
metot "hiçbir şeye ihtiyacım yok" der. ██

**██ Ve bu projede o arama ZATEN VAR. ██** Adı `FindObjectOfType` değil,
`Camera.main`. Yerel belgesinden doğrulandı: *"The first enabled Camera component
that is tagged "MainCamera" (Read Only)."* — *ilk* · *etkin* · *etiketi eşleşen*.
Bu bir aramadır; ölçütü tip değil **etikettir**.

```csharp
// BoardAdapter.cs:586-592
if (Camera.main == null)
{
    Debug.LogError("[Board] No camera tagged MainCamera in the Scene.", this);
    return false;
}

Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
```

██ ÖLÇÜ ██ — `TryReadPointerCell` `Camera.main`'i çağrı başına **iki kez** okuyor
ve iki çağıranı var: `HandleClick` (`:762`, tıklama başına) ve `UpdatePlacement`
(`:404`) — ikincisi **yerleştirme kipindeyken her karede** koşuyor. Okumanın
motor içinde önbelleklenip önbelleklenmediği bu turda **doğrulanmadı**; yerel
belge önbellekten söz etmiyor. ██ Zaten konumuz o değil: maliyet bilinmiyor,
görünmezlik ölçülmüş — ⑤'te. ██

### ② SAHİP ETİKETİ

```
FindObjectOfType / FindObjectsOfType    ►  UnityEngine API (UnityEngine.Object)
FindObjectsByType / FindAnyObjectByType ►  UnityEngine API — 2021.3.45f2'de VAR
Camera.main                             ►  UnityEngine API (etikete göre arama)
"MainCamera" etiketi                    ►  Editor aracı + proje kararı
"varsayılan olarak reddet" kuralı       ►  proje kararı (kalite kapısı)
```

### ③ ÖLÇÜ

**(a) Sayım deneyi.** Play'e bas ve `FindObjectsOfType<Transform>().Length`
yazdır. **Tahmin: 21.** ██ Bu sayı Editor'de doğrulanmadı ██ — sahne dosyası,
prefab dosyası ve `BuildCellVisuals`'ın `3 × 5` döngüsünden türetildi. Deneyin
değeri sayı değil: tahminin tutması, "tarama neyi tarıyor" sorusunun cevabının
senin elinde olduğunu gösterir.

**(b) Görünmezlik deneyi — ██ asıl deney budur ██.** İki bağımlılığı da sil, sonra
hatanın **ne zaman** göründüğüne bak:

```
serileştirilmiş alan silinir  ►  Inspector'da boş kutu GÖRÜNÜR, ve Awake'te
                                 LogError yazılabilir — bu projede yazılıyor:
                                 BoardAdapter.cs:265 "unitPrefab is not assigned"
                                 ██ hata, ihtiyaç duyulmadan ÖNCE bildirilir ██
Find çağrısı boş döner        ►  hiçbir yerde görünmez. Hata o metot ÇAĞRILDIĞI
                                 anda doğar — belki hiç, belki üçüncü dakikada,
                                 belki yalnız bir kip açıkken
```

**(c) `Find All References` deneyi.** Bir serileştirilmiş alanı kimin doldurduğunu
Editor gösterir; bir `Find` çağrısının **kimi bulacağını** hiçbir araç
gösteremez, çünkü cevap çalışma zamanına aittir.

**(d) Deaktif deneyi.** Aranan nesneyi kapat, parametresiz sürümü çağır: `null`.
`includeInactive: true` alan sürümü çağır: nesne. ██ Aynı sahne, aynı satır, iki
farklı cevap. ██

### ④ BU PROJEDE NEREYE DÜŞERDİ

Bağımlılıklar bugün üç yoldan geliyor ve **sayıldı**:

| Yol | Sayı | Nerede |
|---|---|---|
| Serileştirilmiş alan | **13** | `BoardAdapter.cs` — `:109 :110 :113 :120 :131 :134 :137 :146 :156 :164 :167 :170 :174` |
| Serileştirilmiş alan | **3** | `UnitView.cs` — `:51` · `:59` · `:66` |
| Kurucu parametresi | — | `Combatant.cs:59` (4 bağımlılık) · `Structure.cs:51` · `PointerGesture.cs:127` · `new Battle(width, height)` |
| `GetComponent` (kendi nesnesinde) | **2** | `BoardAdapter.cs:230` (`Grid`) · `UnitView.cs:118` (`SpriteRenderer`) |
| ██ Etikete göre arama ██ | **2 satır** | `BoardAdapter.cs:586` ve `:592` — `Camera.main` |
| Tip taraması (`Find*`) | **0** | — |

██ İki `GetComponent`, bir `Find` DEĞİLDİR ██ — `GetComponent` **bu nesnenin**
bileşen listesinde arar, sahnede değil; kodda yazılı (`:227-229`): "bileşen
listesinde arar ... Listede bir Grid bulunacağını RequireComponent garanti eder."
Kapsam farkı ölçülebilir: `GetComponent<Grid>()` için aday sayısı bu
GameObject'in bileşen sayısı (**3**), `FindObjectOfType<Grid>()` için yüklü
bileşenlerin tamamı (**~46**). Ve `[RequireComponent(typeof(Grid))]` (`:105`)
birincisine bir **garanti** ekliyor; ikincisine böyle bir garanti veren hiçbir
etiket yok.

### ⑤ NE KIRAR

① **Inspector kapıları düşer.** Bugün üç `LogError` var ve üçü de `Awake`'te,
yani **kullanımdan önce**: `unitPrefab` (`:265`), `placementGhost` (`:243`),
`selectionOverlay` (`UnitView.cs:92`). Bir `Find` bu kapıların hiçbirini
üretemez — "atanmamış" diye bir hâl yoktur, yalnız "bulunamadı" vardır ve o da
çağrı anında öğrenilir.

② **██ Bedeli ZATEN ödenmiş bir yer var ██ — ve yazılı.** `UnitView`'ın test
dosyası `BoardAdapter`'ın neden sınanmadığını açıkça sayıyor:

```csharp
// Assets/Tests/EditMode/Unity/UnitViewTests.cs:12
// BoardAdapter ise Input, Camera.main, prefab ve Instantiate ister — bu
```

26 EditMode test dosyası var ve `BoardAdapter`'ı sınayan yok; gerekçe dört
kalemle sayılmış ve ikincisi `Camera.main`. ██ "Global arama testi zorlaştırır"
bir etikettir; ölçüsü budur — bir tip test yüzeyinin dışında kaldı ve gerekçe
dosyada yazılı. ██

③ **`02`'nin ölçüsü bayatlar.**
[Aşama 4](02-sonraki-asamalar.md#asama-4-singleton-ve-reddedilisin-kendisi)
"bağımlılıklar üç yoldan geliyor ve üçü de görünür" diyor; dördüncü, görünmez bir
yol açıldığı gün o cümle yanlış olur.

### ⑥ EN YAKIN ALTERNATİF

| Alternatif | Hangi koşulda kazanır |
|---|---|
| **Serileştirilmiş referans** | Sahne kenarında bir bileşen başka bir bileşeni görmek zorundaysa; boşluğu Editor gösterir |
| **Kurucu parametresi** | Düz C# nesneleri arasında; derleyici çağıranı zorlar. Bu projede baskın yol |
| **`GetComponent` + `[RequireComponent]`** | Bağımlılık **aynı GameObject** üstündeyse; kapsam bir nesneye iner, varlığı Editor garanti eder |
| **Kökte bir kez `Find`, sonra dağıt** | Yazamadığın bir sahneyi ya da eklentiyi bağlarken. Kural: arama **kenarda** kalır, oyun kodunun içine inmez |
| **`FindObjectsOfTypeAll`** | Editör aracı yazarken — deaktif ve varlık nesnelerini de bulur, ve o iş zaten çalışma zamanına ait değildir |

██ `Find` ne zaman kazanır: ██ tek seferlik göç betiği, editör aracı, test
kurulumu, "sahneyi ben kurmadım" durumları. Ortak yanları: **oyun döngüsünde
koşmuyor** olmaları.

---

## 4 · `ScriptableObject`

### ① ARKADA NE OLUYOR

**Bir `UnityEngine.Object` türevidir ama bir `Component` DEĞİLDİR.** Tek başına
en önemli cümle, ve ██ yansımayla ölçüldü ██ (2021.3.45f2,
`UnityEngine.CoreModule.dll`):

```
ScriptableObject ──► UnityEngine.Object            System.Object
MonoBehaviour    ──► Behaviour                           │
Behaviour        ──► Component                     UnityEngine.Object
Component        ──► UnityEngine.Object            ┌─────┴──────────────┐
UnityEngine.Object ► System.Object            Component          ScriptableObject
                                                   │     ██ DAL BURADA AYRILIYOR ██
                                              Behaviour
                                                   │
                                             MonoBehaviour
```

`System.Object` ile `UnityEngine.Object`'in neden aynı şey olmadığı ve bunun
`== null` üzerindeki ölçülebilir sonucu
[Unity'nin iki ömrü](../deep/dil/07-bellek-canlilik-ve-yikim.md#dorduncu-durak-unitynin-iki-omru)
durağının konusu.

**Bu yüzden bir GameObject'e BAĞLANAMAZ — ve reddi derleyici verir.** Yansımadan
okunan kısıt: `AddComponent<T>()` üzerinde `where T : UnityEngine.Component`, ve
tip alan `AddComponent(System.Type)`'ın dönüş tipi `UnityEngine.Component`.
██ Generic sürüm derleme hatası verir, tip alan sürüm çalışma zamanında
reddeder. ██ Yani "ScriptableObject'i bir nesneye ekleyemezsin" bir öğüt değil,
imzada yazılı bir kuraldır.

**Bir VARLIKTIR (asset).** Diskte bir dosya olarak yaşar, bir `.meta` dosyası ve
içindeki GUID ile kimliklenir, sahneden bağımsızdır, oyun boyunca **tek
örnektir**. `new` ile kurulmaz; doğrulanan üretim yolu
`ScriptableObject.CreateInstance` (üç aşırı yükleme). `[CreateAssetMenu]` de
doğrulandı (`T:UnityEngine.CreateAssetMenuAttribute`) — ██ ama bu attribute
hiçbir şey yaratmaz ██: tek işi Editor'ün `Assets > Create` menüsüne bir satır
eklemektir. Etiket `Unity attribute`, üreten taraf `Editor aracı`.

**██ EN SIK HATA: bir varlık, çalışma zamanı durumu taşımaya başlar. ██**
Mekanizması tek cümlede: bir `ScriptableObject` **bir dosyadır**, onu gösteren
yüzlerce nesne **aynı** nesneyi gösterir. Örneğe özel değişen bir alan — kalan
bekleme süresi, mevcut can, bu turda kaç kez vurdu — o dosyanın içine yazılırsa
**bütün** kullanıcılar aynı değeri paylaşır. Asıl tuzak ikinci yarıdadır:

```
EDITOR'DE          Play sırasında yazılan değer diske YAZILIR ve Stop'tan sonra
                   da durur; oyunu kapatıp açtığında son savaşın canı oradadır.
                   yolun adı: ScriptableObject.SetDirty  (yansımayla doğrulandı)
DERLENMİŞ OYUNDA   yazılmaz; varlık salt okunur bir pakettedir, değer oturum
                   bitince kaybolur.

██ Bu asimetri tek başına bir tuzaktır: Editor'de görülen hata derlemede
   kaybolur, derlemede görülen hata Editor'de üretilemez. ██
```

Doğrulama sınırı: bu iki satırın **davranışı** Editor koşturularak gözlenmedi —
██ doğrulanmadı ██. Doğrulanan şey, o davranışı mümkün kılan **yolun varlığıdır**.

**Yaşam döngüsü geri çağrıları `MonoBehaviour`'ınkinden FARKLIDIR.** `Awake`,
`OnEnable`, `OnDisable`, `OnDestroy` vardır; ██ `Update` YOKTUR ██ — ve sebebi
yukarıdaki tip ağacında duruyor, ezberde değil: motor kare başına dolaştığı
listeyi `Behaviour`'lardan kurar, `ScriptableObject` `Behaviour`'dan **türemez**
(yansımayla ölçüldü), o listeye giremez, `Update` diye bir ad hiç aranmaz. Geri
çağrıların **nasıl bulunduğu** — motorun abone olmadığı, adı okuduğu —
[`Awake` bir `event` DEĞİLDİR](../deep/konular/08-motor-cagri-dongusu.md#birinci-durak-awake-bir-event-degildir)
durağının konusu.

`HENÜZ YOK → Editor aracı` — `OnValidate`: Inspector'da bir değer değiştiğinde
çağrılan, **yalnız Editor'de** koşan bir mesaj. Bu projede sıfır satır; ama adı
kodda geçiyor ve bir gerekçenin parçası (`AttackProfile.cs:40`).

### ② SAHİP ETİKETİ

```
ScriptableObject tipi · CreateInstance    ►  UnityEngine API
[CreateAssetMenu]                         ►  Unity attribute (yalnız menü satırı)
.asset üretimi · .meta+GUID · Inspector   ►  Editor aracı
OnValidate                                ►  Editor aracı (derlemede koşmaz)
hangi sayının varlığa taşınacağı          ►  proje kararı
"içine çalışma zamanı durumu koyma"       ►  proje kararı — ██ motor ENGELLEMEZ ██
```

Son satır önemlidir: motorun `ScriptableObject`'e yazmayı yasaklayan hiçbir
kuralı yoktur. Alan `public` ise herkes yazar; kelepçe yine `proje kararı`.

### ③ ÖLÇÜ

**(a) Paylaşım deneyi — mekanizmanın kendisi.** Bir `ScriptableObject`'e
`public int counter;` koy, iki ayrı sahne nesnesine **aynı** varlığı ata,
birinden `counter++` yap, ikincisinden oku. Sonuç: **1**. İki nesne, tek sayı.
██ Aynı deney hem "paylaşılan tanım" cümlesinin hem de "runtime durumu buraya
yazma" kuralının kanıtıdır. ██

**(b) Kalıcılık asimetrisi deneyi.** Aynı deneyi iki kez koştur: Editor'de
(Play → artır → Stop → Inspector) ve derlenmiş oyunda (aynı akış, sonra kapat/aç).
Beklenen fark: birincisinde değer kalır, ikincisinde sıfırlanır. ██ Bu deney bu
turda koşturulmadı; sonuç doğrulanmadı. ██

**(c) Doğrulama kaybı deneyi — ██ bu proje için en keskin ölçü ██.**
`AttackProfile` bugün kurucusunda doğruluyor ve bunun bir testi **var**:

```csharp
// Assets/Tests/EditMode/Combat/AttackProfileTests.cs:31
() => new AttackProfile(damage: -1, range: 1));
```

`AttackProfile` bir `ScriptableObject`'ten türerse bu satır **derlenmez**:
`ScriptableObject` `new` ile kurulamaz, `CreateInstance` ile kurulur, ve
`CreateInstance` parametre almaz — yani kurucu **hiç çağrılmaz**. Doğrulama
`OnValidate`'e kayar, `OnValidate` yalnız Editor'de koşar, koddan üretilen profil
hiç sınanmaz. Bu, `AttackProfile.cs:38-42`'de **zaten yazılı** olan gerekçenin
ölçüsüdür. ██ SAYILDI ██ — kaç satır kırılır: `new AttackProfile` testlerde
**16**, üretimde **1**; `new MoveProfile` testlerde **7**, üretimde **1**.

### ④ BU PROJEDE NEREYE DÜŞERDİ

İki tip, ikisi de düz C# **tanım** tipi, ikisi de motoru hiç tanımayan bir
assembly'nin içinde (`noEngineReferences: true` — `GridStrategy.Core` ve
`GridStrategy.Combat`, ikisi de okundu):

```csharp
// AttackProfile.cs:6-14  (rol başlığı)
// kimlik : yok — ... bu yüzden yüzlerce asker tek bir örneği paylaşabilir
// Unity  : gerekmez — bugün düz C# nesnesi; ScriptableObject kararı
//          geldiğinde bu satır değişir, rol değişmez

// MoveProfile.cs:6-8  (rol başlığı)
// kimlik : yok — "3 menzil" olan iki nesne aynı şeydir; bütün süvari
//          sınıfı tek bir örneği paylaşabilir
```

██ İKİSİ DE "PAYLAŞILABİLİR" DİYOR. ██ Şimdi üretimde ne olduğuna bak.

**██ KOD SORUSU — doğrulandı ██**

```csharp
// BattleActions.cs:175   (Move metodunun İÇİNDE)
var profile = new MoveProfile(moveRange);
```

`Move` her çağrıldığında **yeni bir** `MoveProfile` kuruluyor — aynı sayıyla,
aynı değerlerle, her seferinde:

```
"bütün süvari sınıfı tek bir örneği paylaşabilir"   ◄── MoveProfile.cs:6-7
                    ██ ÖLÇÜ ██
üretimde paylaşılan MoveProfile örneği sayısı        ◄── 0
her Move çağrısında kurulan MoveProfile sayısı       ◄── 1
```

Aynısı bir adım hafifiyle `AttackProfile` için de geçerli: `NewCombatant` her
birime kendi profilini kuruyor (`BoardAdapter.cs:748`) — iki birim, iki profil,
aynı `damage`, aynı `attackRange`, iki ayrı nesne.
[6. desen](01-koda-gomulu-desenler.md#6-paylasilan-degismez-tanim-flyweightin-ic-durum-yarisi)
iç durum yarısının var olduğunu zaten yazıyor; borç defteri de "paylaşımı yöneten
havuz/fabrika yok" diye adlandırmış.

██ Buradaki katkı tek cümle: "paylaşılan tanım" iddiasının bugün üretimde
karşılığı YOKTUR, ve bu, `ScriptableObject`'in çözdüğü problemin işlenmiş
örneğidir. ██ Bir `.asset` dosyası tam olarak *o* paylaşılan tek örnektir: onu
kullanan herkes aynı GUID'i gösterir, çünkü gösterecek başka bir şey yoktur.

██ Dürüst karşı cümle: ██ bu kusuru düzeltmek için `ScriptableObject`
**gerekmez** — `Move`'un içindeki `new`'i dışarı çıkarmak aynı işi yapar ve
motoru hiç davet etmez. `ScriptableObject`'in satın aldığı şey paylaşım değil,
**tasarımcının kod derlemeden düzenleyebileceği bir dosyadır**. O günün koşulu
[Aşama 1](02-sonraki-asamalar.md#asama-1-scriptableobject)'de.

### ⑤ NE KIRAR

| Bugün duran karar | `ScriptableObject` geldiğinde |
|---|---|
| `GridStrategy.Core` ve `GridStrategy.Combat`'ın `noEngineReferences: true` sınırı | Tanım motora bağlandığı gün düşer; ayrıntısı `02` Aşama 1'in **C** alanında |
| Kurucudaki doğrulama (`AttackProfile.cs:55-58`) | `CreateInstance` kurucuyu çağırmaz; doğrulama `OnValidate`'e kayar ve yalnız Editor'de koşar |
| `new AttackProfile(...)` — 16 test + 1 üretim | 17 satır yeniden yazılır |
| `new MoveProfile(...)` — 7 test + 1 üretim | 8 satır |
| `AttackProfile.cs:30-34` — "`readonly struct` olsaydı paylaşım yalan olurdu" | Yeni bir sahip kazanır: paylaşımı artık **dosya kimliği** garanti eder, tip seçimi değil |
| "Bir sayı üç dosyadan birinde yaşayabilir" haritası (`../deep/kod/Unity/BoardAdapter.md`) | Dördüncü bir yer doğar: `.asset`. Ve o dosya **koddan doğmaz**, atanmadığı gün sahneyi bozar |

### ⑥ EN YAKIN ALTERNATİF

| Alternatif | Hangi koşulda kazanır |
|---|---|
| **Düz C# tanım + kökte tek örnek** | Bugünkü kusurun en küçük düzeltmesi: paylaşımı sağlar, motoru davet etmez, duvarı ayakta tutar |
| **`const` / `static readonly`** | Sayı **hiç** değişmeyecekse. Örneği var: `TurnState.FirstTurnNumber` bir `const`, `DefaultTurnOrder` bir `static readonly` |
| **`[SerializeField]` alanı** (bugünkü) | Sayı **tek** bir sahne bileşenine aitse; bugün 13 alan böyle yaşıyor |
| **JSON / CSV yükleyici** | Veri depo **dışından** geliyorsa ya da çalışma anında güncelleniyorsa. `ScriptableObject` bunu yapamaz: varlık derleme anında paketlenir |
| **`[CreateAssetMenu]` + `ScriptableObject`** | Tasarımcı Editor'de yazacaksa **ve** Inspector çizimi isteniyorsa |

---

## 5 · Nesne havuzu (object pool)

### ① ARKADA NE OLUYOR

**Havuz yok etmez.** `Destroy`'un yerine iki iş yapar: nesneyi **deaktif eder** ve
bir listeye **geri koyar**. Nesne yaşamaya devam eder — bellekte durur, alanları
**eski değerlerini tutar**, ve sonraki `Get` çağrısında **aynı** nesne geri
verilir.

```
BUGÜN                              HAVUZLA
Instantiate ► yeni nesne           Get     ► listede varsa ESKİSİ
              yeni alanlar                   ██ ESKİ ALANLARIYLA ██
              Awake koşar                    Awake KOŞMAZ · OnEnable koşar
Destroy     ► yıkım sırasına       Release ► SetActive(false) + listeye koy
              girer                          OnDisable koşar
              OnDestroy koşar                OnDestroy KOŞMAZ
```

██ Havuzun asıl işi bellek değil, SIFIRLAMA SÖZLEŞMESİDİR: ██ kim temizleyecek,
ne zaman, hangi alanı. Sıfırlanmayan bir alan bir sonraki kullanıcının hatası
olur — ve o hata **yeni** nesnede görünür, **eski** nesnenin kodunda yatar.

**Sözleşmenin kendisi bir API imzasında duruyor** — ve o API bu sürümde ██ ZATEN
VAR ██ (yansımayla doğrulandı, 2021.3.45f2):

```
UnityEngine.Pool.ObjectPool<T>   where T : class

  ctor(Func<T>   createFunc,        ◄── nasıl doğar
       Action<T> actionOnGet,       ◄── ██ SIFIRLAMA burada ██
       Action<T> actionOnRelease,   ◄── ██ ya da burada ██
       Action<T> actionOnDestroy,   ◄── havuz taşarsa gerçekten yok et
       bool      collectionCheck,   ◄── ██ ÇİFT BIRAKMA kapısı ██
       int defaultCapacity, int maxSize)

  Get() · Release(T) · Clear() · Dispose()
  CountAll · CountActive · CountInactive
```

Aynı ad alanında hazır koleksiyon havuzları da var (yansımayla doğrulandı):
`ListPool<T>`, `DictionaryPool<,>`, `HashSetPool<T>`, `LinkedPool<T>`,
`GenericPool<T>`, `CollectionPool<,>`, `UnsafeGenericPool<T>`, `PooledObject<T>`.

██ İmzayı okumak sözleşmeyi okumaktır: ██ dört delege dört ayrı soruya karşılık
geliyor, ve `actionOnGet` ile `actionOnRelease` arasındaki seçim bir **karardır**
— alma anında mı temizlersin, bırakma anında mı. İkisi de boşsa sözleşme yoktur;
ikisi de doluysa iş iki kez yapılır.

**`OnEnable`/`OnDisable` havuzda TEKRAR TEKRAR koşar, `Awake` yalnız ilk kez.**
Ayrımın kendisi motor tarafında yazılı:
[`OnEnable` tekrar eder, `Start` etmez](../deep/konular/08-motor-cagri-dongusu.md#onenable-tekrar-eder-start-etmez).
Üzerine koyduğum şey **hangi çiftin havuzdan sağ çıktığı**:

```
Awake / OnDestroy çifti     ►  ██ HAVUZDA KOPAR ██
                               Awake bir kez koştu, OnDestroy hiç koşmayacak
OnEnable / OnDisable çifti  ►  ██ HAVUZDA SAĞ KALIR ██
                               her Get bir OnEnable, her Release bir OnDisable
```

██ Bu yüzden havuzlanan bir tipte kurulum `Awake`'te değil `OnEnable`'da
olmalıdır. ██ Ama tam orada ikinci bir sorun oturuyor: **abonelikler de
`OnEnable`'dadır**, ve `+=` bir "listeye ekleme" değildir — arkasında
`Delegate.Combine` durur ve o **elemez**. Aynı hedef iki kez eklenirse davet
listesinde iki kez durur ve olay yayınlandığında dinleyici **iki kez** koşar;
arka tarafın tamamı
[`event` derleyicide neye dönüşür](../deep/dil/06-delege-arka-taraf.md#ikinci-durak-event-derleyicide-neye-donusur)
durağında.

```csharp
// BoardAdapter.cs:277-284
private void OnEnable()  { battle.UnitStateChanged += OnUnitStateChanged; }
private void OnDisable() { battle.UnitStateChanged -= OnUnitStateChanged; ... }
```

██ İyi haber: bu çift havuzda bozulmaz. ██ Kötü haber: onu simetrik tutan şey
derleyici değil, kodda adı konmuş bir disiplin (`:275-276`). Havuz o disiplini
**çoğaltır**: bir nesne yüz kez alınıp bırakılırsa disiplin yüz kez sınanır.

**Ölçüsüz havuz erken iyileştirmedir.** Tetikleyicisi ve kanıt sınırı `02`'nin
işi: [Aşama 2](02-sonraki-asamalar.md#asama-2-nesne-havuzu-object-pool) ve
[Aşama 6](02-sonraki-asamalar.md#asama-6-profil-cikarma-profiling-kanit-siniri).

### ② SAHİP ETİKETİ

```
UnityEngine.Pool.ObjectPool<T>      ►  UnityEngine API — 2021.3.45f2'de VAR
Func<T> · Action<T> · where T:class ►  C# dili
Delegate.Combine (`+=`'in arkası)   ►  .NET kütüphanesi
SetActive / OnEnable / OnDisable    ►  UnityEngine API
sıfırlama sözleşmesinin METNİ       ►  ██ proje kararı — hiçbir API vermez ██
```

██ Son satır mekanizmanın kalbidir: ██ `ObjectPool<T>` sana dört **boş** delege
parametresi verir; içini dolduran sensin ve havuzun doğruluğu tamamen o dört
gövdededir.

### ③ ÖLÇÜ

**(a) Doyma ölçüsü.** `CountAll`, `CountActive`, `CountInactive` üçünü bir kare
boyunca yazdır. `CountAll` büyümeyi bıraktığı an havuz doymuştur; o ana kadar
havuz **tahsis yapıyordur** ve "kare başına sıfır" iddiası o pencerede
geçersizdir.

**(b) Sıfırlama deneyi — ██ bu proje için somut ██.** `UnitView`'ın üç görsel hâli
var ve gri tonu bir alanda yazılı (`deadTint`, `UnitView.cs:66`). Bir birimi
öldür, görselini havuza bırak, hemen yeni bir birim al: **gri kalıyorsa**
sıfırlama sözleşmesi eksiktir. Bugün o sözleşmenin metni **var** ama yanlış yerde:

```csharp
// UnitView.cs:86 ve :100   (İKİSİ DE Awake'in içinde)
SetState(UnitState.Alive);
SetSelected(false);
```

██ Havuzdan çıkan nesnede `Awake` koşmaz, yani bu iki satır da koşmaz. ██ Havuz
geldiği gün ilk iş bunları çağrılabilir bir metoda taşımaktır.

**(c) Çift abonelik deneyi.** Aynı hedefi `+=` ile iki kez ekle, olayı **bir kez**
yayınla, dinleyicinin kaç kez koştuğunu say. Cevap: **2**. Havuzla ilgisiz
görünür; havuzun her `Get`'inde `OnEnable` koştuğu için tam olarak ilgilidir.

**(d) Tahsis ölçüsü.** Araç bu projede kurulu: Unity Test Framework'ün
`AllocatingGCMemory` kısıtı
(`Assets/Tests/EditMode/Combat/DamageRulesAllocationTests.cs:103`) ve negatif
kontrolü (`:69`).

### ④ BU PROJEDE NEREYE DÜŞERDİ — ██ SAYILDI ██

```
ÜRETİM KODU (Assets/Game/)
Instantiate       1 çağrı yeri   BoardAdapter.cs:728  Instantiate(unitPrefab, transform)
                                 tek çağıranı SpawnUnit (:709); onun tek
                                 çağıranları :260 · :261 (Awake)
                                 ██ ömür boyu toplam 2 çağrı ██
Destroy           1 çağrı yeri   BoardAdapter.cs:992  Destroy(view.gameObject)
new GameObject    2 çağrı yeri   :656 Cell_{x}_{y} ► 15 kez · :557 Structure_{x}_{y}
AddComponent      2 çağrı yeri   :671 (hücre) · :561 (yapı)
DestroyImmediate  0              (yalnız testlerde: 2 satır)

██ KARE BAŞINA DOĞAN NESNE: 0 ██
```

Buna karşılık **tahsis bilinci** projede zaten var ve havuzsuz uygulanmış — kap
yeniden kullanılıyor (`BoardAdapter.cs:201-206`, `cleanupBuffer`): "her karede
yeni bir List kurmak kare başına çöp üretirdi." ██ Bu, `ListPool<T>`'nin elle
yazılmış ve tek kullanıcılı hâlidir; ██ havuzun çözdüğü problemin en küçük örneği
zaten burada ve doğru çözülmüş.

**██ KOD SORUSU — doğrulandı ██.** Yapı görselinin bir sahibi yok:

```csharp
// BoardAdapter.cs:552-553
// GÖRSEL BİR TABLOYA KAYDEDİLMİYOR: bugün onu tekrar bulması
// gereken hiçbir çağıran yok.
```

Ama `StructureState.Destroyed` bir hâl olarak **var** (`StructureState.cs:47`) ve
`Destroy` üretim kodunda yalnız **birim** görseline uygulanıyor. Havuz geldiği
gün ilk soru bu olur: **bırakma çağrısını kim yapacak** — bugün yapı görselini
bulabilen hiçbir çağıran yok. ██ Bu bir düzeltme önerisi değil bir tespit:
sıfırlama sözleşmesinin ilk maddesi "geri veren kim" sorusudur ve bu projede o
sorunun bir yerde cevabı yok. ██

### ⑤ NE KIRAR

`02`'nin **D** alanı üç kırılmayı zaten yazıyor (`Awake` bir daha koşmaz · ölçüm
penceresi kayar · `unitViews` üçüncü bir hâl kazanır). ██ Üzerine koyduğum iki
tanesi: ██

① **Ebeveyn eski kullanımdan kalır.** `Instantiate`'in ikinci parametresi bugün
bir yaşam döngüsü kararı — `:724-725`: "ikinci parametre ebeveyni verir, böylece
tahta yok olunca birimler de gider." Havuzdan gelen nesnenin `transform.parent`'ı
**önceki** kullanımdan kalır, ve bu projede tam bir anlamı var: ebeveyn yanlışsa
"tahtayı yok et" tek çağrısı artık o nesneyi kapsamaz.

② **Hierarchy yalan söyler.** Bir sonraki satır adı yazıyor:

```csharp
// BoardAdapter.cs:729
view.name = $"Unit_{unit.Name}_{x}_{y}";
```

Ad **doğuşta** yazılıyor ve konumu içeriyor; havuzda nesne taşınır ama adı
değişmez, yani Hierarchy'de `Unit_Raider_1_3` yazan nesne başka bir hücrede
durur. ██ Bu bir hata değil bir **kanıt kirliliğidir** — ve kanıt kirliliği
hatanın kendisinden pahalıya mal olur. ██

### ⑥ EN YAKIN ALTERNATİF

| Alternatif | Hangi koşulda kazanır |
|---|---|
| **Bugünkü: doğrudan `Instantiate` / `Destroy`** | Doğum sayısı ölçülebilir bir maliyet üretmiyorsa. Bugünkü ölçü: ömür boyu 2 `Instantiate` |
| **Hiç doğurmamak** | Nesne sayısı **sabit**se: bir kez doğur, sonra yalnız veriyi değiştir. Hücreler bugün böyle — 15 hücre `Awake`'te doğar, hiç yok edilmez |
| **`SetActive(false)` + elle liste** | Tek tip, tek sahip, kısa ömür; sözleşme küçükse yeter |
| **`UnityEngine.Pool.ObjectPool<T>`** | Sözleşmenin dört delegeyle **adlandırılmış** olması isteniyorsa. Yazmadığın kod, unutmadığın maddedir |
| **`ListPool<T>` / `DictionaryPool<,>`** | Havuzlanacak şey bir sahne nesnesi değil bir **koleksiyon**sa; `cleanupBuffer` aynı işi tek kullanıcı için elle yapıyor |

---

## 6 · Üç oyun — altı mekanizma, tek tablo

██ DOĞRULAMA SINIRI: üç oyunun kaynağı kapalıdır. ██ Hiçbir hücre kaynak koda ya
da resmî belgeye karşı doğrulanmadı; hepsi **oyuncunun gördüğü** olgular
üzerinden yazıldı ve motor tarafındaki karşılıkları **doğrulanmamıştır**.
Eşleşmeyen satır `██ EŞLEŞMEZ ██` ile işaretli.

| Mekanizma | Slay the Spire | Vampire Survivors | Stardew Valley |
|---|---|---|---|
| **`static Instance`** — bir şeye her yerden erişmek | Bir koşuda tek altın, tek deste, tek tırmanış durumu vardır; kart oynandığında hasar, zırh ve kalıntı etkileri **aynı** duruma yazar | Geçen süre ve toplanan seviye tek yerde tutulur; her silahın sayacı ve her düşman dalgası **o** sayıya bakar | Tek takvim ve tek saat vardır; ekin, hayvan, kasabalı, mektup ve etkinlik hepsi aynı güne bakar |
| **`DontDestroyOnLoad`** — ekran değişirken yaşayan | Savaştan haritaya, haritadan dükkâna geçilir; **ekran değişir, koşu değişmez** — deste ve can taşınır | Ölüm ekranına geçilirken toplanan yükseltmeler ve altın korunur; harita gider, kazanım kalır | Kapıdan içeri girmek yeni bir alan yükler; envanter, para ve saat geçişte **hiç durmaz** |
| **`FindObjectOfType`** — sahnede tek olanı bulmak | Ekranda tek bir enerji göstergesi vardır ve oynanan her kart ona yazar | Ekranda tek bir oyuncu karakteri vardır; **her** düşman ona doğru yürür | Ekranda tek bir oyuncu vardır; kasabalıların "yakında mı" sorusu ona sorulur |
| **`FindObjectsOfType`** — bütün X'leri toplamak | Bir kart "bütün düşmanlara 5 hasar" der; o an masadaki düşman kümesinin **tamamı** gerekir, ve küme onlarla ölçülür | Bir silah "en yakın düşmanı" arar ve ekranda yüzlerce gövde vardır; ██ tarama tam burada gerçekten bir MALİYETTİR ██ ve kare hızı düşerken **görünür** | Bir sprinkler etrafındaki ekili karolara su verir; tarama tarlayla sınırlıdır, kasabayla değil |
| **`ScriptableObject`** — tanım ile örneğin ayrımı | Her kartın adı, maliyeti ve metni bir yerde tanımlıdır ve destedeki iki kopya aynı tanımı okur — ama birinin **yükseltilmiş** olması kopyaya aittir, tanıma değil | Her silahın seviye tablosu önceden yazılıdır; o silahın **şu anki** seviyesi ve bekleme sayacı koşuya aittir | Her tohumun büyüme günleri ve mevsimi sabittir; tarladaki bitkinin **kaçıncı gününde** olduğu o karoya aittir |
| **Nesne havuzu** — sıfırlama sözleşmesi | ██ EŞLEŞMEZ ██ Masada sürekli doğup ölen bir nesne akışı yok; eleman sayısı onlarla ölçülür ve tur başına değişir | Aynı anda yüzlerce düşman ve mermi doğar ve ölür; bir düşman öldüğünde yerine geleninin canı, hızı ve görüntüsü **sıfırlanmış** olmak zorundadır — yoksa yeni düşman öncekinin yarım canıyla doğar | Her vuruşta odun parçası, taş kırığı ve eşya damlası doğar; toplandığında ya da gün bittiğinde kaybolur |

██ Beşinci satır bu tablonun en öğretici satırıdır ██ ve tesadüf değil: üçünde de
aynı ayrım görünüyor — **tanım sabit, örnek değişken**. Kartın metni tanımdır,
yükseltilmiş olması örneğe aittir; bu tam olarak `ScriptableObject`'in en sık
ihlal edilen kuralıdır ve oyuncunun gözünde zaten görünür.

██ Doğrulanmadı diye işaretlenenler: tablonun **on sekiz hücresinin tamamı**. ██
Doğrulananlar bu dosyanın başka yerlerinde: `Assets/` altındaki sayımlar,
`ProjectVersion.txt`, yerel `UnityEngine.CoreModule.xml` ve yansıma çıktısı.

---

## Kural — bu mekanizmayı ne zaman getirirsin

```
┌ Bir şeye erişemiyorum ────────────────────────────────────────┐
│  Aynı GameObject'te?  ► GetComponent + [RequireComponent]     │
│  Sahne kenarında?     ► serileştirilmiş referans              │
│  Düz C# nesnesi?      ► kurucu parametresi (bu projede baskın) │
└──────────────────────────┬────────────────────────────────────┘
              hiçbiri olmuyorsa ► ÜÇ SORUYU AYRI AYRI SOR
                 ① teklik gerçekten bir alan değişmezi mi
                 ② sahneler arası ömür gerekli mi
                 ③ GLOBAL ERİŞİM gerekli mi

  ① evet · ② hayır · ③ hayır  ►  sahibi olan bir tip  (bugünkü Battle)
  ① evet · ② evet  · ③ hayır  ►  kalıcı bootstrap sahnesi
  ① evet · ② evet  · ③ evet   ►  ██ önce ③'ü TEKRAR sor ██ — cevap genellikle
                                  "yazmaktan kaçınmak"tır ve o bir gerekçe değil

┌ Bir sayıyı kod derlemeden değiştirmek ────────────────────────┐
│  Tek bir sahne bileşenine mi ait? ► [SerializeField] (bugünkü) │
│  Hiç değişmeyecek mi?             ► const / static readonly   │
│  Tasarımcı Editor'de mi yazacak?   ► ScriptableObject          │
│  Veri depo DIŞINDAN mı geliyor?    ► yükleyici (JSON/CSV)      │
│  ██ Yalnız PAYLAŞIM mı? ██         ► kökte tek örnek —         │
│                                      ██ motor GEREKMİYOR ██    │
└───────────────────────────────────────────────────────────────┘

┌ Çok fazla nesne doğup ölüyor GİBİ geliyor ────────────────────┐
│  ██ "gibi geliyor" bir ölçü DEĞİLDİR ██                        │
│  hedef cihazda, öncesi/sonrası eşleşen bir ölçüm var mı?      │
│      HAYIR ► hiçbir şey yapma  (bugünkü cevap)                │
│      EVET  ► sabit sayıda nesne yeter mi?                     │
│                evet  ► hiç doğurma                            │
│                hayır ► sıfırlama sözleşmesini YAZ, SONRA kur  │
└───────────────────────────────────────────────────────────────┘
```

## Yanlış hatırlanan üç şey

```
"Singleton demek `static Instance` demektir"
   DEĞİL. Üç bağımsız karar tek satıra yapıştırılmıştır: teklik ·
   sahneler arası ömür · global erişim.
   Ölçü: Assets/Game/ altında değiştirilebilir static alan sayısı 0, ama
         bir savaşta bir tahta olması GERÇEK bir değişmez ve Battle onu
         SAHİPLENEREK sağlıyor; iki BoardAdapter iki ayrı savaş üretiyor
         (BoardAdapter.cs:65-67). ██ Teklik VAR, `static` YOK. ██

"FindObjectOfType kötüdür çünkü yavaştır"
   YANLIŞ SEBEP. Bu projede tarama 21 GameObject üzerinde biter; maliyet
   ölçülemez. Asıl kusur GÖRÜNMEZLİKTİR: bağımlılık ne imzada, ne
   Inspector'da, ne testin kurulumunda durur.
   Ölçü: bedeli bu projede ZATEN ödenmiş ve yazılı —
         Assets/Tests/EditMode/Unity/UnitViewTests.cs:12 BoardAdapter'ın
         neden sınanmadığını sayıyor; listenin ikinci maddesi Camera.main.

"ScriptableObject, sahnesiz bir MonoBehaviour'dır"
   DEĞİL, ve bu yansımayla ölçüldü (2021.3.45f2):
      ScriptableObject ──► UnityEngine.Object
      MonoBehaviour ──► Behaviour ──► Component ──► UnityEngine.Object
   Dal Component'ten ÖNCE ayrılıyor. Sonucu bir öğüt değil bir imza:
      AddComponent<T>()  where T : UnityEngine.Component
   yani derleyici reddeder. Ve `Update`'in olmaması bir eksiklik değil,
   bu ağacın SONUCUDUR: motorun kare listesi Behaviour'lardan kurulur.
```

## Kaçış yolu — bu beş mekanizma hiç gelmezse

```
static Instance YOK   ►  kompozisyon kökü büyür. Bugün BoardAdapter.Awake dört
                         iş yapıyor (savaş · jest · zemin · iki birim).
                         Sınır: kurulum kodu oyun kodundan uzun olduğunda.
DontDestroyOnLoad YOK ►  tek sahne. Sınır: ikinci ekran doğduğunda
                         (menü, sonuç ekranı, kayıt yükleme).
Find* YOK             ►  serileştirilmiş referans + kurucu parametresi.
                         Sınır: sahneyi SEN kurmadığında.
ScriptableObject YOK  ►  [SerializeField] + kurucu doğrulaması.
                         Sınır: `02` Aşama 1'de ölçülmüş.
Havuz YOK             ►  doğrudan Instantiate/Destroy, mümkünse hiç doğurmamak.
                         Sınır: hedef cihazda ölçülmüş, öncesi/sonrası eşleşen
                         bir tahsis farkı.
```

██ Beşinin de ortak yanı: hiçbiri "iyi mimari" olduğu için gelmez. ██ Her biri
**ölçülmüş bir baskının** cevabıdır; baskı yoksa mekanizma bir maliyetten
ibarettir — öğrenilmesi, yazılması, sınanması ve bozulduğunda anlaşılması gereken
bir maliyet.

## İlgili

- Bu ağacın yönlendirmesi: [README.md](README.md)
- Bu mekanizmalar **ne zaman** gelir: [02-sonraki-asamalar.md](02-sonraki-asamalar.md)
- Kodda **zaten** duran desenler: [01-koda-gomulu-desenler.md](01-koda-gomulu-desenler.md)
- Kapsama tablosu: [03-kavram-borc-defteri.md](03-kavram-borc-defteri.md)
- Bellek, kök kümesi ve Unity'nin iki ömrü: [../deep/dil/07-bellek-canlilik-ve-yikim.md](../deep/dil/07-bellek-canlilik-ve-yikim.md)
- Delegenin arka tarafı ve `Delegate.Combine`: [../deep/dil/06-delege-arka-taraf.md](../deep/dil/06-delege-arka-taraf.md)
- Motorun çağrı döngüsü ve Domain Reload: [../deep/konular/08-motor-cagri-dongusu.md](../deep/konular/08-motor-cagri-dongusu.md)
- Değer, referans ve kimlik: [../deep/dil/05-deger-referans-ve-kimlik.md](../deep/dil/05-deger-referans-ve-kimlik.md)
- Assembly duvarının faturaları: [../deep/konular/02-assembly-duvari.md](../deep/konular/02-assembly-duvari.md)
- Tahtanın tek yazarı: [../deep/konular/03-tahta-sahipligi.md](../deep/konular/03-tahta-sahipligi.md)
- Tip başına ayna belgeler: [../deep/kod/README.md](../deep/kod/README.md)
- Bu ağacın kapısı: `Tools/check-curriculum-coverage.py`
- Belge bağlantı kapısı: `Tools/check-doc-links.py`
