namespace GridStrategy.Battle
{
    // ═══ ROL: TANIM (Profile) ════════════════════════════════════════
    // kimlik : yok — iki PlayerWon değeri aynı şeydir
    // hafıza : yok — ölçüsü şu: Outcome'ın döndürdüğü PlayerWon'ı bir değişkene
    //          al, sonra kalan birimleri de öldür — değişken hâlâ PlayerWon'dır,
    //          çünkü savaşın O ANKİ hâlini değil geçmiş bir çağrının cevabını
    //          taşır. Şu anki hâli yeni bir Outcome çağrısı söyler
    // Unity  : gerekmez — noEngineReferences: true
    // karar  : vermez — olup biteni ADLANDIRIR; kararı VictoryRules verir
    /// <summary>
    /// Savaşın o andaki hâli — panonun "KAZANDIN / KAYBETTİN / BERABERE"
    /// cümlesini seçtiği tek değer. Kararı veren <see cref="VictoryRules"/> ile
    /// aynı katmanda yaşıyor. TUZAK: sıfırıncı değer <see cref="Ongoing"/>,
    /// yani atanmayı unutulan bir alan "savaş sürüyor" der, bir zafer değil.
    /// </summary>
    // GEREKÇELER: AYNA BELGE HENÜZ YOK ve kardeşi VictoryRules ile aynı borç,
    // aynı sebeple boş bırakıldı: var olmayan bir yola işaretçi, okuyanı tam bir
    // güvenle boşluğa gönderir.
    public enum BattleOutcome
    {
        // SIFIRINCI DEĞER AİLENİN ÖLÇÜSÜYLE SEÇİLDİ, ADIYLA DEĞİL. Ölçü tek bir
        // soru: atanmamış hâlin doğal karşılığı ne? Öteki dört enum'da o karşılık
        // "hiçbir şey denenmedi" olduğu için sıfır bir RET; burada sorulan şey bir
        // deneme değil bir HÂL, ve atanmamış bir alanın doğal karşılığı "savaş
        // daha bitmedi". Aynı ölçü PointerGesture'ın PointerPhase.Idle değerini
        // de sıfıra koymuştu. Sıfırda bir zafer dursaydı, atanmayı unutulan her
        // alan oyuncuya kazanmadığı bir savaşı kazanmış gösterirdi.
        // KURAL: Docs/deep/konular/06-sonuc-enumlari.md — "Birinci durak".
        /// <summary>Savaş sürüyor: iki taraf da hâlâ oyunda.</summary>
        Ongoing,

        /// <summary>Oyuncu kazandı: düşmanın oyunda ayakta hiçbir şeyi kalmadı.</summary>
        PlayerWon,

        /// <summary>Düşman kazandı: oyuncunun oyunda ayakta hiçbir şeyi kalmadı.</summary>
        EnemyWon,

        // BERABERLİK BUGÜN GERÇEKTEN OLUŞABİLİR ve bu değer tam olarak onu
        // adlandırmak için yazıldı: iki tarafın son birimleri aynı Tick içinde
        // kalıcı ölürse Battle.IsTeamInPlay ikisi için de false döner.
        // VictoryRules.Winner o hâli "savaş sürüyor" ile AYNI cevaba
        // (Team.None) indiriyor ve bu bir eksiklik değil yazılı bir karar —
        // VictoryRulesTests onu "mutual annihilation is not a victory either"
        // cümlesiyle sabitliyor. Ayrım o kararı bozarak değil, onun ÜSTÜNE
        // kuruluyor.
        /// <summary>İki taraf da tükendi: ilan edilecek kazanan yok.</summary>
        Draw
    }
}
