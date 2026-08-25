# Müfredat ve çatal kararı — 2026-08-24

***Bu dosya, bu ağaçta ÖNERİ yazan tek dosyadır.*** [`02-sonraki-asamalar.md`](02-sonraki-asamalar.md)
kendi künyesinde *"hiçbir şey önermez"* diyor ve o yasak yerinde duruyor: `02`
**projenin koduna** dair karar vermez. Buradaki karar ise projenin koduna değil,
**okuyanın yoluna** dairdir. İki ayrı yetki, iki ayrı dosya.

Ne yazar: hangi desenin bu projede baskısı var, hangisi ayrı bir projeye ait,
hangi gün başvuru yapılır, ve çatal ne zaman açılır.
Ne yazmaz: bir desenin nasıl uygulanacağını. O `02`'nin ve `01`'in işi.

---

## Soru neydi

Videolarda geçen on dokuz konu (yedi temel + on iki desen) bu projeye eklenmeli
mi, yoksa ayrı bir projede mi ele alınmalı — ve iş başvurusu hangi gün başlar.

---

## Ölçüm 1 · On dokuz konunun gerçek durumu

```
A  ZATEN KODDA VAR ve BELGELI                 01-koda-gomulu-desenler.md
   saf kural sinifi . durum makinesi (8 enum tipi) . Gozlemci (C# event)
   Flyweight'in ic durum yarisi . bilesim . sinir cevirmeni
   sonuc enum'u . akis sahibi . kimlik + yan tablo
   EKLENECEK BIR SEY YOK. okunacak var.

B  BELGEDE GERCEK BOSLUK                      iki tane, ikisi de ucuz
   UnityEvent       kodda 0 . BELGEDE DE 0
   Strategy deseni  ADIYLA hic gecmiyor
                    (61 sahte eslesme vardi: GridStrategy AD ALANI)

C  BU PROJEDE BASKISI YOK                     kodda olculdu
   Object Pool     Instantiate 5 . Destroy 1   devir yok, havuz bosa doner
   Singleton       static Instance 0           zaten reddedilmis, Asama 4
   Coroutine       IEnumerator 0
   async/UniTask   async/await 0
   interface       tanim 0
   Decorator . MVC/MVP . Command . Factory . Service Locator
```

Bu tabloyu reddeden şey bir görüş değil, bu ağacın kendi kuralıdır: **bir yokluk
bir özelliğe borçlanır, bir derse değil.** `Instantiate` beş kez geçen bir projeye
nesne havuzu koymak, kanamayan bir yarayı dikmektir.

---

## Ölçüm 2 · Grid tavan değil, karar katmanı tavan

Soru şuydu: bu proje kule savunmasına (yoldan gelen düşman, saldıran yapılar,
mermiler) evrilebilir mi, yoksa grid mantığı bir yere kadar mı gider.

```
ALTI KURAL SINIFI                    456 satir . grid atifi: SIFIR
  MovementRules . AttackRules . TargetingRules
  DamageRules   . HealingRules . ReviveRules
  hicbiri x, y, Cell ya da Board gormuyor -- saf, tasinabilir, TEST EDILMIS

SUREKLI ZAMAN OMURGASI               zaten var
  Tick(float deltaSeconds)  5 tipte: Battle . Combatant
                                     Structure . StructureLifecycle . UnitLifecycle

SIRA OMURGASI                        35 atif . TurnState / EndTurn
SUREKLI HAREKET                      0   MoveTowards / Lerp / velocity hic yok
```

Grid engel değil — Clash of Clans da grid, Kingdom Rush'ın yerleştirmesi de grid.
Engel **sıra tabanlı karar katmanı**, ve o katman 35 atıfta yoğunlaşmış,
kurallara hiç bulaşmamış.

***Genel ders:*** okuyan tavanı hep görünen şeyde arar (burada: grid). Genellikle
tavan **karar katmanıdır**, veri katmanı değil. Ölçmeden söylenen her cevap
görüştür.

---

## Karar · Üç seçenek, ölçülerek

