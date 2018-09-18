using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Player;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class Ghoul : EnemyBehaviour
    {
        private float _distanceToTouch = 0.5f;
        private GameObject _ghoulDeathPrefab;
        
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
            GetTarget().Sicken();
            if (_ghoulDeathPrefab == null) _ghoulDeathPrefab = Resources.Load<GameObject>("Prefabs/Combat/Visuals/Sicken Effect");
            Instantiate(_ghoulDeathPrefab).transform.position = transform.position;
            Kill();
        }
    }
}