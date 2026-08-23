# Aynı tıklama, dört ayrı ret — sıranın gözlenebilir olduğu yer

> **Nerede geçiyor:** `BattleActions.cs` → `AttackAction.cs` / `MoveAction.cs` → `AttackRules.cs` / `TargetingRules.cs` / `TurnRules.cs`
> **Kodda nereden geldin:** `BattleActions` sınıf başlığındaki ADIM 0-7 zinciri, `AttackAction.Execute`'un erken çıkış merdiveni, `MoveAction.Execute`'un SEVİYE 1-3 tablosu, `PlacementOutcome`'un dördüncü-değer bloğu
> **Ne zaman oku:** bir `if` bloğunu yukarı ya da aşağı taşımadan önce, ya da "bu iki ret zaten aynı, sırası ne fark eder" diye sorduğunda.

---

## Sahne

Oyuncu bir düşman askerine tıklıyor. Ekranda hiçbir şey olmuyor.

Neden olmadı? Dört ayrı cevap aynı anda doğru olabilir:

```
sıran değil  ·  askerin yerde yatıyor  ·  o senin adamın  ·  çok uzakta
```

Oyuncu bunlardan **tek birini** görecek. Hangisini gördüğü, kodda hangi `if`
bloğunun daha yukarıda durduğuna bağlı — ve bu bir üslup tercihi değil.
Testler hangi cevabın kazanacağını tutuyor, çünkü yanlış cevap oyuncuyu
düzeltemeyeceği bir şeyi düzeltmeye gönderiyor.

Bu dosya o sıranın nasıl kurulduğunu anlatıyor: iki ölçüt, iki cevap kanalı,
ve altına düşen her kuralı açıklamaya çeviren tek bir çizgi.

---

## Karakterler

Altı tip var ve hikâyeyi kuran şey yine bildikleri değil, **bilmedikleri**.

```
╔═ BattleActions ═══════════════════════════════════════════════╗
║  İşi     : AKIŞI yürütmek. Hangi soru, hangi sırayla          ║
║  Bilir   : tahtayı, savaşçıları, sırayı, dört eylemin şeklini ║
║  BİLMEZ  : ██ HİÇBİR KURALIN METNİNİ ██ — her `if` bir kuralı ║
║            SORAR, hiçbiri bir kural YAZMAZ                    ║
╚═══════════════════════════════════════════════════════════════╝

╔═ TurnRules ═══════════════════════════════════════════════════╗
║  İşi     : "sıra bu tarafta mı" sorusunun tek sahibi          ║
║  Bilir   : Team değerlerini, tur bütçesinin eşiğini           ║
║  BİLMEZ  : hangi birimin sorulduğunu — yalnızca TARAFINI      ║
║            görür. Hedefi, menzili, tahtayı hiç görmez         ║
╚═══════════════════════════════════════════════════════════════╝

╔═ AttackRules ═════════════════════════════════════════════════╗
║  İşi     : "ben vurabilir miyim" — EYLEYEN kuralı             ║
║  Bilir   : tek bir UnitState                                  ║
║  BİLMEZ  : hedefi, menzili, ve ██ SIRANIN KİMDE OLDUĞUNU ██   ║
║            TurnState başka bir assembly'de; adı yazılamaz     ║
╚═══════════════════════════════════════════════════════════════╝

╔═ TargetingRules ══════════════════════════════════════════════╗
║  İşi     : "kime vurulur / kim kaldırılır" — HEDEF kuralı     ║
║  Bilir   : hedefin durumunu VE iki tarafı birden              ║
║  BİLMEZ  : saldıranın kendi durumunu. Menzili. Sırayı.        ║
╚═══════════════════════════════════════════════════════════════╝

╔═ AttackAction ════════════════════════════════════════════════╗
║  İşi     : saldırı akışı — KENDİ erken çıkış merdiveni var    ║
║  Bilir   : üç kuralı ve onları hangi sırayla soracağını       ║
║  BİLMEZ  : iki birimin nerede durduğunu (mesafe hazır gelir), ║
║            sırayı kimin verdiğini                             ║
╚═══════════════════════════════════════════════════════════════╝

╔═ MoveAction ══════════════════════════════════════════════════╗
║  İşi     : hareket akışı — üç seviyelik kendi tablosu var     ║
║  Bilir   : tahtayı, uzaklık ölçümünü, üç ret sebebini         ║
║  BİLMEZ  : ██ BİRİMİN DURUMUNU ██ — `UnitState` diye bir tipin║
║            adını bile yazamaz; asmdef Combat'ı görmüyor       ║
╚═══════════════════════════════════════════════════════════════╝
```

