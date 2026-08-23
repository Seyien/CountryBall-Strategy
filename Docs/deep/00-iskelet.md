# İskelet — bütün sistem, tek dosyada

> **Ne bu:** üç ağacın ÖNÜNDE duran giriş kapısı — oyunun ne olduğu, hangi
> tasarım basıncının hangi parçayı doğurduğu, sistemin tek figürü, hangi sorunun
> hangi dosyaya gittiği.
> **Ne değil:** hiçbir mekanizmanın ayrıntısı burada yok; ayrıntı üç ağacın malı
> ve oraya `→` ile gidilir. Ağaçların tanıtımı [README.md](README.md)'de.
> **Ne zaman oku:** projeye ilk geldiğinde, TEK BAŞINA, baştan sona; sonra da
> yalnızca "bu parça neyin içindeydi" diye sorduğunda.

---

## Bu dosya neden var

Üç ağaç (`kod/` · `konular/` · `dil/`) her biri kendi başına iyi. Ama sisteme
ilk kez gelen biri `konular/01-olay-zinciri.md`'den başlıyor ve bir mekanizmanın
ORTASINA düşüyor: neyin parçası olduğunu, oyunun ne olduğunu, hangi tipin nerede
durduğunu bilmiyor. Operatörün 2026-08-19'daki ifadesiyle: *"sen sadece şuraya
bak şuraya dediğinde diğer kısımlarını görmediğim için kafamda
oturtturamıyorum"*. Kusurun adı belli: **bitmiş bir sistemi hiç görmemiş birine
dilim dilim öğretmek parça biriktirir, resim kurmaz.** Bu dosya önce BÜTÜN
yüzeyi veriyor.

---

## 1. Oyun — önce oynanış, sonra kod

### Tek cümle

> Kareli bir tahtada iki takım sırayla hareket eder ve birbirine vurur; düşen
> birim hemen ölmez, bir süre kurtarılabilir kalır; seçili bir birim tahtaya
> baraka koyabilir.

Tek bir tip adı geçmiyor ve bu kasıtlı: kod adı geçmeden önce oyun anlaşılmalı.

### Ekranda ne var

Ölçüsü `BoardAdapter.cs`'teki serileştirilmiş varsayılanlar ("serileştirilmiş"
= sahne dosyasına yazılıp Play'de geri okunan, Inspector'dan değiştirilebilen
değer):

```
     x=0     x=1     x=2
   ┌───────┬───────┬───────┐
y=4│       │       │       │      width = 3   height = 5   → 15 hücre
   ├───────┼───────┼───────┤      maxHealth = 30   damage = 10
y=3│       │◆ Raider       │      attackRange = 1  moveRange = 1
   ├───────┼───────┼───────┤      structureMaxHealth = 50
y=2│       │● Vanguard     │
   ├───────┼───────┼───────┤      ◆ Team.Enemy   ● Team.Player
y=1│       │       │       │      ikisi KOMŞU: ilk tıklamada saldırı
   ├───────┼───────┼───────┤      zinciri kapanabilsin diye
y=0│       │       │       │
   └───────┴───────┴───────┘
```

Zemin karoları rastgele değil — hücrenin sprite'ı `(x * 7 + y * 13) %` sprite
sayısı ile seçiliyor. Ölçüsü: gördüğün bir görsel hatayı Play'i kapatıp açarak
yeniden üretebilirsin.

### Bir tıklamanın dört anlamı

Aynı sol tık, tahtanın hâline göre dört ayrı şey demek:

```
                        seçili birim YOK      seçili birim VAR
                     ┌────────────────────┬─────────────────────────┐
 tıklanan hücre DOLU │  o birimi SEÇ      │  aynı birimse BIRAK     │
                     │                    │  başkasıysa SALDIR      │
                     ├────────────────────┼─────────────────────────┤
 tıklanan hücre BOŞ  │  yalnız bildir     │  HAREKET ET             │
                     └────────────────────┴─────────────────────────┘
        ██ BU BİR NİYET TABLOSUDUR ██  "izin var mı" sorusu burada YOK;
        onu bir alt katman cevaplıyor (→ §3)
```

Beşinci bir giriş ayrı bir KİP: seçili bir birim varken `B` tuşu yerleştirme
kipini açıyor, hayalet bir baraka fareyi izliyor, `Escape` iptal ediyor.
Sürükle-bırak ile tıkla-bırak farklı davranıyor ve farkı ölçen tek şey
"basılıyken imleç 0.25 dünya biriminden fazla hareket etti mi".
→ [konular/07-tiklamadan-eyleme.md](konular/07-tiklamadan-eyleme.md)

### Bir birim nasıl ölür

Bu oyunun en ayırt edici kuralı, ve tek figürde:

```
   Alive ──canı 0'a indi──► Downed ──10 saniye──► Dead ──5 saniye──► tahtadan
     ▲                        │   bu pencerede birim:                 silinir
     │                        │     · hedeflenmeye ve hasar almaya DEVAM eder
     └──── diriltildi ────────┘     · hareket EDEMEZ

   ██ Downed'dan Alive'a GERİ DÖNEN ok, bu üçlünün var olma sebebi ██
   Ölçüsü: UnitLifecycle.DefaultDownedWindowSeconds = 10f · corpse = 5f
```

Yapıların (baraka) ikizi bu değil: onlarda ayakta/yıkık iki durum var, geri
dönen ok YOK. Neden ayrı bir enum (numaralandırma) yazıldığı ve bedelinin ne
olduğu → [konular/05-yasam-dongusu.md](konular/05-yasam-dongusu.md).

---

## 2. Yedi tasarım basıncı — bu dosyanın omurgası

Aşağıdaki sıra **dosya sırası değil**, bağımlılık grafiği değil, alfabe değil.
Sıra tek bir soruya göre: *hangi basınç bir sonrakini mümkün kıldı.* Her basınç
aynı beş adımla açılıyor:

