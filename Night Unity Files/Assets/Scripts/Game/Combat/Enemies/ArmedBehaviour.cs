using Game.Combat.Generation;
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
            CalculateMaxMinDistance();
            Debug.Log(ArmourController.GetCurrentProtection());
        }

        protected void CalculateMaxMinDistance()
        {
            MaxDistance = 5;
            MinDistance = Weapon().CalculateMinimumDistance();
        }

        protected override void OnAlert()
        {
            TryFire();
        }

        protected void TryFire()
        {
            Aim();
        }

        public override Weapon Weapon() => Enemy.EquippedWeapon;

        private Cell _coverCell;

        private void TryReload()
        {
            _coverCell = MoveBehaviour.MoveToCover();
            if (_coverCell == null)
            {
                Reload();
                return;
            }

            CurrentAction = () =>
            {
                if (CurrentCell() != _coverCell) return;
                Reload();
            };
        }

        private void Reload()
        {
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
            if (_weaponBehaviour.Empty()) TryReload();
            else CurrentAction = Fire;
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