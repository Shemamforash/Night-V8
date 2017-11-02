namespace Game.Combat.Enemies.EnemyBehaviours
{
    public class Herd : EnemyBehaviour
    {
        public Herd(EnemyPlayerRelation relation) : base(nameof(Herd), relation, true)
        {
        }

        public override void OnDetect()
        {
            base.OnDetect();
            CombatManager.Scenario().Enemies().ForEach(e =>
            {
                Herd otherHerdBehaviour = (Herd) e.GetBehaviour(this);
                if (otherHerdBehaviour != null)
                {
                    e.Alert();
                }
            });
        }
    }
}