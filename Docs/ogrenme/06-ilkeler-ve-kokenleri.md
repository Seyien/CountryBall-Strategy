# İlkeler ve kökenleri — adı konmamış olanı adlandırmak

***Bu dosyanın tezi tek cümle: **bu proje aşağıdaki ilkelerin çoğunu zaten
uyguluyor, ama hiçbirinin adını koymuyor.*****

Fark küçük görünür, değil. `Docs/deep/` ağacı 12.600 satır boyunca bir
mekanizmayı **anlatıyor**; okuyan onu öğreniyor, kodda tanıyor, savunabiliyor.
Sonra mülakatta soru şu biçimde geliyor:

> *"Law of Demeter'ı bilir misin?"*

Ve saatlerdir tam olarak onun ihlalinden kaçınmayı okumuş olan kişi **hayır**
diyor. Çünkü okuduğu şeyin adı ona hiç söylenmedi.

Bu, [`README.md`](README.md)'nin kurduğu boşluğun **ikinci yarısı**. Birinci
yarısını [`01-koda-gomulu-desenler.md`](01-koda-gomulu-desenler.md) kapattı:
projede duran dokuz **desen** adlandırıldı. Ama desen bir yapıdır; ilke bir
**ölçüttür**. Desen "burada ne var" sorusuna cevap verir, ilke "neden böyle
olması gerektiği" sorusuna. Mülakatta ikincisi sorulur.

## Ölçülen boşluk — grep ile, tahmin değil

Bu dosya yazılmadan önce `Docs/` ağacının tamamı arandı:

```
SOLID beş harf          KAPALI      ogrenme/01'de dokuz desenin her birinde
                                    "SOLID KARŞILIĞI" satırı var (S×4 O×2 L×1 I×2 D×3)

Law of Demeter          0 dosya     Tell, Don't Ask          0 dosya
YAGNI                   0 dosya     Fail fast                0 dosya
DRY                     0 dosya     Separation of concerns   0 dosya
Dependency Injection    0 dosya     Kalıtım yerine bileşim   1 dosya (yalnız ogrenme/01 §5)

Tek doğruluk kaynağı    >> kavram DÖRT belgede kullanılıyor, hiçbirinde ADLANDIRILMIYOR <<
```

En keskin satır sonuncusu. `Assets/Game/Battle/Battle.cs:526`'daki yorum
kavramı tam olarak tarif ediyor ve adını hiç söylemiyor:

```
Assets/Game/Battle/Battle.cs:526             // kararı değil, ikinci bir doğruluk kaynağı yaratma kararıdır.
```

"İkinci bir doğruluk kaynağı" cümlesi, "single source of truth" ilkesinin
**tanımının olumsuzu**. Kavram kodda yaşıyor, adı hiçbir yerde yazmıyor.

## Bu dosya ne YAPMAZ

***Mekanizmayı **tekrar anlatmaz**.*** Aşağıdaki dokuz ilkenin çoğunun
mekanizması `Docs/deep/` ağacında satır satır anlatılmış durumda ve bu dosya
oraya **link verir**, oradaki cümleyi kopyalamaz. Bu dosyanın işi üç şey:

1. İlkenin **adını** koymak — İngilizce ve Türkçe.
2. **Kökenini** vermek — ve doğrulanamayanı `kaynak doğrulanmadı` diye
   işaretlemek.
3. **Mülakat cevabını** yazmak — bu projeden somut örnek taşıyan iki uzunlukta.

## Her ilkenin altı alanı

| Alan | Ne yazar |
|---|---|
| **ADI VE KÖKENİ** | İngilizce adı, Türkçe karşılığı, kim/ne zaman ortaya attı. Doğrulanamayan `kaynak doğrulanmadı` |
| **NE DER** | Tek cümle, jargonsuz |
| **BU PROJEDE NEREDE** | `dosya:satır` + alıntı. Uygulanmış mı, ihlal mi edilmiş mi |
| **ÖLÇÜSÜ** | Okuyucunun koşturabileceği deney: ihlal edilse NE gözlenir |
| **NE ZAMAN UYGULANMAZ** | ██ Sınırsız ilke aşırı uygulanır. Bu alan zorunlu ██ |
| **MÜLAKAT CEVABI** | Kısa (30 saniye) ve genişletilmiş (2 dakika); ikisi de bu projeden örnek taşır |

Sıra `Docs/deep/README.md`'nin *"Nasıl yazılır"* şekline uyuyor: önce baskı,
sonra ad; önce ölçü, sonra iddia.

## Dokuz ilke — tek bakışta

Bu dosya uzun ve baştan sona okunmak zorunda değil. Mülakat provası yapıyorsan
**10. bölümden başla**: ilkeleri saymak değil, çatışmayı çözmek ayırt edicidir.

| # | İlke | Bu projedeki durumu | Tek cümlelik yeri |
|---|---|---|---|
| 1 | Fail fast | uygulanmış | akış sahibinde 13 `throw`, iki kanal ayrılmış |
| 2 | Tell, Don't Ask | uygulanmış · ihlal bulunamadı | dört sonuç enum'u |
| 3 | Tek doğruluk kaynağı | uygulanmış | tahta tek sahipli, konum önbelleği reddedilmiş |
| 4 | Dependency Injection | uygulanmış · konteynersiz | parçalar kurucudan ve `[SerializeField]`'den |
| 5 | Law of Demeter | ██ ihlal VAR, bilinerek ██ | sıra devri zinciri, borç yazılı |
| 6 | Kalıtım yerine bileşim | uygulanmış | kalıtım toplam iki satır, ikisi de motor zorunluluğu |
| 7 | YAGNI | uygulanmış | altı mekanizma yazılı, hiçbiri uygulanmamış |
| 8 | DRY | doğru uygulanmış | kural tek yerde, aynı satır üç yerde ve bu ihlal değil |
| 9 | Separation of concerns | uygulanmış · derleyiciyle zorlanıyor | dört `.asmdef` |

***5. satır bu tablonun en dürüst yeri*** — dokuz ilkeden sekizi "uygulanmış"
diyor ve bir belge bunu yazınca inandırıcılığını kaybeder. Bulunan tek ihlal
işaretli, bedeli ölçülü ve neden kabul edildiği yazılı.

## ***Kökenler hakkında dürüst not***

Bu dosya **çevrimdışı** yazıldı: hiçbir birincil kaynağa (makale, kitap, arşiv)
bu oturumda erişilmedi. Aşağıdaki köken satırlarının hiçbiri bir kaynak
belgesine karşı doğrulanmadı; her biri **hatırlanan** atıftır. Bu yüzden her
ilkenin köken alanı bir **güven etiketi** taşıyor:

```
İYİ BİLİNEN     ad, kişi ve kaynak yaygın olarak bu şekilde anılıyor,
                ama bu oturumda BİRİNCİL KAYNAĞA BAKILMADI
TARTIŞMALI      atıf hakkında birden fazla anlatı dolaşıyor
KAYNAK DOĞRULANMADI   ilk kullanımı bilinmiyor ya da bu oturumda saptanamadı
```

***Mülakatta bir tarih ya da kişi adını **emin değilsen söyleme**.*** "Kim
söylemiş hatırlamıyorum ama ilke şunu der" cümlesi, yanlış bir tarih vermekten
her zaman daha güçlüdür. Aşağıdaki ilkelerin **projedeki karşılıkları**
doğrulandı; **kökenleri** doğrulanmadı ve bu ayrım kasıtlı olarak görünür.

---

## 1. Fail fast (erken patla)

**ADI VE KÖKENİ** — İngilizce *fail fast*; Türkçe "erken patla" ya da "hemen
başarısız ol". `İYİ BİLİNEN`: yazılım mühendisliği literatüründe adı Jim
Shore'un 2004 tarihli *Fail Fast* yazısıyla yaygınlaştı. Fikir daha eskidir ve
donanım/sistem mühendisliğinde başka bir anlamda da kullanılır. ***Bu oturumda
birincil kaynağa bakılmadı; tarihi mülakatta telaffuz etme.***

**NE DER** — Bozuk bir girdiyi sessizce kabul edip yola devam etmek yerine,
bozukluğun **doğduğu yerde** dur ve söyle.

**BU PROJEDE NEREDE** — ***Uygulanmış, ve projenin en yoğun uygulandığı yer
`BattleActions`.*** Sayı: bu tek dosyada **13 `throw` deyimi** var — 11'i
`ArgumentNullException`, 2'si `ArgumentException` — ve bunların 10'u iki özel
kapıdan geçiyor.

```
Assets/Game/Battle/BattleActions.cs:50       public static class BattleActions
Assets/Game/Battle/BattleActions.cs:65       throw new ArgumentNullException(nameof(battle));
Assets/Game/Battle/BattleActions.cs:82       Combatant attackerCombatant = RequireCombatant(battle, attacker, nameof(attacker));
Assets/Game/Battle/BattleActions.cs:375      private static Combatant RequireCombatant(Battle battle, Unit unit, string paramName)
Assets/Game/Battle/BattleActions.cs:379      throw new ArgumentException("The unit is not in this battle.", paramName);
Assets/Game/Battle/BattleActions.cs:389      private static void RequireCell(
Assets/Game/Battle/BattleActions.cs:392      if (!battle.TryGetPosition(unit, out x, out y))
```

***SAYIYI DÜZELTİYORUM*** — dolaşımda "13 `Require*` kapısı" diye bir sayı var
ve **yanlış**. Doğrusu: `Require` kelimesi bu dosyada 13 kez geçiyor, ama bunun
2'si tanım (`BattleActions.cs:375`, `:389`), 1'i bir yorum satırı (`:86` —
`RequireCombatant` bir gerekçe cümlesinde anılıyor, çağrılmıyor), ve **10'u
gerçek çağrı** — `RequireCombatant` 5 kez, `RequireCell` 5 kez. 13 olan şey
`Require` sayısı değil, `throw` sayısı. İki sayının aynı yere düşmesi tesadüf.

Kapı sınıflara da inmiş durumda, yani ilke tek dosyaya ait değil:

```
Assets/Game/Core/Combat/Combatant.cs:70      this.health = health ?? throw new ArgumentNullException(nameof(health));
Assets/Game/Core/Combat/Combatant.cs:72      AttackProfile = attackProfile ?? throw new ArgumentNullException(nameof(attackProfile));
Assets/Game/Core/Combat/AttackProfile.cs:49  public AttackProfile(int damage, int range)
Assets/Game/Core/MoveProfile.cs:50           public MoveProfile(int range)
Assets/Game/Core/PointerGesture.cs:127       public PointerGesture(float dragThreshold)
```

`Assets/Game/` altındaki 33 üretim dosyasında toplam **66 `throw` deyimi** var
ve 18 dosyaya dağılmış durumda. Bu bir üslup değil, bir sözleşme.

***İLKENİN ADI KONMAMIŞ AMA SINIRI ZATEN ÇİZİLMİŞ*** — Fail fast'in bu projede
"her hataya patla" biçiminde uygulanmadığı, kodun kendi cümlesiyle yazılı.
Ayrım ölçütü `BattleActions.cs:370-373`'te duruyor: *bu cevabı alan çağıran
yapacak bir şey bulabilir mi?* Bulabiliyorsa sonuç değeri, bulamıyorsa istisna.
Mekanizmanın tamamı burada, **tekrar edilmiyor**:
[`../deep/kod/Battle/BattleActions.md`](../deep/kod/Battle/BattleActions.md) —
*"İŞ BÖLÜMÜ: İSTİSNA ile SONUÇ DEĞERİ ÖRTÜŞMEZ, BÖLÜŞÜR"* bölümü. Dil tarafı:
[`../deep/dil/03-hata-bildirme-ve-dogrulama.md`](../deep/dil/03-hata-bildirme-ve-dogrulama.md).

**ÖLÇÜSÜ** — Deney: `Combatant.cs:70`'teki `?? throw`'u sil ve `health`'i
`null` geçir. Kurucu **sessizce** geçer. Patlama, ilk `CurrentHealth`
okumasında — muhtemelen üç katman ötede, `BoardAdapter`'ın bir log satırında —
`NullReferenceException` olarak doğar. Yığın izi seni `BoardAdapter`'a
gönderir; hatayı yapan yer ise `new Combatant(...)` yazan satırdır. ***Fail
fast'in kazandırdığı şey hata sayısı değil, hata ile **sebebi** arasındaki
mesafedir.***

**NE ZAMAN UYGULANMAZ** — ***Üç durumda***:

