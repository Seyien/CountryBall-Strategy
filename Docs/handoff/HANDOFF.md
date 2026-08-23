# Devir — gerekçe kalitesi zinciri

> **Tarih:** 2026-08-22 · **Makine-okunur eş:** [`STATE.json`](STATE.json)
> İkisi çelişirse `STATE.json` kazanır; bu dosya tek bakışta harita.

## Bir bakışta: bugün ne kuruldu

```
KOD                                    BELGE
Assets/Game 7.578 → 5.344 satır        Docs/deep/
%69,6 → %56,9 yorum                    ├── kod/      33 tip, 220 üye  (ayna)
                                       ├── konular/   7 mekanizma
442/442 test geçiyor                   └── dil/       5 kavram (BCL + C#)
5 kapı sıfır ihlal                                   ~17.400 satır
```

Sayılar `HEAD` (b0897e3) ile bugünkü çalışma ağacı arasında, kapsam
`Assets/Game/**/*.cs`, payda **tüm satırlar** (boş satırlar dahil).
Önceki devirdeki `13.686 → 5.377 / %82 → %56` çifti bu paydayla doğrulanmadı.

```
                    ÜÇ EKSEN, DİK
   kod/       TİP ekseni       "bu üye neden böyle"
   konular/   MEKANİZMA ekseni "bu akış nasıl çalışıyor"
   dil/       KAVRAM ekseni    "bu ödünç tip ne vaat ediyor"
```

## Zincirin öğrettiği tek şey

Her tur aynı desenle ilerledi ve her turda aynı sınıf hata bulundu:

```
operatör bir yorumu anlamadı
        │
        ▼
  ██ metin kusurlu, okuyucu değil ██
        │
        ├─► kusuru adlandır          → kural
        ├─► işlenmiş örnek yaz       → pattern dosyası
        ├─► makine kapısı kur        → check-*.py
        └─► geriye dönük tara        → worker wave
```

Dört tur, dört kural, hepsi skill katmanında (`unity-expert-code-quality` →
`references/unity-csharp-quality-flow.archive`):

| Kural | Neyi yakalar | Bulduğu gerçek hata |
|---|---|---|
| **Comment Diagram Debt** | konumsal ilişki çizilmemiş | CS0118 ad ağacı iki turda anlaşılmadı |
| **Claim Needs a Measure** | iddia var, ölçü yok | **15 yanlış ölçü** — 3'ü dosyanın kendi içinde çelişki |
| **Borrowed Types Owe a Line** | BCL/dil özelliği hiç açıklanmamış | `nameof` 80 kez kullanılmış, tek satır yok |
| **Show the Shape You Are Discussing** | bahsedilen kod gösterilmiyor | "bir sonraki satır" — belgede o satır yok |

## ██ EN ACİL: bugünün tamamı commit edilmemiş ██

```
39 değişen/yeni dosya · son commit 05:22 (ayna geçişinden ÖNCE)

Bugün bu yüzden VERİ KAYBEDİLDİ: bir temizleme betiği 209 satır sildi
(40 silmesi gerekirken), git'te ara hâl yoktu, 5 dosyanın yedeği de yoktu.
Ayna belgeler olmasa geri getirilemezdi.
```

Sıradaki oturumun **ilk işi** bu olmalı. Commit operatör onayı ister.

## Açık işler — öncelik sırasıyla

```
P0  commit                    39 dosya, kurtarma kaynağı yok
P1  BattleActions.md:233      "HER ŞEYDEN ÖNCE" yanıltıcı + performans
                              sorusu ele alınmamış  (aşağıda ayrıntı)
P1  LANE E                    Assets/Tests/EditMode/** — 21 blok hiç işlenmedi
P2  çapa-işaretçi boşluğu     33 ayna belgenin 9'unda kodda karşılığı yok
P2  resolve-rejected-anchors  bloklar koddan çıktı, araç çoğu dosyada boşa çalışıyor
P3  kod soruları (5 adet)     aşağıda
```

### P1 — `BattleActions.md` `### Attack: TurnRules.CanAct`

Operatör şunu sordu ve belge cevaplamıyor:

> *"Neden `attackerCombatant` bulunduktan hemen sonra `CanAct` kontrol edilmiyor?
> Diğer işlemler gereksiz yere yapılıyor."*

Gözlem teknik olarak **doğru**: `RequireCell` → `Battle.TryGetPosition` tam tahta
taraması yapıyor (O(en×boy)) ve iki kez çağrılıyor. `CanAct` reddedecekse ikisi
de boşa gidiyor.

Cevap belgede yok. Yazılması gereken:

