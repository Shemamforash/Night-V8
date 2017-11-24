using Facilitating.UI.Elements;
using Game.Characters.Attributes;
using SamsHelper;
using UnityEngine;
using UnityEngine.UI;

public class UIConditionController : MonoBehaviour
{
    public Slider ConditionSlider;
    public EnhancedText ConditionText;

    public void HookStarvation(SurvivalAttributes survivalAttributes)
    {
        survivalAttributes.Starvation.AddOnValueChange(a =>
        {
            if(ConditionSlider != null)
                ConditionSlider.value = 1 - Helper.Normalise(a.CurrentValue(), a.Max);
            ConditionText.Text(survivalAttributes.GetHungerStatus());
        });
    }

    public void HookDehydration(SurvivalAttributes survivalAttributes)
    {
        survivalAttributes.Dehydration.AddOnValueChange(a =>
        {
            if(ConditionSlider != null)
                ConditionSlider.value = 1 - Helper.Normalise(a.CurrentValue(), a.Max);
            ConditionText.Text(survivalAttributes.GetThirstStatus());
        });
    }
    
}
