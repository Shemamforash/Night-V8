using System;
using System.Collections.Generic;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using UnityEngine;

namespace Game.Combat.Enemies.EnemyTypes
{
    public class Medic : Enemy
    {
        private int _healAmount = 50;
        private Enemy _healTarget;

        public Medic(float position) : base(nameof(Medic), position)
        {
            GenerateWeapon(new List<WeaponType>{WeaponType.Pistol, WeaponType.SMG});
            MinimumFindCoverDistance = 20f;
            ArmourLevel.SetCurrentValue(2);
        }

        public override void Kill()
        {
            base.Kill();
            _healTarget?.ClearHealWait();
        }
        
        public void RequestHeal(Enemy healTarget)
        {
            if (_healTarget != null) return;
            _healTarget = healTarget;
            if (_healTarget == null || _healTarget.IsDead)
            {
                TryHeal();
            }
        }
        
        private Action Heal()
        {
            float healTime = 2f;
            EnemyView.SetActionText("Healing " + _healTarget.Name);
            return () =>
            {
                healTime -= Time.deltaTime;
                if (healTime > 0) return;
                _healTarget.ReceiveHealing(_healAmount);
                _healTarget = null;
                ChooseNextAction();
            };
        }

        protected override void ReachTarget()
        {
            base.ReachTarget();
            if (_healTarget != null)
            {
                CurrentAction = Heal();
            }
        }
        
        private void TryHeal()
        {
            CurrentAction = MoveToTargetPosition(_healTarget.Position.CurrentValue());
            EnemyView.SetActionText("Running to " + _healTarget.Name);
        }

        public bool HasTarget()
        {
            return _healTarget != null;
        }
    }
}