using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Extensions;
using Facilitating.Persistence;
using Game.Characters;
using NUnit.Framework;
using SamsHelper.ReactiveUI;

namespace SamsHelper.BaseGameFunctionality.Basic
{
	public class CharacterAttribute : Number
	{
		private readonly List<AttributeModifier> _modifiers = new List<AttributeModifier>();
		private          float                   _calculatedValue;

		public override float CurrentValue
		{
			get => _calculatedValue;
			set
			{
				base.CurrentValue = value;
				Recalculate();
			}
		}

		public override void Increment(float amount = 1)
		{
			base.Increment(amount);
			Recalculate();
		}

		public void Recalculate()
		{
			float rawBonus = _modifiers.Sum(m => m.RawBonus());
			_calculatedValue = base.CurrentValue + rawBonus;
		}

		public void AddModifier(AttributeModifier modifier)
		{
			Assert.IsFalse(_modifiers.Contains(modifier));
			_modifiers.Add(modifier);
			modifier.TargetAttributes.Add(this);
			Recalculate();
		}

		public void RemoveModifier(AttributeModifier modifier)
		{
			_modifiers.Remove(modifier);
			modifier.TargetAttributes.Remove(this);
			Recalculate();
		}

		public void SetToMax()
		{
			CurrentValue = Max;
		}

		public void Save(XmlNode doc)
		{
			doc.CreateChild("Value", base.CurrentValue);
			doc.CreateChild("Min",   Min);
			doc.CreateChild("Max",   Max);
		}

		public void Load(XmlNode attributeNode)
		{
			Min = attributeNode.ParseFloat("Min");
			string max               = attributeNode.ParseString("Max");
			if (max.Length > 10) max = "1000000";
			Max          = float.Parse(max);
			CurrentValue = attributeNode.ParseFloat("Value");
		}

		public static bool IsCharacterAttribute(AttributeType attribute) => attribute == AttributeType.Life || attribute == AttributeType.Will;
	}
}