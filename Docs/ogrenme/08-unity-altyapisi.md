# Unity'nin altyapısı — motor neyi, NEDEN ve NASIL çözüyor

> **Nerede geçiyor:** `UnityEngine.CoreModule.dll` → `Assets/Game/Unity/BoardAdapter.cs`
> → `Assets/Game/Unity/UnitView.cs` → `Assets/Scenes/SampleScene.unity`
> → `Assets/Game/Prefabs/Unit.prefab` → `ProjectSettings/ProjectSettings.asset`
> **Kodda nereden geldin:** `Vector3`, `transform.position`, `[SerializeField]`,
> `private void Awake()`, `Instantiate`, `GetComponent`, `.meta` / GUID
> **Ne zaman oku:** birine "GameObject bir C# nesnesidir" demeden hemen önce;
> `transform.position.x = 5` yazıp derleyicinin neden reddettiğini anlamadığında;
> ya da "kodu öğrendim, artık Unity tarafına geçeyim" dediğin gün.

---

## Bu dosyanın sınırı — üç sahip, üç ayrı soru

Aynı konuya bakan üç dosya var ve **üçü ayrı soruyu** cevaplıyor. Karıştırılırsa
buradaki her cümle yanlış okunur.

```
   [konular/08]                 [dil/07]                  [BU DOSYA — 08]
   ───────────────              ──────────                ────────────────
   NE OLUYOR                    NE ZAMAN BİTİYOR          >> NEDEN VE NASIL <<
   Awake→OnEnable→Update        kapsam · canlılık ·       motor bu işi neden
   sırası ve sahipleri          erişilebilirlik ·         böyle çözdü, ve
   `Awake` bir `event`          kaynak ömrü               TEKNİK OLARAK nasıl
   DEĞİL                        `Destroy` sonrası         çözüyor
   `IEnumerator`in ikinci       `== null` ama null        yönetilen ↔ yerel
   hayatı · Domain Reload       DEĞİL                     sınırı, ÖLÇÜLMÜŞ
```

- Çağrı sırası, garantiler, `Start`'ın neden yok olduğu, coroutine sayımı ve
  Domain Reload: [`konular/08-motor-cagri-dongusu.md`](../deep/konular/08-motor-cagri-dongusu.md).
  ***Burada TEKRAR EDİLMİYOR.***
- Yönetilen/yerel ömür, `Destroy`, yıkılmış nesnenin `== null` demesi, "değer
  tipi = stack değildir": [`dil/07-bellek-canlilik-ve-yikim.md`](../deep/dil/07-bellek-canlilik-ve-yikim.md).
  ***Burada TEKRAR EDİLMİYOR.***

Onlar *"ne oluyor"* diyor. Bu dosya ***"motor bunu neden ve nasıl böyle
çözüyor"*** diyor. Ve her cevabını yerel kurulumdan ölçüyor.

---

## Ölçüm künyesi — bu dosyadaki her iddia nasıl ölçüldü

```
ProjectSettings/ProjectVersion.txt
    m_EditorVersion             : 2021.3.45f2
    m_EditorVersionWithRevision : 2021.3.45f2 (88f88f591b2e)

Doğrulama tarihi : 2026-08-23
Ölçüm yeri       : C:/Program Files/Unity/Hub/Editor/2021.3.45f2/Editor/Data/
Kullanılan araçlar (üçü de YEREL KURULUMDAN, dışarıdan indirilen yok):
    MonoBleedingEdge/bin/mono.exe                 — çalıştırıcı
    MonoBleedingEdge/lib/mono/4.5/ikdasm.exe      — IL sökücü (disassembler)
    MonoBleedingEdge/lib/mono/4.5/csc.exe         — C# derleyicisi
Sökülen ikili    : Managed/UnityEngine/UnityEngine.CoreModule.dll (1.422.336 bayt)
Üretilen IL      : 266.910 satır
```

*****`monodis` bu kurulumda ÇALIŞMAZ.***** Ölçüldü: Windows kurulumundaki
`MonoBleedingEdge/bin/monodis` bir **Mach-O 64-bit x86_64** ikilisi, yani macOS
artığı; `bin/ikdasm` ise gövdesinde `/Users/bokken/...` yazan bir POSIX kabuk
betiği ve o yol Windows'ta yok. Çalışan yol `lib/mono/4.5/ikdasm.exe`'yi doğrudan
`mono.exe` ile koşturmaktır. Bunu yazıyorum çünkü ilk denemede iki araç da
sessizce yanlış sonuç üretebilirdi.

*****Editor bu turda HİÇ AÇILMADI.***** Bütün ölçümler diskteki dosyalara,
sökülen IL'e ve yansımayla (reflection) koşturulan sondalara karşı yapıldı.
Editor'de koşturulması gereken her şey açıkça ***DOĞRULANMADI*** diye
işaretlendi ve son duraktaki geçiş listesine bir adım olarak yazıldı.

---

## Sahne

Play'e basıyorsun. Tahta beliriyor. `BoardAdapter` bir `Grid` buluyor, bir
`Battle` kuruyor, iki asker doğuruyor ve her karede `battle.Tick(...)` çağırıyor.

Şimdi tek soru: *****`transform.position = ...` yazdığında ne oluyor?*****
Bir C# alanına mı yazıyorsun? Hayır. Bir C# nesnesinin içindeki üç `float`'a mı?
Hayır. Bu dosyanın tamamı o "hayır"ın açılımı.

---

## Karakterler

```
╔═ YEREL MOTOR (C++ tarafı) ════════════════════════════════════╗
║  İşi   : sahneyi tutmak, kareyi saymak, çizmek, dönüştürmek   ║
║  BİLMEZ: senin C# tipini, senin oyununu                       ║
║  ÖLÇÜ  : `Vector3` tipinin üstünde NativeClassAttribute       ║
║          ("Vector3f") ve NativeHeaderAttribute                ║
║          ("Runtime/Math/Vector3.h") yazıyor — C# tarafı bir   ║
║          C++ tipinin AYNASI                                   ║
╚═══════════════════════════════════════════════════════════════╝
╔═ YÖNETİLEN CEPHE (UnityEngine.CoreModule.dll) ════════════════╗
║  Bilir : yerel nesnenin ADRESİNİ — o kadar                    ║
║  BİLMEZ: nesnenin İÇİNİ. Veri onda DEĞİL                      ║
║  ÖLÇÜ  : GameObject · Component · Behaviour · MonoBehaviour   ║
║          >> DÖRDÜNÜN DE BİLDİRDİĞİ ÖRNEK ALAN SAYISI: 0 <<    ║
║          Zincirdeki tek veri UnityEngine.Object'te:           ║
║          IntPtr m_CachedPtr · Int32 m_InstanceID              ║
╚═══════════════════════════════════════════════════════════════╝
╔═ SENİN KODUN (GridStrategy.Unity) ════════════════════════════╗
║  İşi   : iki dünya arasında çeviri                            ║
║  BİLMEZ: `Awake`'ini kimin çağırdığını — gerek de yok         ║
║  ÖLÇÜ  : `Vector3` üretim kodunda YALNIZ 2 satırda geçiyor    ║
╚═══════════════════════════════════════════════════════════════╝
╔═ DERLEYİCİ (Roslyn / csc) ════════════════════════════════════╗
║  Bilir : tip, imza, erişim, değer tipi kuralları              ║
║  BİLMEZ: >> `Awake` diye bir ADIN motor için anlamı olduğunu <<║
║  ÖLÇÜ  : `void Awakee()` -warn:4 ile derlendi, 0 uyarı,       ║
║          3072 baytlık bir DLL üretti                          ║
╚═══════════════════════════════════════════════════════════════╝
```

En öğretici satır sonuncusudur ve dördüncü durakta açılıyor.

---

## Birinci durak: ***İKİ DÜNYA — yönetilen C# ve yerel motor***

Unity'nin çekirdeği C++ ile yazılı. Senin yazdığın C# o çekirdeğe bir **cephe**
(facade). ***`GameObject`, `Transform`, `Camera` ve `SpriteRenderer` veri taşıyan
C# nesneleri DEĞİLDİR.*** Bu dördü, yerel taraftaki nesnelere birer TUTAMAKTIR.

### ***Bunun ölçüsü tek bir sayı: SIFIR***

Yerel kurulumdaki `UnityEngine.CoreModule.dll` yansımayla açıldı ve her tipin
**kendi bildirdiği** örnek alanları sayıldı:

```
tip                        bildirilen ÖRNEK ALAN sayısı
──────────────────────     ────────────────────────────
UnityEngine.GameObject                  >> 0 <<
UnityEngine.Component                   >> 0 <<
UnityEngine.Behaviour                   >> 0 <<
UnityEngine.MonoBehaviour               >> 0 <<
UnityEngine.Transform                   >> 0 <<
──────────────────────     ────────────────────────────
UnityEngine.Object                          3
    IntPtr m_CachedPtr            ◄── >> YEREL NESNENİN ADRESİ <<
    Int32  m_InstanceID           ◄── oturum içi kimlik
    String m_UnityRuntimeErrorString
```

***Bir `GameObject`'in adı, katmanı, etkinliği ve bileşen listesi — bu dördünün
**hiçbiri** C# tarafında durmuyor.*** Yönetilen nesnenin taşıdığı tek gerçek
veri, kalıtımla gelen bir `IntPtr`'dir. Geri kalan her şey, o adresin ucundaki
C++ nesnesinde yaşıyor.

Bu, "GameObject bir C# nesnesidir" cümlesinin neden yanlış olduğunun tam
ölçüsüdür: **o bir C# nesnesidir, ama içi boştur.**

### İki ömür — ve bu dosyanın burada durduğu yer

