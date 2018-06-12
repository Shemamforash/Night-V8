namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class Bombardment : TimedAttackBehaviour
    {
        private int _projectileCount;

        public void Initialise(float maxTimer, int projectileCount)
        {
            Initialise(maxTimer);
            _projectileCount = projectileCount;
        }

        protected override void Attack()
        {
            for (int i = 0; i < _projectileCount; ++i)
            {
                GhastProjectile.Create(transform.position, (Enemy.GetTarget().transform.position - transform.position).normalized);
            }
        }
    }
}