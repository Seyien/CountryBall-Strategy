# Devir — otomatik karşılık verme ve arayüz işleri

**Tarih:** 2026-08-29 · **Önceki oturum:** `assets-b5`
**Bu belge ne değil:** bir ders. Bir sonraki oturumun nereden devam edeceğini yazar.

---

## Sistem haritası — bir bakışta

```
                 ┌──────────────────────────────────────────────┐
                 │  GridStrategy.Core / .Combat / .Battle       │
                 │  noEngineReferences: true  (motor GİREMEZ)   │
                 │                                              │
                 │  Unit ── kimlik (savaşçı VE yapı)            │
                 │  Combatant · Structure   (ikisi de sealed)   │
                 │  ApproachRules ◄── YAZILDI, ÇAĞIRANI YOK     │
                 │  VictoryRules → BattleOutcome                │
                 └───────────────────┬──────────────────────────┘
                                     │  duvar (.asmdef)
                 ┌───────────────────▼──────────────────────────┐
                 │  GridStrategy.Unity                          │
                 │                                              │
                 │  BoardAdapter ── kompozisyon kökü, 4000+ satır│
                 │    ├── UnitOrderBook  ◄─ IUnitOrder          │
                 │    │     ├── AttackOrder    (DOKUNULMAZ)     │
                 │    │     ├── ReviveOrder                     │
                 │    │     └── ??? karşılık emri  ◄── EKSİK    │
                 │    ├── BoardModeMachine ◄─ IBoardMode        │
                 │    ├── BoardCameraRig + BoardFraming         │
                 │    ├── WorldBackdrop                         │
                 │    ├── UnitView · StructureView              │
                 │    └── BattleOverView                        │
                 └──────────────────────────────────────────────┘
```

## Açık iş haritası — öncelik sırasıyla

```
P0  otomatik karşılık verme          LANE-J koşuyor (2026-08-29)
    └─ kural VAR, belge VAR, DAVRANIŞ YOK

P1  ekran gözlemi                    yalnız OPERATÖR kapatabilir
    └─ çerçeve · zemin · yıkım · pano · yeniden başlat

P2  bilgi diyaloğu                   brief hazır, başlamadı
    └─ çarpı · Esc · perde · tek modal · tahta boyutundan bağımsız

P3  boş hâl etiketi ekrana ulaşıyor mu   doğrulanmadı
    └─ kodda düzeltildi, operatör hâlâ eskisini görüyor

P4  24 açıklama metni                 operatörden bekleniyor
```

---

## Bu oturumda ÖLÇÜLMÜŞ doğrular

Aşağıdakiler yeniden ölçülmeden doğru kabul edilebilir.

| Olgu | Ölçü |
|---|---|
| EditMode takımı | **781/781 yeşil**, 0 kırmızı — `Tools/.test-results/EditMode-results.xml` |
| Tahta kipi | `TurnMode.FreeForAll` → tur kapısı karşılık vermeyi ENGELLEMEZ |
| Tahta ölçüsü | `width=5 height=10`, hücre başına bir birim → N için sert tavan 50 |
| `Combatant` ve `Structure` | ikisi de `sealed`, üretilen somut tip sayısı 1 |
| `abstract` / `virtual` | `Assets/Game/` altında **sıfır** |
| Birim türü sayısı | 10 `.asset`, hepsi aynı `m_Script` GUID'i → tür farkı VERİ, tip değil |
| Üreten yapılar | Kışla 1 · Fabrika 2 · Karargâh 2 · Taret 0 · Hisar 0 |
| Ses | `AudioSource` / `AudioClip` / `PlayOneShot` → **sıfır** |
| Jobs / Burst | `BurstCompile` / `IJob` / `NativeArray` → **sıfır** |
| Unity sürümü | `6000.5.7f1` — Entities sürüm duvarı ARTIK YOK |
| Sahne Build Settings'te | `SampleScene` 0. sırada, etkin → yeniden başlat düğmesi güvenli |

## Kapalı kararlar — yeni kanıt olmadan yeniden açma

