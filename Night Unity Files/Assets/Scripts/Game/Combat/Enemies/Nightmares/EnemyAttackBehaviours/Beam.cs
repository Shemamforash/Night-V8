using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class Beam : TimedAttackBehaviour
    {
        private Transform _target;
        
        public void Initialise(Transform target, float maxTimer, float minTimer = -1)
        {
            Initialise(maxTimer, minTimer);
            _target = target;
        }
        
        protected override void Attack()
        {
            BeamController.Create(transform, _target).SetBeamWidth(0.5f);
        }
    }
}