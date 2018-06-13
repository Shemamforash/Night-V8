using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Player;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class Ghoul : EnemyBehaviour
    {
        private float _distanceToTouch = 0.5f;
        
        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            CurrentAction = SeekPlayer;
            gameObject.AddComponent<FeedTarget>();
        }

        private void SeekPlayer()
        {
            Vector2 direction = PlayerCombat.Instance.transform.position - transform.position;
            Move(direction.normalized);
        }

        public override void Update()
        {
            base.Update();
            if (DistanceToTarget() > _distanceToTouch) return;
            GetTarget().Sicken();
            Kill();
        }
    }
}