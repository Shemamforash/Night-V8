﻿namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
	public abstract class DamageThresholdAttackBehaviour : BasicAttackBehaviour
	{
		private float _healthLost;
		public  int   _healthThreshold;

		public void Initialise(int healthThreshold, bool activateOnDeath = false)
		{
			_healthThreshold = healthThreshold;
			Enemy.HealthController.AddOnTakeDamage(d =>
			{
				_healthLost += d;
				bool enoughHealthLost        = _healthLost >= _healthThreshold;
				bool isDeadAndShouldActivate = Enemy.HealthController.GetCurrentHealth() <= 0 && activateOnDeath;
				if (!enoughHealthLost && !isDeadAndShouldActivate) return;
				Attack();
				_healthLost = 0f;
			});
		}
	}
}