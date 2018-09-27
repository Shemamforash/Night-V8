using Game.Gear.Weapons;
using NUnit.Framework;
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
            Assert.IsNotNull(Weapon());
            _weaponBehaviour = Weapon().InstantiateWeaponBehaviour(this);
            MaxDistance = Weapon().CalculateIdealDistance();
            MinDistance = MaxDistance * 0.25f;
        }

        protected override void OnAlert()
        {
            TryFire();
//            MoveBehaviour.FollowTarget(GetTarget().transform, IdealWeaponDistance * 0.5f, IdealWeaponDistance * 1.5f);
        }

        protected void TryFire()
        {
            Aim();
        }

        public override Weapon Weapon() => Enemy.EquippedWeapon;

        private void Reload()
        {
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

        private void Aim()
        {
            if (_weaponBehaviour.Empty())
            {
                Reload();
                return;
            }

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