| Seçenek | Hazırlık | Bağımlılık | Değer | Churn | Altyapı | Toplam |
|---|---|---|---|---|---|---|
| **A** · CountryBall'ı kule savunmasına evrilt | 3 | 2 | 3 | 2 | 4 | **14/25** |
| **B** · Sıfırdan yeni repo | 2 | 5 | 4 | 4 | 1 | **16/25** |
| **C** · Çatalla, saf katmanı taşı, karar katmanını sil | 5 | 5 | 5 | 4 | 5 | **24/25** |

**Seçilen: C.**

**A neden reddedildi:** başvuru yapılırken portföy parçası ortadan ikiye bölünür.
Test gövdesi sıra tabanlı; `TurnState` çıkarıldığı an yeşil biter ve haftalarca
kırmızıda kalınır. Mülakat çağrısı tam o haftaya denk gelir.

> ***İKİ TEST SAYISI VAR ve ikisi de doğru — farklı şey sayıyorlar.***
> **451 nitelik**, statik sayım: `grep -rho '\[Test\]'` 327 artı
> `grep -rho '\[TestCase('` 124. **442 koşan test**, Test Runner'ın son
> kaydedilen koşusundan. Aradaki **9 fark AÇIKLANMADI**: `[Ignore]`,
> `[Explicit]`, `[TestCaseSource]` ve PlayMode derlemesi arandı, dördü de
> sıfır çıktı. En olası açıklama 442'nin daha eski bir commit'te ölçülmüş
> olması. ***Çözen eylem:*** `Tools/run-editmode-tests.ps1` yeniden koşturulur
> ve sayı oradan alınır. O güne kadar bu belgede test sayısı
> ***"451 nitelik (statik)"*** olarak anılır.

**B neden reddedildi:** 456 satır ölçülmüş, test edilmiş, grid'den bağımsız kural
çöpe gider. Ayrıca `Docs/deep/` ağacı, dokuz kapı ve okuma sırası da gider —
asıl değerli varlık zaten onlar.

---

## Çatalın ilk commit'i EKLEMEZ, SİLER

```
SIL   TurnState.cs . TurnRules.cs . EndTurn zinciri     35 atif
SIL   sira tabanli testler                              (yeniden yazilacak)
TUT   alti kural sinifi                                 456 satir, 0 grid atifi
TUT   Tick(float) omurgasi                              5 tip
TUT   Docs/deep agaci . Tools/check-*.py . okuma sirasi YONTEM, koddan degerli
```

Silmeyi eklemeden önce göndermenin üç sebebi var. Çatalı dürüst tutar — aylarca
ölü sıra kodu taşınmaz. Karar gibi okunan temiz bir diff üretir. Ve mülakatta
herhangi bir desen benimsemesinden iyi bir hikâyedir.

---

## Kule savunması hangi baskıları doğuruyor

Bir ikinci proje ***"desen pratiği yapılacak yer"*** diye seçilmez. Bir **alan**
seçilir, ve o alanın doğurduğu baskılar hangi desenleri kaçınılmaz kılıyorsa
onlar gelir.

```
TD ozelligi                     dogan gercek baski           desen
──────────────────────────────────────────────────────────────────────────────
yol boyunca ilerleyen dusman    surekli konum + hiz          Tick omurgasi (VAR)
birden cok YAPI davranisi       ortak sozlesme               INTERFACE  ilk gercek baski
birden cok MERMI turu           davranis degistirme          Strategy
dusman dalgalari                dogum/olum devri             Object Pool  5 -> yuzlerce
hedef secimi cesitleri          degistirilebilir kural       Strategy
yapi yukseltme                  sarmalayarak guclendirme     Decorator
hazirlik / dalga / bitis        oyun durumu                  State (ZATEN VAR)
ses + skor + basarim            cok dinleyici                Event Bus  1 -> N
```

Sekiz baskının altısı bugün CountryBall'da **sıfır**.

***Üçü kule savunmasında da baskısız kalır:*** Singleton, Service Locator,
MVC/MVP. Bu bir eksiklik değildir ve öyle yazılmaz.
[`02-sonraki-asamalar.md`](02-sonraki-asamalar.md) Aşama 4 zaten Singleton'ı
*"ve reddedilişin kendisi"* diye adlandırıyor. Reddedilme gerekçesini
anlatabilmek, uygulamış olmaktan ağır basar.

