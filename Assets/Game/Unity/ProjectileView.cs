using UnityEngine;

namespace GridStrategy.Unity
{
    /// <summary>
    /// Menzilli bir saldırıda uçan tek atımlık görsel: ok, mermi ya da büyücünün
    /// asasından çıkan parıltı.
    ///
    /// OYUNDA NE İŞE YARAR: uzaktan vuran bir birimin saldırısı bugüne kadar
    /// ekranda yalnız hedefin canında görünüyordu; oyuncu vuruşun NEREDEN
    /// geldiğini göremiyordu. Uçan bir görsel o bağı kuruyor — okçu ile hedefi
    /// arasındaki çizgi bir kez de gözle çiziliyor.
    ///
    /// KURALLARI HİÇ BİLMEZ, <see cref="UnitAttackView"/> ve
    /// <see cref="UnitWalker"/> ile aynı sözü veriyor: hasar çoktan hesaplandı,
    /// hedef çoktan düştü. Bu nesne yarıda silinse bile oyunun kaydı bozulmaz —
    /// nitekim hedefe varmadan yok edilmesi de mümkün ve sonucu yalnızca
    /// görülmeyen bir oktur.
    ///
    /// HAVUZ YOK ve bu ölçülmüş bir sadelik: mermi kare başına değil VURUŞ
    /// başına doğuyor, yani sayısı saldırı temposuyla sınırlı. Havuz, kazandığı
    /// tahsisten daha fazla durum (kimin elinde, temizlendi mi) getirirdi.
    /// Birikmemesini sağlayan şey havuz değil, varışta kendini yok etmesi.
    /// </summary>
    public sealed class ProjectileView : MonoBehaviour
    {
        // Uçuş hızı: saniyede kaç hücre. Hücreler 1 dünya birimi olduğu için
        // sayı doğrudan "saniyede kaç hücre" demek. Yüksek tutuldu: mermi bir
        // gösteri değil bir işaret, oyuncuyu bekletmemeli.
        private const float CellsPerSecond = 14f;

        // Ok hedefine "vardı" sayılma eşiği. Sıfır olamaz: kayan nokta
        // aritmetiğinde konum hedefe tam eşit olmaz ve mermi son karede titrerdi.
        private const float ArrivalThreshold = 0.03f;

        // Zemin 0, birim ve yapı 1, imleç çerçevesi 2, can barı 3. Mermi 4:
        // uçtuğu her şeyin üstünde görünsün, çünkü kısacık bir an yaşıyor ve o
        // an bir savaşçının arkasına düşerse hiç görülmez.
        private const int SortingOrder = 4;

        private Vector3 destination;
        private bool inFlight;

        /// <summary>
        /// Bir mermiyi doğurur ve yola çıkarır.
        /// </summary>
        /// <param name="parent">
        /// Merminin bağlanacağı nesne — tahtanın kendisi. Toplu yaşam döngüsü
        /// için: tahta yok olduğunda havada kalan mermiler de gider.
        /// </param>
        /// <param name="sprite">
        /// Uçacak görsel. <c>null</c> ise hiçbir şey doğmaz ve hiçbir hata
        /// basılmaz — atanmamış bir mermi simgesi oyunu oynanamaz yapmaz.
        /// </param>
        /// <returns>Doğan mermi; simge yoksa <c>null</c>.</returns>
        // FABRİKA STATIC, çünkü çağıranın elinde henüz bir nesne yok ve nesneyi
        // kurmanın üç adımı (GameObject, SpriteRenderer, ilk konum) çağıranda
        // kopyalansaydı ikinci bir çağıran doğduğu gün ikisi ayrışırdı.
        public static ProjectileView Fire(Transform parent, Sprite sprite, Vector3 from, Vector3 to)
        {
            if (sprite == null)
            {
                return null;
            }

            var go = new GameObject("Projectile");
            go.transform.SetParent(parent, worldPositionStays: false);
            go.transform.position = from;

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = SortingOrder;

            var projectile = go.AddComponent<ProjectileView>();
            projectile.Launch(to);
            return projectile;
        }

        /// <summary>
        /// Merminin uçuşunu başlatır.
        /// </summary>
        // AYRI BİR ÜYE, çünkü fabrika bileşeni AddComponent ile kuruyor ve
        // AddComponent argüman almaz. Public olması bir gereklilik değil bir
        // kolaylık olurdu, o yüzden değil.
        private void Launch(Vector3 worldDestination)
        {
            destination = worldDestination;
            inFlight = true;

            // OKUN BURNU HEDEFE BAKAR. Yön yalnızca göze hoş gelsin diye değil:
            // yatay uçan bir ok ile dikey uçan bir ok aynı çizildiğinde oyuncu
            // merminin nereye gittiğini tek karede okuyamıyordu.
            Vector3 toTarget = worldDestination - transform.position;
            if (toTarget.sqrMagnitude > 0.0001f)
            {
                float degrees = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, degrees);
            }
        }

        private void Update()
        {
            if (!inFlight)
            {
                return;
            }

            Vector3 position = transform.position;
            Vector3 toTarget = destination - position;
            float step = CellsPerSecond * Time.deltaTime;

            // Kalan mesafe bu karede atılacak adımdan küçükse hedefi AŞMADAN
            // oraya otur ve bit. Bu kontrol olmasaydı mermi hedefin bir o yana
            // bir bu yana geçip sonsuza dek titrerdi — UnitWalker'daki varış
            // kontrolüyle aynı kelepçe, aynı gerekçe.
            if (toTarget.sqrMagnitude <= (ArrivalThreshold * ArrivalThreshold)
                || toTarget.magnitude <= step)
            {
                transform.position = destination;
                inFlight = false;

                // KENDİNİ KAPATMAKLA YETİNMİYOR, KENDİNİ YOK EDİYOR: yalnız
                // kapatılsaydı her vuruş sahnede sessizce bir nesne daha
                // bırakırdı ve uzun bir savaşın sonunda tahta, görünmeyen
                // yüzlerce okun ebeveyni olurdu.
                Destroy(gameObject);
                return;
            }

            transform.position = position + (toTarget.normalized * step);
        }
    }
}
