using Game.Characters;
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

        public void UpdateAttributes(Player player)
        {
            _fettleMarker.SetValue(player.Attributes.Get(AttributeType.Fettle));
            _gritMarker.SetValue(player.Attributes.Get(AttributeType.Grit));
            _focusMarker.SetValue(player.Attributes.Get(AttributeType.Focus));
            _willMarker.SetValue(player.Attributes.Get(AttributeType.Will));
        }
    }
}