namespace GridStrategy.Core
{
    // ═══ ROL: VARLIK (Entity) ════════════════════════════════════════
    // kimlik : var — aynı isimli iki Unit ayrı şeydir; BoardAdapter'ın
    //          sözlüğü ve Battle'ın iki sözlüğü tam olarak bu referans
    //          kimliğini anahtar yapıyor
    // hafıza : var (bugün ince) — tuttuğu tek durum Name; can, yaşam döngüsü
    //          ve savaş durumu bu tipte DEĞİL, ona eşlenen parçalarda yaşıyor
    // Unity  : gerekmez — sahne, prefab, koordinat bilmez
    // karar  : vermez — kim olduğunu bilir, ne yapacağını bilmez
    //
    // Yapılar da bu tiptir, çünkü strateji oyunlarında binalar da birimdir:
    // bir Barracks seçilir, canı vardır, hedeflenir, hücre kaplar.
    //
    // REDDEDILEN - Unit.cs:46 yerine (bu tipin adı kapsamına göre değişir ve
    //              yapılar için ikinci bir kimlik tipi doğar):
    //     public sealed class BoardPiece { }     // Unit yeniden adlandırılır
    //     public sealed class StructureId { }    // yapılar için ayrı kimlik
    // KIRILAN  : ikinci kimlik tipi ikinci bir tahtayı ZORUNLU kılar.
    //            UnitGrid StructureId tutamaz -> ikinci bir dizi doğar
    //            "bu hücre dolu mu" iki kez   -> biri unutulur
    //            aynı hücrede iki şey durur   -> hiçbir uyarı yok
    //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
    // KAZANIRDI: yapılar tahtada YER KAPLAMASAYDI — arka planda işleyen bir
    //            araştırma binası, ızgaraya oturmayan bir küresel yükseltme.
    // KARSILASTIRMA:
    //     Unit (bugün)        anahtar = yer kaplamak  -> tek tahta, tek doluluk sorusu
    //     BoardPiece          aynı tip, yeni ad       -> yalnızca okunurluk kazanır,
    //                                                    beş assembly'de ad değişir
    //     Unit + StructureId  tür başına kimlik       -> iki tahta, iki doluluk sorusu
    // TEK CUMLE: Kimlik tipini varlığın TÜRÜ değil, tahtanın SORDUĞU soru belirler;
    //            tahta "burada ne var" diye sorduğu için tek tip yeter.
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
    /// </summary>
    public sealed class Unit
    {
        public Unit(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
