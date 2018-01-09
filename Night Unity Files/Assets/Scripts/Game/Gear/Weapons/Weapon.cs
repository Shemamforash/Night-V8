using System;
using System.Threading;
using Facilitating.Audio;
using Facilitating.UIControllers;
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
        public bool Cocked = true;
        private int _ammoInMagazine;
        private bool _canEquip;
        public readonly WeaponAttributes WeaponAttributes;
        public Action OnFireAction;
        public Action OnReceiveDamageAction;

        public Weapon(string name, float weight) : base(name, weight, GearSubtype.Weapon)
        {
            WeaponAttributes = new WeaponAttributes();
//            Durability.OnMin(() => { _canEquip = false; });
        }
        
        public override bool IsStackable()
        {
            return false;
        }

        public WeaponType WeaponType()
        {
            return WeaponAttributes.WeaponType;
        }

        public int GetRemainingMagazines()
        {
            return (int)ParentInventory.GetResourceQuantity(WeaponAttributes.AmmoType);
        }

        public void ConsumeAmmo(int amount = 0)
        {
            _ammoInMagazine -= amount;
            if (_ammoInMagazine < 0)
            {
                throw new Exceptions.MoreAmmoConsumedThanAvailableException();
            }
        }

        public float GetAttributeValue(AttributeType attributeType)
        {
            return WeaponAttributes.Get(attributeType).CurrentValue();
        }

        public void IncreaseDurability()
        {
            _canEquip = true;
            WeaponAttributes.Durability.Increment(1);
            WeaponAttributes.RecalculateAttributeValues();
            WorldState.HomeInventory().GetResource(InventoryResourceType.Scrap).Decrement(GetUpgradeCost());
            SetName();
        }

        public void SetName()
        {
            string quality = WeaponAttributes.Durability.GetThresholdName();
            Name = WeaponAttributes.GetName() + " -- (" + quality + ")";
        }

        public void DecreaseDurability()
        {
            WeaponAttributes.Durability.Decrement(1);
            WeaponAttributes.RecalculateAttributeValues();
            SetName();
        }

        public string GetWeaponType()
        {
            return WeaponAttributes.WeaponType.ToString();
        }

        public void Reload(Inventory inventory)
        {
            if (!(inventory?.GetResourceQuantity(WeaponAttributes.AmmoType) >= 1)) return;
            _ammoInMagazine = (int) WeaponAttributes.Capacity.CurrentValue();
            inventory.DecrementResource(WeaponAttributes.AmmoType, 1);
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

        public int GetUpgradeCost()
        {
            return (int) (WeaponAttributes.Durability.CurrentValue() * 10 + 100);
        }

        public override ViewParent CreateUi(Transform parent)
        {
            ViewParent weaponUi = base.CreateUi(parent);
            weaponUi.PrimaryButton.AddOnClick(() => UiWeaponUpgradeController.Show(this));
            return weaponUi;
        }
    }
}