```
① OYUNCU HATASI İSTİSNA DEĞİLDİR
   "Menzil dışı bir hücreye tıkladın" bir programcı hatası değil, bir oyun
   olgusudur. Bu projede o yola dört sonuç enum'u bakıyor (2. ilke), istisna
   değil. İstisna atsaydı her tıklama bir try/catch gerektirirdi.

② ÜRETİM DÖNGÜSÜNDE HER KAREDE ÇALIŞAN DOĞRULAMA
   Bu projede henüz karşılığı yok — Tick yolunda doğrulama yapılmıyor —
   ama ilke sınırsız uygulandığında sıcak yola girer.
   >> HENÜZ YOK → ölçülmüş bir kare bütçesi (02-sonraki-asamalar.md · Aşama 6) <<

③ AĞ / DOSYA / KULLANICI GİRDİSİ SINIRI
   Dışarıdan gelen veri "bozuk olabilir" varsayılır; orada doğru cevap patlamak
   değil, reddetmek ve devam etmektir. Bu projede böyle bir sınır YOK:
   Assets/Game/ altında ağ, dosya ve serileştirme okuması hiç geçmiyor.
```

**MÜLAKAT CEVABI**

> **KISA (30 sn).** Fail fast, bozuk bir girdiyi kabul edip yola devam etmek
> yerine doğduğu yerde durdurmaktır. Benim savaş çekirdeğimde akış sahibi
> `BattleActions`, dört eylemin başında `null` ve "bu savaşta mı" sorularını
> istisnayla kapatıyor — tek dosyada 13 `throw`. Ama ilkeyi **her şeye**
> uygulamadım: oyuncunun yapabileceği bir hata istisna değil, sonuç değeri
> döndürüyor. Ayırıcı soru şu: bu cevabı alan çağıran yapacak bir şey bulabilir
> mi?

> **GENİŞLETİLMİŞ (2 dk).** Bağlam: `BattleActions` dört eylemi yürüten tek
> yer, ve iki farklı türde "hayır" üretmesi gerekiyordu. Gözlenen sorun şuydu:
> ikisini aynı kanala koyarsan çağıran ayırt edemiyor. `null` bir `Battle`
> geçmek bir **kod** hatası; menzil dışı bir hücreye tıklamak bir **oyun**
> olgusu. Seçtiğim mekanizma iki kanal: istisna ve sonuç enum'u, ve sırası
> sabit — önce bütün çağıran hataları, sonra bütün kurallar. En yakın
> alternatif her şeyi `bool` döndürmekti; kaybettiği şey ret **sebebi** oluyordu
> ve o sebebi çağıran kendi içinde yeniden hesaplamak zorunda kalırdı — yani
> kuralın ikinci bir kopyası doğardı. Kanıt: `BoardAdapter`'ın `ReactToAttack`
> ve `ReactToMove` metotları beş ret sebebine beş farklı satır yazıyor ve
> hiçbirini yeniden hesaplamıyor. Ödün: `throw` sıcak bir yolda çalışırsa
> maliyetli olur; bugün hiçbiri `Tick` yolunda değil, ama kare bütçesi ölçüldüğü
> gün bu kararı yeniden açarım.

---

## 2. Tell, Don't Ask (sor değil söyle)

**ADI VE KÖKENİ** — İngilizce *Tell, Don't Ask*; Türkçe "sorma, söyle".
`KAYNAK DOĞRULANMADI`: ilke nesne yönelimli tasarım çevrelerinde 1990'ların
sonundan beri dolaşıyor ve genellikle Pragmatic Programmer yazarlarına
(Andy Hunt, Dave Thomas) ve daha eski Smalltalk literatürüne bağlanıyor. ***İlk
kullanımı bu oturumda saptanamadı. Mülakatta atıf verme; ilkeyi tarif et.***

**NE DER** — Bir nesnenin **iç durumunu sorup** kararı dışarıda vermek yerine,
ona **ne istediğini söyle** ve kararı kendisine ver.

**BU PROJEDE NEREDE** — ***Uygulanmış, ve mekanizması dört enum.*** Çağıran
`BattleActions.Attack`'e "saldır" der; menzili, hedefin uygunluğunu ya da sıranın
kimde olduğunu **sormaz**. Cevap tek bir adlandırılmış değer olarak geri gelir:

```
Assets/Game/Core/MoveOutcome.cs:26           public enum MoveOutcome
Assets/Game/Core/Combat/AttackOutcome.cs:27  public enum AttackOutcome
Assets/Game/Battle/PlacementOutcome.cs:24    public enum PlacementOutcome
Assets/Game/Battle/ReviveOutcome.cs:25       public enum ReviveOutcome
```

Mekanizmanın tam anlatımı — sıfırıncı değerin neden RET olduğu, bir asmdef'in
bir enum değerini nasıl yasakladığı, ret değerlerini birleştirmenin bedeli —
burada ve **tekrar edilmiyor**:
[`../deep/konular/06-sonuc-enumlari.md`](../deep/konular/06-sonuc-enumlari.md).
Bu dosyanın tek katkısı ilkeye adını vermek.

***KARŞI ÖRNEK ARANDI VE BULUNDU*** — "önce sor, sonra karar ver" biçiminde
yazılmış iki yer var ve **ikisi de savunulabilir**:

```
Assets/Game/Unity/BoardAdapter.cs:1079       private string DescribeCondition(Unit unit)
Assets/Game/Unity/BoardAdapter.cs:1086       return $"health={combatant.CurrentHealth}, state={combatant.State}";
Assets/Game/Unity/BoardAdapter.cs:550        Team team = battle.TryGetCombatant(placer, out Combatant combatant)
```

Birincisi (`DescribeCondition`) bir savaşçının canını ve durumunu **sorup** bir
log satırı kuruyor. Bu, ilkenin klasik ihlal şekli. Ama ***raporlama ilkenin
kabul edilmiş istisnasıdır***: bir nesneye "kendini logla" demek, ona log
biçimini, hedefini ve dilini de öğretmek demektir — ve o an `GridStrategy.Combat`
`UnityEngine.Debug`'ı tanımak zorunda kalır, yani assembly duvarı düşer
(4. ilke). ***Bu bir ihlal değil, ilkenin sınırıdır.***

İkincisi (`NewStructure`) yapının tarafını, onu koyan birimden **soruyor**.
Burada da alternatif daha kötü: taraf Inspector'dan alınsaydı aynı bilginin
ikinci kaynağı doğardı ve düşmanın yaptığı bina oyuncunun tarafında
görünebilirdi — gerekçe `BoardAdapter.cs:544-547`'de yazılı. Yani "sor" burada
**tek doğruluk kaynağını** koruyor (3. ilke).

***SONUÇ: bu projede Tell-Don't-Ask'ın gerçek bir ihlali BULUNAMADI.*** İki
aday da sınırın doğru tarafında.

**ÖLÇÜSÜ** — Deney: `AttackOutcome`'u tek bir `bool`'a indir.
`BoardAdapter.ReactToAttack`'te bugün beş ayrı `case` var; `bool`'la yazıldığında
o beş dal tek bir `if`'e çöker ve sebebi öğrenmek isteyen çağıran menzili
**yeniden ölçmek**, hedefin durumunu **yeniden sormak** zorunda kalır. 
***Gözlenen şey: `MoveAction`'ın kuralları `BoardAdapter`'ın içinde ikinci kez
belirir.*** Ölçüsü de yazılı — bugün `BoardAdapter` içinde `GridDistance`
kelimesi hiç geçmiyor; o gün geçmek zorunda kalır.

**NE ZAMAN UYGULANMAZ** — ***Üç durumda***:

```
① SORGU (query) BİR İHLAL DEĞİLDİR
   "Kaç canı var" sorusunun cevabını almak Tell-Don't-Ask'ı bozmaz. İhlal,
   cevabı alıp KARARI dışarıda vermektir. Combatant.cs:152 ve :154 birer
   sorgudur ve kimse onlara bakıp kural yazmıyor.

② RAPORLAMA, SERİLEŞTİRME VE GÖRSELLEŞTİRME
   Bu üç iş nesnenin dışında yaşamak ZORUNDA — aksi hâlde nesne log biçimini,
   dosya formatını ve ekran teknolojisini öğrenir. BoardAdapter.cs:1079 tam
   olarak budur ve doğru yerdedir.

③ SINIRIN ÖTESİNDE DURAN NESNE
   Bir tipe "kendini yap" diyebilmen için onu tanıman gerekir. GridStrategy.Combat
   UnityEngine'i TANIMIYOR (noEngineReferences: true), dolayısıyla "kendini
   çiz" diyemez. Burada sormak bir tercih değil, duvarın sonucudur.
```

**MÜLAKAT CEVABI**

> **KISA (30 sn).** Tell-Don't-Ask, bir nesnenin durumunu sorup kararı dışarıda
> vermek yerine ona ne istediğini söylemektir. Projemde çağıran
> `BattleActions.Attack`'e "saldır" diyor; menzili, hedefin uygunluğunu ya da
> sırayı sormuyor. Cevap dört sonuç enum'undan biriyle, adlandırılmış bir değer
> olarak dönüyor. İlkeyi bilerek uygulamadığım tek yer log satırı: orada
> nesneye "kendini logla" demek ona `UnityEngine`'i öğretmek olurdu ve assembly
> duvarımı yıkardı.

> **GENİŞLETİLMİŞ (2 dk).** Bağlam: Unity katmanı savaş kurallarını
> **görmüyor**, çünkü kurallar motorsuz assembly'lerde yaşıyor ve testleri sahne
> kurmadan koşuyor. Gözlenen sorun: `bool` döndüren bir `Move`, çağıranı
> kuralları yeniden yazmaya zorluyordu — "neden olmadı" sorusunun cevabı
> çağıranın içinde ikinci kez doğuyordu. Seçtiğim mekanizma sonuç enum'u:
> `MoveOutcome`'da beş değer var ve üçü ayrı ret sebebi. Sahibi akış katmanı,
> yani `Assets/Game/Core` ve `Assets/Game/Battle`. En yakın alternatif tek bir
> `Rejected` değeriydi; ayıran ölçüt sebep sayısı değil **davranış** sayısı
> çıktı: "hücre dolu" bir tur sonra değişebilir, "geçersiz hedef" asla
> değişmez — tek değer bu çizgiyi siler ve yol bulucu sonsuza dek yeniden
> dener. Kanıt olarak `BoardAdapter`'daki `switch`'in `default` dalı `LogError`
> atıyor: enum'a yeni bir değer eklenip burası güncellenmezse çalışma zamanında
> görünür oluyor, çünkü C#'ta `switch` **deyimi** eksik dal için uyarmaz. Ödün:
> her yeni ret sebebi bir enum değeri ve bir `case` demek; sebep sayısı
> arttıkça bu maliyet büyür ve o gün ret sebeplerini bir aileye toplamayı
> yeniden değerlendiririm.

---

## 3. Tek doğruluk kaynağı (single source of truth)

**ADI VE KÖKENİ** — İngilizce *single source of truth* (SSOT); Türkçe "tek
doğruluk kaynağı". ***`KAYNAK DOĞRULANMADI`*** — terim bilgi sistemleri ve
veritabanı tasarımı literatüründen geliyor ve tek bir yazara bağlanamıyor. Bir
kişi ya da tarih söyleme.

**NE DER** — Bir olgunun **tek bir** yazma yeri olsun; ikinci bir kopya
tutulacaksa o kopya bir **türev** olmalı, ikinci bir yetkili değil.

**BU PROJEDE NEREDE** — ***Uygulanmış, ve projenin en sıkı korunan
değişmezlerinden biri.*** Tahtanın tek sahibi:

```
Assets/Game/Battle/Battle.cs:53              private readonly UnitGrid board;
Assets/Game/Battle/Battle.cs:107             internal UnitGrid Board => board;
Assets/Game/Battle/Battle.cs:528             public bool TryGetPosition(Unit unit, out int x, out int y)
```

`TryGetPosition` her çağrıda tahtayı **yeniden tarıyor** — `Width × Height`
hücre, bugün 15. Bir `Dictionary<Unit, (int, int)>` önbelleği bunu tek okumaya
indirirdi ve reddedildi. Gerekçe koddan:

```
Assets/Game/Battle/Battle.cs:526             // kararı değil, ikinci bir doğruluk kaynağı yaratma kararıdır.
```

***İşte ilkenin adı konmamış hâli tam olarak bu satır.*** Cümle ilkeyi
kusursuz tarif ediyor ve adını hiç söylemiyor.

"İkinci yazar"ın nasıl doğmadığı, `readonly`'nin burada neyi **korumadığı**
ve garantinin tam olarak nerede bittiği ayrı bir belgede anlatılıyor ve burada
**tekrar edilmiyor**:
[`../deep/konular/03-tahta-sahipligi.md`](../deep/konular/03-tahta-sahipligi.md).

Aynı ilkenin ikinci uygulaması yerleştirmede:

```
Assets/Game/Battle/BattleActions.cs:355      if (battle.TryGetUnit(x, y, out Unit _))
```

