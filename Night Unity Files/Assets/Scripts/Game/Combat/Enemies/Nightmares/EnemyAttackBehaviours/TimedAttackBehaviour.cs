using Game.Combat.Generation;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public abstract class TimedAttackBehaviour : BasicAttackBehaviour
    {
        private float _currentTimer;
        private float MaxTimer, MinTimer;
        private float _targetTime;

        public void Initialise(float maxTimer, float minTimer = -1)
        {
            MaxTimer = maxTimer;
            MinTimer = minTimer < 0 ? MaxTimer : minTimer;
            SetTargetTime();
        }

        private void SetTargetTime()
        {
            _targetTime = Random.Range(MinTimer, MaxTimer);
        }
        
        public virtual void Update()
        {
            if (Paused || !CombatManager.IsCombatActive()) return;
            _currentTimer += Time.deltaTime;
            if (_currentTimer < _targetTime) return;
            Attack();
            _currentTimer = 0;
            SetTargetTime();
        }
    }
}