# Koleksiyonlar ve "salt okunur" sözünün kapsamı

> **Nerede geçiyor:** `TurnState.DefaultTurnOrder`, `TurnState.TurnOrder`,
> `Battle.combatants`/`structures`/`stateForwarders`, `UnitGrid.cells`
> **Kodda nereden geldin:** `IReadOnlyList<Team>`, `Array.AsReadOnly`,
> `Dictionary<,>`, `foreach`, `KeyValuePair<,>`
> **Ne zaman oku:** bir koleksiyonu dışarıya açarken, ya da
> `IReadOnlyList` görüp "demek ki değişmez" diye düşündüğünde.

Bu dosya projenin kendi kararlarını değil, projenin **ödünç aldığı** tipleri
anlatıyor. Onların kodunu biz yazmadık; ama neyi vaat ettiklerini bilmeden
kendi kararlarımızı okuyamayız.

---

## Sahne

`TurnState`'in başında şu satır duruyor:

```csharp
public static readonly IReadOnlyList<Team> DefaultTurnOrder =
    Array.AsReadOnly(new[] { Team.Player, Team.Enemy });
```

Tek satır, dört ayrı kavram: bir dizi, bir sarmalayıcı, bir arayüz, ve iki
anahtar kelime. Hiçbiri projeye ait değil, hepsi .NET'ten geliyor.

---

## Karakterler

```
╔═ Team[] (dizi) ═══════════════════════════════════════════════╗
║  Ne yapar : sabit uzunlukta, sıralı, indeksle erişilen kutu   ║
║  Vaadi    : hızlı erişim                                       ║
║  BİLMEZ   : kimin elinde olduğunu. Referansı olan HERKES       ║
║             içeriğini değiştirebilir.                          ║
╚═══════════════════════════════════════════════════════════════╝

╔═ ReadOnlyCollection<T> ═══════════════════════════════════════╗
║  Ne yapar : bir diziyi/listeyi SARMALAR                        ║
║  Vaadi    : bu sarmalayıcı üzerinden yazma yolu YOK             ║
║  BİLMEZ   : alttaki diziyi başka kimin tuttuğunu ██ KRİTİK ██  ║
╚═══════════════════════════════════════════════════════════════╝

╔═ IReadOnlyList<T> (arayüz) ═══════════════════════════════════╗
║  Ne yapar : "sayılabilir + indekslenebilir + yazılamaz" YÜZÜ   ║
║  Vaadi    : bu referansı tutan mutasyon metodu ÇAĞIRAMAZ        ║
║  BİLMEZ   : arkasındaki nesnenin gerçekte ne olduğunu           ║
╚═══════════════════════════════════════════════════════════════╝
```

---

## Birinci durak: `IReadOnlyList` neyi vaat ETMEZ

En yaygın yanlış model bu:

```
YANLIŞ:  IReadOnlyList<T> gördüm  →  bu koleksiyon DEĞİŞMEZ
DOĞRU :  IReadOnlyList<T> gördüm  →  BEN değiştiremem
```

Fark hayatî:

```
        ┌─ elinde Team[] tutan kod ──► DEĞİŞTİREBİLİR ✓
        │
   aynı nesne
        │
        └─ elinde IReadOnlyList<Team> tutan kod ──► değiştiremez ✗
                                                    ama ARKASINDAN
                                                    değişebilir  ██
```

Yani `IReadOnlyList` bir **kilit** değil, bir **yüz**. Alttaki diziyi tutan biri
varsa, senin salt-okunur referansın onun yaptığı değişiklikleri sessizce yansıtır.

Bu tam olarak `Battle.board`'da anlatılan "ikinci yazar" probleminin aynısı —
oradaki çözüm de aynıydı: nesneyi dışarıdan alma, içeride doğur.

### Bu projede neden güvenli

```csharp
Array.AsReadOnly(new[] { Team.Player, Team.Enemy })
//               ▲
//    dizi TAM BURADA doğuyor, kimsenin elinde referansı YOK
//    → sarmalayıcı gerçek bir kilide dönüşüyor
```

