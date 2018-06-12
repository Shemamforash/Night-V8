using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public abstract class TimedAttackBehaviour : BasicAttackBehaviour
    {
        private float _currentTimer;
        protected float MaxTimer;

        public void Initialise(float maxTimer)
        {
            MaxTimer = maxTimer;
        }
        
        public void Update()
        {
            if (Paused) return;
            _currentTimer += Time.deltaTime;
            if (_currentTimer < MaxTimer) return;
            Attack();
            _currentTimer = 0;
        }
    }
}