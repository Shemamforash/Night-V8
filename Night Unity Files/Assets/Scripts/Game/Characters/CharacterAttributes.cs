using System;
using System.Xml;
using Facilitating.Persistence;
using Game.Exploration.Environment;
using Game.Exploration.Weather;
using Game.Exploration.WorldEvents;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Characters
{
    public class CharacterAttributes : DesolationAttributes, IPersistenceTemplate
    {
        private readonly string[] _dehydrationLevels = {"Slaked", "Quenched", "Thirsty", "Aching", "Parched"};
        private readonly string[] _starvationLevels = {"Full", "Sated", "Hungry", "Ravenous", "Starving"};
        private readonly float[] _toleranceThresholds = {0, 0.1f, 0.25f, 0.5f, 0.75f};
        public const int PlayerHealthChunkSize = 50;
        private readonly Player _player;


        /*instead of consuming x food or water every minutes, consume 1 food or water every x minutes
        use the max value of the hunger and thirst to keep track of the interval at which eating or drinking should occur
        consume 1 food or water whenever the current value reaches the max value, then reset
        this allows the duration to easily change depending on temperature, modifiers, etc.
        */

        public CharacterAttributes(Player player)
        {
            _player = player;

            Set(AttributeType.EssenceLossBonus, 1);
            Set(AttributeType.SkillRechargeBonus, 1);
            Set(AttributeType.AdrenalineRechargeBonus, 1);

            Set(AttributeType.Hunger, 0, 0, 10);
            Set(AttributeType.Thirst, 0, 0, 10);
        }

        public float GetSkillRechargeModifier()
        {
            return Mathf.Pow(0.95f, Val(AttributeType.Willpower));
        }

        public float GetGunDamageModifier()
        {
            return Mathf.Pow(1.05f, Val(AttributeType.Perception));
        }

        public void IncreaseEnduranceMax(int amount)
        {
            int newEnduranceMax = (int) (Max(AttributeType.Endurance) + amount);
            newEnduranceMax = Mathf.Clamp(newEnduranceMax, 0, _player.CharacterTemplate.EnduranceCap);
            SetMax(AttributeType.Endurance, newEnduranceMax);
            WorldEventManager.GenerateEvent(new CharacterMessage("My body will endure", _player));
        }

        public void IncreaseStrengthMax(int amount)
        {
            int newStrengthMax = (int) (Max(AttributeType.Strength) + amount);
            newStrengthMax = Mathf.Clamp(newStrengthMax, 0, _player.CharacterTemplate.StrengthCap);
            SetMax(AttributeType.Strength, newStrengthMax);
            WorldEventManager.GenerateEvent(new CharacterMessage("My strength grows", _player));
        }

        public void IncreasePerceptionMax(int amount)
        {
            int newPerceptionMax = (int) (Max(AttributeType.Perception) + amount);
            newPerceptionMax = Mathf.Clamp(newPerceptionMax, 0, _player.CharacterTemplate.PerceptionCap);
            SetMax(AttributeType.Perception, newPerceptionMax);
            WorldEventManager.GenerateEvent(new CharacterMessage("My eyes become keener", _player));
        }

        public void IncreaseWillpowerMax(int amount)
        {
            int newWillpowerMax = (int) (Max(AttributeType.Willpower) + amount);
            newWillpowerMax = Mathf.Clamp(newWillpowerMax, 0, _player.CharacterTemplate.WillpowerCap);
            SetMax(AttributeType.Willpower, newWillpowerMax);
            WorldEventManager.GenerateEvent(new CharacterMessage("My mind is clearer now", _player));
        }

        public float CalculateAdrenalineRecoveryRate()
        {
            return Mathf.Pow(1.05f, Val(AttributeType.Perception) + Val(AttributeType.AdrenalineRechargeBonus));
        }

        public float CalculateSpeed()
        {
            return 3f + (Val(AttributeType.Endurance) - 1f) * 0.3f;
        }

        public float CalculateSkillCooldownModifier()
        {
            return (float) Math.Pow(0.95f, Val(AttributeType.Willpower)) + Val(AttributeType.SkillRechargeBonus);
        }

        public int CalculateCombatHealth()
        {
            return (int) (Val(AttributeType.Strength) * PlayerHealthChunkSize);
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

            CharacterAttribute hunger = Get(AttributeType.Hunger);
            hunger.Increment(hungerIncrementAmount);
            if (hunger.ReachedMax()) _player.Kill();

            CharacterAttribute thirst = Get(AttributeType.Thirst);
            thirst.Increment(thirstIncrementAmount);
            if (thirst.ReachedMax()) _player.Kill();
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
            if (Val(AttributeType.Hunger) == 8)
            {
                WorldEventManager.GenerateEvent(new CharacterMessage("I might starve", _player));
            }

            return GetAttributeStatus(Get(AttributeType.Hunger), _starvationLevels);
        }

        public string GetThirstStatus()
        {
            if (Val(AttributeType.Thirst) == 8)
            {
                WorldEventManager.GenerateEvent(new CharacterMessage("I feel parched", _player));
            }

            return GetAttributeStatus(Get(AttributeType.Thirst), _dehydrationLevels);
        }

        public void Load(XmlNode doc, PersistenceType saveType)
        {
        }

        public XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
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
            CharacterAttribute willpower = Get(AttributeType.Willpower);
            willpower.Decrement();
            int minorBreakThreshold = Mathf.FloorToInt(willpower.Max / 2f);
            int majorBreakThreshold = Mathf.FloorToInt(willpower.Max / 4f);
            if (willpower.CurrentValue() <= majorBreakThreshold)
            {
                WorldEventManager.GenerateEvent(new CharacterMessage("I can't take any more", _player));
                //todo major break;                
            }
            else if (willpower.CurrentValue() <= minorBreakThreshold)
            {
                WorldEventManager.GenerateEvent(new CharacterMessage("I can see the light beyond the veil, and it speaks to me!", _player));
                //todo minor break;
            }
        }

        public int CalculateCompassPulses()
        {
            return (int) (Val(AttributeType.Perception) + Val(AttributeType.CompassBonus));
        }

        public void Drink()
        {
            Get(AttributeType.Thirst).Decrement();
            WorldEventManager.GenerateEvent(new CharacterMessage("I needed that", _player));
        }

        public void Eat()
        {
            Get(AttributeType.Hunger).Decrement();
            WorldEventManager.GenerateEvent(new CharacterMessage("That should stave off starvation, at least for a while", _player));
        }
    }
}