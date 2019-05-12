﻿using Game.Characters;
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
			UpdateMarker(_lifeMarker, player, AttributeType.Life, attributeType == AttributeType.Life ? offset : 0);
			UpdateMarker(_willMarker, player, AttributeType.Will, attributeType == AttributeType.Will ? offset : 0);
		}

		private void UpdateMarker(UIAttributeMarkerController marker, Player player, AttributeType attributeType, float offset = 0)
		{
			if (player == null || marker == null) return;
			float current = player.Attributes.Val(attributeType);
			float max     = player.Attributes.Max(attributeType);
			marker.SetValue(max, current, offset);
		}

		public void UpdateAttributes(Player player)
		{
			UpdateMarker(_lifeMarker, player, AttributeType.Life);
			UpdateMarker(_willMarker, player, AttributeType.Will);
		}
	}
}