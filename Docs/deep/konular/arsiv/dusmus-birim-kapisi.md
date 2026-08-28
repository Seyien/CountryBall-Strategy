# `UnitLifecycle.OnHealthDepleted` — genişletilip GERİ ALINAN kapı

| | |
|---|---|
| Sahibi olduğu tip | `UnitLifecycle` |
| Yerine geçen | Hiçbir şey — kapı eski hâline döndü |
| Hikâyesi | [../09-kararlarin-cevrilmesi.md](../09-kararlarin-cevrilmesi.md) madde 3 |
| Kaydı | [../10-geri-alinan-kararlar.md](../10-geri-alinan-kararlar.md) bölüm 3 |
| Kaynağı | `b1f1286` işlemesinin ebeveyni |

Ötekilerden farkı: burada **denenen** şekil ağaçta hiç yaşamadı. Değişiklik
yazıldı, üç test kırmızıya döndü, ve değişiklik geri alındı. Bugün ağaçta duran
kod, aşağıdaki eski koddur.

Bu yüzden arşivin taşıdığı şey silinen kod değil, **silinen kararın kanıtı**:
kapının kendisi ve onu koruyan üç test.

---

## Kapı, birebir

```csharp
        public void OnHealthDepleted()
        {
            // KURTARMA PENCERESİNİ ATLAYAN KESTİRME, DURUMU DA SİLER. Kapı bilerek
            // yalnız Alive'dan geçirir: düşmüş birime tekrar vurmak onu ANINDA
            // öldürmemeli — "işini bitirme" ayrı bir kuraldır (düşme canı) ve o
            // kural henüz yazılmadı; buraya sessizce koymak yerini de yok ederdi.
            // → UnitLifecycle.md#onhealthdepleted
            if (State != UnitState.Alive)
            {
                return;
            }
```

Yorumun kendisi, eksik kuralı **adıyla** söylüyor. Denenen değişiklik tam olarak
o yorumun yasakladığı şeydi.

## Karar kaydı olan üç test

Birincisi, `UnitLifecycleTests` içinde, adıyla:

```csharp
        // Downed birime tekrar vurmak onu ANINDA öldürmemeli. "İşini bitirme"
        // ayrı bir kural (düşme canı) ve henüz yazılmadı; bu test o boşluğun
        // sessizce kapanmasını engelliyor.
        [Test]
        public void Downed_HealthDepletedAgain_DoesNotSkipTheWindow()
        {
            var lifecycle = NewLifecycle();
            lifecycle.OnHealthDepleted();
            lifecycle.Tick(3f);

            lifecycle.OnHealthDepleted();

            Assert.That(lifecycle.State, Is.EqualTo(UnitState.Downed));
            Assert.That(lifecycle.RemainingSeconds, Is.EqualTo(7f), "Geri sayım sıfırlanmamalı.");
        }
```

Kalan ikisi aynı cümleyi üç ayrı test dosyasında tekrar ediyor, ve o tekrar
kararın ne kadar yayıldığının ölçüsü. `AttackRulesTests`, `MovementRulesTests` ve
`TargetingRulesTests` dosyalarının üçünde de aynı gerekçe metni duruyor:

```csharp
                "a downed unit remains a valid target; finishing it off is part of the design");
```

```csharp
                "a downed unit is still a target; finishing it off is part of the design");
```

## Projenin aynı anda söylediği iki şey

Bitirme tasarımın parçası. Bitirme anlık olmamalı.

Denenen değişiklik ikincisini çiğniyordu, ve üç test tam olarak onu yakaladı.

## Neden burada duruyor

Geri alınan bir değişikliğin hiçbir izi kalmaz: ne commit'i vardır, ne `git`
geçmişinde bir satırı. Kalan tek kanıt, değişikliği reddeden kapı ile onu
koruyan testlerdir.
