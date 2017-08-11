using System;
using System.Collections.Generic;
using Game.Misc;
using Random = UnityEngine.Random;

namespace Game.Characters
{
    public static class Traits
    {
        private static readonly Dictionary<string, Trait> TraitDictionary = new Dictionary<string, Trait>();
        private static List<string> _traitNames = new List<string>();


        public static Trait GenerateTrait()
        {
            return TraitDictionary[_traitNames[Random.Range(0, _traitNames.Count)]];
        }

        public static void LoadTraits()
        {
            string[] lines = Helper.ReadLinesFromFile("traits");
            for (int i = 0; i < lines.Length; i += 1)
            {
                string line = lines[i];
                Trait newTrait = new Trait();
                while (line != "")
                {
                    string[] attrValuePair = line.Split('=');
                    string attribute = attrValuePair[0];
                    string value = attrValuePair[1];
                    switch (attribute)
                    {
                        case "name":
                            newTrait.Name = value;
                            TraitDictionary[newTrait.Name] = newTrait;
                            _traitNames.Add(value);
                            break;
                        case "str":
                            newTrait.StrengthBonus = float.Parse(value);
                            break;
                        case "int":
                            newTrait.IntelligenceBonus = float.Parse(value);
                            break;
                        case "stab":
                            newTrait.StabilityBonus = float.Parse(value);
                            break;
                        case "end":
                            newTrait.EnduranceBonus = float.Parse(value);
                            break;
                        case "weight":
                            newTrait.WeightModifier = float.Parse(value);
                            break;
                        case "thirst":
                            newTrait.ThirstToleranceModifier = float.Parse(value);
                            break;
                        case "starve":
                            newTrait.HungerToleranceModifier = float.Parse(value);
                            break;
                        case "sight":
                            newTrait.SightModifier = float.Parse(value);
                            break;
                        default:
                            throw new Exceptions.TraitAttributeNotRecognisedException(attribute);
                    }
                    ++i;
                    if (i == lines.Length)
                    {
                        break;
                    }
                    line = lines[i];
                }
            }
        }

        public static Trait FindTrait(string traitName)
        {
            try
            {
                return TraitDictionary[traitName];
            }
            catch (KeyNotFoundException e)
            {
                throw new Exceptions.UnknownTraitException(traitName);
            }
        }

        public class Trait
        {
            public string Name;

            public float StrengthBonus,
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
                traitDetails += GetValueAsString(WeightModifier, "Weight");
                traitDetails += GetValueAsString(StrengthBonus, " str");
                traitDetails += GetValueAsString(IntelligenceBonus, " int");
                traitDetails += GetValueAsString(StabilityBonus, " stb");
                traitDetails += GetValueAsString(EnduranceBonus, " end");
                traitDetails += GetValueAsString(ThirstToleranceModifier, "% thrst");
                traitDetails += GetValueAsString(HungerToleranceModifier, "% hungr");
                traitDetails += GetValueAsString(SightModifier, "% sight");
                return traitDetails;
            }

            public static string GetValueAsString(float value, string suffix)
            {
                string roundedValue = (Math.Round(value * 10) / 10).ToString();
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