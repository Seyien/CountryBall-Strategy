# Ölmek bir an değil bir süreç — üç durum ve yapının ikizi

> **Nerede geçiyor:** `UnitState.cs` → `UnitLifecycle.cs` → `TargetingRules.cs` → `Battle.cs`
> ikizi: `StructureState.cs` → `StructureLifecycle.cs`
> **Kodda nereden geldin:** `UnitLifecycle.Tick`, `UnitLifecycle.OnHealthDepleted`,
> `UnitLifecycle.TryRevive`, `TargetingRules.CanBeAttacked`, `TargetingRules.CanBeRevived`,
> `Battle.RemoveReadyForCleanup`
> **Ne zaman oku:** `UnitState`'e dördüncü bir değer eklemeden önce, ya da "yapının
> neden `TryRevive`'ı yok" diye sorduğunda.

---

## Sahne

Okçu ateş ediyor. Askerin canı sıfıra iniyor. Asker yere yatıyor.

On beş saniye sonra ekranda o askerden hiçbir iz kalmıyor.

Tek cümle. Ama arada **iki ayrı geri sayım**, **üç ayrı durum** ve "bu birime ne
yapılabilir" sorusunun **beş ayrı cevabı** var. Ve o on beş saniyenin ilk onunda
asker aynı anda hem **vurulabilir** hem **diriltilebilir** — ikisi de `true`, ve
bu bir çelişki değil.

Bu dosya üç durumun neden üç olduğunu, sayacı kimin çevirdiğini ve barakanın
neden hiç düşmediğini anlatıyor.

---

## Karakterler

Yaşam ekseninde altı tip çalışıyor. Hikâyeyi ilginç kılan, her birinin
**bilmediği** şey.

```
╔═ UnitState (enum) ════════════════════════════════════════════╗
║  İşi     : üç durumu ADLANDIRMAK                              ║
║  Bilir   : Alive / Downed / Dead — ve dördüncü hâl YOKTUR     ║
║  BİLMEZ  : geçişleri. Hangi sırayla gidileceğini. Süreyi.     ║
╚═══════════════════════════════════════════════════════════════╝

╔═ UnitLifecycle ═══════════════════════════════════════════════╗
║  İşi     : durum + tek bir geri sayım alanı                   ║
║  Bilir   : hangi durumdayım, ne kadar kaldı, hangi geçiş yasak║
║  BİLMEZ  : ██ CANIN KAÇ OLDUĞUNU ██ · kimin dirilttiğini ·    ║
║            sahnede neyin silineceğini · Time.deltaTime'ı      ║
╚═══════════════════════════════════════════════════════════════╝

╔═ Health ══════════════════════════════════════════════════════╗
║  İşi     : bir sayı tutmak                                    ║
║  Bilir   : current, Max, "kalan var mı"                       ║
║  BİLMEZ  : ██ SAHİBİNİN NE OLDUĞUNU ██ — asker mi baraka mı;  ║
║            "canlı", "ayakta", "sağlam" diyemez                ║
╚═══════════════════════════════════════════════════════════════╝

╔═ TargetingRules ══════════════════════════════════════════════╗
║  İşi     : "bu hedefe bu yetenek uygulanabilir mi"            ║
║  Bilir   : İKİ durum dili — UnitState ve StructureState       ║
║  BİLMEZ  : canı. Menzili. Sırayı. Kimin ne yaptığını.         ║
╚═══════════════════════════════════════════════════════════════╝

╔═ StructureLifecycle ══════════════════════════════════════════╗
║  İşi     : birimin ikizi — iki durum ve enkaz sayacı          ║
║  Bilir   : Standing / Destroyed, kalan enkaz süresi           ║
║  BİLMEZ  : ██ OLAY DİYE BİR ŞEYİ ██ · diriltmeyi · onarımı ·  ║
║            canı · takımı                                      ║
╚═══════════════════════════════════════════════════════════════╝

╔═ Battle ══════════════════════════════════════════════════════╗
║  İşi     : saati çevirmek ve süpürmek                         ║
║  Bilir   : kim savaşta — savaşçılar VE yapılar                ║
║  BİLMEZ  : durumların anlamını. Ceset ile enkazı AYIRT ETMEZ. ║
╚═══════════════════════════════════════════════════════════════╝
```

En tuhafı ikincisi: **`UnitLifecycle` canı bilmiyor.** Düşme kararını veren tip,
düşmeye sebep olan sayıyı hiç görmüyor. Ona "can bitti" diye **haber veriliyor**
(`Combatant.TakeDamage` içinden) ve o haberin doğruluğunu denetleyemiyor.

