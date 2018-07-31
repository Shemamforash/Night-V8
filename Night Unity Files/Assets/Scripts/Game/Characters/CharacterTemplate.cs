using System;
using System.Collections.Generic;
using System.Xml;
using SamsHelper;
using SamsHelper.Libraries;

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
            CharacterClass = StringToClass(classNode.GetNodeText("Name"));
            Endurance = classNode.IntFromNode("Endurance");
            Willpower = classNode.IntFromNode("Willpower");
            Strength = classNode.IntFromNode("Strength");
            Perception = classNode.IntFromNode("Perception");
            StoryLines = new List<string>(classNode.GetNodeText("Story").Split('.'));
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