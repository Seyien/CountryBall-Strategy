# Okuma sırası — yarın sabah izlenecek yol

> **Ne zaman oku:** `Docs/deep/` ağacına ilk kez oturmadan **önce**, ve her
> oturumun başında bir kez daha.
> **Ne yapar:** 12.226 satırlık üç ağacı 14 adıma ve 5 oturuma bölüyor; her
> adımın **niye orada** olduğunu ve yanında **hangi `.cs` dosyasının** açık
> duracağını yazıyor.
> **Ne YAPMAZ:** hiçbir mekanizmayı anlatmaz. Bu bir yol tarifi, bir ders değil.

---

## ██ ÖNCE ŞUNU OKU: NUMARALAR BİR SIRA DEĞİL ██

`konular/01`, `dil/05`, `ogrenme/03` gibi adlardaki sayılar
██ **bir DOSYA KİMLİĞİDİR, bir okuma sırası değil.** ██

Bu tek cümle olmadan aşağıdaki her şey boşa gider: okuyucu yine numaralara uyar,
`01`'den başlar, ve ilk sayfada tanımlanmamış üç kavramın üstüne oturur.

**Numaraların sıra OLMADIĞININ ölçüsü:** bağımlılık grafiğinde sekiz `konular/`
dosyasından yalnız **ikisi** (`03` ve `08`) numara sırasında doğru yerde
duruyor. Doğru sıra `02 · 03 · 05 · 06 · 04 · 07 · 08 · 01`; numara sırası
`01 · 02 · 03 · 04 · 05 · 06 · 07 · 08`.

**Peki neden düzeltilmiyor — numaralar niye yeniden dizilmiyor?**

```
   Yeniden numaralandırmanın KIRACAĞI şeyler — sayıldı:

     4 makine kapısı      check-doc-links.py · check-cited-names.py
                          check-curriculum-coverage.py · check-cross-file-refs.py
     30+ çapa             00-iskelet.md'de 12 atıf, ogrenme/03'te 20'den fazla
                          satır, artı .cs yorumlarındaki "DERİN ANLATIM:" yolları

   ██ Numara ucuz görünür, ama 30+ yerde çapa olarak kullanılıyor. ██
   Sıra ise tek bir yerde yaşayabilir: bu dosyada.
```

**Numara dosyanın ADIDIR. Sıra bu belgededir. İkisi ayrı şeyler** — ve ayrı
kalmaları bilinçli bir karar.

---

## ██ TOPLAM SÜRE VE OTURUM BÖLÜMÜ ██

```
   belge          12.226 satır   yoğun teknik Türkçe anlatı
   yanında kod     ~5.300 satır   (aynı satırları birkaç kez açacaksın)
   ────────────────────────────────────────────────────────────────
   GERÇEKÇİ TAHMİN
     saf okuma                          5 – 6 saat
     kod yan yana açıp izleme         + 2 – 3 saat
     durma noktalarında koşturma      + 1,5 saat
   ────────────────────────────────────────────────────────────────
   TOPLAM                              9 – 11 saat  ·  ██ BEŞ OTURUM ██
```

**Tek oturumda bitirme.** Ölçü: `05`, `06`, `07` ve `08` dosyalarının her biri
700-950 satır ve her biri **kendi karar ağacını** taşıyor. Arka arkaya iki
tanesi okunduğunda ikincinin karar ağacı birincininkiyle karışıyor.

██ **Oturum sınırları tesadüf değil: her oturum bir KOŞTURMA ile bitiyor.** ██
Koşturmadan ilerlemek, kapatılmamış bir haritanın üstüne yenisini koymaktır. Bir
durma noktasını atlamak, o oturumu hiç okumamaktan **daha kötüdür**: okuduğunu
sanırsın.

---

## ██ BAĞIMLILIK GRAFİĞİ — sıranın türetildiği yer ██

Kenar ölçütü: **X → Y** demek, *"X'in taşıyıcı bir gerekçesi Y'nin konusuna
dayanıyor ve X onu kendi içinde tanımlamıyor"*. Üslup benzerliği kenar değildir.

```
                    ╔══════════════════════════╗
                    ║   deep/00-iskelet.md     ║  ◄── ██ TEK ACYCLIC KÖK ██
                    ║   gelen kenar : 0        ║      hiçbir şeye bağımlı değil
                    ╚════════════╤═════════════╝      her şeyi SIĞ tanıtır
                                 │
                                 ▼
                    ╔══════════════════════════╗
                    ║   konular/02  DUVAR      ║  ◄── ██ OMURGA ██
                    ║   gelen kenar : 6        ║      01·03·04·05·06·07
                    ╚════════════╤═════════════╝      altısı da buna dayanıyor
                                 │
             ┌───────────────────┼───────────────────┐
             ▼                   ▼                   ▼
     konular/03  ──► dil/01   konular/05         (aşağıya devam)
     sahiplik                 durum makinesi
                                 │
                                 ▼
                          konular/06  ret değerleri
                                 │
                    ┌────────────┴────────────┐
                    ▼                         ▼
            konular/04 ──► dil/03      konular/07  uçtan uca akış
            karar sırası                       │
                                               ▼
                                       konular/08  motorun tarafı
                                       ██ YAPRAK ██ kimse buna bağımlı değil
                                               │
                                               ▼
                                    ╔═════════════════════╗
                                    ║   konular/01        ║
                                    ║  ██ BULUŞMA NOKTASI ██
                                    ╚══════════╤══════════╝
                                               │
                                               ▼
                          dil/04 ──► dil/06 ──► dil/07   (zorunlu üçlü)
                                               │
                                               ▼
                              dil/05 · dil/02 · dil/03   (referans, sıra serbest)
                                               │
                                               ▼
                            ogrenme/01 ──► ogrenme/03 ──► ogrenme/02
```

