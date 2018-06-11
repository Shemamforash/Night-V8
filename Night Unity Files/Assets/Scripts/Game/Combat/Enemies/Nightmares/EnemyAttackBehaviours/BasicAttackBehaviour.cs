using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public abstract class BasicAttackBehaviour : MonoBehaviour
    {
        private float _currentTimer;
        protected float MaxTimer;
        protected bool Paused;
        protected EnemyBehaviour Enemy;

        public void Awake()
        {
            Enemy = GetComponent<EnemyBehaviour>();
        }
        
        public void Update()
        {
            if (Paused) return;
            _currentTimer += Time.deltaTime;
            if (_currentTimer < MaxTimer) return;
            Attack();
            _currentTimer = 0;
        }

        protected abstract void Attack();

        public void PauseOthers()
        {
            foreach (BasicAttackBehaviour basicAttackBehaviour in GetComponents<BasicAttackBehaviour>())
            {
                if (basicAttackBehaviour == this) continue;
                basicAttackBehaviour.Paused = true;
            }
        }

        public void UnpauseOthers()
        {
            foreach (BasicAttackBehaviour basicAttackBehaviour in GetComponents<BasicAttackBehaviour>())
            {
                if (basicAttackBehaviour == this) continue;
                basicAttackBehaviour.Paused = false;
            }
        }
    }
}