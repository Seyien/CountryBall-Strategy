# Görsel sözlük ve varlık eksikleri

Bu dosya bir soruya cevap verir: **oyuncunun ekranda görmesi gereken şeylerin
kaçı bugün orada duruyor.**

Öteki dosyalar kodun ne dediğini sayar. Bu dosya ekranın ne dediğini sayar.
İkisi ayrı ölçümdür, çünkü hiçbir derleyici, hiçbir test koşucusu ve bu depodaki
hiçbir kapı bir PNG dosyasını **açmaz**. Bir varlığın yokluğu sessiz katmandır.

Ölçüm tarihi: **2026-08-28**. Bütün sayılar o gün depoda koşturuldu. Kod
kazanır; bu sayfa kodla çeliştiği gün bayat olan burasıdır.

## Bu dosyanın yapmadığı üç şey

| Yapmaz | Kimin işi |
|---|---|
| Bir sprite'ı içeri almaz, üretmez, indirmez | operatör; bu sayfa yalnızca yolu yazar |
| `Assets/Art/THIRD_PARTY_ASSETS.md` manifestine satır **yazmaz** | manifest sahibi; buradaki satırlar TASLAKTIR |
| Serileştirilmiş alanın Inspector adımını tekrar etmez | [11-unity-penceresi-adim-adim.md](11-unity-penceresi-adim-adim.md) ve [12-unity-editor-baglama.md](12-unity-editor-baglama.md) |

---

## 1 · Türün görsel sözlüğü

### 1.1 Bu oyunun türü

**Izgara tabanlı, kip taşıyan, yapı yerleştirmeli ve birim üretimli taktik
strateji.**

Dört sıfatın her biri ölçülerek konuldu.

**Izgara.** Sahnede bir `Grid` bileşeni duruyor ve hücre boyu `1`.

**Kip.** Sıra satırını ekrana yazan üye `BattleStatusView.cs:97`.

**Yapı yerleştirme.** On yapı türünü adıyla sayan yer
`Assets/Editor/SceneSetupTool.cs:997`.

**Birim üretimi.** Gövde görselini tanım dosyasından okuyan satır
`Assets/Game/Unity/ProductionDirector.cs:561`.

### 1.2 Sözlük nasıl türetildi

Ezberden bir liste yazılmadı. Yöntem şudur: **oyuncunun tekrarladığı döngü
alınır, her FİİL için tek bir soru sorulur.**

> Oyuncunun bu şeyin olduğunu anlaması için ekranda ne GÖRÜLMELİ?

Yöntem her tür için aynıdır, liste hiçbir tür için aynı değildir. Bir platform
oyunu aynı soruyla karakter, zemin, tehlike, toplanabilir ve zıplama geri
bildirimi üretir. Bu iki liste neredeyse hiç kesişmez ve türetme birebir aynıdır.

Bu oyunun döngüsü on dört fiildir. Her fiil bir sözlük maddesi doğurur.

### 1.3 Sözlük × envanter farkı

| # | Fiil (oyuncu ne yapar) | Görülmesi gereken | Verdik | Dosya |
|---|---|---|---|---|
| 1 | Tahtaya bakar | zemin karosu | VAR | `grass_plain/tufts/flowers_tile_0000..0002` |
| 2 | Tahtanın nerede bittiğini görür | kenar halkası | VAR | 8 yuvada 4 `dirt_fill_*` karosu |
| 3 | Paletten yapı türü seçer | yapı simgesi | VAR | her `Structure_*.asset` bir `icon` taşıyor |
| 4 | Yapıyı bir hücreye sürükler | yerleştirme hayaleti | VAR | `PlacementGhost.prefab` |
| 5 | Hangi hücreye bastığını görür | hücre çerçevesi | VAR | `ui_cell_frame_16x16` |
| 6 | Yapıdan birim üretir | üretim sayacı | VAR | `ui_white_square_4x4` + `ProductionTimerView` |
| 7 | Bir birim seçer | seçim işareti | VAR | `selection_unit_bracket_tile_0061` |
| 8 | Birimi yürütür | birim gövdesi | VAR | 6 birim simgesi |
| 9 | Saldırır | saldırı pozu | VAR | `*_attack_tile_0143` ve `*_attack_tile_0161` |
| 10 | Menzilli vuruşu izler | uçan cisim | **YANLIŞ ŞEY DURUYOR** | bir ASA duruyor, bkz. §4 satır P3 |
| 11 | Vuruşun isabet ettiğini görür | isabet geri bildirimi | YOK | tek sinyal can çubuğunun kısalması |
| 12 | Canı izler | can çubuğu | VAR | `ui_white_square_4x4` |
| 13 | Bir birimin öldüğünü görür | ölüm geri bildirimi | YOK | `UnitViewPool.cs:90` nesneyi kapatıyor, birim bir kare içinde yok oluyor |
| 14 | Sıranın kimde olduğunu görür | sıra göstergesi | VAR | `BattleStatusView.cs:97` |
| 15 | İki tarafı ayırt eder | takım rengi | VAR | paketin kendi kırmızı satırı + `Derived/` |
| 16 | Yapı söker | çöp simgesi | VAR | `icon_trash_tile_0192` |
| 17 | Bir birimin nereye vurabildiğini görür | menzil halkası | YOK | varlık VAR, tüketicisi yok; bkz. §6 A2 |
| 18 | Kazandığını ya da kaybettiğini görür | zafer/yenilgi ekranı | YOK | `BoardAdapter.cs:829` yalnız `Debug.Log` yazıyor |
| 19 | Bir KIŞLA koyduğunu görür | kışla görseli | **YANLIŞ ŞEY DURUYOR** | bir DEPO duruyor, bkz. §4 satır P1 |
| 20 | Bir TOP ARABASI ürettiğini görür | topçu gövdesi | **YANLIŞ ŞEY DURUYOR** | bir NAKLİYE KAMYONU duruyor, bkz. §4 satır P4 |

