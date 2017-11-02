using Game.Combat.CombatStates;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Input;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies.EnemyBehaviours
{
    public class Retreat : EnemyBehaviour
    {
        private float _targetDistance;
        
        public Retreat(EnemyPlayerRelation relation) : base(nameof(Retreat), relation)
        {
        }

        public override void Enter()
        {
            Relation.Enemy.CombatController.OnInputDown(InputAxis.Horizontal, false, -1f);
            if (Relation.Enemy.Weapon() != null)
            {
                _targetDistance = Relation.Enemy.Weapon().GetAttributeValue(AttributeType.Accuracy) * 0.9f;
            }
            else
            {
                _targetDistance = Random.Range(20f, 50f);
            }
            SetStatusText(nameof(Retreating));
        }

        public override void Update()
        {
            if (Relation.Distance.GetCurrentValue() >= _targetDistance)
            {
                SelectRandomTransition();
            }
        }
    }
}