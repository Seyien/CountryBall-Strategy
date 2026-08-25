# OOP dörtlüsü — ve sahipsiz kalan dördüncüsü

**Alt başlık:** kapsülleme · kalıtım · soyutlama · **çok biçimlilik** — dördü bir
**aile** olarak, ve bu projede hangisinin nerede durduğu.

***Bu dosya bir mekanizma anlatısı DEĞİL, bir **çerçeve** dosyasıdır.*** Dörtlünün
üç üyesi bu depoda zaten anlatılmış durumda; okuyucu parçaları biliyor ama
**çerçevesini** bilmiyor. Ve dördüncü üye —çok biçimlilik— bu ağaçta hiçbir
dosyada **adıyla** geçmiyordu. İki iş burada: aileyi kurmak, ve eksik üyeyi
kapatmak.

## Bu dosyanın sınırı — altı sahip, altı ayrı soru

***Aşağıdaki satırların hiçbiri burada TEKRAR EDİLMİYOR.*** Mekanizmanın
kendisi orada; burada yalnız o mekanizmanın **ailedeki yeri** var.

| Soru | Sahip | Ne anlatır |
|---|---|---|
| Erişim duvarı nerede biter | [../deep/dil/08-erisim-ve-sozlesme.md](../deep/dil/08-erisim-ve-sozlesme.md) | altı belirteç, `internal`in assembly duvarı, `InternalsVisibleTo` |
| `interface`in arka tarafı ve neden sıfır olduğu | [../deep/dil/08-erisim-ve-sozlesme.md](../deep/dil/08-erisim-ve-sozlesme.md) | gönderim tablosu, "bir çağıran kaç uygulama görmeli" ölçütü |
| `abstract`/`virtual`/`override` ne vaat eder | [../deep/dil/08-erisim-ve-sozlesme.md](../deep/dil/08-erisim-ve-sozlesme.md) | sanal çağrının nesnenin gerçek tipini seçmesi, `static class`in IL'deki hâli |
| Bileşimin bu projedeki gerekçesi | [01-koda-gomulu-desenler.md](01-koda-gomulu-desenler.md) | 5. desen: baskı, `Structure`'ın reddi, Liskov ölçüsü |
| Koleksiyon generic'lerinin vaadi | [../deep/dil/02-koleksiyonlar-ve-salt-okunur.md](../deep/dil/02-koleksiyonlar-ve-salt-okunur.md) | `IReadOnlyList` ≠ değişmez, `KeyValuePair`, `object Current` |
| `Awake`/`Update`'i motorun nasıl bulduğu | [../deep/konular/08-motor-cagri-dongusu.md](../deep/konular/08-motor-cagri-dongusu.md) | ad tabanlı mesaj geri çağrısı, çağrı sırası, Domain Reload |
| ██ **Dördü bir aile olarak, ve çok biçimliliğin üç türü** ██ | **bu dosya** | hangi soru hangi üyeye ait, aşırı yükleme ≠ ezme, sayımlar |

Yani: erişim tarafı `dil/08`'in, desen tarafı `ogrenme/01`'in, motor tarafı
`konular/08`'in. Buradaki tek konu **dörtlünün kendisi**.

---

## ***ÖNCE SAYILAR — hepsi bu depoya karşı sayıldı***

Beş sayım, beş ayrı soru. Yöntem açık yazılıyor ki tekrar edilebilsin.

### ① Kalıtım satırı — toplam **sekiz**, oyun mantığında **sıfır**

```sh
grep -rn "class .* : " Assets/Game/ --include=*.cs
```

```
Assets/Game/Unity/BoardAdapter.cs:111
    public sealed class BoardAdapter : MonoBehaviour, IPlacementBoard
Assets/Game/Unity/PaletteEntryView.cs:39
    public sealed class PaletteEntryView : MonoBehaviour,
Assets/Game/Unity/ProductionDirector.cs:35
    public sealed class ProductionDirector : MonoBehaviour
Assets/Game/Unity/ProductionPanelView.cs:36
    public sealed class ProductionPanelView : MonoBehaviour
Assets/Game/Unity/StructureBlueprintAsset.cs:37
    public sealed class StructureBlueprintAsset : ScriptableObject
Assets/Game/Unity/StructurePaletteView.cs:30
    public sealed class StructurePaletteView : MonoBehaviour
Assets/Game/Unity/UnitBlueprintAsset.cs:45
    public sealed class UnitBlueprintAsset : ScriptableObject
Assets/Game/Unity/UnitView.cs:43
    public sealed class UnitView : MonoBehaviour
```

***BU SAYI 2026-08-25'te İKİDEN SEKİZE ÇIKTI — ve iddianın asıl konusu
değişmedi, güçlendi.*** Eski cümle *"Başka hiçbir tip hiçbir şeyden
türemiyor"* idi ve artık yanlış. Yerine geçen ölçü daha keskin:

```
  >> SEKİZİN SEKİZİ DE AYNI KLASÖRDE <<

  GridStrategy.Unity   ► 8 kalıtım satırı   (9 dosya)
  GridStrategy.Battle  ► 0                  (7 tip)
  GridStrategy.Combat  ► 0                  (23 tip)
  GridStrategy.Core    ► 0                  (7 tip)
```

Sekizi de `sealed`, sekizi de **motor tipinden** türüyor (`MonoBehaviour` ×6,
`ScriptableObject` ×2), ve hiçbiri **proje-yerel** bir tabandan türemiyor —
yani bu depoda iki katlı bir kalıtım ağacı **yok**. Oyun mantığında kalıtım
hâlâ **sıfır**; değişen tek şey motora bağlanan bileşen sayısı.

### ② Aşırı yükleme grubu — **altı grup, on dört metot**

"Aşırı yükleme grubu" demek: aynı tipin içinde, aynı adı taşıyan iki ya da daha
fazla metot. Tek tek açılıp sayıldı:

```
Assets/Game/Core/Combat/TargetingRules.cs:44
        public static bool CanBeAttacked(UnitState state)
Assets/Game/Core/Combat/TargetingRules.cs:65
        public static bool CanBeAttacked(UnitState state, Team attackerTeam, Team targetTeam)
Assets/Game/Core/Combat/TargetingRules.cs:89
        public static bool CanBeAttacked(StructureState state)
Assets/Game/Core/Combat/TargetingRules.cs:105
        public static bool CanBeAttacked(StructureState state, Team attackerTeam, Team targetTeam)
Assets/Game/Core/Combat/TargetingRules.cs:132
        public static bool CanBeRevived(UnitState state)
Assets/Game/Core/Combat/TargetingRules.cs:149
        public static bool CanBeRevived(UnitState state, Team reviverTeam, Team targetTeam)
Assets/Game/Battle/TurnRules.cs:59
        public static bool CanAct(Team unitTeam, Team currentTurn)
Assets/Game/Battle/TurnRules.cs:91
        public static bool CanAct(Team unitTeam, Team currentTurn, int actionsUsedThisTurn)
Assets/Game/Core/Combat/AttackAction.cs:52
        public static AttackOutcome Execute(Combatant attacker, Combatant target, int distance)
Assets/Game/Core/Combat/AttackAction.cs:127
        public static AttackOutcome Execute(Combatant attacker, Structure target, int distance)
Assets/Game/Core/MoveAction.cs:63
        public static MoveOutcome Execute(
Assets/Game/Core/MoveAction.cs:167
        public static MoveOutcome Execute(
Assets/Game/Battle/TurnState.cs:76
        public TurnState()
Assets/Game/Battle/TurnState.cs:94
        public TurnState(IReadOnlyList<Team> turnOrder)
```

