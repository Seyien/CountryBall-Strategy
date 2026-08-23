# Dil ve BCL — ödünç alınanlar

Bu ağaç projenin kendi kararlarını değil, **ödünç aldığı** şeyleri anlatıyor:
C#'ın dil özellikleri ve .NET'in hazır tipleri. Onların kodunu biz yazmadık; ama
neyi vaat ettiklerini bilmeden kendi kararlarımızı okuyamayız.

## Neden ayrı bir ağaç

`kod/` ağacı **tip başına** bölünür, çünkü her tipin kendi kararları var.
Burada bölüm **kavram başına**, çünkü aynı dil özelliği onlarca dosyada geçiyor:
`readonly` dokuz dosyada, `nameof` on sekiz dosyada. Tip bazlı bir yerleşim aynı
açıklamayı onlarca kez tekrar ederdi.

| Ağaç | Bölünme | Soru |
|---|---|---|
| [`kod/`](../kod/README.md) | tip başına | "bu üye neden böyle" |
| [`konular/`](../konular/) | mekanizma başına | "bu akış nasıl çalışıyor" |
| `dil/` (burası) | kavram başına | "bu dil özelliği ne vaat ediyor" |

## Kural

Bir ödünç tip ya da dil özelliği, bir dosyada **ilk** göründüğü yerde tek satır
borçludur: ne aldığını ve ne almadığını söyleyen bir satır, artı buraya bir
işaretçi. İkinci kullanım borçlu değil — tekrar gürültüdür.

Kuralın kendisi `unity-expert-code-quality` skill'inde:
`references/unity-csharp-quality-flow.archive` → *Borrowed Types Owe a Line*.

## Dosyalar

| # | Konu | Kapsadığı |
|---|---|---|
| [01](01-degismezlik-anahtar-kelimeleri.md) | **Değişmezlik anahtar kelimeleri** — kelime OKA bakar, okun UCUNA değil | `readonly` · `const` · `static readonly` · `{ get; }` · `sealed` |
| [02](02-koleksiyonlar-ve-salt-okunur.md) | **Koleksiyonlar ve "salt okunur"un kapsamı** — vaat edilen ve edilmeyen | `IReadOnlyList<T>` · `Array.AsReadOnly` · indeksleyici · `IEnumerator` · `Dictionary` |
| [03](03-hata-bildirme-ve-dogrulama.md) | **Hata bildirme** — kime söylüyorsun, hangi tiple | `nameof` · `ArgumentNullException` · `ArgumentOutOfRangeException` · `ArgumentException` · `Math.Max/Min` |
| [04](04-delege-olay-ve-kapanis.md) | **Fonksiyonu değişkende tutmak** | `Action<T>` · `event` · `?.Invoke` · kapanış kimliği · metot grubu |
| [05](05-deger-referans-ve-kimlik.md) | **"Aynı" olmak ne demek** | değer/referans · `ReferenceEquals` · `enum` · `out` · `=>` · `switch` · `%` |

## Bu ağacın yakaladığı üç yanlış model

Belgeler yazılırken kaynakta doğrulanan, okuyucuyu yanlış yöne götürecek üç şey:

```
System.Object ≠ UnityEngine.Object
   IEnumerator.Current'taki `object` C#'ın kök tipi. Motor olanı varsayan
   okuyucu foreach'in sahne nesneleriyle ilgisi olduğunu sanır.

IReadOnlyList ≠ immutable
   "Bu referansı tutan değiştiremez" demek. Alttaki diziyi tutan hâlâ
   değiştirebilir — projenin kendi "ikinci yazar" problemiyle aynı şey.

const, assembly sınırında KOPYALANIR
   Kullanan derleme birimi yeniden derlenmezse eski değeri taşımaya devam
   eder. Bu projede TurnRulesTests bir const'u karşılaştırıyor ve IL'de
   `Assert.That(1, Is.EqualTo(1))`'e düşüyor.
```

## Otorite

Kod kazanır. Belge çelişirse belge bayattır — ama bu ağaçta ikinci bir otorite
daha var: dil ve BCL davranışı için **son söz derleyicinin ve çalışma
zamanının**. Her iddia derleyici hata koduyla (CS0134, CS0177, CS0191, CS0200,
CS0509, CS8509) ya da koşturulabilir bir deneyle bağlanmıştır; bağlanmamış bir
iddia varsa o bir kusurdur, üslup değil.
