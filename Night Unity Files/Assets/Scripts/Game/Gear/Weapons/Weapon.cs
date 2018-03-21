using System.Collections.Generic;
using System.Xml;
using Facilitating.Audio;
using Facilitating.UIControllers;
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
        private const float MaxAccuracyOffsetInDegrees = 45f;
        private const float IdealDistanceProbabilityTarget = 0.1f;

        public override XmlNode Save(XmlNode root, PersistenceType saveType)
        {
            root = base.Save(root, saveType);
            WeaponAttributes.Save(root, saveType);
            return root;
        }

        public Weapon(string name, float weight, ItemQuality _itemQuality, int durability = -1) : base(name, weight, GearSubtype.Weapon, _itemQuality)
        {
            WeaponAttributes = new WeaponAttributes(this, durability);
//            Durability.OnMin(() => { _canEquip = false; });
        }

        private bool FireRateElapsedTimeMet()
        {
            long timeElapsed = Helper.TimeInMillis() - _timeAtLastFire;
            long targetTime = (long) (1f / GetAttributeValue(AttributeType.FireRate) * 1000);
            return !(timeElapsed < targetTime);
        }

        public float CalculateHitProbability(float distance)
        {
            float accuracy = CalculateBaseAccuracy();
            float targetWidth = 0.1f;
            float oppositeLength = distance * Mathf.Tan(accuracy * Mathf.Deg2Rad);
            float probability = targetWidth / 2f / oppositeLength;
            probability = Mathf.Clamp(probability, 0f, 1f);
            return probability;
        }

        public float CalculateIdealDistance()
        {
            float accuracy = CalculateBaseAccuracy();
            Debug.Log(accuracy);
            float targetWidth = 0.1f;
            float oppositeLength = targetWidth / 2f / IdealDistanceProbabilityTarget;
            Debug.Log(oppositeLength);
            float distance = oppositeLength / Mathf.Tan(accuracy * Mathf.Deg2Rad);
            Debug.Log(distance);
            Debug.Log(PathingGrid.WorldToGridDistance(distance));
            return distance;
        }

        public float CalculateBaseAccuracy()
        {
            float accuracy = 1 - WeaponAttributes.GetCalculatedValue(AttributeType.Range) / 100f;
            accuracy *= MaxAccuracyOffsetInDegrees;
            return accuracy;
        }

        private bool CanFire()
        {
            return !Empty() && FireRateElapsedTimeMet();
        }

        public List<Shot> Fire(CharacterCombat origin)
        {
            if (origin.GetTarget() == null) return null;
            if (!CanFire() || origin.Immobilised()) return null;
            _timeAtLastFire = Helper.TimeInMillis();
            float distance = origin is PlayerCombat ? 0 : ((DetailedEnemyCombat) origin).DistanceToPlayer;
            GunFire.Fire(WeaponAttributes.WeaponType, distance);
            List<Shot> shots = new List<Shot>();
            for (int i = 0; i < WeaponAttributes.GetCalculatedValue(AttributeType.Pellets); ++i)
            {
                shots.Add(Shot.CreateShot(origin));
            }

            ConsumeAmmo(1);
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

        public int GetRemainingMagazines()
        {
            return (int) ParentInventory.GetResourceQuantity(WeaponAttributes.AmmoType);
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
            string quality = Quality().ToString();
            Name = quality + " " + WeaponAttributes.GetName();
        }

        public void DecreaseDurability()
        {
            WeaponAttributes.Durability.Decrement();
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