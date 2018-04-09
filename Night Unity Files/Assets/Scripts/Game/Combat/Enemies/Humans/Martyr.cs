using Game.Combat.Misc;

namespace Game.Combat.Enemies.Humans
{
    public class Martyr : EnemyBehaviour
    {
        private bool _detonated;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            HealthController.AddOnTakeDamage(damage =>
            {
                if (_detonated) return;
                if (HealthController.GetCurrentHealth() != 0) return;
                Detonate();
            });
        }

        protected override void OnAlert()
        {
            MoveToPlayer();
        }

        public override void ChooseNextAction()
        {
            CurrentAction = null;
        }

        protected override void ReachPlayer()
        {
            if (!_detonated) Detonate();
        }

        private void Detonate()
        {
            _detonated = true;
            SetActionText("Detonating");
            Explosion.CreateExplosion(transform.position, 2, 50).Detonate();
        }
    }
}