﻿using System;
using Facilitating.Audio;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Characters.Player;
using Game.Combat.Enemies;
using Game.Combat.Skills;
using Game.Gear.Weapons;
using NUnit.Framework;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.Input;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Combat
{
    public abstract class CharacterCombat : MonoBehaviour
    {
        public Bleed Bleeding;
        public Burn Burn;
        public Sickness Sick;
        
        public bool InCover;
        public bool IsKnockedDown;
        public bool IsDead;

        protected readonly RecoilManager RecoilManager = new RecoilManager();

        public UIHealthBarController HealthController;
        public UIArmourController ArmourController;
        
        public readonly Number Position = new Number(0f, float.MinValue);
        private float _distanceTravelled;
        protected Action<float> MoveForwardAction;
        protected Action<float> MoveBackwardAction;
    
        public float Speed;
        private const int SprintModifier = 2;
        protected bool Sprinting;
        private const float DistanceToPlay = 2f;
        public const int MeleeDistance = 5;

        private Character _character;
        
        public virtual void Awake()
        {
            ArmourController = Helper.FindChildWithName<UIArmourController>(gameObject, "Armour");
            HealthController = Helper.FindChildWithName<UIHealthBarController>(gameObject, "Health");
            Position.AddOnValueChange(a => { CombatManager.Player?.UpdatePlayerDirection(); });
            SetConditions();
        }

        protected void SetOwnedByEnemy(float speed)
        {
            Speed = speed;
            MoveForwardAction = f =>
            {
                Position.Decrement(f);
            };
            MoveBackwardAction = f =>
            {
                Position.Increment(f);
            };
        }

        private void IncreaseDistance(float distance)
        {
            _distanceTravelled += distance;
            if (_distanceTravelled < DistanceToPlay) return;
            GunFire.Step(Position.CurrentValue());
            _distanceTravelled = 0;
        }

        protected void MoveForward()
        {
            Move(1);
        }

        protected void MoveBackward()
        {
            Move(-1);
        }

        protected void Move(float direction)
        {
            if (Immobilised()) return;
            LeaveCover();
            float distanceToMove = Speed * Time.deltaTime;
            if (Sprinting) distanceToMove *= SprintModifier;
            IncreaseDistance(distanceToMove);
            if (direction > 0)
            {
                MoveForwardAction?.Invoke(distanceToMove);
            }
            else
            {
                MoveBackwardAction?.Invoke(distanceToMove);
            }
        }

        private void KnockBack(float distance)
        {
            MoveBackwardAction?.Invoke(distance);
        }
        
        public virtual void UpdateCombat()
        {
            Burn.Update();
            Sick.Update();
            Bleeding.Update();
            RecoilManager.UpdateCombat();
        }

        public abstract void Kill();
        
        protected virtual void Interrupt()
        {
        }

        protected virtual void KnockDown()
        {
            Interrupt();
            LeaveCover();
            IsKnockedDown = true;
        }

        public void Knockback(float knockbackDistance)
        {
            KnockBack(knockbackDistance);
            KnockDown();
        }
        
        //COVER
        protected virtual void TakeCover()
        {
            if (Immobilised() || InCover) return;
            InCover = true;
        }

        protected virtual void LeaveCover()
        {
            if (Immobilised() || !InCover) return;
            InCover = false;
        }

        protected virtual bool Immobilised()
        {
            return IsKnockedDown;
        }

        
        public virtual void OnHit(int damage, bool critical)
        {
            if (InCover) return;
            //todo pierce through cover?
            float armourModifier = 1 - ArmourController.CurrentArmour() / 10;
            int healthDamage = (int) (armourModifier * damage);
            int armourDamage = (int) ((1 - armourModifier) * damage);
            if(healthDamage != 0) HealthController.TakeDamage(healthDamage);
            if(armourDamage != 0) ArmourController.TakeDamage(armourDamage);
        }

        public float GetHitChance(CharacterCombat target)
        {
            float hitChance = 0f;
            if (target.InCover)
            {
                return hitChance;
            }

            float distanceToTarget = (target as DetailedEnemyCombat)?.DistanceToPlayer ?? ((DetailedEnemyCombat) this).DistanceToPlayer;
            float range = Weapon().WeaponAttributes.Range.CurrentValue();
            if (distanceToTarget <= range)
            {
                hitChance = 1;
            }
            else
            {
                hitChance = (float) Math.Pow(range / distanceToTarget, 2);
            }

            hitChance *= RecoilManager.GetAccuracyModifier();
            return hitChance;
        }

        protected virtual void TakeArmourDamage(int damage)
        {
        }

        public virtual void OnMiss()
        {
        }

        public Weapon Weapon()
        {
            return _character.Weapon;
        }
        
        //FIRING

        protected virtual Shot FireWeapon(CharacterCombat target)
        {
            if (Immobilised() || InCover || !Weapon().CanFire()) return null;
            Assert.IsNotNull(target);
            Shot shot = Weapon().Fire(target, this);
            RecoilManager.Increment(Weapon());
            return shot;
        }

        //CONDITIONS

        private void SetConditions()
        {
            Bleeding = new Bleed(this);
            Burn = new Burn(this);
            Sick = new Sickness(this);
            Burn.OnConditionNonEmpty = HealthController.StartBurning;
            Burn.OnConditionEmpty = HealthController.StopBurning;
            Bleeding.OnConditionNonEmpty = HealthController.StartBleeding;
            Bleeding.OnConditionEmpty = HealthController.StopBleeding;
        }

        public virtual void ExitCombat()
        {
            Burn.Clear();
            Bleeding.Clear();
            Sick.Clear();
        }


        public virtual void SetPlayer(Character character)
        {
            _character = character;
        }
    }
}