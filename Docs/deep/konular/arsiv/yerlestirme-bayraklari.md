# `isPlacingStructure` ve `ghostIsCarried` — yerleştirme kipinin iki bayrağı

| | |
|---|---|
| Sahibi olduğu tip | `BoardAdapter` |
| Yerine geçen | `IBoardMode`, `BoardModeMachine`, `StructurePlacementMode` |
| Hikâyesi | [../09-kararlarin-cevrilmesi.md](../09-kararlarin-cevrilmesi.md) madde 1 |
| Kaydı | [../10-geri-alinan-kararlar.md](../10-geri-alinan-kararlar.md) bölüm 3 |
| Kaynağı | `30a022d` işlemesindeki `BoardAdapter` |

---

## Alan bildirimleri, birebir

```csharp
        // Yerleştirme kipinde miyiz. Bir OYUN durumudur, çeviri durumu değil —
        // yani bu alan da selectedUnit gibi rol başlığındaki "hafıza: var"
        // satırının altına düşer.
        private bool isPlacingStructure;
```

```csharp
        // Hayalet fareye YAPIŞTI mı. İki giriş şeklini ayıran tek alan budur:
        // sürükle-bırak hiç yapıştırmaz, tıkla-bırak ilk bırakışta yapıştırır.
        // Sayaç değil bool, çünkü ayrım "kaçıncı tıklama" değil — hayalet fareye
        // bağlı mı bağlı değil mi. → BoardAdapter.md#ghostiscarried
        private bool ghostIsCarried;
```

## Dağıldıkları yerler

İki alan tek dosyada dokuz ayrı noktada okunuyor ya da yazılıyordu. Yazma ve
okuma noktalarının kendisi:

```csharp
            if (isPlacingStructure)
```

```csharp
            isPlacingStructure = true;
```

```csharp
            ghostIsCarried = false;
```

```csharp
                    if (ghostIsCarried)
```

```csharp
                        ghostIsCarried = true;
```

```csharp
            isPlacingStructure = false;
            ghostIsCarried = false;
```

```csharp
            if (isPlacingStructure || !TryReadPointerCell(out _, out _, out int x, out int y)
```

## Aynı dosyanın kendi tarifi

Bayrakların kip olduğunu, kipe geçmeden önceki yorum zaten söylüyordu:

```csharp
        // kipi kareler arasında yaşayan İKİ alan gerektirdi (isPlacingStructure,
        // ghostIsCarried), diriltme ise tek bir tıklamanın anlamını değiştirmekle
```

## Neden burada duruyor

İki alan da ağaçtan tamamen kalktı. Bugünkü `BoardAdapter` ne `isPlacingStructure`
ne `ghostIsCarried` adında bir alan taşıyor, ve ikisinin cevapladığı sorular
`StructurePlacementMode` tipinin içine geçti.
