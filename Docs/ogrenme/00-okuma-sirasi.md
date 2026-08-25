# Okuma sırası — yarın sabah izlenecek yol

> **Ne zaman oku:** `Docs/deep/` ağacına ilk kez oturmadan **önce**, ve her
> oturumun başında bir kez daha.
> **Ne yapar:** 28.806 satırlık üç ağacı 15 adıma ve 5 oturuma bölüyor; her
> adımın **niye orada** olduğunu ve yanında **hangi `.cs` dosyasının** açık
> duracağını yazıyor.
> **Ne YAPMAZ:** hiçbir mekanizmayı anlatmaz. Bu bir yol tarifi, bir ders değil.

---

## ***ÖNCE ŞUNU OKU: NUMARALAR BİR SIRA DEĞİL***

`konular/01`, `dil/05`, `ogrenme/03` gibi adlardaki sayılar
*****bir DOSYA KİMLİĞİDİR, bir okuma sırası değil.*****

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

   >> Numara ucuz görünür, ama 30+ yerde çapa olarak kullanılıyor. <<
   Sıra ise tek bir yerde yaşayabilir: bu dosyada.
```

**Numara dosyanın ADIDIR. Sıra bu belgededir. İkisi ayrı şeyler.** Ayrı
kalmaları da bilinçli bir karar.

---

## ***TOPLAM SÜRE VE OTURUM BÖLÜMÜ***

```
   belge          28.806 satır   yoğun teknik Türkçe anlatı
   yanında kod     ~5.300 satır   (aynı satırları birkaç kez açacaksın)
   ────────────────────────────────────────────────────────────────
   GERÇEKÇİ TAHMİN
     saf okuma                          5 – 6 saat
     kod yan yana açıp izleme         + 2 – 3 saat
     durma noktalarında koşturma      + 1,5 saat
   ────────────────────────────────────────────────────────────────
   TOPLAM                              9 – 11 saat  ·  >> BEŞ OTURUM <<
```

**Tek oturumda bitirme.** Ölçü: `05`, `06`, `07` ve `08` dosyalarının her biri
700-950 satır ve her biri **kendi karar ağacını** taşıyor. Arka arkaya iki
tanesi okunduğunda ikincinin karar ağacı birincininkiyle karışıyor.

*****Oturum sınırları tesadüf değil: her oturum bir KOŞTURMA ile bitiyor.*****
Koşturmadan ilerlemek, kapatılmamış bir haritanın üstüne yenisini koymaktır. Bir
durma noktasını atlamak, o oturumu hiç okumamaktan **daha kötüdür**: okuduğunu
sanırsın.

---

## ***BAĞIMLILIK GRAFİĞİ — sıranın türetildiği yer***

Kenar ölçütü: **X → Y** demek, *"X'in taşıyıcı bir gerekçesi Y'nin konusuna
dayanıyor ve X onu kendi içinde tanımlamıyor"*. Üslup benzerliği kenar değildir.

```
                    ╔══════════════════════════╗
                    ║   deep/00-iskelet.md     ║  ◄── >> TEK ACYCLIC KÖK <<
                    ║   gelen kenar : 0        ║      hiçbir şeye bağımlı değil
                    ╚════════════╤═════════════╝      her şeyi SIĞ tanıtır
                                 │
                                 ▼
                    ╔══════════════════════════╗
                    ║   konular/02  DUVAR      ║  ◄── >> OMURGA <<
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
                                       >> YAPRAK << kimse buna bağımlı değil
                                               │
                                               ▼
                                    ╔═════════════════════╗
                                    ║   konular/01        ║
                                    ║  >> BULUŞMA NOKTASI <<
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

### ***`konular/01`'in gerçek rolü — en pahalı yanlış anlama***

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

   >> 01 bir GİRİŞ değil, bir DÜĞÜM. << Üç iplik orada düğümleniyor.
   Erken okunursa üçü de tanımsız; geç okunursa düğüm kendiliğinden çözülüyor.
   Zincirin en kısa dosyası (336 satır) tesadüfen değil, BULUŞMA NOKTASI
   olduğu için kısa: üç ipliği anlatmıyor, bağlıyor.
