using System;
using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using Game.Gear;
using Game.World;
using Game.World.Time;
using Game.World.WorldEvents;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI.CustomTypes;
using Attribute = Game.Gear.Attribute;
using Random = UnityEngine.Random;

namespace Game.Characters
{
    public class CharacterAttributes : AttributeContainer
    {
        private const int ToleranceThreshold1 = 10, ToleranceThreshold2 = 25, ToleranceThreshold3 = 50, ToleranceThreshold4 = 75;
        private readonly string[] _dehydrationLevels = {"Slaked", "Quenched", "Thirsty", "Aching", "Parched"};
        private readonly string[] _starvationLevels = {"Full", "Sated", "Hungry", "Ravenous", "Starving"};

        public readonly MyInt Strength = new MyInt(Random.Range(30, 70));
        public readonly MyInt Intelligence = new MyInt(Random.Range(30, 70));
        public readonly MyInt Endurance = new MyInt(Random.Range(30, 70));
        public readonly MyInt Stability = new MyInt(Random.Range(30, 70));
        public readonly MyInt Starvation = new MyInt(0, 0, 50);

        public readonly MyInt Dehydration = new MyInt(0, 0, 30);

        /*instead of consuming x food or water every minutes, consume 1 food or water every x minutes
        use the max value of the hunger and thirst to keep track of the interval at which eating or drinking should occur
        consume 1 food or water whenever the current value reaches the max value, then reset
        this allows the duration to easily change depending on temperature, modifiers, etc.
        */
        public readonly MyInt Hunger = new MyInt(0, 0, 12);

        public readonly MyInt Thirst = new MyInt(0, 0, 12);
        public WeightCategory Weight;
        private readonly DesolationCharacter _character;
        private bool _starving, _dehydrated;

        public CharacterAttributes(DesolationCharacter character)
        {
            _character = character;
            WorldTime.Instance().HourEvent += Fatigue;
            WorldTime.Instance().MinuteEvent += UpdateThirstAndHunger;
        }

        private void UpdateThirstAndHunger()
        {
            Thirst.Max = (int) (-0.2f * WorldState.EnvironmentManager.GetTemperature() + 16f);
            UpdateConsumableTolerance(Hunger, Starvation, _character.Eat);
            UpdateConsumableTolerance(Thirst, Dehydration, _character.Drink);
        }

        private void UpdateConsumableTolerance(MyInt requirement, MyInt tolerance, Action consume)
        {
            float previousTolerance = GetToleranceAsPercentage(tolerance);
            ++requirement.Val;
            if (requirement.ReachedMax())
            {
                ++tolerance.Val;
                requirement.Val = 0;
            }
            float tolerancePercentage = GetToleranceAsPercentage(tolerance);
            if (tolerance.ReachedMax())
            {
                _character.Kill();
            }
            else if (tolerancePercentage >= ToleranceThreshold4)
            {
                if (previousTolerance < ToleranceThreshold4)
                {
                    WorldEventManager.GenerateEvent(new WorldEvent(_character.CharacterName + " is dying of thirst"));
                }
                consume();
            }
        }

        private void Fatigue()
        {
            float starvationLevel = GetToleranceAsPercentage(Starvation);
            float dehydrationLevel = GetToleranceAsPercentage(Dehydration);
            if (starvationLevel >= ToleranceThreshold4 || dehydrationLevel >= ToleranceThreshold4)
            {
                Intelligence.Val -= 2;
            }
            else if (starvationLevel >= ToleranceThreshold3 || dehydrationLevel >= ToleranceThreshold3)
            {
                Intelligence.Val -= 1;
            }
            else
            {
                Intelligence.Val += 1;
            }
        }

        private float GetToleranceAsPercentage(MyInt attribute)
        {
            return 100f / attribute.Max * attribute.Val;
        }

        public string GetAttributeStatus(MyInt attribute, string[] levels)
        {
            float tolerancePercentage = GetToleranceAsPercentage(attribute);
            if (tolerancePercentage <= ToleranceThreshold1)
            {
                return levels[0];
            }
            if (tolerancePercentage <= ToleranceThreshold2)
            {
                return levels[1];
            }
            if (tolerancePercentage <= ToleranceThreshold3)
            {
                return levels[2];
            }
            if (tolerancePercentage <= ToleranceThreshold4)
            {
                return levels[3];
            }
            return levels[4];
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
            LoadAttribute(doc, nameof(Stability), Stability);
            LoadAttribute(doc, nameof(Intelligence), Intelligence);

            LoadAttribute(doc, nameof(Starvation), Starvation);
            LoadAttribute(doc, nameof(Dehydration), Dehydration);
            LoadAttribute(doc, nameof(Hunger), Hunger);
            LoadAttribute(doc, nameof(Thirst), Thirst);

            XmlNode weightNode = doc.SelectSingleNode("Weight");
            Weight = (WeightCategory) SaveController.ParseIntFromSubNode(weightNode);
        }

        private void LoadAttribute(XmlNode root, string attributeName, MyInt attribute)
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

        private void SaveAttribute(XmlNode root, string attributeName, MyInt attribute)
        {
            XmlNode attributeNode = SaveController.CreateNodeAndAppend(attributeName, root);
            SaveController.CreateNodeAndAppend("Val", attributeNode, attribute.Max);
            SaveController.CreateNodeAndAppend("Max", attributeNode, attribute.Val);
        }
    }
}