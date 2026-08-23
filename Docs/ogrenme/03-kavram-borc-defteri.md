# Kavram borç defteri — kapsama tablosu

██ Bu dosya bir **kapsama denetimidir**: mentor yetkinlik evrenindeki her kavramı
bu projenin belgeleriyle karşılaştırır ve her satıra ya bir **sahip dosya** ya
bir **aşama** yazar. ██

Boş hücre yoktur. Sahipsiz kavram yoktur. Bir kavramın karşılığı yoksa cevap
uydurulmaz — `HENÜZ YOK` yazılır ve onu yaratacak aşama adıyla anılır.

Kavram listesi **uydurulmadı**; iki kaynaktan çıkarıldı:
`unity-game-dev-mentor` skill'inin `references/competency-universe.archive` ve
`references/architecture-patterns.archive` dosyaları.

## Kavramlar nereden geldi

`competency-universe.archive` on alan tanımlıyor. Aşağıdaki tablo, o on alanın
bu deftere hangi bölüm olarak düştüğünü gösteriyor — ve hangilerinin bu projede
**hiç** temsil edilmediğini:

| Yetkinlik evrenindeki alan | Bu defterdeki bölüm | Not |
|---|---|---|
| 1 · C# dili ve çalışma zamanı | A | En dolu bölüm; `Docs/deep/dil/` ağacı bu alanı sahipleniyor |
| 2 · Unity Editor ve motor modeli | D | Çağrı döngüsü ve Domain Reload kapandı; `.meta`/GUID ve prefab varyantı hâlâ sahipsiz |
| 3 · Nesne yönelimi ve mimari | B, C | SOLID beş harf ayrı satır; desenler ayrı satır |
| 4 · Algoritma, veri yapısı, matematik | F | Izgara ve mesafe kapalı; yol bulma ve rastgelelik yok |
| 5 · Oyun ve ürün sistemleri | ██ TEMSİL EDİLMİYOR ██ | Ekonomi, ilerleme, kayıt/yükleme, yerelleştirme, erişilebilirlik, öğretici — hiçbiri yok. Bu proje bir savaş çekirdeği, bir oyun değil |
| 6 · Test, hata ayıklama, güvenilirlik | E | En sağlam bölüm; 26 test dosyası ve dört makine kapısı |
| 7 · Performans ve hedef platform | E (kısmen) | Yalnız tahsis ölçümü var; kare bütçesi, Profiler ve cihaz kanıtı yok |
| 8 · Görsel, ses ve varlık hattı | ██ TEMSİL EDİLMİYOR ██ | Materyal, gölgelendirici, ışık, animasyon, ses, sıkıştırma — hiçbiri yok. Ekranda yalnız `SpriteRenderer` var |
| 9 · Araç, derleme ve teslim | D, G | `.asmdef` kapalı; platform derlemesi, sürümleme, çökme raporu yok |
| 10 · Takım ve profesyonel pratik | G | Yorum sözleşmesi ve makine kapıları kapalı; Git akışı ve inceleme yok |

██ İki alanın hiç temsil edilmemesi bir **kusur değil, bir kapsam ölçüsüdür**. ██
Bu depo bir oyun değil, bir savaş çekirdeği ve onun etrafındaki gerekçe ağacı.
Alan 5 ve 8 için satır açmak, sahipsiz otuz satır daha üretir ve defteri
okunmaz kılardı. Bu satır, o kararın kendisidir — ve alan 5 ya da 8'e ait ilk
gerçek özellik doğduğu gün bu tabloya iki yeni bölüm eklenir.

## Sütunlar ve durum sözlüğü

| Sütun | Ne taşır |
|---|---|
| **KAVRAM** | Yetkinlik evrenindeki adı. Boş olamaz |
| **SAHİP DOSYA** | `KAPALI`/`KISMİ` için `yol` ya da `yol:satır`. `HENÜZ YOK` için `AŞAMA:` ile başlayan bir aşama adı. Boş olamaz |
| **DURUM** | Tam olarak `KAPALI`, `KISMİ` ya da `HENÜZ YOK`. Boş olamaz |
| **KANIT** | Neyin nerede yazılı olduğu, ya da eksiğin adı. Boş olamaz |

```
KAPALI     bir sahip belge var VE kavram orada tam anlatılmış
KISMİ      bir parçası var, eksik olan parça ADIYLA yazılı
HENÜZ YOK  hiçbir belge sahiplenmiyor; onu yaratacak aşama yazılı
```

██ Bu defterin **tek ölümcül hatası** ██, var olmayan bir dosyaya `KAPALI`
yazmaktır. Öğreneni tam bir güvenle boşluğa gönderir ve hiçbir yerde kırmızı
görünmez. Kapı tam olarak bunu arıyor:

```
python Tools/check-curriculum-coverage.py
```

