using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public abstract class BasicAttackBehaviour : MonoBehaviour
    {
        protected bool Paused;
        protected EnemyBehaviour Enemy;

        public virtual void Awake()
        {
            Enemy = GetComponent<EnemyBehaviour>();
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