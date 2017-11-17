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
        private Cooldown _applyHealCooldown;
        private Cooldown _recoverHealCooldown;

        public Medic() : base(nameof(Medic), 400)
        {
            Weapon weapon = WeaponGenerator.GenerateWeapon(new List<WeaponType>{WeaponType.Pistol, WeaponType.SMG});
            Equip(weapon);
            BaseAttributes.Endurance.SetCurrentValue(7);
            PreferredCoverDistance = 40f;
            MinimumFindCoverDistance = 20f;
            AlertOthers = true;
            _recoverHealCooldown = CombatManager.CombatCooldowns.CreateCooldown(5f);
            _applyHealCooldown = CombatManager.CombatCooldowns.CreateCooldown(2f);
            _applyHealCooldown.SetStartAction(() => SetActionText("Healing " + _healTarget.Name));
            _applyHealCooldown.SetEndAction(Heal);
        }

        protected override void Alert()
        {
            base.Alert();
            FindCover();
        }

        public override void UpdateBehaviour()
        {
            base.UpdateBehaviour();
            if (!IsAlerted()) return;
            if (_recoverHealCooldown.Running()) return;
            if (_healTarget == null) return;
            CurrentAction = TryHeal;
        }

        private void CheckForDamagedEnemies()
        {
            Enemy weakestEnemy = null;
            float weakestEnemyHealth = 100000;
            foreach(Enemy enemy in CombatManager.GetEnemies())
            {
                if (enemy == this) continue;
                if (!enemy.AcceptsHealing) continue;
                CharacterAttribute enemyHealth = BaseAttributes.Strength;
                Debug.Log(enemyHealth.GetCalculatedValue() + " " + enemyHealth.Max + " " + _healAmount);
                if (enemyHealth.GetCurrentValue() > enemyHealth.Max - _healAmount) continue;
                if (enemyHealth > weakestEnemyHealth) continue;
                weakestEnemyHealth = enemyHealth.GetCurrentValue();
                weakestEnemy = enemy;
            }
            _healTarget = weakestEnemy;
            if (weakestEnemy == null) return;
            CurrentAction = TryHeal;
            ShowMovementText = false;
        }

        public void RequestHeal(Enemy healTarget)
        {
            if (_healTarget != null)
            {
                _healTarget = healTarget;
            }
        }
        
        private void Heal()
        {
            _healTarget.BaseAttributes.Strength.Increment(_healAmount);
            _healTarget = null;
            FindCover();
            ShowMovementText = true;
            _recoverHealCooldown.Start();
        }
        
        private void TryHeal()
        {
            if (_healTarget.IsDead())
            {
                FindCover();
                ShowMovementText = true;
                return;
            }
            if (_applyHealCooldown.Running()) return;
            TargetDistance = _healTarget.Distance.GetCurrentValue();
            SetActionText("Running to " + _healTarget.Name);
            MoveToTargetDistance();
            if (!Moving()) _applyHealCooldown.Start();
        }
    }
}