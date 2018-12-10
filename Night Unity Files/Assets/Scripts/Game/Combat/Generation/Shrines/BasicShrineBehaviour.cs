using System.Collections.Generic;
using DG.Tweening;
using Game.Combat.Enemies;
using UnityEngine;

namespace Game.Combat.Generation.Shrines
{
    public abstract class BasicShrineBehaviour : MonoBehaviour
    {
        protected bool Triggered;
//        private readonly List<EnemyBehaviour> _enemiesAlive = new List<EnemyBehaviour>();
        private static GameObject _disappearPrefab;
        protected bool IsInRange;
        
//        public void AddEnemy(EnemyBehaviour b)
//        {
//            _enemiesAlive.Add(b);
//        }

//        protected bool EnemiesDead()
//        {
//            List<EnemyBehaviour> enemies = _enemiesAlive.FindAll(e => e == null);
//            enemies.ForEach(e => _enemiesAlive.Remove(e));
//            return _enemiesAlive.Count == 0;
//        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (Triggered) return;
            if (!other.gameObject.CompareTag("Player")) return;
            IsInRange = true;
            StartShrine();;
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            if (!other.gameObject.CompareTag("Player")) return;
            IsInRange = false;
        }

        public void Update()
        {
            if (!Triggered) return;
            if (CombatManager.ClearOfEnemies()) OnEnemiesDead();
        }

        protected virtual void OnEnemiesDead()
        {
            
        }

        protected virtual void Succeed()
        {
            End();
        }

        public virtual void Fail()
        {
            End();
        }
        
        protected void End()
        {
            CombatManager.OverrideMaxSize(0, new List<Enemy>());
            for (int i = CombatManager.Enemies().Count - 1; i >= 0; --i)
            {
                if (_disappearPrefab == null) _disappearPrefab = Resources.Load<GameObject>("Prefabs/Combat/Visuals/Disappear");
                Instantiate(_disappearPrefab).transform.position = CombatManager.Enemies()[i].transform.position;
                CombatManager.Enemies()[i].Kill();
            }
        }
        
        protected abstract void StartShrine();
    }
}