Doluluk sorusu **tahtaya** soruluyor, ikinci bir deftere değil — barakalar da
tahtada yer kapladığı için tek soru hem birimleri hem yapıları kapsıyor.
İkinci bir tahta açılsaydı burada iki soru olurdu ve biri unutulduğu gün aynı
hücrede iki şey dururdu, hiçbir derleme hatası çıkmadan.

**ÖLÇÜSÜ** — Deney: `Battle`'a bir `positions` sözlüğü ekle ve `TryGetPosition`
onu okusun. Sonra bir birimi hareket ettir. `MoveAction.Execute` tahtayı
**doğrudan** değiştiriyor; sözlük bunu duymaz. ***Gözlenen şey: birim ekranda
düşmanın yanında duruyor, saldırı "menzil dışı" diyor.*** Hiçbir test kırmızıya
dönmez, çünkü testler ikisini aynı anda kurmaz. Bu, ikinci doğruluk kaynağının
imzasıdır — çelişki **görünür** ama **sessizdir**.

**NE ZAMAN UYGULANMAZ** — ***İki durumda***:

```
① ÖNBELLEK BİR İHLAL DEĞİLDİR — EĞER TÜREV OLDUĞU YAZILIYSA
   İkinci kopya, birincisi değiştiğinde geçersiz kılınıyorsa doğruluk kaynağı
   değil bir TÜREVDİR. Kırılma "kopya var" olgusundan değil, "kopyanın da
   yazma hakkı var" olgusundan doğar. Bu projede türev bir örnek zaten duruyor:
   TurnState.cs:64 salt okunur görünüm — kaynak dizi, görünüm türev, yazma tek yerde.

② ÖLÇÜLMÜŞ BİR DARBOĞAZ VARSA
   TryGetPosition bugün 15 hücre tarıyor. Tahta büyüdüğünde ve tarama PROFİLDE
   göründüğünde önbellek doğru seçim olur — ama o gün "tek yazma kapısı" bir
   tercih olmaktan çıkıp ZORUNLU hâle gelir.
   >> HENÜZ YOK → 02-sonraki-asamalar.md · Aşama 6 (profil çıkarma kanıt sınırı) <<
```

**MÜLAKAT CEVABI**

> **KISA (30 sn).** Tek doğruluk kaynağı, bir olgunun tek bir yazma yeri
> olmasıdır. Projemde bir birimin konumunu soran metot her çağrıda tahtayı
> yeniden tarıyor — bir konum sözlüğü tutmayı bilerek reddettim. Sebebi
> performans değil doğruluk: hareket tahtayı doğrudan değiştiriyor, sözlük bunu
> duymazdı ve birim yaklaşmış olduğu hâlde saldırı "menzil dışı" derdi. Önbellek
> bir hız kararı değil, ikinci bir yazar yaratma kararıdır.

> **GENİŞLETİLMİŞ (2 dk).** Bağlam: tahta `UnitGrid`, sahibi `Battle`, ve
> Unity katmanı tahtaya hiç dokunmuyor. Gözlenen sorun: konumu sorgulamak
> `Width × Height` tarama demek ve refleks olarak bir sözlük eklemek istiyorsun.
> Seçtiğim mekanizma taramayı korumak, sahibi `Battle`. En yakın alternatif
> `Dictionary<Unit, (int, int)>`; kaybettiği şey senkronizasyon garantisi —
> `MoveAction` tahtayı yazıyor ve sözlüğü tanımıyor, tanısaydı da o an tahta
> ile sözlük arasında bir sıra bağımlılığı doğardı. Sahipliği üç katman ayakta
> tutuyor ve ilginç olan sıraları: en güçlüsü "kurucuda `new`, ikinci ok hiç
> doğmuyor" olgusu ve arkasında **derleyici yok**; ikincisi `internal Board`,
> arkasında derleyici **var** — `GridStrategy.Unity` o üyeyi göremiyor; en
> zayıfı `readonly`, çünkü alanı koruyor ama nesnenin içine yazmayı hiç
> görmüyor. Kanıt: tahta bugün 15 hücre, yani tarama ölçülebilir bir maliyet
> bile değil. Ödün: tahta büyüdüğünde bu karar tersine döner, ama o gün ilk
> yazacağım şey önbellek değil, tek yazma kapısı olur.

---

## 4. Dependency Injection (bağımlılığın dışarıdan verilmesi)

**ADI VE KÖKENİ** — İngilizce *dependency injection*, kısaca DI; Türkçe
"bağımlılık enjeksiyonu" ya da "bağımlılığın dışarıdan verilmesi". `İYİ
BİLİNEN`: terimi 2004'te Martin Fowler'ın *Inversion of Control Containers and
the Dependency Injection pattern* yazısı adlandırdı; daha genel olan
*inversion of control* fikri daha eskidir. ***Birincil kaynağa bu oturumda
bakılmadı.***

**NE DER** — Bir tip ihtiyaç duyduğu parçayı **kendi içinde kurmasın**;
dışarıdan alsın.

***EN SIK YANLIŞ MODEL: DI BİR ÇERÇEVE (framework) DEĞİLDİR.***

Bu, mülakatta en çok puan kaybettiren yanlış anlamalardan biri. "DI kullandın
mı?" sorusuna "hayır, Zenject/VContainer kullanmadım" diye cevap vermek, soruyu
**yanlış anlamaktır**. Ayrım şu:

```
  DEPENDENCY INJECTION          bir TASARIM KARARI
                                "parçayı içeride kurma, dışarıdan al"
                                aracı: kurucu parametresi, metot parametresi,
                                       property, [SerializeField]

  DI KONTEYNERİ (container)     bir ARAÇ
                                "hangi parçanın nereye gideceğini bir kayıt
                                 defterinden ben çözerim"
                                >> AYRI BİR KARAR — ve genellikle GEREKMEZ <<
```

***Kurucudan parametre geçirmek **zaten** DI'dır.*** Konteyner, DI'ı yapan şey
değil, DI'ı **otomatikleştiren** şeydir; bir projede DI olmadan konteyner
olamaz, ama konteyner olmadan DI **olur** ve bu projede tam olarak öyle.

**BU PROJEDE NEREDE** — ***Uygulanmış, konteynersiz, üç ayrı yoldan.***

```
Assets/Game/Core/Combat/Combatant.cs:59      public Combatant(
Assets/Game/Core/Combat/Structure.cs:51      public Structure(
Assets/Game/Core/PointerGesture.cs:127       public PointerGesture(float dragThreshold)
Assets/Game/Core/Combat/Health.cs:31         public Health(int max)
```

`Combatant` dört parçayı da kurucudan alıyor: `Health`, `UnitLifecycle`,
`AttackProfile`, `Team`. Gerekçe kodun kendi cümlesinde
(`Combatant.cs:65-68`): parça içeride kurulsaydı 200 okçu tek tanımı
paylaşamaz, 200 ayrı nesne doğardı. Yani DI burada bir mimari süs değil, 6.
desenin (paylaşılan değişmez tanım) **ön koşulu**.

İkinci yol Unity tarafında ve o da DI'dır:

```
Assets/Game/Unity/BoardAdapter.cs:124        [SerializeField] private UnitView unitPrefab;
```

Üçüncü yol **DI değildir** ve ayrımı bilmek önemli:

```
Assets/Game/Battle/Battle.cs:53              private readonly UnitGrid board;
```

`Battle` tahtayı kurucusunda **kendisi kuruyor** ve bu bilinçli bir DI
reddidir — çünkü tahtayı dışarıdan almak ikinci bir ok doğurur ve tek
sahipliği (3. ilke) bitirir. ***Yani bu projede DI ve DI'ın reddi **aynı
ölçütle** verilmiş iki karardır: parça bir değişmez taşıyorsa içeride kurulur,
taşımıyorsa dışarıdan alınır. Karar ağacının tamamı
[`../deep/konular/03-tahta-sahipligi.md`](../deep/konular/03-tahta-sahipligi.md)'de
*"Kural: bir nesneyi dışarıdan almalı mısın"* başlığı altında.***

İlişki: DI, [`01-koda-gomulu-desenler.md`](01-koda-gomulu-desenler.md) §5'teki
**bileşimin** taşıyıcısıdır. Bileşim "parçalardan kurul" der; DI "parçaları
kim verecek" sorusuna cevap verir. Biri yapı, öteki kanal.

**ÖLÇÜSÜ** — Deney: `Combatant`'ın kurucusunu değiştir, `AttackProfile`'ı
içeride `new AttackProfile(10, 1)` diye kur. İki şey aynı anda gözlenir:
① `Assets/Tests/EditMode/` altındaki savaşçı testleri artık farklı hasarlı bir
birim **kuramaz** — her test aynı savaşçıyı almak zorunda kalır; ② 200 birim
200 ayrı profil nesnesi doğurur ve "yüzlerce asker tek tanımı paylaşır" cümlesi
sessizce yalan olur. ***Testin kurulum yarısının çökmesi, DI'ın kaybının en
hızlı ölçüsüdür.***

**NE ZAMAN UYGULANMAZ** — ***Üç durumda***:

```
① PARÇA BİR DEĞİŞMEZ TAŞIYORSA
   Battle.cs:53 tam olarak bu. Tahtayı dışarıdan almak "ikinci ok yok"
   garantisini bitirir; kazanan taraf DI değil sahipliktir.

② KONTEYNER, DI'IN KENDİSİ SANILDIĞINDA
   >> Bir konteyner bağımlılıkları GÖRÜNÜR kılmaz, ÇÖZER — ve çözerken
   çağrı yerinde GİZLER. << Bu projede bağımlılıkların üçü de imzada okunuyor;
   bir konteyner o okunabilirliği bir kayıt defterine taşırdı.
   architecture-patterns.archive bunu ayrı bir kuralla yasaklıyor:
   "Never create a Manager or service locator solely to reduce typing."

③ MonoBehaviour KURUCU ALAMAZ
   Unity bileşenlerini `new` ile kuramazsın; oradaki DI kanalı kurucu değil
   [SerializeField]'dir. Aynı ilke, farklı aracı — ve bu bir taviz değil,
   motorun sözleşmesi.
```

**MÜLAKAT CEVABI**

> **KISA (30 sn).** DI bir çerçeve değil bir tasarım kararıdır: parçayı içeride
> kurma, dışarıdan al. Projemde konteyner yok ama DI her yerde —
> `Combatant` dört parçasını da kurucudan alıyor, Unity tarafında kanal
> `[SerializeField]`. Bunu yapmamın sebebi test edilebilirlik değil paylaşım:
> profil içeride kurulsaydı 200 okçu tek tanımı paylaşamaz, 200 ayrı nesne
> doğardı. Bir yerde DI'ı bilerek **reddettim** de — tahtayı `Battle` kendisi
> kuruyor, çünkü dışarıdan almak ikinci bir yazar doğururdu.

> **GENİŞLETİLMİŞ (2 dk).** Bağlam: çekirdek assembly'ler motoru hiç
> tanımıyor, dolayısıyla hiçbir bağımlılık sahne aramasıyla gelemiyor —
> `FindObjectOfType` ve `Instance` üretim kodunda hiç geçmiyor. Gözlenen sorun:
> bağımlılıkların nereden geldiği görünmezse test kurulumu imkânsızlaşıyor ve
> aynı sınıf iki farklı yapılandırmayla kurulamıyor. Seçtiğim mekanizma kurucu
> enjeksiyonu, sahibi tipin kendi imzası. En yakın alternatif bir servis
> lokatörü ya da statik bir `GameManager`'dı; kaybettiği şey görünürlük — bağımlılık
> çağrı yerinden kaybolur ve test yalıtımı biter. Bunun ölçüsü elimde: projede
> **değiştirilebilir hiçbir `static` alan yok**, tek `static` alan salt okunur
> bir tur sırası. Kanıt: 26 EditMode test dosyası kendi nesnesini kurup atıyor,
> hiçbiri bir başkasının durumunu göremiyor. Ödün: kurucu imzaları uzuyor —
> `Combatant`'ınki dört parametre. Beşinci ve altıncı parça geldiği gün bir
> parametre nesnesi ya da kurucu ayrımı gerekir; konteyner ise ancak bağımlılık
> grafiği elle izlenemez hâle geldiğinde gündeme gelir, ki bugün 33 dosyada
> öyle bir şey yok.

---

## 5. Law of Demeter (en az bilgi ilkesi)

**ADI VE KÖKENİ** — İngilizce *Law of Demeter*, kısaca LoD; *principle of least
knowledge* olarak da anılır. Türkçe "en az bilgi ilkesi". `İYİ BİLİNEN`: 1987'de
Northeastern Üniversitesi'ndeki **Demeter** araştırma projesinde formüle edildi;
adı projeden geliyor, bir kişiden değil — Demeter Yunan mitolojisinde tarım
tanrıçası ve proje adı oradan. Ian Holland ve Karl Lieberherr'in adları bu
formülasyonla birlikte anılır. ***Birincil kaynağa bu oturumda bakılmadı;
tarihi telaffuz edeceksen "seksenlerin sonu" de.***

