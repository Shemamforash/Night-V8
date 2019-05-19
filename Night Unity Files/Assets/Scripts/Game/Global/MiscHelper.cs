using SamsHelper.BaseGameFunctionality.Basic;

namespace Game.Global
{
	public static class MiscHelper
	{
		public static bool IsCoreAttribute(this AttributeType attributeType) => attributeType == AttributeType.Life || attributeType == AttributeType.Will;
	}
}