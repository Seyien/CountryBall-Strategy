using System;
using GridStrategy.Combat;

namespace GridStrategy.Battle
{
    // ═══ ROL: KURAL (Policy) ═════════════════════════════════════════
    // kimlik : yok — static; iki çağrıyı ayıracak bir şey yoktur
    // hafıza : yok — aynı iki cevap her zaman aynı kazananı verir; savaşın
    //          bittiğini HATIRLAYAN bir bayrak burada YOK, çünkü hatırlamak
    //          bir durumdur ve durumun sahibi kural değildir
    // Unity  : gerekmez — noEngineReferences: true
    // karar  : SONUCU söyler; ne birim siler, ne sıra devreder, ne ekrana yazar
    /// <summary>
    /// "Savaş bitti mi, bittiyse kim kazandı" sorusunun tek sahibi.
    ///
    /// <see cref="TurnRules"/> ile aynı ailedendir ve aynı sebeple burada
    /// yaşıyor: soru <c>GridStrategy.Combat</c>'ta sorulamaz, çünkü orada
    /// "kadro" diye bir kavram yok — kimin hangi tarafta olduğunu tek tek
    /// bilen tip <see cref="Battle"/>'dır ve onu tanıyan en alt katman burasıdır.
    ///
    /// Neyi BİLMEZ: kaç birim kaldığını, kimin öldüğünü, savaşın ne zaman
    /// bittiğini, sonucu kimin göstereceğini. Kadroyu da GEZMEZ — iki cevabı
    /// hazır alır (<see cref="Battle.HasUnitsLeft"/>), çünkü kadro üzerinde
    /// dönmek bir DURUM okumasıdır ve kuralın işi değildir.
    ///
    /// GEREKÇELER: AYNA BELGE HENÜZ YOK. Bu tipin gerekçe dosyası bu satırın
    /// altındaki yorumlarda duruyor; ayna ağacına eklenmesi borç olarak
    /// raporlandı. Var olmayan bir yola işaretçi YAZILMADI — çürük bir çapa,
    /// okuyanı tam bir güvenle boşluğa gönderir.
    /// </summary>
    public static class VictoryRules
    {
        // İMZA DEĞER ALIYOR, NESNE DEĞİL — ve bu karar TurnRules.CanAct'ten
        // devralınıyor, orada ölçülmüş hâliyle: bir Battle geçmek kuralı
        // varlığa bağlar, yani "iki taraf da tükendi" hâlini sınamak için
        // gerçekten iki tarafı tüketmiş bir savaş kurmayı gerektirirdi.
        // İki bool, dört hâlin dördünü de tek satırda kurdurur.
        //
        // İKİ PARAMETRE AYNI TİPTE ve yerlerini karıştıran bir çağıran
        // KAZANANI TERS okur; derleyici bunu göremez. Kelepçe bu dosyada
        // değil çağırma yerinde duruyor: BoardAdapter argümanları
        // battle.HasUnitsLeft(Team.Player) ve battle.HasUnitsLeft(Team.Enemy)
        // olarak yazıyor, yani takım adı çağrının kendisinde okunuyor.
        /// <summary>
        /// En küçük zafer koşulu: bir tarafın bütün savaşçıları kalıcı ölüyse
        /// öteki taraf kazanır.
        /// </summary>
        /// <returns>
        /// Kazanan taraf; kazanan YOKSA <see cref="Team.None"/>. Sıfırıncı
        /// değerin burada "kazanan yok" demesi tesadüf değil, Team.cs'te yazılı
        /// olan kararın devamı: atanmayı unutulan bir taraf sessizce kazanmış
        /// sayılmamalı.
        /// </returns>
        public static Team Winner(bool playerHasUnitsLeft, bool enemyHasUnitsLeft)
        {
            // TEK KARŞILAŞTIRMA, İKİ AYRI OLGU — ve birleşmeleri bir kısayol
            // değil, cevabın kendisi: "ikisi de ayakta" savaşın SÜRDÜĞÜNÜ,
            // "ikisi de tükendi" karşılıklı yok oluşu anlatır ve çağıranın ikisi
            // için de yapacağı iş aynıdır, çünkü ilan edilecek bir kazanan yok.
            // Ayrı bir Draw değeri yazılmadı: bugün onu bir ötekinden farklı
            // gösterecek tek bir satır yok, ve eklendiği gün bu enum'a değil
            // yeni bir sonuç tipine ait olur.
            if (playerHasUnitsLeft == enemyHasUnitsLeft)
            {
                return Team.None;
            }

            // Kazanan, kadrosu KALAN taraftır. Tersini yazmak — "kaybeden
            // tükenen taraftır" — aynı cümle gibi görünür ama üç takımlı bir
            // oyunda ayrışır; bugün ikisi eşit olduğu için kısa olanı seçildi
            // ve ayrışacağı gün buraya bir üçüncü soru gelir.
            return playerHasUnitsLeft ? Team.Player : Team.Enemy;
        }

