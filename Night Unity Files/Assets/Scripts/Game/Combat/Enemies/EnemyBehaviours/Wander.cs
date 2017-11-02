using Game.Combat.CombatStates;
using SamsHelper.Input;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies.EnemyBehaviours
{
    public class Wander : EnemyBehaviour
    {
        private float _direction;
        
        public Wander(EnemyPlayerRelation relation) : base(nameof(Wander), relation)
        {
        }

        public override void Update()
        {
            Relation.Enemy.CombatController.OnInputDown(InputAxis.Horizontal, false, _direction);
            Duration -= Time.deltaTime;
            if (Duration < 0)
            {
                SelectRandomTransition();
            }
        }

        public override void Enter()
        {
            Duration = Random.Range(3, 5);
            _direction = Random.Range(0, 2) == 0 ? -1 : 1;
            SetStatusText("Wandering");
        }

        public override void Exit()
        {
            ReturnToDefault();
        }
    }
}