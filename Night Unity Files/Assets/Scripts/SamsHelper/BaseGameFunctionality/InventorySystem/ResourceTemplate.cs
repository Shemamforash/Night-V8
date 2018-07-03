using System;
using System.Collections.Generic;
using Game.Characters;
using Game.Exploration.Environment;
using SamsHelper.BaseGameFunctionality.Basic;
using Random = UnityEngine.Random;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class ResourceTemplate
    {
        public readonly string Name, ResourceType;
        public float Duration;
        private readonly bool Consumable;
        public AttributeType AttributeType;
        public float ModifierVal;
        private bool _additive;
        private bool _hasEffect;
        private static readonly List<ResourceTemplate> Meat = new List<ResourceTemplate>();
        private static readonly List<ResourceTemplate> Water = new List<ResourceTemplate>();
        private static readonly List<ResourceTemplate> Plant = new List<ResourceTemplate>();
        private static readonly List<ResourceTemplate> Resources = new List<ResourceTemplate>();
        public static readonly List<ResourceTemplate> AllResources = new List<ResourceTemplate>();
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

        private static string _lastType = "";

        public ResourceTemplate(string name, string type, float oasisDr, float steppeDr, float ruinsDr, float defilesDr, float wastelandDr) : this(name, type)
        {
            if (_lastType != type)
            {
                OasisDRCur = 0f;
                SteppeDRCur = 0f;
                RuinsDRCur = 0f;
                DefilesDRCur = 0f;
                WastelandDRCur = 0f;
            }

            _lastType = type;
            Consumable = true;
            _dropRates.Add(EnvironmentType.Oasis, new DropRate(ref OasisDRCur, oasisDr));
            _dropRates.Add(EnvironmentType.Steppe, new DropRate(ref SteppeDRCur, steppeDr));
            _dropRates.Add(EnvironmentType.Ruins, new DropRate(ref RuinsDRCur, ruinsDr));
            _dropRates.Add(EnvironmentType.Defiles, new DropRate(ref DefilesDRCur, defilesDr));
            _dropRates.Add(EnvironmentType.Wasteland, new DropRate(ref WastelandDRCur, wastelandDr));
        }

        public ResourceTemplate(string name, string type)
        {
            Name = name;
            ResourceType = type;
            switch (type)
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
                    break;
            }

            AllResources.Add(this);
        }

        public static ResourceTemplate GetMeat()
        {
            float rand = Random.Range(0f, 1f);
            EnvironmentType currentEnvironment = EnvironmentManager.CurrentEnvironment.EnvironmentType;
            foreach (ResourceTemplate meatTemplate in Meat)
            {
                if (!meatTemplate._dropRates[currentEnvironment].ValueWithinRange(rand)) continue;
                return meatTemplate;
            }

            throw new Exception("Can't have invalid meat!");
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

        public void SetEffect(AttributeType attribute, float modifierVal, bool additive, int duration)
        {
            AttributeType = attribute;
            ModifierVal = modifierVal;
            _additive = additive;
            Duration = duration;
            _hasEffect = true;
        }

        public AttributeModifier CreateModifier()
        {
            if (!_hasEffect) return null;
            AttributeModifier attributeModifier = new AttributeModifier();
            if (_additive)
                attributeModifier.SetRawBonus(ModifierVal);
            else
                attributeModifier.SetFinalBonus(ModifierVal);
            return attributeModifier;
        }
    }
}