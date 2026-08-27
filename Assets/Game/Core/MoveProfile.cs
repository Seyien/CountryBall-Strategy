using System;

namespace GridStrategy.Core
{
    // Bir birim türünün TUR BAŞINA hareket menzili: süvari 3 hücre gider, piyade
    // 1. Bu sayı yüzünden oyuncu 3 hücre öteye tıklayabilir, 4 hücre öteye
    // tıklayamazdı.
    //
    // BUGÜN ÜRETİMDE KULLANILMIYOR: tahta artık menzil değil ULAŞILABİLİRLİK
    // soruyor (PathFinder) ve oyuncu haritanın her yerine tıklayabiliyor. Tip
    // duruyor çünkü tur bazlı menzil isteyen bir mod geri gelebilir; o gün
    // sahibinin Combatant olması gerekir, bugünkü gibi her çağrıda yeniden
    // kurulan bir sayı olması değil.
    //
    // Core'da yaşar, ikizi AttackProfile Combat'ta: hareket menzili TAHTAYA ait
    // bir kavram, saldırı menzili SAVAŞA.
    // → MoveProfile.md#moveprofile-tip
    /// <summary>
    /// Bir hareket türünün değişmez tanımı: "süvari 3 hücre, piyade 1".
    /// <see cref="MoveAction"/>'ın bugün çıplak bir <c>int</c> olarak da
    /// aldığı hareket menzilinin sahibi — o dosyada "AttackProfile'ın ikizi
    /// olan bir MoveProfile" diye adı konmuş tipin kendisi.
    ///
    /// Neyi TUTMAZ: kimin hareket ettiğini, bu turda daha önce hareket edilip
    /// edilmediğini, yolun üzerinde ne olduğunu, birimin durumunu. Sonuncusu
    /// bilerek: "düşmüş birim hareket edebilir mi" sorusunun sahibi Combat
    /// katmanındaki MovementRules'tır ve bu tip onu tip olarak bile yazamaz.
    ///
    /// GEREKÇELER: Docs/deep/kod/Core/MoveProfile.md
    /// </summary>
    // DERİN ANLATIM: Docs/deep/konular/02-assembly-duvari.md
    // ÖĞRENME: Docs/ogrenme/02-sonraki-asamalar.md — yukarıdaki "tek örneği
    // paylaşabilir" bir İMKÂNdır: üretimde profil kuran TEK yer BattleActions
    // içindeki Move ve her çağrıda yeni bir örnek doğuyor, paylaşım sıfır.
    // ═══ record, class DEĞİL — VE SEBEBİ BU DOSYADA ZATEN YAZILIYDI ═══
    //
    // Yukarıdaki satırlar "3 menzil olan iki nesne AYNI ŞEYDİR" diyor. Bu bir
    // DEĞER semantiği iddiasıdır, ama düz bir sınıf onu UYGULAMAZ: iki ayrı
    // MoveProfile(3) nesnesi `==` ile karşılaştırıldığında false döner, çünkü
    // sınıflar kimliğe göre karşılaştırılır. Yani tip, kendi belgesinde yazan
    // şeyin tersini yapıyordu.
    //
    // `record` o boşluğu kapatır: derleyici Equals, GetHashCode ve == üyelerini
    // ALANLARDAN türetir, böylece MoveProfile(3) == MoveProfile(3) artık true.
    // Testlerde "aynı menzil mi" sorusu elle alan karşılaştırmadan sorulabilir.
    //
    // ÖLÇÜLDÜ: Unity 2021.3 C# 9 kullanıyor (üretilen csproj'da LangVersion 9.0).
    // record'un ihtiyaç duyduğu IsExternalInit tipi .NET Standard 2.1'de YOK —
    // ama yalnızca `init` erişimcisi ve konumsal (positional) record kullanılırsa
    // gerekiyor. Bu tip açık kurucu ve get-only property kullandığı için hiçbir ek
    // dosya gerekmiyor; derleme sınandı.
    //
    // `record struct` DEĞİL: o C# 10 gerektiriyor ve bu sürümde derlenmiyor.
    // Zaten istenmezdi de — struct, kurucuya UĞRAMADAN doğabilir (default) ve
    // aşağıdaki negatif menzil koruması atlanırdı.
    public sealed record MoveProfile
    {
        // SIFIR MENZİL GEÇERLİ, negatif değil. AttackProfile'ın "range < 1"
        // eşiği buraya KOPYALANMADI: hiçbir hücreye ulaşamayan bir SALDIRI
        // anlamsızdır, hiçbir hücreye gidemeyen bir BİRİM anlamlıdır — kök
        // salmış, sersemlemiş, kuşatılmış. Eşik 1'e çekilseydi menzilin iki
        // kapısı (int alan sürüm ve profil alan sürüm) ayrı kural olurdu.
        // → MoveProfile.md#moveprofileint-range
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
        /// → MoveProfile.md#range
        /// </summary>
        public int Range { get; }
    }
}