`TurnState`'in kurucusunda da aynı desen:

```csharp
order = copy;                      // içeride kopyalanmış dizi
orderView = Array.AsReadOnly(copy);
```

Çağıranın verdiği liste **kopyalanıyor**, sonra kopya sarmalanıyor. Çağıran kendi
listesini sonradan değiştirse bile savaşın sırası bozulmaz. Kopya olmasaydı
`AsReadOnly` hiçbir şey korumazdı.

**Ölçü:** çağırana verdiğin listeyi çağıran değiştirsin. Sıra bozuluyorsa
sarmalayıcı sahte güvenlik veriyor demektir.

---

## İkinci durak: indeksleyici — `T this[int index]`

`IReadOnlyList<T>`'nin tanımı içinde şu üye var:

```csharp
T this[int index] { get; }
```

Bu sözdizimi C#'ta başka hiçbir şeye benzemez ve **adını bilmeden aranamaz**:
buna *indeksleyici* denir.

Buradaki `this` "bu nesne" demek **değil**. "Bu nesnenin üzerine köşeli parantez
yazıldığında çalışacak üye" demek:

```
BİLDİRİM                        KULLANIM
T this[int index] { get; }  ◄──  liste[2]
  ▲      ▲                         ▲
  │      └── parametre              └── 2 buraya `index` olarak gelir
  └── dönen tip
```

Metot olarak yazılsaydı ayırt edilemeyecek kadar sıradan görünürdü:

```csharp
T Get(int index);            // liste.Get(2)
T this[int index] { get; }   // liste[2]      ← aynı iş, farklı çağrı biçimi
```

`Dictionary`'de `sozluk["anahtar"]` çalışmasının sebebi de bu; orada indeksleyici
`int` değil `TKey` alıyor. Yani `[]` bir dizi ayrıcalığı değil, tanımlanabilir bir
üye.

### Projedeki karşılığı

```csharp
private readonly Team[] order;   // [Player, Enemy]
private int index;               // şu an kaçıncı sıradayız

public Team Current => order[index];
//                     ▲     ▲
//                     │     └── 0 ya da 1
//                     └── dizinin kendisi
```

```
order:  [0] Player      index=0  →  Current = Player
        [1] Enemy       index=1  →  Current = Enemy

EndTurn():  index = (index + 1) % order.Length     0→1→0→1…
```

`index` burada "birden fazla listede olma durumu" değil — **tek bir listedeki
konum**, yani "sıra kimde" sorusunun sayısal hâli.

---

## Üçüncü durak: `object Current` ve neden generic değil

`foreach` yazdığında derleyici arka planda `IEnumerator` kullanır. Onun tanımına
bakınca tuhaf bir şey görünür:

```csharp
public interface IEnumerator
{
    object Current { get; }     // ← neden object?
    bool MoveNext();
    void Reset();
}
```

Cevap tasarım değil, **takvim**:

```
2002  .NET 1.0   interface IEnumerator { object Current { get; } }
                                          ▲
                            GENERIC HENÜZ YOK — T yazılamıyordu

2005  .NET 2.0   interface IEnumerator<T> : IEnumerator
                 {  T Current { get; }  }
                            ▲
                 tipli olan bu — ama ESKİSİNDEN TÜRÜYOR,
                 o yüzden object Current de miras kalıyor
```

`object Current` bir gevşek-tipleme tercihi değil, **yirmi yıllık geriye dönük
uyumluluk borcu**. Bugün `foreach` tipli olanı kullanır; `object` olan yalnızca
2002'de yazılmış kodun derlenmeye devam etmesi için orada.

### ██ Hangi `Object` ██ — en pahalı karışıklık

```
System.Object        C#'ın KÖK tipi. Her şey ondan türer: int, string,
                     senin sınıfın. Küçük harfli `object` bunun takma adı.

UnityEngine.Object   Unity'nin temel sınıfı. GameObject, Component,
                     ScriptableObject bundan türer. Sahnede yaşar,
                     Destroy() edilebilir, Inspector'da görünür.
```

