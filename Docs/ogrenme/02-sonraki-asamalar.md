# Sonraki aşamalar — bugün ne yok, ne zaman gelir

██ Bu dosya **hiçbir şey önermez**. ██ İçinde "şunu şimdi ekleyelim" cümlesi
yoktur ve olmamalıdır. Her bölüm bir **tetikleyici koşul** yazar; o koşul
gerçekleşene kadar doğru cevap "bugün yok" olarak kalır.

Sebebi tek cümle: **"bugün önemli değil" eksik bir cümledir.** Onu tamamlayan
şey, önemli hâle getirecek koşuldur — ve o koşul aynı cümlede yazılmazsa
"bugün önemli değil", bir yıl sonra "hiç öğrenmedim"e dönüşür.

## Her bölümün beş alanı

| Alan | Ne yazar |
|---|---|
| **A · BUGÜNKÜ KARŞILIĞI** | Bu işi bugün projede ne yapıyor. `dosya:satır` — doğrulanmış |
| **B · TETİKLEYİCİ KOŞUL** | Hangi somut olay bunu gerekli kılar. Sayı ya da ölçü ile |
| **C · İLK ADIM** | O gün geldiğinde değişecek **ilk** dosya |
| **D · NE KIRAR** | Bugün erken getirilirse ne bozulur |
| **E · ÖN KOŞUL** | Hangi kavram önce kapanmalı |

Her bölümün sonunda bir **ÜÇ OYUN** satırı var: Slay the Spire · Vampire
Survivors · Stardew Valley. Eşleşmeyen satır `██ EŞLEŞMEZ ██` ile işaretli.

---

## Aşama 1 · ScriptableObject

**ScriptableObject nedir:** Unity'nin, sahneye ait olmayan ama projede bir
**dosya** (asset) olarak yaşayan veri tipi. Bir `MonoBehaviour` bir sahne
nesnesine bağlıdır; bir `ScriptableObject` diskte tek başına durur ve onu
kullanan herkes **aynı** dosyayı gösterir.

**A · BUGÜNKÜ KARŞILIĞI** — Tanımların rolü zaten ayrılmış durumda, yalnız
tipleri düz C# sınıfı:

```
Assets/Game/Core/Combat/AttackProfile.cs:40   sealed class · Damage :68 · Range :74
Assets/Game/Core/MoveProfile.cs:42            sealed class · Range :64
```

Sayılar bugün Unity katmanında, `[SerializeField]` alanları olarak yaşıyor:

```
Assets/Game/Unity/BoardAdapter.cs:135   maxHealth = 30
Assets/Game/Unity/BoardAdapter.cs:138   damage = 10
Assets/Game/Unity/BoardAdapter.cs:141   attackRange = 1
Assets/Game/Unity/BoardAdapter.cs:150   moveRange = 1
Assets/Game/Unity/BoardAdapter.cs:178   structureMaxHealth = 50
```

ve tanım nesneleri her doğuşta yeniden kuruluyor
(`BoardAdapter.cs:756-760`, `BoardAdapter.cs:554`). Yani bugün "tasarımcı dosyası" diye bir
şey yok; tek yazma yolu Inspector'daki bir bileşendir.

██ Kararın kendisi kodda zaten adı konmuş ██ — `AttackProfile.cs:13-14`:
"bugün düz C# nesnesi; ScriptableObject kararı geldiğinde bu satır değişir,
rol değişmez." Bu, bekleyen bir borcun **yazılı** hâli.

**B · TETİKLEYİCİ KOŞUL** — Üçünden **biri**:

- Kod derlemeden değer değiştirmek gerektiğinde. Bugün `damage`'ı 10'dan 12'ye
  çekmek Inspector'dan mümkün, ama **birim türü başına** farklı değer vermek
  değil: `NewCombatant` (`BoardAdapter.cs:749`) her birime **aynı** üç sayıyı
  veriyor. İkinci bir birim türü doğduğu gün bu kırılır.
- Aynı tanımı **iki sahnenin** paylaşması gerektiğinde. Bugün tek sahne var;
  ikinci sahne, beş `[SerializeField]` alanının ikinci kopyasını doğurur.
- Tanım sayısı elle yönetilemez olduğunda — ölçü: **beşten fazla** birim türü.
  Beş türde Inspector hâlâ okunur; on beşte bir `.asset` klasörü kaçınılmaz.

**C · İLK ADIM** — Değişecek ilk dosya `Assets/Game/Unity/BoardAdapter.cs`
**değil**, `Assets/Game/Core/Combat/GridStrategy.Combat.asmdef`'tir. Sebebi
somut: `ScriptableObject`, `UnityEngine`'de yaşıyor ve o asmdef bugün
`noEngineReferences: true` taşıyor. `AttackProfile` `ScriptableObject`'ten
türediği an bu satır düşer.

Bu yüzden ilk adımın şekli bir tip değişikliği değil, bir **sınır kararıdır**:
tanım motora bağlanacak mı, yoksa motor tarafında bir `ScriptableObject`
sarmalayıcı doğup çekirdeğe düz C# tanımını mı **üretecek**. İkinci yol duvarı
ayakta tutar; birincisi yıkar.

**D · NE KIRAR** — İki şey, ve ikisi de kodda ölçülmüş:

① **Doğrulama kaybolur.** `AttackProfile.cs:42-47` bunu adıyla yazmış: tip
`ScriptableObject`'ten türeseydi doğrulama `OnValidate`'e kayar, **yalnızca
Inspector'da** çalışır ve koddan üretilen profil hiç sınanmazdı. Bugün
`AttackProfile.cs:59-62`'deki `range < 1` kelepçesi profil hangi yoldan gelirse
gelsin geçerli — kod, test, gelecekteki bir yükleyici.

