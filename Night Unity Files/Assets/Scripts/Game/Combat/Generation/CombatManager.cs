﻿using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Animals;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using Game.Exploration.Weather;
using Game.Global;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Generation
{
    public class CombatManager : Menu
    {
        private readonly CooldownManager _cooldowns = new CooldownManager();
        private Region _currentRegion;
        private bool _inMelee;
        private float _visibilityRange;
        private bool _inCombat;
        private readonly List<EnemyBehaviour> _enemies = new List<EnemyBehaviour>();
        private static CombatManager _instance;

        public static bool AllEnemiesDead() => Instance()._enemies.Count == 0;

        public static float VisibilityRange() => Instance()._visibilityRange;

        public static Cooldown CreateCooldown() => Instance()._cooldowns.CreateCooldown();

        public static bool InCombat() => Instance()._inCombat;

        public static Region Region() => Instance()._currentRegion;

        public override void Awake()
        {
            base.Awake();
            _instance = this;
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

        public override void Enter()
        {
            base.Enter();
            InputHandler.SetCurrentListener(PlayerCombat.Instance);
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
            Weather currentWeather = WeatherManager.CurrentWeather();
            _visibilityRange = 7f * (currentWeather?.GetVisibility() ?? 0.5f);
            _currentRegion = CharacterManager.SelectedCharacter.TravelAction.GetCurrentNode();

            GameObject worldObject = GameObject.Find("World");
            if (_currentRegion.GetRegionType() == RegionType.Temple)
            {
                worldObject.AddComponent<Temple>().Initialise(_currentRegion);
            }
            else if (_currentRegion.GetRegionType() == RegionType.Nightmare)
            {
                worldObject.AddComponent<Nightmare>().Initialise(_currentRegion);
            }
            else
            {
                switch (EnvironmentManager.CurrentEnvironment.EnvironmentType)
                {
                    case EnvironmentType.Oasis:
                        worldObject.AddComponent<Forest>().Initialise(_currentRegion);
                        break;
                    case EnvironmentType.Steppe:
                        //todo
                        worldObject.AddComponent<Forest>().Initialise(_currentRegion);
                        break;
                    case EnvironmentType.Ruins:
                        worldObject.AddComponent<Ruins>().Initialise(_currentRegion);
                        break;
                    case EnvironmentType.Defiles:
                        worldObject.AddComponent<Labyrinth>().Initialise(_currentRegion);
                        break;
                    case EnvironmentType.Wasteland:
                        worldObject.AddComponent<Canyon>().Initialise(_currentRegion);
                        break;
                }
            }

            PlayerCombat.Instance.Initialise();
            _cooldowns.Clear();

            List<List<EnemyBehaviour>> GrazerHerds = new List<List<EnemyBehaviour>>();
            List<EnemyBehaviour> currentGrazerHerd = new List<EnemyBehaviour>();
            List<List<EnemyBehaviour>> FlitHerds = new List<List<EnemyBehaviour>>();
            List<EnemyBehaviour> currentFlitHerd = new List<EnemyBehaviour>();
            Queue<EnemyBehaviour> watchers = new Queue<EnemyBehaviour>();

            _currentRegion.Enemies().ForEach(e =>
            {
                EnemyBehaviour enemyBehaviour = e.GetEnemyBehaviour();
                AddEnemy(enemyBehaviour);
                switch (e.Template.EnemyType)
                {
                    case EnemyType.Grazer:
                        if (currentGrazerHerd.Count == 0)
                            GrazerHerds.Add(currentGrazerHerd);
                        currentGrazerHerd.Add(enemyBehaviour);
                        if (Random.Range(3, 6) >= currentGrazerHerd.Count)
                            currentGrazerHerd = new List<EnemyBehaviour>();
                        break;
                    case EnemyType.Flit:
                        if (currentFlitHerd.Count == 0)
                            FlitHerds.Add(currentFlitHerd);
                        currentFlitHerd.Add(enemyBehaviour);
                        if (Random.Range(5, 12) >= currentFlitHerd.Count)
                            currentFlitHerd = new List<EnemyBehaviour>();
                        break;
                    case EnemyType.Watcher:
                        watchers.Enqueue(enemyBehaviour);
                        break;
                }
            });
            while (watchers.Count > 0)
            {
                foreach (List<EnemyBehaviour> herd in GrazerHerds)
                {
                    herd.Add(watchers.Dequeue());
                    if (watchers.Count == 0) break;
                }
            }

            PositionHerds(GrazerHerds, 2);
            PositionHerds(FlitHerds, 1);
        }

        private static void PositionHerds(List<List<EnemyBehaviour>> herds, float range)
        {
            herds.ForEach(herd =>
            {
                List<EnemyBehaviour> leaders = herd.FindAll(enemyBehaviour => enemyBehaviour is Watcher);
                leaders.ForEach(l => herd.Remove(l));
                Vector3 animalSpawnPosition = PathingGrid.GetCellNearMe(Vector2.zero, 8f, 4f).Position;
                List<Cell> cells = PathingGrid.GetCellsNearMe(PathingGrid.WorldToCellPosition(animalSpawnPosition), herd.Count, range);
                for (int i = 0; i < cells.Count; ++i)
                {
                    herd[i].transform.position = cells[i].Position;
                    if (leaders.Count == 0) return;
                    ((Grazer) herd[i]).SetLeader(Helper.RandomInList(leaders));
                }
            });
        }

        public static void ExitCombat()
        {
            if (!Instance()._inCombat) Debug.Log("Don't try and exit combat twice!");
            Instance()._inCombat = false;
            SceneChanger.ChangeScene("Map", false);
            PlayerCombat.Instance.ExitCombat();
        }

        public static List<CharacterCombat> GetCharactersInRange(Vector2 position, float range)
        {
            List<CharacterCombat> charactersInRange = new List<CharacterCombat>();
            if (Vector2.Distance(PlayerCombat.Instance.transform.position, position) <= range) charactersInRange.Add(PlayerCombat.Instance);

            foreach (EnemyBehaviour enemy in Instance()._enemies)
            {
                if (Vector2.Distance(enemy.transform.position, position) <= range)
                {
                    charactersInRange.Add(enemy);
                }
            }

            return charactersInRange;
        }

        public static List<EnemyBehaviour> EnemiesOnScreen()
        {
            return Instance()._enemies.FindAll(e => e.OnScreen());
        }

        public static EnemyBehaviour QueueEnemyToAdd(EnemyType type)
        {
            Enemy e = Instance()._currentRegion.AddEnemy(type, 10);
            EnemyBehaviour enemyBehaviour = e.GetEnemyBehaviour();
            (enemyBehaviour as UnarmedBehaviour)?.Alert();
            Instance().AddEnemy(enemyBehaviour);
            return enemyBehaviour;
        }

        public static EnemyBehaviour SpawnEnemy(EnemyType enemyType, Vector2 position)
        {
            EnemyBehaviour enemy = QueueEnemyToAdd(enemyType);
            enemy.transform.position = position;
            TeleportInOnly.TeleportObjectIn(enemy.gameObject);
            return enemy;
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