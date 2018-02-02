using System;
using System.Collections.Generic;
using SamsHelper;

namespace Game.Characters
{
    public class CharacterTemplate
    {
        public readonly List<string> StoryLines;
        public CharacterClass CharacterClass;
        public int Strength, Endurance, Willpower, Perception;

        public CharacterTemplate(List<string> storyLines, string name, int strength, int endurance, int willpower, int perception)
        {
            StoryLines = storyLines;
            CharacterClass = StringToClass(name);
            Strength = strength;
            Endurance = endurance;
            Willpower = willpower;
            Perception = perception;
        }

        private static CharacterClass StringToClass(string className)
        {
            foreach (CharacterClass c in Enum.GetValues(typeof(CharacterClass)))
            {
                if (className.Contains(c.ToString()))
                {
                    return c;
                }
            }

            throw new Exceptions.UnknownTraitException(className);
        }
            
    }
}