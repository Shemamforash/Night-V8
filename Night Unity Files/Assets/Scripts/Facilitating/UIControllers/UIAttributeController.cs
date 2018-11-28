using Game.Characters;
using Game.Combat.Player;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Facilitating.UIControllers
{
    public class UIAttributeController : MonoBehaviour
    {
        private UIAttributeMarkerController _fettleMarker, _focusMarker, _gritMarker, _willMarker;

        public void Awake()
        {
            _fettleMarker = CacheAttributeElement("Fettle");
            _focusMarker = CacheAttributeElement("Focus");
            _gritMarker = CacheAttributeElement("Grit");
            _willMarker = CacheAttributeElement("Will");
        }

        private UIAttributeMarkerController CacheAttributeElement(string elementName)
        {
            return gameObject.FindChildWithName(elementName).transform.Find("Bar").GetComponent<UIAttributeMarkerController>();
        }

        public void UpdateAttributesOffset(Player player, AttributeType attributeType, float offset)
        {
            UpdateMarker(_fettleMarker, player, AttributeType.Fettle, attributeType == AttributeType.Fettle ? offset : 0);
            UpdateMarker(_gritMarker, player, AttributeType.Grit, attributeType == AttributeType.Grit ? offset : 0);
            UpdateMarker(_focusMarker, player, AttributeType.Focus, attributeType == AttributeType.Focus ? offset : 0);
            UpdateMarker(_willMarker, player, AttributeType.Will, attributeType == AttributeType.Will ? offset : 0);
        }

        private void UpdateMarker(UIAttributeMarkerController marker, Player player, AttributeType attributeType, float offset = 0)
        {
            float current = player.Attributes.Val(attributeType);
            float max = player.Attributes.Max(attributeType);
            marker.SetValue(max, current, offset);
        }

        public void UpdateAttributes(Player player)
        {
            UpdateMarker(_fettleMarker, player, AttributeType.Fettle);
            UpdateMarker(_gritMarker, player, AttributeType.Grit);
            UpdateMarker(_focusMarker, player, AttributeType.Focus);
            UpdateMarker(_willMarker, player, AttributeType.Will);
        }
    }
}