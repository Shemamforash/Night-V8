using Game.Combat.CombatStates;
using SamsHelper.Input;

namespace Game.Combat.Enemies.EnemyBehaviours
{
    public class Rush : EnemyBehaviour
    {
        public Rush(EnemyPlayerRelation relation) : base(nameof(Rush), relation)
        {
        }

        public override void Enter()
        {
            Relation.Enemy.CombatController.OnInputDown(InputAxis.Horizontal, false, 1f);
            SetStatusText("Charging");
//            Relation.Enemy.StartSprinting();
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
//            Relation.Enemy.StopSprinting();
        }
    }
}