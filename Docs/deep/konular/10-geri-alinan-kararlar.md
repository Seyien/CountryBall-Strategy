# Geri alınan kararlar — kaydın sözleşmesi, arşiv ve iki sınır hâli

`09-kararlarin-cevrilmesi.md` çevrilmiş kararların **hikâyesini** anlatıyor. Bu
dosya o hikâyenin **sözleşmesini** yazıyor: bir geri alma bu ağaca hangi
alanlarla girer, eski kod nerede durur, ve bir karar geri alınmadan önce nasıl
hazırlanır.

İkisini ayıran şey konu değil, **soru**. 09'a giden soru *"bu karar neden
çevrildi"*. Buraya gelen soru *"elimde çevrilmiş bir karar var, onu nasıl
kaydederim"*. Birincisi okunur, ikincisi uygulanır.

Bu ayrımın bedeli ölçüldü. 09 bugün on iki maddeye ulaştı ve maddelerin alan
yapısı **madde madde farklı**: bazısında `Eskisi`, `Kırılan şey`, `Yenisi` üçlüsü
var, bazısında `Referanstan alınan ve reddedilen` dördüncü bir başlık olarak
duruyor, bazısında `Ne zaman ESKİSİ kazanırdı` yalnızca bir maddede yazılı
(madde 10). Biçim yazarken doğdu, önce kararlaştırılmadı. Bu dosya o biçimi
sabitliyor.

---

## Bu belge ile 09 arasındaki sınır

| Soru | Yeri |
|---|---|
| Bu karar neden çevrildi, aradaki cümle ne | `09-kararlarin-cevrilmesi.md` |
| Bir geri alma hangi alanlarla kaydedilir | **burası**, bölüm 1 |
| Silinen kodun tamamı nerede duruyor | **burası**, bölüm 2 → `arsiv/` |
| Hangi kararlar bugüne kadar çevrildi | **burası**, bölüm 3 (indeks) |
| Bugün duran ve yarın çevrilecek olan ne | **burası**, bölüm 4 |
| Reddedilen ama hiç yazılmamış bir desen | **burası**, bölüm 5 |
| Bir üyenin tam gerekçesi | `../kod/` — tip başına ayna |

Bir geri alma **iki yere** yazılır ve ikisi farklı işi yapar. Hikâye 09'a girer.
Silinen kodun tamamı `arsiv/` altına girer. Bu dosya ikisini bağlar.

---

## 1 · Bir geri alma nasıl kaydedilir — yedi alan

Yedisi de zorunlu. Bir alan boş kalıyorsa kayıt henüz tamam değildir.

| # | Alan | Ne yazılır | Ret kapısı |
|---|---|---|---|
| 1 | **NE GERİ ALINDI** | Tip ve üye adı | Satır numarası yazılmışsa ret |
| 2 | **ESKİ ŞEKİL** | Kod bloğu ve `arsiv/` bağı | Eski parça hiçbir yerde yoksa ret |
| 3 | **NEYİ KIRDI** | Oyunda ya da testte görünen zarar | "Çirkindi" bir zarar değildir |
| 4 | **HANGİ ÖLÇÜM** | Sayı, sayım, test adı, konsol satırı | "Daha temiz oldu" bir ölçüm değildir |
| 5 | **YENİ ŞEKİL** | Kod bloğu | Yalnız tarif edilmişse ret |
| 6 | **TEK CÜMLE** | Bir cümlelik sonuç | İki iş yapan cümle ret |
| 7 | **TERSİNE ÇEVİRME KOŞULU** | Bu kararı da geri aldıracak yeni ölçüm | "Muhtemelen kalır" bir koşul değildir |

### Dördüncü alan neden bu belgenin çekirdeği

Bir geri almanın öğrettiği şey yeni şekilde değil, **iki şekil arasındaki
cümlededir**. O cümlenin öznesi bir ölçümdür. Ölçüm yazılmazsa geriye bir zevk
beyanı kalır, ve zevk beyanı bir sonraki turda tersine de savunulabilir.

Bu ağaçta ölçüm sayılan şeyler, hepsi 09'da işlemiş örnekleriyle:

