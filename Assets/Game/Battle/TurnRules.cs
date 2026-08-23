using System;
using GridStrategy.Combat;

namespace GridStrategy.Battle
{
    // ═══ ROL: KURAL (Policy) ═════════════════════════════════════════
    // kimlik : yok — static; iki çağrıyı ayıracak bir şey yoktur
    // hafıza : yok — aynı taraflar ve aynı sayı her zaman aynı cevabı verir
    // Unity  : gerekmez
    // karar  : İZİN söyler; ne hareket ettirir, ne saldırır, ne sıra devreder
    /// <summary>
    /// "Bu birim ŞU AN eyleyebilir mi?" sorusunun tek sahibi.
    ///
    /// <see cref="TurnState"/> sıranın KİMDE olduğunu tutar; burası o bilgiden
    /// ne ÇIKTIĞINI söyler. Ayrım <see cref="Team"/> ile
    /// <see cref="TargetingRules"/> arasındaki ayrımın aynısıdır: taraf bir
    /// değerdir, tarafın ne yapabildiği bir kuraldır, ve ikisi aynı yerde
    /// yaşamaz.
    ///
    /// Neyi BİLMEZ: hangi birimin sorulduğunu (yalnızca tarafını görür), hedefin
    /// kim olduğunu (<see cref="TargetingRules"/>'ın işi), hareketin geçerli
    /// olup olmadığını (<see cref="GridStrategy.Core.MoveAction"/>'ın işi).
    /// Bu kural onların ÖNÜNDE durur: sıra sende değilse geçerli bir hamlenin
    /// bile sırası değildir.
    ///
    /// GEREKÇELER: Docs/deep/kod/Battle/TurnRules.md
    /// </summary>
    public static class TurnRules
    {
        // Sabit BURADA, TurnState'te değil: bir eşik kuralın METNİNİN parçasıdır.
        // Duruma taşınsaydı kural sayıyı okuyamaz, dördüncü bir parametre olarak
        // isterdi ve onu yanlış dolduran çağıran birime fazladan eylem hakkı
        // verirdi. → TurnRules.md#maxactionsperturn
        /// <summary>
        /// Bir birimin kendi turunda kaç kez eyleyebileceği.
        ///
        /// Bugün BİR: turun bir anlamı olması için "sıra sende" cümlesinin bir
        /// sınırı olması gerekir, yoksa sırası gelen birim aynı turda yirmi kez
        /// vurur ve sıra sistemi yalnızca sırayı geciktirmiş olur.
        /// </summary>
        public const int MaxActionsPerTurn = 1;

        // İKİ AŞIRI YÜKLEME, İKİ AYRI SORU — ve bu sürüm yalnız SIRAYI soruyor.
        // Tek imzaya inseydi, elinde sayaç olmayan çağıran (düşman birimlerini
        // soluklaştıran arayüz) uydurma bir sıfır geçerdi ve sorulmamış bir
        // sorunun cevabını almış olurdu. → TurnRules.md#canactteam-team
        /// <summary>
        /// Sıra bu tarafta mı? Bütçeye BAKMAZ — yalnızca sıranın kimde olduğunu
        /// sorar.
        ///
        /// <see cref="Team.None"/> hiçbir sırada eyleyemez, ve gerekçe
        /// <see cref="TargetingRules"/>'ınkiyle aynı yerden gelir: tarafsız olan
        /// taraf tutmaz, duvar vurmaz. Burada ayrıca bir sessiz hatayı da
        /// kapatır — <c>default(Team)</c> tarafsızdır, yani takımı atanmayı
        /// unutulmuş bir birim HER ŞEYİ değil HİÇBİR ŞEYİ yapabilir olur ve
        /// eksiklik ilk denemede görülür.
        /// </summary>
        // DERİN ANLATIM: Docs/deep/konular/04-karar-sirasi.md
        public static bool CanAct(Team unitTeam, Team currentTurn)
        {
            // Tarafsızlık yalnızca BİR yanda sınanıyor. İkinci bir
            // `currentTurn == Team.None` kontrolü gereksiz: gerçek bir takım
            // zaten None'a eşit değildir, None-None hâli ise bu satırda kapandı.
            // İkinci kez yazmak, tek kuralı iki kural gibi gösterirdi.
            if (unitTeam == Team.None)
            {
                return false;
            }

            return unitTeam == currentTurn;
        }

        // İMZA DEĞER ALIYOR, NESNE DEĞİL: bir TurnState geçmek KURAL'ı VARLIK'a
        // bağlar, yani kuralı sınamak için geçerli dizilimli bir savaş kurmayı
        // ve taraf matrisini tek tablodan dokuz metoda dağıtmayı gerektirirdi.
        // Üç parametre üç ayrı sahipten geliyor ve imza bunu görünür kılıyor.
        // → TurnRules.md#canactteam-team-int
        /// <summary>
        /// Sıra bu tarafta mı VE birimin bu turda eylem hakkı kaldı mı?
        ///
        /// Sıra kuralı burada KOPYALANMIYOR, iki parametreli sürüme soruluyor.
        /// Kopyalansaydı "tarafsız eyleyemez" kararı iki yerde yaşardı ve biri
        /// değiştiğinde diğeri sessizce eskirdi.
        /// </summary>
        /// <param name="actionsUsedThisTurn">
        /// Birimin bu turda şimdiye kadar kaç eylem harcadığı. Bu sayıyı bu tip
        /// TUTMAZ — <c>static</c> ve hafızasızdır; sayacın neden burada da
        /// <see cref="TurnState"/>'te de yaşamadığı TurnState'in kendi belgesinde
        /// yazılı.
        /// </param>
        public static bool CanAct(Team unitTeam, Team currentTurn, int actionsUsedThisTurn)
        {
            // Negatif harcama bir oyun sonucu değil, bir çağıran hatasıdır:
            // "eksi bir kez hareket etmiş" diye bir durum yok. Sessizce sıfır
            // saymak, sayacı bozuk olan çağırana sonsuz eylem hakkı verirdi.
            if (actionsUsedThisTurn < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(actionsUsedThisTurn),
                    actionsUsedThisTurn,
                    "Spent action count cannot be negative.");
            }

            if (!CanAct(unitTeam, currentTurn))
            {
                return false;
            }

            // Alternatif: tür başına ayrı sınır (MaxMovesPerTurn ile
            // MaxAttacksPerTurn) ve türü taşıyan bir enum. Seçilmedi: bugün cevap
            // eylem TÜRÜNE bağlı değil — tetiği klasik "önce yürü, sonra vur"
            // turu — ve ayrı sayaçlar her çağırana ölçülmeyen bir argüman
            // doldurtur.
            return actionsUsedThisTurn < MaxActionsPerTurn;
        }
    }
}
