using Game.Characters;
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
            return Helper.FindChildWithName(gameObject, elementName).transform.Find("Bar").GetComponent<UIAttributeMarkerController>();
        }

        public void UpdateAttributes(Player player)
        {
            _strengthMarker.SetValue(player.Attributes.Strength);
            _enduranceMarker.SetValue(player.Attributes.Endurance);
            _perceptionMarker.SetValue(player.Attributes.Perception);
            _willpowerMarker.SetValue(player.Attributes.Willpower);
        }
    }
}