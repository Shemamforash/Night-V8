﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Game.Exploration.Environment;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using Random = UnityEngine.Random;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class ResourceTemplate
    {
        public readonly string Name, ResourceType;
        private readonly bool Consumable;
        public bool HasEffect;
        public float EffectBonus;
        public AttributeType AttributeType;
        private static readonly List<ResourceTemplate> Meat = new List<ResourceTemplate>();
        private static readonly List<ResourceTemplate> Water = new List<ResourceTemplate>();
        private static readonly List<ResourceTemplate> Plant = new List<ResourceTemplate>();
        private static readonly List<ResourceTemplate> Resources = new List<ResourceTemplate>();
        private static readonly List<ResourceTemplate> AllResources = new List<ResourceTemplate>();
        private static float OasisDRCur, SteppeDRCur, RuinsDRCur, DefilesDRCur, WastelandDRCur;
        private readonly Dictionary<EnvironmentType, DropRate> _dropRates = new Dictionary<EnvironmentType, DropRate>();

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

        public static ResourceTemplate StringToTemplate(string templateString)
        {
            return AllResources.FirstOrDefault(t => t.Name == templateString);
        }

        private static string _lastType = "";

        public ResourceTemplate(XmlNode resourceNode)
        {
            Name = resourceNode.StringFromNode("Name");
            Consumable = true;
            ResourceType = resourceNode.StringFromNode("Type");
            AllResources.Add(this);
            switch (ResourceType)
            {
                case "Water":
                    Water.Add(this);
                    break;
                case "Plant":
                    Plant.Add(this);
                    break;
                case "Meat":
                    Meat.Add(this);
                    break;
                case "Resource":
                    Resources.Add(this);
                    Consumable = false;
                    break;
            }

            if (_lastType != ResourceType)
            {
                OasisDRCur = 0f;
                SteppeDRCur = 0f;
                RuinsDRCur = 0f;
                DefilesDRCur = 0f;
                WastelandDRCur = 0f;
            }

            _lastType = ResourceType;

            float oasisDr = resourceNode.FloatFromNode("OasisDropRate");
            float steppeDr = resourceNode.FloatFromNode("SteppeDropRate");
            float ruinsDr = resourceNode.FloatFromNode("RuinsDropRate");
            float defilesDr = resourceNode.FloatFromNode("DefilesDropRate");
            float wastelandDr = resourceNode.FloatFromNode("WastelandDropRate");
            _dropRates.Add(EnvironmentType.Desert, new DropRate(ref OasisDRCur, oasisDr));
            _dropRates.Add(EnvironmentType.Mountains, new DropRate(ref SteppeDRCur, steppeDr));
            _dropRates.Add(EnvironmentType.Ruins, new DropRate(ref RuinsDRCur, ruinsDr));
            _dropRates.Add(EnvironmentType.Sea, new DropRate(ref DefilesDRCur, defilesDr));
            _dropRates.Add(EnvironmentType.Wasteland, new DropRate(ref WastelandDRCur, wastelandDr));
            SetEffect(resourceNode);
        }

        public static InventoryItem GetMeat()
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
            foreach (ResourceTemplate plantTemplate in Plant)
            {
                if (!plantTemplate._dropRates[currentEnvironment].ValueWithinRange(rand)) continue;
                return plantTemplate;
            }

            throw new Exception("Can't have invalid Plant!");
        }

        public InventoryItem Create()
        {
            InventoryItem item = Consumable ? new Consumable(this) : new InventoryItem(this, GameObjectType.Resource);
            item.SetStackable(true);
            return item;
        }

        public static InventoryItem Create(string name)
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