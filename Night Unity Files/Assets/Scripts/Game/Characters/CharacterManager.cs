using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using Game.Gear;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.Persistence;
using UnityEngine;

namespace Game.Characters
{
    public class CharacterManager : Inventory
    {
        public static Player SelectedCharacter;
        public static readonly List<Player> Characters = new List<Player>();
        private static readonly List<CharacterTemplate> Templates = new List<CharacterTemplate>();
        private static bool _loaded;
        private List<Building> _buildings = new List<Building>();

        public CharacterManager() : base("Vehicle")
        {
        }

        public override void Load(XmlNode doc, PersistenceType saveType)
        {
            XmlNode characterManagerNode = doc.GetNode("CharacterManager");
            foreach (XmlNode characterNode in Helper.GetNodesWithName(characterManagerNode, "Character"))
            {
//                Character c = new Character();
//                c.Load(characterNode, saveType);
//                _characters.Add(c);
            }
        }

        public override XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            if (saveType != PersistenceType.Game) return null;
            doc = base.Save(doc, saveType);
            foreach (Player c in Characters)
            {
                XmlNode characterNode = SaveController.CreateNodeAndAppend("Character", doc);
                c.Save(characterNode, saveType);
            }
            return doc;
        }

        public void Start()
        {
            LoadTemplates();
            SaveController.AddPersistenceListener(this);
            if (Characters.Count == 0) AddCharacter(GenerateDriver());
            InitialiseCharacterUI();
        }

        private void InitialiseCharacterUI()
        {
            foreach (Player player in Characters)
            {
                if (player.CharacterView != null) continue;
                Transform characterAreaTransform = GameObject.Find("Character Section").transform;
                if (Items().Count > 0) Helper.AddDelineator(characterAreaTransform);
                GameObject characterObject = Helper.InstantiateUiObject("Prefabs/Character Template", characterAreaTransform);
                characterObject.GetComponent<CharacterView>().SetPlayer(player);
                characterObject.name = player.Name;
            }

            Characters[0].CharacterView.SelectInitial();
            Characters.ForEach(c => c.CharacterView.RefreshNavigation());
        }

        public static void AddCharacter(Player playerCharacter)
        {
            Characters.Add(playerCharacter);
        }

        public static void RemoveCharacter(Player playerCharacter)
        {
            if (playerCharacter.CharacterTemplate.CharacterClass == CharacterClass.Wanderer)
            {
                SceneChanger.ChangeScene("Game Over");
            }
            else
            {
                Characters.Remove(playerCharacter);
                Characters.ForEach(c => c.CharacterView.RefreshNavigation());
            }
        }

        public void UpdateBuildings()
        {
            _buildings.ForEach(b => b.Update());
        }
        
        public static void ExitCharacter(Player character)
        {
            character.CharacterView.SwitchToSimpleView();
        }

        public static void SelectCharacter(Player player)
        {
            SelectedCharacter = player;
            player.CharacterView.SwitchToDetailedView();
        }

        public static Player PreviousCharacter(Player character)
        {
            for (int i = 0; i < Characters.Count; ++i)
            {
                if (Characters[i] != character) continue;
                if (i != 0) return Characters[i - 1];

                break;
            }

            return null;
        }

        public static Player NextCharacter(Player character)
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

        private static int AttributeCapStringToValue(string nodeName, XmlNode node)
        {
            string capString = node.GetNodeText(nodeName);
            switch (capString)
            {
                case "+":
                    return 15;
                case "++":
                    return 20;
                default:
                    return 10;
            }
        }

        private static CharacterTemplate FindClass(CharacterClass characterClass)
        {
            foreach (CharacterTemplate t in Templates)
            {
                if (t.CharacterClass == characterClass)
                {
                    return t;
                }
            }

            throw new Exceptions.UnknownCharacterClassException(characterClass.ToString());
        }

        private Player GenerateDriver()
        {
            Player driver = GenerateCharacter(CharacterClass.Wanderer);
            Weapon weapon = WeaponGenerator.GenerateWeapon(ItemQuality.Worn, WeaponType.SMG);
//            WeaponGenerationTester.Test();
//            Inscription.Test();
            driver.EquipWeapon(weapon);
            return driver;
        }

        private static Player GenerateCharacter(CharacterClass characterClass)
        {
            LoadTemplates();
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

            attributes.SetMax(AttributeType.Endurance, 10);//playerCharacter.CharacterTemplate.Endurance);
            attributes.Get(AttributeType.Endurance).SetToMax();

            attributes.SetMax(AttributeType.Strength, playerCharacter.CharacterTemplate.Strength);
            attributes.Get(AttributeType.Strength).SetToMax();

            attributes.SetMax(AttributeType.Perception, playerCharacter.CharacterTemplate.Perception);
            attributes.Get(AttributeType.Perception).SetToMax();

            attributes.SetMax(AttributeType.Willpower, playerCharacter.CharacterTemplate.Willpower);
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

        public void AddBuilding(Building building)
        {
            _buildings.Add(building);
        }

        public List<Building> Buildings()
        {
            return _buildings;
        }
    }
}