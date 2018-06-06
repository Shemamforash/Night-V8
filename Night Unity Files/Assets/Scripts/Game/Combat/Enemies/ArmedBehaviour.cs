using System.Collections.Generic;
using System.Linq;
using Game.Combat.Enemies.Humans;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Combat.Enemies
{
    public class ArmedBehaviour : UnarmedBehaviour
    {
        protected float IdealWeaponDistance;
        private const int EnemyReloadMultiplier = 4;
        protected bool CouldHitTarget;
        private bool _waitingForHeal;
        private const float MaxAimTime = 2f;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            SetHealBehaviour();
            if (Weapon() != null) IdealWeaponDistance = Weapon().CalculateIdealDistance();
        }

        public override void Update()
        {
            base.Update();
            if (!CombatManager.InCombat()) return;
            CouldHitTarget = TargetVisible() && !OutOfRange();
        }

        protected override void OnAlert()
        {
            if (NeedsRepositioning()) return;
            if (Weapon() == null)
            {
                MoveToPlayer();
                return;
            }

            Aim();
        }

        private bool OutOfRange() => DistanceToTarget() < IdealWeaponDistance * 0.5f || DistanceToTarget() > IdealWeaponDistance * 1.5f;

        private bool TargetVisible()
        {
            bool _obstructed = PathingGrid.IsLineObstructed(transform.position, GetTarget().transform.position);
            return !_obstructed;
        }

        public override void ChooseNextAction()
        {
            base.ChooseNextAction();
            FacePlayer = false;
            if (NeedsRepositioning()) return;
            if (Weapon().Empty())
            {
                Reload();
            }
            else
            {
                Aim();
            }
        }

        private bool NeedsRepositioning()
        {
            if (CouldHitTarget || Weapon() == null) return false;
            Cell targetCell = PathingGrid.FindCellToAttackPlayer(CurrentCell(), (int) (IdealWeaponDistance * 1.25f), (int) (IdealWeaponDistance * 0.75f));
            Reposition(targetCell);
            return true;
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
                Weapon().Reload();
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
            Assert.IsNotNull(Weapon());
            Assert.IsFalse(Weapon().Empty());
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

                List<Shot> shots = Weapon().Fire(this, true);
                if (shots.Any(s => s.DidHit)) CombatManager.Player().TryRetaliate(this);
                if (Weapon().Empty())
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