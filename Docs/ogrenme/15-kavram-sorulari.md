# Kavram soruları — katlanmış cevaplarla

Bu dosya sana **sorulmuş** kavram sorularını tutar. Her soru cevabıyla birlikte
duruyor, ama cevap **katlanmış**: `Cevabı aç` yazan satıra tıklamadan görünmez.

## Nasıl kullanılır

Sıra şu ve sırayı bozmak dosyanın işe yaramasını engeller:

1. **SORU**'yu oku ve dur.
2. **CEVAP İÇİN GEREKEN ÖLÇÜM** alanındaki dosyaları kendin aç. Bu alan sana
   nereye bakacağını söyler, ne göreceğini söylemez.
3. **DESTEKLEYİCİ AYRINTI**'yı oku. Burada cevap yok; cevabı kurmanı sağlayacak
   mekanizma var.
4. Cevabını **kendi cümlenle** kur.
5. **YANLIŞ CEVAP VE NEDEN CAZİP** alanını oku. Kurduğun cevap oradaysa, asıl
   öğrenilecek şey de oradadır.
6. Ancak sonra **CEVAP**'ı aç.

***Dördüncü adımı atlamak, bu dosyayı bir okuma listesine çevirir.*** Kendi
cevabını kurmadan açılan bir cevap, tanıdık gelir ve öğrenilmez.

## Bu dosya büyüyecek

Her yeni kavram sorusu buraya eklenir. Numaralar **kimliktir**: bir soru
cevaplansa bile numarası başka bir soruya verilmez, ve sorular silinmez.
Bugün dört soru var.

***Satır numarası bu dosyada bilerek YOK.*** Atıflar tip ve üye adıyla yazılı,
çünkü bu depoda satır numaraları kayıyor — bugün 351 kırık çapa ölçüldü ve
bunun ne anlama geldiği zaten üçüncü sorunun konusu.

---

## Soru 1 · Bir varlık dosyası TİP mi seçer, VERİ mi

**SORU**

> `StructureBlueprint.DefaultProduced` bugün bir şey SEÇİYOR. Seçtiği şey bir
> TİP mi, bir VERİ mi? Cevabı `Unit_Piyade.asset` ile `Unit_Akinci.asset`
> dosyalarının `m_Script` GUID'lerinin aynı olmasına dayandır.

**NE ÖLÇÜYOR** — Serileştirilmiş bir varlığın **kimliği** ile onun C#
**tipini** ayırt edebilme yetkinliğini.

**CEVAP İÇİN GEREKEN ÖLÇÜM**

```
Assets/Game/Core/Combat/StructureBlueprint.cs   -> DefaultProduced
                                                -> DefaultProducedIndex
Assets/Game/Unity/UnitBlueprintAsset.cs         -> Definition
Assets/Game/Blueprints/Unit_Piyade.asset        -> m_Script satırı
Assets/Game/Blueprints/Unit_Akinci.asset        -> m_Script satırı
```

**DESTEKLEYİCİ AYRINTI**

Bir `.asset` dosyası bir **örnektir**, bir sınıf değil. İçindeki `m_Script`
satırı, o örneğin hangi C# sınıfından olduğunu gösteren bir GUID taşır. O GUID
sınıfın kendi `.cs` dosyasının `.meta` dosyasında yazılıdır ve dosya taşınsa
bile değişmez — kimliği tutan şey yol değil, GUID'dir.

Buradan bir sonuç çıkar ve soru tam olarak onu istiyor: **iki varlık aynı
`m_Script` GUID'ini taşıyorsa, ikisi de aynı sınıfın örneğidir.** Farklı
GUID'ler farklı sınıflar demektir.

Şimdi `DefaultProduced` üyesinin ne yaptığına bak. Bir dizinin bir elemanını
döndürüyor ve hangi elemanı döndüreceğini `DefaultProducedIndex` söylüyor.
Bir dizin bir **tam sayıdır**. Bir tam sayının seçebileceği şeyin ne olduğunu
sor.

Son olarak Piyade ile Akıncı'nın gerçekten neyle ayrıştığına bak: `maxHealth`
30'a karşı 20, `damage` 10'a karşı 6, `attackRange` 1'e karşı 2,
`attackCooldownSeconds` 0,8'e karşı 1,0. Bu dördünün ortak cinsi ne.

**YANLIŞ CEVAP VE NEDEN CAZİP**

