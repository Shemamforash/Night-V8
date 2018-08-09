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
            float angle = AdvancedMaths.AngleFromUp(transform.position, PlayerCombat.Instance.transform.position) + 80f;
            PushController.Create(transform.position, angle + 80f);
        }
    }
}