```
hangi problem → ne karara bağlandı → seçenekler neydi → ne yaratıldı → nasıl rafine edildi
```

---

### B1 — Bir kuralı sınamak için oyunu açmak zorunda kalmak

**Problem.** "10 saniye sonra ölür" kuralını sınamak için Play'e basmak,
10 saniye beklemek ve ekrana bakmak gerekiyordu. Sınanamayan kural, yazılmamış
kuraldır.

**Karar.** Oyun kuralları motoru HİÇ görmeyen ayrı derleme birimlerine
(assembly — birlikte derlenip tek bir `.dll` üreten dosya kümesi) konur.

**Seçenekler.** (a) Tek derleme birimi, kurallar `MonoBehaviour` içinde —
sınamak için sahne şart. (b) Kurallar ayrı klasörde ama aynı derlemede —
**klasör hiçbir şey yasaklamaz**, `using UnityEngine;` yazılabilir ve derlenir.
(c) Ayrı derleme birimi + motor referansı kapalı. Seçilen: (c).

**Ne yaratıldı.** Dört `.asmdef` dosyası; üçünde motor referansı KAPALI:

| Derleme birimi | noEngineReferences | references |
|---|---|---|
| `GridStrategy.Core` | `true` | `[]` |
| `GridStrategy.Combat` | `true` | `[]` |
| `GridStrategy.Battle` | `true` | `Core`, `Combat` |
| `GridStrategy.Unity` | `false` | `Core`, `Combat`, `Battle` |

**Bu satır tam olarak neyi yasaklıyor.** "Motorsuz" bir etiket; ölçüsü şu:
`noEngineReferences: true` olan bir derlemede `UnityEngine` adı ÇÖZÜLMEZ.
`Input`, `Time`, `Vector2`, `MonoBehaviour`, `Debug.Log` yazan satır bir uyarı
üretmez — **derlenmez**. Karşı örnek aynı projede: `GridStrategy.Unity`'de aynı
alan `false` ve `BoardAdapter.cs` dört ayrı satırda `Input.GetMouseButton*`
çağırıyor. Aynı çağrı `PointerGesture.cs`'e yazıldığı gün derleme kırılır.

**Nasıl rafine edildi.** Duvar kurulunca yeni bir borç doğdu: zamanı kim
veriyor? `UnitLifecycle` `Time.deltaTime` OKUYAMAZ, dolayısıyla `Tick(float
deltaSeconds)` saniyeyi dışarıdan alıyor — ve bu tek imza değişikliği kuralı
EditMode'da sınanabilir kıldı.
→ [kod/Core/Combat/UnitLifecycle.md](kod/Core/Combat/UnitLifecycle.md)

**Faturası da var, gizlenmedi.** `Core` ile `Combat`'ın `references` listesi
BOŞ, yani **birbirlerini de görmezler**. Dört somut faturası →
[konular/02-assembly-duvari.md](konular/02-assembly-duvari.md)

### B2 — Tahtada duran şey ne: asker mi, bina mı, ikisi mi

**Problem.** Bir baraka da hücre kaplar, seçilir, canı vardır, hedeflenir. Ama
saldırmaz, düşmez, diriltilmez. Aynı tahtaya nasıl konur?

**Karar.** Tahtanın anahtarı TÜRE GÖRE ÇOĞALMAZ: tek bir kimlik tipi var,
tanımı *"tahtada yer kaplayan, kimliği olan şey"*. Asker de baraka da odur.

**Seçenekler.** (a) `Unit` + `Building` iki ayrı kimlik tipi → iki ayrı tahta →
"bu hücre dolu mu" sorusunun İKİ cevabı olur ve ikisi de "hayır" diyebilir.
(b) `Structure : Combatant` kalıtımı — reddedildi, çünkü kalıtım seçmeli
değildir: baraka devralacağı üyelerin yarısına uymaz (diriltme, düşme hâli,
zorunlu saldırı tanımı, kurtarma penceresi). (c) Tek kimlik tipi + iki yan
tablo. Seçilen: (c).

**Ne yaratıldı.**

```
   Unit  ── yalnız bir ad taşır; tipte çağrılabilir tek metot YOK
     ├─► UnitGrid                      Unit[,] — hangi hücrede hangi kimlik
     ├─► Dictionary<Unit, Combatant>   o kimlik bir asker mi
     └─► Dictionary<Unit, Structure>   o kimlik bir yapı mı
```

**Ölçüsü.** `BattleActions.PlaceStructure` içinde doluluk sorusu **tek satır**:
`battle.TryGetUnit(x, y, out Unit _)`. İki tahta olsaydı burada iki soru olurdu
ve birini unutan gün aynı hücrede iki şey dururdu — hiçbir derleme hatası
çıkmadan.

**Nasıl rafine edildi.** Tek kimliğin bedeli: aynı `Unit` iki sözlükte birden
bulunabilirdi. `Battle.ThrowIfCannotJoin` bunu kapatıyor; o kelepçe kalktığı
gün temizlik süpürmesi aynı görseli iki kez silmeye çalışır.
→ [kod/Core/Unit.md](kod/Core/Unit.md)

### B3 — Ölmek bir an değil, bir süreç

**Problem.** "Ölü ama 10 saniye içinde diriltilebilir" ne canlıdır ne kalıcı
ölüdür — ve hasar almaya DEVAM etmesi gerekir. Durumu tutan şey bir `bool` iken
bu cümle yazılamıyordu.

**Karar.** Geçersiz hâl TİPTE VAR OLMAMALI: üç değerli bir enum kullanılır.

**Seçenekler.** (a) `bool isAlive` — üçüncü hâl ifade edilemez. (b) İki bool
(`isAlive`, `isDowned`) — dört kombinasyon üretir, dördüncüsü (ikisi birden
doğru) anlamsızdır ama YAZILABİLİR. (c) Üç değerli enum — anlamsız hücre tipte
hiç yoktur; engellenmiyor, doğmuyor. Seçilen: (c).

