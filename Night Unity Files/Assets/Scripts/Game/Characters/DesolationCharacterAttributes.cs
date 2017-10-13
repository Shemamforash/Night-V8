using System;
using System.Xml;
using Facilitating.Persistence;
using Game.World;
using Game.World.WorldEvents;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI.CustomTypes;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Characters
{
    public class DesolationCharacterAttributes : AttributeContainer
    {
        private int[] _toleranceThresholds = { 0, 10, 25, 50, 75};
        private readonly string[] _dehydrationLevels = {"Slaked", "Quenched", "Thirsty", "Aching", "Parched"};
        private readonly string[] _starvationLevels = {"Full", "Sated", "Hungry", "Ravenous", "Starving"};
        
        public readonly IntAttribute Strength = new IntAttribute(AttributeType.Strength, Random.Range(30, 70));
        public readonly IntAttribute Intelligence = new IntAttribute(AttributeType.Intelligence, Random.Range(30, 70));
        public readonly IntAttribute Endurance = new IntAttribute(AttributeType.Endurance, Random.Range(30, 70));
        public readonly IntAttribute Stability = new IntAttribute(AttributeType.Stability, Random.Range(30, 70));
        public readonly IntAttribute Starvation = new IntAttribute(AttributeType.Starvation, 0, 0, 50);

        public readonly IntAttribute Dehydration = new IntAttribute(AttributeType.Dehydration, 0, 0, 50);

        /*instead of consuming x food or water every minutes, consume 1 food or water every x minutes
        use the max value of the hunger and thirst to keep track of the interval at which eating or drinking should occur
        consume 1 food or water whenever the current value reaches the max value, then reset
        this allows the duration to easily change depending on temperature, modifiers, etc.
        */
        public readonly IntAttribute Hunger = new IntAttribute(AttributeType.Hunger, 0, 0, 12);

        public readonly IntAttribute Thirst = new IntAttribute(AttributeType.Thirst, 0, 0, 12);
        public WeightCategory Weight;
        private readonly DesolationCharacter _character;
        private bool _starving, _dehydrated;

        public DesolationCharacterAttributes(DesolationCharacter character)
        {
            _character = character;
            WorldState.Instance().HourEvent += Fatigue;
            WorldState.Instance().MinuteEvent += UpdateThirstAndHunger;
            AddAttribute(Strength);
            AddAttribute(Intelligence);
            AddAttribute(Endurance);
            AddAttribute(Stability);
            AddAttribute(Starvation);
            AddAttribute(Dehydration);
            AddAttribute(Hunger);
            AddAttribute(Thirst);
            SetConsumptionEvents(Hunger, Starvation, WorldState.HomeInventory.GetResource(InventoryResourceType.Food));
            SetConsumptionEvents(Thirst, Dehydration, WorldState.HomeInventory.GetResource(InventoryResourceType.Water));
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
        
        private void SetConsumptionEvents(IntAttribute need, IntAttribute tolerance, InventoryResource resource)
        {
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

        private void BindUiToAttribute(IntAttribute a, TextMeshProUGUI simpleText, TextMeshProUGUI detailText)
        {
            a.AddOnValueChange(delegate(MyValue<int> f)
            {
                int calculatedValue = (int)((IntAttribute) f).GetCalculatedValue();
                simpleText.text = calculatedValue.ToString();
                detailText.text = calculatedValue + "/" + a.Max;
            });
        }
        
        public void BindUi()
        {
            CharacterUiDetailed characterUiDetailed = _character.CharacterUiDetailed;
            BindUiToAttribute(Strength, characterUiDetailed.StrengthText, characterUiDetailed.StrengthTextDetail);
            BindUiToAttribute(Endurance, characterUiDetailed.EnduranceText, characterUiDetailed.EnduranceTextDetail);
            BindUiToAttribute(Stability, characterUiDetailed.StabilityText, characterUiDetailed.StabilityTextDetail);
            BindUiToAttribute(Intelligence, characterUiDetailed.IntelligenceText, characterUiDetailed.IntelligenceTextDetail);
            Hunger.AddOnValueChange(f => characterUiDetailed.HungerText.text = GetHungerStatus());
            Thirst.AddOnValueChange(f => characterUiDetailed.ThirstText.text = GetThirstStatus());
        }

        private void UpdateThirstAndHunger()
        {
            Thirst.Max = (int) (-0.2f * WorldState.EnvironmentManager.GetTemperature() + 16f);
            UpdateConsumableTolerance(Hunger, Starvation, Eat, _character.GetCondition(ConditionType.Hunger));
            UpdateConsumableTolerance(Thirst, Dehydration, Drink, _character.GetCondition(ConditionType.Thirst));
        }

        private void UpdateConsumableTolerance(MyInt requirement, MyInt tolerance, Action consume, Condition condition)
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

        public string GetAttributeStatus(IntAttribute intAttribute, string[] levels)
        {
            float tolerancePercentage = intAttribute.AsPercent();
            for(int i = 1; i < _toleranceThresholds.Length; ++i)
            {
                int threshold = _toleranceThresholds[i];
                if (tolerancePercentage <= threshold)
                {
                    return levels[i - 1];
                }
            }
            return levels[_toleranceThresholds.Length];
        }

        private string GetHungerStatus()
        {
            return GetAttributeStatus(Starvation, _starvationLevels);
        }

        private string GetThirstStatus()
        {
            return GetAttributeStatus(Dehydration, _dehydrationLevels);
        }

        private void Drink()
        {
            if (Dehydration.GetCurrentValue() == 0) return;
            int consumed = WorldState.Home().DecrementResource(InventoryResourceType.Water, 1);
            Dehydration.SetCurrentValue(Dehydration.GetCurrentValue() - consumed);
        }

        private void Eat()
        {
            if (Starvation.GetCurrentValue() == 0) return;
            int consumed = WorldState.Home().DecrementResource(InventoryResourceType.Food, 1);
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

        private void LoadAttribute(XmlNode root, string attributeName, IntAttribute intAttribute)
        {
            XmlNode attributeNode = root.SelectSingleNode(attributeName);
            XmlNode maxNode = attributeNode.SelectSingleNode("Max");
            intAttribute.Max = SaveController.ParseIntFromSubNode(maxNode);
            XmlNode valNode = attributeNode.SelectSingleNode("Val");
            intAttribute.SetCurrentValue(SaveController.ParseIntFromSubNode(valNode));
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

        private void SaveAttribute(XmlNode root, string attributeName, IntAttribute intAttribute)
        {
            XmlNode attributeNode = SaveController.CreateNodeAndAppend(attributeName, root);
            SaveController.CreateNodeAndAppend("Val", attributeNode, intAttribute.Max);
            SaveController.CreateNodeAndAppend("Max", attributeNode, intAttribute.GetCurrentValue());
        }
    }
}