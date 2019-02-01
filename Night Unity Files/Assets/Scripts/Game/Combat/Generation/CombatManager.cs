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
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;
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
        private CanvasGroup _hudCanvas;
        private bool _hudShown;
        private Tweener _hudTween;
        private float _timeSinceLastSpawn;
        private static bool _forceShowHud;

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

        public static bool AllEnemiesDead() => Instance()._enemies.Count == 0 && _inactiveEnemies.Count == 0;

        public static int InactiveEnemyCount() => _inactiveEnemies.Count;

        public static float VisibilityRange() => Instance()._visibilityRange;

        public static Region Region() => _currentRegion;

        public override void Awake()
        {
            base.Awake();
            PauseOnOpen = false;
            _hudCanvas = gameObject.FindChildWithName<CanvasGroup>("Combat");
            _hudCanvas.alpha = 0f;
            _instance = this;
            Resume();
        }

        public static void SetForceShowHud(bool forceShow) => _forceShowHud = forceShow;

        private void UpdateHud()
        {
            bool needToShow = !ClearOfEnemies() || _forceShowHud;
            bool needToHide = ClearOfEnemies() && !_forceShowHud;
            if (needToShow && !_hudShown)
            {
                _hudTween?.Kill();
                _hudTween = _hudCanvas.DOFade(1f, 1f);
                _hudShown = true;
            }
            else if (needToHide && _hudShown)
            {
                _hudTween?.Kill();
                _hudTween = _hudCanvas.DOFade(0f, 1f);
                _hudShown = false;
            }
        }

        public void Update()
        {
            if (!IsCombatActive()) return;
            UpdateHud();
            TrySpawnNewEnemy();
            AIMoveManager.UpdateMoveBehaviours();
            if (Time.timeSinceLevelLoad < 1f) return;
            PlayerCombat.Instance.MyUpdate();
            for (int i = _enemies.Count - 1; i >= 0; --i)
                _enemies[i].MyUpdate();
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
            _forceShowHud = false;
            InputHandler.SetCurrentListener(PlayerCombat.Instance);
        }

        public static bool IsPlayerInCombat()
        {
            return Instance() != null && _inCombat && PlayerCombat.Instance != null;
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
            ParticleSystem _nightmareParticles = GameObject.Find("Nightmare Particles").GetComponent<ParticleSystem>();
            ParticleSystem.ShapeModule shape = _nightmareParticles.shape;
            shape.radius = WorldGrid.CombatAreaWidth / 2f + 6f;
            _nightmareParticles.Play();
        }

        private void SetUpDynamicRegion()
        {
            GameObject worldObject = GameObject.Find("World");
            switch (EnvironmentManager.CurrentEnvironmentType())
            {
                case EnvironmentType.Desert:
                    worldObject.AddComponent<Desert>().Initialise(_currentRegion);
                    break;
                case EnvironmentType.Mountains:
                    worldObject.AddComponent<Labyrinth>().Initialise(_currentRegion);
                    break;
                case EnvironmentType.Ruins:
                    worldObject.AddComponent<Ruins>().Initialise(_currentRegion);
                    break;
                case EnvironmentType.Sea:
                    worldObject.AddComponent<Steppe>().Initialise(_currentRegion);
                    break;
                case EnvironmentType.Wasteland:
                    worldObject.AddComponent<Canyon>().Initialise(_currentRegion);
                    break;
            }
        }

        private void SetUpNonDynamicRegion()
        {
            GameObject worldObject = GameObject.Find("World");
            switch (_currentRegion.GetRegionType())
            {
                case RegionType.Tomb:
                    worldObject.AddComponent<Tomb>().Initialise(_currentRegion);
                    break;
                case RegionType.Rite:
                    worldObject.AddComponent<Rite>().Initialise(_currentRegion);
                    break;
                case RegionType.Temple:
                    worldObject.AddComponent<Temple>().Initialise(_currentRegion);
                    break;
                case RegionType.Tutorial:
                    worldObject.AddComponent<Tutorial>().Initialise(_currentRegion);
                    break;
            }

            AudioController.FadeWeatherOut();
            PlayNightmareParticles();
            _visibilityRange = 10f;
        }

        private void CalculateVisibility()
        {
            float visibilityModifier = 0.5f * Mathf.Sin((WorldState.Hours - 6) * Mathf.PI / 12f) + 0.5f;
            _visibilityRange *= WeatherManager.CurrentWeather().GetVisibility();
            _visibilityRange = Mathf.Lerp(3f, 8f, visibilityModifier);
        }

        private void EnterCombat()
        {
            _inCombat = true;
            CalculateVisibility();
            if (_currentRegion.IsDynamic()) SetUpDynamicRegion();
            else SetUpNonDynamicRegion();
            PlayerCombat.Instance.Initialise();
            _inactiveEnemies = _currentRegion.GetEnemies();
            _maxSize = WorldState.Difficulty() / 10 + 2;
            PlaceAnimals();
            _currentRegion.CheckForRegionExplored();
        }

        public static void ExitCombat(bool returnToMap = true)
        {
            if (!_inCombat) return;
            if (_currentRegion.IsDynamic())
            {
                _inactiveEnemies.ForEach(e => _currentRegion.RestoreSize(e.Template.Value));
                _instance._enemies.ForEach(e => _currentRegion.RestoreSize(((EnemyBehaviour) e).Enemy.Template.Value));
            }
            else
            {
                AudioController.FadeWeatherIn();
            }

            _inCombat = false;
            PlayerCombat.Instance.ExitCombat();
            if (returnToMap) ReturnToMap();
            _instance = null;
        }

        public static void ClearInactiveEnemies() => _inactiveEnemies.Clear();

        public static void OverrideMaxSize(int maxSize) => _maxSize = maxSize;

        public static void OverrideInactiveEnemies(List<Enemy> inactiveEnemies) => _inactiveEnemies = inactiveEnemies;

        private void TrySpawnNewEnemy()
        {
            if (_inactiveEnemies.Empty()) return;
            if (_enemies.Count >= _maxSize) return;
            _timeSinceLastSpawn -= Time.deltaTime;
            if (_timeSinceLastSpawn > 0 && _enemies.Count > 0) return;
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
                Vector3 animalSpawnPosition = WorldGrid.GetCellNearMe(Vector2.zero, 8f, 4f).Position;
                List<Cell> cells = WorldGrid.GetCellsNearMe(WorldGrid.WorldToCellPosition(animalSpawnPosition), herd.Count, range);
                for (int i = 0; i < cells.Count; ++i)
                {
                    herd[i].transform.position = cells[i].Position;
                }
            });
        }

        private void OnDrawGizmos()
        {
            if (!_drawGizmos) return;
            Vector3 cubeSize = 1f / WorldGrid.CellResolution * Vector3.one;
            Gizmos.color = Color.red;
            List<Cell> invalid = WorldGrid.InvalidCells();
            invalid.ForEach(c => { Gizmos.DrawCube(c.Position, cubeSize); });
            Gizmos.color = Color.yellow;
            WorldGrid._outOfRangeList.ForEach(c => { Gizmos.DrawCube(c.Position, cubeSize); });
            WorldGrid._edgePositionList.ForEach(c =>
            {
                {
                    Gizmos.color = new Color(0, 1, 0, 0.25f); //Color.green;
                    Gizmos.DrawCube(c.Position, cubeSize);
                }
            });
        }

        private static void ReturnToMap()
        {
            if (_currentRegion.GetRegionType() == RegionType.Rite) _currentRegion = Rite.GetLastRegion();
            SceneChanger.GoToGameScene();
            MapMenuController.IsReturningFromCombat = true;
            InputHandler.SetCurrentListener(null);
        }

        public static List<CanTakeDamage> GetCharactersInRange(Vector2 position, float range)
        {
            List<CanTakeDamage> charactersInRange = GetEnemiesInRange(position, range);
            if (Vector2.Distance(PlayerCombat.Position(), position) <= range) charactersInRange.Add(PlayerCombat.Instance);
            return charactersInRange;
        }

        public static List<CanTakeDamage> GetEnemiesInRange(Vector2 position, float range)
        {
            return new List<CanTakeDamage>(Instance()._enemies.Where(e => Vector2.Distance(e.transform.position, position) <= range));
        }

        public static void RemoveEnemy(CanTakeDamage enemy)
        {
            Instance()._enemies.Remove(enemy);
        }

        public static void AddEnemy(CanTakeDamage enemy)
        {
            Instance()._enemies.Add(enemy);
        }


        public static EnemyBehaviour QueueEnemyToAdd(EnemyTemplate template)
        {
            Enemy e = template.Create();
            return e.GetEnemyBehaviour();
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
            TeleportInOnly.TeleportIn(position);
            return enemy;
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
            float playerDistance = Vector2.Distance(position, PlayerCombat.Position());
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

        public static bool ClearOfEnemies()
        {
            return Enemies().Count == 0 && _inactiveEnemies.Count == 0;
        }

        public static void SetInCombat(bool b)
        {
            _inCombat = false;
        }
    }
}