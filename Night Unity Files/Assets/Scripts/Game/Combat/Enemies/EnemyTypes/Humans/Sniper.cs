using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;

namespace Game.Combat.Enemies.EnemyTypes
{
    public class Sniper : Enemy
    {
        private bool _reachedTarget;

        //Check you've initialised the speed sam!
        public Sniper() : base("Sniper", 7, 5)
        {
            Weapon sniperRifle = WeaponGenerator.GenerateWeapon(WeaponType.Rifle);
            Equip(sniperRifle);
            PreferredCoverDistance = Weapon().GetAttributeValue(AttributeType.Accuracy) * 0.9f;
            AlertOthers = true;
            ArmourLevel.SetCurrentValue(4);
        }

        protected override void Alert()
        {
            base.Alert();
            CurrentAction = FindBetterRange;
        }

        protected override void ReachTarget()
        {
            if (!_reachedTarget)
            {
                base.ReachTarget();
            }
            _reachedTarget = true;
        }
    }
}