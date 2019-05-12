using Game.Combat.Enemies.Nightmares;
using Game.Combat.Player;
using UnityEngine;

namespace Game.Combat.Enemies.Animals
{
	public class Curio : AnimalBehaviour
	{
		public override void Initialise(Enemy enemy)
		{
			base.Initialise(enemy);
			CurrentAction = WaitForPlayer;
		}

		private void WaitForPlayer()
		{
			if (Vector2.Distance(transform.position, PlayerCombat.Position()) > 2) return;
			Move();
			CurrentAction = WaitForPlayer;
		}
	}
}