```

**Bu kararın iki bağımsız kanıtı var** ve ikisi de aynı sonucu veriyor:
① kenar sayımı (yukarıdaki grafik), ② anlatının kendi beyanı. İkincisinin
ölçüsü şu: `deep/00-iskelet.md:523-527` kendi önerdiği yolda `01`'i
**hiç anmıyor**.

---

# ***OTURUM 1 · BÜTÜNÜ KUR***  (~2 saat · 1.262 satır)

Amaç tek bir mekanizmayı öğrenmek değil: **yüzeyin tamamını görmek** ve duvarın
nerede durduğunu bilmek.

### ADIM 1 · [`deep/00-iskelet.md`](../deep/00-iskelet.md) — tamamı (604 satır)

**NEDEN BU SIRADA:** Bu dosya zincirin tek acyclic köküdür; gelen kenarı
**sıfır**. Hiçbir dosyaya bağımlı değil, ve her kavramı sığ biçimde tanıtıyor.
Dahası bu dosya
senin kendi cümlenle yazılmış (`00-iskelet.md:18-21`):

> *"sen sadece şuraya bak şuraya dediğinde diğer kısımlarını görmediğim için
> kafamda oturtturamıyorum"*

***Bugünkü sorun ile bu dosyanın yazılma sebebi **aynı** sorun.***

**AÇIK OLACAK KOD:** ***yok.*** İlk geçişte kod açma. Bu dosya kasten tip adı
kullanmadan başlıyor.

**BU ADIMDAN SONRA CEVAPLAYABİLECEĞİN SORU:**
> *"Bu oyun ne? Tahtada ne var, bir tıklama kaç anlama gelebilir, bir birim
> nasıl ölür, ve dört derleme birimi hangi sırayla birbirini görüyor?"*

*****ATLA:***** `§6 Aynı basınç, üç başka oyun` (`:531-581`). O bölüm
karşılaştırma; ilk geçişte gerekmez. Beşinci oturumda geri gel.

---

### ADIM 2 · [`konular/02-assembly-duvari.md`](../deep/konular/02-assembly-duvari.md) — tamamı (876)

**NEDEN BU SIRADA:** ***Bu dosya zincirin omurgasıdır: altı dosya ona bağımlı,
o hiçbir dosyaya bağımlı değil.*** Şu altısı — `01`, `03`, `04`, `05`, `06`,
`07` — bir noktada "assembly duvarı" diyor ve **hiçbiri onu tanımlamıyor**. Bu
adım altı ön koşulu birden kapatıyor. Zincirdeki en yüksek getirili tek adım bu.

**AÇIK OLACAK KOD — dört `.asmdef`, hepsi kısa:**
```
Assets/Game/Core/GridStrategy.Core.asmdef              references: []
Assets/Game/Core/Combat/GridStrategy.Combat.asmdef     references: []
Assets/Game/Battle/GridStrategy.Battle.asmdef          references: [Core, Combat]
Assets/Game/Unity/GridStrategy.Unity.asmdef            references: [Core, Combat, Battle]
```
artı iki `.cs` yeri:
```
Assets/Game/Unity/BoardAdapter.cs:49      using Battle = global::GridStrategy.Battle.Battle;
Assets/Game/Core/Combat/AttackResolver.cs IsWithinRange(int distance, AttackProfile)
```

**BU ADIMDAN SONRA CEVAPLAYABİLECEĞİN SORU:**
> *"Klasör, ad alanı ve assembly neden **üç ayrı şey**? `Core` ile `Combat`
> neden birbirini görmüyor ve bu bana ne fatura kesiyor? CS0118 neden bir
> `using` ile çözülmüyor?"*

---

### ***DURMA NOKTASI 1*** — duvarın faturasını satın al

**KOMUT:**
```powershell
.\Tools\run-editmode-tests.ps1
```

**KAPI:** testler yeşil mi.

**SONRA — gözünle doğrula:** `Assets/Tests/EditMode/Combat/` klasöründeki
`.asmdef`'i aç ve `references` dizisine bak. `02:216-222`'nin iddiası şu:

> *"`GridStrategy.Combat.EditModeTests`'in `references` dizisinde **tek bir oyun
> assembly'si var**"*

***Doğrulamadan ilerleme.*** Duvarın "faturası"nın satın aldığı şey tam olarak
o tek satırdır. Zincirin geri kalanı da bu takasa dayanıyor.

---

# ***OTURUM 2 · SAHİPLİK VE DURUM***  (~2 saat · 1.825 satır)

### ADIM 3 · [`konular/03-tahta-sahipligi.md`](../deep/konular/03-tahta-sahipligi.md) — tamamı (637)

**NEDEN BU SIRADA:** `02`'nin hemen ardından, çünkü `03`'ün "üç katman"
figürünün **orta katmanı** (`internal Board`) doğrudan duvara dayanıyor ve
`03:271` *"Sözleşme assembly duvarında biter"* diyor. Duvarı bilmeden o cümle
okunamaz. Ayrıca `03` zincirin **en dürüst** dosyası: `:19-22` garantinin nerede
biteceğini önden vaat ediyor ve `:220-271`'de gerçekten ödüyor.

**AÇIK OLACAK KOD:**
```
Assets/Game/Battle/Battle.cs      board alanı · internal Board üyesi · kurucu
Assets/Game/Core/UnitGrid.cs      PlaceUnit · RemoveUnit · TryGetUnit · ThrowIfOutsideGrid
Assets/Game/Battle/BattleActions.cs:207
    MoveAction.Execute(battle.Board, unit, fromX, fromY, toX, toY, profile);
                                  >> Board'un TEK çağırısı — tüm projede <<
```
***O tek satırı **kendin say**:*** `grep -n 'battle.Board' Assets/Game/Battle/BattleActions.cs`

**BU ADIMDAN SONRA CEVAPLAYABİLECEĞİN SORU:**
> *"`readonly` tam olarak neyi **korumuyor**? Tahtaya kim yazabiliyor, ve
> derleyicinin garantisi tam olarak hangi satırda bitiyor?"*

---

### ADIM 4 · [`dil/01-degismezlik-anahtar-kelimeleri.md`](../deep/dil/01-degismezlik-anahtar-kelimeleri.md) — tamamı (780)

**NEDEN BU SIRADA:** `03`'ün ikinci durağı `readonly`'nin **korumadığı** şeyi
gösteriyor ama **neden** korumadığını dil düzeyinde vermiyor. Onu `dil/01`
veriyor. Üstelik `dil/01:176` zaten `03`'e geri işaret ediyor:

> *"Tekrarlamıyorum, gerekçesi ve üç katmanlı haritası burada:
> `konular/03-tahta-sahipligi.md`"*

***İki dosya bilerek bölüşmüş. `03`'ü okumadan `dil/01`'i okumak tersten
okumaktır.***

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

### ADIM 5 · [`konular/05-yasam-dongusu.md`](../deep/konular/05-yasam-dongusu.md) — tamamı (892)

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

### ***DURMA NOKTASI 2*** — bir testin ADI bir kararı nasıl taşır

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

***Aç ve oku. Bir test **adının** bir tasarım kararını nasıl taşıdığını görmek,
bu projedeki en aktarılabilir beceri.***

---

# ***OTURUM 3 · KARAR VE RET***  (~2 saat · 1.342 satır)

### ADIM 6 · [`konular/06-sonuc-enumlari.md`](../deep/konular/06-sonuc-enumlari.md) — tamamı (880)

**NEDEN BU SIRADA — ***ve neden `04`'ten ÖNCE***:** Numara sırası `04 → 06`
diyor; bağımlılık **tersini** diyor. İkisi de aynı ölçütü kuruyor:

```
   06:494    "Ayıraç sebep sayısı değil, >> DAVRANIŞ sayısı <<"
   04:606    "ayırıcı şey SEBEP sayısı değil, >> DAVRANIŞ sayısı <<"
