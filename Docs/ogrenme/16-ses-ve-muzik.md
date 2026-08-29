# Ses ve müzik — sessizliğin ölçüsü

Bu dosya tek bir soruya cevap verir: **oyuncunun duyması gereken şeylerin kaçı
bugün duyuluyor.**

Cevap sıfırdır ve bu dosya sıfırı yazmak için değil, sıfırın ne kadar pahalı
olduğunu ölçmek için var. Bir eksiğin sayısı bir karar değildir. Karar,
eksiğin bugün neye mal olduğunu ve hangi gün mal olmaya başlayacağını
söylediğinde doğar.

Ölçüm tarihi: **2026-08-28**. Bütün depo sayıları o gün koşturuldu. Bütün
motor iddiaları o gün Unity'nin kendi belgesinden okundu ve her birinin yanında
URL'si duruyor. Kod kazanır; bu sayfa kodla çeliştiği gün bayat olan burasıdır.

## Bu dosyanın yapmadığı dört şey

| Yapmaz | Kimin işi |
|---|---|
| Bir ses dosyası indirmez, üretmez, içeri almaz | operatör; bu sayfa yalnızca yolu yazar |
| `Assets/Art/THIRD_PARTY_ASSETS.md` manifestine satır **yazmaz** | manifest sahibi; buradaki satır TASLAKTIR |
| Görsel eksikleri saymaz | [14-gorsel-sozluk-ve-eksikler.md](14-gorsel-sozluk-ve-eksikler.md) |
| Bir desenin nasıl uygulanacağını yazmaz | [13-desen-secim-rehberi.md](13-desen-secim-rehberi.md) |

---

## 1 · Türün işitsel sözlüğü

### 1.1 Sözlük nasıl türetildi

Ezberden bir liste yazılmadı. Yöntem [14-gorsel-sozluk-ve-eksikler.md](14-gorsel-sozluk-ve-eksikler.md)
ile aynıdır, yalnız duyu değişir.

> Oyuncunun bu şeyin olduğunu anlaması için ne DUYULMALI?

Soru her tür için aynıdır ve liste hiçbir tür için aynı değildir. Bir yarış
oyunu aynı soruyla motor devri, lastik cızırtısı, çarpma ve tur zili üretir. İki
liste neredeyse hiç kesişmez ve türetme birebir aynıdır.

Fiiller ezberden değil, kodun kendi sonuç sözlüğünden okundu. Bu projede bir
eylemin nasıl bittiğini beş `enum` söylüyor: `AttackOutcome`, `MoveOutcome`,
`PlacementOutcome`, `ProductionOutcome`, `ReviveOutcome`. Beşinin toplam **26
değeri** var. Bunların **18'i bir REDDİR** ve **8'i bir başarıdır**.

***Bu sayı bu bölümün en önemli sayısıdır.*** Oyunun oyuncuya "hayır" demek için
on sekiz ayrı gerekçesi var, ve bugün on sekizi de aynı sesi çıkarıyor: hiç.

### 1.2 Sözlük × envanter farkı

| # | Fiil (oyuncu ne yapar) | Duyulması gereken | Verdik | Koddaki karşılığı |
|---|---|---|---|---|
| 1 | Tahtaya bakar | arka plan müziği | YOK | — |
| 2 | Paletten yapı türü seçer | palet tıkı | YOK | `PaletteEntryView` → `Clicked` |
| 3 | Yapıyı bir hücreye bırakır | yerleşme sesi | YOK | `PlacementOutcome` → `Placed` |
| 4 | Yerleştirmesi reddedilir | ret sesi | YOK | `PlacementOutcome` → `RejectedCellOccupied` |
| 5 | Bir birim seçer | seçim sesi | YOK | `BoardAdapter` → `SelectionChanged` |
| 6 | Birimi yürütür | adım sesi | YOK | `MoveOutcome` → `Moved` |
| 7 | Hareketi reddedilir | ret sesi | YOK | `MoveOutcome` → `RejectedUnreachable` |
| 8 | Saldırır | vuruş sesi | YOK | `AttackOutcome` → `Hit` |
| 9 | Saldırısı reddedilir | ret sesi | YOK | `AttackOutcome` → `RejectedOnCooldown` |
| 10 | Menzilli vuruşu izler | uçuş sesi | YOK | `ProjectileView` → `Update` |
| 11 | Vuruşun isabet ettiğini duyar | darbe sesi | YOK | `AttackOutcome` → `Hit` |
| 12 | Bir birimin düştüğünü duyar | düşme sesi | YOK | `AttackOutcome` → `HitAndDowned` |
| 13 | Bir birimin öldüğünü duyar | ölüm sesi | YOK | `Battle` → `UnitStateChanged` |
| 14 | Bir yapının yıkıldığını duyar | yıkım sesi | YOK | `AttackOutcome` → `HitAndDestroyed` |
| 15 | Bir birimi dirilttiğini duyar | diriliş sesi | YOK | `ReviveOutcome` → `Revived` |
| 16 | Üretimin başladığını duyar | üretim sesi | YOK | `ProductionOutcome` → `Allowed` |
| 17 | Üretimin bittiğini duyar | tamamlandı zili | YOK | `StructureProduction` → `IsReady` |
| 18 | Sıranın devrettiğini duyar | sıra sesi | YOK | `BoardAdapter` → `TurnChanged` |
| 19 | Kazandığını ya da kaybettiğini duyar | zafer/yenilgi ezgisi | YOK | `VictoryRules` → `Winner` |

**On dokuz maddenin on dokuzu YOK.**

Envanter tarafı da ölçüldü ve aynı sayıyı veriyor. `Assets/` altında `.wav`,
`.mp3`, `.ogg`, `.aiff`, `.flac`, `.m4a` ve `.mixer` uzantılı **sıfır dosya**
var. Üretim kodunda `AudioSource`, `AudioClip`, `PlayOneShot` ve `AudioMixer`
adları **sıfır kez** geçiyor. Sahnede tek bir ses bileşeni duruyor ve o da
kimsenin koymadığı bir bileşen: `Main Camera` üstündeki `AudioListener`, Unity'nin
her yeni sahneye kendiliğinden koyduğu kulak.

***Yani bu projede bir kulak var ve hiçbir ağız yok.***

### 1.3 Sessizliğin bugün oyuncuya okuttuğu yanlış

Sessizlik bir eksik değildir. Sessizlik bir CEVAPTIR ve oyuncu onu okur.

Bugün bu tahtada iki olay birbirinden ayırt edilemiyor.

```
OLAY A   Oyuncu bir hücreye tıkladı, kural REDDETTİ.
         MoveOutcome.RejectedCellOccupied döndü.
         Ekranda hiçbir şey değişmedi. Hiçbir şey duyulmadı.

OLAY B   Oyuncunun tıklaması hiç kaydedilmedi.
         Girdi hücrenin dışına düştü, hiçbir kural çağrılmadı.
         Ekranda hiçbir şey değişmedi. Hiçbir şey duyulmadı.
```

