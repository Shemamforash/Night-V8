using Game.Combat.Enemies.Misc;
using Game.Global;
using UnityEngine;

namespace Game.Combat.Enemies.Humans
{
	public class Witch : ArmedBehaviour
	{
		private const float MaxCooldownTime = 10f;
		private       float _cooldownTime;
		private       bool  _throwing;

		private void ThrowGrenade()
		{
			_throwing     = true;
			CurrentAction = null;
			SkillAnimationController.Create(transform, "Witch", 1f, () =>
			{
				Vector2 currentPosition               = transform.position;
				Vector2 targetPosition                = TargetTransform().position;
				int     max                           = 1;
				if (WorldState.Difficulty() > 30) max = 2;
				if (WorldState.Difficulty() > 37) max = 3;
				switch (Random.Range(0, max))
				{
					case 0:
						Grenade.CreateBasic(currentPosition, targetPosition, false);
						break;
					case 1:
						Grenade.CreateIncendiary(currentPosition, targetPosition, false);
						break;
					case 2:
						Grenade.CreateDecay(currentPosition, targetPosition, false);
						break;
					case 3:
						Grenade.CreateVoid(currentPosition, targetPosition, false);
						break;
				}

				ResetCooldown();
				_throwing = false;
				TryFire();
			});
		}

		private void ResetCooldown()
		{
			float cooldownTime = MaxCooldownTime - WorldState.NormalisedDifficulty() * MaxCooldownTime / 2f;
			_cooldownTime = Random.Range(cooldownTime, cooldownTime + 5f);
		}

		public override void MyUpdate()
		{
			base.MyUpdate();
			if (_throwing) return;
			_cooldownTime -= Time.deltaTime;
			if (_cooldownTime > 0) return;
			ThrowGrenade();
		}
	}
}