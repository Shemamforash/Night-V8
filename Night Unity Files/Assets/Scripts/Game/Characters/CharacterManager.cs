using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.Persistence;
using Game.Exploration.Regions;
using Game.Gear;
using Game.Gear.Weapons;
using Extensions;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Characters
{
	public static class CharacterManager
	{
		public static           Player                  Wanderer;
		public static           Player                  AlternateCharacter;
		public static           Player                  SelectedCharacter;
		private static readonly List<CharacterTemplate> Templates = new List<CharacterTemplate>();
		private static          bool                    _loaded;

		public static void Reset(bool includeDriver = true)
		{
			SelectedCharacter  = null;
			AlternateCharacter = null;
			if (!includeDriver) return;
			LoadTemplates(true);
			GenerateWanderer();
			SelectedCharacter = Wanderer;
		}

		public static void Load(XmlNode doc)
		{
			LoadTemplates(true);
			Reset(false);
			XmlNode     characterManagerNode = doc.GetChild("Characters");
			XmlNodeList nodes                = characterManagerNode.SelectNodes("Character");
			foreach (XmlNode characterNode in nodes) LoadCharacter(characterNode);

			CharacterClass selectedCharacterClass = (CharacterClass) characterManagerNode.ParseInt("SelectedCharacter");
			SelectedCharacter = selectedCharacterClass == CharacterClass.Wanderer ? Wanderer : AlternateCharacter;

			Assert.IsFalse(Templates.Any(t => t.CharacterClass == CharacterClass.Wanderer));
		}

		private static void LoadCharacter(XmlNode characterNode)
		{
			if (characterNode == null) return;
			string            className = characterNode.ParseString("CharacterClass");
			CharacterTemplate template  = FindClass(className);
			Templates.Remove(template);
			Player loadedCharacter = new Player(template);
			loadedCharacter.Load(characterNode);
			if (loadedCharacter.CharacterTemplate.CharacterClass == CharacterClass.Wanderer)
			{
				Wanderer = loadedCharacter;
			}
			else
			{
				AlternateCharacter = loadedCharacter;
			}
		}

		public static void Save(XmlNode doc)
		{
			doc = doc.CreateChild("Characters");
			AlternateCharacter?.Save(doc);
			Wanderer?.Save(doc);
			doc.CreateChild("SelectedCharacter", (int) SelectedCharacter.CharacterTemplate.CharacterClass);
		}

		public static void Start()
		{
			Transform     characterAreaTransform = GameObject.Find("Character Section").transform;
			CharacterView characterView          = characterAreaTransform.FindChildWithName<CharacterView>("Character 0");
			characterView.SetPlayer(Wanderer);
			characterView = characterAreaTransform.FindChildWithName<CharacterView>("Character 1");
			characterView.SetPlayer(AlternateCharacter);
			Wanderer.CharacterView().SelectInitial();
		}

		public static void SetAlternateCharacter(Player playerCharacter)
		{
			AlternateCharacter = playerCharacter;
		}

		public static void RemoveCharacter(Player playerCharacter)
		{
			if (playerCharacter.CharacterView() != null) playerCharacter.CharacterView().SetPlayer(null);
			if (playerCharacter == Wanderer)
			{
				Wanderer = null;
			}
			else
			{
				AlternateCharacter = null;
			}
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
			foreach (XmlNode classNode in root.GetNodesWithName("Class"))
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

		private static void GenerateWanderer()
		{
			Wanderer = GenerateCharacter(CharacterClass.Wanderer);
			Templates.Remove(Wanderer.CharacterTemplate);
			Weapon weapon = WeaponGenerator.Generate(ItemQuality.Dark, WeaponType.Pistol);
			//todo generate wanderer weapon
		}

		public static Player GenerateCharacter(CharacterClass characterClass)
		{
			CharacterTemplate t = FindClass(characterClass);
			Templates.Remove(t);
			return GenerateCharacterObject(t);
		}

		public static Player GenerateRandomCharacter(CharacterClass characterClass = CharacterClass.None)
		{
			LoadTemplates();
			CharacterTemplate newTemplate = Templates.RemoveRandom();
			if (characterClass != CharacterClass.None)
				newTemplate = FindClass(characterClass);
			else
				Assert.IsFalse(Templates.Any(t => t.CharacterClass == CharacterClass.Wanderer));

			Player playerCharacter = GenerateCharacterObject(newTemplate);
			Weapon weapon          = WeaponGenerator.Generate();
			//todo generate character weapon
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

			attributes.SetMax(AttributeType.Life, playerCharacter.CharacterTemplate.Life);
			attributes.SetMax(AttributeType.Will, playerCharacter.CharacterTemplate.Will);

#if UNITY_EDITOR
//            attributes.SetMax(AttributeType.Life, Random.Range(6, 12));
//            attributes.SetMax(AttributeType.Will, Random.Range(6, 12));
#endif

			attributes.Get(AttributeType.Life).SetToMax();
			attributes.Get(AttributeType.Will).SetToMax();
		}

		private static void UpdateCharacter(Player character)
		{
			if (character == null) return;
			character.Update();
			if (character.IsDead)
			{
				RemoveCharacter(character);
			}
		}

		public static void Update()
		{
			UpdateCharacter(Wanderer);
			UpdateCharacter(AlternateCharacter);
		}

		public static Region CurrentRegion() => SelectedCharacter.TravelAction.GetCurrentRegion();
	}
}