using System.Collections.Generic;
using Game.Combat;
using Game.Combat.Enemies;
using Game.Combat.Enemies.EnemyTypes;
using SamsHelper;
using UnityEngine;

namespace Facilitating.UIControllers
{
    public class UIEnemyController : MonoBehaviour, ICombatListener
    {
        public static readonly List<EnemyBehaviour> Enemies = new List<EnemyBehaviour>();
        private static Transform _enemyListLeft, _enemyListRight;
        private static readonly List<GameObject> _leftList = new List<GameObject>();
        private static readonly List<GameObject> _rightList = new List<GameObject>();

        public void Awake()
        {
            _enemyListLeft = Helper.FindChildWithName(gameObject, "Left").transform;
            _enemyListRight = Helper.FindChildWithName(gameObject, "Right").transform;
        }

        private GameObject CreateNewEnemyUi()
        {
            GameObject enemyUi;
            if (_leftList.Count <= _rightList.Count)
            {
                enemyUi = Instantiate(Resources.Load<GameObject>("Prefabs/Combat/Enemy UI Left"));
                enemyUi.transform.SetParent(_enemyListLeft);
                _leftList.Add(enemyUi);
            }
            else
            {
                enemyUi = Instantiate(Resources.Load<GameObject>("Prefabs/Combat/Enemy UI Right"));
                enemyUi.transform.SetParent(_enemyListRight);
                _rightList.Add(enemyUi);
            }

            Vector3 uiPosition = enemyUi.transform.position;
            uiPosition.z = 0;
            enemyUi.transform.position = uiPosition;
            enemyUi.transform.localScale = Vector3.one;
            return enemyUi;
        }

        public void EnterCombat()
        {
            CombatManager.CurrentScenario.Enemies().ForEach(e =>
            {
                if (!e.IsDead) AddEnemy(e.LinkUi(CreateNewEnemyUi()));
            });
        }

        public void UpdateCombat()
        {
        }

        public static void Select(float direction)
        {
            if (direction > 0)
            {
                SelectNextFurthest();
            }
            else
            {
                SelectNextNearest();
            }
        }

        private static void SelectNextFurthest()
        {
            CharacterCombat currentTarget = CombatManager.Player.GetTarget();
            float distance = currentTarget.DistanceToTarget();
            float nearestDistance = 10000;
            EnemyBehaviour nearestEnemy = null;
            Enemies.ForEach(e =>
            {
                if (e == currentTarget) return;
                float candidateDistance = e.DistanceToTarget();
                if (candidateDistance < distance) return;
                if (nearestEnemy != null && candidateDistance >= nearestDistance) return;
                nearestDistance = candidateDistance;
                nearestEnemy = e;
            });
            if (nearestEnemy == null) SelectNearestEnemy();
            else nearestEnemy.SetSelected();
        }

        private static void SelectNearestEnemy()
        {
            NearestEnemy(true)?.SetSelected();
        }

        private static void SelectNextNearest()
        {
            CharacterCombat currentTarget = CombatManager.Player.GetTarget();
            float distance = currentTarget.DistanceToTarget();
            Debug.Log(distance);
            float nearestDistance = -1;
            EnemyBehaviour nearestEnemy = null;
            Enemies.ForEach(e =>
            {
                if (e == currentTarget) return;
                float candidateDistance = e.DistanceToTarget();
                if (candidateDistance > distance) return;
                if (nearestEnemy != null && candidateDistance <= nearestDistance) return;
                nearestDistance = candidateDistance;
                nearestEnemy = e;
            });
            if (nearestEnemy == null) SelectNearestEnemy();
            else nearestEnemy.SetSelected();
        }

        public void ExitCombat()
        {
            Enemies.ForEach(e =>
            {
                e.ExitCombat();
                Destroy(e.gameObject);
            });
            Enemies.Clear();
        }

        public void QueueEnemyToAdd(EnemyType type)
        {
            Enemy e = new Enemy(type);
            EnemyBehaviour enemyUi = e.LinkUi(CreateNewEnemyUi());
            enemyUi.Alert();
            AddEnemy(enemyUi);
            CombatManager.CurrentScenario.AddEnemy(e);
        }

        public static bool AllEnemiesGone()
        {
            return Enemies.Count == 0;
        }

        public static void AlertAll()
        {
            Enemies.ForEach(e => e.Alert());
        }

        public static void Remove(EnemyBehaviour enemy)
        {
            SelectNextNearest();
            Enemies.Remove(enemy);
        }

        private static void AddEnemy(EnemyBehaviour e)
        {
            Enemies.Add(e);
            if (Enemies.Count == 1) e.SetSelected();
        }

        public static EnemyBehaviour NearestEnemy(bool ignoreTarget = false)
        {
            EnemyBehaviour nearestEnemy = null;
            float nearestDistance = 10000;
            Enemies.ForEach(e =>
            {
                if (ignoreTarget && e == CombatManager.Player.GetTarget()) return;
                if (nearestEnemy == null)
                {
                    nearestEnemy = e;
                    return;
                }

                float distance = e.DistanceToPlayer;
                if (distance >= nearestDistance) return;
                nearestDistance = distance;
                nearestEnemy = e;
            });
            return nearestEnemy;
        }
    }
}