# Devir — kalıcı emir, paralel savaş ve arayüz kenarları

> **Tarih:** 2026-08-29 · **Durum:** bir önceki tur KAPALI ve commit'li (8 commit, push YOK)
> Bu belge, `Docs/handoff/2026-08-28-uretim-paneli-devri.md`'nin devamıdır.
> Önce onu oku: kapalı gerçekler, ölçülmüş kısıtlar ve açık borçlar orada.

## Bir bakışta — bugünkü sahiplik ve nerede kırıldığı

```
GIRDI                      TAHTANIN KIPI              BIRIMIN NIYETI
oyuncu tiklamasi   ->   BoardModeMachine        ->    ??? YOK
                        (TEK, ayni anda tek kip)      pendingStrike* dortlusu
                        Idle / Placement /            BoardAdapter'da TEK KOPYA
                        PendingStrike                 => ayni anda TEK emir

                                                      ^^^^^^^^^^^^^^^^^^^^^^^^
                                                      operatorun "iki taraf
                                                      paralel olmuyor"
                                                      sikayetinin KOK SEBEBI
```

`BoardAdapter` bekleyen vuruşu dört tekil alanda tutuyor
(`pendingStrikeAttacker`, `pendingStrikeTarget`, `pendingStrikeX/Y`). İkinci bir
birime emir verildiği an birincisinin emri siliniyor. Paralel emir bir ayar
eksikliği değil, **sahiplik hatası**: emir tahtaya değil BİRİME ait olmalı.

## Bu turda yapılacaklar

### İŞ-1 (P0) — kalıcı saldırı emri, Command pattern ile

Operatör: *"bir attacker'a target belirttiğimizde 1 kere saldırıyor; tekrardan
yönlendirmediğimiz sürece saldırmaya devam edebilmeli. Hedef kaçarak menzilden
çıkarsa saldırı kesilmeli, ve birden fazla saldıran varsa her biri kendi
menzilinden koptuğunda kesmeli."*

**Seçilen pattern: Command (emir nesnesi), State DEĞİL.** Ayrım tek cümlede:

> **State**, TAHTANIN şu an ne yaptığıdır ve **tektir**.
> **Order/Command**, HER BİRİME ne söylendiğidir ve **çoğuldur**.

Kalıcı saldırı çoğul olduğu için kip makinesine sığmaz; `BoardModeMachine`
girdinin anlamını sahiplenmeye devam eder, emirler ondan bağımsız yaşar.

Önerilen şekil (bağlayıcı değil, ölçü):

```
IUnitOrder                       Tick(deltaSeconds) -> Devam / Bitti / Iptal
  AttackOrder(target)            menzildeyse vur, cooldown kapisi Core'da
  MoveOrder(x, y)                yurume bitince Bitti
                                 (bugunku "yaklas sonra vur" ikisinin BILESIMI)

Dictionary<Unit, IUnitOrder>     emir tablosu — birim BASINA bir emir
  yeni emir      -> eskisini degistirir (oyuncu yeniden yonlendirdi)
  hedef menzil disi -> Iptal, ve SADECE o saldiranin emri
  hedef tahtadan kalkti / oldu -> Iptal
  saldiran kalkti  -> Iptal
```

**Zorunlu testler** (bu turun ölçütü):
- İki farklı birime aynı anda emir verilebiliyor ve ikisi de tutuluyor.
- **İki AYRI TAKIMDAN** birer birim aynı anda emir tutabiliyor (operatörün
  bildirdiği belirti tam olarak budur).
- Hedef menzilden çıkınca yalnız etkilenen emir iptal oluyor; aynı hedefe
  saldıran menzildeki öteki birimin emri DEVAM ediyor.
- Yeni emir eskisinin yerine geçiyor, ikisi birden koşmuyor.
- Bekleme süresi kuralı emrin içinde İKİNCİ kez yazılmıyor — `AttackAction`
  zaten `RejectedOnCooldown` döndürüyor, emir onu sessizce yutup bekliyor.
