using System.Collections;
using Game.Combat.Misc;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Bosses.Starfish
{
	public class StarfishSpreadFire : MonoBehaviour
	{
		private float _spinAttackCooldown, _burstAttackCooldown, _radialAttackTimer, _radialAttackAngle, _spinningBombTimer, _radialBombTimer;
		private float _spinningBombAngle,  _radialBombAngle;
		private bool  _tier1Active,        _tier2Active, _tier3Active, _tier4Active, _attacking;

		public void UpdateSpreadFire()
		{
			UpdateSpinningBombAttack();
			UpdateRadialBombAttack();
			if (_attacking) return;
			UpdateSpinningProjectileAttack();
			UpdateBurstProjectileAttack();
			UpdateRadialProjectileAttack();
		}

		private void UpdateRadialBombAttack()
		{
			if (!_tier4Active) return;
			_radialBombTimer += Time.deltaTime;
			if (_radialBombTimer < 2f) return;
			_radialBombTimer =  0f;
			_radialBombAngle += 36;
			if (_radialBombAngle >= 360) _radialBombAngle -= 360f;
			for (int i = 0; i < 5; ++i)
			{
				float     angle     = i * 72 + _radialBombAngle;
				Vector2   position  = AdvancedMaths.CalculatePointOnCircle(angle, 4f, Vector2.zero);
				Explosion explosion = Explosion.CreateExplosion(position);
				explosion.AddIgnoreTargets(StarfishBehaviour.Instance().GetSections());
				explosion.Detonate();
			}
		}

		private void UpdateSpinningBombAttack()
		{
			if (!_tier3Active) return;
			_spinningBombTimer += Time.deltaTime;
			if (_spinningBombTimer < 0.5f) return;
			_spinningBombTimer =  0f;
			_spinningBombAngle += 6f;
			if (_spinningBombAngle >= 360) _spinningBombAngle -= 360f;
			Vector2 position                                  = AdvancedMaths.CalculatePointOnCircle(_spinningBombAngle, 7f, Vector2.zero);
			Explosion.CreateExplosion(position).Detonate();

			position = AdvancedMaths.CalculatePointOnCircle(_spinningBombAngle + 180f, 7f, Vector2.zero);
			Explosion.CreateExplosion(position).Detonate();
		}

		private void UpdateSpinningProjectileAttack()
		{
			if (_spinAttackCooldown < 0f) return;
			_spinAttackCooldown -= Time.deltaTime;
			if (_spinAttackCooldown > 0f) return;
			StartCoroutine(DoSpinAttack());
		}

		private void UpdateRadialProjectileAttack()
		{
			if (!_tier2Active) return;
			_radialAttackTimer -= Time.deltaTime;
			if (_radialAttackTimer > 0f) return;
			_radialAttackTimer = 1f;
			int   count         = 15;
			float angleInterval = 360f / count;
			for (int j = 0; j < count; ++j)
			{
				float   angle     = j * angleInterval + _radialAttackAngle;
				float   x         = Mathf.Cos(angle * Mathf.Deg2Rad);
				float   y         = Mathf.Sin(angle * Mathf.Deg2Rad);
				Vector3 direction = new Vector2(x, y);
				MaelstromShotBehaviour.Create(direction, transform.position + direction, 2f, true, false);
			}

			StarfishBehaviour.Instance().FlashGlow();
			StarfishBehaviour.Instance().PlayAudio(0f, 0.4f);
			_radialAttackAngle += 5f;
		}

		private void UpdateBurstProjectileAttack()
		{
			if (!_tier1Active) return;
			if (_burstAttackCooldown < 0f) return;
			_burstAttackCooldown -= Time.deltaTime;
			if (_burstAttackCooldown > 0f) return;
			StartCoroutine(DoBurstAttack());
		}

		private IEnumerator DoBurstAttack()
		{
			_attacking = true;
			int   count         = 20;
			float angleInterval = 360f / count;
			float startAngle    = 0f;

			for (int i = 0; i < 4; ++i)
			{
				for (int j = 0; j < count; ++j)
				{
					float   angle     = j * angleInterval + startAngle;
					float   x         = Mathf.Cos(angle * Mathf.Deg2Rad);
					float   y         = Mathf.Sin(angle * Mathf.Deg2Rad);
					Vector3 direction = new Vector2(x, y);
					MaelstromShotBehaviour.Create(direction, transform.position + direction, 1f, true, false);
				}

				StarfishBehaviour.Instance().FlashGlow();
				StarfishBehaviour.Instance().PlayAudio(0f, 0.2f);
				startAngle = startAngle == 0 ? angleInterval / 2f : 0;
				yield return new WaitForSeconds(0.5f);
			}

			_burstAttackCooldown = Random.Range(5f, 10f);
			_attacking           = false;
		}

		private IEnumerator DoSpinAttack()
		{
			_attacking = true;
			int   count         = 40;
			float angleInterval = 180f / count;
			while (count > 0f)
			{
				float angle  = angleInterval * count;
				float angleB = angle + 180;
				--count;
				float   x    = Mathf.Cos(angle * Mathf.Deg2Rad);
				float   y    = Mathf.Sin(angle * Mathf.Deg2Rad);
				Vector3 dirA = new Vector2(x, y);
				x = Mathf.Cos(angleB * Mathf.Deg2Rad);
				y = Mathf.Sin(angleB * Mathf.Deg2Rad);
				Vector3 dirB = new Vector2(x, y);
				MaelstromShotBehaviour.Create(dirA, transform.position + dirA, 1f, true, false);
				MaelstromShotBehaviour.Create(dirB, transform.position + dirB, 1f, true, false);
				StarfishBehaviour.Instance().FlashGlow();
				StarfishBehaviour.Instance().PlayAudio(0.3f, 0, 500);
				yield return new WaitForSeconds(0.25f);
			}

			_spinAttackCooldown = Random.Range(7.5f, 15f);
			_attacking          = false;
		}

		public void StartTier1() => _tier1Active = true;

		public void StartTier2() => _tier2Active = true;

		public void StartTier3() => _tier3Active = true;

		public void StartTier4() => _tier4Active = true;
	}
}