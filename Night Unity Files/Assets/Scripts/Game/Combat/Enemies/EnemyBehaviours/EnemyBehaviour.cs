using SamsHelper.ReactiveUI;

namespace Game.Combat.Enemies.EnemyBehaviours
{
    public abstract class EnemyBehaviour
    {
        protected readonly EnemyPlayerRelation Relation;
        protected readonly ValueTextLink<string> StatusTextLink = new ValueTextLink<string>();

        protected EnemyBehaviour(EnemyPlayerRelation relation)
        {
            Relation = relation;
            StatusTextLink.AddTextObject(relation.Enemy.EnemyView().StatusEffects);
        }
        
        public abstract void Execute();
        public abstract float Evaluate();

        protected void NavigateToState(string stateName)
        {
            Relation.Enemy.CombatStates.NavigateToState(stateName);
        }
    }
}