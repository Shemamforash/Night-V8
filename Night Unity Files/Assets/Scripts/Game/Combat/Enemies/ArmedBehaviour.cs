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
        }

        protected void CalculateMaxMinDistance()
        {
            MinDistance = Weapon().CalculateMinimumDistance();
            MaxDistance = MinDistance * 2f;
            if (MaxDistance > 4f) MaxDistance = 4f;
        }

        protected override void OnAlert()
        {
            _aimTime = Random.Range(0.5f, 1f);
            TryFire();
        }

        protected void TryFire()
        {
            Aim();
        }

        public override Weapon Weapon() => Enemy.EquippedWeapon;

        private Cell _coverCell;
        private float _aimTime;

        public override void MyUpdate()
        {
            base.MyUpdate();
            if (_aimTime > 0f) _aimTime -= Time.deltaTime;
        }

        private void Reload()
        {
            float duration = Weapon().GetAttributeValue(AttributeType.ReloadSpeed) * 2f;
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
            CurrentAction = () =>
            {
                if (_aimTime > 0f) return;
                if (_weaponBehaviour.Empty()) Reload();
                else CurrentAction = Fire;
            };
        }

        private void Fire()
        {
            bool automatic = Weapon().WeaponAttributes.Automatic;
            float fireTime = 2f;
            CurrentAction = () =>
            {
                fireTime -= Time.deltaTime;
                if (fireTime <= 0)
                {
                    _weaponBehaviour.StopFiring();
                    _aimTime = Random.Range(0.5f, 1f);
                    Aim();
                    return;
                }

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