# `BoardAdapter.NewCombatant` — Inspector'dan doğan savaşçı

| | |
|---|---|
| Sahibi olduğu tip | `BoardAdapter` |
| Yerine geçecek olan | `UnitBlueprint.CreateCombatant` |
| Kaydı | [../10-geri-alinan-kararlar.md](../10-geri-alinan-kararlar.md) bölüm 4 |
| Kaynağı | Bugünkü çalışma ağacı |

**HENÜZ SİLİNMEDİ.** Bu dosya ötekilerden farklı: buradaki kod bugün hâlâ
çalışıyor. Arşive şimdiden kondu çünkü silineceği belli ve silindiği gün ağaçta
hiçbir izi kalmayacak.

Bir savaşçının canını kaç yazar yazıyor sorusunun cevabı bugün **iki**, ve
derleyici bunu bildirmiyor.

---

## Üye, birebir

```csharp
        /// <summary>
        /// Inspector'daki sayılardan bir savaşçı kurar.
        /// </summary>
        private Combatant NewCombatant(Team team)
        {
            // YAŞAM DÖNGÜSÜ PENCERELERİ BİLEREK SERİLEŞTİRİLMEDİ, oysa can ve
            // hasar serileştirildi: "kaç saniye düşük kalır" sorusunun ZATEN bir
            // sahibi var (UnitLifecycle'daki iki sabit) ve sahnedeki bir alan o
            // sabiti sessizce ezerdi. Canın ve hasarın ilk sahibi ise burası.
            // → BoardAdapter.md#newcombatantteam-team
            return new Combatant(
                new Health(maxHealth),
                new UnitLifecycle(),
                new AttackProfile(damage, attackRange, attackCooldownSeconds),
                team);
        }
```

## Onu besleyen dört alan, birebir

```csharp
        // BİRİM SAYILARI NEREDEN GELİYOR: buradan, düz [SerializeField] olarak.
        // Seçenekleri ayıran şey "kim okur" değil DOSYAYI KİM ÜRETİR — const bir
        // derleme turu ister, .asset ise koddan DOĞMAYAN bir dosyadır ve
        // atanmadığı gün sahneyi bozar; sahne alanı ise zaten var olan bir
        // bileşene yazılır. KAPSAM: sahibi başka yerde olan sayı serileştirilmez
        // (karşı örnek NewCombatant'taki yaşam döngüsü pencereleri).
        // → BoardAdapter.md#maxhealth-damage-attackrange
        [Header("Unit stats - applied to every spawned unit")]
        [Tooltip("Starting and maximum health of each spawned unit.")]
        [SerializeField, Min(1)] private int maxHealth = 30;

        [Tooltip("Raw damage of a single hit, before any resistance.")]
        [SerializeField, Min(0)] private int damage = 10;

        [Tooltip("How many cells away a unit can strike. Must be at least 1.")]
        [SerializeField, Min(1)] private int attackRange = 1;

        // BEKLEME SÜRESİ OLMADAN VURUŞ SINIRSIZDIR: sıfır geçildiğinde savaşçı
        // aynı hedefe kare başına vurabilir ve oyuncunun hızlı tıklaması hasarı
        // yığar. Sayı burada, çünkü bu yolla doğan birimlerin (demo doğuşu)
        // canı ve hasarı da burada; üretim yolundan gelenlerin sahibi ise kendi
        // varlık dosyası. → Combatant.AttackCooldownRemaining
        [Tooltip("Seconds a spawned unit waits between two strikes. 0 means no limit at all.")]
        [SerializeField, Min(0f)] private float attackCooldownSeconds = 1f;
```

Son yorum ikiliği **kendisi bildiriyor**: bu yolla doğan birimlerin sahibi
burası, üretim yolundan gelenlerin sahibi kendi varlık dosyası.

## Tek çağıranı

```csharp
        /// <summary>
        /// Savaşa bir birim katar ve ekrandaki karşılığını doğurur.
        /// </summary>
        private void SpawnUnit(string name, Team team, int x, int y)
        {
            // GÖVDE ARTIK PlaceUnit'TE ve bu üye yalnızca kimliği ile savaşçısını
            // kuruyor. Ayrımın sebebi sahiplik: buradaki iki `new` çağrısı
            // Inspector'daki sayılara bağlı ve o sayıların sahibi bu dosya,
            // oysa sürükleme yolu savaşçısını üretim tanımından getiriyor.
            // → BoardAdapter.md#spawnunitstring-name-team-team-int-x-int-y
            PlaceUnit(new Unit(name), NewCombatant(team), x, y);
        }
```

## `SpawnUnit`'in tek çağıranı — kodda `GEÇİCİ` işaretli

```csharp
            // GEÇİCİ: iki demo birim. İKİSİ de gerekli ve bu bir tercih değil —
            // saldırı zincirinin kapandığını göstermek için birbirine
            // tıklanabilen İKİ birim şart, ve TargetingRules dost ateşini
            // reddettiği için tarafları farklı olmak zorunda.
            if (unitPrefab != null)
            {
                SpawnUnit("Vanguard", Team.Player, 1, 2);
                SpawnUnit("Raider", Team.Enemy, 1, 3);
            }
```

Zincir üç halka: iki demo birim → `SpawnUnit` → `NewCombatant`. Zincirin
tamamının hizmet ettiği şey oyun değil, bir gösterim.

## Karşı taraf — bugün de duran ikinci yazar

```csharp
        public Combatant CreateCombatant(Team team)
        {
            // Health ve UnitLifecycle HER ÇAĞRIDA YENİ — burası tanımın örnek
            // durumu taşımamasının uygulandığı tek satırdır. AttackProfile ise
            // alandan geçiyor, kopyalanmıyor: değişmez olduğu için paylaşılması
            // güvenli, ve paylaşıldığı için iddia gerçek.
            return new Combatant(
                new Health(MaxHealth),
                new UnitLifecycle(),
                AttackProfile,
                team);
        }
```

`UnitBlueprint` tipinin bu üyesinin üstündeki yorum, ikiliği **adıyla**
bildiriyor ve `BoardAdapter`'ın `NewCombatant` üyesini o kopyanın tek örneği
olarak gösteriyor.

## Bugünkü ayrışma

`BoardAdapter`'ın `attackCooldownSeconds` alanının varsayılanı `1f`.
`Unit_Piyade.asset` dosyasında aynı sayı `0.8`.

İki yoldan doğan iki piyade farklı hızda vuruyor. Hiçbir test kırmızı değil,
hiçbir derleme uyarısı yok.

## Neden burada duruyor

Bu kod silindiği gün, ikiliğin var olduğuna dair tek kanıt da silinecek. Geri
almanın öğrettiği şey yeni şekilde değil, iki şekil arasındaki cümlede — ve o
cümlenin okunabilmesi için eski şeklin okunabilir kalması gerekiyor.
