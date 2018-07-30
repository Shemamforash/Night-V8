using System.Collections.Generic;
using Game.Combat.Enemies;
using UnityEngine;

namespace Game.Combat.Generation.Shrines
{
    public abstract class BasicShrineBehaviour : MonoBehaviour
    {
        protected bool Triggered;
        private readonly List<EnemyBehaviour> _enemiesAlive = new List<EnemyBehaviour>();
        private static GameObject _disappearPrefab;

        protected void AddEnemy(EnemyBehaviour b)
        {
            _enemiesAlive.Add(b);
        }

        protected bool EnemiesDead()
        {
            List<EnemyBehaviour> enemies = _enemiesAlive.FindAll(e => e == null);
            enemies.ForEach(e => _enemiesAlive.Remove(e));
            return _enemiesAlive.Count == 0;
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (Triggered) return;
            if (!other.gameObject.CompareTag("Player")) return;
            StartShrine();
        }

        public void Update()
        {
            if (!Triggered) return;
            if (EnemiesDead()) OnEnemiesDead();
        }

        protected virtual void OnEnemiesDead()
        {
            
        }

        protected virtual void Succeed()
        {
            End();
        }

        protected virtual void Fail()
        {
            End();
        }
        
        protected void End()
        {
            for (int i = _enemiesAlive.Count - 1; i >= 0; --i)
            {
                if (_disappearPrefab == null) _disappearPrefab = Resources.Load<GameObject>("Prefabs/Combat/Visuals/Disappear");
                Instantiate(_disappearPrefab).transform.position = _enemiesAlive[i].transform.position;
                _enemiesAlive[i].Kill();
            }
        }
        
        protected abstract void StartShrine();
    }
}