# `BattleActions.Attack` — saldırının sıra kapısı

| | |
|---|---|
| Sahibi olduğu tip | `BattleActions` |
| Yerine geçen | `AttackProfile.CooldownSeconds` |
| Hikâyesi | [../09-kararlarin-cevrilmesi.md](../09-kararlarin-cevrilmesi.md) madde 4 |
| Kaydı | [../10-geri-alinan-kararlar.md](../10-geri-alinan-kararlar.md) bölüm 3 |
| Kaynağı | `b1f1286` işlemesinin ebeveyni |

Kapı kaldırıldı ve **saldırının tek bedeli oydu**. Ölçülen sonuç: fareye ne
kadar hızlı basılırsa o kadar hasar.

---

## Kapı, birebir

```csharp
            // SIRA KURALI HER ŞEYDEN ÖNCE SORULUYOR — hedefin uygunluğundan da,
            // menzilden de önce. Aşağıya, AttackAction.Execute'un ALTINA
            // alınsaydı ret geldiğinde hasar çoktan inmiş olurdu ve "reddedildi"
            // bir kural değil bir metin olurdu. Takım bilgisi SAVAŞÇIDAN geliyor,
            // birimden değil: Unit tarafı bilmez.
            // → BattleActions.md#attack-turnrulescanact
            if (!TurnRules.CanAct(attackerCombatant.Team, battle.Turn.Current))
            {
                return AttackOutcome.RejectedActorCannotAct;
            }
```

## Kapının ikinci yarısı — koşulsuz devir

```csharp
            // SIRA BURADA DEVREDİLİR — ve bu satır olmadan oyun KIRIKTI: kural
            // soruluyordu ama EndTurn üretimde hiç çağrılmıyordu. Liste BEYAZ,
            // kara değil: yarın eklenen bir ret değeri kara listede varsayılan
            // olarak sırayı YAKAR, beyaz listede yakmaz — hata en fazla "sıram
            // bitmedi" olur ve o yön geri alınabilir.
            // → BattleActions.md#attack-endturn
            bool attacked = outcome == AttackOutcome.Hit
                || outcome == AttackOutcome.HitAndDowned
                || outcome == AttackOutcome.HitAndDestroyed;

            if (attacked)
            {
                battle.Turn.EndTurn();
            }
```

## `AttackAction`'daki eşi

Aynı ret değeri alt katmanda da iki kez dönüyordu, ve o iki satır bugün de
duruyor. Kalkan şey `BattleActions` katmanındaki sıra sorusuydu, `AttackRules`
tarafındaki durum sorusu değil:

```csharp
            if (!AttackRules.CanAttack(attacker.State))
            {
                return AttackOutcome.RejectedActorCannotAct;
            }
```

## Bugün ne var

Devir hâlâ orada, ama artık koşullu. Beyaz listeye dördüncü bir değer katıldı ve
`spendsTurn` adında ikinci bir koşul doğdu:

```csharp
            bool attacked = outcome == AttackOutcome.Hit
                || outcome == AttackOutcome.HitAndDowned
                || outcome == AttackOutcome.HitAndFinished
                || outcome == AttackOutcome.HitAndDestroyed;

            if (attacked && spendsTurn)
            {
                battle.Turn.EndTurn();
            }
```

Bedelin yeni sahibi ise tanım tarafında:

```csharp
        public AttackProfile(int damage, int range, float cooldownSeconds = 0f)
```

Varsayılan sıfır olduğu için eski davranış birebir korunuyor.

## Neden burada duruyor

Kaldırılan kapı bir satır değil, bir **bedeldi**. Kapının metni geçmişte
okunabilir, ama neyi ödettiği ancak yanına konan yeni şekille birlikte
görünüyor.
