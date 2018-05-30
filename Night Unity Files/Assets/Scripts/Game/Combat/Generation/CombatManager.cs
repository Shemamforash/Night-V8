using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Exploration.Regions;
using Game.Global;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

namespace Game.Combat.Generation
{
    public class CombatManager : Menu
    {
        private readonly CooldownManager _cooldowns = new CooldownManager();
        private Region _currentRegion;
        private bool _inMelee;
        private int _visibilityRange;
        private PlayerCombat _player;
        private bool _inCombat;
        private readonly List<EnemyBehaviour> _enemies = new List<EnemyBehaviour>();
        private static CombatManager _instance;

        public static bool AllEnemiesDead() => Instance()._enemies.Count == 0;

        public static float VisibilityRange() => Instance()._visibilityRange;

        public static Cooldown CreateCooldown() => Instance()._cooldowns.CreateCooldown();

        public static bool InCombat() => Instance()._inCombat;

        public static Region Region() => Instance()._currentRegion;

        public static PlayerCombat Player() => Instance()._player;

        public override void Awake()
        {
            base.Awake();
            _instance = this;
            _player = GameObject.Find("Player").GetComponent<PlayerCombat>();
        }

        private static CombatManager Instance()
        {
            if (_instance == null) _instance = FindObjectOfType<CombatManager>();
            return _instance;
        }

        private void OnDestroy()
        {
            _instance = null;
        }

        public void Start()
        {
            EnterCombat();
        }

        public void Update()
        {
            if (!_inCombat) return;
            _cooldowns.UpdateCooldowns();
        }

        private void EnterCombat()
        {
            _inCombat = true;
            WorldState.Pause();
            _visibilityRange = 5;
            _currentRegion = CharacterManager.SelectedCharacter.TravelAction.GetCurrentNode();

            _currentRegion.Barriers.ForEach(b => b.CreateObject());
            _currentRegion.Fire?.CreateObject();
            _currentRegion.Containers.ForEach(c => c.CreateObject());
            PathingGrid.Instance().GenerateGrid(_currentRegion.Barriers);

//            VisibilityRange = (int) (100 * WeatherManager.Instance().CurrentWeather().GetVisibility());

            _player.Initialise();
            _cooldowns.Clear();
            _currentRegion.Enemies().ForEach(e => { AddEnemy(e.GetEnemyBehaviour()); });
        }

        public static void ExitCombat()
        {
            if (!Instance()._inCombat) Debug.Log("Don't try and exit combat twice!");
            Instance()._inCombat = false;
            SceneChanger.ChangeScene("Map", false);
            Instance()._player.ExitCombat();
        }

        public static List<CharacterCombat> GetCharactersInRange(Vector2 position, float range)
        {
            List<CharacterCombat> charactersInRange = new List<CharacterCombat>();
            if (Vector2.Distance(Instance()._player.transform.position, position) <= range) charactersInRange.Add(Instance()._player);

            foreach (EnemyBehaviour enemy in Instance()._enemies)
            {
                if (Vector2.Distance(enemy.transform.position, position) <= range)
                {
                    charactersInRange.Add(enemy);
                }
            }

            return charactersInRange;
        }


//        public static void Select(float direction)
//        {
//            if (direction > 0)
//                Instance().SelectAntiClockwise();
//            else
//                Instance().SelectClockwise();
//        }

        public static List<EnemyBehaviour> EnemiesOnScreen()
        {
            return Instance()._enemies.FindAll(e => e.OnScreen());
        }

//        private void SelectEnemy(int direction)
//        {
//            Vector2 playerTransform = _player.transform.position;
//            List<EnemyBehaviour> visibleEnemies = EnemiesOnScreen();
//            EnemyBehaviour newTarget = null;
//            if (visibleEnemies.Count != 0)
//            {
//                visibleEnemies.Sort((a, b) =>
//                {
//                    float aAngle = AdvancedMaths.AngleFromUp(playerTransform, a.transform.position);
//                    float bAngle = AdvancedMaths.AngleFromUp(playerTransform, b.transform.position);
//                    return aAngle.CompareTo(bAngle);
//                });
//                int currentTargetIndex = visibleEnemies.IndexOf((EnemyBehaviour) _player.GetTarget());
//                currentTargetIndex += direction;
//                if (currentTargetIndex == visibleEnemies.Count) currentTargetIndex = 0;
//                if (currentTargetIndex == -1) currentTargetIndex = visibleEnemies.Count - 1;
//                newTarget = visibleEnemies[currentTargetIndex];
//            }
//            Player().SetTarget(newTarget);
//        }

//        private void SelectClockwise()
//        {
//            SelectEnemy(1);
//        }
//
//        private void SelectAntiClockwise()
//        {
//            SelectEnemy(-1);
//        }

        public static EnemyBehaviour QueueEnemyToAdd(EnemyType type)
        {
            Enemy e = Instance()._currentRegion.AddEnemy(type, 10);
            EnemyBehaviour enemyBehaviour = e.GetEnemyBehaviour();
            (enemyBehaviour as UnarmedBehaviour)?.Alert(false);
            Instance().AddEnemy(enemyBehaviour);
            return enemyBehaviour;
        }

        public static void Remove(EnemyBehaviour enemy)
        {
            Instance()._enemies.Remove(enemy);
//            if (Instance()._enemies.Count != 0) Instance().SelectClockwise();
        }

        private void AddEnemy(EnemyBehaviour e)
        {
            _enemies.Add(e);
        }

        public static EnemyBehaviour NearestEnemy()
        {
            EnemyBehaviour nearestEnemy = null;
            float nearestDistance = 10000;
            EnemiesOnScreen().ForEach(e =>
            {
                float distance = e.DistanceToTarget();
                if (distance >= nearestDistance) return;
                nearestDistance = distance;
                nearestEnemy = e;
            });
            return nearestEnemy;
        }

        public static List<EnemyBehaviour> Enemies() => Instance()._enemies;
    }
}