> *"TİP seçiyor, çünkü Piyade ile Akıncı iki farklı birim TÜRÜ."*

Cazip, çünkü Türkçede ve oyun dilinde **"tür"** ile programlamadaki **"tip"**
aynı sözcüğe biniyor. Oyunda gerçekten iki tür var; oyuncu onları farklı
görüyor, farklı kullanıyor, farklı adla anıyor. Ama C# tarafında **tek bir
sınıf** duruyor.

***Bu iki anlamın karıştırılması bu projedeki en pahalı karışıklıktır***, çünkü
fabrikanın gerekip gerekmediğini belirleyen ayrım tam olarak budur. On birim
türü olması on tip olduğu anlamına gelmez.

<details><summary>Cevabı aç</summary>

**VERİ seçiyor.**

`Unit_Piyade.asset` ile `Unit_Akinci.asset` dosyalarının ikisi de
`m_Script: {fileID: 11500000, guid: 40940e9e934d40541a0e5a2f860e211f, type: 3}`
satırını taşıyor. **Aynı GUID**, yani ikisi de `UnitBlueprintAsset` sınıfının
örneği. Ortada seçilecek ikinci bir sınıf yok.

`DefaultProduced` bir `UnitBlueprint` döndürüyor ve bunu `produces` dizisinden
`DefaultProducedIndex` ile alıyor. Seçim bir **dizin**, yani bir tam sayı. Bir
tam sayı bir tip seçemez; bir tam sayı yalnız bir dizinin kaçıncı elemanını
istediğini söyleyebilir.

İki varlığın farkı dört **alan değeridir** — can, hasar, menzil, bekleme
süresi. Dördü de veri.

***Sonucun asıl önemi:*** bu yüzden projede bir fabrika yok. Fabrika, çağıranın
dönüş **tipini** bilmemesini sağlar; burada seçilen şey tip değil, o yüzden
fabrikanın yapacağı bir iş de yok. On birim varlığı, bir sınıfın on dosyasıdır.
Uzun hâli [13-desen-secim-rehberi.md](13-desen-secim-rehberi.md) · Desen 5.

</details>

**MÜLAKAT HÂLİ** — *"Bir `ScriptableObject` varlığının `m_Script` GUID'i neyi
tanımlar, ve iki varlığın aynı GUID'i taşıması size ne söyler?"*

---

## Soru 2 · Polimorfizm var, fabrika yok — eksik olan koşul ne

**SORU**

> `UnitOrderBook` içindeki sözlük `IUnitOrder` tutuyor ve içindekinin
> `AttackOrder` mı `ReviveOrder` mı olduğunu bilmiyor. Buna rağmen bu projede
> bir fabrika yok. POLİMORFİZMİN VARLIĞI ile FABRİKANIN GEREKLİLİĞİ arasındaki
> hangi koşul bugün sağlanmıyor? Cevabı `BoardAdapter` içindeki emir kuran iki
> çağrı noktasını göstererek yaz.

**NE ÖLÇÜYOR** — Polimorfizmin **kullanım** ekseni ile fabrikanın **yaratım**
ekseninin ayrı iki eksen olduğunu görebilme yetkinliğini.

**CEVAP İÇİN GEREKEN ÖLÇÜM**

```
Assets/Game/Unity/Orders/UnitOrderBook.cs   -> Write . TryGet . Advance
Assets/Game/Unity/Orders/IUnitOrder.cs      -> Describe  (üstündeki yorumu da oku)
Assets/Game/Unity/Orders/AttackOrder.cs     -> kurucusu
Assets/Game/Unity/Orders/ReviveOrder.cs     -> kurucusu
Assets/Game/Unity/BoardAdapter.cs           -> IssueOrder  ve onu çağıran İKİ yer
```

**DESTEKLEYİCİ AYRINTI**

Bir nesnenin hayatında birbirinden bağımsız iki eksen vardır.

```
YARATIM ekseni   nesne DOĞARKEN somut tipini kim söylüyor
KULLANIM ekseni  nesne KULLANILIRKEN somut tipini kim biliyor
```

Polimorfizm **kullanım** ekseninde kazanç sağlar. Bir fabrika **yaratım**
ekseninde kazanç sağlar. İkisi aynı anda gerekmez ve biri ötekini
gerektirmez.

