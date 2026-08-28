using UnityEngine;

namespace GridStrategy.Unity
{
    /// <summary>
    /// Bir savaşçının ya da yapının canını başının üstünde gösteren şerit.
    ///
    /// OYUNDA NE İŞE YARAR: oyuncunun "bu düşman bir vuruşta düşer mi, yoksa iki
    /// mi gerekir" sorusunu Console'a bakmadan cevaplayabilmesi için. Can
    /// görünmediği sürece saldırı kararı kör bir tahmindir.
    ///
    /// KENDİ KENDİNİ KURAR: arka plan ve dolgu çocuklarını runtime'da üretir,
    /// prefab'da hazır beklemez. Gerekçe UnitWalker'daki ile aynı — operatöre
    /// unutulabilir bir sürükleme borcu yazmamak.
    ///
    /// TAHTAYI HİÇ BİLMEZ: kendisine bir ORAN söylenir, onu çizer. Canın kim
    /// tarafından, ne zaman azaldığı bu dosyanın sorusu değil.
    /// </summary>
    public sealed class HealthBarView : MonoBehaviour
    {
        // Şeridin ölçüleri, dünya birimi cinsinden. Bir hücre 1 birim olduğu için
        // bar hücrenin %70'i kadar geniş ve çok ince: birimi gölgelemesin ama
        // uzaktan da okunsun.
        private const float BarWidth = 0.7f;
        private const float BarHeight = 0.1f;

        // Yükseklik SÖYLENMEDİĞİNDE kullanılan sayı: bir hücre boyundaki sprite
        // için yarım hücrenin biraz üstü. Söylendiğinde yerini
        // <see cref="SetHeightAboveOwner"/> alıyor, çünkü sahibin ölçeği bu
        // sabiti çarpıyor ve büyük bir bina barını kendi içinde bırakıyordu.
        private const float HeightAboveUnit = 0.58f;

        private Transform fill;
        private SpriteRenderer fillRenderer;
        private float lastFraction = -1f;

        // Kaynak sprite'ın dünyadaki kendi boyutu. Ölçek bununla BÖLÜNÜYOR:
        // "0,7 birim geniş olsun" demek, sprite kaç piksel olursa olsun aynı
        // sonucu vermeli. Sabit bir ölçek yazsaydık PPU ayarı değiştiği gün bar
        // sessizce yanlış boyuta geçerdi.
        private Vector2 spriteUnitSize = Vector2.one;

        /// <summary>
        /// Barı kurar. Aynı nesne üzerinde ikinci kez çağrılması zararsızdır.
        /// </summary>
        /// <param name="sprite">Düz beyaz bir kare; renk koddan veriliyor.</param>
        /// <param name="sortingOrder">
        /// Birimin çizim sırasının ÜSTÜNDE olmalı, yoksa bar savaşçının arkasında
        /// kalır ve hiç görünmez.
        /// </param>
        public void Build(Sprite sprite, int sortingOrder)
        {
            if (fill != null)
            {
                return;
            }

            if (sprite == null)
            {
                Debug.LogError("[HealthBarView] No sprite given; the bar cannot be drawn.", this);
                return;
            }

            Vector2 size = sprite.bounds.size;
            spriteUnitSize = new Vector2(
                size.x > 0.0001f ? size.x : 1f,
                size.y > 0.0001f ? size.y : 1f);

            transform.localPosition = new Vector3(0f, HeightAboveUnit, 0f);

            // Arka plan koyu ve TAM GENİŞLİK: eksik canın nereye kadar gittiğini
            // ancak dolu olmayan kısım görünürse anlarsın.
            CreatePart("Background", sprite, new Color(0.12f, 0.05f, 0.05f, 0.9f),
                sortingOrder, BarWidth, out Transform _, out SpriteRenderer _);

            CreatePart("Fill", sprite, Color.green,
                sortingOrder + 1, BarWidth, out fill, out fillRenderer);

            // DOLGUNUN PİVOTU SOLDA: ölçek küçüldüğünde bar iki yandan değil
            // SAĞDAN kısalsın. Sprite'ın kendi pivotu ortada olduğu için
            // kaydırmayı ebeveyn yapıyor — sprite varlığına dokunmadan.
            SetFraction(1f);
        }

        /// <summary>
        /// Barın sahibinin merkezinden ne kadar yukarıda duracağını yazar.
        ///
        /// OYUNDA NE İŞE YARAR: bar her birimin ve her binanın tam tepesinde
        /// durur; büyük bir bina barını içinde bırakmaz, küçük bir asker barını
        /// havada asılı taşımaz.
        /// </summary>
        /// <param name="localHeight">
        /// Sahibinin YEREL uzayında yükseklik. Değerin dünya karşılığı sahibin
        /// ölçeğiyle çarpılır ve o çarpanı hesaba katmak çağıranın işi.
        /// </param>
        // AYRI BİR ÜYE, Build'in İÇİNDE BİR PARAMETRE DEĞİL: havuzdan gelen bir
        // görselde bar ZATEN kurulu ve Build erken dönüyor. Yükseklik orada da
        // yazılabilsin diye çağrı tek başına duruyor.
        public void SetHeightAboveOwner(float localHeight)
        {
            transform.localPosition = new Vector3(0f, localHeight, 0f);
        }

        /// <summary>
        /// Canı oran olarak yazar: 1 tam dolu, 0 bitmiş.
        /// </summary>
        public void SetFraction(float fraction)
        {
            if (fill == null)
            {
                return;
            }

            fraction = Mathf.Clamp01(fraction);

            // DEĞİŞMEDİYSE DOKUNMA: bu üye her karede çağrılıyor ve renk ataması
            // Unity tarafında bir malzeme güncellemesi tetikliyor. Karşılaştırma,
            // hiçbir şey olmayan karelerde o işi tamamen atlıyor.
            if (Mathf.Approximately(fraction, lastFraction))
            {
                return;
            }

            lastFraction = fraction;

            // Genişlik oranla ölçekleniyor ve dolgu, kalan kısmın ORTASINA
            // kaydırılıyor: sprite'ın pivotu ortada olduğu için sola yaslanmış
            // görünmesi ancak bu kaydırma ile sağlanır. Sonuç: bar soldan dolu
            // kalır, sağdan erir.
            float visibleWidth = BarWidth * fraction;
            fill.localScale = new Vector3(visibleWidth / spriteUnitSize.x, BarHeight / spriteUnitSize.y, 1f);
            fill.localPosition = new Vector3((visibleWidth - BarWidth) * 0.5f, 0f, 0f);

            // Yeşilden kırmızıya: renk, sayıyı okumadan durumu anlatır.
            fillRenderer.color = Color.Lerp(
                new Color(0.85f, 0.15f, 0.15f), new Color(0.25f, 0.85f, 0.30f), fraction);
        }

        private void CreatePart(
            string name, Sprite sprite, Color color, int sortingOrder, float width,
            out Transform partTransform, out SpriteRenderer partRenderer)
        {
            var go = new GameObject(name);
            partTransform = go.transform;
            partTransform.SetParent(transform, worldPositionStays: false);
            partTransform.localPosition = Vector3.zero;
            partTransform.localScale = new Vector3(
                width / spriteUnitSize.x, BarHeight / spriteUnitSize.y, 1f);

            partRenderer = go.AddComponent<SpriteRenderer>();
            partRenderer.sprite = sprite;
            partRenderer.color = color;
            partRenderer.sortingOrder = sortingOrder;
        }
    }
}
