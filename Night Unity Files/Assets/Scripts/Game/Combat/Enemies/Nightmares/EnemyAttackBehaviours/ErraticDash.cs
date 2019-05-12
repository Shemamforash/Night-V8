using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
	public class ErraticDash : TimedAttackBehaviour
	{
		public void Start()
		{
			Initialise(1f, 2f);
		}

		protected override void Attack()
		{
			Vector2 randomPositionNearPlayer = AdvancedMaths.RandomPointInCircle(5f) + (Vector2) PlayerCombat.Position();
			Vector2 directionToPoint         = randomPositionNearPlayer              - (Vector2) transform.position;
			directionToPoint.Normalize();
			Enemy.MovementController.AddForce(directionToPoint * Enemy.Enemy.Template.Speed * 50);
		}
	}
}