| Ölçüm cinsi | Bu repoda geçen örneği |
|---|---|
| Kırmızıya dönen test **adı** | `Downed_HealthDepletedAgain_DoesNotSkipTheWindow` |
| Nesne ya da köşe **sayısı** | `100x50` tahtada 5616 `GameObject`, 161872 köşe |
| Konsola düşen **satır** | Tahta kurulumunun bastığı hücre sayısı |
| Operatörün bildirdiği **davranış** | *"iki taraf için paralel olarak saldırı aşamalarını gerçekleştiremiyorum"* |
| Kod içinde **sayım** | `Assets/Game` altında sıfır `abstract`, sıfır `virtual` |

Ölçüm sayılmayan şeyler: *daha temiz*, *daha okunur*, *daha doğru geldi*, *pattern'e
uydu*. Bunların hiçbiri yarın ölçülüp yanlışlanamaz, dolayısıyla hiçbiri bir
kararı taşıyamaz.

### Yedinci alan neden zorunlu

Bir geri alma, geri alınan kararın da bir zamanlar doğru göründüğünü kanıtlar.
Yeni kararın da bir günü gelecek. O günü tanıyabilmenin tek yolu, koşulu
**bugünden** yazmaktır.

Koşul bir cümle olmalı ve içinde bir eşik geçmeli. *"Ölçüm değişirse"* bir koşul
değildir. *"Tahta kenarı 30'u aşarsa"* bir koşuldur.

### Satır numarası neden yasak

Bu belge kodun yanında yaşıyor ve satır numaraları her turda kayıyor. Atıf tip ve
üye adıyla yapılır.

Ayrım şu: bir üye adı değiştiği gün derleyici onu gösterir, çünkü ad kodda da
geçiyordur. Bir satır numarasının kaydığını hiçbir şey göstermez.

Kuralın makine kapısı `Tools/check-cross-file-refs.py`, ve kapının kapsamı
`Assets/**/*.cs` ile sınırlı. Bu belgedeki yasağı hiçbir kapı denetlemiyor, yani
burada kural **elle** tutuluyor.

---

## 2 · Arşiv — silinmek üzere olan kod nereye gider

`arsiv/` klasörünün tek işi var: **ağaçtan tamamen kalkan** kodu okunur hâlde
tutmak.

09 her maddede kısa ve okunur bir parça taşıyor, ve bu bilerek. Hikâye
okunacaksa parça kısa olmalı. Ama kısaltılmış bir parça, silinen dosyanın
tamamının yerini tutmaz.

Ayrım ölçüldü. `PendingStrikeMode` bugün çalışma ağacında **silinmiş** durumda
(`git status` onu ` D` ile gösteriyor) ve yerine `Assets/Game/Unity/Orders/`
altındaki altı yeni dosya geldi. O silme commit'lendiği gün 174 satırlık dosya
ağaçtan tamamen kalkacak. 09'un madde 2'sinde ondan yalnızca `IBoardMode`
arayüzü ve birkaç satır duruyor.

### Neye arşiv dosyası açılır

| Açılır | Açılmaz |
|---|---|
| Ağaçtan tamamen kalkan bir dosya ya da üye | 09'da tam hâliyle zaten duran parça |
| Denenip geri alınan, hiç commit'lenmemiş bir değişiklik | Yeniden adlandırma, taşıma |
| Yakında silinecek olan ve bugün hâlâ duran kod | Yeni bir üyenin eklenmesi |

### Arşiv dosyasının biçimi

Künye, sonra kod, sonra tek satırlık bağ. Kod **birebir** taşınır; hizası,
yorumları ve yazım hataları düzeltilmez. Amaç okunabilirlik değil, **kanıt**.

Bir arşiv dosyası kararı anlatmaz. Kararın anlatıldığı yer 09, kaydın tutulduğu
yer burası, kodun durduğu yer `arsiv/`.

---

## 3 · Bugüne kadar çevrilmiş kararlar — indeks

Bu tablo hikâyeyi tekrar etmiyor. Her satır 09'daki maddeye ve varsa arşiv
dosyasına gönderiyor.

`Kural 58` sütunu, mentor kuralının **adıyla saydığı** dört geri almayı
işaretliyor. Dördü de bu repoda doğrulandı.

