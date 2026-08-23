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
| `Tools/check-curriculum-coverage.py` | Kavram borç defterinin kapısı — sahipsiz, uydurma ya da bayat satır bırakmaz |
| `Tools/check-doc-code-refs.py` | Belgelerdeki `Dosya.cs:SATIR` atıflarını çözer; kendi körlüğünü her koşumda basar |
| `Tools/check-navigation-loops.py` | Her ara durağın bir dönüşü var mı — gidiş var dönüş yok durumunu yakalar |
| `Docs/comment-diagram-debt/` | Yorum-diyagram borcu: devir paketi, envanter, JSON state |
| `Docs/deep/kod/` | Ayna belgeler — her tipin gerekçeleri ([indeks](deep/kod/README.md)) |
| `Docs/deep/konular/` | Mekanizma anlatıları — çok dosyayı kat eden konular |
| `Docs/deep/dil/` | Ödünç alınan BCL tipleri ve C# özellikleri ([indeks](deep/dil/README.md)) |
| `Docs/ogrenme/` | Öğrenme defteri — **nereden başlanır**, hangi desen kodda var, hangisi yok, hangi kavram borçlu ([indeks](ogrenme/README.md)) |
| `Docs/ogrenme/00-okuma-sirasi.md` | ██ Buradan başla ██ — 15 adım, 5 oturum; dosya numaraları sıra değil kimliktir |
| `Docs/ogrenme/` | Öğrenme defteri — hangi desen kodda var, hangisi yok, hangi kavram borçlu ([indeks](ogrenme/README.md)) |
| `Tools/check-curriculum-coverage.py` | Kavram borç defterinin kapısı — sahipsiz, uydurma ya da bayat satır bırakmaz |

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
