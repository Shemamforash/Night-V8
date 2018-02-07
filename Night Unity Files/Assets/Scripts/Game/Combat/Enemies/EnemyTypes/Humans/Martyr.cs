using SamsHelper.BaseGameFunctionality.CooldownSystem;
using UnityEngine;

namespace Game.Combat.Enemies.EnemyTypes
{
    public class Martyr : Enemy
    {
        private readonly Cooldown _detonateCooldown;
        private bool _detonated;

        public Martyr(float position) : base(nameof(Martyr), position)
        {
            MinimumFindCoverDistance = -1f;
            _detonateCooldown = CombatManager.CombatCooldowns.CreateCooldown(1f);
            _detonateCooldown.SetStartAction(() => EnemyView.SetActionText("Detonating"));
            _detonateCooldown.SetEndAction(Detonate);
            HealthController.AddOnTakeDamage(damage =>
            {
                if (_detonated) return;
                Detonate();
            });
        }

        public override void Alert()
        {
            base.Alert();
            Speed = 10;
        }

        public override void ChooseNextAction()
        {
            CurrentAction = MoveToTargetDistance(0);
        }
        
        protected override void ReachTarget()
        {
            if (!Alerted) return;
            _detonateCooldown.Start();
            CurrentAction = null;
        }
        
        private void Detonate()
        {
            _detonated = true;
            Explosion.CreateAndDetonate(Position.CurrentValue(), 10, 50);
        }
    }    
}