İki olayın ürettiği duyusal iz **birebir aynıdır**. Oyuncu birincisinde "bu
hamle yasak" öğrenmeli, ikincisinde "daha dikkatli tıkla" öğrenmelidir. İkisini
ayıramadığı için ikisini de öğrenemez ve üçüncü bir şey öğrenir: **"bu oyun bazen
tıklamalarımı yiyor."** Bu cümle yanlıştır ve bugünkü yapıdan doğrudan
türemektedir.

Bu, on sekiz ret değerinin hepsi için geçerlidir. Kural katmanı reddin
GEREKÇESİNİ biliyor; oyuncu reddin OLDUĞUNU bilmiyor.

Bir de bunun kaçınılmaz kıldığı bir mimari olgu var ve ölçüldü. Dört derleme
biriminin **üçü** `noEngineReferences: true` taşıyor: `GridStrategy.Core`,
`GridStrategy.Combat` ve `GridStrategy.Battle`. Yalnız `GridStrategy.Unity`
motora bakıyor. Yani `AudioSource` kural katmanından **derleyici tarafından**
erişilemez. Bu bir kısıt değil bir hediyedir: sesin nereye yazılacağı sorusu
tasarımla değil, derleme hatasıyla cevaplanmış durumda. Ret sesini çalacak kod
`GridStrategy.Unity` içinde yaşamak zorunda ve iki katmanın buluştuğu yer zaten
var: kuralın döndürdüğü sonuç değeri.

---

## 2 · Motor sorusu — Singleton gerekiyor mu

Bu bölüm bir merakı ölçüyle kapatmak için yazıldı. "Sahneler arası müzik"
Singleton'ın en çok anlatılan örneğidir ve bu proje o örneği hiç yaşamadı.

[13-desen-secim-rehberi.md](13-desen-secim-rehberi.md) her deseni önce tek bir
soruya sokuyor.

> **"Bunu Unity'nin kendi bir mekanizması zaten sahipleniyor mu?"**

Aşağıdaki beş soru o adımın müzik için koşturulmuş hâlidir.

### ① Tek sahne varken müzik neye ihtiyaç duyar?

Hiçbir C# satırına ihtiyaç duymaz.

Sahneye bir `GameObject` konur, üstüne bir `AudioSource` eklenir, `Loop` ve
`Play On Awake` işaretlenir. Unity'nin kendi belgesi `Play On Awake` için şunu
yazıyor: *"Enable this property to play the sound the moment the scene launches.
If you disable this property, you need to use the `Play()` command in your
scripts to start the audio."* (`docs.unity3d.com/6000.5/Documentation/Manual/AudioSource-reference.html`,
doğrulandı 2026-08-28.)

***Bugünkü müzik ihtiyacının kod tarafındaki maliyeti sıfır satırdır.*** Bir
desen tartışmasının başlayabilmesi için önce bir kodun var olması gerekir.

### ② İkinci sahne doğduğunda ne değişir?

Sahne yıkılır ve içindeki her `GameObject` onunla birlikte ölür. Müzik durur ve
menüye girildiği an baştan başlar.

Doğan baskının adı **ÖMÜR**dür.

Baskının adı erişim DEĞİLDİR ve bu ayrım bu bölümün eksenidir. Singleton iki şey
vaat eder ve `13-desen-secim-rehberi.md` ikisini ayırarak yazıyor: **teklik** ve
**küresel erişim**. Sahne geçişinde müziğin sustuğu olgu bu ikisinden hiçbirini
istemez. Üçüncü bir şey ister ve o şeyin adı ömürdür.

### ③ `DontDestroyOnLoad` ne çözer, ne çözmez?

**Çözer:** ömrü. Nesne sahne yüklemesinden sağ çıkar.

**Çözmez, birinci:** tekliği. Oyuncu menüden savaşa döndüğünde savaş sahnesi
yeniden yüklenir, içindeki müzik nesnesi yeniden doğar, ve şimdi İKİ müzik
çalıyor. Üçüncü gidiş gelişte üç tane olur.

**Çözmez, ikinci:** erişimi. `DontDestroyOnLoad` hiçbir çağırana hiçbir referans
vermez. Yeni sahnedeki bir ses ayarı düğmesi o nesneyi bulmak için hâlâ bir yol
bulmak zorundadır.

***İşte Singleton'ın gerçek doğum yeri burasıdır ve müzik değildir.*** Çoğaltma
sorununu gören geliştirici `if (Instance != null) Destroy(gameObject);` yazar,
ve o satır yazıldığı an `static Instance` alanı doğmuş olur. Yani Singleton'ı
doğuran şey müziğin ihtiyacı değil, `DontDestroyOnLoad`'ın kendi kusurudur.
Kusuru olan aracı seçmemek, kusuru yamayan deseni hiç doğurmamak demektir.

### ④ Additive sahne yükleme aynı işi Singleton'sız yapar mı?

Evet.

Müzik, hiç boşaltılmayan bir üçüncü sahnede yaşar. Savaş ve menü sahneleri onun
üstüne `LoadSceneMode.Additive` ile yüklenir ve boşaltılır. Müzik sahnesi bir
kez yüklendiği için ikinci bir örneği hiç doğmaz.

Bu şeklin `DontDestroyOnLoad`'a karşı iki ölçülebilir üstünlüğü var.

- Teklik, bir `static` alanın koruduğu bir kural olmaktan çıkar ve bir sahne
  dosyasının VARLIĞI olur. Bir dosya iki kez var olamaz.
- Sahne yükleme sırası Editor'da görünür. `File > Build Profiles` penceresindeki
  liste, hangi sahnenin ne zaman yaşadığını okunur kılar.

Sahiplik bir C# alanından bir varlık dosyasına taşındı. `13-desen-secim-rehberi.md`
bu hamlenin adını zaten koymuş durumda ve Singleton satırında şunu yazıyor:
motor bu baskıyı emiyor.

### ⑤ `AudioMixer` ses ayarının sahibi olabilir mi?

Evet, ve bu cevap bir "AudioManager" tipinin doğmasını engelliyor.

Unity'nin belgesi `AudioMixer` için şunu yazıyor: *"The Audio Mixer is an asset
that Audio Sources reference to apply complex routing and mixing to the audio
signal they generate."* Aynı sayfa şunu da yazıyor: *"Snapshots capture the
state of an Audio Mixer and transition between those states as your application
runs."* (`docs.unity3d.com/6000.5/Documentation/Manual/AudioMixerOverview.html`,
doğrulandı 2026-08-28.)

Ses seviyesini bir betikten değiştirmenin yolu `AudioMixer.SetFloat` ve
belgesi şunu diyor: *"SetFloat sets the value of the exposed parameter
specified."* Aynı sayfa parametrenin nasıl açığa çıkarıldığını da yazıyor:
*"To expose a parameter, go to the Audio Mixer group's Inspector window, right
click the parameter you want to expose, and choose Expose [parameter name] to
script."* (`docs.unity3d.com/6000.5/Documentation/ScriptReference/Audio.AudioMixer.SetFloat.html`,
doğrulandı 2026-08-28.)

