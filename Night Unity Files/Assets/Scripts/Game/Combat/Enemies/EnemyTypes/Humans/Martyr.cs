using Game.Characters;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using UnityEngine;

namespace Game.Combat.Enemies.EnemyTypes
{
    public class Martyr : DetailedEnemyCombat
    {
        private Cooldown _detonateCooldown;
        private bool _detonated;

        public override void SetPlayer(Character enemy)
        {
            base.SetPlayer(enemy);
//            MinimumFindCoverDistance = -1f;
            _detonateCooldown = CombatManager.CombatCooldowns.CreateCooldown(1f);
            _detonateCooldown.SetStartAction(() => SetActionText("Detonating"));
            _detonateCooldown.SetEndAction(Detonate);
//            HealthController.AddOnTakeDamage(damage =>
//            {
//                if (_detonated) return;
//                if (HealthController.GetCurrentHealth() != 0) return;
//                Detonate();
//            });
        }
        
        public override void Alert()
        {
            base.Alert();
            Speed = 10;
        }

        public override void ChooseNextAction()
        {
//            CurrentAction = MoveToPlayer;
        }
        
        protected override void ReachTarget()
        {
//            if (!Alerted) return;
//            _detonateCooldown.Start();
//            CurrentAction = null;
        }
        
        private void Detonate()
        {
            _detonated = true;
//            Explosion.CreateAndDetonate(Position.CurrentValue(), 10, 50);
        }
    }    
}