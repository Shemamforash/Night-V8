namespace Game.Combat.Enemies.EnemyTypes.Humans
{
    public class Brawler : Enemy
    {
        public Brawler(float position) : base(nameof(Brawler), position)
        {
        }

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