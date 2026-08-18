using System;

namespace GridStrategy.Combat
{
    // ═══ ROL: KURAL (Policy) ═════════════════════════════════════════
    // kimlik : yok — static; iki IsWithinRange çağrısını ayıracak bir şey yoktur
    // hafıza : yok — IsWithinRange(2, profile) her zaman aynı cevabı verir
    // Unity  : gerekmez — mesafe hazır gelir, sınamak için tahta kurmak gerekmez
    // karar  : yalnızca ULAŞABİLİRLİĞİ hesaplar; vurmayı ne yapar ne emreder
    /// <summary>
    /// Saldırı kurallarının sahibi. <see cref="DamageRules"/> gibi hiçbir durum
    /// tutmaz: sayı ve tanım alır, cevap döndürür.
    ///
    /// MESAFEYİ KENDİ HESAPLAMAZ — dışarıdan hazır alır. Sebebi bilinçli:
    /// "iki hücre arası uzaklık nedir" ayrı bir oyun kuralıdır (Manhattan mı,
    /// Chebyshev mi, engeller sayılır mı). O kural buraya girseydi, menzil
    /// mantığını test etmek için önce bir tahta kurmak gerekirdi.
    ///
    /// Neyi BİLMEZ: hedefin asker mi baraka mı olduğunu, ölü olup olmadığını,
    /// sıranın kimde olduğunu. Bunlar hedef seçiminin işi. Buraya bir
    /// "hedef uygun mu" kontrolü eklemek, Health'e "hedef baraka mı" sormakla
    /// aynı hatadır: kendisine sorulmayan bir soruyu cevaplamak.
    /// </summary>
    public static class AttackResolver
    {
        /// <summary>
        /// Verilen uzaklık, bu saldırının menzili içinde mi?
        /// Yalnızca ULAŞABİLİRLİK söyler; vurmanın doğru olup olmadığını değil.
        /// </summary>
        public static bool IsWithinRange(int distance, AttackProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (distance < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(distance), distance, "Distance cannot be negative.");
            }

            // distance == 0 (aynı hücre) bilerek GEÇERLİ sayılıyor. "Kendine
            // saldırılır mı" bir hedef seçimi kuralıdır; menzil kuralı yalnızca
            // mesafeyi ölçer. Burada engelleseydik, ileride "kendi kendini
            // iyileştirme" gibi bir yetenek geldiğinde bu satırı geri almak
            // gerekirdi.
            return distance <= profile.Range;
        }
    }
}