```
  grup                            üye   ayıran şey
  ──────────────────────────────────────────────────────────────────
  TargetingRules.CanBeAttacked      4   enum TİPİ (UnitState /
                                        StructureState) ve parametre SAYISI
  TargetingRules.CanBeRevived       2   parametre sayısı
  TurnRules.CanAct                  2   parametre sayısı
  AttackAction.Execute              2   >> hedefin TİPİ <<
  MoveAction.Execute                2   menzilin TİPİ (int / MoveProfile)
  TurnState (kurucu)                2   parametre sayısı
  ──────────────────────────────────────────────────────────────────
  TOPLAM                           14 metot, 6 grup
```

### ③ Ezme (`virtual`/`override`) — **sıfır**, ve iki kez ölçüldü

Kaynak tarafı: **46** üretim dosyasında `abstract`, `virtual` ve `override`
kelimelerinin üçü de **sıfır** kez bildirim olarak geçiyor (2026-08-25; dosya
sayısı 33'ten 46'ya çıkarken üç sıfır da korundu). ***Dördüncü kelime düştü:***
`interface` bugün **bir** kez geçiyor (`IPlacementBoard.cs:39`) — ama bu
bölümün konusu EZME ve arayüz üye devretmez, yalnız imza dayatır; ezmenin
sıfırı bundan etkilenmiyor.
Üstveri tarafı: dört üretim DLL'inde `Virtual` damgası taşıyan **tek bir metot
yok**. İki ölçümün tamamı
[`dil/08`](../deep/dil/08-erisim-ve-sozlesme.md)'in sayılar bölümünde.

### ④ Generic — **kullanılıyor, yazılmıyor**

```sh
grep -rnE "(class|struct|interface)[[:space:]]+[A-Za-z_]+[[:space:]]*<" Assets/Game --include=*.cs   # 0
grep -rn "where T" Assets/Game --include=*.cs                                                        # 0
grep -rn "<T>"     Assets/Game --include=*.cs                                                        # 0
```

***Kendi yazdığımız generic tip: SIFIR. Generic metot: SIFIR. Kısıt (`where`):
SIFIR.*** Buna karşılık hazır generic tipler, yorum satırları çıkarıldıktan
sonra **22 satırda** kullanılıyor ve altı ayrı generic tanımdan geliyor:
`Dictionary<,>`, `List<>`, `IReadOnlyList<>`, `KeyValuePair<,>`, ve iki farklı
aritesiyle `Action<>` / `Action<,>` / `Action<,,>`.

### ⑤ Bunlar da sıfır — ve sayılmasının bir sebebi var

```
  operator aşırı yüklemesi (operator + / == / implicit / explicit)   0
  indeksleyici (this[...])                                           0
  params dizisi                                                      0
  Equals / GetHashCode / ToString ezmesi                             0
```

Dördü de çok biçimliliğin komşu mekanizmaları. Sıfır olmaları, aşağıdaki
anlatının **karşılaştıracak ikinci tarafı olmadığını** söylüyor; bu dosya o
boşluğu uydurma proje örneğiyle doldurmuyor, işaretliyor.

---

## Birinci durak: dörtlü tek figürde — hangi soruya cevap veriyorlar

***Dördü bir liste değil, bir **sıra**.*** Her biri bir öncekinin üstüne
oturuyor; dördüncüsü ilk üçü olmadan **kurulamaz**.

```
  ① KAPSÜLLEME (encapsulation)
       SORU   : "bu üyeyi kim GÖREBİLİR, kim DEĞİŞTİREBİLİR"
       ARACI  : erişim belirteçleri · get-only özellik · readonly alan
       BURADA : yok — sahibi dil/08 ve dil/01
       ÖLÇÜ   : public 156 · private 98 · internal 1

  ② KALITIM (inheritance)
       SORU   : "bu tip şunun BİR TÜRÜ mü"
       ARACI  : taban tip · abstract taban · sealed
       BURADA : >> ÜÇÜNCÜ DURAK << — iki satır, ikisi de motor için
       ÖLÇÜ   : 2 satır, 14 sealed class

  ③ SOYUTLAMA (abstraction)
       SORU   : "hangi ayrıntı GİZLENİR, çağıran neyi bilmek zorunda değil"
       ARACI  : imza · sonuç tipi · erişim duvarı · ve EVET, bazen interface
       BURADA : >> DÖRDÜNCÜ DURAK << — arayüzsüz soyutlamanın üç örneği
       ÖLÇÜ   : interface 0, ama soyutlama her dosyada

  ④ ÇOK BİÇİMLİLİK (polymorphism)
       SORU   : >> "AYNI çağrı, FARKLI davranış" <<
       ARACI  : aşırı yükleme · ezme · generic
       BURADA : >> İKİNCİ DURAK — bu dosyanın çekirdeği, sahipsizdi <<
       ÖLÇÜ   : aşırı yükleme 14 metot · ezme 0 · kendi generic'i 0
```

### Dördüncüsü neden en son gelir

Sıra bir gelenek değil, bir **bağımlılık**:

```
  Çok biçimlilik "AYNI çağrı" der       → önce bir SÖZLEŞME lazım
                                          (ad + imza + dönüş tipi)
       ve sözleşme yazmak               → SOYUTLAMADIR (③)

  Çok biçimlilik "FARKLI davranış" der  → önce birden fazla GÖVDE lazım
       gövdeleri bir tip ailesi taşıyorsa → KALITIM (②)
       gövdeleri ayrı tipler taşıyorsa    → yine bir sözleşme (③)

  Ve her iki durumda da gövdenin İÇİ dışarıdan görünmemeli
                                          → KAPSÜLLEME (①)

  >> AYRIŞMA NOKTASI <<
  Bu yüzden "polimorfizm ekleyelim" diye başlayan bir tasarım yoktur.
  Polimorfizm EKLENMEZ; ilk üçü doğru kurulduğunda ORTAYA ÇIKAR.
  Bu projede ① ve ③ kurulu, ② bilerek boş — ve ④'ün aldığı şekil
  tam olarak bunun sonucu.
```

---

## İkinci durak: ***ÇOK BİÇİMLİLİK — üç ayrı tür var ve karıştırılırlar***

Tek bir kelime, üç ayrı mekanizma. Üçünün ortak cümlesi "aynı çağrı, farklı
davranış"; ayrıldıkları yer **seçimi kimin ve NE ZAMAN yaptığı**.

```
                       SEÇİMİ KİM YAPAR      NE ZAMAN       BU PROJEDE
  ─────────────────────────────────────────────────────────────────────
  ① aşırı yükleme      DERLEYİCİ             derleme        >> 14 metot <<
     (overloading)     argümanların STATİK   zamanında      6 grup
                       tipine bakarak
  ─────────────────────────────────────────────────────────────────────
  ② ezme               ÇALIŞMA ZAMANI        çağrı anında   >> SIFIR <<
     (overriding)      nesnenin GERÇEK                      ölçüldü
                       tipine bakarak
  ─────────────────────────────────────────────────────────────────────
  ③ parametrik         DERLEYİCİ             derleme        kullanılan: var
     (generics)        tip argümanını        zamanında      yazılan:  >> 0 <<
                       yerine koyarak
  ─────────────────────────────────────────────────────────────────────
```

Akademik adları da var ve mülakatta geçebiliyor: ①'e **ad-hoc polymorphism**,
②'ye **subtype polymorphism** (ya da *inclusion*), ③'e **parametric
polymorphism** deniyor. Adları bilmek şart değil; ayrımı bilmek şart.

---

### ① Aşırı yükleme (overloading) — ***DERLEME ZAMANI***

**Tanım.** Aynı tipte, aynı ada sahip, **imzası farklı** birden çok metot.
İmzayı ayıran şey parametrelerin **sayısı ve tipidir**. Derleyici, çağrı
yerinde, argümanların **derleme zamanında bilinen** tiplerine bakarak
hangisinin çağrılacağını seçer ve bu seçim çıktıya **sabitlenir**.

***Dönüş tipi imzanın parçası DEĞİLDİR.*** Yalnız dönüş tipi farklı iki metot
yazmak derleme hatasıdır. Bu projede yakın bir karşılığı var ve öğretici:

