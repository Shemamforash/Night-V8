using System.Collections.Generic;
using Game.Characters;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Gear.Weapons
{
    public class WeaponClass : GearModifier
    {
        private readonly bool _canBeManual;
        public readonly WeaponType Type;
        private readonly List<GearModifier> _subtypes = new List<GearModifier>();

        public WeaponClass(WeaponType type, bool canBeManual) : base(type.ToString())
        {
            Type = type;
            _canBeManual = canBeManual;
        }

        public void AddSubtype(GearModifier subtype)
        {
            _subtypes.Add(subtype);
        }

        private GearModifier GetSubtype()
        {
            return _subtypes[Random.Range(0, _subtypes.Count)];
        }

        public Weapon CreateWeapon(GearModifier modifier, bool manualOnly)
        {
            Weapon w = new Weapon(Type.ToString(), 10);
            WeaponAttributes weaponAttributes = w.WeaponAttributes;
            weaponAttributes.SetClass(this);
            weaponAttributes.SetSubClass(GetSubtype());
            weaponAttributes.SetModifier(modifier);
            if (_canBeManual && (Random.Range(0, 2) == 0 || manualOnly))
            {
                weaponAttributes.AddManualModifier();
            }
            return w;
        }
    }
}