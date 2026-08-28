# Devir — üretim paneli, savaş kuralları ve tahta kusurları turu

> **Tarih:** 2026-08-28 · **Durum:** turun bütün P0/P1 kalemleri KAPALI
> Bu belge devir promptunun yanındaki ayrıntı kaynağıdır.
> Bir önceki hâli iki açık bug'ı anlatıyordu; ikisi de bu turda kapandı.

## Bir bakışta

```
                          OYUNCU
                            │
        ┌───────────────────┼───────────────────┐
        │                   │                   │
   SOL PALET          ALT ÜRETİM PANELİ      TAHTA
   StructurePaletteView  ProductionPanelView  BoardAdapter
        │                   │                   │
        │ StructureBlueprintAsset               │ BoardModeMachine
        │   ├─ Icon            ✅               │   ├─ IdleBoardMode
        │   ├─ BoardSizeInCells ✅              │   ├─ StructurePlacementMode
        │   └─ ProducedAssets   ✅ (indeks eşi) │   └─ PendingStrikeMode
        │                   │                   │
        └─────────┬─────────┘                   │
                  ▼                             ▼
          ProductionDirector ──IPlacementBoard──┘
                  │
                  ▼
            BoardSizing.LocalScaleFor(sprite, cells, cellSize)
            (önizleme hayaleti ve kurulan bina AYNI hesabı okur)
```

## Bu turda kapananlar

### Simge zinciri (P0 ×2)
`StructureBlueprintAsset.ProducedAssets` eklendi ve `Definition.Produces` ile
**aynı geçişte** kuruluyor — null gözler ikisinde de aynı yerde atlandığı için
indeks kayması yapısal olarak imkânsız. Zincir varlıktan panele
(`ProductionDirector.ProducedIcon`), oradan tahtaya (`SetPlacementVisual`) ve
tahtadan da birimin gövdesine (`UnitView.SetBodySprite`) kadar uzatıldı.

### Yapı tıklama çökmeleri (P0 ×3)
Yapı seçiliyken boş hücreye tıklama, düşmana tıklama ve düşmüş dosta tıklama
üçü de `RequireCombatant` üzerinden `ArgumentException` fırlatıyordu. Üçü de
kapandı; tanınmayan kimlik için istisna KORUNDU — o gerçekten programcı hatası.

### Motorda yapı saldırısı (P0)
`Structure.CanAttack` vardı ama `AttackAction`'da karşılığı yoktu.
`Execute(Structure, Combatant)` ve `Execute(Structure, Structure)` eklendi;
`BattleActions.Attack` dörtlü dağıtım yapıyor.

### Sıra kuralı (P1)
`TurnMode { Alternating, FreeForAll }`. Varsayılan `Alternating` kaldı, tahta
`FreeForAll` ile kuruluyor. Saldırının bedeli artık sıra değil BEKLEME SÜRESİ:
`AttackProfile.CooldownSeconds` (eşik) + `Combatant`/`Structure` sayacı (örnek).

### Düşme canı (P1)
`TargetingRules` düşmüş birimi zaten hedef sayıyordu ama vuruş hiçbir şeyi
değiştirmiyor, yalnız beklemeyi yakıyordu. `Combatant` artık düşmüş bedene
ayrı bir havuzdan (`maxHealth / DownedHealthDivisor`, taban 1) hasar yazıyor;
havuz boşalınca `UnitLifecycle.OnDownedHealthDepleted` bedeni bitiriyor ve
`AttackOutcome.HitAndFinished` dönüyor. **Anlık ölüm bilerek reddedildi** —
bu dosyanın kendi yorumu onu zaten reddetmişti.

### Diriltme (P1)
`LeftShift` zorunluluğu kalktı (takma ad olarak duruyor): düşmüş bir DOSTA
tıklamak diriltir, çünkü kendi takımına saldıramaz ve dolu hücreye yürüyemezsin.
Saldırıdaki "yaklaş sonra vur" zincirinin ikizi diriltmeye de verildi ve
**ikinci bir kip açılmadan** — emrin cinsini `PendingStrikeMode` taşıyor.

