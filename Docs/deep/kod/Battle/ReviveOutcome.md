# ReviveOutcome

> **Kaynak:** `Assets/Game/Battle/ReviveOutcome.cs`
> **Ad alanı:** `GridStrategy.Battle` · **Assembly:** `GridStrategy.Battle`
> **Rol:** Tanım (Profile) — kimliği yok, hafızası yok, karar vermez adlandırır

Bir diriltme **denemesinin** sonucu. "Deneme" kelimesi kasıtlı: reddedilen bir
diriltme de bir sonuçtur ve çağıranın onu ayırt etmesi gerekir.

**NEDEN BU KATMANDA, `GridStrategy.Combat`'ta DEĞİL:** diriltmenin AKIŞI burada
yaşıyor. `Combatant.TryRevive` kendi payına düşeni bir `bool` ile cevaplıyor ve
doğru cevap o — o tip menzili, sırayı ve tarafı GÖRMEZ. Menzili görebilen tek yer
tahtayı ve savaşı birlikte tanıyan katmandır; sonuç tipi de üreticisiyle aynı
yerde yaşar. Aynı gerekçe `PlacementOutcome` için de geçerli.

**Neden `bool` değil:** "dirildi mi" dört ayrı soruyu tek cevaba sıkıştırırdı ve
çağıran dördüne farklı tepki verir — sırası değilse sessiz kalınır, geçersiz
hedefte bir uyarı sesi çalar, menzil dışında yapay zekâ "önce yaklaş" der,
diriltmede bir animasyon ve muhtemelen bir ses oynar. Bu ayrım `bool` ile
yazılsaydı çağıranın içinde ikinci bir kontrol olarak yeniden doğardı — ve o
kontrol akışın kurallarını kopyalardı.

