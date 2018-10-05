namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class Bombardment : TimedAttackBehaviour
    {
        private int _projectileCount;

        public void Initialise(int projectileCount, float maxTimer, float minTimer = -1)
        {
            Initialise(maxTimer, minTimer);
            _projectileCount = projectileCount;
        }

        protected override void Attack()
        {
            for (int i = 0; i < _projectileCount; ++i)
                GhastProjectile.Create(transform.position, (Enemy.TargetPosition() - transform.position).normalized);
        }
    }
}