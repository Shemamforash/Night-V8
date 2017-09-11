using System.Collections.Generic;
using Characters;
using Game.Combat.Weapons;
using SamsHelper;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Characters
{
    public class CharacterGenerator
    {
        private static List<string> _characterNames;

        public static Character GenerateCharacter()
        {
            Traits.Trait newClass = Traits.GenerateClass();
            string name = GenerateName(newClass);
            Traits.Trait secondaryTrait = Traits.GenerateTrait();
            Character c = GenerateCharacterObject().GetComponent<Character>();
            c.Initialise(name, newClass, secondaryTrait);
            CalculateAttributes(c);
            return c;
        }

        private static void TestCharacterGenerator()
        {
            int charactersToTest = 100;
            Dictionary<Character, List<string>> fails = new Dictionary<Character, List<string>>();
            for (int i = 0; i < charactersToTest; ++i)
            {
                Character c = GenerateCharacter();
                CharacterAttributes attributes = c.CharacterAttributes;
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
            foreach (Character c in fails.Keys)
            {
                Debug.Log(c.CharacterName + " class: " + c.CharacterClass.Name + " trait: " + c.CharacterTrait.Name + " failed following tests:");
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

        public static string GenerateName(Traits.Trait classCharacter)
        {
            return _characterNames[Random.Range(0, _characterNames.Count)];
        }

        public static List<Character> LoadInitialParty()
        {
            _characterNames = new List<string>(Helper.ReadLinesFromFile("names"));
            List<Character> characters = new List<Character>();
            characters.Add(GenerateDriver());
            characters.Add(GenerateCharacter());
            
#if UNITY_EDITOR
//            TestCharacterGenerator();
#endif
            
            return characters;
        }

        private static GameObject GenerateCharacterObject()
        {
            GameObject characterUi = GameObject.Instantiate(Resources.Load("Prefabs/Character Template") as GameObject);
            characterUi.AddComponent<Character>();
            characterUi.transform.SetParent(GameObject.Find("Characters").transform);
            return characterUi;
        }

        private static Character GenerateDriver()
        {
            Character theDriver = GenerateCharacterObject().GetComponent<Character>();
            theDriver.Initialise("Driver", Traits.FindClass("Driver"), Traits.FindTrait("Nomadic"));
            Weapon w = WeaponGenerator.GenerateWeapon();
            theDriver.AddItemToInventory(w);
            theDriver.SetWeapon(w);
            CalculateAttributes(theDriver);
            theDriver.CharacterAttributes.Weight = WeightCategory.Medium;
            return theDriver;
        }

        private static WeightCategory CalculateWeight(Character c)
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
                throw new Exceptions.MaxOrMinWeightExceededException(c.name, targetWeight, c.CharacterClass.Name, c.CharacterTrait.Name);
            }
            return (WeightCategory) targetWeight;
        }

        private static void CalculateAttributes(Character c)
        {
            CharacterAttributes attributes = c.CharacterAttributes;
            int strengthBonusVal = 15;
            int enduranceBonusVal = 15;
            int stabilityBonusVal = 4;
            int intelligenceBonusVal = 4;
            Debug.Log(
                "str " + c.CharacterClass.StrengthBonus + " end: " + c.CharacterClass.EnduranceBonus + " stab: " + c.CharacterClass.StabilityBonus + " int: " + c.CharacterClass.IntelligenceBonus);
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