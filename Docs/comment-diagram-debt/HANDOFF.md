# Comment Diagram Debt — Handoff Overview

> Bu dosya insan-için-tek-bakışta haritadır. Makine-okunur gerçek kaynak
> `handoff.json`; dosya×blok envanteri `INVENTORY.md`. Üçü çelişirse
> `handoff.json` kazanır.

## Neden bu iş var

Bu repoda her alanın/kararın üstünde bir REDDEDILEN / KIRILAN / KAZANIRDI /
TEK CUMLE bloğu var. Bloklar doğru ama çoğu **düzyazı**: bir konumsal ilişkiyi
(ad ağacı, sahiplik, arama sırası, referans paylaşımı) tarif ediyor ve okuyucudan
o şekli zihninde yeniden kurmasını istiyor. Kurma başarısız olunca yorum
**soru olarak geri geliyor** — kusur okuyucuda değil, yorumdadır.

Kural artık skill katmanında yazılı:
`~/.claude/commands/unity-expert-code-quality/references/unity-csharp-quality-flow.archive`
→ **"Comment Diagram Debt"** bölümü.

## Altın standart — bu iki blok referanstır, kopyalanacak şablon budur

```
Assets/Game/Unity/BoardAdapter.cs:13-101     CS0118 / ad çözümleme  (KAPALI)
Assets/Game/Battle/Battle.cs:46-133        tahta sahipliği        (KAPALI)
```

Her worker işe başlamadan ÖNCE bu ikisini okur. Şablon şu üç zorunlu bölümdür:

```
┌─ HARİTA ────────── ilişkiyi ÇİZ, duruş/ayrışma noktasını İŞARETLE
├─ KAPSAM ────────── kural genel mi özel mi + AYNI dosyadan KARŞI ÖRNEK
└─ İŞ BÖLÜMÜ ─────── iki mekanizma varsa hangisi neyi kapatıyor + silinirse ne kırılır
```

Artı: yakındaki hangi modifier'ın **korumadığını** söyle (`readonly` nesneyi
dondurmaz), ve garantinin **nerede bittiğini** yaz (assembly duvarı).

## Sistem haritası — lane'ler disjoint dosya kümesidir

```
                        Assets/Game/                         Assets/Tests/
                             │                                     │
   ┌──────────┬──────────────┼──────────────┬───────────┐          │
   │          │              │              │           │          │
 LANE A    LANE B1        LANE B2        LANE C      LANE D     LANE E
 Battle/   Core/Combat/   Core/Combat/   Core/*.cs   Unity/     EditMode/**
           saldırı ekseni  yaşam ekseni              görsel
   │          │              │              │           │          │
  35 blok    27 blok        34 blok       30 blok    19 blok    ~20 blok
  P1         P1             P1            P1         P0         P2

  P0 = borç/çizim oranı en kötü   P1 = gövde   P2 = kuyruk
```

## Öncelik haritası — ne önce, neden

```
P0  Game/Unity/UnitView.cs           22 tetikleyici / 0 çizim  ◄ en kötü oran
    Game/Battle/BattleActions.cs     10 tetikleyici / 0 çizim, 12 REDDEDILEN
    Game/Unity/BoardAdapter.cs       kalan 14 blok (2'si kapandı)

P1  Game/Core/UnitGrid.cs             5 tetikleyici / 0 çizim
    Game/Core/Combat/Combatant.cs     5 tetikleyici / 0 çizim, 9 REDDEDILEN
    Game/Core/Combat/TargetingRules.cs 4 / 0, 7 REDDEDILEN
    Game/Core/MoveAction.cs           4 / 0, 7 REDDEDILEN
    Game/Battle/Battle.cs             kalan 11 blok (1'i kapandı)
    ... tam liste INVENTORY.md

P2  Assets/Tests/EditMode/**          ~20 blok — test yorumları en son
```

## Kırmızı çizgiler (bir worker bunları çiğnerse lane reddedilir)

```
✗ Yorum OLMAYAN tek bir satıra bile dokunmak       → davranış değişikliği
✗ Satır sonlarını LF'e çevirmek                     → repo CRLF kullanıyor
✗ REDDEDILEN bloğu OLMAYAN sıradan yorumu şişirmek → maliyet freni
✗ Karşı örnek uydurmak                              → aynı dosyadan gerçek olacak
✓ Şekil gerekçeden uzunsa → reference dosyasına taşı, koda tek satır link bırak
```

## Okuma sırası (worker için, bu sırayla)

1. `~/.claude/commands/unity-expert-code-quality/references/unity-csharp-quality-flow.archive`
   → yalnız "Comment Diagram Debt" bölümü
2. `~/.claude/commands/unity-expert-code-quality/references/comment-diagram-debt-patterns.archive`
   → iki işlenmiş örnek, bölüm anatomisi, **reject-gate** (bir bloğu ne zaman reddet)
3. `Assets/Game/Unity/BoardAdapter.cs` satır 9-97 (altın standart #1)
4. `Assets/Game/Battle/Battle.cs` satır 43-126 (altın standart #2)
5. `INVENTORY.md` → yalnız kendi lane satırları
6. Kendi lane dosyaları

## Doğrulama

Değişiklik yorum-only olduğu için derleme davranışı değişmez. Yine de her lane
sonunda:

```powershell
Tools/run-editmode-tests.ps1
```

ve her dosya için satır sonu kontrolü (CRLF sayısı > 0, LF-only == 0).