② **██ EN SIK HATA ██ Bir varlık, çalışma zamanı durumu taşımaya başlar.**
`ScriptableObject` bir **dosyadır**: onu gösteren yüzlerce birim **aynı** nesneyi
gösterir. Örneğe özel değişen bir alan (kalan bekleme süresi, mevcut can, bu
turda kaç kez vurdu) o dosyanın içine girerse **bütün** birimler aynı değeri
paylaşır — ve daha kötüsü, değer Editor'de **kalıcı** olur: oyunu kapatıp
açtığında son savaşın canı hâlâ oradadır.

Bu projede o sınır bugün **doğru** çizilmiş durumda ve ölçüsü var: can
`Health.cs:29`'da, hâl `UnitLifecycle.cs:82`'de, taraf `Combatant.cs:145`'de —
hiçbiri tanımın içinde değil. `AttackProfile.cs:25-26` de aynı çizgiyi yazıyor:
"Neyi TUTMAZ: kimin saldırdığını, kime saldırıldığını, o anki bekleme süresini."

**E · ÖN KOŞUL** — İki kavram önce kapanmalı:
① **Serileştirme** — Unity'nin bir alanı ne zaman kaydettiği, `[SerializeField]`
ile `public`in farkı, `.meta` dosyasının taşıdığı GUID kimliği.
`HENÜZ YOK → bu ağaç dışında bir sahip; bkz. 03-kavram-borc-defteri.md`.
② **Değer ve referans ayrımı** — paylaşılan bir referansı değiştirmenin
paylaşan herkesi etkilemesi. Bu kavram **kapalı**:
[../deep/dil/05-deger-referans-ve-kimlik.md](../deep/dil/05-deger-referans-ve-kimlik.md).

**ÜÇ OYUN** — Slay the Spire: her kartın adı, maliyeti ve metni bir yerde
tanımlıdır ve destedeki iki kopya aynı tanımı okur · Vampire Survivors: her
silahın seviye tablosu önceden yazılmıştır, oyun sırasında yazılmaz ·
Stardew Valley: her tohumun büyüme günleri ve mevsimi sabit bir tablodan gelir,
tarladaki her bitki o tabloya bakar.

---

## Aşama 2 · Nesne havuzu (object pool)

**Nesne havuzu nedir:** Yok edilecek nesneyi silmek yerine kenara koyup, yeni
biri gerektiğinde **onu geri vermek**. Kazancı yaratma/yok etme maliyetini ve
çöp toplayıcı baskısını azaltmak; bedeli, geri verilen nesnenin eski durumunu
**sıfırlama sözleşmesi**.

**A · BUGÜNKÜ KARŞILIĞI** — Doğrudan yaratma ve doğrudan yok etme:

```
Assets/Game/Unity/BoardAdapter.cs:739   Instantiate(unitPrefab, transform)   ← birim görseli
Assets/Game/Unity/BoardAdapter.cs:1007   Destroy(view.gameObject)             ← temizlik
Assets/Game/Unity/BoardAdapter.cs:667   new GameObject($"Cell_{x}_{y}")      ← zemin, Awake'te
Assets/Game/Unity/BoardAdapter.cs:568   new GameObject($"Structure_{x}_{y}") ← yapı görseli
```

██ ÖLÇÜ ██ — `Instantiate`'in tek çağıranı `SpawnUnit` (`BoardAdapter.cs:720`), onun da tek
çağıranları `Awake` içindeki iki satır (`:267`, `:268`). Yani **kare başına
sıfır** birim doğuyor. `Destroy` yalnızca ceset süresi dolduğunda çalışıyor
(`AdvanceBattleTime :625` → `DespawnView :983`). Bugün havuzun azaltacağı bir
maliyet **ölçülebilir değil**, çünkü maliyet yok.

Buna karşılık projede tahsis bilinci **zaten var** ve havuzsuz uygulanmış:

```
Assets/Game/Unity/BoardAdapter.cs:210   cleanupBuffer — her karede yeni List kurmamak için alan
Assets/Game/Battle/Battle.cs:429        RemoveReadyForCleanup(List<Unit>) — tamponu ÖNCE temizler
Assets/Game/Battle/Battle.cs:383        Dictionary üzerinde DOĞRUDAN foreach — kutulama yok
Assets/Game/Battle/TurnState.cs:64      orderView — salt okunur görünüm bir KEZ kuruluyor
```

`Battle.cs:373-376` bunun gerekçesini yazıyor: küme `IEnumerable` olarak dışarı
açılsaydı numaralandırıcı arayüz ardında **kutulanır** ve her `Update` bir
tahsis yapardı.

**B · TETİKLEYİCİ KOŞUL** — Kare başına tahsis **ölçülebilir** hâle geldiğinde.
Ölçünün adı ve aracı bugün projede mevcut: Unity Test Framework'ün
`AllocatingGCMemory` kısıtı
(`Assets/Tests/EditMode/Combat/DamageRulesAllocationTests.cs:103`). Somut eşik:

- Sürekli doğan/ölen bir nesne sınıfı ortaya çıktığında — mermi, hasar sayısı
  yazısı, vuruş efekti, liste satırı. Bugün böyle bir sınıf **yok**.
- `Instantiate`/`Destroy` çifti `Update` yolundan çağrılmaya başladığında.
  Bugün ikisi de o yolda değil.