- **Factory reddedildi.** Fabrikanın seçeceği tip yok. Yeniden açma koşulu: `Combatant`'tan türeyen ikinci bir tip, ya da `Unit_*.asset` dosyalarının ikiden fazla `m_Script` GUID'i taşıması. → `Docs/deep/konular/10-geri-alinan-kararlar.md` §5
- **Singleton zorunlu değil, kolaylık.** Ölçü: 1 sahne, müziğin kod maliyeti 0 satır, `DontDestroyOnLoad` üretim kodunda 0 kez. → `Docs/ogrenme/16-ses-ve-muzik.md`
- **ECS / Jobs / Burst ertelendi.** Engel hız değil VERİ ŞEKLİ: çekirdek tamamen `sealed class` ve `Dictionary<Unit, X>`; Burst HPC# derler. Yeniden açma: bir SAYI değil bir TÜR değişimi — birimler kendi kendine yürüyüp saldırdığı gün, hedef cihazda tek bir `Update` sahibinin ~2 ms'yi aşması, N > 200 hareketli varlıkla. → `Docs/ogrenme/09-ecs-dots-yol-haritasi.md`
- **Tahta dolunca üretim DURAKLAR.** Operatör seçti. `StructureProduction.Tick` zaten sıfırda bekliyor, o tipte sıfır satır değişecek.
- **Tasma:** karşılık emri yalnız üç durumda düşer — hedef öldü · tahtadan gitti · yol yok. Menzil dışı olmak emri DÜŞÜRMEZ.
- **`AttackOrder`'ın menzil dışı iptali korunur.** Operatörün yazılı kararı, kodda `██ OPERATÖRÜN İSTEDİĞİ KESİLME TAM OLARAK BU DAL ██` diye işaretli. Kovalayan davranış AYRI tipe gider.

---

## Açık sınırlar

### P0 — otomatik karşılık verme
`Assets/Game/Core/ApproachRules.cs` yazıldı ve **hiç çağrılmıyor**. `IUnitOrderHost` dört üye taşıyor, hareket üyesi yok. `Orders/` altında yalnız `AttackOrder` ve `ReviveOrder` var.
Gereken: hareket üyesi · kovalayan karşılık emri (birim) · yerinde vuran karşılık emri (yapı) · `ReactToAttack`'in isabet dalında ikisi arasında seçim.
Tasarım hazır: `Docs/deep/konular/11-karsilik-verme-ve-menzil.md`. Yeniden tasarlanmayacak, uygulanacak.

### P1 — ekran gözlemi (hiçbir makine kapatamaz)
781 testin **hiçbiri** ekranda ne göründüğünü söylemiyor. Sıra önemli:
1. Unity'yi aç, **menüye dokunma**, doğrudan Play — çerçeve ve zemin tahtaya uyuyor mu, kum gitti mi
2. Bir taretin canını bitir — kararıp soluyor mu
3. Düşmanı bitir — pano açılıyor, tahta donuyor, düğme çalışıyor mu
4. **Sonra** `CountryBall ▸ Sahneyi Kur (her şey)` — bayat önizleme tazelenir, `check-board-framing` yeşile döner

1. adım menüsüz olmalı: düzeltmenin ölçüsü menü çalıştırılmadan çerçevenin doğru olmasıdır. Menü önce çalıştırılırsa semptom gizlenir, hata gizlenmez.

### P2 — bilgi diyaloğu
Veri hazır: birim tanımında 7 alan, yapı tanımında 10. Eksik olan tek şey `[TextArea]` açıklama alanı.
Sözleşme: X düğmesi sağ üstte · Esc kapatır · perdeye tıklamak kapatır · perde arkadaki ışın izlemeyi keser · kapatan tıklama tahtaya GEÇMEZ · aynı anda tek modal (`BattleOverPanel` ile çakışmayacak).
Statik boyut yasağı: diyalog `boardRect`, `width`, `height`, `BoardSizing` okumayacak. `produces[]` uzunluğu tasarımcıya açık → `VerticalLayoutGroup` + `ContentSizeFitter` + `ScrollRect`, ve yazılı tavan.
Kopyalanacak kardeş: `SceneSetupTool.EnsureBattleOverPanel` ve onun `StretchFull` yardımcısı.

### P3 — boş hâl etiketi
`ProductionPanelView` iki cümleyi taşıyor. Operatör hâlâ eskisini görüyor. Ekrana ulaşıp ulaşmadığı doğrulanmadı.

### P4 — 24 açıklama metni
10 birim, 10+ yapı. Kurgu operatörün. Ölçülen sayılardan türetilmiş taslak önerilebilir, uydurma kurgu YAZILMAZ.

---

## Bu oturumun ÖĞRENİLMİŞ hatası — tekrarlanmasın

**Kural ve belge yazıldı, davranış bağlanmadı ve bu iki tur boyunca fark edilmedi.**
`ApproachRules` ile `Docs/deep/konular/11-...md` indi, operatör "otomatik saldırı yok" diyene kadar kimse çağıranın sıfır olduğunu ölçmedi.

Kural: **bir özellik "yapıldı" sayılmaz, ta ki onu çağıran üretim kodu ölçülene kadar.** Ölçüsü tek komut:
```
grep -rn "<YeniTip>" Assets/Game --include="*.cs" | grep -v "<YeniTip>.cs"
```
Boş dönüyorsa özellik yok, yalnız dosyası var.

