using System;

namespace Game.Combat.Enemies.EnemyTypes.Humans
{
    public class Brawler : Enemy
    {
        public Brawler(float position) : base(nameof(Brawler), 10, 7, position)
        {
        }

        protected override Action ChooseNextAction()
        {
            return MoveToTargetDistance(0);
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