**Bu defterin dışında kalan bir kayıt var:** öğrenenin kendi ifadesiyle kapattığı
kavramlar bu depoda değil, `unity-game-dev-journey` çalışma ağacındaki
`parallel_sessions/S06_COMBAT_CORE/LEARNED_CONCEPTS.md` dosyasında yaşıyor
(bugün K-09 … K-15 arası yedi kavram). O dosya **öğrenenin** kaydı; bu defter
**projenin** kaydı. İkisi ayrı sorulara cevap veriyor ve kapı yalnızca ikincisini
denetliyor — depo dışına uzanan bir yol, taşındığı gün sessizce kırılırdı.

---

## A · C# dili ve çalışma zamanı

| KAVRAM | SAHİP DOSYA | DURUM | KANIT |
|---|---|---|---|
| Değer ve referans tipleri | `Docs/deep/dil/05-deger-referans-ve-kimlik.md:68` | KAPALI | "Birinci durak: kopyalanan mı, paylaşılan mı"; motor tarafı karşı örneği `Vector3` ile :103 |
| Kimlik ve eşitlik — `ReferenceEquals` vs `==` | `Docs/deep/dil/05-deger-referans-ve-kimlik.md:132` | KAPALI | Projede yakalanan canlı hata :134; "yerine geçebilirlik" ölçüsü :162 |
| `enum` gerçekte nedir | `Docs/deep/dil/05-deger-referans-ve-kimlik.md:247` | KAPALI | Adlandırılmamış değerlerin de geçerli olduğu :274 |
| `out` parametre ve `bool + out` şekli | `Docs/deep/dil/05-deger-referans-ve-kimlik.md:292` | KAPALI | Neden nullable dönüş değil :311 |
| `=>` iki ayrı iş — üye gövdesi ve lambda | `Docs/deep/dil/05-deger-referans-ve-kimlik.md:351` | KAPALI | Her okumada yeniden hesaplanan üye :371; delegenin kimliği :402 |
| `switch` deyimi eksik dal için uyarmaz | `Docs/deep/dil/05-deger-referans-ve-kimlik.md:448` | KAPALI | El yazımı `default` dalının boşluğu kapatması :474 |
| `%` dairesel sayaç ve matematiksel mod farkı | `Docs/deep/dil/05-deger-referans-ve-kimlik.md:518` | KAPALI | Tuzak bölümü :551 |
| Değişmezlik anahtar kelimeleri | `Docs/deep/dil/01-degismezlik-anahtar-kelimeleri.md:114` | KAPALI | `readonly` · `const` · `static readonly` · `{ get; }` · `sealed` beşi tek dosyada |
| `const` assembly sınırında kopyalanır | `Docs/deep/dil/01-degismezlik-anahtar-kelimeleri.md:293` | KAPALI | Projede ölçülmüş sonuç: bir test IL'de `Assert.That(1, Is.EqualTo(1))`'e düşüyor |
| `sealed` — tip ağacını keser, nesne grafiğini kesmez | `Docs/deep/dil/01-degismezlik-anahtar-kelimeleri.md:428` | KAPALI | Neyin vaat edilmediği :500 |
| Koleksiyonlar ve "salt okunur"un kapsamı | `Docs/deep/dil/02-koleksiyonlar-ve-salt-okunur.md:55` | KAPALI | `IReadOnlyList` ≠ değişmez; projede neden güvenli :82 |
| `IEnumerator` — iki hayat | `Docs/deep/konular/08-motor-cagri-dongusu.md:559` | KAPALI | "Beşinci durak: `IEnumerator`'un İKİ AYRI HAYATI". Numaralandırıcı yarısı ayrıca `Docs/deep/dil/02-koleksiyonlar-ve-salt-okunur.md:164` |
| Kutulama (boxing) ve tahsis maliyeti | `Docs/deep/dil/07-bellek-canlilik-ve-yikim.md:490` | KAPALI | "Beşinci durak: bu projenin tahsis gerçeği — ÖLÇÜLMÜŞ". `Dictionary`/`KeyValuePair` tarafı `Docs/deep/dil/02-koleksiyonlar-ve-salt-okunur.md:216`; uygulaması `Assets/Game/Battle/Battle.cs:368` |
| Delege, `event` ve `?.Invoke` | `Docs/deep/dil/04-delege-olay-ve-kapanis.md:74` | KAPALI | Düz `Action` alanı ile `event` farkı :152; abonesiz olayın `null` olması :221. Derleyicinin ürettiği arka taraf `Docs/deep/dil/06-delege-arka-taraf.md:181` |
| Kapanış kimliği ve `-=` neye bakar | `Docs/deep/dil/04-delege-olay-ve-kapanis.md:274` | KAPALI | İki delegenin ne zaman eşit olduğu `Docs/deep/dil/04-delege-olay-ve-kapanis.md:292`; uygulaması `Assets/Game/Battle/Battle.cs:74` |
| Hata bildirme: dört istisna tipi ve `nameof` | `Docs/deep/dil/03-hata-bildirme-ve-dogrulama.md:108` | KAPALI | Seçim ölçüsü "orta argümanı doldurabiliyor musun" :261; argüman sırası tuzağı :321 |
| `Math.Max` / `Math.Min` kelepçesi | `Docs/deep/dil/03-hata-bildirme-ve-dogrulama.md:403` | KAPALI | Neden `if` değil :415; her `Math.Max`'in kelepçe olmadığı karşı örneği :458 |
| İstisna mı sonuç değeri mi | `Docs/deep/konular/06-sonuc-enumlari.md:82` | KAPALI | Sıfırıncı hücre kararı; ret değerlerini birleştirmenin bedeli :466 |
| Bellek, canlılık ve nesne yıkımı | `Docs/deep/dil/07-bellek-canlilik-ve-yikim.md:93` | KAPALI | "Birinci durak: dört ayrı soru, dört ayrı cevap"; Unity'nin iki ömrü :389. Koddaki işaretçi `Assets/Game/Unity/BoardAdapter.cs:988` — "null gibi ama null değil" hâli |
| Generic tipler ve kısıtlar | `AŞAMA: sahipsiz — ilk generic tipin yazıldığı gün yeni bir Docs/deep/dil/ dosyası` | HENÜZ YOK | Üretim kodunda tek generic kullanımı hazır BCL tipleri (`Dictionary`, `List`, `Action`); kendi yazdığımız generic tip yok |
| Nullable referans tipleri | `AŞAMA: sahipsiz — proje nullable bağlamını açtığı gün yeni bir Docs/deep/dil/ dosyası` | HENÜZ YOK | Bugün `null` denetimi elle yapılıyor (`Assets/Game/Core/Combat/Combatant.cs:70`); derleyici desteği kapalı |
| LINQ ve kapanış maliyeti | `AŞAMA: sahipsiz — ilk LINQ ifadesinin sıcak yola girdiği gün` | HENÜZ YOK | Üretim kodunda `System.Linq` hiç kullanılmıyor; döngüler elle yazılı (`Assets/Game/Battle/Battle.cs:427`) |
| `async` / `Task` / `Awaitable` — coroutine karşılaştırması | `AŞAMA: sahipsiz — ilk eşzamansız işin (yükleme, ağ) doğduğu gün` | HENÜZ YOK | `Assets/Game/` altında `async`, `Task`, `Awaitable`, `IEnumerator`, `yield` kelimelerinin hiçbiri geçmiyor; karşılaştırılacak taraf henüz yok |

