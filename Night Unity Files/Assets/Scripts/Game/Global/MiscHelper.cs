using Extensions;
using Game.Gear;
using SamsHelper.BaseGameFunctionality.Basic;

namespace Game.Global
{
	public static class MiscHelper
	{
		public static bool IsCoreAttribute(this AttributeType attributeType) => attributeType == AttributeType.Life || attributeType == AttributeType.Will;

		public static bool IsConditionAttribute(this AttributeType attributeType) =>
			attributeType == AttributeType.Shatter || attributeType == AttributeType.Burn || attributeType == AttributeType.Void;

		public static string GetModifierSummary(float modifierValue, ItemQuality quality, AttributeType attributeTarget, bool additive)
		{
			float  scaledValue   = modifierValue * ((int) quality + 1);
			string attributeName = attributeTarget.AttributeToDisplayString();
			if (attributeTarget.IsCoreAttribute() || attributeTarget.IsConditionAttribute()) return "+" + (int) scaledValue + " " + attributeName;
			string prefix = scaledValue > 0 ? "+" : "-";
			if (!additive) return prefix + (int) (scaledValue * 100) + "% " + attributeName;
			return prefix + (int) scaledValue + " " + attributeName;
		}

		public static string AttributeToDisplayString(this AttributeType attributeType)
		{
			return string.Join(" ", attributeType.ToString().SplitOnCamelCase());
		}
	}
}