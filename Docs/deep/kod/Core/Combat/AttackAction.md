# AttackAction

> **Kaynak:** `Assets/Game/Core/Combat/AttackAction.cs`
> **Ad alanı:** `GridStrategy.Combat` · **Assembly:** `GridStrategy.Combat` (`noEngineReferences: true`)
> **Rol:** Kural (Policy) — kimliği yok, KENDİ hafızası yok (hedefi değiştirir); AKIŞI yürütür ama kuralların hiçbirini kendisi yazmaz

Saldırı akışının tek sahibi — ve bu dosyanın var olma sebebi tek cümle: parçalar
hazırdı ama **kimse "saldır" demiyordu**.

`AttackResolver` menzili ölçer, `TargetingRules` uygunluğu söyler, `Combatant`
ya da `Structure` hasarı uygular. Hiçbiri diğerini **tanımaz** ve tanımamalıdır.
Onları bir sıraya dizen tek yer burasıdır.

**Hafızası yoktur ama ölçüsü "aynı üçlü aynı sonucu verir" değildir:**
`Execute` hedefi **değiştirir**. 20 canlı bir hedefe 10 hasarla arka arkaya
iki kez çağır — birincisi `Hit`, ikincisi `HitAndDowned` döner. Farkı doğuran
şey burada saklanan bir alan değil, hedefin kendi canıdır.

**İki hedef tipi, iki aşırı yükleme — tek akış şekli.** Birim ve yapı aynı
sırayı paylaşır (saldıranın durumu → hedef uygunluğu → menzil → hasar) çünkü
sıra kararı hedefin ne olduğuna bağlı değil; ayrıştıkları iki nokta da hedefin
doğasından gelir.

**Takım sorusunu artık kendisi sorar.** Eskiden sormuyordu ve dost ateşi kuralı
yalnızca `BattleActions`'ta yaşıyordu — yani aynı kural iki katmanda iki farklı
cevap veriyordu. Kural indi.

**Saldıranın kendi durumunu da artık kendisi sorar.** Bu, takım sorusunun ikizi
olan ikinci bir borcun kapanışıdır: hedefin durumu soruluyordu, **saldıranın**
durumu sorulmuyordu ve düşmüş bir birim hâlâ vurabiliyordu. Kuralın sahibi
`AttackRules`; soru burada soruluyor çünkü kuralı **uygulayabilen** en alt
katman bu tip.

