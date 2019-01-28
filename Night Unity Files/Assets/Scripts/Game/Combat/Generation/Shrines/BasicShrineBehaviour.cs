using System.Collections.Generic;
using DG.Tweening;
using Game.Combat.Enemies;
using UnityEngine;

namespace Game.Combat.Generation.Shrines
{
    public abstract class BasicShrineBehaviour : MonoBehaviour
    {
        protected bool Triggered;
        private static GameObject _disappearPrefab;
        protected bool IsInRange;

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.gameObject.CompareTag("Player")) return;
            IsInRange = true;
            if (Triggered) return;
            StartShrine();
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            if (!other.gameObject.CompareTag("Player")) return;
            IsInRange = false;
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
            CombatManager.ClearInactiveEnemies();
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