Sonuç şudur. Ses seviyesi **diskteki tek bir dosyada** yaşar ve o dosyayı
gösteren herkes aynı dosyayı gösterir. Teklik zaten vardır. Erişim, o dosyaya
bakan bir `[SerializeField] AudioMixer` alanına sürükleyerek verilir. Bu, `13-desen-secim-rehberi.md`'nin
`ScriptableObject` için yazdığı argümanın birebir aynısıdır ve `AudioMixer` de
bir varlık dosyasıdır.

Snapshot'lar bunun üstüne bir şey daha ekliyor. "Menüye girince müziği kıs"
cümlesi bir KOD dalı olmaktan çıkıp iki snapshot arasında bir geçiş olur. Yani
karar da veriden okunur.

### 2.6 Hüküm

***Singleton bu özellik için ZORUNLU DEĞİL, KOLAYLIKTIR.*** Ölçüsü dört
basamaklıdır ve dördü de bugün depoda doğrulanabilir.

| Basamak | Ölçü | Bugünkü değer |
|---|---|---|
| Kaç sahne var | `Assets/**/*.unity` sayımı | **1** (`SampleScene.unity`) |
| Müziğin bugün gerektirdiği kod | `AudioSource` + iki Inspector kutusu | **0 satır** |
| `DontDestroyOnLoad` kaç yerde geçiyor | üretim kodu taraması | **0** |
| Motora bakan derleme birimi | `noEngineReferences: false` sayımı | **4'te 1** |

İkinci sahne doğduğunda bile hüküm değişmiyor ve sebebi şu: doğan baskının adı
ömürdür, ve ömrü motorun kendi iki mekanizması sahipleniyor. Additive boot
sahnesi tekliği bir dosyaya, `AudioMixer` varlığı ayarı bir dosyaya, `[SerializeField]`
sürüklemesi erişimi bir bağa veriyor. Singleton'ın iki vaadinin ikisi de
başkalarına dağıtılmış oluyor.

***Kolaylık olduğunu söylemek yasak olduğunu söylemek değildir.*** Singleton bu
işi yapar ve daha az yazmayla yapar. Ödediği bedel şudur: bağımlılık imzadan
silinir ve testte yerine başka bir şey konamaz. `13-desen-secim-rehberi.md` bu
bedeli Service Locator ile Singleton'ı ayırdığı satırda zaten yazmış durumda.

### 2.7 `13-desen-secim-rehberi.md` ile çelişme denetimi

Çelişki **yok** ve denetim tek tek yapıldı.

| `13`'ün dediği | Bu dosyanın dediği | Durum |
|---|---|---|
| Singleton bu projede yok, değiştirilebilir `static` alan yok | Aynı; müzik de bir tane doğurmuyor | Uyumlu |
| Motor bu baskıyı emiyor: sahnedeki tek nesne artı serileştirilmiş referans | Aynı mekanizma, artı additive boot sahnesi ve `AudioMixer` varlığı | Uyumlu, genişletilmiş |
| Tetikleyici iki koşulun BİRLİKTE gerçekleşmesi | Müzikte ikinci koşul sağlanmıyor; boot sahnesi bir referans yolu bırakıyor | Uyumlu |
| Taşıyıcı yine Singleton değildir | Aynı | Uyumlu |

Bir AYRIM var ve çelişki değil, kapsam farkı. `13`'ün Singleton bölümündeki
özellik **savaşın durumunu** sahne geçişinde korumaktır. Bu dosyanınki
**müziğin çalmaya devam etmesidir**. İki özellik iki ayrı satırdır ve ikisinin
cevabı aynı çıktı; ama aynı çıkması bir tesadüf değil, aynı motor mekanizmasının
iki kez cevap vermesidir.

---

## 3 · Performans — birincil kaynaktan, tarihli

Bu bölümdeki her iddianın yanında kaynağı ve doğrulama tarihi duruyor.
Doğrulanamayan her iddia `DOĞRULANMADI` diye işaretli. Kaynaksız sayı bu
dosyada yasaktır.

**Yerel olgular.** Proje sürümü `ProjectSettings/ProjectVersion.txt` içinde
`m_EditorVersion: 6000.5.7f1` yazıyor, yani Unity 6.5. Belgeler bu yüzden
`docs.unity3d.com/6000.5/...` ağacından okundu ve o ağacın sayfaları başlıklarında
"Unity 6.5 (6000.5)" yazdığı doğrulandı.

### 3.1 Yükleme tipi — hangi uzunluk hangi tipi ister

Kaynak: `docs.unity3d.com/6000.5/Documentation/Manual/class-AudioClip.html`,
doğrulandı 2026-08-28.

| `Load Type` | Belgenin sözü | Bu projede kime uyar |
|---|---|---|
| `Decompress On Load` | *"Decompress audio files as soon as they're loaded. Use this option for smaller compressed sounds to avoid the performance overhead of decompressing during gameplay."* | Kısa efektlerin hepsi |
| `Compressed In Memory` | *"Keep audio compressed in memory and decompress while playing. This option has a slight performance overhead, especially for Ogg/Vorbis compressed files."* | Bu projede kimse |
| `Streaming` | *"Decode continuous audio. This method uses a minimal amount of memory to buffer compressed data that's incrementally read from the disk and decoded spontaneously."* | Arka plan müziği |

Aynı sayfa iki sayı daha veriyor ve ikisi de kararı belirliyor.

Birincisi: *"decompressing Vorbis-encoded sounds on load will use about ten times
more memory than keeping them compressed (for ADPCM encoding it's about 3.5
times)."*

İkincisi: *"Streaming clips have an overhead of approximately 200KB, even
without loaded audio data."*

***İkinci sayı `Streaming`'i kısa efektler için doğrudan eliyor.*** 200 KB sabit
gider, 13 KB'lık bir tık sesi için ödenecek bir bedel değildir.

### 3.2 Sıkıştırma biçimi

Kaynak aynı sayfa, doğrulandı 2026-08-28.

| Biçim | Belgenin sözü |
|---|---|
| `PCM` | *"Choose this option for short sound effects, and for higher quality audio at the expense of larger file sizes."* |
| `ADPCM` | *"Use this format for sounds that contain a lot of noise and play in large quantities, such as footsteps, impacts, and weapons"* |
| `Vorbis` | *"This format is best for medium length sound effects and music"* |

`ADPCM` satırındaki üç örnek bu projenin ihtiyacına şaşırtıcı ölçüde denk
düşüyor: **footsteps, impacts, weapons**. Sözlük tablosunun 6, 8, 11 ve 12
numaralı maddeleri tam olarak bunlardır.