### ██ `konular/01`'in gerçek rolü — en pahalı yanlış anlama ██

```
   ┌───────────────────────────────────────────────────────────┐
   │  01'i anlamak için ÖNCE gereken — üç ayrı iplik:          │
   │     konular/02   ── ad alanı + assembly       (01:55)     │
   │     konular/05   ── Alive / Downed / Dead     (01:65)     │
   │     dil/04       ── event · Action · lambda   (01:176)    │
   │                                                           │
   │  01'i okuduktan sonra AÇILAN:                             │
   │     hiçbir şey. Dört dosya ona atıf yapıyor ama hiçbiri   │
   │     onu ÖN KOŞUL saymıyor.                                │
   └───────────────────────────────────────────────────────────┘

   ██ 01 bir GİRİŞ değil, bir DÜĞÜM. ██ Üç iplik orada düğümleniyor.
   Erken okunursa üçü de tanımsız; geç okunursa düğüm kendiliğinden çözülüyor.
   Zincirin en kısa dosyası (336 satır) tesadüfen değil, BULUŞMA NOKTASI
   olduğu için kısa: üç ipliği anlatmıyor, bağlıyor.
```

**Bu kararın iki bağımsız kanıtı var** ve ikisi de aynı sonucu veriyor:
① kenar sayımı (yukarıdaki grafik), ② anlatının kendi beyanı —
`deep/00-iskelet.md:523-527` kendi önerdiği yolda `01`'i **hiç anmıyor**.

---

# ██ OTURUM 1 · BÜTÜNÜ KUR ██  (~2 saat · 1.262 satır)

Amaç tek bir mekanizmayı öğrenmek değil: **yüzeyin tamamını görmek** ve duvarın
nerede durduğunu bilmek.

### ADIM 1 · [`deep/00-iskelet.md`](../deep/00-iskelet.md) — tamamı (604 satır)

**NEDEN BU SIRADA:** Zincirin tek acyclic kökü — gelen kenarı **sıfır**. Hiçbir
dosyaya bağımlı değil ve her kavramı sığ biçimde tanıtıyor. Dahası bu dosya
senin kendi cümlenle yazılmış (`00-iskelet.md:18-21`):

> *"sen sadece şuraya bak şuraya dediğinde diğer kısımlarını görmediğim için
> kafamda oturtturamıyorum"*

██ Bugünkü sorun ile bu dosyanın yazılma sebebi **aynı** sorun. ██

**AÇIK OLACAK KOD:** ██ yok. ██ İlk geçişte kod açma — dosya kasten tip adı
kullanmadan başlıyor.

**BU ADIMDAN SONRA CEVAPLAYABİLECEĞİN SORU:**
> *"Bu oyun ne? Tahtada ne var, bir tıklama kaç anlama gelebilir, bir birim
> nasıl ölür, ve dört derleme birimi hangi sırayla birbirini görüyor?"*

**██ ATLA: ██** `§6 Aynı basınç, üç başka oyun` (`:531-581`). O bölüm
karşılaştırma; ilk geçişte gerekmez. Beşinci oturumda geri gel.

---

### ADIM 2 · [`konular/02-assembly-duvari.md`](../deep/konular/02-assembly-duvari.md) — tamamı (658)

**NEDEN BU SIRADA:** ██ Zincirin omurgası: altı dosya buna bağımlı, bu hiçbir
şeye. ██ `01`, `03`, `04`, `05`, `06`, `07` — altısı da bir noktada "assembly
duvarı" diyor ve **hiçbiri onu tanımlamıyor**. Bu adım altı ön koşulu birden
kapatıyor; zincirdeki en yüksek getirili tek adım.

**AÇIK OLACAK KOD — dört `.asmdef`, hepsi kısa:**
```
Assets/Game/Core/GridStrategy.Core.asmdef              references: []
Assets/Game/Core/Combat/GridStrategy.Combat.asmdef     references: []
Assets/Game/Battle/GridStrategy.Battle.asmdef          references: [Core, Combat]
Assets/Game/Unity/GridStrategy.Unity.asmdef            references: [Core, Combat, Battle]
```
artı iki `.cs` yeri:
```
Assets/Game/Unity/BoardAdapter.cs:48      using Battle = global::GridStrategy.Battle.Battle;
Assets/Game/Core/Combat/AttackResolver.cs IsWithinRange(int distance, AttackProfile)
```

**BU ADIMDAN SONRA CEVAPLAYABİLECEĞİN SORU:**
> *"Klasör, ad alanı ve assembly neden **üç ayrı şey**? `Core` ile `Combat`
> neden birbirini görmüyor ve bu bana ne fatura kesiyor? CS0118 neden bir
> `using` ile çözülmüyor?"*

---

### ██ DURMA NOKTASI 1 ██ — duvarın faturasını satın al

**KOMUT:**
```powershell
.\Tools\run-editmode-tests.ps1
```

**KAPI:** testler yeşil mi.

**SONRA — gözünle doğrula:** `Assets/Tests/EditMode/Combat/` klasöründeki
`.asmdef`'i aç ve `references` dizisine bak. `02:216-222`'nin iddiası şu:

> *"`GridStrategy.Combat.EditModeTests`'in `references` dizisinde **tek bir oyun
> assembly'si var**"*

██ Doğrulamadan ilerleme. ██ Duvarın "faturası"nın satın aldığı şey tam olarak
o tek satır — ve zincirin geri kalanı bu takasa dayanıyor.

---

# ██ OTURUM 2 · SAHİPLİK VE DURUM ██  (~2 saat · 1.825 satır)

### ADIM 3 · [`konular/03-tahta-sahipligi.md`](../deep/konular/03-tahta-sahipligi.md) — tamamı (503)