```

***Aynı kural iki dosyada iki kez kuruluyor, ve hiçbiri ötekini anmıyor.***
`06` onu **kurar** (dört enum, on bir ret değeri, tam tablo); `04` onu bir
**sıra kararına uygular**. ***Kuran önce okunur.***

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

### ADIM 7 · [`konular/04-karar-sirasi.md`](../deep/konular/04-karar-sirasi.md) — tamamı (837)

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

*****BU DOSYANIN VERMEDİĞİ TEK ŞEY:***** `04` bu üç tipin rolünü 628 satır
boyunca anlatıyor ve o role **hiç ad vermiyor**. O adı
[`ogrenme/01` §2](01-koda-gomulu-desenler.md#2-akis-sahibi-transaction-script-command-degil)
veriyor: ***rolün adı **akış sahibi** (transaction script), ve bu bir Command
DEĞİL.*** Ölçüsü şu: üçü de `static class` ve tek alanı yok. "Bu projede hangi
desenleri kullandın" sorusunun cevabı orada; ADIM 14'te kapatacaksın.

**BU ADIMDAN SONRA CEVAPLAYABİLECEĞİN SORU:**
> *"Aynı tıklama neden **dört farklı sebeple** reddedilebiliyor ve hangisi
> kazanıyor? Geri dönülemez çizgi nerede, ve altına düşen bir kural neden kural
> olmaktan çıkıyor?"*

---

### ***DURMA NOKTASI 3*** — "Prefers" testleri: koşturulabilir karar ağacı

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

***Her `Prefers` testi bir SIRA KARARINI tutuyor.*** Ama `04:412` şunu da
söylüyor:
> *"Bunu ölçen bir test yok, **ve olmaması doğru**."*

Bir sıra kararının **ne zaman** test edilebilir olduğunu (iki cevabın farklı
olması gerekir) ve ne zaman edilemeyeceğini bu dört test gösteriyor.

---

# ***OTURUM 4 · MOTOR SINIRI***  (~2,5 saat · 1.866+ satır)

***Bu en uzun oturum, ve **içinde tek çalışmayan senaryo var.***** Aşağıdaki
uyarıyı oturuma başlamadan oku.

### ADIM 8 · [`konular/07-tiklamadan-eyleme.md`](../deep/konular/07-tiklamadan-eyleme.md) — tamamı (1126)

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

### ADIM 8b · [`ogrenme/11-unity-penceresi-adim-adim.md`](11-unity-penceresi-adim-adim.md) — tamamı

**NEDEN TAM BURADA:** aşağıdaki durma noktası bu turda Unity penceresini
**ilk kez** açtırıyor ve doğrudan `Play`'e bastırıyor. Ölçü: bu belgedeki altı
durma noktasının beşi bir komut ya da bir test dosyası açtırıyor, yalnız bu
biri Editör'ü açtırıyor. Ama sahne bugün eksik: `placementGhost` alanı atanmamış
ve sahnede referans verilebilecek bir `SpriteRenderer` **hiç yok** (ölçüldü:
`SampleScene.unity` içinde sıfır tane). Bu yüzden aşağıdaki ② senaryosu kendi
ilk cümlesinde düşer — hayalet belirmez, kip hiç açılmaz.

`11` o boşluğu kapatır: on altı serileştirilmiş alanın her birini Inspector
başlığıyla tanıtır, hangisine ne sürükleneceğini yazar, ve sahneyi beş adımda
onartır.

***Ayrıca bu adım bir ÖLÇÜM taşıyor ve ölçüm bu ağaçtaki açık bir soruyu
kapatıyor:*** sahne dosyasında **yazılı olmayan** bir `[SerializeField]` alanı
hangi değeri alır — C# alan başlatıcısını mı, yoksa tipin sıfır değerini mi?
`11` cevabı `Library/` altındaki içe aktarılmış prefab verisinden bayt düzeyinde
okuyor. Aynı soru [`08-unity-altyapisi.md`](08-unity-altyapisi.md)'nin
ADIM 3 ve ADIM 5 adımlarında **DOĞRULANMADI** diye işaretliydi.

**AÇIK OLACAK KOD:**
```
Assets/Game/Unity/BoardAdapter.cs   13 [SerializeField] alani
Assets/Game/Unity/UnitView.cs        3 [SerializeField] alani
Assets/Scenes/SampleScene.unity      bugun 4 alan yazili
Assets/Game/Prefabs/Unit.prefab      bugun 1 alan yazili (uctan)
```

***ONARIM KUSURU DÜZELTMEZ, KUSURU ULAŞILABİLİR YAPAR.*** Aşağıdaki ②'nin
anlattığı `ArgumentException` ancak `11` uygulandıktan sonra görülebilir;
uygulanmadan görünen şey başka bir satırdır ve o satır turun dersi değildir.
***REDDEDİLEN 4 yerinde duruyor:*** kod hâlâ bu turda düzeltilmiyor.

**BU ADIMDAN SONRA CEVAPLAYABİLECEĞİN SORU:**
> *"Inspector'da duran bir alanın değeri nereden geliyor — sahne dosyasından mı,
> C# alan başlatıcısından mı? Ve boş bir referans alanı kodu tam olarak hangi
> satırda durduruyor?"*

---

### ***DURMA NOKTASI 4*** — ***EDITOR · VE BURADA BİR ŞEY KIRILACAK***

Unity'yi aç, `Assets/Scenes/SampleScene.unity`, **Play**.

***ÖN KOŞUL: ADIM 8b.*** Sahne onarılmadan aşağıdaki ② hiç başlamaz.

#### ① Çalışacak olan — dört dal

Bir askere tıkla (seçilir, çerçeve açılır) → boş hücreye tıkla (yürür) →
düşmana tıkla (vurur) → aynı askere ikinci kez tıkla (seçim bırakılır).
`07:560-572`'deki niyet tablosunun dört dalı.

#### ② ***BİR ZAMANLAR ÇALIŞMAYAN — VE 2026-08-25'TE KAPANAN***

Bir birim seç, **`B`**'ye bas, sürükle, ***tahta içindeki boş bir hücreye***
bırak. Yapı yerleşir ve görseli ekranda belirir.

***Bu adım bugüne kadar PATLIYORDU ve teşhis bu belgede yazılıydı.*** Aşağıdaki
tanı hâlâ duruyor, çünkü asıl ders kusurun kendisi değil — bir belge ağacının
koda karşı ölçüldüğünde henüz kimsenin görmediği bir kusuru nasıl bulduğu.

**Teşhis — o günkü hâliyle, üç sıçrama:**

```
CommitPlacement    Unit placer = selectedUnit;
                   BattleActions.PlaceStructure(battle, placer, ...)
                                                        ^
                   >> YAPIYA, ZATEN KAYITLI BIR BIRIMIN KIMLIGI VERILIYORDU <<
        |
        v
