﻿using Game.Combat.Enemies;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Combat.Misc
{
    [RequireComponent(typeof(MovementController))]
    public abstract class CharacterCombat : CanTakeDamage
    {
        public WeaponAudioController WeaponAudio;
        private const float RecoilRecoveryRate = 0.5f;
        private readonly Number Recoil = new Number(0, 0, 1f);
        private float _distanceToTarget = -1;
        protected SpriteRenderer Sprite;
        private CanTakeDamage _target;
        public MovementController MovementController;
        private float _timeToRecoilRecovery;
        private const float ExplosionForceModifier = 10;

        protected float DistanceToTarget()
        {
            if (_distanceToTarget == -1) _distanceToTarget = Vector2.Distance(transform.position, TargetPosition());
            return _distanceToTarget;
        }

        public Vector3 TargetPosition()
        {
            return TargetTransform().position;
        }

        protected Transform TargetTransform()
        {
            return GetTarget().transform;
        }

        public Vector3 Direction() => transform.up;

        public Cell CurrentCell() => PathingGrid.WorldToCellPosition(transform.position);

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
            float distance = direction.magnitude;
            direction.Normalize();
            distance = radius - distance;
            float force = damage * distance * ExplosionForceModifier;
            MovementController.KnockBack(direction, force);
        }

        protected override void Awake()
        {
            base.Awake();
            Sprite = GetComponent<SpriteRenderer>();
            MovementController = GetComponent<MovementController>();
            WeaponAudio = gameObject.FindChildWithName<WeaponAudioController>("Weapon Audio");
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

            Recoil.Decrement(RecoilRecoveryRate * Time.deltaTime);
        }

        public void IncreaseRecoil()
        {
            float recoilLoss = Weapon().GetAttributeValue(AttributeType.Handling);
            recoilLoss = 100f - recoilLoss;
            recoilLoss /= 100f;
            Recoil.Increment(recoilLoss);
            _timeToRecoilRecovery = 0.5f;
        }

        public virtual float GetRecoilModifier() => Recoil.CurrentValue();

        public void SetTarget(CanTakeDamage target)
        {
            _target = target;
        }

        public virtual CanTakeDamage GetTarget()
        {
            return _target;
        }
    }
}