**Bütün asimetri bu tek karardan doğuyor.** Aklında tut — yapı tarafında aynı
sınır bambaşka bir sonuç veriyor.

---

## Neden üç durum: iki olsaydı hangi cümle yazılamazdı

`UnitState` doğmadan önce durumu tutan şey bir `bool`du. İki değerle şu cümle
yazılamıyor:

> **"Ölü ama on saniye içinde diriltilebilir — ve bu sırada hasar almaya
> DEVAM ediyor."**

Kelimeleri tek tek ayır, her biri farklı bir mekanizmayı öldürüyor:

```
   "ölü"                → canı sıfır, ayakta değil, yürüyemez, vuramaz
   "ama diriltilebilir" → geri dönen bir ok var; Dead'de o ok YOK
   "hasar almaya devam" → HÂLÂ geçerli bir hedef; ceset ise değil
```

İki durumla üçünü birden söylemenin yolu yok. Denenebilecek her yol bir şeyi
kaybediyor:

```
   "yaşıyor" sayarsan  ► yürür, vurur, diriltmeye ihtiyacı olmaz
                         ◄── ██ PENCERE HİÇ AÇILMAZ ██
   "ölü" sayarsan      ► CanBeAttacked hayır der, işini bitirme yolu kapanır
                         ve diriltme "ölüyü diriltme"ye dönüşür
                         ◄── ██ PENCERE KAPANMAZ, YOK OLUR ██
   iki bool yazarsan   ► isAlive && isDowned yazılabilir hâle gelir
                         dördüncü hücre anlamsız ama DERLENİR
```

Üçüncü durum bir konfor değil: **`Downed`'ın var olma sebebi tam olarak
`Downed → Alive` geri okudur.** O ok silinirse `Downed`, `Dead`'in uzun yazılışı
olur ve enum üç değere gerek duymaz.

---

## Birinci durak: düşüş — can biter, sayaç başlar

Zincir üç tipten geçiyor ve her biri diğerinin işine karışmıyor:

```csharp
// Combatant.TakeDamage
health.TakeDamage(amount);          // ① SAYI değişir — Health'in işi

if (!health.HasRemaining)           // ② OLGU doğrulanır — Combatant'ın işi
{
    lifecycle.OnHealthDepleted();   // ③ DURUM değişir — UnitLifecycle'ın işi
}
```

Üçüncü satır bir **bildirim**: "canın bittiğini sana söylüyorum". `UnitLifecycle`
bunu doğrulayamaz, çünkü canı görmüyor. Bu yüzden kapı **kendi tarafında**
duruyor:

```csharp
public void OnHealthDepleted()
{
    if (State != UnitState.Alive)   // ◄── ██ KESTİRME BURADA KAPANIYOR ██
    {
        return;                     //     sessizce; soran yok, cevap da yok
    }

    SetState(UnitState.Downed);
    remainingSeconds = downedWindowSeconds;   // 10f
}
```

Bu `if` olmasaydı ne olurdu: düşmüş bir birimin canı zaten sıfırdır, yani ona
değen **her** sonraki vuruş `!health.HasRemaining` sınavını geçer ve
`OnHealthDepleted` tekrar çağrılır. Kapı olmasaydı ikinci sıyırık onu `Dead`'e
atardı — yani **alan hasarı istemeden "işini bitirme" olurdu** ve kurtarma
penceresi diye bir şey kalmazdı.

Tasarımda `Downed → Dead`'in **iki** yolu var ve bugün yalnız biri yazılı:

```
   ① geri sayımın dolması      ► Tick'te, YAZILI
   ② "işini bitirme"           ► kendi kuralı (düşme canı), HENÜZ YAZILMADI
                                 ◄── ██ o kuralın yazılacağı yer, bu if ██
```

Kestirmeyi sessizce eklemek ②'yi ①'in yerine koyardı ve ikinci kuralın
yazılacağı yeri de ortadan kaldırırdı.

---

## İkinci durak: pencere açık — iki kural aynı anda EVET diyor

Aynı birim, aynı kare, iki soru:

```csharp
TargetingRules.CanBeAttacked(UnitState.Downed)   // ► true
TargetingRules.CanBeRevived (UnitState.Downed)   // ► true
```

İlk bakışta çelişki gibi duruyor. Değil — çünkü **iki farklı yeteneğin iki
farklı hedef kümesi** var ve `Downed` onların **kesişimi**:

