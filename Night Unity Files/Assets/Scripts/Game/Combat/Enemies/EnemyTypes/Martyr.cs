using SamsHelper.BaseGameFunctionality.CooldownSystem;

namespace Game.Combat.Enemies.EnemyTypes
{
    public class Martyr : Enemy
    {
        private readonly Cooldown _detonateCooldown;
        private bool _detonated;


        public Martyr() : base(nameof(Martyr), 10)
        {
            _detonateCooldown = CombatManager.CombatCooldowns.CreateCooldown(1f);
            _detonateCooldown.SetStartAction(() => SetActionText("Detonating"));
            _detonateCooldown.SetEndAction(Detonate);
            BaseAttributes.Endurance.SetCurrentValue(3);
            AcceptsHealing = false;
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
            BaseAttributes.Endurance.SetCurrentValue(10);
        }
        
        protected override void ReachTarget()
        {
            if (!IsAlerted()) return;
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
            s.SetDamage(50);
            s.Fire();
        }
    }    
}