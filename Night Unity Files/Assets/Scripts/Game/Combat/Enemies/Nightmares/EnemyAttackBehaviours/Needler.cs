using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class Needler : TimedAttackBehaviour
    {
        public void Start()
        {
            Initialise(1);
        }

        protected override void Attack()
        {
            Vector3 dirToPlayer = transform.Direction(Enemy.GetTarget().transform);
            int damage = (int) (WorldState.Difficulty() / 10f * 10);
            NeedleBehaviour.Create(transform.position + dirToPlayer * 0.25f, Enemy.GetTarget().transform.position, damage);
        }
    }
}