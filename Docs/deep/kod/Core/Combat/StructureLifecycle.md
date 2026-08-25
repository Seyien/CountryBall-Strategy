# StructureLifecycle

> **Kaynak:** `Assets/Game/Core/Combat/StructureLifecycle.cs`
> **Ad alanı:** `GridStrategy.Combat` · **Assembly:** `GridStrategy.Combat` (`noEngineReferences: true`)
> **Rol:** Varlık (Entity) — kimliği var, hafızası var, yalnızca **karar** verir

Bir yapının iki durumlu yaşam döngüsü ve enkaz geri sayımı. Yalnızca karar verir
("artık yıkık", "enkaz kaldırılabilir"); hiçbir şeyi yok etmez, çizmez, sahneye
dokunmaz.

`UnitLifecycle`'ın **kısaltılmışı değil** — farklı bir kural kümesi. Burada
`UnitState.Downed`'a denk bir durum ve bir `TryRevive` yok; eksik bırakıldıkları
için değil, YANLIŞ oldukları için yoklar. Bir baraka düşüp kurtarılmayı beklemez:
ayaktadır ya da enkazdır.

**ONARIM ile DİRİLTME farkı** — bu dosyanın en kolay karıştırılan satırı: diriltme
bir DURUM geçişidir (yıkık → ayakta), onarım ise yalnızca bir SAYI değişikliğidir
(can artar, durum aynı kalır). Bu tip onarımı hiç görmez; onarım `Health`'e aittir
ve ayakta olan bir yapıda yapılır. Yıkılmış bir yapı onarılmaz — yeniden inşa
edilir, ki o da bu tipin bir geçişi değil, yepyeni bir nesnedir.

