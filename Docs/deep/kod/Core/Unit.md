# Unit

> **Kaynak:** `Assets/Game/Core/Unit.cs`
> **Ad alanı:** `GridStrategy.Core` · **Assembly:** `GridStrategy.Core` (`references: []`)
> **Rol:** Varlık (Entity) — kimliği var, hafızası yok, karar vermez

Tahtada yer kaplayan, kimliği olan şey. **"Birim" değil** — bir asker de bir
baraka da budur, çünkü strateji oyunlarında binalar da birimdir: bir Barracks
seçilir, canı vardır, hedeflenir, hücre kaplar.

Taşıdığı tek şey kimliğin kendisidir. Nerede durduğunu (tahtanın işi), canını,
tarafını, durumunu, nasıl çizildiğini bilmez; savaşçı mı yapı mı sorusunun cevabı
bu tipte değil, ona hangi parçanın eşlendiğinde yaşar (`GridStrategy.Combat`'taki
`Combatant` ve `Structure`).

| Üye | Karar | Detay |
|---|---|---|
| `Unit` (tip) | tahtanın anahtarını varlığın türü değil, tahtanın sorduğu soru belirler | [↓](#unit-tip) |
| `Unit(string name)` | tek parametre ad; "ne yapabileceği" bu tipte yaşamaz | [↓](#unitstring-name) |
| `Name` | insan için etiket; sözlüklerin anahtarı referans eşitliğidir | [↓](#name) |

**İlgili anlatılar:** [03-tahta sahipliği](../../konular/03-tahta-sahipligi.md)

---

## Unit (tip)

### TEK KİMLİK TİPİ: TAHTANIN ANAHTARI TÜRE GÖRE ÇOĞALMAZ

### HARİTA: bir kimlik tipi = bir tahta

`UnitGrid`'in içi tek bir `Unit[,] cells` dizisidir. Dizinin ELEMAN TİPİ, tahtaya
girebilen şeylerin kümesini birebir belirler: bir `Unit[,]` hücresine
`StructureId` yazılamaz. Yani ikinci bir kimlik tipi bir tercih değil, ikinci bir
diziyi ZORUNLU kılan bir sonuçtur.

```
SEÇİLEN — tek kimlik, tek dizi, tek doluluk sorusu
  asker ──┐
          ├──► ╔═══════════════╗   (1,2) dolu mu?
  baraka ─┘    ║ Unit[,] cells ║ ──────► TEK cevap
               ╚═══════════════╝

REDDEDILEN — iki kimlik, iki dizi, iki doluluk sorusu
  asker  ──► ╔═══════════════╗   (1,2) dolu mu? ──► "hayır"
             ║ Unit[,] cells ║
             ╚═══════════════╝
  baraka ──► ╔════════════════════════╗
             ║ StructureId[,] cells2  ║ (1,2) dolu mu? ─► "evet"
             ╚════════════════════════╝
                      ◄── AYRIŞMA NOKTASI ──►
  Aynı hücre için iki cevap AYNI ANDA doğrudur ve ikisini uzlaştıran bir
  tip yoktur. Çağıranların biri bir soruyu unuttuğu gün aynı hücrede iki
  şey durur.
```

### REDDEDILEN

`Unit` tipinin bugünkü hâli yerine (bu tipin adı kapsamına göre değişir ve
yapılar için ikinci bir kimlik tipi doğar):

```csharp
public sealed class BoardPiece { }     // Unit yeniden adlandırılır
public sealed class StructureId { }    // yapılar için ayrı kimlik
```

**KIRILAN:** ikinci kimlik tipi ikinci bir tahtayı ZORUNLU kılar.

```
UnitGrid StructureId tutamaz -> ikinci bir dizi doğar
"bu hücre dolu mu" iki kez   -> biri unutulur
aynı hücrede iki şey durur   -> hiçbir uyarı yok
derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
```

**KAZANIRDI:** yapılar tahtada YER KAPLAMASAYDI — arka planda işleyen bir
araştırma binası, ızgaraya oturmayan bir küresel yükseltme.

**KARSILASTIRMA:**

| Seçenek | Anahtar | Sonuç |
|---|---|---|
| `Unit` (bugün) | yer kaplamak | tek tahta, tek doluluk sorusu |
| `BoardPiece` | aynı tip, yeni ad | yalnızca okunurluk kazanır, beş assembly'de ad değişir |
| `Unit` + `StructureId` | tür başına kimlik | iki tahta, iki doluluk sorusu |

### KAPSAM: kural "tek tip yeter" DEĞİL, "anahtarı soru seçer"

Bu blok her kavramı tek tipe toplamayı savunmuyor; yalnızca TAHTANIN
ANAHTARININ tek olmasını savunuyor. Testi kümeyle yap:

```
tahtanın sorduğu soru : { burada bir şey var mı }
Unit'in taşıdığı      : { kimlik, ad }
kesişim               : { kimlik }        ← anahtar tam olarak bu
```

KARŞI ÖRNEK, uydurma değil, bu tipin kendi özetinde adı geçiyor: Combat'taki
`Combatant` ve `Structure` AYRI tiplerdir ve öyle kalmaları doğrudur — tahta
"kimin ne kadar canı var" diye HİÇ sormaz. Onlar `Unit`'e EŞLENİR, `Unit`'in
içine girmez.

Yeni bir kavram eklerken sorulacak tek soru şu: tahta bunu hücre doluluğu olarak
soruyor mu? Sormuyorsa yeni bir kimlik tipi değil, `Unit`'e eşlenen yeni bir yan
tablo doğmalıdır.

### İŞ BÖLÜMÜ: kimlik ile yan tablolar ÖRTÜŞMEZ, BÖLÜŞÜR

Bir birimin hakkında bilinen her şey iki ayrı yerde yaşıyor:

```
NEREDE durduğu       ► UnitGrid'in dizisi  (anahtar = hücre)
NE OLDUĞU/kaç canı   ► Unit ile anahtarlanan yan tablolar:
                       Battle.combatants, Battle.structures,
                       Battle.stateForwarders, BoardAdapter.unitViews
```

Bu tip silinirse dört sözlüğün de anahtarı kalmaz ve konum ile nitelik
arasındaki bağ kopar. Yan tablolar silinirse tahta ayakta kalır ama tuttuğu şeyin
ne olduğu sorulamaz hâle gelir. İkisi yedek değil, bölüşüm.

### ANAHTARI TUTAN ŞEY BİR MODIFIER DEĞİL

`sealed` kimlik ÜRETMEZ; yalnızca türetmeyi keser. `Name` de anahtar DEĞİLDİR —
insan için bir etikettir. Sözlükleri ayakta tutan dil-seviyesi mekanizma REFERANS
EŞİTLİĞİdir: bu tip ne `Equals` ne de `GetHashCode` geçersiz kılıyor, dolayısıyla
`Dictionary<Unit, ...>` varsayılan referans kimliğini kullanır.

### GARANTİ NEREDE BİTER

Bu değişmezi tutan şey bu dosyada bir kodun VARLIĞI değil, YOKLUĞUdur. Bir gün
buraya ada göre değer eşitliği (`Equals`/`GetHashCode`) eklenirse "Piyade" adlı
iki ayrı birim üç ayrı assembly'deki sözlüklerde tek bir girdiye çöker; derleyici
bunu söylemez, çünkü imza değişmez.

**TEK CUMLE:** Kimlik tipini varlığın TÜRÜ değil, tahtanın SORDUĞU soru belirler;
tahta "burada ne var" diye sorduğu için tek tip yeter.

---

## Unit(string name)

Tek parametre: ad. Menzil, can, taraf ve durum bilerek YOK.

Ayırt edici ölçüt yukarıdaki kümenin ta kendisi: bir alan KİM OLDUĞUNU mu
söylüyor, NE YAPABİLECEĞİNİ mi? `Name` birincidir ve burada durur; ikinciler bu
tipe hiç girmedi. Hareket menzilinin neden burada olmadığının uzun gerekçesi
[`MoveAction.md`](MoveAction.md)'de, "menzil kimliğin yanında değil kuralın
yanında yaşar" başlığı altında yazılı.

---

## Name

Get-only otomatik property; kurucuda konur, bir daha yazılmaz.

Ad, insanın okuması için. Sözlüklerin anahtarı O DEĞİLDİR — anahtarı tutan şeyin
ne olduğu ve garantinin nerede bittiği yukarıda, [`Unit` (tip)](#unit-tip)
bölümünün "ANAHTARI TUTAN ŞEY BİR MODIFIER DEĞİL" başlığı altında yazılı.
