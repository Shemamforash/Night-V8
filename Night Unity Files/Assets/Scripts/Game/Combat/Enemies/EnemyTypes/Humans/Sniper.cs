using System;
using Game.Gear.Weapons;

namespace Game.Combat.Enemies.EnemyTypes
{
    public class Sniper : Enemy
    {
        private bool _reachedTarget;

        public Sniper(float position) : base("Sniper", 7, 5, position)
        {
            Weapon sniperRifle = WeaponGenerator.GenerateWeapon(WeaponType.Rifle);
            Equip(sniperRifle);
            ArmourLevel.SetCurrentValue(4);
        }

        protected override Action Aim()
        {
            Action aimAction = base.Aim();
            TakeCover();
            return aimAction;
        }
    }
}