using System;

namespace GridStrategy.Combat
{
    // ═══ ROL: KURAL (Policy) ═════════════════════════════════════════
    // kimlik : yok — static; iki IsWithinRange çağrısını ayıracak bir şey yoktur
    // hafıza : yok — IsWithinRange(2, profile) her zaman aynı cevabı verir
    // Unity  : gerekmez — mesafe hazır gelir, sınamak için tahta kurmak gerekmez
    // karar  : yalnızca ULAŞABİLİRLİĞİ hesaplar; vurmayı ne yapar ne emreder
    /// <summary>
    /// Saldırının ULAŞABİLİRLİK kuralı: verilen uzaklık menzile giriyor mu.
    /// <see cref="DamageRules"/> gibi hiçbir durum tutmaz — sayı ve tanım alır,
    /// cevap döndürür.
    ///
    /// MESAFEYİ KENDİ HESAPLAMAZ, dışarıdan hazır alır: "iki hücre arası uzaklık
    /// nedir" ayrı bir oyun kuralıdır (Manhattan mı, Chebyshev mi, engeller
    /// sayılır mı) ve buraya girseydi menzili sınamak için önce bir tahta kurmak
    /// gerekirdi.
    ///
    /// Neyi BİLMEZ: hedefin asker mi baraka mı olduğunu, ölü olup olmadığını,
    /// sıranın kimde olduğunu — bunlar hedef seçiminin işi.
    ///
    /// GEREKÇELER: Docs/deep/kod/Core/Combat/AttackResolver.md
    /// </summary>
    public static class AttackResolver
    {
        /// <summary>
        /// Verilen uzaklık, bu saldırının menzili içinde mi?
        /// Yalnızca ULAŞABİLİRLİK söyler; vurmanın doğru olup olmadığını değil.
        /// </summary>
        // MESAFE DIŞARIDAN GELİR: `distance` bir ÖLÇÜM ve tahtaya bağlıdır,
        // `profile.Range` bir TANIM ve tahtadan bağımsızdır. Koordinat alan bir
        // imza Manhattan/Chebyshev kararını buraya dondurur ve menzili sınamak
        // için önce bir tahta kurmayı zorunlu kılar. Duvarı kuran şey
        // `noEngineReferences` değil, asmdef'in BOŞ `references` listesidir.
        // → AttackResolver.md#iswithinrangeint-distance-attackprofile-profile
        // DERİN ANLATIM: Docs/deep/konular/02-assembly-duvari.md
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

            // distance == 0 (aynı hücre) bilerek GEÇERLİ. "Kendine uygulanır
            // mı" bir HEDEF SEÇİMİ kuralıdır; menzil kuralı yalnızca mesafeyi
            // ölçer. Engelleseydik, "kendi kendini iyileştirme" geldiği gün bu
            // satırı geri almak gerekirdi.
            // → AttackResolver.md#iswithinrangeint-distance-attackprofile-profile
            return distance <= profile.Range;
        }
    }
}
