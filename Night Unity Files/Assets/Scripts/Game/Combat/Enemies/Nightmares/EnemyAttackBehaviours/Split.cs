using System;
using System.Collections.Generic;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
	public class Split : DamageThresholdAttackBehaviour
	{
		private readonly List<EnemyBehaviour>   _enemies = new List<EnemyBehaviour>();
		private          EnemyBehaviour         _enemy;
		private          EnemyType              _enemyType;
		private          int                    _generation;
		private          Action<EnemyBehaviour> _onSplit;
		private          float                  _spawnForce = 10;
		private          int                    _splitCountMin, _splitCountMax;

		protected override void Attack()
		{
			_enemies.Clear();
			int splitCount    = Random.Range(_splitCountMin, _splitCountMax + 1);
			int newGeneration = _generation + 1;
			if (newGeneration == 3) return;
			for (int i = 0; i < splitCount; ++i)
			{
				EnemyBehaviour enemy     = _enemyType == EnemyType.Decoy ? Decoy.Create(GetComponent<EnemyBehaviour>()) : EnemyTemplate.Create(_enemyType);
				Vector2        randomDir = AdvancedMaths.RandomVectorWithinRange(Vector3.zero, 1).normalized;
				enemy.gameObject.transform.position = transform.position;
				enemy.MovementController.AddForce(randomDir * _spawnForce);
				_enemies.Add(enemy);
				Split split                          = enemy.GetComponent<Split>();
				if (split != null) split._generation = newGeneration;
			}

			if (_onSplit == null) return;
			_enemies.ForEach(e => _onSplit(e));
		}

		public void Initialise(int splitCountMin, float spawnForce, EnemyType enemyType, int healthThreshold, int splitCountMax = -1, bool activateOnDeath = false)
		{
			Initialise(healthThreshold, activateOnDeath);
			_splitCountMin = splitCountMin;
			_splitCountMax = splitCountMax == -1 ? _splitCountMin : splitCountMax;
			_spawnForce    = spawnForce;
			_enemyType     = enemyType;
		}

		public void SetOnSplit(Action<EnemyBehaviour> onSplit) => _onSplit = onSplit;

		public List<EnemyBehaviour> LastSplitEnemies() => _enemies;
	}
}