**Platform başına ayrım: `DOĞRULANMADI`.** Unity'nin `Audio Clip Import
Settings` sayfası platform sekmesi (override) mekanizmasının varlığını yazıyor,
ama hangi platformda hangi biçimin donanımdan destek gördüğüne dair bir tablo bu
sayfada okunmadı. Bu projenin hedef platformu da bir yerde yazılı değil, yani
ayrımı sormak için gereken ikinci bilgi de eksik. Bu satır bir hedef platform
seçildiği gün doldurulmalı.

### 3.3 `Preload Audio Data` ve `Load In Background`

Kaynak aynı sayfa, doğrulandı 2026-08-28.

`Preload Audio Data`: *"Enable to preload the audio clip after the scene fully
loads. This setting is enabled by default."*

`Load In Background`: *"Enable this setting to load the audio clip in the
background, which prevents stalls on the main thread."*

İkisi bir arada bir zaman/anlık takas kurar. `Preload` açıkken maliyet sahne
yüklemesine yığılır ve oyun sırasında hiç ödenmez. `Preload` kapalıyken maliyet
ilk çalma anına kayar ve o an bir kare sıçraması olarak görünebilir. `Load In
Background` o maliyeti ana iş parçacığından alır.

Bu projede bugün alınacak bir karar yok, çünkü yüklenecek bir klip yok. Karar,
ilk müzik dosyası içeri alındığı gün doğar.

### 3.4 `PlayOneShot` ile havuzlanmış `AudioSource` arasındaki gerçek fark

Kaynak: `docs.unity3d.com/6000.2/Documentation/ScriptReference/AudioSource.PlayOneShot.html`,
doğrulandı 2026-08-28.

Belgenin cümlesi şu: *"PlayOneShot does not cancel clips that are already being
played by PlayOneShot and Play."*

Buradan çıkan gerçek fark, çok anlatılan "havuz daha hızlıdır" cümlesi değildir.

**`PlayOneShot` zaten üst üste bindirir.** Tek bir `AudioSource` üstünden
art arda çağrılan `PlayOneShot`, önceki sesi kesmeden yeni bir ses başlatır.
Yani "aynı anda iki tık sesi" için ikinci bir `AudioSource`'a gerek yoktur.

**Havuzun aldığı şey per-ses AYARDIR.** `PlayOneShot` ses ölçeğinden başka bir
şey vermez. Sesin perdesi, uzamsal karışımı, çıkış grubu ve önceliği çalan
`AudioSource` bileşeninindir. İki sesin farklı perdede ya da farklı `AudioMixer`
grubunda çalması isteniyorsa iki ayrı bileşen gerekir, ve havuzun cevapladığı
soru budur.

**Kaçınılması gereken üçüncü yol var ve adı yazılı.** `AudioSource.PlayClipAtPoint`
belgesi şunu diyor: *"This function creates an audio source but automatically
disposes of it once the clip has finished playing."*
(`docs.unity3d.com/6000.5/Documentation/ScriptReference/AudioSource.PlayClipAtPoint.html`,
doğrulandı 2026-08-28.) Yani her çağrı bir `GameObject` doğurur ve öldürür. Bu
projenin `UnitViewPool` tipiyle görsel tarafta reddettiği doğum/ölüm trafiğinin
ses tarafındaki ikizidir.

**Ölçülmemiş olan:** `PlayOneShot` ile havuzlanmış `AudioSource` arasındaki CPU
farkı. `DOĞRULANMADI`, ve doğrulanması bir profil koşumu gerektirir. Unity'nin
`Audio Profiler` modülü bunu ölçen sayaçları taşıyor: *"Playing Audio Sources"*,
*"Audio Voices"*, *"Total Audio CPU"* ve *"Total Audio Memory"*
(`docs.unity3d.com/6000.5/Documentation/Manual/ProfilerAudio.html`, doğrulandı
2026-08-28). Sıfır klipli bir projede o profili çalıştırmanın söyleyeceği bir
şey yok.

### 3.5 Bellek — formül ve bu projedeki değerleri

Sıkıştırılmamış PCM verisinin boyutu bir motor iddiası değil, bir aritmetiktir.

```
bayt = süre_saniye × örnekleme_hızı × kanal_sayısı × örnek_başına_bayt
```

`örnek_başına_bayt` için 2 alındı, yani 16 bit. ***Bu bir ÇIKARIMDIR ve
kaynaktan okunmadı.*** Doğruluğu şöyle sınandı ve Unity'nin kendi sayısıyla
tutuyor.

| Adım | Hesap | Sonuç |
|---|---|---|
| 2 dakika, stereo, 44100 Hz, 16 bit | `120 × 44100 × 2 × 2` | `21 168 000` bayt = **20,2 MiB** |
| Aynı parça, Vorbis ~128 kbps | `120 × 128000 / 8` | `1 920 000` bayt = **1,83 MiB** |
| Oran | `20,2 / 1,83` | **11,0×** |

Unity'nin belgesi Vorbis için *"about ten times more memory"* diyor. Çıkarımla
elde edilen 11,0× o cümleyle tutarlıdır, yani 16 bit varsayımı yanlış değildir.
Yine de bir çıkarım olarak işaretli kalıyor; tutarlılık bir kanıt değildir.

Aynı formül kısa efektler için şunu veriyor.

| Klip | Hesap | Sonuç |
|---|---|---|
| 0,3 sn, mono, 22050 Hz, 16 bit | `0,3 × 22050 × 1 × 2` | `13 230` bayt ≈ **12,9 KiB** |
| Sözlüğün 18 efektinin hepsi, `Decompress On Load` | `18 × 13 230` | `238 140` bayt ≈ **233 KiB** |

***Bu tablonun tek cümlelik özeti şudur:*** bu projenin BÜTÜN ses efekti
sözlüğünü RAM'de açık tutmanın bedeli (233 KiB), TEK BİR `Streaming` klibinin
sabit giderinden (yaklaşık 200 KB) yalnızca biraz büyüktür. Efekt tarafında
bellek bir karar değildir; müzik tarafında karar `Streaming` ile
`Decompress On Load` arasındadır ve fark 20 MiB'tır.

### 3.6 Unity 6'da ses tarafında değişen

Kaynak: `docs.unity3d.com/6000.5/Documentation/Manual/WhatsNewUnity6.html`,
doğrulandı 2026-08-28.

Sayfanın Audio bölümü iki satır taşıyor ve ikisi de aynı özelliğe ait.

> *"Added the Audio Random Container to randomize audio and ensure that volume,
> pitch, time and triggers can be set to non-repetitive intervals, so your game
> never sounds the same twice."*

> *"Added a VU meter to the Audio Random Container."*

`Audio Random Container` belgesi onu şöyle tanımlıyor: *"An Audio Random
Container is an object that lets you create audio playlists for your scene and
apply rules to determine when and how the clips play."* Adını verdiği kullanım
alanları: *"footsteps, weapon hits, and background music."*
(`docs.unity3d.com/6000.5/Documentation/Manual/AudioRandomContainer-fundamentals.html`,
doğrulandı 2026-08-28.)

Bunu taşıyan API değişikliği de var. `AudioResource` şöyle tanımlı: *"Represents
an audio generator asset that you can play through an AudioSource."* Aynı sayfa
şunu ekliyor: *"if your audio generator is an AudioClip, you can access these
properties through AudioSource.clip."*
(`docs.unity3d.com/6000.5/Documentation/ScriptReference/Audio.AudioResource.html`,
doğrulandı 2026-08-28.)

***Bu, bu projenin karar tablosunu doğrudan etkiliyor.*** "Aynı vuruş sesini
otuz kez üst üste duymak" bir sıkılma kaynağıdır ve klasik çözümü bir
`AudioClip[]` dizisi ile `Random.Range` çağrısıdır. Unity 6 o kodu bir varlık
dosyasına taşıdı. `13-desen-secim-rehberi.md`'nin ADIM 0 sorusu burada da "evet"
cevabı alıyor: rastgele seçim bir desen değil, bir `.asset` dosyası.

**Bu sayfanın söylemediği:** `Audio Random Container` dışında Unity 6 hattında
ses tarafında başka bir değişiklik. `WhatsNewUnity6.html` sayfası başka bir ses
maddesi taşımıyor. 6000.1 ile 6000.5 arasındaki ARA sürümlerin ayrı sürüm
notları bu araştırmada **okunmadı**, yani "Unity 6'da başka hiçbir şey
değişmedi" cümlesi `DOĞRULANMADI` sayılmalıdır.

### 3.7 Doğrulanan ve doğrulanmayan iddiaların ayrı listesi

**DOĞRULANDI (hepsi 2026-08-28, birincil kaynak Unity belgeleri):**

- Üç `Load Type` değerinin tanımı ve önerilen kullanımı.
- Vorbis için ~10×, ADPCM için ~3,5× bellek artışı.
- `Streaming` için klip başına ~200 KB sabit gider.
- `PCM` / `ADPCM` / `Vorbis` için önerilen kullanım cümleleri.
- `Preload Audio Data` varsayılan olarak açıktır.
- `Load In Background` ana iş parçacığındaki takılmayı önler.
- `PlayOneShot` çalan sesleri kesmez.
- `PlayClipAtPoint` bir `AudioSource` yaratır ve klip bitince yok eder.
- `Priority` aralığı 0-256, varsayılan 128, müzik için 0 önerilir.
- `AudioMixer` bir varlık dosyasıdır ve snapshot'ları alt varlık olarak taşır.
- `AudioMixer.SetFloat` açığa çıkarılmış parametreyi yazar.
- `Audio Random Container` Unity 6 ile geldi ve `AudioResource` onu taşıyor.
- `Audio Profiler` modülünün sayaç adları.

**DOĞRULANMADI:**

- Platform başına sıkıştırma biçimi ayrımı. Sebep iki katlı: sayfa böyle bir
  tablo vermiyor ve bu projenin hedef platformu da yazılı değil.
- Unity'nin çözdüğü PCM verisini 16 bit olarak tuttuğu. Çıkarımdır, kendi
  10× sayısıyla tutarlıdır, kaynaktan okunmamıştır.
- `PlayOneShot` ile havuzlanmış `AudioSource` arasındaki CPU farkı. Profil
  koşumu gerektirir ve ölçülecek klip yoktur.
- Kenney ses paketlerinin dosya biçimi (`.ogg` mu `.wav` mı). Ürün sayfaları
  biçim yazmıyor; arşiv indirilmediği için içerik okunmadı.
- 6000.1 ile 6000.5 arası ara sürümlerin ses maddeleri. Ara sürüm notları
  okunmadı.

---

## 4 · Yük sayımı

Yedi eksenin her birine bir satır ve her satıra üç alan. Bir eksen ölçülmediyse
`ÖLÇÜLMEDİ` yazılı ve satır duruyor; ölçülmemiş bir eksen sıfır değildir.

`HEDEF` sütunundaki sayı seçilmedi, TÜRETİLDİ. Türetme aşağıda yazılı.

| Eksen | BUGÜN | HEDEF | ÖLÇÜLDÜ MÜ |
|---|---|---|---|
| `N` varlık sayısı | en fazla 15 | en fazla 15 | **Evet.** `BoardAdapter` içinde `width = 3` ve `height = 5`; `UnitGrid` içinde `cells = new Unit[width, height]` |
| `NxM` çift sayısı | sesin eklediği 0 | 0 | **Evet.** Ses bir hedefleme sorusu sormaz; kaynağı olayın kendisidir |
| `d/s` yapısal değişim | sesin eklediği 0 | 0 | **Evet.** `PlayOneShot` nesne doğurmaz; doğuran yol `PlayClipAtPoint` ve o reddedildi |
| `draw` çizim çağrısı | sesin eklediği 0 | 0 | **Evet.** Ses çizmez |
| `phys` fizik | sesin eklediği 0 | 0 | **Evet.** Ses çarpışmaz |
| `io` ses, UI, girdi | 0 klip, 0 `AudioSource`, 1 `AudioListener` | 18 klip, 2-4 `AudioSource` | **Evet, envanter tarafı.** CPU tarafı `ÖLÇÜLMEDİ` |
| `mem` bellek | 0 bayt ses | ~233 KiB efekt + 1,83 MiB müzik | **Kısmen.** BUGÜN sayıldı; HEDEF formülden türetildi, profille ölçülmedi |

### 4.1 Aynı anda kaç ses çalabilir — üst sınırın türetilmesi

Sayı bir tercih değil, üç kuralın çarpımıdır.

```
① UnitGrid  : cells = new Unit[width, height]     -> hücre başına EN FAZLA bir varlık
② BoardAdapter : width = 3 , height = 5           -> 15 hücre, yani 15 varlık
③ BoardAdapter : turnMode = TurnMode.FreeForAll   -> sıra kapısı YOK, herkes her an eyleyebilir
④ BoardAdapter : attackCooldownSeconds = 1f       -> varlık başına saniyede en fazla 1 saldırı

    15 varlık × 1 saldırı/sn × 2 ses/saldırı (atış + darbe) = 30 ses BAŞLANGICI / saniye
