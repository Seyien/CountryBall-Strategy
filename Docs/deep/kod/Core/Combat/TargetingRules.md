# TargetingRules

> **Kaynak:** `Assets/Game/Core/Combat/TargetingRules.cs`
> **Ad alanı:** `GridStrategy.Combat` · **Assembly:** `GridStrategy.Combat` (`noEngineReferences: true`)
> **Rol:** Kural (Policy) — kimliği yok, hafızası yok, UYGUNLUK söyler; ne saldırır ne iyileştirir

"Bu yetenek bu hedefe uygulanabilir mi?" sorusunun tek sahibi.

Bu kural **üç kez reddedildi** ve her seferinde gerekçesi aynıydı: `Health`
hedefin ne olduğunu bilmemeli, `UnitLifecycle` kimin saldırdığını bilmemeli,
`AttackResolver` yalnızca mesafe ölçmeli. Geriye **üçüncü** bir sahip kaldı —
burası.

**Neden iki ayrı metot:** ayakta olan birim vurulabilir ama **diriltilemez** —
iki hedef kümesinin ayrıştığı nokta budur (düşmüş birim ikisine de açık, kalıcı
ölü ikisine de kapalı). Tek bir "uygun mu" metodu onu taşıyamazdı.

**İki durum dili konuşur:** `UnitState` ve `StructureState`. Bu bir tekrar
değil, `StructureState.cs`'te bilerek ödenen bedeldi ve şimdi ödendi.
Alternatifi tek bir enum'du ve onun bedeli her `switch`'te asla çalışmayan bir
`Downed` dalıydı — ikinci aşırı yüklemeyi derleyici ister, ölü dalı hiç kimse
istemez.

