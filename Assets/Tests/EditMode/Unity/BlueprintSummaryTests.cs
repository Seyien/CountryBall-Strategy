using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using GridStrategy.Combat;
using GridStrategy.Unity;

namespace GridStrategy.Tests.EditMode.Unity
{
    /// <summary>
    /// Bilgi penceresinde görünen satırların davranışı.
    ///
    /// <b>SAHNE YOK.</b> <c>BlueprintSummary</c> saf bir C# tipi: girdisi düz
    /// bir tür tanımı, çıktısı bir metin. Sahne kurulmuyor, <c>TearDown</c>
    /// gerekmiyor.
    ///
    /// <b>BU DOSYANIN ÖLÇTÜĞÜ ASIL ŞEY YASAĞIN KENDİSİ.</b> Bilgi penceresi için
    /// yazılı kelepçe şu: <c>boardRect</c>, <c>width</c>, <c>height</c> ve
    /// <c>BoardSizing</c> okunmayacak. Aşağıdaki son test o yasağı bir
    /// hatırlatma olmaktan çıkarıp ölçülebilir kılıyor.
    /// </summary>
    // `using System;` YASAK (CS0104: Object adı UnityEngine.Object ile
    // belirsizleşiyor); tam nitelikli yazılıyor. Bu kural projede ölçüldü.
    public sealed class BlueprintSummaryTests
    {
        // ══ BİRİM — DÖRT SATIR, TEK KAYNAK ═══════════════════════════════

        /// <summary>
        /// Bir savaşçının canı, hasarı, menzili ve beklemesi satır satır çıkıyor.
        /// </summary>
        // OYUNDA NE İŞE YARAR: oyuncu bir türün sayılarını öğrenmek için varlık
        // dosyasını açmak zorunda kalmıyor.
        [Test]
        public void Unit_CarriesHealthDamageRangeAndCooldown()
        {
            var archer = new UnitBlueprint("Okçu", 24, new AttackProfile(7, 3, 1.5f));

            string summary = BlueprintSummary.Describe(archer);

            Assert.That(summary, Does.Contain("Can: 24"));
            Assert.That(summary, Does.Contain("Hasar: 7"));
            Assert.That(summary, Does.Contain("Menzil: 3 hücre"));
            Assert.That(summary, Does.Contain("Bekleme: 1.5 sn"));
        }

        /// <summary>
        /// Saldırı tanımı olmayan bir tür "Silahsız" diyor, sıfır DEMİYOR.
        /// </summary>
        // ██ SIFIR İLE YOKLUK AYNI ŞEY DEĞİL ██
        // "Hasar: 0 / Menzil: 0" satırları, silahı olmayan bir kışlayı silahı
        // BOZUK bir kışla gibi gösterirdi. Ölçülen olgu şu: yapı varlığı menzil
        // sıfırken AttackProfile'ı hiç kurmuyor, null geçiyor.
        [Test]
        public void Unit_WithNoAttackProfile_SaysUnarmedInsteadOfZero()
        {
            var worker = new UnitBlueprint("İşçi", 12, null);

            string summary = BlueprintSummary.Describe(worker);

            Assert.That(summary, Does.Contain(BlueprintSummary.Unarmed));
            Assert.That(summary, Does.Not.Contain("Hasar"));
            Assert.That(summary, Does.Not.Contain("Menzil"));
        }

        // ══ YAPI — İKİ SATIR DAHA ════════════════════════════════════════

        /// <summary>
        /// Bir yapı, savaş sayılarının ÜSTÜNE üretim süresini ve ürettiklerini
        /// ekliyor.
        /// </summary>
        [Test]
        public void Structure_AddsProductionTimeAndTheUnitsItMakes()
        {
            var infantry = new UnitBlueprint("Piyade", 30, new AttackProfile(10, 1, 1f));
            var scout = new UnitBlueprint("İzci", 18, new AttackProfile(5, 2, 0.8f));
            var factory = new StructureBlueprint(
                "Fabrika", 60, new AttackProfile(4, 2, 2f),
                new List<UnitBlueprint> { infantry, scout }, 0, 3f);

            string summary = BlueprintSummary.Describe(factory);

            Assert.That(summary, Does.Contain("Can: 60"));
            Assert.That(summary, Does.Contain("Üretim süresi: 3.0 sn"));
            Assert.That(summary, Does.Contain("Üretir: Piyade, İzci"));
        }

        /// <summary>
        /// Hiçbir şey üretmeyen bir yapı bunu SÖYLÜYOR; satır sessizce
        /// düşmüyor.
        /// </summary>
        // SATIRIN HİÇ YAZILMAMASI BİR CEVAP DEĞİL: oyuncu o zaman "üretmiyor mu,
        // yoksa pencere mi eksik" diye soramaz. Taret ve hisar bugün tam olarak
        // bu hâlde — ikisi de sıfır birim üretiyor.
        [Test]
        public void Structure_ThatMakesNothing_SaysSoInsteadOfDroppingTheLine()
        {
            var turret = new StructureBlueprint(
                "Taret", 40, new AttackProfile(12, 2, 1.2f),
                new List<UnitBlueprint>(), 0, 1f);

            string summary = BlueprintSummary.Describe(turret);

            Assert.That(summary, Does.Contain("Üretir: " + BlueprintSummary.ProducesNothing));
        }

