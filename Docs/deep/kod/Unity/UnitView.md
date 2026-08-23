# UnitView

> **Kaynak:** `Assets/Game/Unity/UnitView.cs`
> **Ad alanı:** `GridStrategy.Unity` · **Assembly:** `GridStrategy.Unity` (MonoBehaviour, motora bağımlı)
> **Rol:** Görünüm (View) — sahne kimliği var, oyun hafızası yok, karar vermez uygular
> **Unity ölçüsü:** motor yüzeyi `SpriteRenderer` ile sınırlı — `Input`, `Camera` ve `Time` bu dosyada hiç geçmez, bu yüzden `UnitViewTests` onu EditMode'da çıplak bir `GameObject` üstünde sürebiliyor; `BoardAdapter`'ın böyle bir testi yok

Bir birimin **ekrandaki karşılığı**. Tahtanın kurallarını bilmez, nerede
durduğunu bilmez, `GridStrategy.Core.Unit` tipini hiç görmez. Yalnızca kendi
görsel durumunu uygular — bugün: seçim çerçevesi ve yaşam durumu.

Var olma sebebi: adaptör "şu birimi seçili göster" demek istediğinde, çerçevenin
bir **çocuk nesnede** yaşadığını bilmek zorunda kalıyordu. O bilgi burada kapalı
kalır; adaptör yalnızca komutu verir.