Kullanım ekseninin bu projede ödendiğine dair kanıt kodun kendi yorumunda
yazılı: `IUnitOrder` üzerindeki `Describe` üyesinin üstünde *"TİP SORGUSUNUN
YERİNE GEÇİYOR"* diyor. O üye olmasaydı tahta `order is AttackOrder` diye
sorardı. Yani polimorfizm burada gerçek ve bir işi var.

Şimdi yaratım eksenine bak, ve tek bir soru sor: **kaç tane yaratım noktası
var, ve her biri ne kurduğunu biliyor mu.** `BoardAdapter` içinde `IssueOrder`
üyesini çağıran iki yer var. Biri düşmana tıklandığında, öteki düşmüş bir dosta
tıklandığında çalışıyor. Her birinin `new` ile ne kurduğuna bak.

Bir fabrikayı doğuran koşulu bu sayımdan kendin çıkarabilirsin.

**YANLIŞ CEVAP VE NEDEN CAZİP**

> *"Arayüz var, iki uygulama var, öyleyse fabrika da gerekir."*

Cazip, çünkü ders kitaplarında Factory ile polimorfizm neredeyse **her zaman
aynı örnekte** birlikte anlatılır — `IShape`, `Circle`, `Square`, ve yanında
bir `ShapeFactory`. Okuyan kişi ikisini tek bir paket sanır.

Ama o örnekte fabrikayı doğuran şey arayüz **değildi**. Fabrikayı doğuran şey,
`Create("circle")` çağrısındaki o **dizeydi**: tip adı veriden geliyordu ve
çağıran onu okuyana kadar ne kuracağını bilmiyordu. Arayüzü silsen bile o dize
fabrikayı gerektirmeye devam ederdi.

***Ayırt edici soru şudur: bu projede o dizenin karşılığı ne.***

<details><summary>Cevabı aç</summary>

**Sağlanmayan koşul şudur: tek bir yaratım noktasının, veriye bakarak iki ayrı
somut tip arasında seçim yapmak zorunda kalması.**

`BoardAdapter` içinde `IssueOrder`'ı çağıran iki yer var ve **her biri ne
kurduğunu derleme zamanında biliyor**:

```
düşman hedefine tıklama    ->  IssueOrder(selectedUnit, new AttackOrder(...))
düşmüş dosta tıklama       ->  IssueOrder(selectedUnit, new ReviveOrder(...))
```

İki nokta, her birinde **bir** tip. Hiçbiri "hangisini kurayım" diye sormuyor,
çünkü ikisi ayrı girdi yollarında oturuyor: soru zaten oyuncunun neye
tıkladığıyla cevaplanmış durumda.

Fabrika, **tek** bir noktanın iki tip arasında seçim yapması gerektiğinde
doğar. İki nokta ve iki tip, fabrika değil; iki ayrı yoldur.

Sözlüğün somut tipi bilmemesi bundan tamamen bağımsızdır ve **doğru** bir
tasarımdır. `UnitOrderBook` emirleri saklıyor ve ilerletiyor; ne sakladığını
bilmesi için hiçbir sebep yok. Polimorfizm orada kazandırıyor, yaratım
tarafında kazandıracak bir şey yok.

***Tek cümle:*** polimorfizm nesneyi **kullananı** somut tipten kurtarır,
fabrika nesneyi **kuranı** kurtarır, ve bu projede kuran zaten kurtulmak
istemiyor.

</details>

**MÜLAKAT HÂLİ** — *"Bir arayüzün iki uygulaması var ve bir koleksiyon onları
birlikte tutuyor. Bu tek başına bir fabrikayı gerekçelendirir mi; etmiyorsa
eksik olan nedir?"*

---

## Soru 3 · İki kapı, 351'e karşı 0 — hangisi neyi ölçüyor

**SORU**

> `check-doc-code-refs` bugün 351 ihlal veriyor ama `check-cited-names` sıfır
> veriyor. İkisi de aynı belgelere, aynı koda bakıyor. Bu iki kapı NEYİ farklı
> ölçüyor, ve hangisi kod kaydığında hayatta kalan atıf biçimini savunuyor?

**NE ÖLÇÜYOR** — Bir atfın neye bağlandığını — bir **konuma** mı bir **ada** mı
— ve hangisinin kod kaymasına dayandığını ayırt edebilme yetkinliğini.

**CEVAP İÇİN GEREKEN ÖLÇÜM**

