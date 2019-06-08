using System.Collections;
using Game.Combat.Misc;
using Game.Global;
using UnityEngine;

namespace Game.Combat.Enemies.Humans
{
	public class Sniper : ArmedBehaviour
	{
		private bool  _firing;
		private float _powerShotCooldown = 15f;

		private void ResetCooldown()
		{
			_powerShotCooldown = Random.Range(10, 20);
			_firing            = false;
		}

		private void FirePowerShot()
		{
			Shot powerShot = ShotManager.Create(this);
			powerShot.Attributes().AddOnHit(() => FireBurstBehaviour.Create(powerShot.transform.position));
			powerShot.Fire();
			ResetCooldown();
			TryFire();
		}

		public override void MyUpdate()
		{
			base.MyUpdate();
			if (_firing) return;
			if (WorldState.Difficulty() < 25) return;
			_powerShotCooldown -= Time.deltaTime;
			if (_powerShotCooldown > 0) return;
			CurrentAction = null;
			_firing       = true;
			SkillAnimationController.Create(transform, "Sniper", 1f, () => StartCoroutine(StartFireShot()));
		}

		private IEnumerator StartFireShot()
		{
			yield return new WaitForSeconds(0.5f);
			if (gameObject == null) yield break;
			FirePowerShot();
		}
	}
}