**NE DER** — Yalnız **komşuna** konuş: bir metot yalnızca kendi alanlarına,
parametrelerine ve kendi kurduğu nesnelere mesaj göndersin — komşusunun
komşusuna değil.

Pratik kısayolu: **nokta zinciri** (`a.B.C.D`). Kısayol ilkenin kendisi değil
ama ihlalin en görünür imzası.

**BU PROJEDE NEREDE** — ***Aday zincirler grep'le sayıldı. Üretim kodunda
üç seviyeli zincir **sekiz kez** geçiyor ve hepsi iki şekilde toplanıyor:***

```
Assets/Game/Battle/BattleActions.cs:107      if (!TurnRules.CanAct(attackerCombatant.Team, battle.Turn.Current))
Assets/Game/Battle/BattleActions.cs:143      battle.Turn.EndTurn();
Assets/Game/Battle/BattleActions.cs:187      if (!TurnRules.CanAct(combatant.Team, battle.Turn.Current))
Assets/Game/Battle/BattleActions.cs:216      battle.Turn.EndTurn();
Assets/Game/Battle/BattleActions.cs:254      if (!TurnRules.CanAct(reviverCombatant.Team, battle.Turn.Current))
Assets/Game/Battle/BattleActions.cs:304      battle.Turn.EndTurn();
Assets/Game/Core/Combat/AttackAction.cs:98   target.TakeDamage(attacker.AttackProfile.Damage);
Assets/Game/Core/Combat/AttackAction.cs:169  return target.TakeDamage(attacker.AttackProfile.Damage)
```

Bir dokuzuncu şekil daha var ve ***ihlal değil***:

```
Assets/Game/Battle/Battle.cs:385             pair.Value.Tick(deltaSeconds);
Assets/Game/Battle/Battle.cs:440             if (pair.Value.IsReadyForCleanup)
```

`pair` bir `KeyValuePair` — dilin sözlük numaralandırmasına verdiği demet.
`pair.Value` bir "komşunun komşusu" değil, döngü değişkeninin kendisi. Bunu
ihlal saymak ilkeyi kısayola indirgemek olurdu.

***İKİ ADAY, DÜRÜST DEĞERLENDİRME*** — ve ikisi **aynı cevabı almıyor**:

```
  attacker.AttackProfile.Damage              >> İHLAL DEĞİL <<
  ────────────────────────────
  AttackProfile bir DEĞİŞMEZ TANIM nesnesi: alanı yok, set'i yok, doğrulama
  kurucuda (AttackProfile.cs:49). Okuma bir DEĞER alıyor, bir davranış
  tetiklemiyor. Ölçüsü şu: bu satır AttackProfile'ın İÇİNİ değiştirebilir mi?
  Hayır — o tip değişmez. Zincir burada bir veri yolu, bir yetki yolu değil.

  battle.Turn.EndTurn()                      >> GERÇEK ADAY <<
  ─────────────────────
  Burada zincirin ucunda bir OKUMA değil bir MUTASYON var: EndTurn sırayı
  devrediyor, yani Battle'ın komşusunun DURUMUNU değiştiriyor. Klasik
  "tren kazası" (train wreck) şekli tam olarak budur.
  >> Ve Battle'ın kendisi bu devri hiç GÖRMÜYOR. <<
```

***DÜRÜST HÜKÜM*** — `battle.Turn.EndTurn()` bir Law of Demeter ihlalidir; ama
**savunulabilir** bir ihlaldir ve iki sebebi kodda yazılı. ①`TurnState` bir
iç parça değil, `Battle`'ın **açıkça yayımladığı** bir alt sözleşme
(`Battle.cs:154`, `public TurnState Turn { get; }` — get-only, yani
`Battle`'ın turu **kimliği** sabit). ② `Battle`'a bir `EndTurn()` iletici
metodu eklemek, sıra kuralını `Battle`'a **ikinci kez** öğretirdi ve bu
projenin en sıkı savunduğu şeye çarpardı: aynı kararın iki yerde yaşamaması
(8. ilke).

Yani burada iki ilke çatışıyor ve ***DRY kazandı, Demeter kaybetti***. Bu
çatışmanın uzun hâli aşağıda, 10. bölümde.

**ÖLÇÜSÜ** — Deney: `TurnState.EndTurn`'ün imzasını değiştir (örneğin bir
`Team` parametresi al). Derleyici sana **üç** yer gösterir: `BattleActions.cs`
`:143`, `:216`, `:304`. ***`Battle` sınıfının kendisi hiç görünmez — sırayı
sahiplenen tip, sıranın devredildiğini bilmez.*** Bu, ihlalin ölçülebilir
bedelidir: değişiklik sahibin üstünden **atlayarak** yayılıyor.

Karşı deney: `AttackProfile.Damage`'ın tipini değiştir. Yine iki yer görünür
(`AttackAction.cs:98`, `:169`) ama bu sefer `Combatant` da görünür, çünkü
`AttackProfile`'ı o taşıyor. Zincir bilgi taşırken sahip kaybolmuyor.

**NE ZAMAN UYGULANMAZ** — ***Üç durumda***:

```
① AKICI ARAYÜZ (fluent) VE OLUŞTURUCU ZİNCİRLERİ
   builder.WithX().WithY().Build() bir ihlal değildir: her adım AYNI nesneyi
   döndürür, yani komşunun komşusu diye bir şey yoktur. Bu projede karşılığı
   YOK — üretim kodunda akıcı bir API hiç geçmiyor.

② DEĞİŞMEZ DEĞER NESNELERİ
   Yukarıdaki AttackProfile örneği. İlkeyi buraya uygulamak, her tanım alanı
   için sahibine bir iletici metot yazdırırdı ve Combatant sessizce
   AttackProfile'ın API'sinin kopyası olurdu.

③ İLETİCİ METOT YAZMANIN BEDELİ ZİNCİRDEN AĞIRSA
   >> İlkeyi sınırsız uygulamanın adı vardır: "orta adam" (middle man) kokusu. <<
   Her zinciri kırmak için bir iletici yazarsan sahip tip, komşusunun API'sinin
   birebir kopyasına dönüşür ve o kopya ayrı hızda eskir. Bu projede
   Battle.EndTurn() yazmamanın sebebi tam olarak budur.
```

**MÜLAKAT CEVABI**

> **KISA (30 sn).** Law of Demeter "yalnız komşuna konuş" der: bir metot
> komşusunun komşusuna mesaj göndermesin. Projemde ihlali **aradım ve buldum**:
> akış sahibim `battle.Turn.EndTurn()` yazıyor, yani `Battle`'ın komşusunun
> durumunu değiştiriyor ve `Battle` bunu görmüyor. Bilerek bıraktım — `Battle`'a
> bir iletici metot eklemek sıra kuralını ikinci bir yere yazardı. Aynı dosyada
> ihlal **olmayan** bir zincir de var: `attacker.AttackProfile.Damage` bir
> değişmez tanımdan değer okuyor, davranış tetiklemiyor.

> **GENİŞLETİLMİŞ (2 dk).** Bağlam: `Battle` savaşın kimlik ve konum sahibi;
> sıra durumu `TurnState` adında ayrı bir tipte ve `Battle` onu get-only bir
> property olarak yayımlıyor. Gözlenen sorun: dört eylemin üçü sırayı devretmek
> zorunda ve devri yapan tip akış sahibi. Seçtiğim mekanizma zinciri kabul
> etmek. En yakın alternatif `Battle`'a bir `EndTurn()` ileticisi eklemekti;
> kaybettiği şey şu: sıranın **ne zaman** devredileceği kararı akış sahibinde
> yaşıyor ve bir beyaz listeye bağlı — hangi sonuç değerleri devri tetikler.
> O listeyi `Battle`'a taşımak, `Battle`'a `AttackOutcome` ve `MoveOutcome`
> tiplerini öğretmek olurdu. Kanıt: ihlalin bedelini ölçebiliyorum —
> `TurnState.EndTurn`'ün imzasını değiştirdiğimde derleyici üç çağrı yeri
> gösteriyor ve `Battle` bunların hiçbirinde görünmüyor, yani değişiklik sahibin
> üstünden atlayarak yayılıyor. Ödün: dördüncü bir eylem eklendiğinde bu zincir
> dördüncü kez yazılacak. Kararı tersine çevirecek kanıt şu olurdu: devir
> kuralının **kendisi** dallanmaya başlarsa — örneğin bazı eylemler yarım tur
> harcarsa — o gün karar `Battle`'a ait olur ve iletici metodu yazarım.

---

## 6. Kalıtım yerine bileşim (composition over inheritance)

**ADI VE KÖKENİ** — İngilizce *favor composition over inheritance*; Türkçe
"kalıtım yerine bileşimi tercih et". `İYİ BİLİNEN`: 1994 tarihli *Design
Patterns* kitabının (Gamma, Helm, Johnson, Vlissides — "Gang of Four") giriş
bölümündeki iki temel ilkeden biri olarak anılır. ***Birincil kaynağa bu
oturumda bakılmadı.***

**NE DER** — Bir tipin yeteneklerini üst sınıftan **devralmak** yerine,
parçaları alan olarak **tutarak** kazan.

**BU PROJEDE NEREDE** — ***Bu bölüm bilerek kısa: mekanizma
[`01-koda-gomulu-desenler.md`](01-koda-gomulu-desenler.md) §5'te tam olarak
anlatılmış durumda ve burada TEKRAR EDİLMİYOR.*** Bu dosyanın katkısı yalnız
ad ve köken.

***ÖLÇÜ DOĞRULANDI*** — iddia "projede toplam iki kalıtım satırı var, ikisi de
`: MonoBehaviour`" idi. Bu oturumda `Assets/Game` altındaki 33 üretim dosyası
tarandı ve **doğrulandı**; kalıtım satırı tam olarak iki tane:

```
Assets/Game/Unity/BoardAdapter.cs:110        public sealed class BoardAdapter : MonoBehaviour
Assets/Game/Unity/UnitView.cs:43             public sealed class UnitView : MonoBehaviour
```

İkisi de `GridStrategy.Unity` içinde, ikisi de `sealed`, ikisi de **zorunlu** —
Unity bir bileşeni ancak `MonoBehaviour`'dan türeyerek tanır. Yani bu projede
kalıtım bir seçenek olarak değil, motorun dayattığı tek yerde kullanılıyor.
`abstract`, `virtual`, `override` ve `interface` kelimelerinin dördü de üretim
kodunda **hiç geçmiyor**.

Kalıtımın bilinçli reddi kodda yazılı:

```
Assets/Game/Core/Combat/Structure.cs:19      // yarısına uymaz — TryRevive, Downed hâli, zorunlu AttackProfile, on
```

Bileşimin görünen yüzü de tek satırda:

```
Assets/Game/Core/Combat/Combatant.cs:152     public UnitState State => lifecycle.State;
```

Dışarıya tek bir tip görünüyor, cevabı bir parça veriyor.

**ÖLÇÜSÜ** — Deney: `Structure`'ı `: Combatant` yap. İki şey aynı anda kırılır:
① `AttackProfile` `Combatant.cs:72`'de `null` reddediliyor, yani saldırmayan bir
baraka **kurulamaz**; ② kelepçe gevşetilse bile `Downed` hâli barakada
**yazılabilir** hâle gelir ve `AttackOutcome.HitAndDowned` ile `HitAndDestroyed`
ayrımı anlamını yitirir. ***Ölçünün adı Liskov: bir alt tip üst tipin yerine
geçebiliyorsa devraldığı **her** üye onda anlamlı olmalıdır.***

**NE ZAMAN UYGULANMAZ** — ***İki durumda***:

```
① MOTOR / ÇERÇEVE KALITIMI DAYATIYORSA
   MonoBehaviour, ScriptableObject, EditorWindow. Bu projedeki iki kalıtım
   satırının ikisi de burada; bu bir tercih değil sözleşmedir.

② GERÇEK BİR ALT TÜR AİLESİ VARSA
   unity-csharp-quality-flow.archive'daki Ownership Map tablosu ölçüyü tek
   satırda veriyor: soyut bir taban "genuine subtype family shares invariant
   state/behavior" baskısında doğru; reddedilme koşulu ise
   "inheritance exists only to reuse a few lines".
   >> Yani kalıtımı SATIR TASARRUFU için kullanmak, ilkenin değil DRY'ın
   yanlış uygulanmasıdır — ve ikisini karıştırmak sık görülür. <<
```

**MÜLAKAT CEVABI**

