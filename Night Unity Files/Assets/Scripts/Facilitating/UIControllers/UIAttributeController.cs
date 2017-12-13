using Facilitating.UI.Elements;
using Game.Characters;
using SamsHelper;
using UnityEngine;

public class UIAttributeController : MonoBehaviour
{
    private EnhancedText _strengthText, _intelligenceText, _enduranceText, _stabilityText;

    public void Awake()
    {
        _strengthText = CacheText("Strength");
        _intelligenceText = CacheText("Intelligence");
        _enduranceText = CacheText("Endurance");
        _stabilityText = CacheText("Stability");
    }

    private EnhancedText CacheText(string name)
    {
        return Helper.FindChildWithName(gameObject, name).transform.Find("Text").GetComponent<EnhancedText>();
    }

    public void HookValues(BaseAttributes attributes)
    {
        attributes.Endurance.AddOnValueChange(a => _enduranceText.Text(((int)a.CurrentValue()).ToString() + " end"));
        attributes.Strength.AddOnValueChange(a => _strengthText.Text(((int)a.CurrentValue()).ToString() + " str"));
        attributes.Stability.AddOnValueChange(a => _stabilityText.Text(((int)a.CurrentValue()).ToString() + " stb"));
        attributes.Intelligence.AddOnValueChange(a => _intelligenceText.Text(((int)a.CurrentValue()).ToString() + " int"));

    }
}