Yirmi maddenin **on dördü VAR**, **üçü YANLIŞ ŞEY DURUYOR**, **üçü YOK**.

Bu oran, bir prototip için iyidir ve bunu söylerken tek bir uyarı taşıyor:
**oranı bozan üç madde de savaş anına bakıyor.** İsabet, ölüm ve zafer üçü
birden savaşın SONUCUNU anlatan maddelerdir, ve bugün üçü de yalnızca sayılarla
anlatılıyor. Oyuncu neyin olduğunu okumak zorunda; görmüyor.

---

## 2 · Üç katman sayımı

Kural şudur: bir özelliğin envanteri **üç katmanda birden** sayılır. Kod
katmanı, serileştirilmiş bağ katmanı, varlık katmanı. Üçünden yalnız birine
bakan bir hüküm, o katmanın sayımıdır; özelliğin sayımı değildir.

### 2.1 Katmanların büyüklüğü

```
kod      118  .cs
bag        1  .unity  ·  3  .prefab  ·  24  .asset      = 28 dosya
varlik    38  PNG   (32 ThirdParty · 2 Derived · 4 Generated)
```

### 2.2 Ölçüm yöntemi

Her PNG'nin `.meta` dosyasından `guid` okundu. Sonra o GUID
`Assets/Scenes/`, `Assets/Game/Prefabs/` ve `Assets/Game/Blueprints/` altındaki
28 YAML dosyasında arandı. İki kova çıktı.

Yöntemin bir sınırı var ve burada yazılı olması gerekiyor: **bu tarama bir
GUID'in bulunduğunu görür, doğru yerde bulunduğunu göremez.** §4'ün tamamı bu
sınırın açtığı boşlukta yaşıyor.

### 2.3 Sonuç

| Kova | Sayı | Oran |
|---|---|---|
| **BAĞLI** — en az bir YAML dosyası GUID'ini taşıyor | **34** | %89,5 |
| **ATIL** — hiçbir YAML dosyası GUID'ini taşımıyor | **4** | %10,5 |

Atıl dört dosya:

```
Assets/Art/Generated/ui_trash_64x64.png
Assets/Art/ThirdParty/Kenney/TinyBattle/UI/icon_heart_tile_0195.png
Assets/Art/ThirdParty/Kenney/TinyDungeon/Equipment/engineer_hammer_tile_0117.png
Assets/Art/ThirdParty/Kenney/TinyDungeon/Equipment/vanguard_sword_tile_0104.png
```

### 2.4 Sayımın ORTAYA ÇIKARDIĞI ikinci şey — öksüz tanım adası

GUID taraması bir yan ürün verdi ve bu yan ürün çıplak sayıdan daha değerli.
Aynı yöntem tanım dosyalarına uygulandığında şu çıktı:

| Tanım dosyası | Sahneden ulaşılabilir mi |
|---|---|
| 10 adet Türkçe adlı `Structure_*` | EVET, sahne doğrudan atıf yapıyor |
| 6 adet Türkçe adlı `Unit_*` | EVET, o on yapının üretim listesinden |
| `Structure_CommandDepot`, `Structure_IndustrialPump`, `Structure_EnemyDepot`, `Structure_EnemyPump` | **HAYIR, hiçbir dosya atıf yapmıyor** |
| `Unit_VanguardInfantry`, `Unit_AirScout`, `Unit_AirRaider`, `Unit_RangedInfantry` | **HAYIR, yalnız o dört öksüz yapıdan** |

Sekiz tanım dosyası bir **ada** oluşturuyor: birbirlerine atıf yapıyorlar,
dışarıdan kimse onlara atıf yapmıyor. `Assets/Editor/SceneSetupTool.cs:392`
içindeki `EnsureBlueprints()` bu sekizini **üretmiyor**; yalnız Türkçe adlı
onaltısını üretiyor. Yani bunlar araç Türkçe adlara geçmeden önceki turdan
kalmış dosyalar.

Bu adanın bugün bir zararı yok, ve zararlı olacağı gün belli: **birisi bir
sprite'ı değiştirmek için tanım dosyalarını gezdiği gün**, çünkü o gün on altı
yerine yirmi dört dosya görecek ve hangi sekizinin ölü olduğunu hiçbir şey
söylemeyecek.

Adanın ikinci bir etkisi ölçüldü: **iki PNG'yi tek başına ada tutuyor.**

