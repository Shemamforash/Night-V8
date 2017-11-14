using System;
using System.Threading;
using Facilitating.Audio;
using Game.Characters;
using Game.Combat;
using Game.Combat.Enemies;
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
using Random = UnityEngine.Random;

namespace Game.Gear.Weapons
{
    public class Weapon : GearItem
    {
        public readonly WeaponClass WeaponClass;
        public readonly WeaponModifier SubClass, SecondaryModifier;
        public readonly bool Automatic;
        public bool Cocked = true;
        private int _ammoInMagazine;
        public readonly MyValue Durability;
        private const int MaxDurability = 20;
        private bool _canEquip;
        public readonly WeaponAttributes WeaponAttributes;
        public readonly int Capacity, Pellets;
        public Action OnFireAction;
        public Action OnReceiveDamageAction;

        public Weapon(WeaponClass weaponClass, WeaponModifier subClass, WeaponModifier secondaryModifier, bool automatic, float weight, int durability) : base(weaponClass.Type.ToString(), weight,
            GearSubtype.Weapon)
        {
            WeaponClass = weaponClass;
            SubClass = subClass;
            SecondaryModifier = secondaryModifier;
            Automatic = automatic;

            Durability = new MyValue(durability, 0, MaxDurability);
            Durability.OnMin(() => { _canEquip = false; });
            Capacity = (int) Math.Ceiling((double) subClass.Capacity * secondaryModifier.CapacityModifier);
            Pellets = (int) Math.Ceiling((double) (subClass.Pellets * secondaryModifier.Pellets));

            WeaponAttributes = new WeaponAttributes(this);
            if (!automatic)
            {
                WeaponAttributes.Damage.AddModifier(1f);
                Capacity /= 2;
                WeaponAttributes.Accuracy.AddModifier(0.5f);
                WeaponAttributes.ReloadSpeed.AddModifier(-0.5f);
            }
#if UNITY_EDITOR
//            Print();
#endif
            UpdateDurability();
            WorldEventManager.GenerateEvent(new WeaponFindEvent(Name));
        }

        public void ConsumeAmmo(int amount = 0)
        {
            _ammoInMagazine -= amount;
            if (_ammoInMagazine < 0)
            {
                throw new Exceptions.MoreAmmoConsumedThanAvailableException();
            }
        }

        private void Print()
        {
            Debug.Log(WeaponClass.Type + " " + SubClass.Name
                      + "\nAutomatic:  " + Automatic
                      + "\nDurability: " + Durability.GetCurrentValue()
                      + "\nAmmo Left:  " + _ammoInMagazine
                      + "\nCapacity:   " + Capacity
                      + "\nPellets:    " + Pellets
                      + "\nDamage:     " + WeaponAttributes.Damage.GetCalculatedValue()
                      + "\nAccuracy:   " + WeaponAttributes.Accuracy.GetCalculatedValue()
                      + "\nFire Rate:  " + WeaponAttributes.FireRate.GetCalculatedValue()
                      + "\nHandling:   " + WeaponAttributes.Handling.GetCalculatedValue()
                      + "\nReload:     " + WeaponAttributes.ReloadSpeed.GetCalculatedValue()
                      + "\nCritChance: " + WeaponAttributes.CriticalChance.GetCalculatedValue() + "\n\n");
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

        public void Reload(Inventory inventory)
        {
            if (inventory == null) return;
            int ammoRequired = Capacity - GetRemainingAmmo();
            int ammoAvailable = (int) inventory.DecrementResource(InventoryResourceType.Ammo, ammoRequired);
            _ammoInMagazine = _ammoInMagazine + ammoAvailable;
        }

        public bool FullyLoaded()
        {
            //TODO check if character has any ammo left
            return GetRemainingAmmo() == Capacity;
        }

        public bool Empty()
        {
            return GetRemainingAmmo() == 0;
        }

        public int GetRemainingAmmo()
        {
            return _ammoInMagazine;
        }

        public override string GetSummary()
        {
            return Helper.Round(WeaponAttributes.DPS(), 1) + "DPS";
        }

        public override ViewParent CreateUi(Transform parent)
        {
            return new WeaponUi(this, parent);
        }
    }
}