        /// <summary>
        /// Savaşın kendisine sorar: iki takımdan biri oyundan tamamen çıktıysa
        /// öteki kazanmıştır.
        /// </summary>
        // İKİ BOOL YERİNE NESNE ALAN AŞIRI YÜKLEME, ve sebebi çağrı yerinde
        // ölçüldü: iki bool'un SIRASI karışırsa kazanan TERS okunur ve hiçbir
        // derleyici uyarmaz. Nesne alan bu yol, iki girdiyi de tek kaynaktan
        // kendisi topluyor.
        //
        // İKİ BOOL'LU YOL DURUYOR: saf kuralı sınayan testlerin savaş kurmasına
        // gerek yok ve bu üye onların girişi olmaya devam ediyor.
        public static Team Winner(Battle battle)
        {
            if (battle == null)
            {
                throw new ArgumentNullException(nameof(battle));
            }

            return Winner(battle.IsTeamInPlay(Team.Player), battle.IsTeamInPlay(Team.Enemy));
        }

        /// <summary>
        /// Aynı iki cevabı ekranın okuyabileceği hâle çevirir: savaş sürüyor,
        /// oyuncu kazandı, düşman kazandı ya da iki taraf da tükendi.
        /// </summary>
        // WINNER'IN ÜSTÜNE OTURUYOR, ONUN YERİNE GEÇMİYOR — ve bunun ölçüsü
        // kazananın kim olduğu kuralının TEK bir yazarı kalması: aşağıda ne bir
        // taraf tercihi ne de ikinci bir karşılaştırma var, üstteki üye ne
        // diyorsa o. Buranın eklediği tek şey, o üyenin Team.None'da birleştirdiği
        // İKİ olguyu geri ayırmak.
        //
        // PARAMETRE ADLARI ÜSTTEKİNDEN FARKLI ve fark kasıtlı: nesne alan yol
        // buraya Battle.IsTeamInPlay'in cevabını taşıyor, yani soru "savaşçısı
        // kaldı mı" değil "oyunda mı". Üstteki adları düzeltmek bu şeridin işi
        // değil; yenisini yanlış adla doğurmak da olmazdı.
        //
        // REDDEDILEN - kazananı sonradan BattleOutcome'a çeviren bir eşleme.
        //     public static BattleOutcome ToOutcome(Team winner)
        //     {
        //         if (winner == Team.Player) { return BattleOutcome.PlayerWon; }
        //         if (winner == Team.Enemy) { return BattleOutcome.EnemyWon; }
        //         return BattleOutcome.Ongoing;
        //     }
        // KIRILAN: Team.None iki olgu taşıyor ve eşlemenin elinde onları ayıracak
        // girdi yok; berabere sonsuza dek "savaş sürüyor" okunur, pano karşılıklı
        // yok oluşta hiç açılmaz ve tahta oyuncunun elinde sonsuza dek donuk
        // kalırdı.
        // KAZANIRDI: Winner bir gün beraberliğe ayrı bir Team değeri döndürseydi
        // eşleme yeterdi ve bu üye tek satıra inerdi.
        // TEK CUMLE: kaybolan bir ayrımı geri getirebilen tek yer girdinin
        // kendisidir.
        public static BattleOutcome Outcome(bool playerInPlay, bool enemyInPlay)
        {
            Team winner = Winner(playerInPlay, enemyInPlay);

            if (winner == Team.Player)
            {
                return BattleOutcome.PlayerWon;
            }

            if (winner == Team.Enemy)
            {
                return BattleOutcome.EnemyWon;
            }

            // BURADA TEK BİR BOOL YETİYOR ve sebebi yukarıdaki dalların ne
            // elediği: kazanan yoksa iki cevap EŞİTTİR, dolayısıyla birine
            // bakmak ikisini birden okumaktır.
            return playerInPlay ? BattleOutcome.Ongoing : BattleOutcome.Draw;
        }

        /// <summary>
        /// Savaşın kendisine sorar ve cevabı ekranın okuyabileceği hâlde verir.
        /// </summary>
        // İKİ AŞIRI YÜKLEME, ÜSTTEKİ ÇİFTİN BİREBİR AYNI GEREKÇESİYLE: değer
        // alan yol dört hâlin dördünü de tek satırda kurdurur, nesne alan yol
        // iki girdiyi tek kaynaktan kendisi toplayarak sıra karışmasını keser.
        public static BattleOutcome Outcome(Battle battle)
        {
            if (battle == null)
            {
                throw new ArgumentNullException(nameof(battle));
            }

            return Outcome(battle.IsTeamInPlay(Team.Player), battle.IsTeamInPlay(Team.Enemy));
        }
    }
}
