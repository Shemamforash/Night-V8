using System.Collections.Generic;
using Game.Combat.Enemies.Humans;
using Game.Combat.Generation;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Combat.Enemies
{
    public class ArmedBehaviour : UnarmedBehaviour
    {
        private float IdealWeaponDistance;
        private const int EnemyReloadMultiplier = 4;
        protected bool CouldHitTarget;
        private bool _waitingForHeal;
        private BaseWeaponBehaviour _weaponBehaviour;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            SetHealBehaviour();
            _weaponBehaviour = Weapon().InstantiateWeaponBehaviour(this);
            IdealWeaponDistance = Weapon().CalculateIdealDistance();
        }

        public override void Update()
        {
            base.Update();
            if (!CombatManager.InCombat()) return;
            CouldHitTarget = TargetVisible() && !OutOfRange();
        }

        protected override void OnAlert()
        {
            ChooseNextAction();
        }

        private bool OutOfRange() => DistanceToTarget() < IdealWeaponDistance * 0.5f || DistanceToTarget() > IdealWeaponDistance * 1.5f;

        private bool TargetVisible()
        {
            return !PathingGrid.IsCellHidden(CurrentCell());
        }

        public override void ChooseNextAction()
        {
            base.ChooseNextAction();
            FacePlayer = false;
            if (!CouldHitTarget)
            {
                FindCellToAttackPlayer((int) (IdealWeaponDistance * 1.25f), (int) (IdealWeaponDistance * 0.75f));
                return;
            }

            if (_weaponBehaviour.Empty())
                Reload();
            else
                Aim();
        }

        public override Weapon Weapon() => Enemy.Weapon;

        private static Medic FindMedic()
        {
            foreach (EnemyBehaviour enemy in CombatManager.EnemiesOnScreen())
            {
                Medic medic = enemy as Medic;
                if (medic == null || medic.HasTarget()) continue;
                return medic;
            }

            return null;
        }

        private void Reload()
        {
            if (MoveToCover(Reload)) return;
//                Flee();
            SetActionText("Reloading");
            float duration = Weapon().GetAttributeValue(AttributeType.ReloadSpeed) * EnemyReloadMultiplier;
            CurrentAction = () =>
            {
                duration -= Time.deltaTime;
                if (duration > 0) return;
                _weaponBehaviour.Reload();
                ChooseNextAction();
            };
        }

        private void SetHealBehaviour()
        {
            HealthController.AddOnTakeDamage(a =>
            {
                if (HealthController.GetNormalisedHealthValue() > 0.25f || _waitingForHeal) return;
                Medic m = FindMedic();
                if (m == null) return;
                MoveToCover(() => WaitForHeal(m));
            });
        }

        private void WaitForHeal(Medic medic)
        {
            CurrentAction = () =>
            {
                if (!_waitingForHeal)
                {
                    SetActionText("Waiting for Medic");
                    medic.RequestHeal(this);
                    _waitingForHeal = true;
                }

                if (medic != null) return;
                medic = FindMedic();
                if (medic == null)
                {
                    ChooseNextAction();
                }
                else
                {
                    WaitForHeal(medic);
                }
            };
        }

        private void Aim()
        {
            FacePlayer = true;
            Immobilised(true);
            Assert.IsFalse(_weaponBehaviour.Empty());
            SetActionText("Aiming");
            CurrentAction = () =>
            {
                if (!CouldHitTarget) ChooseNextAction();
                if (GetAccuracyModifier() > 0.25f) return;
                Fire();
            };
        }

        private void Fire()
        {
            bool automatic = Weapon().WeaponAttributes.Automatic;
            SetActionText("Firing");
            Immobilised(true);
            CurrentAction = () =>
            {
                if (!CouldHitTarget)
                {
                    ChooseNextAction();
                    return;
                }

                _weaponBehaviour.StartFiring(this);
                if (_weaponBehaviour.Empty())
                {
                    Reload();
                    FacePlayer = false;
                }
                else if (!automatic)
                {
                    Aim();
                }
            };
        }
    }
}