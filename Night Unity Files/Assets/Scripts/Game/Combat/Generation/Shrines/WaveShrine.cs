using System.Collections;
using Extensions;
using Game.Combat.Enemies;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation.Shrines
{
	public class WaveShrine : ShrineBehaviour
	{
		private const  float      SpawnRadius = 3f;
		private const  float      SpawnDelay  = 0.25f;
		private static GameObject _filledIndicatorPrefab;
		private        int        _currentShrineLevel = 1;
		private        int        _shrineLevelMax;
		private        float      waveDuration, currentAngle;

		private void Initialise(int shrineLevel)
		{
			_shrineLevelMax        = shrineLevel;
			DangerIndicator.sprite = Resources.Load<Sprite>("Images/Danger Indicators/Evil Circle " + _shrineLevelMax);
		}

		public void Start()
		{
			Initialise(3);
		}

		private void AddIndicator()
		{
			if (_filledIndicatorPrefab == null) _filledIndicatorPrefab = Resources.Load<GameObject>("Prefabs/Combat/Filled Indicator");
			GameObject filledIndicator                                 = Instantiate(_filledIndicatorPrefab, DangerIndicator.transform, false);
			filledIndicator.transform.localRotation = Quaternion.Euler(0, 0, _currentShrineLevel * (360f / _shrineLevelMax));
		}

		protected override void StartChallenge()
		{
			StartCoroutine(SpawnWaves());
		}

		protected override string GetInstructionText() => "Defeat waves within the time limit";

		private IEnumerator SpawnWaves()
		{
			float difficultyModifier = WorldState.Difficulty() / 50f;                         //0 - 1
			difficultyModifier += 1;                                                          //1 - 2
			difficultyModifier *= 5;                                                          //5 - 10
			int   spawnCount    = _currentShrineLevel * Mathf.FloorToInt(difficultyModifier); //5 - 30
			float angleInterval = 360f                / spawnCount;
			currentAngle = Random.Range(0, 360);
			waveDuration = 0f;

			for (int i = 0; i < spawnCount; ++i)
			{
				if (!CombatManager.Instance().IsCombatActive()) yield return null;
				Vector2        ghoulPos   = AdvancedMaths.CalculatePointOnCircle(currentAngle, SpawnRadius, transform.position);
				EnemyType      enemyType  = WorldState.GetAllowedNightmareEnemyTypes().RandomElement();
				EnemyBehaviour enemy      = CombatManager.Instance().SpawnEnemy(enemyType, ghoulPos);
				int            enemyValue = enemy.Enemy.Template.Value;
				waveDuration += enemyValue;
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