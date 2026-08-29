using System.Collections.Generic;
using System.Globalization;
using System.Text;
using GridStrategy.Combat;

namespace GridStrategy.Unity
{
    // ═══ ROL: KURAL (Policy) ═════════════════════════════════════════
    // kimlik : yok — static; iki çağrıyı ayıracak bir şey yoktur
    // hafıza : yok — ölçüsü şu: aynı tanımla yapılan iki çağrı aynı metni
    //          verir. Diyaloğun kaç kez açıldığını, hangi birime bakıldığını
    //          bu tip SAYMAZ
    // Unity  : gerekmez — girdi düz C# tanımı, çıktı bir metin; sınamak için
    //          ne sahne ne kare gerekir
    // karar  : vermez, ÇEVİRİR — tasarımcının yazdığı sayıları oyuncunun
    //          okuyacağı satırlara döker
    /// <summary>
    /// Bir tür tanımının bilgi diyaloğunda görünen satırları.
    ///
    /// OYUNDA NE İŞE YARAR: bir birime ya da yapıya bakan oyuncu "canı kaç, ne
    /// kadar vuruyor, kaç hücreden atıyor, ne üretiyor" sorularının cevabını
    /// tek bir listede görüyor; sayıları öğrenmek için varlık dosyasını açmak
    /// gerekmiyor.
    ///
    /// Neyi BİLMEZ: diyaloğun açık olup olmadığını, hangi takımın baktığını,
    /// tahtanın ölçüsünü. Girdisi bir TÜR tanımıdır, tahtada duran bir örnek
    /// değil — bu yüzden canı azalmış bir askerin GÜNCEL canını da bilmez.
    /// </summary>
    // MonoBehaviour DEĞİL, ve gerekçe UnitOrderBook'unkiyle aynı: bu tip ne
    // Update alır ne sahneye bağlanır, bu yüzden EditMode'da doğrudan çağrılıp
    // sınanabiliyor. Satırlar diyaloğun İÇİNDE üretilseydi tek ölçüm yolu sahne
    // kurmak olurdu ve cümlelerin doğruluğu hiç sınanmazdı.
    //
    // ██ TAHTAYA HİÇ BAKMAMASI BİR KELEPÇE ██
    // Diyalog için yazılı yasak şu: boardRect, width, height ve BoardSizing
    // okunmayacak. Bu tip o yasağın taşıyıcısı — imzasında tahtadan gelen tek
    // bir sayı yok, dolayısıyla yasak bir hatırlatma değil bir DERLEME olgusu.
    public static class BlueprintSummary
    {
        /// <summary>Saldırı tanımı taşımayan bir türün satırlarında geçen cümle.</summary>
        // TEK SABİT, İKİ METİN DEĞİL: aynı cümle hem birimde hem yapıda geçiyor
        // ve kopyalansaydı biri düzeltildiğinde öteki sessizce eskirdi.
        public const string Unarmed = "Silahsız";

        /// <summary>Hiçbir birim üretmeyen bir yapının üretim satırında geçen cümle.</summary>
        public const string ProducesNothing = "Üretmiyor";

        // ██ ÜRETİLEN LİSTESİNİN YAZILI TAVANI ██
        // Bugün ölçülen en uzun liste iki üye taşıyor (Fabrika ve Karargâh).
        // Diyalog kaydırma taşıdığı için uzun bir liste metni KESMİYOR, yalnızca
        // uzatıyor; yani bu sayı bir kırılma noktası değil bir okunabilirlik
        // eşiği. Aşıldığı gün adlar tek tek yazılmayı bırakır ve yerine bir sayı
        // geçer.
        // TETİKLEYİCİ: bir yapının produces dizisi sekizi aştığı gün.
        public const int ReadableProducesCeiling = 8;

        // ONDALIK AYRACI KÜLTÜRE BIRAKILMIYOR ve gerekçe bir ölçüm: makinenin
        // kültürü değiştiğinde "1,5 sn" ile "1.5 sn" arasında gidip gelen bir
        // metin, onu sınayan testi de o makineye bağlar. Ekranda hangi ayracın
        // görüneceği bir tasarım sorusu ve sahibi bu tip değil; sabitlemek o
        // soruyu ERTELİYOR, cevabını vermiyor.
        private static readonly CultureInfo Fixed = CultureInfo.InvariantCulture;