- Emir tablosu `DespawnView` / `RemoveSelected` / kaldırma yollarında sızmıyor.

### İŞ-2 (P1) — seçim emirden sonra bırakılsın, ama geri alınabilsin

Operatör: *"attacker'ın kime saldıracağı belirtildiğinde seçim kaldırılmalı ama
tekrardan seçim alınabilecek şekilde de ayarlanabilir."*

Bugün seçim yalnız **isabet eden** saldırıdan sonra bırakılıyor
(`ReleaseSelectionAfterStrike`). Kalıcı emirle birlikte doğru kural şu olur:
**emir YAZILDIĞI an seçim bırakılır** — çünkü emir artık seçime bağlı değil,
birime ait. Birime tekrar tıklamak onu yeniden seçer ve mevcut emrini gösterir.
DİKKAT: bugünkü bekleyen-vuruş zinciri seçime bağlı (`PendingStrikeIsAlive`
`selectedUnit` ile karşılaştırıyor); emir tablosuna geçince o bağ KOPMALI,
yoksa seçimi bırakmak emri iptal eder.

### İŞ-3 (P1) — durum şeridi "sıra sen" demeyi bıraksın

`FreeForAll` kipinde tur numarası ilerlemiyor, yani şerit ölü bir sayı
gösteriyor. **Yeni mekanizma EKLENMEYECEK** — ölçüldü ve gerekçesi aşağıda
(`Pattern kararları` bölümü). `BattleStatusView` zaten `SelectionChanged`'e
abone; gösterilecek cümle "sıra sen" değil, seçili şeyin TARAFI (senin takımın /
düşman takım, mavi / kırmızı) ve savaşın durumu olmalı.

### İŞ-4 (P2) — arayüz bileşenleri ekran köşelerine yapışık

Operatör: *"sağ alttaki sil düğmesi direkt köşeye yapışık, düzgün değil."*
`SceneSetupTool` panelleri kuruyor; kenar boşluğu (margin), güvenli alan ve
dokunma hedefi ölçüleri orada sabit olarak yaşıyor. Köşeye yapışmayı tek tek
düzeltmek yerine **tek bir kenar boşluğu sahibi** tanımla ve bütün paneller onu
okusun — aynı "tek sahip" kuralı, bu sefer arayüzde.

## Pattern kararları — YENİDEN AÇMA

Bir önceki turda on iki pattern ölçüldü. Kapalı olanlar:

| pattern | durum | gerekçe |
|---|---|---|
| Object Pooling | **VAR** | `UnitViewPool` |
| Observer | **VAR** | 11 `public event` |
| Flyweight | **VAR** | `UnitBlueprint` / `AttackProfile` paylaşılan değişmez tanımlar |
| Factory | **VAR** | `CreateCombatant`, `CreateStructure`, `ProjectileView.Fire` |
| State | **VAR** | `Assets/Game/Unity/Modes/` — tahtanın kipi |
| **Command** | **BU TURDA YAPILACAK** | kalıcı emir; tetikleyici geldi — ama *undo* için değil, **çoğul emir** için |
| Singleton | **REDDEDİLDİ** | referans proje `Map.Instance` kullanıyor; 105 Unity testi tam da global durum olmadığı için koşuyor |
| Event Bus | **REDDEDİLDİ** | bugünkü olaylar tipli ve yönlü; bus onları isimsiz yapar |
| Service Locator | **REDDEDİLDİ** | assembly duvarını deler |
| MVC/MVP | **GEREKSİZ** | yerine daha sert bir ayrım var: `noEngineReferences: true` derleyiciyle zorlanıyor |
| Strategy | **ERTELENDİ** | hedef seçimi tek algoritma; ikinci bir kural doğmadan soyutlamak erken |
| Decorator | **GEREKSİZ** | katmanlı etki (zırh/buff) yok |

**Kural:** bir pattern, ancak mevcut mekanizmanın **ölçülmüş** bir eksiği varsa
eklenir. "Sıra sen" sorununda eksik olan mekanizma değil, gösterilen cümledir.