██ Ölçüsüz havuz erken iyileştirmedir ██ — ve bedeli ödenir: havuz, sıfırlama
sözleşmesi olmadan **yanlış** çalışır ve o sözleşme yalnızca bir liste değil,
bir bakım borcudur (konum ve ebeveyn, hız ve fizik, can, sayaç ve sahiplik,
parçacık ve animasyon, olay abonelikleri, çift bırakma, yabancı nesne reddi,
kapasite ve sahne boşaltma).

**C · İLK ADIM** — Değişecek ilk dosya `Assets/Game/Unity/BoardAdapter.cs`,
ve tek bir satır: `BoardAdapter.cs:739`'daki `Instantiate` bir `Get()` çağrısına döner.
Ama **ikinci** satır asıl karardır: `BoardAdapter.cs:1007`'deki `Destroy`, `Release(view)`'a
dönerken görsel durumun sıfırlanması gerekir — ve bu projede sıfırlanacak durum
`UnitView`'da yazılı: `UnitView.cs:93` (`SetState(UnitState.Alive)`) ve
`UnitView.cs:107` (`SetSelected(false)`). Yani sıfırlama sözleşmesinin metni
bugün **`Awake` içinde** duruyor; havuz geldiği gün o metnin `Awake`'ten çıkıp
çağrılabilir bir metoda taşınması gerekir, çünkü havuzdan çıkan nesnede `Awake`
**tekrar çalışmaz**.

**D · NE KIRAR** — Üç şey:

① **`Awake` bir daha çalışmaz.** Havuzdan dönen nesnede `Awake` ve `OnDestroy`
tetiklenmez; `OnEnable`/`OnDisable` tetiklenir. `UnitView.cs:86`'daki `Awake`
bugün doğan her birimi ayakta ve seçimsiz başlatıyor — havuz o garantiyi
sessizce kaldırır ve önceki birimin gri tonu yeni birimde görünür.

② **Ölçüm penceresi kayar.** Havuz, ilk doldurma anında **büyük** bir tahsis
yapar. O tahsis "kare başına sıfır" iddiasını bozmaz ama ölçümü yapan kişi
bunu bilmezse yanlış yerde arar.

③ **Bugünkü sadelik gider.** `unitViews` sözlüğü (`BoardAdapter.cs:199`) bugün
"tabloda varsa ekranda var" demek. Havuzla birlikte üçüncü bir hâl doğar:
"nesne yaşıyor ama havuzda bekliyor" — ve `TryGetView`'ın (`:1065`) `LogError`
kararı (`:1072`) o gün yanlış alarm üretmeye başlar.

**E · ÖN KOŞUL** — İki kavram önce kapanmalı: **profil çıkarma kanıt sınırı**
(Aşama 6 — ölçüsüz havuz kurulamaz) ve **Unity mesaj geri çağrıları**
(`Awake`/`OnEnable`/`OnDisable`/`OnDestroy` hangi olayda tetiklenir).
İkincisi kapandı:
[../deep/konular/08-motor-cagri-dongusu.md](../deep/konular/08-motor-cagri-dongusu.md)
— `Awake`'in bir `event` **olmadığı** (birinci durak) ve çağrı sırasının
sahipleriyle birlikte (ikinci durak) orada yazılı.

**ÜÇ OYUN** — Slay the Spire: ██ EŞLEŞMEZ ██ orada kare başına doğan/ölen nesne
akışı yok; savaş sıra tabanlı ve ekrandaki eleman sayısı onlarla sınırlı ·
Vampire Survivors: ekranda aynı anda yüzlerce düşman ve mermi doğup ölür, bu
akış oyunun tanımıdır · Stardew Valley: balık tutarken, ağaç keserken ve
madende her vuruşta parçacık ve eşya damlası doğar.

---

## Aşama 3 · Olay veri yolu (event bus)

**Olay veri yolu nedir:** Yayıncı ile aboneyi birbirine hiç bağlamadan,
ortadaki bir dağıtıcı üzerinden haber geçirmek. Yayıncı "kim dinliyor"
bilmez, abone "kim yayınladı" bilmez.

**A · BUGÜNKÜ KARŞILIĞI** — Doğrudan bağlı, dört duraklı bir olay zinciri:

```
① Assets/Game/Core/Combat/UnitLifecycle.cs:80   event Action<UnitState>
② Assets/Game/Core/Combat/Combatant.cs:111      event Action<UnitState, UnitState>
   çevirici :114 · abonelik :86
③ Assets/Game/Battle/Battle.cs:179              event Action<Unit, UnitState, UnitState>
   kapanış üretimi :219 · abonelik :221
   yönlendirici sözlüğü :74 · sökme :336-340
④ Assets/Game/Unity/BoardAdapter.cs:288/:282    += / -=  (OnEnable / OnDisable)
   dinleyici :299
```

Her durakta bilgi **kazanılıyor**: ① yeni durum, ② önceki durum, ③ kimlik,
④ ekran. Zincirin tamamı izlenebilir — bir hata olduğunda dört dosya açılır ve
yol görünür. Hikâyesi:
[../deep/konular/01-olay-zinciri.md](../deep/konular/01-olay-zinciri.md).

`Battle.cs:81`'teki `stateForwarders` sözlüğü bu zincirin faturasıdır: abone
edilen şey birim başına ayrı bir **kapanış** olduğu için, sökmek üzere tam o
örneğin saklanması gerekiyor.

**B · TETİKLEYİCİ KOŞUL** — Birbirini **tanımaması gereken üçüncü** bir
dinleyici çıktığında. Bugün tek dinleyici var (`BoardAdapter`); ikincisi hâlâ
düz bir `+=` ile eklenebilir. Somut eşik:

