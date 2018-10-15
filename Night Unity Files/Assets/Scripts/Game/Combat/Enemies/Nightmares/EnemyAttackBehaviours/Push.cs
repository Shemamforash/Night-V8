using SamsHelper.Libraries;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class Push : TimedAttackBehaviour
    {
        public void Start()
        {
            Initialise(2f);
        }

        protected override void Attack()
        {
            float angle = AdvancedMaths.AngleFromUp(transform.position, Enemy.TargetPosition());
            PushController.Create(transform.position, angle, false);
        }
    }
}