using SamsHelper.BaseGameFunctionality.CooldownSystem;

namespace Game.Combat.Enemies.EnemyTypes
{
    public class Martyr : Enemy
    {
        private readonly Cooldown _detonateCooldown;

        public Martyr() : base(nameof(Martyr), 10)
        {
            _detonateCooldown = CombatManager.CombatCooldowns.CreateCooldown(1f);
            _detonateCooldown.SetStartAction(() => SetActionText("Detonating"));
            _detonateCooldown.SetEndAction(Detonate);
            BaseAttributes.Endurance.SetCurrentValue(10);
        }

        private void MoveTowardsTargetDistance()
        {
            MoveForward();
            SetActionText("Approaching");
            if (Distance.ReachedMin()) _detonateCooldown.Start();
        }

        private void Detonate()
        {
            Shot s = new Shot(this);
            s.SetSplinterRange(10);
            s.SetSplinterFalloff(0.5f);
            s.SetKnockbackDistance(10);
            s.SetDamage(50);
            s.Fire();
        }
        
        public override void UpdateBehaviour()
        {
            base.UpdateBehaviour();
            if(!_detonateCooldown.Running()) MoveTowardsTargetDistance();
        }
    }    
}