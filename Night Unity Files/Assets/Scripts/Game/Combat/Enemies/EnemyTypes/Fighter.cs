using System.Collections.Generic;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;

namespace Game.Combat.Enemies.EnemyTypes
{
    public class Fighter : Enemy
    {
        private readonly int _damageToFindCover;
        private int _damageTaken;

        public Fighter() : base("Fighter", 2000)
        {
            _damageToFindCover = 1000;
            Weapon weapon = WeaponGenerator.GenerateWeapon(new List<WeaponType>{WeaponType.Shotgun, WeaponType.SMG});
            Equip(weapon);
            BaseAttributes.Endurance.SetCurrentValue(4);
            PreferredCoverDistance = EquipmentController.Weapon().GetAttributeValue(AttributeType.Accuracy) * 0.9f;
            ArmourLevel.SetCurrentValue(6);
            MinimumFindCoverDistance = 5f;
            HealthController.AddOnTakeDamage(damage =>
            {
                _damageTaken += damage;
                if (_damageTaken < _damageToFindCover) return;
                CurrentAction = FindCover;
                _damageTaken = 0;
            });
        }

        protected override void Alert()
        {
            base.Alert();
            TargetDistance = PreferredCoverDistance;
            CurrentAction = MoveToTargetDistance;
        }
    }
}