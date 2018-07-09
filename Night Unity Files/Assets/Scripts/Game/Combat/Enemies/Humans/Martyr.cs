using Game.Combat.Misc;
using Game.Combat.Player;

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
            FollowTarget(PlayerCombat.Instance);
        }

        protected override void ReachTarget()
        {
            if (!_detonated) Detonate();
        }

        private void Detonate()
        {
            _detonated = true;
            SetActionText("Detonating");
            Explosion.CreateExplosion(transform.position, 50, 2).Detonate();
        }
    }
}