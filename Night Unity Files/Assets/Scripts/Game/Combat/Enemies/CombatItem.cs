using Game.Characters;
using Game.Characters.Player;
using Game.Combat.Enemies.EnemyTypes.Misc;
using SamsHelper;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Combat.Enemies
{
    public class CombatItem : Character
    {
        public readonly Number Distance = new Number(0, 0, 150);
        public const int ImmediateDistance = 1, CloseDistance = 10, MidDistance = 50, FarDistance = 100, MaxDistance = 150;
        public int Speed;
        protected float TargetDistance;
        public BasicEnemyView EnemyUi;
        
        public CombatItem(string name, int speed, int distance) : base(name)
        {
            Distance.SetCurrentValue(distance);
            Speed = speed;
            MovementController = new MovementController(this, Speed);
            SetDistanceData();
        }

        private void SetDistanceData()
        {
            Distance.AddThreshold(ImmediateDistance, "Immediate");
            Distance.AddThreshold(CloseDistance, "Close");
            Distance.AddThreshold(MidDistance, "Medium");
            Distance.AddThreshold(FarDistance, "Far");
            Distance.AddThreshold(MaxDistance, "Out of Range");
            Distance.AddOnValueChange(a =>
            {
                float distance = Helper.Round(Distance.CurrentValue());
                string distanceText = distance.ToString() + "m";
                EnemyUi.DistanceText.text = distanceText;
                float normalisedDistance = Helper.Normalise(distance, MaxDistance);
                float alpha = 1f - normalisedDistance;
                alpha *= alpha;
                alpha = Mathf.Clamp(alpha, 0.2f, 1f);
                EnemyUi.SetAlpha(alpha);
            });
        }
        
        protected bool Moving()
        {
            return TargetDistance != Distance.CurrentValue();
        }
        
        protected virtual void MoveToTargetDistance()
        {
            float currentDistance = Distance.CurrentValue();
            if (currentDistance > TargetDistance)
            {
                MovementController.Move(1);
                float newDistance = Distance.CurrentValue();
                if (!(newDistance <= TargetDistance)) return;
                ReachTarget();
            }
            else
            {
                MovementController.Move(-1);
                float newDistance = Distance.CurrentValue();
                if (!(newDistance >= TargetDistance)) return;
                ReachTarget();
            }
        }

        protected virtual void ReachTarget()
        {
            Distance.SetCurrentValue(TargetDistance);
            TargetDistance = -1;
        }
    }
}