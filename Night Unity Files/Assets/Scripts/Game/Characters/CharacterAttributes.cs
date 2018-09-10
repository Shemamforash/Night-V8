using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.Persistence;
using Game.Exploration.Environment;
using Game.Exploration.Weather;
using Game.Exploration.WorldEvents;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Characters
{
    public class CharacterAttributes : DesolationAttributes, IPersistenceTemplate
    {
        public const int PlayerHealthChunkSize = 100;
        private readonly string[] _dehydrationLevels = {"Slaked", "Quenched", "Thirsty", "Aching", "Parched"};
        private readonly Player _player;
        private readonly string[] _starvationLevels = {"Full", "Sated", "Hungry", "Ravenous", "Starving"};
        private readonly float[] _toleranceThresholds = {0, 0.1f, 0.25f, 0.5f, 0.75f};

        public readonly HashSet<WeaponType> WeaponSkillOneUnlocks = new HashSet<WeaponType>();
        public readonly HashSet<WeaponType> WeaponSkillTwoUnlocks = new HashSet<WeaponType>();
        private const int DehydrateDeathTime = 18;
        private const int StarvationDeathTime = 36;
        public bool BurnWeakness;
        public bool DecayRetaliate;
        public bool DecayWeakness;
        public bool LeaveFireTrail;

        public bool ReloadOnEmptyMag, ReloadOnLastRound;
        public bool SicknessWeakness;
        public bool SkillOneUnlocked, SkillTwoUnlocked;
        public bool SpreadSickness;
        private static readonly List<AttributeType> _attributeTypes = new List<AttributeType>();

        public CharacterAttributes(Player player)
        {
            _player = player;

            SetVal(AttributeType.EssenceLossBonus, 1);
            SetVal(AttributeType.SkillRechargeBonus, 1);
            SetVal(AttributeType.AdrenalineRechargeBonus, 1);

            SetMax(AttributeType.Hunger, 10);
            SetMax(AttributeType.Thirst, 10);
        }

        public static AttributeType StringToAttributeType(string attributeType)
        {
            LoadAttributeTypes();
            foreach (AttributeType a in _attributeTypes)
            {
                if (a.ToString() != attributeType) continue;
                return a;
            }

            throw new ArgumentOutOfRangeException();
        }

        private static void LoadAttributeTypes()
        {
            if (_attributeTypes.Count != 0) return;
            foreach (AttributeType attributeType in Enum.GetValues(typeof(AttributeType)))
            {
                _attributeTypes.Add(attributeType);
            }
        }

        public void ChangeEnduranceMax(int polarity)
        {
            if (!IncreaseAttribute(AttributeType.Endurance, polarity)) return;
            string message = polarity > 0 ? "My body will endure" : "My body weakens";
            WorldEventManager.GenerateEvent(new CharacterMessage(message, _player));
        }

        private bool IncreaseAttribute(AttributeType attributeType, int polarity)
        {
            int newMax = (int) (Max(attributeType) + polarity);
            if (newMax > 20) return false;
            if (newMax < 0) newMax = 0;
            SetMax(attributeType, newMax);
            return true;
        }

        public void ChangeStrengthMax(int polarity)
        {
            if (!IncreaseAttribute(AttributeType.Strength, polarity)) return;
            string message = polarity > 0 ? "My strength grows" : "My strength wains";
            WorldEventManager.GenerateEvent(new CharacterMessage(message, _player));
        }

        public void ChangePerceptionMax(int polarity)
        {
            if (!IncreaseAttribute(AttributeType.Perception, polarity)) return;
            string message = polarity > 0 ? "My eyes become keener" : "My vision blurs";
            WorldEventManager.GenerateEvent(new CharacterMessage(message, _player));
        }

        public void ChangeWillpowerMax(int polarity)
        {
            if (!IncreaseAttribute(AttributeType.Willpower, polarity)) return;
            string message = polarity > 0 ? "My mind is clearer now" : "My mind is clouded";
            WorldEventManager.GenerateEvent(new CharacterMessage(message, _player));
        }

        public float CalculateAdrenalineRecoveryRate() => Mathf.Pow(1.05f, Val(AttributeType.Perception) + Val(AttributeType.AdrenalineRechargeBonus));

        public float CalculateSpeed() => 5f + Val(AttributeType.Endurance) * 0.25f;

        public float CalculateSkillCooldownModifier() => (float) Math.Pow(0.95f, Val(AttributeType.Willpower)) + Val(AttributeType.SkillRechargeBonus);

        public int CalculateMaxHealth()
        {
            return (int) (Max(AttributeType.Strength) * PlayerHealthChunkSize);
        }

        public int CalculateInitialHealth()
        {
            int startingHealth = (int) (Val(AttributeType.Strength) * PlayerHealthChunkSize);
            float healthLossModifier = Val(AttributeType.HealthLossBonus);
            int startHealthModifier = Mathf.FloorToInt(startingHealth * healthLossModifier);
            startingHealth -= startHealthModifier;
            return startingHealth;
        }

        public void UpdateThirstAndHunger()
        {
            TemperatureCategory temperature = EnvironmentManager.GetTemperature();
            float thirstTemperatureModifier;
            float hungerTemperatureModifier;
            switch (temperature)
            {
                case TemperatureCategory.Freezing:
                    thirstTemperatureModifier = 1f;
                    hungerTemperatureModifier = 2f;
                    break;
                case TemperatureCategory.Cold:
                    thirstTemperatureModifier = 1f;
                    hungerTemperatureModifier = 1.5f;
                    break;
                case TemperatureCategory.Warm:
                    thirstTemperatureModifier = 1f;
                    hungerTemperatureModifier = 1f;
                    break;
                case TemperatureCategory.Hot:
                    thirstTemperatureModifier = 1.5f;
                    hungerTemperatureModifier = 1f;
                    break;
                case TemperatureCategory.Burning:
                    thirstTemperatureModifier = 2f;
                    hungerTemperatureModifier = 1f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            float thirstIncrementAmount = thirstTemperatureModifier / DehydrateDeathTime;
            float hungerIncrementAmount = hungerTemperatureModifier / StarvationDeathTime;

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

        private void LoadAttribute(XmlNode root, string attributeName, CharacterAttribute characterAttribute)
        {
            XmlNode attributeNode = root.GetNode(attributeName);
            characterAttribute.Max = attributeNode.IntFromNode("Max");
            characterAttribute.SetCurrentValue(attributeNode.IntFromNode("Val"));
        }

        private void SaveAttribute(XmlNode root, string attributeName, CharacterAttribute characterAttribute)
        {
            root.CreateChild(attributeName, characterAttribute.CurrentValue() + "/" + characterAttribute.Max);
        }

        public void DecreaseWillpower()
        {
            CharacterAttribute willpower = Get(AttributeType.Willpower);
            willpower.Decrement();
            int perceptionValue = (int) Val(AttributeType.Perception);
            int strengthValue = (int) Val(AttributeType.Strength);
            int enduranceValue = (int) Val(AttributeType.Endurance);
            bool perceptionIsMax = perceptionValue > strengthValue && perceptionValue > enduranceValue;
            bool strengthIsMax = strengthValue > perceptionValue && strengthValue > enduranceValue;
            if (perceptionIsMax) Get(AttributeType.Perception).Decrement();
            else if (strengthIsMax) Get(AttributeType.Strength).Decrement();
            else Get(AttributeType.Endurance).Decrement();
        }

        public int CalculateCompassPulses()
        {
            return Mathf.CeilToInt(Val(AttributeType.Perception) + Val(AttributeType.CompassBonus));
        }

        public void Drink()
        {
            int thirstLoss = (int) Val(AttributeType.ThirstBonus) + 1;
            int hungerGain = (int) Val(AttributeType.StarvingWaterBonus);
            CharacterAttribute thirst = Get(AttributeType.Thirst);
            thirst.Decrement(thirstLoss);
            thirst.Increment(hungerGain);
            Get(AttributeType.Thirst).Decrement(thirstLoss);
            WorldEventManager.GenerateEvent(new CharacterMessage("I needed that", _player));
        }

        public void Eat()
        {
            int hungerLoss = (int) Val(AttributeType.HungerBonus) + 1;
            int thirstGain = (int) Val(AttributeType.DehydratingFoodBonus);
            CharacterAttribute hunger = Get(AttributeType.Hunger);
            hunger.Decrement(hungerLoss);
            hunger.Increment(thirstGain);
            WorldEventManager.GenerateEvent(new CharacterMessage("That should stave off starvation, at least for a while", _player));
        }

        public void CalculateNewStrength(float health)
        {
            float newStrength = Mathf.CeilToInt(health / PlayerHealthChunkSize);
            SetVal(AttributeType.Strength, newStrength);
        }

        public void UnlockWeaponSkillTwo(WeaponType weaponType)
        {
            WeaponSkillTwoUnlocks.Add(weaponType);
            UiBrandMenu.ShowWeaponSkillUnlock(weaponType, _player.EquippedWeapon.WeaponSkillTwo);
        }

        public void UnlockWeaponSkillOne(WeaponType weaponType)
        {
            WeaponSkillOneUnlocks.Add(weaponType);
            UiBrandMenu.ShowWeaponSkillUnlock(weaponType, _player.EquippedWeapon.WeaponSkillOne);
        }

        public void UnlockCharacterSkillOne()
        {
            SkillOneUnlocked = true;
            UiBrandMenu.ShowCharacterSkillUnlock(_player.CharacterSkillOne);
        }

        public void UnlockCharacterSkillTwo()
        {
            SkillTwoUnlocked = true;
            UiBrandMenu.ShowCharacterSkillUnlock(_player.CharacterSkillTwo);
        }
    }
}