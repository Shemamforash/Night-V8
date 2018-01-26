using System;
using Game.Gear.Weapons;

namespace Game.Combat.Enemies.EnemyTypes.Humans
{
    public class Mountain : Enemy
    {
        private bool _firedVolley;

        public Mountain(float position) : base(nameof(Mountain), 50, 2, position)
        {
            Weapon weapon = WeaponGenerator.GenerateWeapon(WeaponType.LMG);
            Equip(weapon);
            ArmourLevel.SetCurrentValue(4);
        }

        protected override Action ChooseNextAction()
        {
            if (!_firedVolley) return Aim();
            _firedVolley = false;
            return MoveToTargetDistance(DistanceToPlayer - 5);
        }
    }
}