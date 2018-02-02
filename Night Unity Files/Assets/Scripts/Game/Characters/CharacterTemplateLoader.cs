using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Facilitating.Persistence;
using SamsHelper;
using SamsHelper.Persistence;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Characters
{
    public static class CharacterTemplateLoader
    {
        private static readonly Dictionary<string, CharacterTemplate> TemplateDictionary = new Dictionary<string, CharacterTemplate>();
        private static readonly List<string> ClassNames = new List<string>();

        public static CharacterTemplate GenerateClass()
        {
            return TemplateDictionary[ClassNames[Random.Range(0, ClassNames.Count)]];
        }

        public static void LoadTemplates()
        {
            string traitText = Resources.Load<TextAsset>("Classes").text;
            XmlDocument traitXml = new XmlDocument();
            traitXml.LoadXml(traitText);
            XmlNode root = traitXml.SelectSingleNode("Classes");
            foreach (XmlNode classNode in root.SelectNodes("Class"))
            {
                string name = classNode.SelectSingleNode("Name").InnerText;
                int endurance = int.Parse(classNode.SelectSingleNode("Endurance").InnerText);
                int willpower = int.Parse(classNode.SelectSingleNode("Willpower").InnerText);
                int strength = int.Parse(classNode.SelectSingleNode("Strength").InnerText);
                int perception = int.Parse(classNode.SelectSingleNode("Perception").InnerText);
                List<string> storyLines = new List<string>(classNode.SelectSingleNode("Story").InnerText.Split('.'));
                CharacterTemplate newTemplate = new CharacterTemplate(storyLines, name, strength, endurance, willpower, perception);
                TemplateDictionary[name] = newTemplate;
                ClassNames.Add(name);
            }
        }

        public static CharacterTemplate FindClass(string className)
        {
            try
            {
                return TemplateDictionary[className];
            }
            catch (KeyNotFoundException)
            {
                throw new Exceptions.UnknownTraitException(className);
            }
        }
    }
}