### İkinci öğrenilmiş hata — araç artıkları
Üç `.cs` ve bir `.meta` dosyasının sonuna `</content>` / `</invoke>` yazıldı ve `GridStrategy.Unity` derlenmedi. `.meta` olanı daha tehlikeliydi: bozuk meta GUID'i düşürüp sahne referanslarını sessizce koparabilirdi.
Her şerit bitirmeden önce:
```
grep -rn "</content>\|</invoke>" Assets/ Tools/
```

### Üçüncü — bayat kilit
`Temp/UnityLockfile` sahipsiz kalabiliyor (Unity çökerse). Testler saatlerce "BLOCKED" sanıldı. Doğru teşhis:
```powershell
Get-Process -Name "Unity" -ErrorAction SilentlyContinue
```
Süreç yoksa kilit bayattır ve kaldırılabilir. Kontrolü koşum betiğine gömmek gerekir.

---

## Şerit disiplini — bu oturumda ölçülerek öğrenildi

- `Assets/Game/Unity/BoardAdapter.cs` **tek yazarlıdır**. 229 KB ve neredeyse her iş ona dokunuyor. İki şerit aynı anda girerse birleşme sessizce bozulur.
- `Assets/Editor/SceneSetupTool.cs` de tek yazarlıdır.
- Belge şeritleri ile kod şeritleri ayrık dosyalarda paralel koşabilir.
- Kod satırları kaydığında `Docs/` içindeki satır çapaları kırılır. Bugün **348 kırık çapa** var ve bu paralel bir oturumun `Assets/` düzenlemesinden geliyor. Yeni belge yazan şerit `.cs:NNN` çapası EKLEMEZ; tip ve üye adıyla atıf yapar.
- Her şerit kendi scratchpad alt klasörünü kullanır.

## Kapılar

```
python Tools/check-comment-contract.py      python Tools/check-doc-links.py
python Tools/check-comment-language.py      python Tools/check-absence-debt.py
python Tools/check-cited-names.py           python Tools/check-navigation-loops.py
python Tools/check-scale-ceilings.py        python Tools/check-cross-file-refs.py
python Tools/check-curriculum-coverage.py   python Tools/check-asset-inventory.py   (yavaş)
python Tools/check-board-framing.py         ← bugün 1 ihlal: sahnedeki bayat önizleme
python Tools/check-doc-code-refs.py         ← bugün 348 ihlal: eski borç, artırma
```

Testler: `Tools/run-editmode-tests.ps1` (Unity kapalıyken). Taban **781/781**.

## Belge ağacı

| Soru | Belge |
|---|---|
| Hangi deseni ne zaman kullanırım | `Docs/ogrenme/13-desen-secim-rehberi.md` |
| Bugün kodda hangi desenler duruyor | `Docs/ogrenme/01-koda-gomulu-desenler.md` |
| Görsel eksikler, büyücü hattı | `Docs/ogrenme/14-gorsel-sozluk-ve-eksikler.md` |
| Kavram soruları (katlı cevaplarla) | `Docs/ogrenme/15-kavram-sorulari.md` |
| Ses, Singleton hükmü | `Docs/ogrenme/16-ses-ve-muzik.md` |
| ECS / Jobs / Burst eşiği | `Docs/ogrenme/09-ecs-dots-yol-haritasi.md` |
| Geri alınan kararlar + eski kod | `Docs/deep/konular/10-geri-alinan-kararlar.md` + `arsiv/` |
| Karşılık verme tasarımı, neden Strategy | `Docs/deep/konular/11-karsilik-verme-ve-menzil.md` |

---

## Sonraki oturumun EYLEM SÖZLEŞMESİ

**Varsayılan (operatörden girdi yoksa):** `LANE-J`'nin çıktısını doğrula — `ApproachRules`'un üretim kodunda çağıranı var mı, `Orders/` altında üçüncü uygulama doğdu mu, testler 781'in altına düştü mü. Sonra P2'yi (bilgi diyaloğu) tek yazarlı bir şeritte aç.

**Eğer operatör Play gözlemini verdiyse:** gözlemi P1'in dört adımına göre eşle, kırık olanı ölç ve onar. Gözlem "görmediklerim" alanını taşımıyorsa gözlem EKSİKTİR, tamamlanması istenir.

**Eğer `LANE-J` çıktı vermediyse:** briefi `Docs/deep/konular/11-karsilik-verme-ve-menzil.md` üzerinden yeniden kur ve tek yazarlı bir şeritte koştur. Tasarımı yeniden tasarlama.

**HARD blocker (durulur, operatör çağrılır):** `git push`, kuvvet birleştirme, üretim dağıtımı. Bunlar bu oturumda hiç yapılmadı ve yapılmayacak.
