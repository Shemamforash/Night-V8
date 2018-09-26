using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Misc;
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
            MovementController.Move(direction.normalized);
        }

        public override void MyUpdate()
        {
            base.MyUpdate();
            if (DistanceToTarget() > _distanceToTouch) return;
            SickenBehaviour.Create(GetTarget().transform.position, GetTarget());
            LeafBehaviour.CreateLeaves(transform.position);
            Kill();
        }
    }
}