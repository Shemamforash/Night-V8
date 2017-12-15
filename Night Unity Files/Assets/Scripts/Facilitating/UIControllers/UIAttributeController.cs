using Facilitating.UI.Elements;
using Game.Characters;
using SamsHelper;
using UnityEngine;

public class UIAttributeController : MonoBehaviour
{
    private UIAttributeMarkerController _strengthMarker, _intelligenceMarker, _enduranceMarker, _stabilityMarker;

    public void Awake()
    {
        _strengthMarker = CacheText("Strength");
        _intelligenceMarker = CacheText("Intelligence");
        _enduranceMarker = CacheText("Endurance");
        _stabilityMarker = CacheText("Stability");
    }

    private UIAttributeMarkerController CacheText(string name)
    {
        return Helper.FindChildWithName(gameObject, name).transform.Find("Marker Container").GetComponent<UIAttributeMarkerController>();
    }

    public void HookValues(BaseAttributes attributes)
    {
        attributes.Endurance.AddOnValueChange(a => _enduranceMarker.SetValue((int)a.CurrentValue(), (int)a.Max));
        attributes.Strength.AddOnValueChange(a => _strengthMarker.SetValue((int)a.CurrentValue(), (int)a.Max));
        attributes.Stability.AddOnValueChange(a => _stabilityMarker.SetValue((int)a.CurrentValue(), (int)a.Max));
        attributes.Intelligence.AddOnValueChange(a => _intelligenceMarker.SetValue((int)a.CurrentValue(), (int)a.Max));
    }
}