BattleActions.cs:365   battle.AddStructure(unit, structure, x, y);
        |
        v
Battle.cs:287          if (combatants.ContainsKey(unit) || structures.ContainsKey(unit))
Battle.cs:289              throw new ArgumentException("The unit is already in this battle.", ...)
```

`selectedUnit` ***tanım gereği*** `combatants` sözlüğündeydi: `TryEnterPlacementMode`
(`BoardAdapter.cs:458`) `selectedUnit == null` ise kipe hiç girmiyor, ve seçim
ancak **dolu** bir hücreye tıklanarak doğuyor.

*****ÜÇ DAL — o gün ölçülmüştü:*****
```
  tahta DISI hucre  --> BattleActions.cs:347  RejectedInvalidCell   ret, istisna YOK
  DOLU hucre        --> BattleActions.cs:357  RejectedCellOccupied  ret, istisna YOK
  tahta ici + BOS   --> BattleActions.cs:365  AddStructure  >> HER SEFERINDE ISTISNA <<
```

Yani ***başarılı olması gereken tek dal*** patlıyordu. `PlacementOutcome.Placed`
arayüzden ulaşılamazdı ve `CreateStructureVisual` üretimde hiç çağrılmıyordu.

**ONARIM — ve neden ORAYA yapıldı:**

```
BoardAdapter.cs:617   var structureUnit = new Unit($"Structure_{x}_{y}");
BoardAdapter.cs:624   PlaceStructure(structureUnit, NewStructure(placer), x, y);
```

Kelepçe gevşetilmedi, ***çağıran düzeltildi.*** İki bağımsız kanıt bunu
söylüyordu: `BattleActions.PlaceStructure`'ın kendi belgesi o argümanı
*"yapının tahtadaki kimliği"* diye tarif ediyor, ve
`BattleActionsTests.PlaceStructure_SameIdentityTwice_Throws` o kelepçeyi
**bilerek** koruyor. Kural doğruydu; sözleşmeye uymayan taraf adaptördü.

***Ve kusurun yıllarca görünmemesinin tek sebebi de yazılıydı:*** `BoardAdapterTests.cs`
diye bir dosya yoktu. Bugün var ve **on bir testi** geçiyor; ilk ikisinin adı
kusurun kendisini anlatıyor:

```
CommitPlacement_OnAFreeCell_PlacesTheStructureAndSaysPlaced
CommitPlacement_GivesTheStructureItsOwnIdentity_NotThePlacers
```

*****NE YAP:***** Önce teşhisi oku, sonra `BoardAdapter.cs:617`'yi aç ve tek
satırlık onarımı gör. Ardından `BoardAdapterTests.cs`'i aç: **kusurun kendisi
artık bir testin adında yaşıyor.** ***Bu, bütün turun en öğretici on dakikası*** —
çünkü zincirin bir kusuru önce ***yazdığını***, sonra ***kapattığını***, sonra
kapanışı bir teste ***çivilediğini*** aynı sayfada göreceksin.

Ve [`../deep/README.md`](../deep/README.md)'nin ilk kuralının neye yaradığını da
göreceksin:

> *"İkisi çelişirse **kod kazanır**."*

O kural olmasaydı belge, kendi yazdığı teşhisi yıllarca doğru sanmaya devam
ederdi.
---

### ADIM 8c · [`ogrenme/12-unity-editor-baglama.md`](12-unity-editor-baglama.md) — tamamı

**NEDEN TAM BURADA:** `8b` bugünkü sahneyi **onarır**, `8c` üstüne **yeni
katmanı kurar**. Sıra tersine çevrilemez: panel kodu tahtayı `IPlacementBoard`
üzerinden çağırıyor ve o çağrıların gideceği yer `8b`'de bağlanan sahnedir.

***Ölçü, ve bu adımın var olma sebebi:*** onuncu kapı
(`Tools/check-asset-inventory.py`) bugün **kırmızı** ve tam yedi ihlal sayıyor —
ikisi `.asset` örneği yokluğu, beşi sahne bağı yokluğu. Yani kod tarafı bitti,
editör tarafı hiç başlamadı, ve bunu söyleyen şey bir görüş değil bir kapı.

`8c` bittiğinde o kapı yeşile döner. ***Editör işinin bitip bitmediğini sana
kapı söyleyecek.***

**AÇIK OLACAK KOD:**
```
Assets/Game/Unity/StructureBlueprintAsset.cs   CreateAssetMenu tasiyan tip
Assets/Game/Unity/UnitBlueprintAsset.cs        CreateAssetMenu tasiyan tip
Assets/Game/Unity/ProductionDirector.cs        tahtayi IPlacementBoard'dan cagirir
Assets/Game/Unity/StructurePaletteView.cs      sol panel
Assets/Game/Unity/ProductionPanelView.cs       sag panel
Assets/Game/Unity/PaletteEntryView.cs          liste ogesi prefabinin kokU
```

**NE ÖĞRENECEKSİN:** Serileştirilen bir alanın neden bir **sözleşme** olduğunu,
ve o sözleşmenin ikinci tarafının kodda değil editörde durduğunu. Belge her alan
için ***boş bırakılırsa ne olur*** sorusunu koddan ölçerek cevaplıyor ve üç
kademeye ayırıyor:

```
SESLI          konsola bir sey basar         operator anlar
SESSIZ-OLU     hicbir sey olmaz              "bozuk mu?" dedirtir
SESSIZ-YANLIS  calisir ama YANLIS            en pahalisi
```

En pahalı alan `StructureBlueprintAsset.attackRange`: başlatıcısı `0` ve `0`
*"saldırmaz"* demek. `damage: 15` yazıp menzili unutmak, yapının **hiç
saldırmaması** demektir — tek satır uyarı çıkmadan.

**BİTİŞ KOŞTURMASI:**
```
python Tools/check-asset-inventory.py
```
Yedi ihlal sıfıra inmeli. İnmiyorsa kapı hangi alanın hangi dosyada bağsız
kaldığını satır satır söylüyor.



---

### ADIM 9 · [`konular/08-motor-cagri-dongusu.md`](../deep/konular/08-motor-cagri-dongusu.md) — tamamı (1181)

**NEDEN BU SIRADA:** `07` `Update`'in **içini** anlatıyor; `08` `Update`'i
**kimin çağırdığını**. `08:391-394` bu bölüşmeyi kendisi yazıyor. Ayrıca `08`
zincirin **en iyi bağlanmış** dosyası (23 çıkış bağlantısı + bir `## İlgili`
bölümü): buradan sonrasında kaybolmazsın.

