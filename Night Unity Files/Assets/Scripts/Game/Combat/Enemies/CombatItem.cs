using System;
using Game.Characters;
using Game.Characters.Player;
using Game.Combat.Enemies.EnemyTypes.Misc;
using SamsHelper;
using UnityEngine;
using static Game.Combat.CombatManager;

namespace Game.Combat.Enemies
{
    public abstract class CombatItem : Character
    {
        protected int Speed;
        public float DistanceToPlayer;

        protected CombatItem(string name, float position) : base(name)
        {
            Position.SetCurrentValue(position);
            MovementController = new MovementController(this, Speed);
        }

        protected virtual void SetDistanceData(BasicEnemyView enemyView)
        {
            Position.AddOnValueChange(a =>
            {
                if (IsDead) return;
                DistanceToPlayer = Position.CurrentValue() - CombatManager.Player.Position.CurrentValue();
                enemyView?.UpdateDistance();
            });
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

        public override void Kill()
        {
            
        }
    }
}