```
enemy_ranged_infantry_tile_0161.png       <- yalniz Unit_RangedInfantry
enemy_industrial_pump_from_tile_0048.png  <- yalniz Structure_EnemyPump
```

Yani sekiz tanım dosyası silinirse §2.3'ün ATIL sayısı **4'ten 6'ya çıkar**.
İkincisi ayrıca bir türetme borcunu da açığa vuruyor: `Derived/` altındaki iki
dosyadan biri (`enemy_command_depot_from_tile_0045`) canlı bir yapıyı besliyor,
öteki yalnızca ölü bir tanımı besliyor.

---

## 3 · Atıl varlık defteri

Atıl bir dosya bir HATA değildir, bir SORUDUR. Soru iki cevaptan birini alır ve
cevap **grup başına** yazılır, dosya başına değil. Çıplak sayının üretemediği
sinyal budur: "4 kullanılmıyor" gürültüdür, "3'ü bu oyunun döngüsünde olmayan
bir fiil için alındı" karardır.

### Grup A — İkonlar (1 dosya)

`ui_trash_64x64.png`

**Verdik: YANLIŞ ENVANTER.** Bu dosya bir çöp simgesiydi ve yerine
`icon_trash_tile_0192` geçti. Yer değiştirmenin sebebi kayıtlı: Kenney satırı
tahtadaki her şeyle aynı paletten geliyor, üretilmiş 64x64 simge gelmiyor.
Yani bu bir **tasarım bekleyen** dosya değil, **görevi devredilmiş** dosya.

Silinmesi için tek koşul var ve bugün karşılanmıyor: `Generated/` altındaki
dosyalar `GENERATED-ASSETS.md`'de kayıtlı, o kaydın da aynı turda güncellenmesi
gerekir. Kayıt güncellenmeden silinirse belge bir hayalet dosyayı anlatmaya
devam eder.

### Grup B — Sağlık ikonu (1 dosya)

`icon_heart_tile_0195.png`

**Verdik: TASARIM HENÜZ GELMEDİ.** Can bugün bir ÇUBUKLA anlatılıyor
(`ui_white_square_4x4`), bir KALPLE değil. Kalp simgesi sayısal bir can
göstergesi için alınmıştı ve o gösterge hiç doğmadı.

Bu doğru envanterdir ve bir fiil bekliyor: oyuncu birim başına canı değil, TAKIM
canını okumak istediği gün bir kalp + sayı satırı doğar ve bu dosya o satırın
varlığıdır. O gün gelene kadar atıl kalması bir eksiklik değildir.

### Grup C — Ekipman ikonları (2 dosya)

`vanguard_sword_tile_0104.png`, `engineer_hammer_tile_0117.png`

**Verdik: YANLIŞ ENVANTER.** Manifest bu üç Tiny Dungeon dosyasının rolünü
`… / attack cue` diye kaydetmiş, yani bir saldırı işareti olarak alınmışlar.
O rol sonradan **başka bir şeyle çözüldü**: Tiny Battle paketinin kendi iki
kareli saldırı pozları bulundu ve `UnitAttackView` iki poz arasında geçiş
yapıyor. Silah çizen bir çocuk nesneye gerek kalmadı.

Bu grubu YANLIŞ ENVANTER yapan şey kullanılmamaları değil, **rollerinin
kapanmış olması**: kılıç ve çekiç bugün hiçbir fiilin cevabı değil, ve envanter
listesindeki bir kardeşleri (`support_staff_tile_0130`) tamamen başka bir role
kaydırılarak kurtarıldı. Bir varlığı rolü olmadığı hâlde tutmak, onu bir gün
uygun olmayan bir yere iliştirmenin en olağan yoludur. §4 satır P3 bunun
gerçekleşmiş hâlidir.

Üçünün de lisansı temiz ve maliyeti sıfır olduğu için silinmeleri acil değil, ve
acil hâle gelecekleri koşul şudur: **paket başına dosya sayısı bir listeye
yazıldığı gün**, çünkü o gün "TinyDungeon'dan 3 dosya alındı" cümlesi, üçünden
ikisinin ölü olduğunu gizler.

### Grup D — Tekrarlanan karo (bir kova arası vaka)

`enemy_ranged_infantry_tile_0161.png`

Bu dosya §2.3'te **BAĞLI** sayıldı, çünkü `Unit_RangedInfantry.asset` GUID'ini
taşıyor. Ama o tanım dosyası §2.4'ün öksüz adasında duruyor. Yani dosya
teknik olarak bağlı, oyunda ulaşılamaz.

Ve ikinci bir olgu var. Bu dosya ile
`enemy_vanguard_infantry_attack_tile_0161.png` **piksel piksel aynı**: 256
pikselin 256'sı birebir eşleşiyor. Bayt olarak farklılar (`65A25C50…` ve
`1584E8B6…`), çünkü PNG kodlamaları ayrı; görüntü olarak tek bir resimler.

**Verdik: YANLIŞ ENVANTER.** Aynı kare iki ayrı adla, iki ayrı rol iddiasıyla
depoda duruyor. `Assets/Editor/SceneSetupTool.cs:71` bu olguyu zaten yazmış ve
o yazı bu ölçümle doğrulandı. Kalan iş bir isimlendirme borcudur, bir görsel
borcu değil.

