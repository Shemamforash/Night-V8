namespace Game.Combat.Enemies.EnemyTypes.Humans
{
    public class Brawler : EnemyBehaviour
    {
//        public override void ChooseNextAction()
//        {
//            CurrentAction = MoveToPlayer;
//        }

        protected override void ReachTarget()
        {
            base.ReachTarget();
//            if (Alerted)
//            {
//                CurrentAction = MoveToPlayer;
//            }
        }
    }
}