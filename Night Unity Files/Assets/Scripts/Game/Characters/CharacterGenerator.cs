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
            Weapon w = WeaponGenerator.GenerateWeapon();
            theDriver.AddItemToInventory(w);
            theDriver.SetWeapon(w);
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
                    enduranceMax = Random.Range(70, 100);
                    hunger = 10;
                    thirst = 2f;
                    break;
                case Character.WeightCategory.Light:
                    strengthMax = Random.Range(20, 50);
                    enduranceMax = Random.Range(50, 80);
                    hunger = 15;
                    thirst = 2.5f;
                    break;
                case Character.WeightCategory.Medium:
                    strengthMax = Random.Range(30, 70);
                    enduranceMax = Random.Range(30, 70);
                    hunger = 20;
                    thirst = 3f;
                    break;
                case Character.WeightCategory.Heavy:
                    strengthMax = Random.Range(50, 80);
                    enduranceMax = Random.Range(20, 50);
                    hunger = 25;
                    thirst = 3.5f;
                    break;
                case Character.WeightCategory.VeryHeavy:
                    strengthMax = Random.Range(70, 100);
                    enduranceMax = Random.Range(10, 30);
                    hunger = 30;
                    thirst = 4f;
                    break;
                default:
                    throw new Exceptions.UnrecognisedWeightCategoryException();
            }

            c.Hunger.Val = hunger;
            c.Thirst.Val = thirst;
            c.Strength.Max = strengthMax;
            c.Strength.Val = strengthMax;
            c.Endurance.Max = enduranceMax;
            c.Endurance.Val = enduranceMax;
            c.StarvationTolerance = hunger * 3;
            c.DehydrationTolerance = thirst * 3;

            c.Starvation.Max = c.StarvationTolerance;
            c.Dehydration.Max = c.DehydrationTolerance;

//            c.CharacterUi.ConditionsText.SetFormattingFunction(f => c.GetConditions());
        }
    }
}