**İkisinin birbiriyle ilgisi yok.** `IEnumerator.Current`'taki `object`
birincisidir ve `netstandard.dll` içinde tanımlıdır — Unity o dosyayı yalnızca
kullanır, yazmaz.

Motor olanı varsayan okuyucu bütün framework modelini yanlış kurar: `foreach`'in
sahne nesneleriyle bir ilgisi olduğunu sanır.

---

## Dördüncü durak: `Dictionary` ve `KeyValuePair`

```csharp
foreach (KeyValuePair<Unit, Combatant> pair in combatants)
```

`Dictionary<TKey, TValue>` üzerinde gezinirken her adımda **iki değer birden**
gelir. C#'ın çoklu dönüş için kullandığı taşıyıcı `KeyValuePair`:

```
combatants:  Unit#1 ──► Combatant#1
             Unit#2 ──► Combatant#2

foreach döngüsünde her adım:
   pair.Key   = Unit#1        (anahtar — aradığın şey)
   pair.Value = Combatant#1   (değer  — bulduğun şey)
```

İki not:

- **Sıra garantisi yoktur.** `Dictionary` ekleme sırasını korumaz ve korumayı
  vaat etmez. Sıraya ihtiyacın varsa `List` ya da dizi gerekir — projede sıra
  `TurnState.order` dizisinde tutuluyor, sözlükte değil.
- **Gezerken değiştirilemez.** `foreach` sırasında `Add`/`Remove` çağırmak
  `InvalidOperationException` atar. `Battle.RemoveReadyForCleanup`'ın neden iki
  geçiş yaptığı (önce topla, sonra sil) tam olarak bu yüzden.

---

## Kural: bir koleksiyonu dışarı açarken

```
① Çağıranın değiştirmesi SORUN mu?
      hayır → düz List<T>/dizi döndür, iş biter
      evet  → ②

② Çağıran senin İÇ koleksiyonunu mu görecek?
      hayır (kopya veriyorsun) → kopya zaten korur, sarmalayıcı şart değil
      evet                     → ③

③ Alttaki koleksiyonu başka biri tutuyor mu?
      evet → ██ AsReadOnly SAHTE GÜVENLİK ██ — önce kopyala
      hayır → Array.AsReadOnly + IReadOnlyList<T> gerçek kilit olur
```

`TurnState` ③'te "hayır" diyebiliyor çünkü kurucuda **kopyalıyor**. Kopyalamadan
sarmalasaydı, çağıran kendi listesini değiştirerek savaşın sırasını bozardı.

---

## Yanlış hatırlanan üç şey

**"`IReadOnlyList` demek immutable demek."** Değil. "Bu referansı tutan
değiştiremez" demek. Alttaki diziyi tutan hâlâ değiştirebilir.

**"`object Current` Unity'nin bir şeyi."** Değil. `System.Object`, .NET'in kök
tipi, 2002'den kalma bir imza. `UnityEngine.Object` bambaşka bir sınıf.

**"`this[int index]` bu nesneyi döndürüyor."** Hayır. `this` orada üyenin adı
yerine geçiyor — köşeli parantezle çağrılan üyeyi bildiriyor.

---

## Kaçış yolu: `IReadOnlyList` yerine ne olurdu

```
düz Team[] döndür       → çağıran order[0] = Team.None yazabilir, sıra bozulur
List<Team> döndür       → Add/Clear açık, aynı sorun daha görünür hâli
IEnumerable<Team>       → yazma kapalı AMA indeksleme de kapalı;
                          Current => order[index] yazılamaz hâle gelir
kopya döndür (her çağrı)→ güvenli ama her okumada tahsis; Tick sıcak yolunda
                          çöp üretir
```

`IReadOnlyList` + kurucuda tek kopya, dördünün ortasını tutuyor: yazma kapalı,
indeksleme açık, tahsis bir kez.

---

Kodda **karar**, burada **ödünç alınan tipin sözleşmesi**. İkisi çelişirse kod
kazanır — orası çalışan metin, burası anlatı.
