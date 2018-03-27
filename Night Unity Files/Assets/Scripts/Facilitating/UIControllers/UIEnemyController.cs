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
//        private static readonly List<GameObject> _leftList = new List<GameObject>();
//        private static readonly List<GameObject> _rightList = new List<GameObject>();

        public void Awake()
        {
//            _enemyListLeft = Helper.FindChildWithName(gameObject, "Left").transform;
//            _enemyListRight = Helper.FindChildWithName(gameObject, "Right").transform;
        }

        private GameObject CreateNewEnemyUi()
        {
            GameObject enemyUi = Instantiate(Resources.Load<GameObject>("Prefabs/Combat/Enemy Item"));
            enemyUi.transform.SetParent(transform);
            enemyUi.transform.position = new Vector3(0,0,0);
            RectTransform rect = enemyUi.GetComponent<RectTransform>();
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
//            if (_leftList.Count <= _rightList.Count)
//            {
//                enemyUi = Instantiate(Resources.Load<GameObject>("Prefabs/Combat/Enemy UI Left"));
//                enemyUi.transform.SetParent(_enemyListLeft);
//                _leftList.Add(enemyUi);
//            }
//            else
//            {
//                enemyUi = Instantiate(Resources.Load<GameObject>("Prefabs/Combat/Enemy UI Right"));
//                enemyUi.transform.SetParent(_enemyListRight);
//                _rightList.Add(enemyUi);
//            }

//            Vector3 uiPosition = enemyUi.transform.position;
//            uiPosition.z = 0;
//            enemyUi.transform.position = uiPosition;
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
                SelectAntiClockwise();
            }
            else
            {
                SelectClockwise();
            }
        }

        private static void SelectEnemy(int direction)
        {
            Vector2 playerTransform = CombatManager.Player.transform.position;
            Enemies.Sort((a, b) =>
            {
                float aAngle = AdvancedMaths.AngleFromUp(playerTransform, a.transform.position);
                float bAngle = AdvancedMaths.AngleFromUp(playerTransform, b.transform.position);
                return aAngle.CompareTo(bAngle);
            });
            int currentTargetIndex = Enemies.IndexOf((EnemyBehaviour) CombatManager.Player.GetTarget());
            currentTargetIndex += direction;
            if (currentTargetIndex == Enemies.Count) currentTargetIndex = 0;
            if (currentTargetIndex == -1) currentTargetIndex = Enemies.Count - 1;
            Enemies[currentTargetIndex].SetSelected();
        }

        private static void SelectClockwise()
        {
            SelectEnemy(1);
        }

        private static void SelectAntiClockwise()
        {
            SelectEnemy(-1);
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
            if(Enemies.Count != 0) SelectClockwise();
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