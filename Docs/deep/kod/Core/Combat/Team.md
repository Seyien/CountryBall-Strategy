# Team

> **Kaynak:** `Assets/Game/Core/Combat/Team.cs`
> **Ad alanı:** `GridStrategy.Combat` · **Assembly:** `GridStrategy.Combat` (`noEngineReferences: true`)
> **Rol:** Tanım (Profile) — kimliği yok, hafızası yok, karar vermez **adlandırır**

Bir birimin ya da yapının hangi tarafta olduğu.

Bu tip **tek başına hiçbir kural taşımaz.** "Aynı takıma saldırılmaz" bir hedefleme
kuralıdır ve `TargetingRules`'a aittir; burada yazılsaydı taraf bilgisi ile taraf
kuralı aynı yerde yaşardı ve dost ateşi gibi bir mod eklemek bu enum'u değiştirmeyi
gerektirirdi.

| Üye | Karar | Detay |
|---|---|---|
| `None` | sıfırıncı değer bilerek tarafsız | [↓](#none) |
| `Player` | oyuncunun birimleri | [↓](#player) |
| `Enemy` | rakip birimler | [↓](#enemy) |

**İlgili anlatılar:** [06-sonuç enumları](../../../konular/06-sonuc-enumlari.md) ·
[04-karar sırası](../../../konular/04-karar-sirasi.md)

---

## None

**SIFIRINCI DEĞER, ATANMAYI UNUTULANIN ADIDIR.**

Sıfır BİLEREK tarafsız. `default(Team)` "oyuncu" olsaydı, takımı atanmayı
unutulmuş her birim sessizce oyuncunun tarafında doğardı ve bu hata ancak oyunda,
yanlış birime saldırılamadığında görünürdü.

### HARİTA: `default(Team)` hangi satıra düşer

C#'ta bir enum'un varsayılanı "ilk YAZILAN üye" değildir; sayısal değeri SIFIR olan
üyedir. İkisi burada çakışıyor çünkü kimseye numara vermedik ve derleyici yukarıdan
aşağı 0, 1, 2 diye sayıyor. Yani koruma sıralamadan doğuyor ama sıralamanın kendisi
değil.

```
  SEÇİLEN                        REDDEDILEN
  ad        sayı                 ad        sayı
  ──────────────────────         ──────────────────────
  None       0  ◄── >> default(Team) BURAYA DÜŞER <<
  Player     1                   Player     0  ◄── >> default <<
  Enemy      2                   Enemy      1
                                 (tarafsız değer YOK)
```

Sağdaki tabloda `new Combatant[10]` on tane OYUNCU birimi doğurur ve bunu hiçbir
satır yazmamıştır: dizinin belleği sıfırla doldurulur, sıfır da o tabloda "Player"
diye okunur. Soldaki tabloda aynı dizi on tane TARAFSIZ birim doğurur — yanlış ama
gürültülü bir yanlış.

### KAPSAM: kural SIFIRINCI konuma özeldir, sıraya değil

`Player` ile `Enemy` arasındaki sıra tamamen SERBEST: ikisini bugün yer değiştirsek
tek bir davranış değişmez, çünkü hiçbir alan sessizce 1'e ya da 2'ye düşemez —
düşülen tek değer sıfırdır.

Karşı örnek aynı dosyanın iki satır aşağısı: `Player` ve `Enemy` üyelerinin üstünde
böyle bir gerekçe bloğu YOKTUR ve olmamalıdır; onların yalnızca birer XML özeti
var. Yeni bir taraf eklenirken (`Ally`, `Neutral`, `Rebel`) sorulacak tek soru
şudur: **bu değeri sıfıra mı koyuyorum?** Hayırsa bu karar o değeri hiç
ilgilendirmez.

### İŞ BÖLÜMÜ: iki mekanizma örtüşmez, bölüşür

"Takımı yazılmamış bir şey sessizce taraf seçmesin" iki ayrı yoldan korunuyor ve
ikisi FARKLI olguyu kapatıyor:

```
  ALAN HİÇ ATANMADI      ► sıfırıncı değer (bu karar)
    dizi elemanı, struct alanı, `default(Team)`, seri hâlden dönüş
  ARGÜMAN HİÇ YAZILMADI  ► Combatant kurucusundaki `= Team.None`
    `new Combatant(h, l, p)` — dördüncü argüman hiç geçilmemiş
```

Sıfırıncı değer silinirse `new Team[8]` sekiz oyuncu üretir; kurucu varsayılanı
bunu yakalayamaz, çünkü orada bir kurucu ÇAĞRISI yok. Kurucudaki varsayılan
silinirse takım kurucunun DÖRDÜNCÜ zorunlu sorusu olur ve gerekçesi
[Combatant.md](Combatant.md#combatant)'de yazılı; sıfırıncı değer bunu yakalayamaz,
çünkü orada atanmamış bir alan yok, yazılmamış bir argüman var. İkisi birbirinin
yedeği değil: biri DEPOLAMAYI, diğeri ÇAĞRI YERİNİ kapatıyor.

### ADIN KENDİSİ HİÇBİR ŞEY KORUMAZ

Okuyucu korumayı "None" sözcüğüne yazabilir; koruma sözcükten değil o sözcüğün
taşıdığı SAYIdan geliyor:

```
  None = 3, Player = 0    ✗ ad aynı, koruma yok: default = Player
  None (numarasız, ilk)   ✓ koruma sıfırdan geliyor
```

Aynı sebeple `None = 0` diye açıkça yazmak da bir şey EKLEMEZ; sıra zaten onu
veriyor. Karar bu yüzden dosyanın başında değil, tam bu üyenin üstünde duruyor:
korunan şey üyenin KONUMU.

### GARANTİ NEREDE BİTER

C#'ta enum KAPALI bir küme değildir: `(Team)7` derlenir, çalışır ve hiçbir switch
dalına uymaz. Sıfırıncı değer yalnız "hiç yazılmamış"ı kapatır, "yanlış yazılmış"ı
değil. Kayıttan gelen bir sayı ya da bir cast bu duvarı geçer; orada tutan şey bu
dosya değil, okuyan tarafın kendi denetimidir (bkz. `TargetingRules`'ın `Team.None`
dalları).

### REDDEDILEN

```csharp
Player,
Enemy
```

**KIRILAN:** `default(Team)` "oyuncu" olur; takımı atanmayı unutulan her şey
sessizce oyuncunun tarafında doğar.

```
tarafsız hiçbir şey ifade edilemez -> duvar, kaynak düğümü, tuzak
bir tarafa yazılır, TargetingRules'a "şu tip hariç" istisnası girer
derleyici: hiçbir şey der  ·  test: hata ancak oyunda görülür
```

**KAZANIRDI:** oyunda tarafsız hiçbir şey olmayacaksa — o gün üçüncü değer ölü kod
olur ve her switch'te anlamsız bir dal açar.

**TEK CUMLE:** Sıfırıncı değer, atanmayı unutulanın adıdır; en zararsız anlam ona
verilir — ve zararsız olan, hiçbir tarafta olmamaktır.

### Bu değeri KULLANAN kararlar

- `Combatant` kurucusundaki `Team team = Team.None` varsayılanı bu kararı
  **kullanıyor**, ikinci kez vermiyor ([Combatant.md](Combatant.md#combatant)).
- `Structure` kurucusunda `Team.None` bilerek geçerli sayılıyor: tarafsız
  yıkılabilir duvar, kapı ya da engel gerçek bir yapıdır
  ([Structure.md](Structure.md#team)).

---

## Player

Oyuncunun birimleri. Üstünde bir gerekçe bloğu yok ve olmamalı: kural sıfırıncı
KONUMA bağlı, `Player` sıfırıncı değil ([↑ None](#none)).

---

## Enemy

Rakip birimler. İki taraflı bir oyunda tek düşman takımı yeter.
