namespace Game.Combat.Enemies.Humans
{
    public class Mountain : EnemyBehaviour
    {
        private bool _firedVolley;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
//            MinimumFindCoverDistance = 20f;
        }

        public override void ChooseNextAction()
        {
            if (!_firedVolley)
            {
                base.ChooseNextAction();
                return;
            }

            _firedVolley = false;
//            CurrentAction = MoveToTargetPosition(CombatManager.Player.Position.CurrentValue() + 5);
        }
    }
}