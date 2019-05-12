using System.Collections;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Bosses
{
	public class OvaExplosionAttack : TimedAttackBehaviour
	{
		protected override void Attack()
		{
			StartCoroutine(DoExplosion());
		}

		private IEnumerator DoExplosion()
		{
			Paused = true;
			float radius         = PlayerCombat.Position().magnitude;
			float explosionCount = radius * 4;
			float angleInterval  = 360f   / explosionCount;
			for (int i = 0; i < explosionCount; ++i)
			{
				float   angle    = i * angleInterval;
				Vector2 position = AdvancedMaths.CalculatePointOnCircle(angle, radius, Vector2.zero);
				Explosion.CreateExplosion(position).Detonate();
				yield return new WaitForSeconds(0.25f);
			}

			Paused = false;
		}
	}
}