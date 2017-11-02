using Game.Combat.CombatStates;

namespace Game.Combat.Enemies.EnemyBehaviours
{
    public class Flee : EnemyBehaviour
    {
        public Flee(EnemyPlayerRelation relation) : base(nameof(Flee), relation)
        {
        }

        public override void Enter()
        {
            NavigateToCombatState(nameof(Retreating));
            SetStatusText("Fleeing");
            Relation.Enemy.StartSprinting();
        }
    }
}