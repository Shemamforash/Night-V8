using System.Collections.Generic;
using System.Xml;
using Game.Gear.UI;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace Game.Gear.Armour
{
    public class Accessory : GearItem
    {
        private static List<AccessoryTemplate> _accessoryTemplates = new List<AccessoryTemplate>();

        private static bool _readTemplates;
        private AccessoryTemplate _template;

        public Accessory(AccessoryTemplate template, ItemQuality itemQuality) : base(template.Name, template.Weight, GearSubtype.Accessory, itemQuality)
        {
            _template = template;
        }

        public override string GetSummary()
        {
            return _template.Description;
        }

        public class AccessoryTemplate
        {
            public readonly string Name, Description;
            public readonly float Weight;

            public AccessoryTemplate(string name, int weight, string description, string effect)
            {
                _accessoryTemplates.Add(this);
                Name = name;
                Weight = weight;
                Description = description;
            }
        }

        private static void ReadTemplates()
        {
            if (_readTemplates) return;
            TextAsset accessoryFile = Resources.Load<TextAsset>("XML/Gear");
            XmlDocument accessoryXml = new XmlDocument();
            accessoryXml.LoadXml(accessoryFile.text);
            XmlNode root = accessoryXml.SelectSingleNode("GearList");
            foreach (XmlNode accessoryNode in root.SelectNodes("Accessory"))
            {
                string name = accessoryNode.SelectSingleNode("Name").InnerText;
                int weight = int.Parse(accessoryNode.SelectSingleNode("Weight").InnerText);
                string description = accessoryNode.SelectSingleNode("Description").InnerText;
                string effect = accessoryNode.SelectSingleNode("Effect").InnerText;
                new AccessoryTemplate(name, weight, description, effect);
            }

            _readTemplates = true;
        }

        public static Accessory GenerateAccessory(ItemQuality quality)
        {
            ReadTemplates();
            AccessoryTemplate randomTemplate = _accessoryTemplates[Random.Range(0, _accessoryTemplates.Count)];
            return new Accessory(randomTemplate, quality);
        }
        
//        public override ViewParent CreateUi(Transform parent)
//        {
//            return new AccessoryUi(this, parent);
//        }
    }
}