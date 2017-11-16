using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.CooldownSystem;

namespace Game.Combat.Enemies.EnemyTypes
{
    public class Sniper : Enemy
    {
        private float _targetDistance = -1;
        private readonly Cooldown _aimCooldown;
        private const float AimTime = 2f;
        private bool _moving;
        private bool _mobile;

        public Sniper(bool mobile) : base("Sniper", 500)
        {
            _mobile = mobile;
            Weapon sniperRifle = WeaponGenerator.GenerateWeapon(WeaponType.Rifle);
            Equip(sniperRifle);
            _aimCooldown = new Cooldown(CombatManager.CombatCooldowns, AimTime);
            _aimCooldown.SetStartAction(() => SetActionText("Aiming"));
            _aimCooldown.SetEndAction(() => FireWeapon(CombatManager.Player()));
            ReloadingCooldown.SetStartAction(() => SetActionText("Reloading"));
            BaseAttributes.Endurance.SetCurrentValue(7);
            SetActionText("Wandering");
            CoverCooldown.SetStartAction(() => SetActionText("Taking Cover"));
        }

        public override void Alert()
        {
            base.Alert();
            _targetDistance = Weapon().GetAttributeValue(AttributeType.Accuracy) * 0.9f;
            _moving = true;
        }

        private void CheckCloseToPlayer()
        {
            if (Distance.GetCurrentValue() >= 25 || _moving) return;
            _targetDistance = Weapon().GetAttributeValue(AttributeType.Accuracy) * 0.9f;
            _moving = true;
        }

        private void ReachTarget()
        {
            Distance.SetCurrentValue(_targetDistance);
            TakeCover();
            _targetDistance = -1;
            _moving = false;
        }

        private void MoveTowardsTargetDistance()
        {
            if (!_moving) return;
            float currentDistance = Distance.GetCurrentValue();
            if (currentDistance > _targetDistance)
            {
                MoveForward();
                SetActionText("Approaching");
                float newDistance = Distance.GetCurrentValue();
                if (!(newDistance <= _targetDistance)) return;
                ReachTarget();
            }
            else
            {
                MoveBackward();
                SetActionText("Retreating");
                float newDistance = Distance.GetCurrentValue();
                if (!(newDistance >= _targetDistance)) return;
                ReachTarget();
            }
        }

        private void AimAndFire()
        {
            if (_moving || Immobilised()) return;
            if (Weapon().Empty())
            {
                TryReload();
                return;
            }
            if (_aimCooldown.Running()) return;
            _aimCooldown.Start();
        }

        public override void UpdateBehaviour()
        {
            base.UpdateBehaviour();
            if (!IsAlerted()) return;
            if(_mobile) CheckCloseToPlayer();
            MoveTowardsTargetDistance();
            AimAndFire();
        }
    }
}