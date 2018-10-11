using System.Collections.Generic;
using System.Xml;
using Game.Gear;
using Game.Gear.Weapons;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Characters
{
    public static class CharacterManager
    {
        public static Player SelectedCharacter;
        public static readonly List<Player> Characters = new List<Player>();
        private static readonly List<CharacterTemplate> Templates = new List<CharacterTemplate>();
        private static bool _loaded;

        public static void Reset()
        {
            SelectedCharacter = null;
            for (int i = Characters.Count - 1; i >= 0; --i)
                RemoveCharacter(Characters[i]);
            AddCharacter(GenerateDriver());
        }

        public static void Load(XmlNode doc)
        {
            Reset();
            XmlNode characterManagerNode = doc.GetNode("Inventory");
            foreach (XmlNode characterNode in Helper.GetNodesWithName(characterManagerNode, "Character"))
            {
                string className = characterNode.StringFromNode("CharacterClass");
                CharacterTemplate template = FindClass(className);
                Player player = new Player(template);
                player.Load(characterNode);
                AddCharacter(player);
            }
        }

        public static void Save(XmlNode doc)
        {
            foreach (Player c in Characters) c.Save(doc);
        }

        public static void Start()
        {
            for (int i = 0; i < Characters.Count; i++)
            {
                Player player = Characters[i];
                if (player.CharacterView() != null) continue;
                Transform characterAreaTransform = GameObject.Find("Character Section").transform;
                if (i > 1) Helper.AddDelineator(characterAreaTransform);
                GameObject characterObject = Helper.InstantiateUiObject("Prefabs/Character Template", characterAreaTransform);
                characterObject.GetComponent<CharacterView>().SetPlayer(player);
                characterObject.name = player.Name;
            }

            Characters[0].CharacterView().SelectInitial();
            Characters.ForEach(c => c.CharacterView().RefreshNavigation());
        }

        public static void AddCharacter(Player playerCharacter)
        {
            Characters.Add(playerCharacter);
        }

        public static void RemoveCharacter(Player playerCharacter)
        {
            GameObject.Destroy(playerCharacter.CharacterView());
            Characters.Remove(playerCharacter);
            Characters.ForEach(c => c.CharacterView().RefreshNavigation());
        }

        private static void ExitCharacter(Player character)
        {
            character.CharacterView().SwitchToSimpleView();
        }

        public static void SelectCharacter(Player player)
        {
            SelectedCharacter = player;
            player.CharacterView().SwitchToDetailedView();
        }

        private static Player PreviousCharacter(Player character)
        {
            for (int i = 0; i < Characters.Count; ++i)
            {
                if (Characters[i] != character) continue;
                if (i != 0) return Characters[i - 1];

                break;
            }

            return null;
        }

        private static Player NextCharacter(Player character)
        {
            for (int i = 0; i < Characters.Count; ++i)
            {
                if (Characters[i] != character) continue;
                if (i != Characters.Count - 1) return Characters[i + 1];

                break;
            }

            return null;
        }

        private static void LoadTemplates()
        {
            if (_loaded) return;
            XmlNode root = Helper.OpenRootNode("Classes");
            foreach (XmlNode classNode in Helper.GetNodesWithName(root, "Class"))
                new CharacterTemplate(classNode, Templates);
            _loaded = true;
        }

        private static CharacterTemplate FindClass(CharacterClass characterClass)
        {
            LoadTemplates();
            foreach (CharacterTemplate t in Templates)
            {
                if (t.CharacterClass == characterClass)
                {
                    return t;
                }
            }

            throw new Exceptions.UnknownCharacterClassException(characterClass.ToString());
        }

        private static CharacterTemplate FindClass(string characterClass)
        {
            LoadTemplates();
            foreach (CharacterTemplate t in Templates)
            {
                if (t.CharacterClass.ToString() == characterClass)
                {
                    return t;
                }
            }

            throw new Exceptions.UnknownCharacterClassException(characterClass);
        }

        private static Player GenerateDriver()
        {
            Player driver = GenerateCharacter(CharacterClass.Wanderer);
            Weapon weapon = WeaponGenerator.GenerateWeapon(ItemQuality.Dark, WeaponType.Pistol);
            driver.EquipWeapon(weapon);
            return driver;
        }

        private static Player GenerateCharacter(CharacterClass characterClass)
        {
            CharacterTemplate t = FindClass(characterClass);
            return GenerateCharacterObject(t);
        }

        public static Player GenerateRandomCharacter()
        {
            LoadTemplates();
            CharacterTemplate newTemplate = Templates.RemoveRandom();
            Player playerCharacter = GenerateCharacterObject(newTemplate);
            return playerCharacter;
        }

        private static Player GenerateCharacterObject(CharacterTemplate characterTemplate)
        {
            Player playerCharacter = new Player(characterTemplate);
            CalculateAttributes(playerCharacter);
            return playerCharacter;
        }

        private static void CalculateAttributes(Player playerCharacter)
        {
            CharacterAttributes attributes = playerCharacter.Attributes;

            attributes.SetMax(AttributeType.Endurance, playerCharacter.CharacterTemplate.Endurance);
            attributes.SetMax(AttributeType.Strength, playerCharacter.CharacterTemplate.Strength);
            attributes.SetMax(AttributeType.Perception, playerCharacter.CharacterTemplate.Perception);
            attributes.SetMax(AttributeType.Willpower, playerCharacter.CharacterTemplate.Willpower);

#if UNITY_EDITOR
            int max = 20;
            attributes.SetMax(AttributeType.Endurance, max);
            attributes.SetMax(AttributeType.Strength, max);
            attributes.SetMax(AttributeType.Perception, max);
            attributes.SetMax(AttributeType.Willpower, max);
#endif

            attributes.Get(AttributeType.Endurance).SetToMax();
            attributes.Get(AttributeType.Strength).SetToMax();
            attributes.Get(AttributeType.Perception).SetToMax();
            attributes.Get(AttributeType.Willpower).SetToMax();
        }

        public static void Update()
        {
            for (int i = Characters.Count - 1; i >= 0; --i)
            {
                Player c = Characters[i];
                c.Update();
                c.Attributes.UpdateThirstAndHunger();
                if (c.IsDead)
                    RemoveCharacter(c);
            }
        }

        public static void SelectPreviousCharacter(bool selectGear)
        {
            Player previousPlayer = PreviousCharacter(SelectedCharacter);
            if (previousPlayer == null) return;
            ExitCharacter(SelectedCharacter);
            SelectCharacter(previousPlayer);
            if (selectGear)
            {
                previousPlayer.CharacterView().ArmourController.EnhancedButton.Select();
            }
            else
            {
                previousPlayer.CharacterView().SelectLast();
            }
        }

        public static void SelectNextCharacter(bool selectGear)
        {
            Player nextCharacter = NextCharacter(SelectedCharacter);
            if (nextCharacter == null) return;
            ExitCharacter(SelectedCharacter);
            SelectCharacter(nextCharacter);
            if (selectGear)
            {
                nextCharacter.CharacterView().WeaponController.EnhancedButton.Select();
            }
            else
            {
                nextCharacter.CharacterView().SelectInitial();
            }
        }
    }
}