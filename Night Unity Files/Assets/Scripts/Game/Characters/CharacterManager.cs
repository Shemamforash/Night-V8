﻿using System.Collections.Generic;
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
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

namespace Game.Characters
{
    public class CharacterManager : Inventory
    {
        public static Player SelectedCharacter;
        public static readonly List<Weapon> Weapons = new List<Weapon>();
        public static readonly List<ArmourPlate> Armour = new List<ArmourPlate>();
        public static readonly List<Accessory> Accessories = new List<Accessory>();
        public static readonly List<Player> Characters = new List<Player>();
        private static readonly List<CharacterTemplate> Templates = new List<CharacterTemplate>();
        private static bool _loaded;

        public CharacterManager() : base("Vehicle")
        {
        }

        public override void Load(XmlNode doc, PersistenceType saveType)
        {
            XmlNode characterManagerNode = doc.SelectSingleNode("CharacterManager");
            XmlNodeList characterNodes = characterManagerNode.SelectNodes("Character");
            foreach (XmlNode characterNode in characterNodes)
            {
//                Character c = new Character();
//                c.Load(characterNode, saveType);
//                _characters.Add(c);
            }
        }

        public XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            if (saveType != PersistenceType.Game) return null;
            doc = base.Save(doc, saveType);
            Debug.Log(doc + " doc");

//            foreach (Player c in Characters)
//            {
//                XmlNode characterNode = SaveController.CreateNodeAndAppend("Character", doc);
//                c.Save(characterNode, saveType);
//            }

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
                Transform characterAreaTransform = GameObject.Find("Character Section").transform.Find("Content").transform;
                if (Items().Count > 0) Helper.AddDelineator(characterAreaTransform);
                GameObject characterObject = Helper.InstantiateUiObject("Prefabs/Character Template", characterAreaTransform);
                characterObject.GetComponent<CharacterView>().SetPlayer(player);
                characterObject.name = player.Name;
            }

            Characters[0].CharacterView.SelectInitial();
            Characters.ForEach(c => c.CharacterView.RefreshNavigation());
        }

        private void AddCharacter(Player playerCharacter)
        {
            Characters.Add(playerCharacter);
            InitialiseCharacterUI();
        }

        protected override void AddItem(InventoryItem item)
        {
            base.AddItem(item);
            if (item is Weapon) Weapons.Add((Weapon) item);
            if (item is ArmourPlate) Armour.Add((ArmourPlate) item);
            if (item is Accessory) Accessories.Add((Accessory) item);
        }

        public void RemoveCharacter(Player playerCharacter)
        {
            Characters.Remove(playerCharacter);
            Characters.ForEach(c => c.CharacterView.RefreshNavigation());
            if (playerCharacter.Name == "Driver") MenuStateMachine.ShowMenu("Game Over Menu");
        }

        protected override InventoryItem RemoveItem(InventoryItem item)
        {
            base.RemoveItem(item);
            Weapons.Remove(item as Weapon);
            Armour.Remove(item as ArmourPlate);
            Accessories.Remove(item as Accessory);
            return item;
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

        private static CharacterTemplate FindClass(CharacterClass characterClass)
        {
            foreach (CharacterTemplate t in Templates)
                if (t.CharacterClass == characterClass)
                    return t;

            throw new Exceptions.UnknownTraitException(characterClass.ToString());
        }

        private Player GenerateDriver()
        {
            Player driver = GenerateCharacter(CharacterClass.Driver);
            Weapon weapon = WeaponGenerator.GenerateWeapon(ItemQuality.Worn, WeaponType.Pistol);
            WeaponGenerationTester.Test();
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
            CharacterTemplate newTemplate = Templates[Random.Range(0, Templates.Count)];
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
            DesolationAttributes attributes = playerCharacter.Attributes;

            attributes.Endurance.Max = playerCharacter.CharacterTemplate.Endurance;
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