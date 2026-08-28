# Derin Anlatım — yönlendirme

Kodda **karar** yaşar, burada **hikâye**. İkisi çelişirse kod kazanır: orası
çalışan metin, burası anlatı.

## Neden ayrı dosya

Bir yorum bloğu üç şeyi birden taşımak zorunda kalınca kimseye yaramaz hâle
geliyor: *ne reddedildi*, *neden reddedildi*, ve *bu mekanizma aslında nasıl
çalışıyor*. İlk ikisi kararın kendisi ve koda ait — onları görmeden kodu
değiştiremezsin. Üçüncüsü ise bir kez öğrenilir, sonra hatırlanır; her okumada
gözünün önünde durması gerekmez.

Sınır şu:

| İçerik | Yeri |
|---|---|
| Reddedilen alternatif, kırılan şey, tek cümlelik sonuç | **kod** — silinmez |
| Tek konuya ait tek figür (ağaç, ok, tablo, zincir) | **kod** |
| Birden fazla dosyayı kat eden mekanizma, katman zinciri, "nasıl çalışıyor" | `konular/` |
| Aynı konunun ikinci ve üçüncü figürü | `konular/` |
| Bir üyenin tam gerekçesi (reddedilen alternatif dahil) | `kod/` — tip başına ayna |
| Ödünç alınan bir BCL tipi ya da dil özelliğinin ne vaat ettiği | `dil/` — kavram başına |

Pratik eşik: bir blok kararı anlatmayı bitirdikten sonra hâlâ ~25 satır
açıklamaya ihtiyaç duyuyorsa, o açıklama buraya taşınır ve koda tek satırlık bir
`DERİN ANLATIM:` yönlendirmesi kalır.

## Neden `Docs/` altında, `Assets/` altında değil

Ölçüldü, tahmin değil:

```
Assets/ altındaki bir .md  →  Unity import eder, .meta + GUID üretir
                              (kanıt: Assets/Art/THIRD_PARTY_ASSETS.md.meta)
Docs/  altındaki bir .md   →  .meta YOK. Unity hiç görmez.

Derlemeye etkisi: İKİSİNDE DE SIFIR.
   Kanıt: GridStrategy.Battle.csproj içinde 6 adet <Compile Include=...>
   ve hepsi .cs. .md hiçbir csproj'de geçmiyor; .asmdef bile
   <None Include=...> olarak duruyor, yani derlenmiyor.
```

Yani derleme kaygısı yersiz — ama `Assets/` altında olsaydı her dosya bir GUID
sahibi olur, taşınırken `.meta` ile birlikte taşınması gerekir, `AssetDatabase`
şişerdi. `Docs/` bu maliyeti sıfırlıyor.

**Uzantı `.md`, `.txt` değil:** derleme etkisi aynı (sıfır), ama `.md` VS Code'da
önizlenir (`Ctrl+Shift+V`), GitHub'da render olur, ASCII figürleri kod bloğu
içinde bozulmaz.

## Koddan buraya nasıl gelinir

Dürüst olmak gerekirse: **C# yorumundaki dosya yoluna `Ctrl` + tıklamak VS
Code'da çalışmaz.** `Ctrl` + tıklama tipler ve üyeler için çalışır, yorum içindeki
düz metin için değil. (Rider yorumdaki yolları link olarak algılar; VS Code
algılamaz.)

Çalışan yol:

```
Ctrl + P  →  dosya adının ayırt edici parçasını yaz  →  Enter
             örnek: "olay-zinciri"
```

Bu yüzden dosya adları **numaralı ve konu adlı**: `01-olay-zinciri.md`. Numara
sıralamayı, ad aramayı sağlıyor. Tip adına göre değil konuya göre adlandırılıyor,
çünkü bir mekanizma çoğu zaman tek dosyaya ait değil — olay zinciri dört ayrı
`.cs` dosyasını kat ediyor.

## Nasıl yazılır

Ders kitabı değil, hikâye. Sırasıyla:

1. **Sahne** — oyunda görünen tek cümlelik olay ("asker yere yatıyor")
2. **Karakterler** — ilgili tipler; her biri için *bilir* ve **BİLMEZ**
   listesi. Hikâyeyi ilginç kılan bilmedikleridir.
