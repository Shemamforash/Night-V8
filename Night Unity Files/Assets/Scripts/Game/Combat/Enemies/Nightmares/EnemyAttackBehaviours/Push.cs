using SamsHelper.Libraries;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class Push : TimedAttackBehaviour
    {
        public void Start()
        {
            Initialise(4f, 1.5f);
        }

        protected override void Attack()
        {
            float angle = AdvancedMaths.AngleFromUp(transform.position, Enemy.TargetPosition());
            SkillAnimationController.Create(transform, "Beam", 0.5f, () => { PushController.Create(transform.position, angle, false); }, 0.5f);
        }
    }
}