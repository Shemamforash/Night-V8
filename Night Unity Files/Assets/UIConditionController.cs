using Facilitating.UI.Elements;
using Game.Characters.Attributes;
using SamsHelper;
using UnityEngine;
using UnityEngine.UI;

public class UIConditionController : MonoBehaviour
{
    private Slider _conditionSlider;
    private EnhancedText _conditionText;

    public void Awake()
    {
        _conditionSlider = Helper.FindChildWithName<Slider>(gameObject, "Progress");
        _conditionText = Helper.FindChildWithName<EnhancedText>(gameObject, "Text");
    }

    public void HookHunger(SurvivalAttributes survivalAttributes)
    {
        survivalAttributes.Hunger.AddOnValueChange(a =>
        {
            _conditionSlider.value = Helper.Normalise(a.CurrentValue(), a.Max);
            _conditionText.Text(survivalAttributes.GetHungerStatus());
        });
    }

    public void HookThirst(SurvivalAttributes survivalAttributes)
    {
        survivalAttributes.Thirst.AddOnValueChange(a =>
        {
            _conditionSlider.value = Helper.Normalise(a.CurrentValue(), a.Max);
            _conditionText.Text(survivalAttributes.GetThirstStatus());
        });
    }
    
}