```
   UnitState = { Alive, Downed, Dead }

     saldırı kümesi                diriltme kümesi
     ┌────────────────────┐        ┌───────────────┐
     │ Alive              │        │               │
     │         ┌──────────┼────────┼──────┐        │
     │         │  Downed  │ ◄── ██ KESİŞİM ██      │
     │         └──────────┼────────┼──────┘        │
     └────────────────────┘        └───────────────┘
              Dead: İKİSİNİN DE DIŞINDA

   Alive  ► vurulur, DİRİLTİLEMEZ   ◄── kümelerin ayrıştığı yer
   Downed ► ikisine de açık          ◄── kesişim: TEK eleman
   Dead   ► ikisine de kapalı
```

Kesişimin tek elemanlı olması bir tesadüf değil, **oyunun kendisi**: on saniye
boyunca düşman ile dost aynı bedene farklı sebeplerle koşuyor. Biri bitirmeye,
diğeri kaldırmaya. `Downed` bir durum değil, bir **yarıştır**.

İkisi birbirini dışlasaydı yarış biterdi:

```
   CanBeAttacked'dan Downed çıkarılsaydı  ► bitirme yolu kapanır, düşman
                                            beklemekten başka bir şey yapamaz
   CanBeRevived'dan Downed çıkarılsaydı   ► kaldırma yolu kapanır, dost
                                            beklemekten başka bir şey yapamaz
   ◄── ██ İKİ HÂLDE DE PENCERE BOŞ BİR GERİ SAYIMA DÖNÜŞÜR ██
```

Ve bu iki cevabı sabitleyen tek bir test var —
`TargetingRulesTests.Downed_IsTheOnlyStateBothAbilitiesAccept`. Adı doğrudan
kesişimi söylüyor.

### Ama hedef olmakla eyleyen olmak ayrı eksenler

Aynı `Downed` birim, **eyleyen** olarak sorulduğunda her kapıdan geri dönüyor:

```
                    EYLEYEN olarak            │      HEDEF olarak
                   (kim yapabilir)            │  (kime yapılabilir)
   ─────────────────────────────────────────────────────────────────────
              CanMove   CanAttack   CanRevive │ CanBeAttacked  CanBeRevived
   Alive         ✓          ✓           ✓     │       ✓              ✗
   Downed        ✗          ✗           ✗     │       ✓              ✓
   Dead          ✗          ✗           ✗     │       ✗              ✗
   ─────────────────────────────────────────────────────────────────────
                                              ▲
              ██ Downed satırı: EYLEYEN DEĞİL, ama HEDEF ██
```

Yerdeki asker vuramaz, yürüyemez, arkadaşını kaldıramaz — ama vurulabilir ve
kaldırılabilir. Sol blok üç ayrı dosyaya (`MovementRules`, `AttackRules`,
`ReviveRules`) dağılmış; sağ blok tek dosyada (`TargetingRules`) iki metot.

---

## Üçüncü durak: sayacı kim çeviriyor, pencere nasıl kapanıyor

`UnitLifecycle` zamanı **okumuyor**; zaman ona dört katman yukarıdan geliyor:

```
Time.deltaTime                              ◄── ██ TEK MOTOR TEMASI ██
   │
   ▼  BoardAdapter.Update()  →  AdvanceBattleTime()
battle.Tick(Time.deltaTime)
   │
   ├─► foreach (combatants) → Combatant.Tick(dt) → lifecycle.Tick(dt)
   └─► foreach (structures) → Structure.Tick(dt) → lifecycle.Tick(dt)
        ◄── ██ İKİ DÖNGÜ, TEK ÇAĞRI: ayrı metot olsaydı biri unutulurdu ██
```

`Update` içinde `AdvanceBattleTime` **erken çıkışların üstünde** duruyor ve bu
bir karar: altına konsaydı savaşın saati yalnızca oyuncu tıkladığında işlerdi —
yani düşmüş bir birim, el sürülmediği sürece asla ölmezdi.

### Geri sayımın sahibi tek bir alan

İki geri sayım var (10 sn kurtarma, 5 sn ceset) ama **tek bir alan**:

```
   durum    downedRemaining   corpseRemaining   tek alan (SEÇİLEN)
   ────────────────────────────────────────────────────────────────
   Alive    (anlamsız)        (anlamsız)        (anlamsız, 0 döner)
   Downed   İŞLİYOR           boşta             İŞLİYOR
   Dead     boşta             İŞLİYOR           İŞLİYOR
   ────────────────────────────────────────────────────────────────
        ◄── ██ HİÇBİR SATIRDA İKİSİ BİRDEN İŞLEMİYOR ██
```

"Hangisi işliyor" bilgisi zaten `State`'te duruyor. İkinci bir alan onu ikinci
kez saklardı ve `TryRevive`'ın birini sıfırlayıp diğerini unuttuğu gün dirilen
bir asker yanında **eski bir ceset sayacı** taşırdı.

### Pencere iki uçtan kapanıyor

