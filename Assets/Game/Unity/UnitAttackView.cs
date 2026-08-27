using UnityEngine;

namespace GridStrategy.Unity
{
    /// <summary>
    /// Bir vuruşu EKRANDA görünür kılar: savaşçı silahını kaldırır, hedefe doğru
    /// hafifçe hamle yapar ve yerine döner.
    ///
    /// OYUNDA NE İŞE YARAR: oyuncunun "saldırım işlendi mi" sorusunu Console'a
    /// bakmadan cevaplayabilmesi için. Vuruş anında ekranda hiçbir şey
    /// değişmeseydi, isabet ile ret aynı görünürdü.
    ///
    /// BURADA YAŞAR, <see cref="UnitView"/>'da değil: hamle bir SÜREÇTİR ve
    /// hatırlaması gereken şeyler vardır (ne kadar kaldı, nereye dönecek).
    /// UnitView bilerek hafızasızdır. Aynı gerekçe <see cref="UnitWalker"/>'ı da
    /// ayrı tutuyor.
    ///
    /// KURALLARI HİÇ BİLMEZ: hasar çoktan hesaplandı, hedef çoktan düştü. Bu
    /// bileşen yarıda kesilse bile oyunun kaydı bozulmaz.
    /// </summary>
    public sealed class UnitAttackView : MonoBehaviour
    {
        // Hamlenin toplam süresi. Kısa tutuluyor: tur tabanlı bir oyunda vuruş
        // gösterimi sırayı BEKLETMEZ, çünkü tahta zaten güncellendi. Uzun bir
        // gösterim yalnızca oyuncuyu bekletirdi.
        private const float LungeSeconds = 0.28f;

        // Hedefe doğru gidilecek mesafe, hücre cinsinden. Bir hücrenin üçte biri:
        // hamle görünsün ama savaşçı komşu hücreye girmiş gibi durmasın.
        private const float LungeDistance = 0.33f;

        private UnitView view;
        private Vector3 restPosition;
        private Vector3 lungeOffset;
        private float remaining;

        /// <summary>
        /// Vuruş gösterimini başlatır.
        /// </summary>
        /// <param name="targetWorldPosition">
        /// Vurulan şeyin dünya konumu. Hamlenin YÖNÜNÜ bu belirler; savaşçı
        /// hedefe doğru gider, rastgele bir yöne değil.
        /// </param>
        public void Play(Vector3 targetWorldPosition)
        {
            if (view == null)
            {
                view = GetComponent<UnitView>();
            }

            // DİNLENME KONUMU HER VURUŞTA YENİDEN OKUNUR: birim iki vuruş
            // arasında yürümüş olabilir. Bir kez kaydedilseydi savaşçı vuruş
            // bitince eski hücresine geri sıçrardı.
            restPosition = transform.position;

            Vector3 toTarget = targetWorldPosition - restPosition;
            lungeOffset = toTarget.sqrMagnitude < 0.0001f
                ? Vector3.zero
                : toTarget.normalized * LungeDistance;

            remaining = LungeSeconds;

            if (view != null)
            {
                view.SetAttacking(true);
            }
        }

        /// <summary>
        /// Süren hamleyi iptal eder ve birimi olduğu yerde bırakır.
        /// </summary>
        // HAVUZ İÇİN VAR: geri verilen bir görselde yarım kalmış bir hamle
        // kalırsa, o görsel yeniden ödünç alındığında Update bir sonraki karede
        // birimi ESKİ sahibinin dinlenme konumuna geri çeker. Havuz kullanan
        // kodlarda en sık görülen hata sınıfı tam olarak budur.
        public void Cancel()
        {
            remaining = 0f;
        }

        private void Update()
        {
            if (remaining <= 0f)
            {
                return;
            }

            remaining -= Time.deltaTime;

            if (remaining <= 0f)
            {
                transform.position = restPosition;
                if (view != null)
                {
                    view.SetAttacking(false);
                }

                return;
            }

            // Gidip GERİ DÖNEN tek bir eğri: 0'da ve sonda sıfır, ortada bir.
            // İki ayrı aşama (git, dön) yazmak aynı hareketi iki durumla
            // anlatırdı; sinüs onu tek satırda veriyor.
            float progress = 1f - (remaining / LungeSeconds);
            float curve = Mathf.Sin(progress * Mathf.PI);
            transform.position = restPosition + (lungeOffset * curve);
        }
    }
}
