using System.Collections;
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

        public void Initialise(int shrineLevel)
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
            GameObject filledIndicator = Instantiate(_filledIndicatorPrefab);
            filledIndicator.transform.SetParent(DangerIndicator.transform, false);
            filledIndicator.transform.localRotation = Quaternion.Euler(0, 0, _currentShrineLevel * (360f / _shrineLevelMax));
        }

        protected override void StartShrine()
        {
            base.StartShrine();
            StartCoroutine(SpawnWaves());
        }

        private IEnumerator SpawnWaves()
        {
            int spawnCount = _currentShrineLevel * (Mathf.FloorToInt(WorldState.Difficulty() / 10f) + 1) * 5;
            float angleInterval = 360f / spawnCount;
            currentAngle = Random.Range(0, 360);
            waveDuration = 0f;
            float currentTime;

            for (int i = 0; i < spawnCount; ++i)
            {
                currentTime = 0f;
                while (currentTime < SpawnDelay)
                {
                    if (!CombatManager.IsCombatActive()) yield return null;
                    Vector2 ghoulPos = AdvancedMaths.CalculatePointOnCircle(currentAngle, SpawnRadius, transform.position);
                    EnemyBehaviour enemy = CombatManager.SpawnEnemy(EnemyType.Ghoul, ghoulPos);
                    AddEnemy(enemy);
                    waveDuration += enemy.HealthController.GetMaxHealth() * Mathf.Sqrt(enemy.Enemy.ArmourController.GetCurrentArmour() + 1);
                    currentTime += SpawnDelay;
                    yield return null;
                }

                currentAngle += angleInterval;
                if (currentAngle > 360f) currentAngle -= 360f;
            }

            waveDuration /= 10f;
            currentTime = waveDuration;
            while (currentTime > 0f)
            {
                if (!CombatManager.IsCombatActive()) yield return null;
                currentTime -= Time.deltaTime;
                UpdateCountdown(currentTime, waveDuration);
                if (EnemiesDead()) break;
                yield return null;
            }

            EndChallenge();
        }

        protected override void EndChallenge()
        {
            if (EnemiesDead())
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