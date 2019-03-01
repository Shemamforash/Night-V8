using System;
using System.Collections.Generic;
using Facilitating.UIControllers;
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
        private readonly string[] _dehydrationLevels = {"Slaked", "Thirsty", "Parched"};
        private readonly Player _player;
        private readonly string[] _starvationLevels = {"Sated", "Hungry", "Starved"};
        private readonly float[] _toleranceThresholds = {0, 0.6f, 0.3f};

        public readonly HashSet<WeaponType> WeaponSkillOneUnlocks = new HashSet<WeaponType>();
        public readonly HashSet<WeaponType> WeaponSkillTwoUnlocks = new HashSet<WeaponType>();
        private const int DehydrateDeathTime = 24;
        private const int StarvationDeathTime = 48;

        public bool SkillOneUnlocked, SkillTwoUnlocked;
        private static readonly List<AttributeType> _attributeTypes = new List<AttributeType>();

        public float EssenceRecoveryModifier;
        public float RallyHealthModifier;
        public float ClaimRegionWillGainModifier;
        public float ResourceFindModifier;
        public float HungerModifier;
        public float ThirstModifier;
        public float FireExplodeChance;
        public float DecayExplodeChance;
        public float FreeSkillChance;
        public bool ReloadOnEmptyMag;
        public bool ReloadOnFatalShot;
        public bool SpreadVoid;

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
            if (newMax > 20 || newMax <= 0) return false;
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

        public float CalculateSpeed() => 5f + Max(AttributeType.Grit) * 0.25f;

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
            if (hunger.ReachedMax())
            {
                _player.Kill(DeathReason.Hunger);
                return;
            }

            CharacterAttribute thirst = Get(AttributeType.Thirst);
            thirst.Increment(thirstIncrementAmount);
            if (thirst.ReachedMax())
            {
                _player.Kill(DeathReason.Thirst);
            }
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

        public void SetTutorialValues()
        {
            Get(AttributeType.Hunger).SetCurrentValue(1);
            Get(AttributeType.Thirst).SetCurrentValue(1);
        }

        public void ResetValues()
        {
            Get(AttributeType.Hunger).SetCurrentValue(0);
            Get(AttributeType.Thirst).SetCurrentValue(0);
        }

        public string GetThirstStatus()
        {
            if (Val(AttributeType.Thirst) == 8)
            {
                WorldEventManager.GenerateEvent(new CharacterMessage("I feel parched", _player));
            }

            return GetAttributeStatus(Get(AttributeType.Thirst), _dehydrationLevels);
        }

        public void Drink(int thirstRecovery)
        {
            int thirstLoss = (int) ThirstModifier + thirstRecovery;
            Get(AttributeType.Thirst).Decrement(thirstLoss);
        }

        public void Eat(int hungerRecovery)
        {
            int hungerLoss = (int) HungerModifier + hungerRecovery;
            Get(AttributeType.Hunger).Decrement(hungerLoss);
        }

        public void CalculateNewFettle(float health)
        {
            float newFettle = Mathf.CeilToInt(health / PlayerHealthChunkSize);
            SetVal(AttributeType.Fettle, newFettle);
        }

        public void UnlockWeaponSkillTwo(WeaponType weaponType, bool showScreen)
        {
            if (WeaponSkillTwoUnlocks.Contains(weaponType)) return;
            WeaponSkillTwoUnlocks.Add(weaponType);
            if (!showScreen) return;
            UiBrandMenu.ShowWeaponSkillUnlock(weaponType, _player.EquippedWeapon.WeaponSkillTwo);
        }

        public void UnlockWeaponSkillOne(WeaponType weaponType, bool showScreen)
        {
            if (WeaponSkillOneUnlocks.Contains(weaponType)) return;
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
            if (SkillTwoUnlocked) return;
            SkillTwoUnlocked = true;
            if (!showScreen) return;
            UiBrandMenu.ShowCharacterSkillUnlock(_player.CharacterSkillTwo);
        }
    }
}