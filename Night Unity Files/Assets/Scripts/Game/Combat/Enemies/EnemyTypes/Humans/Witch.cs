using System.Collections.Generic;
using Game.Combat.Enemies.EnemyTypes.Misc;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using UnityEngine;

namespace Game.Combat.Enemies.EnemyTypes.Humans
{
    public class Witch : Enemy
    {
        private int _damageTaken;
        private readonly Cooldown _throwGrenadeCooldown;

        public Witch() : base("Witch", 5, 15)
        {
            int damageToReposition = 30;
            Weapon weapon = WeaponGenerator.GenerateWeapon(new List<WeaponType> {WeaponType.Pistol, WeaponType.SMG});
            Equip(weapon);
            PreferredCoverDistance = Random.RandomRange(30, 40);
            ArmourLevel.SetCurrentValue(4);
            MinimumFindCoverDistance = 5f;
            HealthController.AddOnTakeDamage(damage =>
            {
                _damageTaken += damage;
                if (_damageTaken < damageToReposition) return;
                CurrentAction = FindBetterRange;
                _damageTaken = 0;
            });
            _throwGrenadeCooldown = CombatManager.CombatCooldowns.CreateCooldown();
            _throwGrenadeCooldown.SetEndAction(ThrowGrenade);
            _throwGrenadeCooldown.SetDuringAction(t =>
            {
                if (!(t <= 2)) return;
                SetActionText("Throwing Grenade");
                CurrentAction = null;
            });
        }

        private void ThrowGrenade()
        {
            int currentPosition = (int) Distance.CurrentValue();
            switch (Random.Range(0, 3))
            {
                case 0:
                    CombatManager.AddGrenade(new Grenade(currentPosition, 0));
                    break;
                case 1:
                    CombatManager.AddGrenade(new SplinterGrenade(currentPosition, 0));
                    break;
                case 2:
                    CombatManager.AddGrenade(new IncendiaryGrenade(currentPosition, 0));
                    break;
            }
            StartGrenadeCooldown();
            CurrentAction = FindBetterRange;
        }

        private void StartGrenadeCooldown()
        {
            _throwGrenadeCooldown.Duration = Random.Range(5f, 10f);
            _throwGrenadeCooldown.Start();
        }

        protected override void Alert()
        {
            base.Alert();
            TargetDistance = PreferredCoverDistance;
            CurrentAction = MoveToTargetDistance;
            StartGrenadeCooldown();
        }
    }
}