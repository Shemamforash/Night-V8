using Game.Combat.Enemies.Humans;
using Game.Combat.Generation;
using Game.Combat.Player;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Combat.Enemies
{
    public class ArmedBehaviour : UnarmedBehaviour
    {
        private float IdealWeaponDistance;
        protected bool CouldHitTarget;
        private bool _waitingForHeal;
        private BaseWeaponBehaviour _weaponBehaviour;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            SetHealBehaviour();
            if(Weapon() == null) Debug.Log(enemy.Template.EnemyType + " " + enemy.Template.HasWeapon);
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
            TryFire();
        }

        private bool OutOfRange() => DistanceToTarget() < IdealWeaponDistance * 0.5f || DistanceToTarget() > IdealWeaponDistance * 1.5f;

        private bool TargetVisible()
        {
            RaycastHit2D hit = Physics2D.Linecast(transform.position, PlayerCombat.Instance.transform.position, 1 << 8);
            return hit.collider == null;
        }

        protected void TryFire()
        {
            FacePlayer = false;

            if (!CouldHitTarget)
            {
                MoveBehaviour.GoToCell(GetTarget().CurrentCell(), IdealWeaponDistance);
                CurrentAction = TryFire;
                return;
            }

            Aim();
        }

        public override Weapon Weapon() => Enemy.EquippedWeapon;

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
            if (MoveToCover(Reload))
            {
                return;
            }
            SetActionText("Reloading");
            float duration = Weapon().GetAttributeValue(AttributeType.ReloadSpeed);
            CurrentAction = () =>
            {
                duration -= Time.deltaTime;
                if (duration > 0) return;
                _weaponBehaviour.Reload();
                TryFire();
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
                    TryFire();
                }
                else
                {
                    WaitForHeal(medic);
                }
            };
        }

        private void Aim()
        {
            if (_weaponBehaviour.Empty())
            {
                Reload();
                return;
            }

            FacePlayer = true;
            SetActionText("Aiming");
            CurrentAction = () =>
            {
                if (!CouldHitTarget) TryFire();
                Fire();
            };
        }

        private void Fire()
        {
            bool automatic = Weapon().WeaponAttributes.Automatic;
            SetActionText("Firing");
            CurrentAction = () =>
            {
                if (!CouldHitTarget)
                {
                    TryFire();
                    return;
                }

                if (!_weaponBehaviour.CanFire()) return;
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