using SamsHelper.BaseGameFunctionality.Basic;
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
            if (EnemyWeapon != null)
            {
                _targetDistance = EnemyWeapon.GetAttributeValue(AttributeType.Accuracy) * 0.9f;
            }
            else
            {
                _targetDistance = Random.Range(20f, 50f);
            }
            SetStatusText(nameof(Retreat));
        }

        public override void Update()
        {
//            EnemyCombatController.Retreat();
            if (Relation.Distance.GetCurrentValue() >= _targetDistance)
            {
                SelectRandomTransition();
            }
        }
    }
}