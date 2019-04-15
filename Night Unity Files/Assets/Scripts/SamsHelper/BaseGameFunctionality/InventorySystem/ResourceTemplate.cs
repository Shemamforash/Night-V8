using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using Game.Gear.Armour;
using InventorySystem;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class ResourceTemplate
    {
        public readonly string Name;
        public readonly ResourceType ResourceType;
        private readonly bool Consumable;
        public readonly string Description;
        public string EffectString;
        public readonly bool IsEffectPermanent;
        public bool HasEffect;
        public float EffectBonus;
        public AttributeType AttributeType;
        private static readonly List<ResourceTemplate> Meat = new List<ResourceTemplate>();
        private static readonly List<ResourceTemplate> Water = new List<ResourceTemplate>();
        private static readonly List<ResourceTemplate> Plant = new List<ResourceTemplate>();
        public static readonly List<ResourceTemplate> OtherResources = new List<ResourceTemplate>();
        public static readonly List<ResourceTemplate> AllResources = new List<ResourceTemplate>();
        private static float DesertDRCur, MountainsDRCur, RuinsDRCur, SeaDRCur, WastelandDRCur;
        private readonly Dictionary<EnvironmentType, DropRate> _dropRates = new Dictionary<EnvironmentType, DropRate>();
        private static readonly Dictionary<string, Sprite> _resourceSprites = new Dictionary<string, Sprite>();
        public Sprite Sprite;

        private class DropRate
        {
            private readonly float _drMin, _dr;

            public DropRate(ref float drCur, float drDiff)
            {
                _drMin = drCur;
                _dr = _drMin + drDiff;
                drCur = _dr;
            }

            public bool ValueWithinRange(float value)
            {
                return value >= _drMin && value <= _dr;
            }
        }

        private static Sprite TryLoadSprite(string spriteName)
        {
            if (!_resourceSprites.ContainsKey(spriteName))
            {
                Debug.Log(spriteName);
                Sprite sprite = Resources.Load<Sprite>("Images/Container Symbols/" + spriteName);
                _resourceSprites.Add(spriteName, sprite);
            }

            return _resourceSprites[spriteName];
        }

        public static Sprite GetSprite(string spriteName)
        {
            TryLoadSprite(spriteName);
            if (!_resourceSprites.ContainsKey(spriteName)) Debug.Log(spriteName);
            return _resourceSprites[spriteName];
        }

        private static ResourceTemplate StringToTemplate(string templateString)
        {
            return AllResources.FirstOrDefault(t => t.Name == templateString);
        }

        private static ResourceType _lastType = ResourceType.None;

        public ResourceTemplate(XmlNode resourceNode)
        {
            Name = resourceNode.StringFromNode("Name");
            Consumable = resourceNode.BoolFromNode("Consumable");
            switch (resourceNode.StringFromNode("Type"))
            {
                case "Water":
                    ResourceType = ResourceType.Water;
                    Water.Add(this);
                    Sprite = TryLoadSprite("Water");
                    break;
                case "Meat":
                    ResourceType = ResourceType.Meat;
                    Meat.Add(this);
                    Sprite = TryLoadSprite("Meat");
                    break;
                case "Resource":
                    ResourceType = ResourceType.Resource;
                    OtherResources.Add(this);
                    Sprite = TryLoadSprite("Loot");
                    break;
                case "Plant":
                    ResourceType = ResourceType.Plant;
                    Plant.Add(this);
                    Sprite = TryLoadSprite(Name);
                    break;
                case "Potion":
                    ResourceType = ResourceType.Potion;
                    Sprite = TryLoadSprite("Potion");
                    break;
                case "Armour":
                    ResourceType = ResourceType.Armour;
                    Sprite = TryLoadSprite("Armour");
                    break;
            }

            IsEffectPermanent = resourceNode.BoolFromNode("Permanent");
            Description = resourceNode.StringFromNode("Description");
            EffectString = resourceNode.StringFromNode("Effect");
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
            EnvironmentType currentEnvironment = EnvironmentManager.CurrentEnvironmentType;
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
            EnvironmentType currentEnvironment = EnvironmentManager.CurrentEnvironmentType;
            foreach (ResourceTemplate resourceTemplate in OtherResources)
            {
                if (!resourceTemplate._dropRates[currentEnvironment].ValueWithinRange(rand)) continue;
                return resourceTemplate;
            }

            throw new Exception("Can't have invalid Resource!");
        }

        public static ResourceTemplate GetWater()
        {
            float rand = Random.Range(0f, 1f);
            EnvironmentType currentEnvironment = EnvironmentManager.CurrentEnvironmentType;
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
            EnvironmentType currentEnvironment = EnvironmentManager.CurrentEnvironmentType;
            foreach (ResourceTemplate plantTemplate in Plant)
            {
                if (!plantTemplate._dropRates[currentEnvironment].ValueWithinRange(rand)) continue;
                return plantTemplate;
            }

            throw new Exception("Can't have invalid Plant!");
        }

        public ResourceItem Create()
        {
            ResourceItem item;
            if (ResourceType == ResourceType.Armour) item = new Armour(this);
            else if (Consumable) item = new Consumable(this);
            else item = new ResourceItem(this);
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
            string attributeString = attributeNode.InnerText;
            if (attributeString == "") return;
            AttributeType = Inventory.StringToAttributeType(attributeString);
            EffectBonus = resourceNode.FloatFromNode("Modifier");
            HasEffect = true;
        }
    }
}