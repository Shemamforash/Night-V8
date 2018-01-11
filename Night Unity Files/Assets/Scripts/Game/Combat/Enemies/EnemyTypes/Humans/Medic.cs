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

        public Medic() : base(nameof(Medic), 4)
        {
            Weapon weapon = WeaponGenerator.GenerateWeapon(new List<WeaponType>{WeaponType.Pistol, WeaponType.SMG});
            Equip(weapon);
            Speed = 7;
            PreferredCoverDistance = 40f;
            MinimumFindCoverDistance = 20f;
            AlertOthers = true;
            _recoverHealCooldown = CombatManager.CombatCooldowns.CreateCooldown(5f);
            _applyHealCooldown = CombatManager.CombatCooldowns.CreateCooldown(2f);
            _applyHealCooldown.SetStartAction(() => SetActionText("Healing " + _healTarget.Name));
            _applyHealCooldown.SetEndAction(Heal);
            ArmourLevel.SetCurrentValue(2);
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
        }

        private void CheckForDamagedEnemies()
        {
            Enemy weakestEnemy = null;
            float weakestEnemyHealth = 100000;
            foreach(Enemy enemy in CombatManager.GetEnemies())
            {
                if (enemy == this) continue;
                if (!enemy.AcceptsHealing) continue;
                float enemyHealth = enemy.HealthController.GetCurrentHealth();
                Debug.Log(enemyHealth + " " + enemy.HealthController.GetMaxHealth() + " " + _healAmount);
                if (enemyHealth > enemy.HealthController.GetMaxHealth() - _healAmount) continue;
                if (enemyHealth > weakestEnemyHealth) continue;
                weakestEnemyHealth = enemyHealth;
                weakestEnemy = enemy;
            }
            _healTarget = weakestEnemy;
            if (weakestEnemy == null) return;
            CurrentAction = TryHeal;
            ShowMovementText = false;
        }

        public void RequestHeal(Enemy healTarget)
        {
            if (_healTarget != null && _recoverHealCooldown.Finished()) return;
            _healTarget = healTarget;
            CurrentAction = TryHeal;
        }
        
        private void Heal()
        {
            _healTarget.ReceiveHealing(_healAmount);
            _healTarget = null;
            FindCover();
            ShowMovementText = true;
            _recoverHealCooldown.Start();
        }
        
        private void TryHeal()
        {
            if (_healTarget == null || _healTarget.IsDead())
            {
                FindCover();
                ShowMovementText = true;
                return;
            }
            if (_applyHealCooldown.Running()) return;
            TargetDistance = _healTarget.Distance.CurrentValue();
            SetActionText("Running to " + _healTarget.Name);
            MoveToTargetDistance();
            if (!Moving()) _applyHealCooldown.Start();
        }
    }
}