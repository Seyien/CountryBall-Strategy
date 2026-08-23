# AttackRules

> **Kaynak:** `Assets/Game/Core/Combat/AttackRules.cs`
> **Ad alanı:** `GridStrategy.Combat` · **Assembly:** `GridStrategy.Combat` (`noEngineReferences: true`)
> **Rol:** Kural (Policy) — kimliği yok, hafızası yok, UYGUNLUK söyler

"Bu birim saldırabilir mi?" sorusunun tek sahibi.

Bu tipin var olma sebebi **ölçülmüş bir boşluktu**: `TargetingRules` "kime
vurulur"u, `MovementRules` "kim yürür"ü sahiplendi; "kim VURUR" sorusunu ise
kimse cevaplamıyordu. Sonucu bir oynanış hatasıydı — düşmüş bir birim hâlâ
vurabiliyordu, çünkü `AttackAction` hedefin durumunu soruyor, **saldıranın**
durumunu sormuyordu.

Neyi **bilmez**: hedefin kim olduğunu (`TargetingRules`'ın işi), menzili
(`AttackResolver`'ın işi), sıranın kimde olduğunu (`TurnRules`'ın işi, ve o
kural bir üst katmanda yaşıyor).

| Üye | Karar | Detay |
|---|---|---|
| `AttackRules` (tip) | kural başına bir sahip; takım aşırı yüklemesi bilerek YOK | [↓](#attackrules-tip) |
| `CanAttack(UnitState)` | beyaz liste — yalnızca `Alive`; türetilmez, kendi metnini taşır | [↓](#canattackunitstate-attackerstate) |

**İlgili anlatılar:** [04-karar sırası](../../../konular/04-karar-sirasi.md)

---

## AttackRules (tip)

### Neden MovementRules'a ikinci bir metot değil

Projenin kendi yerleşik örneği `DamageRules` / `HealingRules`. İkisi de tek
metotlu, ikisi de "ne kadar" ailesinden ve ikisi de **ayrı** dosyalar. Kural
başına bir sahip; aynı ailedendir diye bir araya getirilen iki kural, biri
değiştiğinde diğerini de düzenlemek zorunda bırakır ve dosyanın adı hangisinin
sahibi olduğunu söylemez olur.

### Takım aşırı yüklemesi bilerek YOK

"Kime vurabilirim" iki tarafın sorusudur ve onun sahibi zaten var:
`TargetingRules`'ın üç parametreli `CanBeAttacked`'ı. "Ben vurabilir miyim" ise
tek taraflıdır — S-15'in hareket için verdiği kararın birebir ikizi.

#### HARİTA: üç soru, üç ev, üç ayrı veri

```
soru                     okuduğu veri   sahibi
──────────────────────   ────────────   ──────────────────────
"ben vurabilir miyim"    UnitState      AttackRules ◄── BURASI
                         (tek taraf)
"kime vurabilirim"       Team × Team    TargetingRules
                         (iki taraf)    .IsHostilePairing
"sıra bende mi"          TurnState      TurnRules
                         (tur durumu)   (GridStrategy.Battle —
                                        bu assembly'den GÖRÜNMEZ)

██ AYRIŞMA NOKTASI ██ Team parametresi eklenirse bu metot iki taraflı
soruların İKİNCİ evi olur ve "tarafsız ama SALDIRGAN şeyler" günü hangi
evin güncelleneceği belirsizleşir.
```

#### KAPSAM: taraf sorusu bu ad alanında yasak DEĞİL

Karşı örnek aynı ad alanında: `TargetingRules.CanBeAttacked`'ın taraf aşırı
yüklemesi **var** ve olması doğru — çünkü orada soru zaten iki taraflı; "hedef"
kelimesi ikinci tarafı ima ediyor. Ayıraç, aşırı yüklemenin sorunun **taraf
sayısını** değiştirip değiştirmediğidir: orada değiştirmiyor, burada
değiştirirdi.

#### İŞ BÖLÜMÜ: silinirse ne kırılır

```
AttackRules.CanAttack silinirse ► düşmüş birim vurur
IsHostilePairing silinirse      ► kendi tankını vurur
TurnRules silinirse             ► sırası olmayan vurur
```

Üç kırılma da farklı ve hiçbiri diğerini kapatmaz. **GARANTİ NEREDE BİTER:**
üçüncü satırda görünüyor — bu assembly'nin `references` listesi boş, yani
`TurnState` buradan hiç görünmez ve o kural ancak bir üst katmanda
yaşayabilir.

#### REDDEDILEN

```csharp
public static bool CanAttack(UnitState attackerState, Team attackerTeam)
{
    if (attackerTeam == Team.None) { return false; }
    return CanAttack(attackerState);
}
```

**KIRILAN:** takım aşırı yüklemesi reddinin tamamı `MovementRules`'ta; buradaki
**fark**, ikinci evin `TurnRules` değil `TargetingRules.IsHostilePairing`
olması — tarafsız ama SALDIRGAN şeyler (yaban canavarı, herkese ateş eden nötr
kule) eklendiği gün orası güncellenir, burası kalır.

```
derleyici: hiçbir şey der  ·  test: hiçbiri kırmızıya dönmez
```

**KAZANIRDI:** saldıranın durumu ile tarafı TEK bir soruda birleşseydi — "bu
birim bu tur zaten vurdu mu" da aynı yerden gelseydi.

**TEK CUMLE:** "Kime vurabilirim" iki tarafın sorusudur ve sahibi başkası; "ben
vurabilir miyim" tek taraflıdır ve sahibi burası.

---

## CanAttack(UnitState attackerState)

Saldırabilir mi? Yalnızca `UnitState.Alive`.

`UnitState.Downed` için cevap **hayır** ve bu `TargetingRules.CanBeAttacked`
ile bilerek çelişir: düşmüş birim hâlâ geçerli bir **hedeftir** ama artık bir
**saldıran** değildir. Aynı asimetri hareket tarafında da var
(`MovementRules.CanMove`) ve sebebi tektir — düşmek oyundan çıkmak değil,
**oynamayı** bırakmaktır.

`UnitState.Dead` için cevap da hayır, ama sebebi farklı: `Downed` bir kural
gereği durdurulur, `Dead` zaten oyunda değildir. İki hayırın aynı olması bir
tesadüftür, bir kural değil.

Beyaz liste biçimi — `MovementRules.CanMove` ve `TargetingRules.CanBeRevived`
ile aynı şekil, aynı gerekçe. Dördüncü bir durum (`Stunned`, `Petrified`,
`Fleeing`) eklendiği gün bu metot onu **sessizce** saldırgan saymaz; kararı
yeniden vermek için buraya gelinir.

### HARİTA: enum büyüdüğünde iki biçimin cevabı

```
UnitState değeri    `== Alive`   `!= Downed && != Dead`
─────────────────   ──────────   ──────────────────────
Alive               true         true
Downed              false        false
Dead                false        false
── bugünkü enum BURADA bitiyor ─────────────────────────────
Stunned   (yarın)   false        true   ◄── AYRIŞMA
Petrified (yarın)   false        true   ◄── AYRIŞMA
Fleeing   (yarın)   false        true   ◄── AYRIŞMA
```

Bugün iki biçim **aynı** cevabı verir; bu yüzden hiçbir test seçimi koruyamaz.
Beyaz listenin kazandığı tek şey, dördüncü değerin eklendiği gün kararın
**yeniden verilmesi** zorunluluğu.

### KAPSAM: beyaz liste bu ad alanının tek biçimi DEĞİL

Ayıraç, yeni bir durum eklendiğinde varsayılan cevabın ne **olması**
gerektiğidir:

```
varsayılan HAYIR ► beyaz liste (CanAttack, CanMove, CanBeRevived)
varsayılan EVET  ► kara liste  (TargetingRules.CanBeAttacked)
```

Karşı örnek aynı ad alanında ve bu metotla doğrudan çelişen bir satır:
`TargetingRules.CanBeAttacked(UnitState)` bilerek **kara** liste (`state !=
UnitState.Dead`), çünkü orada düşmüş birimin hedef olarak kalması tasarımın
parçası. Aynı enum, aynı üç değer, zıt biçim — ve o zıtlık bu dosyanın özet
bloğunda "bilerek çelişir" diye zaten yazılı.

### İŞ BÖLÜMÜ: EYLEYEN kuralı ile HEDEF kuralı

```
AttackRules.CanAttack        ► saldıran OLABİLİR mi
TargetingRules.CanBeAttacked ► hedef OLABİLİR mi
```

Aynı enum'u okuyup zıt cevap verirler; asimetri tam da budur. `CanAttack`
silinirse "düşmüş birim vurabilir" hatası geri gelir — bu tipin var olma sebebi
o boşluktu; `CanBeAttacked` silinirse düşmüş birimin işini bitirme yolu
kapanır. Biri diğerinden türetilemez ve sebebi aşağıdaki ikinci reddedilen
alternatifte yazılı.

### REDDEDILEN — kara liste biçimi

```csharp
return state != UnitState.Downed && state != UnitState.Dead;
```

**KIRILAN:** gerekçenin tamamı `MovementRules`'taki aynı reddin içinde yazılı;
buradaki **fark** yalnızca yüklem — yeni durum varsayılan olarak YÜRÜYEBİLİR
değil SALDIRABİLİR sayılır ve sersemletilmiş birim vurmaya devam eder.

```
derleyici: hiçbir şey der  ·  test: hiçbiri kırmızıya dönmez
```

**KAZANIRDI:** `MovementRules`'takiyle aynı eşik — saldırabilenler ÇOĞUNLUK
olsaydı.

**TEK CUMLE:** Üç kuralda tekrarlanan şey BİÇİMdir, gerekçe değil; gerekçe tek
evde durur.

### REDDEDILEN — kural hareket kuralından DEVRALINIR

```csharp
return MovementRules.CanMove(state);
```

#### HARİTA: iki kuralın zaman içindeki yolu

```
bugün    CanMove(state) ═══ CanAttack(state)
         (yalnız Alive)     (yalnız Alive)
               ▲ iki kural AYNI cevabı veriyor

"yaralı birim yürüyemez ama vurabilir" dendiği gün:

  TÜRETİLMİŞ olsaydı
    CanMove  ── değişir ───► Wounded: false
    CanAttack ─ SÜRÜKLENİR ► Wounded: false  ◄── SESSİZ HATA
    derleyici: susar · test: CanAttack'in kendi testi zaten
    yoktur, çünkü kural kendi metni olmayan bir yankıdır

  BAĞIMSIZ yazıldığı için (bugünkü şekil)
    CanMove  ── değişir ───► Wounded: false
    CanAttack ─ DEĞİŞMEZ ──► Wounded: true   ◄── AYRIŞMA
                             kararı programcı verir
```

#### KAPSAM: bu ad alanında delegasyon yasak DEĞİL

Ayıraç, iki metodun **aynı** kural mı yoksa bugün aynı cevabı veren **iki**
kural mı olduğudur:

```
aynı kural, ikinci eksen ► delege et
iki kural, aynı cevap    ► ayrı yaz
```

Karşı örnek aynı ad alanında: `TargetingRules`'un üç parametreli
`CanBeAttacked`'ı durum kuralını **kopyalamaz**, tek parametreli sürüme
**sorar** — ve orada delegasyon doğrudur, çünkü ikisi tek bir kuralın iki
eksenidir; ayrışacakları bir gün yoktur. Burada ise "yürümek" ile "vurmak" iki
ayrı fiil ve ayrışma günü oyun diliyle adlandırılabiliyor.

#### İŞ BÖLÜMÜ: bir fiil, bir sahip

```
MovementRules.CanMove ► "yürüyebilir mi"
AttackRules.CanAttack ► "vurabilir mi"
```

Metinleri bugün aynı, sahipleri ayrı. `CanAttack` silinip yerine `CanMove`
çağrılsaydı hiçbir test kırmızıya dönmez ama saldırı kuralı artık kendi evinde
yaşamazdı; `MovementRules` silinseydi hareket kuralı bu dosyaya taşınmak
zorunda kalır ve dosyanın adı hangisinin sahibi olduğunu söylemez olurdu —
projenin `DamageRules` / `HealingRules` ayrımıyla birebir aynı gerekçe.

**KIRILAN:** türetme reddinin tamamı `MovementRules`'ta yazılı; buradaki
**fark**, ayrılma gününün oyun diliyle adının konmuş olması — "yaralı birim
yürüyemez ama vurabilir" dendiği gün saldırı kuralı hareket kuralının
kuyruğunda sessizce sürüklenir.

```
derleyici: hiçbir şey der  ·  test: hiçbiri kırmızıya dönmez
```

**KAZANIRDI:** tasarım "eyleyebilmek" diye TEK bir kavrama inseydi — o gün adı
ne `CanMove` ne `CanAttack` olurdu.

**TEK CUMLE:** Türetme, iki kuralın ayrıştığı günü hiçbir test kırmadan
geçirir.
