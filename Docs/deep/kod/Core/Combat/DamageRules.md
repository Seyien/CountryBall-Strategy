# DamageRules

> **Kaynak:** `Assets/Game/Core/Combat/DamageRules.cs`
> **Ad alanı:** `GridStrategy.Combat` · **Assembly:** `GridStrategy.Combat` (`noEngineReferences: true`)
> **Rol:** Kural (Policy) — kimliği yok, hafızası yok, hesaplar ama uygulamaz

Hasar formülünün tek sahibi. Hiçbir durum tutmaz ve hiçbir duruma dokunmaz:
sayı alır, sayı döndürür. Zırh, direnç, kalkan emilimi ve kritik vuruş çarpanı
geldiğinde değişecek tek yer burasıdır — `Health` o gün hiç değişmeyecek.

| Üye | Karar | Detay |
|---|---|---|
| `DamageRules` (tip) | formülün girdi uzayı sahibininkinden GENİŞ, o yüzden dışarı çıktı | [↓](#damagerules-tip) |
| `ResolveRemaining(int, int)` | alt kelepçe formülün SÖZLEŞMESİ, çağıranın işi değil | [↓](#resolveremainingint-current-int-amount) |

---

## DamageRules (tip)

### HARİTA: girdi uzayı ve Health'in ulaşabildiği bölge

```
ResolveRemaining(current, amount) girdi uzayı
┌─────────────────────────────────────────────────┐
│ current < 0     ◄── Health BURAYA HİÇ GİREMEZ;  │
│ amount  < 0         kural yine de cevap vermek  │
│                     zorunda                     │
│   ┌───────────────────────────────────┐         │
│   │ Health'in üretebildiği bölge:     │         │
│   │ 0 <= current <= max, amount >= 0  │         │
│   └───────────────────────────────────┘         │
└─────────────────────────────────────────────────┘
██ private kalsaydı SINANABİLİR alan iç dikdörtgene inerdi ██
```

### KAPSAM: her doğrulama ayrı bir kural dosyasına ÇIKMAZ

Ayıraç, kuralın girdi uzayının sahibinkinden **geniş** olup olmadığıdır:

```
girdi uzayı sahibinden geniş ► kural dışarı çıkar (bu dosya)
girdi uzayı = kurucunun
parametreleri                ► doğrulama İÇERİDE kalır
```

Karşı örnek aynı ad alanında: `AttackProfile`'ın `damage < 0` ve `range < 1`
doğrulamaları kendi kurucusunda **duruyor**, ayrı bir kural dosyasına
çıkarılmadı — çünkü orada kural bir formül değil bir **kuruluş değişmezi** ve
girdisi zaten kurucunun parametreleri; dışarı çıkarmak sınanabilir tek bir yeni
durum bile açmazdı.

### İŞ BÖLÜMÜ: hesaplayan ile YAZAN

```
DamageRules ► yeni değeri HESAPLAR, hiçbir duruma dokunmaz
Health      ► dönen değeri kendi alanına YAZAR
```

Bölüşme kopyayı değil **sınanabilirliği** ayırıyor: bu dosya silinip formül
`Health`'e geri konsaydı davranış birebir aynı kalır ve hiçbir test kırmızıya
dönmez — kaybolan tek şey iç dikdörtgenin **dışındaki** sözleşme testleri
olurdu. `Health`'in yazma sorumluluğu buraya taşınsaydı bu kez kural durum
tutmaya başlar ve "sayı alır, sayı döndürür" cümlesi düşerdi.

### REDDEDILEN

Formül `Health.cs` içinde `private` kalsaydı:

```csharp
private int ResolveRemaining(int amount)
{
    return Math.Max(0, current - amount);
}
```

**KIRILAN:** formülün sınır durumları yalnızca bir `Health` nesnesi üzerinden
sınanabilir; `Health`'in asla giremediği durumlar hiç sınanamaz.

```
ResolveRemaining(-1, 3) sözleşme testi yazılamaz -> negatif yol kör kalır
derleyici: hiçbir şey der  ·  test: DamageRulesTests ve
DamageRulesAllocationTests'in formül testleri derlenemez
```

**KAZANIRDI:** kural dörtten fazla alanı aynı anda okumak zorunda kalırsa —
`current`, `Max`, zırh, kalkan — o gün parametre listesi uzar ve kural örnek
metoduna döner; geri dönüş ucuz (ABSOLUTE_F): dosyayı sil, metodu geri taşı.

**TEK CUMLE:** Bir formülü sahibinin İÇİNDE tutmak, formülü sahibinin
ulaşabildiği durumlarla sınırlar.

---

## ResolveRemaining(int current, int amount)

Bir vuruştan sonra geriye kalan canı hesaplar. Mevcut canı değiştirmez; yeni
değeri yalnızca döndürür — yazma işi çağırana aittir.

Alt kelepçe (clamp): can sıfırın altına inemez. **Üst kelepçe burada yok**,
çünkü bu metot yalnızca azaltıyor; `Heal` kendi metodunu ve kendi üst
kelepçesini getirdi (`Math.Min(max, current + amount)` — bkz.
[HealingRules.md](HealingRules.md#resolverestoredint-current-int-max-int-amount)).

### HARİTA: canın sıfırın altına inebileceği İKİ yol

```
yol 1  amount NEGATİF gelir     ► yukarıdaki `amount < 0`
       (çağıran hatası)           ► throw

yol 2  amount current'ten BÜYÜK ► 10 - 25 = -15
       (tamamen meşru: aşırı      ► Math.Max(0, ...) ► 0
        hasar)                      ◄── BU SATIR

██ İKİ YOL, İKİ AYRI KAPI ██ İlki bir sözleşme ihlali, ikincisi normal
oynanış. Aynı kapı ikisini birden kapatamaz: birine `throw`, öbürüne
kelepçe gerekiyor.
```

### KAPSAM: her kelepçe bu metodun içinde DEĞİL

Ayıraç, metodun hangi **yöne** hareket ettiğidir: bu metot yalnızca azaltıyor,
o yüzden yalnızca **alt** kelepçeyi taşıyor.

Karşı örnek aynı ad alanındaki ayna eş: **üst** kelepçe burada yok,
`HealingRules.ResolveRestored`'da yaşıyor — ve bu bir eksiklik değil aynı
kararın öbür yarısı. İki kelepçe tek metotta birleşseydi iki yönün
**doğrulaması** da birleşir ve en gevşek olana inerdi; gerekçenin tamamı
[HealingRules'un reddedilen alternatifinde](HealingRules.md#healingrules-tip).

### İŞ BÖLÜMÜ: guard GİRDİYİ, kelepçe SONUCU korur

```
`amount < 0` throw ► girdiyi eler; çağırana HATA der
`Math.Max(0, ...)` ► sonucu kelepçeler; çağırana geçerli bir CEVAP verir
```

Biri diğerinin yerini tutmaz: guard silinirse `ResolveRemaining(10, -5)`
sessizce 15 döndürür ve hasar cana dönüşür; kelepçe silinirse aşırı hasar canı
eksiye indirir ve düzeltmeyi her çağıran kendi eliyle tekrarlamak zorunda
kalır.

### YUKARIDAKİ `throw`LAR BU SATIRIN YERİNE GEÇMEZ

İki `ArgumentOutOfRangeException` okuyucuya "her şey doğrulanmış" hissi verir,
ama ikisi de yalnızca **girdiyi** sınar. `current` ve `amount` ikisi de negatif
olmasa bile **fark** negatif olabilir — bu satırın kapattığı yol tam olarak
orası ve hiçbir guard onu göremez.

### REDDEDILEN

Kelepçe çağırana bırakılsaydı:

```csharp
return current - amount;
// ...ve Health.TakeDamage içinde, atamadan SONRA:
current = DamageRules.ResolveRemaining(current, amount);
if (current < 0) { current = 0; }
```

**KIRILAN:** alt kelepçe formülün **sözleşmesi** olmaktan çıkar, çağıranın işi
olur.

```
Health dışından çağıran herkes düzeltmeyi kendi eliyle tekrarlar
bugün testler, yarın baraka -> biri unutur, can eksiye iner
derleyici: hiçbir şey der  ·  test:
ResolveRemaining_NeverReturnsNegative süpürmesi kırmızıya döner
```

**KAZANIRDI:** aşırı hasarın **miktarı** bir yerde okunmak zorunda kalırsa —
baraka yıkımı "canı ne kadar aştı" değerini yıkım şiddeti olarak kullanacaksa,
ham fark kelepçeyle silinmemelidir.

**TEK CUMLE:** Bir değişmez onu üreten yerde tutulur; çağırana bırakılan
değişmez, çağıran sayısı kadar kopyalanır.