```

Üçüncü satır bu türetmenin en önemli parçasıdır. `TurnMode` tipinin sıfırıncı
değeri `Alternating`, yani sıra bir kapıdır. Ama `BoardAdapter` üstünde
serileştirilmiş varsayılan `TurnMode.FreeForAll` ve o kipte sıra kapısı yoktur.
***Yani projenin sevk edilen varsayılanı, eşzamanlılık açısından en kötü
durumdur.***

Bu 30 sayısı bir BAŞLANGIÇ hızıdır, bir eşzamanlılık değil. İkisi şöyle
ayrışıyor.

| Soru | Hesap | Sonuç |
|---|---|---|
| Patolojik anlık üst sınır (30 sesin hepsi aynı karede başlarsa) | `30` | **30 eşzamanlı ses** |
| Gerçekçi kararlı durum (0,3 sn'lik klipler) | `30 × 0,3` | **9 eşzamanlı ses** |

Bu iki sayıyı karşılaştıracağımız bütçe yerel olarak ölçüldü.
`ProjectSettings/AudioManager.asset` içinde `m_RealVoiceCount: 32` ve
`m_VirtualVoiceCount: 512` yazıyor. Unity'nin belgesi `Max Real Voices` için
şunu diyor: *"Set the number of real voices that can play at the same time. At
every frame, the loudest voice is picked."*
(`docs.unity3d.com/6000.5/Documentation/Manual/class-AudioManager.html`,
doğrulandı 2026-08-28.)

### 4.2 Hüküm — ses bu projede bir performans sorunu MU

**Hayır, ve marj iki sestir.**

```
patolojik anlık üst sınır   30
sevk edilen gerçek ses sayısı   32
                            ────
