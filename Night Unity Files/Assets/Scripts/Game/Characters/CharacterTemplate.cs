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
        public readonly int Fettle, Grit, Will, Focus;
        private static readonly List<CharacterClass> _characterClasses = new List<CharacterClass>();

        public CharacterTemplate(XmlNode classNode, List<CharacterTemplate> templates)
        {
            CharacterClass = StringToClass(classNode.StringFromNode("Name"));
            Grit = classNode.IntFromNode("Grit");
            Will = classNode.IntFromNode("Will");
            Fettle = classNode.IntFromNode("Fettle");
            Focus = classNode.IntFromNode("Focus");
            templates.Add(this);
        }

        public static CharacterClass StringToClass(string className)
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