using System;
using System.Collections.Generic;
using Game.Characters;
using Game.Combat.CharacterUi;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using UnityEngine;

namespace Game.Combat.Enemies.EnemyTypes
{
    public class Medic : DetailedEnemyCombat
    {
        private const int HealAmount = 50;
        private DetailedEnemyCombat _healTarget;

        public override void Initialise(Enemy enemy, EnemyUi characterUi)
        {
            base.Initialise(enemy, characterUi);
//            MinimumFindCoverDistance = 20f;
        }
        
        private void OnDestroy()
        {
            _healTarget?.ClearHealWait();
        }
        
        public void RequestHeal(DetailedEnemyCombat healTarget)
        {
            if (_healTarget != null) return;
            _healTarget = healTarget;
//            if (_healTarget == null || _healTarget.IsDead)
//            {
//                TryHeal();
//            }
        }
        
        private Action Heal()
        {
            float healTime = 2f;
            SetActionText("Healing " + _healTarget.Enemy.Name);
            return () =>
            {
                healTime -= Time.deltaTime;
                if (healTime > 0) return;
                _healTarget.ReceiveHealing(HealAmount);
                _healTarget = null;
                ChooseNextAction();
            };
        }

        protected override void ReachTarget()
        {
            base.ReachTarget();
            if (_healTarget != null)
            {
//                CurrentAction = Heal();
            }
        }
        
        private void TryHeal()
        {
//            CurrentAction = MoveToTargetPosition(_healTarget.Position.CurrentValue());
            SetActionText("Running to " + _healTarget.Enemy.Name);
        }

        public bool HasTarget()
        {
            return _healTarget != null;
        }
    }
}