| Üye | Karar | Detay |
|---|---|---|
| `RejectedInvalidTarget` | sıfırıncı değer bilerek bir ret | [↓](#rejectedinvalidtarget) |
| `RejectedOutOfRange` | mesafeyi suçlayan tek değer | [↓](#rejectedoutofrange) |
| `RejectedActorCannotAct` | adı ödünç alındı; iki sebebi birden taşır | [↓](#rejectedactorcannotact) |
| `Revived` | bu enum saldırınınkiyle birleşmedi | [↓](#revived) |

**İlgili anlatılar:** [06-sonuç enumları](../../konular/06-sonuc-enumlari.md) ·
[04-karar sırası](../../konular/04-karar-sirasi.md)

---

## RejectedInvalidTarget

Sıfırıncı değer **bilerek bir RET**. Gerekçesi `MoveOutcome` ve `AttackOutcome`
tiplerinde yazılı; burada tekrarlanmıyor, atıfta bulunuluyor. Kısacası:
`default(ReviveOutcome)` "dirildi" demek olsaydı, atanması unutulan her alan
sessizce bir başarı gibi okunurdu.

Değerin kendisi: hedef diriltilemez — ayakta, kalıcı ölü, karşı takımda ya da
taraflardan biri tarafsız. Kuralın sahibi `TargetingRules.CanBeRevived`.

---

## RejectedOutOfRange

Hedef diriltilebilirdi ama ulaşılamadı. Üç ret arasında **mesafeyi** suçlayan tek
değer; yapay zekânın "önce yaklaş" diyebilmesi tam olarak bu ayrıma bağlı.

---

## RejectedActorCannotAct

Üçüncü değer, ve adı **iki enum'dan ödünç alındı** —
`AttackOutcome.RejectedActorCannotAct` ve `MoveOutcome.RejectedActorCannotAct`.
Anlamı üçünde de tek cümle: eylemi yapan taraf şu an eylem yapamaz. Farklı bir ad
seçmek, çağıranı aynı cevabı üçüncü kez öğrenmek zorunda bırakırdı.

**İKİ SEBEBİ BİRDEN TAŞIYOR** ve ikisini de akış soruyor: sıra
(`TurnRules.CanAct`) ve diriltenin kendi durumu (`ReviveRules.CanRevive`). İkinci
kuralın kendi tipi var, ödünç ALINMADI: `AttackRules.CanAttack`'in adı yalan
söylerdi — diriltmek saldırmak değildir — ve bir kuralı başka bir kuraldan
türetmenin reddi hem `MovementRules`'ta hem `AttackRules`'ta yazılı. Ayrımın
tamamı `BattleActions.Revive`'ın yanında.

---

## Revived

Hedef ayağa kalktı. Bu değer var diye enum'un kendisi de var: saldırı sözlüğünde
karşılığı yok, ve karşılığının olmaması iki tipin birleşmemesinin sebebi.

### HARİTA: iki sonuç tipinin SÖZLÜKLERİ

```
  AttackOutcome                    ReviveOutcome
  ──────────────────────────       ──────────────────────────
  RejectedInvalidTarget    ═════   RejectedInvalidTarget
  RejectedOutOfRange       ═════   RejectedOutOfRange
  RejectedActorCannotAct   ═════   RejectedActorCannotAct
  Hit                        ✗     (karşılığı YOK)
  HitAndDowned               ✗     (karşılığı YOK)
  HitAndDestroyed            ✗     (karşılığı YOK)
  (karşılığı YOK)            ✗     Revived
  ──────────────────────────       ──────────────────────────
  kesişim: 3        birleşim: 7
  >> BİREBİR DEĞİL — S-13'ün paylaşma şartı burada düşüyor <<
```

Birleştirilseydi her çağıran, kendi eylemi için ASLA dönmeyecek değerleri de
görürdü: diriltme çağıranı üç saldırı değerini, saldırı çağıranı `Revived`'ı.
İkincisi daha sessiz — eksik enum değeri için `switch` **deyimi** uyarı bile
üretmez.

### KAPSAM: saldırıdan ÖDÜNÇ ALMAK yasak değil

Ölçüt: ödünç alınan tek bir AD mı, yoksa sözlüğün tamamı mı?

**KARŞI ÖRNEK** aynı dosyada, bu enum'un üçüncü değeri
[`RejectedActorCannotAct`](#rejectedactorcannotact): adı doğrudan `AttackOutcome`
ile `MoveOutcome`'dan ödünç alındı ve doğrusu buydu — o adın cümlesi üçünde de
birebir aynı ("eylemi yapan taraf şu an eylem yapamaz") ve farklı bir ad seçmek
çağırana aynı şeyi üçüncü kez öğretirdi. Tek ad paylaşmak için o adın cümlesinin
aynı olması yeter; **TİP** paylaşmak için bütün adların aynı olması gerekir.

### İŞ BÖLÜMÜ: üç ret, üç farklı çağıran tepkisi

```
  RejectedInvalidTarget   HEDEF   ► uyarı sesi
  RejectedOutOfRange      MESAFE  ► yapay zekâ "önce yaklaş"
  RejectedActorCannotAct  EYLEYEN ► sessiz kal
```

Üçü ÖRTÜŞMÜYOR çünkü üçü farklı bir ÖZNEYİ suçluyor ve çağıran her birine başka
türlü tepki veriyor. İkisi birleştirilseydi ayrım kaybolmaz, çağıranın İÇİNDE
ikinci bir kontrol olarak yeniden doğardı — ve o kontrol akışın kurallarını
kopyalardı. Üçüncüsü tek başına İKİ sebebi taşıyor ve bu bilinçli bir istisna:
sıra ile diriltenin durumu, çağıran açısından aynı cevaptır — "şimdi olmaz".

### REDDEDILEN

Bu enum hiç doğmaz, diriltme `AttackOutcome`'u paylaşır:

```csharp
AttackOutcome outcome = BattleActions.Revive(battle, reviver, target);
// ve "Revived" için AttackOutcome'a altıncı bir değer eklenir
```

**KIRILAN:** paylaşmanın şartı (S-13) tutmuyor — ret sebepleri ve başarı cümlesi
BİREBİR aynı değil.

```
Hit, HitAndDowned ve HitAndDestroyed diriltmede karşılıksız
  -> her çağıran "bu bana asla dönmez" diye üç dal yazar
  -> ters yönde eklenen Revived saldırı switch'inde işlenmeden kalır
derleyici: switch DEYİMİnde uyarı bile üretmez  ·  test: yeşil
```

**KAZANIRDI:** diriltme bir SALDIRI çeşidi olsaydı — negatif hasar veren bir
yetenek, ya da "hedefe bir şey uygula" diye tek bir akışa indirgenmiş bir yetenek
sistemi; o gün S-13 birleşmeyi emrederdi.

**TEK CUMLE:** İki sonuç tipi ancak sözlükleri birebir aynıysa birleşir; değilse
birleşme her çağırana asla dönmeyecek dallar yazdırır.
