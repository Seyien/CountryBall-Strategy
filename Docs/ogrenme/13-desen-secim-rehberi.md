# Desen seçim rehberi — on iki desen, tek oyun altyapısı

***Bu dosya **hiçbir şey önermez**.*** İçinde "şu deseni ekleyelim" cümlesi
yoktur ve olmamalıdır. Her bölüm bir **tetikleyici koşul** yazar. O koşul
gerçekleşene kadar doğru cevap "bugün yok" olarak kalır.

Sebebi [02-sonraki-asamalar.md](02-sonraki-asamalar.md) ile aynı tek cümledir:
**"bugün önemli değil" eksik bir cümledir.** Onu tamamlayan şey, önemli hâle
getirecek koşuldur. O koşul aynı cümlede yazılmazsa "bugün önemli değil", bir
yıl sonra "hiç öğrenmedim"e dönüşür.

Bu dosyanın cevapladığı soru şudur: *"tek bir oyun altyapısında bu desenlerin
hangisini hangi durumda kullanırım?"* Cevap bir liste değil, bir **testtir**.

---

## Her bölümün yedi alanı

| Alan | Ne yazar |
|---|---|
| **A · BUGÜNKÜ KARŞILIĞI** | Bu işi bugün projede ne yapıyor. Tip ve üye adıyla |
| **B · TETİKLEYİCİ KOŞUL** | Hangi somut olay bunu gerekli kılar. Sayı ya da ölçü ile |
| **C · İLK ADIM** | O gün geldiğinde değişecek **ilk** dosya |
| **D · NE KIRAR** | Bugün erken getirilirse ne bozulur |
| **E · ÖN KOŞUL** | Hangi kavram önce kapanmalı |
| **F · MOTOR KARŞILIĞI** | Unity'nin kendi mekanizması bu baskıyı zaten emiyor mu |
| **G · KOD TASLAĞI** | Desen doğduğu gün alacağı şekil, bu projenin gerçek tipleriyle |

**F alanı A'dan hemen sonra okunur.** Motor baskıyı emiyorsa B'den G'ye kadarı
bir daha hiç açılmaz. Kurumsal C# kod tabanlarında zorunlu olan bir desenin
Unity'de neden görünmediğini açıklayan tek alan budur.

**G alanının üstünde bir hüküm satırı durur.** İki değer alır: desen bugün
kodda duruyorsa **BUGÜN VAR** ve taslak gerçek kodun şeklidir; durmuyorsa
**BUGÜN YOK, KASTEN YOK** ve taslak o günün şeklidir. İkinci durumda hüküm
satırının yanında tetikleyici koşul da yazılıdır, çünkü koşulsuz bir "yok"
eksik bir cümledir.

Her bölümün sonunda bir **ÜÇ OYUN** satırı var: Slay the Spire · Vampire
Survivors · Stardew Valley. Eşleşmeyen oyun `██ EŞLEŞMEZ ██` ile işaretli ve
eşleşmeme sebebi yazılı. Satırlar **doldurulmuş** verilir; okuyandan eşleştirme
yapması istenmez.

---

## AYIRICI TEST — önüne gelen her yeni ihtiyaç için

Elinde tek bir soru olsun. Yeni bir ihtiyaç geldiğinde önce şunu sor:

> ### **"Bu ihtiyaçta İKİNCİ olan ne, ve o ikinciyi KİM seçiyor?"**

İki parçalı bir cevap ver. Önce ikinci olan şeyin **cinsini** söyle. Sonra o
ikinciler arasından seçimi **kimin** yaptığını söyle. Bu ikili tek bir desene
iner.

Ama bu sorudan önce bir adım daha var, ve rehberin en değerli parçası odur.

### ADIM 0 — motor sorusu

> **"Bunu Unity'nin kendi bir mekanizması zaten sahipleniyor mu?"**

Adaylar: `ScriptableObject` · prefab referansı · `Tilemap` · `Animator` ·
`UnityEvent` · Addressables · bileşen bileşimi · `.asmdef` sınırı.

Cevap **evet** ise desen doğmaz ve iş biter. Kurumsal bir C# projesinde
zorunlu olan birçok desen, Unity'de bir varlık dosyası ya da bir sürükle-bırak
alanı tarafından zaten ödenmiştir. Bu rehberdeki on iki desenin **altısı** bu
adımda düşüyor.

### ADIM 1 — ayırıcı tablo

| İKİNCİ olan şey | SEÇEN kim | İner |
|---|---|---|
| İkinci **sayı takımı**, davranış aynı | Tasarımcı, Inspector'da | Flyweight — ve motor onu zaten ödüyor |
| İkinci **somut tip**, çağıran adını bilmiyor | Veri (bir tanım dosyası, bir kayıt) | Factory |
| İkinci **algoritma**, aynı soruya farklı cevap | Çağıran, kod içinde, kurulumda | Strategy |
| İkinci **hâl**, aynı girdiyi başka yorumlayan | Uygulamanın kendisi, geçişle | State |
| İkinci **dinleyici**, aynı olayı isteyen | Dinleyici, abone olarak | Observer |
| İkinci dinleyici ama **yayıncıyı tanımayan** | Bir kayıt defteri | Event Bus |
| İkinci **zaman** — iş şimdi değil, sonra | Kuyruğu işleyen taraf | Command |
| İkinci **katman**, aynı sözleşmeyi sarıp üstüne ekleyen | Kurulum sırası | Decorator |
| İkinci **ekran**, aynı durumu gösteren | — | MVC / MVP |
| İkinci **arayan**, aynı hizmeti isteyen | Bir kayıt defteri | Service Locator |
| İkinci örnek **olmamalı**, tek olmak zorunlu | — | Singleton |
| İkinci **doğum**, ama birincisi ölmedi | — | Object Pooling |
| **İkinci yok** — tek | — | Desen yok. Düz sınıf yeter |

Son satır en sık gereken satırdır. Bu projede bugün on iki desenin yedisi
orada duruyor.

---

### Testi çalıştıralım — ① VERİYE inen örnek

**İhtiyaç:** *"Piyade'nin canı 30, Akıncı'nınki 20 olsun. Yarın bir Nişancı
gelsin, onunki 25 olsun."*

**ADIM 0.** Motor bunu sahipleniyor mu? **Evet.** `UnitBlueprintAsset` bir
`ScriptableObject` ve her birim türü diskte bir `.asset` dosyası. Bugün on
birim varlığı var. Onuncu birim onuncu **sınıf** değil, onuncu **dosyadır**.
Nişancı'yı doğurmak sıfır satır C# ister.

**Cevap: iş ADIM 0'da bitti.** ADIM 1'e hiç geçilmedi.

**Yine de ADIM 1'i çalıştırırsak** ne göreceğimiz öğreticidir. İkinci olan şey
bir **sayı takımıdır**; davranış aynı kalıyor, yalnız değerler değişiyor.
Seçen kişi **tasarımcıdır** ve seçimi Inspector'da yapar. Tablo bunu
Flyweight'e indiriyor — ve motor onu zaten ödemiş durumda.

***Bu örneğin asıl dersi şudur:*** yeni bir birim türü istemek, yeni bir
**tip** istemek değildir. `Combatant` `sealed` ve üretilen somut tip sayısı
bir. On birim varlığının onu da aynı `m_Script` GUID'ini taşıyor.

---

### Testi çalıştıralım — ② KURALA inen örnek

**İhtiyaç:** *"Uzak menzilli birim en yakını değil, en zayıfı hedeflesin."*

**ADIM 0.** Motor bunu sahipleniyor mu? **Hayır.** `ScriptableObject` bir
**sayı** taşır, bir **dal** taşımaz. Bir varlık dosyasına "en zayıfı seç"
yazamazsın; yazabileceğin tek şey o dalın hangi kolunu istediğini söyleyen bir
`enum` alanıdır, ve dalın kendisi yine kodda durur.

**ADIM 1.** İkinci olan şey bir **algoritmadır**. Aynı soruya — *"kimi
vurayım"* — ikinci bir cevap veriyor. Seçen taraf **çağırandır** ve seçimi
birimin türüne bakarak kurulumda yapar.

**Cevap: Strategy.**

**Ama bugün o soru hiç sorulmuyor.** `AttackOrder`'ın kurucusu hedefi hazır
alıyor; hedefi **oyuncu** tıklayarak seçiyor. Projede otomatik hedef seçen tek
satır yok, dolayısıyla birinci algoritma bile yazılmamış. Strategy'nin
tetikleyicisi "ikinci algoritma" değil, **birinci algoritmanın doğması**.

---

### Testi çalıştıralım — ③ FABRİKAYA inen örnek

**İhtiyaç:** *"Bir kışla hem savaşçı hem de küçük bir kule üretebilsin."*

**ADIM 0.** Motor bunu sahipleniyor mu? **Kısmen.** `Instantiate(prefab)`
Unity'nin kendi fabrikasıdır ve dönüş tipini prefab belirler. Ama burada
üretilen şey bir görsel değil, bir **kural nesnesidir**; `Combatant` ile
`Structure` iki ayrı çekirdek tip ve ikisi de `MonoBehaviour` değil. Motor bu
seçimi ödeyemez.

**ADIM 1.** İkinci olan şey bir **somut tiptir**. Üretimi başlatan
`StructureProduction`, dönüşün `Combatant` mi `Structure` mü olacağını
**bilmiyor**; istek `UnitBlueprint` yerine bir yapı tanımı taşıdığında dönüş
tipi de değişir. Seçen taraf **veridir**.

**Cevap: Factory.**

**Ve bugünün ölçüsü tam da bu yüzden "yok" diyor.** `StructureProduction` şu
anda `requested.CreateCombatant(...)` çağırıyor. Dönüş tipi **her zaman**
`Combatant`, `Combatant` `sealed`, üretilen somut tip sayısı bir. Çağıran
dönüş tipini biliyor. **Nesne üretmek fabrika değildir; fabrikanın ölçüsü
çağıranın dönüş TİPİNİ bilmemesidir.**

---

## Motor sorusunun bilançosu

On iki desen, ADIM 0'da ne oluyor:

| Desen | Motor emiyor mu | Emen mekanizma |
|---|---|---|
| Flyweight | **Evet, tamamen** | `ScriptableObject` varlık dosyası |
| Factory | **Evet, çoğunlukla** | `Instantiate(prefab)` ve `UnitBlueprintAsset` |
| Singleton | **Evet** | Sahnedeki tek nesne + serileştirilmiş referans |
| Service Locator | **Evet** | `[SerializeField]` bağı ve `.asmdef` duvarı |
| MVC / MVP | **Evet, daha sertiyle** | `noEngineReferences: true` derleme sınırı |
| Decorator | **Evet, farklı bir şekille** | Bileşen bileşimi (`GameObject` + N bileşen) |
| Object Pooling | **Evet — ama deseni yok etmiyor** | `UnityEngine.Pool.ObjectPool<T>` |
| State | **Kısmen** | `Animator` — ama yalnız görsel hâller için |
| Observer | **Kısmen** | `UnityEvent` — ama tip güvenliğini bırakarak |
| Command | **Hayır** | Motorda emir kuyruğu diye bir şey yok |
| Strategy | **Hayır** | Varlık dosyası sayı taşır, dal taşımaz |
| Event Bus | **Hayır** | Motorda isimsiz yayın kanalı yok |

***Yedi satır "evet" diyor ama düşen desen sayısı ALTI, ve farkı okumak
gerekiyor.*** Üstteki altı satırda motorun cevabı deseni **yok ediyor**: o
desen elle hiç yazılmıyor. Object Pooling satırı farklı; orada motor deseni
değil, deseni taşıyan **tipi** hazır veriyor. Havuz yine var, yalnız onu kimin
yazdığı sorusu ortadan kalkıyor — ve bu proje o hazır tipi bilerek almadı,
sebebi kendi bölümünde yazılı.

