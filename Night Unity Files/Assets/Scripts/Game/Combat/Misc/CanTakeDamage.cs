using Game.Combat.Generation;
using Game.Combat.Player;
using Game.Gear.Armour;
using Game.Global;
using UnityEngine;

namespace Game.Combat.Misc
{
	public abstract class CanTakeDamage : MonoBehaviour
	{
		private const   float             VoidDurationMax  = 5f;
		private const   int               VoidTargetStacks = 3;
		public readonly HealthController  HealthController = new HealthController();
		protected       BloodSpatter      _bloodSpatter;
		private         float             _timeSinceLastBurn;
		private         float             _voidDuration;
		private         bool              _voided;
		public          ArmourController  ArmourController  = new ArmourController();
		protected       float             BurnDamagePercent = 0.05f;
		public          bool              IsPlayer;
		protected       DamageSpriteFlash SpriteFlash;
		protected       int               VoidStacks;

		private void TakeArmourDamage(int damage)
		{
			if (!ArmourController.CanAbsorbDamage) return;
			ArmourController.TakeDamage(damage);
		}

		protected virtual void Awake()
		{
			SpriteFlash   = GetComponent<DamageSpriteFlash>();
			_bloodSpatter = GetComponent<BloodSpatter>();
			if (!IsPlayer) CombatManager.Instance().AddEnemy(this);
		}

		public virtual void Kill()
		{
			CombatManager.Instance()?.RemoveEnemy(this);
			Destroy(gameObject);
		}

		public virtual void MyUpdate()
		{
			UpdateConditions();
			ArmourController.Update();
		}

		public abstract string GetDisplayName();

		public virtual int Burn()
		{
			if (_timeSinceLastBurn < 0.5f) return -1;
			_timeSinceLastBurn = 0f;
			SpriteFlash.FlashSprite();
			int burnDamage                 = Mathf.FloorToInt(HealthController.GetMaxHealth() * BurnDamagePercent);
			if (burnDamage < 1) burnDamage = 1;
			HealthController.TakeDamage(burnDamage);
			return burnDamage;
		}

		public void Shatter()
		{
			TakeArmourDamage(10000);
		}

		public virtual bool Void()
		{
			bool tookDamage = false;
			++VoidStacks;
			if (VoidStacks >= VoidTargetStacks)
			{
				_voided = true;
				SpriteFlash.FlashSprite();
				float damage = HealthController.GetMaxHealth() / 10f;
				HealthController.TakeDamage(damage);
				VoidStacks = 0;
				tookDamage = true;
			}

			_voidDuration = 0;
			return tookDamage;
		}

		public float GetVoid() => (float) VoidStacks / VoidTargetStacks;

		public bool WasJustVoided()
		{
			if (!_voided) return false;
			_voided = false;
			return true;
		}

		private void UpdateBurn()
		{
			_timeSinceLastBurn += Time.deltaTime;
		}

		private void UpdateVoid()
		{
			if (VoidStacks == 0) return;
			if (_voidDuration > VoidDurationMax)
			{
				_voidDuration = 0;
				--VoidStacks;
				return;
			}

			_voidDuration += Time.deltaTime;
		}

		private void UpdateConditions()
		{
			UpdateBurn();
			UpdateVoid();
		}

		public virtual void TakeShotDamage(Shot shot)
		{
			if (SceneChanger.ChangingScene()) return;
			int damageDealt = shot.Attributes().DamageDealt();
			TakeDamage(damageDealt, shot.Direction());
			if (IsPlayer) return;
			if (!shot.Attributes().ShouldAddAdrenaline()) return;
			PlayerCombat.Instance.UpdateAdrenaline(damageDealt);
		}

		protected virtual void TakeDamage(int damage, Vector2 direction)
		{
			if (SceneChanger.ChangingScene()) return;
			if (!IsPlayer && MarkController.InMarkArea(transform.position)) damage *= 2;
			SpriteFlash.FlashSprite();
			if (!ArmourController.CanAbsorbDamage)
			{
				HealthController.TakeDamage(damage);
				if (_bloodSpatter                       != null) _bloodSpatter.Spray(direction, damage);
				if (HealthController.GetCurrentHealth() != 0) return;
				LeafBehaviour.CreateLeaves(direction, transform.position);
			}
			else
			{
				ArmourController.TakeDamage(damage);
			}
		}

		public virtual void TakeRawDamage(int damage, Vector2 direction)
		{
			if (SceneChanger.ChangingScene()) return;
			TakeDamage(damage, direction);
		}

		public virtual void TakeExplosionDamage(int damage, Vector2 origin, float radius)
		{
			if (SceneChanger.ChangingScene()) return;
			Vector2 direction = (origin - (Vector2) transform.position).normalized;
			TakeDamage(damage, direction);
		}
	}
}