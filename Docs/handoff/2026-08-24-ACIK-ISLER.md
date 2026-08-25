# Açık işler — 2026-08-24 · öncelik sırasıyla

> **Harita:** [`2026-08-24-DEVIR.md`](2026-08-24-DEVIR.md) ·
> **Makine-okunur:** [`2026-08-24-STATE.json`](2026-08-24-STATE.json)
> Her madde bir *tam sonraki adım* taşır. Soru taşımaz.

---

## Öncelik haritası

```
P0  kirli ağaç, kurtarma yok     61 Docs dosyası commit edilmemiş · dokuz kapı yeşil
P1  görünüm sorusu               operatörün yazı tipi — kutu ve ok karakterleri
P2  ogrenme/02 arıza alanı       6 aşamanın 5'i "hangi arızayı kapatıyor" taşımıyor
P3  satır atıfları               .md→.md tazelendi (17 çapa) · kapı hâlâ YOK
P4  kural <-> kapı ayrışması     kural 3 blok biçimi tanımlıyor, kapı 1'ini biliyor
P5  tarihsel işli örnekler       K50'nin 3 ÖNCE bloğu artık canlı dosyada yok
P6  işaret dönüşümü artıkları    kapı yeni lehçeye kör · 21 belirsiz koşu · 4 iç içe çift
P7  iki belge boşluğu            UnityEvent (0/0) · Strategy deseni adıyla hiç geçmiyor
```

---

## P0 — kirli ağaç, hâlâ kurtarma noktası yok

**Durum:** `LANE-BLOK-C` bitti ve doğrulandı. Dokuz kapı yeşil. Ama 61 `Docs/`
dosyası hâlâ commit edilmemiş — üç dalganın (BLOK-A, BLOK-B, BLOK-C) tamamı ile
bu oturumun düzenlemeleri tek bir kirli ağaçta duruyor.

