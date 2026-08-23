# Öğrenme defteri — yönlendirme

`Docs/deep/` **kodun** ne dediğini anlatıyor. Bu ağaç **okuyanın** nerede
durduğunu yazıyor. İki soru farklı, o yüzden iki ağaç.

Bu ağacın var olma sebebi tek bir eksiklikti: proje kodunda gerçekten uygulanmış
mimari kararlar var — durum makinesi, bileşim, olay yayını, kural nesnesi — ama
**hiçbiri adıyla anılmıyor**. Aynı anda, projede henüz yapılmamış olan şeylerin
(nesne havuzu, ScriptableObject, olay veri yolu, ECS) **hiçbiri de yazılı
değil**. Sonuç: öğrenen kişi neyi bildiğini de neyi bilmediğini de göremiyor.
Bilinmeyenin adı yoksa öğrenilecek şeyin de adı yoktur.

## `Docs/deep/` üç ağacına karşı konumu

| Ağaç | Bölünme | Cevapladığı soru | Otoritesi |
|---|---|---|---|
| [`deep/kod/`](../deep/kod/README.md) | tip başına | "bu üye neden böyle" | kod |
| [`deep/konular/`](../deep/konular/) | mekanizma başına | "bu akış nasıl çalışıyor" | kod |
| [`deep/dil/`](../deep/dil/README.md) | kavram başına | "bu dil özelliği ne vaat ediyor" | derleyici ve çalışma zamanı |
| `ogrenme/` (burası) | **yetkinlik başına** | **"ben nerede duruyorum"** | **kod + bu defterin kapısı** |

Ayrım pratikte şöyle görünür. `deep/kod/Core/Combat/UnitLifecycle.md`
"`SetState` neden tek giriş noktası" sorusunu cevaplar. Bu ağaç aynı dosyaya
bakıp şunu yazar: *"bu bir durum makinesidir, adı budur, hangi baskı onu
doğurdu, hangi SOLID harfini taşıyor, ve sen bu kavramı kapattın."*

Yani buradaki hiçbir satır kodu **açıklamaz**. Her satır kodu **etiketler** ve
etiketi bir yetkinlik listesine bağlar.

## Bu ağacın dört sorusu ve üç dosyası

```
Docs/ogrenme/
├── README.md                     ◄── buradasın: neden var, nasıl okunur
├── 01-koda-gomulu-desenler.md        "BUGÜN NEYİ BİLİYORUM"
│                                     kodda gerçekten duran desenler, adlarıyla
├── 02-sonraki-asamalar.md            "BUGÜN NE YOK, NE ZAMAN GELİR"
│                                     eksiklerin tetikleyici koşulları
└── 03-kavram-borc-defteri.md         "HANGİ KAVRAMIN SAHİBİ VAR"
                                      kapsama tablosu: kavram × sahip dosya
```

| Dosya | Ne yapar | Ne YAPMAZ |
|---|---|---|
| [00-okuma-sirasi.md](00-okuma-sirasi.md) | `Docs/deep/` ağacını **hangi sırayla** okuyacağını 14 adım ve 5 oturuma böler; her adımın ön koşulunu, yanında açık duracak `.cs` dosyasını ve bitiş koşturmasını yazar | Hiçbir mekanizmayı **anlatmaz** — bir yol tarifi, bir ders değil. Dosya numaralarını da değiştirmez: ██ numara kimliktir, sıra buradadır ██ |
| [01-koda-gomulu-desenler.md](01-koda-gomulu-desenler.md) | Kodda **doğrulanmış** desenleri adlandırır; her biri için baskıyı, dosya:satır yerini, SOLID karşılığını ve reddedilen rakibi yazar | Kodda olmayan bir deseni "var" diye yazmaz; ders kitabı tanımı vermez |
| [02-sonraki-asamalar.md](02-sonraki-asamalar.md) | Bugün olmayan altı konuyu, her biri için **tetikleyici koşulla** birlikte yazar | Hiçbir şey **önermez**. "Şunu şimdi ekleyelim" cümlesi bu ağaçta yasaktır |
| [03-kavram-borc-defteri.md](03-kavram-borc-defteri.md) | Yetkinlik evrenindeki her kavrama bir **sahip dosya** ya da bir **aşama** yazar | Boş hücre bırakmaz; sahipsiz kavram bırakmaz |

