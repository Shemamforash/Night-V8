using Game.Combat.Enemies.Humans;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Combat.Enemies
{
    public class ArmedBehaviour : UnarmedBehaviour
    {
        private float IdealWeaponDistance;
        private bool _waitingForHeal;
        private BaseWeaponBehaviour _weaponBehaviour;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            SetHealBehaviour();
            if (Weapon() == null) Debug.Log(enemy.Template.EnemyType + " " + enemy.Template.HasWeapon);
            _weaponBehaviour = Weapon().InstantiateWeaponBehaviour(this);
            IdealWeaponDistance = Weapon().CalculateIdealDistance();
        }

        protected override void OnAlert()
        {
            TryFire();
            MoveBehaviour.FollowTarget(GetTarget().transform, IdealWeaponDistance * 0.5f, IdealWeaponDistance * 1.5f);
        }

        protected void TryFire()
        {
            FacePlayer = false;
            Aim();
        }

        public override Weapon Weapon() => Enemy.EquippedWeapon;

        private static Medic FindMedic()
        {
            foreach (ITakeDamageInterface o in CombatManager.EnemiesOnScreen())
            {
                Medic medic = o as Medic;
                if (medic == null || medic.HasTarget()) continue;
                return medic;
            }

            return null;
        }

        private void Reload()
        {
            FacePlayer = false;
            if (MoveToCover(Reload))
            {
                return;
            }

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
            FacePlayer = false;
            CurrentAction = () =>
            {
                if (!_waitingForHeal)
                {
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
            CurrentAction = Fire;
        }

        private void Fire()
        {
            bool automatic = Weapon().WeaponAttributes.Automatic;
            CurrentAction = () =>
            {
                if (!_weaponBehaviour.CanFire())
                {
                    if (!_weaponBehaviour.Empty() && automatic) return;
                    _weaponBehaviour.StopFiring();
                    Aim();
                    return;
                }

                
                _weaponBehaviour.StartFiring();
            };
        }
    }
}