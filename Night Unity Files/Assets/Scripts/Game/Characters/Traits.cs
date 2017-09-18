﻿using System.Collections.Generic;
using SamsHelper;
using Random = UnityEngine.Random;

namespace Game.Characters
{
    public static class Traits
    {
        private static readonly Dictionary<string, Trait> TraitDictionary = new Dictionary<string, Trait>();
        private static readonly Dictionary<string, Trait> ClassDictionary = new Dictionary<string, Trait>();
        private static readonly List<string> TraitNames = new List<string>();
        private static readonly List<string> ClassNames = new List<string>();

        public static Trait GenerateTrait()
        {
            return TraitDictionary[TraitNames[Random.Range(0, TraitNames.Count)]];
        }

        public static Trait GenerateClass()
        {
            return ClassDictionary[ClassNames[Random.Range(0, ClassNames.Count)]];
        }

        private static string DefaultValueIfEmpty(string[] arr, int position, string defaultString)
        {
            return arr[position] == "" ? defaultString : arr[position];
        }
        
        public static void LoadTraits()
        {
            Helper.ConstructObjectsFromCsv("traits", arr =>
            {
                string name = arr[0];
                string traitType = arr[1];
                int strength = int.Parse(DefaultValueIfEmpty(arr, 2, "0"));
                int intelligence = int.Parse(DefaultValueIfEmpty(arr, 3, "0"));
                int stability = int.Parse(DefaultValueIfEmpty(arr, 4, "0"));
                int endurance = int.Parse(DefaultValueIfEmpty(arr, 5, "0"));
                Trait newTrait = new Trait
                {
                    Name = name,
                    StrengthBonus = strength,
                    IntelligenceBonus =  intelligence,
                    StabilityBonus = stability,
                    EnduranceBonus = endurance
                };
                if (traitType == "Trait")
                {
                    TraitDictionary[newTrait.Name] = newTrait;
                    TraitNames.Add(name);
                }
                else
                {
                    ClassNames.Add(name);
                    ClassDictionary[name] = newTrait;
                }
            });
        }

        public static Trait FindTrait(string traitName)
        {
            try
            {
                return TraitDictionary[traitName];
            }
            catch (KeyNotFoundException)
            {
                throw new Exceptions.UnknownTraitException(traitName);
            }
        }

        public static Trait FindClass(string className)
        {
            try
            {
                return ClassDictionary[className];
            }
            catch (KeyNotFoundException)
            {
                throw new Exceptions.UnknownTraitException(className);
            }
        }

        public class Trait
        {
            public string Name;

            public int StrengthBonus,
                IntelligenceBonus,
                StabilityBonus,
                EnduranceBonus,
                WeightModifier,
                ThirstToleranceModifier,
                HungerToleranceModifier,
                SightModifier;

            public string GetTraitDetails()
            {
                string traitDetails = Name + ":";
                traitDetails += GetValueAsString(WeightModifier, "weight");
                traitDetails += GetValueAsString(StrengthBonus, " str");
                traitDetails += GetValueAsString(IntelligenceBonus, " int");
                traitDetails += GetValueAsString(StabilityBonus, " stb");
                traitDetails += GetValueAsString(EnduranceBonus, " end");
                traitDetails += GetValueAsString(ThirstToleranceModifier, "% thrst");
                traitDetails += GetValueAsString(HungerToleranceModifier, "% hungr");
                traitDetails += GetValueAsString(SightModifier, "% sight");
                return traitDetails;
            }

            public static string GetValueAsString(int value, string suffix)
            {
                string roundedValue = value.ToString();
                if (value == 0)
                {
                    return "";
                }
                if (value > 0)
                {
                    roundedValue = "+" + roundedValue;
                }
                return "\n    " + roundedValue + " " + suffix;
            }
        }
    }
}