```
Assets/Game/Core/Combat/Combatant.cs:167
        public void TakeDamage(int amount)
Assets/Game/Core/Combat/Structure.cs:110
        public bool TakeDamage(int amount)
```

Aynı ad, aynı parametre listesi, farklı dönüş tipi — ve **derlenir**, çünkü
bunlar iki AYRI tipte yaşıyor. Aşırı yükleme değil, sadece aynı adı taşıyan iki
bağımsız metot. ***Aşırı yüklemenin sınırı TİPTİR, ad değil.***

#### İşli örnek: hedefin tipine göre ayrılan tek satır

Projedeki en anlamlı aşırı yükleme çifti `AttackAction.Execute`. İkisi de aynı
akışı yürütüyor; ayrılan tek şey **hedefin tipi**:

```
Assets/Game/Core/Combat/AttackAction.cs:52
        public static AttackOutcome Execute(Combatant attacker, Combatant target, int distance)
Assets/Game/Core/Combat/AttackAction.cs:127
        public static AttackOutcome Execute(Combatant attacker, Structure target, int distance)
```

Çağıran taraf, aşırı yüklemenin **sınırını** aynı satırda gösteriyor:

```
Assets/Game/Battle/BattleActions.cs:127
            AttackOutcome outcome = targetIsStructure
                ? AttackAction.Execute(attackerCombatant, targetStructure, distance)
                : AttackAction.Execute(attackerCombatant, targetCombatant, distance);
```

***AYRIŞMA NOKTASI — burada iki şey ayrışıyor ve ayrımı görmek bu dosyanın asıl
işi:***

```
  Bu satır POLİMORFİK bir dallanma DEĞİLDİR.

  Derleyici iki çağrıyı da AYRI AYRI çözdü ve iki AYRI çağrı yazdı.
  targetStructure derleme zamanında bir yapı, targetCombatant derleme
  zamanında bir savaşçı; seçim orada bitti.

  >> Koşullu ifadeyi yazan DERLEYİCİ DEĞİL, İNSAN. <<
  Çünkü aşırı yükleme çalışma zamanında dallanamaz. Ezme olsaydı bu
  üç satır tek satıra inerdi ve dallanmayı çalışma zamanı yapardı —
  ama o gün başka bir bedel ödenirdi ve o bedel AttackAction.cs'in
  ikinci aşırı yüklemesinin üstünde, kodun kendi sözleriyle yazılı.
```

#### ***Aşırı yükleme bir DİKİŞ YERİDİR — ve dikişi kod tutmaz, TEST tutar***

`MoveAction`'ın iki `Execute`'u aşırı yüklemenin en sık kullanılan biçimini
gösteriyor: **kural bir sürümde yaşar, öteki ona devreder.**

```
Assets/Game/Core/MoveAction.cs:167
        public static MoveOutcome Execute(
```

Bu sürümün gövdesi tek satırda bitiyor: profilin menzilini okuyup `int` alan
sürüme devrediyor. Kuralın **metni** tek yerde. Ve bu devretme bir niyet değil,
**sınanan bir davranış**:

```
Assets/Tests/EditMode/Core/MoveActionTests.cs:379
        public void Execute_WithProfile_MatchesTheIntOverload()
```

***Aşırı yüklemenin ödediği ilk fatura burada görünüyor:*** iki imza aynı kuralı
yürüteceğine **söz veriyor**, ama derleyici bu sözü tutmaz. İki gövde zamanla
ayrışabilir ve hiçbir şey kırmızıya dönmez. Sözü tutan şey bir dil özelliği
değil, elle yazılmış bir karşılaştırma testi.

#### Dört üye, iki eksen — `CanBeAttacked`

Dört sürüm iki ayrı eksende ayrışıyor ve bu bir dağınıklık değil, kasıtlı bir
tablo:

```
                    tek parametre        durum + iki taraf
  ────────────────────────────────────────────────────────────
  UnitState              :44                   :65
  StructureState         :89                  :105

  >> İKİ EKSEN, İKİ AYRI SEBEP <<
  Yatay eksen (parametre sayısı) : taraf kuralı EKLENİYOR.
       Üç parametreli sürümler durum kuralını KOPYALAMIYOR,
       tek parametreli sürüme SORUYOR — kodun kendi ifadesiyle
       "kopyası değil ÇAĞIRANI".
  Dikey eksen (enum tipi)        : iki ayrı DURUM DİLİ.
       Tek enum'a katlansaydı her switch'te asla çalışmayan bir
       düşmüş-hâl dalı doğardı; gerekçe tipin kendi özetinde.
```

Aynı tipte `CanBeRevived` yalnız iki üye taşıyor ve **yapı sürümü yok**. Bu bir
eksik değil bir karar: yapı dirilmez, yeniden inşa edilir. Kararın metni
`TargetingRules.cs`'te, diriltme sürümlerinin hemen üstündeki blokta.

---

### ② Ezme (overriding) — ***ÇALIŞMA ZAMANI*** — bu projede **SIFIR**

**Tanım.** Taban tipteki bir metodun `virtual` (ya da `abstract`) işaretlenmesi
ve türeyen tipte `override` ile yeniden yazılması. Çağrı, **çağıranın gördüğü
tipe değil, nesnenin gerçek tipine** gider.

***Bu projede örnek YOK ve bu bir üslup ifadesi değil, iki kez ölçülmüş bir
olgu:*** kaynakta sıfır, üstveride sıfır. Ölçümün tamamı
[`dil/08`](../deep/dil/08-erisim-ve-sozlesme.md)'de.

#### ***BU ÖRNEK PROJE DIŞIDIR***

Aşağıdaki satırlar bu depoda **yoktur** ve buraya yazılmasının tek sebebi
karşılaştıracak ikinci tarafın olmaması. Uydurma bir proje örneği yazmak yerine
mekanizma çıplak hâliyle gösteriliyor:

```csharp
// >> PROJE DIŞI — Assets/ altında böyle bir kod YOKTUR <<
class Taban            { public virtual  string Ad() => "taban";   }
class Türeyen : Taban  { public override string Ad() => "türeyen"; }

Taban t = new Türeyen();
t.Ad();      // ► "türeyen"   >> nesnenin GERÇEK tipi seçti <<

// Taban metottaki `virtual` SİLİNSEYDİ ve türeyen `new` yazsaydı:
t.Ad();      // ► "taban"     >> değişkenin TİPİ seçti <<
```

```
  >> AYRIŞMA NOKTASI <<
  Aynı satır. Aynı nesne. Aynı değişken. İki farklı cevap.
  Ayıran tek şey taban metodun `virtual` olup olmaması.

  Ve üye gizleme (`new`) bir çok biçimlilik ARACI DEĞİLDİR: davranışı
  çağıranın gördüğü tipe bağlar — polimorfizmin tam TERSİ. Bir alt tip
  yerine geçemiyorsa Liskov da çoktan kırılmıştır.
```

#### ***Bugün sıfır — peki hangi gün doğar***

"Bugün yok" eksik bir cümledir. Ezmenin bu projeye gireceği koşullar, en
olasıdan en az olasıya, ve her biri koda bakılarak seçildi:

