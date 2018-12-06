using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.Persistence;
using Game.Combat.Player;
using Game.Gear;
using Game.Gear.Weapons;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Characters
{
    public static class CharacterManager
    {
        public static Player Wanderer;
        public static Player SelectedCharacter;
        public static readonly List<Player> Characters = new List<Player>();
        private static readonly List<CharacterTemplate> Templates = new List<CharacterTemplate>();
        private static bool _loaded;

        public static void Reset(bool includeDriver = true)
        {
            SelectedCharacter = null;
            Characters.Clear();
            if (!includeDriver) return;
            LoadTemplates(true);
            GenerateDriver();
        }

        public static void Load(XmlNode doc)
        {
            LoadTemplates(true);
            Reset(false);
            XmlNode characterManagerNode = doc.GetNode("Characters");
            foreach (XmlNode characterNode in Helper.GetNodesWithName(characterManagerNode, "Character"))
            {
                string className = characterNode.StringFromNode("CharacterClass");
                CharacterTemplate template = FindClass(className);
                Templates.Remove(template);
                Player player = new Player(template);
                player.Load(characterNode);
                AddCharacter(player);
                if (player.CharacterTemplate.CharacterClass == CharacterClass.Wanderer) Wanderer = player;
            }

            Assert.IsFalse(Templates.Any(t => t.CharacterClass == CharacterClass.Wanderer));
        }

        public static void Save(XmlNode doc)
        {
            doc = doc.CreateChild("Characters");
            foreach (Player c in Characters) c.Save(doc);
        }

        public static void Start()
        {
            Transform characterAreaTransform = GameObject.Find("Character Section").transform;
            for (int i = 0; i < 3; i++)
            {
                CharacterView characterView = characterAreaTransform.FindChildWithName<CharacterView>("Character " + i);
                Player player = null;
                if (i < Characters.Count) player = Characters[i];
                characterView.SetPlayer(player);
            }

            Characters[0].CharacterView().SelectInitial();
        }

        public static void AddCharacter(Player playerCharacter)
        {
            Characters.Add(playerCharacter);
        }

        public static void RemoveCharacter(Player playerCharacter)
        {
            playerCharacter.CharacterView().SetPlayer(null);
            Characters.Remove(playerCharacter);
        }

        public static void SelectCharacter(Player player)
        {
            SelectedCharacter = player;
        }

        private static void LoadTemplates(bool force = false)
        {
            if (!force && _loaded) return;
            Templates.Clear();
            XmlNode root = Helper.OpenRootNode("Classes");
            foreach (XmlNode classNode in Helper.GetNodesWithName(root, "Class"))
                new CharacterTemplate(classNode, Templates);
            _loaded = true;
        }

        private static CharacterTemplate FindClass(CharacterClass characterClass)
        {
            LoadTemplates();
            CharacterTemplate template = Templates.FirstOrDefault(t => t.CharacterClass == characterClass);
            if (template != null) return template;
            throw new Exceptions.UnknownCharacterClassException(characterClass.ToString());
        }

        private static CharacterTemplate FindClass(string characterClass)
        {
            LoadTemplates();
            CharacterTemplate template = Templates.FirstOrDefault(t => t.CharacterClass.ToString() == characterClass);
            if (template != null) return template;
            throw new Exceptions.UnknownCharacterClassException(characterClass);
        }

        private static void GenerateDriver()
        {
            Wanderer = GenerateCharacter(CharacterClass.Wanderer);
            Templates.Remove(Wanderer.CharacterTemplate);
            Weapon weapon = WeaponGenerator.GenerateWeapon(ItemQuality.Dark, WeaponType.Pistol);
            Inventory.Move(weapon);
            Wanderer.EquipWeapon(weapon);
            AddCharacter(Wanderer);

            CharacterAttributes attributes = Wanderer.Attributes;
            attributes.Get(AttributeType.Grit).SetCurrentValue(2);
            attributes.Get(AttributeType.Fettle).SetCurrentValue(3);
            attributes.Get(AttributeType.Focus).SetCurrentValue(3);
            attributes.Get(AttributeType.Will).SetCurrentValue(2);
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
            Assert.IsFalse(Templates.Any(t => t.CharacterClass == CharacterClass.Wanderer));
            Player playerCharacter = GenerateCharacterObject(newTemplate);
            Weapon weapon = WeaponGenerator.GenerateWeapon();
            Inventory.Move(weapon);
            playerCharacter.EquipWeapon(weapon);
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

            attributes.SetMax(AttributeType.Grit, playerCharacter.CharacterTemplate.Grit);
            attributes.SetMax(AttributeType.Fettle, playerCharacter.CharacterTemplate.Fettle);
            attributes.SetMax(AttributeType.Focus, playerCharacter.CharacterTemplate.Focus);
            attributes.SetMax(AttributeType.Will, playerCharacter.CharacterTemplate.Will);

#if UNITY_EDITOR
            attributes.SetMax(AttributeType.Grit, Random.Range(6, 12));
            attributes.SetMax(AttributeType.Fettle, Random.Range(6, 12));
            attributes.SetMax(AttributeType.Focus, Random.Range(6, 12));
            attributes.SetMax(AttributeType.Will, Random.Range(6, 12));
#endif

            attributes.Get(AttributeType.Grit).SetToMax();
            attributes.Get(AttributeType.Fettle).SetToMax();
            attributes.Get(AttributeType.Focus).SetToMax();
            attributes.Get(AttributeType.Will).SetToMax();
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
    }
}