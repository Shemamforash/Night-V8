using System.Collections.Generic;
using Extensions;
using SamsHelper.BaseGameFunctionality.Basic;

namespace Game.Characters
{
	public class AttributeModifier
	{
		public readonly List<CharacterAttribute> TargetAttributes = new List<CharacterAttribute>();
		private         float                    _value;

		public float Value
		{
			set
			{
				_value = value;
				UpdateTargetAttributes();
			}
			get => _value;
		}

		private void UpdateTargetAttributes() => TargetAttributes.ForEach(t => t.Recalculate());

		private string ModifierToString(float modifier)
		{
			string modifierString             = "";
			if (modifier != 0) modifierString = modifier.AddSignPrefix();
			return modifierString;
		}
	}
}