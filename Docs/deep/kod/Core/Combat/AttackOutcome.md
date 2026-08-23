# AttackOutcome

> **Kaynak:** `Assets/Game/Core/Combat/AttackOutcome.cs`
> **Ad alanı:** `GridStrategy.Combat` · **Assembly:** `GridStrategy.Combat` (`noEngineReferences: true`)
> **Rol:** Tanım (Profile) — kimliği yok, hafızası yok, karar vermez; olup biteni ADLANDIRIR

Bir saldırı **denemesinin** sonucu. "Deneme" kelimesi kasıtlı: reddedilen bir
saldırı da bir sonuçtur ve çağıranın onu ayırt etmesi gerekir. Kararı
`AttackAction` verir; bu enum yalnızca ada bağlar.

**Hedef tipinden bağımsız:** aynı enum hem `Combatant`'a hem `Structure`'a
yapılan saldırıyı adlandırır. Ret sebepleri ve "vurdu" cevabı ikisinde de
birebir aynı cümledir; ayrışan tek şey **ölüm olayının adıdır** ve o da tek bir
değerle ifade ediliyor (`HitAndDowned` ↔ `HitAndDestroyed`).

| Üye | Karar | Detay |
|---|---|---|
| `AttackOutcome` (tip) | enum, struct değil — "hangi durum" sorusu TİPE sorulur | [↓](#attackoutcome-tip) |
| `RejectedInvalidTarget` | sıfırıncı değer BİLEREK bir RET | [↓](#rejectedinvalidtarget) |
| `RejectedOutOfRange` | ayrı ret değeri: çağıran YAKLAŞIR | [↓](#rejectedoutofrange) |
| `Hit` | hasar uygulandı, hedef ayakta | [↓](#hit) |
| `HitAndDowned` | birim DÜŞTÜ — kurtarma penceresi açan olay | [↓](#hitanddowned) |
| `HitAndDestroyed` | yapı YIKILDI — beşinci değer, ayrı bir enum değil | [↓](#hitanddestroyed) |
| `RejectedActorCannotAct` | altıncı değer, SONA eklendi; iki ayrı üreticisi var | [↓](#rejectedactorcannotact) |

**İlgili anlatılar:** [06-sonuç enum'ları](../../../konular/06-sonuc-enumlari.md)

---

## AttackOutcome (tip)

### Neden `bool` değil

"Saldırı oldu mu" üç ayrı soruyu tek cevaba sıkıştırırdı — reddedildi mi, vurdu
mu, düşürdü mü. Çağıran üçüne farklı tepki verir: ret sessizdir (belki bir uyarı
sesi), vuruş bir efekt ister, düşürme bir animasyon ve muhtemelen bir skor.
`bool` ile yazılsaydı bu ayrım çağıranın içinde ikinci bir kontrol olarak
yeniden doğardı.

### HARİTA: struct'ta hangi alan ne zaman ANLAMSIZ

```
durum             Rejected   DamageDealt   Downed
───────────────   ────────   ───────────   ──────
menzil dışı       true       ??? ◄──       ??? ◄──
geçersiz hedef    true       ??? ◄──       ??? ◄──
vurdu, ayakta     false      12            false
vurdu, düştü      false      12            true

◄── işaretli hücreler: sıfır mı, tanımsız mı? Cevabı TİP söylemiyor;
    çağıran HATIRLAMAK zorunda.
```

### SEÇENEK / ÇAĞIRAN NASIL DALLANIR / EKSİK DALI KİM GÖRÜR

```
enum     switch (outcome)       BoardAdapter'daki
                                `default: LogError` — yeni değer
                                çalışma anında GÖRÜNÜR
struct   if (o.Rejected) ...    HİÇ KİMSE; üç alanın
         else if (o.Downed) ..  kombinasyonu bir tip değil,
                                çağıranın hafızasıdır
```

### KAPSAM: veri taşıyan tip bu ad alanında yasak DEĞİL

Ayıraç: alanların hepsi **her** durumda anlamlı mı?

```
hepsi her zaman anlamlı  ► veri taşıyan tip (AttackProfile)
bazıları bazı durumlarda
anlamsız                 ► enum             (bu tip)
```

Karşı örnek aynı ad alanında, aynı akışın girdisinde: `AttackProfile` tam
olarak veri taşıyan bir tiptir (`Damage`, `Range`) ve orada doğru seçim odur —
orada sorulan soru "ne kadar" ve her alan her örnekte anlamlı.

### İŞ BÖLÜMÜ: ret ailesi ile vuruş ailesi

```
üç Rejected* değeri ► "neden OLMADI" — çağıran farklı DÜZELTİR
iki Hit* değeri     ► "ne OLDU" — çağıran farklı GÖSTERİR
```

İki aile aynı enum'da yaşar ama farklı soruyu kapatır: ret ailesi tek bir
`Rejected`e indirilseydi yapay zekâ yaklaşmakla hedef değiştirmek arasında
seçim yapamazdı; vuruş ailesi tek `Hit`e indirilseydi düşme animasyonu ile skor
kaydı her vuruşta tetiklenirdi.

### REDDEDILEN — enum tamamen kalkar

```csharp
public readonly struct AttackOutcome
{
    public bool Rejected { get; }
    public int DamageDealt { get; }
    public bool Downed { get; }
}
```

**KIRILAN:** üç alanın ikisi her çağrıda anlamsız kalır ve anlamı tip söylemez.

```
ret durumunda DamageDealt sıfır mı tanımsız mı -> çağıran hatırlar
switch'te EKSİK DAL derleyiciden görünmez olur; enum'da görünür
derleyici: hiçbir şey der  ·  test: hiçbiri kırmızıya dönmez
```

**KAZANIRDI:** UI hasar sayısını göstermek istediği gün — "12 hasar" yazmak
için miktar gerekir ve enum onu taşıyamaz. O gün bu enum struct'ın **içine**
bir alan olarak girer; silinmez.

**TEK CUMLE:** Enum "hangi durum" sorusunu TİPE sordurur, struct çağıranın
hafızasına; bugün sorulan soru "hangi durum".

---

## RejectedInvalidTarget

Sıfırıncı değer **bilerek** bir ret değeri.

### HARİTA: `default(AttackOutcome)` nereye düşer

```
index   SEÇİLEN sıra              REDDEDILEN sıra
─────   ───────────────────────   ─────────────────────────
  0     RejectedInvalidTarget     Hit             ◄── TEHLİKE
  1     RejectedOutOfRange        HitAndDowned
  2     Hit                       RejectedInvalidTarget
  3     HitAndDowned              RejectedOutOfRange
  4     HitAndDestroyed           —
  5     RejectedActorCannotAct    —

index 0'a DÜŞEN üç yol (üçü de derleyiciden sessiz geçer):
  default(AttackOutcome)
  new AttackOutcome[n] hücreleri
  atanmayı unutulan bir alan

██ SEÇİM YALNIZCA 0 HÜCRESİNDE YAPILIR ██ Bu üç yolu kapatmanın yolu yok;
seçilebilen tek şey orada NE durduğu.
```

### KAPSAM: bu enum'da yalnızca 0 KONUMU anlam taşır

Karşı örnek altıncı değerde yazılı: `RejectedActorCannotAct` ret ailesinin
**yanına** değil **sona** eklendi ve gerekçesi açıkça "kaynaktaki sıra zaten
akış sırası **değil**" — `HitAndDestroyed` beşinci sırada durup bir **başarı**
değeridir. Yani 1..5 arasındaki konumlar hiçbir şey söylemez; söyleyen tek
konum sıfırdır. Kural genel değil, tek hücreye özeldir.

### İŞ BÖLÜMÜ: sıfırıncı hücre ile SONA EKLEME kararı

```
index 0'ın bir RET olması   ► atanmamış değerin ZARARSIZ okunmasını sağlar
yeni değerin SONA eklenmesi ► aradaki değerlerin sessizce yeniden
                              numaralanmasını önler
```

Aynı kaygının iki ayrı yarısı: ilki bugünkü sıfırı korur, ikincisi yarınki
eklemelerin sıfırı ittirmemesini sağlar. İlki bozulursa sıfırlanmış dizi
hücresi "vurdu" olur; ikincisi bozulursa sıfır yerinde kalır ama 1..5 kayar.

### REDDEDILEN

```csharp
Hit,
HitAndDowned,
RejectedInvalidTarget,
RejectedOutOfRange
```

**KIRILAN:** `default(AttackOutcome)` artık "vurdu" demek olur. Sıfırıncı değer
gerekçesinin tamamı `Team.cs`'te yazılı; buradaki **fark**, atanmamış değerin
BAŞARILI bir saldırı gibi okunması — sıfırlanmış dizi hücresi hasar uygulanmış
sayılır.

```
derleyici: hiçbir şey der  ·  test: hiçbiri kırmızıya dönmez
```

**KAZANIRDI:** `Team.cs`'tekiyle aynı — sıfır bir güvenlik değil **sıklık**
kararı olsaydı (histogram sıkıştırması).

**TEK CUMLE:** Sıfırıncı değer bir varsayılan değil bir SİGORTAdır; en zararsız
cevap "olmadı"dır.

---

## RejectedOutOfRange

Menzil dışı. Hedef geçerliydi ama ulaşılamadı.

`RejectedInvalidTarget` ile tek bir `Rejected` altında **birleştirilmedi** —
çünkü çağıranın cevabı gerçekten farklı: birinde **yaklaşır**, öbüründe
**hedef değiştirir**. Ayıraç tek: çağıran iki sebebe farklı mı davranıyor?

---

## Hit

Hasar uygulandı; hedef hâlâ ayakta.

İki hedef tipinde de aynı cümle — birim ve yapı bu değeri paylaşır. Ayrışan tek
çift `HitAndDowned` ↔ `HitAndDestroyed`.

---

## HitAndDowned

Hasar uygulandı ve hedef **bu vuruşla** düştü.

"Bu vuruşla" ifadesi taşıyıcı: değer bir **durum** değil bir **geçiş**
adlandırır. Zaten `Downed` olan bir hedefe vurulduğunda dönen değer `Hit`'tir,
`HitAndDowned` değil — geçişi ölçen iki okuma
[AttackAction.md](AttackAction.md#executecombatant-attacker-combatant-target-int-distance)'de
anlatılıyor.

Düşme, kurtarma penceresi açan bir durumdur; yıkım öyle değil. `HitAndDestroyed`
bu yüzden ayrı bir değer.

---

## HitAndDestroyed

Hasar uygulandı ve **yapı** bu vuruşla yıkıldı. Beşinci değer, ayrı bir enum
değil.

**Neden `HitAndDowned` yeniden kullanılmıyor:** bir baraka düşmez, **yıkılır**.
"Düşme" (bkz. `StructureState`) kurtarma penceresi açan bir durumdur; yapıda
öyle bir pencere yok. Aynı değeri iki farklı olguya vermek, çağıranın onları
ayırt etmesini imkânsız kılardı.

**Neden düz `Hit` dönülmüyor:** yıkım bilgisi çağıranın elinden alınırdı ve
çağıran onu geri kazanmak için saldırıdan **sonra** `State`'i okumak zorunda
kalırdı — `StructureLifecycle`'ın "dönüş değeri: soran zaten orada" kararının
tam tersi. Üstelik o okuma yanlış cevap verirdi: zaten yıkık bir enkaza vurmak
da `State == Destroyed` gösterir.

### HARİTA: tüketici × enum — ikizlerin ayrıştığı yer

```
SEÇİLEN — tek enum
  Execute(.., Combatant, ..) ─┐
  Execute(.., Structure, ..) ─┴─► AttackOutcome
                                    ├─► BattleActions
                                    └─► BoardAdapter
                                        .ReactToAttack
                                        └ default: LogError
                                          ◄── TEK SİGORTA

REDDEDILEN — iki enum
  Execute(.., Combatant, ..) ──► AttackOutcome ──┐
  Execute(.., Structure, ..) ──► StructureAttack │
                                 Outcome ────────┤
                                    ├─► BattleActions (switch #1)
                                    └─► BoardAdapter  (switch #2)
  ◄── AYRIŞMA NOKTASI: iki switch, iki `default`, ve birine eklenen
      değer öbürünü hiç ilgilendirmez
```

### KAPSAM: "tek enum" bu ad alanının genel kuralı DEĞİL

Ayıraç, **ret sebeplerinin** aynı cümle olup olmadığıdır — burada üçü de birebir
aynı (geçersiz hedef, menzil dışı, eyleyemez).

Karşı örnek aynı ad alanında: `UnitState` ile `StructureState` **iki ayrı**
enum'dur ve birleştirilmeleri `TargetingRules`'ta ismen reddedildi — çünkü orada
değerler örtüşmüyor ve birleşen enum her `switch`'e asla çalışmayan bir `Downed`
dalı eklerdi. Aynı akış, aynı iki hedef tipi: **sonuç ekseninde birleşiyor,
durum ekseninde ayrılıyor.**

### İŞ BÖLÜMÜ: ortak değerler ile ayrışan tek değer

```
RejectedInvalidTarget / RejectedOutOfRange /
RejectedActorCannotAct / Hit    ► İKİ hedef tipinde de AYNI
HitAndDowned ↔ HitAndDestroyed  ► ayrışan TEK çift; ölüm olayının adı
```

Ortak dört değer yapıya kopyalansaydı tüketiciler paralel `switch` taşırdı;
ayrışan çift tek değere indirilseydi çağıran "düştü" ile "yıkıldı"yı ayırt
edemezdi — biri kurtarma penceresi açar, öteki açmaz.

### REDDEDILEN — yapılar kendi enum'unu alır

```csharp
public enum StructureAttackOutcome
{
    RejectedInvalidTarget,
    RejectedOutOfRange,
    Hit,
    HitAndDestroyed
}
```

**KIRILAN:** her tüketici PARALEL bir `switch` taşır ve ikizler zamanla
ayrışır.

```
bugün iki tüketici var -> BattleActions, BoardAdapter.ReactToAttack
biri yeni değeri işler, diğeri işlemez -> switch DEYİMİ uyarmaz
tek `default: LogError` koruması ikiye bölünür -> yıkım duyurulmaz
derleyici: hiçbir şey der  ·  test: hiçbiri kırmızıya dönmez
```

**KAZANIRDI:** yapı saldırıları, birimlerde **hiç** karşılığı olmayan değerler
kazandığı gün — kuşatma bonusu, kısmi çökme, duvar kaybı.

**TEK CUMLE:** İki olgunun ret sebepleri AYNI cümleyse tek enum doğrudur ve
farklı olan tek değer o enum'a EKLENİR, ikinci enum açılmaz.

---

## RejectedActorCannotAct

Saldıran şu an saldıramaz: durumu elvermiyor (`AttackRules.CanAttack`) ya da
sırası değil (bu ikincisini yalnızca `BattleActions` üretir).

### Altıncı değer, ve SONA eklendi — ret ailesinin yanına değil

Diğer üç ret değerinin yanına sokmak, aradaki üç değeri sessizce yeniden
numaralandırırdı. Bugün bu enum'un sayılarını saklayan hiçbir yer yok, yani
kırılma **ölçülebilir değil** — ama sona eklemek geri alınabilir olanıdır ve
kaynaktaki sıra zaten akış sırası **değil**: `HitAndDestroyed` beşinci sırada
durup bir başarı değeridir. Sıra bilgisini enum'dan okumaya çalışan bir gözün
burada zaten yanılacak olması, ret değerlerini kümelemenin taşıdığı tek faydayı
da siler.

### Anlamı tek cümle

Eylemi yapan taraf şu an eylem yapamaz. Üç ayrı sebebi birden kapsar —
saldıran düşmüş, hareket eden düşmüş, sırası değil — ve kapsaması **bilinçli**:
çağıranın dallanması üçünde de aynıdır. Hedefi ya da hedef hücreyi değiştirmek
hiçbirinde yardım etmez.

### Aynı ad `MoveOutcome`'da da var, üretilebilirlikleri FARKLI

Burada `AttackAction` (Combat) bu değeri **kendisi üretebilir**, çünkü
`UnitState`'i görür ve `AttackRules`'a sorar; `MoveOutcome` tarafında ise sahibi
`MoveAction` onu **asla üretemez**. Farkın tamamı ve eşiği `MoveOutcome.cs`'te
yazılı; burada tekrarlanmıyor.

### HARİTA: sebep → çağıranın yapabileceği tek şey

```
ret sebebi              çağıran ne yapar        bugünkü dal
─────────────────────   ─────────────────────   ───────────
RejectedOutOfRange      YAKLAŞIR                ayrı dal
RejectedInvalidTarget   BAŞKA HEDEF seçer       ayrı dal
────────────────────────────────────────────────────────────
saldıran düşmüş         hiçbir şey — bekler     ┐
hareket eden düşmüş     hiçbir şey — bekler     ├ TEK DAL
sıra kendisinde değil   hiçbir şey — bekler     ┘ ◄── ÜÇÜ BİRLEŞTİ

██ EŞİK ██ arayüz oyuncuya "sıran değil" ile "birim düşmüş" farkını
SÖYLEMEK zorunda kaldığı gün alt üç satır ayrılır. Bugünkü tek tüketici
BoardAdapter ve o yalnızca log basıyor.
```

### KAPSAM: bu enum ret sebeplerini genel olarak BİRLEŞTİRMEZ

Karşı örnek aynı enum'un ilk iki değeri: `RejectedInvalidTarget` ile
`RejectedOutOfRange` tek bir `Rejected` altında birleştirilmedi — çünkü
çağıranın cevabı gerçekten farklı: birinde yaklaşır, öbüründe hedef değiştirir.
Aynı dosya, aynı aile, zıt karar. Ayıraç tek: çağıran iki sebebe **farklı** mı
davranıyor?

### İŞ BÖLÜMÜ: tek değer, İKİ ayrı üretici

```
AttackAction (Combat)  ► "saldıranın durumu elvermiyor";
                         UnitState'i görür, AttackRules'a sorar
BattleActions (Battle) ► "sıra sende değil"; TurnState'i görür,
                         bu assembly ONU GÖRMEZ
```

İkisi aynı değeri üretir ama farklı kapıları kapatır: buradaki üretici susarsa
düşmüş birim vurur; oradaki susarsa sırası olmayan vurur. Değer ortak olduğu
için tüketicide tek dal yeter.

**GARANTİ NEREDE BİTER:** tam burada — bu ad alanı sıra bilgisini hiç göremediği
için "sıran değil" cevabını **kendisi asla üretemez**.

### REDDEDILEN — tek değer yerine iki ayrı sebep

```csharp
RejectedNotYourTurn,
RejectedAttackerCannotAct
```

**KIRILAN:** çağıranın dallanması bugün ikisinde de AYNI — `BoardAdapter`
yalnızca log basıyor — yani iki dal aynı satırı iki kez yazar.

```
birini işlemeyi unutan switch DEYİMİ -> "sıran değil" ekrana ulaşmaz
derleyici: hiçbir şey der  ·  test: hiçbiri kırmızıya dönmez
```

**KAZANIRDI:** arayüz oyuncuya "sıran değil" ile "birim düşmüş" farkını SÖYLEMEK
zorunda kaldığı gün; tetiği `MoveOutcome.cs`'te yazılı.

**TEK CUMLE:** Bir ayrım, çağıranın DALLANMASINI değiştirdiği gün doğar; bugün
değiştirmiyor, yalnızca maliyet ekliyor.