**AÇIK OLACAK KOD:**
```
Assets/Game/Unity/BoardAdapter.cs:293   private void Awake()
        ◄── private, ve motor yine çağırıyor
Assets/Game/Unity/BoardAdapter.cs:349   private void OnEnable()
Assets/Game/Unity/BoardAdapter.cs:351   battle.UnitStateChanged += OnUnitStateChanged;
        ◄── C# event
Assets/Game/Unity/BoardAdapter.cs:354   private void OnDisable()
Assets/Game/Unity/BoardAdapter.cs:416   private void Update()
Assets/Game/Unity/UnitView.cs:86        private void Awake()
Assets/Game/Core/PointerGesture.cs      public void Reset()   ◄── Unity mesaj ADI, sıfır anlam
ProjectSettings/EditorSettings.asset    m_EnterPlayModeOptionsEnabled: 0
```

**BU ADIMDAN SONRA CEVAPLAYABİLECEĞİN SORU:**
> *"`Awake` neden bir `event` **değil**, ve motor `private` bir metodu nasıl
> çağırabiliyor? Bu projede gerçekten hangi geri çağrılar tanımlı — ve
> `PointerGesture.Reset` neden onlardan biri **değil**?"*

---

### ***DURMA NOKTASI 5*** — motorun sırasını KENDİN ölç

`08:344-370` bir deney tarif ediyor ve önden şunu diyor (`08:346`):
> *"***Buradaki hiçbir iddiayı bana güvenerek kabul etme.***"*

**DENEY:** Geçici bir `MonoBehaviour`, yedi geri çağrının her birine bir
`Debug.Log($"{Time.frameCount} {name} Awake")`, **iki ayrı GameObject** (A ve
B), Play, Console'u zaman sırasına al.

**SINADIĞIN İDDİA:** *"bütün `Awake`'ler bütün `Start`'lardan önce"* —
***"A önce" DEĞİL*** (`08:363-365`). Gözlem bir kez tuttu diye kural sanma.

Sonra Play'deyken B'nin bileşen kutusunu **kapat-aç**: `OnDisable` · `OnEnable`
görürsün, `Awake` ve `Start` **tekrar etmez**.

***Deneyi bitirince geçici script'i SİL. Repoya ekleme.***

---

# ***OTURUM 5 · DÜĞÜM VE DEFTER***  (~2 saat · 2.243+ satır)

### ADIM 9b · [`ogrenme/08-unity-altyapisi.md`](08-unity-altyapisi.md) — tamamı (1592)

**NEDEN BU SIRADA:** ADIM 9 *"ne oluyor"*u kapattı: çağrı sırası, sahipleri, ve
`Awake`'in bir `event` olmadığı. Bu dosya ***"neden ve teknik olarak nasıl"***
sorusunu açıyor. Açtıkları şunlar: yönetilen C# ile yerel motorun sınırı,
`Vector3` neden `struct`, PlayerLoop'un faz ağacı, serileştirmenin C#'ın
`[Serializable]`'ı **olmadığı**, `.meta`/GUID kimliği, ve ad tabanlı geri
çağrının bedeli. O bedel şudur: `void Awakee()` derlenir, uyarı çıkmaz, hiçbir
zaman çağrılmaz.

**ÖN KOŞUL:** ADIM 2 (duvar) ve ADIM 9. **YANINDA AÇIK:** `Assets/Game/Unity/BoardAdapter.cs`

***Bu dosyanın sonunda 8 adımlık bir **Editor geçiş listesi** var.*** Kod
tarafını bitirdikten sonra Unity tarafına oradan geçilir. O listede her adımda
nereye tıklanacağı, ne görüneceği ve ne zaman durup rapor edileceği yazılı.

**BU ADIMDAN SONRA CEVAPLAYABİLECEĞİN SORU:**
> *"`GameObject` bir C# nesnesi mi? `transform.position.x = 5` neden derlenmiyor?"*

---

### ADIM 10 · [`konular/01-olay-zinciri.md`](../deep/konular/01-olay-zinciri.md) — tamamı (735)

**NEDEN EN SONDA — ***ve numarası `01` olmasına rağmen***:** Üç ipliğin
düğümlendiği yer burası ve üçünü de artık kapattın:

```
   konular/02   ── ad alanı + assembly       (01:55)    ✓ ADIM 2'de kapandı
   konular/05   ── Alive / Downed / Dead     (01:65)    ✓ ADIM 5'te kapandı
   dil/04       ── event · Action · lambda   (01:176)   ── ADIM 11'de gelecek
```

***`dil/04` henüz kapanmadı ve bu bilinçli:*** `01` delegenin **ne yaptığını**
gösteriyor, `dil/04` **ne vaat ettiğini**. Zinciri önce gör, sözleşmeyi sonra
oku. Tersi, malzemeyi hiç kullanmadan öğrenmek olurdu.

