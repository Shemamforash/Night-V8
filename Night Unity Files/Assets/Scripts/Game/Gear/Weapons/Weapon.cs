using System;
using Game.Characters;
using Game.Combat.Weapons;
using Game.Gear.UI;
using Game.World;
using Game.World.WorldEvents;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace Game.Gear.Weapons
{
    public class Weapon : GearItem
    {
        public readonly WeaponClass WeaponClass;
        public readonly WeaponModifier SubClass, SecondaryModifier;
        public readonly bool Automatic;
        public readonly MyValue AmmoInMagazine = new MyValue(0);
        public readonly MyValue Durability;
        private const int MaxDurability = 20;
        private bool _canEquip;
        public readonly WeaponAttributes WeaponAttributes;
        public readonly int Capacity, Pellets;

        public Weapon(WeaponClass weaponClass, WeaponModifier subClass, WeaponModifier secondaryModifier, bool automatic, float weight, int durability) : base(weaponClass.Type.ToString(), weight, GearSubtype.Weapon)
        {
            WeaponClass = weaponClass;
            SubClass = subClass;
            SecondaryModifier = secondaryModifier;
            Automatic = automatic;

            Durability = new MyValue(durability, 0, MaxDurability);
            Durability.OnMin(() => { _canEquip = false; });
            Capacity = (int) Math.Ceiling((double) subClass.Capacity * secondaryModifier.CapacityModifier);
            Pellets = (int) Math.Ceiling((double) (subClass.Pellets * secondaryModifier.Pellets));
            
            if (!automatic)
            {
//                Damage *= 2;
//                AmmoInMagazine.Max = (int) Mathf.Ceil(AmmoInMagazine.Max / 2f);
//                Accuracy *= 1.5f;
//                Mathf.Clamp(Accuracy, 0, 100);
//                ReloadSpeed /= 2f;
            }
            WeaponAttributes = new WeaponAttributes(this);
            AmmoInMagazine.Max = Capacity;
#if UNITY_EDITOR
//            Print();
#endif
            Reload();
            UpdateDurability();
            WorldEventManager.GenerateEvent(new WeaponFindEvent(Name));
        }

        private void Print()
        {
            Debug.Log(WeaponClass.Type + " " + SubClass.Name
                      + "\nAutomatic:  " + Automatic
                      + "\nDurability: " + Durability.GetCurrentValue()
                      + "\nAmmo Left:  " + AmmoInMagazine.GetCurrentValue()
                      + "\nCapacity:   " + Capacity
                      + "\nPellets:    " + Pellets
                      + "\nDamage:     " + GetAttributeValue(AttributeType.Damage)
                      + "\nAccuracy:   " + GetAttributeValue(AttributeType.Accuracy)
                      + "\nFire Rate:  " + GetAttributeValue(AttributeType.FireRate)
                      + "\nHandling:   " + GetAttributeValue(AttributeType.Handling)
                      + "\nReload:     " + GetAttributeValue(AttributeType.ReloadSpeed)
                      + "\nCritChance: " + GetAttributeValue(AttributeType.CriticalChance) + "\n\n");
        }

        public float GetAttributeValue(AttributeType attributeType)
        {
            return WeaponAttributes.Get(attributeType).GetCalculatedValue();
        }

        public void IncreaseDurability()
        {
            _canEquip = true;
            Durability.SetCurrentValue(Durability.GetCurrentValue() + 1);
            UpdateDurability();
        }

        private void UpdateDurability()
        {
            string quality = "Perfected";
            if (Durability < 4)
            {
                quality = "Flawed";
            }
            else if (Durability < 8)
            {
                quality = "Worn";
            }
            else if (Durability < 12)
            {
                quality = "Fresh";
            }
            else if (Durability < 16)
            {
                quality = "Faultless";
            }
            Name = SecondaryModifier.Name + " " + SubClass.Name + " -- (" + quality + ")";
            WeaponAttributes.RecalculateAttributeValues();

        }

        public void DecreaseDurability()
        {
            Durability.SetCurrentValue(Durability.GetCurrentValue() - 1);
            UpdateDurability();
        }

        public string GetWeaponType()
        {
            return WeaponClass.Type.ToString();
        }

        public bool Fire()
        {
            if (AmmoInMagazine.GetCurrentValue() <= 0) return false;
            AmmoInMagazine.SetCurrentValue(AmmoInMagazine.GetCurrentValue() - 1);
            return true;
        }

        public void Reload()
        {
            Inventory inventory = WorldState.HomeInventory();
            if (inventory == null) return;
            float ammoAvailable = inventory.DecrementResource(InventoryResourceType.Ammo, Capacity);
            AmmoInMagazine.SetCurrentValue(AmmoInMagazine.GetCurrentValue() + (int) ammoAvailable);
        }

        public int GetRemainingAmmo()
        {
            return (int)AmmoInMagazine.GetCurrentValue();
        }

        public override string GetSummary()
        {
            return Helper.Round(WeaponAttributes.DPS(), 1) + "DPS";
        }

        public override InventoryUi CreateUi(Transform parent)
        {
            return new WeaponUi(this, parent);
        }
    }
}