using System;
using System.Collections.Generic;
using System.Xml;
using Extensions;
using SamsHelper;

namespace Game.Characters
{
	public class CharacterTemplate
	{
		private static readonly List<CharacterClass> _characterClasses = new List<CharacterClass>();
		public readonly         CharacterClass       CharacterClass;
		public readonly         int                  Life, Will;

		public CharacterTemplate(XmlNode classNode, List<CharacterTemplate> templates)
		{
			CharacterClass = StringToClass(classNode.ParseString("Name"));
			Will           = classNode.ParseInt("Will");
			Life           = classNode.ParseInt("Life");
			templates.Add(this);
		}

		public static CharacterClass StringToClass(string className)
		{
			if (_characterClasses.Count == 0)
			{
				foreach (CharacterClass c in Enum.GetValues(typeof(CharacterClass))) _characterClasses.Add(c);
			}

			foreach (CharacterClass c in _characterClasses)
			{
				if (className.Contains(c.ToString()))
				{
					return c;
				}
			}

			throw new Exceptions.UnknownCharacterClassException(className);
		}
	}
}