---

## B · SOLID — beş harf ayrı ayrı

Beş harfin ne olduğu tek yerde tanımlı:
[`Docs/ogrenme/01-koda-gomulu-desenler.md:20`](01-koda-gomulu-desenler.md).
Aşağıdaki satırlar
her harfin bu projedeki **uygulanmış** karşılığını gösteriyor.

| KAVRAM | SAHİP DOSYA | DURUM | KANIT |
|---|---|---|---|
| S — tek sorumluluk | `Docs/ogrenme/01-koda-gomulu-desenler.md:31` | KAPALI | 1. desen: dokuz saf kural sınıfı; ihlalin faturası `Assets/Game/Core/Combat/TargetingRules.cs:38` |
| O — açık/kapalı | `Docs/ogrenme/01-koda-gomulu-desenler.md:513` | KAPALI | 7. desen: yeni enum değerinin sona eklenmesi; beyaz liste biçimi `Assets/Game/Core/Combat/MovementRules.cs:36` |
| L — Liskov yerine geçme | `Docs/ogrenme/01-koda-gomulu-desenler.md:364` | KAPALI | 5. desen: `Structure`'ın `: Combatant` yazmayı reddetmesi `Assets/Game/Core/Combat/Structure.cs:17` |
| I — arayüz ayrımı | `Docs/ogrenme/01-koda-gomulu-desenler.md:665` | KAPALI | 9. desen: `Unit` tek üye taşıyor; `UnitGrid` tuttuğu şeyin ne olduğunu bilmiyor |
| D — bağımlılık tersine çevirme | `Docs/ogrenme/01-koda-gomulu-desenler.md:274` | KAPALI | 4. desen: arayüzle değil `.asmdef` yönüyle; `GridStrategy.Battle`, `GridStrategy.Unity`'yi göremiyor |

---

## C · Desenler ve mimari kararlar