---

## 4 · Placeholder borç satırları

Bir placeholder bir KARAR değildir, **ödenmemiş bir borçtur.** Aradaki fark
tek bir alanda yaşar: bir gerekçe, geri ödeme koşulu taşımıyorsa **karar gibi
okunur**, ve kararlar bir daha açılmaz.

Tehlikeli placeholder çirkin olan değil, **makul olandır.** Bitmiş, lisanslı ve
palet uyumlu bir varlık yanlış rolde dururken hiç yarım görünmez. Aşağıdaki
dört satır makullüklerine göre sıralandı; ilk satır en tehlikeli olandır.

Ödeme sütunundaki basamaklar kaynak bulma sırasının basamaklarıdır:

```
BASAMAK 1  zaten iceri alinmis
BASAMAK 2  icerdekinden turetilebilir
BASAMAK 3  zaten lisansli ve KAYITLI bir pakette
BASAMAK 4  yeni paket
```

Bu depoda **3. basamak AÇIKTIR**, çünkü `Assets/Art/THIRD_PARTY_ASSETS.md`
gerçek bir sanat manifestidir: üç Kenney paketi (Tiny Battle 1.0, Tiny Dungeon
1.0, Tiny Town 1.1) paket adresi, arşiv SHA-256'sı, tam lisansı (üçü de CC0 1.0)
ve okunmuş lisans metniyle kayıtlı, zorunlu atıf yok. Manifest tutmayan bir
depoda her boşluk doğrudan 4. basamağa sıçrardı.

### P1 · Kışla bir DEPO giyiyor

```
NE DURUYOR         friendly_command_depot_tile_0045 (komuta deposu)
NEYIN YERINE       tur sozlugunun 19. maddesi: KISLA gorseli
OYUNCU NE
YANLIS OKUR        Kisla bu tahtadaki TEK asker basan kucuk bina, ve
                   oyuncu onu bir DEPO olarak goruyor. Karargah ile Kisla
                   arasindaki fark ekranda yalnizca BOYUT: 1,25 hucreye
                   karsi 1,15 hucre. Iki bina birbirinden ancak yan yana
                   dururlarsa ayirt edilebiliyor.
NEYLE ODENIR       BASAMAK 3 -- Tiny Battle arsivi kayitli ve SHA-256'si
                   yazili; ayni bina satirindan bir kisla/baraka karosu
                   kesilir. BASAMAK 1 kapali: elde bagsiz bir dost bina
                   karosu yok. BASAMAK 2 kapali: palet takasi RENGI
                   degistirir, SILUETI degil, ve burada eksik olan siluet.
```

Aynı borcun aynadaki eşi: `Structure_DusmanKislasi`,
`enemy_command_depot_from_tile_0045` (türetilmiş dosya) giyiyor. Dost taraf
ödendiği gün düşman tarafı **BASAMAK 2** ile ödenir, çünkü depo bu projede
zaten bir kez palet takasıyla düşman rengine çevrildi ve yöntem
`Assets/Art/Derived/DERIVED-ASSETS.md`'de ölçülmüş hâlde yazılı.

### P2 · Karargâh doğru karoyu giyiyor, karonun ADI yanlış

Bu satır ötekilerden ayrı bir sınıftır ve burada olması gerekiyor.

```
NE DURUYOR         friendly_industrial_pump_tile_0048
NEYIN YERINE       hicbir seyin; karo DOGRU karo
OYUNCU NE
YANLIS OKUR        HICBIR SEY -- ekran dogru. Yanlis okuyan OKUYUCU.
NEYLE ODENIR       gorsel borcu YOK, isimlendirme borcu VAR
```

Ölçüm şudur. Tiny Battle'ın karo tablosunda düşman satırı dost satırının tam 18
karo altında duruyor: `47/65`, `49/67`, `50/68` üçü de birebir 18 fark veriyor.
Aynı adım `48/66` çiftini de eşliyor, ve o çift ölçüldü:
`friendly_industrial_pump_tile_0048` ile `enemy_headquarters_tile_0066` **aynı
alfa maskesini** taşıyor, 256 pikselin 256'sında eşleşiyorlar, ikisinin de opak
piksel sayısı 223. Bu maske dolu bir kare değil, yani eşleşme kendiliğinden
değil: dört çiftin dördü de opak sayılarında birebir tutuyor (211, 206, 223,
178).

Yani tek bir bina iki takım renginde duruyor, ve **dost kopyasına "sanayi
pompası", düşman kopyasına "karargâh" adı verilmiş.** Oyun onu karargâh olarak
kullanıyor (`Assets/Editor/SceneSetupTool.cs:82`), ve bu doğrudur.

Bu neden bir borçtur: **bir alan doğru doldurulmuş ama YANLIŞ okunuyor ve
hiçbir belirti vermiyor.** Bir sonraki okuyucu `FriendlyHq` sabitinin bir
pompaya işaret ettiğini görecek, bunu §4'ün öteki satırları gibi bir placeholder
sanacak ve **düzeltmesi gerekmeyen bir şeyi düzeltmeye kalkacak.** Öksüz
`Structure_IndustrialPump` tanımı da tam olarak bu yanlış okumanın bir turdan
kalmış izidir.

