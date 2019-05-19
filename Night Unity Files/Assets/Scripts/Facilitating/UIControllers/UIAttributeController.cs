using Game.Characters;
using Extensions;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Facilitating.UIControllers
{
	public class UIAttributeController : MonoBehaviour
	{
		private UIAttributeMarkerController _lifeMarker, _willMarker;

		public void Awake()
		{
			_lifeMarker = CacheAttributeElement("Life");
			_willMarker = CacheAttributeElement("Will");
		}

		private UIAttributeMarkerController CacheAttributeElement(string elementName) => gameObject.FindChildWithName(elementName).transform.Find("Bar").GetComponent<UIAttributeMarkerController>();

		public void UpdateAttributesOffset(Player player, AttributeType attributeType, float offset)
		{
			UpdateMarker(_lifeMarker, player.Attributes.Life, attributeType == AttributeType.Life ? offset : 0);
			UpdateMarker(_willMarker, player.Attributes.Will, attributeType == AttributeType.Will ? offset : 0);
		}

		private void UpdateMarker(UIAttributeMarkerController marker, CharacterAttribute attribute, float offset = 0)
		{
			float current = attribute.CurrentValue;
			float max     = attribute.Max;
			marker.SetValue(max, current, offset);
		}

		public void UpdateAttributes(Player player)
		{
			if (player == null) return;
			UpdateMarker(_lifeMarker, player.Attributes.Life);
			UpdateMarker(_willMarker, player.Attributes.Will);
		}
	}
}