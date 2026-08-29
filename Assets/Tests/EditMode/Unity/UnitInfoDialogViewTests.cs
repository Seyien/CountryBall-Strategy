using System.Reflection;
using NUnit.Framework;
using GridStrategy.Battle;
using GridStrategy.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace GridStrategy.Tests.EditMode.Unity
{
    /// <summary>
    /// Bilgi penceresinin açılma, kapanma ve gizleme davranışı.
    ///
    /// <b>SAHNE KURULUYOR AMA SAVAŞ KURULMUYOR.</b> Ölçülen şey pencerenin kendi
    /// kararları — "ne zaman açılır, ne zaman kapanır, neyi gizler". Gerçek bir
    /// <c>BoardAdapter</c> kurulsaydı testler tahtanın kurulumunu sınar ve
    /// pencerenin mantığı o gürültünün altında kaybolurdu.
    ///
    /// <b>YANSIMA VAR VE SINIRI ÇİZİLİ:</b> yalnız <c>[SerializeField]</c>
    /// alanları yazmak ve savaş sonu geri çağrısını tetiklemek için. İkisi de
    /// üretimde motorun yaptığı işler ve testte onları yapacak başka bir yol yok.
    /// </summary>
    // `using System;` YASAK (CS0104: Object adı UnityEngine.Object ile
    // belirsizleşiyor); tam nitelikli yazılıyor. Bu kural projede ölçüldü.
    public sealed class UnitInfoDialogViewTests
    {
        private GameObject root;
        private GameObject dialog;
        private UnitInfoDialogView view;
        private Text title;
        private Image icon;
        private Text stats;
        private Text description;
        private UnitBlueprintAsset unitAsset;
        private StructureBlueprintAsset structureAsset;

        [SetUp]
        public void SetUp()
        {
            // ██ KÖK BOŞ, BİLEŞEN KÖKTE, KAPANAN NESNE ÇOCUK ██
            // Kurulum aracın kurduğu hiyerarşinin birebir aynısı. Bileşen
            // kapanan nesnenin üstüne konsaydı bu testlerin hiçbiri kırmızı
            // vermezdi ama oyunda pencere bir kez kapandıktan sonra Escape'i
            // duymaz olurdu — yani kurulum şeklinin kendisi ölçünün parçası.
            root = new GameObject("UnitInfoDialog");
            dialog = new GameObject("Dialog");
            dialog.transform.SetParent(root.transform);

            title = NewText("Title", dialog.transform);
            stats = NewText("Stats", dialog.transform);
            description = NewText("Description", dialog.transform);

            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(dialog.transform);
            icon = iconGo.AddComponent<Image>();

            view = root.AddComponent<UnitInfoDialogView>();
            Set("dialog", dialog);
            Set("titleLabel", title);
            Set("iconImage", icon);
            Set("statsLabel", stats);
            Set("descriptionLabel", description);

            unitAsset = ScriptableObject.CreateInstance<UnitBlueprintAsset>();
            structureAsset = ScriptableObject.CreateInstance<StructureBlueprintAsset>();

            view.Close();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Object.DestroyImmediate(unitAsset);
            UnityEngine.Object.DestroyImmediate(structureAsset);
        }

        // ══ AÇILMA ═══════════════════════════════════════════════════════

        /// <summary>
        /// Bir birim türü gösterildiğinde pencere açılıyor ve satırlar
        /// yazılıyor.
        /// </summary>
        [Test]
        public void Show_AUnitType_OpensTheWindowAndWritesItsNumbers()
        {
            view.Show(unitAsset);

            Assert.That(view.IsOpen, Is.True);
            Assert.That(title.text, Is.EqualTo(unitAsset.DisplayName));
            Assert.That(stats.text, Does.Contain("Can: "));
        }

        /// <summary>
        /// Bir yapı türü gösterildiğinde üretim satırı da geliyor.
        /// </summary>
        // İKİ AŞIRI YÜKLEMENİN İKİSİ DE SINANMIŞ OLUYOR: yalnız biri
        // sınansaydı, ötekinin alanları yazmayı unutması sessiz kalırdı.
        [Test]
        public void Show_AStructureType_AlsoWritesTheProductionLine()
        {
            view.Show(structureAsset);

            Assert.That(view.IsOpen, Is.True);
            Assert.That(stats.text, Does.Contain("Üretim süresi"));
        }

        /// <summary>
        /// Gösterilecek tür yoksa pencere AÇILMIYOR.
        /// </summary>
        // BOŞ BİR PENCERE AÇMAK, HİÇ AÇMAMAKTAN KÖTÜ: oyuncu kapatması gereken
        // ama hiçbir şey söylemeyen bir modalla baş başa kalırdı.
        [Test]
        public void Show_WithNothingToShow_LeavesTheWindowClosed()
        {
            view.Show((UnitBlueprintAsset)null);
            Assert.That(view.IsOpen, Is.False);

            view.Show((StructureBlueprintAsset)null);
            Assert.That(view.IsOpen, Is.False);
        }

        // ══ GİZLEME — İKİ ALAN BOŞ DOĞUYOR ═══════════════════════════════

        /// <summary>
        /// Simgesi olmayan tür için simge nesnesi GİZLENİYOR, boş kare
        /// bırakılmıyor.
        /// </summary>
        // ATANMAMIŞ BİR Image BEYAZ BİR DİKDÖRTGEN ÇİZER ve oyuncu onu bir simge
        // sanar. Varlık dosyası simgeyi boş bırakmaya zaten izin veriyor; bu dal
        // o iznin ekrandaki karşılığı.
        [Test]
        public void Show_ATypeWithoutAnIcon_HidesTheIconInsteadOfDrawingAnEmptyBox()
        {
            view.Show(unitAsset);

            Assert.That(unitAsset.Icon, Is.Null, "varsayılan varlık simgesiz doğar");
            Assert.That(icon.gameObject.activeSelf, Is.False);
        }

        /// <summary>
        /// Açıklaması yazılmamış tür için açıklama etiketi GİZLENİYOR.
        /// </summary>
        // ██ BUGÜN BU DAL KURALIN KENDİSİ ██
        // Türlerin açıklama metinleri operatörden bekleniyor ve hiçbiri henüz
        // yazılmadı. Etiket gizlenmeseydi pencere, sebebi görünmeyen bir boşlukla
        // açılırdı.
        [Test]
        public void Show_ATypeWithoutADescription_HidesTheDescriptionLabel()
        {
            view.Show(unitAsset);

            Assert.That(unitAsset.Description, Is.Null.Or.Empty);
            Assert.That(description.gameObject.activeSelf, Is.False);
        }

        // ══ KAPANMA ══════════════════════════════════════════════════════

        /// <summary>
        /// Kapatma açık bir pencereyi kapatıyor.
        /// </summary>
        // ÜÇ KAPANMA YOLUNUN TEK VARIŞ NOKTASI BU ÜYE: çarpı düğmesi ve perde
        // araç tarafından buraya bağlanıyor, Escape ise Update'ten. Üçü ayrı
        // ayrı yazılsaydı biri düzeltildiğinde ötekiler eskirdi.
        [Test]
        public void Close_OnAnOpenWindow_ShutsIt()
        {
            view.Show(unitAsset);
            Assert.That(view.IsOpen, Is.True);

            view.Close();

            Assert.That(view.IsOpen, Is.False);
        }

        /// <summary>
        /// Savaş bittiğinde pencere kendini kapatıyor.
        /// </summary>
        // ██ AYNI ANDA TEK MODAL — SÖZLEŞMENİN ÖLÇÜSÜ ██
        // Savaş sonu panosu ekranı kaplıyor. Bu pencere açık kalsaydı iki modal
        // üst üste biner ve panonun yeniden başlat düğmesi bu pencerenin
        // perdesinin altında kalabilirdi. Geri çağrı yansımayla tetikleniyor
        // çünkü üretimde onu tetikleyen şey tahtanın olayı.
        [Test]
        public void WhenTheBattleEnds_TheWindowClosesItself()
        {
            view.Show(unitAsset);
            Assert.That(view.IsOpen, Is.True);

            RaiseBattleEnded(BattleOutcome.PlayerWon);

            Assert.That(view.IsOpen, Is.False);
        }

        /// <summary>
        /// Pencere nesnesi hiç atanmamışsa cevap "kapalı" — istisna değil.
        /// </summary>
        // YARIM KURULMUŞ BİR SAHNE PATLAMAMALI: araç her alanı yazıyor, ama bir
        // sahne aracın eski bir sürümüyle kurulmuş olabilir ve o hâlde pencere
        // yalnızca görünmez — oyun sürüyor.
        [Test]
        public void IsOpen_WithNoDialogAssigned_IsFalseInsteadOfThrowing()
        {
            Set("dialog", null);

            Assert.That(view.IsOpen, Is.False);
            Assert.DoesNotThrow(() => view.Show(unitAsset));
            Assert.DoesNotThrow(view.Close);
        }

        // ══ YARDIMCILAR ══════════════════════════════════════════════════

        private static Text NewText(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            return go.AddComponent<Text>();
        }

        private void Set(string field, object value)
        {
            FieldInfo info = typeof(UnitInfoDialogView).GetField(
                field, BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(info, Is.Not.Null, field + " alanı bulunamadı");
            info.SetValue(view, value);
        }

        private void RaiseBattleEnded(BattleOutcome outcome)
        {
            MethodInfo handler = typeof(UnitInfoDialogView).GetMethod(
                "OnBattleEnded", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(handler, Is.Not.Null, "OnBattleEnded bulunamadı");
            handler.Invoke(view, new object[] { outcome });
        }
    }
}
