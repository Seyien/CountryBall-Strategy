namespace GridStrategy.Unity
{
    //   ═══ SAVAŞ BİTTİ — SON KİP, ÇIKIŞ OKU YOK ═════════════════════
    //
    //     Gir()  : hiçbir şey
    //     Cik()  : hiçbir şey
    //     Ilerlet: hiçbir şey
    //     Tıklama: KİP SAHİPLENİR ─► tahta donar
    //              (seçim, saldırı, yürüyüş, kaydırma, B tuşu, çöp kutusu)
    //
    //     Buraya varan tek ok: VictoryRules savaşın bittiğini söyledi.
    //     Buradan çıkan ok YOK — yeniden başlamak sahneyi baştan yükler.

    /// <summary>
    /// Savaş bittikten sonraki hâl: pano "KAZANDIN" yazarken oyuncu arkadaki
    /// tahtada oynamaya devam edemez. Tıklama, sürükleme ve tuşlar ölüdür.
    /// TUZAK: bu kipin çıkışı yok; bileşen kapanıp yeniden açılırsa makine
    /// Boşta'da doğar ve donma kalkar — sahne yeniden yüklenmesi bunu kapatır.
    /// </summary>
    // REDDEDILEN - kipe hiç girmemek, tahtayı Update'te bir bayrakla durdurmak.
    //     private void Update()          // BoardAdapter.Update
    //     {
    //         AdvanceBattleTime();
    //         if (winnerAnnounced) { return; }
    //         ...
    //     }
    // KIRILAN: "tıklama ne demek" sorusunun ikinci bir sahibi doğardı ve kip
    // makinesi tam olarak o dağınıklığı toplamak için yazılmıştı; üstelik bayrak
    // yerleştirme kipini sessizce atlar, oyuncunun elindeki hayalet ekranda
    // asılı kalırdı.
    // KAZANIRDI: bitiş kareler arasında YAŞAMAYAN tek seferlik bir iş olsaydı —
    // bir ses çalmak gibi — kip yapmak makineye hiç gözlenemeyen bir durum
    // eklerdi.
    // TEK CUMLE: kip, kareler arasında yaşayan bir cevaptır ve "savaş bitti"
    // ondan sonraki her karede geçerlidir.
    public sealed class BattleOverBoardMode : IBoardMode
    {
        /// <summary>
        /// Kip fareyi ve klavyeyi tek başına sahiplenir: tahta hiçbir girdiyi
        /// kabul etmez.
        /// </summary>
        // TEK true, ÜÇ SONUÇ ve üçü de burada isteniyor: BoardAdapter.Update
        // sıradan tıklama akışını atlıyor, imleç çerçevesi kapanıyor ve yarım
        // kalmış kaydırma iptal ediliyor. Yeni bir üye eklemeye gerek kalmadı —
        // arayüzün var olan sorusu bu kipin ihtiyacını birebir karşılıyor.
        public bool OwnsPointer => true;

        // BOŞ GÖVDELER, IdleBoardMode'un birebir aynı gerekçesiyle: makinenin
        // her kipten Gir/Cik/Ilerlet istemesi, "kip yok" hâlini null ile temsil
        // etmenin doğuracağı kontrol kalabalığını tek bir yerde ödüyor.
        //
        // GİRİŞTE EKRANA DOKUNULMUYOR ve bu bir sınır: panoyu açan şey kip değil,
        // tahtanın yayınladığı olaydır. Kip ekranı da yazsaydı sonucu gösteren
        // İKİ sahip olurdu ve hangisinin kazandığı karenin sırasına kalırdı.
        /// <summary>Girişte yapılacak iş yok.</summary>
        public void Enter()
        {
        }

        /// <summary>Çıkışta toplanacak iz yok.</summary>
        public void Exit()
        {
        }

        /// <summary>Kare başına iş yok.</summary>
        public void Advance()
        {
        }
    }
}
