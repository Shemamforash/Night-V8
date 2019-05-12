using System.Collections.Generic;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Misc;
using Game.Global;

namespace Game.Combat.Enemies.Nightmares
{
	public class Revenant : NightmareEnemyBehaviour
	{
		private LeaveFireTrail _fireTrail;
		private Split          _split;

		public override void Initialise(Enemy enemy)
		{
			base.Initialise(enemy);
			gameObject.AddComponent<Orbit>().Initialise(GetTarget().transform, v => MovementController.Move(v), 1, 0.5f, 4f);

			_split = gameObject.AddComponent<Split>();
			_split.Initialise(3, 200, EnemyType.Revenant, 1000, -1, true);

			if (WorldState.Difficulty() < 25) return;
			_fireTrail = gameObject.AddComponent<LeaveFireTrail>();
			_fireTrail.Initialise();
		}

		public override void Kill()
		{
			base.Kill();
			int newHealth = (int) (HealthController.GetMaxHealth() / 3f);
			if (newHealth < 3) return;
			_split.LastSplitEnemies().ForEach(e =>
			{
				Revenant r = e as Revenant;
				if (r == null) return;
				r.HealthController.SetInitialHealth(newHealth, r);
			});
			Explosion explosion = Explosion.CreateExplosion(transform.position, 0.5f);
			explosion.AddIgnoreTargets(new List<CanTakeDamage>(_split.LastSplitEnemies()));
			explosion.InstantDetonate();
		}
	}
}