`Destroy` çağrıldığında yerel taraf yıkılır, yönetilen sarmalayıcı yerinde
kalır ve `== null` demeye başlar. Bunun tam anlatımı — hangi ömrün ne zaman
bittiği, `ReferenceEquals`'ın neden başka cevap verdiği — bu dosyanın işi değil:
[`dil/07` → Dördüncü durak](../deep/dil/07-bellek-canlilik-ve-yikim.md#dorduncu-durak-unitynin-iki-omru)
ve
[`dil/07` → yıkılmış nesne `== null` der](../deep/dil/07-bellek-canlilik-ve-yikim.md#bu-yuzden-yikilmis-bir-nesne-null-der-ama-null-degildir).

***Buradaki tek katkı şu: o davranış bir kapris değil, yukarıdaki `IntPtr`'nin
doğrudan sonucudur.*** `m_CachedPtr` sıfırlandığında yönetilen kutu hâlâ
duruyordur; `operator ==` tam olarak o alana bakar.

### ***ÇAĞRI SINIRINI ÖLÇ — kaç metot yerel tarafa geçiyor***

`UnityEngine.CoreModule.dll` `ikdasm` ile söküldü ve `MethodImplOptions.InternalCall`
damgalı metotlar sayıldı:

```
UnityEngine.CoreModule.dll  (2021.3.45f2, 2026-08-23)

    toplam .method bildirimi                      11.916
    `cil managed internalcall` damgalı              3.654      >> %30,7 <<
    `pinvokeimpl` (P/Invoke) damgalı                    0
    adında `_Injected` geçen satır                  2.220
        bunlardan internalcall olanlar                324

    Transform :  133 metot,  55 internalcall
    GameObject:   97 metot,  37 internalcall,  0 alan
    Vector3   : 2.668 IL satırı,  >> yalnız 5 internalcall <<
```

*****P/Invoke sayısı SIFIR.***** Yani Unity yerel tarafa `DllImport` ile
gitmiyor; çalışma zamanının kendi iç çağrı mekanizmasını (`InternalCall`)
kullanıyor. Bu iki şey aynı değildir ve karıştırılırsa "Unity her API'de bir DLL
sınırı geçiyor" gibi ölçüsüz bir cümle doğar.

### ***Sınır TEK ŞEKİLLİ DEĞİL — dört ayrı şekil, ölçüldü***

Bu projenin **gerçekten kullandığı** API'ler tek tek yansımayla sorgulandı
(`GetMethodImplementationFlags()`), sonuç ***dört*** kova:

| Şekil | Bu projede geçen üyeler (21 üye tek tek sorgulandı) |
|---|---|
| ██ DOĞRUDAN internalcall ██ — kamu metodunun kendisi damgalı, gövdesi boş | `Time.deltaTime` (get) · `Camera.main` (get) · `Input.GetMouseButtonDown(int)` · `GameObject.SetActive(bool)` · `Behaviour.enabled` (set) · `Transform.SetParent(Transform,bool)` |
| ██ YÖNETİLEN IL + `_Injected` internalcall ██ — kamu metodunun IL'i VAR, bir stub'a iniyor | `Transform.position` (get/set) · `Transform.localPosition` (get) · `Transform.localToWorldMatrix` (get) · `Camera.ScreenToWorldPoint(Vector3)` · `Input.mousePosition` (get) · `Vector3.Slerp` |
| ██ YALNIZ YÖNETİLEN IL ██ — bu üye doğrudan yerel tarafa geçmiyor | `Object.Destroy(Object)` · `Object.name` (get) · `Component.GetComponent(Type)` · `GameObject.AddComponent(Type)` · `Object.operator ==` · `Transform.parent` (get, `get_parentInternal`'e iner) |
| ██ YEREL TARAFA HİÇ GİTMİYOR ██ — saf C# hesabı | `Vector3.operator +` · `Vector3.normalized` (get) |

***"Unity API'sinin IL'i yoktur" cümlesi ÖLÇÜLEREK yanlıştır.*** İşlenmiş örnek,
`Transform.position`'ın tam gövdesi (yerel kurulumdan sökülmüş IL):

```
.method public hidebysig specialname instance valuetype UnityEngine.Vector3
        get_position() cil managed              ◄── >> IL GÖVDESİ VAR: 10 bayt <<
{
  .locals init (valuetype UnityEngine.Vector3 V_0)   ← yerel bir Vector3 ayır
  IL_0000:  ldarg.0                                   ← this
  IL_0001:  ldloca.s   V_0                            ← o Vector3'ün ADRESİ
  IL_0003:  call  instance void UnityEngine.Transform::get_position_Injected(
                                            valuetype UnityEngine.Vector3&)
  IL_0008:  ldloc.0                                   ← doldurulmuş kopyayı yükle
  IL_0009:  ret
}

.method private hidebysig specialname instance void
        get_position_Injected([out] valuetype UnityEngine.Vector3& 'ret')
        cil managed internalcall                ◄── >> GÖVDE BOŞ <<
{
}                                               ◄── tek satır IL yok; yerel taraf doldurur
```

Okunan şekil şudur: **kamu yüzeyi yönetilen, sınırı geçen şey bir adres.**
Değer geri dönerken kopyalanmıyor. Çağıran bir `Vector3` ayırıyor, ve yerel
taraf onu **doldurmak için** adresini alıyor. `_Injected` adı bir proje
sözleşmesi değildir; sürüme göre değişebilir ve buna güvenerek kod yazılmaz.

### ***MALİYET İDDİASI ÖLÇÜSÜZ YAZILMAZ***

"Yerel geçiş pahalıdır" bir **etikettir**, ölçü değil. Bu turda Profiler
açılmadı, hiçbir zamanlama yapılmadı, hedef cihazda hiçbir yapı alınmadı.

*****Bu dosya o cümleyi KURMUYOR.***** Doğru olan şudur:

```
ÖLÇÜLEN   : CoreModule'ün 11.916 metodundan 3.654'ü internalcall damgalı.
            Bu bir YAPI ölçüsüdür — "kaç kapı var" sorusunun cevabı.
ÖLÇÜLMEYEN: o kapılardan geçmenin kaç nanosaniye sürdüğü.
SONUÇ     : >> Bir yerel geçişin varlığı, tek başına bir performans
            probleminin KANITI DEĞİLDİR. << Kanıt, ölçülmüş bir kare
            zamanı ya da ölçülmüş bir tahsis sayısıdır.
```

Bu projenin bugün sahip olduğu **tek** ölçülmüş performans kanıtı bir tahsis
testidir ve konusu motor sınırı değil, saf C# kuralları:
[`dil/07` → Beşinci durak](../deep/dil/07-bellek-canlilik-ve-yikim.md#besinci-durak-bu-projenin-tahsis-gercegi-olculmus).

*****NE ZAMAN ÖLÇERSİN:***** kare zamanı gözle görülür şekilde bozulduğunda ve
Profiler bir API ailesini işaret ettiğinde. Sıra şu: önce ölçü, sonra iddia.
Tersi sırayla yazılan her cümle bu belgenin kendi kuralını çiğner.

---

## İkinci durak: ***`Vector3` NASIL TANIMLANMIŞ, NEDEN***

### ① ÖLÇÜM — önce sayı, sonra cümle

Yansıma sondası, yerel `UnityEngine.CoreModule.dll` üzerinde koşturuldu:

```
typeof(Vector3).Assembly.GetName().Name   ►  UnityEngine.CoreModule
typeof(Vector3).IsValueType               ►  >> True <<
typeof(Vector3).IsClass                   ►  False
typeof(Vector3).BaseType                  ►  System.ValueType
typeof(Vector3).IsSealed                  ►  True
Marshal.SizeOf(typeof(Vector3))           ►  >> 12 <<  (bayt)
örnek alan sayısı                         ►  3
    Single x · Single y · Single z
```

Sökülen IL aynı şeyi bir kez daha söylüyor:

```
.class public sequential ansi sealed beforefieldinit UnityEngine.Vector3
       extends [mscorlib]System.ValueType          ◄── >> struct <<
       implements System.IEquatable`1<Vector3>, System.IFormattable
{
  .custom ... NativeHeaderAttribute("Runtime/Math/Vector3.h")
  .custom ... NativeClassAttribute("Vector3f")     ◄── C++ karşılığının ADI
  .field public float32 x
  .field public float32 y
  .field public float32 z
}
```

***`sequential` damgası bir tesadüf değil:*** alan sırasının bellekte korunmasını
söyler. Yerel taraf aynı 12 baytı `Vector3f` olarak okuyacak; sıra bozulsaydı
`get_position_Injected`'ın doldurduğu şey karışırdı.

### ② NEDEN `struct` — üç ayrı gerekçe, üçü de bağımsız

**GEREKÇE 1 — tahsis.** Üç `float`, toplam 12 bayt. Bir oyunda konum saniyede
onlarca kez, birim sayısıyla çarpılarak okunup yazılır. `class` olsaydı her
`Vector3` bir yönetilen **nesne** olurdu; her `new Vector3(...)` bir yığın
tahsisi, her tahsis çöp toplayıcı için bir gelecek iş.

**GEREKÇE 2 — değer semantiği.** `a = b` yazan kod ikinci bir **yazar**
üretmemeli. `class` olsaydı iki değişken aynı nesneyi gösterirdi ve birinin
`x`'ini değiştirmek ötekini de değiştirirdi. Bu projede tam olarak bu tuzağın
karşıtı bir karar var — tahta sahipliğinin ikinci yazar üretmemesi:
[`konular/03-tahta-sahipligi.md`](../deep/konular/03-tahta-sahipligi.md).

**GEREKÇE 3 — yerel yerleşim.** Yukarıdaki `sequential` + `Vector3f` çifti.
Bir `class` olsaydı nesnenin başında yönetilen bir başlık dururdu ve o 12 bayt
doğrudan C++ tarafına verilemezdi.

### ③ ***AMA: "struct = stack" MUTLAK OLARAK ÖĞRETİLMEZ***

Bir `Vector3`'ün **nerede** durduğu, tipinin değil onu **saranın** sorusudur.
Bir sınıfın alanı ise yönetilen yığındadır. Bu ayrımın işlenmiş üç örneği — ve
bu projedeki `Color authoredColor` örneği — burada tekrar edilmiyor; sahibi:
[`dil/07` → Aynı `int` üç ayrı yerde](../deep/dil/07-bellek-canlilik-ve-yikim.md#ayni-int-uc-ayri-yerde-k21in-islenmis-ornegi).

Kodun kendisi de bunu yazıyor:

```
UnitView.cs:81   private Color authoredColor = Color.white;
```

`Color` bir `struct`'tır ve bu alan bir `UnitView` **nesnesinin içinde**,
yönetilen yığında yaşar.

### ④ ***`Vector3.zero` — ÖLÇÜLDÜ, ve beklenenden farklı çıktı***

Yaygın cümle "`Vector3.zero` bir `static readonly` alandır" der. **Bu sürümde
değil.** Ölçüm:

```
typeof(Vector3).GetMember("zero", Public|Static)[0].MemberType
                                        ►  >> Property <<   (Field DEĞİL)
```

IL bunu açıyor:

```
.method public hidebysig specialname static
        valuetype UnityEngine.Vector3  get_zero() cil managed aggressiveinlining
{
  IL_0001:  ldsfld  valuetype UnityEngine.Vector3 UnityEngine.Vector3::zeroVector
  IL_000a:  ret
}
.field private static initonly valuetype UnityEngine.Vector3 zeroVector
```

Yani: ***`zero` bir **özelliktir**; arkasında `private static initonly`
(`static readonly`) bir alan var ve getter `AggressiveInlining` damgalı.***
Davranışça `new Vector3(0f, 0f, 0f)` ile aynı sonucu verir; **yapı olarak**
aynı şey değildir. Aynısı `one`, `up`, `right`, `forward` için de geçerli.
Onların arkasında da `oneVector`, `upVector`, `rightVector`, `forwardVector`
alanları var.

***Bunu düzeltilmiş olarak yazıyorum çünkü ölçü ezberi bozdu.*** Ezberi
korumak, ölçüyü çöpe atmaktır.

### ⑤ ***`transform.position` bir ALAN DEĞİL — ve faturası bir DERLEME HATASI***

```
typeof(Transform).GetProperty("position").MemberType  ►  Property
typeof(Transform).GetField("position")                ►  >> null — ALAN YOK <<
```

İki karar birleşince görünür bir fatura doğuyor: `position` bir **özellik**,
`Vector3` bir **değer tipi**. Bir özelliğin getter'ı sana bir **kopya** verir.
Kopyaya yazmak anlamsızdır ve derleyici tam olarak bunu söyler.

***ÖLÇÜLDÜ*** — aşağıdaki dosya yerel `csc.exe` ile derlendi:

```csharp
using UnityEngine;
public class Cs1612Probe : MonoBehaviour
{
    private void Deneme()
    {
        transform.position.x = 5f;
    }
}
```

Derleyicinin döndürdüğü tam metin:

```
CS1612.cs(6,9): error CS1612: Cannot modify the return value of
                'Transform.position' because it is not a variable
```

*****CS1612.***** Hata kodunu ezberle, çünkü kodu görünce sebebi tek cümlede
kurabilirsin: *"bir değer tipi döndüren özelliğin dönüşü değiştirilemez."*
Çözüm de tek satırdır: bütün vektörü yaz.

```
BoardAdapter.cs:741   view.transform.position = CellCentre(x, y);
```

Bu satır projedeki dört konum yazımından biri ve dördü de aynı biçimde —
parça değil, **bütün vektör**:

```
BoardAdapter.cs:423   placementGhost.transform.position = CellCentre(x, y);
BoardAdapter.cs:570   structureObject.transform.position = CellCentre(x, y);
BoardAdapter.cs:677   cell.transform.position = CellCentre(x, y);
BoardAdapter.cs:741   view.transform.position = CellCentre(x, y);
```

***İkinci fatura: `position` bir özellik olduğu için her okuma bir ÇAĞRIDIR.***
Ölçülmüş şekli birinci durakta: `get_position` → `get_position_Injected`
(internalcall). Bir alan okuması olsaydı bu bir bellek erişimi olurdu.
***Ama maliyet iddiası yine ölçüsüz yazılmaz.*** Buradaki fark bir **yapı**
farkıdır, ölçülmüş bir performans farkı değil.

### ⑥ `Vector3` matematiği yerel tarafa GİTMİYOR — ölçüldü

```
Vector3.operator +      ►  yönetilen IL gövdesi, internalcall YOK
Vector3.normalized      ►  yönetilen IL gövdesi, internalcall YOK
Vector3 tipinin tamamı  ►  2.668 IL satırı, yalnız >> 5 << internalcall
```

Yani vektör toplama, çıkarma, çarpma, `magnitude` — hepsi saf C#. Yerel tarafa
giden beş metot `Slerp`, `RotateTowards` gibi **karmaşık** olanlar. Bu, birinci
duraktaki "sınır tek şekilli değil" cümlesinin en temiz kanıtı: aynı tipin
içinde bile iki ayrı dünya var.

### ⑦ Bu projede `Vector3` nerede geçiyor — SAYILDI

```
ÜRETİM KODU (Assets/Game/)
    Vector3     ► 2 satır      ikisi de BoardAdapter.cs
    Vector3Int  ► 2 satır      ikisi de BoardAdapter.cs
    UnitView.cs ► >> SIFIR <<  bu dosya bir vektör hiç görmez
TEST KODU (Assets/Tests/)
    Vector3     ► 3 satır      GridCellGapCharacterizationTests.cs
```

Dört üretim satırının tamamı:

```
BoardAdapter.cs:603   Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
BoardAdapter.cs:609   Vector3Int cell = unityGrid.WorldToCell(worldPoint);
BoardAdapter.cs:712   private Vector3 CellCentre(int x, int y)
BoardAdapter.cs:714   return unityGrid.GetCellCenterWorld(new Vector3Int(x, y, 0));
```

***Şeklin kendisi bir mimari ifadesidir:*** `Vector3` **duvarı geçmiyor**.
Piksel, dünya noktası ve hücre indeksi arasındaki çeviri `BoardAdapter`'da
başlıyor ve orada bitiyor; duvarın öte yanına yalnız `int x, int y` geçiyor.
Duvarın kendi hikâyesi: [`konular/02-assembly-duvari.md`](../deep/konular/02-assembly-duvari.md).

`Vector3Int` ayrı bir tiptir ve **bilerek** kullanılıyor: `float` bir hücre
indeksi olamaz. Kodun kendi notu (`BoardAdapter.cs:607-608`) bunu yazıyor.
`Vector3Int` sınırın ötesine geçmez. "Tahta içinde mi" sorusunu soran taraf
yine `Battle`'dır.

---

## Üçüncü durak: ***MOTORUN ÇÖZDÜĞÜ ŞEYLER — teknik olarak nasıl***

Her biri için dört alan: **PROBLEM · MOTORUN ÇÖZÜMÜ · SEN OLSAN NE YAPARDIN ·
MALİYET**.

### 3.1 · Sahne grafiği ve `Transform` hiyerarşisi

**PROBLEM.** Bir tahtayı yok ettiğinde üstündeki 15 hücre görselinin de gitmesi
gerekir. Bir birimi taşıdığında altındaki gölgenin de taşınması gerekir. Yani
nesnelerin **birbirine bağlı** olması, ve bu bağın hem ömrü hem konumu taşıması
gerekir.

**MOTORUN ÇÖZÜMÜ.** Hiyerarşi `GameObject`'te değil ***`Transform`'da*** yaşar.
Her `GameObject` **tam bir** `Transform` taşır; ebeveyn/çocuk bağı o bileşenin
üstündedir. Kodun kendi notu bunu yazıyor:

```
BoardAdapter.cs:673   cell.transform.SetParent(transform, worldPositionStays: false);
```

Çıplak `transform` = `this.transform`. Amaç konum değil ***TOPLU YAŞAM
DÖNGÜSÜ***: tahtayı yok etmek tek çağrıyla 15 hücreye uygulanır.

***Yerel koordinat ile dünya koordinatı ayrı şeylerdir.*** Ölçülmüş şekil şudur:
`Transform` tipinde `position` ve `localPosition` **iki ayrı özelliktir**, ve
ikisinin de ayrı bir `_Injected` internalcall'ı vardır. `localToWorldMatrix` ile
`worldToLocalMatrix` de ayrı birer internalcall'dır. Sahnede saklanan şey
**yereldir**. Bunun ölçüsü sahne dosyasının kendisidir:

```
Assets/Scenes/SampleScene.unity
    Transform:
      m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
      m_LocalPosition: {x: 1.5, y: 2.5, z: -10}     ◄── LOCAL, world DEĞİL
      m_LocalScale:    {x: 1, y: 1, z: 1}
      m_Father: {fileID: 0}                          ◄── ebeveyn bağı BURADA
      m_Children: []
```

*****DOĞRULANMADI:***** "`transform.position` okumak hiyerarşi boyunca matris
çarpımı yapabilir" cümlesi yerel kaynaktan **doğrulanamadı**. Sebebi şu: yerel
taraf C++ ve elimde kaynağı yok. ***ÖLÇÜLEN olgu şudur:*** dünya konumu sahnede
**saklanmıyor**, yalnız yerel konum saklanıyor. Dünya konumunu döndüren metot
ayrı bir yerel çağrıdır, ve `localToWorldMatrix` diye bir kardeşi var. Bu
şekilden "türetiliyor" çıkarımı yapılabilir. Ama o çıkarım **kanıt değildir**,
ve bu dosya onu kanıt gibi yazmıyor.

**SEN OLSAN NE YAPARDIN.** Her nesnede bir `parent` referansı ve bir `localPos`
tutardın; dünya konumunu isteyen olduğunda zinciri yukarı yürürdün. Aynı fikir.
Farkı: motor bunu C++'ta, bütün nesneler için, tek bir bellek düzeninde yapıyor.

**MALİYET.** Derin hiyerarşi = daha uzun zincir. ***Bu projede ölçülmedi ve
ölçülmesi de gerekmiyor: hiyerarşi **iki seviye** derin.***
`Board` → `Cell_x_y` ve `Board` → `Unit_...`. Üçüncü seviye yok.

### 3.2 · Oyun döngüsü — kim çağırıyor, sıra kimin kararı

**PROBLEM.** Yüzlerce nesnenin her karede güncellenmesi, fiziğin sabit adımla
koşması, girdinin kare başında okunması ve çizimin en sonda olması gerekir.
Sıra yanlışsa girdi bir kare gecikir, kamera bir kare titrer.

**MOTORUN ÇÖZÜMÜ.** ***Motorun kare planını `PlayerLoop` tutar.*** Bu plan,
adlandırılmış fazlardan ve alt sistemlerden oluşan bir **ağaçtır**. Ağaç bir
soyutlama değil, ölçülebilir bir API'dir: `UnityEngine.LowLevel.PlayerLoop` ve
`UnityEngine.LowLevel.PlayerLoopSystem` tipleri bu sürümde **var**.

***Yerel `UnityEngine.CoreModule.dll`'den sökülerek sayıldı (2026-08-23):***

```
UnityEngine.PlayerLoop  —  >> 8 faz, 127 alt sistem <<

  1 TimeUpdate       (2)   WaitForLastPresentationAndUpdateTime · ProfilerStartFrame
  2 Initialization   (8)   PlayerUpdateTime · SynchronizeInputs · XREarlyUpdate ...
  3 EarlyUpdate     (32)   >> UpdateInputManager <<  ← girdi BURADA okunuyor
                           >> ScriptRunDelayedStartupFrame <<
  4 FixedUpdate     (12)   >> ScriptRunBehaviourFixedUpdate <<
                           PhysicsFixedUpdate · Physics2DFixedUpdate
                           ScriptRunDelayedFixedFrameRate
  5 PreUpdate        (9)   PhysicsUpdate · SendMouseEvents · AIUpdate
  6 Update           (4)   >> ScriptRunBehaviourUpdate <<   ← senin Update'in
                           DirectorUpdate
                           >> ScriptRunDelayedDynamicFrameRate <<  ← coroutine
                           ScriptRunDelayedTasks
  7 PreLateUpdate   (14)   >> ScriptRunBehaviourLateUpdate <<
                           ParticleSystemBeginUpdateAll
  8 PostLateUpdate  (46)   >> UpdateAllRenderers <<          ← ÇİZİM BURADA
                           ScriptRunDelayedDynamicFrameRate
                           FinishFrameRendering
                           >> TriggerEndOfFrameCallbacks <<  ← WaitForEndOfFrame
                           >> InputEndFrame <<               ← girdi SIFIRLANIYOR
```

***Bu tablo üç soruyu tek bakışta cevaplıyor:***

```
"Update'imi kim çağırıyor?"
    → PlayerLoop'un 6. fazındaki ScriptRunBehaviourUpdate yuvası.
      Adı motorun kendi ikilisinde YAZILI; uydurma değil.

"Neden LateUpdate kamera için doğru yer?"
    → 7. faz. Bütün Update'ler (6) bitmiş, çizim (8) henüz başlamamış.
      Aradaki TEK yuva orası.

"Neden Update'te okunan tıklama o kare boyunca tutarlı?"
    → Girdi 3. fazda okunuyor (UpdateInputManager), 8. fazın sonunda
      sıfırlanıyor (InputEndFrame). Senin Update'in ikisinin ARASINDA.
```

Çağrı **sırası** ve garantileri bu dosyanın işi değil — sahibi
[`konular/08` → İkinci durak](../deep/konular/08-motor-cagri-dongusu.md#ikinci-durak-cagri-sirasi-sahipleriyle-ezberle-degil).
***Buradaki katkı: o sıranın nerede yaşadığı.*** Bir ezber değil, adlandırılmış
bir ağaç.

**SEN OLSAN NE YAPARDIN.** Tek bir `while (true)` döngüsü yazardın: girdi oku,
sistemleri sırayla çağır, çiz, bekle. Motor da aynısını yapıyor. Farkı şu:
motorun 127 yuvası var, ve her yuvanın yerel bir sahibi var.

**MALİYET.** ***Bu projede ÖLÇÜLMEDİ.*** Editor'de kare zamanına bakılmadı,
Profiler açılmadı. Ölçmenin nereye ait olduğu yedinci duraktaki geçiş
listesinde ADIM 6 olarak yazılı. ***Editor ölçümü bir ITERASYON kanıtıdır,
hedef cihaz kanıtı DEĞİLDİR.***

### 3.3 · Serileştirme — `[SerializeField]` `private` bir alanı nasıl gösteriyor

**PROBLEM.** Bir sayının (tahta genişliği, hasar, bekleme süresi) kodda değil
**sahnede** yaşaması gerekir ki tasarımcı onu derleme yapmadan değiştirebilsin.
Ama o alanın kod tarafında `private` kalması gerekir ki kimse dışarıdan yazmasın.

**MOTORUN ÇÖZÜMÜ.** ***Bu işi Unity'nin KENDİ serileştiricisi yapar.*** Buradaki
en pahalı yanılgı şudur: o serileştirici, C#'ın `[Serializable]` mekanizması
**DEĞİLDİR**.

***ÖLÇÜLDÜ — yansıma sondası:***

```
typeof(UnityEngine.Vector3).IsSerializable   ►  >> False <<
typeof(UnityEngine.Color).IsSerializable     ►  >> False <<
```

İki tip de `[System.Serializable]` **taşımıyor**. Yine de ikisi de Inspector'da
görünür, ikisi de sahne dosyasına yazılır. ***İki mekanizmanın ayrı olduğunun
tek satırlık kanıtı budur.***

Sahne dosyası aynı şeyi öbür uçtan gösteriyor:

```
Assets/Scenes/SampleScene.unity
--- !u!114 &1675776205
MonoBehaviour:
  m_GameObject: {fileID: 1675776204}
  m_Enabled: 1
  m_Script: {fileID: 11500000, guid: 99975536c95574b4c9004444d6bc33a6, type: 3}
  width: 3                      ◄── private alan, ama DOSYADA
  height: 5                     ◄── private alan, ama DOSYADA
  terrainSprites: [4 sprite]
  unitPrefab: {fileID: 220021581834759902, guid: eccbfd...}
```

`width` ve `height` C# tarafında `private`:

```
BoardAdapter.cs:113   [SerializeField, Min(1)] private int width = 3;
BoardAdapter.cs:114   [SerializeField, Min(1)] private int height = 5;
```

***Erişim belirteci C# derleyicisinin kuralıdır. Unity'nin serileştiricisi bir
C# çağrı yolu değildir; alanı **doğrudan** okur ve yazar.*** Aynı sebep, bir
`private void Awake()`'in neden çağrıldığını da açıklıyor. İkisi de aynı sınırın
iki yüzüdür.

*****SERİLEŞTİRİCİNİN DESTEKLEMEDİKLERİ — ve bu projedeki doğrudan sonucu*****

Unity'nin serileştiricisi `null` referansları ve `Dictionary<,>` gibi tipleri
desteklemez. Bunun bu projedeki karşılığı ölçülebilir:

```
Assets/Game/Battle/Battle.cs:81
private readonly Dictionary<Unit, Action<UnitState, UnitState>> stateForwarders =
```

***Bu alan üç ayrı sebeple serileştirilemez:***
(1) bir `Dictionary<,>`, (2) değeri bir `Action<,>` yani delege,
(3) `readonly`. Ve zaten hiç denenmemiş — çünkü `Battle` bir `MonoBehaviour`
değil ve o assembly `UnityEngine.dll`'i hiç görmüyor
(`noEngineReferences: true`). ***Yani bu bir eksiklik değil, duvarın ücretsiz
verdiği bir bağışıklık.*** `stateForwarders`'ın ne işe yaradığı ve neden bir
sözlük olmak zorunda olduğu burada tekrar edilmiyor — sahibi
[`dil/07` → `Battle.stateForwarders`](../deep/dil/07-bellek-canlilik-ve-yikim.md#bu-projedeki-canli-ornek-battlestateforwarders-ve-okun-yonu).

*****SERİLEŞTİRME BİR ANLIK GÖRÜNTÜDÜR — bu projede CANLI ÖLÇÜM*****

Şimdi say. `BoardAdapter` üstünde ***13*** adet `[SerializeField]` var.
Sahne dosyasında yazılı olan ***4*** tane.

```
SAHNEDE VAR (4)     : width · height · terrainSprites · unitPrefab
SAHNEDE YOK (9)     : maxHealth · damage · attackRange · moveRange ·
                      placementGhost · dragThreshold · placementModeKey ·
                      placementCancelKey · structureMaxHealth

>> SAYIM TUZAĞI — bu dosya ona düştü ve düzeltti: <<
    grep -c "SerializeField" BoardAdapter.cs   ► 14   ◄ >> YANLIŞ <<
    çünkü BoardAdapter.cs:126 bir YORUM ve içinde o kelime geçiyor.
    grep -cE "^\s*\[SerializeField" ...      ► 13   ◄ doğru sayı
Aynı tuzak UnitView'da da var (UnitView.cs:69 bir yorum): 4 değil >> 3 <<.

Dosya tarihleri:
    Assets/Scenes/SampleScene.unity        2026-08-16 23:28
    Assets/Game/Prefabs/Unit.prefab        2026-08-17 02:11
    Assets/Game/Unity/BoardAdapter.cs      >> 2026-08-23 14:23 <<
    Assets/Game/Unity/UnitView.cs          >> 2026-08-23 14:23 <<
```

Aynısı prefab'da: `UnitView` üstünde 3 `[SerializeField]` var, prefab'da
yalnız ***1*** tane yazılı (`selectionOverlay`); `downedTint` ve `deadTint`
dosyada **hiç geçmiyor**.

```
UnitView.cs:51   [SerializeField] private SpriteRenderer selectionOverlay;
UnitView.cs:59   [SerializeField] private Color downedTint = new Color(1f, 1f, 1f, 0.45f);
UnitView.cs:66   [SerializeField] private Color deadTint = new Color(0.35f, 0.35f, 0.38f, 1f);
```

*****DOĞRULANMADI:***** Unity bu sahneyi açtığında eksik dokuz alana ne yazdığı bu
turda **Editor koşturulmadığı için ölçülmedi**. İki okuma da mümkündür, ve
aralarındaki ayrım Inspector'da bir bakışta görülür. Yedinci duraktaki
***ADIM 4*** tam olarak bu soruyu ölçüyor. ***ÖLÇÜLEN olgu tektir: dosyada o
anahtarlar YOK.***

**SEN OLSAN NE YAPARDIN.** Bir JSON dosyası yazar, `private` alanları
`Newtonsoft` ile okurdun. Farkı: Unity'nin serileştiricisi Editor'ün kendi
düzenleme yüzeyiyle (Inspector), önizlemeyle ve prefab devralma zinciriyle
bütünleşik. JSON'da o üçü yok.

**MALİYET.** Serileştirilmiş her alan bir sahne/prefab anahtarıdır ve
şemayı değiştirdiğin gün eski dosyalar **sessizce** eski kalır. Yukarıdaki
ölçüm bunun canlı örneği. ***Yazım maliyeti ölçülmedi; şema kayması maliyeti
ÖLÇÜLDÜ (13'e 4 ve 3'e 1).***

### 3.4 · Varlık veritabanı ve `.meta` / GUID

**PROBLEM.** `BoardAdapter.cs`'i başka bir klasöre taşırsan sahnedeki bileşen
hangi betiği kullanacağını nereden bilecek? Yolu yazsaydın taşıma her seferinde
kırardı.

**MOTORUN ÇÖZÜMÜ.** ***Bir dosyanın kimliği yolu değil GUID'idir.*** Her varlığın
yanında bir `.meta` durur ve GUID orada yaşar. Sahne/prefab **yolu değil GUID'i**
yazar.

***ÖLÇÜLDÜ — iki uç yan yana:***

```
Assets/Game/Unity/BoardAdapter.cs.meta
    guid: 99975536c95574b4c9004444d6bc33a6
Assets/Scenes/SampleScene.unity
    m_Script: {fileID: 11500000, guid: 99975536c95574b4c9004444d6bc33a6, type: 3}
                                       ▲ >> AYNI GUID <<

Assets/Game/Unity/UnitView.cs.meta
    guid: a11f6ccd21d97e54b997c4f13a8260a9
Assets/Game/Prefabs/Unit.prefab
    m_Script: {fileID: 11500000, guid: a11f6ccd21d97e54b997c4f13a8260a9, type: 3}
                                       ▲ >> AYNI GUID <<

Assets/ altındaki toplam .meta sayısı : 114
```

`.meta` içinde başka bir şey daha var ve adı önemli:

```
Assets/Game/Unity/BoardAdapter.cs.meta
    MonoImporter:
      executionOrder: 0        ◄── Script Execution Order BURADA yaşar
```

***Yani `[DefaultExecutionOrder]`'ın ve Editor'deki "Script Execution Order"
penceresinin sakladığı yer bu satırdır.*** Bu projede o satırın değeri `0`.
Yani sıra zorlanmamış. Bunun neden bir tercih olduğu:
[`konular/08` → Kaçış yolu](../deep/konular/08-motor-cagri-dongusu.md#kacis-yolu-bu-donguden-nasil-kacilirdi).

`.meta` üretiminin **kendi** ölçüsü — `Assets/` altındaki bir `.md`'nin bile
GUID alması, `Docs/` altındakinin almaması — bu dosyanın işi değil, zaten
ölçülmüş: [`deep/README.md` → Neden `Docs/` altında](../deep/README.md#neden-docs-altinda-assets-altinda-degil).

**SEN OLSAN NE YAPARDIN.** Referansları yolla yazar, taşıma günü bir "yeniden
bağla" betiği koşturur ve unuttuğun gün sessizce kırardın. GUID bu betiği
gereksiz kılıyor.

**MALİYET.** ***Her varlık bir GUID sahibi olur ve `.meta` dosyası varlıkla
BİRLİKTE taşınmak zorundadır.*** `.meta` olmadan taşınan bir dosya yeni bir GUID
alır ve ona işaret eden her referans kopar. Sürüm kontrolünde `.meta`
dosyalarını dışlamak bu yüzden bir hatadır. Bu projede dışlanmamış (114 `.meta`
depoda duruyor).

### 3.5 · Çizim (rendering) — ***YALNIZ SINIR***

**Bu projede yalnız `SpriteRenderer` var.** Sayıldı:

```
Assets/Game/ altında `SpriteRenderer` geçen dosya sayısı : 2
    BoardAdapter.cs : 7 satır      UnitView.cs : 10 satır
Öteki 31 üretim dosyasında         : >> SIFIR <<
Mesh · Material · Shader · Camera efekti · katman/aşama ayarı : >> SIFIR <<
```

***Motorun çizim tarafı BU DOSYADA ANLATILMIYOR.*** Anlatılmayanlar şunlar:
kamera kırpması, gruplama (batching), çizim çağrısı, sıralama katmanları,
malzeme örnekleri, gölgelendirici derlemesi.

```
>> HENÜZ YOK << → hangi aşamada gelir:
   ① İkinci bir çizici türü doğduğu gün (metin, çizgi, parçacık).
   ② Aynı anda ekranda yüzlerce çizici olduğu ve kare zamanı ÖLÇÜLEREK
      bozulduğu gün — gruplama sorusu ancak o gün gerçek olur.
   ③ Bir malzeme/gölgelendirici kararı verildiği gün (bugün hiç yok).
Bugün alınan tek çizim kararı: sortingOrder — zemin 0, birimler 1:
BoardAdapter.cs:690   renderer.sortingOrder = 0;
BoardAdapter.cs:576   renderer.sortingOrder = 1;
```

***Bu bir eksiklik değil, bir yokluk.*** Eksiklik yapılması gerekip
yapılmayandır; yokluk henüz basıncı doğmamış olandır.

---

## Dördüncü durak: ***"Unity'nin event fonksiyonları NEDEN, NASIL tanımlanmış"***

### ① ÖNCE ÜÇ ÖLÇÜM — ne DEĞİL olduğu

Yerel `UnityEngine.CoreModule.dll` üzerinde yansımayla:

```
typeof(MonoBehaviour).GetMethod("Awake",  hepsi)      ►  >> TANIMLI DEĞİL <<
typeof(MonoBehaviour).GetMethod("Start",  hepsi)      ►  >> TANIMLI DEĞİL <<
typeof(MonoBehaviour).GetMethod("Update", hepsi)      ►  >> TANIMLI DEĞİL <<
typeof(MonoBehaviour).GetMethod("OnEnable", hepsi)    ►  >> TANIMLI DEĞİL <<
typeof(MonoBehaviour).GetMethod("OnDisable", hepsi)   ►  >> TANIMLI DEĞİL <<
typeof(MonoBehaviour).GetMethod("OnDestroy", hepsi)   ►  >> TANIMLI DEĞİL <<
typeof(MonoBehaviour).GetMethod("LateUpdate", hepsi)  ►  >> TANIMLI DEĞİL <<
typeof(MonoBehaviour).GetMethod("FixedUpdate", hepsi) ►  >> TANIMLI DEĞİL <<
        (BindingFlags: Public|NonPublic|Instance|Static|FlattenHierarchy —
         yani bütün kalıtım zinciri tarandı)

typeof(MonoBehaviour) bildirdiği metot sayısı         ►  24
    bunlardan `virtual` olan                          ►  >> 0 <<
typeof(MonoBehaviour).GetInterfaces().Length          ►  >> 0 <<
bütün zincirdeki `event` üye sayısı                   ►  >> 0 <<

Kalıtım zinciri (ölçüldü):
    MonoBehaviour → Behaviour → Component → Object → System.Object
```

Üç cümle, üçü de ölçülmüş:

```
>> Bir INTERFACE uygulaması DEĞİL <<   MonoBehaviour 0 arayüz uyguluyor
>> Bir VIRTUAL ezme DEĞİL <<           MonoBehaviour 0 virtual metot bildiriyor
>> Bir C# event abonesi DEĞİL <<       bütün zincirde 0 event üyesi var
```

`event` ile mesaj geri çağrısının satır satır farkı burada **tekrar edilmiyor**.
O tabloyu [`konular/08` → Birinci durak](../deep/konular/08-motor-cagri-dongusu.md#birinci-durak-awake-bir-event-degildir)
zaten kuruyor. ***Buradaki katkı: o tablonun makine tarafındaki kanıtı.***

### ② PEKİ NASIL — ***AD TABANLI ÇÖZÜM***

```
① SEN YAZARSIN
   BoardAdapter.cs:232   private void Awake()
   Derleyici için bu SIRADAN bir örnek metodu. `Awake` adının hiçbir
   özel anlamı yok. Ortada override yok, arayüz yok, çağıran satır yok.
        v
② MOTOR BİR AD KATALOĞU TUTAR
   "Awake", "OnEnable", "Start", "Update", "LateUpdate", "FixedUpdate",
   "OnDestroy", "OnValidate", "OnMouseDown", ... — her adın ne anlama
   geldiği ve NE ZAMAN çağrılacağı yerel tarafta yazılı.
        v
③ EŞLEŞME
   Tip MonoBehaviour'dan türüyorsa, motor o tipin ADLARINA bakar,
   kataloğuyla eşleştirir ve eşleşenleri kendi çağrı listelerine yazar.
   >> private olması hiçbir şeyi değiştirmez << — motorun çağrı yolu
   C# çağrı yolu değildir; serileştiricinin `private` alanı okuması
   ile TAM OLARAK AYNI SEBEP (3.3'e bak).
```

### ③ ***NEDEN BÖYLE TASARLANMIŞ — iki alternatif ve ikisinin faturası***

**REDDEDİLEN A — taban sınıfta `virtual` metotlar.**
`MonoBehaviour` üstünde `virtual void Awake()`, `virtual void Update()`, ... diye
100+ metot bildirilseydi:

```
KIRILAN:
  · Her MonoBehaviour türevi o sanal metot tablosunu (vtable) TAŞIRDI.
  · Ezmediğin metotlar bile tabloda yer tutardı.
  · >> Ve asıl bedel: motor "bu tip Update'i ezmiş mi" sorusunu ucuz
    soramaz hâle gelirdi << — taban sınıfta boş bir gövde varsa her tip
    "Update'i var" görünür ve kare başına çağrı listesine girer.
ÖLÇÜ: bugün MonoBehaviour'un bildirdiği virtual metot sayısı >> 0 <<.
      Tasarım bu maliyeti hiç doğurmamış.
```

**REDDEDİLEN B — arayüzler (`IAwake`, `IUpdatable`, ...).**

```
KIRILAN:
  · Bir arayüzü uygulamak BÜTÜN üyelerini yazmayı gerektirir.
    `IUpdatable` alsaydın `Update` yazmak ZORUNDA kalırdın.
  · Ya da her geri çağrı için ayrı bir arayüz — 100+ arayüz.
  · `UnitView` bugün `Update` YAZMIYOR ve yazmak zorunda da değil
    (ölçü: o dosyada `Time` kelimesi hiç geçmez). Arayüz olsaydı ya
    boş bir `Update` yazardı ya ayrı bir arayüz seçerdi.
ÖLÇÜ: bugün MonoBehaviour'un uyguladığı arayüz sayısı >> 0 <<.
```

***AD TABANLI ÇÖZÜM İKİSİNDEN DE KAÇINIR:*** yazmadığın geri çağrının hiçbir
maliyeti yoktur — ne tabloda yer tutar, ne seni bir üye yazmaya zorlar. Motor
yalnız **gerçekten tanımladıklarını** bulur.

Bu projedeki ölçüsü: iki `MonoBehaviour`, **beş** geri çağrı tanımlı.
`Start`, `LateUpdate`, `FixedUpdate`, `OnDestroy`, `OnValidate` — hiçbiri yok ve
hiçbirinin **yokluğu bir maliyet üretmiyor**. Tam sayım ve her birinin neden
gerekmediği: [`konular/08` → Tanımlı OLMAYANLAR](../deep/konular/08-motor-cagri-dongusu.md#tanimli-olmayanlar-ve-neden-gerekmemis).

### ④ ***BEDEL: YAZIM HATASI SESSİZ KALIR — İŞLENMİŞ ÖRNEK***

Bu, tasarımın **en önemli sonucudur** ve bir kez yaşamadan öğrenilmiyor.
Aşağıdaki dosya yerel `csc.exe` ile, **en yüksek uyarı seviyesinde**
(`-warn:4`) derlendi:

```csharp
using UnityEngine;
public class AwakeeProbe : MonoBehaviour
{
    private void Awakee() { Debug.Log("hic cagrilmam"); }
}
```

***ÖLÇÜLEN SONUÇ:***

```
derleyici çıktısı        :  >> TEK SATIR YOK <<  (0 hata, 0 uyarı)
üretilen ikili           :  awakee.dll — 3.072 bayt
çıkış kodu               :  0
```

***Derleyici hiçbir şey söylemedi. Motor da hiçbir şey söylemeyecek.***
Metot orada duracak, derlenecek, ikiliye girecek ve ***hiçbir zaman
çağrılmayacak***.

Şimdi bunu bu projeye uygula:

```
BoardAdapter.cs:238   battle = new Battle(width, height);
```

Bu satır `Awake` içinde. Adı `Awakee` yapsan:

```
derleme      : >> TEMİZ <<        testler : >> YEŞİL <<   (EditMode Awake'i hiç görmez)
Play'e bas   : `battle` sonsuza dek null
ilk kare     : Update'in ilk satırı → NullReferenceException
sen ne dersin: "Battle'da bir hata var"   ← >> YANLIŞ YERE BAKIYORSUN <<
```

***Karşı sınama, aynı repoda ve bilerek orada:*** tipi bozarsan
(`: MonoBehaviour` satırını silersen) derleyici `GetComponent`'te **patlar** ve
Play'e hiç basamazsın. ***İlk hata sessiz, ikincisi gürültülü; tehlikeli olan
sessiz olandır.*** Aynı gözlemin sıra tarafındaki hâli
[`konular/08` → İŞ BÖLÜMÜ](../deep/konular/08-motor-cagri-dongusu.md#is-bolumu-adin-iki-sahibi-ortusmez-bolusur)
içinde yazılı; buradaki katkı, o cümlenin ***derleyici koşturularak ölçülmüş
hâli***.

**KENDİNİ KORUMA YOLU — bugün elde olan.** Bir geri çağrı yazdığında adını
**IDE'nin tamamlamasından** seç, elle yazma. Rider ve Visual Studio bilinen
Unity mesaj adlarını tanır. Bu bir garanti değil, bir alışkanlıktır. Bugün bu
projede geri çağrıların **beşi de** doğru yazılmış (ölçü: beşi de gerçekten
koşuyor, aksi hâlde oyun ilk karede patlardı).

### ⑤ ***Motor bunu her karede YANSIMAYLA mı çözüyor — DOĞRULANMADI***

```
DOĞRULANABİLEN : Motorun çağrı yolu C#'ın erişim kurallarına tabi değil
                 (5 geri çağrının 5'i de `private` ve 5'i de koşuyor).
DOĞRULANABİLEN : MonoBehaviour zincirinde bu adlardan hiçbiri tanımlı değil,
                 yani çözüm kalıtımla YAPILMIYOR.
>> DOĞRULANMADI << :
   · adın ne zaman çözüldüğü (derleme anında mı, tip ilk yüklendiğinde mi,
     her karede mi)
   · sonucun önbelleğe alınıp alınmadığı
   · IL2CPP'de şeklin değişip değişmediği
SEBEP: bu bilgi yerel kurulumun YÖNETİLEN ikililerinde yok; yerel taraf C++
       ve kaynağı elimde değil. Yerel `Managed/UnityEditor.xml` ve
       `Managed/UnityEngine/*.xml` belge dosyalarında da geçmiyor.
>> Bu yüzden bu dosya "her karede yansıma yapılıyor" DA demiyor,
   "derleme anında çözülüyor" DA demiyor. <<
```

***İddia edilmeyen bir şeyi öğretmek, yanlış öğretmekten daha ucuzdur.***
Kararını değiştiren tek olgu zaten ölçüldü: **yazım hatası sessizdir.**

---

## Beşinci durak: Mono ve IL2CPP — iki arka uç

### ① BU PROJE HANGİSİNİ KULLANIYOR — ölçüldü

```
ProjectSettings/ProjectSettings.asset (2026-08-23)
  satır 674   scriptingBackend: {}          ◄── >> BOŞ SÖZLÜK <<
  satır 675   il2cppCompilerConfiguration: {}
  satır 690   incrementalIl2cppBuild: {}
  satır 695   additionalIl2CppArgs:         (boş)
  satır 771   apiCompatibilityLevel: 6
  satır 676   managedStrippingLevel:
                EmbeddedLinux · GameCoreScarlett · GameCoreXboxOne · Lumin ·
                Nintendo Switch · PS4 · PS5 · Stadia · WebGL ·
                Windows Store Apps · XboxOne · iPhone · tvOS
                >> 13 platform yazılı — Standalone (Windows/Mac/Linux) YOK <<
  satır 692   allowUnsafeCode: 0
  satır 693   useDeterministicCompilation: 1
  satır 694   enableRoslynAnalyzers: 1
  satır 697   gcIncremental: 1
```

*****ÖLÇÜLEN OLGU:** `scriptingBackend: {}` boş — yani HİÇBİR platform için
açık bir arka uç değeri yazılmamış.*** Proje hiç dokunmamış; her platform kendi
varsayılanını kullanır.

*****DOĞRULANMADI:***** "Bu projenin hedef platformundaki varsayılan
`Mono2x`'tir" cümlesi **yazılmıyor**, çünkü varsayılanın ne olduğu Editor'ün
kendi kodunda yaşıyor ve bu turda Editor açılmadı. Ayrıca aktif hedef platform
`Library/EditorUserBuildSettings.asset` içinde, ve o dosya **ikili**. Metin
olarak okunamadı.

*****DOĞRULANABİLEN İKİ ŞEY:*****

```
① Editor'ün KENDİSİ Mono ile koşar. Ölçü: bu dosyadaki bütün yansıma
   sondaları Editor'ün kendi Mono çalıştırıcısıyla (MonoBleedingEdge/bin/
   mono.exe) koşturuldu ve çalıştı. IL2CPP diye bir şey burada yok.
② IL2CPP bu projede HİÇ YAPILANDIRILMAMIŞ: il2cppCompilerConfiguration boş,
   additionalIl2CppArgs boş, incrementalIl2cppBuild boş.
```

### ② FARKI — ve bu projede neden HENÜZ ölçülemez

```
MONO                                 IL2CPP
────                                 ──────
C# ─► IL ─► >> JIT << ─► makine      C# ─► IL ─► >> C++ << ─► yerel derleyici
     çalışma anında çevrilir              yapı anında çevrilir (AOT)

yansıma       : geniş                yansıma  : >> KIRPILABİLİR <<
`dynamic`     : var                  `dynamic`: >> YOK <<
kod kirpma    : yok/az               kirpma   : varsayılan olarak AÇIK
yapı süresi   : kısa                 yapı     : uzun
```

*****ASIL TUZAK — ve rule K43'ün tam yeri:*****

```
"IL2CPP bu projede bugün önemli değil"  ← >> EKSİK CÜMLE <<

TAM HÂLİ: Editor'de ve Mono'da çalışan bir kod, IL2CPP + kirpma altında
SESSİZCE bozulabilir — çünkü kirpici (linker) "hiçbir yerden çağrılmayan"
tipleri atar ve YANSIMAYLA erişilen bir tip ona çağrılmıyor GÖRÜNÜR.

>> NE ZAMAN ÖNEMLİ HÂLE GELİR — üç somut kapı: <<
  ① İlk hedef cihaz yapısı alındığı gün (mobil, konsol, WebGL —
     managedStrippingLevel bu 13 platform için ZATEN 1 yazılı).
  ② Yansımaya ya da ada göre tip çözmeye dayanan İLK kod yazıldığı gün
     (JSON okuyucu, eklenti sistemi, `Type.GetType(string)`).
  ③ Bir üçüncü taraf paketi yansıma kullandığı gün.
BUGÜN NEDEN GÜVENDE: üretim kodunda `System.Reflection` hiç geçmiyor ve
tip çözümü tamamen derleme zamanında yapılıyor.
```

*****BU PROJEDE ÖLÇÜM YOK.***** Hiçbir yapı alınmadı, hiçbir cihazda hiçbir şey
koşturulmadı. "Mono daha hızlı" ya da "IL2CPP daha hızlı" cümlelerinden hiçbiri
bu dosyada kurulmuyor.

Derleyici tarafı — `yield return`'ün ne ürettiği, `async`'in makinesi, adın
kendisinin bir sözleşme olmadığı — bu dosyanın işi değil, ölçülmüş sahibi:
[`05-yok-olan-mekanizmalar-csharp.md`](05-yok-olan-mekanizmalar-csharp.md).
Aynı dosya `Awaitable`'ın bu sürümde **var olmadığını** da ölçmüş:
[`05` → `Awaitable` bu sürümde var mı](05-yok-olan-mekanizmalar-csharp.md#awaitable-bu-surumde-var-mi-olculdu).

---

## Altıncı durak: üç oyun, tek soru

**Soru:** *"Motorun hangi hizmeti o oyunun mimarisini en çok belirliyor?"*

*****DOĞRULANMA SINIRI — önce bunu oku:***** Aşağıdaki üç dış oyunun kaynağı
kapalı ve bu turda hiçbiri resmî belgeye ya da koda karşı doğrulanmadı.
***Üç satır da DOĞRULANMAMIŞ genel oyun bilgisidir ve öyle okunmalıdır.***
Yalnız son iki satır bu repoya karşı ölçüldü.

| Oyun | Mimarisini en çok belirleyen hizmet | O hizmetin işi |
|---|---|---|
| **Slay the Spire** ██ DOĞRULANMADI ██ | ██ EŞLEŞMİYOR ██ — Unity kullanmıyor; libGDX üstünde Java ile yazıldığı biliniyor | Kare akışı oyunun mimarisini **belirlemiyor**; iş kart oynandığında doğuyor ve bir eylem sırası hâlinde boşalıyor. Motorun sağladığı şey çizim ve girdi; kural tarafı motordan bağımsız |
| **Vampire Survivors** ██ DOĞRULANMADI ██ | Kare döngüsü ve toplu çarpışma sorgusu | Yüzlerce gövde her kare yaklaşır, her silahın sayacı iner, kim kime değiyor diye bakılır. Kare başına iş **nesne sayısıyla büyür**; mimarinin tamamı bu büyümenin üstüne kurulu |
| **Stardew Valley** ██ DOĞRULANMADI ██ | ██ EŞLEŞMİYOR ██ — Unity kullanmıyor; MonoGame/XNA üstünde yazıldığı biliniyor. `Game1` sınıfının güncelleme metodu akışı yürütüyor | Serileştirme ve varlık kimliği tarafında Unity'nin `.meta`/GUID'ine karşılık gelen bir şey **yok**; kayıt dosyası ve içerik yükleme elle yazılmış |
| **CountryBall (bu proje)** ✅ ÖLÇÜLDÜ | ██ Serileştirme ██ (`[SerializeField]`) ve ██ ad tabanlı geri çağrı ██ | 13 + 3 = 16 serileştirilmiş alan sahnede/prefab'da yaşıyor; 5 geri çağrı motoru kodla buluşturuyor. Üçüncü hizmet `Transform` hiyerarşisi (2 seviye). Çizim tarafı yalnız `SpriteRenderer` |
| ██ KARŞILIĞI OLMAYAN SATIR ██ | Fizik ve çarpışma | ██ HENÜZ YOK ██ → Vampire Survivors satırının belirleyici hizmeti bu projede **hiç yok**: `Rigidbody`, `Collider`, `OnTrigger*` → sıfır. Yaratacağı aşama: birimlerin hücre değil **serbest** konumda hareket ettiği gün |

*****EN ÖĞRETİCİ SATIR BİRİNCİDİR***** ve iki kez öyle: (a) o oyun Unity bile
kullanmıyor, yani "hangi Unity hizmeti" sorusunun **karşılığı yok**; <!-- YOK-MUAF · KAPSAM DIŞI: yokluk BU projede değil, karşılaştırılan başka bir oyunda. -->
(b) kare başına yapılacak oyun işi neredeyse **sıfır**. Bu bir eksiklik değil
bir **tür farkıdır**. Sıra tabanlı bir oyunda iş olaya bağlıdır, ve kare yalnız
çizim için döner. Bu proje ikisinin **arasında**: kararı olay veriyor (tıklama),
zamanı kare taşıyor (`Tick`).

Aynı üç oyunun *"kare başına ne koşuyor"* tarafı — farklı bir soru, farklı bir
tablo — [`konular/08` → Yedinci durak](../deep/konular/08-motor-cagri-dongusu.md#yedinci-durak-kare-basina-ne-kosuyor-kim-baslatiyor).

---

## Yedinci durak: ***KOD TARAFINI BİTİRİNCE UNITY TARAFINA NASIL GEÇERİM***

Sekiz adım. Her adımda dört alan: **NE YAPILIR · NEREYE TIKLANIR · GÖRÜNÜR
SONUÇ · DUR VE RAPOR**. ***Adı geçen her şeyin önce **varlık kategorisi**
yazılı*** — *proje* · *Sahne* · *GameObject* · *Bileşen* · *Inspector alanı* ·
*Proje Ayarı*; hiçbir ad tek başına verilmiyor.

**ADIM 1 · *proje*yi doğru sürümle aç**
**NE YAPILIR:** Unity Hub'ı aç; sol menü → "Projects"; listede
"CountryBall-Strategy" satırını bul; sağındaki "Editor Version" sütununda
***2021.3.45f2*** yazmalı; yazıyorsa proje adına tıkla.
**NEREYE:** Unity Hub → Projects → CountryBall-Strategy satırı.
**GÖRÜNÜR SONUÇ:** Editor açılır, altta "Importing…" çubuğu dönebilir; bitince
pencere başlığında proje adı ve sürüm görünür.
**DUR VE RAPOR:** Sütunda başka bir sürüm yazıyorsa ***AÇMA***. Farklı bir
sürümle açmak `Library/` klasörünü yeniden üretir, ve o üretimle ilk üretimin
sürümünü kaybedersin.

**ADIM 2 · *Sahne*yi aç ve iki *GameObject*'i bul**
**NE YAPILIR:** Project penceresinde (varsayılan: altta) Assets → Scenes yolunu
aç; "SampleScene" varlığına ÇİFT tıkla.
**NEREYE:** Project penceresi → `Assets/Scenes/SampleScene`.
**GÖRÜNÜR SONUÇ:** Hierarchy penceresinde (varsayılan: solda) ***tam iki*** kök
GameObject: "Main Camera" ve ***"Board"***. Ölçü: sahne dosyasında iki
GameObject var, ikisinin de `m_IsActive` değeri 1.
**DUR VE RAPOR:** Hierarchy boşsa ya da ikiden fazla nesne varsa dur. Yanlış
bir sahne açılmış olabilir.

**ADIM 3 · `Board` *GameObject*'inin *Bileşen*lerini oku**
**NE YAPILIR:** Hierarchy'de "Board" GameObject'ine TEK tıkla; Inspector
penceresine (varsayılan: sağda) bak.
**NEREYE:** Hierarchy → Board → Inspector.
**GÖRÜNÜR SONUÇ:** ***Üç Bileşen*** sırayla: `Transform` · `Grid` ·
`Board Adapter (Script)`. Sonuncusunun altında Inspector alanları: Width = 3 ·
Height = 5 · Terrain Sprites (4 eleman) · Unit Prefab.
**DUR VE RAPOR:** ***Asıl gözlem: kaç Inspector alanı görünüyor?*** Kodda 13
`[SerializeField]` var, sahne dosyasında 4 tanesi yazılı. 13'ünü de görüyorsan
Unity kalan 9'un değerini C# alan başlatıcılarından almış demektir.
***Gördüğün sayıyı NOT AL.*** O sayı, 3.3'te "DOĞRULANMADI" diye işaretlenen
tek soruyu tam olarak kapatır.

**ADIM 4 · Bir *Inspector alanı*nı değiştir ve etkisini gör**
**NE YAPILIR:** Inspector'da `Board Adapter` Bileşenindeki ***Width*** alanına
tıkla, 3'ü 6 yap, Enter'a bas; sonra araç çubuğundan ▶ (Play).
**NEREYE:** Inspector → Board Adapter (Script) → Width.
**GÖRÜNÜR SONUÇ:** Game penceresinde tahta ***6 sütun*** genişler; Console'da
tek satır: `[Board] built 6x5 = 30 cells.`
**DUR VE RAPOR:** Alan gri ve tıklanamıyorsa dur (prefab kilidi olabilir).
Değeri 0 yapamazsın, çünkü `[Min(1)]` attribute'ü engeller. ***Bunu bilerek
dene: engellenmiş bir alan, serileştirmenin doğrulama tarafını tek hamlede
gösterir.*** Bitince Play'den çık
ve 3'e döndür. ***Play SIRASINDA yapılan değişiklik çıkışta KAYBOLUR; Play
DIŞINDA yapılan KALIR.***

**ADIM 5 · Play'e bas ve `B` tuşunu dene — ***BURADA BİR ŞEY KIRILACAK*****
**NE YAPILIR:** ▶ (Play); Game penceresinde bir birime TEK tıkla (seçildiğini
çerçeveden anlarsın); klavyeden ***B***; sonra tahta içindeki BOŞ bir hücreye
tıkla.
**NEREYE:** Araç çubuğu → ▶ → Game penceresi → sol tık → B → sol tık.
**GÖRÜNÜR SONUÇ:** Console'da KIRMIZI bir satır belirir. ***Ama hangi satırın
belireceği bu turda ölçülemedi.*** İki aday var, ve aralarındaki ayrım bu adımın
asıl dersi:

```
ADAY A   [Board] Cannot enter placement mode: placementGhost is not assigned.
         → BoardAdapter.cs:369 · kip HİÇ AÇILMAZ, B tuşu hiçbir şey yapmaz
ADAY B   ArgumentException: The unit is already in this battle.
         Parameter name: unit
         → konular/07'nin ölçtüğü yol; kip AÇILIR ve bırakma karesinde patlar

>> AYIRAN ŞEY: `placementGhost` Inspector'da ATANMIŞ MI. <<
ÖLÇÜLEN : Assets/Scenes/SampleScene.unity içinde `placementGhost` anahtarı
          >> HİÇ YOK << (3.3'teki "sahnede yazılı 4 alan" ölçümü).
          Aynı sebeple Awake'te de bir uyarı bekleniyor:
BoardAdapter.cs:367   if (placementGhost == null)
>> DOĞRULANMADI << : Unity sahneyi açtığında alanın null kalıp kalmadığı.
          Editor bu turda açılmadı — ADIM 3'teki gözlem bu soruyu da kapatır.
```

**DUR VE RAPOR:** ***İKİ ADAYDAN HANGİSİNİ GÖRDÜĞÜNÜ YAZ.*** Bu, iki belgenin
farklı şey söylediği tek noktadır. Hangisinin bayat olduğunu **senin gözlemin**
belirler. Gözlemin, [`deep/README.md`](../deep/README.md)'nin ilk kuralının
koşan hâlidir: *ikisi çelişirse kod kazanır.* Aday B'yi görürsen üç daldan
ikisinin neden
sağlıklı ret döndürdüğü ölçülmüş hâliyle yazılı:
[`konular/07` → BUGÜNKÜ SINIR](../deep/konular/07-tiklamadan-eyleme.md#bugunku-sinir-bu-yol-playde-bastan-sona-kosmuyor).
***DÜZELTME BU TURUN İŞİ DEĞİL: not al, geç.***

**ADIM 6 · Motorun çağrı sırasını KENDİN ölç**
**NE YAPILIR:** Project penceresinde `Assets/Game/Unity` klasörüne sağ tık →
Create → C# Script → adı "LifecycleProbe"; içine yedi geri çağrının her biri
için tek satır `Debug.Log` yaz (Awake · OnEnable · Start · Update · LateUpdate ·
OnDisable · OnDestroy). Hierarchy'de sağ tık → Create Empty → adı "A"; betiği
Project'ten "A" nesnesinin Inspector'ına SÜRÜKLE; aynısını "B" için tekrarla; ▶.
**NEREYE:** Project → sağ tık → Create → C# Script; Hierarchy → sağ tık →
Create Empty; sürükle-bırak Inspector'a.
**GÖRÜNÜR SONUÇ:** Console'da (sıralamayı KAPATMA): `A Awake` · `B Awake` ·
`A OnEnable` · `B OnEnable` · `A Start` · `B Start`, sonra her kare Update.
**DUR VE RAPOR:** ***A'nın mı B'nin mi önce geldiği GARANTİ DEĞİLDİR.***
Sınadığın iddia "bütün `Awake`'ler bütün `Start`'lardan önce" — "A önce" değil.
Deneyin tam kurgusu ve ön koşulu (Domain Reload):
[`konular/08` → Bunu kendin ölç](../deep/konular/08-motor-cagri-dongusu.md#bunu-kendin-olc-iki-bilesen-bir-gunluk).
***DENEY BİTİNCE BETİĞİ SİL.*** O betik repoya girmemeli.

**ADIM 7 · Profiler'ı aç ve kare zamanını gör**
**NE YAPILIR:** Üst menü → Window → Analysis → Profiler; pencere açılınca ▶
(Play); sol sütundaki modüllerden "CPU Usage" satırına tıkla.
**NEREYE:** Window → Analysis → Profiler → CPU Usage.
**GÖRÜNÜR SONUÇ:** Grafiğin altında kare başına milisaniye ve "PlayerLoop" ile
başlayan bir ağaç. Ağacı aç. ***3.2'deki faz adlarını orada göreceksin:***
`Update` → `ScriptRunBehaviourUpdate` → `BoardAdapter.Update`.
**DUR VE RAPOR:** ***Buradaki sayı bir ITERASYON kanıtıdır, HEDEF CİHAZ KANITI
DEĞİLDİR.*** Editor'ün kendi yükü ölçüme karışır. "Oyun 200 FPS koşuyor" cümlesi
bu pencereden KURULMAZ; kurulabilecek tek cümle "aynı makinede, aynı sahnede,
bir değişiklikten önce ve sonra şu fark ölçüldü".

**ADIM 8 · *Sahne*yi kaydet ve `.meta` / GUID bağını gör**
**NE YAPILIR:** Play'den ÇIK; üst menü → File → Save (Ctrl+S); sonra bir metin
düzenleyicide `Assets/Scenes/SampleScene.unity` dosyasını aç ve `m_Script:`
satırını ara.
**NEREYE:** File → Save; sonra proje klasöründe `.unity` dosyası.
**GÖRÜNÜR SONUÇ:** `m_Script` satırındaki guid ***99975536c95574b4c9004444d6bc33a6***
olmalı ve `Assets/Game/Unity/BoardAdapter.cs.meta` içindeki guid ile ***AYNI***
olmalı. İkinci gözlem: kaydettikten sonra kaç `[SerializeField]` yazılı?
Kaydetmeden önce 4'tü.
**DUR VE RAPOR:** İki GUID farklıysa ***DUR***. Farklı olmaları, `.meta`
dosyasının bir yerde kaybolduğu anlamına gelir. Bu, sürüm kontrolüne ait bir
sorundur.

*****BU SEKİZ ADIMDAN SONRA NE ÖĞRENMİŞ OLURSUN*****

```
ADIM 3-4 → serileştirme GERÇEKTEN nasıl görünüyor (3.3'ün canlı hâli)
ADIM 5   → bu projenin bilinen kusuru, ve "kod kazanır" kuralının koşan hâli
ADIM 6   → çağrı sırası bir ezber değil, ölçülebilir bir olgu
ADIM 7   → PlayerLoop faz adları Profiler'da AYNEN görünüyor (3.2'nin doğrulaması)
ADIM 8   → GUID bağı bir teori değil, iki dosyada duran aynı 32 karakter
```

---

## Tek bakışta zincir

```
 .cs KAYNAK ──────────────────────────────────────────────────────┐
   BoardAdapter.cs                                                │
        │                                                          │
        ├── DERLEME (Roslyn) ──► IL + üstveri ──► yönetilen assembly
        │      >> `Awake` adının burada HİÇBİR özel anlamı YOK <<  │
        │                                                          │
        └── İÇE AKTARMA (Editor) ──► .meta + GUID                  │
               99975536c95574b4c9004444d6bc33a6                    │
                     │                                             │
                     ▼                                             │
        SampleScene.unity → m_Script: {guid: 9997...}              │
               + serileştirilmiş 4 alan (kodda 13 tane var)        │
                     │                                             │
                     ▼                                             │
 >> YEREL MOTOR (C++) << ◄─────────────────────────────────────────┘
   sahneyi kurar · bileşen örneğini doğurur · ADI kataloğuyla eşleştirir
        │
        ├─► YÖNETİLEN SARMALAYICI doğar
        │     UnityEngine.Object   IntPtr m_CachedPtr ──► yerel nesne
        │     GameObject/Component/Behaviour/MonoBehaviour: >> 0 ALAN <<
        │
        ├─► PlayerLoop döner — 8 faz, 127 alt sistem
        │     EarlyUpdate.UpdateInputManager        girdi okunur
        │     Update.ScriptRunBehaviourUpdate  ───► BoardAdapter.Update
        │     PreLateUpdate.ScriptRunBehaviourLateUpdate   (bu projede YOK)
        │     PostLateUpdate.UpdateAllRenderers     çizim
        │     PostLateUpdate.InputEndFrame          girdi sıfırlanır
        │
        └─► API ÇAĞRISI: transform.position
              get_position (10 bayt IL) ──► get_position_Injected
                                             >> internalcall — gövde BOŞ <<
                                                    │
                                                    ▼
              Vector3 (12 bayt, struct, sequential) ◄── yerel `Vector3f`
                     │
                     ▼
              >> DUVAR << noEngineReferences: true
                     │  duvardan geçen: int x, int y, float deltaSeconds
                     ▼
              Battle · UnitLifecycle  (Vector3 YOK · MonoBehaviour YOK)
```

---

## Kural: bir Unity API'siyle karşılaştığında ne sorarsın

Sırayla sor, ilk "evet"te dur:

```
① Bu tip UnityEngine.Object'ten türüyor mu?
   EVET → >> İKİ ÖMRÜ VARDIR. << `Destroy` sonrası `== null` der ama
          yönetilen kutu durur. Sahibi: dil/07 → Dördüncü durak.
          `new` ile kurmayı deneme (MonoBehaviour kurulamaz).

② Bu bir ÖZELLİK mi (property), bir ALAN mı?
   EVET/ÖZELLİK → >> Her okuma bir ÇAĞRIDIR ve dönüş bir KOPYADIR. <<
          Değer tipi dönüyorsa parçasına yazamazsın (>> CS1612 <<).
          Ölçmek istersen: typeof(T).GetField("ad") null mu?

③ Bu bir `struct` mu, `class` mı?
   struct → >> değer semantiği: `a = b` KOPYALAR. << Ama "nerede durduğu"
          tipin değil SARANIN sorusudur — dil/07'ye bak, cümleyi orada bitir.

④ Bu üye MOTOR TARAFINA mı geçiyor?
   ÖLÇÜSÜ : yansımayla GetMethodImplementationFlags() → InternalCall mı?
          >> Geçiyor olması TEK BAŞINA bir performans problemi DEĞİLDİR. <<
          Karar ancak ölçülmüş bir kare zamanıyla değişir.

⑤ Bu bir GERİ ÇAĞRI adı mı (Awake/Update/OnEnable/...)?
   EVET → >> ADI ELLE YAZMA, tamamlamadan seç. << Yazım hatası SESSİZDİR:
          ölçüldü — `void Awakee()` -warn:4 ile 0 uyarı verip derlendi.
          Sırası ve garantileri: konular/08 → İkinci durak.

⑥ Bu alanı Inspector'da mı yaşatacağım?
   EVET → [SerializeField] + private. >> Ama: serileştirme bir ANLIK
          GÖRÜNTÜDÜR. << Yeni alan eklediğin gün eski sahne/prefab onu
          taşımaz. Ölçüldü: bu projede 13 alanın 4'ü, 3 alanın 1'i yazılı.
          `Dictionary` ve `null` referans serileştirilmez.

⑦ Bir varlığı taşıyacak mıyım?
   EVET → >> `.meta` dosyasını BİRLİKTE taşı. << Kimlik yolda değil GUID'de.
          Ölçü: Assets/ altında 114 `.meta` var ve hepsi depoda.
```

---

## Yanlış hatırlanan üç şey

**1. ***"`GameObject` bir C# nesnesidir."*****
***`GameObject` gerçekten bir C# nesnesidir, ama içi BOŞTUR.*** Ve bu tek olgu,
cümleyi işe yaramaz kılmaya yeter.
Ölçüldü: `GameObject`, `Component`, `Behaviour` ve `MonoBehaviour` tiplerinin
**bildirdiği örnek alan sayısı dördünde de SIFIR**. Bütün zincirdeki tek gerçek
veri `UnityEngine.Object.m_CachedPtr` — bir `IntPtr`, yani yerel nesnenin adresi.
***Pratik zararı üç kalem:*** (a) `Destroy` sonrası nesnenin neden hâlâ "orada"
göründüğünü açıklayamazsın; (b) `GameObject`'i sıradan bir C# nesnesi gibi
serileştirmeye ya da klonlamaya kalkarsın; (c) `== null` ile
`ReferenceEquals`'ın neden farklı cevap verdiğini bir dil kaprisi sanırsın —
oysa iki ayrı ömrün gözlenebilir izidir.

**2. ***"`Awake` bir `event`'tir."*****
***`Awake` bir `event` değildir, ve bunun üç ayrı ölçüsü var.*** `MonoBehaviour`
zincirinde `event` üye sayısı **0**. `Awake` adında bir metot zincirde **hiç
tanımlı değil**. `MonoBehaviour` **0** arayüz uyguluyor ve **0** `virtual` metot
bildiriyor.
Motor onu **ada göre** buluyor. ***Faturası: `Awake` adını bozmak bir derleme
hatası değil, SESSİZ bir çöküş üretir.*** Ölçüldü: `void Awakee()` en yüksek
uyarı seviyesinde (`-warn:4`) 0 uyarıyla derlendi ve 3.072 baytlık bir DLL
üretti. Aynı yanılgının sıra ve abonelik tarafı:
[`konular/08` → Yanlış hatırlanan üç şey](../deep/konular/08-motor-cagri-dongusu.md#yanlis-hatirlanan-uc-sey).

**3. ***"`[SerializeField]` C#'ın `[Serializable]` mekanizmasını kullanır."*****
***`[SerializeField]` o mekanizmayı kullanmaz, ve bunun tek satırlık bir ölçüsü
var.*** `typeof(Vector3).IsSerializable` → **False**. `typeof(Color).IsSerializable` →
**False**. İki tip de `[System.Serializable]` taşımıyor, ama ikisi de
Inspector'da görünüyor ve sahne dosyasına yazılıyor. Unity'nin kendi
serileştiricisi ayrı bir mekanizmadır; `Dictionary<,>` ve `null` referansları
desteklemez. ***Pratik zararı:*** "neden `Dictionary`'m Inspector'da
görünmüyor" sorusuna bir C# cevabı ararsın. Ama cevap C#'ta değil.

---

## Kaçış yolu: bu altyapıdan nasıl kaçılırdı

**① Motoru hiç kullanmamak — saf C# ile yazmak.** Bu bir kaçış değil, bu
projenin zaten yaptığı şeyin abartılmış hâlidir: `Battle`, `UnitLifecycle` ve
`AttackRules` şu anda **motorsuz** yaşıyor ve EditMode'da sahnesiz sınanıyor.
Sınırın faturaları
[`konular/02-assembly-duvari.md`](../deep/konular/02-assembly-duvari.md)
içinde dört kalem hâlinde yazılı. ***Ama çizim, girdi ve kare bir yerden gelmek
zorunda.*** O yer motordur, ve bu projede iki dosyaya sıkışmış.

**② `[SerializeField]` yerine kendi yapılandırma dosyanı okumak.** JSON ya da
`ScriptableObject`. ***HENÜZ YOK*** → aynı sayı kümesinin **birden fazla**
varyantı gerektiği gün (kolay/zor mod, iki farklı birim türü). Bugün tek bir
`Board` var ve tek bir prefab; ikinci varyant doğmadan bu tartışma kurulmaz.
`ScriptableObject`'in altı alanlı tam kaydı zaten yazılı:
[`04-yok-olan-mekanizmalar-unity.md` → 4 · `ScriptableObject`](04-yok-olan-mekanizmalar-unity.md#4-scriptableobject).

**③ Kendi kare akışını yazmak — `PlayerLoop`'a elle sistem eklemek.**
`UnityEngine.LowLevel.PlayerLoop` bu sürümde **var** ve gerçekten
değiştirilebilir (ölçüldü: `PlayerLoopSystem` tipinin `subSystemList` ve
`updateDelegate` alanları public). ***HENÜZ YOK*** → sistem sayısı ikiyi geçtiği
ve aralarındaki sıranın **gerçekten** önem kazandığı gün. İki `MonoBehaviour`
için tartışılmaz bile.

**Neden kaçılmadı:** kaçılacak bir şey yoktu. Motorun yüzeyi bu projede
***5 geri çağrı · 16 serileştirilmiş alan · 4 konum yazımı · 2 vektör satırı***
kadar. Geri kalan her şey duvarın öte yanında, motor diye bir şeyin varlığından
habersiz yaşıyor.

---

## ***Bu turda DOĞRULANMADI diye işaretlenenler***

Dürüstlük listesi. Bu dosyanın hiçbir yerinde bunlar iddia olarak yazılmadı.

```
① Aktif yapı hedefi ve o hedefin varsayılan betik arka ucu.
   SEBEP: Library/EditorUserBuildSettings.asset İKİLİ bir dosya; metin
          olarak okunamadı. Editor bu turda açılmadı.
   ÖLÇÜLEN YERİNE: scriptingBackend: {} — hiçbir platform için açık
          değer YAZILMAMIŞ.

② Motorun geri çağrı adını NE ZAMAN çözdüğü (derleme anı / tip yükleme /
   her kare) ve sonucu önbelleğe alıp almadığı.
   SEBEP: bilgi yönetilen ikililerde yok; yerel taraf C++ ve kaynağı yok.
          Yerel .xml belge dosyalarında da geçmiyor.

③ `transform.position` okumasının hiyerarşi boyunca iş yapıp yapmadığı.
   SEBEP: aynı — yerel uygulama görünmüyor.
   ÖLÇÜLEN YERİNE: sahnede yalnız m_LocalPosition saklanıyor ve
          localToWorldMatrix ayrı bir internalcall.

④ Unity'nin sahneyi açtığında EKSİK serileştirilmiş alanlara ne yazdığı.
   SEBEP: Editor açılmadı. ADIM 3-4 tam olarak bunu ölçüyor.
   ÖLÇÜLEN YERİNE: 13 alandan 4'ü, 3 alandan 1'i dosyada YAZILI.

⑤ `B` tuşuna basınca Console'da hangi hatanın çıktığı.
   SEBEP: yukarıdakinin doğrudan sonucu. `placementGhost` sahne dosyasında
          YOK; null kalırsa BoardAdapter.cs:369 kipi hiç açmaz ve
          konular/07'nin ölçtüğü ArgumentException'a HİÇ ULAŞILMAZ.
   >> Bu, bu turun bulduğu ve raporladığı tek ÇELİŞKİDİR: << iki belge
          farklı Console çıktısı öngörüyor ve ayıran şey tek bir
          serileştirilmiş referans. ADIM 5 ikisini de yazıyor.

⑥ Yerel geçişin MALİYETİ (nanosaniye / kare zamanı / tahsis).
   SEBEP: Profiler açılmadı, hiçbir zamanlama yapılmadı, hedef cihazda
          hiçbir yapı alınmadı.
   >> Bu yüzden bu dosya "pahalıdır" da "ucuzdur" da DEMİYOR. <<

⑦ Üç dış oyunun (Slay the Spire · Vampire Survivors · Stardew Valley)
   motor/çerçeve iddiaları.
   SEBEP: üçünün de kaynağı kapalı; bu turda resmî belgeye karşı
          doğrulanmadı. Altıncı duraktaki tabloda satır satır işaretli.
```

---

## Ölçüm komutları — hiçbirine bana güvenerek inanma

***Yukarıdaki her sayı aşağıdaki dört komuttan biriyle yeniden üretilebilir.***

```
# ① Vector3 ve MonoBehaviour yansıma sondası (Editor GEREKMEZ)
MONO="C:/Program Files/Unity/Hub/Editor/2021.3.45f2/Editor/Data/MonoBleedingEdge/bin/mono.exe"
CSC="C:/Program Files/Unity/Hub/Editor/2021.3.45f2/Editor/Data/MonoBleedingEdge/lib/mono/4.5/csc.exe"
LIB="C:/Program Files/Unity/Hub/Editor/2021.3.45f2/Editor/Data/Managed/UnityEngine"
"$MONO" "$CSC" -r:"$LIB/UnityEngine.CoreModule.dll" Probe.cs && "$MONO" Probe.exe
   ► typeof(Vector3).IsValueType · Marshal.SizeOf · MonoBehaviour'un alanları

# ② CoreModule'ün yerel çağrı sayımı
IKDASM="C:/Program Files/Unity/Hub/Editor/2021.3.45f2/Editor/Data/MonoBleedingEdge/lib/mono/4.5/ikdasm.exe"
"$MONO" "$IKDASM" "$LIB/UnityEngine.CoreModule.dll" > core.il
grep -c "cil managed internalcall" core.il     # ► 3654
grep -c "^\s*\.method" core.il                 # ► 11916
grep -c "pinvokeimpl" core.il                  # ► 0

# ③ CS1612 ve sessiz yazım hatası (ikisi de derleyiciyi KOŞTURUR)
"$MONO" "$CSC" -warn:4 -target:library -r:"$LIB/UnityEngine.CoreModule.dll" CS1612.cs
   ► error CS1612: Cannot modify the return value of 'Transform.position'...
"$MONO" "$CSC" -warn:4 -target:library -r:"$LIB/UnityEngine.CoreModule.dll" Awakee.cs
   ► >> HİÇBİR ÇIKTI YOK << ve awakee.dll üretilir

# ④ Bu repodaki sayımlar
cd <proje kökü>
# >> DIKKAT — DUZ grep YANILTIR: yorum satirlarini da sayar. <<
#    grep -c "SerializeField" BoardAdapter.cs  ► 14   ◄ YANLIS (biri yorum, :126)
#    grep -c "SerializeField" UnitView.cs      ►  4   ◄ YANLIS (biri yorum, :69)
grep -cE "^\s*\[SerializeField" Assets/Game/Unity/BoardAdapter.cs   # ► 13  DOGRU
grep -cE "^\s*\[SerializeField" Assets/Game/Unity/UnitView.cs       # ►  3  DOGRU
# Sahnedeki BoardAdapter blogunun serilestirilmis alanlari (m_* olmayanlar):
sed -n '/^  m_EditorClassIdentifier:/,/^--- /p' Assets/Scenes/SampleScene.unity   | grep -E "^  [a-zA-Z]"                     # ► width · height · terrainSprites · unitPrefab
grep    "guid:" Assets/Game/Unity/BoardAdapter.cs.meta            # ► 9997...
grep    "m_Script" Assets/Scenes/SampleScene.unity                # ► aynı guid
find Assets -name "*.meta" | wc -l                                # ► 114
grep -rn "Vector3" Assets/Game --include=*.cs | wc -l             # ►  5 satır
#    (2 Vector3 + 2 Vector3Int + 1 YORUM — ham grep yine yorumu sayiyor)
find Assets/Game -name "*.cs" | wc -l                             # ► 33 uretim dosyasi
grep -rl "SpriteRenderer" Assets/Game --include=*.cs | wc -l      # ►  2  (oteki 31: sifir)
```

***`grep -E` içinde alternasyon için DÜZ `|` kullan, `\|` DEĞİL.***
Ölçüldü: `\|` düz bir boru karakteridir, desen hiçbir şeyi eşlemez ve
***SAHTE BOŞLUK*** raporlar. Bu dosyanın hazırlığında bu tuzağa canlı olarak
düşülmedi, çünkü baştan biliniyordu. Ama bilinmese düşülürdü.

---

## İlgili

- ***Çağrı sırası, garantiler, coroutine, Domain Reload***:
  [`konular/08-motor-cagri-dongusu.md`](../deep/konular/08-motor-cagri-dongusu.md)
- ***Yönetilen/yerel ömür, `Destroy`, `== null`***:
  [`dil/07-bellek-canlilik-ve-yikim.md`](../deep/dil/07-bellek-canlilik-ve-yikim.md)
- Assembly duvarı ve dört faturası: [`konular/02-assembly-duvari.md`](../deep/konular/02-assembly-duvari.md)
- Tıklamanın yolculuğu ve ADIM 5'te kırılacak şey:
  [`konular/07-tiklamadan-eyleme.md`](../deep/konular/07-tiklamadan-eyleme.md)
- Değer/referans ve kimlik — `==` ile `ReferenceEquals` ayrımı:
  [`dil/05-deger-referans-ve-kimlik.md`](../deep/dil/05-deger-referans-ve-kimlik.md)
- Derleyicinin gizlediği makineler (`yield`, `async`, `Awaitable` yokluğu):
  [`05-yok-olan-mekanizmalar-csharp.md`](05-yok-olan-mekanizmalar-csharp.md)
- Motorun sunduğu ama bu projenin almadığı mekanizmalar:
  [`04-yok-olan-mekanizmalar-unity.md`](04-yok-olan-mekanizmalar-unity.md)
- Üye başına gerekçeler: [`kod/Unity/BoardAdapter.md`](../deep/kod/Unity/BoardAdapter.md) ·
  [`kod/Unity/UnitView.md`](../deep/kod/Unity/UnitView.md)
- Bu ağacın yönlendirmesi: [`README.md`](README.md) ·
  okuma sırası: [`00-okuma-sirasi.md`](00-okuma-sirasi.md) ·
  kavram borcu: [`03-kavram-borc-defteri.md`](03-kavram-borc-defteri.md)
- `Docs/deep/` ağacının kendi yönlendirmesi: [`deep/README.md`](../deep/README.md)
