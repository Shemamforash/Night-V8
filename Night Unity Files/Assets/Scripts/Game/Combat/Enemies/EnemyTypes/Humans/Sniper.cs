using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;

namespace Game.Combat.Enemies.EnemyTypes
{
    public class Sniper : Enemy
    {
        private readonly bool _mobile;
        private bool _reachedTarget;

        public Sniper(bool mobile) : base("Sniper", 5)
        {
            _mobile = mobile;
            Weapon sniperRifle = WeaponGenerator.GenerateWeapon(WeaponType.Rifle);
            Equip(sniperRifle);
            Speed = 7;
            PreferredCoverDistance = Weapon().GetAttributeValue(AttributeType.Accuracy) * 0.9f;
            AlertOthers = true;
            ArmourLevel.SetCurrentValue(4);
        }

        protected override void Alert()
        {
            base.Alert();
            CurrentAction = FindCover;
        }

        protected override void ReachTarget()
        {
            if (_mobile || !_reachedTarget)
            {
                base.ReachTarget();
            }
            _reachedTarget = true;
        }
    }
}