**Ne yaratıldı.** `UnitState` (`Alive`/`Downed`/`Dead`), sayacı ve geçişi tutan
`UnitLifecycle`, cana bakan ayrı `Health`.

**Nasıl rafine edildi.** Yapı geldiğinde aynı enum PAYLAŞILMADI. Ölçüsü: birimin
geçiş grafiğinde `Downed`'a GERİ DÖNEN bir ok var, yapınınkinde yok — ortak enum,
yapı üzerindeki her `switch`'e asla çalışmayan bir `Downed` dalı açardı. Ödenen
bedel görünür: `TargetingRules` iki durum dili konuşuyor ve **altı aşırı
yükleme** taşıyor (`CanBeAttacked` üç kez, `CanBeRevived` üç kez).
→ [konular/05-yasam-dongusu.md](konular/05-yasam-dongusu.md)

### B4 — "Hayır" demenin dört yolu

**Problem.** `MoveAction` bir `bool` döndürüyordu: "taşındı mı". Ama çağıran üç
ayrı ret sebebine üç ayrı tepki veriyor — tahta dışı tıklama sessizce yutulur,
dolu hücre uyarı ister, menzil dışı "önce yaklaş" der. `bool` ile bu ayrım,
çağıranın içinde kuralları KOPYALAYAN ikinci bir kontrol olarak doğardı.

**Karar.** Ret bir SONUÇ DEĞERİDİR — ama yalnızca çağıranın yapabileceği bir
şey varsa. Yapabileceği bir şey yoksa istisna (`throw`) atılır.

**Seçenekler.** (a) `bool`. (b) Her ret için istisna — "dolu hücreye tıkladım"
bir program hatası değil, normal oyun akışı; istisna onu hata kılığına sokar.
(c) Tek bir `Rejected` değeri — sebep sayısını değil DAVRANIŞ sayısını gözden
kaçırır. (d) Sebep başına ayrı değer. Seçilen: (d), ve çizgi (a)/(b) arasında
çekildi.

**Ne yaratıldı.** Dört sonuç enum'u — `MoveOutcome` (5 değer), `AttackOutcome`
(6), `PlacementOutcome` (3), `ReviveOutcome` (4). **Dördünde de sıfırıncı değer
bir RET.**

**Ölçüsü.** Hiçbirinde `= 0` yazılı değil; numarayı satır SIRASI belirliyor.
`default(AttackOutcome)`, sıfırlanmış dizi hücresi ve atanmayı unutulan alan —
üçü de sıfıra düşer. Sıfırda `Hit` dursaydı atanmamış bir değer BAŞARILI bir
saldırı gibi okunurdu.

**Nasıl rafine edildi.** Bir enum, sahibinin ÜRETEMEYECEĞİ bir değer taşıyor:
`MoveOutcome.RejectedActorCannotAct`'i döndürebilen tek yer `GridStrategy.Battle`
katmanı; `Core` o kuralı göremiyor. Taviz verildi ve gizlenmedi.
→ [konular/06-sonuc-enumlari.md](konular/06-sonuc-enumlari.md)

### B5 — Tahtaya ikinci bir yazar doğmasın

**Problem.** `BoardAdapter`'da bir `UnitGrid` alanı vardı. Oradan yazan her
satır, savaş kayıtlarını (iki sözlüğü) ATLIYORDU: tahtada duran ama savaşta
olmayan bir birim üretilebiliyordu.

**Karar.** Tahtanın tek sahibi `Battle`'dır ve tahtayı DIŞARIDAN ALMAZ, kendi
kurar.

**Seçenekler.** (a) `Battle(UnitGrid board)` — kurucu tahtayı alsın. Reddedildi:
aynı nesneye ikinci bir ok doğar ve `readonly` bunu KORUMAZ; `readonly` okun
kendisini dondurur, okun UCUNDAKİ nesneyi değil. (b) `public UnitGrid Board` —
sahiplik sözü tek satırda çözülür. (c) Kurucu içinde `new`, dışarı `internal`
erişim. Seçilen: (c).

**Ne yaratıldı ve ölçüsü — `internal` tam olarak neyi yasaklıyor.** `Battle`
kurucusunda `board = new UnitGrid(width, height)`, dışarı `internal UnitGrid
Board => board;`. `internal`, üyeyi yalnızca
AYNI derleme birimine açar: `GridStrategy.Unity` içindeki `BoardAdapter`'a
`battle.Board` yazıldığı gün derleyici CS0122 verir, `GridStrategy.Battle`
içinden yazıldığında derlenir. Bugün bütün depoda `battle.Board`'ın **tek bir
kullanıcısı** var: `BattleActions.cs`'teki `MoveAction.Execute(battle.Board, ...)`
çağrısı. Söz bir yorumda değil, derleyicide duruyor.

**Nasıl rafine edildi.** Garantinin bittiği çizgi de yazıldı: aynı derleme
birimindeki bir işbirlikçi tahtayı hâlâ değiştirebilir. Koruma total değil →
[konular/03-tahta-sahipligi.md](konular/03-tahta-sahipligi.md)

### B6 — Olay "kim" bilgisini taşımıyor