```csharp
// UÇ ① — süre dolarak
public void Tick(float deltaSeconds)
{
    if (State == UnitState.Alive) { return; }   // ayakta sayaç yok

    remainingSeconds -= deltaSeconds;
    if (remainingSeconds > 0f) { return; }

    if (State == UnitState.Downed)
    {
        SetState(UnitState.Dead);               // ◄── ██ PENCERE KAPANDI ██
        remainingSeconds = corpseWindowSeconds; // 5f — ikinci sayaç başladı
        return;
    }

    remainingSeconds = 0f;
    IsReadyForCleanup = true;                   // Dead: durum DEĞİŞMEDİ
}
```

```csharp
// UÇ ② — kurtarılarak
public bool TryRevive()
{
    if (State != UnitState.Downed) { return false; }   // ceset dirilmez

    SetState(UnitState.Alive);
    remainingSeconds = 0f;                             // sayaç susar
    return true;
}
```

Ve `Combatant.TryRevive` bunun üstüne canı yazıyor — **tam canla değil**:

```csharp
health.Heal(health.Max / ReviveHealthDivisor);   // ReviveHealthDivisor = 2
```

Diriltmek ölümü geri almak değil, riskli bir yatırım: kalkan asker yarım canla
kalkıyor ve ikinci vuruşta yeniden düşüyor.

---

## Dördüncü durak: ceset — durum değişmeden biten sayaç

Ceset sayacının dolduğu `Tick` bu dosyanın en sessiz anı:

```
   Downed → Dead olan Tick      ► DURUM değişir  → StateChanged TETİKLENİR
                                                   ekran griye döner
   Dead → temizlik olan Tick    ► DURUM DEĞİŞMEZ → StateChanged TETİKLENMEZ
                                  yalnız bir bayrak açılır
                                  ◄── ██ OLAYIN GÖREMEDİĞİ AN ██
```

Bu tek satır, `Battle.UnitStateChanged` olayının neden süpürmenin **yerine
geçemeyeceğini** söylüyor: olay "durum değişti" der, süpürme "artık silinebilir"
der. Bir birim `Dead`'e geçtiği an ekranda gri olur ama savaşın kaydından ancak
beş saniye sonra çıkar — ve olay o ikinci anı hiç bilmez.

Süpürme `Battle.RemoveReadyForCleanup` içinde, iki döngü tek tampon:

```
   savaşçı döngüsü ► ceset süresi dolanlar   ─┐
                                              ├─► removed (AYNI liste)
   yapı döngüsü    ► enkaz süresi dolanlar   ─┘
                       │
                       ▼
   ikinci geçiş: for (i...) RemoveUnit(removed[i])
   ◄── ██ İKİ GEÇİŞ ZORUNLU: sözlükte dönerken silmek fırlatır ██
```

Çağıran ceset ile enkazı **ayırt etmiyor** ve etmesine gerek de yok: elindeki iş
ikisinde de aynı — o kimliğin görselini sahneden kaldırmak.

---

## Yapının ikizi: aynı iskelet, üç eksik uzuv

`StructureLifecycle`, `UnitLifecycle`'ın kısaltılmışı değil. Üç şey eksik ve
**üçü de bilerek**:

```
                          BİRİM                    YAPI
   ─────────────────────────────────────────────────────────────────────
   durum sayısı           3 (Alive/Downed/Dead)    2 (Standing/Destroyed)
   geri dönen ok          TryRevive ✓              ██ YOK ██
   StateChanged olayı     ✓                        ██ YOK ██
   SetState tek kapısı    ✓                        ██ YOK ██ (2 yazan var)
   geri sayım             10 sn + 5 sn             8 sn (enkaz)
   OnHealthDepleted       void                     ██ bool ██ döndürür
   onarım/iyileştirme     Combatant.TryRevive      Structure.TryRepair
                          (durum + can)            (durum + can)
   ─────────────────────────────────────────────────────────────────────
```

Her eksiğin kendi gerekçesi var ve hiçbiri "yazmaya üşendik" değil:

**Ara durum yok** — çünkü bir baraka **düşmez**. `Downed`'ın tek var olma sebebi
diriltme penceresidir; yapıda öyle bir pencere yok. Yıkılan bina onarılmaz,
yeniden **inşa** edilir — ve yeniden inşa bu enum'un bir geçişi değil, yepyeni
bir `Structure` nesnesidir.

**Olay yok** — çünkü buradaki tek geçişin **soranı var**:

```
   UnitLifecycle                     StructureLifecycle
   ───────────────────────────────   ────────────────────────────────
   Alive → Downed                    Standing → Destroyed
     OnHealthDepleted içinde           OnHealthDepleted içinde
     çağıran soruyor                   çağıran soruyor VE cevabı
                                       DÖNÜŞ DEĞERİYLE alıyor  ✓

   Downed → Dead                     (BÖYLE BİR GEÇİŞ YOK)
     Tick içinde, kimse sormadan
     ◄── ██ EVENT'İ HAKLI ÇIKARAN TEK SATIR ORASIYDI ██

   Dead → temizlik                   Destroyed → temizlik
     Tick, DURUM değişmiyor            Tick, DURUM değişmiyor
     bayrak açılıyor: yeter            bayrak açılıyor: yeter
```

Gerekçe kopyalanmadı — **aynı soru burada da soruldu ve cevabı farklı çıktı.**
Olay eklenseydi aynı olgu iki yoldan duyurulurdu ve hem dönüşü okuyan hem abone
olan UI **yıkım sesini iki kez çalardı**.

**Diriltme yok, onarım başka evde** — ve bu ayrım en kolay karıştırılan satır:

```
   diriltme = DURUM geçişi (yıkık → ayakta)   ► lifecycle'ın işi olurdu
   onarım   = SAYI değişikliği (can artar)    ► Health'in işi
```

`TryRepair` `StructureLifecycle`'a konsaydı ne olurdu:

```
   ┌──────────────── Structure (bileşik) ────────────────┐
   │  health ────────► Health              (SAYI)        │
   │  lifecycle ─────► StructureLifecycle  (DURUM)       │
   │       ◄── ██ İKİSİNİ AYNI ANDA GÖREN TEK YER ██     │
   └─────────────────────────────────────────────────────┘

   lifecycle.TryRepair() burada dursaydı:
     State = Standing   ✓ yazılır
     Current            ✗ görülmez, sıfırda kalır
       -> SIFIR CANLA AYAKTA duran bir bina
       -> değen ilk hasar onu anında tekrar yıkar
       -> hata "bina bazen hemen yıkılıyor" diye gelir
```

Bu yüzden kelepçe `Structure.TryRepair`'in içinde:

```csharp
if (!IsStanding) { return false; }   // canı ve durumu aynı anda gören tek yer
...
health.Heal(amount);
```

**Kural:** bir geçiş, dayandığı olguyu ya kendi görmeli ya da çağıran onu
doğrulamış olarak getirmelidir. `OnHealthDepleted` ikincisini yapıyor
(`Structure.TakeDamage` içindeki `if (!health.HasRemaining)`), `TryRepair`'in
getireceği bir olgu yoktu — "yıkık olmak" tek başına "onarılabilir" demiyor.

---

## Tek bakışta: iki durum makinesi ve yasak geçişler

```
╔═══════════════════ BİRİM — UnitLifecycle ════════════════════════════╗
║                                                                      ║
║   kurucu ──► ╔═══════╗ ◄──────────────────┐                          ║
║              ║ Alive ║                    │                          ║
║              ╚═══╤═══╝                    │                          ║
║                  │                        │                          ║
║      OnHealthDepleted()             TryRevive()                      ║
║      (Combatant haber verir)        sayaç = 0                        ║
║      sayaç = 10 sn                  can  = Max / 2                   ║
║                  │                        │                          ║
║                  ▼                        │                          ║
║              ╔════════╗───────────────────┘                          ║
║              ║ Downed ║  ◄── ██ KURTARMA PENCERESİ — 10 sn ██        ║
║              ╚═══╤════╝      vurulabilir VE diriltilebilir           ║
║                  │           (kesişim: TEK durum)                    ║
║      Tick: sayaç dolar                                               ║
║      sayaç = 5 sn (ceset)                                            ║
║                  │                                                   ║
║                  ▼                                                   ║
║              ╔══════╗   ◄── ██ SON DURUM: GERİ OK YOK ██             ║
║              ║ Dead ║       hedeflenemez, diriltilemez               ║
║              ╚══╤═══╝                                                ║
║                 │                                                    ║
║      Tick: ceset sayacı dolar                                        ║
║      ██ DURUM DEĞİŞMEZ — StateChanged TETİKLENMEZ ██                 ║
║                 │                                                    ║
║                 ▼                                                    ║
║      IsReadyForCleanup = true                                        ║
║                 │                                                    ║
║                 └──► Battle.RemoveReadyForCleanup ──► savaştan çıkar ║
╚══════════════════════════════════════════════════════════════════════╝

   YASAK GEÇİŞLER — her biri tek bir `if` tarafından tutuluyor
   ─────────────────────────────────────────────────────────────────────
   Downed ──can bitti──► Dead    ██ YOK ██  OnHealthDepleted:
                                            `if (State != Alive) return;`
                                            "işini bitirme" ayrı kural,
                                            HENÜZ YAZILMADI
   Dead ──TryRevive──► Alive     ██ YOK ██  TryRevive:
                                            `if (State != Downed) return false;`
   Alive ──TryRevive──► Alive    ██ YOK ██  aynı kapı; false döner
   Dead ──► Downed               ██ YOK ██  hiçbir yol yazmıyor
   Alive ──Tick──► herhangi bir  ██ YOK ██  Tick:
                                            `if (State == Alive) return;`
   X ──► X (aynı duruma geçiş)   ██ YOK ██  SetState: `if (State == next) return;`
                                            olay iki kez tetiklenmez
   ─────────────────────────────────────────────────────────────────────
   Üç kapı, üç FARKLI yanlış durumu tutuyor. Biri silinirse:
     OnHealthDepleted'inki ► alan hasarı istemeden "bitirme" olur
     Tick'inki             ► ayakta birimin sayacı eksiye gider
     TryRevive'ınki        ► ceset dirilir
```