```
  ① GERÇEK BİR ALT TİP AİLESİ
     Ölçüt tek: iki tip ortak bir DEĞİŞMEZ durumu/davranışı paylaşıyor VE
     her alt tip taban sözleşmesini koruyor. Sınav bu projede bir kez
     yapıldı ve KAYBEDİLDİ — gerekçe Structure.cs'in başında. Sınavı
     kazanabilecek aday: aynı yaşam döngüsünü paylaşan iki BİRİM türü
     (piyade / okçu); ayrı bir döngü taşıyan bir yapı tipi değil.

  ② ÜÇÜNCÜ BİR SALDIRI HEDEFİ
     Bugün "saldırılabilir" iki şey var ve çağıran elle dallanıyor.
     ÜÇÜNCÜSÜ eklendiğinde aşırı yükleme sayısı üçe, koşullu ifade üç
     dala çıkar. >> Eşik burada: kazanç "iki metot" olmaktan çıkıp
     "N metot ve N dallı bir ifade" olur. << O gün ilk seçenek ezme
     DEĞİL, arayüz — çünkü savaşçı ile yapı arasında bir is-a yok.
     Arayüzün ölçütü dil/08'de.

  ③ MONOBEHAVIOUR TARAFINDA BİR ARA TABAN
     En olası ve en az öğretici yol: iki bileşen ortak bir ara taban
     paylaşmaya başlarsa `protected virtual void Awake()` şekli doğar.
     >> DİKKAT: motor geri çağrısının kendisi virtual değildir; virtual
     olan senin yazdığın ara tabandır. Ayrımın tamamı konular/08'de. <<

  ④ HİÇBİR ZAMAN
     Dördüncü ihtimal dürüstçe yazılıyor: bu proje bugünkü şekliyle
     büyürse ezme hiç doğmayabilir. Bileşim + sonuç enum'u + saf kural
     sınıfı üçlüsü, ezmenin çözdüğü problemi başka bir yerden çözüyor.
     >> Bu bir eksiklik değil, ölçülmüş bir denge. <<
```

---

### ③ Parametrik çok biçimlilik (generics)

**Tanım.** Bir tipin ya da metodun, üzerinde çalıştığı **tipi parametre olarak**
alması. `List<T>` bir tip değil bir **tip şablonudur**; `List<Unit>` ile
`List<Team>` ondan üretilmiş iki ayrı tiptir.

Bu projede generic'ler **kullanılıyor ama yazılmıyor**:

```
Assets/Game/Battle/Battle.cs:81
        private readonly Dictionary<Unit, Action<UnitState, UnitState>> stateForwarders =
Assets/Game/Battle/Battle.cs:179
        public event Action<Unit, UnitState, UnitState> UnitStateChanged;
Assets/Game/Battle/TurnState.cs:44
        public static readonly IReadOnlyList<Team> DefaultTurnOrder =
Assets/Game/Core/Combat/UnitLifecycle.cs:80
        public event Action<UnitState> StateChanged;
```

```
  KULLANILAN  22 satır, 6 tanım: Dictionary<,> · List<> · IReadOnlyList<>
              KeyValuePair<,> · Action<> · Action<,> · Action<,,>
  YAZILAN     kendi generic tipimiz 0 · generic metot 0 · kısıt 0

  >> İkisi arasındaki fark, "bu kavramı kapattım" ile "bu kavramı
     kullandım" arasındaki farkın ta kendisi. <<
```

Neden çok biçimlilik sayılıyor: sözlüğün iki farklı örneği —biri savaşçı, öteki
yapı tutuyor— **aynı kodu** iki farklı tip üzerinde çalıştırıyor. Alternatifi
`object` tutan bir sözlük olurdu; o gün her okumada bir tip dönüşümü, her değer
tipinde bir kutulama doğardı. Kutulamanın ölçülmüş tarafı
[`dil/07`](../deep/dil/07-bellek-canlilik-ve-yikim.md)'de, koleksiyon tarafı
[`dil/02`](../deep/dil/02-koleksiyonlar-ve-salt-okunur.md)'de — tekrar
edilmiyor. Delege satırları ayrıca gösteriyor ki generic yalnız koleksiyon işi
değil: `Action<UnitState>` ile `Action<UnitState, UnitState>` **farklı
tiplerdir** — arite imzanın parçası.

---

### ***EN PAHALI KARIŞIKLIK: aşırı yükleme ile ezme AYNI ŞEY DEĞİLDİR***

İki mekanizma da "aynı ad, farklı gövde" diyor. Sonra ayrılıyorlar ve
ayrıldıkları yer bir ayrıntı değil, **tasarımın tamamı**:

```
                     AŞIRI YÜKLEME              EZME
  ─────────────────────────────────────────────────────────────────
  ayıran şey         İMZA                       aynı imza, farklı GÖVDE
                     (parametre sayısı/tipi)
  gerektirdiği       tek bir tip yeter          İKİ tip + kalıtım
  seçimi yapan       DERLEYİCİ                  ÇALIŞMA ZAMANI
  neye bakar         argümanın DERLEME          nesnenin GERÇEK tipine
                     zamanı tipine
  çağrı hedefi       SABİT                      TABLODAN
  yeni davranış      yeni bir METOT             yeni bir TİP
    eklemek demek
  ─────────────────────────────────────────────────────────────────
  bu projede         >> 14 metot, 6 grup <<     >> SIFIR <<
```

Farkı bir cümlede sabitleyen sınav, taban tipli bir referans üzerinden çağrı
yapmaktır:

```csharp
// >> PROJE DIŞI — bu depoda böyle bir hiyerarşi yoktur <<
class Taban { public virtual void V() {}   public void N(Taban t) {} }
class Türeyen : Taban
{
    public override void V() {}            // EZME
    public          void N(Türeyen t) {}   // AŞIRI YÜKLEME
}

Taban t = new Türeyen();
t.V();          // ► Türeyen.V   ── nesne seçti
t.N(t);         // ► Taban.N     ── >> değişken seçti <<
```

```
  >> AYRIŞMA NOKTASI <<
  İki satır yan yana. Aynı nesne, aynı değişken, iki farklı kural.
  V ezildiği için NESNEYE gitti. N aşırı yüklendiği için değişkenin
  TİPİNE gitti — taban tipli bir değişken türeyendeki sürümü GÖRMEZ.

  Cümlenin ezberlenecek hâli:
      >> Aşırı yüklemeyi DERLEYİCİ görür, ezmeyi NESNE bilir. <<
```

---

### ***DÖRDÜNCÜ ADAY: varsayılan parametre — HİÇBİRİ DEĞİL***

Aşırı yüklemeyle en sık karıştırılan şey `virtual` değil, **varsayılan
parametre**. Bu projede beş tane var ve hepsi kuruculardan geliyor:

```
Assets/Game/Core/Combat/Structure.cs:55
            AttackProfile attackProfile = null)
Assets/Game/Core/Combat/StructureLifecycle.cs:55
        public StructureLifecycle(float rubbleWindowSeconds = DefaultRubbleWindowSeconds)
Assets/Game/Core/Combat/UnitLifecycle.cs:47
            float downedWindowSeconds = DefaultDownedWindowSeconds,
Assets/Game/Core/Combat/Combatant.cs:63
            Team team = Team.None)
```

```
  Varsayılan parametre TEK bir metot üretir. Derleyici ÇAĞRI YERİNE
  eksik argümanı yazar; ikinci bir gövde YOKTUR.

  >> SONUCU: varsayılan değer ÇAĞIRANIN assembly'sine kopyalanır. <<
  Değeri değiştirirsen, çağıranı yeniden derlemedikçe eski değer
  yaşamaya devam eder — `const`un assembly sınırında kopyalanmasıyla
  aynı tuzak; ölçülmüş hâli dil/01'de.

  Aşırı yükleme ise İKİ gövde üretir ve ikisi bağımsız değişebilir.
  >> SEÇİM ÖLÇÜSÜ: iki sürüm AYNI kuralı mı yürütecek (→ varsayılan
  parametre ya da devreden aşırı yükleme), yoksa FARKLI akış mı
  taşıyacak (→ ayrı aşırı yükleme). <<
```

Projede ikisi de var ve seçimleri gerekçeli: yapının isteğe bağlı saldırı
profili "yapıların çoğu saldırmaz" kuralını **imzada** okutuyor; saldırı akışı
ise iki ayrı gövde, çünkü akışın **sonu** ayrışıyor.

### ***Awake ve Update HİÇBİRİ DEĞİLDİR***

En yaygın yanlış model, ve tam olarak bu ailenin dışında:

