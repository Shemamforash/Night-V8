using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Game.Gear.Weapons
{
    public class WeaponClass : GearModifier
    {
        public readonly bool CanBeManual;
        public readonly WeaponType Type;
        private readonly List<GearModifier> _subtypes = new List<GearModifier>();

        public WeaponClass(WeaponType type, bool canBeManual) : base(type.ToString())
        {
            Type = type;
            CanBeManual = canBeManual;
        }

        public void AddSubtype(GearModifier subtype)
        {
            _subtypes.Add(subtype);
        }

        public GearModifier GetSubtype()
        {
            return _subtypes[Random.Range(0, _subtypes.Count)];
        }
    }
}