**AÇIK OLACAK KOD — zincirin dört durağı, sonra ekran yarısı:**
```
Assets/Game/Core/Combat/UnitLifecycle.cs:80    public event Action<UnitState> StateChanged;
Assets/Game/Core/Combat/Combatant.cs:90        this.lifecycle.StateChanged += OnLifecycleStateChanged;
        ◄── kurucunun SON satırı
Assets/Game/Core/Combat/Combatant.cs:111       public event Action<UnitState, UnitState> StateChanged;
Assets/Game/Battle/Battle.cs:81                private readonly Dictionary<Unit, Action<UnitState, UnitState>> stateForwarders =
Assets/Game/Battle/Battle.cs:179               public event Action<Unit, UnitState, UnitState> UnitStateChanged;
Assets/Game/Unity/BoardAdapter.cs:351          battle.UnitStateChanged += OnUnitStateChanged;
Assets/Game/Unity/BoardAdapter.cs:356          battle.UnitStateChanged -= OnUnitStateChanged;
        ◄── ekran yarısı: 310 → 954 → UnitView.cs:173
Assets/Game/Unity/BoardAdapter.cs:371          private void OnUnitStateChanged(Unit unit, UnitState from, UnitState to)
Assets/Game/Unity/BoardAdapter.cs:1398          private void ApplyStateVisual(Unit unit, UnitState state)
Assets/Game/Unity/UnitView.cs:173              public void SetState(UnitState state)
```

**BU ADIMDAN SONRA CEVAPLAYABİLECEĞİN SORU:**
> *"`Battle`'daki o tuhaf sözlük neden var? Neden `-=` **sessizce** başarısız
> olabiliyor, ve sızıntının oku hangi yöne gidiyor?"*

---

### ADIM 11 · `dil/04` (607) ***→*** `dil/06` (1029) — bu sırayla, ZORUNLU

Sıra bir tercih değil; [`dil/06`](../deep/dil/06-delege-arka-taraf.md) okuyucusuna
açıkça yazıyor (`dil/06:11-15`):

> *"[`04`] bu malzemenin **sözleşmesini** anlatıyor […] **Orayı okumadan buraya
> girme**; burada hiçbiri tekrar edilmiyor."*

***Zincirdeki TEK açık ön koşul beyanı bu.*** Bölüşme:

```
   dil/04 sorar:  "+= ne VAAT EDİYOR"          ── sözleşme
   dil/06 sorar:  "+= ÇALIŞTIĞINDA ne oluyor"  ── nesne, Target/Method, çağrı listesi
```

- [`dil/04-delege-olay-ve-kapanis.md`](../deep/dil/04-delege-olay-ve-kapanis.md)
- [`dil/06-delege-arka-taraf.md`](../deep/dil/06-delege-arka-taraf.md)

***`dil/06:395-533` doğrudan senin sorduğun bir pasajı cevaplıyor.*** O pasaj,
`Combatant` kurucusunun son iki satırının neden **en sonda** durduğunu anlatıyor.

**KOŞTURULABİLİR:** `dil/06:116-143` geçici bir `DescribeSubscribers()` üyesi
tarif ediyor. ***Yaz, ölç, sil.*** `seen.Add` abone ettiğinde `Target`'ın bir
**liste** olduğunu görmek, delegenin ne olduğunu tek seferde kapatıyor.

**BU ADIMDAN SONRA CEVAPLAYABİLECEĞİN SORU:**
> *"`+=` derleyicide neye dönüşüyor, `event` hangi üç üyeyi doğuruyor, ve bir
> abone **fırlarsa** faturayı kim ödüyor?"*

---

### ADIM 12 · [`dil/07-bellek-canlilik-ve-yikim.md`](../deep/dil/07-bellek-canlilik-ve-yikim.md) (958)

**NEDEN BURAYA AİT:** `dil/07:277-341` ADIM 10'da okuduğun sözlüğün **bellek**
faturasını ölçüyor. Ölçüm şu: yedi hop, ve tek bir `Combatant` referansı bütün
savaşı erişilebilir tutuyor. ***`01` davranış faturasını, `dil/07` bellek faturasını
veriyor: aynı eksik `-=`, iki ayrı fatura.***

**ÖN KOŞUL — dosya kendi yazıyor** (`dil/07:15-18`): `dil/05` semantiği anlatır,
bu dosya **depolamayı**. `dil/05`'i henüz okumadın; `:93-128`'in dört soruluk
figürü yine de ayakta, ama `:157-255` (depolama) için `dil/05` gerekiyor.
***Sıkışırsan ADIM 13'e geç, sonra dön.***

---

### ADIM 13 · `dil/05` (768) · `dil/02` (390) · `dil/03` (809) — sıra serbest

Üçü de **referans**: baştan sona okunabilir, ama asıl işlevleri bir soru
doğduğunda açılmak.

| Dosya | Kapsadığı |
|---|---|
| [`dil/05`](../deep/dil/05-deger-referans-ve-kimlik.md) | "aynı" sözcüğünün dört ölçüsü · `ReferenceEquals` · `enum` · `out` · `=>` · `switch` · `%` |
| [`dil/02`](../deep/dil/02-koleksiyonlar-ve-salt-okunur.md) | `IReadOnlyList` ≠ değişmez · indeksleyici · `object Current` · `Dictionary` |
| [`dil/03`](../deep/dil/03-hata-bildirme-ve-dogrulama.md) | `nameof` · dört istisna tipi · `Math.Max` kelepçesi |

*****`dil/02` KISA (296 satır) VE BU BİR EKSİKLİK DEĞİL:***** bunu ölçtüm.
`dil/README.md:34`'ün saydığı beş konunun **beşi de** var, ve `dil/` ağacının
yedi adımlık kalıbı tam. Küçük olan dosya değil, konusu. Ayrıca `konular/08:570`
ve `dil/07:89` ona **dayanıyor** ve tekrar etmiyor. Yani kısalığın sebebi kapsam
değil, ***iş bölümü***.

---

### ADIM 14 · `Docs/ogrenme/` — ***`01` → `03` → `02`***

[`ogrenme/README.md`](README.md) bu sırayı kendisi öneriyor:

