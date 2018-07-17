using System;
using System.Collections.Generic;
using System.Xml;
using SamsHelper;

namespace Game.Characters
{
    public class CharacterTemplate
    {
        public readonly CharacterClass CharacterClass;
        public readonly List<string> StoryLines;
        public readonly int Strength, Endurance, Willpower, Perception;
        private static readonly List<CharacterClass> _characterClasses = new List<CharacterClass>();

        public CharacterTemplate(XmlNode classNode, List<CharacterTemplate> templates)
        {
            CharacterClass = StringToClass(classNode.SelectSingleNode("Name").InnerText);
            Endurance = int.Parse(classNode.SelectSingleNode("Endurance").InnerText);
            Willpower = int.Parse(classNode.SelectSingleNode("Willpower").InnerText);
            Strength = int.Parse(classNode.SelectSingleNode("Strength").InnerText);
            Perception = int.Parse(classNode.SelectSingleNode("Perception").InnerText);
            StoryLines = new List<string>(classNode.SelectSingleNode("Story").InnerText.Split('.'));
            templates.Add(this);
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