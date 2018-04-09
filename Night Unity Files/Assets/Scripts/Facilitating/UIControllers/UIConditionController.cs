using Game.Characters;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UIControllers
{
    public class UIConditionController : MonoBehaviour
    {
        public Slider ConditionSlider;
        public EnhancedText ConditionText;

        public void HookStarvation(DesolationAttributes attributes)
        {
            attributes.Starvation.AddOnValueChange(a =>
            {
                if (ConditionSlider != null)
                    ConditionSlider.value = 1 - Helper.Normalise(a.CurrentValue(), a.Max);
                ConditionText.Text(attributes.GetHungerStatus());
            });
        }

        public void HookDehydration(DesolationAttributes attributes)
        {
            attributes.Dehydration.AddOnValueChange(a =>
            {
                if (ConditionSlider != null)
                    ConditionSlider.value = 1 - Helper.Normalise(a.CurrentValue(), a.Max);
                ConditionText.Text(attributes.GetThirstStatus());
            });
        }
    }
}