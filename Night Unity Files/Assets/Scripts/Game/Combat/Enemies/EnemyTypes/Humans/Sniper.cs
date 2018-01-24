using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;

namespace Game.Combat.Enemies.EnemyTypes
{
    public class Sniper : Enemy
    {
        private bool _reachedTarget;

        //Check you've initialised the speed sam!
        public Sniper(float position) : base("Sniper", 7, 5, position)
        {
            Weapon sniperRifle = WeaponGenerator.GenerateWeapon(WeaponType.Rifle);
            Equip(sniperRifle);
            ArmourLevel.SetCurrentValue(4);
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