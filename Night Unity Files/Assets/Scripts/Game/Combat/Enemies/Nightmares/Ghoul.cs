using Game.Combat.Misc;
using Game.Combat.Player;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
	public class Ghoul : NightmareEnemyBehaviour
	{
		private readonly float _distanceToTouch = 0.5f;

		public override void Initialise(Enemy enemy)
		{
			base.Initialise(enemy);
			CurrentAction = SeekPlayer;
		}

		private void SeekPlayer()
		{
			Vector2 direction = PlayerCombat.Position() - transform.position;
			MovementController.Move(direction.normalized);
		}

		public override void MyUpdate()
		{
			base.MyUpdate();
			if (DistanceToTarget() > _distanceToTouch) return;
			SickenBehaviour.Create(TargetPosition(), GetTarget());
			LeafBehaviour.CreateLeaves(transform.position);
			Kill();
		}
	}
}