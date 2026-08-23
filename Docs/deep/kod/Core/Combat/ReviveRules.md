# ReviveRules

> **Kaynak:** `Assets/Game/Core/Combat/ReviveRules.cs`
> **Ad alanı:** `GridStrategy.Combat` · **Assembly:** `GridStrategy.Combat` (`noEngineReferences: true`)
> **Rol:** Kural (Policy) — kimliği yok, hafızası yok, **uygunluk** söyler

"Bu birim başkasını diriltebilir mi?" sorusunun tek sahibi. Ne diriltir ne can
yazar.

Ailenin üçüncü ve son üyesi. Üçü birlikte EYLEYENİN durumunu sahipleniyor, her biri
tek bir yetenek için:

- `MovementRules` — kim yürür
- `AttackRules` — kim vurur
- `ReviveRules` — kim kaldırır

Hedefin durumu bunların hiçbirinde değil; o `TargetingRules`'ın işi ve orada da iki
ayrı soru olarak duruyor (`CanBeAttacked` ile `CanBeRevived`). Yani matris iki
eksenli: **EYLEYEN × HEDEF**, ve her hücrenin kendi sahibi var.

**Neyi BİLMEZ:** hedefin diriltilebilir olup olmadığını
(`TargetingRules.CanBeRevived(UnitState)`'in işi), diriltme menzilini (bugün
sahipsiz — `BattleActions.Revive` saldırı menzilini ödünç alıyor ve o taviz orada
yazılı), sıranın kimde olduğunu (`TurnRules`'ın işi, bir üst katmanda).

| Üye | Karar | Detay |
|---|---|---|
| `CanRevive(UnitState)` | tek koşul: eyleyenin kendisi ayakta olmalı | [↓](#canreviveunitstate-reviverstate) |

**İlgili anlatılar:** [05-yaşam döngüsü](../../../konular/05-yasam-dongusu.md) ·
[04-karar sırası](../../../konular/04-karar-sirasi.md)

---

## Bu tipin doğuş hikâyesi

Bu tip doğana kadar **düşmüş bir birim yerdeki arkadaşını ayağa
kaldırabiliyordu.** Boşluk gizlenmemişti: `BattleActions.Revive` onu adı konmuş bir
EŞİK olarak taşıyordu ve sabitleyen test
(`Revive_DownedReviver_StillRevives_BecauseNoRuleOwnsReviverState` — **SİLİNDİ**,
aramaya kalkma; o test artık yok ve olmaması doğrudur) kural yazıldığı gün kırmızıya
dönmek üzere yazılmıştı. Bugün o gün; test silindi ve yerine kararı koruyan
`Revive_DownedReviver_IsRejected` geldi.

### ÖLÇÜLMÜŞ VE DÜRÜSTÇE YAZILIYOR

Bugün üç kural da aynı satırı taşıyor (`state == Alive`) ve birini diğerinden
türetmek üç durumda da doğru cevap verir; **bu bir tesadüftür, bir tasarım değil.**
Üçünü BİRDEN kayda geçiren tek test bu tipin kendi dosyasındadır —
`ReviveRulesTests.ThreeActorRules_StillAgree_WhichIsWhyTheyMustStaySeparate`;
`AttackRules` tarafındaki karakterizasyon testi yalnızca İKİLİ sürümü taşır. Bedel
bugün üç dosya, kazanç ayrıldıkları gün hiçbir çağıranın değişmemesi.

Ayrılacakları gün de tahmin değil, oyunun kendi diliyle adlandırılabilir: **yaralı
bir sıhhiyeci vuramadığı hâlde arkadaşını kaldırabilmelidir.** O gün `ReviveRules`
ile `AttackRules` ayrışır ve türetme yazmış olan proje bunu HİÇBİR TEST KIRILMADAN
kaçırır. Türetmenin neden reddedildiği
[MovementRules.md](MovementRules.md#canmoveunitstate-state)'de.

---

## CanRevive(UnitState reviverState)

**EYLEYEN İLE HEDEF İKİ AYRI EKSENDİR.**

Diriltebilmenin tek koşulu: eyleyenin kendisi ayakta olmalı.

**Beyaz liste** — `== Alive`, `!= Downed && != Dead` değil. Gerekçe
`MovementRules`'ta yazılı ve burada tekrar edilmiyor: kara liste, yeni bir
`UnitState` değerini varsayılan olarak YETKİLİ kılar ve bunu hiçbir derleme hatası
göstermez ([MovementRules.md](MovementRules.md#canmoveunitstate-state)).

### HARİTA: matris ve reddedilen imzanın çökerttiği hücre

Sorulan her uygunluk sorusu iki eksenden birine düşer; bu dosya EYLEYEN sütununun
tek bir hücresini tutuyor.

```
                      EYLEYEN ekseni        HEDEF ekseni
  ─────────────────────────────────────────────────────────────────
  yürümek             MovementRules.CanMove       (hedef yok)
  vurmak              AttackRules.CanAttack       TargetingRules
                                                  .CanBeAttacked
  kaldırmak           ReviveRules.CanRevive       TargetingRules
                      ◄── ██ BU METOT ██          .CanBeRevived

  REDDEDILEN imza iki hücreyi TEK metoda çökertir:
      CanRevive( reviverState , targetState )
                 └─EYLEYEN──┘   └──HEDEF──┘
                                 ◄── ██ ÇÖKME NOKTASI ██
  ikinci satır (`targetState == Downed`) TargetingRules.CanBeRevived
  ile BİREBİR aynı cümledir; kopya olduğu için de sessizce eskir.
```

### KAPSAM: iki parametre yasak DEĞİL

Ayırt edici soru: **iki parametre AYNI eksenden mi geliyor?**

Karşı örnek aynı ad alanında, `TargetingRules`:

```csharp
public static bool CanBeRevived(UnitState state,
                                Team reviverTeam, Team targetTeam)
```

Bu metot ÜÇ parametre alır ve doğrudur: üçü de HEDEF ekseninin sorusunu tamamlıyor
("bu hedef, bu diriltene göre uygun mu"). Orada taraf karşılaştırması eksen
değiştirmiyor, aynı hücrenin içinde kalıyor. Burada reddedilen şey parametre SAYISI
değil, ikinci parametrenin başka bir hücrenin cevabını taşıması.

### İŞ BÖLÜMÜ: iki eksen örtüşmez, bölüşür

```
ReviveRules.CanRevive          ► eyleyen ayakta mı
TargetingRules.CanBeRevived    ► hedef düşmüş mü
ikisini SORAN                  ► BattleActions.Revive
```

Bu metot silinirse düşmüş bir birim yerdeki arkadaşını kaldırır — tipin doğuş sebebi
tam olarak o boşluktu. `TargetingRules.CanBeRevived` silinirse ayakta bir birim,
ayakta olan başka bir birimi "diriltir". İkisi aynı çağrının iki ucunu tutuyor ve
hiçbiri diğerinin cevabını üretemez.

### GARANTİ NEREDE BİTER

Bu tip yalnız DURUM sorar; menzil, mesafe ve sıra buraya hiç gelmez. Diriltme
menzili bugün **sahipsiz** — `BattleActions.Revive` saldırı menzilini ödünç alıyor
ve o taviz orada yazılı. Yani "diriltebilir" cevabı tek başına "diriltmesine izin
var" demek değildir; sözleşme durum duvarında biter.

### REDDEDILEN

Düşmüş birim kendini diriltemesin diye hedef de sorulur:

```csharp
public static bool CanRevive(UnitState reviverState, UnitState targetState)
{
    if (reviverState != UnitState.Alive) { return false; }
    return targetState == UnitState.Downed;
}
```

**KIRILAN:** ikinci satır `TargetingRules.CanBeRevived`'ın birebir kopyasıdır ve o
kural HEDEF eksenine ait; matrisin iki hücresi tek metoda çöker.

```
diriltilebilir durumlar kümesi değişir -> orası güncellenir,
burası kalır ve akış hedef kuralı EVET derken reddeder
derleyici: hiçbir şey der  ·  test: hiçbiri kırmızıya dönmez
```

**KAZANIRDI:** eyleyen ile hedefin BİRLİKTE anlam kazandığı bir kural olsaydı —
"kendini dirilteme". O gün sorulan durum değil KİMLİK olurdu.

**TEK CUMLE:** Eyleyenin durumu ile hedefin durumu iki ayrı eksendir; tek metot
ikisini birden sahiplenemez.