Ödemesi bir dosya adı değişikliğidir ve GUID'i koruyan bir yeniden adlandırma
ister; `.meta` dosyası ada değil dosyaya bağlıdır, o yüzden isim Unity içinde
değiştirilirse bağlar kırılmaz.

### P3 · Uçan cisim bir ASA

```
NE DURUYOR         support_staff_tile_0130 (mavi kristalli asa, TinyDungeon
                   EKIPMAN satiri)
NEYIN YERINE       tur sozlugunun 10. maddesi: ucan cisim / ok
OYUNCU NE
YANLIS OKUR        Menzilli vurus havada donen bir ASA gosteriyor, yani
                   ekran "bu birim bir buyu firlatti" diyor. Tahtada
                   buyu diye bir sey YOK; menzilli birim bir top arabasi
                   ve attigi sey bir mermi olmali.
NEYLE ODENIR       BASAMAK 3 -- Tiny Battle arsivi kayitli; ayni paketten
                   bir mermi/ok karosu kesilir. BASAMAK 1 kapali: elde
                   bagsiz ucan cisim karosu yok. BASAMAK 2 acik ama zayif:
                   ui_white_square_4x4 renklendirilip kucultulerek bir
                   nokta mermi yapilabilir, ve bu paletten kopar.
```

Bu satırın kaynağı `Assets/Editor/SceneSetupTool.cs:93`'te duruyor ve şunu
yazıyor: `OK GÖRSELİ ELDE YOK; TinyDungeon ekipmanları arasından mavi kristalli
asa seçildi`. Gerekçe DOĞRUDUR ve borç DEĞİLDİR. Kılıç ile çekicin 16 pikselde
havada dönen bir cisim olarak okunmadığı ölçülmüş bir olgudur.

Eksik olan tek şey **geri ödeme koşuludur.** Gerekçe onu taşımadığı için
seçim bir karar gibi duruyor, ve bugün depoda hiçbir satır bir ok istemiyor.
Yukarıdaki `NEYLE ÖDENİR` alanı o eksik cümledir.

### P4 · Top arabası bir NAKLİYE KAMYONU

```
NE DURUYOR         friendly_support_transport_tile_0131 (destek nakliye araci)
NEYIN YERINE       tur sozlugunun 20. maddesi: topcu govdesi
OYUNCU NE
YANLIS OKUR        Top Arabasi tahtanin en sert tek vurusu (12 hasar) ve en
                   uzun menzili (3 hucre); ekranda bir NAKLIYE aracidir.
                   Namlu yok, yani birimin tehlikeli oldugu gorselden hic
                   gelmez. Dusman esi (enemy_heavy_vehicle_tile_0158) CIFT
                   NAMLULU, yani ayni rol iki tarafta iki ayri sey okutuyor.
NEYLE ODENIR       BASAMAK 3 -- Tiny Battle arsivi kayitli; dost renk
                   satirindan 0158'in ayna karosu kesilir. 18 karoluk satir
                   adimi §4/P2'de olculdu, yani aday karo aritmetikle
                   bulunabilir ve arsiv acilarak DOGRULANMALIDIR.
```

Bu satırın kaynağı `Assets/Editor/SceneSetupTool.cs:71`'de ve gerekçesi de
doğrudur: elde kalan tek simetrik ikili bu iki araçtı. Yine eksik olan tek şey
geri ödeme koşuludur.

---

## 5 · Büyücü hattı

Operatör bir "büyücü" birimi eklemeyi düşünüyor. Bu bölüm o işin bugünkü
maliyetini ölçer.

### 5.1 Kaç satır kod ister — ÖLÇÜLDÜ: üç

Cevap tahmin edilmedi, kod okundu. Bir birim eklemek için değişen dosya
sayısı **bir**, ve o dosya `Assets/Editor/SceneSetupTool.cs`.

Sebebi üç ölçülmüş olguda yaşıyor:

1. **Gövde görseli tanım dosyasından geliyor.**
   `Assets/Game/Unity/ProductionDirector.cs:561` gövde sprite'ını
   `pendingUnitAsset.Icon`'dan okuyor. Yani birim türü başına bir `if` yok.
2. **Tek bir birim prefabı var.**
   `Assets/Game/Unity/BoardAdapter.cs:138` tek bir `unitPrefab` alanı taşıyor.
   Yani yeni birim yeni prefab istemiyor.
3. **Palet yalnızca YAPI listeliyor.**
   `Assets/Editor/SceneSetupTool.cs:997` on yapı adı sayıyor ve hiç birim adı
   saymıyor. Birimler üretici yapının listesinden geliyor, yani yeni birim
   palet düzenlemesi istemiyor.

Değişiklik:

