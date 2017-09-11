using System;
using System.Xml;
using Characters;
using Facilitating.Persistence;
using Game.World;
using Game.World.Environment;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI.CustomTypes;
using Random = UnityEngine.Random;

namespace Game.Characters
{
    public class CharacterAttributes : IPersistenceTemplate
    {
        public MyInt Strength = new MyInt(Random.Range(30, 70));
        public MyInt Intelligence = new MyInt(Random.Range(30, 70));
        public MyInt Endurance = new MyInt(Random.Range(30, 70));
        public MyInt Stability = new MyInt(Random.Range(30, 70));
        public MyInt Starvation = new MyInt(0, 0, 50);
        public MyInt Dehydration = new MyInt(0, 0, 30);
        /*instead of consuming x food or water every minutes, consume 1 food or water every x minutes
        use the max value of the hunger and thirst to keep track of the interval at which eating or drinking should occur
        consume 1 food or water whenever the current value reaches the max value, then reset
        this allows the duration to easily change depending on temperature, modifiers, etc.
        */
        public MyInt Hunger = new MyInt(0, 0, 12);
        public MyInt Thirst = new MyInt(0, 0, 12);
        public WeightCategory Weight;
        private readonly Character _character;

        public CharacterAttributes(Character character)
        {
            _character = character;
        }

        public void UpdateHunger()
        {
            UpdateConsumableTolerance(Hunger, Starvation, _character.Eat);
        }

        public void UpdateThirst()
        {
            Thirst.Max = (int)(-0.2f * WorldState.EnvironmentManager.GetTemperature() + 16f);
            UpdateConsumableTolerance(Thirst, Dehydration, _character.Drink);
        }

        private void UpdateConsumableTolerance(MyInt requirement, MyInt tolerance, Action consume)
        {
            ++requirement.Val;
            if (requirement.ReachedMax())
            {
                ++tolerance.Val;
                requirement.Val = 0;
            }
            float tolerancePercentage = 100f / tolerance.Max * tolerance.Val;
            if (tolerance.ReachedMax())
            {
                _character.Kill();
            } else if (tolerancePercentage > 70)
            {
                consume();
            }
        }

        public string GetHungerStatus()
        {
            float starvationPercentage = 100f / Starvation.Max * Starvation.Val;
            if (starvationPercentage <= 5)
            {
                return "Full";
            }
            if (starvationPercentage <= 20)
            {
                return "Sated";
            }
            if (starvationPercentage <= 50)
            {
                return "Hungry";
            }
            if (starvationPercentage <= 70)
            {
                return "Ravenous";
            }
            return "Starving";
        }

        public string GetThirstStatus()
        {
            float dehydrationPercentage = 100f / Dehydration.Max * Dehydration.Val;
            if (dehydrationPercentage <= 5)
            {
                return "Slaked";
            }
            if (dehydrationPercentage <= 20)
            {
                return "Quenched";
            }
            if (dehydrationPercentage <= 50)
            {
                return "Thirsty";
            }
            if (dehydrationPercentage <= 70)
            {
                return "Aching";
            }
            return "Parched";
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
            Weight = (WeightCategory)SaveController.ParseIntFromSubNode(weightNode);
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

            SaveController.CreateNodeAndAppend("Weight", doc, (int)Weight);
        }

        private void SaveAttribute(XmlNode root, string attributeName, MyInt attribute)
        {
            XmlNode attributeNode = SaveController.CreateNodeAndAppend(attributeName, root);
            SaveController.CreateNodeAndAppend("Val", attributeNode, attribute.Max);
            SaveController.CreateNodeAndAppend("Max", attributeNode, attribute.Val);
        }
    }
}