Kurumsal bir C# kod tabanında düşen o altısı da elle yazılır. Bu rehberin tek
cümlelik özeti budur.

---

## Desen 1 · Singleton

**Singleton nedir:** Bir tipin **tek** örneğinin olmasını garanti eden ve o
örneğe **her yerden** erişim veren şekil. İki vaat taşır ve ikisi ayrı
şeylerdir: teklik ve küresel erişim.

**A · BUGÜNKÜ KARŞILIĞI** — **Bu desen projede yok.** Üretim kodunda
değiştirilebilir hiçbir `static` alan durmuyor. Tek `static` alan `TurnState`
içindeki varsayılan tur sırası ve o da `readonly` artı salt okunur görünüm.
Tek olması gereken şeyler — tahta, savaş, sıra — birer **alan** olarak
`BoardAdapter` içinde yaşıyor ve oraya kurucudan ya da `Awake`'ten geliyor.

> **HANGİ ÖZELLİK:** Oyuncu bir savaşı yarıda bırakıp ana menüye dönebilsin,
> sonra geri gelip aynı savaşa kaldığı yerden devam edebilsin.
> **NEREYE BAĞLANIR:** `Assets/Game/Unity/BoardAdapter.cs` → `Awake`
> **NE KIRAR:** Savaşın bütün durumu bugün `BoardAdapter` alanlarında yaşıyor
> ve sahne yıkıldığında onunla birlikte yok oluyor. İkinci sahne doğduğu gün
> bu karar çöker.
> **KARARMETRE:** Evet. Ana menüye dönüp geri gelmek, Singleton hiç var
> olmasaydı da istenirdi; oyuncu bunu bir oyun özelliği olarak ister ve
> mekanizmanın adını hiç duymaz.
> **ARAŞTIRMA BORCU:** gerekmiyor. Sahne geçişinde durum taşımak bir **ömür**
> sorusudur; ölçülecek bir maliyet yok.
> **NASIL DOĞURULUR:** İkinci bir sahne dosyası doğar (ana menü) ve basamağı
> "yeni sahne varlığı"dır. Savaşın durumu `BoardAdapter` alanlarından ayrılıp
> motorsuz bir tipe taşınır; o tip `Battle` ile aynı derleme biriminde yaşar
> ve `Awake` onu **kurar**, tutmaz. Numaralı editör adımı: Unity'de
> `File > Build Profiles` penceresi açılır ve ikinci sahne listeye eklenir.
> Kaba süre 6-8 saat. ***Taşıyıcı yine Singleton değildir:*** durumu tutan tip
> sahneler arası bir varlık dosyasına ya da açıkça verilen bir referansa
> bağlanır.

**F · MOTOR KARŞILIĞI** — **Motor bu baskıyı emiyor.** Unity'de "tek olan şey"
sahnedeki tek nesnedir ve ona erişim `[SerializeField]` bir alana sürüklenerek
verilir. Sürükleme, `Instance` özelliğinin yaptığı işi **derleme zamanında** ve
**görünür** biçimde yapar. Bir de ikinci mekanizma var: `ScriptableObject`
varlığı diskte tek bir dosyadır ve onu gösteren herkes aynı dosyayı gösterir,
yani teklik zaten vardır ve küresel erişim ondan **ayrı** kalır.

**B · TETİKLEYİCİ KOŞUL** — Şu ikisi **birlikte** gerçekleştiğinde:

- Bir nesnenin sahne geçişini aşması gerektiğinde. Bugün tek sahne var ve
  `DontDestroyOnLoad` hiçbir yerde geçmiyor.
- Ve o nesneye erişecek çağıranın **hiçbir yoldan** referans alamadığı
  ölçüldüğünde. Ölçü somut: bir çağıran için serileştirilmiş alan, kurucu
  parametresi ve olay aboneliği yollarının **üçü de** denenip başarısız olmalı.

**C · İLK ADIM** — Değişecek ilk dosya bir C# dosyası **değil**, sahnenin
kendisidir. Tek örneğin nerede doğduğu bir sahne kararıdır; kodda bir
`Instance` alanı açmak o kararı gizler, çözmez.

**D · NE KIRAR** — İki şey, ve ikincisi bu projede ölçülmüş durumda:

① **Bağımlılık imzadan siler.** `Instance` üzerinden okuyan bir üye, neye
bağlı olduğunu imzasında söylemez. Okuyan kişi gövdeyi taramak zorunda kalır.

② *****EN SIK HATA*** Test edilemezlik sessizce gelir.** Bu projenin
EditMode testleri, çekirdek tiplerin küresel durum tutmaması sayesinde koşuyor;
`StructureProduction` zamanı bile dışarıdan alıyor ve o karar tipin kendi yorum
satırında yazılı. Bir `Instance` alanı doğduğu gün testler arasında **sızıntı**
başlar: bir testin bıraktığı durum sonrakine geçer ve hata testin kendisinde
görünmez.

**E · ÖN KOŞUL** — İki kavram önce kapanmalı. ① `static` alanın ömrü ve
Domain Reload ayarıyla ilişkisi. ② Serileştirilmiş referansın ne zaman
kurulduğu, yani `Awake` ile Inspector bağının sırası.

**G · KOD TASLAĞI** — **BUGÜN YOK, KASTEN YOK.** Tetikleyici: bir nesne sahne
geçişini aşmak zorunda kalırsa **ve** üç referans yolunun üçü de ölçülüp
başarısız olursa.

```csharp
// BUGÜN BÖYLE DEĞİL. Bugün BoardAdapter tahtayı, savaşı ve sırayı
// kendi ALANLARINDA tutuyor ve onlara kimse dışarıdan erişmiyor.
public sealed class BoardAdapter : MonoBehaviour
{
    public static BoardAdapter Instance { get; private set; }

    private void Awake()
    {
        // İKİNCİ KOPYA SORUSU BURADA DOĞAR ve cevabı yoktur:
        // sahne yeniden yüklenirse eski örnek mi kalır, yeni mi kazanır?
        Instance = this;
    }
}

// ve çağıran tarafta imza YALAN söylemeye başlar:
// bu üye BoardAdapter'a bağlı ama parametresinde bunu söylemiyor.
public void Refresh() => BoardAdapter.Instance.SelectionChanged += OnSelected;
```

**Bugünkü şekil bunun tam tersi ve tercih edilen şekil odur:** `UnitViewPool`
kurucusundan prefabı ve ana nesneyi alıyor, `BoardModeMachine` kurucusundan
boşta kipini alıyor. İkisi de neye bağlı olduğunu **imzasında** söylüyor.

**ÜÇ OYUN** — Slay the Spire: bir koşuda tek bir deste, tek bir can ve tek bir
altın sayacı vardır; ikinci bir deste açamazsın · Stardew Valley: kasabada tek
bir takvim ve tek bir saat vardır; her köylü aynı güne bakar ·
Vampire Survivors: `██ EŞLEŞMEZ ██` — "tek olan ne" sorusu bu oyunda **iki
ayrı ömre** bölünüyor ve tek bir cevabı yok. Koşu içinde tek olan şey oyuncu
karakteridir ve koşu bitince yok olur. Koşular arasında tek olan şey biriken
altındır ve o koşuyu **aşar**. İki farklı ömür, iki farklı sahip; ikisine tek
bir `Instance` demek yanlış olurdu.

---

## Desen 2 · Object Pooling (nesne havuzu)

**Nesne havuzu nedir:** Yok edilecek nesneyi silmek yerine kenara koyup, yeni
biri gerektiğinde **onu geri vermek**. Kazancı yaratma ve yok etme maliyetini
azaltmak. Bedeli, geri verilen nesnenin eski durumunu **sıfırlama
sözleşmesidir**.

**A · BUGÜNKÜ KARŞILIĞI** — **Bu desen projede var.** `UnitViewPool` bir
`Stack<UnitView>` üstünde duruyor, `Rent` ve `Return` üyeleriyle. Üç çağıranı
`BoardAdapter` içinde: birim yerleştirme, ölen birimin görselini geri alma ve
tahtanın toptan temizliği. Havuz ayrıca kendi ölçüsünü taşıyor —
`IdleCount` ve `CreatedCount` iki ayrı sayaç ve ikisi birden olmadan havuzun
işe yarayıp yaramadığı söylenemez.

**F · MOTOR KARŞILIĞI** — **Motor bu baskıyı hazır bir tiple emiyor ve bu
proje onu almadı.** Unity 6000.5.7f1 kullanılıyor ve `UnityEngine.Pool`
ad alanı `ObjectPool<T>` tipini sunuyor. Projede o ad alanı hiç geçmiyor;
havuz elle yazılmış. **Bu bir kusur değil, ölçülmüş bir karardır:** hazır tip
kiralama ve iade geri çağrılarını dışarıdan ister ve havuzun görünürlüğünü
azaltır. Elle yazılmış hâlde `CreatedCount` bir alandır ve bir testte
okunabilir.

**B · TETİKLEYİCİ KOŞUL** — Kare başına tahsis **ölçülebilir** hâle geldiğinde.
Bugün kare başına sıfır birim doğuyor. Somut eşik: `Instantiate` ile `Destroy`
çifti `Update` yolundan çağrılmaya başladığında. Sürekli doğup ölen ikinci bir
nesne sınıfı — mermi, hasar sayısı yazısı, vuruş efekti — bu eşiği geçen ilk
şey olur. `ProjectileView` bugün o sınıfın adayı ve havuza **bağlı değil**.

**C · İLK ADIM** — Değişecek ilk dosya `UnitViewPool` değil, o yeni nesne
sınıfının kendisidir. Havuz zaten var ve şekli belli; eksik olan, ikinci
nesnenin sıfırlama sözleşmesini yazmasıdır.

**D · NE KIRAR** — *****EN SIK HATA*** Sıfırlanmamış alan.** Havuzdan geri
gelen nesne eski durumunu taşır. Bir `UnitView` eski canını, eski rengini ya
da eski olay aboneliğini taşırsa hata **yeni** nesnede görünür ve sebebi
**eski** kullanımdadır. Bu, hata ayıklaması en zor sınıflardan biridir çünkü
neden ile sonuç arasında bir kare değil, bir oyun turu vardır.

**E · ÖN KOŞUL** — Bir kavram: yönetilen nesnenin ömrü ile `GameObject`
ömrünün ayrı olduğu. `Destroy` yerel nesneyi yıkar ama C# referansı hâlâ
elindedir.

**G · KOD TASLAĞI** — **BUGÜN VAR.** Şekil şu ve `UnitViewPool` içinde duruyor:

```csharp
// SIFIRLAMA SÖZLEŞMESİ Rent'in içindedir, çağıranın değil.
// Çağırana bırakılsaydı üç çağıranın üçünün de aynı satırı yazması
// gerekirdi ve yazmayan ilk çağıran sessiz bir hata doğururdu.
public UnitView Rent(Vector3 position, string name)
{
    // idle boşsa YENİ doğar ve CreatedCount artar;
    // doluysa eskisi geri döner ve CreatedCount ARTMAZ.
    // İki sayacın farkı havuzun işe yarayıp yaramadığını söyler.
}

public void Return(UnitView view)
{
    // Nesne YOK EDİLMEZ, yalnız pasifleşir ve yığına girer.
}
```

**İkinci nesne sınıfı doğduğunda** aynı şekil `ProjectileView` için tekrarlanır
ve o gün asıl soru "havuz mu yazayım" değil, "hangi alanları sıfırlamam
gerekiyor" olur.

