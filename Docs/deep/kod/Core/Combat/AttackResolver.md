# AttackResolver

> **Kaynak:** `Assets/Game/Core/Combat/AttackResolver.cs`
> **Ad alanı:** `GridStrategy.Combat` · **Assembly:** `GridStrategy.Combat` (`noEngineReferences: true`)
> **Rol:** Kural (Policy) — kimliği yok, hafızası yok, yalnızca ULAŞABİLİRLİĞİ hesaplar

Saldırının ULAŞABİLİRLİK kuralının sahibi — "verilen uzaklık menzile giriyor
mu". "Kim vurur" `AttackRules`'ın, "kime vurulur" `TargetingRules`'ın.
`DamageRules` gibi hiçbir durum tutmaz: sayı ve tanım alır, cevap döndürür.

**Mesafeyi kendi hesaplamaz** — dışarıdan hazır alır. Sebebi bilinçli: "iki
hücre arası uzaklık nedir" ayrı bir oyun kuralıdır (Manhattan mı, Chebyshev mi,
engeller sayılır mı). O kural buraya girseydi, menzil mantığını test etmek için
önce bir tahta kurmak gerekirdi.

Neyi **bilmez**: hedefin asker mi baraka mı olduğunu, ölü olup olmadığını,
sıranın kimde olduğunu. Bunlar hedef seçiminin işi. Buraya bir "hedef uygun mu"
kontrolü eklemek, `Health`'e "hedef baraka mı" sormakla aynı hatadır: kendisine
sorulmayan bir soruyu cevaplamak.

| Üye | Karar | Detay |
|---|---|---|
| `IsWithinRange(int, AttackProfile)` | mesafe dışarıdan ÖLÇÜM olarak gelir, menzil içeride TANIM olarak durur | [↓](#iswithinrangeint-distance-attackprofile-profile) |

**İlgili anlatılar:** [02-assembly duvarı](../../../konular/02-assembly-duvari.md)

---

## IsWithinRange(int distance, AttackProfile profile)

Verilen uzaklık bu saldırının menzili içinde mi? Yalnızca **ulaşabilirlik**
söyler; vurmanın doğru olup olmadığını değil.

### HARİTA: tahta hangi assembly'de, bu kural hangisinde

```
GridStrategy.Core      references: []   noEngine: TRUE
  UnitGrid, GridDistance   ◄── "iki hücre arası uzaklık"
                               kuralı BURADA yaşıyor
      ▲
      │ >> DUVAR << Combat'ın `references` listesi BOŞ:
      │ bu ok DERLENMEZ — kurulamaz, dolayısıyla unutulamaz
      │
GridStrategy.Combat    references: []   noEngine: TRUE
  AttackResolver, AttackProfile, TargetingRules, ...
  IsWithinRange(distance, profile)
      ▲            ▲
      │            └── profile.Range ► TANIM, içeride üretilir
      └── distance ► ÖLÇÜM, dışarıdan hazır gelir

GridStrategy.Battle ve GridStrategy.Unity İKİSİNİ birden referans eder;
mesafeyi ölçüp buraya getiren katman orası.
```

### KAPSAM: "her sayı dışarıdan gelir" diye bir kural YOK

Ayıraç, sayının bir **ölçüm** mü yoksa bir **tanım** mı olduğudur:

```
ölçüm (tahtaya bağlı)     ► parametre (distance)
tanım (tahtadan bağımsız) ► içeride   (profile.Range)
```

Karşı örnek aynı satırın ikinci parametresi: `profile.Range` bu assembly'nin
**içinde** üretilir ve dışarıdan istenmiyor — çünkü menzilin kaç hücre olduğu
bir tanım kararıdır, tahtanın geometrisine hiç bakmaz. İki sayı, aynı metot,
zıt karar.

### İŞ BÖLÜMÜ: eşik ile ölçüm

```
profile.Range ► EŞİĞİ verir  (kaç hücreye ulaşır)
distance      ► ÖLÇÜMÜ verir (kaç hücre uzakta)
```

Karşılaştırma ancak ikisi birden olunca anlam kazanır: `distance` parametresi
kaldırılıp koordinatlar alınsaydı bu tip tahtayı tanımak zorunda kalır ve
menzili sınamak için önce tahta kurmak gerekirdi; `Range` dışarıdan alınsaydı
profil kavramı boşalır ve eşik her çağıranın içinde yeniden doğardı.

### `noEngineReferences` BU DUVARI KURMUYOR

Okuyucunun yanlış kredi vereceği yer burası: o bayrak yalnızca `UnityEngine`'i
keser. `UnitGrid` düz bir C# sınıfıdır ve bayrak açıkken de pekâlâ referans
edilebilirdi. Tahtayı dışarıda tutan şey `"references": []` —
`GridStrategy.Core`'a hiç ok açılmamış olması.

**GARANTİ NEREDE BİTER:** orada. Aynı assembly'ye yarın bir tahta tipi
eklenirse hiçbir bayrak uyarmaz.

### REDDEDILEN

```csharp
public static bool IsWithinRange(int ax, int ay, int bx, int by, AttackProfile profile)
    => Math.Abs(ax - bx) + Math.Abs(ay - by) <= profile.Range;
```

**KIRILAN:** mesafe ölçümü kuralın İÇİNE girer ve Manhattan/Chebyshev kararı
burada donar.

```
menzili sınamak için önce tahta kurmak gerekir
engel ya da yükseklik kuralı geldiği gün iki dosya birden değişir
derleyici: hiçbir şey der  ·  test: AttackResolverTests tahtaya bağlanır
```

**KAZANIRDI:** oyunda tek bir mesafe metriği olsaydı ve hiç değişmeyecek
olsaydı — her çağıranın aynı formülü tekrar yazması biterdi.

**TEK CUMLE:** "İki hücre arası uzaklık nedir" ayrı bir kuraldır; menzil kuralı
onu SORAR, hesaplamaz.

### `distance == 0` bilerek GEÇERLİ

Aynı hücre bilerek geçerli sayılıyor. "Kendine saldırılır mı" bir **hedef
seçimi** kuralıdır; menzil kuralı yalnızca mesafeyi ölçer. Burada
engelleseydik, ileride "kendi kendini iyileştirme" gibi bir yetenek geldiğinde
bu satırı geri almak gerekirdi.

**Alternatif:** `return distance > 0 && distance <= profile.Range;` — aynı hücre
reddedilirdi. Seçilmedi: sebebi yukarıda; "kendine uygulanır mı" hedef
seçiminin sorusudur, menzil kuralının değil.
