﻿using System.Collections.Generic;
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
                Debug.Log(playerCharacter.Name + " class: " + playerCharacter.CharacterClass.Name + " trait: " + playerCharacter.CharacterTrait.Name + " failed following tests:");
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

        private static string GenerateName() => _characterNames[Random.Range(0, _characterNames.Count)];
        
        public static List<Player.Player> LoadInitialParty()
        {
#if UNITY_EDITOR
//            TestCharacterGenerator();
#endif
            List<Player.Player> initialParty = new List<Player.Player> {GenerateDriver(), GenerateCharacter()};
            return initialParty;
        }

        private static Player.Player GenerateCharacterObject(string name, TraitLoader.CharacterClass characterClass, TraitLoader.Trait characterTrait)
        {
            Player.Player playerCharacter = new Player.Player(name, characterClass, characterTrait);
            CalculateAttributes(playerCharacter);
            return playerCharacter;
        }

        public static Player.Player GenerateCharacter()
        {
            TraitLoader.CharacterClass newClass = TraitLoader.GenerateClass();
            TraitLoader.Trait secondaryTrait = TraitLoader.GenerateTrait();
            string name = GenerateName();
            Player.Player playerCharacter = GenerateCharacterObject(name, newClass, secondaryTrait);
            return playerCharacter;
        }

        private static Player.Player GenerateDriver()
        {
            return GenerateCharacterObject("Driver", TraitLoader.FindClass("Crusader"), TraitLoader.FindTrait("Faithless"));
        }

        private static void CalculateWeight(Player.Player playerCharacter)
        {
            int weight = 5;
            weight += playerCharacter.CharacterClass.Weight + playerCharacter.CharacterTrait.Weight;
            playerCharacter.Attributes.Weight = weight;
        }

        private static void CalculateAttributes(Player.Player playerCharacter)
        {
            DesolationAttributes attributes = playerCharacter.Attributes;
            
            attributes.Endurance.Max = playerCharacter.CharacterClass.Endurance + playerCharacter.CharacterTrait.Endurance;
            playerCharacter.Energy.Max = attributes.Endurance.Max;
            playerCharacter.Energy.SetCurrentValue(attributes.Endurance.Max);
            attributes.Endurance.SetToMax();

            attributes.Strength.Max = playerCharacter.CharacterClass.Strength + playerCharacter.CharacterTrait.Strength;
            attributes.Strength.SetToMax();
            
            attributes.Perception.Max = playerCharacter.CharacterClass.Perception + playerCharacter.CharacterTrait.Perception;
            attributes.Perception.SetToMax();
            
            attributes.Willpower.Max = playerCharacter.CharacterClass.Willpower + playerCharacter.CharacterTrait.Willpower;
            attributes.Willpower.SetToMax();
            
            CalculateWeight(playerCharacter);
        }
    }
}