| 09'daki madde | Ne geri alındı | Ölçüm | Eski şeklin yeri | Kural 58 |
|---|---|---|---|---|
| 1 | `BoardAdapter`'ın `isPlacingStructure` ve `ghostIsCarried` alanları → `IBoardMode` | Klavyeli kip ile sürükleme aynı hayaleti yazıyor, kaybeden taraf uyarısız görünmez kalıyordu | [arsiv/yerlestirme-bayraklari.md](arsiv/yerlestirme-bayraklari.md) | ① |
| 2 | `PendingStrikeMode` → `UnitOrderBook` | Operatör bildirimi: iki taraf paralel saldıramıyor | [arsiv/bekleyen-vurus-kipi.md](arsiv/bekleyen-vurus-kipi.md) | ② |
| 2a, 2a-i, 2b, 2c, 2d | Aynı sahiplik kaymasının beş ayrı sonucu | 09'da madde başına yazılı | 09 içinde | |
| 3 | `UnitLifecycle.OnHealthDepleted` kapısının genişletilmesi, **geri alındı** | Üç test kırmızıya döndü, biri adıyla `Downed_HealthDepletedAgain_DoesNotSkipTheWindow` | [arsiv/dusmus-birim-kapisi.md](arsiv/dusmus-birim-kapisi.md) | ③ |
| 4 | `BattleActions.Attack` içindeki `TurnRules.CanAct` kapısı → `AttackProfile.CooldownSeconds` | Kapı kalkınca saldırının tek bedeli kalmadı, fareye ne kadar hızlı basılırsa o kadar hasar | [arsiv/saldiri-sira-kapisi.md](arsiv/saldiri-sira-kapisi.md) | ④ |
| 5 | Ham çarpan → varlığın kendi ölçüsünden türetme | 09'da yazılı | 09 içinde | |
| 6 | Durum şeridinde sıranın sahibi → seçimin tarafı | 09'da yazılı | 09 içinde | |
| 7 | Adı olan iki sabit → tek sahip | 09'da yazılı | 09 içinde | |
| 8 | Tahta dışında hayaleti gizlemek → kırmızı göstermek | 09'da yazılı | 09 içinde | |
| 9 | Kamera çerçevesi kurulumda bir kez → her oran değişiminde | 09'da yazılı | 09 içinde | |
| 10 | Hücre başına `GameObject` → tek `Tilemap` | `100x50` tahtada 5616 nesne, 161872 köşe, 65535 tavanı | 09 içinde | |
| 11 | `UnitGrid.TryGetPosition`'ın tam tarama → ters dizin | Emir başına karede üç tarama | 09 içinde | |
| 12 | Sol tuşun anlamı basma karesi → bırakma karesi | 09'da yazılı | 09 içinde | |

### Arşiv dosyası neden yalnızca dörde açıldı

Karar ölçüyle alındı, tercihle değil. Bir arşiv dosyası, eski kodun **başka
hiçbir yerde durmadığı** durumda kanıt taşıyor. 09'un madde 5'ten 12'ye kadar
olan maddelerinde eski parça zaten tam hâliyle yazılı, ve ikinci bir kopya
açmak aynı metne ikinci bir sahip verirdi.

O ikilik sessizce ayrışır, çünkü hiçbir derleyici iki markdown bloğunu
karşılaştırmıyor. Aynı defekt sınıfının kod tarafındaki adı bölüm 4'te duruyor.

Kural 58'in ret kapısı *"hiçbir yerde eski parçası saklanmamış bir geri alma"*
diyor. 09 o parçaları saklıyor, yani kapı bugün yeşil.

---

## 4 · HENÜZ GERİ ALINMADI — bir savaşçının canını iki yazar yazıyor

Bu bölüm bir geri alma kaydı **değil**, bir geri almanın **ön hazırlığı**.
Alanların altısı bugünden dolduruldu. Beşinci alan (`YENİ ŞEKİL`) boş ve
sahibi belli.

### NE GERİ ALINACAK

`BoardAdapter`'ın `NewCombatant` üyesi ve onu besleyen dört serileştirilmiş
alan: `maxHealth`, `damage`, `attackRange`, `attackCooldownSeconds`.

### ESKİ ŞEKİL

Bugün ağaçta duran hâli. `UnitBlueprint.CreateCombatant` bir `.asset`
dosyasının sayılarından savaşçı kuruyor:

