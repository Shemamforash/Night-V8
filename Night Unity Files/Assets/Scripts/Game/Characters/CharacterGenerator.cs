using System;
using System.Collections.Generic;
using Characters;
using Game.Combat;
using SamsHelper;
using SamsHelper.ReactiveUI;
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
            c.Initialise(name, newClass, secondaryTrait, GenerateWeightCategory());
            CalculateAttributesFromWeight(c);
            return c;
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
            theDriver.Initialise("Driver", Traits.FindClass("Driver"), Traits.FindTrait("Nomadic"),
                Character.WeightCategory.Medium);
            theDriver.SetWeapon(WeaponGenerator.GenerateWeapon());
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

            c.CharacterUi.StrengthTextDetail.SetFormattingFunction(f => f + "/" + strengthMax + " str");
            c.Strength = new MyFloat(strengthMax, 0, strengthMax);

            c.CharacterUi.EnduranceTextDetail.SetFormattingFunction(f => f + "/" + enduranceMax + " end");
            c.Endurance = new MyFloat(enduranceMax, 0, enduranceMax);

            c.StarvationTolerance = hunger * 3;
            c.DehydrationTolerance = thirst * 3;

            c.CharacterUi.HungerText.SetFormattingFunction(c.GetHungerStatus);
            c.Starvation = new MyFloat(0, 0, c.StarvationTolerance);

            c.CharacterUi.ThirstText.SetFormattingFunction(c.GetThirstStatus);
            c.Dehydration = new MyFloat(0, 0, c.DehydrationTolerance);

            c.CharacterUi.ConditionsText.SetFormattingFunction(f => c.GetConditions());
        }
    }
}