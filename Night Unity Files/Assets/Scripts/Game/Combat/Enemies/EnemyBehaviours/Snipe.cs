namespace Game.Combat.Enemies.EnemyBehaviours
{
    public class Snipe : EnemyBehaviour
    {
        public Snipe(EnemyPlayerRelation relation) : base(relation)
        {
        }

        public override float Evaluate()
        {
            return 0;
        }

        public override void Execute()
        {
            float range = Relation.Distance.GetCurrentValue();
            float selfHitProbability = Relation.Enemy.Weapon().HitProbability(range);
            float playerHitProbability = Relation.Player.Weapon().HitProbability(range);
            if (playerHitProbability > 0.8)
            {
                NavigateToState("Retreating");
            }
            else if (selfHitProbability < 1)
            {
                NavigateToState("Approaching");
            }
            else if (selfHitProbability > 1.1)
            {
               NavigateToState("Aiming");
            }
        }
    }
}