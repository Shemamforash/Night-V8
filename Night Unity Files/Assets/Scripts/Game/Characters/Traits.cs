using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
    public static class Traits
    {
        private static Dictionary<string, Trait> traitDictionary = new Dictionary<string, Trait>();

        public static void LoadTraits()
        {
            string[] lines = Helper.ReadLinesFromFile("traits");
            for (int i = 0; i < lines.Length; i += 10)
            {
                Trait newTrait = new Trait(lines[i], lines[i + 1], lines[i + 2], lines[i + 3], lines[i + 4], lines[i + 5], lines[i + 6], lines[i + 7], lines[i + 8]);
                traitDictionary[lines[i]] = newTrait;
            }
        }

        public static Trait FindTrait(string traitName)
        {
            try
            {
                return traitDictionary[traitName];
            }
            catch (KeyNotFoundException e)
            {
                Debug.Log(traitName + " is not a valid trait name in this dictionary");
                return null;
            }

        }

        public class Trait
        {
            private string name;
            public float strengthBonus = 0f,
             intelligenceBonus = 0f,
             stabilityBonus = 0f,
             enduranceBonus = 0f,
             weightModifier = 0f,
             thirstToleranceModifier = 0f,
             starvationToleranceModifier = 0f,
             sightModifier = 0f;

            public Trait(string name, string strengthBonus, string intelligenceBonus, string stabilityBonus, string enduranceBonus, string weightModifier, string thirstToleranceModifier,
            string starvationToleranceModifier, string sightModifier)
            {
                this.name = name;
                this.strengthBonus = ParseValueFromLine(strengthBonus);
                this.intelligenceBonus = ParseValueFromLine(intelligenceBonus);
                this.stabilityBonus = ParseValueFromLine(stabilityBonus);
                this.enduranceBonus = ParseValueFromLine(enduranceBonus);
                this.weightModifier = ParseValueFromLine(weightModifier);
                this.thirstToleranceModifier = ParseValueFromLine(thirstToleranceModifier);
                this.starvationToleranceModifier = ParseValueFromLine(starvationToleranceModifier);
                this.sightModifier = ParseValueFromLine(sightModifier);
            }

            private static float ParseValueFromLine(string line)
            {
                return float.Parse(line.Split('=')[1]);
            }
        }
    }
}
