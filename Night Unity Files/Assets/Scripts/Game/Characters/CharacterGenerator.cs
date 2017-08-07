using System;
using System.Collections.Generic;
using Characters;
using Game.Misc;
using Random = UnityEngine.Random;

namespace Game.Characters
{
    public class CharacterGenerator
    {
        public static Character GenerateCharacter()
        {
            ClassCharacter newClass = GenerateClass();
            string name = GenerateName(newClass);
            Traits.Trait secondaryTrait = GenerateTrait();
            Character c = new Character(name, newClass, GenerateWeightCategory(), secondaryTrait);
            CalculateAttributesFromWeight(c);
            return c;
        }

        public static ClassCharacter GenerateClass()
        {
            return ClassCharacter.FindClass("Driver");
        }

        public static string GenerateName(ClassCharacter classCharacter)
        {
            return "Character";
        }

        public static Traits.Trait GenerateTrait()
        {
            return Traits.GenerateTrait();
        }

        public static List<Character> LoadInitialParty()
        {
            List<Character> characters = new List<Character>();
            characters.Add(GenerateDriver());
            characters.Add(GenerateCharacter());
            return characters;
        }

        private static Character GenerateDriver()
        {
            Character theDriver = new Character("Driver", ClassCharacter.FindClass("Driver"),
                Character.WeightCategory.Medium, Traits.FindTrait("Scavenger"));
            CalculateAttributesFromWeight(theDriver);
            return theDriver;
        }

        private static Character.WeightCategory GenerateWeightCategory()
        {
            float rand = Random.Range(0f, 1.0f);
            if (rand < 0.25f)
            {
                return Character.WeightCategory.Light;
            }
            if (rand > 0.75f)
            {
                return Character.WeightCategory.Heavy;
            }
            return Character.WeightCategory.Medium;
        }

        private static void CalculateAttributesFromWeight(Character c)
        {
            float strengthMax;
            float enduranceMax;
            float hunger;
            float thirst;
            switch (c.Weight)
            {
                case Character.WeightCategory.VeryLight:
                    strengthMax = Random.Range(10f, 30f);
                    enduranceMax = Random.Range(7, 10);
                    hunger = 1000;
                    thirst = 2f;
                    break;
                case Character.WeightCategory.Light:
                    strengthMax = Random.Range(20, 50);
                    enduranceMax = Random.Range(5, 8);
                    hunger = 1500;
                    thirst = 2.5f;
                    break;
                case Character.WeightCategory.Medium:
                    strengthMax = Random.Range(30, 70);
                    enduranceMax = Random.Range(3, 7);
                    hunger = 2000;
                    thirst = 3f;
                    break;
                case Character.WeightCategory.Heavy:
                    strengthMax = Random.Range(50, 80);
                    enduranceMax = Random.Range(2, 5);
                    hunger = 2500;
                    thirst = 3.5f;
                    break;
                case Character.WeightCategory.VeryHeavy:
                    strengthMax = Random.Range(70, 100);
                    enduranceMax = Random.Range(1, 3);
                    hunger = 3000;
                    thirst = 4f;
                    break;
                default:
                    throw new Exceptions.UnrecognisedWeightCategoryException();
            }
            c.Strength = new MyFloat(strengthMax, c.CharacterUi.StrengthText, 0, strengthMax);
            Func<float, string> strengthFormatting = (f) => f + " +";
            c.Strength.SetFormattingFunction(strengthFormatting);
            
            c.Endurance = new MyFloat(enduranceMax, c.CharacterUi.EnduranceText, 0, enduranceMax);
            Func<float, string> enduranceFormatting = (f) => f + " ..";
            c.Endurance.SetFormattingFunction(enduranceFormatting);
            
            c.Hunger = new MyFloat(hunger, c.CharacterUi.HungerText);
            c.Thirst = new MyFloat(thirst, c.CharacterUi.ThirstText);
        }
    }
}