using UnityEngine;

namespace GridStrategy.Unity
{
    //   ═══ YARIŞ BAŞLANGIÇ IŞIĞI — BİR SANİYE, BİR LAMBA ═════════════
    //
    //     5 sn kaldi   ● ● ● ● ● ○      notr mavi, sakin
    //     3 sn kaldi   ● ● ● ○ ○ ○      KEHRIBAR + her sn bir vurus
    //     2 sn kaldi   ● ● ○ ○ ○ ○      kehribar, vurus
    //     1 sn kaldi   ● ○ ○ ○ ○ ○      kehribar, vurus
    //     HAZIR        ● ● ● ● ● ●      YESIL parlama, sonra kaybolur
    //
    //   Lamba sayisi = KALAN TAM SANIYE.

    // Yani "bir saniye bir saniye düşürtme" ayrı bir animasyon değil,
    // gösterimin KENDİSİ.

    /// <summary>
    /// Savaşçı üreten bir binanın tepesinde, bir sonraki üretime kaç saniye
    /// kaldığını gösteren geri sayım şeridi.
    ///
    /// OYUNDA NE İŞE YARAR: oyuncu kışlasının ne zaman yeni asker vereceğini
    /// binaya tıklayıp panele bakmadan, tahtaya bakarak görür — ve aynı şey
    /// düşman kışlası için de geçerli, yani baskının ne zaman geleceği artık
    /// tahmin değil.
    ///
    /// KENDİ KENDİNİ KURAR: lambalarını runtime'da üretir, prefab'da hazır
    /// beklemez. Gerekçe <see cref="HealthBarView"/>'daki ile birebir aynı —
    /// operatöre unutulabilir bir sürükleme borcu yazmamak.
    ///
    /// TAHTAYI HİÇ BİLMEZ: kendisine iki sayı söylenir (kalan ve toplam), onu
    /// çizer. Sayacın kime ait olduğu, kimin ne ürettiği bu dosyanın sorusu
    /// değil — o soruların sahibi <c>StructureProduction</c>.
    /// </summary>
    // ██ NEDEN YAZI DEĞİL, LAMBA ██
    // İlk şekil bir rakamdı ("3", "2", "1"). Ölçüm onu düşürdü: projede
    // TextMeshPro YOK (`Packages/manifest.json` yalnız `com.unity.ugui`
    // taşıyor) ve dünya uzayında rakam yazmak ya eski `TextMesh` ile gömülü
    // bir yazı tipine ya da her bina için ayrı bir dünya-uzayı Canvas'ına
    // bağlanmayı gerektirirdi. İkisi de tek bir sayıyı göstermek için ağır.
    //
    // Lamba dizisi aynı bilgiyi taşıyor VE operatörün istediği benzetmeyi
    // birebir veriyor: "son 3 saniye yarış araba oyunları olur ya o tarz."
    // Yarış ışığında da okunan şey rakam değil, KAÇ LAMBA kaldığıdır.
    public sealed class ProductionTimerView : MonoBehaviour
    {
        // Şeridin ölçüleri, dünya birimi. Can barıyla aynı genişlikte duruyor
        // ve bilerek: ikisi üst üste, aynı sütunda okunuyor — farklı genişlikte
        // olsalardı göz her ikisini ayrı ayrı hizalamak zorunda kalırdı.
        private const float StripWidth = 0.7f;
        private const float LampHeight = 0.09f;
        private const float LampGap = 0.02f;

        // ██ LAMBA SAYISININ TAVANI — VE SINIRIN DÜRÜST HÂLİ ██
        // Bugün en uzun üretim 5 saniye (`Structure_EnemyPump`,
        // `Structure_IndustrialPump`), yani tavan hiç tetiklenmiyor. Otuz
        // saniyelik bir bina eklendiği gün otuz lamba çizmek şeridi okunmaz
        // yapardı; tavan o günü karşılıyor ve davranışı tek cümlede: son ALTI
        // saniye tek tek sayılır, öncesi dolu görünür.
        //
        // SAYI 6, ÇÜNKÜ SON ÜÇ SANİYE ONUN İÇİNDE RAHAT DURUYOR: üçten küçük
        // bir tavan, operatörün istediği "son 3 saniye" vurgusunun kendisini
        // kırpardı.
        private const int MaxLamps = 6;

        // Vuruşun (punch) süresi. Kısa: bir saniyelik ritmin içinde iz bırakmalı
        // ama bir sonraki saniyeye taşmamalı.
        private const float PunchSeconds = 0.18f;
        private const float PunchScale = 1.45f;

        // Hazır olduğunda yeşil parlamanın süresi. Vuruştan uzun, çünkü bu
        // bir ritim değil bir HABER: "asker çıkabilir".
        private const float ReadyFlashSeconds = 0.45f;

