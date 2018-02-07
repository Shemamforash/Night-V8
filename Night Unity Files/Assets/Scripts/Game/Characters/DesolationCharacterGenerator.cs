using System.Collections.Generic;
using System.Security;
using System.Security.AccessControl;
using System.Xml;
using Game.Gear.Weapons;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Characters
{
    public static class PlayerGenerator
    {
        private static readonly List<CharacterTemplate> Templates = new List<CharacterTemplate>();
        private static bool _loaded;

        public static void LoadTemplates()
        {
            if (_loaded) return;
            string traitText = Resources.Load<TextAsset>("XML/Classes").text;
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
                Templates.Add(newTemplate);
            }

            _loaded = true;
        }

        public static CharacterTemplate FindClass(CharacterClass characterClass)
        {
            foreach (CharacterTemplate t in Templates)
            {
                if (t.CharacterClass == characterClass)
                {
                    return t;
                }
            }

            throw new Exceptions.UnknownTraitException(characterClass.ToString());
        }

        private static void TestCharacterGenerator()
        {
            int charactersToTest = 100;
            Dictionary<Character, List<string>> fails = new Dictionary<Character, List<string>>();
            for (int i = 0; i < charactersToTest; ++i)
            {
                Player.Player c = GenerateRandomCharacter();
                DesolationAttributes attributes = c.Attributes;
                List<string> failMessages = new List<string>();
                if (!InBounds(attributes.Strength.CurrentValue(), 40, 160))
                {
                    failMessages.Add("Strength out of bounds");
                }

                if (!InBounds(attributes.Endurance.CurrentValue(), 40, 160))
                {
                    failMessages.Add("Endurance out of bounds");
                }

                if (!InBounds(attributes.Willpower.CurrentValue(), 5, 25))
                {
                    failMessages.Add("Willpower out of bounds");
                }

                if (!InBounds(attributes.Perception.CurrentValue(), 5, 25))
                {
                    failMessages.Add("Perception out of bounds");
                }

                if (failMessages.Count != 0)
                {
                    fails[c] = failMessages;
                }
            }

            foreach (Player.Player playerCharacter in fails.Keys)
            {
                Debug.Log(playerCharacter.Name + " class: " + playerCharacter.Name + " failed following tests:");
                foreach (string s in fails[playerCharacter])
                {
                    Debug.Log(s);
                }
            }
        }

        private static bool InBounds(float value, int lower, int upper) => value >= lower && value <= upper;

        public static List<Player.Player> LoadInitialParty()
        {
#if UNITY_EDITOR
//            TestCharacterGenerator();
#endif
            List<Player.Player> initialParty = new List<Player.Player> {GenerateCharacter(CharacterClass.Driver), GenerateRandomCharacter()};
            return initialParty;
        }

        public static Player.Player GenerateCharacter(CharacterClass characterClass)
        {
            LoadTemplates();
            CharacterTemplate t = FindClass(characterClass);
            return GenerateCharacterObject(t);
        }

        public static Player.Player GenerateRandomCharacter()
        {
            LoadTemplates();
            CharacterTemplate newTemplate = Templates[Random.Range(0, Templates.Count)];
            Player.Player playerCharacter = GenerateCharacterObject(newTemplate);
            return playerCharacter;
        }

        private static Player.Player GenerateCharacterObject(CharacterTemplate characterTemplate)
        {
            Player.Player playerCharacter = new Player.Player(characterTemplate);
            CalculateAttributes(playerCharacter);
            return playerCharacter;
        }

        private static void CalculateAttributes(Player.Player playerCharacter)
        {
            DesolationAttributes attributes = playerCharacter.Attributes;

            attributes.Endurance.Max = playerCharacter.CharacterTemplate.Endurance;
            playerCharacter.Energy.Max = attributes.Endurance.Max;
            playerCharacter.Energy.SetCurrentValue(attributes.Endurance.Max);
            attributes.Endurance.SetToMax();

            attributes.Strength.Max = playerCharacter.CharacterTemplate.Strength;
            attributes.Strength.SetToMax();

            attributes.Perception.Max = playerCharacter.CharacterTemplate.Perception;
            attributes.Perception.SetToMax();

            attributes.Willpower.Max = playerCharacter.CharacterTemplate.Willpower;
            attributes.Willpower.SetToMax();
        }
    }
}