**NEDEN BU SIRADA:** `02`'nin hemen ardından, çünkü `03`'ün "üç katman"
figürünün **orta katmanı** (`internal Board`) doğrudan duvara dayanıyor ve
`03:271` *"Sözleşme assembly duvarında biter"* diyor — duvarı bilmeden o cümle
okunamaz. Ayrıca `03` zincirin **en dürüst** dosyası: `:19-22` garantinin nerede
biteceğini önden vaat ediyor ve `:220-271`'de gerçekten ödüyor.

**AÇIK OLACAK KOD:**
```
Assets/Game/Battle/Battle.cs      board alanı · internal Board üyesi · kurucu
Assets/Game/Core/UnitGrid.cs      PlaceUnit · RemoveUnit · TryGetUnit · ThrowIfOutsideGrid
Assets/Game/Battle/BattleActions.cs:207
    MoveAction.Execute(battle.Board, unit, fromX, fromY, toX, toY, profile);
                                  ██ Board'un TEK çağırısı — tüm projede ██
```
██ O tek satırı **kendin say**: ██ `grep -n 'battle.Board' Assets/Game/Battle/BattleActions.cs`

**BU ADIMDAN SONRA CEVAPLAYABİLECEĞİN SORU:**
> *"`readonly` tam olarak neyi **korumuyor**? Tahtaya kim yazabiliyor, ve
> derleyicinin garantisi tam olarak hangi satırda bitiyor?"*

---

### ADIM 4 · [`dil/01-degismezlik-anahtar-kelimeleri.md`](../deep/dil/01-degismezlik-anahtar-kelimeleri.md) — tamamı (598)

**NEDEN BU SIRADA:** `03`'ün ikinci durağı `readonly`'nin **korumadığı** şeyi
gösteriyor ama **neden** korumadığını dil düzeyinde vermiyor. `dil/01` veriyor —
ve `dil/01:176` zaten `03`'e geri işaret ediyor:

> *"Tekrarlamıyorum, gerekçesi ve üç katmanlı haritası burada:
> `konular/03-tahta-sahipligi.md`"*

██ İki dosya bilerek bölüşmüş. `03`'ü okumadan `dil/01`'i okumak tersten
okumaktır. ██

**AÇIK OLACAK KOD:**
```
Assets/Game/Battle/TurnState.cs:43-53
    public static readonly IReadOnlyList<Team> DefaultTurnOrder =
        ◄── beş kelimenin dördü üst üste
Assets/Game/Core/UnitGrid.cs                   private readonly Unit[,] cells
Assets/Game/Core/Combat/UnitLifecycle.cs       downedWindowSeconds (readonly)
                                               vs remainingSeconds (DEĞİL)
```

**BU ADIMDAN SONRA CEVAPLAYABİLECEĞİN SORU:**
> *"`const` ile `static readonly` arasındaki fark ne, hangisi zorunlu, ve
> `const`'un **assembly sınırında** ödediği bedel ne?"*

---

### ADIM 5 · [`konular/05-yasam-dongusu.md`](../deep/konular/05-yasam-dongusu.md) — tamamı (724)

**NEDEN BU SIRADA:** Grafikte `05` iki dosyanın (`06` ve `01`) ön koşulu. `06`
sıfırıncı enum değerini `05`'ten ödünç alıyor, `01` üç durumun **adını**
kullanıyor ama anlamını vermiyor. Ayrıca `04`'ün "eyleyen vs hedef" asimetrisi
bu dosyanın tablosuna dayanıyor.

**AÇIK OLACAK KOD:**
```
Assets/Game/Core/Combat/UnitState.cs            üç değer
Assets/Game/Core/Combat/UnitLifecycle.cs        OnHealthDepleted · Tick · TryRevive · SetState
Assets/Game/Core/Combat/StructureLifecycle.cs   ikiz — ve ÜÇ EKSİĞİ
Assets/Game/Core/Combat/TargetingRules.cs       CanBeAttacked · CanBeRevived
Assets/Game/Battle/Battle.cs                    RemoveReadyForCleanup
```

**BU ADIMDAN SONRA CEVAPLAYABİLECEĞİN SORU:**
> *"Neden **üç** durum, iki değil? `Downed` neden hem vurulabilir hem
> diriltilebilir? Yapının `TryRevive`'ı neden yok — ve `TryRepair` neden onun
> yerine geçmiyor?"*

---

### ██ DURMA NOKTASI 2 ██ — bir testin ADI bir kararı nasıl taşır

**KOMUT:**
```powershell
.\Tools\run-editmode-tests.ps1
```

**TEST — aç ve gövdesini oku:**
```
Assets/Tests/EditMode/Combat/TargetingRulesTests.cs
    Downed_IsTheOnlyStateBothAbilitiesAccept
Assets/Tests/EditMode/Combat/UnitLifecycleTests.cs
    Tick(10.1f) ile pencerenin TAM yerini sınayan testler
```

`05:212-214` şunu iddia ediyor:
> *"bu iki cevabı sabitleyen tek bir test var —
> `TargetingRulesTests.Downed_IsTheOnlyStateBothAbilitiesAccept`.
> **Adı doğrudan kesişimi söylüyor.**"*

██ Aç ve oku. Bir test **adının** bir tasarım kararını nasıl taşıdığını görmek,
bu projedeki en aktarılabilir beceri. ██

---

# ██ OTURUM 3 · KARAR VE RET ██  (~2 saat · 1.342 satır)

### ADIM 6 · [`konular/06-sonuc-enumlari.md`](../deep/konular/06-sonuc-enumlari.md) — tamamı (714)