```
Tools/check-doc-code-refs.py   -> dosyanın başındaki "NE TARAR" bloğu
                               -> KATMAN 3 (kayma) açıklaması
Tools/check-cited-names.py     -> dosyanın başındaki "NE TARAR" bloğu
                               -> "YANLIS POZITIF POLITIKASI" paragrafı
```

***Sorunun içinde bir önerme var ve onu da sına.*** Soru *"ikisi de aynı
belgelere, aynı koda bakıyor"* diyor. İki kapının kendi kaynağında yazılı
kapsamlarını oku ve bu cümlenin doğru olup olmadığına kendin karar ver.

**DESTEKLEYİCİ AYRINTI**

Bir atıf iki şeye bağlanabilir ve ikisinin kaymaya dayanıklılığı aynı değildir.

```
KONUMA bağlı atıf   BoardAdapter.cs, 1089. satır
                    -> üstüne tek bir satır eklenirse SESSİZCE ÖLÜR
ADA bağlı atıf      BoardAdapter -> NewCombatant üyesi
                    -> üye dosya içinde nereye taşınırsa taşınsın YAŞAR
```

***Bu örnek uydurma değil.*** `NewCombatant` üyesi gerçekten 1089. satırdaydı ve
bugün 2976. satırda. Ada bağlı atıf o taşınmayı hiç fark etmedi.

Bir kod satırının numarası, o satırın **kendisine** ait bir özellik değildir.
Numara, o satırın üstünde kaç satır olduğunun bir sonucudur. Dosyanın başına
bir yorum satırı eklemek, aşağıdaki her atfı sessizce yanlışlar.

Bir üye adı ise o üyenin kendisine aittir. Üye dosya içinde yukarı aşağı
taşınsa bile ad değişmez.

Şimdi iki kapının ne **soru sorduğuna** bak. Biri *"bu ad projede var mı"* diye
soruyor. Öteki *"bu numarada duran şey hâlâ belgenin iddia ettiği şey mi"* diye
soruyor. İki sorunun kaymaya karşı davranışı aynı değil.

Son olarak sayıların ne anlattığını sor. 351 sayısı bir kapının **sıkılığını**
mı ölçüyor, yoksa denetlediği atıf biçiminin **kırılganlığını** mı.

**YANLIŞ CEVAP VE NEDEN CAZİP**

> *"`check-doc-code-refs` daha sıkı bir kapı olduğu için 351 veriyor;
> `check-cited-names` gevşek olduğu için sıfır veriyor."*

Cazip, çünkü büyük bir hata sayısı sezgisel olarak **sıkılık** gibi okunur, ve
sıfır **gevşeklik** gibi. Kapıları tek bir "sıkılık" eksenine dizmek doğal
geliyor.

Ama bu, iki kapının **aynı şeyi** ölçtüğünü varsayar. Ölçmüyorlar. Sıkılık
sırasına dizmek ancak aynı büyüklüğü ölçen iki alet için anlamlıdır; bir
termometre ile bir cetveli sıkılık sırasına dizemezsin.

***Ve sorunun kendi önermesi de yanlış:*** ikisi aynı belgelere bakmıyor.
Birinin kapsamı `Docs/` altındaki markdown, ötekinin kapsamı `Assets/`
altındaki `.cs` **yorum satırları**. Kesişimleri sıfır.

<details><summary>Cevabı aç</summary>

**Farklı şeyi ölçüyorlar, ve kapsamları bile ayrık.**

```
check-doc-code-refs   KAPSAM  Docs/ altındaki .md dosyaları
                      DESEN   "Dosya.cs:SATIR" biçimindeki atıflar
                      SORU    bu NUMARADA duran şey hâlâ belgenin dediği şey mi

check-cited-names     KAPSAM  Assets/ altındaki .cs dosyalarının YORUM satırları
                      DESEN   test adı biçimindeki tanımlayıcılar ve <see cref="..."/>
                      SORU    bu AD projede herhangi bir yerde var mı
```

İkisinin kesişimi **boş**. Soru bir önerme taşıyordu — *"aynı belgelere, aynı
koda bakıyor"* — ve o önerme yanlış.

**Ölçtükleri büyüklük şu:** biri bir **konumu** doğruluyor, öteki bir **adı**.

