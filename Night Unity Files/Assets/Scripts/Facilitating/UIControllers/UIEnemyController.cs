using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            _shouldAlertAll = false;
            _enemiesToAlert.Clear();
            CombatManager.CurrentScenario.Enemies().ForEach(e =>
            {
                if (!e.IsDead) AddEnemy(e.LinkUi(CreateNewEnemyUi()));
            });
        }

        public void Update()
        {
            if (!_shouldAlertAll || _enemiesToAlert.Count == 0) return;
            _enemiesToAlert[0].Alert();
            _enemiesToAlert.RemoveAt(0);
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
            float distance = CombatManager.Player.DistanceToTarget();
            Enemies.Sort((a, b) => a.DistanceToTarget().CompareTo(b.DistanceToTarget()));
            EnemyBehaviour nearestEnemy = Enemies.FirstOrDefault(e => e.DistanceToTarget() > distance);
            if (nearestEnemy == null) SelectNearestEnemy();
            else nearestEnemy.SetSelected();
        }

        private static void SelectNearestEnemy()
        {
            NearestEnemy()?.SetSelected();
        }

        private static void SelectNextNearest()
        {
            float distance = CombatManager.Player.DistanceToTarget();
            Enemies.Sort((a, b) => a.DistanceToTarget().CompareTo(b.DistanceToTarget()));
            Enemies.Reverse();
            EnemyBehaviour nearestEnemy = Enemies.FirstOrDefault(e => e.DistanceToTarget() < distance);
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

        public void UpdateCombat()
        {
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

        private static List<EnemyBehaviour> _enemiesToAlert = new List<EnemyBehaviour>();
        private static bool _shouldAlertAll;

        public static void AlertAll()
        {
            _shouldAlertAll = true;
        }

        public static void Remove(EnemyBehaviour enemy)
        {
            Enemies.Remove(enemy);
            SelectNextNearest();
        }

        private static void AddEnemy(EnemyBehaviour e)
        {
            Enemies.Add(e);
            _enemiesToAlert.Add(e);
            if (Enemies.Count == 1) e.SetSelected();
        }

        public static EnemyBehaviour NearestEnemy()
        {
            EnemyBehaviour nearestEnemy = null;
            float nearestDistance = 10000;
            Enemies.ForEach(e =>
            {
                float distance = e.DistanceToTarget();
                if (distance >= nearestDistance) return;
                nearestDistance = distance;
                nearestEnemy = e;
            });
            return nearestEnemy;
        }
    }
}