- Aynı olguya (`birim düştü`) ses, skor, görev ilerlemesi ve istatistik **ayrı
  ayrı** abone olduğunda — dört dinleyici, dördü de birbirini bilmemeli.
- Yayıncı sayısı **birden fazla** olduğunda. Bugün `UnitStateChanged`'in tek
  yayıncısı var (`Battle`).

██ Şu üç durumda veri yolu **yanlış** cevaptır ██ ve ölçüsü bu projede
mevcut: (a) çağıran bir **değer** istiyorsa — `TurnRules.CanAct`
(`Assets/Game/Battle/TurnRules.cs:59`) bir sorudur, bir duyuru değil;
(b) eylem doğrulanmalı, sıraya alınmalı ya da geri alınmalıysa; (c) tek
yayıncının yerel dinleyicileri varsa — düz `event` daha keşfedilebilirdir.

**C · İLK ADIM** — Değişecek ilk dosya `Assets/Game/Battle/Battle.cs`, ve
`:179`'daki `UnitStateChanged` olayı. Ama asıl ilk adım bir **kapsam
kararıdır**: veri yolu hangi alanla sınırlı (savaş mı, bütün oyun mu), kim
kuruyor, kim söküyor, yükün sahibi kim. Kapsamsız bir yol, tek bir global
`GameEvents` sınıfına dönüşür ve o gün olay sahipliği, yük tipi ve dinleyici
listesi keşfedilemez hâle gelir.

**D · NE KIRAR** — ██ Çağrı zinciri **görünmez** olur. ██ Bugün "ekrandaki
gri ton nereden geldi" sorusunun cevabı dört dosya açılarak bulunur ve
[../deep/konular/01-olay-zinciri.md](../deep/konular/01-olay-zinciri.md) bu yolu
hikâye olarak anlatabiliyor. Veri yolu geldiğinde yayıncı ile abone arasında
**hiçbir statik bağ kalmaz**: derleyici "buradan oraya" diye bir ok çizemez,
"Find All References" boş döner ve yolu ancak çalışma zamanında izleyebilirsin.

İkinci kırılma daha sinsi: **abonelik ömrü**. Bugün abonelik `OnEnable`/
`OnDisable` çiftinde (`BoardAdapter.cs:288`/`BoardAdapter.cs:293`) ve simetriyi **disiplin**
tutuyor — eksik bir `-=` tek bir uyarı bile üretmez (`:282-283`). Veri yolu bu
sorunu çözmez, **çoğaltır**: ölü bir abone artık bir nesnede değil, merkezî
bir listede yaşar ve sahne yeniden yüklendiğinde de orada kalır.

**E · ÖN KOŞUL** — **Delege, olay ve kapanış kimliği** kavramı kapalı olmalı;
kapalıdır:
[../deep/dil/04-delege-olay-ve-kapanis.md](../deep/dil/04-delege-olay-ve-kapanis.md).
İkinci ön koşul **Unity mesaj geri çağrıları** (abonelik hangi geri çağrıda
kurulur ve hangisinde sökülür); o da kapalı:
[../deep/konular/08-motor-cagri-dongusu.md](../deep/konular/08-motor-cagri-dongusu.md).
Delegenin arka tarafı — abonelik neden kurucunun EN SONUNDA — ayrıca
[../deep/dil/06-delege-arka-taraf.md](../deep/dil/06-delege-arka-taraf.md)'da.

**ÜÇ OYUN** — Slay the Spire: bir düşman öldüğünde reçeteler, kalıntılar ve
savaş bitiş kontrolü aynı olguya ayrı ayrı tepki verir · Vampire Survivors:
bir seviye atlandığında yükseltme ekranı, oyun duraklatma ve müzik ayrı ayrı
tepki verir · Stardew Valley: gün bittiğinde ekinler, hayvanlar, kasaba
etkinlikleri ve mektup kutusu aynı duyuruyu dinler.

---

## Aşama 4 · Singleton — ve reddedilişin kendisi

**Singleton nedir:** Bir tipin tek bir örneğinin olmasını **ve** o örneğe
her yerden erişilebilmesini garanti eden kalıp; Unity'de genellikle
`static Instance` alanı olarak yazılır.

██ Bu proje singleton'ı **bilerek kullanmıyor**, ve reddedilişin kendisi
dersin ta kendisi. ██

**A · BUGÜNKÜ KARŞILIĞI** — Bağımlılıklar üç yoldan geliyor ve üçü de görünür:

```
kurucudan          Assets/Game/Core/Combat/Combatant.cs:59   Health, UnitLifecycle, AttackProfile, Team
                   Assets/Game/Core/Combat/Structure.cs:51
                   Assets/Game/Core/PointerGesture.cs:127    eşik dışarıdan
Inspector'dan      Assets/Game/Unity/BoardAdapter.cs:124     unitPrefab
                   Assets/Game/Unity/UnitView.cs:51          selectionOverlay
sahibinden         Assets/Game/Battle/Battle.cs:53           tahtayı kendisi kurar, dışarıdan ALMAZ
```

██ ÖLÇÜLDÜ ██ — `Assets/Game/` altında **değiştirilebilir hiçbir `static`
alan yok**. Tek `static` alan `Assets/Game/Battle/TurnState.cs:44`
(`DefaultTurnOrder`) ve o da `readonly` **ve** `Array.AsReadOnly` ile sarılmış
bir salt okunur görünüm. `Instance`, `DontDestroyOnLoad` ve `FindObjectOfType`
kelimeleri üretim kodunda **hiç geçmiyor**.