```
   01  "BUGÜN NEYİ BİLİYORUM"      ── önce elindekinin ADINI öğren
   03  "HANGİ KAVRAMIN SAHİBİ VAR" ── sonra haritaya bak
   02  "NE YOK, NE ZAMAN GELİR"    ── en sonda ileriye bak
```

- [`01-koda-gomulu-desenler.md`](01-koda-gomulu-desenler.md)
- [`03-kavram-borc-defteri.md`](03-kavram-borc-defteri.md)
- [`02-sonraki-asamalar.md`](02-sonraki-asamalar.md)

Sonra ***dört tamamlayıcı dosya*** geliyor. Sıraları serbest, ama `07` ile `06`
bu sırada en iyi okunur (önce dörtlünün **adı**, sonra ilkelerin adı):

- [`07-oop-dortlusu.md`](07-oop-dortlusu.md) (1079) — kapsülleme · kalıtım ·
  soyutlama · **çok biçimlilik**; polimorfizmin üç türü sayımla ayrılıyor
- [`06-ilkeler-ve-kokenleri.md`](06-ilkeler-ve-kokenleri.md) (1488) — dokuz ilke,
  kökeni, projedeki karşılığı, ve ***ilkeler çatıştığında hangisinin kazandığı***
- [`04-yok-olan-mekanizmalar-unity.md`](04-yok-olan-mekanizmalar-unity.md) (1151)
  — `Instance` · `DontDestroyOnLoad` · `Find*` · `ScriptableObject` · nesne havuzu
- [`05-yok-olan-mekanizmalar-csharp.md`](05-yok-olan-mekanizmalar-csharp.md) (852)
  — `yield` ve `await`'in derleyici çıktısı, IL'den ölçülmüş

***Bu ağaç zincirin **güvenilirlik bakımından en sağlam** parçası.*** Ölçüldü:
11 rastgele `.cs:satır` atfı elle doğrulandı, **11'i de** tam olarak adlandırdığı
yapıya düştü. Ayrıca `ogrenme/03` bir makine kapısıyla bağlı.

`ogrenme/01` sana `deep/` ağacının **hiç vermediği** şeyi veriyor: dokuz desenin
**adı**, her biri için hangi baskının onu doğurduğu, hangi SOLID harfini
taşıdığı, ve ***neyin yanlış hatırlandığı***. Son kalemin örnekleri şunlar:
*"MoveAction bir Command DEĞİL"*, *"kural sınıfları Strategy DEĞİL"*,
*"BoardAdapter bir GoF Adapter DEĞİL"*.

**BU ADIMDAN SONRA CEVAPLAYABİLECEĞİN SORU:**
> *"Bu projede hangi desenleri kullandım, hangi baskı her birini doğurdu, ve
> hangilerini **bilerek kullanmadım**?"*

***Bu, bir sonraki mülakat sorusunun birebir kendisi.***

---

### ***DURMA NOKTASI 6 — SON*** — defteri kendi kapısıyla sına

```powershell
python Tools/check-curriculum-coverage.py
python Tools/check-doc-links.py
python Tools/check-cited-names.py
```

Üçü de temiz koşmalı. `check-curriculum-coverage.py` bugünkü çıktısı:
`kavram satırı 87 · KAPALI 62 · KISMİ 13 · HENÜZ YOK 12 · ihlal 0`.

***Turu bitirdiğinde o 13 "KISMİ" satırın kaçının senin için artık "KAPALI"
olduğunu **kendin işaretle**. Defterin işi bu.***

---

# ***TAM SIRA — TEK BAKIŞTA***

```
 OTURUM 1  ─ BÜTÜNÜ KUR ─────────────────────────────────── 1.262 satır ─ ~2 sa
   1  deep/00-iskelet.md              604   kod: yok
   2  konular/02-assembly-duvari      658   kod: 4 × .asmdef + BoardAdapter.cs:48
   >> DUR <<  run-editmode-tests.ps1  ·  test asmdef'inin references dizisi

 OTURUM 2  ─ SAHİPLİK VE DURUM ─────────────────────────── 1.825 satır ─ ~2 sa
   3  konular/03-tahta-sahipligi      503   kod: Battle.cs · UnitGrid.cs · BattleActions.cs:207
   4  dil/01-degismezlik              780   kod: TurnState.cs:43-53 · UnitGrid.cs
   5  konular/05-yasam-dongusu        892   kod: UnitLifecycle · StructureLifecycle · TargetingRules
   >> DUR <<  Downed_IsTheOnlyStateBothAbilitiesAccept testini aç ve OKU

 OTURUM 3  ─ KARAR VE RET ──────────────────────────────── 1.342 satır ─ ~2 sa
   6  konular/06-sonuc-enumlari       880   kod: dört enum + ReactToMove/ReactToAttack
   7  konular/04-karar-sirasi         837   kod: BattleActions · AttackAction · MoveAction
   >> DUR <<  dört "Prefers" testi; hangisinin neden YAZILAMADIĞINI gör

 OTURUM 4  ─ MOTOR SINIRI ─────────────────────────────── 1.866+ satır ─ ~2,5 sa
   8  konular/07-tiklamadan-eyleme   1126   kod: BoardAdapter Update/HandleClick · PointerGesture
  8b  ogrenme/11-unity-penceresi      770   16 Inspector alani · sahne onarimi
   >> DUR <<  >> EDITOR · Play · DURMA NOKTASI 4 — kapanmis kusuru gor <<
  8c  ogrenme/12-unity-editor-baglama  499   yeni katmanin editor kurulumu
   >> DUR <<  check-asset-inventory.py -- yedi ihlal sifira inmeli
   9  konular/08-motor-cagri-dongusu 1181   kod: Awake/OnEnable/Update · EditorSettings.asset
   >> DUR <<  iki bileşenli günlük deneyi — 08:344-370, sonra script'i SİL

 OTURUM 5  ─ DÜĞÜM VE DEFTER ───────────────────────────── 2.243+ satır ─ ~2 sa
  10  konular/01-olay-zinciri         735   kod: UnitLifecycle:80 · Combatant:86,107 · Battle:74,172
  11  dil/04 (607)  ->  dil/06 (1029)    kod: Combatant kurucusunun son iki satırı
  12  dil/07-bellek-canlilik          958   kod: DespawnView · RemoveUnit
  13  dil/05 · dil/02 · dil/03      1.967   referans — sıra serbest
  14  ogrenme/01 -> 03 -> 02        1.885   desen adları · kapsama tablosu · aşamalar
   >> DUR <<  üç kapıyı koştur; KISMİ satırları kendin güncelle
```