```csharp
public Combatant CreateCombatant(Team team)
{
    // Health ve UnitLifecycle HER ÇAĞRIDA YENİ — burası tanımın örnek
    // durumu taşımamasının uygulandığı tek satırdır. AttackProfile ise
    // alandan geçiyor, kopyalanmıyor: değişmez olduğu için paylaşılması
    // güvenli, ve paylaşıldığı için iddia gerçek.
    return new Combatant(
        new Health(MaxHealth),
        new UnitLifecycle(),
        AttackProfile,
        team);
}
```

`BoardAdapter.NewCombatant` aynı işi Inspector alanlarından yapıyor:

```csharp
/// <summary>
/// Inspector'daki sayılardan bir savaşçı kurar.
/// </summary>
private Combatant NewCombatant(Team team)
{
    // YAŞAM DÖNGÜSÜ PENCERELERİ BİLEREK SERİLEŞTİRİLMEDİ, oysa can ve
    // hasar serileştirildi: "kaç saniye düşük kalır" sorusunun ZATEN bir
    // sahibi var (UnitLifecycle'daki iki sabit) ve sahnedeki bir alan o
    // sabiti sessizce ezerdi. Canın ve hasarın ilk sahibi ise burası.
    // → BoardAdapter.md#newcombatantteam-team
    return new Combatant(
        new Health(maxHealth),
        new UnitLifecycle(),
        new AttackProfile(damage, attackRange, attackCooldownSeconds),
        team);
}
```

Tam metin ve tek çağıranı: [arsiv/inspector-savascisi.md](arsiv/inspector-savascisi.md).

### NEYİ ENGELLİYOR

*"Bir savaşçının canı kaçtır"* sorusunun **iki yazarı** var, ve derleyici bunu
bildirmiyor.

Bugün ayrışmış durumdalar. `BoardAdapter`'ın `attackCooldownSeconds` alanının
varsayılanı `1f`. `Unit_Piyade.asset` dosyasında aynı sayı `0.8`. İki yoldan
doğan iki piyade farklı hızda vuruyor, ve ekranda hangisinin doğru olduğunu
söyleyen hiçbir işaret yok.

`NewCombatant`'ın tek çağıranı `BoardAdapter` içindeki `SpawnUnit`, onun da tek
çağıranı kodda **GEÇİCİ** işaretli iki demo birim:

```csharp
// GEÇİCİ: iki demo birim. İKİSİ de gerekli ve bu bir tercih değil —
// saldırı zincirinin kapandığını göstermek için birbirine
// tıklanabilen İKİ birim şart, ve TargetingRules dost ateşini
// reddettiği için tarafları farklı olmak zorunda.
if (unitPrefab != null)
{
    SpawnUnit("Vanguard", Team.Player, 1, 2);
    SpawnUnit("Raider", Team.Enemy, 1, 3);
}
```

Yani ikinci yazar, oyunun kendisine değil bir gösterime hizmet ediyor.

### HANGİ ÖLÇÜM

Dört sayı iki tipte **ADIYLA** tekrar ediyor: `maxHealth`, `damage`,
`attackRange`, `attackCooldownSeconds`. Birincisi `BoardAdapter`'da
`[SerializeField]`, ikincisi `UnitBlueprintAsset`'te `[SerializeField]`.

`UnitBlueprint` tipinin kendi yorumu bu ikiliği zaten **adıyla** bildiriyor ve
bedelini de yazıyor: ikinci kopya doğduğu gün biri saldırı profilini paylaşır,
öteki paylaşmaz, ve fark hiçbir derleme hatası vermeden ekrana düşer.

Bugünkü ayrışma bir sayı: `1f` ile `0.8`.

### YENİ ŞEKİL

**LANE D tarafından yazılacak.**

Yazıldığında bu bölüm 09'a bir madde olarak taşınır ve buradaki altı alan o
maddenin gövdesi olur.

### TEK CÜMLE

Bir niceliğin iki yazarı varsa sahibi yoktur, ve derleyici bunu hiç bildirmez.

### TERSİNE ÇEVİRME KOŞULU