Ret gerekçesi de kodda yazılı. `Battle.cs:143` sıra durumu için şunu diyor:
"static bir alana konsaydı durum test metotları arasında **sızardı**."
`BoardAdapter.cs:69-71` aynı ölçüyü tersinden veriyor: aynı sahneye iki
`BoardAdapter` koy, **iki ayrı savaş** doğar — paylaşılan tek bir `static` alan
yok.

**B · TETİKLEYİCİ KOŞUL** — Üç bağımsız kararın **üçü de** aynı anda
gerektiğinde. Bunlar ayrı kararlardır ve karıştırılmaları en sık hatadır:
① tek örneklilik gerçekten bir alan değişmezi mi (bir savaşta bir tahta —
evet; ama bu tekliği `Battle` **sahiplenerek** zaten sağlıyor);
② sahne yüklemeleri arasında yaşaması gerekiyor mu (bugün tek sahne var,
yani soru **sorulmuyor** bile);
③ her yerden **global erişim** gerekiyor mu — bu üçüncüsü neredeyse hiç
gerekmez ve genellikle "yazmaktan kaçınma" isteğidir.

Somut eşik: bir müzik çalar ya da kayıt sistemi geldiğinde ① ve ② doğru olur,
③ hâlâ yanlıştır. Yani o gün bile doğru cevap `static Instance` değil, sahne
başına açıkça bağlanan bir servistir.

**C · İLK ADIM** — Değişecek ilk dosya bir tip **değil**, bir yer: kurulum
kökü. Bugün o kök `Assets/Game/Unity/BoardAdapter.cs:232`'teki `Awake` — savaşı
kuran (`BoardAdapter.cs:238`), jesti kuran (`:243`), zemini kuran (`:259`) ve iki demo birimi
doğuran (`BoardAdapter.cs:267-268`) satırlar orada. Sahne ötesi bir bağımlılık geldiği gün ilk
soru "nereye `static` koyayım" değil, "bu kök kimin" olur.

**D · NE KIRAR** — İki şey, ikisi de ölçülebilir:

① **Test yalıtımı.** Bugün `Assets/Tests/EditMode/` altındaki 26 test dosyası
kendi nesnesini kurup atıyor; iki test birbirinin durumunu göremez. Bir
`static Instance` ilk testte doldurulur ve ikinci testte **hâlâ doludur** —
testler ayrı ayrı yeşil, birlikte kırmızı olur ve sıra bağımlı bir hata doğar.
Bugün bu riskin sıfır olduğunun ölçüsü yukarıda: değiştirilebilir `static` alan
yok.

② **Domain Reload'da hayatta kalan durum.** Unity, Play'e basıldığında
varsayılan olarak `static` alanları sıfırlar (Domain Reload). Ama bu davranış
Editor ayarlarından **kapatılabilir** (Enter Play Mode Options) — ve kapatıldığı
an `static` alanlar oturumlar arasında yaşamaya devam eder. O gün "ilk Play
çalışıyor, ikincisi bozuk" diye bir hata doğar ve sebebi kodda **görünmez**.
Bu projede bugün riskin sıfır olması bir tercih değil, `static` alan yokluğunun
sonucu.

Bellek, canlılık ve yıkım tarafının tamamı:
[../deep/dil/07-bellek-canlilik-ve-yikim.md](../deep/dil/07-bellek-canlilik-ve-yikim.md).
Motorun geri çağrı döngüsü ve Domain Reload tarafı:
[../deep/konular/08-motor-cagri-dongusu.md](../deep/konular/08-motor-cagri-dongusu.md).

**E · ÖN KOŞUL** — **Nesne ömrü ve yıkım** kavramı (bir referans ne zaman
serbest kalır, Unity'nin `Object`'i neden "null gibi ama null değil" olabilir)
ve **Unity mesaj geri çağrıları**. İkisinin de sahibi artık var; ikisi de
yukarıdaki iki işaretçide.

Bu projede bir işaretçi zaten var: `BoardAdapter.cs:999-1001` "önce tablodan
çıkar, sonra sahneden sil" kararını, Unity'nin aşırı yüklenmiş eşitliği
yüzünden "null gibi ama null değil" hâlinde dolaşan referansla gerekçelendiriyor.

**ÜÇ OYUN** — Slay the Spire: koşu boyunca tek bir deste, tek bir altın sayacı
ve tek bir tırmanış durumu vardır; ikisi aynı anda olamaz · Vampire Survivors:
tek bir sahne süresi sayacı bütün oyunu yönetir · Stardew Valley: tek bir takvim
ve tek bir saat vardır, bütün sistemler ona bakar.

---

## Aşama 5 · ECS / DOTS

**ECS nedir:** Entity Component System — varlık (yalnız bir kimlik), bileşen
(yalnız veri), sistem (yalnız davranış) üçlüsü. Veriyi davranıştan ayırır ve
aynı türden bileşenleri bellekte **bitişik** tutarak işlemcinin önbelleğini
verimli kullanmayı hedefler. **DOTS**, Unity'nin bunu içeren paket ailesi
(Entities, Burst derleyici, Job System).

██ Dürüst cümle: bu proje ECS'e **yakın bir şekle** sahip ama ECS **değil**. ██

**A · BUGÜNKÜ KARŞILIĞI** — Üçlünün üç parçasının da bir karşılığı var:

```
varlık gibi   Assets/Game/Core/Unit.cs:41       tek üye Name :56 · başka hiçbir şey yok
bileşen gibi  Assets/Game/Battle/Battle.cs:59   Dictionary<Unit, Combatant>
              Assets/Game/Battle/Battle.cs:66   Dictionary<Unit, Structure>
              Assets/Game/Unity/BoardAdapter.cs:199  Dictionary<Unit, UnitView>
sistem gibi   Assets/Game/Core/Combat/TargetingRules.cs:31   durumsuz, girdiyi alır cevabı verir
              Assets/Game/Core/Combat/DamageRules.cs:24
              Assets/Game/Battle/TurnRules.cs:28
döngü gibi    Assets/Game/Battle/Battle.cs:377  Tick — bütün savaşçıları dolaşır
```

Veri ile davranış **zaten ayrılmış** durumda: kural tiplerinin hiçbirinde alan
yok, varlıkların hiçbirinde kural yok. Bu, ECS'in en çok konuşulan
kazanımlarından birinin bu projede **desen olmadan** elde edilmiş olması demek.

**FARK NEREDE** — Dört şey ve dördü de ECS'in **tanımı**:

| ECS'in istediği | Bu projede olan |
|---|---|
| Varlık bir **sayı** (indeks + sürüm), nesne değil | `Unit` bir `sealed class`, kimlik referans eşitliğinden gelir (`Unit.cs:51-55`) |
| Bileşenler **bitişik dizilerde** tutulur | Bileşenler `Dictionary` içinde — dağınık, düğüm başına ayrı ayrılmış |
| Bir **sistem döngüsü** bileşen kümesine göre iş dağıtır | Döngü elle yazılmış tek bir `foreach` (`Battle.cs:383`, `:383`) ve neyi dolaşacağını sabit biliyor |
| Bellek yerleşimi (chunk) performans için tasarlanır | Bellek yerleşimi hiç düşünülmedi; `Dictionary` düğümlerinin yeri çalışma zamanının kararı |

`Battle.cs:379-382` bu tabloya kısmen dokunuyor: sözlük üzerinde **doğrudan**
`foreach` kullanılıyor çünkü `Dictionary<,>.Enumerator` bir `struct` ve arayüz
ardında saklanmadığı için kutulanmıyor. Yani tahsis bilinci var, **bellek
yerleşimi bilinci** yok — ve ikisi ayrı şeyler.

**B · TETİKLEYİCİ KOŞUL** — Varlık sayısı **ve** kare başına iş, ölçülmüş bir
darboğaz hâline geldiğinde.

██ Ve bu eşik sıra tabanlı bir tahta oyununda **çok yüksektir** ██ — bu cümle
yumuşatma değil, ölçü. Bugünkü tahta `3×5`, yani **15 hücre**, ve tahtadaki
parça sayısı iki:

```
BoardAdapter.cs:113   [SerializeField, Min(1)] private int width = 3;
BoardAdapter.cs:114   [SerializeField, Min(1)] private int height = 5;
BoardAdapter.cs:267   SpawnUnit("Vanguard", Team.Player, 1, 2);
BoardAdapter.cs:268   SpawnUnit("Raider", Team.Enemy, 1, 3);
```

`Battle.Tick` (`Battle.cs:377`) kare başına iki `Combatant`
ve sıfır `Structure` dolaşıyor. ECS'in kazandığı şey, on binlerce varlığın kare
başına aynı işi yapması durumunda ortaya çıkar. Sıra tabanlı bir oyunda kare
başına yapılan iş genellikle **sıfırdır**: hiçbir şey olmayan bir karede
`Tick`'in yaptığı tek şey iki sayacı azaltmak.

Somut eşik: kare başına dolaşılan varlık sayısı **binleri** bulduğunda **ve**
o dolaşma profilde görüldüğünde. Ölçüm yapılmadan bu eşiğe "yaklaşıldı"
denemez.

**C · İLK ADIM** — Değişecek ilk dosya `Assets/Game/Battle/Battle.cs`. Sebebi
`Unit` değil: `Unit` zaten neredeyse bir kimlikten ibaret. Asıl değişecek şey
**depo**: `:59`, `:66` ve `:81`'deki üç `Dictionary`, indeksle erişilen bitişik
dizilere döner ve `Unit` bir nesne olmaktan çıkıp bir indekse dönüşür. O gün
`Unit.cs:51-55`'te yazılı olan "anahtar referans eşitliğidir" cümlesi geçersiz
olur.

██ Ama gerçek ilk adım kod değil ölçüdür ██ — Aşama 6.

**D · NE KIRAR** — İki şey, ikisi de büyük:

① **Bugünkü okunabilir nesne modeli.** `Combatant.cs:152`'deki
`public UnitState State => lifecycle.State;` gibi satırlar kaybolur; yerlerine
bileşen aramaları gelir. `Docs/deep/kod/` ağacındaki 33 ayna belgenin tamamı
tip başına bölünmüş durumda ve o bölünme ECS'te karşılıksız kalır.

② **Bütün EditMode test yüzeyi.** `Assets/Tests/EditMode/` altında 26 dosya
var ve neredeyse hepsi düz `new` ile nesne kurup davranış sınıyor — örneğin
`Assets/Tests/EditMode/Combat/UnitLifecycleTests.cs`. ECS'te nesne kurmak
yerine bir dünya (world) kurmak, varlık yaratmak ve bileşen eklemek gerekir;
bu, testlerin **kurulum** yarısının tamamen yeniden yazılması demektir.

