using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Game.Gear.Weapons
{
    public class WeaponClass
    {
        public int Damage, Accuracy, Handling, CriticalChance;
        public float ReloadSpeed, FireRate;
        public readonly bool CanBeManual;
        public readonly WeaponType Type;
        private readonly List<WeaponModifier> _subtypes = new List<WeaponModifier>();

        public WeaponClass(WeaponType type, bool canBeManual)
        {
            Type = type;
            CanBeManual = canBeManual;
        }

        public void AddSubtype(WeaponModifier subtype)
        {
            _subtypes.Add(subtype);
        }

        public WeaponModifier GetSubtype()
        {
            return _subtypes[Random.Range(0, _subtypes.Count)];
        }
    }
}