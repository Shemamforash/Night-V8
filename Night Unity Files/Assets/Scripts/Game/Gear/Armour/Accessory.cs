using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Gear.Armour
{
    public class Accessory : GearItem
    {
        private static readonly List<AccessoryTemplate> _accessoryTemplates = new List<AccessoryTemplate>();
        private static bool _readTemplates;
        private readonly AccessoryTemplate _template;
        private AttributeModifier modifier;

        private Accessory(AccessoryTemplate template, ItemQuality itemQuality) : base(template.Name, GearSubtype.Accessory, itemQuality)
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
            player?.Attributes.Get(_template.TargetAttribute).AddModifier(modifier);
        }

        public override void Unequip()
        {
            base.Unequip();
            Player player = EquippedCharacter as Player;
            player?.Attributes.Get(_template.TargetAttribute).RemoveModifier(modifier);
        }

        private static void ReadTemplates()
        {
            if (_readTemplates) return;
            XmlNode root = Helper.OpenRootNode("Gear", "GearList");
            foreach (XmlNode accessoryNode in Helper.GetNodesWithName(root, "Gear"))
                new AccessoryTemplate(accessoryNode);
            _readTemplates = true;
        }

        public static Accessory Generate()
        {
            ReadTemplates();
            AccessoryTemplate randomTemplate = _accessoryTemplates[Random.Range(0, _accessoryTemplates.Count)];

            ItemQuality quality = (ItemQuality) WorldState.GenerateGearLevel();

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

        public override XmlNode Save(XmlNode doc)
        {
            doc = base.Save(doc);
            doc.CreateChild("Template", _template.Name);
            return doc;
        }

        private class AccessoryTemplate
        {
            public readonly string Name;
            public readonly AttributeType TargetAttribute;
            private readonly float _modifierValue;

            public AccessoryTemplate(XmlNode accessoryNode)
            {
                Name = accessoryNode.GetNodeText("Name");
                TargetAttribute = Inventory.StringToAttributeType(accessoryNode.GetNodeText("Attribute"));
                _modifierValue = accessoryNode.FloatFromNode("Bonus");
                _accessoryTemplates.Add(this);
            }

            public AttributeModifier GetModifier()
            {
                AttributeModifier modifier = new AttributeModifier();
                modifier.SetFinalBonus(_modifierValue);
                return modifier;
            }
        }

        public static Accessory LoadAccessory(XmlNode accessoryNode)
        {
            string templateString = accessoryNode.GetNodeText("Template");
            AccessoryTemplate template = _accessoryTemplates.First(t => t.Name == templateString);
            Accessory accessory = new Accessory(template, ItemQuality.Worn);
            accessory.Load(accessoryNode);
            return accessory;
        }
    }
}