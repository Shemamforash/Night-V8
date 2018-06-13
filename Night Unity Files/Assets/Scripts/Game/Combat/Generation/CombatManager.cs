using System;
using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Exploration.Regions;
using Game.Global;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.Input;
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
            _visibilityRange = 5;
            _currentRegion = CharacterManager.SelectedCharacter.TravelAction.GetCurrentNode();
            
            switch (_currentRegion.GetRegionType())
            {
                case RegionType.Shelter:
                    break;
                case RegionType.Gate:
                    break;
                case RegionType.Temple:
                    break;
                case RegionType.Resource:
                    break;
                case RegionType.Danger:
                    GameObject.Find("World").AddComponent<Ruins>().Initialise(_currentRegion);
                    break;
                case RegionType.Nightmare:
                    GameObject.Find("World").AddComponent<Nightmare>().Initialise(_currentRegion);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            PlayerCombat.Instance.Initialise();
            _cooldowns.Clear();
            _currentRegion.Enemies().ForEach(e => { AddEnemy(e.GetEnemyBehaviour()); });
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