Üçüncü ve daha az konuşulan kırılma: `noEngineReferences: true` düşer. DOTS
paketleri motora bağlıdır; bugün `GridStrategy.Core` ve `GridStrategy.Combat`
motoru hiç tanımıyor ve bütün çekirdek testleri sahnesiz koşuyor.

**E · ÖN KOŞUL** — İki kavram: **veri yerleşimi** (bir dizinin bitişik durması
ile bir `Dictionary` düğümünün dağınık durması arasındaki fark, ve bunun
işlemci önbelleğinde ne anlama geldiği) ve **ölçüm** (Aşama 6). Birincisi
`HENÜZ YOK → bu ağaç dışında bir sahip`.

**ÜÇ OYUN** — Slay the Spire: ██ EŞLEŞMEZ ██ ekranda aynı anda onlarca eleman
var, kare başına iş yok denecek kadar az · Vampire Survivors: ekranda aynı anda
binlerce düşman ve mermi her karede konum, çarpışma ve ömür günceller —
eşiğin gerçekten aşıldığı yer burasıdır · Stardew Valley: ██ EŞLEŞMEZ ██
kasabanın tamamı bile yüzlerce varlıkla ölçülür ve çoğu iş günde bir kez olur.

---

## Aşama 6 · Profil çıkarma (profiling) kanıt sınırı

**Profil çıkarma nedir:** Programın nerede zaman ve bellek harcadığını
**ölçmek**. Unity'de araçları Profiler, Profile Analyzer, Frame Debugger ve
Memory Profiler.

██ Bu bölüm olmadan yukarıdaki beş "tetikleyici: ölçülünce" cümlesi **boş
kalır**. ██

**A · BUGÜNKÜ KARŞILIĞI** — Ölçüm var, ve tam olarak nereye kadar geçerli
olduğu yazılı:

```
Assets/Tests/EditMode/Combat/DamageRulesAllocationTests.cs:36   maliyet testleri
   negatif kontrol      :69   ölçüm aygıtı tahsisi görebiliyor mu
   sıcak yol ölçümü     :103  ResolveRemaining hiç tahsis yapmaz
   refaktör savunması   :148  ayrı sınıftaki formülü çağırmak tahsis yapmaz
   ısınma (JIT)         :168  ilk çağrının tek seferlik maliyeti ölçüme karışmasın
Tools/run-editmode-tests.ps1        testleri Editor'e dokunmadan koşar
Tools/.test-results/                koşu çıktıları (XML + Unity log)
```

██ İKİ ÖLÇÜLMÜŞ OLGU ██, ikisi de bu dosyada yazılı:

① `GC.GetAllocatedBytesForCurrentThread()` bu projenin Unity sürümünde
(2021.3.45f2, Mono) **her zaman sıfır döndürüyor**
(`DamageRulesAllocationTests.cs:26-31` ve `:72-78`). Yani ölçmüyor. Yerine
Unity Test Framework'ün kendi kısıtı (`AllocatingGCMemory`) kullanılıyor; o
kısıt motorun profil kaydedicisine bağlanır, .NET'in sayacına değil.

② Kısıt bir **lambda** alıyor ve ölçülen şey lambda'nın **içidir**. Döngü
lambda'nın dışına yazıldığında kaydedici boş bir aralık ölçer, sıfır tahsis
görür ve **yeşil verir**. Bu bir kez gerçekten yaşandı ve yalnızca negatif
kontrol yakaladı (`DamageRulesAllocationTests.cs:115-120`).

**B · TETİKLEYİCİ KOŞUL** — Bu maddede tetikleyici bir gelecek olayı değil,
**bugün geçerli olan bir sınırdır**: aşağıdaki üç kovadan hangisinde ölçtüğün,
o sayının neyi kanıtladığını belirler. Diğer beş aşamanın tetikleyicisi bu
sınıra bağlıdır.

```
EditMode'da ölçüm    ►  İTERASYON kanıtıdır.
                        "Bu değişiklik dünküne göre tahsis eklemedi" der.

PlayMode'da ölçüm    ►  SAHNE kanıtıdır.
                        Gerçek oyun döngüsünü ve motor işini içerir.

HEDEF CİHAZDA ölçüm  ►  TEK GERÇEK PERFORMANS KANITIDIR.
                        Editor'ün kendi yükü yok, gerçek işlemci,
                        gerçek bellek, gerçek ısınma davranışı var.
```

Editor'deki bir sayı hedef cihazdaki davranışı **kanıtlamaz**. Editor kendi
profil kaydedicisini, kendi çizim yolunu ve kendi yönetilen yığınını taşır;
Mono ile IL2CPP farklı çalışma zamanlarıdır ve bir mobil cihazın ısınma
davranışı masaüstünde hiç görünmez.

██ Bu yüzden yukarıdaki beş aşamada geçen "ölçülebilir hâle geldiğinde"
cümlelerinin hepsi eksiktir; tam hâli şudur: **hedef cihazda, öncesi ve
sonrası eşleşen bir ölçümle görünür hâle geldiğinde.** ██

**C · İLK ADIM** — Değişecek ilk dosya `Tools/run-editmode-tests.ps1`
**değil**; o zaten var ve EditMode kanıtını üretiyor. İlk adım bir dosya
değil bir **kayıt**: ölçülen sayının hangi kanıt kovasına ait olduğunun
yazılması. Bugün `Tools/.test-results/` altındaki çıktı EditMode kovasına ait
ve bu, `Assets/Tests/EditMode/Combat/DamageRulesAllocationTests.cs:33-34`'te
açıkça yazılı.

