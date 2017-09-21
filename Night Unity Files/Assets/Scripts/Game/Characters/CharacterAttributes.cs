using System;
using System.Collections.Generic;
using System.Xml;
using Characters;
using Facilitating.Persistence;
using Game.Gear;
using Game.World;
using Game.World.Time;
using Game.World.WorldEvents;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI.CustomTypes;
using TMPro;
using Attribute = SamsHelper.BaseGameFunctionality.Basic.Attribute;
using Random = UnityEngine.Random;

namespace Game.Characters
{
    public class CharacterAttributes : AttributeContainer
    {
        private int[] _toleranceThresholds = { 0, 10, 25, 50, 75};
        private readonly string[] _dehydrationLevels = {"Slaked", "Quenched", "Thirsty", "Aching", "Parched"};
        private readonly string[] _starvationLevels = {"Full", "Sated", "Hungry", "Ravenous", "Starving"};

        public readonly Attribute Strength = new Attribute(AttributeType.Strength, Random.Range(30, 70));
        public readonly Attribute Intelligence = new Attribute(AttributeType.Intelligence, Random.Range(30, 70));
        public readonly Attribute Endurance = new Attribute(AttributeType.Endurance, Random.Range(30, 70));
        public readonly Attribute Stability = new Attribute(AttributeType.Stability, Random.Range(30, 70));
        public readonly Attribute Starvation = new Attribute(AttributeType.Starvation, 0, 0, 50);

        public readonly Attribute Dehydration = new Attribute(AttributeType.Dehydration, 0, 0, 50);

        /*instead of consuming x food or water every minutes, consume 1 food or water every x minutes
        use the max value of the hunger and thirst to keep track of the interval at which eating or drinking should occur
        consume 1 food or water whenever the current value reaches the max value, then reset
        this allows the duration to easily change depending on temperature, modifiers, etc.
        */
        public readonly Attribute Hunger = new Attribute(AttributeType.Hunger, 0, 0, 12);

        public readonly Attribute Thirst = new Attribute(AttributeType.Thirst, 0, 0, 12);
        public WeightCategory Weight;
        private readonly DesolationCharacter _character;
        private bool _starving, _dehydrated;

        public CharacterAttributes(DesolationCharacter character)
        {
            _character = character;
            WorldTime.Instance().HourEvent += Fatigue;
            WorldTime.Instance().MinuteEvent += UpdateThirstAndHunger;
            AddAttribute(Strength);
            AddAttribute(Intelligence);
            AddAttribute(Endurance);
            AddAttribute(Stability);
            AddAttribute(Starvation);
            AddAttribute(Dehydration);
            AddAttribute(Hunger);
            AddAttribute(Thirst);
            SetConsumptionEvents(Hunger, Starvation);
            SetConsumptionEvents(Thirst, Dehydration);
        }

        private void SetConsumptionEvents(Attribute need, Attribute tolerance)
        {
            need.OnMax(() =>
            {
                ++tolerance.Val;
                need.Val = 0;
            });
            tolerance.OnMax(_character.Kill);
        }

        private void BindUiToAttribute(Attribute a, TextMeshProUGUI simpleText, TextMeshProUGUI detailText)
        {
            a.AddOnValueChange(delegate(int f)
            {
                simpleText.text = f.ToString();
                detailText.text = f + "/" + a.Max;
            });
        }
        
        public void BindUi()
        {
            CharacterUI characterUi = _character.CharacterUi;
            characterUi.EatButton.onClick.AddListener(Eat);
            characterUi.DrinkButton.onClick.AddListener(Drink);
            BindUiToAttribute(Strength, characterUi.StrengthText, characterUi.StrengthTextDetail);
            BindUiToAttribute(Endurance, characterUi.EnduranceText, characterUi.EnduranceTextDetail);
            BindUiToAttribute(Stability, characterUi.StabilityText, characterUi.StabilityTextDetail);
            BindUiToAttribute(Intelligence, characterUi.IntelligenceText, characterUi.IntelligenceTextDetail);
            Hunger.AddOnValueChange(f => characterUi.HungerText.text = GetHungerStatus());
            Thirst.AddOnValueChange(f => characterUi.ThirstText.text = GetThirstStatus());
        }

        private void UpdateThirstAndHunger()
        {
            Thirst.Max = (int) (-0.2f * WorldState.EnvironmentManager.GetTemperature() + 16f);
            UpdateConsumableTolerance(Hunger, Starvation, Eat);
            UpdateConsumableTolerance(Thirst, Dehydration, Drink);
        }

        private void UpdateConsumableTolerance(MyInt requirement, MyInt tolerance, Action consume)
        {
            float previousTolerance = tolerance.AsPercent();
            ++requirement.Val;
            float tolerancePercentage = tolerance.AsPercent();
            if (!(tolerancePercentage >= _toleranceThresholds[4])) return;
            if (previousTolerance < _toleranceThresholds[4])
            {
                WorldEventManager.GenerateEvent(new WorldEvent(_character.Name + " is dying of thirst"));
            }
            consume();
        }

        private void Fatigue()
        {
            float starvationLevel = Starvation.AsPercent();
            float dehydrationLevel = Dehydration.AsPercent();
            if (starvationLevel >= _toleranceThresholds[4] || dehydrationLevel >= _toleranceThresholds[4])
            {
                Intelligence.Val -= 2;
            }
            else if (starvationLevel >= _toleranceThresholds[3] || dehydrationLevel >= _toleranceThresholds[3])
            {
                Intelligence.Val -= 1;
            }
            else
            {
                Intelligence.Val += 1;
            }
        }

        public string GetAttributeStatus(Attribute attribute, string[] levels)
        {
            float tolerancePercentage = attribute.AsPercent();
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

        public string GetHungerStatus()
        {
            return GetAttributeStatus(Starvation, _starvationLevels);
        }

        public string GetThirstStatus()
        {
            return GetAttributeStatus(Dehydration, _dehydrationLevels);
        }
        
        public void Drink()
        {
            int consumed = WorldState.Inventory().DecrementResource("Water", 1);
            Dehydration.Val -= consumed;
        }

        public void Eat()
        {
            int consumed = WorldState.Inventory().DecrementResource("Food", 1);
            Starvation.Val -= consumed;
        }

        public float RemainingCarryCapacity()
        {
            return Strength.Val;
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

        private void LoadAttribute(XmlNode root, string attributeName, Attribute attribute)
        {
            XmlNode attributeNode = root.SelectSingleNode(attributeName);
            XmlNode maxNode = attributeNode.SelectSingleNode("Max");
            attribute.Max = SaveController.ParseIntFromSubNode(maxNode);
            XmlNode valNode = attributeNode.SelectSingleNode("Val");
            attribute.Val = SaveController.ParseIntFromSubNode(valNode);
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

        private void SaveAttribute(XmlNode root, string attributeName, Attribute attribute)
        {
            XmlNode attributeNode = SaveController.CreateNodeAndAppend(attributeName, root);
            SaveController.CreateNodeAndAppend("Val", attributeNode, attribute.Max);
            SaveController.CreateNodeAndAppend("Max", attributeNode, attribute.Val);
        }
    }
}