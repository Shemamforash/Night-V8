using System;
using System.Collections.Generic;
using Facilitating.UIControllers;
using Game.Exploration.Environment;
using Game.Exploration.Weather;
using Game.Exploration.WorldEvents;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Characters
{
    public class CharacterAttributes : DesolationAttributes
    {
        public const int PlayerHealthChunkSize = 100;
        private readonly Player _player;

        public readonly HashSet<WeaponType> WeaponSkillOneUnlocks = new HashSet<WeaponType>();
        public readonly HashSet<WeaponType> WeaponSkillTwoUnlocks = new HashSet<WeaponType>();
        private const int DehydrateDeathTime = 24; //time per drink = 2.4 hrs
        private const int StarvationDeathTime = 48; //time per eat = 4.8 hrs

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
            IncreaseAttribute(AttributeType.Grit, polarity);
        }

        private void IncreaseAttribute(AttributeType attributeType, int polarity)
        {
            int newMax = (int) (Max(attributeType) + polarity);
            if (newMax > 20 || newMax <= 0) return;
            SetMax(attributeType, newMax);
        }

        public void ChangeLifeMax(int polarity)
        {
            IncreaseAttribute(AttributeType.Life, polarity);
        }

        public void ChangeFocusMax(int polarity)
        {
            IncreaseAttribute(AttributeType.Focus, polarity);
        }

        public void ChangeWillMax(int polarity)
        {
            IncreaseAttribute(AttributeType.Will, polarity);
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
            return (int) (Max(AttributeType.Life) * PlayerHealthChunkSize);
        }

        public int CalculateInitialHealth()
        {
            int startingHealth = (int) (Val(AttributeType.Life) * PlayerHealthChunkSize);
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

            IncreaseHunger(hungerIncrementAmount);
            IncreaseThirst(thirstIncrementAmount);
        }

        private void IncreaseHunger(float hungerIncrementAmount)
        {
            if (_player.IsDead) return;
            CharacterAttribute hunger = Get(AttributeType.Hunger);
            float hungerBefore = hunger.Normalised();
            hunger.Increment(hungerIncrementAmount);
            float hungerAfter = hunger.Normalised();
            if (hungerBefore >= 0.25f && hungerAfter < 0.25f)
            {
                WorldEventManager.GenerateEvent(new CharacterMessage(_hungerEvents.RandomElement(), _player));
            }

            if (!hunger.ReachedMax()) return;
            _player.Kill(DeathReason.Hunger);
        }

        private void IncreaseThirst(float thirstIncrementAmount)
        {
            if (_player.IsDead) return;
            CharacterAttribute thirst = Get(AttributeType.Thirst);
            float thirstBefore = thirst.Normalised();
            thirst.Increment(thirstIncrementAmount);
            float thirstAfter = thirst.Normalised();
            if (thirstBefore >= 0.25f && thirstAfter < 0.25f)
            {
                WorldEventManager.GenerateEvent(new CharacterMessage(_thirstEvents.RandomElement(), _player));
            }

            if (!thirst.ReachedMax()) return;
            _player.Kill(DeathReason.Thirst);
        }

        private readonly string[] _hungerEvents =
        {
            "I have to get something to eat",
            "My stomach has been empty for too long",
            "I can't go on if I don't get any food"
        };

        private readonly string[] _thirstEvents =
        {
            "I'm going to die if I don't get any water",
            "I'm so thirsty, it's been so long since I had a drink",
            "I need to get some water soon"
        };

        public void SetTutorialValues()
        {
            Get(AttributeType.Hunger).SetCurrentValue(1);
            Get(AttributeType.Thirst).SetCurrentValue(1);
        }

        public void ResetValues()
        {
            Get(AttributeType.Grit).SetToMax();
            Get(AttributeType.Life).SetToMax();
            Get(AttributeType.Focus).SetToMax();
            Get(AttributeType.Will).SetToMax();
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

        public void CalculateNewLife(float health)
        {
            float newLife = Mathf.CeilToInt(health / PlayerHealthChunkSize);
            SetVal(AttributeType.Life, newLife);
        }

        public void UnlockWeaponSkillTwo(WeaponType weaponType, bool showScreen)
        {
            if (WeaponSkillTwoUnlocks.Contains(weaponType)) return;
            WeaponSkillTwoUnlocks.Add(weaponType);
            if (!showScreen) return;
            UiBrandMenu.ShowWeaponSkillUnlock(weaponType, _player.EquippedWeapon.WeaponSkillTwo, 4);
        }

        public void UnlockWeaponSkillOne(WeaponType weaponType, bool showScreen)
        {
            if (WeaponSkillOneUnlocks.Contains(weaponType)) return;
            WeaponSkillOneUnlocks.Add(weaponType);
            if (!showScreen) return;
            UiBrandMenu.ShowWeaponSkillUnlock(weaponType, _player.EquippedWeapon.WeaponSkillOne, 3);
        }

        public void UnlockCharacterSkillOne(bool showScreen)
        {
            if (SkillOneUnlocked) return;
            SkillOneUnlocked = true;
            if (!showScreen) return;
            UiBrandMenu.ShowCharacterSkillUnlock(_player.CharacterSkillOne, 1);
        }

        public void UnlockCharacterSkillTwo(bool showScreen)
        {
            if (SkillTwoUnlocked) return;
            SkillTwoUnlocked = true;
            if (!showScreen) return;
            UiBrandMenu.ShowCharacterSkillUnlock(_player.CharacterSkillTwo, 2);
        }

        public float CalculateDashCooldown() => 5f - Max(AttributeType.Grit) * 0.2f;
    }
}