# Bir askerin düşüşü — olayın dört durağı

> **Nerede geçiyor:** `UnitLifecycle.cs` → `Combatant.cs` → `Battle.cs` → `BoardAdapter.cs`
> **Kodda nereden geldin:** `Combatant.StateChanged`, `Battle.stateForwarders`, `Battle.UnitStateChanged`
> **Ne zaman oku:** bu dört üyeden birine dokunmadan önce, ya da "bu sözlük neden var" diye sorduğunda.

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

En tuhafı ikincisi: **`Combatant` kendi kimliğini bilmiyor.**

Bu bir eksiklik değil, bilinçli bir karar. `Combatant` bir `Unit` alsaydı,
`GridStrategy.Combat` ad alanı `GridStrategy.Core`'a bağlanırdı ve savaş kuralları
tahtayı tanımaya başlardı. Kimlik `Unit`'te yaşıyor, eşleme `Battle`'da. `Combatant`
sadece "ne kadar canı var" sorusuna cevap veriyor.

**Bütün hikâye bu tek karardan doğuyor.** Aklında tut.

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

Kim dinliyor? `Combatant`, ve şöyle abone olmuş:

```csharp
lifecycle.StateChanged += OnLifecycleStateChanged;    // ① METOT ADI
```

**Bu detayı işaretle.** Metot adı. Lambda değil. Birazdan önemli olacak.

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
