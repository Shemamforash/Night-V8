using SamsHelper.BaseGameFunctionality.Basic;

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
            float range = EnemyWeapon.GetAttributeValue(AttributeType.Accuracy);
            if (distance > range)
            {
//                EnemyCombatController.Approach();
            }
            else if (distance < range * 0.2f)
            {
//                EnemyCombatController.Retreat();
            }
            else if (distance <= range * 0.8f)
            {
                if (EnemyWeapon.GetRemainingAmmo() == 0)
                {
//                    EnemyCombatController.ReloadWeapon();
//                } else if (!EnemyWeapon.Cocked)
//                {
//                    EnemyCombatController.CockWeapon();
//                }
//                else
//                {
//                    EnemyCombatController.FireWeapon();
                }
            }
        }
    }
}