namespace GridStrategy.Combat
{
    // ═══ ROL: KURAL (Policy) ═════════════════════════════════════════
    // kimlik : yok — static; iki çağrıyı ayıracak bir şey yoktur
    // hafıza : yok — ölçüsü şu: CanProduce(Standing, Team.Player, true, true)
    //          kaç kez çağrılırsa çağrılsın Allowed döner. Bekleme süresini
    //          bu tip SAYMAZ; sayan yer StructureProduction ve buraya
    //          yalnızca "doldu mu" cevabı gelir
    // Unity  : gerekmez — girdi yalnızca iki enum ve iki bool; sınamak için
    //          ne sahne ne kare gerekir
    // karar  : UYGUNLUK söyler; ne üretir ne sayaç başlatır
    /// <summary>
    /// "Bu yapı şu an bu birimi üretebilir mi?" sorusunun tek sahibi.
    ///
    /// <see cref="TargetingRules"/> ile aynı ailedendir ve aynı sebeple ayrı
    /// yaşar: <see cref="Structure"/> kimin üretildiğini bilmemeli,
    /// <see cref="StructureBlueprint"/> yapının o anki hâlini bilmemeli,
    /// <see cref="StructureProduction"/> yalnızca saniye saymalı. Geriye
    /// DÖRDÜNCÜ bir sahip kaldı — burası.
    ///
    /// GİRDİLER NESNE DEĞİL OLGU: iki enum ve iki bool. Bu bir üslup tercihi
    /// değil, kuralın tam ve sonlu bir girdi kümesi üzerinde tüketilebilmesinin
    /// karşılığıdır — nesne alsaydı bu kuralı okumak için önce bir Structure,
    /// bir Health ve bir StructureLifecycle kurmak gerekirdi.
    ///
    /// AYNA BELGE: bu tipin gerekçeleri bugün yalnızca bu dosyada; kural
    /// sınıflarının ortak deseni Docs/deep/konular/04-karar-sirasi.md
    /// dosyasında yazılı.
    /// </summary>
    public static class ProductionRules
    {
        /// <summary>
        /// Üretim isteğinin sonucunu verir. Hiçbir şeyi değiştirmez.
        /// </summary>
        /// <param name="state">Üreten yapının o anki durumu.</param>
        /// <param name="producerTeam">Üreten yapının tarafı.</param>
        /// <param name="producesRequestedUnit">
        /// İstenen birim türü bu yapının üretim listesinde mi. Liste
        /// karşılaştırmasını çağıran yapar — bu kural bir listeyi hiç görmez ve
        /// görseydi <see cref="StructureBlueprint"/>'e bağlanırdı.
        /// </param>
        /// <param name="isReady">Bekleme süresi doldu mu.</param>
        // SIRA BİR KARARDIR VE BURADA TEK ÖLÇÜTLE DİZİLDİ: DÜZELTİLEMEYEN SEBEP
        // ÖNCE SÖYLENİR. Aynı ölçüt BattleActions'ın PlaceStructure üyesinde de
        // yazılı ve devralınıyor. Dördüncü satır tek başına ayrı bir küme:
        // bekleme süresi BEKLEMEKLE geçer, ötekilerin üçü geçmez. Sıra tersine
        // dönseydi yıkık bir barakaya tıklayan oyuncu "henüz hazır değil" cevabını
        // alır ve enkazın önünde beklemeye başlardı.
        public static ProductionOutcome CanProduce(
            StructureState state,
            Team producerTeam,
            bool producesRequestedUnit,
            bool isReady)
        {
            // Birinci sıra ÇAĞIRAN HATASINA ait: istenen birim bu yapının işi
            // değilse geri kalan üç sorunun hiçbirinin anlamı yok.
            if (!producesRequestedUnit)
            {
                return ProductionOutcome.RejectedUnknownUnit;
            }

            // Tarafsız bir yapı ordu kuramaz. Bu satır TargetingRules'taki
            // "attackerTeam == Team.None" kelepçesinin kardeşidir ve ortak bir
            // IsActor(Team) yardımcısına indirilebilir; bugün indirilmedi çünkü
            // ikinci çağıran yok ve o yardımcı iki kuralın arasında üçüncü bir
            // ev açardı. TETİKLEYİCİ: aynı cümleyi soran ÜÇÜNCÜ bir kural
            // doğduğu gün.
            if (producerTeam == Team.None)
            {
                return ProductionOutcome.RejectedNeutralProducer;
            }

            // KAPALI UÇLU (== Standing), açık uçlu değil — ve gerekçe
            // TargetingRules'un yapı sürümünden devralınıyor: "!= Destroyed"
            // bugün aynı cevabı verir, fark üçüncü bir durum eklendiği gün doğar
            // ve o gün açık uç yeni değeri SESSİZCE üretebilir sayardı.
            if (state != StructureState.Standing)
            {
                return ProductionOutcome.RejectedProducerDestroyed;
            }

            if (!isReady)
            {
                return ProductionOutcome.RejectedNotReady;
            }

            return ProductionOutcome.Allowed;
        }

        // MALİYET KONTROLÜ BU İMZADA YOK VE YOKLUĞU ÖLÇÜLDÜ: bu ağaçta enerji,
        // maliyet ya da kaynak adında tek bir üye yok — arama sıfır sonuç
        // veriyor. Buraya bir "yeterli kaynak var mı" parametresi koymak, hiçbir
        // yerde üretilmeyen bir sayıyı sorgulamak olurdu ve o parametre her
        // çağrıda uydurma bir true ile doldurulurdu.
        // TETİKLEYİCİ: ekonomi katmanı doğduğu gün — beşinci bir ret değeri ve
        // beşinci bir parametre o gün yerini hak eder.

        // KUYRUK DA YOK VE AYNI SEBEPLE: operatörün tarif ettiği akışta oyuncu
        // bir birim seçip haritaya koyuyor, sıraya dizmiyor. Kuyruğun ekleyeceği
        // tek şey "kaçıncı sıradasın" sorusudur ve o soruyu bugün soran yok.
        // TETİKLEYİCİ: bir yapıdan aynı anda BİRDEN FAZLA üretim istenebildiği
        // gün.
    }
}
