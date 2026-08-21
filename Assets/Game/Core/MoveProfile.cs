using System;

namespace GridStrategy.Core
{
    // ═══ ROL: TANIM (Value/Definition) ═══════════════════════════════
    // kimlik : yok — "3 menzil" olan iki nesne aynı şeydir; bütün süvari
    //          sınıfı tek bir örneği paylaşabilir
    // hafıza : yok — değer kurucuda donar, Range her okumada aynı sayıdır
    // Unity  : gerekmez — düz C# nesnesi; Core'un asmdef'indeki
    //          noEngineReferences = true bu tiple bozulmaz
    // karar  : vermez — sayıyı TAŞIR; "oraya gidebilir mi" sorusunu
    //          MoveAction cevaplar, "kim hareket edebilir" sorusunu ise
    //          Combat'taki MovementRules
    /// <summary>
    /// Bir hareket türünün değişmez tanımı: "süvari 3 hücre, piyade 1".
    /// <see cref="MoveAction"/>'ın bugün çıplak bir <c>int</c> olarak aldığı
    /// hareket menzilinin sahibi — o dosyada "AttackProfile'ın ikizi olan bir
    /// MoveProfile" diye adı konmuş tipin kendisi.
    ///
    /// NEDEN CORE'DA, İKİZİ AttackProfile GİBİ COMBAT'TA DEĞİL:
    /// hareket menzili TAHTAYA ait bir kavramdır, saldırı menzili SAVAŞA.
    /// Hareketin ihtiyacı olan her şey — hücre, uzaklık, sınır — zaten
    /// Core'da yaşıyor; hasarın, takımın, yaşam döngüsünün hareketle hiçbir
    /// işi yok. Bu yüzden AttackProfile Combat'ta KALIR: onun taşıdığı Damage
    /// sayısının Core'da karşılığı olan bir kavram bile yoktur.
    ///
    /// Kararın mekanik yüzü de aynı yere çıkıyor: <see cref="MoveAction"/>
    /// Core'da ve Core, Combat'ı GÖRMEZ. Profil Combat'ta doğsaydı MoveAction
    /// onu parametre olarak alamazdı; almak için Core'un Combat'a referans
    /// vermesi gerekirdi ve iki assembly'yi ayrı tutmanın bütün gerekçesi
    /// çöpe giderdi.
    ///
    /// Yani ikiz, ikizinin bir kat ALTINDA yaşıyor. Bu asimetri bir kusur
    /// değil, kararın kendisidir.
    ///
    /// Neyi TUTMAZ: kimin hareket ettiğini, bu turda daha önce hareket edilip
    /// edilmediğini, yolun üzerinde ne olduğunu, birimin durumunu. Sonuncusu
    /// bilerek: "düşmüş birim hareket edebilir mi" sorusunun sahibi Combat
    /// katmanındaki MovementRules'tır ve bu tip onu tip olarak bile yazamaz.
    /// </summary>
    // REDDEDILEN - MoveProfile.cs:61 yerine:
    //     public readonly struct MoveProfile
    // KIRILAN  : AttackProfile'ın struct REDDEDILEN bloğundaki gerekçe (null koruması
    //            derlenmez olur) burada AYNI DEĞİL; asıl bedel SIFIRIN ANLAMLI OLMASI.
    //            default(MoveProfile) -> kurucu atlanır, Range sıfır doğar
    //            sıfır "kök salmış" demek -> kımıldamayan birim kusursuz derlenir
    //            derleyici: hiçbir şey der  .  test: hiçbiri kırmızıya dönmez
    // KAZANIRDI: profil paylaşılmayıp birim başına saklansaydı ve yol bulucu her
    //            karede binlerce hücre için okusaydı — o gün struct kazanırdı, ama
    //            önce sıfırın "atanmadı" ile karışmayacağı bir yol bulunmalıdır.
    // TEK CUMLE: struct'ın bedeli KOPYALANMASI değil kurucusunun ATLANABİLMESİDİR,
    //            ve varsayılan değer geçerli bir değerse bunu kimse fark etmez.
    //
    // Alternatif: dosyayı Combat/ altına, AttackProfile'ın yanına koymak.
    // Seçilmedi: Core, Combat'ı görmez ve MoveAction profili parametre olarak
    // alamaz — menzil bir gün SAVAŞ DURUMUNA bağlanana kadar da böyle kalır.
    //
    // Unit'e int MoveRange alanı eklemek seçeneği burada TEKRAR EDİLMİYOR:
    // o blok MoveAction.cs'te, hareket menzilinin nereden geldiğini anlatan
    // yerde zaten yazılı ve sonucu tam olarak bu tiptir.
    public sealed class MoveProfile
    {
        // MENZİL 0 BURADA GEÇERLİ, AttackProfile'da DEĞİL. Asimetri kasıtlı ve
        // gerekçesi MoveAction'da yazılı: hiçbir hücreye ulaşamayan bir SALDIRI
        // anlamsızdır, hiçbir hücreye gidemeyen bir BİRİM anlamlıdır — kök
        // salmış, sersemlemiş, kuşatılmış.
        //
        // REDDEDILEN - MoveProfile.cs:82 yerine (AttackProfile'ın kurucusundaki
        //              "range < 1" eşiği birebir kopyalanır):
        //     if (range < 1) throw new ArgumentOutOfRangeException(...);
        // KIRILAN  : "sıfır menzil geçerlidir" kararı iki yerde birbirine TERS
        //            yaşamaya başlar: int alan sürüm sıfırı kabul eder, profil alan
        //            sürüm sıfırlı bir profil KURAMAZ.
        //            aşırı yüklemeler -> aynı işin iki adı olmaktan çıkar
        //            derleyici: hiçbir şey der  .  test: _ZeroMoveRange_RejectsEveryStep
        //            profil tarafında yazılamaz
        // KAZANIRDI: "kımıldayamaz" ayrı bir durum olarak ifade edilseydi — Combat
        //            tarafındaki bir sersemletme etkisi bunu üstlenseydi; o gün menzil
        //            hep pozitif olur ve sıfır tek sayıya iki anlam yüklerdi.
        // TEK CUMLE: Aynı kavramın iki kapısı varsa ikisi de aynı değer kümesini
        //            kabul etmeli; yoksa aşırı yükleme değil, iki ayrı kural olurlar.
        public MoveProfile(int range)
        {
            if (range < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(range), range, "Range cannot be negative.");
            }

            Range = range;
        }

        /// <summary>
        /// Bir turda kaç hücre uzağa gidebildiği. Mesafenin nasıl ölçüldüğünü
        /// bilmez — o karar <see cref="GridDistance"/>'ın.
        /// 0 geçerlidir ve "yerinden kımıldayamaz" demektir.
        /// </summary>
        public int Range { get; }
    }
}