3. **Duraklar** — olay/veri hangi tipten hangisine, ne kazanarak geçiyor
4. **Tek bakışta zincir** — figürün tamamı, **ayrışma noktası**
   `>> … <<` ile işaretli (terimin tanımı: [Sözlük](#sozluk))
5. **Kural** — okuyucunun kendi kodunda uygulayacağı karar ağacı
6. **Yanlış hatırlananlar** — bu konuda tipik iki üç yanlış model, adıyla
7. **Kaçış yolu** — bu tasarımdan nasıl kaçılırdı ve neden kaçılmadı

## Sözlük

Bu ağaçta tekrar tekrar geçen, ama tek bir yerde tanımlanmadığı için okuyucuyu
tökezleten üç terim. Tanım burada bir kez veriliyor; dosyalar onu tekrar
etmiyor.

**AYRIŞMA NOKTASI.** Bir figürde `>> … <<` ile işaretlenen satır, o figürde
**iki şeyin birbirinden ayrıldığı** yerdir: sezginin bir şey, kodun başka bir
şey söylediği satır. Bir kavram değil, bir **işaret**; her figürde ayrışan iki
şey **farklıdır** ve neyin neyden ayrıldığını figürün kendi metni söyler.
Örnekler: `konular/02`'de klasör ile ad alanının çeliştiği satır; `konular/03`'te
`readonly`'nin bakmadığı satır; `dil/07`'de kapsam ile canlılığın ayrıldığı yer.
***İşareti gördüğünde sorulacak soru tektir: *burada hangi iki şey ayrışıyor?****

***İşaretin biçimi 2026-08-24'te değişti.*** Eskiden `██ … ██` yazılıyordu.
Blok karakteri (`U+2588`) her yazı tipinde çözülmüyor ve çözülmediğinde boş bir
kutu gibi görünüyor — okuyucu onu doldurulacak bir boşluk sanıyor. Yerine iki
ASCII karakter kondu, ve genişlik birebir aynı olduğu için figürlerin sütun
hizası bozulmadı. Ağaçta hâlâ `██` gören olursa ya bir **çizimin** parçasıdır
(kutu duvarı, ayırıcı, çubuk) ya da rolü çıkarılamadığı için kasten bırakılmış
bir işarettir — ikisi de vurgu değildir.

**AKIŞ SAHİBİ** (transaction script). Birden fazla kuralı **belirli bir sırayla**
soran, ama kuralların hiçbirini kendisi yazmayan tip. Bu projede `BattleActions`,
`MoveAction` ve `AttackAction` tam olarak budur — ve üçü de bir **Command
değildir** (ölçü: üçü de `static class`, hiçbirinin alanı yok, dolayısıyla geri
alınabilir bir "komut nesnesi" ortada yok). Deseni adıyla anlatan yer:
[../ogrenme/01-koda-gomulu-desenler.md](../ogrenme/01-koda-gomulu-desenler.md).

**SIRA DEVRİ.** Bir eylemin sonunda sıranın öteki tarafa geçmesi
(`TurnState.EndTurn`) — ve bu projede **her** eylemin sonunda değil, yalnızca
bir **beyaz listeye** düşen sonuç değerleri için. "Beyaz liste", devri
tetikleyecek sonuçların tek tek sayılmasıdır; karşıtı olan kara liste
(*"şunlar hariç hepsi devreder"*) yeni bir ret değeri eklendiği gün sessizce
yanlış cevap verirdi. ***Zincirin geri dönülemez adımı budur: bir kez devredilen
sıra geri alınmaz.*** Yeri: `konular/04` ADIM 7.

## Üç ağaç

```
Docs/deep/
├── kod/       TİP ekseni      Assets/Game/X/Y.cs → kod/X/Y.md
│                              "bu üye neden böyle"        → kod/README.md
├── konular/   MEKANİZMA ekseni  çok dosyayı kat eden akışlar
│                              "bu nasıl çalışıyor"        → aşağıdaki tablo
└── dil/       KAVRAM ekseni   ödünç alınan BCL tipleri ve dil özellikleri
                               "bu ne vaat ediyor"         → dil/README.md
```

## Dosyalar — konular/

***Numara bir DOSYA KİMLİĞİDİR, bir sıra değil.*** Aşağıdaki tablo bir
**indekstir**: hangi soru hangi dosyaya gider. Öğrenme sırası ayrı bir belgede
ve numaralara **uymuyor** — ölçüldü, o günkü sekiz `konular/` dosyasından
(`01`-`08`) yalnız ikisi (03 ve 08) numara sırasında doğru yerde duruyor:
[../ogrenme/00-okuma-sirasi.md](../ogrenme/00-okuma-sirasi.md). Sonradan gelen
`09` ve `10` o ölçümün dışında ve okuma yoluna henüz alınmadı.

Dosyalar birbirinden bağımsız da değil: `01` ile `02` karşılıklı olarak
ötekinin okunmuş olduğunu varsayıyor (`01:55` ↔ `02:440`). Bir dosyayı sırasız
açmak mümkün, ama "niye böyle" sorusunun cevabı çoğu zaman başka bir dosyada.

| # | Konu | Koddaki durakları |
|---|---|---|
| [01](konular/01-olay-zinciri.md) | **Olay zinciri** — kapanış kimliği, sözlüğün neden var olduğu, event leak yönü | `UnitLifecycle.StateChanged`, `Combatant.StateChanged`, `Battle.stateForwarders`, `Battle.UnitStateChanged`, `BoardAdapter.OnUnitStateChanged` |
| [02](konular/02-assembly-duvari.md) | **Assembly duvarı** — klasör ≠ namespace ≠ assembly, CS0118, duvarın somut faturaları | `.asmdef` dosyaları, `BoardAdapter` alias bloğu, `AttackResolver`, `MoveOutcome` |
| [03](konular/03-tahta-sahipligi.md) | **Tahta sahipliği** — ikinci yazarın nasıl doğmadığı, `readonly`'nin korumadığı şey | `Battle.board`, `Battle.Board`, `UnitGrid`, `BoardAdapter.battle` |
| [04](konular/04-karar-sirasi.md) | **Ret sırası** — aynı tıklamanın neden farklı sebeple reddedildiği, geri dönülemez nokta | `BattleActions` ADIM zinciri, `MoveAction.Execute`, `AttackAction.Execute` |
| [05](konular/05-yasam-dongusu.md) | **Yaşam döngüsü** — Alive→Downed→Dead, yasak geçişler, yapı ikizinin üç eksiği | `UnitLifecycle`, `StructureLifecycle`, `TargetingRules`, `Battle.RemoveReadyForCleanup` |
| [06](konular/06-sonuc-enumlari.md) | **Sonuç enum'ları** — sıfırıncı değer neden RET, bir asmdef bir enum değerini nasıl yasaklar | `AttackOutcome`, `MoveOutcome`, `PlacementOutcome`, `ReviveOutcome` |
| [07](konular/07-tiklamadan-eyleme.md) | **Tıklamadan eyleme** — üç giriş sorgusu, jest durum makinesi, niyet vs geçerlilik | `PointerGesture`, `BoardAdapter.Update`/`HandleClick`, `BattleActions` |
| [08](konular/08-motor-cagri-dongusu.md) | **Motorun çağrı döngüsü** — `Awake` neden bir `event` değil, sıranın sahipleri, `IEnumerator`'un ikinci hayatı | `BoardAdapter.Awake`/`OnEnable`/`OnDisable`/`Update`, `UnitView.Awake`, `PointerGesture.Reset`, `EditorSettings.asset` |
| [09](konular/09-kararlarin-cevrilmesi.md) | **Kararların çevrilmesi** — hangi ölçüm dünkü cevabı yanlış yaptı, on iki maddenin hikâyesi | `IBoardMode`, `UnitOrderBook`, `UnitLifecycle.OnHealthDepleted`, `AttackProfile.CooldownSeconds`, `UnitGrid.TryGetPosition` |
| [10](konular/10-geri-alinan-kararlar.md) | **Geri alınan kararlar** — kaydın yedi alanı, arşiv, henüz çevrilmemiş ikilik, reddedilen `Factory` | `UnitBlueprint.CreateCombatant`, `BoardAdapter.NewCombatant`/`SpawnUnit`, `UnitBlueprintAsset`, `arsiv/` |

## İlgili

- Yorum bloklarının kendi kuralı: `unity-expert-code-quality` skill'i →
  `references/unity-csharp-quality-flow.archive` → *Comment Diagram Debt*
- İşlenmiş örnekler ve ret kapısı: `references/comment-diagram-debt-patterns.archive`
- Ödünç alınan tipler ve dil özellikleri: [dil/README.md](dil/README.md)
- Tip başına ayna belgeler: [kod/README.md](kod/README.md)
- Bu turun devir paketi: [../comment-diagram-debt/HANDOFF.md](../comment-diagram-debt/HANDOFF.md)
- Dördüncü ve **ayrı** bir ağaç — "ben nerede duruyorum": [../ogrenme/README.md](../ogrenme/README.md).
  Bu üç ağaç *kodun ne öğrettiğini* anlatır; o ağaç *okuyanın neyi kapattığını ve
  neyin borç kaldığını* tutar. Kapısı: `python Tools/check-curriculum-coverage.py`
