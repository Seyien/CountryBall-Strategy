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
    //
    // SINIF, struct DEĞİL: soru "kopyalanır mı" değil, "kurucuya UĞRAMADAN
    // bir örnek doğabilir mi". default(MoveProfile) kurucuyu atlar ve Range
    // sıfır doğar; sıfır burada "kök salmış" demek olduğu için o örnek
    // KUSURSUZ görünür. readonly ve get-only bu kapıyı KAPATMAZ — ikisi de
    // değişmeyi engeller, doğuşu değil.
    //
    // DOSYA Core'da, ikizi AttackProfile Combat'ta: hareket menzili TAHTAYA
    // ait bir kavram, saldırı menzili SAVAŞA. Profil Combat'ta doğsaydı
    // MoveAction onu parametre olarak yazamazdı — Core, Combat'ı görmez.
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
    public sealed class MoveProfile
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
