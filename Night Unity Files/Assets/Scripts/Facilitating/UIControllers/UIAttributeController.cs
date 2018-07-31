using Game.Characters;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Facilitating.UIControllers
{
    public class UIAttributeController : MonoBehaviour
    {
        private UIAttributeMarkerController _strengthMarker, _perceptionMarker, _enduranceMarker, _willpowerMarker;

        public void Awake()
        {
            _strengthMarker = CacheAttributeElement("Strength");
            _perceptionMarker = CacheAttributeElement("Perception");
            _enduranceMarker = CacheAttributeElement("Endurance");
            _willpowerMarker = CacheAttributeElement("Willpower");
        }

        private UIAttributeMarkerController CacheAttributeElement(string elementName)
        {
            return gameObject.FindChildWithName(elementName).transform.Find("Bar").GetComponent<UIAttributeMarkerController>();
        }

        public void UpdateAttributes(Player player)
        {
            _strengthMarker.SetValue(player.Attributes.Get(AttributeType.Strength));
            _enduranceMarker.SetValue(player.Attributes.Get(AttributeType.Endurance));
            _perceptionMarker.SetValue(player.Attributes.Get(AttributeType.Perception));
            _willpowerMarker.SetValue(player.Attributes.Get(AttributeType.Willpower));
        }
    }
}