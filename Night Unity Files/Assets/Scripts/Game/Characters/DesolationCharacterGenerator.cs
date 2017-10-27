using System.Collections.Generic;
using System.Security;
using Game.Combat.Weapons;
using Game.Gear.Weapons;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Characters
{
    public static class CharacterGenerator
    {
        private static readonly List<string> _characterNames;

        private static void TestCharacterGenerator()
        {
            int charactersToTest = 100;
            Dictionary<Character, List<string>> fails = new Dictionary<Character, List<string>>();
            for (int i = 0; i < charactersToTest; ++i)
            {
                Character c = GenerateCharacter();
                BaseAttributes attributes = c.BaseAttributes;
                List<string> failMessages = new List<string>();
                if (!InBounds(attributes.Strength.GetCurrentValue(), 40, 160))
                {
                    failMessages.Add("Strength out of bounds");
                }
                if (!InBounds(attributes.Endurance.GetCurrentValue(), 40, 160))
                {
                    failMessages.Add("Endurance out of bounds");
                }
                if (!InBounds(attributes.Stability.GetCurrentValue(), 5, 25))
                {
                    failMessages.Add("Stability out of bounds");
                }
                if (!InBounds(attributes.Intelligence.GetCurrentValue(), 5, 25))
                {
                    failMessages.Add("Intelligence out of bounds");
                }
                if (failMessages.Count != 0)
                {
                    fails[c] = failMessages;
                }
            }
            foreach (Player playerCharacter in fails.Keys)
            {
                Debug.Log(playerCharacter.Name + " class: " + playerCharacter.CharacterClass.Name + " trait: " + playerCharacter.CharacterTrait.Name + " failed following tests:");
                foreach (string s in fails[playerCharacter])
                {
                    Debug.Log(s);
                }
            }
        }

        private static bool InBounds(float value, int lower, int upper) => value >= lower && value <= upper;

        static CharacterGenerator()
        {
            _characterNames = new List<string>(Helper.ReadLinesFromFile("names"));
        }

        private static string GenerateName() => _characterNames[Random.Range(0, _characterNames.Count)];
        
        public static void LoadInitialParty()
        {
#if UNITY_EDITOR
//            TestCharacterGenerator();
#endif
            WorldState.HomeInventory().AddItem(GenerateDriver());
            WorldState.HomeInventory().AddItem(GenerateCharacter());
        }

        private static Player GenerateCharacterObject(string name, TraitLoader.Trait characterClass, TraitLoader.Trait characterTrait)
        {
            Player playerCharacter = new Player(name, characterClass, characterTrait);
            CalculateAttributes(playerCharacter);
            return playerCharacter;
        }

        public static Player GenerateCharacter()
        {
            TraitLoader.Trait newClass = TraitLoader.GenerateClass();
            TraitLoader.Trait secondaryTrait = TraitLoader.GenerateTrait();
            string name = GenerateName();
            Player playerCharacter = GenerateCharacterObject(name, newClass, secondaryTrait);
            return playerCharacter;
        }

        private static Character GenerateDriver()
        {
            Player theDriver = GenerateCharacterObject("Driver", TraitLoader.FindClass("Crusader"), TraitLoader.FindTrait("Faithless"));
            theDriver.SurvivalAttributes.Weight = WeightCategory.Medium;
            return theDriver;
        }

        private static WeightCategory CalculateWeight(Player playerCharacter)
        {
            int weightOffset = playerCharacter.CharacterClass.WeightModifier + playerCharacter.CharacterTrait.WeightModifier;
            int targetWeight = 2;
            float rand = Random.Range(0f, 1.0f);
            if (rand < 0.25f)
            {
                targetWeight = 1;
            }
            if (rand > 0.75f)
            {
                targetWeight = 3;
            }
            targetWeight += weightOffset;
            if (targetWeight < 0 || targetWeight > 4)
            {
                throw new Exceptions.MaxOrMinWeightExceededException(playerCharacter.Name, targetWeight, playerCharacter.CharacterClass.Name, playerCharacter.CharacterTrait.Name);
            }
            return (WeightCategory) targetWeight;
        }

        private static void CalculateAttributes(Player playerCharacter)
        {
            BaseAttributes attributes = playerCharacter.BaseAttributes;
            int strengthBonusVal = 15;
            int enduranceBonusVal = 15;
            int stabilityBonusVal = 4;
            int intelligenceBonusVal = 4;
            attributes.Strength.Max = Random.Range(80, 120) + playerCharacter.CharacterClass.StrengthBonus * strengthBonusVal + playerCharacter.CharacterTrait.StrengthBonus;
            attributes.Strength.SetCurrentValue(attributes.Strength.Max);
            attributes.Endurance.Max = Random.Range(30, 70) + playerCharacter.CharacterClass.EnduranceBonus * enduranceBonusVal + playerCharacter.CharacterTrait.EnduranceBonus;
            attributes.Endurance.SetCurrentValue(attributes.Endurance.Max);
            attributes.Stability.Max = Random.Range(15, 20) + playerCharacter.CharacterClass.StabilityBonus * stabilityBonusVal + playerCharacter.CharacterTrait.StabilityBonus;
            attributes.Stability.SetCurrentValue(attributes.Stability.Max);
            attributes.Intelligence.Max = Random.Range(15, 20) + playerCharacter.CharacterClass.IntelligenceBonus * intelligenceBonusVal + playerCharacter.CharacterTrait.IntelligenceBonus;
            attributes.Intelligence.SetCurrentValue(attributes.Intelligence.Max);
            playerCharacter.SurvivalAttributes.Weight = CalculateWeight(playerCharacter);
        }
    }
}