        private static readonly Color IdleColour = new Color(0.35f, 0.55f, 0.85f);
        private static readonly Color CountdownColour = new Color(1f, 0.72f, 0.15f);
        private static readonly Color ReadyColour = new Color(0.25f, 0.90f, 0.35f);
        private static readonly Color DarkColour = new Color(0.10f, 0.10f, 0.13f, 0.85f);

        private SpriteRenderer[] lampRenderers;

        // Son çizilen lamba sayısı. Vuruşu TETİKLEYEN şey bu alanın değişmesi;
        // yani animasyon bir zamanlayıcıya değil, gösterilen sayının kendisine
        // bağlı. Zamanlayıcıya bağlansaydı sayaç durduğunda da vururdu.
        private int lastLit = -1;

        private float punchRemaining;
        private float flashRemaining;
        private bool built;

        /// <summary>
        /// Şeridi kurar. Aynı nesne üzerinde ikinci kez çağrılması zararsızdır.
        /// </summary>
        /// <param name="sprite">Düz beyaz bir kare; renk koddan veriliyor.</param>
        /// <param name="sortingOrder">
        /// Can barının ÜSTÜNDE olmalı, yoksa şerit barın arkasında kalır.
        /// </param>
        public void Build(Sprite sprite, int sortingOrder)
        {
            if (built)
            {
                return;
            }

            if (sprite == null)
            {
                Debug.LogError("[ProductionTimerView] No sprite given; the countdown cannot be drawn.", this);
                return;
            }

            Vector2 size = sprite.bounds.size;
            float spriteWidth = size.x > 0.0001f ? size.x : 1f;
            float spriteHeight = size.y > 0.0001f ? size.y : 1f;

            float lampWidth = (StripWidth - (LampGap * (MaxLamps - 1))) / MaxLamps;

            lampRenderers = new SpriteRenderer[MaxLamps];

            for (int i = 0; i < MaxLamps; i++)
            {
                var go = new GameObject($"Lamp{i}");
                Transform lamp = go.transform;
                lamp.SetParent(transform, worldPositionStays: false);

                // Soldan sağa diziliyor ve şerit ORTALANIYOR: sahibin merkezi
                // şeridin de merkezi olmalı, yoksa geniş bir bina üstünde
                // gösterge yana kayardı.
                float x = (-StripWidth * 0.5f) + (lampWidth * 0.5f) + (i * (lampWidth + LampGap));
                lamp.localPosition = new Vector3(x, 0f, 0f);
                lamp.localScale = new Vector3(lampWidth / spriteWidth, LampHeight / spriteHeight, 1f);

                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.color = DarkColour;
                renderer.sortingOrder = sortingOrder;

                lampRenderers[i] = renderer;
            }

            built = true;
        }

        /// <summary>
        /// Şeridin sahibinin merkezinden ne kadar yukarıda duracağını yazar.
        /// </summary>
        // AYRI BİR ÜYE, Build'in İÇİNDE BİR PARAMETRE DEĞİL — gerekçe
        // HealthBarView.SetHeightAboveOwner ile birebir aynı: havuzdan gelen
        // görselde şerit ZATEN kurulu ve Build erken dönüyor.
        public void SetHeightAboveOwner(float localHeight)
        {
            transform.localPosition = new Vector3(0f, localHeight, 0f);
        }

        /// <summary>
        /// Geri sayımı yazar: kaç saniye kaldı ve toplam kaç saniyeydi.
        /// </summary>
        /// <param name="remainingSeconds">Bir sonraki üretime kalan saniye.</param>
        /// <param name="totalSeconds">Bu binanın tam bekleme süresi.</param>
        // HER KAREDE ÇAĞRILIYOR ve bu bir israf değil: aşağıdaki dallar
        // değişmeyen karelerde hiçbir çizici alanına dokunmuyor. Aynı kelepçe
        // HealthBarView.SetFraction'da da var ve gerekçesi orada yazılı.
        public void SetRemaining(float remainingSeconds, float totalSeconds)
        {
            if (!built)
            {
                return;
            }

            // ██ TAM SANİYEYE YUKARI YUVARLANIYOR, AŞAĞI DEĞİL ██
            // 2,4 saniye kalmışken oyuncunun görmesi gereken şey "3 lamba"dır:
            // aşağı yuvarlansaydı 0,9 saniye kala gösterge SIFIR lamba gösterir
            // ve bina daha üretmemişken hazır görünürdü. Yukarı yuvarlama,
            // "kaç kere daha saniye başı geçecek" sorusunun tam cevabı.
            int lit = Mathf.Clamp(Mathf.CeilToInt(remainingSeconds), 0, MaxLamps);

            // Toplam süre lamba sayısını KIRPIYOR: iki saniyelik bir kışla altı
            // lambanın ikisini kullanır, dördü hiç yanmaz. Kırpılmasaydı iki
            // saniyelik bina ile beş saniyelik bina aynı boyda görünür ve
            // şeridin uzunluğu hiçbir şey anlatmazdı.
            int capacity = Mathf.Clamp(Mathf.CeilToInt(totalSeconds), 0, MaxLamps);

            bool ready = remainingSeconds <= 0f;

            if (lit != lastLit)
            {
                // İLK YAZIM VURMAZ: lastLit -1 ile başlıyor, yani bina doğduğu
                // karede gösterge sessizce yerine oturuyor. Vurmasaydı her yeni
                // bina, hiçbir şey olmadığı hâlde bir kez çakardı.
                if (lastLit >= 0)
                {
                    if (ready)
                    {
                        flashRemaining = ReadyFlashSeconds;
                    }
                    else
                    {
                        punchRemaining = PunchSeconds;
                    }
                }

                lastLit = lit;
                Paint(lit, capacity, ready);
            }
        }

