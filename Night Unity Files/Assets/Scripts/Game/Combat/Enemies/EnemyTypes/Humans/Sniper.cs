using System;
using Game.Characters;
using Game.Gear.Weapons;

namespace Game.Combat.Enemies.EnemyTypes
{
    public class Sniper : DetailedEnemyCombat
    {
        private bool _reachedTarget;

        public override void SetPlayer(Character enemy)
        {
            base.SetPlayer(enemy);
            ArmourController.SetArmourValue(4);
        }

        protected override Action Aim()
        {
            Action aimAction = base.Aim();
            TakeCover();
            return aimAction;
        }
    }
}