# Bir askerin düşüşü — olayın dört durağı

> **NEREDE GEÇİYOR** — *bu mekanizmanın kat ettiği kaynak dosyalar, akış sırasıyla:*
> `Assets/Game/Core/Combat/UnitLifecycle.cs` → `Assets/Game/Core/Combat/Combatant.cs`
> → `Assets/Game/Battle/Battle.cs` → `Assets/Game/Unity/BoardAdapter.cs`
> → `Assets/Game/Unity/UnitView.cs`
>
> **NE ZAMAN OKU** — *hangi soruyu sorduğunda ya da hangi değişikliğe giriştiğinde:*
> aşağıdaki üyelerden birine dokunmadan önce, ya da "bu sözlük neden var" diye
> sorduğunda.

**BURAYA KODDAN GELDİYSEN** — aşağıdaki üyelerin **yorumunda** bu belgeye bir
`DERİN ANLATIM:` işaretçisi var. Yol: `Ctrl+P` → dosya adının ayırt edici
parçasını yaz → `Ctrl+F` ile **üye adını** ara. ██ Satır numarası bilerek
yazılmıyor: satır kayar, üye adı kaymaz. ██

| dosya | üye | koddan işaretçi |
|---|---|---|
| `Assets/Game/Core/Combat/UnitLifecycle.cs` | `StateChanged` | ✓ |
| `Assets/Game/Core/Combat/Combatant.cs` | `StateChanged` | ✓ |
| `Assets/Game/Battle/Battle.cs` | `stateForwarders` · `UnitStateChanged` | ✓ |
| `Assets/Game/Unity/BoardAdapter.cs` | `OnUnitStateChanged` · `ApplyStateVisual` | ██ HENÜZ YOK ██ |
| `Assets/Game/Unity/UnitView.cs` | `SetState` | ██ HENÜZ YOK ██ |
| `Assets/Tests/EditMode/Combat/CombatantTests.cs` | `TransitionLog` | ✓ |

██ **"HENÜZ YOK" ne demek:** o üye burada gerçekten anlatılıyor, ama **kodun
yorumunda buraya geri getiren bir satır yok** — o üyeden yola çıkıp bu belgeye
ulaşamazsın, yalnız tersi çalışır. Listeden silmedim; silmek boşluğu görünmez
kılardı. ██