        /// <summary>
        /// Bir birim türünün satırları.
        /// </summary>
        /// <param name="definition">Tür tanımı; <c>null</c> ise boş metin döner.</param>
        // NULL BİR İSTİSNA DEĞİL BOŞ METİN ÜRETİYOR, kural tiplerinin tersine —
        // ve ayrım çağıranın cinsinde: bu tipi bir oyun kuralı değil bir EKRAN
        // çağırıyor. Yarım kurulmuş bir varlık yüzünden diyaloğun hiç açılmaması,
        // eksik bir satır göstermekten daha kötü bir cevaptır.
        public static string Describe(UnitBlueprint definition)
        {
            if (definition == null)
            {
                return string.Empty;
            }

            var lines = new StringBuilder();
            AppendLine(lines, "Can", definition.MaxHealth.ToString(Fixed));
            AppendCombat(lines, definition.AttackProfile);
            return lines.ToString();
        }

        /// <summary>
        /// Bir yapı türünün satırları.
        /// </summary>
        /// <param name="definition">Tür tanımı; <c>null</c> ise boş metin döner.</param>
        // AŞIRI YÜKLEME, VE BURADA STRATEGY İDDİASI YOK: hangi sürümün
        // çağrılacağını derleyici girdinin tipinden biliyor, yani çalışma
        // zamanında seçilen hiçbir şey yok. Desen adının ölçüsü desen seçim
        // rehberinde yazılı ve bu satır o ölçüyü SAĞLAMIYOR.
        public static string Describe(StructureBlueprint definition)
        {
            if (definition == null)
            {
                return string.Empty;
            }

            var lines = new StringBuilder();
            AppendLine(lines, "Can", definition.MaxHealth.ToString(Fixed));
            AppendCombat(lines, definition.AttackProfile);
            AppendLine(lines, "Üretim süresi", Seconds(definition.ProductionSeconds));
            AppendProduces(lines, definition.Produces);
            return lines.ToString();
        }

        // ÜÇ SATIR TEK ÜYEDE VE AYRILMADILAR: üçü de aynı nesnenin yokluğunda
        // birlikte düşüyor. Ayrı ayrı yazılsaydı her birinde aynı null kontrolü
        // tekrarlanır ve biri unutulduğunda silahsız bir yapı diyaloğu
        // patlatırdı.
        private static void AppendCombat(StringBuilder lines, AttackProfile profile)
        {
            if (profile == null)
            {
                AppendLine(lines, "Saldırı", Unarmed);
                return;
            }

            AppendLine(lines, "Hasar", profile.Damage.ToString(Fixed));
            AppendLine(lines, "Menzil", profile.Range.ToString(Fixed) + " hücre");
            AppendLine(lines, "Bekleme", Seconds(profile.CooldownSeconds));
        }

        private static void AppendProduces(StringBuilder lines, IReadOnlyList<UnitBlueprint> produces)
        {
            if (produces == null || produces.Count == 0)
            {
                AppendLine(lines, "Üretir", ProducesNothing);
                return;
            }

            var names = new StringBuilder();
            for (int i = 0; i < produces.Count; i++)
            {
                if (produces[i] == null)
                {
                    continue;
                }

                if (names.Length > 0)
                {
                    names.Append(", ");
                }

                names.Append(produces[i].DisplayName);
            }

            // BOŞ GÖZLERDEN SONRA LİSTE BOŞ KALABİLİR ve o hâlin cevabı yine
            // "üretmiyor": dizinin uzunluğu değil, gösterilecek AD sayısı
            // belirliyor.
            AppendLine(lines, "Üretir", names.Length == 0 ? ProducesNothing : names.ToString());
        }

        // ETİKET İLE DEĞER ARASINDA İKİ NOKTA, BOŞLUKLA HİZALAMA DEĞİL: uGUI'nin
        // yerleşik fontu eş genişlikli değil, yani boşlukla kurulan bir sütun
        // ekranda eğri görünürdü. Hizalamanın doğru sahibi metin değil düzen
        // bileşenidir ve o gün geldiğinde etiket ile değer iki ayrı Text olur.
        private static void AppendLine(StringBuilder lines, string label, string value)
        {
            if (lines.Length > 0)
            {
                lines.Append('\n');
            }

            lines.Append(label).Append(": ").Append(value);
        }

        private static string Seconds(float value)
        {
            return value.ToString("0.0", Fixed) + " sn";
        }
    }
}
