using Game.Combat.CombatStates;

namespace Game.Combat.Enemies.EnemyBehaviours
{
    public class Rush : EnemyBehaviour
    {
        public Rush(EnemyPlayerRelation relation) : base(nameof(Rush), relation)
        {
        }

        public override void Enter()
        {
            NavigateToCombatState(nameof(Approaching));
            SetStatusText("Charging");
            Relation.Enemy.StartSprinting();
        }

        public override void Update()
        {
            if (Relation.Distance.GetCurrentValue() == 0)
            {
                SelectRandomTransition();
            }
        }

        public override void Exit()
        {
            Relation.Enemy.StopSprinting();
            NavigateToCombatState(nameof(Waiting));
        }
    }
}