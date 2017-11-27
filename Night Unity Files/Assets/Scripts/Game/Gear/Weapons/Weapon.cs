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
        public readonly bool Automatic;
        public bool Cocked = true;
        private int _ammoInMagazine;
        private bool _canEquip;
        public readonly WeaponAttributes WeaponAttributes;
        public Action OnFireAction;
        public Action OnReceiveDamageAction;

        public Weapon(WeaponClass weaponClass, GearModifier subClass, GearModifier secondaryModifier, bool automatic, float weight, int durability) : base(weaponClass.Type.ToString(), weight,
            GearSubtype.Weapon)
        {
            WeaponAttributes = new WeaponAttributes(weaponClass, subClass, secondaryModifier);
            Automatic = automatic;
//            Durability.OnMin(() => { _canEquip = false; });
            GearModifier manualModifier = new GearModifier("Manual");
            manualModifier.CreateAndAddAttributeModifier(WeaponAttributes.Capacity, 0, 1);
            manualModifier.CreateAndAddAttributeModifier(WeaponAttributes.Damage, 0, 1);
            manualModifier.CreateAndAddAttributeModifier(WeaponAttributes.Accuracy, 0, 0.5f);
            manualModifier.CreateAndAddAttributeModifier(WeaponAttributes.ReloadSpeed, 0, -0.5f);

            if (!automatic)
            {
                manualModifier.Apply();
            }
            UpdateDurability();
            WeaponAttributes.RecalculateAttributeValues();
#if UNITY_EDITOR
            Print();
#endif
            WorldEventManager.GenerateEvent(new WeaponFindEvent(Name));
        }

        public override bool IsStackable()
        {
            return false;
        }

        public WeaponType WeaponType()
        {
            return WeaponAttributes.WeaponClass.Type;
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
            Debug.Log(WeaponAttributes.WeaponClass.Type + " " + WeaponAttributes.SubClass.Name
                      + "\nAutomatic:  " + Automatic
                      + "\nDurability: " + WeaponAttributes.Durability.CurrentValue()
                      + "\nAmmo Left:  " + _ammoInMagazine
                      + "\nCapacity:   " + WeaponAttributes.Capacity.CurrentValue()
                      + "\nPellets:    " + WeaponAttributes.Pellets.CurrentValue()
                      + "\nDamage:     " + WeaponAttributes.Damage.CurrentValue()
                      + "\nAccuracy:   " + WeaponAttributes.Accuracy.CurrentValue()
                      + "\nFire Rate:  " + WeaponAttributes.FireRate.CurrentValue()
                      + "\nHandling:   " + WeaponAttributes.Handling.CurrentValue()
                      + "\nReload:     " + WeaponAttributes.ReloadSpeed.CurrentValue()
                      + "\nCritChance: " + WeaponAttributes.CriticalChance.CurrentValue() + "\n\n");
        }

        public float GetAttributeValue(AttributeType attributeType)
        {
            return WeaponAttributes.Get(attributeType).CurrentValue();
        }

        public void IncreaseDurability()
        {
            _canEquip = true;
//            Durability.SetCurrentValue(Durability.CurrentValue() + 1);
            UpdateDurability();
        }

        private void UpdateDurability()
        {
            string quality = "Perfected";
            if (WeaponAttributes.Durability < 4)
            {
                quality = "Flawed";
            }
            else if (WeaponAttributes.Durability < 8)
            {
                quality = "Worn";
            }
            else if (WeaponAttributes.Durability < 12)
            {
                quality = "Fresh";
            }
            else if (WeaponAttributes.Durability < 16)
            {
                quality = "Faultless";
            }
            Name = WeaponAttributes.SecondaryModifier.Name + " " + WeaponAttributes.SubClass.Name + " -- (" + quality + ")";
            WeaponAttributes.RecalculateAttributeValues();
        }

        public void DecreaseDurability()
        {
//            Durability.SetCurrentValue(Durability.CurrentValue() - 1);
            UpdateDurability();
        }

        public string GetWeaponType()
        {
            return WeaponAttributes.WeaponClass.Type.ToString();
        }

        public void Reload(Inventory inventory)
        {
            if (inventory == null) return;
            int ammoRequired = (int)WeaponAttributes.Capacity.CurrentValue() - GetRemainingAmmo();
            int ammoAvailable = (int) inventory.DecrementResource(InventoryResourceType.Ammo, ammoRequired);
            _ammoInMagazine = _ammoInMagazine + ammoAvailable;
        }

        public bool FullyLoaded()
        {
            //TODO check if character has any ammo left
            return GetRemainingAmmo() == (int)WeaponAttributes.Capacity.CurrentValue();
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