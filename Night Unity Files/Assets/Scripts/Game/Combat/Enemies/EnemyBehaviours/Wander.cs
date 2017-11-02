using Game.Combat.CombatStates;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies.EnemyBehaviours
{
    public class Wander : EnemyBehaviour
    {
        public Wander(EnemyPlayerRelation relation) : base(nameof(Wander), relation)
        {
        }

        public override void Update()
        {
            Duration -= Time.deltaTime;
            if (Duration < 0)
            {
                SelectRandomTransition();
            }
        }

        public override void Enter()
        {
            Duration = Random.Range(3, 5);
            NavigateToCombatState(Random.Range(0, 2) == 0 ? nameof(Approaching) : nameof(Retreating));
            SetStatusText("Wandering");
        }

        public override void Exit()
        {
            ReturnToDefault();
        }
    }
}