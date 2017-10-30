namespace Game.Combat.Enemies.EnemyBehaviours
{
    public class Herd : EnemyBehaviour
    {
        private bool _herdAlerted;

        public Herd(EnemyPlayerRelation relation) : base(relation)
        {
        }

        public override void Execute()
        {
            if (_herdAlerted) return;
            if (Relation.Distance < Relation.Enemy.DetectionRange)
            {
                CombatManager.Scenario().Enemies().ForEach(e =>
                {
                    Herd otherHerdBehaviour = (Herd) e.GetBehaviour(this);
                    otherHerdBehaviour.Alert();
                });
                Alert();
            }
            else if (Relation.Distance < Relation.Enemy.VisionRange)
            {
                StatusTextLink.Value("Alerted");
            }
            else
            {
                StatusTextLink.Value("Grazing");
            }
        }

        public override float Evaluate()
        {
            return 0;
        }

        private void Alert()
        {
            _herdAlerted = true;
            StatusTextLink.Value("Fleeing");
            NavigateToState("Retreating");
        }
    }
}