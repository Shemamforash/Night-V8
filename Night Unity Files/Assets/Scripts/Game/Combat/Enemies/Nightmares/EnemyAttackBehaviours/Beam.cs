using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class Beam : TimedAttackBehaviour
    {
        private Transform _target;
        private float _beamWidth;

        public void Initialise(Transform target, float maxTimer, float minTimer = -1, float beamWidth = 0.5f)
        {
            Initialise(maxTimer, minTimer);
            _target = target;
            _beamWidth = beamWidth;
        }
        
        protected override void Attack()
        {
            BeamController.Create(transform, _target).SetBeamWidth(_beamWidth);
        }
    }
}