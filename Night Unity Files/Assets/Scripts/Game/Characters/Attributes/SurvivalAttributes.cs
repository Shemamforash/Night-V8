using System;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters.CharacterActions;
using Game.World;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI;

namespace Game.Characters.Attributes
{
    public class SurvivalAttributes : DesolationAttributes
    {
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
        
        private bool _starving, _dehydrated;
        private readonly int[] _toleranceThresholds = {0, 10, 25, 50, 75};
        private readonly string[] _dehydrationLevels = {"Slaked", "Quenched", "Thirsty", "Aching", "Parched"};
        private readonly string[] _starvationLevels = {"Full", "Sated", "Hungry", "Ravenous", "Starving"};
        
        protected override void RegisterTimedEvents()
        {
            WorldState.RegisterMinuteEvent(UpdateThirstAndHunger);
            WorldState.RegisterHourEvent(Fatigue);
            SetConsumptionEvents(Hunger, Starvation, InventoryResourceType.Food);
            SetConsumptionEvents(Thirst, Dehydration, InventoryResourceType.Water);
        }
        
        protected override void CacheAttributes()
        {
            AddAttribute(Starvation);
            AddAttribute(Dehydration);
            AddAttribute(Hunger);
            AddAttribute(Thirst);
        }

        private void Fatigue()
        {
            float starvationLevel = Starvation.AsPercent();
            float dehydrationLevel = Dehydration.AsPercent();
            CharacterAttribute Intelligence = Character.BaseAttributes.Intelligence;
            if (starvationLevel >= _toleranceThresholds[4] || dehydrationLevel >= _toleranceThresholds[4])
            {
                Intelligence.SetCurrentValue(Intelligence.CurrentValue() - 2);
            }
            else if (starvationLevel >= _toleranceThresholds[3] || dehydrationLevel >= _toleranceThresholds[3])
            {
                Intelligence.SetCurrentValue(Intelligence.CurrentValue() - 1);
            }
            else
            {
                Intelligence.SetCurrentValue(Intelligence.CurrentValue() + 1);
            }
        }
        
        public override void Load(XmlNode doc, PersistenceType saveType)
        {
            LoadAttribute(doc, nameof(Starvation), Starvation);
            LoadAttribute(doc, nameof(Dehydration), Dehydration);
            LoadAttribute(doc, nameof(Hunger), Hunger);
            LoadAttribute(doc, nameof(Thirst), Thirst);

            XmlNode weightNode = doc.SelectSingleNode("Weight");
            Weight = (WeightCategory) SaveController.ParseIntFromSubNode(weightNode);
        }

        public override void Save(XmlNode doc, PersistenceType saveType)
        {
            SaveAttribute(doc, nameof(Starvation), Starvation);
            SaveAttribute(doc, nameof(Dehydration), Dehydration);
            SaveAttribute(doc, nameof(Hunger), Hunger);
            SaveAttribute(doc, nameof(Thirst), Thirst);

            SaveController.CreateNodeAndAppend("Weight", doc, (int) Weight);
        }
        
        private void Drink()
        {
            if (Dehydration.CurrentValue() == 0) return;
            float consumed = WorldState.HomeInventory().DecrementResource(InventoryResourceType.Water, 1);
            Dehydration.SetCurrentValue(Dehydration.CurrentValue() - consumed);
        }

        private void Eat()
        {
            if (Starvation.CurrentValue() == 0) return;
            float consumed = WorldState.HomeInventory().DecrementResource(InventoryResourceType.Food, 1);
            Starvation.SetCurrentValue(Starvation.CurrentValue() - consumed);
        }

        private void SetConsumptionEvents(CharacterAttribute need, CharacterAttribute tolerance, InventoryResourceType resourceType)
        {
            InventoryResource resource = WorldState.HomeInventory().GetResource(resourceType);
            if (resource == null) return;
            need.OnMax(() =>
            {
                if (resource.Decrement(1) != 1)
                {
                    tolerance.SetCurrentValue(tolerance.CurrentValue() + 1);
                }
                need.SetCurrentValue(0);
            });
            tolerance.OnMax(Character.Kill);
        }

        private void UpdateThirstAndHunger()
        {
            Thirst.Max = (int) (-0.2f * WorldState.EnvironmentManager.GetTemperature() + 16f);
            UpdateConsumableTolerance(Hunger, Starvation, Eat);
            UpdateConsumableTolerance(Thirst, Dehydration, Drink);
        }

        public SurvivalAttributes(Character character) : base(character)
        {
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
        
        private void UpdateConsumableTolerance(MyValue requirement, MyValue tolerance, Action consume)
        {
            float previousTolerance = tolerance.AsPercent();
            Intensity previousIntensity = GetIntensity(previousTolerance);
            requirement.SetCurrentValue(requirement.CurrentValue() + 1);
            float tolerancePercentage = tolerance.AsPercent();
            Intensity currentIntensity = GetIntensity(tolerancePercentage);
            consume();
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
    }
}