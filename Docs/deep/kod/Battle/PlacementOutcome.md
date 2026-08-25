# PlacementOutcome

> **Kaynak:** `Assets/Game/Battle/PlacementOutcome.cs`
> **Ad alanı:** `GridStrategy.Battle` · **Assembly:** `GridStrategy.Battle`
> **Rol:** Tanım (Profile) — kimliği yok, hafızası yok, karar vermez adlandırır

Bir **yerleştirme** denemesinin sonucu.

**ÜÇ DEĞER, VE ÜÇÜ DE OKUNARAK BULUNDU** — uydurulmadı. Her biri bugün gerçekten
oluşabilen bir duruma karşılık geliyor:

- **tahta dışı hücre** — fare tahtanın kenarının dışındayken bırakılır;
  `UnitGrid.PlaceUnit` orada gürültüyle patlar, dolayısıyla akış onu çağırmadan
  ÖNCE sormak zorunda
- **dolu hücre** — `Battle.AddUnit` aynı durumu bir ÇAĞIRAN HATASI sayıyor;
  burada neden bir oyun sonucu olduğu [aşağıda](#rejectedcelloccupied) yazılı
- **yerleşti**

**DÖRDÜNCÜ** bir değer (`RejectedActorCannotAct`) yazılmadı ve bu bir unutma
değil — gerekçesi [`Placed`](#placed) başlığı altında.

| Üye | Karar | Detay |
|---|---|---|
| `RejectedInvalidCell` | sıfırıncı değer bilerek bir ret; adı "destination" değil | [↓](#rejectedinvalidcell) |
| `RejectedCellOccupied` | aynı olgu, iki cevap — ayrım hücreyi seçende | [↓](#rejectedcelloccupied) |
| `Placed` | üçlü kapalı bir küme; dördüncü değer bilerek yok | [↓](#placed) |

**İlgili anlatılar:** [06-sonuç enumları](../../konular/06-sonuc-enumlari.md) ·
[04-karar sırası](../../konular/04-karar-sirasi.md)

---

## RejectedInvalidCell

Sıfırıncı değer **bilerek bir RET**. Gerekçesi `MoveOutcome` ve `AttackOutcome`
tiplerinde yazılı; burada tekrarlanmıyor.

**ADI NEDEN `RejectedInvalidDestination` DEĞİL:** yerleştirmede giden bir şey yok.
"Hedef hücre" bir yolculuğun sonunu anlatır; burada hücre bir varış noktası değil,
doğum yeridir. Aynı adı kullanmak iki farklı olguyu tek kelimenin arkasına koyardı
ve ileride "yerleştirme de bir hareket midir" diye sorulmasına yol açardı —
cevabı hayır, çünkü hareketin bir KAYNAK hücresi vardır.

### HARİTA: iki olgu, iki ad

```
  HAREKET   [2,3] ──────────► [9,9]     kaynak VAR
            kaynak   hedef            ► RejectedInvalidDestination
  YERLEŞTİRME       (yok) ──► [9,9]     kaynak YOK
                             doğum yeri ► RejectedInvalidCell
                    ▲ >> AYRIM: bir yolculuğun sonu mu,
                      bir şeyin başlangıcı mı <<
```

---

## RejectedCellOccupied

**AYNI OLGU, İKİ FARKLI CEVAP** — ve fark kasıtlı. `Battle.AddUnit` dolu hücreyi
bir `ArgumentException` ile reddediyor; burada aynı durum bir oyun sonucu. Çelişki
değil, ÇAĞIRANIN farkı:

```
  AddUnit         çağıran hücreyi BİLEREK seçer (kayıt dosyası, seviye
                  dizilimi, spawn tablosu); dolu hücre onun kaydının
                  tahtayla ayrışması demektir
  PlaceStructure  çağıran hücreyi FARE ile seçer; dolu hücreye tıklamak
                  bozuk bir kayıt değil, sıradan bir hamle
```

Aynı ayrımı `MoveAction` zaten bir kez yapmıştı: kaynak hücre uyuşmazlığı patlar
(kayıt ayrışması), dolu hedef hücre ise `MoveOutcome` döndürür (sıradan bir
hamle).

### HARİTA: aynı olgu, iki cevap

```
                   OLGU: (x, y) dolu
                           │
            ┌──────────────┴───────────────┐
            ▼                              ▼
  Battle.AddUnit                  BattleActions.PlaceStructure
  hücreyi KAYIT seçti             hücreyi FARE seçti
  ╔═ ArgumentException ═╗         ╔═ RejectedCellOccupied ═╗
  ╚═════════════════════╝         ╚════════════════════════╝
  >> AYRIM NOKTASI olguda DEĞİL, hücreyi SEÇENDE <<
```

### KAPSAM: aynı akışta bile her ret bu tarafa geçmez

Ölçüt değişmiyor: o hücreyi ya da kimliği **KİM seçti?**

**KARŞI ÖRNEK** aynı metodun içinde, `PlaceStructure`'ın son satırı: "bu birim
zaten savaşta" hâli buraya bir dördüncü değer olarak TAŞINMADI,
`AddStructure`'ın istisnasına bırakıldı — çünkü aynı kimliği ikinci kez
yerleştirmek bir fare hamlesi değil, çağıranın kaydının `Battle`'ınkiyle
ayrışmasıdır. Tek metotta iki ret, iki farklı kanal; ölçüt her ikisinde de aynı.

### İŞ BÖLÜMÜ: üç kapı, üç ayrı cevap

```
  IsInsideGrid       ► tahta dışı   ► RejectedInvalidCell
  TryGetUnit(x,y)    ► dolu hücre   ► RejectedCellOccupied
  ThrowIfCannotJoin  ► kayıt ayrıştı► ArgumentException
```

Üçü ÖRTÜŞMÜYOR ve sıraları da bir karar: düzeltilemeyen sebep (tahta dışı)
beklemekle geçerli olabilecek sebepten (dolu hücre) önce söyleniyor. İlk kapı
silinirse `UnitGrid.PlaceUnit` gürültüyle patlar ve oyuncu tıklaması istisnaya
döner. İkincisi silinirse `ThrowIfCannotJoin` aynı olguyu istisna olarak bildirir
— yani reddedilen dünyaya bu enum korunarak da düşülebilir.

### REDDEDILEN

Bu enum hiç doğmaz; `PlaceStructure`, `Battle.AddStructure`'ın hatalarını olduğu
gibi geçirir:

```csharp
public static void PlaceStructure(Battle battle, Unit unit,
                                  Structure structure, int x, int y)
{
    battle.AddStructure(unit, structure, x, y);
}
```

**KIRILAN:** oyuncunun dolu bir hücreye tıklaması bir İSTİSNA olur.

```
BoardAdapter her bırakmayı try/catch ile sarar
  -> catch "hangi hata benim, hangisi oyuncunun" sorusunu mesaj metnine
     bakarak cevaplar
  -> bu enum string karşılaştırmasına dönüşür
derleyici: hiçbir şey der  ·  test: yeşil kalır
```

**KAZANIRDI:** yerleştirme yalnız EDİTÖRDEN yapılsaydı — seviye kurulumu bir
kayıt işidir ve orada dolu hücre gerçekten bozuk veridir; o gün gürültüyle
patlamak doğru davranıştır.

**TEK CUMLE:** Çağıran hatası ile oyuncu hamlesinin ayrımı `BattleActions`'ta
yazılı; burada aynı ayrım hücreyi KİMİN seçtiğine bakılarak uygulanıyor.

---

## Placed

Yapı tahtaya kondu ve savaşa katıldı. Bu değerin asıl konusu kendisi değil,
**yanında olmayan dördüncü değer**.

**DÖRDÜNCÜ DEĞER YAZILMADI** — ve "sıra kimde" sorusunun burada sorulmaması bir
eksiklik değil, bir karardır.

### HARİTA: imzada EYLEYEN var mı

```
  Attack / Move / Revive          PlaceStructure
  ┌── EYLEYEN ──┐                 ┌── EYLEYEN ──┐
  │ Unit        │                 │    YOK      │
  └──────┬──────┘                 └──────┬──────┘
         │ Combatant.Team                │ >> ödünç alınacak
         ▼                               ▼    tek alan: <<
  TurnRules.CanAct(team, ...)      structure.Team
                                         │
  Structure.Team bir SAHİPLİK değil, bir AİDİYETtir:
  nötr duvar ──► Team.None ──► CanAct HER ZAMAN false
             ──► >> nötr hiçbir yapı tahtaya bir daha konamaz <<
```

### KAPSAM: bu değer başka yerlerde DOĞRU

Ölçüt: imzada, kuralın sorulacağı bir eyleyen duruyor mu?

**KARŞI ÖRNEK** aynı ad alanında,
[`ReviveOutcome.RejectedActorCannotAct`](ReviveOutcome.md#rejectedactorcannotact):
orada aynı değer var ve doğrusu bu — `Revive`'ın imzasında `reviver` diye gerçek
bir eyleyen duruyor, hatta değer İKİ sebebi birden taşıyor (sıra ve diriltenin
durumu). Yani reddedilen şey değerin kendisi değil, öznesi olmayan bir imzada
aranması.

### İŞ BÖLÜMÜ: üç değer, üç kapı, artan yok

```
  RejectedInvalidCell   ◄─► IsInsideGrid
  RejectedCellOccupied  ◄─► TryGetUnit(x, y)
  Placed                ◄─► AddStructure döndü
```

Eşleşme birebir: her değerin tam olarak bir üreteni var ve `PlaceStructure`'da bu
üçünün dışında sonuç döndüren satır yok. Dördüncü değer eklenirse eşi olmayan bir
değer doğar — çağıran asla dönmeyecek bir dal yazar; dördüncü kapı eklenirse
(sıra kuralı) yukarıdaki nötr kırılması doğar. Üçlü bugün KAPALI bir küme ve
kapalılığı bir tesadüf değil.

### REDDEDILEN

Sıra kuralı yerleştirmeye de uygulanır ve dördüncü bir değer doğar:

```csharp
RejectedActorCannotAct
// ve BattleActions.PlaceStructure içinde:
if (!TurnRules.CanAct(structure.Team, battle.Turn.Current))
{
    return PlacementOutcome.RejectedActorCannotAct;
}
```

**KIRILAN:** imzada EYLEYEN yok; satır, yapının tarafını eyleyenin tarafı
sanıyor.

```
Structure.Team bilerek Team.None olabilir (tarafsız duvar, kapı)
  -> tarafsız hiçbir sırada eyleyemez
  -> nötr hiçbir yapı tahtaya bir daha konamaz
derleyici: hiçbir şey
test: PlaceStructure_NeutralStructureOutOfTurn_IsStillPlaced kırmızı
```

**KAZANIRDI:** yerleştirme gerçekten bir TUR EYLEMİ olduğu gün — sınırlı inşa
hakkı, kaynak maliyeti, "turda bir bina" kuralı; o gün imza gerçek bir eyleyen
alır (`Unit builder` ya da `Team placingTeam`) ve kural o eyleyene sorulur.

**TEK CUMLE:** Olmayan özneyi bir başkasının tarafından ödünç almak, doğru kuralı
yanlış şeye sormaktır — S-15'in cümlesi burada birebir geçerli.
