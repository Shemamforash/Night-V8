using System;
using Game.Combat.Misc;
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
            Action<Vector2> onHit = null;
            if (WorldState.Difficulty() > 25) onHit = p => DecayBehaviour.Create(p);
            NeedleBehaviour.Create(transform.position + dirToPlayer * 0.25f, Enemy.GetTarget().transform.position, onHit);
        }
    }
}