using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Animals;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation.Shrines;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Combat.Ui;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using Game.Exploration.Weather;
using Game.Global;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace Game.Combat.Generation
{
    public class CombatManager : Menu
    {
        private readonly CooldownManager _cooldowns = new CooldownManager();
        private static Region _currentRegion;
        private bool _inMelee;
        private float _visibilityRange;
        private bool _inCombat;
        private readonly List<ITakeDamageInterface> _enemies = new List<ITakeDamageInterface>();
        private static CombatManager _instance;
        private bool _shotFired;
        private int _enemiesKilled;
        private int _humansKilled;
        private int _damageTaken;
        private int _damageDealt;
        private int _skillsUsed;
        private int _itemsFound;
        public bool _drawGizmos;
        private TextMeshProUGUI _regionNameText;

        public static bool AllEnemiesDead() => Instance()._enemies.Count == 0;

        public static float VisibilityRange() => Instance()._visibilityRange;

        public static Cooldown CreateCooldown() => Instance()._cooldowns.CreateCooldown();

        public static bool InCombat()
        {
            return Instance() != null && Instance()._inCombat;
        }

        public static Region Region() => _currentRegion;

        public override void Awake()
        {
            Cursor.visible = false;
            base.Awake();
            _instance = this;
            GameObject regionNameObject = GameObject.Find("Screen Fader");
            _regionNameText = regionNameObject.FindChildWithName<TextMeshProUGUI>("Text");
            _regionNameText.text = _currentRegion.Name;
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

        public static void SetCurrentRegion(Region region)
        {
            _currentRegion = region;
        }

        private void EnterCombat()
        {
            _inCombat = true;
            WorldState.Pause();
            _visibilityRange = 10f;

            GameObject worldObject = GameObject.Find("World");
            if (_currentRegion.GetRegionType() == RegionType.Temple)
            {
                worldObject.AddComponent<Temple>().Initialise(_currentRegion);
            }
            else if (_currentRegion.GetRegionType() == RegionType.Nightmare)
            {
                worldObject.AddComponent<Nightmare>().Initialise(_currentRegion);
            }
            else if (_currentRegion.GetRegionType() == RegionType.Rite)
            {
                worldObject.AddComponent<Rite>().Initialise(_currentRegion);
            }
            else if (_currentRegion.GetRegionType() == RegionType.Tomb)
            {
                worldObject.AddComponent<Tomb>().Initialise(_currentRegion);
            }
            else
            {
                _visibilityRange = 5f * WeatherManager.CurrentWeather().GetVisibility() + 2f;
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
                    Grazer g = herd[i] as Grazer;
                    if (g == null) continue;
                    g.AddHerdMembers(herd);
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
            if (!Instance()._inCombat)
            {
                Debug.Log("Don't try and exit combat twice!");
                return;
            }

            BrandManager brandManager = PlayerCombat.Instance.Player.BrandManager;
            if (Enemies().Count == 0)
            {
                if (Instance()._skillsUsed == 0)
                {
                    brandManager.IncreaseBattlesNoSkills();
                }
                else if (!Instance()._shotFired)
                {
                    brandManager.IncreaseOnlySkillBattles();
                }
            }

            brandManager.IncreaseDamageDealt(Instance()._damageDealt);
            brandManager.IncreaseDamageTaken(Instance()._damageTaken);
            brandManager.IncreaseItemsFound(Instance()._itemsFound);
            brandManager.IncreaseSkillsUsed(Instance()._skillsUsed);
            brandManager.IncreaseHumansKilled(Instance()._humansKilled);

            Instance()._inCombat = false;
            Instance()._regionNameText.text = "";
            PlayerCombat.Instance.ExitCombat();
            if (!returnToMap) return;
            SceneChanger.GoToMapScene();
        }

        public static List<ITakeDamageInterface> GetCharactersInRange(Vector2 position, float range)
        {
            List<ITakeDamageInterface> charactersInRange = GetEnemiesInRange(position, range);
            if (Vector2.Distance(PlayerCombat.Instance.transform.position, position) <= range) charactersInRange.Add(PlayerCombat.Instance);
            return charactersInRange;
        }

        public static List<ITakeDamageInterface> GetEnemiesInRange(Vector2 position, float range)
        {
            return new List<ITakeDamageInterface>(Instance()._enemies.Where(e => Vector2.Distance(e.GetGameObject().transform.position, position) <= range));
        }

        public static List<ITakeDamageInterface> EnemiesOnScreen()
        {
            return Instance()._enemies.FindAll(e => e.GetGameObject().gameObject.OnScreen());
        }

        public static EnemyBehaviour QueueEnemyToAdd(EnemyTemplate type, CharacterCombat target = null)
        {
            Enemy e = _currentRegion.AddEnemy(type);
            EnemyBehaviour enemyBehaviour = e.GetEnemyBehaviour();
            if (target != null) enemyBehaviour.SetTarget(target);
            (enemyBehaviour as UnarmedBehaviour)?.Alert(true);
            Instance().AddEnemy(enemyBehaviour);
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

        public static void Remove(ITakeDamageInterface enemy)
        {
            Instance()._enemies.Remove(enemy);
        }

        private void AddEnemy(EnemyBehaviour e)
        {
            _enemies.Add(e);
        }

        public static ITakeDamageInterface NearestEnemy(Vector2 position)
        {
            ITakeDamageInterface nearestEnemy = null;
            float nearestDistance = 10000;
            EnemiesOnScreen().ForEach(e =>
            {
                float distance = Vector2.Distance(e.GetGameObject().transform.position, position);
                if (distance >= nearestDistance) return;
                nearestDistance = distance;
                nearestEnemy = e;
            });
            return nearestEnemy;
        }

        public static ITakeDamageInterface NearestCharacter(Vector2 position)
        {
            ITakeDamageInterface nearestEnemy = NearestEnemy(position);
            float playerDistance = Vector2.Distance(position, PlayerCombat.Instance.transform.position);
            float enemyDistance = Vector2.Distance(position, nearestEnemy.GetGameObject().transform.position);
            return playerDistance < enemyDistance ? PlayerCombat.Instance : nearestEnemy;
        }

        public static List<ITakeDamageInterface> Enemies() => Instance()._enemies;

        public static void IncreaseSkillsUsed()
        {
            ++Instance()._skillsUsed;
        }

        public static void IncreaseDamageDealt(int damageDealt)
        {
            Instance()._damageDealt += damageDealt;
        }

        public static void IncreaseDamageTaken(int damageTaken)
        {
            Instance()._damageTaken += damageTaken;
        }

        public static void IncreaseItemsFound()
        {
            ++Instance()._itemsFound;
        }

        public static void IncreaseHumansKilled()
        {
            ++Instance()._humansKilled;
        }

        public static void SetHasFiredShot()
        {
            Instance()._shotFired = true;
        }
    }
}