### Boyut sahipliği (P1)
Boyut dört otoriteye dağılmıştı (ölü `Structure.prefab`, sahne alanı, üç kod
sabiti, can barının `1/parentScale` düzeltmesi) ve tür kimliğinin sahibi olan
varlık dosyasında HİÇ yoktu. Bugün: `BoardSizeInCells` blueprint'te,
`localScale` `BoardSizing` ile sprite'ın kendi ölçülerinden hesaplanıyor.
`Structure.prefab` silindi (guid'ine sıfır atıf).

### Tarama bulguları
Salt okunur bir denetim 12 kusur buldu; hepsi kapandı. Öne çıkanlar: hayaletin
sahnede yazılı sprite'ının kalıcı silinmesi (bu turun kendi regresyonu), ölen
seçili birimin durum şeridinde asılı kalması, kulelerin binalara ateş etmemesi,
arayüz tıklamasının tahtaya sızması, imleç rengi için her karede tam A*, ve
"BATTLE OVER"ın binaları saymadan tekrar tekrar basılması.

### State pattern
`Assets/Game/Unity/Modes/` — `IBoardMode`, `IBoardModeHost` (+ yetenekle
daraltılmış `IPlacementModeHost` / `IPendingStrikeHost`), `BoardModeMachine`,
üç kip. Referanstaki `IBuildingState`'in üç-üye disiplini alındı, `Map.Instance`
singleton'ı REDDEDİLDİ. Kipler `MonoBehaviour` değil — EditMode'da sınanıyorlar.

## Kapalı gerçekler (yeniden açma)

- Unity **6000.5.7f1**, C# 9. `record struct` YOK (C# 10). `record class` ve
  serileştirilmeyen `readonly struct` (`GridStep`) çalışıyor.
- Unity serileştiricisi alanları yansımayla yazar ve **kurucuyu çağırmaz** —
  Inspector'dan doldurulan bir `readonly struct` değişmezliği hakkında yalan
  söyler. Değer nesnesi deyimi bu projede `sealed record` sınıf.
- Editor açıkken `Tools/run-editmode-tests.ps1` `exit 2` verir. Çözüm:
  `Assets/Packages/ProjectSettings/Tools` bir scratch dizine kopyalanıp koşum
  orada yapılır (3 MB, tam takım).
- `total` tek başına yalan söyler; assembly başına sayı XML'den okunmalı.
- Bütün tahta sanatı 16x16, içe aktarma 16 PPU, `Grid.cellSize` 1 →
  **bir sprite tam bir hücre**.

## Açık borçlar (kod değil, çevre)

| borç | ölçü | sahibi |
|---|---|---|
| `check-doc-code-refs` | HEAD'de 258, şimdi ~350 | `Docs/` içindeki `Dosya.cs:SATIR` çapaları kaydı; tazeleme ayrı bir tur |
| `Assets/Game/Prefabs/PlacementGhost.prefab` | guid'ine sıfır kod atıfı | ikinci ölü varlık; hayalet sahnede nesne olarak yaşıyor |
| `SampleScene.unity` `structureScale: 1.6` | ölü YAML | araç sahneyi yeniden kaydettiğinde düşer |
| sprite'sız yapı uyarısı | EditMode log'unda ~11 satır | testi kırmıyor, Console'da görünüyor |
| operatör adımı | `CountryBall → Sahneyi Kur (her şey)` + Ctrl+S | yeni blueprint'ler, kenar halkası, 0. katman zemin ve hayalet sprite'ı ancak bu çalıştırılınca sahneye iner |

## Sıradaki iş

`BoardAdapter` hâlâ ~3500 satır. State pattern en kötü parçayı çıkardı ama
`IPlacementModeHost` (7 üye) ve `IPendingStrikeHost` (9 üye) **kalan
bağımlılığın fotoğrafı**. God object bölündükçe daralmaları gerekir;
daralmazlarsa pattern kozmetik kalmış demektir — bir sonraki turun ölçütü bu.