| Ne | Nerede | Satır |
|---|---|---|
| Sprite yolu sabiti | `SceneSetupTool.cs`, 68. satır civarındaki blok | **+1** |
| `UnitBlueprint(...)` çağrısı | `SceneSetupTool.cs:392` `EnsureBlueprints()` gövdesi | **+2** (komşularının biçimiyle iki satıra sarılıyor) |
| Üretici yapının birim dizisi | `SceneSetupTool.cs:442` `new[] { piyade, kesif }` | **0 yeni satır, 1 düzenlenen satır** |

**Toplam: 3 yeni satır, 1 düzenlenen satır, 1 dosya.**
`Assets/Game/` altında **sıfır satır**. Yeni tip yok, yeni alan yok, yeni
prefab yok, elle YAML yok.

Projenin kendi kuralı düşman tarafın aynada birebir eşlenmesini istiyor
(`SceneSetupTool.cs:428` civarındaki gerekçe: iki taraf arasındaki tek fark renk
olmalı). O kural tutulursa sayı **6 yeni satır, 2 düzenlenen satır** olur.

### 5.2 Gövde sprite'ı hangi basamaktan gelir — BASAMAK 3

**BASAMAK 1 KAPALI.** Bugün 38 PNG'nin 34'ü bağlı, atıl dördün hiçbiri bir
gövde değil: biri çöp simgesi, biri kalp, ikisi ekipman. Elde bağsız bir dost
gövde karosu yok.

**BASAMAK 2 KAPALI, ve gerekçesi bu projede ölçülmüş.** `Derived/` altındaki
türetme yöntemi bir **palet takasıdır**: PLTE girdileri yeniden yazılıyor,
piksel indeksleri hiç dokunulmadan kalıyor. Yani türetme RENGİ değiştirir,
SİLUETİ değiştiremez. Bir büyücü, mor bir piyade değildir; asalı ve cübbeli
bir siluet ister. Türetme bu farkı üretemez.

Bu basamağın bir yerde AÇIK olduğunu da yazmak gerekiyor: büyücünün DÜŞMAN eşi
BASAMAK 2 ile ödenir, çünkü orada istenen şey tam olarak bir renk takasıdır.

**BASAMAK 3 AÇIK.** `Assets/Art/THIRD_PARTY_ASSETS.md` Kenney Tiny Dungeon
1.0'ı şu alanlarla kaydediyor: ürün sayfası, arşiv adresi, arşiv SHA-256
(`C109438AB06F65FD80F9B2686A4CF9C7C11DC64444B47333EC71D602F8BB5FC7`), tam lisans
**CC0 1.0 Universal**, okunmuş CC0 metni, ve arşiv içi `License.txt`'nin
kendisi. Zorunlu atıf **yok**. Bu paketten bu depoya zaten üç dosya alındı.

Sonuç: **BU HATTIN LİSANS TARAFINDA YENİ BİR ARAŞTIRMA BORCU YOKTUR.** Paket
kayıtlı, hash yazılı, lisans metni okunmuş, ticari ve portföy kullanımı açık.

Kapanmayan tek şey karo KİMLİĞİDİR ve bu bir araştırma değil, bir bakıştır.
Tiny Dungeon arşivi bu depoda **yok**; depo altında hiçbir `.zip` yok ve
manifestin andığı `parallel_sessions/` kanıt dizinleri de bu depoda yok. Bu
depodan alınmış üç dosyanın üçü de `Equipment/` altında ve üçü de ekipman, yani
paketin karakter satırı hiç içeri alınmamış.

> **Bu yüzden hiçbir karo numarası bu belgede YAZILMADI.** Arşiv açılmadan
> yazılacak her `tile_XXXX` uydurma olurdu.

### 5.3 Manifeste eklenecek satırın TASLAĞI

Aşağıdaki satır bir **TASLAKTIR.** Bu şerit `Assets/` altına yazmıyor; bu
satırın manifeste konması manifest sahibinin işidir.

Hedef tablo `THIRD_PARTY_ASSETS.md` içindeki **"Exact file ledger"** tablosudur
ve beş sütunu vardır:

```
| Pack / source ID | Intended role | Imported file | SHA-256 | Palette change |
```

Taslak satır:

```
| Tiny Dungeon `tile_<DOGRULANMADI>.png` | Friendly spellcaster body | `Assets/Art/ThirdParty/Kenney/TinyDungeon/Characters/friendly_spellcaster_tile_<DOGRULANMADI>.png` | <DOSYA ELDE OLMADAN HESAPLANAMAZ> | None |
```

Üç alan bilerek boş bırakıldı ve her birinin sebebi ayrı:

| Alan | Neden boş |
|---|---|
| `tile_<…>` | Tiny Dungeon arşivi bu depoda yok; karo numarası ancak arşiv açılınca okunur |
| Dosya adı | Karo numarasını taşıyor, o yüzden aynı sebeple bekliyor |
| `SHA-256` | **Bir hash ancak dosyanın baytları elde varken hesaplanır.** Dosya içeri alınmadan bu alan doldurulamaz; uydurma bir hash manifestin bütün değerini bitirir |

Doldurulabilen alanlar şimdiden yazılabilir: `Intended role` **Friendly
spellcaster body**, `Palette change` **None** (kaynak dosya düzenlenmiyor).

