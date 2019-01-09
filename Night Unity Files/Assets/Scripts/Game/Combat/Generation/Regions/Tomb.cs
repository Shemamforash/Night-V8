using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Game.Combat.Enemies;
using Game.Combat.Player;
using Game.Exploration.Environment;
using Game.Gear;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation
{
    public class Tomb : RegionGenerator //not a mine
    {
        private static GameObject _tombPrefab;
        private List<JournalSource> _journals = new List<JournalSource>();
        private int _maxEnemies;
        private float _timeToIncreaseMaxEnemies;
        private List<EnemyType> _allowedEnemyTypes = new List<EnemyType>();
        private float _timeToNextEnemyType;
        private float _timeToNextEnemy;
        private Camera _secondaryCamera;
        private float _timeToNextShake;
        private float _shakeModifier = 1;

        private void Awake()
        {
            _secondaryCamera = GameObject.Find("Secondary Camera").GetComponent<Camera>();
        }

        protected override void Generate()
        {
#if UNITY_EDITOR
            Inventory.IncrementResource("Essence", 200);
            Inventory.Move(WeaponGenerator.GenerateWeapon(ItemQuality.Shining));
            Inventory.Move(Accessory.Generate(ItemQuality.Shining));
            Inventory.Move(Inscription.Generate(ItemQuality.Shining));
#endif

            GenerateJournals();
            if (_tombPrefab == null) _tombPrefab = Resources.Load<GameObject>("Prefabs/Combat/Tomb Portal");
            Instantiate(_tombPrefab).transform.position = Vector2.zero;
        }

        private void GenerateJournals()
        {
            if (EnvironmentManager.CurrentEnvironmentType() != EnvironmentType.Wasteland) return;
            for (int angle = 0; angle < 360; angle += 120)
            {
                Vector2 position = AdvancedMaths.CalculatePointOnCircle(angle, 2f, Vector2.zero);
                JournalSource journalSource = new JournalSource(position);
                journalSource.SetEntry(JournalEntry.GetEntry(17));
                _journals.Add(journalSource);
                journalSource.CreateObject(true);
            }

            StartCoroutine(WaitToReadJournals());
        }

        private IEnumerator WaitToReadJournals()
        {
            while (_journals.Any(j => !j.Read())) yield return null;
            yield return null;
            StartCoroutine(StartSpawningEnemies());
        }

        private void CheckToIncreaseMaxEnemies()
        {
            _timeToIncreaseMaxEnemies -= Time.deltaTime;
            if (_timeToIncreaseMaxEnemies > 0) return;
            ++_maxEnemies;
            _timeToIncreaseMaxEnemies = 20;
        }

        private void CheckToAddEnemyType()
        {
            _timeToNextEnemyType -= Time.deltaTime;
            if (_timeToNextEnemyType > 0) return;
            AddNextEnemyType();
        }

        private bool TryAddEnemyType(EnemyType enemyType)
        {
            if (_allowedEnemyTypes.Contains(enemyType)) return false;
            _allowedEnemyTypes.Add(enemyType);
            return true;
        }

        private void AddNextEnemyType()
        {
            _timeToNextEnemyType = 25f;
            if (TryAddEnemyType(EnemyType.Ghoul)) return;
            if (TryAddEnemyType(EnemyType.Brawler)) return;
            if (TryAddEnemyType(EnemyType.Ghast)) return;
            if (TryAddEnemyType(EnemyType.Sentinel)) return;
            if (TryAddEnemyType(EnemyType.Martyr)) return;
            if (TryAddEnemyType(EnemyType.Shadow)) return;
            if (TryAddEnemyType(EnemyType.Sniper)) return;
            if (TryAddEnemyType(EnemyType.Maelstrom)) return;
            if (TryAddEnemyType(EnemyType.Warlord)) return;
            if (TryAddEnemyType(EnemyType.Revenant)) return;
            if (TryAddEnemyType(EnemyType.Mountain)) return;
            if (TryAddEnemyType(EnemyType.Witch)) return;
        }

        private IEnumerator StartSpawningEnemies()
        {
            float maxTime = 60f * 5f;
            float currentTime = maxTime;
            _timeToIncreaseMaxEnemies = 20f;
            _timeToNextShake = 10f;
            _maxEnemies = 2;
            AddNextEnemyType();
            _secondaryCamera.DOColor(new Color(0.6f, 0.3f, 0.3f), maxTime).SetEase(Ease.InExpo);
            while (currentTime > 0f)
            {
                if (!CombatManager.IsCombatActive()) yield return null;
                currentTime -= Time.deltaTime;
                CheckToIncreaseMaxEnemies();
                CheckToAddEnemyType();
                CheckToSpawnEnemy();
                CheckToShakeCamera();
                yield return null;
            }

            CombatManager.SetInCombat(false);
            float flashDuration = 3f;
            ScreenFaderController.FlashWhite(flashDuration, Color.black);
            yield return new WaitForSeconds(flashDuration);
            StoryController.ShowText(JournalEntry.GetStoryText());
        }

        private void CheckToShakeCamera()
        {
            _timeToNextShake -= Time.deltaTime;
            if (_timeToNextShake > 0) return;
            PlayerCombat.Instance.Shake(10 / _shakeModifier);
            _timeToNextShake = 10f * _shakeModifier;
            _timeToNextShake = Random.Range(_timeToNextShake * 0.8f, _timeToNextShake * 1.2f);
            _shakeModifier *= 0.98f;
        }

        private void CheckToSpawnEnemy()
        {
            _timeToNextEnemy -= Time.deltaTime;
            if (_timeToNextEnemy > 0 || CombatManager.Enemies().Count >= _maxEnemies) return;
            _timeToNextEnemy = Random.Range(1f, 2f);
            EnemyType enemyType = _allowedEnemyTypes.RandomElement();
            Vector2 position = AdvancedMaths.RandomDirection() * Random.Range(2f, 6f) + (Vector2) PlayerCombat.Position();
            CombatManager.SpawnEnemy(enemyType, position);
        }
    }
}