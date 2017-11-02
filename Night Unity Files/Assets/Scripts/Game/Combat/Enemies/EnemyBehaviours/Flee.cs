using Game.Combat.CombatStates;
using SamsHelper.Input;

namespace Game.Combat.Enemies.EnemyBehaviours
{
    public class Flee : EnemyBehaviour
    {
        public Flee(EnemyPlayerRelation relation) : base(nameof(Flee), relation)
        {
        }

        public override void Enter()
        {
            Relation.Enemy.CombatController.OnInputDown(InputAxis.Horizontal, false, -1f);
            SetStatusText("Fleeing");
//            Relation.Enemy.CombatController.StartSprinting();
        }
    }
}