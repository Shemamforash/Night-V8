using SamsHelper.BaseGameFunctionality.CooldownSystem;
using UnityEngine;

namespace Game.Combat.Enemies.EnemyTypes
{
    public class Martyr : Enemy
    {
        private readonly Cooldown _detonateCooldown;
        private bool _detonated;

        public Martyr(float position) : base(nameof(Martyr), 3, 1, position)
        {
            _detonateCooldown = CombatManager.CombatCooldowns.CreateCooldown(1f);
            _detonateCooldown.SetStartAction(() => SetActionText("Detonating"));
            _detonateCooldown.SetEndAction(Detonate);
            HealthController.AddOnTakeDamage(damage =>
            {
                if (_detonated) return;
                Detonate();
            });
        }

        protected override void Alert()
        {
            base.Alert();
            TargetDistance = 0f;
            CurrentAction = MoveToTargetDistance;
            Speed = 10;
        }
        
        protected override void ReachTarget()
        {
            if (Alerted) return;
            _detonateCooldown.Start();
            CurrentAction = null;
        }
        
        private void Detonate()
        {
            _detonated = true;
            Explosion.CreateAndDetonate(Position.CurrentValue(), 10, 40);
        }
    }    
}