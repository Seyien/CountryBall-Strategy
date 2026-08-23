# Assembly duvarı — kim kimi göremez, ve bunun bedeli ne

> **NEREDE GEÇİYOR** — *bu mekanizmanın kat ettiği kaynak dosyalar.* Önce dört
> `.asmdef` — ██ duvarın tamamı bu dört JSON dosyasında yazılı ██:
> `Assets/Game/Core/GridStrategy.Core.asmdef` · `Assets/Game/Core/Combat/GridStrategy.Combat.asmdef`
> · `Assets/Game/Battle/GridStrategy.Battle.asmdef` · `Assets/Game/Unity/GridStrategy.Unity.asmdef`
> sonra faturayı ödeyen `.cs` dosyaları, akış sırasıyla:
> `Assets/Game/Core/Combat/AttackResolver.cs` → `Assets/Game/Core/MoveOutcome.cs`
> → `Assets/Game/Battle/Battle.cs` → `Assets/Game/Unity/BoardAdapter.cs`
>
> **NE ZAMAN OKU** — *hangi soruyu sorduğunda ya da hangi değişikliğe giriştiğinde:*
> bir `asmdef`'in `references` dizisine bir ad eklemeye niyetlendiğinde, yeni bir
> tipi hangi klasöre koyacağını sorduğunda, ya da derleyici sana CS0118 dediğinde.

**BURAYA KODDAN GELDİYSEN** — aşağıdaki üyelerin **yorumunda** bu belgeye bir
`DERİN ANLATIM:` işaretçisi var. Yol: `Ctrl+P` → dosya adının ayırt edici
parçasını yaz → `Ctrl+F` ile **üye adını** ara. ██ Satır numarası bilerek
yazılmıyor: satır kayar, üye adı kaymaz. ██