```
İSTİSNA KANALI      okuyucusu PROGRAMCI   "kodun bozuk"
                    RequireCombatant · RequireCell · negatif menzil
════════════════════ ██ ÇİZGİ ██ ════════════════════════════════
SONUÇ DEĞERİ        okuyucusu OYUNCU      "bu hamle olmadı"
                    CanAct · menzil · dost ateşi · dolu hücre

Sıra değiştirilseydi: sırası olmayan oyuncu SAVAŞTA OLMAYAN bir hedefe
saldırdığında RejectedActorCannotAct alırdı — "sıran değil" der, oysa
gerçek sebep çağıranın kodunun bozuk olması. Programcı hatası oyun
sonucu kılığına girer.

Testi: Move_NegativeRangeOutOfTurn_StillThrows (BattleActionsTests.cs:706)
       sıra dışı + geçersiz argüman → yine de FIRLATIR
```

Ayrıca başlıktaki *"HER ŞEYDEN ÖNCE"* düzeltilmeli: sıra kuralı **sonuç değeri
döndüren** her şeyden önce, ama istisna kapılarından sonra.

### P3 — kod soruları (hiçbirine dokunulmadı, hepsi operatör kararı)

```
BoardAdapter.cs      var olmayan API anılıyor: battle.RemoveStructure
Structure.cs         "Takım sonradan DEĞİŞMEZ" yorumu bir ifade AŞAĞI kaymış
                     ve "readonly" diyor — oysa get-only property
MoveOutcome.cs       bir not "üç sebep" diyor, gerçekte iki
Unity.EditModeTests  asmdef GridStrategy.Battle'ı referans etmiyor
BoardAdapterTests    YOK — üç niyet dalı (SEÇ/BIRAK/SALDIR) testsiz
TurnRulesTests:107   const inline'lanması → IL'de Assert.That(1, Is.EqualTo(1))
PickTerrainSprite    negatif koordinatta patlar (bugün çağıranı güvenli)
```

## Kapılar — beşi de sıfır ihlal, ama dördü onarıldı

```
Tools/check-comment-contract.py   blok uzunluğu, numaralı liste, TEK CUMLE
Tools/check-comment-language.py   tam diakritik Türkçe          ██ 2 kez onarıldı
Tools/check-cited-names.py        kodda karşılığı olmayan ad
Tools/check-cross-file-refs.py    çapraz dosya satır atfı
Tools/check-doc-links.py          çapa + göreli yol             ██ 2 kez onarıldı
```

**Bu oturumun en pahalı dersi:** bir kapı **dört kez** yanlışlıkla "temiz" dedi.

```
① maskeleme "//" işaretini de yuttu     → kapı KÖRLEŞTİ
② göreli anahtar, mutlak sorgu           → 50 SAHTE POZİTİF
③ çapa slug'ı ASCII olmak ZORUNDA        → doğru yazılmışı cezalandırdı
④ yalnız satır başı işaretçi             → satır-içi çapalar GÖRÜNMEZ
```

Kural: **bir kapı yazarken önce onun yanlışlıkla "temiz" diyebileceği yolu bul,
o yola bir öz-sınama koy, negatif testle doğrula.** `check-doc-links.py` artık
öz-sınama taşıyor (her belge kendi ilk başlığını çözebilmeli, yoksa "KAPI BOZUK"
deyip çıkar).

## Çalışma disiplini — worker wave'lerinden çıkan

```
her lane KENDİ scratchpad alt dizinini kullanır
   (altı worker aynı apply.py'ye yazdı, üzerine yazıldı — kanıt: apply_2..6.py)

değişiklikler HEDEFLİ olur, toplu regex/silme betiği YASAK
   (209 satırlık kayıp tam olarak bundan doğdu)

worker "dokunmadığım sayıyı" raporlar
   (SHAPE turu: 215 pasaj tarandı, 9 değişti, 206 dokunulmadı — %4)

her ölçü YAZILMADAN ÖNCE koda karşı doğrulanır
   (kaydedilmiş metne güvenmek yetmez; satır numaraları 6 satır kaymıştı)
```

## Doğrulama komutları

```bash
python Tools/check-comment-contract.py
python Tools/check-comment-language.py
python Tools/check-cited-names.py
python Tools/check-cross-file-refs.py
python Tools/check-doc-links.py
Tools/run-editmode-tests.ps1          # 442/442 bekleniyor
```

## Yeni projede aynı düzeni kurmak için

`unity-expert-code-quality` skill'i →
`references/rationale-layout-bootstrap.archive` (dizin şeması, göç sırası) ve
`scripts/` (beş kapının taşınabilir kopyası).
