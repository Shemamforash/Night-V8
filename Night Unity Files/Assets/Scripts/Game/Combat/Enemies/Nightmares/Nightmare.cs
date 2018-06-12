using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class Nightmare : EnemyBehaviour
    {
        private const float BeamAttackTimerMax = 7f;
        private float _beamAttackTimer;
        private const int HealthLostTarget = 200;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            gameObject.AddComponent<Feed>().Initialise(HealthLostTarget);
        }

        public override void Update()
        {
            base.Update();
            UpdateBeamAttack();
        }

        private void UpdateBeamAttack()
        {
            Immobilised(true);
            _beamAttackTimer -= Time.deltaTime;
            if (_beamAttackTimer > 0f) return;
//            BeamController.Create(transform, transform.position + transform.right * 30);
//            BeamController.Create(transform, transform.position - transform.right * 30);
//            BeamController.Create(transform, transform.position + transform.up * 30);
//            BeamController.Create(transform, transform.position - transform.up * 30);
            _beamAttackTimer = BeamAttackTimerMax;
        }
    }
}