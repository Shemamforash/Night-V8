using System;
using System.Collections.Generic;
using System.Xml;
using Game.Characters;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using Random = UnityEngine.Random;

namespace Game.Gear.Armour
{
    public class Accessory : GearItem
    {
        private static readonly List<AccessoryTemplate> _accessoryTemplates = new List<AccessoryTemplate>();

        private static bool _readTemplates;
        private readonly AccessoryTemplate _template;
        private AttributeModifier modifier;

        public Accessory(AccessoryTemplate template, ItemQuality itemQuality) : base(template.Name, GearSubtype.Accessory, itemQuality)
        {
            _template = template;
        }

        public override string GetSummary()
        {
            return Name;
        }

        public override void Equip(Character character)
        {
            base.Equip(character);
            Player player = character as Player;
            if (player == null) return;
            modifier.AddTargetAttribute(player.Attributes.Get(_template.TargetAttribute));
        }

        public override void Unequip()
        {
            base.Unequip();
            Player player = EquippedCharacter as Player;
            if (player == null) return;
            modifier.RemoveTargetAttribute(player.Attributes.Get(_template.TargetAttribute));
        }

        private static void ReadTemplates()
        {
            if (_readTemplates) return;
            XmlNode root = Helper.OpenRootNode("Gear", "GearList");
            foreach (XmlNode accessoryNode in root.SelectNodes("Gear"))
            {
                string name = accessoryNode.SelectSingleNode("Name").InnerText;
                string attributeString = accessoryNode.SelectSingleNode("Attribute").InnerText;
                AttributeType attributeType = Inventory.StringToAttributeType(attributeString);
                float bonus = float.Parse(accessoryNode.SelectSingleNode("Bonus").InnerText);
                bool additive = accessoryNode.SelectSingleNode("Type").InnerText == "+";
                new AccessoryTemplate(name, attributeType, bonus, additive);
            }

            _readTemplates = true;
        }

        public static Accessory GenerateAccessory(ItemQuality quality)
        {
            ReadTemplates();
            AccessoryTemplate randomTemplate = _accessoryTemplates[Random.Range(0, _accessoryTemplates.Count)];
            Accessory accessory = new Accessory(randomTemplate, quality);
            AttributeModifier modifier = new AttributeModifier();
            modifier = randomTemplate.GetModifier();
            float rawBonus = modifier.RawBonus() * (int) quality;
            float finalBonus = modifier.FinalBonus() * (int) quality;
            modifier.SetFinalBonus(finalBonus);
            modifier.SetRawBonus(rawBonus);
            accessory.modifier = modifier;
            return accessory;
        }

        public class AccessoryTemplate
        {
            public readonly string Name;
            public readonly AttributeType TargetAttribute;
            private readonly float _modifierValue;
            private readonly bool _additive;

            public AccessoryTemplate(string name, AttributeType attributeType, float modifierValue, bool additive)
            {
                _accessoryTemplates.Add(this);
                Name = name;
                TargetAttribute = attributeType;
                _modifierValue = modifierValue;
                _additive = additive;
            }

            public AttributeModifier GetModifier()
            {
                AttributeModifier modifier = new AttributeModifier();
                if (_additive) modifier.SetRawBonus(_modifierValue);
                else modifier.SetFinalBonus(_modifierValue);
                return modifier;
            }
        }
    }
}