        /// <summary>
        /// Silahsız bir yapı da "Silahsız" diyor — kural birimle AYNI.
        /// </summary>
        // İKİ AŞIRI YÜKLEME TEK CÜMLEYİ PAYLAŞIYOR ve bu test o paylaşımın
        // kopmadığını ölçüyor: sabit ikiye bölünseydi biri düzeltildiğinde öteki
        // eskirdi.
        [Test]
        public void Structure_WithNoWeapon_UsesTheSameUnarmedSentenceAsAUnit()
        {
            var barracks = new StructureBlueprint(
                "Kışla", 50, null, new List<UnitBlueprint>(), 0, 2f);

            Assert.That(BlueprintSummary.Describe(barracks), Does.Contain(BlueprintSummary.Unarmed));
        }

        /// <summary>
        /// Boş gözlerden sonra gösterilecek ad kalmadıysa cevap yine
        /// "Üretmiyor".
        /// </summary>
        // DİZİNİN UZUNLUĞU DEĞİL, GÖSTERİLECEK AD SAYISI KARAR VERİYOR: uzunluğa
        // bakılsaydı tek gözü boş bir dizi "Üretir: " yazıp arkasını boş
        // bırakırdı.
        [Test]
        public void Structure_WhoseProducedListIsAllHoles_StillSaysItMakesNothing()
        {
            var hollow = new StructureBlueprint(
                "Depo", 30, null, new List<UnitBlueprint> { null, null }, 0, 1f);

            Assert.That(
                BlueprintSummary.Describe(hollow),
                Does.Contain("Üretir: " + BlueprintSummary.ProducesNothing));
        }

        // ══ SINIRLAR ═════════════════════════════════════════════════════

        /// <summary>
        /// Tanım yoksa metin de yok — istisna DEĞİL.
        /// </summary>
        // ÇAĞIRAN BİR EKRAN, BİR KURAL DEĞİL: yarım kurulmuş bir varlık yüzünden
        // pencerenin hiç açılmaması, eksik bir satır göstermekten daha kötü bir
        // cevaptır.
        [Test]
        public void NullDefinitions_ProduceEmptyTextInsteadOfThrowing()
        {
            Assert.That(BlueprintSummary.Describe((UnitBlueprint)null), Is.Empty);
            Assert.That(BlueprintSummary.Describe((StructureBlueprint)null), Is.Empty);
        }

        /// <summary>
        /// Ondalık ayracı makinenin kültürüne göre DEĞİŞMİYOR.
        /// </summary>
        // ██ BU TEST BİR MAKİNE BAĞIMLILIĞINI KESİYOR ██
        // Kültüre bırakılsaydı aynı kod Türkçe bir makinede "1,5 sn", İngilizce
        // bir makinede "1.5 sn" üretir ve testin kendisi hangi makinede
        // koştuğuna bağlanırdı. Aşağıda kültür BİLEREK değiştiriliyor ve cevabın
        // aynı kaldığı ölçülüyor.
        [Test]
        public void DecimalSeparator_DoesNotFollowTheMachineCulture()
        {
            CultureInfo original = Thread.CurrentThread.CurrentCulture;

            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");

                var unit = new UnitBlueprint("Er", 10, new AttackProfile(1, 1, 2.5f));

                Assert.That(BlueprintSummary.Describe(unit), Does.Contain("2.5 sn"));
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = original;
            }
        }

        /// <summary>
        /// Satırları üreten tip tahtadan HİÇBİR ŞEY istemiyor.
        /// </summary>
        // ██ STATİK BOYUT YASAĞININ MAKİNE KARŞILIĞI ██
        // Yazılı kelepçe şuydu: bilgi penceresi boardRect, width, height ve
        // BoardSizing okumayacak. Bir yorum satırı o yasağı hatırlatır ama
        // korumaz; imzanın kendisi korur. Bu iddia, tahtadan gelen bir tipin
        // parametre listesine sızdığı gün kırmızı verir.
        [Test]
        public void Describe_TakesNothingThatComesFromTheBoard()
        {
            MethodInfo[] members = typeof(BlueprintSummary).GetMethods(
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

            Assert.That(members.Length, Is.EqualTo(2), "yalnız iki aşırı yükleme bekleniyor");

            for (int i = 0; i < members.Length; i++)
            {
                ParameterInfo[] parameters = members[i].GetParameters();

                Assert.That(parameters.Length, Is.EqualTo(1), members[i].Name + " tek girdi almalı");
                Assert.That(
                    parameters[0].ParameterType.Namespace,
                    Is.EqualTo("GridStrategy.Combat"),
                    "girdi bir TÜR tanımı olmalı; tahtadan gelen hiçbir tip geçmemeli");
            }
        }
    }
}