**ÜÇ OYUN** — Vampire Survivors: ekranda aynı anda yüzlerce düşman doğar ve
ölür, ve bu doğum ölüm trafiği oyunun kendisidir · Stardew Valley: madende her
kat yeniden kurulur, taşlar ve canavarlar toptan doğar ·
Slay the Spire: `██ EŞLEŞMEZ ██` — sıra tabanlı bir oyunda bir turda doğan
nesne sayısı bir avuç karttır. Havuzun azaltacağı maliyet ölçülemez, çünkü
maliyet yoktur. Bu proje de bugün tam olarak orada duruyor, ve havuzu
performans için değil **görsel yeniden kullanım** için taşıyor.

---

## Desen 3 · State (durum deseni)

**State nedir:** Bir nesnenin hâlini `if` zinciriyle sormak yerine, her hâli
**kendi tipine** koymak. Aynı girdi, hangi hâlde olduğuna göre farklı iş yapar
ve hâller arası geçişi hâllerin kendisi bilir.

**A · BUGÜNKÜ KARŞILIĞI** — **Bu desen projede iki ayrı yerde ve iki ayrı
biçimde var.** ① `Assets/Game/Unity/Modes/` altında GoF biçiminde: `IBoardMode`
sözleşmesi, `IdleBoardMode` ile `StructurePlacementMode` uygulamaları ve
geçişleri yöneten `BoardModeMachine`. ② Çekirdekte `enum` tabanlı biçimde:
`UnitLifecycle` hâl geçişlerini tek kapıdan yürütüyor ve `UnitState` bir
`enum`.

İkisinin bir arada durması bir çelişki değil, bir **ölçüdür**: kip başına
davranış onlarca satır, hâl başına davranış birkaç satır.

**F · MOTOR KARŞILIĞI** — **Motor bunu kısmen emiyor ve bu projede o kısım
kullanılmıyor.** `Animator` bir durum makinesidir; hâlleri, geçişleri ve geçiş
koşullarını bir varlık dosyasında tutar. Ama sahiplendiği şey **görsel**
hâllerdir. Projede `Animator` hiç geçmiyor ve geçmemesi doğrudur:
`StructurePlacementMode` fareyi ve klavyeyi sahipleniyor, `Animator` girdi
sahipliği diye bir kavram tanımıyor.

**B · TETİKLEYİCİ KOŞUL** — Bir hâl başına biriken davranış, bir `switch`
gövdesinde okunamaz hâle geldiğinde. Ölçü somut: `UnitState` üstündeki bir
`switch` kolunun **beş satırı** aştığı gün, ya da bir hâlin **kendi alanına**
ihtiyaç duyduğu gün. İkincisi daha keskin bir sınırdır; bir `enum` alan tutamaz.

**C · İLK ADIM** — Değişecek ilk dosya `UnitLifecycle`. Geçişlerin tek kapıdan
geçmesi kararı zaten orada duruyor, ve GoF biçimine geçiş o kapının **içini**
değiştirir, yerini değil.

**D · NE KIRAR** — **Erken getirilirse hâl sayısı kadar dosya doğar ve
hiçbirinin içi dolu olmaz.** Bugün `UnitState` başına düşen davranış birkaç
satır. Onları dört ayrı dosyaya bölmek okuma maliyetini artırır, davranışı
azaltmaz. `Assets/Game/` altında `abstract` ve `virtual` bugün hiç geçmiyor ve
bu sayı bir başarıdır, bir eksik değil.

**E · ÖN KOŞUL** — Bir kavram: arayüz üzerinden çağrının hangi uygulamaya
gittiğinin **çalışma zamanında** belirlenmesi. `IBoardMode` bu kavramın projede
duran örneğidir.

**G · KOD TASLAĞI** — **BUGÜN VAR, GoF biçiminde.** Şekil şu:

```csharp
// GEÇİŞİN TEMİZLİĞİ HÂLDE, ÇAĞIRANDA DEĞİL.
// Enter/Exit çifti olmasaydı "yerleştirmeye girerken bekleyen vuruşu iptal et"
// satırı her yeni geçişte ELLE yazılırdı ve yazılmadığı gün sessiz hata olurdu.
public interface IBoardMode
{
    bool OwnsPointer { get; }
    void Enter();
    void Exit();
    void Advance();
}

// Geçiş SAHİBİ ayrı bir tip, ve kipin kendisi değil.
public sealed class BoardModeMachine
{
    public IBoardMode Current { get; }
    public void Enter(IBoardMode next);
    public void ToIdle();
}
```

**Çekirdek taraf hâlâ `enum` ve bu bilerek böyle.** `UnitLifecycle` GoF
biçimine geçtiği gün taslak şu olur, ve o gün gelmedi:

```csharp
// BUGÜN YOK, KASTEN YOK. Tetikleyici: bir UnitState kolunun kendi ALANINA
// ihtiyaç duyduğu gün — bir enum alan tutamaz, bir tip tutar.
public interface IUnitLifecycleState
{
    UnitState Value { get; }
    bool CanEnter(UnitState from);
}
```

**ÜÇ OYUN** — Slay the Spire: bir düşmanın niyeti turdan tura değişir ve aynı
kart farklı niyete karşı farklı sonuç verir · Stardew Valley: bir bitkinin
büyüme evresi vardır ve aynı sulama her evrede aynı şeyi yapmaz ·
Vampire Survivors: `██ EŞLEŞMEZ ██` — oyunda hâl geçişi diye bir şey yok.
Karakter tek bir hâlde durur ve her şey **sürekli** akar; bir düşman ya
canlıdır ya yoktur, arada bir evre yoktur. Bu, State'in oyun türüne ne kadar
bağlı olduğunu gösteren en temiz karşı örnektir.

---

## Desen 4 · Flyweight (paylaşılan değişmez tanım)

**Flyweight nedir:** Çok sayıda nesnenin **ortak** olan parçasını bir kez
tutup hepsine **aynı referansı** vermek. Nesneye özel olan parça dışarıda
kalır. Ölçüsü ikinci çağrının aynı referansı döndürmesidir.

**A · BUGÜNKÜ KARŞILIĞI** — **Bu desen projede var.** `AttackProfile` ve
`MoveProfile` değişmez tanımlar. `UnitBlueprint`, `CreateCombatant`'ı arka
arkaya iki kez çağırdığında saldırı profilini **aynı referans** olarak veriyor
ve bu karar tipin kendi yorumunda yazılı. Nesneye özel olan parça dışarıda:
can `Health` içinde, hâl `UnitLifecycle` içinde, taraf `Combatant` içinde.

**F · MOTOR KARŞILIĞI** — **Motor bunu tamamen emiyor ve bu projede o yol
alınmış durumda.** `ScriptableObject` bir **dosyadır**. Onu gösteren yüz birim
diskteki aynı dosyayı gösterir. Paylaşım kodda bir havuz yazarak değil, bir
varlık dosyasına referans vererek olur. `UnitBlueprintAsset` ve
`StructureBlueprintAsset` tam olarak bu iş için var, ve bugün on birim artı on
dört yapı varlığı diskte duruyor.

*****EN SIK HATA*** ve motor onu kolaylaştırıyor.** Bir `ScriptableObject`
çalışma zamanı durumu taşımaya başlarsa — kalan bekleme süresi, mevcut can —
onu gösteren **bütün** birimler aynı değeri paylaşır. Daha kötüsü, değer
Editor'de **kalıcı** olur; oyunu kapatıp açtığında son savaşın canı hâlâ
oradadır. Bu proje o sınırı doğru çizmiş durumda.

**B · TETİKLEYİCİ KOŞUL** — Zaten doğmuş durumda, ama bir alt eşiği var:
paylaşımı **yöneten** bir havuz gerektiğinde. Ölçü somut: aynı tanımın **kod
tarafından** yeniden kurulduğu ikinci bir yol ortaya çıktığında. Bugün böyle
bir yol var — `BoardAdapter` içindeki demo birim yolu Inspector alanlarından
tanım kuruyor ve o yol kodda "GEÇİCİ" işaretli.

**C · İLK ADIM** — Değişecek ilk dosya `BoardAdapter`. İkinci doğum yolunun
kapanması, bir havuz açmaktan önce gelir. Tek yazma kapısı olmadan paylaşım
yönetilemez.

**D · NE KIRAR** — **Değişmezlik kırılırsa desen sessizce zehre döner.** Bir
`AttackProfile` alanı yazılabilir hâle geldiği gün, bir birimin hasarını
değiştirmek **bütün** birimlerin hasarını değiştirir ve hata değiştiren
birimde görünmez.

**E · ÖN KOŞUL** — Bir kavram, ve kapalı: değer ile referans ayrımı —
[../deep/dil/05-deger-referans-ve-kimlik.md](../deep/dil/05-deger-referans-ve-kimlik.md).

**G · KOD TASLAĞI** — **BUGÜN VAR.** Şekil şu:

```csharp
public sealed class UnitBlueprint
{
    // PAYLAŞILAN PARÇA: bir kez kurulur, her savaşçıya AYNI referans gider.
    // Ölçüsü: CreateCombatant iki kez çağrılıp iki profil ReferenceEquals ile
    // karşılaştırıldığında true döner.
    public Combatant CreateCombatant(Team team);
}

// NESNEYE ÖZEL PARÇA tanımın DIŞINDA durur ve üç ayrı sahibi var:
//   Health          -> mevcut can
//   UnitLifecycle   -> hâl
//   Combatant       -> taraf
```

**ÜÇ OYUN** — Slay the Spire: bir kartın adı, maliyeti ve metni bir yerde
tanımlıdır ve destedeki iki kopya aynı tanımı okur · Vampire Survivors: her
silahın seviye tablosu önceden yazılmıştır ve ekrandaki her mermi o tabloya
bakar · Stardew Valley: her tohumun büyüme günleri sabit bir tablodan gelir ve
tarladaki her bitki o tabloya bakar. ***Üçü de eşleşiyor ve bu bir istisnadır;
bu rehberde üç oyunun üçünün birden eşleştiği tek desen budur.*** Sebebi
öğretici: paylaşılan değişmez tanım bir mimari tercih değil, **veri taşıyan her
oyunun zorunluluğudur**.

---

## Desen 5 · Factory (fabrika)

**Factory nedir:** Nesne üretimini çağırandan alıp ayrı bir sahibe veren şekil.
***Ölçüsü nesne üretmek DEĞİLDİR.*** Ölçüsü, çağıranın dönen nesnenin **somut
tipini bilmemesidir**. Çağıran tipi biliyorsa ortada fabrika yok, bir kurucu
sarmalayıcısı var.

**A · BUGÜNKÜ KARŞILIĞI** — **Bu desen projede yok, ve yokluğu ölçülmüştür.**
Üç doğum yolu var ve üçü de aynı cevabı veriyor:

| Doğum yolu | Sahip | Dönüş tipi | Çağıran tipi biliyor mu |
|---|---|---|---|
| `UnitBlueprint.CreateCombatant` | Çekirdek tanım | `Combatant` | **Evet** |
| `StructureBlueprint.CreateStructure` | Çekirdek tanım | `Structure` | **Evet** |
| `BoardAdapter.NewCombatant` | Unity katmanı, "GEÇİCİ" işaretli | `Combatant` | **Evet** |

`Combatant` `sealed`. `Structure` `sealed`. Üretilen somut tip sayısı **bir**.
`Assets/Game/` altında `abstract` ve `virtual` **hiç geçmiyor**. Bir fabrikanın
seçebileceği ikinci tip fiziksel olarak yok.

***En keskin ölçü bir imzada duruyor.*** `StructureProduction` üzerindeki
`Produce` üyesi, ürettiği şeyi `out Combatant produced` parametresiyle
döndürüyor. Somut tip **çağıranın imzasında yazılı**. Fabrika tam olarak bu
satırın imkânsız kıldığı şeydir.

