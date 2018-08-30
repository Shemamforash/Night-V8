using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

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
            float angle = AdvancedMaths.AngleFromUp(transform.position, Enemy.GetTarget().transform.position);
            PushController.Create(transform.position, angle);
        }
    }
}