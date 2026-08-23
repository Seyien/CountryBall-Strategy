# Bellek, canlılık ve yıkım — bir nesne ne zaman gerçekten biter

> **Nerede geçiyor:** `Battle.AddUnit`/`RemoveUnit`, `Battle.stateForwarders`,
> `Combatant` kurucusu, `BoardAdapter.OnEnable`/`OnDisable`/`DespawnView`,
> `UnitView.authoredColor`, `TurnState.DefaultTurnOrder`,
> `DamageRulesAllocationTests`
> **Kodda nereden geldin:** `Destroy`, `DestroyImmediate`, `GC.KeepAlive`,
> `Not.AllocatingGCMemory`, `-=`, `static readonly`
> **Ne zaman oku:** bir aboneliği sökmeyi düşünürken; `Destroy` çağırdığın hâlde
> nesnenin hâlâ orada durduğunu gördüğünde; ya da birine "bu değer stack'te
> durur" demeden hemen önce.

Bu dosya projenin kendi kararlarını değil, projenin **ödünç aldığı** çalışma
zamanı davranışını anlatıyor: C#'ın çöp toplayıcısını ve Unity'nin ikinci ömrünü.
[`dil/05`](05-deger-referans-ve-kimlik.md) "kopyalanır mı, paylaşılır mı" sorusunu
cevaplıyor — **semantik**. Burada başka bir soru var: o değer fiziksel olarak
nerede duruyor, kim onu bir daha okuyabilir, ve onu kim ortadan kaldırıyor.
İkisi karıştırıldığı an bu dosyadaki her cümle yanlış okunur.

---

## Sahne

Düşmüş bir askerin ceset süresi doluyor. `Battle` onu tahtadan çıkarıyor,
`BoardAdapter` görselini sahneden siliyor. Ekranda asker yok.

Şimdi tek soru: **asker gerçekten gitti mi?** Cevap dörde bölünüyor. Adı `unit`
olan yerel değişken gitti mi, o `Unit` nesnesi bellekten silindi mi, ekrandaki
`GameObject` yok edildi mi, ve o `GameObject`'e ait yönetilen C# nesnesi toplandı
mı — **dört ayrı olgu** ve hiçbiri ötekini gerektirmiyor.

---

## Karakterler

```
╔═ DERLEYİCİ (Roslyn) ══════════════════════════════════════════╗
║  İşi     : bir adın hangi metin bölgesinde kullanılabileceğine ║
║            karar vermek — KAPSAM                               ║
║  Bilir   : bloklar, süslü parantezler, adların görünürlüğü     ║
║  BİLMEZ  : çalışırken hangi nesnenin yaşadığını. Bir tek nesne ║
║            bile göremez. ██ `}` ONUN İŞARETİ, GC'nin değil ██  ║
╚════════════════════════════════════════════════════════════════╝

╔═ JIT / ENİYİLEŞTİRİCİ ════════════════════════════════════════╗
║  İşi     : IL'i makine koduna çevirirken gereksiz işi atmak    ║
║  Bilir   : bir değerin SON okumasının nerede olduğu — CANLILIK ║
║  BİLMEZ  : senin niyetini. Kapsam içinde ama bir daha          ║
║            okunmayacak bir referansı ÖLÜ sayabilir             ║
╚════════════════════════════════════════════════════════════════╝

╔═ ÇÖP TOPLAYICI (GC) ══════════════════════════════════════════╗
║  İşi     : hiçbir kökten ULAŞILAMAYAN yönetilen nesnelerin     ║
║            belleğini geri almak — ERİŞİLEBİLİRLİK              ║
║  Vaadi   : erişilemeyen bir nesne er ya da geç toplanır        ║
║  BİLMEZ  : NE ZAMAN toplayacağını sana söylemez; dosya, soket, ║
║            yerel motor nesnesi diye bir kavramı YOKTUR         ║
╚════════════════════════════════════════════════════════════════╝

╔═ UNITY'NİN YEREL TARAFI ══════════════════════════════════════╗
║  İşi     : sahnede gerçekten çizilen, çarpışan, güncellenen    ║
║            nesneyi tutmak                                      ║
║  Bilir   : `Destroy` çağrıldı mı, sahne yüklü mü               ║
║  BİLMEZ  : C# tarafında o nesneye kaç değişkenin ok tuttuğunu  ║
║            ██ İKİ TARAF BİRBİRİNİN ÖMRÜNÜ BELİRLEMEZ ██        ║
╚════════════════════════════════════════════════════════════════╝

╔═ SEN (elle serbest bırakan) ══════════════════════════════════╗
║  İşi     : GC'nin bilmediği her şeyi kapatmak — KAYNAK ÖMRÜ    ║
║  Bu projede bugün: `-=` ile abonelik sökmek, `Destroy` çağırmak║
║  BİLMEZ  : unuttuğunu. Eksik bir `-=` tek bir uyarı üretmez    ║
╚════════════════════════════════════════════════════════════════╝
```

### Ödünç alınan beş ad, beş sahip satırı

| Ad | Sahibi | Ne VAAT EDER | Ne VAAT ETMEZ |
|---|---|---|---|
| `System.Object` | C# dilinin kökü, `mscorlib` | her tipin atası olmak; `ReferenceEquals`, `GetHashCode` | sahneyle, `Destroy` ile, motorla hiçbir ilgisi yok |
| `UnityEngine.Object` | Unity motoru | yerel bir eşi olan nesneler için ortak kimlik/ömür API'si (`Destroy`, `name`, ezilmiş `==`) | C# nesnesini silmeyi. `Destroy` yönetilen belleği geri vermez |
| `GC` | .NET çalışma zamanı | erişilemeyen **yönetilen** belleği er ya da geç geri almak | zamanlaması. Dosya/soket/yerel motor nesnesi kapatmak |
| `GC.KeepAlive(x)` | .NET çalışma zamanı | `x`'in o satıra kadar canlı sayılmasını — eniyileştirici onu erken ölü ilan edemez | belleği tutmayı, `Destroy`'u ertelemeyi. Yalnız **canlılık** sınırını iter |
| `WeakReference` | .NET çalışma zamanı | tutmadan işaret etmeyi | hiçbir şeyin yaşayacağını. **HENÜZ YOK** — `grep -rn "WeakReference" Assets` sıfır satır; ilk gereken gün bir önbellek "bellek sıkışırsa bırakılabilir" demek zorunda kaldığı gündür |