Manifestin ayrıca **kendi ölçülmüş bir boşluğu var** ve büyücü satırı o boşluğu
büyütmemeli: `Exact file ledger` tablosu 17 dosyayı hash'iyle kaydediyor, oysa
`ThirdParty/` altında **32 PNG** var. 2026-08-27'de eklenen on beş karo rol ve
karo numarasıyla yazılmış, **SHA-256'sız**. Yeni satırın hash'siz eklenmesi bu
oranı 15/33'e çıkarır, ve o gün "bu dosya gerçekten o arşivden mi geldi" sorusu
on altı dosya için cevapsız kalır.

### 5.4 Editör adımları — ÖNCE SORU: bunu bir araç yapabilir mi

Rol gereği önce bu soru soruldu, çünkü aracın yapabildiği bir sürükleme
operatöre hiç verilmemelidir.

`Assets/Editor/SceneSetupTool.cs` okundu. Cevap: **EVET, araç bunu yapabilir.**
Ölçüsü şu iki üye:

- `Assets/Editor/SceneSetupTool.cs:392` `EnsureBlueprints()` tanım dosyalarını
  kendisi üretiyor; dosya varsa alanları **yeniden yazıyor**, erken dönmüyor.
- `Assets/Editor/SceneSetupTool.cs:320` `ConfigureSpriteImports()` önce
  `AssetDatabase.Refresh()` çağırıyor, sonra `Assets/Art` altındaki **her**
  dokuyu tarayıp içe aktarma ayarlarını düzeltiyor. Yani Unity odakta değilken
  klasöre bırakılmış bir PNG de bu geçişte doğru ayarlarla içeri giriyor.

**Bu yüzden elle Inspector adımı YAZILMADI.** Aşağıdaki liste araca eklenecek
satırları ve operatörün yapacağı iki fiziksel işi sayar.

Satırların gideceği yerler: sprite yolu sabitleri
`Assets/Editor/SceneSetupTool.cs:68` civarındaki blokta, tanım çağrıları
`Assets/Editor/SceneSetupTool.cs:392` gövdesinde, Karargâh'ın birim dizisi
`Assets/Editor/SceneSetupTool.cs:442` satırında duruyor.

1. Doğrulanmış karoyu `Assets/Art/ThirdParty/Kenney/TinyDungeon/Characters/` klasörüne kopyala.
2. `SceneSetupTool.cs` içindeki sprite yolu sabitleri bloğuna `private const string FriendlyMage = Dungeon + "/Characters/<dosya adı>.png";` satırını ekle.
3. `EnsureBlueprints()` gövdesine `UnitBlueprintAsset buyucu = UnitBlueprint("Unit_Buyucu", "Büyücü", FriendlyMage, <can>, <hasar>, <menzil>, <bekleme>);` çağrısını ekle.
4. Karargâh'ın `new[] { piyade, kesif }` dizisine `buyucu` öğesini ekle.
5. Unity penceresinde `CountryBall > Sahneyi Kur (her şey)` menü öğesini çalıştır.

**Doğrulama:** `Assets/Game/Blueprints/Unit_Buyucu.asset` dosyası oluşmuş
olmalı, Inspector'da `Icon` alanı büyücü karosunu göstermeli, ve Play'e basıp
Karargâh'ın üretim panelinde `Büyücü` düğmesi görünmelidir.

Üç sayının (`<can>`, `<hasar>`, `<menzil>`) bu belgede boş bırakılması bir
eksiklik değil, sınır: bu sayılar tahtanın denge merdivenine aittir ve o
merdiven `EnsureBlueprints()` içindeki yorum bloklarında gerekçesiyle yazılı.
Bir görsel şeridi o merdivene sayı ekleyemez.

---

## 6 · Araştırma borcu

Kapanmayan her varlık için ya `performance-research` şeridine gidecek TAM soru
yazılır, ya da `gerekmiyor` yazılır. Boş alan bırakılmaz.

### A1 · Büyücü gövdesi

> **ARAŞTIRMA BORCU: gerekmiyor.**

Gerekçe ölçülmüş hâlde §5.2'de: paket kayıtlı, arşiv SHA-256'sı manifestte
yazılı, lisans CC0 1.0 ve metni okunmuş, zorunlu atıf yok, bu paketten zaten üç
dosya alınmış. Kalan iş bir araştırma değil, bir **bakıştır**: arşivi aç, dost
renk satırındaki büyücü/cübbeli karakter karosunu seç, numarasını oku.

### A2 · Menzil halkası

> **ARAŞTIRMA BORCU: gerekmiyor.**

Varlık zaten elde: `Assets/Art/Generated/ui_cell_frame_16x16.png`, ve
`GENERATED-ASSETS.md` bu dosyanın hedeflenen rollerini iki tane sayıyor —
`hovered/target cell highlight` ve `range indicator ring`. Bugün sahne yalnız
birincisini bağlıyor (`hoverFrameSprite`). İkinci rolün eksiği varlıkta değil,
**tüketicide**: halkayı çizecek bir üye yok. Bu bir kod işidir, BASAMAK 1'den
ödenir ve dışarıdan hiçbir şey istemez.

### A3 · Ok / mermi görseli (§4 P3'ün ödemesi)

> **ARAŞTIRMA BORCU: gerekmiyor.**