marj                          2
```

***Bu marjın ne kadar dar olduğu bu bölümün asıl bulgusudur.*** Tahta ölçüsü bir
tasarımcı sayısıdır: `BoardAdapter` üstünde `[SerializeField, Min(1)] private int
width = 3` ve aynısı `height` için. O sayı büyüdüğü an marj kapanıyor.

```
hücre_sayısı × 2 ses ≥ 32   ->   hücre_sayısı ≥ 16
```

**16 hücre.** Yani `4 × 4`. Bugünkü tahtaya BİR hücre eklemek, ses bütçesini
tam olarak doldurur. Bunun ötesinde Unity en gürültülü sesi seçmeye başlar ve
sessizleşen ilk şey en kısık ses olur; oyunda bunun karşılığı büyük olasılıkla
bir adım ya da bir ret sesidir, yani **en çok bilgi taşıyan seslerdir**.

Bu, `unity-expert-code-quality` kural 20'nin tarif ettiği şeklin ses tarafındaki
örneğidir: maliyeti bir tasarımcı sayısına bağlı ve tavanı hiçbir yere yazılmamış
bir yol. Tavan şimdi yazılı ve dört alanı taşıyor: sayı `BoardAdapter.width` ile
`BoardAdapter.height`, bugünkü değer `3 × 5 = 15`, tavan `16 hücre`, tavanın
üstünde işi devralan sahip Unity'nin kendi ses çalma önceliği yani `AudioSource`
üstündeki `Priority` alanı.

O alanın belgesi bu devralmayı zaten yazıyor: *"Priority: 0 is most important,
while 256 is least important. Default is 128. Use 0 for music tracks to avoid it
getting occasionally swapped out."*
(`docs.unity3d.com/6000.5/Documentation/Manual/AudioSource-reference.html`,
doğrulandı 2026-08-28.) Yani tavanın üstündeki dünyada kararı kod değil,
Inspector'daki bir sayı verir.

---

## 5 · Kaynak bulma

Basamak sırası dörttür ve sırayla denendi.

| Basamak | Soru | Ses için bugünkü cevap |
|---|---|---|
| ① Zaten içeri alınmış mı | `Assets/` altında ses dosyası var mı | **KAPALI.** Sıfır dosya |
| ② Türetilebilir mi | eldeki bir varlıktan üretilebilir mi | **KAPALI.** Bir PNG'den ses türetilemez; `Assets/Art/Derived/` altında yalnız görsel var |
| ③ Zaten lisanslı ve KAYITLI bir pakette mi | manifest bir ses paketi kaydediyor mu | **KAPALI.** `Assets/Art/THIRD_PARTY_ASSETS.md` üç Kenney GÖRSEL paketi kaydediyor; kapsamı kendi başlığında "the 32 PNG files" diye yazılı |
| ④ Yeni paket | hangi paket, hangi lisans | **AÇIK.** Aşağıda |

***İlk üç basamağın üçü de kapalı, yani ses tek başına dördüncü basamağa
düşüyor.*** Bu, görsel tarafın bugünkü durumundan yapısal olarak farklıdır: orada
manifest var ve üçüncü basamak çalışıyor.

### 5.1 Dördüncü basamak — araştırılan adaylar

Her adayın ürün sayfası açıldı ve lisans adı sayfadan okundu. Doğrulama tarihi
hepsi için 2026-08-28.

| Aday | Ürün sayfası | İçerik | Lisans (sayfadan okundu) | Atıf zorunlu mu |
|---|---|---|---|---|
| Kenney — Impact Sounds | `kenney.nl/assets/impact-sounds` | 130 dosya | "Creative Commons CC0" | Hayır |
| Kenney — Interface Sounds | `kenney.nl/assets/interface-sounds` | 100 ses | "Creative Commons CC0" | Hayır |
| Kenney — RPG Audio | `kenney.nl/assets/rpg-audio` | 50 dosya | "Creative Commons CC0" | Hayır |
| Freesound — CC0 kesiti | `freesound.org` | değişken | CC0, CC-BY ve CC-BY-NC karışık | CC0'da hayır, CC-BY'de **evet** |

**Kenney neden birinci sırada.** Projenin bütün görselleri zaten Kenney'den ve
manifest üç paketi CC0 diye kaydetmiş durumda. Aynı yayıncıdan ses almak, lisans
denetimini yeniden yapmayı gerektirmez ama **muaf da kılmaz**; her yeni paketin
kendi `License.txt` dosyası okunmak zorundadır.

**Rol eşleşmesi.** `Impact Sounds` sözlüğün 8, 11, 12 ve 14 numaralı maddelerine
bakıyor. `Interface Sounds` 2, 4, 5, 7, 9 ve 17 numaralı maddelere bakıyor.
`RPG Audio` sayfası `footstep` ve `weapon` etiketleri taşıyor, yani 6 numaralı
maddeye bakıyor.

**Freesound neden ikinci sırada ve neden riskli.** Freesound'un SSS sayfası üç
lisansı sayıyor ve ikisi atıf istiyor: *"CC-BY (Attribution): you should always
mention the original creators"*. (`freesound.org/help/faq/`, doğrulandı
2026-08-28.) Yani Freesound'dan alınan her dosyanın lisansı **tek tek**
okunmalıdır. Tek bir paketin tek bir lisansı olan Kenney'e göre denetim yükü
dosya sayısı kadar artar.

**Müzik için hiçbir aday araştırılmadı.** Kenney'in `Music Jingles` paketi ürün
listesinde görünüyor ama sayfası açılmadı, yani lisansı OKUNMADI. Arka plan
müziği bu araştırmanın kapsamı dışında kaldı ve bu bir eksiktir, bir hüküm
değil.

### 5.2 Manifest satırının TASLAĞI

`Assets/Art/THIRD_PARTY_ASSETS.md` biçimi incelendi ve ses için eşdeğeri
aşağıdadır. ***Bu bir TASLAKTIR ve manifestin kendisine dokunulmadı.***

Mevcut manifest her paket için altı alan taşıyor: ürün sayfası, okunan arşiv
URL'si, arşivin SHA-256'sı, lisansın tam adı, okunan CC0 hukuk metni ve
arşiv içindeki `License.txt` dosyasından okunan cümle. Ses için de aynı altı
alan gerekir.

```
### Kenney Interface Sounds

- Product page: <https://kenney.nl/assets/interface-sounds>
- Archive read: <ARŞİV İNDİRİLMEDİ>
- Archive SHA-256: <HESAPLANAMAZ — dosya elde değil>
- Exact licence: **CC0 1.0 Universal**   (ÜRÜN SAYFASI "Creative Commons CC0" diyor;
                                          arşiv içi License.txt OKUNMADI)