```
  MonoBehaviour'un Awake / Start / Update / OnEnable / OnDisable
  ──────────────────────────────────────────────────────────────
  aşırı yükleme DEĞİL  →  ortada aynı adı taşıyan ikinci bir imza yok
  ezme          DEĞİL  →  virtual değiller; override yazarsan DERLENMEZ
  generic       DEĞİL  →  tip parametresi yok
  event         DEĞİL  →  += ile abone olunmaz
  arayüz üyesi  DEĞİL  →  hiçbir sözleşmede tanımlı değil

  >> Onlar AD TABANLI mesaj geri çağrılarıdır: motor tipi tarar, o adı
     taşıyan metodu bulur ve çağırır. Sözleşme derleyicide değil,
     MOTORUN kendisinde yaşıyor. <<
```

Ölçülmüş kanıtı ve bütün sonuçları
[`konular/08`](../deep/konular/08-motor-cagri-dongusu.md)'de. Buraya eklenen tek
şey ailedeki yeri: **dörtlünün hiçbir üyesine ait değiller.**

---

## Üçüncü durak: KALITIM — ***oyun mantığında sıfır satır***

Sekizi de yukarıda sayıldı ve sekizi de aynı şeyi söylüyor: **bu proje kalıtımı
oyun mantığında hiç kullanmıyor.** Sekiz kullanımın sekizi de motora bağlanmak
için — altısı bir bileşen olabilmek, ikisi bir varlık dosyası olabilmek adına.

```
Assets/Game/Unity/BoardAdapter.cs:111
    public sealed class BoardAdapter : MonoBehaviour, IPlacementBoard
```

***Bu bir eksiklik değil bir KARAR.*** Gerekçesi bu dosyada değil, kararın kendi
sahibinde: [`ogrenme/01`](01-koda-gomulu-desenler.md)'in 5. deseni (bileşim) —
baskıyı, Liskov ölçüsünü ve reddedilen alternatifi orada tam hâliyle taşıyor.
Burada tekrar edilmiyor; buraya ait olan tek şey **kalıtımın ailedeki yeri**.

### `is-a` / `has-a` — işli örnek

Kalıtımın tek meşru sorusu şudur: **"bu tip şunun BİR TÜRÜ mü?"** Cevabı hayırsa
kalıtım yanlış araçtır, ne kadar çok satır tasarruf ettirirse ettirsin.

```
  >> İKİ SORU, İKİ AYRI CEVAP <<

  "Bir savaşçı bir CAN mıdır?"            →  HAYIR
  "Bir savaşçı bir can TAŞIR mı?"         →  EVET
                                              ▼
Assets/Game/Core/Combat/Combatant.cs:44
        private readonly Health health;
Assets/Game/Core/Combat/Combatant.cs:45
        private readonly UnitLifecycle lifecycle;

  İki alan, iki parça, sıfır kalıtım. Dışarıya tek bir tip görünüyor,
  cevabı parça veriyor:

Assets/Game/Core/Combat/Combatant.cs:152
        public UnitState State => lifecycle.State;

  Aynı sınav yapı tarafında da yapılıyor ve AYNI cevabı veriyor:

Assets/Game/Core/Combat/Structure.cs:39
        private readonly Health health;
Assets/Game/Core/Combat/Structure.cs:40
        private readonly StructureLifecycle lifecycle;

  >> ORTAK OLAN TEK PARÇA: can. << Tesadüf değil, bu tipin varlığıyla
  SINANAN iddia: can kuralı tipten bağımsızsa barakanın canı askerin
  canıyla aynı sınıfla tutulabilmelidir. Tutuluyor. Yaşam döngüleri
  ise AYRI iki tip — çünkü baraka düşmez, yıkılır.

  >> Kalıtımın kaybettiği yer tam burası: parçaların YARISI ortak,
     yarısı değil. Kalıtım "yarısını devral" diyemez. <<
```

### Reddedilen kalıtım — kodun kendi sözleriyle

Bu proje kalıtımı sadece kullanmamış değil, **bir kez açıkça reddetmiş** ve
reddin gerekçesini silmemiş. Blok tipin ilk satırlarında duruyor:

```
Assets/Game/Core/Combat/Structure.cs:17
    // KALITIM AYNI PARÇALAR DEĞİL, AYNI YAŞAM DÖNGÜSÜ DEMEKTİR. `: Combatant`
```

Bloğun tamamı dört üye sayıyor ve dördü de barakada anlamsız: diriltme, düşmüş
hâli, zorunlu saldırı profili, ve kurtarma penceresi. Sonuç cümlesi tam olarak
Liskov'un ölçüsü:

```
  >> KALITIM SEÇMELİ DEĞİLDİR. <<
  `: Combatant` yazan gün, o dört üye de gelir. Gelmelerini
  engelleyecek bir dil özelliği YOK — `sealed` bu satıra karşı
  sıfır koruma sağlar, çünkü tartışılan şey kalıtımın YASAKLANMASI
  değil, SEÇİLMEMESİ.

  Ve reddin ikinci yüzü ölçülebilir: yapının hasar metodu "bu vuruş
  yıktı mı" diye bir bool döndürüyor, savaşçınınki void. Ortak bir
  taban o iki imzadan BİRİNİ seçmek zorunda kalırdı ve seçilmeyen
  taraf bilgisini kaybederdi.
```

Kalıtım dörtlünün **tek isteğe bağlı** üyesidir: kapsülleme her programda var,
soyutlama her imzada var, çok biçimlilik her aşırı yüklemede var — ama kalıtım
bir **hiyerarşi** ister ve hiyerarşi bedava değildir. Bu projenin cevabı
hiyerarşi yok, bileşim var, ve çok biçimlilik ② yerine ① üzerinden yürüyor.

---

## Dördüncü durak: SOYUTLAMA — ***arayüzsüz soyutlama da soyutlamadır***

En sık yanlış model tek cümlelik: *"soyutlama = interface."* Hayır.

```
  >> SOYUTLAMA BİLGİ GİZLEMEKTİR, TİP HİYERARŞİSİ DEĞİL. <<

  Ölçüsü tek soru:  ÇAĞIRAN neyi bilmek zorunda DEĞİL?
  Cevap "bir şey" ise orada bir soyutlama var — arayüz olsun olmasın.

  Bu projede interface sayısı: 0
  Bu projede soyutlama sayısı: her dosyada
```

Üç örnek, üçü de bu depodan ve üçü de **arayüzsüz**.

### ① Bir imza bir soyutlamadır

```
Assets/Game/Battle/Battle.cs:186
        public void AddUnit(Unit unit, Combatant combatant, int x, int y)
```

Çağıran şunları **bilmek zorunda değil**: birimin hangi sözlüğe yazıldığını,
tahtaya hangi sırayla yerleştirildiğini, durum olayının nasıl bağlandığını, ve
yarım kalmış bir hâlin nasıl engellendiğini. Üç şey —birim, savaşçı, konum— tek
çağrıda geliyor; çünkü ayrılırlarsa aralarında yarım bir hâl doğar.

***İşte soyutlama tam burada: bir tek imza, dört ayrı iç adımı gizliyor.***

### ② Bir erişim belirteci bir soyutlamadır

```
Assets/Game/Battle/Battle.cs:107
        internal UnitGrid Board => board;
```

Projenin tek `internal`i. Motor katmanı bu üyeyi **göremez** — tahtayı
tanımaması gerektiği için değil, tahtaya **yazmaması** gerektiği için. Duvarın
tam mekanizması ve garantinin nerede bittiği
[`dil/08`](../deep/dil/08-erisim-ve-sozlesme.md)'de; buraya ait olan tek cümle:
**bir şeyi görünmez yapmak, onu soyutlamanın en ucuz biçimidir.**

### ③ Bir enum bir soyutlamadır

```
Assets/Game/Core/MoveOutcome.cs:26
    public enum MoveOutcome
```

