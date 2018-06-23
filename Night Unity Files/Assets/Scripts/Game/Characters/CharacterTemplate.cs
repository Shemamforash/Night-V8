using System;
using System.Collections.Generic;
using SamsHelper;

namespace Game.Characters
{
    public class CharacterTemplate
    {
        public readonly CharacterClass CharacterClass;
        public readonly List<string> StoryLines;
        public readonly int Strength, Endurance, Willpower, Perception;
        public readonly int StrengthCap, EnduranceCap, WillpowerCap, PerceptionCap;
        private static readonly List<CharacterClass> _characterClasses = new List<CharacterClass>();

        public CharacterTemplate(List<string> storyLines, string name, int strength, int endurance, int willpower, int perception, int strengthCap, int enduranceCap, int willpowerCap,
            int perceptionCap)
        {
            StoryLines = storyLines;
            CharacterClass = StringToClass(name);
            Strength = strength;
            Endurance = endurance;
            Willpower = willpower;
            Perception = perception;
            StrengthCap = strengthCap;
            EnduranceCap = enduranceCap;
            WillpowerCap = willpowerCap;
            PerceptionCap = perceptionCap;
        }

        private static CharacterClass StringToClass(string className)
        {
            if (_characterClasses.Count == 0)
            {
                foreach (CharacterClass c in Enum.GetValues(typeof(CharacterClass))) _characterClasses.Add(c);
            }

            foreach (CharacterClass c in _characterClasses)
                if (className.Contains(c.ToString()))
                    return c;

            throw new Exceptions.UnknownCharacterClassException(className);
        }
    }
}