Bu kararı geri aldıracak ölçüm şudur: sahne kurulumunun, hiçbir `.asset`
dosyası okumadan çalışabilmesi gereken bir yolu doğarsa. Bugün öyle bir yol yok
— on `Unit_*.asset` dosyasının onu da `Assets/Game/Blueprints/` altında duruyor
ve üretim yolu onları okuyor.

---

## 5 · REDDEDİLDİ, GERİ ALINMADI — `Factory`

Bu bölüm bir geri alma **değil**. Bir ret.

Ayrım kuralın kendisinden geliyor: bir geri alma iki şekil arasındaki cümleyi
öğretir, bir ret ise hiç yazılmamış bir şekli. Reddedilen bir desenin eski hâli
yoktur, dolayısıyla arşivi de yoktur.

### Ne reddedildi

Savaşçı üretimini bir `Factory` deseninin arkasına almak.

### Neden hiç yazılmadı

Fabrikanın seçeceği tip yok. Üç ölçüm, üçü de bugün doğrulandı:

| Ölçüm | Bugünkü değeri |
|---|---|
| `Combatant` tipinin bildirimi | `public sealed class Combatant` |
| `Assets/Game` altında `abstract` ve `virtual` sayısı | **sıfır** ve **sıfır** |
| On birim varlığının `m_Script` GUID sayısı | **bir** benzersiz GUID |

Üçüncü ölçüm en açık olanı. `Assets/Game/Blueprints/` altındaki on
`Unit_*.asset` dosyasının onu da aynı `m_Script` GUID'ini taşıyor, yani onu da
`UnitBlueprintAsset` tipinden. Piyade ile keşif uçağı arasındaki fark bir tip
farkı değil, bir **sayı** farkı.

Bir fabrikanın var olma sebebi *"hangi tipi kuracağım"* sorusudur. Bu projede o
sorunun cevabı her seferinde aynı, dolayısıyla soru yok.

### Bugün o işi kim yapıyor

`UnitBlueprint.CreateCombatant`. Fabrika değil, **tanımın kendisi** kuruyor.
Gerekçesi tipin kendi yorumunda yazılı: kurma işi çağırana bırakılsaydı üç
parçayı doğru sırayla birleştirme sözleşmesi her çağıranda yeniden yazılırdı.

Bölüm 4'teki ikilik tam olarak o kopyanın bugünkü tek örneği.

### TERSİNE ÇEVİRME KOŞULU

Bu ret şu ölçümle düşer: `Combatant`'tan türeyen ikinci bir tip doğarsa, ya da
`Unit_*.asset` dosyaları ikiden fazla `m_Script` GUID'i taşımaya başlarsa.

İkisinden biri olmadan fabrika, tek bir `new` çağrısının önüne konmuş bir
klasördür.

---

## Bu belgeye ne eklenir, ne eklenmez

| eklenir | eklenmez |
|---|---|
| Kaydın biçimine dair bir kural değişikliği | Bir geri almanın hikâyesi (yeri 09) |
| Yeni bir arşiv dosyası ve indeks satırı | Bir üyenin gerekçesi (yeri `../kod/`) |
| Henüz çevrilmemiş ama çevrileceği belli bir ikilik | Bir desenin ne olduğunun tarifi |
| Yazılmamış bir desenin reddi ve düşme koşulu | Ekleme, taşıma, yeniden adlandırma |

Satır numarası verilmez. Atıf tip ve üye adıyla yapılır.

---

## İlgili

- Çevrilmiş kararların hikâyesi: [09-kararlarin-cevrilmesi.md](09-kararlarin-cevrilmesi.md)
- Eski kodun kendisi: [arsiv/README.md](arsiv/README.md)
- Yaşam döngüsü kuralları (madde 3'ün konusu): [05-yasam-dongusu.md](05-yasam-dongusu.md)
- Ret sırası ve sıra devri (madde 4'ün konusu): [04-karar-sirasi.md](04-karar-sirasi.md)
- Tahtanın kip makinesi (madde 1 ve 2'nin konusu): [03-tahta-sahipligi.md](03-tahta-sahipligi.md)
- Üye başına gerekçeler: [../kod/Unity/BoardAdapter.md](../kod/Unity/BoardAdapter.md)
- Bu ağacın yönlendirmesi: [../README.md](../README.md)