> **KISA (30 sn).** Bileşim, yetenekleri devralmak yerine parça olarak tutmaktır.
> Projemde kalıtım toplam **iki satır** ve ikisi de `: MonoBehaviour`, yani
> motorun dayattığı yer — üretim kodunda `abstract`, `virtual`, `override` ve
> `interface` hiç geçmiyor. Bunun en somut örneği `Structure`: `: Combatant`
> yazmayı reddettim, çünkü baraka devralacağı üyelerin yarısına uymuyor —
> `TryRevive`, `Downed` hâli, zorunlu saldırı profili.

> **GENİŞLETİLMİŞ (2 dk).** Bağlam: tahtada iki tür şey duruyor, asker ve bina,
> ve ikisinin de canı var. Refleks çözüm ortak bir taban sınıf. Gözlenen sorun:
> ortak olan şey **parçalar** — `Health` ikisinde de aynı — ama **yaşam
> döngüsü** değil. Asker düşer, on saniye diriltilebilir, sonra ölür; bina
> yıkılır ve geri gelmez. Seçtiğim mekanizma bileşim: `Combatant` ve `Structure`
> ikisi de `Health` tutuyor ama farklı yaşam döngüsü tipleri tutuyor.
> En yakın alternatif `Structure : Combatant`; kaybettiği şey Liskov ölçütü —
> `TryRevive`'ı `virtual` yapıp `Structure`'da `return false;` diye ezmek
> zorunda kalırdım, aynı şekilde diriltme bölenini ve kalan süreyi de. Ve
> `sealed` bu satıra karşı sıfır koruma sağlar; `sealed` tip ağacını keser,
> nesne grafiğini kesmez. Kanıt: `Combatant`'ın kurucusu `null` bir saldırı
> profilini reddediyor, yani kalıtım seçilseydi saldırmayan bir barakayı
> **kuramazdım** bile. Ödün: iki tip iki yerde bakım demek; birim ile yapının
> yaşam döngüsü gerçekten aynı kurala yaklaşırsa — binalar da kurtarma penceresi
> kazanırsa — o gün iki tip bir kopya olur ve kararı tersine çeviririm.

---

## 7. YAGNI (şimdilik gerekmiyor)

**ADI VE KÖKENİ** — İngilizce *You Aren't Gonna Need It*, kısaltması YAGNI;
Türkçe "buna ihtiyacın olmayacak". `İYİ BİLİNEN`: Extreme Programming (XP)
pratiklerinden; 1990'ların sonunda Kent Beck ve Ron Jeffries çevresinde
adlandırıldı, sloganın kendisi genellikle Jeffries'e atfedilir. ***Birincil
kaynağa bu oturumda bakılmadı; atıfta ısrar etme.***

**NE DER** — Bugün gerekmeyen bir yeteneği "ileride lazım olur" diye bugün
yazma.

**BU PROJEDE NEREDE** — ***Uygulanmış, ve bir bölümde değil **bir dosyanın
tamamında**.*** [`02-sonraki-asamalar.md`](02-sonraki-asamalar.md) baştan sona
bir YAGNI belgesidir: altı mekanizma (ScriptableObject, nesne havuzu, olay veri
yolu, singleton, ECS/DOTS, profil çıkarma) yazılmış, hiçbiri **önerilmemiş**,
ve her birinin bir **tetikleyici koşulu** var.

Bu dosyanın katkısı iki şey. Birincisi ilkenin adını koymak. İkincisi, o
dosyanın YAGNI'ye ***eksik olan yarısını*** eklediğini görünür kılmak:

```
  YAGNI'nin YAYGIN HÂLİ          "bugün gerekmiyor"
                                 ── ve cümle burada BİTİYOR

  BU PROJEDEKİ HÂLİ              "bugün gerekmiyor, ÇÜNKÜ ...,
                                  ve şu OLDUĞUNDA gerekecek"
                                 ── ölçüyle, dosyayla, eşikle
```

***Fark ölçülebilir.*** `02-sonraki-asamalar.md`'nin kendi kuralı şu:
*"bugün önemli değil" eksik bir cümledir* — ve ölçüsüz bir "gerekirse eklenir"
o dosyada bir **ihlal** sayılıyor. Yani proje YAGNI'yi uygulamakla kalmıyor,
ilkenin en sık düştüğü tuzağı da kapatıyor: bir yıl sonra "bugün önemli değil"
cümlesinin "hiç öğrenmedim"e dönüşmesini.

Somut bir örnek — nesne havuzu:

```
Assets/Game/Unity/BoardAdapter.cs:739        UnitView view = Instantiate(unitPrefab, transform);
Assets/Game/Unity/BoardAdapter.cs:1007       Destroy(view.gameObject);
```

`Instantiate`'in tek çağıranı `SpawnUnit`, onun da tek çağıranları `Awake`
içindeki iki satır. Yani **kare başına sıfır** birim doğuyor ve havuzun
azaltacağı maliyet ölçülebilir değil, çünkü maliyet yok. Tetikleyici koşul
yazılı: sürekli doğup ölen bir nesne sınıfı ortaya çıktığında, ya da
`Instantiate`/`Destroy` çifti `Update` yolundan çağrılmaya başladığında.

**ÖLÇÜSÜ** — Deney: bugün bir nesne havuzu ekle. Ölçülebilir hiçbir şey
iyileşmez — çünkü kare başına doğum sayısı sıfır — ama ***üç şey kırılır***:
① havuzdan dönen nesnede `Awake` **çalışmaz**, yani `UnitView.cs:86`'daki
ayakta-ve-seçimsiz başlatma sessizce kaybolur ve önceki birimin gri tonu yeni
birimde görünür; ② havuzun ilk doldurması büyük bir tahsis yapar ve "kare
başına sıfır" ölçümünü yapan kişi yanlış yerde arar; ③ `unitViews` sözlüğünün
bugünkü anlamı ("tabloda varsa ekranda var") üçüncü bir hâl kazanır. ***YAGNI
ihlalinin bedeli eklenen kod değil, kaybolan **değişmezdir**.***

**NE ZAMAN UYGULANMAZ** — ***Bu alan burada en kritiği***:

```
① >> YAGNI, GEREKLİ BİR SÖZLEŞMEYİ ATLAMANIN BAHANESİ DEĞİLDİR <<
   "İleri seviye görünüyor" diye atlanan şeyler YAGNI kapsamında DEĞİLDİR:
   null doğrulaması, sınır kontrolü, geri dönülemez adımın sırası, olay
   aboneliğinin sökülmesi. Bunlar gelecekteki bir ihtiyaç değil, BUGÜNKÜ
   doğruluk. Bu projede ölçüsü var: 66 throw ve dört enum'un sıfırıncı
   değer kararı hiçbiri "ileride lazım olur" diye yazılmadı.

② GERİ DÖNÜLEMEZ KARARLAR
   Bir enum'un sıfırıncı değeri, bir asmdef'in referans yönü, bir veri
   formatı. Bunları sonra değiştirmek ucuz değildir; YAGNI ucuz-geri-alınabilir
   kararlar için bir ölçüttür. AttackOutcome.cs'teki "yeni değer SONA eklenir"
   kararı tam olarak bu sebeple bugünden verilmiş.

③ ÖĞRENME HEDEFİ VARSA
   ██ Bu projeye özel ve dürüst olmak gerekir: bu depo bir ürün değil, bir
   öğrenme yüzeyi. Bir mekanizmayı öğrenmek için yazmak YAGNI ihlali değildir —
   ama o kodun ÜRETİM kodu gibi durmaması gerekir. Bu projede o ayrım
   02-sonraki-asamalar.md ile korunuyor: mekanizma YAZILMIYOR, ANLATILIYOR.
```

**MÜLAKAT CEVABI**

> **KISA (30 sn).** YAGNI, bugün gerekmeyeni bugün yazmamaktır. Projemde altı
> mekanizmayı bilerek yazmadım — nesne havuzu, ScriptableObject, olay veri yolu,
> singleton, ECS, profil altyapısı — ama hepsini **yazılı** hâle getirdim: her
> birinin bir tetikleyici koşulu var. Havuz örneği somut: `Instantiate`'in tek
> çağıranı `Awake` içinde iki satır, yani kare başına sıfır doğum var; havuzun
> azaltacağı maliyet ölçülebilir bile değil. YAGNI benim için "yapma" değil,
> "ne zaman yapacağını yaz".

> **GENİŞLETİLMİŞ (2 dk).** Bağlam: bu bir savaş çekirdeği, tahta 3×5 ve
> tahtada iki parça var. Gözlenen sorun: refleks olarak eklenen her mekanizma
> bir sözleşme getiriyor ve o sözleşme ölçülmediği için bakım borcu oluyor —
> havuzun sıfırlama sözleşmesi sekiz kalem: konum ve ebeveyn, fizik, can,
> sayaçlar, parçacık, olay abonelikleri, çift bırakma, kapasite. Seçtiğim
> mekanizma "yaz, uygulama": eksikleri bir dosyada tetikleyici koşullarıyla
> tuttum. En yakın alternatif hiçbir şey yazmamaktı; kaybettiği şey
> öğrenilebilirlik — "bugün önemli değil" bir yıl sonra "hiç öğrenmedim"e
> dönüşür. Kanıt: bir aşamanın koşulu gerçekleştiğinde ilk değişecek şey kod
> değil o satır, ve o satırın kendisi ilk değişecek **dosyayı** da yazıyor —
> ScriptableObject için `BoardAdapter.cs` değil, `GridStrategy.Combat.asmdef`,
> çünkü o dosya bugün `noEngineReferences: true` taşıyor. Ödün: bu tarz bir
> defter bakım ister ve bayatlarsa zararlıdır; bu yüzden bir makine kapısına
> bağladım — `check-curriculum-coverage.py` her satırın bir sahibi ya da bir
> aşaması olduğunu denetliyor.

---

## 8. DRY (Don't Repeat Yourself)

*****BU İLKE EN ÇOK YANLIŞ ANLAŞILANIDIR** ve mülakatta ayırt edici olan da
budur.***

**ADI VE KÖKENİ** — İngilizce *Don't Repeat Yourself*, kısaltması DRY; Türkçe
"kendini tekrar etme". `İYİ BİLİNEN`: 1999 tarihli *The Pragmatic Programmer*
kitabında (Andrew Hunt, David Thomas) adlandırıldı. Kitaptaki tanım kod
tekrarından değil **bilgi** tekrarından söz eder: bir sistemdeki her bilgi
parçasının tek, kesin ve yetkili bir temsili olmalıdır. ***Birincil kaynağa bu
oturumda bakılmadı.***

**NE DER** — ***DRY kod tekrarı hakkında **değildir**, **bilgi** tekrarı
hakkındadır.***

```
  İKİ YERDE AYNI SATIRIN OLMASI          >> İHLAL DEĞİL <<
  AYNI KARARIN İKİ YERDE YAŞAMASI        >> İHLAL <<
```

**BU PROJEDE NEREDE** — ***Doğru uygulanmış, ve projede hem OLUMLU hem OLUMSUZ
tarafın ölçüsü var.***

**Doğru uygulama — kural tek yerde, çağrı üç yerde:**

```
Assets/Game/Battle/TurnRules.cs:59           public static bool CanAct(Team unitTeam, Team currentTurn)
Assets/Game/Battle/BattleActions.cs:107      if (!TurnRules.CanAct(attackerCombatant.Team, battle.Turn.Current))
Assets/Game/Battle/BattleActions.cs:187      if (!TurnRules.CanAct(combatant.Team, battle.Turn.Current))
Assets/Game/Battle/BattleActions.cs:254      if (!TurnRules.CanAct(reviverCombatant.Team, battle.Turn.Current))
```

Üç eylem aynı soruyu soruyor. Cümlenin **metni** tek yerde; sorulduğu yer üç.
***Bu DRY'nin doğru uygulanmasıdır: tekrarlanan şey çağrı, karar değil.***

Aynı dosyada bir ikinci kat daha var — `TurnRules` kendi içinde de
tekrarlamıyor:

```
Assets/Game/Battle/TurnRules.cs:91           public static bool CanAct(Team unitTeam, Team currentTurn, int actionsUsedThisTurn)
```

Üç parametreli sürüm sıra kuralını kopyalamıyor, iki parametreli sürüme
**soruyor**. Gerekçe koddan: kopyalansaydı "tarafsız eyleyemez" kararı iki yerde
yaşardı ve biri değiştiğinde diğeri sessizce eskirdi.

*****VE ŞİMDİ İŞİN ZOR YARISI***** — projede üç ayrı dosyada, üç ayrı tipte,
**birebir aynı gövdeyi** taşıyan üç metot var:

```
Assets/Game/Core/Combat/AttackRules.cs:40    return attackerState == UnitState.Alive;
Assets/Game/Core/Combat/MovementRules.cs:49  return state == UnitState.Alive;
Assets/Game/Core/Combat/ReviveRules.cs:50    return reviverState == UnitState.Alive;
```

