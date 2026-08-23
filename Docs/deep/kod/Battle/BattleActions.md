# BattleActions

> **Kaynak:** `Assets/Game/Battle/BattleActions.cs`
> **Ad alanı:** `GridStrategy.Battle` · **Assembly:** `GridStrategy.Battle`
> (`noEngineReferences: true`)
> **Rol:** Kural (Policy) — kimliği yok, hafızası yok; AKIŞI yürütür, kural yazmaz

Tahta ile savaşı birleştiren akışın tek sahibi.

Bu dosyanın var olma sebebi tek satır: `AttackAction` mesafeyi DIŞARIDAN alıyordu
ve o mesafeyi üretecek kimse yoktu. Üreteni `GridDistance`, kimin nerede
durduğunu `Battle`, saldırının çözümünü `AttackAction` biliyor — üçü de birbirini
TANIMAZ. Onları bir sıraya dizen tek yer burasıdır.

**SIRA KURALINI ARTIK SORUYOR.** `TurnState` ile `TurnRules` yazılmıştı ama
üretimde tek bir çağıranları yoktu; sıra sistemi vardı ve hiçbir şeyi
ENGELLEMİYORDU. Soruyu soran taraf burasıdır çünkü sıranın sahibi `Battle` ve o
bilgiyi ilk gören akış budur. Kural aşağı **inmedi ve inemez**: "aktif takım"
diye bir kavram `GridStrategy.Combat`'ta yoktur — kuralı uygulayabilen en alt
katman bu.