En tuhaf iki satır alttaki ikisi: `AttackRules` sırayı **soramaz**,
`MoveAction` durumu **soramaz**. İkisi de nezaketten değil — sorunun cevabını
taşıyan tip o assembly'den görünmüyor.

**Bütün sıra kararı bu iki körlükten doğuyor.** Kural, onu uygulayabilen en alt
katmana iner; inemediği yerde bir üst katmanda kalır. Ret sırası da tam olarak
"hangi kural hangi katmanda yaşayabildi" sorusunun tortusu.

> **Önce oku:** [`02-assembly-duvari.md`](02-assembly-duvari.md#duvarin-engelledigi-sey-gorunurluk)
> — yukarıdaki iki körlüğün tamamı `.asmdef` dosyalarının `references` dizisine
> dayanıyor ve bu dosya `asmdef`'i tanımlamıyor. "`TurnState` başka bir
> assembly'de, adı yazılamaz" cümlesinin **neden** derleme hatası ürettiği orada.
>
> **İki ileri işaretçi de burada dursun:** hangi **istisna tipinin** ne zaman
> seçileceği bu dosyanın işi değil —
> [`dil/03`](../dil/03-hata-bildirme-ve-dogrulama.md); sonuç `enum`'larının
> kendisi de değil — [`konular/06`](06-sonuc-enumlari.md). ██ `06` ölçütü
> **kurar**, bu dosya onu bir sıra kararına **uygular**; kuran önce okunur. ██

---

## Birinci durak: çizginin kendisi

Önce zincire bak. Dört genel metot da aynı adımları yürütüyor:

```
   ADIM 0  null denetimi         çağıran hatası ► ArgumentNullException
   ADIM 1  "bu savaşta mı"       çağıran hatası ► ArgumentException
   ADIM 2  SIRA kuralı           oyun sonucu    ► Rejected...CannotAct
   ADIM 3  EYLEYENİN durumu      oyun sonucu    ► Rejected...CannotAct
   ADIM 4  HEDEF / HÜCRE kuralı  oyun sonucu    ► RejectedInvalidTarget
   ADIM 5  MENZİL                oyun sonucu    ► RejectedOutOfRange
   ────────────────────────────────────────────────────────────────
   ADIM 6  ██ TEK YAZMA ██   ◄── AYRIŞMA NOKTASI: buradan sonrası
   ADIM 7  sıra devri            geri alınamaz
```

O yatay çizgi bu dosyanın tek gerçek yapısal kararı.

**Çizginin üstü soru, altı olgu.** Üstte "olur mu" diye sorulur; altta "oldu"
denir. Bir kural çizginin altına düşerse kural olmaktan çıkıp **açıklamaya**
döner.

Ölçüsü `Attack`'in sıra bloğunda yazılı ve iki sütun hâlinde duruyor:

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

İki sütun **aynı üç satırı** taşıyor. Ayrışan tek şey `██` işaretli geri
dönülemez adımın kaçıncı sırada durduğu. Sağdaki dünyada "reddedildi" cevabı
teknik olarak doğru bir metindir — hasar zaten inmiştir.

Kanıtı yazılı: `Attack_WhenItIsNotTheAttackersTurn_IsRejectedAndDealsNoDamage`.
Testin adındaki ikinci yarım tesadüf değil — sıranın reddedildiğini ölçmek
yetmiyor, **hasarın inmediğini** de ölçmek gerekiyor.

---

## İkinci durak: iki kanal, iki okuyucu

ADIM 0-1 ile ADIM 2-5 aynı zincirde ama aynı kanaldan konuşmuyorlar.

```
  "çağıranın kaydı ayrıştı"      "oyuncunun hamlesi reddedildi"
            │                                  │
            ▼                                  ▼
  ╔═ ArgumentException ═╗       ╔═ AttackOutcome / MoveOutcome ═╗
  ║ okuyucusu PROGRAMCI ║       ║ okuyucusu OYUN                ║
  ║ ele ALINAMAZ        ║       ║ ele ALINIR                    ║
  ║ doğru bir catch yok ║       ║ çağıran switch yazar          ║
  ╚═════════════════════╝       ╚═══════════════════════════════╝
    ██ AYRIM ÖLÇÜTÜ: bu cevabı alan çağıran YAPACAK BİR ŞEY
       bulabilir mi? Bulamıyorsa istisna. ██
```

Ölçüt cevabın *sertliği* değil, karşısında bir eylem durup durmadığı. Sonuç
enum'unun her satırının karşısında oyuncunun yapabileceği bir şey var: "bekle",
"yaklaş", "başka hedef seç". "Kodun bozuk" bu listenin üyesi olamaz — o satırın
karşısına yazılacak tek şey *hiçbiri*.

**İki kanal örtüşmüyor, bölüşüyor.** İstisna yolu silinirse "bu savaşta değil"
bir enum değerine iner ve sessizce yutulabilen bir dala girer. Sonuç değeri
yolu silinirse dolu hücreye her tıklama bir çökmeye dönüşür ve çağıran her
hamleyi `try/catch`'e sarar.

### Sınırın gözlenebilir olduğu tek yer

İki kanalın **sırası** genelde görünmez, çünkü ikisi aynı anda doğru olmaz.
Bir yerde oluyor:

```
Move(battle, unit, toX: 1, toY: 3, moveRange: -1)   ve sıra karşı tarafta
   │
   ├─► istisna kanalı  : moveRange < 0        ► ArgumentOutOfRangeException
   └─► sonuç kanalı    : TurnRules.CanAct ✗   ► RejectedActorCannotAct
                                    ██ İKİSİ DE DOĞRU ██
   dönen: ██ İSTİSNA ██
   çünkü profil kurulumu ADIM 0-1 bölgesinde, sıra kuralı ADIM 2'de
```

Bunu tutan test `Move_NegativeRangeOutOfTurn_StillThrows` — ve tek işi bu.
Bozuk sayı, sırası gelmemiş bir birimde de görülmek zorunda; yoksa çağıranın
hatası bir oyun sonucunun arkasına saklanır ve hiç fark edilmez.

---

## Üçüncü durak: iki ölçüt — ve biri sustuğunda konuşan öteki

ADIM 2-5 bölgesinin içinde sıra neye göre kuruluyor? İki ölçüt var ve
**sırayla** çalışıyorlar.

### DAYANIKLILIK

Önce, diğerleri düzeltilse bile **ayakta kalan** sebep sorulur.

3x5'lik bir tahta, menzili 1 olan bir birim, hedef (9,9). Üç ret sebebi de
aynı anda doğru:

```
   SEVİYE 1  board.IsInsideGrid(9,9)          ✗
             ██ AKIŞ BİTTİ ██ ──► RejectedInvalidDestination
   SEVİYE 2  GridDistance.Between > moveRange ✗  ── BURAYA GELİNMEZ
   SEVİYE 3  hedefte BAŞKASI var mı           ✗  ── BURAYA GELİNMEZ
```

(9,9) hücresi ne yaklaşarak ne bekleyerek geçerli olur. Diğer iki sebep
düzeltilse bile bu sebep ayakta kalır, dolayısıyla doğru cevap odur.
Sabitleyen test: `Execute_OutsideBoardAndOutOfRange_PrefersInvalidDestination`.

Aynı ölçüt akış katmanında sıra kuralını en öne koyuyor: sıra sende değilken
başka bir hedef seçmek de, yaklaşmak da hiçbir şeyi değiştirmez. Sabitleyen
test: `Attack_OutOfTurnAgainstAnInvalidTargetOutOfRange_PrefersActorCannotAct`.

### UCUZ + YEREL

Ama ölçüt her çifti ayıramaz:

```
   sınır  ile menzil   -> sınır, menzil düzeltilse bile kalır ► sınır önce
   sınır  ile doluluk  -> aynı şekilde                        ► sınır önce
   menzil ile doluluk  -> ikisi de ayakta kalır  ◄── ██ ÖLÇÜT SUSAR ██
```

Menzili düzeltirsen doluluk sürer, doluluğu düzeltirsen menzil sürer. Burada
ikinci ölçüt devreye giriyor: **menzil sorusu** çağıranın zaten verdiği
sayılara bakan saf bir aritmetiktir, **doluluk sorusu** ise tahtayı okur. Ucuz
ve yerel olan önce sorulur.

Ve kazandırdığı şey performans değil:

```
   doluluk ÖNCE sorulsaydı
        │
        └─► menzili 1 olan birim, tahtanın öbür ucundaki hücre için
            "orada biri var" cevabını alır
                 │
                 ├─► yol bulucu o hücreyi kalıcı engelli işaretler
                 └─► ██ sis geldiği gün, GÖRÜLEMEYEN bir hücrenin
                     içeriği ret sebebinden sızar ██
```

Sabitleyen test: `Execute_OccupiedCellOutOfRange_PrefersOutOfRange`.

**İki ölçüt aynı şeyi iki kez söylemiyor; iki farklı çifti ayırıyor.**

---

## Dördüncü durak: sıra iner, ama aynı sırayla iner

Akış katmanındaki ADIM 3-4-5, saldırıda hiç sorulmuyor. Üçü de bir kat aşağıda,
`AttackAction`'ın kendi merdiveninde:

```
   BattleActions.Attack              AttackAction.Execute
   ────────────────────────          ─────────────────────────────────
   ADIM 0  null                      0  null ► throw
   ADIM 1  RequireCombatant             ██ BURADAN SONRASI CEVAPTIR ██
   ADIM 2  TurnRules.CanAct ◄──┐     1  AttackRules.CanAttack
   ADIM 3  ·                   │        ► RejectedActorCannotAct
   ADIM 4  ·                   │     2  TargetingRules.CanBeAttacked
   ADIM 5  ·                   │        ► RejectedInvalidTarget
   ADIM 6  Execute ────────────┼──►  3  AttackResolver.IsWithinRange
                               │        ► RejectedOutOfRange
                               │     4  TakeDamage  ██ TEK YAZMA ██
                               │
   ██ BU KURAL İNEMEZ ██ ──────┘
   "aktif takım" kavramı GridStrategy.Combat'ta YOKTUR
```

Alt katmandaki 1-2-3, üst katmandaki ADIM 3-4-5'in **göreli sırasını aynen
koruyor**. Bu tesadüf değil — iki katmanda da aynı dayanıklılık ölçütü
uygulandı:

```
   1  saldıranın durumu  ► çağıran DÜZELTEMEZ: hedef de hücre de
                           değişse cevap aynı
   2  hedefin uygunluğu  ► çağıran BAŞKA HEDEF seçebilir
   3  menzil             ► çağıran YAKLAŞABİLİR
```

Merdiven düzeltilemeyenden düzeltilebilire doğru iniyor. Sabitleyen testler:
`Execute_DownedAttackerOutOfRange_PrefersActorCannotActOverOutOfRange`,
`Execute_SameTeamTargetOutOfRange_PrefersInvalidTargetOverOutOfRange`,
`Execute_DownedAttackerWithDeadTargetOutOfRange_PrefersActorCannotAct`.

Üçüncüsü ilginç: **üç** ret aynı anda doğru ve en tepedeki kazanıyor.

### Eyleyen kuralı ile hedef kuralı bilerek çelişir

Merdivenin 1. ve 2. basamağı aynı `UnitState`'i okuyor ve **zıt** cevap veriyor:

```
   UnitState.Downed
        │
        ├─► AttackRules.CanAttack        ► false  "artık bir SALDIRAN değil"
        └─► TargetingRules.CanBeAttacked ► true   "hâlâ geçerli bir HEDEF"
                                  ██ ASİMETRİ BİLEREK ██
```

Düşmüş birime vurmak "işini bitirme" yoludur ve tasarımın parçası. Kayda geçiren
test: `DownedUnit_IsStillAttackableButCannotAttack`.

Bu yüzden merdivenin 1 ve 2 basamağı birbirinden **türetilemez**. Aynı enum'u
okuyorlar ama iki ayrı fiilin kuralılar; bugün aynı satırı taşımaları
(`CanAttack`, `CanMove`, `CanRevive` — üçü de `== Alive`) bir tesadüf ve
`CanAttack_AndCanMove_StillAgree_WhichIsWhyTheyMustStaySeparate` tam olarak o
tesadüfü kayda geçiriyor.

---

## Beşinci durak: aynı iskelet, dört farklı yürüyüş

```
   Attack          0 1 2 · · ·  6 7    4-5 AttackAction'ın İÇİNDE
   Move            0 1 2 3 · ·  6 7    4-5 MoveAction'ın İÇİNDE
   Revive          0 1 2 3 4 5  6 7    tam zincir, hepsi akışta
   PlaceStructure  0 · · · 4 ·  6 ·    ◄── ██ KARŞI ÖRNEK: 2 de 7 de YOK ██
```

`Attack` satırındaki üçüncü `·`ye dikkat: ADIM 3 de aşağıya indi —
`AttackRules.CanAttack`, `AttackAction`'ın merdiveninin **birinci** basamağı.
Yani saldırıda üç adım birden alt katmanda yaşıyor.

`·` iki farklı sebeple konuluyor ve karıştırılmamalı:

- **Attack ve Move'da** soru sorulmuyor çünkü **alt katman soruyor**.
- **PlaceStructure'da** soru sorulmuyor çünkü **ortada soru yok**.

### İskelet kaba harita, alt katman ince sıra

`Move`'un ADIM 4-5'i `MoveAction`'ın içine indiğinde bir şey oluyor: numaralar
karışıyor.

```
   iskeletteki numara        MoveAction'ın gerçek sırası
   ──────────────────        ───────────────────────────
   ADIM 4  hücre kuralı ──► SEVİYE 1  IsInsideGrid    (hücre)
   ADIM 5  menzil       ──► SEVİYE 2  menzil          (menzil)
   ADIM 4  hücre kuralı ──► SEVİYE 3  doluluk         (hücre)
                                      ▲
              ██ MENZİL, HÜCRE KURALININ İKİ YARISI ARASINA GİRDİ ██
```

Bu bir tutarsızlık değil: iskelet **kaba** bir haritadır, alt katmandaki ince
sırayı yukarıdaki iki ölçüt belirler. Dayanıklılık sınırı tepeye çekti,
ucuz+yerel menzili doluluğun önüne geçirdi — ve ikisi birlikte hücre kuralını
ikiye böldü.

**Okunacak kural:** iskeletten "her hücre sorusu tek bir yerde durur" diye bir
şey çıkmaz. Çıkan tek şey şudur: **sıra sorulacaksa ADIM 2'de sorulur.**

### PlaceStructure neden sıra sormuyor

Çünkü imzasında **eyleyen yok**.

```
   Attack / Move / Revive          PlaceStructure
   ┌── EYLEYEN ──┐                 ┌── EYLEYEN ──┐
   │ Unit        │                 │    YOK      │
   └──────┬──────┘                 └──────┬──────┘
          │ Combatant.Team                │ ██ ödünç alınacak
          ▼                               ▼    tek alan: ██
   TurnRules.CanAct(team, ...)      structure.Team
                                          │
   Structure.Team bir SAHİPLİK değil bir AİDİYET:
   nötr duvar ──► Team.None ──► CanAct HER ZAMAN false
              ──► ██ nötr hiçbir yapı tahtaya bir daha konamaz ██
```

`TurnRules.CanAct` tarafsızlığı ilk satırda kesiyor — `Team.None` hiçbir sırada
eyleyemez. Yapının tarafını eyleyenin tarafı sanan bir satır, o kuralı
yanlış özneye sorar ve bütün tarafsız duvarları oyundan atar.

Kayda geçiren iki test:
`PlaceStructure_NeutralStructureOutOfTurn_IsStillPlaced` (ADIM 2 yok) ve
`PlaceStructure_DoesNotHandTheTurnOver` (ADIM 7 yok). İkisi bir çift: sırayı
sormayan eylem sırayı harcamaz da.

### PlaceStructure'ın kendi iki reddi gözlenemez

`PlaceStructure` içinde iki ret var — tahta dışı ve dolu hücre — ve sıraları
`MoveAction`'dan devralınmış bir gerekçeyle kuruluyor ("düzeltilemeyen sebep
önce"). Ama dürüst olmak gerekirse:

```
   x,y tahta DIŞINDA  ──► IsInsideGrid ✗
        │
        └─► tahta dışı bir hücrede kimse DURAMAZ
                 │
                 └─► TryGetUnit(x, y) zaten false döner
                     ██ İKİ RET AYNI ANDA DOĞRU OLAMAZ ██
                        ⇒ sıra GÖZLENEMEZ, test yazılamaz
```

Yani oradaki sıra bir karar değil, bir **tutarlılık**: aynı gerekçe aynı şekli
üçüncü kez kuruyor. Sırayı korumak için test aramak boşuna — korunacak bir
gözlem yok.

---

## Altıncı durak: gözlenemeyen sıra bir karar değildir

Aynı dürüstlük `Move`'un içinde de var, ve orada daha çarpıcı:

```
   BattleActions.Move
        │
        ├─► TurnRules.CanAct(...)      ✗ ► MoveOutcome.RejectedActorCannotAct
        │                                                    ▲
        └─► MovementRules.CanMove(...) ✗ ► MoveOutcome.RejectedActorCannotAct
                                                             │
                              ██ AYNI DEĞER ⇒ HANGİSİNİN ÖNCE SORULDUĞU
                                 HİÇBİR TESTTEN GÖRÜLEMEZ ██
```

İki kural art arda duruyor, ikisi de aynı değeri döndürüyor. Sıra kuralının
önde durması bir tercih — savaş dışı kısıt, savaş içi kısıttan önce — ve
**doğruluğu değil yalnızca okunuşu** etkiliyor.

Bunu ölçen bir test yok, ve olmaması doğru.
`Move_WhenItIsNotTheUnitsTurn_IsRejectedAndNothingMoves` ile
`Move_DownedUnit_IsRejected` her biri kendi kuralını ayrı ayrı sınıyor;
ikisini bir arada kuran bir "Prefers" testi yazılamaz çünkü ayırt edilecek iki
cevap yok.

**Sıra kararı ancak farklı cevaplar arasında var olur.** Değer ikiye ayrıldığı
gün (eşiği `MoveOutcome.cs`'te yazılı) bu sıra ölçülebilir hâle gelir ve o gün
bir karar olur; bugün değil.

Aynı ölçütün diğer yüzü: `Attack`'te iki ret **farklı** değer döndürüyor
(`RejectedActorCannotAct` ile `RejectedInvalidTarget`), arada tahtaya yazan bir
satır var, ve orada korunacak gerçek bir karar var — o yüzden orada "Prefers"
testleri var.

---

## Çizginin altı: sıra hâlâ önemli, ama başka bir sebeple

Geri dönülemez noktanın üstünde sıra **hangi ret kazanır** sorusunu
cevaplıyordu. Altında da bir sıra var ve orada soru değişiyor: **cevap doğru mu
söylüyor?**

```
   ADIM 6 bölgesi — AttackAction.Execute içinde
   ┌──────────────────────────────────────────────────┐
   │  UnitState stateBeforeHit = target.State;   ◄── ██ BU SATIR ██
   │  target.TakeDamage(...);                        aşağı taşınırsa
   │  return stateBeforeHit == Alive                 derleyici SUSAR,
   │      && target.State == Downed                  iki okuma AYNI
   │      ? HitAndDowned : Hit;                      değeri verir
   └──────────────────────────────────────────────────┘

   önce      sonra     tek okuma        iki okuma (SEÇİLEN)
   ───────   ───────   ──────────────   ───────────────────
   Alive     Alive     Hit              Hit
   Alive     Downed    HitAndDowned     HitAndDowned
   Downed    Downed    HitAndDowned ◄── YALAN      Hit
   Downed    Dead      Hit              Hit
```

"Düştü mü" bir **değişim** sorusudur, bir durum sorusu değil. Ve değişimi
okuyabilmenin tek yolu, okumanın yazmadan **önce** olması.

Üstteki sıra kararında yanlış yapmanın bedeli yanlış bir ret sebebiydi; buradaki
sıra kararında yanlış yapmanın bedeli enkaza her vuruşta oynayan bir düşme
animasyonu ve aynı birim için tekrar tekrar yazılan puan.

`Downed → Downed` satırı oynanışta en sık görülen satır, çünkü düşmüş birime
vurmak `TargetingRules`'ta bilerek serbest bırakıldı. Kayda geçiren test:
`Execute_HittingAnAlreadyDownedTarget_ReportsHitNotHitAndDowned`.

---

## Bütün karar tek bakışta

```
                    ┌─ ÇAĞIRAN HATALARI ─────────────────────┐
                    │  ADIM 0  null                          │
                    │  ADIM 1  bu savaşta mı                 │  ► İSTİSNA
                    │  (+ Move: new MoveProfile(range))      │    okuyucu:
                    └────────────────────────────────────────┘    PROGRAMCI
                                     │
                    ┌─ OYUN KURALLARI ───────────────────────┐
                    │  ADIM 2  SIRA        ◄── inemez, kural │
                    │                          TurnState     │  ► SONUÇ
                    │                          okumak zorunda│    DEĞERİ
                    │  ADIM 3  EYLEYEN'in durumu             │    okuyucu:
                    │  ADIM 4  HEDEF / HÜCRE                 │    OYUN
                    │  ADIM 5  MENZİL                        │
                    │      ▲ sıra: DAYANIKLILIK, sonra       │
                    │        UCUZ+YEREL                      │
                    └────────────────────────────────────────┘
   ═════════════════ ██ GERİ DÖNÜLEMEZ ÇİZGİ ██ ═══════════════════
                    ┌─ OLGULAR ──────────────────────────────┐
                    │  ADIM 6  TEK YAZMA                     │
                    │      ▲ sıra burada da var: okuma       │
                    │        yazmadan ÖNCE                   │
                    │  ADIM 7  sıra devri (beyaz liste)      │
                    └────────────────────────────────────────┘

   ÜSTTE  sıra ⇒ HANGİ ret kazanır
   ALTTA  sıra ⇒ cevap DOĞRU mu
   ÇİZGİNİN ALTINA DÜŞEN KURAL, KURAL OLMAKTAN ÇIKAR
```

---

## Kural: kendi ret sıranı nasıl kuracaksın

Sırayla sor. Her dal bir öncekinin cevabını varsayıyor.

```
① Bu ret, geri dönülemez bir adımdan SONRA mı söyleniyor?
   │  (tahtaya yazıldı mı, sayaç arttı mı, olay yayıldı mı)
   ├─ EVET → kural değil, AÇIKLAMA. Yukarı taşı, ①'e dön.
   └─ HAYIR → ②

② Bu ret ile komşusu aynı anda doğru olabiliyor mu?
   ├─ HAYIR → sıra GÖZLENEMEZ. Karar yok, test yazma.
   │          (PlaceStructure'ın sınır/doluluk çifti)
   └─ EVET → ③

③ İkisi FARKLI değer mi döndürüyor?
   ├─ HAYIR → sıra hâlâ gözlenemez. Tercih yaz, kural yazma;
   │          değer ayrıldığı gün geri gel.
   │          (Move'un iki RejectedActorCannotAct'i)
   └─ EVET → ④

④ Bu cevabı alan çağıran yapacak bir şey bulabilir mi?
   ├─ HAYIR → İSTİSNA kanalı. Her oyun kuralının ÖNÜNDE. Bitti.
   └─ EVET → ⑤

⑤ Sebeplerden biri, öteki DÜZELTİLSE bile ayakta kalıyor mu?
   ├─ EVET → ██ o önce ██        (DAYANIKLILIK)
   └─ HAYIR → ⑥

⑥ Biri ucuz ve YEREL, öteki paylaşılan durumu mu okuyor?
   ├─ EVET → ██ ucuz+yerel önce ██   (UCUZ + YEREL)
   │         kazanç hız değil: görülemeyen şeyin içeriği sızmaz
   └─ HAYIR → sıra serbest. Bir "Prefers" testi değil, bir
              yorum satırı yaz.
```

⑤'e her zaman güvenme: ölçüt yalnız bir sebep ötekini **kapsıyorsa** konuşur.
Sustuğunda ⑥'ya geç, ⑥ da susarsa dürüst ol ve sırayı bir karar gibi savunma.

---

## Yanlış hatırlanan üç şey

**"Sıra kuralı en başta sorulur."** Hayır — ADIM 0-1 onun önünde duruyor.
Korunan şey sıralamanın kendisi değil, **geri dönülemez adıma göre konumu**.
Yazmadan önceki bölgenin içinde sıra serbest, ve `Move`'da profil kurulumu
bilerek sıra kuralının önünde: bozuk bir sayı, sırası gelmemiş bir birimde de
görülmek zorunda (`Move_NegativeRangeOutOfTurn_StillThrows`).

**"Her akış sırayı sorar."** `PlaceStructure` sormuyor ve sormamalı — imzasında
eyleyen yok. Adım tablosundan okunacak kural "her metot sırayı sorar" değil,
"**sıra sorulacaksa ADIM 2'de sorulur**".

**"Ucuz olan önce sorulur."** Tersi. Önce dayanıklılık, ucuzluk ancak o
sustuğunda. `AttackAction`'ın merdiveninde saldıranın durumunu okumak ile hedef
uygunluğunu okumak **aynı fiyatta** (ikisi de bir enum karşılaştırması) ve
sırayı yine de dayanıklılık belirliyor. Ucuzluğun gerçekten öne geçtiği tek yer
`MoveAction`'ın menzil/doluluk çifti — ve orada bile asıl kazanç hız değil,
görülemeyen bir hücrenin içeriğinin ret sebebinden sızmaması.

---

## Kaçış yolu: tek bir `Rejected` değeri

Bütün bu sıra kararı tek bir şeyden doğuyor: **ret sebepleri birbirinden ayrı
değerler.** Ayrı olmasalardı hiçbiri gerekmezdi.

```csharp
✗ RejectedInvalidDestination, RejectedCellOccupied, RejectedOutOfRange, Moved
✓ Rejected, Moved
//  ▲ tek değer → hangi sebep kazanırsa kazansın çağıran AYNI şeyi görür
//                → sıra gözlenemez → bu dosyadaki her figür silinir
```

**Ne kazandırırdı:** iki ölçüt de gereksizleşirdi. "Prefers" testleri
yazılamazdı çünkü ayırt edilecek iki cevap olmazdı. Ret sırası gerçekten bir
üslup tercihine dönerdi ve kimse yanlış yapamazdı.

**Ne kaybettirirdi:** çağıranın davranış ayrımı.

```
   ret sebebi                   bir tur sonra?  çağıranın işi
   ──────────────────────────   ─────────────   ─────────────────
   RejectedInvalidDestination   ASLA değişmez   bir daha hiç deneme
   RejectedCellOccupied         DEĞİŞEBİLİR     bekle, hücre boşalır
   RejectedOutOfRange           DEĞİŞEBİLİR     önce YAKLAŞ, sonra dene
                                      ██ ÜÇ SATIR, İKİ AYRI DAVRANIŞ ██
```

Tek değer bu çizgiyi siler ve çağıran üç satıra tek davranış yazmak zorunda
kalır — hangisini yazarsa yazsın öteki satırlarda yanlış olur. Yapay zekâ ya
tahta dışı bir hücreyi her turda yeniden dener, ya dolu bir hücreden kalıcı
vazgeçer.

**Ne zaman kazanırdı:** sonucu yalnızca arayüz tüketseydi ve tek yaptığı şey
geçersiz tıklamada bir uyarı sesi çalmak olsaydı — üç değer aynı sesi çalmanın
üç yolu olurdu.

### Ve proje bu kaçışı zaten kısmen kullanıyor

`RejectedActorCannotAct` **üç** ayrı sebebi bilerek tek değerde topluyor:
hareket eden düşmüş, saldıran düşmüş, sırası değil. Aynı enum, ters karar.

Çelişki değil, tek ölçütün iki yönü:

```
   ayırıcı şey SEBEP sayısı değil, ██ DAVRANIŞ sayısı ██

   üç sebep, üç davranış  ► üç değer   (Destination / Occupied / OutOfRange)
   üç sebep, tek davranış ► tek değer  (RejectedActorCannotAct)
                             ▲ üçünde de yapılacak tek şey aynı:
                               bekle ya da başka birim seç
```

**Kendi akışını yazarken** ölçüt bu: ret sebebini ayırmadan önce, çağıranın o
iki cevaba **farklı** tepki verip vermeyeceğini sor. Vermeyecekse ayırma —
ayırdığın gün bir sıra kararı doğar ve onu bir testle savunmak zorunda kalırsın.

---

## Bunu okuduktan sonra kodda ne göreceksin

`BattleActions.cs`'in sınıf başlığındaki ADIM 0-7 zinciri artık bir liste değil:
altıncı satırdaki çizgi bir sınır, üstündeki her `if` bir soru, altındaki her
satır bir olgu. `MoveAction`'daki SEVİYE tablosu ile `AttackAction`'daki erken
çıkış merdiveni aynı iki ölçütün iki uygulaması.

██ **Bu üç tipin desen adı bu dosyada geçmiyor** ██ — `BattleActions`,
`MoveAction` ve `AttackAction` birer **akış sahibi**dir (transaction script) ve
bir **Command değildir**; ölçüsü üçünün de `static class` olması ve tek bir
alan taşımaması. Adı, doğuran baskısı ve neden Command olmadığı:
[`../../ogrenme/01-koda-gomulu-desenler.md`](../../ogrenme/01-koda-gomulu-desenler.md#2-akis-sahibi-transaction-script-command-degil).
"Bu projede hangi desenleri kullandın" sorusunun cevabı orada.

Kodda karar, burada hikâye. İkisi çelişirse **kod kazanır** — orası çalışan
metin, burası anlatı.
