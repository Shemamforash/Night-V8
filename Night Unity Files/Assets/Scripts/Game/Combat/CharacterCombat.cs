using System;
using System.Collections.Generic;
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

//        public bool IsKnockedDown;
//        public bool IsDead;

        public readonly RecoilManager RecoilManager = new RecoilManager();

        public UIHealthBarController HealthController;
        public UIArmourController ArmourController;

//        private float _distanceTravelled;

        public float Speed;
        public const int SprintModifier = 2;

        public bool Sprinting;
//        public const int MeleeDistance = 5;

        private Character _character;

        public CombatCharacterController CharacterController;

        public virtual void Awake()
        {
            ArmourController = Helper.FindChildWithName<UIArmourController>(gameObject, "Armour");
            HealthController = Helper.FindChildWithName<UIHealthBarController>(gameObject, "Health");
            SetConditions();
        }

        protected void SetOwnedByEnemy(float speed)
        {
            Speed = speed;
        }

//        private void KnockBack(float distance)
//        {
//            MoveBackwardAction?.Invoke(distance);
//        }

        public virtual void Update()
        {
            if (MeleeController.InMelee) return;
            Burn.Update();
            Sick.Update();
            Bleeding.Update();
            RecoilManager.UpdateCombat();
        }

        public virtual void Kill()
        {
            Destroy(CharacterController.gameObject);
        }

        protected virtual void Interrupt()
        {
        }

        protected virtual void KnockDown()
        {
//            Interrupt();
//            IsKnockedDown = true;
        }

        public void Knockback(float knockbackDistance)
        {
//            KnockBack(knockbackDistance);
//            KnockDown();
        }

        public virtual bool Immobilised()
        {
//            return IsKnockedDown;
            return false;
        }

        public virtual void OnMiss()
        {
        }

        public Weapon Weapon()
        {
            return _character.Weapon;
        }

        //FIRING

        //CONDITIONS

        public float DistanceToTarget()
        {
            return Vector2.Distance(CharacterController.Position(), GetTarget().CharacterController.Position());
        }

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

        public abstract CharacterCombat GetTarget();
    }
}