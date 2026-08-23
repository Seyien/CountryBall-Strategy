using System;
using System.Collections.Generic;
using NUnit.Framework;
using GridStrategy.Combat;

namespace GridStrategy.Tests.EditMode.Combat
{
    /// <summary>
    /// Bu dosyanın tamamı 10 ve 5 saniyelik kuralları sınıyor ve saniyeler
    /// içinde değil MİLİSANİYELER içinde bitiyor — çünkü zaman Tick ile
    /// dışarıdan veriliyor. Time.deltaTime okunsaydı bu testler PlayMode'a
    /// düşer ve gerçekten 15 saniye beklerdi.
    /// </summary>
    public sealed class UnitLifecycleTests
    {
        private static UnitLifecycle NewLifecycle()
        {
            return new UnitLifecycle(downedWindowSeconds: 10f, corpseWindowSeconds: 5f);
        }

        [Test]
        public void NewUnit_StartsAlive()
        {
            var lifecycle = NewLifecycle();

            Assert.That(lifecycle.State, Is.EqualTo(UnitState.Alive));
            Assert.That(lifecycle.IsReadyForCleanup, Is.False);
        }

        [Test]
        public void Alive_TickDoesNothing()
        {
            var lifecycle = NewLifecycle();

            lifecycle.Tick(1000f);

            Assert.That(lifecycle.State, Is.EqualTo(UnitState.Alive), "Zaman tek başına öldürmez.");
        }

        [Test]
        public void HealthDepleted_MovesToDowned()
        {
            var lifecycle = NewLifecycle();

            lifecycle.OnHealthDepleted();

            Assert.That(lifecycle.State, Is.EqualTo(UnitState.Downed));
            Assert.That(lifecycle.RemainingSeconds, Is.EqualTo(10f));
        }

        // Pencerenin İKİ yanı da test ediliyor: sınırın nerede olduğu yorumla
        // değil testle sabitlenir.
        [Test]
        public void Downed_JustBeforeWindowCloses_StillDowned()
        {
            var lifecycle = NewLifecycle();
            lifecycle.OnHealthDepleted();

            lifecycle.Tick(9.9f);

            Assert.That(lifecycle.State, Is.EqualTo(UnitState.Downed), "Kurtarma penceresi hâlâ açık.");
        }

        [Test]
        public void Downed_WhenWindowCloses_BecomesDead()
        {
            var lifecycle = NewLifecycle();
            lifecycle.OnHealthDepleted();

            // REDDEDILEN - UnitLifecycleTests.cs:89 yerine:
            //     [UnityTest] public IEnumerator ... {
            //         yield return new WaitForSeconds(10.1f);
            // Üstteki iki satırın taşıdığı üç ödünç ad (motorun test
            // özniteliği, .NET'in gezinme arayüzü, motorun bekleme nesnesi)
            // bu projede YALNIZCA reddedilen blokların içinde geçiyor; sayımı
            // ve niçin böyle olduğu şurada:
            // DERİN ANLATIM: Docs/deep/konular/08-motor-cagri-dongusu.md
            // KIRILAN  : test EditMode'dan PlayMode'a düşer; dosyanın süresi
            //            milisaniyeden dakikaya çıkar.
            //            kare süresi 0,1 saniyeyi aşar -> "hâlâ Downed" iddiası
            //            rastgele kırmızı olur, pencerenin TAM yeri sınanamaz
            //            derleyici: hiçbir şey der  .  test: kırmızılığı kurala
            //            değil o günkü kare süresine bağlanır
            // KAZANIRDI: sınanan şey gerçekten motorun kare döngüsü olsaydı —
            //            animasyon süresi, fizik adımı ya da coroutine sırası
            //            gibi, Tick ile taklit edilemeyecek bir davranış.
            // TEK CUMLE: Zamanı parametre olarak alan bir tip zamanı BEKLEMEDEN
            //            sınanır; beklemek ölçüyü değil yalnızca süreyi büyütür.
            lifecycle.Tick(10.1f);

            Assert.That(lifecycle.State, Is.EqualTo(UnitState.Dead));
            Assert.That(lifecycle.RemainingSeconds, Is.EqualTo(5f), "Ceset sayacı başlamalı.");
            Assert.That(lifecycle.IsReadyForCleanup, Is.False, "Ölmek, kaldırılmak demek değildir.");
        }

