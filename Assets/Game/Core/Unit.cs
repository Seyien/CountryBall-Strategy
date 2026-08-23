namespace GridStrategy.Core
{
    // ═══ ROL: VARLIK (Entity) ════════════════════════════════════════
    // kimlik : var — ölçüsü şu: aynı adla İKİ Unit kur ve ikisini birden
    //          Battle'ın Dictionary<Unit, Combatant> sözlüğüne anahtar yap;
    //          sözlük İKİ ayrı girdi tutar. Farkı doğuran şey bu dosyada bir
    //          kodun VARLIĞI değil YOKLUĞU: ne Equals ne GetHashCode
    //          geçersiz kılındığı için anahtar referans eşitliğidir.
    // hafıza : yok — ölçüsü şu: kurucudan sonra bu nesneye YAZAN hiçbir üye
    //          yok; Name get-only, ne set'i ne SetName'i var, yani aynı okuma
    //          birinci çağrıda da bininci çağrıda da aynı dizeyi verir. Can,
    //          durum ve taraf bu tipte DEĞİL, ona eşlenen yan tablolarda
    //          yaşar; hafızayı onlar taşır.
    // Unity  : gerekmez — ölçüsü şu: asmdef'te references boş ve
    //          noEngineReferences: true; sahne, prefab, koordinat geçmez
    // karar  : vermez — ölçüsü şu: tipte çağrılabilir tek bir metot yok,
    //          yalnızca kurucu ile bir okuma var; kim olduğunu bilir, ne
    //          yapacağını bilmez
    //
    // TEK KİMLİK TİPİ: TAHTANIN ANAHTARI TÜRE GÖRE ÇOĞALMAZ. Yapılar da bu
    // tiptir, çünkü strateji oyunlarında binalar da birimdir: bir baraka
    // seçilir, canı vardır, hedeflenir, hücre kaplar. Tür başına ikinci bir
    // kimlik tipi ikinci bir tahtayı ZORUNLU kılar ve "bu hücre dolu mu"
    // sorusu ikiye bölünür.
    // → Unit.md#unit-tip
    /// <summary>
    /// Tahtada yer kaplayan, kimliği olan şey.
    ///
    /// <b>"Birim" değil</b> — bir asker de bir baraka da budur.
    /// <see cref="UnitGrid"/> bu tipi tutar ve tuttuğu şeyin ne olduğunu
    /// bilmez; savaşçı mı yapı mı sorusunun cevabı bu tipte değil, ona hangi
    /// parçanın eşlendiğinde yaşar (<c>GridStrategy.Combat</c>'taki
    /// <c>Combatant</c> ve <c>Structure</c>).
    ///
    /// Neyi BİLMEZ: nerede durduğunu (tahtanın işi), canını, tarafını,
    /// durumunu, nasıl çizildiğini. Taşıdığı tek şey kimliğin kendisidir; ad
    /// ise yalnızca insanın okuması için.
    ///
    /// GEREKÇELER: Docs/deep/kod/Core/Unit.md
    /// </summary>
    public sealed class Unit
    {
        // Tek parametre ad. Menzil, can, taraf ve durum bilerek YOK: onlar
        // "ne yapabileceği"dir ve bu tip yalnızca "kim olduğu"nu taşır.
        // → Unit.md#unitstring-name
        public Unit(string name)
        {
            Name = name;
        }

        // Get-only otomatik property; kurucuda konur, bir daha yazılmaz. Ad
        // insanın okuması içindir, ANAHTAR DEĞİLDİR: sözlükleri ayakta tutan
        // şey referans eşitliğidir. Ada göre Equals eklendiği gün "Piyade" adlı
        // iki ayrı birim üç assembly'deki sözlüklerde tek girdiye çöker.
        // → Unit.md#name
        public string Name { get; }
    }
}
