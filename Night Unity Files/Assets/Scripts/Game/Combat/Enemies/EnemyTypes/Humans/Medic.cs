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
            FindBetterRange();
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
            FindBetterRange();
            ShowMovementText = true;
            _recoverHealCooldown.Start();
        }
        
        private void TryHeal()
        {
            if (_healTarget == null || _healTarget.IsDead)
            {
                FindBetterRange();
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