> ██ **ÖNCE OKU — bu dosya bir GİRİŞ değil, bir BULUŞMA NOKTASI.** ██ Numarası
> `01` ama üç ayrı ipliğin düğümlendiği yer burası, ve üçünün de tanımı başka
> dosyalarda:
>
> | Bu dosyada tanımsız kullanılan | Sahibi |
> |---|---|
> | *ad alanı*, *assembly*, "bağlanmak" (aşağıda, "Karakterler"in sonunda) | [`konular/02`](02-assembly-duvari.md#uc-ayri-sey-uc-ayri-is) |
> | `Alive` / `Downed` / `Dead` — üç durumun ne demek olduğu | [`konular/05`](05-yasam-dongusu.md) |
> | `Action<…>`, `event`, `?.Invoke`'un ne **vaat ettiği** | [`dil/04`](../dil/04-delege-olay-ve-kapanis.md#birinci-durak-delege-metoda-isaret-eden-nesne) |
>
> Üçünü de okumadan bu dosya okunur — ama "niye böyle" sorusu cevapsız kalır.
> Önerilen yol bu dosyayı **onuncu** adıma koyuyor:
> [`../../ogrenme/00-okuma-sirasi.md`](../../ogrenme/00-okuma-sirasi.md).
>
> ██ **KAYBOLMAZSIN:** yukarıdaki üç hedefin üçü de bölümünün sonunda bir
> ██ `◀ DÖNÜŞ` ██ satırı taşıyor — nereden gelmiş olabileceğini, dönünce neyi
> bileceğini ve buraya **hangi bölümden** devam edeceğini yazıyor. Bu dosyanın
> gövdesinde ayrıca üç `▶ ARA DURAK` var; her birinin **DÖNÜŞ** alanı seni geri
> getirecek bölümü adıyla söylüyor. ██

---

## Sahne

Oyuncu bir askere vuruyor. Canı bitiyor. Ekranda asker yere yatıyor, rengi soluyor.

Tek cümle. Tek kare. Ama arkada **dört ayrı tip** çalışıyor ve ilginç olan şu:
**hiçbiri diğerini tam olarak tanımıyor.**

Bu dosya o dört tipin nasıl konuştuğunu anlatıyor — ve neden `Battle`'ın içinde,
başka hiçbir yerde olmayan tuhaf bir sözlük durduğunu.

---

## Karakterler

Önce oyuncuları tanıyalım. Her birinin **bildiği** ve **bilmediği** şeyler var, ve
hikâyeyi ilginç kılan tam olarak bilmedikleri.

```
╔═ UnitLifecycle ═══════════════════════════════════════════════╗
║  İşi     : sayaç tutmak. "düştüğünden beri kaç saniye geçti"  ║
║  Bilir   : Alive / Downed / Dead                              ║
║  BİLMEZ  : kimin sayacı olduğunu. Canı. Tarafı. Tahtayı.      ║
╚═══════════════════════════════════════════════════════════════╝

╔═ Combatant ═══════════════════════════════════════════════════╗
║  İşi     : bir savaşçının niteliklerini bir arada tutmak      ║
║  Bilir   : can, taraf, saldırı profili, yaşam döngüsü          ║
║  BİLMEZ  : ██ KENDİ KİMLİĞİNİ ██ ve nerede durduğunu          ║
╚═══════════════════════════════════════════════════════════════╝

╔═ Battle ══════════════════════════════════════════════════════╗
║  İşi     : eşleştirmek. "bu kimlik hangi savaşçı, hangi hücre"║
║  Bilir   : Unit ↔ Combatant eşlemesi, tahta, sıra             ║
║  BİLMEZ  : ekranı. Sprite. Renk. Animasyon.                   ║
╚═══════════════════════════════════════════════════════════════╝

╔═ BoardAdapter ════════════════════════════════════════════════╗
║  İşi     : çevirmenlik. Motor ile savaş arasında              ║
║  Bilir   : GameObject, Sprite, Grid, fare tıklaması           ║
║  BİLMEZ  : savaş kurallarını. Tek satır kural yazmaz.         ║
╚═══════════════════════════════════════════════════════════════╝
```

### ██ KADRO — dört kutu arasındaki ZİNCİR ██

Yukarıdaki dört kutu dört ayrı tipi tanıtıyor ama **aralarındaki oku
göstermiyor**. O oku okuyucunun kendi kafasından çıkarması gerekiyordu; artık
gerekmiyor:

```
UnitLifecycle          sayacı tutar, kimin sayacı olduğunu BİLMEZ
      │ StateChanged  (tek değer: yeni durum)
      ▼
Combatant              can + taraf + profil + yaşam döngüsü; KENDİ KİMLİĞİNİ BİLMEZ
      │ StateChanged  (iki değer: önceki, sonraki)
      ▼
Battle                 kimlik ↔ savaşçı eşlemesi; EKRANI BİLMEZ
      │ UnitStateChanged  (üç değer: kim, önceki, sonraki)
      ▼
BoardAdapter           motor ile savaş arasında çevirmen; KURAL BİLMEZ
      │
      ▼  GameObject · Sprite · renk
```

██ **Koda karşı doğrulandı** ██ — üç olayın imzası sırasıyla
`Action<UnitState>`, `Action<UnitState, UnitState>` ve
`Action<Unit, UnitState, UnitState>`: **bir, iki, üç** değer. Her aşağı ok
kodda tek bir `+=` satırıdır ve üçü de açılabilir.

**Her ok hangi durakta açılıyor:**

| Ok | Bu dosyada nerede | Geçerken ne KAZANIYOR |
|---|---|---|
| `UnitLifecycle → Combatant` | [«Birinci durak: sayaç konuşuyor»](#birinci-durak-sayac-konusuyor) | önceki durum |
| `Combatant → Battle` | [«Üçüncü durak: kayıt memuru…»](#ucuncu-durak-kayit-memuru-kimligi-ekliyor-ve-fatura-burada-kesiliyor) | kimlik — ██ faturanın kesildiği ok ██ |
| `Battle → BoardAdapter` | [«Dördüncü durak: çevirmen…»](#dorduncu-durak-cevirmen-ekrani-guncelliyor) | hiçbir şey — ekleyecek bir şeyi yok |
| `BoardAdapter → ekran` | [«Ekran yarısı»](#ekran-yarisi-guncellendi-tek-kelime-degil-uc-halka) | yatıklık + renk |

> **⌨ KODU AÇ:** `Assets/Game/Core/Combat/UnitLifecycle.cs` → `StateChanged` ·
> `Assets/Game/Core/Combat/Combatant.cs` → `StateChanged` ·
> `Assets/Game/Battle/Battle.cs` → `UnitStateChanged`
> **BAK:** üç bildirimi üst üste koy; tek değişen şey açılı parantezin
> içindeki **değer sayısı**. Zincirin tamamı bu üç satırda yazılı.
> **DÖNÜŞ:** bu dosyanın «Karakterler» bölümü — aşağıdaki cümleyle devam et

En tuhafı ikincisi: **`Combatant` kendi kimliğini bilmiyor.**

Bu bir eksiklik değil, bilinçli bir karar. `Combatant` bir `Unit` alsaydı,
`GridStrategy.Combat` ad alanı `GridStrategy.Core`'a bağlanırdı ve savaş kuralları
tahtayı tanımaya başlardı. Kimlik `Unit`'te yaşıyor, eşleme `Battle`'da. `Combatant`
sadece "ne kadar canı var" sorusuna cevap veriyor.

**Bütün hikâye bu tek karardan doğuyor.** Aklında tut.

> **▶ ARA DURAK:** [02-assembly-duvari.md](02-assembly-duvari.md#uc-ayri-sey-uc-ayri-is)
> **NEDEN:** yukarıdaki cümle bu dosyanın **taşıyıcı gerekçesi** ve üç tanımsız
> kavram taşıyor: *klasör*, *ad alanı* ve *assembly* üç **ayrı** şeydir. `02` o
> kararın bir tercih değil ██ derleyici tarafından uygulanan bir kısıt ██
> olduğunu gösteriyor — ve aynı kararı öteki yönden anlatıyor (`02:440`).
> **DÖNÜŞ:** bu dosyanın [«Birinci durak: sayaç konuşuyor»](#birinci-durak-sayac-konusuyor) bölümü

---

## Birinci durak: sayaç konuşuyor

Askerin canı bitti. `UnitLifecycle` durumu `Alive`'dan `Downed`'a çeviriyor ve
bağırıyor:

```csharp
StateChanged?.Invoke(next);          // UnitLifecycle.cs
//                   ▲
//              tek kelime: "Downed"
```

Bu bağırış çok fakir. Sadece **yeni durumu** taşıyor. Kim olduğu yok, nereden
geldiği yok. Çünkü `UnitLifecycle` gerçekten de bunları bilmiyor — o sadece bir
sayaç.

> **▶ ARA DURAK:** [05-yasam-dongusu.md](05-yasam-dongusu.md#neden-uc-durum-iki-olsaydi-hangi-cumle-yazilamazdi)
> **NEDEN:** `Downed` bu dosyada **tanımsız** kullanılıyor ve zincirin ilk
> halkası tam olarak o değeri taşıyor. `Downed`'ın neden `Dead`'den ayrı bir
> değer olduğunu bilmeden "bağırışın fakirliği" bir eksiklik gibi okunur;
> oysa sayacın taşıdığı tek kelime **üç değerli** bir kümeden geliyor.
> **DÖNÜŞ:** bu dosyanın [«İkinci durak: savaşçı zenginleştiriyor»](#ikinci-durak-savasci-zenginlestiriyor) bölümü

Kim dinliyor? `Combatant`, ve şöyle abone olmuş:

```csharp
lifecycle.StateChanged += OnLifecycleStateChanged;    // ① METOT ADI
```

**Bu detayı işaretle.** Metot adı. Lambda değil. Birazdan önemli olacak.

> **⌨ KODU AÇ:** `Assets/Game/Core/Combat/UnitLifecycle.cs` → `SetState`, sonra
> `Assets/Game/Core/Combat/Combatant.cs` → kurucusunun **son** satırı
> **BAK:** `SetState` durumu **önce yazıyor**, `Invoke`'u sonra çağırıyor —
> yayın anında `State` zaten yeni değerde. Karşı tarafta `Combatant` bu olaya
> bir **metot adıyla** abone oluyor, lambdayla değil.
> **DÖNÜŞ:** bu dosyanın «Birinci durak: sayaç konuşuyor» bölümü

---

## İkinci durak: savaşçı zenginleştiriyor

`Combatant` bağırışı duyuyor ve bir şey ekliyor: **önceki durum.**

```csharp
private void OnLifecycleStateChanged(UnitState next)
{
    UnitState previous = lastObservedState;
    lastObservedState = next;              // önce hatırla
    StateChanged?.Invoke(previous, next);  // sonra yay
    //             ▲
    //        iki kelime: "Alive'dan Downed'a"
}
```

Neden önceki durum lazım? Çünkü ekran "düştü" ile "zaten düşmüştü, bir daha
vuruldu" arasında fark görmek zorunda. Birincisinde düşme animasyonu oynar,
ikincisinde oynamaz.

`Combatant` bu bilgiyi ekleyebiliyor çünkü **sahibi o** — `lastObservedState`
onun alanı. Kimseden istemedi, kendi tuttu.

Ama hâlâ eksik olan bir şey var: **kim düştü?**

`Combatant` bunu ekleyemez. Bilmiyor. Yukarı bak.

---

## Üçüncü durak: kayıt memuru kimliği ekliyor — ve fatura burada kesiliyor

`Battle` kimliği biliyor, çünkü eşlemeyi tutan o. Şimdi `Combatant`'ın bağırışını
dinleyip üstüne "kim" bilgisini eklemesi gerek. Nasıl?

İlk akla gelen, ① ile aynı şeyi yapmak:

```csharp
combatant.StateChanged += OnCombatantStateChanged;   // ✗ ÇALIŞMAZ
```

Neden çalışmaz? Çünkü savaşta **N tane savaşçı var** ve bu metot çağrıldığında
"hangisi bağırdı" diye soracak. Olay kimlik taşımıyor, metot da parametre olarak
alamıyor — imza sabit.

Tek yol: her savaşçı için **kimliği içine gömülmüş ayrı bir fonksiyon** üretmek.

```csharp
Action<UnitState, UnitState> forwarder =
    (previous, next) => UnitStateChanged?.Invoke(unit, previous, next);
//                                        ▲
//                          `unit` YAKALANDI — bu fonksiyon artık
//                          sadece bu birim için çalışan bir nesne
combatant.StateChanged += forwarder;
```

Buna **kapanış** (closure) denir: dışarıdaki bir değişkeni içine alıp taşıyan
fonksiyon. Her birim için bir tane üretiliyor:

```
combatant#1.StateChanged  ◄──  fonksiyon nesnesi #1  (içinde: unitA)
combatant#2.StateChanged  ◄──  fonksiyon nesnesi #2  (içinde: unitB)
combatant#3.StateChanged  ◄──  fonksiyon nesnesi #3  (içinde: unitC)
                                        ▲
                    METİN aynı, YAKALANAN DEĞER farklı → ÜÇ AYRI NESNE
```

### Ve işte sözlüğün doğduğu an

Birim savaştan çıkınca bu aboneliği **sökmek** gerekiyor. Sökme şöyle yazılır:

```csharp
combatant.StateChanged -= <hangi fonksiyon?>
```

Buraya aynı metni yeniden yazarsan **hiçbir şey olmaz**:

```csharp
combatant.StateChanged -= (p, n) => UnitStateChanged?.Invoke(unit, p, n);
//                        ╰─ bu YENİ bir nesne (#4). Listede #1 var.
//                        ██ -= sessizce başarısız olur. Hata YOK. ██
```

C#'ta iki fonksiyon nesnesi, **metinleri aynı olsa bile** eşit değildir. Eşitlik
nesneye bakar. Yani sökebilmek için **tam olarak abone olunan nesneyi** elinde
tutman lazım.

Tutulduğu yer:

```csharp
private readonly Dictionary<Unit, Action<UnitState, UnitState>> stateForwarders;
//                          ▲       ▲
//                   hangi birim   o birim için ürettiğim fonksiyon NESNESİ
```

**Sözlük bir cep.** İçinde "ne oldu" değil, "kimi çıkaracağım" yazıyor.

> **⌨ KODU AÇ:** `Assets/Game/Battle/Battle.cs` → `stateForwarders` alanı, sonra
> `AddUnit` ve `RemoveUnit`
> **BAK:** `AddUnit` kapanışı üretip **hem** olaya abone ediyor **hem** sözlüğe
> yazıyor; `RemoveUnit` önce sözlükten `TryGetValue` ile **aynı nesneyi** geri
> alıyor, `-=` onunla çağrılıyor, sonra sözlükten siliniyor. Üç satır aynı
> nesneyi gezdiriyor — sözlüğün var olma sebebi bu tur.
> **DÖNÜŞ:** bu dosyanın «Ve işte sözlüğün doğduğu an» bölümü

> **◀ DÖNÜŞ:** [../dil/04-delege-olay-ve-kapanis.md](../dil/04-delege-olay-ve-kapanis.md#dorduncu-durak-kapanis-kimligi---neye-bakiyor) — «Dördüncü durak: kapanış kimliği»nden
> geldiysen artık şunu biliyorsun: `-=`'in bakmadığı kimliği bu projede tutan yer
> `Battle.stateForwarders`'tır — dilin kuralı burada bir **alana** dönüşüyor ·
> oraya dön ve kapanış kimliğinden devam et

---

## Dördüncü durak: çevirmen ekranı güncelliyor

`Battle` artık üç kelimelik bir cümle kurabiliyor:

```csharp
public event Action<Unit, UnitState, UnitState> UnitStateChanged;
//                  ▲     ▲          ▲
//                 KİM   önceki     yeni
```

`BoardAdapter` bunu dinliyor:

```csharp
battle.UnitStateChanged += OnUnitStateChanged;     // ③ METOT ADI
```

Yine metot adı. Yine sözlük yok. Neden? Çünkü `BoardAdapter` **tek bir** `Battle`
dinliyor ve olay zaten "kim" bilgisini taşıyor. Ekleyecek bir şeyi yok.

### Ekran yarısı: "güncellendi" tek kelime değil, üç halka

Zincir `OnUnitStateChanged`'de **bitmiyor**. Rengin gerçekten değiştiği yere
kadar üç halka daha var, ve üçü de kodda açılabilir:

```
  BoardAdapter.cs:288   private void OnEnable()
  BoardAdapter.cs:290       battle.UnitStateChanged += OnUnitStateChanged;
  BoardAdapter.cs:293   private void OnDisable()
  BoardAdapter.cs:295       battle.UnitStateChanged -= OnUnitStateChanged;
        │  ██ Abonelik ÇİFTİ burada; sökme sözü tutan tek şey DİSİPLİN ██
        ▼
  BoardAdapter.cs:310   private void OnUnitStateChanged(Unit unit, UnitState from, UnitState to)
  BoardAdapter.cs:312       ApplyStateVisual(unit, to);
        │  ██ AYRIŞMA NOKTASI ██ olay ÜÇ değer taşıyor, kullanılan İKİ
        │     `from` bugün okunmuyor — eksiklik değil, adı hazır bir alan
        ▼
  BoardAdapter.cs:954       private void ApplyStateVisual(Unit unit, UnitState state)
  BoardAdapter.cs:956-959       if (!TryGetView(unit, out UnitView view))
  BoardAdapter.cs:964       view.SetState(state);
        │  ██ ÇEVİRİ YOK ██ durum OLDUĞU GİBİ geçiyor. Bir zamanlar burada
        │     üç değer ikiye iniyordu ve Downed ile Dead ekranda aynı görünüyordu
        ▼
  UnitView.cs:173       public void SetState(UnitState state)
  UnitView.cs:182           bodyRenderer.flipY = state != UnitState.Alive;
  UnitView.cs:190           bodyRenderer.color = authoredColor * TintFor(state);
        │
        ▼
  ██ ASKER EKRANDA YATIK VE SOLGUN ██
```

Son iki satır **üç durumu iki eksenle** yazıyor: `flipY` yalnız "ayakta mı"
sorusunu soruyor (`Downed` ile `Dead` o eksende **aynı**), rengi ayıran ise
çarpım. Üçünü ayıran şey iki eksenin **birleşimi** — tek eksenle yazılamayacak
bir cümle. Gerekçesinin tamamı `UnitView.cs`'in kendi yorumunda.

██ **Bu ekseni `konular/07` tamamlamıyor** ██ — orada da bir "ekran" durağı var
ama o **seçim** ekseninde (`SetSelected` → çerçeve), bu **durum** ekseninde
(`SetState` → yatıklık + renk). `07:658` iki ekseni açıkça ayırıyor:
*"İKİ EKSEN BURADA KESİŞMEZ."* Aynı `UnitView`, iki ayrı soru.

Cümle tamamlandı, asker ekranda yere yattı.

---

## Bütün zincir tek bakışta

```
UnitLifecycle.StateChanged          Action<UnitState>
   "Downed"                         1 değer
        │
        │  ① abone: Combatant.OnLifecycleStateChanged
        │     ██ metot adı ██   1 kaynak   ekleyecek: önceki durum (sahibi o)
        ▼
Combatant.StateChanged              Action<UnitState, UnitState>
   "Alive → Downed"                 2 değer      ◄── kim YOK
        │
        │  ② abone: Battle'ın forwarder'ı
        │     ██ KAPANIŞ ██     N kaynak   ekleyecek: kimlik (sahibi o)
        │     ╰─► her biri ayrı nesne ╰─► sökmek için SÖZLÜK
        ▼
Battle.UnitStateChanged             Action<Unit, UnitState, UnitState>
   "unitA: Alive → Downed"          3 değer
        │
        │  ③ abone: BoardAdapter.OnUnitStateChanged
        │     ██ metot adı ██   1 kaynak   ekleyecek: yok
        ▼
   ekran güncellendi
```

**Üç abonelik var. Sadece biri sözlük gerektiriyor.**

---

## Kural: ne zaman saklaman gerekir

Zincirden çıkan ölçüt tek. Sırayla sor:

```
① Bu aboneliği ileride SÖKECEK misin?
      HAYIR → saklama. Nesne ölünce olay da ölür.
      EVET  → ②

② Abone ettiğin şey METOT ADI mı, LAMBDA mı?
      metot adı → saklama. `-= OnFoo` her zaman çalışır.
      lambda    → ③

③ Lambda neden gerekti? Dışarıdan bir değişken YAKALADIĞIN için mi?
      HAYIR → metoda çevir, ②'ye dön. Sorun biter.
      EVET  → ██ SAKLA ██
```

Ve ③'e düşmenin tek gerçek sebebi şudur: **N kaynağa abone oluyorsun ve olay
kimlik taşımıyor.** Bir kaynağa abone olsaydın "hangisi" diye sormazdın.

---

## Yanlış hatırlanan iki şey

**"Structure'da da aynısı vardır."** Yok. `StructureLifecycle`'ın olayı hiç yok —
`StructureLifecycle.cs`'te reddedilen alternatif olarak duruyor. Abonelik yoksa
saklama sorunu da yok. `AddStructure` hiçbir şey abone etmez, `RemoveStructure`
hiçbir şey sökmez.

**"Sözlük nesneyi tutmak için."** Tam tersi — **bırakabilmek** için. Nesneyi zaten
`combatants` sözlüğü tutuyor. Bu sözlük sadece "sökeceğim fonksiyon hangisiydi"
sorusunun cevabını saklıyor.

---

## Sökülmezse ne olur: ok yönüne dikkat

Sızıntı denince akla "bellek dolar" gelir. Burada önce başka bir şey patlar.

```
╔═ Combatant #1 ══════════════════════════╗
║  StateChanged davet listesi:            ║
║    [0] forwarder ──yakaladığı──► unitA  ║
║                  ──yakaladığı──► Battle ║
╚═════════════════════════════════════════╝
              ▲
   ██ Ok yönü: DİNLENEN → DİNLEYEN ██
   Yani Combatant, Battle'ı hayatta tutuyor. Sezginin tersi.
```

`RemoveUnit`'te sökülmezse:

```
birim sözlükten çıktı, tahtadan çıktı, ekrandan silindi
   │
   ├─► ① O birim bir daha durum değiştirirse Battle HÂLÂ yayın yapar
   │      → BoardAdapter silinmiş birimin görselini arar → LogError
   │      ██ önce bu patlar ██
   │
   └─► ② Combatant'a başka bir yerden referans varsa (havuz, test fixture)
          Battle o referans yaşadıkça toplanamaz
```

Ve en sinsi tarafı: `-=` yanlış nesneyle çağrılırsa **hata vermez**. Derleyici
susar, testler yeşil kalır, liste olduğu gibi durur. `stateForwarders`'ın var olma
sebebi tam olarak bu sessizliği önlemek.

> **◀ DÖNÜŞ:** [../dil/07-bellek-canlilik-ve-yikim.md](../dil/07-bellek-canlilik-ve-yikim.md#kod-bunu-gercekte-ne-yapiyor-sokme-yeri-var) — «Kod bunu GERÇEKTE ne yapıyor: sökme yeri var»dan
> geldiysen artık şunu biliyorsun: aynı eksik `-=` **iki ayrı fatura** kesiyor —
> burada gördüğün **davranış** faturası (silinmiş birimin görseli aranır), orada
> ölçülen **bellek** faturası (yedi hop, bütün savaş erişilebilir kalır) ·
> oraya dön ve sökme yerinden devam et

### ③ Bir abone FIRLARSA — sökülme değil, yayın faturası

Yukarıdaki iki dal **sökülmemiş** bir aboneyi anlatıyor. Üçüncü bir dal var ve
onun tetiği sökme değil: bir abonenin **istisna fırlatması**. `Invoke` bir
`try`/`catch` **taşımaz**, dolayısıyla iki şey birden olur — kalan aboneler hiç
çağrılmaz, **ve yayıncının `Invoke` satırından sonraki kendi satırları da
çalışmaz.** Bu projede o satırın ne olduğu tuzağı soyut olmaktan çıkarıyor:

```csharp
// UnitLifecycle.cs:142-143 — OnHealthDepleted'in son iki satırı
SetState(UnitState.Downed);              // ← içinde StateChanged?.Invoke var
remainingSeconds = downedWindowSeconds;  // ██ BİR ABONE FIRLARSA ÇALIŞMAZ ██
```

```
   State = Downed        ✓ yazıldı   (SetState, Invoke'tan ÖNCE atıyor)
   remainingSeconds      ✗ 0'da kaldı
        │
        ▼
   ilk Tick(0.016f):  Alive değil · 0 − 0,016 ≤ 0 · Downed  →  SetState(Dead)
        │
        ▼
   ██ 10 SANİYELİK KURTARMA PENCERESİ TEK KAREDE YOK OLDU ██
```

██ **AYRIŞMA NOKTASI** ██ — sezgi "bir abonenin hatası aboneyi ilgilendirir"
der; kod "bir abonenin hatası **yayıncının kendi değişmezini** kurmasını
engeller" der. Ayrışan iki şey: *hatanın kaynağı* ile *hatanın faturasını
ödeyen*.

**Bugün neden görünmüyor:** üç olayın da üretimde **birer** abonesi var ve
hiçbiri fırlatmıyor. Önem kazanacağı gün ölçülebilir: `Battle.UnitStateChanged`'e
ikinci bir abone (ses, skor, başarım) eklendiği gün.

> **▶ ARA DURAK:** [../dil/06-delege-arka-taraf.md](../dil/06-delege-arka-taraf.md#istisna-tuzagi)
> **NEDEN:** yukarıdaki paragrafın taşıyıcı gerekçesi — `Invoke`'un bir
> `try`/`catch` **taşımaması** — bu dosyada iddia edilip kanıtlanmıyor.
> Mekanizmanın tamamı ve aynı dosyadan **karşı örneği** (`TryRevive`'da aynı
> şeklin neden zararsız olduğu) orada.
> **DÖNÜŞ:** bu dosyanın [«③ Bir abone FIRLARSA»](#3-bir-abone-firlarsa-sokulme-degil-yayin-faturasi) bölümü

---

## Bu tasarımdan kaçmanın yolu — ve neden kaçılmadı

Bütün fatura tek bir eksiklikten geliyor: **olay göndereni taşımıyor.**

```csharp
✗ public event Action<UnitState, UnitState> StateChanged;
✓ public event Action<Combatant, UnitState, UnitState> StateChanged;
//                    ▲ gönderen imzada → kapanış gereksiz → sözlük gereksiz
```

İkinci şekil `Combatant.cs`'te reddedilen alternatif olarak duruyor. Neden
seçilmedi: `Combatant` kendini gönderse bile `Battle`'ın istediği şey `Combatant`
değil `Unit` — kimlik. Ve `Combatant` `Unit`'i bilmiyor (en başa dön). Yani
gönderen eklemek faturayı **azaltırdı ama sıfırlamazdı**; `Battle` yine
`Combatant → Unit` ters aramasını yapmak zorunda kalırdı, ki o arama sözlükten
daha pahalı.

**Kendi tipini tasarlarken** bu tuzağa hiç düşmezsin: olayın imzasına göndereni
koy, iş biter. Buradaki mimari, kimliği bilerek başka bir katmanda tuttuğu için
bedeli ödemeyi seçti.

---

## Bunu okuduktan sonra kodda ne göreceksin

`Battle.cs`'te `stateForwarders` alanının üstündeki blok artık kısa: kararın
kendisini ve reddedilen alternatifi söylüyor, zinciri anlatmıyor. Zincir burada.

Kodda karar, burada hikâye. İkisi çelişirse **kod kazanır** — orası çalışan
metin, burası anlatı.

---

## ██ SIRADAKİ ADIM ██

> **▶ SIRADA:** [`../dil/04-delege-olay-ve-kapanis.md`](../dil/04-delege-olay-ve-kapanis.md) — okuma yolunun **11.** adımı,
> hemen ardından [`../dil/06-delege-arka-taraf.md`](../dil/06-delege-arka-taraf.md) (bu sıra **zorunlu**, `06` kendi başında yazıyor)
> **NEDEN ORASI:** bu dosya zincirin **ne yaptığını** gösterdi; `dil/04` aynı
> malzemenin **ne vaat ettiğini** anlatıyor, `dil/06` ise `+=` çalıştığında
> derleyicide **ne olduğunu**. Zinciri önce gör, sözleşmeyi sonra oku — tersi,
> malzemeyi hiç kullanmadan öğrenmek olurdu.
> **YOL HARİTASI:** [`../../ogrenme/00-okuma-sirasi.md`](../../ogrenme/00-okuma-sirasi.md)