## Burst / Job System — ÖLÇÜLDÜ VE REDDEDİLDİ

Operatör bunları sordu; cevap dürüst biçimde **hayır**, ve sebebi sayılarla:

- Tahta **10x5 = 50 hücre**. Birim sayısı onlu mertebede.
- `unity-expert-code-quality` kural 15-16 ECS/Burst/Jobs'u on bir soruluk bir
  ön uçuşun ve ölçülmüş bir yük ekseni sayımının arkasına kilitliyor: *"A
  project reaching for jobs has already entered this contract."*
- Jobs/Burst binlerce varlık için vardır. Elli hücrede bir iş kuyruğu kurmanın
  bedeli, kazandırdığından büyüktür.

**Gerçek performans borcu başka yerde ve o ölçüldü:**
- `Battle.TryGetPosition` her çağrıda tahtanın TAMAMINI tarıyor ve sık
  çağrılıyor. Kalıcı emirler her karede konum soracağı için bu, İŞ-1 ile
  birlikte **gerçekten** ısınacak yer burasıdır.
- İmleç çerçevesinin rengi için her karede tam A* çalışıyordu; önbelleğe alındı
  ama önbellek yolu kapayan ÜÇÜNCÜ bir birim kımıldarsa eskiyebilir.

**Yeniden açma tetikleyicisi:** eşzamanlı emir sayısı × birim sayısı, ölçülmüş
bir kare bütçesini aştığı gün — ve o gün önce `TryGetPosition`'ın sözlüğe
alınması denenir, Burst değil.

## Açık borçlar (bir önceki turdan devredenler)

| borç | ölçü |
|---|---|
| `check-doc-code-refs` | HEAD'de 258 → şimdi ~350; `Docs/` içindeki satır çapaları kaydı |
| `PlacementGhost.prefab` | guid'ine sıfır atıf; ikinci ölü varlık |
| `SampleScene.unity` `structureScale: 1.6` | ölü YAML; araç sahneyi kaydettiğinde düşer |
| `IPlacementModeHost` 7 üye, `IPendingStrikeHost` 9 üye | god object'in kalan bağımlılığının fotoğrafı; bölündükçe daralmalı |
| operatör adımı | `CountryBall → Sahneyi Kur (her şey)` + Ctrl+S |

## Ölçülmüş kısıtlar (yeniden keşfetme)

- Unity **6000.5.7f1**, C# 9. `record struct` YOK.
- Unity serileştiricisi alanları yansımayla yazar, **kurucuyu çağırmaz** —
  Inspector'dan doldurulan `readonly struct` değişmezliği hakkında yalan söyler.
- Editor açıkken `run-editmode-tests.ps1` `exit 2`. Çözüm: `Assets/`+`Packages/`
  +`ProjectSettings/`+`Tools/` bir scratch dizine kopyalanır **VE O KOPYA
  YENİDEN KULLANILIR** — her koşumda yeni kopya, testin 0,6 saniyesi için 2-4
  dakika içe aktarma ödetir; ısınmış `Library/` ile ~30 saniye.
- `total` tek başına yalan söyler; XML'den **assembly başına** okunmalı.
- Kopya-koşumun kör noktası: kaynak doğru ama operatörün `Library/` durumu
  bozuksa yeşil verir. Bu turda gerçekten oldu — `BoardSizing.cs` AssetDatabase'e
  sıkışmış bir kayıtla girdi, `csproj` onu derlemeye almadı, ve düzelten şey
  dosyayı `Assets/` ağacından çıkarıp geri koymak oldu.
- Testlerde `using System;` **YASAK** (`Object` adı `UnityEngine.Object` ile
  belirsizleşir, CS0104); `System.ArgumentException` tam nitelikli yazılır.

## Son ölçüm

`482 → 651 test, 651/651 yeşil, 0 başarısız`
(Battle 164 · Combat 241 · Core 100 · Unity 146 — assembly başına doğrulandı)
Dokuz assembly'nin dokuzu da derleniyor; operatörün Editor'ünde MCP ile teyit
edildi.
