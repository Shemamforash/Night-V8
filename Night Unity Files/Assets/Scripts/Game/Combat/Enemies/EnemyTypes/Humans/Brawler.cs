namespace Game.Combat.Enemies.EnemyTypes.Humans
{
    public class Brawler : DetailedEnemyCombat
    {
        public override void ChooseNextAction()
        {
            CurrentAction = MoveToTargetDistance(0);
        }

        protected override void ReachTarget()
        {
            base.ReachTarget();
            if (Alerted)
            {
                CurrentAction = MoveToTargetDistance(0);
            }
        }
    }
}