| Üye | Karar | Detay |
|---|---|---|
| `TargetingRules` (tip) | rol tablosu — yapının diriltme ikizi bir boşluk değil, CEVAP | [↓](#targetingrules-tip) |
| `CanBeAttacked(UnitState)` | girdisi ENUM olan kural nesnenin dışında yaşar; açık uçlu (`!= Dead`) | [↓](#canbeattackedunitstate-state) |
| `CanBeAttacked(UnitState, Team, Team)` | iki eksenli sürüm; durum kuralını KOPYALAMAZ, sorar | [↓](#canbeattackedunitstate-state-team-attackerteam-team-targetteam) |
| `CanBeAttacked(StructureState)` | KAPALI uçlu (`== Standing`) — yeni değeri sordurur | [↓](#canbeattackedstructurestate-state) |
| `CanBeAttacked(StructureState, Team, Team)` | taraf kuralı birim sürümüyle birebir aynı, o yüzden ortak | [↓](#canbeattackedstructurestate-state-team-attackerteam-team-targetteam) |
| `CanBeRevived(UnitState)` | ayrı metot: yetenek cevabı TERS çeviriyor, daraltmıyor | [↓](#canberevivedunitstate-state) |
| `CanBeRevived(UnitState, Team, Team)` | saldırının KOPYASI değil TERSİ — aynı takım ister | [↓](#canberevivedunitstate-state-team-reviverteam-team-targetteam) |
| `IsHostilePairing(Team, Team)` | enum'un sıfırıncı değeri ismen sorulmak zorunda | [↓](#ishostilepairingteam-attackerteam-team-targetteam) |

**İlgili anlatılar:** [05-yaşam döngüsü](../../../konular/05-yasam-dongusu.md) ·
[04-karar sırası](../../../konular/04-karar-sirasi.md)

---

## TargetingRules (tip)

### ROL TABLOSU — birebir eşleşmeyen satır en öğretici olandır

| | birim | yapı |
|---|---|---|
| saldırı | var | **VAR** |
| diriltme | var | **YOK** |

Yapının diriltme ikizi **eksik değil, yanlış olurdu**: yapı dirilmez.
`Structure.TryRepair` bir **onarımdır** — durum geçişi değil, yalnızca bir sayı
değişikliği — ve ön koşulunu ("ayakta olmayan onarılmaz") kendi içinde taşır.

### Yapının diriltme ikizi yok — ve bu, kararın kendisi

Yapı **dirilmez**: yıkık bina onarılmaz, yeniden **inşa** edilir ve yeniden
inşa bu enum'un bir geçişi değil, yepyeni bir `Structure` nesnesidir
(`StructureState.cs` ve `StructureLifecycle.cs` aynı kararı iki kez yazıyor).
`Structure.TryRepair` bir onarımdır ve diriltmeyle karıştırılmamalıdır:
diriltme bir **durum** geçişidir (yıkık → ayakta), onarım yalnızca bir **sayı**
değişikliğidir (can artar, durum aynı kalır).

#### HARİTA: rol tablosu ve ONUN BOŞ HÜCRESİ

```
              BİRİM                  YAPI
saldırı       CanBeAttacked(         CanBeAttacked(
                UnitState)             StructureState)
diriltme      CanBeRevived(          >> HÜCRE BOŞ <<
                UnitState)             ◄── KARARIN KENDİSİ

Boş hücrenin karşılığı yok değil; BAŞKA bir evde:
  Structure.TryRepair(amount)
    ├── ayakta değilse reddeder  ◄── ön koşul BURADA
    └── canı okur, canı yazar    ◄── bu dosyadan GÖRÜNMEYEN veri
```

#### KAPSAM: bu dosya ikiz eklemeyi genel olarak reddetmiyor

Karşı örnek aynı dosyada: saldırının yapı ikizi **var** ve eklenmesi doğruydu —
çünkü "kime vurulur" gerçekten bir hedefleme sorusudur ve girdisi yalnızca bir
enum ile bir taraftır. Ayıraç:

```
girdi = durum (+ taraf)     ► ikiz BU DOSYAYA eklenir
girdi = nesnenin kendi canı ► ikiz nesnenin İÇİNDE kalır
```

#### İŞ BÖLÜMÜ: hedefleme kuralı ile nesnenin DEĞİŞMEZİ

```
TargetingRules      ► "bu hedef seçilebilir mi" (durum + taraf)
Structure.TryRepair ► "bu nesne şu an onarılabilir mi" (durum + can)
```

Bölüşme, kopya değil: `TryRepair`'in kendi kelepçesi silinirse enkaz onarılır ve
buradaki hiçbir metot bunu göremez; buraya uydurma bir `CanBeRepaired`
eklenirse aynı ön koşul iki evde yaşar ve "Damaged da onarılır" günü sessizce
ayrışırlar — biri canı görür, öteki hiç görmez.

#### REDDEDILEN — simetri uğruna uydurma bir ikiz

```csharp
public static bool CanBeRepaired(StructureState state)
{
    return state == StructureState.Standing;
}
```

**KIRILAN:** aynı ön koşul iki yerde yaşar; bu metot kuralı devralmaz,
**çoğaltır**.

```
Structure.TryRepair'in `if (!IsStanding)` kelepçesi kaldırılamaz
burası canı hiç görmez -> "Damaged da onarılır" ikisini ayırır
çağıran, kuralın evet dediği onarımın neden false döndüğünü anlamaz
derleyici: hiçbir şey der  ·  test: iki taraf ayrı ayrı yeşil kalır
```

**KAZANIRDI:** onarım gerçekten **hedeflenen** bir yetenek olsaydı — menzilli
tamirci, alan etkili onarım büyüsü, düşman binasını onaramama kuralı.

**TEK CUMLE:** Eksik ikiz bir boşluk değil bir cevaptır: onarım bir hedefleme
sorusu değil, nesnenin kendi değişmezidir.

---

## CanBeAttacked(UnitState state)

Saldırı hedefleyebilir mi? `UnitState.Downed` **dahildir** — düşmüş birime
vurmak "işini bitirme" yoludur ve tasarımın parçasıdır. Buraya `Downed`'ı
kapatan bir satır koymak, `Combatant.TakeDamage` içinde reddedilen
`if (!health.HasRemaining) return;` ile aynı olurdu.

Kural bilerek **açık uçlu** (`state != UnitState.Dead`): burada varsayılan
cevabın EVET olması isteniyor. Kapalı uçlu ikizi yapı sürümündedir.

### HARİTA: kuralı sınamak için ne KURULUYOR

```
REDDEDILEN — kural Combatant'ın bir property'si olsaydı
  test ──► new Combatant(...)
             ├── Health(max)          ─┐
             ├── UnitLifecycle(...)    ├─ ÜÇÜ DE kurulmadan
             └── AttackProfile(d, r)  ─┘  kural okunamaz
           ◄── girdi artık bir NESNE GRAFI

SEÇİLEN — kural TargetingRules.CanBeAttacked(UnitState)
  test ──► UnitState.Alive
           UnitState.Downed   ◄── girdi kümesi TAM ve SONLU;
           UnitState.Dead         üç satırda tüketiliyor
```

### KAPSAM: her kural nesnenin dışına ÇIKMAZ

Ayıraç, kuralın girdisinin nesnenin **içinden** gelip gelmediğidir:

```
girdi bir enum          ► kural dışarıda yaşayabilir
girdi nesnenin kendi
alanları (can gibi)     ► kural nesnede kalmak zorunda
```

Karşı örnek aynı dosyanın kendi kararı: onarımın ön koşulu ("ayakta olmayan
onarılmaz") buraya **alınmadı**, `Structure.TryRepair`'in içinde bırakıldı —
çünkü onarım canı da okur ve can bu dosyadan görünmez. Gerekçesi
[yapının diriltme ikizi bölümünde](#targetingrules-tip). Yani aynı dosya hem
"dışarı çıkar" hem "içeride kalır" diyor; ayıran şey girdinin nereden
geldiğidir.

### İŞ BÖLÜMÜ: durumun SAHİBİ, durumun KAPISI, durumun KURALI

```
UnitLifecycle   ► durumu üretir ve geçişlerini sahiplenir
Combatant.State ► o durumu dışarıya OKUNUR kılar
TargetingRules  ► okunan durumu bir CEVABA çevirir
```

Üçü örtüşmüyor, zinciri bölüşüyor: bu metot silinirse durum yine üretilir ama
"vurulabilir mi" sorusunun sahibi kalmaz ve cevap her çağıranın içine
kopyalanır; `Combatant.State` silinirse kural ayakta kalır ama sorulacak girdiyi
kimse veremez.

### REDDEDILEN

`Combatant`'ın `State` property'sinin yanı:

```csharp
public bool CanBeAttacked => State != UnitState.Dead;
```

**KIRILAN:** kuralı sınamak için üç parçadan bir `Combatant` kurmak gerekir.

```
Health + UnitLifecycle + AttackProfile -> her satır nesne kurar
durum matrisi tek enum ile yazılamaz -> matris ölçüsünü kaybeder
derleyici: hiçbir şey der  ·  test: yeşil kalır, ölçtüğü şey daralır
```

**KAZANIRDI:** hedef uygunluğu enum dışında birimin iç durumuna bağlı olsaydı —
zırh, gizlenme, mermi — cevap durumdan değil nesneden gelirdi.

**TEK CUMLE:** Girdisi bir ENUM olan kural nesnenin dışında yaşayabilir, ve
dışarıda yaşayan kural nesne kurmadan sınanır.

---

## CanBeAttacked(UnitState state, Team attackerTeam, Team targetTeam)

Saldırı hedefleyebilir mi — durum **ve** taraf birlikte. Üç kural, bu sırayla:

1. `Team.None` **saldıramaz**. Tarafsız olan taraf tutmaz; duvar vurmaz.
2. Aynı takıma saldırılmaz — dost ateşi bu oyunda yok.
3. Geri kalanı durum kuralı, ve o kural burada **kopyalanmıyor**; tek
   parametreli sürüme soruluyor. Kopyalansaydı "Downed vurulabilir" kararı iki
   yerde yaşardı ve biri değiştiğinde diğeri sessizce eskirdi.

`Team.None` **hedef** olarak herkese açıktır — kimseye kapalı değil. Sebebi
`Team`'in `None`'ı niçin taşıdığıdır: yıkılabilir duvar, nötr kaynak düğümü,
tuzak. Yıkılamayan duvar dekordur ve dekorun `Health`'e ihtiyacı yoktur; kapalı
yapılsaydı tarafsız her şey sonsuza dek `AttackOutcome.RejectedInvalidTarget`
döner ve yolu duvarla kesilmiş bir yapay zekâ hiçbir çıkış bulamazdı.

### Tek parametreli sürüm neden silinmedi

`AttackAction` bu sürümü değil taraflı sürümü çağırır; tek parametreli sürüm
yine de silinmedi, çünkü durumu taraftan **bağımsız** soran gerçek çağıranları
var: `DownedUnit_IsStillAttackableButCannotAttack` ve
`DownedUnit_IsStillAttackableButCannotMove`, "aynı birim hedeftir ama eyleyen
değildir" asimetrisini takım parametresi hiç yazmadan sabitler.

#### HARİTA: hangi çağıran hangi ekseni ÖLÇÜYOR

```
çağıran                              sürüm        ölçtüğü
──────────────────────────────────   ──────────   ───────────
AttackAction.Execute (iki aşırı y.)  3 parametre  durum+taraf
CanBeAttacked(state, teams) gövdesi  1 parametre  durum
DownedUnit_IsStillAttackable
  ButCannotAttack                    1 parametre  durum
DownedUnit_IsStillAttackable
  ButCannotMove                      1 parametre  durum
Downed_IsTheOnlyState
  BothAbilitiesAccept                1 parametre  durum
                                     ◄── SİLİNİRSE BU SATIRLAR
                                         TARAF TAŞIMAK ZORUNDA KALIR
```

Tek parametreli sürüm ölü kod değil: birden çok gerçek çağıranı var ve hepsi
taraftan bağımsız soru soruyor.

#### KAPSAM: "tek eksenli sürümü koru" genel bir kural DEĞİL

Ayıraç, o ekseni yalnız başına soran **gerçek** bir çağıranın olup olmadığıdır.

Karşı örnek aynı dosyanın en altında: taraf ekseninin tek eksenli sürümü olan
`IsHostilePairing` `private` yazıldı — dışarıdan hiç sorulamaz, çünkü onu yalnız
başına soran bir çağıran **yok**. Aynı dosya, aynı iki eksen, zıt karar: durum
ekseni public kaldı, taraf ekseni kapatıldı.

#### İŞ BÖLÜMÜ: üç metot, üç ayrı soru

```
CanBeAttacked(state)        ► DURUM ekseni, tek başına
IsHostilePairing(teams)     ► TARAF ekseni, tek başına (yalnızca içeriden)
CanBeAttacked(state, teams) ► ikisini BİRLEŞTİRİR, metinlerini kopyalamadan
```

Üçüncüsü ilk ikisinin kopyası değil **çağıranıdır**. Tek parametreli sürüm
silinirse durum kuralı üç parametreli sürümün gövdesine kopyalanır ve "Downed
vurulabilir" kararı iki yerde yaşar; `IsHostilePairing` silinirse aynı taraf
kuralı birim ve yapı sürümlerinde iki kez yazılır; üç parametreli sürüm
silinirse `AttackAction` her iki aşırı yüklemede de iki soruyu ayrı ayrı sormak
zorunda kalır.

#### REDDEDILEN

Üstteki tek parametreli sürüm silinir, `AttackAction.Execute`'un birim hedefli
aşırı yüklemesindeki hedef uygunluğu çağrısı şuna döner:

```csharp
if (!TargetingRules.CanBeAttacked(target.State, attacker.Team, target.Team))
```

**KIRILAN:** durum kuralını taraftan **bağımsız** sormanın yolu kalmaz.

```
Downed_IsTheOnlyStateBothAbilitiesAccept taraf ölçmüyor
her satırına uydurma bir Player/Enemy çifti eklenir
"Downed vurulabilir" kararı takım gürültüsünün altında kalır
derleyici: hiçbir şey der  ·  test: yeşil kalır, ölçtüğü şey bulanır
```

**KAZANIRDI:** hedef uygunluğu **hiçbir** zaman taraftan bağımsız
sorulmayacaksa — o gün iki aşırı yükleme hangisinin gerçek kural olduğunu
belirsizleştirir.

**TEK CUMLE:** İki eksenli bir kuralın tek eksenli sürümü, o ekseni yalnız
başına sormak isteyen gerçek bir çağıran varsa silinmez.

---

## CanBeAttacked(StructureState state)

Bir **yapı** saldırı hedefleyebilir mi? Yalnızca `StructureState.Standing`.
Yıkılmış bir yapı geçerli hedef değildir: enkazın canı yoktur, vurulması hiçbir
şeyi değiştirmez.

Birim ikizinin **aynısı değil** ve olmaması bilinçli: orada kural
`state != Dead`, yani "düşmüş birim hâlâ vurulur" — çünkü işini bitirme yolu
tasarımın parçası. Yapıda öyle bir ara durum yok, o yüzden burada kural
**kapalı uçlu** yazıldı.

### HARİTA: enum büyüdüğünde iki biçimin cevabı

```
StructureState değeri   `== Standing`   `!= Destroyed`
─────────────────────   ─────────────   ──────────────
Destroyed               false           false
Standing                true            true
── bugünkü enum BURADA bitiyor ──────────────────────────────
Rubble    (yarın)       false           true   ◄── AYRIŞMA
Damaged   (yarın)       false           true   ◄── AYRIŞMA
Burning   (yarın)       false           true   ◄── AYRIŞMA
```

İki biçim bugün **aynı** cevabı veriyor; bu yüzden hiçbir test seçimi
koruyamaz. Fark yalnızca enum büyüdüğü gün doğar ve o gün kapalı uç derleyiciyi
değil **programcıyı** buraya çağırır: yeni değer varsayılan olarak
hedeflenebilir sayılmaz.

### KAPSAM: kapalı uç bu dosyanın genel biçimi DEĞİL

Ayıraç: yeni bir durum eklendiğinde varsayılan cevap ne **olmalı**?

```
varsayılan EVET olmalı  ► açık uç    (birim: != Dead)
varsayılan HAYIR olmalı ► kapalı uç  (yapı: == Standing)
```

Karşı örnek bu dosyanın en üstünde, birim ikizi: orada kural bilerek **açık**
uçlu yazıldı (`state != UnitState.Dead`), çünkü orada varsayılan cevabın EVET
olması isteniyor — düşmüş birime vurmak tasarımın parçası. Aynı dosya, aynı
soru, zıt biçim.

### İŞ BÖLÜMÜ: durum ekseni ile taraf ekseni

```
CanBeAttacked(StructureState)             ► DURUM
CanBeAttacked(StructureState, Team, Team) ► TARAF, sonra durumu buraya SORAR
```

Üç parametreli sürüm bu metodun kopyası değil **çağıranıdır**: bu metot
silinirse kapalı uç kararı onun gövdesine kopyalanır; o silinirse
`AttackAction`'ın yapı aşırı yüklemesi dost ateşini hiç soramaz.

### REDDEDILEN

Birim sürümünün şekli körü körüne kopyalanır:

```csharp
public static bool CanBeAttacked(StructureState state)
    => state != StructureState.Destroyed;
```

**KIRILAN:** açık uçlu kural, gelecekteki her yeni değeri **sessizce** kabul
eder.

```
bugün ikisi de aynı cevabı verir -> hiçbir test seçimi koruyamaz
Rubble geri gelir -> `!= Destroyed` enkazı geçerli hedef yapar
oyuncu enkaza ateş etmeye devam eder
derleyici: hiçbir şey der  ·  test: yeni değer eklenince de yeşil
```

**KAZANIRDI:** eklenecek her yeni durumun varsayılan olarak HEDEFLENEBİLİR
olması istenseydi — `Damaged`, `Burning`, `Reinforced` hepsi ayakta sayılsaydı.

**TEK CUMLE:** Açık uçlu kural yeni değeri kabul eder, kapalı uçlu kural onu
SORDURUR; enum büyüdükçe fark eden tek şey budur.

---

## CanBeAttacked(StructureState state, Team attackerTeam, Team targetTeam)

Bir yapı saldırı hedefleyebilir mi — durum **ve** taraf birlikte.

Taraf kuralı birim sürümüyle **birebir aynıdır** ve bu bir tesadüf değil: "kime
vurulur" sorusunun cevabı hedefin ne **olduğuna** değil hangi **tarafta**
olduğuna bağlıdır. Kural bu yüzden kopyalanmıyor, iki sürüm de aynı
`IsHostilePairing`'i soruyor.

Kendi barakanı yıkmak, kendi tankını vurmakla aynı hatadır; tarafsız duvar ise
iki sürümde de herkese açıktır — zaten `Team.None`'ın var olma sebebi odur.

---

## CanBeRevived(UnitState state)

Diriltme hedefleyebilir mi? Yalnızca `UnitState.Downed`: ayakta olanın
diriltmeye ihtiyacı yok, kalıcı ölü artık kurtarılamaz.

### HARİTA: iki hedef kümesi ve KESİŞİMİ

```
UnitState = { Alive, Downed, Dead }

  saldırı kümesi                diriltme kümesi
  ┌────────────────────┐        ┌───────────────┐
  │ Alive              │        │               │
  │         ┌──────────┼────────┼──────┐        │
  │         │  Downed  │ ◄── KESİŞİM   │        │
  │         └──────────┼────────┼──────┘        │
  └────────────────────┘        └───────────────┘
           Dead: İKİSİNİN DE DIŞINDA

Alive  ► vurulur, DİRİLTİLEMEZ  ◄── kümelerin ayrıştığı yer
Downed ► ikisine de açık         ◄── kesişim
Dead   ► ikisine de kapalı
```

İki metot bu **şeklin kendisidir**: tek metot olsaydı şekil bir enum
parametresinin arkasına düşer ve karşılaştırılamaz olurdu.

### KAPSAM: "parametre değil ayrı metot" genel bir kural DEĞİL

Ayıraç, ek parametrenin **cevabı** değiştirip değiştirmediğidir:

```
parametre cevabı DARALTIR     ► aşırı yükleme doğru
parametre cevabı TERS ÇEVİRİR ► ayrı metot doğru
```

Karşı örnek yukarıda, aynı dosyada: `CanBeAttacked`'ın üç parametreli sürümü tam
olarak "aynı soruya bir eksen daha ekle" biçimidir ve ayrı bir isim **almadı** —
çünkü taraf, durum kuralının cevabını değiştirmiyor, yalnızca önüne bir kapı
koyuyor. Yetenek ise cevabı **ters çeviriyor**: `Alive` için saldırı EVET,
diriltme HAYIR.

### İŞ BÖLÜMÜ: iki metot, iki küme

```
CanBeAttacked(UnitState) ► { Alive, Downed }  (açık uçlu yazım)
CanBeRevived(UnitState)  ► { Downed }         (kapalı uçlu yazım)
```

Kümeler kesişiyor ama metotlar bölüşüyor: `CanBeRevived` silinirse diriltme
"vurulabilir mi" sorusuna düşer ve ayakta olan birim diriltilebilir olur;
`CanBeAttacked` silinirse düşmüş birimin işini bitirme yolu kapanır. Kesişimin
tek elemanı `Downed` olduğu için iki hata da yalnızca `Alive`'da görünür — ve
`Downed_IsTheOnlyStateBothAbilitiesAccept` bu kesişimi sabitler.

### REDDEDILEN

`CanBeAttacked` ile birlikte tek metoda katlanır:

```csharp
public static bool IsValidTarget(UnitState state, AbilityKind kind)
    => kind == AbilityKind.Revive ? state == UnitState.Downed : state != UnitState.Dead;
```

**KIRILAN:** çağıran hangi yeteneği sorduğunu bir enum ile **taşımak** zorunda
kalır.

```
iki hedef kümesi tek metodun içine gizlenir -> karşılaştırılamaz
Downed_IsTheOnlyStateBothAbilitiesAccept ölçtüğü şeyi kaybeder
derleyici: hiçbir şey der  ·  test: yeşil kalır, kesişimi göremez
```

**KAZANIRDI:** yetenek sayısı ona çıksaydı ve her biri aynı durum kümesinden
farklı bir dilim isteseydi — on ayrı metot yerine tek tablo kazanırdı.

**TEK CUMLE:** Aynı hedef soran yeteneğe göre FARKLI cevap veriyorsa, yetenek
bir parametre değil ayrı bir metottur.

---

## CanBeRevived(UnitState state, Team reviverTeam, Team targetTeam)

Diriltme hedefleyebilir mi — durum **ve** taraf birlikte.

Taraf kuralı saldırının **kopyası değil, tersi**: saldırı FARKLI takım ister,
diriltme AYNI takımı. Düşmanını ayağa kaldırmak bir yetenek değil, bir hatadır
— ve bu metot var olmasaydı, yazılacak ilk diriltme yeteneği tam olarak o hatayı
yapardı, üstelik hiçbir test kırmızıya dönmeden.

`Team.None` burada **iki tarafta da kapalı** — ve bu, saldırıdaki kararla
bilerek çelişir. Tarafsız hedefe saldırılabilir çünkü yıkılabilir duvarın var
olma sebebi yıkılmaktır; tarafsız hedef diriltilemez çünkü duvarı ayağa
kaldırmak kimsenin yeteneği değildir. Tarafsız **dirilten** de yoktur: yalnız
"aynı takım" yazılsaydı `None == None` sınavı geçer ve bir duvar başka bir
duvarı diriltirdi.

---

## IsHostilePairing(Team attackerTeam, Team targetTeam)

Saldırının taraf kuralı — **tek yerde**. Birim ve yapı sürümleri bu metodu
ortak soruyor; kopyalasaydık "dost ateşi yok" kararı iki yerde yaşardı ve biri
değiştiğinde diğeri sessizce eskirdi. Kuralın metnini tek yerde tutmak, bu
dosyanın durum kuralı için zaten verdiği kararın aynısı.

Diriltmenin taraf kuralı **buraya katlanmadı** ve katlanmamalı: o kural bunun
kopyası değil **tersi** (aynı takım ister) ve `Team.None`'ı iki tarafta da
kapatır. Ortak bir "taraf kontrolü" metodu ikisini bir bayrakla birleştirseydi,
çağıran hangi yönü sorduğunu parametreyle taşımak zorunda kalır ve iki kuralın
ters olduğu gerçeği bir `bool`'un arkasına gizlenirdi.

### HARİTA: taraf matrisi — iki biçimin cevapları

Sol sütun saldıran, üst satır hedef. Hücre: "yalnız `!=` yazılsaydı / bugünkü
iki adımlı kural".

```
             hedef None    hedef Player   hedef Enemy
──────────   ───────────   ────────────   ────────────
sald. None    hayır/hayır   EVET/hayır     EVET/hayır ◄── SIZAN
sald. Player  evet/evet     hayır/hayır    evet/evet      SATIR
sald. Enemy   evet/evet     evet/evet      hayır/hayır

>> TEK AYRIŞAN SATIR: saldıran None <<
```

Bu satır teorik değil: `Team.cs` sıfırıncı değeri **bilerek** `None` yaptı, yani
takımı atanmayı unutulan her birim tam bu satıra düşer. `!=` biçimi onu "herkes
düşmanım" diye okur.

### KAPSAM: None her iki tarafta da kapatılmıyor

Burada yalnızca **saldıran** `None` kapatılıyor; hedef `None` bilerek **açık** —
yıkılabilir duvarın var olma sebebi yıkılmaktır. Kapalı olsaydı tarafsız her şey
sonsuza dek geçersiz hedef olurdu.

Karşı örnek aynı dosyada, `CanBeRevived`'ın üç parametreli sürümü: orada `None`
**iki** tarafta da kapatılıyor ve bu, buradaki kararla bilerek çelişiyor. Aynı
enum, aynı sıfırıncı değer, zıt karar — ayıraç yeteneğin yönü: saldırı tarafsız
bir hedefe **uygulanır**, diriltme uygulanmaz.

### İŞ BÖLÜMÜ: iki satır, matrisin iki ayrı bölgesi

```
`attackerTeam == Team.None`  ► matrisin İLK SATIRININ tamamı
`attackerTeam != targetTeam` ► KÖŞEGEN (aynı takım)
```

Aynı şeyi iki kez sormuyorlar: ilk satır silinirse tarafsız saldıran herkesi
vurur — dost ateşi kuralı atanmamış birimler için hiç çalışmaz; ikinci satır
silinirse dost ateşi tamamen açılır ve kendi tankını vurmak serbest olur. Biri
diğerinin yerini tutmaz.

### `Team.None`u bir "atanmadı" işareti sanmak

Enum'un sıfırıncı değeri C#'ta ayrıcalıklı **değildir**: `default(Team)` geçerli
bir `Team`'dir ve alanlar, diziler, `new Combatant[10]` gibi her toplu ayırma
onunla dolar. Ne derleyici ne de bir modifier bu durumu uyarır — o yüzden kural
onu **ismen** sormak zorunda.

### REDDEDILEN

Bu blok hiç yazılmaz, kural yalnız "aynı takım" olur:

```csharp
if (attackerTeam == targetTeam) { return false; }
return CanBeAttacked(state);
```

**KIRILAN:** takımı atanmayı unutulmuş birim — `Team.cs`'te sıfır bilerek
`None` — kendi tarafı **dahil** herkesi geçerli hedef görür.

```
birim vurur, testler yeşildir -> hata yalnız oyunda görülür
kendi okçun kendi tankını vurur -> sebebi hiçbir yerde yazmaz
derleyici: hiçbir şey der  ·  test: hiçbiri kırmızıya dönmez
```

**KAZANIRDI:** tarafsız ama **saldırgan** şeyler `Team.None` ile ifade
edilecekse — yaban canavarları, herkese ateş eden nötr kuleler.

**TEK CUMLE:** Enum'un sıfırıncı değeri atanmamış her alanın varsayılanıdır, o
yüzden kural onu ismen sormak zorundadır.
