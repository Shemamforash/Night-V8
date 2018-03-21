using Game.Combat.CharacterUi;

namespace Game.Combat.Enemies.EnemyTypes
{
    public class Martyr : DetailedEnemyCombat
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
        
        public override void ChooseNextAction()
        {
            Speed = 10;
            CurrentAction = MoveToPlayer;
        }

        protected override void ReachPlayer()
        {
            CurrentAction = null;
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