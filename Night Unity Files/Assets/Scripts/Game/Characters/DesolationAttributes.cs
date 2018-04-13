using System;
using System.Xml;
using Facilitating.Persistence;
using Game.Exploration.Environment;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Input;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Characters
{
    public class DesolationAttributes : AttributeContainer, IPersistenceTemplate
    {
        private readonly string[] _dehydrationLevels = {"Slaked", "Quenched", "Thirsty", "Aching", "Parched"};
        private readonly string[] _starvationLevels = {"Full", "Sated", "Hungry", "Ravenous", "Starving"};
        private readonly float[] _toleranceThresholds = {0, 0.1f, 0.25f, 0.5f, 0.75f};
        private readonly Player _player;
        
        public readonly CharacterAttribute Endurance = new CharacterAttribute(AttributeType.Endurance, 0);
        public readonly CharacterAttribute Perception = new CharacterAttribute(AttributeType.Perception, 0);
        public readonly CharacterAttribute Strength = new CharacterAttribute(AttributeType.Strength, 0);
        public readonly CharacterAttribute Willpower = new CharacterAttribute(AttributeType.Willpower, 0);

        /*instead of consuming x food or water every minutes, consume 1 food or water every x minutes
        use the max value of the hunger and thirst to keep track of the interval at which eating or drinking should occur
        consume 1 food or water whenever the current value reaches the max value, then reset
        this allows the duration to easily change depending on temperature, modifiers, etc.
        */

        public readonly CharacterAttribute Hunger = new CharacterAttribute(AttributeType.Hunger, 0, 0, 24);
        public readonly CharacterAttribute Starvation = new CharacterAttribute(AttributeType.Starvation, 0, 0, 50);
        public readonly CharacterAttribute Thirst = new CharacterAttribute(AttributeType.Thirst, 0, 0, 12);
        public readonly CharacterAttribute Dehydration = new CharacterAttribute(AttributeType.Dehydration, 0, 0, 50);

        public DesolationAttributes(Player player)
        {
            _player = player;
        }

        public float GetSkillRechargeModifier()
        {
            return Mathf.Pow(0.95f, Willpower.CurrentValue());
        }

        public float GetGunDamageModifier()
        {
            return Mathf.Pow(1.05f, Perception.CurrentValue());
        }

        protected override void CacheAttributes()
        {
            AddAttribute(Strength);
            AddAttribute(Perception);
            AddAttribute(Endurance);
            AddAttribute(Willpower);
            AddAttribute(Starvation);
            AddAttribute(Dehydration);
            AddAttribute(Hunger);
            AddAttribute(Thirst);
        }

        public float RemainingCarryCapacity()
        {
            return Strength.CurrentValue();
        }

        public void Fatigue()
        {
            float starvationLevel = Starvation.Normalised();
            float dehydrationLevel = Dehydration.Normalised();
            if (starvationLevel >= _toleranceThresholds[4] || dehydrationLevel >= _toleranceThresholds[4])
                Perception.SetCurrentValue(Perception.CurrentValue() - 2);
            else if (starvationLevel >= _toleranceThresholds[3] || dehydrationLevel >= _toleranceThresholds[3])
                Perception.SetCurrentValue(Perception.CurrentValue() - 1);
            else
                Perception.SetCurrentValue(Perception.CurrentValue() + 1);
        }

        private void UpdateNeed(CharacterAttribute need, CharacterAttribute tolerance, InventoryResourceType targetResource)
        {
            need.Increment();
            if (!need.ReachedMax()) return;
            if (tolerance.ReachedMax()) return;
            need.SetCurrentValue(need.Min);
            int amountToConsume = Endurance.ReachedMin() ? 2 : 1;
            bool consumed = _player.ConsumeResource(targetResource, amountToConsume);
            if (consumed) tolerance.Decrement();
            else tolerance.Increment();
            if (tolerance.ReachedMax()) _player.Kill();
        }

        public void UpdateThirstAndHunger()
        {
            Thirst.Max = (int) (-0.2f * EnvironmentManager.GetTemperature() + 16f);
            UpdateNeed(Thirst, Dehydration, InventoryResourceType.Water);
            UpdateNeed(Hunger, Starvation, InventoryResourceType.Food);
        }

        private string GetAttributeStatus(CharacterAttribute characterAttribute, string[] levels)
        {
            float tolerancePercentage = characterAttribute.Normalised();
            for (int i = 1; i < _toleranceThresholds.Length; ++i)
            {
                float threshold = _toleranceThresholds[i];
                if (tolerancePercentage <= threshold) return levels[i - 1];
            }

            return levels[_toleranceThresholds.Length - 1];
        }

        public string GetHungerStatus()
        {
            return GetAttributeStatus(Starvation, _starvationLevels);
        }

        public string GetThirstStatus()
        {
            return GetAttributeStatus(Dehydration, _dehydrationLevels);
        }

        public void Load(XmlNode doc, PersistenceType saveType)
        {
            LoadAttribute(doc, nameof(Strength), Strength);
            LoadAttribute(doc, nameof(Endurance), Endurance);
            LoadAttribute(doc, nameof(Willpower), Willpower);
            LoadAttribute(doc, nameof(Perception), Perception);

            LoadAttribute(doc, nameof(Starvation), Starvation);
            LoadAttribute(doc, nameof(Dehydration), Dehydration);
            LoadAttribute(doc, nameof(Hunger), Hunger);
            LoadAttribute(doc, nameof(Thirst), Thirst);
        }

        public XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            SaveAttribute(doc, nameof(Strength), Strength);
            SaveAttribute(doc, nameof(Endurance), Endurance);
            SaveAttribute(doc, nameof(Willpower), Willpower);
            SaveAttribute(doc, nameof(Perception), Perception);

            SaveAttribute(doc, nameof(Starvation), Starvation);
            SaveAttribute(doc, nameof(Dehydration), Dehydration);
            SaveAttribute(doc, nameof(Hunger), Hunger);
            SaveAttribute(doc, nameof(Thirst), Thirst);
            return doc;
        }

        private void LoadAttribute(XmlNode root, string attributeName, CharacterAttribute characterAttribute)
        {
            XmlNode attributeNode = root.SelectSingleNode(attributeName);
            XmlNode maxNode = attributeNode.SelectSingleNode("Max");
            characterAttribute.Max = SaveController.ParseIntFromSubNode(maxNode);
            XmlNode valNode = attributeNode.SelectSingleNode("Val");
            characterAttribute.SetCurrentValue(SaveController.ParseIntFromSubNode(valNode));
        }

        private void SaveAttribute(XmlNode root, string attributeName, CharacterAttribute characterAttribute)
        {
            SaveController.CreateNodeAndAppend(attributeName, root, characterAttribute.CurrentValue() + "/" + characterAttribute.Max);
        }
    }
}