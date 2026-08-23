# HealingRules

> **Kaynak:** `Assets/Game/Core/Combat/HealingRules.cs`
> **Ad alanı:** `GridStrategy.Combat` · **Assembly:** `GridStrategy.Combat` (`noEngineReferences: true`)
> **Rol:** Kural (Policy) — kimliği yok, hafızası yok, hesaplar ama yazmaz

İyileştirme formülünün tek sahibi ve `DamageRules`'un aynadaki eşi. O **alt**
kelepçeyi taşır (can sıfırın altına inemez), bu **üst** kelepçeyi (can
maksimumu aşamaz).

Neden aynı sınıfa konmadı: iki kuralın değişme sebepleri farklı. Zırh ve direnç
geldiğinde `DamageRules` değişir; iyileştirme verimi, "ölüyken iyileştirilemez"
gibi kurallar geldiğinde burası değişir. Aynı dosyada olsalardı her iki sebep
de tek dosyayı oynatırdı.

| Üye | Karar | Detay |
|---|---|---|
| `HealingRules` (tip) | ayrı sınıf — birleşen doğrulama en gevşek olandır | [↓](#healingrules-tip) |
| `ResolveRestored(int, int, int)` | hesaplar, yazmaz; ÜST kelepçe burada | [↓](#resolverestoredint-current-int-max-int-amount) |

---

## HealingRules (tip)

### HARİTA: iki yön, iki kelepçe, iki doğrulama

```
                      DamageRules           HealingRules
                      ResolveRemaining      ResolveRestored
─────────────────  ────────────────────  ────────────────────
yön                  azaltır               artırır
kelepçe              ALT: Math.Max(0, ..)  ÜST: Math.Min(max, ..)
`amount` doğrulaması amount < 0 ► throw    amount < 0 ► throw
`max` görür mü       HAYIR                 EVET
değişme sebebi       zırh, direnç,         iyileştirme verimi,
                     kalkan, kritik        "ölüyken iyileştirilemez"

REDDEDILEN — tek metot, işaretli delta
  ResolveHealth(current, max, delta)
    delta < 0 ► hasar       ┐
    delta > 0 ► iyileştirme ├─ ██ `amount < 0` DOĞRULAMASI
    Clamp(0, max)           ┘    YAZILAMAZ ██ çünkü negatif delta
                                 artık GEÇERLİ bir çağrı
  ◄── AYRIŞMA NOKTASI: iki yön birleşince ikisinin doğrulaması da
      birleşir; TakeDamage(-3) bir çağıran hatası olmaktan çıkıp
      geçerli bir iyileştirmeye döner
```

### KAPSAM: bu ad alanında iki kural bir dosyaya KONABİLİR

Ayıraç, birleşmenin **doğrulamaları** da birleştirip birleştirmediğidir.

Karşı örnek aynı ad alanında: `TargetingRules` tek dosyada hem `CanBeAttacked`
hem `CanBeRevived` barındırıyor ve orada ayırmak yanlış olurdu — o ikisi aynı
soruyu (uygunluk) soruyor, girdileri aynı enum ve biri diğerinin doğrulamasını
gevşetmiyor. Burada ise birleşme, iki yönün doğrulamasını birbirine yediriyor.

### İŞ BÖLÜMÜ: yön doğrulaması ile sınır kelepçesi

```
`amount < 0` throw   ► YÖNÜ korur: iyileştirme hasara dönemez
`Math.Min(max, ...)` ► SINIRI korur: can maksimumu aşamaz
```

İkisi ayrı kırılır: doğrulama silinirse `ResolveRestored(10, 20, -5)` sessizce
5 döndürür ve iyileştirme hasar uygular; kelepçe silinirse yön korunur ama can
maksimumun üstüne çıkar. Aynı bölüşmenin aynadaki eşi `DamageRules`'ta: orada
alt kelepçe ile aynı `amount < 0` doğrulaması aynı işi öbür yön için yapıyor.

### REDDEDILEN

Ayrı sınıf değil, `DamageRules.cs` içinde tek metot; delta işaretiyle iki yön:

```csharp
public static int ResolveHealth(int current, int max, int delta)
{
    // delta < 0 => hasar, delta > 0 => iyileştirme
    return Math.Clamp(current + delta, 0, max);
}
```

**KIRILAN:** hasar "negatif iyileştirme" olunca `amount < 0` doğrulaması
imkânsızlaşır.

```
TakeDamage(-3) artık çağıran hatası değil geçerli bir iyileştirmedir
işaret hatası sessizce can BASAR; kimse bunun için bug açmaz
derleyici: hiçbir şey der  ·  test:
HealthTests.TakeDamage_WhenAmountIsNegative_Throws silinmek zorunda kalır
```

**KAZANIRDI:** zırh, direnç ve "ölüyken iyileştirilemez" gibi kurallar hiç
gelmeyecekse ve iki yönün de tek kuralı "0..max arası kelepçe" kalacaksa — o
durumda iki sınıf, tek formülün iki kat bakımıdır.

**TEK CUMLE:** İki kuralı tek metotta birleştirmek ikisinin AYRI
doğrulamalarını da birleştirir; birleşen doğrulama en gevşek olanıdır.

---

## ResolveRestored(int current, int max, int amount)

İyileştirmeden sonraki canı hesaplar. Mevcut canı **değiştirmez**; yeni değeri
döndürür — yazma işi çağırana aittir.

Üç ön koşul burada durur (`current < 0`, `max <= 0`, `amount < 0`) çünkü bu
metodun girdi uzayı sahibinin (`Health`) ulaşabildiğinden geniştir; aynı
gerekçenin tam metni [DamageRules.md](DamageRules.md#damagerules-tip)'de.

Üst kelepçe (`Math.Min(max, ...)`): can maksimumu aşamaz. **Alt kelepçe burada
yok**, çünkü bu metot yalnızca artırıyor — alt kelepçenin evi
`DamageRules.ResolveRemaining`.
