using System.Collections.Generic;
using System.Linq;
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
        public static Player Wanderer;
        public static Player SelectedCharacter;
        public static readonly List<Player> Characters = new List<Player>();
        private static readonly List<CharacterTemplate> Templates = new List<CharacterTemplate>();
        private static bool _loaded;

        public static void Reset()
        {
            SelectedCharacter = null;
            for (int i = Characters.Count - 1; i >= 0; --i)
                RemoveCharacter(Characters[i]);
            GenerateDriver();
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
            Characters.ForEach(c => c.CharacterView()?.RefreshNavigation());
        }

        public static void RemoveCharacter(Player playerCharacter)
        {
            GameObject.Destroy(playerCharacter.CharacterView());
            Characters.Remove(playerCharacter);
            Characters.ForEach(c => c.CharacterView().RefreshNavigation());
        }

        public static void SelectCharacter(Player player)
        {
            SelectedCharacter = player;
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
            Weapon weapon = WeaponGenerator.GenerateWeapon(ItemQuality.Dark, WeaponType.Pistol);
            Wanderer.EquipWeapon(weapon);
            AddCharacter(Wanderer);
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

            attributes.SetMax(AttributeType.Grit, playerCharacter.CharacterTemplate.Grit);
            attributes.SetMax(AttributeType.Fettle, playerCharacter.CharacterTemplate.Fettle);
            attributes.SetMax(AttributeType.Focus, playerCharacter.CharacterTemplate.Focus);
            attributes.SetMax(AttributeType.Will, playerCharacter.CharacterTemplate.Will);

//#if UNITY_EDITOR
            int max = 20;
            attributes.SetMax(AttributeType.Grit, Random.Range(6, 12));
            attributes.SetMax(AttributeType.Fettle, Random.Range(6, 12));
            attributes.SetMax(AttributeType.Focus, Random.Range(6, 12));
            attributes.SetMax(AttributeType.Will, Random.Range(6, 12));
//#endif

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