**ZAMANI KENDİ OKUMAZ.** `Tick` saniyeyi dışarıdan alır; içeride `Time.deltaTime`
yoktur. Gerekçe `UnitLifecycle`'da ölçülerek yazıldı ve burada tekrar edilmiyor,
yalnızca uygulanıyor: EditMode'da o değer sıfır DEĞİL, yani zamanı içeriden okuyan
tasarım testte patlamaz — sessizce anlamsız bir sayıyla çalışır. Tamamı
[UnitLifecycle.md](UnitLifecycle.md#tickfloat-deltaseconds)'de.

**Neyi BİLMEZ:** canın kaç olduğunu (`Health`'in işi), hangi takıma ait olduğunu
(`Structure`'ın işi), sahnede neyin silineceğini (Unity katmanının işi).

| Üye | Karar | Detay |
|---|---|---|
| `DefaultRubbleWindowSeconds` | enkaz penceresi cesetten uzun; sayı bir düğme | [↓](#defaultrubblewindowseconds) |
| `StateChanged` | **yok** — burada her geçişin bir soranı var | [↓](#statechanged) |
| `State` | `SetState` **yok**; tetiklenecek bir şey olmadığı için | [↓](#state) |
| `IsReadyForCleanup` | istek bayrakla söylenir | [↓](#isreadyforcleanup) |
| `RemainingSeconds` | ayaktayken anlamsız, 0 döner | [↓](#remainingseconds) |
| `OnHealthDepleted()` | cevabı hesaplayabilen tek yer onu döndürür | [↓](#onhealthdepleted) |
| `TryRepair()` | **yok** — bu tip canı görmez | [↓](#tryrepair) |
| `Tick(float)` | zaman dışarıdan gelir | [↓](#tickfloat-deltaseconds) |

**İlgili anlatılar:** [05-yaşam döngüsü](../../../konular/05-yasam-dongusu.md) ·
[01-olay zinciri](../../../konular/01-olay-zinciri.md)

> Kodda tipin üstündeki `DERİN ANLATIM: Docs/deep/05-yasam-dongusu.md`
> yönlendirmesi yerinde bırakıldı.

---

## DefaultRubbleWindowSeconds

Enkaz penceresi cesetten uzun: yıkık bina bir **harita işaretidir**, oyuncu ona
bakıp "burada bir şey oldu" der. Sayı bir denge düğmesidir ve kurucudan
değiştirilebilir; bu dosyanın sahiplendiği kural sayı değil, "sayaç işler ve
dolunca temizlik İSTENİR" cümlesidir.

---

## StateChanged

**EVENT, GEÇİŞİN SORANI YOKKEN DOĞAR — ve burada her yıkımın soranı var.**

Bu tipte `StateChanged` yok. `UnitLifecycle`'daki gerekçe körü körüne
kopyalanmadı, sınandı ve **burada geçersiz çıktı.** Oradaki tek cümle şuydu: "dönüş
değeri — soran zaten orada; event — ilgilenen başka yerde." Orada event'i haklı
çıkaran şey `Tick`'in içindeki `Downed → Dead` geçişiydi: onu kimse SORMUYORDU,
`Tick`'i çeviren oyun döngüsü o geçişle ilgilenmiyordu. Burada `Tick`'in içinde
hiçbir DURUM geçişi yok — `Tick` yalnızca bir bayrak açıyor. Tek geçiş (ayakta →
yıkık) her zaman bir hasar çağrısından doğar ve o çağrıyı yapan taraf cevabı zaten
dönüş değeriyle alır.

### HARİTA: her geçiş nereden doğuyor, soranı var mı

Kararı belirleyen şey durum sayısı değil, geçişlerin DOĞDUĞU yer:

```
  UnitLifecycle                     StructureLifecycle (bu tip)
  ───────────────────────────────   ────────────────────────────────
  Alive → Downed                    Standing → Destroyed
    OnHealthDepleted içinde            OnHealthDepleted içinde
    çağıran SORUYOR                    çağıran SORUYOR ve cevabı
                                       DÖNÜŞ DEĞERİYLE alıyor  ✓

  Downed → Dead                     (BÖYLE BİR GEÇİŞ YOK)
    Tick içinde, kimse sormadan
    ◄── >> EVENT'İ HAKLI ÇIKARAN
        TEK SATIR ORASIYDI <<

  Dead → temizlik                   Destroyed → temizlik
    Tick, ama DURUM değişmiyor        Tick, ama DURUM değişmiyor
    bayrak açılıyor: yeter            bayrak açılıyor: yeter
```

### KAPSAM: "event kullanma" DEĞİL

Ayırt edici soru: **geçişi doğuran çağrıyı yapan taraf, geçişle İLGİLENEN taraf
mı?**

Karşı örnek aynı ad alanında, `UnitLifecycle.cs`:

```csharp
public event Action<UnitState> StateChanged;
```

Orada event VAR ve doğrudur — çünkü `Downed → Dead` geçişini `Tick`'i çeviren oyun
döngüsü doğurur ve o döngü geçişle ilgilenmez; ilgilenen (ceset efekti, ses, skor)
başka yerdedir. Yani gerekçe kopyalanmadı, aynı soru burada da soruldu ve cevabı
FARKLI çıktı.

### İŞ BÖLÜMÜ: dönüş değeri ile bayrak örtüşmez, bölüşür

```
OnHealthDepleted -> bool  ► SORANI OLAN geçiş (ayakta → yıkık)
IsReadyForCleanup         ► SORANI OLMAYAN, zamanla gelen olgu;
                            ama bu bir geçiş değil, bir istek ve
                            okuyan zaten Tick'i çeviren taraf
```

Dönüş değeri silinirse "bu vuruş mu yıktı" cevabı her çağıranda üç satırla elle
kurulur ([↓ OnHealthDepleted](#onhealthdepleted)). Bayrak silinirse enkazın süresi
dolduğunu kimse öğrenemez ve `Tick`'in tek ürünü kaybolur. Üçüncü bir mekanizma —
event — ikisinin de kapattığı bir boşluğu ikinci kez kapatırdı: aynı olgu iki
yoldan duyurulur ve hem dönüşü okuyan hem abone olan UI yıkım sesini iki kez
çalar.

### EVENT'İN GÖRÜNMEYEN BEDELİ: güçlü referans

Bir event, YAYINCIDAN ABONEYE güçlü bir referans tutar. Bu tip saf Core'dur ve ömrü
uzundur; abone olan bir Unity nesnesi yok edildikten sonra bile abonelik
çözülmediyse o nesne toplanamaz. Çözmek de ücretsiz değildir: `-=` yalnızca ABONE
OLUNAN delege örneğiyle çalışır, aynı gövdeyi taşıyan ikinci bir lambda EŞİT
DEĞİLDİR (kapanış kimliği). `Battle` bu bedeli birim başına bir sözlükle ödüyor —
`stateForwarders` — ve gerekçesi orada yazılı. Burada event olmadığı için o
sözlüğün karşılığı da hiç doğmuyor.

### REDDEDILEN

```csharp
public event Action<StructureState> StateChanged;
```

**KIRILAN:** aynı olgu iki yoldan birden duyurulur ve çağıran ikisini de duyar.

```
OnHealthDepleted "bu çağrı yıktı mı" cevabını zaten döndürüyor
hem dönüşü okuyan hem abone olan UI -> yıkım sesi iki kez çalar
abonelik çözülmezse saf Core nesnesi ölü Unity nesnesini canlı tutar
derleyici: hiçbir şey der  ·  test: hiçbiri kırmızıya dönmez
```

**KAZANIRDI:** yapı, kimsenin ÇAĞRI yapmadığı bir yoldan yıkılabilseydi — yanıp
kendiliğinden çöken bina, enerjisi kesilen kule, sahipsiz alan hasarı. O gün
geçişin "soranı" olmazdı.

**TEK CUMLE:** Dönüş değeri "soran zaten orada" demektir, event "ilgilenen başka
yerde"; burada her yıkımın bir soranı var.

---

## State

Yapının o anki durumu; yeni yapı ayakta doğar. `{ get; private set; }`.

### NEDEN SetState YOK

`UnitLifecycle`'daki tek giriş noktası deseninin gerekçesi orada yazılı —
"`State`'e doğrudan yazan bir satır kalsaydı, o geçiş sessizce kaybolurdu" ve hata
"bazen event gelmiyor" diye çıkardı. Burada event yok, dolayısıyla kaybolacak bir
şey de yok; `SetState` bugün yalnızca bir yönlendirme katmanı olurdu.

**Deseni geri getirecek tetikleyici nettir ve bilerek yazılıyor:** bu tipe bir
event, bir geçiş kaydı (log) ya da ikinci bir geçiş yolu eklendiği GÜN, `State`'e
yazan tek satır kalmalı.

### HARİTA: `State`'e yazan satırlar

```
  BU TİP                            UnitLifecycle
  ────────────────────────────────  ──────────────────────────────
  kurucu: State = Standing          kurucu: State = Alive
  OnHealthDepleted: = Destroyed     SetState(Downed)   ← OnHealthDepleted
  ◄── >> TOPLAM İKİ YAZAN <<        SetState(Alive)    ← TryRevive
  ve ikisi de bir GEÇİŞ             SetState(Dead)     ← Tick
  yayını tetiklemiyor               ◄── >> ÜÇ YAZAN + BİR YAYIN <<
```

Orada `SetState`'in işi yönlendirme değil, her geçişte event'i tetiklemek ve aynı
duruma geçişi elemekti; buradaki iki satırın tetikleyeceği bir şey yok.

### KAPSAM: "tek giriş noktası her zaman iyidir" DEĞİL

Ayırt edici soru: **geçişin yanında yapılması ZORUNLU bir iş var mı?** Varsa tek
kapı o işi garanti eder; yoksa tek kapı yalnız bir kat dolaylılıktır.

Karşı örnek aynı dosyada: `remainingSeconds` alanına da üç yerden yazılıyor (kurucu
değil ama `OnHealthDepleted` ve `Tick`'in iki dalı) ve onun için de bir
`SetRemaining` yok — aynı sebeple.

### İŞ BÖLÜMÜ: `private set` ile bu karar bölüşür

```
`{ get; private set; }`  ► DIŞARIDAN yazmayı imkânsız kılar
bu karar                 ► İÇERİDEN üçüncü bir yazanın sessizce
                           eklenmesini gözle görülür kılar
```

Erişim belirteci silinirse her çağıran durumu çevirir. Karar unutulursa bir sonraki
geliştirici event ya da geçiş kaydı eklerken `State`'e ikinci bir yerden yazar ve o
geçiş sessizce yayınsız kalır — hata "bazen event gelmiyor" diye çıkar. Yani
`private set` sınıf duvarını tutuyor, bu belge sınıfın İÇİNİ tutuyor; ikincisi kod
değil, sözdür.

---

## IsReadyForCleanup

Enkaz süresi dolduğunda `true` olur. Bu bir **İSTEKtir**, bir eylem değil: sahneden
silme işini Unity katmanı yapar. Burada `true` olması orada silindiği anlamına
gelmez — "karar" ile "uygulama" farklı sahiplerdir.

Aynı kararın kardeş tipteki uzun gerekçesi — temizlik için neden event değil bayrak
seçildiği — [UnitLifecycle.md](UnitLifecycle.md#isreadyforcleanup)'de.

---

## RemainingSeconds

Kalan enkaz süresi. Yapı ayaktayken anlamsızdır ve `0` döner. `State` ile
`remainingSeconds`'ı birleştirip anlamı veren üçüncü parça budur.

---

## OnHealthDepleted()

**CEVABI HESAPLAYABİLEN TEK YER ONU DÖNDÜRMELİDİR.**

Canı tükendiğinde çağrılır: ayakta olan yapı yıkılır ve enkaz sayacı başlar. Yapı
BU çağrıyla yıkıldıysa `true`; zaten yıkıksa `false`.

### HARİTA: "bu vuruş mu yıktı" cevabı nerede üretiliyor

```
  SEÇİLEN
    çağıran ─► OnHealthDepleted() ─► bool
                 ◄── >> CEVAP KAYNAĞINDA ÜRETİLİYOR <<
                 tek yer, tek satır, atlanacak adım yok

  REDDEDILEN — void
    çağıran şunu elle kurar:
      ① var before = lifecycle.State;      ◄── >> KIRILGAN ADIM <<
      ② lifecycle.OnHealthDepleted();
      ③ var after  = lifecycle.State;
      ④ bool destroyedNow = before != after;
    ve bu dört satır Structure'da, UI'da, skorda AYRI AYRI doğar

    ① unutulursa: enkaza değen her alan hasarı YENİ bir yıkım
    sayılır -> yıkım sesi tekrar çalar, skor tekrar artar
    derleyici: hiçbir şey der  ·  test: hiçbiri kırmızıya dönmez
```

### İkinci vuruş enkaz sayacını sıfırlamaz

Gövdedeki erken çıkış yalnız `false` döndürmez, aynı zamanda sayacı korur.
Sıfırlasaydı, yıkık bir binaya rastgele düşen alan hasarı enkazı sonsuza dek
ekranda tutardı — ve bu, hiçbir zaman ortaya çıkmayan türden bir hatadır: kimse
"enkaz neden hâlâ duruyor" diye bug açmaz.

### KAPSAM: "void metot yazma" DEĞİL

Ayırt edici soru: **bu çağrı, yalnız kendi içinde bilinebilen YENİ bir olgu
üretiyor mu?**

Karşı örnek aynı dosyada, birkaç satır aşağıda: `Tick` void'dir ve öyle
KALMALIDIR. `Tick`'in ürettiği tek yeni olgu "enkaz süresi doldu" ve o olgu zaten
`IsReadyForCleanup` diye okunabilir bir property; üstelik `Tick` her karede
çağrılıyor, dönen değer her karede atılırdı. Yani kural "her metot bir şey
döndürsün" değil: dışarıdan yeniden hesaplanması gereken bir olgu varsa döndür.

### İŞ BÖLÜMÜ: dönüş değeri ile property örtüşmez, bölüşür

```
bool dönüş           ► ANLIK olgu: "bu çağrı yıktı"
                       çağrı bitince kaybolur, saklanacak yeri yok
IsReadyForCleanup    ► KALICI olgu: "artık kaldırılabilirim"
                       her okunduğunda aynı cevabı verir
StructureState State ► KALICI durum: "şu an neyim"
```

Dönüş değeri silinirse anlık olgu kaybolur ve yukarıdaki dört satır her çağıranda
yeniden doğar. Property silinirse kalıcı olgu kaybolur ve `Tick`'in tek ürünü
ortadan kalkar. Aynı olguyu iki yerde taşımıyorlar — biri bir ANI, diğeri bir
DURUMU söylüyor.

### DÖNÜŞ DEĞERİ OKUNMAYI GARANTİ ETMEZ

Okuyucu korumayı imzaya yazabilir: "bool döndürüyor, çağıran görür". C#'ta bir
dönüş değerini yok saymak tamamen serbesttir; derleyici uyarı bile vermez ve bu
metotta bunu zorlayan bir öznitelik yok. Dönüş değerinin kazancı cevabın
KULLANILMASI değil, TEK YERDE ÜRETİLMESİ. İkincisini garanti ediyor, birincisini
etmiyor.

### REDDEDILEN

Dönüş değeri yok, metot void kalır ve çağıran durumu kendisi okur:

```csharp
public void OnHealthDepleted()
```

**KIRILAN:** "bu vuruş mu yıktı" cevabı her çağıranda üç satırla elle kurulur.

```
önce State'i oku, çağır, tekrar oku -> Structure'da, UI'da, skorda
birinde ilk okuma unutulur -> enkaza vurmak yeni yıkım sayılır
derleyici: hiçbir şey der  ·  test: hiçbiri kırmızıya dönmez
```

**KAZANIRDI:** geçişle ilgilenen taraf çağıran DEĞİLSE — o gün yukarıda reddedilen
event doğru cevaba dönerdi.

**TEK CUMLE:** Cevabı hesaplayabilen tek yer onu DÖNDÜRMELİDİR; yoksa aynı hesap
her çağıranda yeniden doğar.

---

## TryRepair()

**DURUMU ÇEVİREN YER, DAYANDIĞI OLGUYU GÖRMEK ZORUNDA.**

Bu tipte `TryRepair` yok. Bu satır bir eksiklik değil, bir karar.

### HARİTA: kim neyi görüyor

```
  ┌──────────────── Structure (bileşik) ────────────────┐
  │  health ────────► Health              (SAYI)        │
  │  lifecycle ─────► StructureLifecycle  (DURUM)       │
  │       ◄── >> İKİSİNİ AYNI ANDA GÖREN TEK YER <<     │
  └─────────────────────────────────────────────────────┘

  BU TİPİN gördüğü:   DURUM
  BU TİPİN görmediği: CAN   (Health'e giden ok YOK, olmamalı da)

  REDDEDILEN TryRepair burada dursaydı:
    State = Standing   ✓ yazılır
    Current            ✗ görülmez, sıfırda kalır
      -> sıfır canla AYAKTA duran bir bina
      -> değen ilk hasar onu anında tekrar yıkar
      -> hata "bina bazen hemen yıkılıyor" diye gelir
    ◄── >> GEÇİŞ, GÖREMEDİĞİ BİR DEĞİŞMEZE DAYANIYOR <<
```

### KAPSAM: bu tip geçiş yapamaz DEĞİL

Karşı örnek aynı dosyada, hemen yukarıda: `OnHealthDepleted` da bir DURUM geçişi
yapar ve o da canı görmez — ama görmesi gerekmiyor, çünkü dayandığı olguyu ("can
bitti") çağıran zaten doğrulayıp öyle çağırıyor: `Structure.TakeDamage` içindeki
`if (!health.HasRemaining)`.

Kural şu: **bir geçiş, dayandığı olguyu ya KENDİ görmeli ya da çağıran onu
doğrulamış olarak getirmelidir.** `TryRepair`'in getireceği bir olgu yoktu — "yıkık
olmak" tek başına "onarılabilir" demiyor.

### İŞ BÖLÜMÜ: onarım üç parçaya bölünmüş

```
Structure.TryRepair'deki `if (!IsStanding)`  ► KELEPÇE
    yıkık yapıyı ayağa kaldırmayı reddeder; canı ve durumu aynı
    anda gören tek yerde durduğu için doğru yer orası
Health.Heal                                  ► SAYIYI yazar
bu tip                                       ► DURUMU tutar,
    ve onarımı hiç GÖRMEZ
```

Kelepçe silinirse yıkık binaya can basılır ve sayı ile durum sessizce ayrışır.
`Health.Heal` silinirse onaracak bir şey kalmaz. Bu tipe bir `TryRepair` eklenirse
aynı kelepçe İKİ evde yaşar ve biri canı görür, diğeri görmez — ayrıştıkları gün
hangisinin doğru olduğu bilinemez.

### ONARIM ile DİRİLTME farkı, tek cümle

```
diriltme = DURUM geçişi (yıkık → ayakta)   -> bu tipin işi olurdu
onarım   = SAYI değişikliği (can artar)    -> Health'in işi
```

Bu tipte onarım hiç görünmez; görünmemesi bir eksiklik değil, yukarıda çizilen
sınırın ta kendisi.

### REDDEDILEN

```csharp
public bool TryRepair()
{
    if (State != StructureState.Destroyed) { return false; }
    State = StructureState.Standing;
    remainingSeconds = 0f;
    return true;
}
```

**KIRILAN:** bu tip canı GÖRMEZ; durumu "ayakta"ya çevirir, `Current` sıfırda
kalır.

```
sıfır canla ayakta bina -> değen ilk hasar onu anında tekrar yıkar
hata "bina bazen hemen yıkılıyor" diye gelir, sebebi burada aranmaz
derleyici: hiçbir şey der  ·  test: Repair_AfterDestruction_IsRejected
```

**KAZANIRDI:** tasarım "çöken bina enkaz penceresi dolmadan yerinde ayağa
kaldırılabilir" derse — pencere ve geri sayım zaten burada.

**TEK CUMLE:** Durumu değiştiren yer canı da görmek zorundadır; görmüyorsa o geçiş
onun değildir.

---

## Tick(float deltaSeconds)

Zamanı ilerletir. Saniye DIŞARIDAN gelir — bu tipin Unity'ye bağlanmamasının ve
EditMode'da sınanabilmesinin tek sebebi budur; ölçülmüş gerekçe
[UnitLifecycle.md](UnitLifecycle.md#tickfloat-deltaseconds)'de.

**Ayakta geri sayım yok;** oradaki erken çıkış PERFORMANS için değil, DOĞRULUK
için: sonraki çıkarma ayakta bir yapıda anlamsız bir alanı eksiltirdi.

**Sayaç sıfırda tutuluyor** ki sonraki `Tick`'ler onu eksiye götürmesin ve UI
negatif sayı göstermesin.