| Üye | Karar | Detay |
|---|---|---|
| dört eylemin iskeleti | sıra bu dosyanın tek yapısal kararı | [↓](#battleactions-iskelet) |
| `Attack(Battle, Unit, Unit)` | sıra yazmadan ÖNCE sorulur; ön kontrol kalktı | [↓](#attackbattle-unit-unit) |
| `Move(Battle, Unit, int, int, int)` | profil kurallardan önce kurulur; kural inebildiği yerde | [↓](#movebattle-unit-int-int-int) |
| `Revive(Battle, Unit, Unit)` | üç aktör kuralı ayrı durur; menzil ödünç alınmış | [↓](#revivebattle-unit-unit) |
| `PlaceStructure(...)` | eyleyen yok, sıra sorulmaz; katılım hatası kopyalanmaz | [↓](#placestructurebattle-unit-structure-int-int) |
| `RequireCombatant(Battle, Unit, string)` | programcı hatası istisnadır, sonuç değeri değil | [↓](#requirecombatantbattle-unit-string) |
| `RequireCell(Battle, Unit, string, out int, out int)` | "tahtada yok" ile "savaşta değil" aynı cümle | [↓](#requirecellbattle-unit-string-out-int-out-int) |

**İlgili anlatılar:** [04-karar sırası](../../konular/04-karar-sirasi.md) ·
[03-tahta sahipliği](../../konular/03-tahta-sahipligi.md) ·
[06-sonuç enumları](../../konular/06-sonuc-enumlari.md) ·
[07-tıklamadan eyleme](../../konular/07-tiklamadan-eyleme.md)

---

## BattleActions (iskelet)

**DÖRT EYLEMİN ORTAK İSKELETİ — SIRA BU DOSYANIN TEK YAPISAL KARARI.**

### HARİTA: adım zinciri ve GERİ DÖNÜLEMEZ nokta

Dört genel metot da aynı zinciri yürütür; sıraya her metotta yeniden karar
verilmiyor.

```
  ADIM 0  null denetimi         çağıran hatası ► ArgumentNullException
  ADIM 1  "bu savaşta mı"       çağıran hatası ► ArgumentException
          RequireCombatant / RequireCell
  ADIM 2  SIRA kuralı           oyun sonucu    ► Rejected...CannotAct
  ADIM 3  EYLEYENİN durumu      oyun sonucu    ► Rejected...CannotAct
  ADIM 4  HEDEF / HÜCRE kuralı  oyun sonucu    ► RejectedInvalidTarget
  ADIM 5  MENZİL                oyun sonucu    ► RejectedOutOfRange
  ────────────────────────────────────────────────────────────────
  ADIM 6  ██ TEK YAZMA ██   ◄── AYRIŞMA NOKTASI: buradan sonrası
  ADIM 7  sıra devri            geri alınamaz
```

Çizginin **ÜSTÜ soru, ALTI olgu.** Bir kural çizginin altına düşerse kural
olmaktan çıkıp AÇIKLAMAYA döner; ölçüsü
[Attack'teki sıra bloğunda](#attack-turnrulescanact) yazılı — hasar zaten inmişse
"reddedildi" bir metindir.

### KAPSAM: dördü aynı İSKELETİ izler, aynı ADIMLARI izlemez

```
  Attack          0 1 2 · · ·  6 7    4-5 AttackAction'ın İÇİNDE
  Move            0 1 2 3 · ·  6 7    4-5 MoveAction'ın İÇİNDE
  Revive          0 1 2 3 4 5  6 7    tam zincir, hepsi burada
  PlaceStructure  0 · · · 4 ·  6 ·    ◄── KARŞI ÖRNEK: 2 de 7 de YOK
```

`·` = bu metotta sorulmuyor. `Attack` ile `Move`'da sorulmuyor çünkü ALT KATMAN
soruyor; `PlaceStructure`'da sorulmuyor çünkü **ORTADA SORU YOK** — imzasında bir
eyleyen yok, yalnızca konulan şey var (gerekçenin tamamı
[`PlacementOutcome.Placed`](PlacementOutcome.md#placed) başlığında). Yani bu
tablodan "her metot sırayı sorar" diye bir kural OKUNMAZ; okunacak kural şudur:
**sıra sorulacaksa ADIM 2'de sorulur.**

### İŞ BÖLÜMÜ: İSTİSNA ile SONUÇ DEĞERİ ÖRTÜŞMEZ, BÖLÜŞÜR

```
  ADIM 0-1  ► istisna       çağıranın kaydı Battle'ınkiyle ayrıştı
  ADIM 2-5  ► sonuç değeri  oyuncunun yapabildiği bir şey reddedildi
```

İstisna yolu silinirse "bu savaşta değil" bir enum değerine iner ve sessizce
yutulabilen bir dala girer — programcı hatası oyun sonucu kılığına girer
([RequireCombatant](#requirecombatantbattle-unit-string)). Sonuç değeri yolu
silinirse dolu hücreye tıklamak istisna atar ve çağıran her hamleyi try/catch'e
sarar ([PlacementOutcome](PlacementOutcome.md#rejectedcelloccupied)). İkisi aynı
soruyu iki kez cevaplamıyor; iki ayrı soruyu bölüşüyor ve ikisi de gerekli.

---

## Attack(Battle, Unit, Unit)

Bir saldırı denemesini yürütür: konumları `Battle`'dan bulur, mesafeyi
`GridDistance`'a ölçtürür ve saldırıyı `AttackAction`'a çözdürür. Hedef bir BİRİM
de olabilir bir YAPI da; hangisi olduğunu bu metot `Battle.TryGetStructure`'a
**SORAR**.

Metodun başındaki sıra kasıtlı: **önce** bütün "bu savaşta mı" soruları, **sonra**
kurallar. Bir çağıran hatası hiçbir zaman bir oyun sonucu kılığına girmemeli,
dolayısıyla tek bir `AttackOutcome` dönmeden önce hepsinin cevaplanmış olması
gerekir.

Birim tarafı yalnızca hedef bir yapı DEĞİLSE aranıyor: yapıların `Combatant`'ı
yok ve olmamalı — `Structure`, `Combatant`'tan türemiyor.

Mesafe burada hesaplanmıyor, **hesaplatılıyor**. Chebyshev kararı
`GridDistance.Between`'in içinde yaşıyor; buraya kopyalanmış bir
`Math.Max(Math.Abs(...), ...)` o kararı ikinci bir yere yazardı.
`Attack_DiagonalNeighbourWithRangeOne_Hits` testi tam olarak bu sahipliği
koruyor: çapraz komşu Chebyshev'de 1, Manhattan'da 2.

İki aşırı yükleme, tek akış: dallanma `AttackAction.Execute` çağrısında bitiyor
çünkü ayrılan tek şey hedefin **tipi**; sıra, mesafe ve saldıran iki dalda da
aynı.

### Attack: targetIsStructure

**HEDEFİN NE OLDUĞU SORULUYOR, TAŞINMIYOR.** Cevabı zaten bilen tek yer `Battle`;
çağıranın elinde o bilgi yok ve olması da gerekmiyor.

**HARİTA: cevabın TEK kaynağı ve soruyu soran**

```
  soru: "target bir YAPI mı?"

  ╔══════════════ Battle ══════════════╗
  ║ combatants : Unit ──► Combatant    ║ ██ TEK KAYNAK ██
  ║ structures : Unit ──► Structure    ║
  ╚═════════════════▲══════════════════╝
                    │ TryGetStructure(target, out ...)
          ┌─────────┴────────┐
          │  BattleActions   │  ◄── SORAN
          └──────────────────┘

  REDDEDILEN yolda cevabı BoardAdapter ÜRETİRDİ:
  ┌── BoardAdapter ──┐
  │ unitViews        │──► bool targetIsStructure ──► İKİNCİ
  └──────────────────┘    (görsel tablodan çıkarım)   KAYNAK
```

İkisi ayrıştığında hangisinin doğru olduğunu söyleyen tek satır yok; `Battle`
yanlış cevap **VERMEZ**, ona **SORULMAZ**.

**KAPSAM: "çağırandan veri alma" diye bir kural YOK**

Ölçüt tek: cevabı `Battle` üretebiliyor mu? Üretebiliyorsa sorulur, üretemiyorsa
taşınır.

**KARŞI ÖRNEK** aynı dosyada, `Move`'un `moveRange` parametresi: o sayıyı `Battle`
BİLMEZ — hangi birimin bu turda kaç hücre gidebildiği o tipte kayıtlı değil — ve
bu yüzden çağırandan gelmesi doğrudur. Aynı ailenin iki imzasında
`targetIsStructure` yanlış, `moveRange` doğru; fark parametrenin şeklinde değil,
cevabın sahibinde.

**İŞ BÖLÜMÜ: TryGetStructure ile RequireCombatant**

İkisi de aynı hedefi aynı `Battle`'a soruyor, ama farklı soru:

```
  TryGetStructure   "yapı mı"        false = NORMAL cevap
  RequireCombatant  "bu savaşta mı"  false = ÇAĞIRAN HATASI
```

Bu fark koda şöyle yansıyor. **Dikkat: `RequireCombatant` bu metotta İKİ kez
geçiyor ve ikisi farklı davranıyor** — pasajın anlattığı, ikincisidir:

```csharp
// ① SALDIRAN — koşulsuz. Saldıran her zaman bir savaşçıdır; yapılar saldırmaz.
Combatant attackerCombatant = RequireCombatant(battle, attacker, nameof(attacker));

bool targetIsStructure = battle.TryGetStructure(target, out Structure targetStructure);

// ② HEDEF — koşullu. İşte "atlanan" çağrı bu:
Combatant targetCombatant = targetIsStructure
    ? null                                             // ◄── yapı dalı: ÇAĞIRMAZ
    : RequireCombatant(battle, target, nameof(target)); // ◄── savaşçı dalı: çağırır
```

Yani atlanan şey ①  değil ②'dir. Saldıran için `RequireCombatant` her zaman
koşar; yalnızca **hedef** yapı olduğunda ikinci çağrı devre dışı kalır ve yerine
`null` konur. `AttackAction`'ın yapı aşırı yüklemesi zaten `Combatant` değil
`Structure` istediği için o `null` hiçbir yerde okunmaz.

**İki mekanizmanın da silinme bedeli** — hangi satırın kaldırıldığı önemli:

```
② TryGetStructure çağrısı kaldırılsa (targetIsStructure hep false)
      baraka hedefi ternary'nin SAVAŞÇI dalına düşer
   ─► RequireCombatant onu combatants sözlüğünde arar, bulamaz
   ─► ArgumentException("The unit is not in this battle.")
      ██ oysa baraka SAVAŞTA — yanlış deftere bakıldı ██

② RequireCombatant yerine TryGetCombatant konsa (hata yerine null dönse)
      savaşta olmayan bir birim null bir Combatant ile ilerler
   ─► AttackAction.Execute içinde attacker.AttackProfile okunur
   ─► NullReferenceException
      ██ hata, sebebini söylemeyen BAŞKA bir yerde patlar ██
```

İkisi birbirinin yedeği değil: birincisi **hangi deftere** bakılacağını, ikincisi
**bulunamazsa ne olacağını** belirliyor.

**REDDEDILEN** — hedefin tipi çağırandan bir bayrakla gelir:

```csharp
public static AttackOutcome Attack(Battle battle, Unit attacker,
                                   Unit target, bool targetIsStructure)
```

**KIRILAN:** çağıran, `Battle`'ın ZATEN bildiği bir şeyi taşır.

```
BoardAdapter tipi kendi görsel tablosundan çıkarır
  -> bir yerde `false` geçer
  -> akış barakayı RequireCombatant'a düşürüp ilgisiz bir istisna atar
derleyici: yanlış bayrağı GÖRMEZ  ·  test: yeşil kalır
```

**KAZANIRDI:** hedef tipi çağıranın KENDİ kararı olsaydı — aynı hücrede hem bir
birim hem bir yapı durabilseydi (köprü, tuzak) ve "hangisine vuruyorsun" gerçek
bir seçim olsaydı.

**TEK CUMLE:** Cevabı zaten bilen tarafa sorulmayan her soru, çağıranda yanlış
doldurulabilen bir parametreye dönüşür.

### Attack: TurnRules.CanAct

**SIRA KURALI, SONUÇ DEĞERİ DÖNDÜREN HER ŞEYDEN ÖNCE SORULUYOR** — hedefin
uygunluğundan da, menzilden de önce. "HER ŞEYDEN önce" DEĞİL, ve aradaki fark
bu bölümün konusu: `CanAct`'e gelindiğinde istisna kapıları ÇOKTAN geçilmiştir.
Ölçüt S-06'nın sıra dersinin ölçütü: cevabı düzeltmenin çağırana BİR ŞEY
KAZANDIRMADIĞI sebep önce söylenir. Sıra sende değilken başka bir hedef seçmek de,
yaklaşmak da hiçbir şeyi değiştirmez.

Kuralın bir TAVANI, bir de TABANI var ve ikisini iki ayrı şey çiziyor: tavanı
istisna kanalı (yukarı çıkamaz), tabanı geri dönülemez adım (aşağı inemez).
Aşağıdaki iki harita bu iki sınırı ayrı ayrı gösteriyor.

**HARİTA: TAVAN — `CanAct`'ten ÖNCE ne çalışıyor, iki kanal tek çizgi**

```
  İSTİSNA KANALI ── okuyucusu PROGRAMCI ── "senin kaydın Battle'ınkiyle ayrışmış"
  ─────────────────────────────────────────────────────────────────────────────
   battle / attacker / target null mu      ►  ArgumentNullException
   RequireCombatant(battle, attacker, …)   ►  ArgumentException
   RequireCombatant(battle, target, …)     ►  ArgumentException  (hedef YAPI değilse)
   RequireCell(battle, attacker, …)        ►  ArgumentException
   RequireCell(battle, target, …)          ►  ArgumentException

  ███████ ÇİZGİ: İLK AttackOutcome DEĞERİ BURADAN SONRA DOĞAR ███████

  SONUÇ DEĞERİ KANALI ── okuyucusu OYUNCU ── "bu hamle olmadı"
  ─────────────────────────────────────────────────────────────────────────────
   TurnRules.CanAct(…)      ◄── BURADA     ►  RejectedActorCannotAct
   AttackAction.Execute(…)                 ►  menzil · dost ateşi · düşmüş saldıran
```

Sıra kuralı çizginin ALTINDAKİ İLK satır — kanalının başı, akışın başı değil.
Üstündeki hiçbir kapı bir hamleyi reddetmiyor; hepsi "çağıranın kaydı bozuk"
diyor ve o cevap bir `AttackOutcome` değerine HİÇ dönüşmüyor
([RequireCombatant](#requirecombatantbattle-unit-string),
[RequireCell](#requirecellbattle-unit-string-out-int-out-int)).

Listede olmayan iki şeyin yokluğu kasıtlı: `battle.TryGetStructure` bir kapı
DEĞİL, çünkü oradaki `false` bir hata değil "hedef bir birim" cevabıdır. Negatif
menzil doğrulaması da bu listede yok — o kapı `Attack`'te hiç bulunmuyor,
`Move`'un `new MoveProfile(moveRange)` satırında yaşıyor
([Move: MoveProfile](#move-moveprofile)); aşağıda yine karşımıza çıkacak.

**ERKEN ÇIKIŞ SORUSU: `CanAct` neden `attackerCombatant`'tan HEMEN SONRA değil?**

Soru yerinde ve gözlem TEKNİK OLARAK DOĞRU: `CanAct` reddedecekse, iki
`RequireCell` çağrısının yaptığı tarama gerçekten boşa gidiyor. `CanAct`'in veri
olarak beklemek zorunda olduğu tek satır `RequireCombatant(battle, attacker, …)`,
çünkü `attackerCombatant.Team`'i ondan alıyor. Geri kalan üç kapı — hedefin
savaşçısı ve iki hücre araması — teknik olarak AŞAĞI itilebilir. İtilmiyorlar; bu
bölüm bunun bedelini ve karşılığını ölçüyor.

Boşa giden işin ŞEKLİ, `Battle.TryGetPosition`'ın gövdesinde:

```csharp
for (int cellX = 0; cellX < board.Width; cellX++)
{
    for (int cellY = 0; cellY < board.Height; cellY++)
    {
        if (board.TryGetUnit(cellX, cellY, out Unit standing)
            && ReferenceEquals(standing, unit))
        {
            x = cellX; y = cellY; return true;   // ◄── BULUNCA ANINDA DÖNER
        }
    }
}
```

Tam tahta taraması, `O(Genişlik × Yükseklik)`. `UnitGrid.TryGetUnit` bir sınır
kontrolü artı bir `cells[x, y]` dizi okuması, yani maliyeti taşıyan şey YOKLAMA
SAYISI. `Attack` içinde `RequireCell` **iki kez** çağrılıyor (saldıran, hedef),
dolayısıyla `CanAct`'e varmadan önceki en kötü hâl `2 × Genişlik × Yükseklik`.

**ÖLÇÜ** — bu bölümün kendi testi, `Attack_OutOfTurnAgainstAnInvalidTargetOutOfRange_PrefersActorCannotAct`:

```
  tahta: new Battle(3, 5)  ⇒ 15 hücre        tarama: x DIŞ, y İÇ (sütun sütun)
  bir yoklama = sınır kontrolü + cells[x, y] + ReferenceEquals

  saldıran (0,0)  ►  ilk yoklamada bulunur ...................   1 yoklama
  hedef    (2,4)  ►  sütun sırasında SON hücre ...............  15 yoklama
                                                                ───────────
  CanAct reddi gelmeden önce harcanan .......................   16 yoklama
  en kötü hâl (2 × 3 × 5) ...................................   30 yoklama
```

Erken çıkışın KAZANDIRACAĞI şey budur: bu tahtada 16 dizi okuması. Ödeyeceği şey
aşağıda.

**KAYBEDİLEN** — `CanAct` `attackerCombatant`'tan hemen sonraya alınsaydı:

```csharp
Combatant attackerCombatant = RequireCombatant(battle, attacker, nameof(attacker));

if (!TurnRules.CanAct(attackerCombatant.Team, battle.Turn.Current))   // ◄── YUKARI ALINDI
{
    return AttackOutcome.RejectedActorCannotAct;
}

bool targetIsStructure = battle.TryGetStructure(target, out Structure targetStructure);
Combatant targetCombatant = targetIsStructure
    ? null
    : RequireCombatant(battle, target, nameof(target));   // ◄── ARTIK ULAŞILAMIYOR
```

**KIRILAN:** kural çizginin ÜSTÜNE çıkar ve istisna kanalını gölgeler.

```
sırası gelmemiş oyuncu, Battle'ın TANIMADIĞI bir hedefe saldırır
  -> cevap "RejectedActorCannotAct" ................ "sıran değil"
  -> gerçek sebep ......... çağıranın kaydı Battle'ınkiyle ayrışmış
  -> çağıran sırasını bekler, aynı hamleyi tekrar dener, ANCAK O ZAMAN patlar
     ██ programcı hatası, bir tur boyunca oyun sonucu kılığında dolaşır ██
derleyici: hiçbir şey der
test: Attack tarafında YEŞİL KALIR — Attack_UnknownTarget_Throws saldıranı
      sırası GELMİŞ hâlde çağırıyor (EndTurn yok), yani CanAct'i hiç zorlamıyor
test: Move tarafında KIRMIZI — Move_NegativeRangeOutOfTurn_StillThrows
```

Son satır bu bölümün tek makine kanıtı ve `Move` tarafında olması bir kaza değil:
aynı ilkeyi tetikleyen kapı (negatif menzil) orada duruyor. Test önce
`battle.Turn.EndTurn()` ile birimi sıra dışı bırakıyor, sonra `moveRange: -1` ile
çağırıp `ArgumentOutOfRangeException` BEKLİYOR — sıra dışı olmak, bozuk argümanı
YUTMUYOR. `Attack` tarafında bu ikisini birleştiren bir test YOK; kuralı bugün
`Attack`'te tutan şey testler değil, çizginin kendisi.

Ve maliyetin SAHİBİ bu sıralama değil: taramayı yapan `Battle.TryGetPosition`, ve
konumun neden önbelleğe ALINMADIĞI orada ayrıca yazılı — ikinci bir doğruluk
kaynağı doğmasın diye. Tahta büyüdüğünde ödenecek yer orasıdır, kuralın sırası
değil. 16 dizi okuması bir teşhisin doğruluğuna karşı takas EDİLMEZ.

**HARİTA: TABAN — sorunun YERİ cevabın ANLAMINI değiştirir**

```
  SEÇİLEN                        REDDEDILEN
  ──────────────────────────     ──────────────────────────
  1  sıra kuralı  ◄── BURADA     1  mesafe
  2  mesafe                      2  AttackAction.Execute
  3  AttackAction.Execute           ██ CAN AZALDI ██
     ██ CAN AZALDI ██            3  sıra kuralı  ◄── BURADA
  ──────────────────────────     ──────────────────────────
  ret ⇒ tahta HİÇ değişmedi      ret ⇒ tahta ZATEN değişti
```

İki sütun aynı üç satırı taşıyor; ayrışan tek şey ██ işaretli geri dönülemez
adımın kaçıncı sırada durduğu. Kural o adımın **ÜSTÜNDEyse** kuraldır,
**ALTINDAysa** açıklamadır.

Takım bilgisi **SAVAŞÇIDAN** geliyor, birimden değil: `Unit` tarafı bilmez ve bu
akış onu ikinci bir yerden okumaya kalksaydı taraf iki sahipli olurdu.

**KAPSAM: kural "EN BAŞTA" değil, "YAZMADAN ÖNCE"**

Korunan şey sıralamanın kendisi değil, geri dönülemez adıma göre **KONUMU**.
Yazmadan önceki bölgenin içinde sıra serbesttir.

**KARŞI ÖRNEK** aynı dosyada, [`Move`'un iki kuralı](#move-turnrulescanact):
`TurnRules.CanAct` ile `MovementRules.CanMove` art arda duruyor, ikisi de AYNI
`MoveOutcome.RejectedActorCannotAct`'i döndürüyor ve hangisinin önce sorulduğu
hiçbir testten GÖZLENEMİYOR — orada korunacak bir sıra kararı yok. Burada var,
çünkü aradaki satır tahtaya yazıyor. Ölçüt "hangisi daha önemli görünüyor" değil,
"arada geri dönülemez bir adım var mı".

**İŞ BÖLÜMÜ: DURUM, KURAL ve KİMLİK ayrı yerlerde**

```
  battle.Turn.Current      DURUM   "sıra kimde"   TurnState
  TurnRules.CanAct         KURAL   "ondan ne çıkar"
  attackerCombatant.Team   KİMLİK  "soran kim"    Combatant
```

`Turn.Current` silinemez — o bilgi başka hiçbir yerde yok. `TurnRules` silinip
karşılaştırma buraya yazılırsa aynı kural üç akışta (`Attack`, `Move`, `Revive`)
üç kez yaşar ve tarafsızlık denetimi üçünde ayrı ayrı unutulabilir hâle gelir.
`Team`, `Unit`'ten okunursa taraf iki sahipli olur. Üçü ÖRTÜŞMÜYOR; her biri aynı
cümlenin farklı bir kelimesini tutuyor.

**REDDEDILEN** — sıra sorusu hedefin uygunluğundan SONRA, yani `AttackAction`'a
devredildikten sonra sorulur:

```csharp
AttackOutcome outcome = AttackAction.Execute(
    attackerCombatant, targetCombatant, distance);
if (outcome != AttackOutcome.Hit && outcome != AttackOutcome.HitAndDowned)
{
    return outcome;
}
if (!TurnRules.CanAct(attackerCombatant.Team, battle.Turn.Current))
{
    return AttackOutcome.RejectedActorCannotAct;
}
```

**KIRILAN:** hasar ZATEN İNMİŞ olur; ret yalnızca SONUÇ metninde kalır.

```
"vurdu" cevabı hedefin canının çoktan azaldığı demektir
  -> sırası gelmemiş oyuncunun vuruşu tahtada gerçekleşir
derleyici: hiçbir şey der
test: kırmızı — Attack_WhenItIsNotTheAttackersTurn_IsRejectedAndDealsNoDamage
```

**KAZANIRDI:** sıra kuralı bir SİMÜLASYON kısıtı değil bir ARAYÜZ kısıtı olsaydı
— motor her hamleyi çözüp gösterseydi ve sırayı yalnızca ekran engelleseydi ("ne
olurdu" göstergesi, tekrar kaydı önizlemesi).

**TEK CUMLE:** Geri alınamaz bir işlemden SONRA sorulan kural bir kural değil, bir
açıklamadır — ve istisna kapılarından ÖNCE sorulan kural, bir programcı hatasını
oyun sonucu kılığına sokar.

### Attack: TargetingRules.CanBeAttacked

**TAKIM ÖN KONTROLÜ BURADAN KALKTI** — ve bu, bir borcun kapanışıdır. Eskiden
`TurnRules.CanAct` ile mesafe hesabı arasında ikinci bir
`TargetingRules.CanBeAttacked` çağrısı vardı; o çağrı, `AttackAction` takımı
sormadığı dönemde dost ateşini engelleyen **TEK** yerdi. `AttackAction` artık
kuralı kendisi soruyor (S-12: kural uygulayabilen en alt katmana iner),
dolayısıyla buradaki soru bir koruma değil yalnızca bir **TEKRAR**.

**HARİTA: kuralın METNİ nereye indi**

```
  ÖNCE                            ŞİMDİ
  BattleActions                   BattleActions
    └─ CanBeAttacked ◄── metin      └─ (soru YOK)
         │                              │
  AttackAction                    AttackAction
    └─ (takımı SORMAZ)              └─ CanBeAttacked ◄── metin
         │                              │   ██ TEK YER ██
  TargetingRules  ◄── kuralın sahibi, iki dünyada da aynı
```

Metin **AŞAĞI indi, silinmedi.** Kalkan şey kuralın kendisi değil, aynı kuralın
İKİNCİ kez sorulması.

Kaldırmanın güvenli olduğunun kanıtı testte yazılıydı ve önceden yazılmıştı:
`Attack_SameTeamTarget_IsRejectedByTheFlowAndByAttackActionAlike` — o testin
ikinci yarısı akışı ATLAYIP doğrudan `AttackAction`'a soruyor ve aynı cevabı
alıyor. Ön kontrol kalktı, o test kırmızıya dönmedi — tam olarak öngörüldüğü gibi.

**KAPSAM: "akış kural SORMAZ" diye bir kural YOK**

Kalkan şey bir TEKRARdı, bir SORU değil. Ölçüt: alt katman o kuralı zaten soruyor
mu?

**KARŞI ÖRNEK** aynı dosyada, [`Revive`'ın iki satırı](#revivebattle-unit-unit) —
`ReviveRules.CanRevive` ve `TargetingRules.CanBeRevived`. İkisi de akışta
soruluyor ve **KALKMIYOR**, çünkü altta soran yok: `Combatant.TryRevive` menzili,
sırayı ve tarafı GÖRMEZ. Aynı dosyada bir kural kalkıyor, iki kural duruyor;
ayıran şey akışın nezaketi değil, alt katmanın görebildiği tipler.

**İŞ BÖLÜMÜ: kalan iki soru ÖRTÜŞMÜYOR**

```
  TurnRules.CanAct        eyleyenin SIRASI   yalnız BURADA
  AttackAction (içeride)  hedefin UYGUNLUĞU  yalnız ORADA
```

Kalkan üçüncü soru ikincisinin kopyasıydı, birincisinin değil. `TurnRules.CanAct`
silinirse sırası gelmemiş taraf vurur ve `AttackAction` bunu göremez — "aktif
takım" kavramı `GridStrategy.Combat`'ta yoktur. `AttackAction`'ın kuralı
silinirse dost ateşi serbest kalır ve buradan görülmez. Bu yüzden buradaki silme
güvenliydi, oradaki asla olmaz.

**REDDEDILEN** — tekrar korunur, "iki kere sormak zarar vermez" diye:

```csharp
if (!TargetingRules.CanBeAttacked(
        targetCombatant.State, attackerCombatant.Team, targetCombatant.Team))
{
    return AttackOutcome.RejectedInvalidTarget;
}
```

**KIRILAN:** kopya, hedef tipi başına **ÇOĞALIR**.

```
yapı hedefinde State bir StructureState'tir
  -> aynı ön kontrol için ikinci bir dal gerekir
  -> kural değişince (dost ateşi kipi) iki yerden biri güncellenmeyi unutur
derleyici: hiçbir şey der  ·  test: hepsi yeşil kalır
```

**KAZANIRDI:** ön kontrol PAHALI bir işi engelleseydi — mesafe hesabı bir yol
bulma çağrısı olsaydı ya da `AttackAction` bir ağ isteği yapsaydı; o gün ucuz
eleme önde durur.

**TEK CUMLE:** Aynı kuralı iki kere sormak bugün bedava, yarın iki farklı
cevaptır.

### Attack: EndTurn

**SIRA BURADA DEVREDİLİR — ve bu satır olmadan oyun KIRIKTI.**
`TurnRules.CanAct` üç eylemde de soruluyordu ama `TurnState.EndTurn` üretimde
HİÇ çağrılmıyordu: Player sonsuza kadar oynardı, Enemy hiç sıra almazdı, ve
hiçbir test kırmızı olmazdı çünkü her test tek bir eylemi sınıyordu. **Bir kuralı
SORMAK ile onu İŞLETMEK farklı işlerdir**; ilki yazılmıştı, ikincisi
yazılmamıştı.

Neden burada, `BoardAdapter`'da değil: S-12. Sırayı ekranda devretseydik, akışın
ikinci bir çağıranı doğduğu gün (yapay zekâ, tekrar kaydı, bir test) sırayı
devretmeyi unuturdu ve kural yalnızca fare ile oynandığında geçerli olurdu.

**BEYAZ LİSTE, kara liste değil** — `MovementRules.CanMove` ile aynı gerekçe:
yarın eklenen bir ret değeri (ör. `RejectedNoAmmo`) kara listede varsayılan
olarak "eylem sayılır" ve sırayı SESSİZCE yakar.

**HARİTA: YARIN eklenen değer hangi kutuya düşer**

```
                            BEYAZ LİSTE    KARA LİSTE
  AttackOutcome değeri      (seçilen)      (reddedilen)
  ───────────────────────   ───────────    ────────────
  Hit                       ✓ yakar        ✓ yakar
  HitAndDowned              ✓ yakar        ✓ yakar
  HitAndDestroyed           ✓ yakar        ✓ yakar
  RejectedOutOfRange        · yakmaz       · yakmaz
  RejectedInvalidTarget     · yakmaz       · yakmaz
  RejectedActorCannotAct    · yakmaz       · yakmaz
  ───────────────────────   ───────────    ────────────
  RejectedNoAmmo  (YARIN)   · yakmaz       ✓ YAKAR
                            ◄── güvenli    ◄── ██ SESSİZ HATA ██
```

Bugünkü altı satırda iki sütun BİREBİR aynı; ayrışma yalnızca son satırda — yani
listeyi yazan gün değil, listeye dokunmayı **UNUTAN** gün. Fark bir doğruluk farkı
değil, **VARSAYILAN** farkı.

**KAPSAM: beyaz liste her sıra devrinde gerekmez**

Ölçüt: "başarılı mı" sorusunun cevabı bir ALT KATMANDAN mı geliyor?

**KARŞI ÖRNEK** aynı dosyada, [`Revive`'ın sonundaki](#revive-endturn) çıplak
`battle.Turn.EndTurn()`: orada liste YOK ve olmamalı, çünkü o satıra ulaşmanın
tek yolu bütün ret dallarını geçmektir — başarıyı akışın **KONUMU** söylüyor, bir
değer karşılaştırması değil. `Attack` ile `Move`'da sonuç
`AttackAction`/`MoveAction`'dan geliyor ve bu metot onu görmeden döndürüyor;
liste tam olarak o körlüğü kapatıyor.

**İŞ BÖLÜMÜ: GİRİŞ kapısı ile ÇIKIŞ kapısı**

```
  TurnRules.CanAct (yukarıda)  ► kim eyleyebilir    GİRİŞ
  beyaz liste (bu satır)       ► ne sırayı harcar   ÇIKIŞ
```

Biri sırayı KORUR, diğeri sırayı İLERLETİR; hiçbiri diğerinin işini yapamaz.
`CanAct` silinirse sırası gelmemiş taraf vurur. Beyaz liste silinirse iki yönden
biri olur: koşul hiç yazılmazsa `EndTurn` hiç çağrılmaz — kural sorulur ama
işlemez, bu dosyanın kapattığı borcun ta kendisi — ya da koşul kalkıp her deneme
sırayı yakar. İkisi birlikte "bir tur = bir BAŞARILI eylem" cümlesini kuruyor.

**REDDEDILEN** — reddedilen deneme de sırayı yakar, "düşünüp doğru hamleyi bul"
diye:

```csharp
battle.Turn.EndTurn();
return outcome;
```

**KIRILAN:** kırılan şey oynanabilirlik.

```
menzil dışı bir hücreye tek bir yanlış tıklama olur
  -> tur biter
  -> RejectedActorCannotAct dönen çağrı bile sırayı yakar, yani
     "sıran değil" cevabı sıranı bitirir
derleyici: hiçbir şey der  ·  test: yeşil kalır
```

**KAZANIRDI:** eylem bir KAYNAK harcasaydı (mermi, mana, hareket puanı) ve deneme
o kaynağı zaten tüketiyorsa — o gün ret de bir maliyettir ve sırayı yakması
tutarlıdır.

**TEK CUMLE:** Beyaz liste yanlışı "sıram bitmedi"ye, kara liste yanlışı
"turumu kaybettim"e düşürür — biri geri alınabilir, diğeri değil.

---

## Move(Battle, Unit, int, int, int)

Bir hareket denemesini yürütür: birimin bulunduğu hücreyi `Battle`'dan bulur, sıra
ve durum kurallarını **SORAR**, geri kalan kararı `MoveAction`'a bırakır.

`moveRange` sayısının nereden geldiği hâlâ çağıranın sorunu — gerekçesi
`MoveAction`'ın `int` menzil alan `Execute` sürümünün üstündeki "HAREKET MENZİLİ
NEREDEN GELİR" bloğunda.

Metodun sonundaki sıra devri: gerekçesi [`Attack`](#attack-endturn)'te tek kez
yazılı, burada TEKRAR EDİLMİYOR. Beyaz liste burada tek değere iniyor çünkü
`MoveOutcome`'un tek başarı değeri var; kara liste yazılsaydı üç ret değerinin
üçünü de saymak gerekirdi ve dördüncüsü eklendiği gün sessizce sıra yakardı.

### Move: MoveProfile

**PROFİL BURADA KURULUYOR — VE KURALLARDAN ÖNCE.** İki ayrı sebep var, ve ikisi
de aynı satırı savunuyor:

- **Sayıyı tipine çevirmenin yeri burasıdır.** `MoveProfile` Core'da doğdu ve
  `MoveAction`'ın profil alan aşırı yüklemesi onu bekliyor; imzası dondurulmuş
  olan bu metot ise hâlâ çıplak bir `int` alıyor. Çeviriyi yapacak yer, ikisini de
  tanıyan akıştır — `MoveAction`'ın profil alan `Execute` sürümünün üstündeki EŞİK
  notu bu çağıranı adıyla anıyor.
- **Negatif menzil bir ÇAĞIRAN HATASIdır** ve bu dosyanın iskeleti çağıran
  hatalarını kurallardan önce sorar.

**HARİTA: kurulumun YERİ**

```
  ┌─ ADIM 0-1  çağıran hataları ──┐
  │  null denetimi                │
  │  new MoveProfile(moveRange) ◄─┼── ██ BURADA ██ negatif
  │  RequireCombatant/RequireCell │   sayı burada patlar
  └───────────────────────────────┘
  ┌─ ADIM 2-3  oyun kuralları ────┐
  │  TurnRules.CanAct             │ ◄── aşağı alınsaydı kurulum
  │  MovementRules.CanMove        │     BURAYA düşerdi: sırası
  └───────────────────────────────┘     gelmemiş birimde bozuk
                                        sayı hiç görülmezdi
```

Doğrulamanın **METNİ** burada değil: eşik ("0 geçerli, negatif değil")
`MoveProfile`'ın kendi kurucusunda yaşıyor. Bunun görünür bir bedeli var ve
yazılıyor — fırlayan `ArgumentOutOfRangeException`'ın `ParamName`'i artık
`"moveRange"` değil `"range"`.

**Kare başına tahsis DEĞİL:** bu metot bir oyuncu ya da yapay zekâ hamlesi başına
bir kez çağrılıyor, her karede değil. Ölçüt `DamageRulesAllocationTests`'in
koruduğu çizgi — sıcak yolda çöp yok — ve bu yol sıcak değil. **EŞİK:** bir yol
bulucu aynı kareyi yüzlerce kez denemeye başladığı gün profil çağırandan gelmeli,
çünkü o gün yol GERÇEKTEN sıcak olur.

### Move: TurnRules.CanAct

**İKİ KURAL, TEK RET DEĞERİ — ve sıraları GÖZLENEMEZ.** İkisi de aynı
`MoveOutcome.RejectedActorCannotAct`'i döndürdüğü için hangisinin önce sorulduğunu
hiçbir test ayırt edemez. Bu dürüst bir sınır ve öyle yazılıyor: burada korunacak
bir sıra kararı **YOK**, çünkü sıra kararı ancak farklı cevaplar arasında var
olur. Sıra kuralının önde durması bir tercih — savaş dışı kısıt, savaş içi
kısıttan önce — ve doğruluğu değil yalnızca okunuşu etkiliyor.

Değer **İKİYE ayrıldığı gün** (eşiği `MoveOutcome`'da yazılı) bu sıra ölçülebilir
hâle gelir ve o gün bir karar olur; bugün değil.

### Move: MovementRules.CanMove

**DÜŞMÜŞ BİRİM ARTIK YÜRÜYEMEZ.** Bu satırın yerinde bir borç notu duruyordu:
"kuralın SAHİBİ YOK". Sahip doğdu — `MovementRules` — ve akış onu **SORUYOR**,
kuralı kendisi yazmıyor. Borcu sabitleyen test
(`Move_DownedUnit_StillMoves_BecauseNoRuleOwnsMovementState`, silindi) silindi;
yerine kararı koruyan `Move_DownedUnit_IsRejected` geldi.

**Kural neden burada soruluyor da `MoveAction`'ın içinde değil:**
`MovementRules` ve `UnitState` `GridStrategy.Combat`'ta, `MoveAction` ise
`GridStrategy.Core`'da ve Core, Combat'ı **GÖRMEZ**. S-12'nin "kuralı
uygulayabilen en alt katman" ölçütü tam olarak burada duruyor — alt katman kuralı
SORAMAZ, çünkü sormak için gereken tipi göremiyor. Gerekçenin tamamı
`MoveAction`'ın profil aşırı yüklemesinin üstündeki REDDEDILEN bloğunda yazılı.

**HARİTA: kimin kimi GÖRDÜĞÜ**

```
  GridStrategy.Battle  ◄── bu dosya
       ├──► GridStrategy.Core    (MoveAction, UnitGrid)
       └──► GridStrategy.Combat  (MovementRules, UnitState)
            ██ İKİSİNİ BİRDEN GÖREN TEK KATMAN ██

  GridStrategy.Core ──✗──► GridStrategy.Combat
       ◄── ok YOK: MoveAction, UnitState'i yazamaz bile
```

Kural aşağıya inebildiği kadar iner; **İNEBİLDİĞİ** yer burasıdır. Alt katman bu
kuralı sormayı reddetmiyor — **soramıyor**.

### Move: MoveAction.Execute

**Alternatif:** `MoveAction`'ın `int` alan aşırı yüklemesini çağırmayı sürdürmek.
Seçilmedi: o sürümün kalkma eşiği `MoveAction`'da yazılı ve son `int` çağıranı
burasıydı; çağrı değişmezse eşik hiç tetiklenmez ve `MoveProfile` üretimde
çağıranı olmayan bir tip kalır.

---

## Revive(Battle, Unit, Unit)

Bir diriltme denemesini yürütür: `Combatant.TryRevive` ve
`TargetingRules.CanBeRevived` yazılmıştı ve üretimde tek bir çağıranları yoktu —
bu metot onları bir **EYLEME** bağlıyor.

Sıra: sıra kuralı → hedefin uygunluğu (durum VE taraf) → menzil → diriltme.
İskelet saldırınınkiyle aynı ve olması gereken de bu: iki eylem de "bir hedefe bir
şey uygula" ailesinden.

### Revive: ReviveRules.CanRevive

**DİRİLTENİN KENDİ DURUMU ARTIK SORULUYOR** — ve bu, adı konmuş bir borcun
kapanışıdır. Bu satırın yerinde bir EŞİK notu duruyordu: "ReviveRules doğduğu gün
bu satırların yerine tek bir soru gelir ve sabitleyen test kırmızıya döner." Tip
doğdu, soru geldi, test silindi; yerine kararı koruyan
`Revive_DownedReviver_IsRejected` gelmiş durumda.

Aşağıdaki iki REDDEDILEN bloğu **KALDIRILMADI.** Onlar "neden ayrı bir tip"
sorusunun cevabı ve o soru kural yazıldıktan sonra da geçerli; silinselerdi geriye
yalnızca sonuç kalırdı, kararın kendisi değil.

**HARİTA: ÜÇ KURAL, BUGÜN AYNI SATIR**

```
                   soru              bugünkü metni
  MovementRules    "kim yürür"    ──► state == Alive
  AttackRules      "kim vurur"    ──► state == Alive
  ReviveRules      "kim kaldırır" ──► state == Alive
                                      ▲
                       ██ ÜÇÜ AYNI — AMA TEK METİN DEĞİL ██

  REDDEDILEN — türetme:
  ReviveRules ────► AttackRules ────► state == Alive
                    ▲
                    ██ AYRILMA GÜNÜ BURADA SESSİZCE KIRILIR ██
```

"Yaralı sıhhiyeci vuramaz ama kaldırabilir" yazıldığı gün `AttackRules` değişir ve
diriltme onunla **BİRLİKTE** değişir; ok bir ad değil bir **BAĞ** olduğu için
derleyicinin diyeceği yok.

Ailenin üçüncü ve son üyesi: `MovementRules` "kim yürür", `AttackRules` "kim
vurur", `ReviveRules` "kim kaldırır". Üçü de bugün aynı satırı taşıyor
(`state == Alive`) ve bu bir **TESADÜF** — üçünü birden kayda geçiren test
`ReviveRulesTests`'teki
`ThreeActorRules_StillAgree_WhichIsWhyTheyMustStaySeparate`. Ayrıldıkları gün
burada hiçbir şey değişmez; türetme yazan proje o günü hiçbir test kırılmadan
kaçırır.

**KAPSAM: bu metot saldırıdan bir şey ÖDÜNÇ ALIYOR**

Kural "diriltme saldırıdan hiçbir şey almaz" DEĞİL. Ölçüt: ödünç alınan şeyin
İKİNCİ bir sahibi olabilir mi?

**KARŞI ÖRNEK** aynı metodun içinde, birkaç satır aşağıda:
[menzil `AttackProfile`'dan ödünç alınıyor](#revive-attackresolveriswithinrange) —
bir okçu üç hücre öteden diriltiyor — ve bu bilerek kabul edildi, çünkü "erişim"
kavramının bu projede ikinci bir sahibi **YOK**. Orada eksik olan şey bir tip;
burada eksik olan bir şey yoktu. Aynı metotta bir ödünç kabul, bir ödünç ret;
ayıran şey tutarsızlık değil, ödünç alınanın SAHİPSİZ mi SAHİPLİ mi olduğu.

**İŞ BÖLÜMÜ: aynı ret değerini döndüren iki kural**

```
  TurnRules.CanAct             ► SIRA o tarafta mı    aktör
  ReviveRules.CanRevive        ► aktörün DURUMU       aktör
  TargetingRules.CanBeRevived  ► HEDEFİN uygunluğu    hedef
```

İlk ikisi aynı `ReviveOutcome.RejectedActorCannotAct`'i döndürüyor ve bu bir
tekrar değil bir **BÖLÜŞÜM**: biri savaş DIŞI bir kısıt (sıra), diğeri savaş İÇİ
bir kısıt (düşmüş sıhhiyeci). `CanAct` silinirse sırası gelmemiş taraf diriltir;
`CanRevive` silinirse düşmüş savaşçı kendi yerden kalkmadan başkasını kaldırır.
Üçüncüsü farklı bir değer döndürüyor çünkü farklı bir özneyi konu ediyor — çağıran
"ben mi yapamıyorum, hedef mi olmuyor" ayrımını tam olarak buradan okuyor.

**REDDEDILEN (1)** — saldırı kuralı ödünç alınır:

```csharp
if (!AttackRules.CanAttack(reviverCombatant.State))
{
    return ReviveOutcome.RejectedActorCannotAct;
}
```

**KIRILAN:** ad yalan söyler ve yalanı derleyici göremez — türetmenin reddi
`MovementRules` ile `AttackRules`'ta zaten yazılı.

```
yaralı sıhhiyeci kuralı doğar
  -> AttackRules değişir
  -> diriltme de onunla birlikte değişir
derleyici: hiçbir şey der  ·  test: hiçbiri kırılmaz
```

**KAZANIRDI:** tasarım "eyleyebilmek" diye TEK bir kavrama inseydi — o gün üç
kural bir kurala iner ve adı hiçbirinin adı olmazdı.

**TEK CUMLE:** Bir kuralı başka bir kuraldan türetmek iki kararı tek satıra
bağlar; ayrılmaları gereken gün sessiz geçer.

**REDDEDILEN (2)** — kural burada YAZILIR:

```csharp
if (reviverCombatant.State != UnitState.Alive)
{
    return ReviveOutcome.RejectedActorCannotAct;
}
```

**KIRILAN:** akış bir kural YAZMIŞ olur — rol başlığındaki tek sözün ihlali, ve
aynı ihlal bu dosyada hareket için de reddedildi.

```
kuralı çağırmanın tek yolu bu metot olur
  -> sınamak için bir Battle kurup içine düşmüş savaşçı koymak gerekir
derleyici: hiçbir şey der  ·  test: kuralı değil akışı sınar
```

**KAZANIRDI:** kuralın TEK çağıranı bu olsaydı ve başka hiçbir anlamı olmasaydı —
o gün ayrı bir tip yalnızca bir dolaylılık katmanı olurdu.

**TEK CUMLE:** Akışa yazılan kural, savaş kurmadan sorulamaz hâle gelir; ayrı bir
tip onu tek başına sınanabilir tutar.

### Revive: AttackResolver.IsWithinRange

**DİRİLTME MENZİLİ BUGÜN SALDIRI PROFİLİNDEN GELİYOR** — ve bu bir taviz, öyle de
yazılıyor. Menzil kavramının bu projedeki tek sahibi `AttackProfile`; "erişim"
ölçüsü orada yaşıyor ve ikinci bir sahip yok. Yani bir okçu üç hücre öteden
diriltir. Oyun açısından tartışmalı, tasarım açısından dürüst: uydurulmuş bir
sayıdan iyidir.

**HARİTA: "menzil" kavramının sahipleri**

```
  SEÇİLEN
  ╔═══ AttackProfile.Range ═══╗  ██ TEK SAHİP ██
  ╚═════════════╤═════════════╝
         ┌──────┴───────┐
         ▼              ▼
  AttackAction     bu satır (AttackResolver.IsWithinRange)
  hasar erişimi    destek erişimi ◄── ██ ÖDÜNÇ: kabul edilmiş
  (doğru sahip)                       taviz, gizlenmiyor ██

  REDDEDILEN — const int ReviveRange = 1;
  ╔═══ AttackProfile.Range ═══╗   ╔═══ akış dosyası ═══╗
  ╚═══════════════════════════╝   ╚════════════════════╝
         hasar erişimi                destek erişimi
                             ▲ İKİ SAHİP — ve ikincisi bir
                               KURAL dosyası değil, bir AKIŞ
```

**KAPSAM: yasak SAYILARA değil, sayının YERİNE**

Projede denge sayıları var ve her biri bir sahibin içinde:

```
  AttackProfile.Range          menzil         ► TANIM
  TurnRules.MaxActionsPerTurn  tur bütçesi    ► KURAL
  MoveProfile(range)           hareket eşiği  ► TANIM
  BattleActions                ─── HİÇBİRİ ─── ◄── ██ AKIŞ ██
```

**KARŞI ÖRNEK** aynı dosyada, [`Move`'un başındaki](#move-moveprofile)
`new MoveProfile(moveRange)`: orada da bir sayı geçiyor ama bu dosya onu
**ÜRETMİYOR** — çağırandan alıp sahibine veriyor. Fark sayının görünmesi değil,
sayının burada **DOĞMASI**.

**İŞ BÖLÜMÜ: üç parça, üç ayrı soru**

```
  GridDistance.Between  "kaç hücre"   ölçüm  (Chebyshev)
  AttackProfile.Range   "kaça kadar"  eşik
  AttackResolver        "yetiyor mu"  karşılaştırma
```

`GridDistance` silinip `Math.Abs` buraya yazılırsa Chebyshev kararı ikinci bir
yere kopyalanır. `AttackProfile` silinip sayı buraya yazılırsa yukarıdaki İKİ
SAHİP şekli doğar. `AttackResolver` silinip `distance <= range` buraya yazılırsa
karşılaştırmanın ucu — sınır dahil mi değil mi — saldırıda ve diriltmede ayrı ayrı
kararlaştırılır. Üçü ÖRTÜŞMÜYOR.

**REDDEDILEN** — menzil burada yazılır:

```csharp
const int ReviveRange = 1;
if (distance > ReviveRange)
{
    return ReviveOutcome.RejectedOutOfRange;
}
```

**KIRILAN:** akış bir DENGE SAYISI sahiplenir.

```
sayı akış dosyasına gömülür
  -> tasarımcı onu değiştirmek için kural dosyasını değil akışı düzenler
  -> sayı iki birim için de aynı olur, "menzilli sıhhiyeci" yazılamaz
derleyici: hiçbir şey der  ·  test: yeşil kalır
```

**KAZANIRDI:** diriltme menzili bir DENGE değil bir KURAL olsaydı — "diriltmek her
zaman bitişik hücreden yapılır" evrensel bir oyun kuralıysa; o gün sayı yine
buraya değil o kuralın sahibine yazılır.

**TEK CUMLE:** Bu dosyada başka hiçbir sayı yok ve olmaması bir karar — menzil
`AttackProfile`'da, tur bütçesi `TurnRules`'ta yaşar.

**EŞİK:** bir birimin DESTEK erişimi ile HASAR erişimi ayrıldığı gün (menzilli
sıhhiyeci, bitişik-diriltme kuralı) buraya bir `ReviveProfile` ya da
`SupportProfile` gelir ve o satır onu sorar.

### Revive: TryRevive

`TryRevive`'ın dönüşü **YOK SAYILMIYOR.** Buraya ulaşıldığında hedefin `Downed`
olduğu bir satır önce doğrulandı, yani `false` dönmesi `Combatant.State` ile
`UnitLifecycle.State`'in ayrıştığı anlamına gelir — bugün imkânsız, çünkü biri
diğerine devrediyor. Dönüşü sessizce atmak, o imkânsızlığı bir **VARSAYIMA**
çevirirdi; okumak onu bir **DEĞİŞMEZ** olarak tutuyor ve ayrışma bir gün doğarsa
çağıran "dirildi" yalanını almaz.

**HARİTA: değişmezin iki ucu**

```
  bir satır önce  TargetingRules.CanBeRevived(target.State)
                                             ✓ "Downed"
         │  aradaki TEK bağ:
         │  Combatant.State ──devreder──► UnitLifecycle.State
         ▼
  bu satır        target.TryRevive()  ──► false ANCAK iki uç
                  ayrıştıysa döner  ◄── ██ bugün imkânsız ██
```

### Revive: EndTurn

Sıra devri. Burada **beyaz listeye gerek yok**: bu satıra ulaşmanın tek yolu
bütün ret dallarını geçmektir, yani "başarılı" durumu akışın **KONUMU** söylüyor,
bir değer karşılaştırması değil. `Attack` ve `Move`'da karşılaştırma gerekiyor
çünkü orada sonuç bir alt katmandan geliyor ve o metot onu görmeden döndürüyor.

---

## PlaceStructure(Battle, Unit, Structure, int, int)

Bir yapıyı tahtaya koyar ve savaşa katar.

Bu metot `Battle.AddStructure`'ın önündeki **KURAL kapısıdır**: oradaki katılım
bir çağıran sözleşmesidir (dolu hücre bir istisnadır), buradaki yerleştirme ise
bir OYUN eylemidir ve reddi bir sonuç döndürür. İkisinin farkı
[`PlacementOutcome`](PlacementOutcome.md#rejectedcelloccupied) belgesinde yazılı.

`unit` parametresi yapının **tahtadaki kimliği**. Bir baraka da bir `Unit`'tir —
"tahtada yer kaplayan, kimliği olan şey" — ve ikinci bir tahta açmamanın bedeli
tam olarak budur.

### PlaceStructure: IsInsideGrid

**SIRA BİR KARARDIR:** önce tahta sınırı, sonra doluluk. Aynı sıra `MoveAction`'da
da var ve gerekçesi devralınıyor: tahta dışı bir hücre **BEKLEYEREK de** geçerli
olmaz, dolu bir hücre ise bir tur sonra boşalabilir. Düzeltilemeyen sebep önce
söylenir.

### PlaceStructure: TryGetUnit

Doluluk sorusu **TAHTAYA** soruluyor, ikinci bir deftere değil: barakalar da
tahtada yer kapladığı için bu tek soru hem birimleri hem yapıları kapsıyor.
İkinci bir tahta açsaydık burada İKİ soru olurdu ve biri unutulduğu gün aynı
hücrede iki şey dururdu — hiçbir derleme hatası çıkmadan. Aynı kararın uzun hâli
[`Battle.structures`](Battle.md#structures) başlığında.

### PlaceStructure: AddStructure

**"BU BİRİM ZATEN SAVAŞTA" BURADA BİR RET DEĞİL, BİR ÇAĞIRAN HATASI** — ve
kontrolü kopyalanmıyor, `AddStructure`'a bırakılıyor. Aynı kimliği ikinci kez
yerleştirmek bir oyuncu hamlesi değil, çağıranın kaydının `Battle`'ınkiyle
ayrışmasıdır; [`RequireCombatant`](#requirecombatantbattle-unit-string)'ın
gerekçesi bu satır için de geçerli.

**Yıkık bir yapının yerleştirilmesi için ret değeri YOK:** bugün tahtaya konmadan
yıkılmış bir `Structure` üretebilen tek yol testtir. Kayıt dosyasından hazır yapı
yüklendiği gün soru gerçek olur ve o gün sahibi bu akış değil, **yükleyici** olur.

---

## RequireCombatant(Battle, Unit, string)

**"BU SAVAŞTA DEĞİL" BİR OYUN SONUCU DEĞİL, BİR ÇAĞIRAN HATASIDIR.** `Battle`
konumun ve eşleşmenin tek sahibi; tanımadığı bir birimle çağrılması "benim kaydım
`Battle`'ınkiyle ayrışmış" demektir. Aynı felsefe `MoveAction`'ın kaynak hücre
kontrolündeki REDDEDILEN bloğunda zaten uygulanmış durumda.

### HARİTA: iki cevap kanalı, iki farklı soru

```
  "çağıranın kaydı ayrıştı"      "oyuncunun hamlesi reddedildi"
            │                                  │
            ▼                                  ▼
  ╔═ ArgumentException ═╗       ╔═ AttackOutcome / MoveOutcome ═╗
  ║ ele ALINAMAZ        ║       ║ ele ALINIR                    ║
  ║ doğru bir catch yok ║       ║ çağıran switch yazar          ║
  ╚═════════════════════╝       ╚═══════════════════════════════╝
    ██ AYRIM ÖLÇÜTÜ: bu cevabı alan çağıran YAPACAK BİR ŞEY
       bulabilir mi? Bulamıyorsa istisna. ██

  REDDEDILEN — tek kanal (BattleOutcome):
  çağıran ──► BattleOutcome ──► AttackOutcome
                   ▲                 ▲
                   └── İKİ switch ───┘  ve Hit / HitAndDowned
                       ayrımı sarmalayıcının içine gömülür
```

### KAPSAM: aynı OLGU iki farklı cevap alabilir

Ayıran şey olgunun kendisi değil, hücreyi ya da birimi **KİMİN seçtiği**.

**KARŞI ÖRNEK** aynı dosyada,
[`PlaceStructure`'ın doluluk satırı](#placestructure-trygetunit): dolu hücre orada
bir `PlacementOutcome.RejectedCellOccupied`, yani bir oyun sonucu.
`Battle.AddUnit` ise TAM AYNI olguyu bir `ArgumentException` ile reddediyor.
Çelişki değil: orada hücreyi kayıt dosyası seçer, burada fare seçer. Gerekçenin
tamamı [`PlacementOutcome`](PlacementOutcome.md#rejectedcelloccupied) belgesinde.

### İŞ BÖLÜMÜ: üç arama, iki farklı sertlik

```
  RequireCombatant  "savaşçı mı"  false ► ArgumentException
  RequireCell       "nerede"      false ► ArgumentException
  TryGetStructure   "yapı mı"     false ► NORMAL cevap, dal döner
```

Üçü de aynı `Battle`'a soruyor; ayrışan şey `false`'un ne DEMEK olduğu. Sertliği
taşıyan satır bu metodun kendi gövdesinde duruyor:

```csharp
private static Combatant RequireCombatant(Battle battle, Unit unit, string paramName)
{
    if (!battle.TryGetCombatant(unit, out Combatant combatant))
    {
        throw new ArgumentException("The unit is not in this battle.", paramName); // ◄── SERTLİK
    }

    return combatant;
}
```

Üç karşı-olgu, üç ayrı düzenleme — hiçbiri "metodu sil" değil, çünkü beş çağrı
yeri var ve silinen metot derlenmez:

```
◄── işaretli throw kaldırılsa (bulunamayınca `combatant` null olarak dönse)
      tanınmayan birim null bir Combatant ile ilerler
   ─► AttackAction.Execute içinde attacker.AttackProfile okunur
   ─► NullReferenceException — sebebini SÖYLEMEYEN bir yerde

RequireCell'in yerine dönüşü yok sayılan bir battle.TryGetPosition konsa
      Battle.TryGetPosition bulunamayınca x = y = -1 yazıp false döner
   ─► mesafe (-1,-1) üzerinden ölçülür
   ─► saldırı sessizce "menzil dışı" der — ret DOĞRU görünür, sebebi yanlıştır

battle.TryGetStructure çağrısı, false'ta atan bir RequireStructure sarmalayıcısına
dönse
   ─► her yapı hedefi "bu savaşta değil" diye patlar
      ██ oysa baraka SAVAŞTA — normal bir cevap istisnaya çevrildi ██
```

### REDDEDILEN

Yeni bir `BattleOutcome` tipi doğar ve bu dosya onu döndürür:

```csharp
public enum BattleOutcome { RejectedUnknownUnit, Attacked, Moved }
```

**KIRILAN:** bir programcı hatası oyun sonucu kılığına girer.

```
hata artık patlamaz
  -> sessizce yutulabilen bir dala döner
  -> çağıran hem BattleOutcome hem AttackOutcome üstünde switch yazar ve
     Hit / HitAndDowned ayrımı sarmalayıcıya gömülür
derleyici: hiçbir şey der  ·  test: Attack_UnknownAttacker_Throws kırmızı
```

**KAZANIRDI:** emirler AĞDAN ya da bir tekrar kaydından gelseydi — orada "o birim
artık yok" beklenen bir ayrışmadır ve çökmek yerine reddedip senkronizasyon
istemek doğru davranıştır; `MoveAction`'ın kaynak hücre bloğundaki KAZANIRDI
satırı aynı kapıyı açık bırakıyor.

**TEK CUMLE:** Sonuç tipi OYUNCUNUN yapabildiklerini adlandırır; programcının
yapamayacağı şey istisnadır, çünkü onu ele almanın doğru bir yolu yoktur.

---

## RequireCell(Battle, Unit, string, out int, out int)

Tahtaya yalnız `Battle.AddUnit` ve `Battle.AddStructure` yazar; dolayısıyla
"tahtada durmuyor" ile "bu savaşta değil" **aynı cümledir** ve iki ayrı mesaj
yazmak o değişmezi ikinci bir yerde anlatmak olurdu.

Sertlik gerekçesi [`RequireCombatant`](#requirecombatantbattle-unit-string) ile
ortak; ayrıştığı tek nokta, dönüşü yok sayılan bir `TryGetPosition` konulsaydı
mesafenin `(-1,-1)` üzerinden ölçülecek olması.
