using Game.Characters;
using Game.Combat.Enemies;
using SamsHelper;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat
{
    public class EnemyPlayerRelation
    {
        public readonly Enemy Enemy;
        public readonly Player Player;
        public readonly MyValue Distance = new MyValue(0, 0, 150);
//        public readonly MyValue EnemyCover = new MyValue(0, 0, 100);
//        public readonly MyValue PlayerCover = new MyValue(0, 0, 100);
        private bool _hasFled, _isDead;
        private const float ImmediateDistance = 1f, CloseDistance = 10f, MidDistance = 50f, FarDistance = 100f, MaxDistance = 150f;
//        private ValueTextLink<string> PlayerSightLink = new ValueTextLink<string>();
//        private ValueTextLink<string> EnemySightLink = new ValueTextLink<string>();

        public EnemyPlayerRelation(Enemy enemy, Player player)
        {
            Enemy = enemy;
            Player = player;
            SetDistanceData();
//            EnemySightLink.AddTextObject(enemy.EnemyView().VisionText);
//            EnemyCover.AddThreshold(100, "Obscured");
//            EnemyCover.AddThreshold(80, "Barely Visible");
//            EnemyCover.AddThreshold(50, "Partial Sight");
//            EnemyCover.AddThreshold(20, "Clear shot");
//            EnemyCover.AddThreshold(00, "Totally Exposed");
//            PlayerSightLink.AddTextObject(enemy.EnemyView().CoverText);
//            PlayerCover.AddThreshold(100, "Obscured");
//            PlayerCover.AddThreshold(80, "Barely Visible");
//            PlayerCover.AddThreshold(50, "Partial Sight");
//            PlayerCover.AddThreshold(20, "Clear shot");
//            PlayerCover.AddThreshold(0, "Totally Exposed");
//            PlayerCover.AddOnValueChange(a => UpdateEnemySight());
//            PlayerCover.AddOnValueChange(a => UpdatePlayerCover());
        }

        private void SetDistanceData()
        {
            Distance.SetCurrentValue(Random.Range(25, 50));
            Distance.AddThreshold(ImmediateDistance, "Immediate");
            Distance.AddThreshold(CloseDistance, "Close");
            Distance.AddThreshold(MidDistance, "Medium");
            Distance.AddThreshold(FarDistance, "Far");
            Distance.AddThreshold(MaxDistance, "Out of Range");
            Distance.AddOnValueChange(a =>
            {
                if (_hasFled || _isDead) return;
                Enemy.EnemyView().DistanceText.text = Helper.Round(Distance.GetCurrentValue(), 0) + "m (" + a.GetThresholdName() + ")";
                float normalisedDistance = Distance.GetCurrentValue() / MaxDistance;
                float alpha = 1 - normalisedDistance;
                alpha *= alpha;
                if (alpha < 0)
                {
                    alpha = 0;
                }
                Enemy.EnemyView().SetColour(new Color(1,1,1,alpha));
                if (a.GetCurrentValue() <= MaxDistance) return;
                CombatManager.Scenario().Remove(Enemy);
                _hasFled = true;
            });
        }

//        private void UpdateEnemySight()
//        {
//            EnemySightLink.Value(EnemyCover.GetThresholdName() + " " + Helper.Round(100 - EnemyCover.GetCurrentValue(), 0) + "%");
//        }

//        private void UpdatePlayerCover()
//        {
//            PlayerSightLink.Value(PlayerCover.GetThresholdName() + " " + Helper.Round(PlayerCover.GetCurrentValue(), 0) + "%");
//        }

        public void MarkFled()
        {
            _hasFled = true;
        }

        public void UpdateRelation()
        {
            if (!_hasFled && !_isDead)
            {
                Enemy.UpdateBehaviour();
            }
        }
    }
}