---

### ADIM 15 · [`ogrenme/09-ecs-dots-yol-haritasi.md`](09-ecs-dots-yol-haritasi.md) (1415)

***Bu dosya ADIM 14'ten SONRA okunur.*** `01`/`02`/`03` okunmadan açılmaz,
çünkü bu dosya üçüne de geri bağlanıyor.

**NEDEN EN SONDA:** ECS, Job System ve Burst ***üç ayrı şey*** ve üçü de bu
projede **yok**. Bu dosya onları mekanizma olarak anlatır, eşiği sayıyla ölçer
(3×5 tahta, 2 birim — ECS'in kazandığı eşik binlerce varlık), ve projeyi
genişletmek için **yedi basamaklı bir merdiven** verir. Beş basamak mevcut koda
hiç dokunmadan yapılabilir.

**YANINDA AÇIK:** `Assets/Game/Battle/Battle.cs` · `Assets/Game/Core/Unit.cs`

***Ölçülmüş sınır şu: Entities 1.0 Unity 2022.3+ istiyor, bu depo ise
2021.3.45f2. Yani ECS örnek kodu bu projede **derlenmez**. Dosya bu yüzden
örnek kod yazmıyor ve hiçbir hızlanma oranı vermiyor.***

**BU ADIMDAN SONRA CEVAPLAYABİLECEĞİN SORU:**
> *"ECS ne zaman kazanır, ve bu oyun neden o eşiğe hiç ulaşmıyor?"*

---

# ***SEÇİLEN / REDDEDİLEN***

## SEÇİLEN
`00 → 02 → 03 → dil/01 → 05 → 06 → 04 → 07 → 08 → 01 → dil/04 → dil/06 → dil/07
→ dil/05,02,03 → ogrenme/01,03,02 → ogrenme/07,06,04,05 → ogrenme/09`
(ve `08` ADIM 9b olarak `konular/08`in hemen ardında; `11` ise ADIM 8b olarak
`konular/07`nin hemen ardında, DURMA NOKTASI 4'ün ***önünde***)

***İki **bağımsız** yöntem aynı başlangıcı verdi:***
① **Kenar sayımı** — `02`'nin 6 gelen kenarı var, `00-iskelet`'in 0.
② **Anlatının kendi beyanı** — `deep/00-iskelet.md:523-527`:
*"bu dosya baştan sona → `02` → `03` → `07`"*.
Bu belge o dört adımlık yolu **on dört adıma** genişletiyor ve aradaki boşlukları
bağımlılık yönüne göre dolduruyor.

## REDDEDİLEN 1 · Numara sırası (`01 → 02 → … → 08`)
Sekiz `konular/` dosyasından yalnız **ikisi** numara sırasında doğru yerde. `01`
ilk okunursa üç tanımsız kavram taşıyor ve dosya kendi ifadesiyle *"bütün hikâye
bu tek karardan doğuyor"* (`01:59`) diyerek okuyucuyu ***tanımsız bir gerekçenin
üstüne*** oturtuyor.

## REDDEDİLEN 2 · Dosyaları yeniden numaralandırmak
Numaralar 30+ yerde ve dört makine kapısında **çapa**. Yeniden numaralandırma
`check-doc-links.py` ile `check-curriculum-coverage.py`'yi anında kırar.
***Numara DOSYA KİMLİĞİ; sıra AYRI BİR BELGEDE.***

## REDDEDİLEN 3 · `dil/` ağacını `konular/`'dan önce okumak
Beş `dil/` dosyası `konular/`'a geri bağlanıyor ve hepsi *"bunun proje tarafı
şurada"* diyor. Ok yönü net: ***`dil/` `konular/`'ı açıklıyor, tersi değil.***
Tek istisna `dil/01`. Onun `03`'ün hemen ardına konmasının sebebi tam olarak bu
(`dil/01:176` `03`'e işaret ediyor).

## REDDEDİLEN 4 · Yerleştirme hatasını okumadan ÖNCE düzeltmek
Bugünkü soru *"okuduğumda anlayabilecek miyim"*, kod tamir etmek değil. Ayrıca
***hatayı Play'de görmek, *"İkisi çelişirse kod kazanır"* kuralının
koşturulabilir tek örneğidir.*** Aynı on dakika turun en öğretici on dakikası.
Düzeltme ayrı bir tura ait: not al, geç.

***ADIM 8b bu reddi ÇİĞNEMİYOR ve karışması pahalı olurdu.*** Orada onarılan şey
**sahne**dir (atanmamış bir Inspector alanı), **kod** değil. Onarım
`ArgumentException`'ı ortadan kaldırmaz; onu ulaşılabilir kılar. Onarımsız
görülen kırmızı satır başka bir satırdır ve turun dersi o değildir.

## REDDEDİLEN 5 · `deep/kod/` ağacını (14.788 satır) sıraya dahil etmek
33 ayna belge, tip başına. Sıraya girseydi 9-11 saatlik bütçeyi **üçe**
katlardı. ***Doğru kullanımı referans:*** bir tipe **dokunmadan önce** onun
aynasını aç. Ayna belgelerin dizini
[`deep/kod/README.md`](../deep/kod/README.md).

---

## İlgili

- Üç ağacın yönlendirmesi: [`../deep/README.md`](../deep/README.md)
- Bu ağacın yönlendirmesi: [`README.md`](README.md)
- Tip başına ayna belgeler: [`../deep/kod/README.md`](../deep/kod/README.md)
- Üst düzey belge haritası: [`../README.md`](../README.md)
