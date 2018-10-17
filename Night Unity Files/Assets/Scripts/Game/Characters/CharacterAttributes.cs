using System;
using System.Collections.Generic;
using Game.Exploration.Environment;
using Game.Exploration.Weather;
using Game.Exploration.WorldEvents;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Characters
{
    public class CharacterAttributes : DesolationAttributes
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

        public bool SkillOneUnlocked, SkillTwoUnlocked;
        private static readonly List<AttributeType> _attributeTypes = new List<AttributeType>();

        public float EssenceRecoveryModifier;
        public float DurabilityLossModifier;
        public float RallyHealthModifier;
        public float StartHealthModifier;
        public float ClaimRegionWillGainModifier;
        public float EnemyKillHealthLoss;
        public float ReloadFailureChance;
        public float ResourceFindModifier;
        public float HungerModifier;
        public float ThirstModifier;
        public float FoodThirstModifier;
        public float WaterHungerModifier;
        public float FireExplodeChance;
        public float FireDamageModifier;
        public float DecayExplodeChance;
        public float DecayDamageModifier;
        public float SicknessStackModifier;
        public float FreeSkillChance;
        public float InstantCooldownChance;
        public float SkillDisableChance;
        public bool ReloadOnEmptyMag;
        public bool ReloadOnLastRound;
        public bool SpreadSickness;

        public CharacterAttributes(Player player)
        {
            _player = player;

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

        public void ChangeGritMax(int polarity)
        {
            if (!IncreaseAttribute(AttributeType.Grit, polarity)) return;
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

        public void ChangeFettleMax(int polarity)
        {
            if (!IncreaseAttribute(AttributeType.Fettle, polarity)) return;
            string message = polarity > 0 ? "My fettle grows" : "My fettle wains";
            WorldEventManager.GenerateEvent(new CharacterMessage(message, _player));
        }

        public void ChangeFocusMax(int polarity)
        {
            if (!IncreaseAttribute(AttributeType.Focus, polarity)) return;
            string message = polarity > 0 ? "My eyes become keener" : "My vision blurs";
            WorldEventManager.GenerateEvent(new CharacterMessage(message, _player));
        }

        public void ChangeWillMax(int polarity)
        {
            if (!IncreaseAttribute(AttributeType.Will, polarity)) return;
            string message = polarity > 0 ? "My mind is clearer now" : "My mind is clouded";
            WorldEventManager.GenerateEvent(new CharacterMessage(message, _player));
        }

        public float CalculateAdrenalineRecoveryRate()
        {
            return (0.1f * Val(AttributeType.Focus) + 1) * Val(AttributeType.AdrenalineRechargeBonus);
        }

        public float CalculateSpeed() => 5f + Val(AttributeType.Grit) * 0.25f;

        public float CalculateSkillCooldownModifier()
        {
            return (-0.025f * Val(AttributeType.Will) + 1) * Val(AttributeType.SkillRechargeBonus);
        }

        public int CalculateMaxHealth()
        {
            return (int) (Max(AttributeType.Fettle) * PlayerHealthChunkSize);
        }

        public int CalculateInitialHealth()
        {
            int startingHealth = (int) (Val(AttributeType.Fettle) * PlayerHealthChunkSize);
            startingHealth = Mathf.FloorToInt(startingHealth * (1f - StartHealthModifier));
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

            if (!_player.IsConsuming())
            {
                thirstIncrementAmount = 0;
                hungerIncrementAmount = 0;
            }

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

        public int CalculateCompassPulses()
        {
            return Mathf.CeilToInt(Val(AttributeType.Focus) + Val(AttributeType.CompassBonus));
        }

        public void Drink(int thirstRecovery)
        {
            int thirstLoss = (int) ThirstModifier + thirstRecovery;
            int hungerGain = (int) WaterHungerModifier;
            Debug.Log("thirst modifier " + thirstLoss + " hunger gain " + hungerGain);
            Get(AttributeType.Thirst).Decrement(thirstLoss);
            Get(AttributeType.Hunger).Increment(hungerGain);
            WorldEventManager.GenerateEvent(new CharacterMessage("I needed that", _player));
        }

        public void Eat(int hungerRecovery)
        {
            int hungerLoss = (int) HungerModifier + hungerRecovery;
            int thirstGain = (int) FoodThirstModifier;
            Debug.Log("hunger modifier " + hungerRecovery + " thirst gain " + thirstGain);
            Get(AttributeType.Hunger).Decrement(hungerLoss);
            Get(AttributeType.Thirst).Increment(thirstGain);
            WorldEventManager.GenerateEvent(new CharacterMessage("That should stave off starvation, at least for a while", _player));
        }

        public void CalculateNewFettle(float health)
        {
            float newFettle = Mathf.CeilToInt(health / PlayerHealthChunkSize);
            SetVal(AttributeType.Fettle, newFettle);
        }

        public void UnlockWeaponSkillTwo(WeaponType weaponType, bool showScreen)
        {
            WeaponSkillTwoUnlocks.Add(weaponType);
            if (!showScreen) return;
            UiBrandMenu.ShowWeaponSkillUnlock(weaponType, _player.EquippedWeapon.WeaponSkillTwo);
        }

        public void UnlockWeaponSkillOne(WeaponType weaponType, bool showScreen)
        {
            WeaponSkillOneUnlocks.Add(weaponType);
            if (!showScreen) return;
            UiBrandMenu.ShowWeaponSkillUnlock(weaponType, _player.EquippedWeapon.WeaponSkillOne);
        }

        public void UnlockCharacterSkillOne(bool showScreen)
        {
            if (SkillOneUnlocked) return;
            SkillOneUnlocked = true;
            if (!showScreen) return;
            UiBrandMenu.ShowCharacterSkillUnlock(_player.CharacterSkillOne);
        }

        public void UnlockCharacterSkillTwo(bool showScreen)
        {
            if (SkillOneUnlocked) return;
            SkillTwoUnlocked = true;
            if (!showScreen) return;
            UiBrandMenu.ShowCharacterSkillUnlock(_player.CharacterSkillTwo);
        }
    }
}