**D · NE KIRAR** — Ölçüm **atlanırsa** ne kırılır, sorusu buranın asıl konusu:
havuz kurulur ve hiçbir şeyi hızlandırmaz; ECS'e geçilir ve bütün test yüzeyi
ölçülmemiş bir kazanç uğruna yeniden yazılır. Erken getirilirse ne kırılır
sorusunun cevabı ise **hiçbir şey** — ölçüm, üretim kodunu değiştirmeyen tek
maddedir. Bedeli yalnızca zaman.

Tersi de doğru ve kodda yazılı: ölçüm aracının kendisi ölçüm penceresinin
içinde iş yapıyorsa, ölçtüğü şey artık üretim kodu değildir
(`DamageRulesAllocationTests.cs:55-56`).

**E · ÖN KOŞUL** — **Tahsis ve çöp toplama** kavramı (bir nesnenin ne zaman
yönetilen yığında yer kapladığı, kutulamanın ne olduğu) ve **test kanıt
seviyeleri** (EditMode / PlayMode / cihaz). Birincisi kapalı: kutulama ve
numaralandırıcı tarafı için
[../deep/dil/02-koleksiyonlar-ve-salt-okunur.md](../deep/dil/02-koleksiyonlar-ve-salt-okunur.md),
yönetilen yığın ve çöp toplama tarafı için
[../deep/dil/07-bellek-canlilik-ve-yikim.md](../deep/dil/07-bellek-canlilik-ve-yikim.md).

**ÜÇ OYUN** — Slay the Spire: ██ EŞLEŞMEZ ██ oyuncunun gördüğü bir kare
bütçesi yok; oyun oyuncunun düşünme hızında ilerler · Vampire Survivors:
ekrandaki düşman sayısı arttıkça kare hızının düşmesi doğrudan **görünür** ve
oyunun zorluk eğrisiyle iç içedir · Stardew Valley: gün sonu hesabının
takılmadan geçmesi, kasaba büyüdükçe ölçülmesi gereken bir şeydir.

---

## Bu dosyanın kendi sınırı

Buradaki hiçbir satır "yapılacak iş" değildir. Altı aşamanın tamamı bugün
**doğru** durumda: yokluk bir eksiklik değil, ölçülmüş bir karar. Bir aşamanın
tetikleyici koşulu gerçekleştiğinde o satır bir yol haritası satırına dönüşür —
ve o gün bu dosya güncellenir, kod değil.

---

## Alıntı çapaları

Aşağıdaki satırlar bu belgede geçen satır numaralarının **çapasıdır**. Her satır
`Tools/check-doc-code-refs.py`'nin ALINTI katmanına, o numarada duran kodun
BİREBİR metnini verir. Ölçüldü: ALINTI katmanı 3 satırlık kaymayı bile %100
yakalıyor, YAKIN AD katmanı 6 satırlık kaymanın %1'ini. Tablo hücrelerindeki ve
cümle içindeki atıflar alıntı biçimine giremez — o biçim atfın satır BAŞINDA
olmasını ister. Kod kaydığında kızacak olan yer burasıdır; kızdığı gün bu
belgede geçen aynı numaraların hepsi elden geçirilir.

```
Assets/Game/Unity/BoardAdapter.cs:135      [SerializeField, Min(1)] private int maxHealth = 30;
Assets/Game/Unity/BoardAdapter.cs:138      [SerializeField, Min(0)] private int damage = 10;
Assets/Game/Unity/BoardAdapter.cs:141      [SerializeField, Min(1)] private int attackRange = 1;
Assets/Game/Unity/BoardAdapter.cs:150      [SerializeField, Min(0)] private int moveRange = 1;
Assets/Game/Unity/BoardAdapter.cs:178      [SerializeField, Min(1)] private int structureMaxHealth = 50;
Assets/Game/Unity/BoardAdapter.cs:667      var cell = new GameObject($"Cell_{x}_{y}");
Assets/Game/Unity/BoardAdapter.cs:568      var structureObject = new GameObject($"Structure_{x}_{y}");
Assets/Game/Unity/BoardAdapter.cs:210      private readonly List<Unit> cleanupBuffer = new List<Unit>();
Assets/Game/Unity/BoardAdapter.cs:124      [SerializeField] private UnitView unitPrefab;
Assets/Game/Unity/BoardAdapter.cs:232      private void Awake()
Assets/Game/Battle/Battle.cs:429           public int RemoveReadyForCleanup(List<Unit> removed)
Assets/Game/Battle/Battle.cs:377           public void Tick(float deltaSeconds)
Assets/Game/Battle/TurnState.cs:64         private readonly ReadOnlyCollection<Team> orderView;
Assets/Game/Core/Combat/Combatant.cs:111   public event Action<UnitState, UnitState> StateChanged;
Assets/Game/Unity/UnitView.cs:86           private void Awake()
Assets/Game/Unity/UnitView.cs:107          SetSelected(false);
```


## İlgili

- Bu ağacın yönlendirmesi: [README.md](README.md)
- Kodda **zaten** duran desenler: [01-koda-gomulu-desenler.md](01-koda-gomulu-desenler.md)
- Kapsama tablosu: [03-kavram-borc-defteri.md](03-kavram-borc-defteri.md)
- Olay zincirinin hikâyesi: [../deep/konular/01-olay-zinciri.md](../deep/konular/01-olay-zinciri.md)
- Assembly duvarının faturaları: [../deep/konular/02-assembly-duvari.md](../deep/konular/02-assembly-duvari.md)
- Yaşam döngüsü ve yasak geçişler: [../deep/konular/05-yasam-dongusu.md](../deep/konular/05-yasam-dongusu.md)
