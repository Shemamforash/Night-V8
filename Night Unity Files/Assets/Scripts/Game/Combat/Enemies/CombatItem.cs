using System;
using Game.Characters;
using Game.Characters.Player;
using Game.Combat.Enemies.EnemyTypes.Misc;
using SamsHelper;
using UnityEngine;

namespace Game.Combat.Enemies
{
    public abstract class CombatItem : Character
    {
        protected const int MaxDistance = 150;
        protected int Speed;
        private const float AlphaCutoff = 0.2f;
        private const float FadeVisibilityDistance = 2f;
        public float DistanceToPlayer;

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
                if (IsDead) return;
                SetDistanceText(enemyView);
                CalculateAlphaFromDistance(enemyView);
                DistanceToPlayer = Math.Abs(Position.CurrentValue() - CombatManager.Player().Position.CurrentValue());
            });
        }

        private void SetDistanceText(BasicEnemyView enemyView)
        {
            float distance = Helper.Round(DistanceToPlayer);
            if (CombatManager.Player().Position.CurrentValue() > Position.CurrentValue()) distance = -distance;
            string distanceText = distance + "m";
            enemyView.DistanceText.text = distanceText;
        }

        private void CalculateAlphaFromDistance(BasicEnemyView enemyView)
        {
            float distanceToMaxVisibility = CombatManager.VisibilityRange + FadeVisibilityDistance - DistanceToPlayer;
            float alpha = 0;
            if (DistanceToPlayer < CombatManager.VisibilityRange)
            {
                float normalisedDistance = Helper.Normalise(DistanceToPlayer, CombatManager.VisibilityRange);
                alpha = 1f - normalisedDistance;
                alpha = Mathf.Clamp(alpha, AlphaCutoff, 1f);
            }
            else if (distanceToMaxVisibility >= 0)
            {
                alpha = Helper.Normalise(distanceToMaxVisibility, FadeVisibilityDistance);
                alpha = Mathf.Clamp(alpha, 0, AlphaCutoff);
            }

            enemyView.SetNavigatable(alpha > 0);
            enemyView.SetAlpha(alpha);
        }

        protected Action MoveToTargetPosition(float position)
        {
            if (Position.CurrentValue() > position)
            {
                return () =>
                {
                    MovementController.MoveForward();
                    if (Position.CurrentValue() > position) return;
                    Position.SetCurrentValue(position);
                    ReachTarget();
                };
            }
            return () =>
            {
                MovementController.MoveBackward();
                if (Position.CurrentValue() < position) return;
                Position.SetCurrentValue(position);
                ReachTarget();
            };
        }

        protected abstract void ReachTarget();
    }
}