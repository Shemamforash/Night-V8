using System;
using System.Xml;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.Persistence;
using UnityEngine;

namespace Game.Gear.Weapons
{
    public class Weapon : GearItem
    {
        public readonly WeaponAttributes WeaponAttributes;
        public Skill WeaponSkillOne, WeaponSkillTwo;
        private const float MaxAccuracyOffsetInDegrees = 25f;
        private const float RangeMin = 1f;
        private const float RangeMax = 4.5f;

        public Weapon(string name, float weight, ItemQuality _itemQuality) : base(name, weight, GearSubtype.Weapon, _itemQuality)
        {
            WeaponAttributes = new WeaponAttributes(this);
//            Durability.OnMin(() => { _canEquip = false; });
        }

        public override XmlNode Save(XmlNode root, PersistenceType saveType)
        {
            root = base.Save(root, saveType);
            WeaponAttributes.Save(root, saveType);
            return root;
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

        public override bool IsStackable() => false;

        public WeaponType WeaponType() => WeaponAttributes.WeaponType;

        public float GetAttributeValue(AttributeType attributeType) => WeaponAttributes.Get(attributeType).CurrentValue();

        public void SetName()
        {
            string quality = Quality().ToString();
            Name = quality + " " + WeaponAttributes.GetWeaponClass();
        }

        public string GetWeaponType() => WeaponAttributes.WeaponType.ToString();

        public override string GetSummary() => Helper.Round(WeaponAttributes.DPS(), 1) + "DPS";

        public int GetUpgradeCost() => (int) (WeaponAttributes.Durability.CurrentValue() * 10 + 100);

        public bool Inscribable() => Quality() == ItemQuality.Shining;

        public BaseWeaponBehaviour InstantiateWeaponBehaviour(CharacterCombat player)
        {
            BaseWeaponBehaviour weaponBehaviour;
            switch (WeaponAttributes.GetWeaponClass())
            {
                case WeaponClassType.Shortshooter:
                    weaponBehaviour = player.gameObject.AddComponent<DoubleFireInstant>();
                    break;
//                case WeaponClassType.Voidwalker:
//                    break;
                case WeaponClassType.Skullcrusher:
                    weaponBehaviour = player.gameObject.AddComponent<DoubleFireDelay>();
                    break;
                case WeaponClassType.Spitter:
                    weaponBehaviour = player.gameObject.AddComponent<Burstfire>();
                    break;
                case WeaponClassType.Spewer:
                    weaponBehaviour = player.gameObject.AddComponent<AccuracyGainer>();
                    break;
                case WeaponClassType.Breacher:
                    weaponBehaviour = player.gameObject.AddComponent<AttributeGainer>();
                    break;
                case WeaponClassType.Annihilator:
                    weaponBehaviour = player.gameObject.AddComponent<RandomFire>();
                    break;
                default:
                    weaponBehaviour = player.gameObject.AddComponent<DefaultBehaviour>();
                    break;
            }
            weaponBehaviour.Initialise(player.Weapon());
            return weaponBehaviour;
        }
    }
}