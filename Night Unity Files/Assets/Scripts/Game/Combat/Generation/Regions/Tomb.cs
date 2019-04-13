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
using NUnit.Framework;
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
        public static bool TombActive;

        private void Awake()
        {
            _secondaryCamera = GameObject.Find("Secondary Camera").GetComponent<Camera>();
        }

        protected override void Generate()
        {
#if UNITY_EDITOR
            Inventory.IncrementResource("Essence", 200);
            Inventory.Move(WeaponGenerator.GenerateWeapon());
            Inventory.Move(Accessory.Generate());
            Inventory.Move(Inscription.Generate());
            WorldState.OverrideDifficulty(((int) EnvironmentManager.CurrentEnvironmentType + 1) * 10);
#endif

            BossRingController.Create();
            if (GenerateJournals()) return;
            if (_tombPrefab == null) _tombPrefab = Resources.Load<GameObject>("Prefabs/Combat/Tomb Portal");
            Instantiate(_tombPrefab).transform.position = Vector2.zero;
        }

        private bool GenerateJournals()
        {
            if (EnvironmentManager.CurrentEnvironmentType != EnvironmentType.Wasteland) return false;
            List<JournalEntry> journals = JournalEntry.GetCorypthosLore();
            int entryNo = 0;
            for (int angle = 0; angle < 360; angle += 120)
            {
                Vector2 position = AdvancedMaths.CalculatePointOnCircle(angle, 1f, Vector2.zero);
                JournalSource journalSource = new JournalSource(position);
                journalSource.SetEntry(journals[entryNo]);
                ++entryNo;
                _journals.Add(journalSource);
                journalSource.CreateObject(true);
            }

            StartCoroutine(WaitToReadJournals());
            return true;
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
            _timeToNextEnemyType = 15f;
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
            if (TryAddEnemyType(EnemyType.Nightmare)) return;
        }

        private IEnumerator StartSpawningEnemies()
        {
            TombActive = true;
            float maxTime = 60f * 5f;
            float currentTime = maxTime;
            _timeToIncreaseMaxEnemies = 20f;
            float endAudioOffset = 60f;
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(maxTime - endAudioOffset);
            seq.AppendCallback(SpawnEndGameAudio);
            _timeToNextShake = 10f;
            _maxEnemies = 2;
            AddNextEnemyType();
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_secondaryCamera.DOColor(new Color(0.7f, 0.4f, 0.4f), maxTime - endAudioOffset).SetEase(Ease.OutExpo));
            sequence.Append(_secondaryCamera.DOColor(new Color(0.1f, 0.1f, 0.1f), endAudioOffset).SetEase(Ease.OutExpo));
            PostProcessInvertColour invertColour = Camera.main.GetComponent<PostProcessInvertColour>();
            sequence.Insert(maxTime - endAudioOffset, DOTween.To(invertColour.CurrentValue, invertColour.Set, 1f, 20f).SetEase(Ease.InOutSine));
            while (currentTime > 0f)
            {
                if (!CombatManager.Instance().IsCombatActive()) yield return null;
                AudioController.SetAmbientVolume(1f - currentTime / maxTime);
                currentTime -= Time.deltaTime;
                CheckToIncreaseMaxEnemies();
                CheckToAddEnemyType();
                CheckToSpawnEnemy();
                CheckToShakeCamera();
                yield return null;
            }

            AudioController.SetAmbientVolume(1f);
            CombatManager.Instance().LeaveCombat();
            CombatManager.Instance().ExitCombat(false);
            EnvironmentManager.NextLevel(false, false);
            StoryController.Show();
        }

        private void SpawnEndGameAudio()
        {
            TombActive = false;
            GameObject audioPrefab = Resources.Load<GameObject>("Prefabs/Combat/Bosses/End Game Audio");
            GameObject audioObject = Instantiate(audioPrefab);
            audioObject.transform.position = Vector2.zero;
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
            if (_timeToNextEnemy > 0 || CombatManager.Instance().Enemies().Count >= _maxEnemies) return;
            _timeToNextEnemy = Random.Range(1f, 2f);
            EnemyType enemyType = _allowedEnemyTypes.RandomElement();
            Vector2 position = AdvancedMaths.RandomDirection() * Random.Range(2f, 6f) + (Vector2) PlayerCombat.Position();
            CombatManager.Instance().SpawnEnemy(enemyType, position);
        }
    }
}