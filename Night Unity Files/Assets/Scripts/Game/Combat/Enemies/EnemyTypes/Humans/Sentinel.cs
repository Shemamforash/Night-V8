using System.Collections.Generic;
using Game.Characters;
using Game.Gear.Weapons;

namespace Game.Combat.Enemies.EnemyTypes.Humans
{
    public class Sentinel : DetailedEnemyCombat
    {
        private float _timeSinceLastHeal;
        private const float DefaultHealTime = 0.5f;
        private int _damageTaken;
        private bool _healingInCover;
        private int _targetHealAmount;

        public override void SetPlayer(Character enemy)
        {
            base.SetPlayer(enemy);
            MinimumFindCoverDistance = 5f;
            ArmourController.SetArmourValue(6);
        }
        
//        public override void ChooseNextAction()
//        {
//            if (!_healingInCover)
//            {
//                base.ChooseNextAction();
//                return;
//            } 
//            _healingInCover = false;
//            EnemyView.SetActionText("Bandaging Wounds");
//            CurrentAction = CoverAndHeal;
//        }

//        private void CoverAndHeal()
//        {
//            if(!InCover) TakeCover();
//            _timeSinceLastHeal += Time.deltaTime;
//            if (_timeSinceLastHeal < DefaultHealTime) return;
//            HealthController.Heal(1);
//            _targetHealAmount -= 1;
//            if (_targetHealAmount == 0)
//            {
//                ChooseNextAction();
//            }
//            _timeSinceLastHeal = 0;
//        }

//        public override void TakeCover()
//        {
//            base.TakeCover();
//            _timeSinceLastHeal = 0;
//        }
    }
}