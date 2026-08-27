using System.Collections.Generic;
using UnityEngine;

namespace GridStrategy.Unity
{
    /// <summary>
    /// Savaşçı görsellerini yok etmek yerine SAKLAYIP yeniden kullanır.
    ///
    /// OYUNDA NE İŞE YARAR: oyuncu bunu doğrudan görmez — göreceği şey, uzun bir
    /// savaşta kareler arası takılmanın azalmasıdır. Yapılar sürekli birim
    /// üretiyor, ölenler temizleniyor; her doğum bir <c>Instantiate</c>, her ölüm
    /// bir <c>Destroy</c> demek olsaydı çöp toplayıcı düzenli olarak devreye girip
    /// oyunu bir an dondururdu.
    ///
    /// DÜZ C# SINIFI, MonoBehaviour DEĞİL: havuzun bir yaşam döngüsü geri
    /// çağrısına ihtiyacı yok, sahnede bir nesne olarak durmasına da. Böylece
    /// sahnesiz sınanabiliyor ve operatöre bir sürükleme borcu daha yazmıyor.
    ///
    /// TEMİZLİK SÖZLEŞMESİ: geri verilen bir görsel, YENİ DOĞMUŞ gibi görünmek
    /// zorundadır. Yürüyüşü durdurulur, saldırı pozu iner, seçim çerçevesi kapanır.
    /// Bu yapılmasaydı havuzdan çıkan birim, bir önceki sahibinin yarım kalmış
    /// yürüyüşünü sürdürürdü — havuz kullanan kodlarda en sık görülen hata budur.
    /// </summary>
    public sealed class UnitViewPool
    {
        private readonly UnitView prefab;
        private readonly Transform parent;
        private readonly Stack<UnitView> idle = new Stack<UnitView>();

        public UnitViewPool(UnitView prefab, Transform parent)
        {
            this.prefab = prefab;
            this.parent = parent;
        }

        /// <summary>Havuzda bekleyen görsel sayısı. Yalnız test ve teşhis için.</summary>
        public int IdleCount => idle.Count;

        /// <summary>Bugüne kadar kaç gerçek <c>Instantiate</c> yapıldığı.</summary>
        // ÖLÇÜ OLMADAN HAVUZ BİR İNANÇTIR: bu sayaç, havuzun gerçekten
        // çalıştığını gösteren tek kanıt. Doğan birim sayısından küçük kalıyorsa
        // yeniden kullanım oluyor demektir.
        public int CreatedCount { get; private set; }

        /// <summary>
        /// Bir görsel ödünç alır: havuzda varsa oradan, yoksa yeni doğurarak.
        /// </summary>
        public UnitView Rent(Vector3 position, string name)
        {
            UnitView view;

            if (idle.Count > 0)
            {
                view = idle.Pop();
                view.gameObject.SetActive(true);
            }
            else
            {
                view = Object.Instantiate(prefab, parent);
                CreatedCount++;
            }

            view.name = name;
            view.transform.position = position;

            // SIFIRLAMA ÖDÜNÇ ALIRKEN DE YAPILIYOR, yalnız geri verirken değil.
            // Sebep: havuzdan ÇIKMAYAN, ilk kez doğan bir görsel de tertemiz
            // başlamalı ve iki yolun aynı garantiyi vermesi gerekiyor.
            ResetVisualState(view);
            return view;
        }

        /// <summary>
        /// Görseli havuza geri verir. Sahneden silinmez, gizlenir.
        /// </summary>
        public void Return(UnitView view)
        {
            if (view == null)
            {
                return;
            }

            ResetVisualState(view);
            view.gameObject.SetActive(false);
            idle.Push(view);
        }

        // Yürüyüş ve saldırı bileşenleri runtime'da takıldığı için burada
        // OLMAYABİLİRLER; ikisi de null kontrolüyle geçiliyor.
        private static void ResetVisualState(UnitView view)
        {
            view.SetSelected(false);
            view.SetAttacking(false);

            UnitWalker walker = view.GetComponent<UnitWalker>();
            if (walker != null)
            {
                walker.Cancel();
            }

            UnitAttackView attack = view.GetComponent<UnitAttackView>();
            if (attack != null)
            {
                attack.Cancel();
            }
        }
    }
}