Ve 351'e karşı 0 farkının sebebi budur. Bu depoda paralel oturumlar `Assets/`
altındaki satırları kaydırdı. Kayma, konuma bağlı 351 atfı yanlışladı; ada
bağlı hiçbir atfı yanlışlamadı, çünkü **hiçbir ad kaybolmadı** — yalnız
yerleri değişti.

**Kod kaydığında hayatta kalan atıf biçimini savunan kapı `check-cited-names`
kapısıdır.** Onun savunduğu biçim ada bağlı atıftır ve o biçim kaymaya
duyarsızdır.

***Ama iki incelik var ve ikisi de kapıların kendi kaynağında yazılı.***

① `check-cited-names`'in sıfırı bir **başarı belgesi değildir**. O kapı
"uydurulmuş ad" arıyor, adın doğru yerde olduğunu doğrulamıyor; kendi kaynağı
bunu *"bu tarama İDDİA denetler, kod denetlemez"* diye yazıyor.

② `check-doc-code-refs` bu zayıflığı bildiği için bir ara biçim taşıyor:
**ALINTI çapası**. Belge, satır numarasının yanına o satırdaki kodun birebir
metnini yazar; kod kayınca kapı metni yeni yerinde bulur ve kaymayı **rakamla**
bildirir. Yani numara kullanmak yasak değil, **çapasız** numara kullanmak
kırılgan.

</details>

**MÜLAKAT HÂLİ** — *"İki denetiminiz var, biri 351 hata veriyor biri sıfır. Bu,
ilkinin daha sıkı olduğunu mu gösterir?"*

---

## Soru 4 · Alt sınıfların işini ne yapıyor, ve bu Unity'ye mi özgü

**SORU**

> Bir Factory Method'un işi, çağıranın dönüş TİPİNİ bilmemesini sağlamaktır. Bu
> projede `CreateCombatant`'ın dönüşü `Combatant`, `sealed`, tek. Öyleyse
> `UnitBlueprintAsset` bir fabrikanın alt sınıflarının işini neyle yapıyor, ve
> bu Unity'ye özgü mü yoksa her dilde mümkün mü?

**NE ÖLÇÜYOR** — Kalıtımla taşınan değişkenliğin veriyle de taşınabildiğini
görebilme, ve **mekanizmayı** motorun kattığı **kolaylıktan** ayırabilme
yetkinliğini.

**CEVAP İÇİN GEREKEN ÖLÇÜM**

```
Assets/Game/Unity/UnitBlueprintAsset.cs      -> Definition
                                             -> [CreateAssetMenu] ve [SerializeField] alanları
Assets/Game/Core/Combat/UnitBlueprint.cs     -> CreateCombatant
Assets/Game/Blueprints/                      -> kaç dosya var, saydır
Assets/Game/ altında                         -> "abstract" ve "virtual" kaç kez geçiyor
```

**DESTEKLEYİCİ AYRINTI**

Klasik Factory Method'da değişkenlik **alt sınıflarda** yaşar:

```
                    IUnitFactory
                    /          \
        PiyadeFactory        AkinciFactory
        Create() -> can 30    Create() -> can 20
```

N tür istiyorsan N alt sınıf yazarsın. Her yeni tür bir **derleme birimi
değişikliğidir**: yeni dosya, yeni sınıf, yeni derleme.

Şimdi bu projeye bak ve iki şey say. Birincisi: `Assets/Game/` altında kaç
`abstract` ve kaç `virtual` var. İkincisi: `Assets/Game/Blueprints/` altında
kaç dosya var.

İki sayının **oranı** soruyu cevaplıyor.

Sonra ikinci yarıya geç. `UnitBlueprintAsset` içindeki değişkenliğin nerede
durduğuna bak: `maxHealth`, `damage`, `attackRange`, `attackCooldownSeconds`.
Bunların **cinsi** ne, ve o cinsi bir JSON dosyasında ya da bir veritabanı
satırında tutmanı engelleyen bir şey var mı diye sor.

Engel yoksa, Unity'nin kattığı şeyin ne olduğunu ayrıca sor. `[CreateAssetMenu]`
ne yapıyor, `[SerializeField]` ne yapıyor, ve `.meta` dosyasındaki GUID ne işe
yarıyor.

**YANLIŞ CEVAP VE NEDEN CAZİP**

> *"Unity'ye özgü, çünkü `ScriptableObject` Unity'nin tipi ve bu iş onsuz
> olmaz."*