        // Zaman parça parça da gelse aynı sonuç: kare süreleri değişkendir,
        // kural kare sayısına değil TOPLAM süreye bağlı olmalı.
        [Test]
        public void Downed_ManySmallTicks_SumToTheSameOutcome()
        {
            var lifecycle = NewLifecycle();
            lifecycle.OnHealthDepleted();

            for (int i = 0; i < 100; i++)
            {
                lifecycle.Tick(0.1f);
            }

            Assert.That(lifecycle.State, Is.EqualTo(UnitState.Dead));
        }

        [Test]
        public void Downed_Revive_ReturnsToAlive()
        {
            var lifecycle = NewLifecycle();
            lifecycle.OnHealthDepleted();
            lifecycle.Tick(9f);

            bool revived = lifecycle.TryRevive();

            Assert.That(revived, Is.True);
            Assert.That(lifecycle.State, Is.EqualTo(UnitState.Alive));
        }

        [Test]
        public void Dead_CannotBeRevived()
        {
            var lifecycle = NewLifecycle();
            lifecycle.OnHealthDepleted();
            lifecycle.Tick(10.1f);

            bool revived = lifecycle.TryRevive();

            Assert.That(revived, Is.False, "Kalıcı ölüm kalıcıdır.");
            Assert.That(lifecycle.State, Is.EqualTo(UnitState.Dead));
        }

        [Test]
        public void Alive_CannotBeRevived()
        {
            var lifecycle = NewLifecycle();

            Assert.That(lifecycle.TryRevive(), Is.False, "Ayakta olan birim diriltilemez.");
        }

        [Test]
        public void Dead_WhenCorpseWindowCloses_RequestsCleanup()
        {
            var lifecycle = NewLifecycle();
            lifecycle.OnHealthDepleted();
            lifecycle.Tick(10.1f);

            lifecycle.Tick(5.1f);

            Assert.That(lifecycle.IsReadyForCleanup, Is.True);
            Assert.That(lifecycle.RemainingSeconds, Is.Zero, "UI negatif sayı göstermemeli.");
        }

        // Downed birime tekrar vurmak onu ANINDA öldürmemeli. "İşini bitirme"
        // ayrı bir kural (düşme canı) ve henüz yazılmadı; bu test o boşluğun
        // sessizce kapanmasını engelliyor.
        [Test]
        public void Downed_HealthDepletedAgain_DoesNotSkipTheWindow()
        {
            var lifecycle = NewLifecycle();
            lifecycle.OnHealthDepleted();
            lifecycle.Tick(3f);

            lifecycle.OnHealthDepleted();

            Assert.That(lifecycle.State, Is.EqualTo(UnitState.Downed));
            Assert.That(lifecycle.RemainingSeconds, Is.EqualTo(7f), "Geri sayım sıfırlanmamalı.");
        }

