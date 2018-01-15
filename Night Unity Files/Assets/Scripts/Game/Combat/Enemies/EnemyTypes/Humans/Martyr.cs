using SamsHelper.BaseGameFunctionality.CooldownSystem;
using UnityEngine;

namespace Game.Combat.Enemies.EnemyTypes
{
    public class Martyr : Enemy
    {
        private readonly Cooldown _detonateCooldown;
        private bool _detonated;

        public Martyr() : base(nameof(Martyr), 1)
        {
            _detonateCooldown = CombatManager.CombatCooldowns.CreateCooldown(1f);
            _detonateCooldown.SetStartAction(() => SetActionText("Detonating"));
            _detonateCooldown.SetEndAction(Detonate);
            Speed = 3;
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
            Shot s = new Shot(this, null);
            s.SetSplinterRange(10);
            s.SetSplinterFalloff(0.5f);
            s.SetKnockbackDistance(10);
            s.SetKnockdownChance(1);
            s.SetKnockDownRadius(10);
            s.SetDamage(40);
            s.Fire();
        }
    }    
}