| KAVRAM | SAHİP DOSYA | DURUM | KANIT |
|---|---|---|---|
| Saf kural sınıfı (stateless policy) | `Docs/ogrenme/01-koda-gomulu-desenler.md:31` | KAPALI | Dokuz tip, hepsi `static class`, hiçbirinde alan yok |
| Akış sahibi (transaction script) | `Docs/ogrenme/01-koda-gomulu-desenler.md:102` | KAPALI | `MoveAction`, `AttackAction`, `BattleActions`; ret sırası bir karar olarak yazılı |
| Durum makinesi — enum tabanlı | `Docs/ogrenme/01-koda-gomulu-desenler.md:190` | KAPALI | Üç makine, yasak geçişler, beyaz liste biçimi |
| Katman sınırı çevirmeni | `Docs/ogrenme/01-koda-gomulu-desenler.md:274` | KAPALI | Duvar dört `.asmdef`'te, kapı `Assets/Game/Unity/BoardAdapter.cs:106` |
| Bileşim ve kalıtımın reddi | `Docs/ogrenme/01-koda-gomulu-desenler.md:364` | KAPALI | 33 üretim dosyasında `abstract`/`virtual`/`override` hiç geçmiyor |
| Sonuç değeri kanalı (result enum) | `Docs/ogrenme/01-koda-gomulu-desenler.md:513` | KAPALI | Dört enum; sıfırıncı değer kararı dördünde de aynı |
| Gözlemci (Observer) | `Docs/ogrenme/01-koda-gomulu-desenler.md:580` | KAPALI | Dört duraklı zincir; hikâyesi `Docs/deep/konular/01-olay-zinciri.md:63` |
| Kimlik + yan tablo | `Docs/ogrenme/01-koda-gomulu-desenler.md:665` | KAPALI | `Unit` + üç sözlük; anahtarı ayakta tutan şey `Equals`'in YOKLUĞU |
| Flyweight — paylaşılan değişmez tanım | `Docs/ogrenme/01-koda-gomulu-desenler.md:439` | KISMİ | İç durum yarısı var (`AttackProfile`, `MoveProfile` değişmez). EKSİK: paylaşımı yöneten havuz/fabrika; bugün her birim kendi profilini alıyor `Assets/Game/Unity/BoardAdapter.cs:748` |
| Command | `Docs/ogrenme/01-koda-gomulu-desenler.md:102` | KISMİ | Neden bu projede olmadığı ve neyin yanlış hatırlandığı yazılı. EKSİK: tetikleyici koşulu — hamle geçmişi, tekrar izleme ya da geri alma özelliği doğmadı |
| GoF State — hâl başına sınıf | `Docs/ogrenme/01-koda-gomulu-desenler.md:190` | KISMİ | Enum tabanlı makine kapalı; GoF biçimiyle farkı yazılı. EKSİK: hâl başına onlarca satır davranış biriktiğinde geçiş eşiği ölçülmedi |
| GoF Adapter — arayüz dönüştürücü | `Docs/ogrenme/01-koda-gomulu-desenler.md:274` | KISMİ | Katman çevirmeni ile farkı yazılı. EKSİK: gerçek bir arayüz dönüştürme örneği yok, çünkü projede `interface` hiç yok |
| Strategy | `Docs/ogrenme/01-koda-gomulu-desenler.md:743` | KISMİ | Elenen adaylar tablosunda yokluğun ölçüsü yazılı. EKSİK: ikinci bir algoritma (mesafe ölçüsü, hedef seçim politikası) doğmadığı için karşılaştırma yapılamıyor |
| Factory | `Docs/ogrenme/01-koda-gomulu-desenler.md:743` | KISMİ | `NewCombatant` ve `NewStructure` birer private yardımcı, üretim politikası taşımıyorlar. EKSİK: tip seçimi gerektiren bir üretim noktası yok |
| Decorator | `Docs/ogrenme/01-koda-gomulu-desenler.md:743` | KISMİ | Sarmalanacak sabit bir sözleşme olmadığı yazılı. EKSİK: bağımsız birleşen davranış (hasar değiştirici, yetenek etkisi) henüz yok |
| Service Locator | `Docs/ogrenme/01-koda-gomulu-desenler.md:743` | KISMİ | Kayıt defteri olmadığı yazılı. EKSİK: neden varsayılan mekanizma olarak reddedildiğinin uzun gerekçesi bu ağaçta değil |
| MVP / MVC ailesi | `Docs/ogrenme/01-koda-gomulu-desenler.md:743` | KISMİ | `UnitView` edilgen bir görünüm, karşısında sunucu tipi yok. EKSİK: gerçek bir menü ya da HUD ekranı doğmadığı için ayrım sınanamıyor |
| Singleton — ve reddedilişi | `Docs/ogrenme/02-sonraki-asamalar.md:290` | KAPALI | Üç bağımsız kararın ayrımı, tetikleyici, ne kırar, ön koşul; ölçü: değiştirilebilir `static` alan yok |
| Olay veri yolu (event bus) | `Docs/ogrenme/02-sonraki-asamalar.md:210` | KAPALI | Bugünkü dört duraklı zincirin ne kaybedeceği yazılı |
| Nesne havuzu (object pool) | `Docs/ogrenme/02-sonraki-asamalar.md:117` | KAPALI | Bugün kare başına sıfır doğum ölçüsü; sıfırlama sözleşmesinin `Awake`'te yaşadığı tespiti |
| ScriptableObject sınırı | `Docs/ogrenme/02-sonraki-asamalar.md:26` | KAPALI | En sık hata (varlığın çalışma zamanı durumu taşıması) adıyla yazılı |
| ECS / DOTS | `Docs/ogrenme/02-sonraki-asamalar.md:379` | KAPALI | Dört farkın tablosu; sıra tabanlı oyunda eşiğin neden çok yüksek olduğu ölçüyle |
| Clean Architecture — bağımlılık yönü | `Docs/deep/konular/02-assembly-duvari.md:131` | KAPALI | Duvarın engellediği ve engellemediği şeyler ayrı ayrı; dört somut fatura :452 |
| Kompozisyon kökü (composition root) | `Assets/Game/Unity/BoardAdapter.cs:225` | KISMİ | `Awake` bugün fiilen kompozisyon kökü: savaşı, jesti, zemini ve iki demo birimi orada kuruyor. EKSİK: bu rolün adı kodda geçmiyor ve ikinci bir kök doğduğunda ne olacağı yazılı değil |
| Veriye dayalı tasarım (data-driven) | `Docs/deep/kod/Unity/BoardAdapter.md:269` | KISMİ | "Bir sayı üç dosyadan birinde yaşayabilir" haritası kapalı. EKSİK: tasarımcı tarafı, yani kod derlemeden değişebilen veri; bkz. Aşama 1 |