> **HANGİ ÖZELLİK:** Bir kışla, savaşçının yanında küçük bir savunma kulesi de
> üretebilsin; oyuncu üretim panelinden hangisini istediğini seçsin.
> **NEREYE BAĞLANIR:** `Assets/Game/Core/Combat/StructureProduction.cs` →
> `Produce`
> **NE KIRAR:** O üyenin `out Combatant produced` parametresi çöker. Dönüş iki
> ayrı somut tip olabildiği gün çağıran artık ne aldığını bilemez.
> **KARARMETRE:** Evet. Karma üretim, Factory hiç var olmasaydı da istenirdi;
> oyuncu bir üretim **seçeneği** ister, bir fabrika istemez.
> **ARAŞTIRMA BORCU:** gerekmiyor. Üretim kararı kare başına bir kez, oyuncu
> tıkladığında çalışıyor; ölçülecek bir maliyet yok.
> **NASIL DOĞURULUR:** Basamak "elde olan varlık"tır — on dört yapı varlığı
> zaten diskte ve `Structure_Kisla` onlardan biri. Önce üretim listesi ikinci
> bir tanım cinsini kabul eder, sonra `StructureProduction` içinde ortak
> sözleşme doğar; sözleşme çekirdekte doğar, Unity katmanında değil. Numaralı
> editör adımı: Project penceresinde
> `Assets/Game/Blueprints/Structure_Kisla.asset` seçilir ve Inspector'daki
> üretim listesine ikinci bir tanım sürüklenir. Kaba süre 4-6 saat.

***BU SATIR BİR ÇELİŞKİYİ ONARIYOR.*** Çalışma günlüğü bir tur boyunca
"Factory · VAR" yazdı ve gerekçe olarak `CreateCombatant` ile `CreateStructure`
üyelerini gösterdi. O hüküm **gevşekti**: iki üye de nesne üretiyor ama ikisi
de tip seçmiyor. Doğru hüküm kavram borç defterindeki hükümdür — *"tip seçimi
gerektiren bir üretim noktası yok"*.

**F · MOTOR KARŞILIĞI** — **Motor bu baskıyı çoğunlukla emiyor ve iki ayrı
mekanizmayla emiyor.**

① `Instantiate(prefab)` Unity'nin kendi fabrikasıdır. Dönüş tipini **prefab**
belirler ve çağıran o prefabı bir `[SerializeField]` alanından alır. Yani tip
seçimi koda değil, sürükle-bırak bağına düşer. Projede tek prefab alanı var ve
`UnitView` tipini taşıyor.

② `UnitBlueprintAsset.Definition` bir varlık dosyasını düz C# tanımına
çeviriyor. Yeni bir birim türü istemek yeni bir **dosya** ister, yeni bir
**tip** değil. Bugün on birim varlığı var ve onu da aynı `m_Script` GUID'ini
taşıyor. Gövde görselinin geldiği yer bile veri: `ProductionDirector` sprite'ı
tanımın ikonundan okuyor, yani yeni bir birim **görseli** sıfır satır kod
ister.

***Kurumsal C# ile Unity'nin ayrıldığı en net yer burasıdır.*** Bir kurumsal
kod tabanında "yeni bir tür" genellikle yeni bir sınıftır ve o sınıfı seçen
şey bir fabrikadır. Unity'de "yeni bir tür" genellikle yeni bir **varlık
dosyasıdır** ve onu seçen şey bir Inspector alanıdır.

**B · TETİKLEYİCİ KOŞUL** — Şu üçünden **biri**:

- Aynı üretim çağrısının **iki farklı somut tip** döndürmesi gerektiğinde.
  Somut örnek: bir kışlanın hem `Combatant` hem `Structure` üretebilmesi.
- Üretimden **önce** çalışan bir kural doğduğunda ve o kural çağıranı
  ilgilendirmediğinde. Örnek: nüfus sınırı dolduğunda üretimin sessizce
  reddedilmesi.
- Üretilecek tipin adı **veriden** geldiğinde. Örnek: bir kayıt dosyasında
  yazan birim türünün yüklenmesi.

**C · İLK ADIM** — Değişecek ilk dosya `StructureProduction`. Bugün üretimin
tek karar noktası orada ve `requested.CreateCombatant(...)` satırı orada
duruyor. Fabrika doğarsa o satırın **dönüş tipi** değişir, yeri değişmez.

**D · NE KIRAR** — İki şey:

① **Bugün getirilirse bir ara katman doğar ve altı boş kalır.** Tek somut tip
döndüren bir fabrika, bir kurucuya verilmiş uzun bir addır.

② *****EN SIK HATA*** Varlık SAYISI bir fabrikayı gerekçelendirmez.** On birim
varlığı olması on tip olduğu anlamına gelmez. Onu da aynı sınıfın on ayrı
**veri** dosyasıdır. Bir fabrikanın gerekçesi sayı değil, **dürüst bir
çağırandır**: dönüş tipini gerçekten bilmeyen bir çağıran.

**E · ÖN KOŞUL** — İki kavram. ① Kalıtım ve arayüzün ne zaman ikinci bir somut
tip **yarattığı**. ② `sealed` anahtar sözcüğünün ne söz verdiği; bugün
`Combatant` ve `Structure` ikisi de `sealed` ve bu bir kapıdır.

**G · KOD TASLAĞI** — **BUGÜN YOK, KASTEN YOK.** Tetikleyici: aynı üretim
çağrısının iki farklı somut tip döndürmesi gerektiğinde.

```csharp
// BUGÜNKÜ ŞEKİL — fabrika DEĞİL, çünkü dönüş tipi çağıranda YAZILI.
// StructureProduction bu satırı çağırıyor ve Combatant aldığını biliyor.
produced = requested.CreateCombatant(structure.Team);


// O GÜNÜN ŞEKLİ — çağıran dönüş tipini artık BİLMİYOR.
// Ön koşul: Combatant'ın sealed'i düşer YA DA ortak bir sözleşme doğar.
public interface IProducible
{
    Team Team { get; }
}

// Fabrikanın sahibi çekirdek tanımdır, Unity katmanı DEĞİL.
// Sebebi ölçülmüş: GridStrategy.Combat asmdef'i noEngineReferences: true
// taşıyor ve üretim kararı motora bağlanırsa o duvar düşer.
public interface IUnitProducer
{
    IProducible Produce(Team team);
}

// UnitBlueprint ve StructureBlueprint ikisi de bunu uygular:
public sealed class UnitBlueprint : IUnitProducer
{
    public IProducible Produce(Team team) => CreateCombatant(team);
}

// ve çağıranın İMZASI artık somut tipi söylemez.
// BUGÜNKÜ imza şu ve fabrikayı imkânsız kılan satır odur:
//     public ProductionOutcome Produce(UnitBlueprint requested, out Combatant produced)
// O günün imzası:
public ProductionOutcome Produce(IUnitProducer requested, out IProducible produced)
{
    produced = requested.Produce(structure.Team);
    return ProductionOutcome.Allowed;
}
```

***Taslağın en pahalı satırı `IProducible`'dır ve bugün onu hak eden çağıran
yok.*** `StructureProduction` üretilen şeyle ne yapacağını **tam olarak**
biliyor. Ortak sözleşme, yalnız o bilgi kaybolduğu gün kazanç getirir.

**ÜÇ OYUN** — Slay the Spire: savaş sonu ödül ekranı üç kartı adıyla değil,
bir havuzdan **çekerek** üretir ve çeken taraf hangi kartın geleceğini
bilmez · Vampire Survivors: dalga tablosu hangi düşmanın doğacağını yazar ve
doğuran taraf o adı tablodan okur, kendisi seçmez ·
Stardew Valley: `██ EŞLEŞMEZ ██` — bir tohum ekildiğinde ne çıkacağı tohumun
**kendisinde** yazılıdır ve üretimi yapan taraf hiçbir seçim yapmaz; tek yol
vardır. Bu proje bugün tam olarak orada duruyor: `UnitBlueprint` ne
üreteceğini kendisi biliyor ve seçilecek ikinci bir şey yok.

---

## Desen 6 · Observer (gözlemci)

**Observer nedir:** Bir nesnenin değiştiğini, ona **soru sormadan** öğrenmek.
Değişen taraf yayınlar, ilgilenen taraf abone olur. Ölçüsü, yayıncının
abonelerini tip olarak **tanımamasıdır**.

**A · BUGÜNKÜ KARŞILIĞI** — **Bu desen projede var.** Üretim kodunda on bir
`public event` duruyor, beş ayrı tipte: `Battle`, `Combatant`, `UnitLifecycle`,
`BoardAdapter`, `PaletteEntryView` ve `ProductionDirector`. Zincir dört
duraklı: bir birimin hâli değişince `UnitLifecycle` yayınlıyor, `Combatant`
iletiyor, `Battle` topluyor ve `BoardAdapter` ekrana yazıyor.

**F · MOTOR KARŞILIĞI** — **Motor bunu kısmen emiyor ve bu proje o yolu
almadı.** `UnityEvent` bir olay alanıdır ve abonelikleri **Inspector'da**
kurdurur; tasarımcı bir düğmeye hangi üyenin bağlanacağını sürükleyerek seçer.
Projede `UnityEvent` hiç geçmiyor.

***Karar ölçülebilir bir takasa dayanıyor.*** `UnityEvent` aboneliği sahneye
taşır ve tasarımcıya verir; bedeli, bağın **ad üzerinden** kurulmasıdır ve
yeniden adlandırılan bir üyenin bağı derleyici uyarmadan kopar. C# `event`
tersini yapar: bağ derleme zamanında sınanır ve tasarımcı onu göremez. Bu
projede bütün olaylar kod tarafında, çünkü zincirin dört durağının dördü de
üretim tipleri arasında ve hiçbiri tasarımcı yüzeyi değil.

**B · TETİKLEYİCİ KOŞUL** — Bir olayın aboneliğini **tasarımcının** kurması
gerektiğinde. Somut ölçü: bir arayüz düğmesinin ne yapacağı kodda değil,
sahnede kararlaştırılacaksa. Bugün paletler ve paneller aboneliklerini kodda
kuruyor.

**C · İLK ADIM** — Değişecek ilk dosya `PaletteEntryView`. Dört olayıyla
projedeki en yoğun yayıncı odur ve tasarımcı yüzeyine en yakın duran tiptir.

**D · NE KIRAR** — *****EN SIK HATA*** Abonelikten çıkmamak.** Bir abone yok
edildikten sonra da yayıncının listesinde kalırsa, yayıncı ölmüş bir nesneye
çağrı yapar. Unity'de bu hata özellikle sinsi çünkü yok edilmiş bir
`MonoBehaviour` C# tarafında hâlâ `null` değildir. Ölçü: her `+=` için bir
`-=` bulunabilmeli.

**E · ÖN KOŞUL** — İki kavram. ① Delege ile olay arasındaki fark; olay,
delegenin yalnız abone ekleme ve çıkarmaya izin veren hâlidir. ② Yok edilmiş
Unity nesnesinin sahte `null` davranışı.

**G · KOD TASLAĞI** — **BUGÜN VAR.** Şekil şu:

```csharp
// YAYINCI ABONESİNİ TANIMIYOR — Observer'ın ölçüsü budur.
// Battle, BoardAdapter diye bir tip olduğunu bilmiyor.
public sealed class Battle
{
    public event Action<Unit, UnitState, UnitState> UnitStateChanged;
}

// ABONE YAYINCIYI TANIYOR — ve Observer bunu YASAKLAMAZ.
// Yasaklayan şey Event Bus'tır; farkı bu rehberin son tablosunda.
battle.UnitStateChanged += OnUnitStateChanged;
```

