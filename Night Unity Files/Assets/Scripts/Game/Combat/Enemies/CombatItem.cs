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

        protected virtual void SetDistanceData(BasicEnemyView enemyView)
        {
            Position.AddOnValueChange(a =>
            {
                float distance = Helper.Round(CombatManager.DistanceToPlayer(this));
                if (CombatManager.Player().Position.CurrentValue() > Position.CurrentValue()) distance = -distance;
                string distanceText = distance.ToString() + "m";
                enemyView.DistanceText.text = distanceText;

                float alpha = 0;
//                if (a.CurrentValue() > CombatManager.VisibilityRange)
//                {
//                    enemyView.SetNavigatable(false);
//                }
//                else
//                {
//                    enemyView.SetNavigatable(true);
                    float normalisedDistance = Helper.Normalise(distance, MaxDistance);
                    alpha = 1f - normalisedDistance;
                    alpha = Mathf.Clamp(alpha, 0.2f, 1f);
//                    Debug.Log(distance + " " + MaxDistance + " " + alpha);
//                }
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