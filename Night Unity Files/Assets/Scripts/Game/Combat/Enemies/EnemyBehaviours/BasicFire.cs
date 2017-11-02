using System;
using Game.Combat.CombatStates;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Input;

namespace Game.Combat.Enemies.EnemyBehaviours
{
    public class BasicFire : EnemyBehaviour
    {
        public BasicFire(EnemyPlayerRelation relation) : base("Firing", relation)
        {
        }

        public override void Enter()
        {
            SetStatusText("Wandering");
        }

        public override void Update()
        {
            float distance = Relation.Distance.GetCurrentValue();
            float range = Relation.Enemy.Weapon().GetAttributeValue(AttributeType.Accuracy);
            if (distance > range)
            {
                Relation.Enemy.CombatController.OnInputDown(InputAxis.Horizontal, false, 1f);
            } else if (distance < range * 0.2f)
            {
                Relation.Enemy.CombatController.OnInputDown(InputAxis.Horizontal, false, -1f);
            }
            else if (distance <= range * 0.8f)
            {
                Relation.Enemy.CombatController.OnInputDown(InputAxis.Fire, false);
            }
        }
    }
}