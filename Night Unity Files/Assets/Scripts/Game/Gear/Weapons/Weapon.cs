using System.Xml;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Combat;
using Game.Combat.Skills;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace Game.Gear.Weapons
{
    public class Weapon : GearItem
    {
        private int _ammoInMagazine;
        public readonly WeaponAttributes WeaponAttributes;
        public Skill WeaponSkillOne, WeaponSkillTwo;
        private long _timeAtLastFire;

        public override XmlNode Save(XmlNode root, PersistenceType saveType)
        {
            root = base.Save(root, saveType);
            WeaponAttributes.Save(root, saveType);
            return root;
        }
        
        public Weapon(string name, float weight, int durability) : base(name, weight, GearSubtype.Weapon)
        {
            WeaponAttributes = new WeaponAttributes(durability);
//            Durability.OnMin(() => { _canEquip = false; });
        }
        
        private bool FireRateElapsedTimeMet()
        {
            long timeElapsed = Helper.TimeInMillis() - _timeAtLastFire;
            long targetTime = (long) (1f / GetAttributeValue(AttributeType.FireRate) * 1000);
            return !(timeElapsed < targetTime);
        }

        public bool CanFire()
        {
            return !Empty() && FireRateElapsedTimeMet();
        }
        
        public Shot Fire(Character target, Character origin)
        {
            _timeAtLastFire = Helper.TimeInMillis();
            return new Shot(target, origin);
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
            WeaponAttributes.Durability.Increment();
            WorldState.HomeInventory().GetResource(InventoryResourceType.Scrap).Decrement(GetUpgradeCost());
            SetName();
        }

        public void SetName()
        {
            string quality = WeaponAttributes.DurabilityToQuality();
            Name = WeaponAttributes.GetName() + " -- (" + quality + ")";
        }

        public void DecreaseDurability()
        {
            WeaponAttributes.Durability.Decrement();
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