| dosya | üye | koddan işaretçi |
|---|---|---|
| `Assets/Game/Core/Combat/AttackResolver.cs` | `IsWithinRange` (`distance` parametresi) | ✓ |
| `Assets/Game/Core/Combat/AttackAction.cs` | `Execute` | ✓ |
| `Assets/Game/Core/Combat/AttackProfile.cs` | `AttackProfile` (tip başlığı) | ✓ |
| `Assets/Game/Core/MoveOutcome.cs` | `MoveOutcome` (tip başlığı; anlatılan değer `RejectedActorCannotAct`) | ✓ |
| `Assets/Game/Core/MoveProfile.cs` | `MoveProfile` (tip başlığı — Core'da durma kararı) | ✓ |
| `Assets/Game/Battle/Battle.cs` | `Battle` (tip başlığı — var olma sebebi) | ✓ |
| `Assets/Game/Unity/BoardAdapter.cs` | `using Battle = global::…` alias satırı | ██ HENÜZ YOK ██ |

██ **"HENÜZ YOK" ne demek:** o satır burada gerçekten anlatılıyor, ama **kodun
yorumunda buraya geri getiren bir işaretçi yok** — o satırdan yola çıkıp bu
belgeye ulaşamazsın, yalnız tersi çalışır. ██

---

## Sahne

Oyuncu bir askere tıklıyor, yanındakine tıklıyor. Vuruş oluyor.

Sonuna kadar sadeleştirilmiş hâli tek satır:

```csharp
AttackAction.Execute(attacker, target, distance);
```

Şu `distance`'a bak. Bir `int`. Neden dışarıdan geliyor? Saldırı kuralı iki
askerin nerede durduğunu görüyor; iki koordinatı çıkarıp bir mutlak değer almak
üç satır. Neden almıyor?

Cevap tembellik değil, üslup da değil. **Alamıyor.** O üç satırı yazmayı denesen
derleyici tipin adını tanımaz. Aradaki şey bir duvar ve duvarın nerede durduğu
tek bir yerde yazılı: bir JSON dosyasının boş bir dizisinde.

Bu dosya o duvarın **neyi engellediğini**, **neyi engellemediğini** ve
karşılığında bu projenin **ne ödediğini** anlatıyor.

---

## Karakterler

Bu hikâyenin karakterleri sınıf değil, **derleme birimi** — dört tane. Her
birinin bildikleri ve bilmedikleri `references` dizileri tarafından belirleniyor,
başka hiçbir şey tarafından değil.

```
╔═ GridStrategy.Core ═══════════════════════════════════════════╗
║  references : []            noEngineReferences : true         ║
║  İşi     : tahta. Hücre, koordinat, uzaklık, sınır, hareket   ║
║  Bilir   : Unit, UnitGrid, GridDistance, MoveAction,          ║
║            MoveProfile, MoveOutcome, PointerGesture           ║
║  BİLMEZ  : can, hasar, taraf, yaşam döngüsü, sıra, motor      ║
╚═══════════════════════════════════════════════════════════════╝

╔═ GridStrategy.Combat ═════════════════════════════════════════╗
║  references : []            noEngineReferences : true         ║
║  İşi     : savaş. Can, hasar, taraf, menzil, yaşam döngüsü    ║
║  Bilir   : Combatant, Health, AttackProfile, AttackResolver,  ║
║            TargetingRules, MovementRules, Team, UnitState     ║
║  BİLMEZ  : ██ TAHTAYI ██ hücre yok, koordinat yok, uzaklık    ║
║            yok. Ve sıra yok. Ve motor yok.                    ║
╚═══════════════════════════════════════════════════════════════╝

╔═ GridStrategy.Battle ═════════════════════════════════════════╗
║  references : [ Core, Combat ]   noEngineReferences : true    ║
║  İşi     : birleştirmek. İki kutuyu AYNI ANDA gören ilk yer   ║
║  Bilir   : Unit ↔ Combatant / Structure eşlemesi, sıra        ║
║  BİLMEZ  : motoru. MonoBehaviour, Sprite, Vector2Int          ║
╚═══════════════════════════════════════════════════════════════╝

╔═ GridStrategy.Unity ══════════════════════════════════════════╗
║  references : [ Core, Combat, Battle ]                        ║
║                            noEngineReferences : ██ FALSE ██   ║
║  İşi     : çevirmenlik. Motor burada başlıyor                 ║
║  Bilir   : MonoBehaviour, SerializeField, Grid, fare          ║
║  BİLMEZ  : tek bir oyun kuralı bile yazmaz                    ║
╚═══════════════════════════════════════════════════════════════╝
```

### ██ KUTULARIN GERÇEK SATIR KARŞILIĞI ██

██ **Bu dördü AKTÖR kutusudur, tip kutusu değil.** ██ Hiçbirinin bir `.cs` tanım
satırı **yok** ve aranmamalı: bir assembly bir sınıf gibi bildirilmez, bir JSON
dosyasıyla kurulur. Bu yüzden aşağıda her kutu için **iki** satır var — kilidi
**kuran** satır (`.asmdef`) ve o kilidin faturasını **ödeyen** satır (`.cs`).
Kutunun iddiası ancak ikisi yan yana konunca ölçülebiliyor. ██ Satır numarası
bilerek yazılmıyor: satır kayar, anahtar adı kaymaz. ██

**`GridStrategy.Core` bu projede** — kilidi kuran satır:
`Assets/Game/Core/GridStrategy.Core.asmdef` → `references`

```json
  "references": [],
```

Faturayı ödeyen satır: `Assets/Game/Core/MoveOutcome.cs` → `RejectedActorCannotAct`

```csharp
        /// <summary>
        /// Hareket eden şu an eylem yapamaz: sırası değil ya da durumu
        /// elvermiyor (<c>MovementRules.CanMove</c> — bu tipin GÖREMEDİĞİ bir
        /// kural). Bu değeri yalnızca <c>GridStrategy.Battle</c> katmanı üretir.
        /// </summary>
        RejectedActorCannotAct
```

Kutudaki «BİLMEZ : can, hasar, taraf, yaşam döngüsü, sıra, motor» satırının en
pahalı sonucu bu: `MoveOutcome` Core'da **tanımlı**, ama bu değerin cevabını
veren `MovementRules` Combat'ta. Üstteki boş dizi o adı burada aranamaz kılıyor,
dolayısıyla tip kendi değerini üretemiyor. Kutunun "bilmez" listesi bir üslup
değil, bir enum değerinin doğum yerini kaydıran bir kısıt.

**`GridStrategy.Combat` bu projede** — kilidi kuran satır:
`Assets/Game/Core/Combat/GridStrategy.Combat.asmdef` → `references`

```json
  "references": [],
```

Faturayı ödeyen satır: `Assets/Game/Core/Combat/AttackAction.cs` → `Execute`

```csharp
        public static AttackOutcome Execute(Combatant attacker, Combatant target, int distance)
```

Kutudaki «BİLMEZ : ██ TAHTAYI ██ hücre yok, koordinat yok, uzaklık yok» satırı
bu imzada tek bir kelimeye iniyor: `int distance`. Uzaklık **ölçülmüş** olarak
geliyor; ölçen tip Core'da ve üstteki boş dizi onun adını bu dosyada aranamaz
kılıyor. Duvarı geçen şeyin bir sayı olduğu, gözle görülebilir biçimde, bu
parametre listesinde yazılı.

**`GridStrategy.Battle` bu projede** — kilidi kuran satır:
`Assets/Game/Battle/GridStrategy.Battle.asmdef` → `references`

```json
  "references": [
    "GridStrategy.Core",
    "GridStrategy.Combat"
  ],
```

Faturayı ödeyen satır — burada fatura değil **kazanç**:
`Assets/Game/Battle/Battle.cs` → `combatants`

```csharp
        private readonly Dictionary<Unit, Combatant> combatants =
            new Dictionary<Unit, Combatant>();
```

Kutudaki «İki kutuyu AYNI ANDA gören ilk yer» satırının karşılığı bu tek
bildirimde görülebiliyor: `Unit` `GridStrategy.Core`'un tipi, `Combatant`
`GridStrategy.Combat`'ın tipi ve ikisi aynı satırda yan yana **yazılabiliyor**.
Üstteki iki adı `references` dizisinden silersen derlenmeyen ilk satır bu olur.

**`GridStrategy.Unity` bu projede** — kilidi kuran satır:
`Assets/Game/Unity/GridStrategy.Unity.asmdef` → `noEngineReferences`

```json
  "noEngineReferences": false
```

Faturayı ödeyen satır: `Assets/Game/Unity/BoardAdapter.cs` → `using Battle` alias'ı

```csharp
    using Battle = global::GridStrategy.Battle.Battle;
```

Kutudaki «noEngineReferences : ██ FALSE ██» satırı motorun projeye girdiği tek
kapı — üç kutuda `true`, yalnız burada `false`. Alttaki alias ise aynı kutunun
`references` dizisine `GridStrategy.Battle` yazmanın kesilen faturası: tek bir
`Battle` kelimesi hem bir ad alanını hem onun içindeki sınıfı adlandırıyor,
çıplak yazıldığında ad alanı kazanıyor ve derleyici **CS0118** veriyor. Kutunun
"çevirmenlik" işi burada bir satırlık bakım borcuna dönüşüyor; gerekçesinin
tamamı o satırın hemen üstünde, kodun kendi yorumunda.

██ **DERLEYİCİNİN KENDİSİ İÇİN AYRI BİR KUTU YOK — ve olmamalı.** ██ Yukarıdaki
dört kutunun hiçbiri derleyiciyi adlandırmıyor, ama dördünün de uygulayıcısı o.
Derleyicinin bu projedeki gözlemlenebilir karşılığı bir dosya ya da üye değil,
**bir hata kodu**: `CS0118`. Nerede doğduğu yazılı — `BoardAdapter.cs`'teki
alias satırı dosyanın **başına**, öteki `using`'lerin yanına taşınırsa geri
gelir. Metin harfi harfine aynı, sonuç zıt.

Dört kutunun en tuhaf yanı ilk ikisinde: **Core ile Combat kardeş.** Ne biri
diğerini görüyor, ne de tersi. İkisinin de `references` dizisi harfi harfine
`[]`.

Ve ikinci tuhaflık: `Assets/Game/Core/Combat/` klasörü diskte **Core'un
içinde**. Aynı klasörde yaşayan iki assembly, birbirini görmeyen iki kutu.

**Bütün hikâye bu iki cümleden doğuyor.** Aklında tut.

---

## Üç ayrı şey, üç ayrı iş

Aynı dosyayı — `Assets/Game/Core/Combat/AttackProfile.cs` — üç ayrı gözle oku.
Üçü de sana farklı bir cevap veriyor:

```
                     AttackProfile.cs

  ① KLASÖR            Assets/Game/Core/Combat/
                      ╰─► "Core'un İÇİNDEyim"

  ② AD ALANI          namespace GridStrategy.Combat
                      ╰─► "Core'un KARDEŞİyim"      ◄── ██ AYRIŞMA ██

  ③ ASSEMBLY          GridStrategy.Combat.asmdef
                      references: []
                      ╰─► "Core'u GÖRMÜYORUM"

  ① ile ② birbiriyle çelişiyor ve bu bir kaza DEĞİL:
  klasörün hiçbir şey üzerinde hükmü yok.
```

Üçünün kontrol ettiği şeyler ayrı:

```
  ne kontrol ediyor          kim uyguluyor         ihlal edilirse
  ─────────────────────      ────────────────      ────────────────
  ① klasör
     HİÇBİR ŞEY              kimse                 hiçbir şey olmaz
     (yalnız insanın gözü)                         ── derleme sürer

  ② ad alanı
     AD ÇÖZÜMLEME            derleyici             CS0118 / CS0246
     "bu kelime neyi                                ── derleme durur
      adlandırıyor"

  ③ assembly
     GÖRÜNÜRLÜK              derleyici             "tip bulunamadı"
     "bu ad aranabilir mi"                          ── derleme durur
```

Kanıtı tahmin değil, bu projede duruyor: `MoveProfile.cs`'in yer kararında
yazılı olduğu gibi, o dosyayı `Combat/` klasörüne taşımak **tek başına hiçbir
şeyi bozmaz**. Bozan şey ad alanının ya da asmdef'in değişmesi olur. Klasör bir
etiket, asmdef bir kilit.

> **⌨ KODU AÇ:** `Assets/Game/Core/Combat/AttackProfile.cs` → dosyanın en üstündeki
> `namespace` satırı; sonra yanındaki `Assets/Game/Core/Combat/GridStrategy.Combat.asmdef`
> → `references` dizisi
> **BAK:** dosya `Core/` klasörünün **içinde** duruyor, ad alanı `GridStrategy.Combat`
> diyor, asmdef `references: []` diyor. Üç cevap, üç ayrı soru — ve ikisi
> birbiriyle çelişiyor.
> **DÖNÜŞ:** bu dosyanın «Üç ayrı şey, üç ayrı iş» bölümü

> **◀ DÖNÜŞ:** [01-olay-zinciri.md](01-olay-zinciri.md#birinci-durak-sayac-konusuyor) — «Karakterler»den
> geldiysen artık şunu biliyorsun: `Combatant`'ın kendi kimliğini bilmemesi bir
> üslup tercihi değil — `GridStrategy.Combat`'in `references` dizisi boş olduğu
> için `Unit` adı orada **yazılamıyor** · oraya dön ve sayaçtan devam et

> **◀ DÖNÜŞ:** [../dil/01-degismezlik-anahtar-kelimeleri.md](../dil/01-degismezlik-anahtar-kelimeleri.md#serbest-secim-olan-ucu-ve-constun-assembly-sinirinda-yaptigi-sey) — «`const`'un assembly sınırında yaptığı şey»den
> geldiysen artık şunu biliyorsun: "assembly sınırı" bir klasör sınırı **değil**,
> `.asmdef`'in çizdiği ayrı bir derleme birimi sınırı — `const`'un pişirdiği
> değerin öteki tarafa **kopyalanmasının** sebebi bu · oraya dön ve serbest seçim
> olan üçünden devam et

---

## Duvarın engellediği şey: görünürlük

`references: []` şunu söylüyor: bu assembly'deki hiçbir dosya, öteki
assembly'deki hiçbir tipin **adını yazamaz**. Tam nitelenmiş yazsan da olmaz,
`global::` koysan da olmaz, `using` eklesen de olmaz. Ad orada yok.

```
  ┌─ GridStrategy.Core ──────┐        ┌─ GridStrategy.Combat ────┐
  │  Unit                    │        │  Combatant               │
  │  UnitGrid                │        │  Health                  │
  │  GridDistance            │        │  AttackProfile           │
  │  MoveAction              │        │  AttackResolver          │
  │  MoveOutcome             │        │  MovementRules           │
  │  MoveProfile             │        │  UnitState / Team        │
  └──────────────────────────┘        └──────────────────────────┘
              ▲    ✗ OK YOK ── iki yönde de ──  ✗ OK YOK    ▲
              └────────────────────┬───────────────────────┘
                                   │
                    ██ DURUŞ NOKTASI: her iki dizi de []  ██
                       yasak değil — İMKÂNSIZ. Engellenen
                       şey unutulabilir; doğmayan şey unutulamaz.
                                   │
              ┌────────────────────┴────────────────────┐
              │                                         │
  ┌─ GridStrategy.Battle ────┐        ┌─ GridStrategy.Unity ─────┐
  │  references:             │◄───────│  references:             │
  │    Core, Combat          │        │    Core, Combat, Battle  │
  │  Battle, BattleActions   │        │  BoardAdapter, UnitView  │
  │  TurnState, TurnRules    │        │  noEngineReferences:FALSE│
  └──────────────────────────┘        └──────────────────────────┘
```

Oklar **tek yönlü**. Yukarıdaki kutu aşağıyı görür, aşağıdaki yukarıyı asla.

Bu duvarın kestiği dört fatura var. Üçünü şimdi oku; dördüncüsü — duvarın en
sessizi — kendi bölümünde.

> **⌨ KODU AÇ:** `Assets/Game/Battle/GridStrategy.Battle.asmdef` → `references`,
> yanında `Assets/Game/Core/GridStrategy.Core.asmdef` → `references`
> **BAK:** `Battle`'ınki `Core` ile `Combat`'ı **sayıyor**, `Core`'unki hiçbir şey
> saymıyor. Okun tek yönlü olması bu iki JSON dosyasındaki iki dizi farkından
> ibaret — başka hiçbir yerde yazılı değil.
> **DÖNÜŞ:** bu dosyanın «Duvarın engellediği şey: görünürlük» bölümü

> **◀ DÖNÜŞ:** [04-karar-sirasi.md](04-karar-sirasi.md#birinci-durak-cizginin-kendisi) — «Karakterler»den
> geldiysen artık şunu biliyorsun: `AttackRules`'un sırayı, `MoveAction`'ın durumu
> **soramaması** nezaket değil — o tiplerin adı o assembly'de aranabilir bile
> değil · oraya dön ve çizginin kendisinden devam et

> **◀ DÖNÜŞ:** [03-tahta-sahipligi.md](03-tahta-sahipligi.md#besinci-durak-tahtanin-kendi-ic-sozlesmesi) — «Dördüncü durak: garantinin bittiği çizgi»den
> geldiysen artık şunu biliyorsun: `internal`'ın çizdiği çember **bu duvarla
> aynı** çember — "aynı assembly" cümlesindeki assembly tam olarak burada
> tanımlanan şey · oraya dön ve tahtanın kendi iç sözleşmesinden devam et

> **◀ DÖNÜŞ:** [07-tiklamadan-eyleme.md](07-tiklamadan-eyleme.md#faturanin-adi-editmodeda-sinanabilirlik) — «Dördüncü durak: duvar»dan
> geldiysen artık şunu biliyorsun: duvarın yasakladığı şey **veri** değil, o
> verinin adının aranabilirliği — `Vector3`'ün duvarı geçememesi de bu tek
> kuraldan · oraya dön ve faturanın adından devam et

---

### Birinci fatura: mesafe dışarıdan gelmek zorunda

`AttackResolver` menzil kuralının sahibi. İmzası şu:

```csharp
public static bool IsWithinRange(int distance, AttackProfile profile)
//                               ▲            ▲
//                          ÖLÇÜM        TANIM
//                     dışarıdan gelir   içeride üretilir
```

`distance` bir `int` olarak geliyor çünkü onu üreten `GridDistance`,
`GridStrategy.Core`'da yaşıyor ve **bu kutudan görünmüyor**. `AttackAction` da
aynı sebeple mesafeyi parametre alıyor — iki aşırı yüklemesinin ikisinde de
(`Combatant` hedefi ve `Structure` hedefi) son parametre `int distance`.

Sayı duvarı geçiyor, koordinat geçmiyor. Yolculuğu izle:

```
  oyuncu tıkladı
      │
      ▼
  BoardAdapter                             ── GridStrategy.Unity
      │   hücreyi bilir, kuralı bilmez
      ▼
  BattleActions.Attack(battle, attacker, target)
      │                                    ── GridStrategy.Battle
      │  ① battle'dan iki konumu bulur
      │  ② mesafeyi ölçtürür ──────────┐
      │                                ▼
      │                    GridDistance.Between(ax, ay, bx, by)
      │                                │      ── GridStrategy.Core
      │                                ▼
      │                          ╔═══════════╗
      │  ③ sonucu taşır ◄────────║ int = 2   ║
      ▼                          ╚═══════════╝
  AttackAction.Execute(attacker, target, distance)
      │                                    ── GridStrategy.Combat
      ▼
  AttackResolver.IsWithinRange(distance, profile)

  ██ DUVARI GEÇEN: bir SAYI. Geçemeyen: koordinat, tahta, metrik. ██
        Mesafenin Chebyshev mi Manhattan mı olduğu Core'da kalıyor;
        Combat "kaç hücre" sorusunun cevabını alıyor, sorusunu değil.
```

Bunun bedeli var: `BattleActions` var olmak zorunda ve her saldırı çağrısı iki
katmandan geçiyor. Karşılığında kazanılan da somut —
`GridStrategy.Combat.EditModeTests`'in `references` dizisinde **tek bir oyun
assembly'si var**: `GridStrategy.Combat` (geri kalanı TestRunner'lar). Menzil
kuralını sınamak için tahta kurmak gerekmiyor, çünkü kural tahtayı hiç
tanımıyor. `GridStrategy.Core.EditModeTests` de simetrik: yalnız
`GridStrategy.Core`. Duvar üretimde neyse testte de o.

> **Yanlış kredi tuzağı:** bu duvarı `noEngineReferences` kurmuyor. O bayrak
> yalnızca `UnityEngine`'i keser. `UnitGrid` düz bir C# sınıfı ve bayrak açıkken
> de pekâlâ referans edilebilirdi. Tahtayı dışarıda tutan tek şey
> `"references": []`. Aynı uyarı `AttackResolver.cs`'te de yazılı.

---

### İkinci fatura: bir enum, sahibinin üretemediği bir değer taşıyor

`MoveOutcome` `GridStrategy.Core`'da tanımlı. Beş değeri var. Dördünü
`MoveAction` üretebiliyor. Beşincisi — `RejectedActorCannotAct` — **Core'dan
asla çıkamaz.**

Sebep: o değerin cevabı `MovementRules.CanMove(state)`'te ve `MovementRules`
`GridStrategy.Combat`'ta. Core o adı yazamaz.

```
  değer                          Core'da üretilebilir mi?    kanıtı gören tip
  ────────────────────────────   ─────────────────────────   ────────────────
  RejectedInvalidDestination     ✓  tahtanın sınırı          UnitGrid    │Core
  RejectedCellOccupied           ✓  hücrenin içeriği         UnitGrid    │Core
  RejectedOutOfRange             ✓  iki koordinat arası      GridDistance│Core
  Moved                          ✓  yazma başarılı           UnitGrid    │Core
  ────────────────────────────   ─────────────────────────   ────────────────
  RejectedActorCannotAct         ✗  ██ SIRA / DURUM ██       TurnRules   │Battle
                                    ◄── AYRIŞMA NOKTASI      MovementRules│Combat
```

Değerin dünyadaki tek çıkış noktası `BattleActions.Move` ve orada **iki** dal
onu döndürüyor: `TurnRules.CanAct` başarısız olursa, ya da
`MovementRules.CanMove` başarısız olursa. İkisi de duvarın öteki tarafındaki
veriyi okuyor.

Bu bir taviz ve proje onu gizlemiyor, **sabitliyor**: `BattleActionsTests`'teki
`MoveAction_NeverReturnsRejectedActorCannotAct` testi hiçbir girdiyle bu değerin
Core'dan çıkmadığını tutuyor.

İki koruma, iki ayrı delik:

```
  KAZAYLA yazılması   ► asmdef: references boş → `MovementRules` adı
                        derlenmez        (derleme zamanı, kesin)

  BAŞKA yoldan sızma  ► MoveAction_NeverReturnsRejectedActorCannotAct
                        (çalışma zamanı, davranışsal)

  ██ Biri kapıyı kilitliyor, öteki kapının kilitli kaldığını ölçüyor. ██
     asmdef'e Combat referansı eklenirse birinci koruma AYNI GÜN düşer
     ve kuralı tutan tek şey test kalır.
```

> **⌨ KODU AÇ:** `Assets/Game/Core/MoveOutcome.cs` → `RejectedActorCannotAct`
> **BAK:** değer `Core`'da **bildirilmiş** ama `Core`'da hiçbir kod onu
> üretemiyor; üreten tek yer duvarın öteki tarafındaki `BattleActions.Move`.
> Bir enum değerinin sahibi ile üreticisi burada ayrışıyor.
> **DÖNÜŞ:** bu dosyanın «İkinci fatura: bir enum, sahibinin üretemediği bir değer taşıyor» bölümü

> **◀ DÖNÜŞ:** [06-sonuc-enumlari.md](06-sonuc-enumlari.md#iki-koruma-iki-farkli-kirilma) — «Bir asmdef bir enum değerini nasıl yasaklar»dan
> geldiysen artık şunu biliyorsun: yasağı koyan şey enum'un kendisi değil,
> `references` dizisi — ve o dizi değiştiği gün yasak **testten** başka hiçbir
> şeye dayanmıyor · oraya dön ve iki korumadan devam et

---

### Üçüncü fatura: ikizler ayrı katlarda yaşıyor

`AttackProfile` ile `MoveProfile` aynı şekle sahip: kurucuda donan, get-only bir
`Range` taşıyan `sealed class`. İkisi de "menzil" kelimesini kullanıyor. Ama
farklı assembly'lerde:

```
  ┌─ GridStrategy.Core ──────┐        ┌─ GridStrategy.Combat ────┐
  │  MoveAction              │        │  AttackAction            │
  │  MoveProfile   ◄── BURADA│        │  AttackProfile ◄── ORADA │
  │      Range : int         │        │      Range  : int        │
  └──────────────────────────┘        │      Damage : int  ◄─────┼── ██ AYIRAN
           ▲                          └──────────────────────────┘   ŞEY BU ██
           │  ✗ OK YOK — references: []
           └──────────────────────────────┘

  Profil sağdaki kutuda doğsaydı, soldaki MoveAction onu parametre
  olarak YAZAMAZDI: tip adı derlenmezdi. Ok olmadığı için ikiz
  bir kat aşağıda kalıyor.
```

Ayıran ölçüt kelime değil, **tipin ihtiyaç duyduğu kavramın hangi kutuda
yaşadığı**. `MoveProfile`'ın ihtiyacı olan her şey — hücre, uzaklık, sınır —
zaten Core'da. `AttackProfile`'ın yanında `Damage` var ve hasarın Core'da
karşılığı olan bir kavram **yok**. <!-- YOK-MUAF · DÜŞÜLDÜ · gerekçe aşağıdaki senette -->

> ██ YOKLUK SENEDİ — DÜŞÜLDÜ ██ — Core'da bir hasar kavramı
>
> **GEREKÇE:** Bu cümle bir ölü son değil, bir YERLEŞTİRME gerekçesidir. Hasar
> Core'da bir kavram olarak yaşamadığı için `AttackProfile` bir kat ötede
> doğuyor. ① dolmuyor. En güçlü aday tuzak hücresidir: oyuncu bir gün bazı
> hücrelere basınca canının yandığını görebilir. O özellik bile hasarı Core'a
> taşımıyor. Hücrenin tehlikeli olduğu bilgisi Core'da yaşar, hasarı uygulayan
> kural `DamageRules` ile birlikte Combat'ta kalır, ve ikisini bir araya getiren
> çağrıyı `Battle` yapar. Bugün iki kutu birbirini zaten tanımıyor: hem
> `GridStrategy.Core`'un hem `GridStrategy.Combat`'ın `references` dizisi boştur.
>
> **④ HAYIR.** Core'un hasarı öğrendiği gün bu bölümün ölçtüğü duvar ortadan
> kalkar. Yani burada uydurulabilecek şey mekanizmayı zorunlu kılan bir özellik
> değil, bölümün tezini yıkan bir özellik olurdu.

İki eşik bile ayrışıyor ve bu asimetri kasıtlı: `MoveProfile` kurucusu
`range < 0` ile keser (sıfır geçerli — "kök salmış birim"), `AttackProfile`
kurucusu `range < 1` ile keser (sıfır anlamsız — hiçbir hücreye ulaşamayan
saldırı). Aynı kelime, aynı şekil, farklı kutu, farklı eşik.

Ve `AttackProfile`'ın kurucusundaki iki `throw` da duvarın ürünü: doğrulama
`OnValidate`'e kaçamaz, çünkü `noEngineReferences: true` `ScriptableObject`'in bu
assembly'ye girmesini imkânsız kılıyor. Yasak değil — **yersizlik**. Aynı desenin
meşru evi bir duvar ötede, `GridStrategy.Unity`'de, ve orada `BoardAdapter` ile
`UnitView` motoru doğrudan kullanıyor.

---

## Duvarın engellemediği şey: ad çözümleme

Şimdi hikâyenin ters yüzü. `BoardAdapter`, `GridStrategy.Unity`'de yaşıyor ve
`references` dizisinde **`GridStrategy.Battle` var**. Yani duvar yok, ok açık,
görünürlük tam.

Ve buna rağmen çıplak `Battle` yazmak bu dosyada bir **derleme hatası** —
CS0118.

Sebep duvar değil. Sebep, duvarın hiç ilgilenmediği ikinci mekanizma: derleyici
bir kelimeyi hangi sırayla arıyor.

```
  global::
  └── GridStrategy
      ├── Battle           ◄── ① AD ALANI
      │   ├── Battle       ◄── ② SINIF (istenen şey)
      │   ├── BattleActions
      │   └── PlacementOutcome
      ├── Combat
      ├── Core
      └── Unity            ◄── BoardAdapter BURADA yaşıyor

  Tek kelime, iki şey: ① bir ad alanı, ② onun içindeki bir sınıf.
```

Arama merdiveni — ve nerede durduğu:

```
  SEVİYE 1   GridStrategy.Unity'nin ÜYELERİ
             BoardAdapter, UnitView                          ✗ yok
                 │
  SEVİYE 1b  namespace GÖVDESİNDEKİ using / alias    ◄── ALIAS BURADA
                 │                                       (BoardAdapter.cs:48)
  SEVİYE 2   GridStrategy'nin ÜYELERİ
             Battle, Combat, Core, Unity                     ✓ BULDU
             ██ ARAMA BİTTİ ██ bulunan şey bir AD ALANI, tip değil
                              ────────────────────────► CS0118
                 │
  SEVİYE 3   dosya BAŞINDAKİ using'ler
             using GridStrategy.Battle;      ── BURAYA HİÇ GELİNMEZ
```

Üç sonuç çıkıyor ve üçü de kolay unutuluyor:

**① `using` kurtaramaz.** Dosyanın 2. satırındaki `using GridStrategy.Battle;`
SEVİYE 3'te bekliyor; arama SEVİYE 2'de bitiyor. Üst ad alanının bir **üyesi**,
dosya başındaki `using`'i **her zaman** yener.

**② Alias'ın yeri kuralın kendisidir.** `BoardAdapter.cs`'te alias satırı
`namespace GridStrategy.Unity { … }` bloğunun **içinde** — 44. satırda, öteki
`using`'lerin (1-5. satırlar) 39 satır aşağısında. Orada olduğu için SEVİYE 1b'de
yakalanıyor ve arama SEVİYE 2'ye hiç çıkmıyor. Aynı satır dosyanın başına, öteki
`using`'lerin yanına taşınsaydı SEVİYE 3'e düşerdi ve CS0118 geri gelirdi.
**Metin harfi harfine aynı, sonuç zıt.** Yeri tesadüf değil, kararın kendisi.

**③ `global::` bugünkü hatayı çözmez, yarınkini engeller.** Alias'ın sağ
tarafındaki `GridStrategy` adı da çözülmek zorunda ve o çözüm de aynı merdivene
tabi. `global::` aramayı SEVİYE 1-2'yi atlayıp kökten başlatıyor: ileride
`GridStrategy` adlı bir tip ya da ad alanı eklense bile alias'ın hedefi sessizce
kaymaz. Sigorta, çözüm değil. Aynı desen `BattleTests.cs` ve
`BattleActionsTests.cs`'teki kardeş alias'larda da tekrarlanıyor.

### Tuzağın kapsamı: tek kelime

Bu tuzak `Battle` adına özel. Ölçüt: bir tip adı, aynı zamanda kapsayan zincirde
görünen bir **ad alanının** adı mı? Kesişimi al:

```
  GridStrategy'nin ad alanları  :  Battle   Combat   Core   Unity
  projedeki 34 tip adı          :  Battle   BattleActions   Unit   ...
  ──────────────────────────────────────────────────────────────────
  kesişim                       :  { Battle }        ◄── tek eleman
```

Karşı örnek aynı dosyada: `BattleActions` ve `PlacementOutcome` **tam olarak
aynı** ad alanında, aynı klasörde, aynı assembly'de yaşıyor — ve alias olmadan
çalışıyorlar, çünkü o adlarda bir ad alanı yok.

Bu yüzden `using` ile alias örtüşmüyor, **bölüşüyor**:

```
  Battle             çakışıyor   ► ALIAS halleder   (using etkisiz)
  BattleActions      çakışmıyor  ► using halleder   (alias gereksiz)
  PlacementOutcome   çakışmıyor  ► using halleder   (alias gereksiz)

  ██ İkisi de gerekli: using silinirse alttaki ikisi kırılır,
     alias silinirse üstteki kırılır. ██
```

> **⌨ KODU AÇ:** `Assets/Game/Unity/BoardAdapter.cs` → dosyanın başındaki
> `using Battle = global::GridStrategy.Battle.Battle;` satırı ve hemen üstündeki
> düz `using` satırları
> **BAK:** iki satır yan yana duruyor ve **aynı işi yapmıyorlar**. Alias yalnız
> `Battle` adını kurtarıyor; `BattleActions` ile `PlacementOutcome` düz `using`
> ile geliyor. Birini sil, hangi tarafın kırıldığını derleyiciye sor.
> **DÖNÜŞ:** bu dosyanın «Tuzağın kapsamı: tek kelime» bölümü

---

## Duvarın ürünü: `Battle` var olmak zorunda

Şimdi ilk bölümdeki iki cümleyi birleştir. Core konumu bilir, savaşı tanımaz.
Combat savaşı bilir, tahtayı tanımaz. **İkisi birbirini görmez.**

Yani `AttackAction`'ın dışarıdan istediği mesafeyi üretebilecek kimse yoktu.
`Combatant`'ın kim olduğunu söyleyebilecek kimse yoktu.

`Battle` bu boşluğun adı. Sınıfın kendi özeti bunu tek cümlede söylüyor: iki
assembly'yi birden referans eden **ilk** yer burası. Sınıf durumu tutuyor,
`BattleActions` akışı yürütüyor.

Ve `Battle`'ın üstlendiği eşleme, duvarın en sessiz faturası:

```
  ┌─ GridStrategy.Core ──────┐        ┌─ GridStrategy.Combat ────┐
  │  Unit                    │        │  Combatant               │
  │  ╰─ KİMLİK burada        │        │  ╰─ CAN, TARAF burada    │
  └──────────────────────────┘        └──────────────────────────┘
              ▲                                  ▲
              │        ✗ Combatant `Unit` yazamaz — adı yok
              │                                  │
              └───────────┬──────────────────────┘
                          │
              ┌─ GridStrategy.Battle ─────────────────┐
              │  Dictionary<Unit, Combatant>          │
              │  Dictionary<Unit, Structure>          │
              │  Dictionary<Unit, Action<…>>  ◄───────┼── ██ BU SÖZLÜĞÜN
              │       stateForwarders                 │      KÖKÜ DUVAR ██
              └───────────────────────────────────────┘
```

`01-olay-zinciri.md` `Combatant`'ın kendi kimliğini bilmemesini **bilinçli bir
karar** olarak anlatıyor. Buradan bakınca aynı şeyin ikinci yüzü görünüyor: o
karar sadece bir tercih değil, **derleyici tarafından uygulanan bir kısıt**.
`Combatant.StateChanged` olayı `Unit` taşıyamıyor çünkü `Combatant` `Unit`
kelimesini yazamıyor. Olay kimlik taşımayınca `Battle` her savaşçı için kapanış
üretmek zorunda kalıyor, kapanışları sökebilmek için de bir sözlük tutmak
zorunda kalıyor.

**Duvarın faturası dört dosya öteye kadar uzanıyor.**

---

## Tek bakışta: duvar ve dört faturası

```
  ╔═ GridStrategy.Core ═══╗          ╔═ GridStrategy.Combat ═╗
  ║ references: []        ║          ║ references: []        ║
  ║ noEngine   : true     ║          ║ noEngine   : true     ║
  ║                       ║          ║                       ║
  ║ tahta · uzaklık       ║  ██ ██   ║ can · hasar · taraf   ║
  ║ hareket · kimlik      ║  ██ ██   ║ menzil · durum        ║
  ╚═══════════════════════╝  ██ ██   ╚═══════════════════════╝
              │              DUVAR               │
              │       çift yönlü, ok YOK         │
              └──────────────┬───────────────────┘
                             ▼
              ╔═ GridStrategy.Battle ══════════╗
              ║ references: [Core, Combat]     ║
              ║ ██ İLK BİRLEŞME NOKTASI ██     ║
              ╚════════════════┬═══════════════╝
                               ▼
              ╔═ GridStrategy.Unity ═══════════╗
              ║ references: [Core,Combat,Battle]║
              ║ noEngine  : FALSE ◄─ MOTOR     ║
              ╚════════════════════════════════╝

  DUVARIN KESTİĞİ DÖRT FATURA
  ───────────────────────────────────────────────────────────────
  ① AttackResolver / AttackAction  mesafeyi PARAMETRE alır
                                   (koordinat duvarı geçemez)
  ② MoveOutcome                    beşinci değerini sahibi ÜRETEMEZ
                                   (tek üretici BattleActions.Move)
  ③ MoveProfile                    ikizinin BİR KAT ALTINDA yaşar
                                   (AttackProfile Combat'ta kalır)
  ④ Battle + stateForwarders       eşleme dışarıda tutulur
                                   (Combatant `Unit` yazamaz)

  DUVARIN KESMEDİĞİ ŞEY
  ───────────────────────────────────────────────────────────────
  ✗ AD ÇÖZÜMLEME.  BoardAdapter → Battle oku AÇIK, görünürlük TAM,
    ve çıplak `Battle` yine de derlenmiyor.  ██ İKİ MEKANİZMA BAĞIMSIZ ██
```

---

## Kural: iki karar ağacı

### A — yeni bir tip yazıyorum, nereye koyacağım?

```
① Bu tipin ihtiyaç duyduğu KAVRAMLAR hangi kutuda yaşıyor?
      hepsi tek kutuda   → o kutuya koy. Bitti.
      birden çok kutuda  → ②

② İhtiyaç duyduğu şeyi bir SAYIYA / DEĞERE indirebilir misin?
      EVET  → ██ tipi alt kutuda bırak, değeri PARAMETRE al ██
              kanıt: IsWithinRange(int distance, …) — koordinat değil,
              ölçüm geçiyor
      HAYIR → ③

③ İkisini birden GÖREN bir kutu zaten var mı?
      VAR   → akışı oraya koy (kanıt: BattleActions)
      YOK   → ④

④ ██ Şimdi asmdef'e referans eklemeyi düşünüyorsun. DUR. ██
      Sor: bu oku açtığım gün hangi TEST kutusu tahtaya bağlanır?
      Combat.EditModeTests bugün YALNIZ Combat'ı referans ediyor.
      Cevabın "hiçbiri" ise ok meşru; değilse ② ye geri dön.
```

**②'nin ayıracı:** sayı bir **ölçüm** mü (tahtaya bağlı → parametre) yoksa bir
**tanım** mı (tahtadan bağımsız → içeride). `IsWithinRange`'in iki parametresi
tam olarak bu ikisi ve zıt karar alıyorlar.

### B — CS0118 aldım, ne yapacağım?

```
① Yazdığın tip adı, KAPSAYAN zincirde bir AD ALANI adı mı?
      kesişimi al: {Battle, Combat, Core, Unity} ∩ {tip adların}
      boş  → sorun bu değil. `using` yeter, başka yere bak.
      dolu → ②

② Bu ad, bu dosyada KAÇ kez geçiyor?
      1 kez  → tam nitele: GridStrategy.Battle.Battle
               (alias tek kullanım için gürültü olur)
      N kez  → ③

③ ██ ALIAS — ve namespace GÖVDESİNE yaz ██
      dosya başına yazarsan SEVİYE 3'e düşer, hata geri gelir
      sağ tarafa `global::` koy → hedefin gelecekte kaymasın

④ Ama asıl kök ADLANDIRMADIR:
      sınıf `BattleState` olsaydı  → tuzak hiç doğmazdı
      ad alanı `GridStrategy.Battles` olsaydı → tuzak hiç doğmazdı
      ad korundu; bedeli BoardAdapter.cs'teki 88 satırlık blok oldu.
```

---

## Yanlış hatırlanan dört şey

**"Klasörü taşırsam bağımlılık değişir."** Değişmez. Klasör hiçbir şeyi
uygulamıyor. Kanıt aynı projede: `Core/Combat/` diskte Core'un **içinde**, ad
alanı `GridStrategy.Combat`, yani Core'un **kardeşi**. Bağımlılığı uygulayan tek
şey asmdef'in `references` dizisi; klasör yalnızca insanın gözü için.

**"`noEngineReferences` duvarı kuruyor."** Kurmuyor. O bayrak yalnızca
`UnityEngine`'i kesiyor. `UnitGrid` düz bir C# sınıfı ve bayrak `true` iken de
pekâlâ referans edilebilirdi — Core ile Combat'ı ayıran tek şey `"references":
[]`. Garanti de tam orada bitiyor: **aynı** assembly'ye yarın bir tahta tipi
eklenirse hiçbir bayrak uyarmaz. Bu uyarı `AttackResolver.cs`'in kendi bloğunda
da yazılı, çünkü tam olarak burada yanlış kredi veriliyor.

**"`using` eklersem CS0118 çözülür."** Çözülmez. Arama SEVİYE 2'de bitiyor,
`using`'ler SEVİYE 3'te bekliyor. Zaten `BoardAdapter.cs`'in 2. satırında o
`using` **var** ve hata yine de doğuyor.

**"Assembly duvarı ad çözümlemeyi de etkiler."** Etkilemiyor, ve bu ikisini
karıştırmak en pahalı yanlış model. `BoardAdapter`'ın `GridStrategy.Battle`'a
oku **açık**, görünürlük **tam** — ve çıplak `Battle` yine derlenmiyor.
Görünürlük "bu ad aranabilir mi" sorusunu cevaplıyor; ad çözümleme "arama nerede
duruyor" sorusunu. İkisi bağımsız ve ikisi de tek başına derlemeyi durdurabilir.

---

## Kaçış yolu: duvar kaldırılsaydı

Tek satırlık bir değişiklik. `GridStrategy.Core.asmdef`:

```json
✗  "references": []
✓  "references": [ "GridStrategy.Combat" ]
```

**Kazanılırdı:**

```
  MoveOutcome.RejectedActorCannotAct    ► MoveAction kendi üretir;
                                          BattleActions.Move'un bir dalı
                                          gereksizleşir
  AttackAction / AttackResolver         ► (ters ok da açılırsa) mesafeyi
                                          kendi ölçer; BattleActions.Attack'in
                                          yarısı gereksizleşir
  Combatant.StateChanged                ► `Unit` taşıyabilir → kapanış
                                          gereksiz → Battle.stateForwarders
                                          sözlüğü tamamen kaybolur
  MoveProfile / AttackProfile           ► ikizler aynı klasörde durabilir
```

Yani liste kısa değil. Dört faturanın üçü siliniyor.

**Kaybedilirdi:**

```
  Combat.EditModeTests                  ► bugün YALNIZ GridStrategy.Combat'ı
                                          referans ediyor. Ok açıldığı gün
                                          menzil kuralını sınamak için tahta
                                          kurulabilir hâle gelir — ve bir gün
                                          kurulur.
  mesafe metriği                        ► Chebyshev kararı saldırı kuralının
                                          içine donar; engel/yükseklik kuralı
                                          geldiği gün iki dosya birden değişir
  ok yönü                               ► tek yönlü ok çift yönlü olur;
                                          "hangi kural hangi katmanda"
                                          sorusunun cevabı kalmaz
  MoveOutcome'un kısıt izi              ► beşinci değer hâlâ tek yerde
                                          üretilir ama artık KİMSE SEBEBİNİ
                                          hatırlamaz
```

Ve asıl mesele son satırda. Bu değişikliğin en tehlikeli yanı şu:

```
  ██ DUVAR KALDIRILDIĞI GÜN HİÇBİR ŞEY KIRILMAZ. ██

  derleyici : hiçbir şey der
  testler   : hepsi yeşil kalır
  oyun      : aynı şekilde çalışır

  Bedel görünür  : mesafe parametresi, beşinci değerin tuhaflığı,
                   Battle'ın varlığı, sözlük — hepsi kodda duruyor
  Fayda görünmez : yazılmamış bağımlılıklar. Var olmayan bir oku
                   kimse sayamaz.

  ◄── Kaldırma kararı bu asimetriyle veriliyor: elinde bedelin
      tam listesi var, faydanın hiç yok.
```

Duvar bir **kısıt**. Kısıtlar bedeli önden ve görünür şekilde tahsil eder,
faydayı ise ödemediğin faturalarda saklar. `MoveOutcome`'un beşinci değeri bu
yüzden gizlenmedi de enum'a yazıldı: kısıtın izini görünür bir yerde bırakmak,
onu unutulmaz kılıyor.

**Kendi projende** bu tuzağa hiç düşmezsin: tek assembly ile başlarsın, hiçbir
fatura kesilmez, hiçbir ok yasaklanmaz. Buradaki mimari, tahtayı savaştan ve
ikisini birden motordan ayrı tutmayı seçtiği için bedeli ödemeyi seçti — ve
bedeli kodun içinde okunur bıraktı.

---

## Bunu okuduktan sonra kodda ne göreceksin

`AttackResolver`'ın `distance` parametresi, `MoveOutcome`'un beşinci değeri,
`MoveProfile`'ın Core'da durması ve `BoardAdapter`'ın 44. satırındaki alias artık
dört ayrı tuhaflık değil — **aynı iki mekanizmanın** izleri. İlk üçü
görünürlüğün, sonuncusu ad çözümlemenin.

Kodda karar, burada hikâye. İkisi çelişirse **kod kazanır** — orası çalışan
metin, burası anlatı.

---

## ██ SIRADAKİ ADIM ██

> **▶ SIRADA:** [`03-tahta-sahipligi.md`](03-tahta-sahipligi.md) — okuma yolunun **3.** adımı
> **NEDEN ORASI:** `03`'ün üç katmanlı figürünün **orta katmanı** (`internal Board`)
> doğrudan bu duvara dayanıyor ve `03` *"Sözleşme assembly duvarında biter"*
> diyor — o cümle ancak bu dosya okunduktan sonra okunabilir hâle geldi. ██ Bu
> adımdan önce ██ [`../../ogrenme/00-okuma-sirasi.md`](../../ogrenme/00-okuma-sirasi.md)'ndaki
> **DURMA NOKTASI 1**'i geç: testleri koştur, sonra test `asmdef`'inin
> `references` dizisine gözünle bak.
> **YOL HARİTASI:** [`../../ogrenme/00-okuma-sirasi.md`](../../ogrenme/00-okuma-sirasi.md)