Bu tip **savaşın sözlüğünü konuşuyor** (`UnitState`) ve bu bir taviz değil,
bilerek ödenmiş bir bedeldir; hangi olayın kararı çevirdiği
[`SetState`](#setstateunitstate-state) bölümünde yazılı. Ödenen bedelin adı da
orada: `GridStrategy.Unity.EditModeTests` artık `GridStrategy.Combat`'a referans
vermek zorunda.

| Üye | Karar | Detay |
|---|---|---|
| `selectionOverlay` | prefab'da duran çocuk çizici; seçim rengi burada alan DEĞİL | [↓](#selectionoverlay) |
| `downedTint` | renk alanı, ama ikinci kaynak değil — ve mutlak değil ÇARPAN | [↓](#downedtint) |
| `deadTint` | ikinci çarpan, ölçülmüş sınırıyla birlikte | [↓](#deadtint) |
| `body` · `Body` | tembel çözülür; yazılı renk aynı satırda yakalanır | [↓](#body) |
| `authoredColor` | oyun durumu değil, TÜREV bir değerin önbelleği | [↓](#authoredcolor) |
| `Awake()` | sıra bir karardır: normalizasyon erken çıkışın ÜSTÜNDE | [↓](#awake) |
| `SetSelected(bool)` | seçim ile durum iki BAĞIMSIZ eksendir | [↓](#setselectedbool-isselected) |
| `SetState(UnitState)` | üç durum, iki eksen: yatıklık × çarpan | [↓](#setstateunitstate-state) |
| `TintFor(UnitState)` | bilinmeyen durumda birimi GÖRÜNÜR bırakır | [↓](#tintforunitstate-state) |

**İlgili anlatılar:** [01-olay zinciri](../../konular/01-olay-zinciri.md) ·
[05-yaşam döngüsü](../../konular/05-yasam-dongusu.md)

---

## ROL

### İKİ ÜYE, İKİ FARKLI GARANTİ KAYNAĞI

Bu ayrım dosyanın en öğretici satırıdır ve tek bir ağaçta duruyor:

```
Unit (prefab kökü)
├── SpriteRenderer      ◄── body : RequireComponent GARANTİ EDER;
│                           GetComponent boş dönemez, null kontrolü YOK
├── UnitView (bu bileşen)
└── SelectionOverlay (ÇOCUK GameObject)
    └── SpriteRenderer  ◄── selectionOverlay : hiçbir attribute garanti ETMEZ,
                            elle sürüklenir ve boş bırakılabilir ── null BURADAN
```

Sınıfın üstündeki `[RequireComponent(typeof(SpriteRenderer))]` yalnız **kendi
GameObject'ine** bakar; çerçeve bir çocukta yaşadığı için onu kapsamaz.
`[SerializeField]` de bir garanti değildir: alanı Inspector'da **görünür** yapar,
**dolu** olmasını sağlamaz. Aynı dosyada iki farklı savunma, çünkü iki farklı
risk.

### "hafıza: yok" satırının tek istisnası

Ne "seçili miyim" ne "hangi durumdayım" burada saklanır; ikisinin de tek
doğruluk kaynağı dışarıdadır (seçim `BoardAdapter.selectedUnit`, durum
`Combatant.State`) ve çiziciler yalnızca onların yansımasıdır.

Tek istisna ve adı konmuş: `authoredColor`. O bir **oyun durumu değil**,
prefab'da **yazılı** bir değerin önbelleğidir — `body` referansının
önbelleklenmesiyle aynı sınıftan bir şey, aynı satırda ve aynı anda alınır.
Gerekçesi [`Body`](#body) bölümünde.

---

## selectionOverlay

Prefab'da hazır duran seçim çerçevesinin çizicisi.

### Neden runtime'da `Instantiate` edilmiyor

Her seçimde nesne doğurup yok etmek çöp (garbage) üretir ve kurulumu koda
gömerdi. Prefab'da duran bir çocuk ise Editor'de görülebilir, sprite'ı ve rengi
elle ayarlanabilir; kod yalnızca açıp kapatır.

### Tip neden `GameObject` değil `SpriteRenderer`

Bu tek referanstan hem "çizilsin mi" (`enabled`) hem "hangi renk" (`color`)
doğrudan okunur. `GameObject` tutsaydık her erişimde `GetComponent` gerekirdi.

### Seçim RENGİ burada bir alan DEĞİL

Renk zaten bu `SpriteRenderer`'ın kendi `color` alanında yaşıyor ve prefab'da
yazılıyor. Ayrıca bir `[SerializeField] Color` tutsaydık aynı bilginin **iki**
kaynağı olurdu (`DECISION_TOOLKIT` sorusu 1) ve koddaki değer, Editor'de
ayarlanan rengi her `Awake`'te sessizce ezerdi.

---

## downedTint

### Aşağıdaki iki renk alanı, yukarıdaki "renk alanı tutma" gerekçesiyle ÇELİŞMİYOR

Fark tek cümlede duruyor: `selectionOverlay`'de reddedilen şey **başka bir
nesnenin** kendi `color` alanında **zaten yazılı** olan bilginin ikinci
kopyasıydı. Burada ikinci kopya yok: "düşmüş bir birim nasıl görünür" sorusunun
projede başka hiçbir sahibi yok.

Bir alanın kusuru "renk olması" değil, **"aynı bilginin ikinci kaynağı olması"**dır.

### ÇARPAN, MUTLAK DEĞER DEĞİL

Değer gövdenin **yazılı** rengiyle çarpılır. Böylece `Alive` için nötr çarpan
(`Color.white`) tek bir kod yolunu korur ve gövde bir gün takım rengi
taşıdığında düşme onu **silmez, karartır**. Mutlak yazsaydık üç durumdan ikisi
takım rengini yok ederdi.

---

## deadTint

### ÖLÇÜLMÜŞ SINIR — ve süslemeden

Çarpma **karartır**, doygunluğu düşürmez. Gövde bugün beyaz olduğu için
(`BoardAdapter` birimin kendi `color`'ına hiç dokunmuyor) beyaz × 0,35 gerçekten
**gri** verir ve üç durum ekranda ayrışır.

Gövde bir gün takım rengi taşıdığı gün bu çarpan "koyu kırmızı" üretir, gri
değil — o gün ölü görseli ya bir materyal/shader ya da ayrı bir sprite ister ve
bu alan yetmez.

---

## Body

Bu başlık hem `private SpriteRenderer body` alanını hem onu tembel çözen `Body`
property'sini kapsıyor: ikisi tek bir karardır.

### `body` neden `[SerializeField]` DEĞİL

`selectionOverlay`'in aksine bu çizici bir çocuk nesnede yaşamıyor; tam olarak
**bu** GameObject'in üstünde ve `GetComponent` onu her zaman bulur.
Serileştirilseydi Inspector'da boş bırakılabilen ikinci bir kurulum adımı
doğardı — hem de gereksiz yere.

### NEDEN AWAKE'TE DEĞİL — bu ÖLÇÜLMÜŞ bir sebep, üslup değil

`Awake` EditMode'da **hiç** çalışmaz (bu script `[ExecuteAlways]` değil).
Referans `Awake`'te kurulsaydı `new GameObject().AddComponent<UnitView>()` ile
kurulan bir testte `body` sonsuza dek null kalır, `SetState` sessizce hiçbir şey
yapar ve üye sınanamaz olurdu — ekranda görünmeyen bir hata, kırmızıya dönmeyen
bir test.

Tembel çözüm bu tipi sahne kurmadan, Play'e basmadan sınanabilir kılıyor; bedeli
ilk çağrıdaki tek bir `GetComponent`.

### `body == null` Unity'nin AŞIRI YÜKLENMİŞ eşitliğidir

Yok edilmiş bir nesne C# tarafında hâlâ referans taşır ama bu karşılaştırma
`true` döner. Yani nesne yok edilip yeniden kurulursa önbellek kendini tazeler;
`ReferenceEquals(body, null)` yazsaydık ölü referansı canlıymış gibi
kullanırdık.

### YAZILI RENK NEDEN TAM BURADA YAKALANIYOR

`SetState`'in reddettiği eski seçenek, renk soldurmanın "kopyayı almayı unutan
bir yol" ürettiğini söylüyordu (bloğun tamamı
[`SetState`](#setstateunitstate-state) bölümünde). O yolu kapatan şey, kopyanın
ayrı bir adımda **değil**, çizicinin çözüldüğü **tek satırda** alınmasıdır:
gövdeye erişmenin tek kapısı bu property olduğu için "rengi yakalamadan renge
yazmak" diye bir sıralama **yoktur**.

Yok edilip yeniden kurulan bir nesnede de doğru davranır — önbellek tazelenirken
**yeni** nesnenin yazılı rengi yakalanır, eskisi değil.

---

## authoredColor

Gövdenin **prefab'da yazılı** rengi. Bir oyun durumu değil, yazılı bir değerin
önbelleği. Neden "unutulamaz" olduğu [`Body`](#body) bölümünde; neden
"hafıza: yok" satırını bozmadığı ise
[`SetSelected`](#setselectedbool-isselected)'ın KAPSAM bölümünde.

---

## Awake()

### SIRA BİR KARARDIR

Bu başlığın konusu iki satırın **sırası**, o yüzden gövde burada duruyor:

```csharp
private void Awake()
{
    SetState(UnitState.Alive);          // ◄── ① NORMALİZASYON — kontrolün ÜSTÜNDE

    if (selectionOverlay == null)
    {
        Debug.LogError("[UnitView] selectionOverlay is not assigned. ...", this);
        return;                         // ◄── ② ERKEN ÇIKIŞ — altındaki her şey atlanır
    }

    SetSelected(false);
}
```

Durum normalizasyonu ①, `selectionOverlay` kontrolünün **üstünde** duruyor.
Kastedilen düzenleme ①'in `if` bloğunun **altına** — `SetSelected(false)` ile aynı
bölgeye — taşınması. O gün çerçeve referansı atanmamış bir prefab'da ② artık ①'i
de atlardı ve prefab'da ters ya da soluk kaydedilmiş bir gövde öyle kalırdı —
atanmamış **bir** alan, ilgisiz **iki** şeyi birden bozardı.

### Neden normalizasyon gerekiyor

Prefab'da gövde ters ya da soluk bırakılmış olabilir; Editor'de "ölü hâli nasıl
duruyor" diye bakıp öyle kaydetmek çok kolaydır. Doğan her birim **ayakta**
başlamak zorunda. Bu satır, yazılı durumu (authored state) çalışma zamanı
değişmezine çevirir — `SetSelected(false)` ile birebir aynı işi, birebir aynı
gerekçeyle yapar: prefab'da çerçeve **açık** bırakılmış olabilir ve doğan her
birim seçimsiz başlamak zorunda.

### Neden `LogError`

Eksik atama **sessiz** kalmasın: referans boşsa seçim hiç çalışmaz ve ekranda
hiçbir hata görünmez. Bir kez, doğuşta, gürültüyle söyle. Bu kontrolün
`SetSelected`'daki sessiz `return` ile nasıl bölüştüğü
[aşağıda](#setselectedbool-isselected).

---

## SetSelected(bool isSelected)

### SEÇİM İLE DURUM İKİ BAĞIMSIZ EKSENDİR

Ve bu bir karardır: bu metot birimin ölü olup olmadığına **bakmaz**. "Ölü birim
seçilemez" bir **görsel** kural değil bir **oyun** kuralıdır ve bu tip oyun
kuralı uygulamaz.

### HARİTA: iki eksenin doğruluk kaynağı BAŞKA nesnelerde

Ok "kim yazdırıyor" demektir; kutudaki ad, o bilginin **tek** kopyasının yaşadığı
yerdir. `UnitView` hiçbirinin kopyasını almaz.

```
SEÇİLEN — bugünkü şekil
┌─BoardAdapter──────┐          ┌─Combatant─────────┐
│ selectedUnit      │          │ State             │
└─────────┬─────────┘          └─────────┬─────────┘
   SetSelected(bool)            UnitStateChanged olayı
          │                       -> ApplyStateVisual
          ▼                               ▼
╔═════════════════════ UnitView ══════════════════════╗
║  selectionOverlay.enabled  │  Body.flipY / .color   ║
║      (yalnız YAZILIR)      │    (yalnız YAZILIR)    ║
╚════════════════════════════╪════════════════════════╝
                             ◄── İKİ EKSEN BURADA KESİŞMEZ

REDDEDILEN — SetSelected durumu SORSAYDI
┌─Combatant─────────┐   ┌─UnitView────────────────────┐
│ State             │──►│ lastState  ◄── İKİNCİ KOPYA │
└───────────────────┘   │      │                      │
  tazelenmesini hiçbir  │      ▼ OKUNUR               │
  tip ÜSTLENMİYOR       │ SetSelected ──► bazen hiç   │
                        │                  çalışmaz   │
                        └─────────────────────────────┘
```

Ok artık `UnitView`'ın **içindeki** bir düğüme uğruyor. Kırılma o düğümün yanlış
olmasında değil **eskimesinde**: onu `Combatant` ile aynı tutmayı hiçbir tip
üstlenmiyor.

**Figürdeki `UnitView` kutusu, kaynakta:** `Assets/Game/Unity/UnitView.cs` → `SetSelected(bool)`

```csharp
selectionOverlay.enabled = isSelected;
```

Kutunun sol hücresi — «selectionOverlay.enabled (yalnız YAZILIR)» — bu tek satır,
ve «yalnız YAZILIR» ölçülebilir bir iddiadır: `Assets/` altında `.enabled` bu alan
üstünde **bir kez** geçiyor ve o da burası, atamanın sol tarafı. `selectionOverlay`
geçen kalan satırlar bildirim ve iki `null` kontrolü; hiçbiri `.enabled`'ı
SORMUYOR. Yani seçim durumunun doğruluk kaynağı `BoardAdapter.selectedUnit`'te
kalıyor, burada bir ikinci kopya doğmuyor — figürün SEÇİLEN sütununun tamamı bu.

Kutunun sağ hücresi («Body.flipY / .color») aynı dosyanın `SetState` metodunda ve
aynı şekilde yalnız yazılıyor: `bodyRenderer.flipY = state != UnitState.Alive;`.
**İki eksenin KESİŞMEMESİ** de tam bu iki satırın karşılaştırılmasından okunuyor:
buradaki satır `state`'i sormuyor, oradaki satır `isSelected`'ı sormuyor, ve
ikisinin paylaştığı tek bir alan yok. REDDEDİLEN sütundaki `lastState` düğümünün
kaynakta karşılığı **YOK** — o alan hiç yazılmadı; figür onu gözlenmiş bir kusur <!-- YOK-MUAF · DÜŞÜLDÜ · gerekçe aşağıdaki senette -->
olarak değil, bu iki satırın birbirine bakmaya başladığı gün doğacak olan şey
olarak gösteriyor.

> ██ YOKLUK SENEDİ — DÜŞÜLDÜ ██ — `lastState` ikinci kopyası
>
> **GEREKÇE:** Reddedilen düğüm, dışarıda sahibi olan bir bilginin bu dosyada
> tutulan kopyasıdır. Bir alttaki başlık bunu açıkça yasaklıyor: bu tip alan
> tutabilir, tutamayacağı şey ikinci KAYNAK olmaktır. ① dolmuyor, çünkü hiçbir
> oyun özelliği bayatlayabilen bir kopya istemez.
>
> **EN GÜÇLÜ ADAY DA YETMİYOR.** Seçim ile yaşam durumunun kesişmesi gerçek bir
> özellik olabilir. Örneğin seçili birimin çerçevesi, birim düştüğü anda renk
> değiştirsin. O özellik bile bir alanı gerektirmiyor: iki satırın aynı çağrıda
> okunmasını gerektiriyor, ve bilginin sahibi yine `BoardAdapter` ile
> `Combatant` tarafında kalıyor.
>
> **④ HAYIR.** Burada uydurulacak her özellik, tam olarak bu dosyanın kendi
> kuralını çiğnetmek için uydurulmuş olurdu.

### KAPSAM: yasak "alan tutmak" değil, "İKİNCİ KAYNAK olmak"

Kural özeldir: bu tip alan tutabilir; tutamayacağı şey **dışarıda sahibi olan**
bir bilginin kopyasıdır. İki kümeyi ayır:

```
dışarıda sahibi VAR (yasak)  : seçim, yaşam durumu, konum, taraf
yalnız burada doğan (serbest): body, authoredColor, downedTint
```

**KARŞI ÖRNEK** aynı dosyada, `authoredColor` alanı: saklanan bir **alandır** ve
yine de kuralı çiğnemez — prefab'da yazılı bir değerin önbelleğidir ve onu
değiştirebilecek ikinci bir sahip yoktur. Aynısı `body` için de doğru.

Yeni bir alan eklerken sorulacak tek soru: **bu bilginin bu dosyanın dışında bir
sahibi var mı?** Varsa alan değil parametre olmalı.

### İŞ BÖLÜMÜ: İKİ null savunması ÖRTÜŞMEZ, BÖLÜŞÜR

`selectionOverlay`'in atanmamış olma riskini iki ayrı yer karşılıyor:

```
Awake'teki LogError    ► TEŞHİS — bir kez, doğuşta, kırmızıyla
buradaki sessiz return ► HAYATTA KALMA — tık başına, gürültüsüz
```

`Awake`'teki silinirse hata **tamamen** görünmez olur: buradaki `return` hiçbir
şey söylemez, seçim de hiç çalışmaz. Buradaki silinirse her tıklama bir
`NullReferenceException` atar ve Console'u asıl mesajı gömecek kadar doldurur.
Biri sebebi, öteki sonucu kapatıyor.

### HANGİ ATTRIBUTE KORUMUYOR

`[RequireComponent(typeof(SpriteRenderer))]` bu alanı **kapsamaz**; yalnız kendi
GameObject'ine bakar ve çerçeve bir **çocukta** yaşar. Ağacın kendisi
[ROL künyesi](#rol) bölümünde çizili. `[SerializeField]` de bir
garanti değil: alanı Inspector'da **görünür** yapar, **dolu** olmasını sağlamaz.

### GARANTİ NEREDE BİTİYOR

"Bu tip durum saklamaz" sözünü **derleyici** tutmuyor: yarın buraya bir
`private bool isSelected;` eklemek serbesttir ve hiçbir şey kırmızıya dönmez.
Sözü ayakta tutan tek şey üyelerin parametre alıp hiçbir şey **döndürmemesi** —
okunacak bir durum olmadığı için saklamanın alıcısı da yok.

### REDDEDILEN

Görünüm seçilebilirliği kendisi kısıtlar:

```csharp
public void SetSelected(bool isSelected)
{
    if (lastState == UnitState.Dead) { return; }   // sessiz ret
    ...
}
```

**KIRILAN:** görünüm, tutmamaya söz verdiği durumu tutmak **zorunda** kalır.

```
lastState alanı doğar     -> "hafıza: yok" satırı düşer
SetSelected bazen çalışır -> çağıran neden'i göremez, dönüşü yok
adaptör "seçtim" der      -> ekran seçmez, ikisi sessizce ayrışır
derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
```

**KAZANIRDI:** kısıt gerçekten bir **çizim** kuralı olsaydı — çerçeve sprite'ı
yalnızca ayakta duran gövdeye oturuyorsa ve ölü gövdede ekranda kayıyorsa. O gün
sahibi burasıydı.

**TEK CUMLE:** Görünüm bir kuralı uygulayabilmek için o kuralın girdisini
saklamak zorunda kalıyorsa, kural görünümün değildir.

### BUGÜNKÜ SAHİP ve EŞİK

Kısıtın bugünkü sahibi **eylem katmanı**: ölü birim seçilebilir ama seçildikten
sonra yapabileceği her şey `AttackRules` ile `MovementRules`'a takılır —
`AttackOutcome.RejectedActorCannotAct` ve `MoveOutcome.RejectedActorCannotAct`.

**EŞİK:** arayüz tıklamadan **önce** "bunu seçemezsin" demek zorunda kaldığı gün
(imleç değişimi, soluk çerçeve) kısıt görsel hâle gelir ve bu metot durumu
**saklayarak** değil, **parametre olarak alarak** sormaya başlar.

### Gövdedeki iki karar

**`SetActive(false)` DEĞİL:** GameObject'i kapatmak çocuklarını da kapatır ve
`OnDisable`/`OnEnable` geri çağrılarını tetikler. Bizim istediğimiz tek şey "bu
kareyi çizme". `renderer.enabled` tam olarak bunu söyler; nesne hayatta kalır,
hiçbir yaşam döngüsü olayı tetiklenmez, referanslar bozulmaz.

**Alternatif:** rengin alfasını 0 yapmak da görünmez kılardı ama nesne yine
**çizilirdi** (görünmez bir draw call) ve prefab'da ayarlanan gerçek rengi
ezerdi.

Metodun sonunda "seçili miyim" bilgisi **saklanmıyor**: tek doğruluk kaynağı
`BoardAdapter.selectedUnit`; burada bir bool daha tutsaydık ikisi kayabilir ve
hata sessiz olurdu (`DECISION_TOOLKIT` sorusu 1). Bu bileşen durumu tutmaz,
durumu **uygular**.

---

## SetState(UnitState state)

### BU BLOK ÇEVRİLDİ, SİLİNMEDİ

Projedeki **ilk ters dönen karar** burasıdır. Çevrilmeden önce kazanan
`SetDowned(bool)`'du ve `SetState(UnitState)` bu dosyada REDDEDILEN olarak
yazılıydı; o bloğun kendi KAZANIRDI satırı tetiği önceden adlandırmıştı —
*"Dead ile Downed FARKLI görünmek zorunda kaldığı gün"*. Tetik geldi: madde #9,
"üç durum, iki görsel".

**ÇEVİREN OLAY, tek cümle:** `Dead` kendi görselini kazandı.

### HARİTA: iki bayrak DÖRT satır üretir, üçü anlamlı

Bir bool iki, iki bool dört kombinasyon taşır; gösterilecek durum ise üç. Sayılar
tutmadığı için fazladan bir satır doğar ve o satırın ekranda bir karşılığı
**yoktur** — kırılma tam orada.

```
REDDEDILEN — SetDowned(bool) + SetDead(bool)
  isDowned  isDead    ekranda              anlamı
  ────────  ──────    ─────────────────    ────────────
   false    false     ayakta, nötr         Alive      ✓
   true     false     yatık, soluk         Downed     ✓
   true     true      yatık, gri           Dead       ✓
   false    true      AYAKTA ama GRİ       ◄── YALAN
                      ██ KARŞILIĞI YOK ██
                      bu satıra düşmenin tek yolu
                      SetDead(true) demeyi unutmaktır

SEÇİLEN — SetState(UnitState)
  state      ekranda              taşınamayan değer
  ────────   ─────────────────    ─────────────────────
   Alive     ayakta, nötr         enum ÜÇ değerlidir;
   Downed    yatık, soluk         dördüncüsü YOKTUR,
   Dead      yatık, gri           yalan satırı DOĞMAZ
```

### KAPSAM: kural "bool kullanma" DEĞİL

Kural şu: bir üyenin taşıdığı değer sayısı, gösterilecek durum sayısından **az**
olduğu anda bool yalan söylemeye başlar. Sayılar eşitse bool doğru araçtır ve
enum yalnız gürültü olur.

**KARŞI ÖRNEK** aynı dosyada, hemen yukarıdaki `SetSelected(bool)`: o da bir bool
alır ve **doğrudur** — "seçili" gerçekten iki değerlidir, üçüncü bir seçim durumu
(yarı seçili, önizlenen) bugün yok. O gün gelirse aynı çeviri orada da yapılır;
bugün yapmak alıcısı olmayan bir enum üretirdi. İki üye, aynı dosyada, **zıt**
karar — ayıran tek şey sayı.

### İŞ BÖLÜMÜ: ÜÇ durumu İKİ eksen bölüşerek ayırır

Görsel tek eksende değil, iki eksenin **birleşiminde** oluşuyor:

```
              flipY (yatıklık)    çarpan (renk)
  Alive           hayır            Color.white
  Downed          EVET             downedTint
  Dead            EVET             deadTint
  ayırdığı çift:  Alive│Downed     Downed│Dead
```

`flipY` silinirse `Alive` ile `Downed` yalnız alfada ayrışır: ayakta duran soluk
bir birim "düşmüş" diye okunmaz. Çarpan silinirse `Downed` ile `Dead` ekranda
**birebir aynı** olur — madde #9'un kapattığı hata tam olarak buydu. İkisi aynı
işi iki kez yapmıyor; üç durumu ikiye bölüp paylaşıyor, bu yüzden hiçbiri
ötekinin yedeği değil.

### GARANTİ NEREDE BİTİYOR

"Derleyici üç dalı da sorar" kazancı **derleme zamanına kadar gitmez**: bir
switch **deyimi** eksik dal için uyarmaz. `UnitState`'e dördüncü bir değer
eklendiği gün bu dosya sorunsuz derlenir ve eksik yalnız çalışırken,
[`TintFor`](#tintforunitstate-state)'un `default` dalındaki `LogError` ile
görünür. Garanti derleyicide değil, o dalda bitiyor.

### REDDEDILEN

Çevrilmeden önceki kazanan; savaş sözlüğü hiç öğrenilmez, üç durum bayrağa iner:

```csharp
public void SetDowned(bool isDowned)
{
    Body.flipY = isDowned;
}
```

**KIRILAN:** bir bool **iki** değer taşır, gösterilecek durum ise **üç** tane.

```
üçüncüsü ikinci üyeyi doğurur -> SetDowned + SetDead
SetDead(true) demeyi unutan yol -> ölü birim ayağa kalkar
iki bayrak dört kombinasyon    -> üçü anlamlı, biri yalan
derleyici: hiçbir şey der  .  test: iki üye ayrı sınandığı için ikisi de yeşil
```

**KAZANIRDI:** hâlâ kazanırdı ve koşulu değişmedi — gösterilecek durum gerçekten
**iki** değerli olsaydı; madde #9'dan önce `Downed` ile `Dead` ekranda **aynı**
görünüyordu ve bool yeterliydi.

**TEK CUMLE:** Enum'u içeri almanın bedeli bir assembly referansı, kazancı
derleyicinin üç dalı da sormasıdır; bool'un bedeli ise çağıranın kimsenin
sormadığı bir sözü tutmak zorunda kalmasıdır.

### ESKİ KAZANANIN BEDELİ ÖDENDİ, ve tahmin birebir tuttu

Çevrilen blok *"`GridStrategy.Unity.EditModeTests`'e `GridStrategy.Combat`
referansı eklemek gerekir"* diyordu; bu dosyadaki `using GridStrategy.Combat;`
yüzünden `UnitViewTests` bugün bir `UnitState` değeri yazabilmek için o referansa
muhtaç.

Aynı bloğun **ikinci** iddiası ise **çürüdü**: *"'karar vermez, uygular' sözü ilk
satırda düşer"* denmişti — düşmedi, çünkü "Dead nasıl görünür" sorusuna bu metot
değil Inspector'daki iki çarpan cevap veriyor; tasarım kararı koda değil yazılı
veriye taşındı.

### Neden tek bir yerel değişken

Çizici tek bir yerele alınıyor ve bu bir üslup tercihi değil: property'ye ilk
dokunuş hem referansı hem **yazılı rengi** yakalar, ve iki satırın hangi sırayla
değerlendirildiğine güvenmek zorunda kalmak istemiyoruz. Yakalama gözle görülür
ve tek bir satırda oluyor.

### AYAKTA OLMAYAN HER DURUM YATIK

`Downed` ve `Dead` `flipY` ekseninde **aynı**. Üç durumu ayıran şey iki eksenin
birleşimi (yatıklık × çarpan), tek bir eksen değil — `Alive`/`Downed`
yatıklıkta, `Downed`/`Dead` çarpanda, `Alive`/`Dead` ikisinde birden ayrışır.

---

### İKİNCİ ÇEVRİLEN BLOK — renk soldurma

Burada renk soldurmayı **reddeden** bir blok duruyordu: *"`authoredColor` bir
alan olmak zorunda ve o kopya bu tipin 'hafıza: yok' satırını bozar; kopyayı
almayı unutan tek bir yol, birimin gerçek rengini kalıcı olarak siler."*
ÇEVİREN OLAY aynı: `Dead` kendi görselini kazandı ve üç durumu ayırmanın renksiz
bir yolu yok.

#### HARİTA: "unutma" riski nasıl YOK EDİLDİ

İtiraz bir **sıralama** riskiydi: kopya ayrı bir adımda alınırsa o adımı atlayan
bir yol doğabilir. Sıralamayı ortadan kaldırınca itirazın dayandığı zemin de
kalkıyor.

```
REDDEDILEN SIRA — iki ayrı adım
  ① Awake ─────► authoredColor = body.color
  ② SetState ──► body.color = authoredColor * tint
  ① atlanırsa (EditMode'da Awake HİÇ koşmaz):
     authoredColor = Color.white kalır   ◄── SESSİZ KAYIP
     beyaz "makul" göründüğü için hata fark bile edilmez

BUGÜNKÜ SIRA — atlanacak adım YOK
  her çağıran ──► Body { get }    ██ TEK KAPI ██
                    ├── body = GetComponent<SpriteRenderer>()
                    └── authoredColor = body.color
                        (aynı if, arka arkaya iki satır)
  yazma ancak kapıdan GEÇTİKTEN sonra mümkün; gövdeye erişmenin
  başka yolu olmadığı için "rengi yakalamadan renge yazmak"
  diye bir sıralama KURULAMAZ
```

**İTİRAZ NEDEN ÇÜRÜDÜ:** "unutmak" bir sıralama riskiydi ve sıralama ortadan
kalktı — kopya artık gövdeye erişmenin **tek kapısı** olan `Body` property'sinde,
referansın çözüldüğü satırın yanında alınıyor.

**AYAKTA KALAN YARISI:** "gövde rengi bir gün takımı taşır ve düşme onu ezer"
hâlâ doğru; cevabı **çarpmadır** — düşme ezmez, karartır ve takım bilgisi
çarpanın altında yaşar. Çarpmanın kendi sınırı [`deadTint`](#deadtint)
bölümünde ölçülmüş hâliyle yazılı.

#### Null kontrolü YOK — ve bu `selectionOverlay` ile çelişmiyor

`RequireComponent` bu çizicinin varlığını garanti eder. Ayrımın ağacı
[ROL künyesi](#rol) bölümünde.

#### KAPSAM: kural TÜREV değerlere özeldir

Kural: bir alan başka bir nesneden **türetiliyorsa**, türetildiği referansla
**aynı satırda** alınmalıdır. Dışarıdan yazılan değerler bu kuralın tamamen
dışında kalır.

**KARŞI ÖRNEK** aynı dosyada, `selectionOverlay` ve `downedTint`: ikisi de
alandır, ikisinin de kapıya ihtiyacı **yoktur** — ikisini de serileştirici
doldurur ve hiçbiri başka bir nesneden türetilmez, yani eskiyebilecek bir kopya
değildir. `authoredColor`'ı onlardan ayıran tek şey **türev** olması; "renk
alanı" olması değil.

#### İŞ BÖLÜMÜ: çarpanın iki tarafı iki ayrı soruyu taşır

```
authoredColor  ► birimin KENDİ kimliği (bugün beyaz, yarın takım rengi)
                 — prefab'da yazılı
TintFor(state) ► birimin YAŞAM durumu — Inspector'da yazılı
```

Sol taraf silinip mutlak renk yazılsaydı düşme, takım bilgisini **silerdi**. Sağ
taraf silinip yalnız `flipY` bırakılsaydı `Downed` ile `Dead` ekranda aynı
olurdu. Çarpma bir "renk hesabı" değil, iki ayrı sahibin tek satırda buluşma
noktasıdır.

#### HANGİ SATIR KORUMUYOR

Alanın bildirimindeki `= Color.white` başlangıç değeri bir savunma **değil**, tam
tersi: kapı bir gün atlanırsa sonuç görünür bir hata değil, **makul görünen
yanlış bir renk** olur. `private` da korumaz — yanlış değerin dışarıdan
**yazılmasını** engeller, **eskimesini** değil. Koruma tek bir olgudan geliyor:
kopyanın alındığı satırdan başka gövdeye erişen bir satır yok.

#### REDDEDILEN

Renk hiç kullanılmaz, üçüncü durum ikinci bir **bool** eksenine bindirilir:

```csharp
bodyRenderer.flipY = state != UnitState.Alive;
bodyRenderer.flipX = state == UnitState.Dead;
```

**KIRILAN:** hiçbir alan gerekmez — cazip olan yanı bu; kırılan şey görsel dilin
kendisi.

```
iki eksende ters sprite -> oyuncu "ölü" değil "yanlış çizilmiş" okur;
                           simetrik sprite'ta hiçbir şey okumaz
derleyici: hiçbir şey der  .  test: YEŞİL kalır, çünkü flipX/flipY
                                    alanlarını okur, ekranı değil
```

**KAZANIRDI:** gövde rengi bu tipin **dışından** yazılırsa (takım rengi, hasar
yanıp sönmesi) `authoredColor` gövdenin yazılı rengi olmaktan çıkar; o gün bool
ekseni kusurlu ama dürüsttür.

**TEK CUMLE:** Bir görsel eksenin değeri kaç durum taşıyabildiğiyle değil,
oyuncunun onu ne sandığıyla ölçülür.

#### `Alive` için ayrı bir dal neden yok

Çarpan `Color.white`'tır ve bu **nötr eleman** olduğu için ayrı bir dal
gerekmiyor: `authoredColor * white` kayan noktada **tam olarak**
`authoredColor`'dır. Yani diriltme geldiği gün renk birebir geri gelir;
"yaklaşık geri gelir" değil.

#### Durum burada SAKLANMIYOR

"Hangi durumdayım" bilgisinin tek doğruluk kaynağı `Combatant.State`; buraya bir
alan koysaydık savaş çekirdeği ile ekran sessizce ayrışabilirdi — tam olarak
`SetSelected`'ın kaçındığı hatanın ikizi.

---

## TintFor(UnitState state)

Bir duruma karşılık gelen renk **çarpanını** verir. `Alive` dalı nötr çarpan
döner: yazılı renk aynen kalır.

### `default` LOG DEĞİL LogError

Buraya düşmek "`UnitState`'e dördüncü bir değer eklendi ve bu switch
güncellenmedi" demektir, yani bir **programcı hatasıdır**. Aynı gerekçe
`BoardAdapter`'ın sonuç switch'lerinde de yazılı.

### Nötr çarpanla dönmek BİR KARAR

Bilinmeyen bir durumda birimi **görünür** bırakır. `Color.clear` dönseydi birim
ekrandan kaybolurdu ve bir programcı hatası bir **oyun** hatasına dönüşürdü.
