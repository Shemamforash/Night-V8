namespace Game.Combat.Enemies.EnemyBehaviours
{
    public class Rush : EnemyBehaviour
    {
        public Rush(EnemyPlayerRelation relation) : base(nameof(Rush), relation)
        {
        }

        public override void Enter()
        {
            SetStatusText("Charging");
//            EnemyCombatController.StartSprinting();
        }

        public override void Update()
        {
//            EnemyCombatController.Approach();
            if (Relation.Distance.GetCurrentValue() == 0)
            {
                SelectRandomTransition();
            }
        }

        public override void Exit()
        {
//            EnemyCombatController.StopSprinting();
        }
    }
}