```
╔═══════════════ YAPI — StructureLifecycle (ikiz) ═════════════════════╗
║                                                                      ║
║   kurucu ──► ╔══════════╗                                            ║
║              ║ Standing ║   ◄── ██ GERİ OK YOK — YOKLUĞU KURALDIR ██ ║
║              ╚════╤═════╝       yıkık bina onarılmaz, yeniden        ║
║                   │             İNŞA edilir (= yeni nesne)           ║
║      OnHealthDepleted() ──► bool döner: "bu vuruş mu yıktı"          ║
║      sayaç = 8 sn (enkaz)                                            ║
║                   │                                                  ║
║                   ▼                                                  ║
║              ╔═══════════╗  ◄── enum'un SIFIRINCI değeri:            ║
║              ║ Destroyed ║      atanmayı unutulan alan tahtada       ║
║              ╚═════╤═════╝      SAĞLAM bina üretmesin diye           ║
║                    │                                                 ║
║      Tick: enkaz sayacı dolar → IsReadyForCleanup = true             ║
║                    │                                                 ║
║                    └──► Battle.RemoveReadyForCleanup ──► AYNI tampon ║
╚══════════════════════════════════════════════════════════════════════╝
   ██ OLAY YOK · DİRİLTME YOK · ARA DURUM YOK · SetState YOK ██
   ve dördü de ayrı ayrı gerekçelendirilmiş birer RED.
```

---

## Beyaz liste mi kara liste mi: aynı enum, iki dosyada zıt biçim

Durum okuyan altı kuralın **beşi kapalı uçlu** (`==`), yalnız biri açık uçlu
(`!=`):

```
   metot                                     biçim        yarın `Frozen` gelirse
   ──────────────────────────────────────────────────────────────────────────
   TargetingRules.CanBeAttacked(UnitState)   != Dead      ✓ HEDEFLENEBİLİR SAYILIR
   TargetingRules.CanBeRevived (UnitState)   == Downed    ✗ diriltilemez
   MovementRules.CanMove                     == Alive     ✗ yürüyemez
   AttackRules.CanAttack                     == Alive     ✗ vuramaz
   ReviveRules.CanRevive                     == Alive     ✗ kaldıramaz
   TargetingRules.CanBeAttacked(Structure)   == Standing  ✗ hedeflenemez
   ──────────────────────────────────────────────────────────────────────────
                                             ▲
                    ██ TEK AÇIK UÇLU KURAL — ve bilerek öyle ██
```

Bugün ikisi de aynı cevabı veriyor; fark **enum büyüdüğü gün** doğuyor:

```
   StructureState değeri   `== Standing`   `!= Destroyed`
   ─────────────────────   ─────────────   ──────────────
   Destroyed               false           false
   Standing                true            true
   ── bugünkü enum BURADA bitiyor ──────────────────────────
   Rubble    (yarın)       false           true   ◄── AYRIŞMA
   Damaged   (yarın)       false           true   ◄── AYRIŞMA
   Burning   (yarın)       false           true   ◄── AYRIŞMA
```

Kapalı uç derleyiciyi değil **programcıyı** çağırır: yeni değer varsayılan olarak
hedeflenebilir sayılmaz, biri gelip o satırı açmak zorunda kalır. Açık uç ise yeni
değeri **sessizce kabul eder** ve hiçbir test kırmızıya dönmez.

Ayıraç tek soru:

```
   yeni durum eklendiğinde varsayılan cevap ne OLMALI?
     EVET olmalı   ► açık uç    (birim: != Dead — düşmüşe vurmak tasarımın parçası)
     HAYIR olmalı  ► kapalı uç  (yapı:  == Standing)
```