Cazip, çünkü mekanizmayı bu projede **taşıyan araç** gerçekten Unity'ye ait.
`ScriptableObject`, `[CreateAssetMenu]`, Inspector, `.meta` GUID'i — dördü de
motorun.

Ama soru aracı sormuyor, **işi** soruyor. "Değişkenliği alt sınıf yerine veride
tutmak" fikri hiçbir motora ait değildir; her dilde yapılabilir ve adı da
vardır. Aracı fikirle karıştırmak, aynı çözümü C#'sız bir ortamda tanıyamamana
yol açar.

***İkinci bir cazibe daha var ve ters yönde:*** "her dilde mümkün, o hâlde
Unity'nin kattığı bir şey yok" demek de yanlış. Unity gerçek bir şey katıyor,
yalnız kattığı şey mekanizma değil.

<details><summary>Cevabı aç</summary>

**Alt sınıfların işini VERİ yapıyor — bir varlık dosyası.**

Ölçüm iki sayıyla bitiyor. `Assets/Game/` altında `abstract` ve `virtual`
**sıfır** kez geçiyor. `Assets/Game/Blueprints/` altında **10** birim ve **14**
yapı varlığı duruyor. Yani yirmi dört "tür", sıfır alt sınıf.

Değişkenlik `UnitBlueprintAsset` üzerindeki `[SerializeField]` alanlarında
yaşıyor — `maxHealth`, `damage`, `attackRange`, `attackCooldownSeconds` — ve
`Definition` üyesi o alanları düz bir C# `UnitBlueprint` nesnesine çeviriyor.
Yeni bir birim türü yeni bir **dosya** ister, yeni bir **sınıf** değil.

Bu takasın adı var: **kalıtım yerine veri**. Kalıp adıyla *type object*, daha
yaygın söylenişiyle **veri güdümlü tasarım**.

**İkinci yarının cevabı: mekanizma her dilde mümkün, ama Unity üç şey
katıyor.**

Fikir evrenseldir. Aynı sonucu bir JSON dosyası, bir CSV satırı ya da bir
veritabanı kaydıyla her dilde alırsın; tek gereken, değişkenliği bir kayıt
tipinde tutup çalışma zamanında okumaktır.

Unity'nin kattığı şey mekanizma değil, **üç kolaylıktır**:

```
① YAZMA YÜZEYİ   tasarımcı sayıyı Inspector'da değiştirir, kod derlemeden.
                 [Min] ve [Header] kelepçeleri de orada yaşar.
② KİMLİK         varlık dosyası bir GUID taşır. Dosya taşınsa, yeniden
                 adlandırılsa bile ona bakan sahne ve prefab bağı KOPMAZ.
③ BAĞ DENETİMİ   sahneden varlığa giden referans derleme ve yapı zamanında
                 bilinir; bir JSON yolunun yazım hatası ancak çalışırken patlar.
```

***Tek cümle:*** fikir her dilde var, kimlik ve yazma yüzeyi Unity'nin. Ve
fabrikanın olmamasının sebebi de budur — seçilecek ikinci bir tip hiç
doğmadığı için, alt sınıfları seçecek bir mekanizmaya da ihtiyaç kalmıyor.
Uzun hâli [13-desen-secim-rehberi.md](13-desen-secim-rehberi.md) · Desen 5.

</details>

**MÜLAKAT HÂLİ** — *"On birim türünüz var ve tek bir alt sınıfınız yok. Factory
Method'un alt sınıflarının yaptığı işi ne yapıyor?"*

---

## İlgili

- On iki desenin ayırıcı testi, motor karşılığı ve tetikleyicileri:
  [13-desen-secim-rehberi.md](13-desen-secim-rehberi.md)
- Kodda **zaten** duran desenler:
  [01-koda-gomulu-desenler.md](01-koda-gomulu-desenler.md)
- Bugün ne yok ve ne zaman gelir:
  [02-sonraki-asamalar.md](02-sonraki-asamalar.md)
- Hangi kavramın sahip dosyası var:
  [03-kavram-borc-defteri.md](03-kavram-borc-defteri.md)
- Bu ağacın yönlendirmesi: [README.md](README.md)
- Serileştirme, `.meta` ve GUID kimliği — birinci sorunun mekanizması:
  [08-unity-altyapisi.md](08-unity-altyapisi.md)
