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
            _strengthMarker = CacheText("Strength");
            _perceptionMarker = CacheText("Perception");
            _enduranceMarker = CacheText("Endurance");
            _willpowerMarker = CacheText("Willpower");
        }

        private UIAttributeMarkerController CacheText(string name)
        {
            return Helper.FindChildWithName(gameObject, name).transform.Find("Marker Container").GetComponent<UIAttributeMarkerController>();
        }

        public void HookValues(DesolationAttributes attributes)
        {
            attributes.Endurance.AddOnValueChange(a => _enduranceMarker.SetValue((int) a.CurrentValue(), (int) a.Max));
            attributes.Strength.AddOnValueChange(a => _strengthMarker.SetValue((int) a.CurrentValue(), (int) a.Max));
            attributes.Willpower.AddOnValueChange(a => _willpowerMarker.SetValue((int) a.CurrentValue(), (int) a.Max));
            attributes.Perception.AddOnValueChange(a => _perceptionMarker.SetValue((int) a.CurrentValue(), (int) a.Max));
        }
    }
}