Neyi **bilmez**: mesafenin nasıl ölçüldüğünü (hazır alır — aynı desen
`AttackResolver`'da da var), sırayı kimin verdiğini, sonucu kimin göstereceğini,
saldırının kaç kez tekrarlanacağını.

| Üye | Karar | Detay |
|---|---|---|
| `AttackAction` (tip) | İKİ bileşiğin arasındaki kural ikisinin de DIŞINDA durur | [↓](#attackaction-tip) |
| `Execute(Combatant, Combatant, int)` | sıra bir CEVAP kararıdır; "düştü mü" bir DEĞİŞİM sorusudur | [↓](#executecombatant-attacker-combatant-target-int-distance) |
| `Execute(Combatant, Structure, int)` | aynı sıra, iki ayrışma; simetri bir gerekçe değildir | [↓](#executecombatant-attacker-structure-target-int-distance) |

**İlgili anlatılar:** [02-assembly duvarı](../../../konular/02-assembly-duvari.md) ·
[04-karar sırası](../../../konular/04-karar-sirasi.md)

---

## AttackAction (tip)

### HARİTA: ok = "derlemek için TANIMAK zorunda"

```
REDDEDILEN — metot Combatant'ın İÇİNE taşınırsa
  Combatant ──────► TargetingRules    (uygunluğu sormak için)
      │    ──────► AttackResolver    (menzili sormak için)
      └──────────► ikinci bir Combatant  (hedef)
  ◄── AYRIŞMA NOKTASI: bileşiğin sınırı "kendi parçalarım"dan
      "tahtadaki herkes"e genişledi

SEÇİLEN — akış, ikisinin de DIŞINDA duran static bir tipte
  AttackAction ──► AttackRules      "kim VURUR"
       │      ──► TargetingRules   "kime VURULUR"
       │      ──► AttackResolver   "ULAŞIR mı"
       └──────► Combatant / Structure   (yalnızca veri okur)
  Combatant ────► (ok yok)   ◄── ÜÇ KURALDAN HİÇBİRİNİ TANIMAZ
  Structure ────► (ok yok)   ◄── ÜÇ KURALDAN HİÇBİRİNİ TANIMAZ
```

Fark bir üslup tercihi değil bir **yön** farkı: seçilen şekilde oklar hep
akıştan kurala akar ve hiçbir bileşik ikinci bir bileşiği tanımak zorunda
kalmaz.

### KAPSAM: "kural nesnenin DIŞINDA durur" genel bir kural DEĞİL

Ayıraç, sorunun kaç nesnenin durumunu okuduğudur:

```
tek nesne ► kural nesnenin İÇİNDE      (durum + can birlikte)
iki nesne ► kural İKİSİNİN DE DIŞINDA  (saldıran + hedef)
```

Karşı örnek aynı dosyada, yapı aşırı yüklemesinin sonunda: "bu vuruş mu yıktı"
kararı buraya **alınmadı**, `Structure.TakeDamage`'ın dönüş değerinde bırakıldı
— o soru tek bir nesnenin kendi parçaları arasındaki kuraldır. Oradaki
reddedilen alternatif tam olarak kararı buraya çekmeyi reddediyor. Yani bu dosya
iki kararı da veriyor; ayıran şey **nesne sayısıdır**, alışkanlık değil.

### İŞ BÖLÜMÜ: SIRA sahibi ile KURAL sahipleri ÖRTÜŞMEZ

```
AttackAction   ► hangi soru, hangi SIRAYLA (kural metni yazmaz)
AttackRules    ► "saldıran eyleyebilir mi" kuralının METNİ
TargetingRules ► "hedef uygun mu" kuralının METNİ
AttackResolver ► "menzile giriyor mu" kuralının METNİ
```

`AttackAction` silinirse üç kural ayakta kalır ama kimse "saldır" demez — bu
dosyanın var olma sebebi zaten o boşluktu. Üç kuraldan biri silinirse akış
ayakta kalır ama o kuralın metni `Execute`'un içine kopyalanır ve **iki** aşırı
yükleme tarafından iki kez tekrarlanır.

### `static` BU AYRIMI SAĞLAMAZ

`static` olmak yalnızca kimlik ve hafıza yokluğunu söyler, okların **yönünü**
söylemez: `Combatant`'a eklenecek bir `Attack(...)` metodu da static bir kurala
delege edebilirdi ve ayrışma yine olurdu. Koruma `Combatant`'ın bu üç kurala
**hiç ok açmamasından** geliyor.

**GARANTİ NEREDE BİTER:** orası. Aynı assembly'de yaşayan herhangi bir tip yarın
o oku açabilir ve ne derleyici ne `.asmdef` uyarır.

### REDDEDILEN

Bu sınıf hiç doğmaz, metot `Combatant`'a taşınır:

```csharp
public AttackOutcome Attack(Combatant target, int distance)
{
    if (!TargetingRules.CanBeAttacked(target.State)) ...
}
```

**KIRILAN:** bileşiğin sınırı "kendi parçalarım"dan "tahtadaki herkes"e
genişler.

```
Combatant TargetingRules ve AttackResolver'ı tanımak zorunda kalır
saldırıyı sınamak için HER ZAMAN iki tam Combatant kurulur
derleyici: hiçbir şey der  ·  test: kurulum maliyeti sessizce iki katı
```

**KAZANIRDI:** saldırı saldıranın **iç** durumuna bağlı olsaydı — bekleme
süresi, öfke birikimi, mermi sayısı — kural alanları okumak zorunda kalırdı.

**TEK CUMLE:** Bileşik kendi parçaları arasındaki kuralı yürütür; İKİ bileşik
arasındaki kural ikisinin de dışında durmak zorundadır.

---

## Execute(Combatant attacker, Combatant target, int distance)

Bir saldırı denemesini yürütür ve ne olduğunu döndürür. Mesafe **dışarıdan**
gelir; bu tip iki birimin nerede durduğunu bilmez.

### Sıra bir CEVAP kararıdır

#### HARİTA: erken çıkış merdiveni ve her basamağın SAHİBİ

Her basamak, çağıranın o cevaba karşı ne yapabileceğiyle birlikte okunur.
Merdiven düzeltilemeyenden düzeltilebilire doğru iner.

```
0   attacker/target null ► throw — çağıran HATASI, cevap değil
>> BURADAN SONRASI CEVAPTIR <<
1   AttackRules.CanAttack(attacker.State)
    ► RejectedActorCannotAct  çağıran DÜZELTEMEZ: hedef de hücre de
      değişse cevap aynı
2   TargetingRules.CanBeAttacked(state, teams)
    ► RejectedInvalidTarget   çağıran BAŞKA HEDEF seçebilir
3   AttackResolver.IsWithinRange(distance, profile)
    ► RejectedOutOfRange      çağıran YAKLAŞABİLİR
4   target.TakeDamage(...)    ► Hit / HitAndDowned
```

Basamak 1 yukarı çıktığı için değil, 2 ve 3'ün **önüne geçtiği** için doğru: 2
ya da 3 önce cevaplasaydı çağıran boş yere hedef değiştirir ya da yaklaşır, her
seferinde 1'e çarpardı.

#### KAPSAM: merdiven yalnızca CEVAP DÖNEN basamaklar için

Karşı örnek aynı metodun hemen başında: iki null kontrolü bu sıralamaya **tabi
değil** ve olamaz da — onlar bir `AttackOutcome` döndürmez,
`ArgumentNullException` atar. "Çağıranın düzeltemeyeceği sebep önce" ilkesi
**cevapları** sıralar; atılan istisna bir cevap değil sözleşmenin ihlalidir.
Null kontrollerinin en başta olması bu kuralın örneği değil, kapsam dışındaki
komşusudur.

#### İŞ BÖLÜMÜ: kuralın METNİ ile kuralın YERİ

```
AttackRules.CanAttack ► kuralın METNİ (yalnızca Alive)
o satırın konumu      ► kuralın SIRASI (1. basamak)
```

İkisi ayrı ayrı bozulabilir ve belirtileri farklıdır: `AttackRules` silinirse
"kim vurur" sorusunun sahibi kalmaz ve metin buraya kopyalanır; o **satır**
aşağı taşınırsa kural ayakta kalır, metni değişmez, yalnızca cevap sırası
bozulur — ve
`Execute_DownedAttackerOutOfRange_PrefersActorCannotActOverOutOfRange` tam
olarak ikinci kırılmayı yakalamak için yazıldı.

Saldıranın durumu sorusunun **yeri** burası, bir üst katman değil: kuralı
uygulayabilen en alt katman bu tip, çünkü kural `UnitState`'i sormak zorunda ve
`UnitState` bu ad alanında yaşıyor. `BattleActions`'ta sorulsaydı bu metodu
**doğrudan** çağıran (yapay zekâ, tekrar kaydı, gelecekteki ikinci bir akış)
düşmüş birimle vurmaya devam ederdi ve hiçbir test kırmızı olmazdı — dost ateşi
borcunun aynısı, bir kural sonra.

**Alternatif:** soruyu `BattleActions`'ta sormak. Seçilmedi: ikinci bir çağıran
aynı boşluğu sessizce yeniden açar.

#### REDDEDILEN — blok hedef ve menzil kontrollerinin ALTINA taşınır

```csharp
if (!TargetingRules.CanBeAttacked(target.State, attacker.Team, target.Team)) ...
if (!AttackResolver.IsWithinRange(distance, attacker.AttackProfile)) ...
if (!AttackRules.CanAttack(attacker.State))
{
    return AttackOutcome.RejectedActorCannotAct;
}
```

**KIRILAN:** cevap, çağıranı düzeltemeyeceği bir şeyi düzeltmeye çağırır.

```
düşmüş birim geçersiz hedefe vurur -> cevap "geçersiz hedef"
çağıran hedefi değiştirir, yaklaşır -> her seferinde reddedilir
yapay zekâ bütün hedef listesini boşuna tarar
derleyici: hiçbir şey der  ·  test: Execute_DownedAttackerOutOfRange_
PrefersActorCannotActOverOutOfRange tam bunun için yazıldı
```

**KAZANIRDI:** saldıranın durumunu okumak PAHALI, hedef uygunluğu ucuz olsaydı —
ucuz eleme önce yapılır; bugün ikisi de bir enum karşılaştırması.

**TEK CUMLE:** Sıra bir kurgu değil bir CEVAP kararıdır: çağıranın
düzeltemeyeceği sebep en önce söylenir.

### Kural, uygulayabilen en alt katmanda sorulur

Takım sorusu burada soruluyor — ve bu, bir borcun kapanışıdır. Önceden bu satır
tek parametreli `CanBeAttacked(UnitState)`'ti ve takımı hiç sormuyordu; dost
ateşini yalnızca `BattleActions` engelliyordu. Aynı kural iki katmanda iki
farklı cevap veriyordu: akıştan geçen saldırı reddediliyor, bu metodu
**doğrudan** çağıran kendi takımını vurabiliyordu. Kural artık tek katmanda
yaşıyor.

#### HARİTA: kapının YERİ kimi kapsadığını belirler

```
REDDEDILEN — kapı bir üst katmanda (BattleActions)
  oyuncu ──► BattleActions ─[dost ateşi kapısı]─► AttackAction
  yapay zekâ ───────────────────────────────────► AttackAction
  tekrar kaydı ─────────────────────────────────► AttackAction
               ▲ alttaki İKİ ok kapıyı HİÇ görmeden geçer
                 ◄── SIZINTI NOKTASI

SEÇİLEN — kapı kuralın kendisinde (TargetingRules)
  oyuncu ──► BattleActions ──┐
  yapay zekâ ────────────────┼─► AttackAction
  tekrar kaydı ──────────────┘         │
                                       ▼
                         [dost ateşi kapısı]  ◄── TEK GEÇİT
```

#### KAPSAM: her kural aşağı İNMEZ

Ölçüt "kuralı **uygulayabilen** en alt katman": kural hangi veriyi okumak
zorundaysa, o verinin **görüldüğü** en alt katmanda yaşar. Dost ateşi kuralı
`Team` okur ve `Team` bu ad alanında yaşıyor — o yüzden indi.

Karşı örnek bu dosyanın kendi özet bloğunda yazılı: "sırayı kimin verdiğini" bu
tip **bilmez**. "Sırası mı" kuralı `TurnState` okumak zorunda, `TurnState` ise
`GridStrategy.Battle` assembly'sinde; bu assembly'nin `references` listesi
**boş**, yani o veri buradan **görünmez**. Kural bu yüzden yukarıda kaldı ve
kalması doğru. İki kararı ayıran şey katman hiyerarşisi değil, verinin nerede
görülebildiğidir.

#### İŞ BÖLÜMÜ: iki kapı, iki ayrı kaçak

```
TargetingRules'taki taraf kapısı ► HER çağırana uygulanır
BattleActions'taki sıra kapısı   ► YALNIZ akıştan geçene
```

Fazlalık değil bölüşme: buradaki satır silinirse yapay zekâ kendi takımını vurur
ve `BattleActions`'tan geçen testler yeşil kalır; oradaki sıra kapısı silinirse
oyuncu sırası olmadan vurur ve bu metot bunu göremez, çünkü sıra bilgisi
assembly duvarının ötesinde. **GARANTİ TAM ORADA BİTER.**

#### REDDEDILEN — soru burada sorulmaz, her akış kendi ön kontrolünü taşır

```csharp
if (!TargetingRules.CanBeAttacked(target.State))
```

**KIRILAN:** "dost ateşi yok" kuralı, kuralın kendisine değil **çağıranın**
dikkatine bağlı kalır.

```
BattleActions'tan geçen testler yeşil kalır
yapı aşırı yüklemesi boşluğu ikinci kez açar -> kendi barakanı yıkarsın
derleyici: hiçbir şey der  ·  test: ikinci çağıran doğunca da yeşil
```

**KAZANIRDI:** dost ateşi bir oyun **kipi** olsaydı — topçu saçılması, "zorlu"
kip — ama doğru şekil yine kipi parametre alan bir `TargetingRules` kuralıdır.

**TEK CUMLE:** Bir kural, onu UYGULAYABİLEN en alt katmanda sorulur; yukarıda
kalan kural yalnızca bir yoldan geçenleri korur.

**Sıra kararı:** önce hedef uygunluğu, sonra menzil. **Alternatif:** menzili önce
sormak. Seçilmedi: cesede "menzil dışı" denince yapay zekâ yaklaşır ve yine
reddedilir.

### "Düştü mü" bir DEĞİŞİM sorusudur

Durumu vuruştan **önce** oku. Sonucu ayırt etmenin tek yolu bu: "düştü mü"
sorusu bir **değişim** sorusudur, bir durum sorusu değil. Sonradan okunan
`State` tek başına yeterli olmaz — hedef zaten `Downed`'ken vurulmuş da
olabilir.

#### HARİTA: tek okuma ile iki okumanın ayrıştığı satır

İki okuma bir **geçiş** tanımlar; tek okuma yalnızca varış durumunu görür:

```
önce      sonra     tek okuma        iki okuma (SEÇİLEN)
───────   ───────   ──────────────   ───────────────────
Alive     Alive     Hit              Hit
Alive     Downed    HitAndDowned     HitAndDowned
Downed    Downed    HitAndDowned ◄── YALAN      Hit
Downed    Dead      Hit              Hit
```

Tek ayrışan satır üçüncüsü ve oynanışta en sık görülen satır tam o: düşmüş
birime "işini bitirmek" için vurmak `TargetingRules`'ta bilerek serbest
bırakıldı, yani bu yol bir istisna değil tasarımın kendisi.

#### KAPSAM: "önce oku" deseni her vuruş için DEĞİL

Desen yalnızca değişimi **söylemeyen** bir mutasyonun etrafında gerekir. Ayıraç:
mutasyon metodu geçişi kendisi döndürüyor mu?

```
Combatant.TakeDamage ► void ► değişimi ÇAĞIRAN ölçer
Structure.TakeDamage ► bool ► değişimi METODUN KENDİSİ söyler
```

Karşı örnek aynı dosyada, yapı aşırı yüklemesinde: orada `stateBeforeHit` gibi
bir yerel **yoktur** ve olmaması bilinçli; oradaki reddedilen alternatif bu
deseni oraya kopyalamayı ismen reddediyor. Aynı dosya, aynı soru, zıt karar —
çünkü sözleşmeler farklı.

#### İŞ BÖLÜMÜ: iki okuma, iki farklı yalanı kapatır

```
stateBeforeHit == Alive  ► "zaten düşmüştü" halini eler
target.State == Downed   ► "bu vuruş yetmedi" halini eler
```

Aynı şeyi iki kez sormuyorlar; `&&` ile birleşen iki ayrı eleme yapıyorlar. İlk
koşul silinirse tablonun üçüncü satırı geri gelir ve enkaza her vuruşta düşme
animasyonu oynar; ikinci koşul silinirse hedef ayakta kalsa bile düşmüş sayılır.

#### `stateBeforeHit`in YERİ kuralın kendisidir

Yerele `readonly` ya da başka bir modifier eklemek bu kırılmaya karşı hiçbir şey
yapmazdı; koruma tamamen **atamanın `TakeDamage` çağrısından önce olmasından**
geliyor. Aynı satır vuruştan sonraya taşınsaydı derleyici susar, iki okuma da
aynı değeri verir ve koşul her zaman `false` döner.

#### REDDEDILEN

```csharp
return target.State == UnitState.Downed
    ? AttackOutcome.HitAndDowned
    : AttackOutcome.Hit;
```

**KIRILAN:** DURUM sorusu, DEĞİŞİM sorusunun yerine geçer.

```
zaten Downed olan hedefe vuruş -> yine "HitAndDowned" döner
çağıran her vuruşta düşme animasyonu oynatır
skor tablosu aynı birim için defalarca puan yazar
derleyici: hiçbir şey der  ·  test: DEĞİŞİM sınanana kadar yeşil
```

**KAZANIRDI:** `Downed` bir hedefe saldırmak yasak olsaydı — o zaman bu satıra
gelindiğinde hedef kesinlikle `Alive` olurdu.

**TEK CUMLE:** "Düştü mü" bir değişim sorusudur, ve tek bir okumayla cevaplanan
her değişim sorusu er ya da geç yalan söyler.

---

## Execute(Combatant attacker, Structure target, int distance)

Bir **yapıya** yapılan saldırı denemesini yürütür. Akışın şekli birim sürümüyle
aynıdır — saldıranın durumu, hedef uygunluğu, menzil, hasar — çünkü **sıra**
kararı hedefin ne olduğuna bağlı değil.

İki yerde ayrışır ve ikisi de hedefin doğasından gelir:

1. uygunluğu `StructureState` söyler, `UnitState` değil — bir baraka düşmez,
   yıkılır;
2. ölüm olayının adı `AttackOutcome.HitAndDestroyed`'dır.

### İki aşırı yükleme, tek akış şekli

#### HARİTA: bedelin KURAL SAHİPLİĞİNDE ödendiği yer

```
REDDEDILEN — IAttackTarget arkasında tek gövde
  AttackAction ──► IAttackTarget.CanBeAttackedBy(team)
  Combatant ────► TargetingRules   ◄── kural HEDEFİN İÇİNE indi
  Structure ────► TargetingRules   ◄── kural HEDEFİN İÇİNE indi

SEÇİLEN — iki aşırı yükleme, kural dışarıda
  Execute(.., Combatant, ..) ─┐
  Execute(.., Structure, ..) ─┴─► TargetingRules   ◄── TEK EV
  Combatant ──► (ok yok)      Structure ──► (ok yok)
```

#### SEÇENEK / NEYİ ANAHTAR ALIR / KIRILDIĞI YER

```
seçenek            anahtar             kırıldığı yer
────────────────   ─────────────────   ──────────────────────
iki aşırı yükleme  hedefin TİPİ        beşinci hedef tipi
                   (derleyici seçer)   eklendiği gün
IAttackTarget      hedefin DAVRANIŞI   uygunluk kuralı hedefe
                   (bool döndürür)     taşınır; Downed ile
                                       Destroyed aynı bool'un
                                       arkasına düşer
```

#### KAPSAM: "soyutlama yapma" bu dosyanın kuralı DEĞİL

Reddedilen şey soyutlamanın kendisi değil bugünkü **fiyatı**: iki metot silmek
için kural sahipliğini bozmak.

Karşı örnek aynı akışın sonuç ekseninde: hedef tipi ikiye ayrılmışken
`AttackOutcome` ikiye **ayrılmadı** — tek enum hem birime hem yapıya yapılan
saldırıyı adlandırıyor ve ikinci bir `StructureAttackOutcome` ismen reddedildi
([AttackOutcome.md](AttackOutcome.md#hitanddestroyed)). Yani aynı akış bir
eksende ortaklaştırıyor, öbüründe ayırıyor. Ayıraç: ortaklaştırma bir **kuralın
evini** değiştiriyor mu? Sonuç enum'u değiştirmiyordu, arayüz değiştirirdi.

#### İŞ BÖLÜMÜ: tip ekseni ile sonuç ekseni

```
iki aşırı yükleme ► hedefin TİPİNİ ayırır (StructureState ↔ UnitState dili)
tek AttackOutcome ► iki akışın CEVABINI birleştirir; ayrışan tek değer
                    HitAndDestroyed
```

Aşırı yüklemelerden biri silinirse o hedef tipine saldırı yolu kapanır;
`AttackOutcome` ikiye bölünürse her tüketici paralel bir `switch` taşır ve
ikizler zamanla ayrışır. İki ayrı kırılma, iki ayrı sahip.

#### REDDEDILEN — iki aşırı yükleme tek gövdede birleşir

```csharp
public interface IAttackTarget
{
    bool CanBeAttackedBy(Team attackerTeam);
    bool TakeDamage(int amount);
}
public static AttackOutcome Execute(Combatant attacker, IAttackTarget target, int distance)
```

**KIRILAN:** hedef uygunluğu kuralı `TargetingRules`'tan **hedefin içine**
taşınır.

```
Combatant ve Structure ikisi de TargetingRules'ı tanır
durum matrisi tek enum ile sınanamaz -> testler nesne kurar
arayüzün bool'u -> Downed ile Destroyed aynı cevabın arkasına düşer
derleyici: hiçbir şey der  ·  test: TargetingRulesTests satır sayısı artar
```

**KAZANIRDI:** hedeflenebilir tip sayısı üçü geçtiği gün — birim, yapı, araç,
tuzak, kapı — aynı akışı beş kez kopyalamak gerçek bir tekrar olurdu.

**TEK CUMLE:** Soyutlama tekrarı sildiği için değil, KURAL sahipliğini bozmadan
sildiği için kazanır; bugün sildiği tek şey iki metot.

### Saldıranın durumu burada da soruluyor

Bu tekrar değil, kuralın **iki akışta da uygulanmasıdır**. Kuralın **metni** tek
yerde (`AttackRules`); burada yalnızca soruluyor. Bu satır olmasaydı düşmüş bir
birim askere vuramaz ama barakayı yıkabilirdi ve o fark hiçbir yerde yazılı
olmazdı — yalnızca hangi aşırı yüklemeye düştüğüne bağlı olurdu.

**Dikkat: `AttackRules.CanAttack` bu dosyada İKİ kez çağrılıyor** — her aşırı
yüklemede bir kez, ve yukarıdaki "bu satır" ikincisidir:

```csharp
// ① Execute(Combatant attacker, Combatant target, int distance)
if (!AttackRules.CanAttack(attacker.State))
{
    return AttackOutcome.RejectedActorCannotAct;
}

// ② Execute(Combatant attacker, Structure target, int distance)
if (!AttackRules.CanAttack(attacker.State))   // ◄── "BU SATIR" bu
{
    return AttackOutcome.RejectedActorCannotAct;
}
```

İki blok karakter karakter aynı; ayıran tek şey hangi metodun içinde durdukları,
ve hangisine düşüleceğini hedefin TİPİ üzerinden derleyici seçer.

**"Olmasaydı" tam olarak şu düzenlemedir:** ② işaretli dört satır silinir, ①
yerinde kalır.

```
Execute(attacker, Structure, distance) saldıranın durumunu HİÇ sormaz
   ─► düşmüş birim askere vuramaz  (① eler)
      düşmüş birim barakayı YIKAR  (② artık yok)
   ─► yüzeye çıktığı yer: Execute_DownedAttackerAgainstStructure_
      IsRejectedAndDealsNoDamage kırmızıya döner; birim ikizi
      Execute_DownedAttacker_IsRejectedAndDealsNoDamage YEŞİL kalır
      >> ayakta duran tek fark hedefin TİPİ <<
```

Saldıranın tipi iki akışta da `Combatant`, yani soru **değişmiyor**; değişen tek
şey hedefin dili (`StructureState` ↔ `UnitState`). `AttackRules`'ın yapı ikizi bu
yüzden **yok**: yapı saldırmaz, saldıran hep bir `Combatant`'tır. Aynı gerekçe
`TargetingRules`'ta diriltmenin yapı ikizi için de yazılı — birebir eşleşmeyen
satır bir eksiklik değil, bir **karardır**.

Sıra da birim sürümüyle aynı ve gerekçesi de aynı: menzil dışındaki bir
**enkaza** saldırınca cevap "menzil dışı" olsaydı, yapay zekâ yaklaşır ve yine
reddedilirdi — sonsuz döngü, ve hiçbir test kırmızı olmazdı.

### Simetri bir gerekçe DEĞİLDİR

Burada "önceki durumu oku" deseni **yok** — ve bu bir eksiklik değil,
`Structure`'ın sözleşmesinin farkı. `Structure.TakeDamage` zaten "yapı **bu**
vuruşla yıkıldı mı" cevabını döndürüyor; birim tarafında `Combatant.TakeDamage`
`void` olduğu için değişimi çağıranın kendisi ölçmek zorundaydı. Aynı deseni
burada tekrarlamak, cevabı zaten veren bir metodun cevabını görmezden gelmek
olurdu.

#### HARİTA: cevabın nerede ÜRETİLDİĞİ

```
BİRİM YOLU (yukarıdaki aşırı yükleme)
  Combatant.TakeDamage(int) ──► void
  cevap YOK ► Execute iki okuma yapıp geçişi KENDİ kurar
              ◄── kararın sahibi burada olmak ZORUNDA

YAPI YOLU (bu aşırı yükleme)
  Structure.TakeDamage(int) ──► bool "bu vuruş yıktı mı"
  cevap VAR ► Execute yalnızca ADLANDIRIR
              ◄── ikinci bir karar üretmek, kararı sahibinden
                  geri almak olurdu
```

#### KAPSAM: bu dosya simetriyi genel olarak REDDETMİYOR

Karşı örnek hemen yukarıda, aynı metodun içinde: üç erken çıkış — saldıranın
durumu, hedef uygunluğu, menzil — birim sürümüyle **birebir** aynı sırada
tekrarlanıyor ve bu tekrar doğru, çünkü orada ikinci bir cevap sahibi yok; aynı
soru iki akışta da soruluyor. Ayıraç tek: cevabı zaten veren bir metot var mı?
Varsa simetri onu görmezden gelmektir; yoksa simetri yalnızca aynı kuralın iki
kez sorulmasıdır.

#### İŞ BÖLÜMÜ: kim yıkımı BİLİR, kim ADLANDIRIR

```
Structure.TakeDamage'ın bool'u ► yıkım OLGUSU
AttackOutcome.HitAndDestroyed  ► olgunun çağırana giden ADI
```

Aynı bilgiyi iki kez taşımıyorlar, zinciri bölüşüyorlar: `bool` silinirse (metot
`void`'e dönerse) burada birim sürümündeki iki okuma deseni geri gelmek zorunda
kalır; `HitAndDestroyed` silinirse olgu üretilir ama çağıran onu asla duymaz ve
yıkımı öğrenmek için saldırıdan sonra `State`'i okumak zorunda kalır — ki o
okuma enkaza yapılan vuruşta yanlış cevap verir.

#### REDDEDILEN — birim sürümündeki desen körü körüne kopyalanır

```csharp
StructureState stateBeforeHit = target.State;
target.TakeDamage(attacker.AttackProfile.Damage);
return stateBeforeHit == StructureState.Standing
    && target.State == StructureState.Destroyed
    ? AttackOutcome.HitAndDestroyed
    : AttackOutcome.Hit;
```

**KIRILAN:** "bu vuruş mu yıktı" kararı, cevabı zaten veren metodun dışında
**ikinci** kez verilir.

```
bugün aynı cevabı verir -> hiçbir test ayırt edemez
yıkım koşulu değişir — can sıfıra inmeden çöken bina — burası eskir
derleyici: hiçbir şey der  ·  test: yeşil kalır, sahiplik kayar
```

**KAZANIRDI:** `Structure.TakeDamage` `void` olsaydı ya da `bool`'u "hasar
uygulandı" gibi başka bir soruyu cevaplasaydı.

**TEK CUMLE:** Simetri bir gerekçe değildir; cevabı zaten veren bir metodun
cevabını görmezden gelmek, kararı sahibinden geri almaktır.