**NEDEN BU SIRADA — ██ ve neden `04`'ten ÖNCE ██:** Numara sırası `04 → 06`
diyor; bağımlılık **tersini** diyor. İkisi de aynı ölçütü kuruyor:

```
   06:494    "Ayıraç sebep sayısı değil, ██ DAVRANIŞ sayısı ██"
   04:606    "ayırıcı şey SEBEP sayısı değil, ██ DAVRANIŞ sayısı ██"
```

██ Aynı kural, iki dosyada iki kez, ve hiçbiri ötekini anmıyor. ██
`06` onu **kurar** (dört enum, on bir ret değeri, tam tablo); `04` onu bir
**sıra kararına uygular**. ██ Kuran önce okunur. ██

**AÇIK OLACAK KOD:**
```
Assets/Game/Core/MoveOutcome.cs             5 değer · sıfırıncı bir RET
Assets/Game/Core/Combat/AttackOutcome.cs    6 değer · altıncısı SONA eklendi
Assets/Game/Battle/PlacementOutcome.cs      3 değer · üç kapı
Assets/Game/Battle/ReviveOutcome.cs         4 değer · ekran tüketicisi YOK
Assets/Game/Unity/BoardAdapter.cs           ReactToMove · ReactToAttack
```

**BU ADIMDAN SONRA CEVAPLAYABİLECEĞİN SORU:**
> *"Sıfırıncı değer neden ret — ve kural neden 'sıfır hep ret olsun' değil? Bir
> `asmdef` bir enum değerini nasıl **üretilemez** kılar? Ret değerlerini
> birleştirmenin bedeli kimin cebinden çıkar?"*

---

### ADIM 7 · [`konular/04-karar-sirasi.md`](../deep/konular/04-karar-sirasi.md) — tamamı (628)

**NEDEN BU SIRADA:** `06` ölçütü kurdu; `04` onu **uyguluyor**. Ve `04`'ün iki
körlüğü (`AttackRules` sırayı soramaz, `MoveAction` durumu soramaz) doğrudan
ADIM 2'de kapattığın duvara dayanıyor.

**AÇIK OLACAK KOD:**
```
Assets/Game/Battle/BattleActions.cs        sınıf başlığındaki ADIM 0-7 zinciri
Assets/Game/Core/Combat/AttackAction.cs    erken çıkış merdiveni
Assets/Game/Core/MoveAction.cs             SEVİYE 1-3 tablosu
Assets/Game/Core/Combat/AttackRules.cs · TargetingRules.cs
Assets/Game/Battle/TurnRules.cs
```

