using System;
using GridStrategy.Core;
using NUnit.Framework;

namespace GridStrategy.Tests.EditMode.Core
{
    /// <summary>
    /// Bu dosyada sınanan şey iki satırlık bir kurucu değil, bir EŞİK:
    /// hareket menzilinin alt sınırı 0 mı, 1 mi. Sayıyı saklamak sınanmaya
    /// değmez; sınırın nerede olduğu kararı ise saldırı tarafındaki ikiziyle
    /// bilerek ayrışıyor ve o ayrışmayı koruyan tek yer burasıdır.
    /// </summary>
    // REDDEDILEN - MoveProfileTests.cs:27 yerine (dosya
    //              Assets/Tests/EditMode/Combat/ altına, ikizi
    //              AttackProfileTests'in yanına konur):
    //     namespace GridStrategy.Tests.EditMode.Combat { ... }
    // KIRILAN  : DERLENMEZ, ve reddin sebebi kararın ta kendisi.
    //            Combat.EditModeTests asmdef'i yalnızca GridStrategy.Combat'a
    //            referans veriyor -> MoveProfile ise GridStrategy.Core'da
    //            derleyici: tipi bulamaz der  .  test: "iki profil kardeştir"
    //            refleksini derleyici reddeder, okuyucu değil
    // KAZANIRDI: iki test asmdef'i birleştirilip tek bir EditMode test
    //            assembly'si yapılsaydı — o gün dosyanın hangi klasörde durduğu
    //            bir derleme sorusu olmaktan çıkar, okunabilirlik sorusu olurdu.
    // TEK CUMLE: Bir test dosyasının klasörü zevk meselesi değil, hangi
    //            assembly'nin neyi görebildiğinin sonucudur.
    public sealed class MoveProfileTests
    {
        [Test]
        public void Constructor_StoresRange()
        {
            var profile = new MoveProfile(range: 3);

            Assert.That(profile.Range, Is.EqualTo(3));
        }

        /// <summary>
        /// ASİMETRİ KARARINI koruyan test. Aynı sayı, iki profile TERS cevap
        /// veriyor: <c>AttackProfile(damage: 5, range: 0)</c> fırlatır,
        /// <c>MoveProfile(range: 0)</c> geçerlidir.
        ///
        /// Sebep tasarımda: hiçbir hücreye ulaşamayan bir SALDIRI anlamsızdır,
        /// hiçbir hücreye gidemeyen bir BİRİM anlamlıdır — kök salmış,
        /// sersemlemiş, kuşatılmış.
        ///
        /// Kırmızıya dönerse biri iki profili simetrik hâle getirmiş demektir.
        /// O değişiklik hiçbir şeyi patlatmaz: yalnızca "kımıldayamaz" hâli
        /// profil diliyle ifade edilemez olur ve MoveAction'ın int alan sürümü
        /// ile profil alan sürümü aynı oyun durumlarını anlatmayı bırakır.
        /// </summary>
        [Test]
        public void Constructor_ZeroRange_IsAllowed()
        {
            Assert.That(
                new MoveProfile(range: 0).Range,
                Is.Zero,
                "zero move range is a valid unit that cannot leave its cell, not an invalid profile");
        }

        // Tek kural var (range < 0), dolayısıyla tek test var; iki değer aynı
        // kuralın iki örneği. Gerekçe AttackProfileTests'te yazılı ve burada
        // TEKRARLANMIYOR — orada -3 ile 0 aynı satırdaydı, burada 0 artık
        // geçerli olduğu için yalnızca negatifler kaldı.
        [TestCase(-1)]
        [TestCase(-3)]
        public void Constructor_NegativeRange_Throws(int range)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new MoveProfile(range: range));
        }

        /// <summary>
        /// KATMAN KARARINI koruyan test. <see cref="MoveProfile"/>
        /// GridStrategy.Core'da yaşıyor, ikizi AttackProfile ise
        /// GridStrategy.Combat'ta — çünkü hareket menzili TAHTAYA, saldırı
        /// menzili SAVAŞA ait bir kavramdır.
        ///
        /// Bu satır o kararı çalışabilir hâle getiriyor: profili
        /// <see cref="MoveAction"/>'a parametre olarak GEÇEBİLİYORUZ. Profil
        /// Combat'ta doğsaydı bu dosya derlenmezdi, çünkü Core'un test
        /// assembly'si yalnızca GridStrategy.Core'u görür.
        ///
        /// Kırmızıya dönerse "kod bozuldu" değil, "profilin katmanı değişti"
        /// demektir.
        /// </summary>
        [Test]
        public void Profile_IsAcceptedByTheCoreMoveFlow()
        {
            var board = new UnitGrid(width: 3, height: 5);
            var mover = new Unit("kral");
            board.PlaceUnit(1, 2, mover);

            MoveOutcome outcome = MoveAction.Execute(
                board, mover, 1, 2, 1, 3, new MoveProfile(range: 1));

            Assert.That(
                outcome,
                Is.EqualTo(MoveOutcome.Moved),
                "the move profile must be usable by Core's own move flow without any combat assembly");
        }
    }
}
