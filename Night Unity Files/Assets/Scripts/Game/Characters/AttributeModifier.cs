using System.Collections.Generic;
using Extensions;
using SamsHelper.BaseGameFunctionality.Basic;

namespace Game.Characters
{
	public class AttributeModifier
	{
		public readonly List<CharacterAttribute> TargetAttributes = new List<CharacterAttribute>();
		private         float                    _rawBonus;

		public void SetRawBonus(float rawBonus)
		{
			_rawBonus = rawBonus;
			UpdateTargetAttributes();
		}

		private void UpdateTargetAttributes()
		{
			TargetAttributes.ForEach(t => t.Recalculate());
		}

		private string ModifierToString(float modifier)
		{
			string modifierString             = "";
			if (modifier != 0) modifierString = modifier.AddSignPrefix();
			return modifierString;
		}

		public string RawBonusToString()
		{
			if (_rawBonus == 0) return "";
			string rawBonusString = ModifierToString(_rawBonus * 100) + "%";
			return rawBonusString;
		}

		public float RawBonus() => _rawBonus;
	}
}