Naif DRY okuması burada bir ihlal görür ve üçünü birleştirmek ister. ***Kod bunu
açıkça reddediyor ve gerekçeyi tek cümlede yazıyor:***

```
Assets/Game/Core/Combat/MovementRules.cs:45  // Üç eyleyen kuralı bugün aynı satırı taşıyor; bu bir kesişme, bağ değil.
```

***"Kesişme, bağ değil" — DRY'nin doğru okumasının Türkçe karşılığı budur.***
Üç satır bugün aynı; ama üçü **ayrı üç bilgiyi** temsil ediyor: "kim vurur",
"kim yürür", "kim diriltir". Birleştirilseydi, "yaralı sıhhiyeci vuramaz ama
kaldırabilir" kararı verildiği gün üç kural birden değişir ve hiçbir test
kırmızıya dönmezdi.

**Karşı örnek arandı:** `MovementRules`'un `TargetingRules`'tan **türetilmiş**
bir hâli var mı? Yok — ve olmadığı da yazılı. `01-koda-gomulu-desenler.md` §1'in
REDDEDİLEN bloğu bu türetmeyi bir alternatif olarak koyup eliyor. Yani projede
DRY'nin **yanlış** uygulandığı bir yer bu oturumda **bulunamadı**.

**ÖLÇÜSÜ** — İki deney, iki yön:

```
  ① YANLIŞ BİRLEŞTİRME DENEYİ
     Üç kuralı tek bir ActorRules.CanAct(UnitState) altında birleştir.
     Bugün: bütün testler yeşil, hiçbir davranış değişmiyor.
     "Yaralı sıhhiyeci" kararı verildiği gün: bir kuralı değiştirirsin,
     >> üçü birden değişir ve hiçbir derleme hatası çıkmaz <<.
     Ölçü: değişiklik yarıçapı. Ayrıyken 1 dosya, birleşikken 3 akış.

  ② EKSİK BİRLEŞTİRME DENEYİ (karşı yön)
     TurnRules.CanAct'ı BattleActions'ın üç metoduna elle kopyala.
     Bugün: yine bütün testler yeşil.
     Tarafsızlık kuralı değiştiği gün: üç kopyadan ikisi güncellenir,
     biri unutulur ve >> yalnız diriltme akışında yanlış cevap doğar <<.
```

***İki deneyin ayırdığı şey aynı ölçüt: bu satır bir **karar** mı, yoksa bir
**tesadüf** mü?*** Karar ise tek yerde durmalı; tesadüf ise ayrılmalı.

**NE ZAMAN UYGULANMAZ** — ***Üç durumda***:

```
① TESADÜFİ BENZERLİK (coincidental duplication)
   Yukarıdaki üç kural. Aynı satır ≠ aynı bilgi.
   Ölçü: "biri değişirse öteki de değişmeli mi?" Cevap hayırsa birleştirme.

② FARKLI SINIRLARIN İKİ TARAFI
   Aynı bilgi iki assembly'de yaşıyorsa ve aralarında bir duvar varsa,
   birleştirmek duvarı yıkabilir. Bu projede ölçüsü var:
   MoveProfile'ın "range < 0" eşiği AttackProfile'ın "range < 1" eşiğine
   KOPYALANMADI — ikisi ayrı kural, çünkü hiçbir hücreye ulaşamayan bir
   SALDIRI anlamsız, hiçbir hücreye gidemeyen bir BİRİM anlamlı.

③ SATIR TASARRUFU İÇİN KALITIM
   >> DRY'yi kalıtımla uygulamak, ilkenin en pahalı yanlış kullanımıdır. <<
   unity-csharp-quality-flow.archive'ın reddetme koşulu tam olarak bu:
   "inheritance exists only to reuse a few lines". Bu projede Structure'ın
   `: Combatant` yazmayı reddetmesi aynı kararın uygulanmış hâli (6. ilke).
```

**MÜLAKAT CEVABI**

> **KISA (30 sn).** DRY kod tekrarı hakkında değil, **bilgi** tekrarı
> hakkındadır: iki yerde aynı satırın olması ihlal değil, aynı **kararın** iki
> yerde yaşaması ihlaldir. Projemden iki örnek veriyorum. Doğru uygulama: sıra
> kuralının metni tek bir tipte, üç eylem ona soruyor. Ve bilerek
> **birleştirmediğim** yer: üç ayrı kural sınıfında birebir aynı satır duruyor,
> `== UnitState.Alive`. Birleştirmedim çünkü bu bir kesişme, bağ değil — üçü
> ayrı üç soruyu temsil ediyor.

> **GENİŞLETİLMİŞ (2 dk).** Bağlam: savaşta üç eyleyen kuralı var — kim vurur,
> kim yürür, kim diriltir — ve üçünün de bugünkü cevabı aynı: yalnız ayakta
> olan. Gözlenen sorun: refleks olarak üçünü tek metoda indirmek istiyorsun,
> üç dosya bire iniyor. Seçtiğim mekanizma ayrı tutmak, sahibi üç ayrı `static`
> kural sınıfı. En yakın alternatif ortak bir `CanAct(UnitState)`; kaybettiği
> şey kuralların **ayrı ayrı değişebilme hakkı**. Ölçüsü şu: yarın "yaralı
> sıhhiyeci vuramaz ama kaldırabilir" kararını verdiğimde, birleşik sürümde bir
> satırı değiştiririm ve üç akış birden değişir — hiçbir test kırmızıya dönmez,
> çünkü bugün üçü zaten aynı cevabı veriyor. Kanıt kodda yazılı: gerekçe
> yorumu "bu bir kesişme, bağ değil" diyor ve aynı gerekçe hareket kuralının
> hedefleme kuralından **türetilmesini** de reddediyor. Ayırt edici soru tek:
> "biri değişirse öteki de değişmek zorunda mı?" Evet ise DRY uygulanır, hayır
> ise uygulanmaz. Ödün: üç dosya üç bakım noktası; birleştirme baskısı bir gün
> gerçekten doğarsa — üç kural gerçekten aynı **kararın** üç yüzü olursa —
> o gün birleştiririm, ama önce o kararın adını yazarım.

---

## 9. Separation of concerns (ilgi alanlarının ayrılması)

**ADI VE KÖKENİ** — İngilizce *separation of concerns*, kısaca SoC; Türkçe
"ilgi alanlarının ayrılması". `İYİ BİLİNEN`: terim Edsger W. Dijkstra'nın 1974
tarihli *On the role of scientific thought* yazısıyla anılır. ***Birincil
kaynağa bu oturumda bakılmadı.***

**NE DER** — Birbirinden bağımsız değişen işleri ayrı yerlerde tut.

**BU PROJEDE NEREDE** — ***Uygulanmış, ve **derleyici tarafından zorlanıyor**.
Bu projede ilgi ayrımı bir klasör düzeni değil, dört `.asmdef` dosyası.***

```
Assets/Game/Core/GridStrategy.Core.asmdef             references: []                        noEngineReferences: true
Assets/Game/Core/Combat/GridStrategy.Combat.asmdef    references: []                        noEngineReferences: true
Assets/Game/Battle/GridStrategy.Battle.asmdef         references: [Core, Combat]            noEngineReferences: true
Assets/Game/Unity/GridStrategy.Unity.asmdef           references: [Core, Combat, Battle]    noEngineReferences: false
```

***Kritik ayrım: `Core` ile `Combat`'ın `references` listesi **boş**.*** Yani
konum savaşı tanımıyor, savaş konumu tanımıyor, ve ikisi de motoru tanımıyor.
`GridStrategy.Unity` üçünü de tanıyor; tersi **derlenmez bile**.

Duvarın somut faturaları — mesafenin neden dışarıdan gelmek zorunda olduğu, bir
enum'un sahibinin üretemediği bir değeri nasıl taşıdığı, ikizlerin neden ayrı
katlarda yaşadığı — ayrı bir belgede ve burada **tekrar edilmiyor**:
[`../deep/konular/02-assembly-duvari.md`](../deep/konular/02-assembly-duvari.md).

***İSİMLENDİRME UYARISI*** — bu ilke `01-koda-gomulu-desenler.md` §4'te
**SOLID'in D'si** olarak da anılıyor ve ikisi aynı şey değil:

```
  Separation of concerns   İŞLER ayrı yerlerde mi
  Dependency inversion     OKUN YÖNÜ hangi tarafa bakıyor
```

Dört asmdef ikisini birden sağlıyor: işler ayrı (SoC) **ve** ok yalnız içeriye
bakıyor (D). Mülakatta ikisini karıştırmamak ayırt edici bir sinyaldir.

**ÖLÇÜSÜ** — Deney: `GridStrategy.Combat.asmdef`'in `noEngineReferences`
satırını `false` yap ve `UnitLifecycle`'a `Time.deltaTime` okut. Derleme geçer.
***Ve `Assets/Tests/EditMode/Combat/` altındaki testlerin tamamı PlayMode'a
taşınmak zorunda kalır — çünkü zamanı içeriden okuyan bir tasarım EditMode'da
**patlamaz**, sessizce 0,017675 döner.*** Bu, ölçülmüş bir olgu ve
`UnitLifecycle.cs:163-166`'da yazılı. İlgi ayrımının kaybı bir derleme hatası
olarak değil, bir **kanıt seviyesi düşüşü** olarak görünür.

**NE ZAMAN UYGULANMAZ** — ***İki durumda***:

```
① KÜÇÜK VE TUTARLI BİR DAVRANIŞ İÇİN KATMAN AÇMAK
   unity-csharp-quality-flow.archive'ın ilk satırı bunu yasaklıyor: tek bir
   GameObject'e gerçekten ait olan ve bağımsız kural/test/yeniden kullanım
   baskısı olmayan davranış için tek bir cohesive MonoBehaviour doğrudur.
   architecture-patterns.archive aynı şeyi Clean Architecture için tekrarlıyor:
   katmanlar "a tiny stable one-scene behavior" için reddedilir.

② AYRIM ÖLÇÜLEBİLİR BİR ŞEY GETİRMİYORSA
   Klasör ve ad alanı tek başına mimari uygulamaz — bu cümle bu projede
   ölçülmüş bir olgu, çünkü klasör ≠ ad alanı ≠ assembly ve ayrımın gerçek
   sahibi asmdef. Ayrımı asmdef'e taşımayan bir klasör düzeni bir SoC
   uygulaması değil, bir dosya düzenidir.
```

***Bu projedeki `BoardAdapter` bu sınırın kendi itirafını taşıyor*** — künyesi
rolünü "karma" diye yazıyor, ve altında bir **koku notu** duruyor: eşiğin
aşıldığı yazılı ve silinmemiş.

```
Assets/Game/Unity/BoardAdapter.cs:68         // ═══ ROL: KARMA — ÇEVİRMEN + VARLIK (Adapter + Entity) ═══════════
Assets/Game/Unity/BoardAdapter.cs:83         // KOKU   : evet ve BÜYÜDÜ. EŞİK AŞILDI — ve notu SİLMİYORUM, çünkü bir
Assets/Game/Unity/BoardAdapter.cs:88         //          SIRADAKİ EŞİK → BoardAdapter.md#rol
```

Yani ilke burada mükemmel uygulanmamış ve bu **bilinerek** böyle: eşiği aştığını
söyleyen satır, eşiği koyan satır kadar öğreticidir.

**MÜLAKAT CEVABI**

> **KISA (30 sn).** İlgi ayrımı, birbirinden bağımsız değişen işleri ayrı
> yerlerde tutmaktır. Projemde bunu klasörle değil **derleyiciyle** yaptım: dört
> `.asmdef` var, çekirdek iki tanesinin referans listesi boş ve
> `noEngineReferences: true`. Yani savaş kuralları Unity'yi tanımıyor ve
> testleri sahne kurmadan koşuyor. Tersi derlenmiyor bile — bu bir disiplin sözü
> değil, bir derleme hatası.

> **GENİŞLETİLMİŞ (2 dk).** Bağlam: savaş kurallarını Unity olmadan sınamak
> istiyordum, ama girdi, kamera, zaman ve nesne doğurma motorsuz yaşayamaz.
> Gözlenen sorun: aynı derlemede durdukları sürece "kural motora dokunmasın"
> cümlesi birinin hatırlamasına bağlı kalıyor ve unutulduğu gün sessizce
> düşüyor. Seçtiğim mekanizma assembly sınırı, sahibi dört `.asmdef` dosyası. En
> yakın alternatif klasör ve ad alanı ayrımıydı; kaybettiği şey zorlama —
> klasör bir `using` satırını engellemez. Kanıt: `GridStrategy.Battle`,
> `GridStrategy.Unity`'yi göremiyor ve o satır derlenmiyor; ayrıca zamanın
> dışarıdan verilmesi ölçülmüş bir sebeple — EditMode'da `Time.deltaTime` sıfır
> değil, 0,017675 dönüyor, yani içeriden okuyan bir tasarım testte patlamaz,
> **sessizce anlamsız bir sayıyla yürür**. Ödün: sınırın bedeli gerçek — mesafe
> akış katmanına çıkmak zorunda kaldı, çünkü konumu `Core` biliyor, kuralı
> `Combat`, ve ikisi birbirini görmüyor. Bu yüzden `BattleActions` diye bir tip
> var. Ve ayrımı mükemmel uygulamadım: `BoardAdapter` hem çevirmen hem varlık
> ve bunu dosyanın künyesine yazdım — eşiğin aşıldığını gizlemek, aşmaktan daha
> kötü olurdu.

