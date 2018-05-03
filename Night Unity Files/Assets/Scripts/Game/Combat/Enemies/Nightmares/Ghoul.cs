using Game.Combat.Generation;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class Ghoul : EnemyBehaviour
    {
        private const float DistanceToTouch = 0.5f;
        
        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            Alerted = true;
            CurrentAction = SeekPlayer;
        }

        private void SeekPlayer()
        {
            Vector2 direction = CombatManager.Player().transform.position - transform.position;
            Move(direction.normalized);
        }
        
        public override void Update()
        {
            base.Update();
            if (DistanceToTarget() > DistanceToTouch) return;
            GetTarget().Sicken();
            Kill();
        }
    }
}