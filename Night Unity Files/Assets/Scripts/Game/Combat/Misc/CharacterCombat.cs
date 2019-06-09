using Game.Combat.Enemies;
using Game.Combat.Generation;
using Game.Gear.Weapons;
using Extensions;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Combat.Misc
{
	[RequireComponent(typeof(MovementController))]
	public abstract class CharacterCombat : CanTakeDamage
	{
		private const    float                 ExplosionForceModifier = 200;
		private readonly Number                Recoil                 = new Number(0, 0, 1f);
		private          float                 _distanceToTarget      = -1;
		private          CanTakeDamage         _target;
		private          float                 _timeToRecoilRecovery;
		public           MovementController    MovementController;
		protected        SpriteRenderer        Sprite;
		public           WeaponAudioController WeaponAudio;
		public WeaponBehaviour   WeaponBehaviour;


		protected float DistanceToTarget()
		{
			if (_distanceToTarget == -1) _distanceToTarget = Vector2.Distance(transform.position, TargetPosition());
			return _distanceToTarget;
		}

		public Vector3 TargetPosition() => TargetTransform().position;

		protected Transform TargetTransform() => GetTarget().transform;

		public Vector3 Direction() => transform.up;

		public Cell CurrentCell() => WorldGrid.WorldToCellPosition(transform.position);

		public override void Kill()
		{
			base.Kill();
			WeaponAudio.Destroy();
		}

		public virtual void ApplyShotEffects(Shot s)
		{
		}

		public abstract Weapon Weapon();

		public override void TakeShotDamage(Shot shot)
		{
			base.TakeShotDamage(shot);
			MovementController.KnockBack(shot.Direction(), shot.Attributes().GetKnockBackForce());
		}

		public override void TakeExplosionDamage(int damage, Vector2 origin, float radius)
		{
			base.TakeExplosionDamage(damage, origin, radius);
			Vector2 direction = (Vector2) transform.position - origin;
			direction.Normalize();
			float force = ExplosionForceModifier * radius;
			MovementController.KnockBack(direction, force);
		}

		protected override void Awake()
		{
			base.Awake();
			Sprite             = GetComponent<SpriteRenderer>();
			MovementController = GetComponent<MovementController>();
			WeaponAudio        = gameObject.FindChildWithName<WeaponAudioController>("Weapon Audio");
		}

		public override void MyUpdate()
		{
			base.MyUpdate();
			if (GetTarget() != null) _distanceToTarget = Vector2.Distance(transform.position, TargetPosition());
			if (_timeToRecoilRecovery > 0)
			{
				_timeToRecoilRecovery -= Time.deltaTime;
				return;
			}

			Recoil.CurrentValue -= Time.deltaTime;
		}

		public void IncreaseRecoil()
		{
			float recoil = Weapon().WeaponAttributes.Recoil() / 100f;
			Recoil.CurrentValue   += recoil;
			_timeToRecoilRecovery =  0.5f;
		}

		public float GetRecoilModifier()
		{
			float recoil                                 = Recoil.CurrentValue;
			if (WeaponBehaviour.InvertedAccuracy) recoil = 1 - recoil;
			return recoil;
		}

		public void SetTarget(CanTakeDamage target)
		{
			_target = target;
		}

		public virtual CanTakeDamage GetTarget() => _target;
	}
}