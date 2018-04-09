namespace Game.Combat.Enemies.Humans
{
    public class Sentinel : EnemyBehaviour
    {
        private const float DefaultHealTime = 0.5f;
        private int _damageTaken;
        private bool _healingInCover;
        private int _targetHealAmount;
        private float _timeSinceLastHeal;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
//            MinimumFindCoverDistance = 5f;
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