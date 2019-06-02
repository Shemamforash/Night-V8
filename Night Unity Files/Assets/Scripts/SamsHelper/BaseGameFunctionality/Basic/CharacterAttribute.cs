using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Extensions;
using Game.Characters;
using NUnit.Framework;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.Basic
{
	public class CharacterAttribute : Number
	{
		private readonly List<AttributeModifier> _modifiers = new List<AttributeModifier>();
		private          float                   _calculatedValue;

		public override float CurrentValue
		{
			get => Mathf.Clamp(_calculatedValue, Min, Max);
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

		public override void Decrement(float amount = 1)
		{
			base.Decrement(amount);
			Recalculate();
		}

		public void Recalculate()
		{
			float rawBonus = _modifiers.Sum(m => m.RawBonus());
			_calculatedValue = base.CurrentValue + rawBonus;
			if (_modifiers.Count == 0) return;
			_modifiers.Sort((a, b) => a.Depth().CompareTo(b.Depth()));

			int   currentDepth = _modifiers[0].Depth();
			float finalBonus   = 1;
			_modifiers.ForEach(m =>
			{
				int depth = m.Depth();
				if (depth != currentDepth)
				{
					_calculatedValue *= finalBonus;
					finalBonus       =  1;
					currentDepth     =  depth;
				}

				finalBonus += m.FinalBonus();
			});
			if (finalBonus < 0) finalBonus = 0;
			_calculatedValue *= finalBonus;
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
	}
}