        [Test]
        public void Tick_NegativeTime_Throws()
        {
            var lifecycle = NewLifecycle();

            Assert.Throws<ArgumentOutOfRangeException>(() => lifecycle.Tick(-0.1f));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void Constructor_NonPositiveWindow_Throws(float seconds)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new UnitLifecycle(downedWindowSeconds: seconds));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new UnitLifecycle(corpseWindowSeconds: seconds));
        }

        // ÖĞRENEN KARARI (2026-08-18): diriltilen birim tekrar düşerse geri
        // sayım SIFIRDAN başlar, kaldığı yerden devam etmez.
        //
        // Gerekçesi denge değil kapsam: "kalan süreyi hatırlama" ikinci bir
        // sayaç ve ikinci bir kural demek. Bugün en basit doğru davranış
        // seçiliyor; farklılaştırmak istenirse o gün tek bir alan eklenir.
        //
        // Bu test o kararı SABİTLİYOR: biri ileride "kaldığı yerden devam
        // etsin" diye değiştirirse, tercih sessizce kaymaz, burası kırmızı olur.
        //
        // ÖLÇÜ, okuyucunun kafasında koşturabileceği hâliyle: aynı tipe
        // OnHealthDepleted -> Tick(7) -> TryRevive -> OnHealthDepleted sırasını
        // uygula. Tek alanlı uygulamada RemainingSeconds 10 döner; "kalanı
        // hatırlayan" uygulamada 3 döner. Aradaki 7 saniye, ikinci bir alanın
        // ve onu ne zaman sıfırlayacağını söyleyen ikinci bir kuralın fiyatı.
        // DERİN ANLATIM: Docs/deep/konular/05-yasam-dongusu.md
        [Test]
        public void Revived_ThenDownedAgain_StartsAFullWindow()
        {
            var lifecycle = NewLifecycle();
            lifecycle.OnHealthDepleted();
            lifecycle.Tick(7f);                 // 3 saniyesi kalmisti
            lifecycle.TryRevive();

            lifecycle.OnHealthDepleted();       // tekrar dustu

            Assert.That(lifecycle.State, Is.EqualTo(UnitState.Downed));
            // REDDEDILEN - UnitLifecycleTests.cs:234 yerine:
            //     Assert.That(lifecycle.RemainingSeconds, Is.EqualTo(3f),
            //         "Kalan sure devralinir.");
            // KIRILAN  : "kaldığı yerden devam" tek sayaçla YAZILAMAZ.
            //            kalanı saklayan ikinci alan gerekir -> onu ne zaman
            //            sıfırlayacağı belli olmayan ikinci bir kural doğar
            //            TryRevive geçmişi de taşır -> tek remainingSeconds
            //            alanı ayakta kalamaz
            //            derleyici: hiçbir şey der  .  test: bedeli başka dosyada
            // KAZANIRDI: denge "bir kere diriltilmiş birim daha kırılgan olsun"
            //            derse — o zaman kalan sürenin devralınması hata değil,
            //            tam istenen ceza olurdu.
            // TEK CUMLE: Bir testin beklediği sayı, sınanan tipin kaç alan
            //            taşımak zorunda olduğunu belirler.
            Assert.That(lifecycle.RemainingSeconds, Is.EqualTo(10f),
                "Yeni dusus yeni pencere acar; kalan 3 saniye devralinmaz.");
        }

        // ── StateChanged event'i ────────────────────────────────────────
        //
        // Bu blok bir DAVRANIŞI değil bir SÖZLEŞMEYİ koruyor: event yalnız
        // gerçek geçişlerde ve her geçiş için TAM BİR KEZ tetiklenir. İkisi de
        // sessizce bozulabilen türden: fazladan tetikleme iki kez ses çalar,
        // eksik tetikleme cesedi ekranda bırakır — ve ikisi de derleme hatası
        // vermez.

        [Test]
        public void StateChanged_FiresOnceWhenHealthIsDepleted()
        {
            var lifecycle = new UnitLifecycle();
            var seen = new List<UnitState>();
            // ÖDÜNÇ ALINAN — metot grubu (`seen.Add`): parantez yok, yani
            // çağrı değil; derleyici `seen` ALICISINI de saran bir delege
            // üretiyor ve olay listesine giren şey o çifttir. Kayıt kabı bu
            // yüzden ayrı bir sınıf istemiyor — List'in kendi metodu yetiyor.
            // DİL: Docs/deep/dil/04-delege-olay-ve-kapanis.md
            //
            // ÖDÜNÇ ALINAN — NUnit `Is.EqualTo` bir KOLEKSİYONA verildiğinde
            // referans değil, ELEMAN SIRASI karşılaştırır; List ile dizi bu
            // yüzden eşit sayılabiliyor. Ölçüsü: aşağıdaki dizi bir List'e
            // çevrilse iddia yine geçer, ama eleman sırası değişirse geçmez.
            lifecycle.StateChanged += seen.Add;

            lifecycle.OnHealthDepleted();

            Assert.That(seen, Is.EqualTo(new[] { UnitState.Downed }));
        }

        [Test]
        public void StateChanged_DoesNotFireWhileAliveTicksPass()
        {
            var lifecycle = new UnitLifecycle();
            var seen = new List<UnitState>();
            lifecycle.StateChanged += seen.Add;

            lifecycle.Tick(1f);
            lifecycle.Tick(100f);

            // Alive'da zaman gecmesi bir GECIS degildir. Bu test olmasaydi
            // "her Tick'te haber ver" hatasi her kare event yayardi ve dinleyen
            // taraf kare basina is yapardi.
            Assert.That(seen, Is.Empty);
        }

        [Test]
        public void StateChanged_FiresForEachRealTransitionInOrder()
        {
            var lifecycle = new UnitLifecycle(downedWindowSeconds: 10f, corpseWindowSeconds: 5f);
            var seen = new List<UnitState>();
            lifecycle.StateChanged += seen.Add;

            lifecycle.OnHealthDepleted();   // Alive -> Downed
            lifecycle.Tick(10.1f);          // Downed -> Dead
            lifecycle.Tick(5.1f);           // Dead -> Dead (geçiş YOK, temizlik bayrağı)

            Assert.That(seen, Is.EqualTo(new[] { UnitState.Downed, UnitState.Dead }));
            // Üçüncü Tick durumu değiştirmedi ama IsReadyForCleanup'ı açtı.
            // Yani "bir şey oldu" ile "durum değişti" aynı şey değil — ve event
            // yalnızca ikincisini taşır.
            Assert.That(lifecycle.IsReadyForCleanup, Is.True);
        }

        [Test]
        public void StateChanged_DoesNotFireWhenReviveIsRejected()
        {
            var lifecycle = new UnitLifecycle();
            var seen = new List<UnitState>();
            lifecycle.StateChanged += seen.Add;

            bool revived = lifecycle.TryRevive();   // Alive iken başarısız

            Assert.That(revived, Is.False);
            // Başarısız bir işlem sessizdir. Bu satır olmasaydı "denedim ama
            // olmadı" da bir geçiş gibi yayılır ve dinleyen taraf durumu
            // değişmemiş bir birim için animasyon oynatırdı.
            Assert.That(seen, Is.Empty);
        }

        [Test]
        public void StateChanged_FiresOnSuccessfulRevive()
        {
            var lifecycle = new UnitLifecycle();
            lifecycle.OnHealthDepleted();

            var seen = new List<UnitState>();
            lifecycle.StateChanged += seen.Add;   // abone DUSTUKTEN sonra eklendi

            bool revived = lifecycle.TryRevive();

            Assert.That(revived, Is.True);
            Assert.That(seen, Is.EqualTo(new[] { UnitState.Alive }));
        }

        [Test]
        public void StateChanged_WithNoSubscribers_DoesNotThrow()
        {
            var lifecycle = new UnitLifecycle();

            // Abonesiz bir event null'dur; ?.Invoke olmadan bu satir
            // NullReferenceException atardi. Uretim kodunda hicbir zaman
            // abone GARANTISI yok - UI kapaliyken de birim dusebilir.
            Assert.DoesNotThrow(() => lifecycle.OnHealthDepleted());
            Assert.That(lifecycle.State, Is.EqualTo(UnitState.Downed));
        }
    }
}
