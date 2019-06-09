using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using UnityEngine;

namespace Game.Combat.Enemies.Humans
{
	public class Medic : ArmedBehaviour
	{
		private static GameObject _healPrefab;
		private        float      _healCooldown;
		private        bool       _healing;

		public override void Initialise(Enemy enemy)
		{
			base.Initialise(enemy);
			if (_healPrefab == null) _healPrefab = Resources.Load<GameObject>("Prefabs/Combat/Visuals/Heal Indicator");
		}

		private void ResetCooldown()
		{
			_healCooldown = Random.Range(5, 10);
		}

		public override void MyUpdate()
		{
			base.MyUpdate();
			if (_healing) return;
			UpdateHealCooldown();
		}

		private void UpdateHealCooldown()
		{
			if (_healCooldown < 0)
			{
				TryHeal();
				return;
			}

			_healCooldown -= Time.deltaTime;
		}

		private List<EnemyBehaviour> GetEnemiesNearby()
		{
			List<CanTakeDamage>  chars         = CombatManager.Instance().GetEnemiesInRange(transform.position, 1f);
			List<EnemyBehaviour> enemiesNearby = new List<EnemyBehaviour>();
			chars.ForEach(c =>
			{
				EnemyBehaviour behaviour = c as EnemyBehaviour;
				if (behaviour == null) return;
				if (behaviour is Martyr) return;
				if (behaviour.HealthController.GetNormalisedHealthValue() > 0.75f) return;
				enemiesNearby.Add(behaviour);
			});
			return enemiesNearby;
		}

		private void TryHeal()
		{
			List<EnemyBehaviour> enemiesNearby = GetEnemiesNearby();
			if (enemiesNearby.Count == 0) return;
			Heal();
		}

		private void Heal()
		{
			_healing = true;
			SkillAnimationController.Create(transform, "Medic", 1.5f, () =>
			{
				CombatManager.Instance().GetEnemiesInRange(transform.position, 1f).ForEach(e =>
				{
					EnemyBehaviour enemy = e as EnemyBehaviour;
					if (enemy == null) return;
					int healAmount = (int) (enemy.HealthController.GetMaxHealth() * 0.2f);
					enemy.HealthController.Heal(healAmount);
					GameObject healObject = Instantiate(_healPrefab);
					healObject.transform.position   = enemy.transform.position;
					healObject.transform.localScale = Vector3.one;
				});
				ResetCooldown();
				_healing = false;
			});
		}
	}
}