**ÜÇ OYUN** — Slay the Spire: bir kart oynandığında ekranın dört ayrı yerindeki
relik tepki verir ve kartın kendisi o reliklerden haberdar değildir ·
Vampire Survivors: bir düşman ölünce tecrübe taşı düşer, sayaç artar ve
gerektiğinde seviye ekranı açılır · Stardew Valley: bir gün bittiğinde
bitkiler büyür, köylülerin programı değişir ve hava yeniden belirlenir; günün
kendisi bunların hiçbirini bilmez.

---

## Desen 7 · Command (emir)

**Command nedir:** Bir eylemi **nesneye** bağlamak. Nesne olduğu için
saklanabilir, kuyruğa alınabilir, tekrarlanabilir ve geri alınabilir. Ölçüsü,
eylemin çağrıldıktan sonra da **yaşamasıdır**.

**A · BUGÜNKÜ KARŞILIĞI** — **Bu desen projede var, ve geri alma için değil.**
`IUnitOrder` sözleşmesi iki uygulama taşıyor: `AttackOrder` ve `ReviveOrder`.
Emirler `UnitOrderBook` içinde bir `Dictionary<Unit, IUnitOrder>` olarak
**yaşıyor** ve her kare `Advance` ile bir adım ilerliyor.

***Bu ayrım rehberin en sık yanlış anlaşılan noktasıdır.*** Command'ın tetiği
bu projede **geri alma** değil, **çoğul emirdi**: aynı anda birden fazla birim
kendi emrini taşıyabilmeli. Geri alma bugün hâlâ yok ve gerekmiyor.

Karşı örnek de aynı dosyada duruyor: `AttackAction`, `MoveAction` ve
`BattleActions` üçü de `static class`. Çağrılır ve biter. Saklanacak nesne yok.
Bunlara "Command" demek yanlış olur; doğru adları **akış sahibi**.

**F · MOTOR KARŞILIĞI** — **Motor bu baskıyı emmiyor.** Unity'de emir kuyruğu,
geri alma yığını ya da eylem nesnesi diye bir çalışma zamanı mekanizması yok.
Editor tarafında bir geri alma yığını var — `Undo` sınıfı — ama o yalnız
**Editor** işlemlerini geri alır ve oyunun içinde çalışmaz. Bu, on iki desen
içinde motorun en az yardım ettiği yerlerden biridir.

**B · TETİKLEYİCİ KOŞUL** — Zaten doğmuş durumda. Bir sonraki eşiği var:
**geri alma** gerektiğinde. Somut ölçü: bir emrin `Advance` üyesi, geri almayı
mümkün kılacak kadar bilgi tutmuyor bugün. Geri alma istendiği gün her emir
tipine bir `Undo` üyesi ve **eski hâlin kopyası** eklenir.

**C · İLK ADIM** — Değişecek ilk dosya `IUnitOrder`. Sözleşmeye eklenen her
üye iki uygulamayı birden zorlar ve bu iyi bir şeydir: derleyici, eksik kalan
uygulamayı **yazmadan** haber verir.

**D · NE KIRAR** — **Geri alma erken getirilirse her emir eski hâli kopyalamak
zorunda kalır ve o kopya sessizce bayatlar.** Bir emrin kaydettiği "eski can"
değeri, arada başka bir şey canı değiştirdiyse artık doğru değildir.

**E · ÖN KOŞUL** — Bir kavram: bir arayüz üzerinden tutulan nesnenin somut
tipinin sorulmaması. `IUnitOrder` bunu kendi içinde çözmüş durumda ve gerekçe
yazılı: `Describe` üyesi tip sorgusunun yerine geçiyor, yani tahta
`order is AttackOrder` diye sormuyor.

**G · KOD TASLAĞI** — **BUGÜN VAR.** Şekil şu:

```csharp
// EYLEM ÇAĞRILDIKTAN SONRA DA YAŞIYOR — Command'ın ölçüsü budur.
public interface IUnitOrder
{
    Unit Target { get; }
    OrderProgress Advance();

    // TİP SORGUSUNUN YERİNE GEÇİYOR: bu üye olmasaydı tahta
    // `order is AttackOrder` diye sorardı ve üçüncü emir cinsi
    // doğduğu gün o soru SESSİZCE eskirdi.
    string Describe();
}

// TUTUCU ayrı bir tip ve emirlerin ömrünü O sahipleniyor.
public sealed class UnitOrderBook
{
    // Emir başına DEĞİL, birim başına tek emir: sözlüğün anahtarı budur.
    public void Write(Unit unit, IUnitOrder order);
    public int CancelTargeting(Unit target);
    public void Advance();
}
```

**Geri alma doğduğu gün** eklenecek üye şu, ve bugün yok:

```csharp
// BUGÜN YOK, KASTEN YOK. Tetikleyici: oyuncunun bir hamleyi geri alması
// istendiğinde — ve o gün her emir tipi ESKİ HÂLİN kopyasını taşımak zorunda.
public interface IUnitOrder
{
    void Undo();
}
```

**ÜÇ OYUN** — Slay the Spire: oynanan kart bir çözüm kuyruğuna girer ve
etkileri sırayla açılır; kart oynandıktan sonra da bir süre **yaşar** ·
Stardew Valley: ekilen tohum ve kurulan makine birer **ertelenmiş emirdir**;
bugün verilir, yarın sonuçlanır · Vampire Survivors: `██ EŞLEŞMEZ ██` —
oyuncu emir vermez. Saldırılar otomatiktir ve tek girdi yön tuşudur.
Saklanacak, kuyruğa alınacak ya da geri alınacak bir emir nesnesi yoktur;
oyunun tasarımı Command'ı **gereksiz** kılıyor, zor kılmıyor.

---

## Desen 8 · Strategy (algoritma seçimi)

**Strategy nedir:** Aynı soruya birden fazla cevap veren algoritmaları ayrı
tiplere koyup, çağırana **hangisini kullanacağını seçtirmek**. Ölçüsü arayüzün
varlığı değildir; ölçüsü, **aynı çağıranın iki uygulama arasında seçim
yapmasıdır**.

**A · BUGÜNKÜ KARŞILIĞI** — **Bu desen projede yok, ve yokluğun ölçüsü
değişti.**

***ESKİ ÖLÇÜ BAYATLADI.*** Belge bir tur boyunca "üretim kodunda bir `interface`
var ve uygulayanı tek" diyordu. Bugün altı `interface` var ve altısı da
`GridStrategy.Unity` içinde: `IPlacementBoard`, `IBoardMode`, `IBoardModeHost`,
`IPlacementModeHost`, `IUnitOrder`, `IUnitOrderHost`. `GridStrategy.Core` ve
`GridStrategy.Combat` içinde **sıfır** `interface` duruyor.

***AMA HÜKÜM DEĞİŞMEDİ, ÇÜNKÜ ÖLÇÜ SAYI DEĞİL.*** Altı arayüzden yalnız ikisi
birden fazla uygulama taşıyor, ve ikisi de Strategy değil:

| Arayüz | Uygulama sayısı | Ne olduğu | Neden Strategy değil |
|---|---|---|---|
| `IUnitOrder` | 2 — `AttackOrder`, `ReviveOrder` | **Command** | Yaratım noktası hangi tipi istediğini biliyor; seçim yok |
| `IBoardMode` | 2 — `IdleBoardMode`, `StructurePlacementMode` | **State** | Seçimi `BoardModeMachine` bir **geçişle** yapıyor, çağıran değil |
| `IPlacementBoard` | 1 — `BoardAdapter` | Katman sözleşmesi | Seçilecek ikinci uygulama yok |

Asıl ölçü şu: projede **hedef seçen bir algoritma hiç yok**. Hedefi oyuncu
tıklayarak veriyor ve `AttackOrder` onu kurucusunda hazır alıyor.
`TargetingRules` bir seçim yapmıyor, bir **izin** veriyor — *"bu hedefe
vurulabilir mi"* sorusunu cevaplıyor.

> **HANGİ ÖZELLİK:** Birimler oyuncu tıklamadan da savaşsın; okçu menzilindeki
> en zayıf düşmanı kendi seçsin, piyade en yakınına vursun.
> **NEREYE BAĞLANIR:** `Assets/Game/Unity/Orders/AttackOrder.cs` → `Advance`
> **NE KIRAR:** `AttackOrder` hedefi kurucusunda alıyor ve hedef emrin ömrü
> boyunca sabit. Otomatik seçim doğduğu gün hedefin kare başına
> değişebilmesi gerekir ve bu karar çöker.
> **KARARMETRE:** Evet. Otomatik savaş, Strategy hiç var olmasaydı da
> istenirdi; oyuncu her vuruşu tek tek tıklamak istemiyor.
> **ARAŞTIRMA BORCU:** `performance-research` — *"aday listesi kaç birime
> kadar kare başına taranabilir, ve tarama hangi sayıdan sonra uzamsal bölmeye
> ihtiyaç duyar?"* Tahta bugün 100x50 kurulabiliyor ve tam tahta taraması
> ölçülmedi.
> **NASIL DOĞURULUR:** İki adım ve basamak ikisinde de "kod", çünkü yeni
> varlık dosyası gerekmiyor. Önce **birinci** algoritma `Advance` içinde tek
> satır olarak doğar: menzildeki en yakın düşman. Sonra ikinci algoritma
> istendiği gün ortak sözleşme çekirdekte doğar ve seçimi `UnitBlueprint`
> taşıdığı bir `enum` alanıyla yapar. Numaralı editör adımı: Project
> penceresinde `Assets/Game/Blueprints/Unit_Akinci.asset` seçilir ve
> Inspector'da yeni hedef politikası alanı ayarlanır. Kaba süre 8-10 saat.

**F · MOTOR KARŞILIĞI** — **Motor bu baskıyı emmiyor, ve sebebi keskin bir
sınırdır.** Bir `ScriptableObject` bir **sayı** taşır, bir **dal** taşımaz.
Varlık dosyasına yazabileceğin en fazla şey, hangi dalı istediğini söyleyen bir
`enum` alanıdır; dalın kendisi yine kodda durur. Bu, veriyle çözülemeyen ilk
desendir ve rehberdeki motor sorusunun sınırını çizen yerdir.

**B · TETİKLEYİCİ KOŞUL** — İki adımlı, ve **birinci adım henüz atılmadı**:

① **Birinci algoritmanın doğması.** Otomatik hedef seçen ilk satır yazıldığı
gün. Bugün o satır yok.

② Sonra **ikinci** algoritmanın istenmesi. Somut örnek: *"okçu en zayıfı,
piyade en yakını hedeflesin"*. İki cevap tek bir soruya bağlandığı gün Strategy
doğar.

**C · İLK ADIM** — Değişecek ilk dosya `TargetingRules` **değil**. O tip bir
`static class` ve izin veriyor, seçim yapmıyor. Değişecek ilk dosya
`AttackOrder`: hedefi kurucusunda hazır almayı bıraktığı gün seçim sorusu ilk
kez orada doğar.

**D · NE KIRAR** — İki şey:

① **Tek uygulamalı bir arayüz, okuyanı yalan bir esneklik vaadiyle
karşılar.** Arayüzü gören kişi "demek ki ikinci bir uygulama var" diye düşünür
ve aramaya gider.

② *****EN SIK HATA*** Aşırı yükleme Strategy sanılır.** `AttackAction.Execute`
dört aşırı yükleme taşıyor: savaşçı savaşçıya, savaşçı yapıya, yapı savaşçıya,
yapı yapıya. Bu **derleme zamanı** dağıtımıdır; hangisinin çağrıldığı
derlendiği anda bellidir ve çalışma zamanında seçilecek bir şey yoktur.
Strategy'nin tamamı çalışma zamanında olur.