---

## D · Unity motor modeli

| KAVRAM | SAHİP DOSYA | DURUM | KANIT |
|---|---|---|---|
| Assembly sınırı (`.asmdef`) | `Docs/deep/konular/02-assembly-duvari.md:83` | KAPALI | Klasör ≠ ad alanı ≠ assembly; CS0118'in kendisi ve alias çözümü :316 |
| Ad çözümleme ve `using` alias | `Docs/deep/kod/Unity/BoardAdapter.md:78` | KAPALI | Arama seviyeleri haritası; alias'ın yerinin kuralın kendisi olması |
| Serileştirme — `[SerializeField]` ve alan tipi | `Docs/deep/kod/Unity/BoardAdapter.md:258` | KISMİ | Sayının hangi dosyada yaşayacağı kararı ve üç attribute'ün üç ayrı işi :308 kapalı. EKSİK: Unity hangi tipleri serileştirir, `[SerializeReference]`, özel çizici; `.meta` GUID tarafı ayrı satırda |
| `.meta` dosyası ve GUID sahipliği | `AŞAMA: sahipsiz — ilk prefab/asset taşıma işi yapıldığı gün` | HENÜZ YOK | Bugün tek değinme `Docs/deep/README.md:44` — `Assets/` altındaki bir `.md`'nin `.meta` üretmesi; kavram olarak anlatılmıyor |
| Unity mesaj geri çağrıları (`Awake`/`OnEnable`/`Update`) | `Docs/deep/konular/08-motor-cagri-dongusu.md:125` | KAPALI | "Birinci durak: `Awake` bir `event` DEĞİLDİR"; çağrı sırası sahipleriyle :248. Koddaki karar `Assets/Game/Unity/BoardAdapter.cs:269` |
| Tembel `GetComponent` ve `Awake`'in EditMode'da çalışmaması | `Assets/Game/Unity/UnitView.cs:106` | KAPALI | Ölçülmüş sebep: `Awake` EditMode'da hiç çalışmaz, orada kurulan referans tipi sahnesiz sınanamaz kılardı |
| Domain Reload ve sahne yeniden yükleme | `Docs/deep/konular/08-motor-cagri-dongusu.md:718` | KAPALI | "Altıncı durak: Domain Reload — sessiz kanıt kirleticisi". `static` durumun Domain Reload kapalıyken hayatta kalması ayrıca Aşama 4'te |
| Zamanın dışarıdan verilmesi (`Time.deltaTime` sınırı) | `Assets/Game/Core/Combat/UnitLifecycle.cs:159` | KAPALI | Ölçülmüş: EditMode'da `Time.deltaTime` sıfır değil 0,017675 döner; zamanı içeriden okuyan tasarım sessizce anlamsız yürür |
| Girdi okuma — üç fare sorgusu | `Docs/deep/konular/07-tiklamadan-eyleme.md:178` | KAPALI | Motorun üç sorusu; üçlünün yalnız bir akışta gerektiği ölçüsü `Assets/Game/Unity/BoardAdapter.cs:46` |
| Koordinat çevirisi — motor `Grid` bileşeni | `Docs/deep/konular/07-tiklamadan-eyleme.md:133` | KAPALI | Tek çeviri iki çağıran; motor `Grid`'inin yalnız koordinat çevirmeni olduğu `Assets/Game/Unity/BoardAdapter.cs:176` |
| Prefab, varyant ve `Instantiate` sözleşmesi | `AŞAMA: sahipsiz — ikinci prefab türü ya da ilk varyant doğduğu gün` | HENÜZ YOK | Bugün tek prefab var (`Assets/Game/Unity/BoardAdapter.cs:120`) ve tek `Instantiate` çağrısı :728; karşılaştırılacak ikinci durum yok |
| `MonoBehaviour` ile düz C# sınıfı arasındaki sınır | `Docs/deep/konular/02-assembly-duvari.md:408` | KAPALI | Duvarın ürünü olarak `Battle`'ın var olmak zorunda kalması; `new` ile kurulamayan tiplerin listesi `Assets/Game/Unity/BoardAdapter.cs:72` |

