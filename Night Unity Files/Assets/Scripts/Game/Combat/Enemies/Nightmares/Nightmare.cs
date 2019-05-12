using System.Collections.Generic;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Global;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
	public class Nightmare : NightmareEnemyBehaviour
	{
		private readonly List<Drone> _drones = new List<Drone>();
		private          Beam        _beam;

		public override void Initialise(Enemy enemy)
		{
			base.Initialise(enemy);
			_beam = gameObject.AddComponent<Beam>();
			_beam.Initialise(15f, 10f);
			CurrentAction = Move;
		}

		public void Start()
		{
			int NumberOfDrones = (int) (WorldState.Difficulty() / 10f) + 2;
			for (int i = 0; i < NumberOfDrones; ++i)
			{
				float angleOffset = Random.Range(0f,   360f);
				float radius      = Random.Range(0.5f, 0.75f);
				Drone drone       = Drone.Create(transform, radius, angleOffset);
				_drones.Add(drone);
			}
		}

		public override void Kill()
		{
			_drones.ForEach(d =>
			{
				if (d            == null) return;
				if (d.gameObject == null) return;
				d.Kill();
			});
			base.Kill();
		}

		protected override void UpdateRotation()
		{
			if (_beam.Active()) return;
			base.UpdateRotation();
		}
	}
}