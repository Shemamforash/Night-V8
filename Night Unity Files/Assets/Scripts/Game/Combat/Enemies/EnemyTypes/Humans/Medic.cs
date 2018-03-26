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
    public class Medic : EnemyBehaviour
    {
        private const int HealAmount = 50;
        private EnemyBehaviour _healTarget;

        public override void Initialise(Enemy enemy, EnemyUi characterUi)
        {
            base.Initialise(enemy, characterUi);
//            MinimumFindCoverDistance = 20f;
        }
        
        private void OnDestroy()
        {
            _healTarget?.ClearHealWait();
        }
        
        public void RequestHeal(EnemyBehaviour healTarget)
        {
            _healTarget = healTarget;
            MoveToCharacter(healTarget, Heal);
        }

        private void Heal()
        {
            float healTime = 2f;
            SetActionText("Healing " + _healTarget.Enemy.Name);
            CurrentAction = () =>
            {
                healTime -= Time.deltaTime;
                if (_healTarget == null)
                {
                    ChooseNextAction();
                    return;
                }

                if (healTime < 0) return;
                _healTarget.ReceiveHealing(HealAmount);
                _healTarget = null;
                ChooseNextAction();
            };
        }

        public bool HasTarget()
        {
            return _healTarget != null;
        }
    }
}