using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using InventorySystem;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using Random = UnityEngine.Random;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class ResourceTemplate
    {
        public readonly string Name;
        public readonly ResourceType ResourceType;
        private readonly bool Consumable;
        public readonly string Description;
        public readonly bool IsEffectPermanent;
        public bool HasEffect;
        public float EffectBonus;
        public AttributeType AttributeType;
        private static readonly List<ResourceTemplate> Meat = new List<ResourceTemplate>();
        private static readonly List<ResourceTemplate> Water = new List<ResourceTemplate>();
        private static readonly List<ResourceTemplate> Plant = new List<ResourceTemplate>();
        private static readonly List<ResourceTemplate> Resources = new List<ResourceTemplate>();
        public static readonly List<ResourceTemplate> AllResources = new List<ResourceTemplate>();
        private static float DesertDRCur, MountainsDRCur, RuinsDRCur, SeaDRCur, WastelandDRCur;
        private readonly Dictionary<EnvironmentType, DropRate> _dropRates = new Dictionary<EnvironmentType, DropRate>();

        private class DropRate
        {
            private readonly float _drMin, _dr;
            public readonly bool CanDrop;

            public DropRate(ref float drCur, float drDiff)
            {
                CanDrop = drDiff != 0;
                _drMin = drCur;
                _dr = _drMin + drDiff;
                drCur = _dr;
            }

            public bool ValueWithinRange(float value)
            {
                return value >= _drMin && value <= _dr;
            }
        }

        public static ResourceTemplate StringToTemplate(string templateString)
        {
            return AllResources.FirstOrDefault(t => t.Name == templateString);
        }

        private static ResourceType _lastType = ResourceType.None;

        public ResourceTemplate(XmlNode resourceNode)
        {
            Name = resourceNode.StringFromNode("Name");
            Consumable = true;
            switch (resourceNode.StringFromNode("Type"))
            {
                case "Water":
                    ResourceType = ResourceType.Water;
                    Water.Add(this);
                    break;
                case "Meat":
                    ResourceType = ResourceType.Meat;
                    Meat.Add(this);
                    break;
                case "Resource":
                    ResourceType = ResourceType.Resource;
                    Resources.Add(this);
                    Consumable = false;
                    break;
                case "Plant":
                    ResourceType = ResourceType.Plant;
                    Plant.Add(this);
                    break;
                case "Potion":
                    ResourceType = ResourceType.Potion;
                    break;
            }

            IsEffectPermanent = resourceNode.BoolFromNode("Permanent");
            Description = resourceNode.StringFromNode("Description");
            AllResources.Add(this);

            if (_lastType != ResourceType)
            {
                DesertDRCur = 0f;
                MountainsDRCur = 0f;
                RuinsDRCur = 0f;
                SeaDRCur = 0f;
                WastelandDRCur = 0f;
            }

            _lastType = ResourceType;

            float desertDr = resourceNode.FloatFromNode("DesertDropRate");
            float mountainsDr = resourceNode.FloatFromNode("MountainsDropRate");
            float ruinsDr = resourceNode.FloatFromNode("RuinsDropRate");
            float seaDr = resourceNode.FloatFromNode("SeaDropRate");
            float wastelandDr = resourceNode.FloatFromNode("WastelandDropRate");
            _dropRates.Add(EnvironmentType.Desert, new DropRate(ref DesertDRCur, desertDr));
            _dropRates.Add(EnvironmentType.Mountains, new DropRate(ref MountainsDRCur, mountainsDr));
            _dropRates.Add(EnvironmentType.Sea, new DropRate(ref SeaDRCur, seaDr));
            _dropRates.Add(EnvironmentType.Ruins, new DropRate(ref RuinsDRCur, ruinsDr));
            _dropRates.Add(EnvironmentType.Wasteland, new DropRate(ref WastelandDRCur, wastelandDr));
            SetEffect(resourceNode);
        }

        public static ResourceItem GetMeat()
        {
            float rand = Random.Range(0f, 1f);
            EnvironmentType currentEnvironment = EnvironmentManager.CurrentEnvironment.EnvironmentType;
            foreach (ResourceTemplate meatTemplate in Meat)
            {
                if (!meatTemplate._dropRates[currentEnvironment].ValueWithinRange(rand)) continue;
                return meatTemplate.Create();
            }

            throw new Exception("Can't have invalid meat!");
        }

        public static ResourceTemplate GetResource()
        {
            float rand = Random.Range(0f, 1f);
            EnvironmentType currentEnvironment = EnvironmentManager.CurrentEnvironment.EnvironmentType;
            foreach (ResourceTemplate resourceTemplate in Resources)
            {
                if (!resourceTemplate._dropRates[currentEnvironment].ValueWithinRange(rand)) continue;
                return resourceTemplate;
            }

            throw new Exception("Can't have invalid Resource!");
        }

        public static ResourceTemplate GetWater()
        {
            float rand = Random.Range(0f, 1f);
            EnvironmentType currentEnvironment = EnvironmentManager.CurrentEnvironment.EnvironmentType;
            foreach (ResourceTemplate waterTemplate in Water)
            {
                if (!waterTemplate._dropRates[currentEnvironment].ValueWithinRange(rand)) continue;
                return waterTemplate;
            }

            throw new Exception("Can't have invalid Water!");
        }

        public static ResourceTemplate GetPlant()
        {
            float rand = Random.Range(0f, 1f);
            EnvironmentType currentEnvironment = EnvironmentManager.CurrentEnvironment.EnvironmentType;
            if (Region.InTutorialPeriod())
            {
                return Plant.FindAll(r => r.AttributeType != AttributeType.Grit && r._dropRates[currentEnvironment].CanDrop).RandomElement();
            }
            foreach (ResourceTemplate plantTemplate in Plant)
            {
                if (!plantTemplate._dropRates[currentEnvironment].ValueWithinRange(rand)) continue;
                return plantTemplate;
            }
            
            throw new Exception("Can't have invalid Plant!");
        }

        public ResourceItem Create()
        {
            ResourceItem item = Consumable ? new Consumable(this) : new ResourceItem(this);
            return item;
        }

        public static ResourceItem Create(string name)
        {
            ResourceTemplate template = StringToTemplate(name);
            if (template == null) throw new Exceptions.ResourceDoesNotExistException(name);
            return template.Create();
        }

        private void SetEffect(XmlNode resourceNode)
        {
            XmlNode attributeNode = resourceNode.SelectSingleNode("Attribute");
            if (attributeNode == null) return;
            string attributeString = attributeNode.InnerText;
            if (attributeString == "") return;
            AttributeType = Inventory.StringToAttributeType(attributeString);
            EffectBonus = resourceNode.FloatFromNode("Modifier");
            HasEffect = true;
        }
    }
}