Aynı dosya, aynı soru, zıt biçim — ve ikisi de doğru. `CanBeAttacked(UnitState)`
açık uçlu, çünkü orada gelecekteki her yeni durumun **hedeflenebilir olması
isteniyor**: `Frozen`, `Stunned`, `Burning` — hepsi vurulabilir olmalı.
`CanBeAttacked(StructureState)` kapalı uçlu, çünkü orada tersi isteniyor.

---

## Kural: yeni bir durum eklerken hangi listeleri gözden geçirmen gerekir

`UnitState`'e dördüncü bir değer eklediğin gün derleyici sana **neredeyse hiçbir
şey söylemez**. Bu ağacı sırayla yürü:

```
① Bu değer en az BİR kuralın cevabını değiştiriyor mu?
     HAYIR → ██ EKLEME ██. Bu bir durum değil, bir İSTEK.
             İstekler bayrakla yazılır (IsReadyForCleanup gibi).
             Kanıt: Rubble tam bu sınavda reddedildi — dört kuralın
             hiçbirinde Destroyed'dan ayrışmıyordu.
     EVET  → ②

② Yeni değere GİDEN bir geçiş yazdın mı? DÖNEN bir ok var mı?
     giden yok  → değer ulaşılamaz; her switch'te ÖLÜ DAL doğar
     dönen yok  → son durumdur; Dead'in yanına oturur
     ikisi de   → ③   ◄── Downed tam olarak buraya düşmüştü

③ ██ ALTI KURALI TEK TEK AÇ ██ — derleyici hiçbirini göstermez:
     TargetingRules.CanBeAttacked(UnitState)   != Dead    ► SESSİZCE EVET der
     TargetingRules.CanBeRevived (UnitState)   == Downed  ► sessizce hayır der
     MovementRules.CanMove                     == Alive   ► sessizce hayır der
     AttackRules.CanAttack                     == Alive   ► sessizce hayır der
     ReviveRules.CanRevive                     == Alive   ► sessizce hayır der
     UnitView.TintFor                          switch     ► default'a düşer,
                                                            LogError basar
                                              ◄── ██ TEK GÜRÜLTÜ ÇIKARAN YER ██

④ UnitLifecycle'ın ÜÇ KAPISINI aç:
     OnHealthDepleted `!= Alive`  ► yeni durumdayken can biterse ne olmalı?
     Tick `== Alive` erken çıkış  ► yeni durumun bir geri sayımı var mı?
     TryRevive `!= Downed`        ► yeni durumdan kalkılabilir mi?

⑤ Geri sayım eklediysen: `remainingSeconds` TEK alan.
     Yeni durum, mevcut bir sayaçla AYNI ANDA mı işleyecek?
       HAYIR → tek alan yeter, hiçbir şey yapma
       EVET  → ██ ancak o gün ikinci alan hak edilir ██

⑥ Yapı tarafına da eklenecek mi?
     Aynı DEĞER  ama farklı GEÇİŞ  → ██ EKLEME ██, iki enum ayrı kalsın
     Aynı GEÇİŞ                    → o gün tek enum tartışılabilir (aşağı bak)
```

③ ile ④ arasındaki fark önemli: **③ sessizce yanlış cevap verir, ④ sessizce
yanlış davranır.** İkisi de derleme hatası üretmez. Enum'un asıl kazancı uyarı
değil, **geçersiz hâlin yazılamaması** — `switch` ifadesi (`expression`) eksik
dalda `CS8509` verir ama `switch` deyimi ve iç içe `if` **sessizdir**.

---

## Yanlış hatırlanan üç şey

**"`Downed` = ölü demek, artık ona dokunulmaz."** Tam tersi.
`CanBeAttacked(Downed)` **true** döndürüyor ve bu tasarımın parçası: düşmüş birime
vurmak "işini bitirme" yoludur. Buraya `Downed`'ı kapatan bir satır koymak,
kurtarma penceresini tek taraflı bir bekleme odasına çevirir.

**"Yapının da bir `TryRevive`'ı vardır, adı `TryRepair`."** Hayır — ve karıştırma
tam burada oluyor. `TryRevive` bir **durum geçişidir** (`Downed → Alive`),
`TryRepair` yalnızca bir **sayı değişikliğidir** (can artar, durum aynı kalır).
`Structure.TryRepair`'in ilk satırı zaten `if (!IsStanding) return false;` — yani
o metot yıkık binayı **ayağa kaldırmaz, kaldırmayı reddeder.**

