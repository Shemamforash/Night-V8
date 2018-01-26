using System;
using System.Collections.Generic;
using Game.Gear.Weapons;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies.EnemyTypes.Humans
{
    public class Sentinel : Enemy
    {
        private float _timeSinceLastHeal;
        private const float DefaultHealTime = 0.5f;
        private int _damageTaken;
        private bool _healingInCover;
        private int _targetHealAmount;

        public Sentinel(float position) : base("Sentinel", 20, 4, position)
        {
            Weapon weapon = WeaponGenerator.GenerateWeapon(new List<WeaponType>{WeaponType.Shotgun, WeaponType.SMG});
            Equip(weapon);
            ArmourLevel.SetCurrentValue(6);
            MinimumFindCoverDistance = 5f;
            Speed = 5;
            HealthController.AddOnTakeDamage(damage =>
            {
                if (_healingInCover) return;
                _damageTaken += damage;
                if (_damageTaken < HealthController.GetMaxHealth() * 0.25f) return;
                _healingInCover = true;
                _targetHealAmount = (int) Random.Range(0, HealthController.GetMaxHealth() * 0.1f);
                CheckForRepositioning(true);
                _damageTaken = 0;
            });
        }
        
        protected override Action ChooseNextAction()
        {
            if (!_healingInCover) return Aim();
            _healingInCover = false;
            SetActionText("Bandaging Wounds");
            return CoverAndHeal;
        }

        private void CoverAndHeal()
        {
            if(!InCover) TakeCover();
            _timeSinceLastHeal += Time.deltaTime;
            if (_timeSinceLastHeal < DefaultHealTime) return;
            HealthController.Heal(1);
            _targetHealAmount -= 1;
            if (_targetHealAmount == 0)
            {
                CurrentAction = ChooseNextAction();
            }
            _timeSinceLastHeal = 0;
        }

        public override void TakeCover()
        {
            base.TakeCover();
            _timeSinceLastHeal = 0;
        }
    }
}