---

## Çatal not defteri — çatal açıldığı gün açılacak notlar

Buraya, çatal öncesinde ortaya çıkan ama çatal açılmadan uygulanamayacak
kararlar yazılıyor. Her not bir **ölçüye** dayanıyor ve hiçbiri bu projeye
uygulanmıyor.

### N1 · `async` / `await` / UniTask savaş döngüsüne KONMAZ

`async` *"aynı anda çok şey olsun"* için değil, ***"CPU dışında bir şeyi
bekle"*** için vardır: ağ, disk, asset yükleme. Kule savunmasının savaş
döngüsüne konursa, bu defterin reddettiği hatanın aynısı tekrarlanır — baskısı
olmayan bir mekanizmayı benimsemek.

```
BASKISI OLAN yerler        catalda menu + kayit varsa MESRU
   Addressables ile asset yukleme
   kaydet / yukle
   skor tablosu . reklam . IAP

BASKISI OLMAYAN yer
   savas dongusunun kendisi     <- buraya konmaz
```

Sebebi ölçülü: bugünkü savaş zinciri baştan sona tek iş parçacığında koşuyor ve
bu bir eksiklik değil. Ayrımın tamamı
[`../deep/konular/08-motor-cagri-dongusu.md`](../deep/konular/08-motor-cagri-dongusu.md)
Yedinci durak'ta, ***"İki ayrı paralel"*** başlığı altında yazılı.

### N2 · Havuz, bugün ulaşılamayan bir tehlikeyi CANLI hâle getirir

Bugün bir birim `RemoveUnit` ile çıkarıldığında ona giden üretim yolu **kalmıyor**
— tek sahip `Battle.combatants` sözlüğü. Bu yüzden *"sökülmemiş abonelik"*in
birinci dalı (silinmiş birim yayın yapar) bugün **erişilemez**.

Nesne havuzu tam olarak bunu değiştirir: havuz, sözlükten çıkmış bir `Combatant`
için **ikinci bir sahip** olur.

```
BUGUN                       HAVUZ EKLENDIGI GUN
Combatant --- combatants    Combatant --- combatants
                                      \-- havuz          <- ikinci sahip
RemoveUnit -> hicbir sahip  RemoveUnit -> havuz tutuyor
             = ulasilamaz                = ULASILABILIR
```

Ve daha pahalıya patlar: havuz aynı nesneyi geri verdiğinde **eski abonelik hâlâ
listededir** ve aynı olay iki kez işlenir. Yani çatalda havuz yazılırken
`-=` disiplini bir tercih değil, ilk gün kurulması gereken bir sözleşmedir.
Ölçüsü ve iki koşulu:
[`../deep/konular/01-olay-zinciri.md`](../deep/konular/01-olay-zinciri.md).

### N3 · ECS için editör sürümü ENGELDİR

```
bu repo            Unity 2021.3.45f2      ProjectSettings/ProjectVersion.txt
Entities 1.0       Unity 2022.3+ ister
```

Üçüncü çatal (ECS/DOTS) bu editörde **açılamaz**. Sürüm yükseltmesi çatal
planının bir adımıdır, sonradan fark edilecek bir ayrıntı değil.

### N4 · Yükseltme ekseni — hangisi öğretir, hangisi yalnız karmaşıklaştırır

```
MAKINE YUKSELTMESI          ALINIR
   1 atis -> 2 atis -> daha hizli -> sersemletme / yavaslatma / puskurtme
   dogurdugu baski: davranis degistirme (Strategy) + sarmalayarak guclendirme (Decorator)
   yani her yukseltme seviyesi bir desen baskisi URETIYOR

KULEYE SAVASCI YERLESTIRME  ERTELENIR
   dogurdugu baski: yeni desen YOK
   getirdigi sey: kule ile savasci omurleri arasinda baglasim
   yani karmasiklik ekliyor, ogreti eklemiyor
```

### N5 · Varlık tedarik sırası — ve manifesti taşımanın sebebi

Çatalda *"buna görsel lazım"* dendiği an refleks yeni paket aramaktır. O **son**
basamaktır, ilk değil.