Beş değer, ve çağıran hareketin **neden** reddedildiğini biliyor ama **nasıl**
karar verildiğini bilmiyor: Chebyshev mesafesinin nerede hesaplandığını, hangi
kuralın hangi sırayla sorulduğunu, hangi katmanın hangi değeri üretebildiğini.

```
  >> AYRIŞMA NOKTASI <<
  Bir enum "veri" gibi görünür, ama burada bir SÖZLEŞME:
  beş değerden hangisinin döneceği bir karar zinciridir ve
  zincir tamamen gizlidir. bool döndürülseydi çağıran üç ayrı
  reddi ayırt edemez, ayırt etmek için kuralları KOPYALARDI.

  Soyutlamanın ölçüsü budur: kopya doğuruyor mu, doğurmuyor mu.
```

### Üçünün ortak şekli

```
  ① imza          →  ADIMLARI gizler
  ② erişim duvarı →  ÜYEYİ gizler
  ③ sonuç enum'u  →  KARAR ZİNCİRİNİ gizler
  ────────────────────────────────────────────────────────────
  >> Üçünde de arayüz YOK, üçü de soyutlama. <<
  Arayüz dördüncü bir araçtır ve ötekilerden ÜSTÜN değildir:
  başka bir soruya cevap verir — "bir çağıran KAÇ uygulama
  görmeli". Ölçütü ve bu projedeki cevabı dil/08'de.
```

---

## Üç oyun: "aynı çağrıya farklı cevap veren şey nasıl kuruluyor"

> ***DOĞRULAMA SINIRI: üç oyunun da kaynağı KAPALIDIR.*** Aşağıdaki üç hücrenin
> hiçbiri kaynak koda ya da resmî belgeye karşı **doğrulanmadı**; hepsi
> *oyuncunun gördüğü* olgular. Mekanizma adı ve rol etiketi bilerek yazılmıyor —
> bu tabloda yalnız **ad** ve **iş** var.

| Oyun | Aynı basıncı taşıyan şeyin ADI ve İŞİ |
|---|---|
| **Slay the Spire** | ██ EŞLEŞMEYEN ██ Kartlar. Tek bir "oyna" hareketi, karta göre bambaşka bir iş: hasar, blok, çekiliş, güç, dönüştürme. Oyuncu hep aynı şeyi yapar; ne olacağını elindeki kart taşır. |
| **Vampire Survivors** | Silahlar. Hepsi kendiliğinden ateşlenir ve hepsi aynı ana ("sayaç doldu") cevap verir — ama biri halka çizer, biri kırbaç savurur, biri hedefe yönelir. |
| **Stardew Valley** | ██ EŞLEŞMEYEN ██ Aletler. Aynı tıklama, aynı karo: balta ağaç keser, kazma taş kırar, kova su alır. Cevabı belirleyen şey hedef değil, elde tutulan alet. |

### ***İKİ SATIR NEDEN EŞLEŞMİYOR***

```
  BİZDE                                 SLAY THE SPIRE'DA
  ────────────────────────────          ──────────────────────────────
  farklı cevap veren şey sayısı: İKİ    yüzlerce, ve sayı İÇERİKLE büyür
  küme KAPALI: üçüncüsü ancak yeni      küme AÇIK: yeni davranış yeni
  bir TİP yazılarak doğar               derleme istemez
  seçim DERLEME zamanında               seçim ÇALIŞMA zamanında, liste
  (aşırı yükleme çözümlemesi)           VERİDEN geliyor
  ██ ölçü: tek ikili koşul, iki dal ██  ██ o ölçekte "her tip için bir
                                           dal" şekli hiç yazılamaz ██

  BİZDE                                 STARDEW'DE
  ────────────────────────────          ──────────────────────────────
  ayıran şey HEDEFİN tipi               ayıran şey ELDE TUTULANın tipi
  (saldıran hep aynı)                   (hedef hep aynı: karo)
  >> AYRIŞMA NOKTASI: yön TERS <<  Bizde davranışı seçen şey ARGÜMAN,
  orada SAHNENİN DURUMU. İkisi de "aynı çağrı, farklı davranış"
  cümlesine uyar ve ikisi tamamen farklı bir mekanizma ister.
```

**Vampire Survivors** tek eşleşen satır ve sebebi ölçülebilir: silahlar
birbirinden **bağımsız** değişiyor ve hiçbiri ötekinin alt türü değil — yani bu
projenin **bileşim** cevabıyla aynı şekil. Kalıtım orada da kaybederdi.

---

## ***MÜLAKAT — beş soru, iki biçimde***

***Kural: uydurma cevap yazılmıyor.*** "Kullanmadım" bir kayıp değil, ölçülü bir
cevaptır — ölçüsüyle söylendiğinde. Cevap sırası
`portfolio-and-interview.archive`'ın *Interview Answer Contract*'ından geliyor:
bağlam → gözlenen problem → seçilen mekanizma → en yakın alternatif → alternatif
neden kaybetti → kanıt → takas ve geri dönüş koşulu.

### S1 — "Overloading ile overriding farkı nedir?"

**KISA (30 sn).** Aşırı yüklemede aynı ad **farklı imza** taşır ve seçimi
**derleyici**, argümanın derleme zamanı tipine bakarak yapar. Ezmede imza
aynıdır, gövde farklıdır ve seçimi **çalışma zamanı**, nesnenin gerçek tipine
bakarak yapar. Tek cümlelik ayırıcı: *aşırı yüklemeyi derleyici görür, ezmeyi
nesne bilir.*

**GENİŞLETİLMİŞ (2 dk).** Projemde ikisinin de karşılığı ölçülü: aşırı yükleme
**altı grup, on dört metot**; ezme **sıfır**. En anlamlı çift bir saldırı
akışında — hedefin savaşçı mı yapı mı olduğuna göre iki `Execute`. Çağıran
tarafta bir koşullu ifade var ve ***o ifadeyi derleyici değil ben yazdım***,
çünkü aşırı yükleme çalışma zamanında dallanamaz. Ezme olsaydı o üç satır tek
satıra inerdi; inmedi, çünkü ortak bir sözleşme yazmak hedef uygunluğu kuralını
saf kural sınıfından hedefin içine taşırdı ve iki farklı ölüm sonucunu tek bir
`bool`un arkasına düşürürdü. Kazanç iki metot, bedel üç karar — takas o yüzden
bu tarafta duruyor. Geri dönüş koşulu net: üçüncü bir hedef türü eklendiği gün
kazanç "iki metot"tan "N metot ve N dallı ifade"ye döner ve karar değişir.

### S2 — "Neden kalıtım yerine bileşim?"

**KISA (30 sn).** Çünkü kalıtım **seçmeli değildir**: `: Taban` yazdığın gün
tabanın **her** üyesi gelir. Projemde kalıtım satırı **sekiz** ve sekizi de
motorun zorunlu kıldığı satır — hepsi tek assembly'de, hiçbiri proje-yerel bir
tabandan değil; oyun mantığında **sıfır**. Ana tiplerim yeteneklerini
devralarak değil, parçaları **alan olarak tutarak** kazanıyor.

**GENİŞLETİLMİŞ (2 dk).** Sınavı bir kez gerçekten yaptım ve **kaybettim**:
yapı tipini savaşçıdan türetmeyi denedim, reddettim, ve reddin gerekçesini
koddan silmedim — tipin ilk satırlarında duruyor. Baraka, devralacağı dört
üyeye uymuyordu: diriltme, düşmüş hâli, zorunlu saldırı profili, kurtarma
penceresi. Bileşimin ölçüsü ise şu: savaşçı **iki** parça tutuyor (can + yaşam
döngüsü), yapı **iki** parça tutuyor (can + yapı yaşam döngüsü), ve ortak olan
**tek** parça can. Bu tesadüf değil, sınanan iddia: can kuralı tipten
bağımsızsa barakanın canı askerinkiyle aynı sınıfla tutulabilmelidir —
tutuluyor. ***Kalıtım "parçaların yarısını devral" diyemez; bileşim diyebilir.***
Geri dönüş koşulu: aynı yaşam döngüsünü paylaşan iki gerçek birim türü
doğduğunda soyut taban yeniden sınanır.

