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
        private readonly ArmourType _armourType;
        private readonly int _maxProtection;
        private int _currentProtection;

        public enum ArmourType
        {
            Head,
            Chest
        }

        public override XmlNode Save(XmlNode root)
        {
            root = root.CreateChild("Armour");
            base.Save(root);
            root.CreateChild("CurrentProtection", _currentProtection);
            root.CreateChild("Type", (int) _armourType);
            return root;
        }

        private Armour(ItemQuality quality, ArmourType armourType) : base(QualityToName(quality, armourType), quality)
        {
            _maxProtection = (int) quality + 1;
            _currentProtection = _maxProtection;
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

        public bool TakeDamage()
        {
            --_currentProtection;
            if (_currentProtection > 0) return false;
            PlayerCombat.Instance.WeaponAudio.BreakArmour();
            UnEquip();
            Inventory.Destroy(this);
            return true;
        }

        public bool CanRepair()
        {
            return _currentProtection < _maxProtection;
        }

        public void Repair()
        {
            ++_currentProtection;
        }

        public ArmourType GetArmourType() => _armourType;
        public int GetMaxProtection() => _maxProtection;
        public int GetCurrentProtection() => _currentProtection;

        public static Armour LoadArmour(XmlNode root)
        {
            ItemQuality quality = (ItemQuality) root.IntFromNode("Quality");
            ArmourType type = (ArmourType) root.IntFromNode("Type");
            Armour plate = Create(quality, type);
            plate.Load(root);
            plate._currentProtection = root.IntFromNode("CurrentProtection");
            return plate;
        }
    }
}