using Game.Combat.CharacterUi;

namespace Game.Combat.Enemies.EnemyTypes
{
    public class Martyr : EnemyBehaviour
    {
        private bool _detonated;

        public override void Initialise(Enemy enemy, EnemyUi characterUi)
        {
            base.Initialise(enemy, characterUi);
            EnemyUi.HealthController.AddOnTakeDamage(damage =>
            {
                if (_detonated) return;
                if (EnemyUi.HealthController.GetCurrentHealth() != 0) return;
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
            if(!_detonated) Detonate();
        }
        
        private void Detonate()
        {
            _detonated = true;
            SetActionText("Detonating");
            Explosion.CreateExplosion(transform.position, 2, 50).Detonate();
        }
    }    
}