- Full CC0 legal text read: <https://creativecommons.org/publicdomain/zero/1.0/legalcode>
- In-archive text read: <OKUNMADI>
```

***SHA-256 dosya elde olmadan hesaplanamaz ve bu satıra uydurma bir değer
yazılmadı.*** Aynı kısıt "arşiv içi metin okundu" alanı için de geçerlidir:
mevcut manifest o alanı üç paketin üçünde de gerçekten okunmuş bir cümleyle
dolduruyor, ve o cümle indirilmemiş bir arşivden alınamaz.

Bir alan daha eklenmelidir ve görsel manifestte karşılığı yoktur: **içeri
alma sözleşmesi**. Görsel tarafta o sözleşme `Pixels Per Unit`, `Filter Mode` ve
`Compression` diyor. Ses tarafında `Load Type`, `Compression Format`,
`Preload Audio Data` ve `Load In Background` demelidir, yani §3'ün dört alanı.

---

## 6 · Karar ve tetikleyici

### 6.1 Beş eksenli puanlama

| Eksen | Soru | Puan | Gerekçe |
|---|---|---|---|
| Hazırlık | Kod tabanı bu değişikliğe ne kadar hazır | **5** | Beş sonuç `enum`'ı ve dört olay zaten var; sesin bağlanacağı dikiş açık |
| Bağımlılık riski | Yakınında oynak bir sistem var mı | **4** | `GridStrategy.Unity` dışındaki üç birim motora bakmıyor, yani ses onları hiç kıpırdatmaz |
| Stratejik değer | Ne kadar kalıcı değer katar | **3** | On sekiz ret değerinin görünürlüğü gerçek bir oynanış kazancı; ama görsel geri bildirim aynı boşluğun bir kısmını daha ucuza kapatıyor |
| Kod çalkantısı | Yakında yeniden yazılır mı | **2** | Ses seviyesi, kip ve karışım kararları henüz hiç verilmedi; bugün yazılan bağlama yarın `AudioMixer` gelince değişir |
| Mevcut altyapı | Uzatılacak yerleşik bir desen var mı | **1** | Sıfır klip, sıfır `AudioSource`, sıfır manifest satırı; her şey sıfırdan |

**Toplam: 15 / 25.**

### 6.2 Hüküm

## Strategic Assessment

| Alan | Hazırlık | Bağımlılık | Stratejik değer | Öneri |
|---|---|---|---|---|
| A · Sesi ŞİMDİ ekle (18 maddelik sözlüğün tamamı) | %30 | Düşük risk | Orta | **Bekle** |
| B · Yalnız RET sesini ekle (tek klip, tek `AudioSource`) | %85 | Düşük risk | Yüksek | **Bekle, ama tetikleyicisi yakın** |
| C · Hiç ekleme, koşulu yaz | %100 | Yok | Düşük | **İlerle** |

## Öneri: C, ve içindeki B tetikleyicisiyle birlikte

**Gerekçe:** Üç ölçü bu kararı veriyor. Birincisi, ses bugün bir performans
sorunu değil ve §4 bunu 30'a karşı 32 ile gösterdi; yani aciliyet performanstan
gelmiyor. İkincisi, kaynak basamağının ilk üçü kapalı, yani en küçük ses işi
bile bir paket indirmeyi, bir lisans okumayı ve bir manifest satırı yazmayı
gerektiriyor; bu, tek bir `.wav` için ödenecek gerçek bedeldir. Üçüncüsü ve
belirleyici olanı: bugün ekranda hâlâ **üç görsel eksik** duruyor ve
[14-gorsel-sozluk-ve-eksikler.md](14-gorsel-sozluk-ve-eksikler.md) üçünün de
savaşın SONUCUNA baktığını ölçmüş durumda. Aynı boşluğa iki duyudan aynı anda
saldırmak, hangisinin işe yaradığını ölçülemez kılar.

**Takas:** Oyuncu bir süre daha reddi göremeyecek ve duyamayacak. Bu bedel
bilinerek ödeniyor ve tetikleyicisi aşağıda yazılı.

**Reddedilen alternatif:** A seçeneği, yani sözlüğün tamamı. Reddedilme sebebi
ölçülü: 18 klibin her biri bir `AudioSource` bağı, bir içeri alma ayarı ve bir
manifest satırı demektir, ve bunların hiçbiri bugün var olmayan bir ses karışımı
kararının altında yapılmamalıdır. `AudioMixer` gelmeden bağlanan 18 klip,
`AudioMixer` geldiği gün 18 kez yeniden bağlanır.

### 6.3 "Şimdi gerekmez" cümlesinin ikinci yarısı

***Ses şimdi gerekmez, ve şu gün gerekli olur:*** oyuncunun bir eylemi
reddedildiğinde reddi FARK ETMEDİĞİ ilk kez gözlemlendiğinde. Ölçü bir sayı
değil bir gözlemdir çünkü kanıt kovası burada insandır; kural 18'in altıncı
kanıt kovasını hiçbir makine kapatmaz.

### 6.4 Reddedilen her şeyin yeniden açma koşulu

| Reddedilen | HANGİ EKSEN | HANGİ EŞİK | HANGİ KANIT KOVASI | KİM YENİDEN ÖLÇER |
|---|---|---|---|---|
| Ses efektlerinin tamamı (A) | `io` | Görsel sözlüğün üç eksiği kapandığında, yani `14-gorsel-sozluk-ve-eksikler.md` §1.3'te YOK sayısı 0 olduğunda | editör görünürlüğü kanıtı, insan gözlemi | operatör |
| Ret sesi (B) | `io` | Operatör bir oturumda kendi reddini iki kez fark etmediğinde | editör görünürlüğü kanıtı, insan gözlemi | operatör |
| Arka plan müziği | `mem` | İkinci sahne dosyası doğduğunda | Editor/çalışma zamanı kanıtı | operatör |
| Havuzlanmış `AudioSource` (`PlayOneShot` yerine) | `io` | İki sesin farklı perdede ya da farklı `AudioMixer` grubunda çalması gerektiğinde | profil kanıtı, `Audio Profiler` "Audio Voices" sayacı | operatör |
| `Streaming` yükleme tipi (efektler için) | `mem` | Tek bir klip 200 KB sabit gideri aştığında, yani ~4,5 saniyeyi geçtiğinde | formül, sonra profil kanıtı | operatör |
| `Compressed In Memory` | `mem` | `Decompress On Load` toplamı 20 MiB'ı aştığında | profil kanıtı, `Audio Profiler` "Total Audio Memory" | operatör |
| Singleton (müzik taşıyıcısı olarak) | — | Bir çağıran için serileştirilmiş alan, kurucu parametresi ve olay aboneliği yollarının ÜÇÜ birden denenip başarısız olduğunda | derleme kanıtı | operatör |
| `AudioClip[]` + `Random.Range` (rastgele varyasyon) | `io` | Aynı sesin arka arkaya tekrarı rahatsız edici bulunduğunda; motorun `Audio Random Container` cevabı önce denenir | insan gözlemi | operatör |

### 6.5 Bu dosyanın verdiği tek yokluk hükmü

Yukarıdaki tabloların hepsi ERTELEME yazıyor. Bir tanesi ertelemiyor ve bir
özellik borcu doğuruyor.

**Bir reddin işitsel karşılığı bu projede YOK, ve bu bir eksik değil bir borçtur.**

> **HANGİ ÖZELLİK:** Oyuncu yasak bir hamle yaptığında, hamlenin yasak olduğunu
> ekrana bakmadan anlasın. Bugün tıklıyor, hiçbir şey olmuyor, ve tıklamasının
> kaydedilip reddedildiğini mi yoksa hiç kaydedilmediğini mi bilmiyor.
> **NEREYE BAĞLANIR:** `Assets/Game/Core/MoveOutcome.cs` → `RejectedCellOccupied`
> **NE KIRAR:** Hiçbir mevcut kararı kırmaz ve sebebi ölçülü. Sesi çalacak kod
> `GridStrategy.Unity` içinde yaşar, kural katmanı `noEngineReferences: true`
> taşıdığı için oraya bakamaz bile, ve sonuç değerleri zaten o sınırı geçiyor.
> Kırdığı tek şey manifestin bugünkü kapsamıdır: `Assets/Art/THIRD_PARTY_ASSETS.md`
> kendini "the 32 PNG files" ile sınırlıyor ve ilk ses dosyası o cümleyi
> yanlış kılar.
> **KARARMETRE:** Evet. Bir oyuncu reddedildiğini anlamak ister ve bunu
> `AudioSource` diye bir tip hiç var olmasaydı da isterdi. Ses o isteğin
> mekanizması değil, taşıyıcısıdır; ve bu tahtada taşıyıcı olarak seçilmesinin
> ölçülü bir sebebi var. Ret gerekçelerinin bir kısmı EYLEYENE ait
> (`RejectedActorCannotAct`), bir kısmı HEDEFE ait (`RejectedOutOfRange`), yani
> çizilecek tek bir yer yok. Sesin yeri yoktur, bu yüzden yer sorunu da yoktur.
> **ARAŞTIRMA BORCU:** Var. `performance-research` sorusu şudur: Kenney
> `Interface Sounds` arşivi indirilip SHA-256'sı hesaplandığında ve arşiv içi
> `License.txt` okunduğunda, paketin dosya biçimi nedir ve hangi ses "ret"
> rolüne uyar. Bu soru bu şeritte cevaplanamadı çünkü şeridin yazma kapsamı
> indirmeyi içermiyor.
> **NASIL DOĞURULUR:** Kaynak basamağı **4**, yani yeni paket. Kenney
> `Interface Sounds` indirilir ve tek bir dosya alınır. Doğan dosya
> `Assets/Audio/ThirdParty/Kenney/InterfaceSounds/` altına konur ve
> `Assets/Art/THIRD_PARTY_ASSETS.md` bir ses bölümü kazanır. Açılan kod dosyası
> `Assets/Game/Unity/BoardAdapter.cs` değil, yeni bir `BoardAudioView` olmalıdır;
> sebebi `BoardAdapter`'ın zaten 27'den fazla serileştirilmiş alan taşıması ve
> sesin ayrı bir görünüm sorumluluğu olması. Numaralı editör adımı: Unity'de
> `Hierarchy`'de `Board` seçilir ve Inspector'daki `Reject Clip` alanına
> indirilen klip sürüklenir. Kaba süre 2-3 saat ve büyük kısmı lisans okuma ile
> içeri alma ayarlarıdır, kod değil.

---

## 7 · Göremedikleri

Bu belgenin kapatamadığı sınırlar. Hepsi bilinerek açık bırakıldı.

**① Hiçbir ses dinlenmedi.** Bu dosya bir ses dosyası indirmedi, açmadı ve
çalmadı. "Kenney `Impact Sounds` bu oyunun vuruşuna uyar" cümlesi bir ETİKET
okumasıdır, bir dinleme değil. Görsel tarafta aynı sınırın adı `14-gorsel-sozluk-ve-eksikler.md`'de
yazılı ve orada bir kapı bile PNG açmıyor; burada durum daha kötüdür, çünkü bir
sesin uygunluğu piksel sayarak da anlaşılmaz.

**② Hiçbir profil koşulmadı.** §4'ün bütün `HEDEF` sayıları formülden türetildi.
`Audio Profiler` modülünün "Total Audio CPU" ve "Total Audio Memory" sayaçları
bu projede hiç okunmadı ve okunacak bir şey de yoktu. Formül bir profil değildir.

**③ Hedef platform bilinmiyor.** Sıkıştırma biçimi kararının yarısı platforma
bağlıdır ve bu projede bir hedef platform yazılı değil. §3.2'nin platform
satırının `DOĞRULANMADI` kalmasının sebebi budur ve sebep bu dosyanın dışındadır.

**④ Müzik tarafı araştırılmadı.** Kenney'in `Music Jingles` paketi listede
görüldü, sayfası açılmadı, lisansı okunmadı. Zafer ve yenilgi ezgileri için tek
bir aday bile adıyla yazılamadı.

**⑤ İkinci sahne bir varsayımdır.** §2'nin bütün ② ③ ④ soruları "ikinci sahne
doğduğunda" diye başlıyor ve bugün ikinci sahne yok. `DontDestroyOnLoad`'ın
ikinci kopya ürettiği ölçülmedi, okundu. Additive boot sahnesinin işe yaradığı
ölçülmedi, türetildi. Hüküm (Singleton kolaylıktır) bugünkü tek sahne için
KESİNDİR; iki sahne için TÜRETİLMİŞTİR.

**⑥ Ses erişilebilirliği hiç düşünülmedi.** İşitme engelli bir oyuncu için ses
bir çözüm değildir ve bu dosya §6.5'te ret geri bildirimini sese bağlarken o
oyuncuyu hiç saymadı. Doğru şekil büyük olasılıkla ses ARTI görsel bir işarettir
ve bu dosya yalnız birinci yarısını yazdı.

**⑦ Bir kapı bu dosyanın hiçbir sayısını denetlemiyor.** `check-doc-links.py`
bağlantıları çözüyor, `check-absence-debt.py` §6.5'in beş alanının dolu olduğunu
görüyor. Hiçbiri "30 ses gerçekten 32'nin altında mı" diye sormuyor, çünkü o
soru bir aritmetiktir ve aritmetiği yazan da denetleyen de aynı kişidir.

---

## İlgili

- Ekranın ne dediğini sayan dosya: [14-gorsel-sozluk-ve-eksikler.md](14-gorsel-sozluk-ve-eksikler.md)
- Singleton'ın beş alanlı reddi: [13-desen-secim-rehberi.md](13-desen-secim-rehberi.md)
- Bugün ne yok, ne zaman gelir: [02-sonraki-asamalar.md](02-sonraki-asamalar.md)
- Hangi kavramın sahibi var: [03-kavram-borc-defteri.md](03-kavram-borc-defteri.md)
- Bu ağacın yönlendirmesi: [README.md](README.md)
