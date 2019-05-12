using System.Collections.Generic;
using Extensions;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Global;


using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Bosses.Starfish
{
	public class StarfishGhoulSpawn : MonoBehaviour
	{
		private const    float               GhoulSpawnRate = 4f;
		private readonly List<CanTakeDamage> _enemies       = new List<CanTakeDamage>();
		private          float               _timeAlive;
		private          float               _timeToNextEnemy;

		public void UpdateGhoulSpawn(int armCount)
		{
			_timeAlive += Time.deltaTime;
			UpdateAliveGhouls();
			UpdateSpawn();
		}

		private void UpdateAliveGhouls()
		{
			for (int i = _enemies.Count - 1; i >= 0; --i)
			{
				if (_enemies[i] == null)
				{
					_enemies.RemoveAt(i);
				}
			}
		}

		private void UpdateSpawn()
		{
			int maxEnemies                     = (int) (_timeAlive / 6f);
			if (maxEnemies     > 5) maxEnemies = 5;
			if (_enemies.Count >= maxEnemies) return;
			_timeToNextEnemy -= Time.deltaTime;
			if (_timeToNextEnemy > 0f) return;
			EnemyType      typeToSpawn = WorldState.GetAllowedNightmareEnemyTypes().RandomElement();
			EnemyBehaviour enemy       = CombatManager.Instance().SpawnEnemy(typeToSpawn, AdvancedMaths.RandomDirection() * 9);
			_enemies.Add(enemy);
			if (_timeAlive > 60 && NumericExtensions.RollDie(0, 3))
			{
				LeaveFireTrail fireTrail = enemy.gameObject.AddComponent<LeaveFireTrail>();
				fireTrail.Initialise();
				fireTrail.AddIgnoreTargets(StarfishBehaviour.Instance().Sections);
			}

			_timeToNextEnemy = Random.Range(GhoulSpawnRate, GhoulSpawnRate * 2f);
		}
	}
}