using System.Collections.Generic;
using System.Security;
using System.Security.AccessControl;
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
        private static readonly List<string> _characterNames;

        private static void TestCharacterGenerator()
        {
            int charactersToTest = 100;
            Dictionary<Character, List<string>> fails = new Dictionary<Character, List<string>>();
            for (int i = 0; i < charactersToTest; ++i)
            {
                Player.Player c = GenerateCharacter();
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

        static PlayerGenerator()
        {
            _characterNames = new List<string>(Helper.ReadLinesFromFile("names"));
        }

        public static List<Player.Player> LoadInitialParty()
        {
#if UNITY_EDITOR
//            TestCharacterGenerator();
#endif
            List<Player.Player> initialParty = new List<Player.Player> {GenerateDriver(), GenerateCharacter()};
            return initialParty;
        }

        private static Player.Player GenerateCharacterObject(CharacterTemplate characterTemplate)
        {
            Player.Player playerCharacter = new Player.Player(characterTemplate);
            CalculateAttributes(playerCharacter);
            return playerCharacter;
        }

        public static Player.Player GenerateCharacter()
        {
            CharacterTemplate newTemplate = CharacterTemplateLoader.GenerateClass();
            Player.Player playerCharacter = GenerateCharacterObject(newTemplate);
            return playerCharacter;
        }

        private static Player.Player GenerateDriver()
        {
            return GenerateCharacterObject(CharacterTemplateLoader.FindClass("The Driver"));
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