---

## E · Test, kanıt ve ölçüm

| KAVRAM | SAHİP DOSYA | DURUM | KANIT |
|---|---|---|---|
| Kanıt seviyeleri — EditMode / PlayMode / cihaz | `Docs/ogrenme/02-sonraki-asamalar.md:475` | KAPALI | Üç kova ve her birinin neyi kanıtladığı; Editor sayısının hedef cihazı kanıtlamadığı |
| EditMode davranış testi | `Assets/Tests/EditMode/Combat/UnitLifecycleTests.cs` | KAPALI | 26 test dosyasının çoğu düz `new` ile nesne kurup davranış sınıyor; sahne gerekmiyor |
| Tahsis (allocation) testi | `Assets/Tests/EditMode/Combat/DamageRulesAllocationTests.cs:36` | KAPALI | Ölçülen şey süre değil tahsis; 0 byte her makinede tam olarak 0 byte |
| Negatif kontrol — ölçüm aygıtını sınamak | `Assets/Tests/EditMode/Combat/DamageRulesAllocationTests.cs:69` | KAPALI | Araç körse diğer testler kod bozulsa bile yeşil kalır; `GC.GetAllocatedBytesForCurrentThread` bu sürümde her zaman 0 döndürüyor |
| Ölçüm penceresi — lambda'nın içi ve dışı | `Assets/Tests/EditMode/Combat/DamageRulesAllocationTests.cs:108` | KAPALI | Döngü lambda'nın dışında kalırsa kaydedici boş aralık ölçer ve yeşil verir; bu bir kez gerçekten yaşandı |
| Komut satırından test koşumu | `Tools/run-editmode-tests.ps1:1` | KAPALI | Editor'e dokunmadan koşum; filtre söz diziminin regex olduğu ölçülmüş |
| Profil çıkarma araçları (Profiler, Frame Debugger, Memory Profiler) | `AŞAMA: Docs/ogrenme/02-sonraki-asamalar.md · Aşama 6 genişlemesi — ilk kare bütçesi sorusu doğduğu gün` | HENÜZ YOK | Bugün tek ölçüm aracı Unity Test Framework kısıtı; Profiler penceresi hiç kullanılmadı |
| Hedef cihaz üstünde ölçüm | `AŞAMA: Docs/ogrenme/02-sonraki-asamalar.md · Aşama 6 genişlemesi — ilk cihaz derlemesi alındığı gün` | HENÜZ YOK | Bugün proje hiç cihaza derlenmedi; Mono ile IL2CPP farkı sınanmadı |
| Makine kapısı deseni | `Tools/check-doc-links.py:1` | KAPALI | Kapının kendi öz-sınaması :101; ilk sürümünde 50 sahte pozitif üretmesinin kaydı :65 |

---

## F · Algoritma, veri yapısı ve oyun kuralı

