using System.Collections.Generic;
using UnityEngine;

namespace GridStrategy.Unity
{
    /// <summary>
    /// Birimin EKRANDA yürümesini sağlar: verilen durakları sırayla, hızına bağlı
    /// olarak gezer.
    ///
    /// OYUNDA NE İŞE YARAR: oyuncu haritada bir yere tıkladığında savaşçının oraya
    /// ışınlanmasını değil YÜRÜMESİNİ görmesini sağlayan tek yer burasıdır.
    /// Yürürken bakış yönüne göre de döner, böylece hareket canlı görünür.
    ///
    /// BURADA YAŞAR, <see cref="UnitView"/>'da değil: yürüyüş bir SÜREÇTİR ve
    /// hatırlaması gereken şeyler vardır (kalan duraklar, hangisindeyiz). UnitView
    /// ise bilerek hafızasızdır — "seçili miyim", "ölü müyüm" bilgisini bile
    /// tutmaz. İkisini aynı dosyaya koymak o sözü bozardı.
    ///
    /// TAHTAYI DEĞİL EKRANI İLGİLENDİRİR: birim tahtada çoktan hedefe taşındı,
    /// bu bileşen yalnızca gecikmeli olarak onu takip ediyor. Bu yüzden yürüyüş
    /// yarıda kesilse bile oyun kuralları hiç bozulmaz.
    /// </summary>
    public sealed class UnitWalker : MonoBehaviour
    {
        // Hedefe "vardık" sayılma eşiği. Sıfır olamaz: kayan nokta aritmetiğinde
        // pozisyon hedefe tam eşit olmaz, birim son durakta titrer.
        private const float ArrivalThreshold = 0.02f;

        private readonly List<Vector3> waypoints = new List<Vector3>();
        private int nextWaypoint;
        private float unitsPerSecond;
        private SpriteRenderer body;

        /// <summary>
        /// Şu anda yürüyor mu? Yürüyen bir birime yeni emir vermek yolu baştan
        /// kurar; oyun kuralları bunu engellemez, çünkü tahta zaten güncel.
        /// </summary>
        public bool IsWalking => nextWaypoint < waypoints.Count;

        /// <summary>
        /// Yürüyüşü başlatır.
        /// </summary>
        /// <param name="worldPoints">
        /// Sırayla gidilecek DÜNYA konumları. Boşsa yürüyüş olmaz.
        /// </param>
        /// <param name="speed">
        /// Saniyede kaç Unity birimi. Hücreler 1 birim olduğu için bu sayı aynı
        /// zamanda "saniyede kaç hücre" demektir — Inspector'daki değeri
        /// büyütmek birimi gözle görülür biçimde hızlandırır.
        /// </param>
        public void Walk(IReadOnlyList<Vector3> worldPoints, float speed)
        {
            waypoints.Clear();
            nextWaypoint = 0;
            unitsPerSecond = speed;

            if (worldPoints == null || worldPoints.Count == 0)
            {
                return;
            }

            for (int i = 0; i < worldPoints.Count; i++)
            {
                waypoints.Add(worldPoints[i]);
            }

            // Hız sıfır ya da negatifse yürüyüş asla ilerlemez ve birim yolun
            // başında donup kalır — sessizce kaybolmasın diye bir kez bağır.
            if (unitsPerSecond <= 0f)
            {
                Debug.LogError(
                    "[UnitWalker] Move speed must be greater than zero; the unit would never arrive. " +
                    "Set 'Move Speed' on the Board component.",
                    this);
                SnapToEnd();
            }
        }

        /// <summary>
        /// Yürüyüşü iptal eder ve birimi son durağa oturtur. Birim sahneden
        /// kaldırılırken ya da tahta ile ekranın anında eşitlenmesi gerektiğinde
        /// kullanılır.
        /// </summary>
        public void SnapToEnd()
        {
            if (waypoints.Count > 0)
            {
                transform.position = waypoints[waypoints.Count - 1];
            }

            Cancel();
        }

        /// <summary>
        /// Yürüyüşü UNUTUR ve birimi OLDUĞU YERDE bırakır.
        /// </summary>
        // SnapToEnd'DEN FARKI TEK CÜMLE: o "varmış say", bu "hiç yürümemiş say".
        // Havuza geri verilen bir görsel için doğrusu bu — konumu zaten ödünç
        // alınırken yeniden yazılıyor ve araya giren bir sıçrama, bir sonraki
        // sahibinin ilk karesinde yanlış yerde görünmesine yol açardı.
        public void Cancel()
        {
            waypoints.Clear();
            nextWaypoint = 0;
        }

        private void Update()
        {
            if (!IsWalking)
            {
                return;
            }

            Vector3 target = waypoints[nextWaypoint];
            Vector3 position = transform.position;
            Vector3 toTarget = target - position;

            // Kalan mesafe bu karede atılacak adımdan küçükse hedefi AŞMADAN
            // oraya otur. Bu kontrol olmasaydı birim her karede hedefin bir o
            // yana bir bu yana geçip titrerdi.
            float step = unitsPerSecond * Time.deltaTime;
            if (toTarget.sqrMagnitude <= (ArrivalThreshold * ArrivalThreshold)
                || toTarget.magnitude <= step)
            {
                transform.position = target;
                nextWaypoint++;
                return;
            }

            Vector3 direction = toTarget.normalized;
            transform.position = position + (direction * step);
            FaceDirection(direction);
        }

        // Sağa giderken sprite'ı çevirmez, sola giderken çevirir. Sırf göze hoş
        // gelsin diye değil: yürüyüş yönünü görmek, oyuncunun emrinin işlendiğini
        // anlamasının en hızlı yolu.
        private void FaceDirection(Vector3 direction)
        {
            if (Mathf.Abs(direction.x) < 0.01f)
            {
                return;
            }

            if (body == null)
            {
                body = GetComponent<SpriteRenderer>();
            }

            if (body != null)
            {
                body.flipX = direction.x < 0f;
            }
        }
    }
}