Tiny Battle arşivi kayıtlı ve SHA-256'sı manifestte doğrulanmış; 2026-08-27'de
aynı arşiv yeniden indirilip hash'i birebir tutmuş. Aday karonun seçimi arşivin
içinden okunur, dışarıdan araştırma istemez.

### A4 · Kışla görseli (§4 P1'in ödemesi)

> **ARAŞTIRMA BORCU: gerekmiyor.**

A3 ile aynı gerekçe: aynı arşiv, aynı bina satırı, kayıtlı hash.

### A5 · İsabet, ölüm ve zafer geri bildirimi

Sözlüğün 11., 13. ve 18. maddeleri. Bunlar bir DOSYA eksiği değil, bir
**mekanizma** eksiğidir ve her üçü de aynı sınıfa düşüyor: ekranda bir olayın
gerçekleştiğini söyleyen anlık bir işaret yok.

> **HANGİ ÖZELLİK:** Bir birim vurulduğunda, öldüğünde ve savaş bittiğinde
> oyuncu bunu ekranda ANINDA görür; bugün vuruşu ancak can çubuğunun kısalmasından
> çıkarıyor, ölümü birimin bir karede yok olmasından anlıyor, savaşın bittiğini ise
> hiç göremiyor.
> **NEREYE BAĞLANIR:** `Assets/Game/Unity/BoardAdapter.cs` → `AnnounceWinnerIfAny`
> **NE KIRAR:** Bugün zafer yalnızca `Debug.Log` ile duyuruluyor, yani oyunun
> bittiği olgusu Console penceresi açık olmayan bir oyuncuya hiç ulaşmıyor; savaş
> biter ve tahta hiçbir şey söylemeden durur.
> **KARARMETRE:** Bu özellik, `SpriteRenderer` renk yanıp sönmesi diye bir
> mekanizma HİÇ var olmasaydı da istenir miydi? EVET. "Kazandım mı" sorusunun
> ekranda bir cevabı olması bir oyunun asgari borcudur ve bunu hangi mekanizmanın
> çizdiği sorunun kendisini hiç değiştirmez.
> **NASIL DOĞURULUR:** Varlık tarafı BASAMAK 1'den ödeniyor, yani hiçbir yeni
> dosya istemiyor: `ui_white_square_4x4` renklendirilerek isabet parlaması ve
> tam ekran zafer perdesi olur, `ui_cell_frame_16x16` ise ölüm yerinde bir kare
> bırakır. Açılacak dosya `Assets/Game/Unity/BattleStatusView.cs`, çünkü sıra ve
> seçim satırlarının sahibi zaten o. Numaralı editör adımı: `CountryBall > Sahneyi
> Kur (her şey)` menü öğesi çalıştırılır ve `EnsureStatusBar` yeni satırı kurar.
> Kaba tahmin: 3-4 saat.
> **ARAŞTIRMA BORCU:** gerekmiyor.

### A6 · Öksüz tanım adası (§2.4)

> **ARAŞTIRMA BORCU: gerekmiyor.**

Bu bir varlık sorunu değil, bir temizlik borcudur. Sekiz dosyanın silinmesi
`SceneSetupTool` tarafından yeniden üretilmeyecekleri için güvenlidir, ve
silindikleri gün §2.3'ün ATIL sayısı 4'ten 6'ya çıkar: iki PNG tek bağını
kaybeder ve §2.4'te adıyla yazılıdır.

---

## 7 · Bu sayfanın göremedikleri

Yeşilken de geçerli olan sınırlar.

1. **GUID taraması bir bağın DOĞRU olduğunu göremez.** Bir sprite alanı dolu
   olduğu sürece bu sayfa onu BAĞLI sayar; §4'ün dört satırı bu körlüğün
   içinde yaşıyor ve oraya elle bakılarak konuldu.
2. **Hiçbir sayı bir görüntünün ANLAMINI ölçmez.** 1x1 saydam bir PNG bu
   sayfanın her testinden geçer.
3. **Sözlük listesi bir yargıdır, bir ölçüm değildir.** Türetme yöntemi (§1.2)
   sabittir; yirmi maddenin hangileri olduğu tartışılabilir ve tartışılmalıdır.
4. **Çalışma zamanında atanan bir sprite atıl görünür.** Bu tarama yalnız
   serileştirilmiş YAML'a bakıyor.
5. **Karo numarası aritmetiği (§4 P2) arşivi açmanın yerine geçmez.** 18
   karoluk satır adımı dört çiftte tuttu, ve bu bir kanıt değil güçlü bir
   göstergedir.

## İlgili

- Serileştirilmiş alanların Inspector sözleşmesi: [11-unity-penceresi-adim-adim.md](11-unity-penceresi-adim-adim.md)
- Yapı, üretim ve panel katmanının kurulumu: [12-unity-editor-baglama.md](12-unity-editor-baglama.md)
- Bugün olmayanların tetikleyici koşulları: [02-sonraki-asamalar.md](02-sonraki-asamalar.md)
- Bu ağacın yönlendirmesi: [README.md](README.md)
- Bu sayfanın kapısı: `Tools/check-asset-inventory.py`
