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
    /// Bu tipin var olma sebebi <see cref="GridStrategy.Core.MoveAction"/>'ın
    /// kendi başlığında zaten yazılıydı: <i>"Neyi BİLMEZ: sıranın kimde
    /// olduğunu, birimin bu turda daha önce hareket edip etmediğini."</i> O iki
    /// soru sahipsizdi, ve sahipsiz kaldığı sürece cevapları her çağıranın
    /// içinde ayrı ayrı doğacaktı — arayüzde bir <c>if</c>, yapay zekâda bir
    /// başkası, ve ikisi ilk denge değişikliğinde ayrışacaktı.
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
    /// </summary>
    public static class TurnRules
    {
        // REDDEDILEN - TurnRules.cs:57 yerine (sabit buradan silinir, sayı
        //              savaşın durumuna taşınır):
        //     // TurnState içinde:
        //     public int MaxActionsPerTurn { get; }
        // KIRILAN  : kural, kuralı sahiplenmeyi bırakır.
        //            CanAct sayıyı okuyamaz -> DÖRDÜNCÜ parametre olarak ister ->
        //            onu yanlış dolduran çağıran birime fazladan eylem hakkı verir
        //            derleyici: hiçbir şey der  .  test: CanAct_BudgetPlane_OnItsOwnTurn
        //            tablosu [TestCase] ile yazılamaz olur — özniteliğe nesne konmaz
        // KAZANIRDI: sınır savaştan savaşa DEĞİŞSEYDİ — "yıldırım modunda herkes
        //            iki kez oynar" gibi bir mod düğmesi ya da zorluk seviyesine
        //            bağlı bir ayar; o gün sayı gerçekten durumdur ve buradaki
        //            sabit her mod için yeniden derleme gerektirirdi.
        // TEK CUMLE: Bir sabit kuralın metninin parçasıdır; onu duruma taşımak
        //            kuralı her çağıranın yeniden beyan ettiği bir sayıya çevirir.
        /// <summary>
        /// Bir birimin kendi turunda kaç kez eyleyebileceği.
        ///
        /// Bugün BİR: turun bir anlamı olması için "sıra sende" cümlesinin bir
        /// sınırı olması gerekir, yoksa sırası gelen birim aynı turda yirmi kez
        /// vurur ve sıra sistemi yalnızca sırayı geciktirmiş olur.
        /// </summary>
        public const int MaxActionsPerTurn = 1;

        // REDDEDILEN - TurnRules.cs:85 yerine (bu aşırı yükleme hiç doğmaz,
        //              yalnızca üç parametreli sürüm kalır ve taraf sorusunu
        //              soran uydurma bir sıfır geçer):
        //     TurnRules.CanAct(unitTeam, currentTurn, actionsUsedThisTurn: 0)
        // KIRILAN  : uydurma sıfır, çağrıyı bir yalana çevirir.
        //            arayüz düşman birimlerini soluklaştırırken elinde ne birim
        //            ne sayaç vardır -> sıfır geçer -> cevap "eyleyebilir" olur,
        //            oysa sorulan şey eylem değil SIRAydı
        //            derleyici: hiçbir şey der  .  test: CanAct_TeamPlane matrisi
        //            ölçmediği bir sütun taşır ve taraf kararı bütçenin altında kalır
        // KAZANIRDI: sıra sorusu HİÇBİR zaman birimsiz sorulmayacaksa — o gün
        //            iki aşırı yükleme hangisinin gerçek kural olduğunu
        //            belirsizleştirir ve biri sessizce eskir.
        // TEK CUMLE: İki farklı soru varsa iki imza vardır; birini diğerinin
        //            varsayılanıyla sormak, sorulmayan soruyu cevaplamış olur.
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

        // REDDEDILEN - TurnRules.cs:130 yerine (değerler yerine savaşın kendisi
        //              geçilir):
        //     public static bool CanAct(Team unitTeam, TurnState turn,
        //                               int actionsUsedThisTurn)
        // KIRILAN  : bağımlılık yönü tersine döner — KURAL, VARLIK'ı tanır.
        //            kuralı sınamak için geçerli dizilimli bir savaş kurmak gerekir
        //            -> öznitelik argümanı sabit olmak zorunda -> TurnState örneği
        //            [TestCase] içine konulamaz, taraf matrisi dokuz metoda dağılır
        //            derleyici: hiçbir şey der  .  test: tablolar var olamaz
        // KAZANIRDI: cevap savaşın BİRDEN ÇOK alanına bağlı olsaydı — sıra, tur
        //            numarası, kalan süre ve bütçe birlikte; o gün dört ayrı
        //            değeri elden geçirmek tören olurdu.
        // TEK CUMLE: Bir kurala nesne geçmek onu o nesnenin ömrüne bağlar; değer
        //            geçmek kuralı tek başına sorulabilir bırakır.
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
        /// <see cref="TurnState"/>'te de yaşamadığı TurnState.cs'te yazılı.
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
