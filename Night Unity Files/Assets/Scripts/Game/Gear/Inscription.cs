using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Extensions;
using Game.Characters;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Gear
{
	public class Inscription : GearItem
	{
		private static readonly List<InscriptionTemplate> _inscriptionTemplates = new List<InscriptionTemplate>();
		private static          bool                      _readTemplates;
		private readonly        AttributeModifier         _modifier;
		private readonly        InscriptionTemplate       _template;
		private readonly        int                       _inscriptionCost;

		private Inscription(InscriptionTemplate template, ItemQuality quality) : base("A " + QualityToInscription(quality) + " of " + template.Name, quality)
		{
			_template = template;
			_modifier = _template.GetModifier();
			float finalBonus   = _modifier.FinalBonus();
			float rawBonus     = _modifier.RawBonus();
			int   tierModifier = (int) (quality + 1);
			finalBonus *= tierModifier;
			rawBonus   *= tierModifier;
			_modifier.SetFinalBonus(finalBonus);
			_modifier.SetRawBonus(rawBonus);
			_inscriptionCost = (int) quality + 1;
		}

		public AttributeModifier Modifier()
		{
			return _modifier;
		}

		public AttributeType Target()
		{
			return _template.AttributeTarget;
		}

		public static Inscription Generate(bool includeCore)
		{
			ItemQuality tier = WorldState.GenerateGearLevel();
			return Generate(tier, includeCore);
		}

		public static Inscription Generate(ItemQuality tier, bool includeCore)
		{
			ReadTemplates();
			List<InscriptionTemplate> valid    = _inscriptionTemplates;
			if (!includeCore) valid            = valid.Where(i => !i.AttributeTarget.IsCoreAttribute()).ToList();
			InscriptionTemplate randomTemplate = valid.RandomElement();
			return new Inscription(randomTemplate, tier);
		}

		private static void ReadTemplates()
		{
			if (_readTemplates) return;
			XmlNode inscriptions = Helper.OpenRootNode("Inscriptions");
			foreach (XmlNode inscriptionNode in inscriptions.GetNodesWithName("Inscription"))
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
			InscriptionTemplate template       = _inscriptionTemplates.FirstOrDefault(t => t.Name == templateString);
			if (template == null)
			{
				Debug.Log("Unknown inscription template: " + templateString);
				return null;
			}
			ItemQuality         quality        = (ItemQuality) root.ParseInt("Quality");
			Inscription         inscription    = new Inscription(template, quality);
			inscription.Load(root);
			return inscription;
		}

		private class InscriptionTemplate
		{
			public readonly  string        Name;
			public readonly  AttributeType AttributeTarget;
			private readonly bool          _additive;
			private readonly float         _modifierValue;

			public InscriptionTemplate(XmlNode inscriptionNode)
			{
				Name            = inscriptionNode.ParseString("Name");
				AttributeTarget = Inventory.StringToAttributeType(inscriptionNode.ParseString("Attribute"));
				_modifierValue  = inscriptionNode.ParseFloat("Value");
				_additive       = AttributeTarget.IsCoreAttribute() || AttributeTarget.IsConditionAttribute();
				_inscriptionTemplates.Add(this);
			}

			public AttributeModifier GetModifier()
			{
				AttributeModifier modifier = new AttributeModifier();
				if (_additive) modifier.SetRawBonus(_modifierValue);
				else modifier.SetFinalBonus(_modifierValue);
				return modifier;
			}

			public string GetSummary(ItemQuality quality) => MiscHelper.GetModifierSummary(_modifierValue, quality, AttributeTarget, _additive);
		}

		public override string GetSummary()
		{
			return _template.GetSummary(Quality());
		}

		public int InscriptionCost()
		{
			return _inscriptionCost;
		}

		public bool CanAfford()
		{
			return Inventory.GetResourceQuantity("Essence") >= _inscriptionCost;
		}

		protected override void CalculateDismantleRewards()
		{
			base.CalculateDismantleRewards();
			int quality = (int) Quality();
			if (quality == 0)
			{
				int essenceReward = Random.Range(3, 5);
				AddReward(essenceReward + " Essence", () => Inventory.IncrementResource("Essence", essenceReward));
				return;
			}

			ItemQuality lowerQuality = (ItemQuality) (quality - 1);
			Inscription inscription  = new Inscription(_template, lowerQuality);
			AddReward(inscription.Name, () => Inventory.Move(inscription));
		}
	}
}