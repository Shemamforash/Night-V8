using Game.Combat.Misc;

namespace Game.Combat.Enemies.Humans
{
    public class Martyr : UnarmedBehaviour
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
            FollowTarget();
        }

        protected override void ReachTarget()
        {
            if (!_detonated) Detonate();
        }

        private void Detonate()
        {
            _detonated = true;
            SetActionText("Detonating");
            SkillAnimationController.Create("Martyr", 2f, () =>
            {
                Explosion.CreateExplosion(transform.position, 50, 2).InstantDetonate();
            });
        }
    }
}