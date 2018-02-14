using System;
using Game.Characters;
using Game.Gear.Weapons;

namespace Game.Combat.Enemies.EnemyTypes.Humans
{
    public class Mountain : DetailedEnemyCombat
    {
        private bool _firedVolley;

        public override void SetPlayer(Character enemy)
        {
            base.SetPlayer(enemy);
            MinimumFindCoverDistance = 20f;
            ArmourController.SetArmourValue(4);
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