using System.Collections;
using DG.Tweening;
using Game.Combat.Enemies;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation.Shrines
{
    public class WaveShrine : ShrineBehaviour
    {
        private int _shrineLevelMax;
        private int _currentShrineLevel = 1;
        private const float SpawnRadius = 3f;
        private const float SpawnDelay = 0.25f;
        private float waveDuration, currentAngle;
        private static GameObject _filledIndicatorPrefab;

        private void Initialise(int shrineLevel)
        {
            _shrineLevelMax = shrineLevel;
            DangerIndicator.sprite = Resources.Load<Sprite>("Images/Danger Indicators/Evil Circle " + _shrineLevelMax);
        }

        public void Start()
        {
            Initialise(3);
        }

        private void AddIndicator()
        {
            if (_filledIndicatorPrefab == null) _filledIndicatorPrefab = Resources.Load<GameObject>("Prefabs/Combat/Filled Indicator");
            GameObject filledIndicator = Instantiate(_filledIndicatorPrefab, DangerIndicator.transform, false);
            filledIndicator.transform.localRotation = Quaternion.Euler(0, 0, _currentShrineLevel * (360f / _shrineLevelMax));
        }

        protected override void StartChallenge()
        {
            StartCoroutine(SpawnWaves());
        }

        protected override string GetInstructionText() => "Defeat waves within the time limit";

        private IEnumerator SpawnWaves()
        {
            int spawnCount = _currentShrineLevel * (Mathf.FloorToInt(WorldState.Difficulty() / 10f) + 1) * 5;
            float angleInterval = 360f / spawnCount;
            currentAngle = Random.Range(0, 360);
            waveDuration = 0f;

            for (int i = 0; i < spawnCount; ++i)
            {
                if (!CombatManager.Instance().IsCombatActive()) yield return null;
                Vector2 ghoulPos = AdvancedMaths.CalculatePointOnCircle(currentAngle, SpawnRadius, transform.position);
                EnemyType enemyType = WorldState.GetAllowedNightmareEnemyTypes().RandomElement();
                EnemyBehaviour enemy = CombatManager.Instance().SpawnEnemy(enemyType, ghoulPos);
                waveDuration += enemy.Enemy.Template.Value;
                yield return new WaitForSeconds(SpawnDelay);
                currentAngle += angleInterval;
                if (currentAngle > 360f) currentAngle -= 360f;
            }

            waveDuration *= 5f;
            float currentTime = waveDuration;
            UpdateCountdown(currentTime, waveDuration, true);
            while (currentTime > 0f)
            {
                if (!CombatManager.Instance().IsCombatActive()) yield return null;
                currentTime -= Time.deltaTime;
                UpdateCountdown(currentTime, waveDuration);
                if (CombatManager.Instance().ClearOfEnemies()) break;
                yield return null;
            }

            EndChallenge();
        }

        protected override void EndChallenge()
        {
            if (CombatManager.Instance().ClearOfEnemies())
            {
                AddIndicator();
                if (_currentShrineLevel == _shrineLevelMax)
                {
                    Succeed();
                    base.EndChallenge();
                    return;
                }

                ++_currentShrineLevel;
                StartCoroutine(SpawnWaves());
            }
            else
            {
                Fail();
                base.EndChallenge();
            }
        }
    }
}