### S3 — "Interface'i ne zaman yazarsın?"

**KISA (30 sn).** Ölçüt varlık sayısı değil, **çağıranın ihtiyacı**: bir çağıran
aynı yetenek sözleşmesi arkasında **gerçekten** birden fazla uygulamaya ihtiyaç
duyuyorsa. "Üç tip var, o hâlde bir arayüz olmalı" bir ölçü değildir.

**GENİŞLETİLMİŞ (2 dk).** Projemde arayüz sayısı **bir** (`IPlacementBoard`) ve
bu sayının uzun süre **sıfır** kalması bir ihmal değildi: arayüzü yazdıran şey
"üç tip var" değil, ***adı konmuş bir çağıranın somut tipi görmemesi
gerekmesi*** oldu. O gün 2026-08-25'te geldi — üretim katmanı yazılırken
`ProductionDirector`'ın tahtayı çağırması gerekti ama tahtanın dosyası başka
bir hattın malıydı. Basınç somuttu, arayüz o gün doğdu. Ondan önce arayüz
basıncının şeklini taşıyan tek yer vardı —bir çağıran, tek bir yetenek, iki
farklı uygulama— ve orada yazılmadı, çünkü kazanç iki metot, bedel üç karardı. Ölçütün tam metni ve arayüzü doğuracak dört somut
olay `dil/08`'de yazılı; ben o dosyanın karar ağacını kullanıyorum. En yakın
alternatifleri de eledim ve elemeler kayıtlı: soyut taban reddedildi (gerekçe
kodda), test sahtesi reddedildi (gerekçe bir testin içinde). Arayüzün ilk
geleceği gün de belli: bir parça ağ, dosya ya da rastgelelik taşıdığı gün sahte
parça şart olur — ve sahte parça için önce bir arayüz gerekir, çünkü parçalarım
`sealed`.

### S4 — "Soyutlama ile kapsülleme farkı nedir?"

**KISA (30 sn).** Kapsülleme **kim görebilir** sorusunun cevabı; soyutlama
**neyi bilmek zorunda değil** sorusunun. Biri erişimi, öteki bilgiyi yönetir. En
temiz örnek ikisinin aynı satırda buluşması: bir `internal` özellik.

**GENİŞLETİLMİŞ (2 dk).** Projemin tek `internal` üyesi tahtayı veren bir
özellik. **Kapsülleme yüzü**: motor katmanı o üyeyi göremez — referans vermek
yetmiyor, duvarı `.asmdef` çiziyor. **Soyutlama yüzü**: o üyenin varlığı sayesinde
hareket kuralı savaşı hiç tanımadan çalışabiliyor; çağıran tahtanın nasıl
tutulduğunu bilmiyor. ***İki ayrı iş, tek satır.*** Ve ikisi birbirinin yerine
geçmez: her şeyi `public` yapsam soyutlama durur, kapsülleme biter; tahtayı hiç
dışarı vermesem kapsülleme mükemmel olur ama hareket kuralını savaşın içine
kopyalamam gerekir — yani soyutlama bozulur. Arayüzsüz soyutlamanın iki örneği
daha var bende: bir birim ekleme imzası dört iç adımı gizliyor, bir sonuç
enum'u tüm karar zincirini gizliyor. Soyutlama bilgi gizlemektir; tip
hiyerarşisi değil.

### S5 — "Polimorfizmi nerede kullandın?"

***DÜRÜST CEVAP — uydurma yok.***

**KISA (30 sn).** Üç türden **ikisini** kullandım. **Aşırı yükleme**: altı grup,
on dört metot. **Parametrik**: hazır generic tipler her katmanda — sözlükler,
salt okunur listeler, delege tipleri. **Ezme: sıfır**, çünkü projede sanal metot
yok ve bunu ölçtüm — hem kaynakta hem derlenmiş çıktının üstverisinde.

**GENİŞLETİLMİŞ (2 dk).** Ezmenin sıfır olması bir boşluk değil, bir kararın
sonucu: kalıtımı oyun mantığında hiç kullanmadım, dolayısıyla ezilecek bir taban
metot da doğmadı. Buna karşılık aynı işi başka üç mekanizma yapıyor: aşırı
yükleme (derleme zamanı dallanma), sonuç enum'u (çağıranın davranışı seçmesi),
ve bileşim (davranışın parçadan gelmesi). ***En sevdiğim örnek bir aşırı yükleme
çifti:*** biri menzili çıplak sayı, öteki bir profil nesnesi alıyor ve **kuralı
kopyalamıyor, ötekine devrediyor**. Bu bir dikiş yeri ve derleyici o dikişi
tutmaz — iki gövde sessizce ayrışabilir. Onun için ikisinin aynı sonucu
verdiğini sabitleyen bir test yazdım; kırmızıya dönerse biri profil sürümüne
ikinci bir akış yazmış demektir. Ezmenin geri döneceği koşulu da yazılı tutuyorum:
gerçek bir alt tip ailesi doğduğu gün — parçaların yarısı değil, **yaşam
döngüsünün tamamı** ortaksa.

### Kendini puanla — 0'dan 4'e

```
  0  yanlış ya da boş
  1  yalnız tanım              ("overloading aynı ad farklı imza")
  2  doğru örnek               ("projede iki Execute var")
  3  takas ve alternatif       ("kazanç iki metot, bedel üç karar")
  4  >> sahiplenilmiş kanıt + sınır/geri dönüş koşulu <<
     ("ölçtüm: 14 metot, 0 ezme; üçüncü hedef türünde karar değişir")

  >> 3 ile 4 arasındaki fark bir cümle: NE ZAMAN YANLIŞ OLUR. <<
```

---

## Kural: karar ağacı — "aynı çağrı, farklı davranış" istediğinde

```
① Davranış sayısı DERLEME zamanında biliniyor mu?
      EVET → ②
      HAYIR (liste veriden geliyor, içerikle büyüyor) → >> hiçbiri <<
             Bu bir tip problemi değil bir VERİ problemi; tabloya bak,
             tip ağacına değil. (Slay the Spire satırı)

② Farkı ARGÜMANIN tipi mi yaratıyor?
      EVET → >> AŞIRI YÜKLEME << — ve iki soruyu hemen cevapla:
             · gövdeler aynı kuralı mı yürütecek?
                 EVET → biri ötekine DEVRETSİN + karşılaştırma testi yaz
                 HAYIR→ iki bağımsız gövde, ve ayrıştıkları yeri YAZ
             · çağıran runtime'da dallanmak zorunda kalacak mı?
                 EVET → dal sayısını SAY. 2 ise kabul, 3+ ise ③'e dön
      HAYIR → ③

③ Farkı NESNENİN kendi tipi mi yaratıyor?
      EVET → ④
      HAYIR (farkı bir DURUM yaratıyor) → sonuç enum'u ya da durum
             makinesi. Polimorfizm gerekmez.

④ İki tip gerçek bir AİLE mi — ortak değişmez durum/davranış paylaşıp
   her biri taban sözleşmesini koruyor mu?
      EVET → >> EZME << (virtual/override, abstract taban)
      HAYIR→ ⑤

⑤ Yetenekler birbirinden BAĞIMSIZ değişiyor mu?
      EVET → >> BİLEŞİM << — parçayı alan olarak tut, devralma
             (bu projenin cevabı)
      HAYIR→ >> ARAYÜZ << — ve dar tut. Ölçütü ve geri kalanı dil/08'de.

  >> Beş adımın ikisi bu projede kullanılıyor (② ve ⑤), üçü boş. <<
  Boş olanlar işaretli; doldurulmuyor.
```

---