        // ANİMASYON Update'TE, SetRemaining'İN İÇİNDE DEĞİL: vuruş bir saniye
        // boyunca değil, kendi süresi boyunca yaşıyor ve o süre kare sayısından
        // bağımsız. İçeride yazılsaydı sayaç durduğunda (bina hazırken) vuruş da
        // yarım kalırdı.
        private void Update()
        {
            if (!built)
            {
                return;
            }

            if (punchRemaining <= 0f && flashRemaining <= 0f)
            {
                return;
            }

            float scale = 1f;

            if (punchRemaining > 0f)
            {
                punchRemaining -= Time.deltaTime;

                // Sin eğrisi: 0'dan 1'e çıkıp 0'a dönüyor, yani vuruş sıçrayıp
                // yumuşakça yerine oturuyor. Doğrusal olsaydı bitişte bir
                // kesinti (pop) görünürdü.
                float t = Mathf.Clamp01(1f - (punchRemaining / PunchSeconds));
                scale = 1f + ((PunchScale - 1f) * Mathf.Sin(t * Mathf.PI));

                if (punchRemaining <= 0f)
                {
                    punchRemaining = 0f;
                    scale = 1f;
                }
            }

            if (flashRemaining > 0f)
            {
                flashRemaining -= Time.deltaTime;
                if (flashRemaining <= 0f)
                {
                    flashRemaining = 0f;

                    // PARLAMA BİTİNCE ŞERİT GERÇEKTEN KAYBOLUYOR: hazır bir bina
                    // için gösterecek bir sayı yok. Koyu renge çekilseydi
                    // "kaybolmuş" olmazdı — altı sönük kutu ekranda durmaya
                    // devam eder ve tahtayı okunmaz yapardı.
                    for (int i = 0; i < lampRenderers.Length; i++)
                    {
                        lampRenderers[i].color = Color.clear;
                    }
                }
            }

            // ██ ŞERİDİN KENDİSİ ÖLÇEKLENİYOR, LAMBALAR TEK TEK DEĞİL ██
            // Tek satır, altı yerine. Ve YALNIZ DİKEY: lambalar yan yana
            // duruyor, yatayda da büyüselerdi birbirlerinin üstüne binerlerdi.
            // Yükseklikte büyümek, komşusuna dokunmadan aynı "çakma" hissini
            // veriyor.
            transform.localScale = new Vector3(1f, scale, 1f);
        }

        private void Paint(int lit, int capacity, bool ready)
        {
            for (int i = 0; i < lampRenderers.Length; i++)
            {
                if (i >= capacity)
                {
                    // KAPASİTE DIŞI LAMBA TAMAMEN SAYDAM, koyu değil: koyu
                    // kalsaydı iki saniyelik bir kışla altı kutuluk bir şerit
                    // taşır ve oyuncu "dördü neden hiç yanmıyor" diye sorardı.
                    lampRenderers[i].color = Color.clear;
                    continue;
                }

                if (ready)
                {
                    lampRenderers[i].color = ReadyColour;
                    continue;
                }

                // SON ÜÇ SANİYE KEHRİBAR — operatörün istediği yarış ışığı tam
                // olarak bu eşik. Eşik lamba SAYISINA bakıyor, kalan saniyenin
                // kesirine değil: kesire bakılsaydı renk saniyenin ortasında
                // değişir ve vuruşla aynı ana düşmezdi.
                lampRenderers[i].color = i < lit
                    ? (lit <= 3 ? CountdownColour : IdleColour)
                    : DarkColour;
            }
        }
    }
}
