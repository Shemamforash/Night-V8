using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Combat.Player;
using Game.Gear.Weapons;
using Game.Global;
using Extensions;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace Game.Gear.Armour
{
	public class Accessory : GearItem
	{
		private static readonly List<AccessoryTemplate> _accessoryTemplates = new List<AccessoryTemplate>();
		private static          bool                    _loaded;
		private readonly        AttributeModifier       _modifier;
		private readonly        string                  _summary;
		private readonly        AccessoryTemplate       _template;

		private Accessory(AccessoryTemplate template, ItemQuality itemQuality) : base(template.Name, itemQuality)
		{
			_template = template;
			_modifier = template.GetModifier((int) itemQuality + 1);
			_summary  = _modifier.Value.ToString(CultureInfo.InvariantCulture);
			string attributeString = _template.TargetAttribute.ToString();
			_summary += " " + attributeString;
		}

		public string Description() => _template.Description;

		public override string GetSummary() => _summary;

		public override void Equip(Character character)
		{
			base.Equip(character);
			(character as Player)?.ApplyModifier(_template.TargetAttribute, _modifier);
			if (PlayerCombat.Instance == null) return;
			PlayerCombat.Instance.RecalculateAttributes();
		}

		public override void UnEquip()
		{
			if (EquippedCharacter is Player player)
				player.RemoveModifier(_template.TargetAttribute, _modifier);
			base.UnEquip();
			if (PlayerCombat.Instance == null) return;
			PlayerCombat.Instance.RecalculateAttributes();
		}

		private static void ReadTemplates()
		{
			if (_loaded) return;
			XmlNode root = Helper.OpenRootNode("Gear", "GearList");
			foreach (XmlNode accessoryNode in root.GetNodesWithName("Gear"))
				new AccessoryTemplate(accessoryNode);
			_loaded = true;
		}

		public static Accessory Generate()
		{
			ItemQuality quality = WorldState.GenerateGearLevel();
			return Generate(quality);
		}

		public static Accessory Generate(ItemQuality quality)
		{
			ReadTemplates();
			AccessoryTemplate randomTemplate = _accessoryTemplates[Random.Range(0, _accessoryTemplates.Count)];
			return new Accessory(randomTemplate, quality);
		}

		public override XmlNode Save(XmlNode root)
		{
			root = root.CreateChild("Accessory");
			base.Save(root);
			return root;
		}

		public static Accessory LoadAccessory(XmlNode accessoryNode)
		{
			ReadTemplates();
			string            templateName = accessoryNode.ParseString("Name");
			AccessoryTemplate template     = _accessoryTemplates.First(t => t.Name == templateName);
			ItemQuality       qualityLevel = (ItemQuality) accessoryNode.ParseInt("Quality");
			Accessory         accessory    = new Accessory(template, qualityLevel);
			accessory.Load(accessoryNode);
			return accessory;
		}

		protected override void CalculateDismantleRewards()
		{
			base.CalculateDismantleRewards();
			int quality = (int) Quality() + 1;
			AddReward("Salt",    quality);
			AddReward("Essence", quality);
			List<string> possibleRewards = new List<string>();
			for (int i = 0; i < quality; ++i)
			{
				if (i == 0) possibleRewards.Add("Essence");
				if (i == 1) possibleRewards.Add("Rusty Scrap");
				if (i == 2) possibleRewards.Add("Metal Shards");
				if (i == 3) possibleRewards.Add("Ancient Relics");
				if (i == 4) possibleRewards.Add("Celestial Shards");
			}

			int count = Mathf.FloorToInt(quality / 2f) + 1;
			for (int i = 0; i < count; ++i) AddReward(possibleRewards.RemoveRandom(), 1);
		}

		private class AccessoryTemplate
		{
			private readonly float         _modifierValue;
			public readonly  string        Name, Description;
			public readonly  AttributeType TargetAttribute;

			public AccessoryTemplate(XmlNode accessoryNode)
			{
				Name            = accessoryNode.ParseString("Name");
				Description     = accessoryNode.ParseString("Description");
				TargetAttribute = Inventory.StringToAttributeType(accessoryNode.ParseString("Attribute"));
				_modifierValue  = accessoryNode.ParseFloat("Bonus");
				_accessoryTemplates.Add(this);
			}

			public AttributeModifier GetModifier(int qualityMultiplier)
			{
				AttributeModifier modifier = new AttributeModifier();
				modifier.Value = _modifierValue * qualityMultiplier;
				return modifier;
			}
		}
	}
}