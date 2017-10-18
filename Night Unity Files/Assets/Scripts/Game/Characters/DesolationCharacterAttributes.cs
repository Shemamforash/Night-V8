using System;
using System.Xml;
using Facilitating.Persistence;
using Game.World;
using Game.World.WorldEvents;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Characters
{
    public class DesolationCharacterAttributes : AttributeContainer
    {
        private int[] _toleranceThresholds = {0, 10, 25, 50, 75};
        private readonly string[] _dehydrationLevels = {"Slaked", "Quenched", "Thirsty", "Aching", "Parched"};
        private readonly string[] _starvationLevels = {"Full", "Sated", "Hungry", "Ravenous", "Starving"};

        public readonly CharacterAttribute Strength = new CharacterAttribute(AttributeType.Strength, Random.Range(30, 70));
        public readonly CharacterAttribute Intelligence = new CharacterAttribute(AttributeType.Intelligence, Random.Range(30, 70));
        public readonly CharacterAttribute Endurance = new CharacterAttribute(AttributeType.Endurance, Random.Range(30, 70));
        public readonly CharacterAttribute Stability = new CharacterAttribute(AttributeType.Stability, Random.Range(30, 70));
        public readonly CharacterAttribute Starvation = new CharacterAttribute(AttributeType.Starvation, 0, 0, 50);

        public readonly CharacterAttribute Dehydration = new CharacterAttribute(AttributeType.Dehydration, 0, 0, 50);

        /*instead of consuming x food or water every minutes, consume 1 food or water every x minutes
        use the max value of the hunger and thirst to keep track of the interval at which eating or drinking should occur
        consume 1 food or water whenever the current value reaches the max value, then reset
        this allows the duration to easily change depending on temperature, modifiers, etc.
        */
        public readonly CharacterAttribute Hunger = new CharacterAttribute(AttributeType.Hunger, 0, 0, 12);

        public readonly CharacterAttribute Thirst = new CharacterAttribute(AttributeType.Thirst, 0, 0, 12);
        public WeightCategory Weight;
        private readonly DesolationCharacter _character;
        private bool _starving, _dehydrated;

        private void RegisterTimedEvents()
        {
            WorldState.RegisterHourEvent(Fatigue);
            WorldState.RegisterMinuteEvent(UpdateThirstAndHunger);
            SetConsumptionEvents(Hunger, Starvation, InventoryResourceType.Food);
            SetConsumptionEvents(Thirst, Dehydration, InventoryResourceType.Water);
        }

        protected override void CacheAttributes()
        {
            AddAttribute(Strength);
            AddAttribute(Intelligence);
            AddAttribute(Endurance);
            AddAttribute(Stability);
            AddAttribute(Starvation);
            AddAttribute(Dehydration);
            AddAttribute(Hunger);
            AddAttribute(Thirst);
        }
        
        public DesolationCharacterAttributes(DesolationCharacter character)
        {
            _character = character;
            RegisterTimedEvents();
        }

        private Intensity GetIntensity(float percent)
        {
            for (int i = _toleranceThresholds.Length - 1; i >= 0; --i)
            {
                int threshold = _toleranceThresholds[i];
                if (!(percent > threshold)) continue;
                foreach (Intensity intensity in Enum.GetValues(typeof(Intensity)))
                {
                    if (intensity == (Intensity) i)
                    {
                        return intensity;
                    }
                }
                break;
            }
            return Intensity.None;
        }

        private void SetConsumptionEvents(CharacterAttribute need, CharacterAttribute tolerance, InventoryResourceType resourceType)
        {
            InventoryResource resource = WorldState.HomeInventory().GetResource(resourceType);
            if (resource == null) return;
            need.OnMax(() =>
            {
                if (resource.Decrement(1) != 1)
                {
                    tolerance.SetCurrentValue(tolerance.GetCurrentValue() + 1);
                }
                need.SetCurrentValue(0);
            });
            tolerance.OnMax(_character.Kill);
        }

        private void UpdateThirstAndHunger()
        {
            Thirst.Max = (int) (-0.2f * WorldState.EnvironmentManager.GetTemperature() + 16f);
            UpdateConsumableTolerance(Hunger, Starvation, Eat, _character.GetCondition(ConditionType.Hunger));
            UpdateConsumableTolerance(Thirst, Dehydration, Drink, _character.GetCondition(ConditionType.Thirst));
        }

        private void UpdateConsumableTolerance(MyValue requirement, MyValue tolerance, Action consume, Condition condition)
        {
            float previousTolerance = tolerance.AsPercent();
            Intensity previousIntensity = GetIntensity(previousTolerance);
            requirement.SetCurrentValue(requirement.GetCurrentValue() + 1);
            float tolerancePercentage = tolerance.AsPercent();
            Intensity currentIntensity = GetIntensity(tolerancePercentage);
            if (previousIntensity != currentIntensity)
            {
                condition.SetConditionLevel(currentIntensity);
            }
            consume();
        }

        private void Fatigue()
        {
            float starvationLevel = Starvation.AsPercent();
            float dehydrationLevel = Dehydration.AsPercent();
            if (starvationLevel >= _toleranceThresholds[4] || dehydrationLevel >= _toleranceThresholds[4])
            {
                Intelligence.SetCurrentValue(Intelligence.GetCurrentValue() - 2);
            }
            else if (starvationLevel >= _toleranceThresholds[3] || dehydrationLevel >= _toleranceThresholds[3])
            {
                Intelligence.SetCurrentValue(Intelligence.GetCurrentValue() - 1);
            }
            else
            {
                Intelligence.SetCurrentValue(Intelligence.GetCurrentValue() + 1);
            }
        }

        public string GetAttributeStatus(CharacterAttribute characterAttribute, string[] levels)
        {
            float tolerancePercentage = characterAttribute.AsPercent();
            for (int i = 1; i < _toleranceThresholds.Length; ++i)
            {
                int threshold = _toleranceThresholds[i];
                if (tolerancePercentage <= threshold)
                {
                    return levels[i - 1];
                }
            }
            return levels[_toleranceThresholds.Length];
        }

        public string GetHungerStatus()
        {
            return GetAttributeStatus(Starvation, _starvationLevels);
        }

        public string GetThirstStatus()
        {
            return GetAttributeStatus(Dehydration, _dehydrationLevels);
        }

        private void Drink()
        {
            if (Dehydration.GetCurrentValue() == 0) return;
            float consumed = WorldState.HomeInventory().DecrementResource(InventoryResourceType.Water, 1);
            Dehydration.SetCurrentValue(Dehydration.GetCurrentValue() - consumed);
        }

        private void Eat()
        {
            if (Starvation.GetCurrentValue() == 0) return;
            float consumed = WorldState.HomeInventory().DecrementResource(InventoryResourceType.Food, 1);
            Starvation.SetCurrentValue(Starvation.GetCurrentValue() - consumed);
        }

        public float RemainingCarryCapacity()
        {
            return Strength.GetCurrentValue();
        }

        public void Load(XmlNode doc, PersistenceType saveType)
        {
            LoadAttribute(doc, nameof(Strength), Strength);
            LoadAttribute(doc, nameof(Endurance), Endurance);
            LoadAttribute(doc, nameof(Stability), Stability);
            LoadAttribute(doc, nameof(Intelligence), Intelligence);

            LoadAttribute(doc, nameof(Starvation), Starvation);
            LoadAttribute(doc, nameof(Dehydration), Dehydration);
            LoadAttribute(doc, nameof(Hunger), Hunger);
            LoadAttribute(doc, nameof(Thirst), Thirst);

            XmlNode weightNode = doc.SelectSingleNode("Weight");
            Weight = (WeightCategory) SaveController.ParseIntFromSubNode(weightNode);
        }

        private void LoadAttribute(XmlNode root, string attributeName, CharacterAttribute characterAttribute)
        {
            XmlNode attributeNode = root.SelectSingleNode(attributeName);
            XmlNode maxNode = attributeNode.SelectSingleNode("Max");
            characterAttribute.Max = SaveController.ParseIntFromSubNode(maxNode);
            XmlNode valNode = attributeNode.SelectSingleNode("Val");
            characterAttribute.SetCurrentValue(SaveController.ParseIntFromSubNode(valNode));
        }

        public void Save(XmlNode doc, PersistenceType saveType)
        {
            SaveAttribute(doc, nameof(Strength), Strength);
            SaveAttribute(doc, nameof(Endurance), Endurance);
            SaveAttribute(doc, nameof(Stability), Stability);
            SaveAttribute(doc, nameof(Intelligence), Intelligence);

            SaveAttribute(doc, nameof(Starvation), Starvation);
            SaveAttribute(doc, nameof(Dehydration), Dehydration);
            SaveAttribute(doc, nameof(Hunger), Hunger);
            SaveAttribute(doc, nameof(Thirst), Thirst);

            SaveController.CreateNodeAndAppend("Weight", doc, (int) Weight);
        }

        private void SaveAttribute(XmlNode root, string attributeName, CharacterAttribute characterAttribute)
        {
            XmlNode attributeNode = SaveController.CreateNodeAndAppend(attributeName, root);
            SaveController.CreateNodeAndAppend("Val", attributeNode, characterAttribute.Max);
            SaveController.CreateNodeAndAppend("Max", attributeNode, characterAttribute.GetCurrentValue());
        }
    }
}