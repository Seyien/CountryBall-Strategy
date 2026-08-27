using System.Collections.Generic;
using GridStrategy.Combat;
using GridStrategy.Core;
using NUnit.Framework;

namespace GridStrategy.Tests.EditMode.Battle
{
    /// <summary>
    /// "Düşmana tıkla, yanına gidip vursun" akışının ve can barının dayandığı
    /// çekirdek yüzeyi koruyan testler.
    ///
    /// Ekranın çizdiği iki şey burada sınanıyor: yaklaşma hücresini bulmak için
    /// gereken YOL SORUSU (Battle.TryFindPath) ve can barının oranını hesaplamak
    /// için gereken TAVAN DEĞERİ (MaxHealth). İkisi de savaşın defterinden
    /// okunuyor; biri sessizce yanlış cevap verirse ekranda ya yanlış yere
    /// yürünür ya da bar hep dolu görünür.
    /// </summary>
    public sealed class ApproachAndHealthTests
    {
        private static GridStrategy.Battle.Battle NewBattle(int width = 6, int height = 6)
        {
            return new GridStrategy.Battle.Battle(width, height);
        }

        private static Combatant NewCombatant(Team team, int maxHealth = 30)
        {
            return new Combatant(
                new Health(maxHealth),
                new UnitLifecycle(),
                new AttackProfile(10, 1),
                team);
        }

        private static Unit AddUnit(
            GridStrategy.Battle.Battle battle, string name, Team team, int x, int y, int maxHealth = 30)
        {
            var unit = new Unit(name);
            battle.AddUnit(unit, NewCombatant(team, maxHealth), x, y);
            return unit;
        }

        [Test]
        public void TryFindPath_ThroughTheBattle_FindsAnOpenRoute()
        {
            // Ekran, hedefe yaklaşacak hücreyi seçerken bu kapıyı kullanıyor.
            // Kapalı olsaydı yaklaşma hiç hesaplanamazdı.
            GridStrategy.Battle.Battle battle = NewBattle();
            Unit walker = AddUnit(battle, "walker", Team.Player, 0, 0);

            bool found = battle.TryFindPath(walker, 0, 0, 4, 4, out List<GridStep> path);

            Assert.IsTrue(found);
            Assert.IsNotEmpty(path);
            Assert.AreEqual(4, path[path.Count - 1].X);
            Assert.AreEqual(4, path[path.Count - 1].Y);
        }

        [Test]
        public void TryFindPath_DoesNotMoveAnyone()
        {
            // SORU SORMAK TAHTAYI DEĞİŞTİRMEZ. Yaklaşma hücresi seçilirken bu
            // kapı ONLARCA kez çağrılıyor; her çağrı tahtayı oynatsaydı tek bir
            // tıklama savaşı altüst ederdi.
            GridStrategy.Battle.Battle battle = NewBattle();
            Unit walker = AddUnit(battle, "walker", Team.Player, 1, 1);

            battle.TryFindPath(walker, 1, 1, 5, 5, out List<GridStep> _);

            Assert.IsTrue(battle.TryGetPosition(walker, out int x, out int y));
            Assert.AreEqual(1, x);
            Assert.AreEqual(1, y);
        }

        [Test]
        public void TryFindPath_AroundAnotherUnit_AvoidsTheOccupiedCell()
        {
            GridStrategy.Battle.Battle battle = NewBattle();
            Unit walker = AddUnit(battle, "walker", Team.Player, 0, 2);
            AddUnit(battle, "blocker", Team.Enemy, 1, 2);

            bool found = battle.TryFindPath(walker, 0, 2, 2, 2, out List<GridStep> path);

            Assert.IsTrue(found);
            foreach (GridStep step in path)
            {
                Assert.IsFalse(step.X == 1 && step.Y == 2, "Yol dolu hücreden geçiyor.");
            }
        }

        [Test]
        public void CombatantMaxHealth_IsTheAuthoredCeiling_AndDoesNotDropWithDamage()
        {
            // Can barı oranı CurrentHealth / MaxHealth. Tavan hasarla birlikte
            // düşseydi bar hep dolu görünür ve hiçbir vuruş ekranda okunmazdı.
            Combatant combatant = NewCombatant(Team.Player, maxHealth: 40);

            Assert.AreEqual(40, combatant.MaxHealth);
            Assert.AreEqual(40, combatant.CurrentHealth);

            combatant.TakeDamage(15);

            Assert.AreEqual(40, combatant.MaxHealth, "Tavan hasarla değişmemeli.");
            Assert.AreEqual(25, combatant.CurrentHealth);
        }

        [Test]
        public void StructureMaxHealth_IsTheAuthoredCeiling()
        {
            var structure = new Structure(
                new Health(55), new StructureLifecycle(), Team.Player);

            Assert.AreEqual(55, structure.MaxHealth);

            structure.TakeDamage(20);

            Assert.AreEqual(55, structure.MaxHealth);
            Assert.AreEqual(35, structure.CurrentHealth);
        }
    }
}
