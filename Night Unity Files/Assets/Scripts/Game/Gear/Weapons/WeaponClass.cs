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
        private readonly List<WeaponSubClass> _subtypes = new List<WeaponSubClass>();

        public WeaponClass(WeaponType type, bool canBeManual)
        {
            Type = type;
            CanBeManual = canBeManual;
        }

        public void AddSubtype(WeaponSubClass subtype)
        {
            _subtypes.Add(subtype);
        }

        public WeaponSubClass GetSubtype()
        {
            return _subtypes[Random.Range(0, _subtypes.Count)];
        }
    }
}