using System;
using Game.Gear.Weapons;

namespace Game.Combat.Enemies.EnemyTypes
{
    public class Sniper : Enemy
    {
        private bool _reachedTarget;

        public Sniper(float position) : base(nameof(Sniper), position)
        {
            GenerateWeapon(WeaponType.Rifle);
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