**E · ÖN KOŞUL** — İki kavram. ① Arayüz üzerinden sanal dağıtım. ② Aşırı
yükleme çözümlemesinin **derleme zamanında** yapıldığı.

**G · KOD TASLAĞI** — **BUGÜN YOK, KASTEN YOK.** Tetikleyici: önce otomatik
hedef seçimi doğar, sonra ikinci bir seçim kuralı istenir.

```csharp
// BUGÜNKÜ ŞEKİL — hedef DIŞARIDAN geliyor, seçilmiyor.
// Oyuncu tıklıyor, AttackOrder hazır alıyor.
public AttackOrder(IUnitOrderHost orderHost, Unit orderAttacker, Unit orderTarget)


// O GÜNÜN ŞEKLİ — sözleşme çekirdekte doğar, Unity katmanında DEĞİL.
// Sebebi ölçülmüş: bugün altı interface'in ALTISI da GridStrategy.Unity
// içinde ve çekirdekte sıfır tane var. Hedef seçimi bir OYUN kuralıdır ve
// motorsuz sınanabilmesi gerekir.
public interface ITargetPicker
{
    // Combatant'ı DEĞİL Unit'i döndürür: kimlik ile durum ayrı tutuluyor.
    Unit Pick(Unit attacker, IReadOnlyList<Unit> candidates);
}

public sealed class NearestTargetPicker : ITargetPicker { }
public sealed class WeakestTargetPicker : ITargetPicker { }

// SEÇİMİ ÇAĞIRAN YAPAR ve seçim ömür boyu sabit kalabilir —
// State'ten ayrıldığı yer tam olarak burasıdır.
public sealed class UnitBlueprint
{
    // Varlık dosyası DALI değil, dalın ADINI taşır:
    // ScriptableObject bir enum alanı tutabilir, bir algoritma tutamaz.
    public TargetPolicy Policy { get; }
}
```

**ÜÇ OYUN** — Slay the Spire: kartlar aynı *"kimi vurayım"* sorusuna farklı
cevap verir — biri rastgele seçer, biri en çok canlıyı, biri hepsini ·
Vampire Survivors: her silah kendi hedefleme kuralını taşır — biri en yakını,
biri oyuncunun baktığı yönü, biri rastgele bir noktayı ·
Stardew Valley: `██ EŞLEŞMEZ ██` — hedefleme diye bir soru yok. Alet, imlecin
durduğu kareye uygulanır ve seçenek tektir. Seçilecek ikinci bir algoritma
olmadığı için Strategy'nin doğacağı bir zemin de yok. Bu proje bugün tam olarak
orada duruyor.

---

## Desen 9 · Decorator (sarmalayıcı)

**Decorator nedir:** Bir nesneyi, **aynı sözleşmeyi uygulayan** başka bir
nesneyle sarıp davranışa katman eklemek. Ölçüsü, sarmalayan ile sarmalananın
aynı arayüzü uygulaması ve çağıranın farkı **görememesidir**.

**A · BUGÜNKÜ KARŞILIĞI** — **Bu desen projede yok.** Sözleşme artık var —
altı `interface` duruyor — ama Decorator'ın ölçüsü sözleşme değil. Hiçbir tip
aynı sözleşmeyi uygulayıp **başka bir uygulamayı sarmalamıyor**. Sarmalama
sayısı sıfır.

Katmanlı etki de yok: zırh, güçlenme, hasar değiştirici gibi üst üste binen bir
kavram bugün oyunda bulunmuyor. Hasar hesabı `AttackAction` içinde tek geçişte
bitiyor.

> **HANGİ ÖZELLİK:** Bir yapı üstündeki birimlere zırh versin ve gelen hasarı
> azaltsın; ikinci bir etki de hasarı artırsın, ve ikisi aynı vuruşta üst üste
> binsin.
> **NEREYE BAĞLANIR:** `Assets/Game/Core/Combat/AttackAction.cs` → `Execute`
> **NE KIRAR:** O üye hasarı profilden okuyup tek adımda uyguluyor. İki
> değiştirici üst üste bindiği gün *"sıra kim tarafından belirleniyor"* sorusu
> doğar ve bugünkü tek geçişli şekil o soruyu cevaplayamaz.
> **KARARMETRE:** Evet. Zırh ve güçlenme, Decorator hiç var olmasaydı da
> istenirdi; strateji oyunlarının temel dilidir.
> **ARAŞTIRMA BORCU:** gerekmiyor. Hasar hesabı vuruş başına bir kez çalışıyor
> ve vuruş sayısını bekleme süresi kelepçeliyor.
> **NASIL DOĞURULUR:** Basamak "elde olan varlık"tır — zırh önce bir **sayı**
> olarak `StructureBlueprintAsset` üstünde doğar ve on dört yapı varlığı zaten
> diskte. İkinci değiştirici istendiği gün ortak hasar sözleşmesi çekirdekte
> doğar; o güne kadar tek değiştirici `Execute` içinde bir satırdır ve öyle
> kalmalıdır. Numaralı editör adımı: Project penceresinde
> `Assets/Game/Blueprints/Structure_Hisar.asset` seçilir ve Inspector'daki yeni
> zırh alanına bir sayı yazılır. Kaba süre 5-7 saat.

**F · MOTOR KARŞILIĞI** — **Motor bu baskıyı farklı bir şekille emiyor ve şekli
sarmalama değil, YAN YANA KOYMA.** Unity'de bir davranışa katman eklemenin
yolu bir `GameObject` üstüne ikinci bir bileşen koymaktır. Katmanlar birbirini
sarmalamaz, aynı nesne üzerinde **paralel** durur. Bu proje bunu zaten
kullanıyor: bir `UnitView` yanında `HealthBarView`, `UnitAttackView` ve
`UnitWalker` ayrı bileşenler olarak duruyor.

***İki şeklin farkı ölçülebilir.*** Sarmalama **sıra** üretir — dıştaki
içtekini çağırır ve sıra anlamlıdır. Yan yana koyma sıra üretmez ve bileşenler
birbirini beklemez. Katmanlı hasar hesabı sıra ister; görsel eklentiler
istemez. Projedeki bileşenler ikinci gruptadır ve doğru şekli almış durumdalar.

**B · TETİKLEYİCİ KOŞUL** — **Sıralı** ve **bağımsız birleşen** bir etki
doğduğunda. Somut ölçü: aynı hasar sayısını art arda değiştiren **ikinci**
değiştirici yazıldığı gün. Bir tane yeterli değildir; bir tanesi `AttackAction`
içinde bir satırdır.

**C · İLK ADIM** — Değişecek ilk dosya `AttackAction`. Hasar hesabı bugün orada
tek geçişte bitiyor ve katman fikri ilk kez orada bir sıraya ihtiyaç duyar.

**D · NE KIRAR** — İki şey:

① **Sıra görünmez bir davranış hâline gelir.** İki sarmalayıcının hangi sırayla
kurulduğu sonucu değiştirir ve o sıra kurulum kodunda, hesabın kendisinde
değil, yazılıdır. Yanlış sıra derlenir ve sessizce yanlış sayı üretir.

② *****EN SIK HATA*** Kural sınıfı zinciri Decorator sanılır.** `TargetingRules`
ve `AttackAction` birer `static class`; çağıran onları **sırayla kendisi**
çağırıyor. Sarmalama yok, örnek yok, arayüz yok. Farkın tamamı bu rehberin
son tablosunda.

**E · ÖN KOŞUL** — Bir kavram: bir arayüzü uygulayan tipin, **aynı arayüzü**
bir alan olarak tutabilmesi. Decorator'ın tamamı bu tek cümlenin üstünde durur.

**G · KOD TASLAĞI** — **BUGÜN YOK, KASTEN YOK.** Tetikleyici: aynı hasar
sayısını art arda değiştiren ikinci değiştirici yazıldığı gün.

```csharp
// BUGÜNKÜ ŞEKİL — hasar tek geçişte hesaplanıyor, katman yok.
public static AttackOutcome Execute(Combatant attacker, Combatant target, int distance)


// O GÜNÜN ŞEKLİ — sarmalayan ve sarmalanan AYNI sözleşmeyi uyguluyor
// ve çağıran hangisini tuttuğunu GÖREMİYOR.
public interface IDamageSource
{
    int Amount { get; }
}

// Taban: AttackProfile'ın taşıdığı düz sayı.
public sealed class ProfileDamage : IDamageSource { }

// Katman: içindekini SARMALIYOR ve sözleşmeyi de uyguluyor.
public sealed class ArmorReduction : IDamageSource
{
    private readonly IDamageSource inner;
    public int Amount => Math.Max(1, inner.Amount - armor);
}

// SIRA ANLAMLI: zırhın önce mi sonra mı uygulandığı sonucu değiştirir
// ve o karar KURULUMDA yazılı olur, hesabın içinde değil.
IDamageSource damage = new ArmorReduction(new ProfileDamage(profile), armor);
```

**ÜÇ OYUN** — Slay the Spire: Güç (Strength) hasarı hesaplayan zincire bir
halka ekler ve kartın kendisi değişmez; zayıflık ayrı bir halkadır ve iki
halkanın sırası sonucu değiştirir · Vampire Survivors: bir yükseltme silahın
alanını ya da mermi sayısını çarpar, silahın kendisi aynı kalır ·
Stardew Valley: `██ EŞLEŞMEZ ██` — bir aletin yükseltmesi katman **eklemez**,
aletin kendisini **değiştirir**. Bakır balta yerine demir balta gelir ve
ortada sarmalanan bir eski balta kalmaz. Sarmalama değil, yer değiştirme.

---

## Desen 10 · MVC / MVP (model ile görünümün ayrılması)

**MVC / MVP nedir:** Veriyi, ekranı ve ikisini bağlayan niyet çevirisini ayrı
tiplere koymak. Ölçüsü, modelin ekranı **hiç tanımamasıdır**.

**A · BUGÜNKÜ KARŞILIĞI** — **Bu desen adıyla yok, ama ayrımın kendisi var ve
daha sert bir biçimde var.** `UnitView` edilgen bir görünüm; kendi kararını
vermiyor, söyleneni çiziyor. Ama karşısında bir sunucu (presenter) **tipi**
yok; niyet çevirisi `BoardAdapter` içinde yaşıyor.

> **HANGİ ÖZELLİK:** Oyuncu bir birim listesi ekranı açsın ve bütün
> birimlerini canlarıyla birlikte görsün, tahtayı gezmek zorunda kalmadan.
> **NEREYE BAĞLANIR:** `Assets/Game/Unity/BoardAdapter.cs` → `SelectionChanged`
> **NE KIRAR:** Niyet çevirisi bugün `BoardAdapter` gövdesinde duruyor. İkinci
> ekran doğduğu gün aynı çeviri iki yerde tekrarlanır ve iki kopya ayrı ayrı
> bayatlar.
> **KARARMETRE:** Evet. Birim listesi ekranı, MVC hiç var olmasaydı da
> istenirdi; oyuncu kaç birimi kaldığını sayarak öğrenmek istemez.
> **ARAŞTIRMA BORCU:** gerekmiyor. Liste ekranı açıkken çizim maliyeti
> tahtanınkinden küçüktür ve tahta zaten ölçülüyor.
> **NASIL DOĞURULUR:** Basamak "yeni prefab"tır ve kalıbı hazır —
> `ProductionPanelView` bir panel prefabını zaten kuruyor. Sunucu tipi ikinci
> ekranla **birlikte** doğar, ondan önce değil; tek ekranlı bir sistemde o tip
> boş bir posta kutusudur. Numaralı editör adımı: Hierarchy'de `Canvas` altına
> yeni bir panel nesnesi eklenir ve `BoardAdapter` üstündeki alana sürüklenir.
> Kaba süre 6-8 saat.