> **`System.Object` ≠ `UnityEngine.Object`.** `UnityEngine.Object`
> `System.Object`'ten TÜRER, ama tersi bir şey ifade etmez — `new Unit("Piyade")`
> bir `System.Object`'tir ve `Destroy` edilemez. Motor olanını varsayan okuyucu
> bütün çerçeve modelini yanlış kurar. Aynı karışıklığın `foreach` tarafındaki
> yüzü: [`dil/02`](02-koleksiyonlar-ve-salt-okunur.md#hangi-object-en-pahali-karisiklik).

---

## Birinci durak: dört ayrı soru, dört ayrı cevap

Bu dosyanın çekirdeği bu figür. Dördü aynı anda doğru olabilir; biri ötekini
gerektirmez.

```
KAPSAM (lexical scope)
   soru   : derleyici bu ADI hangi metin bölgesinde görüyor
   sahibi : DERLEYİCİ                             ██ DERLEME ZAMANI ██
   biter  : kapanış parantezinde
   gözlem : derleme hatası (CS0103: "adı geçerli bağlamda yok")

        ██ AYRIŞMA ██  kapsam biter, nesne yaşayabilir

CANLILIK (liveness)
   soru   : bu değişken bir daha OKUNACAK mı
   sahibi : JIT / ENİYİLEŞTİRİCİ                  ██ ÇEVİRİ ANI ██
   biter  : son okumadan sonra — kapanış parantezinden ÖNCE olabilir
   gözlem : doğrudan gözlenemez; ancak GC.KeepAlive ile SINIRI itilir

        ██ AYRIŞMA ██  değişken ölü, nesne başka bir kökten erişilebilir

ERİŞİLEBİLİRLİK (reachability)
   soru   : bir KÖKTEN bu nesneye zincirle gidilebiliyor mu
   sahibi : ÇÖP TOPLAYICI                         ██ ÇALIŞMA ZAMANI ██
   biter  : son zincir koptuğunda — toplama İŞTE O AN olmak zorunda değil
   gözlem : bellek profilcisi anlık görüntüsü (bu projede HENÜZ YOK)

        ██ AYRIŞMA ██  yönetilen bellek geri alınır, kaynak açık kalır

KAYNAK ÖMRÜ (resource lifetime)
   soru   : dosya / soket / yerel motor nesnesi / yerel bellek hâlâ duruyor mu
   sahibi : SEN                                   ██ ELLE ██
   biter  : sen kapatınca. GC bu soruyu SORMAZ BİLE
   gözlem : bu projede iki yolla: `-=` satırı ve `Destroy` çağrısı
```

**Kapanış parantezi `}` bir çöp toplama noktası DEĞİLDİR.** Yalnızca birinci
satırı bitirir; ikinciyi bitirmesi bile şart değil — eniyileştirici bir yereli
kapsamın ortasında ölü ilan edebilir.

### Bu projeden dört cevap, tek metotta

`BoardAdapter.DespawnView` (BoardAdapter.cs:983-1010) dördünü tek ekranda gösteriyor:

```csharp
private void DespawnView(Unit unit)
{
    if (ReferenceEquals(unit, selectedUnit)) { selectedUnit = null; }
    if (!TryGetView(unit, out UnitView view)) { return; }

    unitViews.Remove(unit);        // ← ERİŞİLEBİLİRLİK: son sözlük bağı koptu
    Destroy(view.gameObject);      // ← KAYNAK ÖMRÜ: yerel nesne yıkım sırasına girdi
}                                  // ← KAPSAM: `view` adı burada biter
```

Dördüncü satır **CANLILIK**: `view`'ın son okuması `Destroy(view.gameObject)`
satırıdır, sonrası ölüdür. Ve dikkat: `unitViews.Remove` yalnız ÇAĞIRANIN
tablosundaki bağı koparır — `Battle` tarafındaki bağları bir satır önce
`Battle.RemoveUnit` koparmıştı. Son satırdaki `Destroy` ise yalnız YEREL tarafı
bitirir; yönetilen `UnitView` nesnesi orada silinmez.

---

## İkinci durak: depolama — nerede duruyor, ve soruyu neden nadiren sorarsın

### Üç bölge (öğretme haritası, kesin bir çalışma zamanı vaadi değil)

```
① YÜRÜTME DEPOSU VE KÖKLER  yerel değişkenler, parametreler, yığın (stack)
                            çerçeveleri, işlemci kayıtları, çalışma zamanı
                            tutamakları
② YÖNETİLEN YIĞIN           sınıf örnekleri, diziler, dizeler, List<T>'nin arka
   (managed heap)           dizisi, KAPANIŞ nesneleri, yönetilen
                            UnityEngine.Object temsilleri
③ YÖNETİLMEYEN / MOTOR      Unity'nin yerel nesneleri, doku/mesh/render hedefi
   BELLEĞİ                  ██ BU PROJEDE BUGÜN YALNIZ MOTOR TARAFI VAR ██
                            yerel konteyner (NativeArray<T>) HENÜZ YOK
```

### ██ Aynı `int` üç ayrı yerde — K21'in işlenmiş örneği ██

Bir tipin **değer tipi olması** onun nerede durduğunu SÖYLEMEZ. Aynı `int`,
onu SARAN şeye göre üç ayrı yerde yaşar. Üçü de bu projeden, üçü de gerçek:

```
① YEREL DEĞİŞKEN — yürütme deposunda
   DamageRulesAllocationTests.cs:129
       for (int i = 0; i < Iterations; i++)
                ▲
       döngü sayacı; hiçbir nesnenin içinde değil, hiçbir kapanış
       onu yakalamıyor. Yönetilen yığında bir karşılığı YOK.

② SINIF ALANI — YÖNETİLEN YIĞINDA, sarmalayan nesnenin İÇİNDE
   UnitView.cs:81
       private Color authoredColor = Color.white;
                     ▲
       `Color` bir struct, yani DEĞER TİPİ. Ama bu değer bir
       UnitView nesnesinin alanı ve o nesne yönetilen yığında duruyor.
       ██ DEĞER TİPİ, HEAP'TE ██  — çelişki değil, kuralın kendisi:
       depolama yeri tipin değil, SARMALAYANIN sorusudur.

③ KAPANIŞ TARAFINDAN YAKALANMIŞ — YÖNETİLEN YIĞINDA, derleyicinin
   ürettiği ayrı bir nesnenin içinde
   DamageRulesAllocationTests.cs:40-57 — bu projenin kendi kararı:
       private int sink;      ◄── ALAN seçildi, yerel DEĞİL
   Gerekçesi dosyada yazılı: kısıt bir lambda alıyor ve lambda içinden
   bir YERELE dokunmak derleyiciye bir kapanış SINIFI ürettirir; o
   sınıfın örneği yönetilen yığında doğar ve ölçüm penceresinin
   İÇİNDE bir tahsis olarak görünür.
       ██ Aynı `int`, yalnızca YAKALANDIĞI için yığına taşındı ██
```

Üç satırda da tip aynı. Değişen tek şey **bağlam**.

### Bu projede kullanıcı tanımlı `struct` YOK — ölçü

`Assets/Game` altında `struct` anahtar kelimesiyle bildirilen hiçbir tip yok.
Kelime dört yerde geçiyor ve dördü de ya **reddedilen alternatif** ya bir BCL
tipi hakkında not: `AttackProfile.cs:31` (*"`readonly struct` olsaydı her alan
okumasında ve her parametre geçişinde yeni bir kopya doğar"*), `MoveProfile.cs:15`
(*"SINIF, struct DEĞİL"*), `AttackOutcome.cs:22`, ve `Battle.cs:380`
(*"`Dictionary<,>.Enumerator` bir struct'tır"* — BCL'in tipi, projenin değil).

Bu yüzden yukarıdaki ② örneği `Color` üstünden verildi: `Color` Unity'nin
`struct`'ı ve bu projede gerçekten bir sınıf alanı olarak yaşıyor. Aynı şeklin
üç örneği daha: `UnitView.downedTint`/`deadTint` (UnitView.cs:59, 66) ve
`Combatant.lastObservedState` (Combatant.cs:52) — sonuncusu bir `enum`, yani
arkasında bir `int` var ve o `int` `Combatant` nesnesinin **içinde**, yönetilen
yığında duruyor.

### Statik depo — ve bu projedeki tek örneği

```csharp
// TurnState.cs:44
public static readonly IReadOnlyList<Team> DefaultTurnOrder =
    Array.AsReadOnly(new[] { Team.Player, Team.Enemy });
```

**Ölçü:** hiçbir `Battle` kurmadan `TurnState.DefaultTurnOrder` okunabilir;
`Turn.Current` okunamaz. Statik alan tipin ömrüne bağlıdır, bir örneğin ömrüne
değil — ve bu, üçüncü durağın konusu: **statik bir referans bir KÖKtür.**

Karşı örnek aynı dosyada: `TurnState.FirstTurnNumber` bir `const`
(TurnState.cs:51) ve `const`'un çalışma zamanında **depolaması yoktur**; değer
her çağrı yerine derleme anında kopyalanır (`Combatant.ReviveHealthDivisor` de
öyle, Combatant.cs:42). Bu kopyalamanın assembly sınırındaki bedeli
[`dil/01`](01-degismezlik-anahtar-kelimeleri.md)'in konusu.

### Ve şimdi asıl cümle: bu soruyu neden NADİREN sorarsın

```
  KARARI DEĞİŞTİREN sorular  ► bu nesneye kim ok tutuyor   (erişilebilirlik)
                             ► bu abonelik sökülüyor mu    (kaynak ömrü)
                             ► bu kod kare başına tahsis yapıyor mu (ÖLÇÜLÜR)

  DEĞİŞTİRMEYEN soru         ► bu int stack'te mi heap'te mi
      ██ ölçüsü şu: cevabı değiştir — kodun tek satırı bile değişmez ██
```

Depolama sorusu ancak **ölçülmüş bir tahsis** ile geri geldiğinde önem kazanır —
beşinci durağın konusu. Ölçüm yoksa soru da yoktur.

---

## Üçüncü durak: kök kümesi ve erişilebilirlik

GC "bu nesne kullanılıyor mu" diye sormaz. Tek bir soru sorar: **bir kökten bu
nesneye zincirle gidilebiliyor mu.**

```
KÖKLER (GC roots)
├── canlı yerel/parametre referansları ve kayıtlar
├── STATİK referanslar          ◄── bu projede: TurnState.DefaultTurnOrder
├── çalışma zamanı tutamakları  ◄── Unity'nin sahnedeki nesneleri buradan tutması
└── ve bu köklerden ULAŞILAN her nesne
        └── alanlar / dizi elemanları / ██ DELEGE HEDEFLERİ ██ → daha fazlası
```

**Bir alan kendiliğinden kök DEĞİLDİR.** Yalnızca onu barındıran nesne bir kökten
erişilebilir olduğu sürece tutar. Ölçü: `Battle.combatants` sözlüğü yüzlerce
`Combatant` tutabilir; ama `Battle`'ın kendisine hiçbir kökten gidilemiyorsa, o
sözlük de içindekiler de topluca erişilemez olur.

### ██ Bu projedeki canlı örnek: `Battle.stateForwarders` ve okun yönü ██

`Battle.AddUnit` her savaşçı için bir kapanış üretip abone ediyor (Battle.cs:226-229):

```csharp
Action<UnitState, UnitState> forwarder =
    (previous, next) => UnitStateChanged?.Invoke(unit, previous, next);
combatant.StateChanged += forwarder;
stateForwarders.Add(unit, forwarder);
```

O lambda İKİ şeyi yakalıyor: yerel `unit` parametresi, ve `UnitStateChanged` bir
**örnek** olayı olduğu için `this` — yani `Battle`'ın kendisi. **Sızıntının yönü
sezginin tersidir, ve ölçüsü zincirin kendisidir.** Diyelim `RemoveUnit` aboneliği
sökmedi ve bir yerde (bir test fixture'ı, bir gün gelecek birim havuzu) o
`Combatant`'a canlı bir referans kaldı. Zinciri say:

```
   ██ OK YÖNÜ: YAYINCI → ABONE ██  Combatant, Battle'ı TUTUYOR — tersi değil

canlı yerel ──► Combatant (YAYINCI)
                   └── StateChanged davet listesi
                         └── [0] forwarder (delege nesnesi)
                               ├─ Method ──► derleyicinin ürettiği metot
                               └─ Target ──► kapanış nesnesi
                                               ├──► unit ──► Unit
                                               └──► this ──► Battle
                                                              ├──► UnitGrid (board)
                                                              ├──► combatants sözlüğü
                                                              │      └──► DİĞER BÜTÜN Combatant'lar
                                                              ├──► structures sözlüğü
                                                              └──► TurnState
```

Tek bir `Combatant` referansı, **bütün savaşı** erişilebilir tutar. "Sızıntı
olur" bir etiket; ölçüsü işte bu zincir — yedi hop ve savaşın tamamı.

### Kod bunu GERÇEKTE ne yapıyor: sökme yeri var

`Battle.RemoveUnit` (Battle.cs:336-354) aboneliği söküyor:

```csharp
if (wasCombatant)
{
    if (stateForwarders.TryGetValue(unit, out Action<UnitState, UnitState> forwarder))
    {
        combatants[unit].StateChanged -= forwarder;   // ← zincir BURADA kopuyor
        stateForwarders.Remove(unit);
    }
    combatants.Remove(unit);
}
```

`RemoveReadyForCleanup` her adayı bu metottan geçiriyor, yani temizlik yolu
sökmeyi **atlayamıyor**:

```
Battle.cs:467-470   for (int i = 0; i < removed.Count; i++)
```

Kodun kendi cümlesi iki faturayı yan yana koyuyor:

```
Battle.cs:338-342
                // ABONELİK BURADA BIRAKILIYOR. Yalnız sözlükten silmek, çağıranın
                // elinde kalan Combatant üzerinden savaşta OLMAYAN bir birim için
                // kimlikli olay yayılmasına izin verirdi; delege bu savaşı
                // tuttuğu için o birim çöp de olamazdı.
```

> **Bu aboneliğin dört durağı ve sökülmezse ÖNCE neyin patladığı (yanlış
> görsel araması, `LogError`):**
> [`konular/01`](../konular/01-olay-zinciri.md#sokulmezse-ne-olur-ok-yonune-dikkat).
> Orada anlatılan **davranış** hatası; burada anlatılan **bellek** hatası. Aynı
> eksik `-=`, iki ayrı fatura — ve davranış faturası her zaman önce gelir.

> **Delege nesnesinin `Target`/`Method` ikilisinin içi, `-=`'in kimliği neye
> göre karşılaştırdığı:**
> [`dil/06`](06-delege-arka-taraf.md#birinci-durak-delegenin-ici-target-method).
> Sözleşme tarafı için
> [`dil/04`](04-delege-olay-ve-kapanis.md#dorduncu-durak-kapanis-kimligi---neye-bakiyor).
> Burada yalnız **tutma** var, mekanizma orada.

### Sökülmediği hâlde sızıntı OLMAYAN abonelik — karşı örnek

`Combatant` kurucusu abone oluyor ve **hiçbir yerde sökmüyor**
(Combatant.cs:90):

```csharp
this.lifecycle.StateChanged += OnLifecycleStateChanged;
```

Bu ihmal değil. Ölçü: `lifecycle` bu tipin **kendi alanı** (Combatant.cs:45).
Zincir bir DÖNGÜ:

```
   Combatant ──alan──► UnitLifecycle ──davet listesi──► delege ──Target──► Combatant
       ▲                                                                       │
       └───────────────────────────────────────────────────────────────────────┘
                  ██ KAPALI HALKA — dışarıya hiçbir ok çıkmıyor ██
```

Kimse `Combatant`'a erişemez olduğunda halkanın tamamı erişilemez olur ve
topluca toplanır. Bunu mümkün kılan şey GC'nin **izleyen** (tracing) bir toplayıcı
olması: köklerden yürür, sayaç tutmaz. Referans sayan bir toplayıcı olsaydı bu
halka asla sıfıra düşmez ve gerçek bir sızıntı olurdu. Kodun kendi cümlesi
(Combatant.cs:82-83): *"Aboneliğin çözüldüğü yer yok ve bu ihmal değil; bu tip
lifecycle'ının SAHİBİ, abonelik sınır geçmiyor."*

**Ayıran ölçüt tek: abonelik bir SAHİPLİK SINIRINI geçiyor mu.**

```
lifecycle → Combatant   sınır GEÇMEZ (parça ile sahibi)   → sökme gerekmez
Combatant → Battle      sınır GEÇER  (iki ayrı ömür)      → ██ SÖKMEK ŞART ██
battle → BoardAdapter   sınır GEÇER  (motor ile çekirdek) → ██ SÖKMEK ŞART ██
```

Üçüncü satır `BoardAdapter.OnEnable`/`OnDisable` (BoardAdapter.cs:288-301) ve kodun
oradaki uyarısı bu dosyanın da uyarısı: *"Simetriyi derleyici değil disiplin
tutuyor: eksik bir `-=` tek bir uyarı bile üretmez."*

---

## Dördüncü durak: Unity'nin iki ömrü

`UnityEngine.Object`'ten türeyen her nesnenin **iki tarafı** vardır: yönetilen
C# temsili ve onunla ilişkili yerel motor durumu. İkisinin ömrü ayrı yönetilir.

```
BoardAdapter.cs:1007    Destroy(view.gameObject);
       ┌─────────────────────┴──────────────────────┐
       ▼                                            ▼
YEREL TARAF                              YÖNETİLEN TARAF
─────────────                            ────────────────
yıkım SIRAYA KOYULUR                     hiçbir şey olmaz;
(güncelleme döngüsünden sonra,           C# nesnesi yerinde durur
 çizimden önce — Destroy'un                       │
 belgelenmiş sözleşmesi)                          ▼
       │                                 sarmalayıcı "yıkılmış" işaretlenir
       ▼                                          │
yerel nesne yıkılır                               ▼
       │                            hiçbir kök ona ulaşamayınca GC'ye
       │                            UYGUN olur; SONRAKİ bir toplama alır
       └───────────────────────► ██ İKİSİ AYNI AN DEĞİL ██
```

### ██ Bu yüzden yıkılmış bir nesne `== null` DER ama null DEĞİLDİR ██

`UnityEngine.Object` `operator ==`'i bilerek aşırı yükler:

```
Destroy(view.gameObject) çağrıldıktan ve yıkım gerçekleştikten SONRA:

   body == null                 ►  true    ◄── Unity öyle diyor
   ReferenceEquals(body, null)  ►  false   ◄── C# öyle diyor
                                             ██ İKİSİ DE DOĞRU ██
```

Bu iki cevabın neden çeliştiği, hangisinin ne zaman doğru araç olduğu ve
`UnitView.Body`'deki `if (body == null)` satırının neden `ReferenceEquals`'a
çevrilemeyeceği:
[`dil/05`](05-deger-referans-ve-kimlik.md#bolusme-nerede-dogru-secim-ve-neden).
Burada tekrar edilmiyor; buradaki katkı tek cümle: **fark bir kaprisin değil,
iki ayrı ömrün gözlemlenebilir izidir.** `==` "yerel taraf yaşıyor mu" diye
sorar, `ReferenceEquals` "yönetilen kutu aynı kutu mu" diye.

`BoardAdapter.DespawnView`'daki sıra (BoardAdapter.cs:1002-1007) tam olarak bunun
sonucu: önce `unitViews.Remove(unit)`, sonra `Destroy`. Tersi olsaydı tabloda
"null gibi ama null değil" bir referans kalırdı — sözlükte duran, `== null`
diyen, `ReferenceEquals` ile bulunabilen bir hayalet.

### Bu projede `Destroy` gerçekten çağrılıyor mu — SAYARAK

```
ÜRETİM KODU (Assets/Game/)
   Destroy(...)            ► 1 çağrı    BoardAdapter.cs:1007
   DestroyImmediate(...)   ► 0 çağrı
   OnDestroy()             ► 0 tanım    (yalnız BoardAdapter.cs:279'de
                                         REDDEDİLEN alternatif olarak anılıyor)
   Instantiate(...)        ► 1 çağrı    BoardAdapter.cs:739
   new GameObject(...)     ► 2 çağrı    BoardAdapter.cs:568, 656
   AddComponent<...>()     ► 2 çağrı    BoardAdapter.cs:572, 671

TEST KODU (Assets/Tests/)
   DestroyImmediate(...)   ► 2 çağrı    UnitViewTests.cs:76
                                        GridCellGapCharacterizationTests.cs:40
```

**Yaratan ile serbest bırakan simetrisi tek yerde tam:** birim görselleri —
`Instantiate` (728) ↔ `Destroy` (992), aradaki tabloyu `unitViews` tutuyor.
**Simetrinin eksik olduğu yer:** `CreateStructureVisual` (BoardAdapter.cs:566)
bir `GameObject` doğuruyor ama hiçbir tabloya yazmıyor — kodun kendi notu bunu
bilerek söylüyor: *"GÖRSEL BİR TABLOYA KAYDEDİLMİYOR: bugün onu tekrar bulması
gereken hiçbir çağıran yok."* Oysa `RemoveReadyForCleanup` enkaz süresi dolan
yapıları da aynı tampona yazıyor ve `AdvanceBattleTime` her
biri için `DespawnView` çağırıyor (BoardAdapter.cs:634-637); o çağrı `unitViews`'te
bir şey bulamaz ve `TryGetView` `LogError` basıp döner (BoardAdapter.cs:1072).

```
Battle.cs:456-462   foreach (KeyValuePair<Unit, Structure> pair in structures)
```

**Yapı görselinin serbest bırakan tarafı yazılmamış.** Dürüst sınır: bu yol bugün
baştan sona koşmuyor (yerleştirme çalışma anında bir istisnayla kesiliyor; bir kod
kusuru olarak ayrıca raporlandı), yani sahnede biriken bir yapı görseli bugün YOK
— ama yazılı hâliyle yaratan var, serbest bırakan yok.

### `DestroyImmediate` neden testlerde, ve neden yalnız orada

`UnitViewTests.cs:74-76`'daki gerekçe: *"DestroyImmediate, Destroy değil: Destroy
karenin sonunu bekler ve EditMode'da o kare hiç gelmez, sahnede sızıntı kalırdı."*
Bu, birinci duraktaki dördüncü satırın en saf hâli: `[TearDown]` bloğunun kapanış
parantezi hiçbir şeyi yok etmez; yok eden şey o satırdaki çağrıdır.
`DestroyImmediate` üretim kodunun normal aracı **değildir** — editör dışında
varlık dosyalarını bozabilir; bu projede üretim tarafında hiç geçmiyor.

### HENÜZ YOK — dört temizlik mekanizmasından ikisi

Dört ayrı mekanizma var; bu proje bugün **ikisine** dokunuyor:

| Mekanizma | Bu projede | Hangi aşama getirir |
|---|---|---|
| Yönetilen GC | **VAR** — üç olay aboneliği, iki sözlük, kapanış nesneleri | — |
| Unity `Destroy` | **VAR** — tek çağrı, BoardAdapter.cs:1007 | — |
| Açık yerel konteyner elden çıkarma (`NativeArray<T>` + `Dispose`) | **HENÜZ YOK** (ölçü: `IDisposable`, `using (`, `NativeArray` → Assets altında sıfır satır) | Jobs/Burst ile bir yol hesabı ya da toplu görünürlük hesabı geldiği gün |
| Varlık (asset) boşaltma (`Resources.UnloadUnusedAssets`, Addressables) | **HENÜZ YOK** (ölçü: `Resources.` ve `SceneManager` → sıfır satır; sprite'lar Inspector referansı) | İkinci bir sahne ya da çalışma anında yüklenen ilk varlık geldiği gün |

---

## Beşinci durak: bu projenin tahsis gerçeği — ÖLÇÜLMÜŞ

### Ne ölçüldü

`Assets/Tests/EditMode/Combat/DamageRulesAllocationTests.cs` — projedeki **tek**
tahsis testi dosyası, üç test:

```
① Olcum_Aygiti_Tahsisi_Gorebiliyor   ██ NEGATİF KONTROL ██
     `new int[64]` + GC.KeepAlive  ►  UnityIs.AllocatingGCMemory()
     Aracın kör OLMADIĞINI kanıtlar. Bu kırmızıysa alttaki ikisinin
     yeşili hiçbir şey ifade etmez.
② ResolveRemaining_Hic_Tahsis_Yapmaz
     1000 × DamageRules.ResolveRemaining(100, 10) ► Not.AllocatingGCMemory()
     + sink > 0 kontrolü: döngü elenmemiş olmalı
③ TakeDamage_Ayri_Siniftaki_Formulu_Cagirirken_Tahsis_Yapmaz
     1000 × health.TakeDamage(1) ► Not.AllocatingGCMemory()
     Bir REFAKTÖR kararını savunur: formülü ayrı sınıfa çıkarmanın çalışma
     zamanı bedeli olsaydı burada görünürdü.
```

### ██ Aracın kendisi hakkında ölçülmüş olgu ██

```
ÖLÇÜLDÜ, Unity 2021.3.45f2 Mono, 2026-08-17:
   GC.GetAllocatedBytesForCurrentThread()  ►  bilerek yapılan bir dizi
                                              tahsisi için bile 0 döner
```

BCL sayacı bu Mono çalışma zamanında **bağlı değil**; üstüne kurulan her test
koşulsuz yeşil verir — önlemek için yazıldığı sahte yeşilin ta kendisi. Sürüm
doğrulandı: `ProjectSettings/ProjectVersion.txt` → `m_EditorVersion: 2021.3.45f2`.
Yerine kullanılan araç Unity Test Framework'ün kendi kısıtı; motorun `GC.Alloc`
kaydedicisine bağlanıyor, .NET'in sayacına değil.

İkinci ölçülmüş olgu, aynı dosyanın kendi kaydından: kısıt bir **lambda** alır
ve ölçülen şey **lambda'nın içi**dir. Döngü lambda'nın dışında kalırsa kaydedici
boş bir aralık ölçer ve yeşil verir — *"bu tam olarak bir kez yaşandı"*
(DamageRulesAllocationTests.cs:115-120).

### Ne ölçülMEDİ — ve bunu yazmak zorundayım

Kodda tahsis hakkında **iddia** taşıyan ama hiçbir testi olmayan dört yorum var:

| Yer | İddia | Durum |
|---|---|---|
| `Battle.cs:373-376` | küme `IEnumerable` olarak açılsaydı numaralandırıcı kutulanır ve her `Update` bir tahsis yapardı | **ÖLÇÜLMEDİ** |
| `Battle.cs:379-382` | `Dictionary<,>.Enumerator` bir `struct` olduğu için doğrudan `foreach`'te kutulanmaz | **ÖLÇÜLMEDİ** |
| `BoardAdapter.cs:205-210` | `cleanupBuffer` alan olmasaydı her karede yeni bir `List` kare başına çöp üretirdi | **ÖLÇÜLMEDİ** |
| `TurnState.cs:60-63` | her okumada `Array.AsReadOnly` çağırmak yeni bir sarmalayıcı üretir ve çöp toplayıcıyı besler | **ÖLÇÜLMEDİ** |

Ayrıca **hiç ölçülmemiş katmanlar**: `GridStrategy.Battle` ve `GridStrategy.Unity`
assembly'lerinde tek bir tahsis testi yok (ölçü: `AllocatingGCMemory` ya da
`ProfilerRecorder` geçen tek dosya `Assets/Tests/EditMode/Combat/` altında). Yani
`Battle.Tick`'in, `RemoveReadyForCleanup`'ın ve `BoardAdapter.Update`'in kare
başına tahsisi hakkında bu projenin **hiçbir kanıtı yok**.

### Dürüst sınır — her sayının yanına

Bu ölçümler EditMode'da, Editor'ün Mono'sunda, geliştirme makinesinde koşuyor:
**oyuncu derlemesi kanıtı DEĞİL, hedef cihaz kanıtı DEĞİL, IL2CPP kanıtı DEĞİL.**
Dayanıklı iddia yalnızca "0 / 0 değil"dir; mutlak bir bayt sayısı asla
sabitlenmemeli, çünkü bayt toplamları Editor Mono'ya ve 64 bite özgüdür. Ve
**sözdizimi kanıt değildir**: `foreach` görmek "kutulanıyor", `readonly` görmek
"tahsis yok" demek değil. Kanıt bir kaydedicinin okuduğu sayıdır.

---

## Altıncı durak: üç oyun, tek soru

**Soru: bir varlık sahneden çıktığında belleğine ne oluyor?**

| Oyun | Sahneden ne çıkıyor, ne sıklıkta | Bu projedeki karşılığı |
|---|---|---|
| **Slay the Spire** | Oynanan kart elden ayrılır, yığına gider. Savaş boyunca aynı küçük deste kimlikleri dolaşır; yeni kimlik yalnız ödül ekranında doğar. Çıkan şey sahneden çıkar, oyundan çıkmaz. | **VAR ve birebir.** `Battle.combatants` ve `unitViews` sabit, küçük bir kimlik kümesi taşıyor; `Awake` iki demo birim doğuruyor (BoardAdapter.cs:267-268), varsayılan tahta 3×5 = 15 hücre (BoardAdapter.cs:113-114). Doğum nadir, çıkış nadir. <!-- ATIF-MUAF: tablo hücresi; alıntı biçimi atfın satır BAŞINDA olmasını ister, tablo satırında mümkün değil. 113-114 ve 267-268'in alıntılı çapası Docs/ogrenme/02-sonraki-asamalar.md'de. --> |
| **Vampire Survivors** | ██ EŞLEŞMİYOR — ve en öğretici satır bu ██ Saniyede onlarca düşman doğar ve ölür; ekranda aynı anda binlercesi olabilir. Çıkan her varlığın belleği **aynı karenin bütçesi içinde** çözülmek zorunda. | **YOK, ve yokluğu bir karar değil bir ÖLÇEK farkı.** Bu projede `Destroy` tek bir yerde, tek bir çağrı (BoardAdapter.cs:1007) ve tetikleyicisi bir ceset sayacı. Doğum hızı yükseldiği gün ilk düşen şey `Destroy`-ve-unut yaklaşımı olur: her `Instantiate`/`Destroy` çifti yeni yönetilen nesne ve yeni yerel nesne demektir. **HENÜZ YOK → birim havuzu**, ve önem kazanacağı koşul yazılabilir: aynı karede birden fazla birim doğduğu gün. |
| **Stardew Valley** | Gün biter, harita ve içindeki her şey değişir. Envanter ve çiftliğin durumu geçişten **sağ çıkar**; sahnedeki her şey çıkmaz. İki ayrı ömür: kaydedilen ve kaydedilmeyen. | **HENÜZ YOK.** Bu projede tek sahne var, sahne geçişi yok, kayıt/yükleme yok. `Battle` `BoardAdapter.Awake`'te doğuyor (BoardAdapter.cs:238) ve hiçbir yere yazılmıyor. **HENÜZ YOK → sahne geçişi ya da kayıt sistemi**; o gün "hangi nesne geçişten sağ çıkar" sorusu doğar ve statik alanların (bugün tek örnek: `TurnState.DefaultTurnOrder`) sahne geçişinde SIFIRLANMADIĞI ilk kez önem kazanır. |

Üç satırın ortak dersi: **temizlik stratejisini oyunun kendisi değil, oyunun
DOĞUM HIZI seçer.** Bu proje bugün birinci satırda oturuyor.

---

## Bütün zincir tek bakışta

```
BİR NESNENİN ÖMRÜ — dört soru, dört sahip     `new Unit("Piyade")`
       │
  KAPSAM ──────────────────────────► `}` ile biter
       │  DERLEYİCİ                   ██ VE BURADA HİÇBİR ŞEY SİLİNMEZ ██
       ▼
  CANLILIK ────────────────────────► son okumada biter
       │  JIT                         (kapanış parantezinden ÖNCE olabilir)
       ▼
  ERİŞİLEBİLİRLİK ─────────────────► son zincir koptuğunda biter
       │  GC: kökten yürü, ulaşabildiklerini tut
       │
       │  ██ AYRIŞMA #1: bu projede zinciri koparan yer BELLİ ██
       │     Battle.RemoveUnit  →  StateChanged -= forwarder  (Battle.cs:349)
       │     Battle.RemoveUnit  →  combatants.Remove(unit)    (Battle.cs:353)
       │     BoardAdapter       →  unitViews.Remove(unit)     (BoardAdapter.cs:1002)
       ▼
  yönetilen bellek geri alınır ────► NE ZAMAN: GC bilir, sen bilmezsin
       │
       │  ██ AYRIŞMA #2: UnityEngine.Object burada İKİYE bölünür ██
       ▼
  KAYNAK ÖMRÜ ─────────────────────► Destroy(view.gameObject)  (BoardAdapter.cs:1007)
       ├─► YEREL taraf    : sıraya girer, güncelleme döngüsünden sonra yıkılır
       └─► YÖNETİLEN taraf: yerinde durur, "yıkılmış" işaretlenir
                            == null ► true   ██ ama null DEĞİL ██
                            ReferenceEquals(x, null) ► false
```

---

## Kural: bellek sorusunu ne zaman sorarsın

```
① Bir ABONELİK mi yazıyorsun? (`+=`)
      HAYIR → ②
      EVET  → Yayıncı ile abone AYRI ÖMÜRLERE mi ait?
                 hayır (parça ve sahibi) → sökme gerekmez, GERİ DÖN
                                            (örnek: Combatant → lifecycle)
                 evet  → ██ SÖKME YERİNİ ŞİMDİ YAZ ██ ve sorunu değiştir:
                         "sökülmezse hangi nesne, hangi zincirle
                          erişilebilir kalır" — cevabı yazamıyorsan
                          henüz anlamamışsındır
                         (örnek: Combatant → Battle, battle → BoardAdapter)

② Bir UnityEngine.Object mi YOK EDİYORSUN?
      HAYIR → ③
      EVET  → İKİ soruyu ayrı sor:
                 yerel taraf     → Destroy çağrıldı mı
                 yönetilen taraf → o nesneye ok tutan tablo/alan/liste
                                   TEMİZLENDİ Mİ
              ██ İkincisini atlarsan tabloda "null gibi ama null
                 değil" bir referans kalır ██
              SIRA: önce tablodan çıkar, SONRA Destroy et

③ Bir PERFORMANS iddiası mı yazacaksın? ("bu tahsis yapmaz", "bu ucuz")
      HAYIR → ④
      EVET  → ÖLÇ. Ölçemiyorsan İDDİA ETME, "ölçülmedi" yaz. Sıra zorunlu:
                 1. negatif kontrol (araç kör mü)
                 2. ısınma (JIT'in tek seferlik maliyeti dışarıda kalsın)
                 3. ölçüm penceresi (kod pencerenin İÇİNDE koşsun)
                 4. eleme koruması (sonucu oku, döngü atılmasın)
              ██ Sözdizimi kanıt değildir ██

④ Sorduğun şey "bu değer stack'te mi heap'te mi" mi?
      EVET  → ██ SORUYU BIRAK ██ Doğru soru ①, ② ya da ③'tü.
      HAYIR → soru gerçekten bellekle ilgili değil. Kaldığın yere dön.
```

---

## Yanlış hatırlanan dört şey

**"Değer tipi stack'te durur, referans tipi heap'te."** ██ Bu dosyanın en pahalı
yanlışı ██ ve yaygın olması onu doğru yapmıyor. Değer/referans ayrımı bir
**semantik** kuraldır: değer tipi kopyalanarak aktarılır. **Depolama yeri bağlama
bağlıdır** — bir `struct` bir sınıfın alanıysa yönetilen yığındadır
(`UnitView.authoredColor`), bir lambda tarafından yakalanırsa yığındadır
(`DamageRulesAllocationTests` bundan kaçınmak için bilerek bir ALAN kullanıyor),
ve JIT bir nesneyi kayıtta tutabilir. Doğru cümle: *"değer tipi kopyalanarak
aktarılır"*, "değer tipi stack'tedir" değil.

**"Kapanış parantezi `}` nesneyi siler / bir GC noktasıdır."** Hiçbiri; `}`
yalnızca **kapsamı** bitirir ve kapsam derleme zamanına ait bir kavramdır.
Canlılık ondan önce bitebilir; erişilebilirlik ondan çok sonra — referans bir
alana, statiğe, davet listesine, kapanışa ya da koleksiyona kopyalanmışsa. En
görünür örneği `Battle.AddUnit`: metot biter, ürettiği `forwarder` yaşamaya
devam eder çünkü `stateForwarders`'ta ve `Combatant`'ın davet listesinde duruyor.

**"`Destroy` çağırdım, C# nesnesi de gitti."** Gitmedi. `Destroy` yalnızca
**yerel** eşi yıkar; yönetilen sarmalayıcı yerinde durur, "yıkılmış" işaretlenir
ve ancak hiçbir kök ona ulaşamayınca GC'ye uygun hâle gelir. Gözlemlenebilir izi:
`== null` `true`, `ReferenceEquals(x, null)` `false`.

**"Bir yerde referansı varsa nesne yaşar."** Eksik. Bir alan kendiliğinden kök
değildir — **onu barındıran nesnenin kendisi bir kökten erişilebilir olmalı.**
`Combatant` → `UnitLifecycle` → delege → `Combatant` halkası bunun kanıtı: üç
nesne birbirine ok tutuyor ve üçü birden toplanabiliyor. İzleyen bir toplayıcı
için döngü sorun değildir; referans sayan bir toplayıcı için ölümcüldür.

---

## Kaçış yolu: bu tasarımlardan nasıl kaçılırdı ve neden kaçılmadı

```
stateForwarders yerine    → olayın imzasına GÖNDERENİ koy: kapanış gereksiz
gönderenli imza             kalır, sökülecek şey bir metot adı olur.
                            NEDEN KAÇILMADI: Combatant kendi Unit'ini bilmiyor
                            (gerekçe konular/01'de) — fatura azalırdı, SIFIRLANMAZDI

`-=` disiplini yerine     → aboneliği bir IDisposable'a sarıp `using` ile kapat.
IDisposable                 NEDEN YOK: projede tek bir IDisposable yok; eklemek
                            dört assembly'ye bir kavram sokardı.
                            HENÜZ YOK → abonelik sayısı üçü aştığı gün

Destroy yerine havuz      → yok etme, kapat ve listeye geri koy; iki taraf da
(pooling)                   HİÇ ölmez.
                            NEDEN YOK: doğum hızı iki (BoardAdapter.cs:267-268);
                            havuz ölçülmemiş bir soruna yazılmış kod olurdu.
                            HENÜZ YOK → Vampire Survivors satırındaki koşul

GC.Collect() ile          → NEDEN YOK ve OLMAMALI: duraklama üretir ve
zamanlamayı ele almak       erişilebilirliği DEĞİŞTİRMEZ — tutan zinciri
                            koparmaz. Sızıntının çaresi `-=`'dir

WeakReference ile         → NEDEN YOK: bu projede sökme yeri VAR ve tek satır
sökmeyi gereksizleştirmek   (Battle.cs:349); zayıf referans, var olan bir satırı
                            belirsiz bir zamanlamayla değiştirmek olurdu
```

---

Kodda **karar**, burada **ödünç alınan çalışma zamanının sözleşmesi**. İkisi
çelişirse kod kazanır — orası çalışan metin, burası anlatı. Ve bu dosyadaki her
sayı koda karşı doğrulandı; doğrulanamayan her iddianın yanında "ÖLÇÜLMEDİ"
yazıyor.

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
Assets/Game/Unity/BoardAdapter.cs:983     private void DespawnView(Unit unit)
Assets/Game/Unity/BoardAdapter.cs:1002    unitViews.Remove(unit);
Assets/Game/Unity/BoardAdapter.cs:1007    Destroy(view.gameObject);
Assets/Game/Unity/BoardAdapter.cs:288     private void OnEnable()
Assets/Game/Unity/BoardAdapter.cs:238     battle = new Battle(width, height);
Assets/Game/Unity/BoardAdapter.cs:566     private void CreateStructureVisual(int x, int y)
Assets/Game/Unity/BoardAdapter.cs:572     var renderer = structureObject.AddComponent<SpriteRenderer>();
Assets/Game/Unity/BoardAdapter.cs:634     for (int i = 0; i < cleanupBuffer.Count; i++)
Assets/Game/Battle/Battle.cs:226          Action<UnitState, UnitState> forwarder =
Assets/Game/Battle/Battle.cs:349          combatants[unit].StateChanged -= forwarder;
Assets/Game/Battle/Battle.cs:353          combatants.Remove(unit);
Assets/Game/Battle/TurnState.cs:51        public const int FirstTurnNumber = 1;
Assets/Game/Core/Combat/Combatant.cs:90   this.lifecycle.StateChanged += OnLifecycleStateChanged;
```
