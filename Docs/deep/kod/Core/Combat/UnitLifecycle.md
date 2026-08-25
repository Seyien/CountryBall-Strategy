# UnitLifecycle

> **Kaynak:** `Assets/Game/Core/Combat/UnitLifecycle.cs`
> **Ad alanı:** `GridStrategy.Combat` · **Assembly:** `GridStrategy.Combat` (`noEngineReferences: true`)
> **Rol:** Varlık (Entity) — kimliği var, hafızası var, yalnızca **karar** verir

Bir birimin üç durumlu yaşam döngüsü ve geri sayımları. Yalnızca karar verir
("artık Dead", "ceset kaldırılmalı"); hiçbir şeyi yok etmez, çizmez, sahneye
dokunmaz.

**ZAMANI KENDİ OKUMAZ.** `Tick` saniyeyi dışarıdan alır; içeride `Time.deltaTime`
yoktur. Sebebi ölçülmüş ve tamamı [↓ Tick](#tickfloat-deltaseconds) başlığında.

**Neyi BİLMEZ:** canın kaç olduğunu (`Health`'in işi), kimin dirilttiğini, sahnede
neyin silineceğini. Yalnızca "hangi durumdayım ve ne kadar kaldı" sorusunu
cevaplar.

| Üye | Karar | Detay |
|---|---|---|
| `remainingSeconds` | tek alan iki sayacı taşır | [↓](#remainingseconds-alan) |
| `StateChanged` | soranı yok, ilgileneni var → event | [↓](#statechanged) |
| `State` | tek yazan kapıdan geçer | [↓](#state) |
| `SetState(UnitState)` | tek giriş noktası; aynı duruma geçiş elenir | [↓](#setstateunitstate-next) |
| `IsReadyForCleanup` | istek bayrakla söylenir, event'le değil | [↓](#isreadyforcleanup) |
| `RemainingSeconds` | durum ile sayıyı birleştirip anlamı verir | [↓](#remainingseconds) |
| `OnHealthDepleted()` | kapı yalnız Alive'dan geçirir | [↓](#onhealthdepleted) |
| `TryRevive()` | istek olduğu için `false` döndürür | [↓](#tryrevive) |
| `Tick(float deltaSeconds)` | zaman dışarıdan gelir | [↓](#tickfloat-deltaseconds) |

**İlgili anlatılar:** [05-yaşam döngüsü](../../../konular/05-yasam-dongusu.md) ·
[01-olay zinciri](../../../konular/01-olay-zinciri.md) ·
[02-assembly duvarı](../../../konular/02-assembly-duvari.md)

> Kodda `StateChanged`, `OnHealthDepleted` ve `Tick` üyelerinin üstündeki
> `DERİN ANLATIM:` yönlendirmeleri yerinde bırakıldı; bu belge onların yerini
> almaz, kararın gerekçesini taşır.

---

## remainingSeconds (alan)

**AYNI ANDA BİR TANESİ İŞLİYORSA, ALAN DA BİR TANEDİR.**

Bulunulan durumun geri sayımı. `Alive`'da anlamsızdır ve kullanılmaz; tek alanla
iki sayaç taşımak, iki alanın senkron kalmasını sağlamaktan basittir — bir anda
yalnızca bir geri sayım işler.

### HARİTA: hangi durumda hangi sayaç işliyor

```
  durum    downedRemaining   corpseRemaining   tek alan (seçilen)
  ────────────────────────────────────────────────────────────────
  Alive    (anlamsız)        (anlamsız)        (anlamsız, 0 döner)
  Downed   İŞLİYOR           boşta             İŞLİYOR
  Dead     boşta             İŞLİYOR           İŞLİYOR
  ────────────────────────────────────────────────────────────────
       ◄── >> HİÇBİR SATIRDA İKİSİ BİRDEN İŞLEMİYOR <<
```

"Hangisi işliyor" bilgisi zaten `State`'te duruyor. İki alan onu İKİNCİ kez saklar
ve iki kaydın senkron kalması bir söz hâline gelir; sözü tutmayı unutan tek satır
şurada olurdu:

```
TryRevive: downedRemaining = 0  ✓ hatırlandı
           corpseRemaining = 0  ✗ unutuldu
  -> dirilen birim yanında ESKİ bir ceset sayacı taşır
  -> saniyeler sonra o sayaç dolar ve kimse sebebini aramaz
```

### KAPSAM: "iki alan kötüdür" DEĞİL

Ayırt edici soru: **iki değer aynı anda ikisi birden ANLAMLI mı?**

Karşı örnek aynı dosyada, üç satır yukarıda:

```csharp
private readonly float downedWindowSeconds;
private readonly float corpseWindowSeconds;
```

Bunlar İKİ ayrı alandır ve öyle kalmalıdır — çünkü ikisi de HER AN geçerlidir;
biri diğerinin yerini almaz, aralarında bir "şu an hangisi" sorusu yoktur ve
`readonly` oldukları için ayrışamazlar da. Yani sayaçlar tek alanda, pencereler
iki alanda: fark alanların sayısında değil, aynı anda kaçının anlamlı olduğunda.

### İŞ BÖLÜMÜ: State ile bu alan örtüşmez, bölüşür

```
State             ► HANGİ geri sayımın işlediğini söyler
remainingSeconds  ► NE KADAR kaldığını söyler
RemainingSeconds  ► ikisini birleştirip anlamı verir
                    (`State == Alive ? 0f : remainingSeconds`)
```

`State` silinirse kalan saniyenin neyin saniyesi olduğu bilinemez. Alan silinirse
geri sayım diye bir şey kalmaz. Property silinirse `Alive`'daki anlamsız değer
dışarı sızar ve UI onu gösterir. Üçü aynı gerçeği üç kez taşımıyor: biri türü,
biri miktarı, biri de ikisinin birleşimini veriyor.

### `private` senkron sorununu çözmez

Okuyucu korumayı gizliliğe yazabilir: "iki alan da private, dışarıdan kimse
bozamaz". Yukarıdaki ayrışma DIŞARIDAN değil İÇERİDEN doğuyor — `TryRevive`'ın
unuttuğu satırdan. `private` iki alanın senkron kalmasını sağlayan hiçbir şey
yapmaz; onu sağlayan tek şey ikinci alanın hiç var olmamasıdır.

### REDDEDILEN

```csharp
private float downedRemaining;
private float corpseRemaining;
```

**KIRILAN:** "hangi sayaç işliyor" bilgisi `State`'in yanında İKİNCİ kez durur.

```
RemainingSeconds hangisini döndüreceğini State'e sorar
TryRevive ikisini sıfırlamayı unutur -> ceset sayacı diriye taşınır
derleyici: hiçbir şey der  ·  test: hata saniyeler sonra görünür
```

**KAZANIRDI:** iki sayaç AYNI ANDA işlemek zorunda olsaydı — ceset süresi düşme
anında başlayıp `Downed` boyunca da aksaydı.

**TEK CUMLE:** Bir anda yalnız bir geri sayım işliyorsa iki alan iki gerçek değil,
senkron tutulacak tek gerçeğin iki kopyasıdır.

---

## StateChanged

**SORAN YOKKEN İLGİLENEN VARSA, ŞEKİL EVENT'TİR.**

Durum her DEĞİŞTİĞİNDE tetiklenir ve yeni durumu taşır. Kurucudaki ilk atama
tetiklemez — o bir geçiş değil, başlangıçtır; ve o anda abone olabilmiş kimse
yoktur.

### NEDEN DÖNÜŞ DEĞERİ DEĞİL — bu dosyanın en pahalı ayrımı

`AttackAction.Execute` zaten "düşürdü" bilgisini **döndürüyor** ve orada event
gereksiz olurdu, çünkü soran zaten oradadır. Ama `Tick` ile olan `Downed → Dead`
geçişini **soran yoktur**: `Tick`'i çeviren taraf oyun döngüsüdür ve o geçişle
ilgilenmez; ilgilenen (ceset efekti, ses, skor) BAŞKA yerdedir.

Tek cümlelik ayrım: **Dönüş değeri — soran zaten orada. Event — ilgilenen başka
yerde.**

### HARİTA: geçiş kime ulaşıyor

```
  SEÇİLEN — event
    SetState ─► StateChanged
       └─► Combatant.OnLifecycleStateChanged
             └─► Combatant.StateChanged  (önceki + yeni)
                   └─► Battle'ın kapanış yönlendiricisi
                         └─► Battle.UnitStateChanged  (kimlikli)
                               └─► BoardAdapter.OnUnitStateChanged
       └─► (ses, skor, başarım ... henüz yazılmamış dinleyiciler)
    ◄── >> TICK'İ HİÇ BİLMEYENE DE ULAŞIR <<

  REDDEDILEN — her kare oku ve karşılaştır
    Tick'i çeviren taraf ─► before/after karşılaştırması
    ◄── >> ZİNCİR BURADA BİTER: GEÇİŞİ YALNIZ O GÖRÜR <<
    UI, ses ve skor geçişi öğrenmek isterse her biri KENDİ "önceki
    durum" kopyasını tutmak zorunda kalır:
        üç kopya -> biri güncellemeyi unutur -> hata sessizdir
```

Ve o unutulan kopya tam olarak `Combatant`'ta bir kez, doğru yerde tutuluyor:
`lastObservedState` (gerekçesi [Combatant.md](Combatant.md#lastobservedstate)'de).

### KAPSAM: "her geçiş event olsun" DEĞİL

Ayırt edici soru: **bu geçişi SORAN biri var mı?**

Karşı örnek aynı dosyada, kırk satır aşağıda: `IsReadyForCleanup` bir event DEĞİL,
bir bayraktır — ve doğrusu odur. Onu okuyan taraf zaten `Tick`'i çeviren taraftır,
yani soran oradadır. Aynı dosyada iki zıt karar var ve ikisini ayıran şey durum
sayısı değil, SORANIN NEREDE OLDUĞU.

### İŞ BÖLÜMÜ: üç bildirim şekli örtüşmez, bölüşür

```
dönüş değeri  ► AttackAction.Execute "düşürdü" bilgisini döndürür;
                soran zaten oradadır, event gereksiz olurdu
event         ► Tick içindeki Downed → Dead; soranı YOK,
                ilgilenen başka yerde
bayrak        ► ceset süresinin dolması; okuyan Tick'i çeviren taraf
```

Dönüş değeri silinirse saldıran, vuruşunun sonucunu öğrenemez. Event silinirse
`Tick`'in doğurduğu geçişi kimse duyamaz. Bayrak silinirse temizlik zamanı hiç
bilinemez. Üçü aynı işi üç kez yapmıyor; her biri farklı bir "soran/ilgilenen"
dizilimini kapatıyor.

### EVENT'İN BEDELİ: güçlü referans ve kapanış kimliği

Bir event, YAYINCIDAN ABONEYE güçlü bir referans tutar; bu tip saf Core'dur ve
abone olan Unity nesnesi yok edildikten sonra bile abonelik çözülmediyse o nesne
toplanamaz. Çözmek de ücretsiz değil: `-=` yalnızca ABONE OLUNAN delege örneğiyle
çalışır — aynı gövdeyi taşıyan ikinci bir lambda EŞİT DEĞİLDİR (kapanış kimliği).
`Battle` bu bedeli birim başına bir sözlükle ödüyor (`stateForwarders`), çünkü
kimliği ekleyen kapanışı sökebilmek için saklamak zorunda. Bedel bilinerek kabul
edildi; bu bölüm onu görünür tutuyor.

### REDDEDILEN

Event hiç doğmaz, çağıran her kare `State`'i okuyup önceki değerle karşılaştırır:

```csharp
var before = lifecycle.State;
lifecycle.Tick(dt);
if (lifecycle.State != before) { /* tepki */ }
```

**KIRILAN:** "önceki durumu hatırlamak" her çağıranın kendi işi olur.

```
UI, ses ve skor üç ayrı kopya tutar -> biri unutur, hata sessizdir
geçişi yalnız Tick'i çeviren görür -> başka dinleyici HİÇ göremez
derleyici: hiçbir şey der  ·  test: hiçbiri kırmızıya dönmez
```

**KARSILASTIRMA:**

| şekil | soran nerede | sonucu |
|---|---|---|
| dönüş değeri | soran zaten orada | bu geçişi soran yok, cevap boşa gider |
| her kare oku | soran belirsiz | hatırlama işi her çağırana dağılır |
| event | ilgilenen başkada | geçiş, Tick'i hiç bilmeyene de ulaşır |

**KAZANIRDI:** tek bir çağıran olsaydı ve o çağıran zaten her kare durumu okuyor
olsaydı — o gün event yalnızca bir dolaylılık katmanı olurdu.

**TEK CUMLE:** Geçişi SORAN varsa dönüş değeri yeter; soran yokken ilgilenen varsa
tek dürüst şekil event'tir.

---

## State

`{ get; private set; }`. Yeni birim `Alive` doğar. Dışarıdan yazılamaz, ama asıl
koruma erişim belirtecinde değil: `State`'e yazan tek satırın `SetState` olmasında.
Gerekçesi [↓ SetState](#setstateunitstate-next) başlığında.

---

## SetState(UnitState next)

Durumu değiştirir ve dinleyenlere haber verir. **Tek giriş noktası olması
kasıtlı:** `State`'e doğrudan yazan bir satır kalsaydı, o geçiş sessizce
kaybolurdu ve hata "bazen event gelmiyor" şeklinde çıkardı.

Gövdenin tamamı şu ve "tek kapı" ilk bloktur:

```csharp
private void SetState(UnitState next)
{
    if (State == next)
    {
        return;                        // ◄── AYNI DURUMA GEÇİŞ: kapı burada
    }

    State = next;
    StateChanged?.Invoke(next);
}
```

Gövdedeki tek kapı, aynı duruma geçişi eler: aynı duruma geçiş bir DEĞİŞİM
değildir ve event tetiklenmez. O satır olmasaydı bir dinleyici aynı geçişi iki kez
duyabilir ve iki kez ses çalabilirdi.

**"Olmasaydı" tam olarak şu düzenlemedir:** `if (State == next) { return; }` bloğu
silinir, geriye alttaki iki satır kalır.

```
SetState(Downed) durum ZATEN Downed iken çağrılsın
   ─► State = next                  (değer değişmez)
   ─► StateChanged?.Invoke(next)    ◄── yine de YAYILIR
   ─► Combatant.OnLifecycleStateChanged onu ikiliye çevirir ve
      previous == next olan bir StateChanged(previous, next) gider
      >> çağıran, OLMAYAN bir geçişi geçiş diye duyar <<
```

Aynı desenin **gerekmediği** yer kardeş tiptir: `StructureLifecycle`'da event
olmadığı için kaybolacak bir yayın da yoktur ve orada `SetState` yalnız bir
yönlendirme katmanı olurdu — gerekçesi
[StructureLifecycle.md](StructureLifecycle.md#state)'de.

---

## IsReadyForCleanup

**İSTEK BAYRAKLA SÖYLENİR, ÇÜNKÜ BAYRAK KİMSEYİ TUTMAZ.**

Ceset süresi dolduğunda `true` olur. Bu bir İSTEKtir, bir eylem değil: sahneden
silme işini Unity katmanı yapar. Burada `true` olması, orada silindiği anlamına
gelmez — bu ayrım bilinçlidir, çünkü "karar" ile "uygulama" farklı sahiplerdir.

### HARİTA: yok etme çağrısı hangi yığın çerçevesinde

```
  REDDEDILEN — event
    oyun döngüsü: foreach (var c in ...) c.Tick(dt);
      ① Tick içinde remainingSeconds <= 0
      ② OnReadyForCleanup?.Invoke()   ◄── >> TICK'İN ORTASI <<
      ③ abone HEMEN yok etmeye başlar
      ④ dönmekte olan foreach yok edilmiş birime dokunur
    ② ile ④ AYNI yığın çerçevesinde: ikisi arasında hiçbir güvenli
    kesme noktası yok. Ayrıca abonelik çözülmezse saf Core nesnesi
    ölü Unity nesnesini hayatta tutar (yayıncı → abone güçlü referans).

  SEÇİLEN — bayrak
    ① Tick içinde IsReadyForCleanup = true   (kimse çağrılmıyor)
    ② döngü BİTER
    ③ çağıran AYRI bir geçişte bayrağı okur ve temizler
    ◄── >> YOK ETME, TICK'TEN SONRAKİ ADIMDA <<
    bayrak hiçbir referans tutmaz: sızdıracak bir abonelik yok.
```

### KAPSAM: "Tick içinde event tetikleme" DEĞİL

Karşı örnek aynı dosyada, kırk satır yukarıda: `StateChanged` DA `Tick`'in içinden
tetikleniyor (`Downed → Dead` geçişi `Tick`'te doğar) ve bu sorun değil. Fark
tetiklenme ANINDA değil, dinleyicinin YAPTIĞI İŞTE: `StateChanged`'in dinleyicileri
görünüşü değiştirir — renk, ses, skor. Bir temizlik event'inin dinleyicisi VARLIĞI
ortadan kaldırır.

Ayırt edici soru: **bu bildirimi duyan taraf, döngünün hâlâ üzerinde yürüdüğü
nesneyi yok edecek mi?**

### İŞ BÖLÜMÜ: event ile bayrak örtüşmez, bölüşür

```
StateChanged (event)  ► DURUM geçişi; soranı yok, ilgilenen başka
                        yerde, dinleyici görünüşü değiştirir
IsReadyForCleanup     ► İSTEK; okuyan Tick'i çeviren taraf,
                        yapacağı iş YIKICI
```

Event silinirse geçiş kimseye ulaşmaz. Bayrak silinirse temizlik zamanı ya hiç
bilinmez ya da yukarıdaki tehlikeli event geri gelir. İkisi birbirinin yedeği
değil; farklı cinsten iki bildirimi taşıyorlar ve bu karar üstteki event kararının
TERS yönüdür.

### "KARAR" ile "UYGULAMA" ayrımı nerede biter

Bayrağın `true` olması sahnede bir şeyin silindiği anlamına gelmez; silme işini
Unity katmanı yapar ve bu tip onu hiç görmez. Sözleşme orada biter: bayrak
okunmazsa hiçbir şey olmaz ve bu tip bunu öğrenemez. Karar burada, uygulama başka
sahipte — bilerek.

### REDDEDILEN

```csharp
public event Action OnReadyForCleanup;
```

**KIRILAN:** olay `Tick`'in ORTASINDA tetiklenir ve abone hemen yok etmeye başlar.

```
dönmekte olan güncelleme döngüsü yok edilmiş birime dokunur
abonelik çözülmezse saf Core nesnesi ölü Unity nesnesini tutar
derleyici: hiçbir şey der  ·  test: Core testleri yeşil kalır
```

**KAZANIRDI:** temizliği isteyen taraf ile `Tick`'i çeviren taraf farklı olsaydı —
bayrağı kimse okumazdı ve haber vermek şart olurdu.

**TEK CUMLE:** Üstteki event kararının TERS yönü: burada okuyan zaten `Tick`'i
çeviren taraf, o yüzden bayrak yeter ve bayrak kimseyi tutmaz.

---

## RemainingSeconds

Kalan geri sayım. UI bu sayıyı gösterecek ("5 saniye sonra kaldırılacak").
`UnitState.Alive` iken anlamsızdır ve `0` döner.

Bu property, `State` ile `remainingSeconds`'ı birleştirip anlamı veren üçüncü
parçadır; üçünün iş bölümü [↑ remainingSeconds](#remainingseconds-alan)
başlığında.

---

## OnHealthDepleted()

**KURTARMA PENCERESİNİ ATLAYAN KESTİRME, DURUMU DA SİLER.**

Canı tükendiğinde çağrılır: ayakta olan birim düşer. Bilerek yalnızca `Alive`'dan
çalışır. `Downed` bir birime tekrar vurmak onu ANINDA öldürmemeli — "işini
bitirme" ayrı bir kural (düşme canı) ve o kural henüz yazılmadı. Buraya sessizce
koymak, tasarımdaki iki ayrı `Downed → Dead` yolunu bire indirirdi.

### HARİTA: izin verilen ve YASAKLANAN geçişler

```
  İZİN VERİLEN                     YASAKLANAN (bu karar)
  ─────────────────────────────    ─────────────────────────
  Alive ──can bitti──► Downed      Downed ──can bitti──► Dead
  Downed ──10 sn dolar──► Dead              ◄── >> KESTİRME <<
  Downed ──TryRevive──► Alive

  Bu metodun kapısı — tek `if` — üç durumu şöyle ayırır:
    Alive   ► geçer, düşer                              ✓
    Downed  ► sessizce döner  ◄── >> PENCERE KORUNDU <<
    Dead    ► sessizce döner

  Tasarımda Downed → Dead'in İKİ yolu var ve yalnız biri yazıldı:
    ① geri sayımın dolması            ► Tick'te, yazılı
    ② "işini bitirme"                 ► kendi kuralı (düşme canı)
                                        HENÜZ YAZILMADI
```

Reddedilen satır ②'yi ①'in yerine sessizce koyar ve o kuralın yazılacağı yeri de
ortadan kaldırırdı.

### KAPSAM: "erken çıkış" değil, KAPININ ŞEKLİ

Karşı örnek aynı dosyada, hemen aşağıda: `TryRevive` de bir durum kapısı taşıyor —

```csharp
if (State != UnitState.Downed) { return false; }
```

— ama o `false` DÖNDÜRÜYOR, bu ise sessizce dönüyor. Fark keyfî değil: `TryRevive`
bir İSTEKtir ve isteği yapanın cevabı bilmesi gerekir; `OnHealthDepleted` bir
BİLDİRİMdir ve bildirimi yapanın (`Combatant.TakeDamage`) sorduğu bir soru yoktur.
Kapının varlığı ortak, şekli soranın olup olmamasına bağlı.

### İŞ BÖLÜMÜ: bu dosyadaki üç kapı örtüşmez, bölüşür

```
OnHealthDepleted `!= Alive`  ► bitirme kestirmesini kapatır
Tick `== Alive` erken çıkış  ► Alive'da anlamsız alanı
                               eksiltmeyi önler (DOĞRULUK,
                               performans değil)
TryRevive `!= Downed`        ► kalıcı ölünün dirilmesini önler
```

Birincisi silinirse alan hasarı istemeden "bitirme" olur. İkincisi silinirse
ayakta duran birimin sayacı eksiye gider ve `Downed`'a girdiği an yanlış süreyle
başlar. Üçüncüsü silinirse ceset dirilir. Üçü de "yanlış durumdan gelme"yi
engelliyor ama üç FARKLI yanlış durumu.

### `private set` hiçbirini sağlamaz

`State { get; private set; }` yalnızca dışarıdan yazmayı keser; yukarıdaki üç
kapının hiçbirini kurmaz. Reddedilen satır tamamen bu sınıfın İÇİNDE yazılırdı ve
erişim belirteci ona hiçbir şey demezdi. Geçiş tablosunu ayakta tutan şey belirteç
değil, bu üç `if` ve gerekçeleri.

### REDDEDILEN

```csharp
if (State == UnitState.Downed)
{
    State = UnitState.Dead;
    remainingSeconds = corpseWindowSeconds;
    return;
}
```

**KIRILAN:** düşmüş birime değen tek bir sıyırık kurtarma penceresini kapatır.

```
alan hasarı olan her saldırı istemeden "bitirme" hâline gelir
düşme canı kuralının yazılacağı yer kalmaz
derleyici: hiçbir şey der  ·  test: Downed_HealthDepletedAgain_
DoesNotSkipTheWindow kırmızı olur
```

**KAZANIRDI:** tasarım "yerdekine vuran bitirir" derse ve bitirmenin ayrı bir
maliyeti, menzili ya da süresi olmayacaksa.

**TEK CUMLE:** `Downed`'ın var olma sebebi bir PENCEREdir; o pencereyi atlayan
kısayol, durumun kendisini de gereksizleştirir.

---

## TryRevive()

Düşmüş birimi ayağa kaldırır. Yalnızca `UnitState.Downed` iken başarılı olur;
kalıcı ölü diriltilemez. `true`/`false` döndürmesi bir şekil kararıdır: bu bir
İSTEKtir ve isteği yapanın cevabı bilmesi gerekir — sessizce dönen
`OnHealthDepleted` ile arasındaki fark [↑ OnHealthDepleted](#onhealthdepleted)
başlığında.

Başarılı geçişte sayaç sıfırlanır. Tek alan tutulduğu için burada sıfırlanacak
ikinci bir sayaç yok; iki alan olsaydı unutulacak satır tam olarak burasıydı —
[↑ remainingSeconds](#remainingseconds-alan).

---

## Tick(float deltaSeconds)

**ZAMANI DIŞARIDAN ALMAK, SESSİZ BİR YANLIŞI İMKÂNSIZ KILAR.**

Zamanı ilerletir. Saniye DIŞARIDAN gelir — bu tipin Unity'ye bağlanmamasının ve
EditMode'da sınanabilmesinin tek sebebi budur.

### HARİTA: `Time.deltaTime` nerede ne döndürür

```
  çalıştığı yer        Time.deltaTime          sonuç
  ──────────────────────────────────────────────────────────────
  PlayMode / oyun      gerçek kare süresi      doğru
  EditMode testi       0,017675  ◄── >> SIFIR DEĞİL <<  SESSİZ YANLIŞ
  ──────────────────────────────────────────────────────────────
  Sıfır DÖNSEYDİ test sonsuza kadar ilerlemez ve hata GÖRÜNÜRDÜ.
  Sıfır dönmediği için test yeşil kalır ve hiçbir şey ölçmez:
    10 saniyelik pencereyi doldurmak için ≈566 Tick çağrısı gerekir
    (10 / 0,017675) ve "Tick(10.1f) verince öldü" diyen test HİÇ
    YAZILAMAZ  ◄── >> KAYBEDİLEN ŞEY BU <<

  SEÇİLEN — saniye parametreden gelir
    test:  lifecycle.Tick(10.1f)  -> tek çağrı, tek satır, kesin
    oyun:  lifecycle.Tick(Time.deltaTime)  -> çağıran katmanın işi
```

### KAPSAM: "her sayıyı dışarıdan al" DEĞİL

Ayırt edici soru: **bu değer ORTAMDAN mı okunuyor ve ortam değişince sessizce
farklı mı geliyor?**

Karşı örnek aynı dosyada, en üstte:

```csharp
public const float DefaultDownedWindowSeconds = 10f;
public const float DefaultCorpseWindowSeconds = 5f;
```

Bu iki sayı dosyanın İÇİNDE sabit yazılı ve doğrusu da bu — ortamdan okunmuyorlar,
testte de oyunda da aynı gelirler, üstelik kurucudan değiştirilebiliyorlar. Yani
zorunlu olan şey "parametreleştirmek" değil, ortama göre sessizce değişen girdiyi
dışarı çıkarmak.

### İŞ BÖLÜMÜ: imza ile asmdef örtüşmez, bölüşür

```
`Tick(float deltaSeconds)`     ► TASARIMI kapatır: 10 saniyelik
                                 kural tek çağrıda sınanabilir ve
                                 niyet imzada okunur
GridStrategy.Combat.asmdef     ► ERİŞİMİ kapatır:
`"noEngineReferences": true`     UnityEngine bu assembly'ye hiç
                                 gelmiyor
```

İmza silinip zaman içeriden okunmak istenseydi bugün asmdef duvarı devreye
girerdi; ama duvar bir JSON satırıdır ve birinin referans eklemesi onu kaldırmaya
yeter — o gün geriye tek koruma olarak imza kalır ve aşağıdaki KIRILAN zinciri
harfi harfine geçerli olur. Tersten: asmdef dursa bile imza kalkarsa kural tek
çağrıyla hiç sınanamaz. Biri erişimi, diğeri tasarımı tutuyor. Duvarın kendi
hikâyesi [02-assembly duvarı](../../../konular/02-assembly-duvari.md)'nda.

### GARANTİ NEREDE BİTER

Bu tip zamanı okumaz ama zamanın DOĞRU geçirildiğini de denetleyemez: negatif
değer için bir `throw` var, geri kalan her şey çağıranın sözü. Kare atlayan, iki
kez `Tick` çeviren ya da sabit `1f` geçen bir çağıran hiçbir kural çiğnemez.
Sözleşme "geriye gitme" duvarında biter.

### Gövdedeki erken çıkış

`Alive`'da geri sayım yok; oradaki erken çıkış PERFORMANS için değil, DOĞRULUK
için: sonraki çıkarma `Alive`'da anlamsız bir alanı eksiltirdi. Aynı dosyadaki üç
kapının iş bölümü [↑ OnHealthDepleted](#onhealthdepleted) başlığında.

### REDDEDILEN

```csharp
public void Tick()
{
    float deltaSeconds = Time.deltaTime;
```

**KIRILAN:** ölçüldü — EditMode'da `Time.deltaTime` sıfır DEĞİL, `0,017675` döner.

```
test patlamaz, sessizce 0,017675'lik adımlarla ilerler
"10.1f verince öldü" diyen test hiç yazılamaz
dosya UnityEngine'e bağlanır -> testler PlayMode'a düşer
derleyici: hiçbir şey der  ·  test: yeşil kalır, hiçbir şey ölçmez
```

**KAZANIRDI:** tip bir `MonoBehaviour` olsaydı ve zamanın tek bir kaynağı
bulunsaydı — herkes aynı sayıyı geçiyorsa parametre seçim sunmaz.

**TEK CUMLE:** Zamanı DIŞARIDAN almak bir test kolaylığı değil, testin hiç
yazılamayacağı bir sessizliği imkânsız kılmaktır.