---

## 10. ***İLKELER ÇATIŞTIĞINDA***

***Bu bölüm bu dosyanın en değerli yeri.*** Mülakatta ayırt edici soru "bu
ilkeyi bilir misin" değildir — o soruyu herkes geçer. Ayırt edici soru şudur:

> *"Bu iki ilke aynı satırda zıt şeyler söylüyor. Hangisini seçersin ve neden?"*

İlkeler bir kural kümesi değil, bir **kuvvet alanıdır**. Her biri bir yöne
çeker ve bazı satırlarda iki kuvvet zıt yöne bakar. Aşağıda bu projeden **üç
gerçek çatışma** var; üçünün de kazananı kodda yazılı.

### Çatışma A — DRY ile tek sorumluluk (S)

```
  DURUM      Üç kural sınıfı, üç dosya, birebir aynı gövde:
             AttackRules.cs:40 · MovementRules.cs:49 · ReviveRules.cs:50

  DRY DER    "Aynı satır üç yerde. Birleştir."
  S DER      "Üç ayrı sorumluluk. Ayrı tut."

  >> KAZANAN: S <<
```

**NEDEN** — Ölçüt "satır aynı mı" değil, **"biri değişirse öteki de değişmek
zorunda mı"** sorusudur. Cevap hayır: "yaralı sıhhiyeci vuramaz ama kaldırabilir"
kararı verilebilir bir karardır ve o gün iki kural ayrışır. Kod bu hükmü tek
cümlede veriyor: *bu bir kesişme, bağ değil* (`MovementRules.cs:45`).

***Ve DRY tamamen kaybetmiyor*** — aynı dosyada DRY'nin **kazandığı** bir yer
de var: `TurnRules.cs:91`'deki üç parametreli sürüm, sıra kuralını
kopyalamıyor, iki parametreli sürüme soruyor. Yani aynı iki kuvvet iki satırda
iki farklı sonuç veriyor ve ayıran şey ölçüttür, tercih değil.

### Çatışma B — DRY ile Law of Demeter

```
  DURUM      battle.Turn.EndTurn()  — üç kez, BattleActions.cs:143 · :216 · :304

  DEMETER DER  "Komşunun komşusuna dokunma. Battle'a bir EndTurn() ileticisi ekle."
  DRY DER      "İletici eklersen, devrin ne zaman olacağı kararı iki yerde yaşar."

  >> KAZANAN: DRY <<
```

**NEDEN** — Devri tetikleyen şey bir **beyaz listedir**: hangi sonuç değerleri
sırayı devreder. O liste akış sahibinde yaşıyor ve `AttackOutcome` ile
`MoveOutcome` tiplerini tanıyor. `Battle`'a taşınsaydı `Battle` da o iki tipi
tanımak zorunda kalırdı — ve `Battle`, `Attack` ile `Move` akışlarını
tanımıyor, tanımamalı. ***İhlalin bedeli ölçüldü ve kabul edildi: `EndTurn`'ün
imzası değişirse derleyici üç yer gösterir, `Battle` görünmez.*** Bu bilinen ve
yazılı bir borç, bir gözden kaçma değil.

### Çatışma C — Fail fast ile oyuncuya nazik hata

```
  DURUM      Oyuncu menzil dışı bir hücreye tıklıyor.

  FAIL FAST DER    "Geçersiz girdi. Patla, sebebi orada söyle."
  KULLANICI DENEYİMİ DER  "Bu bir hata değil, bir oyun olgusu. Bir mesaj göster ve devam et."

  >> KAZANAN: ikisi de — çünkü İKİ AYRI KANAL açıldı <<
```

**NEDEN** — Bu çatışmanın çözümü "birini seç" değil, **soruyu yeniden sormak**
oldu. Ayırt edici ölçüt kodda yazılı (`BattleActions.cs:370-373`): *bu cevabı
alan çağıran yapacak bir şey bulabilir mi?* Bulabiliyorsa sonuç değeri,
bulamıyorsa istisna. `null` bir `Battle` için çağıranın yapacağı bir şey yok →
`throw`. Menzil dışı bir hücre için var → `AttackOutcome.RejectedOutOfRange`.

***Bu, çatışma çözümünün en güçlü biçimidir: iki ilke zıt görünüyorsa, ikisinin
farklı **alanlara** ait olma ihtimalini önce sına.*** Sıra da bir karar ve
kodda yazılı: önce bütün çağıran hataları (istisna), sonra bütün kurallar
(sonuç değeri). Bir kural geri dönülemez adımın altına düşerse kural olmaktan
çıkıp açıklamaya döner.

### Çatışma D — YAGNI ile açık/kapalı (O)

```
  DURUM      AttackOutcome'a yeni bir ret değeri eklemek.

  YAGNI DER  "Bugün gereken beş değer var. Altıncıyı yazma."
  O DER      "Yeni davranış eklenirken var olan kod değişmemeli. Bugünden hazırlan."

  >> KAZANAN: YAGNI — ama O'ya bir ödün verildi <<
```

**NEDEN** — Altıncı değer **yazılmadı** (YAGNI). Ama yazıldığı gün ne olacağı
**bugünden** karara bağlandı: yeni değerler **sona** eklenir, ret ailesinin
yanına sokulmaz — çünkü sokulsaydı aradaki değerler sessizce yeniden
numaralanırdı. Aynı biçim kurallarda da var: beyaz liste (`== Alive`), kara
liste (`!= Downed && != Dead`) değil. İki biçim bugün aynı cevabı veriyor;
fark dördüncü değer eklendiği gün doğar.

***Ayrım şu: YAGNI **kodu** yazmamaktır, **kararı** ertelemek değil.***
Geri alınması pahalı kararlar (sıfırıncı enum değeri, değer sırası, beyaz
liste biçimi) bugün verilir; geri alınması ucuz kod (altıncı değerin kendisi)
bugün yazılmaz.

### Çatışmaların ortak ölçütü

```
  ① İki ilke aynı satırda zıt söylüyorsa, önce ikisinin AYNI ALANDAN
     bahsedip bahsetmediğini sına. (Çatışma C bu şekilde çözüldü.)

  ② Hâlâ zıtsa, sor: hangi ilkenin ihlali SESSİZ kalır?
     Sessiz kalan ihlal daha pahalıdır ve o ilke kazanır.
     (Çatışma A: DRY ihlali gürültülüdür — üç dosya bakarsın.
      S ihlali sessizdir — bir satır değişir, üç akış değişir, test yeşil kalır.)

  ③ Kaybeden ilkenin bedelini YAZ ve silme.
     (Çatışma B: Demeter kaybetti ve borç kodda duruyor.)

  >> ④ "İki ilkeyi de tam uyguladım" cevabı mülakatta bir ZAYIFLIK sinyalidir. <<
     İlkeler bazı satırlarda çatışır; çatışmayı görmemiş olmak, o satıra hiç
     bakmamış olmak demektir.
```

---

## 11. Üç oyun — hangi ilke o oyunun mimarisini en çok şekillendirirdi

*****UYARI: bu tablonun tamamı DOĞRULANMAMIŞTIR.***** Slay the Spire, Vampire
Survivors ve Stardew Valley'nin kaynak kodu kapalı. Aşağıdaki satırlar oyunların
**oynanışından çıkarılmış tahminlerdir**, mimari bilgi değil. Mülakatta "şu oyun
şöyle yapmıştır" demek bir iddiadır ve savunulamaz; "o oynanış şu ilkeye baskı
yapardı" demek bir muhakemedir ve savunulabilir.

| İlke | Slay the Spire | Vampire Survivors | Stardew Valley |
|---|---|---|---|
| Fail fast | kart oynanamadığında sebep tek tek ayrılır; hangi kısıtın çiğnendiği oyuncuya söylenir | ██ EŞLEŞMEZ ██ oyuncuya dönen bir ret kanalı görünmüyor; hasar ya olur ya olmaz | bir alet yanlış yerde kullanıldığında iş yapmaz ve sebebi ayrı ayrı geri bildirilir |
| Tell, Don't Ask | kart "kendini oyna" der; enerjiyi, hedefi ve etkiyi ayrı ayrı sorgulayan bir dış karar görünmüyor | ██ EŞLEŞMEZ ██ silahlar kendi zamanlayıcılarıyla ateşler, birine bir şey söyleyen taraf yok | bir eşya "kullanılabilir mi" sorusunu kendi taşır; sandık, tarla ve fırın ayrı ayrı cevap verir |
| Tek doğruluk kaynağı | koşu boyunca tek deste, tek altın, tek tırmanış durumu; ikisi aynı anda olamaz | tek sahne süresi sayacı bütün oyunu yönetir | tek takvim ve tek saat; ekinler, hayvanlar ve etkinlikler hepsi ona bakar |
| Dependency Injection | ██ EŞLEŞMEZ ██ oynanıştan bağımlılık kanalı hakkında hiçbir şey görünmüyor | ██ EŞLEŞMEZ ██ aynı sebep | ██ EŞLEŞMEZ ██ aynı sebep |
| Law of Demeter | ██ EŞLEŞMEZ ██ iç yapı hakkında oynanıştan çıkarım yapılamaz | ██ EŞLEŞMEZ ██ aynı sebep | ██ EŞLEŞMEZ ██ aynı sebep |
| Kalıtım yerine bileşim | kartın maliyeti, hedef sayısı ve etkisi ayrı ayrı tanımlı; tek bir "saldırı kartı" kalıbı yok | silah menzil, hız ve hasar parçalarından kurulur; yükseltmeler parçalara ayrı dokunur | eşya satılabilir, hediye edilebilir, yenebilir olabilir; bunlar bir tür ağacından değil özelliklerden gelir |
| YAGNI | ██ EŞLEŞMEZ ██ hangi mekanizmanın ne zaman eklendiği dışarıdan görünmüyor | ekranda yüzlerce düşman var ve oyun ölçünün gerektirdiği yere kadar gitmiş görünüyor | ██ EŞLEŞMEZ ██ aynı sebep |
| DRY | aynı kartın iki kopyası aynı metni ve maliyeti taşır, ama biri yükseltilmişse ayrılır | yüzlerce aynı düşman aynı hasar ve hız tanımını paylaşır, canları ayrıdır | her "kırmızı lahana" aynı fiyatı ve büyüme süresini taşır, hangi tarlada olduğu her birine özeldir |
| Separation of concerns | kart üstündeki sayı ile ekrandaki animasyon ayrı ilerler; sayı değişmeden animasyon oynamaz | yüzlerce düşman görselinin arkasında konum ve can ayrı tutulur, görsel yalnız takip eder | ekinin büyüme günü ile ekrandaki görseli ayrı ilerler; oyun kapalıyken de gün geçer |

***Tablodaki `EŞLEŞMEZ` satırlarının çoğu aynı sebeple işaretli*** ve bu
sebebin kendisi bir ders: **Dependency Injection ve Law of Demeter iç yapı
ilkeleridir; oynanıştan gözlemlenemezler.** Bir oyunu oynayarak öğrenilebilecek
şey davranıştır, mimari değil. Bir mülakatta "Vampire Survivors şöyle bir
mimari kullanıyor" demek, kaynağı görmediysen bir uydurmadır.

---

## 12. Kural, yanlış hatırlananlar, kaçış yolu

### Kural — hangi ilkeyi ne zaman öne alırsın

```
  Bir satıra bakıyorsun ve iki ilke zıt söylüyor. Sırayla sor:

  ① Bu iki ilke AYNI ALANDAN mı bahsediyor?
        HAYIR → ikisi de uygulanır, iki ayrı kanal aç. (Çatışma C)
        EVET  → ②

  ② İhlallerden hangisi SESSİZ kalır?
        biri sessiz → >> sessiz olan kazanır <<
                      (test yeşil kalan ihlal, en pahalı ihlaldir)
        ikisi de gürültülü → ③

  ③ Hangi kararı geri almak PAHALI?
        enum değer sırası, asmdef yönü, veri formatı → bugün karara bağla
        metot gövdesi, sınıf sayısı, iletici metot   → ertele
        (Çatışma D bu dalda çözüldü)

  ④ Kaybeden ilkenin BEDELİNİ yaz.
        >> Yazılmayan borç, bir gün gözden kaçma sayılır. <<
        (Çatışma B'nin borcu BattleActions'ta duruyor)
```