**Problem.** Bir birim `Downed`'a düştüğünde ekranın güncellenmesi gerekiyor.
Ama düşüşü FARK EDEN tip (`UnitLifecycle`) yalnızca bir sayaç: kimin sayacı
olduğunu bilmiyor. Üstündeki `Combatant` da kendi kimliğini bilmiyor — kimlik
`GridStrategy.Core`'da, savaş `GridStrategy.Combat`'ta ve **ikisi birbirini
görmüyor** (B1'in faturası).

**Karar.** Zincirin her halkası bir şey EKLER, ve ekleyen halka o bilginin
SAHİBİ olur.

**Seçenekler.** (a) Olayın imzasına göndereni koymak
(`Action<Combatant, UnitState, UnitState>`) — faturayı azaltır ama sıfırlamaz;
üst katmanın istediği şey `Combatant` değil `Unit`. (b) Her savaşçı için
kimliği içine gömülmüş ayrı bir fonksiyon üretmek. Seçilen: (b).

**Ne yaratıldı.** Üç olay, her biri bir öncekinden bir değer daha zengin:

```
   UnitLifecycle.StateChanged     Action<UnitState>                  1 değer
            ▼  +önceki durum (sahibi: Combatant)
   Combatant.StateChanged         Action<UnitState, UnitState>       2 değer
            ▼  +kimlik (sahibi: Battle)
   Battle.UnitStateChanged        Action<Unit, UnitState, UnitState> 3 değer
            ▼
   BoardAdapter.OnUnitStateChanged ──► UnitView.SetState
```

**Nasıl rafine edildi.** İkinci geçişte üretilen fonksiyonlar birbirine EŞİT
DEĞİL — aynı metni ikinci kez yazarak abonelik çözülemez. Sökebilmek için tam o
örneği saklayan bir sözlük gerekti: `Battle.stateForwarders`; projedeki tek
"garip" alan ve tek sebebi bu.
→ [konular/01-olay-zinciri.md](konular/01-olay-zinciri.md) ·
[dil/04-delege-olay-ve-kapanis.md](dil/04-delege-olay-ve-kapanis.md)

### B7 — Motorun tarafında ne kaldı, ve ne kadar kaldı

**Problem.** `BoardAdapter` iki iş birden yapıyor: piksel→hücre çevirisi
(çevirmenlik) ve "dolu hücreye tıklamak SALDIRI demektir" (oyun kuralı).
İkincisi bir çevirmenin işi değil.

**Karar.** Karma rol KABUL EDİLDİ ama bir EŞİK yazıldı: bağımsız bir değişme
baskısı üreten her parça çıkarılır.

**Seçenekler.** (a) Her şeyi bölmek — bugün var olmayan bir baskı için bugün
maliyet. (b) Hiç bölmemek. (c) Eşiği yaz, aşıldığında böl. Seçilen: (c).

**Ne yaratıldı — eşik iki kez aşıldı, iki kez bölündü.**

```
   ÇIKAN                              KALAN (bugün baskı üretmiyor)
   ─────────────────────────────      ────────────────────────────────
   PointerGesture (Core)              Input okuma (dört çağrı)
     "tıklama mı sürükleme mi"        zemin karolarının kurulumu
   UnitView (Unity)                   "tıklama neyi kastediyor" tablosu
     birim başına görsel durum        piksel → dünya → hücre çevirisi
```

**Ölçüsü — eşiğin aşıldığı görünür sayı.** `BoardAdapter.cs` projenin en büyük
dosyası, ikinci sıradaki `Battle.cs`'in neredeyse iki katı. Ve
`Assets/Tests/EditMode/Unity/` klasöründe `UnitViewTests.cs` VAR,
`BoardAdapterTests.cs` YOK — 26 test dosyasının hiçbiri bu tipi sürmüyor, çünkü
`Input`, `Camera`, `Time` ve `Instantiate` kullanan bir tip `new` ile kurulamaz.

**Nasıl rafine edildi.** Eşik notu SİLİNMEDİ; rol başlığında "KOKU: evet ve
BÜYÜDÜ" olarak duruyor — bir eşiğin aşıldığını söyleyen satır, eşiği koyan satır
kadar öğretici. → [kod/Unity/BoardAdapter.md](kod/Unity/BoardAdapter.md)

---

## 3. Tek bakışta sistem

### 3a. Derleme duvarı ve tıklamanın yolu

Kutular DERLEME BİRİMİ, oklar REFERANS. Klasörler bu ağacı kurmuyor:
`Core/Combat/` diskte `Core`'un İÇİNDE ama ad alanı `GridStrategy.Combat` —
yani `Core`'un KARDEŞİ.

```
   fare tıklaması
        │
        ▼
┌─ EKRAN VE GİRDİ ──────────────────────────────────────────────────────┐
│  GridStrategy.Unity   noEngineReferences: FALSE                        │
│  referansları: Core · Combat · Battle   (autoReferenced: true — tek)   │
│   BoardAdapter.Update                                                  │
│     ① AdvanceBattleTime()   ── her kare, tıklama olsun olmasın         │
│     ② ██ isPlacingStructure ? ██  GİRDİ AKIŞI BURADA İKİYE AYRILIYOR   │
│          AÇIK   → UpdatePlacement → FeedGesture → PointerGesture       │
│          KAPALI → GetMouseButtonDown → HandleClick                     │
│     ③ TryReadPointerCell:  ekran pikseli → dünya noktası → HÜCRE       │
│     ④ ██ NİYET ██  dolu = SALDIR · boş = HAREKET · kendisi = BIRAK     │
│   UnitView.SetState / SetSelected   ◄── dönüş yolu (aşağıdan)          │
└───────────────────────┬───────────────────────────────▲───────────────┘
       int x, int y     │                               │ Unit + UnitState
       Unit             │                               │
════════════════════════╪═══════════════════════════════╪══════════════════
  ██ DUVAR ██  bu çizginin ALTINDA `UnityEngine` adı ÇÖZÜLMEZ
               duvarı geçen tek şey: sayılar, kimlikler ve enum değerleri
════════════════════════╪═══════════════════════════════╪══════════════════
                        ▼                               │
┌─ AKIŞ ────────────────────────────────────────────────┼───────────────┐
│  GridStrategy.Battle   noEngineReferences: TRUE       │               │
│  referansları: Core · Combat                          │               │
│  ██ CORE İLE COMBAT'I AYNI ANDA GÖREN İLK VE TEK KATMAN ██            │
│   BattleActions.Attack / Move / Revive / PlaceStructure               │
│     ADIM 0-1  çağıran hataları  ──► istisna           │               │
│     ────── ██ ÇİZGİ ██ ──────                         │               │
│     ADIM 2-5  kural soruları    ──► sonuç enum'u      │               │
│     ADIM 6-7  TEK YAZMA + sıra devri                  │               │
│   Battle  (kim nerede · kim hangi savaşçı) ──── UnitStateChanged ─────┘
│   TurnState (sıra kimde) · TurnRules (sıradan ne çıkar)               │
└──────────┬──────────────────────────────────────┬─────────────────────┘
           │ UnitGrid, Unit, MoveAction,          │ Combatant, Structure,
           │ GridDistance, MoveOutcome            │ Team, UnitState, AttackAction
           ▼                                      ▼
┌─ KONUM ─────────────────────────┐  ┌─ SAVAŞ ────────────────────────────┐
│ GridStrategy.Core               │  │ GridStrategy.Combat                │
│ references: []  noEngine: true  │  │ references: []  noEngine: true     │
│ Unit · UnitGrid · MoveAction    │  │ Combatant · Structure · Health     │
│ GridDistance · MoveProfile      │  │ UnitLifecycle · StructureLifecycle │
│ MoveOutcome · PointerGesture    │  │ TargetingRules · AttackAction ...  │
└─────────────────────────────────┘  └────────────────────────────────────┘
              ▲                                      ▲
              └──── ██ BU İKİSİ BİRBİRİNİ GÖRMEZ ██ ─┘
                    kanıt: iki BOŞ `references` listesi
```

Duvarın kaldırılması hâlinde ne olurdu ve dört somut faturası →
[konular/02-assembly-duvari.md](konular/02-assembly-duvari.md)

### 3b. Kalıtım ve içerme — AYRI iki ilişki, ayrı iki figür

Bu ikisi karıştırıldığında sistem yanlış okunur. **Kalıtım** "bir tür"dür ve
seçmeli değildir: devralınan her şey gelir. **İçerme** "bir parçası"dır ve
seçmelidir: hangi parçanın alınacağına sahip karar verir. İkisi ayrı çizilir.

```
KALITIM ( `:` ile yazılan )        — bütün projede TOPLAM İKİ satır

   UnityEngine.MonoBehaviour        ← motorun kare düzenine bağlanma yeteneği:
        ├── BoardAdapter               Awake / OnEnable / Update / OnDisable
        └── UnitView                   ve Inspector'da serileştirilme

   ██ BAŞKA HİÇBİR TİP HİÇBİR ŞEYDEN TÜREMİYOR ██  34 tipin 32'si düz sınıf,
   static sınıf ya da enum. `Structure : Combatant` bilerek YAZILMADI (→ B2).
```

```
İÇERME ( alan/property olarak tutulan )   — sistemin asıl iskeleti

   Combatant                       Structure
      ├── Health                      ├── Health
      ├── UnitLifecycle               ├── StructureLifecycle
      ├── AttackProfile               ├── Team
      └── Team                        └── AttackProfile  ██ null OLABİLİR ██
                                           yapıların ÇOĞU saldırmaz
   Battle                          BoardAdapter
      ├── UnitGrid  (internal)        ├── Battle
      ├── Dict<Unit, Combatant>       ├── Dict<Unit, UnitView>
      ├── Dict<Unit, Structure>       ├── PointerGesture
      ├── Dict<Unit, Action<...>>     └── UnityEngine.Grid
      └── TurnState                        (GetComponent ile bulunur)

   ██ ORTAK OLAN TEK TİP: Health ██
   Combatant ile Structure'ın paylaştığı tek şey bu — ve bu bir tesadüf değil,
   Structure'ın varlığıyla SINANAN iddia: can kuralı tipten bağımsızsa, bir
   barakanın canı bir askerin canıyla aynı sınıfla tutulabilmelidir.
```

Bir düz C# nesnesi bir `GameObject`'e **"eklenmez"**, ona bir alan üzerinden
REFERANSLA tutulur; eklenebilmesi için tipin `Component`'ten türemesi gerekir ve
bu projede yalnızca iki tip öyle. Sonucu: `Combatant` sahne kapandığında Unity
tarafından yok edilmez, onu tutan `Battle` bırakınca çöp toplayıcıya kalır.

---

## 4. Kadro — her tip *bilir* ve **BİLMEZ**

Hikâyeyi ilginç kılan bilmedikleridir. Omurgadaki on iki tip aşağıda (on bir
kutuda — sıra ikilisi tek kutuyu paylaşıyor); kalan yirmi bir tip, yani
33 belgelenmiş tipin geri kalanı → [kod/README.md](kod/README.md).

```
╔═ Unit ═══════════════════════ Core ═══════════════════════════════╗
║ bilir  : adını                                                     ║
║ BİLMEZ : nerede durduğunu · canını · tarafını · ASKER Mİ YAPI MI   ║
║ ölçüsü : tipte çağrılabilir tek bir metot yok                      ║
╠═ UnitGrid ═══════════════════ Core ═══════════════════════════════╣
║ bilir  : hangi hücrede hangi kimlik · sınırlarını                  ║
║ BİLMEZ : tuttuğu şeyin ne olduğunu · canını · tarafını · sırayı    ║
╠═ UnitLifecycle ══════════════ Combat ═════════════════════════════╣
║ bilir  : Alive / Downed / Dead · kalan saniye                      ║
║ BİLMEZ : kimin sayacı olduğunu · canı · tarafı · tahtayı · SAATİ   ║
║ ölçüsü : Tick(float) saniyeyi DIŞARIDAN alır; Time.deltaTime yok   ║
╠═ Combatant ══════════════════ Combat ═════════════════════════════╣
║ bilir  : can · taraf · saldırı tanımı · yaşam döngüsü              ║
║ BİLMEZ : ██ KENDİ KİMLİĞİNİ ██ ve nerede durduğunu                 ║
║ sonucu : B6'daki bütün olay zinciri bu tek eksiklikten doğuyor     ║
╠═ Structure ══════════════════ Combat ═════════════════════════════╣
║ bilir  : can · taraf · ayakta/yıkık · (varsa) saldırı tanımı       ║
║ BİLMEZ : Combatant'ı — ondan TÜREMEZ; ortak tek tip Health         ║
║ eksiği : olayı YOK · diriltmesi YOK · düşme hâli YOK               ║
╠═ TargetingRules ═════════════ Combat ═════════════════════════════╣
║ bilir  : iki durum dili (UnitState, StructureState) · iki taraf    ║
║ BİLMEZ : tahtayı · mesafeyi · sırayı · canı                        ║
║ ölçüsü : girdisi yalnız enum; sınamak için ne sahne ne tahta gerek ║
╠═ TurnState + TurnRules ══════ Battle ═════════════════════════════╣
║ TurnState : sıra hangi tarafta · kaçıncı turdayız — ama bunun NE   ║
║             ANLAMA geldiğini BİLMEZ                                ║
║ TurnRules : "bu taraf şu an eyleyebilir mi" — ██ TAHTAYI ██, hangi ║
║             birim olduğunu (yalnız TARAFINI görür), hedefi BİLMEZ  ║
╠═ Battle ═════════════════════ Battle ═════════════════════════════╣
║ bilir  : kim nerede · kim hangi savaşçı · sıra kimde               ║
║ BİLMEZ : ekranı · sprite · rengi · animasyonu · KURALLARI          ║
╠═ BattleActions ══════════════ Battle ═════════════════════════════╣
║ bilir  : hangi kuralı hangi SIRAYLA soracağını                     ║
║ BİLMEZ : ██ HİÇBİR KURALIN METNİNİ ██ — mesafe Chebyshev mi, hasar ║
║          nasıl hesaplanır, hedef neden uygun                       ║
║ ölçüsü : buradaki her `if` bir kuralı SORAR; hiçbiri kural YAZMAZ  ║
╠═ BoardAdapter ═══════════════ Unity ══════════════════════════════╣
║ bilir  : Input · Camera · Grid · Time · sahne · seçili birim ·     ║
║          tıklamanın NİYETİ                                         ║
║ BİLMEZ : jestin ne olduğunu · hamlenin GEÇERLİ olup olmadığını     ║
╠═ UnitView ═══════════════════ Unity ══════════════════════════════╣
║ bilir  : kendi iki SpriteRenderer'ını                              ║
║ BİLMEZ : Unit tipini · seçili olup olmadığını — "seçili miyim" diye║
║          SORULACAK bir yeri yok, kendisine SÖYLENİR                ║
╚════════════════════════════════════════════════════════════════════╝
```

### Kutunun en pahalı satırının GERÇEK SATIRLAR tarafındaki karşılığı

██ Bu kutuda on bir gözlem var; kaynağa tek bir satır bağlanacaksa hangisi? ██
Seçimi kutunun kendisi söylüyor — `Combatant` satırındaki
«sonucu : B6'daki bütün olay zinciri bu tek eksiklikten doğuyor».

**PROJE TİPİ: `Combatant`'ın eksiği, kaynakta** — `Assets/Game/Battle/Battle.cs` → `AddUnit`

```csharp
Action<UnitState, UnitState> forwarder =
    (previous, next) => UnitStateChanged?.Invoke(unit, previous, next);
```

██ EN ÖĞRETİCİ SEÇİMİ ██ — bu satır **`Combatant.cs`'te değil**, ve seçilme sebebi
tam olarak bu: kutudaki «`Combatant` BİLMEZ : ██ KENDİ KİMLİĞİNİ ██» satırının
karşılığı, `Combatant`'ta **olmayan** bir alan. Bir yokluğun kaynakta satırı
olmaz; onun yerine, o yokluğu telafi eden satır gösterilir. Burada olan şu:
`Combatant.StateChanged` yalnız «bir durum değişti» diyebiliyor, «HANGİ birim»
diyemiyor; eksik kimliği bu kapanış dışarıdan ekliyor — `unit`i yakalayıp
`UnitStateChanged`e üç parametreyle geçiriyor.

Aynı satır kutunun iki gözlemini daha aynı anda kapatıyor: `UnitGrid` satırındaki
«BİLMEZ : tuttuğu şeyin ne olduğunu» (tahta kimliği tutar, savaşçıyı bilmez) ve
`Battle` satırındaki «bilir : kim nerede · kim hangi savaşçı» — ikisini
buluşturan tek yer burası. ██ Kutuda üç ayrı satır olarak duran şey, kaynakta tek
bir ifade. ██

Mekanizmanın tamamı (`Target`/`Method`, kapanışın neden saklandığı, sökülmezse ne
olduğu): [dil/06-delege-arka-taraf.md](dil/06-delege-arka-taraf.md) ve
[konular/01-olay-zinciri.md](konular/01-olay-zinciri.md).

---

## 5. Okuma sırası — hangi soru hangi dosyaya gider

Numaraların önerdiği öğrenme sırası [README.md](README.md)'de yazılı; burada
eksik olan eksen veriliyor: **soru**.

| Aklındaki soru | Git |
|---|---|
| "Oyun ne, ekranda ne oluyor" | bu dosya, §1 |
| "Neden `Core` Unity'yi görmüyor, bunun bedeli ne" | [konular/02-assembly-duvari.md](konular/02-assembly-duvari.md) |
| "`CS0118` aldım, `Battle` neden çözülmüyor" | [konular/02-assembly-duvari.md](konular/02-assembly-duvari.md) |
| "Tahtaya kim yazabilir, `readonly` neyi korumuyor" | [konular/03-tahta-sahipligi.md](konular/03-tahta-sahipligi.md) |
| "Aynı tıklama neden bazen başka sebeple reddediliyor" | [konular/04-karar-sirasi.md](konular/04-karar-sirasi.md) |
| "Birim neden hemen ölmüyor, yapının ikizi neden eksik" | [konular/05-yasam-dongusu.md](konular/05-yasam-dongusu.md) |
| "Neden `bool` değil enum, sıfırıncı değer neden ret" | [konular/06-sonuc-enumlari.md](konular/06-sonuc-enumlari.md) |
| "Tıklama nereye gidiyor, sürükleme nerede ayrılıyor" | [konular/07-tiklamadan-eyleme.md](konular/07-tiklamadan-eyleme.md) |
| "`Battle`'daki o sözlük neden var, olay nasıl akıyor" | [konular/01-olay-zinciri.md](konular/01-olay-zinciri.md) |
| "Bu tip neden var, bu ÜYE neden böyle yazılmış" | [kod/README.md](kod/README.md) → tipin ayna belgesi |
| "`readonly` / `IReadOnlyList` / `nameof` ne vaat ediyor" | [dil/README.md](dil/README.md) |
| "Kapanış (closure) kimliği nedir, `-=` neden sessizce başarısız olur" | [dil/04-delege-olay-ve-kapanis.md](dil/04-delege-olay-ve-kapanis.md) |

**İlk kez geliyorsan önerilen yol:** bu dosya baştan sona →
[02](konular/02-assembly-duvari.md) (sınırlar) →
[03](konular/03-tahta-sahipligi.md) (sahiplik) →
[07](konular/07-tiklamadan-eyleme.md) (uçtan uca tek akış). Bu dördü bittiğinde
geri kalan her şey referans hâline gelir; sırayla okunması gerekmez.

---

## 6. Aynı basınç, üç başka oyun

Sabit üçlü: **Slay the Spire** (sıra tabanlı kart oyunu) · **Vampire Survivors**
(gerçek zamanlı, aynı anda binlerce varlık) · **Stardew Valley** (gerçek zamanlı
sim + kayıt/yükleme). Tabloda **yalnızca ad ve iş** var; rol etiketi (çevirmen,
bileşik, kural, varlık) BİLEREK yazılmadı — onu okuyucu atayacak, sonra kendine
şunu soracak: *"bunlardan hangisi bizim `Battle`'ımız?"*

> **Ad kaynağı dürüstçe:** Slay the Spire ve Stardew Valley için kullanılan
> adlar o oyunların açık mod arayüzünden bilinen gerçek tip adlarıdır. Vampire
> Survivors'ın kodu kapalı ve tip adları kamuya açık değil; oradaki hücrelerde
> oynanışta GÖRÜNEN mekanizmanın adı yazıyor, uydurulmuş bir sınıf adı değil.

| Bizde — adı ve işi | Slay the Spire | Vampire Survivors | Stardew Valley |
|---|---|---|---|
| `UnitGrid` — hangi hücrede ne duruyor, tek defter | ██ EŞLEŞMİYOR ██ ızgara yok; "yer" bir koordinat değil bir DESTE üyeliğidir (`AbstractDungeon` destelerinin arasında yer değiştirme) | ██ EŞLEŞMİYOR ██ konum sürekli bir sayı çifti; "kim nerede" sorusu bir uzamsal indeksle cevaplanır ve o indeks OTORİTE değil ÖNBELLEKTİR | `GameLocation` — bir yerin döşemesi ve üstündeki nesne sözlüğü |
| `Unit` — tahtada yer kaplayan kimlik | `AbstractCreature` — sahada duran şeyin kimliği | ekrandaki varlık kimliği | `Character` / `SObject` — bir şeyin dünyadaki kimliği |
| `Combatant` — can, taraf, saldırı tanımı bir arada | `AbstractMonster` · `AbstractPlayer` — ██ EŞLEŞMİYOR: BİZDE BİR TİP, ORADA İKİ ██ oyuncunun eli ve enerjisi var, canavarın niyet göstergesi | düşman varlığı — can + hız + temas hasarı | `Farmer` · `Monster` — burada da ikiye ayrılmış |
| `Health` — can sayacı, tavana kelepçeli | `AbstractCreature.currentHealth` + `maxHealth` | can çubuğu | `Farmer.health` / `stamina` — ██ İKİ AYRI SAYAÇ ██ bizde yalnız biri var |
| `UnitLifecycle` — üç durum ve iki geri sayım | ██ EŞLEŞMİYOR ██ 0 cana inen canavar ANINDA sahadan kalkar; "düşmüş ama kurtarılabilir" penceresi yok | ██ EŞLEŞMİYOR ██ 0 canda düşman anında yok olur; ölüm bir SÜREÇ değil bir AN | oyuncunun bayılması — ██ YARIM EŞLEŞME ██ tam olarak bizim `Downed`'ımız (uyanırsın, bedel ödersin) ama yalnız OYUNCUDA var, her varlıkta değil |
| `TargetingRules` — bu hedefe uygulanabilir mi | `AbstractCard.canUse` — bu kart bu hedefe oynanabilir mi | ██ EŞLEŞMİYOR ██ hedef seçimi YOK; silah menzile gireni otomatik vurur, "uygun mu" sorusunu soran bir yer yok | araç–nesne uyumu (balta ağaca, kazma taşa) |
| `TurnState` + `TurnRules` — sıra kimde, sıradan ne çıkar | `GameActionManager` + enerji bütçesi — sıra ve harcanabilir kaynak | ██ HİÇ YOK ██ gerçek zamanlı; "sıra" kavramının yerini SOĞUMA SÜRESİ alır. En öğretici satır bu: sıra kavramını silersen `BattleActions`'taki `EndTurn` çağrıları da silinir, geri kalan iskelet AYNI KALIR | `Game1` saati + gün sayacı — sıra yok ama GÜN var; hareket bütçesini enerji tutar |
| `MoveOutcome` / `AttackOutcome` — reddin sebebini adlandır | ██ EŞLEŞMİYOR ██ oynanamayan kart zaten OYNANAMAZ (arayüz izin vermez); ret bir DEĞER değil bir engellemedir | ██ EŞLEŞMİYOR ██ reddedilecek bir eylem yok; oyuncu yalnızca yürür | araç kullanımının başarısız olması — ama sebebi bir değer olarak DÖNMEZ, ekranda gösterilir |
| `BattleActions` — kuralları sıraya dizen tek akış | `AbstractGameAction` + kuyruk — her etki sıraya girer ve sırayla çözülür | çarpışma çözümleyicisi — her kare, herkes için | araç kullanımı akışı |
| `BoardAdapter` — tıklamayı niyete çevirir, ekranı günceller | `AbstractDungeon` ekran katmanı + kart sürükleme | girdi + çizim döngüsü | `Game1.Update` — ██ ORADA TEK BİR TİP, BİZDE DE TEK ██ ve orada da aynı koku: girdi, çizim ve oyun kuralı aynı yerde |
| kayıt / yükleme — bizdeki karşılığı **HENÜZ YOK** | oyun-içi kaydetme ve devam | çalışma başına ilerleme | tam kayıt dosyası — dünyanın her nesnesi diske yazılır |

### Eşleşmeyen satırlar neden en öğretici olanlar

**Sıra kavramı (Vampire Survivors'ta HİÇ YOK).** `TurnState` ile `TurnRules`'un
neden AYRI olduğunun en güçlü kanıtı. Sıra bir DURUM, sıradan ne çıktığı bir
KURAL. Gerçek zamanlı bir oyunda durum silinir, kural ise "şu an eyleyebilir mi"
sorusu olarak soğuma süresiyle YAŞAR. Tek tipte birleştirilseydi bu
taşınabilirlik görünmezdi.

**`UnitLifecycle`'ın üç durumu (üç oyunun ikisinde YOK).** Slay the Spire ve
Vampire Survivors'ta ölüm bir AN; yalnızca Stardew Valley'de bir SÜREÇ, orada da
yalnız oyuncuda. "Üç durum daha iyidir" diye bir kural YOK — bizde üç durum var
çünkü diriltme penceresi bir OYNANIŞ kararı.

**`UnitGrid` (üç oyunun ikisinde yok).** Izgara zorunluluk değil TERCİH. Vampire
Survivors'ın uzamsal indeksinden keskin ayrılıyoruz: orada indeks yeniden inşa
edilebilir bir önbellek, bizde tahta TEK DOĞRULUK KAYNAĞI. Ölçüsü: tahtamız 15
hücre ve `Battle.Tick` iki sözlüğü doğrudan dolaşıyor — binlerce varlıkta bu
döngü çalışmazdı ve o gün bir indeks doğardı.

**Kayıt/yükleme (bizde HENÜZ YOK).** Basıncın adı konmuş: bir savaşı kapatıp
aynı yerden devam ettirmek istendiği gün `Battle`'ın üç sözlüğü ve `TurnState`
diske yazılabilir olmak zorunda. İlk çarpacağı duvar `Unit` kimliğinin REFERANS
EŞİTLİĞİ olması — diskten okunan `Unit`, yazılan ile aynı NESNE olmayacak. O iki
oyun bunu kalıcı bir kimlik numarasıyla çözüyor; bizde öyle bir numara yok.

---

## 7. Üç ağacın nasıl okunacağı

Üç eksen **dik**: `kod/` tip başına ("bu üye neden böyle"), `konular/` mekanizma
başına ("bu akış nasıl çalışıyor"), `dil/` kavram başına ("bu ödünç alınan şey ne
vaat ediyor"). Aynı `readonly` dokuz dosyada geçtiği için tip başına bir yerleşim
onu dokuz kez tekrar ederdi; aynı olay zinciri dört dosyayı kat ettiği için
mekanizma tek bir tipin belgesine sığmazdı. Eşik tablosu ve koddan buraya nasıl
gelineceği → [README.md](README.md).

---

## Bu dosyanın sınırı

Burada **hiçbir mekanizma tam olarak anlatılmadı** ve bu bir eksiklik değil, bu
dosyanın tanımı: harita, arazi değil. Bir figürün ayrıntısını merak ettiğin anda
§5'teki tabloya dön.

Otorite sırası her yerde aynı: **kod kazanır.** Bu dosya ile üç ağaç çelişirse
ağaçlar; ağaçlarla kod çelişirse kod. Buradaki her sayı yazılırken kaynağa karşı
doğrulandı — doğrulanmamış bir sayı üslup hatası değil, kusurdur.

---

## ██ SIRADAKİ ADIM ██

Bu dosya okuma yolunun **1. adımıydı**. Sıradaki adım
[`konular/02-assembly-duvari.md`](konular/02-assembly-duvari.md) —
██ `konular/01` DEĞİL. ██

> **NEDEN 02:** `01`'in taşıyıcı gerekçesi (*"`Combatant` bir `Unit` alsaydı
> `GridStrategy.Combat` ad alanı `GridStrategy.Core`'a bağlanırdı"*) `02`'de
> tanımlanan üç kavrama dayanıyor: *klasör*, *ad alanı* ve *assembly* üç **ayrı**
> şey. `01`'i önce okursan en kritik cümlesi havada kalır.
>
> **DOSYA NUMARALARI SIRA DEĞİL, KİMLİKTİR.** Yeniden numaralandırma dört makine
> kapısını ve otuzdan fazla çapayı kırardı. Sıranın tamamı ve her adımın ön
> koşulu: [`../ogrenme/00-okuma-sirasi.md`](../ogrenme/00-okuma-sirasi.md)
> — 15 adım, 5 oturum, 6 koşturma noktası.

`konular/01` o yolda **onuncu** sırada durur: üç ipliğin düğümlendiği yer, giriş
değil.