| KAVRAM | SAHİP DOSYA | DURUM | KANIT |
|---|---|---|---|
| Izgara mesafesi — Chebyshev kararı | `Assets/Game/Core/GridDistance.cs:25` | KAPALI | Mesafenin tek sahibi; çapraz komşu Chebyshev'de 1, Manhattan'da 2 — `Assets/Game/Battle/BattleActions.cs:115` |
| Sözlük ve anahtar seçimi | `Assets/Game/Battle/Battle.cs:56` | KAPALI | Anahtar nesnenin kendisi, hücre değil: hücreyle anahtarlansaydı her hareket anahtarı bozardı |
| İki boyutlu dizi ve sınır sahipliği | `Assets/Game/Core/UnitGrid.cs:26` | KAPALI | Ölçünün tek sahibi dizinin kendisi :49; sınır sorgusu tahtada kalıyor :65 |
| Tarama maliyeti ve karmaşıklık | `Assets/Game/Battle/Battle.cs:517` | KISMİ | `TryGetPosition` tahtayı `Width × Height` tarıyor ve maliyet gerekçede adı konmuş. EKSİK: karmaşıklık gösterimi ve ölçüm; bugün tahta 15 hücre |
| Belirlenimci kural ve saf fonksiyon | `Assets/Game/Core/Combat/DamageRules.cs:19` | KAPALI | Formülün girdi uzayı sahibininkinden geniş; negatif yollar da sınanabiliyor |
| Yol bulma (A\*) ve engel maliyeti | `AŞAMA: sahipsiz — birim adım adım yürümeye başladığı gün` | HENÜZ YOK | Bugün hareket ışınlanma: `Assets/Game/Core/MoveAction.cs:27` yolun üzerinde ne olduğunu bilmediğini açıkça yazıyor |
| Rastgelelik ve tohum (seed) | `AŞAMA: sahipsiz — ilk rastgele karar doğduğu gün` | HENÜZ YOK | Üretim kodunda `Random` hiç geçmiyor; zemin deseni bile sabit bir formül `Assets/Game/Unity/BoardAdapter.cs:691` |

---

## G · Süreç ve profesyonel pratik

| KAVRAM | SAHİP DOSYA | DURUM | KANIT |
|---|---|---|---|
| Yorum sözleşmesi ve reddedilen alternatif biçimi | `Tools/check-comment-contract.py:1` | KAPALI | `KIRILAN` uzunluğu, numaralı liste yasağı ve `TEK CUMLE` zorunluluğu makineyle denetleniyor |
| Yorum dili — tam aksanlı Türkçe | `Tools/check-comment-language.py:1` | KAPALI | Ölçülmüş tuzak: `grep -i` 385 sahte pozitif üretti, sebebi Türkçe noktasız i |
| Anılan adın gerçekten var olması | `Tools/check-cited-names.py:1` | KAPALI | Var olmayan bir teste atıf, bayat bir satır numarası kadar kötüdür |
| Belge ağacının kendi kapsaması | `Tools/check-curriculum-coverage.py:1` | KAPALI | Bu defterin kapısı; öz-sınaması ve üç ihlal türü |
| Git dal, birleştirme ve kod incelemesi | `AŞAMA: sahipsiz — ikinci bir katkıcı ya da ilk paylaşılan dal doğduğu gün` | HENÜZ YOK | Bugün tek çalışan var; inceleme yerine makine kapıları kullanılıyor |
| Portföy ve mülakat savunması | `AŞAMA: Docs/ogrenme/01-koda-gomulu-desenler.md genişlemesi — ilk mülakat provası yapıldığı gün` | HENÜZ YOK | Desen adları artık yazılı, ama "bu kararı neden verdin" sorusunun sözlü provası yapılmadı |

---

## Sayım

| Bölüm | Satır | KAPALI | KISMİ | HENÜZ YOK |
|---|---|---|---|---|
| A · C# dili ve çalışma zamanı | 23 | 19 | 0 | 4 |
| B · SOLID beş harf | 5 | 5 | 0 | 0 |
| C · Desenler ve mimari kararlar | 25 | 14 | 11 | 0 |
| D · Unity motor modeli | 12 | 9 | 1 | 2 |
| E · Test, kanıt ve ölçüm | 9 | 7 | 0 | 2 |
| F · Algoritma ve veri | 7 | 4 | 1 | 2 |
| G · Süreç | 6 | 4 | 0 | 2 |
| **Toplam** | **87** | **62** | **13** | **12** |

██ Bu sayılar bir tur içinde **değişti** ve bu, defterin çalıştığının kanıtı. ██
İlk yazımda A bölümünde iki `KISMİ` ve beş `HENÜZ YOK` vardı; aynı oturumda
başka aşamalar `Docs/deep/dil/06-delege-arka-taraf.md`,
`Docs/deep/dil/07-bellek-canlilik-ve-yikim.md` ve
`Docs/deep/konular/08-motor-cagri-dongusu.md` dosyalarını yazdı ve beş satır
terfi etti — bellek/canlılık, kutulama, `IEnumerator`'un iki hayatı, Unity
mesaj geri çağrıları ve Domain Reload.

Bu sayıların doğruluğu elle korunmuyor: kapı her satırı ayrıştırıp kendi
sayısını basıyor. İki sayı ayrışırsa **kapının** sayısı doğrudur, bu tablo
bayattır.

## Bir satır nasıl okunur

Örnek, A bölümünden:

```
| İstisna mı sonuç değeri mi | Docs/deep/konular/06-sonuc-enumlari.md:82 | KAPALI | Sıfırıncı hücre kararı; ... |
  ─────────────┬────────────   ──────────────┬─────────────────────────   ───┬───   ──────────┬──────────
       KAVRAM  │                 SAHİP DOSYA │                       DURUM │              KANIT │
               │                             │                             │                    │
   yetkinlik   │        açılıp o satıra      │      üç değerden biri       │   neyin nerede
   evrenindeki │        bakılabilecek yer    │      (başka değer yok)      │   yazılı olduğu
   adı         │                             │                             │
```

