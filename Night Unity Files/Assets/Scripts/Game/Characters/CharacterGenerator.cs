using System;
using System.Collections.Generic;
using Characters;
using Game.Misc;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Characters
{
    public class CharacterGenerator
    {
        private static List<string> _characterNames;

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
            return _characterNames[Random.Range(0, _characterNames.Count)];
        }

        public static Traits.Trait GenerateTrait()
        {
            return Traits.GenerateTrait();
        }

        public static List<Character> LoadInitialParty()
        {
            _characterNames = new List<string>(Helper.ReadLinesFromFile("names"));
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
                    hunger = 10;
                    thirst = 2f;
                    break;
                case Character.WeightCategory.Light:
                    strengthMax = Random.Range(20, 50);
                    enduranceMax = Random.Range(5, 8);
                    hunger = 15;
                    thirst = 2.5f;
                    break;
                case Character.WeightCategory.Medium:
                    strengthMax = Random.Range(30, 70);
                    enduranceMax = Random.Range(3, 7);
                    hunger = 20;
                    thirst = 3f;
                    break;
                case Character.WeightCategory.Heavy:
                    strengthMax = Random.Range(50, 80);
                    enduranceMax = Random.Range(2, 5);
                    hunger = 25;
                    thirst = 3.5f;
                    break;
                case Character.WeightCategory.VeryHeavy:
                    strengthMax = Random.Range(70, 100);
                    enduranceMax = Random.Range(1, 3);
                    hunger = 30;
                    thirst = 4f;
                    break;
                default:
                    throw new Exceptions.UnrecognisedWeightCategoryException();
            }
            
            c.Hunger = new MyFloat(hunger);
            c.Thirst = new MyFloat(thirst);

            List<TextAssociation> strengthAssociations = new List<TextAssociation>()
            {
                new TextAssociation(c.CharacterUi.StrengthText, f => f + " str", true),
                new TextAssociation(c.CharacterUi.StrengthTextDetail, f => f + "/" + strengthMax + " str", true)
            };
            c.Strength = new MyFloat(strengthMax, strengthAssociations, 0, strengthMax);

            List<TextAssociation> enduranceAssociations = new List<TextAssociation>()
            {
                new TextAssociation(c.CharacterUi.EnduranceText, f => f + " end", true),
                new TextAssociation(c.CharacterUi.EnduranceTextDetail, f => f + "/" + enduranceMax + " end", true)
            };
            c.Strength = new MyFloat(enduranceMax, enduranceAssociations, 0, enduranceMax);

            c.StarvationTolerance = hunger * 3;
            c.DehydrationTolerance = thirst * 3;

            Func<float, string> starvationFormatting = c.GetHungerStatus;
            TextAssociation starvationAssociation = new TextAssociation(c.CharacterUi.HungerText, starvationFormatting, false);
            c.Starvation = new MyFloat(0, starvationAssociation, 0, c.StarvationTolerance);

            Func<float, string> dehydrationFormatting = c.GetThirstStatus;
            TextAssociation dehydrationAssociation =
                new TextAssociation(c.CharacterUi.ThirstText, dehydrationFormatting, false);
            c.Dehydration = new MyFloat(0, dehydrationAssociation, 0, c.DehydrationTolerance);
            
            TextAssociation conditionAssociation = new TextAssociation(c.CharacterUi.ConditionsText, f => c.GetConditions(), false);
            c.Starvation.AddAssociatedText(conditionAssociation);
            c.Dehydration.AddAssociatedText(conditionAssociation);
        }
    }
}