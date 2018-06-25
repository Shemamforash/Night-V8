using System;
using System.Xml;
using Facilitating.Persistence;
using Game.Exploration.Environment;
using Game.Exploration.Weather;
using SamsHelper.BaseGameFunctionality.Basic;
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
        private const float MinSpeed = 3, MaxSpeed = 6;
        public const int PlayerHealthChunkSize = 50;

        private readonly Player _player;

        public CharacterAttribute Endurance, Perception, Strength, Willpower;

        /*instead of consuming x food or water every minutes, consume 1 food or water every x minutes
        use the max value of the hunger and thirst to keep track of the interval at which eating or drinking should occur
        consume 1 food or water whenever the current value reaches the max value, then reset
        this allows the duration to easily change depending on temperature, modifiers, etc.
        */

        public CharacterAttribute Hunger, Thirst;

        //summative
        public CharacterAttribute ScrapFindBonus, FoodFindBonus, WaterFindBonus, EssenceFindBonus, HealthBonus, WillpowerLossBonus, FireChanceBonus, DecayChanceBonus, SicknessChanceBonus;

        //multiplicative
        public CharacterAttribute EssenceLossBonus, SkillRechargeBonus, AdrenalineRechargeBonus, DamageBonus, CompassBonus;

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

        private readonly AttributeModifier _hungerModifier = new AttributeModifier();
        private readonly AttributeModifier _thirstModifier = new AttributeModifier();

        protected override void CacheAttributes()
        {
            Endurance = new CharacterAttribute(this, AttributeType.Endurance, 0);
            Perception = new CharacterAttribute(this, AttributeType.Perception, 0);
            Strength = new CharacterAttribute(this, AttributeType.Strength, 0);
            Willpower = new CharacterAttribute(this, AttributeType.Willpower, 0);

            Strength.AddModifier(_hungerModifier);
            Endurance.AddModifier(_hungerModifier);
            Willpower.AddModifier(_thirstModifier);
            Perception.AddModifier(_thirstModifier);

            ScrapFindBonus = new CharacterAttribute(this, AttributeType.ScrapFindBonus, 0, float.NegativeInfinity);
            FoodFindBonus = new CharacterAttribute(this, AttributeType.FoodFindBonus, 0, float.NegativeInfinity);
            WaterFindBonus = new CharacterAttribute(this, AttributeType.WaterFindBonus, 0, float.NegativeInfinity);
            EssenceFindBonus = new CharacterAttribute(this, AttributeType.EssenceFindBonus, 0, float.NegativeInfinity);
            HealthBonus = new CharacterAttribute(this, AttributeType.HealthBonus, 0, float.NegativeInfinity);
            WillpowerLossBonus = new CharacterAttribute(this, AttributeType.WillpowerLossBonus, 0, float.NegativeInfinity);
            FireChanceBonus = new CharacterAttribute(this, AttributeType.BurnChanceBonus, 0, float.NegativeInfinity);
            DecayChanceBonus = new CharacterAttribute(this, AttributeType.DecayChanceBonus, 0, float.NegativeInfinity);
            SicknessChanceBonus = new CharacterAttribute(this, AttributeType.SicknessChanceBonus, 0, float.NegativeInfinity);
            EssenceLossBonus = new CharacterAttribute(this, AttributeType.EssenceLossBonus, 1, float.NegativeInfinity);
            DamageBonus = new CharacterAttribute(this, AttributeType.DamageBonus, 1, float.NegativeInfinity);
            CompassBonus = new CharacterAttribute(this, AttributeType.CompassBonus, 0);
            SkillRechargeBonus = new CharacterAttribute(this, AttributeType.SkillRechargeBonus, 1, float.NegativeInfinity);
            AdrenalineRechargeBonus = new CharacterAttribute(this, AttributeType.AdrenalineRechargeBonus, 1, float.NegativeInfinity);

            Hunger = new CharacterAttribute(this, AttributeType.Hunger, 0, 0, 10);
            Thirst = new CharacterAttribute(this, AttributeType.Thirst, 0, 0, 10);
        }

        public void IncreaseEnduranceMax(int amount)
        {
            int newEnduranceMax = (int) (Endurance.Max + amount);
            newEnduranceMax = Mathf.Clamp(newEnduranceMax, 0, _player.CharacterTemplate.EnduranceCap);
            Endurance.Max = newEnduranceMax;
        }

        public void IncreaseStrengthMax(int amount)
        {
            int newStrengthMax = (int) (Strength.Max + amount);
            newStrengthMax = Mathf.Clamp(newStrengthMax, 0, _player.CharacterTemplate.StrengthCap);
            Strength.Max = newStrengthMax;
        }

        public void IncreasePerceptionMax(int amount)
        {
            int newPerceptionMax = (int) (Perception.Max + amount);
            newPerceptionMax = Mathf.Clamp(newPerceptionMax, 0, _player.CharacterTemplate.PerceptionCap);
            Perception.Max = newPerceptionMax;
        }

        public void IncreaseWillpowerMax(int amount)
        {
            int newWillpowerMax = (int) (Willpower.Max + amount);
            newWillpowerMax = Mathf.Clamp(newWillpowerMax, 0, _player.CharacterTemplate.WillpowerCap);
            Willpower.Max = newWillpowerMax;
        }

        public float RemainingCarryCapacity()
        {
            return Strength.CurrentValue();
        }

        public float CalculateAdrenalineRecoveryRate()
        {
            return Mathf.Pow(1.05f, Perception.CurrentValue() + AdrenalineRechargeBonus.CurrentValue());
        }

        public float CalculateSpeed()
        {
            return 3f + (Endurance.CurrentValue() - 1f) * 0.3f;
        }

        public float CalculateSkillCooldownModifier()
        {
            return (float) Math.Pow(0.95f, Willpower.CurrentValue()) + SkillRechargeBonus.CurrentValue();
        }

        public int CalculateCombatHealth()
        {
            return (int) (Strength.CurrentValue() * (PlayerHealthChunkSize + HealthBonus.CurrentValue()));
        }

        public void UpdateThirstAndHunger()
        {
            TemperatureCategory temperature = EnvironmentManager.GetTemperature();
            float thirstTemperatureModifier;
            float hungerTemperatureModifier;
            switch (temperature)
            {
                case TemperatureCategory.Freezing:
                    thirstTemperatureModifier = 0.5f;
                    hungerTemperatureModifier = 1.5f;
                    break;
                case TemperatureCategory.Cold:
                    thirstTemperatureModifier = 0.75f;
                    hungerTemperatureModifier = 1.25f;
                    break;
                case TemperatureCategory.Warm:
                    thirstTemperatureModifier = 1f;
                    hungerTemperatureModifier = 1f;
                    break;
                case TemperatureCategory.Hot:
                    thirstTemperatureModifier = 1.25f;
                    hungerTemperatureModifier = 0.75f;
                    break;
                case TemperatureCategory.Boiling:
                    thirstTemperatureModifier = 1.5f;
                    hungerTemperatureModifier = 0.5f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            float thirstIncrementAmount = thirstTemperatureModifier / 12f;
            float hungerIncrementAmount = hungerTemperatureModifier / 24f;

            Hunger.Increment(hungerIncrementAmount);
            _hungerModifier.SetFinalBonus(-Hunger.Normalised());
            if (Hunger.ReachedMax()) _player.Kill();

            Thirst.Increment(thirstIncrementAmount);
            _thirstModifier.SetFinalBonus(-Thirst.Normalised());
            if (Thirst.ReachedMax()) _player.Kill();
        }

        private string GetAttributeStatus(Number characterAttribute, string[] levels)
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
            return GetAttributeStatus(Hunger, _starvationLevels);
        }

        public string GetThirstStatus()
        {
            return GetAttributeStatus(Thirst, _dehydrationLevels);
        }

        public void Load(XmlNode doc, PersistenceType saveType)
        {
            LoadAttribute(doc, nameof(Strength), Strength);
            LoadAttribute(doc, nameof(Endurance), Endurance);
            LoadAttribute(doc, nameof(Willpower), Willpower);
            LoadAttribute(doc, nameof(Perception), Perception);

            float hungerVal = float.Parse(doc.SelectSingleNode("Hunger").InnerText);
            float thirstVal = float.Parse(doc.SelectSingleNode("Thirst").InnerText);

            Hunger.SetCurrentValue(hungerVal);
            Thirst.SetCurrentValue(thirstVal);
        }

        public XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            SaveAttribute(doc, nameof(Strength), Strength);
            SaveAttribute(doc, nameof(Endurance), Endurance);
            SaveAttribute(doc, nameof(Willpower), Willpower);
            SaveAttribute(doc, nameof(Perception), Perception);

            SaveController.CreateNodeAndAppend("Hunger", doc, Hunger.CurrentValue());
            SaveController.CreateNodeAndAppend("Thirst", doc, Thirst.CurrentValue());
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

        public void DecreaseWillpower()
        {
            Willpower.Decrement();
            int minorBreakThreshold = Mathf.FloorToInt(Willpower.Max / 2f);
            int majorBreakThreshold = Mathf.FloorToInt(Willpower.Max / 4f);
            if (Willpower.CurrentValue() <= majorBreakThreshold)
            {
                //todo major break;                
            }
            else if (Willpower.CurrentValue() <= minorBreakThreshold)
            {
                //todo minor break;
            }
        }

        public int CalculateCompassPulses()
        {
            return (int) (Perception.CurrentValue() + CompassBonus.CurrentValue());
        }
    }
}