using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Global;
using Extensions;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;

namespace Game.Gear
{
	public class Inscription : GearItem
	{
		private static readonly List<InscriptionTemplate> _inscriptionTemplates = new List<InscriptionTemplate>();
		private static          bool                      _readTemplates;
		private readonly        int                       _inscriptionCost;
		private readonly        AttributeModifier         _modifier;
		private readonly        InscriptionTemplate       _template;

		private Inscription(InscriptionTemplate template, ItemQuality quality) : base("A " + QualityToInscription(quality) + " of " + template.Name, quality)
		{
			_template = template;
			_modifier = _template.GetModifier();
			int qualityModifier = (int) (quality + 1);
			_modifier.Value  *= qualityModifier;
			_inscriptionCost =  (int) quality + 1;
		}

		public AttributeModifier Modifier() => _modifier;

		public AttributeType Target() => _template.AttributeTarget;

		public string TemplateName() => _template.Name;

		public static Inscription Generate()
		{
			ItemQuality tier = WorldState.GenerateGearLevel();
			return Generate(tier);
		}

		public static Inscription Generate(ItemQuality tier)
		{
			ReadTemplates();
			InscriptionTemplate randomTemplate = _inscriptionTemplates.RandomElement();
			return new Inscription(randomTemplate, tier);
		}

		private static void ReadTemplates()
		{
			if (_readTemplates) return;
			XmlNode inscriptions = Helper.OpenRootNode("Inscriptions");
			foreach (XmlNode inscriptionNode in inscriptions.SelectNodes("Inscription"))
				new InscriptionTemplate(inscriptionNode);

			_readTemplates = true;
		}

		private static string QualityToInscription(ItemQuality quality)
		{
			switch (quality)
			{
				case ItemQuality.Dark:
					return "Murmur";
				case ItemQuality.Dull:
					return "Whisper";
				case ItemQuality.Glowing:
					return "Cry";
				case ItemQuality.Shining:
					return "Wail";
				case ItemQuality.Radiant:
					return "Bellow";
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public override XmlNode Save(XmlNode root)
		{
			root = root.CreateChild("Inscription");
			base.Save(root);
			root.CreateChild("InscriptionTemplate", _template.Name);
			return root;
		}

		public static Inscription LoadInscription(XmlNode root)
		{
			ReadTemplates();
			string              templateString = root.ParseString("InscriptionTemplate");
			InscriptionTemplate template       = _inscriptionTemplates.First(t => t.Name == templateString);
			ItemQuality         quality        = (ItemQuality) root.ParseInt("Quality");
			Inscription         inscription    = new Inscription(template, quality);
			inscription.Load(root);
			return inscription;
		}

		public override string GetSummary() => _template.GetSummary(Quality());

		public int InscriptionCost() => _inscriptionCost;

		public bool CanAfford() => Inventory.GetResourceQuantity("Essence") >= _inscriptionCost;

		protected override void CalculateDismantleRewards()
		{
			base.CalculateDismantleRewards();
			int quality = (int) Quality() + 1;
			AddReward("Essence", 5 * quality);
		}

		private class InscriptionTemplate
		{
			private readonly float         _modifierValue;
			public readonly  AttributeType AttributeTarget;
			public readonly  string        Name;

			public InscriptionTemplate(XmlNode inscriptionNode)
			{
				Name            = inscriptionNode.ParseString("Name");
				AttributeTarget = Inventory.StringToAttributeType(inscriptionNode.ParseString("Attribute"));
				_modifierValue  = inscriptionNode.ParseFloat("Value");
				_inscriptionTemplates.Add(this);
			}

			public AttributeModifier GetModifier()
			{
				AttributeModifier modifier = new AttributeModifier();
				modifier.Value = _modifierValue;
				return modifier;
			}

			public string GetSummary(ItemQuality quality)
			{
				float  scaledValue          = _modifierValue * ((int) quality + 1);
				string attributeName        = AttributeTarget.ToString();
				string prefix               = "";
				if (scaledValue > 0) prefix = "+";
				return prefix + (int) scaledValue + " " + attributeName;
			}
		}
	}
}