**F · MOTOR KARŞILIĞI** — **Motor bu baskıyı emiyor, ve emme biçimi bir
desenden daha sert.** Ayrım bu projede bir isimlendirme kuralına değil,
`.asmdef` duvarına dayanıyor. `GridStrategy.Core`, `GridStrategy.Combat` ve
`GridStrategy.Battle` üçü de `noEngineReferences: true` taşıyor. Yani çekirdek
tipler `UnityEngine`'i **göremiyor**; bir model tipinin ekranı tanıması
derleyici tarafından **reddediliyor**.

***Bu, MVC'nin vaat ettiği şeyi vaat etmekle değil ZORLAMAKLA yapıyor.*** Bir
MVC uygulamasında "model view'ı tanımamalı" bir anlaşmadır ve ihlali kod
incelemesinde yakalanır. Burada ihlal **derlenmiyor**. Bir mimari kuralı
yürürlüğe koymanın en güçlü seviyesi budur.

**B · TETİKLEYİCİ KOŞUL** — **Aynı durumu gösteren ikinci bir ekran**
doğduğunda. Somut örnek: aynı birim listesinin hem tahtada hem de bir menüde
gösterilmesi. Bugün her durumun tek bir gösterimi var. Ölçü ekran sayısıdır,
dosya sayısı değil.

**C · İLK ADIM** — Değişecek ilk dosya `BoardAdapter`. Niyet çevirisi bugün
orada ve ikinci ekran doğduğu gün o gövdenin bir kısmı ayrı bir sunucu tipine
taşınır.

**D · NE KIRAR** — **Erken getirilirse bir sunucu tipi doğar ve tek işi
mesajları iletmek olur.** Tek ekranlı bir sistemde sunucu, `BoardAdapter` ile
`UnitView` arasına konmuş boş bir posta kutusudur. Duvar zaten ayrımı
sağlıyor; ikinci bir katman ayrımı artırmaz, yalnız durak sayısını artırır.

**E · ÖN KOŞUL** — Bir kavram: derleme birimi (`assembly`) nedir ve bir
`.asmdef` dosyasının bir klasörü ve altındaki bütün klasörleri sahiplendiği.
Uzun hâli:
[../deep/konular/02-assembly-duvari.md](../deep/konular/02-assembly-duvari.md).

**G · KOD TASLAĞI** — **BUGÜN YOK, KASTEN YOK.** Tetikleyici: aynı durumu
gösteren ikinci bir ekran doğduğunda.

```jsonc
// BUGÜNKÜ ŞEKİL bir C# dosyasında DEĞİL, bir asmdef dosyasında duruyor:
// GridStrategy.Core.asmdef
{
    "name": "GridStrategy.Core",
    // MODEL EKRANI GÖREMİYOR ve bu bir anlaşma değil, bir DERLEME HATASI.
    "noEngineReferences": true
}
```

```csharp
// O GÜNÜN ŞEKLİ — sunucu tipi, İKİNCİ ekran doğduğunda.
// Sunucu Unity katmanında yaşar, çünkü iki EKRANI tanıması gerekir
// ve çekirdek onları göremez.
public sealed class UnitRosterPresenter
{
    private readonly Battle battle;

    // İKİ ABONE, TEK KAYNAK: sunucunun varlık sebebi budur.
    // Tek abonede bu tip boş bir posta kutusudur.
    public void Bind(IUnitRosterScreen board, IUnitRosterScreen menu);
}
```

**ÜÇ OYUN** — Slay the Spire: aynı deste savaş ekranında, harita ekranında ve
deste görüntüleyicisinde ayrı ayrı gösterilir; üç görünüm, tek kaynak ·
Stardew Valley: aynı envanter çantada, sandıkta ve dükkânda gösterilir ·
Vampire Survivors: `██ EŞLEŞMEZ ██` — aynı durumu gösteren ikinci bir ekran
yok. Envanter ekranı yoktur, harita ekranı yoktur; her şey tek ekranda ve tek
gösterimdedir. İkinci görünüm olmadığı için model ile görünümü ayırmanın
ölçülebilir bir kazancı da yok.

---

## Desen 11 · Event Bus (olay veri yolu)

**Event Bus nedir:** Olayları **isimsiz** bir kanaldan geçirmek. Yayıncı
abonesini tanımaz, abone de yayıncıyı tanımaz; ikisi de ortadaki kayıt
defterini tanır. Observer'dan farkı budur ve tek farkı budur.

**A · BUGÜNKÜ KARŞILIĞI** — **Bu desen projede yok.** Ortak bir yayın noktası
ve kayıt defteri yok. On bir `public event` doğrudan zincir hâlinde bağlı ve
**on birinin de** abonesi yayıncısını tip olarak tanıyor.

***Ölçü sayı değil, dolaylılıktır.*** Olay sayısı üçten on bire çıktı ve bu
Event Bus'ı bir adım yaklaştırmadı. On bir olay, birbirini tanıyan dört durak
arasında dağılmış durumda.

> **HANGİ ÖZELLİK:** Oyuncu ekranın kenarında bir olay akışı görsün — *"Akıncı
> düştü"*, *"Kışla tamamlandı"*, *"Sıra düşmanda"* — ve akış her yeni olay
> türünde kendiliğinden büyüsün.
> **NEREYE BAĞLANIR:** `Assets/Game/Battle/Battle.cs` → `UnitStateChanged`
> **NE KIRAR:** Bugün her abone yayıncısını tip olarak tanıyor. Akış ekranı
> altı ayrı yayıncıyı birden dinlemek zorunda kalır ve o tanıma zinciri çöker.
> **KARARMETRE:** Evet. Olay akışı, Event Bus hiç var olmasaydı da istenirdi;
> oyuncu gözünü kaçırdığı anda ne olduğunu okumak ister.
> **ARAŞTIRMA BORCU:** gerekmiyor. Akış satır başına bir metin nesnesidir ve
> sayısı ekranla kelepçelidir.
> **NASIL DOĞURULUR:** Basamak "elde olan prefab"tır — `BattleStatusView`
> yanında ikinci bir görünüm tipi doğar ve **üç doğrudan aboneliği kendi
> içinde** kurar. Veri yolu ancak dinlenen yayıncı sayısı üçü aştığı gün
> doğar; o güne kadar doğrudan abonelik derleyici yardımını korur. Numaralı
> editör adımı: Hierarchy'de `Canvas` altına bir metin nesnesi eklenir ve yeni
> görünüm bileşeni ona takılır. Kaba süre 4-5 saat, akış ekranı için; veri
> yolu ayrı bir karardır.

**F · MOTOR KARŞILIĞI** — **Motor bu baskıyı emmiyor.** Unity'de isimsiz bir
yayın kanalı yok. `UnityEvent` bir kanal değil, bir **alan**; hâlâ belirli bir
yayıncıya ait ve hâlâ o yayıncıyı gösteren bir referans üzerinden bağlanıyor.
Bir veri yolu istendiği gün elle yazılır ya da bir bağımlılık çerçevesi
alınır. Bu, Strategy ve Command ile birlikte motorun yardım etmediği üç
desenden biridir.

**B · TETİKLEYİCİ KOŞUL** — **Bağlantı sayısı** dayanılmaz olduğunda, abone
sayısı değil. Ölçü keskin: birbirini tanıması gereken **tip çifti** sayısı.
Bugün dört durak var ve zincir doğrusal, yani bağlantı sayısı üç. Aynı olayı
**birbirinden habersiz** üçten fazla tip dinlemek zorunda kaldığı gün eşik
geçilir.

**C · İLK ADIM** — Değişecek ilk dosya `Battle`. Zincirin ortasındaki durak
odur ve bir veri yolu doğarsa ilk kesilecek bağ oradan geçer.

**D · NE KIRAR** — İki şey, ve birincisi bu projede ölçülmüş bir kazancı yok
eder:

① **Olaylar isimsizleşir ve derleyici yardımı biter.** Bugün bir aboneliği
kaldırdığında derleyici uyarır. Bir veri yolunda abonelik bir tip anahtarıyla
kurulur ve yanlış yazılan bir anahtar **çalışma zamanına** kadar sessiz kalır.

② *****EN SIK HATA*** Ölü abone.** Veri yolu abonelerini tutar ve bir abone
kendini silmezse yolun listesinde kalır. Doğrudan olayda bu hatanın kapsamı
bir yayıncıdır; veri yolunda kapsamı **bütün oyundur**.

**E · ÖN KOŞUL** — İki kavram. ① Delege ile olay farkı. ② Bir sözlükte tip
anahtarı tutmanın ne demek olduğu.

**G · KOD TASLAĞI** — **BUGÜN YOK, KASTEN YOK.** Tetikleyici: aynı olayı
birbirinden habersiz üçten fazla tip dinlemek zorunda kaldığında.

```csharp
// BUGÜNKÜ ŞEKİL — bağ TİPLİ ve YÖNLÜ. Derleyici her iki ucu da görüyor.
battle.UnitStateChanged += OnUnitStateChanged;


// O GÜNÜN ŞEKLİ — bağ isimsiz ve derleyici ARTIK GÖRMÜYOR.
public sealed class GameEventBus
{
    // Anahtar bir TİP. Yanlış tip yazmak derlenir ve sessizce hiç tetiklenmez.
    public void Subscribe<T>(Action<T> handler);
    public void Publish<T>(T message);
}

// Yayıncı artık Battle DEĞİL, ve abone Battle'ı hiç tanımıyor:
bus.Publish(new UnitStateChanged(unit, previous, current));
bus.Subscribe<UnitStateChanged>(OnUnitStateChanged);
```

***Taslağın en pahalı satırı `Subscribe<T>`'dir.*** Bugünkü şekilde yanlış bir
abonelik **derlenmez**; taslakta derlenir ve hiç tetiklenmez. Bir veri yolu
alırken satın alınan şey esnekliktir ve satılan şey derleyici yardımıdır.

**ÜÇ OYUN** — Slay the Spire: bir kart oynandığında ekranın dört ayrı yerindeki
relik aynı anda tepki verir ve hiçbiri ötekini beklemez · Stardew Valley: bir
gün bittiğinde kasabanın her yerinde birbirinden habersiz şeyler olur — bitki
büyür, köylü yürür, hava değişir · Vampire Survivors: `██ EŞLEŞMEZ ██` — koşu
boyunca tek bir önemli olay türü vardır, düşman öldü, ve onu bekleyen tek bir
şey vardır, tecrübe sayacı. Bir yayın kanalını hak edecek **çeşitlilik** yok.
Bu projenin bugünkü hâli de aynı sebeple orada duruyor.

---

## Desen 12 · Service Locator (hizmet defteri)

**Service Locator nedir:** Bağımlılıkları kurucudan almak yerine bir merkezî
defterden **istemek**. Ölçüsü, çağıranın neye bağlı olduğunu imzasında
söylememesidir.

**A · BUGÜNKÜ KARŞILIĞI** — **Bu desen projede yok.** Kayıt defteri yok.
Bağımlılıklar iki yoldan geliyor ve ikisi de görünür: kurucu parametresi
(`UnitViewPool`, `BoardModeMachine`, `AttackOrder` böyle kuruluyor) ya da
`[SerializeField]` alanı (`BoardAdapter` prefabını böyle alıyor).