## Üç kural — bu ağaç yazılırken uyulan

**① Baskı adlandırılmadan desen tanıtılmaz.** Bir desenin adı, onu doğuran
somut sıkıntıdan sonra gelir. "Burada Observer var" cümlesi tek başına
öğretmez; "üç durak sonraki ekran, `Downed → Dead` geçişini soran kimse
olmadığı için duyamıyordu" cümlesi öğretir. Ad ikinci gelir.

**② "Bugün önemli değil" eksik bir cümledir.** Onu tamamlayan şey, önemli hâle
getirecek **koşuldur** ve aynı cümlede yazılır. `02-sonraki-asamalar.md`'deki
her satırın bir **TETİKLEYİCİ KOŞUL** alanı vardır; ölçüsüz bir "gerekirse
eklenir" cümlesi o dosyada ihlaldir.

**③ Karşılığı olmayan satır işaretlenir, doldurulmaz.** Bir kavramın bu projede
karşılığı yoksa cevap `HENÜZ YOK`'tur ve yanına onu yaratacak aşamanın adı
yazılır. Uydurma bir benzetmeyle kapatılmaz.

## Otorite — bu ağaç bir gerçek kaynağı DEĞİLDİR

Kod kazanır. Bu ağaç bir **defterdir**: neyin okunduğunu, neyin adlandırıldığını
ve neyin hâlâ borç olduğunu tutar. Kodla çelişirse bayat olan burasıdır.

Ama defter olmak "gevşek olmak" demek değil. Buradaki her `dosya:satır` atfı
yazıldığı gün açılıp sayılmıştır, ve `03-kavram-borc-defteri.md` bir **makine
kapısıyla** bağlanmıştır:

```
python Tools/check-curriculum-coverage.py
```

Kapı şunları arar:

```
KAPALI / KISMİ  satırı  ──►  SAHİP DOSYA gerçekten var mı?
                             satır numarası verilmişse dosya o kadar satır taşıyor mu?
HENÜZ YOK       satırı  ──►  onu yaratacak AŞAMA yazılı mı?
her satır               ──►  DURUM hücresi boş mu?
```

Kapı önce **kendi ayrıştırıcısını** sınar: bilinen-iyi bir satırı çözemezse
"KAPI BOZUK" der ve sıfır dışı kodla çıkar. Sebebi ölçülmüş bir olaydır — bu
projede bir kapı, anahtarları yanlış normalize ettiği için dört kez yanlışlıkla
"temiz" dedi (`Tools/check-doc-links.py`'nin `anchors` sözlüğünün üstündeki
nota bak). Sessizce yeşil yanan bir kapı, hiç olmayan bir kapıdan **daha
kötüdür**: birincisi güven üretir.

## Nasıl okunur

██ **`Docs/deep/` ağacına ilk kez oturuyorsan buradan başlama** ██ — o üç ağacın
okuma sırası ayrı bir belgede ve **dosya numaralarına uymuyor**:
[00-okuma-sirasi.md](00-okuma-sirasi.md) (14 adım, 5 oturum, 9-11 saat). Bu
ağacın kendisi orada **ADIM 14**'tür, yani en sonda.

Bu ağacın kendi içinde zorunlu bir sıra yok, ama bir öğrenme sırası öneriliyor:

1. **`01`** — önce elindekini gör. Bilmediğini öğrenmeden önce bildiğinin adını
   öğren; bir sonraki mülakat sorusu "bu projede hangi desenleri kullandın"
   olacak ve cevabın kodda duruyor.
