using System;
using NUnit.Framework;
using GridStrategy.Combat;

// Bu using satırı SINIF için değil, uzatma metodu (extension method) için
// gerekli: "Not.AllocatingGCMemory()" NUnit'in ConstraintExpression tipine
// Unity tarafından sonradan eklenmiş bir metottur ve namespace açık değilse
// derleyici onu bulamaz.
using UnityEngine.TestTools.Constraints;

// İki ayrı kütüphanede de "Is" adında bir sınıf var: NUnit'inki (Is.EqualTo,
// Is.GreaterThan) ve Unity'ninki (Is.AllocatingGCMemory). Yukarıdaki iki
// namespace de açık olduğu için çıplak "Is" yazmak belirsizdir ve CS0104 verir.
// Takma ad (alias) her ikisini de tekilleştiriyor; alias, namespace'ten
// önceliklidir.
using Is = NUnit.Framework.Is;
using UnityIs = UnityEngine.TestTools.Constraints.Is;

namespace GridStrategy.Tests.EditMode.Combat
{
    /// <summary>
    /// Maliyet testleri: davranışın DOĞRU değil, UCUZ olduğunu iddia eder.
    /// Ölçülen şey süre değil, tahsis edilen bellektir — süre makineye göre
    /// oynar, tahsis oynamaz: 0 byte her makinede tam olarak 0 byte'tır.
    ///
    /// ÖLÇÜM ARACI DEĞİŞTİ (2026-08-17): ilk sürüm
    /// GC.GetAllocatedBytesForCurrentThread kullanıyordu. Negatif kontrol o
    /// aracın bu Unity/Mono sürümünde her zaman 0 döndürdüğünü ortaya çıkardı —
    /// yani ölçmüyordu. Yerine Unity Test Framework'ün kendi kısıtı
    /// (AllocatingGCMemory) kullanılıyor; bu kısıt motorun profiler kaydediciye
    /// bağlanır, .NET'in sayacına değil.
    ///
    /// NOT: Unity'ye bağımlı olan bu TEST assembly'sidir. Ölçülen üretim kodu
    /// (GridStrategy.Combat) noEngineReferences: true olarak kalır.
    /// </summary>
    public sealed class DamageRulesAllocationTests
    {
        private const int Iterations = 1000;

        // Bilerek alan (field), yerel değişken değil: kısıt bir lambda alıyor ve
        // lambda içinden yerel değişkene dokunmak "closure" nesnesi doğurur —
        // yani ölçmek istediğimiz kodun yanında ölçüm kurulumunun kendisi tahsis
        // yapardı. Alan kullanınca closure oluşmaz.
        private int sink;
        private Health health;

        /// <summary>
        /// NEGATİF KONTROL — ölçüm aracının kendisini sınar, üretim kodunu değil.
        ///
        /// Diğer iki test "hiç tahsis yok" bekliyor. Araç körse o testler kod
        /// bozulsa bile yeşil kalır: yanlış güven. Bu test bilerek bellek tahsis
        /// eder ve aracın bunu GÖRDÜĞÜNÜ kanıtlar. Bu kırmızıysa, diğer ikisinin
        /// yeşili hiçbir şey ifade etmez ve önce burası düzeltilir.
        /// </summary>
        [Test]
        public void Olcum_Aygiti_Tahsisi_Gorebiliyor()
        {
            Assert.That(() =>
            {
                // new ile dizi ayırmak managed heap'te yer kaplar; araç bunu
                // görmek ZORUNDA. KeepAlive, "bu dizi kullanılmıyor" denip
                // tahsisin elenme ihtimaline karşı.
                var buffer = new int[64];
                GC.KeepAlive(buffer);
            }, UnityIs.AllocatingGCMemory(),
                "Unity'nin GC.Alloc kaydedicisi bilerek yapılan bir tahsisi bile " +
                "görmüyor. Bu dosyadaki diğer testler yanlış yeşil verir; " +
                "ölçüm yöntemi değişmeden onlara güvenilmemeli.");
        }

        /// <summary>
        /// Hasar formülü sıcak yolda çalışır: her vuruşta, her birim için.
        /// Sıfır tahsis, kare başına çöp üretmediği ve GC duraklamalarına
        /// katkı yapmadığı anlamına gelir.
        /// </summary>
        [Test]
        public void ResolveRemaining_Hic_Tahsis_Yapmaz()
        {
            Warmup();
            sink = 0;

            Assert.That(() =>
            {
                for (int i = 0; i < Iterations; i++)
                {
                    sink += DamageRules.ResolveRemaining(100, 10);
                }
            }, UnityIs.Not.AllocatingGCMemory());

            // sink okunmasaydı derleyicinin döngüyü tamamen atma hakkı olurdu ve
            // "tahsis yok" sonucu kodun ucuz olduğunu değil, hiç çalışmadığını
            // gösterirdi.
            Assert.That(sink, Is.GreaterThan(0), "Döngü elenmiş olmamalı.");
        }

        /// <summary>
        /// Bu test bir REFAKTÖR kararını savunur: hasar formülü ayrı bir sınıfa
        /// (DamageRules) çıkarıldı. Ayrı sınıfa çıkarmanın çalışma zamanı bedeli
        /// olsaydı burada görünürdü. Görünmüyorsa "temiz ayrım pahalıdır"
        /// itirazının bu vaka için karşılığı yok demektir.
        /// </summary>
        [Test]
        public void TakeDamage_Ayri_Siniftaki_Formulu_Cagirirken_Tahsis_Yapmaz()
        {
            health = new Health(int.MaxValue);
            Warmup();
            health.TakeDamage(0);

            Assert.That(() =>
            {
                for (int i = 0; i < Iterations; i++)
                {
                    health.TakeDamage(1);
                }
            }, UnityIs.Not.AllocatingGCMemory());

            Assert.That(health.Current, Is.EqualTo(int.MaxValue - Iterations));
        }

        // ISINMA: bir metot ilk çağrıldığında JIT onu makine koduna çevirir ve bu
        // tek seferlik iş tahsis yapabilir. Isınmadan ölçseydik derleyicinin bir
        // kerelik maliyetini formülün sürekli maliyeti sanırdık.
        private static void Warmup()
        {
            for (int i = 0; i < Iterations; i++)
            {
                DamageRules.ResolveRemaining(100, 10);
            }
        }
    }
}

/*
1. kaydediciyi başlat
2. delegate'i çağır          ← kod TAM BURADA çalışmalı
3. kaydediciyi durdur
4. sayıyı oku, karara bağla
*/