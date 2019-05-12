using System.Collections;
using System.Collections.Generic;
using Extensions;
using Game.Combat.Enemies;
using Game.Combat.Misc;
using Game.Global;

using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation.Shrines
{
	public class FireShrine : ShrineBehaviour
	{
		private readonly List<FireBehaviour> _fires = new List<FireBehaviour>();
		private          float               _fireAngle;
		private          float               _fireRadiusModifier;
		private          float               _fireRingTimer;
		private          int                 _maxEnemies;

		protected override void StartChallenge()
		{
			StartCoroutine(SpawnFire());
		}

		protected override string GetInstructionText() => "Kill all enemies within the time limit";

		private void CreateFires()
		{
			int fireCount = 30;
			for (int i = 0; i < fireCount; ++i)
			{
				Vector2       firePosition = AdvancedMaths.CalculatePointOnCircle(360f / fireCount * i, 6.5f, transform.position);
				FireBehaviour fire         = FireBehaviour.Create(firePosition);
				fire.gameObject.AddComponent<FireDamageDeal>();
				_fires.Add(fire);
			}
		}


		private IEnumerator SpawnFire()
		{
			CreateFires();

			int numberOfEnemies = WorldState.ScaleValue(6);
			_maxEnemies = numberOfEnemies / 3;

			List<EnemyType> inactiveEnemies = new List<EnemyType>();
			for (int i = 0; i < numberOfEnemies; ++i)
			{
				EnemyType spawnType = NumericExtensions.RollDie(0, 2) ? EnemyType.Maelstrom : EnemyType.Shadow;
				inactiveEnemies.Add(spawnType);
			}

			CombatManager.Instance().OverrideInactiveEnemies(inactiveEnemies);
			CombatManager.Instance().OverrideMaxSize(_maxEnemies);

			float roundTime   = numberOfEnemies * 10;
			float currentTime = roundTime;

			while (currentTime > 0)
			{
				if (!CombatManager.Instance().IsCombatActive()) yield return null;
				currentTime -= Time.deltaTime;
				UpdateFireRing();
				UpdateCountdown(currentTime, roundTime);
				if (CombatManager.Instance().ClearOfEnemies())
				{
					Succeed();
					break;
				}

				yield return null;
			}

			EndChallenge();
		}

		private void UpdateFireRing()
		{
			_fireRingTimer -= Time.deltaTime;
			if (_fireRingTimer > 0f) return;
			_fireRingTimer      =  0.1f;
			_fireRadiusModifier =  Mathf.Sin(Time.timeSinceLevelLoad / 5f);
			_fireRadiusModifier += 1f;
			_fireRadiusModifier /= 2f;
			float angleDiff = 2.5f * _fireRadiusModifier + 7.5f;
			_fireAngle += angleDiff;
			if (_fireAngle > 360f) _fireAngle -= 360f;
			float   radius                    = 6.5f * _fireRadiusModifier;
			Vector2 position                  = AdvancedMaths.CalculatePointOnCircle(_fireAngle, radius, Vector2.zero);
			FireBehaviour.Create(position).SetLifeTime(1.5f);

			position = AdvancedMaths.CalculatePointOnCircle(_fireAngle + 180f, radius, Vector2.zero);
			FireBehaviour fire = FireBehaviour.Create(position);
			fire.gameObject.AddComponent<FireDamageDeal>();
			fire.SetLifeTime(1.5f);
		}

		protected override void EndChallenge()
		{
			_fires.ForEach(f => f.LetDie());
			if (!CombatManager.Instance().ClearOfEnemies()) Fail();
			base.EndChallenge();
		}
	}
}