**Doğrulanan ölçüler** (lane'in raporuna güvenilmedi, bağımsız ölçüldü):

```
dokuz kapı              exit=0 · dokuzu da
Assets/*.cs ve Tools/   0 değişiklik
Docs/*.md CR baytı      0            tr -cd '\r' ile, grep -c $'\r' ile DEĞİL
Battle.md kalan ██      0            operatörün örnek verdiği dosya
Docs/ kalan ██          362          lane 355 dedi; fark 7 = bu oturumun kendi
                                     dosyaları (DEVIR 1 · AÇIK-İŞLER 2 · STATE 4)
```

Hizalama lane tarafından ölçüldü: 870 satır değişti, 870'inin uzunluğu aynı
kaldı, kutu karakteri sütun konumu değişen satır 0.

**Lane bir ölçümü çürüttü:** çit içi figür payı **%5**, benim söylediğim %15
değil. 1.691 koşunun yalnız 22'si kesin çizim. Önceki 186 sayısı büyük olasılıkla
yalnız `Docs/deep/` üzerinden ve daha geniş bir ölçütle sayılmış.

**Tam sonraki adım:** Kapsam korumasıyla commit et.

```
git add Docs
bash "<scratchpad>/lane-main/stage-check2.sh" "C15" <sayı> '^Docs/'
```

`<sayı>` = `git status --porcelain -- Docs/ | wc -l` çıktısı.

---

## P1 — operatörün yazı tipi: hangi karakterler çözülmüyor

**Durum:** `█` (U+2588) operatörün görüntüleyicisinde çözülmüyor. Boşluk gibi
görünüyor ve *"burayı ben mi dolduracağım"* diye okunuyor. Bu ölçüldü ve
operatör bildirdi.

Düz yazıdaki 784 çift `***X***` biçimine çevrildi. Çit içindekiler
`LANE-BLOK-C` tarafından `>> X <<` biçimine çevriliyor.

**Cevaplanmamış soru:** Aynı sorun kutu çizgilerinde ve oklarda da var mı?

```
kutu çizgileri   ╔ ║ ╚ ═ ╗ ╝ ╠ ╣        79 kutu
oklar ve simge   → ▶ ◀ ▼ ⌨ ─ │ ✓ ✗      yüzlerce, ara durak/dönüş işaretçilerinde
```

**Tam sonraki adım:** Operatöre şu satırı geri yazdır ve hangi karakterlerin
bozuk çıktığını gör:

```
█ ╔ ║ ╚ ═ → ▶ ◀ ▼ ⌨ ─ │ ✓ ✗ ██
```

Cevap gelene kadar kutu ve ok dönüşümü **başlatılmaz**. Sebebi ölçülü: bu
oturumda ana ajan iki kez doğrulamadan varsayım verdi ve iki lane'i yanlış
yönlendirdi. Üçüncüsü yapılmaz.

Cevap "kutular da bozuk" ise: aynı hizalama kısıtı geçerlidir — `╔` bir
karakter, karşılığı da bir karakter olmalı (`+`, `-`, `|` gibi saf ASCII).

---

## P2 — `ogrenme/02`'nin kalan beş aşaması arıza adı taşımıyor

**Durum:** `Every Structure Closes a Named Failure` kuralı (2026-08-24) her
aşamanın kapattığı arızayı, arızanın bugünkü sayımını ve kararmetreyi istiyor.
Aşama 3 bu bloğu taşıyor. Kalan beşi taşımıyor.

```
Aşama 1  ScriptableObject              YOK
Aşama 2  Nesne havuzu (object pool)    YOK
Aşama 3  Olay veri yolu (event bus)    VAR   ← işli örnek
Aşama 4  Singleton — ve reddedilişi    YOK
Aşama 5  ECS / DOTS                    YOK
Aşama 6  Profil çıkarma                YOK
```

**Tam sonraki adım:** Beş aşamayı ayrık worker'lara böl. Her worker kendi
aşamasının kapattığı arızaları **ölçer**, sayar, ve kararmetreyi adlandırır.
Ölçemediği bir arızayı yazmaz — o durumda `An Absence Owes a Feature` devreye
girer. Aşama 3 işli örnektir; biçimi oradan alınır.

Sonra kapıyı yaz: `Tools/check-curriculum-coverage.py` zaten aşama başlıklarını
dolaşıyor. Her `## Aşama` başlığı `A ·` alanından önce
`KAPATTIĞI ÖLÇÜLMÜŞ ARIZA` bloğu taşımalı, her arıza satırı bir rakam taşımalı,
blokta tam bir kararmetre cümlesi olmalı.

**Kapanan (aynı gün):** `dil/06`'nın `event`in TEK işi bölümündeki mekanizma
eksiği ve dönüş değeri tuzağındaki belirsiz özne. `dil/06` ↔ `ogrenme/02`
çifti `▶ ARA DURAK` / `◀ DÖNÜŞ` ile işaretlendi ve sabotajla doğrulandı:
işaretsiz hâlde dönüşü silmek kapıyı yeşil bırakıyordu, işaretli hâlde
kızarıyor.

---

## P3 — satır atıfları: `.cs` korunuyor, `.md` hâlâ kapısız

```
.md -> .cs   KORUNUYOR    check-doc-code-refs.py Katman 3, alıntı eşlemesi
.md -> .md   KAPISIZ      ama 2026-08-24'te elle tazelendi ve şu an TEMİZ
```

**Tazelenen (hepsi kaynağa karşı ölçüldü, aritmetik kullanılmadı):**

```
00-okuma-sirasi.md   17 satır sayısı atıfı      hepsi bayattı, hepsi düzeltildi
00-okuma-sirasi.md   "14 adım" -> "15 adım"     dosyada ADIM 15 var
00-okuma-sirasi.md   "12.226"  -> "28.806"      üç ağacın gerçek toplamı
ogrenme/README.md    iki yerde "14 adım"        aynı düzeltme
```

**Tarayıcının kendisinde iki hata çıktı, ikisini de öz-sınama yakaladı:**

```
① text.split(satir-sonu)  sondaki satir sonundan sonra BOS eleman uretiyor -> +1 kayma
   düzeltme: splitlines() . doğrulama: wc -l ile birebir eşleşti
② tolerans abs(fark)<=1  bir satırlık kaymayı YEŞİL geçiriyordu
   ve ①'i de gizliyordu . düzeltme: tolerans YOK, tam eşitlik
```

Bilinen-kötü girdi (670 iddia / 671 gerçek) artık KAYMIS diyor, bilinen-iyi
girdi (671/671) TAMAM diyor. Düzeltmeden önce ikisi de TAMAM diyordu.

**Tam sonraki adım:** Kapıyı yaz — `Tools/check-satir-atiflari.py`. Tarayıcı
mantığı hazır ve sınanmış; `/tmp/olc3.py` biçiminde duruyor ama kalıcı değil.
Kapıya taşınırken iki düzeltme de taşınmalı, ve öz-sınama gömülmeli.

---

## P4 — kural ile kapı ayrışıyor

**Durum:** `An Absence Owes a Feature, Not a Lesson` kuralı **üç** blok biçimi
tanımlıyor:

```
◇ YOKLUK SENEDİ    beş alan dolu
DÜŞÜLDÜ            meşru bir geleceği yok — TAM ve GEÇERLİ bir hüküm
DEVREDİLDİ         borcu ogrenme/02 zaten ödemiş
```

`Tools/check-absence-debt.py` yalnız beş alanı ve `YOK-MUAF` / `YOK-HÜKÜM`
işaretçilerini biliyor. `DÜŞÜLDÜ` ve `DEVREDİLDİ` şu an muafiyet işaretçisine
binerek taşınıyor, ve kapı onların `GEREKÇE:` taşıyıp taşımadığını göremiyor.

**Tam sonraki adım:** Kapıya dördüncü madde ekle — `DÜŞÜLDÜ` bloğu `GEREKÇE:`
taşımalı ve numaralı alan taşımamalı; `DEVREDİLDİ` bloğu çapa taşıyan tek bir
bağlantı taşımalı. Kuralın kendi metni bu maddeyi zaten tarif ediyor.

---

## P5 — K50'nin üç işli örneği tarihsel

**Durum:** `One Sentence, One Job` kuralı üç ÖNCE bloğunu
`Docs/ogrenme/08-unity-altyapisi.md` dosyasından alıntılıyor. O satırlar aynı
oturumda düzeltildi; canlı dosyada artık yoklar.

Desen ve gerekçe aynen geçerli. Ama bir kural "şu an şurada duruyor" diyorsa ve
durmuyorsa, ya tarihsel diye işaretlenir ya değiştirilir.

**Tam sonraki adım:** `unity-csharp-quality-flow.archive` içindeki
`One Sentence, One Job` bölümünde o üç ÖNCE bloğunun yanına
`(2026-08-24 öncesi hâli; aynı gün düzeltildi)` notu düş. Alıntıları değiştirme
— kusurun şeklini gösteren tek kanıt onlar.

---

## P6 — işaret dönüşümünün üç artığı

**① Kapı yeni lehçeye kör.** `Tools/check-absence-debt.py` içindeki
`VURGU_ALANI` deseni yalnız `**` ve `██` belirteçlerini tanır. `>>` / `<<`
lehçesi ona **görünmez**. Bugün zarar yok — çevrilen anmaların hepsi çit
içindeydi ve zaten kapsam dışıydı, kapsam içi hüküm sayısı 1'de sabit kaldı.
Ama çit **dışında** `>>` kullanılırsa kapı sessizce körleşir.

**Tam sonraki adım:** `VURGU_ALANI` desenine üçüncü alternatifi ekle
(`>>[\s\S]{1,400}?<<`), sonra öz-sına: çit dışına `>> HENÜZ YOK <<` biçiminde
bir anma enjekte et, kapsam içi sayı 1 → 2 olmalı. Olmuyorsa desen tutmamıştır.

**② 21 koşu rolü belirsiz bırakıldı** — bilerek, ve doğru olan buydu.

```
00-okuma-sirasi.md:773, :776   ██→██ ok süsü, 6 koşu — depoda tek seferlik bir
                               biçim; ──► ile aynı işi yapıyor
05-deger-referans:627          satırda tek sayıda (3) koşu — hangi ikisi eş,
07-oop-dortlusu:745            çıkarılamadı, 6 koşu
02-koleksiyonlar:152 · 07-tiklamadan:697 · 07-oop:746 · 08-motor:1014 ·
06-ilkeler:854                 tek yanlı işaretler, 5 koşu
HANDOFF.md:127, :130           kapanışsız not işaretleri, 2 koşu
2026-08-24-DEVIR.md:144        grep komutunun kendi metni, 2 koşu — dokunulmamalı
```

**Tam sonraki adım:** Bunlar dönüşüm artığı değil, **karar bekleyen** satırlar.
Her birine tek tek bakılır ve rolü metne bakarak verilir. Toplu işlem YAPILMAZ —
belirsizlik zaten toplu işlemin çözemediği şeydi.

**③ 4 düz yazı çifti iç içe.** `01-olay-zinciri.md:45-49` dış vurgunun içinde
ayrı bir `██ ◀ DÖNÜŞ ██` çifti taşıyor; `04-karar-sirasi.md:28-33` ve
`08-motor-cagri-dongusu.md:27-31` aynı desende. İki bağımsız süzgeç aynı dördü
reddetti. Bunlar elle açılır.

---

## P7 — iki gerçek belge boşluğu (müfredat ölçümünden)

```
UnityEvent        kodda 0 · BELGEDE DE 0 — ağaçta hiç geçmiyor
Strategy deseni   ADIYLA hiç geçmiyor; 61 "eşleşme" GridStrategy AD ALANI'ydı
```

İkisi de ucuz ve ikisi de ölçülmüş eksik. `UnityEvent` için doğru ev
`ogrenme/04-yok-olan-mekanizmalar-unity.md` (motorun sunduğu ama alınmayan
mekanizmalar). Strategy için `ogrenme/01-koda-gomulu-desenler.md` — orada saf
kural sınıfları zaten anlatılıyor ama desen adıyla anılmıyor.

**Tam sonraki adım:** İkisini de yaz. `UnityEvent` bloğu `04`'ün altı alanını
taşımalı; Strategy `01`'in beş alanını. Uydurma yok — `Strategy` için bu
projede gerçek karşılık `MoveProfile`/`AttackProfile` + kural sınıfı çifti mi,
yoksa "karşılığı YOK + yokluk senedi" mi, önce ölçülür.

---

## Fail-closed — bu zincirde kesinlikle yapılmayacaklar

```
HARD   git commit / push operatör onayı olmadan
HARD   toplu silme ya da regex tabanlı düzenleme betiği
       (bir gün önce 209 satırlık kayıp tam bundan doğdu)
HARD   kod davranışı değişikliği — kod sorusu bulunursa RAPORLANIR
HARD   git checkout -- <dosya>  (birden fazla lane aynı dosyaya dokundu)
HARD   Docs/deep/kod/Core/Combat/Health.cs ve Health.md şablon — örnek olarak okunur

SOFT   yeni belge yazmak (additive, geri alınabilir)
SOFT   yorum-only .cs düzenlemesi (git diff'te her satır // ile başlamalı)
SOFT   skill katmanına saf ekleme (silinen sütun 0 olmak kaydıyla)
```

---

## Kod soruları — açık, hiçbirine dokunulmadı

```
CANLI   yapı yerleştirme tahta içi + boş hücrede ArgumentException atıyor
CANLI   enkaz görseli hiç yok edilmiyor, her temizlikte LogError
        BoardAdapterTests YOK — ikisinin de yakalanmamasının tek sebebi

BattleActions.Revive üretimde çağıransız
UnitLifecycle.OnHealthDepleted yayını remainingSeconds'tan ÖNCE  (K47 ihlali)
UnitLifecycle.Tick aynı şekil, ceset penceresi için                (K47 ihlali)
Combatant.TryRevive olay health.Heal'den ÖNCE ateşliyor            (K47 ihlali)
Camera.main yerleştirme kipinde kare başına iki kez
"paylaşılan tanım" iddiası üretimde karşılıksız (MoveProfile, AttackProfile)
CQS ihlali dört yer + Combatant.TakeDamage void / Structure.TakeDamage bool
PointerGesture.Reset gerçek bir Unity mesaj adı taşıyor
SampleScene 7 gün eski · 13 alandan 4'ü yazılı · kalan 9 C# başlatıcısıyla doğuyor
        canlı arıza TEK: placementGhost (referans, başlatıcısı yok) -> LogError
        sahnede atanabilecek SpriteRenderer SIFIR: onarım nesne YARATMA işi
```