Okuma sırası: **DURUM önce**. `KAPALI` görürsen dosyayı aç ve oku. `KISMİ`
görürsen dosyayı aç, ama KANIT hücresindeki "EKSİK:" kelimesinden sonrasını da
oku — orada ne öğrenilmediği yazılı. `HENÜZ YOK` görürsen dosya arama; SAHİP
DOSYA hücresi bir dosya değil, bir **aşama** taşır.

`KISMİ` satırlarının hepsinde "EKSİK:" kelimesi geçer ve bu tesadüf değil: bir
parçanın eksik olduğunu söylemek, eksik olan parçanın **adını** söylemeden
öğretici değildir.

## Bu defter nasıl güncellenir

Üç işlem var ve üçünün de kuralı farklı:

**① Yeni bir kavram eklemek.** Kavram, iki `.archive` dosyasından birinde
geçmelidir; oradan gelmiyorsa önce oraya eklenmesi gerekir. Satır dört hücreyle
birden yazılır — boş hücreyle bir satır açmak, kapının reddedeceği tek şeydir.

**② `HENÜZ YOK` → `KISMİ` ya da `KAPALI`.** Yeni bir sahip belge yazıldığında
DURUM değişir **ve** SAHİP DOSYA hücresindeki `AŞAMA:` metni gerçek bir
`yol:satır` ile değiştirilir. İkisinden birini unutmak kapıyı kırar: `KAPALI`
satırında `AŞAMA:` görürse kapı "dosya bekleniyordu, aşama yazılmış" der.

**③ `KISMİ` → `KAPALI`.** Yalnız KANIT hücresindeki "EKSİK:" cümlesi
karşılandığında. Eksiğin adı yazılı olduğu için bu terfi tartışmaya açık
değildir: ya o cümle karşılanmıştır ya karşılanmamıştır.

██ Satır numaraları kayar. ██ Sahip belge büyüdüğünde eski numara başka bir
satırı gösterir ve kapı bunu **yakalamaz** — yalnız dosyanın toplam satır
sayısını aşan numarayı yakalar. Bu, kapının bilinen ve kabul edilmiş sınırı:
"var olmayan dosya" ile "imkânsız satır" makineyle, "kaymış satır" gözle
denetlenir.

## Kapının canlı çıktısı

Temiz koşum:

```
$ python Tools/check-curriculum-coverage.py

kavram satiri: 87  (KAPALI 62 . KISMI 13 . HENUZ YOK 12)
ihlal: 0
```

Bilerek bozulmuş üç satırla koşum — kapının üç ihlal türünü de yakaladığı:

```
negatif-defter.md:8
    SAHIP DOSYA YOK: Docs/deep/dil/99-hic-yazilmadi.md
negatif-defter.md:9
    SATIR NUMARASI DOSYAYI ASIYOR: Docs/deep/dil/README.md:9999 (dosya 64 satir)
negatif-defter.md:10
    HENUZ YOK satirinda asama yazili degil: `bir gün birileri yazar`

kavram satiri: 5  (KAPALI 2 . KISMI 1 . HENUZ YOK 2)
ihlal: 3
```

Kapının kendi ayrıştırıcısı bozulursa çıktı hiç bu şekle gelmez:

```
KAPI BOZUK: bilinen-kotu ornek YAKALANMADI -- kapi her zaman temiz diyor
```

Bu satır bir olasılık değil, **yaşanmış** bir olayın karşılığı: kapının ilk
sürümü ters tırnakları soymadığı için `AŞAMA:` ön ekini hiç göremiyordu ve
öz-sınama onu ilk koşumda yakaladı. Gerekçe
`Tools/check-curriculum-coverage.py` içinde, `bare` değişkeninin üstünde yazılı.

## Bu defterin sınırı

`KAPALI` demek "öğrenildi" demek **değildir**; "bu projede bir sahip belge var
ve o belge kavramı tam anlatıyor" demektir. Öğrenmenin kendi kaydı bu depoda
değil — yukarıda adı geçen `LEARNED_CONCEPTS.md`'de, öğrenenin kendi
ifadesiyle.

## İlgili

- Bu ağacın yönlendirmesi: [README.md](README.md)
- Kodda zaten duran desenler: [01-koda-gomulu-desenler.md](01-koda-gomulu-desenler.md)
- Eksiklerin tetikleyici koşulları: [02-sonraki-asamalar.md](02-sonraki-asamalar.md)
- Ödünç alınan dil özellikleri: [../deep/dil/README.md](../deep/dil/README.md)
- Tip başına gerekçeler: [../deep/kod/README.md](../deep/kod/README.md)
