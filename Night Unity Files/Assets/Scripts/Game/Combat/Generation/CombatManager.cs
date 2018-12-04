using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Game.Characters;
using Game.Characters.CharacterActions;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using Game.Exploration.Weather;
using Game.Global;
using NUnit.Framework;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace Game.Combat.Generation
{
    public class CombatManager : Menu
    {
        private static Region _currentRegion;
        private float _visibilityRange;
        private static bool _inCombat;
        private readonly List<CanTakeDamage> _enemies = new List<CanTakeDamage>();
        private static CombatManager _instance;
        public bool _drawGizmos;
        private static bool _paused;
        private static List<Enemy> _inactiveEnemies;
        private static int _maxSize;
        private static int _currentSize;

        public static List<EnemyTemplate> GenerateEnemies(int size, List<EnemyTemplate> allowedTypes)
        {
            List<EnemyTemplate> templates = new List<EnemyTemplate>();
            while (size > 0)
            {
                allowedTypes.Shuffle();
                foreach (EnemyTemplate e in allowedTypes)
                {
                    if (e.Value > size) continue;
                    templates.Add(e);
                    size -= e.Value;
                    break;
                }
            }

            return templates;
        }

        public static bool AllEnemiesDead() => Instance()._enemies.Count == 0;

        public static float VisibilityRange() => Instance()._visibilityRange;

        public static Region Region() => _currentRegion;

        public override void Awake()
        {
            base.Awake();
            _instance = this;
            Resume();
            ScreenFaderController.ShowText(_currentRegion.Name);
        }

        public void Update()
        {
            if (!IsCombatActive()) return;
            UpdateEnemiesOnScreen();
            AIMoveManager.UpdateMoveBehaviours();
            if (Time.timeSinceLevelLoad < 1f) return;
            PlayerCombat.Instance.MyUpdate();
            for (int i = _enemies.Count - 1; i >= 0; --i)
            {
                _enemies[i].MyUpdate();
            }
        }

        public static CombatManager Instance()
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
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(3f);
            sequence.AppendCallback(() =>
            {
                if (PlayerCombat.Instance == null) return;
                TutorialManager.TryOpenTutorial(3);
            });
        }

        public static bool IsPlayerInCombat()
        {
            return Instance() != null && _inCombat;
        }

        public static bool IsCombatActive()
        {
            return IsPlayerInCombat() && !IsCombatPaused();
        }

        public static bool IsCombatPaused()
        {
            return Instance() != null && _paused;
        }

        public static void SetCurrentRegion(Region region)
        {
            _currentRegion = region;
        }

        private void PlayNightmareParticles()
        {
            ParticleSystem _nightmareParticles = transform.Find("Nightmare Particles").GetComponent<ParticleSystem>();
            _nightmareParticles.Play();
            ParticleSystem.ShapeModule shape = _nightmareParticles.shape;
            shape.radius = PathingGrid.CombatAreaWidth + 4;
        }

        private void EnterCombat()
        {
            _inCombat = true;
            WorldState.Pause();
            float visibilityModifier = 0.5f * Mathf.Sin((WorldState.Hours - 6) * Mathf.PI / 12f) + 0.5f;
            _visibilityRange *= WeatherManager.CurrentWeather().GetVisibility();
            _visibilityRange = Mathf.Lerp(2f, 8f, visibilityModifier);
            GameObject worldObject = GameObject.Find("World");
            if (_currentRegion.GetRegionType() == RegionType.Temple)
            {
                worldObject.AddComponent<Temple>().Initialise(_currentRegion);
            }
            else if (_currentRegion.GetRegionType() == RegionType.Nightmare)
            {
                PlayNightmareParticles();
                worldObject.AddComponent<Nightmare>().Initialise(_currentRegion);
            }
            else if (_currentRegion.GetRegionType() == RegionType.Rite)
            {
                PlayNightmareParticles();
                worldObject.AddComponent<Rite>().Initialise(_currentRegion);
            }
            else if (_currentRegion.GetRegionType() == RegionType.Tomb)
            {
                PlayNightmareParticles();
                worldObject.AddComponent<Tomb>().Initialise(_currentRegion);
            }
            else
            {
                switch (EnvironmentManager.CurrentEnvironment.EnvironmentType)
                {
                    case EnvironmentType.Desert:
                        worldObject.AddComponent<Desert>().Initialise(_currentRegion);
                        break;
                    case EnvironmentType.Mountains:
                        worldObject.AddComponent<Steppe>().Initialise(_currentRegion);
                        break;
                    case EnvironmentType.Ruins:
                        worldObject.AddComponent<Ruins>().Initialise(_currentRegion);
                        break;
                    case EnvironmentType.Sea:
                        worldObject.AddComponent<Labyrinth>().Initialise(_currentRegion);
                        break;
                    case EnvironmentType.Wasteland:
                        worldObject.AddComponent<Canyon>().Initialise(_currentRegion);
                        break;
                }
            }

            PlayerCombat.Instance.Initialise();
            _inactiveEnemies = _currentRegion.GetEnemies();
            _maxSize = WorldState.Difficulty() / 10 + 2;
            _currentSize = 0;
            PlaceAnimals();
        }

        private float _timeSinceLastSpawn;

        private void UpdateEnemiesOnScreen()
        {
            if (_inactiveEnemies.Empty()) return;
            if (_currentSize >= _maxSize) return;
            _timeSinceLastSpawn -= Time.deltaTime;
            if (_timeSinceLastSpawn > 0) return;
            _timeSinceLastSpawn = Random.Range(1f, 3f);
            Enemy e = _inactiveEnemies.RemoveLast();
            e.GetEnemyBehaviour();
        }

        private static void PlaceAnimals()
        {
            if (_currentRegion.GetRegionType() != RegionType.Animal) return;
            List<List<EnemyBehaviour>> GrazerHerds = new List<List<EnemyBehaviour>>();
            List<EnemyBehaviour> currentGrazerHerd = new List<EnemyBehaviour>();
            List<List<EnemyBehaviour>> FlitHerds = new List<List<EnemyBehaviour>>();
            List<EnemyBehaviour> currentFlitHerd = new List<EnemyBehaviour>();
            _inactiveEnemies.ForEach(e =>
            {
                EnemyBehaviour enemyBehaviour = e.GetEnemyBehaviour();
                switch (e.Template.EnemyType)
                {
                    case EnemyType.Grazer:
                        if (currentGrazerHerd.Count == 0)
                            GrazerHerds.Add(currentGrazerHerd);
                        currentGrazerHerd.Add(enemyBehaviour);
                        if (Random.Range(3, 6) >= currentGrazerHerd.Count)
                            currentGrazerHerd = new List<EnemyBehaviour>();
                        break;
                    case EnemyType.Curio:
                        if (currentFlitHerd.Count == 0)
                            FlitHerds.Add(currentFlitHerd);
                        currentFlitHerd.Add(enemyBehaviour);
                        if (Random.Range(5, 12) >= currentFlitHerd.Count)
                            currentFlitHerd = new List<EnemyBehaviour>();
                        break;
                }
            });

            _inactiveEnemies.Clear();
            PositionHerds(GrazerHerds, 1.5f);
            PositionHerds(FlitHerds, 1);
        }

        private static void PositionHerds(List<List<EnemyBehaviour>> herds, float range)
        {
            herds.ForEach(herd =>
            {
                Vector3 animalSpawnPosition = PathingGrid.GetCellNearMe(Vector2.zero, 8f, 4f).Position;
                List<Cell> cells = PathingGrid.GetCellsNearMe(PathingGrid.WorldToCellPosition(animalSpawnPosition), herd.Count, range);
                for (int i = 0; i < cells.Count; ++i)
                {
                    herd[i].transform.position = cells[i].Position;
                }
            });
        }

        private void OnDrawGizmos()
        {
            if (!_drawGizmos) return;
            Vector3 cubeSize = 1f / PathingGrid.CellResolution * Vector3.one;
            Gizmos.color = Color.red;
            List<Cell> invalid = PathingGrid._invalidCells.ToList();
            invalid.ForEach(c => { Gizmos.DrawCube(c.Position, cubeSize); });
            Gizmos.color = Color.yellow;
            PathingGrid._outOfRangeList.ForEach(c => { Gizmos.DrawCube(c.Position, cubeSize); });
            Gizmos.color = new Color(0, 1, 0, 0.25f); //Color.green;
            PathingGrid._edgePositionList.ForEach(c => { Gizmos.DrawCube(c.Position, cubeSize); });
        }

        public static void ExitCombat(bool returnToMap = true)
        {
            if (!_inCombat)
            {
                Debug.Log("Don't try and exit combat twice!");
                return;
            }

            _inactiveEnemies.ForEach(e => _currentRegion.RestoreSize(e.Template.Value));
            _instance._enemies.ForEach(e => _currentRegion.RestoreSize(((EnemyBehaviour) e).Enemy.Template.Value));
            _inCombat = false;
            ScreenFaderController.HideText();
            PlayerCombat.Instance.ExitCombat();
            ChangeScene(returnToMap);
            _instance = null;
        }

        private static void ChangeScene(bool returnToMap)
        {
            if (!returnToMap) return;
            if (CharacterManager.SelectedCharacter.CanAffordTravel())
            {
                SceneChanger.GoToGameScene();
                MapMenuController.IsReturningFromCombat = true;
                return;
            }

            Travel travelAction = CharacterManager.SelectedCharacter.TravelAction;
            if (_currentRegion.GetRegionType() == RegionType.Rite) _currentRegion = Rite.GetLastRegion();
            int gritCost = RoutePlotter.RouteBetween(_currentRegion, MapGenerator.GetInitialNode()).Count - 1;
            travelAction.TravelTo(MapGenerator.GetInitialNode(), gritCost);
            SceneChanger.GoToGameScene();
            InputHandler.SetCurrentListener(null);
        }

        public static List<CanTakeDamage> GetCharactersInRange(Vector2 position, float range)
        {
            List<CanTakeDamage> charactersInRange = GetEnemiesInRange(position, range);
            if (Vector2.Distance(PlayerCombat.Instance.transform.position, position) <= range) charactersInRange.Add(PlayerCombat.Instance);
            return charactersInRange;
        }

        public static List<CanTakeDamage> GetEnemiesInRange(Vector2 position, float range)
        {
            return new List<CanTakeDamage>(Instance()._enemies.Where(e => Vector2.Distance(e.transform.position, position) <= range));
        }

        public static EnemyBehaviour QueueEnemyToAdd(EnemyTemplate type, CharacterCombat target = null)
        {
            Enemy e = type.Create();
            EnemyBehaviour enemyBehaviour = e.GetEnemyBehaviour();
            if (target != null) enemyBehaviour.SetTarget(target);
            return enemyBehaviour;
        }

        public static EnemyBehaviour QueueEnemyToAdd(EnemyType type)
        {
            return QueueEnemyToAdd(EnemyTemplate.GetEnemyTemplate(type));
        }

        public static EnemyBehaviour SpawnEnemy(EnemyType enemyType, Vector2 position)
        {
            EnemyTemplate template = EnemyTemplate.GetEnemyTemplate(enemyType);
            EnemyBehaviour enemy = QueueEnemyToAdd(template);
            enemy.transform.position = position;
            TeleportInOnly.TeleportObjectIn(enemy.gameObject);
            return enemy;
        }

        public static void RemoveEnemy(CanTakeDamage enemy)
        {
            --_currentSize;
            Assert.IsTrue(_currentSize >= 0);
            Instance()._enemies.Remove(enemy);
        }

        public void AddEnemy(CanTakeDamage e)
        {
            ++_currentSize;
            _enemies.Add(e);
        }

        public static CanTakeDamage NearestEnemy(Vector2 position)
        {
            CanTakeDamage nearestEnemy = null;
            float nearestDistance = 10000;
            Enemies().ForEach(e =>
            {
                float distance = Vector2.Distance(e.transform.position, position);
                if (distance >= nearestDistance) return;
                nearestDistance = distance;
                nearestEnemy = e;
            });
            return nearestEnemy;
        }

        public static CanTakeDamage NearestCharacter(Vector2 position)
        {
            CanTakeDamage nearestEnemy = NearestEnemy(position);
            float playerDistance = Vector2.Distance(position, PlayerCombat.Instance.transform.position);
            float enemyDistance = Vector2.Distance(position, nearestEnemy.transform.position);
            return playerDistance < enemyDistance ? PlayerCombat.Instance : nearestEnemy;
        }

        public static List<CanTakeDamage> Enemies() => Instance()._enemies;

        public static void Pause()
        {
            if (_instance == null) return;
            _paused = true;
            Time.timeScale = 0;
        }

        public static void Resume()
        {
            if (_instance == null) return;
            _paused = false;
            Time.timeScale = 1;
        }

        public static Region GetCurrentRegion()
        {
            return _currentRegion;
        }
    }
}