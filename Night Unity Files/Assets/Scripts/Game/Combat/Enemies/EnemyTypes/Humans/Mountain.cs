using System;
using Game.Gear.Weapons;

namespace Game.Combat.Enemies.EnemyTypes.Humans
{
    public class Mountain : Enemy
    {
        private bool _firedVolley;

        public Mountain(float position) : base(nameof(Mountain), position)
        {
            GenerateWeapon(WeaponType.LMG);
            ArmourLevel.SetCurrentValue(4);
        }

        public override void ChooseNextAction()
        {
            if (!_firedVolley)
            {
                base.ChooseNextAction();
                return;
            }
            _firedVolley = false;
            CurrentAction = MoveToTargetDistance(DistanceToPlayer - 5);
        }
    }
}