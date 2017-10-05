using System;
using Game.Characters;
using Game.Combat.Weapons;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.CustomTypes;
using UnityEngine;

namespace Game.Gear.Weapons
{
    public class Weapon : GearItem
    {
        public readonly WeaponClass WeaponClass;
        public readonly WeaponSubClass SubClass;
        public readonly bool Automatic;
        public readonly MyInt AmmoInMagazine = new MyInt(0);
        public readonly MyInt Durability;
        private const int MaxDurability = 20;
        private bool _canEquip;
        public readonly WeaponAttributes WeaponAttributes;

        public Weapon(WeaponClass weaponClass, WeaponSubClass subClass, bool automatic, float weight, int durability) : base(weaponClass.Type.ToString(), weight, GearSubtype.Weapon)
        {
            WeaponClass = weaponClass;
            SubClass = subClass;
            Automatic = automatic;

            AmmoInMagazine.Max = subClass.Capacity;
            Durability = new MyInt(durability, 0, MaxDurability);
            Durability.OnMin(() => { _canEquip = false; });

            if (!automatic)
            {
//                Damage *= 2;
//                AmmoInMagazine.Max = (int) Mathf.Ceil(AmmoInMagazine.Max / 2f);
//                Accuracy *= 1.5f;
//                Mathf.Clamp(Accuracy, 0, 100);
//                ReloadSpeed /= 2f;
            }
            WeaponAttributes = new WeaponAttributes(this);
#if UNITY_EDITOR
            Print();
#endif
            Reload();
            SetExtendedName(Name + (Automatic ? " (A)" : ""));
        }

        private void Print()
        {
            Debug.Log(WeaponClass.Type + " " + SubClass.Name
                      + "\nAutomatic:  " + Automatic
                      + "\nDurability: " + Durability.Val
                      + "\nAmmo Left:  " + AmmoInMagazine.Val
                      + "\nDamage:     " + AttributeVal(AttributeType.Damage)
                      + "\nAccuracy:   " + AttributeVal(AttributeType.Accuracy)
                      + "\nReload:     " + AttributeVal(AttributeType.ReloadSpeed)
                      + "\nCritChance: " + AttributeVal(AttributeType.CriticalChance)
                      + "\nHandling:   " + AttributeVal(AttributeType.Handling)
                      + "\nFire Rate:  " + AttributeVal(AttributeType.FireRate)
                      + "\nCapacity:   " + AttributeVal(AttributeType.Capacity)
                      + "\nPellets:    " + AttributeVal(AttributeType.Pellets) + "\n\n");
        }

        public float AttributeVal(AttributeType attributeType)
        {
            return WeaponAttributes.Get(attributeType).CalculatedValue();
        }

        public void IncreaseDurability()
        {
            _canEquip = true;
            ++Durability.Val;
            WeaponAttributes.RecalculateAttributeValues();
        }

        public void DecreaseDurability()
        {
            --Durability.Val;
            WeaponAttributes.RecalculateAttributeValues();
        }

        public string GetWeaponType()
        {
            return WeaponClass.Type.ToString();
        }

        public bool Fire()
        {
            if (AmmoInMagazine > 0)
            {
                --AmmoInMagazine.Val;
                return true;
            }
            return false;
        }

        public void Reload()
        {
            float ammoAvailable = WorldState.Home().DecrementResource(InventoryResourceType.Ammo, (int) WeaponAttributes.Capacity.CalculatedValue());
            AmmoInMagazine.Val += (int) ammoAvailable;
        }

        public int GetRemainingAmmo()
        {
            return AmmoInMagazine.Val;
        }

        public override string GetSummary()
        {
            return Helper.Round(WeaponAttributes.DPS(), 1) + "DPS";
        }
    }
}