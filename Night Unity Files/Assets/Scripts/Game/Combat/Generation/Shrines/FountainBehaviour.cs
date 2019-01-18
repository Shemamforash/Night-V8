using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Exploration.Regions;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation.Shrines
{
    public class FountainBehaviour : BasicShrineBehaviour, ICombatEvent
    {
        private static GameObject _fountainPrefab;
        private static FountainBehaviour _instance;

        private List<EnemyBehaviour> _enemies;
        private Region _region;
        private ParticleSystem[] _particleSystems;
        private AudioSource _audioSource;

        public void Awake()
        {
            _instance = this;
            _particleSystems = transform.GetComponentsInChildren<ParticleSystem>();
            _audioSource = GetComponent<AudioSource>();
        }

        private void OnDestroy()
        {
            Debug.Log("destroyed fountain");
            _instance = null;
        }

        public static FountainBehaviour Instance()
        {
            return _instance;
        }

        public static void Generate(Region region)
        {
            if (_fountainPrefab == null) _fountainPrefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Fountain");
            GameObject riteShrineObject = Instantiate(_fountainPrefab);
            riteShrineObject.GetComponent<FountainBehaviour>().Initialise(region);
        }

        private void Initialise(Region region)
        {
            _region = region;
            transform.position = Vector2.zero;
            PathingGrid.AddBlockingArea(Vector2.zero, 1.5f);
            if (!_region.FountainVisited) return;
            GetComponent<CompassItem>().Die();
            Destroy(this);
        }

        private IEnumerator SpawnEnemies()
        {
            int daysSpent = WorldState.GetDaysSpentHere() + 5;
            List<EnemyTemplate> allowedEnemies = WorldState.GetAllowedHumanEnemyTypes();
            float timeToSpawn = 0f;
            for (int i = 0; i < Random.Range(daysSpent / 2f, daysSpent); ++i)
            {
                if (!CombatManager.IsCombatActive()) yield return null;
                while (timeToSpawn > 0f)
                {
                    if (!CombatManager.IsCombatActive()) yield return null;
                    timeToSpawn -= Time.deltaTime;
                    yield return null;
                }

                Vector2 spawnPosition = PathingGrid.GetCellNearMe(transform.position, 5).Position;
                SpawnTrailController.Create(transform.position, spawnPosition, allowedEnemies.RandomElement().EnemyType);
                timeToSpawn = Random.Range(0.5f, 1f);
                yield return null;
            }
        }

        protected override void OnEnemiesDead()
        {
            Succeed();
        }

        protected override void Succeed()
        {
            CharacterManager.SelectedCharacter.Attributes.Get(AttributeType.Thirst).Decrement(10);
            CharacterManager.SelectedCharacter.Attributes.Get(AttributeType.Hunger).Decrement(10);
            PlayerCombat.Instance.HealthController.Heal(1000000);
            PlayerCombat.Instance.ResetCompass();
            CombatLogController.PostLog("Health recovered");
            CombatLogController.PostLog("Hunger and Thirst restored");
            foreach (ParticleSystem system in _particleSystems) system.Stop();
            _audioSource.DOFade(0f, 2f);
        }

        protected override void StartShrine()
        {
        }

        public float InRange()
        {
            return IsInRange ? 1 : -1;
        }

        public string GetEventText()
        {
            return Triggered ? "You feel refreshed and restored" : "Drink from the fountain... [T]";
        }

        public void Activate()
        {
            if (Triggered) return;
            GetComponent<CompassItem>().Die();
            Triggered = true;
            StartCoroutine(SpawnEnemies());
        }
    }
}