> **HANGİ ÖZELLİK:** En yakın aday şudur — oyuncu bir ayarlar menüsünden ses
> seviyesini değiştirsin ve o değer bütün sahnelerde okunsun. Bu mekanizmanın
> **kendisini** zorunlu kılan bir oyun özelliği bulunamadı.
> **NEREYE BAĞLANIR:** `Assets/Game/Unity/UnitViewPool.cs` → `UnitViewPool`
> **NE KIRAR:** Üç derleme birimi bugün `noEngineReferences: true` taşıyor.
> Merkezî bir defter, çekirdeğin motor tarafındaki hizmetleri istemesini
> gerektirir ve o duvarı deler.
> **KARARMETRE:** **HAYIR.** Ayarlar menüsü Service Locator hiç var olmasaydı
> da istenirdi, ama onu bu mekanizma **taşımaz**; onu bir varlık dosyası
> taşır. Yani özellik gerçek, mekanizma zorunlu değil — bu, aday özelliğin
> mekanizmayı meşrulaştırmadığı anlamına gelir.
> **ARAŞTIRMA BORCU:** gerekmiyor. Tartışılan şey bir maliyet değil, bir
> **görünürlük**: bağın imzada mı yoksa bir defterde mi durduğu.
> **ALTINCI ALAN YOK VE YOKLUĞU HÜKMÜN KENDİSİDİR.** KARARMETRE hayır dediği
> için *"nasıl doğurulur"* alanı yazılmaz. Bu mekanizmanın bu projede meşru
> bir geleceği bulunmadı, ve bu **tam** bir hükümdür.

**F · MOTOR KARŞILIĞI** — **Motor bu baskıyı emiyor, ve iki katmanla emiyor.**

① `[SerializeField]` alanı bir hizmet defterinin yaptığı işi yapar, ama
bağlantıyı **görünür** kılar. Bir defterde bağ bir dizedir ya da bir tip
anahtarıdır; Inspector'da bağ bir **oktur** ve sahnede gözle görülür.

② `.asmdef` duvarı bir hizmet defterini fiilen **imkânsız** kılıyor. Merkezî
bir defter, çekirdek tiplerin Unity katmanındaki hizmetleri istemesini
gerektirir. `GridStrategy.Core` `GridStrategy.Unity`'yi göremiyor, dolayısıyla
o istek **derlenmiyor**.

**B · TETİKLEYİCİ KOŞUL** — Bir bağımlılığın kurucudan ve serileştirilmiş
alandan **geçemediği** ölçüldüğünde. Somut ölçü: kurucu parametre sayısı beşi
aştığı **ve** o parametrelerin en az yarısının yalnız bir alt nesneye
iletilmek için taşındığı gün. Bugün en kalabalık kurucu üç parametre alıyor.

**C · İLK ADIM** — Değişecek ilk dosya bir C# dosyası değil, bir `.asmdef`
dosyasıdır. Duvar bu deseni bugün fiilen yasaklıyor ve önce o karar tartışılır.

**D · NE KIRAR** — İki şey:

① **Duvarı deler.** Merkezî bir defter, çekirdeğin motoru görmesini isteyen ilk
şey olur. Bugün üç `.asmdef` `noEngineReferences: true` taşıyor ve bu satır
projenin en güçlü mimari güvencesidir.

② *****EN SIK HATA*** Testler sessizce birbirine bağlanır.** Defterden okuyan
bir tip, testte de defteri ister. Defter test başına sıfırlanmazsa bir testin
kaydı sonrakine sızar ve hata **sonraki** testte görünür.

**E · ÖN KOŞUL** — Bir kavram: derleme birimi ve bağımlılık yönü. Uzun hâli
[../deep/konular/02-assembly-duvari.md](../deep/konular/02-assembly-duvari.md).

**G · KOD TASLAĞI** — **BUGÜN YOK, KASTEN YOK.** Tetikleyici: bir bağımlılığın
kurucudan ve serileştirilmiş alandan geçemediği ölçüldüğünde.

```csharp
// BUGÜNKÜ ŞEKİL — bağımlılık İMZADA yazılı ve derleyici onu görüyor.
public UnitViewPool(UnitView prefab, Transform parent)
public BoardModeMachine(IBoardMode idleMode)


// O GÜNÜN ŞEKLİ — bağımlılık imzadan SİLİNİYOR.
public static class ServiceLocator
{
    public static T Get<T>();
}

// ve kurucu artık NEYE bağlı olduğunu söylemiyor:
public UnitViewPool()
{
    // İMZA YALAN SÖYLÜYOR: bu tip UnitView'a bağlı ama parametresi bunu
    // söylemiyor. Okuyan kişi GÖVDEYİ taramak zorunda.
    this.prefab = ServiceLocator.Get<UnitView>();
}
```

**ÜÇ OYUN** — Slay the Spire: `██ EŞLEŞMEZ ██` · Vampire Survivors:
`██ EŞLEŞMEZ ██` · Stardew Valley: `██ EŞLEŞMEZ ██`.

***Üç eşleşmezlik ve tek bir sebep, ve bu rehberin en öğretici satırı budur:***
Service Locator'ın oynanış tarafında **hiçbir karşılığı yok**. <!-- YOK-MUAF: bu cümle ÜÇ REFERANS OYUN hakkında bir gözlemdir, bu projede bir mekanizma yokluğu hükmü DEĞİL; bölümün kendi hükmü ve beş alanı A alanının altında duruyor --> Oyuncunun
gördüğü, hissettiği ya da isteyeceği bir şeye karşılık gelmiyor. Bir kurulum
kararıdır ve yalnız kodu yazanı ilgilendirir. Bu rehberdeki on iki desenden
oynanış karşılığı sıfır olan tek desen odur — ve o boşluk, deseni değerlendiren
tek ölçünün *"kodu okumak kolaylaştı mı"* olduğunu söyler. Bu projede cevap
hayır, çünkü kurucu ve Inspector bağı zaten görünür.

---

## YANLIŞ EŞLEŞTİRME — en sık karıştırılan çiftler

Aşağıdaki her satır, iki desenin **aynı görünmesine** rağmen farklı bir soruya
cevap verdiğini gösterir. Üçüncü sütun ayıran tek ölçüyü verir; dördüncü sütun
bu projedeki karşılığını.

| Karıştırılan | ile | AYIRAN ÖLÇÜ | Bu projede |
|---|---|---|---|
| **Factory** | **Flyweight** | Factory nesne **doğurur** ve çağıran dönüş tipini bilmez. Flyweight nesne **paylaştırır** ve tip zaten bellidir. İkinci çağrı aynı referansı döndürüyorsa Flyweight, yeni nesne döndürüyorsa Factory adayı | `UnitBlueprint.CreateCombatant` her çağrıda **yeni** `Combatant` döndürüyor ama tip hep aynı; içindeki `AttackProfile` ise **aynı referans**. Yani üretim Factory değil, taşınan profil Flyweight |
| **Strategy** | **State** | İkisi de "aynı çağıran, iki uygulama". Ayıran şey **seçimi kimin yaptığı**. Strategy'de seçimi **dışarıdaki çağıran** yapar ve seçim ömür boyu sabit kalabilir. State'te seçimi **uygulamanın kendisi** yapar, bir **geçişle** | `IBoardMode` iki uygulama taşıyor ama seçimi `BoardModeMachine.Enter` bir geçişle yapıyor. Bu **State**. Bir `ITargetPicker` doğsaydı seçimi birimin tanımı yapardı ve o **Strategy** olurdu |
| **Observer** | **Event Bus** | Ayıran tek soru: **abone yayıncıyı tip olarak tanıyor mu**. Tanıyorsa Observer. Tanımıyorsa ve arada bir kayıt defteri varsa Event Bus. Abone **sayısı** ikisini ayırmaz | On bir `public event` var ve **on birinin de** abonesi yayıncısını tanıyor. Sayı üçten on bire çıktı ve hüküm değişmedi, çünkü ölçü sayı değil dolaylılık |
| **Service Locator** | **Singleton** | İkisi de küresel erişim verir. Ayıran şey **kaç tip**. Singleton **bir** tipin tek örneğini verir. Service Locator **N** tipin örneğini bir defterden verir. Kusurları aynıdır ve ölçüsü testtir: ikisi de bağımlılığı imzadan siler | İkisi de yok. `TurnState` içindeki tek `static` alan `readonly` ve salt okunur görünüm döndürüyor, yani ne biri ne öteki |
| **Decorator** | **Kural sınıfı zinciri** | Ayıran soru: **sarmalanan şey aynı sözleşmeyi mi uyguluyor**. Decorator'da sarmalayan ile sarmalanan aynı arayüzü uygular ve çağıran farkı göremez. Kural zincirinde kurallar `static` üyelerdir ve çağıran onları **sırayla kendisi** çağırır | `TargetingRules` ve `AttackAction` ikisi de `static class`. Sarmalanacak örnek yok, uygulanacak sözleşme yok. Zincir var, Decorator yok |
| **Command** | **Akış sahibi** (`static` eylem sınıfı) | Ayıran soru: **nesne çağrıdan sonra yaşıyor mu**. Command bir nesnedir ve saklanır. Akış sahibi bir üyedir; çağrılır ve biter | `AttackOrder` bir `Dictionary` içinde **yaşıyor**. `AttackAction.Execute` çağrılır ve **biter**. Aynı savaş, iki farklı şekil, ve ikisi de doğru |
| **Strategy** | **Aşırı yükleme** (`overload`) | Ayıran şey **ne zaman seçildiği**. Aşırı yükleme **derleme zamanında** çözülür ve çalışma zamanında seçilecek bir şey kalmaz. Strategy'nin tamamı **çalışma zamanında** olur | `AttackAction.Execute` dört aşırı yükleme taşıyor: savaşçı→savaşçı, savaşçı→yapı, yapı→savaşçı, yapı→yapı. Dördü de derleme zamanı. Strategy değil |
| **Factory** | **Kurucu sarmalayıcısı** | Ayıran soru: **çağıran dönüş tipini biliyor mu**. Bilmiyorsa Factory. Biliyorsa ortada yalnız bir kurucuya verilmiş uzun bir ad var | `CreateCombatant`, `CreateStructure` ve `NewCombatant` üçü de dönüş tipini çağırana **söylüyor**. Üçü de kurucu sarmalayıcısı |
| **MVC / MVP** | **Katman duvarı** | Ayıran şey **neyin zorladığı**. MVC'de "model view'ı tanımamalı" bir **anlaşmadır** ve ihlali kod incelemesinde yakalanır. Katman duvarında ihlal **derlenmez** | Üç `.asmdef` `noEngineReferences: true` taşıyor. Ayrım var, sunucu tipi yok, ve zorlayan şey derleyici |

---

## Bu rehberin tek cümlelik özeti

Bir desen adına uzanmadan önce iki soru sorulur ve ikisi de "hangi desen"
sorusundan **önce** gelir. Birincisi motorun sorusudur: *"bunu Unity zaten
sahipleniyor mu?"* İkincisi ayırıcı sorudur: *"ikinci olan ne, ve onu kim
seçiyor?"*

Bu iki soru bugün bu projede on iki desenden **yedisini** eliyor, ve elenme
sebepleri ölçülmüş durumda. Bir desenin yokluğu bir eksiklik değildir; bir
**ölçüm sonucudur** ve o ölçümün ne zaman değişeceği her bölümün B alanında
yazılı.

---

## İlgili

- Kodda **gerçekten duran** desenler ve ölçüleri:
  [01-koda-gomulu-desenler.md](01-koda-gomulu-desenler.md)
- Bugün olmayan mekanizmaların tetikleyici koşulları:
  [02-sonraki-asamalar.md](02-sonraki-asamalar.md)
- Hangi kavramın sahip dosyası var, hangisi borç:
  [03-kavram-borc-defteri.md](03-kavram-borc-defteri.md)
- Bu ağacın yönlendirmesi: [README.md](README.md)
- Motorun sunduğu ama alınmayan mekanizmalar:
  [04-yok-olan-mekanizmalar-unity.md](04-yok-olan-mekanizmalar-unity.md)
- Katman duvarının tamamı:
  [../deep/konular/02-assembly-duvari.md](../deep/konular/02-assembly-duvari.md)