## Yanlış hatırlanan üç şey

**"Polimorfizm = interface."** Hayır — ve bu iddia bu projede tek satırda
çürüyor: arayüz sayısı **bir**, buna karşılık on dört aşırı yüklenmiş metot
ve altı ayrı generic tanım kullanımda. ***Oran bu iddiayı eskisinden daha iyi
çürütüyor:*** arayüz sıfırken "henüz sırası gelmemiş" denebilirdi; bir arayüz
yazıldıktan sonra bile çok biçimliliğin geri kalanı ona hiç dokunmadan
çalışmaya devam ediyor. Arayüz polimorfizmin **bir aracıdır**,
tanımı değil; hatta çok biçimliliğin üç türünden **hiçbirinin zorunlu şartı**
değildir. Aşırı yükleme tek bir tip içinde olur, generic hiçbir hiyerarşi
istemez, ezme için arayüz değil **kalıtım** yeter. ***Ölçü: bu projede
polimorfizm var, arayüz yok.***

**"Overloading ile overriding aynı şey."** Hayır — ve fark bir üslup ayrıntısı
değil, seçimi kimin yaptığıdır. Aşırı yüklemede seçimi **derleyici** yapar ve
argümanın **derleme zamanı** tipine bakar; ezmede seçimi **çalışma zamanı**
yapar ve nesnenin **gerçek** tipine bakar. Karşı örnek yukarıda işlendi: taban
tipli bir referans üzerinden ezilmiş metot türeyene gider, aşırı yüklenmiş metot
tabanda kalır. ***Ezberlenecek hâli: aşırı yüklemeyi derleyici görür, ezmeyi
nesne bilir.*** Ve bu projede biri var, öteki yok — ikisi aynı şey olsaydı bu
cümle kurulamazdı.

**"`Awake` ve `Update` ezilmiş metotlardır."** Hayır — üçlü bir ret: `virtual`
değiller (yazarsan derlenmez), `event` değiller, ve hiçbir sözleşmede tanımlı
değiller. Ad tabanlı **mesaj geri çağrılarıdır**; motor tipi tarar, o adı taşıyan
metodu bulur ve çağırır. Sözleşme derleyicide değil motorda yaşıyor. ***Ölçü: bu
projede o geri çağrıların hepsi `private` yazılmış ve bu mümkün — bir `override`
`private` olamazdı.*** Mekanizmanın tamamı
[`konular/08`](../deep/konular/08-motor-cagri-dongusu.md)'de.

---

## Kaçış yolu: bu kararlar tersine çevrilseydi

```
  Aşırı yükleme yerine     → Çağıranın koşullu ifadesi tek satıra iner.
  ortak bir sözleşme         KAZANÇ: iki metot, bir dal. BEDEL: hedef
  yaz                        uygunluğu kuralı saf kural sınıfından
                             hedefin İÇİNE taşınır, iki farklı ölüm
                             sonucu tek bir bool'un arkasına düşer.
                             >> Gerekçe kodun kendi bloğunda yazılı. <<

  Bileşim yerine kalıtım   → Yapı, savaşçının dört üyesini de devralır:
  kur                        diriltme, düşmüş hâli, zorunlu saldırı
                             profili, kurtarma penceresi. Derleyici
                             susar. İlk kırılan şey bir test değil bir
                             ANLAM: "yıkık bina diriltilebilir" olur.

  Ezme ekle (virtual)      → Bugün ezilecek bir taban metot YOK; önce
                             bir hiyerarşi kurman gerekir. >> Yani bu
                             satır tersine çevrilebilir bir karar bile
                             değil — bir ÖN KOŞUL eksikliği. <<

  Her şeyi arayüzün        → Hiçbir şey KIRILMAZ ve mesele tam olarak bu:
  arkasına koy               soyutlamanın faturası bugün değil, ikinci
                             uygulamayı yazdığın gün ödenir. Bugün ikinci
                             uygulama YOK. >> Ödenmemiş bir faturanın
                             karşılığında bugün ödenen bedel, bir tasarım
                             değil bir alışkanlıktır. <<
```

---

## ***Adı geçen ama anlatılmayan mekanizmalar***

```
  Kovaryans / kontravaryans (out T, in T)   → HENÜZ YOK → sahipsiz.
      Doğacağı an: ilk kendi generic arayüzümüzün yazıldığı gün.

  Açık arayüz uygulaması, varsayılan üyeler → HENÜZ YOK → sahipsiz.
      "Doğacağı an: ilk arayüz yazıldığı gün" yazıyordu; O GÜN GELDİ
      (IPlacementBoard.cs:39) ve mekanizma yine de doğmadı. Ölçü:
      BoardAdapter sekiz üyeyi de ÖRTÜK uyguluyor, yani "void
      IPlacementBoard.PlaceUnit(...)" biçiminde tek satır yok; varsayılan
      gövde taşıyan üye de yok. Doğacağı yeni an: aynı tipin ADI ÇAKIŞAN
      iki arayüz uyguladığı gün.

  operator aşırı yüklemesi, implicit /      → HENÜZ YOK → sahipsiz.
  explicit dönüşümler                          Doğacağı an: ilk değer
      tipi (koordinat benzeri) yazıldığı gün.

  Generic kısıtlar (where T : ...)          → HENÜZ YOK → kayıtlı;
      defterde satırı var: [03-kavram-borc-defteri.md](03-kavram-borc-defteri.md)

  Sanal çağrının ÖLÇÜSÜ (nanosaniye/bayt)   → ÖLÇÜLMEDİ ve bugün
      ÖLÇÜLEMEZ: Virtual damgalı metot sıfır, karşılaştırılacak
      ikinci taraf yok. "Sanal çağrı pahalıdır" bu belgede bir ÖLÇÜ
      değil bir ETİKET olurdu, ve etiket yazılmaz.
```

---

## Bunu okuduktan sonra kodda ne göreceksin

`TargetingRules`'ın altı metodu artık bir tekrar değil — **iki eksende yayılmış
bir aşırı yükleme tablosu**, ve üç parametreli sürümlerin tek parametreli
sürümleri çağırması bir konfor değil, kuralın tek metinde kalması. Saldırı
akışındaki koşullu ifade bir `if` değil, **aşırı yüklemenin sınırının el
yazımıyla kapatıldığı yer**. Yapı tipinin başındaki reddedilmiş kalıtım bloğu
bir açıklama değil, **Liskov sınavının tutanağı**. Ve `virtual` kelimesinin
hiçbir yerde geçmemesi bir eksiklik değil — ilk üç üye doğru kurulduğu için
dördüncünün **başka bir kapıdan** girmiş olması.

Kodda **karar**, burada **ailenin haritası**. İkisi çelişirse kod kazanır —
orası çalışan metin, burası anlatı.

---

## İlgili

- Erişim, sözleşme, `interface`in arka tarafı ve K43:
  [`dil/08`](../deep/dil/08-erisim-ve-sozlesme.md)
- `sealed` / `readonly` / `const`un assembly sınırında kopyalanması:
  [`dil/01`](../deep/dil/01-degismezlik-anahtar-kelimeleri.md)
- Generic koleksiyonların vaadi ve `KeyValuePair`:
  [`dil/02`](../deep/dil/02-koleksiyonlar-ve-salt-okunur.md)
- Kutulama ve tahsis ölçümü: [`dil/07`](../deep/dil/07-bellek-canlilik-ve-yikim.md)
- Motor geri çağrıları neden hiçbiri değil:
  [`konular/08`](../deep/konular/08-motor-cagri-dongusu.md)
- Bileşimin gerekçesi, elenen desen adayları ve SOLID beş harf:
  [01-koda-gomulu-desenler.md](01-koda-gomulu-desenler.md)
- Kavramın defterdeki satırı: [03-kavram-borc-defteri.md](03-kavram-borc-defteri.md)
- Bu ağacın yönlendirmesi: [README.md](README.md)