### Yanlış hatırlanan üç şey

```
"DRY = kod tekrarı yok"
   >> DEĞİL. << DRY bilgi tekrarı hakkındadır. Bu projede üç ayrı dosyada
   BİREBİR aynı satır duruyor ve bu bir ihlal değil — üçü ayrı üç kararı
   temsil ediyor.
   Ölçü: AttackRules.cs:40 · MovementRules.cs:49 · ReviveRules.cs:50,
         üçü de `== UnitState.Alive` ve gerekçe MovementRules.cs:45'te:
         "bu bir kesişme, bağ değil".
   Doğru soru: "biri değişirse öteki de değişmek zorunda mı?"

"DI = bir çerçeve (Zenject, VContainer)"
   >> DEĞİL. << DI bir tasarım kararıdır: parçayı içeride kurma, dışarıdan al.
   Konteyner o kararı otomatikleştiren AYRI bir araçtır ve genellikle gerekmez.
   Ölçü: bu projede konteyner YOK ama DI her yerde — Combatant.cs:59 dört
         parçayı da kurucudan alıyor, Unity tarafında kanal [SerializeField].
   Yanlış cevabın bedeli: "DI kullanmadım" demek, kullandığın şeyin adını
   bilmediğini söylemektir.

"İlkeleri ne kadar çok uygularsan o kadar iyi"
   >> DEĞİL. << Her ilkenin bir aşırı uygulama biçimi var ve adı da var:
   Demeter'ın aşırısı "orta adam" (her zincir için bir iletici metot),
   DRY'nin aşırısı tesadüfi benzerliği birleştirmek,
   SoC'nin aşırısı küçük ve tutarlı bir davranış için katman açmak,
   fail fast'in aşırısı oyuncu hatasını istisnaya çevirmek.
   Ölçü: bu dosyadaki dokuz ilkenin dokuzunda da bir "NE ZAMAN UYGULANMAZ"
         alanı var ve bu tesadüf değil — sınırsız ilke bir kural değil bir
         refleks olur.
```

### Kaçış yolu — bu ilkelerin hiçbiri uygulanmasaydı

Dürüst cevap: ***proje yine çalışırdı.*** 33 dosya, 3×5 tahta ve iki birim
için tek bir `MonoBehaviour` yeterdi — kurallar `if` olarak akışın içinde
yaşardı, tahta da savaş da aynı sınıfta dururdu, ve oyun bugünkü hâliyle aynı
görünürdü.

Kaybedilecek şey bugün değil **ikinci gün** görünür:

```
  bugün           tek dosya çalışıyor, testler yeşil, oyun aynı
  ikinci hafta    üçüncü birim türü geliyor ve kural üç yerde
  birinci ay      bir kural değişiyor, iki yer güncelleniyor, biri unutuluyor
  >> ve o unutma bir DERLEME HATASI olarak değil, bir OYUN HATASI olarak doğuyor <<
```

Bu projede o günün maliyeti **ölçüldü** ve ölçü şu: `Assets/Tests/EditMode/`
altında 26 test dosyası var ve neredeyse hepsi sahne kurmadan koşuyor. Tek
`MonoBehaviour` çözümünde o 26 dosyanın tamamı PlayMode'a taşınırdı, çünkü
kurallar motorsuz kurulamazdı. ***İlkelerin bu projede satın aldığı şey mimari
güzellik değil, bir **kanıt seviyesidir**.***

---

## Alıntı çapaları

Aşağıdaki satırlar bu belgede geçen satır numaralarının **çapasıdır**. Her satır
`Tools/check-doc-code-refs.py`'nin ALINTI katmanına, o numarada duran kodun
BİREBİR metnini verir. Ölçüldü: ALINTI katmanı 3 satırlık kaymayı bile %100
yakalıyor, YAKIN AD katmanı 6 satırlık kaymanın %1'ini. Tablo hücrelerindeki ve
cümle içindeki atıflar alıntı biçimine giremez — o biçim atfın satır BAŞINDA
olmasını ister. Kod kaydığında kızacak olan yer burasıdır; kızdığı gün bu
belgede geçen aynı numaraların hepsi elden geçirilir.

```
Assets/Game/Battle/BattleActions.cs:50       public static class BattleActions
Assets/Game/Battle/BattleActions.cs:61       public static AttackOutcome Attack(Battle battle, Unit attacker, Unit target)
Assets/Game/Battle/BattleActions.cs:65       throw new ArgumentNullException(nameof(battle));
Assets/Game/Battle/BattleActions.cs:82       Combatant attackerCombatant = RequireCombatant(battle, attacker, nameof(attacker));
Assets/Game/Battle/BattleActions.cs:107      if (!TurnRules.CanAct(attackerCombatant.Team, battle.Turn.Current))
Assets/Game/Battle/BattleActions.cs:143      battle.Turn.EndTurn();
Assets/Game/Battle/BattleActions.cs:187      if (!TurnRules.CanAct(combatant.Team, battle.Turn.Current))
Assets/Game/Battle/BattleActions.cs:254      if (!TurnRules.CanAct(reviverCombatant.Team, battle.Turn.Current))
Assets/Game/Battle/BattleActions.cs:355      if (battle.TryGetUnit(x, y, out Unit _))
Assets/Game/Battle/BattleActions.cs:375      private static Combatant RequireCombatant(Battle battle, Unit unit, string paramName)
Assets/Game/Battle/BattleActions.cs:379      throw new ArgumentException("The unit is not in this battle.", paramName);
Assets/Game/Battle/BattleActions.cs:389      private static void RequireCell(
Assets/Game/Battle/BattleActions.cs:392      if (!battle.TryGetPosition(unit, out x, out y))
Assets/Game/Battle/Battle.cs:53              private readonly UnitGrid board;
Assets/Game/Battle/Battle.cs:107             internal UnitGrid Board => board;
Assets/Game/Battle/Battle.cs:154             public TurnState Turn { get; }
Assets/Game/Battle/Battle.cs:385             pair.Value.Tick(deltaSeconds);
Assets/Game/Battle/Battle.cs:440             if (pair.Value.IsReadyForCleanup)
Assets/Game/Battle/Battle.cs:528             public bool TryGetPosition(Unit unit, out int x, out int y)
Assets/Game/Battle/TurnRules.cs:59           public static bool CanAct(Team unitTeam, Team currentTurn)
Assets/Game/Battle/TurnRules.cs:91           public static bool CanAct(Team unitTeam, Team currentTurn, int actionsUsedThisTurn)
Assets/Game/Core/Combat/AttackRules.cs:38    public static bool CanAttack(UnitState attackerState)
Assets/Game/Core/Combat/AttackRules.cs:40    return attackerState == UnitState.Alive;
Assets/Game/Core/Combat/MovementRules.cs:47  public static bool CanMove(UnitState state)
Assets/Game/Core/Combat/MovementRules.cs:49  return state == UnitState.Alive;
Assets/Game/Core/Combat/ReviveRules.cs:48    public static bool CanRevive(UnitState reviverState)
Assets/Game/Core/Combat/ReviveRules.cs:50    return reviverState == UnitState.Alive;
Assets/Game/Core/Combat/Combatant.cs:59      public Combatant(
Assets/Game/Core/Combat/Combatant.cs:70      this.health = health ?? throw new ArgumentNullException(nameof(health));
Assets/Game/Core/Combat/Combatant.cs:72      AttackProfile = attackProfile ?? throw new ArgumentNullException(nameof(attackProfile));
Assets/Game/Core/Combat/Combatant.cs:152     public UnitState State => lifecycle.State;
Assets/Game/Core/Combat/Combatant.cs:154     public int CurrentHealth => health.Current;
Assets/Game/Core/Combat/Structure.cs:51      public Structure(
Assets/Game/Core/Combat/Health.cs:31         public Health(int max)
Assets/Game/Core/Combat/AttackProfile.cs:49  public AttackProfile(int damage, int range)
Assets/Game/Core/Combat/AttackAction.cs:98   target.TakeDamage(attacker.AttackProfile.Damage);
Assets/Game/Core/Combat/AttackAction.cs:169  return target.TakeDamage(attacker.AttackProfile.Damage)
Assets/Game/Core/MoveProfile.cs:50           public MoveProfile(int range)
Assets/Game/Core/PointerGesture.cs:127       public PointerGesture(float dragThreshold)
Assets/Game/Core/MoveOutcome.cs:26           public enum MoveOutcome
Assets/Game/Core/Combat/AttackOutcome.cs:27  public enum AttackOutcome
Assets/Game/Battle/PlacementOutcome.cs:24    public enum PlacementOutcome
Assets/Game/Battle/ReviveOutcome.cs:25       public enum ReviveOutcome
Assets/Game/Unity/BoardAdapter.cs:110        public sealed class BoardAdapter : MonoBehaviour
Assets/Game/Unity/BoardAdapter.cs:124        [SerializeField] private UnitView unitPrefab;
Assets/Game/Unity/BoardAdapter.cs:550        Team team = battle.TryGetCombatant(placer, out Combatant combatant)
Assets/Game/Unity/BoardAdapter.cs:739        UnitView view = Instantiate(unitPrefab, transform);
Assets/Game/Unity/BoardAdapter.cs:1007       Destroy(view.gameObject);
Assets/Game/Unity/BoardAdapter.cs:1079       private string DescribeCondition(Unit unit)
Assets/Game/Unity/BoardAdapter.cs:1086       return $"health={combatant.CurrentHealth}, state={combatant.State}";
Assets/Game/Unity/UnitView.cs:43             public sealed class UnitView : MonoBehaviour
```

İki yorum satırı da çapa, çünkü bu dosyanın tezi tam olarak onlara dayanıyor:

```
Assets/Game/Battle/Battle.cs:526             // kararı değil, ikinci bir doğruluk kaynağı yaratma kararıdır.
Assets/Game/Core/Combat/MovementRules.cs:45  // Üç eyleyen kuralı bugün aynı satırı taşıyor; bu bir kesişme, bağ değil.
Assets/Game/Core/Combat/Structure.cs:19      // yarısına uymaz — TryRevive, Downed hâli, zorunlu AttackProfile, on
```

---

## Bu dosyanın kendi sınırı

Köken tarafının sınırı yukarıda yazılı ve tekrar edilmiyor: hiçbiri birincil
kaynağa karşı doğrulanmadı. Karşılık tarafı ise tersi — 137 atfın 105'i, o
satırda duran kodun birebir metnini taşıyor ve makine kapısının ALINTI
katmanına bağlı.

`Docs/ogrenme/` ağacının üçüncü kuralı burada da geçerli: karşılığı olmayan
satır işaretlenir, doldurulmaz. Bu dosyada iki arama boş döndü ve ikisi de
uydurma bir karşılıkla kapatılmadı — Tell-Don't-Ask'ın gerçek bir ihlali
**bulunamadı**, DRY'nin yanlış uygulandığı bir yer **bulunamadı**.

## İlgili

- Bu ağacın yönlendirmesi: [README.md](README.md)
- Kodda zaten duran dokuz desen: [01-koda-gomulu-desenler.md](01-koda-gomulu-desenler.md)
- Eksiklerin tetikleyici koşulları: [02-sonraki-asamalar.md](02-sonraki-asamalar.md)
- Kapsama tablosu: [03-kavram-borc-defteri.md](03-kavram-borc-defteri.md)
- `Docs/deep/` ağacının okuma sırası: [00-okuma-sirasi.md](00-okuma-sirasi.md)
- İstisna ile sonuç değerinin bölüşümü: [../deep/kod/Battle/BattleActions.md](../deep/kod/Battle/BattleActions.md)
- Ret sırası ve geri dönülemez nokta: [../deep/konular/04-karar-sirasi.md](../deep/konular/04-karar-sirasi.md)
- Sonuç enum'larının anatomisi: [../deep/konular/06-sonuc-enumlari.md](../deep/konular/06-sonuc-enumlari.md)
- Tahtanın tek sahibi: [../deep/konular/03-tahta-sahipligi.md](../deep/konular/03-tahta-sahipligi.md)
- Assembly duvarı ve dört faturası: [../deep/konular/02-assembly-duvari.md](../deep/konular/02-assembly-duvari.md)
- `interface`in bu projede neden sıfır olduğu: [../deep/dil/08-erisim-ve-sozlesme.md](../deep/dil/08-erisim-ve-sozlesme.md)
- Hata bildirme ve doğrulama: [../deep/dil/03-hata-bildirme-ve-dogrulama.md](../deep/dil/03-hata-bildirme-ve-dogrulama.md)
- Kodun kendi gerekçeleri: [../deep/README.md](../deep/README.md)
