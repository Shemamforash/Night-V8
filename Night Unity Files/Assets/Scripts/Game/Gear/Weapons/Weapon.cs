using System.Collections.Generic;
using System.Xml;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Global;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Gear.Weapons
{
    public class Weapon : GearItem
    {
        private const float MaxAccuracyOffsetInDegrees = 25f;
        private const float RangeMin = 1f;
        private const float RangeMax = 4.5f;
        public readonly WeaponAttributes WeaponAttributes;
        private int _ammoInMagazine;
        private long _timeAtLastFire;
        public Skill WeaponSkillOne, WeaponSkillTwo;

        public Weapon(string name, float weight, ItemQuality _itemQuality, int durability = -1) : base(name, weight, GearSubtype.Weapon, _itemQuality)
        {
            WeaponAttributes = new WeaponAttributes(this, durability);
//            Durability.OnMin(() => { _canEquip = false; });
        }

        public override XmlNode Save(XmlNode root, PersistenceType saveType)
        {
            root = base.Save(root, saveType);
            WeaponAttributes.Save(root, saveType);
            return root;
        }

        private bool FireRateElapsedTimeMet()
        {
            long timeElapsed = Helper.TimeInMillis() - _timeAtLastFire;
            long targetTime = (long) (1f / GetAttributeValue(AttributeType.FireRate) * 1000);
            return !(timeElapsed < targetTime);
        }

        public float CalculateIdealDistance()
        {
            float range = WeaponAttributes.GetCalculatedValue(AttributeType.Accuracy) / 100f;
            float idealDistance = (RangeMax - RangeMin) * range + RangeMin;
            return idealDistance;
        }

        public float CalculateBaseAccuracy()
        {
            float accuracy = 1f - WeaponAttributes.GetCalculatedValue(AttributeType.Accuracy) / 100f;
            accuracy *= MaxAccuracyOffsetInDegrees;
            return accuracy;
        }

        private bool CanFire()
        {
            return !Empty() && FireRateElapsedTimeMet();
        }

        public List<Shot> Fire(CharacterCombat origin, bool fireShots = false)
        {
            List<Shot> shots = new List<Shot>();
            if (CanFire())
            {
                _timeAtLastFire = Helper.TimeInMillis();
                //todo play sound GunFire.Fire(WeaponAttributes.WeaponType, distance);
                for (int i = 0; i < WeaponAttributes.GetCalculatedValue(AttributeType.Pellets); ++i)
                {
                    shots.Add(Shot.Create(origin));
                }
                ConsumeAmmo(1);
                Assert.IsTrue(shots.Count > 0);
                Assert.IsNotNull(shots);
                if (fireShots) shots.ForEach(s => s.Fire());
            }

            return shots;
        }

        public override bool IsStackable()
        {
            return false;
        }

        public WeaponType WeaponType()
        {
            return WeaponAttributes.WeaponType;
        }

        public void ConsumeAmmo(int amount = 0)
        {
            _ammoInMagazine -= amount;
            if (_ammoInMagazine < 0) throw new Exceptions.MoreAmmoConsumedThanAvailableException();
        }

        public float GetAttributeValue(AttributeType attributeType)
        {
            return WeaponAttributes.Get(attributeType).CurrentValue();
        }

        public int Capacity()
        {
            return (int) WeaponAttributes.Get(AttributeType.Capacity).CurrentValue();
        }

        public void IncreaseDurability()
        {
            WeaponAttributes.Durability.Increment();
            WeaponAttributes.RecalculateAttributeValues();
            WorldState.HomeInventory().GetResource(InventoryResourceType.Scrap).Decrement(GetUpgradeCost());
            SetName();
        }

        public void SetName()
        {
            string quality = Quality().ToString();
            Name = quality + " " + WeaponAttributes.GetName();
        }

        public void DecreaseDurability()
        {
            WeaponAttributes.Durability.Decrement();
            WeaponAttributes.RecalculateAttributeValues();
        }

        public string GetWeaponType()
        {
            return WeaponAttributes.WeaponType.ToString();
        }

        public void Reload(Inventory inventory = null)
        {
            _ammoInMagazine = (int) WeaponAttributes.Capacity.CurrentValue();
        }

        public bool FullyLoaded()
        {
            return GetRemainingAmmo() == (int) WeaponAttributes.Capacity.CurrentValue();
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
            return weaponUi;
        }

        public bool Inscribable()
        {
            return Quality() == ItemQuality.Shining;
        }
    }
}