﻿using System.Collections.Generic;
using System.Xml;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Gear.Armour
{
    public class Accessory : GearItem
    {
        private static readonly List<AccessoryTemplate> _accessoryTemplates = new List<AccessoryTemplate>();

        private static bool _readTemplates;
        private readonly AccessoryTemplate _template;

        public Accessory(AccessoryTemplate template, ItemQuality itemQuality) : base(template.Name, GearSubtype.Accessory, itemQuality)
        {
            _template = template;
        }

        public override string GetSummary()
        {
            return _template.Description;
        }

        private static void ReadTemplates()
        {
            if (_readTemplates) return;
            XmlNode root = Helper.OpenRootNode("Gear", "GearList");
            foreach (XmlNode accessoryNode in root.SelectNodes("Accessory"))
            {
                string name = accessoryNode.SelectSingleNode("Name").InnerText;
                int weight = int.Parse(accessoryNode.SelectSingleNode("Weight").InnerText);
                string description = accessoryNode.SelectSingleNode("Description").InnerText;
                string effect = accessoryNode.SelectSingleNode("Effect").InnerText;
                new AccessoryTemplate(name, description);
            }

            _readTemplates = true;
        }

        public static Accessory GenerateAccessory(ItemQuality quality)
        {
            ReadTemplates();
            AccessoryTemplate randomTemplate = _accessoryTemplates[Random.Range(0, _accessoryTemplates.Count)];
            return new Accessory(randomTemplate, quality);
        }

        public bool Inscribable()
        {
            return Quality() == ItemQuality.Radiant;
        }

        public class AccessoryTemplate
        {
            public readonly string Name, Description;

            public AccessoryTemplate(string name, string description)
            {
                _accessoryTemplates.Add(this);
                Name = name;
                Description = description;
            }
        }
    }
}