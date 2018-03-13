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
        public static readonly List<DetailedEnemyCombat> Enemies = new List<DetailedEnemyCombat>();
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
                TrySelectAtDistance(Enemies.IndexOf(CombatManager.Player.CurrentTarget), -1);
            }
            else
            {
                TrySelectAtDistance(Enemies.IndexOf(CombatManager.Player.CurrentTarget), 1);
            }
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
            DetailedEnemyCombat enemyUi = e.LinkUi(CreateNewEnemyUi());
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

        private static bool TrySelectAtDistance(int index, int distance)
        {
            if (index + distance >= Enemies.Count || index + distance < 0) return false;
            DetailedEnemyCombat enemy = Enemies[index + distance];
            if (enemy == null) return false;
//            enemy.PrimaryButton.Button().Select();
            return true;
        }

        public static void Remove(DetailedEnemyCombat enemy)
        {
            int enemyPosition = Enemies.IndexOf(enemy);
            int distance = 1;
            while (distance < Enemies.Count)
            {
                if (TrySelectAtDistance(enemyPosition, distance) || TrySelectAtDistance(enemyPosition, -distance))
                {
                    break;
                }

                ++distance;
            }

            Enemies.Remove(enemy);
        }

        private static void AddEnemy(DetailedEnemyCombat e)
        {
            Enemies.Add(e);
            if (Enemies.Count == 1) e.PrimaryButton.Button().Select();
        }

        public static DetailedEnemyCombat NearestEnemy()
        {
            DetailedEnemyCombat nearestEnemy = null;
            float nearestDistance = 10000;
            Enemies.ForEach(e =>
            {
                if (!e.InCombat()) return;
                if (nearestEnemy == null)
                {
                    nearestEnemy = e;
                    return;
                }

                float distance = Vector2.Distance(e.CharacterController.Position(), CombatManager.Player.CharacterController.Position());
                if (distance >= nearestDistance) return;
                nearestDistance = distance;
                nearestEnemy = e;
            });
            return nearestEnemy;
        }
    }
}