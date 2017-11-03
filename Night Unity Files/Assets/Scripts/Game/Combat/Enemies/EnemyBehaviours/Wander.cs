using SamsHelper;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies.EnemyBehaviours
{
    public class Wander : EnemyBehaviour
    {
        private Direction _direction;
        
        public Wander(EnemyPlayerRelation relation) : base(nameof(Wander), relation)
        {
        }

        public override void Update()
        {
            if (_direction == Direction.Left)
            {
                EnemyCombatController.Retreat();
            }
            else
            {
                EnemyCombatController.Approach();
            }
            Duration -= Time.deltaTime;
            if (Duration < 0)
            {
                SelectRandomTransition();
            }
        }

        public override void Enter()
        {
            Duration = Random.Range(3, 5);
            _direction = Random.Range(0, 2) == 0 ? Direction.Left : Direction.Right;
            SetStatusText("Wandering");
        }

        public override void Exit()
        {
            ReturnToDefault();
        }
    }
}