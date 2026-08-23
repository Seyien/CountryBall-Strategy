# Comment Diagram Debt — Envanter

Ölçüm tarihi: 2026-08-21. Sayımlar `grep -c` ile alındı, kesindir.

- **REDDEDILEN** = dosyadaki reddedilen-alternatif bloğu sayısı
- **tetik** = konumsal ifade sayısı ("iki sahip", "bir üst", "içten dışa",
  "aynı nesne", "iki yerde", "referans", "önce…sonra" …)
- **çizim** = dosyada hâlihazırda bulunan ASCII kutu/ağaç karakteri sayısı
- **borç** = tetik > 0 ve çizim == 0 olan dosyalar; oran kötüleştikçe öncelik artar

## LANE A — `Assets/Game/Battle/` (35 blok)

| Dosya | REDDEDILEN | tetik | çizim | Not |
|---|---|---|---|---|
| `Battle.cs` | 12 | 25 | 8 | 1 blok KAPALI (`board`, satır 43-126) — şablon örneği |
| `BattleActions.cs` | 12 | 10 | 0 | **P0** — akış sahibi, hiç çizim yok |
| `TurnState.cs` | 5 | 1 | 0 | |
| `TurnRules.cs` | 3 | 3 | 0 | |
| `PlacementOutcome.cs` | 3 | 0 | 0 | sonuç tipi; sadece REDDEDILEN denetimi |
| `ReviveOutcome.cs` | 1 | 0 | 0 | |

## LANE B1 — `Assets/Game/Core/Combat/` saldırı ekseni (27 blok)

| Dosya | REDDEDILEN | tetik | çizim |
|---|---|---|---|
| `AttackAction.cs` | 7 | 4 | 0 |
| `TargetingRules.cs` | 7 | 4 | 0 |
| `AttackOutcome.cs` | 4 | 0 | 0 |
| `AttackRules.cs` | 3 | 1 | 0 |
| `AttackProfile.cs` | 2 | 1 | 0 |
| `DamageRules.cs` | 2 | 0 | 0 |
| `AttackResolver.cs` | 1 | 0 | 0 |
| `HealingRules.cs` | 1 | 0 | 0 |

## LANE B2 — `Assets/Game/Core/Combat/` yaşam ekseni (34 blok)

| Dosya | REDDEDILEN | tetik | çizim |
|---|---|---|---|
| `Combatant.cs` | 9 | 5 | 0 |
| `UnitLifecycle.cs` | 5 | 0 | 0 |
| `Structure.cs` | 4 | 2 | 0 |
| `StructureState.cs` | 4 | 0 | 0 |
| `Health.cs` | 3 | 1 | 0 |
| `StructureLifecycle.cs` | 3 | 0 | 0 |
| `MovementRules.cs` | 3 | 1 | 0 |
| `ReviveRules.cs` | 1 | 1 | 0 |
| `UnitState.cs` | 1 | 0 | 0 |
| `Team.cs` | 1 | 0 | 0 |

## LANE C — `Assets/Game/Core/` (Combat hariç, 30 blok)

| Dosya | REDDEDILEN | tetik | çizim |
|---|---|---|---|
| `PointerGesture.cs` | 10 | 1 | 0 |
| `MoveAction.cs` | 7 | 4 | 0 |
| `UnitGrid.cs` | 4 | 5 | 0 |
| `MoveOutcome.cs` | 3 | 0 | 0 |
| `MoveProfile.cs` | 3 | 3 | 0 |
| `GridDistance.cs` | 2 | 0 | 0 |
| `Unit.cs` | 1 | 1 | 0 |

## LANE D — `Assets/Game/Unity/` (19 blok) — **P0**

| Dosya | REDDEDILEN | tetik | çizim | Not |
|---|---|---|---|---|
| `BoardAdapter.cs` | 15 | 18 | 6 | 1 blok KAPALI (CS0118, satır 9-97) — şablon örneği |
| `UnitView.cs` | 5 | 22 | 0 | **en kötü borç/çizim oranı** — MonoBehaviour ↔ Core sınırı |

## LANE E — `Assets/Tests/EditMode/` (~20 blok) — P2

| Dosya | REDDEDILEN | tetik |
|---|---|---|
| `Combat/CombatantTests.cs` | 3 | 0 |
| `Combat/DamageRulesAllocationTests.cs` | 3 | 1 |
| `Battle/TurnRulesTests.cs` | 2 | 1 |
| `Combat/TargetingRulesTests.cs` | 2 | 0 |
| `Combat/UnitLifecycleTests.cs` | 2 | 0 |
| `Battle/TurnStateTests.cs` | 1 | 0 |
| `Combat/AttackResolverTests.cs` | 1 | 0 |
| `Combat/DamageRulesTests.cs` | 1 | 0 |
| `Combat/HealthTests.cs` | 1 | 0 |
| `Combat/MovementRulesTests.cs` | 1 | 1 |
| `Core/MoveActionTests.cs` | 1 | 4 |
| `Core/MoveProfileTests.cs` | 1 | 1 |
| `Core/PointerGestureTests.cs` | 1 | 5 |
| `Unity/UnitViewTests.cs` | 1 | 4 |

Çizim borcu olup REDDEDILEN taşımayan test dosyaları (yalnız tetik denetimi):
`Battle/BattleActionsTests.cs` (3), `Battle/BattleTests.cs` (1),
`Combat/AttackActionTests.cs` (2), `Core/UnitGridTests.cs` (1).

## Kapanmış lane'ler (bu zincirde)

| Dosya:satır | Konu | Tarih |
|---|---|---|
| `Assets/Game/Unity/BoardAdapter.cs:13-101` | CS0118 ad çözümleme sırası, alias yerleşimi, `global::` | 2026-08-21 |
| `Assets/Game/Battle/Battle.cs:46-133` | tahta sahipliği, referans paylaşımı, `readonly` yanılgısı | 2026-08-21 |

Bu ikisi **yeniden açılmaz**. Şablon olarak okunur, üstünde çalışılmaz.
