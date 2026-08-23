# CountryBall-Strategy — Belge Haritası

Bu projeyle ilgili kararlar iki yerde yaşıyor. Bu dosya, Unity projesini açan
birinin diğerini bulabilmesi için var.

## Bu projenin içinde

| Yol | Ne |
|---|---|
| `Assets/Game/Core/Combat/` | Savaş çekirdeği — Unity'siz (`noEngineReferences: true`) |
| `Assets/Tests/EditMode/Combat/` | Davranış testleri + tahsis (allocation) testleri |
| `Tools/run-editmode-tests.ps1` | Testleri Editor'e dokunmadan komut satırından koşar |
| `Tools/.test-results/` | Koşu çıktıları (XML + Unity log) |
| `Docs/comment-diagram-debt/` | Yorum-diyagram borcu: devir paketi, envanter, JSON state |
| `Docs/deep/kod/` | Ayna belgeler — her tipin gerekçeleri ([indeks](deep/kod/README.md)) |
| `Docs/deep/konular/` | Mekanizma anlatıları — çok dosyayı kat eden konular |

## Kararların ve öğrenme kaydının yeri

Kök: `../../unity-game-dev-journey/parallel_sessions/S06_COMBAT_CORE/`

| Dosya | İçerik |
|---|---|
| `LANE_LOG.md` | Tarih sıralı karar günlüğü — ne yapıldı, neden, hangi kanıtla |
| `LEARNED_CONCEPTS.md` | Kapanmış kavramlar (öğrenenin kendi ifadesiyle) |
| `ABSOLUTE_F_REFACTOR_READY.md` | "Ayrı dosya mı, aynı dosya mı" karar kuralı |
| `PERF_TEST_RESEARCH.md` | Performans ölçümü araştırması ve paket kararı |
| `DEATH_LIFECYCLE_DESIGN.md` | Alive / Downed / Dead yaşam döngüsü tasarımı |
| `ROLES_AND_STRUCTURES_ROADMAP.md` | Roller, barakalar, hız, denge; aşama sırası |

## Test yazım kuralları

Kalıcı kurallar projede değil, skill katmanında:
`C:\Users\green\.claude\commands\testing-unity\`

Oradaki `references/performance-measurement.archive`, bu projede **ölçülmüş**
olguları taşır — örneğin `GC.GetAllocatedBytesForCurrentThread()`'in Unity
2021.3.45f2 Mono'da 0 döndürdüğü.
