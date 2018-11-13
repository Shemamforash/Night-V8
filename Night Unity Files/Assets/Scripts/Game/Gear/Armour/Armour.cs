using System;
using System.Xml;
using Facilitating.Persistence;
using Game.Combat.Player;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Gear.Armour
{
    public class Armour : GearItem
    {
        public const float ArmourHealthUnit = 200;
        private readonly Number _armourHealth = new Number();
        private readonly ArmourType _armourType;
        private readonly int _maxProtection;

        public enum ArmourType
        {
            Head,
            Chest
        }

        public override XmlNode Save(XmlNode root)
        {
            root = root.CreateChild("Armour");
            base.Save(root);
            root.CreateChild("Health", _armourHealth.CurrentValue());
            root.CreateChild("Type", (int)_armourType);
            return root;
        }

        private Armour(ItemQuality quality, ArmourType armourType) : base(QualityToName(quality, armourType), quality)
        {
            _maxProtection = (int) quality + 1;
            _armourHealth.Max = _maxProtection * ArmourHealthUnit;
            _armourHealth.SetCurrentValue(_armourHealth.Max);
            _armourType = armourType;
        }

        private static string QualityToName(ItemQuality quality, ArmourType type)
        {
            switch (type)
            {
                case ArmourType.Head:
                    return GetHeadArmourName(quality);
                case ArmourType.Chest:
                    return GetChestArmourName(quality);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string GetChestArmourName(ItemQuality quality)
        {
            string armourName = quality + " ";
            switch (quality)
            {
                case ItemQuality.Dark:
                    armourName += "Hide";
                    break;
                case ItemQuality.Dull:
                    armourName += "Weave";
                    break;
                case ItemQuality.Glowing:
                    armourName += "Plate";
                    break;
                case ItemQuality.Shining:
                    armourName += "Scale";
                    break;
                case ItemQuality.Radiant:
                    armourName += "Aegis";
                    break;
            }

            return armourName;
        }

        private static string GetHeadArmourName(ItemQuality quality)
        {
            string armourName = quality + " ";
            switch (quality)
            {
                case ItemQuality.Dark:
                    armourName += "Veil";
                    break;
                case ItemQuality.Dull:
                    armourName += "Mask";
                    break;
                case ItemQuality.Glowing:
                    armourName += "Visor";
                    break;
                case ItemQuality.Shining:
                    armourName += "Crown";
                    break;
                case ItemQuality.Radiant:
                    armourName += "Halo";
                    break;
            }

            return armourName;
        }

        public static Armour Create(ItemQuality quality)
        {
            ArmourType armourType = (ArmourType) Random.Range(0, 2);
            return new Armour(quality, armourType);
        }

        public static Armour Create(ItemQuality quality, ArmourType armourType)
        {
            return new Armour(quality, armourType);
        }

        public override string GetSummary() => "+" + _maxProtection + " Armour";

        public bool TakeDamage(float amount)
        {
            _armourHealth.Decrement(amount);
            if (!_armourHealth.ReachedMin()) return false;
            PlayerCombat.Instance.WeaponAudio.BreakArmour();
            UnEquip();
            Inventory.Destroy(this);
            return true;
        }

        public float Repair(float amount)
        {
            float difference = _armourHealth.Max - _armourHealth.CurrentValue();
            _armourHealth.Increment(amount);
            amount -= difference;
            if (amount < 0) amount = 0;
            return amount;
        }
        
        public ArmourType GetArmourType() => _armourType;
        public int GetMaxProtection() => (int) Quality() + 1;
        public int GetCurrentProtection() => Mathf.CeilToInt(_armourHealth.CurrentValue() / ArmourHealthUnit);

        public static Armour LoadArmour(XmlNode root)
        {
            ItemQuality quality = (ItemQuality) root.IntFromNode("Quality");
            ArmourType type = (ArmourType) root.IntFromNode("Type");
            Armour plate = Create(quality, type);
            plate.Load(root);
            plate._armourHealth.SetCurrentValue(root.FloatFromNode("Health"));
            return plate;
        }
    }
}