**"Ceset süresi dolunca bir olay tetiklenir."** Tetiklenmez. O `Tick` hiçbir
durum değiştirmez — `Dead` `Dead` olarak kalır, yalnız `IsReadyForCleanup`
bayrağı açılır. Bu yüzden `Battle.UnitStateChanged`'i dinleyen bir kod cesedin
kaldırıldığını **asla öğrenemez**; onu bulan tek yol
`Battle.RemoveReadyForCleanup`'tır. Yapı tarafında ayrım daha da keskin:
`Structure`'ın hiçbir olayı yok, yani enkazı bulan **tek** yol süpürmedir.

---

## Bu tasarımdan kaçmanın yolu — ve neden kaçılmadı

İki kaçış yolu var ve ikisi de aynı yerde çöküyor.

### ① Tek enum

```csharp
✗ enum EntityState { Alive, Downed, Dead }          // yapılar da bunu kullansın
```

Eşleme neredeyse tutuyor:

```
   Standing → Alive ✓      Destroyed → Dead ✓      Downed → ██ KARŞILIĞI YOK ██
```

Ve o karşılıksız değer her yapı `switch`'inde bir dal açmaya **devam ederdi**.
Dal `throw` olsa hiçbir testin geçmediği ölü kod doğar; dal "ayakta"ya düşse
yıkılmış baraka sağlam çizilir. Daha kötüsü: `CanBeAttacked(state)` tek metoda
inerdi ve `!= Dead` kuralı — yani **kurtarma penceresi kuralı** — sessizce
binalara uygulanırdı.

Ödenen bedel görünür: `TargetingRules`'ta iki aşırı yükleme çifti.
**Aşırı yüklemeyi derleyici İSTER ve gözle görülür; ölü dalı kimse istemez ve
görünmez.**

Bu red kalıcı bir yasak değil, bugünkü grafiğe bağlı: yapılar `Downed`'a denk bir
ara duruma kavuşursa — "yıkılan baraka yanar, itfaiye yetişirse ayağa kalkar" —
o gün tek enum doğru cevap olur.

### ② Tek lifecycle

```csharp
✗ class EntityLifecycle   // üç durum, tek sınıf, yapılar Downed'a hiç girmez
```

Bu daha da sinsi, çünkü **bugün çalışır**. Yapı hiç `OnHealthDepleted`'dan
`Downed`'a geçmese sorun görünmez. Kaybedilen şey kod değil, **cevap**:

```
   Tek sınıfta cevaplanamayan sorular
   ─────────────────────────────────────────────────────────────────
   "olay olmalı mı"        ► birimde EVET (Tick'in soranı yok)
                             yapıda HAYIR (her yıkımın soranı var)
                             ◄── tek sınıf ikisine birden cevap veremez
   "SetState gerekli mi"   ► birimde EVET (olay var, tetiklenmeli)
                             yapıda HAYIR (tetiklenecek şey yok)
   "OnHealthDepleted ne
    döndürmeli"            ► birimde void, yapıda bool
   "kaç geri sayım"        ► birimde 2 (10 + 5), yapıda 1 (8)
```

Dördü de aynı yönde ayrışıyor ve tek sınıf her birinde **birimin cevabını
yapıya da dayatırdı** — çünkü birim daha zengin olan taraf. Yapı, ihtiyacı
olmayan bir olayı, ihtiyacı olmayan bir tek-giriş-kapısını ve ihtiyacı olmayan
bir ara durumu miras alırdı.

**Kendi tipini tasarlarken** ölçüt şu: iki tip aynı **değerleri** taşıyabilir ama
aynı **geçişleri** taşımıyorsa ortak tipi hak etmiyorlar. Değerler benziyor
diye birleştirmek, oklardaki farkı görünmez kılar.

Bu arada aynı iki tip `Health`'i **paylaşıyor** ve bu doğru — çünkü `Health`'in
bir geçiş grafiği yok, yalnız bir sayısı var. Sayının "düşmüş" hâli olmaz.

---

## Bunu okuduktan sonra kodda ne göreceksin

`UnitLifecycle.cs`'teki üç kapı (`OnHealthDepleted`, `Tick`, `TryRevive`) artık üç
ayrı yanlış durumu tutan üç ayrı karar olarak okunacak, birbirinin kopyası gibi
görünen üç `if` olarak değil. `StructureLifecycle.cs`'teki eksikler birer boşluk
değil, birer **cevap** olarak duracak. Ve `TargetingRules.cs`'te aynı sorunun iki
farklı biçimde yazılmış olması bir tutarsızlık değil, enum büyüdüğü gün ayrışacak
iki ayrı niyet olarak görünecek.

Kodda karar, burada hikâye. İkisi çelişirse **kod kazanır** — orası çalışan
metin, burası anlatı.
