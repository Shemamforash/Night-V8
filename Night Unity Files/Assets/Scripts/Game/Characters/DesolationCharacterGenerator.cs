using System.Collections.Generic;
using System.Security;
using Game.Combat.Weapons;
using Game.Gear.Weapons;
using Game.World;
using SamsHelper;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Characters
{
    public static class DesolationCharacterGenerator
    {
        private static List<string> _characterNames;

        private static void TestCharacterGenerator()
        {
            int charactersToTest = 100;
            Dictionary<DesolationCharacter, List<string>> fails = new Dictionary<DesolationCharacter, List<string>>();
            for (int i = 0; i < charactersToTest; ++i)
            {
                DesolationCharacter c = GenerateCharacter();
                DesolationCharacterAttributes attributes = c.Attributes;
                List<string> failMessages = new List<string>();
                if (!InBounds(attributes.Strength.Val, 40, 160))
                {
                    failMessages.Add("Strength out of bounds");
                }
                if (!InBounds(attributes.Endurance.Val, 40, 160))
                {
                    failMessages.Add("Endurance out of bounds");
                }
                if (!InBounds(attributes.Stability.Val, 5, 25))
                {
                    failMessages.Add("Stability out of bounds");
                }
                if (!InBounds(attributes.Intelligence.Val, 5, 25))
                {
                    failMessages.Add("Intelligence out of bounds");
                }
                if (failMessages.Count != 0)
                {
                    fails[c] = failMessages;
                }
            }
            foreach (DesolationCharacter c in fails.Keys)
            {
                Debug.Log(c.Name + " class: " + c.CharacterClass.Name + " trait: " + c.CharacterTrait.Name + " failed following tests:");
                foreach (string s in fails[c])
                {
                    Debug.Log(s);
                }
            }
        }

        private static bool InBounds(int value, int lower, int upper)
        {
            return value >= lower && value <= upper;
        }

        private static string GenerateName(TraitLoader.Trait classCharacter)
        {
            return _characterNames[Random.Range(0, _characterNames.Count)];
        }

        public static void LoadInitialParty()
        {
            _characterNames = new List<string>(Helper.ReadLinesFromFile("names"));
#if UNITY_EDITOR
//            TestCharacterGenerator();
#endif
            WorldState.HomeInventory.AddItem(GenerateDriver());
            WorldState.HomeInventory.AddItem(GenerateCharacter());
        }

        private static DesolationCharacter GenerateCharacterObject(string name, TraitLoader.Trait characterClass, TraitLoader.Trait characterTrait)
        {
            GameObject characterObject = Helper.InstantiateUiObject("Prefabs/Character Template", GameObject.Find("Character Section").transform.Find("Content").transform);
            DesolationCharacter newCharacter = new DesolationCharacter(name, characterClass, characterTrait, characterObject);
            CalculateAttributes(newCharacter);
            return newCharacter;
        }
        
        public static DesolationCharacter GenerateCharacter()
        {
            TraitLoader.Trait newClass = TraitLoader.GenerateClass();
            TraitLoader.Trait secondaryTrait = TraitLoader.GenerateTrait();
            string name = GenerateName(newClass);
            DesolationCharacter c = GenerateCharacterObject(name, newClass, secondaryTrait);
            return c;
        }

        private static DesolationCharacter GenerateDriver()
        {
            DesolationCharacter theDriver = GenerateCharacterObject("Driver", TraitLoader.FindClass("Crusader"), TraitLoader.FindTrait("Faithless"));
            theDriver.Attributes.Weight = WeightCategory.Medium;
            return theDriver;
        }

        private static WeightCategory CalculateWeight(DesolationCharacter c)
        {
            int weightOffset = c.CharacterClass.WeightModifier + c.CharacterTrait.WeightModifier;
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
                throw new Exceptions.MaxOrMinWeightExceededException(c.Name, targetWeight, c.CharacterClass.Name, c.CharacterTrait.Name);
            }
            return (WeightCategory) targetWeight;
        }

        private static void CalculateAttributes(DesolationCharacter c)
        {
            DesolationCharacterAttributes attributes = c.Attributes;
            int strengthBonusVal = 15;
            int enduranceBonusVal = 15;
            int stabilityBonusVal = 4;
            int intelligenceBonusVal = 4;
            attributes.Strength.Max = Random.Range(80, 120) + c.CharacterClass.StrengthBonus * strengthBonusVal + c.CharacterTrait.StrengthBonus;
            attributes.Strength.Val = attributes.Strength.Max;
            attributes.Endurance.Max = Random.Range(30, 70) + c.CharacterClass.EnduranceBonus * enduranceBonusVal + c.CharacterTrait.EnduranceBonus;
            attributes.Endurance.Val = attributes.Endurance.Max;
            attributes.Stability.Max = Random.Range(15, 20) + c.CharacterClass.StabilityBonus * stabilityBonusVal + c.CharacterTrait.StabilityBonus;
            attributes.Stability.Val = attributes.Stability.Max;
            attributes.Intelligence.Max = Random.Range(15, 20) + c.CharacterClass.IntelligenceBonus * intelligenceBonusVal + c.CharacterTrait.IntelligenceBonus;
            attributes.Intelligence.Val = attributes.Intelligence.Max;
            attributes.Weight = CalculateWeight(c);
        }
    }
}