```
1  ZATEN ICERI ALINMIS      once projenin kendi sanat agacini ara
2  ICERIDEKINDEN TURETILIR  palet takasi . tint . dondurme . fill
                            KANIT zorunlu: IHDR/tRNS/acilmis IDAT birebir,
                            renkler paketin KENDI semasindan olculur
3  LISANSLI ve KAYITLI PAKETTE HALA VAR
                            paketler yuzlerce karo tasir, proje bir avuc alir
                            bu basamak MANIFEST yoksa GORUNMEZ
4  YENI PAKET               ancak burada kaynak arama baslar, ve tam bir
                            lisans incelemesini de beraberinde surukler
```

***Üçüncü basamağı var eden şey manifesttir.*** Nereden geldiğini yazmadan
varlık içeri alan bir proje o basamağı sessizce silmiştir ve her boşlukta
2'den 4'e atlar.

Bu projede manifest var ve çatala **taşınacak** varlıkların arasındadır:
[`../../Assets/Art/THIRD_PARTY_ASSETS.md`](../../Assets/Art/THIRD_PARTY_ASSETS.md).
Paket başına şunları taşıyor: ürün sayfası, arşiv adresi, arşiv SHA-256, tam
lisans adı, arşiv içi lisans metninin okunduğu teyidi, zorunlu atıf dizesi
(bu üç pakette: ***yok***), ve bilerek alınmayanların gerekçesi.

**Hükümde basamağı adlandır.** *"Buna görsel lazım"* ile *"4. basamak: elimizdeki
hiçbir paket bunu karşılamıyor"* farklı iddialardır, ve yalnız ikincisi yeni
paket aramayı meşrulaştırır.

---

## Zaman çizelgesi

```
SIMDI -> ESIK      yalniz CountryBall . okuma sirasinin 11. adimi . ~3 oturum
                   paralel is YOK -- bolunme burada iki yarim repo uretir

ESIKTE             1. CountryBall'i DONDUR
                   2. iki canli hatayi + bayat sahneyi kapat   demo tusu
                   3. BASVURULARI BASLAT
                   4. AYNI GUN catali ac

ESIKTEN SONRA      basvurular haftalarca surer . catal o haftalarin icinde
                   kalan okuma (07, 06, 04, 05, 09) catalin baskisiyla okunur
```

Tek kural: **eşiğe kadar bölünme.** Eşikten sonra paralel gitmek ise doğrudur,
çünkü başvuru süreci zaten bekletiyor.

---

## Eşik nedir — sayı değil, cevaplayabilirlik

Belgeye bakmadan şu beşi anlatabiliyorsan eşik geçilmiştir.

```
1  Bir tiklamadan ekrandaki degisiklige kadar zincir kac durak,
   her durak ne EKLIYOR
2  Battle neden sozluk tutuyor -- ve tutmasaydi ne patlardi
3  Bu projede neden hic interface yok -- ve hangi gun ilki yazilir
4  Invoke'tan sonraki satir neden calismayabilir -- yigin CIZEREK
5  Bir deseni REDDETME gerekcesi: "MoveAction neden Command degil"
```

Beşi de [`00-okuma-sirasi.md`](00-okuma-sirasi.md)'nın **11. adımına** kadar
okumakla kapanır. Ölçüldü: `konular/01-olay-zinciri.md` ADIM 10'da,
`dil/06-delege-arka-taraf.md` ise `dil/04` ile birlikte ADIM 11'de duruyor —
yani eşik, 4. soruyu (`Invoke`tan sonraki satır) kapatan adımdır. On beş
adımlık, beş oturumluk yolun yaklaşık üçte ikisi.

Desen sayısı, dosya sayısı ve özellik sayısı — üçü de daha kötü ölçütlerdir.

---

## İlgili

- Kodda doğrulanmış dokuz desen: [`01-koda-gomulu-desenler.md`](01-koda-gomulu-desenler.md)
- Tetikleyici koşullar (öneri yok): [`02-sonraki-asamalar.md`](02-sonraki-asamalar.md)
- Okuma sırası ve eşik adımı: [`00-okuma-sirasi.md`](00-okuma-sirasi.md)
- Kuralın kalıcı hâli: `unity-game-dev-mentor` K51 —
  gövdesi `references/portfolio-and-interview.archive`