**██ BU DOSYANIN VERMEDİĞİ TEK ŞEY: ██** `04` bu üç tipin rolünü 628 satır
boyunca anlatıyor ve o role **hiç ad vermiyor**. Adı
[`ogrenme/01` §2](01-koda-gomulu-desenler.md#2-akis-sahibi-transaction-script-command-degil)'de:
██ **akış sahibi** (transaction script) — ve bir Command DEĞİL ██
(ölçü: üçü de `static class`, tek alanı yok). "Bu projede hangi desenleri
kullandın" sorusunun cevabı orada; ADIM 14'te kapatacaksın.

**BU ADIMDAN SONRA CEVAPLAYABİLECEĞİN SORU:**
> *"Aynı tıklama neden **dört farklı sebeple** reddedilebiliyor ve hangisi
> kazanıyor? Geri dönülemez çizgi nerede, ve altına düşen bir kural neden kural
> olmaktan çıkıyor?"*

---

### ██ DURMA NOKTASI 3 ██ — "Prefers" testleri: koşturulabilir karar ağacı

**KOMUT:**
```powershell
.\Tools\run-editmode-tests.ps1
```

**TESTLER —** `Assets/Tests/EditMode/` altında `Prefers` geçenleri ara:
```
Execute_OutsideBoardAndOutOfRange_PrefersInvalidDestination
Execute_OccupiedCellOutOfRange_PrefersOutOfRange
Attack_OutOfTurnAgainstAnInvalidTargetOutOfRange_PrefersActorCannotAct
Execute_DownedAttackerWithDeadTargetOutOfRange_PrefersActorCannotAct
```

██ Her `Prefers` testi bir SIRA KARARINI tutuyor. ██ Ama `04:412` şunu da
söylüyor:
> *"Bunu ölçen bir test yok, **ve olmaması doğru**."*

Bir sıra kararının **ne zaman** test edilebilir olduğunu (iki cevabın farklı
olması gerekir) ve ne zaman edilemeyeceğini bu dört test gösteriyor.

---

# ██ OTURUM 4 · MOTOR SINIRI ██  (~2,5 saat · 1.866 satır)

██ En uzun oturum, ve **içinde tek çalışmayan senaryo var.** Aşağıdaki uyarıyı
oturuma başlamadan oku. ██

### ADIM 8 · [`konular/07-tiklamadan-eyleme.md`](../deep/konular/07-tiklamadan-eyleme.md) — tamamı (916)

**NEDEN BU SIRADA:** `deep/00-iskelet.md:526` bunu doğru adlandırıyor:
*"uçtan uca tek akış"*. Duvar (`02`), sahiplik (`03`), durum (`05`) ve ret
(`06`, `04`) bilindikten sonra bu dosya hepsini **tek bir tıklamada**
birleştiriyor. Daha erken okunursa birleştirecek bir şey yok.

**AÇIK OLACAK KOD:**
```
Assets/Game/Unity/BoardAdapter.cs   Update · HandleClick · UpdatePlacement
                                    FeedGesture · TryReadPointerCell · CommitPlacement
Assets/Game/Core/PointerGesture.cs  Press · MoveTo · Release · Reset · PointerPhase
Assets/Game/Unity/UnitView.cs       SetSelected
```

**BU ADIMDAN SONRA CEVAPLAYABİLECEĞİN SORU:**
> *"Bir **piksel** nasıl bir **hücreye** dönüşüyor, duvarı ne geçiyor, ve
> tıklama ile sürükleme tam olarak nerede ayrılıyor? **Niyet** ile
> **geçerlilik** neden iki ayrı soru?"*

---

### ██ DURMA NOKTASI 4 ██ — ██ EDITOR · VE BURADA BİR ŞEY KIRILACAK ██

Unity'yi aç, `Assets/Scenes/SampleScene.unity`, **Play**.

#### ① Çalışacak olan — dört dal

Bir askere tıkla (seçilir, çerçeve açılır) → boş hücreye tıkla (yürür) →
düşmana tıkla (vurur) → aynı askere ikinci kez tıkla (seçim bırakılır).
`07:560-572`'deki niyet tablosunun dört dalı.

#### ② ██ ÇALIŞMAYACAK OLAN — VE BU SENİN HATAN DEĞİL ██

Bir birim seç, **`B`**'ye bas (yerleştirme kipi açılır, hayalet belirir),
sürükle, **tahta içindeki boş bir hücreye** bırak. Console'da şunu göreceksin:

```
ArgumentException: The unit is already in this battle.
Parameter name: unit
```

██ Bu bir KOD KUSURU ve `konular/07` dışında zincirin hiçbir yeri onu
söylemiyor. ██

**Sebebi — koda karşı doğrulandı, üç sıçrama:**

```
BoardAdapter.cs:502   Unit placer = selectedUnit;
BoardAdapter.cs:504   BattleActions.PlaceStructure(battle, placer, NewStructure(placer), x, y);
                                                          ▲
                      ██ YAPIYA, ZATEN KAYITLI BİR BİRİMİN KİMLİĞİ VERİLİYOR ██
        │
        ▼
BattleActions.cs:365  battle.AddStructure(unit, structure, x, y);
        │
        ▼
Battle.cs:287         if (combatants.ContainsKey(unit) || structures.ContainsKey(unit))
Battle.cs:289             throw new ArgumentException("The unit is already in this battle.", nameof(unit));
```

`selectedUnit` **tanım gereği** `combatants` sözlüğünde:
`TryEnterPlacementMode` `selectedUnit == null` ise kipe hiç girmiyor, ve seçim
ancak **dolu** bir hücreye tıklanarak doğuyor:

```
BoardAdapter.cs:361   if (selectedUnit == null)
```

**██ HANGİ HÜCREDE NE OLUYOR — üç dal, ölçüldü: ██**
```
  tahta DIŞI hücre  ──► BattleActions.cs:347  RejectedInvalidCell    ✓ ret, istisna YOK
  DOLU hücre        ──► BattleActions.cs:357  RejectedCellOccupied   ✓ ret, istisna YOK
  tahta içi + BOŞ   ──► BattleActions.cs:365  AddStructure  ██ HER SEFERİNDE İSTİSNA ██
```

██ Yani **başarılı olması gereken tek dal** patlıyor. `PlacementOutcome.Placed`
arayüzden ULAŞILAMAZ; `CreateStructureVisual` üretimde HİÇ çağrılmaz. ██

```
BoardAdapter.cs:566   private void CreateStructureVisual(int x, int y)
```

**Testler neden yeşil:** `BattleActionsTests` her çağrıda **taze bir kimlik**
veriyor (`new Unit("Barracks")`), yani adaptörün çağrı **şekli** hiçbir testte
yok. `BoardAdapterTests.cs` diye bir dosya da yok —
`deep/00-iskelet.md:330-332` bunu zaten yazıyor.

**██ NE YAP: ██** İstisnayı **gör**, yukarıdaki üç sıçramayı kodda aç, zinciri
kendin izle. ██ Bu, bütün turun en öğretici on dakikası olacak ██ — çünkü
belgenin doğru olduğu yerle kodun doğru olduğu yerin nerede ayrıştığını kendi
gözünle göreceksin, ve [`deep/README.md`](../deep/README.md)'nin ilk kuralını
ilk kez gerçekten uygulayacaksın:

> *"İkisi çelişirse **kod kazanır**."*

██ **Düzeltmeyi bu turda yapma.** ██ Not al, ayrı bir tura bırak. Bugünkü soru
"okuduğumda anlayabilecek miyim", "kodu tamir edebilecek miyim" değil.

---

### ADIM 9 · [`konular/08-motor-cagri-dongusu.md`](../deep/konular/08-motor-cagri-dongusu.md) — tamamı (950)

**NEDEN BU SIRADA:** `07` `Update`'in **içini** anlatıyor; `08` `Update`'i
**kimin çağırdığını**. `08:391-394` bu bölüşmeyi kendisi yazıyor. Ayrıca `08`
zincirin **en iyi bağlanmış** dosyası (23 çıkış bağlantısı + bir `## İlgili`
bölümü): buradan sonrasında kaybolmazsın.

**AÇIK OLACAK KOD:**
```
Assets/Game/Unity/BoardAdapter.cs:232   private void Awake()
        ◄── private, ve motor yine çağırıyor
Assets/Game/Unity/BoardAdapter.cs:288   private void OnEnable()
Assets/Game/Unity/BoardAdapter.cs:290   battle.UnitStateChanged += OnUnitStateChanged;
        ◄── C# event
Assets/Game/Unity/BoardAdapter.cs:293   private void OnDisable()
Assets/Game/Unity/BoardAdapter.cs:317   private void Update()
Assets/Game/Unity/UnitView.cs:86        private void Awake()
Assets/Game/Core/PointerGesture.cs      public void Reset()   ◄── Unity mesaj ADI, sıfır anlam
ProjectSettings/EditorSettings.asset    m_EnterPlayModeOptionsEnabled: 0
```

**BU ADIMDAN SONRA CEVAPLAYABİLECEĞİN SORU:**
> *"`Awake` neden bir `event` **değil**, ve motor `private` bir metodu nasıl
> çağırabiliyor? Bu projede gerçekten hangi geri çağrılar tanımlı — ve
> `PointerGesture.Reset` neden onlardan biri **değil**?"*

---

### ██ DURMA NOKTASI 5 ██ — motorun sırasını KENDİN ölç

`08:344-370` bir deney tarif ediyor ve önden şunu diyor (`08:346`):
> *"██ Buradaki hiçbir iddiayı bana güvenerek kabul etme. ██"*

**DENEY:** Geçici bir `MonoBehaviour`, yedi geri çağrının her birine bir
`Debug.Log($"{Time.frameCount} {name} Awake")`, **iki ayrı GameObject** (A ve
B), Play, Console'u zaman sırasına al.

**SINADIĞIN İDDİA:** *"bütün `Awake`'ler bütün `Start`'lardan önce"* —
██ "A önce" DEĞİL ██ (`08:363-365`). Gözlem bir kez tuttu diye kural sanma.

Sonra Play'deyken B'nin bileşen kutusunu **kapat-aç**: `OnDisable` · `OnEnable`
görürsün, `Awake` ve `Start` **tekrar etmez**.

██ Deneyi bitirince geçici script'i SİL. Repoya ekleme. ██

---

# ██ OTURUM 5 · DÜĞÜM VE DEFTER ██  (~2 saat · 2.243+ satır)

### ADIM 10 · [`konular/01-olay-zinciri.md`](../deep/konular/01-olay-zinciri.md) — tamamı (336)

**NEDEN EN SONDA — ██ ve numarası `01` olmasına rağmen ██:** Üç ipliğin
düğümlendiği yer burası ve üçünü de artık kapattın:

```
   konular/02   ── ad alanı + assembly       (01:55)    ✓ ADIM 2'de kapandı
   konular/05   ── Alive / Downed / Dead     (01:65)    ✓ ADIM 5'te kapandı
   dil/04       ── event · Action · lambda   (01:176)   ── ADIM 11'de gelecek
```

██ `dil/04` henüz kapanmadı ve bu bilinçli: ██ `01` delegenin **ne yaptığını**
gösteriyor, `dil/04` **ne vaat ettiğini**. Zinciri önce gör, sözleşmeyi sonra
oku — tersi, malzemeyi hiç kullanmadan öğrenmek olurdu.

**AÇIK OLACAK KOD — zincirin dört durağı, sonra ekran yarısı:**
```
Assets/Game/Core/Combat/UnitLifecycle.cs:80    public event Action<UnitState> StateChanged;
Assets/Game/Core/Combat/Combatant.cs:90        this.lifecycle.StateChanged += OnLifecycleStateChanged;
        ◄── kurucunun SON satırı
Assets/Game/Core/Combat/Combatant.cs:111       public event Action<UnitState, UnitState> StateChanged;
Assets/Game/Battle/Battle.cs:81                private readonly Dictionary<Unit, Action<UnitState, UnitState>> stateForwarders =
Assets/Game/Battle/Battle.cs:179               public event Action<Unit, UnitState, UnitState> UnitStateChanged;
Assets/Game/Unity/BoardAdapter.cs:290          battle.UnitStateChanged += OnUnitStateChanged;
Assets/Game/Unity/BoardAdapter.cs:295          battle.UnitStateChanged -= OnUnitStateChanged;
        ◄── ekran yarısı: 310 → 954 → UnitView.cs:173
Assets/Game/Unity/BoardAdapter.cs:310          private void OnUnitStateChanged(Unit unit, UnitState from, UnitState to)
Assets/Game/Unity/BoardAdapter.cs:954          private void ApplyStateVisual(Unit unit, UnitState state)
Assets/Game/Unity/UnitView.cs:173              public void SetState(UnitState state)
```

**BU ADIMDAN SONRA CEVAPLAYABİLECEĞİN SORU:**
> *"`Battle`'daki o tuhaf sözlük neden var? Neden `-=` **sessizce** başarısız
> olabiliyor, ve sızıntının oku hangi yöne gidiyor?"*

---

### ADIM 11 · `dil/04` (460) ██→██ `dil/06` (738) — bu sırayla, ZORUNLU

Sıra bir tercih değil; [`dil/06`](../deep/dil/06-delege-arka-taraf.md) okuyucusuna
açıkça yazıyor (`dil/06:11-15`):

> *"[`04`] bu malzemenin **sözleşmesini** anlatıyor […] **Orayı okumadan buraya
> girme**; burada hiçbiri tekrar edilmiyor."*

██ Zincirdeki TEK açık ön koşul beyanı bu. ██ Bölüşme:

```
   dil/04 sorar:  "+= ne VAAT EDİYOR"          ── sözleşme
   dil/06 sorar:  "+= ÇALIŞTIĞINDA ne oluyor"  ── nesne, Target/Method, çağrı listesi
```

- [`dil/04-delege-olay-ve-kapanis.md`](../deep/dil/04-delege-olay-ve-kapanis.md)
- [`dil/06-delege-arka-taraf.md`](../deep/dil/06-delege-arka-taraf.md)

██ `dil/06:395-533` doğrudan senin sorduğun bir pasajı cevaplıyor ██ —
`Combatant` kurucusunun son iki satırının neden **en sonda** durduğunu.

**KOŞTURULABİLİR:** `dil/06:116-143` geçici bir `DescribeSubscribers()` üyesi
tarif ediyor. ██ Yaz, ölç, sil. ██ `seen.Add` abone ettiğinde `Target`'ın bir
**liste** olduğunu görmek, delegenin ne olduğunu tek seferde kapatıyor.

**BU ADIMDAN SONRA CEVAPLAYABİLECEĞİN SORU:**
> *"`+=` derleyicide neye dönüşüyor, `event` hangi üç üyeyi doğuruyor, ve bir
> abone **fırlarsa** faturayı kim ödüyor?"*

---

### ADIM 12 · [`dil/07-bellek-canlilik-ve-yikim.md`](../deep/dil/07-bellek-canlilik-ve-yikim.md) (709)

**NEDEN BURAYA AİT:** `dil/07:277-341` ADIM 10'da okuduğun sözlüğün **bellek**
faturasını ölçüyor — yedi hop, ve tek bir `Combatant` referansı bütün savaşı
erişilebilir tutuyor. ██ `01` davranış faturasını, `dil/07` bellek faturasını
veriyor: aynı eksik `-=`, iki ayrı fatura. ██

**ÖN KOŞUL — dosya kendi yazıyor** (`dil/07:15-18`): `dil/05` semantiği anlatır,
bu dosya **depolamayı**. `dil/05`'i henüz okumadın; `:93-128`'in dört soruluk
figürü yine de ayakta, ama `:157-255` (depolama) için `dil/05` gerekiyor.
██ Sıkışırsan ADIM 13'e geç, sonra dön. ██

---

### ADIM 13 · `dil/05` (660) · `dil/02` (296) · `dil/03` (632) — sıra serbest

Üçü de **referans**: baştan sona okunabilir, ama asıl işlevleri bir soru
doğduğunda açılmak.

| Dosya | Kapsadığı |
|---|---|
| [`dil/05`](../deep/dil/05-deger-referans-ve-kimlik.md) | "aynı" sözcüğünün dört ölçüsü · `ReferenceEquals` · `enum` · `out` · `=>` · `switch` · `%` |
| [`dil/02`](../deep/dil/02-koleksiyonlar-ve-salt-okunur.md) | `IReadOnlyList` ≠ değişmez · indeksleyici · `object Current` · `Dictionary` |
| [`dil/03`](../deep/dil/03-hata-bildirme-ve-dogrulama.md) | `nameof` · dört istisna tipi · `Math.Max` kelepçesi |

**██ `dil/02` KISA (296 satır) VE BU BİR EKSİKLİK DEĞİL: ██** ölçüldü —
`dil/README.md:34`'ün saydığı beş konunun **beşi de** var ve `dil/` ağacının
yedi adımlık kalıbı tam. Konusu küçük, dosya değil. Ayrıca `konular/08:570` ve
`dil/07:89` ona **dayanıyor** ve tekrar etmiyor: kısalığın sebebi kapsam değil,
██ iş bölümü ██.

---

### ADIM 14 · `Docs/ogrenme/` — ██ `01` → `03` → `02` ██

[`ogrenme/README.md`](README.md) bu sırayı kendisi öneriyor:

```
   01  "BUGÜN NEYİ BİLİYORUM"      ── önce elindekinin ADINI öğren
   03  "HANGİ KAVRAMIN SAHİBİ VAR" ── sonra haritaya bak
   02  "NE YOK, NE ZAMAN GELİR"    ── en sonda ileriye bak
```

- [`01-koda-gomulu-desenler.md`](01-koda-gomulu-desenler.md)
- [`03-kavram-borc-defteri.md`](03-kavram-borc-defteri.md)
- [`02-sonraki-asamalar.md`](02-sonraki-asamalar.md)

██ Bu ağaç zincirin **güvenilirlik bakımından en sağlam** parçası. ██ Ölçüldü:
11 rastgele `.cs:satır` atfı elle doğrulandı, **11'i de** tam olarak adlandırdığı
yapıya düştü. Ayrıca `ogrenme/03` bir makine kapısıyla bağlı.

`ogrenme/01` sana `deep/` ağacının **hiç vermediği** şeyi veriyor: dokuz desenin
**adı**, her biri için hangi baskının onu doğurduğu, hangi SOLID harfini
taşıdığı, ve ██ neyin yanlış hatırlandığı ██ — *"MoveAction bir Command
DEĞİL"*, *"kural sınıfları Strategy DEĞİL"*, *"BoardAdapter bir GoF Adapter
DEĞİL"*.

**BU ADIMDAN SONRA CEVAPLAYABİLECEĞİN SORU:**
> *"Bu projede hangi desenleri kullandım, hangi baskı her birini doğurdu, ve
> hangilerini **bilerek kullanmadım**?"*

██ Bu, bir sonraki mülakat sorusunun birebir kendisi. ██

---

### ██ DURMA NOKTASI 6 — SON ██ — defteri kendi kapısıyla sına

```powershell
python Tools/check-curriculum-coverage.py
python Tools/check-doc-links.py
python Tools/check-cited-names.py
```

Üçü de temiz koşmalı. `check-curriculum-coverage.py` bugünkü çıktısı:
`kavram satırı 87 · KAPALI 62 · KISMİ 13 · HENÜZ YOK 12 · ihlal 0`.

██ Turu bitirdiğinde o 13 "KISMİ" satırın kaçının senin için artık "KAPALI"
olduğunu **kendin işaretle**. Defterin işi bu. ██

---

# ██ TAM SIRA — TEK BAKIŞTA ██

```
 OTURUM 1  ─ BÜTÜNÜ KUR ─────────────────────────────────── 1.262 satır ─ ~2 sa
   1  deep/00-iskelet.md              604   kod: yok
   2  konular/02-assembly-duvari      658   kod: 4 × .asmdef + BoardAdapter.cs:48
   ██ DUR ██  run-editmode-tests.ps1  ·  test asmdef'inin references dizisi

 OTURUM 2  ─ SAHİPLİK VE DURUM ─────────────────────────── 1.825 satır ─ ~2 sa
   3  konular/03-tahta-sahipligi      503   kod: Battle.cs · UnitGrid.cs · BattleActions.cs:207
   4  dil/01-degismezlik              598   kod: TurnState.cs:43-53 · UnitGrid.cs
   5  konular/05-yasam-dongusu        724   kod: UnitLifecycle · StructureLifecycle · TargetingRules
   ██ DUR ██  Downed_IsTheOnlyStateBothAbilitiesAccept testini aç ve OKU

 OTURUM 3  ─ KARAR VE RET ──────────────────────────────── 1.342 satır ─ ~2 sa
   6  konular/06-sonuc-enumlari       714   kod: dört enum + ReactToMove/ReactToAttack
   7  konular/04-karar-sirasi         628   kod: BattleActions · AttackAction · MoveAction
   ██ DUR ██  dört "Prefers" testi; hangisinin neden YAZILAMADIĞINI gör

 OTURUM 4  ─ MOTOR SINIRI ──────────────────────────────── 1.866 satır ─ ~2,5 sa
   8  konular/07-tiklamadan-eyleme    916   kod: BoardAdapter Update/HandleClick · PointerGesture
   ██ DUR ██  ██ EDITOR · Play · ve YERLEŞTİRME KIRILACAK — DURMA NOKTASI 4 ██
   9  konular/08-motor-cagri-dongusu  950   kod: Awake/OnEnable/Update · EditorSettings.asset
   ██ DUR ██  iki bileşenli günlük deneyi — 08:344-370, sonra script'i SİL

 OTURUM 5  ─ DÜĞÜM VE DEFTER ───────────────────────────── 2.243+ satır ─ ~2 sa
  10  konular/01-olay-zinciri         336   kod: UnitLifecycle:80 · Combatant:86,107 · Battle:74,172
  11  dil/04 (460) ██→██ dil/06 (738)       kod: Combatant kurucusunun son iki satırı
  12  dil/07-bellek-canlilik          709   kod: DespawnView · RemoveUnit
  13  dil/05 · dil/02 · dil/03      1.588   referans — sıra serbest
  14  ogrenme/01 ██→██ 03 ██→██ 02  1.685   desen adları · kapsama tablosu · aşamalar
   ██ DUR ██  üç kapıyı koştur; KISMİ satırları kendin güncelle
```

---

# ██ SEÇİLEN / REDDEDİLEN ██

## SEÇİLEN
`00 → 02 → 03 → dil/01 → 05 → 06 → 04 → 07 → 08 → 01 → dil/04 → dil/06 → dil/07
→ dil/05,02,03 → ogrenme/01,03,02`

██ İki **bağımsız** yöntem aynı başlangıcı verdi: ██
① **Kenar sayımı** — `02`'nin 6 gelen kenarı var, `00-iskelet`'in 0.
② **Anlatının kendi beyanı** — `deep/00-iskelet.md:523-527`:
*"bu dosya baştan sona → `02` → `03` → `07`"*.
Bu belge o dört adımlık yolu **on dört adıma** genişletiyor ve aradaki boşlukları
bağımlılık yönüne göre dolduruyor.

## REDDEDİLEN 1 · Numara sırası (`01 → 02 → … → 08`)
Sekiz `konular/` dosyasından yalnız **ikisi** numara sırasında doğru yerde. `01`
ilk okunursa üç tanımsız kavram taşıyor ve dosya kendi ifadesiyle *"bütün hikâye
bu tek karardan doğuyor"* (`01:59`) diyerek okuyucuyu ██ tanımsız bir gerekçenin
üstüne ██ oturtuyor.

## REDDEDİLEN 2 · Dosyaları yeniden numaralandırmak
Numaralar 30+ yerde ve dört makine kapısında **çapa**. Yeniden numaralandırma
`check-doc-links.py` ile `check-curriculum-coverage.py`'yi anında kırar.
██ Numara DOSYA KİMLİĞİ; sıra AYRI BİR BELGEDE. ██

## REDDEDİLEN 3 · `dil/` ağacını `konular/`'dan önce okumak
Beş `dil/` dosyası `konular/`'a geri bağlanıyor ve hepsi *"bunun proje tarafı
şurada"* diyor. Ok yönü net: ██ `dil/` `konular/`'ı açıklıyor, tersi değil. ██
Tek istisna `dil/01` — `03`'ün hemen ardına konmasının sebebi tam olarak bu
(`dil/01:176` `03`'e işaret ediyor).

## REDDEDİLEN 4 · Yerleştirme hatasını okumadan ÖNCE düzeltmek
Bugünkü soru *"okuduğumda anlayabilecek miyim"*, kod tamir etmek değil. Ayrıca
██ hatayı Play'de görmek, *"İkisi çelişirse kod kazanır"* kuralının
koşturulabilir tek örneği ██ — ve turun en öğretici on dakikası. Düzeltme ayrı
bir tura ait: not al, geç.

## REDDEDİLEN 5 · `deep/kod/` ağacını (14.788 satır) sıraya dahil etmek
33 ayna belge, tip başına. Sıraya girseydi 9-11 saatlik bütçeyi **üçe**
katlardı. ██ Doğru kullanımı referans: ██ bir tipe **dokunmadan önce** onun
aynasını aç — [`deep/kod/README.md`](../deep/kod/README.md).

---

## İlgili

- Üç ağacın yönlendirmesi: [`../deep/README.md`](../deep/README.md)
- Bu ağacın yönlendirmesi: [`README.md`](README.md)
- Tip başına ayna belgeler: [`../deep/kod/README.md`](../deep/kod/README.md)
- Üst düzey belge haritası: [`../README.md`](../README.md)