2. **`03`** — sonra haritaya bak. Hangi kavramın sahibi var, hangisi borç.
3. **`02`** — en sonda ileriye bak. Her satır "ne zaman" sorusuna cevap verir,
   "şimdi mi" sorusuna değil.

Dosyalara `Ctrl + P` ile gelinir; ad ayırt edici parçasıyla yazılır
(`borc-defteri`, `sonraki-asamalar`). Gerekçesi `deep/README.md`'de yazılı: C#
yorumundaki ya da markdown metnindeki düz yollar VS Code'da tıklanabilir değil.

## Bu ağaç ne zaman güncellenir

Üç olay ve üçünde de değişen dosya farklı:

| Olay | Değişen dosya | Ne yazılır |
|---|---|---|
| Kodda yeni bir desen doğdu | `01-koda-gomulu-desenler.md` | Beş alan birden: baskı, yer, SOLID, reddedilen, üç oyun. Baskı yazılmadan ad yazılmaz |
| Bir aşamanın tetikleyici koşulu gerçekleşti | `02-sonraki-asamalar.md` | O satır bir yol haritası satırına dönüşür. **Kod değil, önce bu satır değişir** |
| Yeni bir sahip belge yazıldı | `03-kavram-borc-defteri.md` | İlgili satırın DURUM ve SAHİP DOSYA hücreleri birlikte güncellenir |

██ Dördüncü bir olay yok. ██ Özellikle şu ikisi bu ağacı güncellemez: bir
dosyanın satır sayısının değişmesi (kapı yalnız "dosya var mı" ve "bu satır
numarası mümkün mü" sorularını sorar) ve yeni bir test eklenmesi (test kanıt
seviyeleri satırı zaten kapalı).

## Bu ağacın yakaladığı üç yanlış model

Yazılırken kaynakta doğrulanan, okuyucuyu yanlış yöne götürecek üç şey:

```
"MoveAction/AttackAction bir Command'dır"
   DEĞİL. İkisi de `static class` ve tek bir alanı yok. Command bir eylemi
   NESNEYE bağlar; nesne olmadan kuyruğa alma, geri alma ve tekrar oynatma
   diye bir şey de yoktur. Doğru ad: akış sahibi.
   Ölçü: Assets/Game/Core/MoveAction.cs:42 ve
         Assets/Game/Core/Combat/AttackAction.cs:36 — ikisinde de `static class`.

"Kural sınıfları birer Strategy'dir"
   DEĞİL. Strategy'nin ölçüsü aynı çağıranın İKİ ayrı uygulamayı
   çalıştırabilmesidir; bunun için bir arayüz ya da soyut tip gerekir.
   Bu projenin üretim kodunda `interface` kelimesi HİÇ geçmiyor,
   `abstract`/`virtual`/`override` de geçmiyor. Doğru ad: saf kural sınıfı.

"BoardAdapter bir GoF Adapter'ıdır"
   TAM DEĞİL. GoF Adapter bir tipin arayüzünü BAŞKA bir arayüze çevirir;
   burada çevrilen bir arayüz yok, çevrilen şey iki DÜNYA — piksel/kare/sahne
   ile hücre/tur/kural. Doğru ad: katman sınırı çevirmeni.
   Ölçü: Assets/Game/Unity/BoardAdapter.cs:110 — `MonoBehaviour`'dan türüyor,
         hiçbir arayüz uygulamıyor.
```

Üçünün de uzun hâli `01-koda-gomulu-desenler.md`'de, ilgili desenin altında.

## İlgili

- Kodun kendi gerekçeleri: [../deep/README.md](../deep/README.md)
- Tip başına ayna belgeler: [../deep/kod/README.md](../deep/kod/README.md)
- Ödünç alınan dil özellikleri: [../deep/dil/README.md](../deep/dil/README.md)
- Üst düzey belge haritası: [../README.md](../README.md)
- Bu ağacın kapısı: `Tools/check-curriculum-coverage.py`
- Belge bağlantı kapısı: `Tools/check-doc-links.py`
