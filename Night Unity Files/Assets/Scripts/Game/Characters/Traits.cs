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
            TextAsset traitFile = Resources.Load("traits") as TextAsset;
            string[] lines = traitFile.text.Split('\n');
            for (int i = 0; i < lines.Length; i += 10)
            {
                Trait newTrait = new Trait(lines[0], lines[1], lines[2], lines[3], lines[4], lines[5], lines[6], lines[7], lines[8]);
            }
        }

		public static Trait FindTrait(string traitName){
			return traitDictionary[traitName];
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
