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
		private readonly        string                  _summary;
		private readonly        AccessoryTemplate       _template;
		private readonly        float                   _modifierValue;

		private Accessory(AccessoryTemplate template, ItemQuality itemQuality) : base(template.Name, itemQuality)
		{
			_template      = template;
			_modifierValue = ((int) itemQuality + 1) * template.ModifierValue;
			_summary       = MiscHelper.GetModifierSummary(template.ModifierValue, itemQuality, _template.TargetAttribute, false);
		}

		public string Description() => _template.Description;

		public override string GetSummary() => _summary;

		public override void Equip(Character character)
		{
			base.Equip(character);
			if (PlayerCombat.Instance == null) return;
			PlayerCombat.Instance.RecalculateAttributes();
		}

		public override void UnEquip()
		{
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
			AccessoryTemplate template     = _accessoryTemplates.FirstOrDefault(t => t.Name == templateName);
			if (template == null) template = _accessoryTemplates.RandomElement();
			ItemQuality qualityLevel       = (ItemQuality) accessoryNode.ParseInt("Quality");
			Accessory   accessory          = new Accessory(template, qualityLevel);
			accessory.Load(accessoryNode);
			return accessory;
		}

		protected override void CalculateDismantleRewards()
		{
			base.CalculateDismantleRewards();
			int quality = (int) Quality() + 1;
			AddReward(quality             + " Essence", () => Inventory.IncrementResource("Essence", quality));
			List<string> possibleRewards = new List<string>();
			for (int i = 0; i < quality; ++i)
			{
				if (i == 1) possibleRewards.Add("Rusty Scrap");
				if (i == 2) possibleRewards.Add("Metal Shards");
				if (i == 3) possibleRewards.Add("Ancient Relics");
				if (i == 4) possibleRewards.Add("Celestial Shards");
			}

			if (possibleRewards.Count == 0) return;
			string randomReward = possibleRewards.RemoveRandom();
			AddReward("1 " + randomReward, () => Inventory.IncrementResource(randomReward, 1));
		}

		public AttributeType TargetAttribute => _template.TargetAttribute;
		public float         ModifierValue   => _modifierValue;
		public bool          Additive        => _template.Additive;

		private class AccessoryTemplate
		{
			public readonly string        Name, Description;
			public readonly AttributeType TargetAttribute;
			public readonly float         ModifierValue;
			public readonly bool          Additive;

			public AccessoryTemplate(XmlNode accessoryNode)
			{
				Name            = accessoryNode.ParseString("Name");
				Description     = accessoryNode.ParseString("Description");
				TargetAttribute = Inventory.StringToAttributeType(accessoryNode.ParseString("Attribute"));
				ModifierValue   = accessoryNode.ParseFloat("Bonus");
				_accessoryTemplates.Add(this);
				Additive = TargetAttribute.IsConditionAttribute();
			}

			public AttributeModifier GetModifier(int qualityMultiplier)
			{
				AttributeModifier modifier = new AttributeModifier();
				if (Additive) modifier.SetRawBonus(ModifierValue * qualityMultiplier);
				else modifier.SetFinalBonus(ModifierValue        * qualityMultiplier);
				return modifier;
			}
		}
	}
}