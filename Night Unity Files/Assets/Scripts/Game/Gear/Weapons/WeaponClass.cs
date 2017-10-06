using System.Collections.Generic;
using Game.Gear;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace Game.Combat.Weapons
{
    public class WeaponClass
    {
        public ScaleableValue Damage, Accuracy, FireRate, Handling, ReloadSpeed, CriticalChance;
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