using Game.Characters;
using Game.Characters.Player;
using Game.Combat.Enemies.EnemyTypes.Misc;
using SamsHelper;
using UnityEngine;

namespace Game.Combat.Enemies
{
    public class CombatItem : Character
    {
        public const int ImmediateDistance = 1, CloseDistance = 10, MidDistance = 50, FarDistance = 100, MaxDistance = 150;
        public int Speed;
        protected float TargetDistance;

        protected CombatItem(string name, int speed, float position) : base(name)
        {
            Position.SetCurrentValue(position);
            Speed = speed;
            MovementController = new MovementController(this, Speed);
        }

        public string DistanceToRange()
        {
            float distance = CombatManager.DistanceToPlayer(this);
            if (distance <= ImmediateDistance) return "Immediate";
            if (distance <= CloseDistance) return "Close";
            if (distance <= MidDistance) return "Medium";
            if (distance <= FarDistance) return "Far";
            return distance <= MaxDistance ? "Out of Range" : "Too far!";
        }
        
        protected virtual void SetDistanceData(BasicEnemyView enemyView)
        {
            Position.AddOnValueChange(a =>
            {
                float distance = Helper.Round(CombatManager.DistanceToPlayer(this));
                if (CombatManager.Player().Position.CurrentValue() > Position.CurrentValue()) distance = -distance;
                string distanceText = distance.ToString() + "m";
                enemyView.DistanceText.text = distanceText;
                float normalisedDistance = Helper.Normalise(distance, MaxDistance);
                float alpha = 1f - normalisedDistance;
                alpha *= alpha;
                alpha = Mathf.Clamp(alpha, 0.2f, 1f);
                enemyView.SetAlpha(alpha);
            });
        }
        
        protected bool Moving()
        {
            return TargetDistance >= 0;
        }
        
        protected virtual void MoveToTargetDistance()
        {
            float currentDistance = Position.CurrentValue();
            if (currentDistance > TargetDistance)
            {
                MovementController.MoveForward();
                float newDistance = Position.CurrentValue();
                if (!(newDistance <= TargetDistance)) return;
                ReachTarget();
            }
            else
            {
                MovementController.MoveBackward();
                float newDistance = Position.CurrentValue();
                if (!(newDistance >= TargetDistance)) return;
                ReachTarget();
            }
        }

        protected virtual void ReachTarget()
        {
            TargetDistance = -1;
        }
    }
}