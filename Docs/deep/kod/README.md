# Ayna Belgeler — kod indeksi

Her üretim tipinin gerekçeleri burada. Koddaki her üyenin üstünde 2-5 satırlık
karar özeti ve `→ <Tip>.md#<çapa>` işaretçisi duruyor; ayrıntı bu ağaçta.

**Ayna kuralı:** `Assets/Game/X/Y.cs` → `Docs/deep/kod/X/Y.md`

**Nasıl gelinir:** `Ctrl+P` → tip adını yaz → hem `.cs` hem `.md` listelenir.
C# yorumundaki yol `Ctrl`+tıklanabilir değildir; işaretçi bu arama için var.

**Otorite:** kod kazanır. Belge çelişirse belge bayattır.

## Battle

| Tip | Üye | Kod | Belge |
|---|---|---|---|
| [Battle](Battle/Battle.md) | 17 | 547 satır | 1154 satır |
| [BattleActions](Battle/BattleActions.md) | 7 | 390 satır | 935 satır |
| [PlacementOutcome](Battle/PlacementOutcome.md) | 3 | 44 satır | 224 satır |
| [ReviveOutcome](Battle/ReviveOutcome.md) | 4 | 54 satır | 147 satır |
| [TurnRules](Battle/TurnRules.md) | 3 | 118 satır | 282 satır |
| [TurnState](Battle/TurnState.md) | 13 | 206 satır | 530 satır |

## Core/Combat

| Tip | Üye | Kod | Belge |
|---|---|---|---|
| [AttackAction](Core/Combat/AttackAction.md) | 3 | 171 satır | 564 satır |
| [AttackOutcome](Core/Combat/AttackOutcome.md) | 7 | 74 satır | 405 satır |
| [AttackProfile](Core/Combat/AttackProfile.md) | 4 | 72 satır | 219 satır |
| [AttackResolver](Core/Combat/AttackResolver.md) | 1 | 59 satır | 122 satır |
| [AttackRules](Core/Combat/AttackRules.md) | 2 | 50 satır | 266 satır |
| [Combatant](Core/Combat/Combatant.md) | 9 | 198 satır | 936 satır |
| [DamageRules](Core/Combat/DamageRules.md) | 2 | 53 satır | 176 satır |
| [HealingRules](Core/Combat/HealingRules.md) | 2 | 59 satır | 113 satır |
| [Health](Core/Combat/Health.md) | 6 | 80 satır | 302 satır |
| [MovementRules](Core/Combat/MovementRules.md) | 2 | 59 satır | 318 satır |
| [ReviveRules](Core/Combat/ReviveRules.md) | 2 | 54 satır | 157 satır |
| [Structure](Core/Combat/Structure.md) | 10 | 153 satır | 411 satır |
| [StructureLifecycle](Core/Combat/StructureLifecycle.md) | 8 | 154 satır | 430 satır |
| [StructureState](Core/Combat/StructureState.md) | 4 | 55 satır | 321 satır |
| [TargetingRules](Core/Combat/TargetingRules.md) | 8 | 180 satır | 581 satır |
| [Team](Core/Combat/Team.md) | 3 | 35 satır | 150 satır |
| [UnitLifecycle](Core/Combat/UnitLifecycle.md) | 9 | 200 satır | 565 satır |
| [UnitState](Core/Combat/UnitState.md) | 4 | 39 satır | 168 satır |

## Core

| Tip | Üye | Kod | Belge |
|---|---|---|---|
| [GridDistance](Core/GridDistance.md) | 2 | 51 satır | 204 satır |
| [MoveAction](Core/MoveAction.md) | 3 | 179 satır | 641 satır |
| [MoveOutcome](Core/MoveOutcome.md) | 5 | 67 satır | 346 satır |
| [MoveProfile](Core/MoveProfile.md) | 3 | 67 satır | 290 satır |
| [PointerGesture](Core/PointerGesture.md) | 11 | 297 satır | 939 satır |
| [Unit](Core/Unit.md) | 3 | 52 satır | 160 satır |
| [UnitGrid](Core/UnitGrid.md) | 7 | 170 satır | 490 satır |

## Unity

| Tip | Üye | Kod | Belge |
|---|---|---|---|
| [BoardAdapter](Unity/BoardAdapter.md) | 43 | 1056 satır | 1622 satır |
| [UnitView](Unity/UnitView.md) | 10 | 217 satır | 620 satır |

## Toplam

33 tip · 220 belgelenmiş üye · 5260 satır kod · 14788 satır belge

## İlgili

- Mekanizma anlatıları (çok dosyayı kat eden konular): [../konular/](../konular/)
- Kapı: `python Tools/check-doc-links.py` — her çapa ve göreli yol çözülmeli
