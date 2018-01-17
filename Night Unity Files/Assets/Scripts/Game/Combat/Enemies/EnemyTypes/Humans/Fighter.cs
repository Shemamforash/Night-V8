using System.Collections.Generic;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Combat.Enemies.EnemyTypes
{
    public class Fighter : Enemy
    {
        private int _damageTaken;

        public Fighter(float position) : base("Fighter", 20, 4, position)
        {
            int damageToFindCover = 1000;
            Weapon weapon = WeaponGenerator.GenerateWeapon(new List<WeaponType>{WeaponType.Shotgun, WeaponType.SMG});
            Equip(weapon);
//            Attributes.Endurance.SetCurrentValue(4);
            PreferredCoverDistance = EquipmentController.Weapon().GetAttributeValue(AttributeType.Accuracy) * 0.9f;
            ArmourLevel.SetCurrentValue(6);
            MinimumFindCoverDistance = 5f;
            Speed = 5;
            HealthController.AddOnTakeDamage(damage =>
            {
                _damageTaken += damage;
                if (_damageTaken < damageToFindCover) return;
                CurrentAction = FindBetterRange;
                _damageTaken = 0;
            });
        }

        protected override void Alert()
        {
            base.Alert();
            TargetDistance = PreferredCoverDistance;
            CurrentAction = MoveToTargetDistance;
        }

        protected override void PrintUpdate()
        {
//            Debug.Log(TargetDistance);
        }
    }
}